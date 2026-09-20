using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.EndToEndTests.Shared.Route;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteRemoveProcessTests
{
    [Fact(DisplayName = "Published Route Remove help and invalid input remain write-free and truthful"), Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task HelpAndInvalidInputHaveExactPublicBoundaries()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteRemoveWorkspace.CreateAsync(target);
        var missingWorkspace = workspace.Combine("missing-workspace");
        var help = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", "--help", "--workspace", missingWorkspace]);
        var invalid = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove"]);

        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Contains(
            "open-forge route remove <source-reference>",
            help.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("--dry-run", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--automatic", help.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", help.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--recursive", help.StandardOutput, StringComparison.Ordinal);
        AssertExitMapping(help.StandardOutput, "completed", 0, "stdout");
        AssertExitMapping(help.StandardOutput, "completed-with-warnings", 2, "stdout");
        AssertExitMapping(help.StandardOutput, "incomplete", 3, "stdout");
        AssertExitMapping(help.StandardOutput, "invalid-input", 4, "stderr");
        AssertExitMapping(help.StandardOutput, "blocked", 5, "stderr");
        AssertExitMapping(help.StandardOutput, "failed", 1, "stderr");
        AssertExitMapping(help.StandardOutput, "cancelled", 130, "stderr");
        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains(
            "Cannot remove source-reference: Route Remove requires one nonblank source reference.",
            invalid.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route remove --help", invalid.StandardError, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Remove previews then applies one leaf with label-preserving detachment"), Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task LeafDryRunAndApplicationPreserveLifecycleAndSurroundingBytes()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteRemoveWorkspace.CreateAsync(target);
        var lifecycleBefore = workspace.ReadBytes(PublishedRouteRemoveWorkspace.LifecyclePath);
        var before = workspace.SnapshotState();
        var preview = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "remove", PublishedRouteWorkspaceSeed.SourceId,
                "--dry-run", "--format=json",
            ]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        var previewRoot = previewDocument.RootElement;
        Assert.Equal("route remove", previewRoot.GetProperty("command").GetString());
        Assert.Equal("completed", previewRoot.GetProperty("status").GetString());
        Assert.Equal(
            "dry-run",
            previewRoot.GetProperty("data").GetProperty("mode").GetString());
        Assert.Equal(
            "file",
            previewRoot.GetProperty("data").GetProperty("subject").GetString());
        Assert.Contains(
            previewRoot.GetProperty("data").GetProperty("detachedLinks").EnumerateArray(),
            detachment => detachment.GetProperty("path").GetString() == "README.md");
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoLockInfrastructure();

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["route", "remove", PublishedRouteWorkspaceSeed.SourceId, "--automatic"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Removed .agents/guidance/old guide.md", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Entry removed from .agents/guidance/_guidance.md", applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.SourcePath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.SourceOverwritePath)));
        Assert.Equal("Prefix Old guide suffix.\n", workspace.ReadText("README.md"));
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(PublishedRouteRemoveWorkspace.LifecyclePath));
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Remove applies a category projection and reports repeated absence without changes"), Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task CategoryApplicationProjectionAndRepeatAreStable()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteRemoveWorkspace.CreateAsync(target);
        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["route", "remove", PublishedRouteWorkspaceSeed.CategoryId, "--format=json", "--automatic"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        var appliedRoot = appliedDocument.RootElement;
        Assert.Equal("completed", appliedRoot.GetProperty("status").GetString());
        var result = appliedRoot.GetProperty("data");
        Assert.Equal("route", result.GetProperty("subject").GetString());
        Assert.Contains(
            result.GetProperty("removed").EnumerateArray(),
            path => path.GetString() == PublishedRouteWorkspaceSeed.CategoryPath);
        Assert.Contains(
            appliedRoot.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == PublishedRouteRemoveWorkspace.ParentPath
                && effect.GetProperty("action").GetString() == "rewritten");
        Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.CategoryPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.CategoryChildPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.CategoryNotesPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.CategoryResourcePath)));
        Assert.DoesNotContain("Topics", workspace.ReadText(PublishedRouteRemoveWorkspace.ParentPath), StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();

        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", PublishedRouteWorkspaceSeed.CategoryId, "--format=json"]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var repeatedDocument = JsonDocument.Parse(repeated.StandardOutput);
        var repeatedRoot = repeatedDocument.RootElement;
        Assert.Equal("completed", repeatedRoot.GetProperty("status").GetString());
        Assert.Equal(
            $"Nothing to do for {PublishedRouteWorkspaceSeed.CategoryId}.",
            repeatedRoot.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Empty(repeatedRoot.GetProperty("findings").EnumerateArray());
        Assert.Empty(repeatedRoot.GetProperty("effects").EnumerateArray());
        Assert.Empty(repeatedRoot.GetProperty("data").GetProperty("removed").EnumerateArray());

        var repeatedText = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", PublishedRouteWorkspaceSeed.CategoryId]);

        Assert.Equal(0, repeatedText.ExitCode);
        Assert.Equal(string.Empty, repeatedText.StandardError);
        Assert.Contains(
            $"Nothing to do for {PublishedRouteWorkspaceSeed.CategoryId}.",
            repeatedText.StandardOutput,
            StringComparison.Ordinal);
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteRemoveWorkspace workspace,
        IReadOnlyList<string> arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertExitMapping(
        string help,
        string status,
        int exit,
        string stream)
        => Assert.Contains(
            $"{status}: exit {exit} and text {stream}.",
            help,
            StringComparison.Ordinal);
}
