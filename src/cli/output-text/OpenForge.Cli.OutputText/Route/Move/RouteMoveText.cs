using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Move;

internal static class RouteMoveText
{
    // @OpenForgeText route.move.message.route-move-was-cancelled-nothing-was-changed
    internal static string MessageRouteMoveWasCancelledNothingWasChanged()
        => "Route move was cancelled. Nothing was changed.";

    // @OpenForgeText route.move.label.link-source
    internal static string LabelLinkSource()
        => "link source";

    // @OpenForgeText route.move.title.entries-section
    internal static string TitleEntriesSection()
        => "Entries section";

    // @OpenForgeText route.move.message.the-source-and-the-destination-are-the-same
    internal static string MessageTheSourceAndTheDestinationAreTheSame()
        => "The source and the destination are the same.";

    // @OpenForgeText route.move.message.the-destination-is-inside-the-folder-being-moved
    internal static string MessageTheDestinationIsInsideTheFolderBeingMoved()
        => "The destination is inside the folder being moved.";

    // @OpenForgeText route.move.label.child-metadata-could-not-be-read
    internal static string LabelChildMetadataCouldNotBeRead()
        => "child metadata could not be read";

    // @OpenForgeText route.move.action-title.route-move
    internal static string ActionTitleRouteMove()
        => "Route move";

    // @OpenForgeText route.move.message.correct-the-named-route-move-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedRouteMoveInputThenRerunTheRequest()
        => "Correct the named Route Move input, then rerun the request.";

    // @OpenForgeText route.move.message.list-all-routes-then-rerun-route-move-with-an-exact-source
    internal static string MessageListAllRoutesThenRerunRouteMoveWithAnExactSource()
        => "List all routes, then rerun Route Move with an exact source.";

    // @OpenForgeText route.move.label.choose-another-destination
    internal static string LabelChooseAnotherDestination()
        => "choose another destination";

    // @OpenForgeText route.move.message.choose-another-destination-then-rerun-route-move
    internal static string MessageChooseAnotherDestinationThenRerunRouteMove()
        => "Choose another destination, then rerun Route Move.";

    // @OpenForgeText route.move.message.initialize-the-exact-missing-destination-parent-route-then-rerun-route-move
    internal static string MessageInitializeTheExactMissingDestinationParentRouteThenRerunRouteMove()
        => "Initialize the exact missing destination parent route, then rerun Route Move.";

    // @OpenForgeText route.move.message.fix-the-named-boundary-by-hand-then-rerun-route-move
    internal static string MessageFixTheNamedBoundaryByHandThenRerunRouteMove()
        => "Fix the named boundary by hand, then rerun Route Move.";

    // @OpenForgeText route.move.message.reconcile-the-managed-source-before-rerunning-route-move
    internal static string MessageReconcileTheManagedSourceBeforeRerunningRouteMove()
        => "Reconcile the managed source before rerunning Route Move.";

    // @OpenForgeText route.move.message.update-the-owning-extension-before-rerunning-route-move
    internal static string MessageUpdateTheOwningExtensionBeforeRerunningRouteMove()
        => "Update the owning Extension before rerunning Route Move.";

    // @OpenForgeText route.move.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-route-move-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteMoveResult()
        => "Review and remove the reported recovery artifact after confirming the verified Route Move result.";

    // @OpenForgeText route.move.message.inspect-the-unavailable-inventory-reference-projection-or-recovery-facts-before-relying-on-this-route-move-result
    internal static string MessageInspectTheUnavailableInventoryReferenceProjectionOrRecoveryFactsBeforeRelyingOnThisRouteMoveResult()
        => "Inspect the unavailable inventory, reference, projection, or recovery facts before relying on this Route Move result.";

    // @OpenForgeText route.move.message.wait-for-the-blocking-condition-or-inspect-the-changed-target-then-rerun-route-move-from-a-fresh-plan
    internal static string MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteMoveFromAFreshPlan()
        => "Wait for the blocking condition or inspect the changed target, then rerun Route Move from a fresh plan.";

    // @OpenForgeText route.move.message.inspect-the-blocked-workspace-route-ownership-reference-generated-region-destination-or-recovery-boundary-before-rerunning-route-move
    internal static string MessageInspectTheBlockedWorkspaceRouteOwnershipReferenceGeneratedRegionDestinationOrRecoveryBoundaryBeforeRerunningRouteMove()
        => "Inspect the blocked workspace, route, ownership, reference, generated-region, destination, or recovery boundary before rerunning Route Move.";

    // @OpenForgeText route.move.message.report-the-failure-and-retry-the-same-route-move-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameRouteMoveRequestWithBoundedDiagnostics()
        => "Report the failure and retry the same Route Move request with bounded diagnostics.";

