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

- Task records define current task phase, blockers, findings, and evidence.
- The Program Plan defines dependencies and schedule.
- This ledger defines task display positions, lane, branch, worktree, session, integration, links to task-owned budgets, and genuinely project-level or integration-boundary reservations.
- Checkpoints link to these mutable sources for resumption. Sealed Handoffs remain fixed transfer snapshots.
- Current contracts or designated current documents define accepted product and Framework behavior.

## Work Queue And Active Tasks

| Display task | Task record | Outcome | Priority | Profile | Owned capability | Lane, workspace, branch, and base | Owner session | Integration mapping | Budget mapping |
| ------------ | ----------- | ------- | -------- | ------- | ---------------- | --------------------------------- | ------------- | ------------------- | -------------- |

Assign each top-level display number once and never reuse it. Keep task-local phase, blocker, finding, evidence, and next-action state in the linked Task record. Keep dependency and schedule state in the Program Plan.

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
