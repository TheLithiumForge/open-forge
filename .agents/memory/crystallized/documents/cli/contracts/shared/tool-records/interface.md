---
open-forge:
  description: Define supported ranges, exact locks and additive change records
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool record interface

Tool records are the three data sets that separate project intent, project additions and machine facts.
For example, the lock records the exclusion strings Open Forge added without storing an earlier JSON array.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Settings

Settings stay at `.agents/open-forge.json`, including with a custom context root.
Settings v6 derives freshly from released v5 and keeps existing fields and unknown keys.
The optional `tools` map has one entry per ID.
Each entry requires `version`, a supported major.minor range.
An absent or empty map declares no tools.

Proposed:

```json
{
  "schemaVersion": 6,
  "tools": {
    "codegraph": { "version": "1.6.x" },
    "codebase-memory": { "version": "0.12.x" }
  }
}
```

A declaration doesn't install bytes.
Named installation records the chosen profile's range.
It doesn't put paths, client discovery or native account settings into settings.

## Lock

The committed lock stays at `.agents/open-forge.lock.json`.
Lock v2 derives freshly from released v1 and keeps existing ownership and Library records.
Each `tools.<id>` entry requires `version`, `channel` and `changes`.
Version is the exact installed version, not a separate tested baseline.
Channel names the published channel, such as `npm` or `github-release`.
The actual npm client and runtime paths stay in machine state.
`profileDigest` is an optional SHA-256 of the applied profile.

Proposed:

```json
{
  "version": "1.6.2",
  "channel": "npm",
  "changes": [
    {
      "kind": "json-union",
      "id": "archived-memory",
      "path": "codegraph.json",
      "pointer": "/exclude",
      "items": [".agents/memory/archived/**"]
    }
  ]
}
```

Each change records only what Open Forge added:

| Kind             | Required data                                                                 |
| ---------------- | ----------------------------------------------------------------------------- |
| `json-union`     | ID, workspace-relative path, JSON Pointer and added items                     |
| `json-value`     | ID, workspace-relative path, JSON Pointer and applied value                   |
| `marked-block`   | ID, workspace-relative path, unique start and end markers and content SHA-256 |
| `managed-file`   | ID, workspace-relative path and applied file SHA-256                          |
| `native-command` | ID and allow-list ID, with previous account value in machine state            |

There are no repository preimages or stored prior values in the lock.
Union receipts omit items already present before apply.
A JSON value receipt exists only for a member that Open Forge created.
Native setup outputs stay under upstream control and aren't rewritten as Open Forge addition receipts.

Bare Install uses each exact lock version.
The [RD1 channel rule](../../tool/behavior.md#channel-choice) keeps the lock channel as a recorded fact.
A declared tool without a lock entry gets the latest version supported by its declaration.
Named Install or Update without a version gets latest support.
The optional digest detects profile changes. It isn't an executable pin.
Its fixed UTF-8 JSON projection covers profile ID, range, minimum versions, setup and uninstall vectors, environment overlays, process bounds, native inventory declarations, possible outputs, declared effects and config recipes with template hashes.
Include the authored native home policy, command-read arguments, environment, parser ID and bounds, stored-value reader declarations, missing-value defaults and effective-ignore conditions.
Hash authored token strings and resource bytes before expansion.
Use declared array order and ordinal object-key order for authored JSON values.
Exclude machine paths, observed values and timestamps.

## Machine state

Machine facts live under `~/.open-forge/state/`.
`HomePath` names the Open Forge home itself, so state is directly below it.
Reading an absent home reports missing facts without creating that folder.
Use schema 1 files `tools/<id>.json`, `runtimes/node/<version>.json`, `path.json` and `workspaces/<key>.json`.
They hold runtime and command paths, launchers, installed-client facts, integrity evidence, PATH history and native account-setting previous and applied values.
Workspace keys are SHA-256 of the normalized physical workspace path in UTF-8.
On Windows use the existing physical-path comparison form before hashing.
The physical path stays in machine state so readers can check the binding.

PATH records keep the exact chosen files and the prior and applied PATH entry or marked block.
Native-setting records keep the previous setting, resolved absolute `NativeHomePath` and allow-listed manual change arguments per workspace.
Match facts by tool ID, change ID and resolved native home using the existing physical-path comparison.
A changed home or a missing binding can't reuse an old previous value.
Read the newly resolved home before any setting write.
Keep each retained fact bound to its recorded home and identify that home in manual instructions.
This remains workspace-keyed machine state and adds no cross-project registry.
Project Remove keeps account-wide settings and reports those commands instead of running them.
A missing or unreadable native previous value blocks that change.
This doesn't create an executable-version reference registry.

## Result sets

Config apply and removal results carry the complete remaining receipt set for that tool.
Include untouched, unstarted, conflicting and intentionally kept receipts.
Completed and removed change IDs are per-call deltas.
Native-setting lists are complete for that tool and keep the original previous value through reapply.
C replaces that tool's receipt set with the returned complete set and keeps other tools unchanged.
An empty complete set removes all of that tool's project receipts.
A successful no-op keeps its existing receipts.

## Publication

Repository facts go in the committed lock.
Machine facts go only in the per-user files.
There is no workspace Tool edit state and no Tool expansion of `.open-forge-state/`.
Don't acquire the existing workspace lease or recovery bundle for Tool operations.
Write through normal expected-state checks, backups and atomic file replacement.
The lock in this checkout declares schema v2 with an empty tool map until a tool is actually installed.
The [Behavior](behavior.md) defines failure and removal.
