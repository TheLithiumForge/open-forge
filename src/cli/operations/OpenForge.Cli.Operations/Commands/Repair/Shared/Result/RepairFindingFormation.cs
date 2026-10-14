using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairFindingFormation
{
    internal static IReadOnlyList<RepairFinding> Report(RepairResult result)
    {
        var findings = result.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? result.Findings
            : result.Findings.Where(finding => finding.Code is not (RepairFindingCode.GuidedFindingRemaining or RepairFindingCode.ManualFindingRemaining)
                || finding.IsUnselectedLibraryResidual)
                .Concat(result.PostDiagnosis.Findings.Where(finding => finding.Code is RepairFindingCode.GuidedFindingRemaining or RepairFindingCode.ManualFindingRemaining));
        return LibraryObservations(findings.Where(finding => !finding.LibraryRecoveryPlanned));
    }

    internal static IReadOnlyList<RepairFinding> Remaining(RepairResult result)
    {
        var findings = result.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? result.Findings
            : result.PostDiagnosis.Findings.Concat(result.Findings.Where(finding => finding.IsUnselectedLibraryResidual));
        return LibraryObservations(findings).Where(finding => !finding.LibraryRecoveryPlanned
            && finding.Code is RepairFindingCode.GuidedFindingRemaining or RepairFindingCode.ManualFindingRemaining).ToArray();
    }

    internal static IReadOnlyList<RepairFinding> Read(RepairResultInput input)
    {
        var values = input.Findings.ToList();
        values.AddRange((input.Plan.Maintenance?.Intent.Limitations ?? []).Where(item => item.Malformed).Select(item =>
            new RepairFinding(RepairFindingCode.MetadataSkipped, item.Cause, item.Path)));
        values.AddRange(input.Plan.Steps.Where(step => input.Request.Mode == RepairMode.DryRun || step.Outcome == RepairStepOutcome.Verified)
            .SelectMany(step => new[] { step.Proposal.RenameProof, step.Proposal.SourceMoveProof })
            .OfType<OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations.RenameEvidenceObservation>()
            .DistinctBy(proof => (proof.OldPath, proof.NewPath)).Select(proof => new RepairFinding(RepairFindingCode.RenameProven,
                "A unique rename or move was proven from current identity evidence.", proof.OldPath)));
        if (input.PostDiagnosis.State != RepairPostDiagnosisState.NotRequested)
        {
            values.RemoveAll(finding => finding.IsLibraryObservation);
            values.AddRange(input.PostDiagnosis.Findings.Where(finding => finding.IsLibraryObservation));
        }
        values.AddRange(input.Plan.Selection.Unselected.Select(proposal => new RepairFinding(
            proposal.IsGuided ? RepairFindingCode.GuidedFindingRemaining : RepairFindingCode.ManualFindingRemaining,
            "A current Repair proposal remains unselected.",
            proposal.SourceCanonicalPath,
            proposal.Occurrence)));

        foreach (var proposal in input.Plan.Selection.Libraries.Unselected)
        {
            if (input.Plan.LibrarySteps.IsEmpty && input.LibraryExecution is null)
            {
                break;
            }
            var observation = new RepairDiagnosisObservation(proposal.TargetPath, proposal.LibraryIdValue);
            if (!values.Any(finding => finding.IsLibraryObservation && finding.Observation == observation))
            {
                values.Add(new RepairFinding(RepairFindingCode.ManualFindingRemaining,
                    "A current typed Library residual remains unselected.")
                {
                    Observation = observation,
                    IsUnselectedLibraryResidual = true,
                });
            }
        }

        return LibraryObservations(values).Select(finding => finding with
        {
            LibraryRecoveryPlanned = finding.IsLibraryObservation && input.Request.Mode == RepairMode.DryRun
                && input.Plan.LibrarySteps.Any(step => step.Effect is not null
                    && step.Outcome == RepairStepOutcome.Planned
                    && step.Selection.Proposal.IsRelativeFileLink
                    && Matches(finding, step.Selection.Proposal)),
        }).ToArray();
    }

    internal static IReadOnlyList<RepairFinding> LibraryObservations(IEnumerable<RepairFinding> values)
    {
        var snapshot = values.ToArray();
        var findings = snapshot.Where(finding => !finding.IsLibraryObservation).ToList();
        foreach (var group in snapshot.Where(finding => finding.IsLibraryObservation).GroupBy(finding => finding.Observation))
        {
            var first = group.First();
            findings.Add(new RepairFinding(first.Code,
                string.Join(" ", group.Select(finding => finding.Cause).Distinct(StringComparer.Ordinal)),
                first.SourceCanonicalPath, first.Occurrence, first.Target)
            {
                Observation = first.Observation,
                LibraryRecoveryPlanned = first.LibraryRecoveryPlanned,
                IsUnselectedLibraryResidual = first.IsUnselectedLibraryResidual,
            });
        }
        return findings;
    }

    internal static RepairFinding[] Remaining(RepairResultInput input, IReadOnlyList<RepairFinding> findings)
    {
        IEnumerable<RepairFinding> current = findings;
        if (input.PostDiagnosis.State != RepairPostDiagnosisState.NotRequested)
        {
            current = input.PostDiagnosis.Findings.Where(finding => !finding.IsLibraryObservation)
                .Concat(findings.Where(finding => finding.IsLibraryObservation));
        }
        return current.Where(finding => !finding.LibraryRecoveryPlanned
            && finding.Code is RepairFindingCode.GuidedFindingRemaining or RepairFindingCode.ManualFindingRemaining).ToArray();
    }

    private static bool Matches(RepairFinding finding, RepairLibraryRecoveryProposal proposal)
        => finding.Observation is { } observation
            && observation.Path == proposal.TargetPath && observation.Identifier == proposal.LibraryIdValue;
}
