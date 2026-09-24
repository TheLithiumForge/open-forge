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
- The entrypoint directs agents to check Patterns when creating, changing, or reviewing something with visible structure
- Every routed Pattern defines one recognizable reusable shape and enough scope to judge relevance
- A new Pattern requires an accepted shape that remains useful for future related work
- One-off work, temporary transitions, and unsettled candidates do not become Patterns only because they have structure
- A Pattern stays focused on one reusable shape
- Concrete examples remain valid against their APIs, formats, and tools. Intentionally incomplete examples identify themselves as schematic
- An applicable Pattern remains the default shape. Deliberate adaptations are allowed within granted authority; explain material departures before dependent work. A mandatory shape requires authority for its exception
- Binding shape requirements come from a Directive, an active Axiom, or an accepted requirement rather than Pattern classification alone
- Child routes may narrow or preserve positive scope through the ordinary routing contract
- The installable source begins with no opinionated Pattern files

## Verification

- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Route tests verify arbitrary-depth child entrypoints and generated-region integrity
