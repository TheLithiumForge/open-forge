using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectAxiomsProfileBuilder
{
    private const string LoaderPath = ".agents/loader.md";
    private const string InheritedSentinel = "- inherited - No local axioms; loaded ancestor axioms remain active.";
    private readonly RouteInspectGraph _graph;
    private readonly RouteInspectIdentity _identity;
    private readonly CancellationToken _cancellationToken;

    internal RouteInspectAxiomsProfileBuilder(
        RouteInspectResolution resolution,
        CancellationToken cancellationToken)
    {
        _graph = resolution.ReadGraph();
        _identity = resolution.ReadIdentity();
        _cancellationToken = cancellationToken;
    }

    internal RouteInspectFact<RouteInspectAxiomsProfile> Build()
    {
        if (_identity.RouteState == RouteInspectRouteState.NotRouted)
        {
            const string reason = "Route Axioms do not apply to an unrouted source.";
            return RouteInspectFact<RouteInspectAxiomsProfile>.Available(
                new RouteInspectAxiomsProfile(
                    RouteInspectFact<RouteInspectAxiomsSources>.NotApplicable(reason),
                    RouteInspectFact<RouteInspectAxiomsLocalState>.NotApplicable(reason)));
        }

        var selected = _graph.ProjectionSet.FindByPath(_identity.CanonicalWorkspaceRelativePath);
        if (selected is null)
        {
            return RouteInspectFact<RouteInspectAxiomsProfile>.Unavailable(
                "The selected source Axioms are unavailable.");
        }

        var inherited = ReadInherited(selected);
        var local = ReadLocal(selected);
        return RouteInspectFact<RouteInspectAxiomsProfile>.Available(
            new RouteInspectAxiomsProfile(inherited, local));
    }

    private RouteInspectFact<RouteInspectAxiomsSources> ReadInherited(RouteSource selected)
    {
        if (_identity.RouteState == RouteInspectRouteState.Detached)
        {
            return RouteInspectFact<RouteInspectAxiomsSources>.NotApplicable(
                "Detached sources have no Loader-rooted inherited Axioms.");
        }

        var chain = ReadChain(_graph, selected.CanonicalPath);
        if (chain is null || _identity.RouteState == RouteInspectRouteState.Unresolved)
        {
            return RouteInspectFact<RouteInspectAxiomsSources>.Unavailable(
                "The inherited route chain is unavailable.");
        }

        var sources = new List<string>();
        var loader = _graph.ProjectionSet.FindByPath(LoaderPath);
        if (loader is null && _graph.RouteFacts.Topology.LoaderRootPaths.Count != 0)
        {
            return RouteInspectFact<RouteInspectAxiomsSources>.Unavailable(
                "The Loader Axioms body is unavailable.");
        }

        if (loader is not null && !AddSubstantive(loader, sources))
        {
            return RouteInspectFact<RouteInspectAxiomsSources>.Unavailable(
                "The Loader Axioms body is unavailable.");
        }

        foreach (var source in chain.Where(source =>
                     source.Kind == RouteSourceKind.Entrypoint
                     && source.CanonicalPath != selected.CanonicalPath))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!AddSubstantive(source, sources))
            {
                return RouteInspectFact<RouteInspectAxiomsSources>.Unavailable(
                    "An inherited entrypoint Axioms body is unavailable.");
            }
        }

        return RouteInspectFact<RouteInspectAxiomsSources>.Available(
            new RouteInspectAxiomsSources(sources));
    }

    private bool AddSubstantive(RouteSource source, ICollection<string> sourceIds)
    {
        if (!ReadSection(source.Base, out var state))
        {
            return false;
        }

        if (source.Overwrite is not null)
        {
            if (!ReadSection(source.Overwrite, out var overwriteState))
            {
                return false;
            }

            if (overwriteState != AxiomsSectionState.Missing)
            {
                state = overwriteState;
            }
        }

        if (state == AxiomsSectionState.Substantive && !sourceIds.Contains(source.Id))
        {
            sourceIds.Add(source.Id);
        }

        return true;
    }

    private RouteInspectFact<RouteInspectAxiomsLocalState> ReadLocal(RouteSource selected)
    {
        if (selected.Kind != RouteSourceKind.Entrypoint)
        {
            return RouteInspectFact<RouteInspectAxiomsLocalState>.Available(
                RouteInspectAxiomsLocalState.NotApplicable);
        }

        if (!ReadSection(selected.Base, out var state))
        {
            return RouteInspectFact<RouteInspectAxiomsLocalState>.Unavailable(
                "The local entrypoint Axioms body is unavailable.");
        }

        if (selected.Overwrite is not null)
        {
            if (!ReadSection(selected.Overwrite, out var overwriteState))
            {
                return RouteInspectFact<RouteInspectAxiomsLocalState>.Unavailable(
                    "The local overwrite Axioms body is unavailable.");
            }

            if (overwriteState != AxiomsSectionState.Missing)
            {
                state = overwriteState;
            }
        }

        return RouteInspectFact<RouteInspectAxiomsLocalState>.Available(ReadLocalState(state));
    }

    private static IReadOnlyList<RouteSource>? ReadChain(RouteInspectGraph graph, string path)
    {
        var topology = graph.RouteFacts.Topology;
        var current = topology.FindByPath(path);
        if (current is null)
        {
            return null;
        }

        var chain = new List<RouteSource>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(current.Identity.CanonicalBasePath))
        {
            var source = graph.ProjectionSet.FindByPath(current.Identity.CanonicalBasePath);
            if (source is null)
            {
                return null;
            }

            chain.Add(source);
            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                return current.ParentState == SourceRouteParentState.None
                    ? chain.AsEnumerable().Reverse().ToArray()
                    : null;
            }

            current = topology.FindByPath(current.ParentPaths[0]);
            if (current is null)
            {
                return null;
            }
        }

        return null;
    }

}
