---
open-forge:
  description: Apply one conditional material test-structure or evidence improvement without changing accepted meaning
  tags: [Workflow, Development, Phase, Purple, Testing, Refactoring, Evidence]
---

# Phase 5 - Purple

## Goal

Resolve one named material test-structure or evidence concern after Green while preserving production behavior, contracts, and expectation meaning.

## Steps

1. Confirm that production and focused evidence are green and identify a concrete trigger such as unclear tier placement, harmful fixture duplication, unstable selection identity, shared mutable state, weak cleanup, or poor evidence focus.
2. Skip Purple when no material trigger exists. Do not run it merely because the phase is available.
3. Inspect the changed tests and only the immediate projects, fixtures, support, and runner configuration needed to judge the trigger.
4. Make the smallest test-only change that materially improves locality, independence, clarity, durability, or maintenance.
5. Keep production behavior, callable contracts, and expectation meaning unchanged. Return missing or incorrect behavior evidence to Red rather than redefining it in Purple.
6. Run focused and integration evidence and record the resulting baseline or a no-change conclusion.

## Completion

- The named trigger is materially improved, or inspection found no justified change.
- Production behavior, contracts, and expectation meaning remain unchanged and green.
- No speculative test infrastructure or new behavior entered the task.
