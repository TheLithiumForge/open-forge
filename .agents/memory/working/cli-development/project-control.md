---
open-forge:
  description: Current CLI task selection and retained task ownership
  tags: [LoadNow, Memory, Working, CLI, Task, Contextual, Active]
---

# CLI Project Control

This ledger owns only current identity, queue state, ownership, and the next
boundary. The ledger as it stood at the beta release is preserved as the
[beta control ledger](../../archived/cli-development/project-control-beta.md).
Older receipts and retired queue state are in the
[archived control ledger](../../archived/cli-development/project-control.md).

## Active task ledger

| ID  | Task                                                                                 | State  | Current boundary                                                             | Owner                    |
| --- | ------------------------------------------------------------------------------------ | ------ | ---------------------------------------------------------------------------- | ------------------------ |
| 32  | [Minimal output sweep](tasks/task32-minimal-output-sweep.md)                         | Active | Bounded pending/next-action polish integrated under [Task 69](tasks/task69-next-beta-stabilization-release.md); full sweep remains queued | Root (Worker Watch) |
| 34  | [Interpolated value markup](tasks/task34-interpolated-value-markup.md)               | Open   | Do before 1.0; write the rule down first                                     | Unassigned               |
| 36  | [Extension merge and guards](tasks/task36-extension-merge-and-guards.md)             | Open   | Needs the maintainer's decision; now holds Task 30's heading direction       | Maintainer               |
| 37  | [Wording review](tasks/task37-wording-review-against-proposals.md)                   | Open   | Needs the maintainer's scope decision; after 32 and 39                       | Maintainer               |
| 39  | [Output audit](tasks/task39-output-audit.md)                                         | Open   | Do before 1.0; now holds Task 41's checks                                    | Unassigned               |
| 40  | [Capture coverage](tasks/task40-capture-coverage.md)                                 | Open   | Do after 1.0; each fix lands with its capture meanwhile                      | Unassigned               |
| 44  | [Template and core file content](tasks/task44-template-content.md)                   | Open   | Do before 1.0; after 53 and 54                                               | Unassigned               |
| 47  | [Entrypoint reachability](tasks/task47-entrypoint-reachability.md)                   | Open   | Do before 1.0; its Skill indexing follow-up now holds Task 46                | Unassigned               |
| 48  | [Scoping for Extension routes](tasks/task48-scoping-for-extension-routes.md)         | Open   | Do after 1.0                                                                 | Unassigned               |
| 53  | [Loading and scoping audit](tasks/task53-loading-and-scoping-audit.md)               | Open   | Do next; now holds Task 60's loader question                                 | Unassigned               |
| 54  | [Tag trimming](tasks/task54-tag-trimming.md)                                         | Open   | Do before 1.0, shipped files only; after 53                                  | Unassigned               |
| 58  | [Demo-based evaluations](tasks/task58-demo-evals.md)                                 | Open   | Do after 1.0; needs a decision on cost and publication                       | Unassigned               |
| 59 | [Beta 2 release](tasks/task59-beta-2-release.md) | Complete | `0.9.0-beta.2` published; final release and package/site verification are recorded in Task 59 | Root |
| 61  | [Documentation accuracy and voice](tasks/task61-documentation-accuracy-and-voice.md) | Active | Current documentation reconciliation integrated under [Task 69](tasks/task69-next-beta-stabilization-release.md); maintainer review checkbox remains unchecked | Root (Worker Watch), parallel reviewers |
| 55  | [Alternative workspace root such as `.apm`](tasks/task55-alternative-root.md)       | Open   | Restored separately on 2026-09-29; bounded local triage recorded, full investigation open | Unassigned               |
| 62 | [Glob-scoped loading](tasks/task62-glob-scoped-loading.md) | Complete | Implementation reviewed and verified; candidate is integrated on `develop` and `main`. Publication remains in Task 59 | Root |
| 63  | [Keeping edits through updates](tasks/task63-keeping-edits-through-updates.md)       | Open   | Needs the maintainer's decision before 1.0                                   | Maintainer               |
| 64  | [CLI defects and contract drift](tasks/task64-cli-defects-and-contract-drift.md)     | Active | Accepted fixes integrated under [Task 69](tasks/task69-next-beta-stabilization-release.md); B6 follow-up remains open and current recovery blocking is preserved | Root (Worker Watch) |
| 65  | [Where open Tasks live](tasks/task65-where-open-tasks-live.md)                       | Open   | Recorded; needs the maintainer's decision                                    | Maintainer               |
| 66  | [Council polish](tasks/task66-council-polish.md)                                     | Active | Committed; answers to its questions on `task66-council-decisions`            | Root, council            |
| 67  | [Diagram labels](tasks/task67-diagram-labels.md)                                     | Active | Committed on `task67-diagram-labels`; the maintainer reviews                 | Root, council            |
| 68  | [Repository link validation](tasks/task68-repository-link-validation.md)             | Complete | Phase 2/2, milestone 3/3; exact scope and checker contract preserved, integrated/tested gate green, two stale anchors fixed | Root (Worker Watch) |
| 69  | [Next beta stabilization and release](tasks/task69-next-beta-stabilization-release.md) | Complete | Phase 3/3, milestone 5/5; beta4 qualification, publication, package, documentation, upgrade, and demonstration receipts complete | Root (Worker Watch) |

