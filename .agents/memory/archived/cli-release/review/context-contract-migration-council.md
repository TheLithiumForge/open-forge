---
open-forge:
  description: Historical council evidence for the settled Queue 23 Context refinement
  responsibility: Preserve Queue 23 review history without making it current Context authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Review, Council, Analysis, Context, Contract, Migration]
---

# Context Contract Migration Council

## Status And Authority

This is archived, contextual, historical review material from Queue 23. It
originated as the `context` contract refinement council record under the Working
CLI release review route. It was archived after the maintainer accepted the
bounded Context refinement and Queue 23 moved to settled history. The record
preserves three independent grounded lenses, a source-coverage map, and the
council-time execution recommendation. It is not a command contract or current
authority source.

Origin: `.agents/memory/working/cli-release/review/context-contract-migration-council.md`.

Archival reason: Queue 23 settled, so this council analysis no longer belongs in
the active review queue.

Current authority: the [Context Interface](../../../crystallized/documents/cli/contracts/context/interface.md),
[Context Behavior](../../../crystallized/documents/cli/contracts/context/behavior.md), and
[Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
define current Context meaning. The [Context Technical Design](../../../crystallized/documents/cli/contracts/context/technical-design.md)
remains subordinate Gate 3 work. The [Decision Agenda](../../../working/cli-release/decision-agenda.md)
records the accepted refinement, and the [Review Queue](queue.md)
records its settled disposition.

## Current State

The cutover is complete. The [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
and its [Interface](../../../crystallized/documents/cli/contracts/context/interface.md) and
[Behavior](../../../crystallized/documents/cli/contracts/context/behavior.md) files are the current Working
authorities for the accepted `context` meaning. The subordinate
[Technical Design](../../../crystallized/documents/cli/contracts/context/technical-design.md) remains work in
progress and cannot redefine either contract. Detailed review may refine these
current contracts, but it does not demote them or create a replacement
authority. Gate 2 remains in progress.

The former mixed source is named below only as council-time history. The old
`commands/context-command.md` path is deleted and is not linked. The sections
under **Historical Council-Time Record** preserve the authority boundary and
recommendations that applied before cutover; they are not current facts.

This record does not claim implementation or shipping. It does not edit the
current contracts, the migration ledger, the review queue, or generated
navigation. No requirement or migration IDs are introduced. A caller-visible
source identifier remains a source fact; it is not a migration label.

## Historical Council-Time Record

At council time, the former mixed source was treated as the source of meaning
while an additive candidate set was prepared, reviewed, and explicitly cut over.
Candidate files could preserve accepted meaning, but their existence,
completeness, links, or review status could not change authority or make the
command available before release. This record did not claim acceptance,
implementation, shipping, or migration completion at that boundary.

At the time of this council round, the
[CLI Contract Migration Ledger](../contract-migration-ledger.md) recorded only
the migration rules and the Find and Index candidate coverage. This record's map
was prospective review evidence rather than a ledger update. Later authorized
preparation added the Context artifact row and fact-level coverage to the ledger
and created the non-authoritative candidate under `commands/context/`.

## Question

How can the complete meaning of the mixed `context-command.md` source be staged
additively in a command-local routed scope while preserving its authority,
caller-visible behavior, technology-neutral semantics, concrete implementation
direction, open Gate 3 choices, and every example and verification obligation?

The recommendation below answers the placement and accounting question only.
It does not decide the unresolved product or Gate 3 questions in the source.

## Constraints

- At council time, the former mixed `commands/context-command.md` source remained
  authoritative until a reviewed cutover explicitly named replacements.
- The command is stateless, deterministic, read-only, and non-mutating. The
  candidate split must not introduce sessions, receipts, persistent graph truth,
  inferred relevance, or mutation behavior.
- The [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md) and [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md)
  contracts remain shared sources. The candidate must link to them instead of
  copying their complete definitions.
- The Framework [loading](../../../crystallized/documents/framework/routing/loading.md),
  [routing model](../../../crystallized/documents/framework/routing/model.md),
  [scope](../../../crystallized/documents/framework/routing/scope.md),
  [path](../../../crystallized/documents/framework/routing/paths.md), and
  [overwrite](../../../crystallized/documents/framework/routing/overwrites.md)
  contracts retain authority for the Framework meaning consumed by `context`.
- The [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)
  keeps related command files together, separates Interface, Behavior, and
  Technical Design questions, and requires additive authority boundaries.
- Interface must own the complete caller-visible grammar and result meaning.
  Behavior must own technology-neutral closure, graph, projection, ordering,
  deduplication, completeness, and safety mechanics. Technical Design may not
  redefine either contract.
- The source-stated concrete direction may be preserved, but no library,
  parser, serializer, schema, numeric exit mapping, filesystem strategy, or
  .NET source-module boundary may be invented or silently settled.
- The resulting proposal uses no permanent requirement-ID system and no new
  temporary label system for this command.
- This council round created only this record. Preparing the candidate directory
  was a recommendation recorded here, not an additional file change in that
  round. Later authorized preparation created the candidate without accepting
  this council or cutting over authority.

## Common Ground

The three lenses agree on these points:

- The mixed source is the current authority. Additive candidates are useful
  staging material, not replacements.
- The public contract and its technology-neutral realization answer different
  questions and must not compete for detailed ownership.
- Concrete implementation choices belong in a visibly provisional Technical
  Design, and open Gate 3 choices must remain open.
- The command-local scope is the narrowest useful place for the three contract
  roles and their directly related evidence.
- Shared flags, source-reference grammar, and Framework routing meaning should
  be linked at their existing scopes rather than copied into the command set.
- The invocation-local content graph is derived from current human-readable
  sources. It is not persisted workspace truth, and `status` should not build the
  complete graph merely because another operation can reuse a graph builder.
- Base and overwrite layers, route and link relationships, loading and scope,
  source identity and path, and inclusion reason are distinct facts. A link does
  not create a route, loading, scope, or authority relationship.
- Every condition, exception, default, prohibition, recommendation, and open
  choice in the old source must remain visible with the same strength.
- The source map below is preparation evidence. It is not a cutover record and
  does not make the candidate set authoritative.

## Independent Grounded Lenses

These are independent analytical lenses, not votes. Each was checked against the
authoritative command at council time, the shared and Framework contracts, the
CLI command-contract Working Documents, the Decision Agenda, and the migration
ledger.

### Authority And Boundary

The authority lens starts with the question each source is allowed to answer.
The old command source answers the current `context` contract. The shared and
Framework sources answer their own questions. A candidate entrypoint can expose
files, but a route, generated `Entries` block, or complete-looking candidate
cannot promote itself.

This lens therefore requires:

- a visibly non-authoritative candidate scope;
- one complete public owner in `interface.md`;
- one technology-neutral semantic owner in `behavior.md`;
- one subordinate, work-in-progress implementation owner in
  `technical-design.md`; and
- a route-only `_context.md` that cannot become a competing summary of the
  command.

It also keeps the following boundaries explicit:

- Interface may promise observable comparison, order, status, safety, and
  completeness without naming a runtime API.
- Behavior may explain how closure, graph, projection, ordering, and safety
  reach the Interface result without selecting a library or private schema.
- Technical Design may preserve the invocation-local in-memory graph and shared
  graph-builder direction, the source-named .NET comparison API, and the Native
  AOT evidence boundary. It must not turn any of those facts into a new public
  guarantee or use them to close an unresolved choice.

### Lossless Accounting

The lossless lens treats the 850-line mixed source as an inventory, not as a
summary to be shortened. It requires an unambiguous destination for every
heading, subsection, complete example, non-goal, output rule, error condition,
and verification obligation. A fact may be split across layers when the layers
answer different questions, but it must have one detailed owner and a visible
cross-layer link.

This lens gives special attention to facts that are easy to lose when moving
from one long source into several files:

- omission, repetition, order, dependency, and invalid-input rules;
- source identity, physical path, logical source, base layer, overwrite layer,
  inclusion reason, and incoming link;
- startup, explicit, additions-only, and link-expanded closure differences;
- the distinction between `metadata` and authored `frontmatter`, and between
  logical-source output and the operation-level `paths` projection;
- parsed heading visibility, source form, canonical-authoring evidence, exact
  section boundaries, missing sections, duplicate headings, and layer-specific
  ambiguity;
- partial safe output and the difference between `complete`, `attention`,
  `incomplete`, `invalid`, `blocked`, `failed`, and `interrupted`;
- exact authored bytes, compact and expanded framing, structured-result parity,
  non-goals, and the stated verification strength; and
- every unresolved stream, repetition, ordering, status-combination, schema,
  exit, parser, filesystem, and source-boundary choice.

The source-heading map and complete-example inventory below are the accounting
boundary for this lens. They are intentionally more detailed than an ordinary
review summary.

### Locality, Simplicity, And Conformance

The locality lens keeps the files under the final public command path and keeps
the command's related contracts together. The simplicity part resists a fourth
semantic contract, duplicated shared definitions, a public graph product, and
file splits that exist only because of artifact labels. The conformance part
checks the result against the CLI command-contract Working Documents and the existing
Framework routing model.

Its conclusion is that the route shell is necessary for routability but must not
become a fourth semantic owner. Three files carry contract meaning. The fourth
physical file carries only the route that exposes those three files. This gives
the command a local, discoverable scope without making the entrypoint repeat
public or behavioral detail.

## Material Three-Versus-Four-File Disagreement And Synthesis

The disagreement is about what is being counted, not about whether the route
needs a recognized entrypoint.

| Reading                                                                                                           | Strength                                                                                                           | Risk                                                                                                       | Synthesis                                                                                 |
| ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Three-file reading: `interface.md`, `behavior.md`, and `technical-design.md` are the only contract-bearing files. | Smallest semantic surface. It prevents an entrypoint from becoming a fourth place where command meaning can drift. | If the route shell is omitted or asked to carry detail, routability and contract ownership become unclear. | Retain this as the semantic count. `_context.md` has no detailed contract responsibility. |
| Four-file reading: `_context.md` plus the three contract files form one routed set.                               | Matches the routed command-scope shape and makes the local set discoverable through `Entries`.                     | Counting the entrypoint as another contract can duplicate status, roles, purpose, or behavior.             | Retain this as the physical file count, while making `_context.md` routes only.           |

The resulting recommendation is **four physical files with three semantic
owners**:

- `commands/context/_context.md` is a route-only entrypoint. It exposes the
  sibling contract files through generated `Entries` and carries only the
  metadata and routing scaffold needed for that purpose. It does not summarize
  the command or define status, grammar, graph behavior, results, or technical
  choices.
- `commands/context/interface.md` owns the complete caller-visible contract.
- `commands/context/behavior.md` owns the complete technology-neutral operation
  mechanics behind that interface.
- `commands/context/technical-design.md` owns only source-stated concrete
  direction and visibly open Gate 3 choices.

This resolves the structural disagreement without resolving any product,
schema, stream, parser, filesystem, or source-boundary question.

## Agreed Execution Recommendation

The lenses agree on the following additive execution recommendation. It remains
unaccepted until the maintainer reviews the candidate and explicitly accepts a
cutover.

| Destination           | Owns                                                                                                                                                                                                                                                                                      | Must not do                                                                                                                |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| `_context.md`         | Route metadata, command-scope title, and generated navigation for the three sibling files.                                                                                                                                                                                                | Define a competing contract, repeat detailed status or roles, or become an authority source.                               |
| `interface.md`        | The full public purpose, syntax, operands, flags, defaults, repetition, composition, workspace-visible behavior, projections, output, statuses, errors, complete examples, non-goals, and caller-visible verification.                                                                    | Choose a library, source module, parser, serializer, schema, numeric exit mapping, or private storage design.              |
| `behavior.md`         | Technology-neutral request resolution, startup and explicit closure, additions difference, graph facts, link traversal, overwrite layering, projection mechanics, completeness, ordering, deduplication, one typed result, read-only safety, and conformance.                             | Add public flags or result meanings, name a runtime API or library, or settle an open Gate 3 choice.                       |
| `technical-design.md` | The accepted .NET Native AOT implementation context; the invocation-local in-memory graph and shared graph-builder direction; the source-named `StringComparison.OrdinalIgnoreCase` API; the Native AOT process-evidence boundary; and open Gate 3 choices traced to the other two files. | Invent a library, parser extension, schema, exit mapping, filesystem strategy, source-module boundary, or behavior change. |

The technical design must preserve the source-stated direction that each
`context` invocation builds an in-memory graph and that other operations may
reuse the same graph builder when they need those relationships. It must also
preserve the source's named `.NET` `StringComparison.OrdinalIgnoreCase` API for
case-insensitive heading-name comparison. Neither fact selects a library or
settles the complete parser, filesystem, comparison, or source architecture.

The accepted .NET Native AOT direction and the small built Native AOT process
suite belong in Technical Design as implementation context and planned evidence.
They do not claim that the command or its process suite already exists.

## Exhaustive Source-Heading-To-Destination Map

The following map accounts for every heading and subsection in the authoritative
source. A row with more than one destination is a deliberate boundary split:
the first destination owns the detailed meaning, and the other destination owns
only its distinct relationship or conformance mechanics.

| Authoritative source heading or subsection | Destination                                                                                  | Preservation boundary                                                                                                                                                                                                                                                                                         |
| ------------------------------------------ | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| File frontmatter and `# Context Command`   | `_context.md` for route metadata and the route label; each sibling for its own file metadata | Keep the command scope discoverable without copying the old file's contract prose into the route shell.                                                                                                                                                                                                       |
| `## Status`                                | `interface.md`, `behavior.md`, and `technical-design.md`                                     | Interface states candidate authority, command availability, Gate 3 public deferrals, and related contracts. Behavior states its subordinate semantic boundary. Technical Design states WIP status, accepted implementation context, and open Gate 3 work. `_context.md` remains route-only.                   |
| `## Purpose`                               | `interface.md`                                                                               | Preserve the ordered context purpose, explicit-route behavior, no inference, no mutation, no session state, and deterministic promise.                                                                                                                                                                        |
| `## Syntax`                                | `interface.md`                                                                               | Preserve the complete command form, separate source operands, all operation flags, and the source-reference link. Inline syntax and operand examples stay with the public grammar.                                                                                                                            |
| `## Workspace`                             | `interface.md` and `behavior.md`; link to the shared Global Flags contract set               | Interface owns the visible workspace selection and reporting relationship. Behavior owns exact request resolution without creating a second workspace rule.                                                                                                                                                   |
| `## Context Resolution`                    | `interface.md` and `behavior.md`                                                             | Interface owns what each closure means to a caller. Behavior owns the deterministic closure flow and its prerequisites.                                                                                                                                                                                       |
| `### No Explicit Source`                   | `interface.md` and `behavior.md`                                                             | Preserve the startup-required closure, the five loading-order steps, target-sensitive continuity behavior, and the boundary against frozen broad loading.                                                                                                                                                     |
| `### Explicit Sources`                     | `interface.md` and `behavior.md`; link to the shared Source References contract set          | Preserve selected parent chain, source, overwrite, visible `#LoadNow` descendants, scope-local loading, no ordinary link following by default, and exact-path unrouted behavior.                                                                                                                              |
| `### Several Sources`                      | `interface.md` and `behavior.md`                                                             | Preserve independent route chains, scope and authority, all inclusion reasons, physical-source deduplication, and the prohibition on combined-scope conflict resolution.                                                                                                                                      |
| `### Source Errors`                        | `interface.md` and `behavior.md`                                                             | Interface owns invalid, blocked, attention, and incomplete caller-visible outcomes. Behavior owns safe continuation only when the missing boundary cannot change the claimed complete result.                                                                                                                 |
| `## Additions Beyond Startup`              | `interface.md` and `behavior.md`                                                             | Interface owns `--additions-only`, its required explicit route, and its result meaning. Behavior owns same-invocation expansion and ordered set difference, including subtraction of startup-reachable links.                                                                                                 |
| `## Content Graph`                         | `behavior.md` and `technical-design.md`                                                      | Behavior owns the graph's technology-neutral nodes, relationships, per-invocation lifetime, non-persistence, and reuse boundary. Technical Design preserves only the source-stated in-memory/shared graph-builder direction and leaves the source units open.                                                 |
| `## Content Projection`                    | `interface.md` and `behavior.md`                                                             | Interface owns the public projection grammar and emitted meaning. Behavior owns projection from the already resolved result without rerunning selection.                                                                                                                                                      |
| `### Flag` under Content Projection        | `interface.md`                                                                               | Preserve comma and backslash grammar, idempotent parts, invalid repetition, canonical output independence from flag order, and the default `frontmatter,body`.                                                                                                                                                |
| `### Source Metadata`                      | `interface.md`                                                                               | Preserve generated metadata fields, base/overwrite layer identity, compact and expanded differences, complete JSON facts, and the distinction from authored frontmatter.                                                                                                                                      |
| `### Ordered Paths`                        | `interface.md` and `behavior.md`                                                             | Interface owns exact physical-layer path output, path-only replacement behavior, framing, and provenance. Behavior owns the invariant that selection and status are not rerun or simplified by projection.                                                                                                    |
| `### Frontmatter`                          | `interface.md`                                                                               | Preserve complete authored YAML, without normalization or reconstructed replacement, and visible malformed or missing metadata findings.                                                                                                                                                                      |
| `### Body`                                 | `interface.md`                                                                               | Preserve all Markdown after frontmatter, including generated regions and exact authored bytes; do not silently summarize, truncate, or remove.                                                                                                                                                                |
| `### Headings`                             | `interface.md`, `behavior.md`, and `technical-design.md`                                     | Interface owns heading outline fields, source form, levels, visible text, canonical evidence, and view differences. Behavior owns structural projection and shared section boundaries without naming a parser. Technical Design preserves the named ordinal API and leaves parser and extension choices open. |
| `### Exact Sections`                       | `interface.md`, `behavior.md`, and `technical-design.md`                                     | Interface owns exact visible-name matching, section boundaries, escaping, missing and duplicate-section outcomes, layer framing, and projection coverage. Behavior owns deterministic section selection from parsed nodes. Technical Design leaves parser and source-range realization open.                  |
| `### Valid Combinations`                   | `interface.md`                                                                               | Preserve every listed valid combination and every invalid empty, unknown, malformed, or unquoted-list condition. Do not expand compatible combinations into an invented catalogue.                                                                                                                            |
| `## Explicit Link Expansion`               | `interface.md` and `behavior.md`                                                             | Interface owns the public selection modifier and link-visible findings. Behavior owns bounded traversal, cycle handling, deduplication, containment, and result formation.                                                                                                                                    |
| `### Flag` under Explicit Link Expansion   | `interface.md`                                                                               | Preserve positive-depth and `all` values, edge counting from pre-expansion seeds, zero invalidity, and omitted-flag meaning.                                                                                                                                                                                  |
| `### Link Rules`                           | `interface.md` and `behavior.md`                                                             | Interface owns local-only following, external-link recording, overwrite-target framing, outside-`.agents` `ID: none` behavior, and no inference. Behavior owns relative resolution, physical containment, cycle termination, one-emission traversal, and incoming-reason retention.                           |
| `### Link Findings`                        | `interface.md` and `behavior.md`                                                             | Interface owns visible missing, broken, fragment, case, containment, encoding, and ambiguity findings. Behavior owns the rule that broken edges cannot be silently dropped and that complete status cannot be claimed without the required boundary.                                                          |
| `## Overwrites`                            | `interface.md` and `behavior.md`                                                             | Interface owns base-first human framing, separate structured layers, and orphan or ambiguous outcomes. Behavior owns logical-source normalization, layer order, and independent-selection prohibition.                                                                                                        |
| `## Ordering And Deduplication`            | `interface.md` and `behavior.md`                                                             | Interface owns observable startup, operand, breadth-first link, first-canonical-position, all-reasons, and no-duplicate-content rules. Behavior owns deterministic ordering and deduplication mechanics. Technical Design records unresolved cross-platform identity details only.                            |
| `## Results And Failures`                  | `interface.md` and `behavior.md`                                                             | Interface owns the public status and summary. Behavior owns the conditions that form one typed result without fabricating evidence.                                                                                                                                                                           |
| `### Semantic Status`                      | `interface.md`, `behavior.md`, and `technical-design.md`                                     | Interface owns all seven semantic meanings and the fact that numeric exits are open. Behavior maps conditions to those names without adding statuses. Technical Design leaves numeric mapping open.                                                                                                           |
| `### Required Summary`                     | `interface.md` and `behavior.md`                                                             | Interface owns every required workspace, source, closure, flag, count, finding, boundary, view, and content fact. Behavior forms one complete result consumed by both presentations.                                                                                                                          |
| `### Human-Readable Errors`                | `interface.md` and `behavior.md`                                                             | Interface owns the four required error components. Behavior owns formation from the failed operation and known boundary. Output-stream assignment remains open.                                                                                                                                               |
| `## Global Flags`                          | `interface.md` and `behavior.md`; link to the current shared Global Flags contracts          | Interface declares applicability and the no-op or terminal relationship. Behavior reuses shared parsing and request semantics without copying them.                                                                                                                                                           |
| `## Complete Examples`                     | `interface.md`                                                                               | All ten complete examples remain public-interface evidence. Their exact inventory is listed below. Behavior may link to the relevant operation stages but must not duplicate their output prose.                                                                                                              |
| `## Non-Goals`                             | `interface.md` and `behavior.md`                                                             | Interface owns the caller-visible exclusions. Behavior enforces the no-inference, no-persistence, no-fetch, no-repair, no-mutation, and no-silent-truncation boundaries.                                                                                                                                      |
| `## Verification Requirements`             | `interface.md`, `behavior.md`, and `technical-design.md`                                     | Interface owns caller-visible coverage. Behavior owns direct and focused semantic conformance. Technical Design owns the source-stated built Native AOT process-evidence boundary and unresolved concrete evidence choices. Preserve `must` versus `should`.                                                  |
| `## Related Accepted Direction`            | Link-only relationships in the applicable sibling files                                      | Link to the Decision Agenda, Release Plan, Checkpoint, shared contracts, and current CLI Working Documents. Do not copy their authority, status, or technical detail into the context contracts.                                                                                                              |

### Complete Example Inventory

The following entries account for every subsection under `## Complete Examples`.
The command forms and result purposes remain Interface material.

#### Startup Context

```text
open-forge context
```

Preserve the result that returns complete authored content for the
startup-required closure.

#### Source Metadata

```text
open-forge context --content=metadata
```

Preserve ordered CLI-generated source metadata without authored frontmatter or
bodies.

#### Ordered Paths

```text
open-forge context --content=paths --view=compact
```

Preserve canonical paths in exact context order with the compact summary and
required completeness findings.

#### Heading Outlines

```text
open-forge context directives --content=headings --view=expanded
```

Preserve parsed outline levels, lines, source forms, layers, and
canonical-authoring evidence without section bodies.

#### One Scope

```text
open-forge context memory/project-a/crystallized/documents
```

Preserve the startup closure plus the selected scope closure.

#### Additions Beyond Startup

```text
open-forge context \
  memory/project-a/crystallized/documents \
  --additions-only
```

Preserve only sources added by the selected route beyond startup.

#### Inherited Rules

```text
open-forge context \
  directives/open-forge/framework \
  --content=section:Axioms,section:Instructions
```

Preserve exact rule sections from the selected closure and visible missing
sections per source.

#### Direct Link Expansion

```text
open-forge context \
  memory/crystallized/documents/architecture \
  --follow-links=1
```

Preserve normal selected context plus directly linked local sources.

#### Full Reachable Link Closure

```text
open-forge context \
  memory/crystallized/documents/architecture \
  --follow-links=all \
  --content=frontmatter,section:Scope
```

Preserve complete reachable contained local-link traversal and the two selected
projections.

#### Structured Output From Another Workspace

```text
open-forge context \
  memory/crystallized/documents \
  --workspace ../another-workspace \
  --additions-only \
  --follow-links=2 \
  --content=metadata,section:Axioms \
  --json
```

Preserve exact workspace selection, startup-relative additions, two link
levels, selected content, and one structured result.

The other command and output code blocks remain with their owning Interface
subsections: syntax and source-operand forms with `## Syntax`; metadata, paths,
headings, sections, and valid combinations with `## Content Projection`; link
depth forms with `## Explicit Link Expansion`; and the base/overwrite separator
with `## Overwrites`. No code example is evidence for a technical library or
schema choice.

## Cross-Layer And Link-Only Rules

- The old command source is the detailed authority during migration. Candidate
  files link back to it when authority or provenance matters; the link does not
  transfer authority.
- `interface.md` owns a caller-visible fact once. `behavior.md` cites that fact
  when describing its resolution or conformance mechanics. Behavior must not
  restate a second grammar, status table, output contract, or detailed example.
- `behavior.md` owns a semantic fact once. `technical-design.md` cites the
  exact Interface or Behavior passage that a concrete choice realizes. A
  Technical Design entry is not a second definition of the behavior.
- `_context.md` exposes the sibling files only. It does not become a summary,
  authority source, or fourth contract owner.
- The current shared [Global Flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/shared/global-flags/behavior.md) files remain the
  complete source for global spelling, defaults, repetition, terminal behavior,
  JSON stdout, and no-op behavior. The context Interface records only context
  applicability and links there.
- The current shared [Source References Interface](../../../crystallized/documents/cli/contracts/shared/source-references/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/shared/source-references/behavior.md) files remain
  the complete source for source IDs, exact `.agents` paths, collisions, quoting,
  overwrite identity, and display. The context Interface records only how those
  references participate in context selection and links there.
- The Framework routing sources remain the complete source for loading, route
  selection, scope inheritance, path containment, and overwrite meaning. The
  context contracts state what the command consumes and reports without
  redefining Framework semantics.
- Shared source and Framework links are navigational relationships. They do not
  merge scope, loading, authority, responsibility, or lifecycle, and they do
  not allow a later source to resolve a conflict owned by the old command.
- The Decision Agenda, Release Plan, Checkpoint, and migration ledger remain
  links to their own questions. This council must not copy their status or make
  a recommendation in one source look like acceptance in another.
- The eventual cutover may regenerate one route index after authority is
  explicitly changed. Candidate completeness and generated `Entries` must not
  be used as a cutover signal.

## Preserved Unknowns

The candidate must carry these as visible open choices. Neither file placement
nor a convenient implementation may resolve them.

| Open question                    | What is known                                                                                                                                                                 | What remains open                                                                                                                                                                                            |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Output streams                   | JSON is rendered to stdout by the shared global contract. Human output and ordinary errors have required content.                                                             | The normal human-result stream and ordinary-error stream for `context` are not assigned by the source. Do not infer stdout or stderr from convention.                                                        |
| Operation-flag repetition        | Repeating `--content` is explicitly invalid. Repeated source operands compose under the source-reference and closure rules.                                                   | Repetition of `--additions-only` and `--follow-links`, including conflicting repeated values, is not defined by the source. Global-flag repetition remains owned by the shared contract.                     |
| Canonical projection ordering    | Parts are composable and flag order does not change canonical output. Paths have a defined operation-level position, and source, layer, and document order remain meaningful. | The complete canonical order for every mixed `metadata`, `paths`, authored part, and multiple `section:<name>` combination is not fully closed. Do not choose requested-part order or a new canonical order. |
| Link-status combinations         | Broken edges remain visible. Safe sources may be returned beside unrelated findings. The result must distinguish `attention`, `incomplete`, and `blocked` when applicable.    | Precedence and combined status formation for multiple link findings, link findings plus projection findings, and link findings plus closure failures remain open.                                            |
| Structured schemas and exits     | One typed result supplies human and structured output. All semantic status names are defined.                                                                                 | Exact structured field names, schema version and compatibility, and numeric process-exit mapping remain Gate 3 work.                                                                                         |
| Parser boundary                  | Structural headings use parser-provided ATX and Setext nodes, visible text, source forms, levels, and boundaries. Malformed text is not guessed.                              | The Markdown library, parser extensions, exact source-range representation, and parser-to-command source boundary remain open.                                                                               |
| Filesystem and source boundaries | Workspace and local link targets must remain within the selected workspace. Cross-platform physical identity and containment protect safe resolution.                         | Exact case, Unicode, alias, symlink, hardlink, canonical-identity, filesystem API, graph-builder, result, and command/shared source-module boundaries remain Gate 3 choices.                                 |

The Technical Design may list evidence needed to resolve these questions. It
must not turn a candidate, experiment, or recommendation into a choice.

## Likely Loss Traps

- Treating a complete candidate folder as authoritative because it has a route
  or generated `Entries`.
- Putting public syntax, flags, result fields, or complete examples in
  `behavior.md`, then leaving `interface.md` incomplete.
- Making `_context.md` a fourth contract by copying status, purpose, or behavior
  into the route-only entrypoint.
- Describing the graph as persistent truth, a public graph operation, or a
  complete graph required by quick `status`.
- Losing the distinction between startup closure, explicit route closure,
  additions-only difference, and optional link expansion.
- Computing additions against an unexpanded startup set when links are enabled,
  or forgetting that startup-reachable sources are subtracted from additions.
- Collapsing logical sources and physical layers, or emitting an overwrite
  independently instead of base first followed by overwrite.
- Losing inclusion reasons, incoming links, first canonical position, stable
  breadth-first ordering, or the no-duplicate-content rule.
- Treating `metadata` as authored YAML, treating `paths` as per-source content,
  or rerunning selection when a projection is requested.
- Replacing visible heading text with raw marker text, losing Setext source-form
  evidence, accepting malformed headings, or changing the exact
  `OrdinalIgnoreCase` equality boundary.
- Collapsing a known missing section into `incomplete`, or turning an ambiguous
  section into a chosen occurrence instead of preserving `attention` versus
  `incomplete`.
- Summarizing, truncating, normalizing, or relabeling requested authored bytes;
  losing compact/expanded distinctions; or allowing JSON and human rendering to
  rerun the operation.
- Strengthening a source `should` into `must`, weakening a `must`, or dropping a
  condition, exception, next action, or non-goal while moving verification text.
- Assigning streams, repeated operation flags, mixed projection order, link
  status precedence, schemas, exits, libraries, or source boundaries merely to
  make the candidate look complete.
- Introducing migration labels or permanent requirement IDs, or confusing
  caller-visible source identity with migration traceability.
- Treating ordinary links as route, loading, scope, precedence, or authority
  edges, or fetching external links despite the source's explicit boundary.
- Copying shared and Framework contracts into the command set and allowing the
  copy to drift from its authoritative source.

## Verification Checklist

Before any candidate cutover, a reviewer should verify all of the following:

- [ ] The old `commands/context-command.md` is unchanged and still named as the
      authority.
- [ ] The candidate state, non-shipping status, and explicit cutover boundary
      are stated in every contract file that needs them.
- [ ] The physical scope is exactly `commands/context/` with `_context.md`,
      `interface.md`, `behavior.md`, and `technical-design.md`; `_context.md` is
      route-only.
- [ ] No requirement IDs or migration labels were added to the context
      candidate or this synthesis.
- [ ] Every source heading and subsection appears in the map, including all
      ten `Complete Examples` subsections.
- [ ] Every inline command, output, separator, valid-combination, and escaping
      example is retained under its owning Interface subsection.
- [ ] Interface contains the complete public syntax, operand grammar, flag
      values, omission, repetition, ordering, dependencies, errors, outputs,
      statuses, non-goals, examples, and caller-visible verification.
- [ ] Interface preserves the exact distinction between generated metadata,
      authored frontmatter, authored body, headings, exact sections, and ordered
      physical paths.
- [ ] Interface preserves source IDs and canonical paths as caller-visible
      source facts without treating them as migration labels.
- [ ] Behavior contains technology-neutral startup and explicit closure,
      additions difference, graph relationships, link traversal, overwrite
      layering, projection, completeness, ordering, deduplication, result
      formation, read-only effects, and safety boundaries.
- [ ] Behavior forms one typed result and does not rerun work in human or
      structured rendering.
- [ ] Behavior does not name a library, runtime API, parser, serializer,
      private schema, source module, or filesystem implementation.
- [ ] Technical Design contains only the source-stated Native AOT context,
      invocation-local in-memory/shared graph-builder direction, named ordinal API,
      Native AOT process-evidence boundary, and explicit open Gate 3 choices.
- [ ] Technical Design traces each concrete fact to an Interface or Behavior
      passage and does not settle a library, parser extension, schema, exit, stream,
      filesystem, or source-boundary question.
- [ ] The shared global-flags and source-reference definitions are linked rather
      than duplicated, while context-specific effects remain visible.
- [ ] The loading, routing, scope, path, and overwrite Framework sources are
      linked rather than redefined, and ordinary links are not treated as route or
      authority edges.
- [ ] All defaults, invalid states, incomplete coverage, partial safe output,
      base/overwrite cases, link findings, and semantic statuses compare exactly
      with the old source.
- [ ] All preserved unknowns remain visibly open, including streams, operation
      flag repetition, projection ordering, link-status combinations, schemas,
      exits, parser boundaries, filesystem boundaries, and source boundaries.
- [ ] Verification retains the old requirement strength: mandatory evidence
      remains mandatory and recommended direct, integration, and Native AOT layers
      remain recommendations.
- [ ] The migration ledger receives a fact-level Context coverage and conflict
      record before any cutover is considered; this council record is not that
      ledger update.
- [ ] Authority review, independent lossless review, conflict review, link and
      Markdown validation, and the one generated-index cutover occur before the
      maintainer is asked to accept replacement authority.

## Hard Stops

Stop preparation or cutover if any of these conditions occurs:

- A source fact has no destination, two competing detailed destinations, or a
  changed requirement strength.
- The candidate or its generated navigation is treated as authoritative before
  explicit maintainer-accepted cutover.
- The old command source is deleted, weakened, rewritten, or replaced as part
  of additive staging.
- `_context.md` carries contract detail instead of routing the siblings.
- Interface and Behavior disagree about a public grammar, result, status,
  ordering rule, projection, or safety condition.
- Behavior names or depends on a library, API, parser, serializer, schema,
  source module, filesystem strategy, or private storage choice.
- Technical Design invents a library or settles an unknown stream, repetition,
  projection order, link-status combination, schema, exit, parser, filesystem,
  or source boundary.
- A link is used to merge authority, scope, loading, lifecycle, or
  responsibility, or a shared/Framework source is copied as a competing
  definition.
- A missing, ambiguous, broken, or incomplete boundary is hidden by a complete
  result, a chosen occurrence, a shortened example, or a convenient fallback.
- The migration introduces requirement IDs or labels, or claims that the
  caller-visible source identifier is migration traceability.
- A destructive cutover, deletion, generated-index rewrite, or authority
  change is attempted without complete ledger coverage, resolved conflicts,
  independent lossless and authority review, and explicit maintainer acceptance.

## Related Sources

- [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
- [CLI Decision Agenda](../../../working/cli-release/decision-agenda.md)
- [CLI Contract Migration Ledger](../contract-migration-ledger.md)
- [CLI Review Queue](queue.md)
- [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)
- [CLI Command Interface Contract Working Document](../../../crystallized/documents/cli/command-interface-contract.md)
- [CLI Command Behavior Contract Working Document](../../../crystallized/documents/cli/command-behavior-contract.md)
- [CLI Command Technical Design Working Document](../../../crystallized/documents/cli/command-technical-design.md)
- [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md)
- [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
- [Routing Loading And Continuity](../../../crystallized/documents/framework/routing/loading.md)
- [Routing Model](../../../crystallized/documents/framework/routing/model.md)
- [Route Scope And Inheritance](../../../crystallized/documents/framework/routing/scope.md)
- [Routing Paths And Identity](../../../crystallized/documents/framework/routing/paths.md)
- [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md)
- [Open Forge Writing Standard](../../../crystallized/documents/maintenance/writing.md)
