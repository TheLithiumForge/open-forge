---
open-forge:
  description: Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route remove`
  responsibility: Define how `route remove` forms one complete leaf or category plan, detaches incoming references, projects navigation, and recovers safely
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Remove, Behavior, Mutation, Reference, Safety, Recovery, CurrentTruth]
---

# route remove Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route remove`. The command does not ship yet;
implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The [Interface Contract](interface.md) defines the complete public syntax,
subject boundary, observable output, semantic results, honest repeat rule,
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

`route remove` performs one complete structural removal for one eligible ordinary
unmanaged logical leaf or one eligible ordinary unmanaged category. For
unchanged workspace bytes and explicit input, it resolves the same subject,
forms the same complete incoming-reference detachment and generated-navigation
plan, and produces the same semantic result.

The operation satisfies these invariants:

- A leaf is one ordinary routed Markdown base plus its valid adjacent overwrite,
  when present. The pair is one logical source and one removal effect.
- A category is the complete physically contained folder tree rooted at one
  recognized entrypoint. Every contained regular file and directory—including
  routed or unrouted Markdown, native or binary resources, support files, and
  overwrite companions—is part of one category plan.
- Every category item passes complete containment, identity, ownership,
  lifecycle, collision, reference, and recovery classification before the plan
  is complete.
- Positive trusted unmanaged proof covers every selected logical source or
  resource. Absence of one receipt never substitutes for a complete inventory.
- The complete supported-workspace-Markdown reference pass covers sources inside
  and outside `.agents`. Every supported incoming link to the removed subject is
  detached, or the complete operation is refused.
- Generated `Entries` are projected from intended topology and authored metadata.
  Current generated lines do not establish identity or authored reference
  authority.
- No persistent effect begins until the subject, ownership proof, reference
  coverage, generated projection, complete plan, and preflight are complete.
- Every ordinary source, detachment, generated, and removal effect target
  consumes the neutral no-follow final-leaf observation before ordinary
  physical resolution, at initial preflight, during under-lease revalidation,
  and immediately before its effect. A present link, reparse point, or special
  final leaf blocks the ordinary delete or replacement; Route Remove never
  follows, writes, or deletes a Library projection. Stable contained
  directory-link ancestry remains governed by the current ordinary path
  contract, and this guard does not consult Library record authority.
- A category is one recovery and verification boundary, never a sequence of
  independently committed leaf removals.
- A repeated remove is `complete` as a verified no-op only when exact intended
  absence and every required ownership, topology, reference, and generated
  projection fact are independently proven. Missing evidence is not a no-op.

## Request Resolution

### Command and workspace

