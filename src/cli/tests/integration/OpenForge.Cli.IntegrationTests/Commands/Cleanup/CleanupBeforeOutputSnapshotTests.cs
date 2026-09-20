using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Presentation.Cleanup;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class CleanupBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Cleanup output preserves empty, applied, preview and damaged recovery catalogues")]
    public async Task RecoveryCatalogue()
    {
        var situations = new (string Situation, CliSemanticStatus Status)[]
        {
            ("nothing-to-remove", CliSemanticStatus.Complete),
            ("two-bundles-one-draft", CliSemanticStatus.Complete),
            ("dry-run", CliSemanticStatus.Complete),
            ("damaged-bundle", CliSemanticStatus.Attention),
            ("lock-held", CliSemanticStatus.Blocked),
            ("deletion-failed-partial", CliSemanticStatus.Failed),
        };
        if (!OperatingSystem.IsWindows())
        {
            situations = situations
                .Where(situation => situation.Situation != "deletion-failed-partial")
                .ToArray();
        }

        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in situations)
        {
            using var workspace = CleanupIntegrationWorkspace.Create("cleanup-output");
            var paths = new List<string>();
            if (situation == "damaged-bundle")
            {
                paths.Add(workspace.AddMalformedFinal(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            }
            else if (situation != "nothing-to-remove")
            {
                paths.Add(await workspace.AddVerifiedFinalAsync(RecoveryBundleProducer.Index, RecoveryBundleOperation.Index,
                    "open-forge index", Guid.Parse("11111111-1111-1111-1111-111111111111")));
                paths.Add(await workspace.AddVerifiedFinalAsync(RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair,
                    "open-forge repair", Guid.Parse("22222222-2222-2222-2222-222222222222")));
                paths.Add(workspace.AddDraft(Guid.Parse("33333333-3333-3333-3333-333333333333")));
            }

            Assert.All(paths, path => Assert.True(File.Exists(path)));
            var before = workspace.SnapshotWorkspace();
            var recoveryBefore = workspace.SnapshotRecovery();
            using var held = situation == "lock-held" ? workspace.HoldLock() : null;
            CleanupResult result;
            using (var denied = situation == "deletion-failed-partial"
                       ? File.Open(paths[1], FileMode.Open, FileAccess.Read, FileShare.Read) : null)
            {
                result = await new CleanupOperation(workspace.LockStoreRoot).ExecuteAsync(
                    new CleanupRequest(workspace.Workspace, situation == "dry-run" ? CleanupMode.DryRun : CleanupMode.Apply), TestContext.Current.CancellationToken);
            }
            Assert.Equal(status, result.Status);
            Assert.Equal(before, workspace.SnapshotWorkspace());
            if (situation == "two-bundles-one-draft")
            {
                Assert.All(paths, path => Assert.False(File.Exists(path)));
                Assert.Equal(3, result.Effects.Length);
            }
            else if (situation == "deletion-failed-partial")
            {
                Assert.False(File.Exists(paths[0]));
                Assert.True(File.Exists(paths[1]));
                Assert.Equal(recoveryBefore[Path.GetFileName(paths[1])], workspace.SnapshotRecovery()[Path.GetFileName(paths[1])]);
                Assert.True(File.Exists(paths[2]));
                Assert.Equal(recoveryBefore[Path.GetFileName(paths[2])], workspace.SnapshotRecovery()[Path.GetFileName(paths[2])]);
                Assert.Equal(CleanupEffectOutcome.Verified, result.Effects[0].Outcome);
                Assert.Equal(CleanupEffectOutcome.NotStarted, result.Effects[2].Outcome);
            }
            else
            {
                Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
            }

            var renderers = CommandOutputRenderers<CleanupResult>.From(
                CleanupPresentation.Rendering,
                output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths),
                output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths));
            renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Cleanup output preserves cancellation between two real deletion stages")]
    public async Task CancelledBetweenRealDeletionStages()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-output-cancelled-stages");
        string[] paths =
        [
            await workspace.AddVerifiedFinalAsync(RecoveryBundleProducer.Index, RecoveryBundleOperation.Index,
                "open-forge index", Guid.Parse("11111111-1111-1111-1111-111111111111")),
            await workspace.AddVerifiedFinalAsync(RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair,
                "open-forge repair", Guid.Parse("22222222-2222-2222-2222-222222222222")),
            workspace.AddDraft(Guid.Parse("33333333-3333-3333-3333-333333333333")),
        ];
        var before = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();
        var preview = await new CleanupOperation(workspace.LockStoreRoot).ExecuteAsync(
            new CleanupRequest(workspace.Workspace, CleanupMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        var request = new CleanupRequest(workspace.Workspace, CleanupMode.Apply);
        var frozen = await RecoveryBundleCatalogue.ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleCatalogueState.Available, frozen.State);
        var plan = CleanupPlanner.Create(request, frozen);
        var acquired = await new WorkspaceLockManager(workspace.LockStoreRoot).AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, CleanupDefinitions.CommandIdentity, CleanupPlanner.LeaseOperationId(request)),
            TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, acquired.State);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var progress = new CleanupDeletionProgress(plan);
        var first = await session.DeleteAsync(plan.DeletionEntries[0].Candidate.Snapshot, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, first.State);
        progress.Record(first);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var second = await session.DeleteAsync(plan.DeletionEntries[1].Candidate.Snapshot, cancellation.Token);
        Assert.Equal(RecoveryBundleDeletionState.Cancelled, second.State);
        Assert.Equal(RecoveryBundleDisposition.Retained, second.Disposition);
        progress.Record(second);
        Assert.Null(progress.Next);
        Assert.Equal(CleanupEffectOutcome.Verified, progress.Effects[0].Outcome);
        Assert.Equal(CleanupEffectOutcome.NotStarted, progress.Effects[1].Outcome);
        Assert.Equal(CleanupEffectOutcome.NotStarted, progress.Effects[2].Outcome);

        // The top-level loop has no between-deletion seam. These boundary views map the
        // asserted real stage outcomes exactly as CleanupApplicationOperation does.
        var builder = new CleanupResultBuilder(plan)
        {
            Preflight = preview.Preflight,
            Lease = new CleanupLease { State = CleanupLeaseState.Acquired, Cause = acquired.Cause },
            Revalidation = new CleanupCatalogueComparison
            {
                State = CleanupCatalogueComparisonState.Matched,
                Planned = plan.Catalogue,
                Observed = CleanupPlanner.ReadCatalogue(request, Assert.IsType<RecoveryBundleCatalogueResult>(opened.ObservedCatalogue)),
                Cause = opened.Cause,
            },
            Effects = progress.Effects,
            Findings = progress.Findings,
            Verification = new CleanupVerification
            {
                State = CleanupVerificationState.Unknown,
                Cause = Assert.Single(progress.Findings).Cause,
            },
        };
        var result = builder.Build();
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotWorkspace());
        Assert.False(File.Exists(paths[0]));
        Assert.Equal(recoveryBefore[Path.GetFileName(paths[1])], workspace.SnapshotRecovery()[Path.GetFileName(paths[1])]);
        Assert.Equal(recoveryBefore[Path.GetFileName(paths[2])], workspace.SnapshotRecovery()[Path.GetFileName(paths[2])]);
        var renderers = CommandOutputRenderers<CleanupResult>.From(
            CleanupPresentation.Rendering,
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths),
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths));
        renderers.MatchDetails(result, "cancelled-partial");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Cleanup output preserves invalid operands without effects")]
    public async Task InvalidInput()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-output-invalid");
        var before = workspace.SnapshotWorkspace();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["cleanup", "unexpected-operand"],
            ExitCode = 4,
            ShellDiagnostic = true,
        });
        Assert.Equal(before, workspace.SnapshotWorkspace());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Cleanup output preserves an unavailable owned recovery store")]
    public async Task StoreUnreadable()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-output-store");
        var store = workspace.BlockRecoveryDirectory();
        var before = File.ReadAllBytes(store);
        var result = await new CleanupOperation(workspace.LockStoreRoot).ExecuteAsync(
            new CleanupRequest(workspace.Workspace, CleanupMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(before, File.ReadAllBytes(store));
        var renderers = CommandOutputRenderers<CleanupResult>.From(
            CleanupPresentation.Rendering,
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, [], store),
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, [], store));
        renderers.MatchDetails(result, "store-unreadable");
    }

}
