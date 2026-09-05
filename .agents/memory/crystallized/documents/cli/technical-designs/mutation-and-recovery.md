---
open-forge:
  description: Exact BCL-first locking, recovery-bundle, expected-state, atomic-file, receipt, and guarded-deletion design
  responsibility: Define the shared mutation and recovery realization without choosing command policy
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Mutation, Recovery, Filesystem, Lock]
---

# Mutation And Recovery Technical Design

## Boundary

Shared mutation support provides immutable mechanical facts and directly
callable locking, preflight, revalidation, recovery, application, verification,
and guarded-deletion capabilities. Each command retains its plan, effect order,
selection, policy, lifecycle publication timing, findings, recovery mapping, and
result.

The [CLI Architecture](../architecture.md) defines the cross-cutting stage,
cooperating-process, external-recovery, no-automatic-rollback, and provenance
invariants. The [Shared CLI Operation Contract](../shared-operation-contract.md)
and command contracts define observable operation policy.

## Application-Data Stores And Workspace Identity

The mutation bundle writer resolves exactly:

```text
Environment.GetFolderPath(
  Environment.SpecialFolder.LocalApplicationData,
  Environment.SpecialFolderOption.Create)
```

and uses only the application-owned `OpenForge/recovery/v1` subtree. There is no
temporary-directory, repository, `HOME`, or custom-platform fallback.

Status, Doctor, and Cleanup are observers. They resolve the same special folder
with `Environment.SpecialFolderOption.None` and never create the operating-
system application-data root or either Open Forge subtree. An absent root or
store produces an available empty catalogue. An existing selected-workspace
bucket that cannot be read remains unavailable. A final ZIP that fails semantic
validation retains its exact malformed, unsupported, or unavailable condition;
neither condition is treated as absence.

Lock and recovery keys derive from the normalized physical workspace path. A
workspace move is outside automatic rediscovery. The two stores occupy separate
versioned subtrees.

## Persistent Workspace Lock

`WorkspaceLockManager` owns a persistent reusable zero-byte ordinary file under
`LocalApplicationData/OpenForge/locks/v1`, named:

```text
<friendly-workspace-name>-<full-sha256-workspace-key>.lock
```

The full lowercase SHA-256 of the normalized physical workspace path is the
identity authority. The bounded filename-safe friendly prefix derives from the
final normalized directory name, uses `workspace` as its deterministic fallback,
and is display only.

The manager opens one writable `FileStream` and holds its read/write
`FileShare.None` handle as the sole lock ownership proof. It writes no metadata,
does not truncate or delete the file, and does not infer ownership from
existence. An unlocked file is reused. Disposal releases only the handle. The
persistent pathname avoids split coordination through unlink and recreation
while another handle is open.

Current-user store resolution is deferred until acquisition after the initial
cancellation boundary. Composition, help, dry-run, verified no-op, and prompt
refusal do not create lock infrastructure. The managed BCL does not expose one
portable sharing-violation type, so an ambiguous open `IOException` remains a
typed input/output failure rather than inferred contention.

## Expected State And Preflight

`FileExpectation` distinguishes missing, ordinary-file, and directory states.
An ordinary file retains its normalized logical path, resolved physical path,
lowercase SHA-256 exact-byte identity, and, through `FileStateSnapshot`, owned
immutable bytes. Missing paths and directories invent no hash or byte payload.

`PlannedFileChange` has exactly four mechanical kinds: `Create`, `Replace`,
`Delete`, and `ReplaceGeneratedRegion`. Create requires Missing and intended
bytes. Replace and generated-region replacement require an existing ordinary
file and complete intended document bytes. Delete requires an existing ordinary
file and no intended bytes.

The plan entering shared preflight is already the command-owned ordered and
coalesced mechanical set. Equivalent effects have been coalesced, while
contradictions and overlaps have been resolved or blocked by command policy.
Shared support preserves that order and does not choose or reorder effects.

