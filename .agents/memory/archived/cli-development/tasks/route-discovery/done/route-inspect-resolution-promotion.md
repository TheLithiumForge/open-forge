---
open-forge:
  description: Resolve one route-inspect source and audit every route-list candidate before any truthful promotion
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Route, Inspect, Resolution, Promotion, Complete]
---

# Resolve Route Inspect Inputs And Audit Promotion

## Task State

- State: Complete after accepted [Freeze Route-Inspect Contracts And Evidence](../route-inspect-contracts.md).
- Responsible/implementer: Mastermind. Bounded phase delegation is permitted only after this packet and its exact predecessor are closed. Mastermind owns integration, staging, commits, and acceptance.
- Parent: [Implement Route Inspect And Promote Shared Route Facts](../route-inspect.md).
- Accepted boundary: Green commit `334f2ba` (`Implement shared route resolution`) plus the reviewed Purple delimiter regression committed with this completion record.

## Expected Outcome

One explicit source reference and workspace form one invocation route/loading fact graph. Every route-list-private candidate is compared and ends with exactly one decision: **promote complete identical unit**, **retain list-local**, **implement inspect-local**, or **already shared**.

Promotion is allowed only when two real consumers have identical meaning, the capability moves to the nearest common scope, matching tests and fixtures move with it, shared facts contain no command policy/status/findings/results, no forwarding wrapper remains, and no cross-leaf private import is introduced.

## Authority And Backlinks

