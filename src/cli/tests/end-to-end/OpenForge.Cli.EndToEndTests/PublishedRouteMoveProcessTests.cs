using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.EndToEndTests.Shared.Route;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteMoveProcessTests
{
    [Fact(DisplayName = "Published outside Move preserves repeated authored rows and consumes the old exact identity"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task OutsideComposition()
    {
        using var workspace = TemporaryWorkspace.Create(nameof(OutsideComposition));
        using var locks = PublishedWorkspaceLockStore.Create(nameof(OutsideComposition));
        _ = locks.Track(workspace.Path);
        const string outside = "knowledge/team";
        const string oldName = "old";
        const string newName = "renamed";
        var anchor = outside + "/" + PublishedTokens.Layout.IndexFile;
        var source = outside + "/" + oldName + PublishedTokens.Layout.MarkdownExtension;
        var destination = outside + "/" + newName + PublishedTokens.Layout.MarkdownExtension;
        var settings = new JsonObject
        {
            [PublishedTokens.Settings.SchemaVersionProperty] = PublishedTokens.Settings.SchemaVersion,
            [PublishedTokens.Settings.MainRootProperty] = MainRoot,
            [PublishedTokens.Settings.OutsideRoutesProperty] = new JsonArray(outside),
        }.ToJsonString();
        workspace.WriteText(PublishedTokens.Settings.RelativePath, settings);
        workspace.WriteText(PublishedTokens.Ownership.RelativePath, new JsonObject
        {
            [PublishedTokens.Ownership.Framework] = new JsonObject
            {
                [PublishedTokens.Ownership.Source] = new JsonObject { [PublishedTokens.Ownership.Id] = FrameworkId },
            },
        }.ToJsonString());
        var loader = OpenForgeDocumentSeed.GeneratedEntries(string.Empty);
        workspace.WriteText(Loader, loader);
        var rows = OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], OpenForgeDocumentSeed.GeneratedEntries(
            $"- [First]({oldName}{PublishedTokens.Layout.MarkdownExtension}) - #{FixtureTag}\n" +
            $"- [Second]({oldName}{PublishedTokens.Layout.MarkdownExtension}) - #{FixtureTag} - applies to `src/**`"));
        workspace.WriteText(anchor, rows);
        var body = OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], AuthoredBody);
        workspace.WriteText(source, body);
        var arguments = MoveArguments(PublishedTokens.Layout.LiteralReferencePrefix + source, PublishedTokens.Layout.LiteralReferencePrefix + destination);
        var target = PublishedExecutableTarget.Discover();
        try
        {
            AssertMoveCompleted(await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
                [.. arguments, PublishedTokens.RouteMove.DryRun], locks.EnvironmentVariables));
            locks.AssertNoInfrastructure();
            AssertMoveCompleted(await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, locks.EnvironmentVariables));
            Assert.False(File.Exists(workspace.Combine(source)));
            Assert.Equal(Encoding.UTF8.GetBytes(body), File.ReadAllBytes(workspace.Combine(destination)));
            Assert.Equal(Encoding.UTF8.GetBytes(rows.Replace($"({oldName}{PublishedTokens.Layout.MarkdownExtension})", $"({newName}{PublishedTokens.Layout.MarkdownExtension})", StringComparison.Ordinal)), File.ReadAllBytes(workspace.Combine(anchor)));
            Assert.Equal(Encoding.UTF8.GetBytes(loader), File.ReadAllBytes(workspace.Combine(Loader)));
            Assert.Equal(Encoding.UTF8.GetBytes(settings), File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
            var repeat = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes, arguments, locks.EnvironmentVariables);
            Assert.Equal(PublishedTokens.Statuses.InvalidExitCode, repeat.ExitCode);
            using var report = JsonDocument.Parse(repeat.StandardOutput);
            Assert.Equal(PublishedTokens.Statuses.Invalid, report.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        }
        finally
        {
            if (File.Exists(workspace.Combine(destination))) File.Delete(workspace.Combine(destination));
        }
    }
    [Fact(DisplayName = "Published Route Move JSON dry-run previews the intended move without writes"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        string[] arguments =
        [
            "route", "move", PublishedRouteWorkspaceSeed.SourceId,
            PublishedRouteMoveWorkspace.DestinationPath,
            "--dry-run", "--format=json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(PublishedRouteWorkspaceSeed.SourcePath, result.GetProperty("source").GetProperty("path").GetString());
        Assert.Equal(PublishedRouteMoveWorkspace.DestinationPath, result.GetProperty("destination").GetProperty("path").GetString());
        Assert.NotEmpty(result.GetProperty("moved").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move applies a leaf overwrite references and navigation then old-source repeat is invalid"), Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task LeafApplyPreservesBytesAndConsumesTheOldIdentity()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var lifecycleBefore = workspace.ReadBytes(".agents/open-forge.lock.json");
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
        Assert.Contains("Moved ", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(PublishedRouteMoveWorkspace.DestinationPath, applied.StandardOutput, StringComparison.Ordinal);
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
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(".agents/open-forge.lock.json"));
        workspace.AssertPersistentExternalLock();

        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(4, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardOutput);
        Assert.Contains("Cannot move ", repeated.StandardError, StringComparison.Ordinal);
        Assert.Contains("No source has the ID", repeated.StandardError, StringComparison.Ordinal);
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
        Assert.Contains("Cannot move ", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("already exists.", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteMoveWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
