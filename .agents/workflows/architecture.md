---
open-forge:
  description: Define or review a system's structure, placement, archetypes, invariants, tradeoffs, and transition in one accepted top-down model
  tags: [Extension, Workflow, Architecture, Design, Planning]
---

# Architecture

## Goal

Produce an accepted or decision-ready top-down structure that future work can adopt without inventing cross-cutting decisions inside implementation slices.

## Steps

1. Classify the starting point as greenfield, rescue, or established. Confirm outcome, decision authority, accepted horizon, mutable surfaces, and whether the request needs analysis, documentation, implementation direction, or all three.
2. Map components, responsibilities, ownership, dependency direction, information and control flow, composition, state, integrations, failure and safety boundaries, constraints, and direct consumers.
3. Distinguish process-wide foundations, nearest-shared capabilities, family-local support, and consumer-local semantics. Count accepted future consumers as evidence; do not count hypothetical reuse.
4. Identify command, feature, or component archetypes and the first golden slice for each when the work is a related program.
5. Compare meaningfully different viable directions by fit, complexity, evidence, operations, compatibility, transition cost, and reversibility.
6. Define the smallest coherent architecture, placement map, invariant set, callable foundations, evidence strategy, integration sequence, and stop conditions.
7. Slice adoption into foundation and derivative outcomes with one owner, expected paths, protected paths, integration neighborhoods, and observable verification.
8. After acceptance, update the source that defines current architecture and preserve only consequential rationale or unresolved alternatives in their matching roles.

## Completion

- Starting condition, authority, and accepted horizon are explicit.
- Responsibilities, boundaries, dependency direction, placement, invariants, archetypes, and tradeoffs are clear.
- Accepted consumers and hypothetical reuse are distinguished.
- Adoption is divided into independently executable and verifiable slices.
