---
open-forge:
  description: Apply one conditional material production-structure improvement without changing behavior or evidence
  tags: [Workflow, Development, Phase, Blue, Refactoring, Readability]
---

# Phase 4 - Blue

## Goal

Resolve one named material production-structure concern after Green while preserving contracts, behavior, and tests.

## Steps

1. Confirm that Green is accepted and identify a concrete trigger such as mixed responsibility, harmful duplication, unsafe ownership, confusing control flow, poor locality, or excessive next-slice cost.
2. Skip Blue when no material trigger exists. Do not run it merely because the phase is available.
3. Inspect the changed production code and only the immediate integration neighborhood needed to judge the trigger.
4. Make the smallest structural change that materially improves clarity, safety, locality, or maintenance. Avoid speculative abstraction, cosmetic churn, and unrelated cleanup.
5. Keep contracts, tests, fixtures, snapshots, accepted behavior, and expectation meaning unchanged.
6. Run focused and integration evidence and record the resulting baseline or a no-change conclusion.

## Completion

- The named trigger is materially improved, or inspection found no justified change.
- Behavior, contracts, and evidence remain unchanged and green.
- No speculative or preference-only cleanup entered the task.
