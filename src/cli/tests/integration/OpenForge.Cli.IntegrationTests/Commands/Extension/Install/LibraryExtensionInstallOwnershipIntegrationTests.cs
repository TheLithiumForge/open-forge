using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class LibraryExtensionInstallOwnershipIntegrationTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibraryClaimCannotBeAdoptedByExtensionInstall()
    {
        const string targetPath = ".agents/toolkit/note.md";
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("library-extension-install");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("library-extension-install-source");
        source.AddPackage("toolkit", [], (targetPath, "Extension-owned bytes."));
        workspace.CreateOccupant(".agents/open-forge.libraries.json", """
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/toolkit/note.md"]}]}
            """);
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var run = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--json"]);
        Assert.Equal(5, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }
}
