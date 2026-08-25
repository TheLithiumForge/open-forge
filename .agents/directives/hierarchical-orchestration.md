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
- Use parallel worktrees only when the user explicitly requests parallel execution or has explicitly authorized a managed parallel wave. Otherwise use direct or sequential execution.
- Create one durable project control ledger only when work spans multiple tasks, worktrees, sessions, integration boundaries, or resumptions. Do not create project machinery for ordinary discussion or a small sequential change. Extend an existing ledger instead of creating a competing one.
- Record execution profiles, maximum review, council, and correction budgets, and stable consumed IDs in Markdown execution capsules. Never put live execution controls or orchestration state in frontmatter. Treat these budgets as internal resource controls, not interactive spawn permissions.
- The Overseer owns project architecture, cross-task contracts, semantic authority, priorities, lane graph, permissions, integration policy, and final acceptance. A Task Mastermind may decide task-local structure that does not alter those boundaries. The Integration Mastermind may resolve mechanical conflicts and accepted convergence but may not invent project meaning.
- Before parallel mutation, freeze the necessary shared contracts, safety and compatibility invariants, ownership, dependency direction, evidence, integration order, and protected surfaces. Do not assign two tasks the same mutable semantic authority.
- A true parallel task requires an exact base commit, distinct local branch and worktree, a session rooted in that worktree, bounded ownership, dependencies, evidence, review budget, and stop conditions. Separate conversations against one mutable worktree are not isolated.
- Use a worktree-aware orchestration broker or equivalent capability when available. If safe isolated child-session execution is unavailable, use sequential hidden tasks or direct work instead of asking the user to manage sessions or pretending parallel mutation is isolated.
- Keep project-level context in the Overseer. Pass compact packets downward and return compact checkpoints or completion packets upward. Do not copy complete program history, raw successful logs, or routine child transcripts between levels.
- Expected paths are forecasts. Protected paths and protected semantic authorities are hard boundaries. A directly required integration-neighborhood path may be added when accepted meaning already requires it and must be reported.
- Keep one continuous implementation owner for a coherent tests, production, repair, and local-refactor loop. Use separate phase specialists only when an independently frozen boundary materially protects a named risk.
- Default to no independent review for direct or routine work. Use one review when a named risk justifies it and add another only for a different named risk. Consolidate accepted findings into one repair pass and recheck only affected boundaries.
- Spawning an already authorized internal role must not require interactive approval. Internal agents never ask the user directly; they return `PROJECT_CHANGE_REQUEST`, `AUTHORIZATION_REQUIRED`, or `BLOCKED` upward.
- Integrate mechanically first, reach a reproducible green baseline, then converge proven duplication. Generalize identical semantic authority, share neutral mechanism when policy differs, and keep coincidental similarity local.
- Treat Git state, commits, worktree identity, session state, and reproduced evidence as authoritative. Open a circuit breaker after the same normalized model, tool, or infrastructure failure repeats twice without new evidence.
- Report progress and completion at project level. Do not require the user to choose internal agents, inspect child sessions, schedule lanes, merge branches, or clean worktrees.
