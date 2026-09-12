using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectProfileBuilderTopologyAndAxiomsTests
{
    [Fact(DisplayName = "Route inspect rooted entrypoint topology reports ordered chain depth direct children and descendants")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void RootEntrypointTopologyReportsCounts()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var directLeaf = RouteInspectSourceTestData.Source(
            ".agents/root/leaf.md",
            RouteSourceKind.Markdown,
            "leaf");
        var directNative = RouteInspectSourceTestData.Source(
            ".agents/root/native/SKILL.md",
            RouteSourceKind.Native,
            "native");
        var child = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/child/_child.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("child"),
            });
        var childLeaf = RouteInspectSourceTestData.Source(
            ".agents/root/child/leaf.md",
            RouteSourceKind.Markdown,
            "child leaf");
        var grandchild = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/child/grand/_grand.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("grandchild"),
            });
        var grandchildLeaf = RouteInspectSourceTestData.Source(
            ".agents/root/child/grand/leaf.md",
            RouteSourceKind.Markdown,
            "grandchild leaf");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, directLeaf, directNative, child, childLeaf, grandchild, grandchildLeaf],
            [root.CanonicalPath]);

        var profile = Build(graph, root.CanonicalPath);

        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        Assert.Equal("root", topology.RootRoute);
        Assert.Equal(["root"], topology.RouteChain);
        Assert.Null(topology.ParentId);
        Assert.Equal(1, topology.Depth);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(2, counts.DirectRoutedFileCount);
        Assert.Equal(1, counts.DirectEntrypointCount);
        Assert.Equal(4, counts.DescendantRoutedFileCount);
        Assert.Equal(2, counts.DescendantEntrypointCount);
    }

    [Fact(DisplayName = "Route inspect routed leaf topology reports its parent chain and not-applicable child counts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void RoutedLeafTopologyReportsParentAndNoChildCounts()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var child = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/child/_child.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("child"),
            });
        var leaf = RouteInspectSourceTestData.Source(
            ".agents/root/child/leaf.md",
            RouteSourceKind.Markdown,
            "leaf");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, child, leaf],
            [root.CanonicalPath]);

        var profile = Build(graph, leaf.CanonicalPath);

        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        Assert.Equal("root", topology.RootRoute);
        Assert.Equal(["root", "child", "leaf"], topology.RouteChain);
        Assert.Equal("root/child", topology.ParentId);
        Assert.Equal(3, topology.Depth);
        Assert.Equal(RouteInspectFactState.NotApplicable, topology.Counts.State);
        Assert.Null(topology.Counts.Value);
    }

    [Fact(DisplayName = "Route inspect detached entrypoint retains local topology without Loader-rooted reading facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DetachedEntrypointKeepsLocalTopology()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var rooted = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var detached = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/detached/_detached.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("detached"),
            });
        var detachedLeaf = RouteInspectSourceTestData.Source(
            ".agents/detached/leaf.md",
            RouteSourceKind.Markdown,
            "detached leaf");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, rooted, detached, detachedLeaf],
            [rooted.CanonicalPath]);

        var profile = Build(
            graph,
            detached.CanonicalPath,
            RouteInspectRouteState.Detached);

        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);
        Assert.Equal("detached", topology.RootRoute);
        Assert.Equal(["detached"], topology.RouteChain);
        Assert.Null(topology.ParentId);
        Assert.Equal(1, topology.Depth);
        var counts = Assert.IsType<RouteInspectTopologyCounts>(topology.Counts.Value);
        Assert.Equal(1, counts.DirectRoutedFileCount);
        Assert.Equal(0, counts.DirectEntrypointCount);
        Assert.Equal(1, counts.DescendantRoutedFileCount);
        Assert.Equal(0, counts.DescendantEntrypointCount);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.TaskStart.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.Automatic.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.Later.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Measurements.SelectionAddition.State);
    }

    [Fact(DisplayName = "Route inspect known unrouted source preserves own measurement and marks route facts not-applicable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnroutedSourceMarksRouteDependentFactsNotApplicable()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var unrouted = RouteInspectSourceTestData.Source(
            ".agents/unrouted.md",
            RouteSourceKind.Markdown,
            "unrouted");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, unrouted],
            [root.CanonicalPath]);

        var profile = Build(
            graph,
            unrouted.CanonicalPath,
            RouteInspectRouteState.NotRouted);

        Assert.Equal(RouteInspectFactState.Value, profile.Measurements.OwnSource.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.TaskStart.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.Automatic.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Reading.Later.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Measurements.SelectedClosure.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Measurements.TaskStartOverlap.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Measurements.SelectionAddition.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Measurements.LoadNowDescendants.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, profile.Topology.State);
        Assert.Equal(RouteInspectFactState.Value, profile.Axioms.State);
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Inherited.State);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Local.State);
    }

    [Fact(DisplayName = "Route inspect incomplete Loader-root facts remain unavailable rather than becoming unrouted or zero")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IncompleteLoaderFactsRemainUnavailable()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var target = RouteInspectSourceTestData.Source(
            ".agents/root/target.md",
            RouteSourceKind.Markdown,
            "target");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            []);
        var selection = RouteInspectResolutionTestData.Selection(
            target,
            RouteInspectSelectionMethod.AutomaticId);
        var identity = RouteInspectResolutionTestData.Identity(
            target,
            RouteInspectRouteState.Unresolved);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Incomplete,
            Selection = selection,
            Identity = identity,
            Graph = graph,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.IncompleteRoute,
                target.CanonicalPath)],
        });

        var profile = new RouteInspectProfileBuilder().Build(
            resolution,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectCompleteness.Incomplete, profile.Completeness);
        Assert.Equal(RouteInspectFactState.Unavailable, profile.Topology.State);
        Assert.Equal(RouteInspectFactState.Unavailable, profile.Reading.TaskStart.State);
        Assert.Equal(RouteInspectFactState.Unavailable, profile.Reading.Automatic.State);
        Assert.Equal(RouteInspectFactState.Unavailable, profile.Reading.Later.State);
    }

    [Theory(DisplayName = "Route inspect local Axioms state distinguishes substantive sentinel empty and missing sections")]
    [InlineData("substantive", (int)RouteInspectAxiomsLocalState.Substantive)]
    [InlineData("sentinel", (int)RouteInspectAxiomsLocalState.InheritedSentinel)]
    [InlineData("empty", (int)RouteInspectAxiomsLocalState.Empty)]
    [InlineData("missing", (int)RouteInspectAxiomsLocalState.Missing)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void LocalAxiomsStatesRemainDistinct(string localCase, int expectedState)
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Loader rule."));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Root rule."),
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries(
                    RouteSourceTestDataForAxioms(localCase)),
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);

        var profile = Build(graph, target.CanonicalPath);

        Assert.Equal(RouteInspectFactState.Value, profile.Axioms.State);
        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        var inherited = Assert.IsType<RouteInspectAxiomsSources>(axioms.Inherited.Value);
        Assert.Equal(["loader", "root"], inherited.SourceIds);
        Assert.Equal((RouteInspectAxiomsLocalState)expectedState, axioms.Local.Value);
    }

    [Fact(DisplayName = "Route inspect local Axioms remains substantive when inherited provenance is unavailable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InheritedAndLocalAxiomsAvailabilityIsIndependent()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("loader"));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                MetadataState = RouteSourceMetadataState.ReadUnavailable,
                DocumentReadState = FileReadState.AccessDenied,
            });
        var target = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/target/_target.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Target rule."),
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, target],
            [root.CanonicalPath]);
        var selection = RouteInspectResolutionTestData.Selection(
            target,
            RouteInspectSelectionMethod.AutomaticId);
        var identity = RouteInspectResolutionTestData.Identity(
            target,
            RouteInspectRouteState.Routed);
        var resolution = RouteInspectResolutionTestData.Create(new RouteInspectResolutionSpec
        {
            State = RouteInspectResolutionState.Incomplete,
            Selection = selection,
            Identity = identity,
            Graph = graph,
            Issues = [RouteInspectResolutionTestData.Issue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                root.CanonicalPath)],
        });

        var profile = new RouteInspectProfileBuilder().Build(
            resolution,
            TestContext.Current.CancellationToken);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectFactState.Unavailable, axioms.Inherited.State);
        Assert.Equal(RouteInspectFactState.Value, axioms.Local.State);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
    }

    [Fact(DisplayName = "Route inspect ordinary leaf Axioms heading remains inactive while ancestor provenance remains visible")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void LeafAxiomsHeadingIsNotActivated()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Loader rule."));
        var root = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Root rule."),
            });
        var leaf = RouteInspectSourceTestData.Source(
            ".agents/root/leaf.md",
            RouteSourceKind.Markdown,
            "## Axioms\n\n- Leaf rule must not activate.");
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, root, leaf],
            [root.CanonicalPath]);

        var profile = Build(graph, leaf.CanonicalPath);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        var inherited = Assert.IsType<RouteInspectAxiomsSources>(axioms.Inherited.Value);
        Assert.Equal(["loader", "root"], inherited.SourceIds);
        Assert.Equal(RouteInspectAxiomsLocalState.NotApplicable, axioms.Local.Value);
    }

    [Fact(DisplayName = "Route inspect detached entrypoint local Axioms remains available without Loader inheritance")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DetachedAxiomsKeepLocalFactWithoutLoaderInheritance()
    {
        var loader = RouteInspectSourceTestData.Source(
            ".agents/loader.md",
            RouteSourceKind.Loader,
            RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Loader rule."));
        var rooted = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/root/_root.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("root"),
            });
        var detached = RouteInspectSourceTestData.Source(
            new RouteInspectSourceSpec
            {
                Path = ".agents/detached/_detached.md",
                Kind = RouteSourceKind.Entrypoint,
                Body = RouteInspectSourceTestData.BodyWithEntries("## Axioms\n\n- Detached rule."),
            });
        var graph = RouteInspectResolutionTestData.Graph(
            [loader, rooted, detached],
            [rooted.CanonicalPath]);

        var profile = Build(
            graph,
            detached.CanonicalPath,
            RouteInspectRouteState.Detached);

        var axioms = Assert.IsType<RouteInspectAxiomsProfile>(profile.Axioms.Value);
        Assert.Equal(RouteInspectFactState.NotApplicable, axioms.Inherited.State);
        Assert.Equal(RouteInspectAxiomsLocalState.Substantive, axioms.Local.Value);
    }

    private static RouteInspectProfile Build(
        RouteInspectGraph graph,
        string selectedPath,
        RouteInspectRouteState routeState = RouteInspectRouteState.Routed)
    {
        return new RouteInspectProfileBuilder().Build(
            RouteInspectResolutionTestData.Resolved(graph, selectedPath, routeState: routeState),
            TestContext.Current.CancellationToken);
    }

    private static string RouteSourceTestDataForAxioms(string localCase)
    {
        return localCase switch
        {
            "substantive" => "## Axioms\n\n- Target rule.\n",
            "sentinel" => "## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n",
            "empty" => "## Axioms",
            "missing" => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(localCase), localCase),
        };
    }
}
