using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.Deletion;

public sealed class RecoveryBundleDeletionSessionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A qualified catalogue and matching held lease open one session for consecutive verified deletions"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task QualifiedCatalogueAndHeldLeaseOpenSessionAndDeleteConsecutively()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var final = await workspace.AddFinalAsync();
        var draft = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(2);
        await using var lease = await workspace.AcquireAsync();

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        foreach (var candidate in frozen.Candidates)
        {
            var deletion = await session.DeleteAsync(candidate, TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleDeletionState.Deleted, deletion.State);
            Assert.Equal(RecoveryBundleDisposition.Removed, deletion.Disposition);
            Assert.Null(deletion.ResidualPath);
            Assert.False(File.Exists(candidate.Path));
            Assert.True(lease.IsHeldFor(workspace.Workspace));
        }

        Assert.False(File.Exists(final));
        Assert.False(File.Exists(draft));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "An ordinary malformed catalogue member is retained while direct session deletion stays strict"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task OrdinaryMalformedMemberOpensSessionButCannotBeDeleted()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var malformed = await workspace.AddFinalAsync();
        var draft = workspace.AddDraft();
        var malformedBytes = "not a recovery zip"u8.ToArray();
        File.WriteAllBytes(malformed, malformedBytes);
        var frozen = await RecoveryBundleCatalogue.ReadAsync(
            workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleCatalogueState.Available, frozen.State);
        Assert.Equal(2, frozen.Candidates.Length);
        var malformedCandidate = Assert.Single(frozen.Candidates, candidate => candidate.Path == malformed);
        Assert.Equal(RecoveryBundleIntegrity.Malformed, malformedCandidate.Integrity);

        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(
            lease,
            frozen,
            TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var rejected = await session.DeleteAsync(
            malformedCandidate,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Blocked, rejected.State);
        Assert.True(File.Exists(malformed));
        Assert.Equal(malformedBytes, File.ReadAllBytes(malformed));

        var deleted = await session.DeleteAsync(
            Assert.Single(frozen.Candidates, candidate => candidate.Path == draft),
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, deleted.State);
        Assert.False(File.Exists(draft));
        Assert.True(File.Exists(malformed));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A new exact candidate between planning and session opening blocks every deletion"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task NewCandidateBeforeOpenBlocksAllDeletion()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var first = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        var added = workspace.AddDraft();

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Changed, opened.State);
        Assert.Null(opened.Session);
        Assert.Equal(2, Assert.IsType<RecoveryBundleCatalogueResult>(opened.ObservedCatalogue).Candidates.Length);
        Assert.True(File.Exists(first));
        Assert.True(File.Exists(added));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A removed exact candidate between planning and session opening blocks the remaining candidate"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task RemovedCandidateBeforeOpenBlocksRemainingDeletion()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var removed = workspace.AddDraft();
        var remaining = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(2);
        await using var lease = await workspace.AcquireAsync();
        File.Delete(removed);

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Changed, opened.State);
        Assert.Null(opened.Session);
        Assert.Single(Assert.IsType<RecoveryBundleCatalogueResult>(opened.ObservedCatalogue).Candidates);
        Assert.True(File.Exists(remaining));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Changed valid final attribution between planning and session opening blocks deletion"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task ValidSemanticChangeBeforeOpenBlocksDeletion()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var final = await workspace.AddFinalAsync();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        await workspace.ChangeAttributionAsync(final);

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Changed, opened.State);
        Assert.Null(opened.Session);
        Assert.True(File.Exists(final));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Unknown names and draft byte changes stay outside session catalogue equality"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task UnknownNamesAndDraftBytesDoNotChangeRelevantFacts()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var draft = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        var unknown = workspace.AddUnknown();
        File.WriteAllBytes(draft, [9, 8, 7, 6]);
        var unknownBefore = File.ReadAllBytes(unknown);

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        Assert.Single(Assert.IsType<RecoveryBundleCatalogueResult>(opened.ObservedCatalogue).Candidates);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var deletion = await session.DeleteAsync(Assert.Single(frozen.Candidates), TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, deletion.State);
        Assert.False(File.Exists(draft));
        Assert.Equal(unknownBefore, File.ReadAllBytes(unknown));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Within one held session a later final semantic change preserves prior verified deletion and blocks the changed final"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task FinalSemanticDriftAfterVerifiedDeletionIsBlocked()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var first = workspace.AddDraft();
        var final = await workspace.AddFinalAsync();
        var frozen = await workspace.FreezeAsync(2);
        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var deleted = await session.DeleteAsync(Assert.Single(frozen.Candidates, value => value.Path == first), TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, deleted.State);
        await workspace.ChangeAttributionAsync(final);

        var blocked = await session.DeleteAsync(Assert.Single(frozen.Candidates, value => value.Path == final), TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionState.Blocked, blocked.State);
        Assert.NotEqual(RecoveryBundleDisposition.Removed, blocked.Disposition);
        Assert.Equal(final, blocked.ResidualPath);
        Assert.False(File.Exists(first));
        Assert.True(File.Exists(final));
        Assert.True(lease.IsHeldFor(workspace.Workspace));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Within one held session a draft becoming a directory blocks deletion and retains the exact path"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task DraftKindDriftInsideSessionIsBlocked()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var draft = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        File.Delete(draft);
        Directory.CreateDirectory(draft);

        var blocked = await session.DeleteAsync(Assert.Single(frozen.Candidates), TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionState.Blocked, blocked.State);
        Assert.NotEqual(RecoveryBundleDisposition.Removed, blocked.Disposition);
        Assert.Equal(draft, blocked.ResidualPath);
        Assert.True(Directory.Exists(draft));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Cancellation after one verified session deletion preserves the remaining artifact and residual facts"),
     Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task CancellationAfterVerifiedDeletionPreservesRemainingCandidate()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        workspace.AddDraft();
        workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(2);
        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var deleted = await session.DeleteAsync(frozen.Candidates[0], TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, deleted.State);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var interrupted = await session.DeleteAsync(frozen.Candidates[1], cancellation.Token);

        Assert.Equal(RecoveryBundleDeletionState.Cancelled, interrupted.State);
        Assert.NotEqual(RecoveryBundleDisposition.Removed, interrupted.Disposition);
        Assert.Equal(frozen.Candidates[1].Path, interrupted.ResidualPath);
        Assert.False(File.Exists(frozen.Candidates[0].Path));
        Assert.True(File.Exists(frozen.Candidates[1].Path));
        Assert.True(lease.IsHeldFor(workspace.Workspace));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Disposing the held lease prevents a session from deleting its candidate"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task DisposedLeaseBlocksSessionDeletion()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var draft = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        await lease.DisposeAsync();
        Assert.False(lease.IsHeld);

        var blocked = await session.DeleteAsync(Assert.Single(frozen.Candidates), TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionState.Blocked, blocked.State);
        Assert.True(File.Exists(draft));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A session refuses an exact candidate created outside its frozen membership"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task CandidateOutsideFrozenSessionMembershipIsBlocked()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var added = workspace.AddDraft();
        var current = await workspace.FreezeAsync(2);

        var blocked = await session.DeleteAsync(Assert.Single(current.Candidates, value => value.Path == added), TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionState.Blocked, blocked.State);
        Assert.True(File.Exists(added));
        Assert.True(File.Exists(Assert.Single(frozen.Candidates).Path));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Cancellation before session opening creates no session or deletion"), Trait("Feature", "recovery-deletion-session"), Trait("Evidence", "Integration")]
    public async Task CancelledOpeningPreservesFrozenCandidates()
    {
        using var workspace = new RecoveryDeletionSessionWorkspace();
        var draft = workspace.AddDraft();
        var frozen = await workspace.FreezeAsync(1);
        await using var lease = await workspace.AcquireAsync();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, cancellation.Token);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Cancelled, opened.State);
        Assert.Null(opened.Session);
        Assert.True(File.Exists(draft));
    }

}
