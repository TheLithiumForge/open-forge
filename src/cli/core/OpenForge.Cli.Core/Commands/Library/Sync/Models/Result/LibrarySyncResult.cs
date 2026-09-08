using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;

internal sealed record LibrarySyncResult : ICliCommandResult
{
    public string Command => LibrarySyncDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibrarySyncPayload Result { get; init; }
    public required CliNextAction? Next { get; init; }
}
