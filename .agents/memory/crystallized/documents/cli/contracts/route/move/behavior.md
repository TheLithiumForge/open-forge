---
open-forge:
  description: Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route move`
  responsibility: Define how `route move` forms one complete leaf or category plan, preserves references, projects navigation, and recovers safely
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Move, Behavior, Mutation, Reference, Safety, Recovery, CurrentTruth]
---

# route move Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route move`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The [Interface Contract](interface.md) defines the complete public syntax,
subject boundary, destination meaning, observable output, semantic results,
errors, examples, non-goals, and public verification. This file defines how a
conforming implementation resolves current facts, forms one plan, applies and
verifies its effects, recovers from handled failure, and produces that result.
It does not add flags, operands, aliases, output fields, semantic statuses, or
implementation technology.

The shared [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md),
[CLI Source References Behavior Contract](../../shared/source-references/behavior.md),
and [Index Behavior Contract](../../index/behavior.md) remain
authoritative at their own scopes. The current Framework routing, Markdown,
overwrite, and lifecycle sources define the meaning consumed by this operation.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
result schema, process-status mapping, parser and serializer dependencies,
filesystem and physical-identity boundary, workspace lock, recovery boundary,
libraries, test evidence, Native AOT, and source-layout choices. This behavior
remains technology-neutral within those accepted boundaries.

## Operation Invariants

`route move` performs one complete structural move for one eligible ordinary
unmanaged logical leaf or one eligible ordinary unmanaged category. For
unchanged workspace bytes and explicit input, it resolves the same subject and
destination, forms the same complete reference and generated-navigation plan,
and produces the same semantic result.

The operation satisfies these invariants:

- A leaf is one ordinary routed Markdown base plus its valid adjacent overwrite,
  when present. The pair is one logical source and one move effect.
- A category is the complete physically contained folder tree rooted at one
  recognized entrypoint. Every contained regular file and directory—including
  routed or unrouted Markdown, native or binary resources, support files, and
  overwrite companions—is part of one category plan.
- Every category item passes complete containment, identity, ownership,
  lifecycle, collision, and recovery classification before the plan is complete.
- Positive trusted unmanaged proof covers every selected logical source or
  resource. Absence of one receipt never substitutes for a complete inventory.
- The complete supported-workspace-Markdown reference pass covers sources inside
  and outside `.agents`. No supported potentially affected reference is silently
  left stale.
- Generated `Entries` are projected from intended topology and authored metadata.
  Current generated lines do not establish identity or authored reference
  authority.
- No persistent effect begins until the subject, destination, ownership proof,
  reference coverage, generated projection, complete plan, and preflight are
  complete.
- A category is one recovery and verification boundary, never a sequence of
  independently committed leaf operations.
- A repeated move using the consumed old source forms exact source-not-found
  `invalid` evidence. It does not manufacture a verified no-op from the absence.

## Request Resolution

### Command and workspace

