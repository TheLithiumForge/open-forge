using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListCoverageAndResultTests
{
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
            [Finding(RouteListFindingCode.MetadataMissing, CliSemanticStatus.Incomplete)],
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
            var compact = RouteListHumanRenderer.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
            var expanded = RouteListHumanRenderer.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));
            var json = RouteListJsonRenderer.Render(new CliPresentationRequest<RouteListResult>(
                result,
                new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
            using var document = JsonDocument.Parse(json);

            Assert.Contains($"result={expectedStatus}", compact, StringComparison.Ordinal);
            Assert.Contains($"Result: {expectedStatus}", expanded, StringComparison.Ordinal);
            Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
            if (result.Rows.Count > 0 && result.Findings.Count > 0)
            {
                Assert.True(
                    compact.IndexOf("finding code=", StringComparison.Ordinal)
                    < compact.IndexOf(
                        $"{Environment.NewLine}{result.Rows[0].Id}  {result.Rows[0].Path}  description=",
                        StringComparison.Ordinal));
                Assert.True(
                    expanded.IndexOf("Finding 1:", StringComparison.Ordinal)
                    < expanded.IndexOf($"ID: {result.Rows[0].Id}", StringComparison.Ordinal));
                Assert.True(
                    expanded.IndexOf("Next:", StringComparison.Ordinal)
                    < expanded.IndexOf($"ID: {result.Rows[0].Id}", StringComparison.Ordinal));
            }
        }
        Assert.Equal(1, complete.SchemaVersion);
        Assert.Equal("route list", complete.Command);
        Assert.Equal("Review valid route-list input.", invalid.Next?.Reason);
    }

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
            [Finding(RouteListFindingCode.MetadataMissing, CliSemanticStatus.Incomplete)],
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
        var physicalPath = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        var source = new RouteListSource(
            id,
            canonicalPath,
            physicalPath,
            RouteListSourceKind.Entrypoint);
        return RouteListSelectionFactory.ResolvedId(id, source);
    }

}
