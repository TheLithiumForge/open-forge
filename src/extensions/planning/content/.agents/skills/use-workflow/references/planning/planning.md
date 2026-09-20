---
open-forge:
  description: "Turn accepted direction into an executable sequence without creating competing task records"
  tags: [Extension, Workflow, Planning]
---

# Planning

## Goal

Produce an executable plan with clear ownership of outcome, sequence, and current state, so another capable agent can proceed without guessing intent.

Use the project's established task source and record conventions. The [Work Records Pattern](../../../../patterns/work-records.md) fills structural gaps; [Planning Templates](../../../../templates/planning/_planning.md) provide optional starting files. Keep small work together. Ideas, Analysis, and Decisions are optional records for possibilities, evidence, and accepted choices, not prerequisites for a Task or Plan.

## Steps

1. **Identify the task source.** Use the source declared by the user or workspace, or the current request when no durable source is needed. Do not select a competing source merely because a file exists.
2. **Establish the outcome.** Carry forward accepted scope, non-goals, constraints, completion evidence, and existing authorization. Identify decisions that remain outside implementation authority.
3. **Inspect before sequencing.** Read applicable project scopes, affected interfaces, current work, tools, Patterns, and verification requirements. Check the actual state rather than planning from a stale summary.
4. **Expose dependencies.** Identify required inputs, predecessor work, unknowns, compatibility risks, rollback needs, and decisions that must precede execution.
5. **Order coherent steps.** Give each step an observable result and an appropriate verification path. Mark genuinely independent work, sequencing constraints, and user decision points. Do not introduce phases only to fill a template.
6. **Maintain one answer per question.** Update the authorized task and planning sources. Link to accepted reasoning and project knowledge instead of copying them. A separate Plan or Checkpoint is useful only when it needs independent maintenance.

When evidence changes the sequence, revise the affected remaining steps and dependencies. Preserve valid completed evidence, distinguish blocked work from work that can proceed, and name the next action.

## Completion

- The task source or current request is clear.
- The plan covers the outcome, boundaries, dependencies, ordered steps, decision points, and completion condition.
- Each step has an observable result and verification path.
- No unauthorized task, competing status source, or execution permission was created.
