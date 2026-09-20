using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteMoveBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteMoveResult> Renderers = CommandOutputRenderers<RouteMoveResult>.From(RouteMovePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route move output preserves copied destination files when a later reference rewrite is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-output-partial");
        workspace.OwnLeafDestination(crossRoute: false);
        var referenceBefore = File.ReadAllBytes(workspace.Absolute("README.md"));
        RouteMoveResult result;
        using (var held = File.Open(workspace.Absolute("README.md"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await RouteMoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination, RouteMoveMode.Apply),
                TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafDestination)));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafPath)));
        Assert.Equal(referenceBefore, File.ReadAllBytes(workspace.Absolute("README.md")));
        Assert.NotNull(result.Recovery.ResidualPath);
        CaptureRecovery(workspace, result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route move reports an unavailable ambiguous source without interactive input")]
    public async Task AttemptedAmbiguousSourcePrompt()
    {
        using var workspace = new RoutedOutputWorkspace();
        workspace.CollidingGuideId();
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "ambiguous-source-prompt",
            Arguments = ["route", "move", "docs/guide", "docs/moved"],
            ExitCode = 5,
            DiagnosticId = "route-move.identity-collision",
        });
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route move output preserves leaf and category moves with rewritten references")]
    public async Task Move()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[] { "leaf-move", "leaf-move-with-rewritten-links", "category-move", "dry-run" })
        {
            using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-output");
            var category = situation == "category-move";
            if (category) workspace.OwnCategoryDestination();
            else if (situation != "dry-run") workspace.OwnLeafDestination(situation != "leaf-move");
            var source = category ? RouteMoveIntegrationWorkspace.CategoryId : RouteMoveIntegrationWorkspace.LeafId;
            var oldPath = category ? RouteMoveIntegrationWorkspace.CategoryPath : RouteMoveIntegrationWorkspace.LeafPath;
            var destination = situation switch
            {
                "category-move" => RouteMoveIntegrationWorkspace.CategoryDestination,
                "leaf-move" => RouteMoveIntegrationWorkspace.LeafDestination,
                _ => RouteMoveIntegrationWorkspace.CrossRouteDestination,
            };
            var before = workspace.SnapshotHashes();
            var result = await RouteMoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(source, destination, situation == "dry-run" ? RouteMoveMode.DryRun : RouteMoveMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            if (situation == "dry-run") Assert.Equal(before, workspace.SnapshotHashes());
            else
            {
                Assert.False(File.Exists(workspace.Absolute(oldPath)));
                Assert.True(File.Exists(workspace.Absolute(destination)));
            }

            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(Move));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route move output preserves destination, ownership and scan refusals without effects")]
    public async Task SafetyBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var scenario in new[]
        {
            (Situation: "destination-exists", Status: CliSemanticStatus.Blocked),
            (Situation: "destination-inside-source", Status: CliSemanticStatus.Invalid),
            (Situation: "self-move", Status: CliSemanticStatus.Invalid),
            (Situation: "managed-source", Status: CliSemanticStatus.Blocked),
            (Situation: "source-not-found", Status: CliSemanticStatus.Invalid),
            (Situation: "reference-scan-incomplete", Status: CliSemanticStatus.Incomplete),
            (Situation: "lock-held", Status: CliSemanticStatus.Blocked),
            (Situation: "cancelled", Status: CliSemanticStatus.Interrupted),
        })
        {
            var situation = scenario.Situation;
            using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-output-boundary");
            if (situation == "destination-exists") workspace.SeedScenario("occupied-destination");
            if (situation == "managed-source") workspace.SeedScenario("ownership-claim");
            if (situation == "reference-scan-incomplete") workspace.SeedScenario("invalid-utf8");
            var source = situation switch
            {
                "source-not-found" => "missing",
                "destination-inside-source" => RouteMoveIntegrationWorkspace.CategoryId,
                _ => RouteMoveIntegrationWorkspace.LeafId,
            };
            var destination = situation switch
            {
                "self-move" => RouteMoveIntegrationWorkspace.LeafPath,
                "destination-inside-source" => ".agents/guidance/topics/nested/_nested.md",
                _ => RouteMoveIntegrationWorkspace.LeafDestination,
            };
            var before = workspace.SnapshotHashes();
            using var cancellation = new CancellationTokenSource();
            if (situation == "cancelled") cancellation.Cancel();
            using var held = situation == "lock-held" ? workspace.HoldLock() : null;
            var result = await RouteMoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(source, destination, RouteMoveMode.Apply), cancellation.Token);
            Assert.Equal(scenario.Status, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            CaptureRecovery(workspace, result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(SafetyBoundary));
    }

    private static void CaptureRecovery(RouteMoveIntegrationWorkspace workspace, RouteMoveResult result)
    {
        if (result.Recovery.ResidualPath is { } path)
        {
            workspace.TrackRecoveryPath(path);
            Assert.True(File.Exists(path));
        }
    }
}
