---
open-forge:
  description: Accepted workflow shape - Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, Completion; generated Entries are containment, Required Routes are cross-tree dependency
  tags: [Memory, Decision, CurrentTruth, Workflow, Routing, Orchestration]
---

# Workflow Shape

Accepted 2026-07-10 and revised 2026-07-17 after dogfood showed workflows were not selected reliably and the implicit execution-mode distinction added little.

- Every workflow defines `Mode`, `Goal` (outcome, optional `- helpful before: ...` prior work, acceptance, stop), `Required Routes`, `Constraints`, `Steps`, `Loop`, `Outputs`, and `Completion`, in that order.
- Every complete workflow declares exactly one primary phase tag: `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification`. The tag is visible in generated Entries and lets agents orient before opening recipes without adding physical routing layers.
- The phases describe increasing commitment, not a waterfall. Work can begin in any phase with sufficient current truth, skip, repeat, or move backward; verification evidence can reopen any earlier phase.
- `Mode` appears before `Goal` and is exactly `linear` or `iterative`. Every workflow is goal-oriented; the former goal-seeking mode is simply iterative work whose Loop runs until Goal acceptance.
- `Constraints` is always present. It contains cross-step invariants or exactly `- none` when no workflow-specific invariant exists.
- Generated `Entries` express containment; `Required Routes` express cross-tree dependency. The name won over "Required Skill Entries" (too narrow) and "Required Route Entries" (collides with the defined term `entry`).
- Required Routes are flat and unconditional, read before Step 1; an unreadable route is a blocker, not a step to skip; "none" is valid; entrypoint-level targets preferred with stable routed files allowed; lines use the generated entry format so tooling can follow them; directives never appear; keep the list short and split when it grows.
- `Loop` remains explicit: linear work executes Steps once, while iterative work states the repeated range, trigger, evidence gained, and Goal stop condition.
- The baseline-loaded workflows route uses the request and routed current truth to select from visible descriptions and tags before opening a relevant workflow, then confirms its Goal. After selection, an optional `- helpful before: ...` Goal item may name prior work that would improve the result. Recommend one available earlier workflow once when that work would help, never block the selected workflow, and proceed with explicit assumptions when the prior work is unavailable or skipped. Honor explicit choice or opt-out.
- Runtime `_workflows.md` axioms own selection and execution only. Exact authoring shape, heading validation, and scaffolding detail belong to maintainer descriptors, `doctor`, and authoring documentation rather than repeated baseline instructions.
- Orchestration: a step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route; a sub-workflow's Required Routes are read at that activation; delegation handoffs name the workflow route and active step.
- Workflow-local #Core routes remain a narrow capability, typically local directives, as the natural consequence of the loader's narrower-scope preference. Reusable skills remain ordinary skills shared through Required Routes.

Rationale detail and the historical TDD dev-workflow example: `.agents/memory/archived/ideas/workflow-redesign.md`. The shipped `dev-workflow` later generalized to an adaptive implement-test-improve-retest-diagnose-fix cycle without changing this workflow shape.
