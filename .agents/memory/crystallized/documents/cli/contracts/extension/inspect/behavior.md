---
open-forge:
  description: Accepted technology-neutral behavior for read-only Extension identity inspection and source-unavailable comparisons
  responsibility: Define inspect's deterministic ID/source resolution, ownership coverage, comparison facts, result formation, and non-mutation
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Behavior, ReadOnly, Determinism, Lifecycle, CurrentTruth]
---

# extension inspect Behavior Contract

## Status And Boundary

This is the current Crystallized Behavior Contract for read-only
`open-forge extension inspect`. It defines how one already parsed request moves
through exact workspace and source boundaries, lifecycle and package facts,
dependency and path coverage, fingerprints, comparison, findings, status, and
`next` formation. The [Interface Contract](interface.md) owns the wire graph,
finite values, finding meanings, golden vectors, and public examples. The [Shared
Result Coordinates](../../shared/result-coordinates/interface.md) own the shared
envelope, source-location primitive, and process exits. The [CLI
Architecture](../../../architecture.md) owns parser and concrete serialization
boundaries. This document claims neither implementation nor Gate-5 evidence.

Inspect is a read operation. It never installs, updates, removes, creates,
repairs, adopts, indexes, locks, downloads, executes package content, writes an
ownership lock, or turns an observation or recommendation into authority.
The operation forms one typed result and stops before presentation; human and
JSON renderers consume that same result without rereading the workspace or
source.

## Behavioral Invariants

The following invariants apply at every stage:

- The supplied stable ID remains the only subject. No path, folder spelling,
  version, proximity, cache, registry, network, glob, resemblance, or fallback
  supplies a different identity.
- An explicit source remains the only source for that invocation. A missing,
  malformed, inaccessible, overlapping, or ambiguous explicit source is
  represented by its typed boundary; the embedded catalogue is never a fallback.
- Every fact is kept separate from the confidence or coverage of that fact.
  Safe facts established before a later boundary remain visible. Facts that
  were not reached are `not-started`; facts that were reached but unavailable
  have their defined unavailable, invalid-input, blocked, failed, or cancelled
  state.
- A known empty collection is represented by an empty array and a count of `0`.
  A collection whose coverage is not established is still an empty array, but
  its count is `null`. No array is ever `null`.
- Filesystem, source, dependency, owner, and finding ordering is derived from
  defined ordinal keys, never enumeration or exception order.
- A path, matching bytes, matching fingerprint, package ID, or force-like input
  never creates ownership, trust, or mutation authority.

## Typed Flow And Stage Stops

The operation follows one monotonic typed flow:

```text
parsed invocation
  -> request validation
  -> exact workspace selection
  -> exact source classification and disjointness
  -> subject resolution
  -> ownership read and coverage classification
  -> package and dependency facts
  -> declared/current path facts
  -> operation-time fingerprints and generated-region facts
  -> current/intended comparison
  -> findings, aggregate status, and zero-or-one next action
  -> one human or JSON presentation
```

Terminal help and version are resolved by the shared CLI pipeline before
domain work. Invalid request input stops domain stages after the typed invalid
subject/request facts are formed. A blocked workspace or source boundary stops
the affected and later stages. A failed or cancelled event stops the
operation at the observed point. In every stop case, the result retains safe
facts formed before the stop and emits every command-local member in the
Interface grammar, including empty arrays and `not-started` states.

The operation is invoked at most once. Rendering never reopens files, retries a
source, recalculates a hash, changes a finding, or changes the selected
status. Cancellation is observed at each read boundary and maps to the
`cancelled` event without writing anything.

## Request, Workspace, And Source Resolution

1. Apply shared terminal-mode and parser rules. Require exactly one positional
   stable ID for domain execution. Missing, extra, malformed, or conflicting
   input produces `invalid-input` or `invalid-stable-id` with no inferred
   subject.
2. Select exactly the current directory or the exact `--workspace` directory.
   Record the shared envelope workspace selection. Do not discover an ancestor,
   child, repository, or alternate root. A missing, non-directory, inaccessible,
   or unprovable workspace is `workspace-unavailable`; an unsafe physical or
   containment identity is `workspace-unsafe`.
3. If `--source` is omitted, select the embedded catalogue as the available
   source universe. If it is supplied, classify that one exact location as one
   package or one catalogue. A source is not classified by its folder name,
   source order, or likely intent.
