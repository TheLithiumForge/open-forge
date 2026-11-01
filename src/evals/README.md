# Open Forge Evals
Open Forge Evals is a local tool that runs the same case through Claude Code and Codex under different Open Forge setups and saves comparable results.

For example, the Layer 1 plan runs Neon Pairs five times on each of No setup, Minimal, Essentials and Full Core. Every attempt starts with one prompt and gets literal answers from the same frozen answer bank when it asks questions.

## Requirements

Run the commands below from the repository root. You need these tools:

- Node 24 or later and npm.
- Docker for model attempts and verifier builds.
- A sign-in or API key for each model provider you choose.

The mock smoke command needs Node and the eval dependencies. It uses mock verification, browser checks and judging, so you can check the run protocol and saved records without Docker or models.

## Install

Install the pinned dependencies:

```powershell
npm ci --prefix src/evals
```

For real browser checks, install the pinned browser:

```powershell
npm exec --prefix src/evals -- playwright install chromium
```

Claude Code reads `CLAUDE_CODE_OAUTH_TOKEN` or `ANTHROPIC_API_KEY` from `src/evals/.env.local`. To get a subscription token, run the pinned tool's `setup-token` command through npm:

```powershell
npm exec --prefix src/evals -- claude setup-token
```

Codex keeps its sign-in in an eval-only home:

```powershell
npm --prefix src/evals run login:codex
```

`OPENAI_API_KEY` and `CODEX_API_KEY` also work in `src/evals/.env.local`. Sign-in values stay out of command arguments. Only the chosen provider's sign-in enters each model container. A refreshed Codex sign-in goes back to the eval-only home.

## Quickstart

Run the mock attempt:

```powershell
npm run evals -- smoke
```

It prints `Smoke passed` after the conversation, mock build, mock checks and mock judge finish. It checks the saved v2 run and assessments, then removes its temporary records. It runs no project code on this machine.

Before a model run, fill both base-image digests in `src/evals/images/lock.json`. Then check the prerequisites, build the images and check the setups:

```powershell
npm run evals -- doctor
npm run evals -- images
npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json
npm run evals -- run --plan src/evals/experiments/neon-pairs-l1.json
```

`doctor` checks Node, Docker, installed tool package versions, configured sign-ins and image digests. It doesn't perform a model sign-in or a paid probe. `images` refuses to build while a digest is missing. `qualify` installs fresh setups with the pinned Open Forge CLI and checks entry files, the framework lock, planned files and unwanted files before the scored sample starts.

## Commands

Every command accepts `--help`. Each help text includes an example:

| Command | Job | Example |
| --- | --- | --- |
| `doctor` | Check prerequisites | `npm run evals -- doctor` |
| `images` | Build the three locked images | `npm run evals -- images` |
| `smoke` | Run the mock attempt | `npm run evals -- smoke` |
| `qualify` | Check generated setups | `npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json` |
| `run` | Run a saved plan | `npm run evals -- run --plan src/evals/experiments/neon-pairs-l1.json` |
| `ui` | Open the local results app | `npm run evals -- ui` |
| `recheck` | Append checks for saved files | `npm run evals -- recheck <batch> --rebuild` |
| `rejudge` | Append judge ballots | `npm run evals -- rejudge <batch>` |
| `export` | Write sanitized finished records | `npm run evals -- export <batch> --out ./eval-export` |

`recheck` and `rejudge` accept an optional attempt after the batch. `--rebuild` runs the saved project through the verifier again. Checks and ballots append assessments instead of replacing earlier grades. `export` writes a new immutable pack and refuses an existing target.

## Protocol

Every case follows this sequence:

1. Copy frozen inputs into a fresh agent container and tool home.
2. Send the case prompt once to start the coding session.
3. Classify questions in that session with the fixed classifier prompt, in a fresh assessor container for each call.
4. Return only the bank's literal answers for the matching topic IDs.
5. Freeze the files when the agent finishes or reaches its registered limit.
6. Install dependencies without lifecycle scripts in a fresh verifier, disconnect its network and build as a separate non-root account.
7. Copy plain output files back for host browser checks, then save checks and judge assessments.

The classifier splits bundled decisions into separate questions. The runner allows three rounds of bank answers. A later question gets exactly "Your call, within the supplied requirements." and is recorded as unanswered. After sending that terminal fallback, the runner waits for one final agent reply and stops. Uncovered choices get the same fallback. The simulator gives no coding advice and volunteers nothing.

The batch saves a copy and hash of its bank before any attempt. For Layer 1, each intent item's ID is its topic ID and its text is its literal answer. `conversation.json` records individual questions, rounds, repeats, decisions already answered by public documents, answer lengths and the classifier's separate usage. A classifier failure is an infrastructure error. It never counts as a message with no question.

Claude assessor calls turn model tools off. Codex assessor calls use an empty working folder and a read-only sandbox, with all evidence in the prompt. Their tools are recorded as restricted. Containers use Docker copy, with no host mounts, Docker socket or published ports. The agent never receives grading. Full rules are in the [Protocol](../../.agents/memory/crystallized/documents/evals/protocol.md).

## Plans

The saved plan is [neon-pairs-l1.json](experiments/neon-pairs-l1.json). It fixes Codex `gpt-6.1-sol` at high effort, Docker isolation, one attempt at a time and five attempts per setup. Helpful stays defined in the suite and is left out because Extension testing waits.

The plan's `local-config` choices copy the judge and simulated user from `src/evals/eval.config.local.json`, or `eval.config.json` when no local file exists. Those resolved choices are saved with the batch. Plans can select other cases and layers that the benchmark provides. Later layers use banks with only their permitted facts.

`run` refuses a plan with `local` isolation unless you pass `--development`. Such attempts are labeled unfit for scored results. Model agents and verifier builds still use containers so project code never runs on this machine. The mock smoke command supplies mock stages for development without Docker.

## Results

Attempts stay in the ignored `src/evals/runs/` folder until you export them. Open the UI with `npm run evals -- ui`. It serves `http://localhost:4318/`.

The UI has these views:

- Play and compare - play games A and B on separate origins and pick one before revealing the setups.
- Results - read tables, charts, checks, judge answers, questions and screenshots for each tool and model.

The pinned `mock` provider still hands in the reference game for harness development. Editing intent through the UI writes `intent.local.json` and changes the input fingerprint. A batch keeps its frozen bank even if a later edit changes that file.

The [Results](../../.agents/memory/crystallized/documents/evals/results.md) document explains saved records and export. [Grading](../../.agents/memory/crystallized/documents/evals/grading.md) explains checks and ballots. [Running Evals](../../.agents/memory/crystallized/documents/evals/running-evals.md) covers sign-in and recovery.

## Limits

- Five runs per setup screen behavior. They don't support a claim that Open Forge improves results, and the CLI stays beta.
- Hidden checks take about six minutes a run, most of it playing whole games, one run at a time.
- Each check keeps its first outcome. Rechecking saved files appends a new assessment and never changes the old one.
- Real agents always run in Docker. `local` isolation is for harness development with the mock provider, and its runs are marked as not for scoring.
- Codex reports no dollar cost, and Claude Code's cost is its own estimate. Missing token counts stay missing.
