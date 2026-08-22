using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed class RouteListExplicitSourceResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteListExplicitSourceResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal PhysicalPathResolution ResolveCandidate(
        CliWorkspace workspace,
        string sourcePath)
    {
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            sourcePath);
    }

    internal RouteListSelectionResolution? ResolveOverwriteBase(
        RouteListRequest request,
        RouteListSelection attempted,
        RouteSource source)
    {
        var basePhysical = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            RouteLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, source.CanonicalPath));
        if (basePhysical.State == PhysicalPathState.Missing)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnsupportedSource,
                    attempted.AttemptedPath,
                    "The overwrite companion has no available base source."));
        }

        if (basePhysical.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(
                basePhysical.ResolvedPhysicalPath!,
                source.PhysicalPath))
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    attempted.AttemptedPath,
                    "The overwrite base source crosses an unproved physical boundary or has changed identity.")]);
        }

        return null;
    }

    internal RouteListSelectionResolution ResolveSource(
        RouteListSelection attempted,
        RouteSource source,
        RouteListRequest request,
        string requestedPath)
    {
        var physical = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            RouteLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, requestedPath));
        if (physical.State == PhysicalPathState.Missing)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnknownSource,
                    attempted.AttemptedId,
                    "The catalogue source path does not exist."));
        }

        if (physical.State != PhysicalPathState.Contained)
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    requestedPath,
                    "The catalogue source path crosses an unproved physical boundary.")]);
        }

        return ResolveContainedSource(attempted, source, physical, attempted.Kind);
    }

    internal RouteListSelectionResolution ResolveContainedSource(
        RouteListSelection attempted,
        RouteSource source,
        PhysicalPathResolution physical,
        RouteListSelectionKind selectionKind)
    {
        if (source.Kind == RouteSourceKind.Loader)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderSubject,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The Loader is only used for operand-free root selection."));
        }

        if (source.Kind is RouteSourceKind.Markdown or RouteSourceKind.Native
            && source.Metadata.State != RouteSourceMetadataState.Complete)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnsupportedSource,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The source is not a routed source."));
        }

        if (source.IsRouteAmbiguous)
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.RouteAmbiguous,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The selected route has ambiguous entrypoint meaning.")]);
        }

        var selectedOverwrite = selectionKind == RouteListSelectionKind.SourcePath
            && string.Equals(attempted.AttemptedPath, source.OverwritePath, StringComparison.Ordinal);
        if (!selectedOverwrite
            && !PhysicalIdentityTracker.PathComparer.Equals(
                physical.ResolvedPhysicalPath!,
                source.PhysicalPath))
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The selected source does not match its catalogue physical identity.")]);
        }

        var resolved = selectionKind == RouteListSelectionKind.SourceId
            ? RouteListSelectionFactory.ResolvedId(attempted.AttemptedId!, source)
            : RouteListSelectionFactory.ResolvedPath(attempted.AttemptedPath!, source);
        return RouteListSelectionResolutionFactory.Resolved(resolved, [source]);
    }

}
