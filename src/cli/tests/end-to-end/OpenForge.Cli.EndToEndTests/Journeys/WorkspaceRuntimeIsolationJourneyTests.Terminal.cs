using System.Diagnostics;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    [Fact(DisplayName = "A live removal prompt and a human decline create no runtime state")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task PromptAndDeclineCreateNothing()
    {
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
        if (OperatingSystem.IsWindows())
        {
            var result = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path,
                ["remove", "remove-me.md"], environment,
                new PublishedTerminalScenario { Steps = [new("Apply these changes, including deleting 1 file? [y/N]", "n\r")] });
            Assert.Equal(130, result.ExitCode);
        }
        else
        {
            await DeclineUnixPromptAsync(workspace, environment);
        }

        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
        Assert.Equal(0, Volatile.Read(ref creations));
    }

    private static async Task DeclineUnixPromptAsync(PublishedJourneyWorkspace workspace, IReadOnlyDictionary<string, string> environment)
    {
        var start = new ProcessStartInfo("script")
        {
            WorkingDirectory = workspace.Path,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var item in environment)
        {
            start.Environment[item.Key] = item.Value;
        }

        var command = "'" + workspace.Target.ExecutablePath.Replace("'", "'\\''", StringComparison.Ordinal) + "' remove remove-me.md\n"
            + "result=$?\nprintf '\nOPENFORGE_PROMPT_EXIT=%s\n' \"$result\"\nexit \"$result\"";
        start.ArgumentList.Add("-q");
        if (OperatingSystem.IsLinux())
        {
            start.ArgumentList.Add("-e");
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add(command);
            start.ArgumentList.Add("/dev/null");
        }
        else
        {
            start.ArgumentList.Add("/dev/null");
            start.ArgumentList.Add("/bin/sh");
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add(command);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("The Unix terminal recorder did not start.");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(60));
        var error = process.StandardError.ReadToEndAsync(CancellationToken.None);
        var transcript = new StringBuilder();
        var buffer = new char[1];
        try
        {
            while (!transcript.ToString().Contains("Apply these changes, including deleting 1 file? [y/N]", StringComparison.Ordinal))
            {
                var count = await process.StandardOutput.ReadAsync(buffer.AsMemory(), deadline.Token);
                Assert.True(count > 0, transcript.ToString());
                transcript.Append(buffer[0]);
            }

            Assert.False(Directory.Exists(workspace.Combine(Runtime)));
            await process.StandardInput.WriteLineAsync("n");
            await process.StandardInput.FlushAsync(deadline.Token);
            var remainder = process.StandardOutput.ReadToEndAsync(deadline.Token);
            await process.WaitForExitAsync(deadline.Token);
            transcript.Append(await remainder);
            Assert.Contains("OPENFORGE_PROMPT_EXIT=130", transcript.ToString(), StringComparison.Ordinal);
            Assert.Empty(await error);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}
