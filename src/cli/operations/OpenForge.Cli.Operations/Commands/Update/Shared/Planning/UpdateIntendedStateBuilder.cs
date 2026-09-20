using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdateIntendedStateBuilder(
    UpdateComparisonReader targetReader,
    UpdateGeneratedNavigationPlanner navigationPlanner)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly UpdateComparisonReader _targetReader = targetReader;
    private readonly UpdateGeneratedNavigationPlanner _navigationPlanner = navigationPlanner;

    internal async ValueTask<UpdateIntendedStateBuild> BuildAsync(
        UpdateRequest request,
        FrameworkPayload payload,
        WorkspaceOwnershipRead ownership,
        WorkspaceSettingsDocument settings,
        CancellationToken cancellationToken)
    {
        var framework = ownership.Document.Framework;
        var targets = (framework?.Paths.Select(path => new UpdateOwnedTarget(path,
                path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath
                    ? WorkspaceOwnershipDefinitions.ManagedBlockRegion : null,
                FrameworkSourceAlignment.ReadAsset(request.Workspace, path, payload)?.Path)) ?? [])
            .Concat(framework?.Regions.Select(region => new UpdateOwnedTarget(region.Path, region.Region,
                region.Region == "entries" ? null
                    : FrameworkSourceAlignment.ReadAsset(request.Workspace, region.Path, payload)?.Path)) ?? [])
            .Where(target => FrameworkPayloadSelection.IncludesPath(target.Path, settings.RemovedCategories))
            .Distinct().ToArray();
        var selectedAssets = payload.Assets.Where(asset => FrameworkPayloadSelection.IncludesPath(asset.Path, settings.RemovedCategories)).ToArray();
        if (targets.Any(target => !IsAdmittedTarget(request, settings, target.Path)
            || target.Region is not (null or "entries" or WorkspaceOwnershipDefinitions.ManagedBlockRegion)
            || target.Region == WorkspaceOwnershipDefinitions.ManagedBlockRegion
                && target.Path is not (FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath))
            || targets.Select(target => target.Path).Distinct(StringComparer.Ordinal)
                .GroupBy(PortableWorkspacePath.CreatePortableKey, StringComparer.Ordinal).Any(group => group.Count() != 1))
            return Blocked(UpdateFindingCode.OwnershipObservation, WorkspaceOwnershipDefinitions.RelativePath,
                "Recorded Framework destinations could not be safely interpreted; no effects were inferred.");
        var selectedPaths = targets.Select(target => target.Path).Concat(selectedAssets.Select(asset => asset.Path)).ToArray();
        try
        {
            var otherPaths = ownership.Document.Extensions.SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path)))
                .Concat(LibraryRegistrationReader.ReadRegistrations(ownership.Document).Libraries
                    .SelectMany(library => LibraryPathIdentity.Mappings(library).Select(mapping => mapping.DestinationPath.Value)))
                .Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
            if (selectedPaths.FirstOrDefault(path => otherPaths.Contains(PortableWorkspacePath.CreatePortableKey(path))) is { } conflict)
                return Blocked(UpdateFindingCode.OwnershipConflict, conflict,
                    "Another owner records a selected Framework destination; no effects were planned.");
        }
        catch (ArgumentException exception)
        {
            return Blocked(UpdateFindingCode.OwnershipObservation, WorkspaceOwnershipDefinitions.RelativePath, exception.Message);
        }
        var retiredTargetPaths = request.Prune
            ? targets.Where(target => target.Region is null && target.SourceAssetPath is null)
                .Select(target => target.Path).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        var mappedAssets = selectedAssets.Concat(targets.Where(target => target.SourceAssetPath is not null
                && target.Path != target.SourceAssetPath)
            .Select(target => FrameworkPayloadAsset.Create(target.Path, payload.Find(target.SourceAssetPath!)!.Bytes.AsSpan())))
            .DistinctBy(asset => asset.Path, StringComparer.Ordinal).ToArray();
        var retiredWholeHosts = targets.Where(target => target.Region is null && target.SourceAssetPath is null)
            .Select(target => target.Path).ToHashSet(StringComparer.Ordinal);
        targets = targets.Where(target => target.Region != "entries" || !retiredWholeHosts.Contains(target.Path)).ToArray();
        var generatedHosts = targets.Where(target => target.Region == "entries").Select(target => target.Path).ToHashSet(StringComparer.Ordinal);
        var navigation = await _navigationPlanner
            .BuildAsync(request, mappedAssets, retiredTargetPaths, generatedHosts, cancellationToken)
            .ConfigureAwait(false);
        if (navigation.Finding is { } navigationFinding)
        {
            return new UpdateIntendedStateBuild(
                Observations: [],
                ProjectionInputs: [],
                navigationFinding,
                Cancelled: navigationFinding.Code == UpdateFindingCode.Interrupted);
        }

        var generated = targets.Where(target => target.Region == "entries")
            .Select(target => (target.Path, target.Region)).ToHashSet();
        var generatedPaths = generated.Select(target => target.Path).ToHashSet(StringComparer.Ordinal);
        var validatedGeneratedHosts = new HashSet<string>(StringComparer.Ordinal);
        var snapshots = new Dictionary<string, UpdateTargetRead>(StringComparer.Ordinal);
        var observations = new List<UpdateComparisonObservation>();
        foreach (var target in targets)
        {
            var read = await ReadTargetAsync(
                    request,
                    target.Path,
                    snapshots,
                    cancellationToken)
                .ConfigureAwait(false);
            var boundary = ReadBoundary(read);
            if (boundary is not null)
            {
                return boundary;
            }

            var snapshot = read.Snapshot
                ?? throw new InvalidOperationException(
                    "A complete Update target read requires a snapshot.");
            if (snapshot.Kind == FileExpectationKind.File
                && generatedPaths.Contains(target.Path)
                && validatedGeneratedHosts.Add(target.Path))
            {
                try
                {
                    ValidateGeneratedBoundary(snapshot.Bytes.AsSpan());
                }
                catch (Exception exception) when (exception is ArgumentException
                    or InvalidDataException
                    or InvalidOperationException)
                {
                    return Blocked(
                        UpdateFindingCode.GeneratedRegionUnsafe,
                        target.Path,
                        exception.Message);
                }
            }

            var isGenerated = generated.Contains((target.Path, target.Region));
            try
            {
                observations.Add(CreateExistingObservation(
                    target,
                    isGenerated,
                    mappedAssets,
                    navigation.TargetBytes,
                    snapshot,
                    settings));
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Blocked(
                    isGenerated
                        ? UpdateFindingCode.GeneratedRegionUnsafe
                        : UpdateFindingCode.SourceProvenanceInvalid,
                    target.Path,
                    exception.Message);
            }
        }

        var currentSources = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in targets)
        {
            if (target.SourceAssetPath is { } path)
            {
                currentSources.Add(path);
            }
        }

        foreach (var asset in selectedAssets.Where(asset => !currentSources.Contains(asset.Path)))
        {
            var read = await ReadTargetAsync(
                    request,
                    asset.Path,
                    snapshots,
                    cancellationToken)
                .ConfigureAwait(false);
            var boundary = ReadBoundary(read);
            if (boundary is not null)
            {
                return boundary;
            }

            var snapshot = read.Snapshot
                ?? throw new InvalidOperationException(
                    "A complete Update target read requires a snapshot.");
            if (snapshot.Kind != FileExpectationKind.Missing)
            {
                try
                {
                    var identity = asset.Path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath
                        ? ReadManagedFingerprint(snapshot.Bytes.AsSpan())
                        : _contentIdentity.ReadSourceFingerprint(snapshot.Bytes.AsSpan()).Sha256;
                    if (identity != _contentIdentity.ReadSourceFingerprint(asset.Bytes.AsSpan()).Sha256)
                        return Blocked(UpdateFindingCode.OwnershipConflict, asset.Path,
                            "An unowned current Framework target differs from intended content; no ownership was inferred.");
                }
                catch (InvalidDataException exception)
                {
                    return Blocked(UpdateFindingCode.OwnershipConflict, asset.Path, exception.Message);
                }
                continue;
            }

            observations.Add(CreateNewObservation(
                asset,
                navigation.TargetBytes.TryGetValue(asset.Path, out var projected)
                    ? projected
                    : asset.Bytes.ToArray(),
                snapshot));
        }

        var missingParent = observations.FirstOrDefault(observation => observation.Snapshot.Kind == FileExpectationKind.Missing
            && observation.IntendedDocumentBytes is not null
            && !Directory.Exists(Path.GetDirectoryName(observation.Snapshot.LogicalPath)));
        if (missingParent is not null)
        {
            return Blocked(UpdateFindingCode.TargetUnsafe, missingParent.Comparison.RelativePath,
                "A required Update destination parent directory is missing; no effects were planned.");
        }

        return Complete(observations, navigation.ProjectionInputs);
    }

    private static bool IsAdmittedTarget(UpdateRequest request, WorkspaceSettingsDocument settings, string path)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized) || normalized != path) return false;
        var key = PortableWorkspacePath.CreatePortableKey(path);
        string[] controls = [WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath,
            ".agents/open-forge.lock"];
        if (key.Split('/').Contains(".git", StringComparer.Ordinal) || SourceOverwritePath.HasSuffix(key)
            || controls.Any(control => key == control || key.StartsWith($"{control}/", StringComparison.Ordinal))) return false;
        var store = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        if (store is not null)
        {
            var storeKey = PortableWorkspacePath.CreatePortableKey(Path.GetRelativePath(request.Workspace.PhysicalRoot, store).Replace(Path.DirectorySeparatorChar, '/'));
            if (storeKey == "." || key == storeKey || key.StartsWith($"{storeKey}/", StringComparison.Ordinal)) return false;
        }
        return path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath
            || path.StartsWith(".agents/", StringComparison.Ordinal)
            || WorkspaceAllowList.Admits(settings.AllowInstallPaths, path);
    }

    private async ValueTask<UpdateTargetRead> ReadTargetAsync(
        UpdateRequest request,
        string path,
        IDictionary<string, UpdateTargetRead> snapshots,
        CancellationToken cancellationToken)
    {
        if (snapshots.TryGetValue(path, out var existing))
        {
            return existing;
        }

        var read = await _targetReader.ReadAsync(
                request.Workspace,
                path,
                cancellationToken)
            .ConfigureAwait(false);
        snapshots.Add(path, read);
        return read;
    }

    private UpdateComparisonObservation CreateExistingObservation(
        UpdateOwnedTarget target,
        bool generated,
        IReadOnlyList<FrameworkPayloadAsset> assets,
        IReadOnlyDictionary<string, byte[]> projectedTargetBytes,
        FileStateSnapshot snapshot,
        WorkspaceSettingsDocument settings)
    {
        UpdateComparisonTargetKind kind;
        if (generated)
        {
            kind = UpdateComparisonTargetKind.GeneratedRegion;
        }
        else if (target.Region is null)
        {
            kind = UpdateComparisonTargetKind.File;
        }
        else
        {
            kind = UpdateComparisonTargetKind.ManagedRegion;
        }
        var fingerprintKind = generated
            ? UpdateComparisonFingerprintKind.ExactBytes
            : UpdateComparisonFingerprintKind.OpenForgeMarkdownV1;
        var currentFingerprint = snapshot.Kind == FileExpectationKind.Missing
            ? null
            : ReadCurrentFingerprint(target, kind, snapshot.Bytes.AsSpan());
        var asset = assets.FirstOrDefault(asset => asset.Path == (target.SourceAssetPath ?? target.Path));
        var sourcePresent = generated ? (bool?)null : asset is not null;
        string? intendedFingerprint;
        byte[]? intendedDocumentBytes;
        byte[]? intendedTargetBytes = null;
        if (generated)
        {
            intendedTargetBytes = projectedTargetBytes.GetValueOrDefault(target.Path);
        }
        else if (asset is not null)
        {
            intendedTargetBytes = projectedTargetBytes.GetValueOrDefault(target.Path) ?? asset.Bytes.ToArray();
        }
        if (generated && intendedTargetBytes is null)
            throw new InvalidDataException("A recorded generated region has no complete current host projection.");
        if (intendedTargetBytes is null)
        {
            intendedFingerprint = null;
            intendedDocumentBytes = null;
        }
        else
        {
            intendedFingerprint = ReadIntendedFingerprint(target, kind, intendedTargetBytes);
            intendedDocumentBytes = ReadIntendedDocumentBytes(target, snapshot, intendedTargetBytes);
        }

        var currentState = ReadCurrentState(
            snapshot,
            currentFingerprint,
            intendedFingerprint,
            intendedDocumentBytes);
        UpdateComparisonIntendedState intendedState;
        if (intendedFingerprint is null)
        {
            intendedState = UpdateComparisonIntendedState.Retired;
        }
        else if (string.Equals(currentFingerprint, intendedFingerprint, StringComparison.Ordinal))
        {
            intendedState = UpdateComparisonIntendedState.Same;
        }
        else
        {
            intendedState = UpdateComparisonIntendedState.Changed;
        }
        var retirement = ReadRetirementEligibility(
            intendedState,
            currentState,
            kind == UpdateComparisonTargetKind.File
                && (target.Path.StartsWith(".agents/", StringComparison.Ordinal)
                    || WorkspaceAllowList.Admits(settings.AllowInstallPaths, target.Path)));
        var comparison = new UpdateComparison
        {
            RelativePath = target.Path,
            Kind = kind,
            RegionIdentity = target.Region,
            SourceAssetPath = target.SourceAssetPath,
            SourceAssetPresentInCurrentInventory = sourcePresent,
            FingerprintKind = fingerprintKind,
            CurrentFingerprint = currentFingerprint,
            IntendedFingerprint = intendedFingerprint,
            CurrentState = currentState,
            IntendedState = intendedState,
            RetirementEligibility = retirement,
            CurrentBytes = ByteFacts(snapshot.Kind == FileExpectationKind.File
                ? snapshot.Bytes.ToArray()
                : null),
            IntendedBytes = ByteFacts(intendedDocumentBytes),
        };
        comparison.Validate();
        return new UpdateComparisonObservation(
            comparison,
            snapshot,
            intendedDocumentBytes);
    }

    private UpdateComparisonObservation CreateNewObservation(
        FrameworkPayloadAsset asset,
        byte[] intended,
        FileStateSnapshot snapshot)
    {
        var identity = _contentIdentity.ReadSourceFingerprint(intended);
        if (!identity.IsSemantic || identity.Sha256 is null)
        {
            throw new InvalidDataException(
                identity.Cause ?? "A new embedded Framework target has no semantic identity.");
        }

        var comparison = new UpdateComparison
        {
            RelativePath = asset.Path,
            Kind = UpdateComparisonTargetKind.File,
            RegionIdentity = null,
            SourceAssetPath = asset.Path,
            SourceAssetPresentInCurrentInventory = true,
            FingerprintKind = UpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
            CurrentFingerprint = null,
            IntendedFingerprint = identity.Sha256,
            CurrentState = UpdateComparisonCurrentState.Missing,
            IntendedState = UpdateComparisonIntendedState.New,
            RetirementEligibility = UpdateRetirementEligibility.NotApplicable,
            CurrentBytes = ByteFacts(bytes: null),
            IntendedBytes = ByteFacts(intended),
        };
        comparison.Validate();
        return new UpdateComparisonObservation(comparison, snapshot, intended);
    }

    private string? ReadCurrentFingerprint(
        UpdateOwnedTarget target,
        UpdateComparisonTargetKind kind,
        ReadOnlySpan<byte> bytes)
        => kind switch
        {
            UpdateComparisonTargetKind.File when IsManagedRoot(target) => ReadManagedFingerprint(bytes),
            UpdateComparisonTargetKind.File => _contentIdentity.ReadSourceFingerprint(bytes).Sha256 ?? FileExpectation.Hash(bytes),
            UpdateComparisonTargetKind.ManagedRegion => ReadManagedFingerprint(bytes),
            UpdateComparisonTargetKind.GeneratedRegion =>
                ReadGeneratedCurrentFingerprint(bytes),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Update comparison target kind is not defined."),
        };

    private string? ReadGeneratedCurrentFingerprint(
        ReadOnlySpan<byte> bytes)
    {
        var region = _markdownParser.Parse(StrictUtf8.GetString(bytes)).GeneratedRegion;
        return region.State switch
        {
            MarkdownGeneratedRegionState.Complete =>
                _contentIdentity.ReadGeneratedEntriesFingerprint(bytes),
            MarkdownGeneratedRegionState.Absent =>
                throw new InvalidDataException(
                    "An ownership-recorded generated Update region is absent."),
            MarkdownGeneratedRegionState.Invalid or MarkdownGeneratedRegionState.Unavailable =>
                throw new InvalidDataException(
                    region.Cause ?? "The generated Update region is unsafe or unavailable."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region.State,
                "The Markdown generated-region state is not defined."),
        };
    }

    private void ValidateGeneratedBoundary(ReadOnlySpan<byte> bytes)
    {
        var region = _markdownParser.Parse(StrictUtf8.GetString(bytes)).GeneratedRegion;
        if (region.State != MarkdownGeneratedRegionState.Complete)
        {
            throw new InvalidDataException(
                region.Cause ?? "An ownership-recorded generated Update region is absent.");
        }
    }

    private string ReadIntendedFingerprint(
        UpdateOwnedTarget target,
        UpdateComparisonTargetKind kind,
        ReadOnlySpan<byte> bytes)
        => kind == UpdateComparisonTargetKind.GeneratedRegion
            ? _contentIdentity.ReadGeneratedEntriesFingerprint(bytes)
            : _contentIdentity.ReadSourceFingerprint(bytes).Sha256 ?? FileExpectation.Hash(bytes);

    private string ReadManagedFingerprint(ReadOnlySpan<byte> bytes)
    {
        var block = _contentIdentity.ReadManagedBlock(bytes);
        return block.State == FrameworkManagedBlockState.Present
            && block.ExistingBlockBytes is { } existing
            ? _contentIdentity.ReadSourceFingerprint(existing).Sha256 ?? FileExpectation.Hash(existing)
            : throw new InvalidDataException(
                block.Cause ?? "A managed Update target requires one complete marker boundary.");
    }

    private byte[] ReadIntendedDocumentBytes(
        UpdateOwnedTarget target,
        FileStateSnapshot snapshot,
        ReadOnlySpan<byte> intendedTargetBytes)
    {
        if (!IsManagedRoot(target))
        {
            return intendedTargetBytes.ToArray();
        }

        if (snapshot.Kind == FileExpectationKind.Missing)
        {
            return intendedTargetBytes.ToArray();
        }

        var current = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
        var block = _contentIdentity.ReadManagedBlock(current);
        if (block.State != FrameworkManagedBlockState.Present
            || block.Start is not { } start
            || block.EndExclusive is not { } end)
        {
            throw new InvalidDataException(
                block.Cause ?? "A managed Update host requires one complete marker boundary.");
        }

        var startByteOffset = StrictUtf8.GetByteCount(current.AsSpan(0, start));
        var endByteOffset = StrictUtf8.GetByteCount(current.AsSpan(0, end));
        return snapshot.Bytes.AsSpan()[..startByteOffset]
            .ToArray()
            .Concat(intendedTargetBytes.ToArray())
            .Concat(snapshot.Bytes.AsSpan()[endByteOffset..].ToArray())
            .ToArray();
    }

    private static bool IsManagedRoot(UpdateOwnedTarget target)
        => target.SourceAssetPath is not null
            && target.Path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath;

    private static UpdateComparisonCurrentState ReadCurrentState(
        FileStateSnapshot snapshot,
        string? current,
        string? intended,
        byte[]? intendedBytes)
    {
        if (snapshot.Kind == FileExpectationKind.Missing)
        {
            return UpdateComparisonCurrentState.Missing;
        }
        if (current is null)
        {
            return UpdateComparisonCurrentState.Missing;
        }
        if (!string.Equals(intended, current, StringComparison.Ordinal))
        {
            return UpdateComparisonCurrentState.Changed;
        }
        if (intendedBytes is not null
            && string.Equals(current, intended, StringComparison.Ordinal)
            && !snapshot.Bytes.AsSpan().SequenceEqual(intendedBytes))
        {
            return UpdateComparisonCurrentState.FormatOnly;
        }

        return UpdateComparisonCurrentState.Same;
    }

    private static UpdateRetirementEligibility ReadRetirementEligibility(
        UpdateComparisonIntendedState intended,
        UpdateComparisonCurrentState current,
        bool eligibleWholeFile)
    {
        if (intended != UpdateComparisonIntendedState.Retired
            || current == UpdateComparisonCurrentState.Missing)
        {
            return UpdateRetirementEligibility.NotApplicable;
        }

        return eligibleWholeFile
            ? UpdateRetirementEligibility.Eligible
            : UpdateRetirementEligibility.Ineligible;
    }

    private static UpdateComparisonByteFacts ByteFacts(byte[]? bytes)
        => bytes is null
            ? new UpdateComparisonByteFacts
            {
                ExactBytes = null,
                Sha256 = null,
            }
            : new UpdateComparisonByteFacts
            {
                ExactBytes = ImmutableArray.CreateRange(bytes),
                Sha256 = FileExpectation.Hash(bytes),
            };

    private static UpdateIntendedStateBuild? ReadBoundary(UpdateTargetRead read)
        => read.State switch
        {
            UpdateTargetReadState.Available or UpdateTargetReadState.Missing => null,
            UpdateTargetReadState.Unavailable => Incomplete(
                read.RelativePath,
                read.Cause ?? "The Update target is unavailable."),
            UpdateTargetReadState.Blocked => Blocked(
                UpdateFindingCode.TargetUnsafe,
                read.RelativePath,
                read.Cause ?? "The Update target is unsafe."),
            UpdateTargetReadState.Cancelled => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The Update target read state is not defined."),
        };

    private static UpdateIntendedStateBuild Complete(
        IReadOnlyList<UpdateComparisonObservation> observations,
        IReadOnlyList<FileStateSnapshot> projectionInputs)
        => new(observations, projectionInputs, Finding: null, Cancelled: false);

    private static UpdateIntendedStateBuild Incomplete(string target, string cause)
        => new(
            Observations: [],
            ProjectionInputs: [],
            new UpdateFinding(UpdateFindingCode.TargetUnavailable, target, cause),
            Cancelled: false);

    private static UpdateIntendedStateBuild Blocked(
        UpdateFindingCode code,
        string? target,
        string cause)
        => new(
            Observations: [],
            ProjectionInputs: [],
            new UpdateFinding(code, target, cause),
            Cancelled: false);

    private static UpdateIntendedStateBuild Cancelled()
        => new(
            Observations: [],
            ProjectionInputs: [],
            new UpdateFinding(
                UpdateFindingCode.Interrupted,
                target: null,
                "Update comparison was interrupted."),
            Cancelled: true);
}
