---
open-forge:
  description: Task 30 phase 7-S slice 70 mapping every journey and command status to its cheapest proving boundary and naming each intentionally process-only assertion
  tags: [Memory, Working, CLI, Task, Subtask, Testing, Scenarios, Contextual, Active]
---

# 70 — Coverage matrix

## Outcome

This is the current test inventory measured on 2026-09-17. It is an evidence-
cost map, not a recommendation to move tests. Every command/status pair is a
row, including rows with no executed command situation. The status names and
codes are the accepted G4 vocabulary:

| Status | Exit code |
| --- | ---: |
| `completed` | 0 |
| `completed-with-warnings` | 2 |
| `incomplete` | 3 |
| `invalid-input` | 4 |
| `blocked` | 5 |
| `failed` | 1 |
| `cancelled` | 130 |

Boundary markers used below:

| Marker | Meaning and relative cost |
| --- | --- |
| `I-run` | In-process integration through the composed application boundary (`CliCoreApplication.RunAsync`, `CliHostCapture`, or `ReadCommandOutputCapture`), including argv, exit, streams, and result assertions. This is the normal proving boundary. |
| `I-result` | In-process integration that executes the command operation or renders its real result model, but does not exercise the complete shell composition. It is still cheaper than a child process and is labelled so it is not mistaken for full shell evidence. |
| `I-event` | Integration pipeline evidence formed from an explicitly supplied terminal event/result. It proves the terminal rendering and exit policy, not the physical cause of that event. |
| `U` | Unit result, definition, or renderer contract. It is the cheapest boundary for that contract, but does not prove a command journey. |
| `P` | Published executable process evidence. It is intentionally expensive and is only the cheapest current evidence where no command-level in-process situation exists. |
| `W` | An existing integration test is skipped on the current Windows host; the Linux permission situation is not proven here. |
| `G` | Explicit gap: no current command situation at any executable boundary was found for the row. Generic status-policy or renderer fixtures do not close this gap. |

The matrix uses the cheapest boundary that actually proves the stated fact.
Published process smoke that repeats a row already covered by `I-run` or
`I-result` remains useful as smoke, but is not counted as the cheapest proof.

## Journey matrix

The shape counts below are the measured mentions in integration files, not a
claim that every mention is a complete journey. They are included to keep the
new scenario work tied to the current test tree.

| Journey or shape | Cheapest current boundary | Existing evidence and disposition |
| --- | --- | --- |
| Clean/current workspace | `I-run` / `I-result` | Normal and dry-run cases in the `*BeforeOutputSnapshotTests` classes and command application tests. |
| Missing or unavailable workspace/source | `I-run` / `I-result` | Context missing fragments, library missing sources, unreadable records, and recovery-store-unavailable cases. |
| Malformed workspace/source | `I-run` / `I-result` | 66 integration files mention malformed seeded input; command snapshot and boundary tests preserve the resulting status and findings. |
| Dirty or changed workspace | `I-run` | `WorkspaceShapeJourneyIntegrationTests.DirtyWorkspaceBlocksInstallWithoutWrites` seeds a changed managed file through `InstallOperationWorkspace` and runs composed Install through `CliCoreApplication.RunAsync`; it asserts UTF-8-safe JSON, blocked exit 5, the managed-divergence subject and `Next`, no workspace writes, and not-required recovery. Existing individual drift cases remain narrower result evidence. |
| Nested workspace/source | `I-run` / `I-result` | 20 integration files mention the seeded `nested` shape. The existing route, context, library, and recovery fixtures cover nested topology in narrower journeys. |
| Externally sourced content | `I-run` / `I-result` | 64 integration files mention the seeded `external` shape, including library sources, extension catalogues, links, and outside Markdown. |
| Relocated workspace shape | `I-run` | `WorkspaceShapeJourneyIntegrationTests.RelocatedWorkspaceRemainsCurrentWithoutWrites` physically moves an installed isolated workspace, runs composed Status through `CliCoreApplication.RunAsync`, and asserts the explicit relocated path, complete status, empty findings/`Next`, null recovery, and unchanged files. |
| Relocated published artifact | `P` | `PublishedEmbeddedPayloadProcessTests.RelocatedArtifactReachesEmbeddedPayload` and `.RelocatedArtifactReachesEmbeddedExtensions` deliberately copy the published artifact and prove deployment-relative embedded-resource discovery. |
| Interactive selection, permission, or confirmation | `I-run` | Command interaction application tests and permission fixtures exercise prompts, redirected input, non-interactive refusal, and approval invalidation. The platform-limited permission cases are listed separately below. |
| Lock/refusal and partial mutation/recovery | `I-result` / `I-run` | The command snapshot suites and revalidation/recovery integration suites prove lock-held, occupied, unsafe, partial, and recovery consequences without needing a child process. |
| Multi-command lifecycle | `I-run` | Install/status/update, extension install/inspect/update/remove, library attach/inspect/list/sync/detach, route init/create/inspect/list/move/update/remove, and doctor/repair/cleanup chains are represented by application or lifecycle integration fixtures. Published versions are smoke only unless the process boundary is the subject. |
| Shell help, parser, version, and option boundaries | `I-run` for command semantics; `P` for published shell smoke | `PublishedShellBoundaryProcessTests` and `CliProcessTests` prove the published executable's parser and streams. Equivalent command parsing and invalid-input rows remain in-process where available. |
| Command cancellation | `I-run` / `I-result` for status 130; `P` for child ownership | Command tests cancel the in-process token and assert status 130. `CliProcessTests.PublishedProcessCancellationKillsAndDrainsOwnedChild` deliberately proves child kill/drain ownership. |

