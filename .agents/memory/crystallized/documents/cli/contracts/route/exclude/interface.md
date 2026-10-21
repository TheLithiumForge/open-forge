---
open-forge:
  description: Accepted public contract for removing a workspace file from the loader without deleting it
  responsibility: Define what route exclude accepts, changes, reports and preserves
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Exclude, Mutation, CurrentTruth]
---

# route exclude Interface Contract

## Status And Authority

This contract implements the maintainer's frozen route include decision.
The [shared result coordinates](../../shared/result-coordinates/interface.md)
define the envelope, statuses, exits and streams. The [CLI Architecture](../../../architecture.md)
defines layering, parsing and serialization. The [mutation and recovery design](../../../technical-designs/mutation-and-recovery.md)
defines locking, expected-state checks and recovery.

## Purpose

Exclude removes every loader row for one destination and its roots registration. The target stays where it is.

## Syntax

```text
open-forge route exclude <workspace-file>
  [--dry-run]
```

Shared flags keep their existing meaning. There is no prompt or confirmation flag.
Omitting `--dry-run` authorizes application after preflight.

## Operand

Accept one workspace-relative file with either path separator and an optional
leading `./`. Report its canonical slash-separated workspace path.
The file may be missing. An existing target must contain UTF-8 text.
Reject directories, globs, URLs, fragments, lexical or linked workspace escapes,
Git metadata, the loader, settings and lock files as invalid input.
A directory error names its `_<folder>.md` catalogue and says whether to create it first.

Outside `.agents/`, any text file is eligible. A Markdown file with
`## Entries` is followed as a catalogue. Its own frontmatter never sets loading.
Inside `.agents/`, accept only a direct child folder's canonical catalogue.
A deeper file error names that folder's catalogue. Files directly in `.agents/` are invalid.

## Removal And No Change

Remove every row for the destination, including hand-written rows and rows whose
target is missing. Remove its external `roots` registration too.
For a main-root catalogue, switch its generated row off through loader membership.
When neither a row nor a registration exists, report nothing changed and succeed.

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

Command data version 1 uses the shared schema-3 envelope with command `route exclude`.
The data has `mode`, `target`, `before`, `after`, `registeredBefore`,
`registeredAfter` and `verified`. Missing rows are null.
Effects, recovery and the next action stay on the shared envelope.
A successful no-op uses the existing `nothing-to-do` headline and an empty effects list.

## Findings catalogue

| Code | Meaning |
| --- | --- |
| `route-exclude.invalid-input` | Correct the named operand or option |
| `route-exclude.workspace-unsafe` | Repair the loader, settings or catalogue boundary |
| `route-exclude.workspace-lock-unavailable` | Wait for the other operation |
| `route-exclude.target-changed` | Review a fresh preview |
| `route-exclude.recovery-unavailable` | Restore access to recovery storage |
| `route-exclude.recovery-conflict` | Resolve the existing recovery artifact |
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
