---
open-forge:
  description: Accepted public contract for excluding files or deregistering outside folders without deleting content
  responsibility: Define what route exclude accepts, changes, reports and preserves
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Exclude, Mutation, CurrentTruth]
---

# route exclude Interface Contract

## Workspace Layout

Exclude removes a row from the selected loader. The native Skills catalogue follows primary-category rules with a custom root.

See the [shared layout contract](../../../shared-operation-contract.md#workspace-layout). Path and transcript examples using `.agents` show the default layout.

## Status And Authority

This contract implements the maintainer's frozen route include decision.
The [shared result coordinates](../../shared/result-coordinates/interface.md)
define the envelope, statuses, exits and streams. The [CLI Architecture](../../../architecture.md)
defines layering, parsing and serialization. The [mutation and recovery design](../../../technical-designs/mutation-and-recovery.md)
defines locking, expected-state checks and recovery.

## Purpose

Exclude removes every loader row for one destination and its matching registration. A file uses `roots`. A folder uses `outsideRoutes` and its catalogue row. Content stays where it is.

## Syntax

```text
open-forge route exclude <workspace-path>
  [--dry-run]
```

Shared flags keep their existing meaning. There is no prompt or confirmation flag.
Omitting `--dry-run` authorizes application after preflight.

## Operand

Accept one workspace-relative file or outside folder with either path separator and an optional
leading `./`. Report its canonical slash-separated workspace path.
The file or registered folder may be missing. An existing file target must contain UTF-8 text. Resolve a missing registered folder from its exact declaration without traversing or deleting its tree. The shared layout observation and its anchor expected-state checks still apply. Reject globs, URLs, fragments, lexical or linked workspace escapes,
Git metadata, the loader, settings and lock files as invalid input.
Keep outside registration separate from primary/native category membership and file inclusion.

Outside the primary content locations, any text file is eligible. A Markdown file with
`## Entries` is followed as a catalogue. Its own frontmatter never sets loading.
Within the main root, accept only a direct child folder's canonical catalogue. The fixed `.agents/skills/_skills.md` catalogue is also a primary category.
A deeper file error names that category's catalogue. Other files directly in the main root are invalid.

## Removal And No Change

Remove every row for the destination, including hand-written rows and rows whose
target is missing. Remove its external `roots` registration too.
For a primary-category catalogue, remove its generated row through loader membership.
When neither a row nor a registration exists, report nothing changed and succeed.

Folder exclusion removes its directory registration and matching catalogue rows without changing catalogue or descendant files. An independent file registration remains. Excluding the catalogue as a file removes its file registration and row while leaving an independent folder registration maintained but unlisted. Neither form records content removal or recreates an omitted row.

## Preview And Effects

Dry-run and apply use the same plan. The report shows loader rows before and after,
registration before and after, and every planned loader or settings change.
Only the selected layout's loader and fixed `.agents/open-forge.json` may change.
Target bytes remain unchanged. Dry-run creates no lock, recovery bundle or file.

## Results And JSON

Use the shared statuses and exit mapping. Valid previews, verified application
and verified no-ops are `completed`. Rejected input is `invalid-input`.
Unsafe or stale plans are `blocked`. Unavailable required recovery facts are
`incomplete`. Application or verification failure is `failed`.
Cancellation is `cancelled`. A verified result with a positively retained recovery
artifact or a historical recovery warning is `completed-with-warnings`.

Command data version 1 uses the shared schema-3 envelope with command `route exclude`.
The data has `mode`, `target`, `before`, `after`, `registeredBefore`,
`registeredAfter` and `verified`. Missing rows are null.
Effects, recovery and the next action stay on the shared envelope.
Folder data additionally carries `targetKind` and the canonical workspace-relative `catalogue` path. Its `target` identifies the directory and registration flags describe `outsideRoutes`. File data keeps its existing shape and `roots` flag meaning, with those extra facts absent.
A successful no-op uses the existing `nothing-to-do` headline and an empty effects list.

## Findings catalogue

| Code | Meaning |
| --- | --- |
| `route-exclude.invalid-input` | Correct the named operand or option |
| `route-exclude.workspace-unsafe` | Repair the loader, settings or catalogue boundary |
| `route-exclude.workspace-lock-unavailable` | Use the exact cause and path from the shared lock-failure wording |
| `route-exclude.target-changed` | Review a fresh preview |
| `route-exclude.recovery-unavailable` | Restore access to recovery storage |
| `route-exclude.recovery-history` | Warning. A draft, damaged or invalid bundle is retained at the reported path. It does not block unrelated work |
| `route-exclude.recovery-coverage-unavailable` | Warning. History could not be checked for the reported store. Current workspace validation still applies |
| `route-exclude.recovery-conflict` | Current-operation recovery identity or preparation conflict |
| `route-exclude.write-failed` | Review retained recovery and partial effects |
| `route-exclude.verification-failed` | Review final bytes and retained recovery |
| `route-exclude.recovery-failed` | Review the recovery artifact |
| `route-exclude.recovery-artifact-retained` | Verified changes retain a cleanup artifact |
| `route-exclude.interrupted` | Review retained effects before retrying |

## Conformance

Prove the accepted operand forms and every rejection, previews without effects,
text and source-generated JSON, no-change output, main-root membership, target
byte preservation, and the mutation and recovery boundary.
Prove all matching rows are removed, missing targets work, and unrelated rows keep their order.

## Recovery History

Drafts, damaged or invalid bundles, and unavailable recovery coverage produce warnings. Verified final bundles from earlier runs are normal retained evidence and produce no warning in writing commands. Historical artifacts never veto unrelated work. Current target, ownership, permission, plan and expected-state checks still apply. The current operation must prepare and verify its own required recovery bundle before effects. Historical paths remain separate from that preparation and remain unchanged.
