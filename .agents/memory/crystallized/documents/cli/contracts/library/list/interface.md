---
open-forge:
  description: Accepted public interface for bounded read-only Library list observations
  responsibility: Define Library list syntax, strict record input, source-root and link states, results, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, List, Interface, ReadOnly, Workspace, CurrentTruth]
---

# library list Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for read-only
`open-forge library list`. It owns the public syntax, strict record input,
bounded observation result, semantic statuses, output, errors, examples, and
caller-visible conformance for this command. The command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and result formation. The [Library group entrypoint](../_library.md)
defines group routing and the exact command order. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
define the six unchanged global flags. The shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define the schema-v1 envelope, status exits, and output streams. The shared [CLI
Source References](../../shared/source-references/interface.md) define automatic
source IDs and canonical `.agents` paths; this command does not create another
source-ID grammar.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines shared realization detail without changing this public contract.

## Purpose And Boundary

`library list` answers which consumer-local Library records are readable and
which registered destination links are currently observable. It reads one
bounded `.agents/open-forge.libraries.json` record, checks each recorded source
root, and observes each registered destination entry without reading its link
target's file bytes.

The command does not perform a complete source inventory. It cannot report
source additions or retirements that are absent from the record. A safely
observed missing or changed registered link is projection drift and yields
`attention`; it is not a request to repair that link. An unavailable or
incomplete fact yields `incomplete`, and an unsafe identity or ambiguous record
or link yields `blocked`.

The operation is read-only, deterministic, and stateless. It does not acquire a
lock, create a recovery artifact, write any file, follow a link to inspect its
target, or invoke another command.

## Syntax

The complete public command form is:

```text
open-forge library list [global flags]
```

There are no domain operands and no operation-specific flags. The command does
not accept a Library ID, a source ID, a source-reference path, a source-root
operand, or a write-policy flag. The six shared global flags are unchanged:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

Their spelling, values, defaults, repetition, terminal behavior, and no-op
rules remain in [Global CLI Flags](../../shared/global-flags/interface.md).
`--help` and `--version` stop before domain work. A well-formed shared flag with
no applicable behavior remains a shared no-op.

## Workspace And Record

The workspace is the exact current directory unless `--workspace <path>` selects
one exact directory. The command does not search for another workspace or infer
one from a Library ID or path.

The only record path is `.agents/open-forge.libraries.json`. It is separate from
`.agents/open-forge.lifecycle.json`. The record has this strict schema-v1 shape.
Object members not shown here are not accepted:

```text
LibrariesRecord {
  schemaVersion: integer(1),
  libraries: LibraryRecord[]
}

LibraryRecord {
  id: string matching [a-z0-9]+(-[a-z0-9]+)*, length 1..128,
  sourceRoot: workspace-relative slash path,
  paths: string[] of `.agents/...` paths
}
```

The `libraries` array is the complete bounded record. A `LibraryRecord` has only
`id`, `sourceRoot`, and `paths`. Each `paths` item is one canonical
consumer-relative `.agents/...` path. That path is both the source-relative path
below `sourceRoot` and the destination path below the selected workspace. The
expected link is derived from that path and `sourceRoot`; it is not stored in the
record.

The record is invalid when `schemaVersion` is not the integer `1`, a required
member is missing, an unknown member is present, a value has the wrong type, an
ID is malformed, or a path item is empty, absolute, escaping, backslash-
separated, duplicated, or not a canonical `.agents/...` path.
Duplicate Library IDs, duplicate paths across Libraries, ambiguous path
identity, and unsafe record identity are `blocked` because the command cannot
choose one meaning. The record is not treated as empty when parsing or reading
it fails.

The expected link derived from `sourceRoot` and a path is a raw relative slash
path. It may contain `..` segments needed to reach the contained source root,
but it must not be absolute, backslash-separated, or resolve outside the
selected workspace or the recorded source file.

A missing record is a known zero-library record. In that case `libraries` is an
empty array, record coverage is complete, and the command returns `complete`.
An unreadable or unavailable record is not a missing record and returns
`incomplete`. A structurally unsafe record identity returns `blocked`.

