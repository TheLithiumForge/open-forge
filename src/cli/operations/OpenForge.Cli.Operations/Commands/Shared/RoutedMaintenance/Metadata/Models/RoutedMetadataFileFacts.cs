using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

// Root and ancestor entrypoints are selected observed or intended facts supplied by the maintenance caller.
// Directory names alone do not establish a root or a Memory state.
internal sealed record RoutedMetadataFileFacts
{
    public required string CanonicalPath { get; init; }
    public required string? LoaderRootEntrypointPath { get; init; }
    public required ImmutableArray<string> RouteEntrypointPaths { get; init; }
    public required bool IsSelected { get; init; }
    public required bool IsReadable { get; init; }
    public required bool IsSafeOrdinaryLeaf { get; init; }
    public required bool IsGeneratedFile { get; init; }
    public required bool HasManagedAuthoredRegions { get; init; }
}
