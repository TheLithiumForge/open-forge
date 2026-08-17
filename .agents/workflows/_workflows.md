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
- Choose a Workflow from its description, tags, route meaning, and user direction. After loading it, use `Goal` to confirm that it fits. Honor an explicit choice or opt-out.
- Choose the smallest Workflow that resolves an important missing decision or execution risk. Do not turn installed Workflows into mandatory stages or require users to name them.
- When a Workflow significantly changes the interaction, tell the user which approach is being used and why in one natural sentence. Internal route details are optional.

### Recipe Shape

- Create a Workflow only when repeating its recipe significantly changes execution, preserves a deliberate user method, or improves reliability beyond normal agent behavior.
- Every direct sibling Workflow file is a complete recipe.
- A complete recipe has one non-empty `## Goal`, `## Steps`, and `## Completion` section in that order.
- An `entrypoint` may omit recipe sections only when it organizes descendants.
- If a file uses any standard recipe section, it must include the complete recipe shape.
- Recipe-specific headings may add useful context without becoming part of the Framework schema.
- Steps may link to relevant sources, invoke capabilities, delegate bounded work, repeat based on evidence, or hand off to another Workflow. State these relationships directly.

## Entries

<!-- open-forge:generated-index:start -->
- [Deliver development work through one persistent primary owner with proportionate planning, implementation, testing, cleanup, and optional bounded delegation instead of mandatory phase separation](adaptive-development.md) - #Workflow #Development #Adaptive #Implementation #Testing #Review #Delegation
- [Define or review a system's structure, boundaries, tradeoffs, and transition](architecture.md) - #Extension #Workflow #Architecture #Design
- [Obtain independent perspectives on one high-leverage decision, resolve factual disagreement with evidence, and synthesize without voting or consensus pressure](council.md) - #Workflow #Council #Collaboration #Decision #Ideation #Architecture #Review #Evidence
- [Reproduce and isolate a defect, identify its root cause, and verify an authorized minimal fix](debugging.md) - #Extension #Workflow #Quality #Debugging
- [Deliver custom behavior through a Mastermind-owned strict cycle with read-only Preflight, complete evidence, bounded cleanup, a public gate, and one correction allowance](development/_development.md) - #Workflow #Development #Orchestration #Implementation #Testing #Refactoring #Review
- [Turn an accepted direction into an executable plan with explicit dependencies, decisions, and verification](planning.md) - #Extension #Workflow #Planning
- [Review a change, design, or repository state and report prioritized evidence-backed findings without modifying it by default](review.md) - #Extension #Workflow #Quality #Review
- [Define or challenge a subject's purpose, core value, first useful version, boundaries, and success before execution](vision.md) - #Extension #Workflow #Vision #Product
<!-- open-forge:generated-index:end -->
