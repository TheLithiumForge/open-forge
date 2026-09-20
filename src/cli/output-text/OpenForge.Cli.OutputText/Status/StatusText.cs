using System.Globalization;

namespace OpenForge.Cli.OutputText.Status;

internal static class StatusText
{
    // @OpenForgeText status.message.open-forge-is-installed-and-current
    internal static string MessageOpenForgeIsInstalledAndCurrent()
        => "Open Forge is installed and current.";

    // @OpenForgeText status.message.status-was-cancelled
    internal static string MessageStatusWasCancelled()
        => "Status was cancelled.";

    // @OpenForgeText status.title.startup-context
    internal static string TitleStartupContext()
        => "Startup context";

    // @OpenForgeText status.heading.shipped-by-this-cli
    internal static string HeadingShippedByThisCli()
        => "Shipped by this CLI:";

    // @OpenForgeText status.heading.this-workspace
    internal static string HeadingThisWorkspace()
        => "This workspace:";

    // @OpenForgeText status.heading.may-load-again-later
    internal static string HeadingMayLoadAgainLater()
        => "May load again later:";

    // @OpenForgeText status.title.all-routed-files
    internal static string TitleAllRoutedFiles()
        => "All routed files";

    // @OpenForgeText status.title.routes
    internal static string TitleRoutes()
        => "Routes";

    // @OpenForgeText status.title.entries-sections
    internal static string TitleEntriesSections()
        => "Entries sections";

    // @OpenForgeText status.title.largest-may-load-again-sources
    internal static string TitleLargestMayLoadAgainSources()
        => "Largest may-load-again sources";

    // @OpenForgeText status.message.the-current-startup-context-could-not-be-measured
    internal static string MessageTheCurrentStartupContextCouldNotBeMeasured()
        => "The current startup context could not be measured.";

    // @OpenForgeText status.message.the-may-load-again-context-could-not-be-measured
    internal static string MessageTheMayLoadAgainContextCouldNotBeMeasured()
        => "The may-load-again context could not be measured.";

    // @OpenForgeText status.message.the-root-categories-could-not-be-read-from-agents-loader-md
    internal static string MessageTheRootCategoriesCouldNotBeReadFromAgentsLoaderMd()
        => "The root categories could not be read from .agents/loader.md.";

    // @OpenForgeText status.message.agents-md-could-not-be-read
    internal static string MessageAgentsMdCouldNotBeRead()
        => "AGENTS.md could not be read.";

    // @OpenForgeText status.message.no-ownership-record-exists-so-framework-files-cannot-be-checked-against-it
    internal static string MessageNoOwnershipRecordExistsSoFrameworkFilesCannotBeCheckedAgainstIt()
        => "No ownership record exists, so Framework files cannot be checked against it.";

    // @OpenForgeText status.message.no-ownership-record-exists-so-installed-extensions-cannot-be-listed-from-it
    internal static string MessageNoOwnershipRecordExistsSoInstalledExtensionsCannotBeListedFromIt()
        => "No ownership record exists, so installed Extensions cannot be listed from it.";

    // @OpenForgeText status.message.does-not-exist-or-cannot-be-read
    internal static string MessageDoesNotExistOrCannotBeRead()
        => "does not exist or cannot be read.";

    // @OpenForgeText status.message.is-not-a-directory
    internal static string MessageIsNotADirectory()
        => "is not a directory.";

    // @OpenForgeText status.message.preview-the-installation-for-this-workspace
    internal static string MessagePreviewTheInstallationForThisWorkspace()
        => "Preview the installation for this workspace.";

    // @OpenForgeText status.label.routed-files
    internal static string LabelRoutedFiles()
        => "routed files";

    // @OpenForgeText status.label.startup-files
    internal static string LabelStartupFiles()
        => "startup files";

    // @OpenForgeText status.label.startup-tokens
    internal static string LabelStartupTokens()
        => "startup tokens";

    // @OpenForgeText status.label.may-load-again-files
    internal static string LabelMayLoadAgainFiles()
        => "may-load-again files";

    // @OpenForgeText status.label.may-load-again-tokens
    internal static string LabelMayLoadAgainTokens()
        => "may-load-again tokens";

    // @OpenForgeText status.label.all-routed-tokens
    internal static string LabelAllRoutedTokens()
        => "all routed tokens";

