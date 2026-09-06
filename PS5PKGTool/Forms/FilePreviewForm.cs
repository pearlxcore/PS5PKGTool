using DarkUI.Forms;

namespace PS5PKGTool.Forms;

public partial class FilePreviewForm : DarkDialog
{
    private FilePreviewForm(string title)
    {
        InitializeComponent();
        Text = title;
        DialogButtons = MessageBoxButtons.OK;
    }

    public static void ShowImage(IWin32Window owner, string title, byte[] imageBytes)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(imageBytes);
        using var form = new FilePreviewForm(title);
        using var stream = new MemoryStream(imageBytes, writable: false);
        using Image source = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: true);
        form.picturePreview.Image = new Bitmap(source);
        form.picturePreview.Visible = true;
        form.textPreview.Visible = false;
        form.ShowDialog(owner);
    }

    public static void ShowText(IWin32Window owner, string title, string text)
    {
        ArgumentNullException.ThrowIfNull(owner);
        using var form = new FilePreviewForm(title);
        form.textPreview.Text = text ?? string.Empty;
        form.textPreview.SelectionStart = 0;
        form.textPreview.SelectionLength = 0;
        form.picturePreview.Visible = false;
        form.textPreview.Visible = true;
        form.ShowDialog(owner);
    }
}
