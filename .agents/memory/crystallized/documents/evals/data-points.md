---
open-forge:
  description: The values every Open Forge eval records for each run, and how we get, count, judge and report each one
  tags: [Memory, Document, CurrentTruth, Evaluation, Measurement, Context, Tokens]
---

# Eval Data Points

Eval Data Points is the set of values every Open Forge eval records for each run, and how we get each value.

An eval compares setups, so every setup has to be measured the same way. Keeping the data points in one place does that. For example, the [Greenfield Baseline](greenfield-baseline.md) records functional success, tokens and compactions for runs in an empty folder and for runs in a fresh Open Forge install, then compares the two.

Status: proposed on 2026-10-30, then updated the same day with rules followed, intent coverage, build and blind picks. The [eval tool](../../../../../evals/README.md) records them, and only its mock runs are tested.

## Runs and setups

A run is one agent session, from the prompt until the agent stops or hits a limit. Each run gets one record with:

- Its identity: eval, setup, tool, tool version, model, effort, Open Forge version, date and run number.
- Every data point below that the eval uses.

Each setup also gets one record, written once: its files, their hashes and size, and the effort it took to write them.

## How we judge

Each data point names how we get it:

- Automatic - the harness reads it from the tool's usage records, the transcript or the files, with no judgment.
- Hidden checks - tests the agent never sees, run after it stops, on a frozen copy of its files.
- Model judge - a fixed model scores against a written rubric and cites the code it scored.
- People - judges who don't know the setup score the same screenshots and recordings.

[Eval Judging](judging.md) says who judges each data point and when people check the others.

## Results

| Data point           | Unit                                             | How we get it                                                                                      |
| -------------------- | ------------------------------------------------ | -------------------------------------------------------------------------------------------------- |
| Functional success   | Pass or fail                                     | Hidden checks: the result builds, starts and passes every check                                    |
| Requirement coverage | Checks passed, %                                 | Hidden checks: one check per requirement in the prompt                                             |
| Rules followed       | Share of yes answers per rule                    | Model judge: each rule the user gave, with the code it cites                                       |
| Intent coverage      | Intent items met, %                              | Hidden checks where an item can be checked, model judge for the rest                               |
| Picked               | Share of blind comparisons won                   | People: two finished games played side by side, labeled A and B, setups hidden                    |
| Code quality         | Share of yes answers                             | Model judge: yes or no questions on readability, structure, needless complexity and saved data     |
| Code metrics         | Type errors, warnings, duplication %, complexity | Automatic: fixed analysis tools on the code the agent wrote                                        |
| Build                | Pass or fail, seconds, packages                  | Automatic: the project's build run again offline, and the count of installed packages              |
| Performance          | Milliseconds, bytes                              | Automatic: first render, interaction time at the 95th percentile and shipped size, ten times each  |
| Target met           | Measured value against the target                | Automatic, only when the prompt sets a target such as a size limit                                 |

Checks test only what the prompt asks for. Extra features earn nothing, and no check requires a file layout or a name the prompt didn't give.

Intent coverage is the exception, on purpose: it scores what the user wanted but didn't say, from a hidden intent sheet. It stays apart from requirement coverage and never counts toward functional success. It shows whether a setup that asks better questions gets the user closer to what they wanted.

## Cost

| Data point | Unit                                                           | How we get it                                  |
| ---------- | -------------------------------------------------------------- | ---------------------------------------------- |
| Tokens     | Uncached input, cached input, cache writes, output, reasoning  | Automatic: the tool's usage records            |
| Dollars    | Per run, at API prices                                         | Automatic: tokens times a dated price list     |
| Time       | Seconds                                                        | Automatic: install, agent run and checks apart |
| Tool calls | Count, failures                                                | Automatic: the transcript                      |

## Context

| Data point        | Unit                        | How we get it                                                                                                  |
| ----------------- | --------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Peak context      | Tokens, share of the window | Automatic: the largest input of a single request                                                               |
| Total input       | Tokens                      | Automatic: all request inputs added up                                                                         |
| Context delivered | Bytes, unique and repeated  | Automatic: text that reached the agent, with Open Forge files counted apart and startup apart from later reads |
| Compactions       | Count, size before and after| Automatic, where the tool reports them                                                                         |
| Lost progress     | Requirements, %             | Hidden checks on snapshots saved during the run                                                                |

Context degradation is what an agent forgets during a run, and we can't see that directly. Lost progress is our proxy: a requirement that passes on a snapshot during the run and fails at the end. A short run with no compaction tells us little about it.

## User effort

| Data point   | Unit                  | How we get it                                                   |
| ------------ | --------------------- | --------------------------------------------------------------- |
| Prompt words | Words                 | Automatic: the submitted prompt                                 |
| Questions    | Count, rounds         | Automatic: the agent's questions                                |
| Answers      | Words, tokens         | Automatic: the simulated user's answers and what they cost      |
| Setup effort | Minutes, tokens       | Setup record, counted once per setup                            |

## Counting rules

These rules keep the numbers comparable:

- Count each model request once, including helpers and retries. Take differences between running totals instead of adding them up.
- Cached input counts as input and as context.
- In Claude Code, a request's input is `input_tokens` plus `cache_read_input_tokens` plus `cache_creation_input_tokens`. In Codex, input already includes cached input.
- Missing telemetry is "unknown", never zero.
- Compare tokens only within one tool and model, because tokenizers differ.
- The simulated user's tokens count as user effort, never as the agent's cost.
- Screenshots, checks and judges measure one run. They never count as extra runs.

## Reporting

Every eval reports its results the same way:

- One table per tool and model, with one row per setup: runs, success with its interval, coverage, wishes, rules, code, picks, time, tokens, peak context, compactions and lost progress.
- A chart of each setup's difference from the comparison setup, with its interval.
- A chart of tokens against coverage, one dot per run.
- Every run stays in, including failures and timeouts. Quality and cost stay separate, and there's no blended score.

## Not decided yet

- The dated price list used for dollars.
- The analysis tools for code metrics.
- How many snapshots a run saves for lost progress.

Back to [Evals](_evals.md).
