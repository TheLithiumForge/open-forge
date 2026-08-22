using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal static class RouteListInventoryFindingBuilder
{
    internal static void AddMetadataFinding(
        string subject,
        RouteSourceMetadata metadata,
        ICollection<RouteListFilesystemFinding> findings)
    {
        if (metadata.State == RouteSourceMetadataState.Missing)
        {
            findings.Add(RouteListFilesystemFindingPolicy.MetadataMissing(subject));
        }
        else if (metadata.State == RouteSourceMetadataState.Malformed)
        {
            findings.Add(RouteListFilesystemFindingPolicy.MetadataMalformed(subject));
        }
    }

    internal static void AddIdentityCollisionFindings(
        IEnumerable<RouteListInventorySource> sources,
        ICollection<RouteListFilesystemFinding> findings)
    {
        var firstSourceByPhysical = new Dictionary<string, string>(PhysicalIdentityTracker.PathComparer);
        foreach (var source in sources.OrderBy(item => item.Source.CanonicalPath, StringComparer.Ordinal))
        {
            if (!firstSourceByPhysical.TryAdd(source.Source.PhysicalPath, source.Source.CanonicalPath))
            {
                findings.Add(RouteListFilesystemFindingPolicy.IdentityCollision(source.Source.CanonicalPath));
            }
        }
    }
}
