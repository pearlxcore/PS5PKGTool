#nullable enable

namespace PS5PKGTool.Forms;

partial class SettingsForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkTabControl tabsSettings = null!;
    private DarkUI.Controls.DarkTabPage tabLibrary = null!;
    private DarkUI.Controls.DarkLabel lblLibraryInfo = null!;
    private DarkUI.Controls.DarkListBox lstFolders = null!;
    private DarkUI.Controls.DarkButton btnAdd = null!;
    private DarkUI.Controls.DarkButton btnRemove = null!;
    private DarkUI.Controls.DarkTabPage tabAppearance = null!;
    private DarkUI.Controls.DarkTabPage tabFiles = null!;
    private DarkUI.Controls.DarkTabPage tabPaths = null!;
    private DarkUI.Controls.DarkTabPage tabSafety = null!;
    private DarkUI.Controls.DarkTabPage tabDiagnostics = null!;
    private DarkUI.Controls.DarkCheckBox chkRefreshOnStartup = null!;
    private DarkUI.Controls.DarkCheckBox chkRecursive = null!;
    private DarkUI.Controls.DarkLabel lblRenameFormat = null!;
    private DarkUI.Controls.DarkTextBox txtRenameFormat = null!;
    private DarkUI.Controls.DarkLabel lblRenameTokens = null!;
    private DarkUI.Controls.DarkLabel lblRenameTokens2 = null!;
    private DarkUI.Controls.DarkLabel lblAppearanceInfo = null!;
    private DarkUI.Controls.DarkLabel lblTheme = null!;
    private DarkUI.Controls.DarkComboBox cboTheme = null!;
    private DarkUI.Controls.DarkLabel lblRowHeight = null!;
    private DarkUI.Controls.DarkNumericUpDown nudRowHeight = null!;
    private DarkUI.Controls.DarkLabel lblDensity = null!;
    private DarkUI.Controls.DarkComboBox cboDensity = null!;
    private DarkUI.Controls.DarkCheckBox chkShowThumbnails = null!;
    private DarkUI.Controls.DarkCheckBox chkShowGridLines = null!;
    private DarkUI.Controls.DarkLabel lblDefaultGroup = null!;
    private DarkUI.Controls.DarkComboBox cboDefaultGroup = null!;
    private DarkUI.Controls.DarkButton btnResetLayout = null!;
    private DarkUI.Controls.DarkLabel lblViewingInfo = null!;
    private DarkUI.Controls.DarkLabel lblMaxPreview = null!;
    private DarkUI.Controls.DarkNumericUpDown nudMaxPreviewMb = null!;
    private DarkUI.Controls.DarkLabel lblHexPage = null!;
    private DarkUI.Controls.DarkNumericUpDown nudHexPageKb = null!;
    private DarkUI.Controls.DarkLabel lblThumbCache = null!;
    private DarkUI.Controls.DarkNumericUpDown nudThumbnailCache = null!;
    private DarkUI.Controls.DarkLabel lblPathsInfo = null!;
    private DarkUI.Controls.DarkLabel lblOutputDir = null!;
    private DarkUI.Controls.DarkTextBox txtOutputDirectory = null!;
    private DarkUI.Controls.DarkButton btnBrowseOutput = null!;
    private DarkUI.Controls.DarkCheckBox chkOpenOutputAfterTask = null!;
    private DarkUI.Controls.DarkLabel lblPasscode = null!;
    private DarkUI.Controls.DarkTextBox txtDebugPasscode = null!;
    private DarkUI.Controls.DarkCheckBox chkShowPasscode = null!;
    private DarkUI.Controls.DarkLabel lblPasscodeHint = null!;
    private DarkUI.Controls.DarkLabel lblSafetyInfo = null!;
    private DarkUI.Controls.DarkCheckBox chkConfirmDelete = null!;
    private DarkUI.Controls.DarkCheckBox chkConfirmMove = null!;
    private DarkUI.Controls.DarkCheckBox chkPermanentDelete = null!;
    private DarkUI.Controls.DarkLabel lblMaintenanceInfo = null!;
    private DarkUI.Controls.DarkButton btnExportSettings = null!;
    private DarkUI.Controls.DarkButton btnImportSettings = null!;
    private DarkUI.Controls.DarkButton btnResetSettings = null!;
    private DarkUI.Controls.DarkButton btnClearCaches = null!;
    private DarkUI.Controls.DarkButton btnOpenLogs = null!;
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
        lstFolders = new DarkUI.Controls.DarkListBox();
        btnAdd = new DarkUI.Controls.DarkButton();
        btnRemove = new DarkUI.Controls.DarkButton();
        chkRefreshOnStartup = new DarkUI.Controls.DarkCheckBox();
        chkRecursive = new DarkUI.Controls.DarkCheckBox();
        lblRenameFormat = new DarkUI.Controls.DarkLabel();
        txtRenameFormat = new DarkUI.Controls.DarkTextBox();
        lblRenameTokens = new DarkUI.Controls.DarkLabel();
        lblRenameTokens2 = new DarkUI.Controls.DarkLabel();
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
        lblDefaultGroup = new DarkUI.Controls.DarkLabel();
        cboDefaultGroup = new DarkUI.Controls.DarkComboBox();
        btnResetLayout = new DarkUI.Controls.DarkButton();
        tabFiles = new DarkUI.Controls.DarkTabPage();
        lblViewingInfo = new DarkUI.Controls.DarkLabel();
        lblMaxPreview = new DarkUI.Controls.DarkLabel();
        nudMaxPreviewMb = new DarkUI.Controls.DarkNumericUpDown();
        lblHexPage = new DarkUI.Controls.DarkLabel();
        nudHexPageKb = new DarkUI.Controls.DarkNumericUpDown();
        lblThumbCache = new DarkUI.Controls.DarkLabel();
        nudThumbnailCache = new DarkUI.Controls.DarkNumericUpDown();
        tabPaths = new DarkUI.Controls.DarkTabPage();
        lblPathsInfo = new DarkUI.Controls.DarkLabel();
        lblOutputDir = new DarkUI.Controls.DarkLabel();
        txtOutputDirectory = new DarkUI.Controls.DarkTextBox();
        btnBrowseOutput = new DarkUI.Controls.DarkButton();
        chkOpenOutputAfterTask = new DarkUI.Controls.DarkCheckBox();
        lblPasscode = new DarkUI.Controls.DarkLabel();
        txtDebugPasscode = new DarkUI.Controls.DarkTextBox();
        chkShowPasscode = new DarkUI.Controls.DarkCheckBox();
        lblPasscodeHint = new DarkUI.Controls.DarkLabel();
        tabSafety = new DarkUI.Controls.DarkTabPage();
        lblSafetyInfo = new DarkUI.Controls.DarkLabel();
        chkConfirmDelete = new DarkUI.Controls.DarkCheckBox();
        chkConfirmMove = new DarkUI.Controls.DarkCheckBox();
        chkPermanentDelete = new DarkUI.Controls.DarkCheckBox();
        tabDiagnostics = new DarkUI.Controls.DarkTabPage();
        lblMaintenanceInfo = new DarkUI.Controls.DarkLabel();
        btnExportSettings = new DarkUI.Controls.DarkButton();
        btnImportSettings = new DarkUI.Controls.DarkButton();
        btnResetSettings = new DarkUI.Controls.DarkButton();
        btnClearCaches = new DarkUI.Controls.DarkButton();
        btnOpenLogs = new DarkUI.Controls.DarkButton();
        btnSave = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        folderBrowserDialog = new FolderBrowserDialog();
        exportSettingsDialog = new SaveFileDialog();
        importSettingsDialog = new OpenFileDialog();
        tabsSettings.SuspendLayout();
        tabLibrary.SuspendLayout();
        tabAppearance.SuspendLayout();
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
        tabsSettings.Controls.Add(tabFiles);
        tabsSettings.Controls.Add(tabPaths);
        tabsSettings.Controls.Add(tabSafety);
        tabsSettings.Controls.Add(tabDiagnostics);
        tabsSettings.Location = new Point(12, 12);
        tabsSettings.Name = "tabsSettings";
        tabsSettings.SelectedIndex = 0;
        tabsSettings.Size = new Size(696, 372);
        tabsSettings.TabIndex = 0;
        // 
        // tabLibrary
        // 
        tabLibrary.BackColor = Color.FromArgb(60, 63, 65);
        tabLibrary.Controls.Add(lblLibraryInfo);
        tabLibrary.Controls.Add(lstFolders);
        tabLibrary.Controls.Add(btnAdd);
        tabLibrary.Controls.Add(btnRemove);
        tabLibrary.Controls.Add(chkRecursive);
        tabLibrary.Controls.Add(chkRefreshOnStartup);
        tabLibrary.Controls.Add(lblRenameFormat);
        tabLibrary.Controls.Add(txtRenameFormat);
        tabLibrary.Controls.Add(lblRenameTokens);
        tabLibrary.Controls.Add(lblRenameTokens2);
        tabLibrary.Location = new Point(4, 26);
        tabLibrary.Name = "tabLibrary";
        tabLibrary.Size = new Size(688, 342);
        tabLibrary.TabIndex = 0;
        tabLibrary.Text = "Library";
        // 
        // lblLibraryInfo
        // 
        lblLibraryInfo.Location = new Point(16, 16);
        lblLibraryInfo.Name = "lblLibraryInfo";
        lblLibraryInfo.Size = new Size(656, 15);
        lblLibraryInfo.TabIndex = 0;
        lblLibraryInfo.Text = "Folders scanned for games.";
        // 
        // lstFolders
        // 
        lstFolders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstFolders.DrawMode = DrawMode.OwnerDrawFixed;
        lstFolders.FormattingEnabled = true;
        lstFolders.ItemHeight = 20;
        lstFolders.Location = new Point(16, 40);
        lstFolders.Name = "lstFolders";
        lstFolders.SelectionMode = SelectionMode.MultiExtended;
        lstFolders.Size = new Size(656, 184);
        lstFolders.TabIndex = 1;
        // 
        // btnAdd
        // 
        btnAdd.Location = new Point(16, 232);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(130, 30);
        btnAdd.TabIndex = 2;
        btnAdd.Text = "Add Folder...";
        btnAdd.Click += btnAdd_Click;
        // 
        // btnRemove
        // 
        btnRemove.Location = new Point(152, 232);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new Size(110, 30);
        btnRemove.TabIndex = 3;
        btnRemove.Text = "Remove";
        btnRemove.Click += btnRemove_Click;
        // 
        // chkRecursive
        // 
        chkRecursive.AutoSize = true;
        chkRecursive.Location = new Point(16, 276);
        chkRecursive.Name = "chkRecursive";
        chkRecursive.Size = new Size(240, 19);
        chkRecursive.TabIndex = 4;
        chkRecursive.Text = "Search nested dump folders";
        // 
        // chkRefreshOnStartup
        // 
        chkRefreshOnStartup.AutoSize = true;
        chkRefreshOnStartup.Location = new Point(16, 302);
        chkRefreshOnStartup.Name = "chkRefreshOnStartup";
        chkRefreshOnStartup.Size = new Size(220, 19);
        chkRefreshOnStartup.TabIndex = 5;
        chkRefreshOnStartup.Text = "Refresh library on startup";
        // 
        // lblRenameFormat
        // 
        lblRenameFormat.Location = new Point(360, 268);
        lblRenameFormat.Name = "lblRenameFormat";
        lblRenameFormat.Size = new Size(320, 15);
        lblRenameFormat.TabIndex = 6;
        lblRenameFormat.Text = "Rename format";
        // 
        // txtRenameFormat
        // 
        txtRenameFormat.Location = new Point(360, 286);
        txtRenameFormat.Name = "txtRenameFormat";
        txtRenameFormat.PlaceholderText = "{TITLE} [{TITLE_ID}]";
        txtRenameFormat.Size = new Size(320, 23);
        txtRenameFormat.TabIndex = 7;
        // 
        // lblRenameTokens
        // 
        lblRenameTokens.Location = new Point(360, 314);
        lblRenameTokens.Name = "lblRenameTokens";
        lblRenameTokens.Size = new Size(320, 13);
        lblRenameTokens.TabIndex = 8;
        lblRenameTokens.Text = "Tokens: {TITLE} {TITLE_ID} {CONTENT_ID} {PLATFORM} {CATEGORY} {REGION}";
        // 
        // lblRenameTokens2
        // 
        lblRenameTokens2.Location = new Point(360, 328);
        lblRenameTokens2.Name = "lblRenameTokens2";
        lblRenameTokens2.Size = new Size(320, 13);
        lblRenameTokens2.TabIndex = 9;
        lblRenameTokens2.Text = "{VERSION} {SYSTEM_VERSION} {SOURCE} {SIZE} {LANGUAGE} {DRM} {DATE}";
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
        tabAppearance.Controls.Add(lblDefaultGroup);
        tabAppearance.Controls.Add(cboDefaultGroup);
        tabAppearance.Controls.Add(btnResetLayout);
        tabAppearance.Location = new Point(4, 26);
        tabAppearance.Name = "tabAppearance";
        tabAppearance.Size = new Size(688, 342);
        tabAppearance.TabIndex = 1;
        tabAppearance.Text = "Appearance";
        // 
        // lblAppearanceInfo
        // 
        lblAppearanceInfo.Location = new Point(16, 16);
        lblAppearanceInfo.Name = "lblAppearanceInfo";
        lblAppearanceInfo.Size = new Size(656, 15);
        lblAppearanceInfo.TabIndex = 0;
        lblAppearanceInfo.Text = "Theme and library grid display.";
        // 
        // lblTheme
        // 
        lblTheme.Location = new Point(16, 49);
        lblTheme.Name = "lblTheme";
        lblTheme.Size = new Size(170, 15);
        lblTheme.TabIndex = 1;
        lblTheme.Text = "Theme";
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
        lblDensity.Size = new Size(120, 15);
        lblDensity.TabIndex = 10;
        lblDensity.Text = "Density";
        // 
        // cboDensity
        // 
        cboDensity.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDensity.Location = new Point(400, 79);
        cboDensity.Name = "cboDensity";
        cboDensity.Size = new Size(160, 23);
        cboDensity.TabIndex = 11;
        // 
        // chkShowThumbnails
        // 
        chkShowThumbnails.AutoSize = true;
        chkShowThumbnails.Location = new Point(16, 118);
        chkShowThumbnails.Name = "chkShowThumbnails";
        chkShowThumbnails.Size = new Size(260, 19);
        chkShowThumbnails.TabIndex = 5;
        chkShowThumbnails.Text = "Show game thumbnails in the list";
        // 
        // chkShowGridLines
        // 
        chkShowGridLines.AutoSize = true;
        chkShowGridLines.Location = new Point(16, 146);
        chkShowGridLines.Name = "chkShowGridLines";
        chkShowGridLines.Size = new Size(260, 19);
        chkShowGridLines.TabIndex = 6;
        chkShowGridLines.Text = "Show grid lines";
        // 
        // lblDefaultGroup
        // 
        lblDefaultGroup.Location = new Point(16, 181);
        lblDefaultGroup.Name = "lblDefaultGroup";
        lblDefaultGroup.Size = new Size(170, 15);
        lblDefaultGroup.TabIndex = 7;
        lblDefaultGroup.Text = "Default grouping";
        // 
        // cboDefaultGroup
        // 
        cboDefaultGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDefaultGroup.Location = new Point(196, 177);
        cboDefaultGroup.Name = "cboDefaultGroup";
        cboDefaultGroup.Size = new Size(220, 23);
        cboDefaultGroup.TabIndex = 8;
        // 
        // btnResetLayout
        // 
        btnResetLayout.Location = new Point(196, 210);
        btnResetLayout.Name = "btnResetLayout";
        btnResetLayout.Size = new Size(190, 30);
        btnResetLayout.TabIndex = 9;
        btnResetLayout.Text = "Reset Column Layout";
        btnResetLayout.Click += btnResetLayout_Click;
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
        tabFiles.Location = new Point(4, 26);
        tabFiles.Name = "tabFiles";
        tabFiles.Size = new Size(688, 342);
        tabFiles.TabIndex = 2;
        tabFiles.Text = "Viewing";
        // 
        // lblViewingInfo
        // 
        lblViewingInfo.Location = new Point(16, 16);
        lblViewingInfo.Name = "lblViewingInfo";
        lblViewingInfo.Size = new Size(656, 15);
        lblViewingInfo.TabIndex = 0;
        lblViewingInfo.Text = "Preview limits and cache size.";
        // 
        // lblMaxPreview
        // 
        lblMaxPreview.Location = new Point(16, 49);
        lblMaxPreview.Name = "lblMaxPreview";
        lblMaxPreview.Size = new Size(170, 15);
        lblMaxPreview.TabIndex = 1;
        lblMaxPreview.Text = "Max auto-preview (MB)";
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
        lblHexPage.Text = "Hex page size (KB)";
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
        nudThumbnailCache.Name = "nudThumbnailCache";
        nudThumbnailCache.Size = new Size(80, 23);
        nudThumbnailCache.TabIndex = 6;
        nudThumbnailCache.Value = new decimal(new int[] { 512, 0, 0, 0 });
        // 
        // tabPaths
        // 
        tabPaths.BackColor = Color.FromArgb(60, 63, 65);
        tabPaths.Controls.Add(lblPathsInfo);
        tabPaths.Controls.Add(lblOutputDir);
        tabPaths.Controls.Add(txtOutputDirectory);
        tabPaths.Controls.Add(btnBrowseOutput);
        tabPaths.Controls.Add(chkOpenOutputAfterTask);
        tabPaths.Controls.Add(lblPasscode);
        tabPaths.Controls.Add(txtDebugPasscode);
        tabPaths.Controls.Add(chkShowPasscode);
        tabPaths.Controls.Add(lblPasscodeHint);
        tabPaths.Location = new Point(4, 26);
        tabPaths.Name = "tabPaths";
        tabPaths.Size = new Size(688, 342);
        tabPaths.TabIndex = 3;
        tabPaths.Text = "Paths and Build";
        // 
        // lblPathsInfo
        // 
        lblPathsInfo.Location = new Point(16, 16);
        lblPathsInfo.Name = "lblPathsInfo";
        lblPathsInfo.Size = new Size(656, 15);
        lblPathsInfo.TabIndex = 0;
        lblPathsInfo.Text = "Default output location and package passcode.";
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
        // chkOpenOutputAfterTask
        // 
        chkOpenOutputAfterTask.AutoSize = true;
        chkOpenOutputAfterTask.Location = new Point(16, 80);
        chkOpenOutputAfterTask.Name = "chkOpenOutputAfterTask";
        chkOpenOutputAfterTask.Size = new Size(320, 19);
        chkOpenOutputAfterTask.TabIndex = 4;
        chkOpenOutputAfterTask.Text = "Open output folder when a task finishes";
        // 
        // lblPasscode
        // 
        lblPasscode.Location = new Point(16, 117);
        lblPasscode.Name = "lblPasscode";
        lblPasscode.Size = new Size(170, 15);
        lblPasscode.TabIndex = 5;
        lblPasscode.Text = "Default debug passcode";
        // 
        // txtDebugPasscode
        // 
        txtDebugPasscode.Location = new Point(196, 113);
        txtDebugPasscode.Name = "txtDebugPasscode";
        txtDebugPasscode.Size = new Size(280, 23);
        txtDebugPasscode.TabIndex = 6;
        txtDebugPasscode.UseSystemPasswordChar = true;
        // 
        // chkShowPasscode
        // 
        chkShowPasscode.AutoSize = true;
        chkShowPasscode.Location = new Point(486, 115);
        chkShowPasscode.Name = "chkShowPasscode";
        chkShowPasscode.Size = new Size(60, 19);
        chkShowPasscode.TabIndex = 7;
        chkShowPasscode.Text = "Show";
        chkShowPasscode.CheckedChanged += chkShowPasscode_CheckedChanged;
        // 
        // lblPasscodeHint
        // 
        lblPasscodeHint.Location = new Point(196, 142);
        lblPasscodeHint.Name = "lblPasscodeHint";
        lblPasscodeHint.Size = new Size(470, 15);
        lblPasscodeHint.TabIndex = 8;
        lblPasscodeHint.Text = "32 characters. Leave blank to use the default all-zero passcode.";
        // 
        // tabSafety
        // 
        tabSafety.BackColor = Color.FromArgb(60, 63, 65);
        tabSafety.Controls.Add(lblSafetyInfo);
        tabSafety.Controls.Add(chkConfirmDelete);
        tabSafety.Controls.Add(chkConfirmMove);
        tabSafety.Controls.Add(chkPermanentDelete);
        tabSafety.Location = new Point(4, 26);
        tabSafety.Name = "tabSafety";
        tabSafety.Size = new Size(688, 342);
        tabSafety.TabIndex = 4;
        tabSafety.Text = "Safety";
        // 
        // lblSafetyInfo
        // 
        lblSafetyInfo.Location = new Point(16, 16);
        lblSafetyInfo.Name = "lblSafetyInfo";
        lblSafetyInfo.Size = new Size(656, 15);
        lblSafetyInfo.TabIndex = 0;
        lblSafetyInfo.Text = "Confirmation prompts before destructive actions.";
        // 
        // chkConfirmDelete
        // 
        chkConfirmDelete.AutoSize = true;
        chkConfirmDelete.Location = new Point(16, 49);
        chkConfirmDelete.Name = "chkConfirmDelete";
        chkConfirmDelete.Size = new Size(320, 19);
        chkConfirmDelete.TabIndex = 1;
        chkConfirmDelete.Text = "Confirm before deleting a source";
        // 
        // chkConfirmMove
        // 
        chkConfirmMove.AutoSize = true;
        chkConfirmMove.Location = new Point(16, 77);
        chkConfirmMove.Name = "chkConfirmMove";
        chkConfirmMove.Size = new Size(320, 19);
        chkConfirmMove.TabIndex = 2;
        chkConfirmMove.Text = "Confirm before moving a source";
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
        // tabDiagnostics
        // 
        tabDiagnostics.BackColor = Color.FromArgb(60, 63, 65);
        tabDiagnostics.Controls.Add(lblMaintenanceInfo);
        tabDiagnostics.Controls.Add(btnExportSettings);
        tabDiagnostics.Controls.Add(btnImportSettings);
        tabDiagnostics.Controls.Add(btnResetSettings);
        tabDiagnostics.Controls.Add(btnClearCaches);
        tabDiagnostics.Controls.Add(btnOpenLogs);
        tabDiagnostics.Location = new Point(4, 26);
        tabDiagnostics.Name = "tabDiagnostics";
        tabDiagnostics.Size = new Size(688, 342);
        tabDiagnostics.TabIndex = 5;
        tabDiagnostics.Text = "Maintenance";
        // 
        // lblMaintenanceInfo
        // 
        lblMaintenanceInfo.Location = new Point(16, 16);
        lblMaintenanceInfo.Name = "lblMaintenanceInfo";
        lblMaintenanceInfo.Size = new Size(656, 15);
        lblMaintenanceInfo.TabIndex = 0;
        lblMaintenanceInfo.Text = "Back up, restore or clear application data.";
        // 
        // btnExportSettings
        // 
        btnExportSettings.Location = new Point(16, 48);
        btnExportSettings.Name = "btnExportSettings";
        btnExportSettings.Size = new Size(190, 30);
        btnExportSettings.TabIndex = 1;
        btnExportSettings.Text = "Export Settings...";
        btnExportSettings.Click += btnExportSettings_Click;
        // 
        // btnImportSettings
        // 
        btnImportSettings.Location = new Point(16, 84);
        btnImportSettings.Name = "btnImportSettings";
        btnImportSettings.Size = new Size(190, 30);
        btnImportSettings.TabIndex = 2;
        btnImportSettings.Text = "Import Settings...";
        btnImportSettings.Click += btnImportSettings_Click;
        // 
        // btnResetSettings
        // 
        btnResetSettings.Location = new Point(16, 120);
        btnResetSettings.Name = "btnResetSettings";
        btnResetSettings.Size = new Size(190, 30);
        btnResetSettings.TabIndex = 3;
        btnResetSettings.Text = "Reset Settings";
        btnResetSettings.Click += btnResetSettings_Click;
        // 
        // btnClearCaches
        // 
        btnClearCaches.Location = new Point(16, 156);
        btnClearCaches.Name = "btnClearCaches";
        btnClearCaches.Size = new Size(190, 30);
        btnClearCaches.TabIndex = 4;
        btnClearCaches.Text = "Clear Caches";
        btnClearCaches.Click += btnClearCaches_Click;
        // 
        // btnOpenLogs
        // 
        btnOpenLogs.Location = new Point(16, 192);
        btnOpenLogs.Name = "btnOpenLogs";
        btnOpenLogs.Size = new Size(190, 30);
        btnOpenLogs.TabIndex = 5;
        btnOpenLogs.Text = "Open Log Folder";
        btnOpenLogs.Click += btnOpenLogs_Click;
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSave.Location = new Point(478, 396);
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
        btnCancel.Location = new Point(598, 396);
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
        ClientSize = new Size(720, 440);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(tabsSettings);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(600, 380);
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
