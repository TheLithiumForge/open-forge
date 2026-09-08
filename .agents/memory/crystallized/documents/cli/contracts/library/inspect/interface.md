---
open-forge:
  description: Accepted public interface for complete Library inventory and exact projection comparison
  responsibility: Define Library inspect syntax, strict record subject, inventory, comparisons, results, errors, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Inspect, Interface, ReadOnly, Workspace, CurrentTruth]
---

# library inspect Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for read-only
`open-forge library inspect`. It owns the public syntax, Library-ID subject,
strict record shape, complete source inventory, registered/observed comparison,
semantic statuses, output, errors, examples, and caller-visible conformance.
The command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and result formation. The [Library group entrypoint](../_library.md)
defines group routing and exact help order. The [Library list Interface](../list/interface.md)
defines the shared record shape and bounded list observation. The shared [Global
CLI Flags](../../shared/global-flags/interface.md) define the six unchanged
global flags. The shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define the schema-v1 envelope, status exits, and output streams. The shared [CLI
Source References](../../shared/source-references/interface.md) define automatic
source-ID derivation; a Library ID remains outside that source-reference grammar.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines shared realization detail without changing this public contract.

## Purpose And Boundary

`library inspect` explains one exact consumer-local Library record. It performs
a complete inventory of eligible ordinary files below that record's real
ordinary `.agents` directory, derives their consumer-relative destinations, and
compares the complete observed projection with every registered path and its
expected relative file link.

Inspect distinguishes a healthy exact projection from safely observed additions,
retirements, missing links, and changed links. A complete safe comparison with
any such drift yields `attention`. An unavailable or incomplete record, source
root, inventory, or projection fact yields `incomplete`. Unsafe identity or
record or link ambiguity yields `blocked`. A missing or unknown inspect ID is
`invalid`.

The operation is read-only, deterministic, and stateless. It does not acquire a
lock, create recovery state, write any file, follow a destination link to read
source bytes, or invoke another command.

## Syntax And Subject

The complete public command form is:

```text
open-forge library inspect <library-id> [global flags]
```

Exactly one Library ID is required in domain mode. It matches:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The complete ID length is 1–128 characters. Matching is exact, case-sensitive,
and limited to the `id` members of the selected workspace record. A malformed,
missing, or unknown ID is `invalid`; duplicate or unsafe identity is `blocked`.

The operand is a Library management identity, not an automatic source ID and
not a source-reference operand. `.agents/...` path values, source IDs, and a
source-root path are not alternate forms for this subject. There is no `--all`,
source selector, inventory filter, link selector, or mutation flag.

The shared flags are unchanged:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

Their grammar, defaults, repetition, composition, terminal behavior, and no-op
rules remain in [Global CLI Flags](../../shared/global-flags/interface.md).

## Workspace And Strict Record

The workspace is the exact current directory unless `--workspace <path>` selects
one exact directory. Inspect reads only `.agents/open-forge.libraries.json` in
that workspace. The Library record is separate from
`.agents/open-forge.lifecycle.json`.

The record is schema v1 with this strict shape:

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

The top-level record has only `schemaVersion` and `libraries`. Each Library
object has only `id`, `sourceRoot`, and `paths`. Each `paths` item is one
canonical `.agents/...` path string that is both the source-relative path below
`sourceRoot` and the identical destination path below the selected workspace.
Unknown members, missing members, wrong types, malformed paths, and an invalid
source-root/path relationship make the record invalid. Duplicate IDs, duplicate
paths across Libraries, and another ambiguous record identity are `blocked`
because Inspect cannot choose one meaning.

The expected link derived from `sourceRoot` and a path is a raw relative slash
path. It may contain `..` segments needed to reach the contained source root,
but it must not be absolute, backslash-separated, or resolve outside the
selected workspace or the recorded source file.

A missing record cannot establish the requested ID and therefore returns
`invalid` with the requested subject retained. A record read failure is
`incomplete`, not an empty record. An unsafe record identity or ambiguous
mapping is `blocked`.

Libraries and path arrays use ordinal deterministic ordering: Libraries by `id`
and each path array by its path string. Inspect never rewrites or normalizes the
record.

## Source-Root Preconditions

The selected Library's `sourceRoot` must be a non-empty workspace-relative slash
path without `.` or `..` segments that is lexically and physically contained by
the selected workspace. It must identify a real ordinary directory. Its direct
`.agents` child must also be a real ordinary directory. A source root without a
real ordinary `.agents` directory is invalid. The source root must be physically
selected consumer `.agents` destination namespace; overlap or alias is
`blocked`.

Unsafe containment, alias, or physical identity is `blocked`. An absent or
unreadable required directory is `incomplete`. Inspect does not substitute a
different root or treat an unavailable root as an empty source.

## Complete Eligible Inventory

