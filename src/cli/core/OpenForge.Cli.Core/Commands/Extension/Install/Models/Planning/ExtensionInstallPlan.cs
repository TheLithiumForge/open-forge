using System.Collections.ObjectModel;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;

internal sealed record ExtensionInstallPlannedEffect
{
    internal required ExtensionInstallEffect Result { get; init; }
    internal PlannedDirectoryCreation? DirectoryCreation { get; init; }
    internal PlannedFileChange? FileChange { get; init; }
    internal RecoveryBundleTarget? RecoveryTarget { get; init; }
    internal bool IsDirectory => DirectoryCreation is not null;
}

internal sealed record ExtensionInstallPlanInput
{
    internal required ExtensionInstallRequest Request { get; init; }
    internal required ExtensionSourceReadResult SourceRead { get; init; }
    internal required string SourceSignature { get; init; }
    internal required string? InferredRootId { get; init; }
    internal required ExtensionInstallSelection Selection { get; init; }
    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }
    internal required FrameworkPayload FrameworkPayload { get; init; }
    internal required FrameworkLifecycleState FrameworkLifecycle { get; init; }
    internal required ExtensionLifecycleState CurrentLifecycle { get; init; }
    internal required ExtensionLifecycleState IntendedLifecycle { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal required ExtensionInstallResultFacts Facts { get; init; }
    internal required IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; init; }
    internal required PlannedFileChange? LifecycleChange { get; init; }
    internal required RecoveryBundleTarget? LifecycleRecoveryTarget { get; init; }
}

internal sealed class ExtensionInstallPlan
{
    private ExtensionInstallPlan(ExtensionInstallPlanInput input)
    {
        Request = input.Request;
        SourceRead = input.SourceRead;
        SourceSignature = input.SourceSignature;
        InferredRootId = input.InferredRootId;
        Selection = new ExtensionInstallSelection(input.Selection.SelectedBy, input.Selection.RootIds);
        Packages = Snapshot(input.Packages, nameof(input.Packages));
        FrameworkPayload = input.FrameworkPayload;
        FrameworkLifecycle = ExtensionInstallLifecycleSnapshots.Framework(input.FrameworkLifecycle);
        CurrentLifecycle = ExtensionInstallLifecycleSnapshots.Extensions(input.CurrentLifecycle);
        IntendedLifecycle = ExtensionInstallLifecycleSnapshots.Extensions(input.IntendedLifecycle);
        Topology = ExtensionInstallTopology.Create(
            input.Topology.IntendedTargetBytes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.Ordinal),
            input.Topology.GeneratedTargetBytes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.Ordinal),
            input.Topology.Regions,
            input.Topology.ProtectedPaths,
            input.Topology.InitialForceEligiblePaths);
        Facts = ExtensionInstallResultFacts.Snapshot(input.Facts);
        Effects = Snapshot(input.Effects, nameof(input.Effects));
        LifecycleChange = input.LifecycleChange;
        LifecycleRecoveryTarget = input.LifecycleRecoveryTarget;
        DirectoryCreations = Snapshot(
            Effects.Select(effect => effect.DirectoryCreation).OfType<PlannedDirectoryCreation>(),
            nameof(input.Effects));
        TargetChanges = Snapshot(
            Effects.Select(effect => effect.FileChange).OfType<PlannedFileChange>(),
            nameof(input.Effects));
        AllFileChanges = LifecycleChange is null
            ? TargetChanges
            : Snapshot(TargetChanges.Append(LifecycleChange), nameof(input.LifecycleChange));
        RecoveryTargets = LifecycleRecoveryTarget is null
            ? Snapshot(
                Effects.Select(effect => effect.RecoveryTarget).OfType<RecoveryBundleTarget>(),
                nameof(input.Effects))
            : Snapshot(
                Effects.Select(effect => effect.RecoveryTarget)
                    .OfType<RecoveryBundleTarget>()
                    .Append(LifecycleRecoveryTarget),
                nameof(input.LifecycleRecoveryTarget));
    }

    internal ExtensionInstallRequest Request { get; }
    internal ExtensionSourceReadResult SourceRead { get; }
    internal string SourceSignature { get; }
    internal string? InferredRootId { get; }
    internal ExtensionInstallSelection Selection { get; }
    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }
    internal FrameworkPayload FrameworkPayload { get; }
    internal FrameworkLifecycleState FrameworkLifecycle { get; }
    internal ExtensionLifecycleState CurrentLifecycle { get; }
    internal ExtensionLifecycleState IntendedLifecycle { get; }
    internal ExtensionInstallTopology Topology { get; }
    internal ExtensionInstallResultFacts Facts { get; }
    internal IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; }
    internal PlannedFileChange? LifecycleChange { get; }
    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; }
    internal IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; }
    internal IReadOnlyList<PlannedFileChange> TargetChanges { get; }
    internal IReadOnlyList<PlannedFileChange> AllFileChanges { get; }
    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }
    internal bool IsNoOp => Effects.Count == 0 && LifecycleChange is null;
    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal static ExtensionInstallPlan Create(ExtensionInstallPlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new ExtensionInstallPlan(input);
    }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        return new ReadOnlyCollection<T>(values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Install planning collections cannot contain null members.",
                parameterName))
            .ToArray());
    }
}

