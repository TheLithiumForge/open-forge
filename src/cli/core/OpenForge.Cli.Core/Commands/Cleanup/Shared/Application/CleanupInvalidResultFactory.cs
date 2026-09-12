using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;

internal static class CleanupInvalidResultFactory
{
    internal static CleanupResult Create(CliInvalidBindingInput input)
    {
        var cause = string.Join(" ", input.InvalidInput.Diagnostics);
        var result = new CleanupResultBuilder(CleanupPlan.NotEstablished());
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            result.AddFinding(ReadWorkspaceFinding(input.WorkspaceSelectionState), cause, input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory);
        }
        else
        {
            result.AddFinding(CleanupFindingCode.InvalidInput, cause);
        }

        return result.Build();
    }

    private static CleanupFindingCode ReadWorkspaceFinding(CliWorkspaceSelectionState? state)
        => state switch
        {
            CliWorkspaceSelectionState.NotDirectory => CleanupFindingCode.WorkspaceNotDirectory,
            CliWorkspaceSelectionState.Unsafe => CleanupFindingCode.WorkspaceUnsafe,
            CliWorkspaceSelectionState.Missing or CliWorkspaceSelectionState.Invalid or CliWorkspaceSelectionState.Inaccessible
                or CliWorkspaceSelectionState.Unsupported or CliWorkspaceSelectionState.InputOutputFailure or null => CleanupFindingCode.WorkspaceUnavailable,
            CliWorkspaceSelectionState.Selected => throw new InvalidOperationException("A selected workspace cannot form an invalid binding result."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The workspace selection state is not defined."),
        };
}
