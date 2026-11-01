---
open-forge:
  description: The maintainer's eval outline from 2026-10-30, from a prompt-only greenfield baseline up to brownfield, each case checking that Open Forge is at least as good as the default
  tags: [Memory, Idea, Contextual, Candidate, Evaluation, Greenfield, Brownfield, Baseline, Demo]
---

# Eval Ladder

The Eval Ladder is the maintainer's outline for Open Forge evals. It starts from the barest case and adds context one step at a time.

The maintainer stated it in chat on 2026-10-30, after setting the earlier plans aside. It isn't accepted yet. [Twelve-Eval Catalogue](eval-catalogue.md) keeps the overseer's earlier plan, so we can compare the two later. [Task 58](../../working/cli-development/tasks/task58-demo-evals.md) tracks the work.

## Goal

Each case shows that using Open Forge is at least as good as the default case. The first case is a new small project with no context at all.

## Greenfield cases

Each greenfield case adds more specification and context:

1. Baseline - one generic prompt that states the user's goal, and maybe the technologies. We compare no setup with an Open Forge install. We can run every preset (Minimal, Essentials, Full Core) to check whether the preset changes how the agent works. A council picks the prompt so it reads the way a user would write it.
2. Some specs - we know what we want and wrote some specs: the technology, a way of doing things. The same specs go in three places: plain documents, `AGENTS.md`, or Open Forge. The context is still small.
3. Full specs - specs for every part. We compare `AGENTS.md` plus links, all specs in a `docs` folder, and Open Forge with proper scopes.

[Baseline Lessons](baseline-lessons.md) lists what the first real Baseline batch taught, as specs for cases 2 and 3.

## Brownfield case

We give an existing codebase, built later for this purpose and never changed between runs. The task adds something new to it. We compare:

- The code only.
- The code with an `AGENTS.md` holding instructions.
- The code with an `AGENTS.md` or another layout that puts the specs in order.
- The code with Open Forge.

Respect for the project's code conventions, when they're declared as instructions, is a data point here. It can also become its own eval.

## Data points

The maintainer named these:

- Context used per task.
- Context compactions per task.
- Tokens per task, especially when runs did the task the same way.
- Context degradation, if we find a way to measure it.
- Quality of the project or the added feature.
- Results of gimmicks that ask for optimized builds, such as lower cost, fewer tokens or better performance, because they give values we can chart.

Some of these need a person to judge them, for example from screenshots.

## Baseline additions

The maintainer added these on 2026-10-30, after the first Baseline draft:

- The project is Neon Pairs, a neon memory game with an adjustable grid from 4x4 to 16x16 and card faces drawn by the code.
- It's built with React and TypeScript, with a small build step that gives one `index.html`.
- The prompt carries one of the maintainer's rules, locality of behavior, so every eval can score how well rules are followed.
- A third kind of setup adds the Collaboration extension, whose Guidance helps the agent find out what the user wants, to see whether that's useful.
- A thousand runs per setup isn't realistic. A few runs per setup on the same model, each isolated so runs can't contaminate each other, should still show whether it works.
- The checks avoid randomness and timing problems.
- We still need to find the best way to run the evals and decide who judges what, in which situations.

## Next step

Run the first real Greenfield Baseline batch with the [eval tool](../../../../src/evals/README.md). Then write the next greenfield evals.

## Not decided yet

- Whether there's a case after brownfield.
- Whether respect for code conventions is a data point or its own eval.
