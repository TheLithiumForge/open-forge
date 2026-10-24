using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

[Trait("Boundary", "Processing"), Trait("Feature", "source-routing"), Trait("Evidence", "Unit")]
public sealed class SourceOutsideTopologyTests
{
    private const string Directory = "docs/team";
    private const string CustomRoot = "knowledge/main";
    private const string OtherDirectory = "reference";
    private const string ChildName = "guide" + SourceFormClassifier.MarkdownExtension;
    private const string Child = Directory + "/" + ChildName;
    private const string Unlisted = Directory + "/unlisted" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "Retained row purpose";
    private const string LaterDescription = "Later canonical row purpose";
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    private static readonly string Above = WorkspaceRoutePath.CanonicalEntrypointPath("docs");

    [Theory(DisplayName = "Included merge preserves both primary inventory identities and short IDs when their physical paths alias")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void PrimaryInventoryAliasesKeepIds(string mainRoot)
    {
        var layout = WorkspaceLayout.Create(Layout().Workspace, mainRoot);
        var physical = Path.Combine(AppContext.BaseDirectory, ChildName);
        var sources = new[] { "guidance/first", "guidance/second" }.Select(coordinate =>
        {
            var path = layout.SourcePath(coordinate + SourceFormClassifier.MarkdownExtension);
            var id = Assert.IsType<string>(SourceIdentity.DeriveId(layout, path));
            return new SourceLogicalSource(new SourceLogicalIdentity(id, path), new SourceLayer(path, physical, SourceDocumentForm.Markdown, SourceLayerKind.Base));
        }).ToArray();
        var candidates = sources.Select(source => new SourceCandidate(source.Base.CanonicalPath, source.Base.Form, source.Identity.AutomaticId,
            PhysicalPathState.Contained, physical, Path.GetDirectoryName(physical)));
        var catalogue = new SourceCatalogue(layout, candidates, sources, [], false);
        var extended = SourceIncludedEntryReader.Extend(catalogue, []);
        var resolver = new SourceReferenceResolver((_, path) => PhysicalPathResolution.Classified(PhysicalPathState.Missing, path));

        Assert.Empty(layout.OutsideRoutes);
        foreach (var source in sources)
        {
            Assert.Same(source, resolver.Resolve(source.Identity.AutomaticId, catalogue).Source);
            var resolved = resolver.Resolve(source.Identity.AutomaticId, extended);
            Assert.Equal(SourceReferenceResolutionState.Resolved, resolved.State);
            Assert.Same(source, resolved.Source);
            Assert.Same(source, extended.FindByPath(source.Base.CanonicalPath));
        }
        Assert.Equal(catalogue.Sources, extended.Sources);
        Assert.Equal(catalogue.Candidates, extended.Candidates);
    }

    [Fact(DisplayName = "Registered anchors stop structural ancestry without becoming loader members")]
    public void RegisteredBoundaryStopsParents()
    {
        var layout = Layout().WithOutsideRoutes([Directory]);
        var anchor = Source(Anchor, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(Child, SourceDocumentForm.Markdown);
        var above = Source(Above, SourceDocumentForm.CanonicalEntrypoint);
        var topology = new SourceRouteTopologyBuilder().Build(layout, [above, anchor, child], []);

        Assert.Equal(SourceRouteParentState.None, topology.FindByPath(Anchor)?.ParentState);
        Assert.Equal(Anchor, Assert.Single(Assert.IsType<SourceRouteNode>(topology.FindByPath(Child)).ParentPaths));
        Assert.Empty(topology.LoaderRootPaths);
        Assert.DoesNotContain(Anchor, Assert.IsType<SourceRouteNode>(topology.FindByPath(Above)).ChildPaths);
    }

    [Fact(DisplayName = "Registered catalogue selection covers unlisted inventory even when it also has an included row")]
    public void RegisteredSelectionUsesFolderScope()
    {
        var layout = Layout().WithOutsideRoutes([Directory]);
        var anchor = new SourceLogicalSource(Source(Anchor, SourceDocumentForm.CanonicalEntrypoint).Identity,
            Source(Anchor, SourceDocumentForm.CanonicalEntrypoint).Base)
        {
            IsIncludedCatalogue = true,
            IncludedParentPath = layout.LoaderPath,
            IncludedEntry = Entry(Anchor),
        };
        var catalogue = new SourceCatalogue(layout, [], [anchor, Source(Child, SourceDocumentForm.Markdown), Source(Unlisted, SourceDocumentForm.Markdown)], [], false);
        var resolution = Filter(catalogue, Anchor);

        Assert.True(resolution.IsResolved);
        Assert.Equal(Directory, Assert.Single(resolution.Selectors).Scope?.CanonicalDirectoryPath);
        Assert.Equal(new[] { Anchor, Child, Unlisted }.Order(StringComparer.Ordinal), resolution.Selection.Sources.Select(source => source.Base.CanonicalPath).Order(StringComparer.Ordinal));
    }

    [Fact(DisplayName = "Ordinary included catalogue selection follows retained edges once through cycles without scanning siblings")]
    public void IncludedSelectionTerminatesCycles()
    {
        var a = Source(Anchor, SourceDocumentForm.Markdown);
        var bPath = WorkspaceRoutePath.CanonicalEntrypointPath(OtherDirectory);
        var b = Source(bPath, SourceDocumentForm.Markdown);
        a = new SourceLogicalSource(a.Identity, a.Base) { IsIncludedCatalogue = true };
        b = new SourceLogicalSource(b.Identity, b.Base) { IsIncludedCatalogue = true };
        var rows = new[]
        {
            new SourceIncludedEntryRow(bPath, Anchor, Entry(bPath), PhysicalPathState.Contained),
            new SourceIncludedEntryRow(Anchor, bPath, Entry(Anchor), PhysicalPathState.Contained),
            new SourceIncludedEntryRow(Child, bPath, Entry(Child), PhysicalPathState.Contained),
        };
        var catalogue = new SourceCatalogue(Layout(), [], [a, b, Source(Child, SourceDocumentForm.Markdown), Source(Unlisted, SourceDocumentForm.Markdown)], [], false, null, rows);
        var resolution = Filter(catalogue, Anchor);

        Assert.Null(Assert.Single(resolution.Selectors).Scope);
        Assert.Equal(3, resolution.Selection.Sources.Count);
        Assert.DoesNotContain(resolution.Selection.Sources, source => source.Base.CanonicalPath == Unlisted);
    }

    [Theory(DisplayName = "Generated outside destinations stay within their registered boundary while leaving links retain their identity")]
    [InlineData(ChildName, Child)]
    [InlineData("../outside" + SourceFormClassifier.MarkdownExtension, null)]
    [InlineData("../../" + OtherDirectory + "/" + ChildName, null)]
    [InlineData("../../" + Directory + "/" + ChildName, null)]
    public void GeneratedDestinationsRespectBoundary(string destination, string? expected)
    {
        var layout = Layout().WithOutsideRoutes([Directory, OtherDirectory]);
        Assert.Equal(expected, SourceGeneratedDestinationResolver.Resolve(layout, Anchor, false, destination));
        Assert.Equal(Child, SourceGeneratedDestinationResolver.ResolveLinked(layout, Anchor, false, ChildName));
        Assert.Null(SourceGeneratedDestinationResolver.Resolve(layout, layout.LoaderPath, true, "../" + Anchor));
        Assert.Equal(Anchor, SourceGeneratedDestinationResolver.ResolveLinked(layout, layout.LoaderPath, true, "../" + Anchor));
    }

    [Fact(DisplayName = "Formation discovers registered structural anchors without attaching an included catalogue above the boundary")]
    public void FormationKeepsRegistrationAndRowsSeparate()
    {
        var layout = Layout().WithOutsideRoutes([Directory]);
        var anchor = Source(Anchor, SourceDocumentForm.CanonicalEntrypoint);
        var loader = Source(layout.LoaderPath, SourceDocumentForm.Loader);
        var row = new SourceIncludedEntryRow(Child, Anchor, Entry(Child), PhysicalPathState.Contained);
        var catalogue = new SourceCatalogue(layout, [], [loader, anchor, Source(Child, SourceDocumentForm.Markdown)], [], false, null, [row]);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue, catalogue.Sources.Reverse().ToArray());

        Assert.Equal(Anchor, Assert.Single(formation.Topology.LoaderRootPaths));
        Assert.Equal(SourceRouteParentState.None, formation.Topology.FindByPath(Anchor)?.ParentState);
        Assert.Same(catalogue, formation.Catalogue);
        Assert.Same(row, Assert.Single(formation.Catalogue.IncludedRows));
        Assert.Null(formation.Catalogue.FindCandidateByPath(Anchor));
        Assert.Empty(formation.IntendedTargetCollisions);
    }

    [Fact(DisplayName = "The first retained alias row owns effective policy over a later canonical inventory projection")]
    public void AliasRowOwnsFirstPolicy()
    {
        var layout = Layout().WithOutsideRoutes([Directory]);
        var child = Source(Child, SourceDocumentForm.Markdown);
        var later = new SourceGeneratedEntry(LaterDescription, Child, [SourceLoadingClosureResolver.KeepInMindTag], new MarkdownTextSpan(1, 1));
        var projection = new SourceLogicalSource(child.Identity, child.Base) { IncludedEntry = later, IncludedParentPath = Anchor };
        var alias = OtherDirectory + "/" + ChildName;
        var first = Entry(alias);
        var rows = new[]
        {
            new SourceIncludedEntryRow(alias, layout.LoaderPath, first, PhysicalPathState.Contained) { AliasOf = Child },
            new SourceIncludedEntryRow(Child, Anchor, later, PhysicalPathState.Contained),
        };
        var catalogue = new SourceCatalogue(layout, [], [child], [], false);
        var merged = SourceIncludedEntryReader.Extend(catalogue, [projection], rows);
        var selected = Assert.Single(merged.Sources);

        Assert.Same(first, selected.IncludedEntry);
        Assert.Equal(layout.LoaderPath, selected.IncludedParentPath);
        Assert.Same(selected, merged.FindByPath(alias));
        Assert.Equal(rows, merged.IncludedRows);
    }

    private static SourceUniverseFilterResolution Filter(SourceCatalogue catalogue, string path)
        => new SourceUniverseFilterResolver(new SourceReferenceResolver((_, logical) => PhysicalPathResolution.Classified(PhysicalPathState.Missing, logical)))
            .Resolve(new SourceUniverseFilterRequest(catalogue, [], [new(SourceUniverseSelectorRole.Include, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, 1)]));

    private static SourceGeneratedEntry Entry(string destination) => new(Description, destination, [], new MarkdownTextSpan(0, 1));

    private static WorkspaceLayout Layout()
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), WorkspaceLayout.DefaultMainRoot);

    private static SourceLogicalSource Source(string path, SourceDocumentForm form)
        => new(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new SourceLayer(path, Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar)), form, SourceLayerKind.Base));
}
