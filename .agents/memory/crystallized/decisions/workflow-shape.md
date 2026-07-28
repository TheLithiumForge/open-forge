---
open-forge:
  description: Workflows use one predictable goal-oriented recipe shape while phases remain non-waterfall wayfinding and dependencies stay distinct from containment
  tags: [Memory, Decision, CurrentTruth, Workflow, Routing, Orchestration]
---

# Workflow Shape

## Context

Earlier Workflow experiments mixed selection, execution, dependencies, phase order, and iteration in ways that made recipes difficult to compose and validate. A goal-oriented recipe needed enough stable structure for agents and deterministic tools without turning Open Forge into one prescribed development lifecycle.

## Decision

Open Forge uses one complete ordered recipe shape for every Workflow, with explicit execution mode, goal, dependencies, constraints, steps, loop, outputs, and completion evidence.

The accepted design separates several concerns:

- Generated `Entries` express containment, while `Required Routes` express unconditional cross-tree dependencies
- Helpful prior work remains advisory inside the Goal instead of becoming a hidden blocker
- Linear and iterative modes share one explicit Loop section rather than hiding repetition in prose
- Primary phases are visible metadata for increasing commitment, not physical route layers or mandatory chronology
- Organizational entrypoints can route Workflows without pretending to be complete recipes
- The installed entrypoint contains runtime selection behavior while current documents and deterministic validation preserve authoring detail
- Workflow-local #Core remains narrowly available when locality adds real meaning, while reusable Skills stay in their native shared route

## Rationale

One predictable shape lowers navigation and validation cost. Explicit dependencies fail visibly before execution, explicit loops make stopping conditions reviewable, and one primary phase helps pre-load selection without adding a waterfall.

Keeping optional preparation separate from required context lets an agent recommend valuable earlier work without turning every Workflow into ceremony. Keeping containment separate from dependency prevents generated tree structure from becoming an implicit execution graph.

## Alternatives And Tradeoffs

- Free-form recipes reduce schema cost but move recurring interpretation into every agent and prevent reliable validation
- Physical phase folders improve visual grouping but impose extra routing depth and suggest chronology that the model rejects
- A goal-seeking third mode duplicates iterative execution with a Goal stop condition
- Workflow-local copies of reusable Skills improve locality but fragment native runtime discovery and updates

The stable schema adds authoring discipline and must evolve deliberately when dogfood reveals a real limitation.

## Consequences

- A stable recipe shape makes execution and deterministic validation more predictable while adding authoring cost
- A future redesign must preserve or explicitly replace the distinctions this choice established

## Authoritative Sources

- [Current Workflow contract](../documents/framework/primitives/workflows.md)
- [Installed Workflows entrypoint](../../../workflows/_workflows.md)
- [Workflows maintenance contract](../documents/maintenance/payload/agents/workflows.md)

## Historical Context

Historical redesign detail and the former TDD example remain in [Workflow redesign](../../archived/ideas/workflow-redesign.md).

## Decision Relationships

- [Distinct Core primitive roles](core-primitives.md)
- [Routing surfaces](routing-surfaces.md)
