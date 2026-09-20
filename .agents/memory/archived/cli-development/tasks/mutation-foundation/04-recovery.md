---
open-forge:
  description: Implement external recovery bundles and interruption boundaries for mutations
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Mutation, Recovery]
---

# Implement Recovery Bundle Boundaries

## Task State

- State: Complete at exact production candidate `e7d937f`, under current parent
  authority `01dd552`. This closeout supersedes the stale command-level scope in
  the historical task prose without rewriting its history.
- Responsible role: Overseer-managed Task Mastermind, sequential.
- Parent: [Mutation Foundation](_mutation-foundation.md).
- Last updated: 2026-08-29.

## Expected Outcome

The recovery store prepares and verifies one immutable external recovery bundle
for a caller-described operation containing an existing-target effect
(`Replace`, `ReplaceGeneratedRegion`, or `Delete`). The bundle contains exact
prior bytes and static prior/intended fingerprints. M1 supplies store,
preparation, catalogue, and eligible-candidate deletion-guard mechanics; it does
not coordinate a complete operation, choose deletion candidates, or decide
preparation, verification, effect, or removal timing. An eligible deletion
candidate is a caller-selected `Verified` final or ordinary exact-name
`Incomplete` draft. Recovery never classifies current target state. The
foundation never automatically restores a target, rolls back an effect,
compensates for a target effect, or persists a journal, receipt, progress
history, or replayable plan.

An operation containing only creates and semantic or byte no-ops creates no
bundle. Standalone Extension Create remains a separate create-only
destination/collision/revalidation path: it has no workspace lease, none of
`Replace`, `ReplaceGeneratedRegion`, or `Delete`, and no recovery bundle.

## Accepted Components And Boundaries

### External bundle store

