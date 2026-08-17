---
open-forge:
  description: Historical council evidence for the pre-refinement Status contract migration
  responsibility: Preserve Queue 22 review history without making it current Status authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Council, Analysis, Status, Command, Contract, Migration]
---

# Status Contract Migration Council

## Status

This is archived, contextual, historical review material from Queue 22. It
originated as the `status` contract migration council record under the Working
CLI release review route. It was archived after the maintainer accepted the
bounded Status refinement and Queue 22 moved to settled history. The record
preserves three independent grounded lenses, their synthesis, and the decision
frontier that preceded that acceptance. It is not a command contract or current
authority source.

Origin: `.agents/memory/working/cli-release/review/status-contract-migration-council.md`.

Archival reason: Queue 22 settled, so this council analysis no longer belongs in
the active review queue.

Current authority: the [Status Interface](../../../crystallized/documents/cli/contracts/status/interface.md),
[Status Behavior](../../../crystallized/documents/cli/contracts/status/behavior.md), and
[Status contract set](../../../crystallized/documents/cli/contracts/status/_status.md)
define current Status meaning. The [Decision Agenda](../../../working/cli-release/decision-agenda.md)
records the accepted refinement, and the [Review Queue](queue.md)
records its settled disposition.

## State At Archival

At archival, the cutover was complete. The [Status contract set](../../../crystallized/documents/cli/contracts/status/_status.md)
and its [Interface](../../../crystallized/documents/cli/contracts/status/interface.md) and
[Behavior](../../../crystallized/documents/cli/contracts/status/behavior.md) files were
the current Working authorities for the accepted `status` meaning. Detailed
review could refine those contracts, but it did not demote them or create a
replacement authority. Gate 2 remained in progress.

The former mixed `commands/status-command.md` path is retained below only as
council-time history. It is deleted and is not linked. The following section
preserves the authority boundary and recommendations that applied before
cutover; it is not current status.

## Historical Council-Time Status And Authority

- This record is Archived Memory under the historical CLI release review route.
  Its analysis was `#Contextual` and unaccepted; the archive does not promote it
  to current authority.
- The former mixed Status source remained authoritative throughout the council's
  additive staging. Candidate files could preserve its meaning, but they could
  not replace it until a reviewed cutover explicitly named them and the
  maintainer accepted that authority change.
- The migration ledger's fact-coverage, conflict, lossless-review, and additive
  staging rules apply. This record did not amend that ledger or make a new
  Status row during the council round. Later authorized preparation added the
  row and exact coverage.
- The proposed `_status.md` is an entrypoint only. It may state candidate
  lifecycle and authority and route its two sibling contract files through
  generated `Entries`. It must not become a third contract or a summary of
  Status behavior.
- The recommendation uses no migration requirement IDs, temporary contract
  labels, or new traceability system. The automatic source identity named by the
  Status source remains a product fact and may use the shared source-reference
  contract; it is not a new migration ID.
- At council time, nothing in this record claimed that the new CLI shipped or
  that the mixed source had been cut over. Later authorized preparation created
  the then-non-authoritative files under `commands/status/`.

## Question

Can the accepted mixed Status source be staged losslessly under the final public
command path while keeping authority, public Interface meaning, technology-
neutral Behavior meaning, shared-contract boundaries, and every unresolved
Gate 3 fact visible without adding a Technical Design or inventing traceability
IDs?

## Constraints

- Preserve the accepted Status purpose: quick, deterministic, read-only
  orientation for one exact workspace. Do not turn it into complete diagnosis,
  recommendation, repair, lifecycle mutation, or a complete context-graph
  operation.
- Use the final public path `commands/status/` for the candidate scope. The
  proposed shape is exactly `commands/status/_status.md`,
  `commands/status/interface.md`, and `commands/status/behavior.md`.
- Keep the three contract roles distinct. Interface defines the complete public
  surface and observable result. Behavior defines deterministic semantics,
  accounting, safety, read-only effects, result formation, and conformance
  without choosing implementation technology.
- Do not create `technical-design.md`. The authoritative source leaves Gate 3
  boundaries open but contains no Status-specific concrete implementation choice
  that needs a separate home.
- Keep `_status.md` limited to routing and authority. Its generated `Entries` are
  navigation only and do not establish authority or add contract facts.
- Link to shared contracts instead of copying their detailed meaning. A link
  does not merge authority, loading, scope, responsibility, or lifecycle.