Effect-free preflight validates the set. An empty set is valid. A non-empty set
rejects duplicate logical targets, incompatible present or prospective physical
aliases, unsafe paths, stale expectations, and cancellation. Under-lock
revalidation requires a live same-workspace lease and repeats volatile
expected-state checks. Each target is revalidated again immediately before its
effect.

## Recovery Bundle

An operation with one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) prepares exactly one immutable external
ZIP before the first target effect. A create-only or semantic or byte no-op
operation does not resolve recovery storage and creates no bundle.

The deterministic external directory key and names bind the normalized physical
workspace path and operation ID. The source-generated public schema-v1
`manifest.json` has exactly one schema discriminator value: `1`; no v2, dual
reader, or compatibility layer exists. It records command and operation
identity, workspace identity, one required immutable `attribution` object, and
the ordered exact target set. The attribution contains one finite `producer`, one
finite `operation`, and one typed `subject` with `{kind, identity}`. Every current
and future recovery writer supplies those facts from trusted producer-owned
inputs. Command text, GUID, path, filename, or
ordered-entry values never infer or substitute for attribution. Ordered entries
remain exact plan evidence. Each target entry records its normalized relative
path, change kind, prior byte length, prior lowercase SHA-256, ordinal payload
name, and intended final absence or intended length and lowercase SHA-256. The
ordinal payload entries contain exact prior bytes. These fingerprints and the
attribution are static provenance, not an evolving journal.

### Schema-v1 Attribution Vocabulary

The attribution wire properties are exactly `producer`, `operation`, and
`subject`; `subject` has exactly `kind` and `identity`. Producer, operation, and
subject-kind values are case-sensitive lowercase ASCII tokens. The complete
admissible combinations are:

| Availability    | `producer`  | `operation` | `subject.kind` | Existing or accepted command identity |
| --------------- | ----------- | ----------- | -------------- | ------------------------------------- |
| Current writer  | `framework` | `install`   | `workspace`    | `install`                             |
| Current writer  | `extension` | `install`   | `workspace`    | `extension install`                   |
| Current writer  | `index`     | `index`     | `workspace`    | `index`                               |
| Current writer  | `route`     | `create`    | `workspace`    | `route create`                        |
| Current writer  | `route`     | `init`      | `workspace`    | `route init`                          |
| Current writer  | `route`     | `move`      | `workspace`    | `route move`                          |
| Current writer  | `route`     | `update`    | `workspace`    | `route update`                        |
| Accepted future | `route`     | `remove`    | `workspace`    | `route remove`                        |
| Accepted future | `framework` | `update`    | `workspace`    | `update`                              |
| Accepted future | `extension` | `update`    | `workspace`    | `extension update`                    |
| Accepted future | `extension` | `remove`    | `workspace`    | `extension remove`                    |
| Accepted future | `repair`    | `repair`    | `workspace`    | `repair`                              |

The command identity in this table is the already accepted command identity
associated with the fixed writer tuple; it never supplies or substitutes for a
typed attribution value. Cleanup has no row because its narrow deletion
operation never writes a recovery bundle.

Every admissible tuple uses the selected `CliWorkspace.PhysicalRoot` as its
trusted subject source. The writer applies
`WorkspaceIdentity.NormalizePhysicalPath` (fully qualified `Path.GetFullPath`,
with trailing directory separators trimmed except for the filesystem root),
then sets `subject.identity` to the existing
`WorkspaceIdentity.Key(normalizedPhysicalRoot)`, also exposed as the manifest's
`workspaceKey`. The key is required, non-null, and exactly the lowercase
64-hex-character SHA-256 identity produced by that helper: Windows uppercases
the normalized path before hashing, while other platforms hash it unchanged.
No operation-specific route, package, set, command, GUID, path, filename,
ordered-entry, or untrusted-byte value supplies a v1 subject. A writer that
cannot provide this trusted workspace identity cannot emit a valid final.

Readers accept only the exact property names, values, tuple combinations, and
non-null subject identity above. A missing, null, empty, unknown, malformed, or
cross-combined value is `Malformed`/unattributed with no command, operation ID,
path, filename, ordered-entry, or draft-byte fallback.

