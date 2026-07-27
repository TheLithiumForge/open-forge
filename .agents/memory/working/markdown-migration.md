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

Directive, Workflow, Skill, and category-index semantics have now been extracted into the [Core Primitives scope](../crystallized/documents/framework/primitives/_primitives.md) and matching component maintenance contracts.

## Deferred Extractions

Move each remaining concern to the matching component maintenance document when that source receives its file-by-file migration. Keep the installed component source authoritative for user-visible runtime wording.

| Concern | Current authoritative source or migration input | Intended maintenance destination |
|---|---|---|
| Parser, generator, validator, migration, help, and scaffolding obligations | [CLI MVP Architecture](../crystallized/documents/cli/architecture.md) and [CLI overhaul candidate](../emerging/ideas/cli-overhaul.md) | Future CLI maintenance and architecture contracts |
| Positive prose, punctuation, and selection-surface writing | [User-facing writing decision](../crystallized/decisions/user-facing-writing.md) | Matching #Core writing guidance or directive during its migration |

## Removal Condition

Remove this helper after every deferred concern has a clear authoritative component source and migrated maintenance contract, and no current document relies on this file for meaning.
