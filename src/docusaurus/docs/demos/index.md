---
title: Demos
description: Small, realistic projects for trying Open Forge with your own agent, with requests at four levels of detail and checks for the result.
---

# Demos

The quickest way to see what Open Forge changes is to run the same request with and without it. The demos are small, realistic projects with requests at four levels of detail and a checklist for the result. They live in the repository's [`demos/`](../../../../demos/) folder.

| Demo                                               | Start from                  | Try it to see                                                               |
| -------------------------------------------------- | --------------------------- | --------------------------------------------------------------------------- |
| [Greenfield: build it from an idea](greenfield.md) | An empty folder and an idea | How much an agent gets right from one sentence versus a full set of records |
| [Brownfield: add a feature](brownfield.md)         | A half-built app with tests | Whether an agent keeps a rule that lives only in the author's notes         |

Both demos use the same product, a command-line expense splitter, so you can compare building it fresh with changing it later.

## Seed levels

Every request comes at four levels. The levels change only how much you tell the agent up front.

| Level | What the agent gets                                                                        |
| ----- | ------------------------------------------------------------------------------------------ |
| 1     | One sentence                                                                               |
| 2     | A short brief: who it's for, what it must do, the constraints                              |
| 3     | The brief plus a table of exact expected results                                           |
| 4     | Open Forge records: Decisions and scenarios, plus a Vision and Architecture for greenfield |

Lower levels test what an agent notices on its own. Higher levels test whether it follows what it's given.

## Before you start

- **Copy the demo out of this repository.** Run your agent in its own folder, so it sees the demo and not Open Forge's own workspace.
- **Commit a starting point** before the agent changes anything, so `git diff` shows exactly what happened.
- **Run it twice if you can:** once with Open Forge and once without. The differences are the point.

## Demos as evaluations

The same fixed inputs can compare agents, models, and spec-driven tools. For now, each demo's `checks.md` is a manual scoring sheet. Turning the checks into a repeatable evaluation is planned work.
