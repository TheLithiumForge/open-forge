---
open-forge:
  description: Technology-neutral behavior and conformance for deterministic routed-topology enumeration
  responsibility: Define route-list resolution, authority, coverage, safety, ordering, result formation, and verification
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, List, Behavior, Topology, CurrentTruth]
---

# route list Behavior Contract

## Status And Authority

This is the accepted current Crystallized Behavior Contract for `open-forge route
list`. It defines how a conforming implementation resolves and reports the
Interface Contract without selecting a runtime, parser, library, storage model,
or generated-index implementation. The [Interface Contract](interface.md) owns
the public vocabulary and observable shape. The shared [CLI Source References](../../shared/source-references/interface.md)
and [Global CLI Flags](../../shared/global-flags/interface.md) contracts own
their shared input and presentation meaning.

The operation is read-only, stateless, and non-shipping. Implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). Its behavior ends at one typed result;
it has no mutation phase, receipt, cache, persistent reverse index,
or hidden `index` invocation.

## Conformance Model

A conforming execution follows this technology-neutral relationship:

```text
validated request
  -> exact workspace and authored topology facts
  -> root and subject resolution
  -> bounded route closure
  -> typed rows, coverage, findings, and semantic result
  -> human or JSON projection of that same typed result
```

Human view selection cannot rerun resolution, change membership, alter order,
drop coverage evidence, or turn an incomplete result into a complete one.

## Request Validation And Workspace

1. Apply the shared terminal handling for `--help` and `--version` before domain
   execution.
2. Resolve the exact current directory or exact `--workspace` directory under the
   shared global contract. Do not search parent directories, Git roots, marker
   files, or nearby workspaces.
3. Parse at most one source operand and one scalar `--depth` value. Reject unknown
   depth spellings, negative values, non-integers, empty values, and repeated
   depth values.
4. Resolve source references with the shared exact-ID and exact-path rules,
   including collisions, quoting, containment, and overwrite identity.
5. Reject the Loader as a route subject, reject an unrouted source as a route-list
   subject, and block unsafe or structurally ambiguous route meaning.

Invalid input produces `invalid`; an unresolved safe boundary produces `blocked`.
No route facts are used to silently repair an invalid request.

## Authored Topology Is Authority

After validation, inspect the current authored filesystem and the source contracts
that establish route meaning. The resolver must derive route membership, parentage,
root exposure, child relationships, metadata, and route depth from those facts.

Generated `Entries` are not an input authority for this operation. They may be
stale, incomplete, reordered, or contain lines that do not match current authored
topology. They cannot add, hide, reparent, or reorder a route-list row. The
operation does not regenerate or edit them.

The exact Loader is authoritative for the operand-free root set. Read its current
exposure mechanically, including workspace-defined roots. Do not substitute a
hardcoded standard-root list. Do not emit the Loader as a route row. If the
Loader-root boundary cannot be established safely, report the applicable blocked
or incomplete result rather than inventing roots.

For operand-free selection only, the Loader's current direct route declarations
establish root membership. This narrow boundary does not make their order
canonical and does not let any generated `Entries` interior establish descendant
membership, parentage, metadata, depth, or order.

## Subject And Root Resolution

### No Operand

With no source operand, select every current root entrypoint directly exposed by
the exact Loader. Each selected root is a root of the result and has relative
depth `0`. Detached authored entrypoints not exposed by that Loader are not
selected.

### Explicit Entrypoint

With one routed entrypoint operand, resolve its logical source, retain its own
route facts, and traverse its routed descendants. The selected entrypoint has
relative depth `0`. Its actual routed parent and absolute depth are retained from
its current authored route tree even when ancestors are outside the selected row
set.

An explicitly selected detached entrypoint may be a valid root for a local
subtree. The result must preserve detached provenance and must not fabricate a
Loader parent, absolute Loader-rooted depth, or Loader-rooted authority. Its
parent and absolute-depth fields are null when no local authored parent is
mechanically established.

### Explicit Leaf And Overwrite

A routed leaf is a valid subject and produces that leaf only, at relative depth
`0`, regardless of the requested descendant depth. A base or valid overwrite
operand resolves to the same logical base route. The overwrite layer is evidence
for that logical row, not a second row.

An unrouted source is `invalid`. Unknown references follow the shared invalid
meaning. Unsafe paths and ambiguous IDs or route structures are `blocked`.

## Depth And Closure

Depth counts routed parent-child edges. For each selected root:

- Omitted depth is `1`, so include the root and direct routed children.
- `0` includes the root only.
- A non-negative integer `N` includes descendants through relative depth `N`.
- `all` continues until the complete local routed descendant closure is known.

The selected root is relative `0` even when its absolute route depth is nonzero.
For Loader-rooted trees, absolute depth counts from the current tree root. For a
detached tree without established Loader ancestry, absolute depth is null and
relative depth counts from the explicitly selected local root.

