using System.Security.AccessControl;
using System.Security.Principal;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

[Trait("Boundary", "OS"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class WorkspaceLockFailureIntegrationTests
{
    private const string RuntimeTarget = "runtime";
    private const string IgnoreTarget = "ignore";
    private const string SentinelTarget = "sentinel";
    private const string PreservedContent = "preserved";
    private const string OutsideContent = "outside-content";

    [Fact(DisplayName = "Unobservable existing legacy files fail as access denied and release the local lock")]
    public async Task UnobservableLegacyFileIsNotAssumedAbsent()
    {
        using var temporary = TemporaryWorkspace.Create("bridge-unobservable");
        using var store = WorkspaceLockTestStore.Create("bridge-unobservable-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = store.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        File.WriteAllBytes(path, []);
        var file = new FileInfo(path);
        FileSecurity? original = null;
        UnixFileMode mode = default;
        if (OperatingSystem.IsWindows())
        {
            original = file.GetAccessControl();
            var denied = file.GetAccessControl();
            using var identity = WindowsIdentity.GetCurrent();
            denied.AddAccessRule(new FileSystemAccessRule(
                identity.User ?? throw new InvalidOperationException(),
                FileSystemRights.ReadData | FileSystemRights.WriteData | FileSystemRights.ReadAttributes,
                AccessControlType.Deny));
            file.SetAccessControl(denied);
        }
        else
        {
            mode = File.GetUnixFileMode(path);
            File.SetUnixFileMode(path, UnixFileMode.None);
        }

        try
        {
            var result = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
            if (result.Lease is not null)
            {
                result.Lease.Dispose();
                Assert.Fail("This host bypasses the fixture access denial.");
            }

            Assert.Equal(WorkspaceLockFailureKind.AccessDenied, result.LockFailure?.Kind);
            Assert.Equal(path, result.LockFailure?.Path);
            using var local = new FileStream(WorkspaceRuntimePaths.LockPath(workspace), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(local.CanWrite);
        }
        finally
        {
            if (OperatingSystem.IsWindows() && original is not null)
            {
                file.SetAccessControl(original);
            }
            else if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, mode);
            }
        }
    }

    [Fact(DisplayName = "A legacy storage IO failure retains its exact path and releases local ownership")]
    public async Task LegacyStorageFailureReleasesLocal()
    {
        using var temporary = TemporaryWorkspace.Create("bridge-io");
        using var legacy = TemporaryWorkspace.Create("bridge-io-store");
        using var cleanup = WorkspaceLockTestStore.Create("bridge-io-cleanup");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = cleanup.Track(workspace);
        var root = WorkspaceLockStoreRoot.FromAbsolutePath(legacy.Path);
        var path = legacy.CreateFile(WorkspaceLockPathIdentity.ApplicationDirectoryName, PreservedContent);
        var result = await new WorkspaceLockManager(root).AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockFailureKind.IoError, result.LockFailure?.Kind);
        Assert.Equal(WorkspaceLockPathIdentity.StoreDirectory(root), result.LockFailure?.Path);
        Assert.Equal(PreservedContent, File.ReadAllText(path));
        using var local = new FileStream(WorkspaceRuntimePaths.LockPath(workspace), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(local.CanWrite);
    }

    [Theory(DisplayName = "Runtime links are refused before following or modifying their targets")]
    [InlineData(RuntimeTarget)]
    [InlineData(IgnoreTarget)]
    [InlineData(SentinelTarget)]
    public async Task RuntimeLinksAreRefused(string target)
    {
        using var temporary = TemporaryWorkspace.Create("runtime-link");
        using var outside = TemporaryWorkspace.Create("runtime-link-outside");
        using var store = WorkspaceLockTestStore.Create("runtime-link-store");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var relativePath = target switch
        {
            RuntimeTarget => Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.DirectoryPath(workspace)),
            IgnoreTarget => Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.IgnorePath(workspace)),
            SentinelTarget => Path.GetRelativePath(temporary.Path, WorkspaceRuntimePaths.LockPath(workspace)),
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };
        var outsideFile = outside.CreateFile(PreservedContent, OutsideContent);
        var created = target == RuntimeTarget
            ? temporary.TryCreateDirectorySymbolicLink(relativePath, outside.Path, out _)
            : temporary.TryCreateFileSymbolicLink(relativePath, outsideFile, out _);
        if (!created)
        {
            Assert.Fail("This host does not permit symbolic-link creation.");
        }

        var path = temporary.Combine(relativePath);
        try
        {
            var result = await store.AcquireAsync(new(workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
            Assert.Equal(WorkspaceLockFailureKind.InvalidTarget, result.LockFailure?.Kind);
            Assert.Equal(path, result.LockFailure?.Path);
            Assert.Equal(OutsideContent, File.ReadAllText(outsideFile));
            Assert.False(File.Exists(outside.Combine(Path.GetFileName(WorkspaceRuntimePaths.LockPath(workspace)))));
        }
        finally
        {
            if (target == RuntimeTarget)
            {
                Directory.Delete(path);
            }
        }
    }
}
