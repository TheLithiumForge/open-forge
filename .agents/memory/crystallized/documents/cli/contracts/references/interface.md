---
open-forge:
  description: Current public interface and observable result for direct incoming and outgoing reference facts
  responsibility: Define the public grammar, direction and scan filters, occurrence facts, views, results, errors, and verification for `references`
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, References, Interface, Links, CurrentTruth]
---

# references Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge references`. It owns the public purpose, exact grammar, direction
selection, incoming scan universe, occurrence facts, overwrite evidence, human
and structured presentation, semantic results, errors, examples, non-goals, and
caller-visible verification. The command does not ship yet; implementation and
executable proof remain pending Gate 5.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral resolution, scan coverage, safety, and conformance. The
[Source References](../shared/source-references/interface.md) contract owns the
source operand grammar, IDs, exact paths, quoting, collisions, and overwrite
identity. The shared [Global CLI Flags](../shared/global-flags/interface.md)
contract owns workspace selection, presentation, diagnostics, and terminal
flags. The reusable [Source Universe Filters](../shared/source-universe-filters/interface.md)
contract owns the complete include/exclude expansion, union, and exclusion
precedence used by incoming scans.

## Purpose And Boundary

`references` answers two direct, one-hop questions for one selected source:

- Which authored reference occurrences leave this source?
- Which eligible source layers contain authored references to this source?

The result keeps outgoing and incoming evidence in separate sections. It reports
where an occurrence was authored, what destination was written, how a local
destination resolved, and what coverage was actually established.

The command is read-only, stateless, and non-shipping. It does not load target
bodies into context, expand a context selection, modify files, fetch a URL, build
a persistent reverse index, diagnose a workspace, or repair a reference.

## Syntax

The complete public command form is:

```text
open-forge references <source-reference>
  [--direction=in|out|both]
  [--include=<source-reference>]...
  [--exclude=<source-reference>]...
  [global flags]
```

There is exactly one required source operand. It uses the shared source-reference
grammar and identifies the logical local source whose outgoing references are
read and whose incoming references are sought.

### Operands And Flags

| Input                          | Role                                                                  | Accepted value                                | Omission                         | Repetition and composition                                                              |
| ------------------------------ | --------------------------------------------------------------------- | --------------------------------------------- | -------------------------------- | --------------------------------------------------------------------------------------- |
| `source-reference`             | Select the logical local source                                       | One shared source ID or exact `.agents` path  | None; omission is invalid        | Exactly one operand                                                                     |
| `--direction=in\|out\|both`    | Select requested one-hop sections                                     | Exactly `in`, `out`, or `both`                | `both`                           | Repetition is invalid; no aliases and no Boolean trim flags                             |
| `--include=<source-reference>` | Include eligible incoming-scan sources                                | One shared source reference per occurrence    | Normal default incoming universe | Repeatable; shared filter contract forms the include union                              |
| `--exclude=<source-reference>` | Exclude eligible incoming-scan sources                                | One shared source reference per occurrence    | Exclude nothing                  | Repeatable; shared filter contract forms the exclusion union and exclusion wins overlap |
| global flags                   | Select workspace, presentation, diagnostics, or terminal help/version | The six values defined by the shared contract | Shared defaults                  | Shared repetition, terminal, and composition rules apply                                |

