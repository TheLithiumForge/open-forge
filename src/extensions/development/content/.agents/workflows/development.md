---
open-forge:
  description: Implement an accepted change, verify it, improve it where useful, and diagnose failures before changing expectations
  tags: [Extension, Workflow, Development, Implementation, Testing, Refactoring]
---

# Development

## Goal

Deliver one accepted change that fits the current system and has evidence matched to its behavior and risk.

## Steps

1. Establish the accepted outcome, scope, non-goals, existing authority to change files, acceptance evidence, and task source when one exists. Carry clear authorization forward. Ask only when a material decision or required permission is missing.
2. Check whether an unsettled product, experience, or architecture choice could significantly change the slice. If so, use the smallest relevant capability or ask the smallest necessary question. Otherwise, proceed without inventing a phase before development.
3. Inspect the current implementation or work product, applicable project scopes, existing verification, and pre-existing failures. Use the project's sources for technologies, tools, commands, conventions, and required checks where these choices are already defined. Make remaining choices within the accepted scope, then choose the smallest coherent slice.
4. Define the slice's behavior change or output contract, preservation boundary, and verification path. Use test-first ordering when a regression, stable contract, high-risk behavior, or local convention benefits from a failing baseline. Otherwise, use the clearest practical order.
5. Implement the smallest change that fits the system and run the narrowest checks that can distinguish success from failure.
6. Classify failures as implementation, expectation, setup, environment, or unrelated state before deciding what to change. When an unexpected failure needs investigation, select the [Debugging Workflow](debugging.md) instead of using speculative edits.
7. After focused checks pass, review fit, clarity, boundaries, duplication, error handling, and maintainability. Refactor only when the gain is significant and intended behavior remains protected.
8. Ensure the evidence covers the final changed state. Rerun checks whose inputs changed, then run neighboring or end-to-end checks justified by risk or the workspace contract. An unchanged passing result needs no duplicate run.
9. Compare the result with acceptance evidence. Report pre-existing failures, residual risk, and any work that remains outside scope.

## Completion

- The accepted behavior or deliverable exists and fits the current system.
- Relevant tests or checks pass after the final implementation and improvement.
- Valid expectations were not weakened to obtain a passing result.
- Unexpected failures were diagnosed before behavior or evidence changed.
- Acceptance, residual risk, and verification gaps are explicit.
