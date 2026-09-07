using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairEffectPlanner
{
    private static readonly RepairDependency Dependency = new(
    [
        RepairDependencyDomain.WorkspaceContainment,
        RepairDependencyDomain.RouteAndHeading,
        RepairDependencyDomain.LocalReference,
    ]);
    private static readonly RepairVerificationRequirement Verification = new(
    [
        RepairVerificationKind.DestinationLiteral,
        RepairVerificationKind.SameTargetIdentity,
        RepairVerificationKind.ResultingBytes,
    ]);

    internal static RepairPlan Build(
        RepairRequest request,
        RepairPlanningSelection planning)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(planning);

        var conflicts = planning.Conflicts.ToList();
        var actions = planning.Selected.ToDictionary(
            entry => entry.Selected,
            entry => ResolveAction(entry, conflicts),
            ReferenceIdentityComparer<RepairSelectedProposal>.Instance);
        ResolveFileEffects(request, actions, conflicts);

        var steps = planning.Selected
            .Select((entry, index) => CreateStep(
                ordinal: index + 1,
                entry.Selected,
                actions[entry.Selected]))
            .ToArray();
        return new RepairPlan(request, planning.Selection, steps, conflicts);
    }

    private static RepairPlanningAction ResolveAction(
        RepairPlanningSelectionEntry entry,
        List<RepairConflict> conflicts)
    {
        var input = entry.Input;
        if (string.Equals(
            input.ObservedDestination,
            input.IntendedDestination,
            StringComparison.Ordinal))
        {
            return RepairPlanningAction.ForNoOp(new RepairNoOp(
                input.SourceCanonicalPath,
                input.Occurrence,
                input.IntendedDestination,
                input.CatalogueMember,
                input.Target,
                input.ObservedFileState,
                entry.Selected.Origins));
        }

        if (!string.Equals(
            input.ObservedDestination,
            input.ExpectedDestination,
            StringComparison.Ordinal)
            || !RepairEffectProjection.SpanMatches(input))
        {
            conflicts.Add(new RepairConflict(
                RepairConflictKind.ExpectedStateMismatch,
                input.SourceCanonicalPath,
                input.Occurrence,
                "The current destination or complete file state differs from the selected Repair expectation."));
            return RepairPlanningAction.Blocked();
        }

        return RepairPlanningAction.PendingFor(input);
    }

    private static void ResolveFileEffects(
        RepairRequest request,
        Dictionary<RepairSelectedProposal, RepairPlanningAction> actions,
        List<RepairConflict> conflicts)
    {
        foreach (var group in actions
            .Where(pair => pair.Value.Kind == RepairPlanningActionKind.Pending)
            .GroupBy(pair => pair.Key.Proposal.SourceCanonicalPath))
        {
            var selected = group.Select(pair => pair.Key).ToArray();
            var inputs = selected
                .Select(value => actions[value].Input
                    ?? throw new InvalidOperationException("A pending Repair action requires its planning input."))
                .ToArray();
            if (!RepairEffectProjection.HasCommonExpectedState(inputs)
                || RepairEffectProjection.HasOverlap(inputs))
            {
                var kind = RepairEffectProjection.HasOverlap(inputs)
                    ? RepairConflictKind.OverlappingChanges
                    : RepairConflictKind.ExpectedStateMismatch;
                conflicts.Add(new RepairConflict(
                    kind,
                    group.Key,
                    inputs[0].Occurrence,
                    kind == RepairConflictKind.OverlappingChanges
                        ? "Selected destination spans overlap."
                        : "Selected changes do not share one common expected complete-file state."));
                foreach (var value in selected)
                {
                    actions[value].Block();
                }

                continue;
            }

            try
            {
                var attribution = RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Repair,
                    RecoveryBundleOperation.Repair,
                    request.Workspace);
                var changes = selected.Select(value => CreateChange(
                    actions[value].Input
                        ?? throw new InvalidOperationException("A pending Repair action requires its planning input."),
                    value)).ToArray();
                var expected = inputs[0].ObservedFileState;
                var intended = RepairEffectProjection.Project(expected, changes);
                var effect = new RepairEffect(
                    group.Key,
                    expected,
                    intended,
                    changes,
                    attribution);
                foreach (var value in selected)
                {
                    actions[value].Resolve(effect);
                }
            }
            catch (Exception exception) when (exception is ArgumentException or OverflowException)
            {
                conflicts.Add(new RepairConflict(
                    RepairConflictKind.UnsafeBoundary,
                    group.Key,
                    inputs[0].Occurrence,
                    $"The selected Repair changes cannot form one safe exact file effect: {exception.Message}"));
                foreach (var value in selected)
                {
                    actions[value].Block();
                }
            }
        }
    }

    private static RepairStep CreateStep(
        int ordinal,
        RepairSelectedProposal selected,
        RepairPlanningAction action)
        => action.Kind switch
        {
            RepairPlanningActionKind.Effect => new RepairStep(
                ordinal,
                selected,
                Dependency,
                Verification,
                RepairRecoveryRequirement.Required((action.Effect
                    ?? throw new InvalidOperationException("An effect action requires its exact file effect.")).RecoveryAttribution),
                action.Effect,
                noOp: null,
                RepairStepOutcome.Planned),
            RepairPlanningActionKind.NoOp => new RepairStep(
                ordinal,
                selected,
                Dependency,
                Verification,
                RepairRecoveryRequirement.NotRequired,
                effect: null,
                action.NoOp,
                RepairStepOutcome.NoOp),
            RepairPlanningActionKind.Pending or RepairPlanningActionKind.Blocked => new RepairStep(
                ordinal,
                selected,
                Dependency,
                Verification,
                RepairRecoveryRequirement.NotRequired,
                effect: null,
                noOp: null,
                RepairStepOutcome.Blocked),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action.Kind, "The Repair action kind is not defined."),
        };

    private static RepairChange CreateChange(
        RepairableReferenceInput input,
        RepairSelectedProposal selected)
        => new(
            input.Occurrence,
            input.ExpectedDestination,
            input.IntendedDestination,
            input.CatalogueMember,
            input.Target,
            selected.Origins);
}
