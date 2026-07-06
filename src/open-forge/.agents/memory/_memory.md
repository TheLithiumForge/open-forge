---
open-forge:
    description: Self-growing markdown memory for workspace state, AI communication, current and historical records and learning
    tags: [OpenForge, Memory, OrganicGrowth, Index, LoadWithParentEntrypoint]
---

# Memory

Memory is self-growing durable markdown memory for workspace state: current truth, live work, AI communication, written records, candidate learning, and useful history.

## Axioms

- Use memory state `entries` when their path, description, or tags match the current request.
- Read `Entries` before deciding which memory state applies.
- State `entrypoints` expose routes; load only the memory bodies and child categories relevant to the current request.
- Use loader-defined #Contextual and #CurrentTruth tags to distinguish context from accepted current truth.
- Keep memory below current user instructions, runtime safety, platform constraints, applicable #Core routes, and declared external sources of truth.
- Memory records state; it must not own operational behavior.
- Write useful durable state to the matching #Memory route when safe and allowed; do not treat private agent memory as the source of truth.
- Move material between #Memory routes when its state or owner changes.
- Extract behavior, reusable form, guidance, capability, workflow, or workspace routing to the matching #Core route, including user-created #Core categories and files.
- Add child categories when they improve routing, ownership, or clarity.
- Discuss new root memory states with the user before creating them.
- If memory does not fit the current routes, propose a clearer route before writing it.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->

- `archived/_archived.md` - Historical memory kept for context after it stops being current - #OpenForge #Memory #Archived #Index #Contextual #Historical
- `crystallized/_crystallized.md` - Accepted current memory that should guide work within its scope - #OpenForge #Memory #Crystallized #Index #CurrentTruth #LoadWithParentEntrypoint
- `emerging/_emerging.md` - Candidate memory that is useful but not yet accepted as truth - #OpenForge #Memory #Emerging #OrganicGrowth #Index #Contextual #Candidate #LoadForPostWorkReview
- `working/_working.md` - Temporary context needed to continue or resume active work - #OpenForge #Memory #Working #Index #Contextual #LoadWithParentEntrypoint
  <!-- open-forge:generated-index:end -->
