---
open-forge:
  description: "Coordinate dependent tasks, preserve interrupted work, and verify authorized integration"
  tags: [Extension, Workflow, Orchestration, Planning, Worktree, Integration]
---

# Managed Delivery

## Goal

Deliver related tasks with clear responsibilities, resumable state, and verified integration within the user's authority. Use this method when dependencies or multiple contributors make coordination useful. One person or agent may perform every responsibility; independent small work can use [Development](../development/development.md) directly.

[Orchestration Templates](../../../../templates/orchestration/_orchestration.md) offer an Observation for reusable evidence and a Handoff for an actual transfer. Use existing workspace formats when suitable; neither record is required for every task.

## Steps

1. **Establish the delivery boundary.** Load applicable scopes and confirm the outcome, current baseline, constraints, decision authority, delivery target, and any withheld integration or publication action. Use the project's record, integration, and verification conventions.
2. **Plan the dependency graph.** Use [Planning](../planning/planning.md) for task outcomes, dependencies, decisions, and verification. Apply the [Work Records Pattern](../../../../patterns/work-records.md) where no suitable structure exists. Add a project record only when cross-task state needs its own maintained answer.
3. **Assign clear responsibility.** Coordination owns shared decisions, dependencies, and integration. Each task carries one bounded outcome through implementation, correction, and verification. These responsibilities may remain with one person or agent.
4. **Prepare executable assignments.** Resolve shared requirements before dependent work starts. Give each task the accepted outcome, applicable sources, allowed and protected paths, dependencies, completion evidence, and decisions to return to coordination.
5. **Choose safe concurrency.** Use parallel work only when dependencies, authority, and available capabilities allow it. Parallel mutations need distinct responsibilities and isolated worktrees or equivalent accepted isolation. Record the exact starting revision or identifiable state and verify each working location before edits. Otherwise proceed sequentially.
6. **Execute with local continuity.** Use [Development](../development/development.md) within each task. Keep related implementation and repair together. Return discoveries that change shared meaning before changing another task's assumptions; revise only affected plans.
7. **Preserve and resume deliberately.** At useful checkpoints or interruptions, retain changed and untracked work, valid evidence, unresolved choices, and the next action. Before resuming, compare recorded and actual state. Resolve consequential discrepancies before dependent edits. Create a sealed Handoff only when a real transfer or planned resumption needs a fixed snapshot. Clean up resources only after work is retained and cleanup is authorized.
8. **Review integration candidates.** Check each completed task against its outcome. Use [Review](../development/review.md) when required or when fresh scrutiny adds value. Record evidence, limitations, and the exact changes offered for integration. Commit only when authorized and useful.
9. **Integrate and verify.** Integrate in dependency order at the accepted target when authorized. Use a separate integrator only when combining results warrants it. Resolve textual conflicts, inspect combined behavior and shared assumptions, and run checks that expose interactions between tasks.
10. **Report the real delivery state.** Update the sources that define delivered results and remaining work. If integration or acceptance is withheld, leave reviewable changes and name that unfinished boundary. Retain an Observation only when concrete evidence may prevent repeated cost or improve future work; extend a matching record when its scope and meaning agree. Task completion never grants permission to merge, push, or publish.

## Completion

- Responsibilities, dependencies, current state, and starting baselines are clear.
- Interrupted or unfinished work remains recoverable.
- Completed changes have evidence tied to their actual state.
- Authorized integration has passed combined checks, or a reviewable handoff identifies the remaining integration or acceptance decision.
- Project and task records agree with what was delivered and what remains.
