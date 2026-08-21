namespace OpenForge.Cli.EndToEndTests.Process;

internal sealed record CliEndToEndEnvironment(
    string ExecutablePath,
    string ExpectedVersion)
{
    internal const string ExecutablePathVariable = "OPEN_FORGE_CLI_PATH";
    internal const string ExpectedVersionVariable = "OPEN_FORGE_CLI_EXPECTED_VERSION";

    internal static CliEndToEndEnvironment ReadRequired()
    {
        var executablePath = ReadRequiredVariable(ExecutablePathVariable);
        var expectedVersion = ReadRequiredVariable(ExpectedVersionVariable);
        if (!File.Exists(executablePath))
        {
            throw new InvalidOperationException(
                $"Required CLI executable from {ExecutablePathVariable} does not exist: {executablePath}");
        }

        return new CliEndToEndEnvironment(executablePath, expectedVersion);
    }

    private static string ReadRequiredVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required end-to-end environment variable {name} is missing or empty.");
        }

        return value;
    }
}
