using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class LibraryExtensionInstallOwnershipIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibraryClaimCannotBeAdoptedByExtensionInstall()
    {
        const string targetPath = ".agents/toolkit/note.md";
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-install");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("library-extension-install-source");
        source.AddPackage("toolkit", [], (targetPath, "Extension-owned bytes."));
        OpenForge.Cli.IntegrationTests.Framework.Ownership.OwnershipFixture.Libraries(workspace.Path,
            new("team-knowledge", "shared/team-knowledge", ".", [targetPath]));
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var run = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(5, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibrarySourceCannotBeReplacedByExtensionInstallForce()
    {
        const string sourcePath = "shared/team-knowledge/.agents/toolkit/note.md";
        const string destinationPath = ".agents/toolkit/note.md";
        const string relativeLink = "../../shared/team-knowledge/.agents/toolkit/note.md";
        const string sourceBytes = "Library-owned bytes.";
        const string permissionPath = ".agents/open-forge.json";

        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-install-source");
        workspace.CreateDirectory(".agents/toolkit");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("library-extension-install-source-catalogue");
        source.AddPackage("toolkit", [], (sourcePath, "Extension-owned bytes."));
        workspace.CreateDirectory("shared/team-knowledge/.agents/toolkit");
        workspace.CreateOccupant(sourcePath, sourceBytes);

        var sourcePhysicalPath = workspace.Combine(sourcePath);
        var destinationPhysicalPath = workspace.Combine(destinationPath);
        try
        {
            File.CreateSymbolicLink(destinationPhysicalPath, relativeLink);
            OpenForge.Cli.IntegrationTests.Framework.Ownership.OwnershipFixture.Libraries(workspace.Path,
                new("team-knowledge", "shared/team-knowledge", ".", [".agents/toolkit/note.md"]));

            var before = workspace.Snapshot();
            var sourceBefore = source.Snapshot();
            var ownershipBefore = File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath));
            var permissionExistedBefore = File.Exists(workspace.Combine(permissionPath));
            var permissionBefore = permissionExistedBefore
                ? File.ReadAllBytes(workspace.Combine(permissionPath))
                : null;

            var run = await workspace.RunAsync([
                "extension", "install", "toolkit", "--source", source.Path,
                "--force", "--allow-path", sourcePath, "--automatic", "--format", "json",
            ]);

            Assert.Equal(5, run.ExitCode);
            using var document = JsonDocument.Parse(run.StandardOutput);
            Assert.Contains(
                document.RootElement.GetProperty("findings").EnumerateArray(),
                value => value.GetProperty("code").GetString() == "extension-install.ownership-conflict"
                    && value.GetProperty("message").GetString()!.Contains("source root", StringComparison.OrdinalIgnoreCase)
                    && value.GetProperty("subject").GetProperty("path").GetString() == sourcePath);
            Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(sourceBefore, source.Snapshot());
            Assert.Equal(sourceBytes, File.ReadAllText(sourcePhysicalPath));
            Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath)));
            Assert.Equal(relativeLink, new FileInfo(destinationPhysicalPath).LinkTarget);
            Assert.Equal(permissionExistedBefore, File.Exists(workspace.Combine(permissionPath)));
            if (permissionExistedBefore)
            {
                Assert.Equal(permissionBefore, File.ReadAllBytes(workspace.Combine(permissionPath)));
            }
        }
        finally
        {
            if (File.Exists(destinationPhysicalPath) || new FileInfo(destinationPhysicalPath).LinkTarget is not null)
            {
                File.Delete(destinationPhysicalPath);
            }

            if (File.Exists(sourcePhysicalPath) || new FileInfo(sourcePhysicalPath).LinkTarget is not null)
            {
                File.Delete(sourcePhysicalPath);
            }
        }
    }
}
