using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Models;

internal sealed record CleanupData
{
    public required string Mode { get; init; }

    public required IReadOnlyList<CleanupDataItem> Items { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CleanupDataNotEligible>? NotEligible { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Lock { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FinalCheck { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<CleanupDataTextRow> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record CleanupDataItem
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Outcome { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Origin { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Integrity { get; init; }
}

internal sealed record CleanupDataNotEligible
{
    public required string Path { get; init; }

    public required string Reason { get; init; }
}

internal sealed record CleanupDataTextRow(
    string Path,
    string Wording,
    string? Detail = null);
