using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal static class PublishedJourneyProcess
{
    internal static async Task<ProcessRunResult> RunAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        WriteCommand(workingDirectory, arguments);
        var result = await PublishedProcessTestSupport.RunAsync(
            target, workingDirectory, arguments, environmentVariables);
        WriteResult(result);
        return result;
    }

    internal static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        Func<IReadOnlyDictionary<string, string>> snapshot,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        WriteCommand(workingDirectory, arguments);
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, workingDirectory, snapshot, arguments, environmentVariables);
        WriteResult(result);
        return result;
    }

    private static void WriteCommand(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var display = string.Join(" ", arguments.Select(argument =>
            "\"" + argument.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal) + "\""));
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Command: {display}\nWorking directory: {workingDirectory}");
    }

    private static void WriteResult(ProcessRunResult result)
        => TestContext.Current.TestOutputHelper?.WriteLine(
            $"Exit: {result.ExitCode}\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}");
}
