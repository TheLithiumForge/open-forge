---
open-forge:
  description: Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route move`
  responsibility: Define how `route move` forms one complete leaf or category plan, preserves references, projects navigation, and recovers safely
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Move, Behavior, Mutation, Reference, Safety, Recovery, CurrentTruth]
---

# route move Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route move`. The command is implemented in the
merged native CLI. Its implementation and complete managed and Native AOT
executable proof are squash-integrated by the commit containing this record.

The [Interface Contract](interface.md) defines the complete public syntax,
subject boundary, destination meaning, observable output, semantic results,
errors, examples, non-goals, and public verification. This file defines how a
conforming implementation resolves current facts, forms one plan, applies and
verifies its effects, recovers from handled failure, and produces that result.
It does not add flags, operands, aliases, output fields, semantic statuses, or
implementation technology.

The shared [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md),
[CLI Source References Behavior Contract](../../shared/source-references/behavior.md),
and [Index Behavior Contract](../../index-candidate/behavior.md) remain
authoritative at their own scopes. The current Framework routing, Markdown,
overwrite, and lifecycle sources define the meaning consumed by this operation.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared result schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines parser and serializer roles,
filesystem and physical-identity, workspace-lock and recovery boundaries,
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
- Every ordinary source, destination, reference, and generated effect target
  consumes the neutral no-follow final-leaf observation before ordinary
  physical resolution, at initial preflight, during under-lease revalidation,
  and immediately before its effect. A present link, reparse point, or special
  final leaf blocks the ordinary move, create, delete, or replacement; Route
  Move never follows, writes, or deletes a Library projection. Stable contained
  directory-link ancestry remains governed by the current ordinary path
  contract, and this guard does not consult Library record authority.
- A category is one recovery and verification boundary, never a sequence of
  independently committed leaf operations.
- A repeated move using the consumed old source forms exact source-not-found
  `invalid-input` evidence. It does not manufacture a verified no-op from the absence.

## Request Resolution

### Command and workspace

Request resolution applies the shared terminal rules before domain work. It
first leaves unmatched positional input, unknown symbols, and parser diagnostics
to the shared shell. A third positional operand therefore stops as shell-owned
`cli.parser.invalid` input before Route Move binding, workspace selection, or domain
execution. It returns exit `4` with empty stdout and a nonempty stderr diagnostic,
but no Route Move result, status, finding, or next action. Parser diagnostic text
is not a Route Move contract.