Library IDs are exact management identities. They use the grammar above and are
separate from automatic source IDs. The ID for an eligible projected file is
derived from its destination path under the shared [Automatic Source IDs](../../shared/source-references/interface.md#automatic-source-ids)
rules. A Library ID is never accepted where a source-reference operand is
expected.

## Source-Root State

For each record, `sourceRoot` is resolved from the selected workspace as one
workspace-relative path. It must be lexically and physically contained by that
workspace, be non-empty and free of `.` or `..` segments, identify a real
ordinary directory, and contain a direct child named `.agents` that is also a
real ordinary directory. A source root without that ordinary `.agents`
directory is invalid. The source root must be physically
disjoint from the selected consumer `.agents` destination namespace; overlap or
alias is blocked.

The list operation does not enumerate that `.agents` directory. Its source-root
state is one of:

| State | Meaning | Result effect |
| --- | --- | --- |
| `available` | The source root and its ordinary `.agents` directory are established. | Registered-link observations may be complete. |
| `missing` or `unavailable` | A required directory or fact cannot be read. | `incomplete`; no empty source is inferred. |
| `invalid` | The path is not a valid ordinary source root or has no ordinary `.agents` directory. | `invalid`. |
| `blocked` | Lexical or physical containment, alias, or identity is unsafe or ambiguous. | `blocked`. |
| `not-started` | Record resolution stopped at an earlier boundary. | No independent status. |

## Registered-Link Observation

For every path in a Library's `paths` array, the command observes only the
destination directory entry named by that same path. It does not follow the
destination to read source bytes or to discover unregistered files.

The observation is one of:

| State | Meaning |
| --- | --- |
| `current` | The destination is a relative file link and its stored target text exactly equals the derived expected relative link. |
| `missing` | No destination entry is present. |
| `changed` | A destination entry is safely observable but is not the expected relative file link. |
| `unavailable` | The destination fact cannot be read completely. |
| `blocked` | Link type, identity, containment, or alias meaning is unsafe or ambiguous. |
| `not-started` | An earlier boundary prevented observation. |

`sourceId` in a path observation is the normal destination-derived automatic
source ID when the destination is an eligible `.agents` file. It is retained
separately from `library.id`; the Library ID is never substituted for it.

## Result Shape

JSON uses the shared schema-v1 envelope. The command-local `result` object has
this complete member order:

```text
ListResult {
  record: RecordView,
  libraries: LibraryView[],
  inventory: "not-started" | "not-requested",
  coverage: "not-started" | "complete" | "incomplete" | "blocked" | "failed" | "interrupted",
  findings: Finding[]
}

RecordView {
  path: `.agents/open-forge.libraries.json`,
  state: "not-started" | "missing" | "complete" | "invalid" | "unavailable" | "blocked" | "failed" | "interrupted",
  libraryCount: nonnegative-integer | null
}

LibraryView {
  id: string,
  sourceRoot: string,
  sourceRootState: "not-started" | "available" | "missing" | "unavailable" | "invalid" | "blocked",
  paths: RegisteredPath[]
}

RegisteredPath {
  sourcePath: string,
  destinationPath: string,
  expectedRelativeLink: string | null,
  sourceId: string | null,
  state: "not-started" | "current" | "missing" | "changed" | "unavailable" | "blocked",
  observedRelativeLink: string | null
}

Finding {
  code: "library-list.invalid-record" | "library-list.record-unavailable" |
    "library-list.record-blocked" |
    "library-list.source-root-invalid" | "library-list.source-root-unavailable" |
    "library-list.source-root-blocked" | "library-list.link-missing" |
    "library-list.link-changed" | "library-list.link-unavailable" |
    "library-list.link-blocked" | "library-list.operation-failed" |
    "library-list.interrupted",
  status: SharedStatus,
  libraryId: string | null,
  path: string | null,
  cause: bounded-string
}
```

Every array is present. Known empty arrays have count `0`; unavailable counts
are `null`. `inventory` is `not-requested` for a valid domain request because
List never forms a complete source inventory. `coverage` describes only the
bounded record, source-root, and registered-link facts requested by this
command.

Libraries are ordered by `id` using ordinal comparison. Each `paths` array is
ordered by its path string using ordinal comparison. The same order is used by
compact human output, expanded human output, and JSON.

## Semantic Results

The command uses the shared seven statuses:

| Status | Meaning for `library list` |
| --- | --- |
| `complete` | The record is known missing and therefore has zero libraries, or every bounded record, source-root, and registered-link fact is complete and safe with no drift. |
| `attention` | Bounded facts are complete and safe, and one or more registered links are missing or changed. No complete source inventory was performed. |
| `incomplete` | A required record, source-root, or registered-link fact is unavailable or incomplete. The command does not report an empty substitute. |
| `invalid` | The command input or strict record shape is invalid, including a source root without a real ordinary `.agents` directory. |
| `blocked` | Unsafe identity, containment, record identity, or link ambiguity prevents safe observation. |
| `failed` | An unexpected operation or result-formation failure occurred. |
| `interrupted` | The caller interrupted the operation before its result was complete. |

When several ordinary conditions apply, the shared status precedence is
`failed`, `invalid`, `blocked`, `incomplete`, `attention`, then `complete`, with
`interrupted` retaining its event meaning. A missing record is never combined
with an unavailable or malformed record state.

The `next` member of the shared envelope is `null` for this command. List
reports observations; it does not select a repair or another operation.

## Human And Structured Output

The default human view is expanded. It begins with the selected workspace and
record state, identifies that source inventory is not requested, and then
renders each Library in deterministic order with its source-root state and
registered-link observations. A safe drift result identifies the affected path
and uses `Status: attention`.

An empty or missing record may render:

```text
Open Forge library list
Workspace: <selected workspace>
Record: .agents/open-forge.libraries.json (missing; zero libraries)
Inventory: not requested
Libraries: 0
Status: complete
```

A healthy record may render:

```text
Open Forge library list
Workspace: <selected workspace>
Record: .agents/open-forge.libraries.json (complete)
Inventory: not requested

Library: team-knowledge
Source root: shared/team-knowledge (available)
  .agents/directives/review.md -> .agents/directives/review.md
  Source ID: directives/review
  Link: current

Status: complete
```

Compact output retains the result, coverage, `inventory=not-requested`, Library
IDs, source roots, registered paths, source IDs when available, link state, and
status. It omits optional explanation but does not omit a finding or safety
state. `--verbose` adds bounded diagnostic detail without changing facts or
status.

`--json` emits one complete structured result from the same typed result for
every semantic status. It does not follow links, start an inventory, or prompt.
Human `complete`, `attention`, and `incomplete` output uses stdout. Human
`invalid`, `blocked`, `failed`, and `interrupted` output uses stderr. Bounded
diagnostics use stderr under the shared result-coordinate rules.

## Errors And Non-Goals

Every human error names `library list`, the record or affected Library when
known, the cause, and the bounded observation that could not be established. A
malformed or unavailable record is not rendered as zero libraries. An unsafe or
ambiguous link is not treated as a changed link. No fallback source root or
record is inferred.

`library list` does not:

- Accept a Library ID or source-reference operand.
- Enumerate source files or infer additions or retirements.
- Read target file bytes through a registered link.
- Create, remove, retarget, or repair a link or record.
- Acquire a lock or create recovery, cache, index, or receipt state.
- Change route meaning, generated navigation, or Framework loading.

## Examples

List the bounded record from the selected workspace:

```text
open-forge library list
```

Request the same bounded facts as structured output:

```text
open-forge library list --json
```

Select one exact workspace and use compact human output:

```text
open-forge library list --workspace ../workspace --view=compact
```

## Public EndToEnd Journeys (exactly three)

These are the only public EndToEnd journeys for `library list`:

1. **Empty or missing record, complete.** With the record absent or present as
   a strict empty record at `.agents/open-forge.libraries.json`, invoke
   `open-forge library list` and observe `complete`, zero libraries, and
   `inventory=not-requested`.
2. **Healthy records, deterministic complete.** With a strict record containing
   valid source roots and current registered links, invoke List twice and
   observe identical ordinal Library and path order, source IDs, observations,
   and `complete` status.
3. **Complete registered-link drift, attention without full inventory.** With a
   strict record and a safely observable missing or changed registered link,
   invoke List and observe `attention`, the link finding, and
   `inventory=not-requested`; no source-inventory addition or retirement is
   asserted.

## Lower-Tier Conformance

Unit and Integration evidence may cover the complete finite boundary, including
strict schema rejection, duplicate identities, path validation, missing versus
unavailable records, source-root states, ordinary-directory checks, no-follow
link observations, unsafe identity, deterministic ordering, shared result
coordinates, stream selection, and unchanged workspace bytes. Those cases do
not add public EndToEnd journeys beyond the three above.

## Related Current Sources

- [library list Behavior Contract](behavior.md)
- [Library group entrypoint](../_library.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Shared Result Coordinates Interface Contract](../../shared/result-coordinates/interface.md)
- [CLI Architecture](../../../architecture.md)
