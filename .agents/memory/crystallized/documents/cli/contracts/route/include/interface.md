---
open-forge:
  description: Accepted public contract for including files and registering maintained outside folders
  responsibility: Define what route include accepts, changes, reports and preserves
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Include, Mutation, CurrentTruth]
---

# route include Interface Contract

## Workspace Layout

Include changes the selected loader. The native Skills catalogue remains a primary category even when it is outside the main root.

Including a safe outside folder registers a maintained route in `outsideRoutes` and lists its canonical catalogue. Including a file, including a catalogue file, retains the existing file-only `roots` meaning. Registration, loading, ownership and package permissions remain separate.

See the [shared layout contract](../../../shared-operation-contract.md#workspace-layout). Path and transcript examples using `.agents` show the default layout.

## Status And Authority

This contract implements the maintainer's frozen route include decision.
The [shared result coordinates](../../shared/result-coordinates/interface.md)
define the envelope, statuses, exits and streams. The [CLI Architecture](../../../architecture.md)
defines layering, parsing and serialization. The [mutation and recovery design](../../../technical-designs/mutation-and-recovery.md)
defines locking, expected-state checks and recovery.

## Purpose

Include lists an existing document where it lives or registers an existing outside folder. File inclusion never changes the target. Folder inclusion creates only its missing canonical catalogue and preserves an existing catalogue byte-for-byte.

## Syntax

```text
open-forge route include <workspace-path>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
```

Shared flags keep their existing meaning. There is no prompt or confirmation flag.
Omitting `--dry-run` authorizes application after preflight.

## Operand

Accept one workspace-relative file or outside folder with either path separator and an optional
leading `./`. Report its canonical slash-separated workspace path.
An included file must exist and contain UTF-8 text. An included folder must be an existing safe ordinary directory. Reject globs, URLs, fragments, lexical or linked workspace escapes,
Git metadata, the loader, settings and lock files as invalid input.

A maintained folder stays inside the repository and cannot overlap the workspace root, main root, `.agents`, another outside route, Git metadata or runtime storage. Apply portable lexical validation and physical checks. Registration keeps the exact outside source identities and adds no alias or short-ID mapping.

Require its canonical catalogue to be the only recognized entrypoint before registration or re-inclusion. Preserve competing entrypoints and refuse the operation until their ambiguity is resolved.

Outside the primary content locations, any text file is eligible. A Markdown file with
`## Entries` is followed as a catalogue. Its own frontmatter never sets loading.
Within the main root, accept only a direct child folder's canonical catalogue. The fixed `.agents/skills/_skills.md` catalogue is also a primary category.
A deeper file error names that category's catalogue. Other files directly in the main root are invalid.

## Metadata And Existing Rows

A new external file row requires `--description`. A new folder row uses the shared generic Route Init scaffold description when it is omitted. Repeat `--tag` in the order to keep,
with or without a leading `#`. Reject blank, invalid and duplicate tags.
Without tags the document loads on demand. Repeat `--apply-to` using the existing
applyTo expression grammar. Sort supplied atomic patterns by ordinal comparison and remove exact duplicates, as Create does.

Each omitted field keeps its current value. A supplied option replaces only its
own field. An identical request is a verified no-op. Duplicate matching rows are invalid and
point to Exclude followed by Include to choose one policy. An existing hand-written row
is registered without rewriting it. To clear tags or conditions, edit the row or
exclude and include again. Metadata options on a primary-category catalogue are invalid
and name its frontmatter as the editing location.

New rows follow the existing loader shape, use Index's percent-encoding form
and append after the last Entries row. No tags or conditions means no suffix.
The loader row controls loading. External files are registered in `roots`. Folder directories are registered only in `outsideRoutes`, in declaration order. Omitted tags and conditions preserve an existing row or use the existing empty defaults for a new row. Row options never rewrite an existing catalogue or infer policy from its frontmatter.

## Preview And Effects

Dry-run and apply use the same plan. The report shows loader rows before and after,
registration before and after, and every planned loader or settings change.
File inclusion changes only the selected loader and fixed settings file. Folder inclusion may also create its missing canonical catalogue, using the workspace metadata form and generic scaffold. Existing catalogue and descendant bytes remain unchanged. Preserve ownership, exclusions and Library protections before any catalogue create. Dry-run creates no lock, recovery bundle or file.

## Results And JSON

Use the shared statuses and exit mapping. Valid previews, verified application
and verified no-ops are `completed`. Rejected input is `invalid-input`.
Unsafe or stale plans are `blocked`. Unavailable required recovery facts are
`incomplete`. Application or verification failure is `failed`.
Cancellation is `cancelled`. A verified result with a positively retained recovery
artifact or a historical recovery warning is `completed-with-warnings`.

Command data version 1 uses the shared schema-3 envelope with command `route include`.
The data has `mode`, `target`, `before`, `after`, `registeredBefore`,
`registeredAfter` and `verified`. Missing rows are null.
Effects, recovery and the next action stay on the shared envelope.
Folder data additionally carries `targetKind` and the canonical workspace-relative `catalogue` path. Its `target` identifies the directory and registration flags describe `outsideRoutes`. File data keeps its existing shape and `roots` flag meaning, with those extra facts absent.
A successful no-op uses the existing `nothing-to-do` headline and an empty effects list.

## Findings catalogue

| Code | Meaning |
| --- | --- |
| `route-include.invalid-input` | Correct the named operand or option |
| `route-include.workspace-unsafe` | Repair the loader, settings or catalogue boundary |
| `route-include.workspace-lock-unavailable` | Use the exact cause and path from the shared lock-failure wording |
| `route-include.target-changed` | Review a fresh preview |
| `route-include.recovery-unavailable` | Restore access to recovery storage |
| `route-include.recovery-history` | Warning. A draft, damaged or invalid bundle is retained at the reported path. It does not block unrelated work |
| `route-include.recovery-coverage-unavailable` | Warning. History could not be checked for the reported store. Current workspace validation still applies |
| `route-include.recovery-conflict` | Current-operation recovery identity or preparation conflict |
| `route-include.write-failed` | Review retained recovery and partial effects |
| `route-include.verification-failed` | Review final bytes and retained recovery |
| `route-include.recovery-failed` | Review the recovery artifact |
| `route-include.recovery-artifact-retained` | Verified changes retain a cleanup artifact |
| `route-include.interrupted` | Review retained effects before retrying |

## Conformance

Prove the accepted operand forms and every rejection, previews without effects,
text and source-generated JSON, no-change output, main-root membership, target
byte preservation, and the mutation and recovery boundary.
Prove field-wise merging, hand-written registration, and Include followed by Index with identical loader bytes.

## Recovery History

Drafts, damaged or invalid bundles, and unavailable recovery coverage produce warnings. Verified final bundles from earlier runs are normal retained evidence and produce no warning in writing commands. Historical artifacts never veto unrelated work. Current target, ownership, permission, plan and expected-state checks still apply. The current operation must prepare and verify its own required recovery bundle before effects. Historical paths remain separate from that preparation and remain unchanged.
