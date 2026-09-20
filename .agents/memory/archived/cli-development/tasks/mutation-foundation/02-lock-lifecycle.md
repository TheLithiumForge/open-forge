---
open-forge:
  description: Implement persistent lock ownership, lifecycle reads and writes, and expected-state revalidation
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Mutation, Lock, Lifecycle]
---

# Implement Locking, Lifecycle, And Revalidation

## Task State

- State: Complete. Historical implementation remains recorded at `5b926e7` from
  exact accepted contract record `3967534`; `e7d937f` later removed metadata
  writes. The accepted C2-era correction relocates the lock externally, requires
  persistent zero-byte identity, and removes workspace bootstrap state.
- Responsible role: Overseer-managed Task Mastermind, sequential.
- Parent: [Mutation Foundation](_mutation-foundation.md).
- Last updated: 2026-08-30.

## Expected Outcome

Workspace mutations acquire positive OS lock ownership, read and validate exact
lifecycle state, and revalidate every planned precondition after locking and
before the first effect.

## Components

- `WorkspaceLockManager` acquires one persistent external zero-byte lock below
  `LocalApplicationData/OpenForge/locks/v1` with a real exclusive OS handle,
  cancellation, and typed failure states. Disposal releases the handle; stale
  file existence is not ownership. The manager writes no metadata and never
  truncates or deletes the persistent file.
- `LifecycleStore` strictly reads and source-generates schema version 1, rejects
  malformed/unknown/legacy content, and writes only through atomic application.
- `FileExpectationValidator` checks existence, kind, resolved physical path, and hash
  from accepted BCL facts.
- `MutationPreflight` validates a command-local plan before lock without effects.
- `MutationRevalidator` repeats only volatile expected-state checks under the lock
  and returns typed mismatches.

## Accepted Design Decisions

### Lock ownership

- `WorkspaceLockManager` uses one open writable `FileStream` with `FileShare.None`
  as the sole cross-platform OS-backed exclusion boundary. It does not add a
  byte-range lock, platform branch, `WriteThrough` option, or forced-durability
  flush. `WorkspaceLockLease` owns that exact handle; file existence is not
  acquisition.
- Acquisition uses the normalized physical workspace path and authoritative full
  lowercase SHA-256 key. The filename is
  `<friendly-workspace-name>-<full-sha256-workspace-key>.lock`; the bounded
  filename-safe friendly prefix is display only and has deterministic
  `workspace` fallback. No planned workspace, lifecycle, or recovery-bundle
  effect occurs before the acquired result.
- After ownership, the manager does not write lock metadata, timestamps, command,
  operation, process, workspace, or recovery facts. The file remains exactly
  zero bytes. The held read/write `FileShare.None` handle alone establishes
  ownership.
- An unlocked existing file is opened and reused. The managed BCL does not expose
  a portable sharing-violation type, so an ambiguous pre-effect open `IOException`
  is reported as a typed input/output failure rather than inferred contention;
  the manager does not delete, truncate, replace, or inspect another actor's
  metadata. Cancellation before ownership yields `Cancelled`. Path, access,
  unsupported, and I/O failures remain typed. Disposal alone releases ownership;
  the external unlocked file intentionally remains because unlink/recreate could
  split coordination across open handles.

### Lifecycle store

- `LifecycleStore` owns mutation-facing reads and source-generated write-plan
  formation for the sole schema-v1 lifecycle path. It performs no filesystem
  write in this child. A write is represented by one `PlannedFileChange` that the
  atomic application child must later execute and verify.
- Reads return the exact `FileStateSnapshot`, common envelope, isolated selected
  section, and finite missing/available/invalid/blocked/unavailable/cancelled
  state. Syntax, duplicate selected fields, schema version, fingerprint policy,
  canonical workspace binding, selected-section shape, deterministic ordering,
  fingerprints, ownership reciprocity, and Framework generated-region identity
  fail closed. Malformed unrelated-section meaning does not erase a valid
  selected-section read.
- Framework and Extension update-plan methods accept the cohesive selected state,
  not its individual members. A plan requires the existing common envelope and
  every present section to be valid, rejects cross-section path ownership
  collisions, rebuilds the selected state and semantically retains any unrelated
  state. A newly created document emits the ordered root keys
  `schemaVersion`, `fingerprintPolicy`, `workspacePath`, `framework`, and
  `extensions`; Framework creation uses a complete empty `ExtensionLifecycleState`
  (`coverage: "complete"`, `packages: []`, `paths: []`) for that section. An
  existing section-missing, explicit-null, malformed, or incomplete selected or
  unrelated section is untrusted: both update-plan methods return `Blocked` with
  no change and never repair it. When selected meaning changes, serialization
  emits one deterministic canonical UTF-8 whole-document representation;
  formatting, ordering, and line-ending trivia are not preserved. A semantic
  no-op emits no write plan.
  Serialization uses only `LifecycleJsonContext` and no reflection or legacy
  fallback.
