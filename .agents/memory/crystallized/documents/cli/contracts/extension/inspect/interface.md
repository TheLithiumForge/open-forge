---
open-forge:
  description: Accepted read-only Interface for inspecting one Extension ID across installed, available, and three-way facts
  responsibility: Define inspect's stable-ID syntax, exact source handling, result graph, finding vocabulary, fingerprint compatibility, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Interface, ReadOnly, Source, Lifecycle, CurrentTruth]
---

# extension inspect Interface Contract

## Status And Authority

This is the current Crystallized Interface Contract for read-only
`open-forge extension inspect`. It owns the exact syntax, stable-ID subject,
source selection, installed and available projections, dependency facts,
comparison result, generated-navigation boundary, finding vocabulary, status,
`next` values, structured result graph, examples, and public conformance. The
command does not ship yet; this contract does not claim implementation or
Gate-5 evidence.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution, fingerprint formation, comparison, retention, and result
formation. The [Extension group entrypoint](../_extension.md) defines group help
only. [Global CLI Flags](../../shared/global-flags/interface.md) defines shared
flags and terminal behavior. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the shared JSON
envelope, source-location primitive, and status/process coordinates. The [CLI
Architecture](../../../architecture.md) defines parser, filesystem, concrete
serialization, and evidence boundaries.

## Purpose And Boundary

`inspect` gives package-specific read-only detail before an install, update, or
remove decision. It reports one exact stable package ID from installed
lifecycle facts, one selected package source, or a comparison of both when the
required facts are trusted and complete.

It may report installed-only, available-only, or baseline/current/intended
facts. It never installs, updates, removes, creates, repairs, adopts, indexes,
locks, writes, downloads, executes package content, or turns a recommendation
into authority. An installed fact can remain visible without a healthy current
Framework, but that fact does not grant mutation trust.

## Syntax

```text
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
```

Exactly one stable-ID operand is required. `--source` is one exact external
package or catalogue read location. There is no `--all`, `--automatic`,
`--dry-run`, `--force`, `--prune`, wizard, or mutation flag.

The shared flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`. Their grammar,
defaults, repetition, terminal behavior, and no-op rules remain in [Global CLI
Flags](../../shared/global-flags/interface.md).

The stable ID uses the accepted Extension identity grammar: lowercase ASCII
letters and digits in non-empty segments separated by single hyphens, with a
maximum length of 128 characters. It is an exact package identity, not a folder
name, path spelling, version selector, fuzzy query, or semantic
recommendation. A malformed, missing, duplicated, or conflicting manifest ID
never supplies an inferred subject.

## Subject And Source

The subject is the exact supplied stable ID. Inspect resolves it only against
the selected source universe and the selected workspace's installed lifecycle
facts. No path, folder, version, proximity, registry, cache, network, ambient
search, glob, resemblance, or embedded fallback supplies a different identity.

Without `--source`, Inspect reads installed facts from the exact workspace and
available facts from the embedded catalogue. With `--source`, it reads only
that exact package or catalogue and proves that source is lexically and
physically disjoint from the target workspace. An explicit source remains the
only source for that invocation, even when it is missing, malformed, or
unreadable.

The selected source may contain one requested package or a catalogue containing
it. Dependency closure is resolved only inside that one source universe. A
multi-package source never selects another package by folder spelling, source
order, version, or proximity.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed
source assets; it is not evidence of a selected workspace's current
installation or of a proven runtime implementation.

## Installed, Available, And Comparison Views

The result has one explicit projection state:

- **Installed-only:** the requested ID has readable managed facts, but current
  package source bytes are unavailable. Installed facts and persisted baseline
  facts remain visible.
- **Available-only:** the selected source has the requested ID, but no complete
  managed installation record exists. A missing lifecycle document or section
  is not silently converted into a trusted absent record.
- **Baseline/current/intended:** trusted installed facts and current source bytes
  both exist. Inspect can compare declared paths, dependency closure, owners,
  semantic or exact-byte fingerprints, and derived generated effects.
- **Unavailable or blocked:** safe facts already established remain visible,
  while the unavailable, invalid, ambiguous, or unsafe boundary is represented
  by its state and finding. Inspect never fabricates zero, empty, trusted,
  available, or absent facts.

External destinations have exact-byte fingerprints and no generated-region
entries, including when their filenames denote Markdown. Comparable external
baseline/current/intended hashes determine byte relations; the existing
semantic-only recommendation gate remains unchanged.

The result distinguishes a known empty collection from unavailable coverage. A
known empty count is `0`. A count whose input could not be established is
`null`. Every array is present, including arrays in partial and event results.

## Structured Result And Shared Envelope

For JSON presentation, Inspect emits one `CliJsonEnvelopeV1` document in the
shared [result-coordinate envelope](../../shared/result-coordinates/interface.md).
Its camel-case members and order are fixed:

```text
CliJsonEnvelopeV1 {
  schemaVersion: integer(1),
  command: "extension inspect",
  status: "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted",
  workspace: { path: string, selectedBy: "current-directory" | "explicit-workspace" } | null,
  result: InspectResult,
  next: { command: string, reason: string } | null
}
```

`schemaVersion`, `command`, `status`, `workspace`, and `next` belong to the
shared envelope. They do not occur inside `InspectResult`. `result` is never
`null`. Every envelope member is present, including a `null` workspace or `null`
next action. JSON uses stdout for all seven statuses; terminal help and version
retain the shared text-only bypass.

### Exact Command-Local Schema

The following grammar is the complete command-local result in frozen camel-case
wire order. No command-local member is omitted. `SourceLocation` is the shared
Architecture primitive. `bounded-string` means a bounded, escaped string that
does not expose exception names, secrets, complete environment state, or
unbounded source content.

```text
type InspectResult = {
  subject: Subject;
  source: Source;
  lifecycle: Lifecycle;
  installed: Installed;
  available: Available;
  dependencies: DependencyClosure;
  pathFacts: PathFacts;
  comparison: Comparison;
  generated: GeneratedNavigation;
  findings: Finding[];
  counts: Counts;
};

type Subject = {
  supplied: string | null;
  form: "stable-id" | null;
  id: string | null;
  state: "not-started" | "resolved" | "invalid" | "unknown" | "ambiguous" | "unsafe";
  candidates: Candidate[];
};

type Candidate = {
  id: string;
  path: string;
};

type Source = {
  supplied: string | null;
  explicit: boolean;
  identity: string | null;
  kind: "embedded-catalogue" | "package" | "catalogue" | null;
  state: "not-started" | "available" | "missing" | "invalid" | "blocked" | "unavailable" | "failed" | "interrupted";
};

type Lifecycle = {
  documentPath: ".agents/open-forge.lifecycle.json";
  readState: "not-started" | "complete" | "missing" | "invalid" | "unavailable" | "blocked" | "failed" | "interrupted";
  trust: "not-started" | "trusted" | "untrusted" | "incomplete" | "blocked" | "absent";
  coverage: "not-started" | "complete" | "incomplete" | "blocked" | "failed" | "interrupted";
  workspaceBinding: "not-checked" | "matched" | "mismatched" | "unavailable";
  fingerprintPolicy: "open-forge-markdown-v1" | null;
};

type Installed = {
  state: "not-started" | "present" | "absent" | "unavailable" | "invalid" | "blocked" | "failed" | "interrupted";
  package: InstalledPackage | null;
};

type InstalledPackage = {
  id: string;
  version: string | null;
  source: string | null;
  dependencies: string[];
  paths: string[];
};

type Available = {
  state: "not-started" | "present" | "absent" | "unavailable" | "invalid" | "blocked" | "failed" | "interrupted";
  package: AvailablePackage | null;
};

