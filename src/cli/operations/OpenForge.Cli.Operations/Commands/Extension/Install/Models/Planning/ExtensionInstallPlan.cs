using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

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
    internal IReadOnlyList<ExtensionPackageFact> ValidationPackages { get; init; } = [];
    internal required FrameworkPayload FrameworkPayload { get; init; }
    internal required FrameworkOwnership? FrameworkOwnership { get; init; }
    internal required WorkspaceOwnershipRead Ownership { get; init; }
    internal required ImmutableArray<ExtensionOwnership> IntendedExtensions { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal IReadOnlyList<ExtensionInstallFinding> TopologyFindings { get; init; } = [];
    internal IReadOnlyList<string> ForceEligiblePaths { get; init; } = [];
    internal required ExtensionInstallResultFacts Facts { get; init; }
    internal required IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; init; }
    internal required PlannedFileChange? OwnershipChange { get; init; }
    internal required RecoveryBundleTarget? OwnershipRecoveryTarget { get; init; }
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
        ValidationPackages = Snapshot(
            input.ValidationPackages.Count == 0 ? input.Packages : input.ValidationPackages,
            nameof(input.ValidationPackages));
        FrameworkPayload = input.FrameworkPayload;
        FrameworkOwnership = input.FrameworkOwnership;
        Ownership = input.Ownership;
        IntendedExtensions = input.IntendedExtensions;
        Topology = input.Topology;
        TopologyFindings = Snapshot(input.TopologyFindings, nameof(input.TopologyFindings));
        ArgumentNullException.ThrowIfNull(input.ForceEligiblePaths);
        ForceEligiblePaths = new ReadOnlyCollection<string>([.. input.ForceEligiblePaths
            .Select(path => path ?? throw new ArgumentException(
                "Extension Install force eligible paths cannot contain null members.",
                nameof(input.ForceEligiblePaths)))
            .Order(StringComparer.Ordinal)]);
        Facts = ExtensionInstallResultFacts.Snapshot(input.Facts);
        Effects = Snapshot(input.Effects, nameof(input.Effects));
        OwnershipChange = input.OwnershipChange;
        OwnershipRecoveryTarget = input.OwnershipRecoveryTarget;
        DirectoryCreations = Snapshot(
            Effects.Select(effect => effect.DirectoryCreation).OfType<PlannedDirectoryCreation>(),
            nameof(input.Effects));
        TargetChanges = Snapshot(
            Effects.Select(effect => effect.FileChange).OfType<PlannedFileChange>(),
            nameof(input.Effects));
        AllFileChanges = Snapshot(
            TargetChanges
                .Concat(OwnershipChange is { } ownershipChange ? [ownershipChange] : []),
            nameof(input.OwnershipChange));
        RecoveryTargets = Snapshot(
            Effects.Select(effect => effect.RecoveryTarget).OfType<RecoveryBundleTarget>()
                .Concat(OwnershipRecoveryTarget is { } ownershipRecoveryTarget
                    ? [ownershipRecoveryTarget]
                    : []),
            nameof(input.OwnershipRecoveryTarget));
    }

    internal ExtensionInstallRequest Request { get; }
    internal ExtensionSourceReadResult SourceRead { get; }
    internal string SourceSignature { get; }
    internal string? InferredRootId { get; }
    internal ExtensionInstallSelection Selection { get; }
    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }
    internal IReadOnlyList<ExtensionPackageFact> ValidationPackages { get; }
    internal FrameworkPayload FrameworkPayload { get; }
    internal FrameworkOwnership? FrameworkOwnership { get; }
    internal WorkspaceOwnershipRead Ownership { get; }
    internal ImmutableArray<ExtensionOwnership> IntendedExtensions { get; }
    internal ExtensionInstallTopology Topology { get; }
    internal IReadOnlyList<ExtensionInstallFinding> TopologyFindings { get; }
    internal IReadOnlyList<string> ForceEligiblePaths { get; }
    internal ExtensionInstallResultFacts Facts { get; }
    internal IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; }
    internal PlannedFileChange? OwnershipChange { get; }
    internal RecoveryBundleTarget? OwnershipRecoveryTarget { get; }
    internal IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; }
    internal IReadOnlyList<PlannedFileChange> TargetChanges { get; }
    internal IReadOnlyList<PlannedFileChange> AllFileChanges { get; }
    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }
    internal bool IsNoOp => Effects.Count == 0
        && OwnershipChange is null;
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
        T[] snapshot =
        [
            .. values.Select(value => value ?? throw new ArgumentException(
                "Extension Install planning collections cannot contain null members.",
                parameterName)),
        ];
        return new ReadOnlyCollection<T>(snapshot);
    }
}

internal sealed record ExtensionInstallPlanBuild(
    ExtensionInstallPlan? Plan,
    ExtensionInstallResult Result);

