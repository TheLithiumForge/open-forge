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
spelling. The six shared global flags, `--workspace`, `--format json`, `--detail`,
`--detail debug`, `--help`, and `--version`, apply under the shared contract.

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
behavior, output behavior, and errors. In particular, `--detail` changes only
human presentation, `--format json` selects one structured result, and `--detail debug`
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
with an `completed-with-warnings` or `incomplete` result, provided the command states the
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
standard startup set. A source reachable from startup does not become a route
addition only because a selected route also links to it.

If that fully resolved difference contains zero added sources, the command
returns an explicit `completed` empty result. This is distinct from the invalid
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
order in which their parts appeared in `--content`. The default standard
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

In the default standard view, each emitted layer has a labelled source metadata
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

Minimal view renders one token-friendly row per physical layer with sequence,
ID, path, and layer. It omits optional route, scope, and inclusion explanation;
required completeness findings and next actions remain visible. Standard view
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

Minimal human view emits one canonical workspace-relative path per line in
context order:

```text
.agents/loader.md
.agents/memory/_memory.md
.agents/memory/working/_working.md
```

Standard human view adds sequence, ID, layer, inclusion category, and every
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

returns metadata blocks without frontmatter or bodies. Illustrative excerpt:

```text
Context

Workspace: D:/work/example
Sources: 3; coverage complete
Content: metadata
Startup context included: no; additions only: yes

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

Minimal human view emits one authored heading per line in document order with
enough indentation or heading markers to preserve level:

```text
# Writing
## Instructions
```

Standard human view adds the physical layer, 1-based line, heading level, source
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
completely, the known absence produces `completed-with-warnings`: selection and inspection are
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

Minimal human output retains result and projection coverage plus each affected
source and missing or ambiguous section name. Standard and structured output add
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
a target is link-standard content, not a source-reference operand, route, or
managed Open Forge source.

### Link Findings

A missing target, broken fragment, case mismatch, containment escape, invalid
encoding, or ambiguous local target remains visible. The command never silently
drops a broken edge. The status classification and precedence for these and
other findings are defined under [Semantic Results](#semantic-results).

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
target, #LoadNow, #KeepInMind, and overwrite order. Link-standard sources are appended in
stable breadth-first order by link depth, source order, and document link order.

When several relationships select the same source:

- Emit it at its first canonical position.
- Record every inclusion reason.
- Do not repeat its authored content.
- Do not use later selection as higher authority.

Canonical path identity and physical containment prevent aliases from producing
duplicate or unsafe nodes. The CLI Architecture defines the accepted
cross-platform identity and containment realization.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines



### Text by level



### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-completed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/__snapshots__/ContextBeforeOutputSnapshotTests/Selection/paths.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-completed-with-warnings). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/__snapshots__/ContextBeforeOutputSnapshotTests/Selection/section-missing.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/__snapshots__/ContextBeforeOutputSnapshotTests/Selection/unreadable-source.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/__snapshots__/ContextBeforeOutputSnapshotTests/Selection/invalid-content.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/__snapshots__/ContextBeforeOutputSnapshotTests/Selection/ambiguous-source.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#context-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ sources: [ { path, id, layer: "base" \| "overwrite", parts: [ { part, text \| headings: [ ... ] \| paths: [ ... ] } ] } ] }` |
| standard | + per source `includedBecause: [ ... ]`, `route`, `scope`                                                                       |
| full     | + `order`, headings with `line`, `links: [ { from, location, destination, resolvedPath, resolution, followed } ]`               |

Text values are exact; JSON escaping is the serializer's.

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Counts and limitations

`sources`, `tokens`, `bytes`, `linksFollowed`, `linksNotFollowed`.

### Next rules

