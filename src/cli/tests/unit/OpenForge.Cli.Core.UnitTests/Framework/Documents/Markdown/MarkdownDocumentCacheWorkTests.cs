using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

[Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class MarkdownDocumentCacheWorkTests
{
    [Fact(DisplayName = "Concurrent content readers parse identical Markdown once")]
    public async Task ConcurrentReadersShareOneParse()
    {
        var cache = new MarkdownDocumentCache();
        var parses = 0;
        using var start = new Barrier(8);
        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Factory.StartNew(() =>
        {
            start.SignalAndWait(TestContext.Current.CancellationToken);
            return cache.Parse(text =>
            {
                Interlocked.Increment(ref parses);
                return new MarkdownDocumentParser().Parse(text);
            }, "# Same content\n");
        }, TestContext.Current.CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();

        MarkdownDocumentFacts[] documents = await Task.WhenAll(tasks);
        Assert.Equal(1, parses);
        Assert.All(documents, document => Assert.Same(documents[0], document));
    }

    [Fact(DisplayName = "A failed parse does not suppress a later successful content read")]
    public void FailedParseIsNotRetained()
    {
        var cache = new MarkdownDocumentCache();
        var parses = 0;
        MarkdownDocumentFacts Parse(string text)
        {
            if (++parses == 1) throw new IOException("First parse unavailable.");
            return new MarkdownDocumentParser().Parse(text);
        }
        Assert.Throws<IOException>(() => cache.Parse(Parse, "# Content\n"));
        var document = cache.Parse(Parse, "# Content\n");
        Assert.Same(document, cache.Parse(Parse, "# Content\n"));
        Assert.Equal(2, parses);
    }
}
