using System.Globalization;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;

internal static class RepairWording
{
    internal static string NothingToRepair() => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageNothingToRepair();

    internal static string Repaired(int count)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatRepaired(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link")}"));

    internal static string WouldRepair(int count)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatWouldRepair(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link")}"));

    internal static string RepairedWithRemaining(int repaired, int remaining, bool includesManual)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatRepairedStillA(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{repaired}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(repaired, "link")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{remaining}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "problem")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "needs", "need")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(includesManual ? "choice or a hand" : "choice")}"));

    internal static string WouldRepairWithRemaining(int repaired, int remaining, bool includesManual)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatWouldRepairStillA(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{repaired}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(repaired, "link")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{remaining}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "problem")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "needs", "need")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(includesManual ? "choice or a hand" : "choice")}"));

    internal static string NothingAutomatic(int remaining, bool includesManual)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatNothingCouldBeRepairedAutomaticallyA(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{remaining}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "problem")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(remaining, "needs", "need")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(includesManual ? "choice or a hand" : "choice")}"));

    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageRepairCouldNotCheckTheWorkspaceCompletelyNothingWasChanged();

    internal static string Cannot(string reason)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatCannotRepairNothingWasChanged($"{TrimSentence(reason)}");

    internal static string Invalid(string problem)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatCannotRepair($"{TrimSentence(problem)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Repair.RepairWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageRepairWasCancelledNothingWasChanged();

    internal static string GuidedFinding(int candidates)
        => candidates == 0
            ? global::OpenForge.Cli.OutputText.Repair.RepairText.TitleBrokenLinkNoPossibleTargetFixByHand()
            : global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatBrokenLinkPossible(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{candidates}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(candidates, "target")}"));

    internal static string GuidedFinding(string destination, int candidates)
        => global::OpenForge.Cli.OutputText.Repair.RepairWording.BrokenDestination(destination, GuidedFinding(candidates));

    internal static string ManualFinding() => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleBrokenLinkNoPossibleTargetFixByHand();

    internal static string ProposalUnavailable(string path, int line, int column)
        => OpenForge.Cli.OutputText.Repair.RepairLocationText.ProposalUnavailable(Location(path, line, column));

    internal static string FactsConflicting(string path, int line, int column)
        => OpenForge.Cli.OutputText.Repair.RepairLocationText.FactsConflicting(Location(path, line, column));

    internal static string ProposalUnsupported(string path, int line, int column, string reason)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatTheRepairForCannotBeApplied($"{Location(path, line, column)}", $"{TrimSentence(reason)}");

    internal static string DiagnosisBlocked(string reason)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatTheWorkspaceCouldNotBeChecked($"{TrimSentence(reason)}");

    internal static string DiagnosisIncomplete(string path)
        => global::OpenForge.Cli.OutputText.Repair.RepairWording.DiagnosisIncomplete(path);

    internal static string FindingTitle(string code) => code switch
    {
        "InvalidInput" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        "ConfirmationRequired" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        "RelinkInvalid" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleInvalidRelink(),
        "ContradictoryRelink" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleContradictoryRelink(),
        "SelectionRequired" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleSelectionRequired(),
        "DiagnosisIncomplete" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleWorkspaceCouldNotBeChecked(),
        "DiagnosisBlocked" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleWorkspaceCouldNotBeChecked(),
        "ProposalUnavailable" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairProposalUnavailable(),
        "FactsConflicting" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairFactsConflict(),
        "ProposalUnsupported" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairProposalUnsupported(),
        "MissingAuthority" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairAuthorityIsMissing(),
        "TargetChanged" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        "TargetUnsafe" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        "PlanConflict" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairPlanConflict(),
        "WorkspaceLockUnavailable" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
        "RecoveryUnavailable" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        "RecoveryConflict" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRecoveryDataBlocksRepair(),
        "GuidedFindingRemaining" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleBrokenLink(),
        "ManualFindingRemaining" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleBrokenLink(),
        "RecoveryArtifactRetained" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        "WriteFailed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        "VerificationFailed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        "RecoveryFailed" => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        "OperationFailed" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairFailed(),
        "Interrupted" => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleRepairWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Repair finding code is not defined."),
    };

    internal static string Recovery(RepairRecoveryState state, string? path)
        => state switch
        {
            RepairRecoveryState.NotRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasRequired(),
            RepairRecoveryState.NotCreated => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasCreated(),
            RepairRecoveryState.Prepared => path is null
                ? global::OpenForge.Cli.OutputText.Repair.RepairText.MessageARecoveryBundleWasPrepared()
                : global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatARecoveryBundleWasPreparedAt($"{path}"),
            RepairRecoveryState.Removed => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRemoved(),
            RepairRecoveryState.Retained => path is null
                ? global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRetained()
                : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt($"{path}"),
            RepairRecoveryState.Incomplete => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageTheRecoveryBundleStateIsIncomplete(),
            RepairRecoveryState.Blocked => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageTheRecoveryBundleStateIsBlocked(),
            RepairRecoveryState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalStateOfTheRecoveryBundleIsUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair recovery state is not defined."),
        };

    internal static string Verification(
        RepairVerificationState targets,
        RepairVerificationState bytes,
        RepairVerificationState postConditions)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatVerificationTargets($"{VerificationState(targets, postConditions: false)}")
            + global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatResultingBytes($"{VerificationState(bytes, postConditions: false)}")
            + global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatPostConditions($"{VerificationState(postConditions, postConditions: true)}");

    internal static string Coverage(RepairDiagnosisCoverage coverage)
        => string.Join(
            "; ",
            CoverageClause(global::OpenForge.Cli.OutputText.Repair.RepairText.TitleWorkspaceAndPath(), coverage.WorkspaceAndPath, plural: true),
            CoverageClause(global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRouteAndHeading(), coverage.RouteAndHeading, plural: true),
            CoverageClause(global::OpenForge.Cli.OutputText.Repair.RepairText.LabelLocalReferences(), coverage.LocalReferences, plural: true),
            CoverageClause(global::OpenForge.Cli.OutputText.Repair.RepairText.LabelSelectedScope(), coverage.SelectedScope, plural: false));

    internal static string CountLabel(string name, decimal? value)
    {
        var singular = value == 1m;
        return name switch
        {
            "linksRepaired" => singular ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelLinkRepaired() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelLinksRepaired(),
            "problemsRemaining" => singular ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemRemaining() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsRemaining(),
            "problemsNeedingChoice" => singular ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemNeedingAChoice() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsNeedingAChoice(),
            "problemsNeedingHand" => singular ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemNeedingAHand() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelProblemsNeedingAHand(),
            "librarySteps" => singular ? global::OpenForge.Cli.OutputText.Repair.RepairText.TitleLibraryRecoveryStep() : global::OpenForge.Cli.OutputText.Repair.RepairText.TitleLibraryRecoverySteps(),
            "errors" => singular ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelError() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelErrors(),
            "warnings" => singular ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelWarning() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelWarnings(),
            "infos" => singular ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInfo() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInfos(),
            _ => name,
        };
    }

    internal static string CandidateReason(RepairCandidateEvidence evidence)
        => $"{EvidenceKind(evidence.Kind)}: {evidence.Value}";

    internal static string Target(RepairTargetSelection target)
        => target.TargetFragment is { } fragment
            ? $"{target.CanonicalTargetPath}#{fragment}"
            : target.CanonicalTargetPath;

    internal static string Location(string path, int line, int column)
        => string.Create(CultureInfo.InvariantCulture, $"{path}:{line}:{column}");

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpSyntax());
    internal static string HelpSelection() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpSelection());
    internal static string HelpCatalogue() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpCatalogue());
    internal static string HelpLibraryRecovery() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpLibraryRecovery());
    internal static string HelpPreviewAndSafety() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpPreviewAndSafety());
    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection());
    internal static string HelpNotes() => ("  " + global::OpenForge.Cli.OutputText.Repair.RepairText.HelpNotes());

    internal static string NextAttentionReason()
        => global::OpenForge.Cli.OutputText.Repair.RepairText.MessageReviewRemainingBrokenLinksInRepairThenSelectAPossibleTargetInTheInteractivePromptOrProvideAnExplicitRelinkTarget();

    internal static string ReferenceQuestion(RepairProposal proposal)
        => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatChooseATargetFor(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.SourceCanonicalPath}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.OccurrenceView.Line}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.OccurrenceView.Column}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.ExpectedDestination}"));

    internal static string CandidateLabel(RepairCandidate candidate)
        => candidate.Target.TargetFragment is { } fragment
            ? $"{candidate.Target.CanonicalTargetPath}#{fragment}"
            : candidate.Target.CanonicalTargetPath;

    internal static string CandidateDescription(RepairCandidate candidate)
    {
        var evidence = string.Join(
            "; ",
            candidate.Evidence.Select(value => $"{EvidenceKind(value.Kind)}: {value.Value}"));
        return candidate.RecommendedForReview
            ? global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatRecommendedForReview($"{evidence}")
            : evidence;
    }

    internal static string LibraryQuestion(RepairLibraryPromptQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);
        var proposal = question.Proposal;
        return global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatLibraryRecoverAt(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.LibraryIdValue}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{EntryKind(question.EntryKind)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{proposal.TargetPath}"));
    }

    internal static string Skip() => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelSkip();

    internal static string Select() => global::OpenForge.Cli.OutputText.Repair.RepairText.TitleSelect();

    internal static string Confirmation(RepairConfirmationQuestion question)
        => question.Kind switch
        {
            RepairConfirmationKind.Safe => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatApplyTheThatSafeYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{question.Count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(question.Count, "repair")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(question.Count, "is", "are")}")),
            RepairConfirmationKind.Final => CliPromptWording.Confirm(),
            _ => throw new ArgumentOutOfRangeException(nameof(question), question.Kind, "The Repair confirmation kind is not defined."),
        };

    private static string EvidenceKind(RepairCandidateEvidenceKind kind)
        => kind switch
        {
            RepairCandidateEvidenceKind.Filename => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilenameMatch(),
            RepairCandidateEvidenceKind.Title => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTitleMatch(),
            RepairCandidateEvidenceKind.LiteralContent => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelContentMatch(),
            RepairCandidateEvidenceKind.RouteNeighborhood => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNearbyRoute(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Repair evidence kind is not defined."),
        };

    private static string EntryKind(RepairLibraryPromptEntryKind kind)
        => kind switch
        {
            RepairLibraryPromptEntryKind.OrdinaryCreate => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelOrdinaryCreate(),
            RepairLibraryPromptEntryKind.OrdinaryReplace => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelOrdinaryReplace(),
            RepairLibraryPromptEntryKind.OrdinaryReplaceGeneratedRegion => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelGeneratedRegionReplace(),
            RepairLibraryPromptEntryKind.OrdinaryDelete => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelOrdinaryDelete(),
            RepairLibraryPromptEntryKind.RelativeFileLinkCreate => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRelativeLinkCreate(),
            RepairLibraryPromptEntryKind.RelativeFileLinkDelete => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelRelativeLinkDelete(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The recovery entry kind is not defined."),
        };

    private static string CoverageClause(
        string subject,
        RepairCoverageState state,
        bool plural)
    {
        var verb = plural ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWere() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWas();
        return state switch
        {
            RepairCoverageState.NotRequested => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatNotChecked($"{subject}", $"{verb}"),
            RepairCoverageState.Complete => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatChecked($"{subject}", $"{verb}"),
            RepairCoverageState.Incomplete => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatCouldNotBeChecked($"{subject}"),
            RepairCoverageState.Blocked => global::OpenForge.Cli.OutputText.Repair.RepairPhrases.FormatBlocked($"{subject}", $"{verb}"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair coverage state is not defined."),
        };
    }

    private static string VerificationState(
        RepairVerificationState state,
        bool postConditions)
        => state switch
        {
            RepairVerificationState.NotRequested => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWereNotChecked(),
            RepairVerificationState.Planned => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelWerePlannedButNotChecked(),
            RepairVerificationState.Verified => postConditions ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelHeld() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelMatched(),
            RepairVerificationState.Failed => postConditions ? global::OpenForge.Cli.OutputText.Repair.RepairText.LabelDidNotHold() : global::OpenForge.Cli.OutputText.Repair.RepairText.LabelDidNotMatch(),
            RepairVerificationState.Unknown => global::OpenForge.Cli.OutputText.Repair.RepairText.LabelCouldNotBeDetermined(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair verification state is not defined."),
        };

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
