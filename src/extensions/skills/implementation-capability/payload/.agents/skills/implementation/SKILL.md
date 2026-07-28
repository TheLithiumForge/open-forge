---
name: implementation
description: Fit, implement, and verify an accepted concrete change. Use to align that change with the current system, design its contracts, derive implementation tests, change a work product safely, or review its verification results.
---

# Implementation

Turn an accepted goal into a verified change.

## Process

- Inspect the existing implementation before deciding the change shape.
- Design how the change fits current architecture, source boundaries, patterns, and directives before editing.
- Prefer tests as verification when practical. State the substitute when tests are not practical.
- Use verification results to review and improve the final implementation.

## References

- Read [Fit the change to the system](references/fit-change-to-system.md) before editing an existing implementation or work product.
- Read [Build a contract skeleton](references/contract-skeleton.md) when tests or implementation need a stable contract, API, file shape, or skeleton before behavior is filled in.
- Read [Derive test cases](references/derive-test-cases.md) before or during implementation when behavior, edge case, regression, or integration tests are needed.
- Read [Use the contract-test-implementation loop](references/contract-test-implementation-loop.md) when the user prefers TDD or the change is safer with tests driving the implementation.
- Read [Review verification](references/verification-review.md) after verification, before final response, handoff, or memory extraction.
