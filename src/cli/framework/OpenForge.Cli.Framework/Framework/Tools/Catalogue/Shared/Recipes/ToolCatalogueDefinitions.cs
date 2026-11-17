namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;

internal static class ToolCatalogueDefinitions
{
    // First shipping build approved by the maintainer on 14 November 2026.
    internal const string MinimumOpenForgeVersion = "0.9.0-beta.19";
    internal const string NpmChannelId = "npm";
    internal const string GitHubChannelId = "github-release";
    internal const string NodeRuntime = "node";
    internal const string VersionArgument = "--version";
    internal const string InstallCommand = "install";
    internal const string UninstallCommand = "uninstall";
    internal const string YesArgument = "--yes";
    internal const string DryRunArgument = "--dry-run";
    internal const string ArchiveChangeId = "archived-memory";
    internal static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(15);
    internal const int ProbeMaximumBytes = 16 * 1024;
    internal static readonly TimeSpan SetupTimeout = TimeSpan.FromMinutes(2);
    internal const int SetupMaximumBytes = 1024 * 1024;
}
