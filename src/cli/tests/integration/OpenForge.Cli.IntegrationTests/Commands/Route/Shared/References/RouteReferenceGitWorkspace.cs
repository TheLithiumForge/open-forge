using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.Git;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.References;

internal sealed class RouteReferenceGitWorkspace(string root) : IDisposable
{
    private readonly string _gitDirectory = Path.Combine(root, ".git");

    internal async Task InitializeAsync()
    {
        Assert.False(Directory.Exists(_gitDirectory));
        await GitAsync("init", "--quiet");
        await GitAsync("config", "core.excludesFile", Path.Combine(_gitDirectory, "global-ignore"));
        File.WriteAllText(Path.Combine(_gitDirectory, "global-ignore"), string.Empty);
    }

    internal async Task GitAsync(params string[] arguments)
    {
        var result = await new GitProcessRunner("git", Configure).RunAsync(root, arguments, TestContext.Current.CancellationToken);
        Assert.True(result is { ExitCode: 0 }, result?.Error ?? "Git did not start.");
    }

    internal void Configure(ProcessStartInfo start)
    {
        start.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(_gitDirectory, "absent-global-config");
        start.Environment["GIT_CONFIG_SYSTEM"] = Path.Combine(_gitDirectory, "absent-system-config");
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        start.Environment["GIT_CONFIG_COUNT"] = "0";
        start.Environment.Remove("GIT_CONFIG_PARAMETERS");
        start.Environment.Remove("GIT_TRACE");
    }

    internal void RenameAgentsRoot(string physicalName)
    {
        var original = Path.Combine(root, ".agents");
        var intermediate = Path.Combine(root, ".agents-renaming");
        Directory.Move(original, intermediate);
        try
        {
            Directory.Move(intermediate, Path.Combine(root, physicalName));
        }
        catch
        {
            Directory.Move(intermediate, original);
            throw;
        }
    }

    internal async Task CreateDirectoryLinkAsync(string link, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(link, target);
            return;
        }

        var start = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in new[] { "/c", "mklink", "/J", link, target })
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Junction creation did not start.");
        var output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var error = process.StandardError.ReadToEndAsync(CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            Assert.True(process.ExitCode == 0, await error);
            await output;
        }
        finally
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (process.HasExited)
                {
                    // Junction creation exited between observation and cancellation cleanup.
                }
            }

            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(output, error);
        }
    }

    public void Dispose()
    {
        if (!Directory.Exists(_gitDirectory)) return;
        Assert.Null(new DirectoryInfo(_gitDirectory).LinkTarget);
        foreach (var file in Directory.EnumerateFiles(_gitDirectory, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }
        Directory.Delete(_gitDirectory, recursive: true);
    }
}
