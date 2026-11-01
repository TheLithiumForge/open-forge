---
open-forge:
  description: The measurements every eval records, their units, counting rules and separate result panels
  tags: [Memory, Document, CurrentTruth, Evaluation, Measurement, Context, Tokens]
---

# Eval Data Points

Eval Data Points is the measurement contract that records the same outcomes, usage and user effort for every setup.

Status: aligned with the approval of 2026-11-01. Complete measurement support is Not built yet.

A run is one agent attempt from the prompt until completion or a limit. Its record identifies the case, setup, tool and version, model, effort, Open Forge version, date and repetition. A setup record keeps its files, hashes, size and setup effort once. Versioned records follow [Results](results.md).

Every layer records tokens, tool calls, failed tool calls, time, peak context, compactions, Open Forge reads and user effort. Simulated user and judge usage stay apart from coding usage. The [proposal's measurement rule](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) sets this minimum.

## Outcomes

The harness counts usage and build results. Hidden checks run on frozen files after the agent stops. The pinned model judge cites evidence for review questions. People audit results and make blind picks. Their [grading rules](grading.md) define statuses, coverage and audits.

Each outcome has its separate measure:

| Data point | Unit | How it is measured |
| --- | --- | --- |
| Functional success | Working gate and stated success | The layer's executable checks |
| Requirement coverage | Passed checks and percentage | Passed checks out of all applicable checks |
| Rules followed | Yes, no and unsure ballots per rule | Static checks where possible, judge for review clauses |
| Wishes | Items met and percentage | Checks where executable, judge for the rest |
| Blind preference | Share of comparisons won and ties | People play two games with hidden setups |
| Code quality | Yes, no and unsure ballots | Frozen questions on legibility, structure, complexity and saved data |
| Build | Outcome, seconds and package count | Rebuild submitted files offline |
| Code metrics | Type errors, warnings, duplication and complexity | Fixed tools, Not built yet |
| Performance | Milliseconds and bytes | First render, 95th percentile interaction time and shipped size, ten measurements each |
| Target met | Value against the stated target | Measured only when the task sets a target |
| Pipeline completion | Stage outcomes | Execution records, separate from functional success |

Checks test the prompt and the requirements provided at that layer. Extra features earn nothing. A check can't require a file layout or hook the task didn't give.

Wishes measure private preferences and stay apart from requirement coverage and functional success. They show how closely the result matches what the simulated user wanted.

## Usage

Keep these measurements apart:

| Data point | Unit | How it is measured |
| --- | --- | --- |
| Tokens | Uncached input, cached input, cache writes, output and reasoning | Tool usage records |
| Dollars | API-price estimate per attempt | Tokens times a dated price list |
| Time | Seconds | Install, coding, verifier and assessor stages apart |
| Tool calls | Calls and failures | Transcript and tool events |

Subscriptions don't bill per attempt. A dollar figure estimates equivalent API cost. Claude Code reports an estimate. Codex reports none in the draft runner. Cost estimates depend on the price list under Not decided yet.

## Context

Record context where the provider gives evidence:

| Data point | Unit | How it is measured |
| --- | --- | --- |
| Peak context | Tokens and share of the window | Largest input of one model request |
| Total input | Tokens | Sum of request inputs |
| Context delivered | Unique and repeated bytes | Text reaching the agent, with Open Forge files and startup reads apart |
| Open Forge reads | Paths, bytes and read counts | Files the agent actually reads |
| Compactions | Count and size before and after | Provider records where reported |
| Lost progress | Requirements and percentage | A requirement passes during the run and fails at the end |

Lost progress uses saved snapshots as a proxy for forgetting. Snapshot capture for this measure is Not built yet. Capture depends on the snapshot schedule under Not decided yet. A short run without compaction tells us little about context degradation.

## User effort

Record these values without mixing them into coding usage:

| Data point | Unit | How it is measured |
| --- | --- | --- |
| Prompt | Words | Submitted task prompt |
| Questions | Decisions and rounds | Agent questions, with bundled decisions counted apart |
| Answers | Words and tokens | Simulated user replies and usage |
| Question-driven help | Opportunities asked, answered and used | Registered opportunities and transcript evidence |
| Avoidable effort | Questions already answered in the documents | Transcript review |
| Setup effort | Minutes and tokens | Setup record, counted once |

The [conversation protocol](protocol.md#simulated-user) defines answer limits and failures.

## Counting

Apply these rules to usage:

- Count each model request once, including helpers and retries.
- Take differences between running totals instead of adding the totals.
- Count cached input as input and context.
- For Claude Code, add `input_tokens`, `cache_read_input_tokens` and `cache_creation_input_tokens` for request input.
- For Codex, input already includes cached input.
- Store missing telemetry as `null` and report it as missing, never zero.
- Compare tokens within one tool and model, because tokenizers differ.
- Count simulated user usage as user effort and judge usage as assessment cost.
- Count screenshots, checks and ballots as evidence for an attempt, never extra attempts.

Failures, timeouts, blocked checks and infrastructure replacements follow [Grading](grading.md#counting). The [proposal's counting plan](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) keeps failed attempts and missing tokens in the result.

## Reporting

Report comparisons in these views:

- One table per tool and model, with runs, success intervals, coverage, wishes, rules, code, picks, time, tokens, peak context, compactions and lost progress.
- Each setup's difference from its comparison setup, with an interval.
- Tokens against coverage, one dot per attempt.
- Separate correctness, rules, cost, wishes and preference panels.

Keep quality and cost separate. There is no blended score. Publish regressions and inconclusive results. Current five-run comparisons support no public claim, as [Vision](vision.md#claims) defines. The [proposal's results views](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) define the published panels.

## Not decided yet

- The dated price list for API-cost estimates.
- The fixed analysis tools for code metrics.
- The snapshot schedule for lost progress.

Back to [Evals](_evals.md).
