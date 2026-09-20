---
open-forge:
  description: "Implement and verify an accepted change using the workspace's methods and constraints"
  tags: [Extension, Workflow, Development, Implementation, Testing, Refactoring]
---

# Development

## Goal

Deliver one accepted change that fits the current system and has evidence matched to its behavior and risk.

## Steps

1. **Establish the boundary.** Carry forward the accepted outcome, scope, non-goals, authorization, completion evidence, and task source when one exists. Ask only for a missing consequential decision or permission.
2. **Resolve blocking design choices.** Check whether an unsettled product, experience, or architecture question could materially change the work. Use an appropriate configured capability when needed; otherwise proceed without an invented preliminary phase.
3. **Inspect the actual system.** Load applicable project scopes, existing implementation, verification procedures, and pre-existing failures. Use the project's technologies, tools, commands, and conventions rather than substituting familiar defaults.
4. **Define a coherent slice.** State the behavior change, preservation boundary, and verification path. Use test-first ordering when a regression, stable contract, high-risk behavior, or local convention benefits from it. Otherwise choose the clearest practical order.
5. **Implement and check.** Make the smallest change that fits the system, then run the narrowest checks that distinguish success from failure.
6. **Diagnose before repairing.** Classify a failure as implementation, expectation, setup, environment, or unrelated state before deciding what to change. Use [Debugging](debugging.md) when investigation is needed. Do not weaken valid expectations to obtain a passing result.
7. **Improve and verify the final state.** Review fit, clarity, boundaries, duplication, error handling, and maintainability. Refactor when the gain justifies it and behavior remains protected. Rerun checks whose inputs changed, plus neighboring or end-to-end checks required by risk or workspace rules.
8. **Close the accepted boundary.** Compare the result with completion evidence. Preserve useful durable outcomes and report pre-existing failures, residual risks, verification gaps, and work outside scope.

## Completion

- The requested behavior or deliverable exists and fits the current system.
- Relevant verification covers the final changed state, with failures and gaps reported explicitly.
- Valid expectations were preserved and unexpected failures were diagnosed.
- Acceptance status, residual risk, and remaining work are clear. Completion does not imply permission to publish or integrate.
