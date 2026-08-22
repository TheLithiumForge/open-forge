---
open-forge:
  description: Freeze route-inspect callable contracts and map every guarantee to evidence before behavior
  tags: [Memory, Working, CLI, Task, Route, Inspect, Contract, Evidence, Contextual, Complete]
---

# Freeze Route-Inspect Contracts And Evidence

## Task State

- State: Complete.
- Responsible/implementer: Mastermind. Bounded phase delegation is permitted only after this packet is closed. Mastermind owns integration, staging, commits, and acceptance.
- Parent: [Implement Route Inspect And Promote Shared Route Facts](route-inspect.md).
- Task source: This file.
- Baseline: Exact `edca509` (`Establish and accept route list`) on `feature/cli-route-inspect`; this is the exact `develop` baseline.
- Decomposition boundary: Parent decomposition and read-only Phase 0 (Preflight) are complete. Gray is accepted; no test or domain behavior entered the phase.
- Model-locality decision: The maintainer requires every new record, interface, and property-only class, plus every existing model materially changed or promoted by this slice, under its nearest `Models/` path. Untouched accepted CLI types are outside this migration.

## Expected Outcome

Every `route inspect` Interface and Behavior heading maps to its production owner and required Unit, Integration, EndToEnd, and Native AOT evidence. Candidate and historical evidence is inventoried with an explicit disposition requirement. Before behavior begins, the command-local definitions, binding, request, operation call surface, typed result/profile/availability models, help ownership, and JSON projection ownership are frozen.

There is no preserved route-inspect production or test tree at this baseline. Commit `4b873de` and [`src/cli/tests/preserved/route-list-v1/`](../../../../../../src/cli/tests/preserved/route-list-v1/) contain route-list evidence only. Historical material is candidate evidence, never Architecture authority.

## Authority And Backlinks

| Source | Question it answers | Use in this Task |
| --- | --- | --- |
| [Route Inspect Interface Contract](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) | What callers enter and observe | Accepted public authority; do not change |
| [Route Inspect Behavior Contract](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md) | How one deterministic profile is formed and proved | Accepted behavior authority; do not change |
| [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) | Which source, dependency, evidence, serialization, and AOT boundaries apply | Binding implementation authority |
| [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md) | Which cross-command call, status, stream, and locality rules apply | Binding shared conventions |
| [Program Architecture Directive](../../../../../directives/program-architecture.md) | Who closes architecture and when delegation is allowed | Mastermind ownership and readiness |
| [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md) | Which replacement-CLI paths and test tiers are allowed | Source and evidence boundaries |
| [Source Locality Directive](../../../../../directives/source-locality.md) and [Nearest Shared Scope Pattern](../../../../../patterns/software/source-locality/nearest-shared-scope.md) | When support remains local or may move | No speculative promotion |
| [Development phase workflow](../../../../../workflows/development/_development.md) and [Task Lifecycle](../../../../../workflows/development/task-lifecycle.md) | How Phase 0, Gray, evidence, and commits are separated | Phase-boundary record |
| [Replacement CLI Edge-Case Ledger](../../edge-cases.md) | Which deferred edge cases must enter the packet | Required inputs, not automatic blockers |

## Analysis And Accepted Plan

1. Treat the two Crystallized route-inspect contracts as the meaning authority. Treat the Architecture as the implementation-boundary authority. Do not turn this Task into a Technical Design or add public behavior.
2. Inventory the known candidate and historical material below. For every relevant case, map it to an exact contract heading and record an explicit disposition before behavior. No candidate is preserved as route-inspect evidence merely because it has a similar name or passed route-list tests.
3. Build the complete matrix below. Its Unit, Integration, EndToEnd, and Native AOT cells describe required future proof; they do not claim that route-inspect evidence exists today.
4. Freeze one command root: definitions own symbols and local finite identities; binding owns parser-to-request binding; request owns complete command-local input; one operation call surface owns one invocation; typed result/profile/availability models own route-inspect facts; the Inspect binding owns product help sections; and an Inspect-local concrete JSON projection is registered through the existing Shell serialization boundary. Place records, interfaces, and property-only classes under the nearest `Models/` path, grouping larger sets by topic. The domain result is not the wire DTO.
5. The only permitted production change is an explicit compilable Gray skeleton under `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/`. It freezes callable interfaces and immutable models without an executable placeholder result. No test, domain behavior, filesystem traversal, graph construction, rendering, composition, promotion, or test-only failure hook is permitted in Gray.

### Phase Boundaries

