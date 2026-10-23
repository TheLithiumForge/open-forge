using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    [Fact(DisplayName = "A live removal prompt and a human decline create no runtime state")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task PromptAndDeclineCreateNothing()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This confirmation journey requires the repository's Windows ConPTY harness.");
        using var workspace = PublishedJourneyWorkspace.Create("runtime-prompt");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"))).ExitCode);
        workspace.WriteText("remove-me.md", "Keep these bytes after declining\n");
        var runtime = workspace.Combine(Runtime);
        File.Delete(Path.Combine(runtime, "workspace.lock"));
        File.Delete(Path.Combine(runtime, ".gitignore"));
        Directory.Delete(runtime, recursive: false);
        var before = workspace.SnapshotState(includeRuntime: true);
        var creations = 0;
        using var watcher = new FileSystemWatcher(workspace.Path) { NotifyFilter = NotifyFilters.DirectoryName };
        watcher.Created += (_, change) =>
        {
            if (change.Name == Runtime)
            {
                Interlocked.Increment(ref creations);
            }
        };
        watcher.EnableRaisingEvents = true;
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment) { ["TERM"] = "dumb" };
        var result = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path,
            ["remove", "remove-me.md"], environment,
            new PublishedTerminalScenario { Steps = [new("Apply these changes, including deleting 1 file? [y/N]", "n\r")] });
        Assert.Equal(130, result.ExitCode);

        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
        Assert.Equal(0, Volatile.Read(ref creations));
    }
}
