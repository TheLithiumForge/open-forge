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

The consumer record is `.agents/open-forge.libraries.json`, separate from
lifecycle ownership and consumer permissions. Its exact current schema is:

```json
{
  "schemaVersion": 1,
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "destinationRoot": ".apm/agents/team",
      "paths": ["checks/security.md", "review.md"]
    }
  ]
}
```

Require exactly `schemaVersion` and `libraries` at the top level, and exactly
`id`, `sourceRoot`, `destinationRoot` and `paths` per Library. Require integer
`1`, existing Library-ID grammar, canonical portable roots and source-relative
eligible paths. The destination root is `.` or a normal relative directory;
source roots do not admit `.`. Unknown, missing, null, duplicate and wrongly
typed members are malformed. No previous schema shape, migration or alternate
reader is accepted.

IDs and each source-relative path array use ordinal order. Paths are unique
within a Library. Derived destinations must be unique across Libraries under
portable identity; equal source-relative paths at different destinations are
valid. Empty path arrays and an empty Library array are valid. The record stores
no expected-link text, contents, hashes, timestamps, Git facts, dependencies,
globs or per-file remapping. Link identity derives from both recorded roots and
the source-relative path. Permission is separate from ownership and may be
revoked independently.

A missing record is a valid prior-absence fact for Attach and a complete empty
List result. Inspect, Sync and Detach require the requested ID in a valid record.
Malformed, unavailable and unsafe records retain their existing invalid,
incomplete and blocked classification; none becomes an empty valid record.

## Source-Root State

The source root is a non-empty canonical portable workspace-relative directory,
strictly contained by the selected workspace lexically and physically. It has
no absolute, empty, backslash, `.` or `..` segment and no portable alias. Every
ancestor and the selected root must be a real ordinary directory, without
symlink, junction or reparse ancestry. No specially named child is required.
The selected directory itself scopes the recursively discovered eligible files.

An absent or non-directory source root is `invalid` for Attach. For an existing
registration, unavailable or missing source facts make Inspect or Sync
`incomplete`; a readable non-directory root is `invalid`. Unsafe containment,
linked ancestry or ambiguous identity is `blocked`. List reports only bounded
root availability and does not enumerate descendants. An incomplete source is
never an empty source inventory.

The consumer workspace and its existing ordinary `.agents` control directory
remain consumer-owned. The operation does not create or replace either root.
The destination root may be an ancestor of a contained source root, including
`.`. Actual destination leaves and every mutation target must remain outside
all selected and registered source trees. This per-leaf check preserves source
contents without forbidding workspace-root projection.

## Registered-Link Observation

For every path in a Library's `paths` array, the command observes only the
destination directory entry derived from that source suffix and the recorded destination root. It does not follow the
destination to read source bytes or to discover unregistered files.

The observation is one of:

| State         | Meaning                                                                                                               |
| ------------- | --------------------------------------------------------------------------------------------------------------------- |
| `current`     | The destination is a relative file link and its stored target text exactly equals the derived expected relative link. |
| `missing`     | No destination entry is present.                                                                                      |
| `changed`     | A destination entry is safely observable but is not the expected relative file link.                                  |
| `unavailable` | The destination fact cannot be read completely.                                                                       |
| `blocked`     | Link type, identity, containment, or alias meaning is unsafe or ambiguous.                                            |
| `not-started` | An earlier boundary prevented observation.                                                                            |

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
  destinationRoot: string,
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

| Status        | Meaning for `library list`                                                                                                                                       |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The record is known missing and therefore has zero libraries, or every bounded record, source-root, and registered-link fact is complete and safe with no drift. |
| `attention`   | Bounded facts are complete and safe, and one or more registered links are missing or changed. No complete source inventory was performed.                        |
| `incomplete`  | A required record, source-root, or registered-link fact is unavailable or incomplete. The command does not report an empty substitute.                           |
| `invalid`     | The command input or strict record shape is invalid, including a source root that is not an ordinary directory.                                                  |
| `blocked`     | Unsafe identity, containment, record identity, or link ambiguity prevents safe observation.                                                                      |
| `failed`      | An unexpected operation or result-formation failure occurred.                                                                                                    |
| `interrupted` | The caller interrupted the operation before its result was complete.                                                                                             |

When several ordinary conditions apply, the shared status precedence is
`failed`, `invalid`, `blocked`, `incomplete`, `attention`, then `complete`, with
`interrupted` retaining its event meaning. A missing record is never combined
with an unavailable or malformed record state.

The `next` member of the shared envelope is `null` for this command. List
reports observations; it does not select a repair or another operation.

## Human And Structured Output

Both human views begin with the registration outcome, status, exact workspace
and selection method, followed by the record path/state and coverage. They state
that source inventory was not scanned. Human `requires attention` represents the
typed `attention` status. A missing or known-empty record says that no Libraries
are registered. An invalid, unavailable or unchecked record does not become an
empty result merely because no rows were returned.

Each Library retains its ID, source and destination roots, source-root state,
registered source/destination paths, source IDs when available, and observed
link states. Expanded is the default and adds exact expected/observed link
targets and supporting explanations. Compact uses short mapping rows. Both
retain every finding and safety/availability state. Paths are not truncated.

For example, a missing record may render:

```text
No Libraries are registered.
Status: complete
Workspace: <selected workspace>
Selected by: current directory
Record: missing (.agents/open-forge.libraries.json)
Registered Libraries: 0
Source inventory: not scanned (library list checks registered links only)
Checks: complete
```

The values are illustrative. `--verbose` remains a separate bounded diagnostic
surface and does not change facts or status.

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

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The compact result retains the complete command-owned result graph defined by
its structured schema, including every nullable value and ordered collection.
Its core already carries the facts needed to use the result. For mutation
commands this includes plans, exact previews, effects, permissions when
applicable, verification, findings and recovery. Rendering never asks a caller
to rerun a mutation to recover an omitted receipt.

No collection is truncated and no finding is filtered. Counts describe the
original operation. Both JSON views retain the same result facts.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