type AvailablePackage = {
  id: string;
  name: string;
  description: string;
  version: string;
  manifestPath: string;
  dependencies: string[];
  payload: PackageFile[];
};

type PackageFile = {
  path: string;
  targetPath: string | null;
  state: "not-started" | "available" | "missing" | "invalid" | "blocked" | "unavailable" | "failed" | "interrupted";
  byteLength: nonnegative-integer | null;
  sha256: lowercase-hex-64 | null;
};

type DependencyClosure = {
  state: "not-started" | "complete" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
  declared: DependencyEdge[];
  resolved: DependencyPackage[];
  order: string[];
};

type DependencyEdge = {
  from: string;
  to: string;
  position: positive-integer;
};

type DependencyPackage = {
  id: string;
  version: string | null;
  source: string | null;
  state: "not-started" | "available" | "missing" | "invalid" | "blocked" | "unavailable" | "failed" | "interrupted";
};

type PathFacts = {
  state: "not-started" | "complete" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
  declared: DeclaredPath[];
  current: CurrentPath[];
};

type DeclaredPath = {
  path: string;
  sourcePath: string | null;
  state: "not-started" | "available" | "missing" | "invalid" | "blocked" | "unavailable" | "failed" | "interrupted";
};

type CurrentPath = {
  path: string;
  state: "not-started" | "present" | "missing" | "invalid" | "blocked" | "unavailable" | "failed" | "interrupted";
  physicalIdentity: string | null;
  byteLength: nonnegative-integer | null;
  exactSha256: lowercase-hex-64 | null;
};

type Comparison = {
  state: "not-started" | "complete" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
  mode: "none" | "installed-only" | "available-only" | "three-way";
  baseline: ComparisonSide;
  current: ComparisonSide;
  intended: ComparisonSide;
  paths: PathComparison[];
  dependencies: DependencyComparison;
};

type ComparisonSide = {
  state: "not-started" | "not-applicable" | "available" | "unavailable" | "invalid" | "blocked" | "failed" | "interrupted";
  fingerprints: FingerprintFact[];
};

type FingerprintFact = {
  path: string;
  fingerprint: Fingerprint | null;
};

type Fingerprint = {
  kind: "semantic" | "exact-bytes";
  policy: "open-forge-markdown-v1" | null;
  sha256: lowercase-hex-64;
  origin: "persisted-baseline" | "operation-time-current" | "operation-time-intended";
};

type PathComparison = {
  path: string;
  baseline: Fingerprint | null;
  current: Fingerprint | null;
  intended: Fingerprint | null;
  relation: "not-started" | "not-applicable" | "unchanged" | "changed" | "current-diverged" | "missing" | "new" | "retired" | "shared" | "unknown" | "unavailable" | "invalid" | "blocked";
  baselineOwners: string[];
  currentOwners: string[];
  intendedOwners: string[];
};

type DependencyComparison = {
  state: "not-started" | "not-applicable" | "available" | "unavailable" | "invalid" | "blocked" | "failed" | "interrupted";
  baseline: string[];
  current: string[];
  intended: string[];
  relation: "not-started" | "not-applicable" | "equal" | "changed" | "unavailable" | "invalid" | "blocked";
};

type GeneratedNavigation = {
  state: "not-started" | "complete" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
  ownership: "derived-navigation-only";
  regions: GeneratedRegion[];
};

type GeneratedRegion = {
  path: string;
  state: "not-started" | "valid" | "absent" | "invalid" | "ambiguous" | "unavailable";
  startMarker: string | null;
  endMarker: string | null;
  startByteOffset: nonnegative-integer | null;
  endByteOffset: nonnegative-integer | null;
  excludedInteriorByteLength: nonnegative-integer | null;
  markerLinesRetained: boolean;
};

type Finding = {
  code: InspectFindingCode;
  status: SharedStatus;
  subject: string | null;
  packageId: string | null;
  dependency: string | null;
  path: string | null;
  cause: bounded-string;
  location: SourceLocation | null;
  candidates: Candidate[];
};

type InspectFindingCode =
  "extension-inspect.invalid-input" | "extension-inspect.invalid-stable-id" |
  "extension-inspect.workspace-unavailable" | "extension-inspect.workspace-unsafe" |
  "extension-inspect.source-unavailable" | "extension-inspect.source-invalid" |
  "extension-inspect.source-overlap" | "extension-inspect.source-ambiguous" |
  "extension-inspect.identity-ambiguous" | "extension-inspect.lifecycle-unavailable" |
  "extension-inspect.lifecycle-invalid" | "extension-inspect.lifecycle-blocked" |
  "extension-inspect.package-unavailable" | "extension-inspect.package-invalid" |
  "extension-inspect.dependency-incomplete" | "extension-inspect.dependency-cycle" |
  "extension-inspect.dependency-conflict" | "extension-inspect.path-unavailable" |
  "extension-inspect.path-invalid" | "extension-inspect.ownership-conflict" |
  "extension-inspect.fingerprint-unavailable" | "extension-inspect.fingerprint-fallback" |
  "extension-inspect.generated-boundary-invalid" | "extension-inspect.dependency-changed" |
  "extension-inspect.path-changed" | "extension-inspect.path-current-diverged" |
  "extension-inspect.path-missing" | "extension-inspect.path-new" |
  "extension-inspect.path-retired" | "extension-inspect.operation-failed" |
  "extension-inspect.interrupted";

type Counts = {
  installedPackages: nonnegative-integer | null;
  availablePackages: nonnegative-integer | null;
  declaredPaths: nonnegative-integer | null;
  currentPaths: nonnegative-integer | null;
  baselinePaths: nonnegative-integer | null;
  intendedPaths: nonnegative-integer | null;
  dependencies: nonnegative-integer | null;
  unchangedPaths: nonnegative-integer | null;
  changedPaths: nonnegative-integer | null;
  currentDivergedPaths: nonnegative-integer | null;
  missingPaths: nonnegative-integer | null;
  newPaths: nonnegative-integer | null;
  retiredPaths: nonnegative-integer | null;
  sharedPaths: nonnegative-integer | null;
  generatedRegions: nonnegative-integer | null;
  excludedGeneratedBytes: nonnegative-integer | null;
  findings: nonnegative-integer;
};

type SharedStatus =
  "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";

