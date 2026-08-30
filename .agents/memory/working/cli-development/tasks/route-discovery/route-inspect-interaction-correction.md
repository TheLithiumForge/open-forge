---
open-forge:
  description: Complete the already-accepted Route Inspect interactive source-collision selection
  tags: [Memory, Working, CLI, Task, Route, Inspect, Interaction, Correction, Contextual]
---

# Complete Route Inspect Interactive Collision Selection

## Task State

- State: Planned after the native interactive-session foundation; all
  command-local prompt decisions are closed.
- Parent: [Route Discovery](_route-discovery.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).

## Outcome

When a prompt-capable human request uses a non-unique automatic source ID, Route
Inspect lists the exact candidate paths on stderr, asks once, and accepts either
the one-based displayed number or one exact displayed path. Invalid input or end
of input retains the existing blocked collision; cancellation is interrupted.
JSON and redirected requests retain the blocked exact-path behavior and never
prompt.

This is a post-completion conformance correction. It does not reopen Route
Inspect profile, measurement, loading, topology, rendering, or promotion meaning.

## Ownership

- Production: `Commands/Route/Inspect/**` only.
- Evidence: focused Route Inspect Unit and Integration tests.
- Protected: `Shell/Interaction/**`, root composition/help, shared serialization,
  other Route commands, and all existing result schemas and finite values.
- Integration owns the exact root composition delta and published-process
  noninteractive evidence.

## Evidence

Cover ordinal candidate display, one valid number, one exact displayed path,
blocked invalid answer/end of input, interrupted cancellation, `interactive`
selection method, retained `attention` and exact-path next action, ambiguous-route
blocking, no stdout prompt text, and no session call for JSON or redirected
input. Re-run directly affected Route Inspect regressions.

## Stop Conditions

Stop before adding a qualifier syntax, selection heuristic, prompt library,
general retry/default policy, renderer/schema change, or any feature addition or
removal not accepted by the maintainer.
