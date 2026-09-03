---
open-forge:
  description: Repeatable Markdown recipes for reaching a defined goal
  tags: [LoadNow, Core, Workflow]
---

# Workflows

Workflows are optional Markdown recipes for reaching defined goals.

## Axioms

### Selection And Use

- Check `Entries` when the user selects a Workflow or its visible description shows that the recipe would significantly help the current goal. Work directly when no Workflow adds value.
- Choose the smallest Workflow that resolves an important missing decision or execution risk. Do not turn installed Workflows into mandatory stages or require users to name them.
- Honor an explicit choice or opt-out. Change workflow only when evidence shows that its risk profile no longer fits.
- When a Workflow significantly changes execution, tell the user which approach is being used and why in one natural sentence.

### Recipe Shape

- Create a Workflow only when repeating its recipe significantly changes execution, preserves a deliberate method, or improves reliability beyond normal behavior.
- Every direct sibling Workflow file is a complete recipe.
- A complete recipe has one non-empty `## Goal`, `## Steps`, and `## Completion` section in that order.
- An `entrypoint` may omit recipe sections only when it organizes descendants.
- If a file uses any standard recipe section, it must include the complete recipe shape.
- Steps may link to relevant sources, invoke capabilities, delegate bounded work, repeat based on evidence, or hand off to another Workflow. State these relationships directly.

## Entries

<!-- open-forge:generated-index:start -->
- [Deliver development work through profile-based rigor, one persistent owner per coherent slice, bounded parallelism, and conditional review](adaptive-development.md) - #Workflow #Development #Adaptive #Implementation #Testing #Review #Delegation #Efficiency
- [Define or review a system's structure, placement, archetypes, invariants, tradeoffs, and transition in one accepted top-down model](architecture.md) - #Extension #Workflow #Architecture #Design #Planning
- [Obtain compact independent perspectives on one high-leverage uncertainty, resolve factual disagreement, and synthesize without voting](council.md) - #Workflow #Council #Collaboration #Decision #Ideation #Architecture #Review #Evidence #Efficiency
- [Reproduce and isolate a defect, test competing hypotheses efficiently, and verify an authorized cause-level fix](debugging.md) - #Extension #Workflow #Quality #Debugging #Efficiency
- [Deliver high-consequence custom behavior through explicit architecture, frozen critical boundaries, progressive evidence, conditional improvement passes, and one correction budget](development/_development.md) - #Workflow #Development #Assurance #Orchestration #Implementation #Testing #Refactoring #Review
- [Compatibility route for the former experimental program workflow; use Program Development as the current recipe](experimental-development.md) - #Workflow #Compatibility #Deprecated #Development #Program
- [Turn accepted direction into an executable profile, compact execution capsule, dependency graph, parallel lanes, and evidence plan](planning.md) - #Extension #Workflow #Planning #Efficiency #Delegation
- [Accelerate a related development program by extracting archetypes, golden slices, shared foundations, delta packets, parallel lanes, and batch gates](program-development.md) - #Workflow #Development #Program #Orchestration #Planning #Implementation #Efficiency #Review #Batch
- [Review a bounded change or design with stable finding IDs, evidence-backed consequences, and no automatic duplicate review](review.md) - #Extension #Workflow #Quality #Review #Evidence #Efficiency
- [Trial a Luna/max Task Mastermind on one bounded preparation lane without changing the default development workflow](supervised-luna-preparation-trial.md) - #Workflow #Experimental #Development #Preparation #Delegation #Parallelism #Luna #Efficiency #Review
- [Define or challenge a subject's purpose, core value, first useful version, boundaries, and success before execution](vision.md) - #Extension #Workflow #Vision #Product
- [Deliver an explicitly authorized parallel project wave through isolated worktrees, hidden task ownership, bounded integration, and one project-level result](worktree-program-development.md) - #Workflow #Development #Project #Orchestration #Worktree #Parallelism #Integration #Review #Efficiency
<!-- open-forge:generated-index:end -->
