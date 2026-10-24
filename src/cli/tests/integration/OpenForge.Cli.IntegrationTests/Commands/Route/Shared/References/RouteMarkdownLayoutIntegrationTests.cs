using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.References;

[Trait("Boundary", "OS"), Trait("Feature", "route-markdown-catalogue"), Trait("Evidence", "Integration")]
public sealed class RouteMarkdownLayoutIntegrationTests
{
    private const string FixtureName = "route-markdown-layout";
    private const string Parent = "docs";
    private const string CustomRoot = Parent + "/forge";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string Body = "# Note\n";
    private const string IgnoredSibling = Parent + "/other";
    private const string SkillName = "example";

    [Fact(DisplayName = "Markdown traversal reaches both content locations through Gitignored ancestors")]
    public async Task BothContentLocationsRemainObservable()
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), CustomRoot);
        var mainPath = CustomRoot + "/" + Note;
        var skillPath = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
        var outside = IgnoredSibling + "/" + Note;
        temporary.CreateFile(mainPath, Body);
        temporary.CreateFile(skillPath, Body);
        temporary.CreateFile(outside, Body);
        var request = new RouteMarkdownCatalogueRequest(layout, [Parent, WorkspaceLayout.DefaultMainRoot], [], new([SourceFormClassifier.MarkdownExtension]));
        var reader = new RouteMarkdownCatalogueReader(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult<ImmutableArray<string>?>([Parent, WorkspaceLayout.DefaultMainRoot]));
        var result = await reader.ReadAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, result.Coverage);
        Assert.Equal(new[] { mainPath, skillPath }.Order(StringComparer.Ordinal), result.SelectedPaths);
        Assert.DoesNotContain(outside, result.SelectedPaths);
        Assert.Empty(result.Findings);
        Assert.True(result.IgnoredPathCount > 0);
    }
}
