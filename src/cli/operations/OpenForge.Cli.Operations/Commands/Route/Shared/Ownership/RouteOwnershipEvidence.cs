using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Ownership;

internal static class RouteOwnershipEvidence
{
    internal static bool IsEstablished(WorkspaceOwnershipRead read)
        => read.State == WorkspaceOwnershipReadState.Complete && read.Document.Framework is not null
            && Claims(read).All(claim => PortableWorkspacePath.TryNormalize(claim.Path, out var normalized) && normalized == claim.Path);

    internal static IEnumerable<OwnedPath> Claims(WorkspaceOwnershipRead read)
        => (read.Document.Framework is { } framework
                ? framework.Paths.Concat(framework.Regions.Select(region => region.Path))
                    .Select(path => new OwnedPath(path, OwnedPathManager.Framework, framework.Source.Id)) : [])
            .Concat(read.Document.Extensions.SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path))
                .Select(path => new OwnedPath(path, OwnedPathManager.Extension, extension.Id))))
            .Distinct().OrderBy(claim => claim.Path, StringComparer.Ordinal)
            .ThenBy(claim => claim.Manager).ThenBy(claim => claim.Owner, StringComparer.Ordinal);

    internal static string Cause(WorkspaceOwnershipRead read)
        => read.Cause ?? "The ownership lock does not establish a complete inventory; no unmanaged state was inferred.";
}
