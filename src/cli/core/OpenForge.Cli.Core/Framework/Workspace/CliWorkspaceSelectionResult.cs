using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Workspace;

internal enum CliWorkspaceSelectionState
{
    Selected,
    Missing,
    Invalid,
    Inaccessible,
    Unsafe,
    Unsupported,
    InputOutputFailure,
}

internal sealed class CliWorkspaceSelectionResult
{
    private CliWorkspaceSelectionResult(
        CliWorkspaceSelectionState state,
        CliWorkspace? workspace,
        FilesystemFailure? failure)
    {
        State = state;
        Workspace = workspace;
        Failure = failure;
    }

    internal CliWorkspaceSelectionState State { get; }

    internal CliWorkspace? Workspace { get; }

    internal FilesystemFailure? Failure { get; }

    internal static CliWorkspaceSelectionResult Selected(CliWorkspace workspace)
    {
        return new CliWorkspaceSelectionResult(CliWorkspaceSelectionState.Selected, workspace, null);
    }

    internal static CliWorkspaceSelectionResult Classified(CliWorkspaceSelectionState state)
    {
        if (state is CliWorkspaceSelectionState.Selected
            or CliWorkspaceSelectionState.Invalid
            or CliWorkspaceSelectionState.Inaccessible
            or CliWorkspaceSelectionState.Unsupported
            or CliWorkspaceSelectionState.InputOutputFailure
            || !Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state requires another factory.");
        }

        return new CliWorkspaceSelectionResult(state, null, null);
    }

    internal static CliWorkspaceSelectionResult Failed(
        CliWorkspaceSelectionState state,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        if (state is not (CliWorkspaceSelectionState.Invalid
            or CliWorkspaceSelectionState.Inaccessible
            or CliWorkspaceSelectionState.Unsupported
            or CliWorkspaceSelectionState.InputOutputFailure))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state.");
        }

        var expectedFailureKind = state switch
        {
            CliWorkspaceSelectionState.Invalid => FilesystemFailureKind.InvalidPath,
            CliWorkspaceSelectionState.Inaccessible => FilesystemFailureKind.AccessDenied,
            CliWorkspaceSelectionState.Unsupported => FilesystemFailureKind.Unsupported,
            CliWorkspaceSelectionState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state."),
        };
        if (failure.Kind != expectedFailureKind)
        {
            throw new ArgumentException("The filesystem failure kind does not match the workspace state.", nameof(failure));
        }

        return new CliWorkspaceSelectionResult(state, null, failure);
    }
}
