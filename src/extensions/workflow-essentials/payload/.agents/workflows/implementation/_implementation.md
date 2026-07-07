---
open-forge:
  description: Workflow for designing, testing, implementing, and reviewing a change against current patterns
  tags: [OpenForge, Extension, Core, Workflow, Implementation, Testing, Index]
---

# Implementation

Implementation is the workflow for turning an accepted goal into a verified change that fits the current system.

## Axioms

- Use this workflow when the user wants a concrete change implemented, tested, reviewed, or prepared for handoff.
- Do not edit before loading the routes that constrain the change.
- Design the fit before writing the implementation.
- Use tests as the preferred verification when they are practical.
- State the verification substitute when tests are not practical.
- Keep the final result aligned with loaded patterns and directives; report unresolved conflicts.
- Generated `entries` are navigation and reserved load policy only.

## Required Skills

- `.agents/skills/workflow-primitives/loaded-context-check.md` - load relevant #Memory, #Workspace, #Directive, #Guidance, and #Pattern routes.
- `.agents/skills/implementation/fit-change-to-system.md` - design how the requested change fits existing boundaries and patterns.
- `.agents/skills/implementation/contract-skeleton.md` - define contracts, APIs, file shapes, or skeletons before implementation details when useful.
- `.agents/skills/implementation/derive-test-cases.md` - derive behavior, edge case, regression, and integration tests.
- `.agents/skills/implementation/contract-test-implementation-loop.md` - run the contract, failing-test, implementation, and improvement loop when tests are warranted.
- `.agents/skills/implementation/verification-review.md` - use verification results to review and improve the final implementation.
- `.agents/skills/workflow-primitives/memory-routing.md` - route useful discoveries, decisions, patterns, and handoff material.
- `.agents/skills/workflow-primitives/completion-handoff.md` - finish with status, verification, residual risk, and resume context when useful.

## Steps

1. Load the required skills and relevant routed context.
2. Inspect the existing implementation before deciding the change shape.
3. Design how the change fits current architecture, source boundaries, patterns, and directives.
4. Decide the verification path: tests first, tests during implementation, tests after implementation, or an explicit substitute.
5. Define contracts, APIs, file shapes, skeletons, or expected behavior before implementation details when useful.
6. Derive test cases from the goal, contracts, edge cases, risks, and existing regressions.
7. Implement until verification passes.
8. Review the passing implementation for clarity, boundaries, duplication, error handling, and pattern alignment.
9. Route useful discoveries, accepted changes, and continuation context to the right #Memory or #Core routes.

## Loop

This workflow is iterative when verification is available.

Repeat steps 5 through 8 until tests or the chosen verification pass, the requested scope changes, or a blocker requires user input. If tests are not practical, run steps 3 through 8 once with the declared verification substitute.

## Outputs

- changed behavior
- files changed
- verification performed
- unresolved risks or blockers
- pattern, guidance, decision, or memory candidates
- handoff when continuation would benefit from a static resume note

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
