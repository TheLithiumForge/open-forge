---
open-forge:
  description: Review of every open and unstarted CLI development Task on 2026-09-28, with a recommendation per Task, a suggested order, the blockers, and the stale records found
  tags: [Memory, Analysis, TaskReview, Planning, Contextual, Candidate]
---

# Open Task Review

## What should happen to each open Task, and in what order?

The maintainer asked on 2026-09-28 for an analysis of every open and unstarted
Task: what it implies, its dependencies, what remains, whether it's a good
idea, and its pros and cons. Four reviewers each took a related group and
checked every record against the repository, not only against what the record
says. Each Task has its own file below. This entrypoint is the summary.

The recommendations are proposals. Nothing here changes a Task's state until
the maintainer accepts it and the ledger is updated.

Tasks 52, 57, and 61 were not reviewed, because they are complete or waiting
for review. Task 62 has its own
[Glob-Scoped Loading Analysis](../glob-scoped-loading.md).

## Blockers first

- **CI is red on `main` and `develop`.** The Build at `ebf50034` failed on all
  six hosts at the test step. The failures seen were integration snapshot
  mismatches from the `open-forge-cli` Skill: Install and Status snapshots
  still record the 14-file payload. This is
  [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md)'s open
  snapshot refresh. It blocks beta 2, and it blocks verifying any change to
  shipped files in Tasks 44, 53, 54, and 60. The refresh needs a machine or CI
  run where the integration suite can execute.
- **The npm `latest` tag still points at `0.9.0-beta.1`**, which has no README.
  The publish script refuses to move `latest` to a prerelease, so beta 2 alone
  won't change the package page. Moving it is a maintainer step with
  `npm dist-tag add`.
- **Merge Task 61 before the refresh.** It changes the README and the shipped
  Skill. Merging first means the snapshots are refreshed once.

## Recommendations

