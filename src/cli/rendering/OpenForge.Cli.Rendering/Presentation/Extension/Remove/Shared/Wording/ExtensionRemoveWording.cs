using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;

internal static class ExtensionRemoveWording
{
    internal static string Selection() => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.PromptWhichExtensionsDoYouWantToRemove();

    internal static string Delete(int count)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatDeleteTheFileListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(count == 1 ? string.Empty : "s")}"));

    internal static string Removed(IReadOnlyList<string> ids)
        => ids.Count == 1
            ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatRemovedTheExtension($"{ids[0]}")
            : global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatRemovedExtensions(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{ids.Count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{string.Join(", ", ids)}"));

    internal static string WouldRemove(string id) => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.WouldRemove(id);

    internal static string NoFilesRecorded(string id)
        => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.NoFilesRecorded(id);

    internal static string Warning(string id, string dependency)
        => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.Warning(id, dependency);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatTheExtensionCouldNotBeRemovedNothingWasChanged($"{id}", $"{Sentence(limitation)}");

    internal static string CannotRemove(string problem) => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatCannotRemove($"{Sentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotRemove($"{id}", $"{Sentence(reason)}");

    internal static string LockUnavailable()
        => CliFindingWording.WorkspaceLockUnavailable();

    internal static string DependencyBlockedReason(string cause, string fallback)
    {
        const string retained = "Retained Extension '";
        const string selected = "' depends on selected package '";
        var selectedIndex = cause.IndexOf(selected, StringComparison.Ordinal);
        if (cause.StartsWith(retained, StringComparison.Ordinal) && selectedIndex > retained.Length)
        {
            var dependent = cause[retained.Length..selectedIndex];
            var dependency = cause[(selectedIndex + selected.Length)..].Trim().TrimEnd('.').TrimEnd('\'');
            return global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatTheExtensionStillNeeds($"{dependent}", $"{dependency}");
        }

        return fallback;
    }

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageExtensionRemoveWasCancelledNothingWasChanged();

    internal static string DeleteRow(ExtensionRemoveEffectOutcome outcome)
        => ReadMutationOutcome(
            outcome,
            global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelWouldDelete(),
            global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDeleted());

    internal static string KeepRow(IReadOnlyList<string> owners)
        => owners.Count == 0 ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelKeptStillOwnedByNone() : global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatKeptStillOwnedBy($"{string.Join(", ", owners)}");

    internal static string ReleaseRow(ExtensionRemoveEffectOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelWouldReleaseOwnership(),
            ExtensionRemoveEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            ExtensionRemoveEffectOutcome.Verified => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelWasAlreadyGoneItsOwnershipWasReleased(),
            ExtensionRemoveEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            ExtensionRemoveEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Extension Remove effect outcome is not defined."),
        };

    internal static string SettingsEffect(ExtensionRemoveEffectOutcome outcome)
        => ReadMutationOutcome(
            outcome,
            global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.SettingsEffect(planned: true),
            global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.SettingsEffect(planned: false));

    internal static string DirectoryEffect(ExtensionRemoveEffectOutcome outcome)
        => ReadMutationOutcome(
            outcome,
            global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.DirectoryEffect(planned: true),
            global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.DirectoryEffect(planned: false));

    internal static string EntriesUpdate(string path, ExtensionRemoveEffectOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelWouldUpdate(),
            ExtensionRemoveEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            ExtensionRemoveEffectOutcome.Verified => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.EntriesUpdated(path),
            ExtensionRemoveEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            ExtensionRemoveEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Extension Remove effect outcome is not defined."),
        };

    internal static string Recovery(string path) => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveWording.Recovery(path);

    internal static string NextCleanup() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelAfterReviewingTheBundle();

    internal static string Lock(ExtensionRemoveLifecycle lifecycle)
    {
        if (lifecycle.Action == ExtensionRemoveLifecycleAction.Preserve)
        {
            return global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelUnchanged();
        }

        return lifecycle.Outcome switch
        {
            ExtensionRemoveLifecycleOutcome.Planned => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.LabelWouldUpdate(),
            ExtensionRemoveLifecycleOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            ExtensionRemoveLifecycleOutcome.Verified => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdated(),
            ExtensionRemoveLifecycleOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            ExtensionRemoveLifecycleOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            ExtensionRemoveLifecycleOutcome.AlreadyCurrent or ExtensionRemoveLifecycleOutcome.NotRequested => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelUnchanged(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.Outcome, "The Extension Remove lifecycle outcome is not defined."),
        };
    }

    private static string ReadMutationOutcome(
        ExtensionRemoveEffectOutcome outcome,
        string planned,
        string verified)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned => planned,
            ExtensionRemoveEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            ExtensionRemoveEffectOutcome.Verified => verified,
            ExtensionRemoveEffectOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelVerificationFailed(),
            ExtensionRemoveEffectOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Extension Remove effect outcome is not defined."),
        };

    internal static string Owners(IReadOnlyList<string> owners)
        => owners.Count == 0 ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() : string.Join(", ", owners);

    internal static string EntriesUnchanged(IReadOnlyList<string> paths)
        => paths.Count == 0 ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleEntriesSectionsUnchangedNone() : global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatEntriesSectionsUnchanged($"{string.Join(", ", paths)}");

    internal static string Verification(ExtensionRemoveVerification verification)
        => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatVerificationTargetsEntriesExtensionRecord($"{HumanName(verification.Targets)}", $"{HumanName(verification.Topology)}", $"{HumanName(verification.ExtensionsLifecycle)}");

    internal static string RecoveryFacts(ExtensionRemoveRecovery recovery)
        => recovery.ResidualPath is { } path
            ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatRecoveryAtProtected($"{Name(recovery.State)}", $"{path}", $"{recovery.ProtectedPaths.Count}", $"{Plural(recovery.ProtectedPaths.Count, "path")}")
            : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRecovery($"{Name(recovery.State)}");

    internal static string FindingCode(ExtensionRemoveFindingCode code) => $"extension-remove.{Name(code)}";

    internal static string FindingTitle(ExtensionRemoveFindingCode code) => code switch
    {
        ExtensionRemoveFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        ExtensionRemoveFindingCode.SelectionRequired => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleSelectionIsRequired(),
        ExtensionRemoveFindingCode.InteractionEnded => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleInteractionEnded(),
        ExtensionRemoveFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        ExtensionRemoveFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
        ExtensionRemoveFindingCode.LifecycleObservation => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleDependencyRemainsInstalled(),
        ExtensionRemoveFindingCode.DependencyBlocked => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleDependencyBlocksRemoval(),
        ExtensionRemoveFindingCode.SettingsInvalid => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleSettingsAreInvalid(),
        ExtensionRemoveFindingCode.SettingsUnavailable => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleSettingsAreUnavailable(),
        ExtensionRemoveFindingCode.PathExcluded => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleGeneratedNavigationPathExcluded(),
        ExtensionRemoveFindingCode.FrameworkUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkIsUnavailable(),
        ExtensionRemoveFindingCode.FrameworkUnsafe => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleFrameworkIsUnsafe(),
        ExtensionRemoveFindingCode.LifecycleUnavailable => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordIsUnavailable(),
        ExtensionRemoveFindingCode.LifecycleBlocked => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordIsBlocked(),
        ExtensionRemoveFindingCode.OwnershipConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipConflicts(),
        ExtensionRemoveFindingCode.PermissionRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionIsRequired(),
        ExtensionRemoveFindingCode.PermissionDeclined => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionWasDeclined(),
        ExtensionRemoveFindingCode.PermissionsInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreInvalid(),
        ExtensionRemoveFindingCode.PermissionsUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsAreUnavailable(),
        ExtensionRemoveFindingCode.PermissionsChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionsChanged(),
        ExtensionRemoveFindingCode.PermissionWriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitlePermissionCouldNotBeSaved(),
        ExtensionRemoveFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        ExtensionRemoveFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        ExtensionRemoveFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        ExtensionRemoveFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
        ExtensionRemoveFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        ExtensionRemoveFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleBlocksRemoval(),
        ExtensionRemoveFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsUnavailable(),
        ExtensionRemoveFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        ExtensionRemoveFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWritingFailed(),
        ExtensionRemoveFindingCode.TopologyVerificationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleEntriesVerificationFailed(),
        ExtensionRemoveFindingCode.LifecyclePublicationFailed => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.TitleExtensionRecordCouldNotBeWritten(),
        ExtensionRemoveFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleRemovalVerificationFailed(),
        ExtensionRemoveFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryFailed(),
        ExtensionRemoveFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleExtensionRemoveFailed(),
        ExtensionRemoveFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleExtensionRemoveWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Remove finding code is not defined."),
    };

    internal static string FindingMessage(ExtensionRemoveFinding finding, string id)
        => finding.Code switch
        {
            ExtensionRemoveFindingCode.InvalidInput => CannotRemove(finding.Cause),
            ExtensionRemoveFindingCode.SelectionRequired => CliFindingWording.SelectionRequired(global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleExtensionRemove(), false),
            ExtensionRemoveFindingCode.InteractionEnded => CliFindingWording.InteractionEnded(),
            ExtensionRemoveFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.TitleExtensionRemove()),
            ExtensionRemoveFindingCode.OwnershipObservation => finding.Cause,
            ExtensionRemoveFindingCode.LifecycleObservation => finding.Target is { } dependency
                ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemovePhrases.FormatRemainsInstalledAndIsNoLongerNeededBy($"{dependency}", $"{id}")
                : finding.Cause,
            ExtensionRemoveFindingCode.DependencyBlocked
                => DependencyBlockedReason(finding.Cause, finding.Cause),
            ExtensionRemoveFindingCode.SettingsInvalid => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageFixInvalidSettings(),
            ExtensionRemoveFindingCode.SettingsUnavailable => global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageSettingsUnavailable(),
            ExtensionRemoveFindingCode.PathExcluded => finding.Target is { } path
                ? global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.MessageGeneratedNavigationPathExcluded(path)
                : finding.Cause,
            ExtensionRemoveFindingCode.WorkspaceLockUnavailable => LockUnavailable(),
            ExtensionRemoveFindingCode.WriteFailed => CliFindingWording.CauseSentence(finding.Cause),
            ExtensionRemoveFindingCode.Interrupted => Cancelled(),
            _ => finding.Cause,
        };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpSyntax());
    internal static string HelpSelection() => ("  " + global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpSelection());
    internal static string HelpOwnership() => ("  " + global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpOwnership());
    internal static string HelpPermissions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpPermissions());
    internal static string HelpResults() => ("  " + global::OpenForge.Cli.OutputText.Extension.Remove.ExtensionRemoveText.HelpResults());

    private static string Sentence(string value) => value.Trim().TrimEnd('.');
    private static string Plural(int count, string singular) => count == 1 ? singular : singular + "s";
    private static string Name<T>(T value) where T : struct, Enum
        => JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    private static string HumanName<T>(T value) where T : struct, Enum
        => Name(value) switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => Name(value).Replace('-', ' '),
        };
}