A schema-1 final that is missing or has invalid attribution is
`Malformed`/unattributed and remains preserved. It is never migrated, rewritten,
repaired, deleted, adopted, or inferred. An unknown schema version is
`Unsupported`. Drafts remain exact-name, path-only `Incomplete` facts; catalogue
observers do not inspect or use draft bytes for attribution. Only a semantically
verified current-v1 final exposes the attribution facts.

The writer uses `CreateNew` for the exact deterministic draft name. It closes and
reopens the draft, semantically decodes the source-generated manifest, and
validates exact ordered entry names and counts, declared lengths and hashes, and
exact payload bytes through bounded streaming. It then moves the draft within
the same directory to the deterministic final name, reopens the final, and
verifies it again.

Only the real store's successful final close/reopen semantic readback constructs
the opaque `RecoveryBundlePreparation`. It has no freely initializable or
caller-forgeable verification surface. A draft remains `Draft`/`Incomplete` and
never forms preparation. The final bundle is immutable.

The writer may choose `CompressionLevel.NoCompression`, but schema-v1 does not
recognize or promise compression method, ZIP entry timestamp, deterministic
archive bytes, or a whole-archive length or hash. The implementation uses
managed BCL archive and file APIs and source-generated serialization. It has no
custom ZIP parser, reflection path, native dependency, extra package, or
extraction behavior.

`RecoveryBundleCatalogue` reports neutral candidate path, kind, and integrity
facts. Finals receive one semantic read, and only a current-v1 final with valid
attribution exposes those neutral producer facts. Drafts remain exact-name,
path-only incomplete facts, and their bytes are not inspected or used for
attribution. Payload validation uses fixed bounded buffers and
never extracts, discloses, renders, logs, returns, retains, or materializes
payload bytes.

### Neutral Recovery-State Comparison

Framework attribution and a generic mixed lifecycle state do not establish
partial recovery. For one semantically verified current-v1 final selected for
the same workspace, a neutral producer may compare the current ordinary target
state for that bundle's ordered existing-target entries (`Replace`,
`ReplaceGeneratedRegion`, and `Delete`) with each entry's recorded exact prior
and intended state. The Framework attribution must be verified first, including
the `workspace` subject identity matching the selected workspace key. The
producer exposes only finite comparison states or equivalent bounded evidence;
it never exposes recovery payload bytes to Doctor.

Partial recovery is emitted only when at least one compared entry has a prior
match and at least one other entry has an intended match, every compared entry
is safely observable, and no entry is third or unknown. A prior match means the
current state is a safely observable ordinary file contained by the workspace
whose exact length and lowercase SHA-256 match the recorded prior state. An
intended match means the same exact ordinary-file comparison against the
recorded intended state, or safely proven absence when the intended state is
absence. Absence is not unavailable. An unsafe, unavailable, non-ordinary,
third, unknown, or mismatched state produces incomplete or blocked coverage or
another applicable exact finding, never partial recovery.

Each verified Framework-attributed final is evaluated independently in
deterministic catalogue order. Entries from separate bundles are never ranked,
selected as a winner, or combined. When every compared entry in one final is
intended, there is no finding (the state is compatible with a completed
historical operation); when every compared entry is prior, there is no partial
recovery finding (the state is compatible with an unapplied or fully restored
operation). Neither state proves operation history. The partial-recovery
finding describes mixed current state relative to recovery evidence and never
claims that recovery occurred.

## Atomic File Application

`FileChangeApplier` consumes one live same-workspace `WorkspaceLockLease`, one
`PlannedFileChange`, one matching expectation validation, a cancellation token,
and the matching verified `RecoveryBundlePreparation` for an existing-target
effect. Create requires `null` preparation and rejects a non-null value. Every
existing-target effect requires the matching final preparation. The applier
performs one final effect per target.

Create, Replace, and ReplaceGeneratedRegion use full-document mechanics:

