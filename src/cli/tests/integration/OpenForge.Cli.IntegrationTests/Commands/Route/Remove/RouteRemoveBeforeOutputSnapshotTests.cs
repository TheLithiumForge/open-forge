using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteRemoveBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteRemoveResult> Renderers = CommandOutputRenderers<RouteRemoveResult>.From(RouteRemovePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route remove output preserves detached references when the later source deletion is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic deletion failure requires Windows file sharing.");
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-output-partial");
        var sourceBefore = File.ReadAllBytes(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath));
        RouteRemoveResult result;
        using (var held = File.Open(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                new RouteRemoveRequest(
                    workspace.Workspace,
                    RouteRemoveIntegrationWorkspace.LeafId,
                    RouteRemoveMode.Apply,
                    automatic: true),
                TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal("Prefix Old guide suffix.\n", workspace.ReadText("README.md"));
        Assert.Equal(sourceBefore, File.ReadAllBytes(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.NotNull(result.Recovery.ResidualPath);
        AssertRecovery(result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route remove reports an unavailable ambiguous source without interactive input")]
    public async Task AttemptedAmbiguousSourcePrompt()
    {
        using var workspace = new RoutedOutputWorkspace();
        workspace.CollidingGuideId();
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "ambiguous-source-prompt",
            Arguments = ["route", "remove", "docs/guide"],
            ExitCode = 5,
            DiagnosticId = "route-remove.identity-collision",
        });
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route remove output preserves leaf and category removal and link detachment")]
    [InlineData("leaf-removed")]
    [InlineData("leaf-with-detached-links")]
    [InlineData("category-removed")]
    [InlineData("dry-run")]
    public async Task Removal(string situation)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-output");
        if (situation == "leaf-removed") workspace.WriteText("README.md", "# No incoming leaf links\n");
        var category = situation == "category-removed";
        var before = workspace.SnapshotHashes();
        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(workspace.Workspace, category ? RouteRemoveIntegrationWorkspace.CategoryId : RouteRemoveIntegrationWorkspace.LeafId,
                situation == "dry-run" ? RouteRemoveMode.DryRun : RouteRemoveMode.Apply,
                automatic: situation != "dry-run"), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        if (situation == "dry-run") Assert.Equal(before, workspace.SnapshotHashes());
        else Assert.False(File.Exists(workspace.Combine(category ? RouteRemoveIntegrationWorkspace.CategoryPath : RouteRemoveIntegrationWorkspace.LeafPath)));
        if (situation == "leaf-with-detached-links") Assert.Equal("Prefix Old guide suffix.\n", workspace.ReadText("README.md"));
        AssertRecovery(result);
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath,
            testName: $"{nameof(Removal)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route remove output preserves ownership, reference and execution refusals without effects")]
    [InlineData("source-not-found", (int)CliSemanticStatus.Complete)]
    [InlineData("ownership-unavailable", (int)CliSemanticStatus.Blocked)]
    [InlineData("unsafe-link-detach", (int)CliSemanticStatus.Blocked)]
    [InlineData("reference-scan-incomplete", (int)CliSemanticStatus.Incomplete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    public async Task SafetyBoundary(string situation, int status)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-output-boundary");
        if (situation == "ownership-unavailable") workspace.WriteText(RouteRemoveIntegrationWorkspace.OwnershipPath, "{ invalid ownership json");
        if (situation == "unsafe-link-detach") workspace.WriteText("outside.md", "Before [Readable guide][target], after.\n\n[target]: .agents/guidance/old%20guide.md\n");
        if (situation == "reference-scan-incomplete") workspace.SeedInvalidUtf8();
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        using var held = situation == "lock-held" ? workspace.HoldLock() : null;
        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                situation == "source-not-found" ? "missing" : RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            cancellation.Token);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation == "source-not-found")
        {
            Assert.Empty(result.Findings);
            Assert.Equal(RouteRemovePlanCompleteness.Complete, result.Plan.Completeness);
            Assert.Equal(RouteRemovePlanSafety.Safe, result.Plan.Safety);
            Assert.Equal(RouteRemoveVerificationState.Verified, result.Verification);
            Assert.Empty(result.Effects);
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertRecovery(result);
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath,
            testName: $"{nameof(SafetyBoundary)}_{situation}");
    }

    private static void AssertRecovery(RouteRemoveResult result)
    {
        if (result.Recovery.ResidualPath is { } path) Assert.True(File.Exists(path));
    }
}
