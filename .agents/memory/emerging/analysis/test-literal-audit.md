---
open-forge:
  description: Counts per file of the pre-existing magic strings, literal-string assertions and tier problems in the CLI tests that the Task 86 branch touched, the input for Task 88
  tags: [Memory, Analysis, Contextual, Testing, Evidence, CLI]
---

# Test Literal Audit

On 2026-10-24 an Astra high audit checked every test file the Task 86 branch touched against the maintainer's rules: no magic strings, no literal-string assertions of user-facing text, and clean test tiers. Violations on lines the branch added or changed were fixed in Task 86. These are the older ones, at commit `7c86a8ba3`, for [Task 88](../../working/cli-development/tasks/task88-test-literal-cleanup.md). Paths are relative to `src/cli/tests/`, with `E` for EndToEnd, `I` for Integration, `U` for Unit and `T` for test support.

## Pre-existing violations

Counts are sorted by the combined count, descending. Rule 3 uses repair groups rather than individual call sites.

| File | Rule 1 | Rule 2 | Rule 3 | Total |
|---|---:|---:|---:|---:|
| I/Commands/Extension/Remove/ExtensionRemoveApplicationIntegrationTests.cs | 117 | 0 | 0 | 117 |
| E/Journeys/F26BundledExtensionsJourneyTests.cs | 114 | 0 | 0 | 114 |
| I/Commands/Remove/RemoveRootPathIntegrationTests.cs | 107 | 4 | 0 | 111 |
| U/Commands/Extension/Remove/ExtensionRemoveDefinitionsTests.cs | 101 | 3 | 0 | 104 |
| I/Presentation/Invariants/CliReportInvariantsTests.cs | 83 | 19 | 1 | 103 |
| I/Commands/Extension/Update/ExtensionUpdateMutationIntegrationTests.cs | 84 | 2 | 0 | 86 |
| E/Journeys/F01InstallReadinessJourneyTests.cs | 56 | 26 | 0 | 82 |
| I/Commands/Remove/RemoveRootPathSafetyIntegrationTests.cs | 70 | 0 | 0 | 70 |
| I/Commands/Extension/Update/ExtensionUpdatePlanningIntegrationTests.cs | 65 | 0 | 0 | 65 |
| I/Commands/Index/IndexApplicationIntegrationTests.cs | 47 | 17 | 0 | 64 |
| E/Journeys/F11PackageLifecycleJourneyTests.cs | 45 | 15 | 1 | 61 |
| U/Commands/Extension/Update/ExtensionUpdateDefinitionsTests.cs | 56 | 1 | 1 | 58 |
| U/Commands/Extension/Install/ExtensionInstallContractTests.cs | 47 | 0 | 0 | 47 |
| I/Commands/Cleanup/CleanupCatalogueIntegrationTests.cs | 38 | 3 | 0 | 41 |
| I/Commands/Extension/Remove/ExtensionRemoveBeforeOutputSnapshotTests.cs | 35 | 6 | 0 | 41 |
| E/Journeys/F19PartialOperationJourneyTests.cs | 38 | 0 | 0 | 38 |
| I/Commands/Route/Move/RouteMoveIntegrationWorkspace.cs | 38 | 0 | 0 | 38 |
| I/Commands/Status/StatusJsonAssertions.cs | 36 | 0 | 0 | 36 |
| E/Journeys/RootRemoveRecoveryConsumerJourneyTests.cs | 35 | 0 | 0 | 35 |
| E/Journeys/F23RecoveryCleanupJourneyTests.cs | 23 | 9 | 0 | 32 |
| I/Commands/Extension/Install/ExtensionInstallBeforeOutputSnapshotTests.cs | 30 | 2 | 0 | 32 |
| E/Journeys/F14DestinationConsentJourneyTests.cs | 27 | 4 | 0 | 31 |
| I/Commands/Route/Init/Framework/RouteInitFrameworkSafetyIntegrationTests.cs | 31 | 0 | 0 | 31 |
| E/PublishedCleanupProcessTests.cs | 20 | 9 | 1 | 30 |
| U/Commands/Repair/LibraryRepairProjectionTests.cs | 22 | 5 | 1 | 28 |
| U/Presentation/Route/Init/RouteInitRecoveryConflictPresentationTests.cs | 19 | 9 | 0 | 28 |
| E/PublishedInstallWorkspace.cs | 26 | 0 | 0 | 26 |
| I/Commands/Install/InstallOperationWorkspace.cs | 26 | 0 | 0 | 26 |
| U/Presentation/Route/Move/RouteMoveRecoveryConflictPresentationTests.cs | 19 | 7 | 0 | 26 |
| U/Presentation/Route/Update/RouteUpdateRecoveryConflictPresentationTests.cs | 18 | 8 | 0 | 26 |
| I/Commands/Extension/Update/ExtensionUpdateBeforeOutputSnapshotTests.cs | 25 | 0 | 0 | 25 |
| U/Presentation/Route/Create/RouteCreateRecoveryConflictPresentationTests.cs | 17 | 8 | 0 | 25 |
| I/Commands/Route/Remove/RouteRemoveStatusDoctorIntegrationTests.cs | 23 | 1 | 0 | 24 |
| I/Commands/Route/Remove/RouteRemoveIntegrationWorkspace.cs | 20 | 0 | 0 | 20 |
| U/Commands/Cleanup/CleanupPresentationContractTests.cs | 13 | 6 | 0 | 19 |
| I/Commands/Index/IndexOperationWorkspace.cs | 16 | 0 | 0 | 16 |
| U/Commands/Doctor/Shared/Rendering/DoctorBeforeOutputFixtures.cs | 16 | 0 | 0 | 16 |
| E/Journeys/F20WorkspaceLockJourneyTests.cs | 14 | 1 | 0 | 15 |
| I/Framework/Recovery/LibraryRecoveryBundleIntegrationTests.cs | 15 | 0 | 0 | 15 |
| I/Commands/Extension/Remove/ExtensionRemovePriorRecoveryIntegrationTests.cs | 13 | 0 | 0 | 13 |
| I/Commands/Library/Shared/GitIgnore/LibraryGitIgnoreApplicationIntegrationTests.cs | 13 | 0 | 0 | 13 |
| I/Commands/Install/InstallConfigurationFrontmatterIntegrationTests.cs | 12 | 0 | 0 | 12 |
| I/Commands/Shared/Snapshots/CommandOutputDetailComparison.cs | 12 | 0 | 0 | 12 |
| U/Commands/Cleanup/CleanupDefinitionsAndBindingContractTests.cs | 10 | 2 | 0 | 12 |
| U/Commands/Shared/Rendering/SharedWireVocabularyTests.cs | 12 | 0 | 0 | 12 |
| E/PublishedCleanupWorkspace.cs | 11 | 0 | 0 | 11 |
| I/Commands/Extension/Install/ExtensionInstallIntegrationWorkspace.cs | 11 | 0 | 0 | 11 |
| I/Commands/Library/Detach/LibraryDetachBeforeOutputSnapshotTests.cs | 11 | 0 | 0 | 11 |
| I/Commands/Repair/RepairIntegrationWorkspace.cs | 11 | 0 | 0 | 11 |
| I/Framework/Libraries/Operational/LibraryResidualAttributionReaderIntegrationTests.cs | 11 | 0 | 0 | 11 |
| I/Framework/Libraries/Operational/Shared/Reading/LibraryObservationWorkspace.cs | 11 | 0 | 0 | 11 |
| U/Framework/Recovery/Models/RecoveryBundleContractTests.cs | 10 | 0 | 1 | 11 |
| I/Commands/Extension/Update/ExtensionUpdateRecoveryHistoryIntegrationTests.cs | 10 | 0 | 0 | 10 |
| I/Commands/Install/InstallPreservationIntegrationTests.cs | 10 | 0 | 0 | 10 |
| I/Commands/Library/Shared/Reading/LibraryReadWorkspace.cs | 10 | 0 | 0 | 10 |
| U/Framework/Libraries/Operational/Shared/Attribution/LibraryResidualFacts.cs | 10 | 0 | 0 | 10 |
| E/Shared/Journeys/PublishedJourneyAssertions.cs | 9 | 0 | 0 | 9 |
| I/Commands/Extension/Install/ExtensionInstallRecoveryHistoryIntegrationTests.cs | 9 | 0 | 0 | 9 |
| I/Commands/Route/Create/RouteCreateIntegrationWorkspace.cs | 9 | 0 | 0 | 9 |
| I/Commands/Shared/LibraryRecovery/LibraryResidualWorkspace.cs | 9 | 0 | 0 | 9 |
| I/Framework/Libraries/Operational/Shared/Attribution/LibraryResidualArchive.cs | 9 | 0 | 0 | 9 |
| U/Commands/Index/Shared/Operation/IndexMutationMapperTests.cs | 7 | 2 | 0 | 9 |
| U/Commands/Route/Remove/RouteRemoveTestData.cs | 9 | 0 | 0 | 9 |
| I/Commands/Cleanup/CleanupJsonAssertions.cs | 8 | 0 | 0 | 8 |
| I/Commands/Remove/RemovePathOperationRevalidationIntegrationTests.cs | 8 | 0 | 0 | 8 |
| U/Commands/Library/Shared/Mutation/LibraryMutationPlanningData.cs | 8 | 0 | 0 | 8 |
| U/Commands/Library/Shared/Planning/LibraryDestinationPolicyTests.cs | 7 | 0 | 1 | 8 |
| I/Commands/Install/InstallBeforeOutputSnapshotTests.cs | 7 | 0 | 0 | 7 |
| I/Commands/Library/Shared/Mutation/LibraryMutationWorkspace.cs | 7 | 0 | 0 | 7 |
| I/Commands/Route/Remove/RouteRemovePriorRecoveryIntegrationTests.cs | 7 | 0 | 0 | 7 |
| I/Commands/Shared/Snapshots/RoutedOutputWorkspace.cs | 7 | 0 | 0 | 7 |
| I/Framework/Recovery/Shared/LibraryResiduals/LibraryRecoveryWorkspace.cs | 7 | 0 | 0 | 7 |
| E/Shared/PublishedProcess/PublishedWorkspaceLockStore.cs | 6 | 0 | 0 | 6 |
| I/Commands/Route/Remove/RouteRemoveRecoveryIntegrationTests.cs | 6 | 0 | 0 | 6 |
| U/Commands/Doctor/DoctorOperationTestSupport.cs | 6 | 0 | 0 | 6 |
| U/Commands/Repair/LibraryRepairData.cs | 6 | 0 | 0 | 6 |
| E/Shared/Journeys/JourneyFrontmatter.cs | 5 | 0 | 0 | 5 |
| E/Shared/Journeys/PublishedJourneyWorkspace.cs | 5 | 0 | 0 | 5 |
| I/Commands/Cleanup/CleanupBeforeOutputSnapshotTests.cs | 5 | 0 | 0 | 5 |
| I/Commands/Library/Attach/LibraryAttachApplicationIntegrationTests.cs | 5 | 0 | 0 | 5 |
| I/Commands/Library/Detach/LibraryDetachApplicationIntegrationTests.cs | 5 | 0 | 0 | 5 |
| I/Commands/Library/Shared/Mutation/LibraryMutationApplicationData.cs | 5 | 0 | 0 | 5 |
| I/Commands/Library/Sync/LibrarySyncApplicationIntegrationTests.cs | 5 | 0 | 0 | 5 |
| I/Commands/Library/Sync/LibrarySyncBeforeOutputSnapshotTests.cs | 5 | 0 | 0 | 5 |
| U/Commands/Doctor/DoctorTypedFactMappingTests.cs | 5 | 0 | 0 | 5 |
| U/Commands/Route/Create/RouteCreateTestData.cs | 5 | 0 | 0 | 5 |
| I/Commands/Cleanup/CleanupIntegrationWorkspace.cs | 4 | 0 | 0 | 4 |
| I/Commands/Library/Attach/LibraryAttachBeforeOutputSnapshotTests.cs | 4 | 0 | 0 | 4 |
| I/Commands/Repair/RepairBeforeOutputSnapshotTests.cs | 4 | 0 | 0 | 4 |
| I/Commands/Route/Init/Framework/RouteInitFrameworkIntegrationWorkspace.cs | 4 | 0 | 0 | 4 |
| I/Commands/Route/Shared/Inclusion/RouteInclusionWorkspace.cs | 4 | 0 | 0 | 4 |
| I/Commands/Route/Update/RouteUpdateIntegrationWorkspace.cs | 4 | 0 | 0 | 4 |
| I/Framework/Mutation/Locking/WorkspaceLockManagerIntegrationTests.cs | 4 | 0 | 0 | 4 |
| T/OpenForgeDocumentSeed.cs | 4 | 0 | 0 | 4 |
| U/Commands/Extension/Shared/Permissions/ExtensionDestinationPolicyTests.cs | 4 | 0 | 0 | 4 |
| U/Commands/Route/Update/RouteUpdateTestData.cs | 4 | 0 | 0 | 4 |
| U/Framework/Recovery/Models/Comparison/LibraryRecoveryEntrySetObservationTests.cs | 4 | 0 | 0 | 4 |
| U/Shell/Presentation/Shared/Help/CliHelpAssertions.cs | 0 | 4 | 0 | 4 |
| I/Commands/Route/Create/RouteCreateBeforeOutputSnapshotTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Route/Init/RouteInitBeforeOutputSnapshotTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Route/Init/RouteInitRecoveryConflictCoordinateIntegrationTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Route/Move/RouteMoveBeforeOutputSnapshotTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Route/Remove/RouteRemoveBeforeOutputSnapshotTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Route/Update/RouteUpdateBeforeOutputSnapshotTests.cs | 3 | 0 | 0 | 3 |
| I/Commands/Shared/Recovery/RecoveryFinalIdentityFixture.cs | 3 | 0 | 0 | 3 |
| I/Commands/Shared/Snapshots/ReadCommandOutputCapture.cs | 3 | 0 | 0 | 3 |
| I/Framework/Recovery/LibrarySelectedFinalReadIntegrationTests.cs | 3 | 0 | 0 | 3 |
| U/Commands/Cleanup/CleanupTestData.cs | 3 | 0 | 0 | 3 |
| U/Commands/Doctor/DoctorLifecycleAbsenceTests.cs | 2 | 0 | 1 | 3 |
| U/Commands/Doctor/Shared/Rendering/DoctorCandidateTestData.cs | 3 | 0 | 0 | 3 |
| U/Commands/Repair/RepairTestData.cs | 3 | 0 | 0 | 3 |
| U/Commands/Route/Init/RouteInitRedTestData.cs | 3 | 0 | 0 | 3 |
| U/Commands/Route/Move/RouteMoveTestData.cs | 3 | 0 | 0 | 3 |
| U/Framework/Mutation/Locking/WorkspaceLockContractTests.cs | 3 | 0 | 0 | 3 |
| E/Shared/Journeys/PublishedLockHolder.cs | 2 | 0 | 0 | 2 |
| I/Commands/Install/InstallStaleRecoveryIntegrationTests.cs | 2 | 0 | 0 | 2 |
| I/Commands/Remove/RemoveRootIntegrationWorkspace.cs | 2 | 0 | 0 | 2 |
| I/Commands/Route/Remove/RouteRemoveRevalidationIntegrationTests.cs | 2 | 0 | 0 | 2 |
| I/Commands/Shared/LibraryRecovery/LibraryRecoveryPermissionIntegrationTests.cs | 2 | 0 | 0 | 2 |
| I/Framework/Recovery/Application/LibraryOrdinaryRecoverySafetyIntegrationTests.cs | 2 | 0 | 0 | 2 |
| I/Framework/Recovery/RecoveryBundleStoreIntegrationTests.cs | 2 | 0 | 0 | 2 |
| T/Snapshots/CommandOutputNormalization.cs | 2 | 0 | 0 | 2 |
| E/Shared/PublishedProcess/PublishedExecutableTarget.cs | 1 | 0 | 0 | 1 |
| I/Commands/Extension/Install/Shared/Permissions/PermissionFixture.cs | 1 | 0 | 0 | 1 |
| I/Commands/Extension/Shared/Recovery/ExtensionRecoveryHistoryFixture.cs | 1 | 0 | 0 | 1 |
| I/Commands/Index/IndexBeforeOutputSnapshotTests.cs | 1 | 0 | 0 | 1 |
| I/Commands/Library/Shared/Mutation/LibraryOutputArtifacts.cs | 1 | 0 | 0 | 1 |
| I/Commands/Route/Create/RouteCreateRecoveryConflictCoordinateIntegrationTests.cs | 1 | 0 | 0 | 1 |
| I/Commands/Route/Init/Generic/GenericRouteInitIntegrationWorkspace.cs | 1 | 0 | 0 | 1 |
| I/Framework/Recovery/RecoveryBundleDeletionAttributionIntegrationTests.cs | 1 | 0 | 0 | 1 |
| I/Framework/Recovery/Shared/Deletion/RecoveryDeletionSessionWorkspace.cs | 1 | 0 | 0 | 1 |
| I/Framework/Recovery/Shared/Deletion/RecoveryDeletionStorageBoundaryIntegrationTests.cs | 1 | 0 | 0 | 1 |
| I/TestSupport/TemporaryWorkspaceSafetyTests.cs | 1 | 0 | 0 | 1 |
| T/Snapshots/PlatformSnapshotComparer.cs | 1 | 0 | 0 | 1 |
| U/Commands/Library/Shared/Mutation/LibraryMutationPresentationData.cs | 1 | 0 | 0 | 1 |
| U/TestProcessIsolation.cs | 0 | 0 | 1 | 1 |

**Totals**

| Classification | Rule 1 locations | Rule 2 locations | Rule 3 groups |
|---|---:|---:|---:|
| Introduced by this branch | **486** | **21** | **3** |
| Pre-existing | **2,141** | **173** | **9** |
| Original audit total | **2,627** | **194** | **12** |
