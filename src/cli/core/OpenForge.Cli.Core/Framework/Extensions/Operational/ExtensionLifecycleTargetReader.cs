using System.Text;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

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
        var observations = new List<ExtensionManagedTargetObservation>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            observations.Add(new ExtensionManagedTargetObservation
            {
                Path = path.Path,
                Owners = path.Owners.ToArray(),
                BaselineFingerprint = path.BaselineFingerprint,
                FingerprintKind = path.FingerprintKind,
                State = await ReadStateAsync(workspace, path, cancellationToken).ConfigureAwait(false),
            });
        }

        return observations.ToArray();
    }

    private async ValueTask<OperationalTargetState> ReadStateAsync(
        CliWorkspace workspace,
        LifecycleInstalledPath path,
        CancellationToken cancellationToken)
    {
        var read = await _managedTargetReader
            .ReadAsync(workspace, path.Path, cancellationToken)
            .ConfigureAwait(false);
        var boundary = ReadBoundaryState(read.State, cancellationToken);
        if (boundary.HasValue)
        {
            return boundary.Value;
        }

        try
        {
            var fingerprint = _contentIdentity.ReadSourceFingerprint(
                read.Bytes.Span,
                path.FingerprintKind);
            return string.Equals(fingerprint, path.BaselineFingerprint, StringComparison.Ordinal)
                ? OperationalTargetState.Current
                : OperationalTargetState.Changed;
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return OperationalTargetState.Blocked;
        }
    }

    private static OperationalTargetState? ReadBoundaryState(
        LifecycleManagedTargetReadState state,
        CancellationToken cancellationToken)
        => state switch
        {
            LifecycleManagedTargetReadState.Available => null,
            LifecycleManagedTargetReadState.Missing => OperationalTargetState.Missing,
            LifecycleManagedTargetReadState.Unavailable => OperationalTargetState.Unavailable,
            LifecycleManagedTargetReadState.Blocked => OperationalTargetState.Blocked,
            LifecycleManagedTargetReadState.Cancelled
                => throw new OperationCanceledException(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension target read state is not defined."),
        };
}
