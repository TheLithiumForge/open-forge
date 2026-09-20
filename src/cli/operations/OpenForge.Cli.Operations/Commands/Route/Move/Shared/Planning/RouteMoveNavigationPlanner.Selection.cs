using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveNavigationPlanner
{
    private static IReadOnlyList<SelectedRegion> SelectRegions(
        RouteMoveNavigationPlanningRequest request,
        GeneratedNavigationFormation observed,
        GeneratedNavigationFormation intended)
    {
        var destinationPath = request.Destination.Destination.Path
            ?? throw new InvalidOperationException("A resolved Route Move destination requires its path.");
        var moved = intended.FindSource(destinationPath)
            ?? throw new InvalidOperationException("The moved source must remain in the intended formation.");
        var selection = new RegionSelection();
        selection.AddObservedParent(
            observed,
            intended,
            request.Destination.Inventory.Subject.SelectedSource.Identity.CanonicalBasePath);
        selection.AddParent(
            intended,
            moved.Identity.CanonicalBasePath,
            RouteMoveGeneratedReason.NewParent);
        if (intended.Loader is { } loader)
        {
            selection.Add(loader, RouteMoveGeneratedReason.Loader);
        }

        if (request.Destination.Inventory.Subject.Kind == RouteMoveSubjectKind.Category)
        {
            selection.Add(moved, RouteMoveGeneratedReason.MovedEntrypoint);
        }

        return selection.Form();
    }

    private sealed class RegionSelection
    {
        private readonly Dictionary<string, HashSet<RouteMoveGeneratedReason>> _reasons =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, SourceLogicalSource> _sources =
            new(StringComparer.Ordinal);

        internal void AddObservedParent(
            GeneratedNavigationFormation observed,
            GeneratedNavigationFormation intended,
            string childPath)
        {
            var node = observed.Topology.FindByPath(childPath);
            if (node is null)
            {
                return;
            }

            var parent = node.ParentState == SourceRouteParentState.Resolved
                ? intended.FindSource(node.ParentPaths[0])
                : intended.Loader;
            if (parent is not null)
            {
                Add(parent, RouteMoveGeneratedReason.OldParent);
            }
        }

        internal void AddParent(
            GeneratedNavigationFormation formation,
            string childPath,
            RouteMoveGeneratedReason reason)
        {
            var node = formation.Topology.FindByPath(childPath);
            if (node?.ParentState != SourceRouteParentState.Resolved)
            {
                return;
            }

            var parent = formation.FindSource(node.ParentPaths[0]);
            if (parent is not null)
            {
                Add(parent, reason);
            }
        }

        internal void Add(SourceLogicalSource source, RouteMoveGeneratedReason reason)
        {
            var path = source.Identity.CanonicalBasePath;
            _sources[path] = source;
            if (!_reasons.TryGetValue(path, out var values))
            {
                values = [];
                _reasons.Add(path, values);
            }

            values.Add(reason);
        }

        internal IReadOnlyList<SelectedRegion> Form()
            => _sources.Values.Select(source => new SelectedRegion(
                    source,
                    _reasons[source.Identity.CanonicalBasePath].Order().ToImmutableArray()))
                .OrderBy(region => region.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
    }

    private sealed record SelectedRegion(
        SourceLogicalSource Source,
        ImmutableArray<RouteMoveGeneratedReason> Reasons);
}
