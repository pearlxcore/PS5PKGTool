using PS5PKGTool.Ffpfsc;

namespace PS5PKGTool.Forms;

public partial class ConvertImageForm : DarkUI.Forms.DarkForm
{
    private static readonly (Ps5ImageConversionTarget Target, string Name, string Extension, string Filter)[] Targets =
    [
        (Ps5ImageConversionTarget.Exfat, "exFAT image", ".exfat", "exFAT image (*.exfat)|*.exfat"),
        (Ps5ImageConversionTarget.Ffpkg, "FFPKG (UFS2) image", ".ffpkg", "FFPKG image (*.ffpkg)|*.ffpkg"),
        (Ps5ImageConversionTarget.Ffpfsc, "FFPFSC image", ".ffpfsc", "FFPFSC image (*.ffpfsc)|*.ffpfsc")
    ];

    public ConvertImageForm(string sourcePath, IReadOnlyList<Ps5ImageConversionTarget> targets,
        Ps5ImageConversionTarget? preferred = null)
    {
        InitializeComponent();
        SourcePath = sourcePath;
        lblSource.Text = "Source: " + sourcePath;
        foreach ((Ps5ImageConversionTarget target, string name, string _, string _) in Targets)
            if (targets.Contains(target))
                cboTarget.Items.Add(name);
        if (cboTarget.Items.Count > 0)
        {
            int index = 0;
            if (preferred is { } preferredTarget && TargetOf(preferredTarget) is { } preferredInfo)
            {
                int match = cboTarget.Items.IndexOf(preferredInfo.Name);
                if (match >= 0) index = match;
            }
            cboTarget.SelectedIndex = index;
        }
        UpdateForTarget();
    }

    public string SourcePath { get; }

    public Ps5ImageConversionTarget Target => Current?.Target ?? Ps5ImageConversionTarget.Exfat;

    public string OutputPath => txtOutput.Text.Trim();

    public bool Overwrite => chkOverwrite.Checked;

    private (Ps5ImageConversionTarget Target, string Name, string Extension, string Filter)? Current =>
        cboTarget.SelectedItem is string selected ? TargetOf(selected) : null;

    private static (Ps5ImageConversionTarget Target, string Name, string Extension, string Filter)? TargetOf(string name)
    {
        foreach ((Ps5ImageConversionTarget target, string display, string extension, string filter) in Targets)
            if (display == name)
                return (target, display, extension, filter);
        return null;
    }

    private static (Ps5ImageConversionTarget Target, string Name, string Extension, string Filter)? TargetOf(
        Ps5ImageConversionTarget target)
    {
        foreach ((Ps5ImageConversionTarget candidate, string display, string extension, string filter) in Targets)
            if (candidate == target)
                return (candidate, display, extension, filter);
        return null;
    }

    private void cboTarget_SelectedIndexChanged(object? sender, EventArgs e) => UpdateForTarget();

    private void UpdateForTarget()
    {
        if (Current is not { } info) return;
        saveFileDialog.Filter = info.Filter + "|All files (*.*)|*.*";
        txtOutput.Text = string.IsNullOrWhiteSpace(txtOutput.Text)
            ? DefaultOutput(info.Extension)
            : Path.ChangeExtension(txtOutput.Text, info.Extension);
    }

    private string DefaultOutput(string extension)
    {
        bool directory = Directory.Exists(SourcePath);
        string name = directory ? new DirectoryInfo(SourcePath).Name : Path.GetFileNameWithoutExtension(SourcePath);
        string? parent = directory ? Directory.GetParent(SourcePath)?.FullName : Path.GetDirectoryName(SourcePath);
        return Path.Combine(parent ?? SourcePath, name + extension);
    }

    private void btnBrowseOutput_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtOutput.Text))
        {
            saveFileDialog.FileName = Path.GetFileName(txtOutput.Text);
            string? directory = Path.GetDirectoryName(txtOutput.Text);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                saveFileDialog.InitialDirectory = directory;
        }
        if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            txtOutput.Text = saveFileDialog.FileName;
    }

    private void btnConvert_Click(object? sender, EventArgs e)
    {
        string output = txtOutput.Text.Trim();
        if (output.Length == 0)
        {
            AppDialog.ShowWarning("Select an output file.", "Convert image");
            return;
        }
        if (string.Equals(Path.GetFullPath(output), Path.GetFullPath(SourcePath), StringComparison.OrdinalIgnoreCase))
        {
            AppDialog.ShowWarning("The output must differ from the source.", "Convert image");
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }
}
