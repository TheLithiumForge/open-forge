---
open-forge:
  description: Define the technology-neutral inventory, planning, application, verification, and recovery behavior for `library attach`
  responsibility: Define how one complete attach plan is formed, guarded, applied, verified, or refused
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Attach, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# library attach Behavior Contract

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

## Operation Invariants

Attach performs one complete operation for one new library identity. A
conforming implementation follows this conceptual flow:

```text
validated library ID, source root, workspace, and flags
  -> complete physical source and consumer boundary facts
  -> complete eligible source inventory
  -> valid current library record and every destination collision fact
  -> identical source/destination mappings
  -> permitted generated-region projection and intended record
  -> one deterministic ordered mutation plan
  -> effect-free preflight
  -> dry-run or lease-bound application
  -> under-lock revalidation and no-follow final checks
  -> typed recovery preparation, monotonic effects, and verification
  -> record publication last and one typed result
```

No persistent effect begins until the source boundary, complete inventory,
record, every destination, generated-region boundary, intended record, ordered
plan, and preflight are complete. One invalid source or record input, duplicate,
collision, incomplete source fact, unsafe identity, or other blocker prevents
every effect. Attach never creates a safe subset and never treats source content
as destination authority.

For unchanged workspace bytes and explicit input, resolution, inventory,
mappings, generated projection, record bytes, plan, and semantic result are
deterministic. A repeated ID remains a blocked duplicate; attach does not infer
a no-op from matching source or destination bytes.

## Request And Workspace Resolution

Request resolution validates the exact command path, two required singleton
operands, the library-ID grammar, the source-root spelling, `--dry-run`, shared
global flags, and terminal behavior. It does not rescan raw arguments, infer an
ID from a path, or turn a global or write-policy flag into another operation.

The selected workspace is the exact current workspace or exact shared
`--workspace` value. Source-root resolution is relative to that selected
workspace. The resolver forms both lexical and physical path facts without
searching parent directories, substituting a Git root, or accepting an
external destination.

The source root must be strictly contained by the selected workspace in both
representations. An absolute, empty, backslash, dot-traversing, escaping, or
non-portable spelling is `invalid`; normalization never repairs an invalid
value. An alias, physical escape, or ambiguous identity is `blocked`. Each
existing component from the workspace through the source root and its `.agents`
child is checked as a real ordinary directory with no link or reparse ancestry.
The source root tree and the consumer `.agents` tree must be physically
disjoint.

The selected consumer `.agents` path is consumer-owned. Attach does not create
the Loader, route parents, or source-root controls. The consumer `.agents` root
must already be a real ordinary directory without link or reparse ancestry; it
is never created or replaced. A missing or non-ordinary mandatory source root or
`.agents` child is `invalid`. An existing source boundary or required source
fact that is unavailable or inaccessible is `incomplete`. An unsafe, aliased,
or ambiguous source identity is `blocked`; no plan is effectful in any case.

## Complete Source Inventory

After boundary resolution, attach enumerates the source root's `.agents`
subtree once as a complete narrow inventory. It records every eligible
ordinary regular file by portable source-relative `.agents/...` path and
records enough bounded exclusion and coverage facts to prove that no eligible
file was silently skipped.

The inventory excludes the source Loader, recognized entrypoints, adjacent
overwrite companions, lifecycle and library records, other manager controls,
symlinks, junctions, reparse points, and special or non-ordinary entries. The
enumerator never traverses a link or reparse point. A safely identified excluded
link does not become an eligible mapping and does not authorize following its
target.

The inventory is `incomplete` when an inaccessible directory or subtree,
enumeration failure, or unavailable required fact prevents complete coverage.
It is not silently narrowed to readable children. A missing or non-ordinary
mandatory source root or `.agents` child is `invalid`; an unsafe, aliased, or
ambiguous source boundary is `blocked`. The command does not read source bytes
to manufacture ownership and does not store, write, move, delete, or copy source
bytes.

## Record Resolution And Identity

Attach reads the exact consumer record path
`.agents/open-forge.libraries.json` as a separate ordinary record. An absent
record is a valid prior-missing state for the intended record Create. An
existing record must have exactly `schemaVersion`, `libraries`, and each exact
library member property `id`, `sourceRoot`, and `paths`; no extra property or
malformed value is accepted. A malformed record is `invalid`; an unavailable or
inaccessible existing record is `incomplete`; a duplicate ID, duplicate path,
unsafe path, or other ambiguous record identity is `blocked`.

The schema discriminator is exactly numeric `1`. The record's library IDs are
sorted by ID, and each path list is sorted by portable path spelling. The
record's `sourceRoot` is the normalized workspace-relative source-origin fact.
The record stores only eligible source-relative path strings. It stores no
expected relative target because each source and destination path is identical
and the expected target is derived from the source root and destination.

