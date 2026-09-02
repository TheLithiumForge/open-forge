---
open-forge:
  description: Starting structure for an optional project ledger that preserves accepted meaning, managed tasks, integration, evidence, and recovery across one repository
  tags: [Template, Memory, Working, Project, Architecture, Orchestration, Worktree, Integration, Evidence, Recovery]
---

# Project Control Ledger

Use one active ledger only when managed execution spans multiple tasks, worktrees, sessions, integration boundaries, or resumptions. Do not create it for ordinary discussion or a small sequential change. Extend an existing ledger instead of creating another control plane.

## Project Identity

- Project:
- Repository root:
- Overseer session:
- Vision and architecture sources:
- Decision authority:
- Permission and local-only policy:
- Non-goals:
- Declared stable repository-global task horizon: {`None` or fixed `Y` with its accepted scope; never the active or visible queue size.}

## Vision And Accepted Meaning

- Purpose and current horizon:
- Quality bar and principles:
- User preferences:

Link the current sources that define accepted behavior. Do not restate their detailed contracts in this ledger.

| ID  | Accepted contract, invariant, decision, or semantic authority | Owner | Consumers | Evidence | Status |
| --- | ------------------------------------------------------------- | ----- | --------- | -------- | ------ |

## Active Design Discussions

| ID  | Question | Options or hypothesis | Recommendation | User position | Status | Affected work |
| --- | -------- | --------------------- | -------------- | ------------- | ------ | ------------- |

Use `EXPLORING`, `RECOMMENDED`, `ACCEPTED`, `REJECTED`, `DEFERRED`, or `SUPERSEDED`. Only accepted entries change project authority.

## Accepted Baseline

- Integration workspace and branch:
- Accepted commit:
- Required project evidence:
- Residual baseline risks:

## Mutable Authority Boundaries

- Task records define each accepted phase and milestone horizon, current phase ordinal, completed milestone count, current-state suffix, blockers, findings, and evidence.
- The Program Plan defines dependencies and schedule.
- This ledger defines each task's permanent repository-global numeric ID and actual name, queue state, completion-grace counter, lane, branch, worktree, session, integration, links to task-owned budgets, and genuinely project-level or integration-boundary reservations.
- Checkpoints link to these mutable sources for resumption. Sealed Handoffs remain fixed transfer snapshots.
- Current contracts or designated current documents define accepted product and Framework behavior.

## Task Identity, Queue, And Active Work

| Permanent task ID and actual name | Task record | Queue state | Completion grace | Outcome | Queue order and dependency reason | Profile | Owned capability | Lane, workspace, branch, and base | Owner session | Integration mapping | Budget mapping |
| --------------------------------- | ----------- | ----------- | ---------------- | ------- | --------------------------------- | ------- | ---------------- | --------------------------------- | ------------- | ------------------- | -------------- |

Assign each top-level numeric ID once across the repository, record its mandatory actual name, and never reuse or replace that mapping. Use `ACTIVE`, `RECENTLY_COMPLETED`, or `QUEUED` while the task appears in progress output. After completion grace expires, move the task out of this live queue and preserve its result in the completion ledger. Reopened or follow-up work re-enters under the same ID and name, clears stale completion grace, and links to a Task record that declares the new explicit horizon, phase `1/<new B>`, and truthful zero or preserved completed milestones. Order queued rows by project priority and dependencies, not by ID.

When a task completes, first require the linked Task record to show its final declared phase and full milestone count. Set its state to `RECENTLY_COMPLETED` with two subsequent progress-bearing Overseer updates remaining. Show it on the completion-bearing update without decrementing that count. On each subsequent progress-bearing Overseer update, show it and then decrement the count; dequeue it before the next update when the count is zero. Non-progress Overseer messages and descendant updates never change this counter.

Keep task-local phase and milestone horizons, current phase ordinal, completed milestone count, current-state suffix, blocker, finding, evidence, and next-action state in the linked Task record. Never copy those progress values into this ledger. A queued task does not need a phase or milestone horizon until the Task record accepts one. Keep dependency and schedule state in the Program Plan.

## Current Integration Boundary

- Boundary and input baseline:
- Input tasks and order:
- Review budget:
- Required evidence:
- State and candidate baseline:

## Project And Integration Budget Reservations

| Project or integration boundary | Profile | Reserved review units | Reserved council units | Reserved correction cycles | Authority and reason |
| ------------------------------- | ------- | --------------------- | ---------------------- | -------------------------- | -------------------- |

Never copy a Task's budget maxima or consumed IDs into this ledger. Link to the authoritative Task through its work-queue budget mapping. Record only reservations owned by the project or an integration boundary here. Each coordinated review topic consumes one stable named review unit in its owning Task or boundary. Read-only coordinator validation and synthesis consume none. Record any required holistic review as a separate unit in that same authority.

## Change Requests And Authorizations

| ID  | Kind | Requesting owner | Requested decision or effect | Reason and affected authority | User decision and exact scope | Status |
| --- | ---- | ---------------- | ---------------------------- | ----------------------------- | ----------------------------- | ------ |

## Completion And Integration Ledger

| Boundary or task | Result commits | Evidence | Contract effects | Convergence or conflict decisions | Residual risk | State |
| ---------------- | -------------- | -------- | ---------------- | --------------------------------- | ------------- | ----- |

## Recovery And Current State

- Last reconciled at:
- Git and session state sources:
- Active processes, tasks, and integration:
- Progress visibility: {Observed runtime/process state or `progress unobserved`; silence alone is not failure.}
- Recoverable or stale resources:
- Required recovery action:
- Decision or authorization required:
- Next meaningful milestone:
