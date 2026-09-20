using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;

namespace OpenForge.Cli.Core.Presentation.References.Models;

internal sealed record ReferencesData
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReferencesDataSource? Source { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Direction { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ReferencesDataIncoming>? Incoming { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ReferencesDataOutgoing>? Outgoing { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReferencesDataCoverage? Coverage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReferencesDataFilters? Filters { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ReferencesDataScannedSource>? Scanned { get; init; }

    /// <summary>The source path, printed as the headline when the scan produced rows.</summary>
    [JsonIgnore]
    internal string? SourcePath { get; init; }

    /// <summary>`standard` and above resolve each outgoing destination and name the filters.</summary>
    [JsonIgnore]
    internal bool ShowResolvedTargets { get; init; }

    /// <summary>`full` and above name the layer of each occurrence and the scanned sources.</summary>
    [JsonIgnore]
    internal bool ShowLayers { get; init; }
}

internal sealed record ReferencesDataSource
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record ReferencesDataIncoming
{
    public required string Path { get; init; }
    public required string Location { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Layer { get; init; }
}

internal sealed record ReferencesDataOutgoing
{
    public required string Location { get; init; }
    public required string Destination { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResolvedPath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Layer { get; init; }

    /// <summary>The row state word, absent when the link is fine.</summary>
    [JsonIgnore]
    internal string? RowState { get; init; }

    [JsonIgnore]
    internal ReferencesTargetKind TargetKind { get; init; }
}

internal sealed record ReferencesDataCoverage
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Incoming { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Outgoing { get; init; }
}

internal sealed record ReferencesDataFilters
{
    public required IReadOnlyList<string> Include { get; init; }
    public required IReadOnlyList<string> Exclude { get; init; }
}

internal sealed record ReferencesDataScannedSource
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }

    public required string Path { get; init; }
    public required string Layer { get; init; }
}
