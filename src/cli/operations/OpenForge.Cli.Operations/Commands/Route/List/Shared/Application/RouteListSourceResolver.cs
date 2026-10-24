using OpenForge.Cli.Core.Commands.Route.List.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Application;

internal sealed class RouteListSourceResolver(
    RouteListInventoryReader inventoryReader,
    RouteListSelectionResolver selectionResolver,
    SourceRouteFactsResolver routeFactsResolver)
{
    private readonly RouteListInventoryReader _inventoryReader = inventoryReader;
    private readonly RouteListSelectionResolver _selectionResolver = selectionResolver;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;

    internal async ValueTask<RouteListSourceResolution> ResolveAsync(
        RouteListRequest request,
        CancellationToken cancellationToken,
        WorkspaceLayoutRead? layoutRead = null)
    {
        var documentReader = new SourceDocumentReader(request.Workspace);
        layoutRead ??= await new WorkspaceLayoutReader(documentReader.PhysicalPaths)
            .ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        var layout = layoutRead.Layout ?? throw new InvalidOperationException("The workspace layout is unavailable.");
        var inventory = await _inventoryReader
            .ReadAsync(
                new RouteListInventoryRequest(layout, cancellationToken),
                documentReader)
            .ConfigureAwait(false);
        var sourceCatalogue = inventory.SourceCatalogue;
        var projectionSet = inventory.ProjectionBuildResult.ProjectionSet;
        var routeFacts = await _routeFactsResolver
            .ResolveAsync(
                new SourceRouteFactsRequest(sourceCatalogue, sourceCatalogue.SelectAll()),
                documentReader,
                cancellationToken)
            .ConfigureAwait(false);
        var selection = await _selectionResolver
            .ResolveAsync(
                request,
                sourceCatalogue,
                projectionSet,
                routeFacts,
                cancellationToken)
            .ConfigureAwait(false);
        var included = await new SourceIncludedEntryReader().ReadEntriesAsync(sourceCatalogue, documentReader, cancellationToken).ConfigureAwait(false);
        if (request.SourceReference is { } reference)
        {
            var path = SourceReferenceParser.Parse(reference).AttemptedPath;
            var retainedPath = included.Rows.FirstOrDefault(row => row.Path == path)?.AliasOf;
            if (retainedPath is not null && sourceCatalogue.FindByPath(retainedPath) is not null)
            {
                selection = await _selectionResolver.ResolveAsync(new RouteListRequest(request.Workspace, retainedPath, request.RequestedDepth),
                    sourceCatalogue, projectionSet, routeFacts, cancellationToken).ConfigureAwait(false);
            }
        }

        var settings = layoutRead.Settings;
        var active = included.Rows.Where(row => row.ParentPath == layout.LoaderPath).Select(row => row.Path).ToHashSet(StringComparer.Ordinal);
        return new RouteListSourceResolution(layoutRead, inventory, routeFacts, selection)
        {
            IncludedSources = included.Sources,
            IncludedRows = included.Rows,
            UnlistedRoots = settings.Document.Roots.Where(path => !active.Contains(path)).ToArray(),
        };
    }

    internal async ValueTask<RouteListSelectionResolution?> ResolveLoaderRootAsync(
        RouteListRequest request,
        RouteListSourceResolution source,
        CancellationToken cancellationToken)
    {
        if (request.SourceReference is null
            || source.Selection.State != RouteListSelectionResolutionState.Resolved)
        {
            return null;
        }

        return await _selectionResolver
            .ResolveAsync(
                new RouteListRequest(
                    request.Workspace,
                    null,
                    request.RequestedDepth),
                source.Inventory.SourceCatalogue,
                source.Inventory.ProjectionBuildResult.ProjectionSet,
                source.RouteFacts,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