Invalid source -> `open-forge route list --depth=all`; unreadable ->
`open-forge doctor`; case mismatch -> `open-forge repair --automatic`;
otherwise none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                           | Severity | Family                | Message                                                                                                                     | Next                                |
| ------------------------------ | -------- | --------------------- | --------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| context.invalid-input          | error    | invalid-input         |                                                                                                                             |                                     |
| context.invalid-source         | error    | unknown-source        | [`context.label.the-requested-source`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.invalid-source`).                                                              | `open-forge route list --depth=all` |
| context.invalid-content        | error    | local                 | [`context.phrase.content-is-not-a-known-part-use-metadata-paths-frontmatter-headings-body-or-section-name`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.invalid-content`).               | none                                |
| context.invalid-link-depth     | error    | local                 | [`context.message.follow-links-must-be-a-positive-number-or-all`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.invalid-link-depth`).                                                                          | none                                |
| context.workspace-unavailable  | error    | workspace-unavailable |                                                                                                                             |                                     |
| context.workspace-unsafe       | error    | workspace-unsafe      |                                                                                                                             |                                     |
| context.source-ambiguous       | error    | source-ambiguous      |                                                                                                                             |                                     |
| context.source-unsafe          | error    | source-unsafe         |                                                                                                                             |                                     |
| context.overwrite-ambiguous    | error    | local                 | [`shared.phrase.could-belong-to-more-than-one-base-file`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.overwrite-ambiguous`).                                                              | fix by hand                         |
| context.target-ambiguous       | error    | local                 | [`context.phrase.the-link-at-could-point-to-more-than-one-file-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.target-ambiguous`).                                            | fix by hand                         |
| context.target-unsafe          | error    | local                 | [`context.phrase.the-link-at-points-outside-the-workspace-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.target-unsafe`).                                                 | none                                |
| context.closure-unavailable    | warning  | local                 | [`context.phrase.the-startup-files-could-not-be-resolved`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.closure-unavailable`).                                                                        | `open-forge doctor`                 |
| context.layer-unavailable      | warning  | local                 | [`context.phrase.could-not-be-read-so-it-was-not-included`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.layer-unavailable`).                                                                         | `open-forge doctor`                 |
| context.invalid-encoding       | warning  | local                 | [`context.phrase.is-not-valid-utf-8-so-it-was-not-included`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.invalid-encoding`).                                                                        | fix the file                        |
| context.markdown-unavailable   | warning  | local                 | [`context.phrase.could-not-be-parsed-as-markdown-so-its-was-not-produced`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.markdown-unavailable`).                                                   | `open-forge doctor`                 |
| context.target-missing         | warning  | local                 | [`context.phrase.the-link-at-points-to-which-does-not-exist-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.target-missing`).                                | `open-forge doctor`                 |
| context.fragment-missing       | warning  | local                 | [`context.phrase.the-link-at-points-to-which-has-no-heading-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.fragment-missing`).                           | `open-forge doctor`                 |
| context.link-encoding-invalid  | warning  | local                 | [`context.phrase.the-link-at-has-an-encoding-that-cannot-be-resolved-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.link-encoding-invalid`).                                      | fix by hand                         |
| context.target-unreadable      | warning  | local                 | [`context.phrase.the-link-at-points-to-which-could-not-be-read-it-was-not-followed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.target-unreadable`).                                    | none                                |
| context.section-ambiguous      | warning  | local                 | [`context.phrase.has-more-than-one-section-named-none-was-included`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.section-ambiguous`).                                                         | fix by hand                         |
| context.projection-unavailable | warning  | local                 | [`shared.phrase.the-of-could-not-be-produced`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.projection-unavailable`).                                                                     | `open-forge doctor`                 |
| context.identity-collision     | warning  | identity-collision    |                                                                                                                             |                                     |
| context.target-case-mismatch   | warning  | local                 | [`context.phrase.the-link-at-is-written-but-the-file-is-named`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.target-case-mismatch`).                                          | `open-forge repair --automatic`     |
| context.frontmatter-missing    | warning  | local                 | `<path> has no frontmatter.` (only when `frontmatter` was requested for a routed source; never for AGENTS.md or the Loader) | none                                |
| context.section-missing        | warning  | local                 | [`shared.phrase.has-no-section-named`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`context.section-missing`).                                                                                       | none                                |
| context.operation-failed       | error    | operation-failed      |                                                                                                                             |                                     |
| context.interrupted            | error    | cancelled             |                                                                                                                             |                                     |

## Scenarios

### Catalogue situations

`startup`, `one-source`, `additions-only`, `paths`, `headings`, `section`,
`frontmatter-missing-host` (AGENTS.md; no finding), `section-missing`,
`follow-links`, `broken-followed-link`, `unknown-source` (invalid-input),
`ambiguous-source` (blocked), `unreadable-source` (incomplete), `invalid-content`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

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
- Deterministic blocked, incomplete, completed-with-warnings, completed,
  invalid-input, failed, and cancelled classification, safety/coverage
  precedence, preservation of safe
  content and observations, and visible broken edges.
- Minimal and standard metadata, paths, headings, and authored-content framing.
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
- Completed, completed-with-warnings, incomplete, invalid-input, blocked,
  failed, and cancelled outcomes.
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
- [Loading And Refreshing Context](../../../framework/routing/loading.md)
- [Routing Model](../../../framework/routing/model.md)
- [Route Scope And Inheritance](../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../framework/routing/paths.md)
- [Overwrite Customization](../../../framework/routing/overwrites.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`context.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Context/ContextText.cs).

<!-- @OpenForgeTextRef context.help.syntax -->
<!-- @OpenForgeTextRef context.label.the-requested-source -->
<!-- @OpenForgeTextRef context.message.follow-links-must-be-a-positive-number-or-all -->
<!-- @OpenForgeTextRef context.phrase.content-is-not-a-known-part-use-metadata-paths-frontmatter-headings-body-or-section-name -->
<!-- @OpenForgeTextRef context.phrase.could-not-be-parsed-as-markdown-so-its-was-not-produced -->
<!-- @OpenForgeTextRef context.phrase.could-not-be-read-so-it-was-not-included -->
<!-- @OpenForgeTextRef context.phrase.has-more-than-one-section-named-none-was-included -->
<!-- @OpenForgeTextRef context.phrase.is-not-valid-utf-8-so-it-was-not-included -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-could-point-to-more-than-one-file-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-has-an-encoding-that-cannot-be-resolved-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-is-written-but-the-file-is-named -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-points-outside-the-workspace-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-points-to-which-could-not-be-read-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-points-to-which-does-not-exist-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-link-at-points-to-which-has-no-heading-it-was-not-followed -->
<!-- @OpenForgeTextRef context.phrase.the-startup-files-could-not-be-resolved -->
<!-- @OpenForgeTextRef shared.phrase.could-belong-to-more-than-one-base-file -->
<!-- @OpenForgeTextRef shared.phrase.has-no-section-named -->
<!-- @OpenForgeTextRef shared.phrase.the-of-could-not-be-produced -->
