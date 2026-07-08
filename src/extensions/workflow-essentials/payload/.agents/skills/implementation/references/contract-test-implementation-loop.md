# Contract Test Implementation Loop

## Use When

Use when the user prefers TDD or the change is safer with tests first.

## Capability

- Contract: define the APIs, file shapes, skeletons, or wiring needed for tests to express the design clearly.
- Test: create the expected failing tests for the intended behavior.
- Implement: add behavior until the tests pass.
- Improve: use the passing tests to review, simplify, and align the implementation with local patterns.

## Expected Result

The final implementation is verified by tests shaped by the intended behavior, not by the final code after the fact.
