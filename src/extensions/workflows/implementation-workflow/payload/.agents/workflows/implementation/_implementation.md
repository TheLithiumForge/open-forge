---
open-forge:
  description: Implement one accepted concrete change with proportionate verification; use for a focused code, configuration, documentation, design-asset, or other deliverable change that does not need the full iterative development cycle
  tags: [Extension, Workflow, PhaseDelivery, Implementation, Testing]
---

# Implementation

Implementation turns an accepted goal into a verified change that fits the current system.

## Mode

iterative

## Goal

- outcome: the requested change exists and fits the current system, patterns, and directives
- helpful before: accepted architecture when it would materially improve delivery
- acceptance: the chosen verification passes and the result aligns with loaded patterns and directives, or conflicts are reported
- stop: acceptance and verification reached, blocker, scope change, or user decision

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- [fit design, contracts, test derivation, verification review](../../skills/implementation/SKILL.md) - #Skill #Implementation

## Constraints

- Do not edit before loading the routes that constrain the change.
- Design the fit before writing the implementation.
- Use tests as the preferred verification when practical; state the substitute when they are not.

## Steps

1. Load the routed context that constrains the change: patterns, directives, guidance, and memory beyond the required routes.
2. Inspect the existing implementation or work product before deciding the change shape.
3. Design how the change fits current architecture, ownership boundaries, patterns, and directives.
4. Decide the verification path: tests or checks first, during implementation, after implementation, or an explicit substitute.
5. Define contracts, APIs, file shapes, skeletons, or expected behavior before implementation details when useful.
6. Derive test cases from the goal, contracts, edge cases, risks, and existing regressions.
7. Implement the smallest coherent slice and verify it; continue until the accepted outcome is complete.
8. Review the passing implementation for clarity, boundaries, duplication, error handling, and pattern alignment.
9. Route useful discoveries, accepted changes, and continuation context to the right #Memory or #Core routes when warranted, safe, and authorized; otherwise report a proposed destination or that no durable routing is warranted.

## Loop

Repeat steps 5 through 8 until the chosen tests, checks, or substitute verification pass and the accepted outcome is complete. If the substitute fails, correct the implementation or verification premise and repeat the smallest applicable step range. Stop rather than repeat when no pass adds evidence or narrows the problem.

## Outputs

- changed behavior, work product, and files
- verification performed
- unresolved risks or blockers
- pattern, guidance, decision, or memory candidates

## Completion

- [ ] tests or checks passed, or the declared substitute produced acceptance evidence; blockers and gaps are explicit
- [ ] result aligned with loaded patterns and directives, or conflicts reported
- [ ] warranted discoveries and continuation context were routed safely and with authority, a proposed destination was reported, or no routing was warranted
- [ ] handoff written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
