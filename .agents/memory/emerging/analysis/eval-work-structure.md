---
open-forge:
  description: Proposed eval vocabulary from the maintainer's 2026-11-03 direction, with a task pool, work packets, work modes and scenarios replacing the L4-1, L4-2 and L4-3 scenarios, waiting for the maintainer's approval
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Terminology, Brownfield, Orchestration, Proposal]
---

# Eval Work Structure

## How should eval tasks, the ways of doing them and the project contexts be named and combined?

The maintainer's direction on 2026-11-03: the evals hold "a pool of tasks and how to do the tasks". Task IDs should read as a letter for the kind, a number and a name. The L4-1, L4-2 and L4-3 scenarios collapse into one work packet done three ways, filtered by a toggle on the Results page. What the maintainer calls a scenario is the project context: plain docs, organized docs, Open Forge, Open Forge scoped, Open Forge with Extensions. This replaces the scenario tables in [eval-program-plan.md](eval-program-plan.md) once approved.

## Terms

| Term | Meaning | Today's name |
| --- | --- | --- |
| Layer | How much the agent gets before it starts: an idea, a short brief, a full spec or an existing app | Layer, starting point |
| Task | One change request from the pool, with its own checks | Case |
| Work packet | Tasks handed over together, with the union of their checks | The combined case |
| Work mode | How a packet is done | L4-1, L4-2, L4-3 |
| Scenario | The project context the agent starts with | Setup |
| Implementer | The model, its reasoning level and, in orchestrated mode, its subagent model | Implementer |
| Run | One packet or task, in one scenario and work mode, by one implementer | Run |
| Stage | Work, then checks, then judges, each run whenever chosen | `run`, `check`, `judge` |

## Task pool

| ID | Name | Request | Spec scope | Today |
| --- | --- | --- | --- | --- |
| B1 | best-score | A worse game replaces the best score | Gameplay | l4-r02 |
| B2 | arrow-keys | The arrow keys stopped moving between cards | Controls | l4-r03 |
| B3 | reduced-motion | Animations ignore reduced motion | Visuals | l4-r04 |
| B4 | silence | The game beeps on each pair | Sound | l4-r05 |
| B5 | pasted-saves | Pasted saved games won't open | Sharing | l4-r01 |
| F1 | name-saves | Name a game before saving it | Sharing | l4-f01 |

B is a bug fix and F a feature. Folders become `b1-best-score` and so on. The combined cases l4-r06 and l4-fr01 become packets, generated from their tasks' requests and checks instead of written by hand.

| Packet | Tasks |
| --- | --- |
| P1 | B1, B2, B3, B4 |
| P2 | F1, B5 |

## Work modes

| Mode | What the agent gets | Replaces |
| --- | --- | --- |
| Whole packet | One agent, one chat, every task in one request | L4-3 solo |
| Task by task | A new agent per task, each starting from the previous task's result | L4-2 |
| Orchestrated | One orchestrator for the packet, handing each task to a subagent | L4-3 teams |

L4-1, each task alone from a clean start, drops out. Task by task starts its first task from a clean start, and the four L4-1 batches of 2026-11-03 stay as history. Layer 3 fits the same modes: the full build in one request is a whole packet, and the three milestones are task by task.

## Scenarios

The project contexts, from nothing to progressive disclosure: Code only, Plain docs, Organized docs, Overloaded docs, Full Core, Full Core scoped and Full Core scoped with memory. Later ones add Extensions one at a time. Docs and the site say scenario. Plan files and records keep the `setups` key until the next records migration, so the published setup ladder in the Demos keeps its meaning.

## Results page

Rows are scenarios. Toggles pick the layer, packet or task, work mode and implementer. Columns are the measures: check coverage, tokens, tool calls and time.

## What the first L4-1 data says about the design

- Every checked run of the four small bug fixes so far reached 99 or 100% coverage, in every scenario, and the earlier P1 smoke reached 94 to 98%. Quality doesn't separate scenarios on tasks this small.
- Tokens for the same cell differ by up to half between two runs. One best-score run with Organized docs read 4.5M cached tokens and its replacement 6.8M. One run per cell can't separate scenarios whose means are that close.
- So a measured difference needs either three or more runs per cell, or a packet large enough that the context decides whether the work succeeds. P1 in whole packet and orchestrated modes is the cheapest next step that does both.

## Migration

1. Rename task folders, IDs, plans and records, and validate every task again.
2. Packets as files listing their tasks, with a generated request and checks.
3. A `workMode` plan field running a packet whole, task by task in one batch or orchestrated, replacing chain milestones and `--parent`.
4. The Results toggles, and the docs renamed to the terms above.
5. The audit of the game's own tests, so agents run only useful, non-duplicate tests.
