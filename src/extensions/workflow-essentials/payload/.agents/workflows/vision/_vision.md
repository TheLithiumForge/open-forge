---
open-forge:
  description: Workflow for turning intent into vision, MVP boundary, and growth direction
  tags: [OpenForge, Extension, Core, Workflow, Vision, Product, Index]
---

# Vision

Vision is the workflow for clarifying what should exist, why it matters, what belongs in the first useful version, and where it may grow later.

## Axioms

- Use this workflow when the user wants to define, refine, or challenge product, project, feature, or workspace direction before execution.
- Treat the vision as candidate direction until the user accepts it.
- Separate accepted truth from candidate ideas; do not promote strategic direction to #CurrentTruth without user confirmation.
- Prefer concise questions and concrete options over broad strategy prose.
- Generated `entries` are navigation and reserved load policy only.

## Required Skills

- `.agents/skills/workflow-primitives/loaded-context-check.md` - load enough #Memory, #Workspace, #Guidance, and #Pattern context to ground the vision.
- `.agents/skills/vision/clarify-intent.md` - expose problem, audience, desired outcome, constraints, and non-goals.
- `.agents/skills/vision/shape-mvp.md` - define core value, MVP boundary, future directions, and non-goals.
- `.agents/skills/vision/fit-and-risk-check.md` - check audience fit, market or use fit, validation signals, and risks.
- `.agents/skills/workflow-primitives/memory-routing.md` - route accepted truth and candidate ideas to the right owner.
- `.agents/skills/workflow-primitives/completion-handoff.md` - finish with status, open questions, and resume context when useful.

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
