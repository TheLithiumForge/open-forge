---
open-forge:
  description: "Historical record: Record the decision to keep Task records in Working Memory and use backlogs and ledgers as selection views"
  tags: [Memory, Task, Planning, Backlog, Contextual, Complete, Archived, Historical]
---

# Task 65 — Where open Tasks live

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/tasks/task65-where-open-tasks-live.md` after the maintainer selected Memory cleanup. Placement decision complete 1/1,1/1; broader Local Planning explicitly remains open.

This record preserves historical evidence. The [current CLI development route](../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Outcome

Recorded on 2026-09-28 from the maintainer's question. Open Tasks live in
Working Memory under `cli-development/tasks/`. Many of them are long-running and
not in progress, which stretches what Working Memory is for: temporary state
needed to continue or resume active work. The maintainer suggested moving them
to a backlog at some point, and asked whether that backlog belongs in Working
or Crystallized Memory.

## What the sources say

- **Working Memory** holds temporary context needed to continue or resume
  active work, and should stay small and current.
- **The Planning backlog Template** describes a small selection view of
  candidate work. Listed work is not automatically accepted, and once a Task
  exists the backlog links to it instead of copying its plan or state.
- **Crystallized Memory** holds accepted knowledge that stays current. A
  queue of candidate work isn't accepted knowledge, so a backlog doesn't fit
  there unless the selection itself is an accepted plan.
- **The ledger** in `project-control.md` owns queue state and ownership today,
  and the Task index orders the open Tasks.

## Existing records

- A [Working backlog](../../../working/backlog.md) already lists the current priorities
  and links to the Task index. It is a selection view, as the Template intends.
- [Local Planning](../../../working/local-planning.md) is an open review of the task
  lifecycle, completion tracking, organization, and archival. The Working
  backlog says it must be completed before those semantics change. This Task's
  question falls inside it, so it may be better folded into Local Planning than
  answered separately.

## Options

1. **Keep everything in Working Memory** and archive promptly, as the
   2026-09-28 review did. Simple, but Working keeps growing with Tasks that
   aren't active.
2. **Active Tasks in Working, candidates in an Emerging backlog.** Tasks not in
   progress move to a backlog scope in Emerging Memory, which already holds
   useful, unaccepted material. A Task moves back to Working when selected.
3. **A backlog file in Working** that replaces the index's ordering sections,
   with Task records staying where they are.

## Decision (2026-10-01)

Task records remain in Working Memory when they are open, including when they
are not currently active. A record may describe locally managed work or point
to an external work system when that system is the appropriate place for its
live facts.

Do not move these Tasks to Emerging Memory or create another Memory category.
The Working backlog and Task index remain selection views. The project-control
ledger remains the source for permanent IDs, queue state, and ownership. These
views link to Task records and do not duplicate Task scope, decisions, or
implementation state. No file moves are needed for this decision. Existing
archived records stay archived.

This closes only the Task 65 placement question. The broader
[Local Planning review](../../../working/local-planning.md) remains open for task
lifecycle, completion, organization, and archival rules. The options above are
preserved as historical analysis, not as current alternatives.

## Current State

**Status:** Task 65 "Where open Tasks live" (phase 1/1): milestone 1/1. The
placement decision is recorded. The broader Local Planning review remains open.

## Decision completion

These checkboxes track decision capture and record reconciliation. They do not
claim runtime tests or a file migration.

- [x] The Working Memory placement and no-new-category decision is recorded.
- [x] The ledger, Task index, plan, and backlog reflect that choice without
      duplicating Task authority.
- [x] The broader Local Planning review remains linked and open.
