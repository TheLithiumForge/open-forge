using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Selection;

[Trait("Feature", "index-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class IndexLayoutModelsTests
{
    private const string MainRoot = "handbook/forge";
    private const string Route = "notes";
    private const string CatalogueName = "_notes.md";
    private const string CustomCatalogue = MainRoot + "/" + Route + "/" + CatalogueName;
    private const string DefaultCatalogue = WorkspaceLayout.DefaultMainRoot + "/" + Route + "/" + CatalogueName;
    private static readonly string LoaderId = Path.GetFileNameWithoutExtension(WorkspaceLayoutNames.LoaderFileName);
    private const string SkillName = "workshop";
    private const string SkillId = WorkspaceLayoutNames.SkillsDirectoryName + "/" + SkillName;
    private const string SkillPath = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string Cause = "Unavailable fixture target";

    [Fact(DisplayName = "Index selection retains custom loader and native Skill coordinates")]
    public void SelectionRetainsLayoutCoordinates()
    {
        var layout = WorkspaceLayout.Create(IndexTestData.Workspace(), MainRoot);
        var loader = new IndexLogicalSource(layout, LoaderId, MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, IndexLogicalSourceScope.Rooted);
        var selection = new IndexSelection(IndexSelectionOrigin.AutomaticLoader, IndexSelectionScope.Rooted, [loader]);
        Assert.Same(loader, Assert.Single(selection.Sources));
        var skill = new IndexLogicalSource(layout, SkillId, SkillPath, IndexLogicalSourceScope.Rooted);
        Assert.Equal(SkillId, skill.Id);
        Assert.Equal(SkillPath, skill.Path);
        Assert.False(skill.IsLoader);
        Assert.Throws<ArgumentException>(() => new IndexLogicalSource(layout, Route, DefaultCatalogue, IndexLogicalSourceScope.Rooted));
    }

    [Fact(DisplayName = "Index readiness validates unavailable targets against the selected content locations")]
    public void ReadinessUsesSelectedLayout()
    {
        var layout = WorkspaceLayout.Create(IndexTestData.Workspace(), MainRoot);
        var source = new IndexLogicalSource(layout, Route, CustomCatalogue, IndexLogicalSourceScope.Rooted);
        var finding = new IndexFinding(IndexFindingCode.TargetUnsafe, null, source, Cause, []);
        var readiness = new IndexProjectionReadiness(layout, [finding], [CustomCatalogue]);
        Assert.Equal(CustomCatalogue, Assert.Single(readiness.UnavailableTargetPaths));
        Assert.Same(finding, Assert.Single(readiness.Findings));
        Assert.Throws<ArgumentException>(() => new IndexProjectionReadiness(layout, [finding], [DefaultCatalogue]));
    }
}
