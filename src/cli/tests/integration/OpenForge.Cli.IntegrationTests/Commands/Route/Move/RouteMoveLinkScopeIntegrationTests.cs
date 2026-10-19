using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class RouteMoveLinkScopeIntegrationTests
{
    [Fact(DisplayName = "Applying a leaf move remaps base and overwrite entries in both ignore lists")]
    public async Task LeafApplicationRemapsBothLayers()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-leaf-link-scope");
        var entries = new[] { RouteMoveIntegrationWorkspace.LeafPath, RouteMoveIntegrationWorkspace.OverwritePath, "keep.md" };
        var authored = new JsonObject { ["future"] = JsonValue.Create(7) };
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            authored[key] = new JsonArray(entries.Select(entry => (JsonNode?)JsonValue.Create(entry)).ToArray());
        workspace.WriteText(".agents/open-forge.json", authored.ToJsonString());
        workspace.OwnLeafDestination(crossRoute: false);
        var run = await CliHostCapture.RunAsync(["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination], workspace.Workspace.LexicalRoot);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.LeafDestination)));
        Assert.True(File.Exists(workspace.Absolute(".agents/guidance/new guide.overwrite.md")));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            Assert.Equal([".agents/guidance/new guide.md", ".agents/guidance/new guide.overwrite.md", "keep.md"], settings.RootElement.GetProperty(key).EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, settings.RootElement.GetProperty("future").GetInt32());
    }

    [Theory(DisplayName = "Applying a category rename maps listed root layers before folder descendants")]
    [InlineData("_topics.md", "_renamed-topics.md")]
    [InlineData("index.md", "index.md")]
    [InlineData("_index.md", "_index.md")]
    [InlineData("references.md", "references.md")]
    [InlineData("_references.md", "_references.md")]
    public async Task CategoryApplicationRemapsRootLayers(string entrypoint, string destinationEntrypoint)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-root-layer-link-scope");
        var source = $".agents/guidance/topics/{entrypoint}";
        if (entrypoint != "_topics.md")
        {
            workspace.WriteText(source, workspace.ReadText(RouteMoveIntegrationWorkspace.CategoryPath));
            workspace.DeleteFile(RouteMoveIntegrationWorkspace.CategoryPath);
            const string parent = ".agents/guidance/_guidance.md";
            workspace.WriteText(parent, workspace.ReadText(parent).Replace("topics/_topics.md", $"topics/{entrypoint}", StringComparison.Ordinal));
        }
        var overwrite = source[..^3] + ".overwrite.md";
        workspace.WriteText(overwrite, "Local root overwrite.\n");
        var entries = new[] { source, overwrite, ".agents/guidance/topics", ".agents/guidance/topics/child.md", ".agents/guidance/topics/future/note.md", ".agents/guidance/topics.md" };
        var authored = new JsonObject { ["future"] = JsonValue.Create(7) };
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            authored[key] = new JsonArray(entries.Select(entry => (JsonNode?)JsonValue.Create(entry)).ToArray());
        workspace.WriteText(".agents/open-forge.json", authored.ToJsonString());
        const string destination = ".agents/archive/renamed-topics";
        workspace.OwnCategoryDestination(destination, destinationEntrypoint);
        var run = await CliHostCapture.RunAsync(["route", "move", RouteMoveIntegrationWorkspace.CategoryId, "archive/renamed-topics"], workspace.Workspace.LexicalRoot);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        var target = $"{destination}/{destinationEntrypoint}";
        Assert.True(File.Exists(workspace.Absolute(target)));
        Assert.Equal("Local root overwrite.\n", workspace.ReadText(target[..^3] + ".overwrite.md"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            Assert.Equal([target, target[..^3] + ".overwrite.md", destination, $"{destination}/child.md", $"{destination}/future/note.md", ".agents/guidance/topics.md"], settings.RootElement.GetProperty(key).EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, settings.RootElement.GetProperty("future").GetInt32());
    }

    [Fact(DisplayName = "A renamed category keeps its canonical destination entrypoint and remaps listed paths")]
    public async Task RenamedCategoryProjectsCanonicalRootAndListedPaths()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-renamed-link-scope");
        workspace.WriteText(".agents/open-forge.json", """{"ignoreLinksFrom":[".agents/guidance/topics"],"ignoreLinksTo":[".agents/guidance/topics/child.md"]}""");
        var before = workspace.SnapshotHashes();
        var built = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(RouteMoveIntegrationWorkspace.CategoryId, "archive/renamed-topics"), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(built.Plan);
        Assert.Contains(plan.Projection.FileChanges, change => change.LogicalPath == workspace.Absolute(".agents/archive/renamed-topics/_renamed-topics.md"));
        Assert.DoesNotContain(plan.Projection.FileChanges, change => change.LogicalPath == workspace.Absolute(".agents/archive/renamed-topics/_topics.md"));
        var change = Assert.Single(plan.Projection.FileChanges, change => change.LogicalPath == workspace.Absolute(".agents/open-forge.json"));
        using var settings = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        Assert.Equal([".agents/archive/renamed-topics"], settings.RootElement.GetProperty("ignoreLinksFrom").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/archive/renamed-topics/child.md"], settings.RootElement.GetProperty("ignoreLinksTo").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move preserves ignored links and remaps listed folders and descendants")]
    public async Task RemapsScopeAndPreservesIgnoredBytes()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-link-scope");
        workspace.WriteText(".agents/open-forge.json", """{"future":{"keep":7},"ignoreLinksFrom":[".agents/guidance/topics","notes.md"],"ignoreLinksTo":[".agents/guidance/topics/child.md"]}""");
        var childBefore = workspace.ReadText(".agents/guidance/topics/child.md");
        workspace.WriteText("notes-to.md", "[Child](.agents/guidance/topics/child.md)\n");
        var targetIgnoredBefore = workspace.ReadText("notes-to.md");
        var notesBefore = workspace.ReadText("notes.md");
        var before = workspace.SnapshotHashes();
        var args = new[] { "route", "move", RouteMoveIntegrationWorkspace.CategoryId, "archive/topics" };
        var preview = await CliHostCapture.RunAsync([.. args, "--dry-run"], workspace.Workspace.LexicalRoot);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.OwnCategoryDestination();
        var run = await CliHostCapture.RunAsync(args, workspace.Workspace.LexicalRoot);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.Equal(childBefore, workspace.ReadText(".agents/archive/topics/child.md"));
        Assert.Equal(notesBefore, workspace.ReadText("notes.md"));
        Assert.Equal(targetIgnoredBefore, workspace.ReadText("notes-to.md"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Equal([".agents/archive/topics", "notes.md"], settings.RootElement.GetProperty("ignoreLinksFrom").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/archive/topics/child.md"], settings.RootElement.GetProperty("ignoreLinksTo").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, settings.RootElement.GetProperty("future").GetProperty("keep").GetInt32());
    }
}
