---
open-forge:
  description: This workspace's own development, delivery, and deliberation methods
  tags: [Workflow, OpenForge]
---

# Open Forge Methods

## Which of this project's own methods fits the work?

These recipes are workspace-owned. They were maintained under the retired
`.agents/workflows/` root and moved here when Workflows stopped being a Core
category. They are not supplied by any Extension and are not updated by one.

Several share a name with a first-party recipe in a sibling scope — Architecture,
Vision, Planning, Debugging, Review, and Managed Delivery all exist in both
places. The versions here are this project's, and they have diverged. Reconcile
them deliberately rather than assuming either is current.

## Axioms

- Follow the [catalogue's recipe convention](../_references.md): one non-empty
  `## Goal`, `## Steps`, and `## Completion`, in that order.
- Change a method only when evidence shows that its risk profile no longer fits.
- Create one only when repeating its recipe significantly changes execution,
  preserves a deliberate user method, or improves reliability beyond normal agent
  behavior.
- Every direct sibling file is a complete recipe. An entrypoint may omit the
  recipe sections when it only organizes descendants.
- Steps may link to relevant sources, invoke capabilities, delegate bounded work,
  repeat based on evidence, or hand off to another recipe. State those
  relationships directly.

## Entries

- [Deliver development work through profile-based rigor, one persistent owner per coherent slice, bounded parallelism, and conditional review](adaptive-development.md) - #Workflow #Development #Adaptive #Implementation #Testing #Review #Delegation #Efficiency
- [Define or review a system's structure, placement, archetypes, invariants, tradeoffs, and transition in one accepted top-down model](architecture.md) - #Workflow #Architecture #Design #Planning
- [Obtain compact independent perspectives on one high-leverage uncertainty, resolve factual disagreement, and synthesize without voting](council.md) - #Workflow #Council #Collaboration #Decision #Ideation #Architecture #Review #Evidence #Efficiency
- [Reproduce and isolate a defect, test competing hypotheses efficiently, and verify an authorized cause-level fix](debugging.md) - #Workflow #Quality #Debugging #Efficiency
- [Deliver high-consequence custom behavior through explicit architecture, frozen critical boundaries, progressive evidence, conditional improvement passes, and one correction budget](development/_development.md) - #Workflow #Development #Assurance #Orchestration #Implementation #Testing #Refactoring #Review
- [Compatibility route for the former experimental program workflow; use Program Development as the current recipe](experimental-development.md) - #Workflow #Compatibility #Deprecated #Development #Program
- [Coordinate dependent tasks, preserve interrupted work, and verify authorized integration](managed-delivery.md) - #Workflow #Orchestration #Planning #Worktree #Integration
- [Turn accepted direction into an executable profile, compact execution capsule, dependency graph, parallel lanes, and evidence plan](planning.md) - #Workflow #Planning #Efficiency #Delegation
- [Accelerate a related development program by extracting archetypes, golden slices, shared foundations, delta packets, parallel lanes, and batch gates](program-development.md) - #Workflow #Development #Program #Orchestration #Planning #Implementation #Efficiency #Review #Batch
- [Review a bounded change or design with stable finding IDs, evidence-backed consequences, and no automatic duplicate review](review.md) - #Workflow #Quality #Review #Evidence #Efficiency
- [Trial a Luna/max Task Mastermind on one bounded preparation lane without changing the default development workflow](supervised-luna-preparation-trial.md) - #Workflow #Experimental #Development #Preparation #Delegation #Parallelism #Luna #Efficiency #Review
- [Define or challenge a subject's purpose, core value, first useful version, boundaries, and success before execution](vision.md) - #Workflow #Vision #Product
- [Deliver an explicitly authorized parallel project wave through isolated worktrees, hidden task ownership, bounded integration, and one project-level result](worktree-program-development.md) - #Workflow #Development #Project #Orchestration #Worktree #Parallelism #Integration #Review #Efficiency
