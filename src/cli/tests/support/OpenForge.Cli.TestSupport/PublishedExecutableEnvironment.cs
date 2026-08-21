namespace OpenForge.Cli.TestSupport;

/// <summary>
/// Identifies the published executable and informational version used by
/// end-to-end evidence.
/// </summary>
public sealed class PublishedExecutableEnvironment
{
    /// <summary>
    /// Names the environment variable containing the published executable path.
    /// </summary>
    public const string ExecutablePathVariable = "OPEN_FORGE_CLI_PATH";

    /// <summary>
    /// Names the environment variable containing the expected informational version.
    /// </summary>
    public const string ExpectedVersionVariable = "OPEN_FORGE_CLI_EXPECTED_VERSION";

    /// <summary>
    /// Creates a published-executable environment from explicit values.
    /// </summary>
    public PublishedExecutableEnvironment(string executablePath, string expectedVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);
        if (!File.Exists(executablePath))
        {
            throw new InvalidOperationException(
                $"The published executable does not exist: {executablePath}");
        }

        ExecutablePath = executablePath;
        ExpectedVersion = expectedVersion;
    }

    /// <summary>
    /// Gets the published executable path.
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// Gets the expected informational version.
    /// </summary>
    public string ExpectedVersion { get; }

    /// <summary>
    /// Reads the required published-executable variables from the process
    /// environment and reports which value is missing.
    /// </summary>
    public static PublishedExecutableEnvironment ReadRequired()
    {
        var executablePath = ReadRequiredVariable(ExecutablePathVariable);
        var expectedVersion = ReadRequiredVariable(ExpectedVersionVariable);
        return new PublishedExecutableEnvironment(executablePath, expectedVersion);
    }

    /// <summary>
    /// Reads the required published-executable variables from an explicit map.
    /// This overload keeps tests independent from ambient process environment.
    /// </summary>
    public static PublishedExecutableEnvironment ReadRequired(
        IReadOnlyDictionary<string, string?> environmentVariables)
    {
        ArgumentNullException.ThrowIfNull(environmentVariables);
        var executablePath = ReadRequiredVariable(environmentVariables, ExecutablePathVariable);
        var expectedVersion = ReadRequiredVariable(environmentVariables, ExpectedVersionVariable);
        return new PublishedExecutableEnvironment(executablePath, expectedVersion);
    }

    private static string ReadRequiredVariable(string name)
    {
        return ReadRequiredValue(name, Environment.GetEnvironmentVariable(name));
    }

    private static string ReadRequiredVariable(
        IReadOnlyDictionary<string, string?> environmentVariables,
        string name)
    {
        foreach (var pair in environmentVariables)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return ReadRequiredValue(name, pair.Value);
            }
        }

        return ReadRequiredValue(name, null);
    }

    private static string ReadRequiredValue(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required published-executable environment variable '{name}' is missing or empty.");
        }

        return value;
    }
}
