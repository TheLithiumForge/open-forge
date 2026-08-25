namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

internal static class PublishedProcessTestSupport
{
    internal static Task<ProcessRunResult> RunAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        return ProcessRunner.RunAsync(
            new ProcessRunRequest(
                target.ExecutablePath,
                arguments,
                workingDirectory,
                timeout: TimeSpan.FromSeconds(30)),
            TestContext.Current.CancellationToken);
    }

    internal static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        string workingDirectory,
        Func<IReadOnlyDictionary<string, string>> snapshot,
        IReadOnlyList<string> arguments)
    {
        var before = snapshot();
        var result = await RunAsync(target, workingDirectory, arguments);
        Assert.Equal(before, snapshot());
        return result;
    }
}
