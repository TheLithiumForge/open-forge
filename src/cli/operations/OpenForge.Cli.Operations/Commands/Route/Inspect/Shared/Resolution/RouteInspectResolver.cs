using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly SourceRouteFactsResolver _routeFactsResolver = new();
    private readonly RouteInspectSourceSelectionResolver _sourceSelectionResolver;

    internal RouteInspectResolver()
        : this(new RouteInspectSourceSelector(prompt: null))
    {
    }

    internal RouteInspectResolver(RouteInspectSourceSelector sourceSelector)
    {
        _sourceSelectionResolver = new RouteInspectSourceSelectionResolver(
            new RouteInspectSourceFactsResolver(),
            sourceSelector);
    }

    internal async ValueTask<RouteInspectResolution> ResolveAsync(
        RouteInspectRequest request,
        CancellationToken cancellationToken)
    {
        var parsed = SourceReferenceParser.Parse(request.SourceReference);
        var unresolvedSelection = RouteInspectResolutionSupport.UnresolvedSelection(parsed);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                request.SourceReference);
        }

        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            var attemptedReference = ReadAttemptedReference(parsed);
            var cause = parsed.Cause
                ?? throw new InvalidOperationException("An invalid source reference requires its cause.");
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.InvalidReference,
                    attemptedReference,
                    cause)]);
        }

        try
        {
            var layoutRead = await new WorkspaceLayoutReader(_physicalPathResolver)
                .ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
            if (layoutRead.Layout is not { } layout)
            {
                return RouteInspectResolutionSupport.CreateBoundaryResolution(
                    unresolvedSelection,
                    layoutRead.Issues.Select(issue => RouteInspectResolutionSupport.CreateIssue(
                        RouteInspectResolutionIssueCode.UnsafeSource,
                        issue.Path,
                        issue.Cause,
                        issue.RelatedPaths)).ToArray());
            }

            var exactPathPhysical = parsed.Kind == SourceReferenceKind.SourcePath
                ? ResolveExactPath(request.Workspace, ReadAttemptedReference(parsed))
                : null;
            if (exactPathPhysical is not null)
            {
                var earlyResult = ReadEarlyPathResult(
                    ReadAttemptedReference(parsed),
                    unresolvedSelection,
                    exactPathPhysical,
                    cancellationToken);
                if (earlyResult is not null)
                {
                    return earlyResult;
                }
            }

            var outside = parsed.AttemptedPath is { } path ? layoutRead.OutsideRoutes.FirstOrDefault(route =>
                RouteOutsideRouteReader.Contains(route.Descriptor.DirectoryPath, path)) : null;
            if (outside is not null && outside.State != WorkspaceOutsideRouteReadState.Available)
                return RouteInspectResolutionSupport.CreateBoundaryResolution(unresolvedSelection,
                    [RouteInspectResolutionSupport.CreateIssue(outside.State == WorkspaceOutsideRouteReadState.Unsafe
                        ? RouteInspectResolutionIssueCode.UnsafeSource : RouteInspectResolutionIssueCode.ReadUnavailable,
                        outside.Descriptor.DirectoryPath, outside.Failure?.DirectCause ?? "The selected registered outside directory is not available.")]);

            var session = await new SourceReadSessionReader(_physicalPathResolver)
                .ReadIncludedAsync(RouteOutsideRouteReader.InventoryLayout(layoutRead), cancellationToken)
                .ConfigureAwait(false);
            var reader = session.DocumentReader;
            var catalogue = session.Catalogue;
            if (catalogue.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var boundaryIssues = RouteInspectResolutionSupport.ReadCatalogueBoundaryIssues(catalogue, parsed.AttemptedPath);
            if (boundaryIssues.Count != 0)
            {
                return RouteInspectResolutionSupport.CreateBoundaryResolution(
                    unresolvedSelection,
                    boundaryIssues);
            }

            var catalogueSelection = catalogue.SelectAll();
            var projections = await RouteInspectSourceProjectionBuilder
                .ReadAsync(catalogueSelection, reader, cancellationToken)
                .ConfigureAwait(false);
            if (projections.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var routeFacts = await _routeFactsResolver
                .ResolveAsync(
                    new SourceRouteFactsRequest(catalogue, catalogueSelection),
                    reader,
                    cancellationToken)
                .ConfigureAwait(false);
            if (routeFacts.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            var input = new RouteInspectResolutionInput(
                request,
                parsed,
                unresolvedSelection,
                catalogue,
                catalogueSelection,
                projections,
                routeFacts,
                exactPathPhysical)
            {
                LayoutRead = layoutRead,
            };
            if (cancellationToken.IsCancellationRequested)
            {
                return RouteInspectResolutionSupport.Interrupted(
                    unresolvedSelection,
                    ReadAttemptedReference(parsed));
            }

            return await _sourceSelectionResolver
                .ResolveAsync(input, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(
                unresolvedSelection,
                ReadAttemptedReference(parsed));
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
                    ReadAttemptedReference(parsed),
                    "Route inspection failed while resolving the source.")]);
        }
    }

    private static string ReadAttemptedReference(SourceReferenceParseResult parsed)
    {
        return parsed.AttemptedId ?? parsed.AttemptedPath
            ?? throw new InvalidOperationException("A source-reference result requires an attempted identity.");
    }

}