For a shell-accepted selected command, binding reads the source reference,
destination target, Boolean write-policy flags, and shared global flags against
the [Interface Contract](interface.md#syntax). A missing source or destination
forms the typed Route Move `invalid-input` result before workspace or domain work. A
valid request contains exactly one source reference and one destination target.

The selected workspace is the exact current working directory or the exact
`--workspace <path>` value under the shared [Global CLI Flags Behavior
Contract](../../shared/global-flags/behavior.md). Resolution does not search
parent directories, substitute a Git root, infer a workspace from an operand,
or use a nearby `.agents` directory.

`--dry-run` normalizes to one idempotent Boolean choice. No command-specific
input has last-wins or precedence behavior. Shared
global flags retain their own repetition, composition, and terminal rules.

Command-local invalid input stops before source resolution. JSON and other
non-interactive modes never prompt for a collision or missing semantic subject.
Ordinary help and version invocations retain the shared successful terminal
short-circuit. Unmatched or unknown input remains shell terminal invalid even
when a terminal flag is present.

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

Before ordinary physical resolution of a selected source or any intended effect
target, the resolver obtains its neutral no-follow observation of the logical
final leaf. A present link, reparse point, or special final leaf is not admitted
as an ordinary Route Move subject or effect target. This observation is a
filesystem safety fact, not a Library-record lookup; a missing record cannot
make a projection writable, movable, or removable.

## Current Facts And Coverage

Before planning, the operation establishes the complete facts required by the
selected subject kind.

### Leaf facts

For a leaf, current facts include:

- requested and resolved source identity, base path, and valid overwrite layer;
- ordinary routed Markdown kind, exposing parent, route identity, and
  compatibility representation;
- no-follow final-leaf fact for the source, its overwrite companion, and each
  resolved effect target;
- lexical and physical containment and exact physical identity;
- destination target kind, parent entrypoint, occupancy, alias, collision, and
  containment facts;
- complete interpretable lock ownership inventory and the proof that neither the
  base nor overwrite is claimed;
- supported-workspace-Markdown catalogue coverage for the complete reference
  pass;
- old and new exposing parent and Loader projection facts when applicable;
- expected bytes, generated boundaries, recovery-bundle readiness, and volatile
  expected-state facts.

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

The neutral final-leaf observation is part of that inspection. A contained link,
reparse point, or special final leaf blocks the complete ordinary category plan
instead of being traversed or treated as source bytes. Stable directory-link
ancestry that remains within the ordinary path contract is not newly rejected by
this rule.

### Ownership inventory

The command reads Framework and Extension ownership from the forgiving
`.agents/open-forge.lock.json` reader. A complete interpretable inventory must
establish that none of the selected logical sources or resources is claimed
before a mutation plan can form. Both whole-file and region receipts protect
their hosts, including portable case aliases. One claim protects the whole
selected category; the operation never skips a claimed member.

Missing, unreadable or uninterpretable ownership does not infer unmanaged state.
It produces `ownership-unavailable` with complete informational status, no plan
or effects, and ownership shown as not-established. The summary explicitly says
that no route changed. Schema/release metadata and stale content hashes are not
gates. Actual ownership, physical safety, route and reference conflicts remain
blocking boundaries. The exact lock expectation is revalidated before and after
mutation. No legacy record is read, migrated or deleted; these commands neither
adopt current content nor release or rewrite ownership.

Path names, routing tags, generated lines, matching bytes and prior command
results cannot independently establish unmanaged status. Framework-aware Route
Init's region receipts remain positive ownership even in a user-authored host.

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

- A leaf destination is either one exact ordinary routed Markdown filesystem
  path under one existing valid parent route, or one accepted logical leaf ID.
  For the logical form, canonical route resolution maps the slash-separated
  ID without an `.agents/` prefix or `.md` suffix to the physical Markdown
  path; for example, `guidance/team/moved-note` maps to
  `.agents/guidance/team/moved-note.md`. The resolver then applies the same
  parent, occupancy, identity, and safety checks to that canonical path.
  The final segment maps to the Markdown leaf, while each intermediate segment
  must map through an existing valid route parent.
- A category destination is one exact recognized entrypoint path inside a new
  category folder whose parent is one existing valid route. That path names the
  new category root and its entrypoint; the rest of the category layout is
  derived from the source-relative paths.

Logical leaf IDs use slash-separated route segments and reject `.`, `..`,
empty or otherwise unsafe segments. The resolver rejects a missing parent route,
implicit parent initialization, occupied path, orphan companion, overwrite
conflict, route-identity collision, self-move, destination-inside-source,
lexical escape, physical alias, or unsafe identity. It never chooses a
destination by basename, slug similarity, generated order, or likely intent,
and it never invents a category or route.

Destination and reference/generated effect targets receive the same no-follow
final-leaf observation before ordinary physical resolution. A present link,
reparse point, or special final leaf blocks the complete plan, including when an
eligible `.agents/...` leaf is a filesystem projection whose target is contained
by the workspace. Route Move does not follow or write through that projection,
and the decision is independent of Library record authority.

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

The projection uses the accepted [Index Behavior Contract](../../index-candidate/behavior.md)
against the hypothetical post-move workspace. It derives topology from intended
physical placement and authored metadata, not from stale generated lines.

The automatic generated target set includes the old exposing parent and the new
exposing parent. If a moved category is exposed directly by the Loader, the
Loader region is included. A moved category may also require a bounded generated
region inside a moved entrypoint when the destination changes the direct-child
projection or its containing-file-relative destination; the planner includes
only such dependency-minimal regions.

The projection validates every required generated boundary, direct-child
metadata fact, destination, and route relationship. It preserves the Entries
heading and bytes outside its generated body. A missing or duplicate heading
boundary blocks the complete plan. A
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

- A complete safe plan in dry-run is `completed` unless a stronger condition
  applies. Planned effects do not create `completed-with-warnings`.
- A complete and verified application with recovery `Deleted`/`Removed` is
  `completed`.
- Post-verification recovery deletion `Failed`/positively observed `Retained`
  after verified effects is `completed-with-warnings`.
- Safe but unfinished catalogue or reference coverage is `incomplete` and has no
  effects.
- Missing required source or destination and consumed-source exact
  source-not-found are Route Move `invalid-input` results. A third positional operand
  has already stopped as `cli.parser.invalid` at the shell parser and never
  reaches this selector.
- Unsafe or ambiguous ownership, identity, route, destination, generated,
  expected-state, or recovery boundaries are `blocked`.
- An unexpected post-effect application or verification failure, or
  post-verification recovery deletion `Failed`/`Unknown`, is `failed`.
- Cancellation without an unexpected application or verification failure is
  `cancelled`.

For ordinary operation conditions, precedence is `blocked` > `incomplete` >
`completed-with-warnings` > `completed`. Invalid input stops before operation work. Failed and
cancelled retain their event meaning.

## Complete Plan And Effects

The complete plan contains all effects before the first persistent effect:

- directory and file moves or deletions needed for the leaf pair or category
  tree;
- destination creation at the exact selected leaf or category target;
- complete-file reference rewrites with expected old bytes and intended new
  bytes; and
- old/new parent and applicable Loader generated-region replacements.

Initial preflight rechecks the no-follow final-leaf fact for every ordinary
effect target before any effect is admitted. A present link, reparse point, or
special final leaf blocks all effects; no safe subset is applied.

Compatible changes to one physical path coalesce. A category's many contained
files are still one operation with one preflight, one application result, one
verification boundary, and one recovery-bundle preparation/retention boundary. A blocker prevents
every effect; no safe subset is applied.

The operation preserves authored bytes outside exact reference literals and
bounded generated interiors. It preserves the category's relative layout,
base/overwrite adjacency, source layering, native and ordinary resource bytes,
and unrelated workspace content. It does not format files, repair metadata,
change route meaning, or create an ownership lock.

## Dry-Run Parity

Dry-run and application use the same normalized request, workspace and source
resolution, category inventory, lifecycle-ownership proof, reference catalogue,
intended bytes, generated projection, ordered plan, expected-state facts, and
preflight. Dry-run stops before recovery-bundle creation, directory creation, file move,
deletion, replacement, reference rewrite, generated-region write, or any other
persistent effect.
It performs the same no-follow final-leaf observations and reports the same
blocking facts as application.

Dry-run result evidence contains every planned path and exact bounded effect:
selected and destination paths, reference old/new literals, generated-region
diffs, category-relative paths, and required recovery facts. It does not claim
that intended bytes were verified on disk. It reports `No files changed
(--dry-run)` in human presentation and retains equivalent typed evidence in
JSON.

## Application Authority And Recovery Bundle

The command path and exact source and destination subjects provide application
consent. Application is otherwise gated by the complete plan and preflight. It
does not prompt and does not accept a confirmation flag.

The command does not inspect or report repository state. Before the
first target effect, orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. When the operation has one or more existing-target
effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`), it prepares exactly
one immutable ZIP bundle outside the workspace. An operation containing only Create effects or
no-ops creates no bundle. Its source-generated
versioned `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires the matching preparation for every existing-target effect
and performs one final effect per target. All preparation completes before the
first target effect; unknown, malformed, mismatched, or colliding bundles block.

A verified no-op is not available for an ordinary move whose old source is
missing. The operation must resolve the requested source to form a move; the
consumed-source repeat is exact source-not-found `invalid-input` rather than a
provenance claim.

## Revalidation, Verification, And Recovery

Immediately before application, the operation rechecks every source item,
destination path, category-relative collision, ownership inventory, reference
occurrence, generated boundary, expected byte state, containment fact, and
recovery condition in the complete plan. Volatile effect targets are rechecked
under the held lease, including each no-follow final-leaf observation, before
their effect is applied. Immediately before every ordinary move, create, delete,
or replacement, it repeats the final-leaf observation. A changed fact or a
present link, reparse point, or special leaf prevents stale intent from writing,
following, or deleting.

Application verifies each moved or created path, each reference replacement, each
generated region, and the preserved logical base/overwrite relationship. Final
verification rebuilds the complete intended route and reference facts from the
post-move workspace and verifies the subject's destination, relative category
layout, absence of the old subject, every intended reference meaning, and every
affected generated projection.

Before post-verification deletion begins, if application or verification fails
after an effect begins, no new effect is started and no earlier effect is
restored, reversed, or compensated. The actual residual draft or final path is
reported; a valid final remains when preparation completed. Recovery provenance does not classify current target state. An
unexpected concurrent edit is preserved and reported as residual state. After
all effects and final verification, delete only the positively recognized bundle
created by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one.

Cancellation is `cancelled` when no stronger failure remains. A closed final
ZIP may remain after abrupt process termination, without an executable crash or
power-loss guarantee. Cleanup owns exact named final and draft deletion under
its separate lease-bound contract. A rerun forms a fresh plan from current
facts and never replays a saved plan, receipt, journal, history, or progress
record. It may converge only when the current requested source, destination,
ownership, reference, topology, and recovery facts establish a new safe
operation.

## Presentation Relationship

One typed result feeds full-detail human, minimal-detail human, and JSON rendering. The
renderers do not rerun source resolution, inventory, reference scanning,
planning, application, verification, or retained-state reporting. Presentation cannot change
status or hide a required safety or coverage boundary.

This presentation relationship applies only after Route Move result formation.
A shell parser failure writes its terminal diagnostic directly under shared
shell policy and never enters Route Move human or JSON rendering.

Human `completed`, `completed-with-warnings`, and `incomplete` results go to stdout. Human
`invalid-input`, `blocked`, `failed`, and `cancelled` results go to stderr. JSON
emits one complete result on stdout for every semantic status, and bounded
diagnostics use stderr. minimal-detail and structured results retain at most one
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
- shell-owned `cli.parser.invalid` rejection of a third positional operand with
  exit `4`, empty stdout, nonempty stderr, no workspace/domain/lock effect, and no Route Move
  result envelope, while missing required operands remain typed Route Move
  `invalid-input` results;
- leaf base/overwrite pairing, category root selection, complete physical
  inventory, preserved relative layout, and rejection of Loader/workspace-root,
  native, unsupported, orphan, ambiguous, and lifecycle-managed subjects;
- complete positive unmanaged proof from the Framework ownership and all
  applicable Extension claims, including every missing, malformed, conflicting,
  stale, and incomplete inventory boundary;
- exact destination parent requirements, occupied-target and complete-category
  collision checks, self-move and destination-inside-source rejection, aliases,
  unsafe containment, and no implicit parent initialization;
- no-follow final-leaf observations before physical resolution, at initial
  preflight, under-lease revalidation, and immediately before every ordinary
  effect, including refusal to follow, write, or delete an eligible `.agents/...`
  Library projection without consulting its record;
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
- recovery-bundle storage/readiness, semantic final-ZIP verification, expected-state
  revalidation, per-effect verification, complete postcondition verification,
  retained partial state without restoration, residual preservation, and
  fresh-plan rerun;
- all seven semantic statuses, including `Failed`/positively observed `Retained`
  recovery `completed-with-warnings`, `Failed`/`Unknown` recovery `failed`, and consumed-source
  exact source-not-found `invalid-input`; and
- human/JSON parity, stream assignment, minimal-detail retention, structured
  detachment/effect evidence, and one-result rendering without rerunning work.

Direct tests should prove request and subject resolution, ownership proof,
category inventory, destination mapping, reference transformations, projection,
plan completeness, dry-run parity, status, and no-op/repeat boundaries. Focused
integration tests should use real temporary workspaces with leaf and category
trees, overwrite pairs and orphans, native and ordinary resources, contained
Markdown outside `.agents`, generated parent effects, external recovery bundles,
expected-state changes, and concurrent edits. Gate 5 executable proof must
exercise the accepted parser, filesystem, physical-identity, lock, recovery,
library, Native AOT, test, and source-layout boundaries; source inspection or a
managed build alone is insufficient. The persistent reusable zero-byte external
workspace lock below `LocalApplicationData/OpenForge/locks/v1` is held with one
read/write `FileShare.None` handle; it never receives metadata writes, deletion,
or truncation.

## Related Current Sources

- [route move Interface Contract](interface.md)
- [route move Command Contract Set](_move.md)
- [Route group entrypoint](../_route.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [References Behavior Contract](../../references-candidate/behavior.md)
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
