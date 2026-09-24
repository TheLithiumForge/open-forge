using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Planning;

[Trait("Feature", "workspace-removals"), Trait("Evidence", "Unit")]
public sealed class WorkspaceRemovalsTests
{
    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Path exclusions match exact files and directory boundaries")]
    public void MatchesExactFilesAndDirectoryBoundaries()
    {
        var settings = WorkspaceSettingsDocument.Empty with
        {
            RemovedCategories = ["templates"],
            RemovedFiles = ["README.md", "docs/readme.md"],
            RemovedDirectories = ["docs/archive"],
        };

        Assert.True(WorkspaceRemovals.IsPathRemoved(".agents/templates", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(".agents/templates/one.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(".agents/templates-old/one.md", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved("README.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved("README.md.copy", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved("docs/readme.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved("docs/readme.md/child", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved("docs/archive", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved("docs/archive/future.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved("docs/archived/future.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Stable ID exclusions match by exact ordinal identity")]
    public void MatchesExtensionAndLibraryIdsExactly()
    {
        var settings = WorkspaceSettingsDocument.Empty with
        {
            RemovedExtensions = ["planning"],
            RemovedLibraries = ["team-knowledge"],
        };

        Assert.True(WorkspaceRemovals.IsExtensionRemoved("planning", settings));
        Assert.False(WorkspaceRemovals.IsExtensionRemoved("planning-tools", settings));
        Assert.True(WorkspaceRemovals.IsLibraryRemoved("team-knowledge", settings));
        Assert.False(WorkspaceRemovals.IsLibraryRemoved("team", settings));
    }
}
