using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallRemovedCategoriesIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install honors authored removed root categories without editing settings"), Trait("Evidence", "Integration")]
    public async Task DoesNotReinstateRemovedRootCategory()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("install-removed-categories");
        workspace.CreateDirectory(".agents");
        const string settings = """{ "removedCategories": ["skills"], "foreign": true }""";
        workspace.CreateOccupant(".agents/open-forge.json", settings);
        var run = await workspace.RunAsync(["install", "--automatic", "--format", "json"]);
        Assert.Equal(0, run.ExitCode);
        Assert.False(Directory.Exists(workspace.Combine(".agents/skills")));
        Assert.True(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
        Assert.DoesNotContain("skills/_skills.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        using var ownership = JsonDocument.Parse(workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath));
        Assert.DoesNotContain(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray(),
            path => path.GetString()!.StartsWith(".agents/skills/", StringComparison.Ordinal));
    }
}
