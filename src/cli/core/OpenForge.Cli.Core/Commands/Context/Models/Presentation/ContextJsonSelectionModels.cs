namespace OpenForge.Cli.Core.Commands.Context.Models.Presentation;

internal sealed class ContextJsonSelection
{
    public required ContextJsonRequestedSource[] RequestedSources { get; init; }

    public required bool StartupIncluded { get; init; }

    public required bool AdditionsOnly { get; init; }

    public required ContextJsonLinkExpansion LinkExpansion { get; init; }

    public required int? SourceCount { get; init; }
}

internal sealed class ContextJsonRequestedSource
{
    public required string Supplied { get; init; }

    public required string Form { get; init; }

    public required string Resolution { get; init; }

    public required ContextJsonSourceIdentity? Source { get; init; }

    public required string? RouteState { get; init; }

    public required ContextJsonSourceIdentity[] Candidates { get; init; }
}

internal sealed class ContextJsonLinkExpansion
{
    public required string Mode { get; init; }

    public required int? Depth { get; init; }
}

internal sealed class ContextJsonPresentation
{
    public required ContextJsonView View { get; init; }

    public required ContextJsonContent Content { get; init; }
}

internal sealed class ContextJsonView
{
    public required string? Supplied { get; init; }

    public required string Effective { get; init; }
}

internal sealed class ContextJsonContent
{
    public required string[] Supplied { get; init; }

    public required string[] Effective { get; init; }
}

internal sealed class ContextJsonCoverage
{
    public required string State { get; init; }

    public required string Selection { get; init; }

    public required string Links { get; init; }

    public required string Projection { get; init; }
}
