using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;

internal enum FrameworkManagedTargetKind
{
    File,
    ManagedRegion,
    GeneratedRegion,
}

internal enum FrameworkManagedTargetBoundaryKind
{
    ProviderBridge,
    RootRegion,
}

internal sealed record FrameworkManagedTargetObservation
{
    public required string Path { get; init; }

    public required FrameworkManagedTargetKind Kind { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? Region { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required FrameworkLifecycleTargetSourceValidation Source { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed class FrameworkManagedTargetDoctorObservation
{
    private FrameworkManagedTargetDoctorObservation(
        FrameworkManagedTargetObservation target,
        FrameworkManagedTargetBoundaryKind? boundary,
        LifecycleManagedTargetReadState? readState,
        string? currentFingerprint,
        string? cause)
    {
        Target = target;
        Boundary = boundary;
        ReadState = readState;
        CurrentFingerprint = currentFingerprint;
        Cause = cause;
    }

    internal FrameworkManagedTargetObservation Target { get; }

    internal FrameworkManagedTargetBoundaryKind? Boundary { get; }

    internal LifecycleManagedTargetReadState? ReadState { get; }

    internal string? CurrentFingerprint { get; }

    internal string? Cause { get; }

    internal static FrameworkManagedTargetDoctorObservation SourceInvalid(
        FrameworkManagedTargetObservation target,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target.Source.State == FrameworkLifecycleTargetSourceState.Valid
            || target.State != OperationalTargetState.Blocked)
        {
            throw new ArgumentException(
                "A source-invalid Framework target requires a blocked target and a non-valid source.",
                nameof(target));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new(target, boundary: null, readState: null, currentFingerprint: null, cause);
    }

    internal static FrameworkManagedTargetDoctorObservation Observed(
        FrameworkManagedTargetObservation target,
        FrameworkManagedTargetBoundaryKind? boundary,
        string fingerprint)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (boundary is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(boundary),
                boundary,
                "The Framework target boundary is not defined.");
        }

        if (target.State is not (OperationalTargetState.Current or OperationalTargetState.Changed)
            || target.Source.State != FrameworkLifecycleTargetSourceState.Valid)
        {
            throw new ArgumentException(
                "An observed Framework target requires a valid source and current or changed state.",
                nameof(target));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        return new(target, boundary, LifecycleManagedTargetReadState.Available, fingerprint, cause: null);
    }

    internal static FrameworkManagedTargetDoctorObservation AtBoundary(
        FrameworkManagedTargetObservation target,
        FrameworkManagedTargetBoundaryKind? boundary,
        LifecycleManagedTargetReadState readState,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (boundary is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(boundary),
                boundary,
                "The Framework target boundary is not defined.");
        }

        var matches = readState switch
        {
            LifecycleManagedTargetReadState.Missing => target.State == OperationalTargetState.Missing
                && cause is null,
            LifecycleManagedTargetReadState.Unavailable => target.State == OperationalTargetState.Unavailable
                && cause is not null,
            LifecycleManagedTargetReadState.Blocked => target.State == OperationalTargetState.Blocked
                && cause is not null,
            _ => false,
        };
        if (!matches || target.Source.State != FrameworkLifecycleTargetSourceState.Valid)
        {
            throw new ArgumentException(
                "The Framework target boundary does not match its source and target state.",
                nameof(target));
        }

        return new(target, boundary, readState, currentFingerprint: null, cause);
    }
}
