using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairResultBuilder
{
    internal static RepairResult Build(RepairResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Plan.LibrarySteps.IsEmpty || input.LibraryExecution is not null)
        {
            return RepairLibraryResultFormation.Build(input);
        }

        input = input with { Plan = RepairStepOutcomeReader.Project(input) };
        var findings = AddSelectionFindings(input.Plan.Selection, input.Findings);
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
                Diagnosis = input.Diagnosis,
                Selection = input.Plan.Selection,
                Plan = input.Plan,
                Findings = findings,
                AffectedPaths =
                [
                    .. input.Plan.Effects.Select(effect => effect.SourceCanonicalPath),
                    .. input.Plan.NoOps.Select(noOp => noOp.SourceCanonicalPath),
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

    internal static RepairResult Boundary(
        RepairRequest request, RepairFinding finding, RepairPostDiagnosis diagnosis, IReadOnlyList<RepairFinding> initialFindings)
        => new(new RepairResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Automatic = request.Automatic,
            Relinks = request.Relinks,
            SelectionMode = request.SelectionMode,
            Facts = new RepairResultFacts
            {
                Diagnosis = diagnosis.Coverage,
                Selection = null,
                Plan = null,
                Findings = [finding],
                AffectedPaths = [],
                Counts = new RepairCounts(
                    selectedFindings: 0,
                    unselectedFindings: initialFindings.Count,
                    repaired: 0,
                    remaining: diagnosis.Findings.Count(IsRemaining),
                    newFindings: diagnosis.Findings.Count(current => IsRemaining(current)
                        && !initialFindings.Any(before => SameFinding(before, current))),
                    manual: diagnosis.Findings.Count(current => current.Code == RepairFindingCode.ManualFindingRemaining),
                    guided: diagnosis.Findings.Count(current => current.Code == RepairFindingCode.GuidedFindingRemaining),
                    blocked: diagnosis.Findings.Count(current => current.Status == Shell.Definitions.CliSemanticStatus.Blocked),
                    selectedEffects: 0, appliedEffects: 0, verifiedEffects: 0, noOps: 0, conflicts: 0),
                Preflight = RepairPreflight.NotRequested,
                Application = RepairApplication.NotRequested,
                Verification = RepairVerification.NotRequested,
                Recovery = RepairRecovery.NotRequired,
                PostDiagnosis = diagnosis,
            },
        });

    internal static RepairResult SelectionRequired(RepairRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return RepairResult.Empty(
            request.Workspace,
            request.Mode,
            request.Automatic,
            request.SelectionMode,
            new RepairFinding(
                RepairFindingCode.SelectionRequired,
                "Repair requires --automatic, an exact --relink, or an interactive wizard."));
    }

    internal static RepairFinding PlanConflict(string cause)
        => new(RepairFindingCode.PlanConflict, cause);

    private static List<RepairFinding> AddSelectionFindings(
        RepairSelection selection,
        IReadOnlyList<RepairFinding> findings)
    {
        var values = findings.ToList();
        values.AddRange(selection.Unselected
            .Select(proposal => new RepairFinding(
                proposal.IsGuided ? RepairFindingCode.GuidedFindingRemaining : RepairFindingCode.ManualFindingRemaining,
                "A current Repair proposal remains unselected.",
                proposal.SourceCanonicalPath,
                proposal.Occurrence)));
        return values;
    }

    private static bool SameFinding(RepairFinding left, RepairFinding right)
        => left.Code == right.Code && left.SourceCanonicalPath == right.SourceCanonicalPath && left.Occurrence == right.Occurrence;

    private static bool SameCurrentFinding(RepairResultInput input, RepairFinding before, RepairFinding current)
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

    private static RepairCounts ReadCounts(
        RepairResultInput input,
        IReadOnlyList<RepairFinding> findings)
    {
        var selection = input.Plan.Selection;
        var remaining = input.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? findings.Where(IsRemaining).ToArray()
            : [.. input.PostDiagnosis.Findings.Where(IsRemaining)];
        var guided = remaining.Count(finding => finding.Code == RepairFindingCode.GuidedFindingRemaining);
        var blocked = findings.Count(finding => finding.Status == Shell.Definitions.CliSemanticStatus.Blocked);
        var verified = input.Verification.Effects.Count(effect =>
            effect.Receipt?.EffectState == FilesystemEffectState.Applied
            && effect.ResultingBytes == RepairVerificationState.Verified
            && effect.Targets == RepairVerificationState.Verified);
        return new RepairCounts(
            selectedFindings: selection.Selected.Count,
            unselectedFindings: selection.Unselected.Count,
            repaired: input.Plan.Steps.Count(step => step.Outcome == RepairStepOutcome.Verified)
                + (input.Verification.Targets == RepairVerificationState.Verified ? input.Plan.NoOps.Count : 0),
            remaining: remaining.Length,
            newFindings: remaining.Count(finding => !input.InitialFindings.Any(before => SameCurrentFinding(input, before, finding))),
            manual: remaining.Count(finding => finding.Code == RepairFindingCode.ManualFindingRemaining),
            guided: guided,
            blocked: blocked,
            selectedEffects: input.Plan.Effects.Count,
            appliedEffects: input.Application.AppliedEffects,
            verifiedEffects: verified,
            noOps: input.Plan.NoOps.Count,
            conflicts: input.Plan.Conflicts.Count);
    }
}
