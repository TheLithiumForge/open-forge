---
slug: /evals
title: Evals
---
# Evals
Open Forge Evals is a tool that compares the same coding task with and without Open Forge on the same implementer model.

For example, Neon Pairs gives each setup the same memory game request, freezes the finished files, then checks and judges those files. You can compare what passed with the time and tokens used.

## Layers

Each layer gives the implementer more information and adds checks to the earlier layers:

| Layer | Implementer receives | What gets checked |
| --- | --- | --- |
| 1, an idea | One task prompt | Playability and the stated request |
| 2, a defined idea | A player-facing subset of the spec | Every provided requirement |
| 3, a full spec | The full spec | Every executable clause, with review results apart |
| 4, brownfield | Starting code, project rules and a change request | The change and no regressions |

Layer 2 leaves board codes, snapshots and replay to Layer 3. Layer 4 starts from the reviewed reference solution to the full Layer 3 spec. A Layer 3 model attempt doesn't seed Layer 4.

## Setups

The current comparison uses four setups:

| Setup | Context arrangement |
| --- | --- |
| No setup | The control without Open Forge |
| Minimal | The Minimal Open Forge preset |
| Essentials | The Essentials Open Forge preset |
| Full Core | The Full Core Open Forge preset |

Every attempt gets fresh tool settings and an empty Memory. Frozen inputs and limits stay the same within a comparison. Helpful and Extension testing wait until later.

## Models

The implementer writes the app. The judge reads the saved source and images and answers fixed questions. The simulated user classifies the implementer's questions and returns literal answers from a frozen bank. It gives no coding advice.

Two implementers and two judges give four pairs. Each implementer produces an attempt once, then both judges read that same attempt. The second judge runs through the judge command, which appends its ballots. This lets you compare the judges on the same work. Saved plans currently use Claude Opus to implement and judge, with Claude Sonnet as the simulated user. Codex records already saved stay in history while they apply.

To choose role presets and run a saved plan, follow [Run evals](run.md).

## Grading

Checks test frozen files. Every check returns `pass`, `fail`, `error` or `blocked` with evidence. Coverage is checks passed divided by all applicable checks. A blocked check counts as not passed. A grader error needs another check on the saved files. If unresolved, it counts as not passed and appears in the error count.

The working gate passes only when every working check passes. Stated success passes only when every stated check passes. For Layer 1, `L1-v2` has 6 working checks, 16 stated checks and 3 wishes kept apart. Starting size, remembered size and no sound are wishes. Asking questions belongs to collaboration.

Judges cast three ballots per frozen yes, no or unsure question. Answers include file and line evidence. Rules, code, wishes and collaboration get separate scores. Unsure ballots stay counted. Human audits cover unsure answers, a registered random sample, split ballots, failed hidden checks and suspicious measurements. Blind human picks measure preference separately from correctness.

Failed attempts and timeouts stay in results. Infrastructure failures can get one replacement under the registered rule. Both attempts stay recorded. The check and judge commands append assessments with their grading versions.

You can read coverage, gates, wishes, judge answers and usage in [Results](results.mdx). You can compare CLI versions and the reference data point in [History](history.mdx).

## Limits

These are screening results. Every Open Forge CLI stays beta. Nothing is a claim yet. Current full plans use three runs per setup, and each case runs at most five times per setup, model and tool. These samples can't establish the future evidence requirements for a benefit or non-inferiority.

Whole-game success and the working gate are descriptive results. Intervals describe screening variation. They don't establish a causal benefit. Different benchmark inputs, implementers, judges, tools and grading versions stay in separate comparison groups. Missing versions or reported models keep a batch apart from comparisons across batches.

Missing tokens stay missing. Codex reports no dollar cost. Claude Code's cost is its estimate. Implementation usage comes from one attempt and isn't added again for each judge.
