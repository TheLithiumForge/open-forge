---
open-forge:
  description: Implement explicit repair planning, dry run, application, verification, and recovery
  tags: [Memory, Working, CLI, Task, Repair, Mutation, Recovery, Contextual]
---

# Task 19: Repair

## Task State

- State: Queued after Task 18 “Extension Remove” and final revalidation of the
  Task 15/16 contributor inventory.
- Permanent mapping: Task 19 “Repair” in the
  [project control ledger](../../project-control.md).
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/repair/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/repair/behavior.md).

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
