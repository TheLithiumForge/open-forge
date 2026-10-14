using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal sealed record RoutedMetadataCompletionInput
{
    public required RoutedMetadataFileFacts File { get; init; }
    public required ImmutableArray<byte> Bytes { get; init; }
}
