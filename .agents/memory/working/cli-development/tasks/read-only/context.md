---
open-forge:
  description: Implement ordered context selection, exact content projection, and explicit contained-link expansion
  tags: [Memory, Working, CLI, Task, Context, ReadOnly, Loading, Contextual]
---

# Implement Context

## Task State

- State: Complete. Accepted feature tip `303ad7d897de78f55d19aa7b5bca0da9ff786ed9`
  is squash-integrated into local `develop` at
  `ca097a24d9d7fdc30e2872ba35c8ca96175b2319`. The final integrated correctness
  review found no material behavior, regression, CLI-contract,
  Native-AOT/source-generated-JSON, safety, or C# Directive issue. Contract and
  Preflight are
  `0e04061`, callable Gray is `39827cc`, independently failing Red evidence is
  `f16dc2d`, closure/projection Green is `716afc8`, the grouped loading repair is
  `d45da71`, link expansion is `e8a82cb`, presentation/public evidence is
  `b3d5ced`, and the final behavior and C# Directive-conformance increment is
  `ce459b2`. The first grouped review correction is `c350c23`; the routed
  completeness, logical-section, and exact-case rereview correction is
  `823aecd`.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/context/interface.md), [Behavior](../../../../crystallized/documents/cli/contracts/context/behavior.md), and [Technical Design](../../../../crystallized/documents/cli/contracts/context/technical-design.md).
- Responsible role: Context Task Mastermind.
- Workspace: isolated `codex/cli-context` worktree at exact base
  `32069d3e0516cde007bd35f31da82b88ff613759`.
- Profile: Assured because the command freezes a public JSON surface and crosses
  source identity, physical containment, strict UTF-8, Markdown range, and Native
  AOT boundaries.

## Expected Outcome

`context` implements its accepted direct-root public binding and projects exact
startup-required and explicit-route closures, ordered physical layers, every
inclusion reason, selected authored or derived content, and explicit contained
link expansion without reading more bodies than the request requires.

The Crystallized Context Interface and Behavior control this Task. Context does
not add byte or character measurements, token estimates, or a
startup/selected/automatic/later measurement profile. The Interface's
`token-friendly` wording describes compact rendering density only.

## Architecture

- Keep command models at `Commands/Context/` and supporting behavior below its
  narrowest `Shared/<Capability>/` scope.
- Consume the accepted source catalogue, source-reference resolver, route facts,
  document reader, Markdown facts, and contained local-reference facts. Promote
  only an exact neutral loading-closure or reference-graph unit when Context is
  the real second consumer, and migrate the existing consumer without behavior
  change.
- Model logical source identity, physical layer, inclusion reason, route state,
  link edge, projection state, coverage, finding, and ordering as separate
  immutable facts.
- Keep request binding, closure policy, result formation, rendering, diagnostics,
  help, finding/status meaning, and next actions local to Context.
- Form one typed result before presentation. Serialize only the exact concrete
  source-generated Context JSON graph.

## Requirements

Implement the accepted startup-required closure, explicit-route closure,
startup-relative `--additions-only` difference, content grammar and canonical
projection order, positive-depth or `all` breadth-first link expansion,
base-before-overwrite layering, deduplication with retained reasons, compact and
expanded views, JSON, bounded diagnostics, help, findings, statuses, streams,
exits, and next actions. Preserve complete empty results, safe partial evidence,
unavailable projections, and unsafe boundaries as distinct typed states.

## Behavior And Acceptance Matrix

| Boundary | Required behavior |
| --- | --- |
| Binding | One direct root `context`; zero or more source-reference operands; exact `--additions-only`, `--content`, and `--follow-links` occurrence and value rules; shared global flags unchanged |
| Startup | Workspace entry, Loader, visible `#LoadNow`, applicable target-sensitive `#KeepInMind`, missing ancestors, and adjacent overwrite layers in exact loading order |
| Selection | Explicit closures follow operand order and retain their own ancestors, selected target, visible `#LoadNow`, scope-local loading, and every inclusion reason without activating unrelated scopes |
| Additions | Resolve startup and selected closures in one invocation, expand both with the same link depth, subtract startup canonically, and preserve a complete empty difference |
| Links | Follow only explicit contained local Markdown destinations in stable breadth-first order; retain cycles, duplicates, external unchecked observations, broken edges, fragments, containment, and overwrite-target meaning |
| Projection | Canonical paths-first, source, layer, metadata/frontmatter/headings/body/section order; exact authored text; structural ATX and Setext headings; missing, unavailable, and ambiguous section states |
| Result | One ordered typed result with exact schema-v1 Context object, fixed finding vocabulary/order, seven statuses, deterministic next action, compact/expanded parity, and source-generated JSON |
| Safety | Real filesystem and strict UTF-8; physical containment; no network fetch, cache, index, session, receipt, lock, recovery artifact, or workspace write |
| Runtime | Warning-free Release, unchanged affected commands, deterministic published process behavior, and supported local `linux-x64` Native AOT root/Integration/EndToEnd execution |

