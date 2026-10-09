using System.Globalization;

namespace OpenForge.Cli.OutputText.Install;

internal static class InstallText
{
    // @OpenForgeText install.message.open-forge-is-already-installed-and-current-nothing-to-do
    internal static string MessageOpenForgeIsAlreadyInstalledAndCurrentNothingToDo()
        => "Open Forge is already installed and current. Nothing to do.";

    // @OpenForgeText install.message.install-was-cancelled-nothing-was-changed
    internal static string MessageInstallWasCancelledNothingWasChanged()
        => "Install was cancelled. Nothing was changed.";

    // @OpenForgeText install.message.created-agents-md-and-claude-md-with-an-open-forge-section
    internal static string MessageCreatedAgentsMdAndClaudeMdWithAnOpenForgeSection()
        => "Created AGENTS.md and CLAUDE.md with an Open Forge section.";

    // @OpenForgeText install.message.would-create-agents-md-and-claude-md-with-an-open-forge-section
    internal static string MessageWouldCreateAgentsMdAndClaudeMdWithAnOpenForgeSection()
        => "Would create AGENTS.md and CLAUDE.md with an Open Forge section.";

    // @OpenForgeText install.label.created-with-an-open-forge-section
    internal static string LabelCreatedWithAnOpenForgeSection()
        => "created with an Open Forge section";

    // @OpenForgeText install.label.would-be-created-with-an-open-forge-section
    internal static string LabelWouldBeCreatedWithAnOpenForgeSection()
        => "would be created with an Open Forge section";

    // @OpenForgeText install.message.nothing-that-already-exists-would-be-changed
    internal static string MessageNothingThatAlreadyExistsWouldBeChanged()
        => "Nothing that already exists would be changed.";

    // @OpenForgeText install.message.preview-replacing-the-occupied-framework-targets
    internal static string MessagePreviewReplacingTheOccupiedFrameworkTargets()
        => "Preview replacing the occupied Framework targets.";

    // @OpenForgeText install.message.update-the-existing-managed-framework-state-from-a-fresh-plan
    internal static string MessageUpdateTheExistingManagedFrameworkStateFromAFreshPlan()
        => "Update the existing managed Framework state from a fresh plan.";

    // @OpenForgeText install.message.rerun-the-same-install-request-with-explicit-automatic-mode
    internal static string MessageRerunTheSameInstallRequestWithExplicitAutomaticMode()
        => "Rerun the same Install request with explicit automatic mode.";

    // @OpenForgeText install.message.review-and-remove-the-reported-recovery-artifact-after-confirming-the-verified-install-result
    internal static string MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedInstallResult()
        => "Review and remove the reported recovery artifact after confirming the verified Install result.";

    // @OpenForgeText install.message.rerun-the-same-install-request
    internal static string MessageRerunTheSameInstallRequest()
        => "Rerun the same Install request.";

    // @OpenForgeText install.message.inspect-the-reported-install-boundary-before-rerunning-it
    internal static string MessageInspectTheReportedInstallBoundaryBeforeRerunningIt()
        => "Inspect the reported Install boundary before rerunning it.";

    // @OpenForgeText install.message.correct-the-named-install-input-then-rerun-the-request
    internal static string MessageCorrectTheNamedInstallInputThenRerunTheRequest()
        => "Correct the named Install input, then rerun the request.";

    // @OpenForgeText install.label.required-facts-are-unavailable
    internal static string LabelRequiredFactsAreUnavailable()
        => "required facts are unavailable";

    // @OpenForgeText install.label.the-workspace-is-unsafe
    internal static string LabelTheWorkspaceIsUnsafe()
        => "the workspace is unsafe";

    // @OpenForgeText install.help.syntax
    internal static string HelpSyntax()
        => "open-forge install [--configure] [--preset <essentials|full-core|custom>] [--frontmatter <root|scoped>] [--route <id>=<add|remove|git-ignore>...] [--force] [--automatic] [--dry-run] [global options]";

    // @OpenForgeText install.help.frontmatter
    internal static string HelpFrontmatter()
        => "Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure.";

    // @OpenForgeText install.help.heading.frontmatter
    internal static string HelpHeadingFrontmatter() => "Frontmatter";

    // @OpenForgeText install.help.establishment
    internal static string HelpEstablishment()
        => "Install adds selected bundled defaults to the workspace. Ordinary unattended first Install uses Full Core and retains existing omissions. An identical managed installation needs no changes. Explicit configuration can restore missing selected defaults and retains existing content. Use open-forge update to reconcile other managed changes.";

