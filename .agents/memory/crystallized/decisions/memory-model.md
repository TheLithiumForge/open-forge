---
open-forge:
  description: Memory is self-growing markdown state with working, emerging, crystallized, and archived states; it records state and never owns behavior
  tags: [Memory, Decision, CurrentTruth, MemoryModel]
---

# Memory Model

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- The #Memory layer is self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.
- Memory records state and must not own operational behavior.
- Operational behavior, reusable form, guidance, capability, workflow, or workspace routing must move to the matching #Core route.
- Memory states are `working/`, `emerging/`, `crystallized/`, and `archived/`.
- `working/` is temporary memory for active or recently interrupted work.
- `emerging/` is candidate memory that may be useful but is not accepted truth yet.
- `crystallized/` is accepted durable memory and current truth.
- `archived/` is historical memory kept for context after it is no longer current truth.
- `decisions/` is installed under `crystallized/` because decisions are accepted rationale, not a lifecycle state.
- `documents/` is installed under `crystallized/` for durable accepted records or routes to those records.
- `references/` is not installed for now.
- `archived/` remains a root memory state. Archive child routes can be created when they preserve origin, ownership, or clarity better than a flat archive.
