using OpenForge.Cli.Core.Commands.Shared.NavigationSharing.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.NavigationSharing;

internal static class NavigationSharingReader
{
    internal static async ValueTask<NavigationSharingRead> ReadAsync(CliWorkspace workspace, CancellationToken token)
        => FromOwnership(await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace, token).ConfigureAwait(false));

    internal static NavigationSharingRead FromOwnership(WorkspaceOwnershipRead ownership)
        => new(ownership, ownership.State is WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete
            ? new SourceSharing(ownership.Document.Framework?.GitIgnoredRoutes ?? []) : null);
}