    // @OpenForgeText install.help.write-policy
    internal static string HelpWritePolicy()
        => "--force may replace one eligible existing item during initial installation. It never updates or adopts managed state. --automatic suppresses confirmation without adding force. --dry-run previews the same complete checked plan and writes nothing. Repeating the command makes no further changes.";

    // @OpenForgeText install.help.confirmation
    internal static string HelpConfirmation()
        => "First interactive Install offers a setup preset. --configure offers the current setup choices. A writing plan asks for confirmation once after selection and checks. Dry-run, unchanged installations, --automatic, JSON, and redirected requests never prompt. Redirected text requests that would write require --automatic.";

    // @OpenForgeText install.help.examples
    internal static string HelpExamples()
        => "open-forge install\n  open-forge install --automatic --dry-run --format json\n  open-forge install --force\n  open-forge install --force --automatic --dry-run\n  open-forge install --frontmatter scoped --automatic\n  open-forge install --configure --frontmatter root --dry-run";

    // @OpenForgeText install.help.notes
    internal static string HelpNotes()
        => "Ordinary Install delivers only the Framework payload embedded in the running CLI. With a form change, --configure also converts eligible owned Extension files from their recorded sources. Install does not discover another workspace, fetch content, manipulate Git, repair markers, reconcile managed divergence, or roll back target effects.";

    // @OpenForgeText install.label.sections-added
    internal static string LabelSectionsAdded()
        => "sections added";

    // @OpenForgeText install.message.listed-in-agents-open-forge-lock-json
    internal static string MessageListedInAgentsOpenForgeLockJson()
        => "(listed in .agents/open-forge.lock.json).";

    // @OpenForgeText install.message.plus-agents-md-and-claude-md
    internal static string MessagePlusAgentsMdAndClaudeMd()
        => ", plus AGENTS.md and CLAUDE.md.";

    // @OpenForgeText install.label.would-be-created
    internal static string LabelWouldBeCreated()
        => "would be created";

    // @OpenForgeText install.label.would-append-the-open-forge-section
    internal static string LabelWouldAppendTheOpenForgeSection()
        => "would append the Open Forge section";

    // @OpenForgeText install.title.open-forge-section-added
    internal static string TitleOpenForgeSectionAdded()
        => "Open Forge section added";

    // @OpenForgeText install.label.created-records-the-files-above
    internal static string LabelCreatedRecordsTheFilesAbove()
        => "created; records the files above";

    // @OpenForgeText install.label.ownership-receipt-created
    internal static string LabelOwnershipReceiptCreated()
        => "created as the ownership receipt";

    // @OpenForgeText install.migration.action-label
    internal static string MigrationActionLabel(string action)
        => action switch
        {
            "metadata-completed" => "metadata updated",
            "entrypoint-created" => "entrypoint created",
            "entries-section-added" => "Entries section added",
            "navigation-updated" => "navigation updated",
            "content-preserved" => "content preserved",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Install migration action is not defined."),
        };

    // @OpenForgeText install.migration.actions
    internal static string FormatMigrationActions(IEnumerable<string> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        var values = actions
            .Select(action => action ?? throw new ArgumentException(
                "Install migration action labels cannot contain null members.",
                nameof(actions)))
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException("An Install migration row must contain an action.", nameof(actions));
        }

