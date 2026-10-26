using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteExcludeProcessTests
{
    private const string WorkspaceNote = "workspace-note.md";
    private const string NoteDescription = "Project note";
    private const string GuideDescription = "Project guide";
    private const string DocsTag = "Docs";
    private const string SourcePattern = "src/**";
    private const string ExtensionName = "toolkit";

    [Theory(DisplayName = "Published Exclude previews and removes an included row without changing the target"), Trait("Feature", "route-exclude"), Trait("Evidence", "EndToEnd")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewApplyAndNoOp(bool missing)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task<ProcessRunResult> Run(string[] args) => await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run([PublishedTokens.Install.Command, PublishedTokens.Install.Automatic])).ExitCode);
        Assert.Equal(0, (await Run([PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, WorkspaceNote, PublishedTokens.RouteInclude.Description, NoteDescription])).ExitCode);
        var file = Path.Combine(workspace.WorkspacePath, WorkspaceNote);
        var bytes = File.ReadAllBytes(file);
        if (missing) File.Delete(file);
        string[] args = [PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, WorkspaceNote, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.WorkspacePath,
            workspace.SnapshotWorkspace, [.. args, PublishedTokens.RouteExclude.DryRun], workspace.EnvironmentVariables);
        Assert.Equal(0, preview.ExitCode);
        var applied = await Run(args);
        Assert.Equal(0, applied.ExitCode);
        using var json = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        Assert.Null(json.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.Json.After).GetString());
        var noOp = await Run(args);
        Assert.Equal(0, noOp.ExitCode);
        using var repeated = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(repeated.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        if (!missing) Assert.Equal(bytes, File.ReadAllBytes(file));
    }
}
