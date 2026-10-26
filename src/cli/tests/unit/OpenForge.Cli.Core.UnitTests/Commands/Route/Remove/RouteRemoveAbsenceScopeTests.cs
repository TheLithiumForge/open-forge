using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveAbsenceScopeTests
{
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