    // @OpenForgeText route.move.message.rerun-the-same-route-move-request
    internal static string MessageRerunTheSameRouteMoveRequest()
        => "Rerun the same Route Move request.";

    // @OpenForgeText route.move.label.files-moved
    internal static string LabelFilesMoved()
        => "files moved";

    // @OpenForgeText route.move.label.links-rewritten
    internal static string LabelLinksRewritten()
        => "links rewritten";

    // @OpenForgeText route.move.label.the-reference-scan-was-not-completed
    internal static string LabelTheReferenceScanWasNotCompleted()
        => "the reference scan was not completed";

    // @OpenForgeText route.move.label.a-link-leaves-it
    internal static string LabelALinkLeavesIt()
        => "a link leaves it";

    // @OpenForgeText route.move.label.its-identity-is-ambiguous
    internal static string LabelItsIdentityIsAmbiguous()
        => "its identity is ambiguous";

    // @OpenForgeText route.move.label.it-is-missing
    internal static string LabelItIsMissing()
        => "it is missing";

    // @OpenForgeText route.move.label.there-is-more-than-one
    internal static string LabelThereIsMoreThanOne()
        => "there is more than one";

    // @OpenForgeText route.move.label.it-is-malformed
    internal static string LabelItIsMalformed()
        => "it is malformed";

    // @OpenForgeText route.move.label.another-manager
    internal static string LabelAnotherManager()
        => "another manager";

    // @OpenForgeText route.move.title.route-move
    internal static string TitleRouteMove()
        => "Route Move";

    // @OpenForgeText route.move.title.invalid-subject
    internal static string TitleInvalidSubject()
        => "Invalid subject";

    // @OpenForgeText route.move.title.invalid-destination
    internal static string TitleInvalidDestination()
        => "Invalid destination";

    // @OpenForgeText route.move.title.ownership-could-not-be-read
    internal static string TitleOwnershipCouldNotBeRead()
        => "Ownership could not be read";

    // @OpenForgeText route.move.title.destination-is-unsafe
    internal static string TitleDestinationIsUnsafe()
        => "Destination is unsafe";

    // @OpenForgeText route.move.title.destination-parent-is-missing
    internal static string TitleDestinationParentIsMissing()
        => "Destination parent is missing";

    // @OpenForgeText route.move.title.destination-already-exists
    internal static string TitleDestinationAlreadyExists()
        => "Destination already exists";

    // @OpenForgeText route.move.title.source-and-destination-are-the-same
    internal static string TitleSourceAndDestinationAreTheSame()
        => "Source and destination are the same";

    // @OpenForgeText route.move.title.destination-is-inside-source
    internal static string TitleDestinationIsInsideSource()
        => "Destination is inside source";

    // @OpenForgeText route.move.title.reference-could-not-be-rewritten
    internal static string TitleReferenceCouldNotBeRewritten()
        => "Reference could not be rewritten";

    // @OpenForgeText route.move.title.route-move-failed
    internal static string TitleRouteMoveFailed()
        => "Route move failed";

    // @OpenForgeText route.move.title.route-move-was-cancelled
    internal static string TitleRouteMoveWasCancelled()
        => "Route move was cancelled";

    // @OpenForgeText route.move.heading.effects
    internal static string HeadingEffects()
        => "Effects:";

    // @OpenForgeText route.move.label.rewrite
    internal static string LabelRewrite()
        => "rewrite";

    // @OpenForgeText route.move.label.planned
    internal static string LabelPlanned()
        => "planned";

    // @OpenForgeText route.move.label.move
    internal static string LabelMove()
        => "move";

    // @OpenForgeText route.move.help.syntax
    internal static string HelpSyntax()
        => "open-forge route move <source-reference> <destination-target> [--dry-run] [global options]";

    // @OpenForgeText route.move.help.heading.destination
    internal static string HelpHeadingDestination()
        => "Destination";

    // @OpenForgeText route.move.help.destination
    internal static string HelpDestination()
        => "<destination-target> is one exact workspace-relative .agents path below an existing routable parent. Leaf and category destination forms must match the selected subject.";

    // @OpenForgeText route.move.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete locked and revalidated move, reference, and generated-navigation plan. --dry-run previews that same plan without writing files.";

    // @OpenForgeText route.move.help.examples
    internal static string HelpExamples()
        => "open-forge route move guidance/old-guide .agents/archive/new-guide.md\n  open-forge route move .agents/guidance/topics/_topics.md .agents/archive/topics/_topics.md --dry-run";

    // @OpenForgeText route.move.help.notes
    internal static string HelpNotes()
        => "Route Move never moves lifecycle-managed content, initializes a missing parent route, overwrites a destination, prompts, or invokes Index as a subprocess.";
}
