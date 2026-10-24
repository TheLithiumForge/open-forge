using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Unit")]
public sealed class RouteCreateLayoutTargetTests
{
    private const string MainRoot = "handbook/context";
    private const string TargetId = "notes/release";
    private const string TargetPath = MainRoot + "/" + TargetId + SourceFormClassifier.MarkdownExtension;
    private const string SkillNoteId = WorkspaceLayoutNames.SkillsDirectoryName + "/workshop/references/note";
    private const string SkillNotePath = WorkspaceLayout.DefaultMainRoot + "/" + SkillNoteId + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Route Create resolves custom paths and native Skill references from stable IDs")]
    [InlineData(TargetId, TargetId, TargetPath)]
    [InlineData(SourceWorkspaceRelativePath.LiteralReferencePrefix + TargetPath, TargetId, TargetPath)]
    [InlineData(SkillNoteId, SkillNoteId, SkillNotePath)]
    [InlineData(SkillNotePath, SkillNoteId, SkillNotePath)]
    public void ResolvesWithinSelectedContentLocations(string requested, string expectedId, string expectedPath)
    {
        var request = RouteCreateTestData.Request(fileTarget: requested);
        var layout = WorkspaceLayout.Create(request.Workspace, MainRoot);
        var resolution = new RouteCreateTargetPlanner().Resolve(request, layout);
        Assert.Equal(RouteCreateTargetResolutionState.Resolved, resolution.State);
        Assert.Equal(expectedId, resolution.Target.Id);
        Assert.Equal(expectedPath, resolution.Target.Path);
    }

    [Fact(DisplayName = "Route Create never admits content-root case aliases or redirects literal default paths")]
    public void RefusesCaseAliasesAndLiteralDefaultDestinations()
    {
        var request = RouteCreateTestData.Request();
        var layout = WorkspaceLayout.Create(request.Workspace, MainRoot);
        string[] refused =
        [
            SourceWorkspaceRelativePath.LiteralReferencePrefix + MainRoot.ToUpperInvariant() + "/" + TargetId + SourceFormClassifier.MarkdownExtension,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/" + SkillNoteId + SourceFormClassifier.MarkdownExtension,
            WorkspaceLayout.DefaultMainRoot + "/" + TargetId + SourceFormClassifier.MarkdownExtension,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + MainRoot + "/" + SkillNoteId + SourceFormClassifier.MarkdownExtension,
        ];
        foreach (var target in refused)
            Assert.Equal(RouteCreateTargetResolutionState.Invalid,
                new RouteCreateTargetPlanner().Resolve(RouteCreateTestData.Request(fileTarget: target), layout).State);
    }
}
