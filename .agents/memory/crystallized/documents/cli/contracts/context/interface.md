---
open-forge:
  description: Current public interface and observable result for stateless ordered `context` resolution, projection, and explicit link expansion
  responsibility: Define what a context caller may enter and observe without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Context, Interface, Route, Scope, Projection, Graph, CurrentTruth]
---

# Context Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for the accepted `context`
command. This file is authoritative for the command's public grammar, selected
content, observable result, errors, non-goals, examples, and verification. The
command does not ship yet. Implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md). The sibling Behavior Contract defines the technology-neutral operation,
and the Technical Design is subordinate to the [CLI Architecture](../../architecture.md)
for the accepted context realization.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact shared structured result schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines source and runtime boundaries and
filesystem structure. The [Technical
Design](technical-design.md) records the accepted context-specific realization;
neither source can redefine this Interface Contract.

The [Framework loading contract](../../../framework/routing/loading.md)
defines loading behavior. The [routing model](../../../framework/routing/model.md),
[scope rules](../../../framework/routing/scope.md),
[path rules](../../../framework/routing/paths.md), and
[overwrite rules](../../../framework/routing/overwrites.md)
define the Framework meaning consumed by this command.

The shared [CLI Source References](../shared/source-references/interface.md) contract defines
automatic IDs, exact `.agents` paths, quoting, collisions, and disambiguation.
The shared [Global CLI Flags](../shared/global-flags/interface.md) contract defines the global
flags used here. Those sources remain authoritative for their complete shared
meaning; this file records only how `context` uses them.

## Purpose

`context` returns ordered Open Forge context from an exact workspace and zero or
more explicit routes. It makes required and selected content easier to retrieve
and read. It does not infer relevant routes, modify files, or create session
state.

Given the same workspace bytes, routes, and flags, the command returns the same
selected sources, order, content, findings, and status.

## Syntax

The complete public command form is:

```text
open-forge context [source-reference...]
  [--additions-only]
  [--content=<part>[,<part>...]]
  [--follow-links=<positive-depth|all>]
  [global flags]
```

Source references are separate positional operands:

```text
open-forge context route-a .agents/directives/security.md
```

Do not encode several sources as a comma-separated flag. A separate operand
keeps each source independently parsable, completable, and reportable.

Every source operand follows the shared [CLI Source References](../shared/source-references/interface.md)
contract. That contract owns the automatic-ID and exact-path forms, quoting,
collisions, and disambiguation rather than this command creating a second source
grammar.

`context` has no child operation, alias, additional global flag, or alternate
spelling. The six shared global flags, `--workspace`, `--json`, `--view`,
`--verbose`, `--help`, and `--version`, apply under the shared contract.

### Operands

| Operand               | State                                    | Accepted value                                           | Omission                             | Repetition and order                                                                                                                                                                                              |
| --------------------- | ---------------------------------------- | -------------------------------------------------------- | ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `source-reference...` | Optional; required by `--additions-only` | One shared source ID or exact `.agents` path per operand | Resolve the startup-required closure | Each source is a separate shell value. Route closures follow operand order. Repeated or overlapping physical sources are emitted once at their first canonical position while every inclusion reason is retained. |

The shared source-reference contract defines the complete accepted grammar and
identity behavior for each operand. Context-specific closure and repetition
behavior is defined below.

### Operation-Specific Flags

| Flag                                   | Role       | Accepted value                                            | Omission                                                                    | Repetition, ordering, and composition                                                                                                                                 |
| -------------------------------------- | ---------- | --------------------------------------------------------- | --------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--additions-only`                     | Selection  | Boolean flag with no value                                | Include the startup-required closure together with explicit route additions | Requires at least one explicit source. Repetition is accepted and idempotent.                                                                                         |
| `--content=<part>[,<part>...]`         | Projection | One comma-separated content value using the grammar below | `frontmatter,body`                                                          | Parts are composable and idempotent. Repeat a part without additional effect. The flag itself may not be repeated. Flag order does not change canonical output order. |
| `--follow-links=<positive-depth\|all>` | Selection  | A positive integer depth or `all`                         | Do not follow ordinary outgoing links                                       | Zero is invalid. Repetition is invalid, including repeated values that are equal and values that conflict.                                                            |

The shared global flags retain their shared defaults, repetition rules, terminal
behavior, output behavior, and errors. In particular, `--view` changes only
human presentation, `--json` selects one structured result, and `--verbose`
adds bounded diagnostic detail under that shared contract.

## Workspace

`context` is workspace-aware. The shared [Global CLI Flags](../shared/global-flags/interface.md)
contract defines exact current-working-directory and `--workspace <path>`
selection, path resolution, reporting, and the prohibition on workspace
discovery. The selected workspace and selection method are part of every
workspace-aware result.

## Context Resolution

### No Explicit Source

```text
open-forge context
```

With no source operand, the command returns the sources required at task start or
resume. This set is called the startup-required closure below. Resolve it in the
Framework loading order:

1. Read the canonical workspace entry and Loader.
2. Follow visible #LoadNow and #KeepInMind entries through loaded parents in
   generated order. For a loaded entrypoint, apply its child loading rules.
3. Place each valid overwrite companion immediately after its base.

Both tags use the same parent and scope boundaries. Neither activates an
otherwise unselected ancestor or scope. Inactive continuity metadata does not
make this closure incomplete solely because it exists elsewhere in the workspace.

### Explicit Sources

```text
open-forge context memory/crystallized/documents
```

Each explicit reference resolves to a source. When that source has valid route
meaning, it adds its selected closure to the startup-required closure:

1. Missing parent entrypoints needed to establish its route and inherited
   `Axioms`.
2. The selected source itself.
3. Its overwrite companion when present.
4. Visible #LoadNow and #KeepInMind descendants exposed by each entrypoint in
   the selected ancestor chain.
5. Applicable scope-local loading required by the current Framework contract.

An explicit source does not activate unrelated sibling or descendant scopes.
Ordinary outgoing links are not followed unless `--follow-links` is supplied.

An exact path may identify a supported file that is not routed. In that case,
the command includes the file and its overwrite layer if valid, but does not
invent a parent route chain or inherited `Axioms`. The source block states that
the file was selected by exact path and has no established route.

### Several Sources

```text
open-forge context \
  memory/project-a/crystallized/documents \
  directives/security
