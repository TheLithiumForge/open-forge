using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

internal sealed record LocalReferenceSourceObservation(
    string Path,
    string? Id,
    IReadOnlyList<LocalReferenceParsedLayer> Layers);

internal sealed record LocalReferenceParsedLayer(
    string Path,
    MarkdownDocumentFacts Document,
    Utf8SourceMap Locations);
