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

## Deferred Extractions

Move each remaining concern to the matching component maintenance document or scoped concept when that source receives its file-by-file migration.

| Concern | Current authoritative source or migration input | Intended destination |
|---|---|---|
| Direct directive scope, direct-file #LoadNow, additive authority, and inactive inspection | [Directives entrypoint](../../directives/_directives.md) and [existing descriptor](../../../docs/framework/payload/agents/directives/_directives.md) | `maintenance/payload/agents/directives.md` and the Agent Primitives migration |
| Workflow selection, phase wayfinding, `Required Routes`, and goal execution | [Workflows entrypoint](../../workflows/_workflows.md) and [existing descriptor](../../../docs/framework/payload/agents/workflows/_workflows.md) | `maintenance/payload/agents/workflows.md` and the Agent Primitives migration |
| Skill routing through `SKILL.md` and skill-owned internal resources | [Skills entrypoint](../../skills/_skills.md) and [existing descriptor](../../../docs/framework/payload/agents/skills/_skills.md) | `maintenance/payload/agents/skills.md` and the Agent Primitives migration |
| Workflow-local mixed primitive bundles | [Agent Primitives descriptor](../../../docs/framework/concepts/agent-primitives.md) | Agent Primitives current contract and matching component maintenance |
| Overwrite naming, adjacency, precedence, indexing, and customization | [Loader](../../loader.md), [Framework Architecture](../crystallized/documents/framework/architecture.md#recursive-customization), and [Overwrite descriptor](../../../docs/framework/concepts/overwrites.md) | Scoped overwrite contract and affected source maintenance |
| Default load tags and generated contents of each shipped category | Installed category entrypoints and descriptors under `docs/framework/payload/agents/` | Matching component maintenance documents |
| Scoped Framework route recognition currently hardcoded by the CLI | [CLI MVP Architecture](../crystallized/documents/cli/architecture.md#partial-recursive-support) and [CLI overhaul candidate](../emerging/ideas/cli-overhaul.md) | Future CLI architecture and maintenance |
| Source-to-dogfood alignment checks for non-loader entrypoints | Installed sources and existing payload descriptors | Matching component maintenance documents |

## Removal Condition

Remove this helper after every deferred concern has a clear current component source and migrated maintenance contract, and no current source relies on this file for meaning.
