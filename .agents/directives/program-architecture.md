---
open-forge:
  description: Keep architecture and cross-cutting contracts in one top-down context before delegating closed implementation Tasks
  tags: [LoadNow, Core, Directive, Architecture, Planning, Task, Delegation, Integration]
---

# Program Architecture And Delegation

## Instructions

- For greenfield systems and consequential cross-cutting changes, maintain one
  top-down model of the complete accepted scope before implementation begins.
  The model must cover system boundaries, dependency direction, composition,
  shared capabilities, command or feature growth, build and release boundaries,
  evidence, and integration order.
- The primary architecture agent owns architecture, system decomposition,
  cross-cutting callable contracts, and final integration. In this workspace that
  responsibility belongs to the Mastermind unless the maintainer explicitly
  assigns another architecture implementer. Advisors and explorers may supply
  evidence and alternatives, but a slice implementer must not resolve an open
  architecture question.
- Implement the architectural foundation before delegating feature slices that
  depend on it. Do not ask a feature implementer to create a local substitute for
  a missing global abstraction, composition boundary, build convention, or test
  boundary.
- Use known future scope to prevent local dead ends and to name extension points,
  dependency direction, and promotion triggers. Do not turn possible future
  consumers into speculative shared code. Keep the accepted horizon visible
  while placing each capability at its narrowest current scope.
- Before delegating implementation, close the Task's architectural decisions and
  provide its parent outcome, system context, dependencies, exact allowed and
  protected surfaces, accepted contracts or model, consumers, integration point,
  evidence, verification, and stop conditions. Link the complete sources instead
  of requiring the implementer to rediscover them.
- A delegated Task is not ready when its implementer must choose system
  structure, invent a cross-cutting contract, infer authority, reconcile several
  possible designs, or guess how later Tasks consume the result. Return that work
  to architecture or planning before implementation.
- Decompose work until each delegated Task has one coherent observable outcome
  and can be reviewed against a bounded artifact set. Preserve necessary parent
  context through backlinks and explicit inherited constraints rather than
  copying a large program history into every Task.
- Inspect the actual integrated result from the top-down system perspective.
  Passing local evidence does not accept a change that violates the architecture,
  creates a future integration dead end, or weakens a cross-cutting invariant.
- At every development-cycle transition, write and report the exact active Task,
  the phase just completed, and the phase now starting. Keep the active Task,
  Plan, or Checkpoint current enough that this position survives resumption; do
  not leave phase position only in chat or infer it from uncommitted artifacts.