type lowercase-hex-64 = string matching [0-9a-f]{64};
type nonnegative-integer = integer >= 0;
type positive-integer = integer >= 1;
```

`subject.supplied` retains the received operand when one exists. `form` is
`stable-id` when the operand occupies the stable-ID position and is `null` when
the subject was not established. `id` is the exact valid or unknown ID when it
can be retained and is `null` when syntax or identity safety prevents it.
`candidates` is always present and is empty except for an ambiguity or
collision. Candidate records are sorted by exact ID, then canonical path.

`source.supplied` is the exact explicit source value or `null` when omitted.
`explicit` is always present. `identity` and `kind` are `null` until the source
identity is established. `available` means the selected source read completed;
`missing`, `invalid`, `blocked`, `unavailable`, `failed`, and `interrupted`
retain the corresponding source boundary. `not-started` records work skipped
because an earlier boundary stopped the operation.

When `explicit` is `false` and embedded source selection succeeds, `identity`
and every retained source scalar for that source, including
`installed.package.source` and resolved dependency `source`, are exactly
`embedded catalogue` (including the space). Only `kind` uses the structural
value `embedded-catalogue`. Explicit package and catalogue identities retain
their accepted external spelling and are not rewritten.

`lifecycle.documentPath` is the fixed workspace-relative input path. `readState`
records the lifecycle document read, `trust` records the Extension-section
trust classification, and `coverage` records whether the required lifecycle
facts were established. `absent` is valid only after a complete, consistent
empty Extension section inspection. A missing document or section has
`readState: "missing"`, `trust: "incomplete"`, and incomplete coverage; it is
not an empty installed set. `workspaceBinding` is independent of the envelope's
workspace selection and records whether the lifecycle document names the
selected workspace. `fingerprintPolicy` is `null` until the accepted lifecycle
policy is read.

`installed.package` retains independently readable lifecycle identity. Its
`source` and scalar facts are `null` when that fact is not safely available;
its arrays remain present. Installed dependency and path arrays use their
validated lifecycle order, with paths in canonical `/` path order.
`installed.state: "absent"` requires complete proof of an empty Extension
section. `available.package` contains the exact manifest facts and source
payload inventory for the requested package. Manifest dependencies retain
declaration order. Each payload record keeps its package-relative `path`, mapped
target path when established, read state, byte length, and lowercase SHA-256
when the bytes are available; the payload array is ordered by canonical package
relative path.

`dependencies.declared` preserves direct declarations in source or installed
facts. `resolved` contains every safely resolved package in the requested
closure, including the requested package when source facts are available.
`order` is dependency-first. Independent nodes and equal-depth ties use ordinal
stable ID order. An unavailable or incomplete closure keeps known declarations
and nodes but leaves unknown arrays empty and counts `null` where coverage is
not established. The requested package is included once in `resolved` and
`order` whenever its selected package facts are available; `counts.dependencies`
counts those resolved closure packages, not the direct declaration edges.

`pathFacts.declared` and `pathFacts.current` are separate projections. Declared
paths are target-relative package paths with an optional exact source path.
Current paths retain the current physical identity, byte length, and fresh
exact-byte hash when available. `exactSha256` is an operation-time observation;
it is not a persisted baseline. Filesystem enumeration order never determines
array order: declared and current paths are ordinal by canonical `/` path.

`comparison.mode` is `none` before either side is established, `installed-only`
when installed facts are retained without an intended source projection,
`available-only` when only source facts are available, and `three-way` only when
trusted baseline, current, and intended facts are all established. A side's
`fingerprints` array is always
present. `not-applicable` means the side has no meaning for the selected view;
`unavailable` means it was required but could not be established. A
source-unavailable installed-only projection may have a complete comparison
state for the applicable persisted baseline and current observations while its
intended side is `not-applicable`; the source-unavailable finding remains the
non-blocking `attention` signal. A three-way state is never complete unless all
three sides are available. A
`Fingerprint` with `origin: "persisted-baseline"` is read from lifecycle state.
Current and intended fingerprints are fresh operation-time observations.
Fingerprint arrays and comparison path arrays use canonical `/` path order;
owner arrays use ordinal stable-ID order.

`PathComparison.relation` uses these exact meanings:

- `unchanged` means baseline, current, and intended identities are equal.
- `changed` means baseline and current are equal, while intended differs and a
  normal update can safely apply the intended identity.
- `current-diverged` means current differs from the persisted baseline.
- `missing`, `new`, and `retired` mean an expected path is absent, a new source
  path has no baseline, or a prior path is no longer intended, respectively.
- `shared` means compatible multiple owners are established. `unknown` means
  the available identities cannot establish a safe relation.
- `unavailable`, `invalid`, and `blocked` retain the comparison boundary that
  stopped classification. `not-started` and `not-applicable` preserve early or
  view-specific states.

`baselineOwners`, `currentOwners`, and `intendedOwners` are always present and
are ordinal, duplicate-free owner IDs. They do not create ownership: they only
report lifecycle and source facts already established by their respective
side.

`generated.ownership` is the fixed literal `derived-navigation-only`. Generated
navigation never becomes package-owned authored content. `regions` contains one
record per inspected Markdown path, in path order. A valid region retains both
marker lines and reports the removed interior byte length. Its offsets are byte
coordinates in the normalized UTF-8 fingerprint bytes, not original physical
input bytes or Unicode scalar indices: `startByteOffset` is immediately after
the normalized start-marker line, `endByteOffset` is at the normalized
end-marker line start, and `excludedInteriorByteLength` is their nonnegative
difference. A valid region therefore has non-null offsets and
`markerLinesRetained: true`. An absent region has null offsets, excluded length
`0`, and `markerLinesRetained: false`. `invalid`, `ambiguous`, `unavailable`,
and `not-started` regions have null offsets, null excluded length, and
`markerLinesRetained: false`; they do not claim semantic exclusion. These
state rules hold even when safe original bytes or an exact-byte fallback hash
remain available.

`findings` is always present and ordered by the finding table below. `cause` is
required. `subject`, `packageId`, `dependency`, `path`, `location`, and
`candidates` remain present even when their values are `null` or empty. A
location is `null` when it does not apply or cannot be safely established; the
finding cause explains the latter. `counts.findings` equals the emitted finding
array length. Every other count is `0` only for a known empty collection and is
`null` when its coverage is unavailable. `generatedRegions` counts recognized
valid regions rather than inspected Markdown paths; `excludedGeneratedBytes`
is the sum of valid omitted interiors and is `0` when no bytes are omitted.

## Finding Codes And Ordering

Inspect has exactly this finding vocabulary. The `Order` column is the global
finding ordinal; each code has exactly one status.

| Order | Machine code                                   | Status        | Meaning                                                                                                                                             |
| ----: | ---------------------------------------------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
|     1 | `extension-inspect.invalid-input`              | `invalid`     | The command has missing or extra operands, an unsupported flag, a terminal-mode conflict, or another invalid request shape.                         |
|     2 | `extension-inspect.invalid-stable-id`          | `invalid`     | The supplied stable ID is empty or violates the exact Extension ID grammar.                                                                         |
|     3 | `extension-inspect.workspace-unavailable`      | `blocked`     | The selected workspace cannot be established as the required directory.                                                                             |
|     4 | `extension-inspect.workspace-unsafe`           | `blocked`     | Workspace physical identity or containment cannot be proved safely.                                                                                 |
|     5 | `extension-inspect.source-unavailable`         | `attention`   | The explicit or embedded source is missing or inaccessible while independently readable installed facts still answer the installed-only inspection. |
|     6 | `extension-inspect.source-invalid`             | `incomplete`  | The selected source has invalid encoding, structure, or package metadata, so source-dependent facts are incomplete.                                 |
|     7 | `extension-inspect.source-overlap`             | `blocked`     | The selected source is lexically or physically overlapping with the target workspace.                                                               |
|     8 | `extension-inspect.source-ambiguous`           | `blocked`     | One source value has more than one structural package or catalogue interpretation.                                                                  |
|     9 | `extension-inspect.identity-ambiguous`         | `blocked`     | The requested ID has duplicate active package identities or unresolved source candidates.                                                           |
|    10 | `extension-inspect.lifecycle-unavailable`      | `incomplete`  | Required lifecycle document or Extension-section facts cannot be read.                                                                              |
|    11 | `extension-inspect.lifecycle-invalid`          | `incomplete`  | Lifecycle encoding, schema, value, order, or reciprocal facts are malformed while safe partial facts remain.                                        |
|    12 | `extension-inspect.lifecycle-blocked`          | `blocked`     | Lifecycle identity, ownership, path, or cross-section ambiguity makes a safe classification impossible.                                             |
|    13 | `extension-inspect.package-unavailable`        | `incomplete`  | The requested package is not readable in the selected source universe or its required package facts are unavailable.                                |
|    14 | `extension-inspect.package-invalid`            | `incomplete`  | The requested package manifest or payload inventory is malformed or cannot be validated.                                                            |
|    15 | `extension-inspect.dependency-incomplete`      | `incomplete`  | Dependency declarations or transitive closure are only partly established.                                                                          |
|    16 | `extension-inspect.dependency-cycle`           | `blocked`     | The selected dependency graph contains a cycle.                                                                                                     |
|    17 | `extension-inspect.dependency-conflict`        | `blocked`     | Dependency IDs, declarations, source identities, or closure order conflict or duplicate.                                                            |
|    18 | `extension-inspect.path-unavailable`           | `incomplete`  | A declared or current path cannot be read or its required fact cannot be established.                                                               |
|    19 | `extension-inspect.path-invalid`               | `blocked`     | A declared target path is unsafe, reserved, malformed, or outside the permitted workspace boundary.                                                 |
|    20 | `extension-inspect.ownership-conflict`         | `blocked`     | Owner sets or route ownership are conflicting, ambiguous, or unsafe.                                                                                |
|    21 | `extension-inspect.fingerprint-unavailable`    | `incomplete`  | Required bytes or an operation-time fingerprint cannot be read.                                                                                     |
|    22 | `extension-inspect.fingerprint-fallback`       | `incomplete`  | Unsupported, binary, invalid-UTF-8, unparseable, or invalid-region input uses exact-byte fallback and cannot claim semantic equivalence.            |
|    23 | `extension-inspect.generated-boundary-invalid` | `incomplete`  | A marker-looking generated region is malformed, duplicate, reversed, nested, or ambiguous and therefore remains in exact-byte fallback.             |
|    24 | `extension-inspect.dependency-changed`         | `attention`   | Complete three-way facts show intended dependency identity differs from the current identity.                                                       |
|    25 | `extension-inspect.path-changed`               | `attention`   | Complete three-way facts show baseline and current identity agree while intended content differs.                                                   |
|    26 | `extension-inspect.path-current-diverged`      | `attention`   | Complete facts show current content differs from the persisted baseline; Inspect does not authorize replacement.                                    |
|    27 | `extension-inspect.path-missing`               | `attention`   | A trusted expected path is missing from the current workspace.                                                                                      |
|    28 | `extension-inspect.path-new`                   | `attention`   | Intended source contains a new path with no persisted baseline.                                                                                     |
|    29 | `extension-inspect.path-retired`               | `attention`   | Intended source no longer contains a previously managed path.                                                                                       |
|    30 | `extension-inspect.operation-failed`           | `failed`      | An unexpected read or result-formation failure prevents normal completion.                                                                          |
|    31 | `extension-inspect.interrupted`                | `interrupted` | Caller cancellation or interruption stopped the operation before completion.                                                                        |

The code meaning is local to Inspect. It does not infer lifecycle ownership,
select an update mode, repair a marker, or convert an observation into an
effect. A formatting-only difference is not a divergence when the admitted
semantic fingerprints are equal. Exact-byte fallback is incomplete for
semantic equivalence even when its fresh exact hash is available.

Finding order is deterministic and independent of filesystem or exception
order. Sort by the table's `Order`, then apply the first applicable tie-break:

1. For subject, source, workspace, and lifecycle findings, sort by
   `subject` ordinal, then `packageId` ordinal, then `path` ordinal.
2. For package and dependency findings, sort by `packageId` ordinal, then
   `dependency` ordinal, then `path` ordinal.
3. For path, ownership, fingerprint, and generated findings, sort by canonical
   `/` path using ordinal comparison, then by location byte offset, line, and
   column. A `null` location sorts after an established location.
4. For ambiguity findings, sort `candidates` by ID and path, and use that
   ordered candidate list as the tie-break. The list itself is never reduced to
   one candidate.
5. A remaining equal key is a duplicate. The operation rejects duplicate
   findings rather than emitting duplicate evidence. Failure and interruption
   findings are last in their fixed event order.

Every finding's `status` must equal the status in this table. A duplicate,
unknown code, unknown status, or code/status mismatch is a result-formation
failure. The implementation must fail closed with the defined
`extension-inspect.operation-failed` finding and must not serialize an unknown
value. A finding cannot be downgraded or upgraded to change aggregate status.

Aggregate status has one total precedence, from strongest to weakest:

```text
interrupted > failed > invalid > blocked > incomplete > attention > complete
```

If no findings exist and every required coverage state is complete, the result
is `complete`. Otherwise the highest-precedence finding status is the envelope
status. A higher result may retain already established lower-severity findings;
it may not retain a finding whose code/status pair is invalid. Event statuses
retain their event meaning even when facts acquired earlier remain visible.

## Next Actions

The top-level `next` member is the Architecture shape `{ command, reason }` or
`null`. It is not repeated inside `InspectResult`. This table is exhaustive and
uses only the exact values shown.

| Condition                                                                     | Top-level `next`                                                                                                                                                  | Compact human line                               |
| ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| `complete`                                                                    | `null`                                                                                                                                                            | no line                                          |
| `attention` with an actionable trusted complete semantic three-way divergence | `{ command: "open-forge extension update {subject.id}", reason: "Apply the trusted current-source change for this stable ID with the explicit update command." }` | `Next: open-forge extension update {subject.id}` |
| other `attention`                                                             | `null`                                                                                                                                                            | no line                                          |
| `incomplete`                                                                  | `{ command: "open-forge doctor", reason: "Inspect unavailable lifecycle, source, dependency, path, or fingerprint facts before relying on this result." }`        | `Next: open-forge doctor`                        |
| `invalid`                                                                     | `{ command: "open-forge extension inspect --help", reason: "Correct the named Extension Inspect input, then rerun the request." }`                                | `Next: open-forge extension inspect --help`      |
| `blocked`                                                                     | `{ command: "open-forge doctor", reason: "Inspect the blocked workspace, source, identity, or ownership boundary before rerunning Extension Inspect." }`          | `Next: open-forge doctor`                        |
| `failed`                                                                      | `{ command: "open-forge extension inspect --verbose", reason: "Report the failure and retry Extension Inspect with bounded diagnostics." }`                       | `Next: open-forge extension inspect --verbose`   |
| `interrupted`                                                                 | `{ command: "open-forge extension inspect", reason: "Rerun the same Extension Inspect request." }`                                                                | `Next: open-forge extension inspect`             |

An actionable three-way divergence is deliberately narrow. It requires all of
the following:

- `subject.id` is one resolved stable ID;
- `source.state` is `available`, `available.package` is present, and no
  explicit-source failure or fallback exists;
- `lifecycle.readState` and `lifecycle.coverage` are `complete`, `trust` is
  `trusted`, and `workspaceBinding` is `matched`;
- `dependencies.state`, `pathFacts.state`, `comparison.state`, and
  `generated.state` are complete;
- `comparison.mode` is `three-way`, all three sides are `available`, and every
  actionable fingerprint is `kind: "semantic"` with policy
  `open-forge-markdown-v1`;
- at least one path has relation `changed` or the dependency comparison is
  `changed`; and
- there is no current divergence, missing path, retired path, ownership
  conflict, dependency conflict, invalid boundary, unavailable fact, or
  exact-byte fallback.

The explanatory update template is `open-forge extension update {subject.id}`.
The emitted `next.command` substitutes the resolved `subject.id`, so it is an
executable action line with no angle-bracket or brace placeholder. It is a
recommendation, not an invocation, plan, lock, source fallback, or mutation
authority. Inspect never recommends update for available-only, installed-only,
exact-byte fallback, incomplete, blocked, invalid, failed, or interrupted
results. Non-actionable attention may have `null`.

## Fingerprint Policy: `open-forge-markdown-v1`

`open-forge-markdown-v1` is one immutable fingerprint policy. The policy string
is its version. Any change to byte admission, UTF-8 handling, BOM or NUL
handling, line-ending rules, CommonMark parser validation, generated-region
recognition, byte omission, hash algorithm, hash encoding, or fallback changes
the policy value and requires a new policy version.

### Admitted Markdown and fallback boundary

For a Markdown package path, the policy first receives the exact original byte
sequence. It does not pre-normalize bytes. Strict UTF-8 decoding is required.
A leading UTF-8 BOM (`EF BB BF`) is retained as the decoded U+FEFF at its exact
position; BOM presence and absence are distinct bytes and are not normalized.
Any NUL byte is a binary boundary. Invalid UTF-8, NUL, unsupported package kind,
binary input, unparseable input, or an invalid generated-region boundary uses
the exact-byte fallback.

The exact-byte fallback hashes the original bytes exactly. It does not normalize
line endings, strip a BOM, remove NUL, parse Markdown, omit generated content,
or add a final newline. Its result has `kind: "exact-bytes"`, its hash is the
lowercase SHA-256 of the original bytes, and its policy remains the selected
`open-forge-markdown-v1` policy when the fallback is reported as an Inspect
observation. A fallback never proves semantic equivalence or mutation
authority.

Admitted Markdown is parsed with one fixed CommonMark parser configuration.
Parser extensions, permissive decoding, renderer output, YAML reserialization,
Unicode normalization, whitespace trimming, frontmatter rewriting, heading
rewriting, tag rewriting, link rewriting, code rewriting, and final-newline
insertion are not part of this policy.

### Byte normalization and generated region

After strict decoding and fixed parser validation, the only ordinary byte
normalization is line-ending normalization: every CRLF pair and every lone CR
becomes one LF. LF remains LF. No other Unicode scalar, whitespace byte,
frontmatter byte, heading byte, tag byte, link or destination byte, code byte,
or final-newline byte changes.

The parser recognizes a generated region only when all conditions below hold:

1. There is exactly one canonical ATX heading line in the document whose
   complete text is `## Entries`, and it is the final heading section. The
   heading is outside an inline code span and fenced code block and has no extra
   heading text or trailing whitespace.
