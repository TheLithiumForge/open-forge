---
open-forge:
  description: Temporary extraction ledger for relocating component-specific and tooling-specific material from the former combined Markdown contract
  tags: [Memory, Working, Contextual, Temporary, Migration, Framework, Markdown]
---

# Markdown Contract Migration

## Status

This file is temporary migration context, not an authoritative Markdown or component contract.

The former combined Markdown document has been split into:

- [Canonical syntax](../crystallized/documents/framework/markdown/syntax.md)
- [Shared routed representation](../crystallized/documents/framework/markdown/routes.md)
- [Compatibility input](../crystallized/documents/framework/markdown/compatibility.md)

## Deferred Extractions

Move each remaining concern to the matching component maintenance document when that source receives its file-by-file migration. Keep the installed component source authoritative for user-visible runtime wording.

| Concern | Current authoritative source or migration input | Intended maintenance destination |
|---|---|---|
| Direct directive file shape and #LoadNow requirement | [Directives entrypoint](../../directives/_directives.md) and [existing descriptor](../../../docs/framework/payload/agents/directives/_directives.md) | `maintenance/payload/agents/directives.md` |
| Workflow section order, dependency meaning, empty sentinels, and phase vocabulary | [Workflows entrypoint](../../workflows/_workflows.md), [Framework Architecture](../crystallized/documents/framework/architecture.md#workflows), and [existing descriptor](../../../docs/framework/payload/agents/workflows/_workflows.md) | `maintenance/payload/agents/workflows.md` |
| Skill entrypoint and runtime-standard `SKILL.md` boundary | [Skills entrypoint](../../skills/_skills.md) and [existing descriptor](../../../docs/framework/payload/agents/skills/_skills.md) | `maintenance/payload/agents/skills.md` |
| Category-specific generated-entry contents | Installed category entrypoints and their existing descriptors under `docs/framework/payload/agents/` | Matching component maintenance documents |
| Parser, generator, validator, migration, help, and scaffolding obligations | [CLI MVP Architecture](../crystallized/documents/cli/architecture.md) and [CLI overhaul candidate](../emerging/ideas/cli-overhaul.md) | Future CLI maintenance and architecture contracts |
| Positive prose, punctuation, and selection-surface writing | [User-facing writing decision](../crystallized/decisions/user-facing-writing.md) | Matching #Core writing guidance or directive during its migration |

## Removal Condition

Remove this helper after every deferred concern has a clear authoritative component source and migrated maintenance contract, and no current document relies on this file for meaning.
