using System.Collections.Concurrent;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    private const string Feature = "workspace-runtime-isolation";
    private const string Runtime = ".open-forge-state";

    [Fact(DisplayName = "First Install writes the runtime ignore before the sentinel and preserves the reviewed plan")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task FirstInstallWritesIgnoreBeforeSentinel()
    {
        using var workspace = PublishedJourneyWorkspace.Create("runtime-first-install");
        workspace.ExpectCoreInstall();
        var before = workspace.SnapshotState(includeRuntime: true);
        var preview = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic", "--dry-run"));
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(workspace.Combine(Runtime)));

        var runtime = Directory.CreateDirectory(workspace.Combine(Runtime));
        var previewWithEmptyRuntime = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic", "--dry-run"));
        Assert.Equal(0, previewWithEmptyRuntime.ExitCode);
        Assert.Equal(preview.StandardOutput, previewWithEmptyRuntime.StandardOutput);
        Assert.Empty(Directory.EnumerateFileSystemEntries(runtime.FullName));
        var created = new ConcurrentQueue<string>();
        var sentinelSeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var watcher = new FileSystemWatcher(runtime.FullName)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
        };
        watcher.Created += (_, change) =>
        {
            created.Enqueue(Path.GetRelativePath(runtime.FullName, change.FullPath));
            if (change.Name == "workspace.lock")
            {
                sentinelSeen.TrySetResult();
            }
        };
        watcher.Error += (_, error) => sentinelSeen.TrySetException(error.GetException());
        watcher.EnableRaisingEvents = true;
        var installed = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"));
        Assert.Equal(0, installed.ExitCode);
        await sentinelSeen.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        watcher.EnableRaisingEvents = false;
        Assert.Equal(".gitignore", created.First());
        Assert.Equal("*\n", File.ReadAllText(workspace.Combine(Runtime, ".gitignore")));
        Assert.Equal([".gitignore", "workspace.lock"], Directory.EnumerateFiles(runtime.FullName).Select(Path.GetFileName).Order(StringComparer.Ordinal));
        Assert.True(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.Equal(0, (await workspace.RunAsync("install", "--automatic")).ExitCode);
    }

    [Fact(DisplayName = "Dry runs, prompt-required refusals, verified no-ops and observers create no runtime state")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task NonWritingCommandsCreateNothing()
    {
        using var workspace = PublishedJourneyWorkspace.Create("runtime-read-only");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"))).ExitCode);
        var runtime = workspace.Combine(Runtime);
        File.Delete(Path.Combine(runtime, "workspace.lock"));
        File.Delete(Path.Combine(runtime, ".gitignore"));
        Directory.Delete(runtime, recursive: false);
        var before = workspace.SnapshotState(includeRuntime: true);
        string[][] readOnlyCommands =
        [
            ["status"],
            ["doctor"],
            ["cleanup"],
            ["cleanup", "--dry-run"],
            ["index", "--dry-run"],
            ["index"],
            ["install", "--automatic"],
        ];
        foreach (var command in readOnlyCommands)
        {
            var result = await workspace.RunAsync(command);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
            Assert.False(Directory.Exists(runtime));
        }

        workspace.WriteText("remove-me.md", OpenForgeDocumentSeed.Metadata(description: "Removal choice", tags: ["Guidance"], body: "\n# Removal choice\n"));
        var pending = workspace.SnapshotState(includeRuntime: true);
        var refusal = await workspace.RunAsync("remove", "remove-me.md");
        Assert.Equal(4, refusal.ExitCode);
        Assert.Equal(pending, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
        var invalid = await workspace.RunAsync("route", "create", "guidance/bad", "--tag", "Guidance", "--automatic");
        Assert.NotEqual(0, invalid.ExitCode);
        Assert.Equal(pending, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
    }
}
