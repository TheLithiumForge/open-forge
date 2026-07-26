---
open-forge:
  description: Keep Open Forge contract changes deliberate, current, dogfooded, reviewable, and evidence-backed
  tags: [LoadNow, Directive, Framework, Change, Dogfood, Review, Evidence]
---

# Deliberate Framework Change

## Axioms

- Establish or update the owning directive, pattern, decision, or current document before or with a framework contract change so implementation follows written current truth.
- Before accepting a product, Framework, or tooling direction that could affect identity, read the [Open Forge Principles](../memory/crystallized/documents/principles.md) and make any tension or required reconsideration explicit.
- Keep baseline context and mandatory rules as small as possible; every always-loaded addition must earn its ongoing attention cost.
- Use existing routing, scope, relationship, and #Core semantics instead of adding special-case machinery when they can express the requirement clearly.
- Governance documents describe what source files must contain, their relationships, and their verification. Source files remain the sole owners of runtime wording; governance documents express maintainer constraints and link to the owners.
- State Framework requirements against the broadest stable semantic owner that preserves their meaning. Refer to a matching #Core owner or entry instead of enumerating standard primitive routes unless a primitive's distinct semantics or the exact shipped defaults are the subject.
- Preserve unrelated work and the user's Git boundary; keep each change reviewable and make generated changes explicit.
- Treat maintainer edits as deliberate design input about clarity, readiness, logical consistency, or tone. Preserve their intent, use implementation discretion within it, and do not revert or neutralize them unless the maintainer asks for another change or an explicit conflict must be reported.
- Update dogfood, installable source, governing documentation, and behavior tests together when they share a contract; document intentional differences.
- Extract current truth before moving superseded analysis, ideas, sessions, handoffs, or reports into their scoped archive.
- Verify claims proportionately with real public interfaces and record failures, gaps, and remaining uncertainty instead of converting intent into claimed behavior.
