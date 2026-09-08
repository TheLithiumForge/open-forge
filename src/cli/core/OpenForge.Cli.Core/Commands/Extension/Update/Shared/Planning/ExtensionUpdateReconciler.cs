using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal sealed class ExtensionUpdateReconciler(FileExpectationValidator validator)
{
    private readonly FileExpectationValidator _validator = validator;
    private readonly FrameworkContentIdentity _contentIdentity = new();

    internal async ValueTask<ExtensionUpdateReconciliation> BuildAsync(
        ExtensionUpdateReconciliationInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        var packages = input.Packages;
        var current = input.Current;
        var topology = input.Topology;
        var packageById = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var currentPackages = current.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var currentPaths = current.Paths.ToDictionary(path => path.Path, StringComparer.Ordinal);
        var selectedIds = packages.Select(package => package.Id).ToHashSet(StringComparer.Ordinal);
        var intendedOwners = packages
            .SelectMany(package => package.Payload.Select(file => (Path: RequiredPath(file), package.Id)))
            .GroupBy(value => value.Path, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(value => value.Id).ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);
        var intendedProvenance = packages
            .SelectMany(package => package.Payload.Select(file => (
                Path: RequiredPath(file),
                file.Path)))
            .GroupBy(value => value.Path, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(value => value.Path)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        var comparisons = new List<ExtensionUpdateComparison>();
        var effects = new List<ExtensionUpdatePlannedEffect>();
        var directories = new Dictionary<string, PlannedDirectoryCreation>(StringComparer.Ordinal);
        var findings = new List<ExtensionUpdateFinding>();
        var admittedOverrides = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var admittedExclusions = new HashSet<string>(StringComparer.Ordinal);
        var plannedPaths = new HashSet<string>(StringComparer.Ordinal);
        var nextPaths = current.Paths.ToDictionary(path => path.Path, SnapshotPath, StringComparer.Ordinal);
        var newPathsByPackage = packages.ToDictionary(
            package => package.Id,
            package => package.Payload.Select(RequiredPath).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
        var stage = new ReconciliationStage
        {
            Comparisons = comparisons,
            Effects = effects,
            Findings = findings,
            AdmittedOverrides = admittedOverrides,
            AdmittedExclusions = admittedExclusions,
            PlannedPaths = plannedPaths,
            SelectedIds = selectedIds,
            IntendedOwners = intendedOwners,
            IntendedProvenance = intendedProvenance,
            NextPaths = nextPaths,
            Current = current,
        };

        foreach (var path in topology.IntendedTargetBytes.Keys.Order(StringComparer.Ordinal))
        {
            var directoryBoundary = await PlanDirectoriesAsync(
                request,
                path,
                directories,
                cancellationToken).ConfigureAwait(false);
            if (directoryBoundary is not null)
            {
                return new ExtensionUpdateReconciliation(
                    comparisons,
                    effects,
                    [.. directories.Values],
                    current,
                    findings,
                    admittedOverrides,
                    admittedExclusions,
                    directoryBoundary);
            }
        }

        foreach (var package in packages)
        {
            var installedPackage = currentPackages[package.Id];
            var sourceFiles = package.Payload.ToDictionary(RequiredPath, StringComparer.Ordinal);
            var paths = installedPackage.Paths.Concat(sourceFiles.Keys)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal);
            foreach (var path in paths)
            {
                var boundary = await ReconcilePackagePathAsync(
                    new PackagePathInput(
                        request,
                        package,
                        sourceFiles.GetValueOrDefault(path),
                        path,
                        currentPaths.GetValueOrDefault(path),
                        topology),
                    stage,
                    cancellationToken).ConfigureAwait(false);
                if (boundary is not null)
                {
                    return boundary;
                }
            }
        }

        foreach (var generated in topology.GeneratedTargetBytes.OrderBy(
                     pair => pair.Key,
                     StringComparer.Ordinal))
        {
            var snapshot = await ObserveAsync(request, generated.Key, cancellationToken)
                .ConfigureAwait(false);
            if (snapshot is null || snapshot.Kind != FileExpectationKind.File)
            {
                return Stop(
                    stage,
                    new ExtensionUpdateFinding(
                        ExtensionUpdateFindingCode.GeneratedRegionUnsafe,
                        "A generated navigation host is unsafe or unavailable.",
                        generated.Key));
            }

            if (snapshot.Bytes.AsSpan().SequenceEqual(generated.Value))
            {
                continue;
            }

            var change = PlannedFileChange.ReplaceGeneratedRegion(
                snapshot.Expectation,
                generated.Value);
            effects.Add(new ExtensionUpdatePlannedEffect
            {
                Result = new ExtensionUpdateEffect(
                    generated.Key,
                    packageId: null,
                    ExtensionUpdateEffectKind.GeneratedRegion,
                    ExtensionUpdateEffectAction.Replace,
                    [new ExtensionUpdateLogicalChange(
                        ExtensionUpdateComparisonTargetKind.GeneratedRegion,
                        ExtensionUpdateChangeAction.Replace,
                        generated.Key,
                        sourceAssetPath: null)],
                    ExtensionUpdateEffectOutcome.Planned,
                    ExtensionUpdateEffectResidual.None),
                FileChange = change,
                RecoveryTarget = RecoveryBundleTarget.Create(change, snapshot),
            });
        }

        var intendedPackages = current.Packages.Select(installedPackage =>
        {
            if (!packageById.TryGetValue(installedPackage.Id, out var package))
            {
                return SnapshotPackage(installedPackage);
            }

            var preservedPaths = findings.Select(finding => finding.Target)
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);
            var paths = request.Prune
                ? newPathsByPackage[package.Id]
                    .Concat(installedPackage.Paths.Where(preservedPaths.Contains))
                    .ToHashSet(StringComparer.Ordinal)
                : installedPackage.Paths.Concat(newPathsByPackage[package.Id])
                    .ToHashSet(StringComparer.Ordinal);
            return new LifecycleExtensionPackageV1
            {
                Id = package.Id,
                Version = package.Version,
                Source = input.SourceIdentity,
                Dependencies = [.. package.Dependencies.Order(StringComparer.Ordinal)],
                Paths = [.. paths.Order(StringComparer.Ordinal)],
            };
        }).OrderBy(package => package.Id, StringComparer.Ordinal).ToArray();
        return new ExtensionUpdateReconciliation(
            comparisons,
            effects,
            [.. directories.Values
                .OrderBy(value => value.LogicalPath.Count(character =>
                    character == Path.DirectorySeparatorChar))
                .ThenBy(value => value.LogicalPath, StringComparer.Ordinal)],
            new ExtensionLifecycleState
            {
                Coverage = LifecycleSchema.CompleteCoverage,
                Packages = intendedPackages,
                Paths = [.. nextPaths.Values.OrderBy(path => path.Path, StringComparer.Ordinal)],
            },
            findings,
            admittedOverrides,
            admittedExclusions,
            Finding: null);
    }

    internal async ValueTask<ExtensionUpdateReconciliation> ReprojectGeneratedAsync(
        ExtensionUpdateRequest request,
        ExtensionUpdateTopology topology,
        ExtensionUpdateReconciliation reconciliation,
        CancellationToken cancellationToken)
    {
        var effects = reconciliation.Effects
            .Where(effect => effect.Result.Kind == ExtensionUpdateEffectKind.PackageFile)
            .ToList();
        if (effects.Count == 0)
        {
            return reconciliation with { Effects = effects };
        }

        foreach (var generated in topology.GeneratedTargetBytes.OrderBy(
                     pair => pair.Key,
                     StringComparer.Ordinal))
        {
            var snapshot = await ObserveAsync(request, generated.Key, cancellationToken)
                .ConfigureAwait(false);
            if (snapshot is null || snapshot.Kind != FileExpectationKind.File)
            {
                return reconciliation with
                {
                    Effects = effects,
                    Finding = new ExtensionUpdateFinding(
                        ExtensionUpdateFindingCode.GeneratedRegionUnsafe,
                        "A generated navigation host is unsafe or unavailable.",
                        generated.Key),
                };
            }

            if (snapshot.Bytes.AsSpan().SequenceEqual(generated.Value))
            {
                continue;
            }

            var change = PlannedFileChange.ReplaceGeneratedRegion(
                snapshot.Expectation,
                generated.Value);
            effects.Add(new ExtensionUpdatePlannedEffect
            {
                Result = new ExtensionUpdateEffect(
                    generated.Key,
                    packageId: null,
                    ExtensionUpdateEffectKind.GeneratedRegion,
                    ExtensionUpdateEffectAction.Replace,
                    [new ExtensionUpdateLogicalChange(
                        ExtensionUpdateComparisonTargetKind.GeneratedRegion,
                        ExtensionUpdateChangeAction.Replace,
                        generated.Key,
                        sourceAssetPath: null)],
                    ExtensionUpdateEffectOutcome.Planned,
                    ExtensionUpdateEffectResidual.None),
                FileChange = change,
                RecoveryTarget = RecoveryBundleTarget.Create(change, snapshot),
            });
        }

        return reconciliation with { Effects = effects };
    }

    private async ValueTask<ExtensionUpdateFinding?> PlanDirectoriesAsync(
        ExtensionUpdateRequest request,
        string relativePath,
        Dictionary<string, PlannedDirectoryCreation> directories,
        CancellationToken cancellationToken)
    {
        var parent = Path.GetDirectoryName(relativePath.Replace('/', Path.DirectorySeparatorChar));
        while (!string.IsNullOrEmpty(parent)
            && parent.Replace(Path.DirectorySeparatorChar, '/') != ".agents")
        {
            var canonical = parent.Replace(Path.DirectorySeparatorChar, '/');
            if (!directories.ContainsKey(canonical))
            {
                var snapshot = await ObserveAsync(request, canonical, cancellationToken)
                    .ConfigureAwait(false);
                if (snapshot is null)
                {
                    return new ExtensionUpdateFinding(
                        ExtensionUpdateFindingCode.TargetUnsafe,
                        "A required Extension target parent is unavailable.",
                        canonical);
                }

                if (snapshot.Kind == FileExpectationKind.Missing)
                {
                    directories.Add(
                        canonical,
                        PlannedDirectoryCreation.Create(snapshot.Expectation));
                }
                else if (snapshot.Kind != FileExpectationKind.Directory)
                {
                    return new ExtensionUpdateFinding(
                        ExtensionUpdateFindingCode.TargetUnsafe,
                        "A required Extension target parent is not an ordinary directory.",
                        canonical);
                }
            }

            parent = Path.GetDirectoryName(parent);
        }

        return null;
    }

    private async ValueTask<ExtensionUpdateReconciliation?> ReconcilePackagePathAsync(
        PackagePathInput input,
        ReconciliationStage stage,
        CancellationToken cancellationToken)
    {
        var comparisons = stage.Comparisons;
        var effects = stage.Effects;
        var findings = stage.Findings;
        var admittedOverrides = stage.AdmittedOverrides;
        var admittedExclusions = stage.AdmittedExclusions;
        var plannedPaths = stage.PlannedPaths;
        var selectedIds = stage.SelectedIds;
        var intendedOwners = stage.IntendedOwners;
        var intendedProvenance = stage.IntendedProvenance;
        var nextPaths = stage.NextPaths;
        var snapshot = await ObserveAsync(input.Request, input.Path, cancellationToken)
            .ConfigureAwait(false);
        if (snapshot is null || snapshot.Kind == FileExpectationKind.Directory)
        {
            return Stop(
                stage,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.TargetUnsafe,
                    "A managed Extension target is unsafe or unavailable.",
                    input.Path));
        }

        var currentFingerprint = snapshot.Kind == FileExpectationKind.File
            && input.Managed is not null
            ? _contentIdentity.ReadSourceFingerprint(
                snapshot.Bytes.AsSpan(),
                input.Managed.FingerprintKind)
            : null;
        var currentState = ExtensionUpdateComparisonCurrentState.Changed;
        if (snapshot.Kind == FileExpectationKind.Missing)
        {
            currentState = ExtensionUpdateComparisonCurrentState.Missing;
        }
        else if (input.Managed is not null && string.Equals(currentFingerprint, input.Managed.BaselineFingerprint, StringComparison.Ordinal))
        {
            currentState = ExtensionUpdateComparisonCurrentState.BaselineEquivalent;
        }
        var intendedBytes = input.SourceFile is null ? null : input.Topology.IntendedTargetBytes[input.Path];
        (string? Fingerprint, string? Kind) intendedIdentity = default;
        if (intendedBytes is not null)
        {
            intendedIdentity = ExtensionDestinationPolicy.IsImplicit(input.Path)
                ? Fingerprint(intendedBytes)
                : (FileExpectation.Hash(intendedBytes), LifecycleSchema.ExactBytesFingerprintKind);
        }
        var intendedState = ExtensionUpdateComparisonIntendedState.Changed;
        if (input.SourceFile is null)
        {
            intendedState = ExtensionUpdateComparisonIntendedState.Retired;
        }
        else if (input.Managed is null)
        {
            intendedState = ExtensionUpdateComparisonIntendedState.New;
        }
        else if (string.Equals(input.Managed.BaselineFingerprint, intendedIdentity.Fingerprint, StringComparison.Ordinal))
        {
            intendedState = ExtensionUpdateComparisonIntendedState.Same;
        }
        var retirement = ExtensionUpdateRetirementEligibility.NotApplicable;
        if (input.SourceFile is null)
        {
            retirement = currentState == ExtensionUpdateComparisonCurrentState.BaselineEquivalent
                ? ExtensionUpdateRetirementEligibility.Eligible
                : ExtensionUpdateRetirementEligibility.Ineligible;
        }
        comparisons.Add(new ExtensionUpdateComparison(
            input.Path,
            input.Package.Id,
            ExtensionUpdateComparisonTargetKind.PackageFile,
            region: null,
            input.SourceFile?.Path,
            ReadFingerprintKind(input.Managed?.FingerprintKind ?? intendedIdentity.Kind),
            input.Managed?.BaselineFingerprint,
            currentFingerprint,
            intendedIdentity.Fingerprint,
            currentState,
            intendedState,
            retirement));

        if (input.SourceFile is null)
        {
            return ReconcileRetired(
                input,
                snapshot,
                currentState,
                stage);
        }

        if (input.Managed is null && snapshot.Kind == FileExpectationKind.File)
        {
            return Stop(
                stage,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.OwnershipConflict,
                    "Semantic equality never adopts an unowned existing Extension target.",
                    input.Path));
        }

        var retainedOwner = input.Managed?.Owners.Any(owner => !selectedIds.Contains(owner)) == true;
        if (retainedOwner && !string.Equals(
                input.Managed?.BaselineFingerprint,
                intendedIdentity.Fingerprint,
                StringComparison.Ordinal))
        {
            return Stop(
                stage,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.OwnershipConflict,
                    "A retained shared owner blocks an incompatible Extension target rewrite.",
                    input.Path));
        }

        if (input.Managed is not null
            && (currentState == ExtensionUpdateComparisonCurrentState.Changed
                || currentState == ExtensionUpdateComparisonCurrentState.Missing))
        {
            if (!input.Request.Force)
            {
                findings.Add(new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.ManagedDivergence,
                    "The managed Extension target differs from its trusted baseline.",
                    input.Path));
                if (snapshot.Kind == FileExpectationKind.File)
                {
                    admittedOverrides[input.Path] = [.. snapshot.Bytes];
                }
                else
                {
                    _ = admittedExclusions.Add(input.Path);
                }

                return null;
            }
        }

        if ((snapshot.Kind == FileExpectationKind.Missing
                || !string.Equals(
                    currentFingerprint,
                    intendedIdentity.Fingerprint,
                    StringComparison.Ordinal))
            && plannedPaths.Add(input.Path))
        {
            var change = snapshot.Kind == FileExpectationKind.Missing
                ? PlannedFileChange.Create(snapshot.Expectation, intendedBytes)
                : PlannedFileChange.Replace(snapshot.Expectation, intendedBytes);
            var logicalAction = ExtensionUpdateChangeAction.Replace;
            if (snapshot.Kind == FileExpectationKind.Missing)
            {
                logicalAction = input.Managed is null ? ExtensionUpdateChangeAction.Create : ExtensionUpdateChangeAction.Restore;
            }
            effects.Add(Effect(new PackageEffectInput
            {
                Path = input.Path,
                PackageId = input.Package.Id,
                Action = snapshot.Kind == FileExpectationKind.Missing
                    ? ExtensionUpdateEffectAction.Create
                    : ExtensionUpdateEffectAction.Replace,
                LogicalAction = logicalAction,
                SourceAssetPaths = intendedProvenance[input.Path],
                Change = change,
                Before = snapshot,
            }));
        }

        var owners = (input.Managed?.Owners ?? [])
            .Where(owner => !selectedIds.Contains(owner))
            .Concat(intendedOwners[input.Path])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        nextPaths[input.Path] = new LifecycleExtensionPathV1
        {
            Path = input.Path,
            Owners = owners,
            BaselineFingerprint = intendedIdentity.Fingerprint
                ?? throw new InvalidDataException("An intended target requires its fingerprint."),
            FingerprintKind = intendedIdentity.Kind
                ?? throw new InvalidDataException("An intended target requires its fingerprint kind."),
        };
        return null;
    }

    private static ExtensionUpdateReconciliation? ReconcileRetired(
        PackagePathInput input,
        FileStateSnapshot snapshot,
        ExtensionUpdateComparisonCurrentState currentState,
        ReconciliationStage stage)
    {
        var effects = stage.Effects;
        var findings = stage.Findings;
        var plannedPaths = stage.PlannedPaths;
        var selectedIds = stage.SelectedIds;
        var intendedOwners = stage.IntendedOwners;
        var admittedOverrides = stage.AdmittedOverrides;
        var admittedExclusions = stage.AdmittedExclusions;
        var nextPaths = stage.NextPaths;
        if (!input.Request.Prune)
        {
            findings.Add(new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.ManagedDivergence,
                "A retired managed Extension target was preserved without --prune.",
                input.Path));
            return null;
        }

        var remainingOwners = (input.Managed?.Owners ?? [])
            .Where(owner => !selectedIds.Contains(owner))
            .Concat(intendedOwners.GetValueOrDefault(input.Path) ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (remainingOwners.Length > 0)
        {
            if (snapshot.Kind == FileExpectationKind.File)
            {
                admittedOverrides[input.Path] = [.. snapshot.Bytes];
            }
            else
            {
                _ = admittedExclusions.Add(input.Path);
            }

            if (input.Managed is not null)
            {
                nextPaths[input.Path] = SnapshotPath(input.Managed, remainingOwners);
            }

            return null;
        }

        if (currentState != ExtensionUpdateComparisonCurrentState.BaselineEquivalent)
        {
            findings.Add(new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.ManagedDivergence,
                "A retired managed target differs from its trusted baseline.",
                input.Path));
            if (snapshot.Kind == FileExpectationKind.File)
            {
                admittedOverrides[input.Path] = [.. snapshot.Bytes];
            }
            else
            {
                _ = admittedExclusions.Add(input.Path);
            }

            return null;
        }

        if (plannedPaths.Add(input.Path))
        {
            effects.Add(Effect(new PackageEffectInput
            {
                Path = input.Path,
                PackageId = input.Package.Id,
                Action = ExtensionUpdateEffectAction.Delete,
                LogicalAction = ExtensionUpdateChangeAction.Delete,
                SourceAssetPaths = [input.Path],
                Change = PlannedFileChange.Delete(snapshot.Expectation),
                Before = snapshot,
            }));
        }
        if (nextPaths.TryGetValue(input.Path, out var retired))
        {
            var owners = retired.Owners.Where(owner => owner != input.Package.Id).ToArray();
            if (owners.Length == 0)
            {
                _ = nextPaths.Remove(input.Path);
            }
            else
            {
                nextPaths[input.Path] = SnapshotPath(retired, owners);
            }
        }

        return null;
    }

    private async ValueTask<FileStateSnapshot?> ObserveAsync(
        ExtensionUpdateRequest request,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var logical = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var check = await _validator.ValidateAsync(
            request.Workspace,
            FileExpectation.Missing(logical),
            cancellationToken).ConfigureAwait(false);
        return check.State is FileExpectationValidationState.Matched
            or FileExpectationValidationState.Mismatched
            ? check.Actual
            : null;
    }

    private (string? Fingerprint, string? Kind) Fingerprint(byte[] bytes)
    {
        var facts = _contentIdentity.ReadSourceFingerprint(bytes);
        return facts.Sha256 is null
            ? (FileExpectation.Hash(bytes), LifecycleSchema.ExactBytesFingerprintKind)
            : (facts.Sha256, facts.State == MarkdownFingerprintState.Semantic
                ? LifecycleSchema.SemanticFingerprintKind
                : LifecycleSchema.ExactBytesFingerprintKind);
    }

    private static ExtensionUpdatePlannedEffect Effect(
        PackageEffectInput input)
        => new()
        {
            Result = new ExtensionUpdateEffect(
                input.Path,
                input.PackageId,
                ExtensionUpdateEffectKind.PackageFile,
                input.Action,
                input.SourceAssetPaths.Distinct(StringComparer.Ordinal).Select(sourceAssetPath =>
                    new ExtensionUpdateLogicalChange(
                    ExtensionUpdateComparisonTargetKind.PackageFile,
                    input.LogicalAction,
                    region: null,
                    sourceAssetPath)),
                ExtensionUpdateEffectOutcome.Planned,
                ExtensionUpdateEffectResidual.None),
            FileChange = input.Change,
            RecoveryTarget = RecoveryBundleTarget.Create(input.Change, input.Before),
        };

    private static ExtensionUpdateReconciliation Stop(
        ReconciliationStage stage,
        ExtensionUpdateFinding finding)
        => new(
            stage.Comparisons,
            stage.Effects,
            DirectoryCreations: [],
            stage.Current,
            Findings: [],
            AdmittedOverrides: new Dictionary<string, byte[]>(StringComparer.Ordinal),
            AdmittedExclusions: new HashSet<string>(StringComparer.Ordinal),
            finding);

    private static string RequiredPath(ExtensionPackageFileFact file)
        => file.TargetPath ?? throw new InvalidDataException(
            "A selected Extension payload requires its normalized target path.");

    private static ExtensionUpdateComparisonFingerprintKind ReadFingerprintKind(string? kind)
        => kind == LifecycleSchema.SemanticFingerprintKind
            ? ExtensionUpdateComparisonFingerprintKind.OpenForgeMarkdownV1
            : ExtensionUpdateComparisonFingerprintKind.ExactBytes;

    private static LifecycleExtensionPathV1 SnapshotPath(LifecycleExtensionPathV1 path)
        => SnapshotPath(path, path.Owners);

    private static LifecycleExtensionPathV1 SnapshotPath(
        LifecycleExtensionPathV1 path,
        IEnumerable<string> owners)
        => new()
        {
            Path = path.Path,
            Owners = [.. owners.Order(StringComparer.Ordinal)],
            BaselineFingerprint = path.BaselineFingerprint,
            FingerprintKind = path.FingerprintKind,
        };

    private static LifecycleExtensionPackageV1 SnapshotPackage(LifecycleExtensionPackageV1 package)
        => new()
        {
            Id = package.Id,
            Version = package.Version,
            Source = package.Source,
            Dependencies = [.. package.Dependencies],
            Paths = [.. package.Paths],
        };

    private sealed record PackagePathInput(
        ExtensionUpdateRequest Request,
        ExtensionPackageFact Package,
        ExtensionPackageFileFact? SourceFile,
        string Path,
        LifecycleExtensionPathV1? Managed,
        ExtensionUpdateTopology Topology);

    private sealed class PackageEffectInput
    {
        internal required string Path { get; init; }

        internal required string PackageId { get; init; }

        internal required ExtensionUpdateEffectAction Action { get; init; }

        internal required ExtensionUpdateChangeAction LogicalAction { get; init; }

        internal required IEnumerable<string> SourceAssetPaths { get; init; }

        internal required PlannedFileChange Change { get; init; }

        internal required FileStateSnapshot Before { get; init; }
    }

    private sealed class ReconciliationStage
    {
        internal required List<ExtensionUpdateComparison> Comparisons { get; init; }

        internal required List<ExtensionUpdatePlannedEffect> Effects { get; init; }

        internal required List<ExtensionUpdateFinding> Findings { get; init; }

        internal required Dictionary<string, byte[]> AdmittedOverrides { get; init; }

        internal required HashSet<string> AdmittedExclusions { get; init; }

        internal required HashSet<string> PlannedPaths { get; init; }

        internal required IReadOnlySet<string> SelectedIds { get; init; }

        internal required IReadOnlyDictionary<string, HashSet<string>> IntendedOwners { get; init; }

        internal required IReadOnlyDictionary<string, string[]> IntendedProvenance { get; init; }

        internal required Dictionary<string, LifecycleExtensionPathV1> NextPaths { get; init; }

        internal required ExtensionLifecycleState Current { get; init; }
    }
}

internal sealed class ExtensionUpdateReconciliationInput
{
    internal required ExtensionUpdateRequest Request { get; init; }

    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

    internal required ExtensionLifecycleState Current { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required string SourceIdentity { get; init; }
}

internal sealed record ExtensionUpdateReconciliation(
    IReadOnlyList<ExtensionUpdateComparison> Comparisons,
    IReadOnlyList<ExtensionUpdatePlannedEffect> Effects,
    IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations,
    ExtensionLifecycleState IntendedLifecycle,
    IReadOnlyList<ExtensionUpdateFinding> Findings,
    IReadOnlyDictionary<string, byte[]> AdmittedOverrides,
    IReadOnlySet<string> AdmittedExclusions,
    ExtensionUpdateFinding? Finding);
