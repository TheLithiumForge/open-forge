using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteCurrentContextReader
{
    internal static RouteContextSet Read(RouteSourceInspection inspection)
    {
        var sources = inspection.Sources.Select(observation =>
        {
            var route = inspection.Routes.RouteFacts.SingleOrDefault(fact =>
                string.Equals(
                    fact.Identity.CanonicalBasePath,
                    observation.Source.Identity.CanonicalBasePath,
                    StringComparison.Ordinal));
            var node = inspection.Routes.Topology.FindByPath(
                observation.Source.Identity.CanonicalBasePath);
            var parentPath = node?.ParentState == SourceRouteParentState.Resolved
                ? node.ParentPaths[0]
                : null;
            return new RouteContextSource
            {
                Id = observation.Source.Identity.AutomaticId,
                Path = observation.Source.Identity.CanonicalBasePath,
                Form = observation.Source.Base.Form,
                RouteState = route?.State ?? SourceRouteState.Unrouted,
                Layers = observation.Layers.Select(layer =>
                    new RouteContextLayer(layer.Path, layer.Text)).ToArray(),
                Metadata = observation.AuthoredMetadata,
                GeneratedEntries = observation.GeneratedEntries,
                ParentPath = parentPath,
            };
        }).ToArray();
        var roots = RouteContextRootProjector.Read(sources);
        return new RouteContextSet
        {
            WorkspaceEntry = new RouteContextSource
            {
                Id = null,
                Path = inspection.WorkspaceEntry.Path,
                Form = SourceDocumentForm.Markdown,
                RouteState = SourceRouteState.Unrouted,
                Layers = [new RouteContextLayer(
                    inspection.WorkspaceEntry.Path,
                    inspection.WorkspaceEntry.Text)],
                Metadata = SourceAuthoredMetadataFacts.WithoutValues(
                    SourceAuthoredMetadataState.NotApplicable),
                GeneratedEntries = SourceGeneratedEntriesFacts.Absent,
                ParentPath = null,
            },
            Sources = sources,
            RootPaths = roots.Paths,
            RootCategories = roots.Categories,
            IsComplete = inspection.State == OperationalViewState.Complete
                && inspection.Catalogue.Sources.Count != 0,
        };
    }
}

internal static class RouteContextRootProjector
{
    internal static RouteContextRootFacts Read(IReadOnlyList<RouteContextSource> sources)
    {
        var loader = sources.SingleOrDefault(source => source.IsLoader);
        if (loader?.GeneratedEntries.State != SourceGeneratedEntriesState.Complete)
        {
            return new RouteContextRootFacts([], []);
        }

        var paths = new List<string>();
        var categories = new List<string>();
        foreach (var entry in loader.GeneratedEntries.Entries)
        {
            var path = SourceGeneratedDestinationResolver.Resolve(
                loader.Path,
                isLoader: true,
                entry.Destination);
            var target = path is null
                ? null
                : sources.SingleOrDefault(source => string.Equals(
                    source.Path,
                    path,
                    StringComparison.Ordinal));
            if (target?.Id is not { } id)
            {
                continue;
            }

            paths.Add(target.Path);
            categories.Add(id);
        }

        return new RouteContextRootFacts(paths.ToArray(), categories.ToArray());
    }
}
