---
open-forge:
  description: Checks per layer, check statuses, counting rules, model judge and blind human audits
  tags: [Memory, Document, CurrentTruth, Evaluation, Grading, Judging, Measurement]
---

# Eval Grading

Eval Grading is the set of checks and reviews that scores frozen attempts without mixing game results with grader failures.

Status: approved on 2026-11-01. The complete grading system is Not built yet.

Each layer keeps the earlier checks and adds stricter ones as more is stated. For example, Layer 2 tests the provided size rules through play, while Layer 3 can use its required board codes and replay command. The [proposal's grading rules](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) define these checks.

## Layer checks

The approved check plan is:

| Layer | Check set | Success |
| --- | --- | --- |
| 1 | `L1-v2`, 6 working checks, 16 stated checks and 3 wishes apart | Working gate passes all 6. Stated success passes all 16 |
| 2 | About 48 checks, Layer 1's 16 plus about 32 from provided clauses | Every provided clause passes |
| 3 | About 160 assertions across 94 requirement IDs | Every executable clause passes, with review clauses apart |
| 4 | Full Layer 3 suite, 10 to 20 change checks and 3 guards | Change works and no earlier check fails |

Layer 1's working checks cover the built page opening offline, finishing a 4x4 game by pointer, phone tap and keyboard, showing the win and New Game clearing the board and moves. All run without an uncaught error. Starting size, remembered size and no sound are wishes. Asking belongs to collaboration. Earlier G1 records keep their grading version.

Layer 2 adds sizes, saved size, mismatch closing, exact moves, best scores, reset, keyboard movement, accessible names, the stated look and delivery. Controlled clocks test stated timing. Layers 1 and 2 find cards through play without requiring an unstated hook.

Layer 3 uses board codes for known deals, snapshots for states and replay for exact moves. Static checks cover packages, compiler settings, sizes and source rules. Architecture and legibility stay review questions. An unsure review leaves compliance pending.

Layer 4 first confirms that the reference passes. For a repair, it also confirms that the planted defect fails its checks. The three guards check grading inputs, compatibility and diff scope. Replacing an earlier expectation needs a published requirement change. The [proposal's layer details](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) define these rules. Finished later-layer packs are Not built yet.

## Statuses

Every check returns evidence with one status:

| Status | Meaning |
| --- | --- |
| `pass` | Every assertion for the check passes |
| `fail` | The submitted result breaks an assertion |
| `error` | The grader couldn't produce a result |
| `blocked` | An earlier failure stopped the check |

Split each rule into clauses. A clause passes only when all its assertions pass. Before freezing a check set, confirm that it passes on the reference and fails on known broken copies. A retry keeps the first outcome. The [proposal's check contract](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) defines this behavior.

## Counting

Count each check once. Coverage is checks passed divided by all applicable checks. Apply these rules:

- Count `blocked` as not passed.
- Grade an `error` again on saved files without a new agent run. If unresolved, count it as not passed and report the error count.
- Keep attempts that fail, time out or build nothing, with what they passed.
- Replace an infrastructure failure with one new attempt under the registered rule. Record both attempts.
- Report missing token counts as missing in the token result.

A grader error is never a game failure. Neither status counts as a pass. A judge failure never lowers a check result. Functional success, pipeline completion, rules, code, wishes and collaboration stay separate. The [proposal's counting rules](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) define these distinctions.

## Judge

Use one pinned judge model, effort and rubric within a batch. The judge answers frozen yes, no or unsure questions with file and line evidence. It casts three ballots per question. The planned question counts are about 8 at Layer 1, 10 at Layer 2, 12 at Layer 3 and Layer 3's questions plus 4 at Layer 4.

Use the strongest available model at its highest effort, at least as strong as the agents. Same-family judging is allowed within one tool and model comparison. The shared [evidence contract](protocol.md#judge-evidence) keeps both providers' inputs equal.

Score rules, code and wishes apart. Score collaboration from the transcript separately. A question opportunity scores only when the agent asked, got the answer and used it. Question count earns nothing. Asking about a choice the documents answer adds avoidable user effort. The [proposal's question rules](../../../emerging/analysis/eval-product-proposal.md#questions-and-the-helpful-setup) define this distinction.

The Helpful planning result waits with Extension testing under the [approval](../../../emerging/analysis/eval-product-proposal.md#approved).

## Human audit

Audit every unsure answer and a registered random sample, blind. Also check split ballots, failed hidden checks and suspicious measurements to tell game failures from grading failures. The [proposal's audit rule](../../../emerging/analysis/eval-product-proposal.md#run-protocol) defines the required blind review.

People can also play pairs from the same tool and model at desktop or phone width. Randomize the sides and keep setups and results hidden until Reveal. Allow a tie. Picks measure preference and agreement with the judge. They don't set the correctness result. The saved pick is an assessment as described in [Results](results.md#records).

## Not decided yet

- Exact later-layer check sets and rubrics. Freeze them before those scored runs.
- The human audit sample size. Register it before the batch.
- Who besides the maintainer makes blind picks and how their picks are collected.

Back to [Evals](_evals.md).
