using DarkUI.Forms;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Tasks;
using PS5PKGTool.Ffpfsc;
using UFS2Tool;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private const string ImageActionConvert = "Create / Convert";
    private const string ImageActionExtract = "Extract";
    private const string ImageActionVerify = "Verify";
    private const string ImageActionEdit = "Edit Files";
    private const string ImageActionRepair = "Repair";
    private const string ImageActionRefreshAmpr = "Refresh AMPR";
    private const string ImageActionRebuild = "Rebuild";

    private enum ImageToolTarget
    {
        Exfat,
        Ffpkg,
        Ffpfsc,
        DebugPackage
    }

    private string? _imageSourcePath;
    private Ps5ImageFormat _imageSourceFormat;
    private bool _imageSourceIsPackage;
    private SonyPkgKind? _imageSourcePackageKind;
    private readonly List<ImageToolTarget> _imageTargets = [];
    private bool _suppressImageEvents;

    private ImageToolTarget? CurrentImageTarget =>
        cboImageTarget.SelectedIndex >= 0 && cboImageTarget.SelectedIndex < _imageTargets.Count
            ? _imageTargets[cboImageTarget.SelectedIndex]
            : null;

    private void RefreshImageTools() => SyncImageSourceFromLibrary();

    private void SyncImageSourceFromLibrary()
    {
        string? source = _selectedGame?.RootPath;
        if (!string.IsNullOrEmpty(source) && (Directory.Exists(source) || File.Exists(source)))
            SetImageSource(source);
        else if (_imageSourcePath is null)
            SetImageSource(null);
    }

    private void SetImageSource(string? path)
    {
        _imageSourcePath = path;
        if (path is null)
        {
            _imageSourceFormat = Ps5ImageFormat.Unknown;
            _imageSourceIsPackage = false;
            _imageSourcePackageKind = null;
            lblImageSourcePath.Text = "Select a game in the library above.";
            lblImageFormat.Text = "Detected: -";
        }
        else
        {
            _imageSourceFormat = Directory.Exists(path) ? Ps5ImageFormat.Unknown : Ps5ImageFormatProbe.Detect(path);
            _imageSourceIsPackage = !Directory.Exists(path) &&
                Path.GetExtension(path).Equals(".pkg", StringComparison.OrdinalIgnoreCase);
            _imageSourcePackageKind = _imageSourceIsPackage ? TryReadPackageKind(path) : null;
            lblImageSourcePath.Text = path;
            lblImageFormat.Text = "Detected: " + ImageFormatLabel(path);
        }
        UpdateImageActionList();
        if (_imageSourceIsPackage && _imageSourcePackageKind != SonyPkgKind.FinalizedDebug)
            lblImageStatus.Text = _imageSourcePackageKind == SonyPkgKind.FinalizedPatch
                ? "Patch package detected. Convert, Extract and Verify require a finalized debug (FPKG) package."
                : "Retail package detected. Convert, Extract and Verify require a debug (FPKG) package.";
        else if (!_imageSourceIsPackage)
            lblImageStatus.Text = "Select a source, then choose an action.";
    }

    private static SonyPkgKind? TryReadPackageKind(string path)
    {
        try
        {
            return new SonyPkgReader().Read(path).Kind;
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            return null;
        }
    }

    private string ImageFormatLabel(string path)
    {
        if (Directory.Exists(path)) return "dump folder";
        if (_imageSourceIsPackage)
            return _imageSourcePackageKind switch
            {
                SonyPkgKind.FinalizedDebug => "debug package (FPKG)",
                SonyPkgKind.FinalizedPatch => "patch package (LIH)",
                SonyPkgKind.FinalizedRetail => "retail package",
                SonyPkgKind.MetadataContainer => "CNT metadata",
                _ => "package"
            };
        return _imageSourceFormat switch
        {
            Ps5ImageFormat.Exfat => "exFAT",
            Ps5ImageFormat.Ufs2 => "FFPKG (UFS2)",
            Ps5ImageFormat.Pfs => "FFPFSC",
            _ => "unknown"
        };
    }

    private List<string> AvailableImageActions()
    {
        List<string> actions = [];
        if (_imageSourcePath is null) return actions;
        if (Directory.Exists(_imageSourcePath))
        {
            actions.Add(ImageActionConvert);
            return actions;
        }
        if (_imageSourceIsPackage)
        {
            if (_imageSourcePackageKind == SonyPkgKind.FinalizedDebug)
            {
                actions.Add(ImageActionConvert);
                actions.Add(ImageActionExtract);
                actions.Add(ImageActionVerify);
            }
            return actions;
        }
        if (!File.Exists(_imageSourcePath)) return actions;
        if (_imageSourceFormat is Ps5ImageFormat.Exfat or Ps5ImageFormat.Ufs2 or Ps5ImageFormat.Pfs)
        {
            actions.Add(ImageActionConvert);
            actions.Add(ImageActionExtract);
            actions.Add(ImageActionVerify);
            if (_imageSourceFormat == Ps5ImageFormat.Exfat)
            {
                actions.Add(ImageActionEdit);
                actions.Add(ImageActionRepair);
                actions.Add(ImageActionRefreshAmpr);
            }
            else if (_imageSourceFormat == Ps5ImageFormat.Ufs2)
            {
                actions.Add(ImageActionEdit);
                actions.Add(ImageActionRebuild);
            }
        }
        return actions;
    }

    private void UpdateImageActionList()
    {
        _suppressImageEvents = true;
        try
        {
            string? previous = cboImageAction.SelectedItem as string;
            List<string> actions = AvailableImageActions();
            cboImageAction.Items.Clear();
            foreach (string action in actions)
                cboImageAction.Items.Add(action);
            if (previous is not null && actions.Contains(previous))
                cboImageAction.SelectedItem = previous;
            else if (cboImageAction.Items.Count > 0)
                cboImageAction.SelectedIndex = 0;
        }
        finally
        {
            _suppressImageEvents = false;
        }
        UpdateImageTargetList();
    }

    private void UpdateImageTargetList()
    {
        _suppressImageEvents = true;
        try
        {
            _imageTargets.Clear();
            cboImageTarget.Items.Clear();
            if (cboImageAction.SelectedItem as string == ImageActionConvert && _imageSourcePath is not null)
            {
                if (_imageSourceIsPackage)
                {
                    _imageTargets.AddRange([ImageToolTarget.Exfat, ImageToolTarget.Ffpkg, ImageToolTarget.Ffpfsc]);
                }
                else if (Directory.Exists(_imageSourcePath))
                {
                    _imageTargets.AddRange([ImageToolTarget.Exfat, ImageToolTarget.Ffpkg,
                        ImageToolTarget.Ffpfsc, ImageToolTarget.DebugPackage]);
                }
                else
                {
                    foreach (Ps5ImageConversionTarget target in Enum.GetValues<Ps5ImageConversionTarget>())
                        if (Ps5ImageConversionService.IsSupported(_imageSourceFormat, target))
                            _imageTargets.Add(MapTarget(target));
                    _imageTargets.Add(ImageToolTarget.DebugPackage);
                }
                foreach (ImageToolTarget target in _imageTargets)
                    cboImageTarget.Items.Add(ImageTargetLabel(target));
                if (cboImageTarget.Items.Count > 0)
                {
                    int index = _imageTargets.IndexOf(DefaultImageTarget());
                    cboImageTarget.SelectedIndex = index >= 0 ? index : 0;
                }
            }
        }
        finally
        {
            _suppressImageEvents = false;
        }
        UpdateImageOptionVisibility();
    }

    private static ImageToolTarget MapTarget(Ps5ImageConversionTarget target) => target switch
    {
        Ps5ImageConversionTarget.Exfat => ImageToolTarget.Exfat,
        Ps5ImageConversionTarget.Ffpkg => ImageToolTarget.Ffpkg,
        _ => ImageToolTarget.Ffpfsc
    };

    private static Ps5ImageConversionTarget MapTarget(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => Ps5ImageConversionTarget.Exfat,
        ImageToolTarget.Ffpkg => Ps5ImageConversionTarget.Ffpkg,
        _ => Ps5ImageConversionTarget.Ffpfsc
    };

    private ImageToolTarget DefaultImageTarget()
    {
        if (_imageSourceIsPackage) return ImageToolTarget.Ffpkg;
        if (_imageSourcePath is not null && Directory.Exists(_imageSourcePath))
            return ImageToolTarget.Ffpkg;
        return _imageSourceFormat switch
        {
            Ps5ImageFormat.Exfat => ImageToolTarget.Ffpfsc,
            Ps5ImageFormat.Ufs2 => ImageToolTarget.Ffpfsc,
            Ps5ImageFormat.Pfs => ImageToolTarget.Exfat,
            _ => ImageToolTarget.Ffpfsc
        };
    }

    private static string ImageTargetLabel(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => "exFAT image",
        ImageToolTarget.Ffpkg => "FFPKG (UFS2)",
        ImageToolTarget.Ffpfsc => "FFPFSC image",
        _ => "Debug Package (FPKG)"
    };

    private static string ImageTargetExtension(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => ".exfat",
        ImageToolTarget.Ffpkg => ".ffpkg",
        ImageToolTarget.Ffpfsc => ".ffpfsc",
        _ => ".pkg"
    };

    private void UpdateImageOptionVisibility()
    {
        string? action = cboImageAction.SelectedItem as string;
        bool convert = action == ImageActionConvert;
        bool extract = action == ImageActionExtract;
        bool verify = action == ImageActionVerify;
        ImageToolTarget? target = convert ? CurrentImageTarget : null;
        bool showCluster = target is ImageToolTarget.Exfat or ImageToolTarget.Ffpfsc;
        bool showCompression = target == ImageToolTarget.Ffpfsc;
        bool showFfpkg = target == ImageToolTarget.Ffpkg;
        bool showDebug = target == ImageToolTarget.DebugPackage;
        bool showPasscode = showDebug || (_imageSourceIsPackage && (extract || verify));

        lblImageTarget.Visible = convert;
        cboImageTarget.Visible = convert;
        lblImageCluster.Visible = showCluster;
        cboImageCluster.Visible = showCluster;
        lblImageLevel.Visible = showCompression;
        nudImageLevel.Visible = showCompression;
        lblImageGain.Visible = showCompression;
        nudImageGain.Visible = showCompression;
        chkImageAmpr.Visible = showCluster;
        lblImageBlock.Visible = showFfpkg;
        cboImageBlock.Visible = showFfpkg;
        lblImageFragment.Visible = showFfpkg;
        cboImageFragment.Visible = showFfpkg;
        lblImageDensity.Visible = showFfpkg;
        cboImageDensity.Visible = showFfpkg;
        lblImageMinFree.Visible = showFfpkg;
        nudImageMinFree.Visible = showFfpkg;
        lblImageContentId.Visible = showDebug;
        txtImageContentId.Visible = showDebug;
        lblImagePasscode.Visible = showPasscode;
        txtImagePasscode.Visible = showPasscode;
        chkImageSdkOverride.Visible = showDebug;
        cboImageSdk.Visible = showDebug && chkImageSdkOverride.Checked;
        bool showOutput = convert || extract;
        lblImageOutput.Visible = showOutput;
        txtImageOutput.Visible = showOutput;
        btnImageBrowseOutput.Visible = showOutput;
        chkImageOverwrite.Visible = showOutput;
        lblImageOutput.Text = extract ? "Output folder:" : "Output file:";

        if (convert)
        {
            SuggestImageOutput(ImageTargetExtension(target ?? ImageToolTarget.Ffpfsc));
            if (showDebug) SuggestImageContentId();
        }
        else if (extract) SuggestImageOutput("-files");

        btnImageRun.Enabled = action is not null;
        btnImageCancel.Enabled = _taskQueue.Tasks.Any(task =>
            task.Status is PackageTaskStatus.Running or PackageTaskStatus.Queued);
    }

    private void SuggestImageOutput(string suffix)
    {
        if (_imageSourcePath is null) { txtImageOutput.Clear(); return; }
        bool directory = Directory.Exists(_imageSourcePath);
        string name = directory ? new DirectoryInfo(_imageSourcePath).Name : Path.GetFileNameWithoutExtension(_imageSourcePath);
        string? parent = directory ? Directory.GetParent(_imageSourcePath)?.FullName : Path.GetDirectoryName(_imageSourcePath);
        txtImageOutput.Text = Path.Combine(parent ?? _imageSourcePath, name + suffix);
    }

    private void btnImageUseSelected_Click(object? sender, EventArgs e)
    {
        if (_selectedGame is not { } game) return;
        SetImageSource(game.RootPath);
    }

    private void btnImageChoose_Click(object? sender, EventArgs e)
    {
        if (openImageDialog.ShowDialog(this) != DialogResult.OK) return;
        SetImageSource(openImageDialog.FileName);
    }

    private void cboImageAction_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressImageEvents) return;
        UpdateImageTargetList();
    }

    private void cboImageTarget_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressImageEvents) return;
        UpdateImageOptionVisibility();
    }

    private void btnImageBrowseOutput_Click(object? sender, EventArgs e)
    {
        if (cboImageAction.SelectedItem as string == ImageActionExtract)
        {
            folderBrowserDialog.Description = "Select the extraction folder";
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                txtImageOutput.Text = folderBrowserDialog.SelectedPath;
            return;
        }

        ImageToolTarget target = CurrentImageTarget ?? ImageToolTarget.Ffpfsc;
        imageSaveDialog.Filter = ImageTargetLabel(target) + " (*" + ImageTargetExtension(target) + ")|*" +
                                 ImageTargetExtension(target) + "|All files (*.*)|*.*";
        imageSaveDialog.FileName = Path.GetFileName(txtImageOutput.Text);
        string? directory = Path.GetDirectoryName(txtImageOutput.Text);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            imageSaveDialog.InitialDirectory = directory;
        if (imageSaveDialog.ShowDialog(this) == DialogResult.OK)
            txtImageOutput.Text = imageSaveDialog.FileName;
    }

    private void btnImageRun_Click(object? sender, EventArgs e)
    {
        if (_imageSourcePath is null) return;
        switch (cboImageAction.SelectedItem as string)
        {
            case ImageActionConvert: RunImageConvert(); break;
            case ImageActionExtract: if (_imageSourceIsPackage) RunPackageExtract(); else RunImageExtract(); break;
            case ImageActionVerify: if (_imageSourceIsPackage) RunPackageVerify(); else RunImageVerify(); break;
            case ImageActionEdit: RunImageEdit(); break;
            case ImageActionRepair: RunImageRepair(); break;
            case ImageActionRefreshAmpr: RunImageRefreshAmpr(); break;
            case ImageActionRebuild: RunImageRebuild(); break;
        }
    }

    private void btnImageCancel_Click(object? sender, EventArgs e)
    {
        QueuedPackageTask? task = _taskQueue.Tasks.FirstOrDefault(candidate =>
            candidate.Status is PackageTaskStatus.Running or PackageTaskStatus.Queued);
        if (task is not null) _taskQueue.Cancel(task.Id);
        lblImageStatus.Text = "Cancelling...";
    }

    private void RunImageConvert()
    {
        string source = _imageSourcePath!;
        string output = txtImageOutput.Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an output file.", "Image Tools");
            return;
        }
        ImageToolTarget toolTarget = CurrentImageTarget ?? ImageToolTarget.Ffpfsc;
        if (toolTarget == ImageToolTarget.DebugPackage)
        {
            RunImageBuildPackage();
            return;
        }
        Ps5ImageConversionTarget target = MapTarget(toolTarget);
        bool overwrite = chkImageOverwrite.Checked;

        ExfatBuildOptions? exfatOptions = target == Ps5ImageConversionTarget.Ffpkg
            ? null
            : new ExfatBuildOptions
            {
                ClusterSize = cboImageCluster.SelectedIndex switch { 1 => 32768, 2 => 65536, _ => null },
                GenerateAmprIndex = chkImageAmpr.Checked
            };
        FfpfscBuildOptions? ffpfscOptions = target == Ps5ImageConversionTarget.Ffpfsc
            ? new FfpfscBuildOptions
            {
                Compression = new PfscCompressionOptions
                {
                    CompressionLevel = (int)nudImageLevel.Value,
                    MinimumGainPercent = (int)nudImageGain.Value
                }
            }
            : null;
        FfpkgBuildOptions? ffpkgOptions = null;
        if (target == Ps5ImageConversionTarget.Ffpkg)
        {
            int blockSize = cboImageBlock.SelectedIndex == 1 ? 65536 : 32768;
            int fragmentSize = cboImageFragment.SelectedIndex == 1 ? 65536 : 4096;
            if (fragmentSize > blockSize) fragmentSize = blockSize;
            ffpkgOptions = new FfpkgBuildOptions
            {
                BlockSize = blockSize,
                FragmentSize = fragmentSize,
                BytesPerInode = cboImageDensity.SelectedIndex switch { 1 => 524288, 2 => 1048576, _ => 262144 },
                MinFreePercent = (int)nudImageMinFree.Value
            };
        }

        bool fromPackage = _imageSourceIsPackage;
        string sourceRoute = fromPackage ? "FPKG" : Directory.Exists(source) ? "dump" : ImageFormatLabel(source);
        string targetRoute = ImageTargetShortLabel(toolTarget);
        lblImageStatus.Text = fromPackage
            ? "Queued: package conversion. See the Tasks tab."
            : "Queued: conversion. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ImageConvert, $"Convert {Path.GetFileName(source)}",
            (progress, token) => ConvertImageAsync(source, output, target, overwrite, exfatOptions, ffpfscOptions,
                ffpkgOptions, fromPackage, progress, token),
            sourcePath: source, outputPath: output,
            operation: "Convert", sourceFormat: sourceRoute, targetFormat: targetRoute,
            stagePlan: fromPackage ? PackageTaskPlans.ConvertPackage : PackageTaskPlans.ConvertImage,
            payload: Payload(("source", source), ("output", output), ("target", target.ToString()),
                ("overwrite", overwrite.ToString()), ("package", fromPackage.ToString())),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? $"Converted to {Path.GetFileName(output)}."
                : $"Conversion {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private static string ImageTargetShortLabel(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => "exFAT",
        ImageToolTarget.Ffpkg => "FFPKG",
        ImageToolTarget.Ffpfsc => "FFPFSC",
        _ => "FPKG"
    };

    private static Task ConvertImageAsync(string source, string output, Ps5ImageConversionTarget target,
        bool overwrite, ExfatBuildOptions? exfatOptions, FfpfscBuildOptions? ffpfscOptions,
        FfpkgBuildOptions? ffpkgOptions, bool fromPackage, IProgress<PackageTaskProgress> progress,
        CancellationToken token)
    {
        var bridge = new Progress<Ps5ImageConversionProgress>(value =>
            progress.Report(new PackageTaskProgress(value.Stage, 0, 0, value.Completed, value.Total, 0, 0,
                string.Empty)));
        return fromPackage
            ? SonyPackageImageConversion.ConvertAsync(source, output, target, overwrite, bridge, token,
                exfatOptions, ffpfscOptions, ffpkgOptions)
            : Ps5ImageConversionService.ConvertAsync(source, output, target, overwrite, bridge, token,
                exfatOptions, ffpfscOptions, ffpkgOptions);
    }

    private void RunImageExtract()
    {
        string source = _imageSourcePath!;
        string output = txtImageOutput.Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an extraction folder.", "Image Tools");
            return;
        }
        if (Directory.Exists(output)) output = FindAvailableDirectory(output);
        Ps5ImageFormat format = _imageSourceFormat;

        lblImageStatus.Text = "Queued: extraction. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.PackageExtract, $"Extract {Path.GetFileName(source)}",
            (progress, token) => ExtractImageToAsync(source, format, output, progress, token),
            sourcePath: source, outputPath: output,
            operation: "Extract", sourceFormat: ImageFormatLabel(source), targetFormat: "folder",
            stagePlan: PackageTaskPlans.Extract,
            payload: Payload(("source", source), ("output", output),
                ("kind", format switch
                {
                    Ps5ImageFormat.Exfat => "exfat",
                    Ps5ImageFormat.Ufs2 => "ffpkg",
                    _ => "ffpfsc"
                })),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? $"Extracted to {output}."
                : $"Extraction {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private static async Task ExtractImageToAsync(string source, Ps5ImageFormat format, string output,
        IProgress<PackageTaskProgress> progress, CancellationToken token)
    {
        switch (format)
        {
            case Ps5ImageFormat.Exfat:
                await ExfatImage.ExtractDirectoryAsync(source, output, AdaptFfpfscProgress(progress), token)
                    .ConfigureAwait(false);
                break;
            case Ps5ImageFormat.Ufs2:
                await Ufs2Operations.ExtractAsync(source, output, AdaptUfs2Progress(progress), token)
                    .ConfigureAwait(false);
                break;
            case Ps5ImageFormat.Pfs:
                await FfpfscImage.ExtractToDirectoryAsync(source, output, overwrite: true,
                    AdaptFfpfscProgress(progress), token).ConfigureAwait(false);
                break;
            default:
                throw new InvalidDataException("Unsupported source image format.");
        }
    }

    private void RunImageVerify()
    {
        string source = _imageSourcePath!;
        Ps5ImageFormat format = _imageSourceFormat;
        lblImageStatus.Text = "Queued: verification. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ImageVerify, $"Verify {Path.GetFileName(source)}",
            (progress, token) => VerifyImageAsync(source, format, token),
            sourcePath: source, outputPath: source,
            operation: "Verify", sourceFormat: ImageFormatLabel(source), targetFormat: "",
            stagePlan: PackageTaskPlans.Verify,
            payload: Payload(("source", source), ("format", format.ToString())),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? "Verification passed."
                : $"Verification {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private static async Task VerifyImageAsync(string source, Ps5ImageFormat format, CancellationToken token)
    {
        switch (format)
        {
            case Ps5ImageFormat.Exfat:
                await ExfatImage.VerifyAsync(source, null, token).ConfigureAwait(false);
                break;
            case Ps5ImageFormat.Ufs2:
                await Ufs2Operations.VerifyAsync(source, null, token).ConfigureAwait(false);
                break;
            case Ps5ImageFormat.Pfs:
                FfpfscVerificationResult result = await FfpfscImage.TryVerifyAsync(source, null, null, token)
                    .ConfigureAwait(false);
                if (!result.StructureValid)
                    throw new InvalidDataException("The FFPFSC structure is invalid. " + (result.Error ?? string.Empty));
                if (!result.EveryPfscBlockDecodes)
                    throw new InvalidDataException("The FFPFSC block decode failed. " + (result.Error ?? string.Empty));
                break;
            default:
                throw new InvalidDataException("Unsupported image format.");
        }
    }

    private string ImagePasscode() => txtImagePasscode.Text.Length > 0
        ? txtImagePasscode.Text
        : !string.IsNullOrEmpty(_settings.DebugPasscode)
            ? _settings.DebugPasscode
            : SonyDebugPackageCredentials.DefaultPasscode;

    private void RunPackageExtract()
    {
        string source = _imageSourcePath!;
        string output = txtImageOutput.Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an extraction folder.", "Image Tools");
            return;
        }
        if (Directory.Exists(output)) output = FindAvailableDirectory(output);
        string passcode = ImagePasscode();
        SonyPackageExtractResult? outcome = null;
        lblImageStatus.Text = "Queued: package extraction. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.PackageExtract, $"Extract {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                outcome = await SonyPackageExtraction.ExtractAsync(source, output, passcode,
                    AdaptSonyExtractProgress(progress), token).ConfigureAwait(false);
            },
            sourcePath: source, outputPath: output,
            operation: "Extract package", sourceFormat: "FPKG", targetFormat: "folder",
            stagePlan: PackageTaskPlans.Extract,
            payload: Payload(("source", source), ("output", output), ("kind", "sony"), ("passcode", passcode)),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed && outcome is not null
                ? $"Extracted {outcome.FileCount:N0} file(s) to {output}."
                : $"Extraction {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private void RunPackageVerify()
    {
        string source = _imageSourcePath!;
        string passcode = ImagePasscode();
        SonyDebugPackageValidationResult? outcome = null;
        lblImageStatus.Text = "Queued: package verification. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.PackageVerify, $"Verify {Path.GetFileName(source)}",
            (progress, token) =>
            {
                outcome = SonyDebugPackageBuilder.Validate(source, passcode);
                return Task.CompletedTask;
            },
            sourcePath: source, outputPath: source,
            operation: "Verify package", sourceFormat: "FPKG", targetFormat: "",
            stagePlan: PackageTaskPlans.Verify,
            payload: Payload(("source", source), ("passcode", passcode)),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed &&
                                outcome is { IsValid: true }
                ? $"Package verified: {outcome.IndexedFiles:N0} indexed file(s)."
                : $"Verification {StatusText(task.Status).ToLowerInvariant()}" +
                  (outcome is null ? "." : ": " + outcome.Message));
    }

    private void RunImageEdit()
    {
        string source = _imageSourcePath!;
        try
        {
            if (_imageSourceFormat == Ps5ImageFormat.Exfat)
            {
                using var editor = new ExfatEditorForm(source);
                editor.ShowDialog(this);
            }
            else if (_imageSourceFormat == Ps5ImageFormat.Ufs2)
            {
                using var editor = new FfpkgEditorForm(source);
                editor.ShowDialog(this);
            }
            lblImageStatus.Text = $"Closed editor for {Path.GetFileName(source)}.";
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            AppDialog.ShowError(ex.Message, "Open image editor");
        }
    }

    private void RunImageRepair()
    {
        string source = _imageSourcePath!;
        if (AppDialog.ShowWarning(
                "Repair first restores a damaged boot region when the other copy is valid, then extracts every " +
                "readable file, rebuilds the filesystem metadata beside the original, fully verifies the result, " +
                "and only then replaces the original image.\n\n" + source,
                "Repair exFAT image?", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        ExfatRepairResult? outcome = null;
        lblImageStatus.Text = "Queued: repair. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ExfatRepair, $"Repair {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                outcome = await ExfatImageMaintenance.RepairAsync(source, AdaptFfpfscProgress(progress), token)
                    .ConfigureAwait(false);
            },
            sourcePath: source, outputPath: source,
            operation: "Repair", sourceFormat: "exFAT", targetFormat: "exFAT",
            payload: Payload(("source", source)),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? "exFAT image repaired."
                : $"Repair {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private void RunImageRefreshAmpr()
    {
        string source = _imageSourcePath!;
        if (AppDialog.ShowWarning(
                "This creates or refreshes the root ampr_emu.index. It can relocate the index, extend the root " +
                "directory, and grow the image tail; all changes are restored if verification fails.\n\n" + source,
                "Refresh AMPR index?", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        ExfatAmprRefreshResult? outcome = null;
        lblImageStatus.Text = "Queued: AMPR refresh. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ExfatAmpr, $"AMPR index for {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                outcome = await ExfatAmprPatcher.RefreshAsync(source, AdaptFfpfscProgress(progress), token)
                    .ConfigureAwait(false);
            },
            sourcePath: source, outputPath: source,
            operation: "Refresh AMPR", sourceFormat: "exFAT", targetFormat: "exFAT",
            payload: Payload(("source", source)),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? "AMPR index refreshed."
                : $"AMPR refresh {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private void RunImageRebuild()
    {
        string source = _imageSourcePath!;
        if (AppDialog.ShowWarning(
                "This extracts every readable file, rebuilds all UFS2 metadata beside the original, fully verifies " +
                "the replacement, and only then swaps it into place. It requires substantial free disk space.\n\n" +
                source, "Rebuild FFPKG image?", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        Ufs2VerificationResult? outcome = null;
        lblImageStatus.Text = "Queued: rebuild. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.FfpkgRebuild, $"Rebuild {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                outcome = await Ufs2Operations.RebuildWithEditsAsync(source, static _ => { },
                    AdaptUfs2Progress(progress), token).ConfigureAwait(false);
            },
            sourcePath: source, outputPath: source,
            operation: "Rebuild", sourceFormat: "FFPKG", targetFormat: "FFPKG",
            payload: Payload(("source", source)),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed && outcome is not null
                ? $"Rebuilt: {outcome.FsckErrors:N0} fsck errors."
                : $"Rebuild {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private void InitializeImageSdkList()
    {
        cboImageSdk.Items.Clear();
        foreach (Ps5SdkVersions.Release release in Ps5SdkVersions.Releases)
            cboImageSdk.Items.Add(release.Version);
        int index = cboImageSdk.Items.IndexOf("9.00.00.40");
        cboImageSdk.SelectedIndex = index >= 0
            ? index
            : cboImageSdk.Items.Count > 0 ? cboImageSdk.Items.Count - 1 : -1;
    }

    private ulong? SelectedSdkOverride() =>
        chkImageSdkOverride.Checked ? Ps5SdkVersions.ExecutableVersionAt(cboImageSdk.SelectedIndex) : null;

    private void chkImageSdkOverride_CheckedChanged(object? sender, EventArgs e) =>
        cboImageSdk.Visible = chkImageSdkOverride.Visible && chkImageSdkOverride.Checked;

    private void RunImageBuildPackage()
    {
        string source = _imageSourcePath!;
        string output = txtImageOutput.Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an output .pkg file.", "Image Tools");
            return;
        }
        string contentId = txtImageContentId.Text.Trim();
        if (contentId.Length == 0)
        {
            AppDialog.ShowWarning("Enter the package content ID.", "Image Tools");
            return;
        }
        string passcode = ImagePasscode();
        bool overwrite = chkImageOverwrite.Checked;
        ulong? sdkVersionOverride = SelectedSdkOverride();

        lblImageStatus.Text = "Queued: package build. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ImageBuildPackage, $"Build package from {Path.GetFileName(source)}",
            (progress, token) => BuildPackageFromSourceAsync(source, output, contentId, passcode, overwrite,
                sdkVersionOverride, progress, token),
            sourcePath: source, outputPath: output,
            operation: "Build package", sourceFormat: ImageFormatLabel(source), targetFormat: "FPKG",
            stagePlan: PackageTaskPlans.BuildPackage,
            payload: Payload(("source", source), ("output", output), ("contentId", contentId),
                ("passcode", passcode), ("overwrite", overwrite.ToString()),
                ("sdk", sdkVersionOverride?.ToString("X16"))),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? $"Built package {Path.GetFileName(output)}."
                : $"Package build {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private static async Task BuildPackageFromSourceAsync(string source, string output, string contentId,
        string passcode, bool overwrite, ulong? sdkVersionOverride, IProgress<PackageTaskProgress> progress,
        CancellationToken token)
    {
        if (!overwrite && File.Exists(output))
            throw new IOException($"The output file already exists: {output}");
        // Build to a sibling partial file and move it into place on success, so a failed build
        // cannot leave a half-written .pkg that looks valid.
        string partial = output + ".partial-" + Guid.NewGuid().ToString("N");
        var options = new SonyDebugPackageBuildOptions
        {
            ContentId = contentId,
            Passcode = passcode,
            SdkVersionOverride = sdkVersionOverride
        };
        var bridge = new Progress<SonyDebugPackageProgress>(value =>
            progress.Report(new PackageTaskProgress(value.Stage, 0, 0, value.CompletedBytes, value.TotalBytes,
                0, 0, value.CurrentPath)));
        try
        {
            if (Directory.Exists(source))
                await SonyDebugPackageBuilder.CreateFromDirectoryAsync(source, partial, options, bridge, token)
                    .ConfigureAwait(false);
            else
                await VolumeDebugPackageBuilder.CreateFromImageAsync(source, partial, options, bridge, token)
                    .ConfigureAwait(false);
            File.Move(partial, output, overwrite: true);
        }
        catch
        {
            TryDeleteFile(partial);
            throw;
        }
    }

    private void SuggestImageContentId()
    {
        if (!string.IsNullOrWhiteSpace(txtImageContentId.Text) || _imageSourcePath is null) return;
        string? contentId = TryReadParamContentId(_imageSourcePath, _imageSourceFormat);
        if (contentId is not null) txtImageContentId.Text = contentId;
    }

    private static string? TryReadParamContentId(string source, Ps5ImageFormat format)
    {
        byte[]? bytes = null;
        try
        {
            if (Directory.Exists(source))
            {
                string path = Path.Combine(source, "sce_sys", "param.json");
                if (File.Exists(path)) bytes = File.ReadAllBytes(path);
            }
            else
            {
                switch (format)
                {
                    case Ps5ImageFormat.Exfat:
                        using (var volume = new ExfatVolume(File.OpenRead(source)))
                            bytes = volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                        break;
                    case Ps5ImageFormat.Ufs2:
                        using (var volume = new Ufs2Volume(source))
                            bytes = volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                        break;
                    case Ps5ImageFormat.Pfs:
                        using (var volume = FfpfscVolume.Open(source))
                            bytes = volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                        break;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   ArgumentException or NotSupportedException)
        {
            return null;
        }

        if (bytes is null || bytes.Length == 0) return null;
        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(bytes);
            if (document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                document.RootElement.TryGetProperty("contentId", out System.Text.Json.JsonElement element) &&
                element.ValueKind == System.Text.Json.JsonValueKind.String)
                return element.GetString();
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
        return null;
    }
}
