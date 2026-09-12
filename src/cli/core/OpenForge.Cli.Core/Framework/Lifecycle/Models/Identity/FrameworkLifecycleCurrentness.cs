namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Identity;

internal enum FrameworkLifecycleCurrentnessState
{
    Current,
    SourceMismatch,
    Changed,
    Missing,
    Unavailable,
    Blocked,
    Cancelled,
}

internal enum FrameworkLifecycleTargetSourceState
{
    Valid,
    SourceMismatch,
    Blocked,
}

internal sealed record FrameworkLifecycleTargetSourceValidation
{
    internal FrameworkLifecycleTargetSourceValidation(
        FrameworkLifecycleTargetSourceState state,
        string? cause)
    {
        var coherent = state switch
        {
            FrameworkLifecycleTargetSourceState.Valid => cause is null,
            FrameworkLifecycleTargetSourceState.SourceMismatch
                or FrameworkLifecycleTargetSourceState.Blocked => !string.IsNullOrWhiteSpace(cause),
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

    internal FrameworkLifecycleTargetSourceState State { get; }

    internal string? Cause { get; }
}

internal sealed record FrameworkLifecycleCurrentness
{
    private const int MaximumCauseLength = 256;

    internal FrameworkLifecycleCurrentness(
        FrameworkLifecycleCurrentnessState state,
        string? path,
        string? cause)
    {
        var coherent = state switch
        {
            FrameworkLifecycleCurrentnessState.Current => path is null && cause is null,
            FrameworkLifecycleCurrentnessState.SourceMismatch => path is null && !string.IsNullOrWhiteSpace(cause),
            FrameworkLifecycleCurrentnessState.Changed
                or FrameworkLifecycleCurrentnessState.Missing
                or FrameworkLifecycleCurrentnessState.Unavailable
                or FrameworkLifecycleCurrentnessState.Blocked => !string.IsNullOrWhiteSpace(path)
                    && !string.IsNullOrWhiteSpace(cause),
            FrameworkLifecycleCurrentnessState.Cancelled => path is null && cause is null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework lifecycle currentness state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "Framework lifecycle currentness details do not match their state.");
        }

        State = state;
        Path = path;
        Cause = cause is null || cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    internal FrameworkLifecycleCurrentnessState State { get; }

    internal string? Path { get; }

    internal string? Cause { get; }
}
