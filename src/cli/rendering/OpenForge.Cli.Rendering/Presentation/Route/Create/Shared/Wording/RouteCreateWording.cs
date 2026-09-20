using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Wording;

internal static class RouteCreateWording
{
    internal static string Created(string path, string id) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.Created(path, id);

    internal static string WouldCreate(string path, string id) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.WouldCreate(path, id);

    internal static string AlreadyMatching(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.AlreadyMatching(path);

    internal static string Incomplete(string limitation)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreatePhrases.FormatTheFileCouldNotBeCreatedNothingWasChanged($"{TrimSentence(limitation)}");

    internal static string Invalid(string problem)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreatePhrases.FormatCannotCreateTheRoutedFile($"{TrimSentence(problem)}");

    internal static string Blocked(string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreatePhrases.FormatCannotCreate($"{path}", $"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.Failed(completed, total);

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageRouteCreateWasCancelledNothingWasChanged();

    internal static string Listed(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.Listed(path);

    internal static string WouldList(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.WouldList(path);

    internal static string BodyCopied(string reference) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.BodyCopied(reference);

    internal static string CreatedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.CreatedEffect(path);

    internal static string WouldCreateEffect(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.WouldCreateEffect(path);

    internal static string NotStarted(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.NotStarted(path);

    internal static string Unknown(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.Unknown(path);

    internal static string FailedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.FailedEffect(path);

    internal static string MetadataDescription(string value) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.MetadataDescription(value);

    internal static string OptionalMetadataTitle()
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.OptionalMetadataTitle();

    internal static string WouldCreateWithoutOptionalMetadata(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.WouldCreateWithoutOptionalMetadata(path);

    internal static string CreatedWithoutOptionalMetadata(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.CreatedWithoutOptionalMetadata(path);

    internal static string CurrentWithoutOptionalMetadata(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.CurrentWithoutOptionalMetadata(path);

    internal static string OptionalMetadataMissing(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.OptionalMetadataMissing(path);

    internal static string MetadataTags(IReadOnlyList<string> values)
        => OpenForge.Cli.OutputText.Route.Shared.MetadataText.Tags(string.Join(", ", values));

    internal static string MetadataResponsibility(string value) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.MetadataResponsibility(value);

    internal static string TemplatePath(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.TemplatePath(path);

    internal static string FileHeader(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.FileHeader(path);

    internal static string EntriesSection(string path) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.EntriesSection(path);

    internal static string Verification(string value) => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.Verification(value);

    internal static string InvalidTarget(string target, bool isSpecialFile)
        => isSpecialFile
            ? global::OpenForge.Cli.OutputText.Route.Create.RouteCreatePhrases.FormatIsAnEntrypointAFolderOrAnOverwriteFileRouteCreateMakesOrdinaryFiles($"{target}")
            : global::OpenForge.Cli.OutputText.Route.Create.RouteCreatePhrases.FormatMustBeAMarkdownFileBelowAnExistingRoute($"{target}");

    internal static string InvalidMetadata(string cause) => cause;

    internal static string InvalidTemplate(string reference)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.InvalidTemplate(reference);

    internal static string ParentMissing(string folder)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.ParentMissing(folder);

    internal static string TargetContentDiffers(string path)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.TargetContentDiffers(path);

    internal static string TemplateUnsafe(string reference)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.TemplateUnsafe(reference);

    internal static string TemplateUnavailable(string reference)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.TemplateUnavailable(reference);

    internal static string IdentityCollision(string id)
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.IdentityCollision(id);

    internal static string CorrectedCreateCommand(string target)
        => $"open-forge route create {target} --description \"<one sentence>\" --tag <Tag>";

    internal static string CorrectInputReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageCorrectTheRouteCreateInputThenRerunTheRequest();

    internal static string FindTemplateReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageFindARoutedTemplateThenRerunRouteCreate();

    internal static string InitParentReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageCreateTheMissingParentRouteThenRerunRouteCreate();

    internal static string UpdateReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageUpdateTheExistingRouteExplicitlyThenRerunRouteCreateIfNeeded();

    internal static string RetryReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteCreateFromAFreshPlan();

    internal static string CleanupReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteCreateResult();

    internal static string DebugReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageReportTheFailureAndRetryTheSameRouteCreateRequestWithBoundedDiagnostics();

    internal static string RerunReason() => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageRerunTheSameRouteCreateRequest();

    internal static string AddOptionalDescriptionOrTag()
        => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.AddOptionalDescriptionOrTag();

    internal static string FindingTitle(RouteCreateFindingCode code) => code switch
    {
        RouteCreateFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        RouteCreateFindingCode.InvalidTarget => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidTarget(),
        RouteCreateFindingCode.InvalidMetadata => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidMetadata(),
        RouteCreateFindingCode.InvalidTemplate => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidTemplate(),
        RouteCreateFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        RouteCreateFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        RouteCreateFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        RouteCreateFindingCode.TargetContentDiffers => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleExistingTargetDiffers(),
        RouteCreateFindingCode.ParentMissing => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleParentRouteIsMissing(),
        RouteCreateFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRoutesAreAmbiguous(),
        RouteCreateFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
        RouteCreateFindingCode.MetadataUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleMetadataIsUnsafe(),
        RouteCreateFindingCode.TemplateUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleTemplateIsUnsafe(),
        RouteCreateFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleGeneratedRegionIsUnsafe(),
        RouteCreateFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
        RouteCreateFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        RouteCreateFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataBlocksTheChange(),
        RouteCreateFindingCode.InspectionIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInspectionIsIncomplete(),
        RouteCreateFindingCode.MetadataIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleMetadataCouldNotBeRead(),
        RouteCreateFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesContentIsUnavailable(),
        RouteCreateFindingCode.TemplateUnavailable => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleTemplateCouldNotBeRead(),
        RouteCreateFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        RouteCreateFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        RouteCreateFindingCode.OptionalMetadata => OptionalMetadataTitle(),
        RouteCreateFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
        RouteCreateFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        RouteCreateFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        RouteCreateFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        RouteCreateFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleRouteCreateFailed(),
        RouteCreateFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleRouteCreateWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Route Create finding code is not defined."),
    };

    private static string TrimSentence(string value) => CliFindingWording.PlainCause(value);
}
