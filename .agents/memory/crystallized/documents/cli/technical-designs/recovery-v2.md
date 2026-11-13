---
open-forge:
  description: Propose typed recovery v2 coordinates, directory coverage, native settings snapshots and dual readers
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, Recovery, Proposal]
---

# Recovery v2
Recovery v2 is a proposed immutable bundle format that saves prior bytes and bounded directory state for tool operations.

Recovery v1 covers workspace ordinary files and relative file links. Tool operations also change account stores, remove generated trees and run bounded native settings actions. These require new coordinates and entry kinds. The proposal doesn't reinterpret or migrate v1.

Status: Proposed. Confirmation is required before writer implementation.

## Bundle

One command prepares one immutable ZIP containing `manifest.json` and exact prior-byte payloads before its first covered effect. Write and read back the bundle through source-generated types. Preview and verified no-ops create none. Use `.open-forge-state/recovery/v2/<workspace-key>/` with the existing physical-workspace identity algorithm and OS lease.

The workspace lock keeps bundle identity, location, digest and state. Payloads stay in the bundle. The store discovers bundles independently if interruption prevents lock-record publication. A record alone never authorizes following an external coordinate. Both workspace settings and lock-file writes are covered effects.

## Coordinates

Proposed:

```text
RecoveryManifestV2 {
  schema: 2, workspaceKey: Sha256, operationId: string, command: string,
  attribution: {producer: "tool" | ExistingV1Producer,
    operation: ToolWriteOperation | ExistingV1Operation,
    subject: {kind: "workspace", identity: Sha256}},
  stores: [{id: Sha256, kind: "install" | "archive" | "state" | "native-cache",
    bindingHash: Sha256}], entries: RecoveryEntryV2[]
}
Coordinate = {kind: "workspace", path: RelativePath}
           | {kind: "tool-store", storeId: Sha256, path: RelativePath}
FileState = Missing | OrdinaryFile(length, sha256)
RecoveryEntryV2 = OrdinaryV1FileOrRelativeLinkEntryWithCoordinate
  | {kind: "directory-create" | "directory-delete", target: Coordinate,
     prior: Missing | Directory(sortedChildNames), intended: Missing | Directory([]),
     effectPrior: Missing | EmptyDirectory}
  | {kind: "native-settings", adapter: AdapterId, scope: Coordinate,
     targets: NativeFileSnapshot[], directories: NativeDirectorySnapshot[],
     intendedSettings: map<Scalar>, unchangedSettings: map<Scalar>,
     allowedOutputs: RelativePath[]}
NativeFileSnapshot {target: Coordinate, prior: FileState, priorPayload: string | null}
NativeDirectorySnapshot {target: Coordinate, prior: Missing | Directory(sortedChildNames)}
```

Ordinary entries keep exact v1 prior and intended ordinary-file or relative-link identities and valid state pairs. Only their target coordinate changes. Payloads hold exact prior ordinary-file bytes, never link targets. Paths are contained relative paths without absolute targets, parent segments or aliases. `.` is allowed only for a receipt-bound tool unit's directory entry. It never names a home directory or global store parent.

Each store ID resolves through a verified local binding to one bounded unit. The binding hash covers kind and resolved physical root. Bundle coordinates can't create or widen bindings. Before an external effect, revalidate current binding, physical root identity and link containment. Missing or changed bindings block. The manifest contains no executable restoration commands.

## Attribution

Tool attribution uses producer `tool`, operation `install`, `configure`, `update`, `remove` or `choose`, and the selected workspace key. Root path Remove involving tool content uses producer `workspace`, operation `remove` and the same subject. Existing valid v1 tuples can occur only with their unchanged combinations. Command text, filenames, payloads and invalid receipts never give attribution.

## Directories

Expand a covered runtime tree into an exact ordered set of ordinary-file and directory entries. Record every directory's prior direct child names, including empty directories. Save every existing file's exact prior bytes. Prior absence has no payload. Reject links, special files, unknown output, duplicate leaves and overlapping independent claims. Parent and child entries within the same covered tree are required.

There is no recursive delete effect. Initial checks compare complete prior membership. Directory creation requires absence and produces an empty directory. Directory deletion requires an empty directory after covered children are removed and produces absence. Saved prior membership is recovery evidence, not the immediate delete predicate. Delete files first, then directories from children to parents. A new child stops deletion. Failure keeps verified effects and the bundle. A tree receipt never permits deletion of arbitrary authored files.

## Native settings

Before a native SQLite settings action, stop affected clients and verify stable files. The pinned adapter declares the exact write set, including settings databases, SQLite journals and sidecars, UI JSON, coordination files and possible logs. Include prior absence for new files and directories. Snapshot and verify every existing target before launching the native client. Every allowed output is a snapshotted target inside one native-cache scope. An adapter without a bounded tested write set can't run.

A native-settings entry saves prior bytes plus typed intended and complete unchanged settings maps. The verification predicate is logical settings equality because future SQLite bytes aren't predictable. The immutable bundle verifies before the action. No discovered output is added afterward. Each file belongs to one effect group. Coalesce member changes before planning. Native-group files can't also receive an ordinary-file effect.

Run only the reviewed finite settings action with declared environment and process consent. Prevent indexing and watching existing projects before off settings apply. Wait for final-client shutdown, then verify requested settings, unchanged settings and the declared write set. Report every touched path, including partial failure. All predicates must pass for verification. Failure stops new effects and reports saved prior bytes. There is no automatic restore or replay.

## Readers

The tools release adds explicit v1 and v2 readers to Status, Doctor and Cleanup. Keep workspace v1, workspace v2 and existing legacy stores separate. Unavailable coverage isn't an empty store. Unknown schemas stay unsupported and untouched. V1 bundle bytes and interpretation stay unchanged.

Status and Doctor validate bounded metadata and prior-byte hashes without extracting, disclosing or following target coordinates. They infer no running activity. Cleanup deletes only a recognized bundle or draft in the selected workspace recovery store under existing guarded-deletion rules. It doesn't restore entries or delete external targets. A valid bundle can be cleaned after its external unit disappears.

Older CLIs don't discover the v2 store. A v1-only reader given a v2 manifest treats it as unsupported and can't report v2 cleaned. Finish tool removal and recovery with the new CLI before a downgrade. Unknown-property preservation doesn't promise that an old writer keeps receipt kinds it doesn't understand.

## Conformance

Required cases are:

- V1 unchanged and v2 round trips with source-generated readers.
- Unknown schema and invalid attribution rejection.
- Prior ordinary bytes, relative links, absence and empty directories.
- Changed or missing bindings without target traversal.
- Directory membership drift and a new child during deletion.
- Native settings snapshots before any action, including SQLite sidecars.
- Unexpected outputs, native failure and final-child shutdown.
- Interrupted publication with independent bundle discovery.
- Cleanup without following or deleting external targets.

## Not decided yet

- Confirm this format and its simultaneous v1/v2 reader rollout before implementation.
- Define the bundle digest and lock-record publication rule together. If the next lock bytes contain the current bundle's digest, and that bundle records the intended lock hash, each hash depends on the other. The proposed format needs a rule that avoids this self-reference before writer implementation. Independent recovery-store discovery stays required.

The [mutation and recovery design](mutation-and-recovery.md) defines the existing v1 mechanics and parent safety rules.
