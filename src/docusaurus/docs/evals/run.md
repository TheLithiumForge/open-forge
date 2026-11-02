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
| Implementer | `--implementers` | Claude `opus`, high | Codex `gpt-6.1-sol`, high | Codex then Claude |
| Judge | `--judges` | Claude `opus`, high | Codex `gpt-6.1-sol`, xhigh | Claude then Codex |
| Simulated user | `--user` | Claude `sonnet`, medium | Codex `gpt-6-luna`, low | Unavailable |

`--parallel` caps how many runs go at once. The [existing app](starting-points.mdx#existing-app) plans hold three cases, and `--case l4-f01` runs one of them as its own batch. Records go to the ignored `src/evals/runs/` folder.

Two machine flags work with any command. `--scratch D:/eval-workspaces` puts run workspaces on a roomier drive. `--check-slots 5` lets five check stages run at once across every eval process. Each check uses about one processor core, so leave room for the agents.

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

`check` builds the saved project in the verifier again and appends new check results. `judge` appends ballots from the chosen judges. `--missing` judges only runs without a judgment from that judge, so judging can continue after a usage limit. Both accept one run after the batch. Earlier grades stay recorded with their versions.

## See results

```sh
npm run evals -- site
```

`site` starts this documentation site on your machine and opens [Results](results.mdx) with your local runs, including batches still running. A data server on the next port serves the runs to that page only. `--port` moves both, and `--repository src/evals/results` shows a results repository instead.

## Add results

```sh
npm run evals -- results add <batch>
```

`results add` copies a finished batch's records and evidence into the results repository in `src/evals/results`: agent code without installed packages, the built game, screenshots, conversations and event logs. Adding a batch again replaces it. The command stops if evidence holds a sign-in, a machine path or your user name, and it validates the whole repository afterwards. Publishing the repository is a separate maintainer decision.

## Reference

```sh
npm run evals -- reference --layer 3 --judges both
```

`reference` grades the tracked Neon Pairs reference solution through the same build, checks and judges, without an agent. `--layer 3` grades it as a full spec, and `--layer 2` as a short brief. Each reference batch is a data point in [History](history.mdx).

Every command and flag is in the [CLI reference](cli.md). Back to [Evals](index.md).
