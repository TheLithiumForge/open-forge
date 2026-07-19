---
open-forge:
  description: Translate an accepted experience design into implementation-ready behavior, states, assets, and acceptance evidence; use when engineers need a precise handoff without design intent being lost
  tags: [Extension, Workflow, Design, Handoff, Implementation]
---

# Implementation Handoff

Implementation Handoff converts an accepted design into precise build context without performing the implementation.

## Mode

iterative

## Goal

- outcome: an implementation-ready specification of user behavior, states, content, assets, constraints, and acceptance evidence
- acceptance: an implementer can build and verify the accepted experience without guessing material design intent
- stop: the design is not accepted, a material state or decision is unresolved, required assets are unavailable, or implementation ownership is unknown

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/experience-design/SKILL.md` - accepted-decision capture, state specification, asset inventory, and handoff validation

## Constraints

- Do not implement the design in this workflow.
- Include only accepted design decisions; label unresolved questions and candidate ideas explicitly.
- Describe behavior and acceptance without prescribing unnecessary internal implementation details.
- Do not invent missing content, assets, tokens, or platform capabilities as accepted truth.

## Steps

1. Confirm the accepted design direction, target users, success signal, implementation owner, and source artifacts.
2. Inventory the primary journey and applicable entry, empty, loading, success, error, permission, interruption, responsive, and recovery states.
3. Specify triggers, transitions, feedback, validation, persistence, cancellation, destructive actions, and recovery behavior.
4. Capture exact content or content ownership, accessibility requirements, responsive behavior, localization considerations, and platform constraints.
5. Inventory assets, components, tokens, data, analytics, dependencies, and ownership; identify anything missing.
6. Define observable acceptance evidence for the journey, critical states, accessibility, and significant edge cases.
7. Separate accepted decisions, implementation discretion, unresolved questions, and follow-up validation.
8. Review the handoff with the implementation perspective and route it to the declared owner without writing implementation code.

## Loop

Repeat steps 2 through 7 when implementer questions expose missing behavior or state coverage. Stop when material intent is explicit or an unresolved design decision requires its owner.

## Outputs

- accepted design source and implementation owner
- journey, state, transition, content, and accessibility specification
- asset, component, data, analytics, and dependency inventory
- observable acceptance evidence
- implementation discretion, unresolved questions, and missing inputs

## Completion

- [ ] only accepted design decisions are presented as requirements
- [ ] relevant states, transitions, and recovery behavior are explicit
- [ ] assets, content, accessibility, dependencies, and owners are accounted for
- [ ] implementation-ready acceptance evidence is defined
- [ ] no implementation was performed by this workflow
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
