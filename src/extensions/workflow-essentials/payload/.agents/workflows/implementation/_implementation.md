---
open-forge:
  description: Turn an accepted goal into a verified change that fits the current system; use when a concrete change needs design, implementation, testing, review, or handoff preparation
  tags: [Extension, Workflow, Implementation, Testing, Index]
---

# Implementation

Implementation is the workflow for turning an accepted goal into a verified change that fits the current system.

## Axioms

- Read every `Required Skill Packages` route before running `Steps`; report missing routes.
- Do not edit before loading the routes that constrain the change.
- Design the fit before writing the implementation.
- Use tests as the preferred verification when practical; state the substitute when they are not.
- Keep the final result aligned with loaded patterns and directives; report unresolved conflicts.

## Required Skill Packages

- `.agents/skills/workflow-primitives/SKILL.md` - shared context loading, memory routing, completion, and handoff primitives.
- `.agents/skills/implementation/SKILL.md` - implementation planning, testing, coding, verification, and review capability.

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
