---
open-forge:
  description: Memory state routes for human-AI work
  tags: [OpenForge, Memory, Index, LoadWithParentEntrypoint]
---

# Memory

Memory is the workspace state record: current truth, live work, candidate learning, and useful history.

## Axioms

- Use other Memory state entries when their path, description, or tags match the current request.
- State entrypoints expose routes; load only the memory bodies and child categories relevant to the current request.
- Use loader-defined `#Contextual` and `#CurrentTruth` tags to distinguish context from accepted current truth.
- Keep memory below current user instructions, runtime safety, platform constraints, and declared external sources of truth.
- Memory records state; it must not own operational behavior.
- Move behavior, reusable form, guidance, capability, workflow, or workspace routing out of Memory and into the matching `#Core` route, including user-created `#Core` categories and files.
- Add child categories when they improve routing, ownership, or clarity.
- Discuss new root Memory states with the user before creating them.
- If memory does not fit the current routes, propose a clearer route before writing it.
- Generated entries are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- `archived/_archived.md` - Archived memory preserved as historical context - #OpenForge #Memory #Archived #Index #Contextual #Historical
- `crystallized/_crystallized.md` - Accepted durable memory and current truth - #OpenForge #Memory #Crystallized #Index #CurrentTruth #LoadWithParentEntrypoint
- `emerging/_emerging.md` - Candidate memory becoming useful but not accepted truth - #OpenForge #Memory #Emerging #Index #Contextual #Candidate
- `working/_working.md` - Working memory alive in current work - #OpenForge #Memory #Working #Index #Contextual #LoadWithParentEntrypoint
<!-- open-forge:generated-index:end -->
