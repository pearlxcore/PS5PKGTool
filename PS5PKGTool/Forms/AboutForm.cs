using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PS5PKGTool.Forms;

public partial class AboutForm : DarkUI.Forms.DarkForm
{
    private const string GitHubUrl = "https://github.com/pearlxcore/PS5PkgTool";
    private const string KoFiUrl = "https://ko-fi.com/pearlxcore";
    private const string IssuesUrl = "https://github.com/pearlxcore/PS5PkgTool/issues";

    public AboutForm(string version)
    {
        InitializeComponent();
        lblVersion.Text = "Version " + version;
        using Stream? stream = typeof(AboutForm).Assembly.GetManifestResourceStream("PS5PKGTool.icon.ico");
        if (stream is not null)
        {
            using var icon = new Icon(stream, new Size(256, 256));
            picAppIcon.Image = icon.ToBitmap();
        }
    }

    private void btnGitHub_Click(object? sender, EventArgs e) => Open(GitHubUrl);

    private void btnKofi_Click(object? sender, EventArgs e) => Open(KoFiUrl);

    private void btnBug_Click(object? sender, EventArgs e) => Open(IssuesUrl);

    private void btnClose_Click(object? sender, EventArgs e) => Close();

    private static void Open(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or System.IO.IOException)
        {
        }
    }
}
