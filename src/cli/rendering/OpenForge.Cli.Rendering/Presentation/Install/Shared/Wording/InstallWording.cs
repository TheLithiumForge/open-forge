using System.Globalization;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Wording;

internal static class InstallWording
{
    internal static string Confirmation(InstallConfirmationFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (facts.ReplacementCount > 0)
        {
            var noun = facts.ReplacementCount == 1 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFile() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFiles();
            return global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatReplaceTheExistingListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{facts.ReplacementCount}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"));
        }

        return CliPromptWording.Confirm();
    }

    internal static string Installed(string workspace, int replacements)
        => replacements > 0
            ? global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatInstalledTheOpenForgeFrameworkIntoReplacingExisting(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{workspace}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{replacements}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(replacements, "file")}"))
            : global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatInstalledTheOpenForgeFrameworkInto($"{workspace}");

    internal static string Preview(string workspace, int replacements)
        => replacements > 0
            ? global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWouldInstallTheOpenForgeFrameworkIntoReplacingExisting(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{workspace}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{replacements}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(replacements, "file")}"))
            : global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWouldInstallTheOpenForgeFrameworkInto($"{workspace}");

    internal static string AlreadyCurrent() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageOpenForgeIsAlreadyInstalledAndCurrentNothingToDo();

    internal static string Incomplete(string limitation)
        => global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatInstallCouldNotStartNothingWasChanged($"{TrimSentence(limitation)}");

    internal static string BlockedOccupied(int count)
        => global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatCannotInstallAlreadyWhereTheFrameworkWouldWrite(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Verb(count, "exists", "exist")}"));