## Command/status matrix

The evidence names are the current test classes and representative situations
read from those classes. A `G` row is not inferred from a missing filename: it
means no executed command situation was found, even where a generic renderer or
definition test accepts the status.

| Command | Status | Cheapest current boundary | Current situation evidence |
| --- | --- | --- | --- |
| `cleanup` | `completed` | `I-run` | `CleanupApplicationIntegrationTests`; `CleanupBeforeOutputSnapshotTests` — `nothing-to-remove`, `two-bundles-one-draft`, `dry-run`. |
| `cleanup` | `completed-with-warnings` | `G` | No Cleanup situation returns `Attention`; only status-policy/next-action contract coverage was found. |
| `cleanup` | `incomplete` | `I-result` | `CleanupBeforeOutputSnapshotTests` — `StoreUnreadable`. |
| `cleanup` | `invalid-input` | `I-run` | `CleanupCompositionIntegrationTests`; `CleanupBeforeOutputSnapshotTests.InvalidInput`. |
| `cleanup` | `blocked` | `I-run` | `CleanupApplicationIntegrationTests`; `CleanupBeforeOutputSnapshotTests` — `damaged-bundle`, `lock-held`. |
| `cleanup` | `failed` | `I-result` | `CleanupBeforeOutputSnapshotTests` — `deletion-failed-partial`; the Windows file-sharing case runs on this host. |
| `cleanup` | `cancelled` | `I-result` | `CleanupBeforeOutputSnapshotTests.CancelledBetweenRealDeletionStages`. |
| `context` | `completed` | `I-run` | `ContextBeforeOutputSnapshotTests` — `startup`, `one-source`, `follow-links`; `ContextApplicationIntegrationTests`. |
| `context` | `completed-with-warnings` | `I-run` | `ContextBeforeOutputSnapshotTests` — `section`, `section-missing`; `ContextOperationFindingTests`. |
| `context` | `incomplete` | `I-run` | `ContextBeforeOutputSnapshotTests` — `broken-followed-link`, `unreadable-source`; `ContextApplicationIntegrationTests`. |
| `context` | `invalid-input` | `I-run` | `ContextBeforeOutputSnapshotTests` — `unknown-source`, `invalid-content`; `ContextApplicationIntegrationTests`. |
| `context` | `blocked` | `I-run` | `ContextBeforeOutputSnapshotTests` — `ambiguous-source`; `ContextApplicationIntegrationTests` blocked workspace/physical escape. |
| `context` | `failed` | `I-event` | `ContextOperationIntegrationTests` and `ContextOperationFindingTests` form an operation-failed terminal event; no physical failure trigger is supplied. |
| `context` | `cancelled` | `I-event` | `ContextOperationIntegrationTests` and `ContextOperationFindingTests` form an interrupted terminal event. |
| `doctor` | `completed` | `P` | `PublishedDoctorProcessTests` — `healthy` proves exact command exit 0; integration only asserts complete category facts. |
| `doctor` | `completed-with-warnings` | `I-run` | `DoctorApplicationIntegrationTests.RepresentativeWorkspaceRetainsSixDomainsWithoutWrites`; exact exit 2 and JSON status. |
| `doctor` | `incomplete` | `U` | `DoctorBeforeOutputSnapshotTests` — `incomplete`; `LibraryDoctorBoundaryIntegrationTests` checks an incomplete category through `CliHostCapture`, but does not assert the root exit/status. |
| `doctor` | `invalid-input` | `P` | `PublishedDoctorProcessTests` — invalid workspace/input, exact exit 4; unit doctor rendering also has an `invalid-input` fixture. |
| `doctor` | `blocked` | `I-run` | `DoctorApplicationIntegrationTests.BlockedWorkspaceRetainsEveryDomain`; exact exit 5 and JSON status. |
| `doctor` | `failed` | `G` | Only `DoctorOperationEventTests` synthetic event formation was found; no executed Doctor failure situation. |
| `doctor` | `cancelled` | `G` | Only `DoctorOperationEventTests` synthetic interrupted event formation was found; no executed Doctor cancellation situation. |
| `find` | `completed` | `I-run` | `FindBeforeOutputSnapshotTests` — `bare-inventory`, `one-tag`, `no-matches`. |
| `find` | `completed-with-warnings` | `I-run` | `FindBeforeOutputSnapshotTests` — `section-missing`; `FindOperationIntegrationTests`. |
| `find` | `incomplete` | `I-run` | `FindBeforeOutputSnapshotTests` — `unreadable-source`; `FindApplicationIntegrationTests` and document inspection tests. |
| `find` | `invalid-input` | `I-run` | `FindBeforeOutputSnapshotTests` — `invalid-selector`, `invalid-require`; `FindApplicationIntegrationTests`. |
| `find` | `blocked` | `I-run` | `FindBeforeOutputSnapshotTests` — `ambiguous-selector`; `FindApplicationIntegrationTests`. |
| `find` | `failed` | `I-event` | `FindApplicationIntegrationTests.DirectTypedTerminalResultsPreserveHeadlines` supplies a failed result to the renderer; no physical failure cause is induced. |
| `find` | `cancelled` | `I-result` | `FindOperationIntegrationTests` cancellation and interrupted finding assertions. |
| `index` | `completed` | `I-result` | `IndexBeforeOutputSnapshotTests` — `all-current`, `one-stale`, `dry-run-one-stale`, `explicit-source`. |
| `index` | `completed-with-warnings` | `U` | `IndexResultTests` and `IndexDefinitionsTests` cover `recovery-artifact-retained`; no integration situation returns this status. |
| `index` | `incomplete` | `I-result` | `IndexBeforeOutputSnapshotTests` — unavailable catalogue/recovery situation. |
| `index` | `invalid-input` | `I-run` | `IndexApplicationIntegrationTests` and `IndexBeforeOutputSnapshotTests` — `folder-operand`, `unknown-source`. |
| `index` | `blocked` | `I-run` | `IndexApplicationIntegrationTests`; `IndexBeforeOutputSnapshotTests` — `blocked-malformed-leaf`. |
| `index` | `failed` | `I-result` | `IndexBeforeOutputSnapshotTests` write/recovery failure situation. |
| `index` | `cancelled` | `I-result` | `IndexOperationIntegrationTests` and `IndexBeforeOutputSnapshotTests` interrupted boundary. |
| `install` | `completed` | `I-result` | `InstallBeforeOutputSnapshotTests` — `fresh-directory`, `fresh-directory-dry-run`, `already-installed`, `existing-agents-md`. |
| `install` | `completed-with-warnings` | `U` | `InstallDefinitionsContractTests` and renderer contracts accept `recovery-artifact-retained`; no integration Install situation returns `Attention`. |
| `install` | `incomplete` | `I-result` | `InstallBeforeOutputSnapshotTests.RecoveryStoreUnavailable`. |
| `install` | `invalid-input` | `I-run` | `InstallBeforeOutputSnapshotTests.InvalidInput` and `ConfirmationUnavailable`. |
| `install` | `blocked` | `I-result` | `InstallBeforeOutputSnapshotTests` — `occupied-without-force`, `changed-framework-file`; revalidation tests add refusal cases. |
| `install` | `failed` | `I-result` | `InstallBeforeOutputSnapshotTests.PartialWriteFailure`; the Windows file-sharing case runs on this host. |
| `install` | `cancelled` | `I-result` | `InstallBeforeOutputSnapshotTests.Cancelled` and `InstallCompositionIntegrationTests`. |
| `status` | `completed` | `I-run` | `StatusBeforeOutputSnapshotTests` — `healthy`, `no-ownership-record`, `not-installed`. |
| `status` | `completed-with-warnings` | `I-run` | `StatusBeforeOutputSnapshotTests` — `changed-managed-file`, `missing-managed-file`, `stale-entries`, `library-link-missing`, `recovery-bundle-present`. |
| `status` | `incomplete` | `I-run` | `StatusBeforeOutputSnapshotTests` — `unreadable-entry-file`; `StatusWorkspaceIntegrationTests`. |
| `status` | `invalid-input` | `I-run` | `StatusBeforeOutputSnapshotTests` — `invalid-input`; `StatusCompositionIntegrationTests`. |
| `status` | `blocked` | `I-run` | `StatusBeforeOutputSnapshotTests` — `blocked-workspace`; `StatusCompositionIntegrationTests`. |
| `status` | `failed` | `G` | Unit aggregation/seeding and presentation policy accept a failed status, but no executed Status command failure situation was found. |
| `status` | `cancelled` | `G` | Unit aggregation/seeding and presentation policy accept an interrupted status, but no executed Status command cancellation situation was found. |
| `update` | `completed` | `I-result` | `UpdateBeforeOutputSnapshotTests` — `changed-file-replaced`, `missing-file-restored`, `retired-pruned`. |
| `update` | `completed-with-warnings` | `I-result` | `UpdateBeforeOutputSnapshotTests` — `retired-kept`; published smoke also observes warning status through downstream Doctor/Status. |
| `update` | `incomplete` | `G` | No executed Update command situation returning incomplete was found; the status exists in result/definition contracts only. |
| `update` | `invalid-input` | `I-run` | `UpdateBeforeOutputSnapshotTests.InvalidInput` and the published/update composition invalid-input cases. |
| `update` | `blocked` | `I-result` | `UpdateOwnershipSafetyIntegrationTests` and `UpdateOperationIntegrationTests` ownership/divergence refusals. |
| `update` | `failed` | `I-result` | `UpdateBeforeOutputSnapshotTests` partial write failure. |
| `update` | `cancelled` | `I-result` | `UpdateBeforeOutputSnapshotTests.Cancelled` and `UpdateOperationIntegrationTests`. |
| `extension create` | `completed` | `I-run` | `ExtensionCreateApplicationInteractionIntegrationTests`, composition tests, and `ExtensionCreateBeforeOutputSnapshotTests` — `created`, `dry-run`, `already-present`. |
| `extension create` | `completed-with-warnings` | `G` | No Extension Create situation returns `Attention`; only generic definition/renderer policy coverage was found. |
| `extension create` | `incomplete` | `I-result` | `ExtensionCreateBeforeOutputSnapshotTests.CatalogueUnreadable`; current Windows runs the Windows-ACL branch. |
| `extension create` | `invalid-input` | `I-run` | `ExtensionCreateApplicationInteractionIntegrationTests`, composition tests, and snapshot — `missing-id-non-interactive`, `invalid-id`. |
| `extension create` | `blocked` | `I-run` | `ExtensionCreateApplicationInteractionIntegrationTests`, scaffold tests, and snapshot — `destination-has-other-content`. |
| `extension create` | `failed` | `I-result` | `ExtensionCreateBeforeOutputSnapshotTests.PartialWriteFailure`; current Windows runs the Windows file-sharing branch. |
| `extension create` | `cancelled` | `I-run` | `ExtensionCreateApplicationInteractionIntegrationTests` and snapshot — `cancelled`. |
| `extension inspect` | `completed` | `I-run` | `ExtensionInspectBeforeOutputSnapshotTests` — `installed-matches`, `available-not-installed`, `newer-available`, `no-ownership-record`. |
| `extension inspect` | `completed-with-warnings` | `I-run` | `ExtensionInspectBeforeOutputSnapshotTests` — `installed-changed-and-retired` (exit 2); application comparison tests. |
| `extension inspect` | `incomplete` | `I-run` | `ExtensionInspectBeforeOutputSnapshotTests` — `installed-source-missing`; application dependency/fingerprint tests. |
| `extension inspect` | `invalid-input` | `I-run` | Snapshot — `unknown-id`, `invalid-input`; `ExtensionInspectApplicationIntegrationTests`. |
| `extension inspect` | `blocked` | `I-run` | Snapshot — `dependency-cycle`, `ambiguous-source`; application overlap/safety tests. |
| `extension inspect` | `failed` | `I-result` | `ExtensionInspectApplicationIntegrationTests.DuplicateFallbackFindingFailsClosedWithoutErasingPathFacts`. |
| `extension inspect` | `cancelled` | `I-result` | `ExtensionInspectApplicationIntegrationTests.CancellationProducesInterruptedEvent`. |
| `extension install` | `completed` | `I-result` | `ExtensionInstallBeforeOutputSnapshotTests` — `single-package`, `with-dependencies`, prompts, force, dry-run. |
| `extension install` | `completed-with-warnings` | `I-result` | Snapshot — `no-content-directory`; `ExtensionInstallContentBoundaryIntegrationTests`. |
| `extension install` | `incomplete` | `I-result` | Snapshot — `source-unreadable`; planning tests preserve unavailable coverage. |
| `extension install` | `invalid-input` | `I-result` | Snapshot — `no-selection-non-interactive`; interaction tests. |
| `extension install` | `blocked` | `I-result` | Snapshot — occupied/changed/lock/permission cases; target-policy and mutation tests. |
| `extension install` | `failed` | `I-result` | Snapshot — `write-failed-partial`; Windows file-sharing branch runs here. The separate Unix permission `FailedCopyRetainsApprovedPermissionAndExactRecovery(false/true)` cases are `W`. |
| `extension install` | `cancelled` | `I-result` | Snapshot — `cancelled`; interaction and permission cancellation tests. |
| `extension list` | `completed` | `I-run` | `ExtensionListBeforeOutputSnapshotTests` registered-source cases and published list smoke. |
| `extension list` | `completed-with-warnings` | `I-run` | Snapshot `SourceBoundary` — `installed-source-missing` (exit 2). |
| `extension list` | `incomplete` | `I-run` | Snapshot `SourceBoundary` — `source-unreadable`. |
| `extension list` | `invalid-input` | `I-run` | Snapshot — `source-invalid`, `invalid-input`. |
| `extension list` | `blocked` | `I-run` | Snapshot `InvalidBoundary` — `source-blocked`. |
| `extension list` | `failed` | `I-event` | `ExtensionListApplicationIntegrationTests.TerminalTypedEventsPreserveHumanPresentationPolicy` validates the supplied failed terminal event; no physical list failure cause is induced. |
| `extension list` | `cancelled` | `I-result` | `ExtensionListApplicationIntegrationTests.OperationCancellationIsAlwaysInterrupted`. |
| `extension remove` | `completed` | `I-run` | Snapshot — `single-package`, `shared-file-kept`, `missing-file-released`, `not-installed`, `dry-run`; interaction tests. |
| `extension remove` | `completed-with-warnings` | `I-result` | Snapshot — `orphaned-dependency`. |
| `extension remove` | `incomplete` | `I-run` | `ExtensionRemoveRegressionIntegrationTests` incomplete continuity case. |
| `extension remove` | `invalid-input` | `I-run` | Snapshot — `no-selection-non-interactive`; application interaction/safety tests. |
| `extension remove` | `blocked` | `I-run` | Snapshot — `dependent-blocks`, permission, lock; planning/regression tests. |
| `extension remove` | `failed` | `I-result` | Snapshot — `write-failed-partial`; current Windows runs the file-sharing branch. |
| `extension remove` | `cancelled` | `I-run` | Snapshot — `cancelled`; `ExtensionRemoveApplicationInteractionIntegrationTests`. |
| `extension update` | `completed` | `I-run` | Snapshot — `up-to-date`, replacements, new files, pruned, all packages, dry-run; interaction tests. |
| `extension update` | `completed-with-warnings` | `I-result` | Snapshot — `retired-kept`, `ownership-unknown`; safety tests. |
| `extension update` | `incomplete` | `I-result` | Snapshot — `source-unreadable`; safety/observation tests. |
| `extension update` | `invalid-input` | `I-run` | Snapshot — `no-selection-non-interactive`; interaction/safety tests. |
| `extension update` | `blocked` | `I-run` | Snapshot — permission/lock; mutation and interaction safety tests. |
| `extension update` | `failed` | `I-result` | Snapshot — `write-failed-partial`; current Windows runs the file-sharing branch. |
| `extension update` | `cancelled` | `I-run` | Snapshot — `cancelled`; application interaction tests. |
| `library attach` | `completed` | `I-result` | Snapshot — `attached-inside-agents`, `attached-outside-with-flag`, prompt, empty, dry-run. |
| `library attach` | `completed-with-warnings` | `G` | Unit completion/rendering fixtures accept retained recovery, but no Library Attach situation returning `Attention` was found. |
| `library attach` | `incomplete` | `I-result` | `LibraryAttachPreparationIntegrationTests.RecoveryBucketFileRetainsIncompleteWithoutTargetEffects`. |
| `library attach` | `invalid-input` | `I-run` | Snapshot `InvalidInput`; `LibraryAttachOperationIntegrationTests` input boundaries. |
| `library attach` | `blocked` | `I-result` | Snapshot — duplicate/destination/permission/lock; mapped-destination and permission lifecycle tests. |
| `library attach` | `failed` | `W` | `LibraryAttachPermissionLifecycleIntegrationTests.LinkFailureRetainsVerifiedGrantAndLeavesRecordUnpublished` is Linux-only and skipped on Windows. |
| `library attach` | `cancelled` | `I-result` | Snapshot `CancelledBetweenRealApplicationStages` and operation cancellation tests. |
| `library detach` | `completed` | `I-result` | Snapshot — `detached`, `detached-no-links`, `dry-run`, `no-ownership-record`. |
| `library detach` | `completed-with-warnings` | `G` | Unit completion/rendering fixtures accept retained recovery, but no Library Detach situation returning `Attention` was found. |
| `library detach` | `incomplete` | `I-result` | `LibraryDetachPreparationIntegrationTests.RecoveryBucketFileRetainsIncompleteWithoutTargetEffects`. |
| `library detach` | `invalid-input` | `I-result` | Snapshot — `unknown-id`; input operation tests. |
| `library detach` | `blocked` | `I-result` | Snapshot — link-gone, changed occupant, protected destination, permission, lock. |
| `library detach` | `failed` | `I-result` | Snapshot — `write-failed-partial`; current Windows runs the file-sharing branch. |
| `library detach` | `cancelled` | `I-result` | Snapshot — `cancelled`; operation tests. |
| `library inspect` | `completed` | `I-result` | Snapshot — `current`, `empty-source`, `no-ownership-record`; comparison/boundary tests. |
| `library inspect` | `completed-with-warnings` | `I-result` | Snapshot — `added-source-files`, `retired-source-files`, `missing-links`, `changed-links`. |
| `library inspect` | `incomplete` | `I-result` | Snapshot `SourceUnreadable` runs on Windows; Linux-only `LibraryInspectCoverageTests` unavailable-inventory/source/destination variants are `W`. |
| `library inspect` | `invalid-input` | `I-run` | Snapshot — `unknown-id`, `invalid-id`; `LibraryInspectBoundaryTests` source-file input. |
| `library inspect` | `blocked` | `I-result` | Snapshot `BlockedMapping`; boundary/coverage tests for blocked source and destination identities. |
| `library inspect` | `failed` | `G` | No executed Library Inspect operation failure situation was found; unit presentation contracts alone do not close it. |
| `library inspect` | `cancelled` | `I-result` | `LibraryInspectBoundaryTests` interrupted boundary. |
| `library list` | `completed` | `I-result` | Snapshot — `none-registered`, `one-current`, `no-ownership-record`; boundary test for inaccessible unregistered subtree is `W`. |
| `library list` | `completed-with-warnings` | `I-result` | Snapshot — `link-missing`, `link-changed`, `source-folder-missing`; Linux-only source-unavailable variant is `W`. |
| `library list` | `incomplete` | `I-result` | Snapshot — `record-invalid`, `record-unreadable`; Linux-only destination-unavailable and permission variants are `W`. |
| `library list` | `invalid-input` | `I-run` | Snapshot `InvalidInput`; boundary tests source-file input. |
| `library list` | `blocked` | `I-result` | Snapshot — `link-blocked`; boundary tests source/link blocked identities. |
| `library list` | `failed` | `G` | No executed Library List failure situation was found; unit presentation contracts alone do not close it. |
| `library list` | `cancelled` | `I-result` | `LibraryListBoundaryTests.CancellationAtIngress`. |
| `library sync` | `completed` | `I-result` | Snapshot — `up-to-date`, `links-added`, `links-removed`, `both`, `dry-run`, `no-ownership-record`. |
| `library sync` | `completed-with-warnings` | `I-result` | Snapshot — `registered-link-gone`. |
| `library sync` | `incomplete` | `I-result` | Snapshot `SourceUnreadable`; current Windows runs its Windows-ACL branch. |
| `library sync` | `invalid-input` | `I-result` | Snapshot — `unknown-id`; input operation tests. |
| `library sync` | `blocked` | `I-result` | Snapshot — changed occupant, permission, lock; mapped-destination and operation tests. |
| `library sync` | `failed` | `I-result` | Snapshot — `write-failed-partial`; current Windows runs the file-sharing branch. |
| `library sync` | `cancelled` | `I-result` | Snapshot — `cancelled`; operation/mapped-destination cancellation tests. |
| `references` | `completed` | `I-run` | `ReferencesBeforeOutputSnapshotTests` — `links-both`, `no-authored-links`, `out-only`, `external-outgoing`. |
| `references` | `completed-with-warnings` | `I-run` | Snapshot — `broken-outgoing`; `ReferencesOperationSmokeTests`. |
| `references` | `incomplete` | `I-run` | Snapshot — `unreadable-source`; operation smoke incomplete coverage. |
| `references` | `invalid-input` | `I-run` | Snapshot — `unknown-source`, `invalid-direction`, `include-with-out-only`. |
| `references` | `blocked` | `I-run` | Snapshot — `ambiguous-source`; operation safety tests. |
| `references` | `failed` | `G` | No executed References operation failure situation was found; renderer/definition contracts do not close it. |
| `references` | `cancelled` | `I-result` | `ReferencesOperationSmokeTests` interrupted operation cases. |
| `repair` | `completed` | `I-result` | Snapshot — `nothing-to-repair`, `automatic-two-links`, `dry-run-automatic`, `relink-one`; application completion tests. |
| `repair` | `completed-with-warnings` | `I-result` | Snapshot — `automatic-nothing-safe-two-guided`; application/safety tests. |
| `repair` | `incomplete` | `G` | No executed Repair situation returning incomplete was found. |
| `repair` | `invalid-input` | `I-run` | Snapshot invalid-input cases; `RepairInteractionIntegrationTests`. |
| `repair` | `blocked` | `I-result` | Snapshot — `relink-invalid`; safety/completion tests. |
| `repair` | `failed` | `I-result` | `RepairBeforeOutputSnapshotTests` failure/recovery situation; current Windows runs the file-sharing branch. |
| `repair` | `cancelled` | `I-result` | Snapshot cancellation and `RepairC1LifecycleIntegrationTests`/interaction tests. |
| `route create` | `completed` | `I-result` | Snapshot/app tests — created route, template, dry-run, and no-op cases. |
| `route create` | `completed-with-warnings` | `G` | No executed Route Create situation returning `Attention` was found. |
| `route create` | `incomplete` | `G` | No executed Route Create situation returning incomplete was found. |
| `route create` | `invalid-input` | `I-run` | Snapshot — `template-unknown` and invalid-input composition. |
| `route create` | `blocked` | `I-result` | Snapshot — `parent-missing`, `exists-with-different-content`, `lock-held`; planning tests. |
| `route create` | `failed` | `I-result` | Snapshot write/recovery failure situation; current Windows runs the file-sharing branch. |
| `route create` | `cancelled` | `I-result` | Snapshot — `cancelled`; operation tests. |
| `route init` | `completed` | `I-result` | Snapshot — `new-chain`, `already-initialized`, `dry-run`, `explicit-metadata`; application tests. |
| `route init` | `completed-with-warnings` | `G` | No executed Route Init situation returning `Attention` was found. |
| `route init` | `incomplete` | `I-result` | `GenericRouteInitOperationIntegrationTests` projection-incomplete case. |
| `route init` | `invalid-input` | `I-run` | Snapshot invalid-input and generic/framework safety tests. |
| `route init` | `blocked` | `I-result` | Snapshot installed/blocked boundary; generic/framework safety and ownership tests. |
| `route init` | `failed` | `I-result` | Snapshot recovery/write failure situation; current Windows runs the file-sharing branch. |
| `route init` | `cancelled` | `I-result` | Snapshot interrupted boundary and generic/framework safety tests. |
| `route inspect` | `completed` | `I-run` | Snapshot — entrypoint/routed-file/load-now/keep-in-mind/overwrite/compatibility/not-routed. |
| `route inspect` | `completed-with-warnings` | `I-run` | Snapshot — `id-not-unique-exact-path`; interaction collision tests. |
| `route inspect` | `incomplete` | `I-run` | Snapshot — `unreadable-source`; operation availability and application tests. |
| `route inspect` | `invalid-input` | `I-run` | Snapshot — `unknown-source`, `loader-subject`; translation/application tests. |
| `route inspect` | `blocked` | `I-run` | Snapshot — `orphan-overwrite`; resolver/interaction collision tests. |
| `route inspect` | `failed` | `G` | No executed Route Inspect failure situation was found. |
| `route inspect` | `cancelled` | `I-result` | `RouteInspectOperationCancellationIntegrationTests` and interaction cancellation tests. |
| `route list` | `completed` | `I-run` | Snapshot — roots, subtree, depth, and empty-subtree cases; application tests. |
| `route list` | `completed-with-warnings` | `I-run` | Snapshot — `metadata-missing`; topology tests. |
| `route list` | `incomplete` | `I-run` | Snapshot — `unreadable-entrypoint`; topology/filesystem tests. |
| `route list` | `invalid-input` | `I-run` | Snapshot — `unknown-source`, `invalid-depth`; application tests. |
| `route list` | `blocked` | `I-run` | Snapshot — `ambiguous-source`, `loader-malformed`; topology/loader tests. |
| `route list` | `failed` | `G` | No executed Route List failure situation was found. |
| `route list` | `cancelled` | `I-run` | `RouteListApplicationIntegrationTests.MidReadCancellationPresentsInterruptedResult` asserts exit 130 and JSON status. |
| `route move` | `completed` | `I-result` | Snapshot/app tests — leaf/category move, rewritten links, and dry-run. |
| `route move` | `completed-with-warnings` | `G` | No executed Route Move situation returning `Attention` was found. |
| `route move` | `incomplete` | `I-result` | Snapshot — `reference-scan-incomplete`; planning/application tests. |
| `route move` | `invalid-input` | `I-run` | Snapshot — destination-inside-source, self-move, source-not-found; planning tests. |
| `route move` | `blocked` | `I-run` | Snapshot — destination/managed/lock; interaction and revalidation tests. |
| `route move` | `failed` | `I-result` | Snapshot/application/recovery identity failure cases. |
| `route move` | `cancelled` | `I-result` | Snapshot — `cancelled`; interaction/application cancellation tests. |
| `route remove` | `completed` | `I-result` | Snapshot — leaf/category removal, detached links, dry-run; composition tests. |
| `route remove` | `completed-with-warnings` | `G` | No executed Route Remove situation returning `Attention` was found. |
| `route remove` | `incomplete` | `I-result` | Snapshot — `reference-scan-incomplete`; reference/status-doctor tests. |
| `route remove` | `invalid-input` | `I-run` | Snapshot — `source-not-found`; interaction/reference tests. |
| `route remove` | `blocked` | `I-run` | Snapshot — managed source, unsafe detach, lock; planning/revalidation/recovery tests. |
| `route remove` | `failed` | `I-result` | Snapshot/application/recovery identity verification failures. |
| `route remove` | `cancelled` | `I-run` | Snapshot — `cancelled`; interaction/reference tests. |
| `route update` | `completed` | `I-result` | Snapshot — description/tags/responsibility/template/no-change/dry-run cases. |
| `route update` | `completed-with-warnings` | `I-result` | Snapshot — `template-body-protected`; application/template tests. |
| `route update` | `incomplete` | `I-result` | `RouteUpdateTargetObservationIntegrationTests` target-observation incomplete case. |
| `route update` | `invalid-input` | `I-run` | Snapshot — `unknown-source`; target-observation and interaction input tests. |
| `route update` | `blocked` | `I-run` | Snapshot — `lock-held`; application/interaction safety tests. |
| `route update` | `failed` | `I-result` | Snapshot/application/recovery failure cases. |
| `route update` | `cancelled` | `I-run` | Snapshot — `cancelled`; interaction/application cancellation tests. |

