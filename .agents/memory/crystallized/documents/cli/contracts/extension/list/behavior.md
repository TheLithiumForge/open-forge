---
open-forge:
  description: Accepted technology-neutral behavior for deterministic read-only Extension list facts and lifecycle trust states
  responsibility: Define list's source resolution, section projection, coverage, result formation, and non-mutation conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, List, Behavior, ReadOnly, Determinism, Lifecycle, CurrentTruth]
---

# extension list Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for read-only
`open-forge extension list`. It defines deterministic request resolution, exact
workspace and source handling, lifecycle-section trust classification, Installed
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
  -> installed lifecycle facts and available package facts
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

## Lifecycle Facts

Read installed facts from `.agents/open-forge.lifecycle.json`, schema v1, in the
selected workspace. Treat `framework` and `extensions` as separate lifecycle
sections for their own facts, not as separate authorities. Keep safe facts from
one section when the unrelated section is malformed, but expose the section
trust and coverage. The document stores no
plan, runtime history, journal, recovery evidence, or session. Files outside this
exact path are ordinary workspace content, not lifecycle input.

Classify a section as safely absent only after complete inspection proves no
expected managed state, managed boundary, or recovery residual. An absent
document or section is not, by itself, proof of an empty installed set. Classify it as
trusted only when exact workspace/package/path/owner/dependency identities,
supported versions and fingerprint policy, reciprocal facts, duplicate-free
IDs and paths, and complete verifiable coverage hold. Classify malformed,
unsupported, unverifiable, or inconsistent evidence as untrusted/incomplete or
blocked when ambiguity is unsafe. A matching path, bytes, fingerprint, source,
or force flag never promotes evidence.

Installed facts remain reportable when the package source is missing. Source
unavailability makes current-source comparison and available facts unavailable;
it does not erase the installed ID, recorded version, trust state, ownership,
or baseline facts that can be read safely.

## Section Projection And Ordering

With no section filter, project Installed followed by Available. With only
`--installed` or only `--available`, project that section. With both, project
both in the stable order. Each package ID appears once per section, while
installed and available facts remain separate records for the same ID.

Installed rows expose stable ID, descriptive version when present, lifecycle
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

Use the seven shared statuses. `complete` requires complete requested coverage;
`attention` records a complete safe observation such as source-unavailable
installed facts when the command can still answer its requested question;
`incomplete` records safe unavailable source or lifecycle coverage;
`invalid` stops on input; `blocked` records unsafe identity, containment,
ownership, or disjointness; `failed` and `interrupted` retain event meaning.
The operation never fabricates zero, empty, trusted, or available facts.

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. JSON emits one complete
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
