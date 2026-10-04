---
open-forge:
  description: Open Task 63 to decide whether and how a direct edit to a managed file can survive updates, since removedFiles already keeps such a file untouched as an undocumented side effect
  tags: [Memory, Working, Task, CLI, Update, Removal, Customization, Investigation, Contextual, Active]
---

# Task 63 — Keeping edits to managed files through updates

**Reviewed on 2026-09-28:** [review](../../../archived/cli-development/analysis/open-task-review/task63-keeping-edits-through-updates.md). Recommendation:
Needs the maintainer's decision before 1.0. The review names any details in this record that are out of date.

## Outcome

Recorded at the maintainer's request on 2026-09-27, from a finding in
[Task 61](task61-documentation-accuracy-and-voice.md). The maintainer is unsure
whether this should be supported, so this Task decides first.

**What happens today:**

- An ordinary update replaces an edited managed file with the current version.
  The old bytes survive only in the recovery bundle. See the
  [update behavior contract](../../../../memory/crystallized/documents/cli/contracts/update/behavior.md).
- The documented way to keep a local change is an overwrite companion,
  `{name}.overwrite.md`, which updates never touch.
- Listing the file's path under `removedFiles` in `.agents/open-forge.json` also
  keeps it untouched. `removedFiles` excludes the exact destination from update
  planning, so an edited file that is still present is simply skipped. Nothing
  documents this as a way to keep an edit, and the name says the file was
  removed when it wasn't.

## Questions

1. **Should keeping a direct edit be supported at all?** Overwrite companions
   may be enough. Supporting direct edits invites drift from the shipped
   version, which updates exist to prevent.
2. **If yes, how is the intent expressed?** Options include documenting
   `removedFiles` for this use, adding a separate list such as a kept or pinned
   file list in `.agents/open-forge.json`, or a command that records it.
3. **What does an update report for a kept file?** For example, that a newer
   shipped version exists and how to compare it, without replacing the file.
4. **How do removal and restoration interact?** Removing the entry must allow a
   later update to restore the shipped version, as it does for `removedFiles`
   today.

## Related work

- The accepted [Workspace State Files decision](../../../crystallized/decisions/framework/workspace-state-files.md)
  deliberately made the recovery bundle and `git diff` the way to recover an
  edit that an update replaced. Supporting kept edits would reopen that
  decision, so this Task should start from its reasoning.
- The `removedFiles` behavior itself is specified in the update contracts and
  tested. Only its use for keeping an edit is undocumented.
- [Task 33](../../../archived/cli-development/tasks/task33-managed-content-removal.md) decides whether individual managed
  content can be removed.
- [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md) explores one removal and
  suppression model across Routes, Extensions, and Libraries. A kept-file list
  is a form of suppression and should agree with it.
- [Task 36](task36-extension-merge-and-guards.md) covers partial merging, another
  way local and shipped content could coexist in one file.

## Done when

- [ ] A recorded decision: supported or not, and if supported, the setting,
      the update report, and the restoration path.
- [ ] If supported, the update contract, `docs/cli.md`, and the site's
      Customizing page describe it, and tests pin the behavior.
- [ ] If not supported, the documentation keeps recommending overwrite
      companions, and the `removedFiles` side effect is either documented as
      unsupported or closed.

## Current State

**Now:** recorded, not started.
