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

Read only `.agents/open-forge.libraries.json` in the selected workspace. Resolve
its exact schema-v1 shape from the [Interface Contract](interface.md#workspace-and-record):

- Require integer `schemaVersion: 1` and the top-level members `schemaVersion`
  and `libraries` only.
- Require each Library object to contain only `id`, `sourceRoot`, and `paths`.
- Require each `paths` item to be one canonical `.agents/...` path string.
- Validate the Library ID grammar, length, and ordinal identity. Duplicate IDs,
  duplicate paths, duplicate destinations across Libraries, and unsafe record
  identity are blocked ambiguities.
- Validate canonical slash paths, `.agents/...` source and destination paths,
  identical source and destination interpretation, and duplicate path absence.
- Retain the exact bounded record facts needed for the result; do not infer
  fields from source bytes or destination occupants.

If the record is absent, form `record.state=missing`, an empty Library array,
`libraryCount=0`, `inventory=not-requested`, and `complete` status. If reading or
parsing is unavailable, retain `record.state=unavailable`, leave unknown arrays
empty, and form `incomplete`. If the shape is malformed, form `invalid`. If a
record identity or path is unsafe or ambiguous, form `blocked`. These states
are never collapsed into a zero-library claim.

Sort Library records by exact `id` using ordinal comparison. Sort each path array
by its path string using ordinal comparison. Sorting is a result projection rule
and does not rewrite the record.

## Source-Root Checks

For each valid Library record, resolve `sourceRoot` from the selected workspace.
Establish all of these facts:

1. The value is non-empty, workspace-relative, canonical slash-separated, and
   free of absolute, `.`, or `..` segments.
2. The resolved root is lexically and physically contained by the selected
   workspace.
3. The root is a real ordinary directory.
4. Its direct `.agents` child is a real ordinary directory.

The source root and selected consumer `.agents` destination namespace must be
physically disjoint. An overlap or alias is `blocked`.

Failure to establish a required directory because it is absent or unreadable
is `missing` or `unavailable` and contributes `incomplete`. A root that is not
an ordinary directory, or that lacks a real ordinary `.agents` child, is
`invalid`. Unsafe containment, alias, or physical identity is `blocked`.

Do not enumerate the `.agents` child. Its presence is only a source-root
precondition for List.

## Registered-Link Observation

For each path in the bounded record, derive the expected relative link from the
Library's `sourceRoot` and the path, then resolve the destination's parent under
the selected workspace and observe the destination directory entry without
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
   without a real ordinary `.agents` directory.
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
the explicit `inventory=not-requested` fact, findings, and status. Expanded
presentation adds workspace framing and bounded causes. `--verbose` adds only
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
- real ordinary source-root and `.agents` directory checks;
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
