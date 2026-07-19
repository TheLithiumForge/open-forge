---
open-forge:
  description: Deliver a concrete change through an adaptive implement, test, improve, retest, diagnose, and fix cycle; use when one task warrants full iterative delivery rather than a focused implementation pass or testing-only work
  tags: [Extension, Workflow, Development, Delivery, Implementation, Testing, Refactoring, Debugging]
---

# Development Cycle

Development Cycle delivers one concrete change end to end with an evidence-driven loop that adapts test-first or implement-first ordering to the work.

## Mode

iterative

## Goal

- outcome: the accepted behavior or deliverable exists, fits the current system, and is guarded by proportionate evidence
- acceptance: the task's acceptance criteria are met and relevant tests or checks pass after the final implementation, improvement, and fix
- stop: acceptance reached, blocker, scope change, user pause, unsafe mutation, or repeated passes that produce no new evidence or narrower hypothesis

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/implementation/SKILL.md` - fit design, contracts, test derivation, verification review
- `.agents/skills/quality/SKILL.md` - test design, evidence review, behavior preservation, failure diagnosis, and regression control

## Constraints

- State the intended behavior or outcome, preservation boundary, and verification path before mutating the work product.
- Use test-first when a regression, stable contract, high-risk behavior, or repository convention benefits from a failing baseline; use implement-first only when that is the clearer or more practical path.
- Never weaken a valid expectation merely to obtain a passing result.
- Refactor or improve only when evidence shows a worthwhile structural gain; preserve intended behavior and exclude unrelated cleanup.
- Diagnose an unexpected failure before changing behavior, tests, or checks.

## Steps

1. Confirm the accepted outcome, scope, non-goals, mutation authority, acceptance evidence, and authoritative task source when one is declared; otherwise use the current user task without inventing another tracker.
2. Inspect the current implementation or work product, applicable routes, existing verification, and pre-existing failures. Choose the smallest coherent slice.
3. Define the slice's behavior delta or output contract, preservation boundary, and focused plus broader verification. Select test-first or implement-first and state why.
4. For test-first work, author or select evidence that fails for the intended reason before implementation. For implement-first work, record the baseline and the check that will decide whether the slice succeeds.
5. Implement the slice with the smallest system-fitting change.
6. Run the focused tests or checks. Classify any failure as implementation, expectation, setup, environment, or unrelated state before deciding what to change.
7. After focused evidence passes, review fit, clarity, boundaries, duplication, error handling, and maintainability. Refactor or improve only where the benefit is material and within scope; "no warranted refactor" is valid.
8. Rerun the focused evidence and proportionate neighboring or end-to-end checks after the final improvement.
9. If verification regresses, preserve the failure evidence, reproduce it, test competing hypotheses, fix the cause, and return to the applicable verification step.
10. Close out against acceptance evidence and account for pre-existing and residual failures. Route warranted rationale and project knowledge to their established owners only when that mutation is safe and authorized; otherwise report a proposed destination or that no durable routing is warranted. Write a handoff when continuation benefits from one.

## Loop

Repeat steps 3 through 6 for each justified slice. Repeat steps 7 and 8 only while each pass produces a material improvement and evidence remains trustworthy. A regression enters step 9 and returns to step 6 or 8 after a cause-level fix. Return to step 2 when the chosen slice or verification path proves wrong. Stop and report instead of thrashing when consecutive passes add neither evidence nor a narrower hypothesis.

## Outputs

- implemented behavior or deliverable with its guarding evidence
- selected test-first or implement-first path and rationale
- baseline, focused, post-improvement, and broader verification results as applicable
- refactor or improvement decision, including a justified no-change result
- diagnosed failures, cause-level fixes, and regression evidence
- acceptance criteria check and residual risk
- observations, memory candidates, and a handover when work continues elsewhere

## Completion

- [ ] task acceptance criteria checked
- [ ] relevant tests or checks pass after the final implementation, improvement, and fix
- [ ] valid expectations were not weakened to obtain green results
- [ ] refactoring or improvement preserved intended behavior and excluded unrelated cleanup
- [ ] unexpected failures were diagnosed before behavior or evidence changed
- [ ] warranted rationale and project knowledge was routed safely and with authority without duplicating task truth, a proposed destination was reported, or no durable routing was warranted
- [ ] handover written when continuation would benefit from a static resume note
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
