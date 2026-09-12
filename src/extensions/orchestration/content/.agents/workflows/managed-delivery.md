---
open-forge:
  description: Coordinate dependent tasks, preserve interrupted work, and verify authorized integration
  tags: [Extension, Workflow, Orchestration, Planning, Worktree, Integration]
---

# Managed Delivery

## Goal

Deliver related tasks with clear responsibilities, resumable state, and verified integration within the user's authority.

Use this recipe when dependencies or multiple contributors make coordination useful. One person or agent may perform every responsibility. Small independent work can use [Development](development.md) directly.

## Steps

1. Establish the accepted outcome, current baseline, constraints, decision authority, and delivery target from the project's current sources and applicable rules. Use its conventions for task records, integration, and verification. Carry existing authorization forward and identify any withheld integration or publication action.
2. Use [Planning](planning.md) to define task outcomes, dependencies, and verification. Keep one source for each task's current state using the [Work Records Pattern](../patterns/work-records.md). Add a project record only when cross-task state needs its own home.
3. Assign responsibility at two scopes. Project coordination resolves shared decisions, dependencies, and integration. Each task carries one bounded outcome through implementation, correction, and verification. These responsibilities may stay with the same person or agent.
4. Settle shared requirements before dependent work starts. Each task receives its outcome, relevant accepted sources, expected and protected paths, dependencies, completion evidence, and conditions requiring a project decision.
5. Choose sequential or parallel execution based on the dependencies, authorization, and available capabilities. Parallel changes require distinct responsibilities and isolated worktrees or equivalent accepted isolation.

   For each task, record its exact starting revision, or an equally identifiable starting state when revision control does not apply, and verify its working location before editing. If isolation or delegation is unavailable, proceed sequentially.

6. Follow [Development](development.md) within each task. Keep related implementation and repair together. Return discoveries that change shared meaning before changing another task's assumptions. Revise only the affected plans.

7. At useful checkpoints or interruptions, preserve changed and untracked work, evidence, unresolved decisions, and the next action.

   Before resuming, verify the recorded baseline and the actual working state. Resolve discrepancies before making dependent edits. Retire temporary resources only after their work has been safely retained and cleanup is authorized.

8. Inspect each completed task against its outcome. Use [Review](review.md) when independent scrutiny adds value or is required. Record evidence, limitations, and the exact changes offered for integration. Commit only when authorized and useful.
9. Integrate in dependency order at the accepted target when authorized. The project coordinator may do this directly. Assign a separate integration responsibility only when combining results warrants it. Resolve textual conflicts and then inspect the combined behavior and shared assumptions. Run checks that can expose interactions between the tasks.
10. Record the delivered result and remaining work in their defining sources. When integration or acceptance is withheld, leave the changes reviewable and label delivery incomplete at that boundary. Report the evidence and the next decision without treating task completion as permission to merge, push, or publish.

## Completion

- Task responsibilities, dependencies, current state, and the accepted baseline are clear.
- Interrupted or unfinished work remains recoverable.
- Completed changes have evidence tied to their actual state.
- Authorized integration has passed the relevant combined checks, or a reviewable handoff identifies the remaining integration or acceptance decision.
- Project and task records agree with what was delivered and what remains.