2. Outside code, the final `Entries` section contains exactly one standalone
   start-marker line and exactly one standalone end-marker line, in that order:

   ```text
   <!-- open-forge:generated-index:start -->
   <!-- open-forge:generated-index:end -->
   ```

   The marker line has no indentation or additional bytes other than its line
   ending; at EOF, the final marker may have no line-ending bytes. Both markers
   belong to that final `Entries` section.

3. The end marker is the final non-empty line of the document. Only its own
   line-ending bytes may follow it. The marker lines and their normalized line
   endings are part of the retained output.
4. The marker tokens are represented outside code by the fixed CommonMark
   parse. Marker text in an inline code span or fenced code block does not form
   a generated region.

If no marker appears outside code, the region state is `absent`; the admitted
Markdown bytes still receive a semantic fingerprint with no generated-byte
omission. If marker-looking input fails any condition, including a duplicate,
reversed, nested, misplaced, malformed, or otherwise ambiguous region, the
region is invalid and the exact-byte fallback is used. The policy does not
choose a likely pair or normalize a partially valid region.

For one valid region, omission is exactly the half-open byte interval from the
end of the normalized start-marker line through the byte immediately before the
start of the normalized end-marker line:

```text
[end-of-start-marker-line, start-of-end-marker-line)
```

These two normalized UTF-8 positions are exactly `startByteOffset` and
`endByteOffset`; the omitted length is
`excludedInteriorByteLength = endByteOffset - startByteOffset`. In the valid
generated LF vector below, the coordinates are `startByteOffset: 54`,
`endByteOffset: 66`, and `excludedInteriorByteLength: 12`, with both marker
lines retained.