```

Each routed source keeps its own parent chain, scope, authority, and inclusion
reasons. The command deduplicates the same physical source while retaining every
reason that included it. It does not create a combined scope or use route order
to resolve a conflict.

### Source Errors

An unknown reference is invalid. An ambiguous or unsafe reference is blocked. A
known source with unrelated structural findings may still return safe context
with an `attention` or `incomplete` result, provided the command states the
missing boundary clearly.

## Additions Beyond Startup

```text
open-forge context route-a --additions-only
```

`--additions-only` removes sources already present in the startup-required
closure and returns only sources added by explicit routes and optional link
expansion.

The calculation is stateless. The command resolves the startup and selected
closures during the same invocation and calculates their ordered set
difference. It does not compare with a previous invocation, read a receipt, or
assume that the caller retained earlier content.

Without at least one explicit source, `--additions-only` is invalid because the
result would have no selected addition.

When link expansion is enabled, the command expands the startup closure alone
and the startup-plus-route closure with the same depth. It then subtracts the
expanded startup set. A source reachable from startup does not become a route
addition only because a selected route also links to it.

If that fully resolved difference contains zero added sources, the command
returns an explicit `complete` empty result. This is distinct from the invalid
request that omits every explicit source while using `--additions-only`.

Source blocks and findings needed to explain an added source remain in the
result. The projection must not hide a missing startup dependency or make an
incomplete addition look complete.

## Content Projection

### Flag

```text
--content=<part>[,<part>...]
```

The flag selects which result content parts to display for every selected
source. Its parts are composable and idempotent. Repeating a part has no
additional effect. Their order in the flag does not change canonical output
order.

The resolved projection has one canonical order independent of flag order:

1. Emit the operation-level `paths` projection first when `paths` is selected.
2. Emit each selected logical source and its physical layers in resolved order.
3. Within each physical layer, emit selected parts in this order: generated
   `metadata`, authored `frontmatter`, `headings`, authored `body`, and then
   requested sections.

Requested sections follow document order within each physical layer, not the
order in which their parts appeared in `--content`. The default expanded
metadata framing remains around each emitted layer even when `metadata` was not
selected explicitly, except that a paths-only projection replaces those repeated
per-source framing blocks. This framing is CLI-generated and does not alter the
exact authored bytes returned by `frontmatter`, `body`, or a section.

Repeating the `--content` flag is invalid. Compose all requested parts in one
comma-separated value.

Supported parts:

| Part             | Result                                                          |
| ---------------- | --------------------------------------------------------------- |
| `metadata`       | CLI-generated source metadata without authored content          |
| `paths`          | Ordered physical-layer path projection without authored content |
| `frontmatter`    | Complete authored YAML frontmatter                              |
| `headings`       | Parsed heading outline without section bodies                   |
| `body`           | Complete Markdown after frontmatter                             |
| `section:<name>` | Exact named Markdown section from each source                   |

The default is:

```text
--content=frontmatter,body
```

This returns complete authored source content without requiring a flag.

`metadata`, `paths`, and `headings` are projections of the `context` operation.
The CLI does not expose standalone provenance or outline commands, and `source`
is not a content part. CLI-generated source metadata uses `metadata`.

### Source Metadata

In the default expanded view, each emitted layer has a labelled source metadata
block, even when `metadata` is not selected explicitly:

```text
Path
ID
Route
Scope
Inclusion reason
Order
Layer
```

`Layer` is `base` or `overwrite`. The block keeps output from multiple sources
understandable. It is CLI-generated result metadata, not authored YAML
frontmatter or file content. `--content=metadata` emits only these generated
metadata blocks; the standard result summary and related findings remain.

Compact view renders one token-friendly row per physical layer with sequence,
ID, path, and layer. It omits optional route, scope, and inclusion explanation;
required completeness findings and next actions remain visible. Expanded view
retains every inclusion reason and provenance field shown above. JSON retains the
complete typed metadata regardless of human view.

### Ordered Paths

```text
open-forge context --content=paths
```

`paths` emits the exact ordered physical layers selected by the context resolver
without authored frontmatter, headings, bodies, or sections. It does not rerun
or simplify source selection. Startup closure, explicit sources,
`--additions-only`, link expansion, deduplication, base-then-overwrite order,
findings, and completeness remain unchanged.

Compact human view emits one canonical workspace-relative path per line in
context order:

```text
.agents/loader.md
.agents/memory/_memory.md
.agents/memory/working/_working.md
```

Expanded human view adds sequence, ID, layer, inclusion category, and every
inclusion reason:

```text
1  .agents/loader.md
   ID: loader
   Layer: base
   Included because: startup

