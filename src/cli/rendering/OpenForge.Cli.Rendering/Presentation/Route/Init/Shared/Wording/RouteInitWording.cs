using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Wording;

internal static class RouteInitWording
{
    internal static string Created(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.Created(path);

    internal static string CreatedMany(int count, string id)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.CreatedMany(count, id);

    internal static string WouldCreate(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.WouldCreate(path);

    internal static string WouldCreateMany(int count, string id)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.WouldCreateMany(count, id);

    internal static string AlreadyInitialized(string id)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.AlreadyInitialized(id);

    internal static string Incomplete(string limitation)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatTheRouteCouldNotBeInitializedNothingWasChanged($"{TrimSentence(limitation)}");

    internal static string Invalid(string target, string problem)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatCannotInitialize($"{target}", $"{TrimSentence(problem)}");

    internal static string Blocked(string target, string reason)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatCannotInitialize($"{target}", $"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.Failed(completed, total);

    internal static string Cancelled()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRouteInitWasCancelledNothingWasChanged();

    internal static string Listed(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.Listed(path);

    internal static string WouldList(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.WouldList(path);

    internal static string EntryPointState(string path, string outcome)
        => $"{path}  {outcome}";

    internal static string Advisory()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageItsDescriptionAndTagsArePlaceholdersEditThemBeforeRelyingOnThisRoute();

    internal static string MetadataDescription(string value)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.MetadataDescription(value);

    internal static string MetadataResponsibility(string value)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.MetadataResponsibility(value);

    internal static string MetadataTags(IReadOnlyList<string> values)
        => OpenForge.Cli.OutputText.Route.Shared.MetadataText.Tags(string.Join(", ", values));

    internal static string Scaffold(string value)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.Scaffold(value);

    internal static string LockUpdated(bool planned)
        => planned ? global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelAgentsOpenForgeLockJsonWouldUpdate() : global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelAgentsOpenForgeLockJsonUpdated();

    internal static string FileHeader(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.FileHeader(path);

    internal static string EntriesSection(string path)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.EntriesSection(path);

    internal static string Verification(string value)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.Verification(value);

    internal static string FrameworkFingerprint(string value)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.FrameworkFingerprint(value);

    internal static string Recovery(RouteInitRecovery recovery)
        => recovery.State switch
        {
            RouteInitRecoveryState.NotRequired => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRecoveryNoRecoveryBundleWasNeeded(),
            RouteInitRecoveryState.NotCreated => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRecoveryNoRecoveryBundleWasCreated(),
            RouteInitRecoveryState.Removed => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRecoveryTheRecoveryBundleWasRemoved(),
            RouteInitRecoveryState.Retained => recovery.ResidualPath is { } path
                ? global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatRecoveryTheRecoveryBundleWasRetainedAt($"{path}")
                : global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRecoveryTheRecoveryBundleWasRetained(),
            RouteInitRecoveryState.Unknown => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRecoveryTheFinalRecoveryStateIsUnknown(),
            _ => throw new ArgumentOutOfRangeException(nameof(recovery), recovery.State,
                "The Route Init recovery state is not defined."),
        };

    internal static string InvalidTarget(string target)
        => string.Equals(target, "loader", StringComparison.Ordinal)
            ? global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageTheLoaderCannotBeInitialized()
            : global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatIsNotARouteIdOrAnEntrypointPathUnderAgents($"{target}");

    internal static string InvalidMetadata(string cause)
        => cause.StartsWith("--", StringComparison.Ordinal)
            ? cause
            : global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRouteInitMetadataIsInvalid();

    internal static string IdentityCollision(string id)
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitWording.IdentityCollision(id);

    internal static string CorrectedCommand(string target)
        => $"open-forge route init {target} --description \"<one sentence>\" --tag <Tag>";

    internal static string CorrectInputReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageCorrectTheNamedRouteInitInputThenRerunTheRequest();

    internal static string InstallReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageEstablishATrustedCurrentFrameworkInstallationBeforeRerunningRouteInitInFrameworkMode();

    internal static string UpdateReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageUpdateTheInstalledFrameworkStateToTheRunningCliSEmbeddedInventoryBeforeRerunningRouteInit();

    internal static string RetryReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageWaitForTheBlockingConditionOrInspectTheChangedTargetThenRerunRouteInitFromAFreshPlan();

    internal static string DoctorReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageInspectTheBlockedRouteInitBoundaryBeforeRerunningTheRequest();

    internal static string IncompleteReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageInspectTheUnavailableRouteMetadataProjectionLifecycleOrRecoveryFactsBeforeRelyingOnThisRouteInitResult();

    internal static string CleanupReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageReviewAndRemoveTheReportedRecoveryArtifactAfterConfirmingTheVerifiedRouteInitResult();

    internal static string DebugReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageReportTheFailureAndRetryTheSameRouteInitRequestWithBoundedDiagnostics();

    internal static string RerunReason()
        => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageRerunTheSameRouteInitRequest();

    internal static string FindingTitle(RouteInitFindingCode code)
        => code switch
        {
            RouteInitFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            RouteInitFindingCode.InvalidTarget => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidTarget(),
            RouteInitFindingCode.InvalidMetadata => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidMetadata(),
            RouteInitFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            RouteInitFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            RouteInitFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
            RouteInitFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRoutesAreAmbiguous(),
            RouteInitFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
            RouteInitFindingCode.LoaderUnsafe => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleLoaderIsUnsafe(),
            RouteInitFindingCode.FrameworkPayloadInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkPayloadIsInvalid(),
            RouteInitFindingCode.FrameworkInstallRequired => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleFrameworkInstallationIsRequired(),
            RouteInitFindingCode.FrameworkUpdateRequired => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleFrameworkUpdateIsRequired(),
            RouteInitFindingCode.FrameworkAlignmentBlocked => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleFrameworkAlignmentIsBlocked(),
            RouteInitFindingCode.MetadataUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleMetadataIsUnsafe(),
            RouteInitFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleGeneratedRegionIsUnsafe(),
            RouteInitFindingCode.LifecycleBlocked => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleOwnershipRecordIsBlocked(),
            RouteInitFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
            RouteInitFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
            RouteInitFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataBlocksTheChange(),
            RouteInitFindingCode.FrameworkPayloadUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkPayloadIsUnavailable(),
            RouteInitFindingCode.InspectionIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInspectionIsIncomplete(),
            RouteInitFindingCode.MetadataIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleMetadataCouldNotBeRead(),
            RouteInitFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesContentIsUnavailable(),
            RouteInitFindingCode.LifecycleUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOwnershipRecordIsUnavailable(),
            RouteInitFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
            RouteInitFindingCode.NeedsAuthoring => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleRouteNeedsAuthoring(),
            RouteInitFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
            RouteInitFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
            RouteInitFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
            RouteInitFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
            RouteInitFindingCode.LifecyclePublicationFailed => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleOwnershipRecordCouldNotBeWritten(),
            RouteInitFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
            RouteInitFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleRouteInitFailed(),
            RouteInitFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleRouteInitWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code,
                "The Route Init finding code is not defined."),
        };

    internal static string Message(
        RouteInitFindingCode code,
        string target,
        string workspace,
        string cause,
        int completed,
        int total,
        string? recoveryPath)
        => code switch
        {
            RouteInitFindingCode.InvalidInput
                => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelInitializeTheRoute(), TrimSentence(cause)),
            RouteInitFindingCode.InvalidTarget => InvalidTarget(target),
            RouteInitFindingCode.InvalidMetadata => InvalidMetadata(cause),
            RouteInitFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(workspace),
            RouteInitFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(workspace, TrimSentence(cause)),
            RouteInitFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(target, TrimSentence(cause)),
            RouteInitFindingCode.RouteAmbiguous => CliFindingWording.RouteAmbiguous(target),
            RouteInitFindingCode.IdentityCollision => IdentityCollision(target),
            RouteInitFindingCode.LoaderUnsafe => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageAgentsLoaderMdCouldNotBeVerifiedSafely(),
            RouteInitFindingCode.FrameworkPayloadInvalid => CliFindingWording.PayloadInvalid(),
            RouteInitFindingCode.FrameworkInstallRequired => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageTheFrameworkScaffoldNeedsAnInstalledFramework(),
            RouteInitFindingCode.FrameworkUpdateRequired => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageTheInstalledFrameworkIsOlderThanTheOneThisCliShips(),
            RouteInitFindingCode.FrameworkAlignmentBlocked => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.MessageTheInstalledFrameworkDoesNotMatchTheVersionThisCliShipsSoTheFrameworkScaffoldCannotBeUsed(),
            RouteInitFindingCode.MetadataUnsafe => CliFindingWording.MetadataUnsafe(target, TrimSentence(cause)),
            RouteInitFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(target, TrimSentence(cause)),
            RouteInitFindingCode.LifecycleBlocked => CliFindingWording.LifecycleBlocked(TrimSentence(cause)),
            RouteInitFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            RouteInitFindingCode.TargetChanged => CliFindingWording.TargetChanged(target),
            RouteInitFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(target),
            RouteInitFindingCode.FrameworkPayloadUnavailable => CliFindingWording.PayloadUnavailable(),
            RouteInitFindingCode.InspectionIncomplete => CliFindingWording.InspectionIncomplete(target),
            RouteInitFindingCode.MetadataIncomplete => CliFindingWording.MetadataIncomplete(target),
            RouteInitFindingCode.ProjectionIncomplete => CliFindingWording.ProjectionUnavailable(target, TrimSentence(cause)),
            RouteInitFindingCode.LifecycleUnavailable => CliFindingWording.LifecycleUnavailable(),
            RouteInitFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(workspace),
            RouteInitFindingCode.NeedsAuthoring => Advisory(),
            RouteInitFindingCode.RecoveryArtifactRetained => CliFindingWording.RecoveryRetained(recoveryPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteInitFindingCode.TargetChangedDuringApply => CliFindingWording.TargetChangedDuringApply(target, completed, total),
            RouteInitFindingCode.WriteFailed => CliFindingWording.WriteFailed(target, completed, total, recoveryPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteInitFindingCode.VerificationFailed => CliFindingWording.VerificationFailed(target, recoveryPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteInitFindingCode.LifecyclePublicationFailed => CliFindingWording.LifecyclePublicationFailed(),
            RouteInitFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            RouteInitFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleRouteInit(), TrimSentence(cause)),
            RouteInitFindingCode.Interrupted => CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.TitleRouteInit()),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code,
                "The Route Init finding code is not defined."),
        };

    private static string TrimSentence(string value)
        => CliFindingWording.PlainCause(value);
}
