using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteExcludeProcessTests
{
    [Theory(DisplayName = "Published Exclude previews and removes an included row without changing the target"), Trait("Feature", "route-exclude"), Trait("Evidence", "EndToEnd")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewApplyAndNoOp(bool missing)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task<ProcessRunResult> Run(string[] args) => await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run(["install", "--automatic"])).ExitCode);
        Assert.Equal(0, (await Run(["route", "include", "workspace-note.md", "--description", "Project note"])).ExitCode);
        var file = Path.Combine(workspace.WorkspacePath, "workspace-note.md");
        var bytes = File.ReadAllBytes(file);
        if (missing) File.Delete(file);
        string[] args = ["route", "exclude", "workspace-note.md", "--format", "json"];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.WorkspacePath,
            workspace.SnapshotWorkspace, [.. args, "--dry-run"], workspace.EnvironmentVariables);
        Assert.Equal(0, preview.ExitCode);
        var applied = await Run(args);
        Assert.Equal(0, applied.ExitCode);
        using var json = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.Null(json.RootElement.GetProperty("data").GetProperty("after").GetString());
        var noOp = await Run(args);
        Assert.Equal(0, noOp.ExitCode);
        using var repeated = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(repeated.RootElement.GetProperty("effects").EnumerateArray());
        if (!missing) Assert.Equal(bytes, File.ReadAllBytes(file));
    }
}