2  .agents/memory/_memory.md
   ID: memory
   Layer: base
   Included because: #LoadNow from .agents/loader.md
```

Structured output retains the same ordered rows and complete provenance
regardless of human view. A complete empty path set is represented explicitly;
an unmet closure never appears complete.

When `paths` is the only content part, the path projection replaces repeated
per-source metadata blocks. When it is combined with another content part, the
ordered path projection appears once before the normal per-source blocks and
selected content. Combining `paths` and `metadata` emits the ordered path list
and the complete generated source blocks.

For example:

```text
open-forge context \
  memory/project-alpha/crystallized/documents \
  --additions-only \
  --content=metadata
```

returns metadata blocks without frontmatter or bodies:

```text
Open Forge context
Workspace: D:/work/example
Selected by: current directory
Result: complete
Startup context included: no (--additions-only)
Content: metadata
Sources: 3 logical sources, 4 emitted layers

Path: .agents/memory/project-alpha/_project-alpha.md
ID: memory/project-alpha
Route: memory/project-alpha
Scope: project-alpha
Inclusion reason: ancestor of the selected source
Order: 1
Layer: base

Path: .agents/memory/project-alpha/crystallized/_crystallized.md
ID: memory/project-alpha/crystallized
Route: memory/project-alpha/crystallized
Scope: project-alpha
Inclusion reason: ancestor of the selected source
Order: 2
Layer: base

Path: .agents/memory/project-alpha/crystallized/documents/_documents.md
ID: memory/project-alpha/crystallized/documents
Route: memory/project-alpha/crystallized/documents
Scope: project-alpha
Inclusion reason: selected source
Order: 3
Layer: base

Path: .agents/memory/project-alpha/crystallized/documents/_documents.overwrite.md
ID: memory/project-alpha/crystallized/documents
Route: memory/project-alpha/crystallized/documents
Scope: project-alpha
Inclusion reason: overwrite companion of the selected source
Order: 4
Layer: overwrite
```

The paths and values are illustrative. Without `--additions-only`, the result
also includes the task-start context before route additions. With no explicit
source, `--content=metadata` returns metadata blocks for the task-start context.

### Frontmatter

```text
open-forge context --content=frontmatter
```

`frontmatter` returns the complete authored metadata block. It does not return a
normalized or reconstructed replacement. Missing or malformed required metadata
remains visible as a finding.

### Body

```text
open-forge context --content=body
```

`body` returns all Markdown after the frontmatter boundary. It does not silently
truncate, summarize, or remove generated regions.

### Headings

```text
open-forge context directives/public-facing-writing --content=headings
```

`headings` returns the parsed heading outline for every emitted physical layer
without returning section bodies. It uses the same structural heading nodes,
visible text, levels, source forms, and source ranges as exact section
projection.

Compact human view emits one authored heading per line in document order with
enough indentation or heading markers to preserve level:

```text
# Writing
## Instructions
```

Expanded human view adds the physical layer, 1-based line, heading level, source
form, and canonical-authoring status:

```text
base  line 7  level 1  ATX  canonical  Writing
base  line 9  level 2  ATX  canonical  Instructions
```

Structured output retains the complete heading facts regardless of the selected
human view. Duplicate visible headings remain separate occurrences. A layer with
no headings returns a complete empty outline rather than a missing-section
finding.

### Exact Sections

```text
open-forge context --content=section:Axioms
```

`section:<name>` returns the complete parsed Markdown section whose structural
heading has visible text matching `<name>`. The heading and its content continue
until the next parsed heading of the same or higher level, or the end of the
document.

The operation recognizes CommonMark ATX and Setext heading nodes, levels, source
forms, and source ranges. No other public heading form is accepted. The CLI does
not guess a heading from malformed text that is not represented as an accepted
heading node.

Visible heading text is the concatenation of parsed inline content as a reader
sees it. Text, decoded character entities, soft breaks, hard breaks, and inline
code content contribute visible text. Emphasis and strong markers do not. Link
and image labels contribute visible text; destinations and titles do not. Raw
inline HTML and HTML blocks do not contribute visible heading text. Consecutive
whitespace created by heading inline structure is collapsed to one ASCII space,
and leading or trailing whitespace is removed before comparison.

Names match the complete visible heading text through ordinal,
culture-independent case-insensitive comparison. Matching ignores case but does
not apply Unicode normalization, slugs, fragments, substrings, fuzzy matching,
or semantic equivalence. The result preserves authored spelling and identifies
the heading level, source form when available, and whether it satisfies
canonical Open Forge ATX syntax. The accepted implementation realization is
recorded in the [Technical Design](technical-design.md).

Broader structural parsing does not make every accepted heading form a canonical
Framework semantic section. Component contracts still decide whether a heading
such as `Axioms` or `Instructions` has active Framework meaning.

Quote the complete shell value when a section name contains spaces:

```text
open-forge context --content="section:Current State"
```

Comma separates content parts. A backslash escapes a comma or another backslash
inside a section name:

```text
open-forge context --content="section:Rules\, Limits,body"
```

This selects the exact `Rules, Limits` section and the complete body.

Several sections compose in one flag:

```text
open-forge context \
  --content=section:Axioms,section:Instructions
