---
open-forge:
  description: Memory preserves working, emerging, crystallized, and archived state without activating behavior it describes
  tags: [Memory, Decision, CurrentTruth, MemoryModel]
---

# Memory Model

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- #Memory identifies the Framework area for self-growing Markdown memory used for live work, agent communication and coordination, continuity, accepted records, historical context, and candidate learning
- Agent communication and coordination belong in Memory when they must survive the current context; ordinary conversation does not become durable memory automatically.
- Memory is authoritative for the capture, movement, consolidation, and archival of recorded state.
- Memory may record any subject, including how work is performed, without making that behavior active.
- Accepted behavior that should guide future work belongs in the matching #Core route; Memory may retain useful context or rationale.
- Memory states are `working/`, `emerging/`, `crystallized/`, and `archived/`.
- `working/` is temporary memory for active or recently interrupted work.
- `emerging/` is candidate memory that may be useful but is not accepted truth yet.
- `crystallized/` is accepted durable memory and current truth.
- `archived/` is historical memory kept for context after it is no longer current truth.
- Workspace authority determines when direction is accepted, while state-specific entrypoints keep each Memory state valid.
- `decisions/` is installed under `crystallized/` because decisions are accepted rationale, not a lifecycle state.
- `documents/` is installed under `crystallized/` for durable accepted records or routes to those records.
- `references/` is not installed for now.
- `archived/` remains a root memory state. Archive child routes can be created when they preserve source, authority, or clarity better than a flat archive.

The [top architecture](../documents/architecture.md#framework-composition) expresses Memory's place in the standard Framework. The [Memory Model](../documents/framework/memory/model.md) defines the complete current Memory model.