4. Before reading an explicit source, prove lexical and physical disjointness
   from the selected workspace in both containment directions. Resolve aliases
   and links component by component under the shared filesystem boundary. Any
   lexical or physical overlap is `source-overlap` and blocked. An ambiguous
   package/catalogue shape is `source-ambiguous` and blocked; a missing or
   inaccessible location is `source-unavailable`; malformed source structure is
   `source-invalid`.
5. Resolve the supplied ID exactly in installed ownership facts and the one
   selected source universe. Retain every safe candidate in ID/path order when
   duplicate active identities or an unresolved collision exists. Never choose a
   candidate by package directory, version, source order, or proximity.

An explicit source is not replaced when it is missing or invalid. Installed
facts can still be projected independently. When those installed facts are
complete, source unavailability is the non-blocking `completed-with-warnings` observation
defined by the Interface. If a required package, lifecycle, dependency, path,
or semantic comparison fact is also unavailable, its `incomplete` finding
raises the aggregate result and selects the incomplete `next` action.

The embedded Framework and first-party Extension assets are a deterministic
distribution fact. Reading that embedded inventory establishes only the source
asset identity; it does not establish a workspace installation, lifecycle
ownership, or runtime implementation.

## Ownership Observation

Read only `.agents/open-forge.lock.json` through the forgiving ownership reader.
Select Extension receipts with unambiguous stable IDs, safe destination paths
and interpretable dependencies. A complete empty section means no recorded
packages; matching files never establish ownership. No persisted fingerprints,
workspace binding, source version gate or integrity policy participates.

Missing, malformed, unreadable, duplicate, cyclic, incomplete-dependency or
unsafe-path claims yield no installed selection and one informational
`ownership-observation` finding with severity `info`. Preserve the raw read
state where available and incomplete ownership coverage. Continue source and
available-only inspection; the ownership observation never raises aggregate
status or authorizes adoption. The lock records no absolute workspace binding.

Old state files are unrelated user content. Inspect never reads, converts,
normalizes, repairs, rewrites or deletes them. Current source failures retain
their independent statuses and never cause a fallback source selection.

## Package Facts And Dependency Closure

When the selected source is readable, inspect its exact package or catalogue
manifest and payload inventory. Package metadata retains the exact ID, name,
description, descriptive version, manifest path, declared dependency order, and
each payload's package-relative path, target path, read state, byte length, and
lowercase SHA-256 when bytes are available. Unsupported or unknown manifest
members, malformed UTF-8/JSON, invalid field types, duplicate dependencies,
unsafe target paths, or an unreadable required payload produce the specific
package or path finding; no value is guessed.

Resolve the requested package's dependency closure offline and only inside the
selected source universe. Traverse declared edges transitively, retain direct
declaration positions, reject unknown IDs and duplicate active identities, and
detect cycles before claiming a complete closure. A dependency declaration or
node that can be retained safely remains visible when a later node is
unavailable. `dependencies.order` is dependency-first; independent nodes and
equal-depth ties are ordered by ordinal stable ID. Include the requested package
once in `resolved` and `order` when its selected facts are available. A cycle,
conflicting source identity, duplicate declaration, or irreconcilable closure
is blocked. An unreadable but non-ambiguous closure is incomplete.

Installed dependency facts are read from the ownership receipts and remain
separate from available source dependencies. Equal IDs do not merge versions,
sources or owners. A source package never causes another source
universe to be searched for a missing dependency.

## Declared, Current, And Ownership Facts

Project each declared package path as a target-relative path with its exact
source-relative path when known. Reject traversal, absolute, reserved, malformed,
or out-of-bound targets with `path-invalid` before treating them as managed
paths. Read current workspace paths only after the shared physical containment
resolver proves each component. Record physical identity separately from the
lexical path, and capture byte length and the fresh exact-byte SHA-256 only when
the current bytes are readable.

Current facts are observations, not adoption. A present unowned file does not
become owned because its bytes match the selected payload. Owner sets are copied from
validated ownership and source facts for each comparison side, de-duplicated,
and ordinally ordered. Conflicting or ambiguous owner sets block comparison
with `ownership-conflict`; Inspect does not choose an owner or synthesize a
shared ownership relation.

Declared and current path arrays are ordered by canonical `/`-separated path
using ordinal comparison. A missing current path retains the declared or
recorded path identity and has `state: "missing"`; it is not represented by a
fabricated empty file or hash. A path read failure is `path-unavailable`, while
an unsafe path identity is `path-invalid` or `workspace-unsafe` as applicable.

