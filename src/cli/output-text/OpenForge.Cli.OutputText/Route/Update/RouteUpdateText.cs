using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Update;

internal static class RouteUpdateText
{
    // @OpenForgeText route.update.message.route-update-was-cancelled-nothing-was-changed
    internal static string MessageRouteUpdateWasCancelledNothingWasChanged()
        => "Route update was cancelled. Nothing was changed.";

    // @OpenForgeText route.update.heading.frontmatter-before
    internal static string HeadingFrontmatterBefore()
        => "Frontmatter before:";

    // @OpenForgeText route.update.heading.frontmatter-after
    internal static string HeadingFrontmatterAfter()
        => "Frontmatter after:";

    // @OpenForgeText route.update.message.nothing-to-update-pass-description-responsibility-tag-or-template
    internal static string MessageNothingToUpdatePassDescriptionResponsibilityTagOrTemplate()
        => "Nothing to update: pass --description, --responsibility, --tag, --apply-to, --clear-apply-to or --template.";

    // @OpenForgeText route.update.message.correct-the-named-route-update-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedRouteUpdateInputThenRerunTheRequest()
        => "Correct the named Route Update input, then rerun the request.";

    // @OpenForgeText route.update.message.choose-an-exact-route-then-rerun-route-update
    internal static string MessageChooseAnExactRouteThenRerunRouteUpdate()
        => "Choose an exact route, then rerun Route Update.";

    // @OpenForgeText route.update.message.find-a-routed-template-then-rerun-route-update
    internal static string MessageFindARoutedTemplateThenRerunRouteUpdate()
        => "Find a routed Template, then rerun Route Update.";

    // @OpenForgeText route.update.message.wait-for-the-blocking-condition-or-inspect-the-changed-target-then-rerun-route-update-from-a-fresh-plan
    internal static string MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteUpdateFromAFreshPlan()
        => "Wait for the blocking condition or inspect the changed target, then rerun Route Update from a fresh plan.";

    // @OpenForgeText route.update.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-route-update-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteUpdateResult()
        => "Review and remove the reported recovery artifact after confirming the verified Route Update result.";

    // @OpenForgeText route.update.message.report-the-failure-and-retry-the-same-route-update-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameRouteUpdateRequestWithBoundedDiagnostics()
        => "Address the reported failure, then retry the same Route Update request.";

    // @OpenForgeText route.update.message.rerun-the-same-route-update-request
    internal static string MessageRerunTheSameRouteUpdateRequest()
        => "Rerun the same Route Update request.";

    // @OpenForgeText route.update.label.fields-changed
    internal static string LabelFieldsChanged()
        => "fields changed";

    // @OpenForgeText route.update.message.the-route-update-result-did-not-contain-a-finding
    internal static string MessageTheRouteUpdateResultDidNotContainAFinding()
        => "The Route Update result did not contain a finding.";

    // @OpenForgeText route.update.title.route-update
    internal static string TitleRouteUpdate()
        => "Route update";

    // @OpenForgeText route.update.message.tag-value-is-repeated
    internal static string MessageTagValueIsRepeated()
        => "--tag <value> is repeated.";

    // @OpenForgeText route.update.message.description-must-not-be-blank
    internal static string MessageDescriptionMustNotBeBlank()
        => "--description must not be blank.";

    // @OpenForgeText route.update.message.choose-a-routed-source-then-rerun-route-update
    internal static string MessageChooseARoutedSourceThenRerunRouteUpdate()
        => "Choose a routed source, then rerun Route Update.";

    // @OpenForgeText route.update.title.source-cannot-be-updated
    internal static string TitleSourceCannotBeUpdated()
        => "Source cannot be updated";

    // @OpenForgeText route.update.title.invalid-update
    internal static string TitleInvalidUpdate()
        => "Invalid update";

    // @OpenForgeText route.update.title.frontmatter-is-unsafe
    internal static string TitleFrontmatterIsUnsafe()
        => "Frontmatter is unsafe";

    // @OpenForgeText route.update.title.frontmatter-cannot-be-preserved
    internal static string TitleFrontmatterCannotBePreserved()
        => "Frontmatter cannot be preserved";

    // @OpenForgeText route.update.title.template-body-was-not-copied
    internal static string TitleTemplateBodyWasNotCopied()
        => "Template body was not copied";