Request resolution applies the shared terminal rules before domain work. It
validates exactly one source reference, one destination target, the two Boolean
write-policy flags, and the shared global flags against the [Interface
Contract](interface.md#syntax).

The selected workspace is the exact current working directory or the exact
`--workspace <path>` value under the shared [Global CLI Flags Behavior
Contract](../../shared/global-flags/behavior.md). Resolution does not search
parent directories, substitute a Git root, infer a workspace from an operand,
or use a nearby `.agents` directory.

`--dry-run` and `--skip-git-check` normalize to one idempotent Boolean choice
each. No command-specific input has last-wins or precedence behavior. Shared
global flags retain their own repetition, composition, and terminal rules.

Invalid command input stops before source resolution. JSON and other
non-interactive modes never prompt for a collision or missing semantic subject.

### Source reference classification

The resolver passes the source operand through the shared source-reference
classification without guessing. A `.agents/` or `./.agents/` prefix requests
an exact path. Every other value requests an automatic source ID. The resolver
preserves the requested form and records the resolved automatic ID and canonical
workspace-relative path.

An unresolved ID collision is `blocked` in non-interactive and JSON use and
retains every candidate path. An exact path can select one physical candidate,
but it cannot repair an ambiguous route, overwrite pair, or category inventory.

The Loader and the `.agents` workspace root are not valid subjects. A source
reference naming an overwrite resolves to its base logical source only when the
base/overwrite pair is complete and unambiguous.

### Subject classification

After shared identity resolution, classify the selected subject exactly once:

1. An ordinary routed Markdown base source is a leaf candidate.
2. A recognized entrypoint is a category candidate and establishes the category
   root folder.
3. A native source, non-Markdown resource, Loader, workspace root, unsupported
   source, orphan overwrite, or ambiguous pair is not a leaf or category subject.

Classification does not use generated entries, tags, filenames that merely look
like categories, route proximity, or a recommendation as ownership or subject
proof.

## Current Facts And Coverage

Before planning, the operation establishes the complete facts required by the
selected subject kind.

### Leaf facts

For a leaf, current facts include:

- requested and resolved source identity, base path, and valid overwrite layer;
- ordinary routed Markdown kind, exposing parent, route identity, and
  compatibility representation;
- lexical and physical containment and exact physical identity;
- destination target kind, parent entrypoint, occupancy, alias, collision, and
  containment facts;
- complete trusted lifecycle-ownership inventory and the proof that neither the
  base nor overwrite is claimed;
- supported-workspace-Markdown catalogue coverage for the complete reference
  pass;
- old and new exposing parent and Loader projection facts when applicable;
- expected bytes, generated boundaries, affected-path Git state, recovery
  readiness, and volatile expected-state facts.

### Category facts

For a category, current facts include all leaf facts generalized to every item in
the physically contained tree, plus:

- root entrypoint and overwrite identity;
- every contained directory and file path, source kind, logical identity, and
  relative path below the category root;
- every descendant entrypoint, routed or unrouted Markdown source, native or
  binary resource, ordinary support file, base/overwrite pair, and other regular
  contained item;
- complete containment and physical-identity proof for each item;
- lifecycle and positive unmanaged proof for each selected logical source or
  resource;
- destination occupancy and collision facts for the complete preserved relative
  layout; and
- generated boundaries and direct-child projections for the affected old and new
  route surfaces.

The category inventory is complete only when every physically contained item has
been inspected. An item that is neither a regular file nor a directory, or that
is unreadable, externally resolving, ambiguous, or unsafe, blocks; an unfamiliar
extension does not exclude an otherwise safe regular file. The resolver does not
list a safe prefix and call the category complete.

### Ownership inventory

The resolver loads one complete trusted lifecycle-ownership inventory containing
the Framework baseline and every applicable Extension receipt or manager claim.
It establishes that no selected logical source or resource is claimed. Missing,
malformed, conflicting, stale, or incomplete inventory forms the public blocked
ownership boundary. A path, route, tag, generated line, matching bytes, missing
receipt, or prior result cannot complete this proof.

### Reference coverage

The reference resolver enumerates every supported Markdown source physically
contained by the selected workspace, including sources outside `.agents`, and
reads every applicable base and overwrite layer. It records complete coverage
before it forms a reference effect. Generated `Entries` interiors are excluded
from authored-reference authority and supplied by the generated projection.

If the catalogue cannot be completely enumerated or a supported source cannot be
inspected, the operation forms `incomplete` and no write begins. An unsafe
containment or physical-identity boundary forms `blocked`.

## Category Inventory And Identity

The category resolver treats the root entrypoint as the category's selected
identity and recursively inventories its complete physical folder tree. It does
not use current generated entries to decide membership. A descendant is included
because it is physically contained and admitted by the selected category
contract, not because a generated line names it.

For each contained item, the resolver establishes:

1. lexical and physical containment below the selected category root;
2. one stable physical identity without unsafe aliases;
3. one source or resource classification;
4. lifecycle ownership and the positive unmanaged result;
5. destination collision and preserved-relative-path facts; and
6. expected-state and recovery evidence for any planned effect.

A failure at any item prevents the category plan. The resolver does not promote a
contained item to an independent command, omit it because it is not routed, or
make its generated region an authored source.

The intended destination maps each category-relative path to the same relative
path below the new destination root. It does not flatten or reorder the tree.
The category's internal relative reference relationships remain valid when their
source and target move together without a relative-layout change.

## Destination Resolution

The destination resolver applies the command-specific target meaning from the
[Interface Contract](interface.md#destination-target):

- A leaf destination is one exact ordinary routed Markdown path under one
  existing valid parent route.
- A category destination is one exact recognized entrypoint path inside a new
  category folder whose parent is one existing valid route. That path names the
  new category root and its entrypoint; the rest of the category layout is
  derived from the source-relative paths.

The resolver rejects a missing parent route, implicit parent initialization,
occupied path, orphan companion, overwrite conflict, route-identity collision,
self-move, destination-inside-source, lexical escape, physical alias, or
unsafe identity. It never chooses a destination by basename, slug similarity,
generated order, or likely intent.

For a category, the destination occupancy check covers every path in the complete
intended layout before the plan is admitted. A destination that would overwrite
any existing file, directory, entrypoint, resource, or overwrite companion
blocks the whole category operation.

## Reference Resolution And Intended Rewrites

The reference pass records each supported authored local Markdown occurrence in
the complete workspace catalogue. For each occurrence, it retains its source
path and layer, authored location, raw destination, fragment, target identity,
target path, and exact supported-resolution state.

The planner compares the old and intended post-move identities and source
locations. It forms a rewrite only when the existing destination would no longer
resolve to the same intended target after the move:

- an outside source points to a moved leaf, category item, or resource;
- a moved source points to a target outside the moved subject and its relative
  destination must change; or
- a moved source and moved target need a new authored destination because their
  relative layout or physical identity relationship changes.

When a source and target both move and their existing relative destination remains
valid, the occurrence is an unchanged observation, not a synthetic rewrite.
External URLs are not local targets and are not rewritten. Unsupported or
ambiguous forms that could apply to the moved meaning prevent complete coverage
and form `incomplete`; unsafe paths or aliases form `blocked`.

Each intended rewrite preserves the visible label, fragment, valid encoding,
surrounding Markdown, and every unrelated byte. The intended file state is the
complete current source bytes with only the exact supported destination literal
changes required by the move. Multiple non-overlapping rewrites in one physical
file coalesce into one complete-file effect with one expected state and one
recovery boundary.

The planner does not change generated `Entries` as ordinary references. It leaves
generated interiors to the Index projection and retains their effect evidence
separately.

## Generated Projection

The projection uses the accepted [Index Behavior Contract](../../index/behavior.md)
against the hypothetical post-move workspace. It derives topology from intended
physical placement and authored metadata, not from stale generated lines.

The automatic generated target set includes the old exposing parent and the new
exposing parent. If a moved category is exposed directly by the Loader, the
Loader region is included. A moved category may also require a bounded generated
region inside a moved entrypoint when the destination changes the direct-child
projection or its containing-file-relative destination; the planner includes
only such dependency-minimal regions.

The projection validates every required generated boundary, direct-child
metadata fact, destination, and route relationship. It preserves markers and
bytes outside each bounded generated interior. Missing, duplicate, nested,
reversed, misplaced, or otherwise ambiguous markers block the complete plan. A
generated planning failure occurs before any authored move or reference rewrite.

The operation never invokes a hidden `index` subprocess or schedules a second
indexing pass. Generated effects are part of the move's one result and recovery
boundary.

## Selection And Result Formation

After all current facts are complete, the planner forms one intended post-move
workspace state:

- the selected leaf pair or complete category tree is absent at the old layout;
- the selected subject is present at the exact destination layout;
- supported local references have their exact intended destination literals;
- internal references that remain valid are byte-for-byte unchanged;
- affected generated interiors match the authoritative post-move projection; and
- all unrelated authored sources and bytes remain unchanged.

The planner compares this intended state with current bytes and classifies each
path as changed, unchanged, or a collision. It does not rewrite unchanged
reference sources or generated regions. It forms one typed result containing the
subject, destination, inventory, reference coverage, effects, expected state,
preflight, verification, recovery, and semantic status.

The status selector applies the Interface meanings:

- A complete safe plan in dry-run is `complete` unless a stronger condition
  applies. Planned effects do not create `attention`.
- A complete and verified application is `complete`.
- `attention` is currently unreachable for `route move`.
- Safe but unfinished catalogue or reference coverage is `incomplete` and has no
  effects.
- Invalid operands and consumed-source exact source-not-found are `invalid`.
- Unsafe or ambiguous ownership, identity, route, destination, generated,
  expected-state, or recovery boundaries are `blocked`.
- An unexpected post-effect application, verification, or recovery failure is
  `failed`.
- Cancellation without residual recovery failure is `interrupted`.

For ordinary operation conditions, precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation work. Failed and
interrupted retain their event meaning.

## Complete Plan And Effects

The complete plan contains all effects before the first persistent effect:

- directory and file moves or deletions needed for the leaf pair or category
  tree;
- destination creation at the exact selected leaf or category target;
- complete-file reference rewrites with expected old bytes and intended new
  bytes; and
- old/new parent and applicable Loader generated-region replacements.

Compatible changes to one physical path coalesce. A category's many contained
files are still one operation with one preflight, one application result, one
verification boundary, and one reverse-recovery boundary. A blocker prevents
every effect; no safe subset is applied.

The operation preserves authored bytes outside exact reference literals and
bounded generated interiors. It preserves the category's relative layout,
base/overwrite adjacency, source layering, native and ordinary resource bytes,
and unrelated workspace content. It does not format files, repair metadata,
change route meaning, or create a lifecycle record.

## Dry-Run Parity

Dry-run and application use the same normalized request, workspace and source
resolution, category inventory, lifecycle-ownership proof, reference catalogue,
intended bytes, generated projection, ordered plan, expected-state facts, and
preflight. Dry-run stops before backup creation, directory creation, file move,
deletion, replacement, reference rewrite, generated-region write, or any other
persistent effect.

Dry-run result evidence contains every planned path and exact bounded effect:
selected and destination paths, reference old/new literals, generated-region
diffs, category-relative paths, and required recovery facts. It does not claim
that intended bytes were verified on disk. It reports `No files changed
(--dry-run)` in human presentation and retains equivalent typed evidence in
JSON.

## Application Authority And Git Policy

The command path and exact source and destination subjects provide application
consent. Application is otherwise gated by the complete plan and preflight. It
does not prompt and does not accept a confirmation flag.

For application, the affected-path Git check covers every existing path that the
complete plan may move, remove, replace, or rewrite, including generated-region
targets and source layers. Dirty affected paths block by default. Read-only
facts that are not effect targets do not become dirty affected paths merely
because they contributed metadata or reference evidence.

`--skip-git-check` normalizes to one bypass of affected-path cleanliness only. It
does not grant move, overwrite, deletion, adoption, ownership, containment,
marker, expected-state, verification, or recovery authority.

When Git cannot provide recovery for an existing replacement or deletion, the
operation establishes the accepted adjacent target-associated backup readiness
before the first write. An unavailable, unknown, or colliding required recovery
artifact blocks the complete plan. The operation never overwrites an unknown
adjacent artifact. Backup naming and mechanics remain implementation details
constrained by the CLI Architecture and Gate 5 recovery proof.

A verified no-op is not available for an ordinary move whose old source is
missing. The operation must resolve the requested source to form a move; the
consumed-source repeat is exact source-not-found `invalid` rather than a
provenance claim.

## Revalidation, Verification, And Recovery

Immediately before application, the operation rechecks every source item,
destination path, category-relative collision, ownership inventory, reference
occurrence, generated boundary, expected byte state, containment fact, and
recovery condition in the complete plan. Volatile effect targets are rechecked
before their effect is applied. A changed fact prevents stale intent from
writing.

Application verifies each moved or created path, each reference replacement, each
generated region, and the preserved logical base/overwrite relationship. Final
verification rebuilds the complete intended route and reference facts from the
post-move workspace and verifies the subject's destination, relative category
layout, absence of the old subject, every intended reference meaning, and every
affected generated projection.

If application or verification fails after an effect begins, no new effect is
started and already applied effects are reversed in reverse effect order. Reverse
recovery changes a target only while its current identity still matches the
identity applied by this operation. It never overwrites an unexpected concurrent
edit. A concurrent edit is preserved and reported as residual state.

Backups are removed only after complete final verification. Needed backups and
residual paths remain visible after interruption or incomplete recovery. A hard
process stop may leave complete old or new versions and recognized recovery
evidence; the operation creates no persistent transaction journal.

An unexpected application, verification, or recovery failure remains `failed`
even when handled recovery succeeds. Cancellation before effects or after
complete recovery is `interrupted`; incomplete recovery is `failed`. A rerun
forms a fresh plan from current facts and never replays a saved plan. It may
converge only when the current requested source, destination, ownership,
reference, topology, and recovery facts establish a new safe operation.

## Presentation Relationship

One typed result feeds expanded human, compact human, and JSON rendering. The
renderers do not rerun source resolution, inventory, reference scanning,
planning, application, verification, or recovery. Presentation cannot change
status or hide a required safety or coverage boundary.

Human `complete`, `attention`, and `incomplete` results go to stdout. Human
`invalid`, `blocked`, `failed`, and `interrupted` results go to stderr. JSON
emits one complete result on stdout for every semantic status, and bounded
diagnostics use stderr. Compact and structured results retain at most one
required `Next:` action. Complete results have none.

The result exposes all reference rewrites and generated effects needed to
understand what moved. Category output retains the complete category inventory
or the safe facts and explicit incomplete/blocked boundary; it does not report a
successful partial category move.

## Behavioral Conformance

The mandatory public evidence boundary is [Interface Verification](interface.md#verification-requirements).
A conforming implementation must additionally prove:

- one exact request resolver with shared source and global flag semantics and no
  hidden category or destination inference;
- leaf base/overwrite pairing, category root selection, complete physical
  inventory, preserved relative layout, and rejection of Loader/workspace-root,
  native, unsupported, orphan, ambiguous, and lifecycle-managed subjects;
- complete positive unmanaged proof from the Framework baseline and all
  applicable Extension claims, including every missing, malformed, conflicting,
  stale, and incomplete inventory boundary;
- exact destination parent requirements, occupied-target and complete-category
  collision checks, self-move and destination-inside-source rejection, aliases,
  unsafe containment, and no implicit parent initialization;
- complete supported-workspace-Markdown enumeration inside and outside `.agents`,
  base/overwrite layer coverage, exact incoming and outgoing move rewrites,
  internal-link preservation, external URL preservation, and label, fragment,
  encoding, and unrelated-byte preservation;
- incomplete unsupported or ambiguous potentially applicable reference coverage
  and blocked unsafe identity or containment;
- authoritative old/new parent projection and Loader projection when applicable,
  generated-boundary validation, bounded-byte preservation, and no hidden Index
  subprocess;
- one complete category plan and recovery boundary rather than independently
  committed leaf effects;
- exact dry-run/application parity, explicit-subject consent, no persistent
  dry-run effect, and complete effect visibility;
- affected-path Git policy, the narrow skip check, backup readiness when Git
  cannot recover, expected-state revalidation, per-effect verification, complete
  postcondition verification, identity-guarded reverse recovery, residual
  preservation, and fresh-plan rerun;
- all seven semantic statuses, including reserved unreachable `attention` and
  consumed-source exact source-not-found `invalid`; and
- human/JSON parity, stream assignment, compact retention, structured
  detachment/effect evidence, and one-result rendering without rerunning work.

Direct tests should prove request and subject resolution, ownership proof,
category inventory, destination mapping, reference transformations, projection,
plan completeness, dry-run parity, status, and no-op/repeat boundaries. Focused
integration tests should use real temporary workspaces with leaf and category
trees, overwrite pairs and orphans, native and ordinary resources, contained
Markdown outside `.agents`, generated parent effects, Git and Gitless recovery,
expected-state changes, and concurrent edits. Gate 5 executable proof must
exercise the accepted parser, filesystem, physical-identity, lock, recovery,
library, Native AOT, test, and source-layout boundaries; source inspection or a
managed build alone is insufficient.

## Related Current Sources

- [route move Interface Contract](interface.md)
- [route move Command Contract Set](_move.md)
- [Route group entrypoint](../_route.md)
- [Index Behavior Contract](../../index/behavior.md)
- [Index Interface Contract](../../index/interface.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [References Behavior Contract](../../references/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [CLI Architecture](../../../architecture.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
