using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle.Models;

public sealed class LifecycleDocumentSnapshotTests
{
    [Fact(DisplayName = "Status lifecycle snapshots preserve finite state payload and canonical path invariants"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void StatusLifecycleSnapshotsPreserveFiniteStatePayloadAndCanonicalPathInvariants()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-status-snapshot"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var lifecyclePath = Path.Combine(root, ".agents", "open-forge.lifecycle.json");
        var availableFile = FileStateSnapshot.File(lifecyclePath, lifecyclePath, "first"u8);
        var missingFile = FileStateSnapshot.Missing(lifecyclePath);
        var directory = FileStateSnapshot.Directory(lifecyclePath, lifecyclePath);
        var failure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Access denied.");

        var available = LifecycleDocumentSnapshot.Available(workspace, availableFile);
        var missing = LifecycleDocumentSnapshot.Missing(workspace, missingFile);
        var blocked = LifecycleDocumentSnapshot.Blocked(workspace, directory, "Lifecycle path is a directory.");
        var unavailable = LifecycleDocumentSnapshot.Unavailable(
            workspace,
            failure,
            LifecycleDocumentFailureStage.ContainedFileAccess);
        var interrupted = LifecycleDocumentSnapshot.Interrupted(workspace);

        var expected = new (
            LifecycleDocumentSnapshot Snapshot,
            LifecycleDocumentSnapshotState State,
            FileStateSnapshot? File,
            FilesystemFailure? Failure,
            LifecycleDocumentFailureStage? FailureStage,
            string? Cause)[]
        {
            (available, LifecycleDocumentSnapshotState.Available, availableFile, null, null, null),
            (missing, LifecycleDocumentSnapshotState.Missing, missingFile, null, null, null),
            (blocked, LifecycleDocumentSnapshotState.Blocked, directory, null, null, "Lifecycle path is a directory."),
            (unavailable, LifecycleDocumentSnapshotState.Unavailable, null, failure, LifecycleDocumentFailureStage.ContainedFileAccess, null),
            (interrupted, LifecycleDocumentSnapshotState.Interrupted, null, null, null, null),
        };
        foreach (var item in expected)
        {
            Assert.Equal(item.State, item.Snapshot.State);
            Assert.Same(workspace, item.Snapshot.Workspace);
            Assert.Same(item.File, item.Snapshot.File);
            Assert.Same(item.Failure, item.Snapshot.Failure);
            Assert.Equal(item.FailureStage, item.Snapshot.FailureStage);
            Assert.Equal(item.Cause, item.Snapshot.Cause);
        }

        var stagedUnavailable = new[]
        {
            LifecycleDocumentSnapshot.Unavailable(
                workspace,
                failure,
                LifecycleDocumentFailureStage.PhysicalResolution),
            LifecycleDocumentSnapshot.Unavailable(
                workspace,
                failure,
                LifecycleDocumentFailureStage.PhysicalReconfirmation),
            unavailable,
        };
        Assert.Equal(
            [
                LifecycleDocumentFailureStage.PhysicalResolution,
                LifecycleDocumentFailureStage.PhysicalReconfirmation,
                LifecycleDocumentFailureStage.ContainedFileAccess,
            ],
            stagedUnavailable.Select(snapshot => snapshot.FailureStage));
        Assert.All(stagedUnavailable, snapshot => Assert.Same(failure, snapshot.Failure));

        var otherPath = Path.Combine(root, ".agents", "other.json");
        var externalPath = Path.GetFullPath(Path.Combine(root, "..", "outside-lifecycle.json"));
        Assert.Throws<ArgumentException>(() =>
            LifecycleDocumentSnapshot.Available(
                workspace,
                FileStateSnapshot.File(otherPath, otherPath, "other"u8)));
        Assert.Throws<ArgumentException>(() =>
            LifecycleDocumentSnapshot.Missing(workspace, FileStateSnapshot.Missing(otherPath)));
        Assert.Throws<ArgumentException>(() =>
            LifecycleDocumentSnapshot.Available(
                workspace,
                FileStateSnapshot.File(lifecyclePath, externalPath, "external"u8)));
        Assert.Throws<ArgumentException>(() => LifecycleDocumentSnapshot.Available(workspace, missingFile));
        Assert.Throws<ArgumentException>(() => LifecycleDocumentSnapshot.Blocked(workspace, availableFile, "blocked"));
        Assert.Throws<ArgumentException>(() => LifecycleDocumentSnapshot.Unavailable(
            workspace,
            new FilesystemFailure(FilesystemFailureKind.InvalidEncoding, "Invalid encoding."),
            LifecycleDocumentFailureStage.PhysicalResolution));
        Assert.Throws<ArgumentException>(() => LifecycleDocumentSnapshot.Unavailable(
            workspace,
            new FilesystemFailure(FilesystemFailureKind.InvalidSyntax, "Invalid syntax."),
            LifecycleDocumentFailureStage.PhysicalReconfirmation));
        Assert.Throws<ArgumentException>(() => LifecycleDocumentSnapshot.Unavailable(
            workspace,
            new FilesystemFailure(FilesystemFailureKind.InvalidPath, "Invalid path."),
            LifecycleDocumentFailureStage.ContainedFileAccess));
        Assert.Throws<ArgumentOutOfRangeException>(() => LifecycleDocumentSnapshot.Unavailable(
            workspace,
            failure,
            (LifecycleDocumentFailureStage)int.MaxValue));
    }
}
