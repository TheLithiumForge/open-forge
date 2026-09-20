using System.Collections.Frozen;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologyInput
{
    internal RouteListTopologyInput(
        RouteListRequest request,
        RouteListInventoryFacts inventory,
        RouteListSelectionResolution selection,
        RouteListSelectionResolution? loaderRootSelection = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(selection);
        ValidateRequestSelectionKind(request, selection.Selection);

        foreach (var source in selection.SelectedSources)
        {
            RequireInventorySource(inventory, source, nameof(selection));
        }

        var roots = selection.Selection.Kind == RouteListSelectionKind.LoaderRoots
            ? selection
            : loaderRootSelection;
        if (selection.State == RouteListSelectionResolutionState.Resolved && roots is null)
        {
            throw new ArgumentNullException(
                nameof(loaderRootSelection),
                "A resolved explicit source requires the independently resolved Loader-root boundary.");
        }

        if (roots is not null)
        {
            if (roots.Selection.Kind != RouteListSelectionKind.LoaderRoots)
            {
                throw new ArgumentException(
                    "The Loader-root boundary requires Loader-root selection.",
                    nameof(loaderRootSelection));
            }

            foreach (var source in roots.SelectedSources)
            {
                RequireInventorySource(inventory, source, nameof(loaderRootSelection));
            }
        }

        Request = request;
        Inventory = inventory;
        Selection = selection;
        LoaderRootSelection = roots;
        LoaderRootPaths = (roots?.SelectedSources ?? [])
            .Select(root => root.CanonicalPath)
            .ToFrozenSet(StringComparer.Ordinal);
    }

    internal RouteListRequest Request { get; }

    internal RouteListInventoryFacts Inventory { get; }

    internal RouteListSelectionResolution Selection { get; }

    internal RouteListSelectionResolution? LoaderRootSelection { get; }

    internal IReadOnlySet<string> LoaderRootPaths { get; }

    internal bool LoaderRootBoundaryIsComplete =>
        LoaderRootSelection?.State == RouteListSelectionResolutionState.Resolved;

    private static void ValidateRequestSelectionKind(
        RouteListRequest request,
        RouteListSelection selection)
    {
        if ((request.SourceReference is null) != (selection.Kind == RouteListSelectionKind.LoaderRoots))
        {
            throw new ArgumentException(
                "The topology selection kind must match operand-free or explicit request selection.",
                nameof(selection));
        }
    }

    private static void RequireInventorySource(
        RouteListInventoryFacts inventory,
        RouteSource source,
        string parameterName)
    {
        var inventorySource = inventory.Sources
            .SingleOrDefault(candidate => string.Equals(
                candidate.Source.CanonicalPath,
                source.CanonicalPath,
                StringComparison.Ordinal));
        if (inventorySource is null || !ReferenceEquals(inventorySource.Source, source))
        {
            throw new ArgumentException(
                "Topology sources must be the accepted immutable inventory facts.",
                parameterName);
        }
    }
}
