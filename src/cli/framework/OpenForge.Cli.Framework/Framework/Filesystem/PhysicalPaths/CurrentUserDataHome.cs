namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

/// <summary>
/// Resolves the directory that holds this user's Open Forge machine state: workspace locks and
/// recovery bundles. Both stores ask the same question, so it is answered once here.
/// </summary>
internal static class CurrentUserDataHome
{
    /// <summary>
    /// Names an absolute directory to use instead of the signed-in user's local application data.
    /// It keeps machine state out of the user profile for an isolated test run, a portable
    /// installation, or a build agent. A value that is not absolute leaves the data home
    /// unavailable rather than silently writing to the profile.
    /// </summary>
    internal const string OverrideVariable = "OPENFORGE_DATA_HOME";

    internal static string? Resolve(Environment.SpecialFolderOption option)
    {
        var configured = Environment.GetEnvironmentVariable(OverrideVariable);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return FullyQualified(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                option));
        }

        var overridden = FullyQualified(configured);
        if (overridden is not null && option == Environment.SpecialFolderOption.Create)
        {
            Directory.CreateDirectory(overridden);
        }

        return overridden;
    }

    private static string? FullyQualified(string? path)
        => string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)
            ? null
            : Path.GetFullPath(path);
}