## Ownership And Paths

Expected production paths are `src/cli/core/OpenForge.Cli.Core/Commands/Context/**`
and their mirrored Unit, Integration, and EndToEnd Context test paths. The Task
may change the root composition registration, `CliJsonContext`, direct-root help
and registration tests, and public EndToEnd project only where complete Context
proof requires them.

An exact neutral loading, closure, reference-graph, or Markdown fact unit may move
to `Framework/Sources/**` or `Framework/Documents/Markdown/**` only after Context
proves it is a second consumer. The existing Route Inspect or References consumer
must migrate in the same coherent increment with unchanged behavior and affected
regressions.

Protected paths and meaning are `Framework/Lifecycle/**`,
`Framework/Extensions/**`, `Commands/Extension/**`, package catalogue/resource
meaning, mutation, generated-navigation application, recovery, shared Plan,
Checkpoint, task indexes, parent Task records, Crystallized architecture, and
unrelated command contracts. Context must not change accepted Find, References,
Route, Shell, global-flag, source-reference, package, or project meaning.

The authorized contract correction is limited to freezing the missing exact
Context command-local JSON result and aligning the stale generic-assistance
sentence in the Framework loading document with the already accepted Context
command. Both require explicit review as integration-sensitive prose.

## Evidence

The evidence ladder is:

1. Contract and binding Unit evidence for exact symbols, grammar, occurrence
   policy, immutable result shapes, finite mappings, closure/set algebra,
   ordering, projection, rendering, diagnostics, help, and source-generated JSON.
2. Focused Integration evidence over owned real temporary workspaces for loading
   routes, sparse scopes, overwrites, strict UTF-8, unreadable sources, Markdown
   headings/sections/links, fragments, cycles, aliases, containment, repeat
   determinism, and recursive unchanged snapshots.
3. Published EndToEnd evidence for representative complete, attention,
   incomplete, invalid, and blocked public journeys, human/JSON stream and exit
   isolation, help, diagnostics, and recursive no-write snapshots.
4. Affected Route Inspect, Find, References, Shell, serialization, root-help, and
   direct-root regressions for every promoted or neighboring change.
5. Locked restore, warning-free Release build, format verification,
   `git diff --check`, full managed Unit/Integration/EndToEnd, concrete Context
   JSON serialization, project/package/generated/protected-surface audits, and
   supported local `linux-x64` Native AOT root/Integration/EndToEnd execution
   with zero skips.

Maximum budgets are one independent correctness review, one conditional local
improvement review only when a named material trigger exists, no council, and one
grouped correction pass. No review or correction ID is consumed at Preflight.
The Context Task Mastermind is the continuous implementation owner.

## Stop Conditions

Stop and return a project change request before changing public or cross-task
contracts, accepted product behavior, dependency direction, protected meaning,
shared safety or identity meaning, or another Task's assumptions. Stop before
adding provider-specific tokenization, measurements, body caching, semantic
ranking, workspace databases, a persistent graph, a fake filesystem, reflection
serialization, or a second route/loading/reference/Markdown model.

The exact JSON result, finding vocabulary, finite values, and next actions below
the accepted envelope were approved and committed as the contract-freeze
boundary in `0e04061`. Gray owns the callable production surface, Red freezes
independently selectable failing evidence at the operation seam, and Green then
implements the accepted behavior without changing that public surface.

## Candidate Seal

- `context` is one direct-root command with the accepted startup and explicit
  closures, startup-relative additions, canonical content projections,
  breadth-first link expansion, compact/expanded/JSON presentation, bounded
  diagnostics, help, seven statuses, exact stream/exit behavior, and recursive
  real-filesystem no-write evidence.
- Context-local policy remains below `Commands/Context/**`. Exact neutral
  generated-Entries, Open Forge metadata, and contained-link resolution facts
  moved to `Framework/Sources/**` only after Route Inspect or References became
  their unchanged second consumer. Root composition and `CliJsonContext` are the
  only shared integration bindings.
