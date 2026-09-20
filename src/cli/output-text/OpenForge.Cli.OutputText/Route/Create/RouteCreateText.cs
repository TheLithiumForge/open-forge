using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Create;

internal static class RouteCreateText
{
    // @OpenForgeText route.create.message.route-create-was-cancelled-nothing-was-changed
    internal static string MessageRouteCreateWasCancelledNothingWasChanged()
        => "Route create was cancelled. Nothing was changed.";

    // @OpenForgeText route.create.message.correct-the-route-create-input-then-rerun-the-request
    internal static string MessageCorrectTheRouteCreateInputThenRerunTheRequest()
        => "Correct the Route Create input, then rerun the request.";

    // @OpenForgeText route.create.message.find-a-routed-template-then-rerun-route-create
    internal static string MessageFindARoutedTemplateThenRerunRouteCreate()
        => "Find a routed Template, then rerun Route Create.";

    // @OpenForgeText route.create.message.create-the-missing-parent-route-then-rerun-route-create
    internal static string MessageCreateTheMissingParentRouteThenRerunRouteCreate()
        => "Create the missing parent route, then rerun Route Create.";

    // @OpenForgeText route.create.message.update-the-existing-route-explicitly-then-rerun-route-create-if-needed
    internal static string MessageUpdateTheExistingRouteExplicitlyThenRerunRouteCreateIfNeeded()
        => "Update the existing route explicitly, then rerun Route Create if needed.";

    // @OpenForgeText route.create.message.wait-for-the-blocking-condition-or-inspect-the-changed-target-then-rerun-route-create-from-a-fresh-plan
    internal static string MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteCreateFromAFreshPlan()
        => "Wait for the blocking condition or inspect the changed target, then rerun Route Create from a fresh plan.";

    // @OpenForgeText route.create.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-route-create-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteCreateResult()
        => "Review and remove the reported recovery artifact after confirming the verified Route Create result.";

    // @OpenForgeText route.create.message.report-the-failure-and-retry-the-same-route-create-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameRouteCreateRequestWithBoundedDiagnostics()
        => "Report the failure and retry the same Route Create request with bounded diagnostics.";

    // @OpenForgeText route.create.message.rerun-the-same-route-create-request
    internal static string MessageRerunTheSameRouteCreateRequest()
        => "Rerun the same Route Create request.";

    // @OpenForgeText route.create.message.the-route-create-result-did-not-contain-a-finding
    internal static string MessageTheRouteCreateResultDidNotContainAFinding()
        => "The Route Create result did not contain a finding.";

    // @OpenForgeText route.create.label.create-the-routed-file
    internal static string LabelCreateTheRoutedFile()
        => "create the routed file";

    // @OpenForgeText route.create.title.route-create
    internal static string TitleRouteCreate()
        => "Route create";

    // @OpenForgeText route.create.message.inspect-the-blocked-route-create-boundary-before-rerunning-the-request
    internal static string MessageInspectTheBlockedRouteCreateBoundaryBeforeRerunningTheRequest()
        => "Inspect the blocked Route Create boundary before rerunning the request.";

    // @OpenForgeText route.create.label.the-target-folder
    internal static string LabelTheTargetFolder()
        => "the target folder";

    // @OpenForgeText route.create.title.existing-target-differs
    internal static string TitleExistingTargetDiffers()
        => "Existing target differs";

    // @OpenForgeText route.create.title.parent-route-is-missing
    internal static string TitleParentRouteIsMissing()
        => "Parent route is missing";

    // @OpenForgeText route.create.title.route-create-failed
    internal static string TitleRouteCreateFailed()
        => "Route create failed";

    // @OpenForgeText route.create.title.route-create-was-cancelled
    internal static string TitleRouteCreateWasCancelled()
        => "Route create was cancelled";

    // @OpenForgeText route.create.help.syntax
    internal static string HelpSyntax()
        => "open-forge route create <file-target> [--description <text>] [--tag <tag>...] [--responsibility <text>] [--template <template-reference>] [--dry-run] [global options]";

    // @OpenForgeText route.create.help.target
    internal static string HelpTarget()
        => "<file-target> selects one ordinary Markdown source ID or exact .agents path below exactly one existing routable root. Missing intermediate route directories and entrypoints are created beneath that root; unknown roots are refused.";

    // @OpenForgeText route.create.help.metadata
    internal static string HelpMetadata()
        => "--description <text>, ordered --tag <tag> values, and --responsibility <text> are optional. Omitting description or tags creates the route with a warning and a next action to add them when useful. Supply --description, --responsibility, and --template at most once each. Repeat --tag for more tags; exact duplicate tags are invalid.";

    // @OpenForgeText route.create.help.template
    internal static string HelpTemplate()
        => "--template <template-reference> copies only the body of one exact routed Markdown source tagged Template. The destination keeps its explicit metadata and no continuing Template relationship.";

    // @OpenForgeText route.create.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete destination and generated-navigation plan. --dry-run previews that same plan without writing files.";

    // @OpenForgeText route.create.help.examples
    internal static string HelpExamples()
        => "open-forge route create memory/project-alpha/overview --description \"Project overview\" --tag Docs\n  open-forge route create .agents/memory/project-alpha/overview.md --description \"Project overview\" --tag Docs --template templates/route --dry-run";

    // @OpenForgeText route.create.help.notes
    internal static string HelpNotes()
        => "Route Create does not initialize a new root, overwrite differing content, create lifecycle ownership, infer metadata, or invoke Index as a subprocess. It may create missing intermediate route directories and canonical entrypoints below an existing root.";
}
