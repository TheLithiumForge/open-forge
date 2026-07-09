---
open-forge:
  description: Understand the current system and define a structural direction future work can follow; use when system structure needs design, evaluation, change, documentation, or review
  tags: [Extension, Workflow, Architecture, Design, Index]
---

# Architecture

Architecture is the workflow for understanding the current system, evaluating structural options, and defining a direction that future work can follow.

## Axioms

- Read every `Required Skill Packages` route before running `Steps`; report missing routes.
- Do not propose architecture before mapping the current structure and accepted constraints.
- Prefer incremental designs that can be implemented, tested, and reviewed in clear slices.
- Keep rationale, behavior, reusable structures, and migration work in their owning routes.

## Required Skill Packages

- `.agents/skills/workflow-primitives/SKILL.md` - shared context loading, memory routing, completion, and handoff primitives.
- `.agents/skills/architecture/SKILL.md` - architecture mapping, option comparison, decision framing, and migration slicing capability.

## Steps

1. Load the required skills and relevant routed context.
2. Map the current system, including boundaries, ownership, data flow, constraints, and existing decisions.
3. Define the architectural goal, constraints, and success criteria.
4. Compare viable options and state rejected options when they matter.
5. Select or propose a direction with explicit tradeoffs and consequences.
6. Slice the direction into implementation, verification, migration, cleanup, and rollback work.
7. Route accepted rationale, reusable structures, and follow-up context to the right #Memory or #Core routes.

## Loop

This workflow is iterative until direction is accepted or deferred.

Repeat steps 2 through 6 when new constraints, source findings, risks, rejected options, or user decisions change the architecture. Stop when the user accepts a direction, asks for only analysis, or defers the decision.

## Outputs

- current-system map summary
- architectural goal and constraints
- compared options and tradeoffs
- selected or proposed direction
- rejected options when useful
- migration and verification slices
- proposed #Memory and #Core updates

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
