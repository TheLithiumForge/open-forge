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
        CliWorkspace workspace, WorkspaceOwnershipRead ownership, CancellationToken cancellationToken);

    ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace, CancellationToken cancellationToken);
}

internal sealed class ExtensionLifecycleOperationalContributor(
    PhysicalPathResolver physicalPathResolver,
    ExtensionSourceReader sourceReader,
    ExtensionLifecycleTargetReader targetReader) : IExtensionLifecycleOperationalContributor
{
    private readonly ExtensionLifecycleDoctorReader _reader = new(
        new ExtensionSourceObservationReader(sourceReader), targetReader,
        new ExtensionBridgeRegistrationObservationReader());

    public async ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        CliWorkspace workspace, WorkspaceOwnershipRead ownership, CancellationToken cancellationToken)
    {
        var view = await _reader.ReadAsync(workspace, ownership, includeEmbedded: false, cancellationToken)
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
        CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var ownership = await WorkspaceOwnershipReader.ReadAsync(physicalPathResolver, workspace, cancellationToken)
            .ConfigureAwait(false);
        return await _reader.ReadAsync(workspace, ownership, includeEmbedded: true, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static string? ReadOwnershipObservation(WorkspaceOwnershipRead ownership)
        => ownership.State == WorkspaceOwnershipReadState.Complete ? null
            : ownership.Cause ?? "The ownership lock is absent; no Extension ownership claims are recorded.";
}
