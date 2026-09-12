using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;

internal sealed record LibrarySourceRootRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required WorkspaceRelativeDirectory SourceRoot { get; init; }
}

internal sealed record LibrarySourceRootObservation
{
    public required LibrarySourceRootRequest Request { get; init; }
    public required LibrarySourceRootState State { get; init; }
    public required string? LexicalSourceRoot { get; init; }
    public required string? PhysicalSourceRoot { get; init; }
    public required bool? LexicallyContained { get; init; }
    public required bool? PhysicallyContained { get; init; }
    public required string? Cause { get; init; }
}


internal enum LibraryInventoryExclusionKind
{
    Loader,
    Entrypoint,
    Overwrite,
    ManagerControl,
    Link,
    ReparsePoint,
    Special,
}

internal sealed record LibraryInventoryExclusion
{
    public required string Path { get; init; }
    public required LibraryInventoryExclusionKind Kind { get; init; }
}

internal sealed record LibraryInventoryUnavailablePath
{
    public required string Path { get; init; }
    public required string Cause { get; init; }
}

internal sealed record LibraryInventoryRead
{
    public required LibrarySourceRootObservation Source { get; init; }
    public required LibraryInventory? Inventory { get; init; }
    public required ImmutableArray<LibraryInventoryExclusion> ExcludedPaths { get; init; }
    public required ImmutableArray<LibraryInventoryUnavailablePath> UnavailablePaths { get; init; }
}