No completion grace has been consumed for Tasks 68 or 69.

**Closed on 2026-09-28**, following the
[open task review](../../emerging/analysis/open-task-review/_open-task-review.md):
[30](../../archived/cli-development/tasks/task30-cli-experience-remediation.md), [31](../../archived/cli-development/tasks/task31-implementation-duplication.md), [33](../../archived/cli-development/tasks/task33-managed-content-removal.md), [35](../../archived/cli-development/tasks/task35-removal-and-suppression-model.md), [41](../../archived/cli-development/tasks/task41-beta-journey-scenarios.md), [42](../../archived/cli-development/tasks/task42-minimal-core.md), [43](../../archived/cli-development/tasks/task43-workflows-as-skill.md), [52](../../archived/cli-development/tasks/task52-documentation-site.md), [57](../../archived/cli-development/tasks/task57-onboarding-and-demos.md), [60](../../archived/cli-development/tasks/task60-cli-skill.md). Task 46 folded into Task 47's Skill indexing follow-up and Task
55 into Task 62. Task 55 was restored separately on 2026-09-29 at the maintainer's
request. Its archived record preserves that earlier folding. Task 56 has no separate record: its
outcome is the 2026-09-25 archive.

Other records, such as the [beta follow-ups](tasks/beta-follow-ups.md) and the
[candidate queue](tasks/potential/_potential.md), are available on demand in
[Open CLI Tasks](tasks/_tasks.md).

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

The current selection is the 1.0 polish queue following the completed
[Task 69](tasks/task69-next-beta-stabilization-release.md) beta4 closeout. Root
owns acceptance and execution runs through Worker Watch.

1. Continue [Task 64](tasks/task64-cli-defects-and-contract-drift.md) for its
   separate B6 follow-up, [Task 61](tasks/task61-documentation-accuracy-and-voice.md)
   for current documentation reconciliation, and [Task 32](tasks/task32-minimal-output-sweep.md)
   for bounded polish. Keep the full Task 32 sweep queued.
2. [Task 68](tasks/task68-repository-link-validation.md) is complete at phase
   2/2, milestone 3/3. Its exact named-root scope, parser/resolver boundary,
   checker contract, tests, and wiring remain accepted. The two stale anchors
   are fixed, and no source beyond the named roots is silently excluded. Task
  69 is complete at phase 3/3, milestone 5/5. Its detailed release receipt is
  linked above. The proposed B6 relaxation is deferred separately, and beta4
  preserves current recovery blocking behavior.
3. Treat `0.9.0-beta.4` as the current public release. Preserve Task 59's beta
   2 receipt and the earlier beta3 limited-verification record as historical.

The detailed candidate, test, package, upgrade, merge, hosted-run, and cleanup
evidence is in [Task 69's candidate and merge receipt](tasks/task69-next-beta-stabilization-release.md#verified-candidate-and-merge-receipt).
Prior failed or stale-assertion runs are historical resolved evidence, not
current blockers.

Root applied the exact five generated hosts after rerunning index outside the
worker lock restriction, and the second dry-run reported zero effects and
findings. Scratch customization completed all five retry steps with exit 0 and
no findings, and removal settings and ownership are correct.

The exact final candidate, hosted-run boundary, and publication receipt are
recorded in Task 69. One whole-candidate review and one focused recheck were
consumed with no material findings. One grouped correction pass was consumed,
with zero councils or completion grace consumed. Task 53's full loading audit
remains outside this horizon. Task 55's bounded local triage is recorded, while
its full investigation remains open outside the horizon.
