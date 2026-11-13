---
open-forge:
  description: Define sibling backups for Tool writes and interrupted-write restoration
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, Recovery, CurrentTruth]
---

# Tool backups
Tool backups are sibling `.bak` files that keep an existing file's bytes while a Tool writer changes it.

For example, before changing `.mcp.json`, copy it to `.mcp.json.bak`. Write the new content and validate it. Delete the backup after validation passes. On failure, restore the saved bytes. A leftover backup after interruption gives Status and Doctor a path to report and Repair a file to restore.

Status: V1 design decided.

## Write files

Before changing an existing file, create its sibling backup without overwriting any existing `.bak`. This applies to host files, `.gitignore`, settings, lock and other existing files in the declared write set. Reject links and special files at either path. Read back the backup and compare it with the expected prior bytes before changing the target.

For a group of related files, prepare every required backup before changing the first file. Keep the backups until every new file validates and the settings and lock agree. Recheck expected bytes under the existing writing leases before each effect. A changed target or an existing backup blocks a new plan.

If validation fails, stop new effects and restore changed existing files from their backups. Remove files created by this operation only when they still match its written bytes. Validate restored bytes before deleting a backup. If restoration fails, keep the backup and report the target, backup and unfinished work. Cancellation uses the same restoration path.

New files have no prior bytes to back up. Directories have no backup manifest. These backups don't promise an atomic transaction across files, account stores or processes. After interruption, a new invocation reads the current files and requires recovery of leftover backups before changing their targets.

## Read backups

Status and Doctor check the exact Tool control paths and receipt-declared file paths for sibling backups. They report each leftover path without reading its contents into output or treating it as a ZIP. Reads create no files and follow no links. Unreadable paths keep their cause and unavailable coverage.

The lock records no backup identity, path, digest or state. Backup discovery works when the current settings or lock is invalid. Existing recovery v1 stores and readers keep their behavior. Cleanup doesn't delete these backups.

## Restore files

Repair can plan restoration from an ordinary sibling backup to its exact target. It shows the target and backup before confirmation and follows the existing destination permission rules. Preview writes nothing. Under the workspace lease, recheck both paths and their bytes, restore the backup bytes, validate the target and delete the backup only after success. A link, changed file, unavailable permission or ambiguous target blocks restoration.

Repair doesn't infer an external store binding from a backup. Account-store restoration requires an independently verified local binding. Existing recovery v1 restoration keeps its format, selection and behavior.

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
