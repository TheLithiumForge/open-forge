---
open-forge:
  description: Accepted technology-neutral behavior for read-only Extension identity inspection and source-unavailable comparisons
  responsibility: Define inspect's deterministic ID/source resolution, lifecycle trust, comparison facts, result formation, and non-mutation
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
repairs, adopts, indexes, locks, downloads, executes package content, writes a
lifecycle record, or turns an observation or recommendation into authority.
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
  have their defined unavailable, invalid, blocked, failed, or interrupted
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
  -> lifecycle read and trust classification
  -> package and dependency facts
  -> declared/current path facts
  -> operation-time fingerprints and generated-region facts
  -> baseline/current/intended comparison
  -> findings, aggregate status, and zero-or-one next action
  -> one human or JSON presentation
```

Terminal help and version are resolved by the shared CLI pipeline before
domain work. Invalid request input stops domain stages after the typed invalid
subject/request facts are formed. A blocked workspace or source boundary stops
the affected and later stages. A failed or interrupted event stops the
operation at the observed point. In every stop case, the result retains safe
facts formed before the stop and emits every command-local member in the
Interface grammar, including empty arrays and `not-started` states.

The operation is invoked at most once. Rendering never reopens files, retries a
source, recalculates a hash, changes a finding, or changes the selected
status. Cancellation is observed at each read boundary and maps to the
`interrupted` event without writing anything.

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
5. Resolve the supplied ID exactly in installed lifecycle facts and the one
   selected source universe. Retain every safe candidate in ID/path order when
   duplicate active identities or an unresolved collision exists. Never choose a
   candidate by package directory, version, source order, or proximity.

An explicit source is not replaced when it is missing or invalid. Installed
facts can still be projected independently. When those installed facts are
complete, source unavailability is the non-blocking `attention` observation
defined by the Interface. If a required package, lifecycle, dependency, path,
or semantic comparison fact is also unavailable, its `incomplete` finding
raises the aggregate result and selects the incomplete `next` action.

The embedded Framework and first-party Extension assets are a deterministic
distribution fact. Reading that embedded inventory establishes only the source
asset identity; it does not establish a workspace installation, lifecycle
ownership, or runtime implementation.

## Lifecycle Read And Trust

Read only the selected workspace's exact
`.agents/open-forge.lifecycle.json`. The document is schema v1. Validate its
common envelope, exact normalized workspace binding, fingerprint policy, and
typed `extensions` section. The `framework` value is an unrelated opaque JSON
value: it is not merged into Extension facts, and its future meaning is not
invented here. Files with other names or legacy formats are ordinary workspace
content and are not lifecycle inputs.

Validate the Extension section as a complete, ordered, reciprocal graph:

- package IDs are exact stable IDs and are duplicate-free;
- package versions may be `null`, while sources, dependencies, and paths retain
  their typed nullable or array rules;
- dependency IDs are distinct and reciprocal facts are consistent;
- target-relative path keys are safe, canonical, duplicate-free, and within
  the selected workspace boundary;
- each path's owner set is non-empty, distinct, and agrees with the package
  records; and
- each path has a baseline fingerprint and `fingerprintKind` of `semantic` or
  `exact-bytes`, with the lifecycle policy read from the common envelope.

Trust is classified only after all facts needed for that classification are
checked. A complete, consistent non-empty Extension section is `trusted`; a
complete empty section is `absent`. A missing document or missing Extension
section is not proof of an empty installed set: it is missing/incomplete
coverage and the installed projection is `unavailable`. Malformed values,
unknown or duplicate members, nonreciprocal records, unsupported policy, and
incomplete reads retain safe identities only when doing so cannot create an
unsafe ambiguity. Otherwise the lifecycle boundary is `blocked`.

`lifecycle.readState`, `trust`, `coverage`, and `workspaceBinding` remain
independent facts. For example, a readable document with a mismatching
workspace is not promoted to trusted merely because its package IDs are valid.
The operation never repairs, rewrites, sorts, normalizes, or migrates the
lifecycle document.

An authored schema-v1 path record with `fingerprintKind: "exact-bytes"` is
reported exactly as lifecycle evidence. Inspect does not relabel it, manufacture
another baseline, decide whether it is safe to mutate, or resolve the separate
Mutation Foundation writer boundary. Only a persisted `semantic` baseline under
`open-forge-markdown-v1` can participate in a semantic persisted three-way
claim. This read-only behavior leaves any accepted reader/writer compatibility
decision for that mutation boundary.

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

Installed dependency facts are read from the lifecycle records and remain
separate from available source dependencies. Equal IDs do not merge versions,
sources, owners, or baselines. A source package never causes another source
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
become owned because its bytes match a baseline. Owner sets are copied from
validated lifecycle and source facts for each comparison side, de-duplicated,
and ordinally ordered. Conflicting or ambiguous owner sets block comparison
with `ownership-conflict`; Inspect does not choose an owner or synthesize a
shared ownership relation.

Declared and current path arrays are ordered by canonical `/`-separated path
using ordinal comparison. A missing current path retains the declared or
baseline path identity and has `state: "missing"`; it is not represented by a
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
4. Recognize a generated region only when the parsed document has exactly one
   canonical outside-code `## Entries` ATX heading and it is the final heading
   section, exactly one pair of standalone unindented marker lines in that
   section in start-then-end order,
   and the end marker is the final non-empty line (apart from its line ending):

   ```text
   <!-- open-forge:generated-index:start -->
   <!-- open-forge:generated-index:end -->
   ```

   Marker text in code is not a marker. A marker line has no extra bytes other
   than its line ending; the final marker may end at EOF without one. Duplicate,
   reversed, nested, malformed, misplaced,
   or ambiguous markers are invalid; no likely pair is selected. No outside-
   code marker means an absent region and semantic hashing of the complete
   line-ending-normalized document.

