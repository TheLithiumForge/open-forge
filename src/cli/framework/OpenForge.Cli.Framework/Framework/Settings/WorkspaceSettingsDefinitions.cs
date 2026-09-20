namespace OpenForge.Cli.Core.Framework.Settings;

/// <summary>
/// The authored workspace settings file. A person writes and reviews this one;
/// its generated counterpart is the lock file, and the npm-shaped pair of names
/// is what tells a reader which is which.
/// </summary>
internal static class WorkspaceSettingsDefinitions
{
    internal const string DirectoryName = ".agents";
    internal const string ImplicitPathPrefix = DirectoryName + "/";
    internal const string FileName = "open-forge.json";
    internal const string RelativePath = DirectoryName + "/" + FileName;

    /// <summary>
    /// The shape this release writes. A file carrying a different number, or none
    /// at all, is still read for the keys this release understands: the version is
    /// a fact to report, never a reason to refuse. The file is hand-edited and
    /// gains keys over releases, so a version that could refuse one would make a
    /// newer file unusable by an older CLI for no safety gained.
    /// </summary>
    internal const int SchemaVersion = 1;

    /// <summary>
    /// The schema an editor uses for completion and validation. The CLI never
    /// fetches it; it is written so the file documents itself in the tools people
    /// already have open.
    /// </summary>
    internal const string SchemaUrl =
        "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v1/open-forge.schema.json";

    internal const string SchemaProperty = "$schema";
    internal const string SchemaVersionProperty = "schemaVersion";
    internal const string AllowInstallPathsProperty = "allowInstallPaths";
    internal const string RemovedCategoriesProperty = "removedCategories";
}