Only bytes strictly inside those marker lines are omitted. The complete start
and end marker lines, all bytes before the start marker, all bytes after the
end marker that are permitted by the final-line rule, and every other authored
byte remain. The omitted interval may have length zero. The normalized text is
encoded as UTF-8 with an encoder that adds no preamble. It retains any admitted
U+FEFF content and never adds a BOM. SHA-256 is rendered as exactly 64 lowercase
hexadecimal characters.

### Equivalence and persistence

Two fingerprints are equivalent only when their policy values, kinds, and
lowercase SHA-256 values are equal. A semantic fingerprint and an exact-byte
fingerprint are never equivalent, even if their digest strings happen to match.
An unavailable fingerprint is not equal to anything. Equal exact-byte hashes
prove equal bytes for that operation; they do not prove semantic equivalence,
ownership, or a safe mutation.

The lifecycle input is exactly `.agents/open-forge.lifecycle.json`, schema v1.
Its common envelope records `fingerprintPolicy`, and each path record records
`baselineFingerprint` and `fingerprintKind`. Inspect reports those persisted
facts exactly. A semantic baseline is usable as a semantic baseline only when
its policy is `open-forge-markdown-v1` and its kind is `semantic`. Fresh current
and intended hashes, including exact-byte fallback hashes, are operation-time
observations and are never written by Inspect.

The schema-v1 reader can surface an authored `fingerprintKind: "exact-bytes"`
record as a persisted lifecycle fact. Inspect does not relabel it, create a new
exact-byte baseline, infer mutation authority, or silently resolve the separate
mutation-contract rule that new lifecycle writers persist semantic baselines
only. Reconciliation of that reader/writer boundary belongs to the Mutation
Foundation; this read-only contract only reports the record it receives.

## Output, Status, And Errors

Both human views begin with outcome, status, exact workspace and selection,
selected package ID/source, installed and available state, and installation-record
trust/coverage. Counts come from the nullable result counts. An unavailable source
or absent available-package object never supplies a fabricated zero intended count.

Compact retains comparison mode and coverage, significant path differences,
required dependency/ownership conditions and every finding with its exact
subject, package/dependency/path identity, location and candidates when supplied.
Healthy unchanged and available source-only paths may be summarized by count;
unavailable observations stay visible. Files with no generated region may also
be counted. Expanded adds complete known dependency and path inventories, package metadata,
and installed-baseline/current-workspace/selected-package comparison facts with
owner and fingerprint details. Generated navigation stays explicitly derived.
Group matching observations beside exact path identities, retain unmatched facts
and keep distinct values separate. This changes human layout only; it never
recomputes or reorders the result or its JSON arrays.

Both views retain the actual required Next command; expanded may add its reason.
Paths are not truncated. `--view` does not alter JSON, which emits the complete graph above.

An inspection excerpt is:

```text
Extension inspection
Status: requires attention
Workspace: /work/example
Selected by: current directory
Package: review-tools; resolved
Source: embedded catalogue; available
Installed: present; version 0.1.0
Available: present; version 0.2.0
Installation record: complete; trusted; coverage complete
Comparison: installed baseline, current workspace and selected package; complete
Paths: 1 current; 1 intended; 0 unchanged
Path checks: complete
  .agents/guidance/review.md
    Comparison: selected package differs; workspace still matches the installed baseline
Next: open-forge extension update review-tools
```

The full result also retains supplied findings and dependency/generated-navigation
facts. Expanded includes the underlying fingerprints and owner observations.

The values are illustrative and are not current inventory or implementation
evidence. Primary human `complete`, `attention`, and `incomplete` results go to
stdout. Primary human `invalid`, `blocked`, `failed`, and `interrupted` results
go to stderr. Bounded diagnostics use stderr. JSON uses one result on stdout
for every status.