```

The result preserves source order and document order. Repeating the same section
part does not duplicate output.

When several requested sections match one physical layer, their emitted sections
follow that layer's document order rather than the order in `--content`.

When neither layer contains a requested section, the source remains present with
a named missing-section finding. If every relevant layer was inspected
completely, the known absence produces `attention`: selection and inspection are
complete, but the requested projection is not available for that source. This
rule also applies when one of several requested sections is known absent or one
of several selected sources lacks the section.

A matching base section and matching overwrite section are both valid and emit
in layer order with separate physical paths. When more than one heading within
the same physical layer matches the requested name, that layer's section
projection is ambiguous and the result is `incomplete` rather than choosing one
occurrence. An unambiguous section in the other layer may still be emitted with
that finding. An unreadable layer or incomplete parse is also `incomplete`
because the command cannot prove absence.

Compact human output retains result and projection coverage plus each affected
source and missing or ambiguous section name. Expanded and structured output add
layer, location, evidence, and every independently available section.

Section projection does not change route selection, loading, or link-expansion
completeness. It has its own projection coverage and may change semantic status
as defined above. It changes only emitted authored content.

### Valid Combinations

```text
--content=metadata
--content=paths
--content=frontmatter
--content=headings
--content=body
--content=frontmatter,body
--content=section:Axioms
--content=headings,section:Axioms
--content=paths,headings
--content=frontmatter,section:Axioms
--content=section:Axioms,section:Instructions
```

Empty parts, unknown parts, or a section with an empty name are invalid.
Whitespace around comma-separated parts is ignored. Whitespace inside a section
name is preserved. A trailing escape is invalid. An escape before any character
other than a comma or backslash is invalid. An unquoted shell value that becomes
several arguments is invalid.

`metadata` is redundant when another per-source authored or derived part is
present because the identity block is always emitted, but accepting the
combination keeps the projection grammar composable and harmless:

```text
--content=metadata,body
```

`paths` is similarly composable but remains an operation-level ordered projection
rather than authored content from each source.

## Explicit Link Expansion

### Flag

```text
--follow-links=<positive-depth|all>
```

The flag adds explicit local Markdown link targets to the selected source set. It
changes source selection, not content projection.

Examples:

```text
open-forge context route-a --follow-links=1
open-forge context route-a --follow-links=2
open-forge context route-a --follow-links=all
```

Positive integers count link edges from sources selected before link expansion.
Those seed sources are the complete startup and explicit-route closures before
ordinary links are added:

- `1` adds direct local link targets.
- `2` also adds direct local links from the depth-1 targets.
- Higher values continue to the stated depth.
- `all` follows the complete reachable local-link closure.

Zero is invalid because it would normally mean no traversal. Omitting the flag
already means that no ordinary links are followed.

### Link Rules

Link expansion:

- Follows explicit local Markdown destinations only.
- Resolves each destination relative to the containing source.
- Keeps every destination physically inside the selected workspace.
- May target `.agents` content or another contained local source.
- Reports external HTTP and HTTPS URLs as unchecked observations, never fetches
  them, and never selects their targets as context. No-fetch alone does not
  weaken completeness.
- Detects cycles.
- Emits each source once.
- Retains every inclusion reason and incoming link.
- Uses the same content projection for route-selected and link-selected sources.
- Does not rank, search for, or infer related content.

Cycles and duplicate links do not change the semantic status by themselves.

Links in code fences or unsupported syntax that conceals links are not graph
edges.

Exact Markdown parsing and fragment handling follow the accepted Framework
Markdown and path contracts.

A link that directly names an overwrite companion resolves to the complete
logical source. The graph records the incoming link to the overwrite layer, then
emits the base followed by its overwrite. It never emits or selects the overwrite
alone.

A contained local link may point outside `.agents`. That target has no automatic
Open Forge source ID. Its source block reports:

```text
ID: none
Path: <canonical workspace-relative path>
```

Structured output uses a null ID and the canonical workspace-relative path. Such
a target is link-expanded content, not a source-reference operand, route, or
managed Open Forge source.

### Link Findings

A missing target, broken fragment, case mismatch, containment escape, invalid
encoding, or ambiguous local target remains visible. The command never silently
drops a broken edge. The status classification and precedence for these and
other findings are defined under [Status Classification](#status-classification).

Safe sources may still be returned beside unrelated broken links. The result
preserves every independently safe source and observation allowed by the finding
boundary. It never hides a broken edge.

## Overwrites

A base and adjacent `{name}.overwrite.md` companion form one logical source with
ordered layers. The overwrite is never independently selected, indexed, or
followed as another route.

The base is emitted first. When bodies or sections include overwrite content,
human output uses a clear boundary:

```text
<base content>

=== Overwrite ===

