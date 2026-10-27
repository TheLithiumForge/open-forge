using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveAbsenceScopeTests
{
    [Theory]
    [InlineData("docs/team/note.md", "docs/team/note.md", false)]
    [InlineData("docs/team/note.overwrite.md", "docs/team/note.md", false)]
    [InlineData("docs/team/topic/_topic.md", "docs/team/topic/_topic.md", true)]
    [InlineData("docs/team/topic/index.md", "docs/team/topic/index.md", true)]
    public void OutsideAbsenceStaysAtExactCoordinates(string path, string intended, bool category)
    {
        var workspace = RouteRemoveTestData.Workspace();
        var layout = WorkspaceLayout.Create(workspace, MainRoot).WithOutsideRoutes(["docs/team"]);
        var scope = Assert.IsType<RouteRemoveAbsenceScope>(RouteRemoveAbsenceScope.TryCreate(
            new RouteRemoveRequest(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RouteRemoveMode.DryRun), layout));
        Assert.Equal(intended, scope.IntendedPath);
        Assert.NotNull(scope.OutsideRoute);
        Assert.True(scope.Contains(intended));
        Assert.False(scope.Contains("docs/team/unrelated.md"));
        Assert.False(scope.Contains(MainRoot + "/" + intended));
        Assert.Equal(category, scope.Contains("docs/team/topic/child.md"));
    }

    [Theory]
    [InlineData("docs/team")]
    [InlineData("docs/team/_team.md")]
    [InlineData("docs/team/index.md")]
    [InlineData("Docs/team/note.md")]
    [InlineData("docs/team-other/note.md")]
    public void OutsideAbsenceNeverGuessesAFormerBoundary(string path)
    {
        var workspace = RouteRemoveTestData.Workspace();
        var layout = WorkspaceLayout.Create(workspace, MainRoot).WithOutsideRoutes(["docs/team"]);
        Assert.Null(RouteRemoveAbsenceScope.TryCreate(new RouteRemoveRequest(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RouteRemoveMode.DryRun), layout));
    }

    private const string MainRoot = "handbook/context";
    private const string Category = "notes";
    private const string CategoryPath = MainRoot + "/" + Category;
    private const string Entrypoint = CategoryPath + "/_" + Category + SourceFormClassifier.MarkdownExtension;
    private const string SkillsEntrypoint = WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.IndexFileName;

    [Theory(DisplayName = "Route Remove absence coordinates use the selected main root and fixed native Skills")]
    [InlineData(Category, CategoryPath, CategoryPath + SourceFormClassifier.MarkdownExtension)]
    [InlineData(WorkspaceLayoutNames.SkillsDirectoryName, WorkspaceLayout.NativeSkillsPath, MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Unit")]
    public void LogicalAbsenceMapsPrimaryContent(string id, string directory, string leaf)
    {
        var workspace = RouteRemoveTestData.Workspace();
        var layout = WorkspaceLayout.Create(workspace, MainRoot);
        var request = new RouteRemoveRequest(workspace, id, RouteRemoveMode.DryRun);
        var scope = Assert.IsType<RouteRemoveAbsenceScope>(RouteRemoveAbsenceScope.TryCreate(request, layout));
        Assert.Same(layout, scope.Layout);
        Assert.Equal(directory, scope.CategoryRoot);
        Assert.Equal(leaf, scope.LeafPath);
        Assert.True(scope.Contains(directory + "/" + SourceFormClassifier.IndexFileName));
        Assert.False(scope.Contains(directory + "-other/" + SourceFormClassifier.IndexFileName));
    }

    [Theory(DisplayName = "Route Remove absent literal targets retain their exact content coordinates")]
    [InlineData(Entrypoint, Category)]
    [InlineData(SkillsEntrypoint, WorkspaceLayoutNames.SkillsDirectoryName)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Unit")]
    public void LiteralAbsenceRetainsExactPaths(string path, string id)
    {
        var workspace = RouteRemoveTestData.Workspace();
        var layout = WorkspaceLayout.Create(workspace, MainRoot);
        var request = new RouteRemoveRequest(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RouteRemoveMode.DryRun);
        var scope = Assert.IsType<RouteRemoveAbsenceScope>(RouteRemoveAbsenceScope.TryCreate(request, layout));
        Assert.Equal(path, scope.IntendedPath);
        Assert.Equal(id, scope.Id);
    }

    [Theory(DisplayName = "Route Remove absent literal targets cannot gain removal authority through a content-root case alias")]
    [InlineData(Entrypoint, MainRoot)]
    [InlineData(SkillsEntrypoint, WorkspaceLayout.DefaultMainRoot)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Unit")]
    public void CaseAliasesCannotAuthorizeAbsence(string path, string root)
    {
        var workspace = RouteRemoveTestData.Workspace();
        var layout = WorkspaceLayout.Create(workspace, MainRoot);
        var request = new RouteRemoveRequest(workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + path.Replace(root, root.ToUpperInvariant(), StringComparison.Ordinal), RouteRemoveMode.DryRun);
        Assert.Null(RouteRemoveAbsenceScope.TryCreate(request, layout));
    }
}
