---
open-forge:
  description: Define sibling backups for Tool writes and interrupted-write restoration
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, Recovery, CurrentTruth]
---

# Tool backups

Tool backups are sibling `.bak` files that keep an existing managed file's bytes during a Tool write outside Git.

For example, without Git, before changing `.gitignore`, copy it to `.gitignore.bak`. Write the new content and validate it. Delete the backup after validation passes. On failure, restore the saved bytes. A leftover backup after interruption gives Status and Doctor a path to report and Repair a file to restore.

Status: V1 design decided.

## Write files

If the workspace is a Git repository, create no backup. This applies even when a managed file is dirty, ignored or untracked. Report Git recovery without claiming that Git saved those bytes. Otherwise, before changing an existing managed file, create its sibling backup without overwriting any existing `.bak`. Managed files include `.gitignore`, settings, lock, required Skills, conditional agent-block lines and the declared post-install entry changes. Tool-owned command effects have no Open Forge file backup. Reject links and special files at either path. Read back the backup and compare it with the expected prior bytes before changing the target.

For a group of related files, prepare every required backup before changing the first file. Keep the backups until every new file validates and the settings and lock agree. Recheck expected bytes under the existing writing leases before each effect. A changed target or an existing backup blocks a new plan.

If validation fails, stop new effects and restore changed existing files from their backups. Remove files created by this operation only when they still match its written bytes. Validate restored bytes before deleting a backup. If restoration fails, keep the backup and report the target, backup and unfinished work. Cancellation uses the same restoration path.

New files have no prior bytes to back up. Directories have no backup manifest. These backups don't promise an atomic transaction across files, account stores or processes. After interruption, a new invocation reads the current files and requires recovery of leftover backups before changing their targets.

## Read backups

Status and Doctor always check settings, lock and `.gitignore` for sibling backups, including in Git workspaces. Once valid control records are readable, also check the exact contained targets declared by stored rewrite and component definitions, required Skill receipts and Open Forge's managed agent blocks. Don't scan arbitrary paths or expand a target set from invalid records. Invalid controls keep this additional coverage unavailable. Report each leftover path without reading its contents into output or treating it as a ZIP. Reads create no files and follow no links. Unreadable paths keep their cause and unavailable coverage.

The lock records no backup identity, path, digest or state. Backup discovery works when the current settings or lock is invalid. Existing recovery v1 stores and readers keep their behavior. Cleanup doesn't delete these backups.

## Restore files

Repair can plan restoration from an ordinary sibling backup to its exact settings, lock or `.gitignore` target. It shows the target and backup before confirmation and follows the existing destination permission rules. Preview writes nothing. Under the workspace lease, recheck both paths and their bytes, restore the backup bytes, validate the target and delete the backup only after success. A link, changed file, unavailable permission or ambiguous target blocks restoration. An agent-file backup can be restored only when a validated stored definition or receipt identifies that exact declared rewrite or managed-block target. Invalid records leave discovery coverage unavailable for those paths.

Restore control records before planning repairs that depend on them. If backups are present, Repair handles that group first. Run Repair again for other workspace repairs after checking the restored files. `--automatic` selects the group. An interactive run asks for confirmation. Explicit relink requests need a separate invocation after restoration.

On the next invocation, valid restored records can identify declared entry, Skill and managed-block backup targets. Repair plans that bounded group with its destination grants and exclusions. Validate the saved document and its declared entries before writing. Recheck target and backup bytes under the lease. Restore the complete saved file only with the reviewed plan, then verify it before deleting the backup. Unavailable or ambiguous declaration data blocks that target. No backup location or payload is recorded in the lock.

Validate saved settings and lock bytes before any write. The resulting Tool pins must agree. Restoring `.gitignore` requires a grant in the current settings. Saved settings don't grant that permission. Recheck current control records under the lease so a changed grant or exclusion blocks the write. A plain backup filename doesn't identify the command that created it.

If restored bytes verify but a backup is still present, report a warning. If backup removal can't be checked, report failure. These results use Tool backup finding codes separately from recovery v1.

Tool backups don't select account-store targets. Existing recovery v1 restoration keeps its format, selection and behavior.

## Conformance

Required cases are:

- Existing bytes copied before any change and restored after failed validation.
- Related settings and lock backups kept until both records validate.
- Successful writes delete their backups.
- Existing backups block changes without being overwritten.
- Interruption leaves readable backups discoverable with invalid current records.
- Status and Doctor report paths without disclosing saved bytes.
- Repair preview, expected-byte revalidation and exact restoration.
- Links, unavailable paths and failed restoration keep their causes.
- Existing recovery v1 tests stay unchanged.

The [mutation and recovery design](mutation-and-recovery.md) defines the existing v1 behavior.
