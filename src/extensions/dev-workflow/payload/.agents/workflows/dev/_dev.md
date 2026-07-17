---
open-forge:
  description: Deliver a development task through contract-first TDD - plan, red, implement, green, blue refactor, closeout; use when a task needs end-to-end delivery with tests guarding behavior
  tags: [Extension, Workflow, Development, TDD, Implementation, Testing]
---

# Dev

Dev delivers one development task end to end: contracts first, tests red to green, refactor on blue, then closeout.

## Goal

- outcome: the selected task's behavior change exists, guarded by tests
- acceptance: the task's acceptance criteria are met and tests are green after the refactor without test edits
- stop: blocker, scope change, or user pause

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/workflow-primitives/SKILL.md` - shared context check, memory routing, completion, and handoff primitives
- `.agents/skills/implementation/SKILL.md` - fit design, contracts, test derivation, verification review

## Constraints

- Contracts before implementation; behavior deltas stated before edits.
- Tests stay untouched during the blue refactor; green must hold.
- Moving parts keep their existing behavior unless the task says otherwise.

## Steps

1. Select the task: from the user, or the next task from the workspace's backlog, issues, or tasks route when one exists.
2. Plan the change. For new behavior, establish the contracts: class shapes, function signatures, API surfaces. For existing behavior, state the behavior delta and identify the moving parts it touches.
3. Red: write failing tests from the contracts. For existing code, ensure tests pin the current behavior of the affected moving parts; add them where missing.
4. Implement: fill the contracts or adjust the behavior.
5. Green: run the tests until they pass.
6. Blue: review the result against the greater picture, record observations, and refactor; tests stay green without being edited.
7. Close out: check the task's acceptance criteria, route useful material to #Memory or #Core, and write the handover.

## Loop

Steps 4 and 5 repeat until green. Step 6 repeats until the refactor stabilizes with green tests. Return to step 2 when the plan proves wrong. Stop per the Goal's stop condition.

## Outputs

- implemented behavior change with its guarding tests
- verification runs: red baseline, green, post-refactor green
- acceptance criteria check
- observations, memory candidates, and a handover when work continues elsewhere

## Completion

- [ ] task acceptance criteria checked
- [ ] tests green after the refactor without test edits
- [ ] useful material routed to #Memory or #Core
- [ ] handover written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
