using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Selection;

internal static class RepairReportSelector
{
    internal static CliReport<RepairData> Select(RepairResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var repairs = ReadRepairs(result, selection.Detail >= CliDetail.Full);
        var remaining = ReadRemaining(result, selection.Detail);
        var libraryRecovery = ReadLibraryRecovery(result);
        var libraryEffects = ReadLibraryEffects(result, libraryRecovery);
        var hasWork = repairs.Count > 0 || libraryEffects.Count > 0;
        var plannedLinks = repairs.Count + libraryEffects.Count;
        var data = new RepairData
        {
            Mode = CliReportVocabulary.Name(result.Mode),
            Selection = Selection(result),
            Repairs = repairs,
            Remaining = remaining,
            LibraryRecovery = selection.Detail >= CliDetail.Standard && libraryRecovery.Count > 0
                ? libraryRecovery
                : null,
            Diagnosis = selection.Detail >= CliDetail.Full ? Coverage(result.Diagnosis) : null,
            Verification = selection.Detail >= CliDetail.Full ? Verification(result, hasWork) : null,
            TextRows = ReadTextRows(result, repairs, remaining, libraryRecovery, libraryEffects, selection.Detail),
            TextDetailLines = selection.Detail >= CliDetail.Full
                ? ReadTextDetails(result, repairs, hasWork)
                : PartialTextDetails(result),
            ShowNoChanges = result.Mode == RepairMode.DryRun && plannedLinks > 0,
        };

        return new CliReport<RepairData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, plannedLinks),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = ReadFindings(result).Select(finding => Finding(result, finding)).ToArray(),
            Effects =
            [
                .. repairs.Select(Effect),
                .. libraryEffects,
            ],
            Counts = Counts(result, plannedLinks),
            Limitations = Limitations(result),
            Data = data,
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, repairs, remaining, libraryEffects, hasWork)
                : [],
        };
    }

    private static IReadOnlyList<RepairDataRepair> ReadRepairs(RepairResult result, bool includeHashes)
    {
        if (result.Plan is not { } plan)
        {
            return [];
        }

        var repairs = new List<RepairDataRepair>();
        foreach (var effect in plan.Effects)
        {
            foreach (var change in effect.Changes)
            {
                var step = plan.Steps.FirstOrDefault(candidate => SameOccurrence(
                    candidate.Proposal.SourceCanonicalPath,
                    candidate.Proposal.OccurrenceView.Line,
                    candidate.Proposal.OccurrenceView.Column,
                    effect.SourceCanonicalPath,
                    change.OccurrenceView.Line,
                    change.OccurrenceView.Column));
                repairs.Add(new RepairDataRepair
                {
                    Path = effect.SourceCanonicalPath,
                    Location = new RepairDataLocation(change.OccurrenceView.Line, change.OccurrenceView.Column),
                    From = change.ExpectedDestination,
                    To = change.IntendedDestination,
                    Before = includeHashes ? effect.BeforeHash : null,
                    After = includeHashes ? effect.AfterHash : null,
                    Outcome = step?.Outcome ?? RepairStepOutcome.Planned,
                });
            }
        }

        return repairs
            .OrderBy(repair => repair.Path, StringComparer.Ordinal)
            .ThenBy(repair => repair.Location.Line)
            .ThenBy(repair => repair.Location.Column)
            .ToArray();
    }

    private static IReadOnlyList<RepairDataRemaining> ReadRemaining(
        RepairResult result,
        CliDetail detail)
    {
        var remaining = new List<RepairDataRemaining>();
        var seen = new HashSet<(string? Path, int? Line, int? Column, string? ObservationCause)>();
        var seenLibraryObservations = new HashSet<(string Code, string? Path, string? Identifier, string Cause)>();
        var selection = result.Selection;
        if (selection is not null)
        {
            foreach (var proposal in selection.Unselected)
            {
                var candidates = proposal.Candidates?.Items ?? [];
                remaining.Add(Remaining(proposal, candidates, detail));
                seen.Add((proposal.SourceCanonicalPath, proposal.OccurrenceView.Line, proposal.OccurrenceView.Column, null));
            }
        }

        foreach (var finding in RemainingFindings(result))
        {
            if (finding.Observation is { } observation)
            {
                if (!seenLibraryObservations.Add((
                        finding.Code.ToString(),
                        observation.Path,
                        observation.Identifier,
                        finding.Cause)))
                {
                    continue;
                }
            }
            else
            {
                var key = (finding.SourceCanonicalPath ?? finding.Observation?.Path ?? finding.Observation?.Identifier,
                    finding.OccurrenceView?.Line, finding.OccurrenceView?.Column,
                    finding.Observation is null ? null : finding.Cause);
                if (!seen.Add(key))
                {
                    continue;
                }
            }

            var proposal = selection?.Unselected.FirstOrDefault(value =>
                value.SourceCanonicalPath == finding.SourceCanonicalPath
                && value.OccurrenceView == finding.OccurrenceView);
            var candidates = proposal?.Candidates?.Items ?? [];
            remaining.Add(Remaining(finding, candidates, detail));
        }

        return remaining
            .OrderBy(value => value.Path, StringComparer.Ordinal)
            .ThenBy(value => value.Location?.Line)
            .ThenBy(value => value.Location?.Column)
            .ToArray();
    }

    private static IReadOnlyList<RepairFinding> ReadFindings(RepairResult result)
    {
        var findings = result.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? result.Findings
            : result.Findings
                .Where(finding => !IsLibraryObservation(finding))
                .Concat(result.PostDiagnosis.Findings.Where(IsLibraryObservation));
        var seenLibraryObservations = new HashSet<(string Code, string? Path, string? Identifier, string Cause)>();
        var projected = new List<RepairFinding>();
        foreach (var finding in findings)
        {
            if (finding.Observation is { } observation
                && !seenLibraryObservations.Add((
                    finding.Code.ToString(),
                    observation.Path,
                    observation.Identifier,
                    finding.Cause)))
            {
                continue;
            }

            projected.Add(finding);
        }

        return projected;
    }

    private static IEnumerable<RepairFinding> RemainingFindings(RepairResult result)
    {
        var findings = result.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested
            ? result.Findings
            : result.PostDiagnosis.Findings;
        return findings.Where(finding => finding.Code.ToString() is
            "GuidedFindingRemaining" or "ManualFindingRemaining");
    }

    private static bool IsLibraryObservation(RepairFinding finding)
        => finding.Code == global::OpenForge.Cli.Core.Commands.Repair.RepairFindingCode.ManualFindingRemaining
            && finding.Observation is not null;

    private static RepairDataRemaining Remaining(
        RepairProposal proposal,
        IReadOnlyList<RepairCandidate> candidates,
        CliDetail detail)
        => new()
        {
            Path = proposal.SourceCanonicalPath,
            Location = new RepairDataLocation(proposal.OccurrenceView.Line, proposal.OccurrenceView.Column),
            Kind = proposal.IsGuided ? "guided" : "manual",
            Candidates = CandidateShape(candidates, detail),
            TextCandidates = candidates.Select(candidate => Candidate(candidate, includeReasons: detail >= CliDetail.Full)).ToArray(),
        };

    private static RepairDataRemaining Remaining(
        RepairFinding finding,
        IReadOnlyList<RepairCandidate> candidates,
        CliDetail detail)
        => new()
        {
            Path = finding.SourceCanonicalPath ?? finding.Observation?.Path ?? finding.Observation?.Identifier ?? "repair",
            Location = finding.OccurrenceView is { } occurrence
                ? new RepairDataLocation(occurrence.Line, occurrence.Column)
                : null,
            Kind = finding.Code.ToString() == "GuidedFindingRemaining" ? "guided" : "manual",
            Candidates = CandidateShape(candidates, detail),
            TextCandidates = candidates.Select(candidate => Candidate(candidate, includeReasons: detail >= CliDetail.Full)).ToArray(),
        };

    private static RepairDataCandidates CandidateShape(
        IReadOnlyList<RepairCandidate> candidates,
        CliDetail detail)
    {
        var projected = candidates
            .Select(candidate => Candidate(candidate, includeReasons: detail >= CliDetail.Full))
            .ToArray();
        return detail switch
        {
            CliDetail.Minimal => RepairDataCandidates.Count(projected.Length),
            CliDetail.Standard => RepairDataCandidates.Paths(projected),
            CliDetail.Full or CliDetail.Debug => RepairDataCandidates.Reasons(projected),
            _ => throw new ArgumentOutOfRangeException(nameof(detail), detail, "The Repair detail level is not defined."),
        };
    }

    private static RepairDataCandidate Candidate(RepairCandidate candidate, bool includeReasons)
        => new()
        {
            Path = RepairWording.Target(candidate.Target),
            Reasons = includeReasons
                ? candidate.Evidence.Select(RepairWording.CandidateReason).ToArray()
                : null,
        };

    private static IReadOnlyList<RepairDataLibraryRecovery> ReadLibraryRecovery(RepairResult result)
    {
        if (result.Selection is not { } selection
            || selection.Libraries.Selected.IsEmpty && selection.Libraries.Unselected.IsEmpty)
        {
            return [];
        }

        var rows = new List<RepairDataLibraryRecovery>();
        foreach (var selected in selection.Libraries.Selected)
        {
            rows.Add(new RepairDataLibraryRecovery
            {
                Id = selected.Proposal.LibraryIdValue,
                Path = selected.Proposal.TargetPath,
                Selected = true,
                Outcome = LibraryOutcome(result, selected.Proposal),
            });
        }

        foreach (var proposal in selection.Libraries.Unselected)
        {
            rows.Add(new RepairDataLibraryRecovery
            {
                Id = proposal.LibraryIdValue,
                Path = proposal.TargetPath,
                Selected = false,
                Outcome = RepairStepOutcome.Blocked,
            });
        }

        return rows
            .OrderBy(value => value.Id, StringComparer.Ordinal)
            .ThenBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static RepairStepOutcome LibraryOutcome(
        RepairResult result,
        RepairLibraryRecoveryProposal proposal)
        => result.Plan?.LibrarySteps.FirstOrDefault(step =>
            ReferenceEquals(step.Selection.Proposal, proposal))?.Outcome
            ?? RepairStepOutcome.Planned;

    private static IReadOnlyList<CliEffect> ReadLibraryEffects(
        RepairResult result,
        IReadOnlyList<RepairDataLibraryRecovery> libraryRecovery)
    {
        if (result.Selection is not { } selection || result.Plan is not { } plan)
        {
            return [];
        }

        var effects = new List<CliEffect>();
        foreach (var selected in selection.Libraries.Selected)
        {
            var step = plan.LibrarySteps.FirstOrDefault(candidate =>
                ReferenceEquals(candidate.Selection.Proposal, selected.Proposal));
            if (step is null || step.Effect is null)
            {
                continue;
            }

            var row = libraryRecovery.FirstOrDefault(candidate =>
                candidate.Selected
                && candidate.Id == selected.Proposal.LibraryIdValue
                && candidate.Path == selected.Proposal.TargetPath);
            effects.Add(new CliEffect
            {
                Path = row?.Path ?? selected.Proposal.TargetPath,
                Kind = CliEffectKind.Link,
                Action = CliEffectAction.Restored,
                Outcome = EffectOutcome(step.Outcome),
            });
        }

        return effects;
    }

    private static IReadOnlyList<RepairDataTextRow> ReadTextRows(
        RepairResult result,
        IReadOnlyList<RepairDataRepair> repairs,
        IReadOnlyList<RepairDataRemaining> remaining,
        IReadOnlyList<RepairDataLibraryRecovery> libraryRecovery,
        IReadOnlyList<CliEffect> libraryEffects,
        CliDetail detail)
    {
        var rows = new List<RepairDataTextRow>();
        var partial = result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted;
        foreach (var repair in repairs)
        {
            var path = RepairWording.Location(repair.Path, repair.Location.Line, repair.Location.Column);
            var useOutcomeWording = partial
                || repair.Outcome is RepairStepOutcome.Blocked
                    or RepairStepOutcome.Failed
                    or RepairStepOutcome.Interrupted;
            var wording = useOutcomeWording
                ? PartialRepairWording(repair.Outcome)
                : $"{repair.From} -> {repair.To}";
            rows.Add(new RepairDataTextRow(path, wording));
        }

        foreach (var effect in libraryEffects)
        {
            rows.Add(new RepairDataTextRow(effect.Path, LibraryEffectWording(effect.Outcome)));
        }

        foreach (var library in libraryRecovery.Where(value =>
                     value.Selected
                     && (value.Outcome is RepairStepOutcome.Blocked
                         or RepairStepOutcome.Failed
                         or RepairStepOutcome.Interrupted)
                     && !libraryEffects.Any(effect =>
                         string.Equals(effect.Path, value.Path, StringComparison.Ordinal))))
        {
            rows.Add(new RepairDataTextRow(
                library.Path,
                LibraryEffectWording(EffectOutcome(library.Outcome))));
        }

        if (detail == CliDetail.Minimal && result.Status != CliSemanticStatus.Interrupted)
        {
            foreach (var item in remaining)
            {
                if (IsAlreadyVisible(result, repairs, item))
                {
                    continue;
                }

                var location = item.Location is { } value
                    ? RepairWording.Location(item.Path, value.Line, value.Column)
                    : item.Path;
                var assessment = item.Kind == "guided"
                    ? RepairWording.GuidedFinding(item.Candidates.ItemCount)
                    : RepairWording.ManualFinding();
                var proposal = item.Location is { } occurrence
                    ? result.Selection?.Unselected.FirstOrDefault(value =>
                        string.Equals(value.SourceCanonicalPath, item.Path, StringComparison.Ordinal)
                        && value.OccurrenceView.Line == occurrence.Line
                        && value.OccurrenceView.Column == occurrence.Column)
                    : null;
                var wording = proposal is { } matched
                    ? global::OpenForge.Cli.OutputText.Repair.RepairPhrases.RemainingLink(
                        matched.ExpectedDestination,
                        assessment)
                    : assessment;
                rows.Add(new RepairDataTextRow(location, wording));
            }
        }

        if (detail >= CliDetail.Standard)
        {
            foreach (var item in remaining)
            {
                var location = item.Location is { } value
                    ? RepairWording.Location(item.Path, value.Line, value.Column)
                    : item.Path;
                foreach (var candidate in item.TextCandidates)
                {
                    var wording = detail >= CliDetail.Full && candidate.Reasons is { Count: > 0 }
                        ? global::OpenForge.Cli.OutputText.Repair.RepairPhrases.TargetCandidateWithEvidence($"{candidate.Path}", $"{string.Join("; ", candidate.Reasons)}")
                        : global::OpenForge.Cli.OutputText.Repair.RepairPhrases.TargetCandidate($"{candidate.Path}");
                    rows.Add(new RepairDataTextRow(location, wording));
                }
            }

            foreach (var library in libraryRecovery.Where(value => !value.Selected))
            {
                rows.Add(new RepairDataTextRow(library.Path, global::OpenForge.Cli.OutputText.Repair.RepairText.LabelSkipped()));
            }
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadTextDetails(
        RepairResult result,
        IReadOnlyList<RepairDataRepair> repairs,
        bool hasWork)
    {
        var lines = new List<string>
        {
            global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatDiagnosisCoverage($"{RepairWording.Coverage(result.Diagnosis)}"),
        };
        foreach (var repair in repairs)
        {
            if (repair.Before is { } before)
            {
                lines.Add(CliFindingWording.BeforeHash(before));
            }

            if (repair.After is { } after)
            {
                lines.Add(CliFindingWording.AfterHash(after));
            }
        }

        lines.Add(global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatChecksBeforeWriting($"{PreflightWording(result.Preflight.State)}"));
        lines.Add(hasWork
            ? RepairWording.Verification(result.Verification.Targets, result.Verification.ResultingBytes, result.Verification.PostConditions)
            : global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoVerificationWasNeeded());
        lines.Add(global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatRecovery($"{RepairWording.Recovery(result.Recovery.State, result.Recovery.ResidualPath)}"));
        lines.Add(global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatChecksAfterWriting($"{PostDiagnosisWording(result.PostDiagnosis.State)}"));
        return lines;
    }

    private static IReadOnlyList<string> PartialTextDetails(RepairResult result)
        => result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted
            && result.Recovery.State == RepairRecoveryState.Retained
            && result.Recovery.ResidualPath is { } path
                ? [global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatRecoveryData($"{path}")]
                : [];

    private static CliHeadline Headline(RepairResult result, int plannedLinks)
    {
        var primary = PrimaryFinding(result);
        var repaired = result.Mode == RepairMode.DryRun ? plannedLinks : result.Counts.Repaired;
        return result.Status switch
        {
            CliSemanticStatus.Complete when repaired == 0 && plannedLinks == 0
                => new(RepairWording.NothingToRepair(), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Mode == RepairMode.DryRun
                => new(RepairWording.WouldRepair(repaired), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(RepairWording.Repaired(repaired), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when repaired == 0
                => new(RepairWording.NothingAutomatic(result.Counts.Remaining, result.Counts.Manual > 0), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when result.Mode == RepairMode.DryRun
                => new(RepairWording.WouldRepairWithRemaining(repaired, result.Counts.Remaining, result.Counts.Manual > 0), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(RepairWording.RepairedWithRemaining(repaired, result.Counts.Remaining, result.Counts.Manual > 0), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RepairWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(RepairWording.Invalid(FindingMessage(result, primary)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RepairWording.Cannot(BlockedReason(result, primary)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RepairWording.Failed(repaired, Math.Max(plannedLinks, repaired)), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RepairWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Repair status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(RepairResult result)
    {
        var finding = PrimaryFinding(result);
        return finding is null || result.Status == CliSemanticStatus.Complete
            || finding.Code is global::OpenForge.Cli.Core.Commands.Repair.RepairFindingCode.GuidedFindingRemaining
                or global::OpenForge.Cli.Core.Commands.Repair.RepairFindingCode.ManualFindingRemaining
            ? null
            : MachineCode(finding.Code);
    }

    private static RepairFinding? PrimaryFinding(RepairResult result)
        => result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();

    private static string BlockedReason(RepairResult result, RepairFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Repair.RepairText.LabelTheRepairRequestIsBlocked();
        }

        if (finding.Code.ToString() == "SelectionRequired")
        {
            return global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRepairNeedsToKnowWhichRepairsToApplyAndThisSessionCannotAsk();
        }

        if (finding.Code.ToString() == "WorkspaceLockUnavailable")
        {
            return global::OpenForge.Cli.OutputText.Shared.SharedText.LabelAnotherOpenForgeCommandHoldsTheWorkspaceLock();
        }

        return FindingMessage(result, finding).Trim().TrimEnd('.');
    }

    private static CliFinding Finding(RepairResult result, RepairFinding finding)
    {
        var proposal = Proposal(result, finding);
        var path = finding.SourceCanonicalPath ?? finding.Observation?.Path ?? proposal?.SourceCanonicalPath ?? FailurePath(result, finding.Code.ToString());
        var occurrence = finding.OccurrenceView ?? proposal?.OccurrenceView;
        var subject = new CliSubject(
            path is null ? CliSubjectKind.Identifier : CliSubjectKind.File,
            path,
            finding.Observation?.Identifier ?? (path is null ? MachineCode(finding.Code) : null),
            occurrence is { } location ? new CliSourceLocation(location.Line, location.Column) : null);
        var candidates = proposal?.Candidates?.Items ?? [];
        var resolution = finding.Code.ToString() switch
        {
            "GuidedFindingRemaining" => CliResolution.GuidedChoice,
            "ManualFindingRemaining" => CliResolution.ManualDecision,
            "ProposalUnavailable"
                or "FactsConflicting"
                or "ProposalUnsupported"
                or "MissingAuthority"
                or "TargetChanged"
                or "TargetUnsafe"
                or "PlanConflict"
                or "WorkspaceLockUnavailable"
                or "RecoveryConflict" => CliResolution.BlockedRepair,
            _ => (CliResolution?)null,
        };
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = MachineCode(finding.Code),
            Title = finding.Observation is null
                ? RepairWording.FindingTitle(finding.Code.ToString())
                : global::OpenForge.Cli.OutputText.Repair.RepairWording.RemainingLibraryDiagnosis(),
            Message = FindingMessage(result, finding),
            Subject = subject,
            Resolution = resolution,
            Candidates = [],
        };
    }

    private static string FindingMessage(RepairResult result, RepairFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Repair.RepairText.MessageTheRepairResultDidNotContainAFinding();
        }

        var proposal = Proposal(result, finding);
        var path = finding.SourceCanonicalPath ?? proposal?.SourceCanonicalPath ?? FailurePath(result, finding.Code.ToString());
        var occurrence = finding.OccurrenceView ?? proposal?.OccurrenceView;
        return finding.Code.ToString() switch
        {
            "GuidedFindingRemaining" when proposal is not null
                => RepairWording.GuidedFinding(proposal.ExpectedDestination, proposal.Candidates?.Items.Count ?? 0),
            "GuidedFindingRemaining" => RepairWording.GuidedFinding(0),
            "ManualFindingRemaining" when finding.Observation is not null => finding.Cause,
            "ManualFindingRemaining" => RepairWording.ManualFinding(),
            "ProposalUnavailable" when path is not null && occurrence is { } location
                => RepairWording.ProposalUnavailable(path, location.Line, location.Column),
            "FactsConflicting" when path is not null && occurrence is { } location
                => RepairWording.FactsConflicting(path, location.Line, location.Column),
            "ProposalUnsupported" when path is not null && occurrence is { } location
                => RepairWording.ProposalUnsupported(path, location.Line, location.Column, finding.Cause),
            "DiagnosisBlocked" => RepairWording.DiagnosisBlocked(finding.Cause),
            "DiagnosisIncomplete" when path is not null
                => RepairWording.DiagnosisIncomplete(path),
            "RecoveryArtifactRetained" when result.Recovery.ResidualPath is { } recovery
                => CliFindingWording.RecoveryRetained(recovery),
            "RecoveryUnavailable" when path is not null
                => CliFindingWording.RecoveryUnavailable(path),
            "RecoveryConflict" when path is not null
                => CliFindingWording.RecoveryConflict(path),
            "TargetChanged" when path is not null
                => CliFindingWording.TargetChanged(path),
            "TargetUnsafe" when path is not null
                => CliFindingWording.TargetUnsafe(path, TrimSentence(finding.Cause)),
            "WorkspaceLockUnavailable" => CliFindingWording.WorkspaceLockUnavailable(),
            "WriteFailed" when path is not null
                => CliFindingWording.WriteFailed(path, result.Counts.Repaired, Math.Max(result.Counts.SelectedEffects, result.Counts.Repaired), result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            "VerificationFailed" when path is not null && result.Recovery.ResidualPath is { } retained
                => CliFindingWording.VerificationFailed(path, retained),
            "RecoveryFailed" => CliFindingWording.RecoveryFailed(),
            "OperationFailed" => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepair(), TrimSentence(finding.Cause)),
            "Interrupted" => RepairWording.Cancelled(),
            _ => finding.Cause,
        };
    }

    private static string? FailurePath(RepairResult result, string code)
        => code is "WriteFailed" or "VerificationFailed" or "TargetChanged"
            && result.Plan is { } plan
                ? plan.Effects
                    .Skip(result.Application.AppliedEffects)
                    .Select(effect => effect.SourceCanonicalPath)
                    .FirstOrDefault()
                : null;

    private static bool IsAlreadyVisible(
        RepairResult result,
        IReadOnlyList<RepairDataRepair> repairs,
        RepairDataRemaining item)
    {
        if (item.Location is { } location
            && repairs.Any(repair => SameOccurrence(
                repair.Path,
                repair.Location.Line,
                repair.Location.Column,
                item.Path,
                location.Line,
                location.Column)))
        {
            return true;
        }

        return ReadFindings(result).Any(finding =>
            finding.Code.ToString() is "GuidedFindingRemaining" or "ManualFindingRemaining"
            && string.Equals(finding.SourceCanonicalPath ?? finding.Observation?.Path ?? finding.Observation?.Identifier, item.Path, StringComparison.Ordinal)
            && ((finding.OccurrenceView is { } occurrence && item.Location is { } itemLocation
                    && occurrence.Line == itemLocation.Line
                    && occurrence.Column == itemLocation.Column)
                || finding.OccurrenceView is null && item.Location is null));
    }

    private static CliNextAction? Next(RepairResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return null;
        }

        var codes = result.Findings
            .Select(finding => finding.Code.ToString())
            .ToHashSet(StringComparer.Ordinal);
        if (codes.Contains("ContradictoryRelink")
            || codes.Contains("PlanConflict")
            || codes.Contains("TargetUnsafe"))
        {
            return null;
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Next is { } remainingAction
            && remainingAction.Command == "open-forge repair")
        {
            var remaining = RemainingFindings(result).ToArray();
            var hasLibraryObservations = remaining.Any(IsLibraryObservation);
            if (hasLibraryObservations && !remaining.Any(finding => !IsLibraryObservation(finding)))
            {
                return new CliNextAction(
                    "open-forge doctor",
                    global::OpenForge.Cli.OutputText.Repair.RepairWording.InspectRemainingLibraryProblems());
            }

            if (hasLibraryObservations && remainingAction.Command == "open-forge repair")
            {
                return new CliNextAction(
                    remainingAction.Command,
                    global::OpenForge.Cli.OutputText.Repair.RepairWording.ReviewRemainingLinksAndInspectLibraryProblems())
                {
                    Kind = remainingAction.Kind,
                };
            }
        }

        if (result.Status == CliSemanticStatus.Attention
            && result.Next is { } attention
            && attention.Command == "open-forge repair")
        {
            return new CliNextAction(attention.Command, RepairWording.NextAttentionReason())
            {
                Kind = attention.Kind,
            };
        }

        return result.Next;
    }

    private static RepairProposal? Proposal(RepairResult result, RepairFinding finding)
        => result.Selection?.Unselected.FirstOrDefault(proposal =>
            proposal.SourceCanonicalPath == finding.SourceCanonicalPath
            && proposal.OccurrenceView == finding.OccurrenceView);

    private static IReadOnlyList<CliCount> Counts(RepairResult result, int plannedLinks)
        =>
        [
            new CliCount("linksRepaired", global::OpenForge.Cli.OutputText.Repair.RepairText.LabelLinksRepaired(), result.Mode == RepairMode.DryRun ? plannedLinks : result.Counts.Repaired),
            new CliCount("problemsRemaining", global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsRemaining(), result.Counts.Remaining),
            new CliCount("problemsNeedingChoice", global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsNeedingAChoice(), result.Counts.Guided),
            new CliCount("problemsNeedingHand", global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsNeedingAHand(), result.Counts.Manual),
            new CliCount("librarySteps", global::OpenForge.Cli.OutputText.Repair.RepairText.TitleLibraryRecoverySteps(), result.Plan is { } plan ? plan.LibrarySteps.Length : 0),
        ];

    private static IReadOnlyList<CliLimitation> Limitations(RepairResult result)
        => result.Findings
            .Where(finding => finding.Code.ToString() is "DiagnosisIncomplete" or "RecoveryUnavailable")
            .Select(finding => new CliLimitation(
                finding.SourceCanonicalPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspace(),
                FindingMessage(result, finding),
                finding.SourceCanonicalPath is { } path
                    ? new CliSubject(CliSubjectKind.File, path)
                    : null))
            .Distinct()
            .ToArray();

    private static CliRecovery Recovery(RepairResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                RepairRecoveryState.NotRequired or RepairRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
                RepairRecoveryState.Removed => CliRecoveryDisposition.Removed,
                RepairRecoveryState.Prepared or RepairRecoveryState.Retained => CliRecoveryDisposition.Retained,
                RepairRecoveryState.Incomplete or RepairRecoveryState.Blocked or RepairRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State, "The Repair recovery state is not defined."),
            });

    private static RepairDataVerification Verification(RepairResult result, bool hasWork)
        => new()
        {
            Targets = hasWork ? CliReportVocabulary.Name(result.Verification.Targets) : "not-requested",
            ResultingBytes = hasWork ? CliReportVocabulary.Name(result.Verification.ResultingBytes) : "not-requested",
            PostConditions = hasWork ? CliReportVocabulary.Name(result.Verification.PostConditions) : "not-requested",
        };

    private static RepairDataDiagnosis Coverage(RepairDiagnosisCoverage coverage)
        => new()
        {
            WorkspaceAndPath = CliReportVocabulary.Name(coverage.WorkspaceAndPath),
            RouteAndHeading = CliReportVocabulary.Name(coverage.RouteAndHeading),
            LocalReferences = CliReportVocabulary.Name(coverage.LocalReferences),
            SelectedScope = CliReportVocabulary.Name(coverage.SelectedScope),
        };

    private static IReadOnlyList<string> Diagnostics(
        RepairResult result,
        IReadOnlyList<RepairDataRepair> repairs,
        IReadOnlyList<RepairDataRemaining> remaining,
        IReadOnlyList<CliEffect> libraryEffects,
        bool hasWork)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={CliReportVocabulary.Name(result.Mode)}",
            $"selection={Selection(result)}",
            $"repairs={repairs.Count + libraryEffects.Count}",
            $"remaining={remaining.Count}",
            $"library-steps={(result.Plan is { } plan ? plan.LibrarySteps.Length : 0)}",
            $"verification={(hasWork ? CliReportVocabulary.Name(result.Verification.Targets) : "not run")}",
            $"next={(Next(result) is null ? "none" : "present")}",
        ];

    private static string Selection(RepairResult result)
        => result.Relinks.Count > 0
            ? "relink"
            : result.SelectionMode == RepairSelectionMode.InteractivePrompt
                ? "prompt"
                : result.Automatic
                    ? "automatic"
                    : "prompt";

    private static CliEffect Effect(RepairDataRepair repair)
        => new()
        {
            Path = RepairWording.Location(repair.Path, repair.Location.Line, repair.Location.Column),
            Kind = CliEffectKind.Link,
            Action = CliEffectAction.Rewritten,
            Outcome = EffectOutcome(repair.Outcome),
            Reason = $"{repair.From} -> {repair.To}",
            Before = repair.Before,
            After = repair.After,
        };

    private static CliEffectOutcome EffectOutcome(RepairStepOutcome outcome)
        => outcome switch
        {
            RepairStepOutcome.Planned => CliEffectOutcome.Planned,
            RepairStepOutcome.Applied or RepairStepOutcome.Verified => CliEffectOutcome.Done,
            RepairStepOutcome.Blocked => CliEffectOutcome.NotStarted,
            RepairStepOutcome.Failed => CliEffectOutcome.Failed,
            RepairStepOutcome.Interrupted => CliEffectOutcome.Unknown,
            RepairStepOutcome.NoOp => CliEffectOutcome.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Repair step outcome is not defined."),
        };

    private static string PartialRepairWording(RepairStepOutcome outcome)
        => outcome switch
        {
            RepairStepOutcome.Applied or RepairStepOutcome.Verified => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRewritten(),
            RepairStepOutcome.Planned or RepairStepOutcome.Blocked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            RepairStepOutcome.Interrupted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            RepairStepOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            RepairStepOutcome.NoOp => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Repair step outcome is not defined."),
        };

    private static string LibraryEffectWording(CliEffectOutcome outcome)
        => outcome switch
        {
            CliEffectOutcome.Planned or CliEffectOutcome.Done => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRestoredFromRecovery(),
            CliEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            CliEffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            CliEffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Repair effect outcome is not defined."),
        };

    private static string PreflightWording(RepairPreflightState state)
        => state switch
        {
            RepairPreflightState.NotRequested => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotRun(),
            RepairPreflightState.Ready => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelCompleted(),
            RepairPreflightState.Incomplete => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelCouldNotFinish(),
            RepairPreflightState.Blocked => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWereBlocked(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair preflight state is not defined."),
        };

    private static string PostDiagnosisWording(RepairPostDiagnosisState state)
        => state switch
        {
            RepairPostDiagnosisState.NotRequested => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotRun(),
            RepairPostDiagnosisState.Complete => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelCompleted(),
            RepairPostDiagnosisState.Incomplete => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelCouldNotFinish(),
            RepairPostDiagnosisState.Blocked => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWereBlocked(),
            RepairPostDiagnosisState.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair post-diagnosis state is not defined."),
        };

    private static bool SameOccurrence(
        string leftPath,
        int leftLine,
        int leftColumn,
        string rightPath,
        int rightLine,
        int rightColumn)
        => leftPath == rightPath && leftLine == rightLine && leftColumn == rightColumn;

    private static string MachineCode<T>(T code) where T : struct, Enum
        => $"repair.{CliReportVocabulary.Name(code)}";

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
