---
open-forge:
  description: Technology-neutral behavior and conformance for direct incoming and outgoing reference inspection
  responsibility: Define references resolution, one-hop occurrence collection, scan coverage, safety, result formation, and verification
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, References, Behavior, Links, CurrentTruth]
---

# references Behavior Contract

## Status And Authority

This is the accepted current Crystallized Behavior Contract for
`open-forge references`. It defines how a conforming implementation resolves one
logical source, records direct incoming and outgoing occurrence facts, establishes
per-section coverage, and forms one typed result without selecting a runtime,
parser, network client, index, or storage technology. The [Interface Contract](interface.md)
owns public meaning. The shared [Source References](../shared/source-references/interface.md),
[Global CLI Flags](../shared/global-flags/interface.md), and [Source Universe
Filters](../shared/source-universe-filters/behavior.md) contracts own their shared boundaries.

The operation is read-only, stateless, and non-shipping. Implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md). It has no mutation phase,
repair authority, network side effect, persistent reverse index, receipt, cache,
or hidden context or doctor invocation.

## Conformance Model

A conforming execution follows this relationship:

```text
validated request
  -> exact workspace and logical source/layer facts
  -> requested direction and effective incoming universe
  -> direct occurrence inspection
  -> per-section facts, coverage, findings, and status
  -> aggregate typed result
  -> human or JSON projection of that same typed result
```

The operation is one hop. A target discovered in one occurrence is a fact, not a
new scan seed. Human view selection cannot add, remove, merge, reorder, or
re-resolve occurrences.

## Request Validation And Workspace

1. Apply shared terminal handling for `--help` and `--version` before domain
   inspection.
2. Resolve the exact current directory or exact `--workspace` directory. Do not
   search parent directories or infer a workspace from the source operand.
3. Require exactly one source operand and resolve it with the shared source-ID,
   exact-path, containment, quoting, collision, and overwrite rules.
4. Parse one exact direction value. Omission becomes `both`; accept only `in`,
   `out`, or `both`. Reject aliases, Boolean trim flags, repeated direction
   values, and malformed values.
5. Validate `--include` and `--exclude` against the requested direction. They are
   valid for `in` and `both`, apply to incoming work only, and are invalid for
   `out`.

Unknown or malformed input is `invalid`. Unsafe paths, ambiguous IDs, ambiguous
targets, or an unsafe scan boundary are `blocked` under the shared meanings.

## Logical Source And Layers

Resolve the required operand to one logical local source. If it names a base or a
valid adjacent overwrite, retain the shared logical identity and both physical
layer facts.

For outgoing inspection, inspect base content first and overwrite content second.
Record each direct occurrence against the physical layer and canonical path where
it was authored. Do not emit the overwrite as a separate source or collapse its
occurrences into the base without layer evidence.

For incoming inspection, inspect every eligible physical source layer in the
effective source universe. Preserve the source's logical ID where available, its
physical layer and canonical path, and its authored location. When a destination
names an overwrite target, resolve the logical target while retaining that
targeted physical layer as evidence. An orphan or ambiguous overwrite is not a
valid independent source or target; retain the applicable invalid, blocked, or
incomplete boundary.

The final `references` route recognizes `_references.md` by physical identity.
That recognized entrypoint is processed once, not once per compatible spelling or
traversal path. This is a required physical-identity regression for Gate 5
executable proof.

## Direction Selection And Work

Omitted direction requests both sections. Explicit `in` evaluates only the
incoming scan; explicit `out` evaluates only the selected source's outgoing
layers; `both` evaluates both. A section excluded by direction is absent from the
typed result and is not evaluated, scanned, or counted toward aggregate coverage.

Include/exclude filters are passed unchanged to the shared Source Universe
Filters contract for incoming work. With no filter flags, use the normal default
of all eligible `.agents` Markdown sources. With filters, use the effective
universe produced by the shared union and exclusion-precedence rules. With both,
the filtered universe affects incoming only; outgoing remains the full selected
source layer read. With out, reject the filters before scanning.

## Authored Link Extraction

