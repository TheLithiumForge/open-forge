using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Libraries;

internal static class LibraryPathIdentity
{
    internal static ImmutableArray<LibraryMapping> Mappings(LibraryRecord library)
        => [.. library.Paths.Select(path => Map(library.SourceRoot, library.DestinationRoot, path))];

    internal const string RecordRelativePath = ".agents/open-forge.libraries.json";

    internal static LibraryMapping Map(
        WorkspaceRelativeDirectory sourceRoot,
        LibraryDestinationRoot destinationRoot,
        SourceRelativeEligiblePath sourcePath)
        => LibraryMapping.Create(sourceRoot, destinationRoot, sourcePath);

    internal static string RecordPath(string workspacePhysicalRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePhysicalRoot);
        return Path.GetFullPath(Path.Combine(
            workspacePhysicalRoot,
            RecordRelativePath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
