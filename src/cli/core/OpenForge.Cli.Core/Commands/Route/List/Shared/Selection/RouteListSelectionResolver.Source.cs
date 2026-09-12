using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private static RouteListSelectionResolution ResolveSource(
        RouteListSelection attempted,
        SourceLogicalSource logicalSource,
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts,
        RouteListSelectionKind selectionKind)
    {
        var projection = projectionSet.Projections.SingleOrDefault(candidate =>
            ReferenceEquals(candidate.LogicalSource, logicalSource));
        var source = projection?.Source;
        if (source is null
            || !ReferenceEquals(projectionSet.FindByPath(source.CanonicalPath), source)
            || selectionKind == RouteListSelectionKind.SourcePath
                && attempted.AttemptedPath is { } attemptedPath
                && !ReferenceEquals(projectionSet.FindByPath(attemptedPath), source))
        {
            return Invalid(
                attempted,
                RouteListFindingCode.UnsupportedSource,
                attempted.AttemptedId ?? attempted.AttemptedPath,
                "The selected source is unavailable in the Route projection allowlist.");
        }

        if (source.Kind == RouteSourceKind.Loader)
        {
            return Invalid(
                attempted,
                RouteListFindingCode.LoaderSubject,
                attempted.AttemptedId ?? attempted.AttemptedPath,
                "The Loader is only used for operand-free root selection.");
        }

        var routeFact = routeFacts.RouteFacts.SingleOrDefault(fact =>
            ReferenceEquals(fact.Identity, logicalSource.Identity));
        if (routeFact is null)
        {
            return Invalid(
                attempted,
                RouteListFindingCode.UnsupportedSource,
                attempted.AttemptedId ?? attempted.AttemptedPath,
                "The selected source is unavailable in the route-fact allowlist.");
        }

        if (source.IsRouteAmbiguous
            || routeFact.State == SourceRouteState.Ambiguous
            || HasAmbiguousEntrypoint(logicalSource, projectionSet))
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.RouteAmbiguous,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The selected route has ambiguous entrypoint meaning.")]);
        }

        if ((source.Kind is RouteSourceKind.Markdown or RouteSourceKind.Native)
            && (source.Metadata.State != RouteSourceMetadataState.Complete
                || routeFact.State != SourceRouteState.Routed))
        {
            return Invalid(
                attempted,
                RouteListFindingCode.UnsupportedSource,
                attempted.AttemptedId ?? attempted.AttemptedPath,
                "The source is not a routed source.");
        }

        var resolved = selectionKind == RouteListSelectionKind.SourceId
            ? RouteListSelectionFactory.ResolvedId(
                attempted.AttemptedId ?? throw new InvalidOperationException("A resolved ID selection requires its attempted ID."),
                source)
            : RouteListSelectionFactory.ResolvedPath(
                attempted.AttemptedPath ?? throw new InvalidOperationException("A resolved path selection requires its attempted path."),
                source);
        return RouteListSelectionResolutionFactory.Resolved(resolved, [source]);
    }

    private static RouteListSelectionResolution? ValidatePhysicalIdentity(
        RouteListSelection attempted,
        string subject,
        SourceCandidate candidate,
        SourceLayer layer,
        PhysicalPathResolution physical)
    {
        if (physical.State != PhysicalPathState.Contained
            || physical.ResolvedPhysicalPath is not { } currentPhysicalPath
            || candidate.PhysicalState != PhysicalPathState.Contained
            || candidate.PhysicalPath is not { } candidatePhysicalPath
            || !PhysicalIdentityTracker.PathComparer.Equals(currentPhysicalPath, candidatePhysicalPath)
            || !PhysicalIdentityTracker.PathComparer.Equals(currentPhysicalPath, layer.PhysicalPath))
        {
            return PhysicalBoundary(
                attempted,
                subject,
                "The selected source crosses an unproved physical boundary or has changed identity.");
        }

        return null;
    }

    private static bool MatchesCandidatePhysicalIdentity(
        SourceCandidate candidate,
        PhysicalPathResolution physical)
    {
        return physical.State == PhysicalPathState.Contained
            && physical.ResolvedPhysicalPath is { } currentPhysicalPath
            && candidate.PhysicalState == PhysicalPathState.Contained
            && candidate.PhysicalPath is { } candidatePhysicalPath
            && PhysicalIdentityTracker.PathComparer.Equals(currentPhysicalPath, candidatePhysicalPath);
    }

    private PhysicalPathResolution ResolveCandidate(RouteListRequest request, string canonicalPath)
    {
        return _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, canonicalPath));
    }

    private static bool HasAmbiguousEntrypoint(
        SourceLogicalSource logicalSource,
        RouteSourceProjectionSet projectionSet)
    {
        if (!SourceFormClassifier.IsEntrypoint(logicalSource.Base.Form))
        {
            return false;
        }

        var directory = SourceLogicalPath.ReadParent(logicalSource.Identity.CanonicalBasePath);
        return projectionSet.Projections.Count(projection =>
            SourceFormClassifier.IsEntrypoint(projection.LogicalSource.Base.Form)
            && string.Equals(
                SourceLogicalPath.ReadParent(projection.LogicalSource.Identity.CanonicalBasePath),
                directory,
                StringComparison.Ordinal)) > 1;
    }

    private static RouteListSelectionResolution Invalid(
        RouteListSelection selection,
        RouteListFindingCode code,
        string? subject,
        string cause)
    {
        return RouteListSelectionResolutionFactory.Invalid(
            selection,
            new RouteListSelectionIssue(code, subject, cause));
    }

    private static RouteListSelectionResolution PhysicalBoundary(
        RouteListSelection selection,
        string? subject,
        string cause)
    {
        return RouteListSelectionResolutionFactory.Blocked(
            selection,
            [],
            [new RouteListSelectionIssue(RouteListFindingCode.PhysicalBoundary, subject, cause)]);
    }
}
