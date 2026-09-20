---
open-forge:
  description: Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Mutation, Contract, Lifecycle, Recovery]
---

# Freeze Mutation And Lifecycle Contracts

## Task State

- State: Complete. Historical contract implementation remains exact commit
  `7cb93e9` from planning commit `0ae9da8` and integrated baseline `bba84b6`.
  Current external bundle, persistent-byte lock, matching-applier, and complete
  replacement-product Git-removal corrections are implemented in exact
  production candidate `e7d937f` under authority `01dd552`; Child 05 final gates
  pass. Public Index is Ready and next.
- Responsible role: Overseer acting directly and sequentially.
- Parent: [Mutation Foundation](_mutation-foundation.md).
- Last updated: 2026-08-29.

## Expected Outcome

Every mutating command can depend on one accepted set of immutable callable
contracts for observation, preconditions, lock ownership, planned changes,
application receipts, lifecycle state, and recovery provenance without delegating
product policy to a generic engine.

## Accepted Contract Decisions

### Lifecycle document

- Keep one physical `.agents/open-forge.lifecycle.json` document at schema
  version `1`. Reuse the existing source-generated lifecycle serialization
  boundary. Do not add a second lifecycle format, legacy reader, migration shape,
  serializer context, or reflection fallback.
- The canonical common envelope has exactly five ordered root keys:
  `schemaVersion`, `fingerprintPolicy`, `workspacePath`, `framework`, and
  `extensions`. A newly created document emits both section keys. Framework
  creation sets `extensions` to a complete empty `ExtensionLifecycleState` with
  `coverage: "complete"`, `packages: []`, and `paths: []`. Framework creation
  never emits a null or omitted `Extensions` section. The envelope contains no
  plan, receipt, comparison, recovery, Git, session, or command history.
- Parse the two logical sections independently. The common envelope exposes each
  section as an isolated JSON value. A Framework reader validates only the
  Framework value and an Extension reader validates only the Extension value.
  Invalid unrelated-section meaning cannot erase valid selected-section facts
  during reads, but an existing missing, explicit-null, malformed, or incomplete
  selected or unrelated section is untrusted for planning. `PlanFrameworkUpdate`
  and `PlanExtensionUpdate` return blocked with no change for those documents;
  update has no repair authority and never silently repairs them.
- The Framework section contains exact `coverage`, one embedded-source identity,
  ordered managed target identities, and ordered generated-region identities.
  Source identity is `id`, optional descriptive `version`, and one lowercase
  SHA-256 `inventoryFingerprint`. A managed target is `path`, optional `region`,
  `baselineFingerprint`, and `fingerprintKind`. A generated-region identity is
  `path` plus `region`. Section trust is derived by strict validation; it is not
  persisted as a second potentially contradictory field.
- The Extension section keeps the already accepted exact schema: `coverage`,
  ordered `packages`, and ordered `paths`. Package and path member shapes do not
  change.
- `open-forge-markdown-v1` remains the sole semantic fingerprint policy. A
  supported parseable target persists a semantic baseline fingerprint. An
  unsupported or unparseable target may persist exact-byte identity only when its
  command contract admits that target and equivalence fails closed. Exact current
  bytes remain operation-time facts and never become a second persistent baseline
  for parseable Markdown.
- Writers publish only their selected lifecycle state after successful
  application and verification. They preserve unrelated section and
  common-envelope meaning semantically. A selected semantic change is serialized
  as one deterministic canonical UTF-8 whole-document representation; lifecycle
  formatting, ordering, whitespace, and line-ending trivia are not preserved. A
  semantic no-op writes nothing. Dry-run and any incomplete, invalid, blocked,
  failed, or interrupted operation publish no lifecycle state.

### Paths, hashes, and observations

- Every mutation path contract carries a normalized absolute logical path. A
  present observation also carries the physically resolved absolute path proven
  by the accepted `PhysicalPathResolver` boundary.
