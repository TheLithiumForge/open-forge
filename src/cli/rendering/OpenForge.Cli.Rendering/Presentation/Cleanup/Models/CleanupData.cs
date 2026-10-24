using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Models;

internal sealed record CleanupData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName(CleanupDataJsonFields.Stores)]
    public IReadOnlyList<CleanupDataStore>? Stores { get; init; }

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
    [JsonPropertyName(CleanupDataJsonFields.Path)]
    public required string Path { get; init; }

    [JsonPropertyName(CleanupDataJsonFields.Kind)]
    public required string Kind { get; init; }

    [JsonPropertyName(CleanupDataJsonFields.Outcome)]
    public required string Outcome { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Origin { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName(CleanupDataJsonFields.Integrity)]
    public string? Integrity { get; init; }
}

internal sealed record CleanupDataNotEligible
{
    [JsonPropertyName(CleanupDataJsonFields.Path)]
    public required string Path { get; init; }

    public required string Reason { get; init; }
}

internal sealed record CleanupDataTextRow(
    string Path,
    string Wording,
    string? Detail = null);

internal sealed record CleanupDataStore(
    [property: JsonPropertyName(CleanupDataJsonFields.Kind)] string Kind,
    [property: JsonPropertyName(CleanupDataJsonFields.Path)] string? Path,
    [property: JsonPropertyName(CleanupDataJsonFields.Coverage)] string Coverage,
    string Safety,
    [property: JsonPropertyName(CleanupDataJsonFields.Candidates)] IReadOnlyList<CleanupDataItem> Candidates,
    [property: JsonPropertyName(CleanupDataJsonFields.Cause)] string? Cause,
    string Revalidation, string Verification);
