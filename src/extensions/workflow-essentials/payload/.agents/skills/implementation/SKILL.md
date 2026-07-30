---
name: implementation
description: Implementation planning, testing, coding, verification, and review. Use when an agent needs to fit a concrete change to the current system, design contracts, derive tests, implement safely, or review verification results.
---

# Implementation

Use this skill to turn an accepted goal into a verified change.

## Process

- Inspect the existing implementation before deciding the change shape.
- Design how the change fits current architecture, source boundaries, patterns, and directives before editing.
- Prefer tests as verification when practical; state the substitute when tests are not practical.
- Use verification results to review and improve the final implementation.

## References

- `references/fit-change-to-system.md` - Read before editing when the change touches code, docs, routing, architecture, or shared behavior.
- `references/contract-skeleton.md` - Read when tests or implementation need a stable contract, API, file shape, or skeleton before behavior is filled in.
- `references/derive-test-cases.md` - Read before or during implementation when behavior, edge case, regression, or integration tests are needed.
- `references/contract-test-implementation-loop.md` - Read when the user prefers TDD or the change is safer with tests driving the implementation.
- `references/verification-review.md` - Read after verification, before final response, handoff, or memory extraction.