<overwrite content>
```

Structured output keeps base and overwrite as separate ordered layers with their
physical paths and shared route identity.

An orphan or ambiguous overwrite cannot establish valid route meaning. It
produces a `blocked` result when its logical identity or boundary is unsafe or
ambiguous rather than an independent source.

## Ordering And Deduplication

The startup-required closure follows Framework loading order. Explicit route
closures follow operand order while preserving each route's internal parent,
target, #LoadNow, #KeepInMind, and overwrite order. Link-expanded sources are appended in
stable breadth-first order by link depth, source order, and document link order.

When several relationships select the same source:

- Emit it at its first canonical position.
- Record every inclusion reason.
- Do not repeat its authored content.
- Do not use later selection as higher authority.

Canonical path identity and physical containment prevent aliases from producing
duplicate or unsafe nodes. The CLI Architecture defines the accepted
cross-platform identity and containment realization.

## Results And Failures

### Status Classification

The command classifies ordinary findings deterministically:

| Condition                                                                                                                                                                            | Semantic status |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------- |
| Unsafe or ambiguous source or local-target identity, workspace containment, or overwrite identity or boundary                                                                        | `blocked`       |
| Missing local target, broken fragment, invalid link encoding, unreadable required layer, incomplete required parse, or ambiguous section                                             | `incomplete`    |
| A completely inspected safe observation, such as a requested section proven absent or an exact target case mismatch                                                                  | `attention`     |
| No unresolved condition, including a fully resolved zero-source `--additions-only` difference, an empty heading outline, external unchecked observations, cycles, or duplicate links | `complete`      |

Invalid request input, including invalid repetition, remains `invalid` and stops
before operation resolution. An unexpected failure retains `failed`, and caller
interruption retains `interrupted`. When multiple ordinary conditions occur,
`blocked` takes precedence over `incomplete`, `incomplete` over `attention`, and
`attention` over `complete`. The command preserves every independently safe
source and observation allowed by that boundary and never hides a broken edge.

### Semantic Status

The command returns one result status:

| Status        | Meaning                                                                                                                                                                                                                                                                |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Resolution completed and the requested context has no unresolved finding; a fully resolved empty difference or empty heading outline is an explicit complete result                                                                                                    |
| `attention`   | Resolution and inspection completed with safe findings, including a requested section proven absent from one or more completely inspected logical sources or an exact target case mismatch                                                                             |
| `incomplete`  | Safe content is available, but one or more requested relationships or projections could not be established completely, including missing local targets, broken fragments, invalid link encodings, unreadable required layers, incomplete parses, or ambiguous sections |
| `invalid`     | Command input or a selected route does not follow the accepted grammar; invalid input stops before operation resolution                                                                                                                                                |
| `blocked`     | The command could not establish a safe context boundary because source or local-target identity, containment, or overwrite identity or boundary is unsafe or ambiguous                                                                                                 |
| `failed`      | An unexpected internal failure prevented normal completion                                                                                                                                                                                                             |
| `interrupted` | The caller cancelled or interrupted the operation before completion                                                                                                                                                                                                    |

The shared process-status mapping is defined by the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

### Required Summary

Every result reports:

- Selected workspace and selection method.
- Explicit source references and their resolved IDs and paths.
- Whether the startup-required closure was included.
- Whether `--additions-only` was applied.
- Requested content parts.
- Requested link depth.
- Ordered source count.
- Findings and missing boundaries.

Default expanded human output provides labelled framing, reasons, and provenance
before source content. Compact human output uses token-friendly rows and
preserves required status, completeness, safety, and next actions. Selected
authored content bytes remain exact in both views. JSON exposes the same complete
facts and source order from the same typed result.

### Exact Schema-v1 Command-Local Result

The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
schema-v1 envelope wraps this command-local `result` object.
The envelope remains exactly `{ schemaVersion, command, status, workspace,
result, next }`; its aggregate `status`, workspace, and next action are not
duplicated below. The shared `SourceLocation` primitive is the same authority's
exact `{ line, column, byteOffset, byteLength }` shape.

The following camel-case grammar lists every command-local member in wire order.
No member is omitted.

```text
type ContextResult = {
  selection: Selection;
  presentation: Presentation;
  coverage: Coverage;
  paths: PathProjection[];
  links: Link[];
  sources: Source[];
  findings: Finding[];
};

type Selection = {
  requestedSources: RequestedSource[];
  startupIncluded: boolean;
  additionsOnly: boolean;
  linkExpansion: LinkExpansion;
  sourceCount: nonnegative-integer | null;
};

type RequestedSource = {
  supplied: string;
  form: "source-id" | "source-path";
  resolution: "resolved" | "invalid" | "unknown" | "ambiguous" | "unsupported" | "unsafe";
  source: SourceIdentity | null;
  routeState: RouteState | null;
  candidates: SourceIdentity[];
};

type SourceIdentity = {
  id: string | null;
  path: string;
};

type LinkExpansion = {
  mode: "none" | "bounded" | "all";
  depth: positive-integer | null;
};

type Presentation = {
  view: {
    supplied: "compact" | "expanded" | null;
    effective: "compact" | "expanded";
  };
  content: {
    supplied: CanonicalPart[];
    effective: CanonicalPart[];
  };
};

type Coverage = {
  state: CoverageState;
  selection: CoverageState;
  links: OptionalCoverageState;
  projection: CoverageState;
};

type PathProjection = {
  position: positive-integer;
  sourcePosition: positive-integer;
  id: string | null;
  path: string;
  layer: SourceLayer;
  inclusionReasons: InclusionReason[];
};

type Source = {
  position: positive-integer;
  id: string | null;
  path: string;
  routeState: RouteState;
  route: string | null;
  scope: string | null;
  inclusionReasons: InclusionReason[];
  layers: Layer[];
};

type Layer = {
  pathPosition: positive-integer;
  kind: SourceLayer;
  path: string;
  inclusionReasons: InclusionReason[];
  projections: Projection[];
};

type InclusionReason = {
  kind: "workspace-entry" | "loader" | "load-now" | "keep-in-mind" | "ancestor-required" | "selected-source" | "scope-local" | "linked-source" | "overwrite-companion";
  source: SourceIdentity | null;
  reference: string | null;
  depth: positive-integer | null;
  location: SourceLocation | null;
};

type Projection = {
  part: "frontmatter" | "headings" | "body" | "section";
  name: string | null;
  state: "available" | "missing" | "unavailable" | "ambiguous";
  text: string | null;
  headings: ProjectedHeading[];
  location: SourceLocation | null;
};

