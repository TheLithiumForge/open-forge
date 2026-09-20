using System.Globalization;

namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorPhrases
{
    // @OpenForgeText doctor.phrase.cannot-run-doctor
    internal static string FormatCannotRunDoctor(string eventReasonText)
        => $"Cannot run doctor: {eventReasonText}";

    // @OpenForgeText doctor.phrase.cannot-check-this-workspace
    internal static string FormatCannotCheckThisWorkspace(string eventReasonText)
        => $"Cannot check this workspace: {eventReasonText}";

    // @OpenForgeText doctor.phrase.doctor-stopped-because-of-an-unexpected-error
    internal static string FormatDoctorStoppedBecauseOfAnUnexpectedError(string eventReasonText)
        => $"Doctor stopped because of an unexpected error: {eventReasonText}";

    // @OpenForgeText doctor.phrase.preview-that-safe-to-apply
    internal static string FormatPreviewThatSafeToApply(string safeText, string pluralText, string valueText)
        => $"Preview {safeText} {pluralText} that {valueText} safe to apply.";

    // @OpenForgeText doctor.phrase.edit-by-hand
    internal static string FormatEditByHand(string valueText)
        => $"Edit {valueText} by hand.";

    // @OpenForgeText doctor.phrase.does-not-exist-or-cannot-be-read
    internal static string FormatDoesNotExistOrCannotBeRead(string pathText)
        => $"{pathText} does not exist or cannot be read.";

    // @OpenForgeText doctor.phrase.is-a-file-not-a-directory
    internal static string FormatIsAFileNotADirectory(string pathText)
        => $"{pathText} is a file, not a directory.";

    // @OpenForgeText doctor.phrase.has-no-agents-folder-open-forge-is-not-installed-here
    internal static string FormatHasNoAgentsFolderOpenForgeIsNotInstalledHere(string pathText)
        => $"{pathText} has no .agents folder. Open Forge is not installed here.";

    // @OpenForgeText doctor.phrase.agents-exists-but-cannot-be-read
    internal static string FormatAgentsExistsButCannotBeRead(string causeText)
        => $".agents exists but cannot be read: {causeText}.";

    // @OpenForgeText doctor.phrase.agents-loader-md-cannot-be-read
    internal static string FormatAgentsLoaderMdCannotBeRead(string causeText)
        => $".agents/loader.md cannot be read: {causeText}.";

    // @OpenForgeText doctor.phrase.agents-loader-md-could-not-be-understood
    internal static string FormatAgentsLoaderMdCouldNotBeUnderstood(string causeText)
        => $".agents/loader.md could not be understood: {causeText}.";

    // @OpenForgeText doctor.phrase.is-routed-but-has-no-entrypoint-file
    internal static string FormatIsRoutedButHasNoEntrypointFile(string pathText)
        => $"{pathText} is routed but has no entrypoint file.";

    // @OpenForgeText doctor.phrase.has-more-than-one-entrypoint-file-keep-one
    internal static string FormatHasMoreThanOneEntrypointFileKeepOne(string pathText, string causeText)
        => $"{pathText} has more than one entrypoint file: {causeText}. Keep one.";

    // @OpenForgeText doctor.phrase.has-incompatible-entrypoint-forms-keep-one
    internal static string FormatHasIncompatibleEntrypointFormsKeepOne(string pathText)
        => $"{pathText} has incompatible entrypoint forms. Keep one.";

    // @OpenForgeText doctor.phrase.derives-an-id-that-is-also-derived-by-another-file
    internal static string FormatDerivesAnIdThatIsAlsoDerivedByAnotherFile(string pathText, string causeText)
        => $"{pathText} derives an ID that is also derived by another file: {causeText}.";

    // @OpenForgeText doctor.phrase.is-not-a-valid-path-for-a-source
    internal static string FormatIsNotAValidPathForASource(string pathText)
        => $"{pathText} is not a valid path for a source.";

    // @OpenForgeText doctor.phrase.points-outside-the-workspace
    internal static string FormatPointsOutsideTheWorkspace(string pathText)
        => $"{pathText} points outside the workspace.";

    // @OpenForgeText doctor.phrase.has-an-ambiguous-physical-identity
    internal static string FormatHasAnAmbiguousPhysicalIdentity(string pathText, string causeText)
        => $"{pathText} has an ambiguous physical identity: {causeText}.";

    // @OpenForgeText doctor.phrase.could-not-be-parsed-completely-its-checks-are-incomplete
    internal static string FormatCouldNotBeParsedCompletelyItsChecksAreIncomplete(string pathText, string causeText)
        => $"{pathText} could not be parsed completely: {causeText}. Its checks are incomplete.";

    // @OpenForgeText doctor.phrase.is-not-a-kind-of-file-open-forge-checks
    internal static string FormatIsNotAKindOfFileOpenForgeChecks(string pathText)
        => $"{pathText} is not a kind of file Open Forge checks.";

    // @OpenForgeText doctor.phrase.the-loader-lists-but-does-not-exist
    internal static string FormatTheLoaderListsButDoesNotExist(string valueText, string pathText)
        => $"The Loader lists {valueText} but {pathText} does not exist.";

    // @OpenForgeText doctor.phrase.is-listed-but-cannot-be-reached-from-the-loader
    internal static string FormatIsListedButCannotBeReachedFromTheLoader(string valueText, string causeText)
        => $"{valueText} is listed but cannot be reached from the Loader: {causeText}.";

    // @OpenForgeText doctor.phrase.is-not-reachable-from-any-route-so-agents-never-load-it
    internal static string FormatIsNotReachableFromAnyRouteSoAgentsNeverLoadIt(string pathText)
        => $"{pathText} is not reachable from any route, so agents never load it.";

    // @OpenForgeText doctor.phrase.the-source-folder-of-could-not-be-scanned-completely
    internal static string FormatTheSourceFolderOfCouldNotBeScannedCompletely(string libraryIdText, string causeText)
        => $"The source folder of {libraryIdText} could not be scanned completely: {causeText}.";

    // @OpenForgeText doctor.phrase.a-link-of-is-missing
    internal static string FormatALinkOfIsMissing(string pathText, string libraryIdText)
        => $"{pathText}, a link of {libraryIdText}, is missing.";

    // @OpenForgeText doctor.phrase.is-used-by-and-by-another-managed-domain
    internal static string FormatIsUsedByAndByAnotherManagedDomain(string pathText, string libraryIdText)
        => $"{pathText} is used by {libraryIdText} and by another managed domain.";

    // @OpenForgeText doctor.phrase.this-system-cannot-create-the-file-links-the-library-needs
    internal static string FormatThisSystemCannotCreateTheFileLinksTheLibraryNeeds(string libraryIdText)
        => $"This system cannot create the file links the {libraryIdText} Library needs.";

    // @OpenForgeText doctor.phrase.is-claimed-by-the-library-and-by-an-extension
    internal static string FormatIsClaimedByTheLibraryAndByAnExtension(string pathText, string libraryIdText)
        => $"{pathText} is claimed by the {libraryIdText} Library and by an Extension.";

    // @OpenForgeText doctor.phrase.a-verified-recovery-step-for-can-restore
    internal static string FormatAVerifiedRecoveryStepForCanRestore(string libraryIdText, string pathText)
        => $"A verified recovery step for {libraryIdText} can restore {pathText}.";

    // @OpenForgeText doctor.phrase.the-recovery-bundle-at-is-damaged-it-was-left-in-place
    internal static string FormatTheRecoveryBundleAtIsDamagedItWasLeftInPlace(string pathText, string causeText)
        => $"The recovery bundle at {pathText} is damaged: {causeText}. It was left in place.";

    // @OpenForgeText doctor.phrase.the-recovery-bundle-at-cannot-be-verified-so-cleanup-will-not-delete-it
    internal static string FormatTheRecoveryBundleAtCannotBeVerifiedSoCleanupWillNotDeleteIt(string pathText)
        => $"The recovery bundle at {pathText} cannot be verified, so cleanup will not delete it.";

    // @OpenForgeText doctor.phrase.has-more-than-one-entrypoint
    internal static string FormatHasMoreThanOneEntrypoint(string pathText, string causeText)
        => $"{pathText} has more than one entrypoint: {causeText}.";

    // @OpenForgeText doctor.phrase.is-routed-but-no-parent-lists-it
    internal static string FormatIsRoutedButNoParentListsIt(string pathText)
        => $"{pathText} is routed but no parent lists it.";

    // @OpenForgeText doctor.phrase.looks-like-a-route-but-no-loader-entry-or-parent-reaches-it
    internal static string FormatLooksLikeARouteButNoLoaderEntryOrParentReachesIt(string pathText)
        => $"{pathText} looks like a route but no Loader entry or parent reaches it.";

    // @OpenForgeText doctor.phrase.has-no-level-1-heading
    internal static string FormatHasNoLevel1Heading(string pathText)
        => $"{pathText} has no level-1 heading.";

    // @OpenForgeText doctor.phrase.has-no-axioms-section-or-its-axioms-section-is-malformed
    internal static string FormatHasNoAxiomsSectionOrItsAxiomsSectionIsMalformed(string pathText)
        => $"{pathText} has no Axioms section, or its Axioms section is malformed.";

    // @OpenForgeText doctor.phrase.the-entries-section-of-does-not-match-its-routed-files
    internal static string FormatTheEntriesSectionOfDoesNotMatchItsRoutedFiles(string pathText)
        => $"The Entries section of {pathText} does not match its routed files.";

    // @OpenForgeText doctor.phrase.the-entries-section-of-could-not-be-read-as-a-list
    internal static string FormatTheEntriesSectionOfCouldNotBeReadAsAList(string pathText)
        => $"The Entries section of {pathText} could not be read as a list.";

    // @OpenForgeText doctor.phrase.the-entries-section-of-is-followed-by-another-section
    internal static string FormatTheEntriesSectionOfIsFollowedByAnotherSection(string pathText)
        => $"The Entries section of {pathText} is followed by another section.";

    // @OpenForgeText doctor.phrase.has-more-than-one-entries-section
    internal static string FormatHasMoreThanOneEntriesSection(string pathText)
        => $"{pathText} has more than one Entries section.";

    // @OpenForgeText doctor.phrase.does-not-list
    internal static string FormatDoesNotList(string pathText, string findingIdentifierText)
        => $"{pathText} does not list {findingIdentifierText}.";

    // @OpenForgeText doctor.phrase.lists-which-does-not-exist
    internal static string FormatListsWhichDoesNotExist(string pathText, string findingIdentifierText)
        => $"{pathText} lists {findingIdentifierText}, which does not exist.";

    // @OpenForgeText doctor.phrase.the-entries-in-are-not-in-the-expected-order
    internal static string FormatTheEntriesInAreNotInTheExpectedOrder(string pathText)
        => $"The entries in {pathText} are not in the expected order.";

    // @OpenForgeText doctor.phrase.the-entry-for-in-points-to
    internal static string FormatTheEntryForInPointsTo(string findingIdentifierText, string pathText, string comparisonActualText)
        => $"The entry for {findingIdentifierText} in {pathText} points to {comparisonActualText}.";

    // @OpenForgeText doctor.phrase.the-entry-for-in-has-an-old-description
    internal static string FormatTheEntryForInHasAnOldDescription(string findingIdentifierText, string pathText)
        => $"The entry for {findingIdentifierText} in {pathText} has an old description.";

    // @OpenForgeText doctor.phrase.the-entry-for-in-has-old-tags
    internal static string FormatTheEntryForInHasOldTags(string findingIdentifierText, string pathText)
        => $"The entry for {findingIdentifierText} in {pathText} has old tags.";

    // @OpenForgeText doctor.phrase.is-not-a-link-open-forge-can-check
    internal static string FormatIsNotALinkOpenForgeCanCheck(string findingIdentifierText)
        => $"{findingIdentifierText} is not a link Open Forge can check.";

    // @OpenForgeText doctor.phrase.is-an-absolute-path-use-a-relative-path
    internal static string FormatIsAnAbsolutePathUseARelativePath(string findingIdentifierText)
        => $"{findingIdentifierText} is an absolute path. Use a relative path.";

    // @OpenForgeText doctor.phrase.has-a-query-string-which-local-links-do-not-support
    internal static string FormatHasAQueryStringWhichLocalLinksDoNotSupport(string findingIdentifierText)
        => $"{findingIdentifierText} has a query string, which local links do not support.";

    // @OpenForgeText doctor.phrase.uses-an-encoding-that-cannot-be-resolved-safely
    internal static string FormatUsesAnEncodingThatCannotBeResolvedSafely(string findingIdentifierText)
        => $"{findingIdentifierText} uses an encoding that cannot be resolved safely.";

    // @OpenForgeText doctor.phrase.resolves-outside-the-workspace-through-a-link
    internal static string FormatResolvesOutsideTheWorkspaceThroughALink(string findingIdentifierText)
        => $"{findingIdentifierText} resolves outside the workspace through a link.";

    // @OpenForgeText doctor.phrase.resolves-to-more-than-one-file
    internal static string FormatResolvesToMoreThanOneFile(string findingIdentifierText)
        => $"{findingIdentifierText} resolves to more than one file.";

    // @OpenForgeText doctor.phrase.exists-but-cannot-be-read
    internal static string FormatExistsButCannotBeRead(string findingIdentifierText, string causeText)
        => $"{findingIdentifierText} exists but cannot be read: {causeText}.";

    // @OpenForgeText doctor.phrase.is-a-kind-of-file-open-forge-does-not-check
    internal static string FormatIsAKindOfFileOpenForgeDoesNotCheck(string findingIdentifierText)
        => $"{findingIdentifierText} is a kind of file Open Forge does not check.";

    // @OpenForgeText doctor.phrase.is-missing-it-was-installed-by-the-framework
    internal static string FormatIsMissingItWasInstalledByTheFramework(string pathText)
        => $"{pathText} is missing. It was installed by the Framework.";

    // @OpenForgeText doctor.phrase.changed-since-it-was-installed
    internal static string FormatChangedSinceItWasInstalled(string pathText)
        => $"{pathText} changed since it was installed.";

    // @OpenForgeText doctor.phrase.agents-open-forge-lock-json-could-not-be-read
    internal static string FormatAgentsOpenForgeLockJsonCouldNotBeRead(string causeText)
        => $".agents/open-forge.lock.json could not be read: {causeText}.";

    // @OpenForgeText doctor.phrase.the-open-forge-section-in-is-missing-or-changed
    internal static string FormatTheOpenForgeSectionInIsMissingOrChanged(string bridgeFileText)
        => $"The Open Forge section in {bridgeFileText} is missing or changed.";

    // @OpenForgeText doctor.phrase.the-open-forge-section-in-has-no-clear-start-or-end
    internal static string FormatTheOpenForgeSectionInHasNoClearStartOrEnd(string pathText)
        => $"The Open Forge section in {pathText} has no clear start or end.";

    // @OpenForgeText doctor.phrase.is-claimed-by-the-framework-and-by-another-managed-domain
    internal static string FormatIsClaimedByTheFrameworkAndByAnotherManagedDomain(string pathText)
        => $"{pathText} is claimed by the Framework and by another managed domain.";

    // @OpenForgeText doctor.phrase.the-recovery-bundle-at-was-partly-applied-some-files-match-the-old-content-and-some-the-new
    internal static string FormatTheRecoveryBundleAtWasPartlyAppliedSomeFilesMatchTheOldContentAndSomeTheNew(string pathText)
        => $"The recovery bundle at {pathText} was partly applied: some files match the old content and some the new.";

    // @OpenForgeText doctor.phrase.the-package-at-has-no-extension-json
    internal static string FormatThePackageAtHasNoExtensionJson(string pathText)
        => $"The package at {pathText} has no extension.json.";

    // @OpenForgeText doctor.phrase.extension-json-could-not-be-read-expected-keys-id-name-description-version-dependencies
    internal static string FormatExtensionJsonCouldNotBeReadExpectedKeysIdNameDescriptionVersionDependencies(string pathText, string causeText)
        => $"{pathText}/extension.json could not be read: {causeText}. Expected keys: id, name, description, version, dependencies.";

    // @OpenForgeText doctor.phrase.two-packages-in-have-the-id
    internal static string FormatTwoPackagesInHaveTheId(string pathText, string findingIdentifierText)
        => $"Two packages in {pathText} have the ID {findingIdentifierText}.";

    // @OpenForgeText doctor.phrase.the-ownership-record-names-which-is-not-in-the-bundled-extensions-or-the-recorded-source
    internal static string FormatTheOwnershipRecordNamesWhichIsNotInTheBundledExtensionsOrTheRecordedSource(string findingIdentifierText)
        => $"The ownership record names {findingIdentifierText}, which is not in the bundled Extensions or the recorded source.";

    // @OpenForgeText doctor.phrase.is-missing-it-was-installed-by
    internal static string FormatIsMissingItWasInstalledBy(string pathText, string extensionIdText)
        => $"{pathText} is missing. It was installed by {extensionIdText}.";

    // @OpenForgeText doctor.phrase.changed-since-it-was-installed-by
    internal static string FormatChangedSinceItWasInstalledBy(string pathText, string extensionIdText)
        => $"{pathText} changed since it was installed by {extensionIdText}.";

    // @OpenForgeText doctor.phrase.the-package-folder-cannot-be-read
    internal static string FormatThePackageFolderCannotBeRead(string pathText)
        => $"The package folder {pathText} cannot be read.";

    // @OpenForgeText doctor.phrase.some-files-of-are-current-and-others-are-not
    internal static string FormatSomeFilesOfAreCurrentAndOthersAreNot(string extensionIdText)
        => $"Some files of {extensionIdText} are current and others are not.";

    // @OpenForgeText doctor.phrase.and-checked
    internal static string FormatAndChecked(string linksText, string cliTextPluralText, string routesCheckedText, string cliTextPluralText2)
        => $"{linksText} {cliTextPluralText} and {routesCheckedText} {cliTextPluralText2} checked.";

    // @OpenForgeText doctor.phrase.valid
    internal static string FormatValid(string linksValidText, string cliTextPluralText)
        => $"{linksValidText} {cliTextPluralText} valid";

    // @OpenForgeText doctor.phrase.checks-complete
    internal static string FormatChecksComplete(string completeText)
        => $"{completeText} checks complete.";

    // @OpenForgeText doctor.phrase.of-checks-complete
    internal static string FormatOfChecksComplete(string completeText, string checksText)
        => $"{completeText} of {checksText} checks complete.";

    // @OpenForgeText doctor.phrase.could-not-finish
    internal static string FormatCouldNotFinish(string remainingText, string cliTextPluralText)
        => $"{remainingText} {cliTextPluralText} could not finish.";

    // @OpenForgeText doctor.phrase.findings
    internal static string FormatFindings(string partsText)
        => $"{partsText} findings";

    // @OpenForgeText doctor.phrase.no-errors-and
    internal static string FormatNoErrorsAnd(string partsText, string recordedText)
        => $"No errors. {partsText} and {recordedText}.";

    // @OpenForgeText doctor.phrase.no-errors-recorded
    internal static string FormatNoErrorsRecorded(string partsText, string valueText)
        => $"No errors. {partsText} {valueText} recorded.";

    // @OpenForgeText doctor.phrase.no-problems-found
    internal static string FormatNoProblemsFound(string recordedText)
        => $"No problems found. {recordedText}.";

    // @OpenForgeText doctor.phrase.the-package-source-cannot-be-read
    internal static string FormatThePackageSourceCannotBeRead(string valueText)
        => $"the package source {valueText} cannot be read.";

    // @OpenForgeText doctor.phrase.checks-did-not-finish
    internal static string FormatChecksDidNotFinish(string categoryText)
        => $"{categoryText} checks did not finish";

    // @OpenForgeText doctor.phrase.the-linked-file-was-not-found
    internal static string FormatTheLinkedFileWasNotFound(string destinationText)
        => $"The linked file was not found: {destinationText}.";

    // @OpenForgeText doctor.phrase.the-linked-file-was-not-found-no-possible-target-was-found
    internal static string FormatTheLinkedFileWasNotFoundNoPossibleTargetWasFound(string destinationText)
        => $"The linked file was not found: {destinationText}. No possible target was found.";

    // @OpenForgeText doctor.phrase.has-no-heading
    internal static string FormatHasNoHeading(string fileText, string fragmentText)
        => $"{fileText} has no heading {fragmentText}.";

    // @OpenForgeText doctor.phrase.has-no-heading-no-possible-target-was-found
    internal static string FormatHasNoHeadingNoPossibleTargetWasFound(string fileText, string fragmentText)
        => $"{fileText} has no heading {fragmentText}. No possible target was found.";

    // @OpenForgeText doctor.phrase.could-not-be-parsed-so-was-not-checked
    internal static string FormatCouldNotBeParsedSoWasNotChecked(string fileText, string fragmentText)
        => $"{fileText} could not be parsed, so {fragmentText} was not checked.";

    // @OpenForgeText doctor.phrase.matches-the-heading-apart-from-spelling
    internal static string FormatMatchesTheHeadingApartFromSpelling(string authoredText, string canonicalText)
        => $"{authoredText} matches the heading {canonicalText} apart from spelling.";

    // @OpenForgeText doctor.phrase.links-to-which-does-not-exist
    internal static string FormatLinksToWhichDoesNotExist(string pathText, string targetText)
        => $"{pathText} links to {targetText}, which does not exist.";

    // @OpenForgeText doctor.phrase.no-longer-links-to-it-links-to
    internal static string FormatNoLongerLinksToItLinksTo(string pathText, string expectedText, string actualText)
        => $"{pathText} no longer links to {expectedText}; it links to {actualText}.";

    // @OpenForgeText doctor.phrase.the-source-folder-of
    internal static string FormatTheSourceFolderOf(string libraryIdText, string pathText, string phraseText)
        => $"The source folder of {libraryIdText}, {pathText}, {phraseText}.";

    // @OpenForgeText doctor.phrase.has-no-md-beside-it
    internal static string FormatHasNoMdBesideIt(string pathText, string basePathText)
        => $"{pathText} has no {basePathText}.md beside it.";

    // @OpenForgeText doctor.phrase.the-source-of-cannot-be-read-so-its-files-were-not-compared
    internal static string FormatTheSourceOfCannotBeReadSoItsFilesWereNotCompared(string extensionIdText, string pathText)
        => $"The source of {extensionIdText}, {pathText}, cannot be read, so its files were not compared.";

    // @OpenForgeText doctor.phrase.does-not-list-which-installed
    internal static string FormatDoesNotListWhichInstalled(string valueText, string pathText, string extensionIdText)
        => $"{valueText} does not list {pathText}, which {extensionIdText} installed.";

    // @OpenForgeText doctor.phrase.recorded
    internal static string FormatRecorded(string partsText, string cliTextPluralText, string valueText)
        => $"{partsText} {cliTextPluralText} {valueText} recorded";

    // @OpenForgeText doctor.word-list.pair
    internal static string WordListPair(string valueText, string valueText2)
        => $"{valueText} and {valueText2}";

    // @OpenForgeText doctor.word-list.final-item
    internal static string WordListFinalItem(string valueText)
        => $" and {valueText}";
}
