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

### Accepted Authored Reference Forms

Reference extraction reports only non-image link nodes produced by the already
accepted fixed CommonMark/Markdig parser. The accepted forms are:

- inline links;
- reference-style links; and
- explicit autolinks.

The command does not infer links from image or other resource embeds, raw HTML
links, code spans, code fences, or bare URL text that the fixed parser does not
represent as a link. These exclusions are silent exclusions, not findings. A
reference occurrence is an authored Markdown link fact, not a route entry,
generated navigation line, or semantic relationship.

The parser's accepted reference node is the compatibility boundary for this
command. Reference-style links are reported here even though they are not
canonical Open Forge route-entry syntax; this command does not make them
canonical authoring or route membership.

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

## Authored And Generated Regions

The operation applies the Framework generated-region boundary before it reports
authored link nodes. For each inspected physical layer, the interior is excluded
only when one valid bounded generated `Entries` region is established under the
Framework Markdown contract. A valid region is derived navigation and never an
authored reference source.

A malformed, duplicate, or unsafe generated region does not authorize the
operation to suppress any link. The operation retains every link occurrence whose
location is safely established outside a proven exclusion boundary and reports
`references.generated-region-unavailable`. The affected section is `incomplete`
when a safe no-suppression boundary and some occurrence evidence remain. It is
`blocked` only when no safe exclusion boundary can be established. The operation
does not select one marker pair by order, generated content, or likely intent.

This boundary is local to one physical layer. A valid region in one layer does
not exclude authored links in another base or overwrite layer.

## One-Hop Occurrences

The first version reports direct authored reference occurrences only. It does not
follow a discovered target to another level. Each occurrence retains the facts
needed to distinguish a source edge from a transitive graph path:

- Source ID when the authored source has one, and canonical source path.
- Physical source layer and path when base/overwrite applies.
- Authored link-use location and, when the parser exposes one, the exact
  destination location. Reference-style links retain the link-use location and
  a separate nullable destination location for the destination definition.
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

Fragment-only destinations resolve against the physical source layer containing
the occurrence. A target path is retained when a safe contained canonical path is
established, even when the target is missing; target ID and target layer remain
null until an existing, unambiguous physical target establishes them. A fragment
that cannot be found on an otherwise resolved target is reported as
`fragment-missing`, not as a missing target.

Non-HTTP external schemes are retained as unsupported findings. They have target
kind `unsupported`, resolution `unsupported`, null local identity and path, and
are never resolved or followed. HTTP and HTTPS are the only external schemes
reported as unchecked external occurrences.

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

### Exact Schema-v1 Command-Local Result

The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
schema-v1 envelope wraps this command-local `result` object.
The envelope is exactly the shared `{ schemaVersion, command, status, workspace,
result, next }` shape; its `status` is the aggregate semantic result and is not
duplicated here. The shared `SourceLocation` primitive is the same authority's
exact `{ line, column, byteOffset, byteLength }` shape. The following grammar
defines every command-local member in wire order. No member is omitted.

