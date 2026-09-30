using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInspectProcessTests
{
    [Fact(DisplayName = "Published Route Inspect help explains the matching-files boundary and fallback scope"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task MatchingFilesHelpExplainsScope()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotHashes, ["route", "inspect", "--help"]);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        var text = string.Join(" ", result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("[--matching-files]", text, StringComparison.Ordinal);
        Assert.Contains("List current workspace files matching this source's own and inherited applyTo conditions.", text, StringComparison.Ordinal);
        Assert.Contains("Matching files do not select or load a source. A source with no restrictive applyTo condition answers all files without scanning. --for supplies working paths independently of this scan. At most 100 matching paths are listed.", text, StringComparison.Ordinal);
        Assert.Contains("In a Git work tree, the scan lists existing tracked and untracked files and excludes ignored untracked files. Without a usable Git inventory, it walks the workspace and applies its .gitignore files using Open Forge rules. The scan stays inside the workspace and does not follow symbolic links or enter submodules.", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Published matching-files composes with repeated working paths without changing applicability"),
     InlineData("minimal"), InlineData("standard"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task MatchingFilesOptionPreservesWorkingPaths(string detail)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateConditioned();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotHashes,
            ["route", "inspect", "root", "--matching-files", "--matching-files", "--format", "json", "--detail", detail,
                "--for", "src/Planned.cs", "--for", "docs/other.md"]);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var data = document.RootElement.GetProperty("data");
        var files = data.GetProperty("matchingFiles");
        Assert.Equal("workspace-files", files.GetProperty("scope").GetString());
        Assert.True(files.GetProperty("complete").GetBoolean());
        Assert.Equal(0, files.GetProperty("count").GetInt32());
        Assert.Empty(files.GetProperty("paths").EnumerateArray());
        Assert.Equal(100, files.GetProperty("pathLimit").GetInt32());
        Assert.False(files.GetProperty("truncated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, files.GetProperty("reason").ValueKind);
        Assert.Equal("No current files match this condition. Planned files may still match.", files.GetProperty("note").GetString());
        Assert.Equal(["src/Planned.cs", "docs/other.md"], data.GetProperty("workingPaths").EnumerateArray().Select(path => path.GetString()));
        if (detail == "standard")
        {
            Assert.Equal("matched", data.GetProperty("applicability").GetProperty("state").GetString());
            Assert.Equal(["src/Planned.cs"], data.GetProperty("applicability").GetProperty("matchingPaths").EnumerateArray().Select(path => path.GetString()));
        }
    }

    [Fact(DisplayName = "Published Route Inspect JSON exposes the complete typed identity and profile graph"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task JsonResultExposesCompleteTypedGraph()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--format=json", "--detail=full"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;

        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");
        Assert.Equal("root", data.GetProperty("selection").GetProperty("requested").GetString());
        Assert.False(data.TryGetProperty("applicability", out _));
        Assert.False(data.TryGetProperty("workingPaths", out _));

        Assert.Equal(".agents/root/_root.md", data.GetProperty("path").GetString());
        var layers = data.GetProperty("layers").EnumerateArray().ToArray();
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            layers.Select(layer => layer.GetProperty("path").GetString()));
        Assert.Equal(["base", "overwrite"], layers.Select(layer => layer.GetProperty("kind").GetString()));

        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Theory(DisplayName = "Published Route Inspect explains a matched repeated working-path set")]
    [InlineData("standard")]
    [InlineData("full")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task JsonResultExplainsMatchedWorkingPaths(string detail)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateConditioned();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            [
                "route", "inspect", "root",
                "--workspace", working.Path,
                "--format=json", $"--detail={detail}",
                "--for", "src/../src/Order.cs",
                "--for", "src/Order.cs",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");
        Assert.Equal(["src/Order.cs"], data.GetProperty("workingPaths").EnumerateArray().Select(path => path.GetString()));
        var applicability = data.GetProperty("applicability");
        Assert.Equal("matched", applicability.GetProperty("state").GetString());
        var condition = Assert.Single(applicability.GetProperty("conditions").EnumerateArray());
        Assert.Equal(".agents/root/_root.md", condition.GetProperty("source").GetString());
        Assert.Equal(["src/**"], condition.GetProperty("patterns").EnumerateArray().Select(pattern => pattern.GetString()));
        Assert.Equal(["src/Order.cs"], applicability.GetProperty("matchingPaths").EnumerateArray().Select(path => path.GetString()));
        var readReasons = data.GetProperty("read").GetProperty("automaticallyWhen")
            .EnumerateArray().Select(reason => reason.GetString()).ToArray();
        Assert.Equal(["the Loader is read"], readReasons);

        if (detail == "standard")
        {
            var minimal = await PublishedProcessTestSupport.RunWithoutWritesAsync(
                target,
                working.Path,
                working.SnapshotHashes,
                [
                    "route", "inspect", "root",
                    "--workspace", working.Path,
                    "--format=json", "--detail=minimal",
                    "--for", "src/Order.cs",
                ]);
            Assert.Equal(0, minimal.ExitCode);
            using var minimalDocument = JsonDocument.Parse(minimal.StandardOutput);
            var minimalData = minimalDocument.RootElement.GetProperty("data");
            Assert.False(minimalData.TryGetProperty("applicability", out _));
            Assert.Equal(["src/Order.cs"], minimalData.GetProperty("workingPaths").EnumerateArray().Select(path => path.GetString()));
        }
    }

    [Fact(DisplayName = "Published Route Inspect exact-path attention status uses stdout without an invented next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task ExactPathAttentionStatusHasNoInventedNext()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", ".agents/root/collision.md", "--detail=minimal"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("root/collision  .agents/root/collision.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published redirected Route Inspect blocks a human collision without prompting"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task BlockedCollisionUsesExactNextWording()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root/collision", "--detail=minimal"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains(
            "Cannot inspect root/collision: root/collision matches more than one source. Use the exact path.",
            result.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect \".agents/root/collision.md\"", result.StandardError, StringComparison.Ordinal);
    }

}
