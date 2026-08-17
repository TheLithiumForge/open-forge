---
open-forge:
  description: Keep tests independently selectable, proportionate to their boundary, isolated, and free of implicit mutation
  tags: [LoadNow, Directive, Testing, Evidence, Isolation, Parallelism, Snapshot]
---

# Test Evidence Integrity

## Instructions

- Give every test stable, durable selection identity through the native runner or repository convention. Temporary Task names do not establish an enduring test boundary.
- Select evidence depth proportionately: direct evidence proves focused behavior, integration evidence crosses one real boundary, and end-to-end evidence proves a complete externally visible journey.
- Parallel tests own every mutable workspace, home, temporary directory, cache, build output, port, and external fixture they can affect. Shared state is read-only unless an explicit synchronization contract proves otherwise.
- Keep ordinary test execution read-only with respect to generated expectations and other review baselines. An update action is explicit, targeted, serial, and reports its exact mutation.
- Prefer real boundaries and observable resulting state. Introduce a test double only when the real boundary cannot be exercised safely and deterministically, and keep that reason local.
