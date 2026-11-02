---
title: Earlier demos
description: Two expense splitter demos written before Open Forge Evals, never evaluated, with requests at four seed levels and a manual checklist for the result.
---

# Earlier demos

The earlier demos are two expense splitter projects for trying Open Forge with your own agent, written before Open Forge Evals.

Status: not evaluated. The evals use [Neon Pairs](../index.md) instead, so these demos have no eval results. Each one keeps a manual checklist for the result. They live in the repository's [`demos/`](../../../../../demos/) folder.

| Demo                                          | Start from                  | Try it to see                                                               |
| --------------------------------------------- | --------------------------- | --------------------------------------------------------------------------- |
| [Expense splitter, greenfield](greenfield.md) | An empty folder and an idea | How much an agent gets right from one sentence versus a full set of records |
| [Expense splitter, brownfield](brownfield.md) | A half-built app with tests | Whether an agent keeps rules whose reasons live only in the author's notes  |

Both demos use the same product, a command-line expense splitter, so you can compare building it fresh with changing it later.

## Seed levels

Every request comes at four seed levels. The levels change only how much you tell the agent up front:

| Level | What the agent gets                                                                        |
| ----- | ------------------------------------------------------------------------------------------ |
| 1     | One sentence                                                                               |
| 2     | A short brief: who it's for, what it must do, the constraints                              |
| 3     | The brief plus a table of exact expected results                                           |
| 4     | Open Forge records: Decisions and scenarios, plus a Vision and Architecture for greenfield |

Lower levels test what an agent notices on its own. Higher levels test whether it follows what it's given.

Level 4 uses Decisions from [Planning](../../extensions/planning.md), Vision and Architecture documents from [Project Documents](../../extensions/project-documents.md), and Scenario starters from [Flows and Scenarios](../../extensions/scenarios.md). [Development Toolkit](../../extensions/development-toolkit.md) installs all three, plus Development and its Workflow Support dependency. Each demo links to the copy instructions for its level 4 records.

## Before you start

Set up each run like this:

- Work outside this repository. Run your agent in its own folder, so it sees the demo and not Open Forge's own workspace.
- Commit a starting point before the agent changes anything, so `git diff` shows exactly what happened.
- Run it twice if you can, once with Open Forge and once without. Comparing the two runs shows what Open Forge changes.

Each demo's `checks.md` is a manual scoring sheet.
