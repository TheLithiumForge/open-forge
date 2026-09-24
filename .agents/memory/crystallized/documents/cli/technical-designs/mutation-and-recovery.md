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

`NoFollowLeafObservation` is the neutral final-component fact used by every
mutation that addresses a logical file leaf. It records the canonical logical
path and one leaf state: `Missing`, `OrdinaryFile`, `Directory`,
`RelativeFileLink`, `Link`, `ReparsePoint`, `Special`, `Inaccessible`, or
`Unknown`. `RelativeFileLink` is the exact supported relative file-symbolic-link
identity. It retains the exact raw relative target and link kind without
resolving the target. `Link` is the generic state for every other safely
observable symbolic link, including an absolute, unsupported, or otherwise
non-relative target. Its `LinkIdentity` retains the observed link kind and,
when safely observable, the raw target and target form (`relative`, `absolute`,
or `unsupported`) without resolving the target. If a link is identifiable but
its target facts are unavailable, it remains `Link` with those fields unavailable
and never becomes `RelativeFileLink`; if the link identity itself cannot be
observed, the state is `Inaccessible` or `Unknown`.

The observation is made before ordinary physical resolution, during initial
preflight, during under-lease revalidation, and immediately before the effect.
Any `RelativeFileLink` or generic `Link`, reparse point, or special final leaf
blocks ordinary `Create`, `Replace`, `Delete`, and `ReplaceGeneratedRegion`.
Every generic `Link`, reparse point, or special final leaf also blocks a
`RelativeFileLinkEffect`; only the exact `RelativeFileLink` identity is accepted
by that effect and by recovery. Stable contained directory-link ancestry
continues to follow the ordinary physical path contract.

`FileExpectation` distinguishes missing, ordinary-file, and directory states.
An ordinary file retains its normalized logical path, resolved physical path,
lowercase SHA-256 exact-byte identity, and, through `FileStateSnapshot`, owned
immutable bytes. Missing paths and directories invent no hash or byte payload.

`PlannedFileChange` has four ordinary-file mechanical kinds: `Create`, `Replace`,
`Delete`, and `ReplaceGeneratedRegion`. Create requires Missing and intended
bytes. Replace and generated-region replacement require an existing ordinary
file and complete intended document bytes. Delete requires an existing ordinary
file and no intended bytes. A command may mark a prior-missing ordinary Create
as reversible when its record must be removed if the operation stops.

`RelativeFileLinkEffect` is a separate typed mechanical shape with `Create` and
`Delete` kinds. It retains the canonical logical destination path, the finite
relative-file-link kind, the exact raw relative target, and state-specific
expected and intended identities. Create requires Missing and intends that
exact `RelativeFileLink` identity. Delete requires that exact identity and
intends Missing. Link identity never includes target bytes. Only this typed
Library relative-file-link effect may create or delete a link object; ordinary
file effects reject every link final leaf, including a generic `Link`.

The plan entering shared preflight is already the command-owned ordered and
coalesced mechanical set. Equivalent effects have been coalesced, while
contradictions and overlaps have been resolved or blocked by command policy.
Shared support preserves that order and does not choose or reorder effects.

Effect-free preflight validates the set. An empty set is valid. A non-empty set
rejects duplicate logical targets, incompatible present or prospective physical
aliases, unsafe paths, stale expectations, and cancellation. It obtains the
no-follow final-leaf fact before ordinary physical resolution and rejects a
present link, reparse point, or special final leaf for an ordinary file effect.
Under-lock revalidation requires a live same-workspace lease and repeats every
volatile expected-state and no-follow leaf check. Each target is revalidated
again immediately before its effect.

## Recovery Bundle

An operation with one or more reversible non-no-op effects prepares exactly one
immutable external ZIP before the first target effect. Covered effects include
ordinary existing-file `Replace`, `ReplaceGeneratedRegion`, and `Delete`,
relative-file-link `Create` and `Delete`, and a prior-missing ordinary `Create`
when it creates a record that the operation must be able to remove. A semantic
or byte no-op operation does not resolve recovery storage and creates no bundle.

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
remain exact plan evidence. Each target entry records its canonical
`logicalPath`, typed `kind`, and state-specific `prior` and `intended` identity.
The admissible entry kinds are `ordinary-create`, `ordinary-replace`,
`ordinary-replace-generated-region`, `ordinary-delete`,
`relative-file-link-create`, and `relative-file-link-delete`. An ordinary-file
identity contains exact byte length and lowercase SHA-256; a missing identity
contains neither; a relative-file-link identity contains the exact raw relative
target and link kind and never target bytes. The ordinal payload entries contain
exact prior bytes only for an existing ordinary-file prior state. A
prior-missing ordinary Create has no payload. These identities and the
attribution are static provenance, not an evolving journal.

