using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Wording;

internal static class RouteUpdateWording
{
    internal static string Updated(string id) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.Updated(id);

    internal static string AlreadyMatching(string id)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.AlreadyMatching(id);

    internal static string WouldUpdate(string id) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.WouldUpdate(id);

    internal static string UpdatedProtected(string id)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.UpdatedProtected(id);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdatePhrases.FormatCouldNotBeUpdatedNothingWasChanged($"{id}", $"{TrimSentence(limitation)}");

    internal static string Invalid(string id, string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.UpdateCannotUpdatePackage($"{id}", $"{TrimSentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.UpdateCannotUpdatePackage($"{id}", $"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageRouteUpdateWasCancelledNothingWasChanged();

    internal static string Path(string path) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.Path(path);

    internal static string TemplatePath(string path) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.TemplatePath(path);

    internal static string FrontmatterRewritten(string path) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.FrontmatterRewritten(path);

    internal static string EntryUpdated(string path) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.EntryUpdated(path);

    internal static string BodyCopied(string reference) => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.BodyCopied(reference);

    internal static string ProtectedBody(string reference)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.ProtectedBody(reference);

    internal static string FrontmatterBefore() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HeadingFrontmatterBefore();

    internal static string FrontmatterAfter() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.HeadingFrontmatterAfter();

    internal static string InvalidTarget(string reference)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.InvalidTarget(reference);

    internal static string NothingToUpdate()
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageNothingToUpdatePassDescriptionResponsibilityTagOrTemplate();

    internal static string InvalidTemplate(string reference)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.InvalidTemplate(reference);

    internal static string FrontmatterUnsafe(string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.FrontmatterUnsafe(path, reason);

    internal static string MetadataPreservationUnsafe(string path)
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateWording.MetadataPreservationUnsafe(path);

    internal static string FindingTitle(RouteUpdateFindingCode code) => code switch
    {
        RouteUpdateFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        RouteUpdateFindingCode.InvalidTarget => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleSourceCannotBeUpdated(),
        RouteUpdateFindingCode.InvalidPatch => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleInvalidUpdate(),
        RouteUpdateFindingCode.InvalidTemplate => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidTemplate(),
        RouteUpdateFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        RouteUpdateFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        RouteUpdateFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        RouteUpdateFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleRouteIsAmbiguous(),
        RouteUpdateFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
        RouteUpdateFindingCode.FrontmatterUnsafe => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleFrontmatterIsUnsafe(),
        RouteUpdateFindingCode.MetadataPreservationUnsafe => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleFrontmatterCannotBePreserved(),
        RouteUpdateFindingCode.TemplateUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleTemplateIsUnsafe(),
        RouteUpdateFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        RouteUpdateFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
        RouteUpdateFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        RouteUpdateFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataBlocksTheChange(),
        RouteUpdateFindingCode.InspectionIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInspectionIsIncomplete(),
        RouteUpdateFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesContentIsUnavailable(),
        RouteUpdateFindingCode.TemplateUnavailable => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleTemplateCouldNotBeRead(),
        RouteUpdateFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        RouteUpdateFindingCode.TemplateBodyProtected => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleTemplateBodyWasNotCopied(),
        RouteUpdateFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        RouteUpdateFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
        RouteUpdateFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        RouteUpdateFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        RouteUpdateFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        RouteUpdateFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleRouteUpdateFailed(),
        RouteUpdateFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleRouteUpdateWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Route Update finding code is not defined."),
    };

    internal static string CorrectInputReason()
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageCorrectTheNamedRouteUpdateInputThenRerunTheRequest();

    internal static string ChooseSourceReason() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageChooseAnExactRouteThenRerunRouteUpdate();

    internal static string FindTemplateReason() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageFindARoutedTemplateThenRerunRouteUpdate();

    internal static string RetryReason()
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteUpdateFromAFreshPlan();

    internal static string CleanupReason()
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteUpdateResult();

    internal static string DebugReason()
        => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageReportTheFailureAndRetryTheSameRouteUpdateRequestWithBoundedDiagnostics();

    internal static string RerunReason() => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageRerunTheSameRouteUpdateRequest();

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);
}
