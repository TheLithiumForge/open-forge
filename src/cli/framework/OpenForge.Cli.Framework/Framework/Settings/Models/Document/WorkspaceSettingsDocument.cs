using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Document;

/// <summary>
/// What a workspace's authored settings say. Absent settings are these defaults,
/// so a workspace without the file behaves exactly as one with an empty object.
/// </summary>
internal sealed record WorkspaceSettingsDocument(
    int SchemaVersion,
    ImmutableArray<string> AllowInstallPaths,
    ImmutableArray<string> RemovedCategories)
{
    internal static readonly WorkspaceSettingsDocument Empty =
        new(WorkspaceSettingsDefinitions.SchemaVersion, [], []);

    /// <summary>
    /// Whether this release understands the declared shape. A file that declares
    /// another one is still read for the keys this release knows; the answer is
    /// reported, never enforced.
    /// </summary>
    internal bool IsKnownSchemaVersion => SchemaVersion == WorkspaceSettingsDefinitions.SchemaVersion;
}

internal sealed record WorkspaceSettingsDecode(
    WorkspaceSettingsDocument? Document,
    string? Cause);
