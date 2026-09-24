---
open-forge:
  description: Current technology-neutral closure, graph, projection, result, safety, and conformance behavior for `context`
  responsibility: Define how a context request resolves and forms one read-only result without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Context, Behavior, Determinism, Graph, Projection, CurrentTruth]
---

# Context Behavior Contract

## Status And Authority

This is the current Crystallized Behavior Contract for the accepted `context`
command. This file is authoritative for the technology-neutral resolution,
graph, projection, ordering, completeness, result, read-only safety, and
conformance mechanics behind the [Interface Contract](interface.md). It does
not add public flags, statuses, output fields, schemas, or implementation
choices. The command does not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md).

The [Technical Design](technical-design.md) is subordinate to the [CLI
Architecture](../../architecture.md) and records the accepted context
realization. It cannot redefine this behavior.

## Operation Invariants

- `context` is one stateless, deterministic, read-only operation. It does not
  infer relevant routes, create a session, read a receipt, or persist operation
  state.
- For unchanged workspace bytes, explicit source operands, and flags, request
  resolution forms the same selected physical layers, canonical order,
  projected content, findings, and semantic result.
- The operation resolves explicit sources and the Framework loading relationships
  they expose. It never turns route order, link proximity, or graph structure
  into relevance or authority.
- Human and structured presentations consume one completed typed result. Neither
  presentation reruns selection, parsing, projection, or verification.

## Request Resolution

### Workspace And Source Identity

Use the exact workspace selected by the shared [Global CLI Flags](../shared/global-flags/behavior.md)
contract: the exact current working directory unless an exact `--workspace`
value is supplied. Do not discover another workspace from parents, markers,
Git, route operands, or nearby `.agents` directories.

Use the shared [CLI Source References](../shared/source-references/behavior.md) contract for
source-ID and exact-path interpretation, collision handling, overwrite identity,
and canonical display. An exact path and a source ID identify sources in the
selected workspace; they do not select another workspace. Local link targets
must also remain physically contained by that workspace.

Cross-platform physical identity and containment prevent aliases from creating
duplicate or unsafe nodes. The CLI Architecture defines the accepted case,
Unicode, alias, symlink, hardlink, canonical-identity, and filesystem
realization; this behavior requires the safety property without changing it.

### Startup-Required Closure

With no explicit source, resolve the startup-required closure in the current
Framework loading order. The resolution is:

1. Read the canonical workspace entry and Loader.
2. Follow visible #LoadNow and #KeepInMind entries through loaded parents in
   generated order. For a loaded entrypoint, apply its child loading rules.
3. Place each valid overwrite companion immediately after its base.

Both tags use the same parent and scope boundaries. Neither activates an
otherwise unselected ancestor or scope. Inactive continuity metadata does not
make this closure incomplete solely because it exists elsewhere in the workspace.

### Explicit Route Closure

For each explicit source that has valid route meaning, resolve and add this
closure:

1. Missing parent entrypoints needed to establish the route and inherited
   `Axioms`.
2. The selected source.
3. Its overwrite companion when present.
4. Visible #LoadNow and #KeepInMind descendants exposed by each entrypoint in
   the selected ancestor chain.
5. Applicable scope-local loading required by the current Framework contract.

An explicit source does not activate unrelated sibling or descendant scopes.
Ordinary outgoing links are a separate expansion stage and remain absent unless
the request supplies `--follow-links`.

An exact path may identify a supported file without route meaning. Resolve and
include that file and its valid overwrite layer, but do not invent a parent route
chain, inherited `Axioms`, or other route facts. Preserve the exact-path and no-
established-route inclusion reason in the result.

### Several Sources

Resolve each routed source with its own parent chain, scope, authority, and
inclusion reasons. Union the resulting physical layers without creating a
combined scope. Preserve operand order and each route's internal parent, target,
#LoadNow, #KeepInMind, and overwrite order. If several relationships select one physical
source, retain every reason but do not use route order to resolve a conflict or
make a later selection higher authority.

### Invalid And Blocked Resolution

An unknown source reference forms `invalid-input`. An ambiguous or unsafe reference
forms `blocked`. A known source with unrelated structural findings may continue
only when the safe context and the missing boundary can both be stated. Do not
claim a complete result when an unresolved boundary could change the selected
context, its requested projection, or its safety.

## Additions Beyond Startup

When `--additions-only` is present, resolve the startup closure and the selected
route closure during the same invocation. Calculate their ordered set
difference, removing every source already in the startup-required closure. The
result contains only sources added by explicit routes and optional link
expansion, together with source blocks and findings needed to explain them.

At least one explicit source is required. Without one, `--additions-only` forms
`invalid-input` because there is no selected addition.