1. Create one unpredictable adjacent stage in the resolved target directory with
   `Path.GetRandomFileName`, `FileMode.CreateNew`, and ordinary managed BCL APIs.
2. Write all intended bytes, close the stage, and read it back for exact-byte
   equality.
3. Repeat target revalidation and the final cancellation check.
4. Use same-directory `File.Move` with `overwrite: false` for Create and
   `overwrite: true` for Replace and ReplaceGeneratedRegion.
5. Revalidate exact resulting identity and bytes.

ReplaceGeneratedRegion receives the complete intended document and does not
parse or recompute a region. There is no copy-delete, in-place write, weaker
fallback, retry, forced durability, native or unsafe path, reflection path, or
deterministic failure-injection seam.

Delete accepts only a matched ordinary-file expectation, revalidates immediately
before `File.Delete`, and verifies Missing. It never deletes a directory, device,
link, or other non-file state.

Stage cleanup removes only the exact ordinary stage this invocation created. A
failed `CreateNew` never claims or removes a pre-existing lookalike. A changed,
reparse, directory, or inaccessible stage is retained rather than followed or
broadly deleted.

## Receipts And Retained State

`FileChangeReceipt` distinguishes `Verified`, `VerificationFailed`,
verification-unavailable `Applied`/`Failed`, `NotStarted`, and
`CompletionUnknown`. A receipt carries the accepted before-state snapshot and,
when observation succeeds, the exact after-state snapshot.

`NotStarted` carries no after-state and exactly one neutral reason:
`Cancelled`, `TargetChanged`, `ApplicationFailed`, or `ContractRejected`.
Pre-effect cancellation maps to `Cancelled`; expected-state drift maps to
`TargetChanged`; a known application failure before the target effect maps to
`ApplicationFailed`; and rejection of an invalid mechanical request maps to
`ContractRejected`.

An exact intended after-state observation produces `Verified`. A known mismatch
after an effect produces `VerificationFailed` and retains the mismatching
after-state. `Applied` or `Failed` is used only when after-state verification is
unavailable after the corresponding known application outcome.

After a successful synchronous Move or Delete, cancellation cannot reinterpret
the effect as NotStarted. A thrown Move or Delete is post-observed: exact
intended state is Verified; unchanged before state is
NotStarted/ApplicationFailed; any other changed or unavailable state is
CompletionUnknown, with the observed state when available. CompletionUnknown is
never used for a known pre-effect failure. A receipt reports mechanics only and
never claims command success, compensation, or rollback.

Handled failure or cancellation stops new effects and reports the actual
residual draft or final path. A valid final remains available after completed
preparation. A closed final may remain after abrupt process termination, but the
design promises no executable crash or power-loss durability. A fresh invocation
plans from current facts; no journal, progress receipt, history, saved plan, or
replay mechanism exists.

## Guarded Recovery-Artifact Deletion

Given an existing same-workspace lease and one caller-selected eligible candidate,
`RecoveryBundleDeletionGuard` re-enumerates the workspace bucket once, proves the
exact path, name, and kind, immediately rereads a verified current-v1 final
semantically or revalidates an exact-name ordinary draft, performs ordinary
`File.Delete`, and verifies positive absence. Valid attribution is an integrity
fact, not deletion authority.

The guard returns one state (`Deleted`, `Failed`, `Blocked`, or `Cancelled`) and
one orthogonal disposition (`Removed`, `Retained`, or `Unknown`). Deleted requires
Removed and no residual. Retained requires a positive current observation and
the exact residual path. Observation failure leaves Unknown. The guard does not
select candidates, acquire the lease, orchestrate multiple deletions, infer
activity, define Cleanup results, or choose operation-level removal timing.
Unknown, malformed, mismatched, differently keyed, or lookalike artifacts remain
untouched.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Shared CLI Operation Contract](../shared-operation-contract.md)
- [Directory Creation Technical Design](directory-creation.md)
- [Lifecycle Provenance Technical Design](lifecycle-provenance.md)
- [Cleanup Contract](../contracts/cleanup/_cleanup.md)
