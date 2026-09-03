using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;

internal sealed record RouteMoveBindingInput
{
    public string? SourceReference { get; init; }

    public string? DestinationTarget { get; init; }

    public required IReadOnlyList<string> ParserErrors { get; init; }
}

internal sealed record RouteMoveBindingFailure(
    RouteMoveFindingCode Code,
    string Cause);

internal sealed record RouteMoveBindingFacts
{
    public required string SourceReference { get; init; }

    public required string DestinationTarget { get; init; }

    public required RouteMoveMode Mode { get; init; }
}

internal sealed record RouteMoveBindingValidation
{
    private RouteMoveBindingValidation(
        RouteMoveBindingFacts? facts,
        RouteMoveBindingFailure? failure)
    {
        if ((facts is null) == (failure is null))
        {
            throw new ArgumentException(
                "Route Move binding validation must contain exactly one facts or failure value.");
        }

        Facts = facts;
        Failure = failure;
    }

    public RouteMoveBindingFacts? Facts { get; }

    public RouteMoveBindingFailure? Failure { get; }

    internal static RouteMoveBindingValidation Valid(RouteMoveBindingFacts facts)
        => new(facts, failure: null);

    internal static RouteMoveBindingValidation Invalid(RouteMoveBindingFailure failure)
        => new(facts: null, failure);
}

internal sealed record RouteMoveInvalidResultInput
{
    public CliWorkspace? Workspace { get; init; }

    public required string SourceReference { get; init; }

    public required string DestinationTarget { get; init; }

    public required RouteMoveMode Mode { get; init; }

    public required RouteMoveBindingFailure Failure { get; init; }
}
