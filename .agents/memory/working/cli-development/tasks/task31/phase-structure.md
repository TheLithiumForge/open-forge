---
open-forge:
  description: Queued Task 31 structure cleanup for one-file folders and oversized seams after the completed G4 presentation work
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Structural, Refactoring]
---

# Task 31 — Structure Cleanup

## Status

Applied after the completed G4 presentation work. Its inventory and closeout
evidence are recorded in [phase-structure-inventory.md](phase-structure-inventory.md).
This phase is a maintainability follow-up, not a license to abstract the Route
Move/Remove contracts or to overlap Task 30 G1.

The earlier figure of 176 one-file C# folders in Core is superseded. The
inventory's reproducible scan finds **294** under an explicit leaf-folder rule
(one direct `.cs` file, no child directories), 123 of them in the settled G4
Presentation tree. Most are deliberate structure: the inventory retains 289 as
Keep, applies the three-file Extension List collapse, and records both former
decision gates as resolved and applied.

## Evidence source

- [C# directives and structure](../../../../emerging/analysis/cli-experience-audit/csharp-directives-and-structure.md)
- [Implementation duplication](../../../../emerging/analysis/cli-experience-audit/implementation-duplication.md)
- [Layer adherence](../../../../emerging/analysis/cli-experience-audit/layer-adherence.md)

## Actionable boundary

- Inventory the one-file folders and proposed moves by real ownership, layer,
  namespace, project reference, generated-code boundary, and test impact.
- Collapse only folders whose move leaves the dependency graph and public
  contracts clearer. Keep the four-layer dependency rule and the shared-owner
  rule enforced by the existing boundary tests.
- Identify whether the largest files have a genuine semantic seam. Split only
  when a new owner answers a distinct question; do not split for line count or
  create generic utility bags.
- Keep Route Move/Remove finding codes and request/result formations local, and
  leave lifecycle/permission internals to Task 30's accepted state work.
- Record every moved type, namespace/reference change, and retained test in a
  reviewable map; the completed map records the 12 production consumers and 3
  directly affected tests for the Extension List collapse.

## Acceptance

- The inventory has a disposition for every one-file folder and every proposed
  oversized-file split.
- Layer-boundary tests and behavior-preserving characterization evidence stay
  green for the moved code; no public finding code, JSON, exit, or stream
  behavior changes.
- The resulting graph is independently understandable, with no new
  cross-layer owner inversion or forwarding-only type.
- The complete managed Unit and Integration gates remain at the accepted
  baseline except for the three intentionally deleted unit cases.
