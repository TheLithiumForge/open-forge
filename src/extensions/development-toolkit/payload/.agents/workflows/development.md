---
open-forge:
  description: Deliver an accepted change through implementation, proportionate verification, focused improvement, and cause-level failure handling
  tags: [Extension, Workflow, Development, Implementation, Testing, Refactoring]
---

# Development

## Goal

Deliver one accepted change that fits the current system and is supported by evidence proportionate to its behavior and risk.

## Steps

1. Confirm the accepted outcome, scope, non-goals, mutation authority, acceptance evidence, and authoritative task source when one exists.
2. Inspect the current implementation or work product, applicable `routes`, existing verification, and pre-existing failures. Choose the smallest coherent slice.
3. Define the slice's behavior change or output contract, preservation boundary, and verification path. Use test-first ordering when a regression, stable contract, high-risk behavior, or local convention benefits from a failing baseline. Otherwise, use the clearest practical order.
4. Implement the smallest system-fitting change and run the narrowest evidence that can distinguish success from failure.
5. Classify failures as implementation, expectation, setup, environment, or unrelated state before deciding what to change. When an unexpected failure needs investigation, select the [Debugging Workflow](debugging.md) instead of using speculative edits.
6. After focused evidence passes, review fit, clarity, boundaries, duplication, error handling, and maintainability. Refactor only when the gain is material and intended behavior remains protected.
7. Run focused evidence again after the final change, then run neighboring or end-to-end checks justified by the risk and workspace contract.
8. Compare the result with acceptance evidence. Report pre-existing failures, residual risk, and any work that remains outside scope.

## Completion

- The accepted behavior or deliverable exists and fits the current system.
- Relevant tests or checks pass after the final implementation and improvement.
- Valid expectations were not weakened to obtain a passing result.
- Unexpected failures were diagnosed before behavior or evidence changed.
- Acceptance, residual risk, and verification gaps are explicit.
