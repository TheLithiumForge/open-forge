using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;

internal sealed record LibraryDetachResult : ICliCommandResult
{
    public string Command => LibraryDetachDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibraryDetachPayload Result { get; init; }
    public required CliNextAction? Next { get; init; }
}
