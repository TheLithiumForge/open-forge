using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

/// <summary>Primary logical locations for source path operations, without physical workspace authority.</summary>
internal readonly record struct SourceContentRoots
{
    private SourceContentRoots(string mainRoot, ImmutableArray<string> sourceRoots)
    {
        MainRoot = mainRoot;
        SourceRoots = sourceRoots;
    }

    internal SourceContentRoots(WorkspaceLayout layout) : this(layout.MainRoot, layout.SourceRoots) { }

    /// <summary>The portable source coordinates used by shipped packages, independent of any consumer workspace.</summary>
    internal static SourceContentRoots PackageCoordinates { get; } = new(WorkspaceLayout.DefaultMainRoot, [WorkspaceLayout.DefaultMainRoot]);

    internal string MainRoot { get; }
    internal ImmutableArray<string> SourceRoots { get; }
    internal bool IsDefault => MainRoot == WorkspaceLayout.DefaultMainRoot;
    internal string LoaderPath => MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
}
