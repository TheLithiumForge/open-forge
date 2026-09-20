using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;

internal sealed record RouteRemoveBindingInput
{
    public string? SourceReference { get; init; }

    public required IReadOnlyList<string> ParserErrors { get; init; }
}

internal sealed record RouteRemoveBindingFailure(
    RouteRemoveFindingCode Code,
    string Cause);

internal sealed record RouteRemoveBindingFacts
{
    public required string SourceReference { get; init; }

    public required RouteRemoveMode Mode { get; init; }
}

internal sealed record RouteRemoveBindingValidation
{
    private RouteRemoveBindingValidation(
        RouteRemoveBindingFacts? facts,
        RouteRemoveBindingFailure? failure)
    {
        if ((facts is null) == (failure is null))
        {
            throw new ArgumentException(
                "Route Remove binding validation must contain exactly one facts or failure value.");
        }

        Facts = facts;
        Failure = failure;
    }

    public RouteRemoveBindingFacts? Facts { get; }

    public RouteRemoveBindingFailure? Failure { get; }

    internal static RouteRemoveBindingValidation Valid(RouteRemoveBindingFacts facts)
        => new(facts, failure: null);

    internal static RouteRemoveBindingValidation Invalid(RouteRemoveBindingFailure failure)
        => new(facts: null, failure);
}

internal sealed record RouteRemoveInvalidResultInput
{
    public CliWorkspace? Workspace { get; init; }

    public required string SourceReference { get; init; }

    public required RouteRemoveMode Mode { get; init; }

    public required RouteRemoveBindingFailure Failure { get; init; }
}
