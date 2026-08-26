namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextJsonLink
{
    public required int Depth { get; init; }

    public required ContextJsonLinkSource Source { get; init; }

    public required ContextJsonLocation Location { get; init; }

    public required ContextJsonLocation? DestinationLocation { get; init; }

    public required string RawDestination { get; init; }

    public required string? Fragment { get; init; }

    public required ContextJsonLinkTarget Target { get; init; }

    public required string Disposition { get; init; }
}

internal sealed class ContextJsonLinkSource
{
    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required string Layer { get; init; }
}

internal sealed class ContextJsonLinkTarget
{
    public required string Kind { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Layer { get; init; }

    public required string Resolution { get; init; }

    public required string? Network { get; init; }
}
