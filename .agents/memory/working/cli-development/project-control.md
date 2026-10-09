---
open-forge:
  description: Current CLI task selection and retained task ownership
  tags: [LoadNow, Memory, Working, CLI, Task, Contextual, Active]
---

# CLI Project Control

## Current release, 2026-10-09

[beta10 release](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.10) is published from `4364a5c4c` after the Task 76 squash into develop. [Release 37990754560](https://github.com/TheLithiumForge/open-forge/actions/runs/37990754560) rebuilt, retested and published all six platforms. [Task 76](../../archived/cli-development/tasks/task76-simple-wizards-and-honest-change-wording.md#beta-10-release-2026-10-09) retains the receipt, including the Windows ARM64 failure in [Build 37983060029](https://github.com/TheLithiumForge/open-forge/actions/runs/37983060029) that its logs did not let us diagnose. Main is fast-forwarded to develop, which publishes the documentation site.

## Active task ledger

This ledger owns permanent identity, queue state and ownership. The Task owns its scope, phase, milestones and evidence. Open includes paused, deferred and unselected work. No stable global Task horizon is declared.

| ID  | Task                                                                                       | State                                                      | Current boundary                                                                                                                                                            | Owner              |
| --- | ------------------------------------------------------------------------------------------ | ---------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| 71  | [Streamline build and release delivery](tasks/task71-streamline-build-release-pipeline.md) | Open follow-up requested during beta5 publication          | Compare platform steps and timings, diagnose stalls, reduce repeated builds, and make publication cancellation effective. No redesign started                               | Root               |
| 39  | [Output Audit](tasks/task39-output-audit.md)                                               | Paused by user, 2026-10-01                                 | Wave phase 2/3, milestone 1/3; final checkpoint `r_f4bdc49aa2c4` saved; all workers stopped; no beta5 integration or captures                                               | Root; Worker Watch |
| 34  | [Interpolated Value Markup](tasks/task34-interpolated-value-markup.md)                     | Paused by user, 2026-10-01                                 | Wave phase 2/3, milestone 1/3; final literal-correction checkpoint `r_b68ae7955e36` saved; all workers stopped; no beta5 integration or captures                            | Root; Worker Watch |
| 48  | [Scoping for Extension routes](tasks/task48-scoping-for-extension-routes.md)               | Frozen at A/B1; deferred beyond beta5; required before 1.0 | Wave phase 2/3, milestone 1/3; corrected Task 64 Init accepted for later resume; Core-first scope unchanged. Checkpoint receipts remain in wave capsule                     | Root; Worker Watch |
| 53  | [Loading and scoping audit](tasks/task53-loading-and-scoping-audit.md)                     | Open, deferred                                             | Full audit postponed by the 2026-10-01 selection                                                                                                                            | Unassigned         |
| 44  | [Template and core file content](tasks/task44-template-content.md)                         | Open, deferred                                             | Template and Core polish is not selected                                                                                                                                    | Unassigned         |
| 36  | [Extension merge and guards](tasks/task36-extension-merge-and-guards.md)                   | Open, deferred                                             | Partial merging is postponed                                                                                                                                                | Maintainer         |
| 63  | [Keeping edits through updates](tasks/task63-keeping-edits-through-updates.md)             | Open, deferred                                             | Managed-file edit policy is postponed                                                                                                                                       | Maintainer         |
| 61  | [Documentation accuracy and voice](tasks/task61-documentation-accuracy-and-voice.md)       | Open                                                       | Delivered documentation is retained. The broader maintainer accuracy and voice review remains postponed                                                                     | Root               |
| 66  | [Council polish](tasks/task66-council-polish.md)                                           | Open, deferred                                             | Existing committed review material remains; prior review is postponed                                                                                                       | Root               |
| 67  | [Diagram labels](tasks/task67-diagram-labels.md)                                           | Open, deferred                                             | Existing committed review material remains; prior review is postponed                                                                                                       | Root               |
| 37  | [Wording review](tasks/task37-wording-review-against-proposals.md)                         | Open, unselected                                           | Prior scope decision remains pending; no selection in this wave                                                                                                             | Maintainer         |
| 40  | [Capture coverage](tasks/task40-capture-coverage.md)                                       | Open, deferred                                             | Broad capture expansion postponed until after 1.0                                                                                                                           | Unassigned         |
| 58  | [Demo-based evaluations](tasks/task58-demo-evals.md)                                       | Open, deferred                                             | Remains after 1.0                                                                                                                                                           | Unassigned         |
| 62  | [Glob-scoped loading](tasks/task62-glob-scoped-loading.md)                                 | Open later review horizon                                  | Original phase 3/3, milestone 6/6 implementation shipped. Later Glob UX/review-polish phase 2/2, milestone 2/3 retains pending M3 maintainer review. See Task 62 Execution. | Root               |

## Completed onboarding release

Tasks [72](../../archived/cli-development/tasks/task72-extension-wizard-terminal-layout.md), [73](../../archived/cli-development/tasks/task73-layered-adoption-and-installation-choices.md) and [74](../../archived/cli-development/tasks/task74-library-attachment-git-ignore-choice.md) are accepted, integrated and released in beta6. The final local Windows six-mode gate passed 10,718 tests with 34 declared platform exclusions and verified executable closures. Matching hosted gates then passed on all six platforms. Their implementation records and the completed Task 70 release record are Archived. [Task 75](../../archived/cli-development/tasks/task75-workspace-frontmatter-form.md), the per-workspace frontmatter form, shipped in beta9 and is Archived. [Task 76](../../archived/cli-development/tasks/task76-simple-wizards-and-honest-change-wording.md), the simpler wizards and honest change wording, shipped in beta10 and is Archived.

## Retired identities and receipts

Completed Task IDs remain permanent. [Archived CLI Tasks](../../archived/cli-development/tasks/_tasks.md) keeps their records. The [ledger before this trim](../../archived/cli-development/project-control-2026-10-04.md#active-task-ledger) preserves the complete ID/name mapping and prior queue state, including completed Tasks 32, 47, 54, 55, 59, 64, 65, 68, 69, 72 and 73. Existing completion-grace counters are unchanged by this lifecycle cleanup. They are not a reason to retain completed execution files in Working.

## Ownership rules

- Each Task defines its current outcome, decisions and implementation reality. Subtasks narrow that scope.
- The [wave capsule](one-zero-polish-wave.md) preserves shared resume boundaries. Paused work stays paused until the maintainer resumes it.
- Candidate work and the remaining [Memory authority analysis](../../emerging/analysis/cli-design-retrospective/memory-authority-boundary.md) remain contextual.
- Archived plans, analyses and receipts explain history. They do not create another current execution plan or grant release authorization.

## Current next action

The selected documentation, onboarding, Library improvement and Memory cleanup work is complete. Release receipts are retained in `artifacts/beta6-release/`. Task 71 remains queued for pipeline analysis. Paused, frozen, deferred and maintainer-review work stays in the [Task index](tasks/_tasks.md); no completed release task remains in Working.