- Parent: [route-inspect parent](../route-inspect.md).
- Predecessor: [contracts and evidence child](../route-inspect-contracts.md).
- Meaning: [Route Inspect Interface](../../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) and [Behavior](../../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).
- Structure: [CLI Architecture](../../../../../crystallized/documents/cli/architecture.md), [Shared CLI Operation Contract](../../../../../crystallized/documents/cli/shared-operation-contract.md), and [CLI implementation Directive](../../../../../../directives/open-forge/cli/implementation.md).
- Promotion: [Source Locality Directive](../../../../../../directives/source-locality.md) and [Nearest Shared Scope Pattern](../../../../../../patterns/software/source-locality/nearest-shared-scope.md).
- Evidence: [Evidence tiers](../../../../../../patterns/testing/evidence-tiers.md), [Test Evidence Integrity](../../../../../../directives/open-forge/testing/evidence-integrity.md), and the [route-list contract Task](route-list-contracts.md).
- Edge inputs: [Replacement CLI Edge-Case Ledger](../../../edge-cases.md), especially [CLI-EDGE-002](../../../edge-cases.md#cli-edge-002--workspace-failure-classification) and [CLI-EDGE-005](../../../edge-cases.md#cli-edge-005--raw-lexical-option-edge).

## Analysis And Accepted Plan

1. Resolve one source reference using the accepted shared grammar and one exact workspace. Preserve attempted and resolved identity separately. Do not add fuzzy matching, parent-workspace discovery, or a second source identity system.
2. Form at most one current in-memory route/loading graph using the accepted route and loading rules. Do not create a second interpretation of startup, selection, `#LoadNow`, `#KeepInMind`, overwrite, or topology behavior.
3. Compare each mandatory candidate below with the accepted inspect contract and the live route-list consumer. Similar names, similar data, or a possible future consumer are not identical meaning.
4. Record one final disposition for every candidate before moving code or tests. If a promotion is truthful, move the complete semantic unit and its evidence to the nearest allowed scope. If it is not, keep the route-list candidate local or implement the inspect meaning locally.
5. Reuse Framework workspace selection, physical path safety, and typed reads as already shared. Record those capabilities as **already shared**, not as new promotions.

### Phase Boundaries

- Phase 0 adopts the frozen Gray surface, exact predecessor commit, promotion audit, allowed paths, and expected failure set before mutation.
- Resolution/promotion Gray adds only candidate shared fact contracts, Inspect resolution handoff models, and explicit throwing callable skeletons. It contains no tests, parser/topology/resolution behavior, List migration, or completed promotion.
- Red adds the complete affected failing Unit and Integration evidence and Task progress without modifying frozen Gray production.
- Green makes the frozen Red evidence pass with the smallest resolution and promotion behavior; it does not change Red expectations or fixtures.
- Blue and Purple receive separate inspected commits only when they mutate their permitted production or test/support surfaces. Every mutating phase updates this Task in the same commit.

### Mandatory Candidate Audit

| Candidate                                           | Route-list evidence to compare                                                  | Required comparison and final decision                                                                                                                                                          |
| --------------------------------------------------- | ------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Source-reference parsing and identity               | `Commands/Route/List/Shared/Selection/` and its matching Unit/Integration cases | Compare attempted/resolved identity, exact path, ID, quoting, collision, and overwrite meaning. End with one of the four permitted decisions.                                                   |
| Source catalogue                                    | Route-list catalogue and source-identity support under `List/Shared/Selection/` | Compare source universe, kind, route state, physical identity, and ordering. Do not promote a command-shaped catalogue or status policy.                                                        |
| Loader Entries, destination parsing, and resolution | `List/Shared/Loader/` and its matching tests                                    | Compare marker, declaration, percent-decoding, containment, root identity, and failure facts. Promote only a complete identical fact unit.                                                      |
| Metadata and source form                            | `List/Shared/Filesystem/` metadata/source-form support and fixtures             | Compare recognized entrypoint, native source, compatibility form, malformed/read states, and generated-region treatment. Keep command interpretation local.                                     |
| Overwrite pairing                                   | Route-list selection/filesystem pairing and overwrite cases                     | Compare base-first logical identity, valid pair, orphan, ambiguity, and physical layers. Do not promote result or status policy.                                                                |
| Inventory and read facts                            | `List/Shared/Filesystem/` inventory/read support and real-OS fixtures           | Compare contained source facts, strict reads, read causes, cancellation, and no-write boundaries. Keep list rows, findings, and coverage local.                                                 |
| Physical alias and collision facts                  | Accepted Framework physical safety plus List alias/collision translation        | Record Framework safety as already shared. Decide separately whether any List translation has identical inspect meaning; never move a command finding as a shared fact.                         |
| Route parent/child graph and topology               | `List/Shared/Topology/` and route-list graph/topology tests                     | Compare authored parentage, chain, depth, child/descendant relationships, ordering, and generated-Entries independence. Do not create a universal graph engine or move list depth/rows/results. |

### Already-Shared Inputs

| Capability                     | Required record                                                                                 |
| ------------------------------ | ----------------------------------------------------------------------------------------------- |
| Framework workspace selection  | Already shared; reuse the accepted capability and do not re-promote it for inspect.             |
| Framework physical path safety | Already shared; reuse component-wise containment and identity facts and do not re-promote them. |
| Framework typed reads          | Already shared; reuse typed read outcomes and do not re-promote them.                           |

### Accepted Promotion Decisions And Gray Contracts

The shared paths below are candidate contracts during Gray. A promotion becomes
accepted only in Green when both real consumers use the same implemented unit,
its evidence moves with it, and the obsolete List-private implementation is
deleted. Gray does not create a second accepted owner.

| Candidate                             | Accepted decision                                                                                                                                                                                    | Gray boundary                                                                                                  |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Source-reference parsing and identity | Promote exact non-null ID/path grammar and automatic identity in Green; keep List's null-to-Loader-root policy local                                                                                 | `RouteSourceReferenceParseResult`, `RouteSourceReferenceParser`, `RouteSourceIdentity`, and `RouteLogicalPath` |
| Source catalogue                      | Promote intrinsic Loader/entrypoint/Markdown/native source facts, documents, metadata, exact-path lookup, and deterministic ID collisions                                                            | `RouteSource`, `RouteSourceDocument`, `RouteSourceMetadata`, and `RouteSourceCatalogue`                        |
| Loader parsing and resolution         | Promote destination and generated-Entries parsing; retain Loader-root selection, issue/status mapping, and physical destination resolution in each command                                           | Neutral Loader parse models and throwing parser skeletons                                                      |
| Metadata and source form              | Promote intrinsic source-form/metadata facts and identical metadata parsing; keep Inspect Axioms/loading interpretation local                                                                        | Source document/metadata models and `RouteMetadataParser` skeleton                                             |
| Overwrite pairing                     | Promote paired/orphan/ambiguous overwrite inventory facts and base/overwrite source layers; keep command status and next-action policy local                                                         | `RouteOverwriteFact` plus catalogue overwrite lookup                                                           |
| Inventory and read facts              | Retain traversal and read-to-command translation locally because coverage differs; reuse existing Framework typed reads and physical safety; both commands produce the candidate shared source facts | No shared inventory reader or status model                                                                     |
| Physical aliases and collisions       | Reuse Framework physical identity; retain command translation locally; promote only deterministic automatic-ID collisions in the catalogue                                                           | `RouteSourceIdentityCollision`; no shared finding/status                                                       |
| Route parent/child graph and topology | Promote fact-only authored parent/child relationships and Loader-root facts; keep List depth/rows/coverage and Inspect loading/profile interpretation local                                          | `RouteTopologyNode`, `RouteTopologyFacts`, and throwing `RouteTopologyBuilder` skeleton                        |

Inspect Gray adds `RouteInspectGraph`, fact-only resolution issues/results, and a
throwing `RouteInspectResolver.ResolveAsync` surface. It does not translate issues
to semantic status or final conditions.

### Phase 0 Evidence Plan

- Red Unit evidence: shared ID/path grammar and identity; source documents,
  metadata, catalogue collisions, overwrite states, Loader parsers, and fixed
  authored topology; Inspect resolution state/issue invariants.
- Red Integration evidence: one exact real workspace for ID/path, collision,
  overwrite, detached, known-unrouted, Loader-subject, unsafe, malformed,
  cancellation, and unchanged-state resolution. No EndToEnd or presentation claim.
- Green regression: all route-list Unit and Integration evidence, including
  CLI-EDGE-002 workspace classification and CLI-EDGE-005 bounded raw arguments.
- Correction boundary: a mismatch in grammar, source facts, Loader parsing,
  overwrite meaning, or parentage returns to this Gray contract. Command-local
  selection/status differences do not justify a shared-policy type.
- Exact commands: focused project tests by feature/evidence trait during Red and
  Green, then full Unit and Integration projects plus focused formatting and the
  warning-free Release solution build.

## Allowed And Protected Surfaces

### Allowed

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Resolution/` for inspect-local resolution when the predecessor packet permits behavior.
- A `Models/` child within the nearest resolution or promoted capability for every record, interface, and property-only class; split larger sets by identity, source, Loader, overwrite, or topology topic rather than creating one flat catalogue.
- A qualified `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/<Capability>/` or `src/cli/core/OpenForge.Cli.Core/Framework/<Capability>/` only after the promotion audit proves the complete identical unit and nearest shared scope.
- Directly affected `Commands/Route/List/` private paths only for a truthful move or narrowly necessary adapter during promotion. Remove obsolete private copies; do not retain forwarding wrappers.
- Matching Unit and Integration tests and fixtures, with the test tree mirroring the accepted production scope.

### Protected

- Route-list depth, rows, findings, coverage, results, renderers, and command policy.
- Inspect profile, measurements, availability, results, and renderers until the later children.
- Generic Shell semantics, contracts, generated `Entries`, and accepted Framework capabilities except as consumed inputs.
- Cross-leaf private imports, a universal route/measurement graph, speculative future consumers, and unrelated production or test paths.

## Requirements

- Resolve one source and one exact workspace per invocation, with no hidden discovery or second graph interpretation.
- Preserve source-reference and workspace raw-option meaning and run the route-list regression for the CLI-EDGE-005 bounded-original-arguments/`--` aspect.
- Compare and decide every mandatory candidate. A candidate cannot remain implicitly shared, implicitly copied, or undecided.
- Resolve and record CLI-EDGE-002's workspace-failure classification without silently changing route-list semantics. The route-list regression is required for any shared correction or explicit residual-risk acceptance.
- Promote only complete identical semantic units with two real consumers, nearest scope, moved evidence, fact-only content, no wrappers, and no cross-leaf private imports.
- Keep route-list command policy, statuses, findings, rows, depth, coverage, result, and rendering local.
- Relocate every new record, interface, and property-only class, and every existing model materially changed or promoted by this slice, into its nearest `Models/` path with matching consumers/tests and no forwarding type.

## Evidence

- Unit evidence uses fixed facts for source identity, catalogue, Loader parsing, overwrite pairing, read translation, alias/collision facts, and authored topology. It does not claim real filesystem behavior.
- Integration evidence uses owned real OS workspaces for exact source/workspace resolution, Loader forms, source metadata, overwrite pairs, aliases/collisions, graph/topology, cancellation, and unchanged bytes where affected.
- Every promotion decision includes both real consumers, identical meaning, nearest scope, moved tests/fixtures, and a dependency/private-import audit. A missing consumer or changed meaning is evidence against promotion.
- The route-list managed regression is mandatory for CLI-EDGE-002 and the CLI-EDGE-005 raw lexical aspect. Inspect public process and Native AOT acceptance remains with later children; no such proof is claimed here.
- The Mastermind inspects exact paths before each mutating phase commit and records the accepted phase boundary. No pending child hash is invented.

## Dependencies

| Dependency                                                       | Required state                       | Effect                                                         |
| ---------------------------------------------------------------- | ------------------------------------ | -------------------------------------------------------------- |
| [route-inspect-contracts.md](../route-inspect-contracts.md)      | Accepted and integrated              | Freezes the local contract and evidence map                    |
| Route-list implementation and evidence at `edca509`              | Available as the first real consumer | Supplies the comparison target, not automatic shared ownership |
| Framework workspace, physical safety, and typed-read foundations | Accepted                             | Reused inputs; not re-promoted                                 |
| CLI-EDGE-002 and CLI-EDGE-005                                    | Compared and recorded                | Blocks silent status or lexical drift                          |

## Stop Conditions

- Stop on merely similar semantics, a broadened command contract, a second graph interpretation, or a speculative future consumer.
- Stop if promotion would move command policy, status, findings, rows, depth, coverage, results, rendering, or other product meaning into shared facts.
- Stop if a complete identical unit, two real consumers, nearest scope, moved evidence, or dependency direction cannot be proved.
- Stop if resolution needs a new architecture, dependency, filesystem guarantee, parser rescan, or private cross-leaf import.
- Stop and return to the parent if CLI-EDGE-002 or CLI-EDGE-005 cannot be resolved without reopening accepted contracts.

## Progress And Next Action

- Current result: Complete. Shared source-reference, logical-path, source-form, metadata, catalogue, overwrite, Loader-parser, and authored-topology units have both List and Inspect consumers. Obsolete List-private implementations and superseded evidence are removed or moved. List inventory/status/depth/findings/results remain local; Inspect inventory/resolution issues remain local; Framework workspace, physical safety, and typed reads remain already shared.
- Evidence: Green commit `334f2ba`. Focused formatting, the warning-free Release build, and 443/443 Unit cases pass after the reviewed Purple `--` delimiter regression. The accepted Green boundary also records 118/118 Integration cases and 7/7 process EndToEnd cases against the built Release executable. Targeted correctness and local-improvement reviews pass. All changed/new production files remain under 200 lines.
- Edge decisions: CLI-EDGE-002 receives no shared correction because Inspect resolution consumes an already selected exact `CliWorkspace`; the existing Shell classification and residual public risk remain later concerns, while the unchanged route-list unavailable-workspace Integration case passes. CLI-EDGE-005 preserves parser-owned typed input and performs no promoted raw-argument rescan; attached-empty depth EndToEnd evidence and the explicit route-list lexical stop at `--` both pass.
- Blockers: None.
- Next action: Start [Form The Route-Inspect Profile](../route-inspect-profile.md) from this accepted completion boundary, binding the resulting closeout hash in profile Phase 0 before profile mutation.

## Completion

Complete this child only when one source and one invocation graph are resolved, every mandatory candidate has one of the four explicit dispositions, reused Framework capabilities are recorded as already shared, any moved semantic unit has its tests/fixtures and dependency audit, CLI-EDGE-002 and CLI-EDGE-005 are recorded with route-list regression evidence, and no protected boundary changed. The Mastermind integrates and commits the accepted phase, updates the parent/Plan/Checkpoint, and enables `route-inspect-profile.md`. Move this record to `done/` only after acceptance.
