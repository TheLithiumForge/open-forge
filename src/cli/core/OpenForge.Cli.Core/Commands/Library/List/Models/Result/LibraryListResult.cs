using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListResult : ICliCommandResult
{
    public string Command => LibraryListDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibraryListPayload Result { get; init; }
    public CliNextAction? Next => null;
}
