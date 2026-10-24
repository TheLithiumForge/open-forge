using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Planning;

[Trait("Feature", "workspace-removals"), Trait("Evidence", "Unit")]
public sealed class WorkspaceRemovalsTests
{
    private const string CustomRoot = "docs/forge";
    private const string Category = "notes";
    private const string OtherCategory = "maps";
    private const string TemplateCategory = "templates";
    private const string Note = "a.md";
    private const string LiteralFile = WorkspaceLayout.DefaultMainRoot + "/" + OtherCategory + "/" + Note;

    [Fact(DisplayName = "Category exclusions follow the selected layout while literal exclusions stay fixed"), Trait("Boundary", "Selection")]
    public void UsesLayoutForCategories()
    {
        var layout = WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), CustomRoot);
        var settings = WorkspaceSettingsDocument.Empty with
        {
            RemovedCategories = [Category, WorkspaceLayoutNames.SkillsDirectoryName],
            RemovedFiles = [LiteralFile],
        };
        Assert.True(WorkspaceRemovals.IsPathRemoved(layout, CustomRoot + "/" + Category + "/" + Note, settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(layout, WorkspaceLayout.NativeSkillsPath + "/" + Note, settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(layout, LiteralFile, settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(layout, WorkspaceLayout.DefaultMainRoot + "/" + Category + "/" + Note, settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(layout, CustomRoot + "/" + OtherCategory + "/" + Note, settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Path exclusions match exact files and directory boundaries")]
    public void MatchesExactFilesAndDirectoryBoundaries()
    {
        var settings = WorkspaceSettingsDocument.Empty with
        {
            RemovedCategories = [TemplateCategory],
            RemovedFiles = [WorkspaceLayoutNames.HumanGuideFileName, "docs/readme.md"],
            RemovedDirectories = ["docs/archive"],
        };

        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, WorkspaceLayout.DefaultMainRoot + "/" + TemplateCategory, settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, WorkspaceLayout.DefaultMainRoot + "/" + TemplateCategory + "/" + Note, settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, WorkspaceLayout.DefaultMainRoot + "/" + TemplateCategory + "-old/" + Note, settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, WorkspaceLayoutNames.HumanGuideFileName, settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, WorkspaceLayoutNames.HumanGuideFileName + ".copy", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, "docs/readme.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, "docs/readme.md/child", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, "docs/archive", settings));
        Assert.True(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, "docs/archive/future.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, "docs/archived/future.md", settings));
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

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(WorkspaceRemovalsTests)),
                Path.GetFullPath(nameof(WorkspaceRemovalsTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
