---
open-forge:
  description: Remaining beta product work and delivery verification after the CLI migration
  tags: [Memory, Working, Backlog, CLI, Beta, Contextual]
---

# Beta follow-up tasks

## Outcome and current state

Keep a practical selection list for the next work after the completed CLI
migration. The maintainer requested this list on 2026-09-21. Recording an item
does not approve new behavior or start its implementation. Existing numbered
Tasks retain their identity and define their own scope.

Stages 0–6 are complete for the approved scope: five libraries, separate test
projects, typed output wording, and 82 journey cases across 26 flows. See
[Task 38](../../../archived/cli-development/tasks/task38/_task38.md) and [Task 45](../../../archived/cli-development/tasks/task45/_task45.md).
The four former baseline failures were corrected. Do not reopen that migration.

This follow-up uses a dedicated feature branch based on `develop`, intended for
a later reviewed squash into `develop`. No release or publication is selected.

The maintainer subsequently approved the specified corrections for actionable errors, stale
documentation and Skill-resource navigation. The [selected specification packet](beta-follow-ups/_beta-follow-ups.md)
records investigated behavior, independent worker ownership and the [live execution](../../../archived/cli-development/tasks/beta-follow-ups/execution.md). It does not activate the other backlog items.

Older Task headers and counts sometimes predate completed changes. Reproduce a
reported gap against current source before assigning a fix. Historical counts
below are deliberately not presented as current measurements.

## Product work to prioritize for beta

- [ ] **Make errors tell the user what failed and what to do.**
      [Task 39](task39-output-audit.md) still calls for replacing cause-string
      interpretation with the path and failure facts the operation already knows.
      The representative manifest-error correction is implemented and all six
      managed/native test modes pass. `PlainCause` and `CauseSentence` still have other production callers. Recheck the
      remaining cases, then ensure each applicable error names its subject and an
      accurate next action. Preserve detailed raw causes where the contract permits
      them. Review complete rendered messages, not isolated sentences.
- [ ] **Finish the concise-output and wording review.**
      [Task 32](../../../archived/cli-development/tasks/task32-minimal-output-sweep.md) completed its minimal-output
      policy and qualification. [Task 34](task34-interpolated-value-markup.md)
      retains the code-markup work but is paused by the maintainer.
      [Task 37](task37-wording-review-against-proposals.md) reviews wording against
      earlier proposals. Changes need explicit per-situation expectations and
      matching contracts; this is not permission for a bulk rewrite.
- [ ] **Polish the files a new user actually installs.**
      [Task 44](task44-template-content.md) reviews Core files and templates so a
      reader knows what belongs in each file. Keep useful starters short and check
      their descriptions against their contents. The reduced Core and workflow
      Skill already landed; do not repeat those moves.
- [x] **Say that Open Forge is built to work beside each harness.**
      Recorded on 2026-09-30 from the maintainer. The README and the site's
      Introduction now say Open Forge is built to work beside the harness,
      encourage its native features, and name APM as a translator. Limits
      points back to that statement.
- [x] **Reconcile documentation left behind by the Core reduction.**
      [Tasks 42](../../../archived/cli-development/tasks/task42-minimal-core.md)/[43](../../../archived/cli-development/tasks/task43-workflows-as-skill.md) now have
      aligned payload maintenance and conceptual documentation: six Core primitives,
      four Core Memory states, optional deeper Memory roles and Skill-based recipes.
      Historical ownership-transfer and user-edited-file upgrade requirements remain
      unqualified; this documentation correction does not claim those guarantees.
- [x] **Settle the selected Skill-resource navigation question.**
      [Task 46](../../../archived/cli-development/tasks/task46-routed-skill-resources.md) and
      [Task 47](../../../archived/cli-development/tasks/task47-entrypoint-reachability.md) were rechecked against current
      fresh installs. Superseded reports are marked; Doctor now names the expected
      missing destination and advertises the owning catalogue's targeted Index.
      A published journey follows that advice successfully without changing recipe
      or Skill bytes. The maintainer requested the [default Skill indexing follow-up](../../../archived/cli-development/tasks/task47-default-skill-indexing.md);
      its bounded Index-only catalogue bridge is implemented and independently
      qualified. No Task 47 requirement remains. This does not change startup
      activation or shared runtime topology.
- [ ] **Make more real failures visible to automated checks.**
      [Task 40](task40-capture-coverage.md) covers missing output captures. Recount
      against the current contracts and test tree, prioritize actionable blocking
      findings, and retain genuine OS-specific evidence. More captures must not
      silently change wording or bless incorrect behavior.
- [ ] **Select the next useful user journeys.**
      Tasks 41 and 45 are closed, so give this an owner when it's selected.
      Compare the deferred permutations with the 82 implemented cases, then choose
      realistic gaps and agree on expectations before adding tests. Do not treat all
      438 source scenario identities as approved or implemented.

## Separate design follow-ups

These were recorded as later work. They are not automatically beta blockers.

- [x] **Remove one managed file without the next update restoring it.** Done by
      Task 50's `remove`. Tasks 33 and 35 closed on 2026-09-28.
      [Task 33](../../../archived/cli-development/tasks/task33-managed-content-removal.md) records the accepted capability.
      [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md) records the accepted
      removal and suppression model. Task 63 retains the broader direct-edit policy.
- [ ] **Create scopes under routes supplied by Extensions.**
      [Task 48](task48-scoping-for-extension-routes.md) extends route scaffolding
      beyond Core. Settle command naming, ownership and later Extension removal;
      restore the relevant missing route-init coverage without expanding Core.
- [ ] **Decide how Extensions and user content can share a file.**
      [Task 36](task36-extension-merge-and-guards.md) holds partial merging and guard
      replacement. Reconcile earlier heading migration first and retain only the
      unresolved behavior. No general merge mechanism is approved by this list.
- [ ] **Select further implementation cleanup only from a current finding.**
      [Task 31](../../../archived/cli-development/tasks/task31-implementation-duplication.md) is closed. Its selected
      duplication work is complete, and the Route Move/Remove policy differences
      were deliberately retained. Optional later cleanup needs a new bounded scope.
- [ ] **Resolve unsafe Markdown unlinking only from a concrete example.**
      C17-07 remains deferred in Task 45. Supply an example where removing a link
      cannot preserve authored meaning, then agree on the expected refusal or edit.
      Do not invent an unsafe case merely to fill coverage.

## Completed delivery verification

The prior six-host and downloadable-archive work completed in the recorded beta releases. [Task 69](../../../archived/cli-development/tasks/task69-next-beta-stabilization-release.md) retains the beta4 receipt. The beta5 hold and cancellation race are historical; the corrected combined beta6 release completed under renewed maintainer authorization. [Task 70](../../../archived/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md#beta-6-release-complete-2026-10-04) retains exact-source hosted qualification and public postchecks. Earlier receipts do not authorize a future release or qualify a different source candidate.

## Suggested sequence and completion

Use the [current ledger](../project-control.md#active-task-ledger) for selection.
Task 39 and Task 34 remain paused. Task 48 is frozen and deferred beyond beta5.
The broader wording, installed-content, and capture work remains unselected or
deferred in its owning Tasks. This backlog does not resume any of them.

Each selected item needs a current reproduction or inventory, a bounded accepted
outcome, reviewed changes, and appropriate verification. This file contains no
new filesystem mocking system, broad filesystem abstraction, template engine,
or automatic approval to execute all later design work.
