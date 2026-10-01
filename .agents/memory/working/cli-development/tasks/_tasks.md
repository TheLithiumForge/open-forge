---
open-forge:
  description: Open CLI tasks, the selected migration and on-demand follow-ups
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Open CLI Tasks

The current selection is 1.0 polish, listed below. Open Tasks stay
here. Completed Tasks and their packets moved to Archived Memory on
2026-09-25, as listed under [Archived on 2026-09-25](#archived-on-2026-09-25).
Older completed and superseded records are preserved under
[Archived CLI Development](../../../archived/cli-development/_cli-development.md).

Read the parent Task before a phase subtask. A subtask carries actionable
evidence and may narrow, but never broaden, its parent Task.

The [Potential CLI Tasks](potential/_potential.md) route is a candidate queue,
not active execution authority.

Beta 2 was published on 2026-09-29. [Task 59](task59-beta-2-release.md) retains
the completed release and published-package verification receipts.

## Accepted beta-stabilization horizon

Accepted on 2026-10-01, [Task 69 — Next beta stabilization and
release](task69-next-beta-stabilization-release.md) coordinates the bounded
implementation, qualification, and release boundary. It is complete at phase
3/3, milestone 5/5: qualification, clean squash, hosted six-host gate,
publication, package, documentation, upgrade, and demonstration receipts are
complete. Root remains the owner and execution was via Worker Watch.
[Task 68 — Repository link validation](task68-repository-link-validation.md)
is complete at phase 2/2, milestone 3/3. Its exact scope and checker contract
remain accepted. No completion grace has been consumed.

## Current order

Accepted on 2026-09-28 from the
[open task review](../../../emerging/analysis/open-task-review/_open-task-review.md).
The ordering that came before it is preserved in
[Task ordering before 2026-09-28](../../../archived/cli-development/tasks/ordering-2026-09-25.md).

1. **Review 1.0 release readiness.** Use the root-authored
   [assessment](../../../emerging/analysis/one-zero-release-readiness.md) as
   contextual input. It does not authorize implementation.
2. The pre-beta order listed [53](task53-loading-and-scoping-audit.md) and
   [64](task64-cli-defects-and-contract-drift.md) as next after the 1.0
   direction was accepted. In the accepted beta-stabilization horizon,
   Task 53's full loading audit remains out of scope, while Task 64 stays open
   for the separate B6 follow-up after its accepted fixes.
   [66](task66-council-polish.md) is committed on its own branch and awaits review.
   [67](task67-diagram-labels.md) builds on it.
3. **Decide before 1.0:** [36](task36-extension-merge-and-guards.md),
   [37](task37-wording-review-against-proposals.md),
   [63](task63-keeping-edits-through-updates.md), and
   [65](task65-where-open-tasks-live.md).
4. **Before 1.0:** [53](task53-loading-and-scoping-audit.md), then
   [54](task54-tag-trimming.md), then [44](task44-template-content.md), because
   each edits files the next reads. [32](task32-minimal-output-sweep.md),
   [39](task39-output-audit.md), and [34](task34-interpolated-value-markup.md)
   run alongside, with 32 and 39 before 37.
   [47](task47-entrypoint-reachability.md) and its
   [Skill indexing follow-up](task47-default-skill-indexing.md) also land
   before 1.0.
5. **After 1.0:** [40](task40-capture-coverage.md),
   [48](task48-scoping-for-extension-routes.md), and
   [58](task58-demo-evals.md).

The remaining backlog items stay in [beta follow-ups](beta-follow-ups.md), and
the [candidate queue](potential/_potential.md) holds work not yet selected.

On 2026-09-29, the maintainer restored [Task 55](task55-alternative-root.md)
as a separate open investigation of alternative roots and APM interoperability.
It remains open, with bounded local triage recorded, and its scheduling is
independent. [Task 62](task62-glob-scoped-loading.md)
covers optional `applyTo` loading and CLI filtering. Its implementation and
qualification are complete in candidate
`00e3ba0294679163d95b42a81295091254debfb7`, now pushed to `develop` and `main`.
Task 59 completed beta 2 publication and package/site verification.

## Archived on 2026-09-28

These records moved to [Archived CLI Tasks](../../../archived/cli-development/tasks/_tasks.md)
when the maintainer accepted the review:

- Closed as done: [30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md) with its phase
  packets, [31](../../../archived/cli-development/tasks/task31-implementation-duplication.md) with its subtasks,
  [33](../../../archived/cli-development/tasks/task33-managed-content-removal.md),
  [35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md),
  [41](../../../archived/cli-development/tasks/task41-beta-journey-scenarios.md) with its journey records,
  [42](../../../archived/cli-development/tasks/task42-minimal-core.md), [43](../../../archived/cli-development/tasks/task43-workflows-as-skill.md), and
  [60](../../../archived/cli-development/tasks/task60-cli-skill.md).
- Complete: [52](../../../archived/cli-development/tasks/task52-documentation-site.md) and
  [57](../../../archived/cli-development/tasks/task57-onboarding-and-demos.md).
- Folded: [46](../../../archived/cli-development/tasks/task46-routed-skill-resources.md) into Task 47's Skill indexing
  follow-up, and [55](../../../archived/cli-development/tasks/task55-alternative-root.md) into Task 62.
  Task 55 was subsequently restored separately on 2026-09-29. The archived
  record remains historical provenance.

## Archived on 2026-09-25

These completed records moved to [Archived CLI Tasks](../../../archived/cli-development/tasks/_tasks.md).
Their links from open Tasks now point there.

- [Task 38, Project and Test Split](../../../archived/cli-development/tasks/task38-project-and-test-split.md), with its [migration packet](../../../archived/cli-development/tasks/task38/_task38.md)
- [Task 45, End-to-end observability](../../../archived/cli-development/tasks/task45-end-to-end-observability.md), with its [G6 evidence](../../../archived/cli-development/tasks/task45/_task45.md)
- [Task 50, Unified Remove](../../../archived/cli-development/tasks/task50-unified-remove.md)
- The completed [Task 30 G1, B1, and G4 packets](../../../archived/cli-development/tasks/task30/_task30.md) and the [G4 execution packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md)
- The completed [Task 31 M1, M3, and M4 subtasks](../../../archived/cli-development/tasks/task31/_task31.md)
- The completed [beta correction execution record](../../../archived/cli-development/tasks/beta-follow-ups/_beta-follow-ups.md)

## Entries

- [Remaining beta product work and delivery verification after the CLI migration](beta-follow-ups.md) - #Memory #Working #Backlog #CLI #Beta #Contextual
- [Specifications for actionable errors, stale Core documentation and Skill navigation follow-ups](beta-follow-ups/_beta-follow-ups.md) - #Memory #Working #Plan #CLI #Contextual
- [Candidate follow-up tasks distilled from the reviewed CLI analyses; not active execution authority](potential/_potential.md) - #Memory #Working #CLI #Task #Potential #Contextual
- [Open Task 32 to review whether the Workspace echo and the Next action belong in minimal output across all 28 commands](task32-minimal-output-sweep.md) - #Memory #Working #CLI #Task #Presentation #Minimal #Contextual #Active
- [Open Task 34 to visually distinguish command names, paths, identifiers and arguments interpolated into user-facing sentences, and to establish it as an authoring rule](task34-interpolated-value-markup.md) - #Memory #Working #CLI #Task #Presentation #Wording #Accessibility #Contextual #Active
- [Open Task 36 to design partial file merging by Extensions and to replace the comment guards in authored Markdown with a boundary an agent still reads as an instruction](task36-extension-merge-and-guards.md) - #Memory #Working #CLI #Task #Extensions #Markers #Authoring #Contextual #Active
- [Open Task 37 to compare every shipped CLI sentence against the unaccepted G4 output proposals and adopt, merge or reject each on its merits](task37-wording-review-against-proposals.md) - #Memory #Working #CLI #Task #Wording #Review #Contextual #Active
- [Open Task 39 to audit every CLI output for actionability, confirm the G4 conversion actually improved each command, and find remaining legacy and evidence gaps](task39-output-audit.md) - #Memory #Working #CLI #Task #Output #Audit #Regression #Contextual #Active
- [Open Task 40 to close the gap between the accepted finding vocabulary and the situations any capture actually exercises, so the output invariants guard more than a quarter of what the CLI can print](task40-capture-coverage.md) - #Memory #Working #CLI #Task #Capture #Coverage #Evidence #Contextual #Active
- [Open Task 44 to fix what the installed templates and core files say, since they are the first Open Forge prose a beta user reads](task44-template-content.md) - #Memory #Working #CLI #Task #Templates #Wording #Beta #Contextual #Active
- [Task 47 follow-up to make default Index reach routed resources through native Skills](task47-default-skill-indexing.md) - #Memory #Working #CLI #Task #Skill #Index #Contextual
- [Open Task 47 to make index and route navigation reach every recognized entrypoint form from the loader, so a catalogue does not need to be named by hand to stay current](task47-entrypoint-reachability.md) - #Memory #Working #CLI #Task #Routing #Navigation #Index #Beta #Contextual #Active
- [Open Task 48 to extend Framework route scaffolding and scope insertion to routes an Extension created, so scoping is a property of the routing model rather than of whatever Core happens to ship](task48-scoping-for-extension-routes.md) - #Memory #Working #CLI #Task #Routing #Scopes #Extensions #RouteInit #Contextual #Active
- [Open Task 53 to audit every LoadNow and KeepInMind entry and the default scoping before 1.0, so startup context holds only what omission would cost more than reading](task53-loading-and-scoping-audit.md) - #Memory #Working #Task #Framework #Loading #Scope #Release #Contextual #Active
- [Open Task 54 to trim tags that add no selection, search, or loading value before 1.0, in shipped files first and then in this workspace](task54-tag-trimming.md) - #Memory #Working #Task #Framework #Tags #Release #Contextual #Active
- [Investigate alternative Framework roots such as .apm and APM interoperability separately from applyTo loading](task55-alternative-root.md) - #Memory #Working #Task #Framework #Root #APM #Investigation #Contextual
- [Open Task 58 to turn the demos into a repeatable evaluation comparing Open Forge with other setups, agents, and models](task58-demo-evals.md) - #Memory #Working #Task #Evaluation #Demo #Contextual #Active
- [Record the beta 2 release, package publication, and documentation verification](task59-beta-2-release.md) - #Memory #Working #Task #Release #Beta #Package #Contextual #Complete
- [Task 61 accuracy and voice pass over the documentation site, the README, and the repository guides, with a diagram that separates Core from Extensions and startup from on-demand loading](task61-documentation-accuracy-and-voice.md) - #Memory #Working #Task #Documentation #Site #Diagram #Writing #Contextual #Active
- [Implement optional applyTo file conditions, loading rules, Entries display, and CLI context filtering through accepted execution packets](task62-glob-scoped-loading.md) - #Memory #Working #Task #Framework #Loading #Frontmatter #CLI #Contextual #Complete
- [Task 62 execution decisions, foundation packets, command packets, and documentation ownership](task62/_task62.md) - #Memory #Working #Task #Plan #CLI #Contextual #Active
- [Open Task 63 to decide whether and how a direct edit to a managed file can survive updates, since removedFiles already keeps such a file untouched as an undocumented side effect](task63-keeping-edits-through-updates.md) - #Memory #Working #Task #CLI #Update #Removal #Customization #Investigation #Contextual #Active
- [Open Task 64 to fix the CLI defects, wording errors, and contract contradictions found by the documentation review, and the leftovers of the closed removal and remediation Tasks](task64-cli-defects-and-contract-drift.md) - #Memory #Working #Task #CLI #Defect #Contract #Wording #Contextual #Active
- [Open Task 65 to decide where long-running open Tasks should live, in Working Memory as now, in a Planning backlog, or split between them](task65-where-open-tasks-live.md) - #Memory #Working #Task #Planning #Backlog #Contextual #Active
- [Task 66 council review of the published documentation for overclaims and polish, and of the shipped Framework and Extension files for consistency, redundancy, order, and readability](task66-council-polish.md) - #Memory #Working #Task #Documentation #Framework #Extensions #Council #Writing #Contextual #Active
- [Task 67 council redesign of the framework diagram's labels, so what loads when and where content comes from reads at a glance and stays accurate](task67-diagram-labels.md) - #Memory #Working #Task #Documentation #Diagram #Loading #Council #Writing #Contextual #Active
- [Offline validation of current README, public guides, and routed Markdown local paths and anchors](task68-repository-link-validation.md) - #Memory #Working #Task #CLI #Contextual #Complete
- [Coordinate the next beta stabilization, qualification, merge, and release boundary](task69-next-beta-stabilization-release.md) - #Memory #Working #Task #CLI #Contextual #Complete
