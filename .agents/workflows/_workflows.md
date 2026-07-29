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
- Create a Workflow only when repeating its recipe materially changes execution, preserves a deliberate user methodology, or improves reliability beyond ordinary capable-agent behavior.
- Every non-`entrypoint` Workflow file is a complete recipe with one non-empty `## Goal`, `## Steps`, and `## Completion` section in that order. An `entrypoint` may omit recipe sections only when it organizes descendants. Declaring any standard recipe section requires the complete applicable contract.
- A Workflow with unconditional routed dependencies puts one `## Required Routes` section between `Goal` and `Steps`. Each dependency uses `- [description](relative/path.md) - #Tags` and resolves relative to the Workflow file. Read every linked `route` before Step 1 and report an unreadable dependency as a blocker. Omit the section when no unconditional routed dependency applies.
- Recipe-specific headings may add useful context without becoming part of the Framework schema.
- Steps may invoke capabilities, delegate bounded work, repeat based on evidence, or hand off to another Workflow. Keep composition explicit in the recipe.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
