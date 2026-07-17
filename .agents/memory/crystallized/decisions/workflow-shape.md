---
open-forge:
  description: Accepted workflow shape - Goal, Required Routes, Steps, Loop, Outputs, Completion; generated Entries are containment, Required Routes are cross-tree dependency
  tags: [Memory, Decision, CurrentTruth, Workflow, Routing, Orchestration]
---

# Workflow Shape

Accepted 2026-07-10 and applied to the payload, the workflows descriptor, the agent-primitives and formatting concepts, workflow-essentials, the dev-workflow extension, and the benchmark harness.

- Every workflow defines `Goal` (outcome, acceptance, stop), `Required Routes`, `Steps`, `Loop`, `Outputs`, and `Completion`; `Constraints` appears only when cross-step invariants exist.
- Generated `Entries` express containment; `Required Routes` express cross-tree dependency. The name won over "Required Skill Entries" (too narrow) and "Required Route Entries" (collides with the defined term `entry`).
- Required Routes are flat and unconditional, read before Step 1; an unreadable route is a blocker, not a step to skip; "none" is valid; entrypoint-level targets preferred with stable routed files allowed; lines use the generated entry format so tooling can follow them; directives never appear; keep the list short and split when it grows.
- Execution modes emerge from section weight: deterministic work carries rich `Steps`, iterative work carries a `Loop` over a step range, goal-seeking work carries a rich `Goal` with an assess-act-check loop until acceptance.
- Orchestration: a step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route; a sub-workflow's Required Routes are read at that activation; delegation handoffs name the workflow route and active step.
- Workflow-local #Core routes remain a narrow capability, typically local directives, as the natural consequence of the loader's narrower-scope preference; local skills are discouraged in favor of native packages shared through Required Routes.

Rationale detail and the worked TDD dev-workflow example: `.agents/memory/emerging/ideas/workflow-redesign.md` (applied; kept for reference until archived).
