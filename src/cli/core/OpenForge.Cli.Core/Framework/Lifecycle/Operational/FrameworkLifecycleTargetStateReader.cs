using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal sealed class FrameworkLifecycleTargetStateReader(
    LifecycleManagedTargetReader managedTargetReader)
{
    private readonly FrameworkLifecycleTargetIdentity _targetIdentity = new();

    internal async ValueTask<FrameworkManagedTargetDoctorRead> ReadAsync(
        CliWorkspace workspace,
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated,
        CancellationToken cancellationToken)
    {
        var read = await managedTargetReader
            .ReadAsync(workspace, target.Path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != LifecycleManagedTargetReadState.Available)
        {
            return FrameworkManagedTargetDoctorRead.Boundary(read.State, cancellationToken);
        }

        try
        {
            var fingerprint = _targetIdentity.ReadFingerprint(target, generated, read.Bytes.Span);
            var state = string.Equals(
                fingerprint,
                target.BaselineFingerprint,
                StringComparison.Ordinal)
                ? OperationalTargetState.Current
                : OperationalTargetState.Changed;
            return FrameworkManagedTargetDoctorRead.Observed(state, fingerprint);
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return FrameworkManagedTargetDoctorRead.Blocked(exception.Message);
        }
    }
}

internal sealed class FrameworkManagedTargetDoctorRead
{
    private FrameworkManagedTargetDoctorRead(
        OperationalTargetState state,
        LifecycleManagedTargetReadState readState,
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
            LifecycleManagedTargetReadState.Available =>
                state is OperationalTargetState.Current or OperationalTargetState.Changed
                && !string.IsNullOrWhiteSpace(currentFingerprint)
                && cause is null,
            LifecycleManagedTargetReadState.Missing =>
                state == OperationalTargetState.Missing
                && currentFingerprint is null
                && cause is null,
            LifecycleManagedTargetReadState.Unavailable =>
                state == OperationalTargetState.Unavailable
                && currentFingerprint is null
                && !string.IsNullOrWhiteSpace(cause),
            LifecycleManagedTargetReadState.Blocked =>
                state == OperationalTargetState.Blocked
                && currentFingerprint is null
                && !string.IsNullOrWhiteSpace(cause),
            LifecycleManagedTargetReadState.Cancelled => false,
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

    internal LifecycleManagedTargetReadState ReadState { get; }

    internal string? CurrentFingerprint { get; }

    internal string? Cause { get; }

    internal static FrameworkManagedTargetDoctorRead Observed(
        OperationalTargetState state,
        string fingerprint)
        => new(state, LifecycleManagedTargetReadState.Available, fingerprint, cause: null);

    internal static FrameworkManagedTargetDoctorRead Blocked(string cause)
        => new(
            OperationalTargetState.Blocked,
            LifecycleManagedTargetReadState.Blocked,
            currentFingerprint: null,
            cause);

    internal static FrameworkManagedTargetDoctorRead Boundary(
        LifecycleManagedTargetReadState state,
        CancellationToken cancellationToken)
        => state switch
        {
            LifecycleManagedTargetReadState.Missing => new(
                OperationalTargetState.Missing,
                state,
                currentFingerprint: null,
                cause: null),
            LifecycleManagedTargetReadState.Unavailable => new(
                OperationalTargetState.Unavailable,
                state,
                currentFingerprint: null,
                "The Framework target is unavailable."),
            LifecycleManagedTargetReadState.Blocked => Blocked(
                "The Framework target boundary is blocked."),
            LifecycleManagedTargetReadState.Cancelled =>
                throw new OperationCanceledException(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework target read state is not defined."),
        };
}
