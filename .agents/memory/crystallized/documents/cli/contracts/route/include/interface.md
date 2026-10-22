---
open-forge:
  description: Accepted public contract for listing an existing workspace file in the loader
  responsibility: Define what route include accepts, changes, reports and preserves
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Include, Mutation, CurrentTruth]
---

# route include Interface Contract

## Status And Authority

This contract implements the maintainer's frozen route include decision.
The [shared result coordinates](../../shared/result-coordinates/interface.md)
define the envelope, statuses, exits and streams. The [CLI Architecture](../../../architecture.md)
defines layering, parsing and serialization. The [mutation and recovery design](../../../technical-designs/mutation-and-recovery.md)
defines locking, expected-state checks and recovery.

## Purpose

Include lists an existing document where it lives. It creates no document or catalogue.

## Syntax

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
```

Shared flags keep their existing meaning. There is no prompt or confirmation flag.
Omitting `--dry-run` authorizes application after preflight.

## Operand

Accept one workspace-relative file with either path separator and an optional
leading `./`. Report its canonical slash-separated workspace path.
The file must exist and contain UTF-8 text.
Reject directories, globs, URLs, fragments, lexical or linked workspace escapes,
Git metadata, the loader, settings and lock files as invalid input.
A directory error names its `_<folder>.md` catalogue and says whether to create it first.

Outside `.agents/`, any text file is eligible. A Markdown file with
`## Entries` is followed as a catalogue. Its own frontmatter never sets loading.
Inside `.agents/`, accept only a direct child folder's canonical catalogue.
A deeper file error names that folder's catalogue. Files directly in `.agents/` are invalid.

## Metadata And Existing Rows

A new external row requires `--description`. Repeat `--tag` in the order to keep,
with or without a leading `#`. Reject blank, invalid and duplicate tags.
Without tags the document loads on demand. Repeat `--apply-to` using the existing
applyTo expression grammar. Sort supplied atomic patterns by ordinal comparison and remove exact duplicates, as Create does.

Each omitted field keeps its current value. A supplied option replaces only its
own field. An identical request is a verified no-op. Duplicate matching rows are invalid and
point to Exclude followed by Include to choose one policy. An existing hand-written row
is registered without rewriting it. To clear tags or conditions, edit the row or
exclude and include again. Metadata options on a main-root catalogue are invalid
and name its frontmatter as the editing location.

New rows follow the existing loader shape, use Index's percent-encoding form
and append after the last Entries row. No tags or conditions means no suffix.
The loader row controls loading. Only external targets are registered in `roots`.

## Preview And Effects

Dry-run and apply use the same plan. The report shows loader rows before and after,
registration before and after, and every planned loader or settings change.
Only `.agents/loader.md` and `.agents/open-forge.json` may change.
Target bytes remain unchanged. Dry-run creates no lock, recovery bundle or file.

## Results And JSON

Use the shared statuses and exit mapping. Valid previews, verified application
and verified no-ops are `completed`. Rejected input is `invalid-input`.
Unsafe or stale plans are `blocked`. Unavailable required recovery facts are
`incomplete`. Application or verification failure is `failed`.
Cancellation is `cancelled`. A verified result with a positively retained recovery
artifact is `completed-with-warnings`.

Command data version 1 uses the shared schema-3 envelope with command `route include`.
The data has `mode`, `target`, `before`, `after`, `registeredBefore`,
`registeredAfter` and `verified`. Missing rows are null.
Effects, recovery and the next action stay on the shared envelope.
A successful no-op uses the existing `nothing-to-do` headline and an empty effects list.

## Findings catalogue

| Code | Meaning |
| --- | --- |
| `route-include.invalid-input` | Correct the named operand or option |
| `route-include.workspace-unsafe` | Repair the loader, settings or catalogue boundary |
| `route-include.workspace-lock-unavailable` | Wait for the other operation |
| `route-include.target-changed` | Review a fresh preview |
| `route-include.recovery-unavailable` | Restore access to recovery storage |
| `route-include.recovery-conflict` | Resolve the existing recovery artifact |
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
