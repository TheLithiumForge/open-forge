using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexOperationIntegrationTests
{
    [Fact(DisplayName = "Index operation refuses unsafe targets and missing generated regions without writes"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task InitialUnsafeAndMissingGeneratedBoundariesAreReadOnly()
    {
        using var missing = IndexOperationWorkspace.Create("index-operation-missing-region");
        missing.ReplaceRootText("# Root\n");
        var missingBefore = missing.SnapshotHashes();

        var missingResult = await IndexOperationFactory.Create(missing.LockStoreRoot).ExecuteAsync(
            missing.Request(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, missingResult.Status);
        Assert.Equal(IndexFindingCode.GeneratedRegionUnsafe, Assert.Single(missingResult.Findings).Code);
        Assert.Equal(IndexRegionOutcome.NotEstablished, Assert.Single(missingResult.Regions).Outcome);
        Assert.Equal(IndexRecoveryState.NotRequired, missingResult.Recovery.State);
        Assert.Equal(missingBefore, missing.SnapshotHashes());

        using var unsafeTarget = IndexOperationWorkspace.Create("index-operation-unsafe-target");
        unsafeTarget.ReplaceRootBytes([0xC3, 0x28]);
        var unsafeBefore = unsafeTarget.SnapshotHashes();

        var unsafeResult = await IndexOperationFactory.Create(unsafeTarget.LockStoreRoot).ExecuteAsync(
            unsafeTarget.Request(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, unsafeResult.Status);
        Assert.Equal(IndexFindingCode.TargetUnsafe, Assert.Single(unsafeResult.Findings).Code);
        Assert.Equal(IndexRecoveryState.NotRequired, unsafeResult.Recovery.State);
        Assert.Equal(unsafeBefore, unsafeTarget.SnapshotHashes());
    }

    [Fact(DisplayName = "Index dry-run reports exact updates without workspace or recovery effects"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task DryRunCreatesNoWritesOrRecoveryArtifact()
    {
        using var workspace = IndexOperationWorkspace.Create("index-operation-dry-run");
        var before = workspace.SnapshotHashes();

        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request(IndexMode.DryRun),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var region = Assert.Single(result.Regions);
        Assert.Equal(IndexRegionAction.Update, region.Action);
        Assert.Equal(IndexRegionOutcome.NotRequested, region.Outcome);
        Assert.Equal(IndexRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Index apply verifies exact bounded bytes, removes recovery, and is idempotent"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task ApplyPreservesUnrelatedBytesRemovesRecoveryAndSecondRunIsNoOp()
    {
        using var workspace = IndexOperationWorkspace.Create("index-operation-apply");
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);

        var first = await operation.ExecuteAsync(
            workspace.Request(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        var applied = Assert.Single(first.Regions);
        Assert.Equal(IndexRegionAction.Update, applied.Action);
        Assert.Equal(IndexRegionOutcome.Verified, applied.Outcome);
        Assert.Equal(IndexRecoveryState.Removed, first.Recovery.State);
        Assert.Empty(first.Findings);
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = IndexOperationWorkspace.ExpectedEntry,
                Prefix = IndexOperationWorkspace.RootPrefix,
            }),
            await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
        var afterFirst = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(
            workspace.Request(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        var unchanged = Assert.Single(second.Regions);
        Assert.Equal(IndexRegionAction.Unchanged, unchanged.Action);
        Assert.Equal(IndexRegionOutcome.AlreadyCurrent, unchanged.Outcome);
        Assert.Equal(IndexRecoveryState.NotRequired, second.Recovery.State);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Index apply verifies multiple targets in canonical order with one removed recovery bundle"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task MultiTargetApplyPreservesExactBytesRemovesRecoveryAndSecondRunIsNoOp()
    {
        using var workspace = IndexOperationWorkspace.CreateMultiTarget("index-operation-multi-target");
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);

        var first = await operation.ExecuteAsync(
            workspace.MultiTargetRequest(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Collection(
            first.Regions,
            region => AssertRegion(
                region: region,
                path: IndexOperationWorkspace.AlphaPath,
                action: IndexRegionAction.Update,
                outcome: IndexRegionOutcome.Verified),
            region => AssertRegion(
                region: region,
                path: IndexOperationWorkspace.BetaPath,
                action: IndexRegionAction.Update,
                outcome: IndexRegionOutcome.Verified));
        Assert.Equal(IndexRecoveryState.Removed, first.Recovery.State);
        Assert.Empty(first.Findings);
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = IndexOperationWorkspace.AlphaExpectedEntry,
                Prefix = IndexOperationWorkspace.AlphaPrefix,
            }),
            await workspace.ReadTargetAsync(
                IndexOperationWorkspace.AlphaPath,
                TestContext.Current.CancellationToken));
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = IndexOperationWorkspace.BetaExpectedEntry,
                Prefix = IndexOperationWorkspace.BetaPrefix,
            }),
            await workspace.ReadTargetAsync(
                IndexOperationWorkspace.BetaPath,
                TestContext.Current.CancellationToken));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
        var afterFirst = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(
            workspace.MultiTargetRequest(IndexMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Collection(
            second.Regions,
            region => AssertRegion(
                region: region,
                path: IndexOperationWorkspace.AlphaPath,
                action: IndexRegionAction.Unchanged,
                outcome: IndexRegionOutcome.AlreadyCurrent),
            region => AssertRegion(
                region: region,
                path: IndexOperationWorkspace.BetaPath,
                action: IndexRegionAction.Unchanged,
                outcome: IndexRegionOutcome.AlreadyCurrent));
        Assert.Equal(IndexRecoveryState.NotRequired, second.Recovery.State);
        Assert.Empty(second.Findings);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Index apply reports real lock unavailability without writing or leaking raw failure text"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task HeldRealLockMapsToUnavailableAndPreservesWorkspace()
    {
        using var workspace = IndexOperationWorkspace.Create("index-operation-lock-unavailable");
        var before = workspace.SnapshotHashes();
        IndexResult result;
        await using (var held = workspace.HoldLock())
        {
            result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(IndexMode.Apply),
                TestContext.Current.CancellationToken);
        }

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(IndexFindingCode.WorkspaceLockUnavailable, finding.Code);
        Assert.DoesNotContain("IOException", finding.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain("0x", finding.Cause, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(IndexRegionOutcome.NotStarted, Assert.Single(result.Regions).Outcome);
        Assert.Equal(IndexRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Index cancellation before discovery creates no workspace or recovery effects"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationIsReadOnlyAndRetainsExplicitOrigin()
    {
        using var workspace = IndexOperationWorkspace.Create("index-operation-cancelled");
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request(IndexMode.Apply),
            cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(IndexFindingCode.Interrupted, Assert.Single(result.Findings).Code);
        Assert.Equal(IndexSelectionOrigin.ExplicitSources, result.Selection.Origin);
        Assert.Empty(result.Regions);
        Assert.Equal(IndexRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    private static void AssertRegion(
        IndexRegion region,
        string path,
        IndexRegionAction action,
        IndexRegionOutcome outcome)
    {
        Assert.Equal(path, region.Source.Path);
        Assert.Equal(action, region.Action);
        Assert.Equal(outcome, region.Outcome);
    }
}
