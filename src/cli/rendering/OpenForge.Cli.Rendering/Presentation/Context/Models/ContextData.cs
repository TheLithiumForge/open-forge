using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Shared.Content.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Context.Models;

[JsonConverter(typeof(OpenForge.Cli.Core.Presentation.Context.Shared.Rendering.ContextDataJsonConverter))]
internal sealed record ContextData
{
    public required IReadOnlyList<ContextDataSource> Sources { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<CliContentBlock> ContentBlocks { get; init; } = [];

    [JsonIgnore]
    internal string? Summary { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ContextDataLink>? Links { get; init; }

    [JsonIgnore]
    internal bool PathsOnly { get; init; }

    [JsonIgnore]
    internal bool EmptyAdditions { get; init; }

    [JsonIgnore]
    internal long? SourceCount { get; init; }

    [JsonIgnore]
    internal long? TokenCount { get; init; }

    [JsonIgnore]
    internal long? LinksFollowed { get; init; }

    [JsonIgnore]
    internal long? LinksNotFollowed { get; init; }
}

internal sealed record ContextDataSource
{
    public required string Path { get; init; }

    public string? Id { get; init; }

    public required string Layer { get; init; }

    public required IReadOnlyList<ContextDataPart> Parts { get; init; }

    public ContextDataSourceStandard? Standard { get; init; }

    public ContextDataSourceFull? Full { get; init; }
}

internal enum ContextDataPartKind
{
    Text,
    Headings,
    Paths,
    Metadata,
}

internal sealed record ContextDataSourceStandard
{
    public required IReadOnlyList<string> IncludedBecause { get; init; }

    public string? Route { get; init; }

    public string? Scope { get; init; }
}

internal sealed record ContextDataSourceFull
{
    public int? Order { get; init; }
}

internal sealed record ContextDataPart
{
    public required ContextDataPartKind Kind { get; init; }

    public required string Part { get; init; }

    public string? Text { get; init; }

    public IReadOnlyList<ContextDataHeading>? Headings { get; init; }

    public IReadOnlyList<string>? Paths { get; init; }

    public string? State { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<ContextDataMetadata> Metadata { get; init; } = [];
}

internal sealed record ContextDataMetadata
{
    public required string Name { get; init; }

    public required string Value { get; init; }
}

internal sealed record ContextDataHeading
{
    public required string Text { get; init; }

    public required int Level { get; init; }

    public int? Line { get; init; }
}

internal sealed record ContextDataLink
{
    public required string From { get; init; }

    public required ContextDataLocation Location { get; init; }

    public required string Destination { get; init; }

    public string? ResolvedPath { get; init; }

    public required string Resolution { get; init; }

    public required bool Followed { get; init; }
}

internal sealed record ContextDataLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }
}