    // @OpenForgeText status.label.startup-share
    internal static string LabelStartupShare()
        => "startup share";

    // @OpenForgeText status.label.root-categories
    internal static string LabelRootCategories()
        => "root categories";

    // @OpenForgeText status.label.current-entries-sections
    internal static string LabelCurrentEntriesSections()
        => "current Entries sections";

    // @OpenForgeText status.label.stale-entries-sections
    internal static string LabelStaleEntriesSections()
        => "stale Entries sections";

    // @OpenForgeText status.label.missing-entries-sections
    internal static string LabelMissingEntriesSections()
        => "missing Entries sections";

    // @OpenForgeText status.label.current-framework-files
    internal static string LabelCurrentFrameworkFiles()
        => "current Framework files";

    // @OpenForgeText status.label.changed-framework-files
    internal static string LabelChangedFrameworkFiles()
        => "changed Framework files";

    // @OpenForgeText status.label.missing-framework-files
    internal static string LabelMissingFrameworkFiles()
        => "missing Framework files";

    // @OpenForgeText status.label.registered-libraries
    internal static string LabelRegisteredLibraries()
        => "registered Libraries";

    // @OpenForgeText status.label.current-library-links
    internal static string LabelCurrentLibraryLinks()
        => "current Library links";

    // @OpenForgeText status.label.missing-library-links
    internal static string LabelMissingLibraryLinks()
        => "missing Library links";

    // @OpenForgeText status.label.changed-library-links
    internal static string LabelChangedLibraryLinks()
        => "changed Library links";

    // @OpenForgeText status.label.recovery-drafts
    internal static string LabelRecoveryDrafts()
        => "recovery drafts";

    // @OpenForgeText status.message.open-forge-is-installed-but-some-checks-could-not-finish
    internal static string MessageOpenForgeIsInstalledButSomeChecksCouldNotFinish()
        => "Open Forge is installed, but some checks could not finish.";

    // @OpenForgeText status.label.root-category
    internal static string LabelRootCategory()
        => "root category";

    // @OpenForgeText status.message.startup-context-could-not-be-measured-completely
    internal static string MessageStartupContextCouldNotBeMeasuredCompletely()
        => "Startup context could not be measured completely.";

    // @OpenForgeText status.message.the-recovery-store-could-not-be-read
    internal static string MessageTheRecoveryStoreCouldNotBeRead()
        => "The recovery store could not be read.";

    // @OpenForgeText status.message.the-entries-section-is-stale
    internal static string MessageTheEntriesSectionIsStale()
        => "The Entries section is stale.";

    // @OpenForgeText status.message.there-is-no-entries-section
    internal static string MessageThereIsNoEntriesSection()
        => "There is no Entries section.";

    // @OpenForgeText status.message.the-entries-section-could-not-be-read
    internal static string MessageTheEntriesSectionCouldNotBeRead()
        => "The Entries section could not be read.";

    // @OpenForgeText status.message.the-entries-section-could-not-be-read-safely
    internal static string MessageTheEntriesSectionCouldNotBeReadSafely()
        => "The Entries section could not be read safely.";

    // @OpenForgeText status.label.missing-it-was-installed-by-the-framework
    internal static string LabelMissingItWasInstalledByTheFramework()
        => "missing; it was installed by the Framework";

    // @OpenForgeText status.message.a-recovery-bundle-from-an-earlier-command-is-kept
    internal static string MessageARecoveryBundleFromAnEarlierCommandIsKept()
        => "A recovery bundle from an earlier command is kept.";

    // @OpenForgeText status.message.an-unfinished-recovery-draft-is-present-a-command-did-not-finish
    internal static string MessageAnUnfinishedRecoveryDraftIsPresentACommandDidNotFinish()
        => "An unfinished recovery draft is present. A command did not finish.";

    // @OpenForgeText status.message.the-recovery-bundle-is-damaged-and-cannot-be-used
    internal static string MessageTheRecoveryBundleIsDamagedAndCannotBeUsed()
        => "The recovery bundle is damaged and cannot be used.";

    // @OpenForgeText status.message.the-recovery-bundle-was-written-by-an-unsupported-version
    internal static string MessageTheRecoveryBundleWasWrittenByAnUnsupportedVersion()
        => "The recovery bundle was written by an unsupported version.";

