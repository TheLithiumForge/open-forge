using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal sealed record RoutedMetadataCompletionInput
{
    public required RoutedMetadataFileFacts File { get; init; }
    public required ImmutableArray<byte> Bytes { get; init; }
    public MarkdownDocumentFacts? Document { get; init; }
}
