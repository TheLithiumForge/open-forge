using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Document;

/// <summary>
/// What a workspace's authored settings say. Absent settings are these defaults,
/// so a workspace without the file behaves exactly as one with an empty object.
/// </summary>
internal sealed record WorkspaceSettingsDocument(
    int SchemaVersion,
    ImmutableArray<string> AllowInstallPaths,
    ImmutableArray<string> RemovedCategories,
    ImmutableArray<string> RemovedFiles)
{
    internal string? DeclaredMainRoot { get; init; }

    internal string MainRoot => DeclaredMainRoot ?? WorkspaceLayout.DefaultMainRoot;

    internal FrontmatterForm? DeclaredFrontmatter { get; init; }

    internal FrontmatterForm Frontmatter => DeclaredFrontmatter ?? FrontmatterForm.Scoped;

    internal ImmutableArray<string> IgnoreLinksFrom { get; init; } = [];

    internal ImmutableArray<string> IgnoreLinksTo { get; init; } = [];

    internal bool HasIgnoreLinksFrom { get; init; }

    internal ImmutableArray<string> RemovedDirectories { get; init; } = [];

    internal ImmutableArray<string> RemovedExtensions { get; init; } = [];

    internal ImmutableArray<string> RemovedLibraries { get; init; } = [];

    /// <summary>
    /// Files outside the content locations that `route include` registered. The loader's Entries decide what
    /// loads, so a path here without a loader line is registered and switched off.
    /// </summary>
    internal ImmutableArray<string> Roots { get; init; } = [];

    internal static readonly WorkspaceSettingsDocument Empty =
        new(WorkspaceSettingsDefinitions.SchemaVersion, [], [], []);

    /// <summary>
    /// Whether this release understands the declared shape. A file that declares
    /// another one is still read for the keys this release knows. the answer is
    /// reported, never enforced.
    /// </summary>
    internal bool IsKnownSchemaVersion => SchemaVersion is >= 1 and <= WorkspaceSettingsDefinitions.SchemaVersion;
}

internal sealed record WorkspaceSettingsDecode(
    WorkspaceSettingsDocument? Document,
    string? Cause);
