---
open-forge:
  description: Plan uninstall from the current ownership and payload comparison, then apply it as one verified strip plan
  responsibility: Define how uninstall selects removals, combines them with strip and leaves the workspace without Open Forge state
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Behavior, Lifecycle, CurrentTruth]
---

# uninstall Behavior Contract

The [Interface Contract](interface.md) defines what uninstall removes and keeps. The [plain-docs Behavior](../shared/plain-docs/behavior.md) defines planning and determinism for the combined plan, which [`export`](../export/behavior.md) shares.

## Select Removals

Read the layout, settings and ownership once. Absent ownership means nothing is owned, so uninstall plans no removals at all, and a second run after a complete one reports `Nothing to do.` Unreadable or unknown ownership stops the run as `uninstall.ownership-unavailable`. A recorded Library stops it as `uninstall.library-attached`.

Read every owned Framework and Extension target through the shared lifecycle target readers, rendered in the workspace's frontmatter form. A current file becomes a deletion. A current managed section becomes a rewrite of its host, or the host's deletion when nothing else remains. An edited target becomes a kept entry, unless `--remove-edited` names it. A missing target adds nothing. An unavailable or blocked target stops the run. Matching bytes never establish ownership for a file the lock does not name, with one exception: route scaffolds.

While ownership records exist, classify unowned entrypoints below the inventory roots against the route scaffold shape, and remove those that match it. Read their metadata and `Entries` through the shared parsers, never from the generated lines. Add the ownership and settings files when present, and end the plan with the runtime folder step. Once the records are gone, an entrypoint that still has the scaffold's shape stays, because matching bytes never establish ownership.

## Combine With Strip

Remove planned deletions from the plan's document set before the parts plan their edits. Plan flatten moves from the documents that stay. A folder becomes an empty-folder removal only when every file below it is deleted or moved. The main root and outside routes can go only when they empty. The flatten folder always stays.

## Apply

Applying acquires the workspace lease, revalidates the layout and forms the plan again. When the fresh plan differs from the reviewed plan in any path, action or hash, the run stops as `uninstall.target-changed` before any write. A file that changed after planning therefore stops the run.

Prepare one recovery bundle with every planned file's exact prior bytes, or its proven absence, before the first effect. Apply moves' new files first, then rewrites, then deletions, then empty folder removals. Delete content before the settings file, and the settings file before the ownership lock, so an interrupted run still records what was installed. Check each file's expected bytes right before its effect, and verify its result after. A mismatch before the first effect is `uninstall.target-changed` and nothing changes. A mismatch after effects started is `uninstall.target-changed-during-apply`, with the `doctor` next action. Stop at the first failure or interruption and report how many changes were made, `Stopped after <n> of <m> changes.`, like other writing commands. The recovery bundle stays, holding every planned file's prior bytes, and no automatic restoration runs. After every effect verifies, delete the recovery bundle. A bundle that cannot be deleted gives `uninstall.recovery-artifact-retained` and the `cleanup` next action.

`.open-forge-state/` is the plan's last entry, `delete-directory` with reason `state`, so `--dry-run` shows it and the result's effects report its outcome. The folder holds the lease's own lock file, so this step runs after every other effect verifies, the recovery bundle is deleted and the lease is released. It removes the folder only when it holds nothing but the lock file, its ignore file and empty folders, and it verifies the folder is gone. When planning finds the folder must stay, because it holds recovery data, is or holds a link, or cannot be read, the plan lists it under `kept` with that reason and reports `uninstall.runtime-retained`. When it must stay at apply time, the entry moves from the plan to `kept` with that reason, the result needs attention and `uninstall.runtime-retained` names the real reason.

## Required Evidence

Cover recognized, edited, missing and shared targets, both root hosts with and without user text, named edited files, route scaffolds with and without user text, absent and malformed ownership, attached Libraries, combined strip and flatten plans, the runtime folder with and without earlier bundles, and a second run that writes nothing, even over an edited entrypoint with the scaffold's shape. The [expected outputs](../shared/plain-docs/interface.md#expected-bytes-from-the-eval-harness) from the eval harness are compared through `export`, which shares this plan. Cover a flatten folder whose managed files are all being removed, and one holding an edited managed file that uninstall keeps, strips and moves.
