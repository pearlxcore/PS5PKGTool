#nullable enable

namespace PS5PKGTool.Forms;

partial class ExfatEditorForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkLabel lblImage = null!;
    private DarkUI.Controls.DarkSplitContainer splitEditor = null!;
    private DarkUI.Controls.DarkSplitPane splitEditorPane1 = null!;
    private DarkUI.Controls.DarkSplitPane splitEditorPane2 = null!;
    private DarkUI.Controls.DarkSectionPanel sectionEntries = null!;
    private DarkUI.Controls.DarkDataGridView gridEntries = null!;
    private DarkUI.Controls.DarkSectionPanel sectionChanges = null!;
    private DarkUI.Controls.DarkListBox lstChanges = null!;
    private DarkUI.Controls.DarkLabel lblTarget = null!;
    private DarkUI.Controls.DarkTextBox txtTarget = null!;
    private DarkUI.Controls.DarkButton btnReplace = null!;
    private DarkUI.Controls.DarkButton btnAddFiles = null!;
    private DarkUI.Controls.DarkButton btnAddFolder = null!;
    private DarkUI.Controls.DarkButton btnNewDirectory = null!;
    private DarkUI.Controls.DarkButton btnDelete = null!;
    private DarkUI.Controls.DarkButton btnUndo = null!;
    private DarkUI.Controls.DarkProgressBar progressEdit = null!;
    private DarkUI.Controls.DarkLabel lblStatus = null!;
    private DarkUI.Controls.DarkButton btnApply = null!;
    private DarkUI.Controls.DarkButton btnCancelOperation = null!;
    private DarkUI.Controls.DarkButton btnClose = null!;
    private OpenFileDialog replacementOpenDialog = null!;
    private OpenFileDialog addFilesOpenDialog = null!;
    private FolderBrowserDialog addFolderDialog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExfatEditorForm));
        components = new System.ComponentModel.Container();
        lblImage = new DarkUI.Controls.DarkLabel();
        splitEditor = new DarkUI.Controls.DarkSplitContainer();
        splitEditorPane1 = new DarkUI.Controls.DarkSplitPane();
        splitEditorPane2 = new DarkUI.Controls.DarkSplitPane();
        sectionEntries = new DarkUI.Controls.DarkSectionPanel();
        gridEntries = new DarkUI.Controls.DarkDataGridView();
        sectionChanges = new DarkUI.Controls.DarkSectionPanel();
        lstChanges = new DarkUI.Controls.DarkListBox();
        lblTarget = new DarkUI.Controls.DarkLabel();
        txtTarget = new DarkUI.Controls.DarkTextBox();
        btnReplace = new DarkUI.Controls.DarkButton();
        btnAddFiles = new DarkUI.Controls.DarkButton();
        btnAddFolder = new DarkUI.Controls.DarkButton();
        btnNewDirectory = new DarkUI.Controls.DarkButton();
        btnDelete = new DarkUI.Controls.DarkButton();
        btnUndo = new DarkUI.Controls.DarkButton();
        progressEdit = new DarkUI.Controls.DarkProgressBar();
        lblStatus = new DarkUI.Controls.DarkLabel();
        btnApply = new DarkUI.Controls.DarkButton();
        btnCancelOperation = new DarkUI.Controls.DarkButton();
        btnClose = new DarkUI.Controls.DarkButton();
        replacementOpenDialog = new OpenFileDialog();
        addFilesOpenDialog = new OpenFileDialog();
        addFolderDialog = new FolderBrowserDialog();
        splitEditor.SuspendLayout();
        splitEditorPane1.SuspendLayout();
        splitEditorPane2.SuspendLayout();
        sectionEntries.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridEntries).BeginInit();
        sectionChanges.SuspendLayout();
        SuspendLayout();
        // 
        // lblImage
        // 
        lblImage.AutoEllipsis = true;
        lblImage.Dock = DockStyle.Top;
        lblImage.Location = new Point(10, 10);
        lblImage.Name = "lblImage";
        lblImage.Padding = new Padding(8, 0, 8, 0);
        lblImage.Size = new Size(1164, 32);
        lblImage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // splitEditor
        // 
        splitEditor.Dock = DockStyle.Fill;
        splitEditor.Location = new Point(10, 42);
        splitEditor.Controls.Add(splitEditorPane1);
        splitEditor.Controls.Add(splitEditorPane2);
        splitEditor.Name = "splitEditor";
        splitEditorPane1.Controls.Add(sectionEntries);
        splitEditorPane2.Controls.Add(sectionChanges);
        splitEditor.Size = new Size(1164, 530);
        splitEditor.PanelSizes = new int[] { 780, 384 };
        // 
        // sectionEntries
        // 
        sectionEntries.Controls.Add(gridEntries);
        sectionEntries.Dock = DockStyle.Fill;
        sectionEntries.Name = "sectionEntries";
        sectionEntries.SectionHeader = "Image files and directories";
        // 
        // gridEntries
        // 
        gridEntries.AllowUserToAddRows = false;
        gridEntries.AllowUserToDeleteRows = false;
        gridEntries.AllowUserToDragDropRows = false;
        gridEntries.AllowUserToOrderColumns = true;
        gridEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridEntries.Dock = DockStyle.Fill;
        gridEntries.MultiSelect = true;
        gridEntries.Name = "gridEntries";
        gridEntries.ReadOnly = true;
        gridEntries.RowHeadersVisible = false;
        gridEntries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridEntries.SelectionChanged += gridEntries_SelectionChanged;
        // 
        // sectionChanges
        // 
        sectionChanges.Controls.Add(lstChanges);
        sectionChanges.Dock = DockStyle.Fill;
        sectionChanges.Name = "sectionChanges";
        sectionChanges.SectionHeader = "Queued changes";
        // 
        // lstChanges
        // 
        lstChanges.Dock = DockStyle.Fill;
        lstChanges.DrawMode = DrawMode.OwnerDrawFixed;
        lstChanges.FormattingEnabled = true;
        lstChanges.ItemHeight = 18;
        lstChanges.Name = "lstChanges";
        // 
        // lblTarget
        // 
        lblTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblTarget.Location = new Point(18, 580);
        lblTarget.Name = "lblTarget";
        lblTarget.Size = new Size(130, 25);
        lblTarget.Text = "Target directory:";
        lblTarget.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtTarget
        // 
        txtTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtTarget.Location = new Point(150, 580);
        txtTarget.Name = "txtTarget";
        txtTarget.PlaceholderText = "Root is empty; use forward slashes for nested paths";
        txtTarget.Size = new Size(1016, 25);
        // 
        // btnReplace
        // 
        btnReplace.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnReplace.Location = new Point(18, 612);
        btnReplace.Name = "btnReplace";
        btnReplace.Size = new Size(120, 29);
        btnReplace.Text = "Replace File...";
        btnReplace.Click += btnReplace_Click;
        // 
        // btnAddFiles
        // 
        btnAddFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnAddFiles.Location = new Point(144, 612);
        btnAddFiles.Name = "btnAddFiles";
        btnAddFiles.Size = new Size(120, 29);
        btnAddFiles.Text = "Add Files...";
        btnAddFiles.Click += btnAddFiles_Click;
        // 
        // btnAddFolder
        // 
        btnAddFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnAddFolder.Location = new Point(270, 612);
        btnAddFolder.Name = "btnAddFolder";
        btnAddFolder.Size = new Size(120, 29);
        btnAddFolder.Text = "Add Folder...";
        btnAddFolder.Click += btnAddFolder_Click;
        // 
        // btnNewDirectory
        // 
        btnNewDirectory.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnNewDirectory.Location = new Point(396, 612);
        btnNewDirectory.Name = "btnNewDirectory";
        btnNewDirectory.Size = new Size(130, 29);
        btnNewDirectory.Text = "New Target Folder";
        btnNewDirectory.Click += btnNewDirectory_Click;
        // 
        // btnDelete
        // 
        btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnDelete.Location = new Point(532, 612);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(120, 29);
        btnDelete.Text = "Delete Selected";
        btnDelete.Click += btnDelete_Click;
        // 
        // btnUndo
        // 
        btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnUndo.Location = new Point(658, 612);
        btnUndo.Name = "btnUndo";
        btnUndo.Size = new Size(120, 29);
        btnUndo.Text = "Undo Last";
        btnUndo.Click += btnUndo_Click;
        // 
        // progressEdit
        // 
        progressEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressEdit.Location = new Point(18, 650);
        progressEdit.Name = "progressEdit";
        progressEdit.Size = new Size(1148, 22);
        progressEdit.TextMode = DarkUI.Controls.DarkProgressBarMode.Percentage;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.AutoEllipsis = true;
        lblStatus.Location = new Point(18, 677);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(730, 32);
        lblStatus.Text = "Queue changes, then Apply. Structural changes use a verified transactional rebuild.";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnApply
        // 
        btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnApply.Location = new Point(796, 680);
        btnApply.Name = "btnApply";
        btnApply.Size = new Size(120, 31);
        btnApply.Text = "Apply Changes";
        btnApply.Click += btnApply_Click;
        // 
        // btnCancelOperation
        // 
        btnCancelOperation.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancelOperation.Enabled = false;
        btnCancelOperation.Location = new Point(922, 680);
        btnCancelOperation.Name = "btnCancelOperation";
        btnCancelOperation.Size = new Size(112, 31);
        btnCancelOperation.Text = "Cancel";
        btnCancelOperation.Click += btnCancelOperation_Click;
        // 
        // btnClose
        // 
        btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClose.DialogResult = DialogResult.Cancel;
        btnClose.Location = new Point(1040, 680);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(126, 31);
        btnClose.Text = "Close";
        // 
        // replacementOpenDialog
        // 
        replacementOpenDialog.CheckFileExists = true;
        replacementOpenDialog.Filter = "All files (*.*)|*.*";
        replacementOpenDialog.Title = "Select replacement file";
        // 
        // addFilesOpenDialog
        // 
        addFilesOpenDialog.CheckFileExists = true;
        addFilesOpenDialog.Filter = "All files (*.*)|*.*";
        addFilesOpenDialog.Multiselect = true;
        addFilesOpenDialog.Title = "Select files to add";
        // 
        // addFolderDialog
        // 
        addFolderDialog.Description = "Select a directory tree to add to the exFAT image";
        // 
        // ExfatEditorForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(60, 63, 65);
        CancelButton = btnClose;
        ClientSize = new Size(1184, 732);
        Controls.Add(splitEditor);
        Controls.Add(lblTarget);
        Controls.Add(txtTarget);
        Controls.Add(btnReplace);
        Controls.Add(btnAddFiles);
        Controls.Add(btnAddFolder);
        Controls.Add(btnNewDirectory);
        Controls.Add(btnDelete);
        Controls.Add(btnUndo);
        Controls.Add(progressEdit);
        Controls.Add(lblStatus);
        Controls.Add(btnApply);
        Controls.Add(btnCancelOperation);
        Controls.Add(btnClose);
        Controls.Add(lblImage);
        MinimumSize = new Size(950, 650);
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "ExfatEditorForm";
        Padding = new Padding(10, 10, 10, 160);
        StartPosition = FormStartPosition.CenterParent;
        Text = "exFAT Image Editor";
        FormClosing += ExfatEditorForm_FormClosing;
        splitEditorPane1.ResumeLayout(false);
        splitEditorPane2.ResumeLayout(false);
        splitEditor.ResumeLayout(false);
        sectionEntries.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridEntries).EndInit();
        sectionChanges.ResumeLayout(false);
        ResumeLayout(false);
    }
}
