using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListDirectoryBoundary(
    string CanonicalLogicalPath,
    IReadOnlyList<string> AncestorPhysicalPaths);

internal sealed class RouteListInventoryAccumulator
{
    private readonly Dictionary<string, string> _firstLogicalByPhysical = new(PhysicalIdentityTracker.PathComparer);
    private readonly HashSet<string> _aliasedLogicalPaths = new(StringComparer.Ordinal);

    internal List<RouteSourceDocument> Files { get; } = [];

    internal List<RouteListFilesystemFinding> Findings { get; } = [];

    internal List<RouteListPhysicalAlias> Aliases { get; } = [];

    internal List<RouteOverwriteFact> OverwriteFacts { get; } = [];

    internal void RegisterIdentity(string canonicalLogicalPath, string physicalPath)
    {
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!_firstLogicalByPhysical.TryGetValue(normalizedPhysicalPath, out var firstLogicalPath))
        {
            _firstLogicalByPhysical.Add(normalizedPhysicalPath, canonicalLogicalPath);
            return;
        }

        if (!string.Equals(firstLogicalPath, canonicalLogicalPath, StringComparison.Ordinal)
            && _aliasedLogicalPaths.Add(canonicalLogicalPath))
        {
            Aliases.Add(new RouteListPhysicalAlias(
                canonicalLogicalPath,
                normalizedPhysicalPath,
                firstLogicalPath));
        }
    }
}
