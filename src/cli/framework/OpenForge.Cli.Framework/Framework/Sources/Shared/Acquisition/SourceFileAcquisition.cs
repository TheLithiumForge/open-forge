using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Acquisition;

internal sealed class SourceFileAcquisition
{
    private readonly Func<string, string, CancellationToken, ValueTask<FileReadResult<string>>> _read;
    private readonly Func<string, MarkdownDocumentFacts> _parse;
    private int _readCount;

    internal SourceFileAcquisition(MarkdownDocumentCache? documents = null,
        Func<string, string, CancellationToken, ValueTask<FileReadResult<string>>>? read = null,
        Func<string, MarkdownDocumentFacts>? parse = null)
    {
        Documents = documents ?? new();
        _read = read ?? StrictUtf8FileReader.ReadAsync;
        _parse = parse ?? new MarkdownDocumentParser().Parse;
    }

    internal MarkdownDocumentCache Documents { get; }
    internal int ReadCount => _readCount;

    internal ValueTask<FileReadResult<string>> ReadAsync(string physicalPath, string logicalPath, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _readCount);
        return _read(physicalPath, logicalPath, token);
    }

    internal MarkdownDocumentFacts ParseDocument(string source) => Documents.Parse(_parse, source);
    internal void RetainDocument(MarkdownDocumentFacts document) => Documents.Retain(document);
}
