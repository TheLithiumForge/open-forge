---
open-forge:
  description: Exact BCL-first locking, recovery-bundle, expected-state, atomic-file, receipt, and guarded-deletion design
  responsibility: Define the shared mutation and recovery realization without choosing command policy
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Mutation, Recovery, Filesystem, Lock]
---

# Mutation And Recovery Technical Design

## Layout Observation

Mutation plans retain the operation’s `WorkspaceLayoutRead`. Validate its settings, anchor and file expectations under the held workspace lease before permission publication or target effects. Never replace the plan’s layout with a fresh selection. Post-effect verification checks intended state. The [layout design](workspace-layout.md#observation-and-revalidation) defines drift and mismatch handling.

A custom context folder does not relocate `.open-forge-state/`, the physical-workspace lock, new recovery storage or historical recorded paths. Recovery paths and recorded installed targets stay literal. Delivery 1 introduces no relocation recovery protocol.

## Boundary

Shared mutation support provides immutable mechanical facts and directly
callable locking, preflight, revalidation, recovery, application, verification,
and guarded-deletion capabilities. Each command retains its plan, effect order,
selection, policy, lifecycle publication timing, findings, recovery mapping, and
result.

The [CLI Architecture](../architecture.md) defines the cross-cutting stage,
cooperating-process, local-recovery, no-automatic-rollback, and provenance
invariants. The [Shared CLI Operation Contract](../shared-operation-contract.md)
and command contracts define observable operation policy.

## Runtime Paths And Workspace Identity

Every selected physical workspace has a fixed runtime folder:

```text
.open-forge-state/
  .gitignore
  workspace.lock
  recovery/v1/<workspace key>/
```

`WorkspaceRuntimePaths` derives these paths from `CliWorkspace.PhysicalRoot`.
Bootstrap creates or repairs an ordinary `.gitignore` containing `*` before
creating any other runtime file. The folder ignores itself in Git. Git clean
with `-x` or `-X` can delete it, including recovery evidence.

New bundles always use the local store. `RecoveryStoreLocator` constructs typed
workspace and legacy locations without creating storage. The legacy location
uses the existing per-user `OpenForge/recovery/v1` layout. An absolute
`OPENFORGE_DATA_HOME` selects only that legacy data root and its `OpenForge/locks/v1`
bridge. A relative value or failed resolution makes legacy coverage unavailable.
It never relocates or prevents the local store.

Read the workspace store at `.open-forge-state/recovery/v1/<workspace key>/` and the legacy per-user store separately, in that order. Reads create neither store and never probe lock writability. An absent store is an available empty catalogue. Unavailable coverage is not an empty catalogue. Keep readable candidates when the other store is unavailable. Preserve store identity, exact paths, integrity and causes. Aggregate totals remain unknown when coverage is incomplete.

The full SHA-256 key derives from the normalized physical workspace path.
Separate worktrees have independent local locks and bundles. Moving a workspace
does not rediscover bundles under its previous key.

The runtime subtree is reserved from source inventory, direct source reads,
Markdown traversal, inclusion, package destinations and removal. Admission checks
both lexical and resolved physical paths. Recovery observers deliberately read
their own stores. Framework removal preserves the runtime folder.

## Persistent Workspace Lock

`WorkspaceLockManager` opens `.open-forge-state/workspace.lock` without truncating,
rewriting or deleting it. Its contents and length do not establish ownership.
The live handle owns the lock. Windows uses `FileShare.None`. Unix also requires
a successful checked `flock(LOCK_EX | LOCK_NB)`. The only native interop is
`flock`. BCL attributes and link targets reject directories and links before
opening. A non-seekable handle is rejected as `InvalidTarget` after opening.

Typed failures preserve the exact path and one cause: `Busy`, `AccessDenied`,
`Unsupported`, `InvalidTarget`, `IoError` or `Cancelled`. Only proven contention
means another writer holds the lock. There is no contention wait loop. Unix
retries an interrupted system call with cancellation checks. No failure advises
deleting a lock or recovery bundle.

After local acquisition, the bridge attempts the existing legacy lock read-write.
On access denial it opens an existing legacy file read-only with exclusive
sharing. It skips the bridge only when denied access accompanies proven absence,
or legacy-root resolution is unavailable. Other bridge failures release the local
handle and fail with their typed cause. The legacy zero-length check remains.
The legacy filename retains its friendly prefix and full workspace key.

The composite lease validates the physical workspace and fixed local path.
It releases the legacy handle before the local handle, including on failure and
cancellation. Process exit releases ownership. A leftover sentinel blocks nothing.
Planning, prompts, refusals, dry runs and verified no-ops create no runtime state.

The bridge ends at 1.0. Its accepted mixed-version gap is an older CLI
(0.9.0-beta.17 or earlier) that has never written this workspace, started outside
the sandbox during a sandboxed write. Older readers may miss new local bundles.
Existing legacy files and bundles stay in place.

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
exact platform-form `RelativeFileLink` identity. Portable mapping targets keep
`/`, while Create uses the platform directory separator. Delete requires the
exact observed owned identity and
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
immutable local ZIP before the first target effect. Covered effects include
ordinary existing-file `Replace`, `ReplaceGeneratedRegion`, and `Delete`,
relative-file-link `Create` and `Delete`, and a prior-missing ordinary `Create`
when it creates a record that the operation must be able to remove. A semantic
or byte no-op operation does not resolve recovery storage and creates no bundle.

The deterministic workspace directory key and names bind the normalized physical
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
| Current writer  | `route`     | `include`   | `workspace`    | `route include`                       |
| Current writer  | `route`     | `exclude`   | `workspace`    | `route exclude`                       |
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
  kind with the platform form of the recorded relative target. It verifies that
  exact created form. Ownership and recovery currency comparisons ignore only
  separator differences, while evidence retains actual raw observations.
  It does not resolve the target or
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

1. Create uses the platform form of the declared relative target and link kind on an exact
   missing final leaf.
2. Delete removes the exact expected relative file link on an immediate
   no-follow match.
3. Verification inspects the final leaf without following it and requires the
   exact intended missing or platform-form `RelativeFileLink` identity.

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

`RecoveryBundleCatalogue.ReadAsync(CliWorkspace, ...)` reads only the workspace store.
`ReadStoresAsync` returns workspace then legacy observations. A location-specific
read retains that exact store identity.

Candidates, verified reads and preparations carry their immutable `RecoveryStoreLocation`.
Deletion uses that recorded location and never resolves the environment again.
Legacy deletion also requires the lease to hold the matching legacy lock.
Cleanup opens a separate frozen deletion session for each store. A store with
unavailable coverage or authority cannot prevent eligible local deletion.

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
