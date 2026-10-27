using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context.Shared.Selection;

[Trait("Boundary", "Processing"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
public sealed class ContextOutsideClosureTests
{
    private const string Directory = "docs/team";
    private const string Root = Directory + "/catalogue" + SourceFormClassifier.MarkdownExtension;
    private const string Nested = Directory + "/nested" + SourceFormClassifier.MarkdownExtension;
    private const string Leaf = Directory + "/leaf" + SourceFormClassifier.MarkdownExtension;

    [Theory, InlineData(false), InlineData(true)]
    public void ExplicitCatalogueTraversesNestedLinkedCataloguesWithoutStartupMembership(bool selected)
    {
        var physicalRoot = Path.GetFullPath("context-outside-unit");
        var layout = WorkspaceLayout.Create(new CliWorkspace(physicalRoot, physicalRoot, CliWorkspaceSelectionMethod.CurrentDirectory),
            WorkspaceLayout.DefaultMainRoot).WithOutsideRoutes([Directory]);
        var workspace = Source(SourceLogicalPath.WorkspaceEntryPath, SourceDocumentForm.Markdown, null, string.Empty);
        var loader = Source(layout.LoaderPath, SourceDocumentForm.Loader, null, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var root = Source(Root, SourceDocumentForm.Markdown, layout.LoaderPath, Catalogue(Path.GetFileName(Nested)));
        var nested = Source(Nested, SourceDocumentForm.Markdown, Root, Catalogue(Path.GetFileName(Leaf)));
        var leaf = Source(Leaf, SourceDocumentForm.Markdown, Nested, "# Leaf\n");
        var sources = new[] { loader, root, nested, leaf };
        var catalogue = new SourceCatalogue(layout, [], sources.Select(source => source.LogicalSource ?? throw new InvalidOperationException()), [], false);
        var observation = new WorkspaceLayoutRead(WorkspaceLayoutReadState.Ready,
            WorkspaceSettingsRead.Absent(Path.Combine(physicalRoot, WorkspaceSettingsDefinitions.RelativePath)),
            WorkspaceOwnershipRead.Absent(Path.Combine(physicalRoot, WorkspaceOwnershipDefinitions.RelativePath)), layout, [], []);
        var graph = new ContextGraph(observation, catalogue, new SourceRouteFacts(new SourceRouteTopology([], []), [], [], true, false), workspace, sources);
        var reference = SourceWorkspaceRelativePath.LiteralReferencePrefix + Root;
        ContextResolvedRequest[] requests = selected ? [new ContextResolvedRequest
        {
            Reference = reference,
            Source = root,
            Resolution = new SourceReferenceResolution(value: reference, form: SourceReferenceKind.SourcePath,
                state: SourceReferenceResolutionState.Resolved, source: root.LogicalSource, candidates: [], canonicalPath: Root, cause: null),
            Result = new ContextRequestedSource(reference, SourceReferenceKind.SourcePath, SourceReferenceResolutionState.Resolved,
                new ContextSourceIdentity(null, Root), ContextRouteState.Unrouted, []),
        }] : [];

        var result = new ContextLoadingClosureResolver().Resolve(graph, requests, new ContextSelectionAccumulator());

        Assert.False(result.Blocked);
        Assert.False(result.Incomplete);
        Assert.Equal(new[] { SourceLogicalPath.WorkspaceEntryPath, layout.LoaderPath }, result.Selection.StartupSources.Select(source => source.Source.CanonicalPath));
        var expected = selected ? new[] { SourceLogicalPath.WorkspaceEntryPath, layout.LoaderPath, Root, Nested, Leaf }
            : [SourceLogicalPath.WorkspaceEntryPath, layout.LoaderPath];
        Assert.Equal(expected, result.Selection.CombinedSources.Select(source => source.Source.CanonicalPath));

        ContextGraphSource Source(string path, SourceDocumentForm form, string? parent, string text)
        {
            var physical = Path.Combine(physicalRoot, path.Replace('/', Path.DirectorySeparatorChar));
            var layer = new SourceLayer(path, physical, form, SourceLayerKind.Base);
            var identity = new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path);
            var document = new MarkdownDocumentParser().Parse(text);
            return new ContextGraphSource(layout: layout, logicalSource: new SourceLogicalSource(identity, layer), id: null,
                canonicalPath: path, form: form, routeState: SourceRouteState.Unrouted, route: null,
                metadata: SourceAuthoredMetadataFacts.Complete("Source", []), generatedEntries: SourceGeneratedEntriesParser.Parse(document),
                layers: [new ContextGraphLayer
                {
                    Kind = SourceLayerKind.Base,
                    CanonicalPath = path,
                    PhysicalPath = physical,
                    ReadState = FileReadState.Complete,
                    Text = text,
                    Document = document,
                }])
            { LinkedParentPath = parent };
        }
    }

    private static string Catalogue(string destination)
        => OpenForgeDocumentSeed.GeneratedEntries($"- [Child]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}");
}
