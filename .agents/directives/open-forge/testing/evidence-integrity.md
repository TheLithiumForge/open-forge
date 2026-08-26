---
open-forge:
  description: Keep tests independently selectable, proportionate to their boundary, isolated, and free of implicit mutation
  tags: [LoadNow, Directive, Testing, Evidence, Isolation, Parallelism, Snapshot]
---

# Test Evidence Integrity

## Instructions

- Give every test stable, durable selection identity through the native runner or repository convention. Temporary Task names do not establish an enduring test boundary.
- Select evidence depth proportionately: direct evidence proves focused behavior, integration evidence crosses one real boundary, and end-to-end evidence proves a complete externally visible journey.
- Follow the [Evidence Tiers Pattern](../../../patterns/testing/evidence-tiers.md): keep Unit evidence cheap and focused over in-memory inputs, use Integration evidence for one real module or system boundary, and reserve EndToEnd evidence for complete black-box journeys through the delivered interface. Do not duplicate detailed lower-tier branch coverage at the system boundary.
- During Preflight, explicitly select the evidence tier and independently runnable project path for every planned test command. When a test surface has multiple projects, project-level tier separation is required; traits and filters are orthogonal selection within a project, not a substitute for the project boundary.
- Name tests by durable behavior, scenario, and expected outcome. Cover the accepted happy, invalid, boundary, failure, and interruption paths at the cheapest tier that proves them, and keep focused selections quick and easy to run without incidental network access, timing sleeps, or unrelated setup.
- Parallel tests own every mutable workspace, home, temporary directory, cache, build output, port, and external fixture they can affect. Shared state is read-only unless an explicit synchronization contract proves otherwise.
- Keep ordinary test execution read-only with respect to generated expectations and other review baselines. An update action is explicit, targeted, serial, and reports its exact mutation.
- Use a focused snapshot when a stable exact string, structured projection, or large reviewed result is clearer than a train of assertions. Keep safety invariants, state transitions, and destructive-effect boundaries as direct assertions, and never broadly normalize a snapshot until it stops proving the contract.
- Prefer real boundaries and observable resulting state. Introduce a test double only when the real boundary cannot be exercised safely and deterministically, and keep that reason local.
- Build test data from small composable seeds, builders, and factories that expose the scenario's meaningful choices. Begin them beside their consumer; promote only identical semantics to the nearest common test scope after demonstrated reuse. Prefer assembling a scenario from focused capabilities over inheritance, a universal fixture, a generic repository, or an options bag that hides which facts the test needs.
- If an applicable Purple test-improvement phase is selected, inspect duplicated real temporary-directory, workspace, and process helpers. Promote a shared real-temp support source only after at least two real test projects or fixtures need identical semantics, place it at their nearest common scope, and keep it out of production. Do not create a generic `Utils` bag or a fake filesystem.
