using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceFactsResolver
{
    private static void AddRouteIssues(
        RouteInspectSourceResolutionInput input,
        ICollection<RouteInspectResolutionIssue> issues)
    {
        foreach (var issue in input.Resolution.RouteFacts.Issues)
        {
            var mapped = MapRouteIssue(input, issue);
            if (mapped is not null)
            {
                issues.Add(mapped);
            }
        }

        foreach (var rootPath in input.Resolution.RouteFacts.Topology.LoaderRootPaths)
        {
            var root = input.Resolution.Projections.Projections
                .Select(projection => projection.LogicalSource)
                .Single(source => string.Equals(
                    source.Identity.CanonicalBasePath,
                    rootPath,
                    StringComparison.Ordinal));
            if (HasDuplicateEntrypoint(root, input.Resolution.Projections.ProjectionSet))
            {
                issues.Add(RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.AmbiguousRoute,
                    rootPath,
                    "The Loader destination has ambiguous authored route meaning."));
            }
        }
    }

    private static RouteInspectResolutionIssue? MapRouteIssue(
        RouteInspectSourceResolutionInput input,
        SourceRouteIssue issue)
    {
        return issue.Code switch
        {
            SourceRouteIssueCode.LoaderUnavailable => RouteInspectResolutionSupport.CreateIssue(
                ReadLoaderUnavailableCode(input, issue),
                issue.CanonicalPath,
                issue.Cause),
            SourceRouteIssueCode.LoaderMalformed => RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.IncompleteRoute,
                issue.CanonicalPath,
                issue.Cause),
            SourceRouteIssueCode.LoaderUnsafe => RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.UnsafeSource,
                issue.CanonicalPath,
                issue.Cause),
            SourceRouteIssueCode.RouteAmbiguous
                when string.Equals(
                    issue.CanonicalPath,
                    input.LogicalSource.Identity.CanonicalBasePath,
                    StringComparison.Ordinal) => RouteInspectResolutionSupport.CreateIssue(
                        RouteInspectResolutionIssueCode.AmbiguousRoute,
                        input.RequestedPath,
                        "The selected source has ambiguous authored route meaning."),
            SourceRouteIssueCode.RouteSupportUnavailable
                when IsSelectedSupportIssue(input, issue) => RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.IncompleteRoute,
                    issue.CanonicalPath,
                    issue.Cause),
            _ => null,
        };
    }

    private static RouteInspectResolutionIssueCode ReadLoaderUnavailableCode(
        RouteInspectSourceResolutionInput input,
        SourceRouteIssue issue)
    {
        if (string.Equals(issue.CanonicalPath, SourceLogicalPath.LoaderPath, StringComparison.Ordinal))
        {
            return RouteInspectResolutionIssueCode.ReadUnavailable;
        }

        var candidate = input.Resolution.Catalogue.FindCandidateByPath(issue.CanonicalPath);
        var isUnsafe = candidate?.PhysicalState is PhysicalPathState.External
            or PhysicalPathState.Dangling
            or PhysicalPathState.Cycle;
        return isUnsafe ? RouteInspectResolutionIssueCode.UnsafeSource
            : RouteInspectResolutionIssueCode.IncompleteRoute;
    }

    private static bool IsSelectedSupportIssue(
        RouteInspectSourceResolutionInput input,
        SourceRouteIssue issue)
    {
        var selectedPath = input.LogicalSource.Identity.CanonicalBasePath;
        return string.Equals(issue.CanonicalPath, selectedPath, StringComparison.Ordinal)
            || issue.RelatedPaths.Contains(selectedPath, StringComparer.Ordinal);
    }
}
