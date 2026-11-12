using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

internal sealed record LocalReferenceLayerInput(
    SourceCatalogue Catalogue,
    string Path,
    string RoutePath,
    MarkdownDocumentFacts Document,
    Utf8SourceMap Locations);
