---
open-forge:
  description: Review of Task 63 Keeping Edits Through Updates, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 63 Keeping Edits Through Updates Review

## Question

Is Task 63 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 63](../../../working/cli-development/tasks/task63-keeping-edits-through-updates.md)

## Current Conclusion

**Recommendation:** Needs a maintainer decision first.

**Size:** Small if the answer is to document existing behavior. Medium if it adds a setting and an update report.

Decide it before 1.0, because renaming or adding a settings key after 1.0 needs a migration. My lean is to add no new mechanism. The accepted [Workspace State Files decision](../../../crystallized/decisions/framework/workspace-state-files.md) already chose where edit protection lives: in the recovery bundle and `git diff`. It removed the `KeepAsUnmanaged` action for that reason. Task 63 would reopen that choice. The cheaper honest answer is to document "replace" as the supported way to take over a file, since the contracts already leave an excluded existing file untouched.

## What It Implies

If supported, a user could edit a shipped file directly and have updates leave it alone, with the update reporting that a newer version exists. That creates a second customization path beside overwrite companions and invites drift from shipped files. If not supported, the documentation explains overwrite companions for adjustments and remove-then-recreate for replacements. The [Extensions Architecture](../../../crystallized/documents/extensions/architecture.md) wording then needs a correction either way.

## State Today

Verified at `815324f9`:

- Ordinary update replaces edited owned content after recovery preparation ([update interface](../../../crystallized/documents/cli/contracts/update/interface.md)). The Customizing page and the flows page say so after Task 61.
- **The record overstates "undocumented side effect".** Leaving an excluded existing file untouched is contracted and tested. The [remove interface](../../../crystallized/documents/cli/contracts/remove/interface.md) says "Existing excluded files remain untouched". `docs/cli.md` says the same, and "Update preserves excluded existing whole-file and generated-region bytes and ownership facts" pins it. Only its use for keeping an edit is undocumented.
- The `docs/cli.md` example already lists `"AGENTS.md"` under `removedFiles`. That is how a user keeps an edited Open Forge block today, since an overwrite companion cannot adjust it.
- Overwrite companions also do not help the two native Skill files, `open-forge-cli/SKILL.md` and `use-workflow/SKILL.md`, because harnesses do not load a `SKILL.overwrite.md`.
- A manually excluded existing file keeps its ownership claim. `extension remove` still deletes such a file as its final owner, because `ExtensionRemoveTopologyBuilder` skips only excluded generated regions. Whether `index` rewrites the `Entries` block of an excluded entrypoint is unverified. It does not consult exclusions.
- Invariant 11 of the Extensions Architecture says removal "protects user changes", and its lifecycle section says managed operations protect user changes. The accepted behavior protects them only through the recovery bundle. Task 61 recorded this contradiction.

## Dependencies

- **Blocked by:** nothing. The exclusion model it must agree with shipped in [Task 50](../../../archived/cli-development/tasks/task50-unified-remove.md).
- **Blocks:** the final wording of Extensions Architecture invariant 11, unless that is corrected to current behavior now.
- **Overlaps with:** [Task 33](task33-managed-content-removal.md) and [Task 35](task35-removal-and-suppression-model.md) (the same settings file), and [Task 36](task36-extension-merge-and-guards.md) (keeping an edited `AGENTS.md` block).

## Remaining Work

1. The maintainer chooses one option:
   - **Not supported.** Recommend overwrite companions for adjustments. Document replacement as `open-forge remove <file>` followed by writing your own file, and state that an excluded existing file is left alone.
   - **A named keep list,** such as `keptFiles`. Update skips the file, keeps the claim, and reports that a newer shipped version exists.
   - **Take over a file.** One command releases the claim, records the exclusion, and keeps the bytes, so the file becomes user-owned.
2. Decide whether the misleading name `removedFiles` stays for exact exclusions of present files, or gains an alias before 1.0.
3. Correct Extensions Architecture invariant 11 and its lifecycle paragraph to say that edited bytes are kept in a recovery bundle before replacement or deletion.
4. If supported, update the update and remove contracts, `docs/cli.md`, and the Customizing page, and add tests for skip, report, and restoration.
5. Record the choice as an amendment to Workspace State Files, since it revisits `KeepAsUnmanaged`.

## Pros And Cons

| Pros                                                                             | Cons                                                              |
| -------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Covers files overwrite companions cannot reach: native Skills and the host block | Reopens an accepted decision made to simplify ownership           |
| Matches "Every installed file is yours" on the Customizing page                  | A second customization path makes drift from shipped files easier |
| Documenting the existing behavior costs almost nothing                           | A new list duplicates `removedFiles` behavior under another name  |
| Settles settings naming before 1.0 fixes it                                      | An update report for kept files adds output to every update       |

## Risks And Open Questions

- Does "Every part remains replaceable by the user" in the Vision require an in-place keep, or is remove-then-recreate enough?
- A kept file that keeps its claim is still deleted by `extension remove`. Users may not expect that.
- The maintainer is unsure the feature is wanted. Real user requests would be the best evidence.

## Next Check

**Action:** Put the three options and the Workspace State Files precedent to the maintainer, and correct invariant 11 to current behavior in the meantime.

**Would change the conclusion:** Beta users asking to keep edits to native Skills or `AGENTS.md`, which would favor the take-over option.

**Acceptance needed:** The maintainer.