Extraction uses the one accepted fixed CommonMark/Markdig parser and its typed
Markdown nodes. It does not add a regular-expression scan, a second parser, or a
route-specific interpretation. For each inspectable physical layer, the
implementation:

1. establishes the generated-region boundary before admitting authored link
   nodes;
2. parses the authored bytes through the fixed parser;
3. visits parser-produced non-image inline links, reference-style links, and
   explicit autolinks in authored document order; and
4. converts each admitted node and destination to the command's typed occurrence
   facts before target resolution.

The parser boundary excludes image and other resource-embed nodes, raw HTML
links, code spans, code fences, and bare URL text that is not a parser-produced
link. The extractor does not search those regions again after parsing. A link
inside a generated `Entries` interior is excluded only by the valid-region rule
below. Excluded nodes produce neither occurrences nor findings.

### Generated-Region Boundary

For one physical layer, the extractor may exclude a generated interior only when
the Framework Markdown contract establishes exactly one valid bounded generated
`Entries` region for that layer. The boundary check is separate from link-node
extraction and does not treat generated entries as authored route references.

If the region is malformed, duplicated, misplaced, or unsafe, the extractor does
not choose a marker pair or suppress the links it might contain. It records
`references.generated-region-unavailable` and retains parser-produced link facts
whose byte boundaries are safely established outside any selected exclusion.
When the source bytes and a safe no-suppression boundary remain established, the
section is incomplete. When no safe exclusion boundary can be established, the
section is blocked. No generated-region condition is converted into a complete
scan merely because some entries can be parsed.

The rule is per physical layer. A valid base region does not exclude links from
the overwrite, and an invalid region in one layer does not invalidate an
independently established region in another layer. The extractor never infers a
generated region from a heading, a link shape, or generated-entry content alone.

### Occurrence Spans And Authored Text

The extractor records the exact source bytes and Unicode-scalar coordinates for
the complete link-use node as `location`. It records a separate exact
`destinationLocation` when the parser exposes the destination span. A
reference-style link keeps the use span as `location` and maps the destination
definition's span separately. Inline and explicit-autolink nodes use their exact
destination span when available. The implementation maps spans to the [Shared
Result Coordinates](../shared/result-coordinates/interface.md) source-location
primitive rather than exposing parser-native
spans; it retains no occurrence whose link-use location cannot be established.

`rawDestination` is the authored destination payload, not a normalized or
reconstructed path. The extractor preserves spaces, Unicode, duplicate spelling,
percent escapes, and authored fragment text. For a reference-style link, it
uses the destination from the parser-resolved definition while keeping the use
and definition spans separate. It retains fragment text separately without
discarding it from `rawDestination`.

## Outgoing Occurrence Collection

Inspect the selected logical source's admitted direct link nodes in base-then-
overwrite order and authored occurrence order within each physical layer. Record
every admitted occurrence, including local references that do not resolve cleanly,
HTTP/HTTPS destinations, and unsupported non-HTTP schemes.

For a local occurrence, retain the raw authored destination, fragment when
present, link-use location, nullable destination location, direction `out`,
source layer/path, target kind, resolved local ID/path when available, and
resolution/containment status. A contained local target outside `.agents` has no
automatic ID but retains its canonical workspace-relative path. Preserve every
duplicate and every base/overwrite occurrence; do not deduplicate by destination,
target identity, or authored text.

For an HTTP or HTTPS occurrence, record the raw URL, direction `out`, target kind
`external`, null local ID and path, `external-unchecked`, and
`network-not-attempted`. Never fetch, follow, or network-check that URL. The
unchecked fact is not by itself an attention or incomplete finding when the
outgoing source layers were otherwise inspected completely.

For a non-HTTP external scheme, record an occurrence with target kind
`unsupported`, null local identity, path, and layer, and resolution `unsupported`.
Retain `references.destination-unsupported` as the typed finding. Do not resolve,
follow, or reinterpret the scheme as a local path.

Do not make a discovered target a new traversal seed. Do not report generated
`Entries`, route edges, context-loading edges, or semantic relationships as
outgoing reference occurrences.

