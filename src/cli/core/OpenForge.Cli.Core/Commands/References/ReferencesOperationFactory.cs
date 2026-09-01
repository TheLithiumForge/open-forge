using OpenForge.Cli.Core.Commands.References.Shared.Documents.Parsing;
using OpenForge.Cli.Core.Commands.References.Shared.Inspection;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;

namespace OpenForge.Cli.Core.Commands.References;

internal static class ReferencesOperationFactory
{
    internal static ReferencesOperation Create()
    {
        var physicalPathResolverImplementation = new PhysicalPathResolver();
        var sourceSessionReader = new SourceReadSessionReader(physicalPathResolverImplementation);
        var neutralPhysicalPathResolver = CreateNeutralPhysicalPathResolver(physicalPathResolverImplementation);
        var physicalPathResolver = CreatePhysicalPathResolver(physicalPathResolverImplementation);
        var sourceReferenceResolver = new SourceReferenceResolver(neutralPhysicalPathResolver);
        var universeFilterResolver = new SourceUniverseFilterResolver(sourceReferenceResolver);
        ReferencesLayerReader layerReader = static (reader, layer, cancellationToken) =>
            reader.ReadAsync(layer, cancellationToken);
        ReferencesMarkdownParser markdownParser = new MarkdownDocumentParser().Parse;
        ReferencesStrictUtf8Reader strictUtf8Reader = StrictUtf8FileReader.ReadAsync;
        var sourceResolver = new ReferencesSourceResolver(
            sourceSessionRead: sourceSessionReader.ReadAsync,
            sourceReferenceResolver: sourceReferenceResolver,
            universeFilterResolver: universeFilterResolver);
        var layerInspector = new ReferencesLayerInspector(
            layerReader: layerReader,
            markdownParser: markdownParser);
        var destinationResolver = new ReferencesDestinationResolver(
            physicalPathResolver: physicalPathResolver,
            strictUtf8Reader: strictUtf8Reader,
            markdownParser: markdownParser);
        var resultBuilder = new ReferencesResultBuilder();
        return new ReferencesOperation(
            sourceResolver: sourceResolver,
            layerInspector: layerInspector,
            destinationResolver: destinationResolver,
            resultBuilder: resultBuilder);
    }

    private static SourcePhysicalPathResolver CreateNeutralPhysicalPathResolver(
        PhysicalPathResolver physicalPathResolver)
        => (workspace, canonicalPath) => physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath));

    private static ReferencesPhysicalPathResolver CreatePhysicalPathResolver(
        PhysicalPathResolver physicalPathResolver)
        => (workspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
}
