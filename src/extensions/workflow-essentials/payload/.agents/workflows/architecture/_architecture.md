---
open-forge:
  description: Understand the current system and define a structural direction future work can follow; use when system structure needs design, evaluation, change, documentation, or review
  tags: [Extension, Workflow, Architecture, Design]
---

# Architecture

Architecture maps the current system, evaluates structural options, and defines a direction that future work can follow.

## Goal

- outcome: a structural direction with explicit tradeoffs, sliced into implementable work
- acceptance: the user accepts a direction, asks for analysis only, or defers the decision explicitly
- stop: direction accepted, analysis delivered, or decision deferred

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/workflow-primitives/SKILL.md` - shared context check, memory routing, completion, and handoff primitives
- `.agents/skills/architecture/SKILL.md` - architecture mapping, option comparison, decision framing, and migration slicing capability

## Constraints

- Do not propose architecture before mapping the current structure and accepted constraints.
- Prefer incremental designs that can be implemented, tested, and reviewed in clear slices.
- Keep rationale, behavior, reusable structures, and migration work in their owning routes.

## Steps

1. Map the current system, including boundaries, ownership, data flow, constraints, and existing decisions.
2. Define the architectural goal, constraints, and success criteria.
3. Compare viable options and state rejected options when they matter.
4. Select or propose a direction with explicit tradeoffs and consequences.
5. Slice the direction into implementation, verification, migration, cleanup, and rollback work.
6. Route accepted rationale, reusable structures, and follow-up context to the right #Memory or #Core routes.

## Loop

Iterative until direction is accepted or deferred: repeat steps 1 through 5 when new constraints, source findings, risks, rejected options, or user decisions change the architecture. Stop per the Goal's stop condition.

## Outputs

- current-system map summary
- architectural goal and constraints
- compared options and tradeoffs
- selected or proposed direction, with rejected options when useful
- migration and verification slices

## Completion

- [ ] direction accepted, analysis delivered, or decision explicitly deferred
- [ ] accepted rationale and reusable structures routed to #Memory or #Core
- [ ] handoff written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
