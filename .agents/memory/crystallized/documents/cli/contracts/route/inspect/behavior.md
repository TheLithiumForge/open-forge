---
open-forge:
  description: Accepted current technology-neutral resolution, graph use, measurement, topology, safety, and conformance for `route inspect`
  responsibility: Define how a conforming implementation forms the one-source route profile without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Route, Inspect, Behavior, CurrentTruth]
---

# route inspect Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract for `route inspect`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The [Interface Contract](interface.md) defines the complete public grammar,
observable profile, output, semantic result names, errors, and non-goals that
this behavior satisfies. This file defines deterministic resolution, graph use,
classification, measurement, topology, result formation, read-only safety,
presentation, and conformance without adding a public operand, flag, output
shape, status, or implementation technology.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
structured schema, process-status mapping, parser and serialization, filesystem
and physical-identity realization, diagnostics, source structure, package and
runtime boundaries, and test boundaries. This Behavior Contract makes no
implementation choice to change those boundaries and remains technology-neutral.

## Operation Invariants

- The operation performs one complete route-profile inspection for one resolved
  source reference. It does not select a hidden child operation.
- For the same CLI payload, workspace bytes, and explicit source reference, it
  forms the same route facts, measurements, ordering, availability, and
  semantic result described by the [Interface Contract](interface.md#purpose).
- The operation builds at most one current in-memory route and loading graph for
  the invocation. It does not persist that graph or create inspection state.
- Reading and parsing workspace bytes to establish facts does not make authored
  content part of agent context, select a Framework route for work, activate
  `Axioms`, or change authority, as required by the public [Interface operation
  boundary](interface.md#purpose).
- The operation is read-only. It acquires no mutation authority and performs no
  persistent workspace mutation, including content, route, index, repair,
  formatting, or recovery mutation.
- It does not return authored source content, perform diagnosis, or create route,
  health, content-placement, or diagnostic recommendations.
- An applicable fact that cannot be established remains `unavailable`. A measured
  zero and a `not-applicable` fact remain distinct. The operation never replaces
  either an unavailable or not-applicable fact with zero, `none`, a guessed role,
  or a guessed relationship.

## Request Resolution

### Workspace and source reference

Request resolution first applies the shared [Global CLI Flags](../../shared/global-flags/behavior.md)
workspace rules and the shared [CLI Source References](../../shared/source-references/behavior.md)
identity rules. It uses the exact current working directory or exact
`--workspace` value, resolves relative workspace values from the process current
working directory, and keeps the selected physical and lexical boundary. It
does not search upward, substitute a Git root, or infer another workspace from
the operand. Unsafe lexical, physical, or containment identity blocks the
operation under the public boundary in
[Workspace And Subject](interface.md#workspace-and-subject).

The resolver validates the one-subject command form and shared global inputs
before domain inspection. Terminal `--help` and `--version` modes stop before
domain resolution under the shared contract. In domain mode, it resolves one
automatic ID or exact path, preserves the requested reference, and retains both
automatic ID and canonical path in the result. It does not add a qualifier
syntax, fuzzy matching, case correction, or another source identity system.

An unknown or unsupported reference forms the public invalid boundary. An
unresolved ambiguous source ID forms `blocked`. Exact-path or interactive
disambiguation may resolve only source identity. If the resulting physical
source and route are safe and complete while the automatic ID remains
non-unique, resolution records the identity observation for `attention`.
Exact-path resolution does not repair a structurally ambiguous route. A Loader
reference is rejected as a workspace root rather than treated as the inspected
route subject. These resolution outcomes use the public [Errors](interface.md#errors)
and [Semantic Results](interface.md#semantic-results) without inventing another
status.

An unresolved source-ID collision retains every candidate path. Non-interactive
and JSON requests do not prompt and direct the caller to rerun with one listed
exact path. Resolution never chooses by kind, order, depth, or likely intent.

### One route and loading graph

After workspace and source-reference validation, the operation constructs at
most one current in-memory graph using the same route resolver and loading
rules as `context`. It does not implement a second interpretation of startup,
selection, `#LoadNow`, `#KeepInMind`, or overwrite behavior.

The graph contains the route and loading relationships needed to establish the
selected logical source's identity, exposing parent, route chain, direct and
descendant topology, loading classifications, scope-local loading, inherited
`Axioms` provenance, and base/overwrite layers. Ordinary Markdown links are
not graph edges for this operation. The graph exists only for the current
invocation and is never persisted as workspace truth.

### Logical source resolution

When the shared source-reference rules identify an overwrite path, resolution
normalizes it to the base logical source and places the valid overwrite layer
immediately after its base. The overwrite inherits the base route, reading
behavior, and scope; it is not a second route node. An orphan or ambiguous pair
cannot be silently converted into an independent source. The public identity
and result boundary are [Identity](interface.md#identity) and
[Workspace And Subject](interface.md#workspace-and-subject).

The resolver distinguishes routed entrypoints, routed leaves, routed native
sources, accepted compatibility entrypoints, valid overwrite pairs, detached
entrypoints, known supported sources with no route, and unresolved or ambiguous
identity according to the current graph. A detached entrypoint can supply local
identity and topology, but no Loader-rooted task-start, parent-triggered, or
later-continuity reading is claimed. A known absence of route produces the
complete `not routed` identity only when that absence is mechanically
established; route-dependent facts remain not-applicable rather than zero.

An orphan or ambiguous overwrite pair, an ambiguous route, unsafe identity, or
containment failure blocks the safe source boundary. A valid pair is one logical
source with base-first layers and does not itself affect status.

## Current Facts And Coverage

The operation inspects only the current workspace bytes and recognized source
relationships needed to answer the route-profile questions in the [Interface
Purpose](interface.md#purpose). It establishes each applicable fact before
including it in the typed result. It does not turn the inspection into a
workspace inventory, content projection, link traversal, validation report, or
diagnosis.

### Source-state classification

The classifier applies the finite source-state table in the [Interface
Contract](interface.md#source-state-classification). It first establishes safe
physical identity and route meaning, then records the source state and each
applicable fact's availability. A routed entrypoint, routed leaf, routed native
source, accepted compatibility entrypoint, valid overwrite pair, detached
entrypoint, or known supported unrouted source can therefore be complete when
its applicable facts are complete. A safe non-unique automatic ID becomes
`attention` only after exact-path or interactive resolution has selected source
identity without leaving route meaning ambiguous.

Unreadable required sources, incomplete route chains, and unmeasurable
applicable facts remain `incomplete`. Orphan or ambiguous overwrite pairs,
ambiguous routes, unsafe identity, and containment failures remain `blocked`.
Zero or several operands, the Loader, and unknown, missing, or unsupported
sources remain `invalid`. The classifier does not reinterpret a compatibility
filename, detached route, unrouted source, overwrite customization, route
depth, added context, or size as a health condition.

### Reading classification

#### Task start or resume

The classifier resolves the same startup-required context as `context` with no
explicit operands. It tests membership of the inspected logical source in that
set independently of the inspection operand. Inspecting the source cannot add
it to the set being measured. The public yes/no result and its prospective
meaning are [Task Start Or Resume](interface.md#task-start-or-resume).

For a detached entrypoint or a known supported unrouted source, no Loader-rooted
chain establishes task-start membership. The classifier records that fact as
`not-applicable`, not as a negative membership result.

#### Automatic reading trigger

The classifier evaluates the current Framework loading rules against the graph's
exposing relationships and tags. It distinguishes parent-triggered
`#LoadNow`, explicit selection for an on-demand source, routed-file
`#KeepInMind`, entrypoint `#KeepInMind`, and overwrite inheritance. For an
entrypoint `#KeepInMind` case it evaluates task-start visibility, selected route
or scope, and ancestor need; for a routed `#KeepInMind` file it retains the
defined task and later review reach. For `#LoadNow`, it retains the actual
exposing parent. For an overwrite it retains the immediate-after-base
relationship.

The resulting typed reason is paired with the actual parent or event needed by
the human renderer. The renderer receives ordinary event wording rather than
internal labels. These mechanics satisfy the public [Automatic Reading
Trigger](interface.md#automatic-reading-trigger) boundary.

A detached entrypoint has no Loader-rooted automatic trigger. Its local
`#LoadNow` descendants can still be measured after local selection. A known
supported unrouted source has no route-triggered automatic reading, so the
route-dependent value is `not-applicable`.

#### Later reads

The classifier applies the Framework's continuity occasions to determine later
read membership: context restoration, handoff, closeout, and another transition
when standing follow-up work may have changed. It reports the applicable
occasions prospectively. It does not observe or claim reads on every message,
prompt, model request, or tool call. See [Later Reads](interface.md#later-reads).

Detached and known supported unrouted sources have `not-applicable`
Loader-rooted later-read membership.

### Measurement formation

All measurement sets use unique physical source layers and the shared record of
exact Unicode scalar-value counts, exact UTF-8 byte lengths, and aggregate
estimated tokens. The estimate is computed from the aggregate character count as
`ceiling(characters / 4)`, not by summing rounded per-file displays. IEC units
and the omission of human character counts are presentation decisions owned by
the [Context Cost](interface.md#context-cost) contract; the underlying exact
values remain in the typed result.

#### Own source

The own-source set contains the inspected base and its valid adjacent overwrite
layer, in base-then-overwrite order. Measurement reads complete physical file
bytes, including frontmatter and generated regions inside an entrypoint. It
does not measure a rendered identity block or other CLI framing.

#### Selected closure and additions

The resolver calculates the selected route closure with the same rules as
`context` and without ordinary link expansion. It retains missing ancestor
entrypoints needed for route establishment, the target and overwrite, visible
`#LoadNow` descendants, and applicable scope-local loading. The selection-addition
set is the ordered selected closure minus the current task-start set. The
task-start overlap is retained as its own measurement relationship, so an
already present source is not counted as an addition and is not described as
having caused startup context.

If the difference is empty, the typed result records the empty addition and the
human renderer uses the exact no-addition meaning in
[Added By Selection](interface.md#added-by-selection). No historical invocation
or caller-retained context is consulted.

The empty difference is a measured zero and selects `complete` when all other
applicable facts are complete. A detached entrypoint has no Loader-rooted
startup comparison, so that comparison is `not-applicable`; a known supported
unrouted source has route-based selection addition `not-applicable`. Own-source
bytes remain measurable for both when their physical layers are readable.

#### `#LoadNow` descendants

For an inspected entrypoint, the operation traverses visible `#LoadNow`
relationships after that entrypoint is read and forms the unique descendant set.
It removes the inspected source and ancestor chain from this measure and places
each valid overwrite immediately after its base. It does not include ordinary
on-demand descendants merely because they are structural descendants. A
routed-file `#KeepInMind` leaf recovered globally is not silently included in
this narrow `#LoadNow` descendant measure. For an ordinary routed leaf, the
measure is not applicable, not zero.

An entrypoint with no visible `#LoadNow` descendants has a measured zero. A
detached entrypoint uses its safe local `#LoadNow` topology when applicable; a
known supported unrouted source has no route descendant measure and therefore
records `not-applicable`.

#### Availability

The operation carries zero, unavailable, and not-applicable measurements as
different states. A readable zero is measured as zero. A safely identified but
unreadable required layer makes the affected measurement unavailable and selects
`incomplete` rather than producing a partial trusted count. An unsafe identity or
containment boundary selects `blocked`; it is never downgraded to a measurement
availability condition. An applicable unmeasurable safe fact remains visibly
unavailable and selects `incomplete`. Human compact output keeps zero and
not-applicable distinct, and structured output retains all three states.

### Route topology

Topology is derived from current filesystem structure and recognized source
contracts. The resolver establishes the root route, ordered route chain, direct
exposing parent, source-ID route depth, and, for an entrypoint, direct routed
file and child-entrypoint counts plus descendant routed file and entrypoint
counts. Route depth counts source-ID route segments from the root through the
subject, with a root entrypoint at depth `1`. A detached entrypoint can provide
safe local topology without a Loader-rooted route. A known supported unrouted
source has route-dependent topology `not-applicable`.

The operation does not use current generated `Entries` as an independent
inventory and does not compare generated navigation for drift. It does not
diagnose drift. `index` owns generated navigation and `doctor` owns structural
diagnosis, as stated in [Route Structure](interface.md#route-structure).

For an ordinary routed leaf, child and descendant counts are not applicable. An
entrypoint with no children has numeric zero for its direct and descendant
counts. A detached entrypoint reports safe local counts, while a known
supported unrouted source reports route-dependent counts as not-applicable. The
resolver does not manufacture a scope count from folders, route segments,
entrypoints, or tags. A named scope role is retained only when mechanically
established in structured provenance; an unknown role remains unknown. The
scope boundary is [Why There Is No Scope Count](interface.md#why-there-is-no-scope-count).

### Inherited rules and customization

The provenance collector walks the ordered Loader and ancestor-entrypoint chain
that establishes the inspected route and records the sources contributing
inherited `Axioms`. It separately checks whether the inspected entrypoint has
substantive local `Axioms`. An inherited sentinel, an empty section, or a
missing optional local section adds no local rule. An ordinary `Axioms` heading
in a routed leaf is not activated as a Framework rule, because only the Loader
and recognized entrypoints define active `Axioms`.

The collector records the valid base/overwrite relationship but does not emit
rule bodies. Human output lists source IDs, and `context` with exact section
projection remains the operation for reading inherited rules. Extension
ownership and managed-file state are not added to this route-profile result;
they belong to lifecycle behavior outside this operation. See [Rules And
Customization](interface.md#rules-and-customization).

## Selection And Result Formation

The operation forms one typed result after request resolution, graph inspection,
classification, measurement, topology, inheritance provenance, and availability
have been established. The result keeps the requested reference, resolved
identity, physical layers, reading facts and reasons, each applicable
measurement, route structure, Axioms provenance, overwrite relationship,
observations, availability conditions, semantic status, and useful next
operations only when one is required. It retains every independently available
fact without presenting an unavailable fact as a value.

For an unresolved source-ID collision, the result retains every candidate path
and the exact-path next operation required by the shared source-reference
contract. It does not choose a candidate or omit the collision from JSON.

Selected physical layers and route facts are deduplicated by their established
logical and physical identities. Base precedes overwrite. Route chain and
topology use the current route relationships rather than generated-line order.
Unchanged input produces deterministic ordering, and the operation does not use
filesystem enumeration timing or ordinary link order as an alternate public
ordering rule.

The result selector uses only the public conditions in
[Semantic Results](interface.md#semantic-results). A routed source, accepted
compatibility entrypoint, valid overwrite pair, safely detached entrypoint, or
known supported unrouted source is `complete` when every applicable fact is
available. A safe, complete source and route with a non-unique automatic ID is
`attention` only when exact-path or interactive resolution selected source
identity. An unreadable required source, incomplete route chain, or unmeasurable
applicable fact is `incomplete`. An orphan or ambiguous overwrite, ambiguous
route, unsafe identity, or containment failure is `blocked`. Zero or several
operands, the Loader, and unknown, missing, or unsupported sources are
`invalid`. Unexpected failure is `failed`, and cancellation before completion
is `interrupted`.

For ordinary conditions, the selector applies `blocked`, `incomplete`,
`attention`, then `complete` precedence. Invalid input stops before operation
work, while failure and interruption retain their event meanings. Size,
customization, compatibility filenames, detached routes, known unrouted
sources, added context, route depth, and context size do not create `attention`
or a recommendation.

The operation does not compare generated navigation, create diagnosis or
recommendations, or follow ordinary links while forming this result. It reports
only directly observed route facts and the availability conditions required by
the public route-profile questions.

## Effects

`route inspect` is a read operation. It may open and parse the workspace bytes
needed to establish the current graph, route facts, and exact measurements, but
it performs no persistent mutation. It does not write source content,
frontmatter, generated `Entries`, indexes, caches, receipts, recovery files, or
other inspection state. It does not create a mutation plan, invoke a repair,
activate a selected Framework route, or make the inspected content part of agent
context merely because it was mechanically read.

Read-only execution stops after the typed result is formed. It does not create
an empty mutation plan or acquire write-policy authority. This satisfies the
public [Non-Goals](interface.md#non-goals) and [Purpose](interface.md#purpose)
boundaries.

## Safety And Recovery

Resolution keeps all consumed local paths inside the exact selected workspace
and applies the shared source-reference and physical-containment boundary.
Unsafe lexical or physical identity, containment escape, or an unresolved safe
source identity blocks rather than falling back to a different path or source.
Interactive disambiguation can resolve only source identity; it does not grant
write, overwrite, delete, force, or ownership authority.

The operation fails closed for orphan or ambiguous overwrite pairs, ambiguous
route chains, unsafe identity, containment failure, unreadable required sources,
and unmeasurable required facts. Orphan or ambiguous overwrite and route
identity conditions are `blocked`; unreadable or unmeasurable safe facts are
`incomplete`. It does not substitute zero, `none`, a guessed role, a guessed
route, or a guessed relationship. A cancellation or unexpected failure cannot
leave a persistent inspection effect, so there is no mutation rollback or
residual recovery state to manufacture. The public error and status boundaries
remain in [Errors](interface.md#errors) and [Semantic Results](interface.md#semantic-results).

## Presentation Relationship

Human output and `--json` consume one typed result. Renderers do not rerun graph
construction, source resolution, classification, measurement, topology, or
verification, and they do not reinterpret the semantic result.

The renderer applies the shared `--view` and `--verbose` presentation rules
after inspection. Expanded human output adds the explanations, evidence,
provenance, locations, and next actions described by the [Human Output](interface.md#human-output)
contract. Both human views retain workspace identity, selection method, source
ID, and canonical path. Compact output also retains source and route state,
route chain, applicable topology, reading behavior, own-source,
selection-addition, and `#LoadNow` descendant measurements, overwrite state,
status, completeness, safety, and at most one required `Next:` line. It omits
inherited-`Axioms` detail and optional explanation while keeping measured zero,
`unavailable`, and `not-applicable` distinct. JSON retains the complete typed
facts regardless of human view, including collision candidate paths,
observations, and availability conditions. `--json` does not prompt or rerun
work.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. `--json` writes one complete structured result to stdout for every
semantic status. Separate bounded diagnostics go to stderr, and human text is
never mixed into JSON stdout. The renderer emits a next operation only when the
Interface rules require one. It never emits route mutation proposals, health or
content-placement advice, or diagnostic recommendations.

When the Interface permits a direct safe correction, the renderer names only
the observed input or safety boundary. It does not turn that correction into a
route mutation proposal.

## Conformance Evidence

Implementation evidence must cover:

- Exact current-directory and `--workspace` selection without workspace
  discovery, as specified by [Workspace And Subject](interface.md#workspace-and-subject).
- IDs, exact paths, quoting, collisions, and source-identity disambiguation from
  the shared [CLI Source References](../../shared/source-references/behavior.md) contract.
- Workspace identity, selection method, source ID, and canonical path in both
  human views and structured output.
- Routed entrypoints, routed leaves, native routed sources, canonical and each
  compatibility entrypoint filename, detached entrypoints, known unrouted
  sources, and unsupported source kinds, as specified by [Identity](interface.md#identity).
- Base, overwrite-path, valid-pair, orphan, ambiguous, and inherited overwrite
  selection, including base-first physical-layer order and blocked orphan or
  ambiguous boundaries.
- Safe non-unique automatic IDs selected by exact path and interactive choice,
  including the interactive exact-path next operation and the no-action
  exact-path observation.
- Unresolved non-interactive ID collisions that retain every candidate path and
  require rerunning with one listed exact path.
- Task-start membership independent of the inspection operand.
- On-demand, parent-triggered `#LoadNow`, visible and selected `#KeepInMind`
  entrypoints, globally recovered routed `#KeepInMind` files, and overwrite
  inheritance.
- Plain human explanations for task-start, parent-triggered, selected, and
  later reads, including another transition that may affect standing follow-up
  work, without `target-sensitive` or `continuity boundary` labels.
- Own-source, selected-closure, task-start-overlap, additions-beyond-startup,
  and `#LoadNow` descendant set definitions and deduplication.
- Proof that the `#LoadNow` descendant measure excludes globally recovered
  `#KeepInMind` leaves and is labelled narrowly enough not to hide that boundary.
- Physical file, character, UTF-8 byte, and estimated-token measurements,
  including zero, unavailable, not-applicable, and overwrite-layer cases.
- Proof that estimated tokens are recomputed from aggregate characters and do
  not sum rounded per-file displays.
- Route root, chain, depth, parent, direct child, and descendant facts for root,
  nested, leaf, sparse, detached, and ambiguous structures.
- Proof that the command does not trust generated lines as an independent route
  inventory or report generated drift as diagnosis.
- No generic scope count and no inferred scope role.
- Inherited substantive `Axioms` provenance, local `Axioms`, inherited
  sentinel, empty local sections, and ordinary leaf headings.
- Measured zero, unavailable, and not-applicable distinctions for empty
  selection additions, empty `#LoadNow` descendant sets, ordinary leaves,
  detached entrypoints, and known unrouted sources.
- Compact retention of the required identity, route, reading, measurement,
  topology, overwrite, status, completeness, safety, and `Next:` facts, with
  inherited-Axiom detail omitted.
- Structured and human rendering from one typed result.
- Observations and availability conditions without a diagnosis-like result
  collection or recommendations.
- Primary human stdout/stderr assignment, one complete JSON result on stdout for
  every status, bounded diagnostics on stderr, and no human text in JSON stdout.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  outcomes.
- Evidence that compatibility, detached and unrouted state, valid overwrite
  customization, route depth, added context, and size do not create attention
  or recommendations.
- One graph construction, no content rendering, no link following, and no
  persistent inspection state.

Direct tests should prove reading classification, set calculation, measurement,
topology, inheritance provenance, availability, and semantic results.
Gate 5 executable proof should use real temporary rooted, detached, ambiguous,
compatibility, overwrite, Git-independent, and malformed route structures. It
should cover parsing, concise human output, structured output, exit behavior, and
packaged execution, including the accepted AOT boundary.

The Route Inspect Interface defines its exact command-local structured result.
The CLI Architecture defines the shared envelope and compatibility coordinates,
process-status mapping, parser and filesystem realization, diagnostics and
redaction, and source boundaries. The source-stated exact heading-name comparison
belongs to the Context contract, not this route-profile operation. No
command-local Technical Design is needed for `route inspect`.

## Related Current Sources

- [Route Inspect Interface Contract](interface.md)
- [route inspect Command Contract Set](_inspect.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Context Behavior Contract](../../context/behavior.md)
- [Status Behavior Contract](../../status/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Init Behavior Contract](../init/behavior.md)
- [Route Create Behavior Contract](../create/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [Find Behavior Contract](../../find/behavior.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Loading And Continuity](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Core Primitive Model](../../../../framework/primitives/model.md)
- [Open Forge Framework Architecture](../../../../framework/architecture.md)
- [Accepted State And Synchronization](../../../../framework/truth.md)
- [Open Forge Principles](../../../../principles.md)
