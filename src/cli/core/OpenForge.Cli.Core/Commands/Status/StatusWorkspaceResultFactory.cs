using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Status;

internal static class StatusWorkspaceResultFactory
{
    internal static StatusResult Create(CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        var workspaceFailure = input.InvalidInput.Source == CliInvalidInputSource.Workspace;
        var subject = workspaceFailure
            ? input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory
            : null;
        var code = workspaceFailure
            ? ReadWorkspaceFinding(input.WorkspaceSelectionState)
            : StatusFindingCode.InvalidInput;
        return StatusResultBuilder.Event(null, code, subject, cause);
    }

    private static StatusFindingCode ReadWorkspaceFinding(
        CliWorkspaceSelectionState? state)
        => state switch
        {
            CliWorkspaceSelectionState.NotDirectory => StatusFindingCode.WorkspaceNotDirectory,
            CliWorkspaceSelectionState.Unsafe => StatusFindingCode.WorkspaceUnsafe,
            CliWorkspaceSelectionState.Selected => throw new InvalidOperationException(
                "A selected workspace cannot form an invalid binding result."),
            CliWorkspaceSelectionState.Missing
                or CliWorkspaceSelectionState.Invalid
                or CliWorkspaceSelectionState.Inaccessible
                or CliWorkspaceSelectionState.Unsupported
                or CliWorkspaceSelectionState.InputOutputFailure
                or null => StatusFindingCode.WorkspaceUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The workspace selection state is not defined."),
        };
}
