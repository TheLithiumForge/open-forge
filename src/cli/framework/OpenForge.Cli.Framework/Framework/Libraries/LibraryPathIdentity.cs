using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Libraries;

internal static class LibraryPathIdentity
{
    internal static ImmutableArray<LibraryMapping> Mappings(LibraryRegistration library)
        => [.. library.Paths.Select(path => Map(library.SourceRoot, library.DestinationRoot, path))];

    internal static LibraryMapping Map(
        WorkspaceRelativeDirectory sourceRoot,
        LibraryDestinationRoot destinationRoot,
        SourceRelativeEligiblePath sourcePath)
        => LibraryMapping.Create(sourceRoot, destinationRoot, sourcePath);

}