internal sealed class ExtensionInstallTopologyBuild
{
    internal ExtensionInstallTopologyBuild(
        ExtensionInstallTopology topology,
        IEnumerable<ExtensionInstallFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(findings);

        Topology = topology;
        Findings = new ReadOnlyCollection<ExtensionInstallFinding>([.. findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Extension Install topology findings cannot contain null members.",
                nameof(findings)))]);
    }

    internal ExtensionInstallTopology Topology { get; }

    internal IReadOnlyList<ExtensionInstallFinding> Findings { get; }
}

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
        ExtensionInstallGeneratedRegion[] regionSnapshot =
        [
            .. regions.Select(region => region ?? throw new ArgumentException(
                "Extension Install topology regions cannot contain null members.",
                nameof(regions))),
        ];
        Regions = new ReadOnlyCollection<ExtensionInstallGeneratedRegion>(regionSnapshot);
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

    /// <summary>The ownership observation acquired before interactive selection.</summary>
    internal WorkspaceOwnershipRead? Ownership { get; init; }

    internal static IReadOnlyList<ExtensionPackageFact> SnapshotPackages(
        IEnumerable<ExtensionPackageFact> packages,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(packages, parameterName);
        ExtensionPackageFact[] snapshot =
        [
            .. packages.Select(package => package ?? throw new ArgumentException(
                "Extension Install package collections cannot contain null members.",
                parameterName)),
        ];
        return new ReadOnlyCollection<ExtensionPackageFact>(snapshot);
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
        WorkspaceOwnershipRead ownership,
        ExtensionInstallTopology topology,
        IEnumerable<ExtensionInstallFinding>? topologyFindings = null)
    {
        FrameworkPayload = frameworkPayload;
        Ownership = ownership;
        Topology = topology;
        TopologyFindings = new ReadOnlyCollection<ExtensionInstallFinding>([.. (topologyFindings
            ?? Array.Empty<ExtensionInstallFinding>())
            .Select(finding => finding ?? throw new ArgumentException(
                "Extension Install topology findings cannot contain null members.",
                nameof(topologyFindings)))]);
    }

    internal FrameworkPayload FrameworkPayload { get; }
    internal FrameworkOwnership? FrameworkOwnership => Ownership.Document.Framework;
    internal WorkspaceOwnershipRead Ownership { get; }
    internal ExtensionInstallTopology Topology { get; }
    internal IReadOnlyList<ExtensionInstallFinding> TopologyFindings { get; }
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
        MarkdownFingerprintState fingerprintKind,
        IEnumerable<string> owners)
    {
        Path = path;
        _bytes = [.. bytes];
        Fingerprint = fingerprint;
        FingerprintKind = fingerprintKind;
        string[] ownerSnapshot = [.. owners.Order(StringComparer.Ordinal)];
        Owners = new ReadOnlyCollection<string>(ownerSnapshot);
    }

    internal string Path { get; }
    internal byte[] Bytes => [.. _bytes];
    internal string Fingerprint { get; }
    internal MarkdownFingerprintState FingerprintKind { get; }
    internal IReadOnlyList<string> Owners { get; }
}

internal sealed class ExtensionInstallTargetState
{
    internal ExtensionInstallTargetState(
        IReadOnlyDictionary<string, FileStateSnapshot> observations,
        IReadOnlyDictionary<string, ExtensionInstallIntendedPath> intendedPaths,
        IEnumerable<string> eligibleOccupants,
        ImmutableArray<ExtensionOwnership> intendedExtensions)
    {
        Observations = new ReadOnlyDictionary<string, FileStateSnapshot>(
            observations.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        IntendedPaths = new ReadOnlyDictionary<string, ExtensionInstallIntendedPath>(
            intendedPaths.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal));
        string[] occupantSnapshot = [.. eligibleOccupants.Order(StringComparer.Ordinal)];
        EligibleOccupants = new ReadOnlyCollection<string>(occupantSnapshot);
        IntendedExtensions = intendedExtensions;
    }

    internal IReadOnlyDictionary<string, FileStateSnapshot> Observations { get; }
    internal IReadOnlyDictionary<string, ExtensionInstallIntendedPath> IntendedPaths { get; }
    internal IReadOnlyList<string> EligibleOccupants { get; }
    internal ImmutableArray<ExtensionOwnership> IntendedExtensions { get; }
}

internal sealed record ExtensionInstallTargetInspection(
    ExtensionInstallTargetState? State,
    ExtensionInstallFinding? Finding);

internal sealed record ExtensionInstallTargetInspectionInput
{
    internal required ExtensionInstallRequest Request { get; init; }
    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }
    internal IReadOnlySet<string> MutationPackageIds { get; init; }
        = new HashSet<string>(StringComparer.Ordinal);
    internal required WorkspaceOwnershipRead Ownership { get; init; }
    internal required FrameworkOwnership? FrameworkOwnership { get; init; }
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
    internal required WorkspaceOwnershipRead Ownership { get; init; }
}

internal sealed class ExtensionInstallEffectPlan
{
    internal ExtensionInstallEffectPlan(
        IEnumerable<ExtensionInstallPlannedEffect> effects,
        PlannedFileChange? ownershipChange,
        RecoveryBundleTarget? ownershipRecoveryTarget,
        ExtensionInstallLifecycleAction lifecycleAction,
        ExtensionInstallFinding? finding)
    {
        ExtensionInstallPlannedEffect[] effectSnapshot =
        [
            .. effects.Select(effect => effect ?? throw new ArgumentException(
                "Extension Install effect plans cannot contain null members.",
                nameof(effects))),
        ];
        Effects = new ReadOnlyCollection<ExtensionInstallPlannedEffect>(effectSnapshot);
        OwnershipChange = ownershipChange;
        OwnershipRecoveryTarget = ownershipRecoveryTarget;
        LifecycleAction = lifecycleAction;
        Finding = finding;
    }

    internal IReadOnlyList<ExtensionInstallPlannedEffect> Effects { get; }
    internal PlannedFileChange? OwnershipChange { get; }
    internal RecoveryBundleTarget? OwnershipRecoveryTarget { get; }
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
    internal required FrameworkOwnership? FrameworkOwnership { get; init; }
    internal required ExtensionInstallTopology Topology { get; init; }
    internal required ExtensionInstallEffectPlan EffectPlan { get; init; }
}
