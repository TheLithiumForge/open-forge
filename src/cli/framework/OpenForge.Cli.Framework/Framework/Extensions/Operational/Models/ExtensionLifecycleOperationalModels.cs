using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
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

    public string? SourceCause { get; init; }
}
internal sealed record ExtensionManagedTargetObservation
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required string? IntendedFingerprint { get; init; }

    public required OperationalTargetState State { get; init; }

    public string? Cause { get; init; }
}

internal enum ExtensionManagedTargetUnavailableReason
{
    IntendedComparisonUnavailable,
    TargetReadUnavailable,
}

internal sealed class ExtensionManagedTargetDoctorObservation
{
    private ExtensionManagedTargetDoctorObservation(
        ExtensionManagedTargetObservation target,
        ManagedTargetReadState readState,
        string? currentFingerprint,
        string? cause,
        ExtensionManagedTargetUnavailableReason? unavailableReason)
    {
        Target = target;
        ReadState = readState;
        CurrentFingerprint = currentFingerprint;
        Cause = cause;
        UnavailableReason = unavailableReason;
    }

    internal ExtensionManagedTargetObservation Target { get; }

    internal ManagedTargetReadState ReadState { get; }

    internal string? CurrentFingerprint { get; }

    internal string? Cause { get; }

    internal ExtensionManagedTargetUnavailableReason? UnavailableReason { get; }

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
            ManagedTargetReadState.Available,
            fingerprint,
            cause: null,
            unavailableReason: null);
    }

    internal static ExtensionManagedTargetDoctorObservation Boundary(
        ExtensionManagedTargetObservation target,
        ManagedTargetReadState readState,
        string? cause,
        ExtensionManagedTargetUnavailableReason? unavailableReason)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (unavailableReason is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(unavailableReason),
                unavailableReason,
                "The Extension target unavailable reason is not defined.");
        }

        var matches = readState switch
        {
            ManagedTargetReadState.Missing => target.State == OperationalTargetState.Missing
                && cause is null
                && unavailableReason is null,
            ManagedTargetReadState.Unavailable => target.State == OperationalTargetState.Unavailable
                && cause is not null
                && unavailableReason is not null,
            ManagedTargetReadState.Blocked => target.State == OperationalTargetState.Blocked
                && cause is not null
                && unavailableReason is null,
            _ => false,
        };
        if (!matches)
        {
            throw new ArgumentException(
                "The Extension target boundary does not match its target state.",
                nameof(target));
        }

        return new(target, readState, currentFingerprint: null, cause, unavailableReason);
    }
}

internal sealed record ExtensionSourceObservation(
    string? RecordedSource,
    ExtensionSourceReadResult Read);

internal sealed record ExtensionLifecycleStatusView
{
    public string? OwnershipObservation { get; init; }

    public required OperationalViewState State { get; init; }

    public required OperationalLifecyclePresenceState Presence { get; init; }

    public required OperationalLifecycleState Lifecycle { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<InstalledExtensionObservation> Installed { get; init; }

    public required IReadOnlyList<ExtensionManagedTargetObservation> Targets { get; init; }

    public ExtensionBridgeRegistrationFacts BridgeRegistrations { get; init; } =
        ExtensionBridgeRegistrationFacts.Incomplete(
            "Typed Extension bridge-registration observations are unavailable.");
}