The typed entry shape is:

```text
RecoveryEntry
  logicalPath: canonical workspace-relative path
  kind: RecoveryEntryKind
  prior: Missing | OrdinaryFile(length, sha256) | RelativeFileLink(rawTarget, linkKind)
  intended: Missing | OrdinaryFile(length, sha256) | RelativeFileLink(rawTarget, linkKind)
  priorPayload: ordinal payload name only when prior is OrdinaryFile
```

The state identity must match the effect kind: ordinary Create is
`Missing -> OrdinaryFile`, ordinary Replace and generated-region replacement
are `OrdinaryFile -> OrdinaryFile`, ordinary Delete is `OrdinaryFile -> Missing`,
relative-file-link Create is `Missing -> RelativeFileLink`, and relative-file-link
Delete is `RelativeFileLink -> Missing`. No other state pair is admissible.

### Schema-v1 Attribution Vocabulary

The attribution wire properties are exactly `producer`, `operation`, and
`subject`; `subject` has exactly `kind` and `identity`. Producer, operation, and
subject-kind values are case-sensitive lowercase ASCII tokens. The complete
admissible combinations are:

| Availability    | `producer`  | `operation` | `subject.kind` | Existing or accepted command identity |
| --------------- | ----------- | ----------- | -------------- | ------------------------------------- |
| Current writer  | `framework` | `install`   | `workspace`    | `install`                             |
| Current writer  | `workspace` | `remove`    | `workspace`    | `remove`                              |
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
| Accepted future | `library`   | `attach`    | `workspace`    | `library attach`                      |
| Accepted future | `library`   | `sync`      | `workspace`    | `library sync`                        |
| Accepted future | `library`   | `detach`    | `workspace`    | `library detach`                      |

The command identity in this table is the already accepted command identity
associated with the fixed writer tuple; it never supplies or substitutes for a
typed attribution value. Root `remove` uses the `workspace` producer for ordinary
path removal. Its route, Extension and Library selections retain their existing
domain writer tuples. No other operation is valid for the `workspace` producer.
Cleanup has no row because its narrow deletion
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

## No-Follow Recovery Comparison And Application

Explicit recovery comparison and application use the same
`NoFollowLeafObservation` as ordinary mutation. They do not resolve the final
leaf before deciding whether it matches a recorded state. For each recovery
entry, the current state must be one of the exact admissible states below:

- To restore a prior `Missing` state, the current leaf must match the exact
  intended object. Recovery deletes only that exact current object. A different
  ordinary file, directory, `RelativeFileLink`, generic `Link`, reparse point,
  special object, or unavailable leaf blocks.
- To restore a prior `OrdinaryFile` state, the current leaf must match the
  intended ordinary-file identity and pass the ordinary no-follow guard.
  Recovery writes the recorded prior bytes through the guarded ordinary
  same-directory replacement path.
- To restore a prior `RelativeFileLink` state, the current leaf must be exact
  `Missing`. Recovery creates only the recorded relative-file-symbolic-link
  kind with the exact raw relative target. It does not resolve the target or
  read target bytes, so an exact dangling link may be recreated. A generic
  `Link`, regardless of target form, never satisfies this identity.

A third, mismatched, unsafe, generic-link, reparse, special, or unavailable
state blocks the entry and the recovery operation does not apply a safe subset.
Recovery never writes, deletes, or resolves a source target. These capabilities
support an explicit Repair operation and residual evidence; no command claims
automatic rollback or compensation.

## Relative-File-Link Application

`RelativeFileLinkApplier` consumes one live same-workspace lease, one typed
relative-file-link effect, its matching no-follow expectation, cancellation,
and the matching verified recovery preparation when the effect is reversible.
It validates the parent as a real contained directory, inspects the final leaf
without following it, and applies exactly one link-object change:

1. Create uses the declared raw relative target and link kind on an exact
   missing final leaf.
2. Delete removes the exact expected relative file link on an immediate
   no-follow match.
