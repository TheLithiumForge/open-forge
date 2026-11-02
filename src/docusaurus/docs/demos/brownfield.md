---
title: "Brownfield: change Neon Pairs"
sidebar_label: Brownfield
description: Ask an agent to add a feature to the finished Neon Pairs game, repair a planted bug or both, at any rung of the setup ladder, with the same inputs the evals use.
---

# Brownfield demo

The brownfield demo asks an agent to change a finished Neon Pairs game: add a feature, repair a planted bug or both.

Every run starts from the reviewed reference game, built to the full spec with its own tests. The request sounds like a player asking for a change, and the rules it touches are in the spec. For example, the feature case asks for a name you can give a saved game, and the spec's update says which snapshot version a named game uses. A careful agent finds that rule before it changes the save format.

## Inputs

The inputs are the files the evals use, under `src/evals/benchmarks/neon-pairs/`:

- [`reference/`](../../../evals/benchmarks/neon-pairs/reference/) - the reference game, with its source, tests and README.
- [`specs/`](../../../evals/benchmarks/neon-pairs/specs/) - the full spec, ten documents.
- `cases/<case>/` - each case's request, answer bank and changes to the starting files.

The three cases are:

| Case                                                                  | Request                                                                       | Starting code                   | Spec                                     |
| --------------------------------------------------------------------- | ----------------------------------------------------------------------------- | ------------------------------- | ---------------------------------------- |
| [Feature, `l4-f01`](../evals/starting-points.mdx#l4-f01)              | [`request.md`](../../../evals/benchmarks/neon-pairs/cases/l4-f01/request.md)  | The reference                   | The spec with the files in `spec-delta/` |
| [Repair, `l4-r01`](../evals/starting-points.mdx#l4-r01)               | [`request.md`](../../../evals/benchmarks/neon-pairs/cases/l4-r01/request.md)  | The reference with `seed.patch` | The spec                                 |
| [Feature and repair, `l4-fr01`](../evals/starting-points.mdx#l4-fr01) | [`request.md`](../../../evals/benchmarks/neon-pairs/cases/l4-fr01/request.md) | The reference with `seed.patch` | The spec with the files in `spec-delta/` |

Each case folder also holds the known solutions and the evals' change checks. Keep them away from the agent.

## Requirements

To run the demo, you need:

- An agent tool, such as Claude Code or Codex.
- Node.js and npm, to install the game's packages and run its tests.
- The Open Forge CLI, for rungs 3 to 5. [Installation](../getting-started/installation.md) covers it.

## Run it

Each step sets up one part of the project the evals build for that rung of the [setup ladder](index.md#setup-ladder):

1. Copy `reference/` into a new folder outside the Open Forge repository, and run `git init` in it. Copy only the files Git tracks, e.g. from a fresh clone, so installed packages and build output stay out.
2. For a repair or both, apply the case's `seed.patch` in that folder. It plants the bug:

   ```sh
   git apply <path-to-open-forge>/src/evals/benchmarks/neon-pairs/cases/l4-r01/seed.patch
   ```

3. From rung 1 up, copy the spec's `.md` files into `docs/`. For a feature or both, replace them with the files in the case's `spec-delta/` folder. Then add this line to the end of `README.md`:

   ```md
   Follow the requirements in docs/ for this project.
   ```

4. Add the setup for your rung, as step 3 of the [Greenfield demo](greenfield.md#run-it) describes.
5. Commit everything as one starting commit. With one commit, the history can't reveal the planted bug, as in the evals.
6. Start your agent in the folder and give it the case's request. From rung 1 up, add this line after it:

   ```text
   Read docs/README.md and follow its linked requirements for this project.
   ```

7. Answer the agent's questions the way the evals' simulated user does. For a repair or both, `answers.json` holds one answer, a saved game that fails to open, for when the agent asks for an example. Reply "Your call, within the supplied requirements." to everything else, and give no coding advice.
8. Run the game's checks from its README, then try the change in the game.

To try Code only, skip `docs/`, the README line and the pointer line, and give the request alone.

## What to compare

Compare your runs on the points the evals measure:

- Rules - for the feature, does the agent read the spec's update and keep unnamed games at snapshot version 1?
- Repair - does it fix the cause, or only the example the simulated user gave?
- Scope - does the change stay within the parts the request needs, or does it rewrite unrelated code?
- Regressions - do the game's existing tests still pass?
- Cost - how many tokens and minutes did each run take?

## Results

[Results](../evals/results.mdx) shows the three cases as the evals ran them, graded by the change checks, the full spec's checks as regressions and two guards. The checks run only inside the evals. [Run evals](../evals/run.md) shows how to run them on your machine.
