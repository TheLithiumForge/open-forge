using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Topology;

public sealed class RouteListTopologyTests
{
    [Fact(DisplayName = "Route list graph derives authored route relationships without routing through an unrepresented folder"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void GraphDerivesAuthoredRelationships()
    {
        var inventory = StandardInventory();
        var input = Input(inventory, RouteListDepth.All);

        var topology = BuildTopology(input);

        var root = topology.FindByPath(".agents/root/_root.md")!;
        Assert.Equal(
            [
                ".agents/root/child/_child.md",
                ".agents/root/leaf.md",
                ".agents/root/native/SKILL.md",
            ],
            root.ChildPaths);
        Assert.Equal(".agents/root/_root.md", topology.FindByPath(".agents/root/child/_child.md")!.ParentPath);
        Assert.Equal(".agents/root/child/_child.md", topology.FindByPath(".agents/root/child/grand.md")!.ParentPath);
        Assert.Equal(".agents/root/_root.md", topology.FindByPath(".agents/root/native/SKILL.md")!.ParentPath);
        Assert.Equal(RouteTopologyParentState.None, topology.FindByPath(".agents/root/unrepresented/leaf.md")!.ParentState);
        Assert.Equal(0, topology.ReadAbsoluteDepth(root.Source.CanonicalPath));
        Assert.Equal(2, topology.ReadAbsoluteDepth(".agents/root/child/grand.md"));
        Assert.Null(topology.ReadAbsoluteDepth(".agents/root/unrepresented/leaf.md"));
    }

    [Theory(DisplayName = "Route list topology applies zero finite and all structural depth"), InlineData(0, 1), InlineData(1, 4), InlineData(2, 5), InlineData(-1, 5), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TopologyAppliesStructuralDepth(int requestedDepth, int expectedRows)
    {
        var inventory = StandardInventory();
        var depth = requestedDepth < 0
            ? RouteListDepth.All
            : RouteListDepth.Finite(requestedDepth);

        var result = BuildResult(Input(inventory, depth));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expectedRows, result.Rows.Count);
        Assert.Equal("root", result.Rows[0].Id);
        Assert.Equal(RouteListSelectionProvenance.LoaderRoot, result.Rows[0].Provenance.Selection);
        if (requestedDepth == 0)
        {
            Assert.Null(result.Rows[0].DirectChildCount);
        }
        else
        {
            Assert.Equal(3, result.Rows[0].DirectChildCount);
        }

        Assert.All(result.Rows.Skip(1), row => Assert.True(row.RelativeDepth > 0));
        string[] expectedIds = requestedDepth switch
        {
            0 => ["root"],
            1 => ["root", "root/child", "root/leaf", "root/native"],
            _ => ["root", "root/child", "root/child/grand", "root/leaf", "root/native"],
        };
        Assert.Equal(expectedIds, result.Rows.Select(row => row.Id));
    }

    [Fact(DisplayName = "Route list explicitly selected nested routes retain actual parent and absolute depth"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExplicitNestedRouteRetainsActualAncestry()
    {
        var inventory = StandardInventory();
        var input = Input(inventory, RouteListDepth.All, "root/child");

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["root/child", "root/child/grand"], result.Rows.Select(row => row.Id));
        var selected = result.Rows[0];
        Assert.Equal("root", selected.ParentId);
        Assert.Equal(".agents/root/_root.md", selected.ParentPath);
        Assert.Equal(1, selected.AbsoluteDepth);
        Assert.Equal(0, selected.RelativeDepth);
        Assert.Equal(RouteListSelectionProvenance.ExplicitRoot, selected.Provenance.Selection);
        Assert.Equal(2, result.Rows[1].AbsoluteDepth);
        Assert.Equal(1, result.Rows[1].RelativeDepth);
    }

    [Fact(DisplayName = "Route list Loader selection preserves nested authored parentage while resetting relative depth"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void LoaderSelectionPreservesNestedAuthoredParentage()
    {
        var root = Source(".agents/root/index.md", RouteListSourceKind.Entrypoint);
        var child = Source(".agents/root/child/_child.md", RouteListSourceKind.Entrypoint);
        var inventory = Inventory(root, child);
        var input = Input(
            inventory,
            RouteListDepth.All,
            null,
            root.Source.CanonicalPath,
            child.Source.CanonicalPath);

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["root", "root/child"], result.Rows.Select(row => row.Id));
        Assert.All(result.Rows, row => Assert.Equal(0, row.RelativeDepth));
        Assert.Equal("root", result.Rows[1].ParentId);
        Assert.Equal(root.Source.CanonicalPath, result.Rows[1].ParentPath);
        Assert.Equal(1, result.Rows[1].AbsoluteDepth);
        Assert.Equal(RouteListSelectionProvenance.LoaderRoot, result.Rows[1].Provenance.Selection);
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            result.Status,
            result.Workspace,
            result.Selection,
            result.Coverage,
            result.Rows.Reverse(),
            result.Findings,
            result.Next));
    }

    [Fact(DisplayName = "Route list a nested-only Loader selection retains authored absolute depth"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void NestedOnlyLoaderSelectionRetainsAuthoredAbsoluteDepth()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var child = Source(".agents/root/child/_child.md", RouteListSourceKind.Entrypoint);
        var inventory = Inventory(root, child);
        var input = Input(
            inventory,
            RouteListDepth.All,
            null,
            child.Source.CanonicalPath);

        var result = BuildResult(input);

        var row = Assert.Single(result.Rows);
        Assert.Equal("root", row.ParentId);
        Assert.Equal(root.Source.CanonicalPath, row.ParentPath);
        Assert.Equal(1, row.AbsoluteDepth);
        Assert.Equal(0, row.RelativeDepth);
        Assert.Equal(RouteListSelectionProvenance.LoaderRoot, row.Provenance.Selection);
    }

    [Fact(DisplayName = "Route list explicitly selected detached routes retain local ancestry without Loader depth"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExplicitDetachedRouteRetainsLocalAncestry()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var detached = Source(".agents/detached/_detached.md", RouteListSourceKind.Entrypoint);
        var leaf = Source(".agents/detached/leaf.md", RouteListSourceKind.RoutedLeaf);
        var inventory = Inventory(root, detached, leaf);
        var input = Input(inventory, RouteListDepth.All, "detached");

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["detached", "detached/leaf"], result.Rows.Select(row => row.Id));
        Assert.All(result.Rows, row => Assert.Null(row.AbsoluteDepth));
        Assert.Null(result.Rows[0].ParentId);
        Assert.Equal(RouteListSelectionProvenance.DetachedRoot, result.Rows[0].Provenance.Selection);
        Assert.Equal("detached", result.Rows[1].ParentId);
        Assert.Equal(RouteListSelectionProvenance.Descendant, result.Rows[1].Provenance.Selection);
    }

    [Fact(DisplayName = "Route list rejects a metadata-bearing leaf below an unrepresented folder as unrouted"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExplicitLeafBelowUnrepresentedFolderIsInvalid()
    {
        var inventory = StandardInventory();
        var input = Input(inventory, RouteListDepth.All, "root/unrepresented/leaf");

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Empty(result.Rows);
        Assert.Equal(RouteListCoverageState.NotStarted, result.Coverage.State);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.UnsupportedSource);
        Assert.Equal("open-forge route list --help", result.Next?.Command);
    }

    [Fact(DisplayName = "Route list exact overwrite selection emits one base row with overwrite provenance"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExactOverwriteSelectionEmitsOneBaseRow()
    {
        const string overwritePath = ".agents/root/leaf.overwrite.md";
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var leaf = Source(
            ".agents/root/leaf.md",
            RouteListSourceKind.RoutedLeaf,
            overwritePath: overwritePath);
        var inventory = Inventory(root, leaf);
        var loaderSelection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.LoaderRoots(),
            [root.Source]);
        var selection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.ResolvedPath(overwritePath, leaf.Source),
            [leaf.Source]);
        var input = new RouteListTopologyInput(
            new RouteListRequest(RouteListContractTestData.Workspace(), overwritePath, RouteListDepth.All),
            inventory,
            selection,
            loaderSelection);

        var result = BuildResult(input);

        var row = Assert.Single(result.Rows);
        Assert.Equal(".agents/root/leaf.md", row.Path);
        Assert.True(row.Provenance.HasOverwrite);
        Assert.Equal(RouteListSelectionProvenance.ExplicitRoot, row.Provenance.Selection);
    }

    [Fact(DisplayName = "Route list blocks an explicitly selected leaf whose routed parent is ambiguous"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExplicitLeafWithAmbiguousParentIsBlocked()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var canonical = Source(
            ".agents/root/ambiguous/_ambiguous.md",
            RouteListSourceKind.Entrypoint,
            isRouteAmbiguous: true);
        var compatibility = Source(
            ".agents/root/ambiguous/index.md",
            RouteListSourceKind.Entrypoint,
            isRouteAmbiguous: true);
        var leaf = Source(".agents/root/ambiguous/leaf.md", RouteListSourceKind.RoutedLeaf);
        var inventory = Inventory(root, canonical, compatibility, leaf);
        var input = Input(inventory, RouteListDepth.All, "root/ambiguous/leaf");

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Rows);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.RouteAmbiguous);
    }

    [Fact(DisplayName = "Route list retains safe rows and incomplete depth when requested metadata is malformed"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void MalformedRequestedMetadataIsIncomplete()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var malformed = Source(
            ".agents/root/malformed.md",
            RouteListSourceKind.Unrouted,
            RouteSourceMetadataState.Malformed);
        var inventory = Inventory(
            [root, malformed],
            [RouteListFilesystemFindingPolicy.MetadataMalformed(malformed.Source.CanonicalPath)]);
        var input = Input(inventory, RouteListDepth.All);

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(["root"], result.Rows.Select(row => row.Id));
        Assert.Equal(RouteListCoverageState.Incomplete, result.Coverage.State);
        Assert.Equal(0, result.EffectiveDepth?.Value);
        Assert.Null(result.Rows[0].DirectChildCount);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.MetadataMalformed);
        Assert.NotNull(result.Next);
    }

    [Fact(DisplayName = "Route list compatibility entrypoints produce attention with complete coverage"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CompatibilityEntrypointProducesAttention()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var compatibility = Source(".agents/root/compat/index.md", RouteListSourceKind.Entrypoint);
        var inventory = Inventory(
            [root, compatibility],
            [RouteListFilesystemFindingPolicy.CompatibilityEntrypoint(compatibility.Source.CanonicalPath)]);

        var result = BuildResult(Input(inventory, RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteListCoverageState.Complete, result.Coverage.State);
        Assert.Equal(["root", "root/compat"], result.Rows.Select(row => row.Id));
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.AuthoredForm);
        Assert.Null(result.Next);
    }

    [Fact(DisplayName = "Route list blocks duplicate selected route IDs while retaining deterministic safe rows"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DuplicateSelectedIdsAreBlocked()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var leaf = Source(".agents/root/collision.md", RouteListSourceKind.RoutedLeaf);
        var nested = Source(".agents/root/collision/_collision.md", RouteListSourceKind.Entrypoint);
        var inventory = Inventory(root, leaf, nested);

        var result = BuildResult(Input(inventory, RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(["root", "root/collision", "root/collision"], result.Rows.Select(row => row.Id));
        Assert.Contains(result.Findings, finding =>
            finding.Code == RouteListFindingCode.IdentityCollision
            && finding.Status == CliSemanticStatus.Blocked);
        Assert.Equal(RouteListCoverageState.Blocked, result.Coverage.State);
    }

    [Fact(DisplayName = "Route list exact-path selection reports an ID collision outside the selected closure"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExactPathSelectionReportsExternalIdCollision()
    {
        const string selectedPath = ".agents/root/collision.md";
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var selected = Source(selectedPath, RouteListSourceKind.RoutedLeaf);
        var collision = Source(
            ".agents/root/collision/SKILL.md",
            RouteListSourceKind.Unrouted,
            RouteSourceMetadataState.Malformed);
        var inventory = Inventory(root, selected, collision);
        var loaderSelection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.LoaderRoots(),
            [root.Source]);
        var sourceSelection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.ResolvedPath(selectedPath, selected.Source),
            [selected.Source]);
        var input = new RouteListTopologyInput(
            new RouteListRequest(RouteListContractTestData.Workspace(), selectedPath, RouteListDepth.All),
            inventory,
            sourceSelection,
            loaderSelection);

        var result = BuildResult(input);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Single(result.Rows);
        Assert.Contains(result.Findings, finding =>
            finding.Code == RouteListFindingCode.IdentityCollision
            && finding.Status == CliSemanticStatus.Attention);
    }

    [Fact(DisplayName = "Route list ignores inventory findings outside the selected route closure"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void UnrelatedInventoryFindingIsIgnored()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var unrelated = new RouteListFilesystemFinding(
            RouteListFindingCode.PhysicalBoundary,
            CliSemanticStatus.Blocked,
            ".agents/unrelated/external",
            "The unrelated path leaves the workspace.");
        var inventory = Inventory([root], [unrelated]);

        var result = BuildResult(Input(inventory, RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Single(result.Rows);
    }

    [Fact(DisplayName = "Route list interruption retains already confirmed parent-first rows"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InterruptedInventoryRetainsConfirmedRows()
    {
        var root = Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint);
        var leaf = Source(".agents/root/leaf.md", RouteListSourceKind.RoutedLeaf);
        var interruption = RouteListFilesystemFindingPolicy.Interrupted(".agents/root/pending.md");
        var inventory = RouteListInventoryFacts.Interrupted(
            [root, leaf],
            [],
            [],
            interruption);

        var result = BuildResult(Input(inventory, RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(["root", "root/leaf"], result.Rows.Select(row => row.Id));
        Assert.Equal(RouteListCoverageState.Interrupted, result.Coverage.State);
        Assert.Equal(0, result.EffectiveDepth?.Value);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.Interrupted);
        Assert.NotNull(result.Next);
    }

    [Fact(DisplayName = "Route list pre-cancelled topology selection is interrupted without inventing rows"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void PreCancelledTopologySelectionIsInterrupted()
    {
        var input = Input(StandardInventory(), RouteListDepth.All);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = BuildResultWithCancellation(input, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Empty(result.Rows);
        Assert.Equal(RouteListCoverageState.Interrupted, result.Coverage.State);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.Interrupted);
    }

    [Fact(DisplayName = "Route list topology input rejects reconstructed source facts"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TopologyInputRejectsReconstructedSourceFacts()
    {
        var inventory = StandardInventory();
        var accepted = inventory.Sources.Single(source => source.Source.Id == "root").Source;
        var reconstructed = RouteSourceTestData.Source(
            accepted.CanonicalPath,
            accepted.Kind,
            form: accepted.Base.Form,
            metadataState: accepted.Metadata.State,
            overwritePath: accepted.OverwritePath,
            isRouteAmbiguous: accepted.IsRouteAmbiguous);
        var selection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.LoaderRoots(),
            [reconstructed]);
        var request = new RouteListRequest(
            RouteListContractTestData.Workspace(),
            null,
            RouteListDepth.Default);

        Assert.Throws<ArgumentException>(() => new RouteListTopologyInput(
            request,
            inventory,
            selection));
    }

    [Fact(DisplayName = "Route list result builder forms a typed failed result without route rows"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResultBuilderFormsTypedFailure()
    {
        var request = new RouteListRequest(
            RouteListContractTestData.Workspace(),
            "root",
            RouteListDepth.All);
        var selection = RouteListSelectionFactory.AttemptedId("root");

        var result = new RouteListResultBuilder().BuildFailure(request, selection);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(RouteListCoverageState.Failed, result.Coverage.State);
        Assert.Empty(result.Rows);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.OperationFailed);
        Assert.NotNull(result.Next);
    }

    [Theory(DisplayName = "Route list next-action policy is exhaustive"), InlineData((int)CliSemanticStatus.Complete, null), InlineData((int)CliSemanticStatus.Attention, null), InlineData((int)CliSemanticStatus.Incomplete, "open-forge route list"), InlineData((int)CliSemanticStatus.Invalid, "open-forge route list --help"), InlineData((int)CliSemanticStatus.Blocked, "open-forge route list"), InlineData((int)CliSemanticStatus.Failed, "open-forge route list"), InlineData((int)CliSemanticStatus.Interrupted, "open-forge route list"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void NextActionPolicyIsExhaustive(int statusValue, string? expectedCommand)
    {
        var next = RouteListResultBuilder.ReadNextAction((CliSemanticStatus)statusValue);

        Assert.Equal(expectedCommand, next?.Command);
        if (next is not null)
        {
            Assert.False(string.IsNullOrWhiteSpace(next.Reason));
        }
    }

    [Fact(DisplayName = "Route list topology status precedence is invalid interrupted blocked incomplete attention complete"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TopologyStatusPrecedenceIsDeterministic()
    {
        var attention = new RouteListFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            ".agents/root/index.md",
            "A compatibility entrypoint is present.");
        var incomplete = new RouteListFinding(
            RouteListFindingCode.MetadataMalformed,
            CliSemanticStatus.Incomplete,
            ".agents/root/child.md",
            "Metadata is malformed.");
        var blocked = new RouteListFinding(
            RouteListFindingCode.RouteAmbiguous,
            CliSemanticStatus.Blocked,
            ".agents/root/ambiguous",
            "Route parentage is ambiguous.");
        var interrupted = new RouteListFinding(
            RouteListFindingCode.Interrupted,
            CliSemanticStatus.Interrupted,
            ".agents/root/pending",
            "Enumeration was interrupted.");

        Assert.Equal(
            CliSemanticStatus.Complete,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Resolved,
                []));
        Assert.Equal(
            CliSemanticStatus.Attention,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Resolved,
                [attention]));
        Assert.Equal(
            CliSemanticStatus.Incomplete,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Resolved,
                [attention, incomplete]));
        Assert.Equal(
            CliSemanticStatus.Blocked,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Resolved,
                [attention, incomplete, blocked]));
        Assert.Equal(
            CliSemanticStatus.Interrupted,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Resolved,
                [attention, incomplete, blocked, interrupted]));
        Assert.Equal(
            CliSemanticStatus.Invalid,
            RouteListTopologyFindingPolicy.ReadAggregateStatus(
                RouteListSelectionResolutionState.Invalid,
                [interrupted]));
    }

    private static RouteListResult BuildResult(RouteListTopologyInput input)
    {
        return BuildResultWithCancellation(input, TestContext.Current.CancellationToken);
    }

    private static RouteListResult BuildResultWithCancellation(
        RouteListTopologyInput input,
        CancellationToken cancellationToken)
    {
        var topology = BuildTopology(input);
        var selected = new RouteListTopologySelector().Select(input, topology, cancellationToken);
        var coverage = new RouteListCoverageBuilder().Build(input, selected);
        return new RouteListResultBuilder().Build(input, selected, coverage);
    }

    private static RouteListTopologyInput Input(
        RouteListInventoryFacts inventory,
        RouteListDepth depth,
        string? sourceId = null,
        params string[] loaderRootPaths)
    {
        RouteSource[] roots = loaderRootPaths.Length == 0
            ? [inventory.Sources.Single(source => source.Source.Id == "root").Source]
            : loaderRootPaths.Select(path => inventory.Catalogue.FindByPath(path)!).ToArray();
        var loaderSelection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.LoaderRoots(),
            roots);
        if (sourceId is null)
        {
            return new RouteListTopologyInput(
                new RouteListRequest(RouteListContractTestData.Workspace(), null, depth),
                inventory,
                loaderSelection);
        }

        var source = Assert.Single(inventory.Catalogue.FindById(sourceId));
        var selection = RouteListSelectionResolutionFactory.Resolved(
            RouteListSelectionFactory.ResolvedId(sourceId, source),
            [source]);
        return new RouteListTopologyInput(
            new RouteListRequest(RouteListContractTestData.Workspace(), sourceId, depth),
            inventory,
            selection,
            loaderSelection);
    }

    private static RouteListInventoryFacts StandardInventory()
    {
        return Inventory(
            Source(".agents/root/_root.md", RouteListSourceKind.Entrypoint),
            Source(".agents/root/leaf.md", RouteListSourceKind.RoutedLeaf),
            Source(".agents/root/child/_child.md", RouteListSourceKind.Entrypoint),
            Source(".agents/root/child/grand.md", RouteListSourceKind.RoutedLeaf),
            Source(".agents/root/native/SKILL.md", RouteListSourceKind.RoutedNative),
            Source(".agents/root/unrepresented/leaf.md", RouteListSourceKind.RoutedLeaf));
    }

    private static RouteListInventoryFacts Inventory(params RouteListInventorySource[] sources)
    {
        return Inventory(sources, []);
    }

    private static RouteListInventoryFacts Inventory(
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> findings)
    {
        return RouteListInventoryFacts.Create(sources, findings, []);
    }

    private static RouteListInventorySource Source(
        string canonicalPath,
        RouteListSourceKind kind,
        RouteSourceMetadataState metadataState = RouteSourceMetadataState.Complete,
        bool isRouteAmbiguous = false,
        string? overwritePath = null)
    {
        var form = ReadForm(canonicalPath, kind);
        var sourceKind = form switch
        {
            RouteSourceForm.Loader => RouteSourceKind.Loader,
            RouteSourceForm.CanonicalEntrypoint
                or RouteSourceForm.IndexEntrypoint
                or RouteSourceForm.UnderscoreIndexEntrypoint
                or RouteSourceForm.ReferencesEntrypoint
                or RouteSourceForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            RouteSourceForm.Skill => RouteSourceKind.Native,
            RouteSourceForm.Markdown => RouteSourceKind.Markdown,
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };
        var source = RouteSourceTestData.Source(
            canonicalPath,
            sourceKind,
            form,
            metadataState,
            overwritePath,
            isRouteAmbiguous: isRouteAmbiguous);
        return new RouteListInventorySource(source);
    }

    private static RouteSourceForm ReadForm(string path, RouteListSourceKind kind)
    {
        if (RouteListLogicalPath.ReadFileName(path) == "SKILL.md")
        {
            return RouteSourceForm.Skill;
        }

        if (kind != RouteListSourceKind.Entrypoint)
        {
            return RouteSourceForm.Markdown;
        }

        return RouteListLogicalPath.ReadFileName(path) switch
        {
            "index.md" => RouteSourceForm.IndexEntrypoint,
            "_index.md" => RouteSourceForm.UnderscoreIndexEntrypoint,
            "references.md" => RouteSourceForm.ReferencesEntrypoint,
            "_references.md" => RouteSourceForm.UnderscoreReferencesEntrypoint,
            _ => RouteSourceForm.CanonicalEntrypoint,
        };
    }

    private static RouteTopologyFacts BuildTopology(RouteListTopologyInput input)
    {
        return new RouteTopologyBuilder().Build(
            input.Inventory.Sources
                .Where(source => source.Kind is (
                    RouteListSourceKind.Entrypoint
                    or RouteListSourceKind.RoutedLeaf
                    or RouteListSourceKind.RoutedNative))
                .Select(source => source.Source),
            input.LoaderRootPaths);
    }

}
