---
title: "Greenfield: build Neon Pairs"
sidebar_label: Greenfield
description: Build Neon Pairs in an empty project from an idea, a short brief or a full spec, at any rung of the setup ladder, with the same inputs the evals use.
---

# Greenfield demo

The greenfield demo builds Neon Pairs in an empty project, starting from an idea, a short brief or a full spec.

Each starting point tells the agent more. The idea is one paragraph and leaves most choices to the agent. The full spec states every rule, down to how a board code repeats a deal. For example, give your agent the idea with no setup. Then give it the same prompt with Essentials installed and compare what each run asks and builds.

## Inputs

The inputs are the files the evals use, under `src/evals/benchmarks/neon-pairs/`:

| Starting point                                        | Prompt                                                                                                            | Documents                                                                                                            | Answer bank                                                                                                                                     |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| [An idea](../evals/starting-points.mdx#idea)          | [`cases/greenfield-baseline/prompt.md`](../../../evals/benchmarks/neon-pairs/cases/greenfield-baseline/prompt.md) | None                                                                                                                 | [`cases/greenfield-baseline/intent.json`](../../../evals/benchmarks/neon-pairs/cases/greenfield-baseline/intent.json), five private preferences |
| [A short brief](../evals/starting-points.mdx#brief)   | [`cases/l2-defined-idea/prompt.md`](../../../evals/benchmarks/neon-pairs/cases/l2-defined-idea/prompt.md)         | [`cases/l2-defined-idea/specs/`](../../../evals/benchmarks/neon-pairs/cases/l2-defined-idea/specs/), seven documents | [`cases/l2-defined-idea/intent.json`](../../../evals/benchmarks/neon-pairs/cases/l2-defined-idea/intent.json), three open choices               |
| [A full spec](../evals/starting-points.mdx#full-spec) | [`cases/l3-full-spec/prompt.md`](../../../evals/benchmarks/neon-pairs/cases/l3-full-spec/prompt.md)               | [`specs/`](../../../evals/benchmarks/neon-pairs/specs/), ten documents                                               | Empty                                                                                                                                           |

Each document folder also holds a `manifest.json` with file hashes for the evals. Leave it out of your project.

## Requirements

To run the demo, you need:

- An agent tool, such as Claude Code or Codex.
- Node.js and npm, so the agent can install packages and build the game.
- The Open Forge CLI, for rungs 3 to 5. [Installation](../getting-started/installation.md) covers it.

## Run it

Each step sets up one part of the project the evals build for that rung of the [setup ladder](index.md#setup-ladder):

1. Create an empty folder outside the Open Forge repository, so the agent sees only the demo, and run `git init` in it.
2. For a short brief or a full spec, copy the documents' `.md` files into `docs/`. Then add a `README.md` with this line, which puts the project on rung 1, Plain docs:

   ```md
   Follow the requirements in docs/ for this project.
   ```

3. Add the setup for your rung:
   - Rung 0 or 1 - nothing more.
   - Rung 2 - add an `AGENTS.md` that says to follow the requirements in `docs/` and links each document with a one-line description.
   - Rungs 3 to 5 - install the preset, then route the documents. For an idea, run only the first command.

     ```sh
     open-forge install --preset essentials --automatic
     open-forge route include ./docs --tag LoadNow
     open-forge index ./docs/_docs.md
     ```

     Use `minimal` for rung 3 or `full-core` for rung 5. Add `--dry-run` to a command to preview it first.

4. Commit everything as one starting commit, so `git diff` later shows only what the agent changed.
5. Start your agent in the folder and give it the prompt. For a short brief or a full spec, add this line after the prompt:

   ```text
   Read docs/README.md and follow its linked requirements for this project.
   ```

6. Answer the agent's questions the way the evals' simulated user does. When a question matches an entry in the answer bank, give that answer word for word. Reply "Your call, within the supplied requirements." to everything else, and give no coding advice.
7. Build the game and open the HTML file it produces. From a short brief or a full spec, that's `dist/index.html`.

To try Spec in prompt, skip `docs/` and the pointer line. Paste the short brief's documents after the prompt instead, starting with `README.md` and then in the order it links them.

## What to compare

Compare your runs on the points the evals measure:

- Questions - from an idea, does the agent ask about the starting grid size or the colors? The answer bank helps only when it asks.
- Requirements - from a short brief or a full spec, does the game follow the stated rules, such as a miss closing after 800 active milliseconds?
- Code - does each component keep its logic, markup and styles together, as the prompt asks?
- Cost - how many tokens and minutes did each run take? Your agent tool reports its usage.

## Results

[Results](../evals/results.mdx) shows the same starting points as the evals ran them, graded by hidden checks and judges. The checks run only inside the evals. [Run evals](../evals/run.md) shows how to run them on your machine.
