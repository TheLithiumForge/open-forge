using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

internal sealed record CliInvalidBindingInput(
    CliInvalidInput InvalidInput,
    CliGlobalInput GlobalInput,
    CliProcessEnvironment ProcessEnvironment,
    CliBindingParse BindingParse)
{
    internal CliWorkspaceSelectionState? WorkspaceSelectionState { get; init; }
}
