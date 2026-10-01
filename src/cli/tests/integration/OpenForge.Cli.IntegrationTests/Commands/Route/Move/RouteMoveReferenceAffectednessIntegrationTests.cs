using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveReferenceAffectednessIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move exempts a stationary unrelated missing link while rewriting and applying a leaf")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task StationaryUnrelatedMissingLeafReferenceAppliesAndVerifies()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-unrelated-leaf");
        workspace.WriteText("docs/.keep", string.Empty);
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md") + "\n[Missing](docs/missing.md)\n");
        workspace.OwnLeafDestination(crossRoute: false);

        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.LeafDestination,
            RouteMoveMode.Apply);
        TrackRecovery(workspace, result);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteMoveVerificationState.Verified, result.Verification);
        Assert.Equal(RouteMoveCoverage.Complete, result.References.Coverage);
        Assert.False(File.Exists(workspace.Absolute("docs/missing.md")));
        Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafPath)));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafDestination)));
        Assert.Contains("[Missing](docs/missing.md)", workspace.ReadText("README.md"), StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/new%20guide.md", workspace.ReadText("README.md"), StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move exempts a stationary unrelated missing link during a category move and verifies it")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task StationaryUnrelatedMissingCategoryReferenceAppliesAndVerifies()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-unrelated-category");
        workspace.WriteText("docs/.keep", string.Empty);
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md") + "\n[Missing](docs/missing.md)\n");
        workspace.OwnCategoryDestination();

        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.CategoryId,
            RouteMoveIntegrationWorkspace.CategoryDestination,
            RouteMoveMode.Apply);
        TrackRecovery(workspace, result);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteMoveVerificationState.Verified, result.Verification);
        Assert.Equal(RouteMoveCoverage.Complete, result.References.Coverage);
        Assert.False(File.Exists(workspace.Absolute("docs/missing.md")));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/guidance/topics")));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.CategoryDestination)));
        Assert.Contains("[Missing](docs/missing.md)", workspace.ReadText("README.md"), StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Move refuses a missing target under either category move root without writes"),
        InlineData(".agents/guidance/topics/missing.md"),
        InlineData(".agents/archive/topics/missing.md")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingCategoryCoordinateRemainsIncomplete(
        string missingTarget)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-category-coordinate");
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md") + $"\n[Missing]({missingTarget})\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.CategoryId,
            RouteMoveIntegrationWorkspace.CategoryDestination,
            RouteMoveMode.DryRun);

        AssertIncompleteReferenceBoundary(result);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Move refuses a missing future leaf or overwrite coordinate without writes"),
        InlineData(".agents/guidance/new%20guide.md"),
        InlineData(".agents/guidance/new%20guide.overwrite.md")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingLeafCoordinateRemainsIncomplete(
        string missingTarget)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-leaf-coordinate");
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md") + $"\n[Missing]({missingTarget})\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.LeafDestination,
            RouteMoveMode.DryRun);

        AssertIncompleteReferenceBoundary(result);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move refuses a missing outgoing link authored inside a moved leaf without writes")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingOutgoingReferenceInsideMovedLeafRemainsIncomplete()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-moved-source");
        workspace.WriteText(
            RouteMoveIntegrationWorkspace.LeafPath,
            workspace.ReadText(RouteMoveIntegrationWorkspace.LeafPath)
                + "\n[Missing](docs/missing.md)\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.LeafDestination,
            RouteMoveMode.DryRun);

        AssertIncompleteReferenceBoundary(result);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move exempts a missing target below topics-extra without confusing the category boundary")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingComponentBoundarySiblingIsUnaffected()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-component-boundary");
        workspace.WriteText(".agents/guidance/topics-extra/.keep", string.Empty);
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md")
                + "\n[Missing](.agents/guidance/topics-extra/missing.md)\n");

        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(
                RouteMoveIntegrationWorkspace.CategoryId,
                RouteMoveIntegrationWorkspace.CategoryDestination),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteMovePlan>(build.Plan);
        var document = Assert.Single(
            plan.Projection.References.Documents,
            document => document.SourcePath == "README.md");
        var meaning = Assert.Single(
            document.Meanings,
            meaning => meaning.TargetPath == ".agents/guidance/topics-extra/missing.md");

        Assert.Equal(CliSemanticStatus.Complete, new RouteMoveResultBuilder().Build(build.Formation).Status);
        Assert.Equal(SourceLinkTargetResolution.Missing, meaning.Resolution);
        Assert.Null(meaning.TargetId);
        Assert.Null(meaning.Layer);
        Assert.Null(meaning.Fragment);
        Assert.Contains(
            "[Missing](.agents/guidance/topics-extra/missing.md)",
            document.IntendedText,
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move preserves a percent-encoded unrelated missing target and fragment meaning")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task PercentEncodedMissingTargetRetainsLiteralAndMeaning()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-encoded-missing");
        workspace.WriteText("docs/.keep", string.Empty);
        workspace.WriteText(
            "README.md",
            workspace.ReadText("README.md")
                + "\n[Missing](docs/missing%20file.md#section)\n");

        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteMovePlan>(build.Plan);
        var document = Assert.Single(
            plan.Projection.References.Documents,
            document => document.SourcePath == "README.md");
        var meaning = Assert.Single(
            document.Meanings,
            meaning => meaning.TargetPath == "docs/missing file.md");

        Assert.Equal(CliSemanticStatus.Complete, new RouteMoveResultBuilder().Build(build.Formation).Status);
        Assert.Equal(SourceLinkTargetResolution.Missing, meaning.Resolution);
        Assert.Equal("section", meaning.Fragment);
        Assert.Contains(
            "[Missing](docs/missing%20file.md#section)",
            document.IntendedText,
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Move retains refusal for malformed, escaping, aliased and unreadable references"),
        InlineData("malformed", (int)CliSemanticStatus.Incomplete, (int)RouteMoveFindingCode.ReferenceCoverageIncomplete),
        InlineData("escaping", (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.ReferenceUnsafe),
        InlineData("internal-alias", (int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.ReferenceUnsafe),
        InlineData("unreadable-source", (int)CliSemanticStatus.Incomplete, (int)RouteMoveFindingCode.ReferenceCoverageIncomplete)]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnprovedReferenceRetainsCurrentRefusal(
        string scenario,
        int expectedStatusValue,
        int expectedFindingValue)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            $"route-move-reference-refusal-{scenario}");
        switch (scenario)
        {
            case "malformed":
                workspace.WriteText(
                    "README.md",
                    workspace.ReadText("README.md") + "\n[Malformed](docs/missing%ZZ.md)\n");
                break;
            case "escaping":
                workspace.WriteText(
                    "README.md",
                    workspace.ReadText("README.md") + "\n[Escaping](../../outside.md)\n");
                break;
            case "internal-alias":
                workspace.SeedScenario("unsafe-reference");
                break;
            case "unreadable-source":
                workspace.SeedScenario("invalid-utf8");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The refusal scenario is not defined.");
        }

        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(
            workspace,
            RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.LeafDestination,
            RouteMoveMode.DryRun);

        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == (RouteMoveFindingCode)expectedFindingValue);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move retains refusal for a dangling alias in missing-target ancestry")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task DanglingAliasAncestryRetainsCurrentRefusal()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-reference-dangling-alias");
        var aliasPath = workspace.Absolute("docs");
        File.CreateSymbolicLink(aliasPath, workspace.Absolute("missing-directory"));
        try
        {
            workspace.WriteText(
                "README.md",
                workspace.ReadText("README.md") + "\n[Missing](docs/missing.md)\n");
            var before = workspace.SnapshotHashes();

            var result = await ExecuteAsync(
                workspace,
                RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination,
                RouteMoveMode.DryRun);

            AssertIncompleteOrUnsafeReferenceBoundary(result);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        finally
        {
            File.Delete(aliasPath);
        }
    }

    private static async ValueTask<RouteMoveResult> ExecuteAsync(
        RouteMoveIntegrationWorkspace workspace,
        string source,
        string destination,
        RouteMoveMode mode)
        => await RouteMoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request(source, destination, mode),
            TestContext.Current.CancellationToken);

    private static void AssertIncompleteReferenceBoundary(RouteMoveResult result)
    {
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteMoveFindingCode.ReferenceCoverageIncomplete);
        Assert.Equal(RouteMoveCoverage.Incomplete, result.References.Coverage);
    }

    private static void AssertIncompleteOrUnsafeReferenceBoundary(RouteMoveResult result)
    {
        Assert.True(
            result.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete);
        Assert.Contains(
            result.Findings,
            finding => finding.Code is
                RouteMoveFindingCode.ReferenceUnsafe
                or RouteMoveFindingCode.ReferenceCoverageIncomplete);
    }

    private static void TrackRecovery(
        RouteMoveIntegrationWorkspace workspace,
        RouteMoveResult result)
    {
        if (result.Recovery.ResidualPath is { } residualPath)
        {
            workspace.TrackRecoveryPath(residualPath);
            Assert.True(File.Exists(residualPath));
        }
    }
}
