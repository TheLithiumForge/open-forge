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
        => "Address the reported failure, then retry the same Route Remove request.";

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
        => "open-forge route remove <source-reference> [options]";

    // @OpenForgeText route.remove.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete removal plan while holding the workspace lock and rechecking the current files. The plan removes the subject, records exclusions, releases matching managed claims, detaches supported incoming references, and updates generated Entries navigation. --dry-run previews that same plan without writing. --automatic applies it without confirmation.";

    // @OpenForgeText route.remove.help.examples
    internal static string HelpExamples()
        => "open-forge route remove guidance --dry-run\nopen-forge route remove guidance\nopen-forge route remove guidance --automatic";

    // @OpenForgeText route.remove.help.notes
    internal static string HelpNotes()
        => "Route Remove records the selected file or directory exclusion and releases its Framework or Extension content claims after verified removal. It does not follow links or run another command. An interactive writing plan asks for confirmation unless --automatic is supplied. Redirected or JSON requests that would write need --automatic.";

    // @OpenForgeText route.remove.routeremove.help.behavior
    internal static string HelpBehavior()
        => "--dry-run shows the complete plan and writes nothing.\nA category selects its whole routed category, not just the entrypoint.\nFramework- and Extension-managed content can be removed when safety checks allow it.\nOpen Forge records the removal so updates do not restore it.\nAn interactive writing plan asks for confirmation unless --automatic is supplied.\nRedirected or JSON requests that would write need --automatic.\nInterrupted removals report retained recovery data for review.";

    // @OpenForgeText route.remove.routeremove.help.relatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("remove --help", "Choose one exact removal target."),
            ("references --help", "Inspect authored links."),
            ("cleanup --dry-run", "Preview recovery-data cleanup."),
        ];

    // @OpenForgeText route.remove.routeremove.help.source
    internal static string HelpSource()
        => "Select one ordinary routed Markdown file by exact ID, base path, or overwrite path, or one complete category by its recognized entrypoint. Framework- and Extension-managed content can be removed when safety checks allow it.\nThe Loader and .agents root are protected. A leaf cannot be a native Skill, non-Markdown resource, or orphan or ambiguous overwrite. A category includes all contained regular files and folders, including native and binary resources. Links, special files, incomplete scans, unsafe references, and nested .git repositories block the whole removal.\nSupported incoming links outside the removed subject are replaced with their visible labels. Removal records exclusions and releases managed claims after verification. To restore content, clear all covering exclusions in .agents/open-forge.json and explicitly run the relevant installer or updater.";
    // @OpenForgeText route.remove.help.details-1
    internal static string HelpSafetyDetails()
        => "Select a Markdown file by exact ID, base path, or overwrite path, or a category by its recognized entrypoint. The Loader and .agents root are protected. A leaf cannot be a native Skill, non-Markdown resource, or orphan or ambiguous overwrite. Categories include all contained regular files and folders, including native and binary resources. Links, special files, incomplete scans, unsafe references, or nested .git repositories block the whole removal.";

    // @OpenForgeText route.remove.help.details-2
    internal static string HelpReferencesAndRestorationDetails()
        => "Supported incoming links outside the subject become their visible labels. Apply locks the workspace, rechecks files, releases matching managed claims after verification, and updates generated Entries. Removal records file or directory exclusions. To restore content, clear every covering exclusion in .agents/open-forge.json and explicitly run the relevant installer or updater. Remove never follows links or invokes another command.";
}
