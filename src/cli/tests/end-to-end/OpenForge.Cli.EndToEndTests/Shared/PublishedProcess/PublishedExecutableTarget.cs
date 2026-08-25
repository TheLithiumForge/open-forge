namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

/// <summary>
/// Identifies the build-selected published executable used by end-to-end evidence.
/// </summary>
internal sealed class PublishedExecutableTarget
{
    private const string SolutionFileName = "OpenForge.Cli.slnx";
    private const string DevelopmentExecutableName = "open-forge-dev";
    private const string DevelopmentVersionFileName = "open-forge-dev.version";
    private const string NativeExecutableName = "OpenForge.Cli";
    private const string NativeVersionFileName = "OpenForge.Cli.version";

    private PublishedExecutableTarget(string executablePath, string expectedVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);
        if (!File.Exists(executablePath))
        {
            throw new InvalidOperationException(
                $"The build-selected published executable does not exist: {executablePath}");
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
    /// Resolves the development publication selected by an ordinary build or the
    /// native publication selected by an explicit build runtime identifier.
    /// </summary>
    public static PublishedExecutableTarget Discover()
    {
        var repositoryRoot = FindRepositoryRoot();
        var expectedVersion = ReadRequiredBuildValue(
            nameof(PublishedExecutableBuildTarget.ExpectedVersion),
            PublishedExecutableBuildTarget.ExpectedVersion);
        var runtimeIdentifier = PublishedExecutableBuildTarget.RuntimeIdentifier;

        return string.IsNullOrEmpty(runtimeIdentifier)
            ? DiscoverDevelopmentPublication(
                repositoryRoot,
                ReadRequiredBuildValue(
                    nameof(PublishedExecutableBuildTarget.Configuration),
                    PublishedExecutableBuildTarget.Configuration),
                expectedVersion)
            : DiscoverNativePublication(repositoryRoot, runtimeIdentifier, expectedVersion);
    }

    private static PublishedExecutableTarget DiscoverDevelopmentPublication(
        string repositoryRoot,
        string configuration,
        string expectedVersion)
    {
        var publicationDirectory = Path.Combine(
            repositoryRoot,
            "artifacts",
            "publish",
            DevelopmentExecutableName,
            configuration);
        var executableFileName = OperatingSystem.IsWindows()
            ? $"{DevelopmentExecutableName}.exe"
            : DevelopmentExecutableName;

        ValidateVersionMarker(
            Path.Combine(publicationDirectory, DevelopmentVersionFileName),
            expectedVersion);

        return new PublishedExecutableTarget(
            Path.Combine(publicationDirectory, executableFileName),
            expectedVersion);
    }

    private static PublishedExecutableTarget DiscoverNativePublication(
        string repositoryRoot,
        string runtimeIdentifier,
        string expectedVersion)
    {
        ValidateRuntimeIdentifier(runtimeIdentifier);
        var publicationDirectory = Path.Combine(
            repositoryRoot,
            "artifacts",
            "publish",
            runtimeIdentifier,
            "open-forge");
        var executableFileName = runtimeIdentifier.StartsWith("win-", StringComparison.Ordinal)
            ? $"{NativeExecutableName}.exe"
            : NativeExecutableName;

        ValidateVersionMarker(
            Path.Combine(publicationDirectory, NativeVersionFileName),
            expectedVersion);

        return new PublishedExecutableTarget(
            Path.Combine(publicationDirectory, executableFileName),
            expectedVersion);
    }

    private static void ValidateRuntimeIdentifier(string runtimeIdentifier)
    {
        if (runtimeIdentifier is not (
            "win-x64"
            or "win-arm64"
            or "linux-x64"
            or "linux-arm64"
            or "osx-x64"
            or "osx-arm64"))
        {
            throw new InvalidOperationException(
                $"The EndToEnd build selected unsupported runtime identifier '{runtimeIdentifier}'.");
        }
    }

    private static void ValidateVersionMarker(string versionPath, string expectedVersion)
    {
        if (!File.Exists(versionPath))
        {
            throw new InvalidOperationException(
                $"The build-selected publication version file does not exist: {versionPath}");
        }

        var publishedVersion = File.ReadAllText(versionPath).Trim();
        if (!string.Equals(publishedVersion, expectedVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The build-selected publication version '{publishedVersion}' does not match expected version '{expectedVersion}': {versionPath}");
        }
    }

    private static string ReadRequiredBuildValue(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"The EndToEnd build did not provide required value '{name}'.");
        }

        return value;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Unable to locate the repository root containing '{SolutionFileName}' from '{AppContext.BaseDirectory}'.");
    }
}
