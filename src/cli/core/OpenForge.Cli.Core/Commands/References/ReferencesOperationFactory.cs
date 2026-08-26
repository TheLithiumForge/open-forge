using OpenForge.Cli.Core.Commands.References.Models.Operation;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References;

internal static class ReferencesOperationFactory
{
    internal static ReferencesOperation Create()
    {
        var physicalPathResolverImplementation = new PhysicalPathResolver();
        ReferencesSourceBoundaryReader sourceBoundaryReader = async (workspace, cancellationToken) =>
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
            return new ReferencesSourceReadContext(catalogue, documentReader, defaultSelectionScope);
        };
        SourcePhysicalPathResolver neutralPhysicalPathResolver = (workspace, canonicalPath) =>
            physicalPathResolverImplementation.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath));
        ReferencesPhysicalPathResolver physicalPathResolver = (workspace, lexicalPath) =>
            physicalPathResolverImplementation.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalPath);
        var sourceReferenceResolver = new SourceReferenceResolver(neutralPhysicalPathResolver);
        var universeFilterResolver = new SourceUniverseFilterResolver(sourceReferenceResolver);
        ReferencesLayerReader layerReader = static (reader, layer, cancellationToken) =>
            reader.ReadAsync(layer, cancellationToken);
        ReferencesMarkdownParser markdownParser = new MarkdownDocumentParser().Parse;
        ReferencesStrictUtf8Reader strictUtf8Reader = StrictUtf8FileReader.ReadAsync;
        return new ReferencesOperation(new ReferencesOperationComponents
        {
            SourceBoundaryReader = sourceBoundaryReader,
            SourceReferenceResolver = sourceReferenceResolver,
            UniverseFilterResolver = universeFilterResolver,
            LayerReader = layerReader,
            MarkdownParser = markdownParser,
            PhysicalPathResolver = physicalPathResolver,
            StrictUtf8Reader = strictUtf8Reader,
            ResultBuilder = new ReferencesResultBuilder(),
        });
    }
}
