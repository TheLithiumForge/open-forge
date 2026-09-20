using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Content.Models;

internal enum CliContentPartKind
{
    Text,
    Headings,
    Paths,
    Metadata,
}

internal sealed record CliContentHeading
{
    public required string Text { get; init; }

    public required int Level { get; init; }

    public int? Line { get; init; }
}

internal sealed record CliContentMetadata
{
    public required string Name { get; init; }

    public required string Value { get; init; }
}

internal sealed record CliContentPart
{
    public required string Name { get; init; }

    public required CliContentPartKind Kind { get; init; }

    public string? State { get; init; }

    public CliAuthoredSpan? AuthoredText { get; init; }

    public IReadOnlyList<CliContentHeading> Headings { get; init; } = [];

    public IReadOnlyList<string> Paths { get; init; } = [];

    public IReadOnlyList<CliContentMetadata> Metadata { get; init; } = [];
}

internal sealed record CliContentBlock
{
    public required string Path { get; init; }

    public string? Id { get; init; }

    public string? DelimiterLayer { get; init; }

    public IReadOnlyList<CliContentPart> Parts { get; init; } = [];

    public IReadOnlyList<string> IncludedBecause { get; init; } = [];

    public string? Route { get; init; }

    public string? Scope { get; init; }

    public int? Order { get; init; }

    public string? Layer { get; init; }
}