## Direct Target Resolution

Target resolution is a bounded fact about the admitted occurrence. It does not
create a second outgoing scan, load target content into context, or follow a
target's links. A bounded target read may establish local existence, physical
containment, readability, and the authored fragment fact required by the
occurrence resolution. No target body is returned or projected.

Classify the authored destination before resolving it:

- An HTTP or HTTPS scheme is `external` with `external-unchecked` resolution and
  `network-not-attempted`.
- Another external scheme is `unsupported` with `unsupported` resolution and a
  `references.destination-unsupported` finding.
- A fragment-only destination uses the containing physical layer as its local
  target path and checks the fragment against that target without changing the
  one-hop boundary.
- A contained relative local destination resolves from the containing physical
  layer's directory. Spaces and Unicode remain part of the authored and
  canonical path facts.
- An absolute local form, a local query form outside the accepted destination
  grammar, or another unparsable local destination remains a local occurrence
  with resolution `absolute`, `query`, or `malformed` as applicable and a
  `references.destination-malformed` finding.
- An unsupported encoding that prevents exact local identity has resolution
  `encoding-unsupported` and a `references.invalid-encoding` finding.
- A path outside the selected workspace or a physical containment mismatch has
  resolution `outside-workspace` or `physical-escape` and a
  `references.target-unsafe` finding.
- More than one local target identity has resolution `ambiguous` and a
  `references.target-ambiguous` finding.
- An unreadable target needed for local resolution has resolution `unreadable`
  and a `references.target-unreadable` finding.
- A safely contained path with no existing target has resolution `missing` and a
  `references.target-missing` finding. An existing target without the authored
  fragment has resolution `fragment-missing` and a
  `references.fragment-missing` finding.
- An existing, safely contained, unambiguous target with an available fragment
  fact has resolution `complete`.

Target kind remains `local` for local path forms even when their local resolution
is malformed, unsafe, ambiguous, missing, or unreadable. `target.id` is present
only for an existing unambiguous `.agents` logical source. `target.path` is the
canonical contained workspace-relative path whenever it can be established,
including a missing path. `target.layer` identifies an existing physical base or
overwrite target when available. External and unsupported targets have all three
local target identity members null. No resolution state is inferred from a URL
fetch or from target relevance.

## Incoming Occurrence Collection

For incoming work, inspect the complete effective source universe and collect every
admitted direct occurrence whose local destination resolves to the selected
logical source. Preserve duplicate occurrences, fragments, link-use and
destination locations, physical layer/path, raw destination, target resolution,
and provenance. A base and overwrite occurrence remains separately attributable
even when both resolve to one logical target. A resolved target with a missing
fragment still contributes an incoming occurrence because its local target file
is established; the fragment finding remains attached to the result.

Malformed, unsupported, unsafe, ambiguous, or unreadable nonmatching destinations
do not become incoming occurrence rows, but their safe typed findings remain in
the result when the scan can establish their source and location. External URLs
do not become incoming local matches and do not produce a no-fetch finding.

Incoming ordering is deterministic by canonical source identity and physical
layer/path/location evidence. It must not depend on filesystem enumeration,
generated navigation order, modification time, or a persistent reverse index.

An empty incoming set is `complete` only after the effective universe is completely
inspected. If scan coverage stops early, safe matches may be returned but the
section is incomplete and cannot be rendered as a complete empty result.

External URLs do not become incoming local matches for this source. The operation
does not fetch them or use network results to decide incoming coverage.

## Occurrence Facts And Resolution Findings

Every occurrence retains typed facts for source identity/path, source layer/path,
link-use location, destination location when applicable, raw destination,
direction, target identity/path when local, target kind, fragment where present,
resolution/containment status, and provenance. `level` is always the integer `1`.
The occurrence's source and target facts preserve physical base/overwrite layers
without turning a layer into a second logical source.

