using OpenForge.Cli.EndToEndTests.Process;

namespace OpenForge.Cli.EndToEndTests;

public sealed class CliProcessTests
{
    [Fact(DisplayName = "Published CLI root help succeeds on stdout"), Trait("Feature", "cli-shell"), Trait("Evidence", "EndToEnd")]
    public async Task HelpSucceedsOnStdout()
    {
        var result = await RunCliAsync("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("--help", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--version", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI root version succeeds on stdout"), Trait("Feature", "cli-shell"), Trait("Evidence", "EndToEnd")]
    public async Task VersionSucceedsOnStdout()
    {
        var result = await RunCliAsync("--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliEndToEndEnvironment.ReadRequired().ExpectedVersion, result.Stdout.Trim());
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI invalid input exits invalid on stderr"), Trait("Feature", "cli-shell"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidInputExitsOnStderr()
    {
        var result = await RunCliAsync("unexpected");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("unexpected", result.Stderr, StringComparison.Ordinal);
    }

    private static Task<CliProcessResult> RunCliAsync(params string[] arguments)
    {
        var environment = CliEndToEndEnvironment.ReadRequired();
        var request = new CliProcessRequest(
            environment.ExecutablePath,
            arguments);
        return CliProcessRunner.RunAsync(request, TestContext.Current.CancellationToken);
    }
}
