using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveBoundary
{
    internal static RouteRemoveResultFormation Start(RouteRemoveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new RouteRemoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Source = new RouteRemoveSource { Requested = request.SourceReference },
            Subject = new RouteRemoveSubject(),
            Ownership = new RouteRemoveOwnership
            {
                State = RouteRemoveOwnershipState.NotEstablished,
                Framework = RouteRemoveOwnershipTrust.NotEstablished,
                Extensions = RouteRemoveOwnershipTrust.NotEstablished,
            },
            References = new RouteRemoveReferences
            {
                Coverage = RouteRemoveCoverage.NotEstablished,
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            GeneratedNavigation = new RouteRemoveGeneratedNavigation
            {
                Coverage = RouteRemoveCoverage.NotEstablished,
            },
            Plan = new RouteRemovePlanFacts
            {
                Completeness = RouteRemovePlanCompleteness.NotEstablished,
                Safety = RouteRemovePlanSafety.NotEstablished,
            },
            Recovery = new RouteRemoveRecovery
            {
                State = request.IsDryRun
                    ? RouteRemoveRecoveryState.NotCreated
                    : RouteRemoveRecoveryState.NotRequired,
            },
            Verification = RouteRemoveVerificationState.NotRequested,
        };
    }

    internal static RouteRemoveResultFormation Stop(
        RouteRemoveResultFormation formation,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause,
        SourceLocation? location = null)
    {
        ArgumentNullException.ThrowIfNull(formation);
        return formation with
        {
            Plan = formation.Plan with
            {
                Completeness = status == CliSemanticStatus.Incomplete
                    ? RouteRemovePlanCompleteness.Incomplete
                    : formation.Plan.Completeness,
                Safety = status == CliSemanticStatus.Blocked
                    ? RouteRemovePlanSafety.Blocked
                    : formation.Plan.Safety,
            },
            Findings = [.. formation.Findings, new RouteRemoveFinding(code, status, target, cause, location)],
        };
    }
}