The record is consumer ownership and authorization evidence, not Framework
runtime meaning. Attach never adopts a link merely because it matches a
derived target, and it never places library data into the lifecycle record. A
record with the requested ID blocks the operation before destination planning,
even if its source root or path set appears equal.

## Mapping, Collision, And Generated Facts

The complete inventory set is converted to one identical source/destination
path set. Every destination is below consumer `.agents` and every target
relationship is relative. The expected raw link target is derived from the
source-root spelling and the destination path; source bytes are not needed for
link identity.

For each destination, current facts distinguish missing, ordinary file,
directory, relative or generic link, reparse, special, separately owned,
unsafe, and unavailable states. Only a missing destination may receive a newly
declared link. An
existing exact-looking link without a registered library record is still a
collision; attach never adopts or overwrites it. Extension-owned, lifecycle,
library-control, and other manager paths are collisions even when their bytes
look replaceable. Two intended mappings to one physical destination block the
complete plan.

Missing parent directories may be included only when they are real ordinary
directories that the declared mappings require. A parent alias, link, reparse
point, special entry, or unsafe identity blocks. Local sibling files outside
declared leaves are preserved.

If an affected destination already belongs to an existing consumer route chain,
the operation may form one bounded generated `Entries` projection according to
the [Index Behavior Contract](../../index-candidate/behavior.md). The region
must already exist in a pre-existing consumer-owned entrypoint, and authored
bytes outside the region are preserved. No source entrypoint is projected, no
missing route chain is made, and the Loader is never rewritten. If no existing
route exposes a projected leaf, no generated region is invented.

## Intended Record And Plan

Attach forms the intended schema-v1 record by inserting the new sorted library
record and its complete sorted path list. The intended record contains no
expected-link property, source bytes, absolute path, Git fact, collection,
remapping, glob, dependency, or exclusion metadata. The resulting record is
the final consumer publication and is not an early ownership marker.

The ordered plan contains only declared effects:

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
change alone does not create `attention`.

## Lease-Bound Application

For application, the complete preflight finishes before one same-workspace
lease is acquired. The lease is held through under-lock revalidation, all
effects, verification, and final record publication. Under the lease, the
operation revalidates the selected workspace, source-root set, complete
registered-record state, every destination and parent, generated regions, and
cancellation. Drift that is safely observable is reported as attention or
replanned only by a fresh invocation; unsafe mutation preconditions are
blocked.

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
bytes, and the final consumer record's exact schema, sorted IDs, sorted paths,
and complete path set. It also verifies source bytes and source topology remain
unchanged by the operation without reading source bytes into recovery state.

An application is `complete` only after all declared effects and the final
record publication verify. A dry run is `complete` when its complete plan and
preflight verify. A safe read-side drift may be surfaced as `attention`, but
an unsafe occupant or changed expected state before mutation is `blocked`.
Recovery-cleanup retention maps to the shared `attention` case; unknown
post-effect state remains `failed`.

The operation forms one typed result and does not let human or JSON rendering
rerun the operation. The Interface Contract's status precedence, stream rules,
structured envelope, and public output remain authoritative.

## Conformance Evidence

Conformance must prove, at the cheapest boundary that directly owns each fact:

- exact parser and shared-flag behavior, ID grammar and duplicate blocking;
- source-root lexical/physical containment, ordinary directory identity,
  disjoint consumer `.agents`, link/reparse ancestry, aliases, and invalid
  source forms;
- complete inventory, inaccessible-subtree `incomplete`, every exclusion,
  ordinary-file coverage, and no source-byte effect;
- identical source/destination path sets, derived relative raw targets, real
  parent directories, local sibling preservation, no copy fallback, collision
  categories, and Extension/lifecycle/library ownership;
- existing generated-region projection, marker and authored-byte preservation,
  no route or Loader creation, and consumer destination identity;
- exact schema-v1 record creation/replacement, no extra fields, sorting,
  malformed-record invalidity, duplicate-identity blocking, and
  record-publication-last ordering;
- immutable dry-run/application plan parity, no dry-run lease or recovery
  capability probe, complete preflight, one lease, under-lock revalidation,
  immediate no-follow checks, typed recovery, monotonic effects, verification,
  residual state, and no automatic rollback; and
- all shared semantic statuses, stream and JSON parity, retained recovery
  `attention`, and useful error actions.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional branches, failure injection supplied by the accepted test boundary,
and exact filesystem identities without creating additional public journeys.

## Related Current Sources

- [library attach Contract Set](_attach.md)
- [library attach Interface Contract](interface.md)
