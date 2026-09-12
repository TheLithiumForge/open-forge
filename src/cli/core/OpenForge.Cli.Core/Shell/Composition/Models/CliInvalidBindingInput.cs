using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

internal sealed record CliInvalidBindingInput(
    CliInvalidInput InvalidInput,
    CliGlobalInput GlobalInput,
    CliProcessEnvironment ProcessEnvironment,
    CliBindingParse BindingParse)
{
    internal CliWorkspaceSelectionState? WorkspaceSelectionState { get; init; }
}