- Preserve requirement strength, defaults, conditions, exceptions, examples,
  ordering, output distinctions, and uncertainty. Do not convert a recommendation
  into a requirement or a Gate 3 boundary into a public guarantee.
- Map every heading and every subsection of the current Status source to an exact
  candidate file and heading before any cutover work.
- Do not resolve the open stream, cardinality, arithmetic, rendering, schema,
  exit, recovery, token-estimation, or .NET boundaries listed below.
- This council round created only this review record. It did not create candidate
  files, regenerate navigation, edit the source, edit the Decision Agenda, edit
  the migration ledger, or change another file. Later authorized preparation
  performed the separate additive work.

## Evidence Basis

The lenses use the following loaded rules and sources:

- The [Open Forge Loader](../../../../loader.md), the [Writing
  Standard](../../../crystallized/documents/maintenance/writing.md), and the
  [Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md)
  establish authority, routing, contextual-memory, link, terminology, and
  lossless-writing rules.
- The [CLI Review Queue](queue.md) and existing council records establish the
  contextual, unaccepted synthesis style: separate facts from recommendations,
  preserve dissent and evidence gaps, and leave the maintainer decision visible.
- The current [Status Interface](../../../crystallized/documents/cli/contracts/status/interface.md) and
  [Behavior](../../../crystallized/documents/cli/contracts/status/behavior.md) contracts supply the accepted
  Status meaning, public sections, examples, statuses, errors, non-goals, and
  verification requirements.
- The shared [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md), [Context
  contract set](../../../crystallized/documents/cli/contracts/context/_context.md), and [CLI Source References
  contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md) define reusable CLI meaning that
  the Status contract must link rather than redefine.
- The [CLI Decision Agenda](../../../working/cli-release/decision-agenda.md) supplies accepted Status,
  contract-role, shared-result, view, determinism, and migration direction. The
  [CLI Contract Migration Ledger](../contract-migration-ledger.md) supplies the
  source-authority, exact-coverage, conflict, and cutover boundaries.
- The [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md), the [Shared CLI Operation
  Contract](../../../crystallized/documents/cli/shared-operation-contract.md), and the
  [Nearest Shared Scope Pattern](../../../../patterns/software/source-locality/nearest-shared-scope.md)
  supply the local shape and simplicity constraints.

## Independent Grounded Lenses

### Authority And Boundary

This lens starts with the question each source is allowed to answer. The mixed
Status source answers Status meaning. The migration ledger answers additive
coverage and cutover readiness. The current command-contract Working Document
answers placement and role boundaries. A candidate route, generated entry, complete-looking
document, or council convergence cannot answer the authority question.

The lens therefore keeps authority and lifecycle in `_status.md`, keeps all
behavioral and public detail in the two role files, and leaves the mixed source
authoritative. It rejects a Technical Design placeholder: an open .NET source
boundary is not a Status-specific implementation choice. It also rejects any
entrypoint summary that could be read as a second Status contract.

Its main risk finding is authority drift. A complete candidate can look current
when it is routed, linked, or described as an accepted contract. The candidate
files must state their non-authoritative state, while this review record must not
claim a cutover.

### Lossless Accounting

This lens treats the migration as an accounting exercise, not a paraphrase. The
current source has frontmatter and twenty-five Markdown headings, including all
Context Inventory and Workspace Structure subsections. Every source heading,
table, list, code block, example, non-goal, verification obligation, and related
source relationship needs an exact future destination.

Caller-visible meaning belongs once in Interface. Behavior may describe the
technology-neutral flow that proves that meaning, but it must cite the Interface
heading rather than create a competing definition. The map below records that
split without inventing contract prose. It also preserves `should`, optional,
illustrative, unavailable, incomplete, and Gate 3 language exactly as distinct
states.

The lens rejects source-line ranges, labels, or IDs as a substitute for meaning.
Line ranges in the map are navigation aids only. The source heading and its full
content remain the accounting unit.

### Locality, Simplicity, And Conformance

This lens asks whether the smallest structure still makes the command easy to
find, review, and implement without collapsing distinct responsibilities. The
accepted command-local shape is the smallest compatible shape here: one local
entrypoint, one Interface file, and one Behavior file. A single mixed candidate
would reopen the accepted role boundary; distant workspace-wide Interface,
Behavior, or test categories would weaken locality.

The absence of a Technical Design is a simplicity decision grounded in the
source, not a permission to move implementation choices into Behavior. Behavior
must remain technology-neutral and directly testable. Shared facts stay at their
existing shared scope and are linked. Status remains a quick read-only operation
and must not inherit the complete graph, diagnosis, or mutation machinery of
other commands merely because those commands share Framework facts.

