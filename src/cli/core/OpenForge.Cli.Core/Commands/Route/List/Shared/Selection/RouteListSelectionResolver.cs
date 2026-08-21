using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed class RouteListSelectionResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteListSelectionResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal async ValueTask<RouteListSelectionResolution> ResolveAsync(
        RouteListRequest request,
        RouteListSourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(catalogue);

        var parsed = RouteListSourceReferenceParser.Parse(request.SourceReference);
        if (parsed.State == RouteListSourceReferenceParseState.Invalid)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                CreateSelection(parsed),
                new RouteListSelectionIssue(
                    RouteListFindingCode.InvalidSourceReference,
                    parsed.AttemptedId ?? parsed.AttemptedPath,
                    parsed.Cause!));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(CreateSelection(parsed), parsed.AttemptedId ?? parsed.AttemptedPath ?? ".agents/loader.md");
        }

        if (parsed.Kind == RouteListSourceReferenceKind.LoaderRoots)
        {
            var loader = await new LoaderDestinationResolver(_physicalPathResolver)
                .ResolveAsync(request.Workspace, catalogue, cancellationToken)
                .ConfigureAwait(false);
            return FromLoaderResolution(loader);
        }

        return ResolveExplicit(parsed, request, catalogue);
    }

    private RouteListSelectionResolution ResolveExplicit(
        RouteListSourceReferenceParseResult parsed,
        RouteListRequest request,
        RouteListSourceCatalogue catalogue)
    {
        if (parsed.Kind == RouteListSourceReferenceKind.SourceId)
        {
            return ResolveId(parsed.AttemptedId!, request, catalogue);
        }

        return ResolvePath(parsed.AttemptedPath!, request, catalogue);
    }

    private RouteListSelectionResolution ResolveId(
        string attemptedId,
        RouteListRequest request,
        RouteListSourceCatalogue catalogue)
    {
        var attempted = RouteListSelectionFactory.AttemptedId(attemptedId);
        var candidates = catalogue.FindById(attemptedId);
        if (candidates.Count == 0)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnknownSource,
                    attemptedId,
                    "The source ID does not identify a current source."));
        }

        if (candidates.Count > 1)
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.AmbiguousSource,
                    attemptedId,
                    "The source ID identifies more than one current source.",
                    candidates.Select(source => source.CanonicalPath))]);
        }

        return ResolveSource(
            attempted,
            candidates[0],
            request,
            requestedPath: candidates[0].CanonicalPath);
    }

    private RouteListSelectionResolution ResolvePath(
        string attemptedPath,
        RouteListRequest request,
        RouteListSourceCatalogue catalogue)
    {
        var attempted = RouteListSelectionFactory.AttemptedPath(attemptedPath);
        var sourcePath = CombineWorkspacePath(request.Workspace.LexicalRoot, attemptedPath);
        var physical = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            sourcePath);
        if (physical.State == PhysicalPathState.Missing)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnknownSource,
                    attemptedPath,
                    "The exact source path does not exist."));
        }

        if (physical.State != PhysicalPathState.Contained)
        {
            return RouteListSelectionResolutionFactory.Blocked(
                attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    attemptedPath,
                    "The exact source path crosses an unproved physical boundary.")]);
        }

        var source = catalogue.FindByPath(attemptedPath);
        if (source is null)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.UnsupportedSource,
                    attemptedPath,
                    "The exact contained path is not a recognized catalogue source."));
        }

        if (string.Equals(attemptedPath, source.OverwritePath, StringComparison.Ordinal))
        {
            var baseResolution = ResolveOverwriteBase(request, attempted, source);
            if (baseResolution is not null)
            {
                return baseResolution;
            }
        }

        return ResolveContainedSource(
            attempted,
            source,
            physical,
            RouteListSelectionKind.SourcePath);
    }

    private RouteListSelectionResolution? ResolveOverwriteBase(
        RouteListRequest request,
        RouteListSelection attempted,
        RouteListSource source)
    {
        var basePhysical = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            CombineWorkspacePath(request.Workspace.LexicalRoot, source.CanonicalPath));
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

    private RouteListSelectionResolution ResolveSource(
        RouteListSelection attempted,
        RouteListSource source,
        RouteListRequest request,
        string requestedPath)
    {
        var physical = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            CombineWorkspacePath(request.Workspace.LexicalRoot, requestedPath));
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

    private static RouteListSelectionResolution ResolveContainedSource(
        RouteListSelection attempted,
        RouteListSource source,
        PhysicalPathResolution physical,
        RouteListSelectionKind selectionKind)
    {
        if (source.Kind == RouteListSourceKind.Loader)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                attempted,
                new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderSubject,
                    attempted.AttemptedId ?? attempted.AttemptedPath,
                    "The Loader is only used for operand-free root selection."));
        }

        if (source.Kind == RouteListSourceKind.Unrouted)
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

    private static RouteListSelection CreateSelection(RouteListSourceReferenceParseResult parsed)
    {
        return parsed.Kind switch
        {
            RouteListSourceReferenceKind.LoaderRoots => RouteListSelectionFactory.LoaderRoots(),
            RouteListSourceReferenceKind.SourceId => RouteListSelectionFactory.AttemptedId(parsed.AttemptedId!),
            RouteListSourceReferenceKind.SourcePath => RouteListSelectionFactory.AttemptedPath(parsed.AttemptedPath!),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source reference kind is not defined."),
        };
    }

    private static RouteListSelectionResolution FromLoaderResolution(
        LoaderDestinationResolution loader)
    {
        var selection = RouteListSelectionFactory.LoaderRoots();
        return loader.State switch
        {
            LoaderDestinationResolutionState.Resolved => RouteListSelectionResolutionFactory.Resolved(
                selection,
                loader.SelectedSources),
            LoaderDestinationResolutionState.Blocked => RouteListSelectionResolutionFactory.Blocked(
                selection,
                loader.SelectedSources,
                loader.Issues),
            LoaderDestinationResolutionState.Incomplete => RouteListSelectionResolutionFactory.Incomplete(
                selection,
                loader.SelectedSources,
                loader.Issues),
            LoaderDestinationResolutionState.Interrupted => RouteListSelectionResolutionFactory.Interrupted(
                selection,
                loader.SelectedSources,
                loader.Issues),
            _ => throw new ArgumentOutOfRangeException(nameof(loader), loader.State, "The Loader resolution state is not defined."),
        };
    }

    private static RouteListSelectionResolution Interrupted(
        RouteListSelection selection,
        string subject)
    {
        return RouteListSelectionResolutionFactory.Interrupted(
            selection,
            [],
            [new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Source selection was interrupted.")]);
    }

    private static string CombineWorkspacePath(string workspaceRoot, string logicalPath)
    {
        var relative = logicalPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(workspaceRoot, relative);
    }
}
