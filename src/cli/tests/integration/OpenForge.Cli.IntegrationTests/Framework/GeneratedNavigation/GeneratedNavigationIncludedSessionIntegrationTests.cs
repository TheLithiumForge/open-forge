using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.GeneratedNavigation;

[Trait("Boundary", "OS"), Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
public sealed class GeneratedNavigationIncludedSessionIntegrationTests
{
    private const string FixtureName = "navigation-included-session";
    private const string CustomRoot = "docs/forge";
    private const string External = "project/guide" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "Included guide";
    private const string OtherDescription = "Other purpose";
    private const string Condition = "src/**";
    private const string OtherCondition = "tests/**";
    private const string SourceText = "# Guide\n";
    private const string Alias = "project/alias" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Navigation accepts real included sessions without inventing inventory candidates")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, "../"), InlineData(CustomRoot, "../../")]
    public async Task CandidateFreeIncludedSourcesKeepTheirIdentityAndRows(string mainRoot, string relativePrefix)
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
        var loaderPath = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        temporary.CreateFile(loaderPath, $"# Loader\n\n## Entries\n\n- [{Description}]({relativePrefix}{External}) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `{Condition}`\n- [{OtherDescription}]({relativePrefix}{External}) - #{SourceLoadingClosureResolver.KeepInMindTag} - applies to `{OtherCondition}`\n");
        temporary.CreateFile(External, SourceText);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(layout, TestContext.Current.CancellationToken);
        var catalogue = session.Catalogue;
        Assert.Empty(catalogue.Issues);
        Assert.Null(catalogue.FindCandidateByPath(External));
        var included = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(External));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + External, included.Identity.AutomaticId);
        Assert.Equal(2, catalogue.IncludedRows.Count);
        Assert.Equal([Description, OtherDescription], catalogue.IncludedRows.Select(row => row.Entry.Description));
        Assert.Equal([Condition, OtherCondition], catalogue.IncludedRows.Select(row => Assert.Single(row.Entry.ApplyTo).Text));
        Assert.All(catalogue.IncludedRows, row => Assert.Equal(loaderPath, row.ParentPath));

        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        Assert.Same(included, formation.FindSource(External));
        Assert.Same(catalogue, formation.Catalogue);
        Assert.Null(formation.Catalogue.FindCandidateByPath(External));
        Assert.Empty(formation.IntendedTargetCollisions);
        var projected = new SourceSharing([]).Project(catalogue);
        var replaced = new RouteScaffoldSourceBuilder().Replace(projected, []);
        Assert.Same(layout, replaced.Catalogue.Layout);
        Assert.Equal(catalogue.IncludedRows, replaced.Catalogue.IncludedRows);
        Assert.Same(included, replaced.FindSource(External));
        Assert.Null(replaced.Catalogue.FindCandidateByPath(External));
        var aliased = new SourceCatalogue(layout, catalogue.Candidates, catalogue.Sources, catalogue.Issues, false,
            new Dictionary<string, string> { [Alias] = External }, catalogue.IncludedRows);
        var filtered = new SourceSharing([]).Project(aliased);
        Assert.Same(included, filtered.FindByPath(Alias));
        Assert.Equal(External, filtered.Aliases[Alias]);
        Assert.Equal(catalogue.IncludedRows, filtered.IncludedRows);

        var route = await new RouteSourceProjector().ReadAsync(included, RouteSourceLayerProjection.ExactLayers, session.DocumentReader, TestContext.Current.CancellationToken);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + External, Assert.IsType<RouteSource>(route.Source).Id);
        Assert.Same(included.Identity, route.Source.Identity);
    }
}
