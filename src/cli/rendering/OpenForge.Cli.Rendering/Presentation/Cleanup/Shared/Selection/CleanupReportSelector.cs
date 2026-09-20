using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Presentation.Cleanup.Models;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Selection;

internal static class CleanupReportSelector
{
    internal static CliReport<CleanupData> Select(CleanupResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var preview = CliReportVocabulary.Name(result.Mode) == "dry-run";
        var verificationFailures = result.Findings.Any(finding => finding.Code == CleanupFindingCode.VerificationFailed);
        var removedEffects = result.Status is CliSemanticStatus.Complete
            or CliSemanticStatus.Attention
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted
            ? result.Effects.Where(effect => IsRemoved(effect, preview)).ToArray()
            : [];
        var bundlesRemoved = removedEffects.Count(effect => CleanupWording.DataKind(effect.KindValue) == "bundle");
        var draftsRemoved = removedEffects.Count(effect => CleanupWording.DataKind(effect.KindValue) == "draft");
        var itemsLeftInPlace = result.Catalogue.Candidates.Count(candidate => CleanupWording.IsPreserved(candidate.Action))
            + result.Effects.Count(effect => effect.Residual != CleanupEffectResidual.None);
        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var data = new CleanupData
        {
            Mode = CliReportVocabulary.Name(result.Mode),
            Items = result.Effects.Select(effect => new CleanupDataItem
            {
                Path = effect.Path,
                Kind = CleanupWording.DataKind(effect.KindValue),
                Outcome = CleanupWording.DataOutcome(effect),
                Origin = standard ? CleanupWording.Origin(effect) : null,
                Integrity = standard ? CleanupWording.Integrity(effect.IntegrityValue) : null,
            }).ToArray(),
            NotEligible = standard
                ? result.Catalogue.Candidates
                    .Where(candidate => CleanupWording.IsPreserved(candidate.Action))
                    .Select(candidate => new CleanupDataNotEligible
                    {
                        Path = candidate.Path,
                        Reason = CleanupWording.CandidateRowReason(candidate),
                    })
                    .ToArray()
                : null,
            Lock = full ? CleanupWording.LockFacts(result.Lease.State) : null,
            FinalCheck = full ? CleanupWording.FinalCheckFacts(result.Revalidation.State) : null,
            TextRows = TextRows(result, selection.Detail, verificationFailures),
            TextDetailLines = full
                ? [CleanupWording.LockFacts(result.Lease.State), CleanupWording.FinalCheckFacts(result.Revalidation.State)]
                : [],
            ShowNoChanges = preview && result.Effects.Length > 0,
        };

        return new CliReport<CleanupData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, data, bundlesRemoved, draftsRemoved, preview),
            HeadlineFindingCode = HeadlineFindingCode(result, bundlesRemoved, draftsRemoved),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings
                .Select(finding => Finding(result, finding, bundlesRemoved + draftsRemoved, result.Effects.Length))
                .ToArray(),
            Effects = result.Effects.Select(Effect).ToArray(),
            Counts =
            [
                new CliCount("bundlesRemoved", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelRecoveryBundlesRemoved(), bundlesRemoved),
                new CliCount("draftsRemoved", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelUnfinishedDraftsRemoved(), draftsRemoved),
                new CliCount("itemsLeftInPlace", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.LabelItemsLeftInPlace(), itemsLeftInPlace),
            ],
            Data = data,
            Recovery = null,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data)
                : [],
        };
    }

    private static CliHeadline Headline(
        CleanupResult result,
        CleanupData data,
        int bundlesRemoved,
        int draftsRemoved,
        bool preview)
    {
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when data.Items.Count == 0
                => new(CleanupWording.NoData(), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete
                => new(CleanupWording.RemovedSummary(bundlesRemoved, draftsRemoved, preview),
                    preview ? CliHeadlineKind.Preview : CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(AttentionHeadline(result, bundlesRemoved, draftsRemoved, preview), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(CleanupWording.StoreIncomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(CleanupWording.FindingMessage(
                        first ?? throw new InvalidOperationException("An invalid Cleanup result requires a finding."),
                        first.Subject ?? result.Command,
                        0,
                        result.Effects.Length),
                    CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(BlockedHeadline(result, first), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(CleanupWording.Failed(bundlesRemoved + draftsRemoved, result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(CleanupWording.Cancelled(bundlesRemoved + draftsRemoved, result.Effects.Length), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Cleanup status is not defined."),
        };
    }

    private static string AttentionHeadline(
        CleanupResult result,
        int bundlesRemoved,
        int draftsRemoved,
        bool preview)
    {
        if (AttentionHeadlineFinding(result, bundlesRemoved, draftsRemoved)
            is { Subject: { } subject } malformed)
        {
            return CleanupWording.FindingMessage(malformed, subject, 0, result.Effects.Length);
        }

        return CleanupWording.RemovedSummary(bundlesRemoved, draftsRemoved, preview);
    }

    private static CleanupFinding? AttentionHeadlineFinding(
        CleanupResult result,
        int bundlesRemoved,
        int draftsRemoved)
    {
        if (bundlesRemoved != 0 || draftsRemoved != 0)
        {
            return null;
        }

        return result.Findings.FirstOrDefault(finding =>
            finding.Status == result.Status
            && finding.Code == CleanupFindingCode.RecoveryFinalMalformed
            && finding.Subject is not null);
    }

    private static string BlockedHeadline(CleanupResult result, CleanupFinding? first)
    {
        if (result.Findings.FirstOrDefault(finding => finding.Code == CleanupFindingCode.WorkspaceLockUnavailable) is not null)
        {
            return CleanupWording.LockHeld();
        }

        if (result.Findings.FirstOrDefault(finding => finding.Code == CleanupFindingCode.CatalogueChangedDuringApply) is not null)
        {
            return CleanupWording.ChangedDuringApply();
        }

        var candidateFinding = result.Findings.FirstOrDefault(finding => finding.Code is
            CleanupFindingCode.RecoveryFinalMalformed
                or CleanupFindingCode.RecoveryFinalUnsupported
                or CleanupFindingCode.RecoveryFinalUnavailable
                or CleanupFindingCode.RecoveryDraftUnsafe);
        if (candidateFinding is not null)
        {
            var candidate = result.Catalogue.Candidates.FirstOrDefault(value =>
                string.Equals(value.Path, candidateFinding.Subject, StringComparison.Ordinal));
            if (candidate is not null)
            {
                return (global::OpenForge.Cli.OutputText.Cleanup.CleanupText.HeadingCannotCleanUp() + " ") + CleanupWording.BlockedCandidate(candidate.Path, candidate.IntegrityValue);
            }
        }

        return first is null
            ? global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCannotCleanUpTheWorkspaceCouldNotBeVerifiedNothingWasRemoved()
            : CleanupWording.FindingMessage(first, first.Subject ?? result.Command, 0, result.Effects.Length);
    }

    private static string? HeadlineFindingCode(
        CleanupResult result,
        int bundlesRemoved,
        int draftsRemoved)
    {
        if (result.Status == CliSemanticStatus.Attention
            && AttentionHeadlineFinding(result, bundlesRemoved, draftsRemoved) is null)
        {
            return null;
        }

        if (result.Status is not (CliSemanticStatus.Invalid
            or CliSemanticStatus.Attention
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted))
        {
            return null;
        }

        var findings = result.Findings.Where(finding => finding.Status == result.Status).ToArray();
        if (findings.Length != 1)
        {
            return null;
        }

        return CleanupWording.FindingCode(findings[0].Code);
    }

    private static CliFinding Finding(
        CleanupResult result,
        CleanupFinding finding,
        int removed,
        int total)
    {
        var path = finding.Subject ?? result.WorkspacePath ?? result.Command;
        var workspaceFinding = finding.Code is
            CleanupFindingCode.WorkspaceUnavailable
                or CleanupFindingCode.WorkspaceNotDirectory
                or CleanupFindingCode.WorkspaceUnsafe;
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = CleanupWording.FindingCode(finding.Code),
            Title = CleanupWording.FindingTitle(finding.Code),
            Message = CleanupWording.FindingMessage(finding, path, removed, total),
            Subject = workspaceFinding
                ? new CliSubject(CliSubjectKind.Workspace, path)
                : finding.Subject is not null
                    ? new CliSubject(CliSubjectKind.File, finding.Subject)
                    : new CliSubject(CliSubjectKind.Identifier, result.Command),
            Actions = Actions(finding.Code),
        };
    }

    private static IReadOnlyList<CliNextAction> Actions(CleanupFindingCode code)
        => code switch
        {
            CleanupFindingCode.CatalogueIncomplete =>
                [new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageInspectTheRecoveryStoreBeforeRelyingOnThisCleanupResult())],
            CleanupFindingCode.RecoveryFinalMalformed
                or CleanupFindingCode.RecoveryFinalUnsupported =>
                [new CliNextAction(global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageRemoveItByHandAfterReview(), global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageReviewTheRecoveryFileBeforeRemovingIt()) { Kind = CliNextActionKind.Sentence }],
            CleanupFindingCode.WorkspaceLockUnavailable
                or CleanupFindingCode.CatalogueChangedDuringApply
                or CleanupFindingCode.CandidateChangedDuringApply
                or CleanupFindingCode.DeletionFailed
                or CleanupFindingCode.VerificationFailed
                or CleanupFindingCode.Interrupted =>
                [new CliNextAction("open-forge cleanup", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageRerunCleanupFromAFreshCatalogue())],
            CleanupFindingCode.InvalidInput =>
                [new CliNextAction("open-forge cleanup --help", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCorrectTheCleanupInputThenRerunTheRequest())],
            _ => [],
        };

    private static CliNextAction? Next(CleanupResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return null;
        }

        if (result.Status == CliSemanticStatus.Incomplete)
        {
            return new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageInspectTheRecoveryStoreBeforeRelyingOnThisCleanupResult());
        }

        if (result.Status == CliSemanticStatus.Invalid)
        {
            return new CliNextAction("open-forge cleanup --help", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageCorrectTheCleanupInputThenRerunTheRequest());
        }

        if (result.Findings.Any(finding => finding.Code is
            CleanupFindingCode.RecoveryFinalMalformed
                or CleanupFindingCode.RecoveryFinalUnsupported))
        {
            return new CliNextAction(global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageRemoveItByHandAfterReview(), global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageReviewTheRecoveryFileBeforeRemovingIt())
            {
                Kind = CliNextActionKind.Sentence,
            };
        }

        return new CliNextAction("open-forge cleanup", global::OpenForge.Cli.OutputText.Cleanup.CleanupText.MessageRerunCleanupFromAFreshCatalogue());
    }

    private static IReadOnlyList<CleanupDataTextRow> TextRows(
        CleanupResult result,
        CliDetail detail,
        bool verificationFailures)
    {
        var rows = result.Effects
            .Select(effect =>
            {
                var wording = CleanupWording.EffectWording(effect, verificationFailures);
                var metadata = detail >= CliDetail.Standard ? CleanupWording.EffectMetadata(effect) : null;
                var integrity = detail >= CliDetail.Full ? CleanupWording.IntegrityCheck(effect) : null;
                return new CleanupDataTextRow(
                    effect.Path,
                    metadata is null ? wording : $"{wording} ({metadata})",
                    integrity);
            })
            .ToList();

        if (detail >= CliDetail.Standard || result.Status == CliSemanticStatus.Attention)
        {
            rows.AddRange(result.Catalogue.Candidates
                .Where(candidate => CleanupWording.IsPreserved(candidate.Action))
                .Select(candidate => new CleanupDataTextRow(
                    candidate.Path,
                    global::OpenForge.Cli.OutputText.Cleanup.CleanupPhrases.FormatLeftInPlace($"{CleanupWording.CandidateRowReason(candidate)}"),
                    detail >= CliDetail.Full ? CleanupWording.CandidateIntegrityCheck(candidate) : null)));
        }

        return rows;
    }

    private static CliEffect Effect(CleanupEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = CliEffectKind.File,
            Action = CliEffectAction.Deleted,
            Outcome = effect.Outcome switch
            {
                CleanupEffectOutcome.Planned => CliEffectOutcome.Planned,
                CleanupEffectOutcome.Verified => CliEffectOutcome.Done,
                CleanupEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                CleanupEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                CleanupEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The cleanup effect outcome is not defined."),
            },
            Reason = effect.Cause is { } cause
                ? CliFindingWording.CauseSentence(cause)
                : null,
            Owner = effect.Provenance?.Command,
        };

    private static IReadOnlyList<string> Diagnostics(CleanupResult result, CleanupData data)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"items={data.Items.Count}",
            $"findings={result.Findings.Length}",
            $"lock={CliReportVocabulary.Name(result.Lease.State)}",
            $"final-check={CliReportVocabulary.Name(result.Revalidation.State)}",
        ];

    private static bool IsRemoved(CleanupEffect effect, bool preview)
        => preview
            ? effect.Outcome == CleanupEffectOutcome.Planned
            : effect.Outcome == CleanupEffectOutcome.Verified;
}
