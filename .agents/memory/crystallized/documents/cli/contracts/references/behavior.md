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

The operation is read-only, stateless, and non-shipping. Implementation and
executable proof remain pending Gate 5. It has no mutation phase,
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

## Outgoing Occurrence Collection

Inspect the selected logical source's authored direct references in base-then-
overwrite order and authored occurrence order within each physical layer. Record
every occurrence, including local references that do not resolve cleanly and
external HTTP/HTTPS destinations.

For a local occurrence, retain the raw authored destination, fragment when
present, source location, direction `out`, source layer/path, target kind,
resolved local ID/path when available, and resolution/containment status. A
contained local target outside `.agents` has no automatic ID but retains its
canonical workspace-relative path.

For an HTTP or HTTPS occurrence, record the raw URL, direction `out`, target kind
`external`, null local ID and path, `external-unchecked`, and
`network-not-attempted`. Never fetch, follow, or network-check that URL. The
unchecked fact is not by itself an attention or incomplete finding when the
outgoing source layers were otherwise inspected completely.

Do not make a discovered target a new traversal seed. Do not report generated
`Entries`, route edges, context-loading edges, or semantic relationships as
outgoing reference occurrences.

## Incoming Occurrence Collection

For incoming work, inspect the complete effective source universe and collect every
direct authored occurrence whose local destination resolves to the selected
logical source. Preserve duplicate occurrences, fragments, source location,
physical layer/path, raw destination, target resolution, and provenance. A base
and overwrite occurrence remains separately attributable even when both resolve
to one logical target.

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
location, raw destination, direction, target identity/path when local, target
kind, fragment where present, resolution/containment status, and provenance.

Broken, unsupported, unsafe, or ambiguous local destinations are not silently
omitted. When safe to retain, the occurrence remains visible with the applicable
finding and section status. If the operation cannot establish a safe boundary,
the section or aggregate becomes blocked. A safe occurrence set beside an
uncompleted scan or unresolved layer becomes incomplete rather than complete.

## Coverage And Aggregate Result

Form one typed result containing the requested direction, selected logical source,
effective incoming filter evidence when applicable, separate Incoming and
Outgoing sections, every safe occurrence, per-section coverage/status, findings,
and aggregate status.

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
overwrite and authored document/occurrence order. Incoming uses deterministic
source/layer/path/location order while preserving every physical occurrence.

Compact rendering groups only requested sections, reports per-section coverage,
and places direct occurrences under `Level 1`. Expanded rendering adds
directional pointers, locations, raw destinations, resolution and target facts,
layers, scan evidence, and provenance. JSON retains the complete typed sections,
occurrences, coverage, and findings. `--view` cannot change JSON.

## Safety And Recovery

- Do not write authored files, generated navigation, indexes, receipts, or caches.
- Do not load target bodies into context or call `context --follow-links`.
- Do not fetch external URLs or make network state part of completion.
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
7. Local contained targets, local targets outside `.agents`, missing targets,
   fragments, unsupported or ambiguous destinations, and external HTTP/HTTPS
   facts are all represented without a network attempt.
8. An otherwise complete outgoing result containing external-unchecked facts is
   not attention or incomplete solely because network access was not attempted.
9. One-hop behavior does not follow targets, merge cycles, load content bodies,
   build a graph, or apply a hidden result cap.
10. Compact, expanded, and JSON have identical section membership, occurrence
    identity, order, and coverage; JSON ignores `--view`.
11. The final `contracts/references/_references.md` physical source is processed once.
12. Repeated unchanged invocations are semantically identical and incomplete,
    blocked, failed, or interrupted scans never appear complete.

## Related Current Sources

- [references Interface Contract](interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [Source Universe Filters Behavior Contract](../shared/source-universe-filters/behavior.md)
- [CLI Architecture](../../architecture.md)
