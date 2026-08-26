using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References.Models.Operation;

internal delegate ValueTask<ReferencesSourceReadContext> ReferencesSourceBoundaryReader(
    CliWorkspace workspace,
    CancellationToken cancellationToken);

internal delegate ValueTask<SourceDocumentReadResult> ReferencesLayerReader(
    SourceDocumentReader reader,
    SourceLayer layer,
    CancellationToken cancellationToken);

internal delegate MarkdownDocumentFacts ReferencesMarkdownParser(string source);

internal delegate PhysicalPathResolution ReferencesPhysicalPathResolver(
    CliWorkspace workspace,
    string lexicalPath);

internal delegate ValueTask<FileReadResult<string>> ReferencesStrictUtf8Reader(
    string physicalPath,
    string logicalPath,
    CancellationToken cancellationToken);

internal sealed class ReferencesSourceReadContext
{
    internal ReferencesSourceReadContext(
        SourceCatalogue catalogue,
        SourceDocumentReader documentReader,
        SourceCatalogueSelectionScope? defaultSelectionScope)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(documentReader);
        Catalogue = catalogue;
        DocumentReader = documentReader;
        DefaultSelectionScope = defaultSelectionScope;
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceDocumentReader DocumentReader { get; }

    internal SourceCatalogueSelectionScope? DefaultSelectionScope { get; }
}

internal sealed class ReferencesOperationComponents
{
    internal required ReferencesSourceBoundaryReader SourceBoundaryReader { get; init; }

    internal required SourceReferenceResolver SourceReferenceResolver { get; init; }

    internal required SourceUniverseFilterResolver UniverseFilterResolver { get; init; }

    internal required ReferencesLayerReader LayerReader { get; init; }

    internal required ReferencesMarkdownParser MarkdownParser { get; init; }

    internal required ReferencesPhysicalPathResolver PhysicalPathResolver { get; init; }

    internal required ReferencesStrictUtf8Reader StrictUtf8Reader { get; init; }

    internal required ReferencesResultBuilder ResultBuilder { get; init; }
}
