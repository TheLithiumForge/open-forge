using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Shared.Content.Models;

namespace OpenForge.Cli.Core.Presentation.Find.Models;

internal sealed record FindData
{
    public required IReadOnlyList<FindDataMatch> Matches { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FindDataQuery? Query { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FindDataSourceSet? SourceSet { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<CliContentBlock> ContentBlocks { get; init; } = [];

    [JsonIgnore]
    internal bool PrependBlankLine { get; init; }
}

internal sealed record FindDataMatch
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public string? Description { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<FindDataEvidence>? Evidence { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<FindDataPart>? Parts { get; init; }
}

internal sealed record FindDataEvidence
{
    public required string Kind { get; init; }

    public required string Value { get; init; }

    public required string Region { get; init; }

    public required string Layer { get; init; }
}

internal sealed record FindDataPart
{
    public required string Part { get; init; }

    public string? Name { get; init; }

    public string? Layer { get; init; }

    public string? Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; init; }

    public string? Text { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<FindDataHeading>? Headings { get; init; }
}

internal sealed record FindDataHeading
{
    public required string Text { get; init; }

    public required int Level { get; init; }

    public int? Line { get; init; }
}

internal sealed record FindDataQuery
{
    public required IReadOnlyList<string> Tags { get; init; }

    public required IReadOnlyList<string> Headings { get; init; }

    public required string Require { get; init; }

    public required IReadOnlyList<string> Within { get; init; }
}

internal sealed record FindDataSourceSet
{
    public required string Mode { get; init; }

    public required IReadOnlyList<string> Include { get; init; }

    public required IReadOnlyList<string> Exclude { get; init; }

    public int? Inspected { get; init; }

    public int? Candidates { get; init; }
}
