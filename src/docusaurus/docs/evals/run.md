# Run evals
Open Forge Evals is a local runner that saves coding attempts and separate check and judge assessments.

For example, a Layer 1 check plan runs Neon Pairs once per setup and implementer. The saved attempts give the local UI and later grading commands their inputs.

## Requirements

Run these commands from the repository root. You need:

- Node 24 or later and npm.
- Docker for model attempts and verifier builds.
- A sign-in or API key for each chosen provider.

Install the pinned eval dependencies with `npm ci --prefix src/evals --cache .npm-cache`. The [evals README](../../../evals/README.md#install) covers provider sign-in and browser installation.

## First run

Run `npm run evals -- smoke`. It prints `Smoke passed` after mock conversation, build, checks and judging finish. It removes its temporary records. The smoke uses no Docker or models.

Before a model run, fill both base-image digests in `src/evals/images/lock.json`. Then run these commands in order:

```sh
npm run evals -- doctor
npm run evals -- images
npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1-check.json
npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json
```

`doctor` checks prerequisites without a paid probe or model sign-in. `images` builds the locked images. `qualify` checks generated setups before the scored sample. `run` saves attempts and assessments in the ignored `src/evals/runs/` folder. Check plans run once per setup and implementer. Full plans run three times.

## Model presets

Each flag replaces only its role in the saved plan:

| Role | Flag | `claude` | `codex` | `both` |
| --- | --- | --- | --- | --- |
| Implementer | `--implementers` | Claude `opus`, high | Codex `gpt-6.1-sol`, high | Codex then Claude |
| Judge | `--judges` | Claude `opus`, high | Codex `gpt-6.1-sol`, xhigh | Claude then Codex |
| Simulated user | `--user` | Claude `sonnet`, medium | Codex `gpt-6-luna`, low | Unavailable |

To produce both implementers and have both judges grade each saved attempt, run:

```sh
npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both --user claude
```

Each judge casts three ballots per question. The second judge reads the saved source and images through the judge command. A resumed batch fills missing judgments without repeating a finished implementation. The batch saves resolved model choices so a later preset change leaves history intact.

## Read results

Run `npm run evals -- ui` to open the local UI at `http://localhost:4318/`. Results has tables, charts, checks, judge answers, questions and screenshots. Play and compare lets you pick between games before revealing their setups. The config editor saves role presets for the next batch started in the app.

Run `npm run evals -- summary --batch <id>` to print the implementer and judge matrix. To read published batches, open [Results](results.mdx).

## Reference

Run `npm run evals -- reference --layer 3 --judges both` to grade the tracked Neon Pairs reference. It skips the implementer, then uses the normal build, hidden checks and judge containers. The command accepts Layer 2 or 3. It defaults to Layer 3 and the `claude` judge preset.

Each reference batch records its source commit, content hash, spec version and spec hash. Assessments record grading hashes and judge choices. Run the command again to add a data point. The local UI's Reference over time view and the public [History](history.mdx) keep it separate from model comparisons.

## Grade again

After a grader error, check the saved files again. These commands append assessments:

```sh
npm run evals -- check <batch> --rebuild
npm run evals -- judge <batch> --judges both
```

Both commands accept an attempt after the batch. `--rebuild` runs the verifier again. Without `--judges`, judge uses the batch's saved judges. Earlier grades stay recorded with their versions.

## Add results

Run `npm run evals -- results add <batch>` to add a finished batch to the results repository in `src/evals/results`. It copies the records and evidence, including agent code, the built game, screenshots and conversations. Adding a batch again replaces it. The command stops if evidence holds a sign-in, a machine path or your user name, and it validates the whole repository afterwards. Install the repository's tools once with `npm ci --prefix src/evals/results`. Publishing the repository is a separate maintainer decision.

Back to [Evals](index.md).
