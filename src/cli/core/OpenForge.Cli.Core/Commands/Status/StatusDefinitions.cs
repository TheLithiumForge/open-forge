using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status;

internal static class StatusDefinitions
{
    private static readonly StatusFindingDefinition[] FindingDefinitions =
    [
        Finding(StatusFindingCode.InvalidInput, "invalid-input", CliSemanticStatus.Invalid),
        Finding(StatusFindingCode.WorkspaceUnavailable, "workspace-unavailable", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.WorkspaceNotDirectory, "workspace-not-directory", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.WorkspaceUnsafe, "workspace-unsafe", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.EntryUnavailable, "entry-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.EmbeddedFrameworkUnavailable, "embedded-framework-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ContextInventoryIncomplete, "context-inventory-incomplete", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.StartupContextUnavailable, "startup-context-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ContinuityContextUnavailable, "continuity-context-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.RootCategoriesUnavailable, "root-categories-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.GeneratedNavigationChanged, "generated-navigation-changed", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.GeneratedNavigationMissing, "generated-navigation-missing", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.GeneratedNavigationUnavailable, "generated-navigation-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.GeneratedNavigationBlocked, "generated-navigation-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.FrameworkLifecycleUntrusted, "framework-lifecycle-untrusted", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.FrameworkLifecycleIncomplete, "framework-lifecycle-incomplete", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.FrameworkLifecycleBlocked, "framework-lifecycle-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.FrameworkTargetChanged, "framework-target-changed", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.FrameworkTargetMissing, "framework-target-missing", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.FrameworkTargetUnavailable, "framework-target-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.FrameworkTargetBlocked, "framework-target-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.ExtensionLifecycleUntrusted, "extension-lifecycle-untrusted", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ExtensionLifecycleIncomplete, "extension-lifecycle-incomplete", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ExtensionLifecycleBlocked, "extension-lifecycle-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.ExtensionSourceUnavailable, "extension-source-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ExtensionTargetChanged, "extension-target-changed", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.ExtensionTargetMissing, "extension-target-missing", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.ExtensionTargetUnavailable, "extension-target-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.ExtensionTargetBlocked, "extension-target-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.RecoveryCandidateVerified, "recovery-candidate-verified", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.RecoveryDraftIncomplete, "recovery-draft-incomplete", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.RecoveryFinalMalformed, "recovery-final-malformed", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.RecoveryFinalUnsupported, "recovery-final-unsupported", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.RecoveryFinalUnavailable, "recovery-final-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.RecoveryCatalogueUnavailable, "recovery-catalogue-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.LibraryRecordMalformed, "library-record-malformed", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.LibraryRecordUnavailable, "library-record-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.LibrarySourceRootInvalid, "library-source-root-invalid", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.LibrarySourceRootAliased, "library-source-root-aliased", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.LibrarySourceRootUnavailable, "library-source-root-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.LibraryProjectionMissing, "library-projection-missing", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.LibraryProjectionChanged, "library-projection-changed", CliSemanticStatus.Attention),
        Finding(StatusFindingCode.LibraryProjectionUnavailable, "library-projection-unavailable", CliSemanticStatus.Incomplete),
        Finding(StatusFindingCode.LibraryProjectionBlocked, "library-projection-blocked", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.LibraryExtensionCollision, "library-extension-collision", CliSemanticStatus.Blocked),
        Finding(StatusFindingCode.OperationFailed, "operation-failed", CliSemanticStatus.Failed),
        Finding(StatusFindingCode.Interrupted, "interrupted", CliSemanticStatus.Interrupted),
    ];

    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "status";
    internal const string TokenEstimator = "ceiling-characters-divided-by-four";

    internal static CliSyntaxDefinition StatusCommand { get; } = new(
        CommandIdentity,
        "Inspect workspace, context, lifecycle, generated-navigation, and recovery status.");

    internal static CliNextAction AttentionNextAction { get; } = new(
        "open-forge doctor",
        "Inspect the reported operational findings before relying on this Status result.");

    internal static CliNextAction IncompleteNextAction { get; } = new(
        "open-forge doctor",
        "Inspect the unavailable operational facts before relying on this Status result.");

    internal static CliNextAction InvalidNextAction { get; } = new(
        "open-forge status --help",
        "Correct the Status input, then rerun the request.");

    internal static CliNextAction BlockedNextAction { get; } = new(
        "open-forge status",
        "Resolve the blocked workspace boundary, then rerun the same Status request.");

    internal static CliNextAction FailedNextAction { get; } = new(
        "open-forge status --verbose",
        "Report the failure and retry Status with bounded diagnostics.");

    internal static CliNextAction InterruptedNextAction { get; } = new(
        "open-forge status",
        "Rerun the same Status request.");

    internal static string ReadFindingCode(StatusFindingCode code) => Read(code).MachineName;

    internal static CliSemanticStatus ReadFindingStatus(StatusFindingCode code) => Read(code).Status;

    private static StatusFindingDefinition Read(StatusFindingCode code)
    {
        var index = (int)code;
        if ((uint)index >= (uint)FindingDefinitions.Length || FindingDefinitions[index].Code != code)
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The Status finding code is not defined.");
        }

        return FindingDefinitions[index];
    }

    private static StatusFindingDefinition Finding(
        StatusFindingCode code,
        string machineName,
        CliSemanticStatus status)
        => new(code, machineName, status);

    private sealed record StatusFindingDefinition(
        StatusFindingCode Code,
        string MachineName,
        CliSemanticStatus Status);
}