- One `FileExpectation` factory distinguishes missing, ordinary-file, and
  directory states. Existing ordinary files carry a lowercase SHA-256 exact-byte
  hash; missing paths and directories do not invent one.
- One immutable `FileStateSnapshot` adds owned immutable exact bytes for an
  observed ordinary file. Missing paths and directories carry no byte payload.
  It is an operation-time fact, not persisted lifecycle state.
- `PlannedFileChange` has exactly four mechanical kinds: create, replace,
  delete, and generated-region replacement. The caller supplies one expectation
  and intended immutable bytes. Create requires a missing expectation. Replace,
  delete, and generated-region replacement require an existing ordinary-file
  expectation. Delete alone has no intended bytes.
- `FileChangeReceipt` distinguishes a verified effect, an effect whose
  verification failed, an effect for which this operation's target effect is
  proved not applied, and an effect whose completion is unknown. Its factories
  enforce the valid before/after snapshot,
  verification, and cause combinations. `NotStarted` requires exactly one neutral
  reason: `Cancelled`, `TargetChanged`, `ApplicationFailed`, or
  `ContractRejected`. A successful effect with unavailable verification is
  `Applied`/`Failed` with no after snapshot; an observed mismatch retains its
  after snapshot. `CompletionUnknown` is exclusive to a thrown target effect
  whose completion cannot be proved; exact intended observation remains
  `Verified`. A receipt reports mechanics only; it never claims compensation,
  rollback, or command success.

### Lock and recovery-bundle contracts

- `WorkspaceLockRequest` carries one accepted `CliWorkspace`, command identity,
  and non-empty operation ID. `WorkspaceLockLease` owns the exclusive OS handle
  and exact logical/resolved-physical lock identity until disposal.
  `WorkspaceLockResult` distinguishes acquired, failed, and cancelled without
  treating an unlocked file as ownership. The persistent lock file's existing
  bytes are preserved; ownership is only the held `FileShare.None` handle, with
  no metadata write, truncation, or deletion.
- `RecoveryBundlePreparation` identifies one immutable, semantically verified
  final external ZIP bundle for a complete operation. Only the real bundle store
  constructs it, after successful final close/reopen semantic readback, binding
  normalized physical workspace identity, command, operation ID, exact ordered
  target entries, prior bytes and fingerprints, intended final fingerprints, and
  final verified storage identity. It grants no replay, restoration, rollback,
  or compensation authority.