The applicable global flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`. `--view`
changes only human presentation. `--view` is accepted as a no-op when `--json`
selects structured presentation. `--help` and `--version` stop before reference
inspection under the shared terminal-mode rules.

Direction values are exact and case-sensitive at the command grammar boundary:
`in`, `out`, and `both`. `incoming`, `outgoing`, Boolean flags, comma lists, and
other aliases are not accepted. `--include` and `--exclude` are valid only when
incoming work is requested: they are accepted with `--direction=in` and
`--direction=both`, and are invalid with `--direction=out`.

## Direction And Coverage

Direction omission requests both sections. An explicit direction removes the
other section from both work and completeness accounting:

| Request                       | Evaluated sections    | Filter behavior                                                       |
| ----------------------------- | --------------------- | --------------------------------------------------------------------- |
| omitted or `--direction=both` | Incoming and outgoing | Include/exclude filters apply to incoming only; outgoing is unchanged |
| `--direction=in`              | Incoming only         | Include/exclude filters apply to the incoming scan                    |
| `--direction=out`             | Outgoing only         | Include/exclude is invalid; no incoming scan is evaluated             |

Every requested section has its own facts, coverage, status, and count. A
filtered-out section is absent, not an empty complete section and not an
incomplete section.

With both directions requested, the aggregate invocation is complete only when
both requested sections are complete. An incomplete, blocked, failed, or
interrupted requested section prevents a complete aggregate result even if the
other section has safe complete occurrences. If all requested coverage is
complete but a safe non-blocking finding remains, the shared semantic result may
be `attention`; a section cannot hide another section's status.

An incoming section is complete only after its effective scan universe has been
inspected completely. A complete incoming result with zero occurrences is valid
only after that complete scan. Excluding every eligible source can therefore
produce a complete empty incoming section when the filtered universe itself was
established completely.

An outgoing section is complete when the selected logical source layers were
inspected and every direct occurrence was recorded with the available resolution
fact. An external HTTP or HTTPS occurrence remains an unchecked fact; the absence
of network access alone does not make an otherwise complete outgoing section
`attention` or `incomplete`.

## Source And Overwrite Resolution

The required source operand follows the shared Source References contract. A
base path, source ID, or valid adjacent overwrite path identifies one logical
source. Unknown or missing references are invalid; unsafe or ambiguous identity
is blocked under the shared rules.

For outgoing work, inspect the logical source in physical layer order: base first,
then its valid overwrite companion. Every occurrence retains the physical layer
and canonical path in which it was authored. The overwrite is not an independent
source operand or section.

For incoming work, scan the effective eligible source universe while preserving
the physical layer, canonical path, and authored location of every occurrence. A
destination naming an overwrite resolves to the complete logical target while
retaining the targeted physical layer as evidence. Selecting either layer as the
source operand does not erase the base/overwrite identity.

The recognized compatibility entrypoint `_references.md` has one physical
identity in the final `references` route. Recognition of that filename must not
create a second logical source or a second scan; the entrypoint is processed once
by physical identity. Gate 5 executable proof must preserve this regression.

An orphan or ambiguous overwrite cannot become an independent source or target.
It produces the applicable invalid, blocked, or incomplete evidence rather than
being silently folded into an unrelated logical source.

## Incoming Source Universe

With no include or exclude flag, incoming work uses this command's normal
default universe: every ordinary Markdown candidate physically contained below
the selected workspace's `.agents` directory, including routed and unrouted
sources, entrypoints, `SKILL.md`, Markdown Skill resources, Templates, and valid
base/overwrite layers grouped as logical sources. Non-Markdown resources,
provider bridges outside `.agents`, receipts, recovery data, and operational
metadata are excluded. An unreadable, unsafe, malformed, or otherwise
uninspectable candidate affects incoming coverage rather than disappearing. The
command does not silently broaden that universe to every workspace Markdown
file. The shared filter contract owns omission and set composition but does not
define this command's default universe.

When include or exclude is supplied, the shared reusable contract at
[Source Universe Filters](../shared/source-universe-filters/interface.md) applies exactly. It
owns selector expansion, repeated-union behavior, exclusion precedence, logical
base/overwrite pairing, effective candidates, and inspected-source evidence.
This command does not create a second filtering grammar.

With `in`, the effective filtered universe controls incoming work. With `both`,
it controls incoming work only; outgoing still reads the selected source's base
then overwrite layers without narrowing. With `out`, include and exclude are
invalid and no incoming universe is evaluated.

## One-Hop Occurrences

The first version reports direct authored reference occurrences only. It does not
follow a discovered target to another level. Each occurrence retains the facts
needed to distinguish a source edge from a transitive graph path:

- Source ID when the authored source has one, and canonical source path.
- Physical source layer and path when base/overwrite applies.
- Authored source location.
- Raw destination as written, including a fragment when present.
- Direction: incoming or outgoing.
- Resolved local target ID and canonical path when available.
- Target kind and containment/resolution status.
- Provenance and coverage evidence for the section that produced the occurrence.

### Local Targets

Contained local targets retain their canonical workspace-relative path. A local
target outside `.agents` has no automatic Open Forge ID and uses a null local ID
while retaining its contained canonical path. Resolution findings such as missing,
unsupported, unsafe, or ambiguous local destinations remain visible rather than
being dropped.

### External Targets

Outgoing HTTP and HTTPS occurrences are included as direct facts. They are typed
as:

```text
raw destination: <authored URL>
target kind: external
local ID: null
local path: null
status: external-unchecked
network: network-not-attempted
```

The command never fetches, follows, resolves, or validates an external URL over
the network. An otherwise complete outgoing read may therefore contain
`external-unchecked` occurrences without an `attention` or `incomplete` result
merely because no network request was attempted.

## Human And Structured Output

The default human view is expanded. Both human views preserve separate requested
Incoming and Outgoing sections, direct `Level 1` grouping, per-section coverage,
and required findings before safe occurrence rows.

### Compact View

Compact output is a token-friendly direct-level projection. It reports the
aggregate result and requested sections without merging their coverage:

```text
result=complete  references=3