## Common Ground

All three lenses support these boundaries:

- The mixed source stays authoritative until exact coverage, conflict review,
  independent review, and an explicit maintainer-accepted cutover are complete.
- The candidate scope follows the public `status` path and contains the three
  local files named in the recommendation.
- `_status.md` routes the contract set and states authority only. It does not
  own Status purpose, syntax, measurements, output, or verification detail.
- Interface and Behavior are distinct but related. Interface owns caller-visible
  meaning. Behavior owns the technology-neutral flow and conformance needed to
  satisfy that meaning.
- Shared global flags, context closure semantics, source identity, and Framework
  loading rules have one detailed owner. The Status candidates link to them.
- No migration requirement IDs or temporary labels are added to this record or
  the proposed Status candidates.
- Every unresolved fact remains visible. Neither a complete map nor a strong
  recommendation resolves an open product, schema, stream, recovery, or
  implementation question.
- Status remains stateless, deterministic for the same accepted inputs and
  bytes, read-only, and bounded to its summary boundary.

## Material Difference

The lenses emphasize different failure modes rather than different execution
shapes. Authority and Boundary would stop a candidate that looks accepted.
Lossless Accounting would stop one with an unmapped sentence, example, or
modality. Locality, Simplicity, And Conformance would stop one that adds a
ceremonial Technical Design, collapses the two roles, or moves the command out
of its local scope. None of the lenses supplies a decision for an unresolved
Status fact, and none authorizes cutover.

## Synthesis

The smallest shape that satisfies all three lenses is an additive candidate set
under `commands/status/`: a routing and authority entrypoint, a complete public
Interface contract, and a technology-neutral Behavior contract. The split is
lossless only if the source map below is treated as a required migration
accounting record rather than as optional documentation.

The synthesis does not promote the candidate set. It recommends staging the
shape while retaining the mixed source as authority, carrying unresolved facts
forward, linking shared contracts, and postponing all implementation-specific
choices. A maintainer must still decide whether and when to accept the
replacement.

## Agreed Execution Recommendation

For later authorized migration work, record and stage this recommendation:

- Use exactly `commands/status/_status.md`,
  `commands/status/interface.md`, and `commands/status/behavior.md` as the
  additive candidate scope.
- Do not add `commands/status/technical-design.md`. The Status source has no
  Status-specific concrete implementation choice; its open .NET, token,
  schema, exit, and recovery boundaries remain unresolved rather than moving
  into an invented design file.
- Keep `_status.md` to the candidate lifecycle and authority statement plus the
  generated `Entries` route. It must not restate Status purpose or behavior.
- Put the complete public surface and observable result in `interface.md`.
  Put deterministic resolution, inventory and measurement accounting, result
  formation, read-only effects, safety boundaries, and conformance in
  `behavior.md`.
- Link shared contracts and Framework sources. Do not copy their detailed
  spelling or semantics into either Status contract.
- Use no migration requirement IDs, temporary labels, or new identity scheme.
- Keep the mixed `commands/status-command.md` source authoritative. Do not
  delete, move, weaken, or claim cutover of that source in this staging step.
- Use the exact map below as the minimum coverage record. Do not write candidate
  prose until every row and every unresolved fact has an explicit disposition
  that preserves its current uncertainty.

This is an execution recommendation recorded for review, not an acceptance
event.

## Source-Heading-To-Destination Map

The source of truth for this table is the current
`../commands/status-command.md`. The line ranges are navigation aids for the
current revision. Each row covers all prose, lists, tables, code blocks, examples,
conditions, exceptions, and links under that heading, except that separately
listed child headings own their own content. The destination headings are exact
planned headings, not files created by this task. The table is traceability, not
candidate contract prose. In destination cells, `_status.md`, `interface.md`,
and `behavior.md` mean the exact paths `commands/status/_status.md`,
`commands/status/interface.md`, and `commands/status/behavior.md`.

