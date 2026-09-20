using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveBoundary
{
    internal static RouteMoveResultFormation Start(RouteMoveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new RouteMoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Source = new RouteMoveSource { Requested = request.SourceReference },
            Destination = new RouteMoveDestination { Requested = request.DestinationTarget },
            Subject = new RouteMoveSubject(),
            Ownership = new RouteMoveOwnership
            {
                State = RouteMoveOwnershipState.NotEstablished,
                Framework = RouteMoveOwnershipTrust.NotEstablished,
                Extensions = RouteMoveOwnershipTrust.NotEstablished,
            },
            References = new RouteMoveReferences
            {
                Coverage = RouteMoveCoverage.NotEstablished,
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            GeneratedNavigation = new RouteMoveGeneratedNavigation
            {
                Coverage = RouteMoveCoverage.NotEstablished,
            },
            Plan = new RouteMovePlanFacts
            {
                Completeness = RouteMovePlanCompleteness.NotEstablished,
                Safety = RouteMovePlanSafety.NotEstablished,
            },
            Recovery = new RouteMoveRecovery
            {
                State = request.IsDryRun
                    ? RouteMoveRecoveryState.NotCreated
                    : RouteMoveRecoveryState.NotRequired,
            },
            Verification = RouteMoveVerificationState.NotRequested,
        };
    }

    internal static RouteMoveResultFormation Stop(
        RouteMoveResultFormation formation,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(formation);
        return formation with
        {
            Plan = formation.Plan with
            {
                Completeness = status == CliSemanticStatus.Incomplete
                    ? RouteMovePlanCompleteness.Incomplete
                    : formation.Plan.Completeness,
                Safety = status == CliSemanticStatus.Blocked
                    ? RouteMovePlanSafety.Blocked
                    : formation.Plan.Safety,
            },
            Findings = [.. formation.Findings, new RouteMoveFinding(code, status, target, cause)],
        };
    }
}
