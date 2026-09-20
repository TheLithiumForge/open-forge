using System.Globalization;

namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorText
{
    // @OpenForgeText doctor.help.syntax
    internal static string HelpSyntax()
        => "open-forge doctor [global options]";

    // @OpenForgeText doctor.help.inspection
    internal static string HelpInspection()
        => "Check the workspace, recovery data, routes, references, managed Framework files, and managed Extensions without changing them.";

    // @OpenForgeText doctor.help.examples
    internal static string HelpExamples()
        => "open-forge doctor --detail standard\n  open-forge doctor --format json --detail full";

    // @OpenForgeText doctor.message.doctor-was-cancelled
    internal static string MessageDoctorWasCancelled()
        => "Doctor was cancelled.";

    // @OpenForgeText doctor.label.checks
    internal static string LabelChecks()
        => "checks";

    // @OpenForgeText doctor.label.checks-complete
    internal static string LabelChecksComplete()
        => "checks complete";

    // @OpenForgeText doctor.label.links-checked
    internal static string LabelLinksChecked()
        => "links checked";

    // @OpenForgeText doctor.label.links-valid
    internal static string LabelLinksValid()
        => "links valid";

    // @OpenForgeText doctor.label.external-links-not-checked
    internal static string LabelExternalLinksNotChecked()
        => "external links not checked";

    // @OpenForgeText doctor.label.image-links
    internal static string LabelImageLinks()
        => "image links";

    // @OpenForgeText doctor.label.routes-checked
    internal static string LabelRoutesChecked()
        => "routes checked";

    // @OpenForgeText doctor.title.extensions-installed
    internal static string TitleExtensionsInstalled()
        => "Extensions installed";

    // @OpenForgeText doctor.title.libraries-registered
    internal static string TitleLibrariesRegistered()
        => "Libraries registered";

    // @OpenForgeText doctor.message.review-the-findings-and-choose-a-repair
    internal static string MessageReviewTheFindingsAndChooseARepair()
        => "Review the findings and choose a repair.";

    // @OpenForgeText doctor.message.edit-the-reported-file-by-hand
    internal static string MessageEditTheReportedFileByHand()
        => "Edit the reported file by hand.";

    // @OpenForgeText doctor.title.routes-and-entries
    internal static string TitleRoutesAndEntries()
        => "Routes and Entries";

    // @OpenForgeText doctor.label.workspace-files
    internal static string LabelWorkspaceFiles()
        => "workspace files";

    // @OpenForgeText doctor.label.recovery-records
    internal static string LabelRecoveryRecords()
        => "recovery records";

    // @OpenForgeText doctor.label.route-scan
    internal static string LabelRouteScan()
        => "route scan";

    // @OpenForgeText doctor.label.route-metadata
    internal static string LabelRouteMetadata()
        => "route metadata";

    // @OpenForgeText doctor.label.generated-navigation
    internal static string LabelGeneratedNavigation()
        => "generated navigation";

    // @OpenForgeText doctor.label.local-links
    internal static string LabelLocalLinks()
        => "local links";

    // @OpenForgeText doctor.title.framework-installation-record
    internal static string TitleFrameworkInstallationRecord()
        => "Framework installation record";

    // @OpenForgeText doctor.label.distributed-framework-content
    internal static string LabelDistributedFrameworkContent()
        => "distributed Framework content";

    // @OpenForgeText doctor.title.extension-installation-record
    internal static string TitleExtensionInstallationRecord()
        => "Extension installation record";

    // @OpenForgeText doctor.title.extension-source
    internal static string TitleExtensionSource()
        => "Extension source";

    // @OpenForgeText doctor.label.file-ownership-records
    internal static string LabelFileOwnershipRecords()
        => "file ownership records";

    // @OpenForgeText doctor.title.workspace-cannot-be-read
    internal static string TitleWorkspaceCannotBeRead()
        => "Workspace cannot be read";

    // @OpenForgeText doctor.title.the-agents-folder-is-missing
    internal static string TitleTheAgentsFolderIsMissing()
        => "The .agents folder is missing";

    // @OpenForgeText doctor.title.the-agents-folder-cannot-be-read
    internal static string TitleTheAgentsFolderCannotBeRead()
        => "The .agents folder cannot be read";

    // @OpenForgeText doctor.title.loader-is-missing
    internal static string TitleLoaderIsMissing()
        => "Loader is missing";

    // @OpenForgeText doctor.title.loader-cannot-be-read
    internal static string TitleLoaderCannotBeRead()
        => "Loader cannot be read";

    // @OpenForgeText doctor.title.loader-has-invalid-content
    internal static string TitleLoaderHasInvalidContent()
        => "Loader has invalid content";

    // @OpenForgeText doctor.title.entrypoint-is-missing
    internal static string TitleEntrypointIsMissing()
        => "Entrypoint is missing";

    // @OpenForgeText doctor.title.several-entrypoints-match
    internal static string TitleSeveralEntrypointsMatch()
        => "Several entrypoints match";

    // @OpenForgeText doctor.title.entrypoint-names-conflict
    internal static string TitleEntrypointNamesConflict()
        => "Entrypoint names conflict";

    // @OpenForgeText doctor.title.source-ids-conflict
    internal static string TitleSourceIdsConflict()
        => "Source IDs conflict";

    // @OpenForgeText doctor.title.path-is-invalid
    internal static string TitlePathIsInvalid()
        => "Path is invalid";

    // @OpenForgeText doctor.title.path-is-outside-the-workspace
    internal static string TitlePathIsOutsideTheWorkspace()
        => "Path is outside the workspace";

    // @OpenForgeText doctor.title.path-identity-is-ambiguous
    internal static string TitlePathIdentityIsAmbiguous()
        => "Path identity is ambiguous";

    // @OpenForgeText doctor.title.frontmatter-contains-duplicate-fields
    internal static string TitleFrontmatterContainsDuplicateFields()
        => "Frontmatter contains duplicate fields";

    // @OpenForgeText doctor.title.source-could-not-be-read-completely
    internal static string TitleSourceCouldNotBeReadCompletely()
        => "Source could not be read completely";

    // @OpenForgeText doctor.title.source-type-is-unsupported
    internal static string TitleSourceTypeIsUnsupported()
        => "Source type is unsupported";

    // @OpenForgeText doctor.title.root-route-is-missing
    internal static string TitleRootRouteIsMissing()
        => "Root route is missing";

    // @OpenForgeText doctor.title.root-route-cannot-be-reached
    internal static string TitleRootRouteCannotBeReached()
        => "Root route cannot be reached";

    // @OpenForgeText doctor.title.source-is-outside-the-loaded-routes
    internal static string TitleSourceIsOutsideTheLoadedRoutes()
        => "Source is outside the loaded routes";

    // @OpenForgeText doctor.title.no-ownership-record
    internal static string TitleNoOwnershipRecord()
        => "No ownership record";

    // @OpenForgeText doctor.title.library-source-root-has-an-ambiguous-path
    internal static string TitleLibrarySourceRootHasAnAmbiguousPath()
        => "Library source root has an ambiguous path";

    // @OpenForgeText doctor.title.library-source-scan-is-incomplete
    internal static string TitleLibrarySourceScanIsIncomplete()
        => "Library source scan is incomplete";

    // @OpenForgeText doctor.title.registered-library-link-is-missing
    internal static string TitleRegisteredLibraryLinkIsMissing()
        => "Registered Library link is missing";

    // @OpenForgeText doctor.title.library-link-target-is-missing
    internal static string TitleLibraryLinkTargetIsMissing()
        => "Library link target is missing";

    // @OpenForgeText doctor.title.library-link-target-changed
    internal static string TitleLibraryLinkTargetChanged()
        => "Library link target changed";

    // @OpenForgeText doctor.title.library-destination-is-occupied
    internal static string TitleLibraryDestinationIsOccupied()
        => "Library destination is occupied";

    // @OpenForgeText doctor.title.required-library-links-are-unsupported
    internal static string TitleRequiredLibraryLinksAreUnsupported()
        => "Required Library links are unsupported";

    // @OpenForgeText doctor.title.library-and-extension-paths-conflict
    internal static string TitleLibraryAndExtensionPathsConflict()
        => "Library and Extension paths conflict";

    // @OpenForgeText doctor.title.verified-library-recovery-is-available
    internal static string TitleVerifiedLibraryRecoveryIsAvailable()
        => "Verified Library recovery is available";

    // @OpenForgeText doctor.title.incomplete-recovery-draft-found
    internal static string TitleIncompleteRecoveryDraftFound()
        => "Incomplete recovery draft found";

    // @OpenForgeText doctor.title.recovery-origin-could-not-be-verified
    internal static string TitleRecoveryOriginCouldNotBeVerified()
        => "Recovery origin could not be verified";

    // @OpenForgeText doctor.title.route-entrypoint-is-missing
    internal static string TitleRouteEntrypointIsMissing()
        => "Route entrypoint is missing";

    // @OpenForgeText doctor.title.route-has-several-entrypoints
    internal static string TitleRouteHasSeveralEntrypoints()
        => "Route has several entrypoints";

    // @OpenForgeText doctor.title.route-leaves-its-allowed-boundary
    internal static string TitleRouteLeavesItsAllowedBoundary()
        => "Route leaves its allowed boundary";

    // @OpenForgeText doctor.title.route-cannot-be-reached
    internal static string TitleRouteCannotBeReached()
        => "Route cannot be reached";

    // @OpenForgeText doctor.title.required-route-metadata-is-missing
    internal static string TitleRequiredRouteMetadataIsMissing()
        => "Required route metadata is missing";

    // @OpenForgeText doctor.title.route-title-is-invalid
    internal static string TitleRouteTitleIsInvalid()
        => "Route title is invalid";

    // @OpenForgeText doctor.title.route-axioms-are-invalid
    internal static string TitleRouteAxiomsAreInvalid()
        => "Route Axioms are invalid";

    // @OpenForgeText doctor.title.entries-section-is-malformed
    internal static string TitleEntriesSectionIsMalformed()
        => "Entries section is malformed";

    // @OpenForgeText doctor.title.entries-section-is-not-last
    internal static string TitleEntriesSectionIsNotLast()
        => "Entries section is not last";

    // @OpenForgeText doctor.title.more-than-one-entries-section
    internal static string TitleMoreThanOneEntriesSection()
        => "More than one Entries section";

    // @OpenForgeText doctor.title.entry-is-missing
    internal static string TitleEntryIsMissing()
        => "Entry is missing";

    // @OpenForgeText doctor.title.entry-has-no-file
    internal static string TitleEntryHasNoFile()
        => "Entry has no file";

    // @OpenForgeText doctor.title.entries-are-out-of-order
    internal static string TitleEntriesAreOutOfOrder()
        => "Entries are out of order";

    // @OpenForgeText doctor.title.entry-path-is-wrong
    internal static string TitleEntryPathIsWrong()
        => "Entry path is wrong";

    // @OpenForgeText doctor.title.entry-description-is-stale
    internal static string TitleEntryDescriptionIsStale()
        => "Entry description is stale";

    // @OpenForgeText doctor.title.entry-tags-are-stale
    internal static string TitleEntryTagsAreStale()
        => "Entry tags are stale";

    // @OpenForgeText doctor.title.overwrite-is-listed-independently
    internal static string TitleOverwriteIsListedIndependently()
        => "Overwrite is listed independently";

    // @OpenForgeText doctor.title.route-names-conflict
    internal static string TitleRouteNamesConflict()
        => "Route names conflict";

    // @OpenForgeText doctor.title.linked-heading-was-not-found
    internal static string TitleLinkedHeadingWasNotFound()
        => "Linked heading was not found";

    // @OpenForgeText doctor.title.linked-heading-could-not-be-checked
    internal static string TitleLinkedHeadingCouldNotBeChecked()
        => "Linked heading could not be checked";

    // @OpenForgeText doctor.title.link-destination-is-invalid
    internal static string TitleLinkDestinationIsInvalid()
        => "Link destination is invalid";

    // @OpenForgeText doctor.title.absolute-local-link-is-unsupported
    internal static string TitleAbsoluteLocalLinkIsUnsupported()
        => "Absolute local link is unsupported";

    // @OpenForgeText doctor.title.local-link-query-is-unsupported
    internal static string TitleLocalLinkQueryIsUnsupported()
        => "Local link query is unsupported";

    // @OpenForgeText doctor.title.link-encoding-is-unsupported
    internal static string TitleLinkEncodingIsUnsupported()
        => "Link encoding is unsupported";

    // @OpenForgeText doctor.title.link-leaves-the-workspace
    internal static string TitleLinkLeavesTheWorkspace()
        => "Link leaves the workspace";

    // @OpenForgeText doctor.title.link-resolves-outside-the-workspace
    internal static string TitleLinkResolvesOutsideTheWorkspace()
        => "Link resolves outside the workspace";

    // @OpenForgeText doctor.title.link-target-identity-is-ambiguous
    internal static string TitleLinkTargetIdentityIsAmbiguous()
        => "Link target identity is ambiguous";

    // @OpenForgeText doctor.title.link-target-cannot-be-read
    internal static string TitleLinkTargetCannotBeRead()
        => "Link target cannot be read";

    // @OpenForgeText doctor.title.link-target-type-is-unsupported
    internal static string TitleLinkTargetTypeIsUnsupported()
        => "Link target type is unsupported";

    // @OpenForgeText doctor.title.equivalent-link-path-is-available
    internal static string TitleEquivalentLinkPathIsAvailable()
        => "Equivalent link path is available";

    // @OpenForgeText doctor.title.equivalent-link-letter-case-is-available
    internal static string TitleEquivalentLinkLetterCaseIsAvailable()
        => "Equivalent link letter case is available";

    // @OpenForgeText doctor.title.equivalent-link-encoding-is-available
    internal static string TitleEquivalentLinkEncodingIsAvailable()
        => "Equivalent link encoding is available";

    // @OpenForgeText doctor.title.equivalent-heading-link-is-available
    internal static string TitleEquivalentHeadingLinkIsAvailable()
        => "Equivalent heading link is available";

    // @OpenForgeText doctor.title.framework-is-not-installed
    internal static string TitleFrameworkIsNotInstalled()
        => "Framework is not installed";

    // @OpenForgeText doctor.title.framework-file-is-missing
    internal static string TitleFrameworkFileIsMissing()
        => "Framework file is missing";

    // @OpenForgeText doctor.title.framework-file-changed
    internal static string TitleFrameworkFileChanged()
        => "Framework file changed";

    // @OpenForgeText doctor.title.ownership-record-cannot-be-read
    internal static string TitleOwnershipRecordCannotBeRead()
        => "Ownership record cannot be read";

    // @OpenForgeText doctor.title.open-forge-section-in-agents-md-needs-review
    internal static string TitleOpenForgeSectionInAgentsMdNeedsReview()
        => "Open Forge section in AGENTS.md needs review";

    // @OpenForgeText doctor.title.open-forge-section-boundary-is-unclear
    internal static string TitleOpenForgeSectionBoundaryIsUnclear()
        => "Open Forge section boundary is unclear";

    // @OpenForgeText doctor.title.framework-file-ownership-conflicts
    internal static string TitleFrameworkFileOwnershipConflicts()
        => "Framework file ownership conflicts";

    // @OpenForgeText doctor.title.framework-update-did-not-finish
    internal static string TitleFrameworkUpdateDidNotFinish()
        => "Framework update did not finish";

    // @OpenForgeText doctor.title.framework-recovery-is-incomplete
    internal static string TitleFrameworkRecoveryIsIncomplete()
        => "Framework recovery is incomplete";

    // @OpenForgeText doctor.title.bundled-framework-is-invalid
    internal static string TitleBundledFrameworkIsInvalid()
        => "Bundled Framework is invalid";

    // @OpenForgeText doctor.title.extension-manifest-is-missing
    internal static string TitleExtensionManifestIsMissing()
        => "Extension manifest is missing";

    // @OpenForgeText doctor.title.extension-manifest-is-invalid
    internal static string TitleExtensionManifestIsInvalid()
        => "Extension manifest is invalid";

    // @OpenForgeText doctor.title.extension-id-is-duplicated
    internal static string TitleExtensionIdIsDuplicated()
        => "Extension ID is duplicated";

    // @OpenForgeText doctor.title.extension-id-is-unknown
    internal static string TitleExtensionIdIsUnknown()
        => "Extension ID is unknown";

    // @OpenForgeText doctor.title.extension-version-is-invalid
    internal static string TitleExtensionVersionIsInvalid()
        => "Extension version is invalid";

    // @OpenForgeText doctor.title.extension-file-is-missing
    internal static string TitleExtensionFileIsMissing()
        => "Extension file is missing";

    // @OpenForgeText doctor.title.extension-file-changed
    internal static string TitleExtensionFileChanged()
        => "Extension file changed";

    // @OpenForgeText doctor.title.required-extension-dependency-is-missing
    internal static string TitleRequiredExtensionDependencyIsMissing()
        => "Required Extension dependency is missing";

    // @OpenForgeText doctor.title.extension-dependencies-form-a-cycle
    internal static string TitleExtensionDependenciesFormACycle()
        => "Extension dependencies form a cycle";

    // @OpenForgeText doctor.title.extension-dependency-version-is-incompatible
    internal static string TitleExtensionDependencyVersionIsIncompatible()
        => "Extension dependency version is incompatible";

    // @OpenForgeText doctor.title.extension-source-cannot-be-read
    internal static string TitleExtensionSourceCannotBeRead()
        => "Extension source cannot be read";

    // @OpenForgeText doctor.title.package-folder-cannot-be-read
    internal static string TitlePackageFolderCannotBeRead()
        => "Package folder cannot be read";

    // @OpenForgeText doctor.title.extension-update-did-not-finish
    internal static string TitleExtensionUpdateDidNotFinish()
        => "Extension update did not finish";

    // @OpenForgeText doctor.title.extension-file-ownership-conflicts
    internal static string TitleExtensionFileOwnershipConflicts()
        => "Extension file ownership conflicts";

    // @OpenForgeText doctor.title.extension-bridge-registration-needs-review
    internal static string TitleExtensionBridgeRegistrationNeedsReview()
        => "Extension bridge registration needs review";

    // @OpenForgeText doctor.message.agents-loader-md-is-missing
    internal static string MessageAgentsLoaderMdIsMissing()
        => ".agents/loader.md is missing.";

    // @OpenForgeText doctor.label.is-not-a-folder-inside-the-workspace
    internal static string LabelIsNotAFolderInsideTheWorkspace()
        => "is not a folder inside the workspace";

    // @OpenForgeText doctor.label.resolves-to-an-ambiguous-location
    internal static string LabelResolvesToAnAmbiguousLocation()
        => "resolves to an ambiguous location";

    // @OpenForgeText doctor.label.works-but-is-not-the-canonical-spelling
    internal static string LabelWorksButIsNotTheCanonicalSpelling()
        => "works but is not the canonical spelling";

    // @OpenForgeText doctor.label.differs-from-the-file-s-name-only-by-letter-case
    internal static string LabelDiffersFromTheFileSNameOnlyByLetterCase()
        => "differs from the file's name only by letter case";

    // @OpenForgeText doctor.label.uses-a-different-encoding-than-the-canonical
    internal static string LabelUsesADifferentEncodingThanTheCanonical()
        => "uses a different encoding than the canonical";

    // @OpenForgeText doctor.message.open-forge-is-not-installed-in-this-workspace
    internal static string MessageOpenForgeIsNotInstalledInThisWorkspace()
        => "Open Forge is not installed in this workspace.";

    // @OpenForgeText doctor.message.some-framework-files-are-current-and-others-are-not-so-an-update-did-not-finish
    internal static string MessageSomeFrameworkFilesAreCurrentAndOthersAreNotSoAnUpdateDidNotFinish()
        => "Some Framework files are current and others are not, so an update did not finish.";

    // @OpenForgeText doctor.message.no-problems-found
    internal static string MessageNoProblemsFound()
        => "No problems found.";

    // @OpenForgeText doctor.title.to-list-the-warnings-open-forge-doctor-detail-standard
    internal static string TitleToListTheWarningsOpenForgeDoctorDetailStandard()
        => "To list the warnings: open-forge doctor --detail standard";

    // @OpenForgeText doctor.title.to-list-the-info-findings-open-forge-doctor-detail-full
    internal static string TitleToListTheInfoFindingsOpenForgeDoctorDetailFull()
        => "To list the info findings: open-forge doctor --detail full";

    // @OpenForgeText doctor.message.the-check-could-not-finish
    internal static string MessageTheCheckCouldNotFinish()
        => "the check could not finish.";

    // @OpenForgeText doctor.title.extensions-were-not-checked
    internal static string TitleExtensionsWereNotChecked()
        => "Extensions were not checked";

    // @OpenForgeText doctor.title.framework-files-were-not-checked
    internal static string TitleFrameworkFilesWereNotChecked()
        => "Framework files were not checked";

    // @OpenForgeText doctor.title.links-were-not-checked
    internal static string TitleLinksWereNotChecked()
        => "Links were not checked";

    // @OpenForgeText doctor.title.routes-were-not-checked
    internal static string TitleRoutesWereNotChecked()
        => "Routes were not checked";

    // @OpenForgeText doctor.title.recovery-data-was-not-checked
    internal static string TitleRecoveryDataWasNotChecked()
        => "Recovery data was not checked";

    // @OpenForgeText doctor.label.need-a-choice
    internal static string LabelNeedAChoice()
        => "need a choice";

    // @OpenForgeText doctor.label.uses-the-indicated-command
    internal static string LabelUsesTheIndicatedCommand()
        => "uses the indicated command";

    // @OpenForgeText doctor.label.use-the-indicated-command
    internal static string LabelUseTheIndicatedCommand()
        => "use the indicated command";

    // @OpenForgeText doctor.label.must-be-fixed-by-hand
    internal static string LabelMustBeFixedByHand()
        => "must be fixed by hand";

    // @OpenForgeText doctor.label.is-blocked
    internal static string LabelIsBlocked()
        => "is blocked";

    // @OpenForgeText doctor.label.are-blocked
    internal static string LabelAreBlocked()
        => "are blocked";

    // @OpenForgeText doctor.label.the-canonical-spelling
    internal static string LabelTheCanonicalSpelling()
        => "the canonical spelling";

    // @OpenForgeText doctor.label.the-target
    internal static string LabelTheTarget()
        => "the target";

    // @OpenForgeText doctor.label.the-expected-target
    internal static string LabelTheExpectedTarget()
        => "the expected target";

    // @OpenForgeText doctor.label.the-actual-target
    internal static string LabelTheActualTarget()
        => "the actual target";

    // @OpenForgeText doctor.label.the-authored-value
    internal static string LabelTheAuthoredValue()
        => "the authored value";

    // @OpenForgeText doctor.placeholder.id
    internal static string PlaceholderId()
        => "<id>";

    // @OpenForgeText doctor.message.preview-the-framework-installation
    internal static string MessagePreviewTheFrameworkInstallation()
        => "Preview the Framework installation.";

    // @OpenForgeText doctor.message.restore-the-shipped-loader
    internal static string MessageRestoreTheShippedLoader()
        => "Restore the shipped Loader.";

    // @OpenForgeText doctor.message.create-the-missing-route-entrypoint
    internal static string MessageCreateTheMissingRouteEntrypoint()
        => "Create the missing route entrypoint.";

    // @OpenForgeText doctor.message.index-the-routed-parent-before-loading-this-source
    internal static string MessageIndexTheRoutedParentBeforeLoadingThisSource()
        => "Index the routed parent before loading this source.";

    // @OpenForgeText doctor.message.refresh-the-route-index
    internal static string MessageRefreshTheRouteIndex()
        => "Refresh the route index.";

    // @OpenForgeText doctor.message.update-deterministic-generated-navigation
    internal static string MessageUpdateDeterministicGeneratedNavigation()
        => "Update deterministic generated navigation.";

    // @OpenForgeText doctor.message.run-doctor-again-after-fixing-the-source-file
    internal static string MessageRunDoctorAgainAfterFixingTheSourceFile()
        => "Run Doctor again after fixing the source file.";

    // @OpenForgeText doctor.message.choose-one-of-the-bounded-candidates
    internal static string MessageChooseOneOfTheBoundedCandidates()
        => "Choose one of the bounded candidates.";

    // @OpenForgeText doctor.message.fix-the-link-by-hand
    internal static string MessageFixTheLinkByHand()
        => "Fix the link by hand.";

    // @OpenForgeText doctor.message.apply-the-exact-same-target-correction
    internal static string MessageApplyTheExactSameTargetCorrection()
        => "Apply the exact same-target correction.";

    // @OpenForgeText doctor.message.reconcile-the-framework-files
    internal static string MessageReconcileTheFrameworkFiles()
        => "Reconcile the Framework files.";

    // @OpenForgeText doctor.message.inspect-the-incomplete-framework-recovery
    internal static string MessageInspectTheIncompleteFrameworkRecovery()
        => "Inspect the incomplete Framework recovery.";

    // @OpenForgeText doctor.message.reinstall-the-cli
    internal static string MessageReinstallTheCli()
        => "Reinstall the CLI.";

    // @OpenForgeText doctor.message.reconcile-the-extension-files
    internal static string MessageReconcileTheExtensionFiles()
        => "Reconcile the Extension files.";

    // @OpenForgeText doctor.message.list-the-available-extensions
    internal static string MessageListTheAvailableExtensions()
        => "List the available Extensions.";

    // @OpenForgeText doctor.message.install-the-missing-extension-dependency
    internal static string MessageInstallTheMissingExtensionDependency()
        => "Install the missing Extension dependency.";

    // @OpenForgeText doctor.message.refresh-the-extension-entry
    internal static string MessageRefreshTheExtensionEntry()
        => "Refresh the Extension entry.";

    // @OpenForgeText doctor.message.inspect-the-library-source-and-projection
    internal static string MessageInspectTheLibrarySourceAndProjection()
        => "Inspect the Library source and projection.";

    // @OpenForgeText doctor.message.review-the-missing-library-link
    internal static string MessageReviewTheMissingLibraryLink()
        => "Review the missing Library link.";

    // @OpenForgeText doctor.message.apply-the-verified-recovery-step
    internal static string MessageApplyTheVerifiedRecoveryStep()
        => "Apply the verified recovery step.";

    // @OpenForgeText doctor.message.review-and-remove-the-kept-recovery-bundle
    internal static string MessageReviewAndRemoveTheKeptRecoveryBundle()
        => "Review and remove the kept recovery bundle.";

    // @OpenForgeText doctor.message.preview-recovery-cleanup
    internal static string MessagePreviewRecoveryCleanup()
        => "Preview recovery cleanup.";

    // @OpenForgeText doctor.label.external-links
    internal static string LabelExternalLinks()
        => "external links";
}