## Deliberate published-process assertions

The E2E suite is 163 tests in 55 classes. The following process assertions are
deliberate because `RunAsync` cannot prove the property without ceasing to be
the boundary under test:

| Expensive assertion | Existing test evidence | Why the process boundary is deliberate |
| --- | --- | --- |
| AOT/native published behavior | `OpenForge.Cli.EndToEndTests.csproj` and the CLI project set `PublishAot=true`; the `Published*ProcessTests` run the selected publication. | Trimming, native startup, generated serialization, and embedded deployment behavior do not exist inside an in-process `CliCoreApplication` invocation. |
| Published executable discovery and identity | `PublishedExecutableTarget.Discover`; `CliProcessTests.PublishedVersionIsExactAndWorkspaceIndependent`. | The test must locate the artifact from its published layout, validate the runtime/version marker, and prove workspace independence. An in-process call would bypass discovery. |
| Real process stdout/stderr and stream ownership | `ProcessRunner`; `PublishedProcessTestSupport`; `PublishedShellBoundaryProcessTests`; `CliProcessTests.PublishedParserFailureUsesFixedInvalidExitAndStandardError`. | Redirected OS streams, primary stream selection, encoding, process completion, and parser diagnostics are process contracts. The matrix uses `I-run` for their command semantics, but retains these tests as published stream smoke. |
| Child cancellation/kill/drain lifecycle | `CliProcessTests.PublishedProcessCancellationKillsAndDrainsOwnedChild`. | The test owns a real child, cancels it after start, requests termination, and drains its streams. This is process ownership evidence, not merely command status 130. No `PosixSignal`/`SIGINT`/`CTRL+C` delivery test was found; OS signal delivery remains unproven rather than being implied by this row. |
| Relocated published artifact and embedded resources | `PublishedEmbeddedPayloadProcessTests.RelocatedArtifactReachesEmbeddedPayload`; `.RelocatedArtifactReachesEmbeddedExtensions`. | Copying the published artifact and proving embedded Install/Extension discovery is a deployment-relative property. It is intentionally process-only. Slice 71 separately closes the relocated-workspace in-process gap. |