5. For one valid pair, omit only
   `[end-of-start-marker-line, start-of-end-marker-line)` after line-ending
   normalization. These positions are the normalized UTF-8
   `startByteOffset` and `endByteOffset`; `excludedInteriorByteLength` is their
   nonnegative difference. A valid record sets `markerLinesRetained: true`.
   Retain both complete marker lines, all bytes before and after them allowed
   by the final-line rule, and all other authored bytes. An absent record uses
   null offsets, excluded length `0`, and `markerLinesRetained: false`;
   `invalid`, `ambiguous`, `unavailable`, and `not-started` records use null
   offsets, null excluded length, and `markerLinesRetained: false`.
   Re-encode as UTF-8 without adding a preamble; an admitted U+FEFF remains at
   its original position. Hash with SHA-256 and emit exactly 64 lowercase hex
   characters.
6. An admitted document is `semantic` under v1; fallback is `exact-bytes`
   under the selected policy when reported by Inspect. Unavailable has no
   identity. Fingerprints are equivalent only when policy, kind, and hash all
   match. Exact-byte equality proves bytes for that operation only, never
   semantic equivalence, ownership, or mutation safety.
7. Read persisted lifecycle baselines exactly. Only an accepted semantic v1
   baseline supports a semantic persisted three-way claim. Current and
   intended fingerprints, including fallback hashes, are operation-time facts;
   Inspect never persists them or grants mutation authority. An authored
   schema-v1 exact-byte record remains visible without being relabeled or
   resolved into writer behavior.

## Comparison Formation

Form comparison only from the facts each mode permits:

- `none` means no comparison side has been established;
- `installed-only` retains lifecycle baseline and any current installed-path
  observations without an intended source projection;
- `available-only` reports source package facts without a managed baseline; and
- `three-way` requires trusted lifecycle baseline facts, current workspace
  facts, and complete intended source facts.

The baseline side comes only from lifecycle records. The current side comes
only from current workspace reads during this invocation. The intended side
comes only from the selected source package and its dependency/path projection.
Never substitute an available source for a missing baseline or a current file
for intended content.

For each canonical path, compare identities and owners in this order:

1. Establish whether baseline, current, and intended path records exist and
   whether their required fingerprints are available and equivalent.
2. Reject conflicting owners or unsafe path identities before declaring a
   relation.
