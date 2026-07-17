---
open-forge:
  description: Turn an accepted goal into a verified change that fits the current system; use when a concrete change needs design, implementation, testing, review, or handoff preparation
  tags: [Extension, Workflow, Implementation, Testing]
---

# Implementation

Implementation turns an accepted goal into a verified change that fits the current system.

## Goal

- outcome: the requested change exists and fits current architecture, patterns, and directives
- acceptance: the chosen verification passes and the result aligns with loaded patterns and directives, or conflicts are reported
- stop: blocker, scope change, or user decision

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/workflow-primitives/SKILL.md` - shared context check, memory routing, completion, and handoff primitives
- `.agents/skills/implementation/SKILL.md` - fit design, contracts, test derivation, verification review

## Constraints

- Do not edit before loading the routes that constrain the change.
- Design the fit before writing the implementation.
- Use tests as the preferred verification when practical; state the substitute when they are not.

## Steps

1. Load the routed context that constrains the change: patterns, directives, guidance, and memory beyond the required routes.
2. Inspect the existing implementation before deciding the change shape.
3. Design how the change fits current architecture, source boundaries, patterns, and directives.
4. Decide the verification path: tests first, tests during implementation, tests after implementation, or an explicit substitute.
5. Define contracts, APIs, file shapes, skeletons, or expected behavior before implementation details when useful.
6. Derive test cases from the goal, contracts, edge cases, risks, and existing regressions.
7. Implement until verification passes.
8. Review the passing implementation for clarity, boundaries, duplication, error handling, and pattern alignment.
9. Route useful discoveries, accepted changes, and continuation context to the right #Memory or #Core routes.

## Loop

Iterative when verification is available: repeat steps 5 through 8 until tests or the chosen verification pass, the requested scope changes, or a blocker requires user input. If tests are not practical, run steps 3 through 8 once with the declared substitute.

## Outputs

- changed behavior and files changed
- verification performed
- unresolved risks or blockers
- pattern, guidance, decision, or memory candidates

## Completion

- [ ] verification passed or the declared substitute is stated
- [ ] result aligned with loaded patterns and directives, or conflicts reported
- [ ] useful discoveries and continuation context routed to #Memory or #Core
- [ ] handoff written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
