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
    private ToolStripSeparator menuSeparator = null!;
    private ToolStripMenuItem menuSettings = null!;
    private ToolStripMenuItem menuExit = null!;
    private ToolStripMenuItem menuTools = null!;
    private ToolStripMenuItem menuBuildDumpFfpfsc = null!;
    private ToolStripMenuItem menuBuildFfpfsc = null!;
    private ToolStripMenuItem menuVerifyFfpfsc = null!;
    private ToolStripMenuItem menuHelp = null!;
    private ToolStripMenuItem menuAbout = null!;
    private Panel commandPanel = null!;
    private DarkUI.Controls.DarkButton btnAddFolder = null!;
    private DarkUI.Controls.DarkButton btnRefresh = null!;
    private DarkUI.Controls.DarkButton btnCancel = null!;
    private DarkUI.Controls.DarkButton btnSettings = null!;
    private DarkUI.Controls.DarkLabel lblSearch = null!;
    private DarkUI.Controls.DarkTextBox txtSearch = null!;
    private SplitContainer splitMain = null!;
    private DarkUI.Controls.DarkDataGridView gridLibrary = null!;
    private DarkUI.Controls.DarkTabControl tabsWorkspace = null!;
    private TabPage tabWorkspaceGeneral = null!;
    private TabPage tabWorkspaceTools = null!;
    private DarkUI.Controls.DarkTabControl tabsDetails = null!;
    private TabPage tabOverview = null!;
    private DarkUI.Controls.DarkDataGridView gridOverview = null!;
    private TabPage tabArtwork = null!;
    private SplitContainer splitArtwork = null!;
    private DarkUI.Controls.DarkSectionPanel sectionIcon = null!;
    private PictureBox pictureIcon = null!;
    private DarkUI.Controls.DarkSectionPanel sectionBackground = null!;
    private DarkUI.Controls.DarkTabControl tabsBackgrounds = null!;
    private TabPage tabPic0 = null!;
    private PictureBox pictureBackground0 = null!;
    private TabPage tabPic1 = null!;
    private PictureBox pictureBackground1 = null!;
    private TabPage tabPic2 = null!;
    private PictureBox pictureBackground2 = null!;
    private TabPage tabTrophies = null!;
    private Panel trophyHeaderPanel = null!;
    private DarkUI.Controls.DarkLabel lblTrophySummary = null!;
    private DarkUI.Controls.DarkDataGridView gridTrophies = null!;
    private TabPage tabActivities = null!;
    private Panel activitiesHeaderPanel = null!;
    private DarkUI.Controls.DarkLabel lblActivitiesSummary = null!;
    private DarkUI.Controls.DarkDataGridView gridActivities = null!;
    private TabPage tabFiles = null!;
    private Panel filesHeaderPanel = null!;
    private DarkUI.Controls.DarkLabel lblFilesSummary = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFileBrowser = null!;
    private SplitContainer splitFileBrowser = null!;
    private DarkUI.Controls.DarkTreeView treeFiles = null!;
    private SplitContainer splitFileContentPreview = null!;
    private Panel fileListPanel = null!;
    private Panel fileFilterPanel = null!;
    private DarkUI.Controls.DarkTextBox txtFileFilter = null!;
    private DarkUI.Controls.DarkButton btnClearFileFilter = null!;
    private DarkUI.Controls.DarkListView listFiles = null!;
    private ColumnHeader colFileName = null!;
    private ColumnHeader colFileType = null!;
    private ColumnHeader colFilePath = null!;
    private ColumnHeader colFileSize = null!;
    private DarkUI.Controls.DarkContextMenu contextFiles = null!;
    private ToolStripMenuItem menuFileOpenContained = null!;
    private ToolStripMenuItem menuFileExtractContained = null!;
    private ToolStripSeparator menuFileContainerSeparator = null!;
    private ToolStripMenuItem menuFileRevealContainer = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFileViewer = null!;
    private Panel fileViewerPanel = null!;
    private DarkUI.Controls.DarkLabel lblFileViewerInfo = null!;
    private Panel fileViewerBody = null!;
    private PictureBox pictureFileViewer = null!;
    private DarkUI.Controls.DarkTextBox txtFileViewer = null!;
    private DarkUI.Controls.DarkTextBox txtHexViewer = null!;
    private System.Windows.Forms.Integration.ElementHost mediaFileHost = null!;
    private System.Windows.Controls.MediaElement mediaFileViewer = null!;
    private Panel fileViewerCommands = null!;
    private DarkUI.Controls.DarkButton btnMediaLoad = null!;
    private DarkUI.Controls.DarkButton btnMediaPlay = null!;
    private DarkUI.Controls.DarkButton btnMediaPause = null!;
    private DarkUI.Controls.DarkButton btnMediaStop = null!;
    private DarkUI.Controls.DarkButton btnHexPrevious = null!;
    private DarkUI.Controls.DarkButton btnHexNext = null!;
    private DarkUI.Controls.DarkLabel lblHexPage = null!;
    private TabPage tabExecutable = null!;
    private Panel executableHeaderPanel = null!;
    private DarkUI.Controls.DarkLabel lblExecutableSummary = null!;
    private DarkUI.Controls.DarkDataGridView gridModules = null!;
    private TabPage tabRaw = null!;
    private DarkUI.Controls.DarkTextBox txtRawMetadata = null!;
    private DarkUI.Controls.DarkTabControl tabsToolFormats = null!;
    private TabPage tabToolFfpfsc = null!;
    private TabPage tabToolFfpkg = null!;
    private TabPage tabToolExfat = null!;
    private TabPage tabToolSonyPkg = null!;
    private TableLayoutPanel layoutFfpfsc = null!;
    private Panel ffpfscSelectionPanel = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscSelectedGame = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFfpfscCreate = null!;
    private Panel ffpfscCreatePanel = null!;
    private TableLayoutPanel layoutFfpfscOutput = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscOutput = null!;
    private DarkUI.Controls.DarkTextBox txtFfpfscOutput = null!;
    private DarkUI.Controls.DarkButton btnFfpfscBrowseOutput = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscLevel = null!;
    private DarkUI.Controls.DarkNumericUpDown nudFfpfscLevel = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscGain = null!;
    private DarkUI.Controls.DarkNumericUpDown nudFfpfscGain = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscCluster = null!;
    private DarkUI.Controls.DarkComboBox cboFfpfscCluster = null!;
    private DarkUI.Controls.DarkCheckBox chkFfpfscAmpr = null!;
    private DarkUI.Controls.DarkButton btnFfpfscCreateSelected = null!;
    private DarkUI.Controls.DarkButton btnFfpfscWrapImage = null!;
    private TableLayoutPanel layoutFfpfscProgress = null!;
    private DarkUI.Controls.DarkProgressBar progressFfpfsc = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscProgress = null!;
    private DarkUI.Controls.DarkButton btnFfpfscCancel = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFfpfscOperations = null!;
    private Panel ffpfscOperationsPanel = null!;
    private DarkUI.Controls.DarkButton btnFfpfscVerify = null!;
    private DarkUI.Controls.DarkButton btnFfpfscExtract = null!;
    private DarkUI.Controls.DarkLabel lblFfpfscOperationsInfo = null!;
    private TableLayoutPanel layoutFfpkg = null!;
    private Panel ffpkgSelectionPanel = null!;
    private DarkUI.Controls.DarkLabel lblFfpkgSelectedGame = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFfpkgCreate = null!;
    private Panel ffpkgCreatePanel = null!;
    private TableLayoutPanel layoutFfpkgOutput = null!;
    private DarkUI.Controls.DarkLabel lblFfpkgOutput = null!;
    private DarkUI.Controls.DarkTextBox txtFfpkgOutput = null!;
    private DarkUI.Controls.DarkButton btnFfpkgBrowseOutput = null!;
    private DarkUI.Controls.DarkLabel lblFfpkgPreset = null!;
    private DarkUI.Controls.DarkButton btnFfpkgCreate = null!;
    private DarkUI.Controls.DarkButton btnFfpkgCancel = null!;
    private TableLayoutPanel layoutFfpkgProgress = null!;
    private DarkUI.Controls.DarkProgressBar progressFfpkg = null!;
    private DarkUI.Controls.DarkLabel lblFfpkgProgress = null!;
    private DarkUI.Controls.DarkSectionPanel sectionFfpkgOperations = null!;
    private Panel ffpkgOperationsPanel = null!;
    private DarkUI.Controls.DarkButton btnFfpkgVerify = null!;
    private DarkUI.Controls.DarkButton btnFfpkgExtract = null!;
    private DarkUI.Controls.DarkButton btnFfpkgEdit = null!;
    private DarkUI.Controls.DarkButton btnFfpkgRebuild = null!;
    private DarkUI.Controls.DarkLabel lblFfpkgOperationsInfo = null!;
    private TableLayoutPanel layoutExfat = null!;
    private Panel exfatSelectionPanel = null!;
    private DarkUI.Controls.DarkLabel lblExfatSelectedGame = null!;
    private DarkUI.Controls.DarkSectionPanel sectionExfatCreate = null!;
    private Panel exfatCreatePanel = null!;
    private TableLayoutPanel layoutExfatOutput = null!;
    private DarkUI.Controls.DarkLabel lblExfatOutput = null!;
    private DarkUI.Controls.DarkTextBox txtExfatOutput = null!;
    private DarkUI.Controls.DarkButton btnExfatBrowseOutput = null!;
    private DarkUI.Controls.DarkLabel lblExfatCluster = null!;
    private DarkUI.Controls.DarkComboBox cboExfatCluster = null!;
    private DarkUI.Controls.DarkCheckBox chkExfatAmpr = null!;
    private DarkUI.Controls.DarkButton btnExfatCreate = null!;
    private DarkUI.Controls.DarkButton btnExfatCancel = null!;
    private TableLayoutPanel layoutExfatProgress = null!;
    private DarkUI.Controls.DarkProgressBar progressExfat = null!;
    private DarkUI.Controls.DarkLabel lblExfatProgress = null!;
    private DarkUI.Controls.DarkSectionPanel sectionExfatOperations = null!;
    private Panel exfatOperationsPanel = null!;
    private DarkUI.Controls.DarkButton btnExfatVerify = null!;
    private DarkUI.Controls.DarkButton btnExfatExtract = null!;
    private DarkUI.Controls.DarkButton btnExfatRefreshAmpr = null!;
    private DarkUI.Controls.DarkButton btnExfatEdit = null!;
    private DarkUI.Controls.DarkButton btnExfatRepair = null!;
    private DarkUI.Controls.DarkLabel lblExfatOperationsInfo = null!;
    private TableLayoutPanel layoutSonyPkg = null!;
    private Panel sonyPkgSelectionPanel = null!;
    private DarkUI.Controls.DarkLabel lblSonyPkgSelectedGame = null!;
    private DarkUI.Controls.DarkSectionPanel sectionSonyPkgCreate = null!;
    private Panel sonyPkgCreatePanel = null!;
    private TableLayoutPanel layoutSonyPkgOutput = null!;
    private DarkUI.Controls.DarkLabel lblSonyPkgOutput = null!;
    private DarkUI.Controls.DarkTextBox txtSonyPkgOutput = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgBrowseOutput = null!;
    private DarkUI.Controls.DarkLabel lblSonyPkgContentId = null!;
    private DarkUI.Controls.DarkTextBox txtSonyPkgContentId = null!;
    private DarkUI.Controls.DarkCheckBox chkSonyPkgCustomPasscode = null!;
    private DarkUI.Controls.DarkTextBox txtSonyPkgPasscode = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgCreate = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgCancel = null!;
    private TableLayoutPanel layoutSonyPkgProgress = null!;
    private DarkUI.Controls.DarkProgressBar progressSonyPkg = null!;
    private DarkUI.Controls.DarkLabel lblSonyPkgProgress = null!;
    private DarkUI.Controls.DarkSectionPanel sectionSonyPkgOperations = null!;
    private Panel sonyPkgOperationsPanel = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgVerify = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgExtract = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgAcceptance = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgSplit = null!;
    private DarkUI.Controls.DarkButton btnSonyPkgMerge = null!;
    private DarkUI.Controls.DarkLabel lblSonyPkgOperationsInfo = null!;
    private DarkUI.Controls.DarkStatusStrip statusMain = null!;
    private ToolStripStatusLabel statusLabel = null!;
    private ToolStripStatusLabel statusSpring = null!;
    private ToolStripStatusLabel statusCount = null!;
    private FolderBrowserDialog folderBrowserDialog = null!;
    private OpenFileDialog packageOpenDialog = null!;
    private OpenFileDialog sourceImageOpenDialog = null!;
    private OpenFileDialog ffpfscOpenDialog = null!;
    private SaveFileDialog ffpfscSaveDialog = null!;
    private SaveFileDialog innerImageSaveDialog = null!;
    private SaveFileDialog containedFileSaveDialog = null!;
    private SaveFileDialog exfatSaveDialog = null!;
    private OpenFileDialog exfatOpenDialog = null!;
    private FolderBrowserDialog exfatExtractFolderDialog = null!;
    private SaveFileDialog ffpkgSaveDialog = null!;
    private OpenFileDialog ffpkgOpenDialog = null!;
    private FolderBrowserDialog ffpkgExtractFolderDialog = null!;
    private SaveFileDialog sonyPkgSaveDialog = null!;
    private OpenFileDialog sonyPkgOpenDialog = null!;
    private FolderBrowserDialog sonyPkgExtractFolderDialog = null!;
    private FolderBrowserDialog sonyPkgSplitFolderDialog = null!;
    private OpenFileDialog sonyPkgSplitManifestOpenDialog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuMain = new DarkUI.Controls.DarkMenuStrip();
        menuFile = new ToolStripMenuItem();
        menuAddFolder = new ToolStripMenuItem();
        menuOpenDump = new ToolStripMenuItem();
        menuOpenPackage = new ToolStripMenuItem();
        menuRefresh = new ToolStripMenuItem();
        menuSeparator = new ToolStripSeparator();
        menuSettings = new ToolStripMenuItem();
        menuExit = new ToolStripMenuItem();
        menuTools = new ToolStripMenuItem();
        menuBuildDumpFfpfsc = new ToolStripMenuItem();
        menuBuildFfpfsc = new ToolStripMenuItem();
        menuVerifyFfpfsc = new ToolStripMenuItem();
        menuHelp = new ToolStripMenuItem();
        menuAbout = new ToolStripMenuItem();
        commandPanel = new Panel();
        btnAddFolder = new DarkUI.Controls.DarkButton();
        btnRefresh = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        btnSettings = new DarkUI.Controls.DarkButton();
        lblSearch = new DarkUI.Controls.DarkLabel();
        txtSearch = new DarkUI.Controls.DarkTextBox();
        splitMain = new SplitContainer();
        gridLibrary = new DarkUI.Controls.DarkDataGridView();
        tabsWorkspace = new DarkUI.Controls.DarkTabControl();
        tabWorkspaceGeneral = new TabPage();
        tabWorkspaceTools = new TabPage();
        tabsDetails = new DarkUI.Controls.DarkTabControl();
        tabOverview = new TabPage();
        gridOverview = new DarkUI.Controls.DarkDataGridView();
        tabArtwork = new TabPage();
        splitArtwork = new SplitContainer();
        sectionIcon = new DarkUI.Controls.DarkSectionPanel();
        pictureIcon = new PictureBox();
        sectionBackground = new DarkUI.Controls.DarkSectionPanel();
        tabsBackgrounds = new DarkUI.Controls.DarkTabControl();
        tabPic0 = new TabPage();
        pictureBackground0 = new PictureBox();
        tabPic1 = new TabPage();
        pictureBackground1 = new PictureBox();
        tabPic2 = new TabPage();
        pictureBackground2 = new PictureBox();
        tabTrophies = new TabPage();
        trophyHeaderPanel = new Panel();
        lblTrophySummary = new DarkUI.Controls.DarkLabel();
        gridTrophies = new DarkUI.Controls.DarkDataGridView();
        tabActivities = new TabPage();
        activitiesHeaderPanel = new Panel();
        lblActivitiesSummary = new DarkUI.Controls.DarkLabel();
        gridActivities = new DarkUI.Controls.DarkDataGridView();
        tabFiles = new TabPage();
        filesHeaderPanel = new Panel();
        lblFilesSummary = new DarkUI.Controls.DarkLabel();
        sectionFileBrowser = new DarkUI.Controls.DarkSectionPanel();
        splitFileBrowser = new SplitContainer();
        treeFiles = new DarkUI.Controls.DarkTreeView();
        splitFileContentPreview = new SplitContainer();
        fileListPanel = new Panel();
        fileFilterPanel = new Panel();
        txtFileFilter = new DarkUI.Controls.DarkTextBox();
        btnClearFileFilter = new DarkUI.Controls.DarkButton();
        listFiles = new DarkUI.Controls.DarkListView();
        colFileName = new ColumnHeader();
        colFileType = new ColumnHeader();
        colFilePath = new ColumnHeader();
        colFileSize = new ColumnHeader();
        contextFiles = new DarkUI.Controls.DarkContextMenu();
        menuFileOpenContained = new ToolStripMenuItem();
        menuFileExtractContained = new ToolStripMenuItem();
        menuFileContainerSeparator = new ToolStripSeparator();
        menuFileRevealContainer = new ToolStripMenuItem();
        sectionFileViewer = new DarkUI.Controls.DarkSectionPanel();
        fileViewerPanel = new Panel();
        lblFileViewerInfo = new DarkUI.Controls.DarkLabel();
        fileViewerBody = new Panel();
        pictureFileViewer = new PictureBox();
        txtFileViewer = new DarkUI.Controls.DarkTextBox();
        txtHexViewer = new DarkUI.Controls.DarkTextBox();
        mediaFileHost = new System.Windows.Forms.Integration.ElementHost();
        mediaFileViewer = new System.Windows.Controls.MediaElement();
        fileViewerCommands = new Panel();
        btnMediaLoad = new DarkUI.Controls.DarkButton();
        btnMediaPlay = new DarkUI.Controls.DarkButton();
        btnMediaPause = new DarkUI.Controls.DarkButton();
        btnMediaStop = new DarkUI.Controls.DarkButton();
        btnHexPrevious = new DarkUI.Controls.DarkButton();
        btnHexNext = new DarkUI.Controls.DarkButton();
        lblHexPage = new DarkUI.Controls.DarkLabel();
        tabExecutable = new TabPage();
        executableHeaderPanel = new Panel();
        lblExecutableSummary = new DarkUI.Controls.DarkLabel();
        gridModules = new DarkUI.Controls.DarkDataGridView();
        tabRaw = new TabPage();
        txtRawMetadata = new DarkUI.Controls.DarkTextBox();
        tabsToolFormats = new DarkUI.Controls.DarkTabControl();
        tabToolFfpfsc = new TabPage();
        tabToolFfpkg = new TabPage();
        tabToolExfat = new TabPage();
        tabToolSonyPkg = new TabPage();
        layoutFfpfsc = new TableLayoutPanel();
        ffpfscSelectionPanel = new Panel();
        lblFfpfscSelectedGame = new DarkUI.Controls.DarkLabel();
        sectionFfpfscCreate = new DarkUI.Controls.DarkSectionPanel();
        ffpfscCreatePanel = new Panel();
        layoutFfpfscOutput = new TableLayoutPanel();
        lblFfpfscOutput = new DarkUI.Controls.DarkLabel();
        txtFfpfscOutput = new DarkUI.Controls.DarkTextBox();
        btnFfpfscBrowseOutput = new DarkUI.Controls.DarkButton();
        lblFfpfscLevel = new DarkUI.Controls.DarkLabel();
        nudFfpfscLevel = new DarkUI.Controls.DarkNumericUpDown();
        lblFfpfscGain = new DarkUI.Controls.DarkLabel();
        nudFfpfscGain = new DarkUI.Controls.DarkNumericUpDown();
        lblFfpfscCluster = new DarkUI.Controls.DarkLabel();
        cboFfpfscCluster = new DarkUI.Controls.DarkComboBox();
        chkFfpfscAmpr = new DarkUI.Controls.DarkCheckBox();
        btnFfpfscCreateSelected = new DarkUI.Controls.DarkButton();
        btnFfpfscWrapImage = new DarkUI.Controls.DarkButton();
        layoutFfpfscProgress = new TableLayoutPanel();
        progressFfpfsc = new DarkUI.Controls.DarkProgressBar();
        lblFfpfscProgress = new DarkUI.Controls.DarkLabel();
        btnFfpfscCancel = new DarkUI.Controls.DarkButton();
        sectionFfpfscOperations = new DarkUI.Controls.DarkSectionPanel();
        ffpfscOperationsPanel = new Panel();
        btnFfpfscVerify = new DarkUI.Controls.DarkButton();
        btnFfpfscExtract = new DarkUI.Controls.DarkButton();
        lblFfpfscOperationsInfo = new DarkUI.Controls.DarkLabel();
        layoutFfpkg = new TableLayoutPanel();
        ffpkgSelectionPanel = new Panel();
        lblFfpkgSelectedGame = new DarkUI.Controls.DarkLabel();
        sectionFfpkgCreate = new DarkUI.Controls.DarkSectionPanel();
        ffpkgCreatePanel = new Panel();
        layoutFfpkgOutput = new TableLayoutPanel();
        lblFfpkgOutput = new DarkUI.Controls.DarkLabel();
        txtFfpkgOutput = new DarkUI.Controls.DarkTextBox();
        btnFfpkgBrowseOutput = new DarkUI.Controls.DarkButton();
        lblFfpkgPreset = new DarkUI.Controls.DarkLabel();
        btnFfpkgCreate = new DarkUI.Controls.DarkButton();
        btnFfpkgCancel = new DarkUI.Controls.DarkButton();
        layoutFfpkgProgress = new TableLayoutPanel();
        progressFfpkg = new DarkUI.Controls.DarkProgressBar();
        lblFfpkgProgress = new DarkUI.Controls.DarkLabel();
        sectionFfpkgOperations = new DarkUI.Controls.DarkSectionPanel();
        ffpkgOperationsPanel = new Panel();
        btnFfpkgVerify = new DarkUI.Controls.DarkButton();
        btnFfpkgExtract = new DarkUI.Controls.DarkButton();
        btnFfpkgEdit = new DarkUI.Controls.DarkButton();
        btnFfpkgRebuild = new DarkUI.Controls.DarkButton();
        lblFfpkgOperationsInfo = new DarkUI.Controls.DarkLabel();
        layoutExfat = new TableLayoutPanel();
        exfatSelectionPanel = new Panel();
        lblExfatSelectedGame = new DarkUI.Controls.DarkLabel();
        sectionExfatCreate = new DarkUI.Controls.DarkSectionPanel();
        exfatCreatePanel = new Panel();
        layoutExfatOutput = new TableLayoutPanel();
        lblExfatOutput = new DarkUI.Controls.DarkLabel();
        txtExfatOutput = new DarkUI.Controls.DarkTextBox();
        btnExfatBrowseOutput = new DarkUI.Controls.DarkButton();
        lblExfatCluster = new DarkUI.Controls.DarkLabel();
        cboExfatCluster = new DarkUI.Controls.DarkComboBox();
        chkExfatAmpr = new DarkUI.Controls.DarkCheckBox();
        btnExfatCreate = new DarkUI.Controls.DarkButton();
        btnExfatCancel = new DarkUI.Controls.DarkButton();
        layoutExfatProgress = new TableLayoutPanel();
        progressExfat = new DarkUI.Controls.DarkProgressBar();
        lblExfatProgress = new DarkUI.Controls.DarkLabel();
        sectionExfatOperations = new DarkUI.Controls.DarkSectionPanel();
        exfatOperationsPanel = new Panel();
        btnExfatVerify = new DarkUI.Controls.DarkButton();
        btnExfatExtract = new DarkUI.Controls.DarkButton();
        btnExfatRefreshAmpr = new DarkUI.Controls.DarkButton();
        btnExfatEdit = new DarkUI.Controls.DarkButton();
        btnExfatRepair = new DarkUI.Controls.DarkButton();
        lblExfatOperationsInfo = new DarkUI.Controls.DarkLabel();
        layoutSonyPkg = new TableLayoutPanel();
        sonyPkgSelectionPanel = new Panel();
        lblSonyPkgSelectedGame = new DarkUI.Controls.DarkLabel();
        sectionSonyPkgCreate = new DarkUI.Controls.DarkSectionPanel();
        sonyPkgCreatePanel = new Panel();
        layoutSonyPkgOutput = new TableLayoutPanel();
        lblSonyPkgOutput = new DarkUI.Controls.DarkLabel();
        txtSonyPkgOutput = new DarkUI.Controls.DarkTextBox();
        btnSonyPkgBrowseOutput = new DarkUI.Controls.DarkButton();
        lblSonyPkgContentId = new DarkUI.Controls.DarkLabel();
        txtSonyPkgContentId = new DarkUI.Controls.DarkTextBox();
        chkSonyPkgCustomPasscode = new DarkUI.Controls.DarkCheckBox();
        txtSonyPkgPasscode = new DarkUI.Controls.DarkTextBox();
        btnSonyPkgCreate = new DarkUI.Controls.DarkButton();
        btnSonyPkgCancel = new DarkUI.Controls.DarkButton();
        layoutSonyPkgProgress = new TableLayoutPanel();
        progressSonyPkg = new DarkUI.Controls.DarkProgressBar();
        lblSonyPkgProgress = new DarkUI.Controls.DarkLabel();
        sectionSonyPkgOperations = new DarkUI.Controls.DarkSectionPanel();
        sonyPkgOperationsPanel = new Panel();
        btnSonyPkgVerify = new DarkUI.Controls.DarkButton();
        btnSonyPkgExtract = new DarkUI.Controls.DarkButton();
        btnSonyPkgAcceptance = new DarkUI.Controls.DarkButton();
        btnSonyPkgSplit = new DarkUI.Controls.DarkButton();
        btnSonyPkgMerge = new DarkUI.Controls.DarkButton();
        lblSonyPkgOperationsInfo = new DarkUI.Controls.DarkLabel();
        statusMain = new DarkUI.Controls.DarkStatusStrip();
        statusLabel = new ToolStripStatusLabel();
        statusSpring = new ToolStripStatusLabel();
        statusCount = new ToolStripStatusLabel();
        folderBrowserDialog = new FolderBrowserDialog();
        packageOpenDialog = new OpenFileDialog();
        sourceImageOpenDialog = new OpenFileDialog();
        ffpfscOpenDialog = new OpenFileDialog();
        ffpfscSaveDialog = new SaveFileDialog();
        innerImageSaveDialog = new SaveFileDialog();
        containedFileSaveDialog = new SaveFileDialog();
        exfatSaveDialog = new SaveFileDialog();
        exfatOpenDialog = new OpenFileDialog();
        exfatExtractFolderDialog = new FolderBrowserDialog();
        ffpkgSaveDialog = new SaveFileDialog();
        ffpkgOpenDialog = new OpenFileDialog();
        ffpkgExtractFolderDialog = new FolderBrowserDialog();
        sonyPkgSaveDialog = new SaveFileDialog();
        sonyPkgOpenDialog = new OpenFileDialog();
        sonyPkgExtractFolderDialog = new FolderBrowserDialog();
        sonyPkgSplitFolderDialog = new FolderBrowserDialog();
        sonyPkgSplitManifestOpenDialog = new OpenFileDialog();
        menuMain.SuspendLayout();
        commandPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridLibrary).BeginInit();
        tabsWorkspace.SuspendLayout();
        tabWorkspaceGeneral.SuspendLayout();
        tabWorkspaceTools.SuspendLayout();
        tabsDetails.SuspendLayout();
        tabOverview.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridOverview).BeginInit();
        tabArtwork.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitArtwork).BeginInit();
        splitArtwork.Panel1.SuspendLayout();
        splitArtwork.Panel2.SuspendLayout();
        splitArtwork.SuspendLayout();
        sectionIcon.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureIcon).BeginInit();
        sectionBackground.SuspendLayout();
        tabsBackgrounds.SuspendLayout();
        tabPic0.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground0).BeginInit();
        tabPic1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground1).BeginInit();
        tabPic2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBackground2).BeginInit();
        tabTrophies.SuspendLayout();
        trophyHeaderPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridTrophies).BeginInit();
        tabActivities.SuspendLayout();
        activitiesHeaderPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridActivities).BeginInit();
        tabFiles.SuspendLayout();
        filesHeaderPanel.SuspendLayout();
        sectionFileBrowser.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitFileBrowser).BeginInit();
        splitFileBrowser.Panel1.SuspendLayout();
        splitFileBrowser.Panel2.SuspendLayout();
        splitFileBrowser.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitFileContentPreview).BeginInit();
        splitFileContentPreview.Panel1.SuspendLayout();
        splitFileContentPreview.Panel2.SuspendLayout();
        splitFileContentPreview.SuspendLayout();
        fileListPanel.SuspendLayout();
        fileFilterPanel.SuspendLayout();
        sectionFileViewer.SuspendLayout();
        fileViewerPanel.SuspendLayout();
        fileViewerBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureFileViewer).BeginInit();
        fileViewerCommands.SuspendLayout();
        tabExecutable.SuspendLayout();
        executableHeaderPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridModules).BeginInit();
        tabRaw.SuspendLayout();
        tabsToolFormats.SuspendLayout();
        tabToolFfpfsc.SuspendLayout();
        tabToolFfpkg.SuspendLayout();
        tabToolExfat.SuspendLayout();
        tabToolSonyPkg.SuspendLayout();
        layoutFfpfsc.SuspendLayout();
        ffpfscSelectionPanel.SuspendLayout();
        sectionFfpfscCreate.SuspendLayout();
        ffpfscCreatePanel.SuspendLayout();
        layoutFfpfscOutput.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudFfpfscLevel).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudFfpfscGain).BeginInit();
        layoutFfpfscProgress.SuspendLayout();
        sectionFfpfscOperations.SuspendLayout();
        ffpfscOperationsPanel.SuspendLayout();
        layoutFfpkg.SuspendLayout();
        ffpkgSelectionPanel.SuspendLayout();
        sectionFfpkgCreate.SuspendLayout();
        ffpkgCreatePanel.SuspendLayout();
        layoutFfpkgOutput.SuspendLayout();
        layoutFfpkgProgress.SuspendLayout();
        sectionFfpkgOperations.SuspendLayout();
        ffpkgOperationsPanel.SuspendLayout();
        layoutSonyPkg.SuspendLayout();
        sonyPkgSelectionPanel.SuspendLayout();
        sectionSonyPkgCreate.SuspendLayout();
        sonyPkgCreatePanel.SuspendLayout();
        layoutSonyPkgOutput.SuspendLayout();
        layoutSonyPkgProgress.SuspendLayout();
        sectionSonyPkgOperations.SuspendLayout();
        sonyPkgOperationsPanel.SuspendLayout();
        layoutExfat.SuspendLayout();
        exfatSelectionPanel.SuspendLayout();
        sectionExfatCreate.SuspendLayout();
        exfatCreatePanel.SuspendLayout();
        layoutExfatOutput.SuspendLayout();
        layoutExfatProgress.SuspendLayout();
        sectionExfatOperations.SuspendLayout();
        exfatOperationsPanel.SuspendLayout();
        statusMain.SuspendLayout();
        SuspendLayout();
        // 
        // menuMain
        // 
        menuMain.BackColor = Color.FromArgb(60, 63, 65);
        menuMain.ForeColor = Color.FromArgb(220, 220, 220);
        menuMain.Items.AddRange(new ToolStripItem[] { menuFile, menuTools, menuHelp });
        menuMain.Location = new Point(0, 0);
        menuMain.Name = "menuMain";
        menuMain.Size = new Size(1384, 24);
        menuMain.TabIndex = 0;
        // 
        // menuFile
        // 
        menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuAddFolder, menuOpenDump, menuOpenPackage, menuRefresh, menuSeparator, menuSettings, menuExit });
        menuFile.Name = "menuFile";
        menuFile.Text = "File";
        // 
        // menuAddFolder
        // 
        menuAddFolder.Name = "menuAddFolder";
        menuAddFolder.Text = "Add Library Folder...";
        menuAddFolder.Click += AddFolder_Click;
        // 
        // menuOpenDump
        // 
        menuOpenDump.Name = "menuOpenDump";
        menuOpenDump.Text = "Open Dump Folder...";
        menuOpenDump.Click += OpenDump_Click;
        // 
        // menuOpenPackage
        // 
        menuOpenPackage.Name = "menuOpenPackage";
        menuOpenPackage.Text = "Open PS5 Container / Image...";
        menuOpenPackage.Click += OpenPackage_Click;
        // 
        // menuRefresh
        // 
        menuRefresh.Name = "menuRefresh";
        menuRefresh.Text = "Refresh Library";
        menuRefresh.Click += Refresh_Click;
        // 
        // menuSettings
        // 
        menuSettings.Name = "menuSettings";
        menuSettings.Text = "Settings...";
        menuSettings.Click += Settings_Click;
        // 
        // menuExit
        // 
        menuExit.Name = "menuExit";
        menuExit.Text = "Exit";
        menuExit.Click += Exit_Click;
        // 
        // menuTools
        // 
        menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuBuildDumpFfpfsc, menuBuildFfpfsc, menuVerifyFfpfsc });
        menuTools.Name = "menuTools";
        menuTools.Text = "Tools";
        // 
        // menuBuildDumpFfpfsc
        // 
        menuBuildDumpFfpfsc.Name = "menuBuildDumpFfpfsc";
        menuBuildDumpFfpfsc.Text = "Convert Dump Folder to FFPFSC...";
        menuBuildDumpFfpfsc.Click += BuildDumpFfpfsc_Click;
        // 
        // menuBuildFfpfsc
        // 
        menuBuildFfpfsc.Name = "menuBuildFfpfsc";
        menuBuildFfpfsc.Text = "Convert exFAT / FFPKG to FFPFSC...";
        menuBuildFfpfsc.Click += BuildFfpfsc_Click;
        // 
        // menuVerifyFfpfsc
        // 
        menuVerifyFfpfsc.Name = "menuVerifyFfpfsc";
        menuVerifyFfpfsc.Text = "Inspect and Verify FFPFSC...";
        menuVerifyFfpfsc.Click += VerifyFfpfsc_Click;
        // 
        // menuHelp
        // 
        menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout });
        menuHelp.Name = "menuHelp";
        menuHelp.Text = "Help";
        // 
        // menuAbout
        // 
        menuAbout.Name = "menuAbout";
        menuAbout.Text = "About";
        menuAbout.Click += About_Click;
        // 
        // packageOpenDialog
        // 
        packageOpenDialog.CheckFileExists = true;
        packageOpenDialog.DefaultExt = "ffpfsc";
        packageOpenDialog.Filter = "Supported PS5 containers (*.pkg;*.ffpfsc;*.ffpkg;*.exfat)|*.pkg;*.ffpfsc;*.ffpkg;*.exfat|FFPFSC images (*.ffpfsc)|*.ffpfsc|FFPKG images (*.ffpkg)|*.ffpkg|exFAT images (*.exfat)|*.exfat|Sony PS5 packages (*.pkg)|*.pkg|All files (*.*)|*.*";
        packageOpenDialog.Multiselect = false;
        packageOpenDialog.Title = "Open a PS5 package or filesystem image";
        // 
        // sourceImageOpenDialog
        // 
        sourceImageOpenDialog.CheckFileExists = true;
        sourceImageOpenDialog.Filter = "PS5 filesystem images (*.exfat;*.ffpkg)|*.exfat;*.ffpkg|All files (*.*)|*.*";
        sourceImageOpenDialog.Multiselect = false;
        sourceImageOpenDialog.Title = "Select a PS5 filesystem image";
        // 
        // ffpfscOpenDialog
        // 
        ffpfscOpenDialog.CheckFileExists = true;
        ffpfscOpenDialog.DefaultExt = "ffpfsc";
        ffpfscOpenDialog.Filter = "FFPFSC images (*.ffpfsc)|*.ffpfsc|All files (*.*)|*.*";
        ffpfscOpenDialog.Multiselect = false;
        ffpfscOpenDialog.Title = "Inspect and verify FFPFSC";
        // 
        // ffpfscSaveDialog
        // 
        ffpfscSaveDialog.AddExtension = true;
        ffpfscSaveDialog.DefaultExt = "ffpfsc";
        ffpfscSaveDialog.Filter = "FFPFSC images (*.ffpfsc)|*.ffpfsc|All files (*.*)|*.*";
        ffpfscSaveDialog.OverwritePrompt = true;
        ffpfscSaveDialog.Title = "Save native FFPFSC image";
        // 
        // innerImageSaveDialog
        // 
        innerImageSaveDialog.AddExtension = true;
        innerImageSaveDialog.DefaultExt = "exfat";
        innerImageSaveDialog.Filter = "PS5 filesystem images (*.exfat;*.ffpkg)|*.exfat;*.ffpkg|All files (*.*)|*.*";
        innerImageSaveDialog.OverwritePrompt = true;
        innerImageSaveDialog.Title = "Extract the inner filesystem image";
        // 
        // containedFileSaveDialog
        // 
        containedFileSaveDialog.Filter = "All files (*.*)|*.*";
        containedFileSaveDialog.OverwritePrompt = true;
        containedFileSaveDialog.Title = "Extract file from FFPFSC";
        // 
        // exfatSaveDialog
        // 
        exfatSaveDialog.AddExtension = true;
        exfatSaveDialog.DefaultExt = "exfat";
        exfatSaveDialog.Filter = "exFAT images (*.exfat)|*.exfat|All files (*.*)|*.*";
        exfatSaveDialog.OverwritePrompt = false;
        exfatSaveDialog.Title = "Save standalone exFAT image";
        // 
        // exfatOpenDialog
        // 
        exfatOpenDialog.CheckFileExists = true;
        exfatOpenDialog.DefaultExt = "exfat";
        exfatOpenDialog.Filter = "exFAT images (*.exfat)|*.exfat|All files (*.*)|*.*";
        exfatOpenDialog.Multiselect = false;
        exfatOpenDialog.Title = "Inspect or extract an exFAT image";
        // 
        // exfatExtractFolderDialog
        // 
        exfatExtractFolderDialog.Description = "Select the parent folder for the extracted game directory";
        // 
        // ffpkgSaveDialog
        // 
        ffpkgSaveDialog.AddExtension = true;
        ffpkgSaveDialog.DefaultExt = "ffpkg";
        ffpkgSaveDialog.Filter = "FFPKG UFS2 images (*.ffpkg)|*.ffpkg|All files (*.*)|*.*";
        ffpkgSaveDialog.OverwritePrompt = false;
        ffpkgSaveDialog.Title = "Save standalone FFPKG image";
        // 
        // sonyPkgSaveDialog
        // 
        sonyPkgSaveDialog.AddExtension = true;
        sonyPkgSaveDialog.DefaultExt = "pkg";
        sonyPkgSaveDialog.Filter = "PS5 debug packages (*.pkg)|*.pkg|All files (*.*)|*.*";
        sonyPkgSaveDialog.OverwritePrompt = false;
        sonyPkgSaveDialog.Title = "Save PS5 debug package";
        // 
        // sonyPkgOpenDialog
        // 
        sonyPkgOpenDialog.Filter = "PS5 packages (*.pkg)|*.pkg|All files (*.*)|*.*";
        sonyPkgOpenDialog.Title = "Select a PS5 debug package";
        // 
        // sonyPkgSplitFolderDialog
        // 
        sonyPkgSplitFolderDialog.Description = "Select an empty folder for the verified split package set";
        // 
        // sonyPkgSplitManifestOpenDialog
        // 
        sonyPkgSplitManifestOpenDialog.Filter = "PS5 split manifests (*.ps5split.json)|*.ps5split.json|JSON files (*.json)|*.json|All files (*.*)|*.*";
        sonyPkgSplitManifestOpenDialog.Title = "Select a verified PS5 package split manifest";
        // 
        // ffpkgOpenDialog
        // 
        ffpkgOpenDialog.CheckFileExists = true;
        ffpkgOpenDialog.DefaultExt = "ffpkg";
        ffpkgOpenDialog.Filter = "FFPKG UFS2 images (*.ffpkg)|*.ffpkg|All files (*.*)|*.*";
        ffpkgOpenDialog.Multiselect = false;
        ffpkgOpenDialog.Title = "Inspect or extract an FFPKG image";
        // 
        // ffpkgExtractFolderDialog
        // 
        ffpkgExtractFolderDialog.Description = "Select the parent folder for the extracted FFPKG directory";
        // 
        // commandPanel
        // 
        commandPanel.BackColor = Color.FromArgb(45, 45, 48);
        commandPanel.Controls.Add(btnAddFolder);
        commandPanel.Controls.Add(btnRefresh);
        commandPanel.Controls.Add(btnCancel);
        commandPanel.Controls.Add(btnSettings);
        commandPanel.Controls.Add(lblSearch);
        commandPanel.Controls.Add(txtSearch);
        commandPanel.Dock = DockStyle.Top;
        commandPanel.Location = new Point(0, 24);
        commandPanel.Name = "commandPanel";
        commandPanel.Size = new Size(1384, 46);
        commandPanel.TabIndex = 1;
        // 
        // btnAddFolder
        // 
        btnAddFolder.Location = new Point(10, 8);
        btnAddFolder.Name = "btnAddFolder";
        btnAddFolder.Size = new Size(118, 30);
        btnAddFolder.TabIndex = 0;
        btnAddFolder.Text = "Add Folder";
        btnAddFolder.Click += AddFolder_Click;
        // 
        // btnRefresh
        // 
        btnRefresh.Location = new Point(134, 8);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(100, 30);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "Refresh";
        btnRefresh.Click += Refresh_Click;
        // 
        // btnCancel
        // 
        btnCancel.Enabled = false;
        btnCancel.Location = new Point(240, 8);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(90, 30);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        btnCancel.Click += Cancel_Click;
        // 
        // btnSettings
        // 
        btnSettings.Location = new Point(336, 8);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(100, 30);
        btnSettings.TabIndex = 3;
        btnSettings.Text = "Settings";
        btnSettings.Click += Settings_Click;
        // 
        // lblSearch
        // 
        lblSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblSearch.AutoSize = true;
        lblSearch.ForeColor = Color.FromArgb(220, 220, 220);
        lblSearch.Location = new Point(1010, 16);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(45, 15);
        lblSearch.TabIndex = 4;
        lblSearch.Text = "Search:";
        // 
        // txtSearch
        // 
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtSearch.Location = new Point(1061, 12);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "Title, PPSA or Content ID";
        txtSearch.Size = new Size(311, 23);
        txtSearch.TabIndex = 5;
        txtSearch.TextChanged += txtSearch_TextChanged;
        // 
        // splitMain
        // 
        splitMain.BackColor = Color.FromArgb(45, 45, 48);
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 70);
        splitMain.Name = "splitMain";
        splitMain.Orientation = Orientation.Horizontal;
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(gridLibrary);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(tabsWorkspace);
        splitMain.Size = new Size(1384, 789);
        splitMain.SplitterDistance = 330;
        splitMain.TabIndex = 2;
        // 
        // gridLibrary
        // 
        gridLibrary.AllowUserToAddRows = false;
        gridLibrary.AllowUserToDeleteRows = false;
        gridLibrary.AllowUserToOrderColumns = true;
        gridLibrary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLibrary.Dock = DockStyle.Fill;
        gridLibrary.MultiSelect = false;
        gridLibrary.Name = "gridLibrary";
        gridLibrary.ReadOnly = true;
        gridLibrary.RowTemplate.Height = 25;
        gridLibrary.SelectionChanged += gridLibrary_SelectionChanged;
        gridLibrary.CellDoubleClick += gridLibrary_CellDoubleClick;
        gridLibrary.CellFormatting += gridLibrary_CellFormatting;
        // 
        // tabsWorkspace
        // 
        tabsWorkspace.Controls.Add(tabWorkspaceGeneral);
        tabsWorkspace.Controls.Add(tabWorkspaceTools);
        tabsWorkspace.Dock = DockStyle.Fill;
        tabsWorkspace.Name = "tabsWorkspace";
        tabsWorkspace.SelectedIndex = 0;
        // 
        // tabWorkspaceGeneral
        // 
        tabWorkspaceGeneral.BackColor = Color.FromArgb(60, 63, 65);
        tabWorkspaceGeneral.Controls.Add(tabsDetails);
        tabWorkspaceGeneral.Text = "General";
        // 
        // tabWorkspaceTools
        // 
        tabWorkspaceTools.BackColor = Color.FromArgb(60, 63, 65);
        tabWorkspaceTools.Controls.Add(tabsToolFormats);
        tabWorkspaceTools.Text = "Tools";
        // 
        // tabsDetails
        // 
        tabsDetails.Controls.Add(tabOverview);
        tabsDetails.Controls.Add(tabArtwork);
        tabsDetails.Controls.Add(tabTrophies);
        tabsDetails.Controls.Add(tabActivities);
        tabsDetails.Controls.Add(tabFiles);
        tabsDetails.Controls.Add(tabExecutable);
        tabsDetails.Controls.Add(tabRaw);
        tabsDetails.Dock = DockStyle.Fill;
        tabsDetails.Name = "tabsDetails";
        tabsDetails.SelectedIndex = 0;
        // 
        // tabOverview
        // 
        tabOverview.BackColor = Color.FromArgb(60, 63, 65);
        tabOverview.Controls.Add(gridOverview);
        tabOverview.Text = "Overview";
        // 
        // gridOverview
        // 
        gridOverview.AllowUserToAddRows = false;
        gridOverview.AllowUserToDeleteRows = false;
        gridOverview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridOverview.Dock = DockStyle.Fill;
        gridOverview.MultiSelect = false;
        gridOverview.Name = "gridOverview";
        gridOverview.ReadOnly = true;
        // 
        // tabArtwork
        // 
        tabArtwork.BackColor = Color.FromArgb(60, 63, 65);
        tabArtwork.Controls.Add(splitArtwork);
        tabArtwork.Text = "Artwork";
        // 
        // splitArtwork
        // 
        splitArtwork.Dock = DockStyle.Fill;
        splitArtwork.Location = new Point(3, 3);
        splitArtwork.Name = "splitArtwork";
        splitArtwork.Panel1.Controls.Add(sectionIcon);
        splitArtwork.Panel2.Controls.Add(sectionBackground);
        splitArtwork.Size = new Size(1370, 420);
        splitArtwork.SplitterDistance = 360;
        // 
        // sectionIcon
        // 
        sectionIcon.Controls.Add(pictureIcon);
        sectionIcon.Dock = DockStyle.Fill;
        sectionIcon.SectionHeader = "Icon (512 × 512)";
        // 
        // pictureIcon
        // 
        pictureIcon.BackColor = Color.FromArgb(45, 45, 48);
        pictureIcon.Dock = DockStyle.Fill;
        pictureIcon.Location = new Point(1, 25);
        pictureIcon.Name = "pictureIcon";
        pictureIcon.SizeMode = PictureBoxSizeMode.Zoom;
        // 
        // sectionBackground
        // 
        sectionBackground.Controls.Add(tabsBackgrounds);
        sectionBackground.Dock = DockStyle.Fill;
        sectionBackground.SectionHeader = "Background Art (PIC0 / PIC1 / PIC2)";
        // 
        // tabsBackgrounds
        // 
        tabsBackgrounds.Controls.Add(tabPic0);
        tabsBackgrounds.Controls.Add(tabPic1);
        tabsBackgrounds.Controls.Add(tabPic2);
        tabsBackgrounds.Dock = DockStyle.Fill;
        tabsBackgrounds.Name = "tabsBackgrounds";
        tabsBackgrounds.SelectedIndex = 0;
        // 
        // tabPic0
        // 
        tabPic0.BackColor = Color.FromArgb(45, 45, 48);
        tabPic0.Controls.Add(pictureBackground0);
        tabPic0.Text = "PIC0";
        // 
        // pictureBackground0
        // 
        pictureBackground0.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground0.Dock = DockStyle.Fill;
        pictureBackground0.Name = "pictureBackground0";
        pictureBackground0.SizeMode = PictureBoxSizeMode.Zoom;
        // 
        // tabPic1
        // 
        tabPic1.BackColor = Color.FromArgb(45, 45, 48);
        tabPic1.Controls.Add(pictureBackground1);
        tabPic1.Text = "PIC1";
        // 
        // pictureBackground1
        // 
        pictureBackground1.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground1.Dock = DockStyle.Fill;
        pictureBackground1.Name = "pictureBackground1";
        pictureBackground1.SizeMode = PictureBoxSizeMode.Zoom;
        // 
        // tabPic2
        // 
        tabPic2.BackColor = Color.FromArgb(45, 45, 48);
        tabPic2.Controls.Add(pictureBackground2);
        tabPic2.Text = "PIC2";
        // 
        // pictureBackground2
        // 
        pictureBackground2.BackColor = Color.FromArgb(45, 45, 48);
        pictureBackground2.Dock = DockStyle.Fill;
        pictureBackground2.Name = "pictureBackground2";
        pictureBackground2.SizeMode = PictureBoxSizeMode.Zoom;
        // 
        // tabTrophies
        // 
        tabTrophies.BackColor = Color.FromArgb(60, 63, 65);
        tabTrophies.Controls.Add(gridTrophies);
        tabTrophies.Controls.Add(trophyHeaderPanel);
        tabTrophies.Text = "Trophies";
        // 
        // trophyHeaderPanel
        // 
        trophyHeaderPanel.BackColor = Color.FromArgb(45, 45, 48);
        trophyHeaderPanel.Controls.Add(lblTrophySummary);
        trophyHeaderPanel.Dock = DockStyle.Top;
        trophyHeaderPanel.Height = 36;
        // 
        // lblTrophySummary
        // 
        lblTrophySummary.AutoEllipsis = true;
        lblTrophySummary.Dock = DockStyle.Fill;
        lblTrophySummary.ForeColor = Color.FromArgb(220, 220, 220);
        lblTrophySummary.Padding = new Padding(10, 0, 10, 0);
        lblTrophySummary.Text = "Select a game to load trophies.";
        lblTrophySummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // gridTrophies
        // 
        gridTrophies.AllowUserToAddRows = false;
        gridTrophies.AllowUserToDeleteRows = false;
        gridTrophies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridTrophies.Dock = DockStyle.Fill;
        gridTrophies.Name = "gridTrophies";
        gridTrophies.ReadOnly = true;
        gridTrophies.RowTemplate.Height = 48;
        // 
        // tabActivities
        // 
        tabActivities.BackColor = Color.FromArgb(60, 63, 65);
        tabActivities.Controls.Add(gridActivities);
        tabActivities.Controls.Add(activitiesHeaderPanel);
        tabActivities.Text = "Activities & UDS";
        // 
        // activitiesHeaderPanel
        // 
        activitiesHeaderPanel.BackColor = Color.FromArgb(45, 45, 48);
        activitiesHeaderPanel.Controls.Add(lblActivitiesSummary);
        activitiesHeaderPanel.Dock = DockStyle.Top;
        activitiesHeaderPanel.Height = 36;
        // 
        // lblActivitiesSummary
        // 
        lblActivitiesSummary.Dock = DockStyle.Fill;
        lblActivitiesSummary.ForeColor = Color.FromArgb(220, 220, 220);
        lblActivitiesSummary.Padding = new Padding(10, 0, 10, 0);
        lblActivitiesSummary.Text = "Select a game to load activity definitions.";
        lblActivitiesSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // gridActivities
        // 
        gridActivities.AllowUserToAddRows = false;
        gridActivities.AllowUserToDeleteRows = false;
        gridActivities.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridActivities.Dock = DockStyle.Fill;
        gridActivities.Name = "gridActivities";
        gridActivities.ReadOnly = true;
        // 
        // tabFiles
        // 
        tabFiles.BackColor = Color.FromArgb(60, 63, 65);
        tabFiles.Controls.Add(sectionFileBrowser);
        tabFiles.Controls.Add(filesHeaderPanel);
        tabFiles.Text = "Files";
        // 
        // filesHeaderPanel
        // 
        filesHeaderPanel.BackColor = Color.FromArgb(45, 45, 48);
        filesHeaderPanel.Controls.Add(lblFilesSummary);
        filesHeaderPanel.Dock = DockStyle.Top;
        filesHeaderPanel.Height = 36;
        // 
        // lblFilesSummary
        // 
        lblFilesSummary.Dock = DockStyle.Fill;
        lblFilesSummary.ForeColor = Color.FromArgb(220, 220, 220);
        lblFilesSummary.Padding = new Padding(10, 0, 10, 0);
        lblFilesSummary.Text = "Select a game to inventory its files.";
        lblFilesSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionFileBrowser
        // 
        sectionFileBrowser.Controls.Add(splitFileBrowser);
        sectionFileBrowser.Dock = DockStyle.Fill;
        sectionFileBrowser.Name = "sectionFileBrowser";
        sectionFileBrowser.SectionHeader = "Dump Files";
        // 
        // splitFileBrowser
        // 
        splitFileBrowser.BorderStyle = BorderStyle.FixedSingle;
        splitFileBrowser.Dock = DockStyle.Fill;
        splitFileBrowser.Name = "splitFileBrowser";
        splitFileBrowser.Panel1.Controls.Add(treeFiles);
        splitFileBrowser.Panel2.Controls.Add(splitFileContentPreview);
        splitFileBrowser.SplitterDistance = 300;
        splitFileBrowser.TabIndex = 0;
        // 
        // treeFiles
        // 
        treeFiles.CheckBoxes = false;
        treeFiles.Dock = DockStyle.Fill;
        treeFiles.FullRowSelect = false;
        treeFiles.HotTracking = false;
        treeFiles.ImageList = null;
        treeFiles.Indent = 19;
        treeFiles.ItemHeight = 24;
        treeFiles.LabelEdit = false;
        treeFiles.Name = "treeFiles";
        treeFiles.PathSeparator = "\\";
        treeFiles.Scrollable = true;
        treeFiles.SelectedNode = null;
        treeFiles.ShowLines = true;
        treeFiles.ShowPlusMinus = true;
        treeFiles.ShowRootLines = true;
        treeFiles.Sorted = false;
        treeFiles.TopNode = null;
        treeFiles.TreeViewNodeSorter = null;
        treeFiles.UseCompatibleStateImageBehavior = false;
        treeFiles.AfterSelect += treeFiles_AfterSelect;
        // 
        // splitFileContentPreview
        // 
        splitFileContentPreview.BorderStyle = BorderStyle.FixedSingle;
        splitFileContentPreview.Dock = DockStyle.Fill;
        splitFileContentPreview.Name = "splitFileContentPreview";
        splitFileContentPreview.Panel1.Controls.Add(fileListPanel);
        splitFileContentPreview.Panel2.Controls.Add(sectionFileViewer);
        splitFileContentPreview.SplitterDistance = 500;
        splitFileContentPreview.TabIndex = 1;
        // 
        // fileListPanel
        // 
        fileListPanel.BackColor = Color.FromArgb(45, 45, 48);
        fileListPanel.Controls.Add(listFiles);
        fileListPanel.Controls.Add(fileFilterPanel);
        fileListPanel.Dock = DockStyle.Fill;
        fileListPanel.Name = "fileListPanel";
        // 
        // fileFilterPanel
        // 
        fileFilterPanel.BackColor = Color.FromArgb(45, 45, 48);
        fileFilterPanel.Controls.Add(txtFileFilter);
        fileFilterPanel.Controls.Add(btnClearFileFilter);
        fileFilterPanel.Dock = DockStyle.Top;
        fileFilterPanel.Height = 32;
        fileFilterPanel.Name = "fileFilterPanel";
        fileFilterPanel.Padding = new Padding(4);
        // 
        // txtFileFilter
        // 
        txtFileFilter.Dock = DockStyle.Fill;
        txtFileFilter.Name = "txtFileFilter";
        txtFileFilter.PlaceholderText = "Filter this directory";
        txtFileFilter.TabIndex = 0;
        txtFileFilter.TextChanged += txtFileFilter_TextChanged;
        // 
        // btnClearFileFilter
        // 
        btnClearFileFilter.Dock = DockStyle.Right;
        btnClearFileFilter.Name = "btnClearFileFilter";
        btnClearFileFilter.Size = new Size(38, 23);
        btnClearFileFilter.TabIndex = 1;
        btnClearFileFilter.Text = "X";
        btnClearFileFilter.Click += btnClearFileFilter_Click;
        // 
        // listFiles
        // 
        listFiles.Columns.AddRange(new ColumnHeader[] { colFileName, colFileType, colFilePath, colFileSize });
        listFiles.ContextMenuStrip = contextFiles;
        listFiles.Dock = DockStyle.Fill;
        listFiles.FullRowSelect = true;
        listFiles.LargeImageList = null;
        listFiles.ListViewItemSorter = null;
        listFiles.MultiSelect = true;
        listFiles.Name = "listFiles";
        listFiles.SmallImageList = null;
        listFiles.TabIndex = 1;
        listFiles.UseCompatibleStateImageBehavior = false;
        listFiles.View = View.Details;
        listFiles.ItemActivate += listFiles_ItemActivate;
        listFiles.MouseDown += listFiles_MouseDown;
        listFiles.SelectedIndexChanged += listFiles_SelectedIndexChanged;
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
        colFileSize.Width = 100;
        // 
        // contextFiles
        // 
        contextFiles.Items.AddRange(new ToolStripItem[] { menuFileOpenContained, menuFileExtractContained, menuFileContainerSeparator, menuFileRevealContainer });
        contextFiles.Opening += contextFiles_Opening;
        // 
        // menuFileOpenContained
        // 
        menuFileOpenContained.Name = "menuFileOpenContained";
        menuFileOpenContained.Text = "Open / Preview";
        menuFileOpenContained.Click += menuFileOpenContained_Click;
        // 
        // menuFileExtractContained
        // 
        menuFileExtractContained.Name = "menuFileExtractContained";
        menuFileExtractContained.Text = "Extract Selected File...";
        menuFileExtractContained.Click += menuFileExtractContained_Click;
        // 
        // menuFileRevealContainer
        // 
        menuFileRevealContainer.Name = "menuFileRevealContainer";
        menuFileRevealContainer.Text = "Reveal Container in Explorer";
        menuFileRevealContainer.Click += menuFileRevealContainer_Click;
        // 
        // sectionFileViewer
        // 
        sectionFileViewer.Controls.Add(fileViewerPanel);
        sectionFileViewer.Dock = DockStyle.Fill;
        sectionFileViewer.Name = "sectionFileViewer";
        sectionFileViewer.SectionHeader = "File Viewer";
        sectionFileViewer.TabIndex = 0;
        // 
        // fileViewerPanel
        // 
        fileViewerPanel.BackColor = Color.FromArgb(37, 37, 38);
        fileViewerPanel.Controls.Add(fileViewerBody);
        fileViewerPanel.Controls.Add(fileViewerCommands);
        fileViewerPanel.Controls.Add(lblFileViewerInfo);
        fileViewerPanel.Dock = DockStyle.Fill;
        fileViewerPanel.Name = "fileViewerPanel";
        fileViewerPanel.TabIndex = 0;
        // 
        // lblFileViewerInfo
        // 
        lblFileViewerInfo.AutoEllipsis = true;
        lblFileViewerInfo.Dock = DockStyle.Top;
        lblFileViewerInfo.ForeColor = Color.FromArgb(220, 220, 220);
        lblFileViewerInfo.Height = 34;
        lblFileViewerInfo.Name = "lblFileViewerInfo";
        lblFileViewerInfo.Padding = new Padding(8, 0, 8, 0);
        lblFileViewerInfo.Text = "Select a file to preview it.";
        lblFileViewerInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fileViewerBody
        // 
        fileViewerBody.BackColor = Color.FromArgb(20, 20, 20);
        fileViewerBody.Controls.Add(mediaFileHost);
        fileViewerBody.Controls.Add(txtHexViewer);
        fileViewerBody.Controls.Add(txtFileViewer);
        fileViewerBody.Controls.Add(pictureFileViewer);
        fileViewerBody.Dock = DockStyle.Fill;
        fileViewerBody.Name = "fileViewerBody";
        fileViewerBody.Padding = new Padding(6);
        fileViewerBody.TabIndex = 1;
        // 
        // pictureFileViewer
        // 
        pictureFileViewer.BackColor = Color.FromArgb(20, 20, 20);
        pictureFileViewer.Dock = DockStyle.Fill;
        pictureFileViewer.Name = "pictureFileViewer";
        pictureFileViewer.SizeMode = PictureBoxSizeMode.Zoom;
        pictureFileViewer.TabIndex = 0;
        pictureFileViewer.TabStop = false;
        pictureFileViewer.Visible = false;
        // 
        // txtFileViewer
        // 
        txtFileViewer.Dock = DockStyle.Fill;
        txtFileViewer.Font = new Font("Consolas", 9F);
        txtFileViewer.Multiline = true;
        txtFileViewer.Name = "txtFileViewer";
        txtFileViewer.ReadOnly = true;
        txtFileViewer.ScrollBars = ScrollBars.Both;
        txtFileViewer.TabIndex = 1;
        txtFileViewer.Visible = false;
        txtFileViewer.WordWrap = false;
        // 
        // txtHexViewer
        // 
        txtHexViewer.Dock = DockStyle.Fill;
        txtHexViewer.Font = new Font("Consolas", 9F);
        txtHexViewer.Multiline = true;
        txtHexViewer.Name = "txtHexViewer";
        txtHexViewer.ReadOnly = true;
        txtHexViewer.ScrollBars = ScrollBars.Both;
        txtHexViewer.TabIndex = 2;
        txtHexViewer.Visible = false;
        txtHexViewer.WordWrap = false;
        // 
        // mediaFileHost
        // 
        mediaFileHost.BackColor = Color.Black;
        mediaFileHost.Child = mediaFileViewer;
        mediaFileHost.Dock = DockStyle.Fill;
        mediaFileHost.Name = "mediaFileHost";
        mediaFileHost.TabIndex = 3;
        mediaFileHost.Visible = false;
        // 
        // mediaFileViewer
        // 
        mediaFileViewer.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
        mediaFileViewer.ScrubbingEnabled = true;
        mediaFileViewer.Stretch = System.Windows.Media.Stretch.Uniform;
        mediaFileViewer.UnloadedBehavior = System.Windows.Controls.MediaState.Manual;
        mediaFileViewer.Volume = 0.75D;
        mediaFileViewer.MediaFailed += mediaFileViewer_MediaFailed;
        // 
        // fileViewerCommands
        // 
        fileViewerCommands.BackColor = Color.FromArgb(45, 45, 48);
        fileViewerCommands.Controls.Add(btnMediaLoad);
        fileViewerCommands.Controls.Add(btnMediaPlay);
        fileViewerCommands.Controls.Add(btnMediaPause);
        fileViewerCommands.Controls.Add(btnMediaStop);
        fileViewerCommands.Controls.Add(btnHexPrevious);
        fileViewerCommands.Controls.Add(btnHexNext);
        fileViewerCommands.Controls.Add(lblHexPage);
        fileViewerCommands.Dock = DockStyle.Bottom;
        fileViewerCommands.Height = 40;
        fileViewerCommands.Name = "fileViewerCommands";
        fileViewerCommands.Padding = new Padding(6);
        fileViewerCommands.TabIndex = 2;
        // 
        // btnMediaLoad
        // 
        btnMediaLoad.Location = new Point(6, 7);
        btnMediaLoad.Name = "btnMediaLoad";
        btnMediaLoad.Size = new Size(100, 27);
        btnMediaLoad.TabIndex = 0;
        btnMediaLoad.Text = "Load && Play";
        btnMediaLoad.Visible = false;
        btnMediaLoad.Click += btnMediaLoad_Click;
        // 
        // btnMediaPlay
        // 
        btnMediaPlay.Location = new Point(112, 7);
        btnMediaPlay.Name = "btnMediaPlay";
        btnMediaPlay.Size = new Size(62, 27);
        btnMediaPlay.TabIndex = 1;
        btnMediaPlay.Text = "Play";
        btnMediaPlay.Visible = false;
        btnMediaPlay.Click += btnMediaPlay_Click;
        // 
        // btnMediaPause
        // 
        btnMediaPause.Location = new Point(180, 7);
        btnMediaPause.Name = "btnMediaPause";
        btnMediaPause.Size = new Size(62, 27);
        btnMediaPause.TabIndex = 2;
        btnMediaPause.Text = "Pause";
        btnMediaPause.Visible = false;
        btnMediaPause.Click += btnMediaPause_Click;
        // 
        // btnMediaStop
        // 
        btnMediaStop.Location = new Point(248, 7);
        btnMediaStop.Name = "btnMediaStop";
        btnMediaStop.Size = new Size(62, 27);
        btnMediaStop.TabIndex = 3;
        btnMediaStop.Text = "Stop";
        btnMediaStop.Visible = false;
        btnMediaStop.Click += btnMediaStop_Click;
        // 
        // btnHexPrevious
        // 
        btnHexPrevious.Location = new Point(6, 7);
        btnHexPrevious.Name = "btnHexPrevious";
        btnHexPrevious.Size = new Size(76, 27);
        btnHexPrevious.TabIndex = 4;
        btnHexPrevious.Text = "Previous";
        btnHexPrevious.Visible = false;
        btnHexPrevious.Click += btnHexPrevious_Click;
        // 
        // btnHexNext
        // 
        btnHexNext.Location = new Point(88, 7);
        btnHexNext.Name = "btnHexNext";
        btnHexNext.Size = new Size(62, 27);
        btnHexNext.TabIndex = 5;
        btnHexNext.Text = "Next";
        btnHexNext.Visible = false;
        btnHexNext.Click += btnHexNext_Click;
        // 
        // lblHexPage
        // 
        lblHexPage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblHexPage.AutoEllipsis = true;
        lblHexPage.ForeColor = Color.FromArgb(220, 220, 220);
        lblHexPage.Location = new Point(158, 7);
        lblHexPage.Name = "lblHexPage";
        lblHexPage.Size = new Size(300, 27);
        lblHexPage.TabIndex = 6;
        lblHexPage.TextAlign = ContentAlignment.MiddleLeft;
        lblHexPage.Visible = false;
        // 
        // tabExecutable
        // 
        tabExecutable.BackColor = Color.FromArgb(60, 63, 65);
        tabExecutable.Controls.Add(gridModules);
        tabExecutable.Controls.Add(executableHeaderPanel);
        tabExecutable.Text = "Executable";
        // 
        // executableHeaderPanel
        // 
        executableHeaderPanel.BackColor = Color.FromArgb(45, 45, 48);
        executableHeaderPanel.Controls.Add(lblExecutableSummary);
        executableHeaderPanel.Dock = DockStyle.Top;
        executableHeaderPanel.Height = 52;
        // 
        // lblExecutableSummary
        // 
        lblExecutableSummary.AutoEllipsis = true;
        lblExecutableSummary.Dock = DockStyle.Fill;
        lblExecutableSummary.ForeColor = Color.FromArgb(220, 220, 220);
        lblExecutableSummary.Padding = new Padding(10, 0, 10, 0);
        lblExecutableSummary.Text = "Select a game to inspect eboot.bin and modules.";
        lblExecutableSummary.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // gridModules
        // 
        gridModules.AllowUserToAddRows = false;
        gridModules.AllowUserToDeleteRows = false;
        gridModules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridModules.Dock = DockStyle.Fill;
        gridModules.Name = "gridModules";
        gridModules.ReadOnly = true;
        // 
        // tabRaw
        // 
        tabRaw.BackColor = Color.FromArgb(60, 63, 65);
        tabRaw.Controls.Add(txtRawMetadata);
        tabRaw.Text = "Raw param.json";
        // 
        // txtRawMetadata
        // 
        txtRawMetadata.BorderStyle = BorderStyle.None;
        txtRawMetadata.Dock = DockStyle.Fill;
        txtRawMetadata.Font = new Font("Consolas", 9F);
        txtRawMetadata.Multiline = true;
        txtRawMetadata.Name = "txtRawMetadata";
        txtRawMetadata.ReadOnly = true;
        txtRawMetadata.ScrollBars = ScrollBars.Both;
        txtRawMetadata.WordWrap = false;
        // 
        // tabsToolFormats
        // 
        tabsToolFormats.Controls.Add(tabToolFfpfsc);
        tabsToolFormats.Controls.Add(tabToolFfpkg);
        tabsToolFormats.Controls.Add(tabToolExfat);
        tabsToolFormats.Controls.Add(tabToolSonyPkg);
        tabsToolFormats.Dock = DockStyle.Fill;
        tabsToolFormats.Name = "tabsToolFormats";
        tabsToolFormats.SelectedIndex = 0;
        // 
        // tabToolFfpfsc
        // 
        tabToolFfpfsc.BackColor = Color.FromArgb(60, 63, 65);
        tabToolFfpfsc.Controls.Add(layoutFfpfsc);
        tabToolFfpfsc.Text = "FFPFSC";
        // 
        // tabToolFfpkg
        // 
        tabToolFfpkg.BackColor = Color.FromArgb(60, 63, 65);
        tabToolFfpkg.Controls.Add(layoutFfpkg);
        tabToolFfpkg.Text = "FFPKG";
        // 
        // tabToolExfat
        // 
        tabToolExfat.BackColor = Color.FromArgb(60, 63, 65);
        tabToolExfat.Controls.Add(layoutExfat);
        tabToolExfat.Text = "exFAT";
        // 
        // tabToolSonyPkg
        // 
        tabToolSonyPkg.BackColor = Color.FromArgb(60, 63, 65);
        tabToolSonyPkg.Controls.Add(layoutSonyPkg);
        tabToolSonyPkg.Text = "PS5 debug PKG";
        // 
        // layoutFfpfsc
        // 
        layoutFfpfsc.ColumnCount = 1;
        layoutFfpfsc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpfsc.Controls.Add(ffpfscSelectionPanel, 0, 0);
        layoutFfpfsc.Controls.Add(sectionFfpfscCreate, 0, 1);
        layoutFfpfsc.Controls.Add(sectionFfpfscOperations, 0, 2);
        layoutFfpfsc.Dock = DockStyle.Fill;
        layoutFfpfsc.Name = "layoutFfpfsc";
        layoutFfpfsc.Padding = new Padding(8);
        layoutFfpfsc.RowCount = 3;
        layoutFfpfsc.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layoutFfpfsc.RowStyles.Add(new RowStyle(SizeType.Absolute, 205F));
        layoutFfpfsc.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // ffpfscSelectionPanel
        // 
        ffpfscSelectionPanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpfscSelectionPanel.Controls.Add(lblFfpfscSelectedGame);
        ffpfscSelectionPanel.Dock = DockStyle.Fill;
        ffpfscSelectionPanel.Margin = new Padding(0, 0, 0, 6);
        ffpfscSelectionPanel.Name = "ffpfscSelectionPanel";
        // 
        // lblFfpfscSelectedGame
        // 
        lblFfpfscSelectedGame.AutoEllipsis = true;
        lblFfpfscSelectedGame.Dock = DockStyle.Fill;
        lblFfpfscSelectedGame.ForeColor = Color.FromArgb(220, 220, 220);
        lblFfpfscSelectedGame.Padding = new Padding(12, 0, 12, 0);
        lblFfpfscSelectedGame.Text = "Select an unpacked game in the library above.";
        lblFfpfscSelectedGame.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionFfpfscCreate
        // 
        sectionFfpfscCreate.Controls.Add(ffpfscCreatePanel);
        sectionFfpfscCreate.Dock = DockStyle.Fill;
        sectionFfpfscCreate.Margin = new Padding(0, 0, 0, 6);
        sectionFfpfscCreate.Name = "sectionFfpfscCreate";
        sectionFfpfscCreate.SectionHeader = "Create compressed FFPFSC";
        // 
        // ffpfscCreatePanel
        // 
        ffpfscCreatePanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpfscCreatePanel.Controls.Add(layoutFfpfscOutput);
        ffpfscCreatePanel.Controls.Add(lblFfpfscLevel);
        ffpfscCreatePanel.Controls.Add(nudFfpfscLevel);
        ffpfscCreatePanel.Controls.Add(lblFfpfscGain);
        ffpfscCreatePanel.Controls.Add(nudFfpfscGain);
        ffpfscCreatePanel.Controls.Add(lblFfpfscCluster);
        ffpfscCreatePanel.Controls.Add(cboFfpfscCluster);
        ffpfscCreatePanel.Controls.Add(chkFfpfscAmpr);
        ffpfscCreatePanel.Controls.Add(btnFfpfscCreateSelected);
        ffpfscCreatePanel.Controls.Add(btnFfpfscWrapImage);
        ffpfscCreatePanel.Controls.Add(layoutFfpfscProgress);
        ffpfscCreatePanel.Controls.Add(btnFfpfscCancel);
        ffpfscCreatePanel.Dock = DockStyle.Fill;
        ffpfscCreatePanel.Name = "ffpfscCreatePanel";
        // 
        // layoutFfpfscOutput
        // 
        layoutFfpfscOutput.ColumnCount = 3;
        layoutFfpfscOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55F));
        layoutFfpfscOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpfscOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98F));
        layoutFfpfscOutput.Controls.Add(lblFfpfscOutput, 0, 0);
        layoutFfpfscOutput.Controls.Add(txtFfpfscOutput, 1, 0);
        layoutFfpfscOutput.Controls.Add(btnFfpfscBrowseOutput, 2, 0);
        layoutFfpfscOutput.Dock = DockStyle.Top;
        layoutFfpfscOutput.Location = new Point(0, 0);
        layoutFfpfscOutput.Name = "layoutFfpfscOutput";
        layoutFfpfscOutput.Padding = new Padding(12, 7, 4, 3);
        layoutFfpfscOutput.RowCount = 1;
        layoutFfpfscOutput.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutFfpfscOutput.Size = new Size(1344, 42);
        layoutFfpfscOutput.TabIndex = 0;
        // 
        // lblFfpfscOutput
        // 
        lblFfpfscOutput.Dock = DockStyle.Fill;
        lblFfpfscOutput.Margin = new Padding(0);
        lblFfpfscOutput.Name = "lblFfpfscOutput";
        lblFfpfscOutput.Text = "Output:";
        lblFfpfscOutput.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtFfpfscOutput
        // 
        txtFfpfscOutput.Dock = DockStyle.Fill;
        txtFfpfscOutput.Margin = new Padding(0, 2, 6, 2);
        txtFfpfscOutput.Name = "txtFfpfscOutput";
        txtFfpfscOutput.PlaceholderText = "Select an output .ffpfsc path";
        txtFfpfscOutput.TabIndex = 0;
        // 
        // btnFfpfscBrowseOutput
        // 
        btnFfpfscBrowseOutput.Dock = DockStyle.Fill;
        btnFfpfscBrowseOutput.Margin = new Padding(0);
        btnFfpfscBrowseOutput.Name = "btnFfpfscBrowseOutput";
        btnFfpfscBrowseOutput.TabIndex = 1;
        btnFfpfscBrowseOutput.Text = "Browse...";
        btnFfpfscBrowseOutput.Click += btnFfpfscBrowseOutput_Click;
        // 
        // lblFfpfscLevel
        // 
        lblFfpfscLevel.Location = new Point(12, 45);
        lblFfpfscLevel.Name = "lblFfpfscLevel";
        lblFfpfscLevel.Size = new Size(110, 23);
        lblFfpfscLevel.Text = "Compression level:";
        lblFfpfscLevel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // nudFfpfscLevel
        // 
        nudFfpfscLevel.Location = new Point(128, 45);
        nudFfpfscLevel.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
        nudFfpfscLevel.Name = "nudFfpfscLevel";
        nudFfpfscLevel.Size = new Size(62, 23);
        nudFfpfscLevel.TabIndex = 2;
        nudFfpfscLevel.Value = new decimal(new int[] { 9, 0, 0, 0 });
        // 
        // lblFfpfscGain
        // 
        lblFfpfscGain.Location = new Point(214, 45);
        lblFfpfscGain.Name = "lblFfpfscGain";
        lblFfpfscGain.Size = new Size(139, 23);
        lblFfpfscGain.Text = "Minimum block gain %:";
        lblFfpfscGain.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // nudFfpfscGain
        // 
        nudFfpfscGain.Location = new Point(359, 45);
        nudFfpfscGain.Name = "nudFfpfscGain";
        nudFfpfscGain.Size = new Size(62, 23);
        nudFfpfscGain.TabIndex = 3;
        nudFfpfscGain.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // lblFfpfscCluster
        // 
        lblFfpfscCluster.Location = new Point(446, 45);
        lblFfpfscCluster.Name = "lblFfpfscCluster";
        lblFfpfscCluster.Size = new Size(111, 23);
        lblFfpfscCluster.Text = "exFAT cluster size:";
        lblFfpfscCluster.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // cboFfpfscCluster
        // 
        cboFfpfscCluster.DrawMode = DrawMode.OwnerDrawVariable;
        cboFfpfscCluster.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFfpfscCluster.FormattingEnabled = true;
        cboFfpfscCluster.Items.AddRange(new object[] { "Automatic", "32 KiB", "64 KiB" });
        cboFfpfscCluster.Location = new Point(563, 45);
        cboFfpfscCluster.Name = "cboFfpfscCluster";
        cboFfpfscCluster.SelectedIndex = 0;
        cboFfpfscCluster.Size = new Size(126, 24);
        cboFfpfscCluster.TabIndex = 4;
        // 
        // chkFfpfscAmpr
        // 
        chkFfpfscAmpr.AutoSize = true;
        chkFfpfscAmpr.Checked = true;
        chkFfpfscAmpr.CheckState = CheckState.Checked;
        chkFfpfscAmpr.Location = new Point(716, 48);
        chkFfpfscAmpr.Name = "chkFfpfscAmpr";
        chkFfpfscAmpr.Size = new Size(223, 19);
        chkFfpfscAmpr.TabIndex = 5;
        chkFfpfscAmpr.Text = "Generate AMPR index when required";
        // 
        // btnFfpfscCreateSelected
        // 
        btnFfpfscCreateSelected.Enabled = false;
        btnFfpfscCreateSelected.Location = new Point(12, 80);
        btnFfpfscCreateSelected.Name = "btnFfpfscCreateSelected";
        btnFfpfscCreateSelected.Size = new Size(198, 31);
        btnFfpfscCreateSelected.TabIndex = 6;
        btnFfpfscCreateSelected.Text = "Create from Selected Dump";
        btnFfpfscCreateSelected.Click += btnFfpfscCreateSelected_Click;
        // 
        // btnFfpfscWrapImage
        // 
        btnFfpfscWrapImage.Location = new Point(216, 80);
        btnFfpfscWrapImage.Name = "btnFfpfscWrapImage";
        btnFfpfscWrapImage.Size = new Size(216, 31);
        btnFfpfscWrapImage.TabIndex = 7;
        btnFfpfscWrapImage.Text = "Wrap Existing exFAT / FFPKG...";
        btnFfpfscWrapImage.Click += btnFfpfscWrapImage_Click;
        // 
        // btnFfpfscCancel
        // 
        btnFfpfscCancel.Enabled = false;
        btnFfpfscCancel.Location = new Point(438, 80);
        btnFfpfscCancel.Name = "btnFfpfscCancel";
        btnFfpfscCancel.Size = new Size(92, 31);
        btnFfpfscCancel.TabIndex = 8;
        btnFfpfscCancel.Text = "Cancel";
        btnFfpfscCancel.Click += btnFfpfscCancel_Click;
        // 
        // layoutFfpfscProgress
        // 
        layoutFfpfscProgress.ColumnCount = 1;
        layoutFfpfscProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpfscProgress.Controls.Add(progressFfpfsc, 0, 0);
        layoutFfpfscProgress.Controls.Add(lblFfpfscProgress, 0, 1);
        layoutFfpfscProgress.Dock = DockStyle.Bottom;
        layoutFfpfscProgress.Location = new Point(0, 118);
        layoutFfpfscProgress.Name = "layoutFfpfscProgress";
        layoutFfpfscProgress.Padding = new Padding(12, 0, 4, 2);
        layoutFfpfscProgress.RowCount = 2;
        layoutFfpfscProgress.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        layoutFfpfscProgress.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutFfpfscProgress.Size = new Size(1344, 58);
        layoutFfpfscProgress.TabIndex = 9;
        // 
        // progressFfpfsc
        // 
        progressFfpfsc.Dock = DockStyle.Fill;
        progressFfpfsc.Margin = new Padding(0, 1, 0, 2);
        progressFfpfsc.Name = "progressFfpfsc";
        progressFfpfsc.TabIndex = 9;
        progressFfpfsc.TextMode = DarkUI.Controls.DarkProgressBarMode.Percentage;
        // 
        // lblFfpfscProgress
        // 
        lblFfpfscProgress.AutoEllipsis = true;
        lblFfpfscProgress.Dock = DockStyle.Fill;
        lblFfpfscProgress.Margin = new Padding(0);
        lblFfpfscProgress.Name = "lblFfpfscProgress";
        lblFfpfscProgress.Text = "Ready. PFSC uses fixed 64 KiB logical blocks with mixed zlib/raw storage.";
        lblFfpfscProgress.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionFfpfscOperations
        // 
        sectionFfpfscOperations.Controls.Add(ffpfscOperationsPanel);
        sectionFfpfscOperations.Dock = DockStyle.Fill;
        sectionFfpfscOperations.Margin = new Padding(0);
        sectionFfpfscOperations.Name = "sectionFfpfscOperations";
        sectionFfpfscOperations.SectionHeader = "Inspect, validate and extract";
        // 
        // ffpfscOperationsPanel
        // 
        ffpfscOperationsPanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpfscOperationsPanel.Controls.Add(btnFfpfscVerify);
        ffpfscOperationsPanel.Controls.Add(btnFfpfscExtract);
        ffpfscOperationsPanel.Controls.Add(lblFfpfscOperationsInfo);
        ffpfscOperationsPanel.Dock = DockStyle.Fill;
        ffpfscOperationsPanel.Name = "ffpfscOperationsPanel";
        // 
        // btnFfpfscVerify
        // 
        btnFfpfscVerify.Location = new Point(12, 13);
        btnFfpfscVerify.Name = "btnFfpfscVerify";
        btnFfpfscVerify.Size = new Size(178, 31);
        btnFfpfscVerify.TabIndex = 0;
        btnFfpfscVerify.Text = "Inspect and Full Verify...";
        btnFfpfscVerify.Click += btnFfpfscVerify_Click;
        // 
        // btnFfpfscExtract
        // 
        btnFfpfscExtract.Location = new Point(196, 13);
        btnFfpfscExtract.Name = "btnFfpfscExtract";
        btnFfpfscExtract.Size = new Size(178, 31);
        btnFfpfscExtract.TabIndex = 1;
        btnFfpfscExtract.Text = "Extract Inner Image...";
        btnFfpfscExtract.Click += btnFfpfscExtract_Click;
        // 
        // lblFfpfscOperationsInfo
        // 
        lblFfpfscOperationsInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblFfpfscOperationsInfo.AutoEllipsis = true;
        lblFfpfscOperationsInfo.Location = new Point(390, 12);
        lblFfpfscOperationsInfo.Name = "lblFfpfscOperationsInfo";
        lblFfpfscOperationsInfo.Size = new Size(941, 34);
        lblFfpfscOperationsInfo.Text = "Verification checks PFS structure, every PFSC block and a decoded SHA-256 hash.";
        lblFfpfscOperationsInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // layoutFfpkg
        // 
        layoutFfpkg.ColumnCount = 1;
        layoutFfpkg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpkg.Controls.Add(ffpkgSelectionPanel, 0, 0);
        layoutFfpkg.Controls.Add(sectionFfpkgCreate, 0, 1);
        layoutFfpkg.Controls.Add(sectionFfpkgOperations, 0, 2);
        layoutFfpkg.Dock = DockStyle.Fill;
        layoutFfpkg.Name = "layoutFfpkg";
        layoutFfpkg.Padding = new Padding(8);
        layoutFfpkg.RowCount = 3;
        layoutFfpkg.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layoutFfpkg.RowStyles.Add(new RowStyle(SizeType.Absolute, 205F));
        layoutFfpkg.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // ffpkgSelectionPanel
        // 
        ffpkgSelectionPanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpkgSelectionPanel.Controls.Add(lblFfpkgSelectedGame);
        ffpkgSelectionPanel.Dock = DockStyle.Fill;
        ffpkgSelectionPanel.Margin = new Padding(0, 0, 0, 6);
        ffpkgSelectionPanel.Name = "ffpkgSelectionPanel";
        // 
        // lblFfpkgSelectedGame
        // 
        lblFfpkgSelectedGame.AutoEllipsis = true;
        lblFfpkgSelectedGame.Dock = DockStyle.Fill;
        lblFfpkgSelectedGame.ForeColor = Color.FromArgb(220, 220, 220);
        lblFfpkgSelectedGame.Padding = new Padding(12, 0, 12, 0);
        lblFfpkgSelectedGame.Text = "Select an unpacked game in the library above.";
        lblFfpkgSelectedGame.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionFfpkgCreate
        // 
        sectionFfpkgCreate.Controls.Add(ffpkgCreatePanel);
        sectionFfpkgCreate.Dock = DockStyle.Fill;
        sectionFfpkgCreate.Margin = new Padding(0, 0, 0, 6);
        sectionFfpkgCreate.Name = "sectionFfpkgCreate";
        sectionFfpkgCreate.SectionHeader = "Create standalone FFPKG (UFS2) image";
        // 
        // ffpkgCreatePanel
        // 
        ffpkgCreatePanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpkgCreatePanel.Controls.Add(layoutFfpkgOutput);
        ffpkgCreatePanel.Controls.Add(lblFfpkgPreset);
        ffpkgCreatePanel.Controls.Add(btnFfpkgCreate);
        ffpkgCreatePanel.Controls.Add(btnFfpkgCancel);
        ffpkgCreatePanel.Controls.Add(layoutFfpkgProgress);
        ffpkgCreatePanel.Dock = DockStyle.Fill;
        ffpkgCreatePanel.Name = "ffpkgCreatePanel";
        // 
        // layoutFfpkgOutput
        // 
        layoutFfpkgOutput.ColumnCount = 3;
        layoutFfpkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55F));
        layoutFfpkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98F));
        layoutFfpkgOutput.Controls.Add(lblFfpkgOutput, 0, 0);
        layoutFfpkgOutput.Controls.Add(txtFfpkgOutput, 1, 0);
        layoutFfpkgOutput.Controls.Add(btnFfpkgBrowseOutput, 2, 0);
        layoutFfpkgOutput.Dock = DockStyle.Top;
        layoutFfpkgOutput.Name = "layoutFfpkgOutput";
        layoutFfpkgOutput.Padding = new Padding(12, 7, 4, 3);
        layoutFfpkgOutput.RowCount = 1;
        layoutFfpkgOutput.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutFfpkgOutput.Size = new Size(1344, 42);
        // 
        // lblFfpkgOutput
        // 
        lblFfpkgOutput.Dock = DockStyle.Fill;
        lblFfpkgOutput.Margin = new Padding(0);
        lblFfpkgOutput.Name = "lblFfpkgOutput";
        lblFfpkgOutput.Text = "Output:";
        lblFfpkgOutput.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtFfpkgOutput
        // 
        txtFfpkgOutput.Dock = DockStyle.Fill;
        txtFfpkgOutput.Margin = new Padding(0, 2, 6, 2);
        txtFfpkgOutput.Name = "txtFfpkgOutput";
        txtFfpkgOutput.PlaceholderText = "Select an output .ffpkg path";
        // 
        // btnFfpkgBrowseOutput
        // 
        btnFfpkgBrowseOutput.Dock = DockStyle.Fill;
        btnFfpkgBrowseOutput.Margin = new Padding(0);
        btnFfpkgBrowseOutput.Name = "btnFfpkgBrowseOutput";
        btnFfpkgBrowseOutput.Text = "Browse...";
        btnFfpkgBrowseOutput.Click += btnFfpkgBrowseOutput_Click;
        // 
        // lblFfpkgPreset
        // 
        lblFfpkgPreset.AutoEllipsis = true;
        lblFfpkgPreset.Location = new Point(12, 45);
        lblFfpkgPreset.Name = "lblFfpkgPreset";
        lblFfpkgPreset.Size = new Size(900, 24);
        lblFfpkgPreset.Text = "PS5 preset: UFS2, 4 KiB sectors/fragments, 32 KiB blocks, 32-bit inode layout, minfree 0%, optimize space.";
        lblFfpkgPreset.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnFfpkgCreate
        // 
        btnFfpkgCreate.Enabled = false;
        btnFfpkgCreate.Location = new Point(12, 76);
        btnFfpkgCreate.Name = "btnFfpkgCreate";
        btnFfpkgCreate.Size = new Size(198, 31);
        btnFfpkgCreate.Text = "Create from Selected Dump";
        btnFfpkgCreate.Click += btnFfpkgCreate_Click;
        // 
        // btnFfpkgCancel
        // 
        btnFfpkgCancel.Enabled = false;
        btnFfpkgCancel.Location = new Point(216, 76);
        btnFfpkgCancel.Name = "btnFfpkgCancel";
        btnFfpkgCancel.Size = new Size(92, 31);
        btnFfpkgCancel.Text = "Cancel";
        btnFfpkgCancel.Click += btnFfpkgCancel_Click;
        // 
        // layoutFfpkgProgress
        // 
        layoutFfpkgProgress.ColumnCount = 1;
        layoutFfpkgProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutFfpkgProgress.Controls.Add(progressFfpkg, 0, 0);
        layoutFfpkgProgress.Controls.Add(lblFfpkgProgress, 0, 1);
        layoutFfpkgProgress.Dock = DockStyle.Bottom;
        layoutFfpkgProgress.Name = "layoutFfpkgProgress";
        layoutFfpkgProgress.Padding = new Padding(12, 0, 4, 2);
        layoutFfpkgProgress.RowCount = 2;
        layoutFfpkgProgress.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        layoutFfpkgProgress.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutFfpkgProgress.Size = new Size(1344, 58);
        // 
        // progressFfpkg
        // 
        progressFfpkg.Dock = DockStyle.Fill;
        progressFfpkg.Margin = new Padding(0, 1, 0, 2);
        progressFfpkg.Name = "progressFfpkg";
        progressFfpkg.TextMode = DarkUI.Controls.DarkProgressBarMode.Percentage;
        // 
        // lblFfpkgProgress
        // 
        lblFfpkgProgress.AutoEllipsis = true;
        lblFfpkgProgress.Dock = DockStyle.Fill;
        lblFfpkgProgress.Margin = new Padding(0);
        lblFfpkgProgress.Name = "lblFfpkgProgress";
        lblFfpkgProgress.Text = "Ready. Creation performs a complete UFS2 verification before publishing the output.";
        lblFfpkgProgress.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionFfpkgOperations
        // 
        sectionFfpkgOperations.Controls.Add(ffpkgOperationsPanel);
        sectionFfpkgOperations.Dock = DockStyle.Fill;
        sectionFfpkgOperations.Margin = new Padding(0);
        sectionFfpkgOperations.Name = "sectionFfpkgOperations";
        sectionFfpkgOperations.SectionHeader = "Inspect, validate, extract, edit and rebuild";
        // 
        // ffpkgOperationsPanel
        // 
        ffpkgOperationsPanel.BackColor = Color.FromArgb(45, 45, 48);
        ffpkgOperationsPanel.Controls.Add(btnFfpkgVerify);
        ffpkgOperationsPanel.Controls.Add(btnFfpkgExtract);
        ffpkgOperationsPanel.Controls.Add(btnFfpkgEdit);
        ffpkgOperationsPanel.Controls.Add(btnFfpkgRebuild);
        ffpkgOperationsPanel.Controls.Add(lblFfpkgOperationsInfo);
        ffpkgOperationsPanel.Dock = DockStyle.Fill;
        ffpkgOperationsPanel.Name = "ffpkgOperationsPanel";
        // 
        // btnFfpkgVerify
        // 
        btnFfpkgVerify.Location = new Point(12, 13);
        btnFfpkgVerify.Name = "btnFfpkgVerify";
        btnFfpkgVerify.Size = new Size(178, 31);
        btnFfpkgVerify.Text = "Inspect and Full Verify...";
        btnFfpkgVerify.Click += btnFfpkgVerify_Click;
        // 
        // btnFfpkgExtract
        // 
        btnFfpkgExtract.Location = new Point(196, 13);
        btnFfpkgExtract.Name = "btnFfpkgExtract";
        btnFfpkgExtract.Size = new Size(146, 31);
        btnFfpkgExtract.Text = "Extract Files...";
        btnFfpkgExtract.Click += btnFfpkgExtract_Click;
        // 
        // btnFfpkgEdit
        // 
        btnFfpkgEdit.Location = new Point(348, 13);
        btnFfpkgEdit.Name = "btnFfpkgEdit";
        btnFfpkgEdit.Size = new Size(130, 31);
        btnFfpkgEdit.Text = "Edit Files...";
        btnFfpkgEdit.Click += btnFfpkgEdit_Click;
        // 
        // btnFfpkgRebuild
        // 
        btnFfpkgRebuild.Location = new Point(484, 13);
        btnFfpkgRebuild.Name = "btnFfpkgRebuild";
        btnFfpkgRebuild.Size = new Size(156, 31);
        btnFfpkgRebuild.Text = "Verified Rebuild...";
        btnFfpkgRebuild.Click += btnFfpkgRebuild_Click;
        // 
        // lblFfpkgOperationsInfo
        // 
        lblFfpkgOperationsInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblFfpkgOperationsInfo.AutoEllipsis = true;
        lblFfpkgOperationsInfo.Location = new Point(656, 12);
        lblFfpkgOperationsInfo.Name = "lblFfpkgOperationsInfo";
        lblFfpkgOperationsInfo.Size = new Size(675, 34);
        lblFfpkgOperationsInfo.Text = "Verification streams every UFS2 file, computes a manifest hash, and runs the embedded filesystem checker.";
        lblFfpkgOperationsInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // layoutExfat
        // 
        layoutExfat.ColumnCount = 1;
        layoutExfat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutExfat.Controls.Add(exfatSelectionPanel, 0, 0);
        layoutExfat.Controls.Add(sectionExfatCreate, 0, 1);
        layoutExfat.Controls.Add(sectionExfatOperations, 0, 2);
        layoutExfat.Dock = DockStyle.Fill;
        layoutExfat.Name = "layoutExfat";
        layoutExfat.Padding = new Padding(8);
        layoutExfat.RowCount = 3;
        layoutExfat.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layoutExfat.RowStyles.Add(new RowStyle(SizeType.Absolute, 215F));
        layoutExfat.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // exfatSelectionPanel
        // 
        exfatSelectionPanel.BackColor = Color.FromArgb(45, 45, 48);
        exfatSelectionPanel.Controls.Add(lblExfatSelectedGame);
        exfatSelectionPanel.Dock = DockStyle.Fill;
        exfatSelectionPanel.Margin = new Padding(0, 0, 0, 6);
        exfatSelectionPanel.Name = "exfatSelectionPanel";
        // 
        // lblExfatSelectedGame
        // 
        lblExfatSelectedGame.AutoEllipsis = true;
        lblExfatSelectedGame.Dock = DockStyle.Fill;
        lblExfatSelectedGame.ForeColor = Color.FromArgb(220, 220, 220);
        lblExfatSelectedGame.Padding = new Padding(12, 0, 12, 0);
        lblExfatSelectedGame.Text = "Select an unpacked game in the library above.";
        lblExfatSelectedGame.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionExfatCreate
        // 
        sectionExfatCreate.Controls.Add(exfatCreatePanel);
        sectionExfatCreate.Dock = DockStyle.Fill;
        sectionExfatCreate.Margin = new Padding(0, 0, 0, 6);
        sectionExfatCreate.Name = "sectionExfatCreate";
        sectionExfatCreate.SectionHeader = "Create standalone exFAT image";
        // 
        // exfatCreatePanel
        // 
        exfatCreatePanel.BackColor = Color.FromArgb(45, 45, 48);
        exfatCreatePanel.Controls.Add(layoutExfatOutput);
        exfatCreatePanel.Controls.Add(lblExfatCluster);
        exfatCreatePanel.Controls.Add(cboExfatCluster);
        exfatCreatePanel.Controls.Add(chkExfatAmpr);
        exfatCreatePanel.Controls.Add(btnExfatCreate);
        exfatCreatePanel.Controls.Add(btnExfatCancel);
        exfatCreatePanel.Controls.Add(layoutExfatProgress);
        exfatCreatePanel.Dock = DockStyle.Fill;
        exfatCreatePanel.Name = "exfatCreatePanel";
        // 
        // layoutExfatOutput
        // 
        layoutExfatOutput.ColumnCount = 3;
        layoutExfatOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55F));
        layoutExfatOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutExfatOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98F));
        layoutExfatOutput.Controls.Add(lblExfatOutput, 0, 0);
        layoutExfatOutput.Controls.Add(txtExfatOutput, 1, 0);
        layoutExfatOutput.Controls.Add(btnExfatBrowseOutput, 2, 0);
        layoutExfatOutput.Dock = DockStyle.Top;
        layoutExfatOutput.Name = "layoutExfatOutput";
        layoutExfatOutput.Padding = new Padding(12, 7, 4, 3);
        layoutExfatOutput.RowCount = 1;
        layoutExfatOutput.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutExfatOutput.Size = new Size(1344, 42);
        // 
        // lblExfatOutput
        // 
        lblExfatOutput.Dock = DockStyle.Fill;
        lblExfatOutput.Margin = new Padding(0);
        lblExfatOutput.Name = "lblExfatOutput";
        lblExfatOutput.Text = "Output:";
        lblExfatOutput.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtExfatOutput
        // 
        txtExfatOutput.Dock = DockStyle.Fill;
        txtExfatOutput.Margin = new Padding(0, 2, 6, 2);
        txtExfatOutput.Name = "txtExfatOutput";
        txtExfatOutput.PlaceholderText = "Select an output .exfat path";
        // 
        // btnExfatBrowseOutput
        // 
        btnExfatBrowseOutput.Dock = DockStyle.Fill;
        btnExfatBrowseOutput.Margin = new Padding(0);
        btnExfatBrowseOutput.Name = "btnExfatBrowseOutput";
        btnExfatBrowseOutput.Text = "Browse...";
        btnExfatBrowseOutput.Click += btnExfatBrowseOutput_Click;
        // 
        // lblExfatCluster
        // 
        lblExfatCluster.Location = new Point(12, 45);
        lblExfatCluster.Name = "lblExfatCluster";
        lblExfatCluster.Size = new Size(111, 23);
        lblExfatCluster.Text = "Cluster size:";
        lblExfatCluster.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // cboExfatCluster
        // 
        cboExfatCluster.DrawMode = DrawMode.OwnerDrawVariable;
        cboExfatCluster.DropDownStyle = ComboBoxStyle.DropDownList;
        cboExfatCluster.FormattingEnabled = true;
        cboExfatCluster.Items.AddRange(new object[] { "Automatic", "32 KiB", "64 KiB (Recommended)" });
        cboExfatCluster.Location = new Point(128, 45);
        cboExfatCluster.Name = "cboExfatCluster";
        cboExfatCluster.SelectedIndex = 2;
        cboExfatCluster.Size = new Size(174, 24);
        // 
        // chkExfatAmpr
        // 
        chkExfatAmpr.AutoSize = true;
        chkExfatAmpr.Checked = true;
        chkExfatAmpr.CheckState = CheckState.Checked;
        chkExfatAmpr.Location = new Point(330, 48);
        chkExfatAmpr.Name = "chkExfatAmpr";
        chkExfatAmpr.Size = new Size(223, 19);
        chkExfatAmpr.Text = "Generate AMPR index when required";
        // 
        // btnExfatCreate
        // 
        btnExfatCreate.Enabled = false;
        btnExfatCreate.Location = new Point(12, 80);
        btnExfatCreate.Name = "btnExfatCreate";
        btnExfatCreate.Size = new Size(198, 31);
        btnExfatCreate.Text = "Create from Selected Dump";
        btnExfatCreate.Click += btnExfatCreate_Click;
        // 
        // btnExfatCancel
        // 
        btnExfatCancel.Enabled = false;
        btnExfatCancel.Location = new Point(216, 80);
        btnExfatCancel.Name = "btnExfatCancel";
        btnExfatCancel.Size = new Size(92, 31);
        btnExfatCancel.Text = "Cancel";
        btnExfatCancel.Click += btnExfatCancel_Click;
        // 
        // layoutExfatProgress
        // 
        layoutExfatProgress.ColumnCount = 1;
        layoutExfatProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutExfatProgress.Controls.Add(progressExfat, 0, 0);
        layoutExfatProgress.Controls.Add(lblExfatProgress, 0, 1);
        layoutExfatProgress.Dock = DockStyle.Bottom;
        layoutExfatProgress.Name = "layoutExfatProgress";
        layoutExfatProgress.Padding = new Padding(12, 0, 4, 2);
        layoutExfatProgress.RowCount = 2;
        layoutExfatProgress.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        layoutExfatProgress.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutExfatProgress.Size = new Size(1344, 58);
        // 
        // progressExfat
        // 
        progressExfat.Dock = DockStyle.Fill;
        progressExfat.Margin = new Padding(0, 1, 0, 2);
        progressExfat.Name = "progressExfat";
        progressExfat.TextMode = DarkUI.Controls.DarkProgressBarMode.Percentage;
        // 
        // lblExfatProgress
        // 
        lblExfatProgress.AutoEllipsis = true;
        lblExfatProgress.Dock = DockStyle.Fill;
        lblExfatProgress.Margin = new Padding(0);
        lblExfatProgress.Name = "lblExfatProgress";
        lblExfatProgress.Text = "Ready. A 64 KiB cluster is recommended for PS5 image mounting.";
        lblExfatProgress.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionExfatOperations
        // 
        sectionExfatOperations.Controls.Add(exfatOperationsPanel);
        sectionExfatOperations.Dock = DockStyle.Fill;
        sectionExfatOperations.Margin = new Padding(0);
        sectionExfatOperations.Name = "sectionExfatOperations";
        sectionExfatOperations.SectionHeader = "Inspect, edit, repair and extract";
        // 
        // exfatOperationsPanel
        // 
        exfatOperationsPanel.BackColor = Color.FromArgb(45, 45, 48);
        exfatOperationsPanel.Controls.Add(btnExfatVerify);
        exfatOperationsPanel.Controls.Add(btnExfatExtract);
        exfatOperationsPanel.Controls.Add(btnExfatRefreshAmpr);
        exfatOperationsPanel.Controls.Add(btnExfatEdit);
        exfatOperationsPanel.Controls.Add(btnExfatRepair);
        exfatOperationsPanel.Controls.Add(lblExfatOperationsInfo);
        exfatOperationsPanel.Dock = DockStyle.Fill;
        exfatOperationsPanel.Name = "exfatOperationsPanel";
        // 
        // btnExfatVerify
        // 
        btnExfatVerify.Location = new Point(12, 13);
        btnExfatVerify.Name = "btnExfatVerify";
        btnExfatVerify.Size = new Size(178, 31);
        btnExfatVerify.Text = "Inspect and Full Verify...";
        btnExfatVerify.Click += btnExfatVerify_Click;
        // 
        // btnExfatExtract
        // 
        btnExfatExtract.Location = new Point(196, 13);
        btnExfatExtract.Name = "btnExfatExtract";
        btnExfatExtract.Size = new Size(178, 31);
        btnExfatExtract.Text = "Extract Files...";
        btnExfatExtract.Click += btnExfatExtract_Click;
        // 
        // btnExfatRefreshAmpr
        // 
        btnExfatRefreshAmpr.Location = new Point(380, 13);
        btnExfatRefreshAmpr.Name = "btnExfatRefreshAmpr";
        btnExfatRefreshAmpr.Size = new Size(190, 31);
        btnExfatRefreshAmpr.Text = "Refresh AMPR Index...";
        btnExfatRefreshAmpr.Click += btnExfatRefreshAmpr_Click;
        // 
        // btnExfatEdit
        // 
        btnExfatEdit.Location = new Point(576, 13);
        btnExfatEdit.Name = "btnExfatEdit";
        btnExfatEdit.Size = new Size(150, 31);
        btnExfatEdit.Text = "Edit Files...";
        btnExfatEdit.Click += btnExfatEdit_Click;
        // 
        // btnExfatRepair
        // 
        btnExfatRepair.Location = new Point(732, 13);
        btnExfatRepair.Name = "btnExfatRepair";
        btnExfatRepair.Size = new Size(150, 31);
        btnExfatRepair.Text = "Repair Image...";
        btnExfatRepair.Click += btnExfatRepair_Click;
        // 
        // lblExfatOperationsInfo
        // 
        lblExfatOperationsInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblExfatOperationsInfo.AutoEllipsis = true;
        lblExfatOperationsInfo.Location = new Point(12, 52);
        lblExfatOperationsInfo.Name = "lblExfatOperationsInfo";
        lblExfatOperationsInfo.Size = new Size(1319, 38);
        lblExfatOperationsInfo.Text = "Editing uses rollback journaling or an atomic verified rebuild. Repair recovers a valid boot mirror and rebuilds readable filesystem metadata.";
        lblExfatOperationsInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // layoutSonyPkg
        // 
        layoutSonyPkg.ColumnCount = 1;
        layoutSonyPkg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutSonyPkg.Controls.Add(sonyPkgSelectionPanel, 0, 0);
        layoutSonyPkg.Controls.Add(sectionSonyPkgCreate, 0, 1);
        layoutSonyPkg.Controls.Add(sectionSonyPkgOperations, 0, 2);
        layoutSonyPkg.Dock = DockStyle.Fill;
        layoutSonyPkg.Name = "layoutSonyPkg";
        layoutSonyPkg.Padding = new Padding(8);
        layoutSonyPkg.RowCount = 3;
        layoutSonyPkg.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layoutSonyPkg.RowStyles.Add(new RowStyle(SizeType.Absolute, 245F));
        layoutSonyPkg.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // 
        // sonyPkgSelectionPanel
        // 
        sonyPkgSelectionPanel.BackColor = Color.FromArgb(45, 45, 48);
        sonyPkgSelectionPanel.Controls.Add(lblSonyPkgSelectedGame);
        sonyPkgSelectionPanel.Dock = DockStyle.Fill;
        sonyPkgSelectionPanel.Margin = new Padding(0, 0, 0, 6);
        sonyPkgSelectionPanel.Name = "sonyPkgSelectionPanel";
        // 
        // lblSonyPkgSelectedGame
        // 
        lblSonyPkgSelectedGame.AutoEllipsis = true;
        lblSonyPkgSelectedGame.Dock = DockStyle.Fill;
        lblSonyPkgSelectedGame.ForeColor = Color.FromArgb(220, 220, 220);
        lblSonyPkgSelectedGame.Padding = new Padding(12, 0, 12, 0);
        lblSonyPkgSelectedGame.Text = "Select an unpacked game in the library above.";
        lblSonyPkgSelectedGame.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionSonyPkgCreate
        // 
        sectionSonyPkgCreate.Controls.Add(sonyPkgCreatePanel);
        sectionSonyPkgCreate.Dock = DockStyle.Fill;
        sectionSonyPkgCreate.Margin = new Padding(0, 0, 0, 6);
        sectionSonyPkgCreate.Name = "sectionSonyPkgCreate";
        sectionSonyPkgCreate.SectionHeader = "Create encrypted PS5 debug package";
        // 
        // sonyPkgCreatePanel
        // 
        sonyPkgCreatePanel.BackColor = Color.FromArgb(45, 45, 48);
        sonyPkgCreatePanel.Controls.Add(layoutSonyPkgOutput);
        sonyPkgCreatePanel.Controls.Add(lblSonyPkgContentId);
        sonyPkgCreatePanel.Controls.Add(txtSonyPkgContentId);
        sonyPkgCreatePanel.Controls.Add(chkSonyPkgCustomPasscode);
        sonyPkgCreatePanel.Controls.Add(txtSonyPkgPasscode);
        sonyPkgCreatePanel.Controls.Add(btnSonyPkgCreate);
        sonyPkgCreatePanel.Controls.Add(btnSonyPkgCancel);
        sonyPkgCreatePanel.Controls.Add(layoutSonyPkgProgress);
        sonyPkgCreatePanel.Dock = DockStyle.Fill;
        sonyPkgCreatePanel.Name = "sonyPkgCreatePanel";
        // 
        // layoutSonyPkgOutput
        // 
        layoutSonyPkgOutput.ColumnCount = 3;
        layoutSonyPkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        layoutSonyPkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutSonyPkgOutput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98F));
        layoutSonyPkgOutput.Controls.Add(lblSonyPkgOutput, 0, 0);
        layoutSonyPkgOutput.Controls.Add(txtSonyPkgOutput, 1, 0);
        layoutSonyPkgOutput.Controls.Add(btnSonyPkgBrowseOutput, 2, 0);
        layoutSonyPkgOutput.Dock = DockStyle.Top;
        layoutSonyPkgOutput.Name = "layoutSonyPkgOutput";
        layoutSonyPkgOutput.Padding = new Padding(12, 7, 4, 3);
        layoutSonyPkgOutput.RowCount = 1;
        layoutSonyPkgOutput.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutSonyPkgOutput.Size = new Size(1344, 42);
        // 
        // lblSonyPkgOutput
        // 
        lblSonyPkgOutput.Dock = DockStyle.Fill;
        lblSonyPkgOutput.Margin = new Padding(0);
        lblSonyPkgOutput.Text = "Output:";
        lblSonyPkgOutput.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtSonyPkgOutput
        // 
        txtSonyPkgOutput.Dock = DockStyle.Fill;
        txtSonyPkgOutput.Margin = new Padding(0, 2, 6, 2);
        txtSonyPkgOutput.Name = "txtSonyPkgOutput";
        txtSonyPkgOutput.PlaceholderText = "Select an output .pkg path";
        // 
        // btnSonyPkgBrowseOutput
        // 
        btnSonyPkgBrowseOutput.Dock = DockStyle.Fill;
        btnSonyPkgBrowseOutput.Margin = new Padding(0);
        btnSonyPkgBrowseOutput.Name = "btnSonyPkgBrowseOutput";
        btnSonyPkgBrowseOutput.Text = "Browse...";
        btnSonyPkgBrowseOutput.Click += btnSonyPkgBrowseOutput_Click;
        // 
        // lblSonyPkgContentId
        // 
        lblSonyPkgContentId.Location = new Point(12, 49);
        lblSonyPkgContentId.Name = "lblSonyPkgContentId";
        lblSonyPkgContentId.Size = new Size(83, 23);
        lblSonyPkgContentId.Text = "Content ID:";
        lblSonyPkgContentId.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtSonyPkgContentId
        // 
        txtSonyPkgContentId.Location = new Point(101, 49);
        txtSonyPkgContentId.Name = "txtSonyPkgContentId";
        txtSonyPkgContentId.Size = new Size(354, 23);
        txtSonyPkgContentId.PlaceholderText = "UP0000-PPSA00000_00-CONTENT000000000";
        // 
        // chkSonyPkgCustomPasscode
        // 
        chkSonyPkgCustomPasscode.AutoSize = true;
        chkSonyPkgCustomPasscode.Location = new Point(12, 84);
        chkSonyPkgCustomPasscode.Name = "chkSonyPkgCustomPasscode";
        chkSonyPkgCustomPasscode.Size = new Size(143, 19);
        chkSonyPkgCustomPasscode.Text = "Use custom passcode";
        chkSonyPkgCustomPasscode.CheckedChanged += chkSonyPkgCustomPasscode_CheckedChanged;
        // 
        // txtSonyPkgPasscode
        // 
        txtSonyPkgPasscode.Enabled = false;
        txtSonyPkgPasscode.Location = new Point(171, 82);
        txtSonyPkgPasscode.MaxLength = 32;
        txtSonyPkgPasscode.Name = "txtSonyPkgPasscode";
        txtSonyPkgPasscode.Size = new Size(284, 23);
        txtSonyPkgPasscode.Text = "00000000000000000000000000000000";
        // 
        // btnSonyPkgCreate
        // 
        btnSonyPkgCreate.Enabled = false;
        btnSonyPkgCreate.Location = new Point(12, 117);
        btnSonyPkgCreate.Name = "btnSonyPkgCreate";
        btnSonyPkgCreate.Size = new Size(220, 31);
        btnSonyPkgCreate.Text = "Create from Selected Dump";
        btnSonyPkgCreate.Click += btnSonyPkgCreate_Click;
        // 
        // btnSonyPkgCancel
        // 
        btnSonyPkgCancel.Enabled = false;
        btnSonyPkgCancel.Location = new Point(238, 117);
        btnSonyPkgCancel.Name = "btnSonyPkgCancel";
        btnSonyPkgCancel.Size = new Size(92, 31);
        btnSonyPkgCancel.Text = "Cancel";
        btnSonyPkgCancel.Click += btnSonyPkgCancel_Click;
        // 
        // layoutSonyPkgProgress
        // 
        layoutSonyPkgProgress.ColumnCount = 1;
        layoutSonyPkgProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutSonyPkgProgress.Controls.Add(progressSonyPkg, 0, 0);
        layoutSonyPkgProgress.Controls.Add(lblSonyPkgProgress, 0, 1);
        layoutSonyPkgProgress.Dock = DockStyle.Bottom;
        layoutSonyPkgProgress.Name = "layoutSonyPkgProgress";
        layoutSonyPkgProgress.Padding = new Padding(12, 0, 4, 2);
        layoutSonyPkgProgress.RowCount = 2;
        layoutSonyPkgProgress.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        layoutSonyPkgProgress.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutSonyPkgProgress.Size = new Size(1344, 58);
        // 
        // progressSonyPkg
        // 
        progressSonyPkg.Dock = DockStyle.Fill;
        progressSonyPkg.Margin = new Padding(0, 1, 0, 2);
        progressSonyPkg.Name = "progressSonyPkg";
        progressSonyPkg.TextMode = DarkUI.Controls.DarkProgressBarMode.Percentage;
        // 
        // lblSonyPkgProgress
        // 
        lblSonyPkgProgress.AutoEllipsis = true;
        lblSonyPkgProgress.Dock = DockStyle.Fill;
        lblSonyPkgProgress.Margin = new Padding(0);
        lblSonyPkgProgress.Name = "lblSonyPkgProgress";
        lblSonyPkgProgress.Text = "Ready. The default passcode is 32 zero characters.";
        lblSonyPkgProgress.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // sectionSonyPkgOperations
        // 
        sectionSonyPkgOperations.Controls.Add(sonyPkgOperationsPanel);
        sectionSonyPkgOperations.Dock = DockStyle.Fill;
        sectionSonyPkgOperations.Margin = new Padding(0);
        sectionSonyPkgOperations.Name = "sectionSonyPkgOperations";
        sectionSonyPkgOperations.SectionHeader = "Validate encrypted debug package";
        // 
        // sonyPkgOperationsPanel
        // 
        sonyPkgOperationsPanel.BackColor = Color.FromArgb(45, 45, 48);
        sonyPkgOperationsPanel.Controls.Add(btnSonyPkgVerify);
        sonyPkgOperationsPanel.Controls.Add(btnSonyPkgExtract);
        sonyPkgOperationsPanel.Controls.Add(btnSonyPkgAcceptance);
        sonyPkgOperationsPanel.Controls.Add(btnSonyPkgSplit);
        sonyPkgOperationsPanel.Controls.Add(btnSonyPkgMerge);
        sonyPkgOperationsPanel.Controls.Add(lblSonyPkgOperationsInfo);
        sonyPkgOperationsPanel.Dock = DockStyle.Fill;
        sonyPkgOperationsPanel.Name = "sonyPkgOperationsPanel";
        // 
        // btnSonyPkgVerify
        // 
        btnSonyPkgVerify.Location = new Point(12, 13);
        btnSonyPkgVerify.Name = "btnSonyPkgVerify";
        btnSonyPkgVerify.Size = new Size(190, 31);
        btnSonyPkgVerify.Text = "Inspect and Full Verify...";
        btnSonyPkgVerify.Click += btnSonyPkgVerify_Click;
        // 
        // btnSonyPkgExtract
        // 
        btnSonyPkgExtract.Location = new Point(208, 13);
        btnSonyPkgExtract.Name = "btnSonyPkgExtract";
        btnSonyPkgExtract.Size = new Size(130, 31);
        btnSonyPkgExtract.Text = "Extract Files...";
        btnSonyPkgExtract.Click += btnSonyPkgExtract_Click;
        // 
        // btnSonyPkgAcceptance
        // 
        btnSonyPkgAcceptance.Location = new Point(344, 13);
        btnSonyPkgAcceptance.Name = "btnSonyPkgAcceptance";
        btnSonyPkgAcceptance.Size = new Size(178, 31);
        btnSonyPkgAcceptance.Text = "Acceptance Check...";
        btnSonyPkgAcceptance.Click += btnSonyPkgAcceptance_Click;
        // 
        // btnSonyPkgSplit
        // 
        btnSonyPkgSplit.Location = new Point(528, 13);
        btnSonyPkgSplit.Name = "btnSonyPkgSplit";
        btnSonyPkgSplit.Size = new Size(132, 31);
        btnSonyPkgSplit.Text = "Split Package...";
        btnSonyPkgSplit.Click += btnSonyPkgSplit_Click;
        // 
        // btnSonyPkgMerge
        // 
        btnSonyPkgMerge.Location = new Point(666, 13);
        btnSonyPkgMerge.Name = "btnSonyPkgMerge";
        btnSonyPkgMerge.Size = new Size(172, 31);
        btnSonyPkgMerge.Text = "Merge Verified Split...";
        btnSonyPkgMerge.Click += btnSonyPkgMerge_Click;
        // 
        // lblSonyPkgOperationsInfo
        // 
        lblSonyPkgOperationsInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblSonyPkgOperationsInfo.AutoEllipsis = true;
        lblSonyPkgOperationsInfo.Location = new Point(850, 12);
        lblSonyPkgOperationsInfo.Name = "lblSonyPkgOperationsInfo";
        lblSonyPkgOperationsInfo.Size = new Size(481, 34);
        lblSonyPkgOperationsInfo.Text = "Validation checks FIH, CNT, passcode key derivation, encrypted PFS sectors, NAPS metadata, and the indexed file tree.";
        lblSonyPkgOperationsInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // statusMain
        // 
        statusMain.BackColor = Color.FromArgb(45, 45, 48);
        statusMain.ForeColor = Color.FromArgb(220, 220, 220);
        statusMain.Items.AddRange(new ToolStripItem[] { statusLabel, statusSpring, statusCount });
        statusMain.Location = new Point(0, 859);
        statusMain.Name = "statusMain";
        statusMain.Size = new Size(1384, 22);
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Text = "Ready";
        // 
        // statusSpring
        // 
        statusSpring.Name = "statusSpring";
        statusSpring.Spring = true;
        // 
        // statusCount
        // 
        statusCount.Name = "statusCount";
        statusCount.Text = "0 games";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(60, 63, 65);
        ClientSize = new Size(1384, 881);
        Controls.Add(splitMain);
        Controls.Add(commandPanel);
        Controls.Add(statusMain);
        Controls.Add(menuMain);
        MainMenuStrip = menuMain;
        MinimumSize = new Size(1050, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PS5 PKG Tool - Dump Library";
        Shown += MainForm_Shown;
        FormClosing += MainForm_FormClosing;
        menuMain.ResumeLayout(false);
        menuMain.PerformLayout();
        commandPanel.ResumeLayout(false);
        commandPanel.PerformLayout();
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridLibrary).EndInit();
        tabsWorkspace.ResumeLayout(false);
        tabWorkspaceGeneral.ResumeLayout(false);
        tabWorkspaceTools.ResumeLayout(false);
        tabsDetails.ResumeLayout(false);
        tabOverview.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridOverview).EndInit();
        tabArtwork.ResumeLayout(false);
        splitArtwork.Panel1.ResumeLayout(false);
        splitArtwork.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitArtwork).EndInit();
        splitArtwork.ResumeLayout(false);
        sectionIcon.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureIcon).EndInit();
        sectionBackground.ResumeLayout(false);
        tabsBackgrounds.ResumeLayout(false);
        tabPic0.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground0).EndInit();
        tabPic1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground1).EndInit();
        tabPic2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBackground2).EndInit();
        tabTrophies.ResumeLayout(false);
        trophyHeaderPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridTrophies).EndInit();
        tabActivities.ResumeLayout(false);
        activitiesHeaderPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridActivities).EndInit();
        tabFiles.ResumeLayout(false);
        filesHeaderPanel.ResumeLayout(false);
        sectionFileBrowser.ResumeLayout(false);
        splitFileBrowser.Panel1.ResumeLayout(false);
        splitFileBrowser.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitFileBrowser).EndInit();
        splitFileBrowser.ResumeLayout(false);
        splitFileContentPreview.Panel1.ResumeLayout(false);
        splitFileContentPreview.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitFileContentPreview).EndInit();
        splitFileContentPreview.ResumeLayout(false);
        fileListPanel.ResumeLayout(false);
        fileFilterPanel.ResumeLayout(false);
        fileFilterPanel.PerformLayout();
        sectionFileViewer.ResumeLayout(false);
        fileViewerPanel.ResumeLayout(false);
        fileViewerBody.ResumeLayout(false);
        fileViewerBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureFileViewer).EndInit();
        fileViewerCommands.ResumeLayout(false);
        tabExecutable.ResumeLayout(false);
        executableHeaderPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridModules).EndInit();
        tabRaw.ResumeLayout(false);
        tabRaw.PerformLayout();
        tabsToolFormats.ResumeLayout(false);
        tabToolFfpfsc.ResumeLayout(false);
        tabToolFfpkg.ResumeLayout(false);
        tabToolExfat.ResumeLayout(false);
        tabToolSonyPkg.ResumeLayout(false);
        layoutFfpfsc.ResumeLayout(false);
        ffpfscSelectionPanel.ResumeLayout(false);
        sectionFfpfscCreate.ResumeLayout(false);
        ffpfscCreatePanel.ResumeLayout(false);
        ffpfscCreatePanel.PerformLayout();
        layoutFfpfscOutput.ResumeLayout(false);
        layoutFfpfscOutput.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudFfpfscLevel).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudFfpfscGain).EndInit();
        layoutFfpfscProgress.ResumeLayout(false);
        sectionFfpfscOperations.ResumeLayout(false);
        ffpfscOperationsPanel.ResumeLayout(false);
        layoutFfpkg.ResumeLayout(false);
        ffpkgSelectionPanel.ResumeLayout(false);
        sectionFfpkgCreate.ResumeLayout(false);
        ffpkgCreatePanel.ResumeLayout(false);
        layoutFfpkgOutput.ResumeLayout(false);
        layoutFfpkgOutput.PerformLayout();
        layoutFfpkgProgress.ResumeLayout(false);
        sectionFfpkgOperations.ResumeLayout(false);
        ffpkgOperationsPanel.ResumeLayout(false);
        layoutSonyPkg.ResumeLayout(false);
        sonyPkgSelectionPanel.ResumeLayout(false);
        sectionSonyPkgCreate.ResumeLayout(false);
        sonyPkgCreatePanel.ResumeLayout(false);
        sonyPkgCreatePanel.PerformLayout();
        layoutSonyPkgOutput.ResumeLayout(false);
        layoutSonyPkgOutput.PerformLayout();
        layoutSonyPkgProgress.ResumeLayout(false);
        sectionSonyPkgOperations.ResumeLayout(false);
        sonyPkgOperationsPanel.ResumeLayout(false);
        layoutExfat.ResumeLayout(false);
        exfatSelectionPanel.ResumeLayout(false);
        sectionExfatCreate.ResumeLayout(false);
        exfatCreatePanel.ResumeLayout(false);
        exfatCreatePanel.PerformLayout();
        layoutExfatOutput.ResumeLayout(false);
        layoutExfatOutput.PerformLayout();
        layoutExfatProgress.ResumeLayout(false);
        sectionExfatOperations.ResumeLayout(false);
        exfatOperationsPanel.ResumeLayout(false);
        statusMain.ResumeLayout(false);
        statusMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
