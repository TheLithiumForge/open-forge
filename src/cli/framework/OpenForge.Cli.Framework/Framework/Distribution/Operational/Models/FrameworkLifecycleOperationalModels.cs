using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

internal enum FrameworkManagedSetState
{
    Empty,
    Current,
    NonCurrent,
    Mixed,
    Unavailable,
}

internal sealed class FrameworkLifecycleDoctorAssessment
{
    private FrameworkLifecycleDoctorAssessment(
        OperationalViewState state,
        OperationalLifecycleState lifecycle,
        OperationalSourceAvailability sourceAvailability,
        FrameworkManagedSetState managedSet)
    {
        if (!Enum.IsDefined(state)
            || !Enum.IsDefined(lifecycle)
            || !Enum.IsDefined(sourceAvailability)
            || !Enum.IsDefined(managedSet))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Framework Doctor assessment values must be defined.");
        }

        if (lifecycle == OperationalLifecycleState.Trusted
            != (sourceAvailability == OperationalSourceAvailability.Available))
        {
            throw new ArgumentException(
                "Framework lifecycle trust must match source availability.",
                nameof(sourceAvailability));
        }

        State = state;
        Lifecycle = lifecycle;
        SourceAvailability = sourceAvailability;
        ManagedSet = managedSet;
    }

    internal OperationalViewState State { get; }

    internal OperationalLifecycleState Lifecycle { get; }

    internal OperationalSourceAvailability SourceAvailability { get; }

    internal FrameworkManagedSetState ManagedSet { get; }

    internal static FrameworkLifecycleDoctorAssessment Create(
        OperationalViewState state,
        OperationalLifecycleState lifecycle,
        OperationalSourceAvailability sourceAvailability,
        FrameworkManagedSetState managedSet)
        => new(state, lifecycle, sourceAvailability, managedSet);
}

internal sealed record FrameworkLifecycleStatusView
{
    public required OperationalViewState State { get; init; }

    public required OperationalLifecyclePresenceState Presence { get; init; }

    public required OperationalLifecycleState Lifecycle { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<FrameworkManagedTargetObservation> Targets { get; init; }

    public string? OwnershipObservation { get; init; }
}

internal sealed class FrameworkLifecycleDoctorView
{
    private FrameworkLifecycleDoctorView(
        FrameworkLifecycleDoctorAssessment assessment,
        WorkspaceOwnershipRead ownership,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(targets);
        if (targets.Any(target => target is null))
        {
            throw new ArgumentException(
                "Framework Doctor target observations cannot contain null members.",
                nameof(targets));
        }

        if (targets.Count == 0 != (assessment.ManagedSet == FrameworkManagedSetState.Empty))
        {
            throw new ArgumentException(
                "The Framework managed-set state must identify an empty target set exactly.",
                nameof(assessment));
        }

        Assessment = assessment;
        Ownership = ownership;
        Payload = payload;
        Targets = targets.ToArray();
    }

    internal FrameworkLifecycleDoctorAssessment Assessment { get; }

    internal OperationalViewState State => Assessment.State;

    internal OperationalLifecycleState LifecycleState => Assessment.Lifecycle;

    internal OperationalSourceAvailability SourceAvailability =>
        Assessment.SourceAvailability;

    internal WorkspaceOwnershipRead Ownership { get; }

    internal FrameworkPayloadReadResult Payload { get; }

    internal IReadOnlyList<FrameworkManagedTargetDoctorObservation> Targets { get; }

    internal FrameworkManagedSetState ManagedSet => Assessment.ManagedSet;

    internal static FrameworkLifecycleDoctorView Create(
        FrameworkLifecycleDoctorAssessment assessment,
        WorkspaceOwnershipRead ownership,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
        => new(assessment, ownership, payload, targets);
}
