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
caller-visible verification. The command does not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md).

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

The applicable global flags are `--workspace <path>`, `--format json`,
`--detail <minimal|standard|full|debug>`, `--detail debug`, `--help`, and `--version`. `--detail`
selects detail in human and JSON presentation. `--help` and `--version` stop before reference
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
cancelled requested section prevents a complete aggregate result even if the
other section has safe complete occurrences. If all requested coverage is
complete but a safe non-blocking finding remains, the shared semantic result may
be `completed-with-warnings`; a section cannot hide another section's status.

An incoming section is complete only after its effective scan universe has been
inspected completely. A complete incoming result with zero occurrences is valid
only after that complete scan. Excluding every eligible source can therefore
produce a complete empty incoming section when the filtered universe itself was
established completely.

An outgoing section is complete when the selected logical source layers were
inspected and every direct occurrence was recorded with the available resolution
fact. An external HTTP or HTTPS occurrence remains an unchecked fact; the absence
of network access alone does not make an otherwise complete outgoing section
`completed-with-warnings` or `incomplete`.

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
It produces the applicable invalid-input, blocked, or incomplete evidence rather than
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
does not select one Entries section by order, generated content, or likely intent.

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
`external-unchecked` occurrences without an `completed-with-warnings` or `incomplete` result
merely because no network request was attempted.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                           | Headline                                                                                               | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | links exist                                                                    | `<path>` then rows                                                                                     |    0 | stdout |
| completed               | none                                                                           | `<path> has no authored links in or out. Entries links are not counted.` (direction-specific variants) |    0 | stdout |
| completed-with-warnings | a broken, malformed or unsupported outgoing link                               | `<path>` then rows with their state, then warning rows                                                 |    2 | stdout |
| incomplete              | a scanned file could not be read, or an Entries section could not be separated | rows plus warning rows; `standard` adds `The scan is incomplete.`                                      |    3 | stdout |
| invalid-input           | unknown source, bad direction, filters with out-only                           | `Cannot list references: <problem>.`                                                                   |    4 | stderr |
| blocked                 | ambiguous or unsafe source, selector or target                                 | `Cannot list references: <reason>.`                                                                    |    5 | stderr |
| failed                  | unexpected error                                                               | `References stopped because of an unexpected error: <reason>.`                                         |    1 | stderr |
| cancelled               | Ctrl+C                                                                         | `References was cancelled.`                                                                            |  130 | stderr |

### Text by level

`minimal`, links:

```text
.agents/memory/_memory.md
  in   .agents/loader.md:104:3
  in   .agents/maps/_maps.md:12:3
  out  :52:3   archived/_archived.md
  out  :53:3   crystallized/_crystallized.md
  out  :60:1   https://example.org/guide   not checked
  out  :61:3   ../old.md   missing
```

`in` rows name where the link is written. `out` rows give the location inside
the source and the destination as written, followed by a state word only when
the link is not fine: `missing`, `heading not found`, `not checked`
(external), `not followed` (unsupported kind).

`minimal`, none:

```text
.agents/patterns/_patterns.md has no authored links in or out. Entries links are not counted.
```

`standard` adds `Workspace:`, the resolved path after each `out` destination
(`-> .agents/memory/archived/_archived.md`), the counts sentence (`2 in, 4
out`), and the filters used for the incoming scan.

`full` adds the sources that were scanned for incoming links and the layer
of each occurrence.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-invalid-input).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-blocked).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#references-candidate-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                               |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ source { id, path }, direction, incoming: [ { path, location } ], outgoing: [ { location, destination, resolvedPath, state } ] }` |
| standard | + `coverage { incoming, outgoing }`, `filters { include, exclude }`                                                                  |
| full     | + `scanned: [ { id, path, layer } ]`, per occurrence `layer`                                                                         |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Counts and limitations

`incoming`, `outgoing`, `sourcesScanned`.

### Next rules

