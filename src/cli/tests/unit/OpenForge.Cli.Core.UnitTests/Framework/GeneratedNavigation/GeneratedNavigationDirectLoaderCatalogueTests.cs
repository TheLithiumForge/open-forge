using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

[Trait("Boundary", "Processing"), Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
public sealed class GeneratedNavigationDirectLoaderCatalogueTests
{
    private const string CustomRoot = "docs/forge";
    private const string Category = "guide";
    private const string Group = "group";
    private const string Description = "Nested catalogue purpose";
    private const string Pattern = "**/*.cs";

    public static IEnumerable<object[]> DirectCatalogueScenarios()
    {
        foreach (var mainRoot in new[] { WorkspaceLayout.DefaultMainRoot, CustomRoot })
            foreach (var category in new[] { WorkspaceLayoutNames.SkillsDirectoryName, Category })
                foreach (var preserveMembership in new[] { false, true })
                    foreach (var replaceLoader in new[] { false, true })
                        yield return [mainRoot, category, preserveMembership, replaceLoader];
    }

    [Theory(DisplayName = "Direct loader catalogues retain membership and metadata without enclosing catalogues")]
    [MemberData(nameof(DirectCatalogueScenarios))]
    public void DirectNestedCataloguesSurviveRegeneration(
        string mainRoot, string category, bool preserveMembership, bool replaceLoader)
    {
        var layout = WorkspaceLayout.Create(
            new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        var loader = Source(layout.LoaderPath, SourceDocumentForm.Loader);
        var nested = Source(layout.SourcePath($"{category}/{Group}/{SourceFormClassifier.CanonicalEntrypointFileName(Group)}"),
            SourceDocumentForm.CanonicalEntrypoint);
        var formation = new GeneratedNavigationFormationBuilder().Build(new SourceCatalogue(layout, [], [loader, nested], [], false));
        var parser = new MarkdownDocumentParser();
        var metadata = new SourceAuthoredMetadataParser().Parse(parser.Parse($"""
            ---
            open-forge:
              description: {Description}
              tags: [{SourceLoadingClosureResolver.LoadNowTag}]
              applyTo: '{Pattern}'
            ---
            """), nested.Base.Form);
        var destination = Path.GetRelativePath(
            SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, mainRoot), nested.Base.PhysicalPath).Replace('\\', '/');
        var row = $"- [{metadata.Description}]({destination}) - #{metadata.Tags[0]}"
            + MarkdownEntryRowFormatter.FormatApplyToSuffix(metadata.ApplyTo.Patterns);
        var observed = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed { Entries = row, LineEnding = "\r\n" });
        var document = parser.Parse(replaceLoader ? OpenForgeDocumentSeed.GeneratedEntries(string.Empty) : observed);
        var input = new GeneratedNavigationRegionInput(loader, document);
        var request = new GeneratedNavigationProjectionRequest(formation, [input], [])
        {
            PreserveLoaderMembership = preserveMembership,
            ObservedLoaderSource = replaceLoader ? observed : null,
        };

        Assert.Empty(formation.Topology.LoaderRootPaths);
        var planner = new GeneratedNavigationRegionPlanner();
        var region = planner.Plan(request, input);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Empty(region.Entries);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        var rewritten = parser.Parse(Encoding.UTF8.GetString(change.ExpectedDocumentBytes.AsSpan()));
        Assert.Single(rewritten.Links);
        Assert.True(MarkdownEntryRowParser.TryParse(change.ExpectedBody.Trim(), out var parsed, out _));
        Assert.Equal(destination, parsed.Destination);
        Assert.Equal(metadata.Description, parsed.Description);
        Assert.Equal(metadata.Tags, parsed.Tags);
        Assert.Equal(metadata.ApplyTo.Patterns.Select(pattern => pattern.Text), parsed.ApplyTo.Select(pattern => pattern.Text));
        Assert.Equal(1, region.AuthoredLinkCount);
        if (!replaceLoader)
        {
            Assert.Equal(Encoding.UTF8.GetBytes(observed), change.ExpectedDocumentBytes.ToArray());
            var projected = planner.PlanProjectedEntries(layout, input, []);
            Assert.Equal(Encoding.UTF8.GetBytes(observed), Assert.IsType<GeneratedNavigationBoundedChange>(projected.Change).ExpectedDocumentBytes.ToArray());
            var supplied = new GeneratedNavigationEntry(nested, parsed.Description, destination, parsed.Tags, row) { ApplyTo = parsed.ApplyTo };
            var generated = planner.PlanProjectedEntries(layout, input, [supplied]);
            Assert.Equal(0, generated.AuthoredLinkCount);
            Assert.Equal(Encoding.UTF8.GetBytes(observed), Assert.IsType<GeneratedNavigationBoundedChange>(generated.Change).ExpectedDocumentBytes.ToArray());
        }
    }

    public enum CatalogueOmission { Unlisted, TurnedOff, Removed, Private }

    public static IEnumerable<object[]> OmittedCatalogueScenarios()
    {
        foreach (var mainRoot in new[] { WorkspaceLayout.DefaultMainRoot, CustomRoot })
            foreach (var category in new[] { WorkspaceLayoutNames.SkillsDirectoryName, Category })
                foreach (var omission in Enum.GetValues<CatalogueOmission>())
                    yield return [mainRoot, category, omission];
    }

    [Theory(DisplayName = "Direct loader catalogues respect omitted membership and private routes")]
    [MemberData(nameof(OmittedCatalogueScenarios))]
    public void OmittedNestedCataloguesStayOmitted(string mainRoot, string category, CatalogueOmission omission)
    {
        var layout = WorkspaceLayout.Create(
            new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        var loader = Source(layout.LoaderPath, SourceDocumentForm.Loader);
        var nested = Source(layout.SourcePath($"{category}/{Group}/{SourceFormClassifier.CanonicalEntrypointFileName(Group)}"),
            SourceDocumentForm.CanonicalEntrypoint);
        var catalogue = new SourceCatalogue(layout, [], [loader, nested], [], false);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue,
            omission == CatalogueOmission.Removed ? [loader] : [loader, nested]);
        var destination = Path.GetRelativePath(
            SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, mainRoot), nested.Base.PhysicalPath).Replace('\\', '/');
        var row = omission == CatalogueOmission.Unlisted ? string.Empty : $"- [{Description}]({destination})";
        var parser = new MarkdownDocumentParser();
        var input = new GeneratedNavigationRegionInput(loader, parser.Parse(OpenForgeDocumentSeed.GeneratedEntries(row)));
        var sharing = omission == CatalogueOmission.Private
            ? new SourceSharing([new(layout.CategoryDirectory(category),
                layout.SourcePath($"{category}/{SourceFormClassifier.CanonicalEntrypointFileName(category)}"))])
            : new SourceSharing([]);
        var request = new GeneratedNavigationProjectionRequest(formation, [input],
            omission == CatalogueOmission.Removed ? [] : [new(nested, SourceAuthoredMetadataFacts.Complete(Description, []))], sharing)
        {
            PreserveLoaderMembership = true,
            TurnedOffLoaderRoots = omission == CatalogueOmission.TurnedOff
                ? new HashSet<string>([nested.Identity.CanonicalBasePath], StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal),
        };

        var region = new GeneratedNavigationRegionPlanner().Plan(request, input);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Empty(region.Entries);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        Assert.Empty(parser.Parse(Encoding.UTF8.GetString(change.ExpectedDocumentBytes.AsSpan())).Links);
    }

    private static SourceLogicalSource Source(string path, SourceDocumentForm form)
        => new(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new SourceLayer(path, SourceLogicalPath.ToLexicalPath(AppContext.BaseDirectory, path), form, SourceLayerKind.Base));
}
