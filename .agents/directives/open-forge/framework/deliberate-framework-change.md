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
