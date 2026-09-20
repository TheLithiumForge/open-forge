---
open-forge:
  description: Current maintenance contract for the installable Workflows entrypoint and recipe shape
  responsibility: Preserve Workflow selection, recipe validation, cross-primitive composition, source alignment, and `route` boundaries
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Workflow]
---

# Workflows Category Maintenance Contract

## Source

[`src/open-forge/.agents/workflows/_workflows.md`](../../../../../../../src/open-forge/.agents/workflows/_workflows.md) is the canonical installed Workflows `entrypoint`. The repository [Workflows `entrypoint`](../../../../../../skills/use-workflow/references/open-forge/_open-forge.md) dogfoods the same authored contract and may add local generated `entries`.

An adjacent [workspace overwrite](../../../../../../skills/use-workflow/references/open-forge/_open-forge.md) adds the accepted local condition: change a Workflow only when evidence shows that its risk profile no longer fits.

The [current Workflow contract](../../../framework/primitives/workflows.md) defines recipe structure, selection, composition, and relationships with other Core primitives.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Workflow so Workflow selection enters baseline context
- The entrypoint keeps the complete compact runtime and manual authoring rules
- Visible descriptions, tags, route meaning, and user direction select a candidate before loading. `Goal` confirms the fit after loading
- The smallest Workflow that resolves an important missing decision or execution risk is preferred. Installed Workflows never become mandatory stages, and users do not need to name them
- When a Workflow significantly changes the interaction, the agent explains the approach in one natural sentence without making internal route details part of the interaction
- The authored rules prevent normal agent behavior from becoming ceremonial Workflows
- A complete Workflow has one non-empty level-2 `Goal`, `Steps`, and `Completion` section in that order
- Recipe-specific headings remain valid without becoming Framework schema
- Organizational entrypoints remain valid without recipe sections. Using any standard recipe section requires the complete recipe shape
- Workflow scopes inherit the Workflow role and may nest recursively
- Other Core `root routes` remain separate and enter recipes through Steps, handoffs, or ordinary links
- A child with a familiar primitive `slug` beneath Workflows does not receive that primitive's runtime or managed tooling behavior
- The installable source begins with no opinionated Workflow recipes

## Optional Recipes And Local Profiles

The [Extension catalogue](../../../../../../../src/extensions/README.md) identifies installable recipes and their package dependencies. Core retains the compact category contract. Local workflows may preserve accepted repository-specific execution profiles without making their agents, model choices, budgets, or tooling portable defaults.

Keep each recipe explicit about selection, required inputs, sequence, stopping or recovery conditions, and observable completion. An incomplete investigation or withheld integration must remain visible as such. Check that related recipes link to their defining method instead of duplicating it.

## Verification

- Workflow validation tests cover required section level and order, recipe-specific headings, organizational `entrypoints`, direct Workflow files, and `root route` boundaries
- Core installation and route tests verify baseline loading, indexing, and recursive Workflow categories
- Package verification assembles declared dependencies, checks links at installed destinations, validates recipe sections and generated indexes, and checks local parity where shared content is intended
- Review specialized and experimental recipes individually. Record why each is revised, retained locally, or promoted; do not infer effectiveness from its existence
