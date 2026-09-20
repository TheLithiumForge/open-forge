using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Request;

internal sealed record LibraryListRequest
{
    public required CliWorkspace Workspace { get; init; }
}
