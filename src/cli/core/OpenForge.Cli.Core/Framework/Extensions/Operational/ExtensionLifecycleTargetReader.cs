using System.Text;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionLifecycleTargetReader(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly LifecycleManagedTargetReader _managedTargetReader = new(physicalPathResolver);

    internal async ValueTask<IReadOnlyList<ExtensionManagedTargetObservation>> ReadAsync(
        CliWorkspace workspace,
        IReadOnlyList<LifecycleInstalledPath> paths,
        CancellationToken cancellationToken)
    {
        var detailed = await ReadDoctorAsync(
            workspace,
            paths,
            cancellationToken).ConfigureAwait(false);
        return detailed.Select(observation => observation.Target).ToArray();
    }

    internal async ValueTask<IReadOnlyList<ExtensionManagedTargetDoctorObservation>> ReadDoctorAsync(
        CliWorkspace workspace,
        IReadOnlyList<LifecycleInstalledPath> paths,
        CancellationToken cancellationToken)
    {
        var observations = new List<ExtensionManagedTargetDoctorObservation>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await ReadStateAsync(
                workspace,
                path,
                cancellationToken).ConfigureAwait(false);
            var observation = new ExtensionManagedTargetObservation
            {
                Path = path.Path,
                Owners = path.Owners.ToArray(),
                BaselineFingerprint = path.BaselineFingerprint,
                FingerprintKind = path.FingerprintKind,
                State = read.State,
            };
            observations.Add(read.ReadState == LifecycleManagedTargetReadState.Available
                ? ExtensionManagedTargetDoctorObservation.Observed(
                    observation,
                    read.CurrentFingerprint
                        ?? throw new InvalidOperationException(
                            "An observed Extension target requires its current fingerprint."))
                : ExtensionManagedTargetDoctorObservation.Boundary(
                    observation,
                    read.ReadState,
                    read.Cause));
        }

        return observations.ToArray();
    }

    private async ValueTask<ExtensionManagedTargetDoctorRead> ReadStateAsync(
        CliWorkspace workspace,
        LifecycleInstalledPath path,
        CancellationToken cancellationToken)
    {
        var read = await _managedTargetReader
            .ReadAsync(workspace, path.Path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != LifecycleManagedTargetReadState.Available)
        {
            return ExtensionManagedTargetDoctorRead.Boundary(
                read.State,
                cancellationToken);
        }

        try
        {
            var fingerprint = _contentIdentity.ReadSourceFingerprint(
                read.Bytes.Span,
                path.FingerprintKind);
            var state = string.Equals(fingerprint, path.BaselineFingerprint, StringComparison.Ordinal)
                ? OperationalTargetState.Current
                : OperationalTargetState.Changed;
            return ExtensionManagedTargetDoctorRead.Observed(state, fingerprint);
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return ExtensionManagedTargetDoctorRead.Blocked(exception.Message);
        }
    }

    private sealed record ExtensionManagedTargetDoctorRead(
        OperationalTargetState State,
        LifecycleManagedTargetReadState ReadState,
        string? CurrentFingerprint,
        string? Cause)
    {
        internal static ExtensionManagedTargetDoctorRead Observed(
            OperationalTargetState state,
            string fingerprint)
            => new(state, LifecycleManagedTargetReadState.Available, fingerprint, Cause: null);

        internal static ExtensionManagedTargetDoctorRead Blocked(string cause)
            => new(
                OperationalTargetState.Blocked,
                LifecycleManagedTargetReadState.Blocked,
                CurrentFingerprint: null,
                cause);

        internal static ExtensionManagedTargetDoctorRead Boundary(
            LifecycleManagedTargetReadState state,
            CancellationToken cancellationToken)
            => state switch
            {
                LifecycleManagedTargetReadState.Missing => new(
                    OperationalTargetState.Missing,
                    state,
                    CurrentFingerprint: null,
                    Cause: null),
                LifecycleManagedTargetReadState.Unavailable => new(
                    OperationalTargetState.Unavailable,
                    state,
                    CurrentFingerprint: null,
                    "The Extension target is unavailable."),
                LifecycleManagedTargetReadState.Blocked => Blocked(
                    "The Extension target boundary is blocked."),
                LifecycleManagedTargetReadState.Cancelled =>
                    throw new OperationCanceledException(cancellationToken),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The Extension target read state is not defined."),
            };
    }
}