| Authoritative source heading | Current lines | Exact future destination | Coverage and ownership boundary |
| --- | --- | --- | --- |
| Frontmatter | 1-6 | Frontmatter in `commands/status/_status.md`, `commands/status/interface.md`, and `commands/status/behavior.md` | Preserve route description, responsibility, lifecycle classification, and role metadata as file metadata. Do not put contract facts into metadata or add migration IDs. |
| `# Status Command` | 8 | `commands/status/_status.md` — `# Status Command Contract Set` | The title establishes the local contract-set identity. It does not copy the mixed source's purpose or behavior into the entrypoint. |
| `## Status` | 10-21 | `_status.md` — `## Status And Authority`; `interface.md` — `## Status And Boundary`; `behavior.md` — `## Status And Boundary` | Split authority and candidate lifecycle to `_status.md`; public Gate 2 scope and unresolved public boundaries to Interface; behavior-role and no-technology-selection boundaries to Behavior. Preserve the Framework links as links. Do not create three competing status definitions. |
| `## Purpose` | 23-40 | `interface.md` — `## Purpose`; `behavior.md` — `## Operation Boundary And Invariants` | Interface owns the purpose and every question Status answers. Behavior may cite the same heading for determinism and read-only invariants, but does not restate or broaden the purpose. |
| `## Syntax` | 43-52 | `interface.md` — `## Syntax`; `behavior.md` — `## Request Resolution` | Interface owns the exact command form, no operands, and applicability of shared flags. Behavior owns validation of that request and cites Interface; shared flag grammar remains link-only. |
| `## Workspace` | 54-62 | `interface.md` — `## Workspace`; `behavior.md` — `## Workspace Resolution` | Interface owns exact CWD and `--workspace` selection, valid uninstalled subjects, and blocked workspace conditions. Behavior owns establishing that boundary without upward, Git-root, marker, or nearby-workspace discovery. |
| `## Inspection Boundary` | 64-79 | `interface.md` — `## Inspection Boundary`; `behavior.md` — `## Inspection Boundary And Completeness` | Interface owns what Status may inspect and what it explicitly does not inspect. Behavior owns safe per-invocation inspection and incomplete-boundary formation without building the complete graph, parsing ordinary links, or planning mutation. |
| `## Context Inventory` | 81 | `interface.md` — `## Context Inventory`; `behavior.md` — `## Context Inventory Accounting` | Keep the inventory as one public subject. Behavior accounts for the same closed boundary and does not invent a second inventory universe. |
| `### Context File` | 83-115 | `interface.md` — `### Context File`; `behavior.md` — `### Context File Enumeration` | Preserve canonical workspace entry inclusion, `.agents` UTF-8 Markdown inclusion, base and overwrite counting, orphan and non-UTF-8 incompleteness, generated-region treatment, and operational/support exclusions. |
| `### Startup Context` | 117-131 | `interface.md` — `### Startup Context`; `behavior.md` — `### Startup Context Resolution` | Interface owns the complete startup-required closure and its relationship to `context`. Behavior resolves the current target-sensitive closure without reproducing frozen-MVP traversal or rendering content. |
| `### Initial And Current` | 133-149 | `interface.md` — `### Initial And Current`; `behavior.md` — `### Initial And Current Comparison` | Preserve embedded shipped payload versus current workspace, statelessness, signed Difference values, ordering, and neutral customization meaning. Do not turn Initial into a saved snapshot. |
| `### Total Available Context` | 151-166 | `interface.md` — `### Total Available Context`; `behavior.md` — `### Total Available Context Measurement` | Interface owns unique current inventory, metrics, startup percentage, and non-relevance claim. Behavior performs the same measurement and preserves unavailable arithmetic rather than fabricating values. |
| `### Continuity Context` | 168-183 | `interface.md` — `### Continuity Context`; `behavior.md` — `### Continuity Context Accounting` | Preserve continuity as a startup subset that may load at defined boundaries, its required parent and `#LoadNow` files, and the prohibition on adding it to startup totals. |
| `### Largest Continuity Sources` | 185-199 | `interface.md` — `### Largest Continuity Sources`; `behavior.md` — `### Largest Continuity Source Ranking` | Interface owns contribution meaning, human default ordering, tie-breaker, combined base-plus-overwrite size, and the distinction from importance or recommendation. The unresolved structured cardinality remains open; Behavior must not choose a limit. |
| `## Measurements` | 201-229 | `interface.md` — `## Measurements`; `behavior.md` — `## Measurement Availability And Arithmetic` | Preserve Files, Characters, Size, Est. tokens, complete authored-text boundary, UTF-8 and Unicode rules, the planning estimate, rounding/display distinction, and unavailable behavior. Keep implementation of estimation open. |
| `## Workspace Structure` | 231 | `interface.md` — `## Workspace Structure`; `behavior.md` — `## Workspace Structure Accounting` | Keep root customization, managed lifecycle facts, and recovery evidence as separate public summaries under one structure section. Behavior accounts for each without turning Status into diagnosis. |
| `### Root Categories` | 233-251 | `interface.md` — `### Root Categories`; `behavior.md` — `### Root Category Comparison` | Preserve current versus embedded Loader comparison, exact root-route identity and order, added/removed neutrality, and the non-counting of scopes. |
| `### Extensions And Managed Files` | 253-269 | `interface.md` — `### Extensions And Managed Files`; `behavior.md` — `### Extension And Managed-File Accounting` | Preserve distinct installed Extension IDs, absent versus invalid receipt semantics, one state per recorded managed path, shared-owner de-duplication, and exclusion of unmanaged files. Receipt rendering remains unresolved as noted below. |
| `### Recovery Files` | 272-280 | `interface.md` — `### Recovery Files`; `behavior.md` — `### Recovery Evidence Accounting` | Preserve the known-artifact and exact-target boundary, the non-claim about recovery need or success, and the prohibition on inspecting or removing private recovery bytes. Do not invent artifact identity or retention rules. |
| `## Human Output` | 282-335 | `interface.md` — `## Human Output`; `behavior.md` — `## Result Formation And Presentation` / `### Human Rendering` | Interface owns sections, labels, attention wording, compact and expanded content, visible zero values, `none` rules, illustrative status, and next-action requirements. Behavior renders from one typed result and does not choose unresolved streams or compact next-action details. |
| `## Structured Output` | 337-361 | `interface.md` — `## Structured Output`; `behavior.md` — `## Result Formation And Presentation` / `### Structured Rendering` | Interface owns the complete typed fact categories, numeric-versus-unavailable distinctions, shared JSON relationship, and unresolved schema boundary. Behavior forms one result and renders it without rerunning measurement or changing cardinality. |
| `## Semantic Results` | 363-379 | `interface.md` — `## Semantic Results`; `behavior.md` — `## Result Formation And Presentation` / `### Semantic Result Formation` | Preserve all seven semantic states, the attention rendering distinction, valid uninstalled completion, neutral size/category changes, and unresolved numeric exits. |
| `## Errors` | 381-393 | `interface.md` — `## Errors`; `behavior.md` — `## Result Formation And Presentation` / `### Error Formation` | Interface owns every invalid, blocked, and incomplete condition and the required operation, subject, cause, and next-action content. Behavior forms errors without assigning unresolved normal-error streams or guessing unavailable facts. |
| `## Non-Goals` | 395-411 | `interface.md` — `## Non-Goals`; `behavior.md` — `## Operation Boundary And Invariants` | Interface keeps every explicit prohibition and delegation to `doctor` or mutating operations. Behavior enforces the read-only, no-diagnosis, no-graph, no-ranking, no-mutation boundary without adding another prohibition or operation. |
| `## Verification Requirements` | 413-444 | `interface.md` — `## Public Verification`; `behavior.md` — `## Behavioral Conformance` | Interface retains every evidence dimension and its modality. Behavior maps the same dimensions to technology-neutral conformance. Preserve `should` for recommended direct, integration, and Native AOT process allocation; do not strengthen it. |
| `## Related Accepted Direction` | 446-454 | `interface.md` — `## Related Current Sources`; `behavior.md` — `## Related Current Sources` | Keep these as navigation and authority links only. They do not copy Context, global-flag, source-reference, current CLI Working Document, Agenda, or release-plan detail into the Status contracts. |

