---
open-forge:
  description: Implement and accept route inspect, then promote only route facts proved identical by both commands
  tags: [Memory, Working, CLI, Task, Route, Inspect, Promotion, Contextual, Complete]
---

# Implement Route Inspect And Promote Shared Route Facts

## Task State

- State: Complete from final production correction `9c690b4` and the accepted integrated managed/native, promotion, architecture, and edge closeout.
- Implementer: Mastermind.
- Parent: [Route Discovery](_route-discovery.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).
- Baseline: Exact `edca509` (`Establish and accept route list`) on branch `feature/cli-route-inspect`; this is the exact `develop` baseline.
- Decomposition boundary: No production changes occurred during decomposition.

## Expected Outcome

`route inspect` resolves one source and reports its route identity, chain,
loading behavior, context cost, overwrite facts, availability, and safety. The
Task establishes the first evidence-driven promotion from route-list-private
support.

## Decomposition And Child Sequence

Route inspect is split into the following ordered child Tasks before behavior.
Each child defines its bounded outcome, allowed and protected surfaces, evidence,
dependencies, and stop conditions. Planning decomposition made no production
changes; contracts froze the production-only Gray surface, resolution/promotion
accepted shared facts only after both List and Inspect consumed them, and every
child is now complete.

- [x] [Freeze route-inspect contracts and evidence](route-inspect-contracts.md) — Complete — accepted Gray callable/model surface with no behavior or tests.
- [x] [Resolve route-inspect inputs and audit promotion](done/route-inspect-resolution-promotion.md) — Complete — forms one source/route-loading graph and decides every route-list-private candidate.
- [x] [Form the route-inspect profile](route-inspect-profile.md) — Complete at `c407e24`; link-safe routing deferred — forms inspect-local reading, measurements, topology, provenance, availability, and status facts.
- [x] [Present route inspect through the CLI](route-inspect-presentation.md) — Complete at production commit `51c0960` plus final managed acceptance — binds/registers the leaf and projects one typed result through human, JSON, diagnostics, and help surfaces.
- [x] [Accept route inspect and close Route Discovery](route-inspect-acceptance.md) — Complete — final managed/native evidence, promotion/architecture audits, edge dispositions, and closeout pass.

Development cycle: parent Task `Route Inspect`; integrated acceptance is complete
from final correction `9c690b4`; Route Discovery closeout is complete; authorized
squash integration is starting without reopening command behavior.

## Maintainer-Authorized Post-Completion Sequence

1. Squash-merge the accepted route-inspect result into `develop`.
2. Branch separately from the resulting exact `develop` into one generic-improvements branch. The Mastermind, acting as the top-down architecture implementer, owns Preflight and architecture. Feature-branch commits are authorized. Record one complete-suite beginning baseline, then use focused authored and affected evidence until final acceptance.
3. First apply the updated CLI standard-behavior Directive. Add focused pinned-version evidence for spaced/equals/colon delimiters, repeated occurrences, multi-value options, and attached-empty values; then audit custom lexical parsing, raw-argument rescans, delimiter guards, attached-empty special cases, occurrence handling, and multi-value handling. Prefer pinned `System.CommandLine` behavior plus general typed validation. Treat cases such as `--depth= --json` as triage evidence for the general parser/value invariant rather than as standalone behavior to preserve. Retain a workaround only as a maintainer-visible documented exception with evidence and a removal condition.
4. Only after parser and standard-behavior remediation is accepted, audit repeated real-workspace, generated-entry, measurement, snapshot, and command-fixture setup across active tests. Promote only complete identical utilities to TestSupport; prefer composable utilities, and use a test base class only when a shared lifecycle invariant justifies inheritance.
5. In that same generic-improvements branch, audit long positional signatures (including `RouteListBinding.Close`) and the root-host/Core project split. Target named inputs after three parameters; retain or collapse the project split only from dependency, AOT, test-boundary, and maintenance evidence.
6. Commit coherent phases, run the complete suite once at final acceptance, review and accept the complete generic-improvements result, squash-merge it into `develop`, and only then start the next product Task.

## Required Inputs

Use the active [Replacement CLI Edge-Case Ledger](../../edge-cases.md) as a
required input to decomposition and comparison, including:

- [CLI-EDGE-002 — Workspace failure classification](../../edge-cases.md#cli-edge-002--workspace-failure-classification).
- [CLI-EDGE-003 — Deterministic public `failed` journey](../../edge-cases.md#cli-edge-003--deterministic-public-failed-journey).
- [CLI-EDGE-004 — Hostile process-input breadth](../../edge-cases.md#cli-edge-004--hostile-process-input-breadth).
- [CLI-EDGE-005 — Raw lexical option edge](../../edge-cases.md#cli-edge-005--raw-lexical-option-edge).

These items are required inputs, not automatic blockers. An owning child Task
must record the comparison, evidence, and resolution or explicit residual-risk
acceptance for its assigned item.

## Architecture And Promotion Process

1. Create command-root definitions, binding, request, operation, result, and local
   help/presentation.
2. Map every contract heading and preserved candidate before behavior.
3. Compare required identity, source catalogue, Loader, overwrite, route graph,
   physical path, and metadata semantics with route list.
4. Promote only complete identical units to `Commands/Route/Shared/<Capability>/`
   or `Framework/<Capability>/` when consumers span command families.
5. Move matching tests and fixtures with the promoted semantic unit.
6. Leave list depth, rows, findings, coverage, result, and renderers local. Leave
   inspect profile, measurements, availability, result, and renderers local.
7. Delete obsolete private copies; do not retain forwarding wrappers.

## Required Behavior

Preserve source-ID and exact-path selection, ambiguity, physical safety, route
chain, task-start and later-read behavior, automatic loading, own and closure
measurements, overlap/addition, local and inherited Axioms provenance, overwrite
facts, compact/expanded/JSON parity, diagnostics, help, status, and next actions as
defined by the contracts.

Do not diagnose, recommend content changes, or propose route mutations. Doctor
owns diagnosis.

## Evidence

Use pure fixed-fact Unit tests, real-source Integration fixtures, complete process
journeys, unchanged snapshots, ambiguity and detached-source cases, published AOT,
and route-list regression. Include a promotion audit showing both real consumers,
identical meaning, new nearest scope, moved tests, and no cross-leaf private import.

## Stop Conditions

Stop if similar names hide different semantics, if promotion broadens a command
contract, or if inspect requires a speculative graph/measurement engine. Keep
separate local implementations until identical meaning is proved.

## Completion

Complete. Route Inspect and all Route List regressions pass, every promotion is
evidence-backed, and the shared route foundation is ready for later source-query
Tasks after the authorized generic-improvements sequence. Completed records remain
in place under the accepted `CLI-EDGE-001` link-safe-routing waiver.
