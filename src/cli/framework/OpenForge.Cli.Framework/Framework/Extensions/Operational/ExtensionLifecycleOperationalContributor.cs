using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal interface IExtensionLifecycleOperationalContributor
{
    ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        WorkspaceLayoutRead observation, CancellationToken cancellationToken);

    ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation, CancellationToken cancellationToken);
}

internal sealed class ExtensionLifecycleOperationalContributor(
    PhysicalPathResolver physicalPathResolver,
    ExtensionSourceReader sourceReader,
    ExtensionLifecycleTargetReader targetReader) : IExtensionLifecycleOperationalContributor
{
    internal async ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(CliWorkspace workspace, CancellationToken cancellationToken)
        => await ReadDoctorAsync(await new WorkspaceLayoutReader(physicalPathResolver).ReadAsync(workspace, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    internal async ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(CliWorkspace workspace, WorkspaceOwnershipRead ownership, CancellationToken cancellationToken)
    {
        var observed = await new WorkspaceLayoutReader(physicalPathResolver).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        var retained = new WorkspaceLayoutRead(observed.State, observed.Settings, ownership, observed.Layout, observed.Issues, observed.Expectations);
        return await ReadStatusAsync(retained, cancellationToken).ConfigureAwait(false);
    }

    private readonly ExtensionLifecycleDoctorReader _reader = new(
        new ExtensionSourceObservationReader(sourceReader), targetReader,
        new ExtensionBridgeRegistrationObservationReader());

    public async ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        WorkspaceLayoutRead observation, CancellationToken cancellationToken)
    {
        var ownership = observation.Ownership;
        var view = await _reader.ReadAsync(observation, includeEmbedded: false, cancellationToken)
            .ConfigureAwait(false);
        return new ExtensionLifecycleStatusView
        {
            State = view.State,
            Presence = ownership.State == WorkspaceOwnershipReadState.Absent
                ? OperationalLifecyclePresenceState.Missing
                : ownership.State == WorkspaceOwnershipReadState.Complete
                    ? OperationalLifecyclePresenceState.Present : OperationalLifecyclePresenceState.Unavailable,
            Lifecycle = view.LifecycleState,
            OwnershipObservation = ReadOwnershipObservation(view.Ownership),
            SourceAvailability = view.SourceAvailability,
            Installed = view.Ownership.Document.Extensions
                .Select(package => ExtensionSourceObservationReader.Project(package, view.Sources)).ToArray(),
            Targets = view.Targets.Select(target => target.Target).ToArray(),
            BridgeRegistrations = view.BridgeRegistrations,
        };
    }

    public async ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        WorkspaceLayoutRead observation, CancellationToken cancellationToken)
    {
        return await _reader.ReadAsync(observation, includeEmbedded: true, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static string? ReadOwnershipObservation(WorkspaceOwnershipRead ownership)
        => ownership.State == WorkspaceOwnershipReadState.Complete ? null
            : ownership.Cause ?? "The ownership lock is absent; no Extension ownership claims are recorded.";
}
