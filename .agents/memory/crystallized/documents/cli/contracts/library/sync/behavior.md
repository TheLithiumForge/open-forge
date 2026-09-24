---
open-forge:
  description: Define the technology-neutral inventory, reconciliation, planning, application, verification, and recovery behavior for `library sync`
  responsibility: Define how one complete sync plan is formed, guarded, applied, verified, or refused
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Sync, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# library sync Behavior Contract

A missing ownership lock is known empty. Invalid or unavailable required ownership blocks the request before any effects.

## Persistent Exclusions

Apply the [removal settings](../../remove/interface.md#keep-removed-and-restore)
to the selected Library ID and each concrete consumer destination. An excluded
ID blocks synchronization. Excluded paths are not restored, changed, pruned or
newly claimed; existing excluded bytes and claims remain intact. In particular,
removing one owned link through root `remove` releases only that mapping and
records its destination, so synchronization leaves it absent while other links
continue to update. Do not recreate required missing excluded ancestors.
Revalidate the exact settings observation under the workspace lease.

## Status And Boundary

This is the current Crystallized Behavior Contract for
`open-forge library sync`. It defines deterministic request and workspace
resolution, lock ownership resolution, source-root boundary checks, complete
eligible inventory, current-versus-registered reconciliation, collision facts,
generated-region and record planning, preflight, dry-run, lease-bound
application, verification, typed recovery, residual truth, and
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
record is read, written, converted, or deleted. A planned lock publication is
required for successful completion. If publication fails after earlier effects,
the result reports the partial state and retains recovery evidence.
Recovery protects the exact prior lock bytes before any effects; the planned
lock publication remains last after verified link and generated-region effects.

## Operation Invariants

Sync performs one complete reconciliation for one registered library identity.
A conforming implementation follows this conceptual flow:

```text
validated library ID, workspace, record, and flags
  -> exact selected record and recorded source root
  -> complete current eligible source inventory
  -> current-versus-registered path sets
  -> every destination, parent, raw-target, and generated-region fact
  -> one deterministic ordered reconciliation plan
  -> effect-free preflight
  -> dry-run or lease-bound application
  -> under-lock revalidation and no-follow final checks
  -> typed recovery preparation, monotonic effects, and verification
  -> record publication last and one typed result
```

No persistent effect begins until the selected record, source-root boundary,
complete inventory, every set relationship, every destination collision,
generated-region boundary, intended record, ordered plan, and preflight are
complete. A changed ordinary destination is retainable only when it is the
current destination of a registered mapping. If independent safe mapping
effects exist, the retained mapping contributes no effect and the plan may
apply exactly those independent effects with an `incomplete` result. If no
independent safe effect exists, the changed mapping remains `blocked`. Sync
never applies additions or unaffected retirements after another unsafe or
unavailable mapping fact, and never uses a partial source inventory to
authorize deletion.

For unchanged workspace bytes and explicit input, resolution, inventory, set
reconciliation, generated projection, record bytes, plan, and semantic result
are deterministic. An unchanged complete set and exact projection is a
verified no-op. A missing current link is safe drift that can be recreated
after complete preflight with an Attention2 result. A changed ordinary
occupant is retainable only for a current registered mapping with independent
safe effects; otherwise it remains unsafe mutation drift and blocks.

## Request, Workspace, And Record Resolution

Request resolution validates the exact command path, one required singleton
library ID, the library-ID grammar, `--dry-run`, shared global flags, and
terminal behavior. It rejects a source-root operand rather than letting an
extra value override the record. It does not rescan raw arguments or infer a
library from a path, basename, or route.

The selected workspace is the exact current workspace or exact shared
`--workspace` value. Sync reads the exact consumer record path
`.agents/open-forge.lock.json`; it does not search a parent workspace,
substitute a Git root, or consult a legacy record. The common lock reader
provides usable claims or a complete ownership observation with no selected
Library and no effects. A requested ID absent from a readable lock is invalid.
Typed IDs, roots, source-relative paths, and unambiguous destinations remain
required before selection.

The record's `sourceRoot` is the source-origin fact and each `paths` item is a
canonical portable source-relative leaf path. Map that suffix below the required
`destinationRoot` to derive its workspace-relative destination.
The record contains no expected relative-link text; Sync derives it from
`sourceRoot`, `destinationRoot` and the source suffix. It contains no source bytes, timestamp,
Git revision, dependency, glob, per-file remapping, collection, exclusion, or source
permission metadata.

## Source Boundary And Complete Inventory

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
facts as partial evidence, never as permission to delete retired links. The
result is `incomplete` (exit 3) with no effects and says the source is
unavailable; it never treats the incomplete observation as an empty source.
Source bytes are never copied, rewritten or deleted. Only eligible leaf
membership and physical path facts feed projection planning.

## Set Reconciliation

Let `P` be the complete current eligible source path set and `R` be the
selected library's registered `paths` set. Both sets use canonical source-relative strings. The recorded destination root
derives all consumer leaves. Sync forms `P ∩ R`, `P \\ R`, and `R \\ P` in
deterministic portable-path order before planning effects.

For `P ∩ R`, the source remains eligible and registered. An exact registered
relative file symlink with the derived raw target is preserved. A missing leaf
with safe parent facts receives a link Create effect and an
`Attention`/`registered-link-restored` finding; the same warning remains in a
dry-run with no write. An ordinary file is retainable only when it is the
current destination of this registered mapping. Its bytes receive no effect.
If another independent safe mapping effect exists, planning continues and the
result is `incomplete` with the retained destination named; if none exists,
planning returns the prior `blocked` refusal. A directory, different link,
special entry, unsafe path, unknown state, or separately owned occupant blocks
the whole request.

For `P \\ R`, the source is newly eligible and has no registered mapping. Its
destination leaf must be exactly missing and its required parents must be real
ordinary directories or safely creatable below the consumer workspace. Sync
creates one exact relative file symlink and adds the path to the intended
record. Any occupant, including an unregistered link with the derived target,
is a collision and blocks the whole request. Sync never adopts a matching
unregistered link.

For `R \\ P`, complete inventory proves that the source path is no longer an
eligible source file. If the destination is an exact registered relative file
symlink whose raw target equals the derived target, Sync plans one link Delete
and removes the path from the intended record. The target is not resolved, so
an exact dangling registered link is eligible for deletion. If the destination
leaf is positively missing, even when its no-follow parent facts are safe, the
registered link cannot be proven and the whole request blocks. Any ordinary
file, directory, different link, special entry, unsafe parent, unknown state,
or separately owned occupant also blocks the whole request.

The complete inventory gate applies before every `R \\ P` decision. A source
file becoming a recognized entrypoint, overwrite companion, lifecycle or
manager control, link, or special entry is absent from `P` and follows the
same retirement proof. A source byte change alone does not alter `P` or create
a replacement effect because the projection remains a live link.

Sync never sweeps paths outside `R` or `P`, removes local sibling files, or
interprets generated navigation as source inventory. A safely observed missing
current link is complete repairable drift and returns `completed-with-warnings`
(Attention2) after application, or the same warning with no writes in a
dry-run. A changed ordinary current mapping is a retained incomplete result
only when independent safe effects exist; all other changed, unsafe, or
unavailable mappings remain `blocked` or incomplete with no effects as their
facts require. No continuation rule adopts or replaces an unregistered path.

## Destination And Parent Facts

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

## Generated Navigation And Intended Record

When `P` adds or retires a projected path in an already established consumer
route, Sync may form a bounded generated `Entries` projection only in the
pre-existing consumer-owned entrypoint and only under the [Index Behavior Contract](../../index-candidate/behavior.md).
The route chain and generated region must already exist. Authored bytes outside
the region remain unchanged.

Sync never projects a source entrypoint, creates a route parent, materializes a
missing route chain, rewrites authored entrypoints, rewrites the Loader, or
invents Framework semantics. If an affected existing generated region is
missing, ambiguous, changed, or unsafe, the complete plan is blocked or
incomplete as applicable. If no existing route exposes a changed path, no
generated region is fabricated.

The intended versioned record replaces the selected library's `paths` with the
sorted current eligible set `P`, keeps its recorded `sourceRoot`, and retains
all other libraries in sorted ID order. It has exactly the accepted properties
and no expected-link, source-byte, Git, collection, per-file remapping, glob,
dependency, or exclusion field. The record is not published until every link
and generated effect verifies.

## Complete Plan And Preflight

The ordered plan contains only:

- an explicitly approved permission create/replace before link deletion;
- real ordinary parent-directory Create effects required by new or missing
  links;
- relative-file-link Create effects for exact missing destinations;
- relative-file-link Delete effects for retired destinations whose registered
  raw target is exact;
- permitted bounded generated-region replacements; and
- one ordinary consumer-record Replace effect when the sorted record changes.

A complete no-op has no effects and does not manufacture a record write. Every
retired path requires an exact registered relative link and its raw target;
positive absence at a retired destination is a blocking fact, not record
evidence for a cleanup-only update. Every effect has an expected state and
verification condition.
The plan contains no source-file effect, copy fallback, broad directory
adoption, unregistered-link adoption, Loader rewrite, route creation, or
unselected path.

Effect-free preflight validates the complete source coverage, set formation,
destination and parent containment, duplicate logical targets, prospective
physical aliases, exact missing or registered raw-target expectations,
generated-region boundaries, record expectation, and cancellation. A single
incomplete source fact rejects the complete plan before any effect. A changed
ordinary destination on a current registered mapping is recorded as retained,
with no effect for that leaf, only when at least one independent safe mapping
effect remains. If none remains, the changed mapping is the existing blocked
refusal. Other changed, unsafe, or unavailable mappings reject all effects.

`--dry-run` uses the same immutable request, record and source facts, complete
inventory, set reconciliation, collision checks, generated projection,
intended record, ordered plan, and preflight as application. It reports every
create, exact retirement, generated effect, record
effect, and blocker, then stops before acquiring a lease or probing recovery
capability. It creates no directory, link, generated navigation, record,
recovery artifact, or other persistent state. Planned changes alone do not
create `completed-with-warnings`; a registered-link-restored finding remains an
Attention2 warning in the preview, and a retained changed destination remains
an Incomplete3 result naming that path. No dry-run warning is converted into a
truthless success headline.

## Consumer Permission

Explicit non-dry-run `--allow-path` edits the shared authored settings after safe
planning and before admission; failure is reported and stops content application.
It is a separate authored edit and remains if later content fails. Interactive
always approval declares a settings effect covered by the operation's recovery
bundle. Once approves only this operation and writes no settings; cancel applies
nothing. The shared contract owns the exact reader, authoring and receipt rules.

Derive required external leaves from the union of complete current mapped inventory and registered destinations, including unchanged links and retirements, using shared destination admission. The [Interface](interface.md#consumer-permission)
selects scope proposals and the [Workspace Permissions Behavior](../../shared/workspace-permissions/behavior.md)
defines exact admission, future-descendant approval, shared destination grants, revocation,
prompt grammar and receipt truth.

Complete source/record, ownership, mapping and structural preflight before the
question. Preserve immutable required/missing leaves, proposed/approved scopes
without owner or source-bound grant coordinates. Missing or declined permission blocks the whole
request; a retained changed ordinary destination does not waive its required
permission or ownership checks. Dry-run writes nothing, and noninteractive
execution authors grants only through explicit `--allow-path`.

Under the same workspace lease, compare exact settings bytes or prior absence
and every other volatile plan fact. Drift invalidates approval without merging
new grants. Prepare one bundle including prior settings bytes or absence, then
verify its ordinary create/replace before directories, links or generated effects.
Publish the Library record last. Failure preserves actual verified permission
outcome and residual evidence. Library repair never applies the permission entry;
explicit recovery checks current grants without widening or restoring them.

## Lease-Bound Application

For application, complete inventory and all collision checks finish before one
same-workspace lease is acquired for the effectful plan. Under that lease Sync
revalidates the record, source-root boundary, complete inventory, `P` and `R`,
every destination and parent, every expected raw target, generated regions,
and the expected record.

Immediately before each effect, it performs an immediate no-follow
final-component check and repeats the matching expected-state validation. It
creates only exact relative file symlinks at missing leaves and removes only
exact registered raw-target links. A retained changed ordinary destination is
revalidated as preserved ordinary bytes and receives no replacement, adoption,
or deletion effect. It never resolves a link to delete its target, writes no
source bytes, and never falls back to copying.

Effects are monotonic and deterministic. Parent directories and link effects
are applied and verified before permitted generated-region replacements. The
sorted consumer record is published last. A retained changed ordinary mapping
does not block independent effects already admitted by the complete plan, but
any fresh unsafe or unavailable drift in an effect target blocks the remaining
mutation. It never creates a general continuation bypass for another mapping.

## Typed Recovery And Residual Truth

For an effectful application, typed recovery is prepared and verified before
the first effect under the shared mutation boundary; an effect-free plan has no
recovery bundle. Library recovery distinguishes:

- a prior-missing ordinary consumer-record `Create`;
- a relative-file-link `Create` at an exact missing destination; and
- a relative-file-link `Delete` carrying the exact registered raw relative
  target, including a target that is dangling.

It stores consumer record and permission bytes and raw link identity only. It never stores,
opens, follows, restores, or deletes source bytes. Strong no-follow recovery
can remove an exact created link when the destination is still that created
link, or recreate an exact deleted link when the destination is safely missing
and the recorded raw target is unchanged. Recreated links may be dangling;
recovery does not resolve their source target.

A retained changed ordinary destination has no link or recovery effect. Its
ordinary bytes remain outside the recovery bundle and are never restored,
replaced, or removed by Sync.

Sync never automatically invokes recovery, rolls back, compensates, or claims
that a source was deleted from incomplete evidence. If application,
verification, cancellation, or final record publication fails after an effect,
no new effect begins. Already verified links, generated regions, and record
state remain residual truth. If the final record did not verify, its prior
state remains the observed record. Exact residual paths and retained recovery
facts are reported for a fresh explicit plan.

## Verification And Result Formation

Verification checks every created parent directory, every created link's
no-follow kind and exact raw target, every retired link's positive absence,
every generated region's bounded intended bytes, and the final record's exact
current record properties, sorted IDs, sorted paths, and complete current path set.
It also verifies source paths and bytes remain unchanged.

An unchanged complete no-op is `completed` only after the complete inventory,
exact registered-link facts, record, and generated boundaries prove no effect
is required. A missing current registered link that is recreated is
`completed-with-warnings` (Attention2) after verification; dry-run retains the
same warning and writes nothing. A changed ordinary current registered mapping
with independent safe effects is `incomplete` (Incomplete3), with its retained
destination named and only the safe effects counted. The same changed mapping
without an independent safe effect remains `blocked`. A source inventory that
is incomplete is Incomplete3 with no effects and says the source is unavailable.
A successful application is `completed` only after all effects and record
publication verify. Recovery-cleanup retention maps to shared
`completed-with-warnings`; unknown post-effect state remains `failed`.

The operation forms one typed result and does not let human or JSON rendering
rerun resolution, inventory, planning, application, verification, or retained
state formation. The Interface Contract's status precedence, stream rules,
structured envelope, and public output remain authoritative.

## Conformance Evidence

Conformance must prove, at the cheapest boundary that directly owns each fact:

- exact parser and shared-flag behavior, singleton ID, no source-root operand,
  terminal modes, and unknown-ID invalidity;
- forgiving lock reads and registration properties, sorting, duplicates, malformed-record
  preservation, source-root/path validation, and namespace separation from
  automatic source IDs;
- strict source-root lexical and physical containment, ordinary source/destination
  directories, no-link/reparse ancestry, aliases, per-effect source-tree
  exclusion, complete inventory, inaccessible-subtree `incomplete`, and
  every excluded entry;
- complete-before-retirement set formation, `P ∩ R`, `P \\ R`, and `R \\ P`
  effects, missing-current-link restoration with Attention2, exact raw-target
  retirement including dangling links, missing-retired-link blocking,
  changed-ordinary-current-mapping retention with independent safe effects and
  Incomplete3 naming, no-independent-work blocking, other unsafe/unavailable
  no-effect boundaries, local-sibling preservation, and no adoption;
- destination containment, real parent-directory support, relative-file-link
  identity, no copy fallback, Extension/lifecycle/library ownership, and
  two-library collisions;
- existing generated-region projection and authored-byte preservation, no route
  creation, no Loader rewrite, and pre-dogfood link-aware guards for Route
  Update, Index, Route Move, and Route Remove;
- immutable dry-run/application parity, no dry-run lease or recovery
  capability probe, complete preflight, one lease, under-lock set/state
  revalidation, immediate no-follow checks, typed recovery, monotonic effects,
  record-last publication, verification, residual truth, and no automatic
  rollback; and
- complete no-op, Attention2/`completed-with-warnings`, Incomplete3 with
  retained destination or source-unavailable truth, Invalid4 for noninteractive
  confirmation without `--automatic`, `blocked`, `failed`, and `cancelled`
  result mapping with human/JSON parity and useful actions.

The three public EndToEnd journeys are defined only by the [Interface
Contract](interface.md#endtoend-journeys). Lower-tier evidence may exercise
additional source, set, collision, retirement, failure, and residual branches
without creating additional public journeys.

## Related Current Sources

- [library sync Contract Set](_sync.md)
- [library sync Interface Contract](interface.md)
