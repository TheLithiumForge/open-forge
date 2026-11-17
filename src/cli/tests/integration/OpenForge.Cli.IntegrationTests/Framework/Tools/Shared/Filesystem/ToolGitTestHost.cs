using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using System.Diagnostics;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Filesystem;

// Backup and coverage tests share real Git with every user store redirected into their owned fixture.
internal sealed class ToolGitTestHost(string home) : IToolProcessHost
{
    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
    {
        var path = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator)
            .Select(directory => Path.Combine(directory, OperatingSystem.IsWindows() ? "git.exe" : ToolGitEnvironment.Executable)).FirstOrDefault(File.Exists);
        return Task.FromResult(path is null ? null : new ToolResolvedCommand(path, new() { ExecutablePath = path, PrefixArguments = [] }));
    }

    public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(request.ExecutablePath)
        {
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        foreach (var key in start.Environment.Keys.Where(key => key.StartsWith("GIT_", StringComparison.Ordinal)).ToArray()) start.Environment.Remove(key);
        start.Environment["HOME"] = home;
        start.Environment["USERPROFILE"] = home;
        start.Environment["XDG_CONFIG_HOME"] = home;
        start.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(home, "absent-config");
        start.Environment["GIT_CONFIG_SYSTEM"] = Path.Combine(home, "absent-config");
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        foreach (var (key, value) in request.Environment)
        {
            if (value is null) start.Environment.Remove(key);
            else start.Environment[key] = value;
        }
        using var process = Process.Start(start) ?? throw new IOException("The isolated Git process did not start.");
        var output = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var error = process.StandardError.ReadToEndAsync(CancellationToken.None);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            return new() { ExitCode = process.ExitCode, StandardOutput = await output, StandardError = await error, TimedOut = false, Cancelled = false };
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(output, error);
        }
    }
}
