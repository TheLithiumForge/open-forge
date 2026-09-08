using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;

internal sealed record LibraryInspectRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required LibraryId LibraryId { get; init; }
}
