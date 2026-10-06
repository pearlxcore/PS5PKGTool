using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Mac;

internal static class NativeGuiChecks
{
    public static int Run() => AppBuilder.Configure<TestApp>().UsePlatformDetect()
        .StartWithClassicDesktopLifetime([]);

    public sealed class TestApp : App
    {
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            var desktop = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
            var window = (MainWindow)desktop.MainWindow!;
            window.Opened += async (_, _) =>
            {
                string root = Path.Combine(Path.GetTempPath(), "ps5pkgtool-native-gui-" + Guid.NewGuid().ToString("N"));
                int result = 0;
                try
                {
                    string source = Path.Combine(root, "source");
                    Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
                    Directory.CreateDirectory(Path.Combine(source, "data/nested"));
                    File.WriteAllText(Path.Combine(source, "sce_sys/param.json"), "{\"titleId\":\"PPSA00000\",\"contentId\":\"HP0000-PPSA00000_00-0000000000000000\",\"localizedParameters\":{\"defaultLanguage\":\"en-US\",\"en-US\":{\"titleName\":\"Generated macOS test\"}}}");
                    File.WriteAllBytes(Path.Combine(source, "data/nested/example.dat"), new byte[2 * 1024 * 1024]);
                    File.WriteAllBytes(Path.Combine(source, "eboot.bin"), [1, 2, 3]);
                    string package = Path.Combine(root, "test.pkg");
                    await Task.Run(() => SonyDebugPackageBuilder.CreateFromDirectoryAsync(source, package,
                        new SonyDebugPackageBuildOptions { ContentId = "HP0000-PPSA00000_00-0000000000000000", Compression = Ps5InnerCompression.Stored,
                            Seed = new byte[16], FakeSignModules = false, InjectRightSprx = false }));
                    await GuiChecks.RunAsync(root, package, source, window.Model);
                    window.Model.Input = Environment.GetEnvironmentVariable("PS5PKGTOOL_TEST_PKG") ?? package;
                    window.Model.Output = "";
                    window.Model.Workspace = "";
                    await window.Model.DetectAsync();
                    window.UpdateLayout();
                    if (!window.Model.Ready || !window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == window.Model.TitleId))
                        throw new InvalidOperationException("Native GUI metadata binding did not update.");
                    using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
                    bitmap.Render(window);
                    string screenshot = Path.Combine(Path.GetTempPath(), "ps5pkgtool-native-gui.png");
                    bitmap.Save(screenshot, PngBitmapEncoderOptions.Default);
                    Console.WriteLine("PASS: native macOS window, bindings and conversion. Screenshot: " + screenshot);
                }
                catch (Exception exception) { Console.Error.WriteLine(exception); result = 1; }
                finally
                {
                    if (Directory.Exists(root)) Directory.Delete(root, true);
                    desktop.Shutdown(result);
                }
            };
        }
    }
}