The parent rows retain their own introductory prose. Child rows retain all child
content. No source line is intentionally left between rows. If the authoritative
source changes, this map is stale and the migration must stop until it is
reconciled again.

## Interface/Behavior Ownership

| Concern | Interface Contract owns | Behavior Contract owns | Must remain elsewhere or link-only |
| --- | --- | --- | --- |
| Public command | Exact `status` syntax, no operands or operation-specific flags, global-flag applicability, defaults, and observable input errors | Request validation and normalized invocation flow that satisfies the public grammar | Full global flag grammar remains in the current shared Global Flags contracts |
| Workspace and inspection | Exact subject selection, supported inspection universe, valid uninstalled state, blocked and incomplete boundaries, and non-goals | Safe workspace establishment, bounded enumeration, inspection disposition, and no alternate-root or complete-graph fallback | Framework loading, routing, scope, and overwrite meaning remains in their authoritative sources |
| Context facts | Startup, Initial, Current, Difference, total, continuity, and largest-source meanings and presentation | Per-invocation closure resolution, deduplication, layer accounting, comparison, ordering, and completeness formation | The `context` closure definition is linked, not copied |
| Measurements | Metric names, units, authored-text boundary, estimate meaning, display rules, and unavailable distinctions | Exact character and byte accounting, deterministic estimate application, signed comparison, percentage preconditions, and non-fabrication | Token-estimation implementation remains a Gate 3 boundary |
| Workspace structure | Root-category, Extension, managed-file, receipt, and recovery facts visible to callers | Per-invocation accounting and neutral status formation without health inference or repair authority | Recovery artifact identity and cleanup policy remain open recovery design |
| Human and structured results | Sections, labels, fields, projections, zero visibility, status wording, next-action requirement, and schema boundary | One typed result consumed by both renderers without rerunning collection or changing semantics | Normal human/error streams and structured largest-source cardinality remain unresolved |
| Statuses and errors | All semantic states, conditions, error content, and numeric-exit boundary | Formation of the selected semantic state from complete facts and safe failure conditions | Numeric exit mapping remains Gate 3 work |
| Non-goals and evidence | Public prohibitions and the complete verification inventory with original modality | Technology-neutral conformance and evidence allocation without selecting libraries or source modules | .NET source boundaries remain open and have no Technical Design file here |
| Authority and routing | Nothing beyond links to the source and contract-set roles | Nothing beyond links to Interface and the shared sources | `_status.md` alone states candidate authority and exposes generated navigation |

