---
open-forge:
  description: Define or review a system's structure, boundaries, tradeoffs, and transition
  tags: [Extension, Workflow, Architecture, Design]
---

# Architecture

## Goal

Produce an accepted or decision-ready structure that future work can understand, adopt, and verify.

## Steps

1. Classify the starting point as greenfield, unstructured or rescue, or established. Confirm the subject, who can decide, what may change, and whether the request needs analysis, current documentation, implementation direction, or a combination.
2. Use the applicable project scopes and current sources to map the relevant components, responsibilities, ownership, dependency direction, information and control flows, integrations, runtime and deployment shape, failure boundaries, constraints, and accepted decisions. For greenfield work, map the first useful vertical slice instead of inventing a current system.
3. Identify the qualities and constraints at the current stage that must shape the structure. Include security, reliability, observability, cost, evolution, scale triggers, and team capability where relevant.
4. Compare meaningfully different viable directions by fit, complexity, verification, operations, transition cost, and reversibility. State rejected options only when their reasoning will matter later.
5. Define or recommend the smallest coherent structure that satisfies the accepted drivers. Make responsibilities, boundaries, dependency direction, failure containment, and important tradeoffs explicit.
6. After the direction is accepted, update the source that defines the current architecture within the established authority. Create a separate Architecture document only when a durable current view is useful. Put only useful invariants, reusable shapes, advice, locations, or reasoning in their matching #Core or #Memory routes. Keep useful unaccepted alternatives contextual.
7. Divide adoption into coherent pieces for implementation, compatibility, verification, rollback, cleanup, and later work tied to specific triggers.

If an unresolved decision prevents a reliable structure, return the decision, supporting evidence, and recommended next step. Keep the structure visibly proposed until applicable authority accepts it.

## Completion

- The current or intended system and its starting condition are explicit.
- The selected or proposed structure states responsibilities, boundaries, relationships, constraints, and tradeoffs.
- Acceptance status and rejected options are clear.
- The transition is divided into adoptable, verifiable slices.
