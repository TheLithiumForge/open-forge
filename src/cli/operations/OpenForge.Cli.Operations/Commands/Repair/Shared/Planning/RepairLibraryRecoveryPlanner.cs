using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairLibraryRecoveryPlanner
{
    internal static RepairPlan Build(RepairLibraryPlanningInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Request);
        if (input.References.IsDefault || input.Libraries.IsDefault || input.PromptRelinks.IsDefault
            || input.PromptLibraries is { IsDefault: true })
        {
            throw new ArgumentException("Atomic Repair planning requires complete typed reference and Library proposal inputs.", nameof(input));
        }

        var referencePlan = input.PromptLibraries is null
            ? RepairPlanner.Build(input.Request, input.References)
            : RepairPlanner.Build(input.Request, input.References, input.PromptRelinks);
        var conflicts = referencePlan.Conflicts.ToList();
        var selected = SelectLibraries(input, conflicts);
        if (selected.Length > 1)
        {
            foreach (var value in selected)
            {
                conflicts.Add(new RepairConflict(
                    RepairConflictKind.OverlappingChanges,
                    value.Proposal.Evidence,
                    "One Repair invocation can select only one exact Library residual entry."));
            }
        }

        var selectedValues = selected.Length > 1 ? [] : selected;
        var selectedProposals = selectedValues.Select(value => value.Proposal).ToHashSet();
        var unselected = input.Libraries.Where(value => !selectedProposals.Contains(value)).ToImmutableArray();
        var librarySelection = new RepairLibrarySelection(selectedValues, unselected);
        var selection = new RepairSelection(
            referencePlan.Selection.Mode,
            referencePlan.Selection.Selected,
            referencePlan.Selection.Unselected,
            librarySelection);
        var steps = ImmutableArray.CreateBuilder<RepairLibraryRecoveryStep>();
        foreach (var value in selectedValues)
        {
            var evidence = value.Proposal.Evidence;
            var safe = evidence.Entry.State is RecoveryBundleTargetComparisonState.Intended
                or RecoveryBundleTargetComparisonState.Prior;
            if (!safe)
            {
                conflicts.Add(new RepairConflict(
                    evidence.Entry.State is RecoveryBundleTargetComparisonState.Unavailable
                        ? RepairConflictKind.IncompleteFacts
                        : RepairConflictKind.UnsafeBoundary,
                    evidence,
                    "The selected Library residual entry is not in an exact recoverable current state."));
            }

            var overlapsReference = referencePlan.Effects.Any(effect =>
                PhysicalIdentityTracker.PathComparer.Equals(
                    effect.ExpectedState.LogicalPath,
                    evidence.Entry.Input.Context.LogicalPath));
            if (overlapsReference)
            {
                conflicts.Add(new RepairConflict(
                    RepairConflictKind.OverlappingChanges,
                    evidence,
                    "A selected reference effect and Library recovery entry address the same logical target."));
            }

            var effect = safe && !overlapsReference
                && evidence.Entry.State == RecoveryBundleTargetComparisonState.Intended
                    ? new RepairLibraryRecoveryEffect(value)
                    : null;
            steps.Add(new RepairLibraryRecoveryStep
            {
                Ordinal = referencePlan.Steps.Count + steps.Count + 1,
                Selection = value,
                Dependency = new RepairDependency(
                [
                    RepairDependencyDomain.WorkspaceContainment,
                    RepairDependencyDomain.LibraryRegistration,
                    RepairDependencyDomain.LibraryResidual,
                ]),
                Verification = new RepairVerificationRequirement(
                [
                    RepairVerificationKind.NoFollowIdentity,
                    RepairVerificationKind.PriorState,
                ]),
                Effect = effect,
                Outcome = !safe || overlapsReference
                    ? RepairStepOutcome.Blocked
                    : effect is null ? RepairStepOutcome.NoOp : RepairStepOutcome.Planned,
            });
        }

        return new RepairPlan(
            input.Request,
            selection,
            referencePlan.Steps,
            conflicts,
            steps.ToImmutable());
    }

    internal static bool Revalidate(RepairPlan expected, RepairLibraryPlanningInput observed)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(observed);
        if (expected.Request.Workspace != observed.Request.Workspace || observed.Libraries.IsDefault || observed.References.IsDefault)
        {
            throw new ArgumentException("Atomic Repair revalidation requires independent complete facts for the same workspace.", nameof(observed));
        }

        var current = Build(observed);
        var libraryOnly = expected.Effects.Count == 0 && !expected.LibrarySteps.IsEmpty;
        return !current.IsBlocked
            && !expected.IsBlocked
            && (libraryOnly ? SameSelectedReferences(expected, current) : SameReferences(expected, current))
            && SameLibraries(expected, current);
    }

    private static bool SameSelectedReferences(RepairPlan expected, RepairPlan observed)
        => expected.Selection.Selected.Count == observed.Selection.Selected.Count
            && expected.Effects.Count == observed.Effects.Count
            && expected.NoOps.Count == observed.NoOps.Count
            && expected.Effects.Zip(observed.Effects).All(pair =>
                pair.First.ExpectedState.Expectation == pair.Second.ExpectedState.Expectation
                && pair.First.IntendedState.Expectation == pair.Second.IntendedState.Expectation)
            && expected.Selection.Selected.Zip(observed.Selection.Selected).All(pair =>
                pair.First.Proposal.SourceCanonicalPath == pair.Second.Proposal.SourceCanonicalPath
                && pair.First.Proposal.Occurrence == pair.Second.Proposal.Occurrence
                && pair.First.Proposal.ExpectedDestination == pair.Second.Proposal.ExpectedDestination
                && pair.First.Resolution.IntendedDestination == pair.Second.Resolution.IntendedDestination
                && pair.First.Resolution.Target.CanonicalTargetPath == pair.Second.Resolution.Target.CanonicalTargetPath
                && pair.First.Resolution.Target.TargetFragment == pair.Second.Resolution.Target.TargetFragment);

    private static ImmutableArray<RepairSelectedLibraryRecovery> SelectLibraries(
        RepairLibraryPlanningInput input,
        List<RepairConflict> conflicts)
    {
        var selected = ImmutableArray.CreateBuilder<RepairSelectedLibraryRecovery>();
        foreach (var proposal in input.Libraries)
        {
            if (proposal is null)
            {
                throw new ArgumentException("Library proposals cannot contain null members.", nameof(input));
            }

            var origins = ImmutableArray.CreateBuilder<RepairSelectionOrigin>();
            if (input.Request.Automatic)
            {
                origins.Add(RepairSelectionOrigin.Automatic);
            }

            if (input.PromptLibraries is { } prompt
                && prompt.Any(value => SameEvidence(value.Evidence, proposal.Evidence)))
            {
                origins.Add(RepairSelectionOrigin.Prompt);
            }

            if (origins.Count != 0)
            {
                selected.Add(new RepairSelectedLibraryRecovery(proposal, origins.ToImmutable()));
            }
        }

        if (input.PromptLibraries is { } requested)
        {
            foreach (var proposal in requested)
            {
                if (proposal is null || !input.Libraries.Any(value => SameEvidence(value.Evidence, proposal.Evidence)))
                {
                    conflicts.Add(new RepairConflict(
                        RepairConflictKind.SelectionUnmatched,
                        proposal?.Evidence ?? input.Libraries[0].Evidence,
                        "The selected Library residual no longer matches a current safe-exact proposal."));
                }
            }
        }

        return selected.ToImmutable();
    }

    private static bool SameReferences(RepairPlan expected, RepairPlan observed)
        => expected.Selection.Selected.Count == observed.Selection.Selected.Count
            && expected.Selection.Unselected.Count == observed.Selection.Unselected.Count
            && expected.Effects.Count == observed.Effects.Count
            && expected.NoOps.Count == observed.NoOps.Count
            && expected.Effects.Zip(observed.Effects).All(pair =>
                pair.First.ExpectedState.Expectation == pair.Second.ExpectedState.Expectation
                && pair.First.IntendedState.Expectation == pair.Second.IntendedState.Expectation)
            && expected.Selection.Selected.Zip(observed.Selection.Selected).All(pair =>
                pair.First.Proposal.SourceCanonicalPath == pair.Second.Proposal.SourceCanonicalPath
                && pair.First.Proposal.Occurrence == pair.Second.Proposal.Occurrence
                && pair.First.Proposal.ExpectedDestination == pair.Second.Proposal.ExpectedDestination
                && pair.First.Resolution.IntendedDestination == pair.Second.Resolution.IntendedDestination
                && pair.First.Resolution.Target.CanonicalTargetPath == pair.Second.Resolution.Target.CanonicalTargetPath
                && pair.First.Resolution.Target.TargetFragment == pair.Second.Resolution.Target.TargetFragment);

    private static bool SameLibraries(RepairPlan expected, RepairPlan observed)
        => expected.Selection.Libraries.Selected.Length == observed.Selection.Libraries.Selected.Length
            && expected.Selection.Libraries.Unselected.Length == observed.Selection.Libraries.Unselected.Length
            && expected.LibrarySteps.Length == observed.LibrarySteps.Length
            && expected.Selection.Libraries.Selected.Zip(observed.Selection.Libraries.Selected).All(pair =>
                pair.First.Origins.SequenceEqual(pair.Second.Origins)
                && SameEvidence(pair.First.Proposal.Evidence, pair.Second.Proposal.Evidence))
            && expected.Selection.Libraries.Unselected.Zip(observed.Selection.Libraries.Unselected).All(pair =>
                SameEvidence(pair.First.Evidence, pair.Second.Evidence))
            && expected.LibrarySteps.Zip(observed.LibrarySteps).All(pair =>
                pair.First.Outcome == pair.Second.Outcome
                && (pair.First.Effect is null) == (pair.Second.Effect is null));

    private static bool SameEvidence(
        Framework.Libraries.Operational.Models.LibraryResidualEvidence first,
        Framework.Libraries.Operational.Models.LibraryResidualEvidence second)
    {
        var firstVerified = first.Residual.Candidate.Verified;
        var secondVerified = second.Residual.Candidate.Verified;
        return first.LibraryId == second.LibraryId
            && first.CurrentRecord.State == second.CurrentRecord.State
            && first.CurrentRecord.Snapshot?.Expectation == second.CurrentRecord.Snapshot?.Expectation
            && first.VerifiedPriorRecord?.Entry == second.VerifiedPriorRecord?.Entry
            && first.VerifiedPriorRecord?.PayloadIdentity == second.VerifiedPriorRecord?.PayloadIdentity
            && string.Equals(first.Residual.Candidate.Path, second.Residual.Candidate.Path, StringComparison.Ordinal)
            && firstVerified is not null
            && secondVerified is not null
            && firstVerified.OperationId == secondVerified.OperationId
            && firstVerified.Attribution == secondVerified.Attribution
            && firstVerified.Entries.SequenceEqual(secondVerified.Entries)
            && first.Entry.Input.Context.Entry == second.Entry.Input.Context.Entry
            && first.Entry.State == second.Entry.State
            && first.Entry.Observed == second.Entry.Observed;
    }

}
