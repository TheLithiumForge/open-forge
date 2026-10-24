using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Workspace.Models.Layout;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string OtherRoot = "outside";
    private const string Category = "notes";
    private const string Note = "a.md";
    private const string RelativeNote = Category + "/" + Note;
    private const string Skill = WorkspaceLayoutNames.SkillsDirectoryName + "/" + Note;
    private const string DefaultLoader = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string CustomLoader = CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Settings = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.SettingsFileName;
    private const string Ownership = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName;

    [Fact(DisplayName = "Default layout preserves package destinations and source coordinates")]
    public void DefaultIdentity()
    {
        var layout = Create(WorkspaceLayout.DefaultMainRoot);
        Assert.True(layout.IsDefault);
        Assert.Equal([WorkspaceLayout.DefaultMainRoot], layout.SourceRoots);
        Assert.Equal(DefaultLoader, layout.LoaderPath);
        Assert.Equal(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName, layout.HumanGuidePath);
        var category = WorkspaceLayoutNames.SkillsDirectoryName.ToUpperInvariant();
        Assert.Equal(WorkspaceLayout.DefaultMainRoot + "/" + category, layout.CategoryDirectory(category));
        foreach (var path in new[] { WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName, Settings, Ownership, DefaultLoader, WorkspaceLayout.DefaultMainRoot + "/" + Skill })
            Assert.Equal(path, layout.MapPayloadPath(path));
    }

    [Fact(DisplayName = "Nested layout maps content once and preserves native Skills and literal paths")]
    public void NestedMapping()
    {
        var layout = Create(CustomRoot);
        Assert.False(layout.IsDefault);
        Assert.Equal([CustomRoot, WorkspaceLayout.NativeSkillsPath], layout.SourceRoots);
        Assert.Equal(CustomLoader, layout.LoaderPath);
        Assert.Equal(CustomRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName, layout.HumanGuidePath);
        Assert.Equal(CustomRoot + "/" + Category, layout.CategoryDirectory(Category));
        Assert.Equal(WorkspaceLayout.NativeSkillsPath, layout.CategoryDirectory(WorkspaceLayoutNames.SkillsDirectoryName));
        Assert.Equal(WorkspaceLayout.NativeSkillsPath + "/" + Note, layout.SourcePath(Skill));
        Assert.Equal(WorkspaceLayout.NativeSkillsPath + "/" + Note, layout.SourcePath(WorkspaceLayoutNames.SkillsDirectoryName.ToUpperInvariant() + "/" + Note));
        Assert.Equal(CustomRoot + "/" + RelativeNote, layout.MapPayloadPath(WorkspaceLayout.DefaultMainRoot + "/" + RelativeNote));
        foreach (var path in new[] { WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName, Settings, Ownership, WorkspaceRuntimePaths.DirectoryName + "/" + Note, CustomLoader, OtherRoot + "/" + Note })
            Assert.Equal(path, layout.MapPayloadPath(path));
        Assert.True(layout.TryGetSourceRelativePath(CustomRoot + "/" + RelativeNote, out var relative));
        Assert.Equal(RelativeNote, relative);
        Assert.True(layout.TryGetSourceRelativePath(WorkspaceLayout.NativeSkillsPath + "/" + Note, out relative));
        Assert.Equal(Skill, relative);
        Assert.True(layout.IsImplicitContentDestination(CustomRoot.ToUpperInvariant() + "/" + RelativeNote));
        foreach (var path in new[] { CustomRoot + "-old/" + Note, CustomRoot + "/" + Skill, DefaultLoader, Settings, CustomRoot + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + Note, OtherRoot + "/" + Note })
            Assert.False(layout.IsContentPath(path));
    }

    [Fact(DisplayName = "Primary roots neither describe all source membership nor grant every descendant as a destination")]
    public void PrimaryLocationsAreNotAuthority()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, CustomRoot })
        {
            var layout = Create(root);
            Assert.Contains(root, layout.SourceRoots);
            Assert.False(layout.IsImplicitContentDestination(root + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + Note));
            Assert.DoesNotContain(OtherRoot, layout.SourceRoots);
            Assert.False(layout.IsImplicitContentDestination(OtherRoot + "/" + RelativeNote));
            Assert.False(layout.TryGetSourceRelativePath(OtherRoot + "/" + RelativeNote, out _));
            Assert.Equal(root + "/" + Category, layout.CategoryDirectory(Category));
            Assert.Equal(root + "/" + RelativeNote, layout.SourcePath(RelativeNote));
            Assert.Equal(root + "/" + RelativeNote, layout.MapPayloadPath(WorkspaceLayout.DefaultMainRoot + "/" + RelativeNote));
            Assert.Equal(OtherRoot + "/" + RelativeNote, layout.MapPayloadPath(OtherRoot + "/" + RelativeNote));
        }
        Assert.False(Create(WorkspaceLayout.DefaultMainRoot).IsImplicitContentDestination(Settings));
    }

    public static TheoryData<string?> InvalidRoots => new()
    {
        (string?)null, string.Empty, ".", "./docs", "docs/../forge", "/docs", "C:/docs", "docs\\forge", "docs/",
        WorkspaceLayout.DefaultMainRoot.ToUpperInvariant(), WorkspaceLayout.DefaultMainRoot + "/forge",
        WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/forge", WorkspaceRuntimePaths.DirectoryName,
        WorkspaceRuntimePaths.DirectoryName.ToUpperInvariant() + "/forge", "docs/" + WorkspaceLayoutNames.GitMetadataDirectoryName.ToUpperInvariant() + "/forge",
        WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName + "/forge", "docs/NUL",
    };

    [Theory(DisplayName = "Main roots reject noncanonical and reserved portable paths")]
    [MemberData(nameof(InvalidRoots))]
    public void RejectsInvalidRoots(string? path) => Assert.False(WorkspaceMainRootPath.IsCanonical(path));

    [Theory(DisplayName = "Main roots accept the default and canonical custom directories")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(CustomRoot)]
    [InlineData(".context")]
    public void AcceptsRoots(string path) => Assert.True(WorkspaceMainRootPath.IsCanonical(path));

    private static WorkspaceLayout Create(string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
}
