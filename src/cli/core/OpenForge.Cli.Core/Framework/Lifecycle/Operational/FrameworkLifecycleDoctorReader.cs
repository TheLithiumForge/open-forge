using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal sealed class FrameworkLifecycleDoctorReader(
    LifecycleStore lifecycleStore,
    FrameworkLifecycleTargetReader targetReader)
{
    internal async ValueTask<FrameworkLifecycleDoctorView> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var lifecycle = await lifecycleStore
            .ReadAsync(workspace, LifecycleSection.Framework, cancellationToken)
            .ConfigureAwait(false);
        var payload = EmbeddedFrameworkPayloadReader.Read();
        var targets = await ReadTargetsAsync(
            lifecycle,
            payload,
            cancellationToken).ConfigureAwait(false);
        var sourceAvailability = ReadSourceAvailability(lifecycle, payload, targets);
        var assessment = FrameworkLifecycleDoctorAssessment.Create(
            ReadViewState(lifecycle, payload, targets),
            ReadLifecycleState(lifecycle, sourceAvailability),
            sourceAvailability,
            ReadManagedSet(lifecycle, payload, targets));
        return FrameworkLifecycleDoctorView.Create(
            assessment,
            lifecycle,
            payload,
            targets);
    }

    private async ValueTask<IReadOnlyList<FrameworkManagedTargetDoctorObservation>> ReadTargetsAsync(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        CancellationToken cancellationToken)
    {
        if (lifecycle.Framework is not { } framework || payload.Payload is not { } value)
        {
            return [];
        }

        return await targetReader
            .ReadDoctorAsync(lifecycle.Workspace, framework, value, cancellationToken)
            .ConfigureAwait(false);
    }

    private static FrameworkManagedSetState ReadManagedSet(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
    {
        if (targets.Count == 0)
        {
            return FrameworkManagedSetState.Empty;
        }

        if (lifecycle.State != LifecycleStoreReadState.Available
            || payload.State != FrameworkPayloadReadState.Available
            || !SourceMatches(lifecycle, payload)
            || targets.Any(target => target.Target.Source.State
                != FrameworkLifecycleTargetSourceState.Valid)
            || targets.Any(target => target.Target.State is OperationalTargetState.Unavailable
                or OperationalTargetState.Blocked))
        {
            return FrameworkManagedSetState.Unavailable;
        }

        var current = targets.Count(target =>
            target.Target.State == OperationalTargetState.Current);
        if (current == targets.Count)
        {
            return FrameworkManagedSetState.Current;
        }

        return current == 0
            ? FrameworkManagedSetState.NonCurrent
            : FrameworkManagedSetState.Mixed;
    }

    private static OperationalViewState ReadViewState(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
    {
        if (lifecycle.State == LifecycleStoreReadState.Cancelled)
        {
            return OperationalViewState.Interrupted;
        }

        if (lifecycle.State == LifecycleStoreReadState.Blocked
            || targets.Any(target =>
                target.Target.Source.State == FrameworkLifecycleTargetSourceState.Blocked))
        {
            return OperationalViewState.Blocked;
        }

        if (lifecycle.State is LifecycleStoreReadState.DocumentMissing
            or LifecycleStoreReadState.SectionMissing)
        {
            return OperationalViewState.Incomplete;
        }

        return lifecycle.State == LifecycleStoreReadState.Available
            && payload.State == FrameworkPayloadReadState.Available
            && SourceMatches(lifecycle, payload)
            && targets.All(target => target.Target.Source.State
                == FrameworkLifecycleTargetSourceState.Valid)
            ? OperationalViewState.Complete
            : OperationalViewState.Incomplete;
    }

    private static OperationalLifecycleState ReadLifecycleState(
        LifecycleStoreReadResult lifecycle,
        OperationalSourceAvailability sourceAvailability)
        => lifecycle.State switch
        {
            LifecycleStoreReadState.Available
                when sourceAvailability == OperationalSourceAvailability.Available =>
                OperationalLifecycleState.Trusted,
            LifecycleStoreReadState.Available => OperationalLifecycleState.Untrusted,
            LifecycleStoreReadState.Blocked => OperationalLifecycleState.Blocked,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing
                or LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Unavailable
                or LifecycleStoreReadState.Cancelled => OperationalLifecycleState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifecycle),
                lifecycle.State,
                "The Framework lifecycle read state is not defined."),
        };

    private static OperationalSourceAvailability ReadSourceAvailability(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
        => lifecycle.State == LifecycleStoreReadState.Available
            && payload.State == FrameworkPayloadReadState.Available
            && SourceMatches(lifecycle, payload)
            && targets.All(target => target.Target.Source.State
                == FrameworkLifecycleTargetSourceState.Valid)
            ? OperationalSourceAvailability.Available
            : OperationalSourceAvailability.Unavailable;

    private static bool SourceMatches(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload)
        => lifecycle.Framework is { } framework
            && payload.Payload is { } value
            && string.Equals(
                framework.Source.InventoryFingerprint,
                value.InventoryFingerprint,
                StringComparison.Ordinal);
}
