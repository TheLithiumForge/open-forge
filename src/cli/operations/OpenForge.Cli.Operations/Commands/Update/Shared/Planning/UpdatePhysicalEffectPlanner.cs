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
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(observations);
        var observationsByIdentity = observations.ToDictionary(
            value => Identity(value.Comparison));
        var effects = new List<UpdatePlannedEffect>();
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
            if (selected.Any(value => value.Snapshot.Expectation != snapshot.Expectation))
            {
                throw new InvalidOperationException(
                    "Coalesced Update changes require one exact physical expectation.");
            }

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

        return effects;
    }

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
