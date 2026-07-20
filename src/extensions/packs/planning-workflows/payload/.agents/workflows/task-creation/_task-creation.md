---
open-forge:
  description: Create one actionable task in its declared authority; use when accepted work needs a durable owner, scope, acceptance criteria, and execution context
  tags: [Extension, Workflow, PhasePlanning, Planning, Task]
---

# Task Creation

Task Creation produces one actionable task without creating a second source of truth.

## Mode

iterative

## Goal

- outcome: one well-scoped task in, or proposed for, the declared task authority
- acceptance: the task has an owner or decision owner, outcome, boundaries, acceptance evidence, dependencies, and enough context to begin
- stop: no task authority can be declared, external mutation lacks authority, or the proposed work duplicates an existing authoritative task

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/planning/SKILL.md` - task shaping, duplicate prevention, and task-authority discipline

## Constraints

- Use exactly one authoritative task source, declared by the user or workspace, for task status, ownership, acceptance, and completion.
- If none is declared, default to the current user task; never nominate a workspace artifact merely because it exists.
- Never create or mirror a task in an external tracker without authority.
- Do not duplicate status, acceptance, or ownership across multiple authorities.
- Keep accepted rationale, decisions, and durable project truth with their established owners; the task source may link to them but must not replace them.

## Steps

1. Name the one user- or workspace-declared authoritative task source, or explicitly default to the current user task, and confirm whether mutation of that source is authorized.
2. Search that authority, when accessible, for an existing task that already owns the outcome.
3. If an authoritative task exists, propose or make an authorized update there instead of creating a duplicate.
4. Define the task outcome, scope, non-goals, task owner, dependencies, and acceptance evidence; link accepted rationale and decisions from their established owners rather than duplicating them.
5. Add the minimum context, links, constraints, and verification notes needed to start safely.
6. Create or update the task only when authorized; otherwise return a ready-to-record proposal naming the intended authority.
7. Confirm that no second tracker or workspace artifact now claims task truth.

## Loop

Repeat steps 2 through 6 only when duplicate discovery or missing context changes the task shape. Stop after one authoritative task or one ready-to-record proposal exists.

## Outputs

- declared authoritative task source and mutation status
- created or updated authoritative task, or a ready-to-record proposal
- duplicate check result
- scope, acceptance evidence, dependencies, and ownership

## Completion

- [ ] exactly one authoritative task source is named or the current user task is the explicit default
- [ ] one task or proposal owns the complete outcome
- [ ] duplicate task state was not created
- [ ] external mutation occurred only with authority
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
