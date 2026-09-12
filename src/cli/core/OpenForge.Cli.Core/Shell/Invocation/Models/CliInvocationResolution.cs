using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Shell.Invocation.Models;

internal sealed record CliInvocationResolution(
    CliInvocation? Invocation,
    CliInvalidInput? InvalidInput)
{
    internal CliWorkspaceSelectionState? WorkspaceSelectionState { get; init; }
}
