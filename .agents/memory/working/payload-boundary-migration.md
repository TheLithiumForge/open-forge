---
open-forge:
  description: Temporary readiness map for consolidating the remaining Payload Boundary descriptor into current architecture and maintenance sources
  tags: [Memory, Working, Contextual, Temporary, Migration, Framework, Payload, Maintenance, Governance]
---

# Payload Boundary Migration Readiness

## Status

The next Framework concept migration should consolidate [`docs/framework/concepts/payload-boundary.md`](../../../docs/framework/concepts/payload-boundary.md).

This file prepares that migration. It is not an authoritative Payload, distribution, or Maintenance contract, and it does not change the current descriptor yet.

Payload Boundary is the next useful target because only it and the Extensions concept remain under `docs/framework/concepts/`. Its cross-cutting contract constrains installed completeness and maintainer synchronization, while the Extensions concept is already tied to an explicitly provisional MVP architecture and later overhaul.

## Accepted Current Sources

| Question | Current authoritative source |
|---|---|
| What must remain complete and understandable in the installed Framework? | [Framework Architecture: Distribution And Dogfood](../crystallized/documents/framework/architecture.md#distribution-and-dogfood) |
| How do Core, Memory, Extensions, tools, and generated state depend on one another? | [Top architecture](../crystallized/documents/architecture.md#authority-and-dependency-direction) and [Framework Architecture](../crystallized/documents/framework/architecture.md) |
| How are reviewed installable files mapped to source, dogfood, relationships, and verification? | [Payload Maintenance](../crystallized/documents/maintenance/payload/_payload.md) and its file-specific children |
| What binds synchronized Framework changes during this migration? | [Deliberate Framework Change](../../directives/deliberate-framework-change.md) |
| Why are installable source and repository Maintenance separate? | [Source And Packaging Decision](../crystallized/decisions/source-and-packaging.md) |
| How do generated Entries and routed Markdown behave? | [Routed Markdown Representation](../crystallized/documents/framework/markdown/routes.md) |
| How do root entries and harness bridges preserve workspace-owned content? | [Managed Root Entry Pattern](../../patterns/open-forge/managed-root-entry.md), [AGENTS Entry Maintenance Contract](../crystallized/documents/maintenance/payload/AGENTS.md), and [Claude Code Bridge Maintenance Contract](../crystallized/documents/maintenance/payload/CLAUDE.md) |
| How should actors, authority, and ownership be stated? | [Open Forge Writing Standard](../crystallized/documents/maintenance/writing.md) and [Terminology Helper](../crystallized/documents/maintenance/helpers/terminology.md) |

## Descriptor Concern Map

| Descriptor concern | Migration direction |
|---|---|
| Description, representation, installed completeness, and why | Keep the complete current meaning in Framework Architecture; do not copy another product explanation into Maintenance |
| Governance changes update installed behavior in the same work | Keep the binding synchronization rule in the deliberate-change Directive; let Payload Maintenance define the source and verification mapping |
| User documentation explains but does not replace installed instructions | Add one explicit sentence to Framework Architecture if the current installed-completeness wording remains too implicit |
| Installed wording uses installed concepts and does not depend on unpublished governance | Keep as part of the Distribution And Dogfood boundary; preserve only a compact Maintenance check |
| Generated Entries are navigation metadata | Link to the routed Markdown contract rather than restating it |
| Canonical entry and harness bridges | Keep entirely in the managed-root Pattern and file-specific Maintenance contracts |
| Lower-level contracts do not depend on optional higher-level areas | State the dependency rule in architecture; use direct actor wording from the Writing Standard without creating a separate scope-wording contract |
| Alignment checklist | Keep only repository-maintenance checks that are not already architectural or component-specific |

## Proposed Migration Shape

1. Apply each concern map row to its listed destination and add a stable `responsibility` to Payload Maintenance. Change other linked contracts only if this exposes a concrete missing responsibility.
2. Link the Source And Packaging Decision forward to the resulting Framework and Payload Maintenance sources without performing the scheduled Decision cleanup early
3. Remove the obsolete descriptor from the Framework Architecture migration inputs, add Payload Maintenance to its related authoritative sources, and redirect the archived loader-readiness link to the Framework Architecture's Distribution And Dogfood contract
4. Remove `docs/framework/concepts/payload-boundary.md` after every retained concern has a current destination
5. Leave `docs/dev.md` saying remaining descriptors still govern their subjects because the Extensions descriptor will remain

## Expected Change Surface

- `docs/framework/concepts/payload-boundary.md`
- `.agents/memory/crystallized/documents/framework/architecture.md`
- `.agents/memory/crystallized/documents/maintenance/payload/_payload.md`
- `.agents/memory/crystallized/decisions/source-and-packaging.md`
- `.agents/memory/archived/analysis/2026-07-26_loader-migration-readiness.md`
- Generated parent indexes when route metadata changes

## Verification Boundary

This migration should require:

- `open-forge index`
- `open-forge doctor` at the repository and installable source roots
- Link and stale-reference searches for the removed descriptor
- `git diff --check`
- Focused tests only when an affected contract already has relevant coverage and no runtime or CLI behavior changes
- Independent semantic-loss, writing-quality, and optimization reviews

The full closure suite is unnecessary unless the migration changes install, packaging, generated output, or another process boundary.

## Historical Inputs

- [Approved design baseline](../archived/sessions/2026-07-26_open-forge-design-baseline.md)
- [Original payload-boundary refinement](../archived/sessions/2026-06-20_open-forge-next-design-decisions.md)
- `feature/initial2:docs/framework/concepts/payload-boundary.md`
- Git history of the current descriptor

## Removal Condition

Remove this readiness file after the descriptor is migrated, every retained concern has one clear current source, and no active work depends on this preparation map.
