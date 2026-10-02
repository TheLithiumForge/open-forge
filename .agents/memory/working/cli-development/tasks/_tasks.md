---
open-forge:
  description: Open CLI tasks, the selected migration and on-demand follow-ups
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Open CLI Tasks

## Current release hold

The maintainer stopped beta5 publication after reporting a stale issue in a
reused worktree. [Task 70](task70-existing-workspace-adoption-during-installation.md#current-release-hold-2026-10-02)
records the investigation and publication cancellation race. No further release
is authorized. [Task 71](task71-streamline-build-release-pipeline.md) records
the requested pipeline follow-up. Older release boundary statements below are
historical checkpoints until this hold is resolved.

The [wave capsule](../one-zero-polish-wave.md) and
[project-control ledger](../project-control.md#active-task-ledger) record the
release boundary, task horizons, frozen decisions, and queue ownership. Open
Task records stay in Working Memory even when they are not currently active.
Existing archived records stay archived, and this selection requires no file
moves. Completed Tasks and their packets moved to Archived Memory on
2026-09-25, as listed under [Archived on 2026-09-25](#archived-on-2026-09-25).

Older completed and superseded records are preserved under
[Archived CLI Development](../../../archived/cli-development/_cli-development.md).

Read the parent Task before a phase subtask. A subtask carries actionable
evidence and may narrow, but never broaden, its parent Task.

The [Potential CLI Tasks](potential/_potential.md) route is a candidate queue,
not active execution authority.

Beta 2 was published on 2026-09-29. [Task 59](task59-beta-2-release.md) retains
the completed release and published-package verification receipts.

## Completed beta4 stabilization horizon

Accepted on 2026-10-01, [Task 69 — Next beta stabilization and
release](task69-next-beta-stabilization-release.md) coordinates the bounded
implementation, qualification, and release boundary. It is complete at phase
3/3, milestone 5/5: qualification, clean squash, hosted six-host gate,
publication, package, documentation, upgrade, and demonstration receipts are
complete. Root remains the owner and execution was via Worker Watch.
[Task 68 — Repository link validation](task68-repository-link-validation.md)
is complete at phase 2/2, milestone 3/3. Its exact scope and checker contract
remain accepted. No completion grace has been consumed.

These are completed beta4 and link-validation receipts. Beta4 remains the public
release; beta5 is selected under the release boundary below.

## Current order

Beta4 remains the public release. Beta5 is selected. Publication is authorized
only after a green, clean squash integration on `develop` and the exact six-host,
package, site, and user-journey gates pass. The [wave capsule](../one-zero-polish-wave.md)
records detailed release boundaries and horizons.
The [project-control ledger](../project-control.md#active-task-ledger) owns queue state and ownership.

1. **Top beta5 priority:** [Task 70 — Existing workspace adoption during
   installation](task70-existing-workspace-adoption-during-installation.md).
   Local v9 qualification and the one fresh review with grouped F1–F5
   corrections are complete; the guarded 77-source handoff is verified. Task70
   remains unintegrated until the authorized noon window. The first-install
   preservation correction and combined managed/native/package/site and actual
   published beta4-to-beta5 upgrade gates pass. Root accepts the local candidate;
   develop squash/push, hosted qualification, release and public postchecks remain.
2. Tasks 32 Minimal Output Sweep, 47 Entrypoint reachability and its default
   Skill indexing follow-up, 54 Tag trimming, and 64 CLI defects and contract
   drift are independently qualified and squash-integrated locally on
   `develop`. Local combined qualification passes; hosted release gates remain.
3. Tasks 39 Output Audit and 34 Interpolated Value Markup are paused with saved
   worktree state.
4. Task 48 Scoping for Extension routes is deferred beyond beta5 but remains
   required before 1.0.
5. [Task 37](task37-wording-review-against-proposals.md) is unselected and its
   earlier scope decision remains pending. Other beta follow-ups stay in
   [Beta follow-ups](beta-follow-ups.md), and unselected work stays in the
   [candidate queue](potential/_potential.md). Other existing postponed and
   unselected scope remains as recorded in the
   [wave capsule](../one-zero-polish-wave.md).
6. [Task 55](task55-alternative-root.md) and
   [Task 65](task65-where-open-tasks-live.md) are complete at phase 1/1,
   milestone 1/1. Task 55 retains the current root without claiming APM
   certification. Task 65 closes only the placement question; the broader
   [Local Planning review](../../local-planning.md) remains open.

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
- [Open Task 32 to reconcile minimal output across all 29 operations, including root Remove](task32-minimal-output-sweep.md) - #Memory #Working #CLI #Task #Presentation #Minimal #Contextual #Active
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
- [Completed Task 54 tag trimming, frozen scope, independent validation, and merge-ready receipt](task54-tag-trimming.md) - #Memory #Working #Task #Framework #Tags #Release #Contextual #Active
- [Record the decision to retain .agents as the sole Framework root and define the APM coexistence boundary](task55-alternative-root.md) - #Memory #Working #Task #Framework #Root #APM #Decision #Contextual #Complete
- [Open Task 58 to turn the demos into a repeatable evaluation comparing Open Forge with other setups, agents, and models](task58-demo-evals.md) - #Memory #Working #Task #Evaluation #Demo #Contextual #Active
- [Record the beta 2 release, package publication, and documentation verification](task59-beta-2-release.md) - #Memory #Working #Task #Release #Beta #Package #Contextual #Complete
- [Task 61 accuracy and voice pass over the documentation site, the README, and the repository guides, with a diagram that separates Core from Extensions and startup from on-demand loading](task61-documentation-accuracy-and-voice.md) - #Memory #Working #Task #Documentation #Site #Diagram #Writing #Contextual #Active
- [Implement optional applyTo file conditions, loading rules, Entries display, and CLI context filtering through accepted execution packets](task62-glob-scoped-loading.md) - #Memory #Working #Task #Framework #Loading #Frontmatter #CLI #Contextual #Complete
- [Task 62 execution decisions, foundation packets, command packets, and documentation ownership](task62/_task62.md) - #Memory #Working #Task #Plan #CLI #Contextual #Active
- [Open Task 63 to decide whether and how a direct edit to a managed file can survive updates, since removedFiles already keeps such a file untouched as an undocumented side effect](task63-keeping-edits-through-updates.md) - #Memory #Working #Task #CLI #Update #Removal #Customization #Investigation #Contextual #Active
- [Record completed Task 64 CLI defect and contract corrections with independent managed, native, package and qualification-closure evidence](task64-cli-defects-and-contract-drift.md) - #Memory #Working #Task #CLI #Defect #Contract #Wording #Contextual #Complete
- [Record the decision to keep Task records in Working Memory and use backlogs and ledgers as selection views](task65-where-open-tasks-live.md) - #Memory #Working #Task #Planning #Backlog #Contextual #Complete
- [Task 66 council review of the published documentation for overclaims and polish, and of the shipped Framework and Extension files for consistency, redundancy, order, and readability](task66-council-polish.md) - #Memory #Working #Task #Documentation #Framework #Extensions #Council #Writing #Contextual #Active
- [Task 67 council redesign of the framework diagram's labels, so what loads when and where content comes from reads at a glance and stays accurate](task67-diagram-labels.md) - #Memory #Working #Task #Documentation #Diagram #Loading #Council #Writing #Contextual #Active
- [Offline validation of current README, public guides, and routed Markdown local paths and anchors](task68-repository-link-validation.md) - #Memory #Working #Task #CLI #Contextual #Complete
- [Coordinate the next beta stabilization, qualification, merge, and release boundary](task69-next-beta-stabilization-release.md) - #Memory #Working #Task #CLI #Contextual #Complete
- [Define bounded adoption of compatible existing workspace metadata and route entrypoints during ordinary Framework installation and ordinary managed Update](task70-existing-workspace-adoption-during-installation.md) - #Memory #Working #Task #CLI #Install #Metadata #Contextual #Active
- [Streamline hosted build and release delivery while preserving required platform coverage and making stalled suites diagnosable](task71-streamline-build-release-pipeline.md) - #Memory #Working #Task #CLI #Pipeline #Build #Release #Contextual #Active
