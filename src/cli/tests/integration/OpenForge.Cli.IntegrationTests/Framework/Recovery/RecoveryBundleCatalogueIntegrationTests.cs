using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleCatalogueIntegrationTests
{
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
            var prepared = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
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

            var result = await new RecoveryBundleCatalogue(new RecoveryBundleReader()).ReadAsync(
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
            var result = await new RecoveryBundleCatalogue(new RecoveryBundleReader()).ReadAsync(
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

    [Fact(DisplayName = "Recovery deletion requires a held lease and verifies immediate absence")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task DeletionGuardDeletesExactFinalAndDraftUnderHeldLease()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-deletion-success");
        temporary.CreateFile(WorkspaceLockRequest.RelativePath, "retained-lock");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        var prepared = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
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
            var reader = new RecoveryBundleReader();
            var catalogue = new RecoveryBundleCatalogue(reader);
            var initial = await catalogue.ReadAsync(workspace, TestContext.Current.CancellationToken);
            var finalCandidate = Candidate(initial, preparation.BundlePath);
            var draftCandidate = Candidate(initial, draftPath);
            var lockResult = await new WorkspaceLockManager(new PhysicalPathResolver()).AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
            var guard = new RecoveryBundleDeletionGuard(catalogue, reader);

            var finalDeletion = await guard.DeleteAsync(
                lease,
                finalCandidate,
                TestContext.Current.CancellationToken);
            var draftDeletion = await guard.DeleteAsync(
                lease,
                draftCandidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Deleted, finalDeletion.State);
            Assert.Equal(RecoveryBundleDeletionState.Deleted, draftDeletion.State);
            Assert.False(File.Exists(preparation.BundlePath));
            Assert.False(File.Exists(draftPath));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.BundlePath);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(draftPath);
        }
    }

    [Fact(DisplayName = "Recovery deletion blocks contention disposed lease and changed snapshot")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task DeletionGuardBlocksWithoutStableHeldLeaseSnapshot()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-deletion-blocked");
        temporary.CreateFile(WorkspaceLockRequest.RelativePath, "retained-lock");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        var prepared = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var reader = new RecoveryBundleReader();
            var catalogue = new RecoveryBundleCatalogue(reader);
            var initial = await catalogue.ReadAsync(workspace, TestContext.Current.CancellationToken);
            var candidate = Candidate(initial, preparation.BundlePath);
            var manager = new WorkspaceLockManager(new PhysicalPathResolver());
            var first = await manager.AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            var lease = Assert.IsType<WorkspaceLockLease>(first.Lease);
            var contended = await manager.AcquireAsync(
                new WorkspaceLockRequest(workspace, "cleanup", Guid.NewGuid()),
                TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockState.Failed, contended.State);
            Assert.True(File.Exists(preparation.BundlePath));

            await lease.DisposeAsync();
            var disposed = await new RecoveryBundleDeletionGuard(catalogue, reader).DeleteAsync(
                lease,
                candidate,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleDeletionState.Blocked, disposed.State);

            using (var archive = ZipFile.Open(preparation.BundlePath, ZipArchiveMode.Update))
            {
                await using var payload = archive.Entries[1].Open();
                await payload.WriteAsync(
                    new byte[] { 42 },
                    TestContext.Current.CancellationToken);
            }

            var reacquired = await manager.AcquireAsync(
                new WorkspaceLockRequest(workspace, "cleanup", Guid.NewGuid()),
                TestContext.Current.CancellationToken);
            await using var held = Assert.IsType<WorkspaceLockLease>(reacquired.Lease);
            var changed = await new RecoveryBundleDeletionGuard(catalogue, reader).DeleteAsync(
                held,
                candidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Blocked, changed.State);
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
            WorkspacePath = RecoveryBundlePathIdentity.NormalizeWorkspacePath(workspace.PhysicalRoot),
            WorkspaceKey = RecoveryBundlePathIdentity.WorkspaceKey(workspace.PhysicalRoot),
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
