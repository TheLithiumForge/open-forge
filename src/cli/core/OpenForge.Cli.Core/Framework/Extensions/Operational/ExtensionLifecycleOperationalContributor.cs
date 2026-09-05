using OpenForge.Cli.Core.Framework.Extensions.Models;
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

internal sealed class ExtensionLifecycleOperationalContributor(
    LifecycleDocumentReader lifecycleReader,
    ExtensionSourceReader sourceReader,
    ExtensionLifecycleTargetReader targetReader,
    LifecycleOwnershipReader ownershipReader) : IExtensionLifecycleOperationalContributor
{
    internal async ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var lifecycle = lifecycleReader.ReadExtensions(snapshot);
        var sources = await ReadSourceObservationsAsync(
            snapshot.Workspace,
            lifecycle.Packages,
            cancellationToken).ConfigureAwait(false);
        var targets = await targetReader
            .ReadAsync(snapshot.Workspace, lifecycle.Paths, cancellationToken)
            .ConfigureAwait(false);
        return new ExtensionLifecycleStatusView
        {
            State = ReadViewState(lifecycle),
            Presence = ReadPresence(lifecycle.State),
            Lifecycle = ReadLifecycleState(lifecycle),
            SourceAvailability = ReadAggregateSourceAvailability(lifecycle, sources),
            Installed = lifecycle.Packages.Select(package => Project(package, sources)).ToArray(),
            Targets = targets,
        };
    }

    internal async ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var lifecycle = await lifecycleReader
            .ReadExtensionsAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var sources = await ReadSourceObservationsAsync(
            workspace,
            lifecycle.Packages,
            cancellationToken).ConfigureAwait(false);
        var ownership = await ownershipReader
            .ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var targets = await targetReader
            .ReadAsync(workspace, lifecycle.Paths, cancellationToken)
            .ConfigureAwait(false);
        return new ExtensionLifecycleDoctorView
        {
            State = ReadViewState(lifecycle),
            Lifecycle = lifecycle,
            Sources = sources,
            Ownership = ownership,
            Targets = targets,
        };
    }

    ValueTask<ExtensionLifecycleStatusView> IExtensionLifecycleOperationalContributor.ReadStatusAsync(
        LifecycleDocumentSnapshot snapshot,
        CancellationToken cancellationToken)
        => ReadStatusAsync(snapshot, cancellationToken);

    ValueTask<ExtensionLifecycleDoctorView> IExtensionLifecycleOperationalContributor.ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => ReadDoctorAsync(workspace, cancellationToken);

    private async ValueTask<IReadOnlyList<ExtensionSourceObservation>> ReadSourceObservationsAsync(
        CliWorkspace workspace,
        IReadOnlyList<LifecycleInstalledPackage> packages,
        CancellationToken cancellationToken)
    {
        var includesEmbedded = false;
        var explicitSources = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var package in packages)
        {
            if (package.Source is null)
            {
                includesEmbedded = true;
                continue;
            }

            explicitSources.Add(package.Source);
        }

        var observations = new List<ExtensionSourceObservation>(
            explicitSources.Count + (includesEmbedded ? 1 : 0));
        if (includesEmbedded)
        {
            var source = await sourceReader
                .ReadAsync(workspace, explicitSource: null, cancellationToken)
                .ConfigureAwait(false);
            observations.Add(new ExtensionSourceObservation(RecordedSource: null, source));
        }

        foreach (var recordedSource in explicitSources)
        {
            var source = await sourceReader
                .ReadAsync(workspace, recordedSource, cancellationToken)
                .ConfigureAwait(false);
            observations.Add(new ExtensionSourceObservation(recordedSource, source));
        }

        return Array.AsReadOnly(observations.ToArray());
    }

    private static InstalledExtensionObservation Project(
        LifecycleInstalledPackage package,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var source = sources.Single(observation => string.Equals(
            observation.RecordedSource,
            package.Source,
            StringComparison.Ordinal));
        return new InstalledExtensionObservation
        {
            Id = package.Id,
            Version = package.Version,
            Source = package.Source,
            SourceAvailability = ReadSourceAvailability(source.Read),
            Dependencies = package.Dependencies.ToArray(),
            Paths = package.Paths.ToArray(),
        };
    }

    private static OperationalViewState ReadViewState(LifecycleReadResult lifecycle)
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

    private static OperationalLifecycleState ReadLifecycleState(LifecycleReadResult lifecycle)
    {
        return lifecycle.Trust switch
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
    }

    private static OperationalSourceAvailability ReadAggregateSourceAvailability(
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var state = ReadLifecycleState(lifecycle);
        if (state == OperationalLifecycleState.Trusted && sources.Count == 0)
        {
            return OperationalSourceAvailability.NotApplicable;
        }

        return state == OperationalLifecycleState.Trusted
            && sources.All(source =>
                ReadSourceAvailability(source.Read) == OperationalSourceAvailability.Available)
            ? OperationalSourceAvailability.Available
            : OperationalSourceAvailability.Unavailable;
    }

    private static OperationalLifecyclePresenceState ReadPresence(
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

    private static OperationalSourceAvailability ReadSourceAvailability(
        ExtensionSourceReadResult source)
        => source.State switch
        {
            ExtensionSourceReadState.Complete => OperationalSourceAvailability.Available,
            ExtensionSourceReadState.Missing
                or ExtensionSourceReadState.Invalid
                or ExtensionSourceReadState.Blocked
                or ExtensionSourceReadState.Unavailable
                or ExtensionSourceReadState.Cancelled => OperationalSourceAvailability.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source.State,
                "The Extension source read state is not defined."),
        };
}
