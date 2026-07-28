---
open-forge:
  description: Current maintenance contract for the installable Workflows Core category `entrypoint` and validated Markdown recipe schema
  responsibility: Preserve Workflow selection, recipe validation, dependency, cross-primitive composition, source alignment, and `route` boundaries
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Workflow]
---

# Workflows Category Maintenance Contract

## Source

[`src/open-forge/.agents/workflows/_workflows.md`](../../../../../../../src/open-forge/.agents/workflows/_workflows.md) is the canonical installed Workflows `entrypoint`. The repository [Workflows `entrypoint`](../../../../../../workflows/_workflows.md) dogfoods the same authored contract and may add local generated `entries`.

The [current Workflow contract](../../../framework/primitives/workflows.md) defines recipe structure, selection, dependencies, composition, and relationships with other Core primitives.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Workflow so Workflow selection enters baseline context
- The `entrypoint` keeps only runtime-facing selection behavior; authoring detail remains in the current contract, Templates, scaffolding, and deterministic validation
- A complete Workflow keeps the ordered `Mode`, `Goal`, `Required Routes`, `Constraints`, `Steps`, `Loop`, `Outputs`, and `Completion` contract
- `Mode`, empty Constraints, optional helpful prior work, primary phase, and Required Routes retain the meanings defined by the current contract
- Organizational `entrypoints` remain valid without recipe sections; declaring any recipe section requires the complete schema
- Generated `Entries` express containment and `Required Routes` express unconditional cross-tree dependencies
- Workflow scopes inherit the Workflow role and may nest recursively
- Other Core `root routes` remain separate and enter recipes through `Required Routes`, Steps, handoffs, or ordinary links
- A child with a familiar primitive `slug` beneath Workflows does not receive that primitive's runtime or managed tooling behavior
- The installable source begins with no opinionated Workflow recipes

## Verification

- Workflow validation tests cover section level and order, Mode, Constraints, phase classification, `Required Routes`, organizational `entrypoints`, and `root route` boundaries
- Required Route tests cover relative resolution, tags, missing dependencies, containment, and canonical syntax
- Core installation and route tests verify baseline loading, indexing, and recursive Workflow categories
