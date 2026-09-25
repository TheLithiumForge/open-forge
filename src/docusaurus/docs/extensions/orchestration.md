---
title: Task Coordination
description: The Managed Delivery workflow for coordinating related tasks, dependencies, interrupted work, and integration.
---

# Task Coordination

Coordinate related tasks, assign responsibilities, manage dependencies, and verify the combined result, using whatever people, agents, and tools your workspace has.

- **Package ID:** `orchestration` (the display name is Task Coordination)
- **Depends on:** [Development](development.md), [Observations and Handoffs](observations-and-handoffs.md), [Planning](planning.md)
- **Loads at startup:** Nothing. The recipe loads when the `use-workflow` Skill selects it.

## Why it exists

Running several related tasks, or several agents at once, is a bigger commitment to one way of working than anything else in the catalogue. That's why it's a separate choice and not part of the Development Toolkit.

Its value is in the things that go wrong at scale: unclear ownership, parallel changes that collide, and interrupted work nobody can resume. Its dependencies bring the methods it calls and the records it uses.

## What it installs

```text
.agents/skills/use-workflow/references/orchestration/
  _orchestration.md      <- recipe scope entrypoint
  managed-delivery.md    <- the coordination workflow
```

Its dependencies install the methods it calls (Planning, Development) and the records it uses (Observations and Handoffs, Checkpoints).

## What each file is for

### `managed-delivery.md`

**Kind:** workflow recipe. **Used when:** dependencies or several contributors make coordination worth it. Independent small work can use [Development](development.md) directly.

**Goal:** deliver related tasks with clear responsibilities, resumable state, and verified integration, within your authority.

| Step                                | What happens                                                                                                                     |
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| 1. Establish the delivery boundary  | Outcome, baseline, constraints, who decides, the delivery target, and any integration or publication you're withholding.         |
| 2. Plan the dependency graph        | Uses the [Planning](planning.md) recipe and the Work Records Pattern.                                                            |
| 3. Assign clear responsibility      | Coordination owns shared decisions and integration. Each task owns one bounded outcome. One person or agent may hold every role. |
| 4. Prepare executable assignments   | Each task gets its outcome, sources, allowed and protected paths, dependencies, and completion evidence.                         |
| 5. Choose safe concurrency          | Parallel work only when dependencies, authority, and isolation (such as separate worktrees) allow it. Otherwise sequential.      |
| 6. Execute with local continuity    | Uses the [Development](development.md) recipe within each task.                                                                  |
| 7. Preserve and resume deliberately | Keeps interrupted work recoverable. Seals a Handoff only for a real transfer.                                                    |
| 8. Review integration candidates    | Uses the Review recipe when fresh scrutiny adds value. Commits only when authorized.                                             |
| 9. Integrate and verify             | In dependency order, with checks that expose interactions between tasks.                                                         |
| 10. Report the real delivery state  | Updates the defining sources, and names any unfinished integration or acceptance boundary.                                       |

It completes when responsibilities and state are clear, unfinished work is recoverable, completed changes have evidence, and records agree with what was delivered.

## How to use it

> Use Managed Delivery for these related tasks. Keep responsibilities clear, work sequentially unless concurrency is justified, and verify the combined result.

## Good to know

- The package installs **no** model roster, hierarchy, scheduler, or tool permissions.
- Installation grants no permission to commit, integrate, publish, or contact external systems. Task completion never grants permission to merge, push, or publish.
- If you only want Observation and Handoff records, install [Observations and Handoffs](observations-and-handoffs.md) alone.
- Task Coordination doesn't install Project Documents or Flows and Scenarios, and isn't part of the Development Toolkit.
