using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal interface IFrameworkLifecycleOperationalContributor
{
    ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken);

    ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class FrameworkLifecycleOperationalContributor(
    LifecycleStore lifecycleStore,
    FrameworkLifecycleTargetReader targetReader) : IFrameworkLifecycleOperationalContributor
{
    private readonly FrameworkLifecycleDoctorReader _doctorReader = new(
        lifecycleStore,
        targetReader);

    internal async ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var lifecycle = lifecycleStore.Read(snapshot, LifecycleSection.Framework);
        var payload = EmbeddedFrameworkPayloadReader.Read();
        var targets = await ReadTargetsAsync(lifecycle, payload, cancellationToken).ConfigureAwait(false);
        return new FrameworkLifecycleStatusView
        {
            State = ReadViewState(lifecycle, payload, targets),
            Presence = ReadPresence(lifecycle.State),
            Lifecycle = ReadLifecycleState(lifecycle.State),
            SourceAvailability = ReadSourceAvailability(lifecycle, payload, targets),
            Targets = targets,
        };
    }

    internal async ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => await _doctorReader.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);

    ValueTask<FrameworkLifecycleStatusView> IFrameworkLifecycleOperationalContributor.ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
        => ReadStatusAsync(snapshot, cancellationToken);

    ValueTask<FrameworkLifecycleDoctorView> IFrameworkLifecycleOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private async ValueTask<IReadOnlyList<FrameworkManagedTargetObservation>> ReadTargetsAsync(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        CancellationToken cancellationToken)
    {
        if (lifecycle.Framework is not { } framework || payload.Payload is not { } value)
        {
            return [];
        }

        return await targetReader
            .ReadAsync(lifecycle.Workspace, framework, value, cancellationToken)
            .ConfigureAwait(false);
    }

    private static OperationalViewState ReadViewState(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetObservation> targets)
    {
        if (lifecycle.State == LifecycleStoreReadState.Cancelled)
        {
            return OperationalViewState.Interrupted;
        }

        if (lifecycle.State is LifecycleStoreReadState.Blocked)
        {
            return OperationalViewState.Blocked;
        }

        if (targets.Any(target =>
                target.Source.State == FrameworkLifecycleTargetSourceState.Blocked))
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
            && targets.All(target =>
                target.Source.State == FrameworkLifecycleTargetSourceState.Valid)
            ? OperationalViewState.Complete
            : OperationalViewState.Incomplete;
    }

    private static OperationalLifecycleState ReadLifecycleState(LifecycleStoreReadState state)
        => state switch
        {
            LifecycleStoreReadState.Available => OperationalLifecycleState.Trusted,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing => OperationalLifecycleState.Incomplete,
            LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Unavailable
                or LifecycleStoreReadState.Cancelled => OperationalLifecycleState.Incomplete,
            LifecycleStoreReadState.Blocked => OperationalLifecycleState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework lifecycle read state is not defined."),
        };

    private static OperationalSourceAvailability ReadSourceAvailability(
        LifecycleStoreReadResult lifecycle,
        FrameworkPayloadReadResult payload,
        IReadOnlyList<FrameworkManagedTargetObservation> targets)
    {
        if (lifecycle.State is LifecycleStoreReadState.DocumentMissing
            or LifecycleStoreReadState.SectionMissing)
        {
            return OperationalSourceAvailability.Unavailable;
        }

        return lifecycle.State == LifecycleStoreReadState.Available
            && payload.State == FrameworkPayloadReadState.Available
            && SourceMatches(lifecycle, payload)
            && targets.All(target =>
                target.Source.State == FrameworkLifecycleTargetSourceState.Valid)
            ? OperationalSourceAvailability.Available
            : OperationalSourceAvailability.Unavailable;
    }

    private static OperationalLifecyclePresenceState ReadPresence(
        LifecycleStoreReadState state)
        => state switch
        {
            LifecycleStoreReadState.Available
                or LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => OperationalLifecyclePresenceState.Present,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing => OperationalLifecyclePresenceState.Missing,
            LifecycleStoreReadState.Unavailable
                or LifecycleStoreReadState.Cancelled => OperationalLifecyclePresenceState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework lifecycle presence state is not defined."),
        };

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
