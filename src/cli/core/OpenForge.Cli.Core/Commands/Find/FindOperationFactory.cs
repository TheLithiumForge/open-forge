using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Selection;

namespace OpenForge.Cli.Core.Commands.Find;

internal static class FindOperationFactory
{
    internal static FindOperation Create()
    {
        var physicalPathResolverImplementation = new PhysicalPathResolver();
        var routeFactsResolver = new SourceRouteFactsResolver();
        var sourceSessionReader = new SourceReadSessionReader(physicalPathResolverImplementation);
        var physicalPathResolver = CreatePhysicalPathResolver(physicalPathResolverImplementation);
        var universeFilterResolver = new SourceUniverseFilterResolver(
            new SourceReferenceResolver(physicalPathResolver));
        FindSelectedLayerReader selectedLayerReader = static (reader, layer, cancellationToken) =>
            reader.ReadAsync(layer, cancellationToken);
        FindRouteFactsReader routeFactsReader = (request, reader, cancellationToken) =>
            routeFactsResolver.ResolveAsync(request, reader, cancellationToken);
        FindMarkdownDocumentReader markdownDocumentReader = new MarkdownDocumentParser().Parse;
        FindFrontmatterFactsReader frontmatterFactsReader = new FindFrontmatterReader().Read;
        var sourceResolver = new FindSourceResolver(
            sourceSessionRead: sourceSessionReader.ReadAsync,
            universeResolver: new FindUniverseResolver(universeFilterResolver),
            physicalPathResolver: physicalPathResolver,
            routeFactsReader: routeFactsReader);
        var layerInspector = new FindLayerInspector(
            selectedLayerReader: selectedLayerReader,
            markdownDocumentReader: markdownDocumentReader,
            frontmatterFactsReader: frontmatterFactsReader,
            bodyTagScanner: new FindBodyTagScanner());
        var matcher = new FindMatcher();
        var projectionBuilder = new FindProjectionBuilder();
        var resultBuilder = new FindResultBuilder();
        return new FindOperation(
            sourceResolver: sourceResolver,
            layerInspector: layerInspector,
            matcher: matcher,
            projectionBuilder: projectionBuilder,
            resultBuilder: resultBuilder);
    }

    private static SourcePhysicalPathResolver CreatePhysicalPathResolver(
        PhysicalPathResolver physicalPathResolver)
        => (workspace, canonicalPath) => physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath));
}
