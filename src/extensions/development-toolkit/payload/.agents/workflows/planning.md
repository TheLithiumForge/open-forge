---
open-forge:
  description: Turn an accepted direction into an executable plan with explicit dependencies, decisions, and verification
  tags: [Extension, Workflow, Planning]
---

# Planning

## Goal

Produce an executable plan that preserves one authoritative source for task state and lets another capable agent proceed without guessing material intent.

## Steps

1. Name the user- or workspace-declared authoritative task source. Use the current user task when no durable task source is declared, and do not nominate another artifact merely because it exists.
2. Confirm the accepted outcome, scope, non-goals, constraints, acceptance evidence, and decisions that remain outside implementation authority.
3. Inspect the affected system, current work, interfaces, relevant patterns, and available verification before choosing the sequence.
4. Identify dependencies, risks, unknowns, compatibility boundaries, rollback needs, and decisions that must precede execution.
5. Divide the work into the smallest coherent ordered steps. Give each step an observable result and proportionate verification.
6. Mark work that can proceed in parallel, sequencing constraints, user decision points, and the condition that completes the plan.
7. Update task state only in the authoritative task source when authorized. Link accepted rationale and durable project truth from their authoritative sources instead of duplicating them.

## Completion

- One authoritative task source or the current user task is explicit.
- The plan states the accepted outcome, boundaries, dependencies, ordered steps, decision points, and completion condition.
- Every step has an observable result and verification path.
- No unauthorized task or duplicate source of task truth was created.