- The grouped correctness repair makes every selected ancestor entrypoint seed
  visible `#LoadNow` traversal and preserves Route Inspect's historical rejection
  of whitespace-only outer generated-Entries lines. Later edge evidence also
  freezes invalid-source startup short-circuiting, authored finding order,
  orphan-overwrite ambiguity, exact-path ID collision attention, safe local
  target case-mismatch recovery, and unavailable authored-frontmatter state.
- C# Directive audit `OF-CS-CTX-01` removed every ordinary null suppression in
  the changed boundary. `OF-CS-CTX-02` split rendering, link traversal/support,
  and result/JSON model topics while retaining only cohesive ordered policy
  owners. The construction audit uses `required init` for independently supplied
  data shapes, named arguments for ambiguous invariant-bearing construction,
  cohesive-source forwarding for link/closure formations, and ordered control
  flow instead of nested conditional expressions. No custom conversions,
  speculative bags, reflection, dependency injection, raw-process-argument
  reparsing, fake filesystem, cache, database, provider tokenizer, production
  write, or network path was added.
- Locked restore passes. Release solution build is zero-warning/zero-error.
  With the inherited stale `DOTNET_ROOT` removed so Roslyn uses the pinned .NET
  10 SDK, full-solution `dotnet format --verify-no-changes`, `git diff --check`,
  changed-path, protected-surface, project/package, source-generation, and
  reflection-disabled serialization audits pass.
- Focused Context evidence passes Unit `46/46`, composed Integration `13/13`,
  concrete source-generated JSON Integration `1/1`, and published EndToEnd
  `13/13`. Focused changed-surface References and Route Inspect regressions pass
  within the affected broad Unit `325/325` and Integration `180/180` selections.
  Full managed projects pass Unit `927/927`, Integration `322/322`, and EndToEnd
  `95/95`, all with zero skips.
- Supported `linux-x64` Native AOT root, Integration, and EndToEnd publications
  pass with the locked `--no-restore` boundary. The ELF64 x86-64 executables
  execute successfully: root and Context help pass, native Integration is
  `322/322`, and native EndToEnd is `95/95`, with zero skips. Their SHA-256 values
  are `e64249db54a4413819070ac8df5bd09280495a32d858624caa731cf9de3b62ab`,
  `f0ff85aedfea1f743c442cc623b665d712afc974b754a8ab5456176f678059a6`, and
  `4ec8ced4e41a1fcf08555b3b7bfe407e8d7acc970bf37ff2df4e5f190e611e49`.
- The vulnerable transitive package audit reports no packages. The C# route
  identity resolves as routed. Legacy `open-forge doctor` reports zero errors
  and the one known protected mixed inherited/local C# Axiom warning. This Task
  does not edit that protected generated/index surface.

## Grouped Correctness Correction

The accepted `CTX-FR-01` through `CTX-FR-05` correction is implemented in
`c350c23` on top of the immutable-base rendering-audit tip `670d3a9`:

- `CTX-FR-01`: startup formation now observes global continuity membership for
  every logical source. Unavailable or malformed Open Forge metadata and
  unavailable or ambiguous routes for authored global `KeepInMind` sources emit
  ordered `context.closure-unavailable` findings, make selection and overall
  coverage incomplete, retain safe sources, and select the exact incomplete
  next action instead of reporting false completeness.
- `CTX-FR-02`: Context no longer scans raw token spellings or owns fallback
  option parsing. Shell supplies the typed effective view and occurrence facts;
  Context consumes only parser-owned source, content, follow-link, and global
  option values.
- `CTX-FR-03`: the neutral contained-link input no longer carries unused source
  or layer-physical-path values. Independently supplied neutral identity,
  input, target, finding, and fact shapes use public `required init` members and
  named object-initializer construction. Remaining constructors retain actual
  validation or identity invariants and ambiguous call sites are named.
- `CTX-FR-04`: `ContextClosureResolver` is now only the closure coordinator.
  Requested-source resolution and finding mapping, selected-source
  accumulation, and loading-closure traversal are cohesive collaborators below
  `Shared/Selection`; link traversal, projection, JSON projection, and result
  formation retain their separate single-policy owners.
- `CTX-FR-05`: the added direct, real-filesystem Integration, and published
  evidence covers failed and interrupted terminal formation, Setext headings,
  duplicate and matching base/overwrite sections, fragment failure, bounded
  depth two and three, multiple explicit routes, sparse and nested loading,
  malformed and unreadable global-continuity metadata, exact JSON/status/stream
  behavior, and recursive no-write snapshots.