Broken, unsupported, unsafe, or ambiguous local destinations are not silently
omitted. When safe to retain, the occurrence remains visible with the applicable
finding and section status. If the operation cannot establish a safe boundary,
the section or aggregate becomes blocked. A safe occurrence set beside an
uncompleted scan or unresolved layer becomes incomplete rather than complete.

Finding construction uses the exact Interface vocabulary. Invalid request,
source, direction, and filter facts are `invalid`. Workspace, source, selector,
and target safety or ambiguity are `blocked`. Established identity collisions
and malformed, unsupported, missing, or fragment-missing destinations are
`attention`. Candidate, layer, inspection, encoding, generated-region, and
target-read boundaries are `incomplete`, except that an unavailable generated
region is `blocked` when no safe exclusion boundary exists. Operation failure and
interruption retain their event statuses. HTTP/HTTPS no-fetch is never a
finding.

Each finding retains its code, mapped status, cause, applicable direction,
selector coordinates, source/layer/path evidence, applicable occurrence spans,
and candidate identities using the exact nullable Finding shape. A source or
section may have several independent findings, but one established issue uses
the most specific applicable code rather than a second synonymous finding.

## Coverage And Aggregate Result

Form one typed result containing the requested direction, selected logical source,
effective incoming filter evidence when applicable, separate Incoming and
Outgoing sections, every safe occurrence, per-section coverage/status, findings,
and aggregate status.

Result construction follows the exact schema-v1 command-local shape in the
Interface. It always forms the nullable selected logical `source`, nullable
effective requested direction, nullable incoming-selection object, nullable
direction sections, and ordered finding array. Invalid, ambiguous, or unsafe
source input leaves `source` null and retains the attempted operand and candidates
in findings. Invalid direction input leaves `requestedDirection` and both
sections null. Once an effective direction is established, each requested section
is present even when its work is blocked before occurrence inspection; an
unrequested direction is null. A present section uses only `complete`,
`incomplete`, or `blocked` coverage, keeps its occurrence count equal to its
occurrence array length, and may report `attention` status while coverage is
complete.

The result builder receives already resolved selector occurrences, physical
source-layer evidence, parsed occurrence facts, and findings. It does not rerun
source enumeration, parsing, target resolution, or network work for a renderer.
It retains selector duplicates in `incomingSelection.supplied`, deduplicates
logical identities only in `effectiveSources`, preserves every physical
occurrence, and serializes all nullable members and arrays even when empty.
After aggregate status and finding order are fixed, the result builder selects the
one exact command-local next action from the Interface table. Ambiguous source or
selector correction precedes the general blocked action only when its finding is
the first blocked finding. Complete results have no next action. Renderers consume
the selected action without changing its command or reason.

- `complete` requires every requested section to complete its declared work and
  have no unresolved finding that changes the result. With both directions,
  both sections must meet that condition. A complete empty incoming set is valid
  after a complete scan.
- `attention` is allowed when requested coverage is complete but a safe,
  non-blocking authored-form or identity finding remains. External no-fetch is
  not such a finding by itself.
- `incomplete` means safe facts exist but a requested incoming scan, layer read,
  or local-resolution boundary did not complete.
- `invalid`, `blocked`, `failed`, and `interrupted` retain their shared meanings.

An excluded direction is absent and contributes no coverage or status. A complete
outgoing section cannot make an incomplete incoming section, or an incomplete
aggregate, appear complete.

No result cap, transitive depth, graph merge, diagnosis, repair, body projection,
network check, or persistent reverse index may be introduced as an implicit
behavior.

## Ordering And Presentation

The typed occurrence order is fixed before rendering. Outgoing uses base then
overwrite and authored document/occurrence order. Incoming uses canonical source
identity, canonical physical path, base then overwrite, and exact link-use
location order while preserving every physical occurrence. A destination-location
byte offset is the final location tie-break. All ordering is ordinal and
independent of filesystem enumeration, generated navigation, modification time,
parser discovery timing, or exception order.

Finding order is fixed by the Interface finding-code table. Within one code,
request facts precede selector facts, then incoming before outgoing facts, followed
by canonical source identity, physical path, layer, location, and destination
location. Include selectors precede exclude selectors, and selector occurrences
retain their role-local 1-based number. Failed and interrupted event findings are
last. This order is fixed before either renderer runs.

