---
open-forge:
  description: Define the technology-neutral inventory, planning, application, verification, and recovery behavior for `library attach`
  responsibility: Define how one complete attach plan is formed, guarded, applied, verified, or refused
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Attach, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# library attach Behavior Contract

A missing ownership lock is known empty. Invalid or unavailable required ownership blocks the request before any effects.

## Persistent Exclusions

Apply the [removal settings](../../remove/interface.md#keep-removed-and-restore)
to the selected Library ID and concrete consumer destination paths. An excluded
ID blocks attachment. File, directory and category exclusions omit destination
links, generated changes and new mapping claims without changing source files.
Preserve existing excluded bytes and claims. A missing excluded ancestor required
by selected content is a blocker, not permission to recreate it. Revalidate the
exact settings observation under the workspace lease. Restoration requires
explicitly clearing every relevant exclusion before attachment.

## Status And Boundary

This is the current Crystallized Behavior Contract for
`open-forge library attach`. It defines deterministic request and workspace
resolution, source-root boundary checks, complete eligible inventory, mapping
and collision facts, generated-region and record planning, preflight, dry-run,
lease-bound application, verification, typed recovery, residual truth, and
technology-neutral conformance.

The [Interface Contract](interface.md) owns public syntax, exact input grammar,
observable effects, statuses, output, examples, non-goals, and exactly three
public EndToEnd journeys. This file does not add operands, flags, aliases,
record properties, status names, output fields, or implementation callables.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define shared realization boundaries. The [Index Behavior Contract](../../index-candidate/behavior.md)
defines generated-navigation facts when an existing consumer region is eligible.
The shared [Global Flags Behavior Contract](../../shared/global-flags/behavior.md),
[Shared Result Coordinates](../../shared/result-coordinates/behavior.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their cross-command meaning.

The sole generated state publication is `.agents/open-forge.lock.json`.
The existing public record-effect and publication fields describe that lock
write. Its Libraries section contains validated registration identities and
source-relative paths; other ownership sections are preserved. No retired
record is read, written, converted, or deleted. A missing lock may be created
after the explicit attach effects verify. Invalid or unavailable required
ownership blocks the request before effects. Recovery protects the
exact prior lock bytes before any effect; the planned lock publication remains
last after verified link and generated-region effects.

## Operation Invariants

Attach performs one complete operation for one new library identity. A
conforming implementation follows this conceptual flow:

```text
validated library ID, source root, workspace, and flags
  -> complete physical source and consumer boundary facts
  -> complete eligible source inventory
  -> ownership observation, duplicate-ID fact, and every destination collision fact
  -> derived source/destination mappings
  -> permitted generated-region projection and intended record
  -> one deterministic ordered mutation plan
  -> effect-free preflight
  -> dry-run or lease-bound application
  -> under-lock revalidation and no-follow final checks
  -> typed recovery preparation, monotonic effects, and verification
  -> record publication last and one typed result
```

No persistent effect begins until the source boundary, complete inventory,
ownership observation, duplicate-ID fact, every destination, generated-region
boundary, intended record, ordered plan, and preflight are complete. A whole
readable ordinary malformed ownership lock blocks the request, even with explicit
source and destination inputs. An unreadable, aliased, nonordinary, or otherwise
unavailable publication also prevents every effect. Any invalid source or record
input, duplicate, collision, incomplete source fact,
unsafe identity, or other blocker prevents every effect. Attach never creates a
safe subset and never treats source content, matching bytes, matching links, or
legacy records as destination authority.

For unchanged workspace bytes and explicit input, resolution, inventory,
mappings, generated projection, record bytes, plan, and semantic result are
deterministic. A repeated ID remains a blocked duplicate; attach does not infer
a no-op from matching source or destination bytes.

## Request And Workspace Resolution

Resolve the workspace and global flags through the shared operation boundary.
Require one Library ID and one source-root operand. Parse the single optional
`--to` through its distinct destination-root grammar; omission and explicit `.`
select the workspace root. Resolve all relative coordinates against that selected
workspace and retain the normalized mapping in the request and record.

The source root is a non-empty canonical portable workspace-relative directory,
strictly contained by the selected workspace lexically and physically. It has
no absolute, empty, backslash, `.` or `..` segment and no portable alias. Every
ancestor and the selected root must be a real ordinary directory, without
symlink, junction or reparse ancestry. No specially named child is required.
The selected directory itself scopes the recursively discovered eligible files.

An absent or non-directory source root is `invalid-input` for Attach. For an existing
registration, unavailable or missing source facts make Inspect or Sync
`incomplete`; a readable non-directory root is `invalid-input`. Unsafe containment,
linked ancestry or ambiguous identity is `blocked`. List reports only bounded
root availability and does not enumerate descendants. An incomplete source is
never an empty source inventory.

The consumer workspace and its existing ordinary `.agents` control directory
remain consumer-owned. The operation does not create or replace either root.
The destination root may be an ancestor of a contained source root, including
`.`. Actual destination leaves and every mutation target must remain outside
all selected and registered source trees. This per-leaf check preserves source
contents without forbidding workspace-root projection.

## Complete Source Inventory

Recursively enumerate the selected real source root, recording each eligible
ordinary file by its canonical portable path relative to that root. A source
containing no `.agents` or `content` child is valid. Empty eligible inventory is
complete when the entire selected tree was safely observed.

Exclude Git metadata at any path segment, known manager controls, symlinks,
junctions, reparse points and special entries. Recognize Open Forge Loader,
entrypoint and overwrite controls at their original `.agents` source coordinates
before remapping. A remap cannot make those controls eligible. External
`_name.md`, `*.overwrite.md` and README remain ordinary opaque Library content. Inspect excluded entries without
following them and never descend into excluded metadata or linked directories.
Existing source classification and protected-control rules remain applicable.

An inaccessible directory, enumeration failure, unavailable eligible ordinary
file or unsafe required boundary prevents complete inventory. Retain known safe
facts as partial evidence, never as permission to delete retired links. Source
bytes are never copied, rewritten or deleted. Only eligible leaf membership and
physical path facts feed projection planning.

## Record Resolution And Identity

Library selection reads the `libraries` claims in `.agents/open-forge.lock.json`.
The shared ownership codec accepts understood keys without requiring an exact
schema version or member set. It never reads the old Library or lifecycle file
for selection. Each usable Library claim supplies `id`, `sourceRoot`,
`destinationRoot`, and source-relative `paths`. IDs and paths are presented in
ordinal order; typed portable identities and unambiguous mapped destinations
remain required before using a claim. The destination root may be `.`; a source
root may not. Link identity derives from the two roots and each source suffix.
Permissions remain separate from ownership.

A missing lock is known empty. Attach may form a new claim from its explicit
source and destination after complete source inventory, destination and
ancestor checks, alias checks, permission admission and all other safety checks.
Invalid or unavailable required ownership blocks the request before effects.
A missing-lock observation may remain present on successful attachment.
Matching files, matching links and legacy records create no claims.

An already registered ID blocks Attach even when all other facts match.

## Mapping, Collision, And Generated Facts

Each record keeps `sourceRoot`, `destinationRoot` and source-relative `paths`.
For a path `p`, its source is `sourceRoot/p`. Its consumer destination is `p`
when `destinationRoot` is `.`, otherwise `destinationRoot/p`. Derive the exact
raw relative file-link target from the destination parent to that source.
Root-level leaf destinations use the workspace root as their parent.

Projection creates individual relative file symlinks. Required missing parents
are separate real ordinary directory effects, including first-level parents.
Existing parents must be real ordinary directories with no linked or reparse
ancestry. Local sibling files remain untouched; no directory symlink, copied
file fallback or directory ownership is introduced.

Validate source eligibility and final destination protection separately. Protect
Git metadata, Framework and recognized manager controls, `.agents` Loader,
entrypoint and overwrite controls, authored settings and generated ownership controls, recovery and temporary
storage, and every selected or registered Library source tree. A grant covering
a containing directory never overrides these leaf checks. Compare portable
identity and physical containment. Different source-relative paths and different
Libraries may share ordinary directories but never the same destination leaf.
An unregistered link, including an exact-looking link, is an existing occupant
and is never adopted.

Only mapped `.agents/**` leaves may participate in an existing consumer route
chain and its bounded generated `Entries` projection under the Index contract.
The region and route chain must already exist and authored bytes remain intact.
External Markdown remains opaque content. No source entrypoint, Loader, missing
route or generated region is created.

## Intended Record And Plan

Attach forms the intended versioned record by inserting the new sorted library
record and its complete sorted path list. For a valid readable ownership record,
other claims are preserved. An absent lock starts with no claims; an invalid or
unavailable required lock prevents planning a publication. The intended record contains no
expected-link property, source bytes, absolute path, Git fact, collection,
per-file remapping, glob, dependency, or exclusion metadata. The resulting record is
the final consumer publication and is not an early ownership marker.

The ordered plan contains only declared effects:

- the approved consumer permission create/replace, verified before all other effects;
- real ordinary parent-directory creation required by declared mappings;
- relative-file-link creation at each missing destination;
- bounded replacement of each permitted pre-existing generated region; and
- ordinary consumer-record creation or replacement as the last publication
  effect.

The command resolves every effect's expected state and verification condition
before preflight. It does not include a source-file copy, source mutation,
Loader rewrite, route creation, broad directory adoption, or unselected
consumer path.

## Preflight And Dry Run

Effect-free preflight validates the complete ordered plan, duplicate logical
targets, prospective physical aliases, destination containment, missing-parent
directory facts, generated-region expectations, record expectations, and
cancellation. It also retains the complete source inventory and every
collision result. A non-empty plan with one unsafe target is rejected as a
whole.

`--dry-run` uses this same immutable request, current facts, plan, and preflight.
It reports all links, parent directories, generated regions, record bytes, and
blockers, then stops before acquiring a workspace lease or probing recovery
capability. It performs no filesystem or application-data effect. A planned
change alone does not create `completed-with-warnings`. Missing ownership is
known empty. Invalid or unavailable required ownership remains a blocker in the
typed preview, just as it does during application.

## Consumer Permission

Explicit non-dry-run `--allow-path` edits the shared authored settings after safe
planning and before admission; failure is reported and stops content application.
It is a separate authored edit and remains if later content fails. Interactive
always approval declares a settings effect covered by the operation's recovery
bundle. Once approves only this operation and writes no settings; cancel applies
nothing. The shared contract owns the exact reader, authoring and receipt rules.

Derive required external leaves from the complete eligible mapped inventory, using shared destination admission. The [Interface](interface.md#consumer-permission)
selects scope proposals and the [Workspace Permissions Behavior](../../shared/workspace-permissions/behavior.md)
defines exact admission, future-descendant approval, shared destination grants, revocation,
prompt grammar and receipt truth.

Complete source/record, ownership, mapping and structural preflight before the
question. Preserve immutable required/missing leaves, proposed/approved scopes
without owner or source-bound grant coordinates. Missing or declined permission blocks the whole
request; dry-run writes nothing, and noninteractive execution authors grants only through explicit `--allow-path`.

Under the same workspace lease, compare exact settings bytes or prior absence
and every other volatile plan fact. Drift invalidates approval without merging
new grants. Prepare one bundle including prior settings bytes or absence, then
verify its ordinary create/replace before directories, links or generated effects.
Publish the Library record last. Failure preserves actual verified permission
outcome and residual evidence. Library repair never applies the permission entry;
explicit recovery checks current grants without widening or restoring them.

## Lease-Bound Application

For application, the complete preflight finishes before one same-workspace
lease is acquired. The lease is held through under-lock revalidation, all
effects, verification, and final record publication. Under the lease, the
operation revalidates the selected workspace, source-root set, complete
registered-record state, every destination and parent, generated regions, and
cancellation. Drift that is safely observable is reported as
`completed-with-warnings` or replanned only by a fresh invocation; unsafe
mutation preconditions are blocked.

Immediately before each effect, the operation performs a final no-follow
check of the target component and repeats the matching expected-state check.
It creates only declared real ordinary directories and exact relative file
symlinks. It never resolves a link by reading source bytes, follows a source
target to write it, or falls back to copying.

The operation's effect order is deterministic. Parent directories and link
creates establish declared projection leaves, permitted generated regions are
updated within their existing bounds, and the record is published last. A
record write cannot authorize a link effect retroactively, and a link effect
cannot authorize a record path not present in the intended sorted record.

## Typed Recovery And Monotonic Effects

For an effectful application, typed recovery preparation is complete and
verified before the first effect under the shared mutation boundary; an
effect-free plan has no recovery bundle. Library recovery distinguishes:

- a prior-missing ordinary consumer-record `Create`;
- a relative-file-link `Create` at a previously missing destination; and
- a relative-file-link `Delete` with its exact raw relative target.

It stores consumer-side record bytes and link identity, never source bytes. A
strong no-follow recovery operation can remove a link created by this attach
only when the destination is still the exact created link. It can recreate an
exact deleted relative link only when the destination is safely missing and the
recorded raw target is the same, including when that target is dangling. The
operation does not automatically invoke recovery, roll back, compensate, or
claim that a source changed.

If a known application, verification, cancellation, or record-publication
failure occurs after effects begin, no new effects are started. Previously
verified links, directories, generated regions, and any record state remain
actual residual state. The prior record remains authoritative when final
publication did not verify. The result reports the exact residual paths and
retained recovery evidence under the shared design.

## Verification And Result Formation

Verification checks every created parent directory, every relative-file link's
no-follow kind and exact raw target, every generated region's intended bounded
bytes, and the planned lock's exact bytes, sorted IDs, sorted paths,
and complete path set. It also verifies source bytes and source topology remain
unchanged by the operation without reading source bytes into recovery state.

An application is `completed` only after all declared effects and any planned
lock publication verify. A dry run is `completed` when its complete plan and
preflight verify. A safe read-side drift may be surfaced as `completed-with-warnings`, but
an unsafe occupant or changed expected state before mutation is `blocked`.
Recovery-cleanup retention maps to the shared `completed-with-warnings` case; unknown
post-effect state remains `failed`.

The operation forms one typed result and does not let human or JSON rendering
rerun the operation. The Interface Contract's status precedence, stream rules,
structured envelope, and public output remain authoritative.

## Conformance Evidence

Conformance must prove, at the cheapest boundary that directly owns each fact:

- exact parser and shared-flag behavior, ID grammar and duplicate blocking;
- source-root lexical/physical containment, ordinary directory identity,
  protected source trees, link/reparse ancestry, aliases, and invalid
  source forms;
- complete inventory, inaccessible-subtree `incomplete`, every exclusion,
  ordinary-file coverage, and no source-byte effect;
- derived source/destination path sets, derived relative raw targets, real
  parent directories, local sibling preservation, no copy fallback, collision
  categories, and Extension/lifecycle/library ownership;
- existing generated-region projection, heading and authored-byte preservation,
  no route or Loader creation, and consumer destination identity;
- normalized lock creation/replacement, no extra fields, sorting,
  known-empty missing ownership, no effects for malformed or unavailable required
  ownership or an unreadable/aliased/nonordinary publication, registered-ID collision blocking,
  no adoption from matching links or legacy records, and record-publication-last
  ordering;
- immutable dry-run/application plan parity, no dry-run lease or recovery
  capability probe, complete preflight, one lease, under-lock revalidation,
  immediate no-follow checks, typed recovery, monotonic effects, verification,
  residual state, and no automatic rollback; and
- all shared semantic statuses, stream and JSON parity, retained recovery
  `completed-with-warnings`, and useful error actions.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional branches, failure injection supplied by the accepted test boundary,
and exact filesystem identities without creating additional public journeys.

## Related Current Sources

- [library attach Contract Set](_attach.md)
- [library attach Interface Contract](interface.md)
