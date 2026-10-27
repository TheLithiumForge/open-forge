using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Presentation.Route.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListCoverageAndResultTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route list coverage factories enforce complete and partial boundaries")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CoverageFactoriesEnforceCompleteAndPartialBoundaries()
    {
        var complete = CompleteCoverage(1);
        var incomplete = RouteListCoverage.Incomplete(
            RouteListDepth.All,
            RouteListDepth.Finite(2),
            1,
            1,
            ["One row confirmed."],
            ["A child boundary is unreadable."]);
        var notStarted = RouteListCoverage.NotStarted(null);

        Assert.Equal(RouteListCoverageState.Complete, complete.State);
        Assert.Empty(complete.UnresolvedBoundaries);
        Assert.Equal(RouteListCoverageState.Incomplete, incomplete.State);
        Assert.Single(incomplete.UnresolvedBoundaries);
        Assert.Equal(RouteListCoverageState.NotStarted, notStarted.State);
        Assert.Null(notStarted.RequestedDepth);
        Assert.Throws<ArgumentException>(() => RouteListCoverage.Incomplete(
            RouteListDepth.Default,
            null,
            0,
            0,
            [],
            []));
        Assert.Throws<ArgumentException>(() => RouteListCoverage.Complete(
            RouteListDepth.Default,
            RouteListDepth.Default,
            0,
            0,
            [""]));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route list result factories represent all seven semantic statuses")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResultFactoriesRepresentAllSemanticStatuses()
    {
        var workspace = RouteListContractTestData.Workspace();
        var row = RootRow();
        var explicitRow = RootRow(RouteListSelectionProvenance.ExplicitRoot);
        var resolved = ResolvedSelection("memory", row.Path);
        var next = new CliNextAction("open-forge route list --help", "Review valid route-list input.");
        var complete = RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            CompleteCoverage(1),
            [row],
            [],
            null);
        var attention = RouteListResult.Create(
            CliSemanticStatus.Attention,
            workspace,
            resolved,
            CompleteCoverage(1),
            [explicitRow],
            [Finding(RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention)],
            null);
        var incomplete = RouteListResult.Create(
            CliSemanticStatus.Incomplete,
            workspace,
            resolved,
            RouteListCoverage.Incomplete(
                RouteListDepth.All,
                RouteListDepth.Finite(0),
                1,
                1,
                ["Root confirmed."],
                ["Descendant coverage is unknown."]),
            [RootRow(RouteListSelectionProvenance.ExplicitRoot, directChildCount: null)],
            [Finding(RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete)],
            next);
        var invalid = RouteListResult.Create(
            CliSemanticStatus.Invalid,
            workspace,
            RouteListSelectionFactory.AttemptedId("unknown"),
            RouteListCoverage.NotStarted(null),
            [],
            [Finding(RouteListFindingCode.UnknownSource, CliSemanticStatus.Invalid)],
            next);
        var blocked = RouteListResult.Create(
            CliSemanticStatus.Blocked,
            workspace,
            RouteListSelectionFactory.AttemptedPath(".agents/unsafe.md"),
            RouteListCoverage.Blocked(null, null, 0, 0, [], ["Physical containment is unresolved."]),
            [],
            [Finding(RouteListFindingCode.UnsafeSource, CliSemanticStatus.Blocked)],
            next);
        var failed = RouteListResult.Create(
            CliSemanticStatus.Failed,
            workspace,
            RouteListSelectionFactory.AttemptedId("memory"),
            RouteListCoverage.Failed(null, null, 0, 0, [], ["The operation failed."]),
            [],
            [Finding(RouteListFindingCode.OperationFailed, CliSemanticStatus.Failed)],
            next);
        var interrupted = RouteListResult.Create(
            CliSemanticStatus.Interrupted,
            workspace,
            resolved,
            RouteListCoverage.Interrupted(
                RouteListDepth.All,
                RouteListDepth.Finite(0),
                1,
                1,
                ["Root confirmed."],
                ["Enumeration was interrupted."]),
            [explicitRow],
            [
                Finding(RouteListFindingCode.PhysicalBoundary, CliSemanticStatus.Blocked),
                Finding(RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            ],
            next);

        RouteListResult[] results =
        [
            complete,
            failed,
            attention,
            incomplete,
            invalid,
            blocked,
            interrupted,
        ];
        Assert.Equal(Enum.GetValues<CliSemanticStatus>(), results.Select(result => result.Status));
        foreach (var result in results)
        {
            var expectedStatus = CliStatusDefinitions.Read(result.Status).MachineName;
            var compact = CliRenderingStage.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Minimal, null)), RouteListPresentation.Rendering).PrimaryContent;
            var expanded = CliRenderingStage.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Standard, null)), RouteListPresentation.Rendering).PrimaryContent;
            var json = CliRenderingStage.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliFormat.Json, CliDetail.Standard, null)), RouteListPresentation.Rendering).PrimaryContent;
            using var document = JsonDocument.Parse(json);

            Assert.DoesNotContain("Status:", compact, StringComparison.Ordinal);
            Assert.DoesNotContain("Status:", expanded, StringComparison.Ordinal);
            Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
            Assert.DoesNotContain("Coverage:", compact, StringComparison.Ordinal);
            Assert.DoesNotContain("Selection:", expanded, StringComparison.Ordinal);
            Assert.DoesNotContain("Confirmed:", expanded, StringComparison.Ordinal);
        }
        Assert.Equal(1, complete.SchemaVersion);
        Assert.Equal("route list", complete.Command);
        Assert.Equal("Review valid route-list input.", invalid.Next?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route list result construction rejects inconsistent state")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResultConstructionRejectsInconsistentState()
    {
        var workspace = RouteListContractTestData.Workspace();
        var row = RootRow();
        var unresolved = RouteListSelectionFactory.AttemptedId("memory");
        var next = new CliNextAction("open-forge route list", "Retry the operation.");

        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            unresolved,
            CompleteCoverage(1),
            [row],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Finite(0),
                RouteListDepth.All,
                1,
                1,
                ["Invalid effective depth."]),
            [RootRow()],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Incomplete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Incomplete(
                RouteListDepth.All,
                null,
                0,
                0,
                [],
                ["Unknown boundary."]),
            [],
            [Finding(RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete)],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            unresolved,
            RouteListCoverage.NotStarted(null),
            [row],
            [Finding(RouteListFindingCode.UnknownSource, CliSemanticStatus.Invalid)],
            next));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Default,
                1,
                0,
                ["Coverage confirmed."]),
            [row],
            [],
            null));
        Assert.Throws<ArgumentException>(() =>
            new RouteListFinding(
                RouteListFindingCode.OperationFailed,
                CliSemanticStatus.Complete,
                null,
                "No finding."));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            CompleteCoverage(1),
            [row],
            [],
            next));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route list results reconcile selection roots depth order and child coverage")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResultsReconcileSelectionRootsDepthOrderAndChildCoverage()
    {
        var workspace = RouteListContractTestData.Workspace();
        var loaderRoot = RootRow(directChildCount: 1);
        var child = ChildRow();
        var completeCoverage = RouteListCoverage.Complete(
            RouteListDepth.Default,
            RouteListDepth.Default,
            1,
            2,
            ["Root and child confirmed."]);

        var complete = RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            completeCoverage,
            [loaderRoot, child],
            [],
            null);

        Assert.Equal([loaderRoot, child], complete.Rows);
        var completeEmpty = RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Default,
                0,
                0,
                ["The Loader root set is empty."]),
            [],
            [],
            null);
        Assert.Empty(completeEmpty.Rows);
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            ResolvedSelection("memory", loaderRoot.Path),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Default,
                0,
                0,
                ["No selected root emitted."]),
            [],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            ResolvedSelection("different", ".agents/different/_different.md"),
            CompleteCoverage(1),
            [RootRow(RouteListSelectionProvenance.ExplicitRoot)],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            ResolvedSelection("memory", loaderRoot.Path),
            CompleteCoverage(1),
            [loaderRoot],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Finite(0),
                1,
                1,
                ["Truncated closure fixture."]),
            [loaderRoot],
            [],
            null));
        var firstOutOfOrder = ChildRow("memory/z", ".agents/memory/z.md");
        var secondOutOfOrder = ChildRow("memory/a", ".agents/memory/a.md");
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Default,
                1,
                3,
                ["Out-of-order fixture."]),
            [RootRow(directChildCount: 2), firstOutOfOrder, secondOutOfOrder],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Finite(0),
                RouteListDepth.Finite(0),
                2,
                2,
                ["Out-of-order roots fixture."]),
            [
                RootRow(id: "z", path: ".agents/z/_z.md"),
                RootRow(id: "a", path: ".agents/a/_a.md"),
            ],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            completeCoverage,
            [child, loaderRoot],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Finite(0),
                RouteListDepth.Finite(0),
                1,
                2,
                ["Invalid depth fixture."]),
            [loaderRoot, child],
            [],
            null));
        var wrongParentId = RouteListRow.RoutedLeaf(
            "memory/child",
            ".agents/memory/child.md",
            "different-parent",
            loaderRoot.Path,
            1,
            1,
            "Child",
            [],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                false));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            completeCoverage,
            [loaderRoot, wrongParentId],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            CompleteCoverage(1),
            [loaderRoot],
            [],
            null));
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                RouteListDepth.Default,
                RouteListDepth.Default,
                1,
                0,
                ["No rows confirmed."]),
            [],
            [],
            null));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route list findings enforce status candidates and workspace phase")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FindingsEnforceStatusCandidatesAndWorkspacePhase()
    {
        Assert.Throws<ArgumentException>(() =>
            Finding(RouteListFindingCode.InvalidDepth, CliSemanticStatus.Blocked));
        Assert.Throws<ArgumentException>(() => new RouteListFinding(
            RouteListFindingCode.AmbiguousSource,
            CliSemanticStatus.Blocked,
            "collision",
            "Several sources have the same ID."));
        var ambiguous = new RouteListFinding(
            RouteListFindingCode.AmbiguousSource,
            CliSemanticStatus.Blocked,
            "collision",
            "Several sources have the same ID.",
            [".agents/a.md", ".agents/a/_a.md"]);
        Assert.Equal(2, ambiguous.CandidatePaths.Count);

        var next = new CliNextAction("open-forge route list --help", "Select a valid workspace.");
        var invalidWorkspace = RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.NotStarted(null),
            [],
            [Finding(RouteListFindingCode.InvalidWorkspace, CliSemanticStatus.Invalid)],
            next);
        var unavailableWorkspace = RouteListResult.Create(
            CliSemanticStatus.Blocked,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Blocked(null, null, 0, 0, [], ["Workspace is unavailable."]),
            [],
            [Finding(RouteListFindingCode.WorkspaceUnavailable, CliSemanticStatus.Blocked)],
            next);
        Assert.Null(invalidWorkspace.Workspace);
        Assert.Null(unavailableWorkspace.Workspace);
        Assert.Throws<ArgumentException>(() => RouteListResult.Create(
            CliSemanticStatus.Blocked,
            null,
            RouteListSelectionFactory.AttemptedPath(".agents/unsafe.md"),
            RouteListCoverage.Blocked(null, null, 0, 0, [], ["Unsafe source."]),
            [],
            [Finding(RouteListFindingCode.UnsafeSource, CliSemanticStatus.Blocked)],
            next));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route list next-action rules are exhaustive")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void NextActionRulesAreExhaustive()
    {
        var workspace = RouteListContractTestData.Workspace();
        var explicitRow = RootRow(RouteListSelectionProvenance.ExplicitRoot);
        var resolved = ResolvedSelection("memory", explicitRow.Path);
        var next = new CliNextAction("open-forge route list", "Retry with corrected input.");
        var attentionWithNext = RouteListResult.Create(
            CliSemanticStatus.Attention,
            workspace,
            resolved,
            CompleteCoverage(1),
            [explicitRow],
            [Finding(RouteListFindingCode.AuthoredForm, CliSemanticStatus.Attention)],
            next);
        Assert.Same(next, attentionWithNext.Next);

        var missingNextCases = new Action[]
        {
            () => RouteListResult.Create(
                CliSemanticStatus.Invalid,
                workspace,
                RouteListSelectionFactory.AttemptedId("unknown"),
                RouteListCoverage.NotStarted(null),
                [],
                [Finding(RouteListFindingCode.UnknownSource, CliSemanticStatus.Invalid)],
                null),
            () => RouteListResult.Create(
                CliSemanticStatus.Blocked,
                workspace,
                RouteListSelectionFactory.AttemptedPath(".agents/unsafe.md"),
                RouteListCoverage.Blocked(null, null, 0, 0, [], ["Unsafe source."]),
                [],
                [Finding(RouteListFindingCode.UnsafeSource, CliSemanticStatus.Blocked)],
                null),
            () => RouteListResult.Create(
                CliSemanticStatus.Failed,
                workspace,
                RouteListSelectionFactory.AttemptedId("memory"),
                RouteListCoverage.Failed(null, null, 0, 0, [], ["Failure."]),
                [],
                [Finding(RouteListFindingCode.OperationFailed, CliSemanticStatus.Failed)],
                null),
            () => RouteListResult.Create(
                CliSemanticStatus.Interrupted,
                workspace,
                resolved,
                RouteListCoverage.Interrupted(
                    RouteListDepth.All,
                    null,
                    0,
                    0,
                    [],
                    ["Interrupted."]),
                [],
                [Finding(RouteListFindingCode.Interrupted, CliSemanticStatus.Interrupted)],
                null),
        };
        foreach (var create in missingNextCases)
        {
            Assert.Throws<ArgumentException>(create);
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DetachedCyclesRetainKnownParentsAndCountsAtEveryRequestedDepth(bool twoCatalogues)
    {
        foreach (var depth in new[] { RouteListDepth.Finite(0), RouteListDepth.Finite(1), RouteListDepth.All })
        {
            var root = CycleRow(CycleSelected, twoCatalogues ? CycleParent : CycleSelected, 0, 1);
            var rows = twoCatalogues && depth != RouteListDepth.Finite(0)
                ? new[] { root, CycleRow(CycleParent, CycleSelected, 1, 1) } : [root];
            var result = CycleResult(rows, depth);

            Assert.Equal(rows, result.Rows);
            Assert.Equal(twoCatalogues ? CycleParent : CycleSelected, result.Rows[0].ParentPath);
            Assert.Equal(CycleId(twoCatalogues ? CycleParent : CycleSelected), result.Rows[0].ParentId);
            Assert.All(result.Rows, row =>
            {
                Assert.Null(row.AbsoluteDepth);
                Assert.Equal(1, row.DirectChildCount);
            });
        }
    }

    [Fact]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DetachedSelectedRootDoesNotParticipateInItsParentsPresentedSiblingOrder()
    {
        var root = CycleRow(CycleSelected, CycleParent, 0, 1);
        var parent = CycleRow(CycleParent, CycleSelected, 1, 3);
        var earlier = CycleRow("docs/team/a.md", CycleParent, 2, 0);
        var later = CycleRow("docs/team/b.md", CycleParent, 2, 0);
        var result = CycleResult([root, parent, earlier, later], RouteListDepth.All);

        Assert.Equal(new[] { CycleSelected, CycleParent, "docs/team/a.md", "docs/team/b.md" }, result.Rows.Select(row => row.Path));
        Assert.Equal(3, result.Rows[1].DirectChildCount);
        Assert.Throws<ArgumentException>(() => CycleResult([root, parent, later, earlier], RouteListDepth.All));
    }

    [Theory]
    [InlineData("root-parent-id"), InlineData("descendant-parent-id"), InlineData("relative-depth")]
    [InlineData("absolute-depth"), InlineData("missing-child"), InlineData("missing-return-child")]
    [InlineData("duplicate"), InlineData("ordinary-later-parent"), InlineData("leaf-root")]
    [InlineData("leaf-parent"), InlineData("later-descendant-parent"), InlineData("self-parent-id"), InlineData("unknown-count")]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DetachedReturnEdgesKeepIdentityDepthCountAndOrdinaryOrderingGuards(string scenario)
    {
        var root = CycleRow(CycleSelected, CycleParent, 0, 1);
        var parent = CycleRow(CycleParent, CycleSelected, 1, 1);
        RouteListRow[] rows = scenario switch
        {
            "root-parent-id" => [CycleRow(CycleSelected, CycleParent, 0, 1, parentId: "wrong"), parent],
            "descendant-parent-id" => [root, CycleRow(CycleParent, CycleSelected, 1, 1, parentId: "wrong")],
            "relative-depth" => [root, CycleRow(CycleParent, CycleSelected, 2, 1)],
            "absolute-depth" => [root, CycleRow(CycleParent, CycleSelected, 1, 1, absoluteDepth: 1)],
            "missing-child" => [CycleRow(CycleSelected, CycleParent, 0, 2), parent],
            "missing-return-child" => [root, CycleRow(CycleParent, CycleSelected, 1, 2)],
            "duplicate" => [root, parent, parent],
            "ordinary-later-parent" => [CycleRow(CycleSelected, CycleParent, 0, 1,
                provenance: RouteListSelectionProvenance.ExplicitRoot, absoluteDepth: 1),
                CycleRow(CycleParent, CycleSelected, 1, 1, absoluteDepth: 2)],
            "leaf-root" => [CycleLeaf(CycleSelected, CycleParent, 0), parent],
            "leaf-parent" => [root, CycleLeaf(CycleParent, CycleSelected, 1)],
            "later-descendant-parent" => [root, CycleLeaf("docs/team/a.md", CycleParent, 2),
                CycleRow(CycleParent, CycleSelected, 1, 2)],
            "self-parent-id" => [CycleRow(CycleSelected, CycleSelected, 0, 1, parentId: "wrong")],
            "unknown-count" => [CycleRow(CycleSelected, CycleParent, 0, null), parent],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

        Assert.Throws<ArgumentException>(() => CycleResult(rows, RouteListDepth.All));
    }

    private const string CycleSelected = "docs/team/other.md";
    private const string CycleParent = "docs/team/_team.md";

    private static string CycleId(string path) => SourceWorkspaceRelativePath.LiteralReferencePrefix + path;

    private static RouteListRow CycleRow(string path, string parentPath, int depth, int? count,
        string? parentId = null, RouteListSelectionProvenance? provenance = null, int? absoluteDepth = null)
        => RouteListRow.Entrypoint(CycleId(path), path, parentId ?? CycleId(parentPath), parentPath,
            absoluteDepth, depth, "Authored catalogue", [], count,
            new(provenance ?? (depth == 0 ? RouteListSelectionProvenance.DetachedRoot : RouteListSelectionProvenance.Descendant),
                RouteListSourceProvenance.AuthoredEntrypoint, false));

    private static RouteListRow CycleLeaf(string path, string parentPath, int depth)
        => RouteListRow.RoutedLeaf(CycleId(path), path, CycleId(parentPath), parentPath,
            null, depth, "Authored leaf", [], new(depth == 0 ? RouteListSelectionProvenance.DetachedRoot : RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf, false));

    private static RouteListResult CycleResult(RouteListRow[] rows, RouteListDepth depth)
        => RouteListResult.Create(CliSemanticStatus.Complete, RouteListContractTestData.Workspace(),
            RouteListSelectionFactory.ResolvedIncluded(RouteSourceTestData.LogicalSource(CycleSelected, CycleId(CycleSelected))),
            RouteListCoverage.Complete(depth, RouteListDepth.Finite(rows.Max(row => row.RelativeDepth)), 1, rows.Length, []), rows, [], null);

    private static RouteListRow RootRow(
        RouteListSelectionProvenance selection = RouteListSelectionProvenance.LoaderRoot,
        int? directChildCount = 0,
        string id = "memory",
        string path = ".agents/memory/_memory.md")
    {
        return RouteListRow.Entrypoint(
            id,
            path,
            null,
            null,
            0,
            0,
            "Memory description",
            ["Memory"],
            directChildCount,
            new RouteListProvenance(
                selection,
                RouteListSourceProvenance.AuthoredEntrypoint,
                hasOverwrite: false));
    }

    private static RouteListRow ChildRow(
        string id = "memory/child",
        string path = ".agents/memory/child.md")
    {
        return RouteListRow.RoutedLeaf(
            id,
            path,
            "memory",
            ".agents/memory/_memory.md",
            1,
            1,
            "Child",
            ["Memory"],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                false));
    }

    private static RouteListCoverage CompleteCoverage(int rows)
    {
        return RouteListCoverage.Complete(
            RouteListDepth.Default,
            RouteListDepth.Default,
            selectedRootCount: 1,
            confirmedRowCount: rows,
            ["Loader roots and requested descendants were confirmed."]);
    }

    private static RouteListFinding Finding(RouteListFindingCode code, CliSemanticStatus status)
    {
        return new RouteListFinding(code, status, "memory", "A bounded route-list finding.");
    }

    private static RouteListSelection ResolvedSelection(string id, string canonicalPath)
    {
        var source = RouteSourceTestData.Source(
            canonicalPath,
            RouteSourceKind.Entrypoint);
        return RouteListSelectionFactory.ResolvedId(id, source);
    }

}
