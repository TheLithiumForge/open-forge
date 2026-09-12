using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectResult : ICliCommandResult
{
    public string Command => LibraryInspectDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibraryInspectPayload Result { get; init; }
    public CliNextAction? Next => null;
}
