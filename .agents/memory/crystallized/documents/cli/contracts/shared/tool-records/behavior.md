---
open-forge:
  description: Define additive changes and removal without repository preimages
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool record behavior

Tool record behavior is the set of rules that records additions and removes only those additions.
For example, Remove deletes an archive exclusion only when it appears in the lock's added-item list and is still present.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Apply

Each profile declares stable change IDs in execution order.
Use these five additive cases:

| Case             | Apply                                                                                | Remove                                                 |
| ---------------- | ------------------------------------------------------------------------------------ | ------------------------------------------------------ |
| JSON array union | Append missing items and record only those items                                     | Delete only recorded items still present               |
| JSON value set   | Set only an absent member, otherwise report a conflict                               | Delete only while equal to the recorded value          |
| Marked block     | Add one uniquely marked block                                                        | Delete that unambiguous block                          |
| Managed file     | Create only if absent                                                                | Delete only while its hash matches                     |
| Native command   | Run an allow-listed command and record the previous account setting in machine state | Restore that setting only while current equals applied |

Keep unrelated JSON members, array items and bytes outside a marked block.
JSON null is a present value and conflicts with an initial set.
An existing matching value without a receipt is still a conflict for a set or managed file.
A non-array union destination conflicts.
Malformed or duplicated markers conflict.
There are no JSON delete recipes, text diffs, merge-patches or regex key edits.

## Reapply

With a matching receipt and matching current result, return no change.
For a union, add newly required missing items and extend its receipt without claiming pre-existing items.
For a set, managed file or block migration, change only a matching recorded value or hash.
A new set still requires absence.
Anything else is drift and stays unchanged.
When a profile drops an addition, use its removal rule.
Don't restore an earlier repository value.

## Native settings

Read the stored native setting, keeping an inherited environment override separate.
For CodeGraph telemetry, read the tool's status with its telemetry override variables removed for that probe only.
Normal tool invocations keep their environment.
Record the stored previous on/off value in machine state.
Disclose that telemetry off deletes the upstream identity and unsent queues.
Restoration covers the setting, not deleted telemetry history.
If previous state is unavailable, block before effects.

## Backups

Git covers a tracked file whose current bytes match HEAD.
Other existing bytes get a sibling `.bak` before a write.
Git can't recover uncommitted bytes.
An occupied backup destination blocks replacement.
Keep one original backup across sequential edits to the same file.
After successful verification, discard only this operation's unchanged backup.
On failure, keep it and print guarded manual restoration instructions.
Tools don't add a Repair action or depend on the existing workspace lock or recovery mechanism.

## Remove

Run the upstream uninstall first.
Then remove only additions recorded for this tool.
Keep changed JSON sets and managed files and report conflicts.
Delete a uniquely marked block as the managed span, including its interior.
Leave unrelated data and every unrecorded value alone.
Keep failed or conflicting receipts until removal completes.
Package-manager uninstall stays a printed command.
Shared tool bytes and runtimes require live confirmation.
Unattended runs print instructions and keep them.

## Publication

After each verified addition, save its actual receipt in the lock and any native setting facts in machine state.
A later failure keeps completed receipts and reports unfinished work.
An interruption between an effect and its receipt doesn't authorize guessing or deleting that unrecorded value.
Report that gap for manual checking.
Don't add a Tool journal, lease or recovery folder.
Commands don't run concurrently by design.

Readers keep unknown keys and report unrecognized schema versions while using understood keys.
Validate fields before using them and keep unrelated settings and ownership data.
The [Interface](interface.md) defines shapes and locations.

## Conformance

Required tests cover:

- Fresh derivation from released settings v5 and lock v1.
- Exact team restore and latest fresh selection.
- Added-item accounting, absent-only sets, marker uniqueness and file hashes.
- Reapply, migration and removal without repository previous values.
- Native previous values only in per-user state.
- Dirty tracked files, untracked files and sequential sibling backups.
- Partial completion and unchanged unrelated bytes.
- No Tool writes or locks under `.open-forge-state/`.
