using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Wording;

internal static class RouteRemoveWording
{
    internal static string PersistenceSettings(RouteRemoveSettingsRemoval settings)
    {
        var targets = settings.Categories.Select(category => $"category .agents/{category}")
            .Concat(settings.Files.Select(path => $"file {path}"))
            .Concat(settings.Directories.Select(path => $"directory {path}"));
        var targetText = string.Join(", ", targets);
        if (targetText.Length == 0)
        {
            targetText = "the selected route";
        }

        var outcome = settings.Outcome switch
        {
            RouteRemovePersistenceOutcome.NotEstablished => "Not established",
            RouteRemovePersistenceOutcome.Planned => "Would record",
            RouteRemovePersistenceOutcome.Applied => "Recorded",
            RouteRemovePersistenceOutcome.Unchanged => "Already recorded",
            RouteRemovePersistenceOutcome.NotStarted => "Did not record",
            RouteRemovePersistenceOutcome.Failed => "Could not verify",
            RouteRemovePersistenceOutcome.Unknown => "Could not determine",
            _ => throw new ArgumentOutOfRangeException(nameof(settings), settings.Outcome, "The persistence outcome is not defined."),
        };
        return global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatRemovalIntentOutcome(
            outcome,
            targetText,
            settings.Path);
    }

    internal static string PersistenceOwnership(RouteRemoveOwnershipRelease ownership)
    {
        if (ownership.Outcome == RouteRemovePersistenceOutcome.Unchanged && ownership.Claims.IsEmpty)
        {
            return global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatNoManagedContentOwnershipToRelease();
        }

        var outcome = ownership.Outcome switch
        {
            RouteRemovePersistenceOutcome.NotEstablished => "Did not establish",
            RouteRemovePersistenceOutcome.Planned => "Would release",
            RouteRemovePersistenceOutcome.Applied => "Released",
            RouteRemovePersistenceOutcome.Unchanged => "Left unchanged",
            RouteRemovePersistenceOutcome.NotStarted => "Did not release",
            RouteRemovePersistenceOutcome.Failed => "Could not verify release of",
            RouteRemovePersistenceOutcome.Unknown => "Could not determine release of",
            _ => throw new ArgumentOutOfRangeException(nameof(ownership), ownership.Outcome, "The persistence outcome is not defined."),
        };
        var count = ownership.Claims.Length;
        return global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatOwnershipReleaseOutcome(
            outcome,
            count.ToString(CultureInfo.InvariantCulture),
            count == 1 ? "claim" : "claims");
    }

    internal static string OwnershipClaim(RouteRemoveOwnershipClaim claim)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatOwnershipClaim(
            claim.Manager == RouteRemoveOwnershipManager.Framework ? "Framework" : "Extension",
            claim.Owner,
            claim.Path);

    internal static string Removed(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.Removed(path);

    internal static string RemovedRoute(string id, int count)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatRemovedTheRoute(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file")}"));

    internal static string WouldRemove(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.WouldRemove(path);

    internal static string WouldRemoveRoute(string id, int count)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatWouldRemoveTheRoute(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "file")}"));

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatCouldNotBeRemovedNothingWasChanged($"{id}", $"{Sentence(limitation)}");