    internal static string BlockedDivergence(int count)
        => global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatCannotInstallFrameworkChangedSinceTheyWereInstalled(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(count, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Verb(count, "has", "have")}"));

    internal static string Blocked(string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInstall($"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Install.InstallWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageInstallWasCancelledNothingWasChanged();

    internal static string CancelledAfter(int completed, int total)
        => global::OpenForge.Cli.OutputText.Install.InstallWording.CancelledAfter(completed, total);

    internal static string NoChanges() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string CreatedSummary(int files, int directories, bool lockVerified)
    {
        var summary = global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatCreatedAndUnderAgents(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(files, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{directories}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(directories, "directory")}"));
        return lockVerified
            ? summary + (" " + global::OpenForge.Cli.OutputText.Install.InstallText.MessageListedInAgentsOpenForgeLockJson())
            : summary + ".";
    }

    internal static string WouldCreateSummary(int files, int directories, bool bothHostFiles)
    {
        var summary = global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWouldCreateAndUnderAgents(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(files, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{directories}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliTextPlural(directories, "directory")}"));
        return bothHostFiles
            ? summary + global::OpenForge.Cli.OutputText.Install.InstallText.MessagePlusAgentsMdAndClaudeMd()
            : summary + ".";
    }

    internal static string CreatedHostFiles() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageCreatedAgentsMdAndClaudeMdWithAnOpenForgeSection();

    internal static string WouldCreateHostFiles() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageWouldCreateAgentsMdAndClaudeMdWithAnOpenForgeSection();

    internal static string CreatedHostFile() => global::OpenForge.Cli.OutputText.Install.InstallText.LabelCreatedWithAnOpenForgeSection();

    internal static string WouldCreateHostFile() => global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldBeCreatedWithAnOpenForgeSection();

    internal static string AppendedHostFile(bool preview)
        => preview
            ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldAddAnOpenForgeSectionYourContentWouldBeKept()
            : global::OpenForge.Cli.OutputText.Install.InstallText.TitleOpenForgeSectionAddedYourContentWasKept();

    internal static string CreatedFile(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldBeCreated() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreated();

    internal static string CreatedDirectory(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldBeCreated() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreated();

    internal static string AppendedSection(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldAppendTheOpenForgeSection() : global::OpenForge.Cli.OutputText.Install.InstallText.TitleOpenForgeSectionAdded();

    internal static string ReplacedFile(bool preview)
        => preview ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldBeReplaced() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelReplaced();

    internal static string NotStartedFile() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();

    internal static string UnknownFile() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown();

    internal static string FailedFile() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed();

    internal static string WriteFailed(string path, string reason)
        => global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWritingFailed($"{path}", $"{TrimSentence(reason)}");

    internal static string VerificationFailed(string path, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatDidNotVerifyAfterItWasWritten($"{path}", $"{TrimSentence(reason)}");

    internal static string LockCreated(bool preview)
        => preview
            ? global::OpenForge.Cli.OutputText.Install.InstallText.LabelWouldBeCreated()
            : global::OpenForge.Cli.OutputText.Install.InstallText.LabelCreatedRecordsTheFilesAbove();

    internal static string PartialSummary(int files, int directories)
    {
        if (files == 1 && directories == 0)
            return global::OpenForge.Cli.OutputText.Install.InstallText.MessageCount1FileWasCreated();
        if (files == 0 && directories == 1)
            return global::OpenForge.Cli.OutputText.Install.InstallText.MessageCount1DirectoryWasCreated();
        if (files == 1 && directories == 1)
            return global::OpenForge.Cli.OutputText.Install.InstallText.MessageCount1FileAnd1DirectoryWereCreated();
        if (directories == 0)
            return global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatFilesWereCreated(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"));
        if (files == 0)
            return global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatDirectoriesWereCreated(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{directories}"));

        var total = files + directories;
        return global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatFilesAndDirectoriesWereCreated(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{total}"));
    }

    internal static string DirectorySummary(int count, bool preview)
    {
        var noun = CliTextPlural(count, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDirectory());
        return preview
            ? global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWouldCreate(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"))
            : global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatCreated(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"));
    }

    internal static string SectionsSummary(int count, bool preview)
    {
        var noun = CliTextPlural(count, global::OpenForge.Cli.OutputText.Install.InstallText.LabelSection());
        return preview
            ? global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatWouldAdd(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"))
            : global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatAdded(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{noun}"));
    }

    internal static string NoExistingChanges() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageNothingThatAlreadyExistsWouldBeChanged();

    internal static string SourceAsset(string path) => global::OpenForge.Cli.OutputText.Install.InstallWording.SourceAsset(path);

    internal static string Lifecycle(InstallLifecycleAction action, InstallLifecycleOutcome outcome)
        => (action, outcome) switch
        {
            (InstallLifecycleAction.None, InstallLifecycleOutcome.NotRequested)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageNoOwnershipRecordChangeWasRequested(),
            (InstallLifecycleAction.Preserve, InstallLifecycleOutcome.NotRequested)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheExistingOwnershipRecordWasPreserved(),
            (InstallLifecycleAction.Preserve, InstallLifecycleOutcome.AlreadyCurrent)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheExistingOwnershipRecordWasPreservedBecauseTheInstallationIsCurrent(),
            (InstallLifecycleAction.Publish, InstallLifecycleOutcome.Planned)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheOwnershipRecordIsPlannedForPublication(),
            (InstallLifecycleAction.Publish, InstallLifecycleOutcome.NotStarted)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageOwnershipRecordPublicationDidNotStart(),
            (InstallLifecycleAction.Publish, InstallLifecycleOutcome.Verified)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheOwnershipRecordWasPublishedAndVerified(),
            (InstallLifecycleAction.Publish, InstallLifecycleOutcome.VerificationFailed)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageOwnershipRecordPublicationFailedVerification(),
            (InstallLifecycleAction.Publish, InstallLifecycleOutcome.CompletionUnknown)
                => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheFinalStateOfOwnershipRecordPublicationIsUnknown(),
            _ => global::OpenForge.Cli.OutputText.Install.InstallPhrases.FormatOwnershipRecordLifecycleIs(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{InstallWireVocabulary.Name(action)}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{InstallWireVocabulary.Name(outcome)}")),
        };

    internal static string RecoveryDetail(InstallResultRecoveryState state, string? path)
        => state switch
        {
            InstallResultRecoveryState.NotRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasRequired(),
            InstallResultRecoveryState.NotCreated => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasCreated(),
            InstallResultRecoveryState.Removed => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRemoved(),
            InstallResultRecoveryState.Retained when path is not null => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt($"{path}"),
            InstallResultRecoveryState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalStateOfTheRecoveryBundleIsUnknown(),
            _ => global::OpenForge.Cli.OutputText.Install.InstallText.MessageTheRecoveryBundleStateIsUnavailable(),
        };

    internal static string Verification(InstallResultVerificationState state)
        => state switch
        {
            InstallResultVerificationState.NotRequested => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageVerificationWasNotRequested(),
            InstallResultVerificationState.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAllWrittenTargetsWereVerified(),
            InstallResultVerificationState.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageWrittenTargetsDidNotVerify(),
            InstallResultVerificationState.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalVerificationStateIsUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Install verification state is not defined."),
        };

    internal static string RecoveryData(string path) => global::OpenForge.Cli.OutputText.Install.InstallWording.RecoveryData(path);

    internal static string NextOccupiedCommand() => "open-forge install --force --dry-run";
    internal static string NextOccupiedReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessagePreviewReplacingTheOccupiedFrameworkTargets();
    internal static string NextDivergenceCommand() => "open-forge update";
    internal static string NextDivergenceReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageUpdateTheExistingManagedFrameworkStateFromAFreshPlan();
    internal static string NextConfirmationCommand(bool force)
        => force ? "open-forge install --force --automatic" : "open-forge install --automatic";
    internal static string NextConfirmationReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageRerunTheSameInstallRequestWithExplicitAutomaticMode();
    internal static string NextRecoveryCommand() => "open-forge cleanup";
    internal static string NextRecoveryReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedInstallResult();
    internal static string NextInterruptedCommand() => "open-forge install";
    internal static string NextInterruptedReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageRerunTheSameInstallRequest();
    internal static string NextDoctorCommand() => "open-forge doctor";
    internal static string NextDoctorReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageInspectTheReportedInstallBoundaryBeforeRerunningIt();
    internal static string NextHelpCommand() => "open-forge install --help";
    internal static string NextHelpReason() => global::OpenForge.Cli.OutputText.Install.InstallText.MessageCorrectTheNamedInstallInputThenRerunTheRequest();
    internal static string ManagedDivergence(string path) => global::OpenForge.Cli.OutputText.Install.InstallWording.ManagedDivergence(path);
    internal static string DefaultIncompleteReason() => global::OpenForge.Cli.OutputText.Install.InstallText.LabelRequiredFactsAreUnavailable();
    internal static string DefaultBlockedReason() => global::OpenForge.Cli.OutputText.Install.InstallText.LabelTheWorkspaceIsUnsafe();
    internal static string DefaultInvalidReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheInputIsInvalid();

    internal static string Recovery(string path) => RecoveryData(path);

    internal static string RecoveryRemoved() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRemoved();

    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Install.InstallWording.RecoveryRetained(path);

    internal static string RecoveryUnknown() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalStateOfTheRecoveryBundleIsUnknown();

    internal static string Verification(string state) => global::OpenForge.Cli.OutputText.Install.InstallWording.Verification(state);

    internal static string Source(string fingerprint, int assets)
        => global::OpenForge.Cli.OutputText.Install.InstallWording.Source(fingerprint, assets);

    internal static string FindingTitle(InstallFindingCode code) => code switch
    {
        InstallFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        InstallFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        InstallFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        InstallFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        InstallFindingCode.ManagedDivergence => global::OpenForge.Cli.OutputText.Install.InstallText.TitleFrameworkFilesHaveChanged(),
        InstallFindingCode.TargetOccupied => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsOccupied(),
        InstallFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflict(),
        InstallFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        InstallFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleGeneratedRegionIsUnsafe(),
        InstallFindingCode.LifecycleBlocked => global::OpenForge.Cli.OutputText.Install.InstallText.TitleOwnershipRecordIsInvalid(),
        InstallFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Install.InstallText.TitleRecoveryDataBlocksInstallation(),
        InstallFindingCode.PayloadUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkPayloadIsUnavailable(),
        InstallFindingCode.PayloadInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkPayloadIsInvalid(),
        InstallFindingCode.LifecycleUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
        InstallFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstallationProjectionIsUnavailable(),
        InstallFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        InstallFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        InstallFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        InstallFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        InstallFindingCode.LifecyclePublicationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipPublicationFailed(),
        InstallFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        InstallFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstallFailed(),
        InstallFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Install.InstallText.TitleInstallWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Install finding code is not defined."),
    };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpSyntax());
    internal static string HelpEstablishment() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpEstablishment());
    internal static string HelpWritePolicy() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpWritePolicy());
    internal static string HelpConfirmation() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpConfirmation());
    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection());
    internal static string HelpExamples() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpExamples());
    internal static string HelpRelatedCommands() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.MessageOpenForgeUpdateReconcileAnExistingManagedFrameworkInstallationOpenForgeDoctorInspectBlockedOrUnavailableLifecycleRetainedRecoveryArtifactAfterReview());
    internal static string HelpNotes() => ("  " + global::OpenForge.Cli.OutputText.Install.InstallText.HelpNotes());

    private static string CliTextPlural(long count, string singular)
        => count == 1
            ? singular
            : singular switch
            {
                "directory" => global::OpenForge.Cli.OutputText.Install.InstallText.LabelDirectories(),
                "section" => global::OpenForge.Cli.OutputText.Install.InstallText.LabelSections(),
                _ => singular + "s",
            };

    private static string Verb(long count, string singular, string plural)
        => count == 1 ? singular : plural;

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);
}
