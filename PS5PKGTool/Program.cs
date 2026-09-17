using DarkUI.Forms;
using PS5PKGTool.Forms;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool;

internal static class Program
{
    private const string SingleInstanceName = "PS5PKGTool.SingleInstance.v1";

    [STAThread]
    private static void Main(string[] args)
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        System.Windows.Forms.Application.ThreadException += (_, e) => HandleFatal("UI thread", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleFatal("AppDomain", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Logger.Exception("Unobserved task", e.Exception);
            e.SetObserved();
        };

        if (args.Length > 0 && args[0] is "--shell-register" or "--shell-unregister")
        {
            try
            {
                if (args[0] == "--shell-register") ShellIntegration.Install();
                else ShellIntegration.Uninstall();
            }
            catch (Exception ex)
            {
                Logger.Exception("Shell integration", ex);
                DarkMessageBox.ShowError(ex.Message, "Explorer integration");
            }
            return;
        }

        if (args.Length > 0 && args[0] == "--settings-smoke")
        {
            // Headless construction check for the settings dialog (no manual interaction).
            try
            {
                using var form = new SettingsForm(new AppSettings(), _ => (true, null));
                form.CreateControl();
                Logger.Info("Settings smoke: OK");
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Logger.Exception("Settings smoke", ex);
                Environment.ExitCode = 1;
            }
            return;
        }

        string? externalPath = args.Length >= 2 && args[0] == "--open" ? args[1] : null;

        using var singleInstance = new Mutex(true, SingleInstanceName, out bool createdNew);
        if (!createdNew)
        {
            DarkMessageBox.ShowWarning("PS5 PKG Tool is already running.", "PS5 PKG Tool");
            return;
        }

        Logger.Info("PS5 PKG Tool started.");
        try
        {
            System.Windows.Forms.Application.Run(new MainForm(externalPath));
        }
        catch (Exception ex)
        {
            HandleFatal("Startup", ex);
        }
        finally
        {
            Logger.Info("PS5 PKG Tool exited.");
        }
    }

    private static void HandleFatal(string context, Exception? exception)
    {
        Logger.Exception(context, exception ?? new Exception("Unknown error"));
        try
        {
            DarkMessageBox.ShowError(
                (exception?.Message ?? "An unknown error occurred.") + $"\n\nA log was written to:\n{Logger.LogPath}",
                "PS5 PKG Tool error");
        }
        catch (Exception)
        {
            // Nothing more we can do while handling a fatal error.
        }
    }
}
