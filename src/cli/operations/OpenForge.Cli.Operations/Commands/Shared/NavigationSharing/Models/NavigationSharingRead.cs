using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Shared.NavigationSharing.Models;

internal sealed record NavigationSharingRead(WorkspaceOwnershipRead Ownership, SourceSharing? Sharing)
{
    internal bool IsAvailable => Sharing is not null;

    internal string Cause => "The route-sharing lock is invalid or unreadable. Restore it before changing shared navigation.";
}
