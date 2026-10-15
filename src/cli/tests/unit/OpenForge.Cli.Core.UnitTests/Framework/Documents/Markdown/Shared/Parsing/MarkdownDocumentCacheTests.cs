using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown.Shared.Parsing;

[Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class MarkdownDocumentCacheTests
{
    [Fact(DisplayName = "Repeated fragment targets and layer inspections share one Markdown parse per read session")]
    public void RepeatedDocumentsParseOnceAndRefreshBetweenSessions()
    {
        var parses = 0;
        Func<string, MarkdownDocumentFacts> parser = source =>
        {
            parses++;
            return new MarkdownDocumentParser().Parse(source);
        };
        var cache = new MarkdownDocumentCache();
        var document = cache.Parse(parser.Invoke, "# Target\n");
        for (var index = 0; index < 100; index++) Assert.Same(document, cache.Parse(parser.Invoke, "# Target\n"));
        Assert.Equal(1, parses);
        new MarkdownDocumentCache().Parse(parser.Invoke, "# Target\n");
        Assert.Equal(2, parses);
    }
}
