---
open-forge:
  description: Current maintenance contract for the installable Guidance Core category entrypoint
  responsibility: Preserve Guidance selection, advisory semantics, recursive scope, source alignment, and deterministic route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Guidance]
---

# Guidance Category Maintenance Contract

## Source

[`src/open-forge/.agents/guidance/_guidance.md`](../../../../../../../src/open-forge/.agents/guidance/_guidance.md) is the canonical installed Guidance entrypoint. The repository [Guidance entrypoint](../../../../../../guidance/_guidance.md) dogfoods the same authored contract and may add local generated entries.

The [current Guidance document](../../../framework/primitives/guidance.md) defines its advisory authority, contextual adaptation, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Guidance so the category and its selection rule enter baseline context
- The entrypoint directs agents to look for Guidance when a recurring scenario or choice may have an established approach
- Routed Guidance identifies its scenario, preferred approach, reasoning, and tradeoffs
- Guidance remains advisory and requires the reason for a context-driven adaptation to stay visible
- Child routes may narrow or preserve positive scope through the ordinary routing contract
- The installable source begins with no opinionated Guidance files

## Verification

- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Route tests verify arbitrary-depth child entrypoints and generated-region integrity
