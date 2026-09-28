---
open-forge:
  description: Open Task 64 to fix the CLI defects, wording errors, and contract contradictions found by the documentation review, and the leftovers of the closed removal and remediation Tasks
  tags: [Memory, Working, Task, CLI, Defect, Contract, Wording, Contextual, Active]
---

# Task 64 — CLI defects and contract drift

## Outcome

Recorded on 2026-09-28, when the maintainer accepted the
[open task review](../../../emerging/analysis/open-task-review/_open-task-review.md). One owner for findings that no
other open Task covers. Recommended to do next, alongside
[Task 53](task53-loading-and-scoping-audit.md).

## Scope

- **CLI behavior and wording defects:** the seven behavior findings and six
  wording findings under "Findings for follow-up" in
  [Task 61](task61-documentation-accuracy-and-voice.md). One wording defect has
  a clear root cause: `LibraryDetachWording.cs` passes "Would" where it needs
  "Would update". The wrong `Next:` line for a kept recovery bundle belongs to
  [Task 32](task32-minimal-output-sweep.md) instead.
- **Contract contradictions:** five of the seven in Task 61. The Extension
  install and create contracts on confirmation, the update contract's
  "non-shipping" wording, the retired `memory-starters` example, help wrapping
  defined only in code, and `library attach` into a Framework category. The
  two removal contradictions are decided in
  [Task 63](task63-keeping-edits-through-updates.md).
- **From the closed removal Tasks:**
  [Task 33](../../../archived/cli-development/tasks/task33-managed-content-removal.md) and
  [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md).
  - Correct the `route remove` interface: its eligibility list, its ownership
    sentence, and the `ownership-claimed` next action.
  - Change the site's Customizing advice for hand-deleted files to
    `open-forge remove <path>`, and confirm with a scratch run that `extension
list` and `doctor` then stay quiet.
  - Write one Crystallized Decision for persistent removal intent, or extend
    the Workspace State Files decision. Record the shape, units, storage,
    consumers, the options not chosen, and the `registered-link-restored`
    split. That decision still lists only two keys for `.agents/open-forge.json`.
  - Fix the stale Task 50 path in the `WorkspaceRemovals.cs` doc comment.
- **From the closed [Task 30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md):** Phase 6,
  the gap between Doctor's `route.axioms-invalid` finding and the loader rule
  that a missing Axioms section adds no rules, and a current home for the
  shared presentation rules: the `Workspace:` echo, the `Next:` rule, and the
  shared message families. They live only in an archived convention file and
  in code, and Tasks 32 and 34 point their acceptance at the archive.

## Done when

- [ ] Each defect is fixed with a capture or test that pins it, or recorded as
      accepted behavior with a reason.
- [ ] Each contract contradiction is resolved in the contract, the help, or the
      code, so the three agree.
- [ ] The removal Decision is recorded, and the `route remove` interface and
      the Customizing page match it.
- [ ] The shared presentation rules live in a current source that Tasks 32 and
      34 can point to.

## Current State

**Now:** recorded, not started.
