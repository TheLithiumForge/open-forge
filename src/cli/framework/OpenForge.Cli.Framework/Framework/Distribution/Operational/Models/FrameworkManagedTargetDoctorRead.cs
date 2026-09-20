using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

internal sealed class FrameworkManagedTargetDoctorRead
{
    private FrameworkManagedTargetDoctorRead(
        OperationalTargetState state,
        ManagedTargetReadState readState,
        string? currentFingerprint,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The target state is not defined.");
        }

        if (!Enum.IsDefined(readState))
        {
            throw new ArgumentOutOfRangeException(nameof(readState), readState, "The target read state is not defined.");
        }

        var compatible = readState switch
        {
            ManagedTargetReadState.Available =>
                state is OperationalTargetState.Current or OperationalTargetState.Changed
                && !string.IsNullOrWhiteSpace(currentFingerprint)
                && cause is null,
            ManagedTargetReadState.Missing =>
                state == OperationalTargetState.Missing
                && currentFingerprint is null
                && cause is null,
            ManagedTargetReadState.Unavailable =>
                state == OperationalTargetState.Unavailable
                && currentFingerprint is null
                && !string.IsNullOrWhiteSpace(cause),
            ManagedTargetReadState.Blocked =>
                state == OperationalTargetState.Blocked
                && currentFingerprint is null
                && !string.IsNullOrWhiteSpace(cause),
            ManagedTargetReadState.Cancelled => false,
            _ => false,
        };
        if (!compatible)
        {
            throw new ArgumentException("The Framework target read fields do not match their state.", nameof(readState));
        }

        State = state;
        ReadState = readState;
        CurrentFingerprint = currentFingerprint;
        Cause = cause;
    }

    internal OperationalTargetState State { get; }

    internal ManagedTargetReadState ReadState { get; }

    internal string? CurrentFingerprint { get; }

    internal string? Cause { get; }

    internal static FrameworkManagedTargetDoctorRead Observed(
        OperationalTargetState state,
        string fingerprint)
        => new(state, ManagedTargetReadState.Available, fingerprint, cause: null);

    internal static FrameworkManagedTargetDoctorRead Unavailable(string cause)
        => new(OperationalTargetState.Unavailable, ManagedTargetReadState.Unavailable,
            currentFingerprint: null, cause);

    internal static FrameworkManagedTargetDoctorRead Blocked(string cause)
        => new(
            OperationalTargetState.Blocked,
            ManagedTargetReadState.Blocked,
            currentFingerprint: null,
            cause);

    internal static FrameworkManagedTargetDoctorRead Boundary(
        ManagedTargetReadState state,
        CancellationToken cancellationToken)
        => state switch
        {
            ManagedTargetReadState.Missing => new(
                OperationalTargetState.Missing,
                state,
                currentFingerprint: null,
                cause: null),
            ManagedTargetReadState.Unavailable => new(
                OperationalTargetState.Unavailable,
                state,
                currentFingerprint: null,
                "The Framework target is unavailable."),
            ManagedTargetReadState.Blocked => Blocked(
                "The Framework target boundary is blocked."),
            ManagedTargetReadState.Cancelled =>
                throw new OperationCanceledException(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework target read state is not defined."),
        };
}
