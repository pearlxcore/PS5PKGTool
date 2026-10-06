using System.ComponentModel;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;

namespace PS5PKGTool.Mac;

public sealed class ConversionViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private CancellationTokenSource? cancellation;
    private string input = "", output = "", workspace = "";
    public string Input { get => input; set { input = value; Title = "No package selected"; TitleId = PackageType = "—"; Ready = false; Notify(); } }
    public string Output { get => output; set { output = value; Notify(); } }
    public string Workspace { get => workspace; set { workspace = value; Notify(); } }
    public string Title { get; private set; } = "No package selected";
    public string TitleId { get; private set; } = "—";
    public string PackageType { get; private set; } = "—";
    public string Status { get; private set; } = "Select a debug/fake PKG to begin.";
    public string Error { get; private set; } = "";
    public double Percent { get; private set; }
    public bool Indeterminate { get; private set; }
    public bool Busy { get; private set; }
    public bool Ready { get; private set; }
    public bool CanEdit => !Busy;
    public bool CanConvert => Ready && !Busy && !string.IsNullOrWhiteSpace(Output);
    public bool CanCancel => Busy && cancellation is { IsCancellationRequested: false };

    public async Task DetectAsync()
    {
        if (Busy) return;
        Begin("Reading package metadata…");
        Ready = false;
        try
        {
            string path = Path.GetFullPath(Input);
            Ps5GameInfo game = await Task.Run(() => new SonyPkgGameReader().Read(path));
            cancellation!.Token.ThrowIfCancellationRequested();
            Title = game.Title;
            TitleId = game.TitleId;
            PackageType = game.Package?.KindDisplayName ?? "Unknown";
            if (game.Package is not { Kind: SonyPkgKind.FinalizedDebug, PfsImageSize: > 0 })
                throw new InvalidDataException("Only finalized debug/fake PKGs are supported. Retail, metadata-only and patch packages cannot be converted here.");
            Ready = true;
            if (string.IsNullOrWhiteSpace(Output)) Output = Path.ChangeExtension(path, ".exfat");
            Status = "Ready to convert to exFAT.";
        }
        catch (OperationCanceledException) { Status = "Cancelled."; }
        catch (Exception exception) { Error = exception.Message; Status = "Package could not be opened."; }
        finally { End(); }
    }

    public async Task ConvertAsync()
    {
        if (!CanConvert) return;
        Begin("Preparing conversion…");
        CancellationTokenSource operation = cancellation!;
        try
        {
            string source = Path.GetFullPath(Input), destination = Path.GetFullPath(Output);
            if (File.Exists(destination) || Directory.Exists(destination))
                throw new IOException("The output already exists. Choose a new filename to preserve it.");
            if (!string.Equals(Path.GetExtension(destination), ".exfat", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Choose an output filename ending in .exfat.");
            string staging = string.IsNullOrWhiteSpace(Workspace)
                ? Path.GetDirectoryName(destination)! : Path.GetFullPath(Workspace);
            if (!Directory.Exists(staging)) throw new DirectoryNotFoundException("The temporary workspace must be an existing directory.");
            var progress = new Progress<Ps5ImageConversionProgress>(value =>
            {
                if (!ReferenceEquals(cancellation, operation)) return;
                Indeterminate = value.Total <= 0;
                Percent = value.Total > 0 ? Math.Clamp(100.0 * value.Completed / value.Total, 0, 100) : 0;
                Status = $"{value.Stage} · {value.Completed / 1048576.0:N1} / {value.Total / 1048576.0:N1} MiB";
                Notify();
            });
            Ps5ImageConversionResult result = await Task.Run(() => SonyPackageImageConversion.ConvertAsync(
                source, destination, Ps5ImageConversionTarget.Exfat, progress: progress,
                cancellationToken: operation.Token, temporaryDirectory: staging));
            Percent = 100;
            Status = $"Verified exFAT image · {result.FileCount:N0} files · {result.OutputBytes / 1073741824.0:N2} GiB";
        }
        catch (OperationCanceledException) { Status = "Cancelled. No image was published."; }
        catch (Exception exception) { Error = exception.Message; Status = "Conversion failed."; }
        finally { End(); }
    }

    public void Cancel()
    {
        cancellation?.Cancel();
        Status = "Cancelling… waiting for the current operation to stop.";
        Notify();
    }

    private void Begin(string status)
    {
        cancellation = new CancellationTokenSource();
        Busy = Indeterminate = true;
        Percent = 0;
        Error = "";
        Status = status;
        Notify();
    }

    private void End()
    {
        cancellation?.Dispose();
        cancellation = null;
        Busy = Indeterminate = false;
        Notify();
    }

    private void Notify() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
