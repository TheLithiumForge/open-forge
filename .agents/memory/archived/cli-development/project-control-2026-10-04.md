---
open-forge:
  description: "Historical snapshot: Current CLI task selection and retained task ownership"
  tags: [Memory, CLI, Task, Contextual, Archived, Historical]
---

# CLI Project Control

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/project-control.md` while the live record was shortened to unfinished work. This snapshot preserves earlier plans, decisions and receipts. Read the current record for its remaining review or execution boundary. Historical timing, dispatch and release statements grant no current authorization.

## Onboarding execution, 2026-10-03

The isolated `docs/onboarding-and-presets` wave from develop `de54c1d` is
locally accepted. The [Task 73 capsule](tasks/task73-layered-adoption-and-installation-choices.md#active-execution-capsule-2026-10-03)
records the accepted setup choices, finding closure and final qualification.
Task 72's wizard correction is included. The combined changes remain
uncommitted, unmerged and unpublished, with their worktrees and evidence retained.

## Current release hold, 2026-10-02

Publication is stopped by the maintainer pending the reused-worktree stale
issue report in Task 70. Both remote branches reached `634ab070`, and the
six-host Build passed. Release cancellation raced the npm and GitHub publish
steps. The GitHub prerelease is now draft. npm beta5 remains published because
local npm authentication is unavailable. No further release is authorized.
The noon release automation is paused. The integration and publication boundary
in the older ledger rows below is superseded by this hold.

This ledger owns current identity, queue state, ownership, and the next
boundary. The [1.0 polish wave capsule](../../working/cli-development/one-zero-polish-wave.md) records the
selected task horizons and frozen decisions. The ledger as it stood at the beta
release is preserved as the
[beta control ledger](project-control-beta.md).
Older receipts and retired queue state are in the
[archived control ledger](project-control.md).

## Historical noon release checkpoint, 2026-10-02

This checkpoint predates the current release hold. Its pending publication
statements describe that earlier boundary and do not authorize further action.

The frozen candidate was integrated and pushed at 12:01 as
`4bb4059e319e5bf536aa7f0a0969a5fd246547f4`. Hosted qualification found a missing
Windows ConPTY skip reason in the delivery exclusion list. The exact reason and
its regression check are corrected; delivery checks pass. A new hosted Build
must pass before publication. Beta5 is not released, and main has not moved.
The Task 70 noon record owns this correction and evidence. The original local
qualification remains intact; its complete tree is the initial squash tree.

## Active task ledger

| ID  | Task                                                                                                                                             | State                                                                                 | Current boundary                                                                                                                                                                                                                                                                              | Owner                         |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------- |
| 73  | [Layered adoption and installation choices](tasks/task73-layered-adoption-and-installation-choices.md)                                           | Locally accepted, phase 3/3, milestone 3/3; unpublished                               | Runtime/docs integrated; three required findings closed; six managed/native modes and final formatting pass; exact receipts retained in the Task capsule                                                                                                                                      | Root with isolated owners     |
| 72  | [Extension wizard layout](tasks/task72-extension-wizard-terminal-layout.md)                                                                      | Locally accepted; unpublished                                                         | Bounded shared viewport and permission details; real small-terminal/resize evidence; six combined modes and holistic finding closure pass                                                                                                                                                     | Root with wizard owner        |
| 71  | [Streamline build and release delivery](../../working/cli-development/tasks/task71-streamline-build-release-pipeline.md)                         | Open follow-up requested during beta5 publication                                     | Compare platform steps and timings, diagnose stalls, reduce repeated builds, and make publication cancellation effective. No redesign started                                                                                                                                                 | Root                          |
| 54  | [Tag trimming for 1.0](tasks/task54-tag-trimming.md)                                                                                             | Task-local work complete; locally integrated                                          | Tag trimming and its independent evidence are complete. Task 70 owns combined release acceptance.                                                                                                                                                                                             | Root (combined qualification) |
| 39  | [Output Audit](../../working/cli-development/tasks/task39-output-audit.md)                                                                       | Paused by user, 2026-10-01                                                            | Wave phase 2/3, milestone 1/3; final checkpoint `r_f4bdc49aa2c4` saved; all workers stopped; no beta5 integration or captures                                                                                                                                                                 | Root; Worker Watch            |
| 34  | [Interpolated Value Markup](../../working/cli-development/tasks/task34-interpolated-value-markup.md)                                             | Paused by user, 2026-10-01                                                            | Wave phase 2/3, milestone 1/3; final literal-correction checkpoint `r_b68ae7955e36` saved; all workers stopped; no beta5 integration or captures                                                                                                                                              | Root; Worker Watch            |
| 47  | [Entrypoint reachability](tasks/task47-entrypoint-reachability.md); [Default Skill indexing follow-up](tasks/task47-default-skill-indexing.md)   | Task-local work complete; locally integrated                                          | The bounded default Skill catalogue Index bridge is complete and qualified. Task 70 owns combined release acceptance.                                                                                                                                                                         | Root (combined qualification) |
| 64  | [CLI defects and contract drift](tasks/task64-cli-defects-and-contract-drift.md)                                                                 | Task-local work complete; locally integrated                                          | Defect and contract corrections, broad checklist reconciliation, and independent qualification are complete. Task 70 owns combined release acceptance.                                                                                                                                        | Root (combined qualification) |
| 70  | [Existing workspace adoption during installation](../../working/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md) | Publication held; immediate stale-recovery correction locally validated on its branch | Original candidate integrated and six-host Build passed. Fresh correction passes 7,087 managed checks with 17 accepted platform exclusions and 79 focused Native AOT Install checks. Reviewable uncommitted fix; renewed release authorization and new-candidate release qualification remain | Root (release coordination)   |
| 48  | [Scoping for Extension routes](../../working/cli-development/tasks/task48-scoping-for-extension-routes.md)                                       | Frozen at A/B1; deferred beyond beta5; required before 1.0                            | Wave phase 2/3, milestone 1/3; corrected Task 64 Init accepted for later resume; Core-first scope unchanged. Checkpoint receipts remain in wave capsule                                                                                                                                       | Root; Worker Watch            |
| 32  | [Minimal Output Sweep](tasks/task32-minimal-output-sweep.md)                                                                                     | Task-local work complete; locally integrated                                          | Independent final qualification is complete. Combined release acceptance is tracked under Task 70 and remains subject to the release hold.                                                                                                                                                    | Root (combined qualification) |
| 53  | [Loading and scoping audit](../../working/cli-development/tasks/task53-loading-and-scoping-audit.md)                                             | Open, deferred                                                                        | Full audit postponed by the 2026-10-01 selection                                                                                                                                                                                                                                              | Unassigned                    |
| 44  | [Template and core file content](../../working/cli-development/tasks/task44-template-content.md)                                                 | Open, deferred                                                                        | Template and Core polish is not selected                                                                                                                                                                                                                                                      | Unassigned                    |
| 36  | [Extension merge and guards](../../working/cli-development/tasks/task36-extension-merge-and-guards.md)                                           | Open, deferred                                                                        | Partial merging is postponed                                                                                                                                                                                                                                                                  | Maintainer                    |
| 63  | [Keeping edits through updates](../../working/cli-development/tasks/task63-keeping-edits-through-updates.md)                                     | Open, deferred                                                                        | Managed-file edit policy is postponed                                                                                                                                                                                                                                                         | Maintainer                    |
| 61  | [Documentation accuracy and voice](../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md)                               | Open                                                                                  | Current beta5 documentation reconciliation is Root-owned in this wave. Maintainer review remains postponed                                                                                                                                                                                    | Root                          |
| 66  | [Council polish](../../working/cli-development/tasks/task66-council-polish.md)                                                                   | Open, deferred                                                                        | Existing committed review material remains; prior review is postponed                                                                                                                                                                                                                         | Root                          |
| 67  | [Diagram labels](../../working/cli-development/tasks/task67-diagram-labels.md)                                                                   | Open, deferred                                                                        | Existing committed review material remains; prior review is postponed                                                                                                                                                                                                                         | Root                          |
| 37  | [Wording review](../../working/cli-development/tasks/task37-wording-review-against-proposals.md)                                                 | Open, unselected                                                                      | Prior scope decision remains pending; no selection in this wave                                                                                                                                                                                                                               | Maintainer                    |
| 40  | [Capture coverage](../../working/cli-development/tasks/task40-capture-coverage.md)                                                               | Open, deferred                                                                        | Broad capture expansion postponed until after 1.0                                                                                                                                                                                                                                             | Unassigned                    |
| 58  | [Demo-based evaluations](../../working/cli-development/tasks/task58-demo-evals.md)                                                               | Open, deferred                                                                        | Remains after 1.0                                                                                                                                                                                                                                                                             | Unassigned                    |
| 55  | [Alternative workspace root such as `.apm`](tasks/task55-alternative-root-decision.md)                                                           | Complete                                                                              | Phase 1/1, milestone 1/1; current root retained; APM coexistence is not certified                                                                                                                                                                                                             | Root                          |
| 65  | [Where open Tasks live](tasks/task65-where-open-tasks-live.md)                                                                                   | Complete                                                                              | Phase 1/1, milestone 1/1; open Tasks stay in Working Memory; broader Local Planning review remains open                                                                                                                                                                                       | Root                          |
| 59  | [Beta 2 release](tasks/task59-beta-2-release.md)                                                                                                 | Complete                                                                              | `0.9.0-beta.2` publication and package/site verification remain historical receipts                                                                                                                                                                                                           | Root                          |
| 62  | [Glob-scoped loading](../../working/cli-development/tasks/task62-glob-scoped-loading.md)                                                         | Open later review horizon                                                             | Original phase 3/3, milestone 6/6 implementation shipped. Later Glob UX/review-polish phase 2/2, milestone 2/3 retains pending M3 maintainer review. See Task 62 Execution.                                                                                                                   | Root                          |
| 68  | [Repository link validation](tasks/task68-repository-link-validation.md)                                                                         | Complete                                                                              | Phase 2/2, milestone 3/3; accepted checker and integration evidence remain unchanged                                                                                                                                                                                                          | Root (Worker Watch)           |
| 69  | [Next beta stabilization and release](tasks/task69-next-beta-stabilization-release.md)                                                           | Complete                                                                              | Phase 3/3, milestone 5/5; beta4 release receipts remain unchanged                                                                                                                                                                                                                             | Root (Worker Watch)           |

No completion grace has been consumed for Tasks 68 or 69.

**Closed on 2026-09-28**, following the
[open task review](analysis/open-task-review/_open-task-review.md):
[30](tasks/task30-cli-experience-remediation.md), [31](tasks/task31-implementation-duplication.md), [33](tasks/task33-managed-content-removal.md), [35](tasks/task35-removal-and-suppression-model.md), [41](tasks/task41-beta-journey-scenarios.md), [42](tasks/task42-minimal-core.md), [43](tasks/task43-workflows-as-skill.md), [52](tasks/task52-documentation-site.md), [57](tasks/task57-onboarding-and-demos.md), [60](tasks/task60-cli-skill.md). Task 46 folded into Task 47's Skill indexing follow-up and Task
55 into Task 62. Task 55 was restored separately on 2026-09-29 at the maintainer's
request. Its archived record preserves that earlier folding. Task 56 has no separate record: its
outcome is the 2026-09-25 archive.

Other records, such as the [beta follow-ups](../../working/cli-development/tasks/beta-follow-ups.md) and the
[candidate queue](../../working/cli-development/tasks/potential/_potential.md), are available on demand in
[Open CLI Tasks](../../working/cli-development/tasks/_tasks.md).

## Ownership rules

- The Task record is the current authority for its scope, state, decisions, and
  implementation reality.
- A phase subtask may hold measured analysis data and acceptance evidence, but
  it cannot broaden the parent Task or create a second current plan.
- The Emerging [CLI Experience Audit](analysis/cli-experience-audit/_cli-experience-audit.md)
  and [CLI Design Retrospective](../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
  are contextual reasoning sources. Do not rewrite them into execution records
  when code findings change.
- Completed work may be recovered from
  [Archived CLI Development](_cli-development.md),
  but it is not active context.

## Current next action

Maintain the [Task 70 release hold](../../working/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md#current-release-hold-2026-10-02).
Its local reused-worktree correction and release investigation are a separate
workstream. The recorded publication cancellation race is unresolved, and
further publication requires renewed maintainer direction.

Tasks 72 and 73 are locally accepted in the isolated onboarding worktree.
Preserve their final source and qualification receipts for integration. Their
local acceptance does not establish merge or publication status. Task 71's
pipeline analysis remains an open follow-up.

Tasks 34 and 39 remain paused with their worktrees preserved. Task 48 remains
frozen at A/B1, deferred beyond beta5 and required before 1.0. Tasks 55 and 65
remain complete. Current queue state is in the ledger above. Older branch
identities and noon instructions are historical checkpoints.