Behavior may cite an Interface heading for a public condition, but it must not
maintain a second detailed definition of that condition. Interface may cite
Behavior for conformance, but it does not move implementation choices into the
public contract.

## Shared Link-Only Rules

- **Global flags.** `interface.md` may state that all six accepted global flags
  apply to `status` and link to `../commands/shared/global-flags/interface.md`
  and its Behavior contract. It must not copy their
  spelling, repetition, terminal-mode, no-op, output, or error rules. The shared
  rule that JSON writes one structured result to stdout remains authoritative.
  It does not settle normal human or ordinary-error streams.
- **Context closure.** Startup and continuity sections link to the current
  [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md) and, where exact Framework meaning matters, to the
  loading, routing, scope, and overwrite sources named by the Status source.
  Status measures the relevant closure without rendering context content and
  without importing the Context command's complete projection or link grammar.
- **Source identity.** Status does not accept source-reference operands. If its
  largest-source result exposes the source identity defined by the authoritative
  source, the current shared [Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md) rather than defining
  another identity or migration label. The source's automatic ID remains a
  product fact; no requirement IDs are added.
- **Operation shape.** The current CLI command-contract Working Documents and
  the Shared CLI Operation Contract define the structural boundary. Link to them
  for role, typed-result, and locality rules; do not copy their prose into the
  Status contract or treat a link as command authority.
- **Program state.** The Decision Agenda and migration ledger remain evidence
  for accepted direction and migration state. They are not replacement sources
  for Status facts. Link to them from review material, not as a second detailed
  Status definition.
- **Contract boundary.** Interface and Behavior link to each other at exact
  headings. Behavior cites Interface meaning and adds only conformance flow.
  Neither file copies a shared contract, and `_status.md` does not copy either
  file's details.
- **Link meaning.** Every link retains the linked source's authority, scope,
  loading behavior, responsibility, and lifecycle. A link cannot make a
  candidate accepted or make generated navigation authoritative.

## Preserved Unresolved Facts

These are deliberately carried forward. This council does not choose a value,
wording, stream, schema, cardinality, implementation, or exit for any of them.

