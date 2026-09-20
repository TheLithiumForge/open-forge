namespace OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

internal enum FrameworkTargetSourceState
{
    Valid,
    SourceMismatch,
    Blocked,
}

internal sealed record FrameworkTargetSourceValidation
{
    internal FrameworkTargetSourceValidation(
        FrameworkTargetSourceState state,
        string? cause)
    {
        var coherent = state switch
        {
            FrameworkTargetSourceState.Valid => cause is null,
            FrameworkTargetSourceState.SourceMismatch
                or FrameworkTargetSourceState.Blocked => !string.IsNullOrWhiteSpace(cause),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework lifecycle target source state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "Framework lifecycle target source details do not match their state.");
        }

        State = state;
        Cause = cause;
    }

    internal FrameworkTargetSourceState State { get; }

    internal string? Cause { get; }
}

