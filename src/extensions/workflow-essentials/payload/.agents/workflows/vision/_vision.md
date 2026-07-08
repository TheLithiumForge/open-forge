---
open-forge:
  description: Workflow for turning intent into vision, MVP boundary, and growth direction
  tags: [Extension, Core, Workflow, Vision, Product, Index]
---

# Vision

Vision is the workflow for clarifying what should exist, why it matters, what belongs in the first useful version, and where it may grow later.

## Axioms

- Use this workflow when the user wants to define, refine, or challenge product, project, feature, or workspace direction before execution.
- Load every `Required Skill Packages` route before running `Steps`; report missing routes.
- Treat the vision as candidate direction until the user accepts it.
- Separate accepted truth from candidate ideas; do not promote strategic direction to #CurrentTruth without user confirmation.
- Prefer concise questions and concrete options over broad strategy prose.
- Generated `entries` are navigation and reserved load policy only.

## Required Skill Packages

- `.agents/skills/workflow-primitives/SKILL.md` - shared context loading, memory routing, completion, and handoff primitives.
- `.agents/skills/vision/SKILL.md` - vision shaping, MVP boundary, fit, risk, and growth-direction capability.

## Steps

1. Load the required skills and relevant routed context.
2. Clarify the problem, audience, desired outcome, constraints, and non-goals.
3. Identify the core value that must exist for the direction to matter.
4. Shape the MVP boundary and separate later growth directions.
5. Check fit, risks, validation signals, and assumptions that could invalidate the direction.
6. Ask the user to accept, revise, defer, or reject the direction.
7. Route accepted truth, candidate ideas, and follow-up context to the right #Memory routes.

## Loop

This workflow is iterative.

Repeat steps 2 through 6 when the user changes the audience, core value, MVP boundary, constraints, risks, or validation path. Stop when the user accepts the direction, asks to defer, or rejects the work.

## Outputs

- accepted or candidate vision summary
- MVP boundary and explicit non-goals
- future directions
- risks, assumptions, and validation signals
- open questions
- proposed #Memory updates or handoff

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
