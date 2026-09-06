using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace PS5PKGTool.Core.Services;

public enum SonyPackageCheckState { Pass, Warning, Fail }
public sealed record SonyPackageAcceptanceCheck(string Name, SonyPackageCheckState State, string Message);
public sealed class SonyPackageAcceptanceReport
{
    public required IReadOnlyList<SonyPackageAcceptanceCheck> Checks { get; init; }
    public bool IsStructurallyReady => Checks.All(check => check.State != SonyPackageCheckState.Fail);
}

public static class SonyPackageAcceptanceValidator
{
    public static SonyPackageAcceptanceReport Validate(string packagePath, string passcode = SonyDebugPackageCredentials.DefaultPasscode)
    {
        var checks = new List<SonyPackageAcceptanceCheck>();
        try
        {
            var reader = new SonyPkgReader();
            SonyPkgSummary package = reader.Read(packagePath, passcode);
            checks.Add(new("FIH", package.Kind == SonyPkgKind.FinalizedDebug ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail,
                package.Kind == SonyPkgKind.FinalizedDebug ? "Finalized debug FIH detected." : "A finalized debug FIH is required."));
            checks.Add(new("Format version", package.FormatVersion == 3 ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail,
                "FIH format version: " + (package.FormatVersion?.ToString() ?? "missing")));
            checks.Add(new("Content ID", package.ContentId.Length == 36 ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail,
                package.ContentId.Length == 36 ? package.ContentId : "Content ID is missing or malformed."));
            checks.Add(new("Image key entry", package.Entries.Any(entry => entry.Id == 0x20) ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail,
                package.Entries.Any(entry => entry.Id == 0x20) ? "Image key entry is present." : "CNT entry 0x20 is missing."));
            checks.Add(new("Outer PFS", package.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail,
                package.NestedPfs?.StatusMessage ?? "PFS could not be indexed."));
            bool param = package.Entries.Any(entry => entry.Id == 0x2000);
            checks.Add(new("param.json", param ? SonyPackageCheckState.Pass : SonyPackageCheckState.Fail, param ? "CNT param.json entry is present." : "CNT param.json entry is missing."));
            checks.Add(CheckFihStructure(packagePath, package));
            checks.Add(CheckFihDigests(packagePath, package));
            checks.Add(CheckOuterMetadataIntegrity(packagePath, package, passcode));
            checks.Add(CheckCntIntegrity(packagePath, package, reader));
            checks.Add(CheckOuterContent(packagePath, package, passcode));
            checks.Add(CheckNestedImageDigest(packagePath, package, passcode));
            checks.Add(CheckSystemFiles(packagePath, package, reader));
            checks.Add(new("Console acceptance", SonyPackageCheckState.Warning, "Structural validation cannot prove acceptance by a specific console, firmware, or debug environment."));
        }
        catch (Exception ex) { checks.Add(new("Package parse", SonyPackageCheckState.Fail, ex.Message)); }
        return new SonyPackageAcceptanceReport { Checks = checks };
    }

    private static SonyPackageAcceptanceCheck CheckFihStructure(string path, SonyPkgSummary package)
    {
        try
        {
            long fileSize = new FileInfo(path).Length;
            if (package.PfsImageOffset > (ulong)fileSize || package.PfsImageSize > (ulong)fileSize - package.PfsImageOffset)
                return Fail("FIH ranges", "The FIH PFS image range is outside the package.");
            if (package.PfsSuperblockOffset < package.PfsImageOffset || package.PfsSuperblockOffset > package.PfsImageOffset + package.PfsImageSize - 0x10000)
                return Fail("FIH ranges", "The FIH PFS superblock is outside the PFS image.");
            if (package.EmbeddedCntOffset > (ulong)fileSize || package.ContainerOffset != checked((long)package.EmbeddedCntOffset))
                return Fail("FIH ranges", "The embedded CNT offset is inconsistent or outside the package.");
            return Pass("FIH ranges", "PFS image, superblock, and CNT ranges are within the package.");
        }
        catch (OverflowException) { return Fail("FIH ranges", "The FIH contains an overflowing range."); }
    }

    private static SonyPackageAcceptanceCheck CheckFihDigests(string path, SonyPkgSummary package)
    {
        try
        {
            if (package.Kind != SonyPkgKind.FinalizedDebug) return Warn("FIH digests", "Digest validation is only available for a readable debug FIH.");
            byte[] fih = ReadExact(path, 0, 0x10000);
            byte[] superblock = ReadExact(path, checked((long)package.PfsSuperblockOffset), 0x10000);
            byte[] digest = SHA3_256.HashData(superblock);
            bool allMatch = fih.AsSpan(0x30, 32).SequenceEqual(digest) &&
                            fih.AsSpan(0x70, 32).SequenceEqual(digest) &&
                            fih.AsSpan(0xD0, 32).SequenceEqual(digest);
            return allMatch
                ? Pass("FIH digests", "All three FIH superblock digests match the stored PFS superblock.")
                : Fail("FIH digests", "One or more FIH superblock digests do not match.");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException)
        {
            return Fail("FIH digests", ex.Message);
        }
    }

