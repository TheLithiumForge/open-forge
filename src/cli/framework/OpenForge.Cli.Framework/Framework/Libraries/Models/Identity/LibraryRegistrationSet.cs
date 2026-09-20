using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

internal sealed record LibraryRegistrationSet
{
    private LibraryRegistrationSet(ImmutableArray<LibraryRegistration> libraries)
    {
        Libraries = libraries;
    }

    internal ImmutableArray<LibraryRegistration> Libraries { get; }

    internal static LibraryRegistrationSet Create(IReadOnlyList<LibraryRegistration> libraries)
    {
        ArgumentNullException.ThrowIfNull(libraries);
        var values = ImmutableArray.CreateBuilder<LibraryRegistration>(libraries.Count);
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

        return new LibraryRegistrationSet(values.MoveToImmutable());
    }
}