- The store never applies its plan and never publishes lifecycle bytes for a
  dry-run, invalid, blocked, cancelled, failed, unverified, or partially applied
  command. Command-local orchestration retains that policy.

### Preflight and revalidation

- `FileExpectationValidator` observes the real filesystem through accepted
  physical-path facts, reads owned exact bytes for ordinary files, and compares
  expected kind, resolved physical path, and lowercase SHA-256 content identity. It
  resolves the candidate physical path around the read so a changed or escaping path fails
  closed rather than becoming a match.
- `MutationPreflight` accepts only the ordered shared `PlannedFileChange`
  mechanics from a command-local plan. An empty set is a valid idempotent no-op;
  non-empty sets reject duplicate logical targets, incompatible present or
  prospective physical aliases, unsafe paths, stale expectations, and
  cancellation without performing effects.
- `MutationRevalidator` accepts a live `WorkspaceLockLease` and ordered changes.
  It proves that the lease belongs to the same workspace, repeats the volatile
  expectation checks, and returns the same finite ordered evidence. A disposed,
  foreign-workspace, empty, stale, unsafe, failed, or cancelled request cannot be
  valid. No general executor or command policy is introduced.

## Placement And Callable Map

| Capability   | Exact production scope                                           | Boundary                                                  |
| ------------ | ---------------------------------------------------------------- | --------------------------------------------------------- |
| Locking      | `Framework/Mutation/Locking/` and its `Models/`                  | Acquire, classify, and own the real lock handle           |
| Observation  | `Framework/Mutation/Validation/` and its `Models/`               | Exact real-filesystem expectation evidence                |
| Preflight    | `Framework/Mutation/Validation/MutationPreflight.cs`             | Effect-free common change-set checks                      |
| Revalidation | `Framework/Mutation/Validation/MutationRevalidator.cs`           | Same checks gated by a live same-workspace lease          |
| Lifecycle    | `Framework/Lifecycle/` and existing `Models/` / `Serialization/` | Selected-section reads and deterministic write-plan bytes |

Production has no mockable filesystem interface, reflection serializer, generic
mutation engine, global registry, or package addition. Property-only result
shapes use `required init`; invariant-bearing models use named factories. Tests
use real isolated files, handles, links, and cancellation.

## Required Behavior

No workspace effect occurs before lock ownership and successful revalidation.
Cancellation before effects leaves workspace bytes unchanged. Lock contention
never deletes or replaces another actor's lock file, and acquisition requires
the persistent file to remain zero bytes.
Workspace-free `extension create` cannot call the workspace lock manager.
For existing-target mutations, the subsequent recovery child supplies one
verified external bundle before the first Replace/Delete effect; this child does
not silently substitute an older recovery design.

Lifecycle writes preserve unrelated accepted section meaning, reject unknown
schema versions, use exact source/package identities, and never infer from legacy
files. They canonicalize the complete document only for a selected semantic
change and do not write for a semantic no-op.

## Evidence

Integration uses separate processes or handles for contention, cancellation,
stale path, marker mismatch, lifecycle absent/valid/malformed/unknown-version,
precondition change between planning and lock, and unchanged snapshots. Published
AOT Integration runs the same lock and lifecycle path.

## Evidence Matrix

