using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Result;

internal sealed class RouteInspectResultBuilder
{
    internal RouteInspectResult Build(
        RouteInspectRequest request,
        RouteInspectResolution resolution,
        RouteInspectProfile? profile)
    {
        if (resolution.State is RouteInspectResolutionState.Resolved
            or RouteInspectResolutionState.Incomplete)
        {
            if (profile is null)
            {
                throw new InvalidOperationException(
                    "A resolved or incomplete route-inspect resolution requires a profile.");
            }
        }

        var observations = ReadObservations(resolution);
        var conditions = resolution.Issues
            .Select(issue =>
            {
                var code = RouteInspectResultPolicy.ReadConditionCode(issue.Code, resolution.Selection);
                return new RouteInspectCondition(
                    code,
                    RouteInspectResultPolicy.ReadConditionStatus(code),
                    issue.Subject,
                    issue.Message,
                    issue.Paths);
            })
            .ToList();
        if (profile?.Completeness == RouteInspectCompleteness.Incomplete
            && !conditions.Any(condition => condition.Status == CliSemanticStatus.Incomplete))
        {
            conditions.Add(new RouteInspectCondition(
                RouteInspectConditionCode.UnavailableFact,
                CliSemanticStatus.Incomplete,
                resolution.Identity?.CanonicalWorkspaceRelativePath
                    ?? resolution.Selection.RequestedReference
                    ?? request.SourceReference,
                "One or more applicable route-inspect facts are unavailable."));
        }

        var status = RouteInspectResultPolicy.ReadStatus(
            resolution,
            profile,
            conditions,
            observations);
        var resultProfile = status == CliSemanticStatus.Blocked
            ? null
            : profile;
        var identity = status == CliSemanticStatus.Invalid ? null : resolution.Identity;
        var next = RouteInspectResultPolicy.ReadNextAction(status, resolution, conditions);
        return RouteInspectResult.Create(
            status,
            request.Workspace,
            resolution.Selection,
            identity,
            resultProfile,
            status == CliSemanticStatus.Invalid ? [] : observations,
            conditions,
            next);
    }

    private static IReadOnlyList<RouteInspectObservation> ReadObservations(
        RouteInspectResolution resolution)
    {
        if (resolution.State is not (
                RouteInspectResolutionState.Resolved
                or RouteInspectResolutionState.Incomplete))
        {
            return [];
        }

        var observations = new List<RouteInspectObservation>();
        var identity = resolution.ReadIdentity();
        var graph = resolution.ReadGraph();
        if (resolution.Selection.SelectionMethod is RouteInspectSelectionMethod.ExactPath
            or RouteInspectSelectionMethod.Interactive)
        {
            var collision = graph.ProjectionSet.IdentityCollisions
                .FirstOrDefault(candidate => candidate.Id == identity.Id);
            if (collision is not null)
            {
                observations.Add(new RouteInspectObservation(
                    RouteInspectObservationCode.AutomaticIdNotUnique,
                    identity.Id,
                    "The automatic source ID is not unique for the selected source.",
                    collision.Paths));
            }
        }

        if (identity.Form == RouteInspectSourceForm.CompatibilityEntrypoint)
        {
            observations.Add(new RouteInspectObservation(
                RouteInspectObservationCode.CompatibilityEntrypoint,
                identity.CanonicalWorkspaceRelativePath,
                "The selected source uses a compatibility entrypoint filename."));
        }

        if (identity.RouteState == RouteInspectRouteState.Detached)
        {
            observations.Add(new RouteInspectObservation(
                RouteInspectObservationCode.DetachedSource,
                identity.CanonicalWorkspaceRelativePath,
                "The selected entrypoint is detached from the Loader roots."));
        }

        if (identity.RouteState == RouteInspectRouteState.NotRouted)
        {
            observations.Add(new RouteInspectObservation(
                RouteInspectObservationCode.NotRouted,
                identity.CanonicalWorkspaceRelativePath,
                "The selected source has no established route."));
        }

        if (identity.PhysicalLayers.Count > 1)
        {
            observations.Add(new RouteInspectObservation(
                RouteInspectObservationCode.ValidOverwrite,
                identity.CanonicalWorkspaceRelativePath,
                "The selected source has a valid overwrite companion."));
        }

        return observations;
    }
}
