using System.Diagnostics;

namespace OpenForge.Cli.SystemTests;

public sealed class CliProcessTests
{
    [Fact(DisplayName = "Native CLI help succeeds on stdout")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "EndToEnd")]
    public async Task HelpSucceedsOnStdout()
    {
        var result = await RunCliAsync("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("--help", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--version", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Native CLI version succeeds on stdout")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "EndToEnd")]
    public async Task VersionSucceedsOnStdout()
    {
        var result = await RunCliAsync("--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("0.0.0-dev", result.Stdout.Trim());
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Native CLI invalid input exits invalid on stderr")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "EndToEnd")]
    public async Task InvalidInputExitsOnStderr()
    {
        var result = await RunCliAsync("unexpected");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("unexpected", result.Stderr, StringComparison.Ordinal);
    }

    private static async Task<ProcessResult> RunCliAsync(params string[] arguments)
    {
        var executablePath = Environment.GetEnvironmentVariable("OPEN_FORGE_CLI_PATH");
        Assert.False(string.IsNullOrWhiteSpace(executablePath));
        Assert.True(File.Exists(executablePath), $"CLI executable does not exist: {executablePath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var cancellationToken = TestContext.Current.CancellationToken;
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }

            throw;
        }

        return new ProcessResult(process.ExitCode, await stdout, await stderr);
    }

    private sealed record ProcessResult(int ExitCode, string Stdout, string Stderr);
}
