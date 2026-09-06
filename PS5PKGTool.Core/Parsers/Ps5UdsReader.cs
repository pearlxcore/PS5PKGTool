using System.Text.Json;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;

namespace PS5PKGTool.Core.Parsers;

public sealed class Ps5UdsReader
{
    private readonly UcpReader _ucp = new();

    public Ps5UdsSummary? Read(string gameRoot, CancellationToken cancellationToken = default)
    {
        string path = Path.Combine(gameRoot, "sce_sys", "uds", "uds00.ucp");
        if (!File.Exists(path)) return null;
        UcpArchive archive = _ucp.Read(path);
        return ReadArchive(archive, cancellationToken);
    }

    public Ps5UdsSummary? Read(IReadOnlyGameFileSystem files, CancellationToken cancellationToken = default)
    {
        const string path = "sce_sys/uds/uds00.ucp";
        if (!files.FileExists(path)) return null;
        UcpArchive archive = _ucp.Read(path, () => files.OpenRead(path));
        return ReadArchive(archive, cancellationToken);
    }

    private Ps5UdsSummary ReadArchive(UcpArchive archive, CancellationToken cancellationToken)
    {

        using JsonDocument enumDocument = ReadJson(archive, "enum_definition.json");
        using JsonDocument eventDocument = ReadJson(archive, "events_definition.json");
        using JsonDocument statDocument = ReadJson(archive, "stats_definition.json");
        using JsonDocument extractionDocument = ReadJson(archive, "stats_extraction.json");

        JsonElement eventsRoot = eventDocument.RootElement;
        var events = new List<Ps5UdsEvent>();
        if (eventsRoot.TryGetProperty("events", out JsonElement eventArray) && eventArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in eventArray.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                events.Add(new Ps5UdsEvent
                {
                    Name = GetString(item, "eventName"),
                    Type = GetString(item, "eventType"),
                    DefinitionGroup = GetString(item, "definitionGroup"),
                    PropertyCount = ArrayLength(item, "properties")
                });
            }
        }

        JsonElement statRoot = statDocument.RootElement;
        string npId = GetString(statRoot, "npCommunicationId");
        if (string.IsNullOrWhiteSpace(npId)) npId = GetString(enumDocument.RootElement, "npCommunicationId");
        return new Ps5UdsSummary
        {
            NpCommunicationId = npId,
            EnumGroupCount = ArrayLength(enumDocument.RootElement, "enumArray"),
            EventCount = events.Count,
            StatCount = ArrayLength(statRoot, "statDefinitionArray"),
            ExtractionRuleCount = ArrayLength(extractionDocument.RootElement, "statsExtractionRuleArray"),
            Events = events,
            IntegrityValid = _ucp.ValidateIntegrity(archive, cancellationToken)
        };
    }

    private JsonDocument ReadJson(UcpArchive archive, string name)
    {
        UcpEntry entry = UcpReader.Find(archive, name) ?? throw new InvalidDataException($"UDS archive has no {name}.");
        return JsonDocument.Parse(_ucp.ReadEntryText(archive, entry));
    }

    private static int ArrayLength(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Array ? value.GetArrayLength() : 0;

    private static string GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
}
