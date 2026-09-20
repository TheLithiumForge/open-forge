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

    LibraryOwnershipObservation,
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
    ReferenceSameTargetPath,
    ReferenceSameTargetCase,
    ReferenceSameTargetEncoding,
    ReferenceSameTargetFragment,

    FrameworkInstallAbsent,
    FrameworkManagedMissing,
    FrameworkManagedChanged,
    FrameworkLifecycleEvidenceUnavailable,
    FrameworkOwnershipObservation,
    ExtensionOwnershipObservation,
    FrameworkBridgeBoundary,
    FrameworkRootRegionBoundary,
    FrameworkOwnershipConflict,
    FrameworkPartialLifecycle,
    FrameworkPartialRecovery,
    FrameworkDistributedPayloadDefect,

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
