using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;
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
        LifecycleStoreReadResult lifecycle,
        CancellationToken cancellationToken)
    {
        var framework = lifecycle.Framework
            ?? throw new ArgumentException(
                "A trusted Update lifecycle read requires Framework state.",
                nameof(lifecycle));
        var retiredTargetPaths = framework.Targets
            .Where(target => target.SourceAssetPath is { } sourceAssetPath
                && payload.Find(sourceAssetPath) is null)
            .Select(target => target.Path)
            .ToHashSet(StringComparer.Ordinal);
        var navigation = await _navigationPlanner
            .BuildAsync(request, payload, retiredTargetPaths, cancellationToken)
            .ConfigureAwait(false);
        if (navigation.Finding is { } navigationFinding)
        {
            return new UpdateIntendedStateBuild(
                Observations: [],
                ProjectionInputs: [],
                navigationFinding,
                Cancelled: navigationFinding.Code == UpdateFindingCode.Interrupted);
        }

        var generated = framework.GeneratedRegions
            .Select(value => (value.Path, Region: (string?)value.Region))
            .ToHashSet();
        var generatedPaths = framework.GeneratedRegions
            .Select(value => value.Path)
            .ToHashSet(StringComparer.Ordinal);
        var validatedGeneratedHosts = new HashSet<string>(StringComparer.Ordinal);
        var snapshots = new Dictionary<string, UpdateTargetRead>(StringComparer.Ordinal);
        var observations = new List<UpdateComparisonObservation>();
        foreach (var target in framework.Targets)
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
                    payload,
                    navigation.TargetBytes,
                    snapshot));
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
        foreach (var target in framework.Targets)
        {
            if (target.SourceAssetPath is { } path)
            {
                currentSources.Add(path);
            }
        }

        foreach (var asset in payload.Assets.Where(asset => !currentSources.Contains(asset.Path)))
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
                return Blocked(
                    UpdateFindingCode.OwnershipConflict,
                    asset.Path,
                    "A new embedded Framework target is already occupied without trusted ownership.");
            }

            observations.Add(CreateNewObservation(
                asset,
                navigation.TargetBytes.TryGetValue(asset.Path, out var projected)
                    ? projected
                    : asset.Bytes.ToArray(),
                snapshot));
        }

        return Complete(observations, navigation.ProjectionInputs);
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
        FrameworkLifecycleTarget target,
        bool generated,
        FrameworkPayload payload,
        IReadOnlyDictionary<string, byte[]> projectedTargetBytes,
        FileStateSnapshot snapshot)
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
            : ReadFingerprintKind(target.FingerprintKind);
        var currentFingerprint = snapshot.Kind == FileExpectationKind.Missing
            ? null
            : ReadCurrentFingerprint(target, kind, snapshot.Bytes.AsSpan());
        var asset = target.SourceAssetPath is null
            ? payload.Find(target.Path)
            : payload.Find(target.SourceAssetPath);
        var sourcePresent = target.SourceAssetPath is null
            ? (bool?)null
            : asset is not null;
        string? intendedFingerprint;
        byte[]? intendedDocumentBytes;
        if (asset is null)
        {
            if (target.SourceAssetPath is null)
            {
                throw new InvalidDataException(
                    "A derived generated target has no current authored host payload.");
            }

            intendedFingerprint = null;
            intendedDocumentBytes = null;
        }
        else
        {
            var intendedTargetBytes = projectedTargetBytes.TryGetValue(
                target.Path,
                out var projected)
                ? projected
                : asset.Bytes.ToArray();
            intendedFingerprint = ReadIntendedFingerprint(
                target,
                kind,
                intendedTargetBytes);
            intendedDocumentBytes = ReadIntendedDocumentBytes(
                target,
                snapshot,
                intendedTargetBytes);
        }

        var currentState = ReadCurrentState(
            snapshot,
            target.BaselineFingerprint,
            currentFingerprint,
            intendedFingerprint,
            intendedDocumentBytes);
        UpdateComparisonIntendedState intendedState;
        if (intendedFingerprint is null)
        {
            intendedState = UpdateComparisonIntendedState.Retired;
        }
        else if (string.Equals(target.BaselineFingerprint, intendedFingerprint, StringComparison.Ordinal))
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
            target.BaselineFingerprint,
            currentFingerprint);
        var comparison = new UpdateComparison
        {
            RelativePath = target.Path,
            Kind = kind,
            RegionIdentity = target.Region,
            SourceAssetPath = target.SourceAssetPath,
            SourceAssetPresentInCurrentInventory = sourcePresent,
            FingerprintKind = fingerprintKind,
            BaselineFingerprint = target.BaselineFingerprint,
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
            intendedDocumentBytes,
            target);
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
            BaselineFingerprint = null,
            CurrentFingerprint = null,
            IntendedFingerprint = identity.Sha256,
            CurrentState = UpdateComparisonCurrentState.Missing,
            IntendedState = UpdateComparisonIntendedState.New,
            RetirementEligibility = UpdateRetirementEligibility.NotApplicable,
            CurrentBytes = ByteFacts(bytes: null),
            IntendedBytes = ByteFacts(intended),
        };
        comparison.Validate();
        return new UpdateComparisonObservation(comparison, snapshot, intended, LifecycleTarget: null);
    }

    private string? ReadCurrentFingerprint(
        FrameworkLifecycleTarget target,
        UpdateComparisonTargetKind kind,
        ReadOnlySpan<byte> bytes)
        => kind switch
        {
            UpdateComparisonTargetKind.File when IsManagedRoot(target) => ReadManagedFingerprint(
                bytes,
                target.FingerprintKind),
            UpdateComparisonTargetKind.File => _contentIdentity.ReadSourceFingerprint(
                bytes,
                target.FingerprintKind),
            UpdateComparisonTargetKind.ManagedRegion => ReadManagedFingerprint(
                bytes,
                target.FingerprintKind),
            UpdateComparisonTargetKind.GeneratedRegion =>
                ReadGeneratedCurrentFingerprint(bytes, target.FingerprintKind),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Update comparison target kind is not defined."),
        };

    private string? ReadGeneratedCurrentFingerprint(
        ReadOnlySpan<byte> bytes,
        string fingerprintKind)
    {
        var region = _markdownParser.Parse(StrictUtf8.GetString(bytes)).GeneratedRegion;
        return region.State switch
        {
            MarkdownGeneratedRegionState.Complete =>
                _contentIdentity.ReadGeneratedEntriesFingerprint(bytes, fingerprintKind),
            MarkdownGeneratedRegionState.Absent =>
                throw new InvalidDataException(
                    "A lifecycle-recorded generated Update region is absent."),
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
                region.Cause ?? "A lifecycle-recorded generated Update region is absent.");
        }
    }

    private string ReadIntendedFingerprint(
        FrameworkLifecycleTarget target,
        UpdateComparisonTargetKind kind,
        ReadOnlySpan<byte> bytes)
        => kind == UpdateComparisonTargetKind.GeneratedRegion
            ? _contentIdentity.ReadGeneratedEntriesFingerprint(bytes, target.FingerprintKind)
            : _contentIdentity.ReadSourceFingerprint(bytes, target.FingerprintKind);

    private string ReadManagedFingerprint(ReadOnlySpan<byte> bytes, string fingerprintKind)
    {
        var block = _contentIdentity.ReadManagedBlock(bytes);
        return block.State == FrameworkManagedBlockState.Present
            && block.ExistingBlockBytes is { } existing
            ? _contentIdentity.ReadSourceFingerprint(existing, fingerprintKind)
            : throw new InvalidDataException(
                block.Cause ?? "A managed Update target requires one complete marker boundary.");
    }

    private byte[] ReadIntendedDocumentBytes(
        FrameworkLifecycleTarget target,
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

    private static bool IsManagedRoot(FrameworkLifecycleTarget target)
        => target.SourceAssetPath is not null
            && target.Path is FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath;

    private static UpdateComparisonCurrentState ReadCurrentState(
        FileStateSnapshot snapshot,
        string baseline,
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
        if (!string.Equals(baseline, current, StringComparison.Ordinal))
        {
            return UpdateComparisonCurrentState.Changed;
        }
        if (intendedBytes is not null
            && string.Equals(current, intended, StringComparison.Ordinal)
            && !snapshot.Bytes.AsSpan().SequenceEqual(intendedBytes))
        {
            return UpdateComparisonCurrentState.FormatOnly;
        }

        return UpdateComparisonCurrentState.BaselineEquivalent;
    }

    private static UpdateRetirementEligibility ReadRetirementEligibility(
        UpdateComparisonIntendedState intended,
        UpdateComparisonCurrentState current,
        string baseline,
        string? currentFingerprint)
    {
        if (intended != UpdateComparisonIntendedState.Retired
            || current == UpdateComparisonCurrentState.Missing)
        {
            return UpdateRetirementEligibility.NotApplicable;
        }

        return string.Equals(baseline, currentFingerprint, StringComparison.Ordinal)
            ? UpdateRetirementEligibility.Eligible
            : UpdateRetirementEligibility.Ineligible;
    }

    private static UpdateComparisonFingerprintKind ReadFingerprintKind(string value)
        => value switch
        {
            LifecycleSchema.SemanticFingerprintKind =>
                UpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
            LifecycleSchema.ExactBytesFingerprintKind =>
                UpdateComparisonFingerprintKind.ExactBytes,
            _ => throw new InvalidDataException(
                "The lifecycle Framework target uses an unsupported fingerprint kind."),
        };

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