| Task                                                                        | Recommendation                          | Size                      | Main reason                                                                                                           |
| --------------------------------------------------------------------------- | --------------------------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| [30](task30-cli-experience-remediation.md) CLI Experience Remediation       | Close as done                           | Small                     | Its slices are implemented and tested. Five leftovers move to other Tasks.                                            |
| [31](task31-implementation-duplication.md) Implementation duplication       | Close as done                           | Small                     | M5 was applied in `dac3bb53`. One optional file split awaits a ruling.                                                |
| [32](task32-minimal-output-sweep.md) Minimal output sweep                   | Do before 1.0                           | Medium                    | 74 distinct minimal `Next:` lines, some wrong, such as `doctor` where `cleanup` is needed.                            |
| [33](task33-managed-content-removal.md) Managed content removal             | Close as done                           | Small                     | Task 50's `remove` delivered it. A few contract and documentation fixes remain.                                       |
| [34](task34-interpolated-value-markup.md) Interpolated value markup         | Do before 1.0                           | Large                     | The rule is accepted and now mechanical: all output strings live in one project.                                      |
| [35](task35-removal-and-suppression-model.md) Removal and suppression model | Close as done                           | Small                     | Task 50 answered its questions. Only a Decision record is missing.                                                    |
| [36](task36-extension-merge-and-guards.md) Extension merge and guards       | Needs a maintainer decision             | Medium                    | Partial merging contradicts the accepted whole-file rule. The guard question narrowed to `AGENTS.md` and `CLAUDE.md`. |
| [37](task37-wording-review-against-proposals.md) Wording review             | Needs a maintainer decision             | Large, Medium if narrowed | Its proposals predate later wording work. Narrow it to help text and shared messages.                                 |
| [39](task39-output-audit.md) Output audit                                   | Do before 1.0                           | Large                     | The per-command audit never ran, and a doubled "Fix it by hand." line remains.                                        |
| [40](task40-capture-coverage.md) Capture coverage                           | Do after 1.0                            | Large                     | About 650 of 867 contract codes have no capture. Before 1.0, each fix lands with its capture.                         |
| [41](task41-beta-journey-scenarios.md) Beta journey scenarios               | Close as done                           | Small                     | The flow collection and Task 45's journeys replaced it. Output checks move to Task 39.                                |
| [42](task42-minimal-core.md) Minimal core                                   | Close as done                           | Small                     | It shipped before the first npm release, so no user had the old layout.                                               |
| [43](task43-workflows-as-skill.md) Workflows as a Skill                     | Close as done                           | Small                     | All acceptance criteria hold in shipped files.                                                                        |
| [44](task44-template-content.md) Template and core file content             | Do before 1.0                           | Medium                    | A keep-or-cut read as a new user remains, including a loader over its line budget.                                    |
| [46](task46-routed-skill-resources.md) Routed Skill resources               | Fold into Task 47                       | Small                     | The reported failure no longer reproduces. Its open question is Task 47's.                                            |
| [47](task47-entrypoint-reachability.md) Entrypoint reachability             | Do before 1.0                           | Medium                    | `index` reports "current" while a Skill catalogue is stale, on an advertised journey.                                 |
| [48](task48-scoping-for-extension-routes.md) Scoping for Extension routes   | Do after 1.0                            | Medium                    | Useful but additive, with a manual workaround.                                                                        |
| [53](task53-loading-and-scoping-audit.md) Loading and scoping audit         | Do next                                 | Medium                    | Heads the 1.0 content chain and carries a shipped defect: an active Checkpoint can't refresh.                         |
| [54](task54-tag-trimming.md) Tag trimming                                   | Do before 1.0, shipped files only       | Small                     | Fix the public tag vocabulary before 1.0. `Template` and `Skill` must stay.                                           |
| [55](task55-alternative-root.md) Alternative root                           | Fold into Task 62                       | Small                     | APM handles only some primitive types and inlines instructions. Keep `.agents/` and handle APM interop in Task 62.    |
| [58](task58-demo-evals.md) Demo-based evaluations                           | Do after 1.0                            | Large, Medium as a pilot  | Worth doing, but needs a harness, hidden checks, and a decision on cost and publication.                              |
| [59](task59-beta-2-release.md) Beta 2 release                               | Do next                                 | Small                     | Ready once CI is green. Needs the `latest` tag decision.                                                              |
| [60](task60-cli-skill.md) CLI Skill loader question                         | Fold into Task 53                       | Small                     | Skills load at startup, so a loader pointer suffices. It saves about 225 tokens.                                      |
| [63](task63-keeping-edits-through-updates.md) Keeping edits through updates | Needs a maintainer decision, before 1.0 | Small to Medium           | The accepted Workspace State Files decision moved edit protection to recovery bundles and `git diff`.                 |

## Suggested order

1. **Unblock the release.** Merge Task 61, refresh the integration snapshots,
   and get CI green. Then release beta 2 (Task 59) and decide the `latest` tag.
2. **Close what is done:** Tasks 30, 31, 33, 35, 41, 42, and 43. Fold 46 into
   47, 55 into 62, and 60's loader question into 53. Update the ledger, the
   plan, and the Task index in the same change.
3. **Open one new Task for CLI defects and contract drift**, described below.
   Do it next, alongside Task 53.
4. **Decide before 1.0:** Task 36's host-file boundary, Task 63, Task 37's
   scope, and Task 62's syntax choices.
5. **Before 1.0, in this order:** Task 53, then 54, then 44, because each edits
   files the next one reads. Tasks 32, 39, and 34 run in parallel with them, and
   32 and 39 come before 37.
6. **After 1.0:** Tasks 40, 48, and 58.

## Proposed new Task: CLI defects and contract drift

The CLI reviewer proposes one Task, sized Medium, to do next. It would own:

- The seven CLI behavior defects and six wording defects recorded under
  "Findings for follow-up" in
  [Task 61](../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md).
- Five of its seven contract contradictions. The two about removal stay with
  Tasks 33, 35, and 63.
- The contract and documentation leftovers from Tasks 33 and 35, and Task 30's
  Phase 6.
