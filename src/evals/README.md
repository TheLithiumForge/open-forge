# Open Forge Evals

Open Forge Evals runs one prompt through Claude Code and Codex, with and without Open Forge, and shows how the results compare. It's a local web app: press Run, wait, then play the games blind and read the numbers.

Status: first draft, built on 2026-10-30. Only mock runs have been tested end to end so far.

Every run starts in its own fresh folder with fresh tool settings, so runs can't see each other or your own setup. After the agent finishes, hidden checks build and play its game, a stronger model judges the code, and you compare finished games side by side without knowing which setup made them.

## Requirements

Run every command below from `src/evals`.

- Node 22.18 or later, and npm.
- Docker, for isolated runs. Without it, set isolation to `local`.
- A Claude subscription or API key, and a ChatGPT plan or OpenAI API key, for the tools you test.

## Install

```bash
npm run setup
```

This installs the pinned Claude Code, Codex and Open Forge versions and the browser the checks use.

Sign in once. Claude Code needs a long-lived token in `src/evals/.env.local`:

```bash
npx claude setup-token
```

Put the token it prints in `.env.local` as `CLAUDE_CODE_OAUTH_TOKEN=...`. Codex signs in to a home kept only for evals:

```bash
npm run login:codex
```

API keys work too: set `ANTHROPIC_API_KEY` or `OPENAI_API_KEY` in `.env.local` instead.

## Run

```bash
npm start
```

The app opens at `http://localhost:4318/`. Pick the tools, models and setups, then press Run.

- **Play and compare** shows two finished games, labeled A and B. Play both and pick the better one. Setups stay hidden until you press Reveal.
- **Results** shows one table per tool and model, two charts, and every run's checks, judge answers, questions and screenshots.

Without the UI, `npm run batch` runs the saved config and prints the table.

When the hidden checks change, `npm run recheck -- <batch ID>` runs them again on that batch's saved games, or on one run when you add its ID. A run whose build failed, or every run with `--rebuild`, is installed from its lockfile and built offline again first. The agents and the judge don't run again.

## What a run does

1. Copies the setup's frozen install into a fresh folder, outside any repository.
2. Sends the prompt. A simulated user answers the agent's questions from hidden wishes.
3. Runs the project's build again with no network, and opens the `index.html` it writes.
4. Plays the game with the mouse, a phone's touch and the keyboard, and reads the faces from screenshots.
5. Copies the code without Open Forge's files to a scratch folder, where the judge answers yes or no questions about it.

Runs land in `src/evals/runs/`, one folder per batch. Nothing there is committed.

## Config

`eval.config.json` holds the defaults. Saving in the UI writes `eval.config.local.json`, which replaces them.

| Field           | Meaning                                                                          |
| --------------- | -------------------------------------------------------------------------------- |
| `agents`        | The tools and models under test. A `null` model or effort uses the tool default. |
| `judge`         | A stronger model than the agents, and how many times it scores each run.        |
| `simulatedUser` | The model that answers the agent's questions.                                   |
| `setups`        | Which setups of the suite to run.                                                |
| `isolation`     | `docker` for a container per run, or `local` for a fresh folder only.           |
| `parallelRuns`  | How many runs go at once. Hidden checks still run one at a time.                 |
| `limits`        | Minutes per run, question rounds, and minutes for the checks.                   |

The `mock` tool hands in a finished reference game without calling any model, so you can test the harness for free.

Editing the hidden wishes writes `intent.local.json` and changes the eval's fingerprint, so those results never mix with the standard ones.

## Limits

- Hidden checks take about six minutes a run, most of it playing whole games.
- A check group that fails runs once more, since a busy machine can miss a short flip-back delay. A pass on the second try says so.
- `local` isolation keeps the tools' settings and memory apart, but the agent can still read the rest of your disk.
- Codex reports no dollar cost. Claude Code's cost is its own estimate.

The design behind it is in [Running Evals](../../.agents/memory/crystallized/documents/evals/running-evals.md) and [Eval Judging](../../.agents/memory/crystallized/documents/evals/judging.md).
