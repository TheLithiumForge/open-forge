---
open-forge:
  description: Form the inspect-local route profile from one accepted graph and fact set without adding diagnosis or mutation
  tags: [Memory, Working, CLI, Task, Route, Inspect, Profile, Contextual, Complete]
---

# Form The Route-Inspect Profile

## Task State

- State: Complete at accepted Green commit `c407e24` (`Implement route inspect profile`).
- Responsible/implementer: Mastermind. Bounded phase delegation is permitted only after this packet and its exact predecessor are closed. Mastermind owns integration, staging, commits, and acceptance.
- Parent: [Implement Route Inspect And Promote Shared Route Facts](route-inspect.md).
- Task baseline: `a54f4e0` (`Complete shared route resolution`). Reviewed Gray is committed at `d53e5f7` (`Freeze route inspect profile contracts`); Red starts from that exact commit and leaves Gray production unchanged.

## Expected Outcome

From the accepted single route/loading graph and fact inputs, form the inspect-local profile: reading classification, task-start membership independent of the operand, automatic and later-read reasons, own/closure/overlap/addition/`#LoadNow`-descendant sets, exact physical-file/Unicode-scalar/UTF-8-byte/aggregate-token measurements, route chain and topology, Axioms provenance, overwrite layers, availability states, semantic status precedence, observations, and the required next action.

## Authority And Backlinks

