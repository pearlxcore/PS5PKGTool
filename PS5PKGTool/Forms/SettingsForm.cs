using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class SettingsForm : DarkUI.Forms.DarkForm
{
    public SettingsForm(AppSettings settings)
    {
        InitializeComponent();
        Settings = new AppSettings
        {
            LibraryFolders = settings.LibraryFolders.ToList(),
            RecursiveScan = settings.RecursiveScan
        };
        foreach (string folder in Settings.LibraryFolders) lstFolders.Items.Add(folder);
        chkRecursive.Checked = Settings.RecursiveScan;
    }

    public AppSettings Settings { get; }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        if (!lstFolders.Items.Cast<string>().Contains(folderBrowserDialog.SelectedPath, StringComparer.OrdinalIgnoreCase))
            lstFolders.Items.Add(folderBrowserDialog.SelectedPath);
    }

    private void btnRemove_Click(object? sender, EventArgs e)
    {
        while (lstFolders.SelectedIndices.Count > 0)
            lstFolders.Items.RemoveAt(lstFolders.SelectedIndices[0]);
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        Settings.LibraryFolders = lstFolders.Items.Cast<string>().ToList();
        Settings.RecursiveScan = chkRecursive.Checked;
        DialogResult = DialogResult.OK;
        Close();
    }
}
