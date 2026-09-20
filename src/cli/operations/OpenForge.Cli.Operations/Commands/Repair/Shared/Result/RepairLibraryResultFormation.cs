using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairLibraryResultFormation
{
    internal static RepairResult Build(RepairResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var referencePlan = RepairStepOutcomeReader.Project(input);
        var plan = ProjectLibrarySteps(referencePlan, input.LibraryExecution);
        input = input with { Plan = plan };
        var findings = AddSelectionFindings(plan, input.Findings);
        var counts = ReadCounts(input, findings);
        return new RepairResult(new RepairResultFormation
        {
            Workspace = input.Request.Workspace,
            Mode = input.Request.Mode,
            Automatic = input.Request.Automatic,
            Relinks = input.Request.Relinks,
            SelectionMode = input.Request.SelectionMode,
            Facts = new RepairResultFacts
            {
                LibraryExecution = input.LibraryExecution,
                Diagnosis = input.Diagnosis,
                Selection = plan.Selection,
                Plan = plan,
                Findings = findings,
                AffectedPaths =
                [
                    .. plan.Effects.Select(effect => effect.SourceCanonicalPath),
                    .. plan.NoOps.Select(noOp => noOp.SourceCanonicalPath),
                    .. plan.LibrarySteps.Select(step => step.Selection.Proposal.Evidence.Entry.Input.Context.Entry.TargetPath),
                ],
                Counts = counts,
                Preflight = input.Preflight,
                Application = input.Application,
                Verification = input.Verification,
                Recovery = input.Recovery,
                PostDiagnosis = input.PostDiagnosis,
            },
        });
    }

    private static RepairPlan ProjectLibrarySteps(
        RepairPlan plan,
        RepairLibraryExecution? execution)
    {
        var effects = execution is null ? [] : RepairAtomicEffectOrder.Read(plan);
        return new RepairPlan(plan.Request, plan.Selection, plan.Steps, plan.Conflicts,
            [.. plan.LibrarySteps.Select(step => step with
            {
                Outcome = ReadOutcome(step, execution, effects),
            })]);
    }

    private static RepairStepOutcome ReadOutcome(
        RepairLibraryRecoveryStep step,
        RepairLibraryExecution? execution,
        ImmutableArray<RepairAtomicEffect> effects)
    {
        if (step.Effect is null || execution is null)
        {
            return step.Outcome;
        }

        var receipt = execution.LibraryReceipts.SingleOrDefault(value =>
            ReferenceEquals(value.Effect, step.Effect));
        if (receipt is not null)
        {
            return IsVerified(receipt)
                ? RepairStepOutcome.Verified
                : IsApplied(receipt)
                    ? RepairStepOutcome.Applied
                    : IsCancelled(receipt)
                        ? RepairStepOutcome.Interrupted
                        : RepairStepOutcome.Failed;
        }

        if (MatchesEffect(execution.UnexpectedFailure?.EffectOrdinal, effects, step.Effect))
        {
            return RepairStepOutcome.Failed;
        }

        return MatchesEffect(execution.Cancellation?.EffectOrdinal, effects, step.Effect)
            ? RepairStepOutcome.Interrupted
            : step.Outcome;
    }

    private static bool MatchesEffect(
        int? ordinal,
        ImmutableArray<RepairAtomicEffect> effects,
        RepairLibraryRecoveryEffect selected)
        => ordinal is { } index
            && (uint)index < (uint)effects.Length
            && ReferenceEquals(effects[index].LibraryRecovery, selected);

    private static List<RepairFinding> AddSelectionFindings(
        RepairPlan plan,
        IReadOnlyList<RepairFinding> findings)
    {
        var values = findings.ToList();
        values.AddRange(plan.Selection.Unselected.Select(proposal => new RepairFinding(
            proposal.IsGuided ? RepairFindingCode.GuidedFindingRemaining : RepairFindingCode.ManualFindingRemaining,
            "A current Repair proposal remains unselected.",
            proposal.SourceCanonicalPath,
            proposal.Occurrence)));
        values.AddRange(plan.Selection.Libraries.Unselected.Select(proposal => new RepairFinding(
            RepairFindingCode.ManualFindingRemaining,
            "A current typed Library residual remains unselected.")));
        return values;
    }

    private static RepairCounts ReadCounts(
        RepairResultInput input,
        IReadOnlyList<RepairFinding> findings)
    {
        var plan = input.Plan;
        var remaining = input.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? findings.Where(IsRemaining).ToArray()
            : [.. input.PostDiagnosis.Findings.Where(IsRemaining)];
        var referenceVerified = input.Verification.Effects.Count(effect =>
            effect.Receipt?.EffectState == FilesystemEffectState.Applied
            && effect.ResultingBytes == RepairVerificationState.Verified
            && effect.Targets == RepairVerificationState.Verified);
        var libraryVerified = input.LibraryExecution?.LibraryReceipts.Count(IsVerified) ?? 0;
        var verifiedNoOps = input.Verification.Targets == RepairVerificationState.Verified
            ? plan.NoOps.Count
            : 0;
        return new RepairCounts(
            selectedFindings: plan.Selection.Selected.Count + plan.Selection.Libraries.Selected.Length,
            unselectedFindings: plan.Selection.Unselected.Count + plan.Selection.Libraries.Unselected.Length,
            repaired: plan.Steps.Count(step => step.Outcome == RepairStepOutcome.Verified)
                + plan.LibrarySteps.Count(step => step.Outcome == RepairStepOutcome.Verified)
                + verifiedNoOps,
            remaining: remaining.Length,
            newFindings: remaining.Count(finding => !input.InitialFindings.Any(before => SameCurrentFinding(input, before, finding))),
            manual: remaining.Count(finding => finding.Code == RepairFindingCode.ManualFindingRemaining),
            guided: remaining.Count(finding => finding.Code == RepairFindingCode.GuidedFindingRemaining),
            blocked: findings.Count(finding => finding.Status == Shell.Definitions.CliSemanticStatus.Blocked),
            selectedEffects: plan.Effects.Count + plan.LibrarySteps.Count(step => step.Effect is not null),
            appliedEffects: input.Application.AppliedEffects,
            verifiedEffects: referenceVerified + libraryVerified,
            noOps: plan.NoOps.Count + plan.LibrarySteps.Count(step => step.Outcome == RepairStepOutcome.NoOp),
            conflicts: plan.Conflicts.Count);
    }

    private static bool SameCurrentFinding(
        RepairResultInput input,
        RepairFinding before,
        RepairFinding current)
    {
        if (before.Code != current.Code || before.SourceCanonicalPath != current.SourceCanonicalPath)
        {
            return false;
        }

        if (before.Occurrence is null || current.Occurrence is null)
        {
            return before.Occurrence == current.Occurrence;
        }

        var shift = input.Plan.Effects.Take(input.Application.AppliedEffects)
            .Where(effect => effect.SourceCanonicalPath == before.SourceCanonicalPath)
            .SelectMany(effect => effect.Changes)
            .Where(change => change.Occurrence.ByteOffset < before.Occurrence.ByteOffset)
            .Sum(change => Encoding.UTF8.GetByteCount(change.IntendedDestination) - change.Occurrence.ByteLength);
        return before.Occurrence.ByteOffset + shift == current.Occurrence.ByteOffset;
    }

    private static bool IsRemaining(RepairFinding finding)
        => finding.Code is RepairFindingCode.GuidedFindingRemaining or RepairFindingCode.ManualFindingRemaining;

    private static bool IsApplied(RepairLibraryRecoveryReceipt receipt)
        => receipt.Kind switch
        {
            RepairLibraryRecoveryKind.Ordinary => receipt.Ordinary?.Effect == FilesystemEffectState.Applied,
            RepairLibraryRecoveryKind.RelativeFileLink =>
                receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Restored,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.Kind, "The Library recovery kind is not defined."),
        };

    private static bool IsVerified(RepairLibraryRecoveryReceipt receipt)
        => receipt.Kind switch
        {
            RepairLibraryRecoveryKind.Ordinary => receipt.Ordinary is
            {
                Effect: FilesystemEffectState.Applied,
                Verification: FilesystemVerificationState.Verified,
            },
            RepairLibraryRecoveryKind.RelativeFileLink =>
                receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Restored,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.Kind, "The Library recovery kind is not defined."),
        };

    private static bool IsCancelled(RepairLibraryRecoveryReceipt receipt)
        => receipt.RelativeFileLink?.State == RelativeFileLinkRecoveryState.Cancelled;
}
