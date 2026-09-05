using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal sealed record InstalledExtensionObservation
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}
internal sealed record ExtensionManagedTargetObservation
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed class ExtensionManagedTargetDoctorObservation
{
    private ExtensionManagedTargetDoctorObservation(
        ExtensionManagedTargetObservation target,
        LifecycleManagedTargetReadState readState,
        string? currentFingerprint,
        string? cause)
    {
        Target = target;
        ReadState = readState;
        CurrentFingerprint = currentFingerprint;
        Cause = cause;
    }

    internal ExtensionManagedTargetObservation Target { get; }

    internal LifecycleManagedTargetReadState ReadState { get; }

    internal string? CurrentFingerprint { get; }

    internal string? Cause { get; }

    internal static ExtensionManagedTargetDoctorObservation Observed(
        ExtensionManagedTargetObservation target,
        string fingerprint)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target.State is not (OperationalTargetState.Current or OperationalTargetState.Changed))
        {
            throw new ArgumentException(
                "An observed Extension target requires a current or changed state.",
                nameof(target));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        return new(
            target,
            LifecycleManagedTargetReadState.Available,
            fingerprint,
            cause: null);
    }

    internal static ExtensionManagedTargetDoctorObservation Boundary(
        ExtensionManagedTargetObservation target,
        LifecycleManagedTargetReadState readState,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(target);
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
        if (!matches)
        {
            throw new ArgumentException(
                "The Extension target boundary does not match its target state.",
                nameof(target));
        }

        return new(target, readState, currentFingerprint: null, cause);
    }
}

internal sealed record ExtensionSourceObservation(
    string? RecordedSource,
    ExtensionSourceReadResult Read);

internal sealed record ExtensionLifecycleStatusView
{
    public required OperationalViewState State { get; init; }

    public required OperationalLifecyclePresenceState Presence { get; init; }

    public required OperationalLifecycleState Lifecycle { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<InstalledExtensionObservation> Installed { get; init; }

    public required IReadOnlyList<ExtensionManagedTargetObservation> Targets { get; init; }
}
