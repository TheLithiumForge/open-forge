# Open Forge Evals
Open Forge Evals is a local tool that runs the same case through Claude Code and Codex under different Open Forge setups and saves comparable results.

For example, the Layer 1 check plan runs Neon Pairs once with Claude on No setup, Minimal, Essentials and Full Core. Claude then judges each saved attempt. Every attempt starts with one prompt and gets literal answers from the same frozen answer bank when it asks questions.

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
npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1-check.json
npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json
```

`doctor` checks Node, Docker, installed tool package versions, configured sign-ins and image digests. It doesn't perform a model sign-in or a paid probe. `images` refuses to build while a digest is missing. `qualify` installs fresh setups with the pinned Open Forge CLI and checks entry files, the framework lock, planned files and unwanted files before the scored sample starts.

## Commands

Every command accepts `--help`. Each help text includes an example:

| Command | Job | Example |
| --- | --- | --- |
| `doctor` | Check prerequisites | `npm run evals -- doctor` |
| `images` | Build the three locked images | `npm run evals -- images` |
| `smoke` | Run the mock attempt | `npm run evals -- smoke` |
| `qualify` | Check generated setups | `npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1-check.json` |
| `run` | Run a saved plan with optional role presets | `npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both --user claude` |
| `resume` | Continue a batch that paused at a usage limit | `npm run evals -- resume <batch>` |
| `reference` | Grade the tracked reference | `npm run evals -- reference --layer 3 --judges both` |
| `summary` | Print the implementer and judge matrix | `npm run evals -- summary --batch <id>` |
| `ui` | Open the local results app | `npm run evals -- ui` |
| `recheck` | Append checks for saved files | `npm run evals -- recheck <batch> --rebuild` |
| `rejudge` | Append judge ballots with optional judge presets | `npm run evals -- rejudge <batch> --judges both` |
| `export` | Write sanitized finished records | `npm run evals -- export <batch> --out ./eval-export` |

`run --case <id>` runs one case of a Layer 4 plan, so each case can run as its own process. `--parallel <n>` caps how many attempts `run` and `resume` start at once. The full plans run two at a time, because every attempt spends the signed-in account's own model limits. A batch resumes only on the runner code it started with. `resume --accept-harness` continues on changed runner code and lists the change in the batch record, while changed inputs, checks, rubric, tooling or models still need a new batch. Check stages share `EVALS_CHECK_SLOTS` machine-wide slots, two by default, so timed browser checks don't run past their limit while many agents work. `recheck` and `rejudge` accept an optional attempt after the batch. `--rebuild` runs the saved project through the verifier again. `rejudge` uses the batch's saved judges unless you pass `--judges`. It accepts a judge preset instead of `--plan`. Checks and ballots append assessments instead of replacing earlier grades. `export` writes a new immutable pack and refuses an existing target.

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

## The reference as a data point

The reference command grades the Neon Pairs reference as one saved attempt. It skips the agent, then uses the normal verifier build, the layer's hidden checks and the judges in their containers.

Run `npm run evals -- reference [--layer 2|3] [--judges <preset>]`. It defaults to Layer 3 and the `claude` judge preset. For example, to grade with both judges:

```powershell
npm run evals -- reference --layer 3 --judges both
```

The batch freezes the Git-tracked working files from `benchmarks/neon-pairs/reference/`. It leaves out ignored output, installed packages and build output. It records `reference` as the implementer, the last commit that changed the reference, a SHA-256 hash of the copied file list and bytes, the spec version and its content hash. Each assessment records the checker, rubric and judge prompt hashes. Judge assessments also record the requested model, effort, ballot count and reported model when available.

Run the command again to add a new batch. To grade its saved files again, use `recheck` or `rejudge`. These commands append assessments and record the spec used for that grading.

`npm run evals -- summary` shows reference coverage and judge scores over time. Add `--batch <id>` to read one batch. The web UI's "Reference over time" view shows the same history. Matching reference, spec and grading versions share a group, with each assessment's time and reference commit shown in its row. Changed hashes or judge models form separate groups. Reference scores stay out of model setup comparisons. The results export includes their records and assessments.

## Plans

Each layer has a check plan and a full plan in `experiments/`. Start with [neon-pairs-l1-check.json](experiments/neon-pairs-l1-check.json). Check plans run once per setup and implementer. Full plans run three times. Every saved plan chooses the `claude` presets for now, as the [vision](../../.agents/memory/crystallized/documents/evals/vision.md#test-matrix) records. Helpful stays out because Extension testing waits.

The role presets are:

| Role | `claude` | `codex` | `both` |
| --- | --- | --- | --- |
| Implementers | Claude `opus`, high | Codex `gpt-6.1-sol`, high | Codex then Claude |
| Judges | Claude `opus`, high | Codex `gpt-6.1-sol`, xhigh | Claude then Codex |
| Simulated user | Claude `sonnet`, medium | Codex `gpt-6-luna`, low | Unavailable |

`run --implementers`, `--judges` and `--user` each replace only that role in the plan. Unknown names fail before the run starts. For example, `run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both` restores all four pairs without changing a plan file. The two implementers produce two attempts per cell. Both judges read each saved attempt.

Every judge preset casts three ballots per question. The first judge runs as an attempt finishes. Each later judge reads the same saved source and images through append-only rejudge. A resumed batch fills missing judgments without repeating a finished implementation.

A plan's `agents`, `judges` and `simulatedUser` can name these presets, give explicit model choices or use `local-config`. `local-config` copies that role from `src/evals/eval.config.local.json`, or `eval.config.json` when no local file exists. Older plans with one `judge` still work. Batches save resolved model choices, so later preset changes never alter saved history. Plans can select other cases and layers that the benchmark provides. Later layers use banks with only their permitted facts.

`run` refuses a plan with `local` isolation unless you pass `--development`. Such attempts are labeled unfit for scored results. Model agents and verifier builds still use containers so project code never runs on this machine. The mock smoke command supplies mock stages for development without Docker.

## Config

In the local app's config editor, choose a preset above the rows for each role. A preset switch saves the config through the existing API right away, so the next batch started in the app uses it. The simulated user offers `claude` and `codex`.

The switch shows the matching preset name. If the rows match none, it shows "Custom". You can still edit tools, models, efforts and judge ballot counts in the rows. Use Save after custom edits. The app writes resolved choices to `src/evals/eval.config.local.json`. CLI preset flags affect only the command you run.

## Results

Published batches appear in the main documentation site under [Evals](https://thelithiumforge.github.io/open-forge/docs/evals). The [run guide](https://thelithiumforge.github.io/open-forge/docs/evals/run) covers commands and presets. The [results browser](https://thelithiumforge.github.io/open-forge/docs/evals/results) shows checks and judge pairs. The [history browser](https://thelithiumforge.github.io/open-forge/docs/evals/history) compares CLI versions and reference assessments. Fetch latest results loads validated records from the public results repository without a docs rebuild.

Attempts stay in the ignored `src/evals/runs/` folder until you export them. Open the UI with `npm run evals -- ui`. It serves `http://localhost:4318/`.