- Phase 0 (Preflight) completes the candidate disposition and heading-level evidence plan without workspace mutation.
- Gray freezes only the command-local callable production surface and records this Task's progress in the same coherent commit. The solution must compile, but Gray contains no tests or domain behavior.
- This child has no Red or Green mutation. The resolution/promotion child begins from the accepted Gray commit and authors its failing resolution evidence before implementing behavior.
- Any later Blue or Purple mutation receives its own inspected commit under the Task Lifecycle; no phase is collapsed into Gray for convenience.

### Candidate And Historical Evidence Inventory

| ID | Candidate or historical source | Known scope at the baseline | Required disposition |
| --- | --- | --- | --- |
| C1 | Git commit `4b873de`, summarized by the [implementation reset](../../../../archived/cli-release/implementation-reset-2026-08-21.md) | Removed route-list WIP and its historical implementation ideas | Inspect each relevant claim against the current contracts, then mark it candidate-only, retest it, reject it, or retain it as a clearly linked historical observation. Never restore its structure or authority. |
| C2 | [`src/cli/tests/preserved/route-list-v1/`](../../../../../../src/cli/tests/preserved/route-list-v1/) | Route-list fixtures and tests only; no preserved route-inspect files | Map each potentially relevant case to an inspect heading and choose keep-as-concept, rewrite, split, merge, or remove with a contract reason. Do not call any file preserved inspect evidence. |
| C3 | Current route-list production and active tests under [`src/cli/`](../../../../../../src/cli/) | The accepted first consumer and its private selection, Loader, filesystem, topology, result, and presentation support | Use only as a predecessor comparison and evidence source. Do not import a List-private path or treat route-list meaning as inspect meaning; the promotion Task makes the per-capability decision. |
| C4 | Completed [route-list contract Task](done/route-list-contracts.md), [route-list acceptance Task](done/route-list-acceptance.md), and their linked evidence | Current route-list planning and acceptance record | Preserve its route-list meaning and exact evidence reference. Reuse no inspect conclusion without a heading-level comparison. |
| C5 | [CLI development-flow observation](../../../../emerging/observations/2026-08-21_cli-development-flow-evaluation.md) | Contextual comparison and rationale, not current architecture | Mark each used claim as accepted by a current source, retest it, or reject it. Historical material never resolves an architecture choice by itself. |
| C6 | Maintainer-supplied [`architecture review`](../../../../../../.temp/review-20.08.2026/open-forge-cli-architecture-review-and-implementation-plan.md), [`shared-folder review`](../../../../../../.temp/review-20.08.2026/open-forge-cli-review-addendum-shared-folders-modern-dotnet-and-scoped-routes.md), and [`3.md`](../../../../../../.temp/review-20.08.2026/3.md.txt) | External or historical review evidence accepted only when projected into current sources | Inventory every used claim, link it to an exact heading, and mark it accepted, retested, or rejected. Do not treat review conclusions as Architecture authority. |

## Complete Capability And Evidence Matrix

Every Interface and Behavior heading is named below, including their nested headings. `contract only` and `proof gap` are planning states; no row is claimed as implemented or covered.

