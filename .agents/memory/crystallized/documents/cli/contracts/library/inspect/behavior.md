---
open-forge:
  description: Accepted technology-neutral behavior for complete Library inventory and exact projection comparison
  responsibility: Define Inspect subject resolution, eligible inventory, registered/observed comparison, result formation, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Inspect, Behavior, ReadOnly, Determinism, CurrentTruth]
---

# library inspect Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for read-only
`open-forge library inspect`. It defines technology-neutral subject resolution,
strict record handling, source-root validation, complete eligible inventory,
exact registered/observed projection comparison, deterministic result
formation, safety, and conformance. The [Interface Contract](interface.md)
defines all public grammar, result fields, statuses, output, errors, examples,
and the exactly three public EndToEnd journeys.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines shared realization detail. This behavior does not choose a runtime,
parser, filesystem library, serializer, or other implementation technology.

## Operation Invariants

- Inspect resolves exactly one Library management ID against the exact selected
  workspace record. It does not reinterpret the ID as a source ID or path.
- A strict schema-v1 record is required. A missing record or missing ID is
  `invalid`; malformed record structure is `invalid`; unavailable record facts
  are `incomplete`; unsafe or ambiguous identity is `blocked`.
- A valid source root must be lexically and physically contained by the selected
  workspace and must contain a real ordinary `.agents` directory.
- Inspect enumerates the complete eligible ordinary-file inventory below that
  `.agents` directory. It never turns an unavailable inventory into an empty
  inventory.
- Every eligible source path maps to the same consumer-relative destination
  path. The registered record and observed destination link remain separate
  facts.
- Destination links are observed without following their targets. No source
  bytes are read through a destination link.
- For unchanged workspace bytes and record bytes, inventory order, comparison
  relations, findings, and status are deterministic.
- The operation has no lock, recovery, mutation, cache, index, receipt, or
  hidden command invocation stage.

## Conforming Flow

```text
validated request
  -> exact workspace and one Library ID
  -> strict record and source-root facts
  -> complete eligible source inventory
  -> exact registered/observed projection comparison
  -> typed result, findings, and semantic status
  -> human or JSON projection of that same result
```

Renderers do not rerun inventory, link observation, comparison, or status
selection. They cannot drop an unavailable relation or turn safe drift into an
exact projection.

## Request And Subject Resolution

1. Apply shared terminal handling for `--help` and `--version`.
2. Resolve the exact current directory or exact `--workspace` directory under
   the shared Global CLI Flags contract.
3. Require exactly one domain operand and validate the complete Library-ID
   grammar and 1–128 length.
4. Read the exact record path and match the supplied ID exactly, without case
   correction, fuzzy matching, path interpretation, or another workspace.

Zero or several operands, malformed IDs, a missing record, and an unknown ID
form `invalid`. Duplicate or ambiguous record identities form `blocked` when
the unsafe identity cannot be classified as a simple input error. A record
read failure forms `incomplete`. No candidate is selected by order or
resemblance.

## Strict Record Facts

