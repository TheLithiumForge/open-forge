using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed class RouteListExplicitSelectionResolver
{
    private readonly RouteListExplicitSourceResolver _sourceResolver;

    internal RouteListExplicitSelectionResolver(PhysicalPathResolver physicalPathResolver)
    {
        _sourceResolver = new RouteListExplicitSourceResolver(physicalPathResolver);
    }

    internal RouteListSelectionResolution Resolve(
        RouteSourceReferenceParseResult parsed,
        RouteListRequest request,
        RouteSourceCatalogue catalogue)
    {
        return parsed.Kind == RouteSourceReferenceKind.SourceId
            ? ResolveId(parsed.AttemptedId!, request, catalogue)
            : ResolvePath(parsed.AttemptedPath!, request, catalogue);
    }

    private RouteListSelectionResolution ResolveId(
        string attemptedId,
        RouteListRequest request,
        RouteSourceCatalogue catalogue)
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

        return _sourceResolver.ResolveSource(
            attempted,
            candidates[0],
            request,
            requestedPath: candidates[0].CanonicalPath);
    }

    private RouteListSelectionResolution ResolvePath(
        string attemptedPath,
        RouteListRequest request,
        RouteSourceCatalogue catalogue)
    {
        var attempted = RouteListSelectionFactory.AttemptedPath(attemptedPath);
        var sourcePath = RouteLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, attemptedPath);
        var physical = _sourceResolver.ResolveCandidate(request.Workspace, sourcePath);
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
            var baseResolution = _sourceResolver.ResolveOverwriteBase(request, attempted, source);
            if (baseResolution is not null)
            {
                return baseResolution;
            }
        }

        return _sourceResolver.ResolveContainedSource(
            attempted,
            source,
            physical,
            RouteListSelectionKind.SourcePath);
    }

}