| Contract heading coverage | Production owner to freeze | Unit evidence | Integration evidence | EndToEnd evidence | Native AOT evidence | Candidate/state |
| --- | --- | --- | --- | --- | --- | --- |
| Interface: Status And Authority; Behavior: Status And Authority | Contract and authority audit; no production owner | Not applicable | Not applicable | Authority and source audit | Not applicable | C1, C4-C6 / contract only |
| Interface: Purpose; Behavior: Operation Invariants | Inspect operation call surface and typed result/profile | Direct immutable call-surface and result-shape tests only | Same-input real-workspace determinism and no persistent state | One public read-only journey | Published read-only journey | C1-C4 / proof gap |
| Interface: Syntax; Operands; Flags; Behavior: Request Resolution / Workspace and source reference | Inspect definitions, binding, and complete request; shared flags remain shared | Exact command symbols, arity, operand, and frozen binding model | Parser, workspace, source-reference, delimiter, and terminal-mode boundary | Exact public grammar and invalid-input journeys | Published grammar and terminal bypass | C2-C4 / proof gap |
| Interface: Workspace And Subject; Behavior: Request Resolution / Workspace and source reference; Logical source resolution | Inspect request/resolution boundary using accepted Framework workspace and physical facts | Attempted versus resolved identity states | Current and explicit workspace, ID/path, detached, unrouted, containment, and overwrite selection | Workspace and subject journeys | Published workspace and subject journeys | C1-C4 / proof gap |
| Interface: Identity; Source-State Classification; Errors; Semantic Results; Behavior: Current Facts And Coverage / Source-state classification; Selection And Result Formation | Typed profile, availability, observation, result, and status selection models | Every valid state, invalid combination, and status precedence invariant | Real incomplete, blocked, invalid, and interruption causes | All seven public statuses, streams, exits, and next-action rules | Native all-status process proof | C1-C4 / proof gap |
| Interface: Route Structure; Behavior: Request Resolution / One route and loading graph; Current Facts And Coverage / Route topology | One invocation graph input and Inspect-local topology/profile facts | Fixed graph/topology facts without filesystem access | Root, nested, leaf, detached, sparse, ambiguous, and generated-Entries-independent topology | Public route-chain and topology journey | Native topology projection | C1-C4 / proof gap |
| Interface: Reading Behavior / Task Start Or Resume / Automatic Reading Trigger / Later Reads; Behavior: Current Facts And Coverage / Reading classification / Task start or resume / Automatic reading trigger / Later reads | Inspect-local reading classification and typed reasons | Fixed task-start, parent/event, selected, overwrite, and later-read cases | Real Loader, `#LoadNow`, `#KeepInMind`, and detached/unrouted cases | Human explanations in a complete process | Native reading classification and output | C1-C4 / proof gap |
| Interface: Context Cost / Own Source / Added By Selection / Automatically Read Below Through `#LoadNow` / No Heaviness Score; Behavior: Current Facts And Coverage / Measurement formation / Own source / Selected closure and additions / `#LoadNow` descendants / Availability | Inspect-local measurement sets, exact measurement record, and availability states | Fixed sets, deduplication, aggregate `ceiling(characters / 4)`, zero, unavailable, and not-applicable | Real bytes, Unicode scalars, UTF-8, overwrite layers, unreadable facts, cancellation, and no-write snapshots | Compact and JSON measurement journeys | Native exact measurement and availability proof | C1-C4 / proof gap |
| Interface: Rules And Customization; Behavior: Current Facts And Coverage / Inherited rules and customization | Inspect-local Axioms provenance and base/overwrite layer facts | Sentinel, empty, local, inherited, ordinary-leaf, and base-first cases | Real ancestor chains and overwrite pairs | Expanded provenance journey without rule bodies | Native provenance projection | C1-C4 / proof gap |
| Interface: Human Output / Structured Output / Scenarios; Behavior: Presentation Relationship | Inspect binding-derived help ownership, local human projections, and concrete JSON projection; Shell owns shared pipeline and serialization boundary | Fixed-result compact/expanded/JSON/help shape tests | Pipeline, serialization, and bounded diagnostic integration | Help, compact, expanded, JSON, diagnostics, streams, and exits | Published projection and serialization proof | C2-C4 / proof gap |
| Interface: Non-Goals; Behavior: Effects / Safety And Recovery | Read-only operation boundary and safety translation; no mutation planner | No links, bodies, diagnosis, recommendation, generated drift comparison, or mutation cases | Real containment, cancellation, unchanged-state hashes, and no persistent inspection state | Public no-write journey | Native no-write journey | C1-C4 / proof gap |
| Interface: Verification; Behavior: Conformance Evidence | Acceptance matrix and evidence ownership, not a new runtime owner | Direct cases for every pure fact | Real-OS matrix and cancellation | Complete published process matrix | Published `win-x64` root, Integration, and EndToEnd proof | C1-C6 / proof gap |
| Interface: Related Current Sources; Behavior: Related Current Sources | Linked authority only; no production owner | Not applicable | Not applicable | Not applicable | Not applicable | C1-C6 / not applicable |

## Accepted Preflight And Gray Surface

- Gray input: planning commit `37d2e70` on `feature/cli-route-inspect`, descended from exact route-list baseline `edca509`.
- Command root: `RouteInspectDefinitions` owns schema version, command/operand identity, and finite observation/condition machine codes. Binding, help content, JSON DTOs, and composition remain later ownership.
- `Models/Operation/`: one validated workspace/reference request and one Shell-compatible operation delegate.
- `Models/Resolution/`: attempted selection, source identity, route state, and exact base/optional-overwrite physical layers.
- `Models/Profile/`: tri-state value/unavailable/not-applicable facts; typed automatic/later reading events; exact measurement aggregates; route topology; Axioms provenance; completeness and safety.
- `Models/Result/`: observations, status-compatible availability conditions, and one concrete Shell command result with structural status/selection/profile/next-action invariants.
- Workspace vocabulary retains invalid, unavailable, and unsafe conditions for the required CLI-EDGE-002 comparison; Gray does not change the current Shell classification.
- No route-list-private type is imported or promoted. No parser, filesystem, graph, measurement, rendering, serialization, process, or composition behavior exists in Gray.
- Active toolchain: .NET 10, the scoped `src/cli/OpenForge.Cli.slnx`, and repository `dotnet format`/Release build commands.
- Gray evidence: focused formatting verification and the full Release solution build pass with zero warnings and errors. No tests were added or run as Gray evidence.
- Targeted correctness and improvement reviews pass for the Gray surface.
- Public scenario, managed behavior, real-OS evidence, EndToEnd, and Native AOT remain assigned to later children; Gray claims none of them.

