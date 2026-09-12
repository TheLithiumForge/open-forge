---
open-forge:
  description: Turn accepted direction into an executable profile, compact execution capsule, dependency graph, parallel lanes, and evidence plan
  tags: [Extension, Workflow, Planning, Efficiency, Delegation]
---

# Planning

## Goal

Produce an executable plan with one task source, one compact current context, explicit dependencies, bounded ownership, and verification that another capable owner can follow without rediscovering intent.

Use the [Work Records Pattern](../patterns/work-records.md) when planning needs a predictable record. Keep the plan and current state together for small work. Use separate records only when they need separate maintenance.

## Steps

1. Name the task source declared by the user or workspace. Use the current user task when no durable source is needed.
2. Confirm the accepted outcome, scope, non-goals, constraints, authority, acceptance evidence, and consequential decisions outside implementation authority.
3. Inspect the affected system, direct consumers, current work, contracts, patterns, and verification before choosing sequence.
4. Select Direct, Standard, Assured, Derivative, or Batch execution. Set explicit non-negative review, council, and correction budgets. Identify an existing archetype or golden slice before designing a new foundation.
5. Record the selected profile, maximum budgets, and consumed-budget IDs in one Markdown Execution Capsule in the durable task record or current working context. Include accepted architecture, invariants, placement map, behavior matrix, expected paths, protected paths, direct integration neighborhood, dependencies, evidence ladder, and stop conditions.
6. Divide work into the smallest coherent outcomes with one owner each. Mark sequencing, non-overlapping parallel lanes, integration points, and conditions that require replan.
7. Give every step an observable result and verification matched to its risk. Place full gates at coherent task, archetype, or batch boundaries.
8. Update task or plan state only in its declared source. Link accepted reasoning and current sources instead of copying them.

When evidence changes the plan, update the remaining steps and dependencies in their defining source. Preserve completed evidence, distinguish blocked work from work that can proceed, and name the next action.

## Completion

- One task source and one compact execution capsule are clear.
- The profile, architecture, boundaries, dependencies, ownership, parallelism, evidence, review budget, and completion condition are explicit.
- Every step has an observable result and stop condition.
- No competing task source or speculative process was created.