    private static SonyPackageAcceptanceCheck CheckCntIntegrity(string path, SonyPkgSummary package, SonyPkgReader reader)
    {
        try
        {
            byte[] header = ReadExact(path, package.ContainerOffset, 0x1000);
            if (!header.AsSpan(0xFE0, 32).SequenceEqual(SHA3_256.HashData(header.AsSpan(0, 0xFE0))))
                return Fail("CNT integrity", "CNT header digest does not match.");
            SonyPkgEntry? digestEntry = package.Entries.FirstOrDefault(entry => entry.Id == 0x0001);
            if (digestEntry is null || digestEntry.IsEncrypted || digestEntry.DataSize != package.Entries.Count * 32)
                return Fail("CNT integrity", "CNT digest table entry is missing or malformed.");
            byte[] table = reader.ReadEntryBytes(path, package, digestEntry, 64 * 1024 * 1024);
            for (int index = 0; index < package.Entries.Count; index++)
            {
                SonyPkgEntry entry = package.Entries[index];
                ReadOnlySpan<byte> expected = table.AsSpan(index * 32, 32);
                if (entry.Id == 0x0001)
                {
                    if (expected.IndexOfAnyExcept((byte)0) >= 0) return Fail("CNT integrity", "CNT digest-table self entry is not zeroed.");
                    continue;
                }
                if (entry.IsEncrypted) return Warn("CNT integrity", "CNT digest table is present. Encrypted entries cannot be verified without their content key.");
                if (entry.DataSize > 64 * 1024 * 1024) return Warn("CNT integrity", "CNT digest table is present. A large entry was not loaded for in-memory verification.");
                byte[] data = reader.ReadEntryBytes(path, package, entry, 64 * 1024 * 1024);
                if (!expected.SequenceEqual(SHA3_256.HashData(data)))
                    return Fail("CNT integrity", "Digest mismatch for " + entry.DisplayName + ".");
            }
            return Pass("CNT integrity", "CNT header and entry digests match.");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException)
        {
            return Fail("CNT integrity", ex.Message);
        }
    }

    private static SonyPackageAcceptanceCheck CheckOuterMetadataIntegrity(string path, SonyPkgSummary package, string passcode)
    {
        try
        {
            if (package.Kind != SonyPkgKind.FinalizedDebug) return Warn("Outer metadata integrity", "Outer metadata integrity needs a readable debug image key.");
            byte[] superblock = ReadExact(path, checked((long)package.PfsSuperblockOffset), 0x10000);
            if (superblock.Length < 0x5A0) return Fail("Outer metadata integrity", "The outer PFS superblock is truncated.");
            byte[] expectedIcv = superblock.AsSpan(0x380, 32).ToArray();
            superblock.AsSpan(0x380, 32).Clear();
            if (!expectedIcv.SequenceEqual(SHA3_256.HashData(superblock.AsSpan(0, 0x5A0))))
                return Fail("Outer metadata integrity", "The outer PFS superblock ICV does not match.");
            int inodeTableBlock = BinaryPrimitives.ReadInt32LittleEndian(superblock.AsSpan(0xD8, 4));
            long imageBlocks = checked((long)package.PfsImageSize / 0x10000);
            if (inodeTableBlock < 0 || inodeTableBlock >= imageBlocks) return Fail("Outer metadata integrity", "The outer PFS inode-table block index is invalid.");
            byte[] encrypted = ReadExact(path, checked((long)package.PfsImageOffset + (long)inodeTableBlock * 0x10000), 0x10000);
            byte[] imageKey = SonyPfsCrypto.DeriveEkpfs(package.ContentId, passcode);
            SonyPfsCryptoContext context = SonyPfsCrypto.DeriveContext(imageKey, superblock.AsSpan(0x370, 16), 0x10000, true);
            SonyPfsCrypto.DecryptBlock(encrypted, context, checked((ulong)inodeTableBlock), true);
            return superblock.AsSpan(0xB8, 32).SequenceEqual(SHA3_256.HashData(encrypted))
                ? Pass("Outer metadata integrity", "The outer PFS superblock ICV and inode-table digest match.")
                : Fail("Outer metadata integrity", "The outer PFS inode-table digest does not match.");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException or CryptographicException)
        {
            return Fail("Outer metadata integrity", ex.Message);
        }
    }

