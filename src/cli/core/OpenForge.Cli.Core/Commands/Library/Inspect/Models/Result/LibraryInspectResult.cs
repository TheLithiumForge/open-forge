using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectResult : ICliCommandResult
{
    public string Command => LibraryInspectDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required LibraryInspectPayload Result { get; init; }
    public CliNextAction? Next => null;
}
