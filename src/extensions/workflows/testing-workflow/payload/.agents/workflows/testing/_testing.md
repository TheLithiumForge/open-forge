---
open-forge:
  description: Produce evidence for a behavior or deliverable without implementing the product change; use to design, run, interpret, or improve tests and checks when confidence, regression coverage, edge cases, or verification gaps are the task
  tags: [Extension, Workflow, PhaseVerification, Quality, Testing]
---

# Testing

Testing produces trustworthy evidence about behavior or outcomes while keeping test intent distinct from implementation details.

## Mode

iterative

## Goal

- outcome: proportionate verification of the requested behavior or outcome, with failures and coverage gaps explained
- acceptance: selected tests or checks target stable behavior, outcomes, or contracts, run as intended, and support a defensible conclusion
- stop: acceptance reached, the expected behavior is unresolved, the environment cannot run required checks, or proceeding would require unauthorized mutation

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/quality/SKILL.md` - test design, evidence standards, behavior preservation, and failure diagnosis

## Constraints

- Declare whether the task is to run existing tests or checks, author new ones, or both before mutating files.
- Do not weaken, delete, skip, or rewrite valid assertions merely to obtain a passing result.
- Prefer observable outcomes and stable contracts over private implementation details.
- Treat an unexpected failure as evidence to investigate, not permission to change expected behavior.

## Steps

1. Establish expected behavior or outcome, risk, scope, and whether authoring tests or checks is authorized.
2. Inspect existing verification conventions, tests or checks, tools, fixtures or evidence sources, coverage, and relevant system boundaries.
3. Select cases for normal outcomes, edge conditions, invalid inputs, state transitions, regressions, and integration seams as applicable.
4. If authoring is in scope, add the smallest clear tests or checks that discriminate the intended result. Prefer a failing pre-change baseline when practical; otherwise state the expected observation before running the check.
5. Run the narrowest relevant checks, then broader checks when the risk or project rules justify them.
6. Investigate unexpected failures to distinguish product defect, test defect, environment issue, and unrelated failure.
7. Correct authored tests or checks only when their expectation, method, or setup is wrong; never weaken a valid contract to pass.
8. Report what the evidence proves, what it does not prove, and any residual coverage gap.

## Loop

Repeat steps 3 through 7 as failures reveal missing cases or incorrect assumptions. Stop when the selected evidence supports the expected behavior or a blocker prevents trustworthy verification.

## Outputs

- declared testing mode: run, author, or both
- tests or checks selected, authored, changed, and run
- pass or failure evidence with diagnosis
- coverage gaps, environment limits, and residual risk

## Completion

- [ ] testing mode and expected behavior are explicit
- [ ] tests or checks target stable behavior, outcomes, or contracts
- [ ] valid assertions were not weakened to pass
- [ ] conclusions match the evidence actually obtained
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
