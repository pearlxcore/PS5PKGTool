using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace PS5PKGTool.Mac;

public partial class MainWindow : Window
{
    public ConversionViewModel Model { get; } = new();

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = Model;
        Closing += (_, eventArgs) =>
        {
            if (!Model.Busy) return;
            eventArgs.Cancel = true;
            Model.Cancel();
        };
    }

    private async void BrowseInput(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a debug/fake PKG", AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("PS5 package") { Patterns = ["*.pkg"] }]
        });
        if (files.Count == 0 || files[0].TryGetLocalPath() is not { } path) return;
        Model.Input = path;
        Model.Output = "";
        await Model.DetectAsync();
    }

    private async void BrowseOutput(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Choose a new exFAT image", SuggestedFileName = Path.GetFileName(Model.Output),
            DefaultExtension = "exfat", FileTypeChoices = [new FilePickerFileType("exFAT image") { Patterns = ["*.exfat"] }]
        });
        if (file?.TryGetLocalPath() is { } path) Model.Output = path;
    }

    private async void BrowseWorkspace(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select temporary workspace" });
        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path) Model.Workspace = path;
    }

    private async void ReadMetadata(object? sender, RoutedEventArgs e) => await Model.DetectAsync();
    private async void Convert(object? sender, RoutedEventArgs e) => await Model.ConvertAsync();
    private void CancelConversion(object? sender, RoutedEventArgs e) => Model.Cancel();
}
