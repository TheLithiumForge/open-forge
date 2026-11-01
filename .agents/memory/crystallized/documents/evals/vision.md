---
open-forge:
  description: What Open Forge Evals must show, the limits on public claims, the four layers and the separate comparison axes
  tags: [Memory, Document, CurrentTruth, Evaluation, Vision]
---

# Eval Vision

Open Forge Evals is a product that compares coding results with and without Open Forge on the same tool and model.

Status: approved on 2026-11-01. The complete product is Not built yet.

We need to learn whether Open Forge is at least as good as No setup and whether it keeps its edge as models improve. For example, one Neon Pairs case compares the same task on Codex with No setup and Essentials. The [approved proposal](../../../emerging/analysis/eval-product-proposal.md#goal-and-claims) defines the goal.

## Claims

We make no public claim about Open Forge from the current comparisons. Every published CLI stays beta until the evidence supports a claim. Whole-game success and the working gate are descriptive results.

The proposed future Layer 1 claim needs both the share of stated checks passed within 10 percentage points of No setup and the geometric mean of coding tokens per attempt within 20%. Essentials against No setup is the primary comparison. Other presets are secondary, with a Holm adjustment. A measured benefit is needed before saying Open Forge is a good addition. These are future evidence requirements, not conclusions from five runs. The [proposal's sample plan](../../../emerging/analysis/eval-product-proposal.md#how-many-runs) records the margins and methods.

## Layers

Each layer gives the agent more stated information and adds checks to the earlier layers:

| Layer | Agent receives | Result being tested |
| --- | --- | --- |
| 1, an idea | One task prompt | Playability and the stated request |
| 2, a defined idea | A player-facing subset of the spec | Every provided requirement |
| 3, a full spec | The full spec | Every executable clause, with review results apart |
| 4, brownfield | Starting code, project rules and a change request | The change and no regressions |

Layer 2 leaves board codes, snapshots and replay to Layer 3. Layer 4 starts from a council-built reference solution to the full Layer 3 spec. Code from a Layer 3 run doesn't seed Layer 4. The [proposal's layer scope](../../../emerging/analysis/eval-product-proposal.md#grading-per-layer) and its approval define these choices.

## Comparison axes

Keep these axes separate:

| Axis | Meaning | Example |
| --- | --- | --- |
| Project | The project being built or changed | Neon Pairs |
| Layer | How much of the task is stated | Layer 3, a full spec |
| Change type | The brownfield work requested | Feature, repair or feature plus repair |
| Setup | The instructions and context arrangement | No setup or Essentials |

A richer-context brownfield case is a Layer 4 variant, not another layer. The [proposal's document scopes](../../../emerging/analysis/eval-product-proposal.md#open-forge-documents) carry forward this separation from the council review.

## Current work

Each case runs five times per setup, model and tool. The current setups are No setup, Minimal, Essentials and Full Core. These runs screen and compare. They can't support the future margins.

We build the production-ready product with Layer 1 tested end to end first. Then we write the Layer 2, 3 and 4 documents. Their descriptions here define the approved direction, and their finished packs are Not built yet.

Helpful and Extension testing are a later part of Task 58. The Helpful planning result, the Plan-Ahead Skill idea and the Task 90 dependency wait with them. The [approval](../../../emerging/analysis/eval-product-proposal.md#approved) overrides the earlier sample size and delivery schedule.

Back to [Evals](_evals.md).
