---
title: Demos
description: Try Open Forge on Neon Pairs, the memory game the evals build, from an idea, a short brief, a full spec or an existing app, at any rung of the setup ladder.
---

# Demos

The demos are projects you can give your own agent to see what Open Forge changes, built around Neon Pairs, the memory game that Open Forge Evals builds and grades.

You give your agent the same request twice, once without Open Forge and once with it, then compare the results. For example, give it the one-paragraph Neon Pairs idea in an empty folder, then again in a folder with Essentials installed. The evals run the same comparisons three times per setup and grade every game with hidden checks and model judges, so you can set your runs beside theirs.

## Neon Pairs

Neon Pairs is a browser memory game where you reveal two cards at a time to find every matching pair. It draws its card faces in code, counts moves, keeps your best scores and works with a mouse, touch or keyboard, all from one offline HTML file.

The game comes in two demos:

| Demo                        | Start from              | The agent gets                          |
| --------------------------- | ----------------------- | --------------------------------------- |
| [Greenfield](greenfield.md) | An empty project        | An idea, a short brief or a full spec   |
| [Brownfield](brownfield.md) | The finished Neon Pairs | The game, its spec and a change request |

Every input lives in [`src/evals/benchmarks/neon-pairs/`](../../../evals/benchmarks/neon-pairs/), the same files the evals use.

## Setup ladder \{#setup-ladder}

The setup ladder is a scale for how much setup a project gives an agent, from nothing to a full Open Forge install. Each rung up adds structure around the same documents:

| Rung | Setup            | What the project has                                                                                  |
| ---- | ---------------- | ----------------------------------------------------------------------------------------------------- |
| 0    | No setup         | Nothing but your request                                                                              |
| 1    | Plain docs       | Your documents in `docs/` and a README line that says to follow them                                  |
| 2    | Linked AGENTS.md | Plain docs plus an `AGENTS.md` that links each document with a one-line description                   |
| 3    | Minimal          | Open Forge's loader, Directives and Crystallized Memory, with the documents routed through the loader |
| 4    | Essentials       | Minimal plus Patterns, Skills, Emerging Memory and Working Memory                                     |
| 5    | Full Core        | Every built-in Open Forge category and Memory state                                                   |

Rungs 3 to 5 are the Open Forge presets, and [Installation](../getting-started/installation.md#choose-the-installed-routes) describes what each one installs. You can place any project on the ladder. E.g. a repository whose README points to a `docs/` folder sits on rung 1.

Two setups in the evals aren't rungs. Spec in prompt pastes a short brief into the prompt instead of the project, and Code only leaves the documents out of an existing app. These delivery variants test where the requirements live and what the documents add. [Setups](../evals/setups.mdx) defines all eight setups as the evals build them.

## Results

[Results](../evals/results.mdx) shows how each setup did in the evals, by starting point, model and judge. The full plans run each setup three times, so the results are screening signals. Every Open Forge CLI stays beta, and nothing there shows yet that Open Forge improves results. [Evals](../evals/index.md) explains how runs are graded.

## Earlier demos

The expense splitter demos came before the evals. They were never evaluated, so they have no eval results. [Earlier demos](earlier/index.md) keeps them, with their seed levels and manual checklists.