| Unresolved fact | What the authoritative sources establish | What remains open and must not be invented |
| --- | --- | --- |
| Normal human and ordinary-error streams | Status defines human and structured content and describes complete errors. The shared JSON rule assigns structured stdout. | The normal stream for human results and the ordinary stream for human-readable errors are not assigned by the Status source. Candidate Interface and Behavior assign neither. |
| Structured largest-source cardinality | Default human output shows at most three logical continuity sources, ordered by contribution and source identity. Structured output exposes ordered continuity-source contributions. | Whether structured output is also capped at three or carries every contribution is unspecified. Behavior must not choose a limit from the human example. |
| Zero and unavailable arithmetic | Measurements distinguish zero, unavailable, and not-applicable values; Difference is signed current minus initial; startup percentage has a stated byte ratio. | Propagation and rendering when one input is unavailable, when both values are unavailable, when the total is zero, or when a percentage denominator cannot be used remain unspecified. No partial or fabricated arithmetic is allowed. |
| Compact next actions | Compact view retains required next-action information, and errors provide a useful next action when one exists. | The exact conditions, wording, ordering, and representation of compact next actions are not defined. Do not infer them from another command. |
| Receipt absence rendering | An absent Extension receipt means zero recorded Extensions. An invalid receipt makes managed facts unavailable. | Exact human and structured rendering for an absent receipt, including interaction with `none`, zero, unavailable, and managed-file state rows, remains open. Do not change the established absent-receipt fact while filling the rendering gap. |
| Structured schema and compatibility | JSON exposes the same typed facts as human output and keeps numeric and unavailable distinctions. | Exact field names, schema version, compatibility rules, and serialization details remain Gate 3 work. |
| Numeric exits | Semantic result categories are accepted and structured `attention` remains `attention`; human output says `requires attention`. | Numeric process-exit mapping remains Gate 3 work. No candidate file may choose numeric values. |
| Token-estimation implementation | The planning estimate is `ceiling(characters / 4)` and is explicitly not a model tokenizer, billing value, context guarantee, latency estimate, or provider count. | The implementation, rounding details beyond the stated display rule, and any .NET/library boundary remain open. Do not turn the estimate into a provider-specific claim. |
| Recovery artifact identity and retention | Status counts only recovery evidence the running CLI can identify around exact Open Forge targets and does not remove or inspect private bytes. | Exact artifact identities, retention, cleanup, and recovery-file discovery rules remain Gate 3 recovery and cleanup work. Do not name an artifact or promise retention behavior. |
| .NET source boundaries | .NET Native AOT is the accepted canonical implementation direction for the CLI, while the Status source leaves its .NET source boundaries open. | No Status-specific module, parser, serializer, filesystem abstraction, or other concrete implementation choice is accepted here. This is why no Technical Design is proposed. |
| Bounded verbose diagnostics | `--verbose` may add bounded diagnostic evidence without changing collection, ordering, semantic result, or exit behavior; exact shared redaction remains open. | Exact Status diagnostic fields, stream handling for diagnostics, and redaction remain shared or Gate 3 work. Do not put them into a Status-specific design. |

## Likely Loss Traps

- Treating a generated `Entries` line, a routed candidate, or a complete review
  file as an authority change.
- Putting purpose, measurements, or output summaries in `_status.md` and thereby
  making the entrypoint a competing contract.
- Dropping the distinction between the mixed source's authority and the
  candidate files' accepted-but-non-authoritative meaning.
- Mapping only top-level headings and losing the six Context Inventory
  subsections, the three Workspace Structure subsections, or the content beneath
  a parent heading.
- Losing tables, code examples, illustrative-value warnings, exact labels,
  ordering rules, non-goals, or verification conditions while moving prose.
- Turning `should`, optional, illustrative, unavailable, incomplete, or
  unresolved language into `must`, exact, zero, complete, or accepted behavior.
- Counting a base and overwrite companion incorrectly, treating an orphan as
  available, losing logical-source combination in largest-source ranking, or
  confusing contribution with importance.
- Adding continuity metrics to startup totals, treating continuity as every
  request, or replacing the target-sensitive closure with frozen-MVP traversal.
- Treating Initial as a historical receipt or saved installation snapshot,
  inventing a scope count, or interpreting added and removed root categories as
  health findings.
- Replacing unavailable metrics with zero, performing signed arithmetic across
  unavailable inputs, dividing by a zero total, or presenting partial counts as
  complete.
- Assuming the human three-source example defines the structured cardinality.
- Filling in compact next actions, receipt-absence presentation, human/error
  streams, schemas, numeric exits, recovery identities, or .NET boundaries from
  convention or another command.
- Copying global flag, Context, source-reference, Framework, or current CLI
  Working Document detail and letting the copy drift from its shared authority.
- Moving public result facts into Behavior or adding behavior facts to Interface
  without a cross-layer link, producing two definitions of one condition.
- Adding a Technical Design merely because the CLI has a .NET Native AOT
  direction, even though Status has no concrete implementation choice.
- Assigning new requirement IDs, reusing another command's temporary labels, or
  treating source line numbers as durable identity.
- Turning Status into `doctor`, a recommendation engine, a graph builder, a
  mutation planner, a repair operation, or a complete route/link validator.
- Claiming cutover, deleting the mixed source, changing the migration ledger,
  or regenerating navigation as part of this contextual record.

## Verification Checklist

Before any later candidate drafting or cutover review, verify all of the
following:

- [ ] The exact current Status source, Decision Agenda, migration ledger, shared
      contracts, Loader, routing rules, writing rules, and council style were
      consulted again if any source changed.
