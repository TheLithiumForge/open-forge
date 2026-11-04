# Run evals
Open Forge Evals is a command-line runner that implements, checks and judges coding runs in Docker containers on your own machine.

For example, the full plan for a [short brief](starting-points.mdx#brief) builds Neon Pairs three times for each of six setups, then checks and judges every game. `npm run evals -- progress` shows how far the batch has come, and `npm run evals -- site` opens the results in this documentation site, including batches still running.

## Requirements

Run every command from the Open Forge repository root. You need:

- Node 24 or later and npm.
- Docker, running. Agents, checks and judges only ever run in containers.
- A Claude Code or Codex sign-in for each provider you choose. Runs spend that account's own usage.

## Install

```sh
npm ci --prefix src/evals
```

```sh
npm ci --prefix src/docusaurus
```

The second command is only needed to see results with `site`. To add results to the results repository, also install its tools:

```sh
npm ci --prefix src/evals/results
```

Claude Code reads `CLAUDE_CODE_OAUTH_TOKEN` or `ANTHROPIC_API_KEY` from `src/evals/.env.local`. For a subscription token, run:

```sh
npm exec --prefix src/evals -- claude setup-token
```

Codex keeps its sign-in in an eval-only folder:

```sh
npm --prefix src/evals run login:codex
```

Sign-in values are the only settings outside flags. They stay in `src/evals/.env.local` and the eval-only folder, never in command arguments. Only the chosen provider's sign-in enters each container.

## First run

```sh
npm run evals -- smoke
```

`smoke` runs a mock implementation through every stage without Docker or models and prints `Smoke passed`. Then check the machine and build the three images:

```sh
npm run evals -- doctor
```

```sh
npm run evals -- images
```

`doctor` checks Node, Docker, the pinned tools, sign-ins and image digests without spending usage. Every command that calls a provider also refuses to start when its sign-in is missing.

## Run a batch

A plan in `src/evals/experiments/` names the case, setups and repeats. Role flags replace its models:

```sh
npm run evals -- run --plan src/evals/experiments/neon-pairs-l2-full.json --implementers codex --judges codex --user codex
```

| Role | Flag | `claude` | `codex` | `both` |
| --- | --- | --- | --- | --- |
| Implementer | `--implementers` | Claude `opus`, high | Codex `gpt-6.1-sol`, xhigh | Codex then Claude |
| Judge | `--judges` | Claude `opus`, high | Codex `gpt-6.1-sol`, xhigh | Claude then Codex |
| Simulated user | `--user` | Claude `sonnet`, medium | Codex `gpt-6-luna`, max | Unavailable |

`--implementers codex-solo` runs Luna at max and Terra at xhigh, each alone. `--implementers codex-orchestrators` runs Sol leading Sol subagents and Sol leading Luna subagents, with Sol at xhigh and Luna at max, as the maintainer set them, for orchestrated work. A team is part of the implementer, so its runs are planned, counted and filtered apart, as another model's are. A solo Codex run switches Codex's subagents off, and a team leader gets one level of them, one at a time, with a registered line after the request that tells it to delegate each task.

A run goes through three stages, and a plan can split them apart:

| Stage | What it does | Command |
| --- | --- | --- |
| Work | The agent and its questions, the offline build, the saved files and the [objective measures](measures.mdx#objective-measures) | `run` |
| Checks | The hidden checks on the saved files | `check <batch> --missing` |
| Judges | The AI judges on the saved files and the checks' screenshots | `judge <batch> --missing` |

A plan with `"checking": "later"` and `"judging": "later"` ends each run after its build, so `progress` shows when every agent is done, before any grading starts. Checks don't take processor cores from agents that are still working, and they can run whenever you choose, on any batch, old ones included. A plan with only `"judging": "later"` checks each run after its build and leaves the judges for later. Checks later needs judges later, since the judges read the checks' screenshots.

`--parallel` caps how many runs go at once. The [existing app](starting-points.mdx#existing-app) plans hold three cases, and `--case l4-f1-name-saves` runs one of them as its own batch. Records go to the ignored `src/evals/runs/` folder.

Two machine flags work with any command. `--scratch D:/eval-workspaces` puts run workspaces on a roomier drive. `--check-slots 5` lets five check stages run at once on the machine, within one batch and across batches. Each check uses about one processor core, so leave room for the agents.

## Run a work packet

A packet plan names its packet and its [work mode](work.mdx#work-modes) instead of cases:

```sh
npm run evals -- run --plan src/evals/experiments/neon-pairs-p1-whole-packet.json --check-slots 6
npm run evals -- run --plan src/evals/experiments/neon-pairs-p1-task-by-task.json --check-slots 6
npm run evals -- run --plan src/evals/experiments/neon-pairs-p1-orchestrated.json --check-slots 6
```

Whole packet and orchestrated work run one batch on the packet. Task by task runs each task as a batch of its own, in order, in one process. Each later task's run starts from the run with the same scenario, implementer and repeat in the task before: its saved files, without build output, with the next task's bug planted. A run that didn't finish, didn't build or changed the lines the next bug needs starts no run after it. The batch lists those pairs with their reason, and they stay out of every mean. Each later run saves its start in `start.json`, and checks compare against it.

`chain-check` builds one later start per scenario from a finished task's batch and grades it without a model, so a planted bug can be checked against real results before the next task runs:

```sh
npm run evals -- chain-check --plan <plan> --parent <batch>
```

`npm run packets --prefix src/evals` writes each packet's request, starting code, solution, checks and answers from its tasks, and a test fails while a packet and its tasks differ.

## Follow a batch

```sh
npm run evals -- progress
```

`progress` shows each batch's implemented, checked and judged runs, then each setup's mean [check coverage](measures.mdx#check-coverage) and each judge's [judge score](measures.mdx#judge-score). These scores are preliminary until every run is graded.

A batch pauses when a provider hits a usage limit or rejects a sign-in. To continue it:

```sh
npm run evals -- resume <batch>
```

Finished runs and grades are kept. A run stopped while it was graded is graded from its saved files, without running the agent again. A batch continues only on the runner code it started with, unless `--accept-harness` records the change.

## Grade again

Grading reads saved files, so it can run any time after the implementation:

```sh
npm run evals -- check <batch> --rebuild
```

```sh
npm run evals -- judge <batch> --judges claude --missing
```

`check` grades a batch's runs at once, as many as the check slots allow, and appends new check results. `--rebuild` builds each saved project in the verifier again first. `--missing` checks only runs without a check free of errors, so it finishes a batch that checked later or one whose checker was fixed. `judge` appends ballots from the chosen judges. `--missing` judges only runs without a judgment from that judge, so judging can continue after a usage limit. Both accept one run after the batch. Earlier grades stay recorded with their versions.

A published batch can be graded on another machine. `results import` rebuilds it in the run store from the results repository's records and evidence, and `results add` publishes it again with the new grades:

```sh
npm run evals -- results import src/evals/results/versions/<cli-version>/batches/<batch>
npm run evals -- check <batch> --missing
npm run evals -- results add <batch>
```

`npm run evals -- results grade` does this in CI: it starts the results repository's Grade published runs workflow with the graders of this repository's pushed commit. The workflow builds the verifier image once, checks each waiting run in its own job on GitHub's Ubuntu runners, up to 20 at a time, and pushes the grades to the results repository's `main`.

The [objective measures](measures.mdx#objective-measures) come from saved logs too, without a model:

```sh
npm run evals -- analyze <batch> --missing
```

`analyze` appends each run's analysis. `--missing` analyzes only runs without one from the current analyzer, such as runs saved before it existed. `usage` prints each setup's means from the same analysis without saving anything.

## See results

```sh
npm run evals -- site
```

`site` starts this documentation site on your machine and opens [Results](results.mdx) with your local runs, including batches still running. A data server on the next port serves the runs to that page only. `--port` moves both, and `--repository src/evals/results` shows a results repository instead.

## Add results

```sh
npm run evals -- results add <batch>
```

`results add` copies a finished batch's records and evidence into the results repository in `src/evals/results`: agent code without installed packages or downloaded tools, the built game, screenshots, conversations and event logs. A browser an agent downloaded stays in the run store, and so does any file over 50 MB. Adding a batch again replaces it. The command stops if evidence holds a sign-in, a machine path or your user name, and it validates the whole repository afterwards. Publishing the repository is a separate maintainer decision.

## Reference

```sh
npm run evals -- reference --layer 3 --judges both
```

`reference` grades the tracked Neon Pairs reference solution through the same build, checks and judges, without an agent. `--layer 3` grades it as a full spec, and `--layer 2` as a short brief. Each reference batch is a data point in [History](history.mdx).

Every command and flag is in the [CLI reference](cli.md). Back to [Evals](index.md).
