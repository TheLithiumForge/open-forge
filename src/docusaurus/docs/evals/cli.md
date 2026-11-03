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

Build the three images from locked digests.

```sh
npm run evals -- images
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
npm run evals -- validate-case --case l4-f01
npm run evals -- validate-case --all --plan src/evals/experiments/neon-pairs-l4-check.json
```

## `run`

Run a saved plan. --implementers: claude, codex, both. --judges: claude, codex, both. --user: claude, codex. Each replaces that role in the plan. --case runs one case of a Layer 4 plan, so each case can run as its own process. --setups runs some of the plan's setups, always with its control. --parallel caps how many runs go at once. Local isolation needs --development.

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

Show where each setup's tokens went, from saved runs: new, cached and output tokens, how much the agent read from spec documents, framework files and code, the share of steps before its first edit and, for Claude, the share of tokens spent before it. Names batches, or shows every model batch.

```sh
npm run evals -- usage <batch>
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

Check saved runs again in the verifier and append the results. Name a batch, and optionally one run. --rebuild builds the saved project again first. --missing checks only runs without a check free of errors, so a fixed checker can finish a batch without grading it all again. Every check also saves its diagnostics, such as Layer 1 card crops and look comparisons.

```sh
npm run evals -- check <batch> --missing
```

## `judge`

Judge saved runs and append the ballots. Name a batch, and optionally one run. --judges: claude, codex, both. Defaults to the batch's saved judges. --missing judges only runs that judge hasn't judged without an error, so it can continue after a usage limit.

```sh
npm run evals -- judge <batch> --judges claude --missing
```

## `results`

Add a finished batch, with its code, game, screenshots and conversations, to the results repository, replacing an earlier copy, then validate the repository. Evidence holding a sign-in, a machine path or the user name stops it. --repository picks another results repository than src/evals/results.

```sh
npm run evals -- results add <batch>
```

## Flags

| Flag | Commands | Effect |
| --- | --- | --- |
| `--help` | Any command | Show the command's help and example without running it |
| `--plan <file>` | `run`, `qualify`, `validate-case` | The saved plan to run or check |
| `--implementers <preset>` | `run` | Replace the plan's implementers with claude, codex, both |
| `--judges <preset>` | `run`, `judge`, `reference` | Replace the judges with claude, codex, both |
| `--user <preset>` | `run` | Replace the simulated user with claude, codex |
| `--case <id>` | `run`, `validate-case` | Run or validate one Layer 4 case, so each case can run as its own process |
| `--setups <ids>` | `run` | Run only these setups of the plan, comma-separated, in the plan's order. The plan's first setup is the control and must stay |
| `--all` | `validate-case` | Validate every case of the plan |
| `--layer <n>` | `reference` | Grade the reference against Layer 2 or 3 |
| `--parallel <n>` | `run`, `resume` | How many runs go at once. The full plans run two, because every run spends the signed-in account's own model limits |
| `--accept-harness` | `resume` | Continue a batch on changed runner code and list the change in the batch record. Changed inputs, checks, rubric, tooling or models still need a new batch |
| `--development` | `run`, `qualify` | Allow local isolation, which is never fit for scoring |
| `--rebuild` | `check` | Build the saved project again before checking it |
| `--missing` | `judge`, `check` | Grade only runs without a usable grade. For judge, that's a run without a judgment from that judge or one that ended in an error. For check, it's a run whose latest check graded nothing or ended a clause in an error |
| `--batch <id>` | `summary` | The batch to summarize |
| `--repository <folder>` | `results`, `site` | The results repository to add to, src/evals/results by default, or a folder in its layout for site to show |
| `--port <n>` | `site` | Serve the docs site on another port than 4320. Its data server takes the next port |
| `--no-browser` | `site` | Start the docs site without opening a browser |
| `--scratch <folder>` | Any command | Put run workspaces on a roomier drive, outside any Git repository |
| `--check-slots <n>` | Any command | How many check stages run at once across every eval process on this machine. 1 by default, because browser checks are timed |

Back to [Evals](index.md).
