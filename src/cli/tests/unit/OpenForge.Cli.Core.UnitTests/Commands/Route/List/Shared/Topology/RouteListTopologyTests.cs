using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Topology;

public sealed class RouteListTopologyTests
{
    [Fact(DisplayName = "Route list graph derives authored route relationships without routing through an unrepresented folder"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void GraphDerivesAuthoredRelationships()
    {
        var inventory = StandardInventory();
        var input = Input(inventory, RouteListDepth.All);

        var topology = new SourceRouteTopologyBuilder().Build(
            inventory.SourceCatalogue.Sources,
            input.LoaderRootPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray());

        SourceRouteNode root = Assert.IsType<SourceRouteNode>(topology.FindByPath(".agents/root/_root.md"));
        Assert.Equal(
            [
                ".agents/root/child/_child.md",
                ".agents/root/leaf.md",
                ".agents/root/native/SKILL.md",
            ],
            root.ChildPaths);
        SourceRouteNode child = Assert.IsType<SourceRouteNode>(topology.FindByPath(".agents/root/child/_child.md"));
        Assert.Equal(SourceRouteParentState.Resolved, child.ParentState);
        Assert.Equal([root.Identity.CanonicalBasePath], child.ParentPaths);
        SourceRouteNode grandchild = Assert.IsType<SourceRouteNode>(topology.FindByPath(".agents/root/child/grand.md"));
        Assert.Equal(SourceRouteParentState.Resolved, grandchild.ParentState);
        Assert.Equal([child.Identity.CanonicalBasePath], grandchild.ParentPaths);
        SourceRouteNode native = Assert.IsType<SourceRouteNode>(topology.FindByPath(".agents/root/native/SKILL.md"));
        Assert.Equal(SourceRouteParentState.Resolved, native.ParentState);
        Assert.Equal([root.Identity.CanonicalBasePath], native.ParentPaths);
        Assert.Null(topology.FindByPath(".agents/root/unrepresented/leaf.md"));
        Assert.Equal(0, topology.ReadAbsoluteDepth(root.Identity.CanonicalBasePath));
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

        var input = Input(inventory, depth);
        var result = BuildResult(input, StandardRouteFacts(input));

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

        var result = BuildResult(input, StandardRouteFacts(input));

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath, child.Source.CanonicalPath),
                RoutedChild(child.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                UnroutedRoot(root.Source.CanonicalPath, child.Source.CanonicalPath),
                RoutedChild(child.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath),
                UnroutedRoot(detached.Source.CanonicalPath, leaf.Source.CanonicalPath),
                UnroutedChild(leaf.Source.CanonicalPath, detached.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var result = BuildResult(input, StandardRouteFacts(input));

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath, leaf.Source.CanonicalPath),
                RoutedChild(leaf.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var ambiguousIssue = new SourceRouteIssue(
            SourceRouteIssueCode.RouteAmbiguous,
            leaf.Source.CanonicalPath,
            [canonical.Source.CanonicalPath, compatibility.Source.CanonicalPath],
            0,
            "The source has multiple authored route parents.");
        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(
                    root.Source.CanonicalPath,
                    canonical.Source.CanonicalPath,
                    compatibility.Source.CanonicalPath),
                RoutedChild(canonical.Source.CanonicalPath, root.Source.CanonicalPath),
                RoutedChild(compatibility.Source.CanonicalPath, root.Source.CanonicalPath),
                AmbiguousSource(
                    leaf.Source.CanonicalPath,
                    canonical.Source.CanonicalPath,
                    compatibility.Source.CanonicalPath),
            ],
            [ambiguousIssue]);
        var result = BuildResult(input, routeFacts);

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath, malformed.Source.CanonicalPath),
                RoutedChild(malformed.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var input = Input(inventory, RouteListDepth.All);
        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath, compatibility.Source.CanonicalPath),
                RoutedChild(compatibility.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var input = Input(inventory, RouteListDepth.All);
        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(
                    root.Source.CanonicalPath,
                    leaf.Source.CanonicalPath,
                    nested.Source.CanonicalPath),
                RoutedChild(leaf.Source.CanonicalPath, root.Source.CanonicalPath),
                RoutedChild(nested.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(
                    root.Source.CanonicalPath,
                    selected.Source.CanonicalPath,
                    collision.Source.CanonicalPath),
                RoutedChild(selected.Source.CanonicalPath, root.Source.CanonicalPath),
                RoutedChild(collision.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var input = Input(inventory, RouteListDepth.All);
        var routeFacts = CreateRouteFacts(
            input,
            [RoutedRoot(root.Source.CanonicalPath)]);
        var result = BuildResult(input, routeFacts);

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
        RouteListInventorySource[] sources = [root, leaf];
        var neutral = NeutralInventory(sources, isCancelled: true);
        var inventory = RouteListInventoryFacts.Interrupted(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            sources,
            [],
            [],
            interruption);

        var input = Input(inventory, RouteListDepth.All);
        var routeFacts = CreateRouteFacts(
            input,
            [
                RoutedRoot(root.Source.CanonicalPath, leaf.Source.CanonicalPath),
                RoutedChild(leaf.Source.CanonicalPath, root.Source.CanonicalPath),
            ]);
        var result = BuildResult(input, routeFacts);

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

        var result = BuildResultWithCancellation(
            input,
            StandardRouteFacts(input),
            cancellation.Token);

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

    private static RouteListResult BuildResult(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts)
    {
        return BuildResultWithCancellation(
            input,
            routeFacts,
            TestContext.Current.CancellationToken);
    }

    private static RouteListResult BuildResultWithCancellation(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        var selected = new RouteListTopologySelector().SelectSources(
            input,
            routeFacts,
            cancellationToken);
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
            : loaderRootPaths
                .Select(path => inventory.Sources.Single(source => source.Source.CanonicalPath == path).Source)
                .ToArray();
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

        var source = Assert.Single(inventory.Sources
            .Where(candidate => candidate.Source.Id == sourceId)
            .Select(candidate => candidate.Source));
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
        var materializedSources = sources.ToArray();
        var materializedFindings = findings.ToArray();
        var neutral = NeutralInventory(
            materializedSources,
            materializedFindings.Any(finding => finding.Code == RouteListFindingCode.Interrupted));
        return RouteListInventoryFacts.Create(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            materializedSources,
            materializedFindings,
            []);
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
            SourceDocumentForm.Loader => RouteSourceKind.Loader,
            SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteSourceKind.Entrypoint,
            SourceDocumentForm.Skill => RouteSourceKind.Native,
            SourceDocumentForm.Markdown => RouteSourceKind.Markdown,
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

    private static SourceDocumentForm ReadForm(string path, RouteListSourceKind kind)
    {
        if (SourceLogicalPath.ReadFileName(path) == "SKILL.md")
        {
            return SourceDocumentForm.Skill;
        }

        if (kind != RouteListSourceKind.Entrypoint)
        {
            return SourceDocumentForm.Markdown;
        }

        return SourceLogicalPath.ReadFileName(path) switch
        {
            "index.md" => SourceDocumentForm.IndexEntrypoint,
            "_index.md" => SourceDocumentForm.UnderscoreIndexEntrypoint,
            "references.md" => SourceDocumentForm.ReferencesEntrypoint,
            "_references.md" => SourceDocumentForm.UnderscoreReferencesEntrypoint,
            _ => SourceDocumentForm.CanonicalEntrypoint,
        };
    }

    private static SourceRouteFacts StandardRouteFacts(RouteListTopologyInput input)
    {
        return CreateRouteFacts(
            input,
            [
                RoutedRoot(
                    ".agents/root/_root.md",
                    ".agents/root/child/_child.md",
                    ".agents/root/leaf.md",
                    ".agents/root/native/SKILL.md"),
                RoutedChild(
                    ".agents/root/child/_child.md",
                    ".agents/root/_root.md",
                    ".agents/root/child/grand.md"),
                RoutedChild(".agents/root/child/grand.md", ".agents/root/child/_child.md"),
                RoutedChild(".agents/root/leaf.md", ".agents/root/_root.md"),
                RoutedChild(".agents/root/native/SKILL.md", ".agents/root/_root.md"),
                Unrepresented(".agents/root/unrepresented/leaf.md"),
            ]);
    }

    private static SourceRouteFacts CreateRouteFacts(
        RouteListTopologyInput input,
        IReadOnlyList<SourceRouteFixture> fixtures,
        IReadOnlyList<SourceRouteIssue>? issues = null)
    {
        var sources = input.Inventory.SourceCatalogue.Sources
            .Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .ToArray();
        var fixturesByPath = fixtures.ToDictionary(fixture => fixture.CanonicalPath, StringComparer.Ordinal);
        if (fixturesByPath.Count != sources.Length
            || sources.Any(source => !fixturesByPath.ContainsKey(source.Identity.CanonicalBasePath)))
        {
            throw new ArgumentException("Every neutral source requires one explicit route-fact fixture.", nameof(fixtures));
        }

        var sourcesByPath = sources.ToDictionary(
            source => source.Identity.CanonicalBasePath,
            StringComparer.Ordinal);
        var nodes = fixtures
            .Where(fixture => fixture.ParentState is not null)
            .Select(fixture =>
            {
                var parentState = fixture.ParentState
                    ?? throw new InvalidOperationException("A topology fixture requires its explicit parent state.");
                return new SourceRouteNode(
                    sourcesByPath[fixture.CanonicalPath].Identity,
                    parentState,
                    fixture.ParentPaths,
                    fixture.ChildPaths);
            })
            .ToArray();
        var topology = new SourceRouteTopology(nodes, input.LoaderRootPaths);
        var identityCounts = sources
            .GroupBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var routeFacts = sources.Select(source => new SourceRouteFact(
            source.Identity,
            fixturesByPath[source.Identity.CanonicalBasePath].RouteState,
            identityCounts[source.Identity.AutomaticId] == 1));
        return new SourceRouteFacts(
            topology,
            routeFacts,
            issues ?? [],
            input.LoaderRootBoundaryIsComplete,
            input.Inventory.SourceCatalogue.IsCancelled
                || input.Inventory.ProjectionBuildResult.IsCancelled);
    }

    private static SourceRouteFixture RoutedRoot(
        string canonicalPath,
        params string[] childPaths)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Routed,
            SourceRouteParentState.None,
            [],
            childPaths);
    }

    private static SourceRouteFixture RoutedChild(
        string canonicalPath,
        string parentPath,
        params string[] childPaths)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Routed,
            SourceRouteParentState.Resolved,
            [parentPath],
            childPaths);
    }

    private static SourceRouteFixture UnroutedRoot(
        string canonicalPath,
        params string[] childPaths)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Unrouted,
            SourceRouteParentState.None,
            [],
            childPaths);
    }

    private static SourceRouteFixture UnroutedChild(
        string canonicalPath,
        string parentPath,
        params string[] childPaths)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Unrouted,
            SourceRouteParentState.Resolved,
            [parentPath],
            childPaths);
    }

    private static SourceRouteFixture Unrepresented(string canonicalPath)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Unrouted,
            null,
            [],
            []);
    }

    private static SourceRouteFixture AmbiguousSource(
        string canonicalPath,
        params string[] parentPaths)
    {
        return new SourceRouteFixture(
            canonicalPath,
            SourceRouteState.Ambiguous,
            SourceRouteParentState.Ambiguous,
            parentPaths,
            []);
    }

    private static (
        SourceCatalogue SourceCatalogue,
        RouteSourceProjectionBuildResult ProjectionBuildResult) NeutralInventory(
        IReadOnlyList<RouteListInventorySource> inventorySources,
        bool isCancelled = false)
    {
        var sourcePairs = inventorySources
            .Select(source => (
                Inventory: source,
                Logical: LogicalSource(source.Source)))
            .ToArray();
        var projections = sourcePairs
            .Select(pair => Projection(pair.Inventory.Source, pair.Logical))
            .ToArray();
        var overwriteFacts = sourcePairs
            .Where(pair => pair.Inventory.Source.Overwrite is not null)
            .Select(pair =>
            {
                var overwrite = pair.Inventory.Source.Overwrite
                    ?? throw new InvalidOperationException("A paired overwrite fixture requires its Route document.");
                return new RouteOverwriteFact(
                    RouteOverwriteState.Paired,
                    overwrite,
                    [pair.Inventory.Source.CanonicalPath]);
            })
            .ToArray();
        var candidates = sourcePairs
            .SelectMany(pair => new SourceCandidate?[]
            {
                Candidate(pair.Logical.Base, pair.Logical.Identity.AutomaticId),
                pair.Logical.Overwrite is null
                    ? null
                    : Candidate(pair.Logical.Overwrite, pair.Logical.Identity.AutomaticId),
            })
            .Where(candidate => candidate is not null)
            .Cast<SourceCandidate>()
            .ToArray();
        var issues = sourcePairs
            .GroupBy(pair => pair.Logical.Identity.AutomaticId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group =>
            {
                var paths = group
                    .Select(pair => pair.Logical.Identity.CanonicalBasePath)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();
                return new SourceCatalogueIssue(
                    SourceCatalogueIssueCode.IdentityCollision,
                    paths[0],
                    paths,
                    Path.GetDirectoryName(group.First().Logical.Base.PhysicalPath),
                    null);
            })
            .ToArray();
        var sourceCatalogue = new SourceCatalogue(
            RouteListContractTestData.Workspace(),
            candidates,
            sourcePairs.Select(pair => pair.Logical),
            issues,
            isCancelled);
        var projectionSet = new RouteSourceProjectionSet(projections, overwriteFacts);
        return (
            sourceCatalogue,
            new RouteSourceProjectionBuildResult(
                projectionSet,
                projections,
                [],
                isCancelled));
    }

    private static SourceLogicalSource LogicalSource(RouteSource source)
    {
        return new SourceLogicalSource(
            new SourceLogicalIdentity(source.Id, source.CanonicalPath),
            NeutralLayer(source.Base),
            source.Overwrite is null ? null : NeutralLayer(source.Overwrite));
    }

    private static RouteSourceProjection Projection(
        RouteSource source,
        SourceLogicalSource logicalSource)
    {
        var baseRead = CompleteRead(logicalSource.Base, source.Base);
        SourceDocumentReadResult? overwriteRead = null;
        if (source.Overwrite is not null)
        {
            var logicalOverwrite = logicalSource.Overwrite
                ?? throw new InvalidOperationException("The neutral fixture must retain the Route overwrite layer.");
            overwriteRead = CompleteRead(logicalOverwrite, source.Overwrite);
        }

        return new RouteSourceProjection(logicalSource, source, baseRead, overwriteRead);
    }

    private static SourceDocumentReadResult CompleteRead(
        SourceLayer layer,
        RouteSourceDocument document)
    {
        if (document.ReadState != FileReadState.Complete || document.Body is null)
        {
            throw new ArgumentException("The topology fixture requires a complete projected source document.", nameof(document));
        }

        return new SourceDocumentReadResult(
            layer,
            new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Verified,
                layer.PhysicalPath,
                null),
            FileReadResult<string>.Complete(layer.CanonicalPath, document.Body));
    }

    private static SourceCandidate Candidate(
        SourceLayer layer,
        string automaticId)
    {
        return new SourceCandidate(
            layer.CanonicalPath,
            layer.Form,
            automaticId,
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            Path.GetDirectoryName(layer.PhysicalPath));
    }

    private static SourceLayer NeutralLayer(RouteSourceDocument document)
    {
        return new SourceLayer(
            document.CanonicalLogicalPath,
            document.PhysicalPath,
            document.Form,
            document.Form == SourceDocumentForm.OverwriteCompanion
                ? SourceLayerKind.Overwrite
                : SourceLayerKind.Base);
    }

    private sealed record SourceRouteFixture(
        string CanonicalPath,
        SourceRouteState RouteState,
        SourceRouteParentState? ParentState,
        IReadOnlyList<string> ParentPaths,
        IReadOnlyList<string> ChildPaths);

}
