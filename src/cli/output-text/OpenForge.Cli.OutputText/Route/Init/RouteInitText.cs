using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Init;

internal static class RouteInitText
{
    // @OpenForgeText route.init.message.route-init-was-cancelled-nothing-was-changed
    internal static string MessageRouteInitWasCancelledNothingWasChanged()
        => "Route init was cancelled. Nothing was changed.";

    // @OpenForgeText route.init.message.its-description-and-tags-are-placeholders-edit-them-before-relying-on-this-route
    internal static string MessageItsDescriptionAndTagsArePlaceholdersEditThemBeforeRelyingOnThisRoute()
        => "Its description and tags are placeholders. Edit them before relying on this route.";

    // @OpenForgeText route.init.message.correct-the-named-route-init-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedRouteInitInputThenRerunTheRequest()
        => "Correct the named Route Init input, then rerun the request.";

    // @OpenForgeText route.init.message.establish-a-trusted-current-framework-installation-before-rerunning-route-init-in-framework-mode
    internal static string MessageEstablishATrustedCurrentFrameworkInstallationBeforeRerunningRouteInitInFrameworkMode()
        => "Establish a trusted current Framework installation before rerunning Route Init in Framework mode.";

    // @OpenForgeText route.init.message.update-the-installed-framework-state-to-the-running-cli-s-embedded-inventory-before-rerunning-route-init
    internal static string MessageUpdateTheInstalledFrameworkStateToTheRunningCliSEmbeddedInventoryBeforeRerunningRouteInit()
        => "Update the installed Framework state to the running CLI's embedded inventory before rerunning Route Init.";

    // @OpenForgeText route.init.message.wait-for-the-blocking-condition-or-inspect-the-changed-target-then-rerun-route-init-from-a-fresh-plan
    internal static string MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteInitFromAFreshPlan()
        => "Wait for the blocking condition or inspect the changed target, then rerun Route Init from a fresh plan.";

    // @OpenForgeText route.init.message.inspect-the-blocked-route-init-boundary-before-rerunning-the-request
    internal static string MessageInspectTheBlockedRouteInitBoundaryBeforeRerunningTheRequest()
        => "Inspect the blocked Route Init boundary before rerunning the request.";

    // @OpenForgeText route.init.message.inspect-the-unavailable-route-metadata-projection-lifecycle-or-recovery-facts-before-relying-on-this-route-init-result
    internal static string MessageInspectTheUnavailableRouteMetadataProjectionLifecycleOrRecoveryFactsBeforeRelyingOnThisRouteInitResult()
        => "Inspect the unavailable route, metadata, projection, lifecycle, or recovery facts before relying on this Route Init result.";

    // @OpenForgeText route.init.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-route-init-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteInitResult()
        => "Review and remove the reported recovery artifact after confirming the verified Route Init result.";

    // @OpenForgeText route.init.message.report-the-failure-and-retry-the-same-route-init-request-with-bounded-diagnostics
    internal static string MessageReportTheFailureAndRetryTheSameRouteInitRequestWithBoundedDiagnostics()
        => "Report the failure and retry the same Route Init request with bounded diagnostics.";

    // @OpenForgeText route.init.message.rerun-the-same-route-init-request
    internal static string MessageRerunTheSameRouteInitRequest()
        => "Rerun the same Route Init request.";

    // @OpenForgeText route.init.label.entrypoints-created
    internal static string LabelEntrypointsCreated()
        => "entrypoints created";

    // @OpenForgeText route.init.label.entrypoints-present
    internal static string LabelEntrypointsPresent()
        => "entrypoints present";

    // @OpenForgeText route.init.label.already-present
    internal static string LabelAlreadyPresent()
        => "already present";

    // @OpenForgeText route.init.label.would-create
    internal static string LabelWouldCreate()
        => "would create";

    // @OpenForgeText route.init.label.not-a-route-id-or-an-entrypoint-path-under-agents
    internal static string LabelNotARouteIdOrAnEntrypointPathUnderAgents()
        => "not a route ID or an entrypoint path under .agents";

    // @OpenForgeText route.init.label.the-route-init-result-did-not-contain-a-finding
    internal static string LabelTheRouteInitResultDidNotContainAFinding()
        => "the Route Init result did not contain a finding";

    // @OpenForgeText route.init.label.agents-open-forge-lock-json-would-update
    internal static string LabelAgentsOpenForgeLockJsonWouldUpdate()
        => ".agents/open-forge.lock.json  would update";

    // @OpenForgeText route.init.label.agents-open-forge-lock-json-updated
    internal static string LabelAgentsOpenForgeLockJsonUpdated()
        => ".agents/open-forge.lock.json  updated";

    // @OpenForgeText route.init.message.recovery-no-recovery-bundle-was-needed
    internal static string MessageRecoveryNoRecoveryBundleWasNeeded()
        => "Recovery: no recovery bundle was needed.";

    // @OpenForgeText route.init.message.recovery-no-recovery-bundle-was-created
    internal static string MessageRecoveryNoRecoveryBundleWasCreated()
        => "Recovery: no recovery bundle was created.";

    // @OpenForgeText route.init.message.recovery-the-recovery-bundle-was-removed
    internal static string MessageRecoveryTheRecoveryBundleWasRemoved()
        => "Recovery: the recovery bundle was removed.";

    // @OpenForgeText route.init.message.recovery-the-recovery-bundle-was-retained
    internal static string MessageRecoveryTheRecoveryBundleWasRetained()
        => "Recovery: the recovery bundle was retained.";