```text
type ReferencesResult = {
  source: Source | null;
  requestedDirection: "in" | "out" | "both" | null;
  incomingSelection: IncomingSelection | null;
  incoming: ReferenceSection | null;
  outgoing: ReferenceSection | null;
  findings: Finding[];
};

type Source = {
  id: string;
  path: string;
  layers: SourceLayer[];
};

type SourceIdentity = {
  id: string;
  path: string;
};

type SourceLayer = "base" | "overwrite";

type SelectorOccurrence = {
  role: "include" | "exclude";
  value: string;
};

type IncomingSelection = {
  mode: "default" | "filtered";
  supplied: SelectorOccurrence[];
  resolved: SelectorResolution[];
  effectiveSources: SourceIdentity[];
  inspectedSources: SourceLayerEvidence[];
};

type SelectorResolution = {
  role: "include" | "exclude";
  occurrence: positive-integer;
  supplied: string;
  form: "source-id" | "source-path";
  resolution: "resolved" | "invalid" | "unknown" | "ambiguous" | "unsupported" | "unsafe";
  source: SourceIdentity | null;
  expansion: "source" | "folder" | null;
  candidates: SourceIdentity[];
};

type SourceLayerEvidence = {
  source: SourceIdentity;
  layer: SourceLayer;
  path: string;
};

type ReferenceSection = {
  coverage: "complete" | "incomplete" | "blocked";
  status: SharedStatus;
  occurrenceCount: nonnegative-integer;
  occurrences: Occurrence[];
};

type Occurrence = {
  direction: "in" | "out";
  level: 1;
  source: {
    id: string | null;
    path: string;
    layer: SourceLayer;
  };
  location: SourceLocation;
  destinationLocation: SourceLocation | null;
  rawDestination: string;
  fragment: string | null;
  target: {
    kind: "local" | "external" | "unsupported";
    id: string | null;
    path: string | null;
    layer: SourceLayer | null;
    resolution: "complete" | "missing" | "fragment-missing" | "malformed" | "absolute" | "query" | "encoding-unsupported" | "outside-workspace" | "physical-escape" | "ambiguous" | "unreadable" | "unsupported" | "external-unchecked";
    network: "network-not-attempted" | null;
  };
  provenance: "selected-source" | "default-incoming-scan" | "filtered-incoming-scan";
};

type Finding = {
  code: ReferencesFindingCode;
  status: SharedStatus;
  direction: "in" | "out" | null;
  subject: string | null;
  cause: string;
  selectorRole: "include" | "exclude" | null;
  selectorOccurrence: positive-integer | null;
  source: SourceIdentity | null;
  layer: SourceLayer | null;
  path: string | null;
  location: SourceLocation | null;
  destinationLocation: SourceLocation | null;
  candidates: SourceIdentity[];
};

type SharedStatus =
  "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";
```

`source` is `null` only when invalid, ambiguous, or unsafe input prevents the
operation from establishing one selected logical source. The corresponding
finding retains the supplied operand, cause, and candidates when applicable. For
an established source, `source.path` is its canonical base path. `layers` is
always present in physical order and contains `base`, followed by `overwrite`
when a valid overwrite companion exists. `SourceIdentity.path` has the same
logical-source meaning. `SourceLayerEvidence.path` and an occurrence source
`path` are physical layer paths.

`requestedDirection` is `null` only when invalid direction input prevents the
operation from establishing one effective direction. Omission establishes
`both`. `incomingSelection` is `null` when incoming work is not requested or no
effective direction was established. Otherwise it is always present. `mode` is
`default` only when neither filter flag occurs and `filtered` otherwise.
`supplied` preserves every include and exclude occurrence in global command-line
order, including duplicates. `resolved` uses that same order; `occurrence` is
1-based within its role. `form` is classified from the shared source-reference
spelling even when resolution fails. `candidates` is always present and is empty
except when ambiguity evidence supplies candidates. `effectiveSources` is a
deduplicated logical-source set. `inspectedSources` contains one physical-layer
record for each effective layer whose inspection was established; an unavailable
layer is represented by its finding instead.

The `incoming` and `outgoing` members are `null` when their direction is not
requested or invalid input prevents an effective direction from being
established. Once an effective direction is established, each requested section
is present even when source or scan resolution blocks it before occurrence
inspection. A present section uses only the three declared coverage values:
`complete`, `incomplete`, or `blocked`. `status` uses the shared seven-status
set, so a section may have `coverage=complete` and `status=attention`. A section
is never represented as `not-started`. `occurrenceCount` equals the length of
`occurrences`, including duplicates.

`location` is the exact authored link-use span. It is never omitted from an
occurrence. For a reference-style link, `destinationLocation` is the separate
exact span of the destination definition when available, not the link-use span.
For an inline or explicit autolink, it is the exact destination span when the
fixed parser exposes one, otherwise `null`. A finding explains an otherwise
applicable location that could not be established. `rawDestination` preserves
the authored destination text, including spaces, Unicode, duplicate spelling,
and fragment text. `fragment` is `null` when no fragment is authored and otherwise
preserves the authored fragment text without the separating `#`.

For a local target, `path` is the safe canonical workspace-relative target path
when established, including a missing contained path. `id` is non-null only for
an existing unambiguous `.agents` source. A contained target outside `.agents`
therefore has a null ID and its canonical workspace-relative path. `layer` is
non-null only when an existing physical base or overwrite target is identified.
External and unsupported targets have null local identity, path, and layer.
`network` is `network-not-attempted` only for HTTP or HTTPS
`external-unchecked` targets and is otherwise `null`.

