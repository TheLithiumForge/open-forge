using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal sealed class FrameworkLifecycleTargetReader(
    PhysicalPathResolver physicalPathResolver)
{
    private readonly LifecycleManagedTargetReader _managedTargetReader = new(physicalPathResolver);
    private readonly FrameworkLifecycleTargetIdentity _targetIdentity = new();

    internal async ValueTask<IReadOnlyList<FrameworkManagedTargetObservation>> ReadAsync(
        CliWorkspace workspace,
        FrameworkLifecycleState lifecycle,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var generated = lifecycle.GeneratedRegions
            .Select(region => (region.Path, Region: (string?)region.Region))
            .ToHashSet();
        var observations = new List<FrameworkManagedTargetObservation>(lifecycle.Targets.Length);
        foreach (var target in lifecycle.Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = _targetIdentity.ValidateSource(target, generated, payload);
            var state = source.State == FrameworkLifecycleTargetSourceState.Blocked
                ? OperationalTargetState.Blocked
                : await ReadStateAsync(
                    workspace,
                    target,
                    generated,
                    cancellationToken).ConfigureAwait(false);
            observations.Add(new FrameworkManagedTargetObservation
            {
                Path = target.Path,
                Kind = ReadKind(target, generated),
                SourceAssetPath = target.SourceAssetPath,
                Region = target.Region,
                BaselineFingerprint = target.BaselineFingerprint,
                FingerprintKind = target.FingerprintKind,
                Source = source,
                State = state,
            });
        }

        return observations.ToArray();
    }

    private async ValueTask<OperationalTargetState> ReadStateAsync(
        CliWorkspace workspace,
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated,
        CancellationToken cancellationToken)
    {
        var read = await _managedTargetReader
            .ReadAsync(workspace, target.Path, cancellationToken)
            .ConfigureAwait(false);
        var boundary = ReadBoundaryState(read.State, cancellationToken);
        if (boundary.HasValue)
        {
            return boundary.Value;
        }

        try
        {
            var fingerprint = _targetIdentity.ReadFingerprint(
                target,
                generated,
                read.Bytes.Span);
            return string.Equals(
                fingerprint,
                target.BaselineFingerprint,
                StringComparison.Ordinal)
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
                "The Framework target read state is not defined."),
        };

    private FrameworkManagedTargetKind ReadKind(
        FrameworkLifecycleTarget target,
        IReadOnlySet<(string Path, string? Region)> generated)
    {
        if (_targetIdentity.IsGenerated(target, generated))
        {
            return FrameworkManagedTargetKind.GeneratedRegion;
        }

        if (target.Path is FrameworkPayloadAsset.RootAgentPath
            or FrameworkPayloadAsset.RootClaudePath)
        {
            return FrameworkManagedTargetKind.ManagedRegion;
        }

        return FrameworkManagedTargetKind.File;
    }
}
