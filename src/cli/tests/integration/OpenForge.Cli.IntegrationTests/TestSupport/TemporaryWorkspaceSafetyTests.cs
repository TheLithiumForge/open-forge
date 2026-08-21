using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

public sealed class TemporaryWorkspaceSafetyTests
{
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
}
