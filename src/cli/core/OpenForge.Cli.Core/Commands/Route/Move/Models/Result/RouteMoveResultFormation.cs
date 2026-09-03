using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

internal sealed record RouteMoveResultFormation
{
    public CliWorkspace? Workspace { get; init; }

    public required RouteMoveMode Mode { get; init; }

    public required RouteMoveSource Source { get; init; }

    public required RouteMoveDestination Destination { get; init; }

    public required RouteMoveSubject Subject { get; init; }

    public required RouteMoveOwnership Ownership { get; init; }

    public required RouteMoveReferences References { get; init; }

    public required RouteMoveGeneratedNavigation GeneratedNavigation { get; init; }

    public required RouteMovePlanFacts Plan { get; init; }

    public ImmutableArray<RouteMoveEffect> Effects { get; init; } = [];

    public ImmutableArray<string> UnchangedPaths { get; init; } = [];

    public required RouteMoveRecovery Recovery { get; init; }

    public required RouteMoveVerificationState Verification { get; init; }

    public ImmutableArray<RouteMoveFinding> Findings { get; init; } = [];
}
