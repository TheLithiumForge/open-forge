using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorWorkspaceResultFactory
{
    private readonly DoctorResultBuilder _resultBuilder = new();

    internal DoctorResult Create(CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        if (input.InvalidInput.Source != CliInvalidInputSource.Workspace)
        {
            return _resultBuilder.Event(
                workspace: null,
                CliSemanticStatus.Invalid,
                kind: null,
                cause);
        }

        var kind = input.WorkspaceSelectionState == CliWorkspaceSelectionState.NotDirectory
            ? DoctorFindingKind.WorkspaceNotDirectory
            : DoctorFindingKind.WorkspaceUnavailable;
        return _resultBuilder.Event(
            workspace: null,
            CliSemanticStatus.Blocked,
            kind,
            cause);
    }
}