type ProjectedHeading = {
  text: string;
  level: positive-integer;
  form: "atx" | "setext";
  location: SourceLocation;
  canonical: boolean;
};

type Link = {
  depth: positive-integer;
  source: {
    id: string | null;
    path: string;
    layer: SourceLayer;
  };
  location: SourceLocation;
  destinationLocation: SourceLocation | null;
  rawDestination: string;
  fragment: string | null;
  target: {
    kind: "local" | "external" | "unsupported";
    id: string | null;
    path: string | null;
    layer: SourceLayer | null;
    resolution: "complete" | "missing" | "fragment-missing" | "case-mismatch" | "malformed" | "absolute" | "query" | "encoding-unsupported" | "outside-workspace" | "physical-escape" | "ambiguous" | "unreadable" | "unsupported" | "external-unchecked";
    network: "network-not-attempted" | null;
  };
  disposition: "selected" | "already-selected" | "cycle" | "external-unchecked" | "unresolved";
};

type Finding = {
  code: ContextFindingCode;
  status: SharedStatus;
  subject: string | null;
  cause: string;
  reference: string | null;
  source: SourceIdentity | null;
  layer: SourceLayer | null;
  path: string | null;
  part: CanonicalPart | null;
  location: SourceLocation | null;
  destinationLocation: SourceLocation | null;
  candidates: SourceIdentity[];
};

type RouteState = "routed" | "unrouted" | "ambiguous" | "unavailable";

type SourceLayer = "base" | "overwrite";

type CoverageState =
  "not-started" | "complete" | "incomplete" | "blocked" | "failed" | "interrupted";

type OptionalCoverageState = CoverageState | "not-requested";

type CanonicalPart =
  "metadata" | "paths" | "frontmatter" | "headings" | "body" | "section:<name>";

type SharedStatus =
  "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
```

`requestedSources` preserves operand order. `supplied` is the exact value after
shell parsing. `form`, `resolution`, `source`, `routeState`, and `candidates`
retain the shared source-reference facts even when resolution cannot continue.
`candidates` is always present and is empty unless ambiguity evidence exists.
An operand-free request uses an empty array.

`startupIncluded` states whether startup sources are emitted in the result. It
is false for `--additions-only` and for invalid input that stops before
resolution. `additionsOnly` retains the normalized Boolean request.
`linkExpansion.mode` is `none` when the flag is omitted, `bounded` for a positive
integer, and `all` for the complete reachable closure. `depth` is non-null only
for `bounded`. `sourceCount` is the known nonnegative ordered logical-source
count when selection is established and `null` otherwise. A complete empty
additions difference uses zero. Physical-layer cardinality remains derivable
from `sources[].layers`.

`view.supplied` is null when omitted, and `view.effective` is always `compact` or
`expanded`. `content.supplied` preserves parsed part order. `content.effective`
uses the canonical order `metadata`, `paths`, `frontmatter`, `headings`, `body`,
then requested sections in their first supplied order. Omission uses an empty
`supplied` array and effective `frontmatter,body`. Both arrays use canonical
`section:<name>` strings after list escaping is resolved.

`coverage.state` is the complete operation coverage. `selection` covers startup,
explicit-route, additions, ordering, and identity work. `links` is
`not-requested` when link expansion is omitted. `projection` covers every
requested effective content part. A known missing requested section has complete
projection coverage and an `attention` finding. Unavailable or ambiguous content
has incomplete projection coverage. Coverage does not repeat the aggregate
semantic status; attention is complete coverage with a safe finding.

`paths` contains the operation-level physical-layer rows only when `paths` is
selected and is otherwise empty. Its `position` is the 1-based global physical
layer order, and `sourcePosition` is the 1-based logical-source position in `sources`.
The array retains complete provenance even when paths are the only selected
content part.

`sources` uses first canonical selection position. `id` is null only for the
workspace entry or contained linked content outside `.agents`. `path` is the
canonical workspace-relative base path. `route` is non-null only for one
established unambiguous route. `scope` is non-null only when an applicable
Framework component contract supplies explicit scope evidence; Context never
infers a semantic scope name from path shape or tags. Every source and layer
retains all independently established inclusion reasons in discovery order.

An inclusion reason's `source` is the established parent, target, seed, or base
logical identity when that relationship has one. `reference` is non-null only
for an explicit source operand. `depth` and `location` are non-null only for a
linked-source reason. The overwrite reason belongs to the overwrite layer; the
logical source remains one base-first identity.

Layer `pathPosition` equals the same 1-based global physical path position used
by `paths[].position`. `projections` contains only requested authored or derived
layer projections, in canonical part and document order. Generated metadata is
represented by the always-present source, layer, and inclusion-reason members
rather than a duplicate projection payload. The operation-level path projection
is represented by `paths`.

Projection `name` is non-null only for `section`. `text` is non-null only for an
available `frontmatter`, `body`, or `section`, and an empty string is valid.
`headings` is populated only for an available heading outline and is otherwise
empty. `location` applies to available authored text; every projected heading
owns its own location. Missing is a proven absence, unavailable means required
inspection did not complete, and ambiguous means one requested section matched
several headings in the same layer.

`links` retains every inspected graph edge in stable breadth-first order,
including edges whose targets were already selected, cycles, external unchecked
observations, and unresolved edges. `depth` counts from the complete pre-expansion
seed set. `disposition` explains why the edge did or did not add one source.
External HTTP and HTTPS targets alone use `external-unchecked` together with
`network-not-attempted` and never create a finding.

Finding members are always present. `subject` and `cause` are bounded escaped
strings; `cause` is never null. `reference`, `source`, `layer`, `path`, `part`,
locations, and candidates retain typed evidence when applicable and are null or
empty otherwise. A finding never exposes exception identity or unbounded source
content.

#### Finding Codes And Ordering

Context has exactly the following finding vocabulary. Each code has only the
status shown, and this table order is the primary finding order.

| Machine code                     | Finding status |
| -------------------------------- | -------------- |
| `context.invalid-input`          | `invalid`      |
| `context.invalid-source`         | `invalid`      |
| `context.invalid-content`        | `invalid`      |
| `context.invalid-link-depth`     | `invalid`      |
| `context.workspace-unavailable`  | `blocked`      |
| `context.workspace-unsafe`       | `blocked`      |
| `context.source-ambiguous`       | `blocked`      |
| `context.source-unsafe`          | `blocked`      |
| `context.overwrite-ambiguous`    | `blocked`      |
| `context.target-ambiguous`       | `blocked`      |
| `context.target-unsafe`          | `blocked`      |
| `context.closure-unavailable`    | `incomplete`   |
| `context.layer-unavailable`      | `incomplete`   |
| `context.invalid-encoding`       | `incomplete`   |
| `context.markdown-unavailable`   | `incomplete`   |
| `context.target-missing`         | `incomplete`   |
| `context.fragment-missing`       | `incomplete`   |
| `context.link-encoding-invalid`  | `incomplete`   |
| `context.target-unreadable`      | `incomplete`   |
| `context.section-ambiguous`      | `incomplete`   |
| `context.projection-unavailable` | `incomplete`   |
| `context.identity-collision`     | `attention`    |
| `context.target-case-mismatch`   | `attention`    |
| `context.frontmatter-missing`    | `attention`    |
| `context.section-missing`        | `attention`    |
| `context.operation-failed`       | `failed`       |
| `context.interrupted`            | `interrupted`  |

Invalid findings stop before operation resolution. Blocked findings mean the
workspace, source, overwrite, or local-target safety boundary cannot be
established. `closure-unavailable` covers a safe but incomplete workspace entry,
Loader, generated-order, parent, `#LoadNow`, `#KeepInMind`, or scope-local
relationship. `layer-unavailable` covers a missing race, access failure,
directory-enumeration failure, or other safe physical-layer read failure.
`markdown-unavailable` covers structural parse or range evidence that cannot be
established after strict decoding. The more specific target, section, and
projection codes retain their Interface meanings.

