---
open-forge:
  description: Current maintenance contract for the installable Patterns Core category entrypoint
  responsibility: Preserve Pattern selection, inspectable-shape semantics, recursive scope, source alignment, and deterministic route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Pattern]
---

# Patterns Category Maintenance Contract

## Source

[`src/open-forge/.agents/patterns/_patterns.md`](../../../../../../../src/open-forge/.agents/patterns/_patterns.md) is the canonical installed Patterns entrypoint. The repository [Patterns entrypoint](../../../../../../patterns/_patterns.md) dogfoods the same authored contract and may add local generated entries.

The [current Patterns document](../../../framework/primitives/patterns.md) defines continuing structural reference, deliberate variation, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Pattern so the category and its selection rule enter baseline context
- The entrypoint directs agents to inspect Patterns when creating, changing, or reviewing an inspectable result
- Every routed Pattern defines a recognizable reusable shape and enough positive scope to judge relevance
- An applicable Pattern remains the established default shape while a deliberate alternative stays possible
- Binding shape requirements use a Directive or another active Axiom rather than silently changing Pattern authority
- Child routes may narrow or preserve positive scope through the ordinary routing contract
- The installable source begins with no opinionated Pattern files

## Verification

- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Route tests verify arbitrary-depth child entrypoints and generated-region integrity
