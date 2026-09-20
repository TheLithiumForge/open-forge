using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;

internal static class ExtensionUpdateWording
{
    internal static string Selection() => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.PromptWhichExtensionsDoYouWantToUpdate();

    internal static string Apply() => CliPromptWording.Confirm();

    internal static string Prune(int count)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatDeleteTheFileListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? string.Empty : "s")}"));

    internal static string UpToDate(string id)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.UpToDate(id);

    internal static string AllUpToDate(int count)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.AllUpToDate(count);

    internal static string Updated(string id, string version)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.Updated(id, version);

    internal static string Updated(int count) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.Updated(count);

    internal static string WouldUpdate(string id, string version)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.WouldUpdate(id, version);

    internal static string UpdatedWithKept(string id, string version, int kept)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatUpdatedTheExtensionToFromAnEarlierVersionKept($"{id}", $"{version}", $"{kept}", $"{Plural(kept, "file")}", $"{Verb(kept, "was", "were")}");

    internal static string WouldUpdateWithKept(string id, string version, int kept)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatWouldUpdateTheExtensionToFromAnEarlierVersionWouldBeKept($"{id}", $"{version}", $"{kept}", $"{Plural(kept, "file")}");

    internal static string UpToDateWithKept(string id, int kept)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatTheExtensionIsUpToDateButFromAnEarlierVersionKept($"{id}", $"{kept}", $"{Plural(kept, "file")}", $"{Verb(kept, "was", "were")}");

    internal static string OwnershipUnknown(string id)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.OwnershipUnknown(id);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatTheExtensionCouldNotBeUpdatedNothingWasChanged($"{id}", $"{Sentence(limitation)}");

    internal static string CannotUpdate(string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.UpdateCannotUpdate($"{Sentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.UpdateCannotUpdatePackage($"{id}", $"{Sentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.MessageExtensionUpdateWasCancelledNothingWasChanged();

    internal static string NoChanges() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string ReplaceRow(string relation, bool planned)
        => $"{(planned ? "replace" : "replaced")} ({relation})";

    internal static string NotStarted() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();

    internal static string RestoreRow(bool planned)
        => planned ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelRestoreItWasMissing() : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelRestoredItWasMissing();

    internal static string CreateRow(bool planned)
        => planned ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelCreateNewInThisVersion() : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelCreatedNewInThisVersion();

    internal static string DeleteRow(bool planned)
        => planned ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelDeleteNoLongerPartOfThePackage() : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelDeletedNoLongerPartOfThePackage();

    internal static string KeepRow(bool planned)
        => planned ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelKeepNoLongerPartOfThePackage() : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelKeptNoLongerPartOfThePackage();

    internal static string EntriesUpdated() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionUpdated();

    internal static string Unchanged(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatUnchanged($"{count}", $"{Plural(count, "file")}");

    internal static string Sections(int count)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatUpdated($"{count}", $"{Plural(count, "Entries section")}");

    internal static string Source(string path) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.Source(path);

    internal static string Packages(IReadOnlyList<string> packageLines)
        => packageLines.Count == 0
            ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleDependencyClosureNone()
            : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatDependencyClosure($"{string.Join(", ", packageLines)}");

    internal static string Grant(
        IReadOnlyList<string> required,
        IReadOnlyList<string> missing,
        string decision,
        string action,
        string outcome)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatGrantRequiredMissingDecisionActionOutcome($"{Values(required)}", $"{Values(missing)}", $"{PermissionDecision(decision)}", $"{PermissionAction(action)}", $"{PermissionOutcome(outcome)}");

    internal static string SavedGrant(string path, bool preview)
        => preview
            ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatWouldSaveAGrantForToAgentsOpenForgeJson($"{path}")
            : global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatSavedAGrantForToAgentsOpenForgeJson($"{path}");

    internal static string PreviousContent(string value) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.PreviousContent(value);

    internal static string Verification(string targets, string topology, string extensionRecord)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatVerificationTargetsEntriesExtensionRecord($"{VerificationState(targets)}", $"{VerificationState(topology)}", $"{VerificationState(extensionRecord)}");

    internal static string Recovery(string state, string? path)
        => state switch
        {
            "not-required" or "not required" => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasNeeded(),
            "not-created" or "not created" => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasCreated(),
            "removed" => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRemoved(),
            "retained" => path is null
                ? global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRetained()
                : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt($"{path}"),
            "unknown" => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalStateOfTheRecoveryBundleIsUnknown(),
            _ => path is null ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecovery($"{state}") : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecoveryAt($"{state}", $"{path}"),
        };

    internal static string FindingCode(ExtensionUpdateFindingCode code)
        => $"extension-update.{Name(code)}";

    internal static string FindingTitle(ExtensionUpdateFindingCode code) => code switch
    {
        ExtensionUpdateFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        ExtensionUpdateFindingCode.SelectionRequired => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSelectionIsRequired(),
        ExtensionUpdateFindingCode.InteractionEnded => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInteractionEnded(),
        ExtensionUpdateFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        ExtensionUpdateFindingCode.SourceUnavailable => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSourceIsUnavailable(),
        ExtensionUpdateFindingCode.SourceInvalid => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSourceIsInvalid(),
        ExtensionUpdateFindingCode.SourceOverlap => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleSourceOverlapsTheWorkspace(),
        ExtensionUpdateFindingCode.SourceIdentityConflict => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleSourceIdentityConflicts(),
        ExtensionUpdateFindingCode.FrameworkUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkIsUnavailable(),
        ExtensionUpdateFindingCode.FrameworkUnsafe => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleFrameworkIsUnsafe(),
        ExtensionUpdateFindingCode.LifecycleUnavailable => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordIsUnavailable(),
        ExtensionUpdateFindingCode.LifecycleBlocked => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordIsBlocked(),
        ExtensionUpdateFindingCode.ManagedDivergence => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleRetiredFileWasKept(),
        ExtensionUpdateFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflicts(),
        ExtensionUpdateFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        ExtensionUpdateFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        ExtensionUpdateFindingCode.PermissionsInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreInvalid(),
        ExtensionUpdateFindingCode.PermissionsUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreUnavailable(),
        ExtensionUpdateFindingCode.PermissionsChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsChanged(),
        ExtensionUpdateFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionCouldNotBeSaved(),
        ExtensionUpdateFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        ExtensionUpdateFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        ExtensionUpdateFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        ExtensionUpdateFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        ExtensionUpdateFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        ExtensionUpdateFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleRecoveryBundleBlocksUpdate(),
        ExtensionUpdateFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsUnavailable(),
        ExtensionUpdateFindingCode.LifecycleObservation => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionRecordWasNotUpdated(),
        ExtensionUpdateFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        ExtensionUpdateFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWritingFailed(),
        ExtensionUpdateFindingCode.TopologyVerificationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleEntriesVerificationFailed(),
        ExtensionUpdateFindingCode.LifecyclePublicationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordCouldNotBeWritten(),
        ExtensionUpdateFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleUpdateVerificationFailed(),
        ExtensionUpdateFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryFailed(),
        ExtensionUpdateFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionUpdateFailed(),
        ExtensionUpdateFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionUpdateWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Update finding code is not defined."),
    };

    internal static string FindingMessage(
        ExtensionUpdateFinding finding,
        string id,
        string? sourcePath,
        string? recoveryPath)
        => finding.Code switch
        {
            ExtensionUpdateFindingCode.InvalidInput => CannotUpdate(finding.Cause),
            ExtensionUpdateFindingCode.SelectionRequired => CliFindingWording.SelectionRequired(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionUpdate(), false),
            ExtensionUpdateFindingCode.InteractionEnded => CliFindingWording.InteractionEnded(),
            ExtensionUpdateFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionUpdate()),
            ExtensionUpdateFindingCode.SourceUnavailable => SourceUnreadable(sourcePath ?? finding.Target ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource()),
            ExtensionUpdateFindingCode.SourceInvalid => InvalidSource(sourcePath ?? finding.Target ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource(), finding.Cause),
            ExtensionUpdateFindingCode.SourceOverlap => SourceOverlap(sourcePath ?? finding.Target ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource()),
            ExtensionUpdateFindingCode.SourceIdentityConflict => SourceIdentityConflict(sourcePath ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheSelectedSource(), finding.Target ?? id),
            ExtensionUpdateFindingCode.FrameworkUnavailable => CliFindingWording.FrameworkUnavailable(),
            ExtensionUpdateFindingCode.FrameworkUnsafe => CliFindingWording.FrameworkUnsafe(),
            ExtensionUpdateFindingCode.LifecycleUnavailable => CliFindingWording.LifecycleUnavailable(),
            ExtensionUpdateFindingCode.LifecycleBlocked => CliFindingWording.LifecycleBlocked(finding.Cause),
            ExtensionUpdateFindingCode.ManagedDivergence => finding.Cause,
            ExtensionUpdateFindingCode.OwnershipConflict => finding.Cause,
            ExtensionUpdateFindingCode.PermissionRequired => CliFindingWording.PermissionRequired(global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdate()),
            ExtensionUpdateFindingCode.PermissionDeclined => CliFindingWording.PermissionDeclined(),
            ExtensionUpdateFindingCode.PermissionsInvalid => CliFindingWording.PermissionsInvalid(finding.Cause),
            ExtensionUpdateFindingCode.PermissionsUnavailable => CliFindingWording.PermissionsUnavailable(),
            ExtensionUpdateFindingCode.PermissionsChanged => CliFindingWording.PermissionsChanged(),
            ExtensionUpdateFindingCode.PermissionWriteFailed => CliFindingWording.PermissionWriteFailed(),
            ExtensionUpdateFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(finding.Target ?? id, finding.Cause),
            ExtensionUpdateFindingCode.ProjectionUnavailable => CliFindingWording.ProjectionUnavailable(finding.Target ?? id, finding.Cause),
            ExtensionUpdateFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(finding.Target ?? id, finding.Cause),
            ExtensionUpdateFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            ExtensionUpdateFindingCode.TargetChanged => CliFindingWording.TargetChanged(finding.Target ?? id),
            ExtensionUpdateFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(finding.Target ?? recoveryPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()),
            ExtensionUpdateFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(finding.Target ?? ".agents"),
            ExtensionUpdateFindingCode.LifecycleObservation => finding.Target is { } observedId
                ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatIsNotRecordedAsInstalledSoItWasNotUpdated($"{observedId}")
                : finding.Cause,
            ExtensionUpdateFindingCode.RecoveryArtifactRetained => recoveryPath is { } retained
                ? CliFindingWording.RecoveryRetained(retained)
                : finding.Cause,
            ExtensionUpdateFindingCode.WriteFailed => CliFindingWording.CauseSentence(finding.Cause),
            ExtensionUpdateFindingCode.TopologyVerificationFailed => TopologyFailure(finding.Target ?? id, recoveryPath),
            ExtensionUpdateFindingCode.LifecyclePublicationFailed => CliFindingWording.LifecyclePublicationFailed(),
            ExtensionUpdateFindingCode.VerificationFailed => finding.Cause,
            ExtensionUpdateFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            ExtensionUpdateFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.TitleExtensionUpdate(), finding.Cause),
            ExtensionUpdateFindingCode.Interrupted => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Extension Update finding code is not defined."),
        };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpSyntax());

    internal static string HelpSelection() => ("  " + global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpSelection());

    internal static string HelpAuthority() => ("  " + global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpAuthority());

    internal static string HelpPermissions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpPermissions());

    internal static string HelpResults() => ("  " + global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.HelpResults());

    private static string SourceUnreadable(string path) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.SourceUnreadable(path);

    private static string InvalidSource(string path, string reason) => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatIsNotAValidPackageOrPackageFolder($"{path}", $"{Sentence(reason)}");

    private static string SourceOverlap(string path) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.SourceOverlap(path);

    private static string SourceIdentityConflict(string path, string id) => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateWording.SourceIdentityConflict(path, id);

    private static string TopologyFailure(string path, string? recoveryPath)
        => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdatePhrases.FormatTheEntriesSectionsDidNotMatchTheInstalledFilesAfterWritingRecoveryData($"{recoveryPath ?? path}");

    private static string Values(IReadOnlyList<string> values)
        => values.Count == 0 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() : string.Join(", ", values);

    private static string PermissionDecision(string value)
        => value switch
        {
            "not-required" => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNotNeeded(),
            "not-evaluated" => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNotEvaluated(),
            _ => value,
        };

    private static string PermissionAction(string value)
        => value == "none" ? global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNoAction() : value;

    private static string PermissionOutcome(string value)
        => value switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelNoOutcomeRecorded(),
            _ => value,
        };

    private static string VerificationState(string value)
        => value switch
        {
            "not-requested" or "not requested" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
            "planned" => global::OpenForge.Cli.OutputText.Extension.Update.ExtensionUpdateText.LabelPlannedButNotChecked(),
            _ => value,
        };

    private static string Sentence(string value) => CliFindingWording.PlainCause(value);

    private static string Plural(int count, string singular) => count == 1 ? singular : singular + "s";

    private static string Verb(int count, string singular, string plural) => count == 1 ? singular : plural;

    private static string Name<T>(T value) where T : struct, Enum
        => JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
}