The ordinary per-command `Published<Command>ProcessTests` that repeat a
completed, dry-run, invalid, or safety result are published smoke. The
corresponding `I-run`/`I-result` row above is the cheaper behavior proof; the
smoke test is not evidence that the status requires a child process.

## Windows-skipped permission evidence

The integration run was on Windows. It reported 17 skipped test cases, all
permission or special-object paths that target Linux. These are retained test
rows, but they are `W` and unproven on this host:

| Test and parameter(s) | Count | Missing current evidence |
| --- | ---: | --- |
| `ExtensionContentLayoutIntegrationTests.UnavailableCatalogueClassificationCannotFallThroughToPackageSuccess` | 1 | Unix source-enumeration unavailability; affects extension source classification. |
| `ExtensionInstallPermissionBoundaryIntegrationTests.FailedCopyRetainsApprovedPermissionAndExactRecovery(false/true)` | 2 | Unix directory-permission copy failure and recovery consequence for Extension Install. |
| `WorkspaceSettingsReaderIntegrationTests.InaccessiblePermissionDoesNotBecomeEmptyApproval` | 1 | Unix permission read failure must not become an empty approval. |
| `LibrarySourceRootReaderIntegrationTests.InaccessibleLibrarySourceBoundaryNeverProducesAvailableEmptyFacts` | 1 | Inaccessible library source root must not become an available empty source. |
| `LibraryInventoryReaderIntegrationTests.InaccessibleEligibleFileDoesNotBecomeCompletePrefix` | 1 | Inaccessible eligible file must produce incomplete inventory, not a safe prefix. |
| `LibraryInventoryReaderIntegrationTests.ExcludesSpecialSourceObject` | 1 | Unix-domain socket/special-object exclusion. |
| `LibrariesRecordReaderIntegrationTests.DistinguishesUnavailableFromMissing` | 1 | Unreadable existing Library record must remain unavailable, not missing. |
| `LibraryInspectCoverageTests.IncompleteInventoryPreventsRetirementClaims` | 1 | Inaccessible inventory suffix must not produce retirement/attention claims. |
| `LibraryInspectCoverageTests.UnavailableFactsDominateDrift(source/destination)` | 2 | Unavailable source/destination facts must dominate otherwise safe drift. |
| `LibraryListBoundaryTests.SourceInventoryIsNeverRequested` | 1 | Inaccessible unregistered source subtree must not be traversed. |
| `LibraryRecoveryEntrySetObserverIntegrationTests.ObservesEveryVerifiedEntry("unavailable")` | 1 | Unavailable recovery entry must retain its explicit comparison state. |
| `LibraryRecoveryEntryObservationIntegrationTests.RetainsIndependentReadFailure` | 1 | Ordinary recovery read failure must remain explicit unavailable evidence. |
| `LibraryListAvailabilityTests.RequiredObservationUnavailable(source/destination)` | 2 | Source-unavailable warning and destination-unavailable incomplete coverage. |
| `LibraryAttachPermissionLifecycleIntegrationTests.LinkFailureRetainsVerifiedGrantAndLeavesRecordUnpublished` | 1 | Link failure after approval must preserve the verified grant and recovery. |
| **Total** | **17** | **These permission-failure rows are not proven by the current Windows run.** |

