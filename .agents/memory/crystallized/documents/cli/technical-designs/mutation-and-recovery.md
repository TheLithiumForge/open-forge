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
workspace path and operation ID. The source-generated schema-v1 `manifest.json`
records schema version, command and operation identity, workspace identity, and
the ordered exact target set. Each target entry records its normalized relative
path, change kind, prior byte length, prior lowercase SHA-256, ordinal payload
name, and intended final absence or intended length and lowercase SHA-256. The
ordinal payload entries contain exact prior bytes. These fingerprints are static
provenance, not an evolving journal.

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
facts. Finals receive one semantic read. Drafts remain exact-name, path-only
incomplete facts. Payload validation uses fixed bounded buffers and never
extracts, discloses, renders, logs, returns, retains, or materializes payload
bytes.

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
exact path, name, and kind, immediately rereads a verified final semantically or
revalidates an exact-name ordinary draft, performs ordinary `File.Delete`, and
verifies positive absence.

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
