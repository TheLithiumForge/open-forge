---
open-forge:
  description: Current Memory capture, direct movement, consolidation, promotion, replacement, archival, restoration, and relationship-update contract
  responsibility: Define how recorded material changes state when its meaning, scope, authority, or future value changes
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Transition, Promotion, Archival, Restoration]
---

# Memory Transitions

## Transition Rule

Any state transition is valid when the destination accurately represents the material's current meaning, scope, authority, and expected use.

Working, Emerging, Crystallized, and Archived do not form a mandatory pipeline. Promotion and demotion are useful shorthand for authority changes, not restrictions on which movement is allowed.

Material may be created directly in the correct state. Clear accepted direction can update a Crystallized source immediately, while an explicitly exploratory idea belongs in Emerging without passing through a formal Working record.

## Capture And Consolidation

Capture the smallest useful record at the route matching its current role:

- Active resumability context enters Working
- Unsettled but plausibly reusable material enters Emerging
- Clearly accepted durable state enters its authoritative current destination
- Non-current material with useful historical value enters Archived after extraction

Before creating a new record, search the relevant route for one with matching scope and meaning. Extend or reshape the existing record when that preserves one coherent source.

Repeated observations, parallel ideas, or overlapping current records trigger consolidation. Recurrence raises the case for promotion but does not substitute for validation or acceptance.

## Common Movements

| Movement | Trigger |
|---|---|
| Working to Emerging | An active discovery may benefit future work but remains unsettled |
| Working to Crystallized or another authoritative source | Active work produces clearly accepted durable state |
| Working to Archived | Work ends and its history remains useful after extraction |
| Emerging to Working | A candidate becomes active work without becoming accepted truth |
| Emerging to Crystallized or another authoritative source | Evidence and direction establish accepted state |
| Emerging to Archived | A rejected or replaced candidate retains useful reasoning |
| Crystallized to Emerging | Accepted state is reopened for reconsideration without a replacement yet |
| Crystallized to Archived | A replacement is established and the previous accepted state becomes history |
| Archived to Working | Historical context becomes active resumability material |
| Archived to Emerging | An old possibility becomes relevant but needs reconsideration |
| Archived to Crystallized or another authoritative source | Current validation and clear acceptance restore it directly |

These examples describe common reasons, not an exhaustive transition graph.

## Acceptance And Promotion

Workspace authority determines when material becomes accepted. The [accepted-state contract](../truth.md#acceptance) defines how clear, tentative, and scoped direction should be interpreted.

Promotion does not mean copying a candidate into Crystallized while leaving the original to imply a second active outcome. Move or reshape the durable result, update links, and archive or prune the candidate once any useful rationale is preserved.

Use the [Memory authority boundary](model.md#authority-boundary) to choose between Crystallized Memory, a matching #Core route, code, or an external system. A linked decision or archived candidate may preserve why and how the accepted result emerged.

## Replacement And Archival

Before archival:

1. Extract useful current meaning to its authoritative destination
2. Record what replaced the material when a replacement exists
3. Preserve origin, scope, and useful rationale
4. Update incoming relationships when the old path would imply current authority
5. Remove active loading or continuity status that no longer applies

Archival preserves useful history. Material without plausible historical value may be pruned instead.

## Restoration

Restoration is a new transition, not an authority reversal.

Archived material is validated against current conditions and moved into an explicit Working, Emerging, Crystallized, #Core, or external destination. Its historical location remains contextual. Conflicts with accepted current state are resolved through current authority rather than by treating the older record as automatically prior.

## Transition Responsibilities

The agent may maintain Working and Emerging state freely within task authority. A transition into accepted durable state requires clear direction, delegated authority, or another valid acceptance source. Ambiguous material remains contextual until dependent work requires clarification.

Every durable transition reports the destination and any affected current or Evergreen source. Git preserves recoverability, but it does not remove the attention, discovery, and review costs of unnecessary records.

## Related Current Sources

- [Memory model](model.md)
- [Working state](working.md)
- [Emerging state](emerging.md)
- [Crystallized state](crystallized.md)
- [Archived state](archived.md)
- [Accepted state and synchronization](../truth.md)
- [Core primitive model](../primitives/model.md)
