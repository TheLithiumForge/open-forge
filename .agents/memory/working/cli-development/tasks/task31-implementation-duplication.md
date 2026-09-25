---
open-forge:
  description: Open Task 31 for removing duplicated CLI implementation while preserving command-local contracts
  tags: [Memory, Working, CLI, Task, Duplication, Refactoring, Contextual, Active]
---

# Task 31 — Implementation Duplication Removal

## Task state

- State: **Open**. M1's accepted enumeration, M2, M3, and M4 are complete.
  Route Move/Remove remains outside the safe extraction set. M5's precondition
  is met: Task 30 G4 is complete and its presentation structure is stable, and
  M5's inventory is recorded.
- Owner: Root, direct sequential implementation.
- Evidence source: the Emerging [Implementation Duplication analysis](../../../emerging/analysis/cli-experience-audit/implementation-duplication.md).
- Current next step: rule on M5's two recorded `Needs a decision` folders, then
  apply the inventory's single proposed collapse as its own verified batch. The
  [inventory](task31/phase-structure-inventory.md) dispositions all 294 current
  one-file Core folders as 289 Keep, 3 Collapse and 2 Needs a decision, and
  finds one genuine oversized-file seam in `CleanupResultFacts.cs`.
  [M1's closeout](../../../archived/cli-development/tasks/task31/08-m1-enumerated-extractions.md) extracted three groups,
  conditionally retired the Library validator group, preserved human/wire
  vocabulary separation and deferred Library finding abstraction. Its four
  convention checks and 24-output zero-diff characterization passed.

## Measured problem and outcome

The survey found 103 byte-identical method groups across roughly 1,400 lines,
eight text escapers producing four answers, and constants repeated in up to
seven places. Type placement is already sound. At completion, each duplicated
body has one justified owner, one escaper exists at its accepted boundary, and
constants sit with the scope that owns them.

## Milestone map

| Milestone                 | State                                                                 | Task-owned route                               |
| ------------------------- | --------------------------------------------------------------------- | ---------------------------------------------- |
| M1 — family-level methods | Complete for accepted enumeration; excluded contracts remain separate | [Phase 3.5](task31/phase-3-5.md)               |
| M2 — cross-family methods | Complete; behavior-preserving                                         | [Phase 3.5](task31/phase-3-5.md)               |
| M3 — one text escaper     | Complete in Task 30 G4's rendering-system work; behavior-changing       | [Escaper boundary](../../../archived/cli-development/tasks/task31/phase-escaper.md)    |
| M4 — constant placement   | Complete; behavior-preserving                                         | [Constant rule](../../../archived/cli-development/tasks/task31/phase-constants.md)     |
| M5 — structure cleanup    | Inventory recorded; application not started                           | [Structure cleanup](task31/phase-structure.md) |

## Actionable boundaries

- Prefer deletion against an existing owner; add a shared owner only when
  accepted consumers need identical neutral meaning.
- Preserve command-local finding codes, result formations, and request types.
  Do not lift Route Move/Remove selectors merely because their bodies coincide:
  their published finding-code sets differ by destination behavior.
- Treat a textual or status-policy difference as a potential behavior change,
  not as duplication, until reachability and output evidence prove otherwise.
- Do not touch lifecycle-baseline or permission code that Task 30 G1 is about
  to redesign.
- Treat one-file-folder and oversized-file cleanup as a separate structural
  inventory. It must follow the real layer graph and preserve the command-local
  contracts; it is not a reason to extract Route Move/Remove or create generic
  utility owners.

## Implementation update rule

This Task and its subtasks own all discoveries made while refactoring. The
The source Analysis remains a reasoning record rather than a second execution
plan. Record the affected group, owner decision,
behavior/evidence impact, and the next stop condition here before proceeding.

## Acceptance

M1, M2, M3, M4, and any M5 move that claims behavior preservation require
byte-identical characterization output against the recorded baseline plus
directly affected tests. M3 requires a reviewed output diff and an accepted
presentation contract; its reviewed output diff and accepted G4 contract are
complete. M5 remains a separate structure cleanup after the presentation seams
stabilize.
