using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairText
{
    // @OpenForgeText repair.message.nothing-to-repair
    internal static string MessageNothingToRepair()
        => "Nothing to repair.";

    // @OpenForgeText repair.message.repair-could-not-check-the-workspace-completely-nothing-was-changed
    internal static string MessageRepairCouldNotCheckTheWorkspaceCompletelyNothingWasChanged()
        => "Repair could not check the workspace completely. Nothing was changed.";

    // @OpenForgeText repair.message.repair-was-cancelled-nothing-was-changed
    internal static string MessageRepairWasCancelledNothingWasChanged()
        => "Repair was cancelled. Nothing was changed.";

    // @OpenForgeText repair.title.broken-link-no-possible-target-fix-by-hand
    internal static string TitleBrokenLinkNoPossibleTargetFixByHand()
        => "Broken link; no possible target, fix by hand";

    // @OpenForgeText repair.help.syntax
    internal static string HelpSyntax()
        => "open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global options]";

    // @OpenForgeText repair.help.selection
    internal static string HelpSelection()
        => "Run repair interactively to choose from the available repairs. --automatic selects currently verified safe corrections; --relink selects one current occurrence and contained target. Guided choices are never selected automatically.";

    // @OpenForgeText repair.help.catalogue
    internal static string HelpCatalogue()
        => "Repair can correct path spelling, case, encoding, and unique fragments while preserving the target. You can also explicitly relink a missing target to a possible target reported by Doctor.\nIt can also complete selected incomplete Library operations when current evidence proves the correction is safe.";

    // @OpenForgeText repair.help.library-recovery
    internal static string HelpLibraryRecovery()
        => "Library recovery requires a matching current-v1 recovery record, matching link identity without following the link, and current permission to change links outside .agents.\nIt adds no recovery syntax, never follows or mutates source targets, and does not restore or widen permission grants.";

    // @OpenForgeText repair.help.preview-and-safety
    internal static string HelpPreviewAndSafety()
        => "--dry-run previews the complete selected plan and writes nothing. Every eventual replacement carries the expected file state and the version this CLI ships, exact verification, and external recovery attribution.";

    // @OpenForgeText repair.help.notes
    internal static string HelpNotes()
        => "Repair is stateless and local. It does not author content, change route or generated navigation, clean recovery artifacts, or invoke another command as a subprocess.";

    // @OpenForgeText repair.message.review-remaining-broken-links-in-repair-then-select-a-possible-target-in-the-interactive-prompt-or-provide-an-explicit-relink-target
    internal static string MessageReviewRemainingBrokenLinksInRepairThenSelectAPossibleTargetInTheInteractivePromptOrProvideAnExplicitRelinkTarget()
        => "Review remaining broken links in Repair, then select a possible target in the interactive prompt or provide an explicit --relink target.";

    // @OpenForgeText repair.label.skip
    internal static string LabelSkip()
        => "skip";

    // @OpenForgeText repair.title.select
    internal static string TitleSelect()
        => "Select";

    // @OpenForgeText repair.label.the-repair-request-is-blocked
    internal static string LabelTheRepairRequestIsBlocked()
        => "the Repair request is blocked";

    // @OpenForgeText repair.label.repair-needs-to-know-which-repairs-to-apply-and-this-session-cannot-ask
    internal static string LabelRepairNeedsToKnowWhichRepairsToApplyAndThisSessionCannotAsk()
        => "repair needs to know which repairs to apply, and this session cannot ask";

    // @OpenForgeText repair.message.the-repair-result-did-not-contain-a-finding
    internal static string MessageTheRepairResultDidNotContainAFinding()
        => "The Repair result did not contain a finding.";

    // @OpenForgeText repair.label.links-repaired
    internal static string LabelLinksRepaired()
        => "links repaired";

    // @OpenForgeText repair.label.problems-remaining
    internal static string LabelProblemsRemaining()
        => "problems remaining";

    // @OpenForgeText repair.label.problems-needing-a-choice
    internal static string LabelProblemsNeedingAChoice()
        => "problems needing a choice";

    // @OpenForgeText repair.label.problems-needing-a-hand
    internal static string LabelProblemsNeedingAHand()
        => "problems needing a hand";

    // @OpenForgeText repair.title.library-recovery-steps
    internal static string TitleLibraryRecoverySteps()
        => "Library recovery steps";

    // @OpenForgeText repair.label.restored-from-recovery
    internal static string LabelRestoredFromRecovery()
        => "restored from recovery";

    // @OpenForgeText repair.label.could-not-finish
    internal static string LabelCouldNotFinish()
        => "could not finish";

    // @OpenForgeText repair.label.were-blocked
    internal static string LabelWereBlocked()
        => "were blocked";

    // @OpenForgeText repair.title.invalid-relink
    internal static string TitleInvalidRelink()
        => "Invalid --relink";

    // @OpenForgeText repair.title.contradictory-relink
    internal static string TitleContradictoryRelink()
        => "Contradictory --relink";

    // @OpenForgeText repair.title.selection-required
    internal static string TitleSelectionRequired()
        => "Selection required";

    // @OpenForgeText repair.title.workspace-could-not-be-checked
    internal static string TitleWorkspaceCouldNotBeChecked()
        => "Workspace could not be checked";

    // @OpenForgeText repair.title.repair-proposal-unavailable
    internal static string TitleRepairProposalUnavailable()
        => "Repair proposal unavailable";

    // @OpenForgeText repair.title.repair-facts-conflict
    internal static string TitleRepairFactsConflict()
        => "Repair facts conflict";

    // @OpenForgeText repair.title.repair-proposal-unsupported
    internal static string TitleRepairProposalUnsupported()
        => "Repair proposal unsupported";

    // @OpenForgeText repair.title.repair-authority-is-missing
    internal static string TitleRepairAuthorityIsMissing()
        => "Repair authority is missing";

    // @OpenForgeText repair.title.repair-plan-conflict
    internal static string TitleRepairPlanConflict()
        => "Repair plan conflict";

    // @OpenForgeText repair.title.recovery-data-blocks-repair
    internal static string TitleRecoveryDataBlocksRepair()
        => "Recovery data blocks repair";

    // @OpenForgeText repair.title.repair-failed
    internal static string TitleRepairFailed()
        => "Repair failed";

    // @OpenForgeText repair.title.repair-was-cancelled
    internal static string TitleRepairWasCancelled()
        => "Repair was cancelled";

    // @OpenForgeText repair.message.a-recovery-bundle-was-prepared
    internal static string MessageARecoveryBundleWasPrepared()
        => "A recovery bundle was prepared.";

    // @OpenForgeText repair.message.the-recovery-bundle-state-is-incomplete
    internal static string MessageTheRecoveryBundleStateIsIncomplete()
        => "The recovery bundle state is incomplete.";

    // @OpenForgeText repair.message.the-recovery-bundle-state-is-blocked
    internal static string MessageTheRecoveryBundleStateIsBlocked()
        => "The recovery bundle state is blocked.";

    // @OpenForgeText repair.title.workspace-and-path
    internal static string TitleWorkspaceAndPath()
        => "Workspace and path";

    // @OpenForgeText repair.label.route-and-heading
    internal static string LabelRouteAndHeading()
        => "route and heading";

    // @OpenForgeText repair.label.local-references
    internal static string LabelLocalReferences()
        => "local references";

    // @OpenForgeText repair.label.selected-scope
    internal static string LabelSelectedScope()
        => "selected scope";

    // @OpenForgeText repair.label.link-repaired
    internal static string LabelLinkRepaired()
        => "link repaired";

    // @OpenForgeText repair.label.problem-remaining
    internal static string LabelProblemRemaining()
        => "problem remaining";

    // @OpenForgeText repair.label.problem-needing-a-choice
    internal static string LabelProblemNeedingAChoice()
        => "problem needing a choice";

    // @OpenForgeText repair.label.problem-needing-a-hand
    internal static string LabelProblemNeedingAHand()
        => "problem needing a hand";

    // @OpenForgeText repair.title.library-recovery-step
    internal static string TitleLibraryRecoveryStep()
        => "Library recovery step";

    // @OpenForgeText repair.label.ordinary-create
    internal static string LabelOrdinaryCreate()
        => "ordinary create";

    // @OpenForgeText repair.label.ordinary-replace
    internal static string LabelOrdinaryReplace()
        => "ordinary replace";

    // @OpenForgeText repair.label.generated-region-replace
    internal static string LabelGeneratedRegionReplace()
        => "generated-region replace";

    // @OpenForgeText repair.label.ordinary-delete
    internal static string LabelOrdinaryDelete()
        => "ordinary delete";

    // @OpenForgeText repair.label.relative-link-create
    internal static string LabelRelativeLinkCreate()
        => "relative-link create";

    // @OpenForgeText repair.label.relative-link-delete
    internal static string LabelRelativeLinkDelete()
        => "relative-link delete";

    // @OpenForgeText repair.label.were
    internal static string LabelWere()
        => "were";

    // @OpenForgeText repair.label.was
    internal static string LabelWas()
        => "was";

    // @OpenForgeText repair.label.were-not-checked
    internal static string LabelWereNotChecked()
        => "were not checked";

    // @OpenForgeText repair.label.were-planned-but-not-checked
    internal static string LabelWerePlannedButNotChecked()
        => "were planned but not checked";

    // @OpenForgeText repair.label.held
    internal static string LabelHeld()
        => "held";

    // @OpenForgeText repair.label.matched
    internal static string LabelMatched()
        => "matched";

    // @OpenForgeText repair.label.did-not-hold
    internal static string LabelDidNotHold()
        => "did not hold";

    // @OpenForgeText repair.label.did-not-match
    internal static string LabelDidNotMatch()
        => "did not match";

    // @OpenForgeText repair.label.could-not-be-determined
    internal static string LabelCouldNotBeDetermined()
        => "could not be determined";

    // @OpenForgeText repair.label.skipped
    internal static string LabelSkipped()
        => "skipped";

    // @OpenForgeText repair.title.repair
    internal static string TitleRepair()
        => "Repair";

    // @OpenForgeText repair.label.rewritten
    internal static string LabelRewritten()
        => "rewritten";

    // @OpenForgeText repair.label.completed
    internal static string LabelCompleted()
        => "completed";

    // @OpenForgeText repair.help.heading.library-recovery
    internal static string HelpHeadingLibraryRecovery()
        => "Library recovery";

    // @OpenForgeText repair.help.heading.preview-and-safety
    internal static string HelpHeadingPreviewAndSafety()
        => "Preview and safety";
}