`provenance` is `selected-source` for outgoing occurrences, and is
`default-incoming-scan` or `filtered-incoming-scan` according to the incoming
selection mode. Incoming occurrences include only direct local occurrences that
resolve to the selected logical source. Findings can additionally preserve safe
nonmatching malformed, unsupported, unreadable, or boundary evidence from the
incoming scan without turning it into an incoming occurrence.

Finding `cause` is always present, and `subject`, selector coordinates, source
coordinates, locations, and `candidates` are present as typed nullable or array
members even when they do not apply. `direction` identifies the affected section
when one exists. `candidates` is always present. The command never emits an
`external-unchecked` finding: that value is an occurrence resolution, not a
finding.

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

### Finding Codes, Status, And Result Effect

References has exactly the following finite finding vocabulary. The table order
is also the primary finding order. A conditional status is part of the mapping,
not an implementation choice.

| Finding code                              | Status                                                            | Result effect                                                                                                                         |
| ----------------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| `references.invalid-input`                | `invalid`                                                         | The request cannot be interpreted; no result may claim valid domain coverage.                                                         |
| `references.invalid-source`               | `invalid`                                                         | The required source operand is missing, unknown, or unsupported under the shared source-reference rules.                              |
| `references.invalid-direction`            | `invalid`                                                         | The direction value or repetition is invalid.                                                                                         |
| `references.invalid-filter`               | `invalid`                                                         | Include/exclude grammar, applicability, or a selector value is invalid.                                                               |
| `references.workspace-unavailable`        | `blocked`                                                         | The selected workspace cannot be established safely.                                                                                  |
| `references.workspace-unsafe`             | `blocked`                                                         | The selected workspace boundary is unsafe.                                                                                            |
| `references.source-ambiguous`             | `blocked`                                                         | The required source identity has unresolved candidates.                                                                               |
| `references.source-unsafe`                | `blocked`                                                         | The required source identity or layer boundary is unsafe.                                                                             |
| `references.selector-ambiguous`           | `blocked`                                                         | A filter selector has unresolved candidates and cannot form a safe effective universe.                                                |
| `references.selector-unsafe`              | `blocked`                                                         | A filter selector crosses or cannot establish its safe physical boundary.                                                             |
| `references.identity-collision`           | `attention`                                                       | Exact physical evidence remains usable, but an established source-identity collision prevents an unqualified complete identity claim. |
| `references.candidate-unsafe`             | `incomplete`                                                      | A candidate boundary cannot be safely inspected while other safe scan evidence remains available.                                     |
| `references.layer-unresolved`             | `incomplete`                                                      | A base/overwrite relationship cannot be established for a candidate layer.                                                            |
| `references.inspection-unavailable`       | `incomplete`                                                      | A required source read, enumeration, parser span, or other direct inspection fact is unavailable while a safe boundary remains.       |
| `references.invalid-encoding`             | `incomplete`                                                      | Encoding prevents safe destination or source-location identity; the affected fact remains unresolved.                                 |
| `references.generated-region-unavailable` | `incomplete`, or `blocked` when no safe exclusion boundary exists | Generated `Entries` cannot be safely excluded as one valid region. Links are not silently suppressed.                                 |
| `references.destination-malformed`        | `attention`                                                       | A complete authored occurrence remains visible with resolution `malformed`, `absolute`, or `query`; no target is invented.            |
| `references.destination-unsupported`      | `attention`                                                       | A complete authored occurrence remains visible with target kind and resolution `unsupported`; it is not resolved or followed.         |
| `references.target-missing`               | `attention`                                                       | A safely established local target path is absent; the occurrence remains visible with resolution `missing`.                           |
| `references.fragment-missing`             | `attention`                                                       | The target exists but the authored fragment is absent; the occurrence remains visible with resolution `fragment-missing`.             |
| `references.target-unsafe`                | `blocked`                                                         | Target containment or physical identity is unsafe; no unsafe target is selected.                                                      |
| `references.target-ambiguous`             | `blocked`                                                         | More than one target identity remains possible; no target is selected.                                                                |
| `references.target-unreadable`            | `incomplete`                                                      | Target-read evidence required for local resolution is unavailable; the safe occurrence remains visible when possible.                 |
| `references.operation-failed`             | `failed`                                                          | An unexpected operation failure stops normal completion and retains only established safe evidence.                                   |
| `references.interrupted`                  | `interrupted`                                                     | Caller interruption stops the operation; partial evidence never becomes complete.                                                     |

