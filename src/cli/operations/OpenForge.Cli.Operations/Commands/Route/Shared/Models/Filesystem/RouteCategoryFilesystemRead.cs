using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;

internal enum RouteCategoryFilesystemReadState
{
    Complete,
    Incomplete,
    Unsafe,
    Interrupted,
}

internal sealed record RouteCategoryFilesystemRead
{
    internal required RouteCategoryFilesystemReadState State { get; init; }

    internal ImmutableArray<RouteCategoryFilesystemItem> Items { get; init; } = [];

    internal string? Cause { get; init; }
}
