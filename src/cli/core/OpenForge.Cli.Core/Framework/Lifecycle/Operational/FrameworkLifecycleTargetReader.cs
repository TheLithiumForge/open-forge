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
    private readonly FrameworkLifecycleTargetIdentity _targetIdentity = new();
    private readonly FrameworkLifecycleTargetStateReader _targetStateReader = new(
        new LifecycleManagedTargetReader(physicalPathResolver));

    internal async ValueTask<IReadOnlyList<FrameworkManagedTargetObservation>> ReadAsync(
        CliWorkspace workspace,
        FrameworkLifecycleState lifecycle,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var generated = lifecycle.GeneratedRegions
            .Select(region => (region.Path, Region: (string?)region.Region))
            .ToHashSet();
        var observations = new List<FrameworkManagedTargetObservation>(
            lifecycle.Targets.Length);
        foreach (var target in lifecycle.Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = _targetIdentity.ValidateSource(target, generated, payload);
            var state = source.State == FrameworkLifecycleTargetSourceState.Blocked
                ? OperationalTargetState.Blocked
                : (await _targetStateReader.ReadAsync(
                    workspace,
                    target,
                    generated,
                    cancellationToken).ConfigureAwait(false)).State;
            observations.Add(CreateTarget(target, source, generated, state));
        }

        return observations;
    }

    internal async ValueTask<IReadOnlyList<FrameworkManagedTargetDoctorObservation>> ReadDoctorAsync(
        CliWorkspace workspace,
        FrameworkLifecycleState lifecycle,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var generated = lifecycle.GeneratedRegions
            .Select(region => (region.Path, Region: (string?)region.Region))
            .ToHashSet();
        var observations = new List<FrameworkManagedTargetDoctorObservation>(lifecycle.Targets.Length);
        foreach (var target in lifecycle.Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = _targetIdentity.ValidateSource(target, generated, payload);
            if (source.State != FrameworkLifecycleTargetSourceState.Valid)
            {
                observations.Add(FrameworkManagedTargetDoctorObservation.SourceInvalid(
                    CreateTarget(
                        target,
                        source,
                        generated,
                        OperationalTargetState.Blocked),
                    source.Cause ?? "The Framework target source is invalid."));
                continue;
            }

            var read = await _targetStateReader.ReadAsync(
                    workspace,
                    target,
                    generated,
                    cancellationToken)
                .ConfigureAwait(false);
            var observation = CreateTarget(target, source, generated, read.State);
            var boundary = ReadBoundary(target);
            observations.Add(read.ReadState == LifecycleManagedTargetReadState.Available
                ? FrameworkManagedTargetDoctorObservation.Observed(
                    observation,
                    boundary,
                    read.CurrentFingerprint
                        ?? throw new InvalidOperationException(
                            "An observed Framework target requires its current fingerprint."))
                : FrameworkManagedTargetDoctorObservation.AtBoundary(
                    observation,
                    boundary,
                    read.ReadState,
                    read.Cause));
        }

        return observations.ToArray();
    }

    private static FrameworkManagedTargetBoundaryKind? ReadBoundary(
        FrameworkLifecycleTarget target)
        => target.Path switch
        {
            FrameworkPayloadAsset.RootClaudePath =>
                FrameworkManagedTargetBoundaryKind.ProviderBridge,
            FrameworkPayloadAsset.RootAgentPath =>
                FrameworkManagedTargetBoundaryKind.RootRegion,
            _ => null,
        };

    private FrameworkManagedTargetObservation CreateTarget(
        FrameworkLifecycleTarget target,
        FrameworkLifecycleTargetSourceValidation source,
        IReadOnlySet<(string Path, string? Region)> generated,
        OperationalTargetState state)
        => new()
        {
            Path = target.Path,
            Kind = ReadKind(target, generated),
            SourceAssetPath = target.SourceAssetPath,
            Region = target.Region,
            BaselineFingerprint = target.BaselineFingerprint,
            FingerprintKind = target.FingerprintKind,
            Source = source,
            State = state,
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
