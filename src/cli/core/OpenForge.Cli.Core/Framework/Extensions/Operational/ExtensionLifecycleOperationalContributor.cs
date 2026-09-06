using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal interface IExtensionLifecycleOperationalContributor
{
    ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken);

    ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken);
}

internal sealed class ExtensionLifecycleOperationalContributor :
    IExtensionLifecycleOperationalContributor
{
    private readonly ExtensionLifecycleDoctorReader _doctorReader;
    private readonly ExtensionBridgeRegistrationObservationReader _bridgeReader;
    private readonly LifecycleDocumentReader _lifecycleReader;
    private readonly ExtensionSourceObservationReader _sourceReader;
    private readonly ExtensionLifecycleTargetReader _targetReader;

    internal ExtensionLifecycleOperationalContributor(
        LifecycleDocumentReader lifecycleReader,
        ExtensionSourceReader sourceReader,
        ExtensionLifecycleTargetReader targetReader,
        LifecycleOwnershipReader ownershipReader)
    {
        _lifecycleReader = lifecycleReader;
        _sourceReader = new ExtensionSourceObservationReader(sourceReader);
        _targetReader = targetReader;
        _bridgeReader = new ExtensionBridgeRegistrationObservationReader();
        _doctorReader = new ExtensionLifecycleDoctorReader(
            lifecycleReader,
            _sourceReader,
            targetReader,
            ownershipReader,
            _bridgeReader);
    }

    internal async ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var lifecycle = _lifecycleReader.ReadExtensions(snapshot);
        var sources = await _sourceReader
            .ReadAsync(
                snapshot.Workspace,
                lifecycle.Packages,
                includeEmbedded: false,
                cancellationToken)
            .ConfigureAwait(false);
        var targets = await _targetReader
            .ReadAsync(snapshot.Workspace, lifecycle.Paths, cancellationToken)
            .ConfigureAwait(false);
        var bridgeRegistrations = await _bridgeReader
            .ReadAsync(snapshot.Workspace, lifecycle, sources, cancellationToken)
            .ConfigureAwait(false);
        return new ExtensionLifecycleStatusView
        {
            State = ExtensionLifecycleEvaluation.ReadViewState(lifecycle),
            Presence = ExtensionLifecycleEvaluation.ReadPresence(lifecycle.State),
            Lifecycle = ExtensionLifecycleEvaluation.ReadLifecycleState(lifecycle),
            SourceAvailability = ExtensionLifecycleEvaluation.ReadSourceAvailability(
                lifecycle,
                sources),
            Installed = lifecycle.Packages
                .Select(package => ExtensionSourceObservationReader.Project(package, sources))
                .ToArray(),
            Targets = targets,
            BridgeRegistrations = bridgeRegistrations,
        };
    }

    internal ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => _doctorReader.ReadAsync(workspace, cancellationToken);

    ValueTask<ExtensionLifecycleStatusView> IExtensionLifecycleOperationalContributor.ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
        => ReadStatusAsync(snapshot, cancellationToken);

    ValueTask<ExtensionLifecycleDoctorView> IExtensionLifecycleOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);
}

internal static class ExtensionLifecycleEvaluation
{
    internal static OperationalViewState ReadViewState(LifecycleReadResult lifecycle)
    {
        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            return OperationalViewState.Interrupted;
        }

        return ReadLifecycleState(lifecycle) switch
        {
            OperationalLifecycleState.Trusted => OperationalViewState.Complete,
            OperationalLifecycleState.Untrusted
                or OperationalLifecycleState.Incomplete => OperationalViewState.Incomplete,
            OperationalLifecycleState.Blocked => OperationalViewState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifecycle),
                lifecycle.Trust,
                "The Extension lifecycle state is not defined."),
        };
    }

    internal static OperationalLifecycleState ReadLifecycleState(LifecycleReadResult lifecycle)
        => lifecycle.Trust switch
        {
            LifecycleExtensionTrust.Trusted => OperationalLifecycleState.Trusted,
            LifecycleExtensionTrust.Untrusted => OperationalLifecycleState.Untrusted,
            LifecycleExtensionTrust.Incomplete => OperationalLifecycleState.Incomplete,
            LifecycleExtensionTrust.Blocked => OperationalLifecycleState.Blocked,
            LifecycleExtensionTrust.Absent when lifecycle.State == LifecycleReadState.Complete
                => OperationalLifecycleState.Trusted,
            LifecycleExtensionTrust.Absent => OperationalLifecycleState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifecycle),
                lifecycle.Trust,
                "The Extension lifecycle trust is not defined."),
        };

    internal static OperationalSourceAvailability ReadSourceAvailability(
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var state = ReadLifecycleState(lifecycle);
        if (state == OperationalLifecycleState.Trusted && sources.Count == 0)
        {
            return OperationalSourceAvailability.NotApplicable;
        }

        return state == OperationalLifecycleState.Trusted
            && sources.All(source => ExtensionSourceObservationReader.ReadAvailability(source.Read)
                == OperationalSourceAvailability.Available)
            ? OperationalSourceAvailability.Available
            : OperationalSourceAvailability.Unavailable;
    }

    internal static OperationalLifecyclePresenceState ReadPresence(
        LifecycleReadState state)
        => state switch
        {
            LifecycleReadState.Complete
                or LifecycleReadState.Invalid => OperationalLifecyclePresenceState.Present,
            LifecycleReadState.Missing => OperationalLifecyclePresenceState.Missing,
            LifecycleReadState.Unavailable
                or LifecycleReadState.Cancelled => OperationalLifecyclePresenceState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension lifecycle presence state is not defined."),
        };
}
