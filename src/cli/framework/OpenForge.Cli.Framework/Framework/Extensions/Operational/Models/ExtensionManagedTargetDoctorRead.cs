using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal sealed record ExtensionManagedTargetDoctorRead(
    OperationalTargetState State,
    ManagedTargetReadState ReadState,
    string? CurrentFingerprint,
    string? Cause,
    ExtensionManagedTargetUnavailableReason? UnavailableReason)
{
    internal static ExtensionManagedTargetDoctorRead Observed(
        OperationalTargetState state,
        string fingerprint)
        => new(state, ManagedTargetReadState.Available, fingerprint, Cause: null, UnavailableReason: null);

    internal static ExtensionManagedTargetDoctorRead Unavailable(
        string cause,
        ExtensionManagedTargetUnavailableReason reason)
        => new(OperationalTargetState.Unavailable, ManagedTargetReadState.Unavailable, null, cause, reason);

    internal static ExtensionManagedTargetDoctorRead Blocked(string cause)
        => new(
            OperationalTargetState.Blocked,
            ManagedTargetReadState.Blocked,
            CurrentFingerprint: null,
            cause,
            UnavailableReason: null);

    internal static ExtensionManagedTargetDoctorRead Boundary(
        ManagedTargetReadState state,
        CancellationToken cancellationToken)
        => state switch
        {
            ManagedTargetReadState.Missing => new(
                OperationalTargetState.Missing,
                state,
                CurrentFingerprint: null,
                Cause: null,
                UnavailableReason: null),
            ManagedTargetReadState.Unavailable => new(
                OperationalTargetState.Unavailable,
                state,
                CurrentFingerprint: null,
                "The Extension target is unavailable.",
                ExtensionManagedTargetUnavailableReason.TargetReadUnavailable),
            ManagedTargetReadState.Blocked => Blocked(
                "The Extension target boundary is blocked."),
            ManagedTargetReadState.Cancelled =>
                throw new OperationCanceledException(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension target read state is not defined."),
        };
}
