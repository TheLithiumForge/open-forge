using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

public sealed class TemporaryWorkspaceSafetyTests
{
    private const string NestedDirectory = "nested";
    private const string SelectedDirectory = "selected";
    private const string SelectedWorkspace = SelectedDirectory + "/" + NestedDirectory;
    private const string AuthoredFile = "authored.txt";
    private const string AuthoredRuntimeFile = "authored/" + TemporaryWorkspace.RuntimeDirectoryName + "/notes.txt";
    private const string RecordFile = "record.json";
    private const string ExternalDirectory = "external";
    private const string PreservedContent = "preserved";

    private static CliWorkspace Workspace(string path)
        => new(path, path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static string RelativeSentinel(TemporaryWorkspace workspace)
        => Path.GetRelativePath(workspace.Path, WorkspaceRuntimePaths.LockPath(Workspace(workspace.Path))).Replace('\\', '/');

    [Fact(DisplayName = "Selected nested workspace runtime cleanup preserves authored files")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void NestedWorkspaceRuntimeCanBeRemovedBeforeParentCleanup()
    {
        using var workspace = TemporaryWorkspace.Create("nested-runtime-ownership");
        var selected = workspace.CreateDirectory(SelectedWorkspace);
        var authored = workspace.CreateFile(SelectedWorkspace + "/" + AuthoredFile, PreservedContent);
        var runtime = Path.Combine(selected, TemporaryWorkspace.RuntimeDirectoryName);
        Directory.CreateDirectory(runtime);
        File.WriteAllText(WorkspaceRuntimePaths.LockPath(Workspace(selected)), "held earlier");

        workspace.DeleteRuntimeState(SelectedWorkspace);

        Assert.False(Directory.Exists(runtime));
        Assert.Equal(PreservedContent, File.ReadAllText(authored));
    }

    [Fact(DisplayName = "Selected nested workspace runtime cleanup refuses linked workspace ancestors")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void NestedWorkspaceRuntimeCleanupDoesNotFollowAncestors()
    {
        using var workspace = TemporaryWorkspace.Create("nested-runtime-link");
        using var external = TemporaryWorkspace.Create("nested-runtime-external");
        var sentinel = external.CreateFile(NestedDirectory + "/" + RelativeSentinel(external), PreservedContent);
        Assert.True(workspace.TryCreateDirectorySymbolicLink(SelectedDirectory, external.Path, out var cause), cause);

        Assert.Throws<InvalidOperationException>(() => workspace.DeleteRuntimeState(SelectedWorkspace));

        Assert.Equal(PreservedContent, File.ReadAllText(sentinel));
    }

    [Fact(DisplayName = "Temporary workspace owns runtime contents created outside fixture APIs")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void RuntimeContentsAreRemovedOnDisposal()
    {
        var workspace = TemporaryWorkspace.Create("runtime-ownership");
        var root = workspace.Path;
        var runtime = workspace.CreateDirectory(TemporaryWorkspace.RuntimeDirectoryName);
        var recovery = TestDataHome.LocalRecoveryStoreRoot(root);
        Directory.CreateDirectory(recovery);
        File.WriteAllText(Path.Combine(recovery, RecordFile), "{}");
        File.WriteAllText(WorkspaceRuntimePaths.LockPath(Workspace(root)), "arbitrary sentinel bytes");

        workspace.Dispose();
        workspace.Dispose();

        Assert.False(Directory.Exists(root));
    }

    [Fact(DisplayName = "Default workspace snapshot excludes a held runtime sentinel only at the workspace root")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void DefaultSnapshotExcludesHeldRuntimeState()
    {
        using var workspace = TemporaryWorkspace.Create("runtime-snapshot");
        workspace.CreateFile(AuthoredRuntimeFile, "authored bytes");
        var before = workspace.SnapshotHashes();
        var sentinel = workspace.CreateFile(RelativeSentinel(workspace), "held");
        using var held = new FileStream(sentinel, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Contains(AuthoredRuntimeFile, before.Keys);
    }

    [Fact(DisplayName = "Explicit runtime snapshot observes empty runtime directories and later contents")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void ExplicitSnapshotIncludesRuntimePresenceAndBytes()
    {
        using var workspace = TemporaryWorkspace.Create("runtime-explicit-snapshot");
        Assert.Empty(workspace.SnapshotHashes(includeRuntime: true));
        workspace.CreateDirectory(TemporaryWorkspace.RuntimeDirectoryName);
        Assert.Contains(TemporaryWorkspace.RuntimeDirectoryName, workspace.SnapshotHashes(includeRuntime: true).Keys);
        workspace.CreateFile(RelativeSentinel(workspace), "first");
        var before = workspace.SnapshotHashes(includeRuntime: true);
        workspace.ReplaceText(RelativeSentinel(workspace), "second");

        Assert.NotEqual(before[RelativeSentinel(workspace)], workspace.SnapshotHashes(includeRuntime: true)[RelativeSentinel(workspace)]);
        Assert.Empty(workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Runtime cleanup removes links without following external targets")]
    [InlineData(true), InlineData(false)]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void RuntimeCleanupDoesNotFollowLinks(bool linkAtRoot)
    {
        using var external = TemporaryWorkspace.Create("runtime-external");
        external.CreateFile("preserved.txt", "external bytes");
        var before = external.SnapshotHashes(includeRuntime: true);
        using var workspace = TemporaryWorkspace.Create("runtime-links");
        var link = linkAtRoot ? TemporaryWorkspace.RuntimeDirectoryName : TemporaryWorkspace.RuntimeDirectoryName + "/" + ExternalDirectory;
        Assert.True(workspace.TryCreateDirectorySymbolicLink(link, external.Path, out var cause), cause);

        workspace.Dispose();

        Assert.Equal(before, external.SnapshotHashes(includeRuntime: true));
    }

    [Fact(DisplayName = "Runtime ownership does not admit unrelated unowned workspace files")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void UnrelatedUnownedEntriesStillPreventCleanup()
    {
        using var workspace = TemporaryWorkspace.Create("runtime-unowned");
        var unexpected = workspace.Combine("unexpected.txt");
        File.WriteAllText(unexpected, "unowned");
        Directory.CreateDirectory(workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName));
        try
        {
            Assert.Throws<InvalidOperationException>(workspace.Dispose);
            Assert.Equal("unowned", File.ReadAllText(unexpected));
        }
        finally
        {
            File.Delete(unexpected);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Temporary workspace refuses cleanup after ownership marker removal")]
    [Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void TemporaryWorkspaceRefusesCleanupWithoutOwnershipMarker()
    {
        var workspace = TemporaryWorkspace.Create("ownership-rejection");
        var path = workspace.Path;
        var marker = Path.Combine(path, ".open-forge-test-workspace-owner");
        try
        {
            File.Delete(marker);

            Assert.Throws<InvalidOperationException>(workspace.Dispose);
            Assert.True(Directory.Exists(path));
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Temporary workspace refuses writes through a replaced file link")]
    [Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void TemporaryWorkspaceRefusesWritesThroughReplacedFileLink()
    {
        using var external = TemporaryWorkspace.Create("external-write-target");
        using var workspace = TemporaryWorkspace.Create("write-replacement");
        var externalFile = external.CreateFile("external.txt", "unchanged");
        var ownedFile = workspace.CreateFile("owned.txt", "owned");
        File.Delete(ownedFile);
        File.CreateSymbolicLink(ownedFile, externalFile);
        try
        {
            Assert.Throws<InvalidOperationException>(() =>
                workspace.WriteText("owned.txt", "changed"));
            Assert.Equal("unchanged", File.ReadAllText(externalFile));
        }
        finally
        {
            File.Delete(ownedFile);
            File.WriteAllText(ownedFile, "restored");
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Temporary workspace replacement preserves exact UTF-8 bytes without a stale tail"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void TemporaryWorkspaceReplacementPreservesExactBytes()
    {
        using var workspace = TemporaryWorkspace.Create("replacement-bytes");
        var ownedFile = workspace.CreateFile("owned.txt", "a longer original file");

        workspace.ReplaceText("owned.txt", "Café\n");

        byte[] expected = [0x43, 0x61, 0x66, 0xC3, 0xA9, 0x0A];
        Assert.Equal(expected, File.ReadAllBytes(ownedFile));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Temporary workspace replacement refuses a substituted link and preserves its target"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void TemporaryWorkspaceReplacementRefusesReplacedLinkWithoutTouchingTarget()
    {
        using var external = TemporaryWorkspace.Create("external-replacement-target");
        using var workspace = TemporaryWorkspace.Create("replacement-link");
        var externalFile = external.CreateFile("external.txt", "unchanged sentinel");
        var ownedFile = workspace.CreateFile("owned.txt", "owned");
        try
        {
            File.Delete(ownedFile);
            File.CreateSymbolicLink(ownedFile, externalFile);

            Assert.Throws<InvalidOperationException>(() =>
                workspace.ReplaceText("owned.txt", "changed"));
            Assert.Equal("unchanged sentinel"u8.ToArray(), File.ReadAllBytes(externalFile));
        }
        finally
        {
            File.Delete(ownedFile);
            File.WriteAllText(ownedFile, "restored");
        }
    }
}