The Windows-only file-sharing tests are different: they run on this host and
are counted as current evidence in the matrix. A test's existence does not
turn a skipped platform path into a passing result.

## Gaps and decisions left for maintainers

Slice 71 closes the seeded dirty-workspace and relocated-workspace shape gaps
with `I-run` evidence. The relocated published-artifact row remains deliberately
process-only (`P`) because it proves deployment-relative embedded-resource
discovery, not workspace relocation.

The remaining `G` rows above are the explicit current gaps. The most consequential are:

| Gap | What is missing | Boundary decision left open |
| --- | --- | --- |
| Status situations without executable evidence | Cleanup warning; Doctor failure/cancellation; Index and Install warnings; several Library/Route warning or failure rows; Repair incomplete; Status failure/cancellation; Update incomplete; and the other `G` rows are named in the matrix. | Adding a situation, finding code, or splitting a code is a behavior/JSON/exit-contract decision, not an inventory edit. |
| Current Windows permission paths | 17 Linux permission/special-object cases are skipped. | A Linux-qualified run or an explicitly approved cross-platform fixture is required before calling those rows proven. |
| OS signal delivery | Child cancellation is tested, but no SIGINT/SIGTERM/CTRL+C injection was found. | Decide separately whether signal delivery belongs in process smoke. |

