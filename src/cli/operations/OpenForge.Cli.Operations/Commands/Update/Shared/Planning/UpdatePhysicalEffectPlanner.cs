using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal static class UpdatePhysicalEffectPlanner
{
    internal static IReadOnlyList<UpdatePlannedEffect> Plan(
        UpdatePlanningPlan plan,
        IReadOnlyList<UpdateComparisonObservation> observations)
        => Plan(plan, observations, []);

    internal static IReadOnlyList<UpdatePlannedEffect> Plan(
        UpdatePlanningPlan plan,
        IReadOnlyList<UpdateComparisonObservation> observations,
        IReadOnlyList<UpdateAdoptionTarget> adoptionTargets)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(adoptionTargets);
        var observationsByIdentity = observations.ToDictionary(
            value => Identity(value.Comparison));
        var effects = new List<UpdatePlannedEffect>();
        var snapshotsByPath = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        var observationsByPath = new Dictionary<string, IReadOnlyList<UpdateComparisonObservation>>(
            StringComparer.Ordinal);
        foreach (var group in plan.Decisions
                     .Where(decision => IsEffect(decision.Disposition))
                     .GroupBy(decision => decision.Comparison.RelativePath, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var decisions = group.ToArray();
            var selected = decisions
                .Select(decision => observationsByIdentity[Identity(decision.Comparison)])
                .ToArray();
            var snapshot = selected[0].Snapshot;
            if (selected.Any(value => !SnapshotsMatch(value.Snapshot, snapshot)))
            {
                throw new InvalidOperationException(
                    "Coalesced Update changes require one exact physical expectation.");
            }

            snapshotsByPath.Add(group.Key, snapshot);
            observationsByPath.Add(group.Key, selected);
            var logical = decisions.Select(CreateLogicalChange).ToArray();
            var physicalAction = ReadPhysicalAction(decisions);
            var result = new UpdatePhysicalEffect(
                group.Key,
                UpdatePhysicalEffectKind.File,
                physicalAction,
                logical,
                UpdatePhysicalEffectOutcome.Planned,
                UpdatePhysicalEffectResidual.None);
            effects.Add(new UpdatePlannedEffect(
                result,
                CreateFileChange(physicalAction, snapshot, selected)));
        }

        if (adoptionTargets.Count == 0)
        {
            return effects;
        }

        var mergedByPath = effects.ToDictionary(
            effect => effect.ResultEffect.Path,
            StringComparer.Ordinal);
        foreach (var group in adoptionTargets
                     .GroupBy(target => target.Path, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var target = CoalesceTargets(group);
            ValidateTarget(target);
            if (mergedByPath.TryGetValue(target.Path, out var planned))
            {
                if (!snapshotsByPath.TryGetValue(target.Path, out var plannedSnapshot)
                    || !SnapshotsMatch(target.Snapshot, plannedSnapshot)
                    || !planned.FileChange.HasIntendedBytes
                    || !planned.FileChange.IntendedBytes.AsSpan()
                        .SequenceEqual(target.IntendedDocumentBytes.AsSpan())
                    || !PlannedDocumentsMatch(observationsByPath[target.Path], target.IntendedDocumentBytes))
                {
                    throw new InvalidOperationException(
                        "A coalesced Update adoption target must preserve the exact snapshot and final bytes of every planned Framework change.");
                }

                mergedByPath[target.Path] = MergeAdoptionChange(planned, target);
                continue;
            }

            if (IsExactNoOp(target.Snapshot, target.IntendedDocumentBytes))
            {
                continue;
            }

            mergedByPath.Add(target.Path, CreateAdoptionEffect(target));
        }

        return mergedByPath.Values
            .OrderBy(effect => effect.ResultEffect.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static UpdateAdoptionTarget CoalesceTargets(IEnumerable<UpdateAdoptionTarget> targets)
    {
        var materialized = targets.ToArray();
        var selected = materialized[0];
        if (materialized.Skip(1).Any(target =>
                !SnapshotsMatch(selected.Snapshot, target.Snapshot)
                || !selected.IntendedDocumentBytes.AsSpan()
                    .SequenceEqual(target.IntendedDocumentBytes.AsSpan())
                || selected.OwnsGeneratedEntries != target.OwnsGeneratedEntries))
        {
            throw new InvalidOperationException(
                "Coalesced Update adoption targets require one exact snapshot, final document, and generated-Entries receipt state.");
        }

        return selected;
    }

    private static void ValidateTarget(UpdateAdoptionTarget target)
    {
        _ = ReadAdoptionAction(target.Snapshot);
    }

    private static UpdatePlannedEffect MergeAdoptionChange(
        UpdatePlannedEffect planned,
        UpdateAdoptionTarget target)
    {
        if (planned.ResultEffect.Kind != UpdatePhysicalEffectKind.File)
        {
            throw new InvalidOperationException(
                "An Update adoption target cannot coalesce with a non-file physical effect.");
        }

        var action = ReadAdoptionAction(target.Snapshot);
        if (planned.ResultEffect.Action != action)
        {
            throw new InvalidOperationException(
                "A coalesced Update adoption target must preserve the physical create or replace action.");
        }

        var logical = new UpdateLogicalChange(
            UpdateComparisonTargetKind.File,
            ToLogicalAction(action),
            region: null,
            sourceAssetPath: null);
        var result = new UpdatePhysicalEffect(
            planned.ResultEffect.Path,
            planned.ResultEffect.Kind,
            planned.ResultEffect.Action,
            planned.ResultEffect.Changes.Append(logical),
            planned.ResultEffect.Outcome,
            planned.ResultEffect.Residual);
        return planned with { ResultEffect = result };
    }

    private static UpdatePlannedEffect CreateAdoptionEffect(UpdateAdoptionTarget target)
    {
        var action = ReadAdoptionAction(target.Snapshot);
        var change = action switch
        {
            UpdatePhysicalEffectAction.Create => PlannedFileChange.Create(
                target.Snapshot.Expectation,
                target.IntendedDocumentBytes),
            UpdatePhysicalEffectAction.Replace => PlannedFileChange.Replace(
                target.Snapshot.Expectation,
                target.IntendedDocumentBytes),
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "An Update adoption target requires a create or replace action."),
        };
        var logical = new UpdateLogicalChange(
            UpdateComparisonTargetKind.File,
            ToLogicalAction(action),
            region: null,
            sourceAssetPath: null);
        var result = new UpdatePhysicalEffect(
            target.Path,
            action,
            [logical],
            UpdatePhysicalEffectOutcome.Planned,
            UpdatePhysicalEffectResidual.None);
        return new UpdatePlannedEffect(result, change);
    }

    private static UpdatePhysicalEffectAction ReadAdoptionAction(FileStateSnapshot snapshot)
        => snapshot.Kind switch
        {
            FileExpectationKind.Missing when !snapshot.HasBytes => UpdatePhysicalEffectAction.Create,
            FileExpectationKind.File when snapshot.HasBytes => UpdatePhysicalEffectAction.Replace,
            _ => throw new InvalidOperationException(
                "An Update adoption target requires an exact missing or ordinary-file snapshot."),
        };

    private static UpdateLogicalChangeAction ToLogicalAction(UpdatePhysicalEffectAction action)
        => action switch
        {
            UpdatePhysicalEffectAction.Create => UpdateLogicalChangeAction.Create,
            UpdatePhysicalEffectAction.Replace => UpdateLogicalChangeAction.Replace,
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "An Update adoption target requires a create or replace action."),
        };

    private static bool IsExactNoOp(FileStateSnapshot snapshot, byte[] intendedBytes)
        => snapshot.HasBytes && snapshot.Bytes.AsSpan().SequenceEqual(intendedBytes.AsSpan());

    private static bool PlannedDocumentsMatch(
        IReadOnlyList<UpdateComparisonObservation> observations,
        byte[] intendedDocumentBytes)
    {
        foreach (var observation in observations)
        {
            if (observation.IntendedDocumentBytes is { } plannedBytes
                && !plannedBytes.AsSpan().SequenceEqual(intendedDocumentBytes.AsSpan()))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SnapshotsMatch(FileStateSnapshot expected, FileStateSnapshot actual)
        => expected.Expectation == actual.Expectation
            && expected.HasBytes == actual.HasBytes
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());

    private static PlannedFileChange CreateFileChange(
        UpdatePhysicalEffectAction action,
        FileStateSnapshot snapshot,
        IReadOnlyList<UpdateComparisonObservation> observations)
    {
        if (action == UpdatePhysicalEffectAction.Delete)
        {
            return PlannedFileChange.Delete(snapshot.Expectation);
        }

        var intended = observations
            .Select(value => value.IntendedDocumentBytes)
            .FirstOrDefault(value => value is not null)
            ?? throw new InvalidOperationException(
                "An Update create or replacement requires complete intended document bytes.");
        return action switch
        {
            UpdatePhysicalEffectAction.Create => PlannedFileChange.Create(
                snapshot.Expectation,
                intended),
            UpdatePhysicalEffectAction.Replace => PlannedFileChange.Replace(
                snapshot.Expectation,
                intended),
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Update physical effect action is not defined."),
        };
    }

    private static UpdatePhysicalEffectAction ReadPhysicalAction(
        IReadOnlyList<UpdatePlanningDecision> decisions)
    {
        if (decisions.All(value => value.Disposition == UpdatePlanningDisposition.Delete))
        {
            return UpdatePhysicalEffectAction.Delete;
        }
        if (decisions.Any(value => value.Disposition == UpdatePlanningDisposition.Delete))
        {
            throw new InvalidOperationException(
                "Update cannot combine deletion with creation or replacement on one physical path.");
        }

        return decisions.All(value => value.Disposition is
                UpdatePlanningDisposition.Create or UpdatePlanningDisposition.Restore)
            ? UpdatePhysicalEffectAction.Create
            : UpdatePhysicalEffectAction.Replace;
    }

    private static UpdateLogicalChange CreateLogicalChange(UpdatePlanningDecision decision)
        => new(
            decision.Comparison.Kind,
            decision.Disposition switch
            {
                UpdatePlanningDisposition.Create => UpdateLogicalChangeAction.Create,
                UpdatePlanningDisposition.Replace => UpdateLogicalChangeAction.Replace,
                UpdatePlanningDisposition.Restore => UpdateLogicalChangeAction.Restore,
                UpdatePlanningDisposition.Delete => UpdateLogicalChangeAction.Delete,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(decision),
                    decision.Disposition,
                    "The Update planning disposition is not an effect."),
            },
            decision.Comparison.RegionIdentity,
            decision.Comparison.SourceAssetPath);

    private static bool IsEffect(UpdatePlanningDisposition disposition)
        => disposition is UpdatePlanningDisposition.Create
            or UpdatePlanningDisposition.Replace
            or UpdatePlanningDisposition.Restore
            or UpdatePlanningDisposition.Delete;

    private static (string Path, UpdateComparisonTargetKind Kind, string? Region) Identity(
        UpdateComparison comparison)
        => (comparison.RelativePath, comparison.Kind, comparison.RegionIdentity);
}