- Resolve the current user's
  `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and use only the application-owned
  `OpenForge/recovery/v1` subtree. There is no temporary-directory,
  repository, `HOME`, or custom-platform fallback. Unavailable storage is a
  pre-effect `incomplete` result.
- Store one ZIP bundle outside the workspace for one complete
  command operation. Its deterministic external directory key and recognized
  final name are keyed by the normalized physical workspace path and operation
  ID. A normalized path is the identity; no repository pointer, stable ID, or
  native filesystem identity is introduced.
- Draft with `CreateNew` under the exact deterministic draft name in the same
  external directory. Close and reopen it; semantically decode the source-
  generated schema-v1 manifest and validate exact ordered entry names and counts,
  declared lengths and hashes, and exact payload bytes by bounded streaming.
  Move it within the same directory to the deterministic final name, reopen it,
  and verify it again. Only the real store's successful final close/reopen
  semantic readback constructs the opaque `RecoveryBundlePreparation`; it has no
  freely initializable or caller-forgeable verification surface. A draft is
  `Draft`/`Incomplete` support data and never preparation. The prepared final
  bundle is immutable.
- The writer may select `CompressionLevel.NoCompression`, but compression method
  is not inspected, recognized, or promised. The format defines no entry
  timestamp, deterministic archive bytes, or whole-ZIP length or hash.
- Use source-generated serialization for `manifest.json`, managed
  BCL archive and file APIs, and streamed ordinal payload entries. Do not add a
  custom archive parser, reflection, native dependency, or package. Never
  extract a bundle.
- `RecoveryBundleCatalogue` supplies neutral observer facts. An absent root or
  workspace store is an available empty catalogue; unavailable storage remains
  distinct. Every exact-name candidate reports its path, kind, and integrity.
  Finals receive one semantic read and drafts remain path-only `Incomplete`
  facts. The catalogue does not map those facts to Status or Doctor results,
  classify live targets, infer activity, or provide command policy. Payload
  validation uses fixed bounded buffers and never extracts, discloses, renders,
  logs, returns, retains, or materializes payload bytes.

### Manifest and preparation

The manifest records schema version, command and operation identity, workspace
identity, and an ordered exact target set. Each existing-target effect
(`Replace`, `ReplaceGeneratedRegion`, or `Delete`) records the normalized
relative target, change kind, prior byte length, prior lowercase SHA-256, and its
ordinal payload name. It records the intended final absence or intended length
and lowercase SHA-256. These are static recovery provenance facts, not an
evolving journal. Every such effect has exactly one matching entry; `Create` and
no-op targets have none.

`FileChangeApplier` requires a matching opaque final
`RecoveryBundlePreparation` for each `Replace`, `Delete`, or
`ReplaceGeneratedRegion`. `Create` must receive `null`, and a non-null
preparation for `Create` is rejected. The applier performs one final effect per
target. Future command orchestration owns satisfying complete-operation
preparation and timing requirements; M1 does not coordinate the plan or its
before-first-effect sequence.

### Lock and command coordination

The workspace lock is a persistent reusable zero-byte ordinary file in the
separate `LocalApplicationData/OpenForge/locks/v1` subtree. Hold one read/write
`FileShare.None` handle only. Do not write metadata, timestamps, ownership facts,
or recovery claims; do not truncate or delete the lock file. Recovery uses the
same normalized physical-workspace identity algorithm without depending on the
locking layer.

Commands own selection, policy, operation-level preparation and removal timing,
effect ordering, lifecycle publication, and result semantics. Shared recovery
support only prepares, validates, retains, recognizes, reports, and mechanically
guards deletion of a caller-selected eligible candidate: a `Verified` final or
ordinary exact-name `Incomplete` draft. It does not infer command policy,
operation success, or candidate selection.

The accepted correction also removes the complete replacement-CLI Git
capability. `Framework/Git/**`, including its models, repository inspector, and
process runner, plus every matching test and static reference are absent. No
replacement-CLI Git inspection, recovery-check, bypass flag, option, binding, or
dormant Git code remains. Historical and frozen legacy authorities remain
unchanged.

## Failure, Interruption, And Cleanup Rules

- A storage or bundle-verification failure returns no usable preparation.
  Unavailable storage is `incomplete`; malformed, mismatched, or colliding
  recognized-state facts are `blocked`. Future command policy owns whether the
  operation may proceed.
- Pre-cancellation creates no artifact and returns a null residual path. A
  deterministic final-name collision retains and reports the existing final.
  Once a later write or move begins, the implementation tracks its currently
  owned draft or final path truthfully; code audit proves those paths without
  manufacturing an operating-system failure seam. A closed final ZIP may remain
  after abrupt process termination, but there is no executable crash or
  power-loss durability guarantee.
- `RecoveryBundleDeletionGuard` provides only the held-lease mechanical deletion
  boundary. Given the existing same-workspace `WorkspaceLockLease`, it
  re-enumerates the workspace bucket once through the neutral catalogue and
  re-establishes the caller-selected candidate's exact path, name, and kind.
  Immediately before ordinary `File.Delete`, a `Verified` final receives a
  semantic reread; an ordinary exact-name `Incomplete` draft repeats exact
  path/name/kind revalidation. The guard then verifies positive absence. It adds
  no marker, PID, journal, or lock metadata. It does not select candidates,
  choose an empty-no-lease default, acquire or arbitrate the lease, orchestrate
  multiple deletions, define Cleanup results, observe operation success, or
  decide deletion timing.
- For one caller-selected eligible candidate, the guard records one finite state
  (`Deleted`, `Failed`, `Blocked`, or `Cancelled`) and one orthogonal disposition
  (`Removed`, `Retained`, or `Unknown`). `Deleted` requires `Removed` and no
  residual. `Retained` requires an exact residual path and only follows a
  positive current observation. Delete failure followed by positive presence is
  `Failed`/`Retained`; an observation failure or exception is
  `Failed`/`Unknown` with the exact expected path allowed. `Blocked` and
  `Cancelled` are `Retained` only after an exact current positive observation;
  otherwise they are `Unknown`. Whole-operation verification, timing and
  removal policy, cleanup guidance, result aggregation, and deletion-failure
  orchestration belong to O2.
- Unknown, lookalike, malformed, mismatched, differently keyed, or otherwise
  unowned items remain untouched. The catalogue and deletion guard never
  extract, bind, restore, roll back, or compensate for a target effect.
- A workspace move is outside the automatic guarantee. The catalogue preserves
  facts under the original normalized root but never auto-binds or restores
  them. Future Status, Doctor, or Cleanup policy may consume those facts under
  its own contract. A later mutation invocation forms a fresh plan from current
  facts and never replays a saved plan, receipt, journal, history, or progress
  record.

## Security And Platform Rules

The recovery root is ordinary current-user `LocalApplicationData` under the
stable workspace and cooperating-client threat model. No special platform-
permission or encryption behavior is promised. No reflection, raw ZIP parser,
native dependency, extra package, or platform-specific archive path is accepted.

## Evidence Matrix

| ID      | Behavior or failure class           | Tier                              | Required proof                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ------- | ----------------------------------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MFR-001 | Small contract facts                | Unit                              | Final-only opaque preparation, draft non-preparation, exact target coverage, Create/no-op absence, and source-generated manifest graph                                                                                                                                                                                                                                                                                                                   |
| MFR-002 | Real multi-target path              | Integration                       | One real-filesystem operation writes exact prior bytes for multiple targets, closes/reopens and semantically verifies the draft, moves and verifies the final, applies matching effects, and preserves bounded payload handling                                                                                                                                                                                                                          |
| MFR-003 | Create and no-op                    | Integration                       | Create-only store input returns `NotNeeded`, and the observer proves no storage is created; future command-planning no-op policy is not claimed here                                                                                                                                                                                                                                                                                                     |
| MFR-004 | Rejected preparation                | Unit and Integration              | Missing, foreign, mismatched, or corrupt preparation blocks `Replace`/`ReplaceGeneratedRegion`/`Delete` before effect; a draft is non-preparation and cannot authorize an effect; Create requires null and rejects non-null preparation                                                                                                                                                                                                                  |
| MFR-005 | Representative residuals            | Integration and code audit        | Pre-cancellation creates no artifact and returns a null residual; a deterministic final-name collision retains and reports the existing final; code audit proves later write/move failures track the owned draft or final path without a manufactured failure seam                                                                                                                                                                                       |
| MFR-006 | Eligible-candidate guarded deletion | Unit, Integration, and code audit | Given an existing held same-workspace lease and one caller-selected eligible candidate—a `Verified` final or ordinary exact-name `Incomplete` draft—guarded ordinary deletion and positive absence prove `Deleted`/`Removed`; contract evidence covers every valid and impossible state/disposition shape; audit proves only a positive current observation can produce `Retained`, without claiming whole-operation ordering or manufacturing a failure |
| MFR-007 | Catalogue, deletion guard, and lock | Integration                       | Persistent external lock remains zero bytes through contention; with the same-workspace lease already held, the guard re-enumerates once and re-establishes exact path/name/kind, semantically rereads a final or repeats exact path/name/kind revalidation for a draft immediately before ordinary deletion, verifies absence, and leaves unknown names untouched                                                                                       |
| MFR-008 | Native AOT smoke                    | Published Integration             | Focused ZIP/source-generation create, verify, retain, catalogue, and delete evidence executes with reflection disabled on `linux-x64`                                                                                                                                                                                                                                                                                                                    |
| MFR-009 | Zero Git capability                 | Static and managed                | No `Framework/Git/**`, Git model/inspector/process capability, related tests, static references, or replacement-CLI Git inspection/recovery-check/bypass flag or option remains                                                                                                                                                                                                                                                                          |

## Protected Decisions And Stop Conditions

Do not add repository discovery, process execution, cleanliness decisions, status
facts, or mutation to the replacement CLI. Do not add a repository or local
backup fallback, automatic restore, rollback engine, compensation path, journal,
persisted plan, receipt history, crash guarantee, or power-loss guarantee. Do not extract bundles,
recognize final validity by filename alone, follow a workspace move automatically, add a custom
archive parser, use reflection/native/package workarounds, or weaken the accepted
BCL boundary. Record Route Init's generic-versus-Framework shape as a maintainer
decision required before M2; do not decide it in this child.

Do not add command-specific Status, Doctor, or Cleanup selection, defaulting,
empty-no-lease, orchestration, guidance, result aggregation, or EndToEnd behavior
to this foundation. Those public command behaviors remain O1/O2 work. Preserve
the structural statement that a closed final may remain after abrupt process
termination, but do not require an executable process-crash or power-loss
harness, permission manipulation, or a fake operating-system failure seam.

Stop and report if any production consumer cannot supply the matching verified
preparation, if an operation would need a second bundle or per-target recovery
artifact, if exact old bytes cannot be validated before the first effect, or if
the result would imply restoration that did not occur.

## Completion And Evidence

Child 05 inspected the current contracts, direct consumers, and exact production
candidate. Child 02's persistent byte-preserving lock and Child 03's matching
preparation integration are complete at `e7d937f`. Final evidence is a
warning-free Release build; focused mutation-foundation Unit `43/43` and
Integration `47/47`; full managed Unit `1096/1096`, Integration `458/458`, and
EndToEnd `120/120`; focused published `linux-x64` Native AOT recovery/source-
generation `10/10`; and the cheap full published Integration runner `458/458`,
all with zero failures or skips. Static replacement-CLI Git audit is zero.

Eligible-candidate guarded deletion and positive absence are executable evidence
for a caller-selected `Verified` final and ordinary exact-name `Incomplete`
draft, with their respective immediate semantic or path/name/kind revalidation.
The state/disposition invariant and positive-observation rule are structurally
audited, but no permission manipulation or fake failure seam was introduced to
manufacture an operating-system deletion failure. Pre-cancellation creates no artifact and returns a
null residual; deterministic collision retains and reports the existing final;
later write/move residual tracking is code-audited. Final independent review
returned `ROBUST`, safe to commit, confidence `0.98`. Public
Status/Doctor implementation remains O1; whole-operation deletion timing,
policy, and orchestration remain O2.
