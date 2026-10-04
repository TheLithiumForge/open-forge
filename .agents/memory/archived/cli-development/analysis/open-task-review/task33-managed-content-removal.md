---
open-forge:
  description: "Historical record: Review of Task 33 Managed Content Removal, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 33 Managed Content Removal Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task33-managed-content-removal.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 33 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 33](../../tasks/task33-managed-content-removal.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because only record keeping and three contract or documentation corrections remain.

[Task 50](../../tasks/task50-unified-remove.md) delivered the requirement Task 33 owns. The root `open-forge remove` command removes one managed file, directory, package, or Library. It records the choice in `.agents/open-forge.json`, and every installer, updater, and synchronizer honors it. The record still says "Open, not started", which is stale. Close it together with Task 35 and move the few residual contradictions into one small reconciliation step.

## What It Implies

Nothing new for users. They can already remove one Extension-installed file and keep it removed. Closing the task removes a stale item from the "After beta" list in the [task index](../../../../working/cli-development/tasks/_tasks.md) and from the [beta follow-up backlog](../../../../working/cli-development/tasks/beta-follow-ups.md), where "Remove one managed file without the next update restoring it" is still unchecked.

## State Today

Verified against the code, tests, and contracts at `815324f9`. Task 50 merged in `a887e730` on 2026-09-24.

| Task 33 acceptance                                         | State today                                                                                                                                                                  |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Per-file removal for Extensions and Libraries in one shape | Done. `remove <path>` handles managed files and individual Library links ([remove interface](../../../../crystallized/documents/cli/contracts/remove/interface.md))          |
| Removed file stays removed across updates                  | Done. Covered by `ExtensionUpdateRemovalSettingsIntegrationTests`, `UpdateRemovedFilesIntegrationTests`, and journey F10                                                     |
| Ownership and disk agree, `doctor` stays quiet             | Done for `remove`, which releases matching claims after verified effects. One gap remains, listed below                                                                      |
| A shared file cannot break its other claimant              | Changed by Task 50. An explicit file selection removes the file for every manager, and all managers then honor the exclusion. Package removal still keeps shared files       |
| Region claims supported or refused                         | Done. Whole-file removal releases region claims. A region claim alone never authorizes deleting its host                                                                     |
| A category is blocked by one managed file                  | Resolved. `route remove` now removes managed content and releases claims, pinned by "Route Remove persists a root-category exclusion and releases managed Extension content" |
| The removability promise matches                           | Done. The Principles promise removable, non-restoring defaults, and the CLI now keeps that promise                                                                           |

The record's "What is true today" section and its "blunt next actions" describe the behavior before Task 50. Three residual problems remain:

1. The [route remove interface](../../../../crystallized/documents/cli/contracts/route/remove/interface.md) still lists "a source with any trusted Framework or Extension lifecycle ownership claim" as ineligible and says the command does not release lifecycle ownership. Its own `managed-source` scenario and entrypoint say the opposite. [Task 61](../../../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md) confirmed with a real run that the CLI removes the file.
2. The same contract's `route-remove.ownership-claimed` row still suggests `open-forge update` and `open-forge extension remove <id>`. The finding now fires only for stale claims, so those next actions point the wrong way.
3. The site's [Customizing page](../../../../../../src/docusaurus/docs/concepts/customizing.md) tells users to add a hand-deleted file to `removedFiles` by hand. That leaves the ownership claim in the lock. `extension list` reads no settings, so it probably still reports `installed-files-missing`. `open-forge remove <path>` on a missing file releases the stale claim and is better advice. I read this from the code and did not run it.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing. [Task 63](../../../../working/cli-development/tasks/task63-keeping-edits-through-updates.md) builds on the same exclusion model.
- **Overlaps with:** [Task 35](../../tasks/task35-removal-and-suppression-model.md), which the same delivery answered, and the Task 61 contract findings.

## Remaining Work

1. Mark Task 33 complete, cite Task 50, and state the reversed shared-file criterion. Archive it with Task 35.
2. Correct the route remove interface: the eligibility list, the ownership sentence, and the `ownership-claimed` next action.
3. Change the Customizing advice for hand-deleted files to `open-forge remove <path>`. Confirm with a scratch run that `extension list` and `doctor` then stay quiet.
4. Tick the backlog item in the beta follow-ups and remove 33 from the "After beta" list.

## Pros And Cons

| Pros                                                                     | Cons                                                                                                                                     |
| ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------- |
| Removes an open item that misdirects readers toward work already shipped | Closing without the residual fixes leaves a contract that contradicts the CLI                                                            |
| Keeps the queue honest before 1.0                                        | The shared-file reversal was accepted by delegation under Task 50, and the maintainer may not have reviewed it against Task 33's wording |
| Residual fixes are small and have exact locations                        |                                                                                                                                          |

## Risks And Open Questions

- The maintainer should confirm the shared-file reversal. An explicit file removal can now remove a file another package still owns. The result is consistent, since every manager honors the exclusion, but it differs from the criterion Task 33 recorded.
- A "removed on purpose" state in `extension list` and `doctor` only matters for exclusions written by hand. If the documentation steers users to `remove`, no new vocabulary is needed.

## Next Check

**Action:** Update the Task 33 record to complete, then fix the route remove interface lines in the same change.

**Would change the conclusion:** A scratch run where `open-forge remove` of an Extension-installed file is restored by `extension update`, or where `doctor` flags it afterward.

**Acceptance needed:** The maintainer.