internal sealed record ExtensionInstallPlanBuild(
    ExtensionInstallPlan? Plan,
    ExtensionInstallResult Result);

internal sealed class ExtensionInstallTopology
{
    private ExtensionInstallTopology(
        IReadOnlyDictionary<string, byte[]> intendedTargetBytes,
        IReadOnlyDictionary<string, byte[]> generatedTargetBytes,
        IReadOnlyList<ExtensionInstallGeneratedRegion> regions,
        IEnumerable<string> protectedPaths,
        IEnumerable<string> initialForceEligiblePaths)
    {
        IntendedTargetBytes = SnapshotBytes(intendedTargetBytes, nameof(intendedTargetBytes));
        GeneratedTargetBytes = SnapshotBytes(generatedTargetBytes, nameof(generatedTargetBytes));
        Regions = new ReadOnlyCollection<ExtensionInstallGeneratedRegion>(regions
            .Select(region => region ?? throw new ArgumentException(
                "Extension Install topology regions cannot contain null members.",
                nameof(regions)))
            .ToArray());
        ProtectedPaths = protectedPaths.ToImmutableHashSet(StringComparer.Ordinal);
        InitialForceEligiblePaths = initialForceEligiblePaths.ToImmutableHashSet(
            StringComparer.Ordinal);
    }

    internal IReadOnlyDictionary<string, ImmutableArray<byte>> IntendedTargetBytes { get; }
    internal IReadOnlyDictionary<string, ImmutableArray<byte>> GeneratedTargetBytes { get; }
    internal IReadOnlyList<ExtensionInstallGeneratedRegion> Regions { get; }
    internal IReadOnlySet<string> ProtectedPaths { get; }
    internal IReadOnlySet<string> InitialForceEligiblePaths { get; }

    internal static ExtensionInstallTopology Create(
        IReadOnlyDictionary<string, byte[]> intendedTargetBytes,
        IReadOnlyDictionary<string, byte[]> generatedTargetBytes,
        IReadOnlyList<ExtensionInstallGeneratedRegion> regions,
        IEnumerable<string>? protectedPaths = null,
        IEnumerable<string>? initialForceEligiblePaths = null)
        => new(
            intendedTargetBytes,
            generatedTargetBytes,
            regions,
            protectedPaths ?? [],
            initialForceEligiblePaths ?? []);

    private static IReadOnlyDictionary<string, ImmutableArray<byte>> SnapshotBytes(
        IReadOnlyDictionary<string, byte[]> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        return new ReadOnlyDictionary<string, ImmutableArray<byte>>(values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value is null
                ? throw new ArgumentException(
                    "Extension Install topology bytes cannot contain null values.",
                    parameterName)
                : ImmutableArray.CreateRange(pair.Value),
            StringComparer.Ordinal));
    }
}

internal sealed record ExtensionInstallSelectionResolution
{
    internal ExtensionInstallSelectionResolution(
        ExtensionInstallSelection? selection,
        IEnumerable<ExtensionPackageFact> packages,
        ExtensionInstallFinding? finding)
    {
        Selection = selection;
        Packages = SnapshotPackages(packages, nameof(packages));
        Finding = finding;
    }

    internal ExtensionInstallSelection? Selection { get; }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal ExtensionInstallFinding? Finding { get; }

    internal static IReadOnlyList<ExtensionPackageFact> SnapshotPackages(
        IEnumerable<ExtensionPackageFact> packages,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(packages, parameterName);
        return new ReadOnlyCollection<ExtensionPackageFact>(packages
            .Select(package => package ?? throw new ArgumentException(
                "Extension Install package collections cannot contain null members.",
                parameterName))
            .ToArray());
    }
}

