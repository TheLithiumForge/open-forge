using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

public sealed class TemporaryWorkspaceSafetyTests
{
    [Fact(DisplayName = "Temporary workspace owns runtime contents created outside fixture APIs")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void RuntimeContentsAreRemovedOnDisposal()
    {
        var workspace = TemporaryWorkspace.Create("runtime-ownership");
        var root = workspace.Path;
        var runtime = workspace.CreateDirectory(TemporaryWorkspace.RuntimeDirectoryName);
        Directory.CreateDirectory(Path.Combine(runtime, "recovery", "v1"));
        File.WriteAllText(Path.Combine(runtime, "recovery", "v1", "record.json"), "{}");
        File.WriteAllText(Path.Combine(runtime, "workspace.lock"), "arbitrary sentinel bytes");

        workspace.Dispose();
        workspace.Dispose();

        Assert.False(Directory.Exists(root));
    }

    [Fact(DisplayName = "Default workspace snapshot excludes a held runtime sentinel only at the workspace root")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void DefaultSnapshotExcludesHeldRuntimeState()
    {
        using var workspace = TemporaryWorkspace.Create("runtime-snapshot");
        workspace.CreateFile("authored/.open-forge-state/notes.txt", "authored bytes");
        var before = workspace.SnapshotHashes();
        var sentinel = workspace.CreateFile(".open-forge-state/workspace.lock", "held");
        using var held = new FileStream(sentinel, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Contains("authored/.open-forge-state/notes.txt", before.Keys);
    }

    [Fact(DisplayName = "Explicit runtime snapshot observes empty runtime directories and later contents")]
    [Trait("Boundary", "OS"), Trait("Feature", "cli-test-support"), Trait("Evidence", "Integration")]
    public void ExplicitSnapshotIncludesRuntimePresenceAndBytes()
    {
        using var workspace = TemporaryWorkspace.Create("runtime-explicit-snapshot");
        Assert.Empty(workspace.SnapshotHashes(includeRuntime: true));
        workspace.CreateDirectory(TemporaryWorkspace.RuntimeDirectoryName);
        Assert.Contains(TemporaryWorkspace.RuntimeDirectoryName, workspace.SnapshotHashes(includeRuntime: true).Keys);
        workspace.CreateFile(".open-forge-state/workspace.lock", "first");
        var before = workspace.SnapshotHashes(includeRuntime: true);
        workspace.ReplaceText(".open-forge-state/workspace.lock", "second");

        Assert.NotEqual(before[".open-forge-state/workspace.lock"], workspace.SnapshotHashes(includeRuntime: true)[".open-forge-state/workspace.lock"]);
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
        var link = linkAtRoot ? TemporaryWorkspace.RuntimeDirectoryName : ".open-forge-state/external";
        Assert.SkipUnless(workspace.TryCreateDirectorySymbolicLink(link, external.Path, out _), "Directory symbolic links are unavailable.");

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
