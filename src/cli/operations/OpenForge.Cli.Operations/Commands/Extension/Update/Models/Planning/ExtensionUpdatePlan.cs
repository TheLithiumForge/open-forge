using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;

internal sealed record ExtensionUpdatePlanningAuthority(
    bool Force,
    bool Prune);

internal enum ExtensionUpdatePlanningDisposition
{
    NoOp,
    Create,
    Replace,
    Restore,
    Delete,
    Preserve,
    Blocked,
}

internal sealed record ExtensionUpdatePlanningDecision
{
    internal required ExtensionUpdateComparison Comparison { get; init; }

    internal required ExtensionUpdatePlanningDisposition Disposition { get; init; }
}

internal sealed record ExtensionUpdateTopology
{
    internal required IReadOnlyDictionary<string, byte[]> IntendedTargetBytes { get; init; }

    internal required IReadOnlyDictionary<string, byte[]> GeneratedTargetBytes { get; init; }

    internal required IReadOnlyList<ExtensionUpdateGeneratedRegion> Regions { get; init; }

    internal required IReadOnlyDictionary<string, IReadOnlyList<GeneratedNavigationEntry>> GeneratedEntries { get; init; }

    internal required IReadOnlySet<string> ProtectedPaths { get; init; }
}

internal sealed record ExtensionUpdatePlanningPlan
{
    internal required ExtensionUpdatePlanningAuthority Authority { get; init; }

    internal required IReadOnlyList<ExtensionUpdatePlanningDecision> Decisions { get; init; }

    internal bool IsNoOp => Decisions.All(
        decision => decision.Disposition == ExtensionUpdatePlanningDisposition.NoOp);

    internal bool IsEffectFree => Decisions.All(
        decision => decision.Disposition is ExtensionUpdatePlanningDisposition.NoOp
            or ExtensionUpdatePlanningDisposition.Preserve);

    internal bool IsBlocked => Decisions.Any(
        decision => decision.Disposition == ExtensionUpdatePlanningDisposition.Blocked);
}

internal sealed record ExtensionUpdatePlannedEffect
{
    internal required ExtensionUpdateEffect Result { get; init; }

    internal PlannedFileChange? FileChange { get; init; }

    internal RecoveryBundleTarget? RecoveryTarget { get; init; }
}

internal sealed record ExtensionUpdatePlanInput
{
    internal required ExtensionUpdateRequest Request { get; init; }

    internal required ExtensionSourceReadResult SourceRead { get; init; }

    internal required string SourceSignature { get; init; }

    internal required ExtensionUpdateSelection Selection { get; init; }

    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

    internal required WorkspaceOwnershipRead Ownership { get; init; }

    internal required ImmutableArray<ExtensionOwnership> IntendedOwnership { get; init; }

    internal required FrameworkPayload FrameworkPayload { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required ExtensionUpdateResultFacts Facts { get; init; }

    internal required IReadOnlyList<ExtensionUpdateFinding> Findings { get; init; }

    internal required IReadOnlyList<ExtensionUpdatePlannedEffect> Effects { get; init; }

    internal required IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; init; }

    internal PlannedFileChange? OwnershipChange { get; init; }

    internal RecoveryBundleTarget? OwnershipRecoveryTarget { get; init; }
}

internal sealed record ExtensionUpdatePlanBuild
{
    internal required ExtensionUpdatePlan? Plan { get; init; }

    internal required ExtensionUpdateResult Result { get; init; }
}

internal sealed class ExtensionUpdatePlan
{
    private ExtensionUpdatePlan(ExtensionUpdatePlanInput input)
    {
        Request = input.Request;
        SourceRead = input.SourceRead;
        SourceSignature = input.SourceSignature;
        Selection = new ExtensionUpdateSelection(
            input.Selection.SelectedBy,
            input.Selection.RootIds,
            input.Selection.FrozenIds);
        Packages = Snapshot(input.Packages, nameof(input.Packages));
        FrameworkPayload = input.FrameworkPayload;
        Ownership = input.Ownership;
        IntendedOwnership = input.IntendedOwnership;
        Topology = new ExtensionUpdateTopologySnapshot(input.Topology);
        Result = new ExtensionUpdateResult(new ExtensionUpdateResultFormation
        {
            Workspace = input.Request.Workspace,
            Mode = input.Request.Mode,
            Force = input.Request.Force,
            Prune = input.Request.Prune,
            Automatic = input.Request.Automatic,
            Facts = input.Facts,
            Findings = input.Findings,
        });
        Effects = Snapshot(input.Effects, nameof(input.Effects));
        DirectoryCreations = Snapshot(input.DirectoryCreations, nameof(input.DirectoryCreations));
        OwnershipChange = input.OwnershipChange;
        OwnershipRecoveryTarget = input.OwnershipRecoveryTarget;
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

    internal ExtensionUpdateRequest Request { get; }

    internal ExtensionSourceReadResult SourceRead { get; }

    internal string SourceSignature { get; }

    internal ExtensionUpdateSelection Selection { get; }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal WorkspaceOwnershipRead Ownership { get; }

    internal ImmutableArray<ExtensionOwnership> IntendedOwnership { get; }

    internal FrameworkPayload FrameworkPayload { get; }

    internal ExtensionUpdateResult Result { get; }

    internal ExtensionUpdateTopologySnapshot Topology { get; }

    internal IReadOnlyList<ExtensionUpdatePlannedEffect> Effects { get; }

    internal IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; }

    internal PlannedFileChange? OwnershipChange { get; }

    internal RecoveryBundleTarget? OwnershipRecoveryTarget { get; }

    internal IReadOnlyList<PlannedFileChange> TargetChanges { get; }

    internal IReadOnlyList<PlannedFileChange> AllFileChanges { get; }

    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool IsNoOp => DirectoryCreations.Count == 0
        && Effects.Count == 0
        && OwnershipChange is null;

    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal static ExtensionUpdatePlan Create(ExtensionUpdatePlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new ExtensionUpdatePlan(input);
    }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
        => new ReadOnlyCollection<T>([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update planning collections cannot contain null members.",
                parameterName))]);
}
