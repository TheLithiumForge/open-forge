---
open-forge:
  description: Task 31 active phase packet for measured method duplication and the Route Move/Remove stop boundary
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Active, KeepInMind]
---

# Task 31 — Phase 3.5 Refactoring

## Evidence source

[Implementation Duplication](../../../../emerging/analysis/cli-experience-audit/implementation-duplication.md)
remains in Emerging. It measured 59 family-level groups at roughly 793 lines and 22
cross-family groups at roughly 348 lines. The prior characterization baseline
is 18 pre-existing unit failures of 3,365 and 297 disposable captures.

> The sequencing sentence in the historical facts below predates G4 closeout.
> M3 is complete with G4; M5 is queued after the stabilized presentation
> structure.

## Completed facts

- The status policy owner, option readers, Route binding singleton validation,
  `LibraryId.TryCreate`, and 88 command-line literals were consolidated in the
  behavior-preserving slice.
- M2 is complete: cross-family work already accepted by the prior Task record
  remains complete and is not reopened here.
- M4 is complete: constants were moved/composed by owner scope; its rule is
  recorded in the [constant subtask](phase-constants.md).

## Closed Enumeration And Stopped Work

- [M1's enumerated closeout](08-m1-enumerated-extractions.md) is complete:
  three identical mappings have one owner each; the Library validator group
  was retired because its request/plan/observation types have no shared shape.
  The four convention checks meet their gates, and 24 published outputs have
  zero diff. The next native integration wave is after B1 and before G4.
- Route Move and Route Remove are not a safe generic extraction. After the
  rename comparison, 73 paired files had 20 byte-identical files and 33 more at
  least 70% similarity, but their finding-code enums differ by destination
  behavior and those codes ship in JSON.
- Context `ReadValues`, Extension Create singleton validation, Library status
  selection, and lifecycle-adjacent reads are not behavior-preserving duplicates
  until their reachability and wording are proven.
- Do not touch lifecycle baseline or permission internals while Task 30 G1 is
  unresolved.

## Acceptance and update rule

Every behavior-preserving move needs zero-diff characterization output and
directly affected tests. If a supposed duplicate changes status, diagnostics,
finding codes, request shape, or JSON, record it as a contract decision in this
Task and stop. New findings update this subtask or Task 31; the Emerging
Analysis is not rewritten into a second execution plan.