Invalid findings set the aggregate status to `invalid`. Blocked findings set it
to `blocked`. Incomplete findings set it to `incomplete` when no higher status
applies. Attention findings leave coverage complete but set the affected section
and aggregate to `attention` when no invalid, blocked, incomplete, failed, or
interrupted status applies. The event findings retain `failed` and `interrupted`
exactly. A malformed generated region is therefore `incomplete` when safely
bounded evidence remains and `blocked` only when no safe exclusion boundary can
be established. HTTP/HTTPS no-fetch produces no finding.

Findings sort first by this table's code order. Within one code, request-level
facts precede selection facts, then incoming before outgoing section facts; source
identity, canonical physical path, base before overwrite, authored location, and
destination location provide ordinal tie-breaks. Selector findings use include
before exclude and then the 1-based occurrence. Event findings follow all
non-event findings. This order is independent of filesystem, parser, discovery,
or exception order.

### Exact Next Actions

The shared envelope's `next` member uses the following command-local contents.
At most one action is emitted. The first applicable row in this table wins after
the aggregate status and ordered findings are fixed.

| Condition                                                                                                    | `next.command`                    | `next.reason`                                                                                                        |
| ------------------------------------------------------------------------------------------------------------ | --------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `complete`                                                                                                   | `null`                            | `null`                                                                                                               |
| `invalid`                                                                                                    | `open-forge references --help`    | `Correct the named References input, then rerun the request.`                                                        |
| `blocked` with `references.source-ambiguous` or `references.selector-ambiguous` as the first blocked finding | `open-forge references`           | `Replace the ambiguous source or selector with one listed exact path, then rerun the request.`                       |
| Other `blocked`                                                                                              | `open-forge doctor`               | `Inspect the blocked workspace, source, selector, generated-region, or target boundary before rerunning References.` |
| `incomplete`                                                                                                 | `open-forge doctor`               | `Inspect the unavailable source, generated-region, or target facts before relying on this References result.`        |
| `attention`                                                                                                  | `open-forge doctor`               | `Inspect the reported reference or identity findings before relying on this References result.`                      |
| `failed`                                                                                                     | `open-forge references --verbose` | `Report the failure and retry the same References request with bounded diagnostics.`                                 |
| `interrupted`                                                                                                | `open-forge references`           | `Rerun the same References request.`                                                                                 |

The JSON `next` object contains those exact strings. Human compact and expanded
results use the same action as their optional `Next:` line. A renderer does not
choose or rewrite the action.

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
- Fixed-parser extraction of inline, reference-style, and explicit-autolink
  non-image links, including exact link-use and destination spans.
- Exclusion of image/resource embeds, raw HTML links, code spans/fences, bare URL
  text, generated `Entries` interiors, and all links inside a valid generated
  region.
- Malformed, duplicate, and unsafe generated regions that retain safe evidence,
  do not suppress links, and produce the required incomplete or blocked coverage.
- Preservation of raw spaces, Unicode, duplicate occurrences, fragments, and
  base/overwrite authored layer evidence.
- Local targets inside and outside `.agents`, fragment-only links, every declared
  local resolution, non-HTTP unsupported schemes, and external HTTP/HTTPS facts
  with no network attempt.
- Complete outgoing results containing unchecked external facts without a false
  attention or incomplete result solely for no-fetch.
- Separate incoming/outgoing sections, per-section coverage/status, aggregate
  completeness, absent unevaluated directions, and complete empty incoming scans.
- Exact schema-v1 member presence, nullability, finite values, finding-code order,
  finding status mapping, selector occurrence preservation, and result effects.
- Incoming default and filtered provenance, source-layer inspection evidence,
  deterministic finding order, and parity of locations between human and JSON
  projections.
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
