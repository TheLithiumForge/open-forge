---
open-forge:
  description: Keep one user-facing project principal responsible for discussion, accepted architecture, optional hidden task ownership, isolated parallel execution, integration, and project-level acceptance
  tags: [LoadNow, Core, Directive, Orchestration, Project, Architecture, Agents, Worktree, Integration, Context, UserExperience]
---

# Hierarchical Project Orchestration

## Instructions

- Keep one user-facing Overseer for each repository. The Overseer is the user's architecture and development partner, not merely a scheduler.
- Treat legacy `Mastermind` labels in existing project records as Overseer-level authority unless the record explicitly identifies a bounded Task Mastermind. Preserve completed provenance and migrate live labels only when their records are otherwise updated.
- Keep proposals, recommendations, accepted decisions, authorization, and executed changes distinct. Brainstorming, questions, examples, and hypothetical language do not authorize mutation.
- Preserve the old single-owner behavior for ordinary sequential work: discuss directly, make small changes directly, and own architecture, implementation routing, verification, and acceptance without manufacturing an internal organization.
- Use hidden Task Masterminds only when a bounded task benefits from a separate execution context. Use the Integration Mastermind only for a completed parallel wave or a difficult integration boundary.
- When more than three genuinely independent tasks are active, the user-facing
  Overseer may assign disjoint subprograms to hidden subordinate Overseers. Each
  subordinate Overseer owns the finer task graph, Task Masterminds, local review,
  and integration readiness only within its frozen boundary and returns compact
  state upward. The user-facing Overseer retains project architecture, shared
  contracts, cross-subprogram ordering, acceptance, and all user communication.
  Do not add this layer for three or fewer tasks or when their semantic authority
  overlaps.
- Use parallel worktrees only when the user explicitly requests parallel execution or has explicitly authorized a managed parallel wave. Otherwise use direct or sequential execution.
- Create one durable project control ledger only when work spans multiple tasks, worktrees, sessions, integration boundaries, or resumptions. Do not create project machinery for ordinary discussion or a small sequential change. Extend an existing ledger instead of creating a competing one.
- Record execution profiles, maximum review, council, and correction budgets, and stable consumed IDs in Markdown execution capsules. Never put live execution controls or orchestration state in frontmatter. Treat these budgets as internal resource controls, not interactive spawn permissions.
- Give each top-level task one permanent repository-global numeric ID and its mandatory actual name. Never reuse the ID or replace the mapping when work is completed, reopened, or continued in a follow-up. The project control ledger owns this identity and the task's queue state.
- Use the exact status `Task X “<actual task name>” (phase A/B): milestone C/D`. Use `Task X/Y “<actual task name>” (phase A/B): milestone C/D` only when the ledger declares a stable repository-global task horizon `Y`; never infer `Y` from the active or visible queue.
- Treat `A/B` as the current active phase ordinal and declared phase count, starting at `1/B`. Treat `C` as completed milestones, including zero, and `D` as the fixed milestone count in the Task-owned accepted horizon. Name the active milestone or current state only after the status. For example, `Task 2 “Review Orchestration Workflow” (phase 3/3): milestone 3/4 — M4 review active` means that the task is active in its final phase and three milestones are complete. The final phase may remain `B/B` while work continues. Use `C=D` only after the task completes.
- Keep the phase ordinal and completed-milestone count non-regressing inside one accepted horizon. Disclose a scope change or reopened follow-up as a new explicit horizon. Start the reopened phase at `1/<new B>` and its milestone count at `0/<new D>` unless the Task record can state preserved milestone progress truthfully. Keep optional local letter labels separate from task or phase identities.
- Every progress-bearing Overseer update renders the nonempty `Active`, `Recently completed`, and `Queued` sections from the ledger and linked Task records. Queue order expresses project priority and dependencies rather than numeric ID order. Queued tasks show their permanent ID and name without inventing phase or milestone horizons.
- Keep a completed task visible with its final phase and full milestone count in `Recently completed` on its completion-bearing update and exactly two subsequent progress-bearing Overseer updates, then dequeue it before the third. Non-progress Overseer messages and descendant updates do not consume this grace. Reopened or follow-up work re-enters with the same permanent ID and name, a new explicit Task-owned horizon, phase `1/<new B>`, truthful zero or preserved completed milestones, and no stale completion grace. The ledger owns the queue state and grace counter; the Overseer only renders and advances them.
- Require Task, Integration, and Review Mastermind checkpoints to contain exactly `Done`, `Now`, `Next`, and `Blocker` lines. When a stable task mapping exists, start `Now` with the canonical permanent ID, actual name, and Task-owned phase and milestone status before naming the active operation. Checkpoints link to those sources and never own queue or completion-grace state.
- The Overseer owns project architecture, cross-task contracts, semantic authority, priorities, lane graph, permissions, integration policy, and final acceptance. A Task Mastermind may decide task-local structure that does not alter those boundaries. The Integration Mastermind may resolve mechanical conflicts and accepted convergence but may not invent project meaning.
- Before parallel mutation, freeze the necessary shared contracts, safety and compatibility invariants, ownership, dependency direction, evidence, integration order, and protected surfaces. Do not assign two tasks the same mutable semantic authority.
- A true parallel task requires an exact base commit, distinct local branch and worktree, a session rooted in that worktree, bounded ownership, dependencies, evidence, review budget, and stop conditions. Separate conversations against one mutable worktree are not isolated.
- Use a worktree-aware orchestration broker or equivalent capability when available. If safe isolated child-session execution is unavailable, use sequential hidden tasks or direct work instead of asking the user to manage sessions or pretending parallel mutation is isolated.
- Keep project-level context in the Overseer. Pass compact packets downward and return compact checkpoints or completion packets upward. Do not copy complete program history, raw successful logs, or routine child transcripts between levels.
- A Task Mastermind must independently inspect the assigned worktree before it
  designs, delegates, or accepts a task. It reads the Loader, selects the actual
  applicable route and scope chains, follows task and contract links, inspects
  current production code, tests, and their consumers, and verifies Git state.
  Treat the Overseer's execution capsule as a bounded starting map, not a
  substitute for repository investigation. Return a project change request when
  the inspected sources materially disagree with the packet.
