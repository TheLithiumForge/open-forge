using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceSelectionResolver
{
    private readonly RouteInspectSourceFactsResolver _sourceFactsResolver;
    private readonly RouteInspectInteractiveSourceSelector _interactiveSourceSelector;

    internal RouteInspectSourceSelectionResolver(
        RouteInspectSourceFactsResolver sourceFactsResolver,
        RouteInspectInteractiveSourceSelector interactiveSourceSelector)
    {
        _sourceFactsResolver = sourceFactsResolver;
        _interactiveSourceSelector = interactiveSourceSelector;
    }

    internal ValueTask<RouteInspectResolution> ResolveAsync(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(Interrupted(input));
        }

        return input.Parsed.Kind == SourceReferenceKind.SourceId
            ? ResolveIdAsync(input, cancellationToken)
            : ValueTask.FromResult(ResolvePath(input, cancellationToken));
    }

    private RouteInspectResolution ResolvePath(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        var requestedPath = input.Parsed.AttemptedPath
            ?? throw new InvalidOperationException("A source-path selection requires its attempted path.");
        var logicalSource = input.Catalogue.FindByPath(requestedPath);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(input);
        }

        if (logicalSource is null)
        {
            var unmatchedOverwrite = input.Projections.ProjectionSet.FindOverwriteByPath(requestedPath);
            if (unmatchedOverwrite is not null)
            {
                return RouteInspectOverwriteResolutionPolicy.Blocked(input.UnresolvedSelection, unmatchedOverwrite);
            }

            var candidate = input.Catalogue.FindCandidateByPath(requestedPath);
            if (candidate is not null && candidate.PhysicalState != PhysicalPathState.Contained)
            {
                return Unsafe(input.UnresolvedSelection, requestedPath);
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

        var selection = new RouteInspectSelection(
            RouteInspectReferenceKind.SourcePath,
            RouteInspectSelectionMethod.ExactPath,
            requestedPath,
            []);
        var ambiguousOverwrite = RouteInspectOverwriteResolutionPolicy.FindAmbiguousCandidate(
            input.Projections.ProjectionSet,
            logicalSource.Identity.CanonicalBasePath);
        if (ambiguousOverwrite is not null)
        {
            return RouteInspectOverwriteResolutionPolicy.Blocked(selection, ambiguousOverwrite);
        }

        var requestedLayer = logicalSource.Base;
        if (logicalSource.Overwrite is { } overwrite
            && string.Equals(overwrite.CanonicalPath, requestedPath, StringComparison.Ordinal))
        {
            requestedLayer = overwrite;
        }

        var exactPathPhysical = input.ExactPathPhysical
            ?? throw new InvalidOperationException("An exact-path selection requires its physical resolution.");
        var resolvedPhysicalPath = exactPathPhysical.GetContainedPhysicalPath();
        if (!PhysicalIdentityTracker.PathComparer.Equals(
                requestedLayer.PhysicalPath,
                resolvedPhysicalPath))
        {
            return Unsafe(input.UnresolvedSelection, requestedPath);
        }

        return ResolveSource(input, selection, logicalSource, requestedPath, cancellationToken);
    }

    private RouteInspectResolution ResolveSource(
        RouteInspectResolutionInput input,
        RouteInspectSelection selection,
        SourceLogicalSource logicalSource,
        string requestedPath,
        CancellationToken cancellationToken)
    {
        var projection = input.Projections.Projections.Single(candidate =>
            ReferenceEquals(candidate.LogicalSource, logicalSource));
        var physicalResolution = ReadPhysicalResolution(selection, projection, requestedPath);
        if (physicalResolution is not null)
        {
            return physicalResolution;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(selection, requestedPath);
        }

        return _sourceFactsResolver.Resolve(
            new RouteInspectSourceResolutionInput(input, selection, projection, requestedPath),
            cancellationToken);
    }
}