The one-code/one-situation rule means this inventory does not invent a shared
finding, wording, or placeholder to fill any gap. It also does not move or
delete an existing test.

## Measurement and gates

Before editing, the slice's measured current state still held: no matrix was
present, unit was 3,205 tests, integration was 2,223 tests with 108 files using
the in-process application boundary, and E2E was 163 tests across 55 classes.
The phase's measured scenario mentions also held: malformed 66, external 64,
nested 20, dirty 0, relocated 0.

The current verification run for slice 70 was:

| Gate | Result |
| --- | --- |
| Release build | Passed with 0 warnings and 0 errors after the offline-audit retry and serialized no-restore build. |
| Unit executable | 3,205 total / 3,205 succeeded / 0 failed / 0 skipped. |
| Integration executable | 2,223 total / 2,206 succeeded / 0 failed / 17 skipped. |
| End-to-end executable | 163 total / 163 succeeded / 0 failed / 0 skipped. |
| Whitespace verification | Exactly 5 pre-existing errors; no new whitespace error was introduced. |

Slice 71 follow-up verification was:

| Gate | Result |
| --- | --- |
| Release build | Passed with 0 warnings and 0 errors after removing only the isolated `task71` scratch outputs and stopping stale workers from the earlier verification run. |
| Unit executable | 3,206 total / 3,206 succeeded / 0 failed / 0 skipped. |
| Integration executable | 2,226 total / 2,209 succeeded / 0 failed / 17 skipped. |
| Whitespace verification | Exactly 5 pre-existing errors; none points at the new journey test. |

