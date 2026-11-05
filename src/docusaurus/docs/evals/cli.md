---
title: CLI reference
---

# CLI reference
Open Forge Evals is one command, `npm run evals -- <command>`, run from the repository root.

This page is generated from the CLI's own help, so it always matches `npm run evals -- help`. Every setting is a flag. Sign-in values are the only exception, and they stay in `src/evals/.env.local`.

## `doctor`

Check Docker, Node, pinned tools, sign-ins and image digests.

```sh
npm run evals -- doctor
```

## `images`

Build the three images from locked digests, or only the kinds named: agent, verifier or assessor. --tag prints each image's local name instead of building it.

```sh
npm run evals -- images verifier
```

## `smoke`

Run a mock implementation run through every stage and saved records, without Docker or models.

```sh
npm run evals -- smoke
```

## `qualify`

Check fresh setup files and the pinned Open Forge version without models.

```sh
npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json
```

## `validate-case`

Validate a Layer 4 starting project and solution in verifiers without models.

```sh
npm run evals -- validate-case --case l4-f1-name-saves
npm run evals -- validate-case --all --plan src/evals/experiments/neon-pairs-l4-check.json
```

## `chain-check`

Before a later task of a task-by-task packet runs, build one child start per setup from the previous task's runs, grade it as it is in verifiers without models, and list every check whose result differs from the parent's.

```sh
npm run evals -- chain-check --plan src/evals/experiments/neon-pairs-p1-task-by-task.json --case l4-b2-arrow-keys --parent <batch>
```

## `run`

Run a saved plan. --implementers: claude, codex, both, codex-orchestrators, codex-all, codex-solo. --judges: claude, codex, both. --user: claude, codex. Each replaces that role in the plan. --case runs one case of a Layer 4 plan, so each case can run as its own process. --setups runs some of the plan's setups, always with its control. --parent names the previous milestone's batch for a chain plan. --parallel caps how many runs go at once. Local isolation needs --development.

```sh
npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both --user claude
```

## `resume`

Continue a batch that paused at a usage limit or a rejected sign-in, or stopped early. Finished runs and judgments are kept, and a run stopped while it was graded is graded from its saved files. --parallel caps how many runs go at once. --accept-harness continues on changed runner code and records it in the batch, while changed inputs, checks, rubric, tooling or models still need a new batch.

```sh
npm run evals -- resume <batch> --parallel 2 --accept-harness
```

## `progress`

Show how far each batch has come: implemented, checked and judged runs, then each setup's mean check coverage and each judge's yes share. These are preliminary scores while a batch runs. Names batches, or shows every model batch.

```sh
npm run evals -- progress <batch> <batch>
```

## `usage`

Show where each setup's tokens went, from saved runs and without saving anything: exact new, cached and output tokens, requests, peak context and the share of input before the first edit, then estimated input tokens by class and file type and tool calls by kind. It reads the same analysis analyze saves. Names batches, or shows every model batch.

```sh
npm run evals -- usage <batch>
```

## `analyze`

Work out each saved run's objective measures and append them as an analysis record: exact token totals, tokens by class and file type, tool calls by kind and the steps before the first edit. Codex's session log and Claude's stream give each request's exact input, so only the split of each request's growth is estimated. Codex runs saved without a session log get a rougher estimate from their events. Names batches, or analyzes every model batch. --missing analyzes only runs without a current analysis, such as runs from before this analyzer version.

```sh
npm run evals -- analyze <batch> --missing
```

## `reference`

Grade the tracked Neon Pairs reference without an agent. Defaults to --layer 3 --judges claude.

```sh
npm run evals -- reference --layer 3 --judges both
```

## `summary`

Print reference history, or implementer and judge pairs for --batch.

```sh
npm run evals -- summary --batch <id>
```

## `site`

Start the docs site on this machine and open its Results page with your local runs, including batches still running. A data server on the next port serves them to the page only. --repository shows a folder in the results repository's layout instead, such as src/evals/results. --port picks the docs port, 4320 by default. --no-browser starts both without opening a browser.

```sh
npm run evals -- site --port 4320
```

## `check`

