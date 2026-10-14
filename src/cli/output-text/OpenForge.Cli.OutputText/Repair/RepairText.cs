using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairText
{
    // @OpenForgeText repair.wording.library-selection-question
    internal static string LibrarySelectionQuestion() => "Choose one Library recovery step for this run";

    // @OpenForgeText repair.wording.library-links-restored
    internal static string LibraryLinksRestored(int count, bool preview)
    {
        var noun = count == 1 ? "Library link" : "Library links";
        return $"{noun} {(preview ? "to restore" : "restored")}";
    }

    // @OpenForgeText repair.wording.library-links-remaining
    internal static string LibraryLinksRemaining(int count, bool preview)
    {
        var noun = count == 1 ? "Library link" : "Library links";
        if (preview)
        {
            return $"{noun} would still need recovery";
        }
        return $"{noun} {(count == 1 ? "still needs" : "still need")} recovery";
    }

    // @OpenForgeText repair.wording.leave-all-recovery-out
    internal static string LeaveAllRecoveryOut() => "Leave all recovery steps out";

    // @OpenForgeText repair.wording.candidate-lists-source
    internal static string CandidateListsSource() => "The file that lists this source.";

    // @OpenForgeText repair.wording.candidate-shares-route-parent
    internal static string CandidateSharesRouteParent() => "Listed alongside this source in the same route.";

    // @OpenForgeText repair.wording.candidate-in-same-folder
    internal static string CandidateInSameFolder() => "In the same folder as this link.";

    // @OpenForgeText repair.wording.candidate-listed-by-source
    internal static string CandidateListedBySource() => "Listed by this source.";
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
        => "This link still needs repair.";

    // @OpenForgeText repair.help.syntax
    internal static string HelpSyntax()
        => "open-forge repair [options]";

    // @OpenForgeText repair.help.selection
    internal static string HelpSelection()
        => "Run repair interactively to choose from the available repairs. --automatic selects currently verified safe corrections. --relink selects one current occurrence and contained target. Guided choices are never selected automatically.";

    // @OpenForgeText repair.help.catalogue
    internal static string HelpCatalogue()
        => "Repair can correct path spelling, case, encoding, and unique fragments while preserving the target. It can also repair a uniquely proven rename or move and update affected Entries, metadata and navigation files. You can explicitly relink a missing target to a reported possible target.\nIt can also complete selected incomplete Library operations when current evidence proves the correction is safe.";

    // @OpenForgeText repair.help.library-recovery
    internal static string HelpLibraryRecovery()
        => "Library recovery requires a matching current-v1 recovery record, matching link identity without following the link, and current permission to change links outside .agents.\nIt adds no recovery syntax, never follows or mutates source targets, and does not restore or widen permission grants.";

    // @OpenForgeText repair.help.preview-and-safety
    internal static string HelpPreviewAndSafety()
        => "--dry-run previews the complete selected plan and writes nothing. Every eventual replacement carries the expected file state and the version this CLI ships, exact verification, and external recovery attribution.";

    // @OpenForgeText repair.help.notes
    internal static string HelpNotes()
        => "Repair is stateless and local. It preserves authored content and supplied metadata, completes affected navigation in the same plan, and never invokes another command as a subprocess. The separate cleanup command owns recovery cleanup.";

    // @OpenForgeText repair.message.review-remaining-broken-links-in-repair-then-select-a-possible-target-in-the-interactive-prompt-or-provide-an-explicit-relink-target
    internal static string MessageReviewRemainingBrokenLinksInRepairThenSelectAPossibleTargetInTheInteractivePromptOrProvideAnExplicitRelinkTarget()
        => "Review remaining broken links in Repair, then select a possible target in the interactive prompt or provide an explicit --relink target.";

    // @OpenForgeText repair.label.skip
    internal static string LeaveLinkUnchanged()
        => "Leave this link unchanged";

    // @OpenForgeText repair.title.select
    internal static string IncludeRecovery()
        => "Include";

    // @OpenForgeText repair.label.leave-recovery-out
    internal static string LeaveRecoveryOut() => "Leave out";

    // @OpenForgeText repair.message.leave-recovery-out-description
    internal static string LeaveRecoveryOutDescription() => "Leave all recovery steps out of the plan.";

    // @OpenForgeText repair.label.in-nearby-folder
    internal static string InNearbyFolder() => "in a nearby folder";

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

    // @OpenForgeText repair.repair.help.examples
    internal static string HelpExamples()
        => "open-forge repair --automatic --dry-run\nopen-forge repair\nopen-forge repair --automatic";

    // @OpenForgeText repair.repair.help.behavior
    internal static string HelpBehavior()
        => "Interactive Repair lets you choose repairs and review the writing plan.\n--automatic selects only currently verified safe corrections and skips confirmation. It never chooses a possible target for you.\nIt also fixes links after manual renames or moves when one new location is proven, and refreshes affected Entries.\n--dry-run shows the selected checked plan and writes nothing.\nRedirected or JSON requests need explicit choices or --automatic.\nInterrupted operations report recovery data for review. Repair does not roll back changes automatically.";

    // @OpenForgeText repair.repair.help.relatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("doctor --detail full", "Diagnose workspace problems."),
            ("cleanup --dry-run", "Preview recovery-data cleanup."),
            ("index --dry-run", "Fill missing metadata and update Entries."),
        ];

    // @OpenForgeText repair.repair.help.relinkcoordinates
    internal static string HelpRelinkCoordinates()
        => "A relink source location is path@line:column. Line and column are one-based, and the column is at the start of the link destination. Doctor points to the link, so its displayed location cannot be copied directly. Inspect the current source or use Repair's destination location.\nFor [Rule](../working/order-rule.md) at the start of line 7, the destination begins at column 8. The expected destination must be the exact current authored value. The new target must be an admitted reported candidate.";

    // @OpenForgeText repair.repair.help.relinkexample
    internal static string HelpRelinkExample()
        => "open-forge repair --relink \".agents/memory/crystallized/repair-note.md@7:8\" \"../working/order-rule.md\" \".agents/memory/crystallized/order-rule.md\" --dry-run";

    // @OpenForgeText repair.repair.help.helpheadingrelinkcoordinates
    internal static string HelpHeadingRelinkCoordinates()
        => "Relink coordinates";

    // @OpenForgeText repair.repair.help.helpheadingrelinkexample
    internal static string HelpHeadingRelinkExample()
        => "Worked relink example";
    // @OpenForgeText repair.help.details-1
    internal static string HelpRelinkCoordinatesDetails()
        => "Use one-based path@line:column, with the column at the start of the link destination. Doctor points to the link, so its displayed location cannot be copied directly. Inspect the current source or use Repair's destination location. For [Rule](../working/order-rule.md) at the start of line 7, use column 8. The expected destination must equal the exact authored value. The new contained target must be an admitted reported candidate.";

    // @OpenForgeText repair.help.details-2
    internal static string HelpWorkedRelinkExampleDetails()
        => "open-forge repair --relink \".agents/memory/crystallized/repair-note.md@7:8\" \"../working/order-rule.md\" \".agents/memory/crystallized/order-rule.md\" --dry-run";

    // @OpenForgeText repair.help.details-3
    internal static string HelpOtherRepairsDetails()
        => "Repair corrects path spelling, case, encoding, or unique fragments while preserving the target. Explicit relinking uses a possible target reported by Doctor. It can complete selected incomplete Library operations only with a current-v1 recovery record, matching link identity without following the link, and current permission outside .agents. No recovery syntax or permission grant is added, restored, or widened. Source targets stay untouched.";

    // @OpenForgeText repair.help.details-4
    internal static string HelpSafetyDetails()
        => "Replacements carry the expected file state, the version this CLI ships, exact verification, and external recovery attribution. Repair completes eligible metadata, missing navigation files, and Entries only in routes affected by selected corrections. Managed files, native Skill packages, overwrites, excluded or private contents, and unsafe leaves remain protected. Repair is stateless and local. It does not clean recovery artifacts or invoke another command.";
}