Incoming — coverage=complete  occurrences=2  scan=default
  Level 1
    <source-id>  .agents/<source>.md  -> <selected-source-id>
    <source-id>  .agents/<other-source>.md  -> <selected-source-id>

Outgoing — coverage=complete  occurrences=1
  Level 1
    <selected-source-id>  -> <target-id>  .agents/<target>.md
```

For an external outgoing occurrence, the direct row retains the raw URL and
external kind rather than pretending that a local ID or path exists. The compact
view may omit explanatory locations and resolution detail, but it never omits a
requested occurrence or section coverage.

### Expanded View

Expanded output adds directional pointers, exact authored source locations, raw
destinations, local or external target facts, resolution status, physical layer,
and provenance:

```text
Incoming — Level 1
  .agents/<source>.md:<line>:<column> -> .agents/<selected>.md
    direction: in
    raw destination: <authored destination>
    target: <selected-source-id>
    layer: base
    resolution: complete
    provenance: default eligible .agents Markdown scan

Outgoing — Level 1
  .agents/<selected>.md:<line>:<column> -> <authored destination>
    direction: out
    target kind: external
    local ID: null
    local path: null
    resolution: external-unchecked
    network: network-not-attempted
    layer: overwrite
```

Expanded framing also identifies the selected workspace, source logical identity,
requested direction, effective incoming universe when applicable, and each
section's status and coverage. `--verbose` adds bounded diagnostic detail without
changing occurrences, order, coverage, or status.

### JSON

`--json` emits one complete structured result derived from the same typed result.
It retains the source identity, requested direction, effective filter selectors,
per-section coverage and status, inspected-source evidence for incoming work, and
every typed occurrence with direction, direct level `1`, locations, raw
destination, resolution, target kind, layer, and provenance. A filtered-out
direction is absent, not represented as complete.
`--view` has no effect under JSON.

There is no graph, transitive-depth, diagnosis, repair, network-checking,
content-loading, persistent-index, result-cap, or minimal output mode.

## Results And Errors

The command uses the shared semantic result and error meanings:

| Result        | Meaning for `references`                                                                                                                        |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Every requested section completed its declared direct read or effective incoming scan with no unresolved finding that changes the result        |
| `attention`   | Requested coverage is complete but a safe non-blocking authored-form or identity finding remains; external no-fetch alone is not such a finding |
| `incomplete`  | Safe occurrences are available, but requested scan, layer inspection, or local-resolution coverage could not be completed                       |
| `invalid`     | The source, direction grammar, repetition, filter composition, or other command input is invalid                                                |
| `blocked`     | An unsafe path, ambiguous identity/target, or unsafe scan boundary prevents safe completion                                                     |
| `failed`      | An unexpected failure prevented normal completion                                                                                               |
| `interrupted` | The caller cancelled or interrupted the operation before completion                                                                             |

The aggregate result accounts only for requested directions. With both requested,
both sections must be complete for aggregate `complete`; a filtered-out direction
is never counted. Broken, unsupported, unsafe, or ambiguous occurrences remain
visible when safe to report, with the section and aggregate status explaining
whether they are attention, incomplete, or blocked.

Every human error names the operation, affected source, direction, filter, or
occurrence when known, the direct cause, and a useful next action. JSON preserves
the same semantic status and complete safe evidence in one structured result.

## Complete Examples

Report both direct directions for one source:

```text
open-forge references memory
```

Report only incoming references after applying the shared source-universe
filters:

```text
open-forge references memory \
  --direction=in \
  --include=directives \
  --include=memory/working \
  --exclude=memory/working/checkpoints
