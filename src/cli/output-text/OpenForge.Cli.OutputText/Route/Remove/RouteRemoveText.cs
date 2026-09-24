using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Remove;

internal static class RouteRemoveText
{
    // @OpenForgeText route.remove.message.route-remove-was-cancelled-nothing-was-changed
    internal static string MessageRouteRemoveWasCancelledNothingWasChanged()
        => "Route remove was cancelled. Nothing was changed.";

    // @OpenForgeText route.remove.message.list-the-routes-then-rerun-route-remove-with-an-exact-source-reference
    internal static string MessageListTheRoutesThenRerunRouteRemoveWithAnExactSourceReference()
        => "List the routes, then rerun Route Remove with an exact source reference.";

    // @OpenForgeText route.remove.help.reason
    internal static string HelpReason()
        => "Correct the Route Remove input, then rerun the request.";

    // @OpenForgeText route.remove.message.fix-the-reported-source-by-hand-then-rerun-route-remove
    internal static string MessageFixTheReportedSourceByHandThenRerunRouteRemove()
        => "Fix the reported source by hand, then rerun Route Remove.";

    // @OpenForgeText route.remove.message.update-the-owning-route-explicitly-then-rerun-route-remove-if-needed
    internal static string MessageUpdateTheOwningRouteExplicitlyThenRerunRouteRemoveIfNeeded()
        => "Update the owning route explicitly, then rerun Route Remove if needed.";

    // @OpenForgeText route.remove.message.remove-the-owning-extension-first-then-rerun-route-remove
    internal static string MessageRemoveTheOwningExtensionFirstThenRerunRouteRemove()
        => "Remove the owning Extension first, then rerun Route Remove.";

    // @OpenForgeText route.remove.message.wait-for-the-blocking-condition-or-inspect-the-changed-target-then-rerun-route-remove-from-a-fresh-plan
    internal static string MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteRemoveFromAFreshPlan()
        => "Wait for the blocking condition or inspect the changed target, then rerun Route Remove from a fresh plan.";

    // @OpenForgeText route.remove.message.inspect-the-reported-route-remove-boundary-before-relying-on-this-result
    internal static string MessageInspectTheReportedRouteRemoveBoundaryBeforeRelyingOnThisResult()
        => "Inspect the reported Route Remove boundary before relying on this result.";

    // @OpenForgeText route.remove.message.report-the-failure-and-retry-the-same-route-remove-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameRouteRemoveRequestWithBoundedDiagnostics()
        => "Report the failure and retry the same Route Remove request with bounded diagnostics.";

    // @OpenForgeText route.remove.message.rerun-the-same-route-remove-request
    internal static string MessageRerunTheSameRouteRemoveRequest()
        => "Rerun the same Route Remove request.";

    // @OpenForgeText route.remove.message.nothing-to-do
    internal static string MessageNothingToDo()
        => "Nothing to do.";

    // @OpenForgeText route.remove.message.nothing-to-do-for-source
    internal static string MessageNothingToDoForSource(string source)
        => string.Create(CultureInfo.InvariantCulture, $"Nothing to do for {source}.");

    // @OpenForgeText route.remove.action-title.route-remove
    internal static string ActionTitleRouteRemove()
        => "Route remove";

    // @OpenForgeText route.remove.label.remove-the-routed-source
    internal static string LabelRemoveTheRoutedSource()
        => "remove the routed source";

    // @OpenForgeText route.remove.title.route-remove
    internal static string TitleRouteRemove()
        => "Route Remove";

    // @OpenForgeText route.remove.message.rerun-route-remove-with-automatic
    internal static string MessageRerunRouteRemoveWithAutomatic()
        => "Rerun Route Remove with --automatic.";

    // @OpenForgeText route.remove.label.files-removed
    internal static string LabelFilesRemoved()
        => "files removed";

    // @OpenForgeText route.remove.label.links-detached
    internal static string LabelLinksDetached()
        => "links detached";

    // @OpenForgeText route.remove.title.source-cannot-be-removed
    internal static string TitleSourceCannotBeRemoved()
        => "Source cannot be removed";

    // @OpenForgeText route.remove.title.link-cannot-be-detached-safely
    internal static string TitleLinkCannotBeDetachedSafely()
        => "Link cannot be detached safely";

    // @OpenForgeText route.remove.title.route-remove-failed
    internal static string TitleRouteRemoveFailed()
        => "Route remove failed";

    // @OpenForgeText route.remove.title.route-remove-was-cancelled
    internal static string TitleRouteRemoveWasCancelled()
        => "Route remove was cancelled";

    // @OpenForgeText route.remove.title.removal-settings-are-unavailable
    internal static string TitleRemovalSettingsAreUnavailable()
        => "Removal settings are unavailable";

    // @OpenForgeText route.remove.title.route-target-is-protected
    internal static string TitleRouteTargetIsProtected()
        => "Route target is protected";

    // @OpenForgeText route.remove.label.remove
    internal static string LabelRemove()
        => "remove";

    // @OpenForgeText route.remove.help.syntax
    internal static string HelpSyntax()
        => "open-forge route remove <source-reference> [--dry-run] [--automatic] [global options]";

    // @OpenForgeText route.remove.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete locked and revalidated removal, persistent exclusion, ownership-release, reference-detachment, and generated-navigation plan. --dry-run previews that same plan without writing. --automatic applies the plan without asking for confirmation.";

    // @OpenForgeText route.remove.help.examples
    internal static string HelpExamples()
        => "open-forge route remove guidance/old-guide\n  open-forge route remove .agents/memory/projects/alpha/_alpha.md --dry-run\n  open-forge route remove guidance/old-guide --automatic";

    // @OpenForgeText route.remove.help.notes
    internal static string HelpNotes()
        => "Route Remove records the selected file or directory exclusion and releases its Framework or Extension content claims after verified removal. It never follows links, prompts when --automatic is omitted, or invokes another command as a subprocess.";
}
