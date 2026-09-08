using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;

internal enum LibrarySourceRootState
{
    Available,
    Missing,
    Invalid,
    Inaccessible,
    Blocked,
    Unavailable,
}

internal enum LibraryInventoryState
{
    Complete,
    Incomplete,
    Blocked,
    Unavailable,
}

internal sealed record EligibleSourceFile
{
    private EligibleSourceFile(
        SourceRelativeEligiblePath sourcePath,
        string physicalPath)
    {
        SourcePath = sourcePath;
        PhysicalPath = Path.GetFullPath(physicalPath);
    }

    internal SourceRelativeEligiblePath SourcePath { get; }

    internal string PhysicalPath { get; }

    internal static EligibleSourceFile Create(
        SourceRelativeEligiblePath sourcePath,
        string physicalPath)
    {
        ArgumentNullException.ThrowIfNull(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        return new EligibleSourceFile(sourcePath, physicalPath);
    }
}

internal sealed record LibraryInventory
{
    private LibraryInventory(
        WorkspaceRelativeDirectory sourceRoot,
        string sourceAgentsDirectory,
        LibrarySourceRootState rootState,
        LibraryInventoryState state,
        ImmutableArray<EligibleSourceFile> entries,
        string? cause)
    {
        SourceRoot = sourceRoot;
        SourceAgentsDirectory = sourceAgentsDirectory;
        RootState = rootState;
        State = state;
        Entries = entries;
        Cause = cause;
    }

    internal WorkspaceRelativeDirectory SourceRoot { get; }

    internal string SourceAgentsDirectory { get; }

    internal LibrarySourceRootState RootState { get; }

    internal LibraryInventoryState State { get; }

    internal ImmutableArray<EligibleSourceFile> Entries { get; }

    internal string? Cause { get; }

    internal static LibraryInventory Complete(
        WorkspaceRelativeDirectory sourceRoot,
        string sourceAgentsDirectory,
        IReadOnlyList<EligibleSourceFile> entries)
        => Create(
            sourceRoot,
            sourceAgentsDirectory,
            LibrarySourceRootState.Available,
            LibraryInventoryState.Complete,
            entries,
            cause: null);

    internal static LibraryInventory Classified(
        WorkspaceRelativeDirectory sourceRoot,
        string sourceAgentsDirectory,
        LibrarySourceRootState rootState,
        LibraryInventoryState state,
        string cause,
        IReadOnlyList<EligibleSourceFile>? entries = null)
    {
        if (state == LibraryInventoryState.Complete)
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return Create(
            sourceRoot,
            sourceAgentsDirectory,
            rootState,
            state,
            entries ?? [],
            cause);
    }

    private static LibraryInventory Create(
        WorkspaceRelativeDirectory sourceRoot,
        string sourceAgentsDirectory,
        LibrarySourceRootState rootState,
        LibraryInventoryState state,
        IReadOnlyList<EligibleSourceFile> entries,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(sourceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAgentsDirectory);
        ArgumentNullException.ThrowIfNull(entries);
        if (!Enum.IsDefined(rootState) || !Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        var values = ImmutableArray.CreateBuilder<EligibleSourceFile>(entries.Count);
        string? previous = null;
        foreach (var entry in entries)
        {
            ArgumentNullException.ThrowIfNull(entry);
            if (previous is not null
                && string.CompareOrdinal(previous, entry.SourcePath.Value) >= 0)
            {
                throw new ArgumentException(
                    "Library inventory entries must be unique and in ordinal path order.",
                    nameof(entries));
            }

            values.Add(entry);
            previous = entry.SourcePath.Value;
        }

        return new LibraryInventory(
            sourceRoot,
            Path.GetFullPath(sourceAgentsDirectory),
            rootState,
            state,
            values.MoveToImmutable(),
            cause);
    }
}
