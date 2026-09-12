using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListResult : ICliCommandResult
{
    public string Command => LibraryListDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibraryListPayload Result { get; init; }
    public CliNextAction? Next => null;
}