3. Emit `unchanged` only when all three applicable identities are equal;
   `changed` only when baseline equals current and intended differs;
   `current-diverged` when current differs from baseline; `missing` when an
   expected current path is absent; `new` when an intended path has no baseline;
   `retired` when an earlier managed path is no longer intended; `shared` when
   compatible multiple owners are established; and `unknown` when the available
   identities cannot prove a relation.
4. Preserve `not-started`, `not-applicable`, `unavailable`, `invalid`, and
   `blocked` relation states when the corresponding comparison boundary does
   not permit classification.

Baseline/current/intended path and dependency arrays are canonical and
duplicate-free. Dependency comparison uses ordered identity sets from each
side; equal complete sets are `equal`, a complete difference is `changed`, and
unavailable, invalid, or blocked sides retain that relation state. A formatting
or line-ending difference with equal admitted semantic fingerprints is not a
divergence. An exact-byte fallback never qualifies for an actionable semantic
three-way recommendation.

For external destinations, persisted and operation-time exact-byte identities
support these same path relations when their policy, kind and hash are
comparable. This reports byte equality or difference only. It grants no
semantic equivalence or mutation authority and does not widen the existing
semantic-only actionable recommendation gate. Internal Markdown fallback
retains its existing comparison limitations.

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
can coexist with a safely read lifecycle record; a path-unavailable finding
can coexist with other current paths; and a parser or generated-boundary
finding retains exact bytes and the fallback identity when readable. Unsafe
identity, overlap, ownership, dependency-cycle, or path boundaries stop the
affected classification rather than inventing an empty or trusted value.

Aggregate status uses the Interface total precedence:

```text
interrupted > failed > invalid > blocked > incomplete > attention > complete
```

Terminal validation prevents invalid input from being combined with domain
findings in ordinary execution. Otherwise the highest-precedence fixed finding
status wins. With no findings and complete applicable coverage, status is
`complete`. Event statuses retain their event meaning even when earlier facts
remain visible.

Form at most one top-level `next` action after aggregate status. Use the exact
Interface table. The only update recommendation is the executable action formed
from the template `open-forge extension update {subject.id}`, and it requires a
resolved ID, an available explicit or embedded source package, complete trusted
lifecycle and workspace binding, complete dependencies/paths/generated facts,
and a `three-way` comparison with all three sides available, with semantic v1
fingerprints for every actionable path, and at least one trusted path or
dependency change. Any current divergence, missing or retired path,
ownership/dependency conflict,
invalid or unavailable fact, exact-byte fallback, or non-three-way mode removes
that recommendation. Non-actionable `attention` has `next: null`.

The action is text only. Inspect does not invoke `doctor`, `update`, or any
other command, does not reconstruct original arguments, and does not invent a
source to make an action available. Incomplete, blocked, invalid, failed, and
interrupted statuses use only their exact Interface action values; complete
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

The shared envelope contains `schemaVersion: 1`, the exact command identity,
aggregate status, shared workspace, this result object, and one `next` object or
`null`, in [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) order. Envelope `status`, `workspace`, `command`, and
`next` are not duplicated in the command-local result. JSON emits one complete
envelope on stdout for every status. Human complete, attention, and incomplete
primary output uses stdout; invalid, blocked, failed, and interrupted primary
output uses stderr. Diagnostics are bounded and remain on stderr.

Human compact and expanded output are projections only. They preserve the
subject, source, lifecycle trust/coverage, package/dependency/path state,
comparison, generated-navigation ownership, findings, counts, aggregate status,
and at most one exact next line. `--view` does not alter JSON. No renderer
reorders facts or recomputes any result member.

## Scenario Behavior Matrix