    internal static string CannotRemove(string reference, string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotRemove($"{reference}", $"{Sentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotRemove($"{id}", $"{Sentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageRouteRemoveWasCancelledNothingWasChanged();

    internal static string EntryRemoved(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.EntryRemoved(path);

    internal static string WouldEntryRemoved(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.WouldEntryRemoved(path);

    internal static string DetachedLinks(int count)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatDetachedThatPointedAtItTheLinkTextWasKept(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link")}"));

    internal static string WouldDetachLinks(int count)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatWouldDetachThatPointedAtItTheLinkTextWouldBeKept(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "link")}"));

    internal static string LinkText(string before, string after) => $"{before} -> {after}";

    internal static string Recovery(int count, string path)
        => count == 1
            ? global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatTheDeletedFileIsKeptInARecoveryBundleAt($"{path}")
            : global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatTheDeletedFilesAreKeptInARecoveryBundleAt($"{path}");

    internal static string Scan(int files, int occurrences)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatScannedAndFound(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{occurrences}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(occurrences, "link")}"));

    internal static string NoChanges() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string NotStarted(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.NotStarted(path);

    internal static string Unknown(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.Unknown(path);

    internal static string FailedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.FailedEffect(path);

    internal static string InvalidSource(string reference)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.InvalidSource(reference);

    internal static string InvalidSubject(string reference, string problem)
        => problem.Contains("Loader", StringComparison.OrdinalIgnoreCase)
            ? global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatIsTheLoaderAndCannotBeRemoved($"{reference}")
            : problem.Contains("overwrite", StringComparison.OrdinalIgnoreCase)
                ? global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatIsAnOverwriteFileRemoveItsBaseFile($"{reference}")
                : Sentence(problem);

    internal static string CategoryUnsafe(string folder, string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatContainsAFileThatCannotBeRemovedSafely($"{folder}", $"{path}", $"{Sentence(reason)}");

    internal static string OverwriteAmbiguous(string name)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveWording.OverwriteAmbiguous(name);

    internal static string ReferenceUnsafe(string path, int line, int column, string reason)
        => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemovePhrases.FormatTheLinkAtCannotBeDetachedSafely($"{path}", $"{line}", $"{column}", $"{Sentence(reason)}");

    internal static string CleanupReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelAfterReviewingTheBundle();

    internal static string ListReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageListTheRoutesThenRerunRouteRemoveWithAnExactSourceReference();

    internal static string HelpReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.HelpReason();

    internal static string ManualReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageFixTheReportedSourceByHandThenRerunRouteRemove();

    internal static string UpdateReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageUpdateTheOwningRouteExplicitlyThenRerunRouteRemoveIfNeeded();

    internal static string ExtensionReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageRemoveTheOwningExtensionFirstThenRerunRouteRemove();

    internal static string RetryReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteRemoveFromAFreshPlan();

    internal static string DoctorReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageInspectTheReportedRouteRemoveBoundaryBeforeRelyingOnThisResult();

    internal static string DebugReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageReportTheFailureAndRetryTheSameRouteRemoveRequestWithBoundedDiagnostics();

    internal static string RerunReason() => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.MessageRerunTheSameRouteRemoveRequest();

    internal static string FindingCode(RouteRemoveFindingCode code)
        => $"route-remove.{Name(code)}";

    internal static string FindingTitle(RouteRemoveFindingCode code) => code switch
    {
        RouteRemoveFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        RouteRemoveFindingCode.ConfirmationRequired => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleConfirmationIsRequired(),
        RouteRemoveFindingCode.InvalidSource => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidSourceReference(),
        RouteRemoveFindingCode.SourceNotFound => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnknown(),
        RouteRemoveFindingCode.InvalidSubject => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleSourceCannotBeRemoved(),
        RouteRemoveFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        RouteRemoveFindingCode.SourceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
        RouteRemoveFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRoutesAreAmbiguous(),
        RouteRemoveFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
        RouteRemoveFindingCode.OverwriteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteIsAmbiguous(),
        RouteRemoveFindingCode.CategoryUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCategoryIsUnsafe(),
        RouteRemoveFindingCode.OwnershipUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
        RouteRemoveFindingCode.SettingsUnavailable => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleRemovalSettingsAreUnavailable(),
        RouteRemoveFindingCode.ProtectedTarget => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleRouteTargetIsProtected(),
        RouteRemoveFindingCode.OwnershipClaimed => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleSourceIsManaged(),
        RouteRemoveFindingCode.ReferenceUnsafe => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleLinkCannotBeDetachedSafely(),
        RouteRemoveFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        RouteRemoveFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
        RouteRemoveFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        RouteRemoveFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleBlocksRemoval(),
        RouteRemoveFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        RouteRemoveFindingCode.CategoryInventoryIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCategoryInventoryIsIncomplete(),
        RouteRemoveFindingCode.ReferenceCoverageIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleReferenceScanIsIncomplete(),
        RouteRemoveFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnavailable(),
        RouteRemoveFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsUnavailable(),
        RouteRemoveFindingCode.InspectionIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInspectionIsIncomplete(),
        RouteRemoveFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        RouteRemoveFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
        RouteRemoveFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        RouteRemoveFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        RouteRemoveFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        RouteRemoveFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleRouteRemoveFailed(),
        RouteRemoveFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Remove.RouteRemoveText.TitleRouteRemoveWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Route Remove finding code is not defined."),
    };

    internal static string Confirmation(int fileCount)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatDeleteTheListedAboveYN(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{fileCount}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(fileCount, "file")}"));

    private static string Sentence(string value) => CliFindingWording.PlainCause(value);

    private static string Name<T>(T value) where T : struct, Enum
        => System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
}
