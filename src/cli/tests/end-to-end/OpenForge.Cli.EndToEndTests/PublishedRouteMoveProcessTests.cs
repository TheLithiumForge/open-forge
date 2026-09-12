using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.EndToEndTests.Shared.Route;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteMoveProcessTests
{
    [Fact(DisplayName = "Published Route Move JSON dry-run previews the intended move without writes"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        string[] arguments =
        [
            "route", "move", PublishedRouteWorkspaceSeed.SourceId,
            PublishedRouteMoveWorkspace.DestinationPath,
            "--dry-run", "--json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteWorkspaceSeed.SourcePath, result.GetProperty("source").GetProperty("path").GetString());
        Assert.Equal(PublishedRouteMoveWorkspace.DestinationPath, result.GetProperty("destination").GetProperty("path").GetString());
        Assert.NotEmpty(result.GetProperty("effects").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move applies a leaf overwrite references and navigation then old-source repeat is invalid"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task LeafApplyPreservesBytesAndConsumesTheOldIdentity()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var lifecycleBefore = workspace.ReadBytes(".agents/open-forge.lifecycle.json");
        string[] arguments =
        [
            "route", "move", PublishedRouteWorkspaceSeed.SourceId,
            PublishedRouteMoveWorkspace.DestinationPath,
        ];

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.SourcePath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteWorkspaceSeed.SourceOverwritePath)));
        Assert.Equal(
            PublishedRouteWorkspaceSeed.SourceText,
            workspace.ReadText(PublishedRouteMoveWorkspace.DestinationPath));
        Assert.Equal(
            PublishedRouteWorkspaceSeed.OverwriteText,
            workspace.ReadText(PublishedRouteMoveWorkspace.DestinationOverwritePath));
        Assert.Contains(
            ".agents/archive/new%20guide.md#section",
            workspace.ReadText("README.md"),
            StringComparison.Ordinal);
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(".agents/open-forge.lifecycle.json"));
        workspace.AssertPersistentExternalLock();

        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(4, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardOutput);
        Assert.Contains("route-move.source-not-found", repeated.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", repeated.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Move refuses an occupied destination without writes"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task OccupiedDestinationRefusesWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        workspace.SeedOccupiedDestination();
        var result = await RunWithoutWritesAsync(target, workspace,
            ["route", "move", PublishedRouteWorkspaceSeed.SourceId, PublishedRouteMoveWorkspace.DestinationPath]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("route-move.destination-occupied", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteMoveWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
