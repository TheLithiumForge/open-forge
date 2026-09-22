---
name: use-workflow
description: Select and follow an installed workflow for project vision, architecture, planning, implementation, debugging, review, or coordinated delivery. Use when the user requests a workflow or an installed recipe would materially improve the task. Recipes can combine the workspace's configured skills, tools, and agents; this skill does not supply their runtime.
---

# Use Workflow

Choose a method when it helps. Once selected, follow its required steps and completion conditions within the user's authority and the workspace's applicable rules.

## Select The Method

1. Read the [workflow catalogue](references/_references.md). Use its descriptions and scopes to find the requested method. Honor an explicit selection or opt-out.
2. Load only the relevant scope entrypoints and the selected recipe. Check its `Goal` before proceeding. If several methods fit, prefer the smallest one that addresses the actual uncertainty or execution risk.
3. Work directly when no installed recipe adds value. A missing or deliberately removed recipe is not permission to reinstall it.

Do not read every recipe to choose one. The catalogue is navigation, not a list of stages to execute.

## Follow The Recipe

- Read the selected recipe's required context before acting. Use the workspace's configured Skills, tools, agents, task sources, and verification procedures. Keep their native invocation and permission rules.
- Execute required steps; take conditional branches only when their conditions hold. Optional steps stay optional. Do not skip a required step merely because a shorter method seems sufficient after selection.
- Resolve missing capabilities before the step that needs them. Use a substitute only when it is permitted and preserves the requirement. Otherwise, report the blocked step and continue only independent authorized work.
- A workflow may call several Skills or another workflow. Keep the caller's outcome and return point clear. Do not restart completed work or recurse into the same unchanged request.
- When the method materially changes how the user will experience the work, explain the approach in one natural sentence. Do not require them to learn internal route names.

## Resume And Finish

Use the existing working record when the task needs durable state. Record the selected recipe, current step, relevant evidence, blockers, and next action only when they are needed to resume. Do not create a record for every invocation.

On resumption, compare the recorded state with the actual work and current recipe. Reconcile consequential changes before continuing; preserve evidence that still applies.

Check `Completion` against the actual result. Report unmet conditions and unavailable evidence. Finishing a recipe does not authorize new scope, accept its own output, or grant permission to commit, merge, publish, or contact external systems.
