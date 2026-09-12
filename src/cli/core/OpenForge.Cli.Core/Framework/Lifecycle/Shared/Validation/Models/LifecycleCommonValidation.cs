using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;

internal enum LifecycleCommonValidationState
{
    Valid,
    Invalid,
    Blocked,
}

internal sealed record LifecycleCommonValidation
{
    private LifecycleCommonValidation(
        LifecycleCommonValidationState state,
        LifecycleWorkspaceBinding workspaceBinding,
        string? cause)
    {
        State = state;
        WorkspaceBinding = workspaceBinding;
        Cause = cause;
    }

    internal LifecycleCommonValidationState State { get; }

    internal LifecycleWorkspaceBinding WorkspaceBinding { get; }

    internal string? Cause { get; }

    internal static LifecycleCommonValidation Valid()
        => new(
            LifecycleCommonValidationState.Valid,
            LifecycleWorkspaceBinding.Matched,
            cause: null);

    internal static LifecycleCommonValidation Invalid(string cause)
        => new(
            LifecycleCommonValidationState.Invalid,
            LifecycleWorkspaceBinding.Unavailable,
            ValidateCause(cause));

    internal static LifecycleCommonValidation Blocked(
        string cause,
        LifecycleWorkspaceBinding workspaceBinding)
        => new(
            LifecycleCommonValidationState.Blocked,
            workspaceBinding,
            ValidateCause(cause));

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause;
    }
}