internal sealed record ExtensionInstallDependencyClosureResolution
{
    internal ExtensionInstallDependencyClosureResolution(
        IEnumerable<ExtensionPackageFact> packages,
        ExtensionInstallFinding? finding)
    {
        Packages = ExtensionInstallSelectionResolution.SnapshotPackages(
            packages,
            nameof(packages));
        Finding = finding;
    }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal ExtensionInstallFinding? Finding { get; }
}

internal sealed record ExtensionInstallPayloadNormalization
{
    internal ExtensionInstallPayloadNormalization(
        IEnumerable<ExtensionPackageFact> packages,
        ExtensionInstallFinding? finding)
    {
        Packages = ExtensionInstallSelectionResolution.SnapshotPackages(
            packages,
            nameof(packages));
        Finding = finding;
    }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal ExtensionInstallFinding? Finding { get; }
}

internal sealed record ExtensionInstallSourceResolution(
    ExtensionSourceReadResult Source,
    string? InferredRootId);

internal sealed class ExtensionInstallFoundation
{
    internal ExtensionInstallFoundation(
        FrameworkPayload frameworkPayload,
        FrameworkLifecycleState frameworkLifecycle,
        LifecycleStoreReadResult extensionsRead,
        ExtensionLifecycleState currentExtensions,
        ExtensionInstallTopology topology)
    {
        FrameworkPayload = frameworkPayload;
        FrameworkLifecycle = ExtensionInstallLifecycleSnapshots.Framework(frameworkLifecycle);
        ExtensionsRead = extensionsRead;
        CurrentExtensions = ExtensionInstallLifecycleSnapshots.Extensions(currentExtensions);
        Topology = ExtensionInstallTopology.Create(
            topology.IntendedTargetBytes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.Ordinal),
            topology.GeneratedTargetBytes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.Ordinal),
            topology.Regions,
            topology.ProtectedPaths,
            topology.InitialForceEligiblePaths);
    }

    internal FrameworkPayload FrameworkPayload { get; }
    internal FrameworkLifecycleState FrameworkLifecycle { get; }
    internal LifecycleStoreReadResult ExtensionsRead { get; }
    internal ExtensionLifecycleState CurrentExtensions { get; }
    internal ExtensionInstallTopology Topology { get; }
}

internal sealed record ExtensionInstallFoundationObservation(
    ExtensionInstallFoundation? Foundation,
    ExtensionInstallFinding? Finding);

internal sealed record ExtensionInstallIntendedPath
{
    private readonly byte[] _bytes;

    internal ExtensionInstallIntendedPath(
        string path,
        byte[] bytes,
        string fingerprint,
        string fingerprintKind,
        IEnumerable<string> owners)
    {
        Path = path;
        _bytes = bytes.ToArray();
        Fingerprint = fingerprint;
        FingerprintKind = fingerprintKind;
        Owners = new ReadOnlyCollection<string>(owners.Order(StringComparer.Ordinal).ToArray());
    }

    internal string Path { get; }
    internal byte[] Bytes => _bytes.ToArray();
    internal string Fingerprint { get; }
    internal string FingerprintKind { get; }
    internal IReadOnlyList<string> Owners { get; }
}

