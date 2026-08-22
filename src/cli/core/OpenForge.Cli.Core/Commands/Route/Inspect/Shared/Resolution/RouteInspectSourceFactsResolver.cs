using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectSourceFactsResolver
{
    private readonly RouteInspectPhysicalVerifier _physicalVerifier;
    private readonly RouteInspectLoaderRootResolver _loaderRootResolver;
    private readonly RouteInspectTopologyResolver _topologyResolver = new();

    internal RouteInspectSourceFactsResolver(
        RouteInspectPhysicalVerifier physicalVerifier,
        RouteInspectLoaderRootResolver loaderRootResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalVerifier);
        ArgumentNullException.ThrowIfNull(loaderRootResolver);
        _physicalVerifier = physicalVerifier;
        _loaderRootResolver = loaderRootResolver;
    }

    internal RouteInspectResolution Resolve(
        RouteInspectSourceResolutionInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var source = input.Source;
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        if (source.Kind == RouteSourceKind.Loader)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                RouteInspectResolutionSupport.UnresolvedSelection(input.Selection),
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.LoaderSubject,
                    input.RequestedPath,
                    "The Loader is the workspace routing root, not an inspectable route source.")]);
        }

        var physicalIssue = _physicalVerifier.VerifyPhysicalLayers(input.Request.Workspace, source);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        if (physicalIssue is not null)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                input.Selection,
                null,
                null,
                [physicalIssue]);
        }

        var loaderRoots = _loaderRootResolver.Resolve(
            input.Request.Workspace,
            input.Catalogue,
            cancellationToken);
        if (loaderRoots.Interrupted || cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        var topology = _topologyResolver.Build(input.Catalogue, loaderRoots.RootPaths);
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        var graph = new RouteInspectGraph(input.Catalogue, topology);
        var routeState = _topologyResolver.ReadRouteState(
            source,
            topology,
            loaderRoots.AreLoaderRootFactsComplete);
        var identity = new RouteInspectIdentity(
            source.Id,
            source.CanonicalPath,
            RouteInspectSourcePolicy.ReadInspectKind(source.Kind),
            RouteInspectSourcePolicy.ReadInspectForm(source.Base.Form),
            routeState,
            RouteInspectTopologyResolver.ReadPhysicalLayers(source));

        var issues = new List<RouteInspectResolutionIssue>();
        issues.AddRange(loaderRoots.Issues);
        AddReadIssues(source, topology, issues);
        if (routeState == RouteInspectRouteState.Ambiguous)
        {
            issues.Add(RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.AmbiguousRoute,
                input.RequestedPath,
                "The selected source has ambiguous authored route meaning."));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(input.Selection, input.RequestedPath);
        }

        var state = RouteInspectResolutionSupport.ReadResolutionState(issues);
        return RouteInspectResolution.Create(state, input.Selection, identity, graph, issues);
    }

    private static void AddReadIssues(
        RouteSource source,
        RouteTopologyFacts topology,
        ICollection<RouteInspectResolutionIssue> issues)
    {
        var requiredSources = ReadRequiredRouteChain(source, topology);
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var required in requiredSources)
        {
            AddDocumentReadIssue(required.Base, seenPaths, issues);
            if (required.Overwrite is not null)
            {
                AddDocumentReadIssue(required.Overwrite, seenPaths, issues);
            }
        }
    }

    private static IReadOnlyList<RouteSource> ReadRequiredRouteChain(
        RouteSource source,
        RouteTopologyFacts topology)
    {
        var chain = new List<RouteSource>();
        var current = topology.FindByPath(source.CanonicalPath);
        if (current is null)
        {
            return [source];
        }

        while (true)
        {
            chain.Add(current.Source);
            if (current.ParentState != RouteTopologyParentState.Resolved)
            {
                break;
            }

            current = topology.FindByPath(current.ParentPath!)
                ?? throw new InvalidOperationException("A resolved route parent is missing from the topology.");
        }

        chain.Reverse();
        return chain;
    }

    private static void AddDocumentReadIssue(
        RouteSourceDocument document,
        ISet<string> seenPaths,
        ICollection<RouteInspectResolutionIssue> issues)
    {
        if (document.ReadState == FileReadState.Complete
            || !seenPaths.Add(document.CanonicalLogicalPath))
        {
            return;
        }

        issues.Add(RouteInspectResolutionSupport.CreateIssue(
            RouteInspectResolutionIssueCode.ReadUnavailable,
            document.CanonicalLogicalPath,
            "A required route-chain source body could not be read completely."));
    }
}
