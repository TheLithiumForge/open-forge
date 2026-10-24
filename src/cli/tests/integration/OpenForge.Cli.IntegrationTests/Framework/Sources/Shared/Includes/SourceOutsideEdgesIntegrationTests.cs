using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Shared.Includes;

[Trait("Boundary", "OS"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
public sealed class SourceOutsideEdgesIntegrationTests
{
    private const string Purpose = "outside-edges";
    private const string CustomRoot = "knowledge/main";
    private const string Directory = "docs/team";
    private const string Disconnected = "aaa";
    private const string ChildName = "guide" + SourceFormClassifier.MarkdownExtension;
    private const string Child = Directory + "/" + ChildName;
    private const string Unlisted = Directory + "/unlisted" + SourceFormClassifier.MarkdownExtension;
    private const string MissingName = "missing" + SourceFormClassifier.MarkdownExtension;
    private const string Alias = "alias" + SourceFormClassifier.MarkdownExtension;
    private const string EarlierAlias = Directory + "/a-alias" + SourceFormClassifier.MarkdownExtension;
    private const string FirstDescription = "Established first-reaching policy";
    private const string OtherDescription = "Disconnected conflicting policy";
    private const string Condition = "src/**";
    private const string OtherCondition = "tests/**";
    private const string Content = "# Authored content\r\n";
    private const string ExternalLink = "https://example.invalid/reference";
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    private static readonly string DetachedAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(Disconnected);
    private static readonly string Nested = WorkspaceRoutePath.CanonicalEntrypointPath(Directory + "/nested");

    [Theory(DisplayName = "A loader row to an inventoried outside overwrite retains its paired base identity and both layers")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task IncludedOverwriteKeepsBaseOwner(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var layout = Layout(temporary, mainRoot).WithOutsideRoutes([Directory]);
        var companion = SourceOverwritePath.ReadAdjacentPath(Child);
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        temporary.CreateFile(layout.LoaderPath, Catalogue(Row(FirstDescription, prefix + companion, Condition)));
        temporary.CreateFile(Anchor, Catalogue(string.Empty));
        temporary.CreateFile(Child, Content);
        temporary.CreateFile(companion, Content);
        var paths = new[] { layout.LoaderPath, Anchor, Child, companion };
        var bytes = paths.ToDictionary(path => path, path => File.ReadAllBytes(temporary.Combine(path)));
        var before = temporary.SnapshotHashes();
        var inventory = await Read(layout, false, retainEdges: false);
        var original = Assert.IsType<SourceLogicalSource>(inventory.Catalogue.FindByPath(Child));
        Assert.Equal(companion, original.Overwrite?.CanonicalPath);
        var session = await Read(layout, false);
        var source = Assert.IsType<SourceLogicalSource>(session.Catalogue.FindByPath(Child));

        Assert.Equal(original.Identity.AutomaticId, source.Identity.AutomaticId);
        Assert.Equal(Child, source.Base.CanonicalPath);
        Assert.Equal(companion, source.Overwrite?.CanonicalPath);
        Assert.Same(source, session.Catalogue.FindByPath(companion));
        Assert.Single(session.Catalogue.Sources, candidate => candidate.Base.CanonicalPath == Child || candidate.Base.CanonicalPath == companion);
        var row = Assert.Single(session.Catalogue.IncludedRows);
        Assert.Equal(companion, row.Path);
        Assert.Equal(Child, row.AliasOf);
        Assert.Equal(layout.LoaderPath, row.ParentPath);
        Assert.Equal(FirstDescription, row.Entry.Description);
        Assert.Contains(SourceLoadingClosureResolver.LoadNowTag, row.Entry.Tags);
        Assert.Equal(Condition, Assert.Single(row.Entry.ApplyTo).Text);
        Assert.NotNull(source.IncludedEntry);
        Assert.Equal(row.Entry.Description, source.IncludedEntry.Description);
        Assert.Equal(row.Entry.ApplyTo, source.IncludedEntry.ApplyTo);
        Assert.Equal(before, temporary.SnapshotHashes());
        foreach (var path in paths) Assert.Equal(bytes[path], File.ReadAllBytes(temporary.Combine(path)));
    }

    [Theory(DisplayName = "An earlier inventoried alias retains the first authored literal target and loading policy as one physical source")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task EarlierInventoryAliasPreservesReachedIdentity(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var original = Layout(temporary, mainRoot);
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        temporary.CreateFile(original.LoaderPath, Catalogue(Row(FirstDescription, prefix + Child, Condition)));
        temporary.CreateFile(Anchor, Catalogue(string.Empty));
        temporary.CreateFile(Child, Content);
        temporary.CreateFileSymbolicLink(EarlierAlias, temporary.Combine(Child));
        var paths = new[] { original.LoaderPath, Anchor, Child, EarlierAlias };
        var bytes = paths.ToDictionary(path => path, path => File.ReadAllBytes(temporary.Combine(path)));
        var before = temporary.SnapshotHashes();
        var initial = await Read(original, false);
        var oldChild = Assert.IsType<SourceLogicalSource>(initial.Catalogue.FindByPath(Child));
        Assert.NotNull(oldChild.IncludedEntry);
        var inventory = await Read(original.WithOutsideRoutes([Directory]), false, retainEdges: false);
        var included = await new SourceIncludedEntryReader().ReadEntriesAsync(inventory.Catalogue, inventory.DocumentReader, TestContext.Current.CancellationToken);
        var merged = SourceIncludedEntryReader.Extend(inventory.Catalogue, included.Sources, included.Rows);

        foreach (var catalogue in new[] { merged, SourceIncludedEntryReader.Extend(merged, []) })
        {
            var child = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(Child));
            Assert.NotNull(child.IncludedEntry);
            Assert.Equal(oldChild.Identity.AutomaticId, child.Identity.AutomaticId);
            Assert.Equal(oldChild.Base.CanonicalPath, child.Base.CanonicalPath);
            Assert.Equal(oldChild.IncludedParentPath, child.IncludedParentPath);
            Assert.Equal(oldChild.IncludedEntry.Description, child.IncludedEntry.Description);
            Assert.Equal(oldChild.IncludedEntry.Tags, child.IncludedEntry.Tags);
            Assert.Equal(oldChild.IncludedEntry.ApplyTo.Select(pattern => pattern.Text), child.IncludedEntry.ApplyTo.Select(pattern => pattern.Text));
            Assert.Single(catalogue.Sources, source => PhysicalIdentityTracker.PathComparer.Equals(source.Base.PhysicalPath, child.Base.PhysicalPath));
            Assert.Same(child, catalogue.FindByPath(EarlierAlias));
            Assert.Equal(Child, catalogue.Aliases[EarlierAlias]);
            AssertRows(initial.Catalogue.IncludedRows, catalogue.IncludedRows);
            Assert.Equal(inventory.Catalogue.Candidates, catalogue.Candidates);
            Assert.Equal(inventory.Catalogue.Issues, catalogue.Issues);
        }
        Assert.Equal(before, temporary.SnapshotHashes());
        foreach (var path in paths) Assert.Equal(bytes[path], File.ReadAllBytes(temporary.Combine(path)));
    }

    [Theory(DisplayName = "Registration and inventory ordering preserve established rows aliases cycles and first-reaching metadata through prospective formation")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false), InlineData(WorkspaceLayout.DefaultMainRoot, false, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false), InlineData(WorkspaceLayout.DefaultMainRoot, true, true)]
    [InlineData(CustomRoot, false, false), InlineData(CustomRoot, false, true)]
    [InlineData(CustomRoot, true, false), InlineData(CustomRoot, true, true)]
    public async Task RegistrationPreservesEstablishedTraversal(string mainRoot, bool reverseRegistrations, bool reverseScan)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var original = Layout(temporary, mainRoot);
        Seed(temporary, original, listed: true);
        var before = temporary.SnapshotHashes();
        var loaderBytes = File.ReadAllBytes(temporary.Combine(original.LoaderPath));
        var catalogueBytes = File.ReadAllBytes(temporary.Combine(Anchor));
        var initial = await Read(original, reverseScan: false);
        var layout = original.WithOutsideRoutes(reverseRegistrations ? [Directory, Disconnected] : [Disconnected, Directory]);
        var registered = await Read(layout, reverseScan);
        var projected = new SourceSharing([]).Project(registered.Catalogue);
        var reconstructed = SourceIncludedEntryReader.Extend(projected, []);
        var added = Source(temporary, Directory + "/added" + SourceFormClassifier.MarkdownExtension);
        var formation = new GeneratedNavigationFormationBuilder().Build(reconstructed, [.. reconstructed.Sources.Reverse(), added]);

        Assert.Empty(initial.Catalogue.Issues);
        Assert.Empty(registered.Catalogue.Issues);
        AssertRows(initial.Catalogue.IncludedRows, registered.Catalogue.IncludedRows.Where(row => row.ParentPath != DetachedAnchor).ToArray());
        AssertRows(registered.Catalogue.IncludedRows, formation.Catalogue.IncludedRows);
        var oldChild = Assert.IsType<SourceLogicalSource>(initial.Catalogue.FindByPath(Child));
        var newChild = Assert.IsType<SourceLogicalSource>(formation.FindSource(Child));
        Assert.Equal(FirstDescription, newChild.IncludedEntry?.Description);
        Assert.Equal(oldChild.IncludedParentPath, newChild.IncludedParentPath);
        Assert.Equal(oldChild.IncludedEntry?.Tags, newChild.IncludedEntry?.Tags);
        Assert.Equal(oldChild.IncludedEntry?.ApplyTo.Select(pattern => pattern.Text), newChild.IncludedEntry?.ApplyTo.Select(pattern => pattern.Text));
        Assert.Equal(oldChild.Identity.AutomaticId, newChild.Identity.AutomaticId);
        Assert.Equal(Child, registered.Catalogue.Aliases[Alias]);
        Assert.Same(newChild, formation.Catalogue.FindByPath(Alias));
        Assert.Contains(registered.Catalogue.IncludedRows, row => row.Path == Directory + "/" + MissingName && row.PhysicalState == PhysicalPathState.Missing);
        Assert.Contains(registered.Catalogue.IncludedRows, row => row.ParentPath == DetachedAnchor && row.Entry.Description == OtherDescription);
        Assert.Null(initial.Catalogue.FindByPath(Unlisted));
        Assert.NotNull(registered.Catalogue.FindByPath(Unlisted));
        Assert.Equal(Anchor, Assert.Single(Assert.IsType<SourceRouteNode>(formation.Topology.FindByPath(added.Base.CanonicalPath)).ParentPaths));
        Assert.Null(formation.Catalogue.FindCandidateByPath(added.Base.CanonicalPath));
        Assert.Empty(formation.IntendedTargetCollisions);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(loaderBytes, File.ReadAllBytes(temporary.Combine(layout.LoaderPath)));
        Assert.Equal(catalogueBytes, File.ReadAllBytes(temporary.Combine(Anchor)));
    }

    [Theory(DisplayName = "Registered route facts distinguish listed anchors from detached registration and ordinary linked files")]
    [InlineData(false), InlineData(true)]
    public async Task StructuralFactsDoNotInferListing(bool listed)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var layout = Layout(temporary, WorkspaceLayout.DefaultMainRoot).WithOutsideRoutes([Directory, Disconnected]);
        Seed(temporary, layout, listed);
        var session = await Read(layout, false);
        var facts = await new SourceRouteFactsResolver().ResolveAsync(new(session.Catalogue, session.Catalogue.SelectAll()), session.DocumentReader, TestContext.Current.CancellationToken);
        var anchor = Assert.Single(facts.RouteFacts, fact => fact.Identity.CanonicalBasePath == Anchor);

        Assert.True(facts.AreLoaderRootFactsComplete);
        Assert.Empty(facts.Issues);
        Assert.Equal(listed ? SourceRouteState.Routed : SourceRouteState.Unrouted, anchor.State);
        Assert.Equal(listed, facts.Topology.LoaderRootPaths.Contains(Anchor));
        Assert.DoesNotContain(DetachedAnchor, facts.Topology.LoaderRootPaths);
        Assert.Equal(SourceRouteParentState.None, facts.Topology.FindByPath(Anchor)?.ParentState);
        var selection = new SourceUniverseFilterResolver(new SourceReferenceResolver((workspace, path) =>
            new PhysicalPathResolver().ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path))))
            .Resolve(new SourceUniverseFilterRequest(session.Catalogue, session.DefaultSelectionScopes,
                [new(SourceUniverseSelectorRole.Include, SourceWorkspaceRelativePath.LiteralReferencePrefix + Anchor, 1)]));
        Assert.Equal(Directory, Assert.Single(selection.Selectors).Scope?.CanonicalDirectoryPath);
        Assert.Contains(selection.Selection.Sources, source => source.Base.CanonicalPath == Unlisted);
        Assert.DoesNotContain(selection.Selection.Sources, source => source.Base.CanonicalPath == DetachedAnchor);
    }

    [Theory(DisplayName = "Loader projection preserves outside rows exactly and never generates a row from registration")]
    [InlineData(false, false, false), InlineData(false, true, false), InlineData(true, false, false), InlineData(true, true, false)]
    [InlineData(false, false, true), InlineData(false, true, true), InlineData(true, false, true), InlineData(true, true, true)]
    public async Task ProjectionKeepsAuthoredPolicy(bool listed, bool preserveMembership, bool missingAnchor)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Directory, Disconnected]);
        Seed(temporary, layout, listed);
        if (missingAnchor) File.Delete(temporary.Combine(Anchor));
        var before = temporary.SnapshotHashes();
        var session = await Read(layout, false);
        var formation = new GeneratedNavigationFormationBuilder().Build(session.Catalogue);
        var loader = Assert.IsType<SourceLogicalSource>(formation.Loader);
        var document = Parse(File.ReadAllText(temporary.Combine(layout.LoaderPath)));
        var input = new GeneratedNavigationRegionInput(loader, document);
        var metadata = formation.Sources.Select(source => new GeneratedNavigationMetadata(source, SourceAuthoredMetadataFacts.Complete(OtherDescription, [])));
        var request = new GeneratedNavigationProjectionRequest(formation, [input], metadata) { PreserveLoaderMembership = preserveMembership };
        var region = new GeneratedNavigationRegionPlanner().Plan(request, input);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.DoesNotContain(region.Entries, entry => SourceLogicalPath.IsOutsideSource(layout, entry.CanonicalPath));
        var rewritten = Parse(Encoding.UTF8.GetString(Assert.IsType<GeneratedNavigationBoundedChange>(region.Change).ExpectedDocumentBytes.AsSpan()));
        var outsideRows = SourceGeneratedEntriesParser.Parse(rewritten).Entries
            .Where(entry => SourceGeneratedDestinationResolver.ResolveLinked(layout, layout.LoaderPath, true, entry.Destination) == Anchor).ToArray();
        Assert.Equal(listed ? 1 : 0, outsideRows.Length);
        if (listed)
        {
            var original = Assert.Single(SourceGeneratedEntriesParser.Parse(document).Entries);
            Assert.Equal(RowBytes(document, original), RowBytes(rewritten, Assert.Single(outsideRows)));
        }
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Registered catalogue projection retains conditioned authored descendants and unresolved links while generating only new descendants")]
    [InlineData(false), InlineData(true)]
    public async Task CatalogueProjectionRetainsRows(bool retainEdges)
    {
        using var temporary = TemporaryWorkspace.Create(Purpose);
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Directory, Disconnected]);
        Seed(temporary, layout, listed: true);
        var session = await Read(layout, false, retainEdges);
        var formation = new GeneratedNavigationFormationBuilder().Build(session.Catalogue);
        var anchor = Assert.IsType<SourceLogicalSource>(formation.FindSource(Anchor));
        var document = Parse(File.ReadAllText(temporary.Combine(Anchor)));
        var input = new GeneratedNavigationRegionInput(anchor, document);
        var metadata = formation.Sources.Select(source => new GeneratedNavigationMetadata(source, SourceAuthoredMetadataFacts.Complete(OtherDescription, [])));
        var region = new GeneratedNavigationRegionPlanner().Plan(new(formation, [input], metadata), input);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        var rewritten = Parse(Encoding.UTF8.GetString(Assert.IsType<GeneratedNavigationBoundedChange>(region.Change).ExpectedDocumentBytes.AsSpan()));
        var actual = SourceGeneratedEntriesParser.Parse(rewritten).Entries;
        foreach (var row in SourceGeneratedEntriesParser.Parse(document).Entries)
        {
            var retained = Assert.Single(actual, candidate => candidate.Destination == row.Destination);
            Assert.Equal(RowBytes(document, row), RowBytes(rewritten, retained));
        }
        Assert.Contains(region.Entries, entry => entry.CanonicalPath == Unlisted);
        Assert.DoesNotContain(region.Entries, entry => entry.CanonicalPath == Child || entry.CanonicalPath == Nested);
    }

    private static void Seed(TemporaryWorkspace temporary, WorkspaceLayout layout, bool listed)
    {
        var prefix = string.Concat(Enumerable.Repeat("../", layout.MainRoot.Split('/').Length));
        temporary.CreateFile(layout.LoaderPath, Catalogue(listed ? Row(FirstDescription, prefix + Anchor, Condition) : string.Empty));
        var guidePath = layout.SourcePath(WorkspaceRoutePath.CanonicalEntrypointPath("guide"));
        temporary.CreateFile(guidePath, Catalogue(Row(FirstDescription, "../" + prefix + Child, Condition)));
        temporary.CreateFile(Anchor, Catalogue(string.Join("\r\n",
            Row(OtherDescription, ChildName, OtherCondition),
            Row(OtherDescription, "nested/" + Path.GetFileName(Nested), Condition),
            Row(OtherDescription, MissingName, Condition),
            Row(OtherDescription, "../../" + Alias, Condition),
            Row(OtherDescription, ExternalLink, Condition))));
        temporary.CreateFile(Nested, Catalogue(Row(OtherDescription, "../" + Path.GetFileName(Anchor), Condition) + "\r\n" + Row(OtherDescription, "../" + ChildName, Condition)));
        temporary.CreateFile(DetachedAnchor, Catalogue(Row(OtherDescription, "../" + Child, OtherCondition)));
        temporary.CreateFile(Child, Content);
        temporary.CreateFile(Unlisted, Content);
        temporary.CreateFileSymbolicLink(Alias, temporary.Combine(Child));
    }

    private static string Catalogue(string rows)
        => OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed { Entries = rows, LineEnding = "\r\n" });

    private static string Row(string description, string destination, string condition)
        => $"- [{description}]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}"
            + MarkdownEntryRowFormatter.FormatApplyToSuffix([new ApplyToPattern(condition, [.. condition.Split('/')])]);

    private static MarkdownDocumentFacts Parse(string source) => new MarkdownDocumentParser().Parse(source);

    private static byte[] RowBytes(MarkdownDocumentFacts document, SourceGeneratedEntry entry)
        => Encoding.UTF8.GetBytes(document.Source[entry.Span.Start..(entry.Span.End + (document.GeneratedRegion.EntriesBlock?.LineEnding.Length ?? 0))]);

    private static WorkspaceLayout Layout(TemporaryWorkspace temporary, string root)
        => WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), root);

    private static SourceLogicalSource Source(TemporaryWorkspace temporary, string path)
        => new(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new SourceLayer(path, temporary.Combine(path), SourceDocumentForm.Markdown, SourceLayerKind.Base));

    private static async Task<SourceReadSession> Read(WorkspaceLayout layout, bool reverseScan, bool retainEdges = true)
    {
        var paths = new PhysicalPathResolver();
        var catalogue = await new SourceCatalogueReader(paths).ReadAsync(new SourceCatalogueRequest(layout,
            reverseScan ? layout.InventoryRoots.Reverse().ToArray() : layout.InventoryRoots), TestContext.Current.CancellationToken);
        var reader = new SourceDocumentReader(layout.Workspace, paths);
        if (!retainEdges) return new(catalogue, reader, []);
        var included = await new SourceIncludedEntryReader().ReadEntriesAsync(catalogue, reader, TestContext.Current.CancellationToken);
        return new(SourceIncludedEntryReader.Extend(catalogue, included.Sources, included.Rows), reader, []);
    }

    private static void AssertRows(IReadOnlyList<SourceIncludedEntryRow> expected, IReadOnlyList<SourceIncludedEntryRow> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        foreach (var (left, right) in expected.Zip(actual))
        {
            Assert.Equal((left.Path, left.ParentPath, left.AliasOf, left.PhysicalState), (right.Path, right.ParentPath, right.AliasOf, right.PhysicalState));
            Assert.Equal(left.Entry.Description, right.Entry.Description);
            Assert.Equal(left.Entry.Destination, right.Entry.Destination);
            Assert.Equal(left.Entry.Tags, right.Entry.Tags);
            Assert.Equal(left.Entry.ApplyTo.Select(pattern => pattern.Text), right.Entry.ApplyTo.Select(pattern => pattern.Text));
            Assert.Equal(left.Entry.Span, right.Entry.Span);
        }
    }
}
