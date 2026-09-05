using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed record StatusIntegrationRun(
    int ExitCode,
    string StandardOutput,
    string StandardError);

internal static class StatusIntegrationApplication
{
    internal static async Task<StatusIntegrationRun> RunAsync(
        StatusIntegrationWorkspace workspace,
        params string[] arguments)
    {
        var completion = await CliHostCapture.RunAsync(arguments, workspace.Path);
        return new StatusIntegrationRun(
            completion.ExitCode,
            completion.Output,
            completion.Error);
    }

    internal static JsonDocument ParseJson(StatusIntegrationRun run)
        => JsonDocument.Parse(run.StandardOutput);
}