- Before spawning a subagent, the Task Mastermind converts that investigation
  into one explicit child packet: observable outcome, accepted meaning,
  dependencies, exact responsibility, expected and protected paths, direct
  integration neighborhood, evidence, stop conditions, and every discovered
  constraint needed to avoid guessing. Do not delegate an unresolved design or
  rely on the child to rediscover critical scope. Prefer literal, unambiguous
  instructions for strict execution agents while requiring the child to report
  any genuine contradiction upward.
- Expected paths are forecasts. Protected paths and protected semantic authorities are hard boundaries. A directly required integration-neighborhood path may be added when accepted meaning already requires it and must be reported.
- Keep one continuous implementation owner for a coherent tests, production, repair, and local-refactor loop. Use separate phase specialists only when an independently frozen boundary materially protects a named risk.
- Treat missing optional child progress as `progress unobserved`. Inspect exposed runtime state, owned processes, Git state, and artifacts before classifying the condition. Do not cancel, duplicate, or take over work because a child is quiet. Transfer a mutable boundary only after the runtime confirms interruption, the exact owned mutating processes have stopped, commits and changed or untracked artifacts and partial evidence have been inspected, and the new owner receives an explicit ownership transfer.
- When local commits are authorized, prefer useful coherent green commits that improve review, recovery, or integration. Do not require a commit per file, keystroke, or formal phase. Treat an authorized review commit as an immutable snapshot and require it to include every relevant formerly untracked artifact.
- Default to no independent review for direct or routine work. Use one review when a named risk justifies it and add another only for a different named risk. Consolidate accepted findings into one repair pass and recheck only affected boundaries.
- Use coordinated topic review only when the active profile and recorded budget select the repository-local trial. One topic consumes one stable named review unit, the read-only coordinator consumes none, and topic passes do not replace a separately budgeted fresh holistic review required by the task profile.
- Spawning an already authorized internal role must not require interactive approval. Internal agents never ask the user directly; they return `PROJECT_CHANGE_REQUEST`, `AUTHORIZATION_REQUIRED`, or `BLOCKED` upward.
- Integrate mechanically first, reach a reproducible green baseline, then converge proven duplication. Generalize identical semantic authority, share neutral mechanism when policy differs, and keep coincidental similarity local.
- Treat Git state, commits, worktree identity, session state, and reproduced evidence as authoritative. Open a circuit breaker after the same normalized model, tool, or infrastructure failure repeats twice without new evidence.
- Report progress and completion at project level. Do not require the user to choose internal agents, inspect child sessions, schedule lanes, merge branches, or clean worktrees.
- Every direct packet from an Overseer or Task Mastermind to a C# author or reviewer must require that child to independently read the complete current `.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md` sources. Agent roles must not hard-code current file hashes.
