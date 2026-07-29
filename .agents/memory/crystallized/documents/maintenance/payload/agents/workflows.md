---
open-forge:
  description: Current maintenance contract for the installable Workflows `entrypoint` and minimal validated Markdown recipe schema
  responsibility: Preserve Workflow selection, recipe validation, dependency, cross-primitive composition, source alignment, and `route` boundaries
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Workflow]
---

# Workflows Category Maintenance Contract

## Source

[`src/open-forge/.agents/workflows/_workflows.md`](../../../../../../../src/open-forge/.agents/workflows/_workflows.md) is the canonical installed Workflows `entrypoint`. The repository [Workflows `entrypoint`](../../../../../../workflows/_workflows.md) dogfoods the same authored contract and may add local generated `entries`.

The [current Workflow contract](../../../framework/primitives/workflows.md) defines recipe structure, selection, dependencies, composition, and relationships with other Core primitives.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Workflow so Workflow selection enters baseline context
- The `entrypoint` keeps the compact complete runtime and manual authoring contract
- Visible `description` values, tags, `route` meaning, and current user direction select a candidate before loading. `Goal` confirms fit after loading
- The authored contract states the admission threshold that prevents ordinary capable-agent behavior from becoming ceremonial Workflows
- A complete Workflow has one non-empty level-2 `Goal`, `Steps`, and `Completion` section in that order
- `Required Routes` is optional, appears between `Goal` and `Steps`, and contains at least one canonical routed link when present
- Recipe-specific headings remain valid without becoming Framework schema
- Organizational `entrypoints` remain valid without recipe sections. Declaring any standard recipe section requires the complete applicable schema
- Generated `Entries` express containment and `Required Routes` express unconditional cross-tree dependencies
- Workflow scopes inherit the Workflow role and may nest recursively
- Other Core `root routes` remain separate and enter recipes through `Required Routes`, Steps, handoffs, or ordinary links
- A child with a familiar primitive `slug` beneath Workflows does not receive that primitive's runtime or managed tooling behavior
- The installable source begins with no opinionated Workflow recipes

## Verification

- Workflow validation tests cover required section level and order, optional `Required Routes`, recipe-specific headings, organizational `entrypoints`, direct Workflow files, and `root route` boundaries
- Required Route tests cover relative resolution, tags, missing dependencies, containment, and canonical syntax
- Core installation and route tests verify baseline loading, indexing, and recursive Workflow categories