3. Verification inspects the final leaf without following it and requires the
   exact intended missing or `RelativeFileLink` identity.

The applier never follows the link, writes its target, deletes its target, or
uses target bytes as identity. It has no copy-delete, absolute-link, directory-
link, native, unsafe, or weaker fallback. A generic `Link` with an absolute,
unsupported, or otherwise non-relative target is never accepted. A changed
occupant, target, link kind, parent, or containment fact stops the effect.

## Atomic File Application

`FileChangeApplier` consumes one live same-workspace `WorkspaceLockLease`, one
`PlannedFileChange`, one matching expectation validation, a cancellation token,
and the matching verified `RecoveryBundlePreparation` for a covered effect. An
ordinary Create requires `null` preparation unless the command marked that
prior-missing Create as reversible, in which case it requires the matching
final preparation. Every covered existing-target effect requires the matching
final preparation. The applier performs one final effect per target.

Create, Replace, and ReplaceGeneratedRegion use full-document mechanics:

1. Create one unpredictable adjacent stage in the resolved target directory with
   `Path.GetRandomFileName`, `FileMode.CreateNew`, and ordinary managed BCL APIs.
2. Write all intended bytes, close the stage, and read it back for exact-byte
   equality.
3. Repeat the no-follow final-leaf observation, target revalidation, and the
   final cancellation check.
4. Use same-directory `File.Move` with `overwrite: false` for Create and
   `overwrite: true` for Replace and ReplaceGeneratedRegion.
5. Revalidate exact resulting identity and bytes.

ReplaceGeneratedRegion receives the complete intended document and does not
parse or recompute a region. There is no copy-delete, in-place write, weaker
fallback, retry, forced durability, native or unsafe path, reflection path, or
deterministic failure-injection seam.

Delete accepts only a matched ordinary-file expectation, revalidates the
no-follow final leaf immediately before `File.Delete`, and verifies Missing. It
never deletes a directory, device, link, or other non-file state.

Stage cleanup removes only the exact ordinary stage this invocation created. A
failed `CreateNew` never claims or removes a pre-existing lookalike. A changed,
reparse, directory, or inaccessible stage is retained rather than followed or
broadly deleted.

## Receipts And Retained State

`FileChangeReceipt` distinguishes `Verified`, `VerificationFailed`,
verification-unavailable `Applied`/`Failed`, `NotStarted`, and
`CompletionUnknown`. A receipt carries the accepted before-state snapshot and,
when observation succeeds, the exact after-state snapshot.

`RelativeFileLinkReceipt` carries the same mechanical outcome states while its
before and after snapshots are no-follow leaf observations. A link snapshot
contains only the logical path, link kind, and exact raw relative target; it
contains no target bytes or resolved target identity. A link effect is
`Verified` only when the intended missing or exact-link observation is proved.

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

Cleanup's multiple-candidate application uses an opaque deletion-validation
session opened from the held same-workspace lease and its frozen complete
catalogue. Opening re-enumerates the selected workspace bucket once and compares
the whole filtered catalogue's relevant identity and semantic facts before any
deletion. An unavailable, blocked, changed, or cancelled opening grants no
deletion authority.

Before catalogue enumeration, Cleanup and session opening validate the physical
recovery-directory chain through the existing managed path-component reader.
They repeat that check after observation; the held session also checks it before
candidate validation and deletion. A link, non-directory, or unavailable
component prevents deletion authority. These checks belong to Cleanup and its
session; the shared catalogue and existing single-candidate guard retain their
producer and deletion policies.

The session accepts only an unchanged member of that validated catalogue. Each
deletion checks the live matching lease and immediately repeats the final's
semantic validation or the ordinary draft's exact name, path, and kind checks,
then performs bounded file deletion and proves absence. It does not recatalogue
after its own deletions. Candidate selection, ordinal ordering, stopping, and
operation-level effect and residual reporting remain with Cleanup. Empty
catalogues and dry runs do not acquire a lease or open a session. The session
adds no persistent state, activity inference, producer policy, or public
operation; existing single-candidate callers retain their guard policy.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Shared CLI Operation Contract](../shared-operation-contract.md)
- [Directory Creation Technical Design](directory-creation.md)
- [Ownership And Source Alignment Technical Design](lifecycle-provenance.md)
- [Workspace Libraries Technical Design](workspace-libraries.md)
- [Cleanup Contract](../contracts/cleanup/_cleanup.md)