- Parent: [route-inspect parent](route-inspect.md).
- Predecessor: [resolution and promotion child](done/route-inspect-resolution-promotion.md).
- Meaning: [Route Inspect Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).
- Structure: [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) and [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md).
- Evidence and locality: [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md), [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md), and [Evidence tiers](../../../../../patterns/testing/evidence-tiers.md).
- Edge inputs: [Replacement CLI Edge-Case Ledger](../../edge-cases.md), especially [CLI-EDGE-003](../../edge-cases.md#cli-edge-003--deterministic-public-failed-journey).

## Execution Provenance

Record phase ownership and helper use from this Task onward so later process analysis can distinguish architecture, implementation, evidence, review, and verification work. The runtime does not expose helper model IDs, so helper roles and counts are recorded instead of guessed model names.

| Stage | Primary owner | Supporting agents | Recorded work |
| --- | --- | --- | --- |
| Read-only Preflight | Mastermind / architecture implementer (`openai/gpt-5.6-sol`) | 3 explorers; grounded inputs from 2 advisors plus 1 adversarial advisor | Mapped contracts, current graph/models, evidence patterns, loading semantics, architecture options, and decision-changing risks; Mastermind synthesized and adopted the packet. |
| Gray | Mastermind / architecture implementer (`openai/gpt-5.6-sol`) | 1 `reviewer`, 1 `improvement-reviewer`, 1 `writing-reviewer` | Mastermind directly authored callable/model skeletons and Task records. Review restored one enum invariant and aligned phase/loading/reason/UTF-8 wording. No implementation subagent was used. |
| Gray verification | Mastermind (`openai/gpt-5.6-sol`) | None | Ran focused whitespace verification, the warning-free Release solution build, `git diff --check`, exact diff inspection, and pre-commit Gray verification. |
| Red Unit | Mastermind (`openai/gpt-5.6-sol`) | 1 `red-evidence-author` session, resumed for bounded corrections; 1 `reviewer` | Mastermind froze expectations; the evidence author added 47 fixed-fact cases and named low-arity fixtures. Review corrected generated markers/tags, impossible resolutions, exact measurements, blocked no-profile meaning, determinism, and diagnostics. |
| Red Integration | Mastermind (`openai/gpt-5.6-sol`) | 1 `red-evidence-author` session, resumed for bounded corrections; 1 `reviewer` | Added 12 owned real-OS operation cases. Review corrected the BOM, metadata availability, full entry/hash snapshots, diagnostics, and removed a timing-dependent mid-read claim. |
| Red evidence refinement | Mastermind (`openai/gpt-5.6-sol`) | 1 `improvement-reviewer` | Improved evidence integrity, immutable comparisons, snapshot breadth, fixture realism, and parameter arity without touching Gray production. |
| Red verification | Mastermind (`openai/gpt-5.6-sol`) | None | Reproduced focused formatting, warning-free Release build, full Unit/Integration expected-failure runs, accepted resolution regression, `git diff --check`, and exact-path review. |
| Exceptional Red correction | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer` | Corrected one impossible `null` assertion for a generic value-type fact. `RouteInspectFact<bool>` distinguishes not-applicable through `State`; its unused value storage remains the value-type default. Production Green WIP was isolated before the Red correction. |
| Exceptional Red correction 2 | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer` | Applied the same value-type rule to unavailable `RouteInspectFact<bool>` and made determinism evidence compare optional topology counts without dereferencing the contract-required absent value for an ordinary routed leaf. Production Green WIP was isolated again. |
| Exceptional Red correction 3 | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer`; its provenance-sanitization objection was superseded by explicit maintainer direction | Added the missing line break between generated frontmatter and body in the profile-only real-OS fixture, and aligned unrouted Axioms Unit evidence with the frozen independent inherited/local model and matching Integration evidence. Production Green WIP was isolated again. |
| Exceptional Red correction 4 | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer` | Aligned one startup/selected-overlap Unit count with the contract-required visible ancestor `#LoadNow` closure and matching Integration evidence, and aligned routed-leaf local-Axioms Integration evidence with the frozen local `NotApplicable` value and matching Unit evidence. Production Green WIP was isolated again. |
| Exceptional Red correction 5 | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer` | Corrected one inherited-Axioms expectation that listed a Loader with no `Axioms` section as a contributor. The accepted contract reports only Loader/ancestor sources that contribute inherited `Axioms`; the substantive root remains independently visible. Production Green WIP was isolated again. |
| Exceptional Red correction 6 | Mastermind (`openai/gpt-5.6-sol`) | 1 targeted `reviewer` | Added explicit Loader `#LoadNow` declarations to measurement/reading fixtures whose assertions require root startup membership. Loader root topology establishes routing, not task-start loading; production Green WIP was isolated again. |
| Green implementation | Mastermind (`openai/gpt-5.6-sol`) | 1 `green-behavior-implementer` session resumed for bounded correction; that session reached its step limit, and Mastermind completed the implementation | Implemented one resolver/profile/result operation, typed loading and reading classification, five physical measurements, topology, independent Axioms facts, status/conditions/observations/next action, cancellation, and failure translation. |
| Green reviews and improvement | Mastermind (`openai/gpt-5.6-sol`) | 4 correctness-review sessions including final `reviewer-terra`; 3 `improvement-reviewer` sessions | Corrected Axioms contribution, selected/startup availability, result-stage cancellation, Loader loading ownership, helper arity, Unicode tags, strict destinations, fenced examples, dead state, allocation, and file decomposition. Final correctness review passed and final improvement review found no material improvement. |
| Green verification | Mastermind (`openai/gpt-5.6-sol`) | None | Ran targeted formatting, warning-free Release build, full Unit and Integration suites, accepted resolution regression, changed-file line audit, `git diff --check`, exact artifact inspection, and final reviews. |

## Analysis And Accepted Plan

1. Consume `RouteInspectResolution` exactly once. Its identity identifies the subject; its `RouteInspectGraph` catalogue/topology, already-read source bodies, Loader-root paths, and typed issues are the only profile inputs. Temporary sets are profile calculations, not a second graph. Profile formation does not reopen files, rerun inventory/resolution/topology, or add a universal context engine.
2. Derive route loading from authored topology, exact `#LoadNow`/`#KeepInMind` metadata, and the ordered generated `Entries` declarations already present in each completely read Loader or entrypoint body. A declaration makes only an accepted direct-topology child visible and orders that edge; it cannot invent inventory/topology or trigger drift diagnosis. Loader declarations own root loading; nested declarations intersect current child metadata. Missing, malformed, reordered, and stale declarations affect loading availability/visibility exactly, while generated lines remain non-authoritative for source existence and parentage.
3. The startup route-source set contains Loader-declared `#LoadNow` roots, visible entrypoint `#LoadNow`/`#KeepInMind` traversal, and every Loader-routed non-entrypoint `#KeepInMind` source with the ancestor chain needed to establish it. Loader root topology alone does not imply startup loading. A target-sensitive `#KeepInMind` entrypoint is proactive only when startup-visible or on the selected chain. The selected closure contains the resolved ancestor chain, target, every ancestor/source base plus valid overwrite, and visible `#LoadNow` closure from selected-chain entrypoints. Ordinary links and unrelated on-demand descendants remain absent. Generated declaration order is retained for loading traversal; base always precedes overwrite.
4. Task-start membership is independent of the inspect operand. Preserve every applicable typed reason rather than selecting one. The canonical reason order is parent `#LoadNow`, entrypoint `#KeepInMind`, routed-file `#KeepInMind`, on-demand when no automatic base trigger applies, then `OverwriteAfterBase` when a valid overwrite exists. Later-reading separately records every applicable continuity occasion. Multiple reasons and entrypoint events do not discard a dual-tag continuity reason.
5. Detached entrypoints retain safe local topology, local `#LoadNow` descendants, own measurement, and local Axioms, while Loader-rooted task-start/later/overlap/addition facts are `not-applicable`. Known unrouted sources retain own measurement while route-dependent facts are `not-applicable`. Incomplete Loader/root facts make rooted loading, closure comparison, Loader-rooted topology, and inherited Axioms unavailable; they never reclassify a subject as detached or unrouted. Missing, malformed, or unreadable metadata or required `Entries` declarations make each affected loading calculation unavailable rather than silently on-demand.
6. Parse only the in-memory exact level-2 ATX `Axioms` section outside fenced examples. Record source IDs root-first only when the effective base/overwrite section contributes substantive inherited Axioms. Keep inherited provenance availability independent from local entrypoint state (`substantive`, inherited sentinel, empty, or missing); local Axioms are `not-applicable` for non-entrypoint subjects. Ordinary leaf headings never become active Axioms, and no rule body is emitted.
7. Form distinct own-source, selected-closure, task-start-overlap, selection-addition, and narrow `#LoadNow` descendant sets. Deduplicate logical sources by accepted canonical identity and physical layers by accepted physical identity. Complete bodies come only from the strict UTF-8 reader; measure exact bytes by the strict UTF-8 encoder round trip, which preserves every accepted byte sequence including BOM, CRLF, and non-ASCII scalars. Also measure exact physical files, Unicode scalar values, and aggregate estimated tokens (`ceiling(characters / 4)`). A readable zero, `unavailable`, and `not-applicable` remain different states; any unreadable member makes the affected applicable measurement unavailable rather than partial.
8. Form route chain/topology, independent availability, observations, semantic status, and at most one required next operation. Resolved and incomplete resolutions form a profile; invalid, blocked, failed, and interrupted resolutions do not guess one. Apply `blocked > incomplete > attention > complete` only to ordinary conditions, preserving failed/interrupted event meaning. Do not diagnose, recommend, mutate, compare generated `Entries`, or emit content.

### Phase Boundaries

- Read-only Phase 0 Preflight adopts the exact resolution/promotion predecessor and produces the profile blueprint without mutating Task, Git, production, tests, or evidence. Its no-change result is recorded in the next coherent Gray commit rather than receiving a status-only commit.
- If new callable production surfaces are required, Gray freezes only those surfaces in a compilable production commit with no tests or behavior.
- Red adds the complete affected failing fixed-fact and real-OS evidence without modifying frozen production.
- Green makes the frozen Red evidence pass without changing its expectations or fixtures. Blue and Purple receive separate inspected commits only when they mutate permitted surfaces, and every mutating phase updates this Task in the same commit.

### Frozen Gray Surface

- Add `RouteInspectOperationFactory.Create()` returning `RouteInspectOperation` and an inspect-local coordinator with `ExecuteAsync(RouteInspectRequest, CancellationToken)`. The later implementation will invoke `RouteInspectResolver.ResolveAsync` once, form a profile once only for `Resolved` or `Incomplete` resolutions, and form the result once. It will use concrete composition only: no interface, injection seam, fake resolver, service locator, or second selection.
- Add `RouteInspectProfileBuilder.Build(RouteInspectResolution, CancellationToken)` under `Inspect/Shared/Profile/`. The later implementation will return the typed profile only for accepted resolved/incomplete inputs and will contain no filesystem access.
- Add `RouteInspectResultBuilder.Build(RouteInspectRequest, RouteInspectResolution, RouteInspectProfile?)` under `Inspect/Shared/Result/`. The later implementation will translate issues, observations, status precedence, and the one typed next action without rendering.
- Refine `RouteInspectReadingProfile.Automatic` to carry one tri-state canonical collection of all applicable typed reasons. The collection preserves dual `#LoadNow`/`#KeepInMind` meaning and retains `OverwriteAfterBase` after the logical base reason.
- Refine `RouteInspectAxiomsProfile` so inherited-source availability and local-state availability are independent, and add local `NotApplicable`. No other production model or accepted resolution/shared contract changes in Gray.
- Gray methods must throw explicitly and compile. Gray contains no tests, profile behavior, result behavior, binding, rendering, JSON, help, process evidence, or List changes.

## Allowed And Protected Surfaces

### Allowed

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/` and its inspect-local `Shared/<Capability>/` profile paths for result-forming behavior, with records, interfaces, and property-only classes under the nearest topical `Models/` path.
- Already accepted shared workspace, physical-safety, typed-read, and resolution fact inputs. Do not alter their contracts in this child.
- Mirrored route-inspect Unit and Integration paths under `src/cli/tests/`, including fixed-fact tests, owned real-OS fixtures, cancellation evidence, and no-write snapshots.

### Protected

- Shell, root composition, binding, renderers, help, JSON registration, and process semantics until presentation.
- Route-list command behavior, depth/rows/findings/coverage/results/renderers, contracts, generated `Entries`, and any private List support not explicitly promoted by the predecessor.
- Ordinary-link edges, authored content output, diagnosis, recommendation, mutation, generated-navigation drift comparison, and speculative universal engines.

## Requirements

- Keep task-start membership independent of the inspection operand and preserve detached/unrouted `not-applicable` meaning.
- Represent automatic and later-read reasons as typed facts that can later be rendered in ordinary language. Do not expose internal policy labels as the semantic result.
- Keep own, selected closure, task-start overlap, addition, and `#LoadNow` descendant sets separate and physically deduplicated. Exclude ordinary links and globally recovered `#KeepInMind` leaves from the narrow `#LoadNow` descendant set.
- Count exact physical files, Unicode scalar values, UTF-8 bytes, and aggregate token estimate. Do not sum rounded per-file estimates.
- Preserve route root/chain/parent/depth/children/descendant facts, Axioms source provenance, base/overwrite order, and source observations without creating a scope count or interpreting ordinary leaf headings as active Axioms.
- Keep `zero`, `unavailable`, and `not-applicable` distinct. Unavailable applicable facts select `incomplete`; unsafe or ambiguous identity selects `blocked`.
- Apply semantic precedence `blocked > incomplete > attention > complete` for ordinary conditions. Invalid input stops before operation work; failed and interrupted retain their event meaning.
- Form observations and a required next action only from the accepted profile contract. Do not add diagnosis, recommendation, content, mutation, generated-Entries drift comparison, or result policy from another command.
- Carry CLI-EDGE-003 as bounded failed-result evidence. Do not add a test-only production failure trigger.
- Keep profile, measurement, availability, provenance, observation, and result models in topical `Models/` groups once the nearest folder grows beyond roughly five to ten types.
- Treat unresolved loading metadata as unavailable input, not as an absent tag, and retain every ancestor overwrite in startup/selection measurement and provenance calculations.
- Keep blocked profiles absent rather than forming route/loading facts through unsafe or ambiguous identity. Preserve independently resolved identity and issue facts in the result.
- Prefer at most three parameters on new profile and test callables; never exceed four. Use one named input object when a coherent value set would otherwise grow; do not add long positional signatures during Green.

## Evidence

- Unit evidence freezes: operand-independent startup membership; parent `#LoadNow`, selected/ancestor-required entrypoint `#KeepInMind`, globally routed-file `#KeepInMind`, on-demand, dual-tag, and overwrite reasons in canonical order; later continuity; startup/selected/overlap/addition/`#LoadNow` set membership, ancestor overwrites, ordinary-link/global-continuity exclusions, deduplication, and declaration ordering; Unicode-scalar/strict-UTF-8/aggregate-token arithmetic; measured zero/unavailable/not-applicable; rooted, leaf, detached, unrouted, and incomplete-root topology; independent inherited/local Axioms states; complete/attention/incomplete/invalid/blocked/failed/interrupted result selection, absent blocked profiles, observations, condition precedence, next actions, cancellation, and determinism.
- Integration evidence uses bounded owned real OS workspaces for: one complete rooted operation and byte/entry snapshot; startup versus selection, global non-entrypoint `#KeepInMind`, selected-chain entrypoint `#KeepInMind`, and narrow `#LoadNow` boundaries with reordered/stale generated lines; BOM/CRLF/non-ASCII byte round-trip plus overwrite measurement and measured zero; unavailable body/metadata/Entries and independently retained Axioms facts; detached and known-unrouted applicability; one invalid and one blocked operation translation with no blocked profile; deterministic pre-cancelled execution; and complete/incomplete/blocked no-write snapshots. Direct Unit cancellation proves the profile-stage boundary, and accepted inventory/typed-read regressions supply deeper cancellation coverage without a timing-dependent profile test.
- CLI-EDGE-003 is recorded as bounded Unit result evidence unless a stable real public trigger is found later. No public failure trigger is invented here.
- Presentation, EndToEnd, and Native AOT projection evidence are later responsibilities. This child does not claim human, JSON, help, process, or AOT acceptance.
- The Mastermind inspects exact paths and protected semantics before each mutating phase-boundary commit. No pending hash is invented.

## Dependencies

| Dependency | Required state | Effect |
| --- | --- | --- |
| [route-inspect-resolution-promotion.md](done/route-inspect-resolution-promotion.md) | Accepted and integrated | Supplies one graph and accepted fact inputs |
| Route Inspect Interface and Behavior Contracts | Unchanged | Define profile facts, exclusions, statuses, and conformance |
| Accepted Framework workspace, physical safety, typed reads, and route facts | Reusable | Supply facts without a new universal engine |
| CLI-EDGE-003 | Explicitly carried | Requires bounded failure-result evidence without a test hook |

## Stop Conditions

- Stop if the predecessor forms more than one graph interpretation or leaves a route/loading fact unresolved.
- Stop if a measurement, topology, or availability rule requires guessed identity, a universal engine, ordinary-link traversal, generated-Entries authority, or a new shared contract.
- Stop if zero, unavailable, or not-applicable would collapse, or if status precedence would be moved into a renderer or another command.
- Stop if profile formation needs content output, diagnosis, recommendation, mutation, or a test-only failure trigger.
- Stop and return to the parent if a source, graph, physical-safety, or public-status choice is not closed.

## Progress And Next Action

- Current result: Complete Green profile/result/operation behavior at `c407e24` from exact predecessor `e8dba19`. One resolver result drives one profile and one result; Loader-owned startup loading, selected and narrow closures, reading reasons, exact measurements, topology, Axioms provenance, availability, cancellation, conditions, observations, semantic status, and next action conform without presentation or mutation.
- Evidence: Targeted formatting and `git diff --check` pass. The Release solution build passes with zero warnings/errors. Full Unit is 490/490, full Integration is 130/130, and accepted Inspect resolution remains 30/30. Every changed production file is below 200 lines and every new callable has at most four parameters. Final correctness review passes; final local-improvement review finds no material improvement. Managed `EndToEnd` evidence remains assigned to presentation, and Native AOT execution remains assigned to acceptance.
- Blockers: None. The six bounded Red correction commits preserve the accepted expectation set while correcting impossible value-type assertions, optional facts, fixture realism, cross-tier conflicts, contributor provenance, and explicit startup edges.
- Next action: Presentation adopts exact predecessor `c407e24` without reopening profile behavior.
- Routing note: This completed record remains in place because the authorized `open-forge index` routing action is blocked by the existing `CLI-EDGE-001` duplicate-entrypoint report outside this Task. Integrated acceptance owns the later link-safe move and generated-index refresh.

## Completion

Completed: the profile forms every required reading, set, measurement, topology, provenance, overwrite, availability, observation, next-action, and status fact with the required distinctions. Unit and real-OS Integration evidence, cancellation, no-write behavior, CLI-EDGE-003 without a test hook, file bounds, and final review all pass. Presentation is enabled; link-safe completed-record routing remains deferred as recorded above.
