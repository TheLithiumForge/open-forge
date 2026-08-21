---
open-forge:
  description: Keep tests independently selectable, proportionate to their boundary, isolated, and free of implicit mutation
  tags: [LoadNow, Directive, Testing, Evidence, Isolation, Parallelism, Snapshot]
---

# Test Evidence Integrity

## Instructions

- Give every test stable, durable selection identity through the native runner or repository convention. Temporary Task names do not establish an enduring test boundary.
- Select evidence depth proportionately: direct evidence proves focused behavior, integration evidence crosses one real boundary, and end-to-end evidence proves a complete externally visible journey.
- During Preflight, explicitly select the evidence tier and independently runnable project path for every planned test command. When a test surface has multiple projects, project-level tier separation is required; traits and filters are orthogonal selection within a project, not a substitute for the project boundary.
- Parallel tests own every mutable workspace, home, temporary directory, cache, build output, port, and external fixture they can affect. Shared state is read-only unless an explicit synchronization contract proves otherwise.
- Keep ordinary test execution read-only with respect to generated expectations and other review baselines. An update action is explicit, targeted, serial, and reports its exact mutation.
- Prefer real boundaries and observable resulting state. Introduce a test double only when the real boundary cannot be exercised safely and deterministically, and keep that reason local.
- If an applicable Purple test-improvement phase is selected, inspect duplicated real temporary-directory, workspace, and process helpers. Promote a shared real-temp support source only after at least two real test projects or fixtures need identical semantics, place it at their nearest common scope, and keep it out of production. Do not create a generic `Utils` bag or a fake filesystem.