When link expansion is requested, expand the startup closure alone and the
startup-plus-route closure with the same depth before subtracting. A source
reachable from startup remains excluded even when the selected route also links
to it. Never compare with a previous invocation, receipt, retained caller
content, or hidden session state.

If the resolved difference contains no added sources, form the explicit completed
empty result defined by the [Interface Contract](interface.md#semantic-results).
This is valid only after startup and selected-route resolution completes; missing
all explicit sources with `--additions-only` remains invalid-input.

### Operation Flag Repetition

Normalize operation-specific flags before resolving a closure. Repeated
`--additions-only` occurrences collapse to one enabled selection and are
idempotent. A repeated `--follow-links` occurrence is invalid-input even when its value
matches the first occurrence. A repeated `--content` flag is invalid-input, while
repeated parts inside its single value are deduplicated without changing the
projection. Shared global flags and source-operand repetition use their linked
shared contracts. Invalid input stops before operation resolution.

## Content Graph

Every invocation builds an in-memory graph from the workspace sources needed by
the request. The graph contains:

- Parsed Markdown sources.
- Route and parent-child relationships.
- Loading relationships and inclusion reasons.
- Scope placement.
- Base and overwrite relationships.
- Named sections and source ranges.
- Explicit contained local Markdown links.

The graph is derived from human-readable files. It exists only for the current
invocation and is never persisted as workspace truth. Other operations may reuse
the same graph builder when they need these relationships. A quick operation such
as `status` should not build the complete graph.

The graph prevents duplicate parsing, keeps cycle handling consistent, and lets
`context`, tag search, checking, indexing, link repair, move, and remove share
one view of the Framework without creating a private authority.

The graph's invocation-local lifetime and non-persistence are behavior, not a
private storage contract. The CLI Architecture defines the source structure
that realizes this graph without changing its lifetime or authority boundary.

## Current Facts And Coverage

The operation may inspect the human-readable workspace sources required by the
resolved request and the relationships represented in the invocation graph. It
must preserve enough current facts to establish:

- The selected workspace and source identities.
- Route parents, selected targets, visible loading, scopes, authorities, and
  inclusion reasons.
- Logical sources and their base-first physical layers.
- Parsed headings, visible heading text, source forms, levels, canonical-form
  evidence, named sections, and section boundaries.
- Explicit local link destinations, incoming links, fragments, external links,
  containment, and resolution findings.
- Exact authored frontmatter and body bytes when those projections are selected.

Completeness is boundary-specific. A known missing section after every relevant
layer was inspected completely is a complete selection and inspection with a
`completed-with-warnings` result. An unreadable required layer, incomplete parse,
duplicate matching heading within one layer, missing link target, broken
fragment, invalid encoding, or ambiguous section prevents the operation from
claiming more complete coverage than the facts support. Unsafe or ambiguous
source identity, physical containment, ambiguous local target identity, or
overwrite identity or boundary prevents a safe boundary and forms `blocked`.
An exact target case mismatch is a safe `completed-with-warnings` observation. Safe sources
and independently safe observations may remain in the result beside unrelated
findings.

Projection must not hide a missing startup dependency or make an incomplete
addition look complete. A broken edge remains a finding even when other selected
sources are safe. The result retains the missing boundary needed to explain why
continuation is safe, incomplete, or blocked.

When several ordinary conditions occur, safety and coverage take precedence in
this order: `blocked`, then `incomplete`, then `completed-with-warnings`, then
`completed`.
Invalid input is rejected before operation work. An unexpected failure and a
caller interruption retain their `failed` and `cancelled` event meanings.

## Selection And Result Formation

### Closure Before Projection

Complete startup and explicit route resolution before ordinary link following.
Apply `--additions-only` to the appropriately resolved startup and selected sets.
Then form one ordered, deduplicated logical result with physical base and
overwrite layers. Content projection operates on that resolved result; it does
not rerun route selection, loading, link expansion, or completeness.

### Link Traversal

When requested, traverse only explicit local Markdown destinations. Resolve a
destination relative to its containing source and retain it only when its
physical target is inside the selected workspace. A link may target `.agents`
content or another contained local source. Record external HTTP and HTTPS links
as unchecked observations without selecting or fetching them.

Positive depths count link edges from the complete pre-expansion startup and
explicit-route seeds. Depth one adds direct targets. Depth two adds targets of
the depth-one sources. Higher positive values continue to that depth. `all`
follows the complete reachable local-link closure. Omitted link expansion adds no
ordinary links; zero is invalid.

Detect cycles and emit each source once. Retain every inclusion reason and
incoming link, and apply the same content projection to route-selected and
link-selected sources. Links in code fences or unsupported syntax that conceals
links are not graph edges. Markdown parsing and fragment handling follow the
accepted Framework Markdown and path contracts.

Record external HTTP and HTTPS destinations as unchecked observations. Never
fetch them or add their targets to the selected context; the absence of a fetch
does not make coverage incomplete. Cycles and duplicate targets do not change
status by themselves.

When a link directly names an overwrite companion, resolve the complete logical
source. Record the incoming link to the overwrite layer, then emit the base and
overwrite in that order. Never emit or select the overwrite alone.

A contained local target outside `.agents` is not an Open Forge source reference,
route, or managed source. It has no automatic ID, so the result uses the public
`ID: none` and canonical workspace-relative path behavior defined by the
[Interface Contract](interface.md#link-rules).

Missing targets, broken fragments, case mismatches, containment escapes, invalid
encodings, and ambiguous local targets remain visible findings. Broken edges are
not silently dropped. Classify unsafe or ambiguous identity and containment as
`blocked`, missing targets, broken fragments, and invalid encodings as
`incomplete`, and a safely established exact-target case mismatch as
`completed-with-warnings`, following the Interface precedence for combinations. Preserve safe
sources and observations where the boundary allows them.

### Projection

Apply the `--content` parts after selection and before rendering. The parts are
composable and idempotent, but repeated `--content` flags are invalid. The
complete public part values, escaping, defaults, invalid forms, and examples
remain in the [Interface Contract](interface.md#content-projection).

`metadata` is generated source metadata and is not authored frontmatter. It is a
projection of the `context` operation, not a standalone provenance operation;
`source` is not a content part.
`frontmatter` is the complete authored YAML block without normalization or
reconstruction. `body` is every Markdown byte after the frontmatter boundary,
including generated regions. `paths` is one operation-level ordered physical-
layer projection; it does not replace or simplify the resolved selection.

The projection stage emits `paths` first when selected, then walks logical
sources and physical layers in the already resolved order. Within each physical
layer, selected parts are emitted in this fixed order: generated `metadata`,
authored `frontmatter`, `headings`, authored `body`, and requested sections.
Requested sections are ordered by their document positions, not by flag order.
The stage preserves the Interface metadata-framing rules and exact authored
bytes; it does not rerun selection, link expansion, or completeness.

Heading projection and exact-section projection use the same structural heading
nodes, visible text, levels, source forms, and source ranges. Structural input
recognizes CommonMark ATX and Setext nodes, does not guess malformed headings,
and does not make every structurally accepted heading a canonical Framework
semantic section. Visible text contributes decoded inline content as described by
the Interface Contract. Comparison is complete, culture-independent, and
case-insensitive without Unicode normalization, slugs, fragments, substrings,
fuzzy matching, or semantic equivalence.

An exact section continues through the next parsed heading of the same or higher
level, or the document end. Repeated matching headings in one physical layer
make that layer's projection ambiguous rather than selecting an occurrence. A
matching base and overwrite section are both valid and remain separate layers.
When a requested section is proven absent after complete inspection, retain the
source and form the Interface-defined `completed-with-warnings` result. An unreadable or
incompletely parsed layer cannot prove absence and forms `incomplete` coverage.
Section projection changes only emitted authored content and its own projection
coverage; it does not change route, loading, or link-expansion completeness.

### Overwrite Layering

Normalize a base and adjacent `{name}.overwrite.md` companion as one logical
source with ordered layers. The base is first. Human authored body and section
output uses the Interface-defined overwrite separator; structured output keeps
base and overwrite rows separate with physical paths and shared route identity.
An orphan or ambiguous overwrite whose logical identity or boundary is unsafe or
ambiguous cannot establish a safe source boundary and forms `blocked`, never an
independent source.

### Ordering And Deduplication

Use Framework loading order for the startup-required closure. Append explicit
route closures in operand order, preserving each route's parent, target,
#LoadNow, #KeepInMind, and overwrite order. Append sources reached by following
links in stable breadth-first order by link depth, source order, and document
link order.

For every repeated relationship:

- Emit the source at its first canonical position.
- Retain every inclusion reason.
- Do not repeat authored content.
- Do not treat later selection as higher authority.

Paths, source identities, physical layers, document order, and inclusion reasons
remain distinct facts. The projection order defined by the Interface is stable
even when flag order changes.

## Results And Failures

Form one typed result with the exact seven status names and public meanings in
the [Interface Contract](interface.md#semantic-status). Behavior forms the
conditions; it does not create another status vocabulary.

- Form `completed` only when resolution, required inspection, requested
  relationships, and requested projections have no unresolved finding. A fully
  resolved zero-source additions difference and an empty heading outline are
  explicit completed results.
- Form `completed-with-warnings` when resolution and inspection are complete but a safe
  finding remains, including a requested section proven absent from one or more
  completely inspected logical sources or an exact target case mismatch.
- Form `incomplete` when safe content is available but a requested relationship
  or projection cannot be established completely, including missing local
  targets, broken fragments, invalid link encodings, unreadable required layers,
  incomplete source inspection, or ambiguous sections.
- Form `invalid-input` for command input or a selected route outside the accepted
  grammar, including an unknown reference, invalid `--content` grammar, invalid
  operation-flag repetition, `--additions-only` without a route, or zero link
  depth. Invalid input stops before operation resolution.
- Form `blocked` when a safe context boundary cannot be established, including
  ambiguous or unsafe source identity, containment, local-target identity, or
  overwrite identity or boundary.
- Form `failed` when an unexpected internal failure prevents normal completion.
- Form `cancelled` when the caller cancels or interrupts before completion.

For multiple ordinary conditions, choose the highest applicable safety or
coverage status in this order: `blocked`, `incomplete`, `completed-with-warnings`,
then `completed`. Preserve every independently safe source and observation that this
boundary allows, and never hide a broken edge. Failed and cancelled retain
their event meanings rather than being replaced by an ordinary finding status.
The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
exact numeric process exits, structured field names, schema version, and
compatibility. This Behavior Contract defines the semantic conditions that those
shared results represent.

The typed result includes the complete Interface-defined workspace, source,
closure, flag, count, finding, boundary, view, and content facts. It preserves
exact authored bytes, source order, layer order, inclusion reasons, and
projection coverage. Human and structured renderers consume that result without
rerunning the operation.

## Effects

`context` is read-only. It does not modify files, generated regions, links,
indexes, overwrites, or any other workspace state. It does not repair malformed
files, links, indexes, or overwrites, and it does not fetch external links.

The invocation graph is temporary in-memory derived context, not persistent
workspace truth. Read-only execution does not create a mutation plan, receipt,
session, cache key, or `--since` state.

## Safety And Recovery

Keep every workspace source and local link target within the selected workspace.
Do not use lexical or physical aliases to escape containment. Preserve the
Framework distinctions between route, loading, scope, source identity, physical
path, overwrite layer, link relationship, inclusion reason, and authority.

Do not follow links hidden in code fences or unsupported syntax, do not fetch
external HTTP or HTTPS destinations, and do not infer relevance from proximity,
tags, natural-language goals, or graph shape. Do not turn a link to an overwrite
into an independently selected layer.

When a boundary is safe and unrelated findings exist, return the safe sources
with the finding. When a boundary could change the claimed complete result or
cannot be safely established, preserve the incomplete or blocked condition and
the affected boundary. Never silently truncate, summarize, normalize, repair,
or choose among ambiguous sections or source identities.

## Presentation Relationship

The [Interface Contract](interface.md) is the complete public owner of human
output, structured output, semantic status, error content, and every example.
This behavior produces one typed result containing those facts. Minimal,
standard, full, debug, and JSON renderers consume that result; they do not rerun
selection, parse different content, or
change status, completeness, safety, or process meaning. `--detail` changes only
human density, and `--format json` uses the complete structured result under the shared
global contract. Primary human completed, completed-with-warnings, and incomplete results are
written to stdout; invalid-input, blocked, failed, and cancelled primary human
results are written to stderr. JSON writes one structured result to stdout for
every status, while bounded diagnostics use stderr and never mix human text into
JSON stdout.

## Conformance Evidence

The mandatory caller-visible coverage is listed in the [Interface Contract](interface.md#verification).
Technology-neutral conformance should additionally establish the mechanics
behind those facts:

- Direct tests should prove invocation graph construction, startup and explicit
  closure selection, startup-relative additions, link-depth traversal and cycle
  termination, projection formation, base/overwrite layering, ordering,
  deduplication, completeness, repetition rules, canonical mixed projection
  order, empty results, external unchecked observations, safety/coverage
  precedence, and each semantic status condition.
- Focused integration tests should use real temporary workspaces to cross the
  filesystem, route, Markdown, overwrite, source-identity, link, and containment
  boundaries named by the Interface Contract.
- Repeat invocations with unchanged input should produce the same semantic
  result without relying on retained state.
- Human and structured renderers should consume one result, including partial
  safe output and findings, without rerunning operation stages.

Gate 5 executable proof must cover parsing, output, exit behavior, and packaged
execution, including the accepted Native AOT boundary. The [Technical
Design](technical-design.md) records that evidence shape without claiming that a
suite, schema, exit mapping, or packaged implementation already exists.

## Related Current Sources

- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Context Interface Contract](interface.md)
- [Context Technical Design](technical-design.md)
- [Context Command Contract Set](_context.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [CLI Architecture](../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Loading And Refreshing Context](../../../framework/routing/loading.md)
- [Routing Model](../../../framework/routing/model.md)
- [Route Scope And Inheritance](../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../framework/routing/paths.md)
- [Overwrite Customization](../../../framework/routing/overwrites.md)
