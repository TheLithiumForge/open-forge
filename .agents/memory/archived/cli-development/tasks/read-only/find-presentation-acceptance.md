---
open-forge:
  description: Bind, present, and accept the non-shipping Find command through its public views, JSON, diagnostics, and evidence
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Find, ReadOnly, Presentation, Acceptance, NativeAOT]
---

# Present And Accept Find

## Task State

- State: Complete and accepted in the commit containing this record update. [Implement
  The Find Query Operation](find-query-operation.md) is complete and accepted at
  exact commit `ff7ce3f` (`Accept Find query operation`).
  Child 3 focused Preflight is complete and accepted at exact `28d316a` (`Freeze
Find presentation preflight`). Gray contracts and stubs are accepted at exact
  `a76a217` (`Establish Find presentation contracts`). The original Red evidence
  packet is accepted at exact `22d3bff`; its Integration metadata correction is
  accepted at exact `6a9a0de`, and the mirrored EndToEnd metadata correction is
  accepted at exact `eea3d59` (`Complete Find presentation metadata evidence`).
  The post-commit Red reproduction succeeded as intentional Red: managed
  non-AOT `win-x64` publish passed, and published Find EndToEnd was `13` total
  with `13` intentional failures and zero skips, all terminating at absent Green
  root registration. Supplemental escaping Red correction is accepted at exact
  `a865fd1` (`Correct Find escaping evidence`). Child 3 Green is accepted at
  exact commit `cb7874c` (`Implement Find presentation`) over corrected Red
  `a865fd1`. It is the production/root-composition-only Green; no test, support,
  contract, project, package, configuration, generated-routing, or Route behavior
  change enters Green. Blue is accepted at exact `3f81e76`
  (`Simplify Find JSON projection`). It changes production structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it replaces the duplicate local finite status and finding-code switches with
  `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`, then removes the two duplicate private
  mapping methods. No behavior, public output/order/schema, test/support,
  contract, package/project/configuration, generated routing, Shell/root, Route,
  workspace-write, or Native AOT change occurs. Blue evidence is a warning-free
  Release solution build, format verification, diff check, focused Unit `206/206`,
  and focused Integration `43/43`, all with zero skips; no managed republish or
  Native AOT claim is needed. Fresh bounded correctness review is `PASS`: all 7
  status and 17 finding-code mappings and undefined-value exception behavior are
  exact; JSON model/property order/context is unchanged; static canonical readers
  remain source-generation/AOT-safe. Fresh local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`. Fresh Purple assessment ran read-only
  from exact clean Blue `3f81e76` against the exact ten Find Child 3 test/support
  surfaces: six Unit surfaces (`DirectRootLeafRedTests`,
  `FindPresentationBindingRedTests`, `FindHumanRenderingRedTests`,
  `FindJsonRenderingRedTests`, `FindDiagnosticsAndHelpRedTests`, and typed
  `FindPresentationTestData`), two Integration surfaces
  (`FindApplicationIntegrationRedTests` and `FindGeneratedSerializationRedTests`),
  and two EndToEnd surfaces (`PublishedFindProcessRedTests` and
  `PublishedFindWorkspace`). Verdict: `NO_MATERIAL_IMPROVEMENTS`; the no-op Purple
  acceptance is recorded at exact `426d4f5`. No
  test/support, production, contract, project, package, configuration, generated,
  Route, Shell, or root file changed, and no Purple code/test commit is
  manufactured. Unit tests and typed `FindPresentationTestData` are coherent at
  nearest scope; splitting the fixture fragments authority. Integration and
  EndToEnd workspace/assertion helpers are intentionally project-local. The
  supplemental diagnostic token-boundary fixture/parser is local, independent,
  structurally sound, and proves complete escapes, bounds, one-line output, and
  payload exclusion. Minor wrappers and repeated status expectations are
  preference-only independent contract oracles. Purple evidence from clean Blue
  is focused Unit `206/206`, focused Integration/serialization `43/43`, and
  published Find EndToEnd `13/13`, all with zero skips; source diff/check against
  `3f81e76` is clean/empty. No Native AOT claim is made, and the record-only
  commit is not a test change.
- Responsible role: Mastermind. The Mastermind owns composition, integration, and
  final acceptance; delegation is allowed only after the predecessor packet is
  closed.
- Parent: [Implement Find](find.md).
- Predecessor: [Implement The Find Query Operation](find-query-operation.md).
- Task source: This file.
- Last updated: 2026-08-25.
- Planning baseline: `e77902a`; initial production/source baseline: exact
  `063c59d`; execution baseline: exact `ff7ce3f` (`Accept Find query operation`).

## Expected Outcome

Register one `open-forge find` leaf, project its one typed result through compact,
expanded, source-generated JSON, diagnostics, and help, and close complete Find
acceptance without changing Shell semantics, Route behavior, or workspace bytes.

## Relationships And Authority

| Relationship             | Link                                                                                                                                                                                                                                                               | Relevance                                                                                   |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| Parent                   | [Implement Find](find.md)                                                                                                                                                                                                                                          | Defines the complete command outcome and final gate.                                        |
| Predecessor              | [Find Query Operation](find-query-operation.md)                                                                                                                                                                                                                    | Supplies the one callable operation and typed result.                                       |
| Interface                | [Find Interface](../../../../crystallized/documents/cli/contracts/find/interface.md)                                                                                                                                                                               | Defines the exact public schema, projections, findings, statuses, and `next`.               |
| Architecture             | [CLI Architecture](../../../../crystallized/documents/cli/architecture.md)                                                                                                                                                                                         | Defines the shared envelope, location, streams, exits, source generation, and AOT boundary. |
| Shared operation         | [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md)                                                                                                                                                               | Defines one invocation, renderer, output, and completion flow.                              |
| Implementation and tests | [CLI Implementation Directive](../../../../../directives/open-forge/cli/implementation.md), [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md), and [Evidence Tiers](../../../../../patterns/testing/evidence-tiers.md) | Bind registration, source generation, isolation, and public evidence.                       |
| Continuity               | [CLI Development Plan](../../plan.md) and [CLI Development Checkpoint](../../../checkpoints/cli-development.md)                                                                                                                                                    | Record integrated completion and the handoff to later read-only commands.                   |

## Allowed And Protected Surfaces

### Allowed

- New Find binding, presentation models, rendering, help, and the exact root
  registration in `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`.
- The bounded neutral direct-root correction in Shell/Parsing: only
  `CliRootLeaf`, `CliCommandTree`, and `CliRootDefinitionFactory`.
- One `CliJsonContext` attribute/import for the concrete Find document graph.
- Find Unit, real-OS Integration, and published-process EndToEnd evidence, plus a
  bounded Shell direct-root regression. TestSupport may change only if identical
  cross-project fixture meaning is proved.
- The active Working records required for final Find integration and acceptance.

### Protected

- All Shell and global parser behavior, including terminal, workspace, pipeline,
  status, stream, and exit semantics, is protected except for the bounded neutral
  root-leaf attachment. Route group, list, and inspect behavior and JSON; project
  graph, package versions, unrelated commands, and every public contract or
  Architecture source are also protected.
- Operation behavior and result formation from Child 2. Presentation must consume
  one result and must not rerun the operation.
- The Core Markdig and trimming flow established by Child 2. This child consumes
  that flow and proves it only at the final Native AOT gate; it does not modify it.
- Generated `Entries` and routing projections, including their generated regions,
  remain unchanged.
- Workspace contents, metadata, indexes, caches, network state, and every write
  effect.

## Accepted Child 3 Focused Preflight

This read-only Preflight is complete and accepted at exact `28d316a` (`Freeze Find
presentation preflight`). It freezes the Child 3 callable, composition,
presentation, JSON, help, diagnostic, evidence, file, and phase boundaries. No
Child 3 production or test mutation existed at that boundary; Gray followed.

### Child 2 Final Acceptance Baseline

- The exact execution baseline is `ff7ce3f` (`Accept Find query operation`). Its
  existing final evidence is locked restore; a warning-free Release build; format
  plus `CA1062`, `CA1510`, and `CA2264` clean; Unit `716/716`; Integration
  `261/261`; managed published EndToEnd `57/57`; no skipped tests; a clean
  package-vulnerability report; and a clean `git diff --check`.
- Native AOT is Child 3 final-gate work. It is not evidence for the Child 2
  execution baseline and is not claimed by this Preflight.

### Authority Order

The [Find Interface](../../../../crystallized/documents/cli/contracts/find/interface.md)
owns exact grammar, output, and command-local schema. [Find Behavior](../../../../crystallized/documents/cli/contracts/find/behavior.md)
owns operation semantics. [CLI Architecture](../../../../crystallized/documents/cli/architecture.md)
and the [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md)
own the envelope, lifecycle, streams, exits, and AOT boundary. [Find Technical
Design](../../../../crystallized/documents/cli/contracts/find/technical-design.md)
is subordinate implementation guidance. The accepted Child 2 callables and
result are the predecessor boundary. This Working Task owns only Child 3's
phase, file, and evidence boundaries. Generated `Entries` and routing are
projections and remain unchanged.

### Preflight Architecture Finding And Bounded Correction

The current `CliRootBranch` is always classified as a group, and branch help wins
`TryAdd`. Reusing it for direct-root `Find` would give the command accidental
group and help ownership. Child 3 therefore authorizes one bounded neutral shared
composition correction, not a Find semantic change:

- Add `CliRootLeaf(Command Command, IReadOnlyList<CliDelimiterPolicy>
DelimiterPolicies)` under `Shell/Parsing`.
- Extend `CliCommandTree.Create` and `CliRootDefinitionFactory.Create` to attach
  explicit root leaves without adding them to `_groups`.
- Require every root leaf to have an exact binding. Binding-owned help remains
  authoritative.
- Register the exact Find command once as a root leaf with an empty delimiter
  policy list. Root composition creates symbols once, closes the binding once,
  adds that same `Command` as one `CliRootLeaf`, adds the binding once, and adds
  Find to root Discovery.
- Preserve all existing Route group, list, and inspect behavior.

This correction changes no parser normalization, terminal handling, workspace
selection, pipeline, status, stream, or exit semantics. It is a top-down shared
mechanical foundation in Shell, not Find semantics in Shell.

### Frozen Root-Leaf Contract

Define a root leaf once: a leaf `Command` attached directly to the root command,
not through a group. The shared composition signatures are frozen as follows:

- `CliCommandTree.Create(rootHelp, branches, bindings, additionalHelp = null,
rootLeaves = null)` preserves the existing first four parameter positions.
  `rootLeaves` is the optional explicit direct-root leaf collection.
- `CliRootDefinitionFactory.Create(branches, rootLeaves)` attaches existing
  branches in supplied order and then explicit root leaves in supplied order.
  Existing Route group, list, and inspect order is preserved.
- `CliCommandTree.Create` owns validation. Each root leaf is non-null, appears
  exactly once, cannot also be a branch, and has a binding whose `Command` is
  reference-equal to the leaf `Command`. Duplicate or reused root commands are
  rejected deterministically. `additionalHelp` may add root help but cannot
  override a root leaf's binding-owned help.
- Binding selection and application remain unchanged: dispatch still selects the
  exact binding by command identity and applies that binding once.

Red directly proves missing binding, duplicate and branch collision,
binding-owned help, direct-leaf selection, and unchanged Route group behavior.

### Frozen Find Callables And Files

- `Commands/Find/Models/Binding/FindBindingComponents.cs` contains required
  `CliHelpContent Help`, required `FindOperation Operation`, required
  `CliRendererSet<FindResult> Renderers`, and optional
  `CliDiagnosticRenderer<FindResult> DiagnosticRenderer`.
- `Commands/Find/FindBinding.cs` exposes `CreateSymbols() -> FindSymbols` and
  `Close(FindSymbols, FindBindingComponents) ->