internal sealed class ExtensionInstallTargetState
{
    internal ExtensionInstallTargetState(
        IReadOnlyDictionary<string, FileStateSnapshot> observations,
        IReadOnlyDictionary<string, ExtensionInstallIntendedPath> intendedPaths,
        IEnumerable<string> eligibleOccupants,
        ExtensionLifecycleState intendedLifecycle)
    {
        Observations = new ReadOnlyDictionary<string, FileStateSnapshot>(
            observations.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        IntendedPaths = new ReadOnlyDictionary<string, ExtensionInstallIntendedPath>(
            intendedPaths.ToDictionary(
                pair => pair.Key,
                pair => new ExtensionInstallIntendedPath(
                    pair.Value.Path,
                    pair.Value.Bytes,
                    pair.Value.Fingerprint,
                    pair.Value.FingerprintKind,
                    pair.Value.Owners),
                StringComparer.Ordinal));
        EligibleOccupants = new ReadOnlyCollection<string>(eligibleOccupants.Order(StringComparer.Ordinal).ToArray());
        IntendedLifecycle = ExtensionInstallLifecycleSnapshots.Extensions(intendedLifecycle);
    }

    internal IReadOnlyDictionary<string, FileStateSnapshot> Observations { get; }
    internal IReadOnlyDictionary<string, ExtensionInstallIntendedPath> IntendedPaths { get; }
    internal IReadOnlyList<string> EligibleOccupants { get; }
    internal ExtensionLifecycleState IntendedLifecycle { get; }
}

internal sealed record ExtensionInstallTargetInspection(
    ExtensionInstallTargetState? State,
    ExtensionInstallFinding? Finding);

internal sealed record ExtensionInstallTargetInspectionInput
{
    internal required ExtensionInstallRequest Request { get; init; }
    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }
    internal required ExtensionLifecycleState CurrentExtensions { get; init; }
    internal required FrameworkLifecycleState FrameworkLifecycle { get; init; }
    internal required IReadOnlySet<string> ProtectedAuthoredPaths { get; init; }
    internal required IReadOnlySet<string> InitialForceEligiblePaths { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal required string SourceIdentity { get; init; }
}

internal sealed record ExtensionInstallEffectPlanningInput
{
    internal required ExtensionInstallRequest Request { get; init; }
    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal required ExtensionInstallTargetState TargetState { get; init; }
    internal required LifecycleStoreReadResult LifecycleRead { get; init; }
}

internal sealed class ExtensionInstallEffectPlan
{
    internal ExtensionInstallEffectPlan(
        IEnumerable<ExtensionInstallPlannedEffect> effects,
        PlannedFileChange? lifecycleChange,
        RecoveryBundleTarget? lifecycleRecoveryTarget,
        ExtensionInstallLifecycleAction lifecycleAction,
        ExtensionInstallFinding? finding)
    {
        Effects = new ReadOnlyCollection<ExtensionInstallPlannedEffect>(effects
            .Select(effect => effect ?? throw new ArgumentException(
                "Extension Install effect plans cannot contain null members.",
                nameof(effects)))
            .ToArray());
        LifecycleChange = lifecycleChange;
        LifecycleRecoveryTarget = lifecycleRecoveryTarget;
        LifecycleAction = lifecycleAction;
        Finding = finding;
    }

    internal IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; }
    internal PlannedFileChange? LifecycleChange { get; }
    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; }
    internal ExtensionInstallLifecycleAction LifecycleAction { get; }
    internal ExtensionInstallFinding? Finding { get; }
}

internal sealed record ExtensionInstallBoundaryEvidence(
    ExtensionInstallSource? Source,
    ExtensionInstallSelection? Selection,
    IReadOnlyList<ExtensionInstallPackage> Packages)
{
    internal static ExtensionInstallBoundaryEvidence Empty { get; } = new(null, null, []);
}

internal sealed record ExtensionInstallPlannedFactsInput
{
    internal required ExtensionInstallRequest Request { get; init; }
    internal required ExtensionInstallSource Source { get; init; }
    internal required ExtensionInstallSelection Selection { get; init; }
    internal required IReadOnlyList<ExtensionInstallPackage> Packages { get; init; }
    internal required FrameworkPayload FrameworkPayload { get; init; }
    internal required FrameworkLifecycleState FrameworkLifecycle { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal required ExtensionInstallEffectPlan EffectPlan { get; init; }
}

internal static class ExtensionInstallLifecycleSnapshots
{
    internal static FrameworkLifecycleState Framework(FrameworkLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = value.Source.Id,
                Version = value.Source.Version,
                InventoryFingerprint = value.Source.InventoryFingerprint,
            },
            Targets = value.Targets.Select(target => new FrameworkLifecycleTarget
            {
                Path = target.Path,
                SourceAssetPath = target.SourceAssetPath,
                Region = target.Region,
                BaselineFingerprint = target.BaselineFingerprint,
                FingerprintKind = target.FingerprintKind,
            }).ToArray(),
            GeneratedRegions = value.GeneratedRegions.Select(region => new FrameworkGeneratedRegion
            {
                Path = region.Path,
                Region = region.Region,
            }).ToArray(),
        };

    internal static ExtensionLifecycleState Extensions(ExtensionLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Packages = value.Packages.Select(package => new LifecycleExtensionPackageV1
            {
                Id = package.Id,
                Version = package.Version,
                Source = package.Source,
                Dependencies = package.Dependencies.ToArray(),
                Paths = package.Paths.ToArray(),
            }).ToArray(),
            Paths = value.Paths.Select(path => new LifecycleExtensionPathV1
            {
                Path = path.Path,
                Owners = path.Owners.ToArray(),
                BaselineFingerprint = path.BaselineFingerprint,
                FingerprintKind = path.FingerprintKind,
            }).ToArray(),
        };
}
