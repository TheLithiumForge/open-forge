---
open-forge:
  description: Define config changes, drift, backups and record publication
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool record behavior

Tool record behavior is the set of rules that keeps tool configuration reversible while it stays unchanged.
For example, Remove restores a JSON member's prior value only if the member still equals the value Open Forge applied.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Change kinds

Each profile declares an ordered list of changes with stable IDs and scopes.
Only these four kinds are supported:

| Kind                  | Selector                                         | Desired result           |
| --------------------- | ------------------------------------------------ | ------------------------ |
| JSON value            | File and JSON Pointer                            | Set a value or delete it |
| Marked block          | File and unique start and end markers            | Exact region content     |
| Managed file          | File and template resource                       | Exact file bytes         |
| Native config command | Allow-listed argument vector and read-back probe | Verified native value    |

A JSON array union is a set operation on the one selected value.
Keep existing elements in order and append missing desired elements once.
Record the original array and resulting array.
There are no text diffs, merge-patches or regex key edits.
A malformed or duplicate marked region conflicts.
A pre-existing unrelated managed-file destination conflicts.
A native change needs a bounded read-back probe and a declared restore action before it can be reversible.
An unavailable prior value blocks that change.

## Reapply

Compare each current value with desired, then with the last applied value:

| Current state       | Action                           |
| ------------------- | -------------------------------- |
| Equals desired      | No change                        |
| Equals last applied | Migrate to desired               |
| Anything else       | Show a conflict and keep current |

On a first apply, use the declared precondition.
Preserve unrelated members and surrounding bytes.
An initial union reads the array before computing the desired value.
On later applies, compare against the recorded applied array before extending it.
When a profile drops a change, use guarded removal before its replacement.

## Removal

Restore prior presence and value only while current equals applied.
Borrowed values stay in place.
Changed values stay in place and keep their history as unfinished removal.
Without trustworthy local history, don't invent prior values.
Open Forge-added config can be restored. Upstream behavior remains controlled by the tool.
While active, the native CodeGraph setup outputs keep their hooks and permissions.
On Remove, restore only the unchanged setup additions recorded for this project.
Whether to run a local native uninstall as well is a review question.

## Backups

For a tracked file whose current bytes match Git's saved bytes, Git provides the backup.
For an untracked file or a file with uncommitted bytes, prepare a sibling `.bak` before changing it.
Git can't recover uncommitted bytes.
An occupied or unreadable backup destination blocks replacement.
Keep backups until verification completes.
After successful verification, discard only this operation's unchanged backup.
On failure, offer guarded restoration through Repair. Never overwrite concurrent edits.
This uses the existing recovery-v1 boundaries for other operations and adds no recovery-v2 format.

## Publication

Before effects, save local edit preimages and pending steps.
After each effect, record its actual outcome so interruption doesn't manufacture a completed setup.
Verify config and direct-command readiness before publishing the portable tool baseline.
Write settings and lock with the existing writers under the workspace lease.
A failed second write is reported as incomplete records. Reapply reconciles from actual facts and saved local history.
Machine records stay outside the workspace.
They don't grant project write permission or prove another project's compatibility.

Readers accept unknown keys and report unrecognized schema versions while using understood keys.
Invalid fields they act on block the dependent effect.
The [Interface](interface.md) defines locations and shapes.

## Conformance

Required evidence covers:

- Released v5 and v1 unchanged, plus the new v6 and v2 additions.
- Lower, higher and absent schema versions and unknown fields.
- Prior absence, JSON null, borrowed values and array union.
- No-op, migration, drift and guarded removal for all four kinds.
- Tracked unchanged, tracked dirty, untracked and missing files.
- Interrupted local and portable writes, unavailable history and concurrent edits.
- No machine paths, PATH history or preimages in committed records.
