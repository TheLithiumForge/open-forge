using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Includes;

[Trait("Boundary", "Processing"), Trait("Feature", "included-sources"), Trait("Evidence", "Unit")]
public sealed class SourceIncludedRowsTests
{
    private const string CustomRoot = "docs/forge";
    private const string Target = "project/guide" + SourceFormClassifier.MarkdownExtension;
    private const string Alias = "project/alias" + SourceFormClassifier.MarkdownExtension;
    private const string Missing = "project/missing" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "First purpose";
    private const string OtherDescription = "Second purpose";
    private const string Condition = "src/**";
    private const string OtherCondition = "tests/**";

    [Theory(DisplayName = "Catalogue reconstruction retains every included edge when inventory already contains its target")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void ExistingInventoryKeepsRowsAndAliases(string mainRoot)
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, mainRoot);
        var target = Source(Target, Physical(Target));
        var alias = Source(Alias, Physical(Target));
        var entries = new[]
        {
            Entry(Description, Condition, SourceLoadingClosureResolver.LoadNowTag, 1),
            Entry(OtherDescription, OtherCondition, SourceLoadingClosureResolver.KeepInMindTag, 2),
        };
        var rows = new[]
        {
            new SourceIncludedEntryRow(Target, layout.LoaderPath, entries[0], PhysicalPathState.Contained),
            new SourceIncludedEntryRow(Target, Missing, entries[1], PhysicalPathState.Contained),
            new SourceIncludedEntryRow(Alias, layout.LoaderPath, entries[1], PhysicalPathState.Contained),
            new SourceIncludedEntryRow(Missing, layout.LoaderPath, entries[0], PhysicalPathState.Missing),
        };
        var candidate = new SourceCandidate(Target, SourceDocumentForm.Markdown, SourceWorkspaceRelativePath.LiteralReferencePrefix + Target,
            PhysicalPathState.Contained, Physical(Target), Path.GetDirectoryName(Physical(Target)));
        var catalogue = new SourceCatalogue(layout, [candidate], [target], [], false);
        var extended = SourceIncludedEntryReader.Extend(catalogue, [target, alias], rows);
        var projected = new SourceSharing([]).Project(extended);

        var formation = new GeneratedNavigationFormationBuilder().Build(projected);
        var scaffolded = new RouteScaffoldSourceBuilder().Add(projected, []);
        Assert.Same(projected, formation.Catalogue);
        Assert.Same(projected, scaffolded.Catalogue);
        Assert.Same(layout, projected.Layout);
        Assert.Equal(rows, projected.IncludedRows);
        Assert.Equal(Target, projected.Aliases[Alias]);
        var retained = Assert.Single(projected.Sources);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Target, retained.Identity.AutomaticId);
        Assert.Same(entries[0], retained.IncludedEntry);
        Assert.Equal(layout.LoaderPath, retained.IncludedParentPath);
        Assert.Same(retained, projected.FindByPath(Alias));
        Assert.Equal(entries[1].ApplyTo, projected.IncludedRows[1].Entry.ApplyTo);
        Assert.Equal(entries[1].Tags, projected.IncludedRows[1].Entry.Tags);
        Assert.Equal(entries[1].Description, projected.IncludedRows[1].Entry.Description);
        Assert.Equal(PhysicalPathState.Missing, projected.IncludedRows[3].PhysicalState);
    }

    private static SourceGeneratedEntry Entry(string description, string condition, string tag, int offset)
        => new(description, Target, [tag], new MarkdownTextSpan(offset, 1))
        {
            ApplyTo = [new ApplyToPattern(condition, [.. condition.Split('/')])],
        };

    private static SourceLogicalSource Source(string path, string physical)
        => new(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path), new SourceLayer(path, physical, SourceDocumentForm.Markdown, SourceLayerKind.Base));

    private static string Physical(string path) => Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
}