| Result        | Meaning for `inspect`                                                                                                                                             |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The requested ID and all applicable installed/source facts are completely readable, with a valid empty comparison or equal comparison where applicable.           |
| `attention`   | Complete facts expose finite divergence or another non-blocking observation that does not prevent safe reporting.                                                 |
| `incomplete`  | Safe installed or source facts remain, but required current source, lifecycle, dependency, parser, path, generated-boundary, or semantic coverage is unavailable. |
| `invalid`     | The ID, source input, operand, flag, repetition, or terminal-mode request is invalid.                                                                             |
| `blocked`     | Ambiguous identity, source overlap, unsafe containment, ownership collision, malformed lifecycle evidence, or another unsafe boundary prevents safe inspection.   |
| `failed`      | An unexpected read or result-formation failure occurs.                                                                                                            |
| `interrupted` | Caller cancellation or interruption occurs before the read-only result completes.                                                                                 |

Every error names `extension inspect`, the ID or source when known, the direct
cause, and at most one useful next action. An explicit missing or malformed
source is not silently replaced by the embedded catalogue. A source-unavailable
installed-only result retains independently readable installed facts and is a
non-actionable `attention`; it does not claim an exact no-op or an update. If
another required boundary is incomplete, its finding raises the aggregate
result to `incomplete` and the incomplete next action applies.

## Scenario-to-Result Mapping

The following table maps the public scenarios to the exact command-local fields,
status, findings, and `next`. It is normative; human wording remains
illustrative.

| Scenario                                                                                                                     | Established fields                                                                                                                                                                                           | Status and finding                                                                                                                                  | `next`                                                |
| ---------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| Trusted installed record, embedded source, semantic three-way path change where baseline equals current and intended differs | `subject.resolved`; `source.available`; lifecycle `complete/trusted/matched`; installed and available `present`; dependency, path, comparison, and generated states `complete`; comparison mode `three-way`  | `attention`; `path-changed` (and `dependency-changed` only when applicable)                                                                         | Exact update action from the actionable three-way row |
| Trusted installed record with explicit source missing                                                                        | Installed package, baseline, declared paths, and current path facts remain; `source.state: missing`; available source facts unavailable and intended comparison is not applicable to the installed-only view | `attention`; `source-unavailable`                                                                                                                   | `null`                                                |
| Valid complete empty lifecycle Extension section with available package                                                      | lifecycle `readState: complete`, `trust: absent`, `coverage: complete`; installed `absent`; available `present`; dependency and source paths complete                                                        | `complete`; no findings                                                                                                                             | `null`                                                |
| Available package with missing lifecycle document or section                                                                 | source and available package facts remain; lifecycle `missing/incomplete`; installed `unavailable`, never fabricated `absent`                                                                                | `incomplete`; `lifecycle-unavailable`                                                                                                               | `open-forge doctor`                                   |
| Explicit source contains no requested package                                                                                | source available; available package `null`; dependency closure not complete; installed facts retained if present                                                                                             | `incomplete`; `package-unavailable`                                                                                                                 | `open-forge doctor`                                   |
| Malformed manifest or incomplete dependency closure                                                                          | source identity remains; available package or dependency nodes retained only when safe; unknown fields are `null` or empty                                                                                   | `incomplete`; `package-invalid` or `dependency-incomplete`                                                                                          | `open-forge doctor`                                   |
| Duplicate ID, cycle, conflicting dependency, unsafe target path, overlap, or ownership ambiguity                             | Subject/source candidates and all safe earlier facts remain; unsafe boundary is not selected                                                                                                                 | `blocked`; corresponding `identity-ambiguous`, `dependency-cycle`, `dependency-conflict`, `path-invalid`, `source-overlap`, or `ownership-conflict` | `open-forge doctor`                                   |
| Supported Markdown with a malformed generated boundary                                                                       | Current exact bytes and exact-byte fallback hash remain; generated region is invalid; semantic exclusion is not claimed                                                                                      | `incomplete`; `generated-boundary-invalid` and/or `fingerprint-fallback`                                                                            | `open-forge doctor`                                   |
| Unsupported, binary, invalid-UTF-8, or unparseable payload                                                                   | Path and exact bytes remain when readable; fingerprint kind is `exact-bytes`; no semantic equality or update recommendation                                                                                  | `incomplete`; `fingerprint-fallback` or `fingerprint-unavailable`                                                                                   | `open-forge doctor`                                   |
| Invalid ID or request grammar                                                                                                | `subject.supplied` may remain; later source, lifecycle, package, path, comparison, and generated states are `not-started`; arrays are empty                                                                  | `invalid`; `invalid-input` or `invalid-stable-id`                                                                                                   | `open-forge extension inspect --help`                 |
| Unexpected read/result-formation failure                                                                                     | Facts completed before the failure remain; later states are `not-started`; no fallback is invented                                                                                                           | `failed`; `operation-failed`                                                                                                                        | `open-forge extension inspect --verbose`              |
| Cancellation before completion                                                                                               | Facts completed before cancellation remain; later states are `not-started`; no write occurs                                                                                                                  | `interrupted`; `interrupted`                                                                                                                        | `open-forge extension inspect`                        |

## Exact Structured Examples

The examples below are normative shape examples, not inventory or Gate-5
evidence. Every member is present and follows the frozen order above.

### Trusted semantic three-way attention

