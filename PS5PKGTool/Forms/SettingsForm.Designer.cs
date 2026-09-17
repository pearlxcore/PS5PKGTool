#nullable enable

namespace PS5PKGTool.Forms;

partial class SettingsForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkTabControl tabsSettings = null!;

    private DarkUI.Controls.DarkTabPage tabLibrary = null!;
    private DarkUI.Controls.DarkLabel lblLibraryInfo = null!;
    private DarkUI.Controls.DarkLabel lblFolders = null!;
    private DarkUI.Controls.DarkListBox lstFolders = null!;
    private DarkUI.Controls.DarkButton btnAdd = null!;
    private DarkUI.Controls.DarkButton btnRemove = null!;
    private DarkUI.Controls.DarkLabel lblManualSources = null!;
    private DarkUI.Controls.DarkListBox lstManualSources = null!;
    private DarkUI.Controls.DarkButton btnRemoveSource = null!;
    private DarkUI.Controls.DarkCheckBox chkRecursive = null!;
    private DarkUI.Controls.DarkCheckBox chkRefreshOnStartup = null!;
    private DarkUI.Controls.DarkLabel lblLibraryNotice = null!;

    private DarkUI.Controls.DarkTabPage tabAppearance = null!;
    private DarkUI.Controls.DarkLabel lblAppearanceInfo = null!;
    private DarkUI.Controls.DarkLabel lblTheme = null!;
    private DarkUI.Controls.DarkComboBox cboTheme = null!;
    private DarkUI.Controls.DarkLabel lblRowHeight = null!;
    private DarkUI.Controls.DarkNumericUpDown nudRowHeight = null!;
    private DarkUI.Controls.DarkLabel lblDensity = null!;
    private DarkUI.Controls.DarkComboBox cboDensity = null!;
    private DarkUI.Controls.DarkCheckBox chkShowThumbnails = null!;
    private DarkUI.Controls.DarkCheckBox chkShowGridLines = null!;
    private DarkUI.Controls.DarkCheckBox chkShowFilePreview = null!;
    private DarkUI.Controls.DarkLabel lblDefaultGroup = null!;
    private DarkUI.Controls.DarkComboBox cboDefaultGroup = null!;
    private DarkUI.Controls.DarkButton btnResetLayout = null!;
    private DarkUI.Controls.DarkLabel lblAppearanceNotice = null!;

    private DarkUI.Controls.DarkTabPage tabNaming = null!;
    private DarkUI.Controls.DarkLabel lblNamingInfo = null!;
    private DarkUI.Controls.DarkLabel lblRenameFormat = null!;
    private DarkUI.Controls.DarkTextBox txtRenameFormat = null!;
    private DarkUI.Controls.DarkLabel lblRenamePreset = null!;
    private DarkUI.Controls.DarkComboBox cboRenamePreset = null!;
    private DarkUI.Controls.DarkLabel lblRenameToken = null!;
    private DarkUI.Controls.DarkComboBox cboRenameToken = null!;
    private DarkUI.Controls.DarkButton btnInsertToken = null!;
    private DarkUI.Controls.DarkLabel lblRenamePreview = null!;
    private DarkUI.Controls.DarkLabel lblRenameUnknown = null!;

    private DarkUI.Controls.DarkTabPage tabFiles = null!;
    private DarkUI.Controls.DarkLabel lblViewingInfo = null!;
    private DarkUI.Controls.DarkLabel lblMaxPreview = null!;
    private DarkUI.Controls.DarkNumericUpDown nudMaxPreviewMb = null!;
    private DarkUI.Controls.DarkLabel lblHexPage = null!;
    private DarkUI.Controls.DarkNumericUpDown nudHexPageKb = null!;
    private DarkUI.Controls.DarkLabel lblThumbCache = null!;
    private DarkUI.Controls.DarkNumericUpDown nudThumbnailCache = null!;
    private DarkUI.Controls.DarkLabel lblThumbCacheHint = null!;
    private DarkUI.Controls.DarkLabel lblViewingNotice = null!;

    private DarkUI.Controls.DarkTabPage tabPaths = null!;
    private DarkUI.Controls.DarkLabel lblPathsInfo = null!;
    private DarkUI.Controls.DarkLabel lblOutputDir = null!;
    private DarkUI.Controls.DarkTextBox txtOutputDirectory = null!;
    private DarkUI.Controls.DarkButton btnBrowseOutput = null!;
    private DarkUI.Controls.DarkLabel lblDefaultBackend = null!;
    private DarkUI.Controls.DarkComboBox cboDefaultBackend = null!;
    private DarkUI.Controls.DarkLabel lblPasscode = null!;
    private DarkUI.Controls.DarkTextBox txtDebugPasscode = null!;
    private DarkUI.Controls.DarkCheckBox chkShowPasscode = null!;
    private DarkUI.Controls.DarkLabel lblPasscodeHint = null!;
    private DarkUI.Controls.DarkCheckBox chkOpenOutputAfterTask = null!;
    private DarkUI.Controls.DarkLabel lblOutputNotice = null!;

    private DarkUI.Controls.DarkTabPage tabSafety = null!;
    private DarkUI.Controls.DarkLabel lblSafetyInfo = null!;
    private DarkUI.Controls.DarkCheckBox chkConfirmMove = null!;
    private DarkUI.Controls.DarkCheckBox chkConfirmDelete = null!;
    private DarkUI.Controls.DarkCheckBox chkPermanentDelete = null!;
    private DarkUI.Controls.DarkLabel lblPermanentNotice = null!;

    private DarkUI.Controls.DarkTabPage tabDiagnostics = null!;
    private DarkUI.Controls.DarkLabel lblMaintenanceInfo = null!;
    private DarkUI.Controls.DarkButton btnExportSettings = null!;
    private DarkUI.Controls.DarkCheckBox chkExportCredentials = null!;
    private DarkUI.Controls.DarkButton btnImportSettings = null!;
    private DarkUI.Controls.DarkButton btnResetSettings = null!;
    private DarkUI.Controls.DarkButton btnClearCaches = null!;
    private DarkUI.Controls.DarkButton btnOpenLogs = null!;
    private DarkUI.Controls.DarkLabel lblMaintenanceNotice = null!;

    private DarkUI.Controls.DarkButton btnSave = null!;
    private DarkUI.Controls.DarkButton btnCancel = null!;
    private FolderBrowserDialog folderBrowserDialog = null!;
    private SaveFileDialog exportSettingsDialog = null!;
    private OpenFileDialog importSettingsDialog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
        components = new System.ComponentModel.Container();
        tabsSettings = new DarkUI.Controls.DarkTabControl();
        tabLibrary = new DarkUI.Controls.DarkTabPage();
        lblLibraryInfo = new DarkUI.Controls.DarkLabel();
        lblFolders = new DarkUI.Controls.DarkLabel();
        lstFolders = new DarkUI.Controls.DarkListBox();
        btnAdd = new DarkUI.Controls.DarkButton();
        btnRemove = new DarkUI.Controls.DarkButton();
        lblManualSources = new DarkUI.Controls.DarkLabel();
        lstManualSources = new DarkUI.Controls.DarkListBox();
        btnRemoveSource = new DarkUI.Controls.DarkButton();
        chkRecursive = new DarkUI.Controls.DarkCheckBox();
        chkRefreshOnStartup = new DarkUI.Controls.DarkCheckBox();
        lblLibraryNotice = new DarkUI.Controls.DarkLabel();
        tabAppearance = new DarkUI.Controls.DarkTabPage();
        lblAppearanceInfo = new DarkUI.Controls.DarkLabel();
        lblTheme = new DarkUI.Controls.DarkLabel();
        cboTheme = new DarkUI.Controls.DarkComboBox();
        lblRowHeight = new DarkUI.Controls.DarkLabel();
        nudRowHeight = new DarkUI.Controls.DarkNumericUpDown();
        lblDensity = new DarkUI.Controls.DarkLabel();
        cboDensity = new DarkUI.Controls.DarkComboBox();
        chkShowThumbnails = new DarkUI.Controls.DarkCheckBox();
        chkShowGridLines = new DarkUI.Controls.DarkCheckBox();
        chkShowFilePreview = new DarkUI.Controls.DarkCheckBox();
        lblDefaultGroup = new DarkUI.Controls.DarkLabel();
        cboDefaultGroup = new DarkUI.Controls.DarkComboBox();
        btnResetLayout = new DarkUI.Controls.DarkButton();
        lblAppearanceNotice = new DarkUI.Controls.DarkLabel();
        tabNaming = new DarkUI.Controls.DarkTabPage();
        lblNamingInfo = new DarkUI.Controls.DarkLabel();
        lblRenameFormat = new DarkUI.Controls.DarkLabel();
        txtRenameFormat = new DarkUI.Controls.DarkTextBox();
        lblRenamePreset = new DarkUI.Controls.DarkLabel();
        cboRenamePreset = new DarkUI.Controls.DarkComboBox();
        lblRenameToken = new DarkUI.Controls.DarkLabel();
        cboRenameToken = new DarkUI.Controls.DarkComboBox();
        btnInsertToken = new DarkUI.Controls.DarkButton();
        lblRenamePreview = new DarkUI.Controls.DarkLabel();
        lblRenameUnknown = new DarkUI.Controls.DarkLabel();
        tabFiles = new DarkUI.Controls.DarkTabPage();
        lblViewingInfo = new DarkUI.Controls.DarkLabel();
        lblMaxPreview = new DarkUI.Controls.DarkLabel();
        nudMaxPreviewMb = new DarkUI.Controls.DarkNumericUpDown();
        lblHexPage = new DarkUI.Controls.DarkLabel();
        nudHexPageKb = new DarkUI.Controls.DarkNumericUpDown();
        lblThumbCache = new DarkUI.Controls.DarkLabel();
        nudThumbnailCache = new DarkUI.Controls.DarkNumericUpDown();
        lblThumbCacheHint = new DarkUI.Controls.DarkLabel();
        lblViewingNotice = new DarkUI.Controls.DarkLabel();
        tabPaths = new DarkUI.Controls.DarkTabPage();
        lblPathsInfo = new DarkUI.Controls.DarkLabel();
        lblOutputDir = new DarkUI.Controls.DarkLabel();
        txtOutputDirectory = new DarkUI.Controls.DarkTextBox();
        btnBrowseOutput = new DarkUI.Controls.DarkButton();
        lblDefaultBackend = new DarkUI.Controls.DarkLabel();
        cboDefaultBackend = new DarkUI.Controls.DarkComboBox();
        lblPasscode = new DarkUI.Controls.DarkLabel();
        txtDebugPasscode = new DarkUI.Controls.DarkTextBox();
        chkShowPasscode = new DarkUI.Controls.DarkCheckBox();
        lblPasscodeHint = new DarkUI.Controls.DarkLabel();
        chkOpenOutputAfterTask = new DarkUI.Controls.DarkCheckBox();
        lblOutputNotice = new DarkUI.Controls.DarkLabel();
        tabSafety = new DarkUI.Controls.DarkTabPage();
        lblSafetyInfo = new DarkUI.Controls.DarkLabel();
        chkConfirmMove = new DarkUI.Controls.DarkCheckBox();
        chkConfirmDelete = new DarkUI.Controls.DarkCheckBox();
        chkPermanentDelete = new DarkUI.Controls.DarkCheckBox();
        lblPermanentNotice = new DarkUI.Controls.DarkLabel();
        tabDiagnostics = new DarkUI.Controls.DarkTabPage();
        lblMaintenanceInfo = new DarkUI.Controls.DarkLabel();
        btnExportSettings = new DarkUI.Controls.DarkButton();
        chkExportCredentials = new DarkUI.Controls.DarkCheckBox();
        btnImportSettings = new DarkUI.Controls.DarkButton();
        btnResetSettings = new DarkUI.Controls.DarkButton();
        btnClearCaches = new DarkUI.Controls.DarkButton();
        btnOpenLogs = new DarkUI.Controls.DarkButton();
        lblMaintenanceNotice = new DarkUI.Controls.DarkLabel();
        btnSave = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        folderBrowserDialog = new FolderBrowserDialog();
        exportSettingsDialog = new SaveFileDialog();
        importSettingsDialog = new OpenFileDialog();
        tabsSettings.SuspendLayout();
        tabLibrary.SuspendLayout();
        tabAppearance.SuspendLayout();
        tabNaming.SuspendLayout();
        tabFiles.SuspendLayout();
        tabPaths.SuspendLayout();
        tabSafety.SuspendLayout();
        tabDiagnostics.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudRowHeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxPreviewMb).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudHexPageKb).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudThumbnailCache).BeginInit();
        SuspendLayout();
        // 
        // tabsSettings
        // 
        tabsSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabsSettings.Controls.Add(tabLibrary);
        tabsSettings.Controls.Add(tabAppearance);
        tabsSettings.Controls.Add(tabNaming);
        tabsSettings.Controls.Add(tabFiles);
        tabsSettings.Controls.Add(tabPaths);
        tabsSettings.Controls.Add(tabSafety);
        tabsSettings.Controls.Add(tabDiagnostics);
        tabsSettings.Location = new Point(12, 12);
        tabsSettings.Name = "tabsSettings";
        tabsSettings.SelectedIndex = 0;
        tabsSettings.Size = new Size(696, 392);
        tabsSettings.TabIndex = 0;
        // 
        // tabLibrary
        // 
        tabLibrary.BackColor = Color.FromArgb(60, 63, 65);
        tabLibrary.Controls.Add(lblLibraryInfo);
        tabLibrary.Controls.Add(lblFolders);
        tabLibrary.Controls.Add(lstFolders);
        tabLibrary.Controls.Add(btnAdd);
        tabLibrary.Controls.Add(btnRemove);
        tabLibrary.Controls.Add(lblManualSources);
        tabLibrary.Controls.Add(lstManualSources);
        tabLibrary.Controls.Add(btnRemoveSource);
        tabLibrary.Controls.Add(chkRecursive);
        tabLibrary.Controls.Add(chkRefreshOnStartup);
        tabLibrary.Controls.Add(lblLibraryNotice);
        tabLibrary.Location = new Point(4, 26);
        tabLibrary.Name = "tabLibrary";
        tabLibrary.Size = new Size(688, 362);
        tabLibrary.TabIndex = 0;
        tabLibrary.Text = "Library";
        // 
        // lblLibraryInfo
        // 
        lblLibraryInfo.Location = new Point(16, 14);
        lblLibraryInfo.Name = "lblLibraryInfo";
        lblLibraryInfo.Size = new Size(656, 15);
        lblLibraryInfo.TabIndex = 0;
        lblLibraryInfo.Text = "Folders scanned for dumps, packages and images, plus sources added individually.";
        // 
        // lblFolders
        // 
        lblFolders.Location = new Point(16, 40);
        lblFolders.Name = "lblFolders";
        lblFolders.Size = new Size(380, 15);
        lblFolders.TabIndex = 1;
        lblFolders.Text = "Library folders";
        // 
        // lstFolders
        // 
        lstFolders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstFolders.DrawMode = DrawMode.OwnerDrawFixed;
        lstFolders.FormattingEnabled = true;
        lstFolders.ItemHeight = 20;
        lstFolders.Location = new Point(16, 58);
        lstFolders.Name = "lstFolders";
        lstFolders.SelectionMode = SelectionMode.MultiExtended;
        lstFolders.Size = new Size(380, 174);
        lstFolders.TabIndex = 2;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(16, 240);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(120, 30);
        btnAdd.TabIndex = 3;
        btnAdd.Text = "Add Folder...";
        btnAdd.Click += btnAdd_Click;
        // 
        // btnRemove
        // 
        btnRemove.Location = new Point(142, 240);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new Size(140, 30);
        btnRemove.TabIndex = 4;
        btnRemove.Text = "Remove from Library";
        btnRemove.Click += btnRemove_Click;
        // 
        // lblManualSources
        // 
        lblManualSources.Location = new Point(420, 40);
        lblManualSources.Name = "lblManualSources";
        lblManualSources.Size = new Size(252, 15);
        lblManualSources.TabIndex = 5;
        lblManualSources.Text = "Manually added sources";
        // 
        // lstManualSources
        // 
        lstManualSources.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        lstManualSources.DrawMode = DrawMode.OwnerDrawFixed;
        lstManualSources.FormattingEnabled = true;
        lstManualSources.ItemHeight = 20;
        lstManualSources.Location = new Point(420, 58);
        lstManualSources.Name = "lstManualSources";
        lstManualSources.SelectionMode = SelectionMode.MultiExtended;
        lstManualSources.Size = new Size(252, 174);
        lstManualSources.TabIndex = 6;
        // 
        // btnRemoveSource
        // 
        btnRemoveSource.Location = new Point(420, 240);
        btnRemoveSource.Name = "btnRemoveSource";
        btnRemoveSource.Size = new Size(180, 30);
        btnRemoveSource.TabIndex = 7;
        btnRemoveSource.Text = "Forget Source";
        btnRemoveSource.Click += btnRemoveSource_Click;
        // 
        // chkRecursive
        // 
        chkRecursive.AutoSize = true;
        chkRecursive.Location = new Point(16, 282);
        chkRecursive.Name = "chkRecursive";
        chkRecursive.Size = new Size(300, 19);
        chkRecursive.TabIndex = 8;
        chkRecursive.Text = "Scan subfolders (dumps, packages and images)";
        // 
        // chkRefreshOnStartup
        // 
        chkRefreshOnStartup.AutoSize = true;
        chkRefreshOnStartup.Location = new Point(16, 306);
        chkRefreshOnStartup.Name = "chkRefreshOnStartup";
        chkRefreshOnStartup.Size = new Size(300, 19);
        chkRefreshOnStartup.TabIndex = 9;
        chkRefreshOnStartup.Text = "Refresh library on startup (rescans every launch)";
        // 
        // lblLibraryNotice
        // 
        lblLibraryNotice.Location = new Point(16, 330);
        lblLibraryNotice.Name = "lblLibraryNotice";
        lblLibraryNotice.Size = new Size(656, 15);
        lblLibraryNotice.TabIndex = 10;
        lblLibraryNotice.Text = "Changing folders or Scan subfolders rescans the library when you save. Removing a folder never deletes files.";
        // 
        // tabAppearance
        // 
        tabAppearance.BackColor = Color.FromArgb(60, 63, 65);
        tabAppearance.Controls.Add(lblAppearanceInfo);
        tabAppearance.Controls.Add(lblTheme);
        tabAppearance.Controls.Add(cboTheme);
        tabAppearance.Controls.Add(lblRowHeight);
        tabAppearance.Controls.Add(nudRowHeight);
        tabAppearance.Controls.Add(lblDensity);
        tabAppearance.Controls.Add(cboDensity);
        tabAppearance.Controls.Add(chkShowThumbnails);
        tabAppearance.Controls.Add(chkShowGridLines);
        tabAppearance.Controls.Add(chkShowFilePreview);
        tabAppearance.Controls.Add(lblDefaultGroup);
        tabAppearance.Controls.Add(cboDefaultGroup);
        tabAppearance.Controls.Add(btnResetLayout);
        tabAppearance.Controls.Add(lblAppearanceNotice);
        tabAppearance.Location = new Point(4, 26);
        tabAppearance.Name = "tabAppearance";
        tabAppearance.Size = new Size(688, 362);
        tabAppearance.TabIndex = 1;
        tabAppearance.Text = "Appearance";
        // 
        // lblAppearanceInfo
        // 
        lblAppearanceInfo.Location = new Point(16, 14);
        lblAppearanceInfo.Name = "lblAppearanceInfo";
        lblAppearanceInfo.Size = new Size(656, 15);
        lblAppearanceInfo.TabIndex = 0;
        lblAppearanceInfo.Text = "Theme and library grid display. Visual preferences apply when you save.";
        // 
        // lblTheme
        // 
        lblTheme.Location = new Point(16, 49);
        lblTheme.Name = "lblTheme";
        lblTheme.Size = new Size(170, 15);
        lblTheme.TabIndex = 1;
        lblTheme.Text = "Theme (live preview)";
        // 
        // cboTheme
        // 
        cboTheme.DropDownStyle = ComboBoxStyle.DropDownList;
        cboTheme.Location = new Point(196, 45);
        cboTheme.Name = "cboTheme";
        cboTheme.Size = new Size(320, 23);
        cboTheme.TabIndex = 2;
        cboTheme.SelectedIndexChanged += cboTheme_SelectedIndexChanged;
        // 
        // lblRowHeight
        // 
        lblRowHeight.Location = new Point(16, 83);
        lblRowHeight.Name = "lblRowHeight";
        lblRowHeight.Size = new Size(170, 15);
        lblRowHeight.TabIndex = 3;
        lblRowHeight.Text = "Grid row height (px)";
        // 
        // nudRowHeight
        // 
        nudRowHeight.Location = new Point(196, 79);
        nudRowHeight.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        nudRowHeight.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
        nudRowHeight.Name = "nudRowHeight";
        nudRowHeight.Size = new Size(80, 23);
        nudRowHeight.TabIndex = 4;
        nudRowHeight.Value = new decimal(new int[] { 22, 0, 0, 0 });
        // 
        // lblDensity
        // 
        lblDensity.Location = new Point(300, 83);
        lblDensity.Name = "lblDensity";
        lblDensity.Size = new Size(90, 15);
        lblDensity.TabIndex = 5;
        lblDensity.Text = "Density";
        // 
        // cboDensity
        // 
        cboDensity.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDensity.Location = new Point(396, 79);
        cboDensity.Name = "cboDensity";
        cboDensity.Size = new Size(160, 23);
        cboDensity.TabIndex = 6;
        // 
        // chkShowThumbnails
        // 
        chkShowThumbnails.AutoSize = true;
        chkShowThumbnails.Location = new Point(16, 118);
        chkShowThumbnails.Name = "chkShowThumbnails";
        chkShowThumbnails.Size = new Size(260, 19);
        chkShowThumbnails.TabIndex = 7;
        chkShowThumbnails.Text = "Show game thumbnails in the list";
        // 
        // chkShowGridLines
        // 
        chkShowGridLines.AutoSize = true;
        chkShowGridLines.Location = new Point(16, 146);
        chkShowGridLines.Name = "chkShowGridLines";
        chkShowGridLines.Size = new Size(260, 19);
        chkShowGridLines.TabIndex = 8;
        chkShowGridLines.Text = "Show grid lines";
        // 
        // chkShowFilePreview
        // 
        chkShowFilePreview.AutoSize = true;
        chkShowFilePreview.Location = new Point(16, 170);
        chkShowFilePreview.Name = "chkShowFilePreview";
        chkShowFilePreview.Size = new Size(400, 19);
        chkShowFilePreview.TabIndex = 13;
        chkShowFilePreview.Text = "Show the file preview pane in the Files tab";
        // 
        // lblDefaultGroup
        // 
        lblDefaultGroup.Location = new Point(16, 205);
        lblDefaultGroup.Name = "lblDefaultGroup";
        lblDefaultGroup.Size = new Size(170, 15);
        lblDefaultGroup.TabIndex = 7;
        lblDefaultGroup.Text = "Default grouping";
        // 
        // cboDefaultGroup
        // 
        cboDefaultGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDefaultGroup.Location = new Point(196, 201);
        cboDefaultGroup.Name = "cboDefaultGroup";
        cboDefaultGroup.Size = new Size(320, 23);
        cboDefaultGroup.TabIndex = 10;
        // 
        // btnResetLayout
        // 
        btnResetLayout.Location = new Point(196, 234);
        btnResetLayout.Name = "btnResetLayout";
        btnResetLayout.Size = new Size(220, 30);
        btnResetLayout.TabIndex = 11;
        btnResetLayout.Text = "Reset Column Layout (on Save)";
        btnResetLayout.Click += btnResetLayout_Click;
        // 
        // lblAppearanceNotice
        // 
        lblAppearanceNotice.Location = new Point(16, 330);
        lblAppearanceNotice.Name = "lblAppearanceNotice";
        lblAppearanceNotice.Size = new Size(656, 15);
        lblAppearanceNotice.TabIndex = 12;
        lblAppearanceNotice.Text = "The default grouping is applied to the open library when you save.";
        // 
        // tabNaming
        // 
        tabNaming.BackColor = Color.FromArgb(60, 63, 65);
        tabNaming.Controls.Add(lblNamingInfo);
        tabNaming.Controls.Add(lblRenameFormat);
        tabNaming.Controls.Add(txtRenameFormat);
        tabNaming.Controls.Add(lblRenamePreset);
        tabNaming.Controls.Add(cboRenamePreset);
        tabNaming.Controls.Add(lblRenameToken);
        tabNaming.Controls.Add(cboRenameToken);
        tabNaming.Controls.Add(btnInsertToken);
        tabNaming.Controls.Add(lblRenamePreview);
        tabNaming.Controls.Add(lblRenameUnknown);
        tabNaming.Location = new Point(4, 26);
        tabNaming.Name = "tabNaming";
        tabNaming.Size = new Size(688, 362);
        tabNaming.TabIndex = 2;
        tabNaming.Text = "Naming";
        // 
        // lblNamingInfo
        // 
        lblNamingInfo.Location = new Point(16, 14);
        lblNamingInfo.Name = "lblNamingInfo";
        lblNamingInfo.Size = new Size(656, 15);
        lblNamingInfo.TabIndex = 0;
        lblNamingInfo.Text = "Default format used by the custom rename option.";
        // 
        // lblRenameFormat
        // 
        lblRenameFormat.Location = new Point(16, 49);
        lblRenameFormat.Name = "lblRenameFormat";
        lblRenameFormat.Size = new Size(170, 15);
        lblRenameFormat.TabIndex = 1;
        lblRenameFormat.Text = "Rename format";
        // 
        // txtRenameFormat
        // 
        txtRenameFormat.Location = new Point(196, 45);
        txtRenameFormat.Name = "txtRenameFormat";
        txtRenameFormat.PlaceholderText = "{TITLE} [{TITLE_ID}]";
        txtRenameFormat.Size = new Size(460, 23);
        txtRenameFormat.TabIndex = 2;
        txtRenameFormat.TextChanged += txtRenameFormat_TextChanged;
        // 
        // lblRenamePreset
        // 
        lblRenamePreset.Location = new Point(16, 87);
        lblRenamePreset.Name = "lblRenamePreset";
        lblRenamePreset.Size = new Size(170, 15);
        lblRenamePreset.TabIndex = 3;
        lblRenamePreset.Text = "Start from a preset";
        // 
        // cboRenamePreset
        // 
        cboRenamePreset.DropDownStyle = ComboBoxStyle.DropDownList;
        cboRenamePreset.Location = new Point(196, 83);
        cboRenamePreset.Name = "cboRenamePreset";
        cboRenamePreset.Size = new Size(460, 23);
        cboRenamePreset.TabIndex = 4;
        cboRenamePreset.SelectedIndexChanged += cboRenamePreset_SelectedIndexChanged;
        // 
        // lblRenameToken
        // 
        lblRenameToken.Location = new Point(16, 125);
        lblRenameToken.Name = "lblRenameToken";
        lblRenameToken.Size = new Size(170, 15);
        lblRenameToken.TabIndex = 5;
        lblRenameToken.Text = "Insert a token";
        // 
        // cboRenameToken
        // 
        cboRenameToken.DropDownStyle = ComboBoxStyle.DropDownList;
        cboRenameToken.Location = new Point(196, 121);
        cboRenameToken.Name = "cboRenameToken";
        cboRenameToken.Size = new Size(300, 23);
        cboRenameToken.TabIndex = 6;
        // 
        // btnInsertToken
        // 
        btnInsertToken.Location = new Point(504, 120);
        btnInsertToken.Name = "btnInsertToken";
        btnInsertToken.Size = new Size(152, 26);
        btnInsertToken.TabIndex = 7;
        btnInsertToken.Text = "Insert";
        btnInsertToken.Click += btnInsertToken_Click;
        // 
        // lblRenamePreview
        // 
        lblRenamePreview.Location = new Point(16, 168);
        lblRenamePreview.Name = "lblRenamePreview";
        lblRenamePreview.Size = new Size(656, 15);
        lblRenamePreview.TabIndex = 8;
        lblRenamePreview.Text = "Example: ...";
        // 
        // lblRenameUnknown
        // 
        lblRenameUnknown.Location = new Point(16, 192);
        lblRenameUnknown.Name = "lblRenameUnknown";
        lblRenameUnknown.Size = new Size(656, 30);
        lblRenameUnknown.TabIndex = 9;
        lblRenameUnknown.Text = "";
        // 
        // tabFiles
        // 
        tabFiles.BackColor = Color.FromArgb(60, 63, 65);
        tabFiles.Controls.Add(lblViewingInfo);
        tabFiles.Controls.Add(lblMaxPreview);
        tabFiles.Controls.Add(nudMaxPreviewMb);
        tabFiles.Controls.Add(lblHexPage);
        tabFiles.Controls.Add(nudHexPageKb);
        tabFiles.Controls.Add(lblThumbCache);
        tabFiles.Controls.Add(nudThumbnailCache);
        tabFiles.Controls.Add(lblThumbCacheHint);
        tabFiles.Controls.Add(lblViewingNotice);
        tabFiles.Location = new Point(4, 26);
        tabFiles.Name = "tabFiles";
        tabFiles.Size = new Size(688, 362);
        tabFiles.TabIndex = 3;
        tabFiles.Text = "Viewing && Cache";
        // 
        // lblViewingInfo
        // 
        lblViewingInfo.Location = new Point(16, 14);
        lblViewingInfo.Name = "lblViewingInfo";
        lblViewingInfo.Size = new Size(656, 15);
        lblViewingInfo.TabIndex = 0;
        lblViewingInfo.Text = "Preview limits and thumbnail cache. Limits bound what is loaded for a preview, not total memory.";
        // 
        // lblMaxPreview
        // 
        lblMaxPreview.Location = new Point(16, 49);
        lblMaxPreview.Name = "lblMaxPreview";
        lblMaxPreview.Size = new Size(170, 15);
        lblMaxPreview.TabIndex = 1;
        lblMaxPreview.Text = "Max auto-preview (MiB)";
        // 
        // nudMaxPreviewMb
        // 
        nudMaxPreviewMb.Location = new Point(196, 45);
        nudMaxPreviewMb.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
        nudMaxPreviewMb.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudMaxPreviewMb.Name = "nudMaxPreviewMb";
        nudMaxPreviewMb.Size = new Size(80, 23);
        nudMaxPreviewMb.TabIndex = 2;
        nudMaxPreviewMb.Value = new decimal(new int[] { 16, 0, 0, 0 });
        // 
        // lblHexPage
        // 
        lblHexPage.Location = new Point(16, 83);
        lblHexPage.Name = "lblHexPage";
        lblHexPage.Size = new Size(170, 15);
        lblHexPage.TabIndex = 3;
        lblHexPage.Text = "Hex page size (KiB)";
        // 
        // nudHexPageKb
        // 
        nudHexPageKb.Location = new Point(196, 79);
        nudHexPageKb.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
        nudHexPageKb.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudHexPageKb.Name = "nudHexPageKb";
        nudHexPageKb.Size = new Size(80, 23);
        nudHexPageKb.TabIndex = 4;
        nudHexPageKb.Value = new decimal(new int[] { 16, 0, 0, 0 });
        // 
        // lblThumbCache
        // 
        lblThumbCache.Location = new Point(16, 117);
        lblThumbCache.Name = "lblThumbCache";
        lblThumbCache.Size = new Size(170, 15);
        lblThumbCache.TabIndex = 5;
        lblThumbCache.Text = "Thumbnail cache entries";
        // 
        // nudThumbnailCache
        // 
        nudThumbnailCache.Location = new Point(196, 113);
        nudThumbnailCache.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        nudThumbnailCache.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        nudThumbnailCache.Name = "nudThumbnailCache";
        nudThumbnailCache.Size = new Size(80, 23);
        nudThumbnailCache.TabIndex = 6;
        nudThumbnailCache.Value = new decimal(new int[] { 512, 0, 0, 0 });
        // 
        // lblThumbCacheHint
        // 
        lblThumbCacheHint.Location = new Point(286, 117);
        lblThumbCacheHint.Name = "lblThumbCacheHint";
        lblThumbCacheHint.Size = new Size(386, 15);
        lblThumbCacheHint.TabIndex = 7;
        lblThumbCacheHint.Text = "Minimum 1. Older thumbnails are evicted first.";
        // 
        // lblViewingNotice
        // 
        lblViewingNotice.Location = new Point(16, 330);
        lblViewingNotice.Name = "lblViewingNotice";
        lblViewingNotice.Size = new Size(656, 15);
        lblViewingNotice.TabIndex = 8;
        lblViewingNotice.Text = "Hex page size applies the next time a page is read. Cache entries are decoded images held in memory.";
        // 
        // tabPaths
        // 
        tabPaths.BackColor = Color.FromArgb(60, 63, 65);
        tabPaths.Controls.Add(lblPathsInfo);
        tabPaths.Controls.Add(lblOutputDir);
        tabPaths.Controls.Add(txtOutputDirectory);
        tabPaths.Controls.Add(btnBrowseOutput);
        tabPaths.Controls.Add(lblDefaultBackend);
        tabPaths.Controls.Add(cboDefaultBackend);
        tabPaths.Controls.Add(lblPasscode);
        tabPaths.Controls.Add(txtDebugPasscode);
        tabPaths.Controls.Add(chkShowPasscode);
        tabPaths.Controls.Add(lblPasscodeHint);
        tabPaths.Controls.Add(chkOpenOutputAfterTask);
        tabPaths.Controls.Add(lblOutputNotice);
        tabPaths.Location = new Point(4, 26);
        tabPaths.Name = "tabPaths";
        tabPaths.Size = new Size(688, 362);
        tabPaths.TabIndex = 4;
        tabPaths.Text = "Output && Defaults";
        // 
        // lblPathsInfo
        // 
        lblPathsInfo.Location = new Point(16, 14);
        lblPathsInfo.Name = "lblPathsInfo";
        lblPathsInfo.Size = new Size(656, 15);
        lblPathsInfo.TabIndex = 0;
        lblPathsInfo.Text = "Defaults for output, the builder and package credentials. New jobs inherit these; existing jobs keep their own values.";
        // 
        // lblOutputDir
        // 
        lblOutputDir.Location = new Point(16, 49);
        lblOutputDir.Name = "lblOutputDir";
        lblOutputDir.Size = new Size(170, 15);
        lblOutputDir.TabIndex = 1;
        lblOutputDir.Text = "Default output folder";
        // 
        // txtOutputDirectory
        // 
        txtOutputDirectory.Location = new Point(196, 45);
        txtOutputDirectory.Name = "txtOutputDirectory";
        txtOutputDirectory.Size = new Size(380, 23);
        txtOutputDirectory.TabIndex = 2;
        // 
        // btnBrowseOutput
        // 
        btnBrowseOutput.Location = new Point(584, 44);
        btnBrowseOutput.Name = "btnBrowseOutput";
        btnBrowseOutput.Size = new Size(80, 26);
        btnBrowseOutput.TabIndex = 3;
        btnBrowseOutput.Text = "Browse...";
        btnBrowseOutput.Click += btnBrowseOutput_Click;
        // 
        // lblDefaultBackend
        // 
        lblDefaultBackend.Location = new Point(16, 87);
        lblDefaultBackend.Name = "lblDefaultBackend";
        lblDefaultBackend.Size = new Size(170, 15);
        lblDefaultBackend.TabIndex = 4;
        lblDefaultBackend.Text = "Default builder";
        // 
        // cboDefaultBackend
        // 
        cboDefaultBackend.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDefaultBackend.Location = new Point(196, 83);
        cboDefaultBackend.Name = "cboDefaultBackend";
        cboDefaultBackend.Size = new Size(320, 23);
        cboDefaultBackend.TabIndex = 5;
        // 
        // lblPasscode
        // 
        lblPasscode.Location = new Point(16, 125);
        lblPasscode.Name = "lblPasscode";
        lblPasscode.Size = new Size(170, 15);
        lblPasscode.TabIndex = 6;
        lblPasscode.Text = "Default debug passcode";
        // 
        // txtDebugPasscode
        // 
        txtDebugPasscode.Location = new Point(196, 121);
        txtDebugPasscode.Name = "txtDebugPasscode";
        txtDebugPasscode.Size = new Size(280, 23);
        txtDebugPasscode.TabIndex = 7;
        txtDebugPasscode.UseSystemPasswordChar = true;
        txtDebugPasscode.TextChanged += txtDebugPasscode_TextChanged;
        // 
        // chkShowPasscode
        // 
        chkShowPasscode.AutoSize = true;
        chkShowPasscode.Location = new Point(486, 123);
        chkShowPasscode.Name = "chkShowPasscode";
        chkShowPasscode.Size = new Size(60, 19);
        chkShowPasscode.TabIndex = 8;
        chkShowPasscode.Text = "Show";
        chkShowPasscode.CheckedChanged += chkShowPasscode_CheckedChanged;
        // 
        // lblPasscodeHint
        // 
        lblPasscodeHint.Location = new Point(196, 150);
        lblPasscodeHint.Name = "lblPasscodeHint";
        lblPasscodeHint.Size = new Size(470, 30);
        lblPasscodeHint.TabIndex = 9;
        lblPasscodeHint.Text = "Blank uses the default all-zero passcode. Otherwise exactly 32 printable ASCII characters.";
        // 
        // chkOpenOutputAfterTask
        // 
        chkOpenOutputAfterTask.AutoSize = true;
        chkOpenOutputAfterTask.Location = new Point(16, 192);
        chkOpenOutputAfterTask.Name = "chkOpenOutputAfterTask";
        chkOpenOutputAfterTask.Size = new Size(400, 19);
        chkOpenOutputAfterTask.TabIndex = 10;
        chkOpenOutputAfterTask.Text = "Open the output folder after a task succeeds";
        // 
        // lblOutputNotice
        // 
        lblOutputNotice.Location = new Point(16, 330);
        lblOutputNotice.Name = "lblOutputNotice";
        lblOutputNotice.Size = new Size(656, 15);
        lblOutputNotice.TabIndex = 11;
        lblOutputNotice.Text = "The output folder also seeds the Move and Save artwork dialogs. Builder changes apply to new jobs.";
        // 
        // tabSafety
        // 
        tabSafety.BackColor = Color.FromArgb(60, 63, 65);
        tabSafety.Controls.Add(lblSafetyInfo);
        tabSafety.Controls.Add(chkConfirmMove);
        tabSafety.Controls.Add(chkConfirmDelete);
        tabSafety.Controls.Add(chkPermanentDelete);
        tabSafety.Controls.Add(lblPermanentNotice);
        tabSafety.Location = new Point(4, 26);
        tabSafety.Name = "tabSafety";
        tabSafety.Size = new Size(688, 362);
        tabSafety.TabIndex = 5;
        tabSafety.Text = "File Operations";
        // 
        // lblSafetyInfo
        // 
        lblSafetyInfo.Location = new Point(16, 14);
        lblSafetyInfo.Name = "lblSafetyInfo";
        lblSafetyInfo.Size = new Size(656, 15);
        lblSafetyInfo.TabIndex = 0;
        lblSafetyInfo.Text = "Confirmation prompts and deletion mode for sources in the library.";
        // 
        // chkConfirmMove
        // 
        chkConfirmMove.AutoSize = true;
        chkConfirmMove.Location = new Point(16, 49);
        chkConfirmMove.Name = "chkConfirmMove";
        chkConfirmMove.Size = new Size(400, 19);
        chkConfirmMove.TabIndex = 1;
        chkConfirmMove.Text = "Confirm before moving sources on disk";
        // 
        // chkConfirmDelete
        // 
        chkConfirmDelete.AutoSize = true;
        chkConfirmDelete.Location = new Point(16, 77);
        chkConfirmDelete.Name = "chkConfirmDelete";
        chkConfirmDelete.Size = new Size(400, 19);
        chkConfirmDelete.TabIndex = 2;
        chkConfirmDelete.Text = "Confirm before sending a source to the Recycle Bin";
        // 
        // chkPermanentDelete
        // 
        chkPermanentDelete.AutoSize = true;
        chkPermanentDelete.Location = new Point(16, 105);
        chkPermanentDelete.Name = "chkPermanentDelete";
        chkPermanentDelete.Size = new Size(400, 19);
        chkPermanentDelete.TabIndex = 3;
        chkPermanentDelete.Text = "Delete permanently instead of sending to the Recycle Bin";
        // 
        // lblPermanentNotice
        // 
        lblPermanentNotice.Location = new Point(16, 132);
        lblPermanentNotice.Name = "lblPermanentNotice";
        lblPermanentNotice.Size = new Size(656, 30);
        lblPermanentNotice.TabIndex = 4;
        lblPermanentNotice.Text = "Permanent deletion always asks for confirmation, even when the Recycle Bin prompt is turned off.";
        // 
        // tabDiagnostics
        // 
        tabDiagnostics.BackColor = Color.FromArgb(60, 63, 65);
        tabDiagnostics.Controls.Add(lblMaintenanceInfo);
        tabDiagnostics.Controls.Add(btnExportSettings);
        tabDiagnostics.Controls.Add(chkExportCredentials);
        tabDiagnostics.Controls.Add(btnImportSettings);
        tabDiagnostics.Controls.Add(btnResetSettings);
        tabDiagnostics.Controls.Add(btnClearCaches);
        tabDiagnostics.Controls.Add(btnOpenLogs);
        tabDiagnostics.Controls.Add(lblMaintenanceNotice);
        tabDiagnostics.Location = new Point(4, 26);
        tabDiagnostics.Name = "tabDiagnostics";
        tabDiagnostics.Size = new Size(688, 362);
        tabDiagnostics.TabIndex = 6;
        tabDiagnostics.Text = "Maintenance";
        // 
        // lblMaintenanceInfo
        // 
        lblMaintenanceInfo.Location = new Point(16, 14);
        lblMaintenanceInfo.Name = "lblMaintenanceInfo";
        lblMaintenanceInfo.Size = new Size(656, 15);
        lblMaintenanceInfo.TabIndex = 0;
        lblMaintenanceInfo.Text = "Back up, restore or clear application data.";
        // 
        // btnExportSettings
        // 
        btnExportSettings.Location = new Point(16, 48);
        btnExportSettings.Name = "btnExportSettings";
        btnExportSettings.Size = new Size(220, 30);
        btnExportSettings.TabIndex = 1;
        btnExportSettings.Text = "Export Preferences...";
        btnExportSettings.Click += btnExportSettings_Click;
        // 
        // chkExportCredentials
        // 
        chkExportCredentials.AutoSize = true;
        chkExportCredentials.Location = new Point(250, 54);
        chkExportCredentials.Name = "chkExportCredentials";
        chkExportCredentials.Size = new Size(300, 19);
        chkExportCredentials.TabIndex = 2;
        chkExportCredentials.Text = "Include the debug passcode (private backup)";
        // 
        // btnImportSettings
        // 
        btnImportSettings.Location = new Point(16, 84);
        btnImportSettings.Name = "btnImportSettings";
        btnImportSettings.Size = new Size(220, 30);
        btnImportSettings.TabIndex = 3;
        btnImportSettings.Text = "Import Preferences...";
        btnImportSettings.Click += btnImportSettings_Click;
        // 
        // btnResetSettings
        // 
        btnResetSettings.Location = new Point(16, 120);
        btnResetSettings.Name = "btnResetSettings";
        btnResetSettings.Size = new Size(220, 30);
        btnResetSettings.TabIndex = 4;
        btnResetSettings.Text = "Reset Preferences...";
        btnResetSettings.Click += btnResetSettings_Click;
        // 
        // btnClearCaches
        // 
        btnClearCaches.Location = new Point(16, 156);
        btnClearCaches.Name = "btnClearCaches";
        btnClearCaches.Size = new Size(220, 30);
        btnClearCaches.TabIndex = 5;
        btnClearCaches.Text = "Clear Caches (on Save)";
        btnClearCaches.Click += btnClearCaches_Click;
        // 
        // btnOpenLogs
        // 
        btnOpenLogs.Location = new Point(16, 192);
        btnOpenLogs.Name = "btnOpenLogs";
        btnOpenLogs.Size = new Size(220, 30);
        btnOpenLogs.TabIndex = 6;
        btnOpenLogs.Text = "Open Log Folder";
        btnOpenLogs.Click += btnOpenLogs_Click;
        // 
        // lblMaintenanceNotice
        // 
        lblMaintenanceNotice.Location = new Point(16, 236);
        lblMaintenanceNotice.Name = "lblMaintenanceNotice";
        lblMaintenanceNotice.Size = new Size(656, 60);
        lblMaintenanceNotice.TabIndex = 7;
        lblMaintenanceNotice.Text = "Export and Import cover preferences; library folders, sources and window layout are included. Open Log Folder is immediate. Clear Caches takes effect when you save; Cancel discards it.";
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSave.Location = new Point(478, 416);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(110, 32);
        btnSave.TabIndex = 1;
        btnSave.Text = "Save";
        btnSave.Click += btnSave_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(598, 416);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(110, 32);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        // 
        // SettingsForm
        // 
        AcceptButton = btnSave;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(60, 63, 65);
        CancelButton = btnCancel;
        ClientSize = new Size(720, 460);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(tabsSettings);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(0, 0);
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "PS5 PKG Tool Settings";
        tabsSettings.ResumeLayout(false);
        tabLibrary.ResumeLayout(false);
        tabLibrary.PerformLayout();
        tabAppearance.ResumeLayout(false);
        tabAppearance.PerformLayout();
        tabNaming.ResumeLayout(false);
        tabNaming.PerformLayout();
        tabFiles.ResumeLayout(false);
        tabFiles.PerformLayout();
        tabPaths.ResumeLayout(false);
        tabPaths.PerformLayout();
        tabSafety.ResumeLayout(false);
        tabSafety.PerformLayout();
        tabDiagnostics.ResumeLayout(false);
        tabDiagnostics.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudRowHeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxPreviewMb).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudHexPageKb).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudThumbnailCache).EndInit();
        ResumeLayout(false);
    }
}
