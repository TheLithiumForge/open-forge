---
open-forge:
  description: Current CLI task selection and retained task ownership
  tags: [LoadNow, Memory, Working, CLI, Task, Contextual, Active]
---

# CLI Project Control

This ledger owns only current identity, queue state, ownership, and the next
boundary. Historical receipts and retired queue state are preserved in the
[archived control ledger](../../archived/cli-development/project-control.md).

## Active task ledger

| ID  | Task                                                                             | State                       | Current boundary                                                                                           | Owner                                                  |
| --- | -------------------------------------------------------------------------------- | --------------------------- | ---------------------------------------------------------------------------------------------------------- | ------------------------------------------------------ |
| 30  | [CLI Experience Remediation](tasks/task30-cli-experience-remediation.md)         | Active                      | G1/B1/G4 complete; Phase 5-D is next                                                                       | Root, direct sequential                                |
| 31  | [Implementation Duplication Removal](tasks/task31-implementation-duplication.md) | Open                        | M1/M2/M3/M4 complete; M5 waits for stable structure                                                        | Root, direct sequential                                |
| 38  | [Project And Test Split](tasks/task38-project-and-test-split.md)                 | Complete                    | G0–G4 accepted; implementation committed locally; preserved task/history edits remain                      | Root, Astra high overseer and direct stage coordinator |
| 45  | [End-to-end observability](tasks/task45-end-to-end-observability.md)             | Complete for approved scope | G5/G6 accepted; post-G6 correction resolves all four failures;17platform skips and C17-07 deferment remain | Root, Astra high coordinator                           |

The maintainer selected Task 38 planning on 2026-09-19 and structural execution
on 2026-09-20. Its
[master plan](tasks/task38/plan.md) owns the current migration checkpoint and
step progression. Task 30/31 state above is retained; it does not dispatch
concurrent work on Task 38's reserved files. Task 41 scenario approval and
Task 45 journey implementation are complete for the explicitly approved scope. Additional scenario selection remains a later boundary.
Tasks 28 and 29 and earlier completed records remain historical evidence.

## Ownership rules

- The Task record is the current authority for its scope, state, decisions, and
  implementation reality.
- A phase subtask may hold measured analysis data and acceptance evidence, but
  it cannot broaden the parent Task or create a second current plan.
- The Emerging [CLI Experience Audit](../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md)
  and [CLI Design Retrospective](../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
  are contextual reasoning sources. Do not rewrite them into execution records
  when code findings change.
- Completed work may be recovered from
  [Archived CLI Development](../../archived/cli-development/_cli-development.md),
  but it is not active context.

## Current next action

Task 51, [Public beta release](../../archived/cli-development/tasks/task51-beta-release.md),
is complete. Version `0.9.0-beta.1` is published on GitHub and npm, with all six
native hosts qualified and fresh public npm installation verified. Task 50, [Unified
Remove](tasks/task50-unified-remove.md), was squash-integrated into `develop` at
`a887e7304`; its record retains the implementation and local verification evidence.

The selected work is the [lossless source wording proposal](../src-wording-proposal.md)
on a separate branch. The maintainer authorized applying its eight replacements; source and
workspace counterparts are now aligned for review. The requested [default Skill indexing task](tasks/task47-default-skill-indexing.md)
is recorded under Task 47; no traversal implementation is released.

The beta correction slices D1–D6, E1–E2 and N1–N3 are complete and squash-integrated
into `develop`; their [execution record](tasks/beta-follow-ups/execution.md)
retains the verification. Task 38 migration and Task 45 approved journeys remain
complete. Do not dispatch older sequences as competing current work.

## Retained Task 30 And 31 State

Task 30 G4 is complete: all G4 subtasks, verification, and documentation
propagation are recorded in the packet. The six executable test suites run with
`--parallel collections`; the Native AOT gate requires `vswhere` on `PATH` and
must be run by the overseer on an unsandboxed host. G1 is complete through A1–A6 and
[P1](tasks/task30/06p-p1-shared-permissions.md); [B1](tasks/task30/07-b1-heading-entries.md)
completed heading migration and its managed/native integration wave. Safe M1 is
complete and now has that native qualification. The slice receipts and
[progression report](tasks/task30/progression-report.md) contain reviewed snapshots,
source/artifact identities and all resolved divergences. The fixed list-only
boundary in [05](tasks/task30-g4/05-index-entries-region.md) is merged and
verified. Structural items 2 and 4 remain deferred; M5 remains the next Task 31
structure step. Do not run `index` over this repository for investigation.

The reviewed implementation was integrated locally at `e40492aa`. Subsequent
maintainer annotations closed the [SG-R1/SG-R2 follow-ups](tasks/task30/review-second-gate-pre-g4.md#accepted-disposition-and-follow-ups):
SG-R1 contracts are corrected and SG-R2's legacy notice requirement is retired.
The [G4 preparation record](tasks/task30/phase-4b-g4.md#preparation-reconciliation)
tracks the bounded assumption check and now records the completed closeout. The
proposed AGENTS heading transition remains under the structural follow-up.

## Branch boundary

The implementation was qualified on `task30-a4-extension-readers`, based on
`7fb104e4` from `feature/development-2`, and merged into that target at `e40492aa`.
The task branch retains the reviewed snapshot and behavior
history. Remote publication and branch/worktree deletion are outside this action.

Task45 continuation update: G5 accepted in [g5-acceptance.json](tasks/task45/_task45.md); G6 accepted in [g6-acceptance.json](tasks/task45/_task45.md), with [flow-by-flow results](tasks/task45/g6-flow-report.md).