When the source-root precondition is valid, Inspect forms the complete eligible
inventory below its `.agents` directory. An eligible item is an ordinary file
with a canonical source-relative `.agents/...` path. The inventory excludes
recognized entrypoints, adjacent overwrite companions, lifecycle and Library
records, Loader and other manager controls, symbolic links, junctions, reparse
aliases, and special files.

Each eligible source path maps to the same consumer-relative destination path.
Inspect does not rename, select a subset, or assign a different destination.
The complete inventory includes every eligible file and proves when the set is
empty. A permission, read, traversal, or identity boundary that prevents a
complete safe inventory is `incomplete`; it is never represented as an empty
set. Unsafe or ambiguous identity is `blocked`.

For every eligible destination path, the result preserves the normal automatic
source ID derived from the destination path under the shared [Automatic Source
IDs](../../shared/source-references/interface.md#automatic-source-ids) rules.
The destination-derived ID is separate from the inspected Library ID and never
becomes a source-reference operand for Inspect.

## Registered And Observed Projection

The registered projection is the exact `paths` array for the selected Library.
The observed projection contains the complete eligible inventory's destination
paths and the no-follow observation of each corresponding consumer entry.

For each destination, Inspect records whether:

| Relation | Meaning |
| --- | --- |
| `current` | The eligible source exists, the path is registered, and the destination is the expected relative file link. |
| `added` | The complete eligible source inventory contains a path absent from the registered record. |
| `retired` | The registered record contains a path absent from the complete eligible source inventory. |
| `missing` | The source path is eligible and registered, but its destination entry is absent. |
| `changed` | The source path is eligible and registered, but the destination occupant or link target differs from the expected relative file link. |
| `unavailable` | A required source or destination fact cannot be established completely. |
| `blocked` | Source, destination, link, containment, or mapping identity is unsafe or ambiguous. |

The exact comparison is set equality over canonical source and destination paths
plus exact expected-link text and observed relative-link identity. A complete
comparison has no `added`, `retired`, `missing`, or `changed` relation. It does
not rely on an order supplied by directory enumeration.

## Result Shape

JSON uses the shared schema-v1 envelope. The command-local `result` object has
this complete member order:

```text
InspectResult {
  record: InspectRecord,
  source: SourceInventory,
  projection: ProjectionComparison,
  findings: Finding[]
}

InspectRecord {
  path: `.agents/open-forge.libraries.json`,
  state: "not-started" | "complete" | "missing" | "invalid" | "unavailable" | "blocked" | "failed" | "interrupted",
  id: string | null,
  sourceRoot: string | null,
  registeredPaths: RegisteredPath[]
}

RegisteredPath {
  sourcePath: string,
  destinationPath: string,
  expectedRelativeLink: string | null,
  sourceId: string | null
}

SourceInventory {
  rootState: "not-started" | "available" | "missing" | "invalid" | "unavailable" | "blocked",
  state: "not-started" | "complete" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted",
  eligiblePaths: EligiblePath[]
}

EligiblePath {
  sourcePath: string,
  destinationPath: string,
  sourceId: string | null
}

ProjectionComparison {
  state: "not-started" | "complete" | "incomplete" | "blocked" | "failed" | "interrupted",
  comparisons: PathComparison[]
}

PathComparison {
  sourcePath: string,
  destinationPath: string,
  sourceId: string | null,
  relation: "not-started" | "current" | "added" | "retired" | "missing" | "changed" | "unavailable" | "blocked",
  registered: RegisteredPath | null,
  observedRelativeLink: string | null
}

Finding {
  code: "library-inspect.invalid-id" | "library-inspect.unknown-id" |
    "library-inspect.record-invalid" | "library-inspect.record-unavailable" |
    "library-inspect.record-blocked" |
    "library-inspect.source-root-invalid" | "library-inspect.source-root-unavailable" |
    "library-inspect.source-root-blocked" | "library-inspect.inventory-incomplete" |
    "library-inspect.path-added" | "library-inspect.path-retired" |
    "library-inspect.link-missing" | "library-inspect.link-changed" |
    "library-inspect.link-blocked" | "library-inspect.operation-failed" |
    "library-inspect.interrupted",
  status: SharedStatus,
  libraryId: string | null,
  path: string | null,
  cause: bounded-string
}
```

Every array is present. A known empty eligible inventory is an empty array with
complete inventory state. An unavailable inventory remains an empty array with
an incomplete state and finding; it is not a known empty source. `sourceId` is
null only when the destination-derived identity is not applicable or cannot be
established safely.

Comparisons are ordered by canonical `destinationPath`, then `sourcePath`, with
ordinal comparison. Registered and observed facts remain separate in each
comparison. The inspected Library ID is not copied into `sourceId`.

## Semantic Results

The command uses the shared seven statuses:

| Status | Meaning for `library inspect` |
| --- | --- |
| `complete` | The exact requested record, valid source root, complete eligible inventory, and exact registered/observed comparison are established with no drift. |
| `attention` | The source inventory and projection comparison are complete and safe, with additions, retirements, missing links, or changed links. |
| `incomplete` | A record, source-root, inventory, or projection fact is unavailable or incomplete. The command does not claim an empty or exact projection. |
| `invalid` | The Library ID is malformed, missing, or unknown, or the strict record or source-root structure is invalid. |
| `blocked` | Unsafe identity, containment, record, mapping, or link ambiguity prevents a safe comparison. |
| `failed` | An unexpected operation or result-formation failure occurred. |
| `interrupted` | The caller interrupted the operation before its result was complete. |

When several ordinary conditions apply, status precedence is `failed`,
`invalid`, `blocked`, `incomplete`, `attention`, then `complete`, with
`interrupted` retaining its event meaning. A missing inspect ID is always
`invalid`; it is never an empty successful inspection.

The `next` member of the shared envelope is `null` for this command. Inspect
reports the exact comparison and does not select a repair or another operation.

## Human And Structured Output

The default human view is expanded. It identifies the exact Library ID, record
path and state, source root state, inventory coverage, deterministic eligible
paths, registered paths, observed link state, comparison relations, findings,
and status. It states `attention` only when all required inventory and
comparison facts are safe and complete.

An exact healthy result may render:

```text
Open Forge library inspect
Library: team-knowledge
Record: .agents/open-forge.libraries.json (complete)
Source root: shared/team-knowledge (available)
Inventory: complete (1 eligible path)

.agents/directives/review.md
  Source ID: directives/review
  Registered: yes
  Observed link: current
  Relation: current

Status: complete
```

Compact output retains the Library ID, record and source-root states, inventory
coverage, source and destination paths, destination-derived source IDs, exact
comparison relations, findings, and status. It omits optional explanation but
does not omit a safety or availability state. `--verbose` adds bounded
diagnostics without changing the typed result.

`--json` emits one complete structured result from the same typed result for
every semantic status and never prompts. Human `complete`, `attention`, and
`incomplete` output uses stdout. Human `invalid`, `blocked`, `failed`, and
`interrupted` output uses stderr. Bounded diagnostics use stderr under the
shared result-coordinate rules.

## Errors And Non-Goals

Every human error names `library inspect`, the supplied Library ID, the record or
path when known, the cause, and the bounded next fact needed to understand the
failure. A missing record or unknown ID does not select a nearby ID. An
unavailable inventory does not become a zero-length inventory. An unsafe or
ambiguous link does not become a changed link.

`library inspect` does not:

- Accept an automatic source ID, exact `.agents` source path, or source-root
  operand in place of the Library ID.
- Limit the inventory to a named path or report a partial inventory as exact.
- Follow destination links to read source bytes or inspect unregistered files.
- Create, remove, retarget, or repair links or the Library record.
- Acquire a lock or create recovery, cache, index, or receipt state.
- Change route meaning, generated navigation, or Framework loading.

## Examples

Inspect one exact Library ID:

```text
open-forge library inspect team-knowledge
```

Request the complete comparison as structured output:

```text
open-forge library inspect team-knowledge --json
```

Select one exact workspace and compact human output:

```text
open-forge library inspect team-knowledge --workspace ../workspace --view=compact
```

## Public EndToEnd Journeys (exactly three)

These are the only public EndToEnd journeys for `library inspect`:

1. **Healthy exact record plus complete inventory.** With one exact strict
   record, a real ordinary source root, a complete eligible inventory, and
   matching registered links, invoke `open-forge library inspect <library-id>`
   and observe deterministic `complete` facts and destination-derived source
   IDs.
2. **Missing ID, invalid.** With a readable strict record, invoke
   `open-forge library inspect` without the required ID and observe `invalid`
   with no source inventory or projection claim.
3. **Complete inventory additions, retirements, and link drift, attention.** With
   a strict record and a safely complete inventory whose set and registered
   links contain an addition, a retirement, and a missing or changed link,
   invoke Inspect and observe `attention` with each exact comparison relation.
   Incomplete and unsafe boundaries remain lower-tier evidence, not additional
   public journeys.

## Lower-Tier Conformance

Unit and Integration evidence may cover malformed and unavailable records,
duplicate IDs and mappings, source-root containment and ordinary-directory
checks, complete eligibility filtering, unreadable or unsafe inventory
boundaries, exact path-set comparison, all safe drift relations, source-ID
derivation, deterministic ordering, shared result coordinates, stream
selection, and unchanged workspace bytes. Those cases do not add public
EndToEnd journeys beyond the three above.

## Related Current Sources

- [library inspect Behavior Contract](behavior.md)
- [Library group entrypoint](../_library.md)
- [Library list Interface Contract](../list/interface.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Shared Result Coordinates Interface Contract](../../shared/result-coordinates/interface.md)
- [CLI Architecture](../../../architecture.md)
