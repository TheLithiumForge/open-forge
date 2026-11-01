---
open-forge:
  description: Open Forge Evals goals, product design, run protocol, grading, measurements and published results
  tags: [Memory, Document, CurrentTruth, Evaluation]
---

# Evals

Open Forge Evals is a product that compares the same coding task across setups on one tool and model.

Status: approved direction on 2026-11-01. The complete product is Not built yet.

For example, a Neon Pairs case runs five times per setup with the same prompt and model, then reports checks and usage apart. The [approved proposal](../../../emerging/analysis/eval-product-proposal.md#approved) sets the scope for [Task 58](../../../working/cli-development/tasks/task58-demo-evals.md).

## Documents

The [Vision](vision.md) defines the goals, four layers, comparison axes and limits on public claims. The current work compares No setup, Minimal, Essentials and Full Core. Helpful and Extension testing wait until later in Task 58.

The [Architecture](architecture.md) defines the product under `src/evals/`, its three images and stage contracts. The [Protocol](protocol.md) defines attempts, the simulated user, fair comparison and pre-registration.

The [Grading](grading.md) defines checks, statuses, the judge and human audits. The [Data Points](data-points.md) defines measurements and usage counting.

The [Running Evals](running-evals.md) guide covers commands, prerequisites, sign-in and recovery. The [Results](results.md) defines versioned records, evidence storage and publication.

The [Neon Pairs](neon-pairs/_neon-pairs.md) pack is the editable game specification the cases use. Its Layer 1 document now covers the Greenfield Baseline. The folder also defines Layers 2, 3 and 4.

Back to [Documents](../_documents.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [The approved Evals product folders, three images, container rules and stage inputs and outputs](architecture.md) - #Memory #Document #CurrentTruth #Evaluation #Architecture #Harness #Isolation
- [The measurements every eval records, their units, counting rules and separate result panels](data-points.md) - #Memory #Document #CurrentTruth #Evaluation #Measurement #Context #Tokens
- [Checks per layer, check statuses, counting rules, model judge and blind human audits](grading.md) - #Memory #Document #CurrentTruth #Evaluation #Grading #Judging #Measurement
- [Editable Neon Pairs specification and frozen pack generation](neon-pairs/_neon-pairs.md) - #Memory #Document #CurrentTruth #Evaluation #Game #Specification
- [The shared run steps, simulated user, judge evidence, fair comparison and pre-registration rules](protocol.md) - #Memory #Document #CurrentTruth #Evaluation #Protocol #Conversation
- [The public results repository, result versions, schema v2 records, evidence assets and publishing flow](results.md) - #Memory #Document #CurrentTruth #Evaluation #Results #History #Publishing
- [The approved eval commands, prerequisites, provider sessions, sign-in, setup preparation and recovery](running-evals.md) - #Memory #Document #CurrentTruth #Evaluation #Harness #Isolation
- [What Open Forge Evals must show, the limits on public claims, the four layers and the separate comparison axes](vision.md) - #Memory #Document #CurrentTruth #Evaluation #Vision