The post-review C# Directive audit also removes Context micro-append rendering,
per-value formatting inside coherent templates, ordinary null suppression,
nested conditional expressions, and ambiguous same-type construction. Fixed
human rows and terminal projections have byte-exact evidence. The remaining
materially large owners are cohesive: loading closure owns ordered loading
traversal, projection owns canonical part formation, JSON projection owns the
one source-generated wire graph, link expansion owns breadth-first traversal,
and result formation owns status/coverage precedence.

### Mandatory Interface Evidence Map

| Interface verification boundary | Durable executable evidence |
| --- | --- |
| CWD and `--workspace`; source IDs, exact paths, collisions, and disambiguation | Context binding and finding Unit evidence; composed workspace and published semantic journeys; full shared source-reference regressions |
| Startup order, target-sensitive entrypoint and global leaf `KeepInMind` | `ContextOperationRedTests` exact startup closure plus `ContextOperationFindingTests` malformed, unreadable, and broken global-continuity cases |
| Single/multiple explicit closures and sparse/nested scopes | Existing explicit closure Unit cases and `ContextApplicationIntegrationTests` multiple-route real-workspace case |
| Base/overwrite order, matching sections, and orphan failure | Exact projection/overwrite Unit evidence, duplicate-versus-matching section case, and orphan-overwrite finding case |
| Additions difference, empty difference, and invalid no-source use | Binding semantic-error matrix and startup-relative additions/link-expansion Unit cases |
| Content grammar, every part, combinations, repetition, and canonical order | Binding contract matrix, exact projection Unit evidence, ordered JSON Integration, and generated schema test |
| Exact authored bytes; ATX/Setext; section visibility, case, bounds, absence, ambiguity, and base/overwrite independence | Projection and presentation Unit evidence including Setext, duplicate/matching sections, unavailable frontmatter, exact human bytes, and published authored-content journeys |
| Link depth 1, 2, higher bounded, and `all`; cycles, duplicates, fragments, external, broken, encoding, and containment | Existing link graph/containment cases plus new depth-two/depth-three direct and published cases and published fragment failure |
| Stable ordering, deduplication, inclusion reasons, and visible broken edges | Startup/explicit/link Unit order assertions, same-code finding order, composed JSON order, and repeat published JSON |
| Seven statuses, coverage/safety precedence, partial safe facts, findings, and next actions | Binding/direct terminal Unit cases, real-filesystem failed/interrupted Integration, public semantic Integration, published attention/incomplete/invalid/blocked, and full status-definition mapping |
| Compact/expanded framing and same-result human/JSON rendering | Byte-exact compact/expanded presentation Unit evidence, frozen JSON property order, and view-neutral published JSON |
| Human/JSON streams, exits, bounded diagnostics, repeatability, and no writes | Operation Integration terminal projections, published semantic/verbose/repeat tests, recursive snapshot helpers, and managed plus Native AOT EndToEnd suites |

No mandatory Interface verification row remains without direct, Integration, or
published executable coverage at the correction tip.

## Rereview Correction

The accepted `CTX-RR-01` through `CTX-RR-03` correction is implemented in
`823aecd`:

- `CTX-RR-01`: startup completeness now uses established route applicability
  before treating unavailable Open Forge metadata as unresolved global
  continuity membership. A definitively unrouted source cannot invalidate
  startup coverage, while routed, ambiguous, or unavailable route facts retain
  the existing incomplete uncertainty. Direct and composed real-filesystem
  evidence covers malformed metadata on an unrelated unrouted source and keeps
  the existing malformed, unreadable, broken, and ambiguous applicable cases.
- `CTX-RR-02`: physical layers retain their independent available or missing
  section projections, but `context.section-missing` is formed once only after
  the requested section is proven absent from every completely inspected layer
  of the logical source. The four base/overwrite combinations prove base-only,
  overwrite-only, both, and neither, including one logical-source finding only
  for neither.
- `CTX-RR-03`: exact target spelling is checked after neutral `complete`,
  `missing`, and `fragment-missing` resolutions. Portable direct evidence drives
  both complete and missing initial facts through the real directory-component
  spelling reader on every operating system; the prior Windows early return is
  removed.

At the correction boundary, locked restore, full-solution format verification,
warning-free Release build, `git diff --check`, concrete source-generated JSON,
package, project, reflection-disabled serialization, protected-surface, route,
doctor, and no-write audits pass. Full managed Unit `927/927`, Integration
`322/322`, and published EndToEnd `95/95` pass with zero skips. Supported local
`linux-x64` Native AOT root help and Context help execute, Integration `322/322`
and EndToEnd `95/95` pass with zero skips, and the executable hashes are recorded
in Candidate Seal.