The two new journey methods move the filename-mention starting point from
dirty 0 to 1 and relocated 0 to 1. These remain filename counts, not a claim
that one file is the complete shape inventory.

Compared with the older gate baseline in the task prompt (unit 3,191 and
integration 2,222), the current tree reports +14 unit tests and +1 integration
test. Compared with this slice's measured current state, the counts did not
move. No product code or test was changed.

No snapshot or capture was regenerated. `OPENFORGE_SNAPSHOT_UPDATE` was not
exported, and no test was moved or deleted during slice 70. Slice 71 adds the
two in-process journey tests recorded above without changing the command
outputs or the process-only relocated-artifact evidence.

## Changes ledger

- `.agents/memory/working/cli-development/tasks/task30/70-coverage-matrix.md`: empty outcome/ledger -> evidence-cost journey and 28-command by 7-status matrix, process-only rationale, Windows skip inventory, and explicit gaps.
- `src/cli/**`: unchanged -> unchanged; no product code was touched.
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Shared/Scenarios/WorkspaceShapeJourneyIntegrationTests.cs`: absent -> two isolated in-process workspace-shape journeys for dirty Install and relocated Status.
- Journey matrix: dirty and relocated workspace rows read `G` -> `I-run`; relocated published-artifact row remains `P`.
- Snapshot/capture files: unchanged -> unchanged; no capture was regenerated.

## Divergences observed

- The task prompt's older gate baseline (3,191 unit and 2,222 integration) differs from the clean current tree by +14 and +1. The slice's own recorded 3,205/2,223 current state was confirmed before and after the inventory.
- The first requested build invocation hit the sandbox's offline NuGet audit. A retry with `NuGetAudit=false`, `--ignore-failed-sources`, `UseSharedCompilation=false`, serialized build, and no restore completed cleanly; this changed no repository file.
- The process suite contains relocated published-artifact tests, while slice 71 adds a separate relocated-workspace `I-run` journey. Those are different facts and remain separate rows.
- The process cancellation test kills and drains a real child; it is not OS signal delivery. No signal injection test was found, so that uncertainty is recorded rather than treated as covered.
- The 17 current integration skips are Linux permission/special-object cases. Windows-only file-sharing cases ran on this host and were not counted as skips.
