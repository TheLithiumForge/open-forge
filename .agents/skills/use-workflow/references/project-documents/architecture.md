---
open-forge:
  description: "Define or review structure, responsibilities, dependency direction, and a verifiable transition"
  tags: [Extension, Workflow, Architecture, Design]
---

# Architecture

## Goal

Produce an accepted or decision-ready structure that future work can understand, adopt, and verify. Distinguish a description of the existing system from a proposed replacement.

[Document Templates](../../../../templates/documents/_documents.md) provide optional starting files. Use existing defining sources when they already answer the question; the Documents category is a convention, not a required destination.

## Steps

1. **Establish the view.** Name the subject, decision authority, allowed changes, and requested result: investigation, current documentation, implementation direction, or a combination. Classify the starting point as new, established, or in need of recovery.
2. **Map the relevant system.** Load applicable scopes and inspect components, responsibilities, ownership, dependencies, flows, integrations, deployment, failure boundaries, constraints, and accepted choices. For a new system, describe the first useful slice rather than inventing existing components.
3. **Find the drivers.** Identify the qualities and constraints that actually shape this decision. Consider security, reliability, observability, cost, change, scale, and team capability where relevant; do not turn the list into mandatory sections.
4. **Compare viable directions.** Explain fit, complexity, verification, operating cost, transition, and reversibility. Retain alternatives only when their tradeoffs help the decision.
5. **Define the smallest coherent structure.** Make responsibilities, exclusions, dependency direction, failure containment, and material compromises explicit. Place component detail in narrower sources when it needs independent maintenance.
6. **Resolve acceptance.** Keep a recommendation proposed until applicable authority accepts it. An unresolved structural decision should return its evidence, consequences, and recommended next step, not a fabricated final architecture.
7. **Prepare adoption.** When authorized, update the defining architecture source and separate implementation into coherent pieces for compatibility, verification, rollback, and cleanup. Keep later work tied to actual triggers instead of speculative scope.

## Completion

- The current or intended system and the view being defined are explicit.
- The structure explains responsibilities, boundaries, relationships, constraints, and tradeoffs.
- Acceptance status, important rejected alternatives, and remaining decisions are clear.
- Required transition work is divided into adoptable, verifiable pieces, or the blocking decision is identified.
