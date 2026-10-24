using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteIncludeProcessTests
{
    private const string WorkspaceNote = "workspace-note.md";
    private const string NoteDescription = "Project note";
    private const string GuideDescription = "Project guide";
    private const string DocsTag = "Docs";
    private const string SourcePattern = "src/**";
    private const string ExtensionName = "toolkit";

    [Theory(DisplayName = "Published Include previews and applies text and JSON with optional metadata"), Trait("Feature", "route-include"), Trait("Evidence", "EndToEnd")]
    [InlineData(PublishedTokens.Syntax.TextFormat, false)]
    [InlineData(PublishedTokens.Syntax.TextFormat, true)]
    [InlineData(PublishedTokens.Syntax.JsonFormat, false)]
    [InlineData(PublishedTokens.Syntax.JsonFormat, true)]
    public async Task PreviewApplyAndNoOp(string format, bool metadata)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task<ProcessRunResult> Run(string[] args) => await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run([PublishedTokens.Install.Command, PublishedTokens.Install.Automatic])).ExitCode);
        var file = Path.Combine(workspace.WorkspacePath, WorkspaceNote);
        var bytes = File.ReadAllBytes(file);
        string[] args = [PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, WorkspaceNote, PublishedTokens.RouteInclude.Description, GuideDescription, PublishedTokens.Syntax.FormatOption, format];
        if (metadata) args = [.. args, PublishedTokens.RouteInclude.Tag, "#" + PublishedTokens.Metadata.LoadNow, PublishedTokens.RouteInclude.Tag, DocsTag, PublishedTokens.RouteInclude.ApplyTo, SourcePattern];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.WorkspacePath,
            workspace.SnapshotWorkspace, [.. args, PublishedTokens.RouteInclude.DryRun], workspace.EnvironmentVariables);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(0, (await Run(args)).ExitCode);
        var loader = File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, PublishedTokens.Sources.LoaderPath));
        Assert.Equal(bytes, File.ReadAllBytes(file));
        Assert.Equal(0, (await Run([PublishedTokens.Index.Command])).ExitCode);
        Assert.Equal(loader, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, PublishedTokens.Sources.LoaderPath)));
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
        await Run([PublishedTokens.Install.Command, PublishedTokens.Install.Automatic]);
        var file = WorkspaceNote;
        var targetBytes = File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file));
        await Run([PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, file, PublishedTokens.RouteInclude.Description, NoteDescription]);
        var loaderPath = Path.Combine(workspace.WorkspacePath, PublishedTokens.Sources.LoaderPath);
        var loader = File.ReadAllText(loaderPath);
        var row = $"- [{NoteDescription}](../{WorkspaceNote})";
        var includedLoader = loader;
        File.WriteAllText(loaderPath, loader.Replace(row + "\n", string.Empty, StringComparison.Ordinal), new UTF8Encoding(false));
        var removedLoader = File.ReadAllBytes(loaderPath);
        Assert.NotEqual(Encoding.UTF8.GetBytes(includedLoader), removedLoader);
        string[][] commands =
        [
            [PublishedTokens.Index.Command], [PublishedTokens.Update.Command, PublishedTokens.Update.Automatic], [PublishedTokens.Install.Command, PublishedTokens.Install.Configure, PublishedTokens.Install.Preset, PublishedTokens.Install.CustomPreset, PublishedTokens.Install.Automatic],
            [PublishedTokens.Extension.Command, PublishedTokens.ExtensionInstall.Command, ExtensionName, PublishedTokens.ExtensionInstall.Source, workspace.CataloguePath, PublishedTokens.ExtensionInstall.Automatic],
        ];
        var step = 0;
        foreach (var command in commands)
        {
            await Run(command);
            CommandOutputSnapshot.MatchSnapshot(File.ReadAllText(loaderPath), $"after-{step++}");
            using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.WorkspacePath, PublishedTokens.Settings.RelativePath)));
            Assert.Contains(file, settings.RootElement.GetProperty(PublishedTokens.Settings.RootsProperty).EnumerateArray().Select(value => value.GetString()));
            Assert.Equal(targetBytes, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file)));
        }
        await Run([PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, file]);
        Assert.Equal(targetBytes, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, file)));
    }
}