Resolve the record using the exact schema in [Workspace And Strict Record](interface.md#workspace-and-strict-record):

- Accept only integer `schemaVersion: 1`.
- Accept only top-level `schemaVersion` and `libraries` members.
- Accept only `id`, `sourceRoot`, and `paths` in each Library object.
- Accept only canonical `.agents/...` path strings in each `paths` array.
- Validate ID grammar, length, canonical slash paths, the path's identical
  source/destination interpretation, and source-root/path relationships.
  Duplicate IDs, duplicate paths, duplicate destinations across Libraries, and
  unsafe record identity are blocked ambiguities. Derive expected-link text from
  the source root and path rather than reading it from the record.

Retain the exact requested Library record and path facts in the typed result.
Do not infer a missing path from the source root or an observed destination.
Sort Libraries by ID and each path array by its path string, using ordinal
comparison. Sorting does not alter the record bytes.

## Source-Root Resolution

Resolve the selected `sourceRoot` from the workspace and establish, in order,
that it is:

1. A non-empty workspace-relative slash path without absolute, `.`, or `..`
   segments.
2. Lexically and physically contained by the selected workspace.
3. A real ordinary directory.
4. The parent of a direct real ordinary `.agents` directory.

The source root and selected consumer `.agents` destination namespace must be
physically disjoint. An overlap or alias is `blocked`.

An absent or unreadable required directory is `incomplete`. A root without the
real ordinary `.agents` directory is `invalid`. Unsafe containment, alias, or
physical identity is `blocked`. Inspect never uses another root and never
substitutes an empty source for an unavailable root.

## Eligible Source Inventory

After source-root validation, enumerate every eligible ordinary file below the
source root's `.agents` directory. Eligibility requires a canonical
source-relative `.agents/...` path and an ordinary file identity. Exclude
recognized entrypoints, adjacent overwrite companions, lifecycle and Library
records, Loader and other manager controls, symbolic links, junctions, reparse
aliases, and special files.

The inventory is complete only when every relevant directory and file boundary
has a safe established state. A read or traversal failure leaves the known safe
facts available, marks the affected inventory `incomplete`, and prevents an
`attention` or `complete` result. An unsafe or ambiguous identity is `blocked`.
No source file outside the selected root is included.

For each eligible source path, set the destination to the identical
consumer-relative `.agents/...` path. Derive `sourceId` from that destination
using the shared automatic source-ID rules. Preserve the source ID as a
separate fact from the selected Library ID. The source ID is descriptive output
for the destination path and does not select a source or alter the exact path
comparison.

## Registered And Observed Facts

The registered side is the exact path set from the selected record. The
observed side is the complete eligible inventory plus the no-follow destination
entry observation for each relevant destination.

For every union member of the registered and observed path sets:

1. Match canonical source and destination paths exactly.
2. Derive the expected relative-link text from the selected `sourceRoot` and
   path, if the source root is available.
3. Observe the destination entry without following its target.
4. Retain observed relative-link text only when link identity is safe and
   unambiguous.
5. Assign one relation from the Interface table.

Use these relations:

- `current` requires a present eligible source, a registered mapping, and an
  observed relative file link whose target text equals the expected text.
- `added` identifies an eligible source path with no registered mapping.
- `retired` identifies a registered path with no eligible source path.
- `missing` identifies a registered and eligible path with no destination entry.
- `changed` identifies a registered and eligible path with a safely observed
  destination occupant or link target that differs from the expected link.
- `unavailable` identifies a required source or destination fact that cannot be
  established completely.
- `blocked` identifies unsafe or ambiguous path, containment, link, or identity
  facts.

The comparison is exact over the canonical path sets and expected/observed link
identity. It does not infer equality from file bytes, nearby paths, source
names, or directory order. A complete empty path set is a valid complete
comparison when the source inventory proves it.

## Result And Status Formation

Form the complete `InspectResult` in the Interface member order after subject,
record, source-root, inventory, and projection facts are established. Keep
registered and observed paths separate. Keep arrays present; a known empty
inventory is distinct from an unavailable inventory.

Select status from the highest applicable condition:

1. `interrupted` when caller interruption stops result formation.
2. `failed` for an unexpected operation or result-formation failure.
3. `invalid` for invalid syntax, missing or unknown ID, malformed record, or a
   source root without a real ordinary `.agents` directory.
4. `blocked` for unsafe or ambiguous identity, containment, record, mapping, or
   link facts.
5. `incomplete` for unavailable or incomplete record, source-root, inventory,
   or projection facts.
6. `attention` when inventory and comparison are complete and safe and at least
   one relation is `added`, `retired`, `missing`, or `changed`.
7. `complete` when the exact record, complete inventory, and exact projection
   comparison have no drift.

The shared envelope's command-local `next` is always `null`. Inspect reports
facts and does not select a repair or mutation operation.

## Presentation And Safety

Human and structured renderers consume one typed result. Compact output retains
the exact Library ID, record and source-root state, complete-inventory state,
source and destination paths, destination-derived source IDs, relations,
findings, and status. Expanded output adds bounded explanations and workspace
framing. `--verbose` adds bounded diagnostics only. JSON retains the complete
result for every status.

Read-only execution leaves record, source, destination, and unrelated workspace
bytes unchanged. It creates no lock, recovery, cache, index, receipt, or other
persistent inspection state. It never follows a link to read source bytes and
never treats an incomplete inventory as permission to remove or ignore a
registered path.

## Lower-Tier Conformance

Unit and Integration evidence should cover:

- exact subject and shared-flag resolution, terminal modes, and result
  coordinates;
- strict schema, unknown-member, duplicate-identity, path, and expected-link
  validation;
- source-root containment, ordinary-directory, missing, unavailable, and
  unsafe states;
- complete eligibility admission and exclusion, including ordinary files and
  special or link entries;
- destination-derived automatic IDs kept separate from Library IDs;
- current, added, retired, missing, changed, unavailable, and blocked relations;
- deterministic ordinal ordering, human/JSON parity, and unchanged-state
  snapshots; and
- complete, attention, incomplete, invalid, blocked, failed, and interrupted
  formation without partial-inventory success claims.

Public EndToEnd evidence is limited to the exactly three journeys named by the
[Interface Contract](interface.md#public-endtoend-journeys-exactly-three). No
additional public EndToEnd journey is defined here.

## Related Current Sources

- [library inspect Interface Contract](interface.md)
- [Library group entrypoint](../_library.md)
- [Library list Interface Contract](../list/interface.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Shared Result Coordinates Behavior Contract](../../shared/result-coordinates/behavior.md)
- [CLI Architecture](../../../architecture.md)