Human rendering names the one-hop boundary “Direct links” and retains every
physical occurrence in the requested sections. Both views report per-section
coverage and status, source/target identity, line/column, authored destination,
resolution and source layer. Findings precede occurrence rows. Expanded adds
fragment and destination coordinates, target layer and the actual scan origin
in plain language. It does not repeat byte ranges outside JSON. JSON retains the complete typed sections,
occurrences, coverage, and findings. `--view` cannot change JSON.

## Safety And Recovery

- Do not write authored files, generated navigation, indexes, receipts, or caches.
- Do not load target bodies into context or call `context --follow-links`.
- Do not fetch external URLs or make network state part of completion.
- Do not treat reference-style compatibility input as canonical route syntax, and
  do not add a competing Markdown parser or post-parse text scan.
- Do not choose an ambiguous source or target by path order, generated order, or
  likely intent.
- Preserve safe occurrence facts when a later scan or layer becomes incomplete,
  and report the missing boundary.
- Preserve `interrupted` when cancelled; never convert a partial scan to complete.

## Conformance Scenarios

A conforming implementation should prove at least these observable scenarios:

1. Omitted direction returns separate incoming and outgoing sections; explicit
   `in` or `out` removes the other section from work and accounting.
2. Repeated or aliased direction values are invalid, and include/exclude with
   `out` is invalid.
3. The default incoming universe is all eligible `.agents` Markdown; shared
   include/exclude selectors form unions with exclusion precedence, and a
   complete empty result follows only a complete effective scan.
4. With both, filters change incoming work only; outgoing still reads the full
   selected source base then overwrite.
5. Outgoing preserves base/overwrite layer, path, location, document order, and
   duplicate occurrences.
6. Incoming preserves scanning source layer/path/location and resolves a raw
   overwrite target to its logical source with targeted-layer evidence.
7. Extraction reports parser-produced inline, reference-style, and explicit
   autolink nodes, while image/resource embeds, raw HTML links, code spans,
   code fences, and parser-unrecognized bare URL text produce no occurrence.
8. Reference-style occurrences preserve distinct link-use and destination
   definition locations; inline and explicit-autolink occurrences preserve exact
   destination spans when available; all locations map to the shared
   result-coordinate primitive.
9. A valid bounded generated `Entries` interior is excluded exactly once per
   physical layer, while malformed, duplicate, misplaced, or unsafe regions do
   not suppress links and produce incomplete or blocked coverage according to the
   safe-boundary rule.
10. Local contained targets, local targets outside `.agents`, fragment-only links,
    every declared local resolution, non-HTTP unsupported schemes, and external
    HTTP/HTTPS facts are represented with no network attempt.
11. An otherwise complete outgoing result containing external-unchecked facts is
    not attention or incomplete solely because network access was not attempted.
12. Raw spaces, Unicode, fragments, duplicate authored occurrences, and
    base/overwrite layer evidence survive extraction, resolution, incoming scan,
    ordering, and JSON serialization.
13. One-hop behavior does not follow targets, merge cycles, load target bodies
    into context, build a graph, or apply a hidden result cap.
14. Compact, expanded, and JSON have identical section membership, occurrence
    identity, order, locations, findings, and coverage; JSON ignores `--view`.
15. The exact schema-v1 result always includes its ordered members, nullable
    values, arrays, finite values, selector duplicates, and source-layer evidence;
    each finding code has its declared status and aggregate effect.
16. The candidate `contracts/references-candidate/_references-candidate.md`
    remains staged until the replacement `index` command validates the final
    compatibility-name path `contracts/references/_references.md` and proves
    that physical source is processed once.
17. Repeated unchanged invocations are semantically identical and incomplete,
    blocked, failed, or interrupted scans never appear complete.

## Related Current Sources

- [references Interface Contract](interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [Source Universe Filters Behavior Contract](../shared/source-universe-filters/behavior.md)
- [CLI Architecture](../../architecture.md)
