---
open-forge:
  description: Keep Open Forge contract changes deliberate, current, dogfooded, reviewable, and evidence-backed
  tags: [LoadNow, Directive, Framework, Change, Dogfood, Review, Evidence]
---

# Deliberate Framework Change

## Axioms

- Establish or update the owning directive, pattern, decision, or current document before or with a framework contract change so implementation follows written current truth.
- Governance documents describe what source files must contain, their relationships, and their verification. Source files remain the sole owners of runtime wording; governance documents express maintainer constraints and link to the owners.
- Preserve unrelated work and the user's Git boundary; keep each change reviewable and make generated changes explicit.
- Treat maintainer edits as deliberate design input about clarity, readiness, logical consistency, or tone. Preserve their intent, use implementation discretion within it, and do not revert or neutralize them unless the maintainer asks for another change or an explicit conflict must be reported.
- Update dogfood, installable source, governing documentation, and behavior tests together when they share a contract; document intentional differences.
- Extract current truth before moving superseded analysis, ideas, sessions, handoffs, or reports into their scoped archive.
- Verify claims proportionately with real public interfaces and record failures, gaps, and remaining uncertainty instead of converting intent into claimed behavior.
