---
open-forge:
  description: Turn an accepted direction into an executable plan with explicit dependencies, decisions, and verification
  tags: [Extension, Workflow, Planning]
---

# Planning

## Goal

Produce an executable plan with one source for task state so another capable agent can proceed without guessing important intent.

Use the project's established task source and record conventions. The [Work Records Pattern](../patterns/work-records.md) supplies default shapes when a predictable record would help. [Planning Templates](../templates/planning/_planning.md) provide optional starting content. Keep the plan and current state together for small work. Use separate records only when they need separate maintenance.

## Steps

1. Name the task source declared by the user or workspace. Use the current user task when no durable task source exists. Do not choose another file only because it exists.
2. Confirm the accepted outcome, scope, non-goals, constraints, acceptance evidence, and decisions that remain outside implementation authority.
3. Inspect the affected system and current work. Use the applicable project scopes to find relevant interfaces, Patterns, tools, and verification requirements before choosing the sequence.
4. Identify dependencies, risks, unknowns, compatibility boundaries, rollback needs, and decisions that must precede execution.
5. Divide the work into the smallest coherent ordered steps. Give each step an observable result and verification matched to its risk.
6. Mark work that can proceed in parallel, sequencing constraints, user decision points, and the condition that completes the plan.
7. Update task state only in the source that defines it and only when authorized. Link to accepted reasoning and current project sources instead of copying them.

When evidence changes the plan, update the remaining steps and dependencies in their defining source. Preserve completed evidence, distinguish blocked work from work that can proceed, and name the next action.

## Completion

- One task source, or the current user task, is clear.
- The plan states the accepted outcome, boundaries, dependencies, ordered steps, decision points, and completion condition.
- Every step has an observable result and verification path.
- No unauthorized task or competing task source was created.
