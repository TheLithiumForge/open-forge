---
open-forge:
  description: Implement retained read-only source, context, extension, and generated-navigation commands
  tags: [Memory, Working, CLI, Task, ReadOnly, Source, Extension, Index, Contextual]
---

# Read-Only Commands

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Accepted Foundation and route discovery facts.

## Outcome And Boundaries

Read-only commands answer source, reference, context, extension, and generated
navigation questions without creating locks, lifecycle files, caches, indexes
outside explicit `index`, recovery artifacts, or workspace mutations.

Find and References may proceed in parallel after shared source and route facts
freeze. Context waits for both. Extension discovery may proceed independently on
accepted source-catalogue boundaries. Index waits for routing, document, source,
and reference facts.

Every child owns its request, result, findings, presentation, help, and public
scenario. Shared facts move only through a Mastermind integration increment.

## Child Tasks

- [ ] [Implement deterministic CommonMark-aware source discovery and accepted Find projections](find.md) — Planned — Implementer: Not assigned
- [ ] [Implement direct incoming and outgoing reference facts with shared source-universe filters](references.md) — Planned — Implementer: Not assigned
- [ ] [Implement ordered context selection, loading reasons, overlap, size, and token projections](context.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension catalogue and source listing without lifecycle inference](extension-list.md) — Planned — Implementer: Not assigned
- [ ] [Implement exact Extension package inspection without mutation or installation behavior](extension-inspect.md) — Planned — Implementer: Not assigned
- [ ] [Implement deterministic generated Entries projection and idempotent index application](index.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Implement deterministic CommonMark-aware source discovery and accepted Find projections](find.md) - #Memory #Working #CLI #Task #Find #ReadOnly #Markdown #Contextual
- [Implement direct incoming and outgoing reference facts with shared source-universe filters](references.md) - #Memory #Working #CLI #Task #References #ReadOnly #Links #Contextual
- [Implement ordered context selection, loading reasons, overlap, size, and token projections](context.md) - #Memory #Working #CLI #Task #Context #ReadOnly #Loading #Contextual
- [Implement Extension catalogue and source listing without lifecycle inference](extension-list.md) - #Memory #Working #CLI #Task #Extension #List #ReadOnly #Contextual
- [Implement exact Extension package inspection without mutation or installation behavior](extension-inspect.md) - #Memory #Working #CLI #Task #Extension #Inspect #ReadOnly #Contextual
- [Implement deterministic generated Entries projection and idempotent index application](index.md) - #Memory #Working #CLI #Task #Index #Generated #Mutation #Contextual

<!-- open-forge:generated-index:end -->
