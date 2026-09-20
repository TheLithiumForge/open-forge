using OpenForge.Cli.Core.Framework.Filesystem.Shared.Reading;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational;

internal sealed class FrameworkLifecycleTargetReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly FrameworkLifecycleTargetStateReader _targetStateReader = new(
        new ManagedTargetReader(physicalPathResolver));

    internal async ValueTask<IReadOnlyList<FrameworkManagedTargetObservation>> ReadAsync(
        CliWorkspace workspace,
        FrameworkOwnership ownership,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
        => (await ReadDoctorAsync(workspace, ownership, payload, cancellationToken).ConfigureAwait(false))
            .Select(observation => observation.Target).ToArray();

    internal async ValueTask<IReadOnlyList<FrameworkManagedTargetDoctorObservation>> ReadDoctorAsync(
        CliWorkspace workspace,
        FrameworkOwnership ownership,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var observations = new List<FrameworkManagedTargetDoctorObservation>();
        var identities = ownership.Paths.Select(path => (Path: path, Region: path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath
                ? WorkspaceOwnershipDefinitions.ManagedBlockRegion : (string?)null))
            .Concat(ownership.Regions.Select(region => (region.Path, Region: (string?)region.Region)))
            .Distinct()
            .OrderBy(identity => identity.Path, StringComparer.Ordinal)
            .ThenBy(identity => identity.Region, StringComparer.Ordinal);
        foreach (var (path, region) in identities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var intent = ReadIntent(workspace, path, region, payload);
            var read = await _targetStateReader.ReadAsync(workspace, intent, cancellationToken).ConfigureAwait(false);
            var target = new FrameworkManagedTargetObservation
            {
                Path = path,
                Kind = intent.Kind,
                SourceAssetPath = intent.SourceAssetPath,
                Region = intent.Region,
                IntendedFingerprint = intent.IntendedFingerprint,
                Source = intent.Source,
                Cause = read.Cause,
                State = read.State,
            };
            var boundary = path switch
            {
                FrameworkPayloadAsset.RootClaudePath => FrameworkManagedTargetBoundaryKind.ProviderBridge,
                FrameworkPayloadAsset.RootAgentPath => FrameworkManagedTargetBoundaryKind.RootRegion,
                _ => (FrameworkManagedTargetBoundaryKind?)null,
            };
            observations.Add(read.ReadState == ManagedTargetReadState.Available
                ? FrameworkManagedTargetDoctorObservation.Observed(target, boundary,
                    read.CurrentFingerprint ?? throw new InvalidOperationException("An observed target needs its current identity."))
                : FrameworkManagedTargetDoctorObservation.AtBoundary(target, boundary, read.ReadState, read.Cause));
        }

        return observations.ToArray();
    }

    private FrameworkManagedTargetIntent ReadIntent(CliWorkspace workspace, string path, string? region, FrameworkPayload payload)
    {
        var kind = FrameworkManagedTargetKind.File;
        if (region == WorkspaceOwnershipDefinitions.EntriesRegion)
        {
            kind = FrameworkManagedTargetKind.GeneratedRegion;
        }
        else if (path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath)
        {
            kind = FrameworkManagedTargetKind.ManagedRegion;
        }
        var asset = kind == FrameworkManagedTargetKind.GeneratedRegion ? null : FrameworkSourceAlignment.ReadAsset(workspace, path, payload);
        string? fingerprint = null;
        string? cause = null;
        if (region is not null && region != WorkspaceOwnershipDefinitions.EntriesRegion
            && !(kind == FrameworkManagedTargetKind.ManagedRegion && region == WorkspaceOwnershipDefinitions.ManagedBlockRegion))
        {
            cause = "The recorded Framework region has no supported current comparison.";
        }
        else if (kind != FrameworkManagedTargetKind.GeneratedRegion)
        {
            if (asset is null)
            {
                cause = "The running Framework payload does not identify this owned target.";
            }
            else
            {
                try
                {
                    fingerprint = _targetStateReader.ReadFingerprint(kind, asset.Bytes.AsSpan());
                }
                catch (Exception exception) when (exception is ArgumentException or InvalidDataException or InvalidOperationException)
                {
                    cause = exception.Message;
                }
            }
        }

        return new FrameworkManagedTargetIntent(path, kind,
            kind == FrameworkManagedTargetKind.GeneratedRegion ? region : null,
            asset?.Path, fingerprint,
            new FrameworkTargetSourceValidation(cause is null
                ? FrameworkTargetSourceState.Valid
                : FrameworkTargetSourceState.SourceMismatch, cause));
    }
}
