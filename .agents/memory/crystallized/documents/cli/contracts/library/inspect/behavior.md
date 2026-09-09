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
  workspace as a real ordinary directory with no linked ancestry.
- Inspect enumerates the complete eligible ordinary-file inventory below that
  selected root. It never turns an unavailable inventory into an empty
  inventory.
- Every eligible source path maps below the recorded destination root, preserving its suffix. The registered record and observed destination link remain separate
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

## Source-Root Resolution

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

## Eligible Source Inventory

Recursively enumerate the selected real source root, recording each eligible
ordinary file by its canonical portable path relative to that root. A source
containing no `.agents` or `content` child is valid. Empty eligible inventory is
complete when the entire selected tree was safely observed.

Exclude Git metadata at any path segment, known manager controls, symlinks,
junctions, reparse points and special entries. Recognize Open Forge Loader,
entrypoint and overwrite controls at their original `.agents` source coordinates
before remapping. A remap cannot make those controls eligible. External
`_name.md`, `*.overwrite.md` and README remain ordinary opaque Library content. Inspect excluded entries without
following them and never descend into excluded metadata or linked directories.
Existing source classification and protected-control rules remain applicable.

An inaccessible directory, enumeration failure, unavailable eligible ordinary
file or unsafe required boundary prevents complete inventory. Retain known safe
facts as partial evidence, never as permission to delete retired links. Source
bytes are never copied, rewritten or deleted. Only eligible leaf membership and
physical path facts feed projection planning.

## Registered And Observed Facts

The registered side is the exact path set from the selected record. The
observed side is the complete eligible inventory plus the no-follow destination
entry observation for each relevant destination.

For every union member of the registered and observed path sets:

1. Match canonical source and destination paths exactly.
2. Derive the source and destination from `sourceRoot`, `destinationRoot` and
   the source-relative suffix. Measure the expected raw relative-link text
   from the actual destination parent to the source leaf; this derivation
   does not require source availability.
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
   source root that is not an ordinary directory.
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
