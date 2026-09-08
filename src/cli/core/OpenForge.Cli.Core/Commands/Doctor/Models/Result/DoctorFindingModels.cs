namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorFindingSeverity
{
    Information,
    Warning,
    Error,
}

internal enum DoctorResolutionLane
{
    SafeExact,
    GuidedChoice,
    TargetedOperation,
    ManualDecision,
    BlockedRepair,
    Informational,
}

internal enum DoctorFindingKind
{
    WorkspaceUnavailable,
    WorkspaceNotDirectory,
    WorkspaceAgentsMissing,
    WorkspaceAgentsInaccessible,
    WorkspaceLoaderMissing,
    WorkspaceLoaderUnreadable,
    WorkspaceLoaderMalformed,
    WorkspaceEntryMissing,
    WorkspaceEntryAmbiguous,
    WorkspaceEntryCompatibilityCollision,
    WorkspaceSourceIdCollision,
    WorkspacePathInvalid,
    WorkspacePathContainment,
    WorkspacePhysicalAlias,
    WorkspaceFrontmatterMalformed,
    WorkspaceFrontmatterDuplicate,
    WorkspaceParseIncomplete,
    WorkspaceUnsupportedSource,
    WorkspaceRootMissing,
    WorkspaceRootUnreachable,
    WorkspaceDetached,

    LibraryRecordMalformed,
    LibraryRecordUnavailable,
    LibrarySourceRootInvalid,
    LibrarySourceRootAliased,
    LibraryInventoryIncomplete,
    LibraryProjectionMissing,
    LibraryProjectionDangling,
    LibraryProjectionRetargeted,
    LibraryPathCollision,
    LibraryLinkCapabilityUnsupported,
    LibraryExtensionCollision,
    LibraryRecoverySafeExact,

    RecoveryBundleRecognized,
    RecoveryDraftRecognized,
    RecoveryBundleCollision,
    RecoveryProvenanceUnavailable,

    RouteEntrypointMissing,
    RouteEntrypointDuplicate,
    RouteEscape,
    RouteUnreachable,
    RouteDetached,
    RouteMetadataRequiredMissing,
    RouteTitleInvalid,
    RouteAxiomsInvalid,
    RouteGeneratedRegionStale,
    RouteGeneratedRegionMissing,
    RouteGeneratedRegionMalformed,
    RouteGeneratedRegionMisplaced,
    RouteGeneratedRegionDuplicate,
    RouteGeneratedEntryMissing,
    RouteGeneratedEntryExtra,
    RouteGeneratedEntryOrder,
    RouteGeneratedEntryPath,
    RouteGeneratedEntryDescription,
    RouteGeneratedEntryTags,
    RouteOverwriteOrphan,
    RouteOverwriteIndependentIndex,
    RouteCompatibilityConflict,

    ReferenceTargetValid,
    ReferenceTargetMissing,
    ReferenceFragmentMissing,
    ReferenceFragmentUnverified,
    ReferenceDestinationMalformed,
    ReferenceDestinationAbsolute,
    ReferenceDestinationQuery,
    ReferenceDestinationEncoding,
    ReferenceTargetOutsideWorkspace,
    ReferenceTargetPhysicalEscape,
    ReferenceTargetAlias,
    ReferenceTargetUnreadable,
    ReferenceTargetUnsupported,
    ReferenceImage,
    ReferenceExternalUnchecked,
    ReferenceCycle,
    ReferenceRepeat,
    ReferenceSameTargetPath,
    ReferenceSameTargetCase,
    ReferenceSameTargetEncoding,
    ReferenceSameTargetFragment,
    ReferenceCandidateFilename,
    ReferenceCandidateTitle,
    ReferenceCandidateLiteralContent,
    ReferenceCandidateRouteNeighborhood,
    ReferenceCandidatesNone,
    ReferenceCandidatesOne,
    ReferenceCandidatesSeveral,

    FrameworkInstallAbsent,
    FrameworkInstallIncomplete,
    FrameworkManagedMissing,
    FrameworkManagedChanged,
    FrameworkLifecycleEvidenceUnavailable,
    FrameworkLifecycleEvidenceMalformed,
    FrameworkLifecycleUntrusted,
    FrameworkLifecycleSectionMissing,
    FrameworkBridgeBoundary,
    FrameworkRootRegionBoundary,
    FrameworkOwnershipConflict,
    FrameworkPartialLifecycle,
    FrameworkPartialRecovery,
    FrameworkDistributedPayloadDefect,

    ExtensionLifecycleDocumentMissing,
    ExtensionLifecycleDocumentInvalid,
    ExtensionLifecycleUntrusted,
    ExtensionLifecycleSectionMissing,
    ExtensionManifestMissing,
    ExtensionManifestMalformed,
    ExtensionDuplicateId,
    ExtensionUnknownId,
    ExtensionVersionInvalid,
    ExtensionManagedMissing,
    ExtensionManagedChanged,
    ExtensionDependencyMissing,
    ExtensionDependencyCycle,
    ExtensionDependencyIncompatible,
    ExtensionSourceUnavailable,
    ExtensionCatalogueUnavailable,
    ExtensionPartialLifecycle,
    ExtensionOwnershipCollision,
    ExtensionBridgeRegistration,
}

internal sealed record DoctorFinding
{
    public required DoctorFindingKind Kind { get; init; }

    public required DoctorFindingSeverity Severity { get; init; }

    public required string Message { get; init; }

    public required DoctorSubject Subject { get; init; }

    public required IReadOnlyList<DoctorEvidence> Evidence { get; init; }

    public required DoctorProvenance Provenance { get; init; }

    public required DoctorResolutionLane Resolution { get; init; }

    public required DoctorCandidateSet? Candidates { get; init; }

    public required DoctorExactProposal? Proposal { get; init; }

    public required IReadOnlyList<DoctorNextAction> Actions { get; init; }
}
