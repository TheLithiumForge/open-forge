using System.Collections.Concurrent;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class WorkspaceRuntimeIntegrationTests
{
    private const string RuntimeTarget = "runtime";
    private const string IgnoreTarget = "ignore";
    private const string SentinelTarget = "sentinel";
    private const string PartialContent = "partial";

    [Fact(DisplayName = "Runtime initialization creates the ignore file before the sentinel")]
    public async Task IgnoreCreationPrecedesSentinel()
    {
        using var temporary = TemporaryWorkspace.Create("runtime-creation-order");
        using var store = WorkspaceLockTestStore.Create("runtime-creation-order-store");
        var workspace = Workspace(temporary);
        var runtime = Directory.CreateDirectory(WorkspaceRuntimePaths.DirectoryPath(workspace));
        var created = new ConcurrentQueue<string>();
        var sentinelSeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var watcher = new FileSystemWatcher(runtime.FullName)
        {
            NotifyFilter = NotifyFilters.FileName,
        };
        watcher.Created += (_, change) =>
        {
            created.Enqueue(change.FullPath);
            if (change.FullPath == WorkspaceRuntimePaths.LockPath(workspace))
            {
                sentinelSeen.TrySetResult();
            }
        };
        watcher.Error += (_, error) => sentinelSeen.TrySetException(error.GetException());
        watcher.EnableRaisingEvents = true;

        var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
        await sentinelSeen.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        watcher.EnableRaisingEvents = false;
        Assert.Equal(WorkspaceRuntimePaths.IgnorePath(workspace), created.First());
    }

    [Fact(DisplayName = "Local ownership contends across legacy homes and physical aliases but permits another worktree")]
    public async Task LocalOwnershipUsesPhysicalWorkspace()
    {
        using var first = TemporaryWorkspace.Create("local-first");
        using var second = TemporaryWorkspace.Create("local-second");
        using var firstStore = WorkspaceLockTestStore.Create("legacy-first");
        using var secondStore = WorkspaceLockTestStore.Create("legacy-second");
        var workspace = Workspace(first);
        var otherWorkspace = Workspace(second);
        var acquired = await firstStore.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var alias = new CliWorkspace(Path.Combine(first.Path, "lexical-alias"), first.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var busy = await secondStore.AcquireAsync(Request(alias), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.Busy, busy.LockFailure?.Kind);
        Assert.Equal(WorkspaceRuntimePaths.LockPath(workspace), busy.LockFailure?.Path);
        var other = await firstStore.AcquireAsync(Request(otherWorkspace), TestContext.Current.CancellationToken);
        await using var otherLease = Assert.IsType<WorkspaceLockLease>(other.Lease);
        Assert.True(otherLease.IsHeldFor(otherWorkspace));
    }

    [Fact(DisplayName = "Nonempty local sentinel contents survive acquisition and release byte for byte")]
    public async Task LocalSentinelPreservesBytes()
    {
        using var temporary = TemporaryWorkspace.Create("local-content");
        using var store = WorkspaceLockTestStore.Create("local-content-store");
        var workspace = Workspace(temporary);
        byte[] bytes = [0, 255, 10, 13, 42, 128];
        var path = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.LockPath(workspace)), bytes);
        var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        await Assert.IsType<WorkspaceLockLease>(result.Lease).DisposeAsync();
        Assert.Equal(bytes, File.ReadAllBytes(path));
        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, File.ReadAllText(WorkspaceRuntimePaths.IgnorePath(workspace)));
    }

    [Theory(DisplayName = "Bootstrap creates only the ignore file and repairs interrupted content")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(PartialContent)]
    public void BootstrapWritesIgnoreBeforeOtherFiles(string? prior)
    {
        using var temporary = TemporaryWorkspace.Create("runtime-bootstrap");
        using var store = WorkspaceLockTestStore.Create("runtime-bootstrap-store");
        var workspace = Workspace(temporary);
        _ = store.Track(workspace);
        if (prior is not null)
        {
            temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace)), prior);
        }

        Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        Assert.Equal([WorkspaceRuntimePaths.IgnorePath(workspace)], Directory.GetFileSystemEntries(WorkspaceRuntimePaths.DirectoryPath(workspace)));
        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, File.ReadAllText(WorkspaceRuntimePaths.IgnorePath(workspace)));
    }

    [Theory(DisplayName = "Unsafe runtime directory ignore and sentinel targets are refused without changing them")]
    [InlineData(RuntimeTarget)]
    [InlineData(IgnoreTarget)]
    [InlineData(SentinelTarget)]
    public async Task UnsafeRuntimeTargetsAreRefused(string target)
    {
        using var temporary = TemporaryWorkspace.Create("runtime-unsafe");
        using var store = WorkspaceLockTestStore.Create("runtime-unsafe-store");
        var workspace = Workspace(temporary);
        var path = target switch
        {
            RuntimeTarget => temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.DirectoryPath(workspace)), "preserved"),
            IgnoreTarget => temporary.CreateDirectory(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace))),
            SentinelTarget => temporary.CreateDirectory(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.LockPath(workspace))),
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };
        var result = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.InvalidTarget, result.LockFailure?.Kind);
        Assert.Equal(path, result.LockFailure?.Path);
        Assert.True(File.Exists(path) || Directory.Exists(path));
    }

    [Theory(DisplayName = "Concurrent initialization grants one lease and keeps ignore conflicts distinct from lock contention")]
    [InlineData(false), InlineData(true)]
    public async Task ConcurrentFirstInitializationIsBounded(bool alreadyInitialized)
    {
        using var temporary = TemporaryWorkspace.Create("runtime-concurrent");
        using var store = WorkspaceLockTestStore.Create("runtime-concurrent-store");
        var workspace = Workspace(temporary);
        _ = store.Track(workspace);
        if (alreadyInitialized)
        {
            Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        }

        using var start = new ManualResetEventSlim();
        var attempts = Enumerable.Range(0, 12).Select(_ => Task.Run(async () =>
        {
            start.Wait(TestContext.Current.CancellationToken);
            return await store.Manager.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        }, TestContext.Current.CancellationToken)).ToArray();
        start.Set();
        var results = await Task.WhenAll(attempts);
        try
        {
            Assert.Single(results, result => result.State == WorkspaceLockState.Acquired);
            Assert.All(results.Where(result => result.Lease is null), result =>
            {
                var failure = Assert.IsType<WorkspaceLockFailure>(result.LockFailure);
                if (!alreadyInitialized && failure.Path == WorkspaceRuntimePaths.IgnorePath(workspace))
                {
                    Assert.Equal(WorkspaceLockFailureKind.IoError, failure.Kind);
                }
                else
                {
                    Assert.Equal(WorkspaceLockFailureKind.Busy, failure.Kind);
                    Assert.Equal(WorkspaceRuntimePaths.LockPath(workspace), failure.Path);
                }
            });
            Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, File.ReadAllText(WorkspaceRuntimePaths.IgnorePath(workspace)));
        }
        finally
        {
            foreach (var result in results)
            {
                result.Lease?.Dispose();
            }
        }
    }

    [Fact(DisplayName = "Correct runtime ignore content is shared for reading and never rewritten")]
    public void CorrectIgnoreFileIsNotRewritten()
    {
        using var temporary = TemporaryWorkspace.Create("runtime-ignore-preserved");
        var workspace = Workspace(temporary);
        var path = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace)), WorkspaceRuntimeBootstrap.IgnoreContent);
        File.SetLastWriteTimeUtc(path, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var before = File.GetLastWriteTimeUtc(path);
        using var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        Assert.Equal(before, File.GetLastWriteTimeUtc(path));
        Assert.Equal([path], Directory.GetFileSystemEntries(WorkspaceRuntimePaths.DirectoryPath(workspace)));
    }

    [Theory(DisplayName = "A shared reader blocks ignore repair with an initialization error and no sentinel")]
    [InlineData(PartialContent), InlineData("xx")]
    public void IgnoreRepairSharingConflictIsNotWorkspaceContention(string prior)
    {
        using var temporary = TemporaryWorkspace.Create("runtime-ignore-repair");
        var workspace = Workspace(temporary);
        var path = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace)), prior);
        using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var failure = WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockFailureKind.IoError, failure?.Kind);
            Assert.Equal(path, failure?.Path);
            Assert.Equal(prior, File.ReadAllText(path));
            Assert.False(File.Exists(WorkspaceRuntimePaths.LockPath(workspace)));
        }

        Assert.Null(WorkspaceRuntimeBootstrap.EnsureCreated(workspace, TestContext.Current.CancellationToken));
        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, File.ReadAllText(path));
    }

    [Fact(DisplayName = "A writer cannot acquire the workspace while ignore repair holds partial bytes")]
    public async Task InProgressIgnoreRepairCannotGrantWorkspaceOwnership()
    {
        using var temporary = TemporaryWorkspace.Create("runtime-ignore-in-progress");
        using var store = WorkspaceLockTestStore.Create("runtime-ignore-in-progress-store");
        var workspace = Workspace(temporary);
        var path = temporary.CreateFile(Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace)), WorkspaceRuntimeBootstrap.IgnoreContent[..1]);
        using (var repair = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var blocked = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
            Assert.Null(blocked.Lease);
            Assert.Equal(WorkspaceLockFailureKind.IoError, blocked.LockFailure?.Kind);
            Assert.Equal(path, blocked.LockFailure?.Path);
            Assert.False(File.Exists(WorkspaceRuntimePaths.LockPath(workspace)));
            repair.Position = 1;
            repair.Write(System.Text.Encoding.UTF8.GetBytes(WorkspaceRuntimeBootstrap.IgnoreContent[1..]));
            repair.Flush(flushToDisk: true);
        }

        var acquired = await store.AcquireAsync(Request(workspace), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, File.ReadAllText(path));
    }

    [Fact(DisplayName = "Recovery location discovery creates neither local nor legacy storage")]
    public void LocationsAreReadOnly()
    {
        using var temporary = TemporaryWorkspace.Create("runtime-locations");
        var workspace = Workspace(temporary);
        var local = RecoveryStoreLocator.Local(workspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.False(Directory.Exists(local.RootPath));
        Assert.NotNull(legacy);
        Assert.NotNull(legacy.LegacyLockStoreRoot);
        Assert.False(Directory.Exists(legacy.WorkspaceDirectory));
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace)));
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static WorkspaceLockRequest Request(CliWorkspace workspace)
        => new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid());
}