        return string.Join(", ", values);
    }

    // @OpenForgeText install.migration.row
    internal static string FormatMigrationRow(string outcome, string actions)
        => outcome switch
        {
            "planned" => $"Planned migration: {actions}",
            "applied" => $"Migrated: {actions}",
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Install migration outcome is not defined."),
        };

    // @OpenForgeText install.migration.summary
    internal static string FormatMigrationSummary(string outcome, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        var countText = count.ToString(CultureInfo.InvariantCulture);
        var source = count == 1 ? "source" : "sources";
        return outcome switch
        {
            "planned" => $"Planned migration for {countText} {source}.",
            "applied" => $"Migrated {countText} {source}.",
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Install migration outcome is not defined."),
        };
    }

    // @OpenForgeText install.message.count-1-file-was-created
    internal static string MessageCount1FileWasCreated()
        => "1 file was created.";

    // @OpenForgeText install.message.count-1-directory-was-created
    internal static string MessageCount1DirectoryWasCreated()
        => "1 directory was created.";

    // @OpenForgeText install.message.count-1-file-and-1-directory-were-created
    internal static string MessageCount1FileAnd1DirectoryWereCreated()
        => "1 file and 1 directory were created.";

    // @OpenForgeText install.label.section
    internal static string LabelSection()
        => "section";

    // @OpenForgeText install.message.no-ownership-record-change-was-requested
    internal static string MessageNoOwnershipRecordChangeWasRequested()
        => "No ownership record change was requested.";

    // @OpenForgeText install.message.the-existing-ownership-record-was-preserved
    internal static string MessageTheExistingOwnershipRecordWasPreserved()
        => "The existing ownership record was preserved.";

    // @OpenForgeText install.message.the-existing-ownership-record-was-preserved-because-the-installation-is-current
    internal static string MessageTheExistingOwnershipRecordWasPreservedBecauseTheInstallationIsCurrent()
        => "The existing ownership record was preserved because the installation is current.";

    // @OpenForgeText install.message.the-ownership-record-is-planned-for-publication
    internal static string MessageTheOwnershipRecordIsPlannedForPublication()
        => "The ownership record is planned for publication.";

    // @OpenForgeText install.message.ownership-record-publication-did-not-start
    internal static string MessageOwnershipRecordPublicationDidNotStart()
        => "Ownership record publication did not start.";

    // @OpenForgeText install.message.the-ownership-record-was-published-and-verified
    internal static string MessageTheOwnershipRecordWasPublishedAndVerified()
        => "The ownership record was published and verified.";

    // @OpenForgeText install.message.ownership-record-publication-failed-verification
    internal static string MessageOwnershipRecordPublicationFailedVerification()
        => "Ownership record publication failed verification.";

    // @OpenForgeText install.message.the-final-state-of-ownership-record-publication-is-unknown
    internal static string MessageTheFinalStateOfOwnershipRecordPublicationIsUnknown()
        => "The final state of ownership record publication is unknown.";

    // @OpenForgeText install.message.the-recovery-bundle-state-is-unavailable
    internal static string MessageTheRecoveryBundleStateIsUnavailable()
        => "The recovery bundle state is unavailable.";

    // @OpenForgeText install.title.framework-files-have-changed
    internal static string TitleFrameworkFilesHaveChanged()
        => "Framework files have changed";

    // @OpenForgeText install.title.ownership-record-is-invalid
    internal static string TitleOwnershipRecordIsInvalid()
        => "Ownership record is invalid";

    // @OpenForgeText install.title.recovery-data-blocks-installation
    internal static string TitleRecoveryDataBlocksInstallation()
        => "Recovery data blocks installation";

    // @OpenForgeText install.title.installation-projection-is-unavailable
    internal static string TitleInstallationProjectionIsUnavailable()
        => "Installation projection is unavailable";

    // @OpenForgeText install.title.install-failed
    internal static string TitleInstallFailed()
        => "Install failed";

    // @OpenForgeText install.title.install-was-cancelled
    internal static string TitleInstallWasCancelled()
        => "Install was cancelled";

    // @OpenForgeText install.message.open-forge-update-reconcile-an-existing-managed-framework-installation-open-forge-doctor-inspect-blocked-or-unavailable-lifecycle-and-safety-facts-open-forge-cleanup-remove-a-reported-retained-recovery-artifact-after-review
    internal static string MessageOpenForgeUpdateReconcileAnExistingManagedFrameworkInstallationOpenForgeDoctorInspectBlockedOrUnavailableLifecycleRetainedRecoveryArtifactAfterReview()
        => "open-forge update — reconcile an existing managed Framework installation.\n  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n  open-forge cleanup — remove a reported retained recovery artifact after review.";

    // @OpenForgeText install.label.directories
    internal static string LabelDirectories()
        => "directories";

    // @OpenForgeText install.label.sections
    internal static string LabelSections()
        => "sections";

    // @OpenForgeText install.title.install
    internal static string TitleInstall()
        => "Install";

    // @OpenForgeText install.label.install
    internal static string LabelInstall()
        => "install";

    // @OpenForgeText install.help.heading.establishment
    internal static string HelpHeadingEstablishment()
        => "Establishment";

    // @OpenForgeText install.help.heading.confirmation
    internal static string HelpHeadingConfirmation()
        => "Confirmation";
}