## `open-forge-markdown-v1` Fingerprint Application

The [Interface fingerprint policy](interface.md#fingerprint-policy-open-forge-markdown-v1)
is the sole byte-level authority. This section defines the behavior-stage
application of that immutable policy; it does not create a second algorithm or
an implementation choice. The policy string is the version. Any change to
admission, UTF-8/BOM/NUL handling, parser validation, line endings, generated
regions, omitted bytes, hash, encoding, or fallback requires a new policy value.

Apply the policy in this order:

1. Start with the exact original bytes and do not pre-normalize them. Only a
   supported Markdown package path under `.agents/` can receive semantic
   treatment. External destinations use exact SHA-256 identities directly,
   including Markdown and marker-like bytes, without fallback findings or
   generated-region observations.
2. Decode with strict UTF-8. Retain a leading `EF BB BF` as U+FEFF at its
   original position; BOM presence and absence remain distinct. Any NUL is a
   binary boundary. Unsupported kind, binary/NUL input, invalid UTF-8,
   unparseable Markdown, or an invalid generated boundary uses exact-byte
   fallback: hash the original bytes exactly, with no line-ending, BOM,
   generated-interior, or final-newline change.
3. Validate the decoded, NUL-free Markdown with the one fixed CommonMark parser
   configuration supplied by Architecture. Parser extensions, permissive
   decoding, rendering, YAML reserialization, and parser-native spans are not
   fingerprint inputs. Replace CRLF and lone CR with LF after validation; this
   is the only ordinary normalization. Preserve Unicode scalars and
   normalization, whitespace, frontmatter, headings, tags, links and
   destinations, code, and final-newline choice.
4. Recognize a generated region from one top-level canonical `## Entries` ATX
   heading through the shared Markdown semantic-section reader. Accept initial
   BOM and trailing horizontal heading whitespace. Code, nested and noncanonical
   lookalikes are not semantic headings. Duplicate sections yield invalid exact
   fallback; absent sections omit nothing.
5. For one valid section, omit its complete heading-owned body up to the next
   top-level same-or-higher heading or EOF after line-ending normalization. The
   normalized UTF-8 positions are `startByteOffset` and `endByteOffset`, with
   `excludedInteriorByteLength` their nonnegative difference. Preserve the heading
   and all outside bytes. Old guards inside the body have no boundary meaning.
   An absent record has null offsets and excluded length 0; invalid, ambiguous,
   unavailable and not-started records have null offsets and excluded length.
   Encode UTF-8 without adding a preamble, retaining admitted U+FEFF content, and
   hash with SHA-256 rendered as 64 lowercase hexadecimal characters.
6. An admitted document is `semantic` under v1; fallback is `exact-bytes`
   under the selected policy when reported by Inspect. Unavailable has no
   identity. Fingerprints are equivalent only when policy, kind, and hash all
   match. Exact-byte equality proves bytes for that operation only, never
   semantic equivalence, ownership, or mutation safety.
7. Current and intended fingerprints, including fallback hashes, are computed
   during this invocation. Inspect never persists them or obtains mutation
   authority from equality. Internal kind and policy facts preserve the existing
   semantic/fallback boundary; the public fingerprint contains only `sha256`.

## Comparison Formation

Select the comparison mode from the established package identities:

- `none`: neither package identity is established;
- `installed-only`: retain receipt membership and current path observations;
  without an intended source, path relations are `unknown`;
- `available-only`: retain source facts without inventing installed ownership;
  path relations are `not-applicable`; and
- `installed-and-available`: compare the current workspace with the selected
  package. The name describes the two package sources and remains unchanged.

There are two computed sides: current bytes from this invocation's workspace
reads and intended bytes from the selected package. Never substitute ownership
receipts for bytes. Dependency comparison compares recorded installed identity
sets against the selected source's dependency closure. Current owner sets come
from receipts; intended owner sets come from package declarations.

For installed-and-available paths, `new` means declared without recorded
ownership; `retired` means owned but no longer declared by the selected payload.
An unreadable declared payload is not retired. An observed missing owned path
is `missing`; unavailable, invalid and blocked current reads retain those
relations. Required missing fingerprints yield `unknown`, without fabrication.
With comparable current and intended hashes, equality is `unchanged` and a
difference is `changed`. Compatible multiple owners may refine `unchanged` to
`shared`. There is no third side or `current-diverged` relation.

Keep canonical ordinal path order and duplicate-free owner and dependency sets.
External destinations retain exact-byte comparisons and the existing
semantic-only recommendation boundary. Internal Markdown fallback never
qualifies for an actionable semantic update recommendation. The accepted
Markdown policy and its golden vectors remain unchanged.

## Findings, Status, And `next`

Each observed issue is translated to exactly one Interface finding code. The
code's fixed status is not inferred from a neighboring fact and cannot be
downgraded or upgraded. Findings are emitted in the Interface's global code
order, then its within-code ordinal tie-breaks. Candidate arrays are sorted by
ID and canonical path; locations use byte offset, line, and column with a null
location last. An equal code and tie-break key is a duplicate finding and is a
result-formation error. An unknown code, unknown status, or code/status mismatch
also fails closed: discard the invalid finding representation, retain safe
facts, emit `extension-inspect.operation-failed`, and never serialize an
unknown finite value.

Finding formation does not erase safe facts. A source-unavailable finding can
coexist with a complete installed projection; a package-unavailable finding
can coexist with a safely read ownership lock; a path-unavailable finding
can coexist with other current paths; and a parser or generated-boundary
finding retains exact bytes and the fallback identity when readable. Unsafe
identity, overlap, ownership, dependency-cycle, or path boundaries stop the
affected classification rather than inventing an empty or trusted value.

Aggregate status uses the Interface total precedence:

```text
cancelled > failed > invalid-input > blocked > incomplete > completed-with-warnings > completed
```

Terminal validation prevents invalid input from being combined with domain
findings in ordinary execution. Otherwise the highest-precedence fixed finding
status wins. With no findings and complete applicable coverage, status is
`completed`. Event statuses retain their event meaning even when earlier facts
remain visible.

Form at most one top-level `next` action after aggregate status. Use the exact
Interface table. The only update recommendation is the executable action formed
from the template `open-forge extension update {subject.id}`, and it requires a
resolved ID, an available explicit or embedded source package, complete ownership observation, complete dependencies/paths/generated facts,
and an `installed-and-available` comparison with every side available, with semantic v1
fingerprints for every actionable path, and at least one trusted path or
dependency change. Any missing or retired path,
ownership/dependency conflict,
invalid or unavailable fact, exact-byte fallback, or any other mode removes
that recommendation. Non-actionable `completed-with-warnings` has `next: null`.

The action is text only. Inspect does not invoke `doctor`, `update`, or any
other command, does not reconstruct original arguments, and does not invent a
source to make an action available. Incomplete, blocked, invalid-input, failed, and
cancelled statuses use only their exact Interface action values; completed
uses `null`.

## Result Formation And Presentation

Construct the complete `InspectResult` object in the Interface order:

```text
subject -> source -> lifecycle -> installed -> available -> dependencies
  -> pathFacts -> comparison -> generated -> findings -> counts
```

Every object member is present. Arrays, including nested candidate, dependency,
path, fingerprint, owner, region, and finding arrays, are never null. A known
empty array has count `0`; unknown coverage leaves the array empty and the
corresponding count `null`. Null scalar facts mean only the nullability stated
by the Interface: not established, not applicable, or unavailable. Do not
replace a null with an empty string, zero, false, or a fabricated identity.

The shared envelope contains `schemaVersion: 3`, the exact command identity,
aggregate status, shared workspace, `detail`, `filter`, this data object,
findings, effects, counts, limitations, recovery facts, and one `next` object or
`null`, in [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) order. Envelope `status`, `workspace`, `command`, and
`next` are not duplicated in the command-local data. JSON emits one schema-3
envelope on stdout for every status. Human completed, completed-with-warnings,
and incomplete primary output uses stdout; invalid-input, blocked, failed, and
cancelled primary output uses stderr. Diagnostics are bounded and remain on
stderr.

Human minimal, standard, full, and debug output are projections only. They preserve the
subject, source, ownership trust/coverage, package/dependency/path state,
comparison, generated-navigation ownership, findings, counts, aggregate status,
and at most one exact next line. Human output may group matching observations by
exact path identity while preserving distinct and unmatched facts. It does not
recompute a data member or change operation/JSON ordering. `--detail` and
`--detail-filter` do not alter JSON data.

## Scenario Behavior Matrix

| Scenario                                                                  | Processing and retained facts                                                                                         | Result and finding                                                                  | `next`                                              |
| ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | --------------------------------------------------- |
| Ownership plus complete source, current and intended differ semantically  | Read the two computed sides; retain owners, path facts, and dependency closure                                        | `completed-with-warnings`; `path-changed` and/or `dependency-changed`               | Update only when the complete actionable gate holds |
| Trusted installed lifecycle plus missing explicit source                  | Preserve installed package, current bytes, and generated observation; mark source missing and intended not applicable | `completed-with-warnings`; `source-unavailable`                                     | `null`                                              |
| Available source plus missing ownership                                   | Preserve source package and payload; do not call installed state absent                                               | `completed`; `ownership-observation`                                                | `null` when ownership is the only observation       |
| Source contains no requested package                                      | Preserve source identity and installed facts if readable; do not choose a nearby package                              | `incomplete`; `package-unavailable`                                                 | Exact doctor action                                 |
| Malformed package or partial closure                                      | Preserve validated metadata/nodes; stop unsafe or unavailable portions                                                | `incomplete`; `package-invalid` or `dependency-incomplete`                          | Exact doctor action                                 |
| Duplicate identity, cycle, unsafe path, source overlap, or owner conflict | Preserve candidates and prior safe facts; do not select an unsafe node                                                | `blocked`; corresponding fixed finding                                              | Exact doctor action                                 |
| Valid Markdown with a malformed generated boundary                        | Preserve original bytes and exact-byte fallback; no generated exclusion                                               | `incomplete`; `generated-boundary-invalid` and `fingerprint-fallback` as applicable | Exact doctor action                                 |
| Unsupported, binary, invalid-UTF-8, or unparseable payload                | Preserve readable path and exact original bytes; semantic equivalence is unavailable                                  | `incomplete`; `fingerprint-fallback` or `fingerprint-unavailable`                   | Exact doctor action                                 |
| Invalid request                                                           | Keep supplied operand when safe; later domain states are `not-started` and arrays empty                               | `invalid-input`; `invalid-input` or `invalid-stable-id`                             | Exact inspect-help action                           |
| Unexpected failure                                                        | Keep facts formed before failure; do not convert failure to unavailable success                                       | `failed`; `operation-failed`                                                        | Exact retry action                                  |
| Cancellation                                                              | Keep facts formed before cancellation; stop all later work                                                            | `cancelled`; `cancelled`                                                            | Exact same-request retry action                     |

These scenarios are behavioral mappings; the Interface's exact field grammar,
status table, finding table, action strings, and structured examples are the
public wire authority.

## Effects, Safety, And Conformance

Inspect reads only the selected workspace ownership lock, selected package
or embedded source facts, bounded package payload bytes, and current target
paths needed for the requested result. It does not write payloads, generated
`Entries`, lifecycle state, locks, backups, recovery files, indexes, caches,
temporary artifacts, or diagnostics. It does not call another public command,
follow package code, access network/registry/cache, or inspect a legacy receipt.

Conformance must prove, using the same typed result for human and JSON:

- exact operand, global-flag, terminal-mode, workspace, and source resolution;
- lexical and physical source/workspace disjointness, alias handling, and no
  explicit-source fallback;
- forgiving lock reads, informational unavailable ownership, receipt membership,
  safe path/dependency interpretation, and ignored leftover files;
- package metadata and payload inventory, one-source offline dependency closure,
  duplicate/cycle/conflict rejection, target-path safety, current physical
  identities, and deterministic ordering;
- every `open-forge-markdown-v1` admission, parser, BOM/NUL/UTF-8, line-ending,
  generated-region, fallback, equivalence, persistence, and golden-vector rule;
- exact current/intended relation formation, finding table and
  tie-break order, duplicate/unknown-value rejection, total status precedence,
  safe retained facts, actionable update gate, and exhaustive `next` mapping;
- all seven statuses, stream and process-exit coordinates, one JSON document per
  status, bounded diagnostics, detail-level parity, no prompts, repeat
  determinism, and recursive unchanged-state/no-write snapshots; and
- source-generated serialization, real filesystem, Native AOT, process, and
  package-journey evidence at Gate 5, without treating this prose or its
  examples as proof.

The shared Architecture and Interface contracts remain the authority for their
respective boundaries. No implementation may widen this behavior by silently
adding a source, fallback, parser extension, lifecycle writer, ownership rule,
or mutation effect.
