# PlayGo reader addendum: extents, header flags, and file-to-chunk assignments

Status: handoff spec for extending `ProsperoPkgTool.Gp5.PlayGoChunkReader` / `PlayGoProject`.
Author: PS5PKGTool contributor (AI-assisted). Delivery is the spec plus synthetic fixture hex only;
no generator source is included. This document contains only format facts plus synthetic fixture
data; no retail game bytes.

## 1. Scope

Extend the existing reader (do **not** add a parallel reader):

- Chunk ownership extents from PLGX sections `0xC0`/`0xC8`/`0xD8`.
- `PlayGoProject.HeaderFlags`.
- Per-file chunk assignments from `playgo-hash-table.dat` + `playgo-ficm.dat`, resolved with the
  public `ProsperoOuterPfsBuilder.FltPathHash`, filling the existing
  `PlayGoProject.FileChunkAssignments`.

Everything already parsed by `PlayGoChunkReader` (header, chunk records, scenario records,
validation) stays as is.

## 2. PLGX chunk record additions

Chunk record stride `0x20`. Fields already read: `+0x10` language mask, `+0x1C` label offset.
Fields to add:

| Offset | Type | Meaning |
| --- | --- | --- |
| `+0x04` | u32 | Extent id count |
| `+0x18` | u32 | Offset of the extent id list within section `0xC8` |

Also observed, worth validating as opaque constants: `+0x00` flags `0x00030080`, `+0x08` use `0x11`.

## 3. Extent sections

Section pointers live in the header directory at `0xC0` through `0xF8`, each `(u32 offset, u32 size)`.

| Pointer | Contents |
| --- | --- |
| `0xC0` | Chunk records, `chunkCount * 0x20` |
| `0xC8` | Flat array of extent ids (u32 each) |
| `0xD0` | Chunk label blob |
| `0xD8` | Extents, `count * 0x10`: `{ ulong start; ulong length; }` |
| `0xE0` | Scenario records, `scenarioCount * 0x20` |
| `0xE8` | Scenario chunk sequences (u16 each) |
| `0xF0` | Scenario label blob |
| `0xF8` | Present with size `0` in our builder output; tolerate |

A chunk owns the extents whose ids appear at
`0xC8 + extentListOffset`, for `extentIdCount` consecutive u32 entries. Each id indexes section
`0xD8` at `id * 0x10`. Suggested additions:

```csharp
public sealed record PlayGoExtent(int ChunkId, ulong Start, ulong Length);
```

exposed as `PlayGoChunk.Extents` (or `PlayGoProject.Extents`); a chunk's byte total is the sum of
its extents. Apply the same bounds and NUL/ASCII validation already used for labels and sequences.

## 4. Header fields to tolerate/validate

Already read: version major `0x04`, version minor `0x06`, image count `0x08`, chunk count `0x0A`,
scenario count `0x0E`, declared file size `0x10`, default scenario `0x14`, content id `0x40`.

Add / validate: `0x16` (observed `1`), header flags `0x1C`, `0x20` (observed `4`), `0x24`
(observed `1`), `0x30` (observed `0x11`), `0x38` (observed `ulong.MaxValue`), and the `0xF8`
section pointer.

**Header flags `0x1C` are opaque.** The debug/nwonly builder writes `0x85`; a homebrew sample we
have shows `0x850000`, produced by a different tool. Validate known bits/ranges, not exact
equality.

## 5. File-to-chunk assignments

Two plaintext CNT entries:

`playgo-hash-table.dat`

| Offset | Type | Meaning |
| --- | --- | --- |
| `0x0C` | u32 | Byte length of the hash array = `count * 8` |
| `0x24` | u32 | Entry count |
| `0x38` | u64[] | FLT path hashes |

`playgo-ficm.dat`

| Offset | Type | Meaning |
| --- | --- | --- |
| `0x0C` | u32 | Byte length of the id array = `count * 2` |
| `0x10` | u16[] | Chunk id per entry, index-aligned with the hash table |

Confirmed rule for FICM count:

- If `0x0C` is present and plausible, `count = u32@0x0C / 2`, and cross-check it equals the
  hash-table count `u32@0x24`.
- If `0x0C` is absent or short, fall back to the hash-table count and require
  `ficm.Length >= 0x10 + count * 2`.
- On any inconsistency, fail loudly (see extent policy in section 9).

For reference, retail SIFU FICM is 320 bytes with 152 assignments: `0x10 + 152 * 2 = 320`, so
`0x0C = 304` there.

Resolution: `FltPathHash` over the **plain inner-image relative path, no leading slash**, for
example `prx/aktimestretch.prx`. Both the plain and a leading-slash variant were tested; only the
plain form matched. Suggested signature over the existing types:

```csharp
public static IReadOnlyDictionary<string, byte> ReadAssignments(
    ReadOnlySpan<byte> hashTable,
    ReadOnlySpan<byte> ficm,
    IEnumerable<string> candidatePaths);
```

## 6. Validation results (documented expectations, no retail bytes attached)

Retail SIFU (`PPSA05132`): `playgo-chunk.dat` 383 bytes, header flags `0x85`,
`playgo-hash-table.dat` 1272 bytes, `playgo-ficm.dat` 320 bytes, 152 assignments, 152/152 hashes
resolved to real inner paths, 1 chunk / 1 scenario.

Homebrew test package: flags `0x850000`, 3 assignments, 1 resolved.
Synthetic debug package: 4/4 resolved.

## 7. Synthetic fixtures (hex only)

The three fixture blobs are reproduced as hex in the appendix. They were produced independently on
our side; no generator code is included, so you can reimplement the fixture writer in your local
test project from the layout described above. Expected values:

```
content_id        = UP0000-PPSA00000_00-SYNTHETIC0000001
header_flags      = 0x85
chunks            = 3
scenarios         = 2
default_scenario  = 0
chunk 0: label='Chunk #0' mask=0xFFFFFFFFFFFFFFFF extents=[(0x100000,0x10000),(0x200000,0x8000)] total_bytes=98304
chunk 1: label='Chunk #1' mask=0x0000000000000001 extents=[(0x300000,0x20000)] total_bytes=131072
chunk 2: label='Chunk #2' mask=0x0000000000000002 extents=[(0x400000,0x4000),(0x500000,0x4000)] total_bytes=32768
scenario 0: label='Scenario #0' initial=2 sequence=[0,1,2]
scenario 1: label='Scenario #1' initial=1 sequence=[0,2]
assignments       = 3
  0xF16A60EEE4F247FC -> chunk 0  eboot.bin
  0x31BA97E20C62D897 -> chunk 1  sce_sys/param.sfo
  0xAEC4B3EC3EC6DCCB -> chunk 2  sce_sys/keystone
```

The existing `PlayGoChunkReader.Read(string)` accepts this `playgo-chunk.dat` unchanged, and it
round-trips through the PS5PKGTool reader with the values above.

### playgo-chunk.dat (608 bytes)

```
0000  70 6c 67 78 00 10 00 00 01 00 03 00 00 00 02 00
0010  60 02 00 00 00 00 01 00 00 00 00 00 85 00 00 00
0020  04 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00
0030  11 00 00 00 00 00 00 00 ff ff ff ff ff ff ff ff
0040  55 50 30 30 30 30 2d 50 50 53 41 30 30 30 30 30
0050  5f 30 30 2d 53 59 4e 54 48 45 54 49 43 30 30 30
0060  30 30 30 31 00 00 00 00 00 00 00 00 00 00 00 00
0070  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0080  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0090  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00A0  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00B0  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00C0  00 01 00 00 60 00 00 00 60 01 00 00 14 00 00 00
00D0  80 01 00 00 1b 00 00 00 a0 01 00 00 50 00 00 00
00E0  f0 01 00 00 40 00 00 00 30 02 00 00 0a 00 00 00
00F0  40 02 00 00 18 00 00 00 60 02 00 00 00 00 00 00
0100  80 00 03 00 02 00 00 00 11 00 00 00 00 00 00 00
0110  ff ff ff ff ff ff ff ff 00 00 00 00 00 00 00 00
0120  80 00 03 00 01 00 00 00 11 00 00 00 00 00 00 00
0130  01 00 00 00 00 00 00 00 08 00 00 00 09 00 00 00
0140  80 00 03 00 02 00 00 00 11 00 00 00 00 00 00 00
0150  02 00 00 00 00 00 00 00 0c 00 00 00 12 00 00 00
0160  00 00 00 00 01 00 00 00 02 00 00 00 03 00 00 00
0170  04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0180  43 68 75 6e 6b 20 23 30 00 43 68 75 6e 6b 20 23
0190  31 00 43 68 75 6e 6b 20 23 32 00 00 00 00 00 00
01A0  00 00 10 00 00 00 00 00 00 00 01 00 00 00 00 00
01B0  00 00 20 00 00 00 00 00 00 80 00 00 00 00 00 00
01C0  00 00 30 00 00 00 00 00 00 00 02 00 00 00 00 00
01D0  00 00 40 00 00 00 00 00 00 40 00 00 00 00 00 00
01E0  00 00 50 00 00 00 00 00 00 40 00 00 00 00 00 00
01F0  21 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0200  00 00 00 00 02 00 03 00 00 00 00 00 00 00 00 00
0210  21 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0220  00 00 00 00 01 00 02 00 06 00 00 00 0c 00 00 00
0230  00 00 01 00 02 00 00 00 02 00 00 00 00 00 00 00
0240  53 63 65 6e 61 72 69 6f 20 23 30 00 53 63 65 6e
0250  61 72 69 6f 20 23 31 00 00 00 00 00 00 00 00 00
```

### playgo-hash-table.dat (80 bytes)

```
0000  00 00 00 00 00 00 00 00 00 00 00 00 18 00 00 00
0010  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
0020  00 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00
0030  00 00 00 00 00 00 00 00 fc 47 f2 e4 ee 60 6a f1
0040  97 d8 62 0c e2 97 ba 31 cb dc c6 3e ec b3 c4 ae
```

### playgo-ficm.dat (22 bytes)

```
0000  00 00 00 00 00 00 00 00 00 00 00 00 06 00 00 00
0010  00 00 01 00 02 00
```

## 8. Delivery, licensing, and policy

- **Delivery is spec plus hex only.** No generator code is included, so no third-party authorship
  enters your tree. The fixture blobs in the appendix are the test vectors.
- Our originating repo has no root license, so we are not shipping any source under MIT. This
  document and the hex are facts/format plus generated data.
- Retail `playgo-chunk.dat` / hash table / FICM are game content and are deliberately not included.
- Retail multi-chunk: neither side has one. We will not block; if we later obtain one we will send
  parsed expectations (flags, counts, extent totals), never bytes.

## 9. Validation policy

- Extent ids in `0xC8`: in well-formed output (either producer) ids are always valid. For malformed
  input the engine convention is to fail loudly, matching the existing "scenario references
  undefined chunk" check. Default is therefore strict: bound-check and throw
  `InvalidDataException`, do not skip. A lenient inspection mode can be added later as an explicit
  opt-in.
- Our own reference reader is intentionally lenient here because it is used to inspect arbitrary
  user files in a GUI. That leniency is an application choice and is **not** the recommended
  engine default.
