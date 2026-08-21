---
open-forge:
  description: Implement and accept route inspect, then promote only route facts proved identical by both commands
  tags: [Memory, Working, CLI, Task, Route, Inspect, Promotion, Contextual, Active]
---

# Implement Route Inspect And Promote Shared Route Facts

## Task State

- State: Active after route-list acceptance.
- Implementer: Mastermind.
- Parent: [Route Discovery](_route-discovery.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).

## Expected Outcome

`route inspect` resolves one source and reports its route identity, chain,
loading behavior, context cost, overwrite facts, availability, and safety. The
Task establishes the first evidence-driven promotion from route-list-private
support.

## Start Boundary And Next Action

Split route inspect into closed child Tasks before changing production code. Begin
with child Tasks that close:

1. The contracts-and-evidence map, including every contract heading, preserved
   candidate, predecessor fact, and required evidence.
2. The explicit promotion comparison and decision for each candidate shared fact,
   including whether the fact is promoted or remains local.

Each child Task must define its bounded outcome, allowed and protected surfaces,
evidence, and stop conditions before implementation starts. This closeout
activates the parent only; it does not create the child Tasks.

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

Complete when route inspect and all route-list regressions pass, every promotion is
evidence-backed, and the shared route foundation is ready for source-query Tasks.
