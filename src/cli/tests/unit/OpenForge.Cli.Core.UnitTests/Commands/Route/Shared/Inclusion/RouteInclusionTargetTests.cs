using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;

public sealed class RouteInclusionTargetTests
{
    private const string MainRoot = "docs/context";
    private const string Category = "topics";
    private const string Catalogue = "/_" + Category + SourceFormClassifier.MarkdownExtension;
    private const string Leaf = "/note" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Content catalogue admission is relative to the layout and ordinal"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData(MainRoot, MainRoot + "/" + Category + Catalogue, true)]
    [InlineData(MainRoot, MainRoot + "/" + Category + Leaf, false)]
    [InlineData(MainRoot, MainRoot + Leaf, false)]
    [InlineData(MainRoot, "DOCS/context" + Leaf, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, ".AGENTS" + Leaf, true)]
    [InlineData(MainRoot, WorkspaceLayout.DefaultMainRoot + Leaf, true)]
    [InlineData(MainRoot, WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension, true)]
    [InlineData(MainRoot, WorkspaceLayout.NativeSkillsPath + "/research/" + SourceFormClassifier.NativeSkillFileName, false)]
    public void ClassifiesWithoutFilesystem(string root, string path, bool allowed)
    {
        var workspace = RouteInclusionTestData.Invocation().Workspace!;
        var layout = WorkspaceLayout.Create(workspace, root);
        var request = new RouteInclusionRequest { Workspace = workspace, File = path, Action = RouteInclusionAction.Include };
        var exception = Record.Exception(() => RouteInclusionTargetResolver.ValidateContentTarget(layout, request, path));
        if (allowed) Assert.Null(exception);
        else Assert.IsType<ArgumentException>(exception);
    }

    [Theory(DisplayName = "Direct content files name the selected context folder"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectFileGuidance(bool custom)
    {
        var workspace = RouteInclusionTestData.Invocation().Workspace!;
        var root = custom ? MainRoot : WorkspaceLayout.DefaultMainRoot;
        var path = root + Leaf;
        var request = new RouteInclusionRequest { Workspace = workspace, File = path, Action = RouteInclusionAction.Include };
        var exception = Assert.Throws<ArgumentException>(() =>
            RouteInclusionTargetResolver.ValidateContentTarget(WorkspaceLayout.Create(workspace, root), request, path));
        CommandOutputSnapshot.MatchSnapshot(exception.Message, custom ? "custom" : "default");
    }

    [Theory(DisplayName = "Folder resolution keeps directory and catalogue coordinates distinct without filesystem access"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData("project/team")]
    [InlineData("./project/team")]
    [InlineData(@"project\team")]
    public void FolderCoordinates(string operand)
    {
        const string folder = "project/team";
        var workspace = RouteInclusionTestData.Invocation().Workspace!;
        var layout = WorkspaceLayout.Create(workspace, MainRoot);
        var request = new RouteInclusionRequest { Workspace = workspace, File = operand, Action = RouteInclusionAction.Include };
        var target = RouteInclusionTargetResolver.ResolveFolder(layout, request, folder);
        Assert.Equal(RouteInclusionTargetKind.Folder, target.Kind);
        Assert.Equal(folder, target.Path);
        Assert.Equal(WorkspaceRoutePath.CanonicalEntrypointPath(folder), target.CataloguePath);
        Assert.Equal(target.CataloguePath, target.LoaderPath);
        Assert.Null(target.DirectoryRead);
    }

    [Theory(DisplayName = "Folder resolution refuses reserved overlap and lexical aliases before physical observation"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData("project/../project/team", "project/team")]
    [InlineData(MainRoot, MainRoot)]
    [InlineData("docs", "docs")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceLayout.DefaultMainRoot)]
    public void FolderAdmission(string operand, string canonical)
    {
        var workspace = RouteInclusionTestData.Invocation().Workspace!;
        var request = new RouteInclusionRequest { Workspace = workspace, File = operand, Action = RouteInclusionAction.Include };
        Assert.Throws<ArgumentException>(() => RouteInclusionTargetResolver.ResolveFolder(WorkspaceLayout.Create(workspace, MainRoot), request, canonical));
    }
}
