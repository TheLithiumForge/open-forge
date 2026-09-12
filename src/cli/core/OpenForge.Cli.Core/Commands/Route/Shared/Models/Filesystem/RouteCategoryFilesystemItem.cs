using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;

internal enum RouteCategoryFilesystemItemKind
{
    Directory,
    Entrypoint,
    NativeSource,
    RoutedMarkdown,
    UnroutedMarkdown,
    Resource,
}

internal sealed record RouteCategoryFilesystemItem
{
    public required RouteCategoryFilesystemItemKind Kind { get; init; }

    public SourceLayerKind? Layer { get; init; }

    public string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string RelativePath { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}