## Allowed And Protected Surfaces

### Allowed

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/`: definitions, binding, and other behavior-owning entry surfaces required by the frozen packet.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Models/`: records, interfaces, property-only classes, and other non-behavioral callable models, grouped further by cohesive topic when the set grows beyond roughly five to ten types.
- This Task's planning record and the parent planning records when the Mastermind records a phase boundary. Only the Mastermind stages or commits.

### Protected

- All route-inspect tests, behavior, filesystem traversal, graph construction, profile formation, rendering, root composition, JSON registration, and promotion until their later child Tasks.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/`, its private support, route-list depth/rows/findings/coverage/results/renderers, and route-list contracts.
- Generic Shell semantics, Framework contracts, preserved candidate files, generated `Entries`, and all unrelated production, test, contract, and generated paths.

## Requirements

- Complete the matrix without omitting a top-level or nested Interface/Behavior heading, including Related Current Sources.
- Freeze command-local definitions, binding, request, one operation call surface, typed result/profile/availability models, help ownership, and JSON projection ownership before any behavior Task starts.
- Record candidate dispositions for C1-C6 and make clear that no preserved route-inspect tree exists. Any newly discovered historical candidate must be added before it is used.
- Keep shared Shell and Framework contracts as inputs. Do not create a command-local replacement for them.
- Keep `zero`, `unavailable`, and `not-applicable` distinct in the frozen model; do not turn this Task into measurement or status behavior.
- Keep the Gray callable boundary explicit and incomplete rather than returning fake domain facts.
- Apply the C# model-placement rule to every new record, interface, and property-only class, and every existing model materially changed or promoted by this slice. Untouched accepted types remain outside this migration.
- Carry CLI-EDGE-002 through CLI-EDGE-005 into the owning later child without silently resolving them here. In particular, do not add a test-only failure trigger for CLI-EDGE-003.

## Evidence

- Phase 0 (Preflight) was read-only and produced the accepted class map and evidence plan above.
- Gray verification is source inspection, focused formatting, targeted review, and a warning-free build of the frozen callable surface. Gray contains no test changes and no executable placeholder behavior.
- The resolution/promotion Red commit authors the first affected executable expectations against the frozen Gray surface. Red may fail as expected but must compile, and it must not modify Gray production.
- The matrix is the required future Unit/Integration/EndToEnd/Native AOT evidence plan. No route-inspect behavior, test pass, promotion, or Native AOT result is claimed by this Task.
- The Mastermind must inspect exact changed paths, protected surfaces, formatting, and the diff before any phase commit. After a mutating Gray phase, the Task update and accepted Gray commit are one boundary; no hash is invented before that commit exists.

## Dependencies

| Dependency | Required state | Effect |
| --- | --- | --- |
| [Route Discovery](_route-discovery.md) and [route-inspect parent](route-inspect.md) | Active with this child selected | Supplies parent outcome and sequence |
| Route-list acceptance at `edca509` | Integrated and exact baseline | Supplies the only accepted predecessor behavior |
| Route Inspect Interface and Behavior Contracts | Current and unchanged | Define every required heading and meaning |
| CLI Architecture, shared operation contract, and CLI implementation Directive | Accepted and applicable | Bound source, dependency, help, serialization, and evidence shape |
| [Edge-case ledger](../../edge-cases.md) | CLI-EDGE-002 through CLI-EDGE-005 recorded | Supplies required later comparisons |

## Stop Conditions

- A contract heading, typed model boundary, help owner, JSON owner, or evidence tier remains materially unresolved.
- Candidate material is treated as preserved inspect implementation, historical architecture, or permission to copy route-list structure.
- A request requires domain behavior, filesystem traversal, graph construction, rendering, composition, promotion, or a test-only failure hook.
- A Gray skeleton would return plausible facts, weaken strictness, or require a new dependency or project.
- The exact baseline or protected route-list tree changes unexpectedly. Return to the Mastermind rather than manufacturing a phase history.

## Progress And Next Action

- Current result: Complete. The route-inspect callable/model surface is frozen under `Commands/Route/Inspect/` with no behavior, tests, promotion, or composition.
- Evidence: Accepted Preflight packet, focused formatting verification, warning-free Release build, exact-path inspection, and passing correctness/improvement reviews.
- Blockers: None.
- Next action: Activate `route-inspect-resolution-promotion.md` from the accepted Gray commit and author its frozen failing Red evidence before production behavior.

## Completion

Completed: the heading matrix, candidate disposition requirements, callable/model/help/JSON ownership, model-locality rule, and compilable Gray surface are accepted. This record remains beside the active route-inspect children until command acceptance performs one link-safe routing closeout.
