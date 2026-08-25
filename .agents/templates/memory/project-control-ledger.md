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

## Work Queue And Active Tasks

| Task | Outcome | Priority | Profile | Dependencies | Owned capability | Status | Workspace, branch, and base | Owner session | Next milestone |
| ---- | ------- | -------- | ------- | ------------ | ---------------- | ------ | --------------------------- | ------------- | -------------- |

Use `QUEUED`, `STARTING`, `ACTIVE`, `WAITING_ON_DEPENDENCY`, `CHANGE_REQUESTED`, `AUTHORIZATION_REQUIRED`, `BLOCKED`, `COMPLETED`, `INTEGRATING`, `ACCEPTED`, or `RETIRED`.

## Current Integration Boundary

- Boundary and input baseline:
- Input tasks and order:
- Review budget:
- Required evidence:
- State and candidate baseline:

## Execution Budget Ledger

| Task or boundary | Profile | Review maximum and consumed IDs | Council maximum and consumed IDs | Correction maximum and consumed IDs | Reason for revision |
| ---------------- | ------- | ------------------------------- | -------------------------------- | ----------------------------------- | ------------------- |

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
- Recoverable or stale resources:
- Required recovery action:
- Decision or authorization required:
- Next meaningful milestone:
