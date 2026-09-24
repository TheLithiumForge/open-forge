using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Fresh Install honors category and exact file exclusions, including managed root hosts"), Trait("Evidence", "Integration")]
    public async Task HonorsRemovedFilesAndCategories()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("install-removed-files");
        workspace.CreateDirectory(".agents");
        const string settings = """{"removedCategories":["skills"],"removedFiles":[".agents/patterns/_patterns.md","AGENTS.md","CLAUDE.md"],"future":true}""";
        const string agentHost = "User-owned AGENTS bytes remain untouched.\r\n";
        workspace.CreateOccupant(".agents/open-forge.json", settings);
        workspace.CreateOccupant("AGENTS.md", agentHost);
        var agentBytes = File.ReadAllBytes(workspace.Combine("AGENTS.md"));

        var run = await workspace.RunAsync(["install", "--force", "--automatic", "--format", "json"]);

        Assert.Equal(0, run.ExitCode);
        Assert.False(Directory.Exists(workspace.Combine(".agents/skills")));
        Assert.False(Directory.Exists(workspace.Combine(".agents/patterns")));
        Assert.False(File.Exists(workspace.Combine(".agents/patterns/_patterns.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.DoesNotContain("patterns/_patterns.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        Assert.Equal(agentBytes, File.ReadAllBytes(workspace.Combine("AGENTS.md")));
        Assert.False(File.Exists(workspace.Combine("CLAUDE.md")));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));

        var framework = workspace.ReadFrameworkOwnership();
        Assert.DoesNotContain(framework.GetProperty("paths").EnumerateArray(),
            path => path.GetString() is ".agents/patterns/_patterns.md" or "AGENTS.md" or "CLAUDE.md");
        Assert.DoesNotContain(framework.GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("path").GetString() is "AGENTS.md" or "CLAUDE.md");
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Fresh Install blocks when an excluded missing loader leaves selected routes unreachable"), Trait("Evidence", "Integration")]
    public async Task BlocksUnreachableRoutesBehindExcludedMissingLoader()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("install-removed-loader-ancestor");
        workspace.CreateDirectory(".agents");
        workspace.CreateOccupant(".agents/open-forge.json", """{"removedFiles":[".agents/loader.md"]}""");

        var run = await workspace.RunAsync(["install", "--automatic", "--format", "json"]);

        Assert.NotEqual(0, run.ExitCode);
        Assert.False(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
        Assert.False(File.Exists(workspace.Combine(".agents/memory/_memory.md")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install refuses malformed removed file paths without creating payload or ownership"), Trait("Evidence", "Integration")]
    public async Task RefusesMalformedRemovedFileSettings()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("install-invalid-removed-files");
        workspace.CreateDirectory(".agents");
        const string settings = """{"removedFiles":[".agents/*.md"]}""";
        workspace.CreateOccupant(".agents/open-forge.json", settings);

        var run = await workspace.RunAsync(["install", "--automatic", "--format", "json"]);

        Assert.NotEqual(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
        Assert.False(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
    }
}
