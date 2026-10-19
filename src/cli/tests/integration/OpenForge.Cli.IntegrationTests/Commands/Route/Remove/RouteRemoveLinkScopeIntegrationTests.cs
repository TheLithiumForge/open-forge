using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Feature", "route-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class RouteRemoveLinkScopeIntegrationTests
{
    [Theory(DisplayName = "Applying Route Remove drops both listed layers of leaves and every entrypoint form")]
    [InlineData(null)]
    [InlineData("_topics.md")]
    [InlineData("index.md")]
    [InlineData("_index.md")]
    [InlineData("references.md")]
    [InlineData("_references.md")]
    public async Task DropsListedLayers(string? entrypoint)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("remove-layer-link-scope");
        var path = RouteRemoveIntegrationWorkspace.LeafPath;
        var subject = RouteRemoveIntegrationWorkspace.LeafId;
        if (entrypoint is not null)
        {
            path = $".agents/guidance/topics/{entrypoint}";
            subject = RouteRemoveIntegrationWorkspace.CategoryId;
            if (entrypoint != "_topics.md")
            {
                workspace.WriteText(path, workspace.ReadText(RouteRemoveIntegrationWorkspace.CategoryPath));
                workspace.DeleteFile(RouteRemoveIntegrationWorkspace.CategoryPath);
                foreach (var source in new[] { RouteRemoveIntegrationWorkspace.ParentPath, RouteRemoveIntegrationWorkspace.LeafOverwritePath, "notes.md" })
                    workspace.WriteText(source, workspace.ReadText(source).Replace("topics/_topics.md", $"topics/{entrypoint}", StringComparison.Ordinal));
            }
        }
        var overwrite = path[..^3] + ".overwrite.md";
        if (entrypoint is not null) workspace.WriteText(overwrite, "Local root overwrite.\n");
        var entries = new[] { path, overwrite, "keep.md" };
        var authored = new JsonObject { ["future"] = JsonValue.Create(7) };
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            authored[key] = new JsonArray(entries.Select(entry => (JsonNode?)JsonValue.Create(entry)).ToArray());
        workspace.WriteText(".agents/open-forge.json", authored.ToJsonString());
        var output = new StringWriter();
        var error = new StringWriter();
        var run = await workspace.RunAsync(["route", "remove", subject, "--automatic"], output, error);
        Assert.True(run.ExitCode == 0, output.ToString() + error);
        Assert.False(File.Exists(workspace.Combine(path)));
        Assert.False(File.Exists(workspace.Combine(overwrite)));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
            Assert.Equal(["keep.md"], settings.RootElement.GetProperty(key).EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, settings.RootElement.GetProperty("future").GetInt32());
    }

    [Fact(DisplayName = "Route Remove drops listed folders and descendants and preserves ignored incoming links")]
    public async Task DropsScopeAndPreservesIgnoredBytes()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("remove-link-scope");
        workspace.WriteText(".agents/open-forge.json", """{"future":{"keep":7},"ignoreLinksFrom":["notes.md",".agents/guidance/topics"],"ignoreLinksTo":[".agents/guidance/topics/child.md","other"]}""");
        workspace.WriteText("notes.md", "# Notes\n\n[Child](.agents/guidance/topics/child.md)\n[Broken](missing.md)\n");
        var before = workspace.ReadText("notes.md");
        workspace.WriteText("notes-to.md", "[Child](.agents/guidance/topics/child.md)\n");
        var targetIgnoredBefore = workspace.ReadText("notes-to.md");
        var output = new StringWriter();
        var error = new StringWriter();
        var result = await workspace.RunAsync(["route", "remove", RouteRemoveIntegrationWorkspace.CategoryId, "--automatic"], output, error);
        Assert.True(result.ExitCode == 0, output.ToString() + error);
        Assert.Equal(before, workspace.ReadText("notes.md"));
        Assert.Equal(targetIgnoredBefore, workspace.ReadText("notes-to.md"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Equal(["notes.md"], settings.RootElement.GetProperty("ignoreLinksFrom").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["other"], settings.RootElement.GetProperty("ignoreLinksTo").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, settings.RootElement.GetProperty("future").GetProperty("keep").GetInt32());
        Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
    }
}
