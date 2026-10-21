using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteIncludeProcessTests
{
    [Theory(DisplayName = "Published Include previews and applies text and JSON with optional metadata"), Trait("Feature", "route-include"), Trait("Evidence", "EndToEnd")]
    [InlineData("text", false)]
    [InlineData("text", true)]
    [InlineData("json", false)]
    [InlineData("json", true)]
    public async Task PreviewApplyAndNoOp(string format, bool metadata)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task<ProcessRunResult> Run(string[] args) => await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run(["install", "--automatic"])).ExitCode);
        var file = Path.Combine(workspace.WorkspacePath, "workspace-note.md");
        var bytes = File.ReadAllBytes(file);
        string[] args = ["route", "include", "workspace-note.md", "--description", "Project guide", "--format", format];
        if (metadata) args = [.. args, "--tag", "#LoadNow", "--tag", "Docs", "--apply-to", "src/**"];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.WorkspacePath,
            workspace.SnapshotWorkspace, [.. args, "--dry-run"], workspace.EnvironmentVariables);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(0, (await Run(args)).ExitCode);
        var loader = File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, ".agents", "loader.md"));
        Assert.Equal(bytes, File.ReadAllBytes(file));
        Assert.Equal(0, (await Run(["index"])).ExitCode);
        Assert.Equal(loader, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, ".agents", "loader.md")));
        var noOp = await Run(args);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(bytes, File.ReadAllBytes(file));
    }

    [Fact(DisplayName = "A removed include stays off through Index, Update, Configure and Extension installation"), Trait("Feature", "route-include"), Trait("Evidence", "EndToEnd")]
    public async Task NoSilentReactivation()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task Run(string[] args)
        {
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
            Assert.True(result.ExitCode == 0, result.StandardOutput + result.StandardError);
        }
        await Run(["install", "--automatic"]);
        const string file = "workspace-note.md";
        var targetBytes = File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file));
        await Run(["route", "include", file, "--description", "Project note"]);
        var loaderPath = Path.Combine(workspace.WorkspacePath, ".agents", "loader.md");
        var loader = File.ReadAllText(loaderPath);
        const string row = "- [Project note](../workspace-note.md)";
        Assert.Contains(row, loader, StringComparison.Ordinal);
        File.WriteAllText(loaderPath, loader.Replace(row + "\n", string.Empty, StringComparison.Ordinal), new UTF8Encoding(false));
        string[][] commands =
        [
            ["index"], ["update", "--automatic"], ["install", "--configure", "--preset", "custom", "--automatic"],
            ["extension", "install", "toolkit", "--source", workspace.CataloguePath, "--automatic"],
        ];
        foreach (var command in commands)
        {
            await Run(command);
            Assert.DoesNotContain("../workspace-note.md", File.ReadAllText(loaderPath), StringComparison.Ordinal);
            using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.WorkspacePath, ".agents", "open-forge.json")));
            Assert.Contains(file, settings.RootElement.GetProperty("roots").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal(targetBytes, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file)));
        }
        await Run(["route", "exclude", file]);
        Assert.Equal(targetBytes, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file)));
    }
}