CliCommandBinding<FindRequest, FindResult>`. `Close` uses one
  `FindResultBuilder`, shared by `FindRequestBinder` and
  `FindWorkspaceResultFactory`; `WorkspaceRequirement.Required`,
  `operation.ExecuteAsync`, one renderer set, and optional diagnostics are each
  wired once. There is no second operation path.
- Under `Commands/Find/Shared/Rendering`, the directly callable surfaces are
  `FindHumanRenderer.Render(CliPresentationRequest<FindResult>)`,
  `FindCompactRenderer.Render(FindResult)`,
  `FindExpandedRenderer.Render(FindResult)`,
  `FindJsonProjection.Create(FindResult)`,
  `FindJsonRenderer.Render(CliPresentationRequest<FindResult>)`,
  `FindDiagnosticRenderer.Render(CliPresentationRequest<FindResult>)`, and
  `FindHelpSections.Create()`. Private or internal local name, escaping, and
  projection helpers are allowed only as implementation support.
- New types with more than three cohesive constructor inputs use one nearest-scope
  named component or input record. New callables have at most four parameters and
  no compatibility overloads.
- Extend the existing `FindContentSelection` DTO with the exact constructor
  `FindContentSelection(IEnumerable<FindContentPart> supplied,
IEnumerable<FindContentPart> effective, bool isRequested = false)` and immutable
  `bool IsRequested` state. `IsRequested`
  is true when the option was explicit even if malformed input yielded no parsed
  parts, and it is also true whenever `Supplied` is nonempty. An omitted option has
  `IsRequested == false` and empty supplied/effective arrays. Green passes
  `FindQueryInput.ContentFacts.IsExplicit`; result invariants and pre-operation
  coverage use `IsRequested`, not parsed-part count. This is the bounded correction
  that preserves the Interface's presence distinction for malformed `--content`.

### Concrete JSON Graph And Composition

The concrete JSON graph lives under `Commands/Find/Models/Presentation/`, split
coherently across document, selection, findings, and matches files. It freezes
the Interface's exact camel-case names and member order:

- The envelope is `schemaVersion`, `command`, `status`, `workspace`, `result`,
  `next`. `schemaVersion` is `1`; `command` is `find`; `workspace` is either the
  `{ path, selectedBy }` object or `null`; `result` is non-null; and `next` is
  either the `{ command, reason }` object or `null`.
- `result` is `universe`, `query`, `presentation`, `coverage`, `findings`,
  `matches`.
- `universe` is `mode`, `include`, `exclude`, `candidateCount`,
  `inspectedCount`, `matchedCount`.
- A selector is `value`, `form`, `resolution`, `identity`, `sourceKind`,
  `expansion`, `candidates`; an identity, source, or candidate is `id`, `path`.
- `query` is `predicates`, `effectivePredicates`, `require`, `within`; a
  predicate is `kind`, `value`; `within` is `supplied`, `tag`, `heading`.
- `presentation` is `view`, `content`; `view` is `supplied`, `effective`, and
  `content` is `supplied`, `effective`.
- `coverage` is `state`, `matching`, `projection`.
- A finding is `code`, `status`, `subject`, `cause`, `selectorRole`,
  `selectorOccurrence`, `source`, `layer`, `path`, `region`, `location`,
  `candidates`.
- A match is `position`, `id`, `path`, `description`, `evidence`, `projections`.
  Evidence is `predicate`, `kind`, `query`, `authored`, `region`, `layer`,
  `path`, `location`, `occurrence`, `heading`; `heading` is `level`, `form`,
  `canonical`.
- A projection is `part`, `name`, `layer`, `path`, `state`, `metadata`, `text`,
  `headings`, `location`. Metadata is `position`, `id`, `path`, `routeState`,
  `route`, `layers`; a metadata layer is `kind`, `path`; a projected heading is
  `text`, `level`, `form`, `location`, `canonical`.

All arrays are concrete and always present, including empty arrays. Nullable
members are explicit rather than omitted. Counts are nonnegative integers when
established, `0` when known empty, and `null` when not established. Projection
payloads use the Interface's exact null and empty-array rules. JSON names are
explicit finite strings, not enum or reflection fallbacks. Add only the
`FindJsonDocument` registration to `CliJsonContext`; do not serialize a result
interface, the shared interface, or any reflection-discovered graph.

The exact finite mappings are frozen from the Interface: statuses are `complete`,
`attention`, `incomplete`, `invalid`, `blocked`, `failed`, and `interrupted`;
coverage states are `not-started`, `complete`, `incomplete`, `blocked`, `failed`,
and `interrupted`, with projection additionally allowing `not-requested`; view
values are `compact` and `expanded` with supplied view also allowing `null`;
universe modes are `default` and `filtered`; `require` is `all` or `any`;
selector forms are `id` and `path`; selector resolutions are `resolved`,
`invalid`, `unknown`, `unsupported`, `ambiguous`, and `unsafe`; selector source
kinds are `loader`, `entrypoint`, `skill`, and `ordinary`; selector expansions
are `folder` and `source`; predicate kinds are `tag` and `heading`; layers are
`base` and `overwrite`; heading forms are `atx` and `setext`; projection states
are `available`, `missing`, `unavailable`, and `ambiguous`; route states are
`routed` and `unrouted`. Nullable forms, kinds, expansions, identities, locations,
and routes remain explicit `null` where the Interface requires them.

`selectorRole` is `include` or `exclude`; `selectedBy` is `current-directory` or
`explicit-workspace`; and `routeState` is `routed` or `unrouted`. The JSON
`projection.part` finite values are `metadata`, `frontmatter`, `headings`, `body`,
and `section`; `projection.name` carries the requested section name and is
otherwise `null`. Canonical regions are `document`, `frontmatter`, `body`, and
`section:<name>`. Only canonical requested content parts use
`section:<name>`; their JSON projection uses `part: "section"` with that name.
Locations use `line`, `column`, `byteOffset`, and `byteLength`.
The exact finding codes are `find.invalid-input`, `find.invalid-selector`,
`find.workspace-unavailable`, `find.workspace-unsafe`,
`find.selector-ambiguous`, `find.selector-unsafe`, `find.identity-collision`,
`find.candidate-unsafe`, `find.layer-unresolved`,
`find.inspection-unavailable`, `find.invalid-encoding`,
`find.frontmatter-unavailable`, `find.section-ambiguous`,
`find.projection-missing`, `find.projection-unavailable`,
`find.operation-failed`, and `find.interrupted`, in the Interface's fixed finding
order. Evidence, metadata, projected headings, locations, `next`, and all
null/empty/count behavior use the Interface definitions without local aliases.

`next` is `null` for `complete` and `attention`. For `incomplete` it is
`{ command: "open-forge doctor", reason: "Inspect the unavailable source or
projection facts before relying on this Find result." }`; for `invalid` it is
`{ command: "open-forge find --help", reason: "Correct the named Find input,
then rerun the request." }`; for selector-ambiguity-only `blocked` it is
`{ command: "open-forge find", reason: "Replace every ambiguous selector with
one listed exact path, then rerun the same request." }`; for other `blocked` it
is `{ command: "open-forge doctor", reason: "Inspect the blocked workspace or
source boundary before rerunning Find." }`; for `failed` it is
`{ command: "open-forge find --verbose", reason: "Report the failure and retry
the same request with bounded diagnostics." }`; and for `interrupted` it is
`{ command: "open-forge find", reason: "Rerun the same Find request." }`. The
compact line uses the Interface's exact corresponding wording and never adds a
second public `Next:` line.

### Status, Terminal, And Workspace Boundaries

- Parser-native syntax, delimiter, and global-semantic failures remain bounded
  Shell parser diagnostics on stderr. They do not invent a Find envelope because
  no typed Find request or result exists at that boundary.
- Typed Find invalid binding results use the Find human and JSON presentations.
  Workspace-selection failure uses the existing typed Find blocked result; its
  JSON envelope has `workspace: null`.
- Help and version remain text-only terminal modes that bypass workspace selection
  and operation execution. A well-formed `--view` under JSON is a no-op; JSON
  still emits the one complete concrete document.
- Shared human primary streams and exits remain: `complete` `0` stdout,
  `attention` `2` stdout, `incomplete` `3` stdout, `invalid` `4` stderr,
  `blocked` `5` stderr, `failed` `1` stderr, and `interrupted` `130` stderr. JSON
  writes one complete document to stdout for every semantic status. Bounded
  diagnostics remain on stderr and never alter result, primary output, status, or
  exit.

### Frozen Human Rendering

- Compact begins with one tab-separated summary in exact contract order:
  `result`, `coverage`, optional `projection` immediately after `coverage` whenever
  `result.Presentation.Content.IsRequested`, `universe`, and `matches`.
  Invalid malformed `--content` has empty parsed supplied/effective arrays,
  projection coverage `not-started`, and an emitted `projection=not-started` field.
  Projection is `not-requested` and the field is absent only when `--content` was
  omitted. It then emits ID-and-path rows. Only `result=complete` with zero matches
  adds `No matches.`.
  It emits ordered concise finding lines and the exact one public `Next:` line only
  when the top-level next action exists; `attention` has no `Next`. Compact omits
  workspace, query, description, and evidence.
- When `--content` is omitted, no projection blocks are emitted. When it is
  present, deterministic projection entries are grouped under each matched
  source in typed match/projection order: metadata has one logical entry;
  frontmatter, headings, and body have one entry per physical layer; and each
  requested section has one entry per layer. Requested projection text is
  emitted from the exact typed selected text and is not reread.
- Expanded emits workspace and selected-by; every filter and predicate in
  supplied order; `require`; supplied and effective `within`; universe mode and
  supplied include/exclude selectors with their resolution and candidates; counts;
  matching and projection coverage; ordered findings; and matches with ID, path,
  description, and all ordered evidence. After the complete match explanation and
  before selected projection blocks, it emits the same exact public human `Next:`
  line as compact when the top-level next action exists: incomplete and ordinary
  blocked use `Next: open-forge doctor`; invalid uses
  `Next: correct the named Find input.`; selector-ambiguity-only blocked uses
  `Next: rerun with one listed exact path for each ambiguous selector.`; failed uses
  `Next: report the failure and retry with bounded diagnostics.`; and interrupted
  uses `Next: rerun the same request.` Complete and attention emit no line.
- Each human projection block always shows its canonical part, `layer`/`path` or
  `none`, and state. `section:<name>` appears only as the human canonical
  requested label; the JSON discriminator remains `part: "section"` with
  `name`. Metadata lists exact logical metadata and layers. Headings list exact
  heading and location facts. Text projections show location, then a labeled
  raw-text block whose interior is the exact typed text and whose delimiters are
  renderer-owned. Missing, unavailable, and ambiguous blocks show their
  discriminators and no payload. No projection block rereads source content.
- One-line values are escaped without truncation so supplied predicates, selector
  values, finding facts, and matched query/authored evidence retain their complete
  spelling. Diagnostics alone are bounded. Neither renderer computes domain facts;
  both consume the one typed `FindResult`.

### Find Escaping Boundary

`FindTextEscaping` is command-local. Its ordinary one-line escape preserves the
complete value without truncation, including over-limit authored heading and tag
evidence. It preserves valid surrogate pairs; escapes a backslash as `\\`, a quote
as `\"`, and a control character or lone surrogate as lowercase `\\u` followed by
four hex digits. Diagnostic values alone have a `240`-character maximum including
`...`; their positive custom limit is validated, truncation never splits a valid
surrogate pair, and a limit too short for an ellipsis emits only dots up to that
limit. Explicit requested projection text remains exact and is not passed through
one-line escaping. JSON maps typed strings, including finding `subject` and
`cause`, unchanged and relies on the source-generated serializer's escaping; it
does not impose a second presentation bound.

### Diagnostics And Help

The command-local diagnostic summary is bounded to status and workspace, universe
counts, coverage, predicate/match/finding/projection counts, and next presence. It
is escaped and clamped, never includes authored bodies, frontmatter, or section
text, and remains within Shell's `4096` total diagnostic bound on stderr.

Binding-owned help sections are, in order: Syntax; Source references; Predicates
and regions; Content and views; Inherited global options; Results and streams
including all seven exits; Examples; Related commands; Notes. Standard grammar
and global help remain derived from the exact composed command tree.

### Strict Phase Boundaries

- **Preflight:** Keep this phase documentation-only; accept it in the commit
  containing this record.
- **Gray:** Add production callables and DTO contracts, including the neutral
  `CliRootLeaf` signature and shape; add the permitted concrete `FindJsonDocument`
  source-generation registration; allow named `NotSupportedException` behavior
  entrypoints. The mechanical registration may already pass its direct evidence.
  Do not register the root or mutate tests.
- **Red:** Add tests only; run the declared evidence; allow only the named Gray
  behavior or absent Green root-registration failures. DTO, invariant, and
  source-generation tests may already pass. Managed published EndToEnd declarations
  may exist and fail because Find is not registered. Do not publish Native AOT or
  change production.
- **Green:** Change production and root composition only; satisfy Red minimally.
- **Blue:** Change production structure only; do not change behavior, tests, or
  expectations.
- **Purple:** Change test/support structure only; do not change production or
  expectations.
- **Final:** Run complete gates, reviews, and record updates; stop on any contract
  or phase contradiction.

### Complete Red Evidence Matrix

The accepted Preflight froze the following files and durable scenario-group
declarations without inventing an exact case count. They did not exist at the
Preflight boundary, which claimed no Red pass, failure, or skip count. The complete
packet now exists, and its accepted counts are recorded below.

#### Unit declarations

- `tests/unit/OpenForge.Cli.Core.UnitTests/Shell/DirectRootLeafRedTests.cs` —
  root-leaf definition and classification;
  missing binding; duplicate and branch collision; binding-owned help;
  direct-leaf selection; and unchanged Route group behavior.
- `tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/FindPresentationBindingRedTests.cs`
  — exact symbol and binding identity; required binding; operation exactly once;
  typed invalid input including explicit malformed content with no parsed part;
  workspace-selection failure; and help/version terminal bypass.
- `tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Rendering/FindHumanRenderingRedTests.cs`
  — compact and
  expanded output for all seven statuses; zero and safe rows; ordered findings
  and top-level next actions; default and filtered query/universe; evidence;
  every content kind and projection state/layer; omitted versus explicit malformed
  content presence and `not-requested` versus `not-started`; exact expanded
  status-specific next-line text, omission, and placement; human escaping; and
  typed projection-block order and payload boundaries. It includes an authored
  heading and tag longer than `512` characters and proves their complete spelling
  remains present after escaping.
- `tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Rendering/FindJsonRenderingRedTests.cs`
  — exact envelope,
  result, and nested member order; every finite mapping; every null, array, and
  count rule; every status; malformed-content request echo and `not-started`
  projection coverage; projection `part`/`name` shape; source locations; and the
  JSON view no-op.
- `tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Rendering/FindDiagnosticsAndHelpRedTests.cs`
  — bounded
  diagnostic fields and limits with no authored-content leak; help grammar,
  ownership, sections, all seven exits, examples, related commands, and notes.
- Test-only `tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Presentation/FindPresentationTestData.cs`
  — one rich typed-result fixture family covering all statuses, zero and safe rows,
  findings and next actions, default and filtered query/universe, evidence,
  content kinds, projection states and layers, escaping, and projection blocks.

#### Integration declarations

- `tests/integration/OpenForge.Cli.IntegrationTests/Commands/Find/FindApplicationIntegrationRedTests.cs`
  — composed `CliHost` root/Find help and exact registration; real
  temporary-workspace bare/default
  and filtered tag/heading scenarios; compact, expanded, JSON, and content
  views; typed invalid input; blocked workspace with `workspace:null`; deterministic
  attention for missing projection and collision; incomplete unreadable and
  invalid-encoding cases; verbose separation; no writes; and parser-native
  failures remaining Shell diagnostics. Failed and interrupted cases use direct
  typed results unless a safe deterministic public trigger already exists; no
  hidden test flag is allowed.
- `tests/integration/OpenForge.Cli.IntegrationTests/Serialization/FindGeneratedSerializationRedTests.cs`
  — concrete Find document source-generation registration, reflection-disabled
  serialization, and exact graph order and null/array/count rules. Its direct
  mechanical registration evidence may pass in Red; result projection and public
  rendering remain named Gray gaps.

#### EndToEnd declarations

- `tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessRedTests.cs`
  — managed published public help, bare expanded, compact filtering, JSON/view
  no-op/content, typed invalid, blocked,
  deterministic attention/incomplete, verbose streams and exits, and
  representative no-write snapshots. These managed declarations may fail only
  because the Find command is not registered in Red.
- Project-local `tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindWorkspace.cs`
  — the owned published-workspace fixture only if the process declarations require
  it; it does not become shared TestSupport.

TestSupport remains unchanged unless a later phase proves identical
cross-project fixture meaning. Red compiles and runs these declarations and
records failures only at explicitly named unimplemented Gray behavior or absent
Green root registration. Native AOT is not published or executed in Red; it is
final-gate evidence only.

### Red Commands

All focused Red commands run from `src/cli/`:

```bash
dotnet restore OpenForge.Cli.slnx --locked-mode
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Find|FullyQualifiedName~Shell.DirectRootLeafRedTests"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Find|FullyQualifiedName~Serialization.FindGeneratedSerializationRedTests"
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~PublishedFindProcessRedTests"
```

The original Red packet is accepted at exact `22d3bff` and compiles and runs from
exact Gray `a76a217`. Focused Unit
is `202` total with `92` passing and `110` intentional failures. Focused
Integration is `43` total with `23` passing and `20` intentional failures; the
concrete generated-serialization declarations pass `2/2`. Managed published
EndToEnd is `13` total with `13` intentional failures at absent root registration.
Every project reports zero skips. The non-AOT managed `win-x64` publish passes.
No Native AOT command belongs to this phase.

During Green integration, two composed metadata scenarios were shown to conflict
with accepted Child 2 effective-universe and unique-route semantics rather than
expose a presentation defect. With metadata requested, filtered `--include=docs` leaves
Loader route support outside the effective universe, while the collision case
cannot establish a unique route; each makes metadata unavailable and the result
`incomplete`. The maintainer authorized a return to
Red that preserves Child 2: the complete metadata scenario keeps route support in
the universe and filters matches by tag, uses a valid empty Loader `Entries`
section, expects truthful unrouted metadata, and proves JSON `--view` changes only
its echoed view facts; the collision scenario requests body content so it
isolates the intended `attention` finding. The initial correction changes only the
named Integration evidence file, preserves its `43` cases and all public
contracts, and is accepted at exact `6a9a0de`. Managed Green published evidence
then exposed the identical stale assumptions in the mirrored EndToEnd scenarios.
The supplemental correction applies the same fixture and scenario meaning only to
`PublishedFindProcessRedTests` and its project-local `PublishedFindWorkspace`,
preserves all `13` cases, and is accepted at exact `eea3d59` (`Complete Find
presentation metadata evidence`). Post-commit Red reproduction succeeded as
intentional Red: managed non-AOT `win-x64` publish passed, and published Find
EndToEnd was `13` total with `13` intentional failures and zero skips, all
terminating at absent Green root registration.

Fresh Green review then found two exact escaping defects. The independent human
test expected `\t` although the frozen contract requires every control as
lowercase `\uXXXX`, and bounded diagnostics escaped before slicing code units,
which allowed partial `\\`, `\"`, or `\uXXXX` tokens. This returned narrowly to
Red without changing contracts, Child 2, Integration, EndToEnd, production,
package/project, generated routing, or Route behavior. Supplemental escaping Red
correction is accepted at exact `a865fd1` (`Correct Find escaping evidence`). It
changes only `FindHumanRenderingRedTests.cs` and
`FindDiagnosticsAndHelpRedTests.cs`: the TAB expectation is `\u0009`, and four
Windows renderer-level cases place backslash, quote, TAB, and U+0001 at the
diagnostic truncation boundary and independently validate complete escape-token
grammar, bounds, one-line output, and no payload leak. Locked restore, the
warning-free Release solution build, and format verification pass. Full focused
Unit is `206` total with `92` pass and `114` intentional Gray-boundary failures,
zero skips. The narrow selected evidence from exact `a865fd1` is `6` total with
`6` intentional Gray failures at the named expanded/diagnostic stubs, zero skips.
Fresh test-only correctness review is `PASS`, with no material optional
improvement.

### Final Full-Gate Commands

After the last production, test, fixture, composition, or configuration change,
run this complete gate from `src/cli/`:

```bash
dotnet restore OpenForge.Cli.slnx --locked-mode
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet format OpenForge.Cli.slnx analyzers --no-restore --verify-no-changes --severity info --diagnostics CA1062 CA1510 CA2264 --verbosity diagnostic
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/open-forge
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/integration
"./artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe" --progress off
dotnet publish tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/end-to-end
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" "./artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe" --progress off
dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore
```

From the repository root, also run `git diff --check`, review `git diff
--name-only` against the exact allowed paths, and complete the protected-surface,
generated-region, project-graph, package, artifact, public no-write, and Route
regression audits. Native evidence is local `win-x64` evidence only; no six-RID
parity is inferred.

### Exact File Boundary

The exact executable/test boundary is separate from the active Working records.
Allowed production paths are limited to new files under
`Commands/Find/Models/Binding/`, `Commands/Find/Models/Presentation/`, and
`Commands/Find/Shared/Rendering/`; new `Commands/Find/FindBinding.cs`; the existing
`Commands/Find/Models/Presentation/FindPresentationModels.cs`,
`Commands/Find/FindRequestBinder.cs`,
`Commands/Find/Models/Result/FindResult.Validation.cs`, and
`Commands/Find/Shared/Result/FindResultBuilder.cs` only for the frozen explicit
content-presence correction; new `Shell/Parsing/CliRootLeaf.cs`; existing
`Shell/Parsing/CliCommandTree.cs` and `Shell/Parsing/CliRootDefinitionFactory.cs`;
one attribute/import in `Shell/Serialization/CliJsonContext.cs`; and root
`Composition/CliCompositionRoot.cs`. Allowed test paths are the exact Red files
declared above, followed only by their phase-preserving rename or split in Purple.
TestSupport is allowed only when identical cross-project fixture meaning is proved.

The documentation-only Preflight paths are exactly the Checkpoint, Plan,
`tasks/00-cli-development.md`, `tasks/read-only/_read-only.md`, this child,
`find-query-operation.md`, `find-source-catalogue.md`, and `find.md`. Final may add
one new maintainer-requested Handoff. No package, project, contract,
generated-routing, or Route behavior change is allowed.

### Final-Gate Evidence Boundary

The final gate repeats the managed build and complete Unit, Integration, and
published EndToEnd runs after the last executable change, then adds the local
`win-x64` Native AOT root, native Integration and EndToEnd, integrated Markdig
execution, no-write public scenarios, audits, Route regressions, and reviews.
Native evidence is local `win-x64` evidence only; no six-RID parity is inferred.

## Stop Conditions

- Stop if registration requires any Shell or parser policy beyond the bounded
  neutral root-leaf correction, a second operation invocation, a second renderer
  catalogue, or a public-contract change.
- Stop if source-generated JSON cannot preserve the exact concrete graph under
  trimming/AOT, if human and JSON views diverge in meaning, or if diagnostics
  alter primary output, status, or exit.
- Stop if any public scenario writes workspace bytes, invents a test-only failure
  flag, changes Route behavior, or requires a package, project, or unrelated
  command change.

## Progress And Completion

### Final Acceptance Result

- Child 3 and the parent Find Task are Complete and accepted in the commit
  containing this record update. That containing commit is the exact accepted
  `feature/cli-find` tip; no future feature-tip hash is claimed here.
- The final gate ran from exact clean source `426d4f5`, after the last executable,
  test, or configuration change. Locked restore passed. The Release build was
  warning-free with 0 warnings and errors. Normal format verification and
  informational `CA1062`/`CA1510`/`CA2264` verification passed. Full managed Unit
  passed `835/835`; Integration passed `284/284`; managed publish and published
  EndToEnd passed `70/70`; the local `win-x64` Native AOT root drove managed
  EndToEnd `70/70`; the Native AOT Integration executable passed `284/284`; and
  the Native AOT EndToEnd executable against the native root passed `70/70`.
  Every test run had zero skips.
- The package vulnerability JSON report found none. The project graph retained the
  six expected projects. The managed root, native root, native Integration, and
  native EndToEnd artifacts were present PE32+ x64 files with these SHA-256 hashes:

  | Artifact           | SHA-256                                                            |
  | ------------------ | ------------------------------------------------------------------ |
  | Managed root       | `c28cb2be58f7a955a6f964c887aad5ef03265e43d65278ffa5f99afc1cceca7e` |
  | Native root        | `2c70fa07b3c7695a5b1e495b0ea03fc2e8cb07f47a51c066d43a904e3b84710a` |
  | Native Integration | `ad910ee7541594b17540d5dd74ca05e81b0ed6e3235ea0e36b18955527630bb0` |
  | Native EndToEnd    | `e44d424ea00f2c787a91063e1b03633bd07998d3924dc9a48256365f076320da` |

- `git diff --check`, source cleanliness at `426d4f5`, and the exact
  changed-path, protected-surface, project, package, configuration, and
  generated-routing audits passed. The diff `28d316a..426d4f5` contains only the
  eight Working records and the exact authorized Child 3 production/tests. No
  Route production diff exists. The static executable-path write audit is empty.
  Concrete Find JSON uses `CliJsonContext.Default.FindJsonDocument`; no reflection
  or alternate JSON graph is present.
- Every Find published-process invocation uses `RunWithoutWritesAsync` with
  recursive entry, metadata, and hash snapshots. The full managed and
  native-root EndToEnd runs execute the representative JSON and compact no-write
  journeys, establishing public no-write evidence.
- Fresh final correctness review: `PASS`, with no material findings. Its optional
  direct compact missing-section-name assertion was rejected as evidence
  strengthening only because this phase is frozen and Purple is a no-op; the
  implementation already emits the typed block.
- Fresh final improvement review: `APPROVED — NO_MATERIAL_IMPROVEMENTS`. The full
  escaped-value transient allocation before bounded diagnostic truncation is
  theoretical and nonmaterial because the overload handles one diagnostic
  workspace value under parser, path, and platform bounds. Streaming would add
  edge-case complexity. Revisit only if this overload is used for unbounded content
  or hot collections.
- The dedicated challenger helper could not run because of a model-routing error.
  A fresh grounded adversarial Advisor substitute inspected the candidate and
  recommended `ACCEPT` with no blocker. It resolved the strongest counterargument:
  `FindOperation` uses effective content count while result paths use `IsRequested`,
  but malformed explicit public content is rejected in binding and recovered as
  requested/not-started before the operation; valid requests make the values
  equivalent. Revisit only if future direct callers may submit synthetic
  inconsistent internal requests.
- Residual limitations are not blockers: native evidence is local `win-x64` and
  does not infer six-RID parity; the unreadable-file fixture uses Windows
  `FileShare.None` and future multi-RID evidence needs portable or
  platform-qualified handling; failed and interrupted scenarios use direct typed
  results because no safe deterministic public trigger exists; malformed-content
  plus unavailable-workspace cross-product coverage is static/focused rather than
  broad process evidence; and `CLI-EDGE-001` remains a non-product generated-
  routing refresh blocker.
- The accepted product boundary remains one public direct-root `find` with the
  exact grammar, help, human compact/expanded, JSON, diagnostic, status, stream,
  exit, and `next` behavior; source-generated JSON; no writes; protected Child 2
  semantics; and unchanged Route behavior. The [Find Interface](../../../../crystallized/documents/cli/contracts/find/interface.md),
  [Find Behavior](../../../../crystallized/documents/cli/contracts/find/behavior.md),
  and [Find Technical Design](../../../../crystallized/documents/cli/contracts/find/technical-design.md)
  remain the contract authority.
- The sole immediate continuation after the containing acceptance commit is for
  the Mastermind to freshly verify `develop`, squash-integrate the accepted
  `feature/cli-find` tip into local `develop`, commit that one squash, prove exact
  tree equality, do not push, and halt. References remains Planned and must not
  start in this session.

- Phase history: Child 2 is accepted at exact `ff7ce3f`, and Child 3 focused
  Preflight at exact `28d316a`. Gray production contracts, named stubs, neutral
  root-leaf shape, concrete JSON graph, source-generation registration, and
  explicit content-presence DTO state are accepted at exact `a76a217`. The
  original ten-file Red packet is accepted at exact `22d3bff`; its Integration
  metadata correction is accepted at exact `6a9a0de`, and the mirrored EndToEnd
  metadata correction is accepted at exact `eea3d59` (`Complete Find presentation
metadata evidence`). Its post-commit Red reproduction succeeded as intentional
  Red: managed non-AOT `win-x64` publish passed, and published Find EndToEnd was
  `13` total with `13` intentional failures and zero skips, all terminating at
  absent Green root registration. Historical pre-correction Green review found the
  human `\t` versus
  lowercase `\uXXXX` contract mismatch and diagnostic escaped-code-unit slicing,
  so the work returned narrowly to Red without changing contracts, Child 2,
  Integration, EndToEnd, production, package/project, generated routing, or Route
  behavior. Supplemental escaping Red correction is accepted at exact `a865fd1`;
  the current corrected Red boundary has Unit `206` total with `92` pass and
  `114` intentional Gray-boundary failures, Integration `43` total with `23`
  pass and `20` intentional failures, and published EndToEnd `13` total with
  `13` intentional failures, all with zero skips. The narrow selected evidence
  from exact `a865fd1` is `6` total with `6` intentional Gray failures at the
  named expanded/diagnostic stubs, zero skips; its test-only correctness review is
  `PASS` with no material optional improvement. Child 3 Green is accepted at
  exact commit `cb7874c` (`Implement Find presentation`) over corrected Red
  `a865fd1`. The exact Green
  scope is 15 production paths: Find binding, request, result builder, and
  validation; compact, expanded, JSON, diagnostic, help, and shared text
  escaping; Shell direct-root command tree and root factory; and root
  composition. It keeps one symbol graph and operation/result flow,
  binding-owned help, source-generated concrete `FindJsonDocument`, explicit
  malformed-content request presence, deterministic bounded human/JSON/diagnostic
  behavior, and exact one root Find registration. It adds no reflection, second
  parser/operation/renderer catalogue, workspace writes, or Native AOT work. The
  correction encodes TAB as `\u0009` and truncates only at complete escaped-token
  boundaries.

  Fresh Green evidence passes locked restore, a warning-free Release solution
  build, format verification, and `git diff --check`; focused Find+direct-root
  Unit `206/206`; focused Find+generated-serialization Integration `43/43`;
  affected Shell+Route Unit `347/347`; affected Shell+Route Integration
  `160/160`; managed non-AOT `win-x64` publish; and published Find EndToEnd
  `13/13`. Every run has zero skips. Fresh final bounded Green correctness
  review is `PASS` with no material findings. It verifies corrected escaping,
  Find binding, explicit malformed-content state, root leaf registration,
  renderer dispatch, JSON projection/source generation, diagnostics, help,
  direct Shell integration, one operation/result flow, and no protected-surface
  drift. Native AOT is intentionally not claimed.

  Earlier local-improvement review found one material bounded Blue candidate only:
  in `FindJsonProjection`, replace duplicate finite `Status` and `FindingCode`
  switches with canonical `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`. Its separate escaping correctness
  finding was resolved in Green; it is not a Green defect. Blue applied that
  candidate and is accepted at exact `3f81e76` (`Simplify Find JSON projection`). The one-file
  production-only change replaces the duplicate local switches with the canonical
  readers and removes the two duplicate private mapping methods. It preserves
  behavior, public output/order/schema, tests/support, contracts, package/project/
  configuration, generated routing, Shell/root, Route, workspace-write, and
  Native AOT surfaces. Blue evidence and both bounded reviews pass; no managed
  republish or Native AOT claim is needed. The no-op Purple acceptance is recorded
  at exact `426d4f5` with verdict `NO_MATERIAL_IMPROVEMENTS`.
  Focused Unit `206/206`, focused Integration/serialization `43/43`, and published
  Find EndToEnd `13/13` pass with zero skips, and source diff/check is clean/empty.
  No test/support, production, contract, project, package, configuration,
  generated, Route, Shell, or root file changed, and no Purple code/test commit is
  manufactured. No Native AOT claim was made. Final acceptance is recorded in the
  commit containing this record update.

- Original Red evidence retained: Locked restore, the warning-free Release solution build, format,
  and diff checks pass. Focused Unit is `202` total with `92` passing and `110`
  intentional failures. Focused Integration is `43` total with `23` passing and
  `20` intentional failures, including passing concrete generated serialization
  `2/2`. Managed non-AOT `win-x64` publish passes. Published Find EndToEnd is `13`
  total with `13` intentional failures. Every run has zero skips. Unit and
  Integration failures terminate only at accepted named Gray behavior boundaries;
  published failures terminate only at absent Green root registration.
- Original Red review result: Fresh targeted correctness review is `PASS` after proving the
  exact repeated human projection signature order, direct composed root topology,
  complete concrete JSON wire graph, binding-owned help, selector-ambiguity next
  actions, and recursive no-write state. A final staged follow-up initially
  questioned the projection-finding fixtures. Integrated inspection established
  that the accepted Child 2 `FindResultBuilder` creates each projection finding
  exactly once; adding manual fixture findings would duplicate that derivation and
  was rejected. The follow-up passed after preserving builder-owned derivation,
  correcting one indentation defect, and rerunning the complete Red commands.
  Fresh local-improvement review is `NO_MATERIAL_IMPROVEMENTS/APPROVED`; further
  restructuring would add phase cost without improving the frozen evidence. The
  current unreadable-file fixture uses Windows `FileShare.None`; a later multi-RID
  requirement may need a non-Windows mechanism or a platform-qualified scenario.
  A failure outside the named Gray or absent-registration boundaries, a production
  drift, or incomplete no-write/JSON proof would invalidate the original Red
  acceptance.
- Blocker: None. Child 3 and the Find parent are Complete and accepted in the
  commit containing this record update. The final managed/native, package,
  artifact, static, public no-write, and protected-surface audits passed, and
  `CLI-EDGE-001` remains non-product only.
- Next action: After the containing acceptance commit, freshly verify `develop`,
  squash-integrate the accepted `feature/cli-find` tip into local `develop`, commit
  that one squash, prove exact tree equality, do not push, and halt. Do not start
  References in this session.

This child is Complete and accepted in the commit containing this record update.
The public Find grammar, help, compact/expanded/JSON/diagnostic projections,
streams, exits, statuses, no-write behavior, managed/published/native evidence,
audits, reviews, and Route regressions pass. All eight live records named in the
Exact File Boundary are synchronized, and the new [CLI Find Accepted
Handoff](../../../handoffs/2026-08-25_cli-find-accepted.md) is sealed. Do not edit
the sealed Handoff or the generated routing projection. The replacement remains
non-shipping; after the local integration and equality proof, do not push and halt.
