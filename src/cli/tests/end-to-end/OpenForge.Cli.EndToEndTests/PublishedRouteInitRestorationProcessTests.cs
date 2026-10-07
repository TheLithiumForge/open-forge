using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedRouteInitRestorationProcessTests
{
    [Fact(DisplayName = "Published canonical Skills restoration previews payload, applies it, and repeats without effects")]
    public async Task SkillsRestorationFormsOnePublicJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateFramework();
        var installed = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
            ["install", "--automatic", "--format", "json", "--workspace", workspace.Path], workspace.ProcessEnvironment);
        Assert.Equal(0, installed.ExitCode);
        Directory.Delete(workspace.Combine(".agents/skills"), recursive: true);
        string[] arguments = ["route", "init", "skills", "--framework", "--format", "json", "--detail", "full", "--workspace", workspace.Path];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            [.. arguments, "--dry-run"], workspace.ProcessEnvironment);
        Assert.Equal(0, preview.ExitCode);
        Assert.Empty(preview.StandardError);
        using var previewJson = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal(3, previewJson.RootElement.GetProperty("schemaVersion").GetInt32());
        var payloadPaths = previewJson.RootElement.GetProperty("data").GetProperty("payload").EnumerateArray()
            .Select(asset => asset.GetProperty("sourceAssetPath").GetString()).ToArray();
        Assert.Equal(new[]
        {
            ".agents/skills/open-forge-cli/SKILL.md",
            ".agents/skills/open-forge-cli/references/common.md",
            ".agents/skills/open-forge-cli/references/discovery.md",
            ".agents/skills/open-forge-cli/references/packages.md",
            ".agents/skills/open-forge-cli/references/routes.md",
        }, payloadPaths);
        Assert.DoesNotContain(previewJson.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray(),
            entry => entry.GetProperty("path").GetString()?.EndsWith("SKILL.md", StringComparison.Ordinal) == true);
        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Empty(applied.StandardError);
        using var appliedJson = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("verified", appliedJson.RootElement.GetProperty("data").GetProperty("verification").GetString());
        Assert.True(File.Exists(workspace.Combine(".agents/skills/open-forge-cli/SKILL.md")));
        Assert.All(payloadPaths, path => Assert.True(File.Exists(workspace.Combine(Assert.IsType<string>(path)))));
        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, repeated.ExitCode);
        using var repeatedJson = JsonDocument.Parse(repeated.StandardOutput);
        Assert.Empty(repeatedJson.RootElement.GetProperty("effects").EnumerateArray());
    }

    [Fact(DisplayName = "Composed Route Init help documents canonical restoration with existing flags")]
    public async Task HelpExposesCanonicalRestoration()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateFramework();
        var help = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            ["route", "init", "--help"], workspace.ProcessEnvironment);
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("route init skills --framework --dry-run", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("exact canonical Core root or Memory state", help.StandardOutput, StringComparison.Ordinal);
        Assert.Empty(help.StandardError);
    }
}
