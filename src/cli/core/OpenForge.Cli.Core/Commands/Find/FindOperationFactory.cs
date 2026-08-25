using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Find;

internal static class FindOperationFactory
{
    internal static FindOperation Create()
    {
        var physicalPathResolverImplementation = new PhysicalPathResolver();
        var routeFactsResolver = new SourceRouteFactsResolver();
        FindSourceBoundaryReader sourceBoundaryReader = async (workspace, cancellationToken) =>
        {
            var documentReader = new SourceDocumentReader(workspace);
            var catalogue = await new SourceCatalogueReader()
                .ReadAsync(
                    new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
                    cancellationToken)
                .ConfigureAwait(false);
            var agentsResolution = physicalPathResolverImplementation.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, SourceLogicalPath.AgentsRoot));
            var defaultSelectionScope = agentsResolution.State == PhysicalPathState.Contained
                ? new SourceCatalogueSelectionScope(
                    SourceLogicalPath.AgentsRoot,
                    agentsResolution.GetContainedPhysicalPath())
                : null;
            return new FindSourceReadContext(catalogue, documentReader, defaultSelectionScope);
        };
        FindPhysicalPathResolver physicalPathResolver = (workspace, canonicalPath) =>
            physicalPathResolverImplementation.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath));
        FindSelectedLayerReader selectedLayerReader = static (reader, layer, cancellationToken) =>
            reader.ReadAsync(layer, cancellationToken);
        FindRouteFactsReader routeFactsReader = (request, reader, cancellationToken) =>
            routeFactsResolver.ResolveAsync(request, reader, cancellationToken);
        FindMarkdownDocumentReader markdownDocumentReader = new MarkdownDocumentParser().Parse;
        FindFrontmatterFactsReader frontmatterFactsReader = new FindFrontmatterReader().Read;
        var components = new FindOperationComponents
        {
            SourceBoundaryReader = sourceBoundaryReader,
            PhysicalPathResolver = physicalPathResolver,
            SelectedLayerReader = selectedLayerReader,
            RouteFactsReader = routeFactsReader,
            MarkdownDocumentReader = markdownDocumentReader,
            FrontmatterFactsReader = frontmatterFactsReader,
            ResultBuilder = new FindResultBuilder(),
        };
        return new(components);
    }
}
