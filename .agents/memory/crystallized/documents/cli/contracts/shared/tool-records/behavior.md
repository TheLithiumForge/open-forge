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

| Case             | Apply                                                                                | Remove                                                       |
| ---------------- | ------------------------------------------------------------------------------------ | ------------------------------------------------------------ |
| JSON array union | Append missing items and record only those items                                     | Delete only recorded items still present                     |
| JSON value set   | Set only an absent member, otherwise report a conflict                               | Delete only while equal to the recorded value                |
| Marked block     | Add one uniquely marked block                                                        | Delete that unambiguous block                                |
| Managed file     | Create only if absent                                                                | Delete only while its hash matches                           |
| Native command   | Run an allow-listed command and record the previous account setting in machine state | Keep the account setting and print its manual change command |

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
When a profile drops an addition, plan its guarded removal before the new profile's changes.
Use the prior receipt order for removals and the new recipe order for additions.
B returns removal observations with current and desired values, disposition and conflicts.
Remove takes that read-only plan and revalidates each observation before effects.
Don't restore an earlier repository value.

## Native settings

Read the native setting through its authored source, keeping inherited environment overrides separate.
Prefer `StoredValue` when authored.
Otherwise use declared `ReadArguments`, `ReadEnvironment` and `ValueParserId` only through `InstalledInvocation`.
That read target comes from verified installed detection, not a future planned write invocation.
A command read also requires authored positive `ReadTimeout` and `ReadMaximumOutputBytes`.
An absent read environment means an empty overlay.
If neither source is available, block preflight.
An authored stored-reader failure blocks without falling back to a command.
Keep the read declarations optional so native-command changes can use either supported source.
For CodeGraph telemetry, read `${nativeHome}/.codegraph/telemetry.json` through the declared stored-value parser.
An absent file or an object without `enabled` uses the documented on default.
For a present Boolean setting, validate only JSON token kinds required by the native format.
On requires a string `machine_id`. Off permits a string or null `machine_id`.
Never extract, compare, log or return identity text.
Unreadable files, invalid JSON, non-object roots, a wrong setting type or rejected member kinds block before effects.
CodeGraph V1 uses this stored reader before installation and for later verification.
This read needs no installed executable and doesn't invoke native status.
Keep only presence and the on/off setting in the observation.
Don't probe identity values or queues.
Normal tool invocations keep their environment.
Record the stored previous on/off value in machine state.
Write through the resolved native `telemetry off` command and verify the stored setting again.
Keep the original previous value on reapply.
Disclose that telemetry off deletes the upstream identity and unsent queues.
Project Remove keeps the account setting because other projects can rely on it.
Report the kept setting and print the native command to change it back.
Changing the setting can't restore deleted telemetry history.
Add no cross-project native-setting registry.
If previous state is unavailable, block before effects.

### Command reads

The shared `CodeGraphTelemetryStatusParserId` is `codegraph-telemetry-status-v1`.
Its declared arguments are `["--no-color", "telemetry", "status"]`.
Its read overlay removes `DO_NOT_TRACK` and `CODEGRAPH_TELEMETRY` and sets `CODEGRAPH_NO_DOWNLOAD=1`.
These changes apply only to that child read.
The parser accepts exactly one complete `Telemetry: enabled (<reason>)` or `Telemetry: disabled (<reason>)` line after line-ending and ANSI handling.
Accept only `your saved choice` or `default` as the reason.
Map enabled to on and disabled to off.
The shared definitions name the parser ID, prefix, values, reasons and environment keys.
Ignore other lines without extracting identity or queue data.
An unknown parser, missing or duplicate setting line, unknown value or reason, failed process or truncated output blocks the read.
Preserve the caller's normal environment for writes.
CodeGraph V1 keeps this command-read metadata while its stored reader stays preferred.

## Backups

Git covers a tracked file whose current bytes match HEAD.
Other existing bytes get a sibling `.bak` before a write.
Git can't recover uncommitted bytes.
An occupied backup destination blocks replacement.
Keep one original backup across sequential edits to the same file.
Services return pending backup completions to D without discarding backups.
After final effect verification and record publication, D discards only this operation's unchanged backups.
Use each target's latest expected hash when completing one original backup shared across stages.
On failure, keep it and print guarded manual restoration instructions.
Tools don't add a Repair action or depend on the existing workspace lock or recovery mechanism.

## Remove

Run a confirmed native uninstall before removing project additions.
For Codebase Memory, `--yes`, no terminal or a declined account-wide confirmation skips native uninstall and prints its command.
Then remove only additions recorded for this tool.
Keep account-wide native settings and print their manual change commands.
Keep changed JSON sets and managed files and report conflicts.
Delete a uniquely marked block as the managed span, including its interior.
Leave unrelated data and every unrecorded value alone.
Keep failed or conflicting receipts until removal completes.
Package-manager uninstall stays a printed command.
Shared tool bytes and runtimes require live confirmation.
Unattended runs print instructions and keep them.

## Publication

After each verified removal or addition, save the complete remaining receipt set and native-setting facts before the next effect.
D passes the full current receipt set in each stage request, even when the stage contains one observation.
A result starts from that input set and changes only verified receipts.
Completed and removed IDs describe only this call.
A failure keeps untouched and unverified receipts and reports unfinished work.
Keep account-setting facts when dropping a project native-command receipt.
C writes with its latest file expectations and D reads fresh expectations after publication.
A record-write failure stops the operation and keeps pending backups.
Report effects that verified but couldn't be published without inventing a receipt.
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
- Kept account-wide settings and project-only removal when account uninstall is skipped.
- Dirty tracked files, untracked files and sequential sibling backups.
- Partial completion and unchanged unrelated bytes.
- No Tool writes or locks under `.open-forge-state/`.
