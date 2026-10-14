using System.Globalization;

namespace OpenForge.Cli.OutputText.Update;

internal static class UpdateText
{
    // @OpenForgeText update.wording.changed-source-files
    internal static string ChangedSourceFiles(int count, bool preview)
        => string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"{(preview ? "Would change" : "Changed")} {count} source {(count == 1 ? "file" : "files")}.");

    // @OpenForgeText update.wording.previous-content-in-bundle
    internal static string PreviousContentInBundle(string path)
        => $"Previous content is in the recovery bundle at {path}.";

    // @OpenForgeText update.wording.previous-content-will-be-kept
    internal static string PreviousContentWillBeKept()
        => "Previous content will be kept in a recovery bundle.";

    // @OpenForgeText update.wording.metadata-completed
    internal static string MetadataCompleted(bool preview)
        => preview ? "metadata would be completed, your content would be kept" : "metadata completed, your content was kept";

    // @OpenForgeText update.wording.local-entrypoint-created
    internal static string LocalEntrypointCreated(bool preview)
        => preview ? "local entrypoint would be created" : "local entrypoint created";

    // @OpenForgeText update.wording.content-kept
    internal static string ContentKept(bool preview)
        => preview ? "your content would be kept" : "your content was kept";
    // @OpenForgeText update.message.the-framework-is-up-to-date-nothing-to-do
    internal static string MessageTheFrameworkIsUpToDateNothingToDo()
        => "The Framework is up to date. Nothing to do.";

    // @OpenForgeText update.message.update-was-cancelled-nothing-was-changed
    internal static string MessageUpdateWasCancelledNothingWasChanged()
        => "Update was cancelled. Nothing was changed.";

    // @OpenForgeText update.message.no-ownership-record-exists-so-update-cannot-tell-which-files-it-manages-nothing-was-changed
    internal static string MessageNoOwnershipRecordExistsSoUpdateCannotTellWhichFilesItManagesNothingWasChanged()
        => "No ownership record exists, so update cannot tell which files it manages. Nothing was changed.";

    // @OpenForgeText update.message.no-ownership-record-exists-agents-open-forge-lock-json-is-missing-so-update-cannot-tell-which-files-it-manages
    internal static string MessageNoOwnershipRecordExistsAgentsOpenForgeLockJsonIsMissingSoUpdateCannotTellWhichFilesItManages()
        => "No ownership record exists (.agents/open-forge.lock.json is missing), so update cannot tell which files it manages.";

    // @OpenForgeText update.message.the-ownership-record-names-files-that-cannot-be-interpreted-so-nothing-was-changed
    internal static string MessageTheOwnershipRecordNamesFilesThatCannotBeInterpretedSoNothingWasChanged()
        => "The ownership record names files that cannot be interpreted, so nothing was changed.";

    // @OpenForgeText update.label.new-content-in-this-release
    internal static string LabelNewContentInThisRelease()
        => "new content in this release";

    // @OpenForgeText update.label.you-had-changed-it-and-this-release-changes-it
    internal static string LabelYouHadChangedItAndThisReleaseChangesIt()
        => "you had changed it and this release changes it";

    // @OpenForgeText update.label.new-in-this-release
    internal static string LabelNewInThisRelease()
        => "new in this release";

    // @OpenForgeText update.label.no-longer-part-of-this-release
    internal static string LabelNoLongerPartOfThisRelease()
        => "no longer part of this release";

    // @OpenForgeText update.label.you-have-changed-it-a-recovery-bundle-is-written-first
    internal static string LabelYouHaveChangedItARecoveryBundleIsWrittenFirst()
        => "you have changed it; a recovery bundle is written first";

    // @OpenForgeText update.label.kept-no-longer-part-of-this-release
    internal static string LabelKeptNoLongerPartOfThisRelease()
        => "kept; no longer part of this release";

    // @OpenForgeText update.title.previous-content-git-diff
    internal static string TitlePreviousContentGitDiff()
        => "Previous content: git diff";

    // @OpenForgeText update.message.correct-the-named-update-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedUpdateInputThenRerunTheRequest()
        => "Correct the named Update input, then rerun the request.";

    // @OpenForgeText update.message.rerun-the-same-update-request-with-explicit-automatic-mode
    internal static string MessageRerunTheSameUpdateRequestWithExplicitAutomaticMode()
        => "Rerun the same Update request with explicit automatic mode.";

    // @OpenForgeText update.message.preview-deleting-the-retained-files-before-applying-the-prune
    internal static string MessagePreviewDeletingTheRetainedFilesBeforeApplyingThePrune()
        => "Preview deleting the retained files before applying the prune.";

    // @OpenForgeText update.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-update-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedUpdateResult()
        => "Review and remove the reported recovery artifact after confirming the verified Update result.";

    // @OpenForgeText update.message.inspect-the-blocked-unavailable-or-incomplete-update-facts-before-rerunning
    internal static string MessageInspectTheBlockedUnavailableOrIncompleteUpdateFactsBeforeRerunning()
        => "Inspect the blocked, unavailable, or incomplete Update facts before rerunning.";

    // @OpenForgeText update.message.rerun-the-same-update-request
    internal static string MessageRerunTheSameUpdateRequest()
        => "Rerun the same Update request.";

    // @OpenForgeText update.label.required-information-was-unavailable
    internal static string LabelRequiredInformationWasUnavailable()
        => "required information was unavailable";

    // @OpenForgeText update.label.the-input-was-invalid
    internal static string LabelTheInputWasInvalid()
        => "the input was invalid";

    // @OpenForgeText update.label.the-workspace-is-blocked
    internal static string LabelTheWorkspaceIsBlocked()
        => "the workspace is blocked";

    // @OpenForgeText update.title.workspace-unavailable
    internal static string TitleWorkspaceUnavailable()
        => "Workspace unavailable";

    // @OpenForgeText update.title.workspace-unsafe
    internal static string TitleWorkspaceUnsafe()
        => "Workspace unsafe";

    // @OpenForgeText update.title.framework-unavailable
    internal static string TitleFrameworkUnavailable()
        => "Framework unavailable";

    // @OpenForgeText update.title.framework-invalid
    internal static string TitleFrameworkInvalid()
        => "Framework invalid";

    // @OpenForgeText update.title.ownership-record-missing
    internal static string TitleOwnershipRecordMissing()
        => "Ownership record missing";

    // @OpenForgeText update.title.ownership-record-unavailable
    internal static string TitleOwnershipRecordUnavailable()
        => "Ownership record unavailable";

    // @OpenForgeText update.title.ownership-record-blocked
    internal static string TitleOwnershipRecordBlocked()
        => "Ownership record blocked";

    // @OpenForgeText update.title.ownership-record-observed
    internal static string TitleOwnershipRecordObserved()
        => "Ownership record observed";

    // @OpenForgeText update.title.target-unavailable
    internal static string TitleTargetUnavailable()
        => "Target unavailable";

    // @OpenForgeText update.title.target-unsafe
    internal static string TitleTargetUnsafe()
        => "Target unsafe";

    // @OpenForgeText update.title.source-provenance-invalid
    internal static string TitleSourceProvenanceInvalid()
        => "Source provenance invalid";

    // @OpenForgeText update.title.fingerprint-unavailable
    internal static string TitleFingerprintUnavailable()
        => "Fingerprint unavailable";

    // @OpenForgeText update.title.retired-content-kept
    internal static string TitleRetiredContentKept()
        => "Retired content kept";

    // @OpenForgeText update.title.retirement-blocked
    internal static string TitleRetirementBlocked()
        => "Retirement blocked";

    // @OpenForgeText update.title.entries-projection-unavailable
    internal static string TitleEntriesProjectionUnavailable()
        => "Entries projection unavailable";

    // @OpenForgeText update.title.entries-section-unsafe
    internal static string TitleEntriesSectionUnsafe()
        => "Entries section unsafe";

    // @OpenForgeText update.title.plan-blocked
    internal static string TitlePlanBlocked()
        => "Plan blocked";

    // @OpenForgeText update.title.recovery-conflict
    internal static string TitleRecoveryConflict()
        => "Recovery conflict";

    // @OpenForgeText update.title.recovery-unavailable
    internal static string TitleRecoveryUnavailable()
        => "Recovery unavailable";

    // @OpenForgeText update.title.recovery-artifact-retained
    internal static string TitleRecoveryArtifactRetained()
        => "Recovery artifact retained";

    // @OpenForgeText update.title.update-failed
    internal static string TitleUpdateFailed()
        => "Update failed";

    // @OpenForgeText update.title.update-cancelled
    internal static string TitleUpdateCancelled()
        => "Update cancelled";

    // @OpenForgeText update.label.replace
    internal static string LabelReplace()
        => "replace";

    // @OpenForgeText update.label.restored
    internal static string LabelRestored()
        => "restored";

    // @OpenForgeText update.label.deletion
    internal static string Deleted(bool preview)
        => preview ? "would be deleted" : Shared.SharedText.LabelDeleted();

    // @OpenForgeText update.label.restoration
    internal static string Restored(bool preview)
        => preview ? "would be restored" : LabelRestored();

    // @OpenForgeText update.label.created-with-section
    internal static string CreatedWithSection(bool preview)
        => preview ? "would be created with an Open Forge section" : "created with an Open Forge section";

    // @OpenForgeText update.label.migration-metadata-completed
    internal static string LabelMigrationMetadataCompleted()
        => "metadata updated";

    // @OpenForgeText update.label.migration-entrypoint-created
    internal static string LabelMigrationEntrypointCreated()
        => "local entrypoint created";

    // @OpenForgeText update.label.migration-entries-section-added
    internal static string LabelMigrationEntriesSectionAdded()
        => "Entries section added";

    // @OpenForgeText update.label.migration-navigation-updated
    internal static string LabelMigrationNavigationUpdated()
        => "navigation updated";

    // @OpenForgeText update.label.migration-content-preserved
    internal static string LabelMigrationContentPreserved()
        => "content preserved";

    // @OpenForgeText update.migration.actions
    internal static string FormatMigrationActions(IEnumerable<string> actions)
        => string.Join(", ", actions.Select(MigrationActionLabel));

    // @OpenForgeText update.migration.row
    internal static string FormatMigrationRow(bool applied, string actions)
        => applied
            ? $"Migrated: {actions}"
            : $"Planned migration: {actions}";

    // @OpenForgeText update.migration.summary
    internal static string FormatMigrationSummary(bool applied, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        if (applied && count == 1)
        {
            return "Migrated 1 source.";
        }

        if (applied)
        {
            return string.Create(CultureInfo.InvariantCulture, $"Migrated {count} sources.");
        }

        if (count == 1)
        {
            return "Planned migration for 1 source.";
        }

        return string.Create(CultureInfo.InvariantCulture, $"Planned migrations for {count} sources.");
    }

    // @OpenForgeText update.migration.headline
    internal static string FormatMigrationHeadline(int appliedCount, int plannedCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(appliedCount);
        ArgumentOutOfRangeException.ThrowIfNegative(plannedCount);
        if (appliedCount == 0 && plannedCount == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(appliedCount));
        }

        if (appliedCount > 0 && plannedCount > 0)
        {
            return $"{FormatMigrationSummary(true, appliedCount)} {FormatMigrationSummary(false, plannedCount)}";
        }

        if (appliedCount > 0)
        {
            return FormatMigrationSummary(true, appliedCount);
        }

        return FormatMigrationSummary(false, plannedCount);
    }

    private static string MigrationActionLabel(string action)
        => action switch
        {
            "metadata-completed" => LabelMigrationMetadataCompleted(),
            "entrypoint-created" => LabelMigrationEntrypointCreated(),
            "entries-section-added" => LabelMigrationEntriesSectionAdded(),
            "navigation-updated" => LabelMigrationNavigationUpdated(),
            "content-preserved" => LabelMigrationContentPreserved(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Update migration action is not defined."),
        };

    // @OpenForgeText update.title.update
    internal static string TitleUpdate()
        => "Update";

    // @OpenForgeText update.help.syntax
    internal static string HelpSyntax()
        => "open-forge update [options]";

    // @OpenForgeText update.help.heading.reconciliation
    internal static string HelpHeadingReconciliation()
        => "Updating managed files";

    // @OpenForgeText update.help.reconciliation
    internal static string HelpReconciliation()
        => "Update compares the managed Framework installation with the complete version bundled in this CLI. Current content that already matches needs no writes.";

    // @OpenForgeText update.help.authority
    internal static string HelpAuthority()
        => "Ordinary Update replaces changed or restores missing eligible current managed content. --prune may delete eligible retired managed content. The recovery bundle retains previous bytes. --automatic skips confirmation.";

    // @OpenForgeText update.help.heading.execution
    internal static string HelpHeadingExecution()
        => "Execution";

    // @OpenForgeText update.help.execution
    internal static string HelpExecution()
        => "--automatic suppresses confirmation without adding force or prune. --dry-run previews the same complete checked plan and writes nothing. An interactive text request asks once after checks if it would write files. JSON and redirected execution never prompt.";

    // @OpenForgeText update.help.examples
    internal static string HelpExamples()
        => "open-forge update --dry-run\nopen-forge update\nopen-forge update --prune --dry-run";

    // @OpenForgeText update.help.related-commands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("install --help", "Install or configure the Framework."),
            ("doctor", "Diagnose workspace problems."),
            ("cleanup --dry-run", "Preview recovery-data cleanup."),
        ];

    // @OpenForgeText update.help.notes
    internal static string HelpNotes()
        => "Update requires complete Framework management and uses the bundled files in the running CLI. It can adopt compatible missing metadata or required entrypoints in selected standard local routes. It does not establish absent or unknown Framework management, claim whole-file ownership of adopted files, fetch content, manipulate Git, or remove recovery artifacts owned by Cleanup.";

    // @OpenForgeText update.update.help.behavior
    internal static string HelpBehavior()
        => "Matching content needs no writes. Changed or missing eligible managed content is updated or restored.\n--dry-run shows the checked plan and writes nothing.\nAn interactive writing plan asks for confirmation unless --automatic is supplied.\nRedirected or JSON requests that would write need --automatic.\n--automatic adds no force or prune permission. Previous bytes are kept in the recovery bundle.";
    // @OpenForgeText update.help.details-1
    internal static string HelpAuthorityDetails()
        => "Interactive confirmation happens once after checks. JSON and redirected requests never prompt.";

    // @OpenForgeText update.help.details-2
    internal static string HelpManagementBoundaryDetails()
        => "Update requires complete Framework management and uses this CLI's bundled version. It may adopt compatible missing metadata or required entrypoints in selected standard local routes, without claiming whole-file ownership of adopted files. It cannot establish absent or unknown management, fetch content, change Git, or delete recovery artifacts owned by Cleanup.";
}