Do not use filesystem directory depth, Markdown heading depth, generated-entry
position, ordinary-link reachability, or source size as route depth. Do not stop
because a convenient output size has been reached. If the requested closure is
not fully inspectable, retain confirmed safe rows and report `incomplete` with the
missing boundary.

## Enumeration And Row Facts

Enumerate all routed entrypoints and routed leaves in the selected closure,
including a routed native source such as `SKILL.md` when its source contract
establishes route metadata. Exclude unrouted sources and independent overwrite
rows.

An ordinary routed leaf is a supported direct sibling source with valid Open
Forge metadata. Its membership does not depend on a current generated entry. A
child folder participates only through exactly one recognized entrypoint, so a
file below an unrepresented intermediate folder is not a routed descendant.

For each row, retain typed facts for automatic ID, canonical path, parent and
hierarchy, absolute depth, relative depth, kind, exact authored description,
exact authored tags, applicable direct-child count, and provenance/coverage.
Do not normalize, inherit, infer, or regenerate the description or tags. A
malformed or unavailable authored fact remains a finding attached to the safe
row or coverage rather than being replaced with a guessed value.

For entrypoints, direct-child count is calculated from the same authored routed
child set used for enumeration. A routed leaf has no routed children; its result
must not acquire a synthetic child or a misleading generated-entry count.

## Canonical Ordering

Order rows parent before child. Within equivalent structural positions, use a
stable canonical route/path ordering derived from authored identity. The ordering
must be independent of filesystem enumeration, generated `Entries` order,
modification time, traversal accident, or semantic relevance.

The typed order is established once and reused by compact, expanded, and JSON
renderers. Multiple selected Loader roots retain their deterministic root order;
each root's descendants remain after their parent and within its topology.

## Result Formation And Coverage

The operation produces one typed result containing workspace selection, requested
subject or operand-free root selection, requested/effective depth, ordered rows,
coverage, findings, and semantic status.

- `complete` requires complete establishment of every requested root and every
  requested depth row. A complete empty result is valid after complete inspection.
- `attention` is allowed only when coverage remains complete and the finding is
  safe and non-blocking, such as an authored-form or identity finding that does
  not remove or add route coverage.
- `incomplete` means safe rows exist but requested topology or closure coverage is
  not fully established. The result states what boundary remains unknown.
- `invalid`, `blocked`, `failed`, and `interrupted` retain their shared meanings.

Missing or malformed required route metadata is `incomplete`. Unsafe identity or
ambiguous route structure is `blocked`. Cancellation is `interrupted`, retains
already confirmed safe rows when available, and never reports complete coverage.
Cancellation observed after result formation does not replace the completed
result.

No row cap, minimal mode, metadata predicate, graph query, or semantic inference
may alter the requested closure. If a later operational limit is ever required,
it must be represented as an explicit incomplete boundary rather than a false
complete result; this contract defines no such limit.

## Presentation Invariants

Compact human output includes the result, coverage, selected roots, effective
depth, count, and deterministic indented rows with ID, canonical path, exact
authored description, and exact authored tags. Expanded output adds explicit
parent, absolute and relative depths, kind, applicable child count, provenance,
coverage evidence, and explanation.

JSON retains the complete typed result and all route rows regardless of human
view. `--view` is a presentation no-op under JSON. `--verbose` may add bounded
diagnostics but cannot change the typed result.

## Safety And Recovery

- Never write authored files, generated `Entries`, backups, receipts, indexes, or
  caches.
- Never run a hidden indexing or repair operation.
- Never choose an ambiguous route by generated order, file kind, modification
  time, or likely intent.
- Never claim a Loader root for a detached tree.
- Preserve safe confirmed rows when a later boundary becomes incomplete, while
  clearly reporting the incomplete status and next action.
- Preserve `interrupted` when the caller stops execution; do not convert a partial
  result into `complete`.

## Conformance Scenarios

Gate 5 executable proof should cover at least these observable scenarios:

1. A Loader with standard and workspace-defined roots returns every current root,
   never hardcodes only the standard set, and never emits the Loader.
2. A detached entrypoint is absent from operand-free selection but returns its
   explicit local subtree with detached provenance.
3. A routed entrypoint returns itself plus direct children by default, roots only
   at `0`, and its complete closure at `all`; a routed leaf remains one row.
4. A valid overwrite operand resolves one base row, while an orphan or ambiguous
   overwrite cannot become an independent route.
5. A routed `SKILL.md` and ordinary routed leaf are included; an unrouted source
   is invalid.
6. Stale generated `Entries` cannot add, hide, or reorder rows.
7. Exact descriptions and tags, parent facts, both depth values, kinds, child
   counts, and provenance remain identical across repeated invocations.
8. Compact, expanded, and JSON use identical row membership, order, and coverage;
   JSON ignores `--view`.
9. Unsafe and ambiguous references block, invalid depth and source kinds are
   invalid, and incomplete topology never reports `complete`.

## Related Current Sources

- [route list Interface Contract](interface.md)
- [Route group entrypoint](../_route.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [CLI Architecture](../../../architecture.md)