Check saved runs in the verifier and append the results, as many at once as the check slots allow. Name a batch, and optionally one run. --rebuild builds the saved project again first. --missing checks only runs without a check free of errors, so it finishes a batch that checked later, or a fixed checker finishes one without grading it all again. Every check also saves its diagnostics, such as Layer 1 card crops and look comparisons.

```sh
npm run evals -- check <batch> --missing
```

## `judge`

Judge saved runs and append the ballots. Name a batch, and optionally one run. --judges: claude, codex, both. Defaults to the batch's saved judges. --missing judges only runs that judge hasn't judged without an error, so it can continue after a usage limit.

```sh
npm run evals -- judge <batch> --judges claude --missing
```

## `results`

Add a finished batch, with its code, game, screenshots and conversations, to the results repository, replacing an earlier copy, then validate the repository. Evidence holding a sign-in, a machine path or the user name stops it. --repository picks another results repository than src/evals/results. results grade starts the results repository's grading workflow, which checks every published run waiting for its checks with this repository's pushed graders and pushes the grades to its main branch. results import, given a published batch folder, rebuilds that batch in the run store, so check --missing and judge --missing can grade it on any machine before results add publishes it again.

```sh
npm run evals -- results add <batch>
```

## Flags

| Flag | Commands | Effect |
| --- | --- | --- |
| `--help` | Any command | Show the command's help and example without running it |
| `--plan <file>` | `run`, `qualify`, `validate-case`, `chain-check` | The saved plan to run or check |
| `--implementers <preset>` | `run` | Replace the plan's implementers with claude, codex, both, codex-orchestrators, codex-all, codex-solo |
| `--judges <preset>` | `run`, `judge`, `reference` | Replace the judges with claude, codex, both |
| `--user <preset>` | `run` | Replace the simulated user with claude, codex |
| `--case <id>` | `run`, `validate-case`, `chain-check` | Run or validate one Layer 4 case, so each case can run as its own process, or name the task chain-check plants next in a task-by-task packet |
| `--setups <ids>` | `run`, `judge` | Run or judge only these setups, comma-separated. For a run they keep the plan's order, and the plan's first setup is the control and must stay |
| `--parent <batch>` | `run`, `chain-check` | Name the previous task's batch. Each run starts from the parent run with the same setup and repeat |
| `--all` | `validate-case` | Validate every case of the plan |
| `--layer <n>` | `reference` | Grade the reference against Layer 2 or 3 |
| `--parallel <n>` | `run`, `resume`, `judge` | How many runs go at once. The full plans run two, because every run spends the signed-in account's own model limits |
| `--accept-harness` | `resume` | Continue a batch on changed runner code and list the change in the batch record. Changed inputs, checks, rubric, tooling or models still need a new batch |
| `--development` | `run`, `qualify` | Allow local isolation, which is never fit for scoring |
| `--rebuild` | `check` | Build the saved project again before checking it |
| `--missing` | `judge`, `check`, `analyze` | Grade or analyze only runs without a usable result. For judge, that's a run without a judgment from that judge or one that ended in an error. For check, it's a run whose latest check graded nothing or ended a clause in an error. For analyze, it's a run without an analysis from the current analyzer or one that ended in an error |
| `--batch <id>` | `summary` | The batch to summarize |
| `--tag` | `images` | Print each named image's local name instead of building it, so a pulled image can be tagged for the checks |
| `--ref <commit>` | `results` | The Open Forge commit whose graders results grade runs, HEAD by default. It must be pushed, since the workflow checks it out from GitHub |
| `--imported` | `results` | Republish a batch that results import brought in. It was scrubbed when first published, so this machine's user name is neither scrubbed nor treated as a leak |
| `--repository <folder>` | `results`, `site` | The results repository to add to, src/evals/results by default, or a folder in its layout for site to show |
| `--port <n>` | `site` | Serve the docs site on another port than 4320. Its data server takes the next port |
| `--no-browser` | `site` | Start the docs site without opening a browser |
| `--scratch <folder>` | Any command | Put run workspaces on a roomier drive, outside any Git repository |
| `--check-slots <n>` | Any command | How many check stages run at once across every eval process on this machine. 1 by default, because browser checks are timed |

Back to [Evals](index.md).
