using DarkUI.Forms;
using PS5PKGTool.Core.Backends;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Tasks;
using PS5PKGTool.Ffpfsc;
using PS5PKGTool.Infrastructure;
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

    private string? _imageContentIdSource;
    private string _imageContentId = string.Empty;

    private ImageToolTarget? CurrentImageTarget =>
        cboImageAction.SelectedItem as string == ImageActionConvert ? TabTarget(tabsImageTargets.SelectedTab) : null;

    private ImageToolTarget? TabTarget(System.Windows.Forms.TabPage? page)
    {
        if (ReferenceEquals(page, tabTargetExfat)) return ImageToolTarget.Exfat;
        if (ReferenceEquals(page, tabTargetFfpkg)) return ImageToolTarget.Ffpkg;
        if (ReferenceEquals(page, tabTargetFfpfsc)) return ImageToolTarget.Ffpfsc;
        if (ReferenceEquals(page, tabTargetDebug)) return ImageToolTarget.DebugPackage;
        return null;
    }

    private DarkUI.Controls.DarkTabPage TabPageFor(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => tabTargetExfat,
        ImageToolTarget.Ffpkg => tabTargetFfpkg,
        ImageToolTarget.Ffpfsc => tabTargetFfpfsc,
        _ => tabTargetDebug
    };


    private readonly List<(DarkUI.Controls.DarkTabPage Page, (Control Control, Point Base)[] Items)> _centeredImageTabs = [];

    /// <summary>
    /// Centers the option controls on each target tab horizontally so they stay centered when the
    /// window is maximized, rather than hugging the left or right edge.
    /// </summary>
    private void InitializeCenteredImageTabs()
    {
        RegisterCenteredTab(tabTargetExfat);
        RegisterCenteredTab(tabTargetFfpfsc);
        RegisterCenteredTab(tabTargetFfpkg);
        RegisterCenteredTab(tabTargetDebug);
    }

    private void RegisterCenteredTab(DarkUI.Controls.DarkTabPage page)
    {
        var items = new List<(Control, Point)>();
        foreach (Control control in page.Controls)
        {
            // Drop the designer's right-anchor: centering is driven by this handler instead.
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            items.Add((control, control.Location));
        }
        if (items.Count == 0) return;
        _centeredImageTabs.Add((page, items.ToArray()));
        page.Resize += (_, _) => CenterImageTab(page);
        CenterImageTab(page);
    }

    private void CenterImageTab(DarkUI.Controls.DarkTabPage page)
    {
        var entry = _centeredImageTabs.FirstOrDefault(candidate => ReferenceEquals(candidate.Page, page));
        if (entry.Items is null || entry.Items.Length == 0) return;
        int minX = entry.Items.Min(item => item.Base.X);
        int maxX = entry.Items.Max(item => item.Base.X + item.Control.Width);
        int offset = ((page.ClientSize.Width - (maxX - minX)) / 2) - minX;
        foreach ((Control control, Point baseLocation) in entry.Items)
            control.Location = new Point(Math.Max(0, baseLocation.X + offset), baseLocation.Y);
    }

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
        _imageContentIdSource = null;
        _imageContentId = string.Empty;
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
            Ps5ImageFormat.Ufs2 => "FFPKG",
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
            }

            tabsImageTargets.TabPages.Clear();
            if (cboImageAction.SelectedItem as string == ImageActionConvert)
            {
                foreach (ImageToolTarget target in _imageTargets)
                    tabsImageTargets.TabPages.Add(TabPageFor(target));
                if (tabsImageTargets.TabPages.Count > 0)
                    tabsImageTargets.SelectedIndex = 0;
            }
            else
            {
                tabsImageTargets.TabPages.Add(tabTargetOptions);
            }
        }
        finally
        {
            _suppressImageEvents = false;
        }
        UpdateImageOptionVisibility();
    }

    private void tabsImageTargets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressImageEvents) return;
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

    private static string ImageTargetLabel(ImageToolTarget target) => target switch
    {
        ImageToolTarget.Exfat => "exFAT image",
        ImageToolTarget.Ffpkg => "FFPKG",
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
        bool showDebug = target == ImageToolTarget.DebugPackage;
        bool showPasscode = _imageSourceIsPackage && (extract || verify);

        // Target-specific option controls live in their own target tab, so they are always shown
        // within that tab. Only the shared Job fields and the non-convert Options tab are toggled.
        lblImageOutput.Visible = extract;
        txtImageOutput.Visible = extract;
        btnImageBrowseOutput.Visible = extract;
        chkImageOverwrite.Visible = extract;
        lblImagePasscode.Visible = showPasscode;
        txtImagePasscode.Visible = showPasscode;
        lblImageOutput.Text = extract ? "Output folder:" : "Output file:";

        if (convert)
        {
            SuggestImageOutput(ImageTargetExtension(target ?? ImageToolTarget.Ffpfsc));
            if (showDebug) SuggestImageContentId();
        }
        else if (extract) SuggestImageOutput("-files");

        // Only the Advanced toggle is always shown; every build setting is hidden until it is ticked.
        void Show(Control? control, bool visible) { if (control is not null) control.Visible = visible; }
        Show(chkImageAdvancedOptions, showDebug);

        // Progressive disclosure: all build settings stay hidden until "Advanced options" is ticked.
        bool advanced = showDebug && ShowAdvancedOptions();
        Show(lblImageBackend, advanced);
        Show(cboImageBackend, advanced);
        Show(lblImageDrm, advanced);
        Show(cboImageDrm, advanced);
        Show(lblImagePkgType, advanced);
        Show(cboImagePkgType, advanced);
        Show(chkImageFakeSign, advanced);
        Show(lblImageSdk, advanced);
        Show(cboImageSdk, advanced);
        Show(lblImageCompression, advanced);
        Show(cboImageCompression, advanced);
        Show(lblImagePlayGo, advanced);
        Show(nudImagePlayGoChunks, advanced);
        Show(lblImageKrakenLevel, advanced);
        Show(cboImageKrakenLevel, advanced);
        Show(lblImageKrakenThreads, advanced);
        Show(nudImageKrakenThreads, advanced);
        Show(lblImageTemp, advanced);
        Show(txtImageTemp, advanced);
        Show(btnImageTempBrowse, advanced);
        Show(chkImageDeterministic, advanced);
        Show(chkImageRightSprx, advanced);

        // Controls a backend does not implement are disabled rather than left editable-but-ignored, so
        // the UI never implies an option was honoured when the library never receives it.
        IPackageBackend backend = SelectedBackend();
        bool lpp = showDebug && backend.Id == BackendRegistry.LppId;
        cboImageCompression.Enabled = true;
        cboImageKrakenLevel.Enabled = true;
        nudImageKrakenThreads.Enabled = true;
        nudImagePlayGoChunks.Enabled = true;
        chkImageDeterministic.Enabled = true;
        // LibProsperoPkg 1.2.0 receives neither a fake-sign nor an inject-right.sprx option.
        chkImageFakeSign.Enabled = !lpp;
        chkImageRightSprx.Enabled = !lpp;
        if (showDebug)
        {
            string compression = cboImageCompression.SelectedIndex switch { 1 => "Kraken", 2 => "stored", _ => "auto" };
            string drm = "DRM " + (cboImageDrm.SelectedItem?.ToString() ?? "Upgradable");
            string sdk = cboImageSdk.SelectedIndex <= 0 ? "SDK auto" : "SDK " + cboImageSdk.SelectedItem;
            string summary = $"{backend.DisplayName} · {compression} · {drm} · {sdk}";
            bool imageSource = !_imageSourceIsPackage && !Directory.Exists(_imageSourcePath ?? string.Empty);
            if (imageSource)
            {
                summary += lpp
                    ? " · image source: extracted to a workspace, then built"
                    : " · image source: built directly (no extract)";
            }
            if (lpp)
                summary += " · LibProsperoPkg applies SDK, DRM, compression, Kraken, PlayGo and workspace; it does " +
                    "not receive fake-sign or right.sprx, and Auto and Kraken (force) give the same result.";
            lblImageStatus.Text = summary;
        }

        btnImageRun.Enabled = action is not null && !(showDebug && ImagePackageTypeUnsupported());
        btnImageCancel.Enabled = _taskQueue.Tasks.Any(task =>
            task.Status is PackageTaskStatus.Running or PackageTaskStatus.Queued);
    }

    private TextBox ImageOutputBox() => CurrentImageTarget switch
    {
        ImageToolTarget.Exfat => txtOutExfat,
        ImageToolTarget.Ffpkg => txtOutFfpkg,
        ImageToolTarget.Ffpfsc => txtOutFfpfsc,
        ImageToolTarget.DebugPackage => txtOutDebug,
        _ => txtImageOutput
    };

    private CheckBox ImageOverwriteBox() => CurrentImageTarget switch
    {
        ImageToolTarget.Exfat => chkOutExfat,
        ImageToolTarget.Ffpkg => chkOutFfpkg,
        ImageToolTarget.Ffpfsc => chkOutFfpfsc,
        ImageToolTarget.DebugPackage => chkOutDebug,
        _ => chkImageOverwrite
    };

    private TextBox ImagePasscodeBox() => CurrentImageTarget == ImageToolTarget.DebugPackage
        ? txtDbgPasscode
        : txtImagePasscode;

    private void SuggestImageOutput(string suffix)
    {
        TextBox box = ImageOutputBox();
        if (_imageSourcePath is null) { box.Clear(); return; }
        bool directory = Directory.Exists(_imageSourcePath);
        string name = directory ? new DirectoryInfo(_imageSourcePath).Name : Path.GetFileNameWithoutExtension(_imageSourcePath);
        // The default output folder (when set and available) takes precedence for suggested outputs.
        string? parent = !string.IsNullOrWhiteSpace(_settings.OutputDirectory) && Directory.Exists(_settings.OutputDirectory)
            ? _settings.OutputDirectory
            : directory ? Directory.GetParent(_imageSourcePath)?.FullName : Path.GetDirectoryName(_imageSourcePath);
        box.Text = Path.Combine(parent ?? _imageSourcePath, name + suffix);
    }


    private void cboImageAction_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressImageEvents) return;
        UpdateImageTargetList();
    }

    private void btnImageBrowseOutput_Click(object? sender, EventArgs e)
    {
        TextBox box = ImageOutputBox();
        if (cboImageAction.SelectedItem as string == ImageActionExtract)
        {
            folderBrowserDialog.Description = "Select the extraction folder";
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                box.Text = folderBrowserDialog.SelectedPath;
            return;
        }

        ImageToolTarget target = CurrentImageTarget ?? ImageToolTarget.Ffpfsc;
        imageSaveDialog.Filter = ImageTargetLabel(target) + " (*" + ImageTargetExtension(target) + ")|*" +
                                 ImageTargetExtension(target) + "|All files (*.*)|*.*";
        imageSaveDialog.FileName = Path.GetFileName(box.Text);
        string? directory = Path.GetDirectoryName(box.Text);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            imageSaveDialog.InitialDirectory = directory;
        if (imageSaveDialog.ShowDialog(this) == DialogResult.OK)
            box.Text = imageSaveDialog.FileName;
    }


    private void btnImageTempBrowse_Click(object? sender, EventArgs e)
    {
        folderBrowserDialog.Description = "Select the temporary workspace folder";
        if (!string.IsNullOrWhiteSpace(txtImageTemp.Text) && Directory.Exists(txtImageTemp.Text))
            folderBrowserDialog.SelectedPath = txtImageTemp.Text;
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            txtImageTemp.Text = folderBrowserDialog.SelectedPath;
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
        string output = ImageOutputBox().Text.Trim();
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
        bool overwrite = ImageOverwriteBox().Checked;


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
        string output = ImageOutputBox().Text.Trim();
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

    /// <summary>
    /// Seeds the passcode boxes from the saved default (or the all-zero default) once, so a new job
    /// inherits the preference instead of a hard-coded value. User edits are never overwritten here.
    /// </summary>
    private void ApplyDefaultCredentials()
    {
        string passcode = string.IsNullOrEmpty(_settings.DebugPasscode)
            ? SonyDebugPackageCredentials.DefaultPasscode
            : _settings.DebugPasscode;
        if (txtImagePasscode.Text.Length == 0) txtImagePasscode.Text = passcode;
        if (txtDbgPasscode.Text.Length == 0) txtDbgPasscode.Text = passcode;
    }

    private string ImagePasscode() => ImagePasscodeBox().Text.Length > 0
        ? ImagePasscodeBox().Text
        : !string.IsNullOrEmpty(_settings.DebugPasscode)
            ? _settings.DebugPasscode
            : SonyDebugPackageCredentials.DefaultPasscode;

    private void RunPackageExtract()
    {
        string source = _imageSourcePath!;
        string output = ImageOutputBox().Text.Trim();
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
                "This extracts every readable file, rebuilds all FFPKG metadata beside the original, fully verifies " +
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
        cboImageSdk.Items.Add(SdkAutoLabel);
        foreach (Ps5SdkVersions.Release release in Ps5SdkVersions.Releases)
            cboImageSdk.Items.Add(release.Version);
        cboImageSdk.SelectedIndex = 0;
    }

    private static readonly (int Value, string Name)[] KrakenLevelNames =
    {
        (-4, "-4 HyperFast4"), (-3, "-3 HyperFast3"), (-2, "-2 HyperFast2"), (-1, "-1 HyperFast1"),
        (0, "0 None"), (1, "1 SuperFast"), (2, "2 VeryFast"), (3, "3 Fast"), (4, "4 Normal"),
        (5, "5 Optimal1"), (6, "6 Optimal2"), (7, "7 Optimal3"), (8, "8 Optimal4"), (9, "9 Optimal5")
    };

    private const string SdkAutoLabel = "Auto (from source)";

    private void InitializeImageBuildLists()
    {
        cboImagePkgType.Items.Clear();
        cboImagePkgType.Items.Add("APP (application)");
        cboImagePkgType.Items.Add("AC (additional content) - not supported yet");
        cboImagePkgType.SelectedIndex = 0;

        cboImageCompression.Items.Clear();
        cboImageCompression.Items.Add("Auto (Kraken where it helps)");
        cboImageCompression.Items.Add("Kraken (force)");
        cboImageCompression.Items.Add("Uncompressed (stored)");
        cboImageCompression.SelectedIndex = 0;

        cboImageKrakenLevel.Items.Clear();
        foreach (var (_, name) in KrakenLevelNames) cboImageKrakenLevel.Items.Add(name);
        cboImageKrakenLevel.SelectedIndex = IndexOfKrakenLevel(7);

        // The Builder combo itself lives in the designer; only its runtime items are populated here.
        foreach (IPackageBackend backend in BackendRegistry.All)
            cboImageBackend.Items.Add(backend.DisplayName);
        cboImageBackend.SelectedIndex = IndexOfBackend(_settings.BuildBackend);

        cboImageDrm.Items.Clear();
        cboImageDrm.Items.AddRange(DrmTypeNames);
        cboImageDrm.SelectedIndex = 0; // Upgradable

        // Conversion-target defaults (these combos previously opened unselected). Values match the
        // option-class defaults so the shown default is what actually gets built.
        cboImageCluster.SelectedIndex = 0;     // exFAT: Auto cluster size
        chkImageAmpr.Checked = true;           // exFAT: generate the AMPR index (ExfatBuildOptions default)
        cboImageBlock.SelectedIndex = 0;       // FFPKG: 32 KB block
        cboImageFragment.SelectedIndex = 0;    // FFPKG: 4 KB fragment
        cboImageDensity.SelectedIndex = 0;     // FFPKG: 256 KiB per inode
        nudImageMinFree.Value = 0;             // FFPKG: 0% reserved fragments

        txtImageTemp.Text = Path.GetTempPath()
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>Combo index of the backend id, falling back to the registry default.</summary>
    private static int IndexOfBackend(string? id)
    {
        IReadOnlyList<IPackageBackend> all = BackendRegistry.All;
        for (int i = 0; i < all.Count; i++)
            if (string.Equals(all[i].Id, id, StringComparison.OrdinalIgnoreCase)) return i;
        for (int i = 0; i < all.Count; i++)
            if (string.Equals(all[i].Id, BackendRegistry.DefaultId, StringComparison.OrdinalIgnoreCase)) return i;
        return 0;
    }

    /// <summary>The backend chosen in the Builder selector (defaults to LibProsperoPkg).</summary>
    private IPackageBackend SelectedBackend()
    {
        IReadOnlyList<IPackageBackend> all = BackendRegistry.All;
        int index = cboImageBackend.SelectedIndex;
        return index >= 0 && index < all.Count ? all[index] : BackendRegistry.Default;
    }

    private void cboImageBackend_SelectedIndexChanged(object? sender, EventArgs e)
    {
        IPackageBackend backend = SelectedBackend();
        // Remember the settings edited for the previous backend and restore this backend's own, so
        // switching builders keeps each backend's customised values instead of discarding them.
        if (_imageOptionsBackend is { } previous &&
            !string.Equals(previous, backend.Id, StringComparison.OrdinalIgnoreCase))
            _imageOptionsByBackend[previous] = CaptureImageOptions();

        _settings.BuildBackend = backend.Id;
        if (_imageOptionsByBackend.TryGetValue(backend.Id, out ImageOptionState saved))
            ApplyImageOptions(saved);
        else if (backend.Id == BackendRegistry.LppId)
            ApplyLppDefaults();
        else
            ApplyPptDefaults();
        _imageOptionsBackend = backend.Id;
        UpdateImageOptionVisibility();
    }

    /// <summary>The build-option controls' values, kept per backend so switching does not reset them.</summary>
    private readonly record struct ImageOptionState(int Compression, int KrakenLevel, decimal KrakenThreads,
        decimal PlayGo, int Sdk, int Drm, bool Deterministic, bool FakeSign, bool RightSprx);

    private readonly Dictionary<string, ImageOptionState> _imageOptionsByBackend = new(StringComparer.OrdinalIgnoreCase);
    private string? _imageOptionsBackend;

    private ImageOptionState CaptureImageOptions() => new(
        cboImageCompression.SelectedIndex, cboImageKrakenLevel.SelectedIndex, nudImageKrakenThreads.Value,
        nudImagePlayGoChunks.Value, cboImageSdk.SelectedIndex, cboImageDrm.SelectedIndex,
        chkImageDeterministic.Checked, chkImageFakeSign.Checked, chkImageRightSprx.Checked);

    private void ApplyImageOptions(ImageOptionState state)
    {
        SetSelectedIndex(cboImageCompression, state.Compression);
        SetSelectedIndex(cboImageKrakenLevel, state.KrakenLevel);
        nudImageKrakenThreads.Value = Math.Clamp(state.KrakenThreads,
            nudImageKrakenThreads.Minimum, nudImageKrakenThreads.Maximum);
        nudImagePlayGoChunks.Value = Math.Clamp(state.PlayGo,
            nudImagePlayGoChunks.Minimum, nudImagePlayGoChunks.Maximum);
        SetSelectedIndex(cboImageSdk, state.Sdk);
        SetSelectedIndex(cboImageDrm, state.Drm);
        chkImageDeterministic.Checked = state.Deterministic;
        chkImageFakeSign.Checked = state.FakeSign;
        chkImageRightSprx.Checked = state.RightSprx;
    }

    private static void SetSelectedIndex(ComboBox combo, int index) =>
        combo.SelectedIndex = index >= 0 && index < combo.Items.Count ? index : 0;

    /// <summary>Sets the build knobs to the defaults the ProsperoPkgTool engine uses.</summary>
    private void ApplyPptDefaults()
    {
        cboImageCompression.SelectedIndex = 0;                      // Auto (Kraken where it helps)
        cboImageKrakenLevel.SelectedIndex = IndexOfKrakenLevel(7);  // 7 - Optimal3
        nudImageKrakenThreads.Value = Math.Clamp(0, nudImageKrakenThreads.Minimum, nudImageKrakenThreads.Maximum);
        nudImagePlayGoChunks.Value = Math.Clamp(1, nudImagePlayGoChunks.Minimum, nudImagePlayGoChunks.Maximum);
        if (cboImageSdk.Items.Count > 0)
            cboImageSdk.SelectedIndex = 0;                          // Auto (from source)
        chkImageRightSprx.Checked = true;
        chkImageDeterministic.Checked = false;
    }

    /// <summary>
    /// Mirrors the fpkg-gui (LibProsperoPkg.Gui 0.6.4) build defaults: Kraken backend Automatic
    /// (Auto compression), Kraken level 7 (Optimal3), threads 0 (auto), PlayGo chunks 64,
    /// Deterministic build on, plus the embedded debug right.sprx. SDK stays Auto (from source).
    /// </summary>
    private void ApplyLppDefaults()
    {
        cboImageCompression.SelectedIndex = 0;                      // Automatic (Kraken where it helps)
        cboImageKrakenLevel.SelectedIndex = IndexOfKrakenLevel(7);  // 7 - Optimal3
        nudImageKrakenThreads.Value = Math.Clamp(0, nudImageKrakenThreads.Minimum, nudImageKrakenThreads.Maximum);
        nudImagePlayGoChunks.Value = Math.Clamp(64, nudImagePlayGoChunks.Minimum, nudImagePlayGoChunks.Maximum);
        if (cboImageSdk.Items.Count > 0)
            cboImageSdk.SelectedIndex = 0;                          // Auto (from source)
        chkImageRightSprx.Checked = true;
        chkImageDeterministic.Checked = true;
    }

    private void chkImageAdvancedOptions_CheckedChanged(object? sender, EventArgs e) => UpdateImageOptionVisibility();

    /// <summary>True when the advanced option knobs should be visible on the build panel.</summary>
    private bool ShowAdvancedOptions() => chkImageAdvancedOptions.Checked;

    /// <summary>DRM type choices; the index maps to the engine's <c>--drm-type</c> token.</summary>
    private static readonly string[] DrmTypeNames = ["Upgradable", "Free", "Standard", "Keep source"];

    /// <summary>Selected DRM override token (null keeps the source param.json value). Default: upgradable.</summary>
    private string? SelectedDrmToken() => cboImageDrm.SelectedIndex switch
    {
        0 => "upgradable",
        1 => "free",
        2 => "standard",
        _ => null,
    };

    private void cboImageDrm_SelectedIndexChanged(object? sender, EventArgs e) => UpdateImageOptionVisibility();

    private static int IndexOfKrakenLevel(int value)
    {
        for (int i = 0; i < KrakenLevelNames.Length; i++)
            if (KrakenLevelNames[i].Value == value) return i;
        return 0;
    }

    private ulong? SelectedSdkOverride()
    {
        int index = cboImageSdk.SelectedIndex - 1;
        return index >= 0 ? Ps5SdkVersions.ExecutableVersionAt(index) : null;
    }

    private Ps5InnerCompression SelectedCompression() => cboImageCompression.SelectedIndex switch
    {
        1 => Ps5InnerCompression.Kraken,
        2 => Ps5InnerCompression.Stored,
        _ => Ps5InnerCompression.Auto
    };

    private int SelectedKrakenLevel() =>
        cboImageKrakenLevel.SelectedIndex >= 0 ? KrakenLevelNames[cboImageKrakenLevel.SelectedIndex].Value : 7;

    private bool ImagePackageTypeUnsupported() => cboImagePkgType.SelectedIndex == 1;

    private void cboImageCompression_SelectedIndexChanged(object? sender, EventArgs e) =>
        UpdateImageOptionVisibility();

    private void cboImagePkgType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (ImagePackageTypeUnsupported())
            lblImageStatus.Text = "AC (additional content) packages are not supported yet; choose APP.";
        UpdateImageOptionVisibility();
    }

    private void RunImageBuildPackage()
    {
        string source = _imageSourcePath!;
        string output = ImageOutputBox().Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an output .pkg file.", "Image Tools");
            return;
        }
        string contentId = _imageContentId.Trim();
        if (contentId.Length == 0)
        {
            AppDialog.ShowWarning(
                "The source does not contain a content ID, so a package cannot be built from it.",
                "Build package");
            return;
        }
        string passcode = ImagePasscode();
        bool overwrite = ImageOverwriteBox().Checked;
        ulong? sdkVersionOverride = SelectedSdkOverride();
        string? tempDirectory = string.IsNullOrWhiteSpace(txtImageTemp.Text) ? null : txtImageTemp.Text.Trim();
        if (ImagePackageTypeUnsupported())
        {
            AppDialog.ShowWarning("AC (additional content) packages are not supported yet. Choose APP.", "Build package");
            return;
        }
        var settings = new ImageBuildSettings(SelectedCompression(), SelectedKrakenLevel(),
            (int)nudImageKrakenThreads.Value, (int)nudImagePlayGoChunks.Value,
            SelectedDrmToken(),
            chkImageDeterministic.Checked,
            chkImageFakeSign.Checked,
            chkImageRightSprx.Checked);

        // Best-effort free-space preflight before a long build; the engine re-checks exactly and
        // throws ProsperoInsufficientSpaceException, which the task failure path surfaces as a dialog.
        Ps5DiskSpaceCheck space = Ps5DiskSpace.Check(EstimateBuildPayload(source), output, tempDirectory);
        if (space.Status == Ps5DiskSpaceStatus.Insufficient)
        {
            AppDialog.ShowError("Not enough free disk space to build this package.\n\n" + space.Message +
                "\n\nFree space, or choose a different workspace or output volume.", "Build package");
            return;
        }
        if (space.Status == Ps5DiskSpaceStatus.NearLimit)
            AppDialog.ShowWarning("Low free disk space for this build.\n\n" + space.Message, "Build package");

        // Capture the backend now, not when the queued delegate runs: the worker must not read the
        // Builder combo (a UI control), and changing it after queueing must not change this job.
        IPackageBackend backend = SelectedBackend();
        lblImageStatus.Text = "Queued: package build. See the Tasks tab.";
        EnqueueTask(PackageTaskTypes.ImageBuildPackage, $"Build package from {Path.GetFileName(source)}",
            (progress, token) => BuildPackageFromSourceAsync(source, output, contentId, passcode, overwrite,
                sdkVersionOverride, tempDirectory, settings, backend, progress, token),
            sourcePath: source, outputPath: output,
            operation: "Build package", sourceFormat: ImageFormatLabel(source), targetFormat: "FPKG",
            targetQualifier: backend.DisplayName,
            stagePlan: PackageTaskPlans.BuildPackageFor(source,
                backend.Id == BackendRegistry.LppId && !Directory.Exists(source)),
            payload: Payload(("source", source), ("output", output), ("contentId", contentId),
                ("backend", backend.Id),
                ("passcode", passcode), ("overwrite", overwrite.ToString()),
                ("sdk", sdkVersionOverride?.ToString("X16")), ("temp", tempDirectory),
                ("compression", settings.Compression.ToString()), ("krakenLevel", settings.KrakenLevel.ToString()),
                ("krakenThreads", settings.KrakenThreads.ToString()), ("playgo", settings.PlayGoChunks.ToString()),
                ("drm", settings.DrmType),
                ("deterministic", settings.Deterministic.ToString()),
                ("fakeSign", settings.FakeSignModules.ToString()),
                ("rightSprx", settings.InjectRightSprx.ToString())),
            onFinished: task => lblImageStatus.Text = task.Status == PackageTaskStatus.Completed
                ? $"Built package {Path.GetFileName(output)}."
                : $"Package build {StatusText(task.Status).ToLowerInvariant()}.");
    }

    private static long EstimateBuildPayload(string source)
    {
        try
        {
            if (Directory.Exists(source))
            {
                long total = 0;
                foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                    total += new FileInfo(file).Length;
                return total;
            }
            if (File.Exists(source)) return new FileInfo(source).Length;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
        return 0;
    }

    private readonly record struct ImageBuildSettings(Ps5InnerCompression Compression, int KrakenLevel,
        int KrakenThreads, int PlayGoChunks, string? DrmType, bool Deterministic, bool FakeSignModules,
        bool InjectRightSprx);

    /// <summary>A fixed 16-byte seed derived from the content id + passcode for reproducible builds.</summary>
    private static byte[] DeterministicSeed(string contentId, string passcode) =>
        System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(contentId + "\0" + passcode))[..16];

    private static async Task BuildPackageFromSourceAsync(string source, string output, string contentId,
        string passcode, bool overwrite, ulong? sdkVersionOverride, string? tempDirectory,
        ImageBuildSettings settings, IPackageBackend backend, IProgress<PackageTaskProgress> progress,
        CancellationToken token)
    {
        if (!overwrite && File.Exists(output))
            throw new IOException($"The output file already exists: {output}");
        // Build inside a private per-job directory, then move the finished package into place, so a
        // failed build cannot leave a half-written .pkg and neither builder's generated output name
        // can collide with the user's files.
        string outputDirectory = Path.GetDirectoryName(Path.GetFullPath(output)) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(outputDirectory);
        string jobDirectory = Path.Combine(outputDirectory, ".ps5pkgtool-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(jobDirectory);
        string partial = Path.Combine(jobDirectory, Path.GetFileName(output) + ".partial");
        var options = new SonyDebugPackageBuildOptions
        {
            ContentId = contentId,
            Passcode = passcode,
            SdkVersionOverride = sdkVersionOverride,
            TempDirectory = tempDirectory,
            Log = Logger.Info,
            Compression = settings.Compression,
            KrakenLevel = settings.KrakenLevel,
            KrakenThreads = settings.KrakenThreads,
            PlayGoChunkCount = settings.PlayGoChunks,
            DrmTypeOverride = settings.DrmType,
            Seed = settings.Deterministic ? DeterministicSeed(contentId, passcode) : null,
            FakeSignModules = settings.FakeSignModules,
            InjectRightSprx = settings.InjectRightSprx
        };
        var bridge = new Progress<SonyDebugPackageProgress>(value =>
            progress.Report(new PackageTaskProgress(value.Stage, 0, 0, value.CompletedBytes, value.TotalBytes,
                0, 0, value.CurrentPath)));
        bool isDirectory = Directory.Exists(source);
        // LibProsperoPkg builds only from a folder, so an image source is extracted to a staging tree
        // first when that backend is selected. ProsperoPkgTool reads the image directly (no extract).
        bool stageFromImage = !isDirectory && backend.Id == BackendRegistry.LppId;
        IPackageBackend effective = isDirectory || stageFromImage ? backend : BackendRegistry.Get(BackendRegistry.PptId);
        if (effective.Id == BackendRegistry.LppId && !settings.InjectRightSprx)
        {
            Logger.Info("LibProsperoPkg 1.2.0 has no inject-right.sprx toggle; the source right.sprx is kept as-is.");
        }

        string? staging = null;
        try
        {
            string buildSource = source;
            if (stageFromImage)
            {
                long rawBytes = VolumeDebugPackageBuilder.EstimatePayloadBytes(source);
                Ps5DiskSpaceCheck space = Ps5DiskSpace.CheckStagedImage(rawBytes, rawBytes, output, tempDirectory);
                if (space.Status == Ps5DiskSpaceStatus.Insufficient)
                    throw new IOException(
                        "Not enough free disk space to extract and build this image package.\n\n" + space.Message);
                if (space.Status == Ps5DiskSpaceStatus.NearLimit)
                    Logger.Info("Low free disk space for this image build. " + space.Message);

                staging = Path.Combine(
                    string.IsNullOrWhiteSpace(tempDirectory) ? Path.GetTempPath() : tempDirectory,
                    "PS5PKGTool-image-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(staging);
                Logger.Info("LibProsperoPkg: extracting " + Path.GetFileName(source) +
                    " to a staging workspace before the build...");
                progress.Report(new PackageTaskProgress("Extract image", 0, 0, 0, rawBytes, 0, 0, Path.GetFileName(source)));
                await ExtractImageToAsync(source, Ps5ImageFormatProbe.Detect(source), staging, progress, token)
                    .ConfigureAwait(false);
                buildSource = staging;
            }

            async Task BuildAsync()
            {
                if (isDirectory || staging is not null)
                    await effective.BuildFromDirectoryAsync(buildSource, partial, options, bridge, token)
                        .ConfigureAwait(false);
                else
                    await effective.BuildFromImageAsync(source, partial, options, bridge, token)
                        .ConfigureAwait(false);
            }

            try
            {
                await BuildAsync().ConfigureAwait(false);
            }
            catch (BackendNotSupportedException ex) when (effective.Id == BackendRegistry.LppId)
            {
                // LibProsperoPkg can still fail to lay out an input; fall back to ProsperoPkgTool
                // automatically instead of making the user switch builders by hand.
                Logger.Info($"Builder {effective.DisplayName} could not lay out this title ({ex.Message}) " +
                    "— retrying with ProsperoPkgTool.");
                effective = BackendRegistry.Get(BackendRegistry.PptId);
                TryDeleteFile(partial);
                await BuildAsync().ConfigureAwait(false);
            }
            // Verify the finished package with the one canonical reader before publishing, so a
            // package the application cannot read back is never presented as a successful build.
            VerifyBuiltPackage(partial, passcode, effective);
            // Honor the requested overwrite policy at publication time too: a file that appeared
            // while the build ran must not be replaced unless the user asked to overwrite.
            token.ThrowIfCancellationRequested();
            File.Move(partial, output, overwrite);
        }
        catch
        {
            TryDeleteFile(partial);
            throw;
        }
        finally
        {
            TryDeleteDirectory(jobDirectory);
            if (staging is not null)
            {
                try { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    /// <summary>
    /// Opens the freshly built package with the canonical engine reader using the job passcode and
    /// requires a non-empty inner filesystem containing <c>eboot.bin</c>. This is the same reader
    /// used to browse packages, so it enforces one acceptance criterion for both builders.
    /// </summary>
    private static void VerifyBuiltPackage(string packagePath, string passcode, IPackageBackend backend)
    {
        try
        {
            PackageReaderVerificationResult result = PackageReaderVerification.Inspect(packagePath, passcode);
            Logger.Info($"Verified with the canonical reader (built by {backend.DisplayName}): " +
                $"{result.FileCount:N0} file(s), eboot.bin {(result.HasEboot ? "present" : "missing")}.");
            if (result.FileCount == 0)
                throw new InvalidDataException("the reconstructed filesystem is empty");
            if (!result.HasEboot)
                throw new InvalidDataException("the expected /uroot/eboot.bin is missing");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidDataException(
                "The built package failed canonical-reader verification: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Reads the content ID from the selected source's param once per source; the build tab no longer
    /// asks for it (it is not user-editable), so an empty result means the source cannot be built.
    /// </summary>
    private void SuggestImageContentId()
    {
        if (_imageSourcePath is null)
        {
            _imageContentId = string.Empty;
            _imageContentIdSource = null;
            return;
        }
        if (string.Equals(_imageContentIdSource, _imageSourcePath, StringComparison.Ordinal)) return;
        ImageParamFields fields = TryReadParamFields(_imageSourcePath, _imageSourceFormat);
        _imageContentId = fields.ContentId;
        _imageContentIdSource = _imageSourcePath;
    }

    private readonly record struct ImageParamFields(string ContentId, string TitleId, string ContentVersion,
        string TitleName);

    private static ImageParamFields TryReadParamFields(string source, Ps5ImageFormat format)
    {
        byte[]? bytes = TryReadParamBytes(source, format);
        if (bytes is null || bytes.Length == 0) return default;
        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(bytes);
            System.Text.Json.JsonElement root = document.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object) return default;
            return new ImageParamFields(ReadString(root, "contentId"), ReadString(root, "titleId"),
                ReadString(root, "contentVersion"), ReadString(root, "titleName"));
        }
        catch (System.Text.Json.JsonException)
        {
            return default;
        }
    }

    private static string ReadString(System.Text.Json.JsonElement root, string name) =>
        root.TryGetProperty(name, out System.Text.Json.JsonElement element) &&
        element.ValueKind == System.Text.Json.JsonValueKind.String
            ? element.GetString() ?? string.Empty
            : string.Empty;

    private static byte[]? TryReadParamBytes(string source, Ps5ImageFormat format)
    {
        try
        {
            if (Directory.Exists(source))
            {
                string path = Path.Combine(source, "sce_sys", "param.json");
                if (File.Exists(path)) return File.ReadAllBytes(path);
                return null;
            }
            switch (format)
            {
                case Ps5ImageFormat.Exfat:
                    using (var volume = new ExfatVolume(File.OpenRead(source)))
                        return volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                case Ps5ImageFormat.Ufs2:
                    using (var volume = new Ufs2Volume(source))
                        return volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                case Ps5ImageFormat.Pfs:
                    using (var volume = FfpfscVolume.Open(source))
                        return volume.ReadAllBytes("sce_sys/param.json", 4 * 1024 * 1024);
                default:
                    return null;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   ArgumentException or NotSupportedException)
        {
            return null;
        }
    }
}
