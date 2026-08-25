using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Documents;

internal delegate ValueTask<SourceDocumentReadResult> FindSelectedLayerReader(
    SourceDocumentReader reader,
    SourceLayer layer,
    CancellationToken cancellationToken);

internal delegate MarkdownDocumentFacts FindMarkdownDocumentReader(string source);

internal delegate FindFrontmatterFacts FindFrontmatterFactsReader(FindFrontmatterInput input);