Request resolution applies the shared terminal rules before domain work. It
validates exactly one source reference, the Boolean write-policy flag, and
the shared global flags against the [Interface Contract](interface.md#syntax).

The selected workspace is the exact current working directory or the exact
`--workspace <path>` value under the shared [Global CLI Flags Behavior
Contract](../../shared/global-flags/behavior.md). Resolution does not search
parent directories, substitute a Git root, infer a workspace from an operand,
or use a nearby `.agents` directory.

`--dry-run` normalizes to one idempotent Boolean choice. No command-specific
input has last-wins or precedence behavior. Shared
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

Before ordinary physical resolution of a selected source or any intended effect
target, the resolver obtains its neutral no-follow observation of the logical
final leaf. A present link, reparse point, or special final leaf is not admitted
as an ordinary Route Remove subject or effect target. This is a filesystem
safety fact, not a Library-record lookup; a missing record cannot make a
projection writable or removable.

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
- complete trusted lifecycle-ownership inventory and the proof that neither the
  base nor overwrite is claimed;
- complete supported-workspace-Markdown catalogue coverage for incoming
  references;
- old exposing parent and Loader projection facts when applicable;
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
- complete incoming-reference classification for every external source that may
  point to any category item;
- expected-state and recovery evidence for every planned effect; and
- generated boundaries and direct-child projections for the affected old route
  surfaces.

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

The resolver loads one complete trusted lifecycle-ownership inventory containing
the Framework baseline and every applicable Extension receipt or manager claim.
It establishes that no selected logical source or resource is claimed. Missing,
malformed, conflicting, stale, or incomplete inventory forms the public blocked
ownership boundary. A path, route, tag, generated line, matching bytes, missing
receipt, or prior result cannot complete this proof.

Framework-aware Route Init targets and generated regions are positive Framework
claims and therefore block current Route Remove. A user-owned inserted scope
entrypoint remains unmanaged, but its category is ineligible when the contained
inventory includes any managed scoped target or region. The operation never skips
that item or removes a subset. `sourceAssetPath` is canonical asset provenance,
not release authority; this operation neither edits the Framework lifecycle
section nor releases ownership.

### Reference coverage

The reference resolver enumerates every supported Markdown source physically
contained by the selected workspace, including sources outside `.agents`, and
reads every applicable base and overwrite layer. It records complete coverage
before it forms a detachment effect. Generated `Entries` interiors are excluded
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
5. reference-detachment and generated-projection consequences; and
6. expected-state and recovery evidence for any planned effect.

A failure at any item prevents the category plan. The resolver does not promote a
contained item to an independent command, omit it because it is not routed, or
make its generated region an authored source.

## Reference Resolution And Intended Detachments

The reference pass records each supported authored local Markdown occurrence in
the complete workspace catalogue. For each occurrence, it retains its source
path and layer, authored location, raw destination, fragment, target identity,
target path, and exact supported-resolution state.

The planner selects only exact incoming references from a source outside the
removed subject whose resolved local target is one selected leaf, category item,
or resource. For each selected occurrence, it forms a replacement of the whole
supported link with its visible label as plain authored text. It preserves the
surrounding prose, whitespace, punctuation, and unrelated bytes.

References originating inside the removed subject are not effects. They disappear
with the selected source. References from outside the subject to other targets
remain unchanged. External URLs are not local incoming references and are not
changed.

The planner refuses an occurrence when its link form or target identity is
unsupported or ambiguous, or when replacing it would lose surrounding prose.
Such a required unsafe transformation forms `blocked`. A catalogue or source
layer that cannot be completely enumerated or inspected forms `incomplete`.
The operation never makes an unsupported link look detached by deleting its
surrounding text or silently leaving a supported link broken.

Every detachment retains its source identity, layer, location, original
destination, visible label, expected bytes, intended bytes, and verification
condition. Multiple non-overlapping detachments in one physical file coalesce
into one complete-file effect with one expected state and one recovery boundary.

The planner does not change generated `Entries` as ordinary references. It leaves
generated interiors to the Index projection and retains their effect evidence
separately.

## Generated Projection

The projection uses the accepted [Index Behavior Contract](../../index-candidate/behavior.md)
against the hypothetical post-remove workspace. It derives topology from the
intended physical absence and authored metadata, not from stale generated lines.

The automatic generated target set includes the old exposing parent. If a
removed category is exposed directly by the Loader, the Loader region is
included. The planner includes only other dependency-minimal generated regions
whose direct-child projection changes under the removal.

The projection validates every required generated boundary, direct-child
metadata fact, destination, and route relationship. It preserves markers and
bytes outside each bounded generated interior. Missing, duplicate, nested,
reversed, misplaced, or otherwise ambiguous markers block the complete plan. A
generated planning failure occurs before any authored detachment or subject
removal.

The operation never invokes a hidden `index` subprocess or schedules a second
indexing pass. Generated effects are part of the remove's one result and recovery
boundary.

## Selection And Result Formation

After all current facts are complete, the planner forms one intended post-remove
workspace state:

- the selected leaf pair or complete category tree is absent;
- every supported external incoming reference has been replaced by its visible
  label as plain authored text;
- every reference inside the selected subject disappears with that subject;
- affected generated interiors match the authoritative post-remove projection;
- no stale generated entry continues to expose the removed subject; and
- all unrelated authored sources and bytes remain unchanged.

The planner compares this intended state with current bytes and classifies each
path as changed, unchanged, or a conflict. It does not rewrite unchanged
reference sources or generated regions. It forms one typed result containing the
subject, inventory, reference coverage and detachments, effects, expected state,
preflight, verification, recovery, and semantic status.

The status selector applies the Interface meanings:

- A complete safe plan in dry-run is `complete` unless a stronger condition
  applies. Planned effects do not create `attention`.
- A complete and verified application with recovery `Deleted`/`Removed` is
  `complete`.
- Post-verification recovery deletion `Failed`/positively observed `Retained`
  after verified effects is `attention`.
- Safe but unfinished catalogue or reference coverage is `incomplete` and has no
  effects.
- Invalid operands or missing source without independent absence proof are
  `invalid`.
- Unsafe or ambiguous ownership, identity, route, reference transformation,
  generated, expected-state, or recovery boundaries are `blocked`.
- An unexpected post-effect application or verification failure, or
  post-verification recovery deletion `Failed`/`Unknown`, is `failed`.
- Cancellation without an unexpected application or verification failure is
  `interrupted`.

For ordinary operation conditions, precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation work. Failed and
interrupted retain their event meaning.

## Complete Plan And Effects

The complete plan contains all effects before the first persistent effect:

- directory and file removal needed for the leaf pair or category tree;
- complete-file replacements for every required incoming-link detachment; and
- old parent and applicable Loader generated-region replacements.

Initial preflight rechecks the no-follow final-leaf fact for every ordinary
effect target before any effect is admitted. A present link, reparse point, or
special final leaf blocks all effects; no safe subset is applied.

Compatible changes to one physical path coalesce. A category's many contained
files are still one operation with one preflight, one application result, one
verification boundary, and one recovery-bundle preparation/retention boundary. A blocker prevents
every effect; no safe subset is applied.

The operation preserves authored bytes outside exact supported links and bounded
generated interiors. It preserves unrelated workspace content and every source
not selected for removal. It does not format files, repair metadata, change
route meaning, or create a lifecycle record.

## Dry-Run Parity

Dry-run and application use the same normalized request, workspace and source
resolution, category inventory, lifecycle-ownership proof, reference catalogue,
intended detachments, generated projection, ordered plan, expected-state facts,
and preflight. Dry-run stops before recovery-bundle creation, deletion, replacement,
reference detachment, generated-region write, or any other persistent effect.
It performs the same no-follow final-leaf observations and reports the same
blocking facts as application.

Dry-run result evidence contains every planned path and exact bounded effect:
selected and removed paths, detachment old/new content, generated-region diffs,
category-relative paths, and required recovery facts. It does not claim that
intended bytes were verified on disk. It reports `No files changed (--dry-run)`
in human presentation and retains equivalent typed evidence in JSON.

## Application Authority And Recovery Bundle

The command path and exact source subject provide application consent.
Application is otherwise gated by the complete plan and preflight. It does not
prompt and does not accept a confirmation flag.

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
schema-v1 `manifest.json` and streamed ordinal payload entries record
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

## Honest Repeat And Verified Absence

Before calling a missing source a verified no-op, the resolver independently
proves the exact intended absence and checks the complete current boundaries:

1. The requested source identity is absent and no replacement source has been
   adopted at that identity.
2. The trusted ownership inventory is complete and no selected ownership claim or
   orphan companion remains.
3. Current topology and the affected generated projection prove the intended
   absence and contain no stale generated region exposing the subject.
4. The complete supported-workspace-Markdown reference pass finds no residual
   incoming reference to the absent subject.
   When every fact is complete and safe, result formation produces `complete` with
   verified no-op evidence and no mutation path. If the source is missing but one of these
   proofs is unavailable, invalid, incomplete, or blocked, result formation keeps
   that applicable status. It never uses a receipt, tombstone, journal, or history
   record to claim that a previous remove caused the absence.

## Revalidation, Verification, And Recovery

Immediately before application, the operation rechecks every source item,
ownership fact, reference occurrence, generated boundary, expected byte state,
containment fact, and recovery condition in the complete plan. Volatile effect
targets are rechecked under the held lease, including each no-follow final-leaf
observation, before their effect is applied. Immediately before every ordinary
delete or replacement, it repeats the final-leaf observation. A changed fact or
a present link, reparse point, or special leaf prevents stale intent from
writing, following, or deleting.

Application verifies each selected removed path, each incoming-link detachment,
and each generated region. Final verification rebuilds the complete intended
route, reference, and generated facts from the post-remove workspace and verifies
the subject's exact absence, no residual supported incoming reference, every
detachment's visible-label result, and every affected generated projection.

Before post-verification deletion begins, if application or verification fails
after an effect begins, no new effect is started and no earlier effect is
restored, reversed, or compensated. The actual residual draft or final path is
reported; a valid final remains when preparation completed. Recovery provenance does not classify current target state. An
unexpected concurrent edit is preserved and reported as residual state. After
all effects and final verification, delete only the positively recognized bundle
created by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one.

An unexpected application or verification failure remains `failed`. Cancellation
before effects is `interrupted` when no stronger failure remains. A closed final
ZIP may remain after abrupt process termination, without an executable crash or
power-loss guarantee. Cleanup owns exact named final and draft deletion under
its separate lease-bound contract. A rerun forms a fresh plan from current facts and never
replays a saved plan, receipt, journal, history, or progress record. It may
converge to a verified no-op only when the full absence proof succeeds.

## Presentation Relationship

One typed result feeds expanded human, compact human, and JSON rendering. The
renderers do not rerun source resolution, inventory, reference scanning,
planning, application, verification, or retained-state reporting. Presentation cannot change
status or hide a required safety or coverage boundary.

Human `complete`, `attention`, and `incomplete` results go to stdout. Human
`invalid`, `blocked`, `failed`, and `interrupted` results go to stderr. JSON
emits one complete result on stdout for every semantic status, and bounded
diagnostics use stderr. Compact and structured results retain at most one
required `Next:` action. Complete results have none.

The result exposes every incoming detachment and every generated effect needed
to understand what was removed. Category output retains the complete category
inventory or the safe facts and explicit incomplete/blocked boundary; it does
not report a successful partial category removal.

## Behavioral Conformance

The mandatory public evidence boundary is [Interface Verification](interface.md#verification-requirements).
A conforming implementation must additionally prove:

- one exact request resolver with shared source and global flag semantics and no
  hidden category inference;
- leaf base/overwrite pairing, category root selection, complete physical
  inventory, and rejection of Loader/workspace-root, native, unsupported,
  orphan, ambiguous, and lifecycle-managed subjects;
- complete positive unmanaged proof from the Framework baseline and all
  applicable Extension claims, including every missing, malformed, conflicting,
  stale, and incomplete inventory boundary;
- complete supported-workspace-Markdown enumeration inside and outside `.agents`,
  base/overwrite layer coverage, exact incoming-link resolution, visible-label
  detachment, surrounding-prose preservation, and external URL preservation;
- unsupported or ambiguous potentially applicable links, prose-loss
  transformations, unsafe identity, incomplete catalogue coverage, and their
  no-write results;
- authoritative old parent projection and Loader projection when applicable,
  generated-boundary validation, bounded-byte preservation, and no hidden Index
  subprocess;
- one complete category plan and recovery boundary rather than independently
  committed leaf effects;
- exact dry-run/application parity, explicit-subject consent, no persistent
  dry-run effect, every detachment in human and JSON results, and complete
  effect visibility;
- external recovery-bundle readiness, exact-entry validation, all-before-first-
  effect preparation, expected-state revalidation, per-effect verification,
  complete postcondition verification, residual preservation, cleanup handling,
  and fresh-plan rerun;
- exact intended-absence proof for verified no-op repeats and invalid,
  incomplete, or blocked results when that proof is unavailable;
- no-follow final-leaf observations before physical resolution, at initial
  preflight, under-lease revalidation, and immediately before every ordinary
  effect, including refusal to follow, write, or delete an eligible `.agents/...`
  Library projection without consulting its record;
- all seven semantic statuses, including `Failed`/positively observed `Retained`
  recovery `attention` and `Failed`/`Unknown` recovery `failed`; and
- human/JSON parity, stream assignment, compact retention, structured
  detachment/effect evidence, and one-result rendering without rerunning work.

Direct tests should prove request and subject resolution, ownership proof,
category inventory, reference transformations, projection, plan completeness,
dry-run parity, status, absence proof, and recovery. Focused integration tests
should use real temporary workspaces with leaf and category trees, overwrite
pairs and orphans, native and ordinary resources, contained Markdown outside
`.agents`, generated parent effects, external recovery-bundle residuals,
expected-state changes, link prose preservation, and concurrent edits. Gate 5 executable proof
must exercise the accepted parser, filesystem, physical-identity, lock, recovery,
library, Native AOT, test, and source-layout boundaries; source inspection or a
managed build alone is insufficient.

## Related Current Sources

- [route remove Interface Contract](interface.md)
- [route remove Command Contract Set](_remove.md)
- [Route group entrypoint](../_route.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [References Behavior Contract](../../references-candidate/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