```

Report outgoing references, including unchecked external URL facts. Filters are
not valid in this mode:

```text
open-forge references .agents/loader.md --direction=out
```

Request the complete typed result. The view is a JSON no-op:

```text
open-forge references memory --direction=both --json --view=compact
```

## Non-Goals And Relationships

`references` is deliberately separate from nearby operations:

- `context --follow-links` expands selected context with reachable local target
  content. `references` reports direct edge facts only and never loads target
  bodies into context.
- `route list` enumerates authored route topology. A reference does not create
  route membership, parentage, loading, or authority.
- `find` returns a flat source inventory and predicate matches. It is not a
  one-hop incoming reverse query or outgoing reference report.
- `doctor` owns diagnosis and recommendations. Repair is a separate mutation
  authority; `references` never repairs an authored destination or generated
  navigation.

The command also does not perform transitive traversal, impact analysis, graph
queries, network checking, content projection, persistent reverse indexing, a
result cap, or semantic relatedness inference.

## Verification Requirements

Conformance evidence must cover:

- Required source resolution, exact IDs and paths, valid base/overwrite operands,
  unknown, unsafe, and ambiguous references.
- Direction omission, exact `in`, `out`, and `both`, invalid aliases, repeated
  direction, and include/exclude invalidity under `out`.
- Default all-eligible-`.agents`-Markdown incoming scan, shared include/exclude
  expansion, repeated unions, exclusion precedence, and filtered-empty complete
  results only after complete effective scans.
- Outgoing base-then-overwrite occurrence order and physical layer/path/location
  evidence for every occurrence.
- Incoming preservation of source layer/path/location and logical resolution when
  a raw destination names a base or overwrite target.
- Local targets inside and outside `.agents`, fragments, missing or ambiguous
  destinations, and external HTTP/HTTPS facts with no network attempt.
- Complete outgoing results containing unchecked external facts without a false
  attention or incomplete result solely for no-fetch.
- Separate incoming/outgoing sections, per-section coverage/status, aggregate
  completeness, absent unevaluated directions, and complete empty incoming scans.
- One-hop-only behavior: no transitive closure, graph merge, result cap, or target
  body loading.
- Compact, expanded, and JSON parity, including complete typed occurrences and
  `--view` being a JSON no-op.
- Stable repeated results and honest complete, attention, incomplete, invalid,
  blocked, failed, and interrupted outcomes.

## Related Current Sources

- [references Behavior Contract](behavior.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [Source Universe Filters Interface Contract](../shared/source-universe-filters/interface.md)
- [Context Interface Contract](../context/interface.md)
- [CLI Architecture](../../architecture.md)