| Scenario                                                                                          | Processing and retained facts                                                                                                   | Result and finding                                                                  | `next`                                              |
| ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | --------------------------------------------------- |
| Trusted lifecycle plus complete source, baseline equals current and intended differs semantically | Read all three sides; retain owners, path facts, and dependency closure                                                         | `attention`; `path-changed` and/or `dependency-changed`                             | Update only when the complete actionable gate holds |
| Trusted installed lifecycle plus missing explicit source                                          | Preserve installed package, baseline, current bytes, and generated observation; mark source missing and intended not applicable | `attention`; `source-unavailable`                                                   | `null`                                              |
| Available source plus missing lifecycle document/section                                          | Preserve source package and payload; do not call installed state absent                                                         | `incomplete`; `lifecycle-unavailable`                                               | Exact doctor action                                 |
| Source contains no requested package                                                              | Preserve source identity and installed facts if readable; do not choose a nearby package                                        | `incomplete`; `package-unavailable`                                                 | Exact doctor action                                 |
| Malformed package or partial closure                                                              | Preserve validated metadata/nodes; stop unsafe or unavailable portions                                                          | `incomplete`; `package-invalid` or `dependency-incomplete`                          | Exact doctor action                                 |
| Duplicate identity, cycle, unsafe path, source overlap, or owner conflict                         | Preserve candidates and prior safe facts; do not select an unsafe node                                                          | `blocked`; corresponding fixed finding                                              | Exact doctor action                                 |
| Valid Markdown with a malformed generated boundary                                                | Preserve original bytes and exact-byte fallback; no generated exclusion                                                         | `incomplete`; `generated-boundary-invalid` and `fingerprint-fallback` as applicable | Exact doctor action                                 |
| Unsupported, binary, invalid-UTF-8, or unparseable payload                                        | Preserve readable path and exact original bytes; semantic equivalence is unavailable                                            | `incomplete`; `fingerprint-fallback` or `fingerprint-unavailable`                   | Exact doctor action                                 |
| Invalid request                                                                                   | Keep supplied operand when safe; later domain states are `not-started` and arrays empty                                         | `invalid`; `invalid-input` or `invalid-stable-id`                                   | Exact inspect-help action                           |
| Unexpected failure                                                                                | Keep facts formed before failure; do not convert failure to unavailable success                                                 | `failed`; `operation-failed`                                                        | Exact verbose retry action                          |
| Cancellation                                                                                      | Keep facts formed before cancellation; stop all later work                                                                      | `interrupted`; `interrupted`                                                        | Exact same-request retry action                     |

These scenarios are behavioral mappings; the Interface's exact field grammar,
status table, finding table, action strings, and structured examples are the
public wire authority.

## Effects, Safety, And Conformance

Inspect reads only the selected workspace lifecycle document, selected package
or embedded source facts, bounded package payload bytes, and current target
paths needed for the requested result. It does not write payloads, generated
`Entries`, lifecycle state, locks, backups, recovery files, indexes, caches,
temporary artifacts, or diagnostics. It does not call another public command,
follow package code, access network/registry/cache, or inspect a legacy receipt.

Conformance must prove, using the same typed result for human and JSON:

- exact operand, global-flag, terminal-mode, workspace, and source resolution;
- lexical and physical source/workspace disjointness, alias handling, and no
  explicit-source fallback;
- strict schema-v1 lifecycle parsing, workspace binding, trust/coverage,
  reciprocal ownership/dependency/path validation, authored fingerprint-kind
  retention, and safe partial fact retention;
- package metadata and payload inventory, one-source offline dependency closure,
  duplicate/cycle/conflict rejection, target-path safety, current physical
  identities, and deterministic ordering;
- every `open-forge-markdown-v1` admission, parser, BOM/NUL/UTF-8, line-ending,
  generated-region, fallback, equivalence, persistence, and golden-vector rule;
- exact baseline/current/intended relation formation, finding table and
  tie-break order, duplicate/unknown-value rejection, total status precedence,
  safe retained facts, actionable update gate, and exhaustive `next` mapping;
- all seven statuses, stream and process-exit coordinates, one JSON document per
  status, bounded diagnostics, compact/expanded parity, no prompts, repeat
  determinism, and recursive unchanged-state/no-write snapshots; and
- source-generated serialization, real filesystem, Native AOT, process, and
  package-journey evidence at Gate 5, without treating this prose or its
  examples as proof.

The shared Architecture and Interface contracts remain the authority for their
respective boundaries. No implementation may widen this behavior by silently
adding a source, fallback, parser extension, lifecycle writer, ownership rule,
or mutation effect.
