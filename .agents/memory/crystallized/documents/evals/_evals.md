---
open-forge:
  description: Open Forge evals, what each one compares, how we run it and which data points it records
  tags: [Memory, Document, CurrentTruth, Evaluation]
---

# Evals

This scope holds Open Forge's evals. Each eval runs the same prompt under different setups on one tool and model, and records the same data points, so the setups can be compared.

The [Eval Ladder](../../../emerging/ideas/eval-ladder.md) orders the planned evals, from a prompt-only greenfield project up to brownfield. It isn't accepted yet. [Task 58](../../../working/cli-development/tasks/task58-demo-evals.md) tracks the work.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [The values every Open Forge eval records for each run, and how we get, count, judge and report each one](data-points.md) - #Memory #Document #CurrentTruth #Evaluation #Measurement #Context #Tokens
- [The first Open Forge eval, one short prompt for Neon Pairs, a small React memory game, run in an empty folder, in fresh Open Forge installs and with the Collaboration extension, checking that Open Forge is not worse](greenfield-baseline.md) - #Memory #Document #CurrentTruth #Evaluation #Greenfield #Baseline #Demo
- [Who judges each Open Forge eval data point, why the headline numbers come only from checks and counts, how the model judge and blind pairwise picks work, and how anyone can rerun them](judging.md) - #Memory #Document #CurrentTruth #Evaluation #Judging #Measurement
- [How every Open Forge eval run starts, runs and ends in isolation on Claude Code and Codex, how the eval tool drives each CLI on a subscription, and what it captures](running-evals.md) - #Memory #Document #CurrentTruth #Evaluation #Harness #Isolation

