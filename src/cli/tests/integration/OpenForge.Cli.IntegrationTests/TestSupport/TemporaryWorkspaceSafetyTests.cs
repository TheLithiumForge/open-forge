using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

public sealed class TemporaryWorkspaceSafetyTests
{
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
