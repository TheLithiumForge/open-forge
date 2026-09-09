using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Record;

internal sealed record LibraryRecord
{
    private LibraryRecord(
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

    internal static LibraryRecord Create(
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

        return new LibraryRecord(id, sourceRoot, destinationRoot, values.MoveToImmutable());
    }
}

internal sealed record LibrariesRecord
{
    internal const int CurrentSchemaVersion = 1;

    private LibrariesRecord(ImmutableArray<LibraryRecord> libraries)
    {
        SchemaVersion = CurrentSchemaVersion;
        Libraries = libraries;
    }

    internal int SchemaVersion { get; }

    internal ImmutableArray<LibraryRecord> Libraries { get; }

    internal static LibrariesRecord Create(IReadOnlyList<LibraryRecord> libraries)
    {
        ArgumentNullException.ThrowIfNull(libraries);
        var values = ImmutableArray.CreateBuilder<LibraryRecord>(libraries.Count);
        string? previous = null;
        var destinations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var library in libraries)
        {
            ArgumentNullException.ThrowIfNull(library);
            var id = library.Id.Value;
            if (previous is not null
                && string.CompareOrdinal(previous, id) >= 0)
            {
                throw new ArgumentException(
                    "Library records must be unique and in ordinal ID order.",
                    nameof(libraries));
            }

            values.Add(library);
            previous = id;
            foreach (var mapping in LibraryPathIdentity.Mappings(library))
            {
                if (!destinations.Add(PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value)))
                {
                    throw new ArgumentException(
                        "Library destination paths must be unique across the record.",
                        nameof(libraries));
                }
            }
        }

        return new LibrariesRecord(values.MoveToImmutable());
    }
}