    // @OpenForgeText status.message.the-recovery-bundle-could-not-be-read
    internal static string MessageTheRecoveryBundleCouldNotBeRead()
        => "The recovery bundle could not be read.";

    // @OpenForgeText status.message.the-library-section-of-agents-open-forge-lock-json-could-not-be-read
    internal static string MessageTheLibrarySectionOfAgentsOpenForgeLockJsonCouldNotBeRead()
        => "The Library section of .agents/open-forge.lock.json could not be read.";

    // @OpenForgeText status.title.entry-is-unavailable
    internal static string TitleEntryIsUnavailable()
        => "Entry is unavailable";

    // @OpenForgeText status.title.startup-context-is-incomplete
    internal static string TitleStartupContextIsIncomplete()
        => "Startup context is incomplete";

    // @OpenForgeText status.title.may-load-again-context-is-unavailable
    internal static string TitleMayLoadAgainContextIsUnavailable()
        => "May-load-again context is unavailable";

    // @OpenForgeText status.title.root-categories-are-unavailable
    internal static string TitleRootCategoriesAreUnavailable()
        => "Root categories are unavailable";

    // @OpenForgeText status.title.framework-ownership-is-unavailable
    internal static string TitleFrameworkOwnershipIsUnavailable()
        => "Framework ownership is unavailable";

    // @OpenForgeText status.title.extension-ownership-is-unavailable
    internal static string TitleExtensionOwnershipIsUnavailable()
        => "Extension ownership is unavailable";

    // @OpenForgeText status.title.library-ownership-is-unavailable
    internal static string TitleLibraryOwnershipIsUnavailable()
        => "Library ownership is unavailable";

    // @OpenForgeText status.title.framework-files-cannot-be-checked
    internal static string TitleFrameworkFilesCannotBeChecked()
        => "Framework files cannot be checked";

    // @OpenForgeText status.title.framework-files-could-not-be-checked-completely
    internal static string TitleFrameworkFilesCouldNotBeCheckedCompletely()
        => "Framework files could not be checked completely";

    // @OpenForgeText status.title.framework-files-cannot-be-checked-safely
    internal static string TitleFrameworkFilesCannotBeCheckedSafely()
        => "Framework files cannot be checked safely";

    // @OpenForgeText status.title.changed-since-it-was-installed
    internal static string TitleChangedSinceItWasInstalled()
        => "Changed since it was installed";

    // @OpenForgeText status.title.missing-managed-file
    internal static string TitleMissingManagedFile()
        => "Missing managed file";

    // @OpenForgeText status.title.managed-file-is-unavailable
    internal static string TitleManagedFileIsUnavailable()
        => "Managed file is unavailable";

    // @OpenForgeText status.title.managed-file-is-unsafe
    internal static string TitleManagedFileIsUnsafe()
        => "Managed file is unsafe";

    // @OpenForgeText status.title.extensions-cannot-be-checked
    internal static string TitleExtensionsCannotBeChecked()
        => "Extensions cannot be checked";

    // @OpenForgeText status.title.extensions-could-not-be-checked-completely
    internal static string TitleExtensionsCouldNotBeCheckedCompletely()
        => "Extensions could not be checked completely";

    // @OpenForgeText status.title.extensions-cannot-be-checked-safely
    internal static string TitleExtensionsCannotBeCheckedSafely()
        => "Extensions cannot be checked safely";

    // @OpenForgeText status.title.recovery-draft-is-incomplete
    internal static string TitleRecoveryDraftIsIncomplete()
        => "Recovery draft is incomplete";

    // @OpenForgeText status.title.recovery-bundle-is-unsupported
    internal static string TitleRecoveryBundleIsUnsupported()
        => "Recovery bundle is unsupported";

    // @OpenForgeText status.title.library-source-folder-is-ambiguous
    internal static string TitleLibrarySourceFolderIsAmbiguous()
        => "Library source folder is ambiguous";

    // @OpenForgeText status.title.library-source-folder-is-unavailable
    internal static string TitleLibrarySourceFolderIsUnavailable()
        => "Library source folder is unavailable";

    // @OpenForgeText status.title.missing-library-link
    internal static string TitleMissingLibraryLink()
        => "Missing Library link";

    // @OpenForgeText status.title.changed-library-link
    internal static string TitleChangedLibraryLink()
        => "Changed Library link";

