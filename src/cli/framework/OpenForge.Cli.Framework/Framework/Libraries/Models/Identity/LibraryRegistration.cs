using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

internal sealed record LibraryRegistration
{
    private LibraryRegistration(
        LibraryId id,
        WorkspaceRelativeDirectory sourceRoot,
        LibraryDestinationRoot destinationRoot,
        ImmutableArray<SourceRelativeEligiblePath> paths)
    {
        Id = id;
        SourceRoot = sourceRoot;
        DestinationRoot = destinationRoot;
        Paths = paths;
    }

    internal LibraryId Id { get; }

    internal WorkspaceRelativeDirectory SourceRoot { get; }

    internal LibraryDestinationRoot DestinationRoot { get; }

    internal ImmutableArray<SourceRelativeEligiblePath> Paths { get; }

    internal static LibraryRegistration Create(
        LibraryId id,
        WorkspaceRelativeDirectory sourceRoot,
        LibraryDestinationRoot destinationRoot,
        IReadOnlyList<SourceRelativeEligiblePath> paths)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(sourceRoot);
        ArgumentNullException.ThrowIfNull(destinationRoot);
        ArgumentNullException.ThrowIfNull(paths);
        var values = ImmutableArray.CreateBuilder<SourceRelativeEligiblePath>(paths.Count);
        string? previous = null;
        foreach (var path in paths)
        {
            ArgumentNullException.ThrowIfNull(path);
            if (previous is not null
                && string.CompareOrdinal(previous, path.Value) >= 0)
            {
                throw new ArgumentException(
                    "Library paths must be unique and in ordinal order.",
                    nameof(paths));
            }

            values.Add(path);
            previous = path.Value;
        }

        return new LibraryRegistration(id, sourceRoot, destinationRoot, values.MoveToImmutable());
    }
}