Broken links -> `open-forge doctor`; invalid source -> the route list;
otherwise none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                    | Severity | Family                | Message                                                                                                                                | Next                                |
| --------------------------------------- | -------- | --------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| references.invalid-input                | error    | invalid-input         |                                                                                                                                        |                                     |
| references.invalid-source               | error    | unknown-source        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.invalid-source`).                                                                         | `open-forge route list --depth=all` |
| references.invalid-direction            | error    | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.invalid-direction`).                                                                                                | none                                |
| references.invalid-filter               | error    | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.invalid-filter`). | none                                |
| references.workspace-unavailable        | error    | workspace-unavailable |                                                                                                                                        |                                     |
| references.workspace-unsafe             | error    | workspace-unsafe      |                                                                                                                                        |                                     |
| references.source-ambiguous             | error    | source-ambiguous      |                                                                                                                                        |                                     |
| references.source-unsafe                | error    | source-unsafe         |                                                                                                                                        |                                     |
| references.selector-ambiguous           | error    | selector-ambiguous    |                                                                                                                                        |                                     |
| references.selector-unsafe              | error    | selector-unsafe       |                                                                                                                                        |                                     |
| references.identity-collision           | warning  | identity-collision    |                                                                                                                                        |                                     |
| references.candidate-unsafe             | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.candidate-unsafe`).                                            | `open-forge doctor`                 |
| references.layer-unresolved             | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.layer-unresolved`).                                                                            | `open-forge doctor`                 |
| references.inspection-unavailable       | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.inspection-unavailable`).                                                                                        | `open-forge doctor`                 |
| references.invalid-encoding             | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.invalid-encoding`).                                                                                    | fix by hand                         |
| references.link-encoding-invalid        | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.link-encoding-invalid`).                                               | `open-forge doctor`                 |
| references.generated-region-unavailable | warning  | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.generated-region-unavailable`).                     | `open-forge doctor`                 |
| references.destination-malformed        | warning  | local                 | row state `not a resolvable link`                                                                                                      | fix by hand                         |
| references.destination-unsupported      | warning  | local                 | row state `not followed`                                                                                                               | none                                |
| references.target-missing               | warning  | local                 | row state `missing`                                                                                                                    | `open-forge doctor`                 |
| references.fragment-missing             | warning  | local                 | row state `heading not found`                                                                                                          | `open-forge doctor`                 |
| references.target-unsafe                | error    | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.target-unsafe`).                                                                                 | fix by hand                         |
| references.target-ambiguous             | error    | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/References/Shared/Wording/ReferencesWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`references.target-ambiguous`).                                                                            | fix by hand                         |
| references.target-unreadable            | warning  | local                 | row state `target could not be read`                                                                                                   | none                                |
| references.operation-failed             | error    | operation-failed      |                                                                                                                                        |                                     |
| references.interrupted                  | error    | cancelled             |                                                                                                                                        |                                     |

Row-state findings are rendered inline on the `out` row at `minimal` and as
finding rows only at `full`, so the same fact is not printed twice.

## Scenarios

### Catalogue situations

`links-both`, `no-authored-links`, `out-only`, `in-only-with-include`,
`broken-outgoing`, `external-outgoing`, `unknown-source` (invalid-input),
`invalid-direction`, `include-with-out-only` (invalid-input), `ambiguous-source`
(blocked), `unreadable-source` (incomplete).

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native command currently emits `references.identity-collision` with
`Two sources share an identity` and `Cannot list references: The source
catalogue retained an unresolved boundary.`, but the catalogue has no wording
row for that code. The contract records the current output without deciding
whether to add a row or change the code. **Maintainer decision remains open.**

The catalogue says `standard` adds `The scan is incomplete.` for
`unreadable-source`, but the native capture omits that sentence. The contract
records the current output without deciding whether to emit the sentence or
change the catalogue row. **Maintainer decision remains open.**

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
  completed-with-warnings or incomplete result solely for no-fetch.
- Separate incoming/outgoing sections, per-section coverage/status, aggregate
  completeness, absent unevaluated directions, and complete empty incoming scans.
- Exact schema-3 member presence, nullability, finite values, finding-code order,
  finding status mapping, selector occurrence preservation, and result effects.
- Incoming default and filtered provenance, source-layer inspection evidence,
  deterministic finding order, and parity of locations between human and JSON
  projections.
- One-hop-only behavior: no transitive closure, graph merge, result cap, or target
  body loading.
- Minimal, standard, and JSON parity, including complete typed occurrences and
  defined minimal JSON membership.
- Stable repeated results and honest completed, completed-with-warnings,
  incomplete, invalid-input, blocked, failed, and cancelled outcomes.

## Related Current Sources

- [references Behavior Contract](behavior.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [Source Universe Filters Interface Contract](../shared/source-universe-filters/interface.md)
- [Context Interface Contract](../context/interface.md)
- [CLI Architecture](../../architecture.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`references.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/References/ReferencesText.cs).

<!-- @OpenForgeTextRef references.help.syntax -->