```json
{
  "schemaVersion": 1,
  "command": "extension inspect",
  "status": "attention",
  "workspace": {
    "path": "/work/example",
    "selectedBy": "current-directory"
  },
  "result": {
    "subject": {
      "supplied": "development-toolkit",
      "form": "stable-id",
      "id": "development-toolkit",
      "state": "resolved",
      "candidates": []
    },
    "source": {
      "supplied": null,
      "explicit": false,
      "identity": "embedded catalogue",
      "kind": "embedded-catalogue",
      "state": "available"
    },
    "lifecycle": {
      "documentPath": ".agents/open-forge.lifecycle.json",
      "readState": "complete",
      "trust": "trusted",
      "coverage": "complete",
      "workspaceBinding": "matched",
      "fingerprintPolicy": "open-forge-markdown-v1"
    },
    "installed": {
      "state": "present",
      "package": {
        "id": "development-toolkit",
        "version": "0.1.0",
        "source": "embedded catalogue",
        "dependencies": [],
        "paths": [".agents/extensions/development-toolkit.md"]
      }
    },
    "available": {
      "state": "present",
      "package": {
        "id": "development-toolkit",
        "name": "Development Toolkit",
        "description": "Development guidance for an Open Forge workspace.",
        "version": "0.2.0",
        "manifestPath": "extension.json",
        "dependencies": [],
        "payload": [
          {
            "path": "content/development-toolkit.md",
            "targetPath": ".agents/extensions/development-toolkit.md",
            "state": "available",
            "byteLength": 8,
            "sha256": "a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c"
          }
        ]
      }
    },
    "dependencies": {
      "state": "complete",
      "declared": [],
      "resolved": [
        {
          "id": "development-toolkit",
          "version": "0.2.0",
          "source": "embedded catalogue",
          "state": "available"
        }
      ],
      "order": ["development-toolkit"]
    },
    "pathFacts": {
      "state": "complete",
      "declared": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "sourcePath": "content/development-toolkit.md",
          "state": "available"
        }
      ],
      "current": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "state": "present",
          "physicalIdentity": "file:/work/example/.agents/extensions/development-toolkit.md",
          "byteLength": 6,
          "exactSha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060"
        }
      ]
    },
    "comparison": {
      "state": "complete",
      "mode": "three-way",
      "baseline": {
        "state": "available",
        "fingerprints": [
          {
            "path": ".agents/extensions/development-toolkit.md",
            "fingerprint": {
              "kind": "semantic",
              "policy": "open-forge-markdown-v1",
              "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
              "origin": "persisted-baseline"
            }
          }
        ]
      },
      "current": {
        "state": "available",
        "fingerprints": [
          {
            "path": ".agents/extensions/development-toolkit.md",
            "fingerprint": {
              "kind": "semantic",
              "policy": "open-forge-markdown-v1",
              "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
              "origin": "operation-time-current"
            }
          }
        ]
      },
      "intended": {
        "state": "available",
        "fingerprints": [
          {
            "path": ".agents/extensions/development-toolkit.md",
            "fingerprint": {
              "kind": "semantic",
              "policy": "open-forge-markdown-v1",
              "sha256": "a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c",
              "origin": "operation-time-intended"
            }
          }
        ]
      },
      "paths": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "baseline": {
            "kind": "semantic",
            "policy": "open-forge-markdown-v1",
            "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
            "origin": "persisted-baseline"
          },
          "current": {
            "kind": "semantic",
            "policy": "open-forge-markdown-v1",
            "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
            "origin": "operation-time-current"
          },
          "intended": {
            "kind": "semantic",
            "policy": "open-forge-markdown-v1",
            "sha256": "a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c",
            "origin": "operation-time-intended"
          },
          "relation": "changed",
          "baselineOwners": ["development-toolkit"],
          "currentOwners": ["development-toolkit"],
          "intendedOwners": ["development-toolkit"]
        }
      ],
      "dependencies": {
        "state": "available",
        "baseline": ["development-toolkit"],
        "current": ["development-toolkit"],
        "intended": ["development-toolkit"],
        "relation": "equal"
      }
    },
    "generated": {
      "state": "complete",
      "ownership": "derived-navigation-only",
      "regions": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "state": "absent",
          "startMarker": null,
          "endMarker": null,
          "startByteOffset": null,
          "endByteOffset": null,
          "excludedInteriorByteLength": 0,
          "markerLinesRetained": false
        }
      ]
    },
    "findings": [
      {
        "code": "extension-inspect.path-changed",
        "status": "attention",
        "subject": "development-toolkit",
        "packageId": "development-toolkit",
        "dependency": null,
        "path": ".agents/extensions/development-toolkit.md",
        "cause": "The intended source differs from the trusted unchanged current path.",
        "location": null,
        "candidates": []
      }
    ],
    "counts": {
      "installedPackages": 1,
      "availablePackages": 1,
      "declaredPaths": 1,
      "currentPaths": 1,
      "baselinePaths": 1,
      "intendedPaths": 1,
      "dependencies": 1,
      "unchangedPaths": 0,
      "changedPaths": 1,
      "currentDivergedPaths": 0,
      "missingPaths": 0,
      "newPaths": 0,
      "retiredPaths": 0,
      "sharedPaths": 0,
      "generatedRegions": 0,
      "excludedGeneratedBytes": 0,
      "findings": 1
    }
  },
  "next": {
    "command": "open-forge extension update development-toolkit",
    "reason": "Apply the trusted current-source change for this stable ID with the explicit update command."
  }
}
```

### Source-unavailable installed-only

```json
{
  "schemaVersion": 1,
  "command": "extension inspect",
  "status": "attention",
  "workspace": {
    "path": "/work/example",
    "selectedBy": "explicit-workspace"
  },
  "result": {
    "subject": {
      "supplied": "development-toolkit",
      "form": "stable-id",
      "id": "development-toolkit",
      "state": "resolved",
      "candidates": []
    },
    "source": {
      "supplied": "/missing/catalogue",
      "explicit": true,
      "identity": null,
      "kind": null,
      "state": "missing"
    },
    "lifecycle": {
      "documentPath": ".agents/open-forge.lifecycle.json",
      "readState": "complete",
      "trust": "trusted",
      "coverage": "complete",
      "workspaceBinding": "matched",
      "fingerprintPolicy": "open-forge-markdown-v1"
    },
    "installed": {
      "state": "present",
      "package": {
        "id": "development-toolkit",
        "version": "0.1.0",
        "source": "catalogue:/packages/open-forge",
        "dependencies": [],
        "paths": [".agents/extensions/development-toolkit.md"]
      }
    },
    "available": {
      "state": "unavailable",
      "package": null
    },
    "dependencies": {
      "state": "complete",
      "declared": [],
      "resolved": [
        {
          "id": "development-toolkit",
          "version": "0.1.0",
          "source": "catalogue:/packages/open-forge",
          "state": "available"
        }
      ],
      "order": ["development-toolkit"]
    },
    "pathFacts": {
      "state": "complete",
      "declared": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "sourcePath": null,
          "state": "available"
        }
      ],
      "current": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "state": "present",
          "physicalIdentity": "file:/work/example/.agents/extensions/development-toolkit.md",
          "byteLength": 6,
          "exactSha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060"
        }
      ]
    },
    "comparison": {
      "state": "complete",
      "mode": "installed-only",
      "baseline": {
        "state": "available",
        "fingerprints": [
          {
            "path": ".agents/extensions/development-toolkit.md",
            "fingerprint": {
              "kind": "semantic",
              "policy": "open-forge-markdown-v1",
              "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
              "origin": "persisted-baseline"
            }
          }
        ]
      },
      "current": {
        "state": "available",
        "fingerprints": [
          {
            "path": ".agents/extensions/development-toolkit.md",
            "fingerprint": {
              "kind": "semantic",
              "policy": "open-forge-markdown-v1",
              "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
              "origin": "operation-time-current"
            }
          }
        ]
      },
      "intended": {
        "state": "not-applicable",
        "fingerprints": []
      },
      "paths": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "baseline": {
            "kind": "semantic",
            "policy": "open-forge-markdown-v1",
            "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
            "origin": "persisted-baseline"
          },
          "current": {
            "kind": "semantic",
            "policy": "open-forge-markdown-v1",
            "sha256": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
            "origin": "operation-time-current"
          },
          "intended": null,
          "relation": "unknown",
          "baselineOwners": ["development-toolkit"],
          "currentOwners": ["development-toolkit"],
          "intendedOwners": []
        }
      ],
      "dependencies": {
        "state": "complete",
        "baseline": ["development-toolkit"],
        "current": ["development-toolkit"],
        "intended": [],
        "relation": "not-applicable"
      }
    },
    "generated": {
      "state": "complete",
      "ownership": "derived-navigation-only",
      "regions": [
        {
          "path": ".agents/extensions/development-toolkit.md",
          "state": "absent",
          "startMarker": null,
          "endMarker": null,
          "startByteOffset": null,
          "endByteOffset": null,
          "excludedInteriorByteLength": 0,
          "markerLinesRetained": false
        }
      ]
    },
    "findings": [
      {
        "code": "extension-inspect.source-unavailable",
        "status": "attention",
        "subject": "development-toolkit",
        "packageId": "development-toolkit",
        "dependency": null,
        "path": null,
        "cause": "The explicit package source is missing; installed lifecycle facts remain available.",
        "location": null,
        "candidates": []
      }
    ],
    "counts": {
      "installedPackages": 1,
      "availablePackages": null,
      "declaredPaths": 1,
      "currentPaths": 1,
      "baselinePaths": 1,
      "intendedPaths": null,
      "dependencies": 1,
      "unchangedPaths": null,
      "changedPaths": null,
      "currentDivergedPaths": 0,
      "missingPaths": null,
      "newPaths": null,
      "retiredPaths": null,
      "sharedPaths": null,
      "generatedRegions": 0,
      "excludedGeneratedBytes": 0,
      "findings": 1
    }
  },
  "next": null
}
```

