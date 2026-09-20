using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleCatalogueIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery catalogue reports every exact final and draft candidate without target observation")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CatalogueClassifiesExactCandidatesAndIgnoresUnknownNames()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-catalogue");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        RecoveryBundlePreparation? preparation = null;
        var created = new List<string>();
        try
        {
            var prepared = await RecoveryBundleStore.PrepareAsync(
                input,
                TestContext.Current.CancellationToken);
            preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace);
            var draftPath = CandidatePath(directory, Guid.NewGuid(), RecoveryBundleCandidateKind.Draft);
            var malformedPath = CandidatePath(directory, Guid.NewGuid(), RecoveryBundleCandidateKind.Final);
            var unsupportedOperation = Guid.NewGuid();
            var unsupportedPath = CandidatePath(
                directory,
                unsupportedOperation,
                RecoveryBundleCandidateKind.Final);
            var unavailablePath = CandidatePath(directory, Guid.NewGuid(), RecoveryBundleCandidateKind.Final);
            var unknownPath = Path.Combine(directory, "keep-me.zip");
            await File.WriteAllBytesAsync(
                draftPath,
                "untrusted draft"u8.ToArray(),
                TestContext.Current.CancellationToken);
            await File.WriteAllBytesAsync(
                malformedPath,
                "not a zip"u8.ToArray(),
                TestContext.Current.CancellationToken);
            await WriteUnsupportedAsync(
                workspace,
                unsupportedPath,
                unsupportedOperation,
                TestContext.Current.CancellationToken);
            Directory.CreateDirectory(unavailablePath);
            await File.WriteAllTextAsync(
                unknownPath,
                "unknown",
                TestContext.Current.CancellationToken);
            created.AddRange([draftPath, malformedPath, unsupportedPath, unavailablePath, unknownPath]);

            var result = await RecoveryBundleCatalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleCatalogueState.Available, result.State);
            Assert.Equal(5, result.Candidates.Length);
            Assert.Equal(
                RecoveryBundleIntegrity.Verified,
                Candidate(result, preparation.BundlePath).Integrity);
            Assert.Equal(RecoveryBundleIntegrity.Incomplete, Candidate(result, draftPath).Integrity);
            Assert.Equal(RecoveryBundleIntegrity.Malformed, Candidate(result, malformedPath).Integrity);
            Assert.Equal(RecoveryBundleIntegrity.Unsupported, Candidate(result, unsupportedPath).Integrity);
            Assert.Equal(RecoveryBundleIntegrity.Unavailable, Candidate(result, unavailablePath).Integrity);
            Assert.DoesNotContain(result.Candidates, candidate => candidate.Path == unknownPath);
            Assert.True(File.Exists(unknownPath));
        }
        finally
        {
            foreach (var path in created)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }
            }

            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation?.BundlePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery catalogue reports an ordinary-file workspace bucket as unavailable")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CatalogueDoesNotTreatExistingFileBucketAsAbsent()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-catalogue-file-bucket");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var workspaceDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace);
        _ = Directory.CreateDirectory(
            Path.GetDirectoryName(workspaceDirectory)
                ?? throw new InvalidOperationException("A recovery workspace bucket requires a parent."));
        await File.WriteAllTextAsync(
            workspaceDirectory,
            "not a directory",
            TestContext.Current.CancellationToken);
        try
        {
            var result = await RecoveryBundleCatalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleCatalogueState.Unavailable, result.State);
            Assert.Equal(FilesystemFailureKind.InvalidPath, result.Failure?.Kind);
            Assert.Empty(result.Candidates);
        }
        finally
        {
            File.Delete(workspaceDirectory);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery deletion requires a held lease and verifies immediate absence")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task DeletionGuardDeletesExactFinalAndDraftUnderHeldLease()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-deletion-success");
        using var lockStore = WorkspaceLockTestStore.Create("recovery-deletion-success-lock-store");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        var prepared = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var draftPath = CandidatePath(
            RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace),
            Guid.NewGuid(),
            RecoveryBundleCandidateKind.Draft);
        await File.WriteAllTextAsync(
            draftPath,
            "draft",
            TestContext.Current.CancellationToken);
        try
        {
            var initial = await RecoveryBundleCatalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);
            var finalCandidate = Candidate(initial, preparation.BundlePath);
            var draftCandidate = Candidate(initial, draftPath);
            var lockResult = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
            var finalDeletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                lease,
                finalCandidate,
                TestContext.Current.CancellationToken);
            var draftDeletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                lease,
                draftCandidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Deleted, finalDeletion.State);
            Assert.Equal(RecoveryBundleDeletionState.Deleted, draftDeletion.State);
            Assert.Equal(RecoveryBundleDisposition.Removed, finalDeletion.Disposition);
            Assert.Equal(RecoveryBundleDisposition.Removed, draftDeletion.Disposition);
            Assert.Null(finalDeletion.ResidualPath);
            Assert.Null(draftDeletion.ResidualPath);
            Assert.False(File.Exists(preparation.BundlePath));
            Assert.False(File.Exists(draftPath));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.BundlePath);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(draftPath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery deletion blocks contention disposed lease and changed snapshot")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task DeletionGuardBlocksWithoutStableHeldLeaseSnapshot()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-deletion-blocked");
        using var lockStore = WorkspaceLockTestStore.Create("recovery-deletion-blocked-lock-store");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        var prepared = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var initial = await RecoveryBundleCatalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);
            var candidate = Candidate(initial, preparation.BundlePath);
            var first = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            var lease = Assert.IsType<WorkspaceLockLease>(first.Lease);
            var contended = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, "cleanup", Guid.NewGuid()),
                TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockState.Failed, contended.State);
            Assert.True(File.Exists(preparation.BundlePath));

            await lease.DisposeAsync();
            var disposed = await RecoveryBundleDeletionGuard.DeleteAsync(
                lease,
                candidate,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleDeletionState.Blocked, disposed.State);
            Assert.Equal(RecoveryBundleDisposition.Unknown, disposed.Disposition);
            Assert.Null(disposed.ResidualPath);

            using (var archive = ZipFile.Open(preparation.BundlePath, ZipArchiveMode.Update))
            {
                await using var payload = archive.Entries[1].Open();
                await payload.WriteAsync(
                    "*"u8.ToArray(),
                    TestContext.Current.CancellationToken);
            }

            var reacquired = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, "cleanup", Guid.NewGuid()),
                TestContext.Current.CancellationToken);
            await using var held = Assert.IsType<WorkspaceLockLease>(reacquired.Lease);
            var changed = await RecoveryBundleDeletionGuard.DeleteAsync(
                held,
                candidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Blocked, changed.State);
            Assert.Equal(RecoveryBundleDisposition.Unknown, changed.Disposition);
            Assert.Equal(preparation.BundlePath, changed.ResidualPath);
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.BundlePath);
        }
    }

    private static RecoveryBundleCandidateSnapshot Candidate(
        RecoveryBundleCatalogueResult result,
        string path)
        => Assert.Single(result.Candidates, item => string.Equals(
            item.Path,
            path,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal));

    private static string CandidatePath(
        string directory,
        Guid operationId,
        RecoveryBundleCandidateKind kind)
        => Path.Combine(
            directory,
            kind == RecoveryBundleCandidateKind.Final
                ? RecoveryBundleFormatV1.FinalFileName(operationId)
                : RecoveryBundleFormatV1.DraftFileName(operationId));

    private static async ValueTask WriteUnsupportedAsync(
        CliWorkspace workspace,
        string path,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var document = new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion + 1,
            Command = "future",
            OperationId = operationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(workspace.PhysicalRoot),
            Attribution = RecoveryBundleAttributionCodec.Serialize(
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    workspace)),
            Entries = [],
        };
        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: true);
        var manifest = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
        await using var stream = manifest.Open();
        await stream.WriteAsync(
            RecoveryBundleManifestCodec.Serialize(document),
            cancellationToken);
    }
}
