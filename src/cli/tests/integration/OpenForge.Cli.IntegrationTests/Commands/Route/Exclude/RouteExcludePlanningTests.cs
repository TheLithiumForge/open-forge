using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed partial class RouteExcludePlanningTests
{
    private const string GuidePath = "guide.md";
    private const string OtherPath = "other.md";
    private const string MissingPath = "missing.md";
    private const string Category = "patterns";
    private const string CataloguePath = SourceLogicalPath.AgentsRoot + "/" + Category + "/_" + Category + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Exclude previews removal of matching rows including missing and hand-written targets"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RemovesRows(bool registered, bool missing)
    {
        using var workspace = new RouteInclusionWorkspace("- [Guide](../guide.md)\n- [Again](../guide.md) - #Docs\n- [Other](../other.md)");
        workspace.Files.WriteText(OtherPath, "other\n");
        if (registered) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, "{\"roots\":[\"guide.md\"]}\n");
        if (missing) File.Delete(workspace.Files.Combine(GuidePath));
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteExcludeOperation().ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = GuidePath, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Outcome.After);
        var loader = Assert.Single(result.Outcome.Effects.Where(effect => effect.Path == SourceLogicalPath.LoaderPath));
        var layout = WorkspaceLayout.Create(workspace.Workspace, WorkspaceLayout.DefaultMainRoot);
        Assert.Equal(OtherPath, Assert.Single(SourceLoaderEntriesParser.Parse(layout, loader.After).Destinations).CanonicalPath);
        Assert.Equal(registered, result.Outcome.RegisteredBefore);
        Assert.False(result.Outcome.RegisteredAfter);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "Exclude turns a main-root catalogue off without changing it"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    public async Task MainRoot()
    {
        using var workspace = new RouteInclusionWorkspace("- [Patterns](patterns/_patterns.md) - #Pattern");
        workspace.AddRoot();
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteExcludeOperation().ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = CataloguePath, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Outcome.After);
        Assert.Empty(SourceLoaderEntriesParser.Parse(WorkspaceLayout.Create(workspace.Workspace, WorkspaceLayout.DefaultMainRoot), Assert.Single(result.Outcome.Effects).After).Destinations);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "Exclude with nothing listed or registered returns successful no-change JSON"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    public async Task NothingToDo()
    {
        using var workspace = new RouteInclusionWorkspace();
        var before = workspace.Files.SnapshotHashes();
        var result = await CliHostCapture.RunAsync([PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, MissingPath, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], workspace.Files.Path);
        Assert.Equal(0, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }
}
