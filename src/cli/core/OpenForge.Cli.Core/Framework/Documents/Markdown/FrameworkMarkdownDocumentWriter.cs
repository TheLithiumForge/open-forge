using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal sealed class FrameworkMarkdownDocumentWriter
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly FrameworkDocumentMetadataEmitter _metadataEmitter = new();

    internal ImmutableArray<byte> Write(
        FrameworkDocumentMetadata metadata,
        string body)
    {
        var yaml = _metadataEmitter.Emit(metadata);
        var document = $"---\n{yaml}---\n{body}";
        return ImmutableArray.CreateRange(StrictUtf8.GetBytes(document));
    }
}
