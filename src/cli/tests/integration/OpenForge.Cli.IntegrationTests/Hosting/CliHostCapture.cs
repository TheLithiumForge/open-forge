using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Hosting;

namespace OpenForge.Cli.IntegrationTests.Hosting;

internal static class CliHostCapture
{
    internal static async Task<CliHostCaptureResult> RunAsync(
        string[] arguments,
        string currentDirectory)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var exitCode = await CliHost.RunAsync(
            arguments,
            currentDirectory,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new CliHostCaptureResult(exitCode, standardOutput.ToString(), standardError.ToString());
    }
}

internal sealed record CliHostCaptureResult(int ExitCode, string Output, string Error);