An established non-unique automatic ID is attention only after exact-path or
interactive resolution leaves the selected physical and route boundary safe.
A missing authored frontmatter block is attention only when loading and requested
projection coverage remain otherwise complete; unavailable loading metadata uses
the incomplete closure or Markdown finding. An exact target case mismatch is a
safe attention observation. A completely inspected missing section is attention,
while an unreadable layer or duplicate match is incomplete.

Within one code, request evidence precedes closure evidence, then sources use
canonical logical order, base precedes overwrite, links use breadth-first edge
order, content uses canonical projection and document order, and locations are
final ordinal tie-breaks. Failure and interruption are last. Filesystem,
enumeration, parser, or exception order never controls finding order.

#### Exact Next Actions

The top-level envelope's `next` member uses at most one Context action. After the
aggregate status and ordered findings are fixed, the first applicable row wins.

| Condition                                                              | `next.command`                 | `next.reason`                                                                                               |
| ---------------------------------------------------------------------- | ------------------------------ | ----------------------------------------------------------------------------------------------------------- |
| `complete` or `attention`                                              | `null`                         | `null`                                                                                                      |
| `invalid`                                                              | `open-forge context --help`    | `Correct the named Context input, then rerun the request.`                                                  |
| `blocked` with `context.source-ambiguous` as the first blocked finding | `open-forge context`           | `Replace every ambiguous source reference with one listed exact path, then rerun the same request.`         |
| Other `blocked`                                                        | `open-forge doctor`            | `Inspect the blocked workspace, source, overwrite, or link-target boundary before rerunning Context.`       |
| `incomplete`                                                           | `open-forge doctor`            | `Inspect the unavailable closure, source, link, or projection facts before relying on this Context result.` |
| `failed`                                                               | `open-forge context --verbose` | `Report the failure and retry the same Context request with bounded diagnostics.`                           |
| `interrupted`                                                          | `open-forge context`           | `Rerun the same Context request.`                                                                           |

Human compact and expanded output use the same optional action. Renderers do not
choose, rewrite, or multiply next actions.

### Human-Readable Errors

Every error names:

1. The failed operation.
2. The affected route, source, section, or link when known.
3. The direct cause.
4. A useful next action when one exists.

`--verbose` adds diagnostic detail. Ordinary errors remain understandable
without it.

### Output Streams And Architecture Boundary

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Each primary human result stays together on its assigned stream.
`--json` emits one complete structured result to stdout for every semantic status.
Separate bounded diagnostics go to stderr, and human text is not mixed into JSON
stdout.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
exact structured field names, schema compatibility, and process-status mapping.
The [CLI Architecture](../../architecture.md) defines parser and concrete
serialization relationships, source ranges, filesystem identity and containment,
source structure, runtime boundaries, and bounded diagnostic structure. These
accepted technical choices do not weaken the status, stream, ordering, or
completeness rules above.

