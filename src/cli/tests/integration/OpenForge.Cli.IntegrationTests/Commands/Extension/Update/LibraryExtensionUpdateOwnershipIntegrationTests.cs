using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class LibraryExtensionUpdateOwnershipIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibraryClaimCannotBeAdoptedByExtensionUpdate()
    {
        const string targetPath = ".agents/toolkit/note.md";
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-update");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("library-extension-update-source");
        source.AddPackage("toolkit", [], (targetPath, "Extension-owned bytes."));
        var installed = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, installed.ExitCode);
        source.ReplacePayload("toolkit", targetPath, "Updated Extension-owned bytes.");
        OpenForge.Cli.IntegrationTests.Framework.Ownership.OwnershipFixture.Libraries(workspace.Path,
            new("team-knowledge", "shared/team-knowledge", ".", [targetPath]));
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var run = await workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(5, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }
}