- The gap between Doctor's `route.axioms-invalid` finding and the loader rule
  that a missing Axioms section adds no rules.

## Cross-cutting findings

- **The shared presentation rules have no current home.** The `Workspace:`
  echo, the `Next:` rule, and the shared message families live only in an
  archived convention file and in code. Tasks 32 and 34 both point their
  acceptance at the archive.
- **The shipped loader is over its budget:** 92 authored lines against its
  maintenance contract's 35 to 80. Task 44 or 53 should own the fix.
- **Two Skills contracts disagree** on whether Skill resources are reachable
  from the loader. Task 47 must choose.
- **`.agents/skills/` may be read natively by other tools.** APM's targets page
  lists Copilot, Cursor, Codex, Gemini, OpenCode, Windsurf, and Hermes. If each
  tool confirms it, Open Forge's Skills need no adapter, which matters for
  Tasks 55, 60, and 62.
- **Task 58's demos are the cheapest evidence** for Task 60 (is a loader
  pointer enough?) and Task 62 (do agents follow glob triggers?).
- **A heading-only boundary for `AGENTS.md` would swallow content.** In this
  repository, the `## Exact Mechanical Execution Exception` section would fall
  inside a `# Open Forge` region, and the next update would overwrite it. That
  argues against the heading-only option in Task 36.

## Stale records to fix

- **Ledger and plan:** Tasks 42, 43, 44, 46, and 47 are missing from
  `project-control.md`. The rows for 30, 31, 52, 57, and 59 are out of date: the
  pushes happened, Pages serves the site, and M5 is done.
- **Task index:** the beta ordering still treats Task 41 as the first blocker,
  and still queues Tasks 33 and 35.
- **Task records:** 30, 31, 32, 33, 34, 35, 36, 41, 42, 43, 44, 46, 48, 53, 54,
  59, 60, and 63 each have a stale state line or section. Each review names the
  specifics.
- **Other records:** `beta-follow-ups.md` still lists per-file removal as open.
  The Workspace State Files decision lists only two keys for
  `.agents/open-forge.json`. The doc comment in `WorkspaceRemovals.cs` points
  at Task 50's old Working path.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Review of Task 30 CLI Experience Remediation, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task30-cli-experience-remediation.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 31 Implementation duplication, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task31-implementation-duplication.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 32 Minimal output sweep, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task32-minimal-output-sweep.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 33 Managed Content Removal, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task33-managed-content-removal.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 34 Interpolated value markup, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task34-interpolated-value-markup.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 35 Removal and Suppression Model, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task35-removal-and-suppression-model.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 36 Extension Merge and Guards, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task36-extension-merge-and-guards.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 37 Wording review against proposals, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task37-wording-review-against-proposals.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 39 Output audit, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task39-output-audit.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 40 Capture coverage, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task40-capture-coverage.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 41 Beta journey scenarios, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task41-beta-journey-scenarios.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 42 Minimal Core, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task42-minimal-core.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 43 Workflows As A Skill, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task43-workflows-as-skill.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 44 Template And Core File Content, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task44-template-content.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 46 Routed Skill Resources, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task46-routed-skill-resources.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 47 Entrypoint Reachability and its default Skill indexing follow-up, covering what they imply, dependencies, remaining work, pros and cons, and a recommendation](task47-entrypoint-reachability.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 48 Scoping for Extension Routes, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task48-scoping-for-extension-routes.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 53 Loading And Scoping Audit, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task53-loading-and-scoping-audit.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 54 Tag Trimming, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task54-tag-trimming.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 55 Alternative root such as .apm, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task55-alternative-root.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 58 Demo-based evaluations, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task58-demo-evals.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 59 Beta 2 release, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task59-beta-2-release.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of the open loader question in Task 60 CLI Skill In Core, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task60-cli-skill.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
- [Review of Task 63 Keeping Edits Through Updates, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation](task63-keeping-edits-through-updates.md) - #Memory #Analysis #TaskReview #Contextual #Candidate
