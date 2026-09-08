using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.Deletion;

public sealed class RecoveryDeletionStorageBoundaryIntegrationTests
{
    [Fact(DisplayName = "Cleanup preserves an ordinary external draft exposed by a qualified static bucket link"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public async Task StaticBucketLinkCannotAuthorizeExternalDraftDeletion()
    {
        using var workspace = new RecoveryDeletionLinkedStorageWorkspace();
        var draft = workspace.AddDraft();
        workspace.LinkBucketToExternal();
        await workspace.QualifyLinkedDraftAsync(draft);

        var result = await new CleanupOperation(workspace.Locks.StoreRoot).ExecuteAsync(
            new CleanupRequest(workspace.Source.Workspace, CleanupMode.Apply), TestContext.Current.CancellationToken);

        Assert.True(File.Exists(workspace.ExternalPath(draft)));
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(workspace.ExternalPath(draft)));
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(CleanupCatalogueCoverage.Incomplete, result.Catalogue.Coverage);
        Assert.Empty(result.Catalogue.Candidates);
        Assert.Empty(result.Effects);
        Assert.Equal(CleanupLeaseState.NotRequested, result.Lease.State);
        Assert.False(workspace.Locks.InfrastructureExists);
        Assert.Equal(workspace.ExternalDirectory, new DirectoryInfo(workspace.Bucket).LinkTarget);
    }

    [Fact(DisplayName = "Cleanup dry-run excludes external candidates exposed by a static bucket link before enumeration"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public async Task StaticBucketLinkCannotBecomeDryRunCatalogue()
    {
        using var workspace = new RecoveryDeletionLinkedStorageWorkspace();
        var draft = workspace.AddDraft();
        workspace.LinkBucketToExternal();
        await workspace.QualifyLinkedDraftAsync(draft);

        var result = await new CleanupOperation(workspace.Locks.StoreRoot).ExecuteAsync(
            new CleanupRequest(workspace.Source.Workspace, CleanupMode.DryRun), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(CleanupCatalogueCoverage.Incomplete, result.Catalogue.Coverage);
        Assert.Empty(result.Catalogue.Candidates);
        Assert.Empty(result.Effects);
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(workspace.ExternalPath(draft)));
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Fact(DisplayName = "A held deletion session rejects a static bucket link before observing its external catalogue"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public async Task StaticBucketLinkCannotOpenDeletionSession()
    {
        using var workspace = new RecoveryDeletionLinkedStorageWorkspace();
        var draft = workspace.AddDraft();
        workspace.LinkBucketToExternal();
        await workspace.QualifyLinkedDraftAsync(draft);
        var frozen = await workspace.Source.FreezeAsync(1);
        await using var lease = await workspace.Source.AcquireAsync();
        Assert.True(lease.IsHeldFor(workspace.Source.Workspace));

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Blocked, opened.State);
        Assert.Null(opened.Session);
        Assert.Null(opened.ObservedCatalogue);
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(workspace.ExternalPath(draft)));
        Assert.True(lease.IsHeldFor(workspace.Source.Workspace));
    }

    [Fact(DisplayName = "A bucket becoming a link after ordinary planning blocks session opening without external enumeration"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public async Task BucketLinkAfterPlanningBlocksSessionOpening()
    {
        using var workspace = new RecoveryDeletionLinkedStorageWorkspace();
        var draft = workspace.AddDraft();
        var frozen = await workspace.Source.FreezeAsync(1);
        await using var lease = await workspace.Source.AcquireAsync();
        workspace.LinkBucketToExternal();
        await workspace.QualifyLinkedDraftAsync(draft);

        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Blocked, opened.State);
        Assert.Null(opened.Session);
        Assert.Null(opened.ObservedCatalogue);
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(workspace.ExternalPath(draft)));
        Assert.True(lease.IsHeldFor(workspace.Source.Workspace));
    }

    [Fact(DisplayName = "A bucket link introduced within a held session preserves prior deletion and the external remaining draft"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public async Task BucketLinkWithinSessionStopsExternalDeletion()
    {
        using var workspace = new RecoveryDeletionLinkedStorageWorkspace();
        _ = workspace.AddDraft();
        _ = workspace.AddDraft();
        var frozen = await workspace.Source.FreezeAsync(2);
        await using var lease = await workspace.Source.AcquireAsync();
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozen, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionSessionOpenState.Opened, opened.State);
        var session = Assert.IsType<RecoveryBundleDeletionSession>(opened.Session);
        var first = await session.DeleteAsync(frozen.Candidates[0], TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleDeletionState.Deleted, first.State);
        Assert.False(File.Exists(frozen.Candidates[0].Path));
        workspace.LinkBucketToExternal();
        var remaining = frozen.Candidates[1];
        await workspace.QualifyLinkedDraftAsync(remaining.Path);
        Assert.True(lease.IsHeldFor(workspace.Source.Workspace));

        var deletion = await session.DeleteAsync(remaining, TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundleDeletionState.Blocked, deletion.State);
        Assert.Equal(RecoveryBundleDisposition.Unknown, deletion.Disposition);
        Assert.Equal(remaining.Path, deletion.ResidualPath);
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(workspace.ExternalPath(remaining.Path)));
        Assert.False(File.Exists(frozen.Candidates[0].Path));
        Assert.True(lease.IsHeldFor(workspace.Source.Workspace));
    }

    [Theory(DisplayName = "The deletion storage boundary rejects isolated ancestor links that expose ordinary exact-name drafts"),
     InlineData("store/recovery", "v1/bucket"), InlineData("store/recovery/v1", "bucket"),
     Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration")]
    public static void StaticAncestorLinksCannotEstablishStorageBoundary(string linkRelative, string remainingRelative)
    {
        using var storage = TemporaryWorkspace.Create("cleanup-linked-ancestor");
        using var external = TemporaryWorkspace.Create("cleanup-ancestor-target");
        var target = external.CreateDirectory("target");
        var name = RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid());
        var externalFile = external.CreateFile(Path.Combine("target", remainingRelative, name), [7, 8, 9]);
        var link = storage.CreateDirectorySymbolicLink(linkRelative, target);
        var bucket = storage.Combine(linkRelative, remainingRelative);
        Assert.Equal(target, new DirectoryInfo(link).LinkTarget);
        Assert.NotEqual((FileAttributes)0, File.GetAttributes(link) & FileAttributes.ReparsePoint);
        Assert.Equal((FileAttributes)0, File.GetAttributes(Path.Combine(bucket, name))
            & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint));
        Assert.Equal<byte>([7, 8, 9], File.ReadAllBytes(externalFile));

        var failure = RecoveryDeletionStorageBoundary.ReadDirectoryChain(bucket);

        Assert.NotNull(failure);
        Assert.Equal(FilesystemFailureKind.InvalidPath, failure.Kind);
        Assert.Equal<byte>([7, 8, 9], File.ReadAllBytes(externalFile));
        Assert.Equal(target, new DirectoryInfo(link).LinkTarget);
    }
}