- Bundle storage resolves only current-user LocalApplicationData's
  `OpenForge/recovery/v1` subtree. A draft remains `Incomplete`; only a final ZIP
  with semantic schema, exact ordered entry, length, hash, and payload-byte
  validation forms preparation. `Create` and no-op plans have no preparation.
  The shared applier requires a matching preparation for every existing-target
  effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`), and all preparations
  are ready before the first target effect.

## Placement And Callable Map

| Capability    | Exact production scope                                      | Contract boundary                                                                  |
| ------------- | ----------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| Locking       | `Framework/Mutation/Locking/Models/`                        | Request, owned lease, and typed result only                                        |
| File mutation | `Framework/Mutation/Models/Filesystem/`                     | Expectations, snapshots, finite changes, and receipts                              |
| Lifecycle     | `Framework/Lifecycle/Models/` and existing `Serialization/` | One isolated schema-v1 envelope and exact section DTOs                             |
| Recovery      | `Framework/Recovery/Models/`                                | Immutable bundle preparation, manifest, target fingerprints, and recognition facts |

Every folder and namespace mirrors this placement. Property-only serialized
shapes use clear `required init` members. Cross-member invariant records and
resource owners use focused named factories or constructors. No long positional
constructor or broad context bag is accepted.

## Required Models

Create cohesive contracts under `Core/Framework/`:

- `WorkspaceLockRequest`, `WorkspaceLockLease`, and `WorkspaceLockResult` under
  `Mutation/Locking/`. A lease is positive OS ownership, not file existence.
- `FileExpectation` captures path, expected kind, existence, identity, and content
  hash needed for revalidation.
- `PlannedFileChange` captures create, replace, delete, or generated-region change
  plus expected state and intended bytes. It contains no command-specific reason.
- `FileChangeReceipt` records exact observed before/after identity, bytes/hash,
  effect state, verification, and the required neutral not-started reason only
  when this operation's target effect is proved not applied.
- `LifecycleEnvelopeV1`, `FrameworkLifecycleState`, and
  `ExtensionLifecycleState` model only accepted persisted schema.
- `RecoveryBundlePreparation` binds the workspace, command, operation ID,
  immutable bundle, ordered target set, exact prior payloads, and intended final
  fingerprints only after final close/readback verification.
- `RecoveryBundleReader` recognizes only the semantic schema, exact ordered
  entry names and counts, declared lengths and hashes, and exact payload bytes;
  it never extracts a bundle and never binds a changed workspace root.

## Invariants

- Fact and effect records are immutable and state-valid by factory.
- No record carries parser, renderer, writer, service, or broad command context.
- Commands form their own plan type from these primitives.
- Lifecycle version is explicit; no legacy shape, fallback, or migration field.
- Recovery never promises automatic restoration, rollback, compensation, or
  current-target classification. It retains static prior and intended provenance
  and reports actual residual draft or final paths.
- Lock, expectation, receipt, lifecycle, and recovery identities use normalized
  and physically proven workspace paths from Foundation.

## Evidence Matrix

| ID      | Contract or failure class               | Tier                             | Required proof                                                                                                                                                                                                                                              |
| ------- | --------------------------------------- | -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| MFC-001 | File expectation and observation states | Unit                             | Valid missing/file/directory facts; invalid path, physical identity, hash, and byte combinations                                                                                                                                                            |
| MFC-002 | Planned changes                         | Unit                             | Every finite kind plus incompatible expectation and intended-byte rejection                                                                                                                                                                                 |
| MFC-003 | Receipts                                | Unit                             | Verified, observed verification mismatch, verification unavailable, all four named not-started reasons plus undefined rejection, and unknown completion; exact intended observation after a thrown effect remains Verified; every impossible state rejected |
| MFC-004 | Lock request/result                     | Unit                             | Cohesive workspace identity, non-empty command/operation, lease-only acquisition, and typed failure/cancellation states                                                                                                                                     |
| MFC-005 | Lifecycle schema v1                     | Unit and Integration             | Exact property graph, independent selected-section decoding, duplicate/unknown selected fields, unknown version, and source-generated in-memory round trip                                                                                                  |
| MFC-006 | Framework schema                        | Unit and Integration             | Exact source/target/generated-region fields, deterministic order prerequisites, semantic versus exact-byte fingerprint values, and no persisted trust/history                                                                                               |
| MFC-007 | Extension compatibility                 | Existing and focused Integration | Existing accepted lifecycle documents retain identical Extension read results, including opaque malformed Framework isolation                                                                                                                               |
| MFC-008 | Recovery bundle preparation             | Unit and Integration             | Semantic schema, immutable final external bundle identity, exact ordered entries, prior payloads, intended fingerprints, draft non-preparation, and rejection of missing or contradictory preparation                                                       |
| MFC-009 | Recovery target coverage                | Unit and Integration             | Every existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) has one matching preparation; Create and no-op have none; no extraction, replay, restoration, rollback, or compensation authority                                           |
| MFC-010 | Native AOT serialization                | Published Integration            | Source-generated lifecycle and recovery-manifest graphs construct, serialize, and decode with reflection disabled and no filesystem write                                                                                                                   |

## Execution Capsule

- Execution profile: assured contract boundary, direct and sequential. No helper
  or parallel lane is active.
- Exact baseline: `bba84b6`.
- Current owner: Overseer; this child is Complete.
- Completed boundary: contract implementation, authority corrections, and
  integrated acceptance evidence.
- Beginning evidence: exact predecessor tree retains warning-free Release and
  final managed Unit `1031/1031`, Integration `411/411`, EndToEnd `120/120`,
  portable `linux-x64` Native AOT Integration `411/411`, and Native AOT EndToEnd
  `120/120`, all with zero failures or skips.
- Expected changed paths: the placement map, mirrored Unit and Integration
  scopes, this Task, its parent, Plan, task index, Program Task, and Checkpoint.
- Protected paths: command production, root composition, Shell, public output and
  JSON, projects, packages, build configuration, generated navigation behavior,
  archived or sealed records, frozen MVP, and every mutating behavior service.
- Direct integration neighborhood: existing Lifecycle reader/validator/context,
  Markdown fingerprint policy, `CliWorkspace`, physical path facts, existing
  Extension List/Inspect consumers, and later children 02 through 04.
- Review budget: one direct fresh-diff correctness and future-consumer audit at
  contract acceptance (`MFC-R1`).
- Correction budget: one grouped contract correction cycle if that review finds
  a material state or schema defect.
- Full gate: warning-free Release build, format and diff checks, focused Unit and
  Integration, affected Extension lifecycle regressions, full managed projects,
  and portable `linux-x64` Native AOT Integration serialization evidence.

## Evidence

Unit tests cover every valid and invalid construction, schema serialization,
unknown versions, missing provenance, impossible receipt states, and workspace
boundaries. Integration source-generation tests construct the lifecycle context
in managed and Native AOT execution without writing files.

## Stop Conditions

Stop if contracts require one universal command plan/result, hide command ordering,
assume every effect is reversible, recognize a legacy lifecycle format, make an
unrelated section invalidate selected-section facts, require reflection or a new
dependency, or cannot preserve the accepted schema with BCL-first Native AOT
mechanics.

## Progress And Evidence

- Historical result: The lifecycle envelope and callable decisions were recorded
  at `7cb93e9`. The lifecycle envelope still isolates Framework and Extension
  JSON values, the existing Extension reader retains accepted behavior, and the
  then-current typed mutation, lock, repository, and recovery contracts rejected
  contradictory states. The historical locking and recovery-contract portions
  are superseded by the current persistent-byte lock and external
  recovery-bundle authority.
- Historical review `MFC-R1`: accepted after one grouped correction cycle for
  the then-current contract record. Its direct fresh-diff review added exact-byte
  no-op rejection for replacement plans, rejected a recovery artifact that
  aliased its target, enforced the then-current repository working-directory
  containment, split its oversized repository contract file, and replaced
  ambiguous positional construction with named arguments. Its final review found
  no remaining material correctness, contract, locality, or future-consumer
  defect within the historical child as then scoped. Those recovery and
  repository decisions do not bind the current external bundle correction
  tracked by Child 04; the historical lifecycle and immutable callable findings
  remain useful evidence but do not accept the current recovery boundary.
- Historical contract evidence: warning-free Release solution build; format and
  diff checks; focused mutation-foundation Unit `44/44` and Integration `25/25`;
  full managed Unit `1075/1075`, Integration `436/436`, and EndToEnd `120/120`;
  and published portable `linux-x64` Native AOT Integration `436/436`, all with
  zero failures or skips.
- Current result: the external immutable bundle, persistent-byte lock, and
  matching-applier contract corrections are implemented at `e7d937f`. Final M1
  focused Unit `43/43`, Integration `47/47`, full managed `1096/458/120`,
  focused published `linux-x64` Recovery/source-generation Native AOT `10/10`,
  and full Native AOT Integration `458/458` pass with zero skips. Static
  replacement-product Git results are zero, and final independent review is
  `ROBUST`, safe to commit, confidence `0.98`.
