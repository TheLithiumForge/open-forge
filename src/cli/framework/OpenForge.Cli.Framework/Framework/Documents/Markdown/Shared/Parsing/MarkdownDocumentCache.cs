using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using System.Collections.Concurrent;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;

internal sealed class MarkdownDocumentCache
{
    private readonly ConcurrentDictionary<string, Lazy<MarkdownDocumentFacts>> _documents = new(StringComparer.Ordinal);

    internal MarkdownDocumentFacts Parse(Func<string, MarkdownDocumentFacts> parser, string source)
    {
        var document = _documents.GetOrAdd(source, static (text, parse) => new(() => parse(text)), parser);
        try { return document.Value; }
        catch
        {
            _documents.TryRemove(source, out _);
            throw;
        }
    }

    internal void Retain(MarkdownDocumentFacts document)
        => _documents.GetOrAdd(document.Source, static (_, retained) => new(() => retained), document);
}
