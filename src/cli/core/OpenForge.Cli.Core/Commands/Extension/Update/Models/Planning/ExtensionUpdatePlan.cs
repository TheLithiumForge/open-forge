using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

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

    internal required FrameworkPayload FrameworkPayload { get; init; }

    internal required FrameworkLifecycleState FrameworkLifecycle { get; init; }

    internal required LifecycleStoreReadResult LifecycleRead { get; init; }

    internal required ExtensionLifecycleState CurrentLifecycle { get; init; }

    internal required ExtensionLifecycleState IntendedLifecycle { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required ExtensionUpdateResultFacts Facts { get; init; }

    internal required IReadOnlyList<ExtensionUpdateFinding> Findings { get; init; }

    internal required IReadOnlyList<ExtensionUpdatePlannedEffect> Effects { get; init; }

    internal required IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; init; }

    internal PlannedFileChange? LifecycleChange { get; init; }

    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; init; }
}

internal sealed record ExtensionUpdatePlanBuild
{
    internal required ExtensionUpdatePlan? Plan { get; init; }

    internal required ExtensionUpdateResult Result { get; init; }
}

internal sealed record ExtensionUpdatePlanResolution
{
    internal required ExtensionUpdatePlanBuild Build { get; init; }

    internal required ExtensionUpdatePlan? Execution { get; init; }
}

internal sealed class ExtensionUpdatePlan
{
    private ExtensionUpdatePlan(ExtensionUpdatePlanInput input)
    {
        Request = input.Request;
        SourceRead = input.SourceRead;
        SourceSignature = input.SourceSignature;
        Selection = new ExtensionUpdateSelection(input.Selection.SelectedBy, input.Selection.RootIds);
        Packages = Snapshot(input.Packages, nameof(input.Packages));
        FrameworkPayload = input.FrameworkPayload;
        FrameworkLifecycle = input.FrameworkLifecycle;
        LifecycleRead = input.LifecycleRead;
        CurrentLifecycle = input.CurrentLifecycle;
        IntendedLifecycle = input.IntendedLifecycle;
        Topology = input.Topology;
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
        LifecycleChange = input.LifecycleChange;
        LifecycleRecoveryTarget = input.LifecycleRecoveryTarget;
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

    internal ExtensionUpdateRequest Request { get; }

    internal ExtensionSourceReadResult SourceRead { get; }

    internal string SourceSignature { get; }

    internal ExtensionUpdateSelection Selection { get; }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal FrameworkPayload FrameworkPayload { get; }

    internal FrameworkLifecycleState FrameworkLifecycle { get; }

    internal LifecycleStoreReadResult LifecycleRead { get; }

    internal ExtensionLifecycleState CurrentLifecycle { get; }

    internal ExtensionLifecycleState IntendedLifecycle { get; }

    internal ExtensionUpdateResult Result { get; }

    internal ExtensionUpdateTopology Topology { get; }

    internal IReadOnlyList<ExtensionUpdatePlannedEffect> Effects { get; }

    internal IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; }

    internal PlannedFileChange? LifecycleChange { get; }

    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; }

    internal IReadOnlyList<PlannedFileChange> TargetChanges { get; }

    internal IReadOnlyList<PlannedFileChange> AllFileChanges { get; }

    internal IReadOnlyList<RecoveryBundleTarget> RecoveryTargets { get; }

    internal bool IsNoOp => DirectoryCreations.Count == 0
        && Effects.Count == 0
        && LifecycleChange is null;

    internal bool RequiresRecovery => RecoveryTargets.Any(target => target.RequiresRecovery);

    internal static ExtensionUpdatePlan Create(ExtensionUpdatePlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new ExtensionUpdatePlan(input);
    }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
        => new ReadOnlyCollection<T>(values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update planning collections cannot contain null members.",
                parameterName))
            .ToArray());
}
