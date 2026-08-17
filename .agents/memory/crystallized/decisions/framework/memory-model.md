---
open-forge:
  description: Memory preserves working, emerging, crystallized, and archived state without activating behavior it describes
  tags: [Memory, Decision, CurrentTruth, MemoryModel]
---

# Memory Model

## Context

Work needs temporary coordination, candidate learning, accepted durable state, and useful history. A single undifferentiated knowledge store makes current authority and expected lifetime unclear, while behavior described in a record can be mistaken for an active instruction.

## Decision

Memory is a self-growing Markdown lifecycle with four states: Working, Emerging, Crystallized, and Archived.

Recorded material follows explicit state-specific rules for capture, classification, movement, consolidation, archival, and restoration. Memory may record any subject without activating behavior it describes. Accepted behavior that should guide future work moves to the matching #Core `route`, while Memory may preserve its rationale or history.

Decisions and Documents are standard roles within Crystallized Memory rather than additional lifecycle states. Memory uses the [accepted universal scoping model](scope-and-slugs.md) rather than a separate Memory-specific mechanism.

## Rationale

Four states distinguish active need, plausible future value, accepted durable truth, and retained history without forcing every record through one rigid pipeline.

Keeping Memory non-activating separates preservation from behavior. This lets observations and alternatives survive without silently becoming instructions and lets accepted behavior use the Core role whose semantics actually apply.

## Alternatives And Tradeoffs

- One knowledge pool would minimize folders but make authority, uncertainty, and expiration implicit
- Three states without Emerging would force candidates into temporary work or accepted truth
- Treating recorded behavior as active would turn Memory into a hidden directive system
- Installing `references/`, tasks, or backlog as universal base `routes` would commit to knowledge and planning roles whose general boundaries are not yet established
- Moving default Memory outside `.agents/` would weaken the current unified Framework entry and has not earned a replacement contract

The lifecycle adds classification work, so transitions remain meaning-based and any justified direct movement is allowed.

## Consequences

- Working Checkpoints keep the mutable current state of one active workstream in a form any future reader can use. Handoffs are sealed boundary snapshots created only when an actual transfer or explicitly planned resumption must remain stable while the Checkpoint may change.
- Working records are expected to expire through extraction, movement, archival, or pruning
- Emerging records meet a minimal usefulness threshold without requiring recurrence before capture
- Crystallized records express accepted durable state or rationale
- Archived records remain historical and do not compete with current truth
- Agent communication belongs in Memory when it must survive the current context. Ordinary conversation is not recorded automatically.
- State and scope remain independent dimensions
- Workspaces may remove, replace, or supplement the standard state and role `routes`
- Baseline loading exposes active and accepted Memory navigation, continuity loading revisits candidate learning, and Archived remains selected on demand

## Authoritative Sources

- [Open Forge Architecture](../../documents/architecture.md#framework-composition)
- [Current Memory model](../../documents/framework/memory/model.md)
- [Current Memory transition contract](../../documents/framework/memory/transitions.md)
- [Installed Memory entrypoint](../../../_memory.md)
- [Memory Maintenance scope](../../documents/maintenance/payload/agents/memory/_memory.md)

## Decision Relationships

- [Product direction](../product/product-direction.md)
- [Distinct Core primitive roles](core-primitives.md)
- [Tag semantics](tags.md)
- [Source and packaging](source-and-packaging.md)
