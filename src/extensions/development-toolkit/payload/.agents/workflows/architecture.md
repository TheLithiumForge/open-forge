---
open-forge:
  description: Define or review a system's structure, tradeoffs, boundaries, and adoptable transition
  tags: [Extension, Workflow, Architecture, Design]
---

# Architecture

## Goal

Produce an accepted or decision-ready structural direction that future work can understand, adopt, and verify.

## Steps

1. Classify the starting point as greenfield, unstructured or rescue, or established. Confirm the subject, decision authority, mutation boundary, and whether the request needs analysis, current documentation, implementation direction, or a combination.
2. Map the relevant components, responsibilities, ownership, dependency direction, information and control flows, integrations, runtime and deployment shape, failure boundaries, constraints, and accepted decisions. For greenfield work, map the first useful vertical slice instead of inventing a current system.
3. Identify the qualities and stage constraints that must shape the structure, including security, reliability, observability, cost, evolution, scale triggers, and team capability where relevant.
4. Compare materially viable directions by fit, complexity, verification, operations, transition cost, and reversibility. State rejected options only when their rationale will matter later.
5. Define or recommend the smallest coherent structure that satisfies the accepted drivers. Make responsibilities, boundaries, dependency direction, failure containment, and important tradeoffs explicit.
6. With the required acceptance, update the current architecture and route only warranted invariants, reusable shapes, contextual judgment, locations, or rationale to their matching #Core or #Memory `routes`. Keep useful unaccepted alternatives contextual.
7. Slice adoption into coherent implementation, compatibility, verification, rollback, cleanup, and later trigger-based work.

## Completion

- The current or intended system and its starting condition are explicit.
- The selected or proposed structure states responsibilities, boundaries, relationships, constraints, and tradeoffs.
- Acceptance status and rejected options are clear.
- The transition is divided into adoptable, verifiable slices.
