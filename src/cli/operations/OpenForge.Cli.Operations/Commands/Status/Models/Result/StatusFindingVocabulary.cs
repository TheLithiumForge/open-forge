using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal static class StatusFindingVocabulary
{
    private static readonly StatusFindingDefinition[] Definitions =
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
        Finding(StatusFindingCode.FrameworkOwnershipObservation, "framework-ownership-observation", CliSemanticStatus.Complete),
        Finding(StatusFindingCode.ExtensionOwnershipObservation, "extension-ownership-observation", CliSemanticStatus.Complete),
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
        Finding(StatusFindingCode.LibraryOwnershipObservation, "library-ownership-observation", CliSemanticStatus.Complete),
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
        Finding(StatusFindingCode.GeneratedNavigationMetadataInvalid, "generated-navigation-metadata-invalid", CliSemanticStatus.Attention),
    ];

    internal static string ReadMachineName(StatusFindingCode code) => Read(code).MachineName;

    internal static CliSemanticStatus ReadStatus(StatusFindingCode code) => Read(code).Status;

    private static StatusFindingDefinition Read(StatusFindingCode code)
    {
        var index = (int)code;
        if ((uint)index >= (uint)Definitions.Length || Definitions[index].Code != code)
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The Status finding code is not defined.");
        }

        return Definitions[index];
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
