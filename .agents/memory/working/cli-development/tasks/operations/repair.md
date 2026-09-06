---
open-forge:
  description: Implement explicit repair planning, dry run, application, verification, and recovery
  tags: [Memory, Working, CLI, Task, Repair, Mutation, Recovery, Contextual]
---

# Task 19: Repair

## Task State

- State: Active at phase 3 of 5, milestone 3 of 8. Preflight, Gray, and Red are
  accepted. Green has not begun and remains closed while the accepted Repair
  lane is merged and refrozen against the integrated Task 18 baseline, followed
  by explicit Overseer authorization.
- Permanent mapping: Task 19 “Repair” in the
  [project control ledger](../../project-control.md).
- Queue relation: Task 18 “Extension Remove” is complete and integrated. Task
  20 “Cleanup” has accepted Red but holds Green until Task 19 is accepted and
  integrated.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/repair/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/repair/behavior.md).

## Execution Horizon

The fixed streamlined-assured horizon has five phases and eight milestones:

1. Preflight, activation, and contributor-inventory revalidation.
2. Explicit Gray callable and public-shape review.
3. Explicit Red behavior review.
4. One coherent production and focused-verification pass.
5. Fresh holistic review, at most one grouped correction, final acceptance,
   and integration.

The milestones are Preflight; Gray; Red; coherent production; focused, public,
full managed, and supported `linux-x64` Native AOT verification; fresh holistic
review `T19-R1`; one grouped correction `T19-C1` or a documented no-op; and
acceptance and integration. The first three milestones are complete. Task
Mastermind Kepler II owns the active lane. Green remains unassigned and closed.

## Accepted Preparation, Gray, And Red

The accepted lane is branch `codex/repair-implementation` at record tip
`fb8ce6724f058dc8c2ce3c111708f02867aa7ead`, tree
`129ef2abf36b00dc181107ad0da722d47dc51c72`. Its immutable preparation base is
`76e8e5f1f58e60a9de159318d10e7b3e9f8fc9c9`, tree
`ed698b992b68f76b037b4567d154eadbc013239b`.

Accepted Gray tip `140920d3116fe0744bac933c77041f22107c8ce7`, tree
`4060c945011b41f17822baf1c4f2aeb535069112`, freezes exactly 20 Repair-local
production paths. Accepted Red tip `af59c957e5b74550731554f6195e1330de98ece9`,
tree `903ddc56e487e2976ed0019b8a5973c09c3242a6`, freezes exactly 12 Repair test
paths. Fresh warning-free Release builds preceded the exact Red selections:
Unit executed 51 cases with 49 passing and two failing only at the deferred
planner seam; Integration executed six cases, all failing only at the deferred
operation seam; and exactly three public Repair journeys reached only the
missing-composition boundary. Every selection had zero skips. Accepted review
`T19-RED-ACC-01` found no issue. The three public Doctor journeys remain
unchanged.

## Expected Outcome

`repair` converts accepted repairable Doctor findings into an explicit
reviewable plan, applies only authorized repairs under mutation safeguards,
verifies results, and preserves exact recovery for incomplete work.

## Architecture

- Doctor findings remain observation input. `RepairPlanner` maps only recognized
  repair codes and complete provenance to command-local `RepairPlan` steps.
- Each repair step names target, expected state, intended effect, verification,
  dependency, and recovery boundary.
- Reuse shared mutation primitives and producer-owned repair capabilities. Do not
  duplicate install, update, index, or Extension behavior inside Repair.
- Dry run forms the complete plan and stops before lock/effects.

## Evidence

Cover no repairs, one/many independent and dependent repairs, unrepairable and
unavailable findings, stale Doctor facts, lock/revalidation race, dry run,
confirmation/write policy, partial failure at each step, recovery, post-repair
Doctor outcome, idempotence, preservation, process, and AOT.

## Stop Conditions

Stop before repairing a finding without complete provenance, executing free-form
instructions, hiding producer behavior, claiming all findings repairable, or
continuing after a dependency-invalidating failure.
