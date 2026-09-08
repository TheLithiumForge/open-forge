using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Request;

internal sealed record LibraryListRequest
{
    public required CliWorkspace Workspace { get; init; }
}
