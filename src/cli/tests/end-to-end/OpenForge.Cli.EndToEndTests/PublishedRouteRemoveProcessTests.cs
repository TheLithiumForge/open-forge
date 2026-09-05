using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteRemoveProcessTests
{
    [Fact(DisplayName = "Published Route Remove help and invalid input remain write-free and truthful")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
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
        Assert.DoesNotContain("--force", help.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--recursive", help.StandardOutput, StringComparison.Ordinal);
        AssertExitMapping(help.StandardOutput, "complete", 0, "stdout");
        AssertExitMapping(help.StandardOutput, "attention", 2, "stdout");
        AssertExitMapping(help.StandardOutput, "incomplete", 3, "stdout");
        AssertExitMapping(help.StandardOutput, "invalid", 4, "stderr");
        AssertExitMapping(help.StandardOutput, "blocked", 5, "stderr");
        AssertExitMapping(help.StandardOutput, "failed", 1, "stderr");
        AssertExitMapping(help.StandardOutput, "interrupted", 130, "stderr");
        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("route-remove.invalid-input", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route remove --help", invalid.StandardError, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Remove previews then applies one leaf with label-preserving detachment")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
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
                "route", "remove", PublishedRouteRemoveWorkspace.SourceId,
                "--dry-run", "--json",
            ]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        var previewRoot = previewDocument.RootElement;
        Assert.Equal("route remove", previewRoot.GetProperty("command").GetString());
        Assert.Equal("complete", previewRoot.GetProperty("status").GetString());
        Assert.Equal(
            "dry-run",
            previewRoot.GetProperty("result").GetProperty("mode").GetString());
        Assert.Equal(
            "leaf",
            previewRoot.GetProperty("result").GetProperty("subject").GetProperty("kind").GetString());
        Assert.Contains(
            previewRoot.GetProperty("result").GetProperty("references")
                .GetProperty("detachments").EnumerateArray(),
            detachment => detachment.GetProperty("visibleLabel").GetString() == "Old guide");
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoLockInfrastructure();

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["route", "remove", PublishedRouteRemoveWorkspace.SourceId],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.SourcePath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.SourceOverwritePath)));
        Assert.Equal("Prefix Old guide suffix.\n", workspace.ReadText("README.md"));
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(PublishedRouteRemoveWorkspace.LifecyclePath));
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Remove applies a category projection and repeats as an honest verified no-op")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task CategoryApplicationProjectionAndRepeatAreStable()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteRemoveWorkspace.CreateAsync(target);
        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["route", "remove", PublishedRouteRemoveWorkspace.CategoryId, "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        var appliedRoot = appliedDocument.RootElement;
        Assert.Equal("complete", appliedRoot.GetProperty("status").GetString());
        var result = appliedRoot.GetProperty("result");
        Assert.Equal("category", result.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Contains(
            result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("reasons").EnumerateArray()
                .Any(reason => reason.GetString() == "old-parent"));
        Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.CategoryPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.CategoryChildPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.CategoryNotesPath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteRemoveWorkspace.CategoryResourcePath)));
        Assert.DoesNotContain("Topics", workspace.ReadText(PublishedRouteRemoveWorkspace.ParentPath), StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();

        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", PublishedRouteRemoveWorkspace.CategoryId, "--json"]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var repeatedDocument = JsonDocument.Parse(repeated.StandardOutput);
        var repeatedResult = repeatedDocument.RootElement.GetProperty("result");
        Assert.Equal("complete", repeatedDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal("verified", repeatedResult.GetProperty("verification").GetString());
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
        Assert.Empty(repeatedResult.GetProperty("subject").GetProperty("items").EnumerateArray());
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
            $"{status}: {exit} ({stream})",
            help,
            StringComparison.Ordinal);
}
