using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational;

internal interface IFrameworkLifecycleOperationalContributor
{
    ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken);

    ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class FrameworkLifecycleOperationalContributor(
    PhysicalPathResolver physicalPathResolver,
    FrameworkLifecycleTargetReader targetReader) : IFrameworkLifecycleOperationalContributor
{
    internal async ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken)
    {
        var view = await ReadAsync(workspace, ownership, cancellationToken).ConfigureAwait(false);
        return new FrameworkLifecycleStatusView
        {
            State = view.State,
            Presence = ownership.State switch
            {
                WorkspaceOwnershipReadState.Absent => OperationalLifecyclePresenceState.Missing,
                WorkspaceOwnershipReadState.Complete => OperationalLifecyclePresenceState.Present,
                _ => OperationalLifecyclePresenceState.Unavailable,
            },
            Lifecycle = view.LifecycleState,
            SourceAvailability = view.SourceAvailability,
            Targets = view.Targets.Select(observation => observation.Target).ToArray(),
            OwnershipObservation = ReadOwnershipObservation(ownership),
        };
    }

    internal async ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            physicalPathResolver, workspace, cancellationToken).ConfigureAwait(false);
        return await ReadAsync(workspace, ownership, cancellationToken).ConfigureAwait(false);
    }

    ValueTask<FrameworkLifecycleStatusView> IFrameworkLifecycleOperationalContributor.ReadStatusAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken)
        => ReadStatusAsync(workspace, ownership, cancellationToken);

    ValueTask<FrameworkLifecycleDoctorView> IFrameworkLifecycleOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    internal static string? ReadOwnershipObservation(WorkspaceOwnershipRead ownership)
        => ownership.State != WorkspaceOwnershipReadState.Complete
            ? ownership.Cause ?? "The ownership lock is absent; no Framework ownership claims are available."
            : ownership.Document.Framework is null
                ? "No Framework ownership claims are recorded in the lock."
                : null;

    private async ValueTask<FrameworkLifecycleDoctorView> ReadAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read();
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets =
            ownership.Document.Framework is { } framework && payload.Payload is { } value
                ? await targetReader.ReadDoctorAsync(workspace, framework, value, cancellationToken).ConfigureAwait(false)
                : [];
        var sourceAvailability = ownership.Document.Framework is null
            ? OperationalSourceAvailability.NotApplicable
            : payload.State == FrameworkPayloadReadState.Available
                && targets.All(target => target.Target.Source.State == FrameworkTargetSourceState.Valid)
                ? OperationalSourceAvailability.Available
                : OperationalSourceAvailability.Unavailable;
        var state = targets.Any(target => target.Target.State == OperationalTargetState.Blocked)
            ? OperationalViewState.Blocked
            : sourceAvailability == OperationalSourceAvailability.Unavailable
                || targets.Any(target => target.Target.State == OperationalTargetState.Unavailable)
                ? OperationalViewState.Incomplete
                : OperationalViewState.Complete;
        var assessment = FrameworkLifecycleDoctorAssessment.Create(
            state,
            sourceAvailability == OperationalSourceAvailability.Available
                ? OperationalLifecycleState.Trusted
                : OperationalLifecycleState.Incomplete,
            sourceAvailability,
            ReadManagedSet(sourceAvailability, targets));
        return FrameworkLifecycleDoctorView.Create(assessment, ownership, payload, targets);
    }

    private static FrameworkManagedSetState ReadManagedSet(
        OperationalSourceAvailability sourceAvailability,
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets)
    {
        if (targets.Count == 0)
        {
            return FrameworkManagedSetState.Empty;
        }

        if (sourceAvailability != OperationalSourceAvailability.Available
            || targets.Any(target => target.Target.State is OperationalTargetState.Unavailable or OperationalTargetState.Blocked))
        {
            return FrameworkManagedSetState.Unavailable;
        }

        var current = targets.Count(target => target.Target.State == OperationalTargetState.Current);
        return current == targets.Count ? FrameworkManagedSetState.Current
            : current == 0 ? FrameworkManagedSetState.NonCurrent : FrameworkManagedSetState.Mixed;
    }
}
