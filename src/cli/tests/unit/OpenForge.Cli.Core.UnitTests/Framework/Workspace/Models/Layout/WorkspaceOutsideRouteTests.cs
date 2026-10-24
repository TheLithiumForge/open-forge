using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Workspace.Models.Layout;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceOutsideRouteTests
{
    private const string MainRoot = "context/forge";
    private const string Team = "docs/team";
    private const string Notes = "notes";
    private const string Note = "note" + WorkspaceRoutePath.MarkdownExtension;

    [Theory(DisplayName = "Outside descriptors preserve declaration order and extend only discovery roots")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(MainRoot)]
    public void OrderedDiscovery(string mainRoot)
    {
        var original = Create(mainRoot);
        var directories = new List<string> { Team, Notes };
        var layout = original.WithOutsideRoutes(directories);
        directories.Clear();

        Assert.Empty(original.OutsideRoutes);
        Assert.Equal(original.SourceRoots, original.InventoryRoots);
        Assert.Equal(original.SourceRoots, layout.SourceRoots);
        Assert.Equal([Team, Notes], layout.OutsideRoutes.Select(route => route.DirectoryPath));
        Assert.Equal([.. original.SourceRoots, Team, Notes], layout.InventoryRoots);
        Assert.Equal([Team + "/_team" + WorkspaceRoutePath.MarkdownExtension, Notes + "/_notes" + WorkspaceRoutePath.MarkdownExtension], layout.OutsideRoutes.Select(route => route.CataloguePath));
        Assert.Empty(layout.WithOutsideRoutes([]).OutsideRoutes);
        Assert.Equal([Notes], layout.WithOutsideRoutes([Notes]).OutsideRoutes.Select(route => route.DirectoryPath));
        foreach (var path in new[] { Team + "/" + Note, Notes + "/" + Note })
        {
            Assert.False(layout.TryGetSourceRelativePath(path, out _));
            Assert.False(layout.IsContentPath(path));
            Assert.False(layout.IsImplicitContentDestination(path));
            Assert.Equal(original.MapPayloadPath(path), layout.MapPayloadPath(path));
        }
        Assert.Same(original.Workspace, layout.Workspace);
        Assert.Equal(original.LoaderPath, layout.LoaderPath);
        Assert.Equal(original.HumanGuidePath, layout.HumanGuidePath);
        Assert.Equal(original.SourcePath(Notes + "/" + Note), layout.SourcePath(Notes + "/" + Note));
        Assert.Equal(original.CategoryDirectory(Notes), layout.CategoryDirectory(Notes));
        Assert.Equal(original.MapPayloadPath(WorkspaceLayout.DefaultMainRoot + "/" + Notes + "/" + Note), layout.MapPayloadPath(WorkspaceLayout.DefaultMainRoot + "/" + Notes + "/" + Note));
    }

    [Theory(DisplayName = "Maintenance lookup uses exact ordinal prefixes and the selected boundary anchor")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(MainRoot)]
    public void OrdinalBoundaries(string mainRoot)
    {
        var layout = Create(mainRoot).WithOutsideRoutes([Team]);
        AssertBoundary(layout, mainRoot, WorkspaceMaintenanceBoundaryKind.Primary, mainRoot, layout.LoaderPath);
        AssertBoundary(layout, layout.LoaderPath, WorkspaceMaintenanceBoundaryKind.Primary, mainRoot, layout.LoaderPath);
        AssertBoundary(layout, mainRoot + "/" + Notes + "/" + Note, WorkspaceMaintenanceBoundaryKind.Primary, mainRoot, layout.LoaderPath);
        AssertBoundary(layout, WorkspaceLayout.NativeSkillsPath, WorkspaceMaintenanceBoundaryKind.NativeSkills, WorkspaceLayout.NativeSkillsPath, WorkspaceRoutePath.CanonicalEntrypointPath(WorkspaceLayout.NativeSkillsPath));
        AssertBoundary(layout, WorkspaceLayout.NativeSkillsPath + "/" + Note, WorkspaceMaintenanceBoundaryKind.NativeSkills, WorkspaceLayout.NativeSkillsPath, WorkspaceRoutePath.CanonicalEntrypointPath(WorkspaceLayout.NativeSkillsPath));
        AssertBoundary(layout, Team, WorkspaceMaintenanceBoundaryKind.Outside, Team, WorkspaceRoutePath.CanonicalEntrypointPath(Team));
        AssertBoundary(layout, Team + "/" + Notes + "/" + Note, WorkspaceMaintenanceBoundaryKind.Outside, Team, WorkspaceRoutePath.CanonicalEntrypointPath(Team));
        foreach (var path in new[] { mainRoot.ToUpperInvariant(), mainRoot + "-old/" + Note, Team.ToUpperInvariant() + "/" + Note, Team + "-old/" + Note, "./" + Team, Team + "/", Notes + "/" + Note })
            Assert.False(layout.TryGetMaintenanceBoundary(path, out _));
        Assert.False(layout.TryGetMaintenanceBoundary(WorkspaceLayout.NativeSkillsPath.ToUpperInvariant() + "/" + Note, out _));
        if (!layout.IsDefault)
        {
            Assert.False(layout.TryGetMaintenanceBoundary(WorkspaceLayout.NativeSkillsPath.Replace(WorkspaceLayoutNames.SkillsDirectoryName, WorkspaceLayoutNames.SkillsDirectoryName.ToUpperInvariant(), StringComparison.Ordinal) + "/" + Note, out _));
            Assert.False(layout.TryGetMaintenanceBoundary(WorkspaceLayout.DefaultMainRoot + "/" + Note, out _));
        }
    }

    public static TheoryData<string?> InvalidDirectories => new()
    {
        (string?)null, string.Empty, ".", "./" + Team, Team + "/", "docs//team", "docs/../team", "docs\\team", "/docs", "C:/docs", "docs/NUL",
        MainRoot, MainRoot.ToUpperInvariant() + "/notes", "context", "CONTEXT", WorkspaceLayout.DefaultMainRoot,
        WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/notes", WorkspaceLayout.NativeSkillsPath,
        WorkspaceRuntimePaths.DirectoryName, WorkspaceRuntimePaths.DirectoryName.ToUpperInvariant() + "/notes",
        WorkspaceLayoutNames.GitMetadataDirectoryName, Notes + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName.ToUpperInvariant() + "/notes",
    };

    [Theory(DisplayName = "Outside routes reject noncanonical directories and reserved symmetric overlaps")]
    [MemberData(nameof(InvalidDirectories))]
    public void RejectsDirectories(string? path)
        => Assert.Throws<ArgumentException>(() => Create(MainRoot).WithOutsideRoutes([path!]));

    [Theory(DisplayName = "Outside route overlap rejects ancestors descendants duplicates and case aliases in either order")]
    [InlineData(Team, Team)]
    [InlineData(Team, "DOCS/TEAM")]
    [InlineData(Team, "docs")]
    [InlineData(Team, Team + "/notes")]
    [InlineData(Team, "DOCS/TEAM/notes")]
    public void RejectsOverlap(string first, string second)
    {
        Assert.Throws<ArgumentException>(() => Create(MainRoot).WithOutsideRoutes([first, second]));
        Assert.Throws<ArgumentException>(() => Create(MainRoot).WithOutsideRoutes([second, first]));
    }

    [Fact(DisplayName = "Portable validation accepts ordinal prefix lookalikes without assigning authority to case aliases")]
    public void PrefixLookalikes()
    {
        var layout = Create(MainRoot).WithOutsideRoutes([Team, Team + "-old", MainRoot + "-old", WorkspaceLayout.DefaultMainRoot + "-old", WorkspaceRuntimePaths.DirectoryName + "-old"]);
        Assert.Equal(5, layout.OutsideRoutes.Length);
        Assert.False(layout.TryGetMaintenanceBoundary("DOCS/TEAM/" + Note, out _));
        Assert.Throws<ArgumentNullException>(() => layout.WithOutsideRoutes(null!));
    }

    [Theory(DisplayName = "Canonical route catalogue paths use the final directory segment")]
    [InlineData(Team, "_team")]
    [InlineData(Notes, "_notes")]
    [InlineData("docs/team.v2", "_team.v2")]
    public void CataloguePath(string directory, string stem)
        => Assert.Equal(directory + "/" + stem + WorkspaceRoutePath.MarkdownExtension, WorkspaceRoutePath.CanonicalEntrypointPath(directory));

    [Theory(DisplayName = "Canonical route catalogue construction rejects noncanonical paths")]
    [InlineData(".")]
    [InlineData("./" + Team)]
    [InlineData(Team + "/")]
    public void RejectsCataloguePath(string directory)
        => Assert.Throws<ArgumentException>(() => WorkspaceRoutePath.CanonicalEntrypointPath(directory));

    private static WorkspaceLayout Create(string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);

    private static void AssertBoundary(WorkspaceLayout layout, string path, WorkspaceMaintenanceBoundaryKind kind, string directory, string anchor)
    {
        Assert.True(layout.TryGetMaintenanceBoundary(path, out var boundary));
        Assert.Equal(kind, boundary.Kind);
        Assert.Equal(directory, boundary.DirectoryPath);
        Assert.Equal(anchor, boundary.AnchorPath);
    }
}
