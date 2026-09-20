---
open-forge:
  description: "Historical CLI-v2 source: Exact shallow status facts, typed data, semantic outcomes, advisory suggestions, and exhaustive share-safe redaction behavior"
  responsibility: Define status orientation and exposure without expanding it into diagnosis, mutation, logging, or an executable recommendation engine
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Status Result Contract

## Scope

`status` returns shallow deterministic orientation for one exact workspace. It
reports mechanically knowable Framework anchors, managed lifecycle summaries,
Git readiness facts, visible recovery evidence, and optional advisory
operations. It never mutates, performs complete diagnosis, builds a route
inventory, scans unrelated workspace files, or fabricates runnable input.

The [CLI Interface](../interface.md) owns the public command and `--redact`
flag. The [operation prerequisite contract](operation-prerequisites.md) owns
workspace eligibility. The [managed lifecycle contract](managed-lifecycle.md)
owns `.agents/open-forge.json` meaning and advisory checksums.

## Guarantees

### Inspection Boundary

Status inspects only:

- The exact selected workspace directory.
- Exact `AGENTS.md` and `.agents/loader.md` anchors.
- The running CLI version and embedded Framework payload fingerprint.
- `.agents/open-forge.json` and the exact managed targets it records.
- Embedded Framework targets needed to calculate shallow reconciliation
  counts.
