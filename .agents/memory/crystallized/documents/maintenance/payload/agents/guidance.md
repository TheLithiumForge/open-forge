---
open-forge:
  description: Current maintenance contract for the installable Guidance Core category and its optional Collaboration Extension
  responsibility: Preserve Guidance selection, advisory semantics, standard content, recursive scope, source alignment, and deterministic route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Guidance]
---

# Guidance Category Maintenance Contract

## Source

[`src/open-forge/.agents/guidance/_guidance.md`](../../../../../../../src/open-forge/.agents/guidance/_guidance.md) is the exact installed Guidance entrypoint. [`adaptive-collaboration.md`](../../../../../../../src/extensions/collaboration/content/.agents/guidance/adaptive-collaboration.md) defines how to match exploration depth to the decision, reveal detail progressively, integrate accepted outcomes, and offer useful independent review. The repository [Guidance route](../../../../../../guidance/_guidance.md) keeps the same category rules with local Entries.

The [current Guidance document](../../../framework/primitives/guidance.md) defines its advisory authority, contextual adaptation, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #Core and #Guidance without a loading tag. Guidance is optional and on demand, with no dependency from other categories.
- The entrypoint directs agents to check for Guidance when a recurring situation or choice may have an established approach
- Routed Guidance explains its situation, recommended approach, reasons, and tradeoffs
- Guidance remains advice. Explain an adaptation when the difference matters to the result
- Child routes may narrow or preserve positive scope through the ordinary routing contract
- Core ships the Guidance category. The optional Collaboration Extension owns Adaptive Collaboration at its existing installed path and a Brainstorming Template. It adds no Memory category or dependency on Planning
- Adaptive Collaboration keeps the first response conversational unless deep analysis was requested, works through one important open choice at a time, and offers deeper detail before supplying it automatically
- At convergence, Adaptive Collaboration separates accepted and unsettled material, then identifies the question each accepted part answers, where it applies, and how long it should last. It asks the user only when uncertainty could change the result
- For broad, important, difficult-to-reverse work or changes spanning several durable knowledge roles, Adaptive Collaboration may offer a fresh read-only review when the likely value justifies the model cost
- The offer identifies the review focus and token cost and requires user approval unless standing direction already grants it
- Context-isolated reviewers receive the accepted goal, rules, workspace, and resulting changes, then find relevant sources independently. A review without context isolation is called an adversarial second pass

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for the category and optional Extension bytes and metadata rendering.
- Compare the category rules and Adaptive Collaboration with their repository counterparts. Check that Guidance stays advisory and on demand in the shipped loader.
- Review the Collaboration manifest and content together. Its review offer must keep the cost and permission boundary in the packaged Guidance.
