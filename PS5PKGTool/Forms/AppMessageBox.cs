using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PS5PKGTool.Forms;

public enum AppMessageType
{
    Info,
    Warning,
    Error
}

public enum AppMessageButtons
{
    OK,
    YesNo,
    YesNoCancel
}

/// <summary>
/// Themed replacement for the system message box: a colored title, a wrapped message that sizes
/// the window to its content, optional Yes/No/Cancel buttons and a Copy button, plus a
/// type-specific notification sound.
/// </summary>
public partial class AppMessageBox : DarkUI.Forms.DarkForm
{
    public DialogResult Result { get; private set; } = DialogResult.None;

    public AppMessageBox(string title, string message, AppMessageType type, AppMessageButtons buttons)
    {
        InitializeComponent();

        Text = title;
        lblTitle.Text = title;
        lblMessage.Text = message;
        lblMessage.MaximumSize = new Size(420, 0);

        Color accent = type switch
        {
            AppMessageType.Error => Color.FromArgb(220, 80, 80),
            AppMessageType.Warning => Color.FromArgb(220, 180, 60),
            _ => Color.FromArgb(100, 160, 220)
        };
        NotificationIcon = type switch
        {
            AppMessageType.Error => MessageBoxIcon.Error,
            AppMessageType.Warning => MessageBoxIcon.Warning,
            _ => MessageBoxIcon.Information
        };
        lblTitle.ForeColor = accent;

        using (Graphics graphics = lblMessage.CreateGraphics())
        {
            SizeF size = graphics.MeasureString(message, lblMessage.Font, 420);
            int neededHeight = (int)Math.Ceiling(size.Height) + 90;
            if (neededHeight < 130) neededHeight = 130;
            if (neededHeight > 520) neededHeight = 520;
            ClientSize = new Size(460, neededHeight);
            lblMessage.Size = new Size(420, neededHeight - 85);
        }

        btnCopy.Visible = true;
        btnCopy.Location = new Point(20, ClientSize.Height - 46);

        switch (buttons)
        {
            case AppMessageButtons.OK:
                btnOK.Visible = true;
                btnOK.Location = new Point((ClientSize.Width - 100) / 2, ClientSize.Height - 46);
                break;
            case AppMessageButtons.YesNo:
                btnYes.Visible = true;
                btnNo.Visible = true;
                btnYes.Location = new Point(ClientSize.Width - 220, ClientSize.Height - 46);
                btnNo.Location = new Point(ClientSize.Width - 110, ClientSize.Height - 46);
                break;
            case AppMessageButtons.YesNoCancel:
                btnYes.Visible = true;
                btnNo.Visible = true;
                btnCancel.Visible = true;
                btnYes.Location = new Point(ClientSize.Width - 330, ClientSize.Height - 46);
                btnNo.Location = new Point(ClientSize.Width - 220, ClientSize.Height - 46);
                btnCancel.Location = new Point(ClientSize.Width - 110, ClientSize.Height - 46);
                break;
        }

        ActiveControl = btnOK.Visible ? btnOK : btnYes;
    }

    private void btnOK_Click(object? sender, EventArgs e) { Result = DialogResult.OK; Close(); }
    private void btnYes_Click(object? sender, EventArgs e) { Result = DialogResult.Yes; Close(); }
    private void btnNo_Click(object? sender, EventArgs e) { Result = DialogResult.No; Close(); }
    private void btnCancel_Click(object? sender, EventArgs e) { Result = DialogResult.Cancel; Close(); }

    private void btnCopy_Click(object? sender, EventArgs e)
    {
        try { Clipboard.SetText($"{lblTitle.Text}\r\n{lblMessage.Text}"); }
        catch (ExternalException) { }
        btnCopy.Text = "Copied!";
    }

    public static DialogResult Show(string title, string message, AppMessageType type, AppMessageButtons buttons)
    {
        using var dialog = new AppMessageBox(title, message, type, buttons);
        dialog.ShowDialog();
        return dialog.Result;
    }

    public static DialogResult Show(IWin32Window? owner, string title, string message, AppMessageType type,
        AppMessageButtons buttons)
    {
        using var dialog = new AppMessageBox(title, message, type, buttons);
        dialog.ShowDialog(owner);
        return dialog.Result;
    }
}
