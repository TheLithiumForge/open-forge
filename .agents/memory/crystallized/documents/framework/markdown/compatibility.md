---
open-forge:
  description: Current input-only Markdown and filename compatibility accepted by Open Forge tools without becoming canonical authoring syntax
  responsibility: Define the boundary between canonical Open Forge authoring and noncanonical input accepted for migration or interoperability
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Compatibility, Migration, CLI]
---

# Markdown Compatibility Boundary

## Scope

This document is authoritative for which noncanonical Markdown and filename forms may be accepted as input without becoming recommended Open Forge authoring forms.

The [canonical syntax](syntax.md) and [routed representation](routes.md) define what Open Forge authors and emits. The [CLI MVP Architecture](../../cli/architecture.md) and current implementation define how the MVP parser diagnoses and processes supported compatibility input.

## Accepted Compatibility Input

The current CLI may read selected noncanonical input for migration or interoperability:

- `index.md`, `_index.md`, `references.md`, and `_references.md` as category-entrypoint aliases
- `Skill.md` as a skill-entrypoint alias
- `rune:` scoped metadata or unscoped `description` and `tags` in external files
- A first suitable body sentence as a generated description fallback when an external or local file has no supported metadata description
- Legacy backtick route entries
- Angle-bracket route destinations accepted by the current parser
- Legacy generated `Entries` sections without bounded markers when they can be migrated safely
- Selected legacy workspace-relative route paths

Compatibility behavior is input-only unless a current authoritative source explicitly says otherwise. Open Forge-generated files, Templates, examples, documentation, and new authored content use canonical forms.

## Unsupported Equivalents

Other Markdown equivalents may render correctly for people while remaining unsupported for Open Forge semantics.

Open Forge does not promise to interpret:

- Setext headings as semantic sections
- Reference-style links as route entries
- Alternate list markers as machine-readable route lines
- Renamed semantic headings
- Arbitrary metadata layouts

Ordinary prose may use normal Markdown without becoming Framework syntax.

## Tool Boundary

Deterministic tools may parse, validate, generate, migrate, scaffold, or display the accepted Markdown contracts. They do not privately define them.

Tools emit canonical syntax, keep compatibility behavior visibly input-only, diagnose ambiguous machine-meaningful structures, and leave unrelated prose alone. The same files remain understandable and maintainable without those tools.

## Related Current Sources

- [Canonical Markdown syntax](syntax.md)
- [Routed Markdown representation](routes.md)
- [CLI MVP Architecture](../../cli/architecture.md)
- [CLI overhaul candidate](../../../../emerging/ideas/cli-overhaul.md)

## Decisions And Rationale

- [Canonical Markdown authoring](../../../decisions/canonical-markdown.md)