    private static SonyPackageAcceptanceCheck CheckOuterContent(string path, SonyPkgSummary package, string passcode)
    {
        try
        {
            SonyPfsSummary? outer = package.Kind == SonyPkgKind.FinalizedDebug
                ? new SonyDebugPfsReader().TryIndex(path, checked((long)package.PfsImageOffset),
                    checked((long)package.PfsImageSize), checked((long)package.PfsSuperblockOffset),
                    package.ContentId, passcode)
                : null;
            IReadOnlyList<SonyPfsEntry> files = outer?.Files ?? [];
            bool image = files.Any(file => file.RelativePath.Equals("pfs_image.dat", StringComparison.OrdinalIgnoreCase));
            bool naps = files.Any(file => file.RelativePath.Equals("naps_pkg_layout.dat", StringComparison.OrdinalIgnoreCase));
            return image && naps
                ? Pass("Outer content", "pfs_image.dat and naps_pkg_layout.dat are indexed in the outer PFS.")
                : Fail("Outer content", "The outer PFS must contain pfs_image.dat and naps_pkg_layout.dat.");
        }
        catch (Exception ex) when (ex is InvalidDataException or OverflowException)
        {
            return Fail("Outer content", ex.Message);
        }
    }

    private static SonyPackageAcceptanceCheck CheckSystemFiles(string path, SonyPkgSummary package, SonyPkgReader reader)
    {
        try
        {
            var failures = new List<string>();
            foreach (uint id in new uint[] { 0x0400, 0x0401, 0x0402, 0x0403 })
            {
                SonyPkgEntry? entry = package.Entries.FirstOrDefault(value => value.Id == id);
                if (entry is null) continue;
                if (entry.IsEncrypted || entry.DataSize > 2 * 1024 * 1024)
                {
                    failures.Add(entry.DisplayName + " could not be inspected");
                    continue;
                }
                Ps5SystemFileValidation result = Ps5SystemFileValidator.Validate(entry.DisplayName,
                    reader.ReadEntryBytes(path, package, entry, 2 * 1024 * 1024));
                if (!result.IsValid) failures.Add(entry.DisplayName + ": " + result.Message);
            }
            return failures.Count == 0
                ? Pass("System files", "Included signed system files passed structural validation.")
                : Warn("System files", string.Join("; ", failures));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return Warn("System files", ex.Message);
        }
    }

    private static SonyPackageAcceptanceCheck CheckNestedImageDigest(string path, SonyPkgSummary package, string passcode)
    {
        try
        {
            if (package.Kind != SonyPkgKind.FinalizedDebug)
                return Warn("Nested image digest", "The nested-image digest requires a readable debug package key.");
            SonyPfsSummary? outer = new SonyDebugPfsReader().TryIndex(path, checked((long)package.PfsImageOffset),
                checked((long)package.PfsImageSize), checked((long)package.PfsSuperblockOffset), package.ContentId, passcode);
            SonyPfsEntry? image = outer?.Files.FirstOrDefault(file => file.RelativePath.Equals("pfs_image.dat", StringComparison.OrdinalIgnoreCase));
            if (outer is null || image is null)
                return Fail("Nested image digest", "pfs_image.dat could not be located in the outer PFS.");
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA3_256);
            long offset = 0;
            while (offset < image.Size)
            {
                int size = checked((int)Math.Min(1024 * 1024, image.Size - offset));
                hash.AppendData(SonyPfsEntryDataReader.ReadRange(path, package, outer, image, offset, size));
                offset += size;
            }
            byte[] fih = ReadExact(path, 0, 0x10000);
            return fih.AsSpan(0xB0, 32).SequenceEqual(hash.GetHashAndReset())
                ? Pass("Nested image digest", "The FIH nested-image digest matches pfs_image.dat.")
                : Fail("Nested image digest", "The FIH nested-image digest does not match pfs_image.dat.");
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException or CryptographicException)
        {
            return Fail("Nested image digest", ex.Message);
        }
    }

    private static byte[] ReadExact(string path, long offset, int length)
    {
        byte[] result = new byte[length];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete,
            64 * 1024, FileOptions.RandomAccess);
        if (offset < 0 || offset > stream.Length || length > stream.Length - offset)
            throw new InvalidDataException("The requested validation range is outside the package.");
        stream.Position = offset;
        stream.ReadExactly(result);
        return result;
    }

    private static SonyPackageAcceptanceCheck Pass(string name, string message) => new(name, SonyPackageCheckState.Pass, message);
    private static SonyPackageAcceptanceCheck Warn(string name, string message) => new(name, SonyPackageCheckState.Warning, message);
    private static SonyPackageAcceptanceCheck Fail(string name, string message) => new(name, SonyPackageCheckState.Fail, message);
}