## Global Flags

The shared [Global CLI Flags](../shared/global-flags/interface.md) contract defines
`--workspace`, `--json`, `--view`, `--verbose`, `--help`, and `--version` once
for the complete CLI. All six apply to `context` under that contract.

The `context` command adds no global flag or alternate spelling.

## Complete Examples

### Startup Context

```text
open-forge context
```

Returns complete authored content for the startup-required closure.

### Source Metadata

```text
open-forge context --content=metadata
```

Returns ordered CLI-generated source metadata without authored frontmatter or
bodies.

### Ordered Paths

```text
open-forge context --content=paths --view=compact
```

Returns only canonical paths in exact context order with the compact result
summary and any required completeness findings.

### Heading Outlines

```text
open-forge context directives --content=headings --view=expanded
```

Returns parsed heading outlines with levels, lines, source forms, layers, and
canonical-authoring evidence without section bodies.

### One Scope

```text
open-forge context memory/project-a/crystallized/documents
```

Returns the startup closure plus the selected scope closure.

### Additions Beyond Startup

```text
open-forge context \
  memory/project-a/crystallized/documents \
  --additions-only
```

Returns only sources that the selected route adds beyond startup.

### Inherited Rules

```text
open-forge context \
  directives/open-forge/framework \
  --content=section:Axioms,section:Instructions
```

Returns exact rule sections from the selected closure. Missing sections remain
visible per source.

### Direct Link Expansion

```text
open-forge context \
  memory/crystallized/documents/architecture \
  --follow-links=1
```

Returns normal selected context plus directly linked local sources.

### Full Reachable Link Closure

```text
open-forge context \
  memory/crystallized/documents/architecture \
  --follow-links=all \
  --content=frontmatter,section:Scope
```

Follows the complete reachable contained local-link graph and emits frontmatter
plus each exact `Scope` section.

### Structured Output From Another Workspace

```text
open-forge context \
  memory/crystallized/documents \
  --workspace ../another-workspace \
  --additions-only \
  --follow-links=2 \
  --content=metadata,section:Axioms \
  --json
```

Uses the exact workspace path, calculates additions from that workspace's startup
closure, follows two link levels, and renders one structured result.

## Non-Goals

`context` does not:

- Infer which route matches a natural-language goal.
- Perform tag, literal, fuzzy, semantic, vector, or ranked search.
- Create or consume a session, receipt, cache key, or `--since` state.
- Treat link proximity as relevance or authority.
- Fetch external links.
- Summarize, paraphrase, or truncate requested authored content silently.
- Repair malformed files, links, indexes, or overwrites.
- Modify the workspace.

Tag discovery belongs to the accepted search capability. Structural checking and
repair belong to their own operations.

## Verification

Gate 5 executable proof must cover:

- Exact CWD and `--workspace` selection.
- IDs, exact paths, collisions, and disambiguation from the shared Source
  References contract.
- Startup closure ordering.
- Parent-scoped #KeepInMind behavior for entrypoints and ordinary files.
- Inactive continuity exclusion and explicitly selected scope loading.
- Single and multiple route closures.
- Sparse and nested scopes.
- Base and overwrite ordering and orphan failure.
- `--additions-only` set difference and invalid no-route use.
- Every content part and valid combination.
- Repeated `--additions-only`, invalid repeated `--follow-links` values,
  repeated `--content`, idempotent parts within one `--content` value, and the
  shared source-operand and global repetition rules.
- Canonical mixed projection order with operation-level paths first, resolved
  source and layer order, metadata/frontmatter/headings/body/section order,
  document order for requested sections, and exact authored bytes.
- A fully resolved empty additions difference, an empty heading outline,
  unchecked external URLs that are not selected or fetched, and cycles or
  duplicate links that do not change status by themselves.
- Deterministic blocked, incomplete, attention, complete, invalid, failed, and
  interrupted classification, safety/coverage precedence, preservation of safe
  content and observations, and visible broken edges.
- Compact and expanded metadata, paths, headings, and authored-content framing.
- Parsed CommonMark ATX and Setext headings; visible-text and
  case-insensitive matching; exact section boundaries; canonical-form evidence;
  missing sections; duplicate headings within one layer; and matching base and
  overwrite sections.
- Link depths 1, 2, a higher bounded value, and `all`.
- Link cycles, duplicate targets, fragments, external links, broken links, and
  containment failures.
- Stable ordering and inclusion reasons.
- Human and structured output from the same typed result.
- Primary human status streams, one structured JSON result on stdout for every
  status, and bounded diagnostics on stderr without human text in JSON stdout.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  outcomes.
- Repeat invocations producing the same semantic result for unchanged input.

Direct tests should prove graph construction, closure selection, projection,
ordering, and statuses. Focused integration tests should use real temporary
workspaces. Built-process, package, and Native AOT proof should cover parsing,
output, exit behavior, and packaged execution. The [Behavior Contract](behavior.md)
defines the technology-neutral conformance relationship, and the [Technical
Design](technical-design.md) records the accepted evidence shape without
claiming that the suite or artifacts exist.

## Related Current Sources

- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Context Behavior Contract](behavior.md)
- [Context Technical Design](technical-design.md)
- [CLI Architecture](../../architecture.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Routing Loading And Continuity](../../../framework/routing/loading.md)
- [Routing Model](../../../framework/routing/model.md)
- [Route Scope And Inheritance](../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../framework/routing/paths.md)
- [Overwrite Customization](../../../framework/routing/overwrites.md)