    // @OpenForgeText status.title.library-link-is-unavailable
    internal static string TitleLibraryLinkIsUnavailable()
        => "Library link is unavailable";

    // @OpenForgeText status.title.library-link-is-unsafe
    internal static string TitleLibraryLinkIsUnsafe()
        => "Library link is unsafe";

    // @OpenForgeText status.title.library-and-extension-overlap
    internal static string TitleLibraryAndExtensionOverlap()
        => "Library and Extension overlap";

    // @OpenForgeText status.title.status-failed
    internal static string TitleStatusFailed()
        => "Status failed";

    // @OpenForgeText status.title.status-was-cancelled
    internal static string TitleStatusWasCancelled()
        => "Status was cancelled";

    // @OpenForgeText status.message.update-the-framework-files-that-need-attention
    internal static string MessageUpdateTheFrameworkFilesThatNeedAttention()
        => "Update the Framework files that need attention.";

    // @OpenForgeText status.message.update-the-extension-file-that-needs-attention
    internal static string MessageUpdateTheExtensionFileThatNeedsAttention()
        => "Update the Extension file that needs attention.";

    // @OpenForgeText status.message.inspect-the-extension-source-before-comparing-its-files
    internal static string MessageInspectTheExtensionSourceBeforeComparingItsFiles()
        => "Inspect the Extension source before comparing its files.";

    // @OpenForgeText status.message.synchronize-the-missing-library-link
    internal static string MessageSynchronizeTheMissingLibraryLink()
        => "Synchronize the missing Library link.";

    // @OpenForgeText status.message.inspect-the-library-source-and-link-boundary
    internal static string MessageInspectTheLibrarySourceAndLinkBoundary()
        => "Inspect the Library source and link boundary.";

    // @OpenForgeText status.message.rebuild-the-entries-section
    internal static string MessageRebuildTheEntriesSection()
        => "Rebuild the Entries section.";

    // @OpenForgeText status.message.review-and-remove-the-recovery-data
    internal static string MessageReviewAndRemoveTheRecoveryData()
        => "Review and remove the recovery data.";

    // @OpenForgeText status.message.correct-the-status-input-then-rerun-the-request
    internal static string MessageCorrectTheStatusInputThenRerunTheRequest()
        => "Correct the Status input, then rerun the request.";

    // @OpenForgeText status.message.rerun-the-same-status-request
    internal static string MessageRerunTheSameStatusRequest()
        => "Rerun the same Status request.";

    // @OpenForgeText status.message.inspect-the-reported-operational-facts-before-rerunning-status
    internal static string MessageInspectTheReportedOperationalFactsBeforeRerunningStatus()
        => "Inspect the reported operational facts before rerunning Status.";

    // @OpenForgeText status.label.not-available
    internal static string LabelNotAvailable()
        => "not available";

    // @OpenForgeText status.label.final
    internal static string LabelFinal()
        => "final";

    // @OpenForgeText status.label.draft
    internal static string LabelDraft()
        => "draft";

    // @OpenForgeText status.label.malformed
    internal static string LabelMalformed()
        => "malformed";

    // @OpenForgeText status.label.unsupported
    internal static string LabelUnsupported()
        => "unsupported";

    // @OpenForgeText status.label.incomplete
    internal static string LabelIncomplete()
        => "incomplete";

    // @OpenForgeText status.label.unavailable-tokens
    internal static string LabelUnavailableTokens()
        => "unavailable tokens";

    // @OpenForgeText status.label.the
    internal static string LabelThe()
        => "the";

    // @OpenForgeText status.help.syntax
    internal static string HelpSyntax()
        => "open-forge status [global options]";

    // @OpenForgeText status.help.inspection
    internal static string HelpInspection()
        => "Inspect the selected workspace's installation, startup and continuity context, generated navigation, lifecycle, managed targets, and exact recovery candidates.";

    // @OpenForgeText status.help.related-commands
    internal static string HelpRelatedCommands()
        => "open-forge doctor — diagnose unavailable or attention-requiring operational facts.\n  open-forge context — return startup or selected authored context.";

    // @OpenForgeText status.help.notes
    internal static string HelpNotes()
        => "Status is deterministic, stateless, and read-only. It does not acquire a workspace lock, repair lifecycle state, restore recovery data, or modify the workspace.";
}
