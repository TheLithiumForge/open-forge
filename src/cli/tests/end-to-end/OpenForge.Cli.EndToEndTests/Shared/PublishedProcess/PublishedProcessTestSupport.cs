namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

internal static class PublishedProcessTestSupport
{
    internal static Task<ProcessRunResult> RunAsync(
        PublishedExecutableEnvironment environment,
        string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        return ProcessRunner.RunAsync(
            new ProcessRunRequest(
                environment.ExecutablePath,
                arguments,
                workingDirectory,
                timeout: TimeSpan.FromSeconds(30)),
            TestContext.Current.CancellationToken);
    }

    internal static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableEnvironment environment,
        string workingDirectory,
        Func<IReadOnlyDictionary<string, string>> snapshot,
        IReadOnlyList<string> arguments)
    {
        var before = snapshot();
        var result = await RunAsync(environment, workingDirectory, arguments);
        Assert.Equal(before, snapshot());
        return result;
    }
}
