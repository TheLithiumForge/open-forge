using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteUpdateProcessTests
{
    [Fact(DisplayName = "Published Route Update JSON dry-run previews an update without writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
            "--dry-run",
            "--json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteUpdateWorkspace.TargetPath, result.GetProperty("target").GetProperty("path").GetString());
        Assert.NotEmpty(result.GetProperty("effects").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Update applies exact base and navigation bytes then converges"),
     Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyThenNoOpIsOneExactJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var targetBefore = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentBefore = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
        ];

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        var targetAfter = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentAfter = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        Assert.Equal(
            targetBefore.Replace("Before overview", "After overview", StringComparison.Ordinal)
                .Replace("[Before, Memory]", "[After, Memory]", StringComparison.Ordinal),
            targetAfter);
        Assert.Equal(
            parentBefore.Replace(
                "- [Before overview](overview.md) - #Before #Memory",
                "- [After overview](overview.md) - #After #Memory",
                StringComparison.Ordinal),
            parentAfter);
        workspace.AssertPersistentExternalLock();
        var after = workspace.SnapshotState();

        var noOp = await RunWithoutWritesAsync(target, workspace, arguments);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(after, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published Route Update refuses an ambiguous target without writes"), Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task AmbiguousTargetRefusesWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        workspace.SeedAmbiguousTarget();
        var result = await RunWithoutWritesAsync(target, workspace,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId, "--description", PublishedRouteUpdateWorkspace.ExpectedDescription, "--dry-run"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteUpdateWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
