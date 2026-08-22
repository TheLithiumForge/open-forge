using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectSourceSelectionResolver
{
    private readonly RouteInspectPhysicalVerifier _physicalVerifier;
    private readonly RouteInspectSourceFactsResolver _sourceFactsResolver;

    internal RouteInspectSourceSelectionResolver(
        RouteInspectPhysicalVerifier physicalVerifier,
        RouteInspectSourceFactsResolver sourceFactsResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalVerifier);
        ArgumentNullException.ThrowIfNull(sourceFactsResolver);
        _physicalVerifier = physicalVerifier;
        _sourceFactsResolver = sourceFactsResolver;
    }
    internal RouteInspectResolution Resolve(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                input.UnresolvedSelection,
                input.Parsed.AttemptedId ?? input.Parsed.AttemptedPath!);
        }
        return input.Parsed.Kind == RouteSourceReferenceKind.SourceId
            ? ResolveId(input, cancellationToken)
            : ResolvePath(input, cancellationToken);
    }
    private RouteInspectResolution ResolveId(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        var requestedId = input.Parsed.AttemptedId!;
        var safeCandidates = input.Catalogue.FindById(requestedId);
        var unsafeCandidates = input.UnsafePaths
            .Where(path => string.Equals(RouteSourceIdentity.DeriveId(path), requestedId, StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var candidatePaths = safeCandidates
            .Select(source => source.CanonicalPath)
            .Concat(unsafeCandidates)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.UnresolvedSelection, requestedId);
        }
        if (candidatePaths.Length == 0)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                input.UnresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnknownSource,
                    requestedId,
                    "The source ID does not identify a current source.")]);
        }
        if (candidatePaths.Length > 1)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                new RouteInspectSelection(
                    RouteInspectReferenceKind.SourceId,
                    RouteInspectSelectionMethod.Unresolved,
                    requestedId,
                    candidatePaths),
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.AmbiguousSource,
                    requestedId,
                    "The source ID identifies more than one current source.",
                    candidatePaths)]);
        }
        if (unsafeCandidates.Length != 0)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                input.UnresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    requestedId,
                    "The selected source crosses an unproved physical boundary.")]);
        }
        var source = safeCandidates.Single();
        var selection = new RouteInspectSelection(
            RouteInspectReferenceKind.SourceId,
            RouteInspectSelectionMethod.AutomaticId,
            requestedId,
            []);
        return ResolveSource(input, selection, source, source.CanonicalPath, cancellationToken);
    }
    private RouteInspectResolution ResolvePath(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        var requestedPath = input.Parsed.AttemptedPath!;
        var source = input.Catalogue.FindByPath(requestedPath);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.UnresolvedSelection, requestedPath);
        }
        if (source is null)
        {
            var overwrite = input.Catalogue.FindOverwriteByPath(requestedPath);
            if (cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(input.UnresolvedSelection, requestedPath);
            }
            if (overwrite is not null)
            {
                return RouteInspectOverwriteResolutionPolicy.Blocked(
                    input.UnresolvedSelection,
                    overwrite);
            }
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                input.UnresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnsupportedSource,
                    requestedPath,
                    "The exact contained path is not a recognized source.")]);
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.UnresolvedSelection, requestedPath);
        }
        var selection = new RouteInspectSelection(
            RouteInspectReferenceKind.SourcePath,
            RouteInspectSelectionMethod.ExactPath,
            requestedPath,
            []);
        var ambiguousOverwrite = RouteInspectOverwriteResolutionPolicy.FindAmbiguousCandidate(
            input.Catalogue,
            source.CanonicalPath);
        if (ambiguousOverwrite is not null)
        {
            return RouteInspectOverwriteResolutionPolicy.Blocked(selection, ambiguousOverwrite);
        }
        if (!_physicalVerifier.MatchesCataloguePhysicalIdentity(
                source,
                requestedPath,
                input.ExactPathPhysical!))
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                input.UnresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    requestedPath,
                    "The selected source does not match its catalogue physical identity.")]);
        }
        return ResolveSource(input, selection, source, requestedPath, cancellationToken);
    }
    private RouteInspectResolution ResolveSource(
        RouteInspectResolutionInput input,
        RouteInspectSelection selection,
        RouteSource source,
        string requestedPath,
        CancellationToken cancellationToken)
    {
        return _sourceFactsResolver.Resolve(
            new RouteInspectSourceResolutionInput(
                input.Request,
                selection,
                source,
                requestedPath,
                input.Catalogue),
            cancellationToken);
    }
}
