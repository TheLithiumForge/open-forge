using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly RouteInspectInventoryTraversal _inventoryTraversal;
    private readonly RouteInspectPhysicalVerifier _physicalVerifier;
    private readonly RouteInspectSourceSelectionResolver _sourceSelectionResolver;
    private readonly RouteInspectCatalogueBuilder _catalogueBuilder = new();

    internal RouteInspectResolver()
    {
        _inventoryTraversal = new RouteInspectInventoryTraversal(_physicalPathResolver);
        _physicalVerifier = new RouteInspectPhysicalVerifier(_physicalPathResolver);
        _sourceSelectionResolver = new RouteInspectSourceSelectionResolver(
            _physicalVerifier,
            new RouteInspectSourceFactsResolver(
                _physicalVerifier,
                new RouteInspectLoaderRootResolver(_physicalVerifier)));
    }

    internal async ValueTask<RouteInspectResolution> ResolveAsync(
        RouteInspectRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parsed = RouteSourceReferenceParser.Parse(request.SourceReference);
        var unresolvedSelection = RouteInspectResolutionSupport.UnresolvedSelection(parsed);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                request.SourceReference);
        }

        if (parsed.State == RouteSourceReferenceParseState.Invalid)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.InvalidReference,
                    parsed.AttemptedId ?? parsed.AttemptedPath!,
                    parsed.Cause!)]);
        }

        try
        {
            var exactPathPhysical = parsed.Kind == RouteSourceReferenceKind.SourcePath
                ? _physicalVerifier.ResolveCandidate(request.Workspace, parsed.AttemptedPath!)
                : null;
            if (exactPathPhysical is not null)
            {
                var earlyResult = ReadEarlyPathResult(
                    parsed.AttemptedPath!,
                    unresolvedSelection,
                    exactPathPhysical,
                    cancellationToken);
                if (earlyResult is not null)
                {
                    return earlyResult;
                }
            }

            var inventory = await _inventoryTraversal
                .ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            if (inventory.Interrupted || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    parsed.AttemptedId ?? parsed.AttemptedPath!);
            }

            if (inventory.BoundaryIssues.Count != 0)
            {
                return RouteInspectResolutionSupport.CreateBoundaryResolution(
                    unresolvedSelection,
                    inventory.BoundaryIssues);
            }

            var input = new RouteInspectResolutionInput(
                request,
                parsed,
                unresolvedSelection,
                _catalogueBuilder.Build(inventory.Files),
                inventory.UnsafePaths,
                exactPathPhysical);
            if (cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    parsed.AttemptedId ?? parsed.AttemptedPath!);
            }

            return _sourceSelectionResolver.Resolve(input, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                parsed.AttemptedId ?? parsed.AttemptedPath!);
        }
        catch (Exception)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Failed,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.OperationFailure,
                    parsed.AttemptedId ?? parsed.AttemptedPath!,
                    "Route inspection failed while resolving the source.")]);
        }
    }

    private static RouteInspectResolution? ReadEarlyPathResult(
        string requestedPath,
        RouteInspectSelection unresolvedSelection,
        PhysicalPathResolution exactPathPhysical,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(unresolvedSelection, requestedPath);
        }

        if (exactPathPhysical.State == PhysicalPathState.Missing)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.MissingSource,
                    requestedPath,
                    "The exact source path does not exist.")]);
        }

        if (exactPathPhysical.State != PhysicalPathState.Contained)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    requestedPath,
                    "The exact source path crosses an unproved physical boundary.")]);
        }

        return null;
    }
}
