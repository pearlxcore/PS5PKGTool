using DarkUI.Forms;

namespace PS5PKGTool.Forms;

/// <summary>
/// Drop-in replacement for <c>DarkMessageBox</c> that routes through the themed
/// <see cref="AppMessageBox"/> dialog.
/// </summary>
public static class AppDialog
{
    public static DialogResult ShowInformation(string message, string caption,
        DarkDialogButton buttons = DarkDialogButton.OK) =>
        AppMessageBox.Show(caption, message, AppMessageType.Info, Map(buttons));

    public static DialogResult ShowWarning(string message, string caption,
        DarkDialogButton buttons = DarkDialogButton.OK) =>
        AppMessageBox.Show(caption, message, AppMessageType.Warning, Map(buttons));

    public static DialogResult ShowError(string message, string caption,
        DarkDialogButton buttons = DarkDialogButton.OK) =>
        AppMessageBox.Show(caption, message, AppMessageType.Error, Map(buttons));

    public static DialogResult DialogYesNo(string message, string caption,
        DarkDialogButton buttons = DarkDialogButton.YesNoCancel) =>
        AppMessageBox.Show(caption, message, AppMessageType.Info, Map(buttons));

    public static DialogResult DialogYesNoCancel(string message, string caption,
        DarkDialogButton buttons = DarkDialogButton.YesNo) =>
        AppMessageBox.Show(caption, message, AppMessageType.Info, Map(buttons));

    private static AppMessageButtons Map(DarkDialogButton button) => button switch
    {
        DarkDialogButton.YesNo => AppMessageButtons.YesNo,
        DarkDialogButton.YesNoCancel => AppMessageButtons.YesNoCancel,
        _ => AppMessageButtons.OK
    };
}