## Fingerprint Golden Byte Vectors

These vectors use one exact notation: printable ASCII is shown literally and
every non-ASCII or control byte is shown as `\xNN` in byte order. The hash is
the lowercase SHA-256 of the normalized or exact-fallback bytes identified in
the `Output bytes` column. They are calculated vectors, not implementation
evidence.

| Vector                            | Input bytes                                                                                                                 | Treatment                        | Output bytes                                                                                                 | SHA-256                                                            |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------ |
| LF baseline                       | `alpha\x0A`                                                                                                                 | admitted Markdown                | `alpha\x0A`                                                                                                  | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| CRLF equals LF                    | `alpha\x0D\x0A`                                                                                                             | CRLF → LF                        | `alpha\x0A`                                                                                                  | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| Lone CR equals LF                 | `alpha\x0D`                                                                                                                 | lone CR → LF                     | `alpha\x0A`                                                                                                  | `b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060` |
| Authored whitespace               | `alpha\x20\x20\x0A`                                                                                                         | whitespace retained              | `alpha\x20\x20\x0A`                                                                                          | `a1d36921b09507031f6a0d2ecbda13dac0d41b20318f6138ccbf3f01907deb5c` |
| Authored final-newline difference | `alpha`                                                                                                                     | final newline not added          | `alpha`                                                                                                      | `8ed3f6ad685b959ead7022518e1af76cd816f8e8ec7ccdda1ed4018e8f2223f8` |
| Unicode                           | `caf\xC3\xA9\x0A`                                                                                                           | Unicode retained                 | `caf\xC3\xA9\x0A`                                                                                            | `7b49b9e063bd91a4f9252b413261f5557b9c570aa61516989499f64a62dbcdd6` |
| Leading BOM retained              | `\xEF\xBB\xBFalpha\x0A`                                                                                                     | BOM retained; no preamble added  | `\xEF\xBB\xBFalpha\x0A`                                                                                      | `9eff3bdb19b9bef9372b96a1244c0f0f939acdb8f539551e0a099a5b20a3862e` |
| Valid generated interior          | `## Entries\x0A\x0A<!-- open-forge:generated-index:start -->\x0A- generated\x0A<!-- open-forge:generated-index:end -->\x0A` | omit only interior               | `## Entries\x0A\x0A<!-- open-forge:generated-index:start -->\x0A<!-- open-forge:generated-index:end -->\x0A` | `2d253cbfbaea22ccffbc10d37e25d5ed52db632b6accf1698896b8d135c4e0e9` |
| Reversed boundary                 | `## Entries\x0A<!-- open-forge:generated-index:end -->\x0A- route\x0A<!-- open-forge:generated-index:start -->\x0A`         | invalid region; exact fallback   | same bytes                                                                                                   | `fa690b96fc34ae0c7fd05e3d4318d947b1d7eef4c33cc745ed8cc5cf4fae10bb` |
| Unsupported bytes                 | `{}\x0D`                                                                                                                    | unsupported kind; exact fallback | `{}\x0D`                                                                                                     | `f545623b541a21d6b8b415ee1793b91001a50ca985d26fad253c3c68aba5ffe9` |
| Binary NUL                        | `\x00a\x0D\x0A`                                                                                                             | binary/NUL; exact fallback       | `\x00a\x0D\x0A`                                                                                              | `c4cbb7cbfda0feb8dde8cd2e8abfb0771fc2c04fe50720acc3b83971937b8ac3` |
| Invalid UTF-8                     | `\xFF\x0D\x0A`                                                                                                              | invalid UTF-8; exact fallback    | `\xFF\x0D\x0A`                                                                                               | `1320b5dc13aa91dbac6eabc346cb655592aef8244a8ed04b8c4b3bdd59b8af4c` |
| Deterministic repeat, first run   | `repeat\x0Avalue`                                                                                                           | admitted Markdown                | `repeat\x0Avalue`                                                                                            | `d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38` |
| Deterministic repeat, second run  | `repeat\x0Avalue`                                                                                                           | admitted Markdown                | `repeat\x0Avalue`                                                                                            | `d9884573b6ea5e967e594532570e613d871fe38fc980df9ac05423e0c2559f38` |

The LF, CRLF, and lone-CR rows have equal output bytes and hashes. The
whitespace, Unicode, BOM, and final-newline rows demonstrate that no other
source bytes are normalized. The generated row retains both marker lines. The
reversed, unsupported, binary, and invalid-UTF-8 rows retain exact bytes,
including CR and NUL where present. Repeated input yields the same bytes and
hash.

## Non-Goals And Public Conformance

Inspect does not select a package from multiple IDs, resolve a semver update,
infer trust, reconstruct lifecycle state from files outside the exact lifecycle
document, invoke another public command, or write package, workspace,
lifecycle, generated-navigation, lock, backup, temporary, recovery, or
diagnostic state. It does not execute package content or access network,
registry, cache, ambient search, or legacy lifecycle input.

Conformance must demonstrate, with the same typed result for human and JSON:

- exact stable-ID grammar, one operand, singleton-source repetition, terminal
  modes, and all global-flag rules;
- exact CWD and `--workspace` selection, source package/catalogue
  classification, lexical and physical disjointness, aliases, containment, and
  no explicit-source fallback;
- installed-only, available-only, trusted three-way, valid empty-section,
  source-unavailable, malformed, ambiguous, unsupported, and event states;
- strict lifecycle schema-v1 reads, exact persisted fields, reciprocal package,
  dependency, path, owner, coverage, and fingerprint facts, including safe
  partial retention;
- offline one-source dependency closure, duplicate and cycle rejection, stable
  dependency-first order, package metadata, payload inventory, target paths,
  current physical identity, and exact-byte observations;
- every command-local member and nested member in frozen order, nullability,
  empty-array, known-zero, and unavailable-null rules;
- all 31 finding codes, one status per code, duplicate and unknown-value
  rejection, global ordering, within-code tie-breaks, aggregate precedence,
  safe fact retention, and the exhaustive `next` table;
- `open-forge-markdown-v1` admission, BOM/NUL/UTF-8 boundaries, fixed
  CommonMark validation, line-ending-only normalization, exact marker and
  generated-region treatment, fallback, fail-closed equivalence, persistence
  boundaries, and every golden byte vector;
- trusted complete actionable three-way recommendation gating, with no update
  recommendation for fallback, unavailable, blocked, invalid, or event states;
- human compact and expanded output, JSON parity, one JSON document per status,
  stream and process-exit mapping, bounded diagnostics, no prompts, repeat
  determinism, and recursive unchanged-state/no-write checks; and
- supported source-generated serialization and Native-AOT/process evidence at
  Gate 5, without treating this contract or its examples as proof.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md) own
the envelope, source-location shape, and status exit map. The [CLI
Architecture](../../../architecture.md) owns parser/runtime boundaries and
system-level evidence. This Interface owns the Inspect result, finding,
fingerprint, scenario, and `next` meanings above.

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The result retains every member except supporting comparison fingerprints.
Comparison baseline/current/intended sides retain state but omit fingerprints.
Comparison paths retain path, relation and baseline/current/intended owners,
while omitting their baseline/current/intended fingerprint objects. Comparison
state, mode, dependency comparison, all path facts, available package contents,
findings and counts remain. Omitted fingerprints do not imply unavailable or
equal content.

Compact omissions are defined field membership, distinct from unavailable data,
null values, empty collections or incomplete inspection. No collection is
truncated and no finding is filtered. Counts describe the original operation.
Select expanded on the original invocation when supporting evidence is needed.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
