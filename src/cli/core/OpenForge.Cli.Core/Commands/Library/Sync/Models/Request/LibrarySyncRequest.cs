using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;

internal sealed record LibrarySyncRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required LibraryId LibraryId { get; init; }

    public required LibraryMode Mode { get; init; }
}
