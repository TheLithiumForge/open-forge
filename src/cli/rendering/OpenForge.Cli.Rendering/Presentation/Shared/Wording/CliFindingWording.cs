using OpenForge.Cli.Core.Presentation.Shared.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliFindingWording
{
    internal static string BeforeHash(string hash) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.BeforeHash(hash);
    internal static string AfterHash(string hash) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.AfterHash(hash);
    internal static string? Resolution(CliResolution resolution, string? command) => resolution switch
    {
        CliResolution.SafeExact => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCanBeFixedAutomatically(),
        CliResolution.GuidedChoice => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNeedsAChoice(),
        CliResolution.TargetedOperation => command is not null ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatUse($"{command}")
            : throw new InvalidOperationException("A targeted-operation finding requires a command action."),
        CliResolution.ManualDecision => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFixByHand(),
        CliResolution.BlockedRepair => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCannotBeFixedAutomatically(),
        CliResolution.Informational => null,
        _ => throw new ArgumentOutOfRangeException(nameof(resolution)),
    };

    internal static string InvalidInput(string command, string problem) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.InvalidInput(command, problem);
    internal static string WorkspaceUnavailable(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WorkspaceUnavailable(path);
    internal static string WorkspaceUnsafe(string path, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WorkspaceUnsafe(path, reason);
    internal static string WorkspaceLockUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAnotherOpenForgeCommandHoldsTheWorkspaceLockNothingWasChanged();
    internal static string WorkspaceLockUnavailableReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelAnotherOpenForgeCommandHoldsTheWorkspaceLock();
    internal static string FilesystemOperationFailed() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheFilesystemOperationFailed();
    internal static string FilesystemAccessDenied() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesystemAccessWasDenied();
    internal static string InvalidJson() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheContentIsNotValidJson();

    internal static string PlainCause(string cause)
    {
        ArgumentNullException.ThrowIfNull(cause);
        var value = cause.Trim().TrimEnd('.');
        if (value.Contains("LineNumber:", StringComparison.OrdinalIgnoreCase)
            && value.Contains("BytePositionInLine:", StringComparison.OrdinalIgnoreCase))
        {
            return InvalidJson();
        }

        if (value.Contains("Unable to translate bytes", StringComparison.OrdinalIgnoreCase))
        {
            return InvalidJson();
        }

        var accessIndex = Earlier(
            FirstIndex(value, "UnauthorizedAccessException", "0x80070005"),
            FirstIndex(value, "Access is denied", "Access to the path"));
        var operationIndex = Earlier(
            FirstIndex(value, "IOException", "0x80070020"),
            FirstIndex(value, "The process cannot access", "being used by another process"));
        if (accessIndex < 0 && operationIndex < 0)
        {
            return value;
        }

        var isAccessFailure = operationIndex < 0
            || accessIndex >= 0 && accessIndex < operationIndex;
        var markerIndex = isAccessFailure ? accessIndex : operationIndex;
        var inUse = !isAccessFailure
            && value.Contains("being used by another process", StringComparison.OrdinalIgnoreCase);
        var reason = isAccessFailure
            ? FilesystemAccessDenied()
            : inUse ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheFileIsInUseByAnotherProcess() : FilesystemOperationFailed();

        // Everything from the marker onward is runtime text that must not reach a reader, but the
        // path it names is the only part that makes the finding actionable. Keep whatever subject
        // the message carries: the prefix when the caller framed the failure ("Writing X failed:"),
        // otherwise the path the operating system quoted inside its own sentence.
        var prefix = value[..markerIndex].Trim().TrimEnd(':').Trim();
        if (prefix.Length != 0)
        {
            return $"{prefix}: {reason}";
        }

        var quoted = QuotedPath(value);
        return quoted is null ? reason : $"{quoted}: {reason}";
    }

    /// <summary>
    /// Reads the path an operating-system message quotes inside its own sentence, so a classified
    /// failure keeps naming what failed. Returns null when the message quotes nothing.
    /// </summary>
    private static string? QuotedPath(string value)
    {
        var open = value.IndexOf('\'', StringComparison.Ordinal);
        if (open < 0)
        {
            return null;
        }

        var close = value.IndexOf('\'', open + 1);
        if (close <= open + 1)
        {
            return null;
        }

        // Returned exactly as the operating system quoted it, including its native separator.
        // Normalising to slashes here was tried and reverted: the capture redaction that replaces
        // machine-specific roots matches the native form, so rewriting the separator first made
        // absolute temporary paths leak into snapshots. The separator style of this one message is
        // cosmetic; a reproducible capture is not.
        var candidate = value[(open + 1)..close].Trim();
        return candidate.Length == 0 ? null : candidate;
    }

    internal static string CauseSentence(string cause)
    {
        ArgumentNullException.ThrowIfNull(cause);
        var value = PlainCause(cause);
        return cause.TrimEnd().EndsWith(".", StringComparison.Ordinal)
            ? $"{value.TrimEnd('.')}."
            : value;
    }

    internal static string TargetChanged(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.TargetChanged(path);
    internal static string TargetChangedDuringApply(string path, long completed, long total) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.TargetChangedDuringApply(path, completed, total);
    internal static string TargetUnsafe(string path, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.TargetUnsafe(path, reason);
    internal static string GeneratedRegionUnsafe(string path, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.GeneratedRegionUnsafe(path, reason);
    internal static string ProjectionUnavailable(string path, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.ProjectionUnavailable(path, reason);
    internal static string MetadataIncomplete(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.MetadataIncomplete(path);
    internal static string MetadataUnsafe(string path, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.MetadataUnsafe(path, reason);
    internal static string RecoveryUnavailable(string store) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.RecoveryUnavailable(store);
    internal static string RecoveryConflict(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.RecoveryConflict(path);
    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.RecoveryRetained(path);
    internal static string RecoveryFailed() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheChangesWereAppliedButTheFinalStateOfTheRecoveryBundleIsUnknown();
    internal static string WriteFailed(string path, long completed, long total, string recovery) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WriteFailed(path, completed, total, recovery);
    internal static string VerificationFailed(string path, string recovery) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.VerificationFailed(path, recovery);
    internal static string SourceAmbiguous(string reference) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.SourceAmbiguous(reference);
    internal static string SourceUnsafe(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.SourceUnsafe(path);
    internal static string OperationFailed(string command, string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OperationFailed(command, reason);

    internal static string ConfirmationRequired(string command) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.ConfirmationRequired(command);
    internal static string WorkspaceNotDirectory(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WorkspaceNotDirectory(path);
    internal static string TargetOccupied(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.TargetOccupied(path);
    internal static string InspectionIncomplete(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.InspectionIncomplete(path);

    internal static string LifecyclePublicationFailed() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheChangesWereAppliedButTheOwnershipRecordAgentsOpenForgeLockJsonCouldNotBeWritten();
    internal static string LifecycleUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAgentsOpenForgeLockJsonCouldNotBeReadCompletely();
    internal static string LifecycleBlocked(string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.LifecycleBlocked(reason);

    internal static string OwnershipObservation(string what) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OwnershipObservation(what);
    internal static string OwnershipConflict(string path, string owner, string command) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OwnershipConflict(path, owner, command);
    internal static string OwnershipClaimed(string path, string owner, string command, string verb) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.OwnershipClaimed(path, owner, command, verb);
    internal static string ManagedDivergence(string path) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.ManagedDivergence(path);

    internal static string PermissionRequired(string command) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.PermissionRequired(command);
    internal static string PermissionDeclined() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageYouDeclinedTheDestinationsSoNothingWasChanged();
    internal static string PermissionsInvalid(string reason) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.PermissionsInvalid(reason);
    internal static string PermissionsUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAgentsOpenForgeJsonCouldNotBeRead();
    internal static string PermissionsChanged() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageAgentsOpenForgeJsonChangedAfterThePlanWasMadeNothingWasChanged();
    internal static string PermissionWriteFailed() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheGrantCouldNotBeSavedToAgentsOpenForgeJson();

    internal static string IdentityCollision(string id) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.IdentityCollision(id);
    internal static string RouteAmbiguous(string id) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.RouteAmbiguous(id);
    internal static string SelectorAmbiguous(string value) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.SelectorAmbiguous(value);
    internal static string SelectorUnsafe(string value) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.SelectorUnsafe(value);

    internal static string PayloadUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkBundledInThisCliCouldNotBeReadCompletely();
    internal static string PayloadInvalid() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkBundledInThisCliIsInvalid();
    internal static string FrameworkUnavailable() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkFilesThisCommandNeedsCouldNotBeReadCompletely();
    internal static string FrameworkUnsafe() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkFilesThisCommandNeedsCouldNotBeVerified();

    internal static string SelectionRequired(string command, bool promptUnavailable)
        => promptUnavailable
            ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatNeedsToKnowWhichPackagesPassTheirIdsOrAllThisSessionCannotAsk($"{command}")
            : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatNeedsToKnowWhichPackagesPassTheirIdsOrAll($"{command}");

    internal static string InteractionEnded() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageInputEndedBeforeAChoiceWasMadeNothingWasChanged();
    internal static string Interrupted(string command) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.Interrupted(command);
    internal static string Interrupted(string command, long completed, long total)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.Interrupted(command, completed, total);

    internal static string UnknownId(string subject, string id) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownId(subject, id);
    internal static string UnknownSource(string id) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownSource(id);

    private static int FirstIndex(string value, string first, string second)
    {
        var firstIndex = value.IndexOf(first, StringComparison.OrdinalIgnoreCase);
        var secondIndex = value.IndexOf(second, StringComparison.OrdinalIgnoreCase);
        return firstIndex < 0
            ? secondIndex
            : secondIndex < 0
                ? firstIndex
                : Math.Min(firstIndex, secondIndex);
    }

    private static int Earlier(int first, int second)
        => first < 0
            ? second
            : second < 0
                ? first
                : Math.Min(first, second);
}
