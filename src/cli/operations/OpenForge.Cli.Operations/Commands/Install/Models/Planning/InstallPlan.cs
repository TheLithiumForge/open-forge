using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal enum InstallManagementState
{
    SafelyAbsent,
    EligibleInitialOccupant,
    TrustedExact,
    ManagedDivergence,
    Incomplete,
    Blocked,
    Interrupted,
}

internal sealed record InstallPlan
{
    public required InstallRequest Request { get; init; }

    public required FrameworkPayload Payload { get; init; }

    public required InstallIntendedState IntendedState { get; init; }

    public required InstallManagementState ManagementState { get; init; }


    public required IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; init; }

    public required IReadOnlyList<InstallFileEffect> TargetEffects { get; init; }


    public required InstallFileEffect? OwnershipEffect { get; init; }

    public required IReadOnlyList<InstallEffectIdentity> Effects { get; init; }

    public required IReadOnlyList<InstallFinding> Findings { get; init; }

    public IReadOnlyList<PlannedFileChange> FileChanges =>
        TargetEffects.Select(effect => effect.Change)

            .Concat(OwnershipEffect is { } ownershipEffect
                ? [ownershipEffect.Change]
                : [])
            .ToArray();

    public IReadOnlyList<RecoveryBundleTarget> RecoveryTargets =>
        TargetEffects.Select(effect => effect.RecoveryTarget)

            .Concat(OwnershipEffect is { } ownershipEffect
                ? [ownershipEffect.RecoveryTarget]
                : [])
            .ToArray();

    public bool IsComplete => Findings.Count == 0;

    public int PlannedFileCount => TargetEffects.Count

        + (OwnershipEffect is null ? 0 : 1);

    public bool IsNoOp => TargetEffects.Count == 0

        && OwnershipEffect is null;

    public bool RequiresRecovery =>
        TargetEffects.Any(static effect => effect.Change.Kind != PlannedFileChangeKind.Create)

        || (OwnershipEffect is { } ownershipEffect
            && ownershipEffect.Change.Kind != PlannedFileChangeKind.Create);
}

internal sealed record InstallPlanBuild
{
    public required InstallManagementState ManagementState { get; init; }

    public required InstallPlan? Plan { get; init; }

    public required IReadOnlyList<InstallFinding> Findings { get; init; }

    public required InstallPlanningEvidence Evidence { get; init; }
}

internal sealed record InstallPlanningEvidence
{
    public required FrameworkPayload? Payload { get; init; }

    public required InstallIntendedState? IntendedState { get; init; }
}

internal sealed record InstallEffectIdentity
{
    public required string Path { get; init; }

    public required InstallEffectKind Kind { get; init; }

    public required InstallEffectAction Action { get; init; }

    public required string? SourceAssetPath { get; init; }
}

internal sealed record InstallFileEffect
{
    public required InstallEffectIdentity Identity { get; init; }

    public required PlannedFileChange Change { get; init; }

    public required RecoveryBundleTarget RecoveryTarget { get; init; }

    public string RelativePath => Identity.Path;
}

internal sealed record InstallFileEffectInput
{
    public required InstallTargetRead Read { get; init; }

    public required byte[] IntendedBytes { get; init; }

    public required InstallEffectKind Kind { get; init; }

    public required InstallEffectAction Action { get; init; }

    public required string? SourceAssetPath { get; init; }
}

internal enum InstallTargetReadState
{
    Missing,
    File,
    Blocked,
    Unavailable,
    Cancelled,
}

internal sealed record InstallTargetRead
{
    public required string RelativePath { get; init; }

    public required InstallTargetReadState State { get; init; }

    public required FileStateSnapshot? Snapshot { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record InstallIntendedState
{
    public required IReadOnlyDictionary<string, byte[]> TargetBytes { get; init; }

    public required IReadOnlyDictionary<string, byte[]> ManagedBlockBytes { get; init; }

    public required IReadOnlySet<string> GeneratedRegionPaths { get; init; }

    public required IReadOnlyList<InstallProjectionInputObservation> ProjectionInputs { get; init; }
}

internal sealed record InstallProjectionInputObservation
{
    public required string AutomaticId { get; init; }

    public required string CanonicalBasePath { get; init; }

    public required string CanonicalLayerPath { get; init; }

    public required SourceDocumentForm Form { get; init; }

    public required SourceLayerKind Kind { get; init; }

    public required FileExpectation Expectation { get; init; }
}

internal enum InstallIntendedStateBuildState
{
    Complete,
    Incomplete,
    Blocked,
    Cancelled,
}

internal sealed record InstallIntendedStateBuild
{
    public required InstallIntendedStateBuildState State { get; init; }

    public required InstallIntendedState? IntendedState { get; init; }

    public required string? Cause { get; init; }

    public InstallFindingCode? FindingCode { get; init; }
}

internal sealed record InstallPlanContext
{
    public required InstallRequest Request { get; init; }

    public required FrameworkPayload Payload { get; init; }

    public required InstallIntendedState IntendedState { get; init; }


    public required FileExpectation AgentsDirectoryExpectation { get; init; }

}

internal sealed record InstallEstablishmentPlanInput
{
    public required InstallPlanContext Context { get; init; }


    public required WorkspaceOwnershipRead Ownership { get; init; }

    public required IReadOnlyDictionary<string, InstallTargetRead> CurrentTargets { get; init; }
}

internal sealed record InstallPlanEffects
{
    public required IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; init; }

    public required IReadOnlyList<InstallFileEffect> TargetEffects { get; init; }


    public required InstallFileEffect? OwnershipEffect { get; init; }
}