- [ ] The map still covers the frontmatter and every one of the twenty-five
      source headings, including every Context Inventory and Workspace Structure
      subsection, with no unowned line or nested content.
- [ ] Every source table, list, code block, example, link, status condition,
      error condition, non-goal, and verification sentence has an exact
      destination or an explicit link-only disposition.
- [ ] `commands/status/_status.md` is the only entrypoint destination and
      contains only lifecycle, authority, and generated navigation. Its `Entries`
      region is navigation and is not treated as source authority.
- [ ] `commands/status/interface.md` contains the complete public syntax,
      workspace, inspection, inventory, context, measurement, structure,
      human, structured, semantic-result, error, non-goal, and verification
      coverage named in the map.
- [ ] `commands/status/behavior.md` contains only technology-neutral request,
      inspection, accounting, result-formation, read-only, safety, and
      conformance responsibilities that cite Interface meaning.
- [ ] No `commands/status/technical-design.md` exists or is required by a
      Status-specific concrete choice.
- [ ] Shared global flags, Context closure, source identity, Framework loading,
      routing, scope, overwrite, and current CLI Working Document detail are linked once rather than
      copied into competing definitions.
- [ ] No migration requirement ID, temporary label, or new identity scheme was
      introduced. Existing automatic source identity is handled only through its
      shared contract.
- [ ] All unresolved stream, structured-cardinality, arithmetic, compact-next-
      action, receipt-rendering, schema, exit, token, recovery, verbose, and
      .NET boundaries remain explicitly unresolved.
- [ ] Requirement strength and uncertainty remain exact, including the source's
      `should` recommendations for direct, integration, and built Native AOT
      process evidence.
- [ ] Evidence coverage includes exact CWD and explicit workspace selection;
      installed, uninstalled, incomplete, and unsafe workspaces; embedded
      Initial measurement; current startup and continuity closure; file, layer,
      character, UTF-8, size, and estimate accounting; signed differences,
      zero, unavailable, and percentage cases; and non-additive continuity.
- [ ] Evidence coverage includes largest-source combination, ordering, ties,
      fewer-than-three cases, root additions and removals, absent/valid/invalid
      Extension receipts, current/changed/missing/shared managed files, and
      zero/present/unknown/unsafe recovery evidence.
- [ ] Evidence coverage includes every semantic result, human and structured
      rendering from one typed result, stable repeat invocation, and proof that
      Status does not parse ordinary links, build the complete graph, inspect
      unrelated files, or mutate anything.
- [ ] The mixed source remains authoritative, no cutover is claimed, and no
      generated navigation or other repository file is changed by this record.

## Hard Stops

Stop the migration and return to evidence if any of these conditions occurs:

- A source heading, subsection, paragraph, table, example, link, or verification
  obligation has no exact destination or link-only disposition.
- A destination weakens, strengthens, combines, or invents a requirement,
  condition, exception, ordering rule, ownership boundary, or uncertainty.
- A candidate assigns any currently unresolved stream, cardinality, arithmetic,
  next-action, receipt-rendering, schema, exit, token, recovery, verbose, or
  .NET value by inference.
- `_status.md` contains Status contract detail, a competing definition, or an
  authority claim beyond candidate lifecycle and routing.
- Interface and Behavior both define the same public fact in detail, or a shared
  contract is copied instead of linked.
- A Technical Design is added without a concrete Status-specific implementation
  choice, or a concrete implementation choice is put into Behavior.
- A migration requirement ID, temporary label, or replacement identity system is
  introduced for Status.
- The candidate path differs from `commands/status/`, or a grouped-command
  interpretation creates an empty or ceremonial contract scope.
- The mixed source is deleted, weakened, moved, or treated as non-authoritative
  before the ledger's exact coverage, conflict resolution, independent review,
  and explicit maintainer-accepted cutover conditions are met.
- This review task would require editing the migration ledger, Decision Agenda,
  generated navigation, the authoritative Status source, or any file other than
  this new record. Those are separate authorized changes.

## Related Sources

- [Status contract set](../../../crystallized/documents/cli/contracts/status/_status.md)
- [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md)
- [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
- [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md)
- [CLI Decision Agenda](../../../working/cli-release/decision-agenda.md)
- [CLI Contract Migration Ledger](../contract-migration-ledger.md)
- [CLI Review Queue](queue.md)
- [Archived Command Contract Set Council](command-contract-set-council.md)
- [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
- [Nearest Shared Scope Pattern](../../../../patterns/software/source-locality/nearest-shared-scope.md)
