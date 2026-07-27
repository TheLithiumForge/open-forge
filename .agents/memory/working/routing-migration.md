---
open-forge:
  description: Temporary extraction ledger for relocating component-specific and implementation-specific material from the former combined routing descriptor
  tags: [Memory, Working, Contextual, Temporary, Migration, Framework, Routing]
---

# Routing Concept Migration

## Status

This file is temporary migration context, not an authoritative routing or component contract.

The former combined routing descriptor has been split into:

- [Routing model](../crystallized/documents/framework/routing/model.md)
- [Route scope and inheritance](../crystallized/documents/framework/routing/scope.md)
- [Loading and continuity](../crystallized/documents/framework/routing/loading.md)
- [Path identity and containment](../crystallized/documents/framework/routing/paths.md)

Agent primitive routing concerns and the non-loader category maintenance contracts have now been extracted into the [Core Primitives scope](../crystallized/documents/framework/primitives/_primitives.md) and [Agents Runtime Maintenance](../crystallized/documents/maintenance/payload/agents/_agents.md).

Overwrite naming, inherited routing and loading, precedence, generated boundaries, ownership, and lifecycle have now been extracted into the [overwrite contract](../crystallized/documents/framework/routing/overwrites.md).

## Deferred Extractions

Move each remaining concern to the matching component maintenance document or scoped concept when that source receives its file-by-file migration.

| Concern | Current authoritative source or migration input | Intended destination |
|---|---|---|
| Scoped Framework route recognition currently hardcoded by the CLI | [CLI MVP Architecture](../crystallized/documents/cli/architecture.md#partial-recursive-support) and [CLI overhaul candidate](../emerging/ideas/cli-overhaul.md) | Future CLI architecture and maintenance |

## Removal Condition

Remove this helper after every deferred concern has a clear current component source and migrated maintenance contract, and no current source relies on this file for meaning.
