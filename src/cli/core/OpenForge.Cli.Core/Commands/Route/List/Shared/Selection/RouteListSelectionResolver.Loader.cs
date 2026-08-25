using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private static RouteListSelectionResolution ResolveLoaderRoots(
        SourceCatalogue catalogue,
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        var selection = RouteListSelectionFactory.LoaderRoots();
        var cancelled = WasCancelled(catalogue, projectionSet, routeFacts, cancellationToken);
        var rootPaths = routeFacts.Topology.LoaderRootPaths.ToHashSet(StringComparer.Ordinal);
        var loaderDestinations = ReadLoaderDestinations(projectionSet);
        var sourceIssues = routeFacts.Issues
            .Where(issue => issue.Code != SourceRouteIssueCode.RouteAmbiguous
                || rootPaths.Contains(issue.CanonicalPath))
            .OrderBy(issue => IsDeferredEntriesIssue(issue))
            .ThenBy(issue => issue.Occurrence)
            .ToArray();
        var issues = sourceIssues
            .Select(issue => MapLoaderIssue(issue, catalogue, projectionSet, loaderDestinations))
            .ToList();
        var selectedSources = new List<RouteSource>();

        foreach (var rootPath in routeFacts.Topology.LoaderRootPaths)
        {
            var projection = projectionSet.Projections.SingleOrDefault(candidate =>
                string.Equals(
                    candidate.LogicalSource.Identity.CanonicalBasePath,
                    rootPath,
                    StringComparison.Ordinal));
            if (projection?.BaseRead.Verification.State == SourceLayerVerificationState.Cancelled)
            {
                continue;
            }

            if (projection is null
                || projection.BaseRead.Verification.State != SourceLayerVerificationState.Verified
                || projection.Source is null)
            {
                AddLoaderProjectionIssue(rootPath, projection, issues);
                continue;
            }

            var source = projection.Source;
            if (source.Kind != RouteSourceKind.Entrypoint)
            {
                throw new ArgumentException("Every Loader root must project an entrypoint source.", nameof(routeFacts));
            }

            var routeFact = routeFacts.RouteFacts.SingleOrDefault(fact =>
                ReferenceEquals(fact.Identity, projection.LogicalSource.Identity));
            if (routeFact is null
                || routeFact.State is SourceRouteState.Unrouted or SourceRouteState.Unavailable)
            {
                issues.Add(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderUnavailable,
                    ReadLoaderAttempt(rootPath, loaderDestinations),
                    "The Loader root is unavailable in the route-fact allowlist."));
                continue;
            }

            if (routeFact.State == SourceRouteState.Ambiguous
                || HasAmbiguousEntrypoint(projection.LogicalSource, projectionSet))
            {
                var subject = ReadLoaderAttempt(rootPath, loaderDestinations);
                if (!issues.Any(issue => issue.Code == RouteListFindingCode.RouteAmbiguous
                    && string.Equals(issue.Subject, subject, StringComparison.Ordinal)))
                {
                    issues.Add(new RouteListSelectionIssue(
                        RouteListFindingCode.RouteAmbiguous,
                        subject,
                        "The Loader root has ambiguous entrypoint meaning."));
                }

                continue;
            }

            selectedSources.Add(source);
        }

        var loaderCandidate = catalogue.FindCandidateByPath(LoaderPath);
        if (!cancelled && loaderCandidate is null)
        {
            foreach (var rootIssue in catalogue.Issues.Where(issue =>
                         issue.Stage == SourceCatalogueIssueStage.Root
                         && string.Equals(issue.AttemptedCanonicalPath, ".agents", StringComparison.Ordinal)))
            {
                issues.Add(MapCatalogueRootIssue(rootIssue));
            }
        }

        if (!cancelled
            && loaderCandidate?.Form != SourceDocumentForm.Loader
            && !issues.Any(issue => string.Equals(issue.Subject, LoaderPath, StringComparison.Ordinal)
                && (issue.Code is RouteListFindingCode.LoaderUnavailable or RouteListFindingCode.PhysicalBoundary)))
        {
            issues.Add(new RouteListSelectionIssue(
                RouteListFindingCode.LoaderUnavailable,
                LoaderPath,
                "The Loader file is missing."));
        }

        if (cancelled)
        {
            issues.Add(new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                LoaderPath,
                "Loader root resolution was interrupted."));
            return RouteListSelectionResolutionFactory.Interrupted(selection, selectedSources, issues);
        }

        if (!routeFacts.AreLoaderRootFactsComplete
            && !issues.Any(issue => issue.Status is CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked))
        {
            issues.Add(new RouteListSelectionIssue(
                RouteListFindingCode.LoaderUnavailable,
                LoaderPath,
                "The Loader root boundary is incomplete."));
        }

        if (issues.Any(issue => issue.Status == CliSemanticStatus.Blocked))
        {
            return RouteListSelectionResolutionFactory.Blocked(selection, selectedSources, issues);
        }

        return issues.Count == 0
            ? RouteListSelectionResolutionFactory.Resolved(selection, selectedSources)
            : RouteListSelectionResolutionFactory.Incomplete(selection, selectedSources, issues);
    }

    private static void AddLoaderProjectionIssue(
        string rootPath,
        RouteSourceProjection? projection,
        ICollection<RouteListSelectionIssue> issues)
    {
        var verificationState = projection?.BaseRead.Verification.State;
        if (verificationState == SourceLayerVerificationState.Cancelled)
        {
            return;
        }

        if (verificationState is SourceLayerVerificationState.Unsafe
            or SourceLayerVerificationState.Unavailable
            or SourceLayerVerificationState.Changed)
        {
            issues.Add(new RouteListSelectionIssue(
                RouteListFindingCode.PhysicalBoundary,
                ReadLoaderDestination(rootPath),
                "The Loader root crosses an unproved physical boundary or has changed identity."));
            return;
        }

        issues.Add(new RouteListSelectionIssue(
            RouteListFindingCode.LoaderUnavailable,
            ReadLoaderDestination(rootPath),
            "The Loader root is unavailable in the Route projection allowlist."));
    }

}
