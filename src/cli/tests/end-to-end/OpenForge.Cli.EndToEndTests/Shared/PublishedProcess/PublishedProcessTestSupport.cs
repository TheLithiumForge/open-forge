namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

internal static class PublishedProcessTestSupport
{
    internal static Task<ProcessRunResult> RunAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
        => RunAsync(
            executablePath: target.ExecutablePath,
            workingDirectory: workingDirectory,
            arguments: arguments,
            environmentVariables: environmentVariables ?? new Dictionary<string, string>());

    internal static Task<ProcessRunResult> RunAsync(
        string executablePath,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables)
    {
        return ProcessRunner.RunAsync(
            new ProcessRunRequest(
                executablePath: executablePath,
                arguments: arguments,
                workingDirectory: workingDirectory,
                timeout: TimeSpan.FromSeconds(30))
            {
                EnvironmentVariables = environmentVariables,
            },
            TestContext.Current.CancellationToken);
    }

    internal static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        Func<IReadOnlyDictionary<string, string>> snapshot,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        var before = snapshot();
        var result = await RunAsync(
            target,
            workingDirectory,
            arguments,
            environmentVariables);
        Assert.Equal(before, snapshot());
        return result;
    }
}
