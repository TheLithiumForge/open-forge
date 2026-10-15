---
open-forge:
  description: Keep Open Forge contract changes deliberate, current, dogfooded, reviewable, and evidence-backed
  tags: [LoadNow, Directive, Framework, Change, Dogfood, Review, Evidence]
---

# Deliberate Framework Change

## Instructions

### Current Authority And Design

- Before changing an Open Forge contract, load and follow the current documents and Maintenance contracts that govern it.
- Before accepting a product, Framework, or tooling direction that could affect identity, read the [Open Forge Principles](../../../memory/crystallized/documents/principles.md) and make any tension or required reconsideration explicit.
- Keep baseline context and mandatory rules as small as possible. Every always-loaded addition must earn its ongoing attention cost.
- Use existing routing, scope, relationship, and #Core semantics instead of adding special-case machinery when they can express the requirement clearly.
- Keep generic first-party Workflows, Patterns, and Templates technology agnostic. Let applicable workspace sources supply project facts, conventions, tools, verification requirements, and accepted decisions. Provide useful methods and starting shapes without importing repository-local procedures or making optional records mandatory.

### Repository And Change Boundaries

- Preserve unrelated work and the user's Git boundary. Keep each change reviewable and make generated changes explicit.
- Before a repository-producing investigation, spike, or experiment, state the question, expected artifacts and scope, exit criteria, and planned disposition. Surface a material expansion before creating it.
- Treat maintainer edits as deliberate design input about clarity, readiness, logical consistency, or tone. Preserve their intent, use implementation discretion within it, and do not revert or neutralize them unless the maintainer asks for another change or an explicit conflict must be reported.

### Verification

- Follow the already-loaded repository [Writing Directive](../../public-facing-writing.md) when writing or reviewing Open Forge prose.
- Verify changed behavior through the public interface that exposes it when one exists.

### Maintenance Documents

- Organize each current maintenance document as Source, Contract and Verification, in that order. Source links to the canonical source, states its responsibility and identifies true counterparts. Contract defines the smallest complete set of maintainer-level invariants, boundaries and change obligations. Verification links to the tests or checks that prove alignment and states necessary manual verification when no automated check exists.
- Optional frontmatter `responsibility` is an edit boundary, not an authority grant or replacement for Source. Place relative links beside the contract or verification statement they support. Include a relationship when it creates a maintenance consequence.
- Optional level-3 Contract subsections name actual, distinct concerns and clarify a distinct part of the contract. Every Contract statement narrows the valid design space or identifies a real change obligation. Link relevant counterparts, dependencies, decisions, consumers and external contracts where they affect maintenance. Keep historical rationale in decisions or archives. Verification names observable evidence rather than repeating the contract.

The [Maintenance Contract Pattern](../../../patterns/open-forge/maintenance-contract.md) shows this shape.

### Managed Root Entries

- A canonical source template contains exactly one complete ordered managed block and only that block. A workspace target has either no canonical or exact legacy boundary candidate, so installation appends the source block, or exactly one complete canonical heading/footer pair or exact legacy comment pair, so installation replaces that span.
- Validate every canonical source template and existing target before applying the installation plan. An incomplete, reversed, duplicate, mixed or otherwise ambiguous boundary blocks before writing and produces no partial output. A heading without its named footer never extends the managed span to the user suffix or end-of-file.
- Recognize boundaries from root-level parsed Markdown blocks and typed source spans. A heading or footer inside fenced code, a quote, a nested list or an inline example is content, not a delimiter. Preserve workspace bytes outside the managed block exactly through creation, replacement and reinstallation.
- Keep source and dogfood blocks identical. Reapplying the same source block produces no change.
- One canonical policy source serves every bridge. A canonical entry is authoritative for its Framework handoff. A harness bridge contains only harness-native references to the canonical Framework entries it exposes and does not restate their policy. Each harness-specific maintenance contract defines its external syntax and compatibility requirements.
- File-specific maintenance documents link to the Pattern, their source, implementation, external contract when present and verification.

The [Managed Root Entry Pattern](../../../patterns/open-forge/managed-root-entry.md) shows the source and target boundaries.