| ID      | Behavior or failure class              | Tier                  | Required proof                                                                                                                                                                                                                                   |
| ------- | -------------------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| MFL-001 | Lock acquisition and disposal          | Integration           | One exclusive `FileShare.None` handle is held until disposal; unlocked file is reusable                                                                                                                                                          |
| MFL-002 | Contention and stale file              | Integration           | Second handle returns a truthful typed pre-effect I/O result without truncation/deletion; stale unlocked file acquires                                                                                                                           |
| MFL-003 | Lock path safety and cancellation      | Integration           | Isolated external store, ordinary zero-byte target, directory/link/nonzero rejection, access/I/O classification, and pre-acquire cancellation                                                                                                    |
| MFL-004 | Persistent lock identity and ownership | Unit and Integration  | Friendly bounded filename plus full normalized-workspace SHA-256 authority; only a held `FileShare.None` handle establishes ownership; persistent zero-byte reuse has no metadata write, truncation, or deletion                                 |
| MFL-005 | File expectation observation           | Integration           | Missing/file/directory, exact bytes/hash, resolved physical path, unsafe link, type mismatch, replacement race, and cancellation                                                                                                                 |
| MFL-006 | Preflight                              | Unit and Integration  | Stable ordered valid set and empty no-op; duplicate logical target, present or prospective physical alias, stale, unsafe, failed, and cancelled sets                                                                                             |
| MFL-007 | Under-lock revalidation                | Integration           | Live same-workspace lease required and an after-plan change blocks before any effect                                                                                                                                                             |
| MFL-008 | Lifecycle selected reads               | Integration           | Missing, valid Framework, valid Extension, missing section, malformed/duplicate selected section, malformed unrelated isolation, unknown schema, workspace mismatch, and physical escape                                                         |
| MFL-009 | Lifecycle update plans                 | Unit and Integration  | Create/replace canonical whole-document bytes, repeated deterministic equality, selected-section replacement, unrelated semantic preservation, invalid state, invalid existing other section, collision, semantic no-op, and no filesystem write |
| MFL-010 | Native AOT path                        | Published Integration | Real lock, revalidation, lifecycle read, and lifecycle plan formation execute from the native test binary                                                                                                                                        |

## Execution Capsule

- Execution profile: assured behavior boundary, direct and sequential. No helper
  or parallel lane is active.
- Exact baseline: `3967534`; accepted contracts are at `7cb93e9`.
- Current owner: Overseer-managed Task Mastermind; this child is Complete.
- Completed boundary: historical `5b926e7` plus the `e7d937f` persistent-byte,
  no-metadata lock correction covers locking, lifecycle read/write-plan
  formation, preflight, and under-lock revalidation.
- Beginning evidence: warning-free Release build; managed Unit `1055/1055`,
  Integration `412/412`, EndToEnd `120/120`; portable `linux-x64` Native AOT
  Integration `412/412`, all with zero failures or skips.
- Expected changed paths: the callable-map scopes, mirrored Unit and Integration
  scopes, this Task, its parent, Plan, task index, Program Task, and Checkpoint.
- Protected paths: commands, Shell/root, public output and JSON, projects,
  packages, build configuration, TestSupport unless two projects require an
  identical helper, generated navigation, recovery-bundle behavior, atomic apply,
  lifecycle command policy, archived/sealed records, and frozen MVP.
- Direct integration neighborhood: `CliWorkspace`, `PhysicalPathResolver`,
  `FilesystemFailure`, accepted contract models, existing Extension lifecycle
  reader/validator/context, later atomic apply, and public command consumers.
- Review budget: one direct fresh-diff concurrency, TOCTOU, lifecycle-preservation,
  AOT, and future-consumer audit at acceptance (`MFL-R1`).
- Correction budget: one grouped behavior correction cycle if the review finds a
  material defect.
- Full gate: warning-free Release build, format and diff checks, focused Unit and
  real-filesystem Integration, affected Extension lifecycle regressions, full
  managed projects, and portable `linux-x64` Native AOT Integration.

## Stop Conditions

Stop if lock ownership is inferred from existence, if revalidation can occur after
the first effect, if lifecycle parsing uses reflection, or if a command bypass is
needed.

Also stop before lifecycle application without the atomic child, deletion of a
contended lock artifact, validation that follows an escaping physical alias,
loss of unrelated lifecycle value meaning, a legacy lifecycle reader, a fake
filesystem, a new dependency, or command-policy promotion.

## Progress And Evidence

- Historical result: locking, lifecycle read/write-plan formation, preflight,
  and under-lock revalidation were implemented at `5b926e7`; the recorded
  focused and full test evidence remains historical evidence for that revision.
  The historical review and evidence must not be read as proof of the current
  no-metadata authority.
- Current result: acquisition maintains persistent zero-byte lock identity and
  writes no metadata. Final M1 focused Unit `43/43`, Integration `47/47`, full managed
  `1096/458/120`, focused published `linux-x64` Recovery/source-generation
  Native AOT `10/10`, and full Native AOT Integration `458/458` pass with zero
  skips. The later accepted correction adds direct evidence for one external
  exclusive held handle, full normalized-workspace hash identity, persistent
  zero-byte reuse, contention, cancellation, and unsafe targets.
- Blockers: none within this child. Child 03 and Child 04 are independently
  Complete.