- Relevant-path Git availability and cleanliness.
- Adjacent Gitless backups and the known replacement sidecars defined by the
  [filesystem effect contract](filesystem-effects.md#file-replacement) around
  exact inspected Open Forge targets.

It does not validate every route, link, fragment, generated region, Extension
source, formatter, or owned-file invariant. `doctor` owns complete diagnosis.

### Named Values

Production source defines each protocol inventory once through a readonly const
object and derives its type from that object.

The focused production modules for [Framework
anchors](../../../../../../src/cli/commands/status/framework-anchors.ts), the
[managed record](../../../../../../src/cli/commands/status/managed-record.ts),
[recovery artifacts](../../../../../../src/cli/commands/status/recovery-artifacts.ts),
and [relevant Git
paths](../../../../../../src/cli/commands/status/status-git-paths.ts) are
authoritative for their exact implemented values, types, interfaces, and import
paths. This contract defines their status meaning without copying those
declarations.

`StatusExposure` contains:

| Value      | Meaning                                                                |
| ---------- | ---------------------------------------------------------------------- |
| `full`     | Normal status values are present                                       |
| `redacted` | Sensitive values have passed the exhaustive status exposure projection |

`FrameworkAnchorState` contains:

| Value         | Meaning                                                                                        |
| ------------- | ---------------------------------------------------------------------------------------------- |
| `absent`      | The exact anchor does not exist                                                                |
| `recognized`  | The exact canonical Open Forge form is recognizable                                            |
| `malformed`   | An ordinary anchor exists but its required Open Forge structure is invalid                     |
| `conflicting` | An incompatible owned boundary, unsupported node kind, or unsafe identity prevents recognition |

The existing `FrameworkPresenceState` remains `uninstalled`, `installed`,
`incomplete`, or `conflicting`. It derives from the two anchor facts without
replacing them.

`ManagedRecordState` contains `absent`, `valid`, and `invalid`. Counts are
available only for a valid record. Status does not calculate trusted partial
counts from an invalid record.

`GitStatusState` contains:

| Value            | Meaning                                                                  |
| ---------------- | ------------------------------------------------------------------------ |
| `unavailable`    | Git cannot be invoked                                                    |
| `not-repository` | The selected workspace is not inside a Git worktree                      |
| `clean`          | Relevant Open Forge paths have no Git-visible changes                    |
| `dirty`          | One or more relevant Open Forge paths have Git-visible changes           |
| `failed`         | Git was available but relevant-path inspection did not complete reliably |

`clean` describes only Git-visible relevant paths. It is not a claim that every
future destructive target is tracked or recoverable; mutation preflight owns
the required per-target recovery classification.

Each diagnostic code and future status discriminant follows the same
named-value rule. Renderers never branch on raw presentation strings.

### Status Data

`StatusData` contains exactly these top-level fields:

| Field         | Meaning                                                  |
| ------------- | -------------------------------------------------------- |
| `exposure`    | One `StatusExposure` value                               |
| `workspace`   | The shared logical `WorkspaceReference`                  |
| `cli`         | Running CLI and embedded Framework identity              |
| `framework`   | Shallow canonical presence and anchor facts              |
| `managed`     | Discriminated lifecycle-record state and valid summaries |
| `git`         | Discriminated relevant-path Git state                    |
| `recovery`    | Counts of adjacent backups and known residuals           |
| `suggestions` | Ordered advisory domain operations, possibly empty       |

#### CLI Identity

`cli` contains `version` and `frameworkPayloadFingerprint`. The latter is the
SHA-256 identity of the complete embedded Framework payload. It identifies
public distributed content and remains visible in redacted output.

#### Framework Summary

`framework` contains:

| Field         | Meaning                                          |
| ------------- | ------------------------------------------------ |
| `presence`    | One `FrameworkPresenceState` value               |
| `entryState`  | Exact `AGENTS.md` `FrameworkAnchorState`         |
| `loaderState` | Exact `.agents/loader.md` `FrameworkAnchorState` |

Recognizable presence is not health, managed ownership, or mutation authority.

#### Managed Summary

`managed` is a discriminated union:

- `absent` contains only `state`.
- `invalid` contains only `state`; coded messages explain the invalid boundary.
- `valid` contains `state`, `framework`, and `extensions` summaries.

A valid Framework summary contains:

| Field                | Meaning                                                                           |
| -------------------- | --------------------------------------------------------------------------------- |
| `currentFiles`       | Current target bytes match the running payload                                    |
| `changedFiles`       | An existing recorded target differs from the running payload without claiming why |
| `missingFiles`       | A target present in both the record and running payload is absent                 |
| `newlySuppliedFiles` | The running payload supplies a target absent from the recorded installation       |
| `retiredFiles`       | The record contains a target no longer supplied by the running payload            |
| `excludedRoutes`     | Exact persisted Framework route exclusions                                        |

Framework file counts are mutually exclusive. Classify record-only targets as
retired, payload-only targets as newly supplied, absent shared targets as
missing, matching shared targets as current, and remaining shared targets as
changed. Advisory recorded checksums may enrich coded evidence, but they do
not make one target appear in more than one count.

A valid Extension summary contains:

| Field               | Meaning                                                               |
| ------------------- | --------------------------------------------------------------------- |
| `installedPackages` | Number of recorded globally unique Extension ids                      |
| `currentFiles`      | Recorded Extension files matching accepted checksums                  |
| `changedFiles`      | Existing bytes differing from accepted checksums without claiming why |
| `missingFiles`      | Recorded Extension files that are absent                              |

Status reports counts, not Extension ids, dependencies, target paths, source
locations, or file checksums. `extension list` and `extension inspect` own
package-level detail.

#### Git And Recovery Summaries

`git` is a discriminated union keyed by `state`. `dirty` additionally contains
`relevantChangeCount`. Other variants contain no fabricated count, repository
root, branch, remote, user identity, or raw Git output.

Git unavailability, a non-repository workspace, and ordinary dirty state are
orientation facts. They do not independently make read-only status
unsuccessful.

`recovery` contains `backupCount` and `residualCount`. `backupCount` counts each
recognized user-owned Gitless `.bak` around an exact inspected target.
`residualCount` counts each recognized prepared-replacement or
preserved-previous sidecar around those targets, so both sidecars beside one
target contribute two. A `.bak` never contributes to `residualCount`, and an
unknown adjacent filename contributes to neither count. Status exposes counts
only. `doctor` owns exact findings, paths, causes, relationships, and safe
repairability.

#### Suggestions

`suggestions` is always present and may be empty. Each `StatusSuggestion`
contains one typed domain `operation` and one human-readable `reason`.

A suggestion contains no executable, shell string, argv, workspace argument,
inferred subject, authorization, or promise that it is the unique next action.
Status performs deterministic work inside its own read-only contract and
suggests another operation only across a distinct purpose or authority
boundary.

The initial deterministic mapping is:

| Facts                                                                                | Suggestions                                         |
| ------------------------------------------------------------------------------------ | --------------------------------------------------- |
| Uninstalled Framework                                                                | `install`                                           |
| Incomplete or conflicting Framework, invalid lifecycle record, backups, or residuals | `doctor`; lower-priority suggestions are suppressed |
| Missing, newly supplied, or retired Framework files under a valid record             | `install`                                           |
| Missing Extension files under a valid record                                         | `extension.list`                                    |
| No applicable condition                                                              | none                                                |

Changed managed bytes and dirty Git do not produce a suggestion by themselves.
Suggestions follow table order and contain no duplicates.

### Semantic Status

| Facts                                                                          | Result status |
| ------------------------------------------------------------------------------ | ------------- |
| Selected root is missing or is not an ordinary directory                       | `blocked`     |
| Workspace is uninstalled and inspection completed                              | `success`     |
| Installed state is recognizable and no attention condition exists              | `success`     |
| Managed bytes changed or relevant Git paths are dirty, with no other condition | `success`     |
| Framework is incomplete or conflicting                                         | `attention`   |
| Managed record is invalid                                                      | `attention`   |
| Managed files are missing, newly supplied, or retired                          | `attention`   |
| Backup or residual evidence is present                                         | `attention`   |
| Anchor, lifecycle, managed-target, or Git inspection cannot complete reliably  | `failed`      |
| Caller cancellation is caught before completion                                | `cancelled`   |

`invalid` remains a parser or request result and does not arise from a valid
status handler invocation.

### Redacted Presentation

`--redact` is a status-local presentation flag with no alias. It composes with
human or JSON presentation:

```text
open-forge status --redact
open-forge status --json --redact
```

The handler performs the same inspection once and returns the same semantic
outcome. Before selecting the terminal or JSON renderer, a status-local
exposure projector transforms the complete result from `full` to `redacted`.
It does not rerun inspection or change message codes, suggestions, status, or
process completion.

Redacted output preserves:

- Schema version, operation id, semantic status, message codes, and levels.
- Named Framework, lifecycle, Git, and exposure states.
- Counts and booleans.
- CLI version and public embedded Framework payload fingerprint.
- Workspace selection source.
- Advisory operation ids and sanitized reasons.

Redacted output replaces the logical workspace root with the named
`<workspace>` placeholder. It exposes no absolute path, home directory,
username, machine name, repository location, workspace-specific identity, raw
operating-system error, raw Git output, or unstructured exception text.
Persisted route exclusions retain their count and order but become
`<route-1>`, `<route-2>`, and so on. Placeholder templates are named protocol
values in production source rather than scattered strings.

Redaction is structural, not a best-effort string scrub. Inspection failures
become typed internal facts. The projector exhaustively maps every public field
and reconstructs share-safe messages from codes and safe values. A newly added
status field cannot enter redacted output until the projection and its tests
handle it.

The full result never enters logs or another output channel after redaction is
selected. Unexpected failures use one generic share-safe message instead of
serializing the caught error.

### Examples

Normal structured status includes the actual selected root and declares full
exposure:

```json
{
  "exposure": "full",
  "workspace": {
    "root": "D:\\Work Projects\\example",
    "selectedBy": "workspace-flag"
  }
}
```

The corresponding redacted projection keeps the same shape:

```json
{
  "exposure": "redacted",
  "workspace": {
    "root": "<workspace>",
    "selectedBy": "workspace-flag"
  }
}
```

Terminal guidance is explicitly advisory:

```text
Suggested operation: doctor
Reason: Inspect the incomplete Framework installation.
```

It is never labeled `Next` or presented as a complete runnable invocation.

## Boundaries

`status` provides shallow deterministic orientation only. It never mutates,
performs complete diagnosis, builds a route or reference inventory, scans
unrelated workspace files, exposes private recovery material, or manufactures
an executable next command. Redaction changes exposure only and cannot change
inspection, status, messages, suggestions, or process completion.

## Verification

Direct status tests prove every named state, discriminated branch, count
meaning, semantic-status row, suggestion row, and no-suggestion case.
Integration tests use real workspace, file, and Git state for anchor,
lifecycle, dirty-path, backup, and residual facts.

Redaction tests use canary paths, usernames, workspace identifiers, raw Git
output, and operating-system errors. They assert that no canary survives in
human or JSON output while every required identifier, state, count, payload
fingerprint, suggestion, status, and exit remains unchanged. A built-process
snapshot proves `--json --redact` emits one share-safe document on stdout and
nothing on stderr.

## Related Current Sources

- [CLI Interface](../interface.md)
- [CLI Architecture](../architecture.md)
- [Parser Result Contract](parser-results.md)
- [Operation prerequisites](operation-prerequisites.md)
- [Managed lifecycle](managed-lifecycle.md)
- [Filesystem effects](filesystem-effects.md)
- [Workspace recovery](workspace-recovery.md)
- [Result and display Pattern](../../../../../patterns/open-forge/cli/commands/result-display-boundary.md)
