---
open-forge:
  description: Repeatable Markdown recipes for reaching a defined goal
  tags: [LoadNow, Core, Workflow]
---

# Workflows

Workflows are optional routed Markdown recipes for reaching defined goals.

## Axioms

- Use `Entries` when the user selects a Workflow or a visible `description` shows that an installed recipe would materially help the current goal. Direct execution remains valid when no Workflow adds value.
- Select a candidate Workflow from its visible `description`, tags, `route` meaning, and current user direction. After loading it, use `Goal` to confirm the fit. Honor an explicit choice or opt-out.
- Select the smallest Workflow that resolves a material missing decision or execution risk. Do not treat installed Workflows as mandatory stages or require users to name them.
- When a Workflow materially changes the interaction, tell the user what useful approach is being applied and why in one natural sentence. Keep internal `route` details optional.
- Create a Workflow only when repeating its recipe materially changes execution, preserves a deliberate user methodology, or improves reliability beyond ordinary capable-agent behavior.
- Every non-`entrypoint` Workflow file is a complete recipe with one non-empty `## Goal`, `## Steps`, and `## Completion` section in that order. An `entrypoint` may omit recipe sections only when it organizes descendants. Declaring any standard recipe section requires the complete applicable contract.
- A Workflow with unconditional routed dependencies puts one `## Required Routes` section between `Goal` and `Steps`. Each dependency uses `- [description](relative/path.md) - #Tags` and resolves relative to the Workflow file. Read every linked `route` before Step 1 and report an unreadable dependency as a blocker. Omit the section when no unconditional routed dependency applies.
- Recipe-specific headings may add useful context without becoming part of the Framework schema.
- Steps may invoke capabilities, delegate bounded work, repeat based on evidence, or hand off to another Workflow. Keep composition explicit in the recipe.

## Entries

<!-- open-forge:generated-index:start -->
- [Define or review a system's structure, tradeoffs, boundaries, and adoptable transition](architecture.md) - #Extension #Workflow #Architecture #Design
- [Reproduce and isolate a defect, identify its root cause, and verify an authorized minimal fix](debugging.md) - #Extension #Workflow #Quality #Debugging
- [Deliver an accepted change through implementation, proportionate verification, focused improvement, and cause-level failure handling](development.md) - #Extension #Workflow #Development #Implementation #Testing #Refactoring
- [Turn an accepted direction into an executable plan with explicit dependencies, decisions, and verification](planning.md) - #Extension #Workflow #Planning
- [Review a change, design, or repository state and report prioritized evidence-backed findings without modifying it by default](review.md) - #Extension #Workflow #Quality #Review
- [Define or challenge a subject's purpose, core value, first useful version, boundaries, and success before execution](vision.md) - #Extension #Workflow #Vision #Product
<!-- open-forge:generated-index:end -->
