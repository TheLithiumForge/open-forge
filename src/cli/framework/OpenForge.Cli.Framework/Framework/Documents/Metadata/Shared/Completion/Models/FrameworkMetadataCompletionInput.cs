using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;

internal sealed record FrameworkMetadataCompletionInput
{
    public required ImmutableArray<byte> Bytes { get; init; }
    public required string CanonicalPath { get; init; }
    public required FrontmatterForm Form { get; init; }
    public required ImmutableArray<string> DefaultTags { get; init; }
    public MarkdownDocumentFacts? Document { get; init; }
}