    // @OpenForgeText route.init.message.recovery-the-final-recovery-state-is-unknown
    internal static string MessageRecoveryTheFinalRecoveryStateIsUnknown()
        => "Recovery: the final recovery state is unknown.";

    // @OpenForgeText route.init.message.the-loader-cannot-be-initialized
    internal static string MessageTheLoaderCannotBeInitialized()
        => "The Loader cannot be initialized.";

    // @OpenForgeText route.init.message.route-init-metadata-is-invalid
    internal static string MessageRouteInitMetadataIsInvalid()
        => "Route Init metadata is invalid.";

    // @OpenForgeText route.init.title.loader-is-unsafe
    internal static string TitleLoaderIsUnsafe()
        => "Loader is unsafe";

    // @OpenForgeText route.init.title.framework-installation-is-required
    internal static string TitleFrameworkInstallationIsRequired()
        => "Framework installation is required";

    // @OpenForgeText route.init.title.framework-update-is-required
    internal static string TitleFrameworkUpdateIsRequired()
        => "Framework update is required";

    // @OpenForgeText route.init.title.framework-alignment-is-blocked
    internal static string TitleFrameworkAlignmentIsBlocked()
        => "Framework alignment is blocked";

    // @OpenForgeText route.init.title.ownership-record-is-blocked
    internal static string TitleOwnershipRecordIsBlocked()
        => "Ownership record is blocked";

    // @OpenForgeText route.init.title.route-needs-authoring
    internal static string TitleRouteNeedsAuthoring()
        => "Route needs authoring";

    // @OpenForgeText route.init.title.ownership-record-could-not-be-written
    internal static string TitleOwnershipRecordCouldNotBeWritten()
        => "Ownership record could not be written";

    // @OpenForgeText route.init.title.route-init-failed
    internal static string TitleRouteInitFailed()
        => "Route Init failed";

    // @OpenForgeText route.init.title.route-init-was-cancelled
    internal static string TitleRouteInitWasCancelled()
        => "Route Init was cancelled";

    // @OpenForgeText route.init.label.initialize-the-route
    internal static string LabelInitializeTheRoute()
        => "initialize the route";

    // @OpenForgeText route.init.message.agents-loader-md-could-not-be-verified-safely
    internal static string MessageAgentsLoaderMdCouldNotBeVerifiedSafely()
        => ".agents/loader.md could not be verified safely.";

    // @OpenForgeText route.init.message.the-framework-scaffold-needs-an-installed-framework
    internal static string MessageTheFrameworkScaffoldNeedsAnInstalledFramework()
        => "The Framework scaffold needs an installed Framework.";

    // @OpenForgeText route.init.message.the-installed-framework-is-older-than-the-one-this-cli-ships
    internal static string MessageTheInstalledFrameworkIsOlderThanTheOneThisCliShips()
        => "The installed Framework is older than the one this CLI ships.";

    // @OpenForgeText route.init.message.the-installed-framework-does-not-match-the-version-this-cli-ships-so-the-framework-scaffold-cannot-be-used
    internal static string MessageTheInstalledFrameworkDoesNotMatchTheVersionThisCliShipsSoTheFrameworkScaffoldCannotBeUsed()
        => "The installed Framework does not match the version this CLI ships, so the Framework scaffold cannot be used.";

    // @OpenForgeText route.init.title.route-init
    internal static string TitleRouteInit()
        => "Route Init";

    // @OpenForgeText route.init.help.syntax
    internal static string HelpSyntax()
        => "open-forge route init <route-target> [--framework] [--description <text>] [--responsibility <text>] [--tag <tag>]... [--dry-run] [global options]";

    // @OpenForgeText route.init.help.target
    internal static string HelpTarget()
        => "<route-target> selects one exact route ID or .agents entrypoint path. A missing exact-path target must use the canonical entrypoint filename. Route Init creates missing entrypoints in that exact chain and never creates the Loader.";

    // @OpenForgeText route.init.help.heading.scaffold-mode
    internal static string HelpHeadingScaffoldMode()
        => "Scaffold mode";

    // @OpenForgeText route.init.help.scaffold-mode
    internal static string HelpScaffoldMode()
        => "Generic mode uses the fixed draft scaffold. --framework uses the trusted embedded Framework topology and managed entrypoint assets. Repeating --framework has no additional effect.";

    // @OpenForgeText route.init.help.metadata
    internal static string HelpMetadata()
        => "--description <text>, --responsibility <text>, and ordered repeated --tag <tag> values apply only to a missing generic final target. Supply description and responsibility only once; empty or duplicate tags are invalid.";

    // @OpenForgeText route.init.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply the complete checked plan. --dry-run previews the same directories, entrypoints, generated-region effects, and bounded changes without writing files. Repeating the command makes no further changes.";

    // @OpenForgeText route.init.help.examples
    internal static string HelpExamples()
        => "open-forge route init memory/project-alpha/documents\n  open-forge route init memory/project-alpha/documents --description \"Project documents\" --tag Memory\n  open-forge route init \"memory/Mobile App/crystallized/documents\" --framework --dry-run\n  open-forge route init .agents/memory/project-alpha/documents/_documents.md --format json";

    // @OpenForgeText route.init.help.related-commands
    internal static string HelpRelatedCommands()
        => "open-forge route update — author an existing entrypoint.\n  open-forge doctor — inspect blocked or unavailable route and safety facts.\n  open-forge cleanup — remove a reported retained recovery artifact after review.";

    // @OpenForgeText route.init.help.notes
    internal static string HelpNotes()
        => "Route Init does not instantiate a Template, infer route meaning, rewrite authored content, normalize compatibility filenames, repair malformed generated regions, manipulate Git, or create commits.";
}
