#nullable enable

namespace PS5PKGTool.Forms;

partial class SettingsForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkSectionPanel sectionFolders = null!;
    private DarkUI.Controls.DarkListBox lstFolders = null!;
    private DarkUI.Controls.DarkButton btnAdd = null!;
    private DarkUI.Controls.DarkButton btnRemove = null!;
    private DarkUI.Controls.DarkCheckBox chkRecursive = null!;
    private DarkUI.Controls.DarkButton btnSave = null!;
    private DarkUI.Controls.DarkButton btnCancel = null!;
    private FolderBrowserDialog folderBrowserDialog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        sectionFolders = new DarkUI.Controls.DarkSectionPanel();
        lstFolders = new DarkUI.Controls.DarkListBox();
        btnAdd = new DarkUI.Controls.DarkButton();
        btnRemove = new DarkUI.Controls.DarkButton();
        chkRecursive = new DarkUI.Controls.DarkCheckBox();
        btnSave = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        folderBrowserDialog = new FolderBrowserDialog();
        sectionFolders.SuspendLayout();
        SuspendLayout();
        // 
        // sectionFolders
        // 
        sectionFolders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        sectionFolders.Controls.Add(lstFolders);
        sectionFolders.Controls.Add(btnAdd);
        sectionFolders.Controls.Add(btnRemove);
        sectionFolders.Controls.Add(chkRecursive);
        sectionFolders.Location = new Point(12, 12);
        sectionFolders.Name = "sectionFolders";
        sectionFolders.SectionHeader = "PS5 Dump Library Folders";
        sectionFolders.Size = new Size(660, 280);
        sectionFolders.TabIndex = 0;
        // 
        // lstFolders
        // 
        lstFolders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstFolders.DrawMode = DrawMode.OwnerDrawFixed;
        lstFolders.FormattingEnabled = true;
        lstFolders.ItemHeight = 18;
        lstFolders.Location = new Point(10, 34);
        lstFolders.Name = "lstFolders";
        lstFolders.SelectionMode = SelectionMode.MultiExtended;
        lstFolders.Size = new Size(640, 186);
        lstFolders.TabIndex = 0;
        // 
        // btnAdd
        // 
        btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnAdd.Location = new Point(10, 233);
        btnAdd.Name = "btnAdd";
        btnAdd.Size = new Size(110, 30);
        btnAdd.TabIndex = 1;
        btnAdd.Text = "Add Folder...";
        btnAdd.Click += btnAdd_Click;
        // 
        // btnRemove
        // 
        btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnRemove.Location = new Point(126, 233);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new Size(110, 30);
        btnRemove.TabIndex = 2;
        btnRemove.Text = "Remove";
        btnRemove.Click += btnRemove_Click;
        // 
        // chkRecursive
        // 
        chkRecursive.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        chkRecursive.AutoSize = true;
        chkRecursive.Location = new Point(474, 240);
        chkRecursive.Name = "chkRecursive";
        chkRecursive.Size = new Size(176, 19);
        chkRecursive.TabIndex = 3;
        chkRecursive.Text = "Search nested dump folders";
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSave.Location = new Point(446, 304);
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
        btnCancel.Location = new Point(562, 304);
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
        ClientSize = new Size(684, 348);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(sectionFolders);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "PS5 PKG Tool Settings";
        sectionFolders.ResumeLayout(false);
        sectionFolders.PerformLayout();
        ResumeLayout(false);
    }
}