    // @OpenForgeText route.update.title.route-update-failed
    internal static string TitleRouteUpdateFailed()
        => "Route update failed";

    // @OpenForgeText route.update.title.route-update-was-cancelled
    internal static string TitleRouteUpdateWasCancelled()
        => "Route update was cancelled";

    // @OpenForgeText route.update.placeholder.none
    internal static string PlaceholderNone()
        => "(none)";

    // @OpenForgeText route.update.placeholder.removed
    internal static string PlaceholderRemoved()
        => "(removed)";

    // @OpenForgeText route.update.help.syntax
    internal static string HelpSyntax()
        => "open-forge route update <source-reference> [options]";

    // @OpenForgeText route.update.help.target
    internal static string HelpTarget()
        => "<source-reference> selects one existing ordinary routed Markdown source by exact ID, base path, or adjacent overwrite path.\n  Route Update preserves its ID, base path, document form, and overwrite ownership.";

    // @OpenForgeText route.update.help.metadata
    internal static string HelpMetadata()
        => "Supply at least one metadata or Template operation. Description, responsibility, and Template values must each be supplied only once.\n  Repeated --tag <tag> values form one ordered replacement list. Empty or duplicate tags are invalid.\n  Repeated --apply-to <glob> values replace the complete local applyTo list, and --clear-apply-to removes local declarations. These options cannot be combined.\n  Patterns match from the workspace root and are case-sensitive. *.py matches root files, **/*.py any depth, and docs/** a folder. A leading ! is literal and does not exclude files.\n  An exact empty responsibility removes that field, while whitespace-only text is invalid.";

    // @OpenForgeText route.update.help.template
    internal static string HelpTemplate()
        => "--template <template-reference> copies only the body of one exact routed Markdown source tagged Template when the target body is empty or whitespace.\n  The target's authored body content is protected. Target metadata and identity remain unchanged.";

    // @OpenForgeText route.update.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete locked and revalidated target and generated-navigation plan.\n  --dry-run previews that same plan without writing files. A verified no-op writes nothing and creates no recovery bundle.";

    // @OpenForgeText route.update.help.examples
    internal static string HelpExamples()
        => "open-forge route update memory/note --description \"Note\" --dry-run\nopen-forge route update memory/note --description \"Note\"\nopen-forge route update memory/note --clear-apply-to --dry-run";

    // @OpenForgeText route.update.help.notes
    internal static string HelpNotes()
        => "Global workspace, format, detail, detail-filter, help, and version options retain their shared meaning.";

    // @OpenForgeText route.update.routeupdate.help.behavior
    internal static string HelpBehavior()
        => "Change only the metadata or Template content you request. Other content stays.\n--dry-run shows the checked plan and writes nothing.\nA verified no-op writes nothing and creates no recovery bundle.\nThis command asks no questions. Interrupted writes report retained recovery data.";

    // @OpenForgeText route.update.routeupdate.help.relatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("route inspect --help", "Inspect one routed source."),
            ("route create --help", "Create a routed Markdown file."),
            ("index --dry-run", "Fill missing metadata and update Entries."),
        ];
    // @OpenForgeText route.update.help.details-1
    internal static string HelpTargetAndMetadataDetails()
        => "Use one existing ordinary source by exact ID, base path, or adjacent overwrite path. ID, base path, document form, and overwrite ownership stay. Request at least one metadata or Template operation. Supply description, responsibility, and Template at most once. Repeated tags replace the ordered list. Empty or duplicate tags are invalid. Repeated --apply-to replaces the whole local list. It cannot accompany --clear-apply-to. An exact empty responsibility removes the field. Whitespace alone is invalid.";

    // @OpenForgeText route.update.help.details-2
    internal static string HelpGlobsAndTemplatesDetails()
        => "Globs are case-sensitive and workspace-relative: *.py at root, **/*.py at any depth, docs/** below docs. ! is literal. --template copies only the body of one exact routed Markdown source tagged Template into an empty or whitespace-only body. Authored target content is protected.";

    // @OpenForgeText route.update.help.details-3
    internal static string HelpWriteBoundaryDetails()
        => "Apply locks the workspace and revalidates the target and generated-navigation plan.";
}
