using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteMoveOutsideApplicationIntegrationTests
{
    [Theory(DisplayName = "Outside leaf preview and application retain conditioned rows and never relist the Loader")]
    [InlineData(false, false, false), InlineData(true, false, true), InlineData(false, true, true), InlineData(true, true, false)]
    public async Task Leaf(bool custom, bool compatibility, bool listed)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom, compatibility, listed);
        var loader = workspace.Bytes(workspace.Loader);
        var settings = workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath);
        var before = workspace.Snapshot();
        var operation = RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot);
        var preview = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Complete(preview);
        Assert.Equal(before, workspace.Snapshot());
        workspace.OwnLeaf();
        var applied = await operation.ExecuteAsync(workspace.Request(mode: RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(applied);
        Assert.False(File.Exists(workspace.Absolute(RouteMoveOutsideWorkspace.Leaf)));
        Assert.Equal(Encoding.UTF8.GetBytes(RouteMoveOutsideWorkspace.Markdown(RouteMoveOutsideWorkspace.LeafText.Replace("(local.txt)", "(../left/local.txt)", StringComparison.Ordinal))),
            workspace.Bytes(RouteMoveOutsideWorkspace.Destination));
        Assert.Equal(Encoding.UTF8.GetBytes(RouteMoveOutsideWorkspace.OverwriteText), workspace.Bytes(SourceOverwritePath.ReadAdjacentPath(RouteMoveOutsideWorkspace.Destination)));
        Assert.Equal(loader, workspace.Bytes(workspace.Loader));
        Assert.Equal(settings, workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath));
        var expectedRows = new[]
        {
            $"- [First](../right/new%20note.md) - #Docs #{SourceLoadingClosureResolver.LoadNowTag} - applies to `src/**`",
            "- [Second](../right/new%20note.md) - #Review - applies to `tests/**`",
            "- [Topic](topic/_topic.md) - #Docs",
            "- [Incoming](incoming.md) - #Docs",
        };
        var left = workspace.Read(workspace.LeftCatalogue);
        var parsed = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(left));
        Assert.Equal(expectedRows, parsed.Entries.Select(row => left[row.Span.Start..row.Span.End]));
        var repeated = await operation.ExecuteAsync(workspace.Request(mode: RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(RouteMoveFindingCode.SourceNotFound, Assert.Single(repeated.Findings).Code);
    }

    [Theory(DisplayName = "Outside descendant categories retain opaque resources and recognized entrypoint forms")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task Category(bool custom, bool compatibility)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom, compatibility: true);
        var source = RouteMoveOutsideWorkspace.Left + "/topic/_topic.md";
        var destination = RouteMoveOutsideWorkspace.Right + "/renamed/_renamed.md";
        if (compatibility)
        {
            var current = workspace.Read(source);
            File.Delete(workspace.Absolute(source));
            source = RouteMoveOutsideWorkspace.Left + "/topic/" + SourceFormClassifier.IndexFileName;
            destination = RouteMoveOutsideWorkspace.Right + "/renamed/" + SourceFormClassifier.IndexFileName;
            workspace.Write(source, current);
            workspace.Write(workspace.LeftCatalogue, workspace.Read(workspace.LeftCatalogue).Replace("topic/_topic.md", "topic/index.md", StringComparison.Ordinal));
        }
        workspace.OwnCategory(source, destination);
        var resource = workspace.Bytes(RouteMoveOutsideWorkspace.Left + "/topic/assets/data.bin");
        var loader = workspace.Bytes(workspace.Loader);
        var result = await RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot).ExecuteAsync(
            workspace.Request(source, destination, RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(result);
        Assert.False(Directory.Exists(workspace.Absolute(RouteMoveOutsideWorkspace.Left + "/topic")));
        Assert.Equal(resource, workspace.Bytes(RouteMoveOutsideWorkspace.Right + "/renamed/assets/data.bin"));
        Assert.Equal(loader, workspace.Bytes(workspace.Loader));
    }

    [Theory(DisplayName = "Outside ignored incoming links preserve their bytes while literal removal exclusions stay put")]
    [InlineData(false), InlineData(true)]
    public async Task IgnoredAndExcluded(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        const string foreign = "foreign.md";
        const string content = "[Foreign](docs/team/left/old%20note.md)\n";
        workspace.Write(foreign, content);
        workspace.ChangeSettings(settings =>
        {
            settings[WorkspaceSettingsDefinitions.IgnoreLinksFromProperty] = new JsonArray(foreign);
            settings[WorkspaceSettingsDefinitions.IgnoreLinksToProperty] = new JsonArray(RouteMoveOutsideWorkspace.Leaf);
            settings[WorkspaceSettingsDefinitions.RemovedFilesProperty] = new JsonArray(RouteMoveOutsideWorkspace.Leaf);
            settings[WorkspaceSettingsDefinitions.RemovedDirectoriesProperty] = new JsonArray(RouteMoveOutsideWorkspace.Left + "/topic");
            settings[WorkspaceSettingsDefinitions.RemovedCategoriesProperty] = new JsonArray("unchanged");
            settings[WorkspaceSettingsDefinitions.RootsProperty] = new JsonArray("shared.md");
        });
        workspace.OwnLeaf();
        var result = await RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot).ExecuteAsync(
            workspace.Request(mode: RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(result);
        Assert.Equal(Encoding.UTF8.GetBytes(content), workspace.Bytes(foreign));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath)).Document);
        Assert.Equal([RouteMoveOutsideWorkspace.Leaf], settings.RemovedFiles);
        Assert.Equal([RouteMoveOutsideWorkspace.Destination], settings.IgnoreLinksTo);
        Assert.Equal([RouteMoveOutsideWorkspace.Root], settings.OutsideRoutes);
        Assert.Equal(["shared.md"], settings.Roots);
        Assert.Equal([RouteMoveOutsideWorkspace.Left + "/topic"], settings.RemovedDirectories);
        Assert.Equal(["unchanged"], settings.RemovedCategories);
        var raw = Assert.IsType<JsonObject>(JsonNode.Parse(workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(7, raw["future"]?["keep"]?.GetValue<int>());
    }

    internal static void Complete(RouteMoveResult result)
        => Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));

    [Theory(DisplayName = "A stale canonical Loader row cannot veto an available outside compatibility anchor")]
    [InlineData(false), InlineData(true)]
    public async Task StaleLoader(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom, compatibility: true);
        var link = Path.GetRelativePath(workspace.MainRoot, RouteMoveOutsideWorkspace.Root + "/_team.md").Replace(Path.DirectorySeparatorChar, '/');
        workspace.Write(workspace.Loader, OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.GeneratedEntries($"- [Stale canonical]({link}) - #Docs"));
        var loader = workspace.Bytes(workspace.Loader);
        workspace.OwnLeaf();
        var result = await RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot).ExecuteAsync(
            workspace.Request(source: SourceOverwritePath.ReadAdjacentPath(RouteMoveOutsideWorkspace.Leaf), mode: RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(result);
        Assert.Equal(loader, workspace.Bytes(workspace.Loader));
        Assert.Equal(Encoding.UTF8.GetBytes(RouteMoveOutsideWorkspace.OverwriteText), workspace.Bytes(SourceOverwritePath.ReadAdjacentPath(RouteMoveOutsideWorkspace.Destination)));
    }

    [Theory(DisplayName = "Outside row destination projection preserves authored blank lines and repeated row policy bytes")]
    [InlineData(false), InlineData(true)]
    public async Task RowBytes(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var original = workspace.Read(workspace.LeftCatalogue).Replace("\n- [Second]", "\n\n- [Second]", StringComparison.Ordinal)
            + "\nUntouched café bytes and [ordinary prose](old%20note.md#leaf).\n";
        workspace.Write(workspace.LeftCatalogue, original);
        var expected = Encoding.UTF8.GetBytes(original.Replace("(old%20note.md)", "(../right/new%20note.md)", StringComparison.Ordinal)
            .Replace("(old%20note.md#leaf)", "(../right/new%20note.md#leaf)", StringComparison.Ordinal));
        workspace.OwnLeaf();
        var result = await RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot).ExecuteAsync(
            workspace.Request(mode: RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(result);
        Assert.Equal(expected, workspace.Bytes(workspace.LeftCatalogue));
    }

    [Theory(DisplayName = "Outside SKILL filenames remain ordinary Markdown leaves")]
    [InlineData(false), InlineData(true)]
    public async Task OrdinarySkill(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var source = RouteMoveOutsideWorkspace.Left + "/" + SourceFormClassifier.NativeSkillFileName;
        var destination = RouteMoveOutsideWorkspace.Right + "/" + SourceFormClassifier.NativeSkillFileName;
        var body = RouteMoveOutsideWorkspace.Markdown("# Ordinary outside skill\n\n[Folder](topic/) and [angle](<local.txt>).\n");
        workspace.Write(source, body);
        workspace.Write(workspace.LeftCatalogue, workspace.Read(workspace.LeftCatalogue).Replace("- [Incoming]", $"- [Ordinary]({SourceFormClassifier.NativeSkillFileName}) - #Docs\n- [Incoming]", StringComparison.Ordinal));
        workspace.OwnLeaf(destination);
        var result = await RouteMoveOperationFactory.Create(workspace.LockStore.StoreRoot).ExecuteAsync(
            workspace.Request(source, destination, RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        Complete(result);
        Assert.Equal(Encoding.UTF8.GetBytes(body.Replace("(topic/)", "(../left/topic/)", StringComparison.Ordinal)
            .Replace("(<local.txt>)", "(<../left/local.txt>)", StringComparison.Ordinal)), workspace.Bytes(destination));
    }
}
