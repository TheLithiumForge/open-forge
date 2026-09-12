---
open-forge:
  description: Accepted technology-neutral behavior for bounded read-only Library list observations
  responsibility: Define List record resolution, source-root checks, no-follow link observation, result formation, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, List, Behavior, ReadOnly, Determinism, CurrentTruth]
---

# library list Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for read-only
`open-forge library list`. It defines technology-neutral request resolution,
strict record handling, source-root checks, bounded registered-link observation,
deterministic result formation, safety, and conformance. The [Interface Contract](interface.md)
owns the public grammar, result fields, statuses, output, errors, examples, and
the exactly three public EndToEnd journeys.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines the shared realization boundary. This behavior does not choose a
runtime, parser, filesystem library, serializer, or other implementation
technology.

## Operation Invariants

- List consumes only the exact selected workspace and the bounded
  `.agents/open-forge.libraries.json` record.
- A missing record is a known empty record with zero libraries and `complete`
  status. A malformed or unavailable record is never converted to that empty
  result.
- The operation reads only source-root directory facts and registered
  destination directory entries. It does not enumerate a source root's
  eligible files or infer unregistered paths.
- A registered destination is observed without following its target. The
  operation does not read target bytes.
- For unchanged selected workspace bytes and record bytes, the typed result,
  ordering, findings, and status are deterministic.
- The operation has no lock, recovery, mutation, cache, index, receipt, or
  hidden command invocation stage.

## Conforming Flow

```text
validated request
  -> exact workspace
  -> strict bounded record resolution
  -> source-root state and registered-link no-follow observations
  -> deterministic typed result, findings, and status
  -> human or JSON projection of that same result
```

Presentation does not rerun record parsing, source-root checks, link
observation, or status selection. It cannot create a complete inventory or
change an incomplete, blocked, or attention result.

## Request And Workspace Resolution

1. Apply the shared terminal handling for `--help` and `--version`.
2. Resolve the exact current directory or exact `--workspace` directory under
   the shared Global CLI Flags contract. Do not search parent directories or
   infer a workspace from a record path.
3. Reject every domain operand and every operation-specific flag. Library list
   has no domain operand.
4. Form one complete request with the shared presentation and diagnostic facts.

Invalid syntax forms `invalid`. An unavailable selected workspace forms the
shared blocked or unavailable workspace result required by the global contract.
No later record fact repairs an invalid request.

## Strict Record Resolution

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

## Source-Root Checks

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

For each source-relative suffix in the bounded record, derive the source and
destination from `sourceRoot` and `destinationRoot`. Measure the expected raw
relative link from the actual destination parent to that source leaf. Resolve
the destination parent under the selected workspace and observe its leaf without
following its target. Record the derived expected and observed relative-link
text only when the corresponding identity can be established safely.

Classify the observation exactly as follows:

- `current` when a relative file link is proven and its target text equals the
  derived expected relative link exactly.
- `missing` when no destination entry exists.
- `changed` when a safely observed occupant exists but is not the expected
  relative file link.
- `unavailable` when the required directory entry cannot be read completely.
- `blocked` when link type, containment, alias, or physical identity is unsafe
  or ambiguous.

Do not resolve the target to inspect its source bytes. Do not search for a
different destination, use a nearby file, or infer a missing path from a source
root. A safely observed `missing` or `changed` link is drift, not proof of a
source addition or retirement.

For an eligible `.agents` destination, derive its automatic source ID from the
destination path under the shared source-reference rules. Keep that ID separate
from the Library management ID. If the destination is not an eligible source
kind or its identity cannot be established, retain a null source ID and the
applicable finding.

## Result And Status Formation

Form the complete `ListResult` in the Interface member order after all requested
bounded facts are established. Arrays remain present and are empty only when a
known empty result or stopped stage requires them. `inventory` is
`not-requested` for valid domain execution.

Select status from the highest applicable condition:

1. `interrupted` when caller interruption stops result formation.
2. `failed` for an unexpected operation or result-formation failure.
3. `invalid` for request or strict-record invalidity, including a source root
   that is not an ordinary directory.
4. `blocked` for unsafe or ambiguous identity, containment, record, or link
   facts.
5. `incomplete` when a required record, source-root, or registered-link fact is
   unavailable or incomplete.
6. `attention` when every bounded fact is complete and safe and at least one
   registered link is safely missing or changed.
7. `complete` when the bounded facts are complete and safe with no drift, or
   when the record is known missing and therefore has zero libraries.

`interrupted` and `failed` retain their event meaning under the shared result
coordinates. A lower-tier finding never becomes `attention` merely because safe
partial facts are present.

The command-local `next` value is always `null`. The result reports the
observation boundary and does not select a repair or follow-up operation.

## Presentation And Safety

Human and structured renderers consume one typed result. Compact presentation
retains IDs, source roots, registered paths, source IDs, link states, coverage,
the explicit fact that source inventory was not scanned, findings, and status. Expanded
presentation adds raw link targets and bounded causes. Both views retain
workspace identity and selection. `--verbose` adds only
bounded diagnostics. JSON preserves the complete command-local result and the
shared envelope for every status.

No renderer may follow a link, enumerate source files, hide a finding, replace a
null fact with zero, or turn a drift observation into a repair action. Read-only
execution leaves workspace bytes and record bytes unchanged and creates no
persistent inspection state.

## Lower-Tier Conformance

Unit and Integration evidence should cover:

- exact workspace selection, operand rejection, shared flags, terminal modes,
  and status/stream coordinates;
- missing, malformed, unavailable, duplicate, and unsafe record shapes;
- exact ID grammar, ordinal Library and path ordering, and path equality;
- real ordinary source-root checks without a mandatory child;
- no-follow current, missing, changed, unavailable, and blocked link states;
- destination-derived automatic source IDs kept separate from Library IDs;
- deterministic human/JSON parity and unchanged-state snapshots; and
- complete, attention, incomplete, invalid, blocked, failed, and interrupted
  formation without a full source inventory.

Public EndToEnd evidence is limited to the exactly three journeys named by the
[Interface Contract](interface.md#public-endtoend-journeys-exactly-three). No
additional public EndToEnd journey is defined here.

## Related Current Sources

- [library list Interface Contract](interface.md)
- [Library group entrypoint](../_library.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Shared Result Coordinates Behavior Contract](../../shared/result-coordinates/behavior.md)
- [CLI Architecture](../../../architecture.md)