The UI has these views:

- Play and compare - play games A and B on separate origins and pick one before revealing the setups.
- Results - read tables, charts, checks, judge answers, questions and screenshots for each tool and model.

To print the same matrix from saved records, run `npm run evals -- summary --batch <id>`. Results group each implementer and judge apart, with every setup inside its pair. Different grading versions and reported models stay in separate groups. Each judge keeps its latest assessment. The judge difference is the second judge's score minus the first judge's score on matching attempts and grading versions. Missing judgments, errors and comparisons with different grading versions are counted apart.

Implementation usage appears in each pair for comparison. It comes from one attempt and isn't added again for each judge.

The pinned `mock` provider still hands in the reference game for harness development. Editing intent through the UI writes `intent.local.json` and changes the input fingerprint. A batch keeps its frozen bank even if a later edit changes that file.

The [Results](../../.agents/memory/crystallized/documents/evals/results.md) document explains saved records and export. [Grading](../../.agents/memory/crystallized/documents/evals/grading.md) explains checks and ballots. [Running Evals](../../.agents/memory/crystallized/documents/evals/running-evals.md) covers sign-in and recovery.

## Limits

- Five runs per setup screen behavior. They don't support a claim that Open Forge improves results, and the CLI stays beta.
- Hidden checks take about six minutes a run, most of it playing whole games, one run at a time.
- Each check keeps its first outcome. Rechecking saved files appends a new assessment and never changes the old one.
- Real agents always run in Docker. `local` isolation is for harness development with the mock provider, and its runs are marked as not for scoring.
- Codex reports no dollar cost, and Claude Code's cost is its own estimate. Missing token counts stay missing.
- Each attempt works in a scratch folder outside any Git repository, by default `~/.workspaces`. Set `EVALS_SCRATCH` to a folder on a roomier drive when the home drive is short of space. It must also sit outside any Git repository.
