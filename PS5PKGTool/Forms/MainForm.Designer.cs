#nullable enable

namespace PS5PKGTool.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkMenuStrip menuMain = null!;
    private ToolStripMenuItem menuFile = null!;
    private ToolStripMenuItem menuAddFolder = null!;
    private ToolStripMenuItem menuOpenDump = null!;
    private ToolStripMenuItem menuOpenPackage = null!;
    private ToolStripMenuItem menuRefresh = null!;
    private ToolStripMenuItem menuSaveManifest = null!;
        private ToolStripMenuItem menuEmptyList = null!;
        private ToolStripMenuItem menuRemoveMissing = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuSeparator = null!;
    private ToolStripMenuItem menuSettings = null!;
    private ToolStripMenuItem menuExit = null!;
    private ToolStripMenuItem menuHelp = null!;
    private ToolStripMenuItem menuAbout = null!;
    private ToolStripMenuItem menuRecent = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuHelpSeparator = null!;
    private ToolStripMenuItem menuHelpCheckUpdate = null!;
        private ToolStripMenuItem menuHelpKofi = null!;
        private ToolStripMenuItem menuHelpPayPal = null!;
    private DarkUI.Controls.DarkContextMenu contextLibrary = null!;
    private ToolStripMenuItem menuLibraryReveal = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuLibrarySeparator1 = null!;
    private ToolStripMenuItem menuLibraryCopy = null!;
    private ToolStripMenuItem menuLibraryCopyTitle = null!;
    private ToolStripMenuItem menuLibraryCopyTitleId = null!;
    private ToolStripMenuItem menuLibraryCopyContentId = null!;
    private ToolStripMenuItem menuLibraryCopyPath = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuLibrarySeparator2 = null!;
    private ToolStripMenuItem menuLibraryRename = null!;
    private ToolStripMenuItem menuLibraryRenameAll = null!;
    private ToolStripMenuItem menuLibraryRenameByPriority = null!;
    private ToolStripMenuItem menuLibraryGroupBy = null!;
    private ToolStripMenuItem menuLibraryGroupNone = null!;
    private ToolStripMenuItem menuLibraryGroupFamily = null!;
    private ToolStripMenuItem menuLibraryGroupTitleId = null!;
    private ToolStripMenuItem menuLibraryGroupCategory = null!;
    private ToolStripMenuItem menuLibraryGroupRegion = null!;
    private ToolStripMenuItem menuLibraryGroupSource = null!;
    private ToolStripMenuItem menuLibraryGroupFirmware = null!;
    private ToolStripMenuItem menuLibraryDuplicates = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuLibrarySeparator3 = null!;
    private ToolStripMenuItem menuLibraryExport = null!;
    private ToolStripMenuItem menuLibraryCopyFileName = null!;
    private ToolStripMenuItem menuLibrarySaveArtwork = null!;
    private ToolStripMenuItem menuLibraryMove = null!;
    private ToolStripMenuItem menuLibraryMoveTitle = null!;
    private ToolStripMenuItem menuLibraryMoveTitleId = null!;
    private ToolStripMenuItem menuLibraryMoveCategory = null!;
    private ToolStripMenuItem menuLibraryMoveRegion = null!;
    private ToolStripMenuItem menuLibraryMoveSource = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuLibraryMoveSeparator = null!;
    private ToolStripMenuItem menuLibraryMoveSingle = null!;
    private ToolStripMenuItem menuLibraryDelete = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuLibrarySeparator5 = null!;
    private ToolStripMenuItem menuLibraryGroupExport = null!;
    private ToolStripMenuItem menuLibraryGroupArtwork = null!;
    private DarkUI.Controls.DarkSearchBox searchLibrary = null!;
    private DarkUI.Controls.DarkLabel lblFilterCategory = null!;
    private DarkUI.Controls.DarkCheckedComboBox cboFilterCategory = null!;
    private DarkUI.Controls.DarkLabel lblFilterRegion = null!;
    private DarkUI.Controls.DarkCheckedComboBox cboFilterRegion = null!;
    private DarkUI.Controls.DarkLabel lblFilterFormat = null!;
    private DarkUI.Controls.DarkCheckedComboBox cboFilterFormat = null!;
    private DarkUI.Controls.DarkLabel lblFilterGroup = null!;
    private DarkUI.Controls.DarkComboBox cboFilterGroup = null!;
    private DarkUI.Controls.DarkButton btnFilterClear = null!;
    private DarkUI.Controls.DarkChipsPanel chipsFilter = null!;
    private DarkUI.Controls.DarkLabel lblFilterPreset = null!;
    private DarkUI.Controls.DarkComboBox cboFilterPreset = null!;
    private DarkUI.Controls.DarkLabel lblFilterEmpty = null!;
    private DarkUI.Controls.DarkSplitContainer splitMain = null!;
    private DarkUI.Controls.DarkSplitPane splitMainPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitMainPane2 = null!;
    private DarkUI.Controls.DarkDataGridView gridLibrary = null!;
    private DarkUI.Controls.DarkTabControl tabsWorkspace = null!;
    private DarkUI.Controls.DarkTabPage tabWorkspaceGeneral = null!;
    private DarkUI.Controls.DarkTabPage tabWorkspaceTools = null!;
        private DarkUI.Controls.DarkTabPage tabTasks = null!;
        private DarkUI.Controls.DarkTabPage tabLog = null!;
        private DarkUI.Controls.DarkLabel lblLogLevel = null!;
        private DarkUI.Controls.DarkComboBox cboLogLevel = null!;
        private DarkUI.Controls.DarkCheckBox chkLogAutoScroll = null!;
        private DarkUI.Controls.DarkButton btnLogClear = null!;
        private DarkUI.Controls.DarkButton btnLogOpenFolder = null!;
        private DarkUI.Controls.DarkRichTextBox txtLogView = null!;
    private DarkUI.Controls.DarkTableLayoutPanel tasksLayout = null!;
    private DarkUI.Controls.DarkCheckBox chkTaskAutoStart = null!;
    private DarkUI.Controls.DarkButton btnTaskStart = null!;
    private DarkUI.Controls.DarkButton btnTaskCancel = null!;
    private DarkUI.Controls.DarkButton btnTaskRetry = null!;
    private DarkUI.Controls.DarkButton btnTaskRemove = null!;
    private DarkUI.Controls.DarkButton btnTaskOpen = null!;
    private DarkUI.Controls.DarkButton btnTaskClear = null!;
    private DarkUI.Controls.DarkLabel lblTaskSummary = null!;
    private DarkUI.Controls.DarkCheckBox chkTaskFollow = null!;
    private DarkUI.Controls.DarkComboBox cboTaskFilter = null!;
    private DarkUI.Controls.DarkSearchBox searchTasks = null!;
    private DarkUI.Controls.DarkButton btnTaskToggleDetails = null!;
    private DarkUI.Controls.DarkLabel lblTaskGroup = null!;
    private DarkUI.Controls.DarkComboBox cboTaskGroup = null!;
    private DarkUI.Controls.DarkSplitContainer splitTasks = null!;
    private DarkUI.Controls.DarkSplitPane splitTasksPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitTasksPane2 = null!;
    private DarkUI.Controls.DarkSectionPanel sectionTasksList = null!;
    private DarkUI.Controls.DarkDataGridView gridTasks = null!;
    private DataGridViewTextBoxColumn colTaskName = null!;
    private DataGridViewTextBoxColumn colTaskOperation = null!;
    private DataGridViewTextBoxColumn colTaskRoute = null!;
    private DataGridViewTextBoxColumn colTaskStatus = null!;
    private DataGridViewTextBoxColumn colTaskStage = null!;
    private DataGridViewTextBoxColumn colTaskProgress = null!;
    private DataGridViewTextBoxColumn colTaskElapsed = null!;
    private DarkUI.Controls.DarkSectionPanel sectionTaskDetails = null!;
    private DarkUI.Controls.DarkTableLayoutPanel taskDetailLayout = null!;
    private DarkUI.Controls.DarkLabel lblTaskStage = null!;
    private DarkUI.Controls.DarkLabel lblTaskCurrentCaption = null!;
    private DarkUI.Controls.DarkLabel lblTaskOverallCaption = null!;
    private DarkUI.Controls.DarkProgressBar barTaskCurrent = null!;
    private DarkUI.Controls.DarkProgressBar barTaskOverall = null!;
    private DarkUI.Controls.DarkLabel lblTaskMessage = null!;
    private DarkUI.Controls.DarkLabel lblTaskMeta = null!;
    private DarkUI.Controls.DarkLabel lblTaskResult = null!;
    private DarkUI.Controls.DarkButton btnTaskDiagnostic = null!;
    private DarkUI.Controls.DarkContextMenu contextTasks = null!;
    private ToolStripMenuItem menuTaskStart = null!;
    private ToolStripMenuItem menuTaskCancel = null!;
    private ToolStripMenuItem menuTaskRetry = null!;
    private ToolStripMenuItem menuTaskRemove = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuTaskSeparator1 = null!;
    private ToolStripMenuItem menuTaskOpen = null!;
    private ToolStripMenuItem menuTaskShowSource = null!;
    private ToolStripMenuItem menuTaskReport = null!;
    private ToolStripMenuItem menuTaskClear = null!;
    private DarkUI.Controls.DarkTabControl tabsDetails = null!;
    private DarkUI.Controls.DarkTabPage tabOverview = null!;
    private DarkUI.Controls.DarkButton btnOverviewCopyAll = null!;
    private DarkUI.Controls.DarkButton btnOverviewCopySelected = null!;
    private DarkUI.Controls.DarkDataGridView gridOverview = null!;
    private DarkUI.Controls.DarkTabPage tabArtwork = null!;
    private DarkUI.Controls.DarkButton btnArtworkSaveAll = null!;
    private DarkUI.Controls.DarkContextMenu contextArtwork = null!;
    private ToolStripMenuItem menuArtworkSaveThis = null!;
    private ToolStripMenuItem menuArtworkSaveAll = null!;
    private DarkUI.Controls.DarkSplitContainer splitArtwork = null!;
    private DarkUI.Controls.DarkSplitPane splitArtworkPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitArtworkPane2 = null!;
    private DarkUI.Controls.DarkSectionPanel sectionIcon = null!;
    private PictureBox pictureIcon = null!;
    private DarkUI.Controls.DarkSectionPanel sectionBackground = null!;
    private DarkUI.Controls.DarkTabControl tabsBackgrounds = null!;
    private DarkUI.Controls.DarkTabPage tabPic0 = null!;
    private PictureBox pictureBackground0 = null!;
    private DarkUI.Controls.DarkTabPage tabPic1 = null!;
    private PictureBox pictureBackground1 = null!;
    private DarkUI.Controls.DarkTabPage tabPic2 = null!;
    private PictureBox pictureBackground2 = null!;
    private DarkUI.Controls.DarkTabPage tabTrophies = null!;
    private DarkUI.Controls.DarkLabel lblTrophySummary = null!;
    private DarkUI.Controls.DarkDataGridView gridTrophies = null!;
    private DarkUI.Controls.DarkButton btnTrophySaveIcons = null!;
    private DarkUI.Controls.DarkButton btnTrophyExportCsv = null!;
    private DarkUI.Controls.DarkCheckBox chkTrophyPlatinum = null!;
    private DarkUI.Controls.DarkCheckBox chkTrophyGold = null!;
    private DarkUI.Controls.DarkCheckBox chkTrophySilver = null!;
    private DarkUI.Controls.DarkCheckBox chkTrophyBronze = null!;
    private DarkUI.Controls.DarkCheckBox chkTrophyShowHidden = null!;
    private DarkUI.Controls.DarkSearchBox searchTrophy = null!;
    private DarkUI.Controls.DarkContextMenu contextTrophies = null!;
    private ToolStripMenuItem menuTrophySaveIcon = null!;
    private ToolStripMenuItem menuTrophySaveAllIcons = null!;
    private ToolStripMenuItem menuTrophyExportCsv = null!;
    private SaveFileDialog trophyCsvSaveDialog = null!;
    private DarkUI.Controls.DarkTabPage tabActivities = null!;
    private DarkUI.Controls.DarkLabel lblActivitiesSummary = null!;
    private DarkUI.Controls.DarkButton btnUdsCopyAll = null!;
    private DarkUI.Controls.DarkButton btnUdsCopySelected = null!;
    private DarkUI.Controls.DarkSearchBox searchUds = null!;
    private DarkUI.Controls.DarkTabControl tabsUds = null!;
    private DarkUI.Controls.DarkTabPage tabUdsEvents = null!;
    private DarkUI.Controls.DarkSplitContainer splitUdsEvents = null!;
    private DarkUI.Controls.DarkSplitPane splitUdsEventsPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitUdsEventsPane2 = null!;
    private DarkUI.Controls.DarkDataGridView gridUdsEvents = null!;
    private DarkUI.Controls.DarkDataGridView gridUdsEventProperties = null!;
    private DarkUI.Controls.DarkTabPage tabUdsStats = null!;
    private DarkUI.Controls.DarkDataGridView gridUdsStats = null!;
    private DarkUI.Controls.DarkTabPage tabUdsEnums = null!;
    private DarkUI.Controls.DarkDataGridView gridUdsEnums = null!;
    private DarkUI.Controls.DarkTabPage tabUdsRules = null!;
    private DarkUI.Controls.DarkDataGridView gridUdsRules = null!;
    private DarkUI.Controls.DarkTabPage tabFiles = null!;
    private DarkUI.Controls.DarkLabel lblFilesSummary = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFileBrowser = null!;
    private DarkUI.Controls.DarkSplitContainer splitFileBrowser = null!;
    private DarkUI.Controls.DarkSplitPane splitFileBrowserPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitFileBrowserPane2 = null!;
    private DarkUI.Controls.DarkTreeView treeFiles = null!;
    private DarkUI.Controls.DarkSplitContainer splitFileContentPreview = null!;
    private DarkUI.Controls.DarkSplitPane splitFileContentPreviewPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitFileContentPreviewPane2 = null!;
    private DarkUI.Controls.DarkPanel fileListPanel = null!;
    private DarkUI.Controls.DarkSearchBox searchFileFilter = null!;
    private DarkUI.Controls.DarkListView listFiles = null!;
    private ColumnHeader colFileName = null!;
    private ColumnHeader colFileType = null!;
    private ColumnHeader colFilePath = null!;
    private ColumnHeader colFileSize = null!;
    private ImageList imageListFiles = null!;
    private DarkUI.Controls.DarkContextMenu contextTreeFiles = null!;
    private ToolStripMenuItem menuTreeExpand = null!;
    private ToolStripMenuItem menuTreeCollapse = null!;
    private ToolStripMenuItem menuTreeExpandAll = null!;
    private ToolStripMenuItem menuTreeCollapseAll = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuTreeSeparator = null!;
    private ToolStripMenuItem menuTreeCopyPath = null!;
    private ToolStripMenuItem menuTreeExtract = null!;
    private ToolStripMenuItem menuFileCopyPath = null!;
    private ToolStripMenuItem menuFileCopyName = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuFileCopySeparator = null!;
    private DarkUI.Controls.DarkContextMenu contextFiles = null!;
    private ToolStripMenuItem menuFileOpenContained = null!;
    private ToolStripMenuItem menuFileExtractContained = null!;
    private ToolStripMenuItem menuFileExtractSelected = null!;
    private DarkUI.Controls.DarkToolStripSeparator menuFileContainerSeparator = null!;
    private ToolStripMenuItem menuFileRevealContainer = null!;
    private DarkUI.Controls.DarkButton btnFileExtractAll = null!;
    private DarkUI.Controls.DarkButton btnFileCancel = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFileViewer = null!;
    private DarkUI.Controls.DarkLabel lblFileViewerInfo = null!;
    private PictureBox pictureFileViewer = null!;
    private DarkUI.Controls.DarkRichTextBox txtFileViewer = null!;
    private DarkUI.Controls.DarkRichTextBox txtHexViewer = null!;
    private System.Windows.Forms.Integration.ElementHost mediaFileHost = null!;
    private System.Windows.Controls.MediaElement mediaFileViewer = null!;
    private DarkUI.Controls.DarkButton btnMediaLoad = null!;
    private DarkUI.Controls.DarkButton btnMediaPlay = null!;
    private DarkUI.Controls.DarkButton btnMediaPause = null!;
    private DarkUI.Controls.DarkButton btnMediaStop = null!;
    private DarkUI.Controls.DarkButton btnHexPrevious = null!;
    private DarkUI.Controls.DarkButton btnHexNext = null!;
    private DarkUI.Controls.DarkLabel lblHexPage = null!;
    private DarkUI.Controls.DarkTabPage tabExecutable = null!;
    private DarkUI.Controls.DarkLabel lblExecutableSummary = null!;
    private DarkUI.Controls.DarkDataGridView gridModules = null!;
    private DarkUI.Controls.DarkButton btnExecExtract = null!;
    private DarkUI.Controls.DarkButton btnExecHash = null!;
    private DarkUI.Controls.DarkButton btnExecCopyAll = null!;
    private DarkUI.Controls.DarkButton btnExecCopySelected = null!;
    private DarkUI.Controls.DarkSearchBox searchExecutable = null!;
    private DarkUI.Controls.DarkTabControl tabsExecutable = null!;
    private DarkUI.Controls.DarkTabPage tabExecModules = null!;
    private DarkUI.Controls.DarkTabPage tabExecElf = null!;
    private DarkUI.Controls.DarkSplitContainer splitExecElf = null!;
    private DarkUI.Controls.DarkSplitPane splitExecElfPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitExecElfPane2 = null!;
    private DarkUI.Controls.DarkDataGridView gridElfPrograms = null!;
    private DarkUI.Controls.DarkDataGridView gridElfSections = null!;
    private DarkUI.Controls.DarkTabPage tabExecSelf = null!;
    private DarkUI.Controls.DarkSplitContainer splitExecSelf = null!;
    private DarkUI.Controls.DarkSplitPane splitExecSelfPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitExecSelfPane2 = null!;
    private DarkUI.Controls.DarkDataGridView gridSelfHeader = null!;
    private DarkUI.Controls.DarkDataGridView gridSelfSegments = null!;
    private DarkUI.Controls.DarkTabPage tabRaw = null!;
    private DarkUI.Controls.DarkButton btnCopyRawJson = null!;
    private DarkUI.Controls.DarkButton btnRawFormatted = null!;
    private DarkUI.Controls.DarkButton btnRawOriginal = null!;
    private DarkUI.Controls.DarkRichTextBox txtRawMetadata = null!;
    private DarkUI.Controls.DarkTabPage tabPackage = null!;
    private DarkUI.Controls.DarkTabControl tabsPackage = null!;
    private DarkUI.Controls.DarkTabPage tabMetadata = null!;
    private DarkUI.Controls.DarkTabControl tabsMetadata = null!;
    private DarkUI.Controls.DarkTabPage tabPkgContainer = null!;
    private DarkUI.Controls.DarkDataGridView gridPkgHeader = null!;
    private DarkUI.Controls.DarkTabPage tabPkgSegments = null!;
    private DarkUI.Controls.DarkDataGridView gridPkgSegments = null!;
    private DarkUI.Controls.DarkTabPage tabPkgEntries = null!;
    private DarkUI.Controls.DarkDataGridView gridPkgEntries = null!;
    private DarkUI.Controls.DarkTabPage tabPkgSfo = null!;
    private DarkUI.Controls.DarkDataGridView gridParamSfo = null!;
    private DarkUI.Controls.DarkTabPage tabPkgKeystone = null!;
    private DarkUI.Controls.DarkDataGridView gridKeystone = null!;
    private DarkUI.Controls.DarkTabPage tabPkgSi = null!;
    private DarkUI.Controls.DarkDataGridView gridSi = null!;
    private DarkUI.Controls.DarkTabPage tabPkgPlayGo = null!;
    private DarkUI.Controls.DarkLabel lblPlayGoSummary = null!;
    private DarkUI.Controls.DarkTabControl tabsPlayGo = null!;
    private DarkUI.Controls.DarkTabPage tabPlayGoChunks = null!;
    private DarkUI.Controls.DarkDataGridView gridPlayGoChunks = null!;
    private DarkUI.Controls.DarkTabPage tabPlayGoScenarios = null!;
    private DarkUI.Controls.DarkDataGridView gridPlayGoScenarios = null!;
    private DarkUI.Controls.DarkTabPage tabPlayGoFiles = null!;
    private DarkUI.Controls.DarkDataGridView gridPlayGoFiles = null!;
    private DarkUI.Controls.DarkTableLayoutPanel toolsLayout = null!;
    private DarkUI.Controls.DarkSectionPanel sectionJob = null!;
    private DarkUI.Controls.DarkTabControl tabsImageTargets = null!;
    private DarkUI.Controls.DarkTabPage tabTargetExfat = null!;
    private DarkUI.Controls.DarkTabPage tabTargetFfpkg = null!;
    private DarkUI.Controls.DarkTabPage tabTargetFfpfsc = null!;
    private DarkUI.Controls.DarkTabPage tabTargetDebug = null!;
    private DarkUI.Controls.DarkTabPage tabTargetOptions = null!;
    private DarkUI.Controls.DarkFooterBar toolsFooter = null!;
    private DarkUI.Controls.DarkLabel lblImageSource = null!;
    private DarkUI.Controls.DarkLabel lblImageSourcePath = null!;
    private DarkUI.Controls.DarkLabel lblImageFormat = null!;
    private DarkUI.Controls.DarkLabel lblImageAction = null!;
    private DarkUI.Controls.DarkComboBox cboImageAction = null!;
    private DarkUI.Controls.DarkLabel lblImageOutput = null!;
    private DarkUI.Controls.DarkTextBox txtImageOutput = null!;
    private DarkUI.Controls.DarkButton btnImageBrowseOutput = null!;
    private DarkUI.Controls.DarkCheckBox chkImageOverwrite = null!;
    private DarkUI.Controls.DarkLabel lblOutExfat = null!;
    private DarkUI.Controls.DarkTextBox txtOutExfat = null!;
    private DarkUI.Controls.DarkButton btnOutExfat = null!;
    private DarkUI.Controls.DarkCheckBox chkOutExfat = null!;
    private DarkUI.Controls.DarkLabel lblOutFfpkg = null!;
    private DarkUI.Controls.DarkTextBox txtOutFfpkg = null!;
    private DarkUI.Controls.DarkButton btnOutFfpkg = null!;
    private DarkUI.Controls.DarkCheckBox chkOutFfpkg = null!;
    private DarkUI.Controls.DarkLabel lblOutFfpfsc = null!;
    private DarkUI.Controls.DarkTextBox txtOutFfpfsc = null!;
    private DarkUI.Controls.DarkButton btnOutFfpfsc = null!;
    private DarkUI.Controls.DarkCheckBox chkOutFfpfsc = null!;
    private DarkUI.Controls.DarkLabel lblOutDebug = null!;
    private DarkUI.Controls.DarkTextBox txtOutDebug = null!;
    private DarkUI.Controls.DarkButton btnOutDebug = null!;
    private DarkUI.Controls.DarkCheckBox chkOutDebug = null!;
    private DarkUI.Controls.DarkLabel lblDbgPasscode = null!;
    private DarkUI.Controls.DarkTextBox txtDbgPasscode = null!;
    private DarkUI.Controls.DarkLabel lblImageCluster = null!;
    private DarkUI.Controls.DarkComboBox cboImageCluster = null!;
    private DarkUI.Controls.DarkLabel lblImageLevel = null!;
    private DarkUI.Controls.DarkNumericUpDown nudImageLevel = null!;
    private DarkUI.Controls.DarkLabel lblImageGain = null!;
    private DarkUI.Controls.DarkNumericUpDown nudImageGain = null!;
    private DarkUI.Controls.DarkCheckBox chkImageAmpr = null!;
    private DarkUI.Controls.DarkButton btnImageRun = null!;
    private DarkUI.Controls.DarkButton btnImageCancel = null!;
    private DarkUI.Controls.DarkLabel lblImageStatus = null!;
    private DarkUI.Controls.DarkLabel lblImageBlock = null!;
    private DarkUI.Controls.DarkComboBox cboImageBlock = null!;
    private DarkUI.Controls.DarkLabel lblImageFragment = null!;
    private DarkUI.Controls.DarkComboBox cboImageFragment = null!;
    private DarkUI.Controls.DarkLabel lblImageDensity = null!;
    private DarkUI.Controls.DarkComboBox cboImageDensity = null!;
    private DarkUI.Controls.DarkLabel lblImageMinFree = null!;
    private DarkUI.Controls.DarkNumericUpDown nudImageMinFree = null!;
    private DarkUI.Controls.DarkLabel lblImagePasscode = null!;
    private DarkUI.Controls.DarkTextBox txtImagePasscode = null!;
    private DarkUI.Controls.DarkLabel lblImageSdk = null!;
    private DarkUI.Controls.DarkComboBox cboImageSdk = null!;
    private DarkUI.Controls.DarkLabel lblImagePkgType = null!;
    private DarkUI.Controls.DarkComboBox cboImagePkgType = null!;
    private DarkUI.Controls.DarkLabel lblImageCompression = null!;
    private DarkUI.Controls.DarkComboBox cboImageCompression = null!;
    private DarkUI.Controls.DarkLabel lblImageKrakenLevel = null!;
    private DarkUI.Controls.DarkComboBox cboImageKrakenLevel = null!;
    private DarkUI.Controls.DarkLabel lblImageKrakenThreads = null!;
    private DarkUI.Controls.DarkNumericUpDown nudImageKrakenThreads = null!;
    private DarkUI.Controls.DarkLabel lblImagePlayGo = null!;
    private DarkUI.Controls.DarkNumericUpDown nudImagePlayGoChunks = null!;
    private DarkUI.Controls.DarkLabel lblImageDrm = null!;
    private DarkUI.Controls.DarkComboBox cboImageDrm = null!;
    private DarkUI.Controls.DarkCheckBox chkImageFakeSign = null!;
    private DarkUI.Controls.DarkCheckBox chkImageRightSprx = null!;
    private DarkUI.Controls.DarkCheckBox chkImageDeterministic = null!;
    private DarkUI.Controls.DarkLabel lblImageBackend = null!;
    private DarkUI.Controls.DarkComboBox cboImageBackend = null!;
    private DarkUI.Controls.DarkCheckBox chkImageAdvancedOptions = null!;
    private DarkUI.Controls.DarkLabel lblImageTemp = null!;
    private DarkUI.Controls.DarkTextBox txtImageTemp = null!;
    private DarkUI.Controls.DarkButton btnImageTempBrowse = null!;
    private SaveFileDialog imageSaveDialog = null!;
    private DarkUI.Controls.DarkStatusStrip statusMain = null!;
    private DarkUI.Controls.DarkToolStripStatusLabel statusLabel = null!;
    private DarkUI.Controls.DarkToolStripStatusLabel statusSpring = null!;
    private DarkUI.Controls.DarkToolStripStatusLabel statusCount = null!;
    private FolderBrowserDialog folderBrowserDialog = null!;
    private OpenFileDialog packageOpenDialog = null!;
    private OpenFileDialog sourceImageOpenDialog = null!;
    private SaveFileDialog containedFileSaveDialog = null!;
    private SaveFileDialog artworkSaveDialog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        imageListFiles = new ImageList(components);
        menuMain = new DarkUI.Controls.DarkMenuStrip();
        menuFile = new ToolStripMenuItem();
        menuAddFolder = new ToolStripMenuItem();
        menuOpenDump = new ToolStripMenuItem();
        menuOpenPackage = new ToolStripMenuItem();
        menuRecent = new ToolStripMenuItem();
        menuRefresh = new ToolStripMenuItem();
        menuSaveManifest = new ToolStripMenuItem();
        menuEmptyList = new ToolStripMenuItem();
        menuRemoveMissing = new ToolStripMenuItem();
        menuSeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuSettings = new ToolStripMenuItem();
        menuExit = new ToolStripMenuItem();
        menuHelp = new ToolStripMenuItem();
        menuAbout = new ToolStripMenuItem();
        menuHelpSeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuHelpCheckUpdate = new ToolStripMenuItem();
        menuHelpKofi = new ToolStripMenuItem();
        menuHelpPayPal = new ToolStripMenuItem();
        contextLibrary = new DarkUI.Controls.DarkContextMenu();
        menuLibraryReveal = new ToolStripMenuItem();
        menuLibrarySeparator1 = new DarkUI.Controls.DarkToolStripSeparator();
        menuLibraryCopy = new ToolStripMenuItem();
        menuLibraryCopyTitle = new ToolStripMenuItem();
        menuLibraryCopyTitleId = new ToolStripMenuItem();
        menuLibraryCopyContentId = new ToolStripMenuItem();
        menuLibraryCopyFileName = new ToolStripMenuItem();
        menuLibraryCopyPath = new ToolStripMenuItem();
        menuLibraryRename = new ToolStripMenuItem();
        menuLibraryRenameAll = new ToolStripMenuItem();
        menuLibraryRenameByPriority = new ToolStripMenuItem();
        menuLibraryMove = new ToolStripMenuItem();
        menuLibraryMoveTitle = new ToolStripMenuItem();
        menuLibraryMoveTitleId = new ToolStripMenuItem();
        menuLibraryMoveCategory = new ToolStripMenuItem();
        menuLibraryMoveRegion = new ToolStripMenuItem();
        menuLibraryMoveSource = new ToolStripMenuItem();
        menuLibraryMoveSeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuLibraryMoveSingle = new ToolStripMenuItem();
        menuLibraryDelete = new ToolStripMenuItem();
        menuLibrarySeparator2 = new DarkUI.Controls.DarkToolStripSeparator();
        menuLibrarySaveArtwork = new ToolStripMenuItem();
        menuLibraryExport = new ToolStripMenuItem();
        menuLibrarySeparator3 = new DarkUI.Controls.DarkToolStripSeparator();
        menuLibraryGroupBy = new ToolStripMenuItem();
        menuLibraryGroupNone = new ToolStripMenuItem();
        menuLibraryGroupFamily = new ToolStripMenuItem();
        menuLibraryGroupTitleId = new ToolStripMenuItem();
        menuLibraryGroupCategory = new ToolStripMenuItem();
        menuLibraryGroupRegion = new ToolStripMenuItem();
        menuLibraryGroupSource = new ToolStripMenuItem();
        menuLibraryGroupFirmware = new ToolStripMenuItem();
        menuLibraryDuplicates = new ToolStripMenuItem();
        menuLibrarySeparator5 = new DarkUI.Controls.DarkToolStripSeparator();
        menuLibraryGroupExport = new ToolStripMenuItem();
        menuLibraryGroupArtwork = new ToolStripMenuItem();
        searchLibrary = new DarkUI.Controls.DarkSearchBox();
        lblFilterCategory = new DarkUI.Controls.DarkLabel();
        cboFilterCategory = new DarkUI.Controls.DarkCheckedComboBox(components);
        lblFilterRegion = new DarkUI.Controls.DarkLabel();
        cboFilterRegion = new DarkUI.Controls.DarkCheckedComboBox(components);
        lblFilterFormat = new DarkUI.Controls.DarkLabel();
        cboFilterFormat = new DarkUI.Controls.DarkCheckedComboBox(components);
        lblFilterGroup = new DarkUI.Controls.DarkLabel();
        cboFilterGroup = new DarkUI.Controls.DarkComboBox();
        btnFilterClear = new DarkUI.Controls.DarkButton();
        chipsFilter = new DarkUI.Controls.DarkChipsPanel(components);
        lblFilterPreset = new DarkUI.Controls.DarkLabel();
        cboFilterPreset = new DarkUI.Controls.DarkComboBox();
        lblFilterEmpty = new DarkUI.Controls.DarkLabel();
        splitMain = new DarkUI.Controls.DarkSplitContainer();
        splitMainPane1 = new DarkUI.Controls.DarkSplitPane();
        gridLibrary = new DarkUI.Controls.DarkDataGridView();
        splitMainPane2 = new DarkUI.Controls.DarkSplitPane();
        tabsWorkspace = new DarkUI.Controls.DarkTabControl();
        tabWorkspaceGeneral = new DarkUI.Controls.DarkTabPage();
        tabsDetails = new DarkUI.Controls.DarkTabControl();
        tabOverview = new DarkUI.Controls.DarkTabPage();
        gridOverview = new DarkUI.Controls.DarkDataGridView();
        btnOverviewCopySelected = new DarkUI.Controls.DarkButton();
        btnOverviewCopyAll = new DarkUI.Controls.DarkButton();
        tabArtwork = new DarkUI.Controls.DarkTabPage();
        splitArtwork = new DarkUI.Controls.DarkSplitContainer();
        splitArtworkPane1 = new DarkUI.Controls.DarkSplitPane();
        sectionIcon = new DarkUI.Controls.DarkSectionPanel();
        pictureIcon = new PictureBox();
        contextArtwork = new DarkUI.Controls.DarkContextMenu();
        menuArtworkSaveThis = new ToolStripMenuItem();
        menuArtworkSaveAll = new ToolStripMenuItem();
        splitArtworkPane2 = new DarkUI.Controls.DarkSplitPane();
        sectionBackground = new DarkUI.Controls.DarkSectionPanel();
        tabsBackgrounds = new DarkUI.Controls.DarkTabControl();
        tabPic0 = new DarkUI.Controls.DarkTabPage();
        pictureBackground0 = new PictureBox();
        tabPic1 = new DarkUI.Controls.DarkTabPage();
        pictureBackground1 = new PictureBox();
        tabPic2 = new DarkUI.Controls.DarkTabPage();
        pictureBackground2 = new PictureBox();
        btnArtworkSaveAll = new DarkUI.Controls.DarkButton();
        tabTrophies = new DarkUI.Controls.DarkTabPage();
        gridTrophies = new DarkUI.Controls.DarkDataGridView();
        contextTrophies = new DarkUI.Controls.DarkContextMenu();
        menuTrophySaveIcon = new ToolStripMenuItem();
        menuTrophySaveAllIcons = new ToolStripMenuItem();
        menuTrophyExportCsv = new ToolStripMenuItem();
        lblTrophySummary = new DarkUI.Controls.DarkLabel();
        searchTrophy = new DarkUI.Controls.DarkSearchBox();
        chkTrophyShowHidden = new DarkUI.Controls.DarkCheckBox();
        chkTrophyBronze = new DarkUI.Controls.DarkCheckBox();
        chkTrophySilver = new DarkUI.Controls.DarkCheckBox();
        chkTrophyGold = new DarkUI.Controls.DarkCheckBox();
        chkTrophyPlatinum = new DarkUI.Controls.DarkCheckBox();
        btnTrophyExportCsv = new DarkUI.Controls.DarkButton();
        btnTrophySaveIcons = new DarkUI.Controls.DarkButton();
        tabActivities = new DarkUI.Controls.DarkTabPage();
        tabsUds = new DarkUI.Controls.DarkTabControl();
        tabUdsEvents = new DarkUI.Controls.DarkTabPage();
        splitUdsEvents = new DarkUI.Controls.DarkSplitContainer();
        splitUdsEventsPane1 = new DarkUI.Controls.DarkSplitPane();
        gridUdsEvents = new DarkUI.Controls.DarkDataGridView();
        splitUdsEventsPane2 = new DarkUI.Controls.DarkSplitPane();
        gridUdsEventProperties = new DarkUI.Controls.DarkDataGridView();
        tabUdsStats = new DarkUI.Controls.DarkTabPage();
        gridUdsStats = new DarkUI.Controls.DarkDataGridView();
        tabUdsEnums = new DarkUI.Controls.DarkTabPage();
        gridUdsEnums = new DarkUI.Controls.DarkDataGridView();
        tabUdsRules = new DarkUI.Controls.DarkTabPage();
        gridUdsRules = new DarkUI.Controls.DarkDataGridView();
        lblActivitiesSummary = new DarkUI.Controls.DarkLabel();
        searchUds = new DarkUI.Controls.DarkSearchBox();
        btnUdsCopySelected = new DarkUI.Controls.DarkButton();
        btnUdsCopyAll = new DarkUI.Controls.DarkButton();
        tabFiles = new DarkUI.Controls.DarkTabPage();
        sectionFileBrowser = new DarkUI.Controls.DarkSectionPanel();
        splitFileBrowser = new DarkUI.Controls.DarkSplitContainer();
        splitFileBrowserPane1 = new DarkUI.Controls.DarkSplitPane();
        treeFiles = new DarkUI.Controls.DarkTreeView();
        contextTreeFiles = new DarkUI.Controls.DarkContextMenu();
        menuTreeExpand = new ToolStripMenuItem();
        menuTreeCollapse = new ToolStripMenuItem();
        menuTreeExpandAll = new ToolStripMenuItem();
        menuTreeCollapseAll = new ToolStripMenuItem();
        menuTreeSeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuTreeCopyPath = new ToolStripMenuItem();
        menuTreeExtract = new ToolStripMenuItem();
        splitFileBrowserPane2 = new DarkUI.Controls.DarkSplitPane();
        splitFileContentPreview = new DarkUI.Controls.DarkSplitContainer();
        splitFileContentPreviewPane1 = new DarkUI.Controls.DarkSplitPane();
        fileListPanel = new DarkUI.Controls.DarkPanel();
        searchFileFilter = new DarkUI.Controls.DarkSearchBox();
        listFiles = new DarkUI.Controls.DarkListView();
        contextFiles = new DarkUI.Controls.DarkContextMenu();
        menuFileOpenContained = new ToolStripMenuItem();
        menuFileExtractContained = new ToolStripMenuItem();
        menuFileExtractSelected = new ToolStripMenuItem();
        menuFileCopySeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuFileCopyPath = new ToolStripMenuItem();
        menuFileCopyName = new ToolStripMenuItem();
        menuFileContainerSeparator = new DarkUI.Controls.DarkToolStripSeparator();
        menuFileRevealContainer = new ToolStripMenuItem();
        splitFileContentPreviewPane2 = new DarkUI.Controls.DarkSplitPane();
        sectionFileViewer = new DarkUI.Controls.DarkSectionPanel();
        mediaFileHost = new System.Windows.Forms.Integration.ElementHost();
        txtHexViewer = new DarkUI.Controls.DarkRichTextBox();
        txtFileViewer = new DarkUI.Controls.DarkRichTextBox();
        pictureFileViewer = new PictureBox();
        lblFileViewerInfo = new DarkUI.Controls.DarkLabel();
        btnMediaLoad = new DarkUI.Controls.DarkButton();
        btnMediaPlay = new DarkUI.Controls.DarkButton();
        btnMediaPause = new DarkUI.Controls.DarkButton();
        btnMediaStop = new DarkUI.Controls.DarkButton();
        btnHexPrevious = new DarkUI.Controls.DarkButton();
        btnHexNext = new DarkUI.Controls.DarkButton();
        lblHexPage = new DarkUI.Controls.DarkLabel();
        lblFilesSummary = new DarkUI.Controls.DarkLabel();
        btnFileExtractAll = new DarkUI.Controls.DarkButton();
        btnFileCancel = new DarkUI.Controls.DarkButton();
        tabExecutable = new DarkUI.Controls.DarkTabPage();
        tabsExecutable = new DarkUI.Controls.DarkTabControl();
        tabExecModules = new DarkUI.Controls.DarkTabPage();
        gridModules = new DarkUI.Controls.DarkDataGridView();
        tabExecElf = new DarkUI.Controls.DarkTabPage();
        splitExecElf = new DarkUI.Controls.DarkSplitContainer();
        splitExecElfPane1 = new DarkUI.Controls.DarkSplitPane();
        gridElfPrograms = new DarkUI.Controls.DarkDataGridView();
        splitExecElfPane2 = new DarkUI.Controls.DarkSplitPane();
        gridElfSections = new DarkUI.Controls.DarkDataGridView();
        tabExecSelf = new DarkUI.Controls.DarkTabPage();
        splitExecSelf = new DarkUI.Controls.DarkSplitContainer();
        splitExecSelfPane1 = new DarkUI.Controls.DarkSplitPane();
        gridSelfHeader = new DarkUI.Controls.DarkDataGridView();
        splitExecSelfPane2 = new DarkUI.Controls.DarkSplitPane();
        gridSelfSegments = new DarkUI.Controls.DarkDataGridView();
        lblExecutableSummary = new DarkUI.Controls.DarkLabel();
        searchExecutable = new DarkUI.Controls.DarkSearchBox();
        btnExecCopySelected = new DarkUI.Controls.DarkButton();
        btnExecCopyAll = new DarkUI.Controls.DarkButton();
        btnExecExtract = new DarkUI.Controls.DarkButton();
        btnExecHash = new DarkUI.Controls.DarkButton();
        tabRaw = new DarkUI.Controls.DarkTabPage();
        txtRawMetadata = new DarkUI.Controls.DarkRichTextBox();
        btnCopyRawJson = new DarkUI.Controls.DarkButton();
        btnRawFormatted = new DarkUI.Controls.DarkButton();
        btnRawOriginal = new DarkUI.Controls.DarkButton();
        tabPackage = new DarkUI.Controls.DarkTabPage();
        tabsPackage = new DarkUI.Controls.DarkTabControl();
        tabMetadata = new DarkUI.Controls.DarkTabPage();
        tabsMetadata = new DarkUI.Controls.DarkTabControl();
        tabPkgContainer = new DarkUI.Controls.DarkTabPage();
        gridPkgHeader = new DarkUI.Controls.DarkDataGridView();
        tabPkgSegments = new DarkUI.Controls.DarkTabPage();
        gridPkgSegments = new DarkUI.Controls.DarkDataGridView();
        tabPkgEntries = new DarkUI.Controls.DarkTabPage();
        gridPkgEntries = new DarkUI.Controls.DarkDataGridView();
        tabPkgSfo = new DarkUI.Controls.DarkTabPage();
        gridParamSfo = new DarkUI.Controls.DarkDataGridView();
        tabPkgKeystone = new DarkUI.Controls.DarkTabPage();
        gridKeystone = new DarkUI.Controls.DarkDataGridView();
        tabPkgSi = new DarkUI.Controls.DarkTabPage();
        gridSi = new DarkUI.Controls.DarkDataGridView();
        tabPkgPlayGo = new DarkUI.Controls.DarkTabPage();
        tabsPlayGo = new DarkUI.Controls.DarkTabControl();
        tabPlayGoChunks = new DarkUI.Controls.DarkTabPage();
        gridPlayGoChunks = new DarkUI.Controls.DarkDataGridView();
        tabPlayGoScenarios = new DarkUI.Controls.DarkTabPage();
        gridPlayGoScenarios = new DarkUI.Controls.DarkDataGridView();
        tabPlayGoFiles = new DarkUI.Controls.DarkTabPage();
        gridPlayGoFiles = new DarkUI.Controls.DarkDataGridView();
        lblPlayGoSummary = new DarkUI.Controls.DarkLabel();
        tabWorkspaceTools = new DarkUI.Controls.DarkTabPage();
        toolsLayout = new DarkUI.Controls.DarkTableLayoutPanel();
        sectionJob = new DarkUI.Controls.DarkSectionPanel();
        lblImageSourcePath = new DarkUI.Controls.DarkLabel();
        lblImageFormat = new DarkUI.Controls.DarkLabel();
        lblImageAction = new DarkUI.Controls.DarkLabel();
        cboImageAction = new DarkUI.Controls.DarkComboBox();
        lblImageSource = new DarkUI.Controls.DarkLabel();
        tabsImageTargets = new DarkUI.Controls.DarkTabControl();
        tabTargetExfat = new DarkUI.Controls.DarkTabPage();
        lblOutExfat = new DarkUI.Controls.DarkLabel();
        txtOutExfat = new DarkUI.Controls.DarkTextBox();
        btnOutExfat = new DarkUI.Controls.DarkButton();
        chkOutExfat = new DarkUI.Controls.DarkCheckBox();
        lblImageCluster = new DarkUI.Controls.DarkLabel();
        cboImageCluster = new DarkUI.Controls.DarkComboBox();
        chkImageAmpr = new DarkUI.Controls.DarkCheckBox();
        tabTargetFfpkg = new DarkUI.Controls.DarkTabPage();
        lblOutFfpkg = new DarkUI.Controls.DarkLabel();
        txtOutFfpkg = new DarkUI.Controls.DarkTextBox();
        btnOutFfpkg = new DarkUI.Controls.DarkButton();
        chkOutFfpkg = new DarkUI.Controls.DarkCheckBox();
        lblImageBlock = new DarkUI.Controls.DarkLabel();
        cboImageBlock = new DarkUI.Controls.DarkComboBox();
        lblImageFragment = new DarkUI.Controls.DarkLabel();
        cboImageFragment = new DarkUI.Controls.DarkComboBox();
        lblImageDensity = new DarkUI.Controls.DarkLabel();
        cboImageDensity = new DarkUI.Controls.DarkComboBox();
        lblImageMinFree = new DarkUI.Controls.DarkLabel();
        nudImageMinFree = new DarkUI.Controls.DarkNumericUpDown();
        tabTargetFfpfsc = new DarkUI.Controls.DarkTabPage();
        lblOutFfpfsc = new DarkUI.Controls.DarkLabel();
        txtOutFfpfsc = new DarkUI.Controls.DarkTextBox();
        btnOutFfpfsc = new DarkUI.Controls.DarkButton();
        chkOutFfpfsc = new DarkUI.Controls.DarkCheckBox();
        lblImageLevel = new DarkUI.Controls.DarkLabel();
        nudImageLevel = new DarkUI.Controls.DarkNumericUpDown();
        lblImageGain = new DarkUI.Controls.DarkLabel();
        nudImageGain = new DarkUI.Controls.DarkNumericUpDown();
        tabTargetDebug = new DarkUI.Controls.DarkTabPage();
        lblOutDebug = new DarkUI.Controls.DarkLabel();
        txtOutDebug = new DarkUI.Controls.DarkTextBox();
        btnOutDebug = new DarkUI.Controls.DarkButton();
        chkOutDebug = new DarkUI.Controls.DarkCheckBox();
        lblDbgPasscode = new DarkUI.Controls.DarkLabel();
        txtDbgPasscode = new DarkUI.Controls.DarkTextBox();
        lblImageSdk = new DarkUI.Controls.DarkLabel();
        cboImageSdk = new DarkUI.Controls.DarkComboBox();
        lblImagePkgType = new DarkUI.Controls.DarkLabel();
        cboImagePkgType = new DarkUI.Controls.DarkComboBox();
        lblImageCompression = new DarkUI.Controls.DarkLabel();
        cboImageCompression = new DarkUI.Controls.DarkComboBox();
        lblImageKrakenLevel = new DarkUI.Controls.DarkLabel();
        cboImageKrakenLevel = new DarkUI.Controls.DarkComboBox();
        lblImagePlayGo = new DarkUI.Controls.DarkLabel();
        nudImagePlayGoChunks = new DarkUI.Controls.DarkNumericUpDown();
        lblImageKrakenThreads = new DarkUI.Controls.DarkLabel();
        nudImageKrakenThreads = new DarkUI.Controls.DarkNumericUpDown();
        lblImageTemp = new DarkUI.Controls.DarkLabel();
        txtImageTemp = new DarkUI.Controls.DarkTextBox();
        btnImageTempBrowse = new DarkUI.Controls.DarkButton();
        lblImageDrm = new DarkUI.Controls.DarkLabel();
        cboImageDrm = new DarkUI.Controls.DarkComboBox();
        lblImageBackend = new DarkUI.Controls.DarkLabel();
        cboImageBackend = new DarkUI.Controls.DarkComboBox();
        chkImageFakeSign = new DarkUI.Controls.DarkCheckBox();
        chkImageRightSprx = new DarkUI.Controls.DarkCheckBox();
        chkImageDeterministic = new DarkUI.Controls.DarkCheckBox();
        chkImageAdvancedOptions = new DarkUI.Controls.DarkCheckBox();
        tabTargetOptions = new DarkUI.Controls.DarkTabPage();
        lblImageOutput = new DarkUI.Controls.DarkLabel();
        txtImageOutput = new DarkUI.Controls.DarkTextBox();
        btnImageBrowseOutput = new DarkUI.Controls.DarkButton();
        chkImageOverwrite = new DarkUI.Controls.DarkCheckBox();
        lblImagePasscode = new DarkUI.Controls.DarkLabel();
        txtImagePasscode = new DarkUI.Controls.DarkTextBox();
        toolsFooter = new DarkUI.Controls.DarkFooterBar();
        lblImageStatus = new DarkUI.Controls.DarkLabel();
        btnImageRun = new DarkUI.Controls.DarkButton();
        btnImageCancel = new DarkUI.Controls.DarkButton();
        tabTasks = new DarkUI.Controls.DarkTabPage();
        tasksLayout = new DarkUI.Controls.DarkTableLayoutPanel();
        chkTaskAutoStart = new DarkUI.Controls.DarkCheckBox();
        btnTaskStart = new DarkUI.Controls.DarkButton();
        btnTaskCancel = new DarkUI.Controls.DarkButton();
        btnTaskRetry = new DarkUI.Controls.DarkButton();
        btnTaskRemove = new DarkUI.Controls.DarkButton();
        btnTaskOpen = new DarkUI.Controls.DarkButton();
        btnTaskClear = new DarkUI.Controls.DarkButton();
        lblTaskGroup = new DarkUI.Controls.DarkLabel();
        cboTaskGroup = new DarkUI.Controls.DarkComboBox();
        lblTaskSummary = new DarkUI.Controls.DarkLabel();
        chkTaskFollow = new DarkUI.Controls.DarkCheckBox();
        cboTaskFilter = new DarkUI.Controls.DarkComboBox();
        searchTasks = new DarkUI.Controls.DarkSearchBox();
        btnTaskToggleDetails = new DarkUI.Controls.DarkButton();
        splitTasks = new DarkUI.Controls.DarkSplitContainer();
        splitTasksPane1 = new DarkUI.Controls.DarkSplitPane();
        sectionTasksList = new DarkUI.Controls.DarkSectionPanel();
        gridTasks = new DarkUI.Controls.DarkDataGridView();
        colTaskName = new DataGridViewTextBoxColumn();
        colTaskOperation = new DataGridViewTextBoxColumn();
        colTaskRoute = new DataGridViewTextBoxColumn();
        colTaskStatus = new DataGridViewTextBoxColumn();
        colTaskStage = new DataGridViewTextBoxColumn();
        colTaskProgress = new DataGridViewTextBoxColumn();
        colTaskElapsed = new DataGridViewTextBoxColumn();
        contextTasks = new DarkUI.Controls.DarkContextMenu();
        menuTaskStart = new ToolStripMenuItem();
        menuTaskCancel = new ToolStripMenuItem();
        menuTaskRetry = new ToolStripMenuItem();
        menuTaskRemove = new ToolStripMenuItem();
        menuTaskSeparator1 = new DarkUI.Controls.DarkToolStripSeparator();
        menuTaskOpen = new ToolStripMenuItem();
        menuTaskShowSource = new ToolStripMenuItem();
        menuTaskReport = new ToolStripMenuItem();
        menuTaskClear = new ToolStripMenuItem();
        splitTasksPane2 = new DarkUI.Controls.DarkSplitPane();
        sectionTaskDetails = new DarkUI.Controls.DarkSectionPanel();
        taskDetailLayout = new DarkUI.Controls.DarkTableLayoutPanel();
        lblTaskStage = new DarkUI.Controls.DarkLabel();
        lblTaskCurrentCaption = new DarkUI.Controls.DarkLabel();
        barTaskCurrent = new DarkUI.Controls.DarkProgressBar();
        lblTaskOverallCaption = new DarkUI.Controls.DarkLabel();
        barTaskOverall = new DarkUI.Controls.DarkProgressBar();
        lblTaskMessage = new DarkUI.Controls.DarkLabel();
        lblTaskMeta = new DarkUI.Controls.DarkLabel();
        lblTaskResult = new DarkUI.Controls.DarkLabel();
        btnTaskDiagnostic = new DarkUI.Controls.DarkButton();
        tabLog = new DarkUI.Controls.DarkTabPage();
        txtLogView = new DarkUI.Controls.DarkRichTextBox();
        lblLogLevel = new DarkUI.Controls.DarkLabel();
        cboLogLevel = new DarkUI.Controls.DarkComboBox();
        chkLogAutoScroll = new DarkUI.Controls.DarkCheckBox();
        btnLogClear = new DarkUI.Controls.DarkButton();
        btnLogOpenFolder = new DarkUI.Controls.DarkButton();
        colFileName = new ColumnHeader();
        colFileType = new ColumnHeader();
        colFilePath = new ColumnHeader();
        colFileSize = new ColumnHeader();
        imageSaveDialog = new SaveFileDialog();
        trophyCsvSaveDialog = new SaveFileDialog();
        statusMain = new DarkUI.Controls.DarkStatusStrip();
        statusLabel = new DarkUI.Controls.DarkToolStripStatusLabel();
        statusSpring = new DarkUI.Controls.DarkToolStripStatusLabel();
        statusCount = new DarkUI.Controls.DarkToolStripStatusLabel();
        folderBrowserDialog = new FolderBrowserDialog();
        packageOpenDialog = new OpenFileDialog();
        sourceImageOpenDialog = new OpenFileDialog();
        containedFileSaveDialog = new SaveFileDialog();
        artworkSaveDialog = new SaveFileDialog();
        menuMain.SuspendLayout();
        contextLibrary.SuspendLayout();
        splitMain.SuspendLayout();
        splitMainPane1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridLibrary).BeginInit();
        splitMainPane2.SuspendLayout();
        tabsWorkspace.SuspendLayout();
        tabWorkspaceGeneral.SuspendLayout();
        tabsDetails.SuspendLayout();
        tabOverview.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridOverview).BeginInit();
        tabArtwork.SuspendLayout();
        splitArtwork.SuspendLayout();
        splitArtworkPane1.SuspendLayout();
        sectionIcon.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureIcon).BeginInit();
        contextArtwork.SuspendLayout();
        splitArtworkPane2.SuspendLayout();
        sectionBackground.SuspendLayout();
        tabsBackgrounds.SuspendLayout();
        tabPic0.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground0).BeginInit();
        tabPic1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground1).BeginInit();
        tabPic2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground2).BeginInit();
        tabTrophies.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridTrophies).BeginInit();
        contextTrophies.SuspendLayout();
        tabActivities.SuspendLayout();
        tabsUds.SuspendLayout();
        tabUdsEvents.SuspendLayout();
        splitUdsEvents.SuspendLayout();
        splitUdsEventsPane1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridUdsEvents).BeginInit();
        splitUdsEventsPane2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridUdsEventProperties).BeginInit();
        tabUdsStats.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridUdsStats).BeginInit();
        tabUdsEnums.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridUdsEnums).BeginInit();
        tabUdsRules.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridUdsRules).BeginInit();
        tabFiles.SuspendLayout();
        sectionFileBrowser.SuspendLayout();
        splitFileBrowser.SuspendLayout();
        splitFileBrowserPane1.SuspendLayout();
        contextTreeFiles.SuspendLayout();
        splitFileBrowserPane2.SuspendLayout();
        splitFileContentPreview.SuspendLayout();
        splitFileContentPreviewPane1.SuspendLayout();
        fileListPanel.SuspendLayout();
        contextFiles.SuspendLayout();
        splitFileContentPreviewPane2.SuspendLayout();
        sectionFileViewer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureFileViewer).BeginInit();
        tabExecutable.SuspendLayout();
        tabsExecutable.SuspendLayout();
        tabExecModules.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridModules).BeginInit();
        tabExecElf.SuspendLayout();
        splitExecElf.SuspendLayout();
        splitExecElfPane1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridElfPrograms).BeginInit();
        splitExecElfPane2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridElfSections).BeginInit();
        tabExecSelf.SuspendLayout();
        splitExecSelf.SuspendLayout();
        splitExecSelfPane1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridSelfHeader).BeginInit();
        splitExecSelfPane2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridSelfSegments).BeginInit();
        tabRaw.SuspendLayout();
        tabPackage.SuspendLayout();
        tabsPackage.SuspendLayout();
        tabPkgContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPkgHeader).BeginInit();
        tabPkgSegments.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPkgSegments).BeginInit();
        tabPkgEntries.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPkgEntries).BeginInit();
        tabPkgSfo.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridParamSfo).BeginInit();
        tabPkgKeystone.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridKeystone).BeginInit();
        tabPkgSi.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridSi).BeginInit();
        tabPkgPlayGo.SuspendLayout();
        tabsPlayGo.SuspendLayout();
        tabPlayGoChunks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPlayGoChunks).BeginInit();
        tabPlayGoScenarios.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPlayGoScenarios).BeginInit();
        tabPlayGoFiles.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridPlayGoFiles).BeginInit();
        tabWorkspaceTools.SuspendLayout();
        toolsLayout.SuspendLayout();
        sectionJob.SuspendLayout();
        tabsImageTargets.SuspendLayout();
        tabTargetExfat.SuspendLayout();
        tabTargetFfpkg.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudImageMinFree).BeginInit();
        tabTargetFfpfsc.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudImageLevel).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudImageGain).BeginInit();
        tabTargetDebug.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudImagePlayGoChunks).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudImageKrakenThreads).BeginInit();
        tabTargetOptions.SuspendLayout();
        toolsFooter.SuspendLayout();
        tabTasks.SuspendLayout();
        tasksLayout.SuspendLayout();
        splitTasks.SuspendLayout();
        splitTasksPane1.SuspendLayout();
        sectionTasksList.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridTasks).BeginInit();
        contextTasks.SuspendLayout();
        splitTasksPane2.SuspendLayout();
        sectionTaskDetails.SuspendLayout();
        taskDetailLayout.SuspendLayout();
        tabLog.SuspendLayout();
        statusMain.SuspendLayout();
        SuspendLayout();
        // 
        // imageListFiles
        // 
        imageListFiles.ColorDepth = ColorDepth.Depth32Bit;
        imageListFiles.ImageSize = new Size(16, 16);
        imageListFiles.TransparentColor = Color.Transparent;
        // 
        // menuMain
        // 
        menuMain.Items.AddRange(new ToolStripItem[] { menuFile, menuHelp });
        menuMain.Location = new Point(0, 0);
        menuMain.Name = "menuMain";
        menuMain.Padding = new Padding(3, 2, 0, 2);
        menuMain.Size = new Size(1384, 24);
        menuMain.TabIndex = 0;
        // 
        // menuFile
        // 
        menuFile.BackColor = Color.FromArgb(60, 63, 65);
        menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuAddFolder, menuOpenDump, menuOpenPackage, menuRecent, menuRefresh, menuLibraryRenameAll, menuSaveManifest, menuEmptyList, menuRemoveMissing, menuSeparator, menuSettings, menuExit });
        menuFile.ForeColor = Color.FromArgb(220, 220, 220);
        menuFile.Name = "menuFile";
        menuFile.Size = new Size(37, 20);
        menuFile.Text = "File";
        // 
        // menuAddFolder
        // 
        menuAddFolder.Name = "menuAddFolder";
        menuAddFolder.Size = new Size(233, 22);
        menuAddFolder.Text = "Add Library Folder...";
        menuAddFolder.Click += AddFolder_Click;
        // 
        // menuOpenDump
        // 
        menuOpenDump.Name = "menuOpenDump";
        menuOpenDump.Size = new Size(233, 22);
        menuOpenDump.Text = "Open Dump Folder...";
        menuOpenDump.Click += OpenDump_Click;
        // 
        // menuOpenPackage
        // 
        menuOpenPackage.Name = "menuOpenPackage";
        menuOpenPackage.Size = new Size(233, 22);
        menuOpenPackage.Text = "Open PS5 Container / Image...";
        menuOpenPackage.Click += OpenPackage_Click;
        // 
        // menuRecent
        // 
        menuRecent.Name = "menuRecent";
        menuRecent.Size = new Size(233, 22);
        menuRecent.Text = "Open Recent";
        // 
        // menuRefresh
        // 
        menuRefresh.Name = "menuRefresh";
        menuRefresh.Size = new Size(233, 22);
        menuRefresh.Text = "Refresh Library";
        menuRefresh.Click += Refresh_Click;
        // 
        // menuSaveManifest
        // 
        menuSaveManifest.Name = "menuSaveManifest";
        menuSaveManifest.Size = new Size(233, 22);
        menuSaveManifest.Text = "Save Manifest";
        menuSaveManifest.Click += menuSaveManifest_Click;
        // 
        // menuEmptyList
        // 
        menuEmptyList.Name = "menuEmptyList";
        menuEmptyList.Size = new Size(233, 22);
        menuEmptyList.Text = "Empty List";
        menuEmptyList.Click += menuEmptyList_Click;
        // 
        // menuRemoveMissing
        // 
        menuRemoveMissing.Name = "menuRemoveMissing";
        menuRemoveMissing.Size = new Size(233, 22);
        menuRemoveMissing.Text = "Remove Missing Items";
        menuRemoveMissing.Click += menuRemoveMissing_Click;
        // 
        // menuSeparator
        // 
        menuSeparator.Name = "menuSeparator";
        menuSeparator.Size = new Size(230, 6);
        // 
        // menuSettings
        // 
        menuSettings.Name = "menuSettings";
        menuSettings.Size = new Size(233, 22);
        menuSettings.Text = "Settings...";
        menuSettings.Click += Settings_Click;
        // 
        // menuExit
        // 
        menuExit.Name = "menuExit";
        menuExit.Size = new Size(233, 22);
        menuExit.Text = "Exit";
        menuExit.Click += Exit_Click;
        // 
        // menuHelp
        // 
        menuHelp.BackColor = Color.FromArgb(60, 63, 65);
        menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout, menuHelpSeparator, menuHelpCheckUpdate, menuHelpKofi, menuHelpPayPal });
        menuHelp.ForeColor = Color.FromArgb(220, 220, 220);
        menuHelp.Name = "menuHelp";
        menuHelp.Size = new Size(44, 20);
        menuHelp.Text = "Help";
        // 
        // menuAbout
        // 
        menuAbout.Name = "menuAbout";
        menuAbout.Size = new Size(180, 22);
        menuAbout.Text = "About";
        menuAbout.Click += About_Click;
        // 
        // menuHelpSeparator
        // 
        menuHelpSeparator.Name = "menuHelpSeparator";
        menuHelpSeparator.Size = new Size(177, 6);
        // 
        // menuHelpCheckUpdate
        // 
        menuHelpCheckUpdate.Name = "menuHelpCheckUpdate";
        menuHelpCheckUpdate.Size = new Size(180, 22);
        menuHelpCheckUpdate.Text = "Check for Updates...";
        menuHelpCheckUpdate.Click += menuHelpCheckUpdate_Click;
        // 
        // menuHelpKofi
        // 
        menuHelpKofi.Name = "menuHelpKofi";
        menuHelpKofi.Size = new Size(180, 22);
        menuHelpKofi.Text = "Buy me a Ko-fi";
        menuHelpKofi.Click += menuHelpKofi_Click;
        // 
        // menuHelpPayPal
        // 
        menuHelpPayPal.Name = "menuHelpPayPal";
        menuHelpPayPal.Size = new Size(180, 22);
        menuHelpPayPal.Text = "Support via PayPal";
        menuHelpPayPal.Click += menuHelpPayPal_Click;
        // 
        // contextLibrary
        // 
        contextLibrary.Items.AddRange(new ToolStripItem[] { menuLibraryReveal, menuLibrarySeparator1, menuLibraryCopy, menuLibraryRename, menuLibraryRenameByPriority, menuLibraryMove, menuLibraryDelete, menuLibrarySeparator2, menuLibrarySaveArtwork, menuLibraryExport, menuLibrarySeparator3, menuLibraryGroupBy, menuLibraryDuplicates, menuLibrarySeparator5, menuLibraryGroupExport, menuLibraryGroupArtwork });
        contextLibrary.Name = "contextLibrary";
        contextLibrary.Size = new Size(261, 318);
        contextLibrary.Opening += contextLibrary_Opening;
        // 
        // menuLibraryReveal
        // 
        menuLibraryReveal.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryReveal.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryReveal.Name = "menuLibraryReveal";
        menuLibraryReveal.Size = new Size(260, 22);
        menuLibraryReveal.Text = "Reveal in Explorer";
        menuLibraryReveal.Click += menuLibraryReveal_Click;
        // 
        // menuLibrarySeparator1
        // 
        menuLibrarySeparator1.BackColor = Color.FromArgb(60, 63, 65);
        menuLibrarySeparator1.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibrarySeparator1.Margin = new Padding(0, 0, 0, 1);
        menuLibrarySeparator1.Name = "menuLibrarySeparator1";
        menuLibrarySeparator1.Size = new Size(257, 6);
        // 
        // menuLibraryCopy
        // 
        menuLibraryCopy.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopy.DropDownItems.AddRange(new ToolStripItem[] { menuLibraryCopyTitle, menuLibraryCopyTitleId, menuLibraryCopyContentId, menuLibraryCopyFileName, menuLibraryCopyPath });
        menuLibraryCopy.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopy.Name = "menuLibraryCopy";
        menuLibraryCopy.Size = new Size(260, 22);
        menuLibraryCopy.Text = "Copy";
        // 
        // menuLibraryCopyTitle
        // 
        menuLibraryCopyTitle.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopyTitle.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopyTitle.Name = "menuLibraryCopyTitle";
        menuLibraryCopyTitle.Size = new Size(162, 22);
        menuLibraryCopyTitle.Text = "Copy Title";
        menuLibraryCopyTitle.Click += menuLibraryCopyTitle_Click;
        // 
        // menuLibraryCopyTitleId
        // 
        menuLibraryCopyTitleId.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopyTitleId.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopyTitleId.Name = "menuLibraryCopyTitleId";
        menuLibraryCopyTitleId.Size = new Size(162, 22);
        menuLibraryCopyTitleId.Text = "Copy Title ID";
        menuLibraryCopyTitleId.Click += menuLibraryCopyTitleId_Click;
        // 
        // menuLibraryCopyContentId
        // 
        menuLibraryCopyContentId.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopyContentId.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopyContentId.Name = "menuLibraryCopyContentId";
        menuLibraryCopyContentId.Size = new Size(162, 22);
        menuLibraryCopyContentId.Text = "Copy Content ID";
        menuLibraryCopyContentId.Click += menuLibraryCopyContentId_Click;
        // 
        // menuLibraryCopyFileName
        // 
        menuLibraryCopyFileName.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopyFileName.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopyFileName.Name = "menuLibraryCopyFileName";
        menuLibraryCopyFileName.Size = new Size(162, 22);
        menuLibraryCopyFileName.Text = "Copy File Name";
        menuLibraryCopyFileName.Click += menuLibraryCopyFileName_Click;
        // 
        // menuLibraryCopyPath
        // 
        menuLibraryCopyPath.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryCopyPath.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryCopyPath.Name = "menuLibraryCopyPath";
        menuLibraryCopyPath.Size = new Size(162, 22);
        menuLibraryCopyPath.Text = "Copy Path";
        menuLibraryCopyPath.Click += menuLibraryCopyPath_Click;
        // 
        // menuLibraryRename
        // 
        menuLibraryRename.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryRename.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryRename.Name = "menuLibraryRename";
        menuLibraryRename.Size = new Size(260, 22);
        menuLibraryRename.Text = "Rename";
        // 
        // menuLibraryRenameAll
        // 
        menuLibraryRenameAll.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryRenameAll.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryRenameAll.Name = "menuLibraryRenameAll";
        menuLibraryRenameAll.Size = new Size(260, 22);
        menuLibraryRenameAll.Text = "Rename All";
        // 
        // menuLibraryRenameByPriority
        // 
        menuLibraryRenameByPriority.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryRenameByPriority.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryRenameByPriority.Name = "menuLibraryRenameByPriority";
        menuLibraryRenameByPriority.Size = new Size(260, 22);
        menuLibraryRenameByPriority.Text = "Rename by Install Order (packages)";
        menuLibraryRenameByPriority.Click += menuLibraryRenameByPriority_Click;
        // 
        // menuLibraryMove
        // 
        menuLibraryMove.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryMove.DropDownItems.AddRange(new ToolStripItem[] { menuLibraryMoveTitle, menuLibraryMoveTitleId, menuLibraryMoveCategory, menuLibraryMoveRegion, menuLibraryMoveSource, menuLibraryMoveSeparator, menuLibraryMoveSingle });
        menuLibraryMove.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryMove.Name = "menuLibraryMove";
        menuLibraryMove.Size = new Size(260, 22);
        menuLibraryMove.Text = "Move to Folder";
        // 
        // menuLibraryMoveTitle
        // 
        menuLibraryMoveTitle.Name = "menuLibraryMoveTitle";
        menuLibraryMoveTitle.Size = new Size(195, 22);
        menuLibraryMoveTitle.Text = "By Title";
        menuLibraryMoveTitle.Click += menuLibraryMoveTitle_Click;
        // 
        // menuLibraryMoveTitleId
        // 
        menuLibraryMoveTitleId.Name = "menuLibraryMoveTitleId";
        menuLibraryMoveTitleId.Size = new Size(195, 22);
        menuLibraryMoveTitleId.Text = "By Title ID";
        menuLibraryMoveTitleId.Click += menuLibraryMoveTitleId_Click;
        // 
        // menuLibraryMoveCategory
        // 
        menuLibraryMoveCategory.Name = "menuLibraryMoveCategory";
        menuLibraryMoveCategory.Size = new Size(195, 22);
        menuLibraryMoveCategory.Text = "By Category";
        menuLibraryMoveCategory.Click += menuLibraryMoveCategory_Click;
        // 
        // menuLibraryMoveRegion
        // 
        menuLibraryMoveRegion.Name = "menuLibraryMoveRegion";
        menuLibraryMoveRegion.Size = new Size(195, 22);
        menuLibraryMoveRegion.Text = "By Region";
        menuLibraryMoveRegion.Click += menuLibraryMoveRegion_Click;
        // 
        // menuLibraryMoveSource
        // 
        menuLibraryMoveSource.Name = "menuLibraryMoveSource";
        menuLibraryMoveSource.Size = new Size(195, 22);
        menuLibraryMoveSource.Text = "By Source";
        menuLibraryMoveSource.Click += menuLibraryMoveSource_Click;
        // 
        // menuLibraryMoveSeparator
        // 
        menuLibraryMoveSeparator.Name = "menuLibraryMoveSeparator";
        menuLibraryMoveSeparator.Size = new Size(192, 6);
        // 
        // menuLibraryMoveSingle
        // 
        menuLibraryMoveSingle.Name = "menuLibraryMoveSingle";
        menuLibraryMoveSingle.Size = new Size(195, 22);
        menuLibraryMoveSingle.Text = "All into the Destination";
        menuLibraryMoveSingle.Click += menuLibraryMoveSingle_Click;
        // 
        // menuLibraryDelete
        // 
        menuLibraryDelete.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryDelete.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryDelete.Name = "menuLibraryDelete";
        menuLibraryDelete.Size = new Size(260, 22);
        menuLibraryDelete.Text = "Delete Package... (Recycle Bin)";
        menuLibraryDelete.Click += menuLibraryDelete_Click;
        // 
        // menuLibrarySeparator2
        // 
        menuLibrarySeparator2.BackColor = Color.FromArgb(60, 63, 65);
        menuLibrarySeparator2.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibrarySeparator2.Margin = new Padding(0, 0, 0, 1);
        menuLibrarySeparator2.Name = "menuLibrarySeparator2";
        menuLibrarySeparator2.Size = new Size(257, 6);
        // 
        // menuLibrarySaveArtwork
        // 
        menuLibrarySaveArtwork.BackColor = Color.FromArgb(60, 63, 65);
        menuLibrarySaveArtwork.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibrarySaveArtwork.Name = "menuLibrarySaveArtwork";
        menuLibrarySaveArtwork.Size = new Size(260, 22);
        menuLibrarySaveArtwork.Text = "Save Artwork...";
        menuLibrarySaveArtwork.Click += menuLibrarySaveArtwork_Click;
        // 
        // menuLibraryExport
        // 
        menuLibraryExport.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryExport.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryExport.Name = "menuLibraryExport";
        menuLibraryExport.Size = new Size(260, 22);
        menuLibraryExport.Text = "Export library (CSV)...";
        menuLibraryExport.Click += menuLibraryExport_Click;
        // 
        // menuLibrarySeparator3
        // 
        menuLibrarySeparator3.BackColor = Color.FromArgb(60, 63, 65);
        menuLibrarySeparator3.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibrarySeparator3.Margin = new Padding(0, 0, 0, 1);
        menuLibrarySeparator3.Name = "menuLibrarySeparator3";
        menuLibrarySeparator3.Size = new Size(257, 6);
        // 
        // menuLibraryGroupBy
        // 
        menuLibraryGroupBy.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryGroupBy.DropDownItems.AddRange(new ToolStripItem[] { menuLibraryGroupNone, menuLibraryGroupFamily, menuLibraryGroupTitleId, menuLibraryGroupCategory, menuLibraryGroupRegion, menuLibraryGroupSource, menuLibraryGroupFirmware });
        menuLibraryGroupBy.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryGroupBy.Name = "menuLibraryGroupBy";
        menuLibraryGroupBy.Size = new Size(260, 22);
        menuLibraryGroupBy.Text = "Group by";
        // 
        // menuLibraryGroupNone
        // 
        menuLibraryGroupNone.Name = "menuLibraryGroupNone";
        menuLibraryGroupNone.Size = new Size(171, 22);
        menuLibraryGroupNone.Text = "None";
        menuLibraryGroupNone.Click += menuLibraryGroupNone_Click;
        // 
        // menuLibraryGroupTitleId
        // 
        menuLibraryGroupTitleId.Name = "menuLibraryGroupTitleId";
        menuLibraryGroupTitleId.Size = new Size(171, 22);
        menuLibraryGroupTitleId.Text = "Title ID";
        menuLibraryGroupTitleId.Click += menuLibraryGroupTitleId_Click;
        // 
        // menuLibraryGroupFamily
        // 
        menuLibraryGroupFamily.Name = "menuLibraryGroupFamily";
        menuLibraryGroupFamily.Size = new Size(171, 22);
        menuLibraryGroupFamily.Text = "Family (base + updates + DLC)";
        menuLibraryGroupFamily.Click += menuLibraryGroupFamily_Click;
        // 
        // menuLibraryGroupCategory
        // 
        menuLibraryGroupCategory.Name = "menuLibraryGroupCategory";
        menuLibraryGroupCategory.Size = new Size(171, 22);
        menuLibraryGroupCategory.Text = "Category";
        menuLibraryGroupCategory.Click += menuLibraryGroupCategory_Click;
        // 
        // menuLibraryGroupRegion
        // 
        menuLibraryGroupRegion.Name = "menuLibraryGroupRegion";
        menuLibraryGroupRegion.Size = new Size(171, 22);
        menuLibraryGroupRegion.Text = "Region";
        menuLibraryGroupRegion.Click += menuLibraryGroupRegion_Click;
        // 
        // menuLibraryGroupSource
        // 
        menuLibraryGroupSource.Name = "menuLibraryGroupSource";
        menuLibraryGroupSource.Size = new Size(171, 22);
        menuLibraryGroupSource.Text = "Source format";
        menuLibraryGroupSource.Click += menuLibraryGroupSource_Click;
        // 
        // menuLibraryGroupFirmware
        // 
        menuLibraryGroupFirmware.Name = "menuLibraryGroupFirmware";
        menuLibraryGroupFirmware.Size = new Size(171, 22);
        menuLibraryGroupFirmware.Text = "Required firmware";
        menuLibraryGroupFirmware.Click += menuLibraryGroupFirmware_Click;
        // 
        // menuLibraryDuplicates
        // 
        menuLibraryDuplicates.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryDuplicates.Enabled = false;
        menuLibraryDuplicates.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryDuplicates.Name = "menuLibraryDuplicates";
        menuLibraryDuplicates.Size = new Size(260, 22);
        menuLibraryDuplicates.Text = "Find Duplicates (Coming soon)";
        menuLibraryDuplicates.Click += menuLibraryDuplicates_Click;
        // 
        // menuLibrarySeparator5
        // 
        menuLibrarySeparator5.BackColor = Color.FromArgb(60, 63, 65);
        menuLibrarySeparator5.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibrarySeparator5.Margin = new Padding(0, 0, 0, 1);
        menuLibrarySeparator5.Name = "menuLibrarySeparator5";
        menuLibrarySeparator5.Size = new Size(257, 6);
        // 
        // menuLibraryGroupExport
        // 
        menuLibraryGroupExport.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryGroupExport.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryGroupExport.Name = "menuLibraryGroupExport";
        menuLibraryGroupExport.Size = new Size(260, 22);
        menuLibraryGroupExport.Text = "Export Group (CSV)...";
        menuLibraryGroupExport.Click += menuLibraryGroupExport_Click;
        // 
        // menuLibraryGroupArtwork
        // 
        menuLibraryGroupArtwork.BackColor = Color.FromArgb(60, 63, 65);
        menuLibraryGroupArtwork.ForeColor = Color.FromArgb(220, 220, 220);
        menuLibraryGroupArtwork.Name = "menuLibraryGroupArtwork";
        menuLibraryGroupArtwork.Size = new Size(260, 22);
        menuLibraryGroupArtwork.Text = "Save Group Artwork...";
        menuLibraryGroupArtwork.Click += menuLibraryGroupArtwork_Click;
        // 
        // searchLibrary
        // 
        searchLibrary.AccessibleDescription = "Search the PS5 library by title, ID, size and more.";
        searchLibrary.AccessibleName = "Library search";
        searchLibrary.Location = new Point(10, 8);
        searchLibrary.Name = "searchLibrary";
        searchLibrary.Placeholder = "Search: title:, id:, size:>50GB, -exclude";
        searchLibrary.Size = new Size(520, 28);
        searchLibrary.TabIndex = 0;
        searchLibrary.SearchTextChanged += searchLibrary_SearchTextChanged;
        // 
        // lblFilterCategory
        // 
        lblFilterCategory.Location = new Point(10, 47);
        lblFilterCategory.Name = "lblFilterCategory";
        lblFilterCategory.Size = new Size(58, 15);
        lblFilterCategory.TabIndex = 1;
        lblFilterCategory.Text = "Category";
        // 
        // cboFilterCategory
        // 
        cboFilterCategory.ItemHeight = 18;
        cboFilterCategory.Items.AddRange(new object[] { "Game", "Patch", "DLC", "App", "Unknown" });
        cboFilterCategory.Location = new Point(72, 42);
        cboFilterCategory.Name = "cboFilterCategory";
        cboFilterCategory.Size = new Size(140, 24);
        cboFilterCategory.TabIndex = 2;
        cboFilterCategory.CheckedItemsChanged += cboFilter_CheckedItemsChanged;
        // 
        // lblFilterRegion
        // 
        lblFilterRegion.Location = new Point(224, 47);
        lblFilterRegion.Name = "lblFilterRegion";
        lblFilterRegion.Size = new Size(44, 15);
        lblFilterRegion.TabIndex = 3;
        lblFilterRegion.Text = "Region";
        // 
        // cboFilterRegion
        // 
        cboFilterRegion.ItemHeight = 18;
        cboFilterRegion.Items.AddRange(new object[] { "Americas", "Europe", "Japan", "Korea", "Asia", "Hong Kong", "Other", "Unknown" });
        cboFilterRegion.Location = new Point(276, 42);
        cboFilterRegion.Name = "cboFilterRegion";
        cboFilterRegion.Size = new Size(150, 24);
        cboFilterRegion.TabIndex = 4;
        cboFilterRegion.CheckedItemsChanged += cboFilter_CheckedItemsChanged;
        // 
        // lblFilterFormat
        // 
        lblFilterFormat.Location = new Point(438, 47);
        lblFilterFormat.Name = "lblFilterFormat";
        lblFilterFormat.Size = new Size(48, 15);
        lblFilterFormat.TabIndex = 5;
        lblFilterFormat.Text = "Format";
        // 
        // cboFilterFormat
        // 
        cboFilterFormat.ItemHeight = 18;
        cboFilterFormat.Items.AddRange(new object[] { "Dump Files", "PKG", "FFPFSC", "exFAT", "FFPKG" });
        cboFilterFormat.Location = new Point(492, 42);
        cboFilterFormat.Name = "cboFilterFormat";
        cboFilterFormat.Size = new Size(160, 24);
        cboFilterFormat.TabIndex = 6;
        cboFilterFormat.CheckedItemsChanged += cboFilter_CheckedItemsChanged;
        // 
        // lblFilterGroup
        // 
        lblFilterGroup.Location = new Point(664, 47);
        lblFilterGroup.Name = "lblFilterGroup";
        lblFilterGroup.Size = new Size(42, 15);
        lblFilterGroup.TabIndex = 7;
        lblFilterGroup.Text = "Group";
        // 
        // cboFilterGroup
        // 
        cboFilterGroup.Items.AddRange(new object[] { "None", "Family", "Title ID", "Category", "Region", "Format", "Firmware" });
        cboFilterGroup.Location = new Point(712, 42);
        cboFilterGroup.Name = "cboFilterGroup";
        cboFilterGroup.Size = new Size(140, 24);
        cboFilterGroup.TabIndex = 8;
        cboFilterGroup.SelectedIndexChanged += cboFilterGroup_SelectedIndexChanged;
        // 
        // btnFilterClear
        // 
        btnFilterClear.AccessibleDescription = "Clear the search query and all checkbox filters.";
        btnFilterClear.AccessibleName = "Reset filters";
        btnFilterClear.Location = new Point(772, 8);
        btnFilterClear.Name = "btnFilterClear";
        btnFilterClear.Size = new Size(110, 28);
        btnFilterClear.TabIndex = 13;
        btnFilterClear.Text = "Clear all";
        btnFilterClear.Click += btnFilterClear_Click;
        // 
        // chipsFilter
        // 
        chipsFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        chipsFilter.Location = new Point(10, 78);
        chipsFilter.Name = "chipsFilter";
        chipsFilter.Padding = new Padding(2);
        chipsFilter.Size = new Size(1364, 30);
        chipsFilter.TabIndex = 15;
        // 
        // lblFilterPreset
        // 
        lblFilterPreset.Location = new Point(560, 14);
        lblFilterPreset.Name = "lblFilterPreset";
        lblFilterPreset.Size = new Size(46, 15);
        lblFilterPreset.TabIndex = 16;
        lblFilterPreset.Text = "Preset";
        // 
        // cboFilterPreset
        // 
        cboFilterPreset.Items.AddRange(new object[] { "Presets...", "All", "Games", "Patches", "DLC", "Dumps", "PKG", "FFPKG", "FFPFSC", "exFAT" });
        cboFilterPreset.Location = new Point(612, 8);
        cboFilterPreset.Name = "cboFilterPreset";
        cboFilterPreset.Size = new Size(150, 24);
        cboFilterPreset.TabIndex = 17;
        cboFilterPreset.SelectedIndexChanged += cboFilterPreset_SelectedIndexChanged;
        // 
        // lblFilterEmpty
        // 
        lblFilterEmpty.Dock = DockStyle.Fill;
        lblFilterEmpty.Location = new Point(0, 114);
        lblFilterEmpty.Name = "lblFilterEmpty";
        lblFilterEmpty.Size = new Size(1384, 297);
        lblFilterEmpty.TabIndex = 18;
        lblFilterEmpty.Text = "No games match the current filters.";
        lblFilterEmpty.TextAlign = ContentAlignment.MiddleCenter;
        lblFilterEmpty.Visible = false;
        // 
        // splitMain
        // 
        splitMain.Controls.Add(splitMainPane1);
        splitMain.Controls.Add(splitMainPane2);
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 24);
        splitMain.Name = "splitMain";
        splitMain.Orientation = DarkUI.Controls.DarkSplitContainer.DarkSplitOrientation.Horizontal;
        splitMain.Size = new Size(1384, 827);
        splitMain.TabIndex = 2;
        // 
        // splitMainPane1
        // 
        splitMainPane1.Controls.Add(gridLibrary);
        splitMainPane1.Controls.Add(lblFilterEmpty);
        splitMainPane1.Controls.Add(searchLibrary);
        splitMainPane1.Controls.Add(lblFilterPreset);
        splitMainPane1.Controls.Add(cboFilterPreset);
        splitMainPane1.Controls.Add(lblFilterCategory);
        splitMainPane1.Controls.Add(cboFilterCategory);
        splitMainPane1.Controls.Add(lblFilterRegion);
        splitMainPane1.Controls.Add(cboFilterRegion);
        splitMainPane1.Controls.Add(lblFilterFormat);
        splitMainPane1.Controls.Add(cboFilterFormat);
        splitMainPane1.Controls.Add(lblFilterGroup);
        splitMainPane1.Controls.Add(cboFilterGroup);
        splitMainPane1.Controls.Add(btnFilterClear);
        splitMainPane1.Controls.Add(chipsFilter);
        splitMainPane1.Location = new Point(0, 0);
        splitMainPane1.Name = "splitMainPane1";
        splitMainPane1.Padding = new Padding(0, 114, 0, 0);
        splitMainPane1.Size = new Size(1384, 411);
        splitMainPane1.TabIndex = 0;
        // 
        // gridLibrary
        // 
        gridLibrary.AllowUserToAddRows = false;
        gridLibrary.AllowUserToDeleteRows = false;
        gridLibrary.AllowUserToDragDropRows = false;
        gridLibrary.AllowUserToOrderColumns = true;
        gridLibrary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLibrary.AutoSortGroups = true;
        gridLibrary.ContextMenuStrip = contextLibrary;
        gridLibrary.Dock = DockStyle.Fill;
        gridLibrary.GroupCellValueComparer = null;
        gridLibrary.GroupHeaderColumnIndex = 0;
        gridLibrary.AccessibleDescription = "The scanned PS5 library. Right-click for actions such as rename, move and export.";
        gridLibrary.AccessibleName = "PS5 library";
        gridLibrary.GroupHeaderColumnName = null;
        gridLibrary.GroupHeaderHeight = 26F;
        gridLibrary.GroupLabelFormatter = null;
        gridLibrary.Location = new Point(0, 114);
        gridLibrary.Name = "gridLibrary";
        gridLibrary.ReadOnly = true;
        gridLibrary.Size = new Size(1384, 297);
        gridLibrary.TabIndex = 0;
        gridLibrary.CellDoubleClick += gridLibrary_CellDoubleClick;
        gridLibrary.ColumnHeaderMouseClick += gridLibrary_ColumnHeaderMouseClick;
        gridLibrary.SelectionChanged += gridLibrary_SelectionChanged;
        gridLibrary.MouseDown += gridLibrary_MouseDown;
        // 
        // splitMainPane2
        // 
        splitMainPane2.Controls.Add(tabsWorkspace);
        splitMainPane2.Location = new Point(0, 416);
        splitMainPane2.Name = "splitMainPane2";
        splitMainPane2.Size = new Size(1384, 411);
        splitMainPane2.TabIndex = 1;
        // 
        // tabsWorkspace
        // 
        tabsWorkspace.AllowDrop = true;
        tabsWorkspace.Controls.Add(tabWorkspaceGeneral);
        tabsWorkspace.Controls.Add(tabWorkspaceTools);
        tabsWorkspace.Controls.Add(tabTasks);
        tabsWorkspace.AccessibleDescription = "Switch between overview, files, tasks, log and other detail panes.";
        tabsWorkspace.AccessibleName = "Details workspace";
        tabsWorkspace.Controls.Add(tabLog);
        tabsWorkspace.Dock = DockStyle.Fill;
        tabsWorkspace.ItemSize = new Size(80, 28);
        tabsWorkspace.Location = new Point(0, 0);
        tabsWorkspace.Name = "tabsWorkspace";
        tabsWorkspace.Padding = new Point(0, 0);
        tabsWorkspace.SelectedIndex = 0;
        tabsWorkspace.Size = new Size(1384, 411);
        tabsWorkspace.TabIndex = 0;
        // 
        // tabWorkspaceGeneral
        // 
        tabWorkspaceGeneral.BackColor = Color.FromArgb(60, 63, 65);
        tabWorkspaceGeneral.Controls.Add(tabsDetails);
        tabWorkspaceGeneral.Location = new Point(4, 32);
        tabWorkspaceGeneral.Name = "tabWorkspaceGeneral";
        tabWorkspaceGeneral.Size = new Size(1376, 375);
        tabWorkspaceGeneral.TabIndex = 0;
        tabWorkspaceGeneral.Text = "General";
        // 
        // tabsDetails
        // 
        tabsDetails.AllowDrop = true;
        tabsDetails.Controls.Add(tabOverview);
        tabsDetails.Controls.Add(tabArtwork);
        tabsDetails.Controls.Add(tabTrophies);
        tabsDetails.Controls.Add(tabActivities);
        tabsDetails.Controls.Add(tabFiles);
        tabsDetails.Controls.Add(tabExecutable);
        tabsDetails.Controls.Add(tabRaw);
        tabsDetails.Controls.Add(tabPackage);
        tabsDetails.Dock = DockStyle.Fill;
        tabsDetails.ItemSize = new Size(125, 28);
        tabsDetails.Location = new Point(0, 0);
        tabsDetails.Name = "tabsDetails";
        tabsDetails.Padding = new Point(0, 0);
        tabsDetails.SelectedIndex = 0;
        tabsDetails.Size = new Size(1376, 375);
        tabsDetails.TabIndex = 0;
        tabsDetails.SelectedIndexChanged += tabsDetails_SelectedIndexChanged;
        // 
        // tabOverview
        // 
        tabOverview.BackColor = Color.FromArgb(60, 63, 65);
        tabOverview.Controls.Add(gridOverview);
        tabOverview.Controls.Add(btnOverviewCopySelected);
        tabOverview.Controls.Add(btnOverviewCopyAll);
        tabOverview.Location = new Point(4, 32);
        tabOverview.Name = "tabOverview";
        tabOverview.Padding = new Padding(0, 38, 0, 0);
        tabOverview.Size = new Size(1368, 339);
        tabOverview.TabIndex = 0;
        tabOverview.Text = "Overview";
        // 
        // gridOverview
        // 
        gridOverview.AllowUserToAddRows = false;
        gridOverview.AllowUserToDeleteRows = false;
        gridOverview.AllowUserToDragDropRows = false;
        gridOverview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridOverview.AutoSortGroups = true;
        gridOverview.Dock = DockStyle.Fill;
        gridOverview.GroupCellValueComparer = null;
        gridOverview.GroupHeaderColumnIndex = 0;
        gridOverview.GroupHeaderColumnName = null;
        gridOverview.GroupHeaderHeight = 26F;
        gridOverview.GroupLabelFormatter = null;
        gridOverview.Location = new Point(0, 38);
        gridOverview.MultiSelect = false;
        gridOverview.Name = "gridOverview";
        gridOverview.ReadOnly = true;
        gridOverview.Size = new Size(1368, 301);
        gridOverview.TabIndex = 0;
        gridOverview.SelectionChanged += gridOverview_SelectionChanged;
        // 
        // btnOverviewCopySelected
        // 
        btnOverviewCopySelected.Location = new Point(110, 6);
        btnOverviewCopySelected.Name = "btnOverviewCopySelected";
        btnOverviewCopySelected.Size = new Size(120, 26);
        btnOverviewCopySelected.TabIndex = 1;
        btnOverviewCopySelected.Text = "Copy Selected";
        btnOverviewCopySelected.Click += btnOverviewCopySelected_Click;
        // 
        // btnOverviewCopyAll
        // 
        btnOverviewCopyAll.Location = new Point(8, 6);
        btnOverviewCopyAll.Name = "btnOverviewCopyAll";
        btnOverviewCopyAll.Size = new Size(96, 26);
        btnOverviewCopyAll.TabIndex = 0;
        btnOverviewCopyAll.Text = "Copy All";
        btnOverviewCopyAll.Click += btnOverviewCopyAll_Click;
        // 
        // tabArtwork
        // 
        tabArtwork.BackColor = Color.FromArgb(60, 63, 65);
        tabArtwork.Controls.Add(splitArtwork);
        tabArtwork.Controls.Add(btnArtworkSaveAll);
        tabArtwork.Location = new Point(4, 32);
        tabArtwork.Name = "tabArtwork";
        tabArtwork.Padding = new Padding(0, 38, 0, 0);
        tabArtwork.Size = new Size(1368, 339);
        tabArtwork.TabIndex = 1;
        tabArtwork.Text = "Artwork";
        // 
        // splitArtwork
        // 
        splitArtwork.Controls.Add(splitArtworkPane1);
        splitArtwork.Controls.Add(splitArtworkPane2);
        splitArtwork.Dock = DockStyle.Fill;
        splitArtwork.Location = new Point(0, 38);
        splitArtwork.Name = "splitArtwork";
        splitArtwork.Size = new Size(1368, 301);
        splitArtwork.TabIndex = 0;
        // 
        // splitArtworkPane1
        // 
        splitArtworkPane1.Controls.Add(sectionIcon);
        splitArtworkPane1.Location = new Point(0, 0);
        splitArtworkPane1.Name = "splitArtworkPane1";
        splitArtworkPane1.Size = new Size(681, 301);
        splitArtworkPane1.TabIndex = 0;
        // 
        // sectionIcon
        // 
        sectionIcon.Controls.Add(pictureIcon);
        sectionIcon.Dock = DockStyle.Fill;
        sectionIcon.Location = new Point(0, 0);
        sectionIcon.Margin = new Padding(6);
        sectionIcon.Name = "sectionIcon";
        sectionIcon.SectionHeader = "Icon (512 �- 512)";
        sectionIcon.Size = new Size(681, 301);
        sectionIcon.TabIndex = 0;
        // 
        // pictureIcon
        // 
        pictureIcon.BackColor = Color.FromArgb(45, 45, 48);
        pictureIcon.ContextMenuStrip = contextArtwork;
        pictureIcon.Dock = DockStyle.Fill;
        pictureIcon.Location = new Point(1, 25);
        pictureIcon.Name = "pictureIcon";
        pictureIcon.Size = new Size(679, 275);
        pictureIcon.SizeMode = PictureBoxSizeMode.Zoom;
        pictureIcon.TabIndex = 0;
        pictureIcon.TabStop = false;
        // 
        // contextArtwork
        // 
        contextArtwork.Items.AddRange(new ToolStripItem[] { menuArtworkSaveThis, menuArtworkSaveAll });
        contextArtwork.Name = "contextArtwork";
        contextArtwork.Size = new Size(170, 48);
        // 
        // menuArtworkSaveThis
        // 
        menuArtworkSaveThis.BackColor = Color.FromArgb(60, 63, 65);
        menuArtworkSaveThis.ForeColor = Color.FromArgb(220, 220, 220);
        menuArtworkSaveThis.Name = "menuArtworkSaveThis";
        menuArtworkSaveThis.Size = new Size(169, 22);
        menuArtworkSaveThis.Text = "Save This Image...";
        menuArtworkSaveThis.Click += menuArtworkSaveThis_Click;
        // 
        // menuArtworkSaveAll
        // 
        menuArtworkSaveAll.BackColor = Color.FromArgb(60, 63, 65);
        menuArtworkSaveAll.ForeColor = Color.FromArgb(220, 220, 220);
        menuArtworkSaveAll.Name = "menuArtworkSaveAll";
        menuArtworkSaveAll.Size = new Size(169, 22);
        menuArtworkSaveAll.Text = "Save All Artwork...";
        menuArtworkSaveAll.Click += btnArtworkSaveAll_Click;
        // 
        // splitArtworkPane2
        // 
        splitArtworkPane2.Controls.Add(sectionBackground);
        splitArtworkPane2.Location = new Point(686, 0);
        splitArtworkPane2.Name = "splitArtworkPane2";
        splitArtworkPane2.Size = new Size(682, 301);
        splitArtworkPane2.TabIndex = 1;
        // 
        // sectionBackground
        // 
        sectionBackground.Controls.Add(tabsBackgrounds);
        sectionBackground.Dock = DockStyle.Fill;
        sectionBackground.Location = new Point(0, 0);
        sectionBackground.Margin = new Padding(6);
        sectionBackground.Name = "sectionBackground";
        sectionBackground.SectionHeader = "Background Art (PIC0 / PIC1 / PIC2)";
        sectionBackground.Size = new Size(682, 301);
        sectionBackground.TabIndex = 0;
        // 
        // tabsBackgrounds
        // 
        tabsBackgrounds.AllowDrop = true;
        tabsBackgrounds.Controls.Add(tabPic0);
        tabsBackgrounds.Controls.Add(tabPic1);
        tabsBackgrounds.Controls.Add(tabPic2);
        tabsBackgrounds.Dock = DockStyle.Fill;
        tabsBackgrounds.ItemSize = new Size(80, 28);
        tabsBackgrounds.Location = new Point(1, 25);
        tabsBackgrounds.Name = "tabsBackgrounds";
        tabsBackgrounds.Padding = new Point(0, 0);
        tabsBackgrounds.SelectedIndex = 0;
        tabsBackgrounds.Size = new Size(680, 275);
        tabsBackgrounds.TabIndex = 0;
        // 
        // tabPic0
        // 
        tabPic0.BackColor = Color.FromArgb(60, 63, 65);
        tabPic0.Controls.Add(pictureBackground0);
        tabPic0.Location = new Point(4, 32);
        tabPic0.Name = "tabPic0";
        tabPic0.Size = new Size(672, 239);
        tabPic0.TabIndex = 0;
        tabPic0.Text = "PIC0";
        // 
        // pictureBackground0
        // 
        pictureBackground0.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground0.ContextMenuStrip = contextArtwork;
        pictureBackground0.Dock = DockStyle.Fill;
        pictureBackground0.Location = new Point(0, 0);
        pictureBackground0.Name = "pictureBackground0";
        pictureBackground0.Size = new Size(672, 239);
        pictureBackground0.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBackground0.TabIndex = 0;
        pictureBackground0.TabStop = false;
        // 
        // tabPic1
        // 
        tabPic1.BackColor = Color.FromArgb(60, 63, 65);
        tabPic1.Controls.Add(pictureBackground1);
        tabPic1.Location = new Point(4, 32);
        tabPic1.Name = "tabPic1";
        tabPic1.Size = new Size(672, 239);
        tabPic1.TabIndex = 1;
        tabPic1.Text = "PIC1";
        // 
        // pictureBackground1
        // 
        pictureBackground1.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground1.ContextMenuStrip = contextArtwork;
        pictureBackground1.Dock = DockStyle.Fill;
        pictureBackground1.Location = new Point(0, 0);
        pictureBackground1.Name = "pictureBackground1";
        pictureBackground1.Size = new Size(672, 239);
        pictureBackground1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBackground1.TabIndex = 0;
        pictureBackground1.TabStop = false;
        // 
        // tabPic2
        // 
        tabPic2.BackColor = Color.FromArgb(60, 63, 65);
        tabPic2.Controls.Add(pictureBackground2);
        tabPic2.Location = new Point(4, 32);
        tabPic2.Name = "tabPic2";
        tabPic2.Size = new Size(672, 239);
        tabPic2.TabIndex = 2;
        tabPic2.Text = "PIC2";
        // 
        // pictureBackground2
        // 
        pictureBackground2.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground2.ContextMenuStrip = contextArtwork;
        pictureBackground2.Dock = DockStyle.Fill;
        pictureBackground2.Location = new Point(0, 0);
        pictureBackground2.Name = "pictureBackground2";
        pictureBackground2.Size = new Size(672, 239);
        pictureBackground2.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBackground2.TabIndex = 0;
        pictureBackground2.TabStop = false;
        // 
        // btnArtworkSaveAll
        // 
        btnArtworkSaveAll.Location = new Point(8, 6);
        btnArtworkSaveAll.Name = "btnArtworkSaveAll";
        btnArtworkSaveAll.Size = new Size(110, 26);
        btnArtworkSaveAll.TabIndex = 0;
        btnArtworkSaveAll.Text = "Save All...";
        btnArtworkSaveAll.Click += btnArtworkSaveAll_Click;
        // 
        // tabTrophies
        // 
        tabTrophies.BackColor = Color.FromArgb(60, 63, 65);
        tabTrophies.Controls.Add(gridTrophies);
        tabTrophies.Controls.Add(lblTrophySummary);
        tabTrophies.Controls.Add(searchTrophy);
        tabTrophies.Controls.Add(chkTrophyShowHidden);
        tabTrophies.Controls.Add(chkTrophyBronze);
        tabTrophies.Controls.Add(chkTrophySilver);
        tabTrophies.Controls.Add(chkTrophyGold);
        tabTrophies.Controls.Add(chkTrophyPlatinum);
        tabTrophies.Controls.Add(btnTrophyExportCsv);
        tabTrophies.Controls.Add(btnTrophySaveIcons);
        tabTrophies.Location = new Point(4, 32);
        tabTrophies.Name = "tabTrophies";
        tabTrophies.Padding = new Padding(0, 74, 0, 0);
        tabTrophies.Size = new Size(1368, 339);
        tabTrophies.TabIndex = 2;
        tabTrophies.Text = "Trophies";
        // 
        // gridTrophies
        // 
        gridTrophies.AllowUserToAddRows = false;
        gridTrophies.AllowUserToDeleteRows = false;
        gridTrophies.AllowUserToDragDropRows = false;
        gridTrophies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridTrophies.AutoSortGroups = true;
        gridTrophies.ContextMenuStrip = contextTrophies;
        gridTrophies.Dock = DockStyle.Fill;
        gridTrophies.GroupCellValueComparer = null;
        gridTrophies.GroupHeaderColumnIndex = 0;
        gridTrophies.GroupHeaderColumnName = null;
        gridTrophies.GroupHeaderHeight = 26F;
        gridTrophies.GroupLabelFormatter = null;
        gridTrophies.Location = new Point(0, 74);
        gridTrophies.Name = "gridTrophies";
        gridTrophies.ReadOnly = true;
        gridTrophies.RowTemplate.Height = 48;
        gridTrophies.Size = new Size(1368, 265);
        gridTrophies.TabIndex = 0;
        // 
        // contextTrophies
        // 
        contextTrophies.Items.AddRange(new ToolStripItem[] { menuTrophySaveIcon, menuTrophySaveAllIcons, menuTrophyExportCsv });
        contextTrophies.Name = "contextTrophies";
        contextTrophies.Size = new Size(156, 70);
        // 
        // menuTrophySaveIcon
        // 
        menuTrophySaveIcon.BackColor = Color.FromArgb(60, 63, 65);
        menuTrophySaveIcon.ForeColor = Color.FromArgb(220, 220, 220);
        menuTrophySaveIcon.Name = "menuTrophySaveIcon";
        menuTrophySaveIcon.Size = new Size(155, 22);
        menuTrophySaveIcon.Text = "Save Icon...";
        menuTrophySaveIcon.Click += menuTrophySaveIcon_Click;
        // 
        // menuTrophySaveAllIcons
        // 
        menuTrophySaveAllIcons.BackColor = Color.FromArgb(60, 63, 65);
        menuTrophySaveAllIcons.ForeColor = Color.FromArgb(220, 220, 220);
        menuTrophySaveAllIcons.Name = "menuTrophySaveAllIcons";
        menuTrophySaveAllIcons.Size = new Size(155, 22);
        menuTrophySaveAllIcons.Text = "Save All Icons...";
        menuTrophySaveAllIcons.Click += btnTrophySaveIcons_Click;
        // 
        // menuTrophyExportCsv
        // 
        menuTrophyExportCsv.BackColor = Color.FromArgb(60, 63, 65);
        menuTrophyExportCsv.ForeColor = Color.FromArgb(220, 220, 220);
        menuTrophyExportCsv.Name = "menuTrophyExportCsv";
        menuTrophyExportCsv.Size = new Size(155, 22);
        menuTrophyExportCsv.Text = "Export CSV...";
        menuTrophyExportCsv.Click += btnTrophyExportCsv_Click;
        // 
        // lblTrophySummary
        // 
        lblTrophySummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblTrophySummary.AutoEllipsis = true;
        lblTrophySummary.Location = new Point(0, 0);
        lblTrophySummary.Name = "lblTrophySummary";
        lblTrophySummary.Padding = new Padding(10, 0, 10, 0);
        lblTrophySummary.Size = new Size(1368, 36);
        lblTrophySummary.TabIndex = 0;
        lblTrophySummary.Text = "Select a game to load trophies.";
        lblTrophySummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // searchTrophy
        // 
        searchTrophy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        searchTrophy.Location = new Point(1070, 41);
        searchTrophy.Name = "searchTrophy";
        searchTrophy.Placeholder = "Search trophies";
        searchTrophy.Size = new Size(290, 28);
        searchTrophy.TabIndex = 7;
        searchTrophy.SearchTextChanged += trophyFilter_Changed;
        // 
        // chkTrophyShowHidden
        // 
        chkTrophyShowHidden.AutoSize = true;
        chkTrophyShowHidden.Checked = true;
        chkTrophyShowHidden.CheckState = CheckState.Checked;
        chkTrophyShowHidden.Location = new Point(509, 47);
        chkTrophyShowHidden.Name = "chkTrophyShowHidden";
        chkTrophyShowHidden.Size = new Size(95, 19);
        chkTrophyShowHidden.TabIndex = 6;
        chkTrophyShowHidden.Text = "Show hidden";
        chkTrophyShowHidden.CheckedChanged += trophyFilter_Changed;
        // 
        // chkTrophyBronze
        // 
        chkTrophyBronze.AutoSize = true;
        chkTrophyBronze.Location = new Point(441, 47);
        chkTrophyBronze.Name = "chkTrophyBronze";
        chkTrophyBronze.Size = new Size(62, 19);
        chkTrophyBronze.TabIndex = 5;
        chkTrophyBronze.Text = "Bronze";
        chkTrophyBronze.CheckedChanged += trophyFilter_Changed;
        // 
        // chkTrophySilver
        // 
        chkTrophySilver.AutoSize = true;
        chkTrophySilver.Location = new Point(381, 47);
        chkTrophySilver.Name = "chkTrophySilver";
        chkTrophySilver.Size = new Size(54, 19);
        chkTrophySilver.TabIndex = 4;
        chkTrophySilver.Text = "Silver";
        chkTrophySilver.CheckedChanged += trophyFilter_Changed;
        // 
        // chkTrophyGold
        // 
        chkTrophyGold.AutoSize = true;
        chkTrophyGold.Location = new Point(324, 47);
        chkTrophyGold.Name = "chkTrophyGold";
        chkTrophyGold.Size = new Size(51, 19);
        chkTrophyGold.TabIndex = 3;
        chkTrophyGold.Text = "Gold";
        chkTrophyGold.CheckedChanged += trophyFilter_Changed;
        // 
        // chkTrophyPlatinum
        // 
        chkTrophyPlatinum.AutoSize = true;
        chkTrophyPlatinum.Location = new Point(244, 47);
        chkTrophyPlatinum.Name = "chkTrophyPlatinum";
        chkTrophyPlatinum.Size = new Size(74, 19);
        chkTrophyPlatinum.TabIndex = 2;
        chkTrophyPlatinum.Text = "Platinum";
        chkTrophyPlatinum.CheckedChanged += trophyFilter_Changed;
        // 
        // btnTrophyExportCsv
        // 
        btnTrophyExportCsv.Location = new Point(118, 42);
        btnTrophyExportCsv.Name = "btnTrophyExportCsv";
        btnTrophyExportCsv.Size = new Size(110, 26);
        btnTrophyExportCsv.TabIndex = 1;
        btnTrophyExportCsv.Text = "Export CSV...";
        btnTrophyExportCsv.Click += btnTrophyExportCsv_Click;
        // 
        // btnTrophySaveIcons
        // 
        btnTrophySaveIcons.Location = new Point(8, 42);
        btnTrophySaveIcons.Name = "btnTrophySaveIcons";
        btnTrophySaveIcons.Size = new Size(104, 26);
        btnTrophySaveIcons.TabIndex = 0;
        btnTrophySaveIcons.Text = "Save Icons...";
        btnTrophySaveIcons.Click += btnTrophySaveIcons_Click;
        // 
        // tabActivities
        // 
        tabActivities.BackColor = Color.FromArgb(60, 63, 65);
        tabActivities.Controls.Add(tabsUds);
        tabActivities.Controls.Add(lblActivitiesSummary);
        tabActivities.Controls.Add(searchUds);
        tabActivities.Controls.Add(btnUdsCopySelected);
        tabActivities.Controls.Add(btnUdsCopyAll);
        tabActivities.Location = new Point(4, 32);
        tabActivities.Name = "tabActivities";
        tabActivities.Padding = new Padding(0, 74, 0, 0);
        tabActivities.Size = new Size(1368, 339);
        tabActivities.TabIndex = 3;
        tabActivities.Text = "Activities & UDS";
        // 
        // tabsUds
        // 
        tabsUds.AllowDrop = true;
        tabsUds.Controls.Add(tabUdsEvents);
        tabsUds.Controls.Add(tabUdsStats);
        tabsUds.Controls.Add(tabUdsEnums);
        tabsUds.Controls.Add(tabUdsRules);
        tabsUds.Dock = DockStyle.Fill;
        tabsUds.ItemSize = new Size(119, 28);
        tabsUds.Location = new Point(0, 74);
        tabsUds.Name = "tabsUds";
        tabsUds.Padding = new Point(0, 0);
        tabsUds.SelectedIndex = 0;
        tabsUds.Size = new Size(1368, 265);
        tabsUds.TabIndex = 0;
        tabsUds.SelectedIndexChanged += tabsUds_SelectedIndexChanged;
        // 
        // tabUdsEvents
        // 
        tabUdsEvents.BackColor = Color.FromArgb(60, 63, 65);
        tabUdsEvents.Controls.Add(splitUdsEvents);
        tabUdsEvents.Location = new Point(4, 32);
        tabUdsEvents.Name = "tabUdsEvents";
        tabUdsEvents.Size = new Size(1360, 229);
        tabUdsEvents.TabIndex = 0;
        tabUdsEvents.Text = "Events";
        // 
        // splitUdsEvents
        // 
        splitUdsEvents.BorderStyle = BorderStyle.FixedSingle;
        splitUdsEvents.Controls.Add(splitUdsEventsPane1);
        splitUdsEvents.Controls.Add(splitUdsEventsPane2);
        splitUdsEvents.Dock = DockStyle.Fill;
        splitUdsEvents.Location = new Point(0, 0);
        splitUdsEvents.Name = "splitUdsEvents";
        splitUdsEvents.Orientation = DarkUI.Controls.DarkSplitContainer.DarkSplitOrientation.Horizontal;
        splitUdsEvents.Size = new Size(1360, 229);
        splitUdsEvents.TabIndex = 0;
        // 
        // splitUdsEventsPane1
        // 
        splitUdsEventsPane1.Controls.Add(gridUdsEvents);
        splitUdsEventsPane1.Location = new Point(0, 0);
        splitUdsEventsPane1.Name = "splitUdsEventsPane1";
        splitUdsEventsPane1.Size = new Size(1358, 111);
        splitUdsEventsPane1.TabIndex = 0;
        // 
        // gridUdsEvents
        // 
        gridUdsEvents.AllowUserToAddRows = false;
        gridUdsEvents.AllowUserToDeleteRows = false;
        gridUdsEvents.AllowUserToDragDropRows = false;
        gridUdsEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUdsEvents.AutoSortGroups = true;
        gridUdsEvents.Dock = DockStyle.Fill;
        gridUdsEvents.GroupCellValueComparer = null;
        gridUdsEvents.GroupHeaderColumnIndex = 0;
        gridUdsEvents.GroupHeaderColumnName = null;
        gridUdsEvents.GroupHeaderHeight = 26F;
        gridUdsEvents.GroupLabelFormatter = null;
        gridUdsEvents.Location = new Point(0, 0);
        gridUdsEvents.Name = "gridUdsEvents";
        gridUdsEvents.ReadOnly = true;
        gridUdsEvents.Size = new Size(1358, 111);
        gridUdsEvents.TabIndex = 0;
        gridUdsEvents.SelectionChanged += gridUdsEvents_SelectionChanged;
        // 
        // splitUdsEventsPane2
        // 
        splitUdsEventsPane2.Controls.Add(gridUdsEventProperties);
        splitUdsEventsPane2.Location = new Point(0, 116);
        splitUdsEventsPane2.Name = "splitUdsEventsPane2";
        splitUdsEventsPane2.Size = new Size(1358, 111);
        splitUdsEventsPane2.TabIndex = 1;
        // 
        // gridUdsEventProperties
        // 
        gridUdsEventProperties.AllowUserToAddRows = false;
        gridUdsEventProperties.AllowUserToDeleteRows = false;
        gridUdsEventProperties.AllowUserToDragDropRows = false;
        gridUdsEventProperties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUdsEventProperties.AutoSortGroups = true;
        gridUdsEventProperties.Dock = DockStyle.Fill;
        gridUdsEventProperties.GroupCellValueComparer = null;
        gridUdsEventProperties.GroupHeaderColumnIndex = 0;
        gridUdsEventProperties.GroupHeaderColumnName = null;
        gridUdsEventProperties.GroupHeaderHeight = 26F;
        gridUdsEventProperties.GroupLabelFormatter = null;
        gridUdsEventProperties.Location = new Point(0, 0);
        gridUdsEventProperties.Name = "gridUdsEventProperties";
        gridUdsEventProperties.ReadOnly = true;
        gridUdsEventProperties.Size = new Size(1358, 111);
        gridUdsEventProperties.TabIndex = 0;
        // 
        // tabUdsStats
        // 
        tabUdsStats.BackColor = Color.FromArgb(60, 63, 65);
        tabUdsStats.Controls.Add(gridUdsStats);
        tabUdsStats.Location = new Point(4, 32);
        tabUdsStats.Name = "tabUdsStats";
        tabUdsStats.Size = new Size(1360, 229);
        tabUdsStats.TabIndex = 1;
        tabUdsStats.Text = "Stats";
        // 
        // gridUdsStats
        // 
        gridUdsStats.AllowUserToAddRows = false;
        gridUdsStats.AllowUserToDeleteRows = false;
        gridUdsStats.AllowUserToDragDropRows = false;
        gridUdsStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUdsStats.AutoSortGroups = true;
        gridUdsStats.Dock = DockStyle.Fill;
        gridUdsStats.GroupCellValueComparer = null;
        gridUdsStats.GroupHeaderColumnIndex = 0;
        gridUdsStats.GroupHeaderColumnName = null;
        gridUdsStats.GroupHeaderHeight = 26F;
        gridUdsStats.GroupLabelFormatter = null;
        gridUdsStats.Location = new Point(0, 0);
        gridUdsStats.Name = "gridUdsStats";
        gridUdsStats.ReadOnly = true;
        gridUdsStats.Size = new Size(1360, 229);
        gridUdsStats.TabIndex = 0;
        // 
        // tabUdsEnums
        // 
        tabUdsEnums.BackColor = Color.FromArgb(60, 63, 65);
        tabUdsEnums.Controls.Add(gridUdsEnums);
        tabUdsEnums.Location = new Point(4, 32);
        tabUdsEnums.Name = "tabUdsEnums";
        tabUdsEnums.Size = new Size(1360, 229);
        tabUdsEnums.TabIndex = 2;
        tabUdsEnums.Text = "Enum groups";
        // 
        // gridUdsEnums
        // 
        gridUdsEnums.AllowUserToAddRows = false;
        gridUdsEnums.AllowUserToDeleteRows = false;
        gridUdsEnums.AllowUserToDragDropRows = false;
        gridUdsEnums.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUdsEnums.AutoSortGroups = true;
        gridUdsEnums.Dock = DockStyle.Fill;
        gridUdsEnums.GroupCellValueComparer = null;
        gridUdsEnums.GroupHeaderColumnIndex = 0;
        gridUdsEnums.GroupHeaderColumnName = null;
        gridUdsEnums.GroupHeaderHeight = 26F;
        gridUdsEnums.GroupLabelFormatter = null;
        gridUdsEnums.Location = new Point(0, 0);
        gridUdsEnums.Name = "gridUdsEnums";
        gridUdsEnums.ReadOnly = true;
        gridUdsEnums.Size = new Size(1360, 229);
        gridUdsEnums.TabIndex = 0;
        // 
        // tabUdsRules
        // 
        tabUdsRules.BackColor = Color.FromArgb(60, 63, 65);
        tabUdsRules.Controls.Add(gridUdsRules);
        tabUdsRules.Location = new Point(4, 32);
        tabUdsRules.Name = "tabUdsRules";
        tabUdsRules.Size = new Size(1360, 229);
        tabUdsRules.TabIndex = 3;
        tabUdsRules.Text = "Extraction rules";
        // 
        // gridUdsRules
        // 
        gridUdsRules.AllowUserToAddRows = false;
        gridUdsRules.AllowUserToDeleteRows = false;
        gridUdsRules.AllowUserToDragDropRows = false;
        gridUdsRules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridUdsRules.AutoSortGroups = true;
        gridUdsRules.Dock = DockStyle.Fill;
        gridUdsRules.GroupCellValueComparer = null;
        gridUdsRules.GroupHeaderColumnIndex = 0;
        gridUdsRules.GroupHeaderColumnName = null;
        gridUdsRules.GroupHeaderHeight = 26F;
        gridUdsRules.GroupLabelFormatter = null;
        gridUdsRules.Location = new Point(0, 0);
        gridUdsRules.Name = "gridUdsRules";
        gridUdsRules.ReadOnly = true;
        gridUdsRules.Size = new Size(1360, 229);
        gridUdsRules.TabIndex = 0;
        // 
        // lblActivitiesSummary
        // 
        lblActivitiesSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblActivitiesSummary.Location = new Point(0, 0);
        lblActivitiesSummary.Name = "lblActivitiesSummary";
        lblActivitiesSummary.Padding = new Padding(10, 0, 10, 0);
        lblActivitiesSummary.Size = new Size(1368, 36);
        lblActivitiesSummary.TabIndex = 0;
        lblActivitiesSummary.Text = "Select a game to load activity definitions.";
        lblActivitiesSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // searchUds
        // 
        searchUds.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        searchUds.Location = new Point(1068, 41);
        searchUds.Name = "searchUds";
        searchUds.Placeholder = "Search events, stats, enums, rules";
        searchUds.Size = new Size(292, 28);
        searchUds.TabIndex = 2;
        searchUds.SearchTextChanged += searchUds_SearchTextChanged;
        // 
        // btnUdsCopySelected
        // 
        btnUdsCopySelected.Location = new Point(110, 42);
        btnUdsCopySelected.Name = "btnUdsCopySelected";
        btnUdsCopySelected.Size = new Size(120, 26);
        btnUdsCopySelected.TabIndex = 1;
        btnUdsCopySelected.Text = "Copy Selected";
        btnUdsCopySelected.Click += btnUdsCopySelected_Click;
        // 
        // btnUdsCopyAll
        // 
        btnUdsCopyAll.Location = new Point(8, 42);
        btnUdsCopyAll.Name = "btnUdsCopyAll";
        btnUdsCopyAll.Size = new Size(96, 26);
        btnUdsCopyAll.TabIndex = 0;
        btnUdsCopyAll.Text = "Copy All";
        btnUdsCopyAll.Click += btnUdsCopyAll_Click;
        // 
        // tabFiles
        // 
        tabFiles.BackColor = Color.FromArgb(60, 63, 65);
        tabFiles.Controls.Add(sectionFileBrowser);
        tabFiles.Controls.Add(lblFilesSummary);
        tabFiles.Controls.Add(btnFileExtractAll);
        tabFiles.Controls.Add(btnFileCancel);
        tabFiles.Location = new Point(4, 32);
        tabFiles.Name = "tabFiles";
        tabFiles.Padding = new Padding(0, 36, 0, 0);
        tabFiles.Size = new Size(1368, 339);
        tabFiles.TabIndex = 4;
        tabFiles.Text = "Files";
        // 
        // sectionFileBrowser
        // 
        sectionFileBrowser.Controls.Add(splitFileBrowser);
        sectionFileBrowser.Dock = DockStyle.Fill;
        sectionFileBrowser.Location = new Point(0, 36);
        sectionFileBrowser.Margin = new Padding(6);
        sectionFileBrowser.Name = "sectionFileBrowser";
        sectionFileBrowser.SectionHeader = "Dump Files";
        sectionFileBrowser.Size = new Size(1368, 303);
        sectionFileBrowser.TabIndex = 0;
        // 
        // splitFileBrowser
        // 
        splitFileBrowser.BorderStyle = BorderStyle.FixedSingle;
        splitFileBrowser.Controls.Add(splitFileBrowserPane1);
        splitFileBrowser.Controls.Add(splitFileBrowserPane2);
        splitFileBrowser.Dock = DockStyle.Fill;
        splitFileBrowser.Location = new Point(1, 25);
        splitFileBrowser.Name = "splitFileBrowser";
        splitFileBrowser.Size = new Size(1366, 277);
        splitFileBrowser.TabIndex = 0;
        splitFileBrowser.SizeChanged += splitFileBrowser_SizeChanged;
        // 
        // splitFileBrowserPane1
        // 
        splitFileBrowserPane1.Controls.Add(treeFiles);
        splitFileBrowserPane1.Location = new Point(0, 0);
        splitFileBrowserPane1.Name = "splitFileBrowserPane1";
        splitFileBrowserPane1.Size = new Size(679, 275);
        splitFileBrowserPane1.TabIndex = 0;
        // 
        // treeFiles
        // 
        treeFiles.CheckBoxes = false;
        treeFiles.ContextMenuStrip = contextTreeFiles;
        treeFiles.Dock = DockStyle.Fill;
        treeFiles.FullRowSelect = false;
        treeFiles.HotTracking = false;
        treeFiles.ImageList = imageListFiles;
        treeFiles.Indent = 19;
        treeFiles.ItemHeight = 24;
        treeFiles.LabelEdit = false;
        treeFiles.Location = new Point(0, 0);
        treeFiles.Name = "treeFiles";
        treeFiles.PathSeparator = "\\";
        treeFiles.Scrollable = true;
        treeFiles.SelectedNode = null;
        treeFiles.ShowLines = true;
        treeFiles.ShowPlusMinus = true;
        treeFiles.ShowRootLines = true;
        treeFiles.Size = new Size(679, 275);
        treeFiles.Sorted = false;
        treeFiles.TabIndex = 0;
        treeFiles.TopNode = null;
        treeFiles.TreeViewNodeSorter = null;
        treeFiles.UseCompatibleStateImageBehavior = false;
        treeFiles.AfterSelect += treeFiles_AfterSelect;
        // 
        // contextTreeFiles
        // 
        contextTreeFiles.Items.AddRange(new ToolStripItem[] { menuTreeExpand, menuTreeCollapse, menuTreeExpandAll, menuTreeCollapseAll, menuTreeSeparator, menuTreeCopyPath, menuTreeExtract });
        contextTreeFiles.Name = "contextTreeFiles";
        contextTreeFiles.Size = new Size(155, 143);
        // 
        // menuTreeExpand
        // 
        menuTreeExpand.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeExpand.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeExpand.Name = "menuTreeExpand";
        menuTreeExpand.Size = new Size(154, 22);
        menuTreeExpand.Text = "Expand";
        menuTreeExpand.Click += menuTreeExpand_Click;
        // 
        // menuTreeCollapse
        // 
        menuTreeCollapse.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeCollapse.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeCollapse.Name = "menuTreeCollapse";
        menuTreeCollapse.Size = new Size(154, 22);
        menuTreeCollapse.Text = "Collapse";
        menuTreeCollapse.Click += menuTreeCollapse_Click;
        // 
        // menuTreeExpandAll
        // 
        menuTreeExpandAll.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeExpandAll.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeExpandAll.Name = "menuTreeExpandAll";
        menuTreeExpandAll.Size = new Size(154, 22);
        menuTreeExpandAll.Text = "Expand All";
        menuTreeExpandAll.Click += menuTreeExpandAll_Click;
        // 
        // menuTreeCollapseAll
        // 
        menuTreeCollapseAll.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeCollapseAll.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeCollapseAll.Name = "menuTreeCollapseAll";
        menuTreeCollapseAll.Size = new Size(154, 22);
        menuTreeCollapseAll.Text = "Collapse All";
        menuTreeCollapseAll.Click += menuTreeCollapseAll_Click;
        // 
        // menuTreeSeparator
        // 
        menuTreeSeparator.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeSeparator.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeSeparator.Margin = new Padding(0, 0, 0, 1);
        menuTreeSeparator.Name = "menuTreeSeparator";
        menuTreeSeparator.Size = new Size(151, 6);
        // 
        // menuTreeCopyPath
        // 
        menuTreeCopyPath.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeCopyPath.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeCopyPath.Name = "menuTreeCopyPath";
        menuTreeCopyPath.Size = new Size(154, 22);
        menuTreeCopyPath.Text = "Copy Path";
        menuTreeCopyPath.Click += menuTreeCopyPath_Click;
        // 
        // menuTreeExtract
        // 
        menuTreeExtract.BackColor = Color.FromArgb(60, 63, 65);
        menuTreeExtract.ForeColor = Color.FromArgb(220, 220, 220);
        menuTreeExtract.Name = "menuTreeExtract";
        menuTreeExtract.Size = new Size(154, 22);
        menuTreeExtract.Text = "Extract Folder...";
        menuTreeExtract.Click += menuTreeExtract_Click;
        // 
        // splitFileBrowserPane2
        // 
        splitFileBrowserPane2.Controls.Add(splitFileContentPreview);
        splitFileBrowserPane2.Location = new Point(684, 0);
        splitFileBrowserPane2.Name = "splitFileBrowserPane2";
        splitFileBrowserPane2.Size = new Size(680, 275);
        splitFileBrowserPane2.TabIndex = 1;
        // 
        // splitFileContentPreview
        // 
        splitFileContentPreview.BorderStyle = BorderStyle.FixedSingle;
        splitFileContentPreview.Controls.Add(splitFileContentPreviewPane1);
        splitFileContentPreview.Controls.Add(splitFileContentPreviewPane2);
        splitFileContentPreview.Dock = DockStyle.Fill;
        splitFileContentPreview.Location = new Point(0, 0);
        splitFileContentPreview.Name = "splitFileContentPreview";
        splitFileContentPreview.Size = new Size(680, 275);
        splitFileContentPreview.TabIndex = 1;
        // 
        // splitFileContentPreviewPane1
        // 
        splitFileContentPreviewPane1.Controls.Add(fileListPanel);
        splitFileContentPreviewPane1.Location = new Point(0, 0);
        splitFileContentPreviewPane1.Name = "splitFileContentPreviewPane1";
        splitFileContentPreviewPane1.Size = new Size(336, 273);
        splitFileContentPreviewPane1.TabIndex = 0;
        // 
        // fileListPanel
        // 
        fileListPanel.Controls.Add(searchFileFilter);
        fileListPanel.Controls.Add(listFiles);
        fileListPanel.Dock = DockStyle.Fill;
        fileListPanel.Location = new Point(0, 0);
        fileListPanel.Name = "fileListPanel";
        fileListPanel.Padding = new Padding(0, 36, 0, 0);
        fileListPanel.Size = new Size(336, 273);
        fileListPanel.Surface = DarkUI.Controls.DarkPanelSurface.MediumBackground;
        fileListPanel.TabIndex = 0;
        // 
        // searchFileFilter
        // 
        searchFileFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        searchFileFilter.Location = new Point(4, 4);
        searchFileFilter.Name = "searchFileFilter";
        searchFileFilter.Placeholder = "Search files (all folders)";
        searchFileFilter.Size = new Size(328, 28);
        searchFileFilter.TabIndex = 0;
        searchFileFilter.SearchTextChanged += searchFileFilter_SearchTextChanged;
        // 
        // listFiles
        // 
        listFiles.ContextMenuStrip = contextFiles;
        listFiles.Dock = DockStyle.Fill;
        listFiles.FullRowSelect = true;
        listFiles.HeaderStyle = ColumnHeaderStyle.Clickable;
        listFiles.LargeImageList = null;
        listFiles.ListViewItemSorter = null;
        listFiles.Location = new Point(0, 36);
        listFiles.MultiSelect = true;
        listFiles.Name = "listFiles";
        listFiles.Size = new Size(336, 237);
        listFiles.SmallImageList = imageListFiles;
        listFiles.TabIndex = 1;
        listFiles.UseCompatibleStateImageBehavior = false;
        listFiles.View = View.Details;
        listFiles.ItemActivate += listFiles_ItemActivate;
        listFiles.ColumnClick += listFiles_ColumnClick;
        listFiles.ItemDrag += listFiles_ItemDrag;
        listFiles.SelectedIndexChanged += listFiles_SelectedIndexChanged;
        listFiles.MouseDown += listFiles_MouseDown;
        // 
        // contextFiles
        // 
        contextFiles.Items.AddRange(new ToolStripItem[] { menuFileOpenContained, menuFileExtractContained, menuFileExtractSelected, menuFileCopySeparator, menuFileCopyPath, menuFileCopyName, menuFileContainerSeparator, menuFileRevealContainer });
        contextFiles.Name = "contextFiles";
        contextFiles.Size = new Size(239, 150);
        contextFiles.Opening += contextFiles_Opening;
        // 
        // menuFileOpenContained
        // 
        menuFileOpenContained.BackColor = Color.FromArgb(60, 63, 65);
        menuFileOpenContained.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileOpenContained.Name = "menuFileOpenContained";
        menuFileOpenContained.Size = new Size(238, 22);
        menuFileOpenContained.Text = "Open / Preview";
        menuFileOpenContained.Click += menuFileOpenContained_Click;
        // 
        // menuFileExtractContained
        // 
        menuFileExtractContained.BackColor = Color.FromArgb(60, 63, 65);
        menuFileExtractContained.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileExtractContained.Name = "menuFileExtractContained";
        menuFileExtractContained.Size = new Size(238, 22);
        menuFileExtractContained.Text = "Extract Selected File...";
        menuFileExtractContained.Click += menuFileExtractContained_Click;
        // 
        // menuFileExtractSelected
        // 
        menuFileExtractSelected.BackColor = Color.FromArgb(60, 63, 65);
        menuFileExtractSelected.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileExtractSelected.Name = "menuFileExtractSelected";
        menuFileExtractSelected.Size = new Size(238, 22);
        menuFileExtractSelected.Text = "Extract Selected (with folders)...";
        menuFileExtractSelected.Click += menuFileExtractSelected_Click;
        // 
        // menuFileCopySeparator
        // 
        menuFileCopySeparator.BackColor = Color.FromArgb(60, 63, 65);
        menuFileCopySeparator.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileCopySeparator.Margin = new Padding(0, 0, 0, 1);
        menuFileCopySeparator.Name = "menuFileCopySeparator";
        menuFileCopySeparator.Size = new Size(235, 6);
        // 
        // menuFileCopyPath
        // 
        menuFileCopyPath.BackColor = Color.FromArgb(60, 63, 65);
        menuFileCopyPath.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileCopyPath.Name = "menuFileCopyPath";
        menuFileCopyPath.Size = new Size(238, 22);
        menuFileCopyPath.Text = "Copy Path";
        menuFileCopyPath.Click += menuFileCopyPath_Click;
        // 
        // menuFileCopyName
        // 
        menuFileCopyName.BackColor = Color.FromArgb(60, 63, 65);
        menuFileCopyName.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileCopyName.Name = "menuFileCopyName";
        menuFileCopyName.Size = new Size(238, 22);
        menuFileCopyName.Text = "Copy Filename";
        menuFileCopyName.Click += menuFileCopyName_Click;
        // 
        // menuFileContainerSeparator
        // 
        menuFileContainerSeparator.BackColor = Color.FromArgb(60, 63, 65);
        menuFileContainerSeparator.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileContainerSeparator.Margin = new Padding(0, 0, 0, 1);
        menuFileContainerSeparator.Name = "menuFileContainerSeparator";
        menuFileContainerSeparator.Size = new Size(235, 6);
        // 
        // menuFileRevealContainer
        // 
        menuFileRevealContainer.BackColor = Color.FromArgb(60, 63, 65);
        menuFileRevealContainer.ForeColor = Color.FromArgb(220, 220, 220);
        menuFileRevealContainer.Name = "menuFileRevealContainer";
        menuFileRevealContainer.Size = new Size(238, 22);
        menuFileRevealContainer.Text = "Reveal Container in Explorer";
        menuFileRevealContainer.Click += menuFileRevealContainer_Click;
        // 
        // splitFileContentPreviewPane2
        // 
        splitFileContentPreviewPane2.Controls.Add(sectionFileViewer);
        splitFileContentPreviewPane2.Location = new Point(341, 0);
        splitFileContentPreviewPane2.Name = "splitFileContentPreviewPane2";
        splitFileContentPreviewPane2.Size = new Size(337, 273);
        splitFileContentPreviewPane2.TabIndex = 1;
        // 
        // sectionFileViewer
        // 
        sectionFileViewer.Controls.Add(mediaFileHost);
        sectionFileViewer.Controls.Add(txtHexViewer);
        sectionFileViewer.Controls.Add(txtFileViewer);
        sectionFileViewer.Controls.Add(pictureFileViewer);
        sectionFileViewer.Controls.Add(lblFileViewerInfo);
        sectionFileViewer.Controls.Add(btnMediaLoad);
        sectionFileViewer.Controls.Add(btnMediaPlay);
        sectionFileViewer.Controls.Add(btnMediaPause);
        sectionFileViewer.Controls.Add(btnMediaStop);
        sectionFileViewer.Controls.Add(btnHexPrevious);
        sectionFileViewer.Controls.Add(btnHexNext);
        sectionFileViewer.Controls.Add(lblHexPage);
        sectionFileViewer.Dock = DockStyle.Fill;
        sectionFileViewer.Location = new Point(0, 0);
        sectionFileViewer.Margin = new Padding(6);
        sectionFileViewer.Name = "sectionFileViewer";
        sectionFileViewer.Padding = new Padding(0, 0, 0, 40);
        sectionFileViewer.SectionHeader = "File Viewer";
        sectionFileViewer.Size = new Size(337, 273);
        sectionFileViewer.TabIndex = 0;
        // 
        // mediaFileHost
        // 
        mediaFileHost.BackColor = Color.Black;
        mediaFileHost.Dock = DockStyle.Fill;
        mediaFileHost.Location = new Point(0, 0);
        mediaFileHost.Name = "mediaFileHost";
        mediaFileHost.TabIndex = 3;
        mediaFileHost.Visible = false;
        // 
        // txtHexViewer
        // 
        txtHexViewer.Dock = DockStyle.Fill;
        txtHexViewer.Font = new Font("Consolas", 9F);
        txtHexViewer.Location = new Point(1, 59);
        txtHexViewer.Name = "txtHexViewer";
        txtHexViewer.ReadOnly = true;
        txtHexViewer.SelectionColor = Color.Gainsboro;
        txtHexViewer.SelectionFont = new Font("Consolas", 9F);
        txtHexViewer.Size = new Size(335, 173);
        txtHexViewer.TabIndex = 2;
        txtHexViewer.TextPadding = new Padding(3);
        txtHexViewer.Visible = false;
        txtHexViewer.WordWrap = false;
        // 
        // txtFileViewer
        // 
        txtFileViewer.Dock = DockStyle.Fill;
        txtFileViewer.Font = new Font("Consolas", 9F);
        txtFileViewer.Location = new Point(1, 59);
        txtFileViewer.Name = "txtFileViewer";
        txtFileViewer.ReadOnly = true;
        txtFileViewer.SelectionColor = Color.Gainsboro;
        txtFileViewer.SelectionFont = new Font("Consolas", 9F);
        txtFileViewer.Size = new Size(335, 173);
        txtFileViewer.TabIndex = 1;
        txtFileViewer.TextPadding = new Padding(3);
        txtFileViewer.Visible = false;
        txtFileViewer.WordWrap = false;
        // 
        // pictureFileViewer
        // 
        pictureFileViewer.BackColor = Color.FromArgb(20, 20, 20);
        pictureFileViewer.Dock = DockStyle.Fill;
        pictureFileViewer.Location = new Point(1, 59);
        pictureFileViewer.Name = "pictureFileViewer";
        pictureFileViewer.Size = new Size(335, 173);
        pictureFileViewer.SizeMode = PictureBoxSizeMode.Zoom;
        pictureFileViewer.TabIndex = 0;
        pictureFileViewer.TabStop = false;
        pictureFileViewer.Visible = false;
        // 
        // lblFileViewerInfo
        // 
        lblFileViewerInfo.AutoEllipsis = true;
        lblFileViewerInfo.Dock = DockStyle.Top;
        lblFileViewerInfo.Location = new Point(1, 25);
        lblFileViewerInfo.Name = "lblFileViewerInfo";
        lblFileViewerInfo.Padding = new Padding(8, 0, 8, 0);
        lblFileViewerInfo.Size = new Size(335, 34);
        lblFileViewerInfo.TabIndex = 3;
        lblFileViewerInfo.Text = "Select a file to preview it.";
        lblFileViewerInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnMediaLoad
        // 
        btnMediaLoad.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnMediaLoad.Location = new Point(7, 239);
        btnMediaLoad.Name = "btnMediaLoad";
        btnMediaLoad.Size = new Size(100, 27);
        btnMediaLoad.TabIndex = 0;
        btnMediaLoad.Text = "Load && Play";
        btnMediaLoad.Visible = false;
        btnMediaLoad.Click += btnMediaLoad_Click;
        // 
        // btnMediaPlay
        // 
        btnMediaPlay.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnMediaPlay.Location = new Point(113, 239);
        btnMediaPlay.Name = "btnMediaPlay";
        btnMediaPlay.Size = new Size(62, 27);
        btnMediaPlay.TabIndex = 1;
        btnMediaPlay.Text = "Play";
        btnMediaPlay.Visible = false;
        btnMediaPlay.Click += btnMediaPlay_Click;
        // 
        // btnMediaPause
        // 
        btnMediaPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnMediaPause.Location = new Point(181, 239);
        btnMediaPause.Name = "btnMediaPause";
        btnMediaPause.Size = new Size(62, 27);
        btnMediaPause.TabIndex = 2;
        btnMediaPause.Text = "Pause";
        btnMediaPause.Visible = false;
        btnMediaPause.Click += btnMediaPause_Click;
        // 
        // btnMediaStop
        // 
        btnMediaStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnMediaStop.Location = new Point(249, 239);
        btnMediaStop.Name = "btnMediaStop";
        btnMediaStop.Size = new Size(62, 27);
        btnMediaStop.TabIndex = 3;
        btnMediaStop.Text = "Stop";
        btnMediaStop.Visible = false;
        btnMediaStop.Click += btnMediaStop_Click;
        // 
        // btnHexPrevious
        // 
        btnHexPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnHexPrevious.Location = new Point(7, 239);
        btnHexPrevious.Name = "btnHexPrevious";
        btnHexPrevious.Size = new Size(76, 27);
        btnHexPrevious.TabIndex = 4;
        btnHexPrevious.Text = "Previous";
        btnHexPrevious.Visible = false;
        btnHexPrevious.Click += btnHexPrevious_Click;
        // 
        // btnHexNext
        // 
        btnHexNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnHexNext.Location = new Point(89, 239);
        btnHexNext.Name = "btnHexNext";
        btnHexNext.Size = new Size(62, 27);
        btnHexNext.TabIndex = 5;
        btnHexNext.Text = "Next";
        btnHexNext.Visible = false;
        btnHexNext.Click += btnHexNext_Click;
        // 
        // lblHexPage
        // 
        lblHexPage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblHexPage.AutoEllipsis = true;
        lblHexPage.Location = new Point(159, 239);
        lblHexPage.Name = "lblHexPage";
        lblHexPage.Size = new Size(435, 27);
        lblHexPage.TabIndex = 6;
        lblHexPage.TextAlign = ContentAlignment.MiddleLeft;
        lblHexPage.Visible = false;
        // 
        // lblFilesSummary
        // 
        lblFilesSummary.AutoEllipsis = true;
        lblFilesSummary.Location = new Point(0, 0);
        lblFilesSummary.Name = "lblFilesSummary";
        lblFilesSummary.Padding = new Padding(10, 0, 10, 0);
        lblFilesSummary.Size = new Size(1130, 36);
        lblFilesSummary.TabIndex = 0;
        lblFilesSummary.Text = "Select a game to inventory its files.";
        lblFilesSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnFileExtractAll
        // 
        btnFileExtractAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnFileExtractAll.Location = new Point(1150, 5);
        btnFileExtractAll.Name = "btnFileExtractAll";
        btnFileExtractAll.Size = new Size(120, 26);
        btnFileExtractAll.TabIndex = 0;
        btnFileExtractAll.Text = "Extract All...";
        btnFileExtractAll.Click += btnFileExtractAll_Click;
        // 
        // btnFileCancel
        // 
        btnFileCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnFileCancel.Location = new Point(1276, 5);
        btnFileCancel.Name = "btnFileCancel";
        btnFileCancel.Size = new Size(84, 26);
        btnFileCancel.TabIndex = 1;
        btnFileCancel.Text = "Cancel";
        btnFileCancel.Click += btnFileCancel_Click;
        // 
        // tabExecutable
        // 
        tabExecutable.BackColor = Color.FromArgb(60, 63, 65);
        tabExecutable.Controls.Add(tabsExecutable);
        tabExecutable.Controls.Add(lblExecutableSummary);
        tabExecutable.Controls.Add(searchExecutable);
        tabExecutable.Controls.Add(btnExecCopySelected);
        tabExecutable.Controls.Add(btnExecCopyAll);
        tabExecutable.Controls.Add(btnExecExtract);
        tabExecutable.Controls.Add(btnExecHash);
        tabExecutable.Location = new Point(4, 32);
        tabExecutable.Name = "tabExecutable";
        tabExecutable.Padding = new Padding(0, 90, 0, 0);
        tabExecutable.Size = new Size(1368, 339);
        tabExecutable.TabIndex = 5;
        tabExecutable.Text = "Executable";
        // 
        // tabsExecutable
        // 
        tabsExecutable.AllowDrop = true;
        tabsExecutable.Controls.Add(tabExecModules);
        tabsExecutable.Controls.Add(tabExecElf);
        tabsExecutable.Controls.Add(tabExecSelf);
        tabsExecutable.Dock = DockStyle.Fill;
        tabsExecutable.ItemSize = new Size(83, 28);
        tabsExecutable.Location = new Point(0, 90);
        tabsExecutable.Name = "tabsExecutable";
        tabsExecutable.Padding = new Point(0, 0);
        tabsExecutable.SelectedIndex = 0;
        tabsExecutable.Size = new Size(1368, 249);
        tabsExecutable.TabIndex = 0;
        tabsExecutable.SelectedIndexChanged += tabsExecutable_SelectedIndexChanged;
        // 
        // tabExecModules
        // 
        tabExecModules.BackColor = Color.FromArgb(60, 63, 65);
        tabExecModules.Controls.Add(gridModules);
        tabExecModules.Location = new Point(4, 32);
        tabExecModules.Name = "tabExecModules";
        tabExecModules.Size = new Size(1360, 213);
        tabExecModules.TabIndex = 0;
        tabExecModules.Text = "Modules";
        // 
        // gridModules
        // 
        gridModules.AllowUserToAddRows = false;
        gridModules.AllowUserToDeleteRows = false;
        gridModules.AllowUserToDragDropRows = false;
        gridModules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridModules.AutoSortGroups = true;
        gridModules.Dock = DockStyle.Fill;
        gridModules.GroupCellValueComparer = null;
        gridModules.GroupHeaderColumnIndex = 0;
        gridModules.GroupHeaderColumnName = null;
        gridModules.GroupHeaderHeight = 26F;
        gridModules.GroupLabelFormatter = null;
        gridModules.Location = new Point(0, 0);
        gridModules.Name = "gridModules";
        gridModules.ReadOnly = true;
        gridModules.Size = new Size(1360, 213);
        gridModules.TabIndex = 0;
        // 
        // tabExecElf
        // 
        tabExecElf.BackColor = Color.FromArgb(60, 63, 65);
        tabExecElf.Controls.Add(splitExecElf);
        tabExecElf.Location = new Point(4, 32);
        tabExecElf.Name = "tabExecElf";
        tabExecElf.Size = new Size(1360, 213);
        tabExecElf.TabIndex = 1;
        tabExecElf.Text = "ELF";
        // 
        // splitExecElf
        // 
        splitExecElf.BorderStyle = BorderStyle.FixedSingle;
        splitExecElf.Controls.Add(splitExecElfPane1);
        splitExecElf.Controls.Add(splitExecElfPane2);
        splitExecElf.Dock = DockStyle.Fill;
        splitExecElf.Location = new Point(0, 0);
        splitExecElf.Name = "splitExecElf";
        splitExecElf.Orientation = DarkUI.Controls.DarkSplitContainer.DarkSplitOrientation.Horizontal;
        splitExecElf.Size = new Size(1360, 213);
        splitExecElf.TabIndex = 0;
        // 
        // splitExecElfPane1
        // 
        splitExecElfPane1.Controls.Add(gridElfPrograms);
        splitExecElfPane1.Location = new Point(0, 0);
        splitExecElfPane1.Name = "splitExecElfPane1";
        splitExecElfPane1.Size = new Size(1358, 103);
        splitExecElfPane1.TabIndex = 0;
        // 
        // gridElfPrograms
        // 
        gridElfPrograms.AllowUserToAddRows = false;
        gridElfPrograms.AllowUserToDeleteRows = false;
        gridElfPrograms.AllowUserToDragDropRows = false;
        gridElfPrograms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridElfPrograms.AutoSortGroups = true;
        gridElfPrograms.Dock = DockStyle.Fill;
        gridElfPrograms.GroupCellValueComparer = null;
        gridElfPrograms.GroupHeaderColumnIndex = 0;
        gridElfPrograms.GroupHeaderColumnName = null;
        gridElfPrograms.GroupHeaderHeight = 26F;
        gridElfPrograms.GroupLabelFormatter = null;
        gridElfPrograms.Location = new Point(0, 0);
        gridElfPrograms.Name = "gridElfPrograms";
        gridElfPrograms.ReadOnly = true;
        gridElfPrograms.Size = new Size(1358, 103);
        gridElfPrograms.TabIndex = 0;
        // 
        // splitExecElfPane2
        // 
        splitExecElfPane2.Controls.Add(gridElfSections);
        splitExecElfPane2.Location = new Point(0, 108);
        splitExecElfPane2.Name = "splitExecElfPane2";
        splitExecElfPane2.Size = new Size(1358, 103);
        splitExecElfPane2.TabIndex = 1;
        // 
        // gridElfSections
        // 
        gridElfSections.AllowUserToAddRows = false;
        gridElfSections.AllowUserToDeleteRows = false;
        gridElfSections.AllowUserToDragDropRows = false;
        gridElfSections.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridElfSections.AutoSortGroups = true;
        gridElfSections.Dock = DockStyle.Fill;
        gridElfSections.GroupCellValueComparer = null;
        gridElfSections.GroupHeaderColumnIndex = 0;
        gridElfSections.GroupHeaderColumnName = null;
        gridElfSections.GroupHeaderHeight = 26F;
        gridElfSections.GroupLabelFormatter = null;
        gridElfSections.Location = new Point(0, 0);
        gridElfSections.Name = "gridElfSections";
        gridElfSections.ReadOnly = true;
        gridElfSections.Size = new Size(1358, 103);
        gridElfSections.TabIndex = 0;
        // 
        // tabExecSelf
        // 
        tabExecSelf.BackColor = Color.FromArgb(60, 63, 65);
        tabExecSelf.Controls.Add(splitExecSelf);
        tabExecSelf.Location = new Point(4, 32);
        tabExecSelf.Name = "tabExecSelf";
        tabExecSelf.Size = new Size(1360, 213);
        tabExecSelf.TabIndex = 2;
        tabExecSelf.Text = "SELF";
        // 
        // splitExecSelf
        // 
        splitExecSelf.BorderStyle = BorderStyle.FixedSingle;
        splitExecSelf.Controls.Add(splitExecSelfPane1);
        splitExecSelf.Controls.Add(splitExecSelfPane2);
        splitExecSelf.Dock = DockStyle.Fill;
        splitExecSelf.Location = new Point(0, 0);
        splitExecSelf.Name = "splitExecSelf";
        splitExecSelf.Orientation = DarkUI.Controls.DarkSplitContainer.DarkSplitOrientation.Horizontal;
        splitExecSelf.Size = new Size(1360, 213);
        splitExecSelf.TabIndex = 0;
        // 
        // splitExecSelfPane1
        // 
        splitExecSelfPane1.Controls.Add(gridSelfHeader);
        splitExecSelfPane1.Location = new Point(0, 0);
        splitExecSelfPane1.Name = "splitExecSelfPane1";
        splitExecSelfPane1.Size = new Size(1358, 103);
        splitExecSelfPane1.TabIndex = 0;
        // 
        // gridSelfHeader
        // 
        gridSelfHeader.AllowUserToAddRows = false;
        gridSelfHeader.AllowUserToDeleteRows = false;
        gridSelfHeader.AllowUserToDragDropRows = false;
        gridSelfHeader.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridSelfHeader.AutoSortGroups = true;
        gridSelfHeader.Dock = DockStyle.Fill;
        gridSelfHeader.GroupCellValueComparer = null;
        gridSelfHeader.GroupHeaderColumnIndex = 0;
        gridSelfHeader.GroupHeaderColumnName = null;
        gridSelfHeader.GroupHeaderHeight = 26F;
        gridSelfHeader.GroupLabelFormatter = null;
        gridSelfHeader.Location = new Point(0, 0);
        gridSelfHeader.Name = "gridSelfHeader";
        gridSelfHeader.ReadOnly = true;
        gridSelfHeader.Size = new Size(1358, 103);
        gridSelfHeader.TabIndex = 0;
        // 
        // splitExecSelfPane2
        // 
        splitExecSelfPane2.Controls.Add(gridSelfSegments);
        splitExecSelfPane2.Location = new Point(0, 108);
        splitExecSelfPane2.Name = "splitExecSelfPane2";
        splitExecSelfPane2.Size = new Size(1358, 103);
        splitExecSelfPane2.TabIndex = 1;
        // 
        // gridSelfSegments
        // 
        gridSelfSegments.AllowUserToAddRows = false;
        gridSelfSegments.AllowUserToDeleteRows = false;
        gridSelfSegments.AllowUserToDragDropRows = false;
        gridSelfSegments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridSelfSegments.AutoSortGroups = true;
        gridSelfSegments.Dock = DockStyle.Fill;
        gridSelfSegments.GroupCellValueComparer = null;
        gridSelfSegments.GroupHeaderColumnIndex = 0;
        gridSelfSegments.GroupHeaderColumnName = null;
        gridSelfSegments.GroupHeaderHeight = 26F;
        gridSelfSegments.GroupLabelFormatter = null;
        gridSelfSegments.Location = new Point(0, 0);
        gridSelfSegments.Name = "gridSelfSegments";
        gridSelfSegments.ReadOnly = true;
        gridSelfSegments.Size = new Size(1358, 103);
        gridSelfSegments.TabIndex = 0;
        // 
        // lblExecutableSummary
        // 
        lblExecutableSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblExecutableSummary.AutoEllipsis = true;
        lblExecutableSummary.Location = new Point(0, 0);
        lblExecutableSummary.Name = "lblExecutableSummary";
        lblExecutableSummary.Padding = new Padding(10, 0, 10, 0);
        lblExecutableSummary.Size = new Size(1368, 52);
        lblExecutableSummary.TabIndex = 0;
        lblExecutableSummary.Text = "Select a game to inspect eboot.bin and modules.";
        lblExecutableSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // searchExecutable
        // 
        searchExecutable.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        searchExecutable.Location = new Point(1068, 57);
        searchExecutable.Name = "searchExecutable";
        searchExecutable.Placeholder = "Search modules";
        searchExecutable.Size = new Size(292, 28);
        searchExecutable.TabIndex = 3;
        searchExecutable.SearchTextChanged += searchExecutable_SearchTextChanged;
        // 
        // btnExecCopySelected
        // 
        btnExecCopySelected.Location = new Point(226, 58);
        btnExecCopySelected.Name = "btnExecCopySelected";
        btnExecCopySelected.Size = new Size(120, 26);
        btnExecCopySelected.TabIndex = 2;
        btnExecCopySelected.Text = "Copy Selected";
        btnExecCopySelected.Click += btnExecCopySelected_Click;
        // 
        // btnExecCopyAll
        // 
        btnExecCopyAll.Location = new Point(124, 58);
        btnExecCopyAll.Name = "btnExecCopyAll";
        btnExecCopyAll.Size = new Size(96, 26);
        btnExecCopyAll.TabIndex = 1;
        btnExecCopyAll.Text = "Copy All";
        btnExecCopyAll.Click += btnExecCopyAll_Click;
        // 
        // btnExecExtract
        // 
        btnExecExtract.Location = new Point(8, 58);
        btnExecExtract.Name = "btnExecExtract";
        btnExecExtract.Size = new Size(110, 26);
        btnExecExtract.TabIndex = 0;
        btnExecExtract.Text = "Extract...";
        btnExecExtract.Click += btnExecExtract_Click;
        // 
        // btnExecHash
        // 
        btnExecHash.Location = new Point(124, 58);
        btnExecHash.Name = "btnExecHash";
        btnExecHash.Size = new Size(130, 26);
        btnExecHash.TabIndex = 1;
        btnExecHash.Text = "SHA-256...";
        btnExecHash.Click += btnExecHash_Click;
        // 
        // tabRaw
        // 
        tabRaw.BackColor = Color.FromArgb(60, 63, 65);
        tabRaw.Controls.Add(txtRawMetadata);
        tabRaw.Controls.Add(btnRawFormatted);
        tabRaw.Controls.Add(btnRawOriginal);
        tabRaw.Controls.Add(btnCopyRawJson);
        tabRaw.Location = new Point(4, 32);
        tabRaw.Name = "tabRaw";
        tabRaw.Padding = new Padding(0, 38, 0, 0);
        tabRaw.Size = new Size(1368, 339);
        tabRaw.TabIndex = 6;
        tabRaw.Text = "Raw param.json";
        // 
        // txtRawMetadata
        // 
        txtRawMetadata.Dock = DockStyle.Fill;
        txtRawMetadata.Font = new Font("Consolas", 9F);
        txtRawMetadata.Location = new Point(0, 38);
        txtRawMetadata.Name = "txtRawMetadata";
        txtRawMetadata.ReadOnly = true;
        txtRawMetadata.SelectionColor = Color.Gainsboro;
        txtRawMetadata.SelectionFont = new Font("Consolas", 9F);
        txtRawMetadata.Size = new Size(1368, 301);
        txtRawMetadata.TabIndex = 0;
        txtRawMetadata.TextPadding = new Padding(3);
        txtRawMetadata.WordWrap = false;
        // 
        // btnCopyRawJson
        // 
        btnCopyRawJson.Location = new Point(8, 6);
        btnCopyRawJson.Name = "btnCopyRawJson";
        btnCopyRawJson.Size = new Size(110, 26);
        btnCopyRawJson.TabIndex = 0;
        btnCopyRawJson.Text = "Copy JSON";
        btnCopyRawJson.Click += btnCopyRawJson_Click;
        // 
        // btnRawFormatted
        // 
        btnRawFormatted.Location = new Point(124, 6);
        btnRawFormatted.Name = "btnRawFormatted";
        btnRawFormatted.Size = new Size(110, 26);
        btnRawFormatted.TabIndex = 1;
        btnRawFormatted.Text = "Formatted";
        btnRawFormatted.Click += btnRawFormatted_Click;
        // 
        // btnRawOriginal
        // 
        btnRawOriginal.Location = new Point(240, 6);
        btnRawOriginal.Name = "btnRawOriginal";
        btnRawOriginal.Size = new Size(130, 26);
        btnRawOriginal.TabIndex = 2;
        btnRawOriginal.Text = "Original bytes";
        btnRawOriginal.Click += btnRawOriginal_Click;
        // 
        // tabPackage
        // 
        tabPackage.BackColor = Color.FromArgb(60, 63, 65);
        tabPackage.Controls.Add(tabsPackage);
        tabPackage.Location = new Point(4, 32);
        tabPackage.Name = "tabPackage";
        tabPackage.Size = new Size(1368, 339);
        tabPackage.TabIndex = 8;
        tabPackage.Text = "Container";
        // 
        // tabsPackage
        // 
        tabsPackage.AllowDrop = true;
        tabsPackage.Controls.Add(tabPkgContainer);
        tabsPackage.Controls.Add(tabPkgSegments);
        tabsPackage.Controls.Add(tabPkgEntries);
        tabsPackage.Controls.Add(tabMetadata);
        tabsPackage.Controls.Add(tabPkgSi);
        tabsPackage.Dock = DockStyle.Fill;
        tabsPackage.ItemSize = new Size(113, 28);
        tabsPackage.Location = new Point(0, 0);
        tabsPackage.Name = "tabsPackage";
        tabsPackage.Padding = new Point(0, 0);
        tabsPackage.SelectedIndex = 0;
        tabsPackage.Size = new Size(1368, 339);
        tabsPackage.TabIndex = 0;
        // 
        // tabMetadata
        // 
        tabMetadata.BackColor = Color.FromArgb(60, 63, 65);
        tabMetadata.Controls.Add(tabsMetadata);
        tabMetadata.Location = new Point(4, 32);
        tabMetadata.Name = "tabMetadata";
        tabMetadata.Padding = new Padding(0);
        tabMetadata.Size = new Size(1360, 303);
        tabMetadata.TabIndex = 3;
        tabMetadata.Text = "Metadata";
        // 
        // tabsMetadata
        // 
        tabsMetadata.Controls.Add(tabPkgSfo);
        tabsMetadata.Controls.Add(tabPkgKeystone);
        tabsMetadata.Controls.Add(tabPkgPlayGo);
        tabsMetadata.Dock = DockStyle.Fill;
        tabsMetadata.Location = new Point(0, 0);
        tabsMetadata.Name = "tabsMetadata";
        tabsMetadata.Padding = new Point(0, 0);
        tabsMetadata.SelectedIndex = 0;
        tabsMetadata.Size = new Size(1360, 303);
        tabsMetadata.TabIndex = 0;
        // 
        // tabPkgContainer
        // 
        tabPkgContainer.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgContainer.Controls.Add(gridPkgHeader);
        tabPkgContainer.Location = new Point(4, 32);
        tabPkgContainer.Name = "tabPkgContainer";
        tabPkgContainer.Size = new Size(1360, 303);
        tabPkgContainer.TabIndex = 0;
        tabPkgContainer.Text = "Source structure";
        // 
        // gridPkgHeader
        // 
        gridPkgHeader.AllowUserToAddRows = false;
        gridPkgHeader.AllowUserToDeleteRows = false;
        gridPkgHeader.AllowUserToDragDropRows = false;
        gridPkgHeader.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPkgHeader.AutoSortGroups = true;
        gridPkgHeader.Dock = DockStyle.Fill;
        gridPkgHeader.GroupCellValueComparer = null;
        gridPkgHeader.GroupHeaderColumnIndex = 0;
        gridPkgHeader.GroupHeaderColumnName = null;
        gridPkgHeader.GroupHeaderHeight = 26F;
        gridPkgHeader.GroupLabelFormatter = null;
        gridPkgHeader.Location = new Point(0, 0);
        gridPkgHeader.Name = "gridPkgHeader";
        gridPkgHeader.ReadOnly = true;
        gridPkgHeader.Size = new Size(1360, 303);
        gridPkgHeader.TabIndex = 0;
        // 
        // tabPkgSegments
        // 
        tabPkgSegments.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgSegments.Controls.Add(gridPkgSegments);
        tabPkgSegments.Location = new Point(4, 32);
        tabPkgSegments.Name = "tabPkgSegments";
        tabPkgSegments.Size = new Size(1360, 303);
        tabPkgSegments.TabIndex = 1;
        tabPkgSegments.Text = "Segments";
        // 
        // gridPkgSegments
        // 
        gridPkgSegments.AllowUserToAddRows = false;
        gridPkgSegments.AllowUserToDeleteRows = false;
        gridPkgSegments.AllowUserToDragDropRows = false;
        gridPkgSegments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPkgSegments.AutoSortGroups = true;
        gridPkgSegments.Dock = DockStyle.Fill;
        gridPkgSegments.GroupCellValueComparer = null;
        gridPkgSegments.GroupHeaderColumnIndex = 0;
        gridPkgSegments.GroupHeaderColumnName = null;
        gridPkgSegments.GroupHeaderHeight = 26F;
        gridPkgSegments.GroupLabelFormatter = null;
        gridPkgSegments.Location = new Point(0, 0);
        gridPkgSegments.Name = "gridPkgSegments";
        gridPkgSegments.ReadOnly = true;
        gridPkgSegments.Size = new Size(1360, 303);
        gridPkgSegments.TabIndex = 0;
        // 
        // tabPkgEntries
        // 
        tabPkgEntries.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgEntries.Controls.Add(gridPkgEntries);
        tabPkgEntries.Location = new Point(4, 32);
        tabPkgEntries.Name = "tabPkgEntries";
        tabPkgEntries.Size = new Size(1360, 303);
        tabPkgEntries.TabIndex = 2;
        tabPkgEntries.Text = "CNT entries";
        // 
        // gridPkgEntries
        // 
        gridPkgEntries.AllowUserToAddRows = false;
        gridPkgEntries.AllowUserToDeleteRows = false;
        gridPkgEntries.AllowUserToDragDropRows = false;
        gridPkgEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPkgEntries.AutoSortGroups = true;
        gridPkgEntries.Dock = DockStyle.Fill;
        gridPkgEntries.GroupCellValueComparer = null;
        gridPkgEntries.GroupHeaderColumnIndex = 0;
        gridPkgEntries.GroupHeaderColumnName = null;
        gridPkgEntries.GroupHeaderHeight = 26F;
        gridPkgEntries.GroupLabelFormatter = null;
        gridPkgEntries.Location = new Point(0, 0);
        gridPkgEntries.Name = "gridPkgEntries";
        gridPkgEntries.ReadOnly = true;
        gridPkgEntries.Size = new Size(1360, 303);
        gridPkgEntries.TabIndex = 0;
        // 
        // tabPkgSfo
        // 
        tabPkgSfo.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgSfo.Controls.Add(gridParamSfo);
        tabPkgSfo.Location = new Point(4, 32);
        tabPkgSfo.Name = "tabPkgSfo";
        tabPkgSfo.Size = new Size(1360, 303);
        tabPkgSfo.TabIndex = 3;
        tabPkgSfo.Text = "param.sfo";
        // 
        // gridParamSfo
        // 
        gridParamSfo.AllowUserToAddRows = false;
        gridParamSfo.AllowUserToDeleteRows = false;
        gridParamSfo.AllowUserToDragDropRows = false;
        gridParamSfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridParamSfo.AutoSortGroups = true;
        gridParamSfo.Dock = DockStyle.Fill;
        gridParamSfo.GroupCellValueComparer = null;
        gridParamSfo.GroupHeaderColumnIndex = 0;
        gridParamSfo.GroupHeaderColumnName = null;
        gridParamSfo.GroupHeaderHeight = 26F;
        gridParamSfo.GroupLabelFormatter = null;
        gridParamSfo.Location = new Point(0, 0);
        gridParamSfo.Name = "gridParamSfo";
        gridParamSfo.ReadOnly = true;
        gridParamSfo.Size = new Size(1360, 303);
        gridParamSfo.TabIndex = 0;
        // 
        // tabPkgKeystone
        // 
        tabPkgKeystone.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgKeystone.Controls.Add(gridKeystone);
        tabPkgKeystone.Location = new Point(4, 32);
        tabPkgKeystone.Name = "tabPkgKeystone";
        tabPkgKeystone.Size = new Size(1360, 303);
        tabPkgKeystone.TabIndex = 4;
        tabPkgKeystone.Text = "Keystone / NP";
        // 
        // gridKeystone
        // 
        gridKeystone.AllowUserToAddRows = false;
        gridKeystone.AllowUserToDeleteRows = false;
        gridKeystone.AllowUserToDragDropRows = false;
        gridKeystone.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridKeystone.AutoSortGroups = true;
        gridKeystone.Dock = DockStyle.Fill;
        gridKeystone.GroupCellValueComparer = null;
        gridKeystone.GroupHeaderColumnIndex = 0;
        gridKeystone.GroupHeaderColumnName = null;
        gridKeystone.GroupHeaderHeight = 26F;
        gridKeystone.GroupLabelFormatter = null;
        gridKeystone.Location = new Point(0, 0);
        gridKeystone.Name = "gridKeystone";
        gridKeystone.ReadOnly = true;
        gridKeystone.Size = new Size(1360, 303);
        gridKeystone.TabIndex = 0;
        // 
        // tabPkgSi
        // 
        tabPkgSi.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgSi.Controls.Add(gridSi);
        tabPkgSi.Location = new Point(4, 32);
        tabPkgSi.Name = "tabPkgSi";
        tabPkgSi.Size = new Size(1360, 303);
        tabPkgSi.TabIndex = 5;
        tabPkgSi.Text = "SI contents";
        // 
        // gridSi
        // 
        gridSi.AllowUserToAddRows = false;
        gridSi.AllowUserToDeleteRows = false;
        gridSi.AllowUserToDragDropRows = false;
        gridSi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridSi.AutoSortGroups = true;
        gridSi.Dock = DockStyle.Fill;
        gridSi.GroupCellValueComparer = null;
        gridSi.GroupHeaderColumnIndex = 0;
        gridSi.GroupHeaderColumnName = null;
        gridSi.GroupHeaderHeight = 26F;
        gridSi.GroupLabelFormatter = null;
        gridSi.Location = new Point(0, 0);
        gridSi.Name = "gridSi";
        gridSi.ReadOnly = true;
        gridSi.Size = new Size(1360, 303);
        gridSi.TabIndex = 0;
        // 
        // tabPkgPlayGo
        // 
        tabPkgPlayGo.BackColor = Color.FromArgb(60, 63, 65);
        tabPkgPlayGo.Controls.Add(tabsPlayGo);
        tabPkgPlayGo.Controls.Add(lblPlayGoSummary);
        tabPkgPlayGo.Location = new Point(4, 32);
        tabPkgPlayGo.Name = "tabPkgPlayGo";
        tabPkgPlayGo.Size = new Size(1360, 303);
        tabPkgPlayGo.TabIndex = 6;
        tabPkgPlayGo.Text = "PlayGo";
        // 
        // tabsPlayGo
        // 
        tabsPlayGo.AllowDrop = true;
        tabsPlayGo.Controls.Add(tabPlayGoChunks);
        tabsPlayGo.Controls.Add(tabPlayGoScenarios);
        tabsPlayGo.Controls.Add(tabPlayGoFiles);
        tabsPlayGo.Dock = DockStyle.Fill;
        tabsPlayGo.ItemSize = new Size(95, 28);
        tabsPlayGo.Location = new Point(0, 30);
        tabsPlayGo.Name = "tabsPlayGo";
        tabsPlayGo.Padding = new Point(0, 0);
        tabsPlayGo.SelectedIndex = 0;
        tabsPlayGo.Size = new Size(1360, 273);
        tabsPlayGo.TabIndex = 0;
        // 
        // tabPlayGoChunks
        // 
        tabPlayGoChunks.BackColor = Color.FromArgb(60, 63, 65);
        tabPlayGoChunks.Controls.Add(gridPlayGoChunks);
        tabPlayGoChunks.Location = new Point(4, 32);
        tabPlayGoChunks.Name = "tabPlayGoChunks";
        tabPlayGoChunks.Size = new Size(1352, 237);
        tabPlayGoChunks.TabIndex = 0;
        tabPlayGoChunks.Text = "Chunks";
        // 
        // gridPlayGoChunks
        // 
        gridPlayGoChunks.AllowUserToAddRows = false;
        gridPlayGoChunks.AllowUserToDeleteRows = false;
        gridPlayGoChunks.AllowUserToDragDropRows = false;
        gridPlayGoChunks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPlayGoChunks.AutoSortGroups = true;
        gridPlayGoChunks.Dock = DockStyle.Fill;
        gridPlayGoChunks.GroupCellValueComparer = null;
        gridPlayGoChunks.GroupHeaderColumnIndex = 0;
        gridPlayGoChunks.GroupHeaderColumnName = null;
        gridPlayGoChunks.GroupHeaderHeight = 26F;
        gridPlayGoChunks.GroupLabelFormatter = null;
        gridPlayGoChunks.Location = new Point(0, 0);
        gridPlayGoChunks.Name = "gridPlayGoChunks";
        gridPlayGoChunks.ReadOnly = true;
        gridPlayGoChunks.Size = new Size(1352, 237);
        gridPlayGoChunks.TabIndex = 0;
        // 
        // tabPlayGoScenarios
        // 
        tabPlayGoScenarios.BackColor = Color.FromArgb(60, 63, 65);
        tabPlayGoScenarios.Controls.Add(gridPlayGoScenarios);
        tabPlayGoScenarios.Location = new Point(4, 32);
        tabPlayGoScenarios.Name = "tabPlayGoScenarios";
        tabPlayGoScenarios.Size = new Size(1352, 237);
        tabPlayGoScenarios.TabIndex = 1;
        tabPlayGoScenarios.Text = "Scenarios";
        // 
        // gridPlayGoScenarios
        // 
        gridPlayGoScenarios.AllowUserToAddRows = false;
        gridPlayGoScenarios.AllowUserToDeleteRows = false;
        gridPlayGoScenarios.AllowUserToDragDropRows = false;
        gridPlayGoScenarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPlayGoScenarios.AutoSortGroups = true;
        gridPlayGoScenarios.Dock = DockStyle.Fill;
        gridPlayGoScenarios.GroupCellValueComparer = null;
        gridPlayGoScenarios.GroupHeaderColumnIndex = 0;
        gridPlayGoScenarios.GroupHeaderColumnName = null;
        gridPlayGoScenarios.GroupHeaderHeight = 26F;
        gridPlayGoScenarios.GroupLabelFormatter = null;
        gridPlayGoScenarios.Location = new Point(0, 0);
        gridPlayGoScenarios.Name = "gridPlayGoScenarios";
        gridPlayGoScenarios.ReadOnly = true;
        gridPlayGoScenarios.Size = new Size(1352, 237);
        gridPlayGoScenarios.TabIndex = 0;
        // 
        // tabPlayGoFiles
        // 
        tabPlayGoFiles.BackColor = Color.FromArgb(60, 63, 65);
        tabPlayGoFiles.Controls.Add(gridPlayGoFiles);
        tabPlayGoFiles.Location = new Point(4, 32);
        tabPlayGoFiles.Name = "tabPlayGoFiles";
        tabPlayGoFiles.Size = new Size(1352, 237);
        tabPlayGoFiles.TabIndex = 2;
        tabPlayGoFiles.Text = "File chunks";
        // 
        // gridPlayGoFiles
        // 
        gridPlayGoFiles.AllowUserToAddRows = false;
        gridPlayGoFiles.AllowUserToDeleteRows = false;
        gridPlayGoFiles.AllowUserToDragDropRows = false;
        gridPlayGoFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridPlayGoFiles.AutoSortGroups = true;
        gridPlayGoFiles.Dock = DockStyle.Fill;
        gridPlayGoFiles.GroupCellValueComparer = null;
        gridPlayGoFiles.GroupHeaderColumnIndex = 0;
        gridPlayGoFiles.GroupHeaderColumnName = null;
        gridPlayGoFiles.GroupHeaderHeight = 26F;
        gridPlayGoFiles.GroupLabelFormatter = null;
        gridPlayGoFiles.Location = new Point(0, 0);
        gridPlayGoFiles.Name = "gridPlayGoFiles";
        gridPlayGoFiles.ReadOnly = true;
        gridPlayGoFiles.Size = new Size(1352, 237);
        gridPlayGoFiles.TabIndex = 0;
        // 
        // lblPlayGoSummary
        // 
        lblPlayGoSummary.AutoEllipsis = true;
        lblPlayGoSummary.Dock = DockStyle.Top;
        lblPlayGoSummary.Location = new Point(0, 0);
        lblPlayGoSummary.Name = "lblPlayGoSummary";
        lblPlayGoSummary.Padding = new Padding(10, 0, 10, 0);
        lblPlayGoSummary.Size = new Size(1360, 30);
        lblPlayGoSummary.TabIndex = 1;
        lblPlayGoSummary.Text = "Select a game to inspect the PlayGo chunk map.";
        lblPlayGoSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tabWorkspaceTools
        // 
        tabWorkspaceTools.BackColor = Color.FromArgb(60, 63, 65);
        tabWorkspaceTools.Controls.Add(toolsLayout);
        tabWorkspaceTools.Location = new Point(4, 32);
        tabWorkspaceTools.Name = "tabWorkspaceTools";
        tabWorkspaceTools.Size = new Size(1376, 375);
        tabWorkspaceTools.TabIndex = 1;
        tabWorkspaceTools.Text = "Tools";
        // 
        // toolsLayout
        // 
        toolsLayout.ColumnCount = 1;
        toolsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        toolsLayout.Controls.Add(sectionJob, 0, 0);
        toolsLayout.Controls.Add(tabsImageTargets, 0, 1);
        toolsLayout.Controls.Add(toolsFooter, 0, 2);
        toolsLayout.Dock = DockStyle.Fill;
        toolsLayout.Location = new Point(0, 0);
        toolsLayout.Name = "toolsLayout";
        toolsLayout.Padding = new Padding(10, 8, 10, 8);
        toolsLayout.RowCount = 3;
        toolsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 115F));
        toolsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        toolsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        toolsLayout.Size = new Size(1376, 375);
        toolsLayout.TabIndex = 0;
        // 
        // sectionJob
        // 
        sectionJob.Controls.Add(lblImageSourcePath);
        sectionJob.Controls.Add(lblImageFormat);
        sectionJob.Controls.Add(lblImageAction);
        sectionJob.Controls.Add(cboImageAction);
        sectionJob.Controls.Add(lblImageSource);
        sectionJob.Dock = DockStyle.Fill;
        sectionJob.Location = new Point(10, 8);
        sectionJob.Margin = new Padding(0, 0, 0, 6);
        sectionJob.Name = "sectionJob";
        sectionJob.SectionHeader = "Job";
        sectionJob.Size = new Size(1356, 109);
        sectionJob.TabIndex = 0;
        // 
        // lblImageSourcePath
        // 
        lblImageSourcePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblImageSourcePath.AutoEllipsis = true;
        lblImageSourcePath.Location = new Point(120, 27);
        lblImageSourcePath.Name = "lblImageSourcePath";
        lblImageSourcePath.Size = new Size(1200, 20);
        lblImageSourcePath.TabIndex = 1;
        lblImageSourcePath.Text = "Select a game in the library above.";
        lblImageSourcePath.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblImageFormat
        // 
        lblImageFormat.AutoEllipsis = true;
        lblImageFormat.Location = new Point(120, 53);
        lblImageFormat.Name = "lblImageFormat";
        lblImageFormat.Size = new Size(320, 15);
        lblImageFormat.TabIndex = 4;
        lblImageFormat.Text = "Detected: -";
        // 
        // lblImageAction
        // 
        lblImageAction.Location = new Point(12, 83);
        lblImageAction.Name = "lblImageAction";
        lblImageAction.Size = new Size(100, 15);
        lblImageAction.TabIndex = 5;
        lblImageAction.Text = "Action:";
        lblImageAction.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageAction
        // 
        cboImageAction.Location = new Point(120, 79);
        cboImageAction.Name = "cboImageAction";
        cboImageAction.Size = new Size(260, 24);
        cboImageAction.TabIndex = 6;
        cboImageAction.SelectedIndexChanged += cboImageAction_SelectedIndexChanged;
        // 
        // lblImageSource
        // 
        lblImageSource.Location = new Point(12, 31);
        lblImageSource.Name = "lblImageSource";
        lblImageSource.Size = new Size(100, 15);
        lblImageSource.TabIndex = 0;
        lblImageSource.Text = "Source:";
        lblImageSource.TextAlign = ContentAlignment.MiddleRight;
        // 
        // tabsImageTargets
        // 
        tabsImageTargets.AllowDrop = true;
        tabsImageTargets.Controls.Add(tabTargetExfat);
        tabsImageTargets.Controls.Add(tabTargetFfpkg);
        tabsImageTargets.Controls.Add(tabTargetFfpfsc);
        tabsImageTargets.Controls.Add(tabTargetDebug);
        tabsImageTargets.Controls.Add(tabTargetOptions);
        tabsImageTargets.Dock = DockStyle.Fill;
        tabsImageTargets.ItemSize = new Size(80, 28);
        tabsImageTargets.Location = new Point(13, 126);
        tabsImageTargets.Name = "tabsImageTargets";
        tabsImageTargets.Padding = new Point(0, 0);
        tabsImageTargets.SelectedIndex = 0;
        tabsImageTargets.Size = new Size(1350, 188);
        tabsImageTargets.TabIndex = 1;
        tabsImageTargets.SelectedIndexChanged += tabsImageTargets_SelectedIndexChanged;
        // 
        // tabTargetExfat
        // 
        tabTargetExfat.BackColor = Color.FromArgb(60, 63, 65);
        tabTargetExfat.Controls.Add(lblOutExfat);
        tabTargetExfat.Controls.Add(txtOutExfat);
        tabTargetExfat.Controls.Add(btnOutExfat);
        tabTargetExfat.Controls.Add(chkOutExfat);
        tabTargetExfat.Controls.Add(lblImageCluster);
        tabTargetExfat.Controls.Add(cboImageCluster);
        tabTargetExfat.Controls.Add(chkImageAmpr);
        tabTargetExfat.Location = new Point(4, 32);
        tabTargetExfat.Name = "tabTargetExfat";
        tabTargetExfat.Size = new Size(1342, 152);
        tabTargetExfat.TabIndex = 0;
        tabTargetExfat.Text = "exFAT";
        // 
        // lblOutExfat
        // 
        lblOutExfat.Location = new Point(229, 16);
        lblOutExfat.Name = "lblOutExfat";
        lblOutExfat.Size = new Size(100, 15);
        lblOutExfat.TabIndex = 9;
        lblOutExfat.Text = "Output:";
        lblOutExfat.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtOutExfat
        // 
        txtOutExfat.Location = new Point(337, 12);
        txtOutExfat.Name = "txtOutExfat";
        txtOutExfat.Size = new Size(560, 23);
        txtOutExfat.TabIndex = 10;
        // 
        // btnOutExfat
        // 
        btnOutExfat.Location = new Point(905, 12);
        btnOutExfat.Name = "btnOutExfat";
        btnOutExfat.Size = new Size(80, 24);
        btnOutExfat.TabIndex = 11;
        btnOutExfat.Text = "Browse...";
        btnOutExfat.Click += btnImageBrowseOutput_Click;
        // 
        // chkOutExfat
        // 
        chkOutExfat.AutoSize = true;
        chkOutExfat.Location = new Point(993, 16);
        chkOutExfat.Name = "chkOutExfat";
        chkOutExfat.Size = new Size(120, 19);
        chkOutExfat.TabIndex = 12;
        chkOutExfat.Text = "Overwrite existing";
        // 
        // lblImageCluster
        // 
        lblImageCluster.Location = new Point(229, 46);
        lblImageCluster.Name = "lblImageCluster";
        lblImageCluster.Size = new Size(100, 15);
        lblImageCluster.TabIndex = 13;
        lblImageCluster.Text = "Cluster size:";
        lblImageCluster.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageCluster
        // 
        cboImageCluster.Items.AddRange(new object[] { "Auto", "32 KB", "64 KB" });
        cboImageCluster.Location = new Point(337, 42);
        cboImageCluster.Name = "cboImageCluster";
        cboImageCluster.Size = new Size(140, 24);
        cboImageCluster.TabIndex = 14;
        // 
        // chkImageAmpr
        // 
        chkImageAmpr.AutoSize = true;
        chkImageAmpr.Location = new Point(497, 46);
        chkImageAmpr.Name = "chkImageAmpr";
        chkImageAmpr.Size = new Size(140, 19);
        chkImageAmpr.TabIndex = 19;
        chkImageAmpr.Text = "Generate AMPR index";
        // 
        // tabTargetFfpkg
        // 
        tabTargetFfpkg.BackColor = Color.FromArgb(60, 63, 65);
        tabTargetFfpkg.Controls.Add(lblOutFfpkg);
        tabTargetFfpkg.Controls.Add(txtOutFfpkg);
        tabTargetFfpkg.Controls.Add(btnOutFfpkg);
        tabTargetFfpkg.Controls.Add(chkOutFfpkg);
        tabTargetFfpkg.Controls.Add(lblImageBlock);
        tabTargetFfpkg.Controls.Add(cboImageBlock);
        tabTargetFfpkg.Controls.Add(lblImageFragment);
        tabTargetFfpkg.Controls.Add(cboImageFragment);
        tabTargetFfpkg.Controls.Add(lblImageDensity);
        tabTargetFfpkg.Controls.Add(cboImageDensity);
        tabTargetFfpkg.Controls.Add(lblImageMinFree);
        tabTargetFfpkg.Controls.Add(nudImageMinFree);
        tabTargetFfpkg.Location = new Point(4, 32);
        tabTargetFfpkg.Name = "tabTargetFfpkg";
        tabTargetFfpkg.Size = new Size(1342, 152);
        tabTargetFfpkg.TabIndex = 1;
        tabTargetFfpkg.Text = "FFPKG";
        // 
        // lblOutFfpkg
        // 
        lblOutFfpkg.Location = new Point(229, 16);
        lblOutFfpkg.Name = "lblOutFfpkg";
        lblOutFfpkg.Size = new Size(100, 15);
        lblOutFfpkg.TabIndex = 9;
        lblOutFfpkg.Text = "Output:";
        lblOutFfpkg.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtOutFfpkg
        // 
        txtOutFfpkg.Location = new Point(337, 12);
        txtOutFfpkg.Name = "txtOutFfpkg";
        txtOutFfpkg.Size = new Size(560, 23);
        txtOutFfpkg.TabIndex = 10;
        // 
        // btnOutFfpkg
        // 
        btnOutFfpkg.Location = new Point(905, 12);
        btnOutFfpkg.Name = "btnOutFfpkg";
        btnOutFfpkg.Size = new Size(80, 24);
        btnOutFfpkg.TabIndex = 11;
        btnOutFfpkg.Text = "Browse...";
        btnOutFfpkg.Click += btnImageBrowseOutput_Click;
        // 
        // chkOutFfpkg
        // 
        chkOutFfpkg.AutoSize = true;
        chkOutFfpkg.Location = new Point(993, 16);
        chkOutFfpkg.Name = "chkOutFfpkg";
        chkOutFfpkg.Size = new Size(120, 19);
        chkOutFfpkg.TabIndex = 12;
        chkOutFfpkg.Text = "Overwrite existing";
        // 
        // lblImageBlock
        // 
        lblImageBlock.Location = new Point(229, 46);
        lblImageBlock.Name = "lblImageBlock";
        lblImageBlock.Size = new Size(100, 15);
        lblImageBlock.TabIndex = 24;
        lblImageBlock.Text = "Block size:";
        lblImageBlock.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageBlock
        // 
        cboImageBlock.Items.AddRange(new object[] { "32 KB", "64 KB" });
        cboImageBlock.Location = new Point(337, 42);
        cboImageBlock.Name = "cboImageBlock";
        cboImageBlock.Size = new Size(100, 24);
        cboImageBlock.TabIndex = 25;
        // 
        // lblImageFragment
        // 
        lblImageFragment.Location = new Point(439, 46);
        lblImageFragment.Name = "lblImageFragment";
        lblImageFragment.Size = new Size(100, 15);
        lblImageFragment.TabIndex = 26;
        lblImageFragment.Text = "Fragment:";
        lblImageFragment.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageFragment
        // 
        cboImageFragment.Items.AddRange(new object[] { "4 KB", "64 KB" });
        cboImageFragment.Location = new Point(547, 42);
        cboImageFragment.Name = "cboImageFragment";
        cboImageFragment.Size = new Size(100, 24);
        cboImageFragment.TabIndex = 27;
        // 
        // lblImageDensity
        // 
        lblImageDensity.Location = new Point(649, 46);
        lblImageDensity.Name = "lblImageDensity";
        lblImageDensity.Size = new Size(100, 15);
        lblImageDensity.TabIndex = 28;
        lblImageDensity.Text = "Density:";
        lblImageDensity.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageDensity
        // 
        cboImageDensity.Items.AddRange(new object[] { "256 KiB", "512 KiB", "1 MiB" });
        cboImageDensity.Location = new Point(757, 42);
        cboImageDensity.Name = "cboImageDensity";
        cboImageDensity.Size = new Size(140, 24);
        cboImageDensity.TabIndex = 29;
        // 
        // lblImageMinFree
        // 
        lblImageMinFree.Location = new Point(899, 46);
        lblImageMinFree.Name = "lblImageMinFree";
        lblImageMinFree.Size = new Size(100, 15);
        lblImageMinFree.TabIndex = 30;
        lblImageMinFree.Text = "Min free %:";
        lblImageMinFree.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudImageMinFree
        // 
        nudImageMinFree.Location = new Point(1007, 42);
        nudImageMinFree.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        nudImageMinFree.Name = "nudImageMinFree";
        nudImageMinFree.Size = new Size(60, 23);
        nudImageMinFree.TabIndex = 31;
        // 
        // tabTargetFfpfsc
        // 
        tabTargetFfpfsc.BackColor = Color.FromArgb(60, 63, 65);
        tabTargetFfpfsc.Controls.Add(lblOutFfpfsc);
        tabTargetFfpfsc.Controls.Add(txtOutFfpfsc);
        tabTargetFfpfsc.Controls.Add(btnOutFfpfsc);
        tabTargetFfpfsc.Controls.Add(chkOutFfpfsc);
        tabTargetFfpfsc.Controls.Add(lblImageLevel);
        tabTargetFfpfsc.Controls.Add(nudImageLevel);
        tabTargetFfpfsc.Controls.Add(lblImageGain);
        tabTargetFfpfsc.Controls.Add(nudImageGain);
        tabTargetFfpfsc.Location = new Point(4, 32);
        tabTargetFfpfsc.Name = "tabTargetFfpfsc";
        tabTargetFfpfsc.Size = new Size(1342, 152);
        tabTargetFfpfsc.TabIndex = 2;
        tabTargetFfpfsc.Text = "FFPFSC";
        // 
        // lblOutFfpfsc
        // 
        lblOutFfpfsc.Location = new Point(229, 16);
        lblOutFfpfsc.Name = "lblOutFfpfsc";
        lblOutFfpfsc.Size = new Size(100, 15);
        lblOutFfpfsc.TabIndex = 9;
        lblOutFfpfsc.Text = "Output:";
        lblOutFfpfsc.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtOutFfpfsc
        // 
        txtOutFfpfsc.Location = new Point(337, 12);
        txtOutFfpfsc.Name = "txtOutFfpfsc";
        txtOutFfpfsc.Size = new Size(560, 23);
        txtOutFfpfsc.TabIndex = 10;
        // 
        // btnOutFfpfsc
        // 
        btnOutFfpfsc.Location = new Point(905, 12);
        btnOutFfpfsc.Name = "btnOutFfpfsc";
        btnOutFfpfsc.Size = new Size(80, 24);
        btnOutFfpfsc.TabIndex = 11;
        btnOutFfpfsc.Text = "Browse...";
        btnOutFfpfsc.Click += btnImageBrowseOutput_Click;
        // 
        // chkOutFfpfsc
        // 
        chkOutFfpfsc.AutoSize = true;
        chkOutFfpfsc.Location = new Point(993, 16);
        chkOutFfpfsc.Name = "chkOutFfpfsc";
        chkOutFfpfsc.Size = new Size(120, 19);
        chkOutFfpfsc.TabIndex = 12;
        chkOutFfpfsc.Text = "Overwrite existing";
        // 
        // lblImageLevel
        // 
        lblImageLevel.Location = new Point(229, 46);
        lblImageLevel.Name = "lblImageLevel";
        lblImageLevel.Size = new Size(100, 15);
        lblImageLevel.TabIndex = 15;
        lblImageLevel.Text = "Level:";
        lblImageLevel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudImageLevel
        // 
        nudImageLevel.Location = new Point(337, 42);
        nudImageLevel.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
        nudImageLevel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudImageLevel.Name = "nudImageLevel";
        nudImageLevel.Size = new Size(60, 23);
        nudImageLevel.TabIndex = 16;
        nudImageLevel.Value = new decimal(new int[] { 7, 0, 0, 0 });
        // 
        // lblImageGain
        // 
        lblImageGain.Location = new Point(417, 46);
        lblImageGain.Name = "lblImageGain";
        lblImageGain.Size = new Size(100, 15);
        lblImageGain.TabIndex = 17;
        lblImageGain.Text = "Gain %:";
        lblImageGain.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudImageGain
        // 
        nudImageGain.Location = new Point(525, 42);
        nudImageGain.Name = "nudImageGain";
        nudImageGain.Size = new Size(60, 23);
        nudImageGain.TabIndex = 18;
        nudImageGain.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // tabTargetDebug
        // 
        tabTargetDebug.BackColor = Color.FromArgb(60, 63, 65);
        tabTargetDebug.Controls.Add(lblOutDebug);
        tabTargetDebug.Controls.Add(txtOutDebug);
        tabTargetDebug.Controls.Add(btnOutDebug);
        tabTargetDebug.Controls.Add(chkOutDebug);
        tabTargetDebug.Controls.Add(lblDbgPasscode);
        tabTargetDebug.Controls.Add(txtDbgPasscode);
        tabTargetDebug.Controls.Add(lblImageSdk);
        tabTargetDebug.Controls.Add(cboImageSdk);
        tabTargetDebug.Controls.Add(lblImagePkgType);
        tabTargetDebug.Controls.Add(cboImagePkgType);
        tabTargetDebug.Controls.Add(lblImageCompression);
        tabTargetDebug.Controls.Add(cboImageCompression);
        tabTargetDebug.Controls.Add(lblImageKrakenLevel);
        tabTargetDebug.Controls.Add(cboImageKrakenLevel);
        tabTargetDebug.Controls.Add(lblImagePlayGo);
        tabTargetDebug.Controls.Add(nudImagePlayGoChunks);
        tabTargetDebug.Controls.Add(lblImageKrakenThreads);
        tabTargetDebug.Controls.Add(nudImageKrakenThreads);
        tabTargetDebug.Controls.Add(lblImageTemp);
        tabTargetDebug.Controls.Add(txtImageTemp);
        tabTargetDebug.Controls.Add(btnImageTempBrowse);
        tabTargetDebug.Controls.Add(lblImageDrm);
        tabTargetDebug.Controls.Add(cboImageDrm);
        tabTargetDebug.Controls.Add(lblImageBackend);
        tabTargetDebug.Controls.Add(cboImageBackend);
        tabTargetDebug.Controls.Add(chkImageFakeSign);
        tabTargetDebug.Controls.Add(chkImageRightSprx);
        tabTargetDebug.Controls.Add(chkImageDeterministic);
        tabTargetDebug.Controls.Add(chkImageAdvancedOptions);
        tabTargetDebug.Location = new Point(4, 32);
        tabTargetDebug.Name = "tabTargetDebug";
        tabTargetDebug.Size = new Size(1342, 152);
        tabTargetDebug.TabIndex = 3;
        tabTargetDebug.Text = "PKG";
        // 
        // lblOutDebug
        // 
        lblOutDebug.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblOutDebug.Location = new Point(217, 16);
        lblOutDebug.Name = "lblOutDebug";
        lblOutDebug.Size = new Size(100, 15);
        lblOutDebug.TabIndex = 9;
        lblOutDebug.Text = "Output:";
        lblOutDebug.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtOutDebug
        // 
        txtOutDebug.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtOutDebug.Location = new Point(325, 12);
        txtOutDebug.Name = "txtOutDebug";
        txtOutDebug.Size = new Size(560, 23);
        txtOutDebug.TabIndex = 10;
        // 
        // btnOutDebug
        // 
        btnOutDebug.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOutDebug.Location = new Point(893, 12);
        btnOutDebug.Name = "btnOutDebug";
        btnOutDebug.Size = new Size(80, 24);
        btnOutDebug.TabIndex = 11;
        btnOutDebug.Text = "Browse...";
        btnOutDebug.Click += btnImageBrowseOutput_Click;
        // 
        // chkOutDebug
        // 
        chkOutDebug.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkOutDebug.AutoSize = true;
        chkOutDebug.Location = new Point(981, 16);
        chkOutDebug.Name = "chkOutDebug";
        chkOutDebug.Size = new Size(120, 19);
        chkOutDebug.TabIndex = 12;
        chkOutDebug.Text = "Overwrite existing";
        // 
        // lblDbgPasscode
        // 
        lblDbgPasscode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblDbgPasscode.Location = new Point(217, 46);
        lblDbgPasscode.Name = "lblDbgPasscode";
        lblDbgPasscode.Size = new Size(100, 15);
        lblDbgPasscode.TabIndex = 34;
        lblDbgPasscode.Text = "Passcode:";
        lblDbgPasscode.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtDbgPasscode
        // 
        txtDbgPasscode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtDbgPasscode.Location = new Point(325, 42);
        txtDbgPasscode.Name = "txtDbgPasscode";
        txtDbgPasscode.Size = new Size(288, 23);
        txtDbgPasscode.TabIndex = 35;
        // 
        // lblImageSdk
        // 
        lblImageSdk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageSdk.Location = new Point(631, 46);
        lblImageSdk.Name = "lblImageSdk";
        lblImageSdk.Size = new Size(100, 15);
        lblImageSdk.TabIndex = 36;
        lblImageSdk.Text = "SDK:";
        lblImageSdk.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageSdk
        // 
        cboImageSdk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImageSdk.Location = new Point(739, 41);
        cboImageSdk.Name = "cboImageSdk";
        cboImageSdk.Size = new Size(234, 24);
        cboImageSdk.TabIndex = 37;
        // 
        // lblImagePkgType
        // 
        lblImagePkgType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImagePkgType.Location = new Point(217, 76);
        lblImagePkgType.Name = "lblImagePkgType";
        lblImagePkgType.Size = new Size(100, 15);
        lblImagePkgType.TabIndex = 42;
        lblImagePkgType.Text = "Package type:";
        lblImagePkgType.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImagePkgType
        // 
        cboImagePkgType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImagePkgType.Location = new Point(325, 72);
        cboImagePkgType.Name = "cboImagePkgType";
        cboImagePkgType.Size = new Size(288, 24);
        cboImagePkgType.TabIndex = 43;
        cboImagePkgType.SelectedIndexChanged += cboImagePkgType_SelectedIndexChanged;
        // 
        // lblImageCompression
        // 
        lblImageCompression.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageCompression.Location = new Point(631, 76);
        lblImageCompression.Name = "lblImageCompression";
        lblImageCompression.Size = new Size(100, 15);
        lblImageCompression.TabIndex = 44;
        lblImageCompression.Text = "Compression:";
        lblImageCompression.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageCompression
        // 
        cboImageCompression.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImageCompression.Location = new Point(739, 72);
        cboImageCompression.Name = "cboImageCompression";
        cboImageCompression.Size = new Size(234, 24);
        cboImageCompression.TabIndex = 45;
        cboImageCompression.SelectedIndexChanged += cboImageCompression_SelectedIndexChanged;
        // 
        // lblImageKrakenLevel
        // 
        lblImageKrakenLevel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageKrakenLevel.Location = new Point(397, 106);
        lblImageKrakenLevel.Name = "lblImageKrakenLevel";
        lblImageKrakenLevel.Size = new Size(100, 15);
        lblImageKrakenLevel.TabIndex = 46;
        lblImageKrakenLevel.Text = "Kraken level:";
        lblImageKrakenLevel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageKrakenLevel
        // 
        cboImageKrakenLevel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImageKrakenLevel.Location = new Point(505, 101);
        cboImageKrakenLevel.Name = "cboImageKrakenLevel";
        cboImageKrakenLevel.Size = new Size(108, 24);
        cboImageKrakenLevel.TabIndex = 47;
        // 
        // lblImagePlayGo
        // 
        lblImagePlayGo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImagePlayGo.Location = new Point(217, 106);
        lblImagePlayGo.Name = "lblImagePlayGo";
        lblImagePlayGo.Size = new Size(100, 15);
        lblImagePlayGo.TabIndex = 50;
        lblImagePlayGo.Text = "PlayGo chunks:";
        lblImagePlayGo.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudImagePlayGoChunks
        // 
        nudImagePlayGoChunks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        nudImagePlayGoChunks.Location = new Point(325, 102);
        nudImagePlayGoChunks.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        nudImagePlayGoChunks.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudImagePlayGoChunks.Name = "nudImagePlayGoChunks";
        nudImagePlayGoChunks.Size = new Size(70, 23);
        nudImagePlayGoChunks.TabIndex = 51;
        nudImagePlayGoChunks.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // lblImageKrakenThreads
        // 
        lblImageKrakenThreads.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageKrakenThreads.Location = new Point(631, 106);
        lblImageKrakenThreads.Name = "lblImageKrakenThreads";
        lblImageKrakenThreads.Size = new Size(100, 15);
        lblImageKrakenThreads.TabIndex = 48;
        lblImageKrakenThreads.Text = "Kraken threads:";
        lblImageKrakenThreads.TextAlign = ContentAlignment.MiddleRight;
        // 
        // nudImageKrakenThreads
        // 
        nudImageKrakenThreads.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        nudImageKrakenThreads.Location = new Point(739, 102);
        nudImageKrakenThreads.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
        nudImageKrakenThreads.Name = "nudImageKrakenThreads";
        nudImageKrakenThreads.Size = new Size(70, 23);
        nudImageKrakenThreads.TabIndex = 49;
        // 
        // lblImageTemp
        // 
        lblImageTemp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageTemp.Location = new Point(217, 136);
        lblImageTemp.Name = "lblImageTemp";
        lblImageTemp.Size = new Size(100, 15);
        lblImageTemp.TabIndex = 52;
        lblImageTemp.Text = "Workspace:";
        lblImageTemp.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtImageTemp
        // 
        txtImageTemp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtImageTemp.Location = new Point(325, 132);
        txtImageTemp.Name = "txtImageTemp";
        txtImageTemp.Size = new Size(288, 23);
        txtImageTemp.TabIndex = 53;
        // 
        // btnImageTempBrowse
        // 
        btnImageTempBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnImageTempBrowse.Location = new Point(619, 131);
        btnImageTempBrowse.Name = "btnImageTempBrowse";
        btnImageTempBrowse.Size = new Size(80, 24);
        btnImageTempBrowse.TabIndex = 54;
        btnImageTempBrowse.Text = "Browse...";
        btnImageTempBrowse.Click += btnImageTempBrowse_Click;
        // 
        // lblImageDrm
        // 
        lblImageDrm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageDrm.Location = new Point(701, 136);
        lblImageDrm.Name = "lblImageDrm";
        lblImageDrm.Size = new Size(44, 15);
        lblImageDrm.TabIndex = 55;
        lblImageDrm.Text = "DRM:";
        lblImageDrm.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageDrm
        // 
        cboImageDrm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImageDrm.Location = new Point(751, 131);
        cboImageDrm.Name = "cboImageDrm";
        cboImageDrm.Size = new Size(130, 24);
        cboImageDrm.TabIndex = 56;
        cboImageDrm.SelectedIndexChanged += cboImageDrm_SelectedIndexChanged;
        // 
        // lblImageBackend
        // 
        lblImageBackend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblImageBackend.Location = new Point(893, 136);
        lblImageBackend.Name = "lblImageBackend";
        lblImageBackend.Size = new Size(56, 15);
        lblImageBackend.TabIndex = 57;
        lblImageBackend.Text = "Builder:";
        lblImageBackend.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cboImageBackend
        // 
        cboImageBackend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboImageBackend.Location = new Point(951, 131);
        cboImageBackend.Name = "cboImageBackend";
        cboImageBackend.Size = new Size(150, 24);
        cboImageBackend.TabIndex = 58;
        cboImageBackend.SelectedIndexChanged += cboImageBackend_SelectedIndexChanged;
        // 
        // chkImageFakeSign
        // 
        chkImageFakeSign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkImageFakeSign.AutoSize = true;
        chkImageFakeSign.Checked = true;
        chkImageFakeSign.CheckState = CheckState.Checked;
        chkImageFakeSign.Location = new Point(981, 75);
        chkImageFakeSign.Name = "chkImageFakeSign";
        chkImageFakeSign.Size = new Size(126, 19);
        chkImageFakeSign.TabIndex = 62;
        chkImageFakeSign.Text = "Fake-sign modules";
        // 
        // chkImageRightSprx
        // 
        chkImageRightSprx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkImageRightSprx.AutoSize = true;
        chkImageRightSprx.Checked = true;
        chkImageRightSprx.CheckState = CheckState.Checked;
        chkImageRightSprx.Location = new Point(981, 104);
        chkImageRightSprx.Name = "chkImageRightSprx";
        chkImageRightSprx.Size = new Size(144, 19);
        chkImageRightSprx.TabIndex = 63;
        chkImageRightSprx.Text = "Inject debug right.sprx";
        // 
        // chkImageDeterministic
        // 
        chkImageDeterministic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkImageDeterministic.AutoSize = true;
        chkImageDeterministic.Location = new Point(836, 104);
        chkImageDeterministic.Name = "chkImageDeterministic";
        chkImageDeterministic.Size = new Size(126, 19);
        chkImageDeterministic.TabIndex = 64;
        chkImageDeterministic.Text = "Deterministic build";
        // 
        // chkImageAdvancedOptions
        // 
        chkImageAdvancedOptions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkImageAdvancedOptions.AutoSize = true;
        chkImageAdvancedOptions.Location = new Point(981, 44);
        chkImageAdvancedOptions.Name = "chkImageAdvancedOptions";
        chkImageAdvancedOptions.Size = new Size(122, 19);
        chkImageAdvancedOptions.TabIndex = 59;
        chkImageAdvancedOptions.Text = "Advanced options";
        chkImageAdvancedOptions.CheckedChanged += chkImageAdvancedOptions_CheckedChanged;
        // 
        // tabTargetOptions
        // 
        tabTargetOptions.BackColor = Color.FromArgb(60, 63, 65);
        tabTargetOptions.Controls.Add(lblImageOutput);
        tabTargetOptions.Controls.Add(txtImageOutput);
        tabTargetOptions.Controls.Add(btnImageBrowseOutput);
        tabTargetOptions.Controls.Add(chkImageOverwrite);
        tabTargetOptions.Controls.Add(lblImagePasscode);
        tabTargetOptions.Controls.Add(txtImagePasscode);
        tabTargetOptions.Location = new Point(4, 32);
        tabTargetOptions.Name = "tabTargetOptions";
        tabTargetOptions.Size = new Size(1342, 152);
        tabTargetOptions.TabIndex = 4;
        tabTargetOptions.Text = "Options";
        // 
        // lblImageOutput
        // 
        lblImageOutput.Location = new Point(12, 16);
        lblImageOutput.Name = "lblImageOutput";
        lblImageOutput.Size = new Size(100, 15);
        lblImageOutput.TabIndex = 9;
        lblImageOutput.Text = "Output:";
        lblImageOutput.TextAlign = ContentAlignment.MiddleRight;
        lblImageOutput.Visible = false;
        // 
        // txtImageOutput
        // 
        txtImageOutput.Location = new Point(120, 12);
        txtImageOutput.Name = "txtImageOutput";
        txtImageOutput.Size = new Size(560, 23);
        txtImageOutput.TabIndex = 10;
        txtImageOutput.Visible = false;
        // 
        // btnImageBrowseOutput
        // 
        btnImageBrowseOutput.Location = new Point(688, 12);
        btnImageBrowseOutput.Name = "btnImageBrowseOutput";
        btnImageBrowseOutput.Size = new Size(80, 24);
        btnImageBrowseOutput.TabIndex = 11;
        btnImageBrowseOutput.Text = "Browse...";
        btnImageBrowseOutput.Visible = false;
        btnImageBrowseOutput.Click += btnImageBrowseOutput_Click;
        // 
        // chkImageOverwrite
        // 
        chkImageOverwrite.AutoSize = true;
        chkImageOverwrite.Location = new Point(776, 16);
        chkImageOverwrite.Name = "chkImageOverwrite";
        chkImageOverwrite.Size = new Size(120, 19);
        chkImageOverwrite.TabIndex = 12;
        chkImageOverwrite.Text = "Overwrite existing";
        chkImageOverwrite.Visible = false;
        // 
        // lblImagePasscode
        // 
        lblImagePasscode.Location = new Point(12, 46);
        lblImagePasscode.Name = "lblImagePasscode";
        lblImagePasscode.Size = new Size(100, 15);
        lblImagePasscode.TabIndex = 34;
        lblImagePasscode.Text = "Passcode:";
        lblImagePasscode.TextAlign = ContentAlignment.MiddleRight;
        lblImagePasscode.Visible = false;
        // 
        // txtImagePasscode
        // 
        txtImagePasscode.Location = new Point(120, 42);
        txtImagePasscode.Name = "txtImagePasscode";
        txtImagePasscode.Size = new Size(300, 23);
        txtImagePasscode.TabIndex = 35;
        txtImagePasscode.Visible = false;
        // 
        // toolsFooter
        // 
        toolsFooter.Controls.Add(lblImageStatus);
        toolsFooter.Controls.Add(btnImageRun);
        toolsFooter.Controls.Add(btnImageCancel);
        toolsFooter.Dock = DockStyle.Fill;
        toolsFooter.Location = new Point(10, 317);
        toolsFooter.Margin = new Padding(0);
        toolsFooter.Name = "toolsFooter";
        toolsFooter.Size = new Size(1356, 50);
        toolsFooter.TabIndex = 2;
        // 
        // lblImageStatus
        // 
        lblImageStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblImageStatus.AutoEllipsis = true;
        lblImageStatus.Location = new Point(12, 14);
        lblImageStatus.Name = "lblImageStatus";
        lblImageStatus.Size = new Size(1086, 18);
        lblImageStatus.TabIndex = 20;
        lblImageStatus.Text = "Select a source, then choose an action.";
        // 
        // btnImageRun
        // 
        btnImageRun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnImageRun.Location = new Point(1214, 8);
        btnImageRun.Name = "btnImageRun";
        btnImageRun.Size = new Size(130, 30);
        btnImageRun.TabIndex = 21;
        btnImageRun.Text = "Run";
        btnImageRun.Click += btnImageRun_Click;
        // 
        // btnImageCancel
        // 
        btnImageCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnImageCancel.Location = new Point(1110, 8);
        btnImageCancel.Name = "btnImageCancel";
        btnImageCancel.Size = new Size(96, 30);
        btnImageCancel.TabIndex = 22;
        btnImageCancel.Text = "Cancel";
        btnImageCancel.Click += btnImageCancel_Click;
        // 
        // tabTasks
        // 
        tabTasks.BackColor = Color.FromArgb(60, 63, 65);
        tabTasks.Controls.Add(tasksLayout);
        tabTasks.Location = new Point(4, 32);
        tabTasks.Name = "tabTasks";
        tabTasks.Size = new Size(1376, 375);
        tabTasks.TabIndex = 2;
        tabTasks.Text = "Tasks";
        // 
        // tasksLayout
        // 
        tasksLayout.ColumnCount = 14;
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        tasksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tasksLayout.Controls.Add(chkTaskAutoStart, 0, 0);
        tasksLayout.Controls.Add(btnTaskStart, 1, 0);
        tasksLayout.Controls.Add(btnTaskCancel, 2, 0);
        tasksLayout.Controls.Add(btnTaskRetry, 3, 0);
        tasksLayout.Controls.Add(btnTaskRemove, 4, 0);
        tasksLayout.Controls.Add(btnTaskOpen, 5, 0);
        tasksLayout.Controls.Add(btnTaskClear, 6, 0);
        tasksLayout.Controls.Add(lblTaskGroup, 7, 0);
        tasksLayout.Controls.Add(cboTaskGroup, 8, 0);
        tasksLayout.Controls.Add(cboTaskFilter, 9, 0);
        tasksLayout.Controls.Add(searchTasks, 10, 0);
        tasksLayout.Controls.Add(btnTaskToggleDetails, 11, 0);
        tasksLayout.Controls.Add(chkTaskFollow, 12, 0);
        tasksLayout.Controls.Add(lblTaskSummary, 13, 0);
        tasksLayout.Controls.Add(splitTasks, 0, 1);
        tasksLayout.Dock = DockStyle.Fill;
        tasksLayout.Location = new Point(0, 0);
        tasksLayout.Name = "tasksLayout";
        tasksLayout.RowCount = 2;
        tasksLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tasksLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tasksLayout.Size = new Size(1376, 375);
        tasksLayout.TabIndex = 0;
        // 
        // chkTaskAutoStart
        // 
        chkTaskAutoStart.Anchor = AnchorStyles.Left;
        chkTaskAutoStart.AutoSize = true;
        chkTaskAutoStart.Location = new Point(3, 9);
        chkTaskAutoStart.Name = "chkTaskAutoStart";
        chkTaskAutoStart.Size = new Size(86, 19);
        chkTaskAutoStart.TabIndex = 0;
        chkTaskAutoStart.Text = "Run queue";
        chkTaskAutoStart.CheckedChanged += chkTaskAutoStart_CheckedChanged;
        // 
        // btnTaskStart
        // 
        btnTaskStart.Anchor = AnchorStyles.Left;
        btnTaskStart.Location = new Point(83, 6);
        btnTaskStart.Name = "btnTaskStart";
        btnTaskStart.Size = new Size(80, 26);
        btnTaskStart.TabIndex = 1;
        btnTaskStart.Text = "Start Next";
        btnTaskStart.Click += btnTaskStart_Click;
        // 
        // btnTaskCancel
        // 
        btnTaskCancel.Anchor = AnchorStyles.Left;
        btnTaskCancel.Location = new Point(169, 6);
        btnTaskCancel.Name = "btnTaskCancel";
        btnTaskCancel.Size = new Size(80, 26);
        btnTaskCancel.TabIndex = 2;
        btnTaskCancel.Text = "Cancel";
        btnTaskCancel.Click += btnTaskCancel_Click;
        // 
        // btnTaskRetry
        // 
        btnTaskRetry.Anchor = AnchorStyles.Left;
        btnTaskRetry.Location = new Point(255, 6);
        btnTaskRetry.Name = "btnTaskRetry";
        btnTaskRetry.Size = new Size(80, 26);
        btnTaskRetry.TabIndex = 3;
        btnTaskRetry.Text = "Retry";
        btnTaskRetry.Click += btnTaskRetry_Click;
        // 
        // btnTaskRemove
        // 
        btnTaskRemove.Anchor = AnchorStyles.Left;
        btnTaskRemove.Location = new Point(341, 6);
        btnTaskRemove.Name = "btnTaskRemove";
        btnTaskRemove.Size = new Size(80, 26);
        btnTaskRemove.TabIndex = 4;
        btnTaskRemove.Text = "Remove";
        btnTaskRemove.Click += btnTaskRemove_Click;
        // 
        // btnTaskOpen
        // 
        btnTaskOpen.Anchor = AnchorStyles.Left;
        btnTaskOpen.Location = new Point(427, 6);
        btnTaskOpen.Name = "btnTaskOpen";
        btnTaskOpen.Size = new Size(90, 26);
        btnTaskOpen.TabIndex = 5;
        btnTaskOpen.Text = "Open Output";
        btnTaskOpen.Click += btnTaskOpen_Click;
        // 
        // btnTaskClear
        // 
        btnTaskClear.Anchor = AnchorStyles.Left;
        btnTaskClear.Location = new Point(523, 6);
        btnTaskClear.Name = "btnTaskClear";
        btnTaskClear.Size = new Size(104, 26);
        btnTaskClear.TabIndex = 6;
        btnTaskClear.Text = "Clear successful";
        btnTaskClear.Click += btnTaskClear_Click;
        // 
        // lblTaskGroup
        // 
        lblTaskGroup.Anchor = AnchorStyles.Left;
        lblTaskGroup.Location = new Point(633, 11);
        lblTaskGroup.Name = "lblTaskGroup";
        lblTaskGroup.Size = new Size(43, 15);
        lblTaskGroup.TabIndex = 7;
        lblTaskGroup.Text = "Group:";
        // 
        // cboTaskGroup
        // 
        cboTaskGroup.Anchor = AnchorStyles.Left;
        cboTaskGroup.Items.AddRange(new object[] { "None", "Status" });
        cboTaskGroup.Location = new Point(683, 7);
        cboTaskGroup.Name = "cboTaskGroup";
        cboTaskGroup.Size = new Size(94, 24);
        cboTaskGroup.TabIndex = 8;
        cboTaskGroup.SelectedIndexChanged += cboTaskGroup_SelectedIndexChanged;
        // 
        // lblTaskSummary
        // 
        lblTaskSummary.Dock = DockStyle.Fill;
        lblTaskSummary.Location = new Point(783, 0);
        lblTaskSummary.Name = "lblTaskSummary";
        lblTaskSummary.Size = new Size(590, 38);
        lblTaskSummary.TabIndex = 9;
        lblTaskSummary.Text = "Queue is empty.";
        lblTaskSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // chkTaskFollow
        // 
        chkTaskFollow.Anchor = AnchorStyles.Left;
        chkTaskFollow.AutoSize = true;
        chkTaskFollow.Location = new Point(513, 9);
        chkTaskFollow.Margin = new Padding(3, 3, 3, 3);
        chkTaskFollow.Name = "chkTaskFollow";
        chkTaskFollow.Size = new Size(114, 19);
        chkTaskFollow.TabIndex = 10;
        chkTaskFollow.Text = "Follow running";
        chkTaskFollow.Checked = true;
        chkTaskFollow.CheckedChanged += chkTaskFollow_CheckedChanged;
        // 
        // cboTaskFilter
        // 
        cboTaskFilter.Anchor = AnchorStyles.Left;
        cboTaskFilter.Location = new Point(443, 7);
        cboTaskFilter.Margin = new Padding(3, 3, 3, 3);
        cboTaskFilter.Name = "cboTaskFilter";
        cboTaskFilter.Size = new Size(124, 24);
        cboTaskFilter.TabIndex = 10;
        cboTaskFilter.SelectedIndexChanged += cboTaskFilter_SelectedIndexChanged;
        // 
        // searchTasks
        // 
        searchTasks.AccessibleDescription = "Filter the task list by title or status.";
        searchTasks.AccessibleName = "Task search";
        searchTasks.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        searchTasks.Location = new Point(613, 5);
        searchTasks.Margin = new Padding(3, 3, 3, 3);
        searchTasks.Name = "searchTasks";
        searchTasks.Size = new Size(164, 28);
        searchTasks.TabIndex = 10;
        searchTasks.SearchTextChanged += searchTasks_SearchTextChanged;
        // 
        // btnTaskToggleDetails
        // 
        btnTaskToggleDetails.Anchor = AnchorStyles.Left;
        btnTaskToggleDetails.Location = new Point(783, 6);
        btnTaskToggleDetails.Margin = new Padding(3, 3, 3, 3);
        btnTaskToggleDetails.Name = "btnTaskToggleDetails";
        btnTaskToggleDetails.Size = new Size(90, 26);
        btnTaskToggleDetails.TabIndex = 11;
        btnTaskToggleDetails.Text = "Details";
        btnTaskToggleDetails.Click += btnTaskToggleDetails_Click;
        // 
        // splitTasks
        // 
        tasksLayout.SetColumnSpan(splitTasks, 14);
        splitTasks.Controls.Add(splitTasksPane1);
        splitTasks.Controls.Add(splitTasksPane2);
        splitTasks.Dock = DockStyle.Fill;
        splitTasks.Location = new Point(3, 41);
        splitTasks.Name = "splitTasks";
        splitTasks.Orientation = DarkUI.Controls.DarkSplitContainer.DarkSplitOrientation.Horizontal;
        splitTasks.Size = new Size(1370, 331);
        splitTasks.TabIndex = 1;
        // 
        // splitTasksPane1
        // 
        splitTasksPane1.Controls.Add(sectionTasksList);
        splitTasksPane1.Location = new Point(0, 0);
        splitTasksPane1.Name = "splitTasksPane1";
        splitTasksPane1.Size = new Size(1370, 163);
        splitTasksPane1.TabIndex = 0;
        // 
        // sectionTasksList
        // 
        sectionTasksList.Controls.Add(gridTasks);
        sectionTasksList.Dock = DockStyle.Fill;
        sectionTasksList.Location = new Point(0, 0);
        sectionTasksList.Margin = new Padding(0);
        sectionTasksList.Name = "sectionTasksList";
        sectionTasksList.SectionHeader = "Task Queue";
        sectionTasksList.Size = new Size(1370, 163);
        sectionTasksList.TabIndex = 0;
        // 
        // gridTasks
        // 
        gridTasks.AllowUserToAddRows = false;
        gridTasks.AllowUserToDeleteRows = false;
        gridTasks.AllowUserToDragDropRows = false;
        gridTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridTasks.AutoSortGroups = true;
        gridTasks.Columns.AddRange(new DataGridViewColumn[] { colTaskName, colTaskOperation, colTaskRoute, colTaskStatus, colTaskStage, colTaskProgress, colTaskElapsed });
        gridTasks.ContextMenuStrip = contextTasks;
        gridTasks.Dock = DockStyle.Fill;
        gridTasks.GroupCellValueComparer = null;
        gridTasks.GroupHeaderColumnIndex = 0;
        gridTasks.GroupHeaderColumnName = null;
        gridTasks.AccessibleDescription = "Queued, running and finished package tasks.";
        gridTasks.AccessibleName = "Task list";
        gridTasks.GroupHeaderHeight = 26F;
        gridTasks.GroupLabelFormatter = null;
        gridTasks.Location = new Point(1, 25);
        gridTasks.MultiSelect = false;
        gridTasks.Name = "gridTasks";
        gridTasks.ReadOnly = true;
        gridTasks.RowTemplate.Height = 24;
        gridTasks.Size = new Size(1368, 137);
        gridTasks.TabIndex = 0;
        gridTasks.CellDoubleClick += gridTasks_CellDoubleClick;
        gridTasks.CellMouseDown += gridTasks_CellMouseDown;
        gridTasks.SelectionChanged += gridTasks_SelectionChanged;
        // 
        // colTaskName
        // 
        colTaskName.FillWeight = 220F;
        colTaskName.HeaderText = "Filename";
        colTaskName.Name = "colTaskName";
        colTaskName.ReadOnly = true;
        // 
        // colTaskOperation
        // 
        colTaskOperation.FillWeight = 90F;
        colTaskOperation.HeaderText = "Operation";
        colTaskOperation.Name = "colTaskOperation";
        colTaskOperation.ReadOnly = true;
        // 
        // colTaskRoute
        // 
        colTaskRoute.HeaderText = "From → To";
        colTaskRoute.Name = "colTaskRoute";
        colTaskRoute.ReadOnly = true;
        // 
        // colTaskStatus
        // 
        colTaskStatus.FillWeight = 80F;
        colTaskStatus.HeaderText = "Status";
        colTaskStatus.Name = "colTaskStatus";
        colTaskStatus.ReadOnly = true;
        // 
        // colTaskStage
        // 
        colTaskStage.FillWeight = 130F;
        colTaskStage.HeaderText = "Stage";
        colTaskStage.Name = "colTaskStage";
        colTaskStage.ReadOnly = true;
        // 
        // colTaskProgress
        // 
        colTaskProgress.FillWeight = 60F;
        colTaskProgress.HeaderText = "Progress";
        colTaskProgress.Name = "colTaskProgress";
        colTaskProgress.ReadOnly = true;
        // 
        // colTaskElapsed
        // 
        colTaskElapsed.FillWeight = 60F;
        colTaskElapsed.HeaderText = "Elapsed";
        colTaskElapsed.Name = "colTaskElapsed";
        colTaskElapsed.ReadOnly = true;
        // 
        // contextTasks
        // 
        contextTasks.Items.AddRange(new ToolStripItem[] { menuTaskStart, menuTaskCancel, menuTaskRetry, menuTaskRemove, menuTaskSeparator1, menuTaskOpen, menuTaskShowSource, menuTaskReport, menuTaskClear });
        contextTasks.Name = "contextTasks";
        contextTasks.Size = new Size(164, 143);
        contextTasks.Opening += contextTasks_Opening;
        // 
        // menuTaskStart
        // 
        menuTaskStart.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskStart.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskStart.Name = "menuTaskStart";
        menuTaskStart.Size = new Size(163, 22);
        menuTaskStart.Text = "Start Next";
        menuTaskStart.Click += btnTaskStart_Click;
        // 
        // menuTaskCancel
        // 
        menuTaskCancel.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskCancel.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskCancel.Name = "menuTaskCancel";
        menuTaskCancel.Size = new Size(163, 22);
        menuTaskCancel.Text = "Cancel";
        menuTaskCancel.Click += btnTaskCancel_Click;
        // 
        // menuTaskRetry
        // 
        menuTaskRetry.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskRetry.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskRetry.Name = "menuTaskRetry";
        menuTaskRetry.Size = new Size(163, 22);
        menuTaskRetry.Text = "Retry";
        menuTaskRetry.Click += btnTaskRetry_Click;
        // 
        // menuTaskRemove
        // 
        menuTaskRemove.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskRemove.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskRemove.Name = "menuTaskRemove";
        menuTaskRemove.Size = new Size(163, 22);
        menuTaskRemove.Text = "Remove";
        menuTaskRemove.Click += btnTaskRemove_Click;
        // 
        // menuTaskSeparator1
        // 
        menuTaskSeparator1.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskSeparator1.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskSeparator1.Margin = new Padding(0, 0, 0, 1);
        menuTaskSeparator1.Name = "menuTaskSeparator1";
        menuTaskSeparator1.Size = new Size(160, 6);
        // 
        // menuTaskOpen
        // 
        menuTaskOpen.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskOpen.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskOpen.Name = "menuTaskOpen";
        menuTaskOpen.Size = new Size(163, 22);
        menuTaskOpen.Text = "Open Output";
        menuTaskOpen.Click += btnTaskOpen_Click;
        // 
        // menuTaskShowSource
        // 
        menuTaskShowSource.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskShowSource.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskShowSource.Name = "menuTaskShowSource";
        menuTaskShowSource.Size = new Size(163, 22);
        menuTaskShowSource.Text = "Show Source";
        menuTaskShowSource.Click += btnTaskShowSource_Click;
        // 
        // menuTaskReport
        // 
        menuTaskReport.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskReport.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskReport.Name = "menuTaskReport";
        menuTaskReport.Size = new Size(163, 22);
        menuTaskReport.Text = "Export Report...";
        menuTaskReport.Click += btnTaskReport_Click;
        // 
        // menuTaskClear
        // 
        menuTaskClear.BackColor = Color.FromArgb(60, 63, 65);
        menuTaskClear.ForeColor = Color.FromArgb(220, 220, 220);
        menuTaskClear.Name = "menuTaskClear";
        menuTaskClear.Size = new Size(163, 22);
        menuTaskClear.Text = "Clear Completed";
        menuTaskClear.Click += btnTaskClear_Click;
        // 
        // splitTasksPane2
        // 
        splitTasksPane2.Controls.Add(sectionTaskDetails);
        splitTasksPane2.Location = new Point(0, 168);
        splitTasksPane2.Name = "splitTasksPane2";
        splitTasksPane2.Size = new Size(1370, 163);
        splitTasksPane2.TabIndex = 1;
        // 
        // sectionTaskDetails
        // 
        sectionTaskDetails.Controls.Add(taskDetailLayout);
        sectionTaskDetails.Dock = DockStyle.Fill;
        sectionTaskDetails.Location = new Point(0, 0);
        sectionTaskDetails.Margin = new Padding(0);
        sectionTaskDetails.Name = "sectionTaskDetails";
        sectionTaskDetails.SectionHeader = "Task Details";
        sectionTaskDetails.Size = new Size(1370, 163);
        sectionTaskDetails.TabIndex = 0;
        // 
        // taskDetailLayout
        // 
        taskDetailLayout.ColumnCount = 2;
        taskDetailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        taskDetailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        taskDetailLayout.Controls.Add(lblTaskStage, 0, 0);
        taskDetailLayout.Controls.Add(lblTaskCurrentCaption, 0, 1);
        taskDetailLayout.Controls.Add(barTaskCurrent, 1, 1);
        taskDetailLayout.Controls.Add(lblTaskOverallCaption, 0, 2);
        taskDetailLayout.Controls.Add(barTaskOverall, 1, 2);
        taskDetailLayout.Controls.Add(lblTaskMessage, 0, 3);
        taskDetailLayout.Controls.Add(lblTaskMeta, 0, 4);
        taskDetailLayout.Controls.Add(lblTaskResult, 0, 5);
        taskDetailLayout.Controls.Add(btnTaskDiagnostic, 0, 6);
        taskDetailLayout.Dock = DockStyle.Fill;
        taskDetailLayout.Location = new Point(1, 25);
        taskDetailLayout.Name = "taskDetailLayout";
        taskDetailLayout.Padding = new Padding(10, 2, 10, 4);
        taskDetailLayout.RowCount = 7;
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        taskDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        taskDetailLayout.Size = new Size(1368, 137);
        taskDetailLayout.TabIndex = 0;
        // 
        // lblTaskStage
        // 
        taskDetailLayout.SetColumnSpan(lblTaskStage, 2);
        lblTaskStage.Dock = DockStyle.Fill;
        lblTaskStage.Location = new Point(13, 2);
        lblTaskStage.Name = "lblTaskStage";
        lblTaskStage.Size = new Size(1342, 20);
        lblTaskStage.TabIndex = 0;
        lblTaskStage.Text = "No task selected.";
        lblTaskStage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblTaskCurrentCaption
        // 
        lblTaskCurrentCaption.Dock = DockStyle.Fill;
        lblTaskCurrentCaption.Location = new Point(13, 22);
        lblTaskCurrentCaption.Name = "lblTaskCurrentCaption";
        lblTaskCurrentCaption.Size = new Size(114, 22);
        lblTaskCurrentCaption.TabIndex = 1;
        lblTaskCurrentCaption.Text = "Step progress";
        lblTaskCurrentCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // barTaskCurrent
        // 
        barTaskCurrent.Dock = DockStyle.Fill;
        barTaskCurrent.Location = new Point(133, 25);
        barTaskCurrent.Name = "barTaskCurrent";
        barTaskCurrent.Size = new Size(1222, 16);
        barTaskCurrent.TabIndex = 2;
        // 
        // lblTaskOverallCaption
        // 
        lblTaskOverallCaption.Dock = DockStyle.Fill;
        lblTaskOverallCaption.Location = new Point(13, 44);
        lblTaskOverallCaption.Name = "lblTaskOverallCaption";
        lblTaskOverallCaption.Size = new Size(114, 22);
        lblTaskOverallCaption.TabIndex = 3;
        lblTaskOverallCaption.Text = "Task steps";
        lblTaskOverallCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // barTaskOverall
        // 
        barTaskOverall.Dock = DockStyle.Fill;
        barTaskOverall.Location = new Point(133, 47);
        barTaskOverall.Name = "barTaskOverall";
        barTaskOverall.Size = new Size(1222, 16);
        barTaskOverall.TabIndex = 4;
        // 
        // lblTaskMessage
        // 
        lblTaskMessage.AutoEllipsis = true;
        taskDetailLayout.SetColumnSpan(lblTaskMessage, 2);
        lblTaskMessage.Dock = DockStyle.Fill;
        lblTaskMessage.Location = new Point(13, 66);
        lblTaskMessage.Name = "lblTaskMessage";
        lblTaskMessage.Size = new Size(1342, 18);
        lblTaskMessage.TabIndex = 5;
        lblTaskMessage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblTaskMeta
        // 
        lblTaskMeta.AutoEllipsis = true;
        taskDetailLayout.SetColumnSpan(lblTaskMeta, 2);
        lblTaskMeta.Dock = DockStyle.Fill;
        lblTaskMeta.Location = new Point(13, 84);
        lblTaskMeta.Name = "lblTaskMeta";
        lblTaskMeta.Size = new Size(1342, 49);
        lblTaskMeta.TabIndex = 6;
        // 
        // lblTaskResult
        // 
        lblTaskResult.AutoEllipsis = true;
        taskDetailLayout.SetColumnSpan(lblTaskResult, 2);
        lblTaskResult.Dock = DockStyle.Fill;
        lblTaskResult.Location = new Point(13, 133);
        lblTaskResult.Name = "lblTaskResult";
        lblTaskResult.Size = new Size(1342, 20);
        lblTaskResult.TabIndex = 7;
        lblTaskResult.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnTaskDiagnostic
        // 
        taskDetailLayout.SetColumnSpan(btnTaskDiagnostic, 2);
        btnTaskDiagnostic.Location = new Point(13, 156);
        btnTaskDiagnostic.Name = "btnTaskDiagnostic";
        btnTaskDiagnostic.Size = new Size(160, 24);
        btnTaskDiagnostic.TabIndex = 8;
        btnTaskDiagnostic.Text = "Diagnostic...";
        btnTaskDiagnostic.Click += btnTaskDiagnostic_Click;
        lblTaskMeta.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tabLog
        // 
        tabLog.BackColor = Color.FromArgb(60, 63, 65);
        tabLog.Controls.Add(txtLogView);
        tabLog.Controls.Add(lblLogLevel);
        tabLog.Controls.Add(cboLogLevel);
        tabLog.Controls.Add(chkLogAutoScroll);
        tabLog.Controls.Add(btnLogClear);
        tabLog.Controls.Add(btnLogOpenFolder);
        tabLog.Location = new Point(4, 32);
        tabLog.Name = "tabLog";
        tabLog.Padding = new Padding(0, 36, 0, 0);
        tabLog.Size = new Size(1376, 375);
        tabLog.TabIndex = 3;
        tabLog.Text = "Log";
        // 
        // txtLogView
        // 
        txtLogView.Dock = DockStyle.Fill;
        txtLogView.Font = new Font("Consolas", 9F);
        txtLogView.Location = new Point(0, 36);
        txtLogView.Name = "txtLogView";
        txtLogView.ReadOnly = true;
        txtLogView.SelectionColor = Color.Gainsboro;
        txtLogView.SelectionFont = new Font("Consolas", 9F);
        txtLogView.Size = new Size(1376, 339);
        txtLogView.TabIndex = 5;
        txtLogView.TextPadding = new Padding(3);
        txtLogView.WordWrap = false;
        // 
        // lblLogLevel
        // 
        lblLogLevel.Location = new Point(8, 10);
        lblLogLevel.Name = "lblLogLevel";
        lblLogLevel.Size = new Size(44, 15);
        lblLogLevel.TabIndex = 0;
        lblLogLevel.Text = "Show:";
        // 
        // cboLogLevel
        // 
        cboLogLevel.Items.AddRange(new object[] { "All", "Info", "Warning", "Error" });
        cboLogLevel.Location = new Point(54, 6);
        cboLogLevel.Name = "cboLogLevel";
        cboLogLevel.Size = new Size(110, 24);
        cboLogLevel.TabIndex = 1;
        cboLogLevel.SelectedIndexChanged += cboLogLevel_SelectedIndexChanged;
        // 
        // chkLogAutoScroll
        // 
        chkLogAutoScroll.AutoSize = true;
        chkLogAutoScroll.Location = new Point(178, 9);
        chkLogAutoScroll.Name = "chkLogAutoScroll";
        chkLogAutoScroll.Size = new Size(85, 19);
        chkLogAutoScroll.TabIndex = 2;
        chkLogAutoScroll.Text = "Auto-scroll";
        chkLogAutoScroll.CheckedChanged += chkLogAutoScroll_CheckedChanged;
        // 
        // btnLogClear
        // 
        btnLogClear.Location = new Point(286, 5);
        btnLogClear.Name = "btnLogClear";
        btnLogClear.Size = new Size(70, 26);
        btnLogClear.TabIndex = 3;
        btnLogClear.Text = "Clear";
        btnLogClear.Click += btnLogClear_Click;
        // 
        // btnLogOpenFolder
        // 
        btnLogOpenFolder.Location = new Point(364, 5);
        btnLogOpenFolder.Name = "btnLogOpenFolder";
        btnLogOpenFolder.Size = new Size(130, 26);
        btnLogOpenFolder.TabIndex = 4;
        btnLogOpenFolder.Text = "Open log folder";
        btnLogOpenFolder.Click += btnLogOpenFolder_Click;
        // 
        // colFileName
        // 
        colFileName.Text = "Name";
        colFileName.Width = 260;
        // 
        // colFileType
        // 
        colFileType.Text = "Type";
        colFileType.Width = 100;
        // 
        // colFilePath
        // 
        colFilePath.Text = "Path";
        colFilePath.Width = 420;
        // 
        // colFileSize
        // 
        colFileSize.Text = "Size";
        colFileSize.TextAlign = HorizontalAlignment.Right;
        colFileSize.Width = 30;
        // 
        // imageSaveDialog
        // 
        imageSaveDialog.Title = "Select the output image";
        // 
        // trophyCsvSaveDialog
        // 
        trophyCsvSaveDialog.DefaultExt = "csv";
        trophyCsvSaveDialog.FileName = "trophies.csv";
        trophyCsvSaveDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
        trophyCsvSaveDialog.Title = "Export trophies";
        // 
        // statusMain
        // 
        statusMain.Items.AddRange(new ToolStripItem[] { statusLabel, statusSpring, statusCount });
        statusMain.Location = new Point(0, 851);
        statusMain.Name = "statusMain";
        statusMain.Padding = new Padding(0, 4, 0, 4);
        statusMain.Size = new Size(1384, 30);
        statusMain.TabIndex = 3;
        // 
        // statusLabel
        // 
        statusLabel.BackColor = Color.FromArgb(60, 63, 65);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
        statusLabel.Text = "Ready";
        // 
        // statusSpring
        // 
        statusSpring.BackColor = Color.FromArgb(60, 63, 65);
        statusSpring.Name = "statusSpring";
        statusSpring.Size = new Size(1294, 17);
        statusSpring.Spring = true;
        // 
        // statusCount
        // 
        statusCount.BackColor = Color.FromArgb(60, 63, 65);
        statusCount.Name = "statusCount";
        statusCount.Size = new Size(51, 17);
        statusCount.Text = "0 games";
        // 
        // packageOpenDialog
        // 
        packageOpenDialog.DefaultExt = "ffpfsc";
        packageOpenDialog.Filter = resources.GetString("packageOpenDialog.Filter");
        packageOpenDialog.Title = "Open a PS5 package or filesystem image";
        // 
        // sourceImageOpenDialog
        // 
        sourceImageOpenDialog.Filter = "PS5 filesystem images (*.exfat;*.ffpkg)|*.exfat;*.ffpkg|All files (*.*)|*.*";
        sourceImageOpenDialog.Title = "Select a PS5 filesystem image";
        // 
        // containedFileSaveDialog
        // 
        containedFileSaveDialog.Filter = "All files (*.*)|*.*";
        containedFileSaveDialog.Title = "Extract file from FFPFSC";
        // 
        // artworkSaveDialog
        // 
        artworkSaveDialog.DefaultExt = "png";
        artworkSaveDialog.FileName = "artwork.png";
        artworkSaveDialog.Filter = "PNG image (*.png)|*.png|JPEG image (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp|All files (*.*)|*.*";
        artworkSaveDialog.Title = "Save artwork image";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1384, 881);
        Controls.Add(splitMain);
        Controls.Add(statusMain);
        Controls.Add(menuMain);
        Icon = (Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        MainMenuStrip = menuMain;
        MinimumSize = new Size(1050, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PS5 PKG Tool";
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        KeyDown += MainForm_KeyDown;
        menuMain.ResumeLayout(false);
        menuMain.PerformLayout();
        contextLibrary.ResumeLayout(false);
        splitMain.ResumeLayout(false);
        splitMainPane1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridLibrary).EndInit();
        splitMainPane2.ResumeLayout(false);
        tabsWorkspace.ResumeLayout(false);
        tabWorkspaceGeneral.ResumeLayout(false);
        tabsDetails.ResumeLayout(false);
        tabOverview.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridOverview).EndInit();
        tabArtwork.ResumeLayout(false);
        splitArtwork.ResumeLayout(false);
        splitArtworkPane1.ResumeLayout(false);
        sectionIcon.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureIcon).EndInit();
        contextArtwork.ResumeLayout(false);
        splitArtworkPane2.ResumeLayout(false);
        sectionBackground.ResumeLayout(false);
        tabsBackgrounds.ResumeLayout(false);
        tabPic0.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground0).EndInit();
        tabPic1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground1).EndInit();
        tabPic2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground2).EndInit();
        tabTrophies.ResumeLayout(false);
        tabTrophies.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridTrophies).EndInit();
        contextTrophies.ResumeLayout(false);
        tabActivities.ResumeLayout(false);
        tabsUds.ResumeLayout(false);
        tabUdsEvents.ResumeLayout(false);
        splitUdsEvents.ResumeLayout(false);
        splitUdsEventsPane1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridUdsEvents).EndInit();
        splitUdsEventsPane2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridUdsEventProperties).EndInit();
        tabUdsStats.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridUdsStats).EndInit();
        tabUdsEnums.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridUdsEnums).EndInit();
        tabUdsRules.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridUdsRules).EndInit();
        tabFiles.ResumeLayout(false);
        sectionFileBrowser.ResumeLayout(false);
        splitFileBrowser.ResumeLayout(false);
        splitFileBrowserPane1.ResumeLayout(false);
        contextTreeFiles.ResumeLayout(false);
        splitFileBrowserPane2.ResumeLayout(false);
        splitFileContentPreview.ResumeLayout(false);
        splitFileContentPreviewPane1.ResumeLayout(false);
        fileListPanel.ResumeLayout(false);
        contextFiles.ResumeLayout(false);
        splitFileContentPreviewPane2.ResumeLayout(false);
        sectionFileViewer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureFileViewer).EndInit();
        tabExecutable.ResumeLayout(false);
        tabsExecutable.ResumeLayout(false);
        tabExecModules.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridModules).EndInit();
        tabExecElf.ResumeLayout(false);
        splitExecElf.ResumeLayout(false);
        splitExecElfPane1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridElfPrograms).EndInit();
        splitExecElfPane2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridElfSections).EndInit();
        tabExecSelf.ResumeLayout(false);
        splitExecSelf.ResumeLayout(false);
        splitExecSelfPane1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridSelfHeader).EndInit();
        splitExecSelfPane2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridSelfSegments).EndInit();
        tabRaw.ResumeLayout(false);
        tabPackage.ResumeLayout(false);
        tabsPackage.ResumeLayout(false);
        tabPkgContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPkgHeader).EndInit();
        tabPkgSegments.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPkgSegments).EndInit();
        tabPkgEntries.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPkgEntries).EndInit();
        tabPkgSfo.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridParamSfo).EndInit();
        tabPkgKeystone.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridKeystone).EndInit();
        tabPkgSi.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridSi).EndInit();
        tabPkgPlayGo.ResumeLayout(false);
        tabsPlayGo.ResumeLayout(false);
        tabPlayGoChunks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPlayGoChunks).EndInit();
        tabPlayGoScenarios.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPlayGoScenarios).EndInit();
        tabPlayGoFiles.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridPlayGoFiles).EndInit();
        tabWorkspaceTools.ResumeLayout(false);
        toolsLayout.ResumeLayout(false);
        sectionJob.ResumeLayout(false);
        sectionJob.PerformLayout();
        tabsImageTargets.ResumeLayout(false);
        tabTargetExfat.ResumeLayout(false);
        tabTargetExfat.PerformLayout();
        tabTargetFfpkg.ResumeLayout(false);
        tabTargetFfpkg.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudImageMinFree).EndInit();
        tabTargetFfpfsc.ResumeLayout(false);
        tabTargetFfpfsc.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudImageLevel).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudImageGain).EndInit();
        tabTargetDebug.ResumeLayout(false);
        tabTargetDebug.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudImagePlayGoChunks).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudImageKrakenThreads).EndInit();
        tabTargetOptions.ResumeLayout(false);
        tabTargetOptions.PerformLayout();
        toolsFooter.ResumeLayout(false);
        tabTasks.ResumeLayout(false);
        tasksLayout.ResumeLayout(false);
        tasksLayout.PerformLayout();
        splitTasks.ResumeLayout(false);
        splitTasksPane1.ResumeLayout(false);
        sectionTasksList.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridTasks).EndInit();
        contextTasks.ResumeLayout(false);
        splitTasksPane2.ResumeLayout(false);
        sectionTaskDetails.ResumeLayout(false);
        taskDetailLayout.ResumeLayout(false);
        tabLog.ResumeLayout(false);
        tabLog.PerformLayout();
        statusMain.ResumeLayout(false);
        statusMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}