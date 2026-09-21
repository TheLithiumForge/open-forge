---
open-forge:
  description: Accepted technology-neutral behavior for deterministic read-only Extension list facts and ownership observations
  responsibility: Define list's source resolution, section projection, coverage, result formation, and non-mutation conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, List, Behavior, ReadOnly, Determinism, Lifecycle, CurrentTruth]
---

# extension list Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for read-only
`open-forge extension list`. It defines deterministic request resolution, exact
workspace and source handling, ownership observation, Installed
and Available projection, coverage, result formation, and read-only conformance.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the exact shared JSON result schema and exit mapping. The accepted [CLI
Architecture](../../../architecture.md) defines parser, filesystem, and package
implementation boundaries. This
behavior does not duplicate those mechanics or claim their Gate 5 proof.

## Typed Flow

```text
validated request
  -> exact workspace and optional exact source
  -> installed ownership facts and available package facts
  -> requested section projection
  -> coverage and trust classification
  -> one typed result
  -> human or JSON rendering
```

The operation stops after the read result. It never forms an empty mutation
plan. Human and JSON renderers consume the same typed result.

## Request And Source Resolution

1. Resolve shared help/version before workspace or source inspection.
2. Reject positional operands and mutation or wizard flags.
3. Collapse repeated `--installed` and `--available` presence idempotently; reject
   repeated singleton `--source` values.
4. Select exactly CWD or exact `--workspace` without discovery.
5. Classify one explicit `--source` structurally as a package or catalogue. If
   omitted, use the embedded catalogue for Available facts only.
6. Prove lexical and physical disjointness between an external source and target
   workspace before reading it as an available source.

An explicit source never falls back to the embedded catalogue. A malformed,
ambiguous, unavailable, aliased, or overlapping source is invalid, blocked, or
incomplete according to the public boundary; no resemblance or path spelling
chooses a different source.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

A failed selected manifest read retains two identities: the requested package or
catalogue and the exact attempted manifest. Preserve its typed failure reason
through source wrapping and finding construction. Rendering consumes those facts
without parsing native exception messages or performing additional reads. This
internal detail is not a new public JSON field and does not change read-state,
source selection, dependency-conflict handling, early exits or cancellation.

## Ownership Facts

Read installed facts only from `.agents/open-forge.lock.json` in the selected
workspace through the forgiving ownership reader. Select its Extension receipts;
Framework and Library claims do not create installed Extension packages. The
receipt has no persisted content baseline, workspace binding or fingerprint
policy. Old state files are unrelated user content and are never read,
converted, rewritten or deleted.

A complete observation with unambiguous IDs exposes the recorded packages in
ordinal ID order. An empty Extension section means no recorded packages, not
proof that manually copied content is absent. Missing, malformed, unreadable or
ambiguous ownership produces no installed rows, incomplete installed coverage,
and the informational `extension-list.ownership-observation` finding with
`status: complete`. It never raises the aggregate status by itself. The finding
names the lock and explains that installed packages could not be established.
No matching file, source ID or payload inventory recreates ownership.

Installed identities, descriptive versions and path counts remain reportable
when the selected package source is unavailable. Source availability is an
independent observation and keeps its existing finding and status rules.

## Section Projection And Ordering

With no section filter, project Installed followed by Available. With only
`--installed` or only `--available`, project that section. With both, project
both in the stable order. Each package ID appears once per section, while
installed and available facts remain separate records for the same ID.

Installed rows expose stable ID, descriptive version when present, ownership
state, managed-path or dependency counts, source availability, and finite
current facts. Available rows expose manifest package facts, dependency facts,
source identity, and package availability. A list does not write lifecycle state
from an Available row and does not infer installed state from equal IDs.

Order rows deterministically by the command's stable package identity and retain
section order. Filesystem enumeration, catalogue folder names, modification
time, or display recommendation never changes order.

## Coverage And Results

The operation forms one typed result containing exact workspace and source,
requested filters, Installed and Available sections, lifecycle trust and
availability, findings, status, and one next action when needed.

Use the seven shared statuses. `completed` permits unavailable ownership with its informational finding;
`completed-with-warnings` records a complete safe observation such as
source-unavailable installed facts when the command can still answer its requested question;
`incomplete` records safe unavailable source coverage;
`invalid-input` stops on input; `blocked` records unsafe identity, containment,
ownership, or disjointness; `failed` and `cancelled` retain event meaning.
The operation never fabricates zero, empty, trusted, or available facts.

Primary human completed/completed-with-warnings/incomplete results go to stdout. Primary human
invalid-input/blocked/failed/cancelled results go to stderr. JSON emits one complete
result on stdout for every status; bounded diagnostics use stderr.

## Read-Only Safety And Conformance

List reads only the exact workspace lifecycle facts, embedded catalogue or one
explicit source, and bounded package metadata needed for the requested sections.
It does not write payload, generated navigation, lifecycle sections, backups,
temporary files, or diagnostics. It does not invoke install,
update, remove, create, status, or doctor as a public subprocess.

Conformance must demonstrate source and target boundaries, section filters,
deterministic ordering, trust-state preservation, source-unavailable installed
facts, one typed result, all statuses and streams, JSON parity, repeated-read
determinism, no prompts, and no mutation. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, isolated
tests, and package journeys.
