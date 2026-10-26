using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.TestSupport.Isolation;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

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
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
        workspace.WriteText(RemovalPath, "Keep these bytes after declining\n");
        var runtime = TestDataHome.LocalRuntimeDirectory(workspace.Path);
        File.Delete(TestDataHome.LocalWorkspaceLockPath(workspace.Path));
        File.Delete(TestDataHome.LocalRuntimeIgnorePath(workspace.Path));
        Directory.Delete(runtime, recursive: false);
        var before = workspace.SnapshotState(includeRuntime: true);
        var creations = 0;
        using var watcher = new FileSystemWatcher(workspace.Path) { NotifyFilter = NotifyFilters.DirectoryName };
        watcher.Created += (_, change) =>
        {
            if (change.FullPath == runtime)
            {
                Interlocked.Increment(ref creations);
            }
        };
        watcher.EnableRaisingEvents = true;
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment) { ["TERM"] = "dumb" };
        var result = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path,
            [Tokens.Remove.Command, RemovalPath], environment,
            new PublishedTerminalScenario { Steps = [new("Apply these changes, including deleting 1 file? [y/N]", "n\r")] });
        Assert.Equal(130, result.ExitCode);

        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
        Assert.Equal(0, Volatile.Read(ref creations));
    }
}
