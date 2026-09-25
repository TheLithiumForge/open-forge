---
open-forge:
  description: Accepted current public grammar and observable one-source route profile for `route inspect`
  responsibility: Define what a caller may enter and observe for `route inspect`
  tags: [Memory, Crystallized, CLI, Release, Command, Route, Inspect, Interface, CurrentTruth]
---

# route inspect Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route inspect`. The command is implemented in the merged CLI;
implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral operation behind this public surface. The shared
[Global CLI Flags](../../shared/global-flags/interface.md) and [CLI Source References](../../shared/source-references/interface.md)
contracts retain their shared meanings rather than being redefined here.

The current [Routing Model](../../../../framework/routing/model.md),
[Loading And Refreshing Context](../../../../framework/routing/loading.md),
[Route Scope And Inheritance](../../../../framework/routing/scope.md),
and [Overwrite Customization](../../../../framework/routing/overwrites.md)
documents define the Framework meaning inspected by this command. The
accepted [Context Interface Contract](../../context/interface.md) defines selected closures,
and the [Status Interface Contract](../../status/interface.md) defines the shared physical
measurements and token estimate.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared JSON envelope and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines source and runtime boundaries,
the BCL-first filesystem boundary, and diagnostic structure. Primary
human `completed`, `completed-with-warnings`, and `incomplete` results use
stdout. Primary human `invalid-input`, `blocked`, `failed`, and `cancelled` results
use stderr. `--format json` writes one complete structured result to stdout for every
status; bounded diagnostics use stderr, and human text is never mixed into JSON
stdout. Command-specific repetition beyond the shared global flags is not
invented here.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the shared envelope, schema compatibility, and process-status mapping.
The [CLI Architecture](../../../architecture.md) defines concrete serialization,
parser and filesystem structure, physical identity, containment, diagnostics,
source and runtime boundaries, and test boundaries. This Interface defines the exact
command-local structured fields below without choosing their implementation.

## Purpose

`route inspect` explains one known source's route behavior without returning its
authored content. It answers:

- Is this source read at task start or resume?
- What other event can cause it to be read automatically?
- May it be read again after context restoration, before handoff or closeout, or
  after a change that may affect its follow-up work?
- How much context does selecting it add beyond task-start context?
- How much descendant context below its entrypoint is read automatically through
  `#LoadNow`?
- Which route chain, parent, children, and descendants establish its position?
- Which ancestor sources contribute inherited `Axioms`?
- Does it have an overwrite companion?

The command is a route profile, not a workspace summary, content reader,
discovery operation, validator, or mutation.

Given the same CLI payload, workspace bytes, and explicit source reference, the
command returns the same route facts, measurements, ordering, and semantic
result.

## Syntax

The complete domain command form is:

```text
open-forge route inspect <source-reference>
  [global flags]
```

This is the exact public shape: `open-forge route inspect <source-reference> [global flags]`.

Exactly one source reference is required in domain mode. The operand uses the
shared source-reference forms:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

The shared [CLI Source References](../../shared/source-references/interface.md) contract defines
automatic IDs, exact paths, prefix interpretation, quoting, collisions,
disambiguation, overwrite identity, and reported paths. A source reference is
not a list of subjects.

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines
`--workspace <path>`, `--format <text|json>`, `--detail <minimal|standard|full|debug>`, repeatable `--detail-filter <error|warning|info|all>`, `--help`, and `--version`. All
six apply to `route inspect` under that contract.

The `route` group itself continues to show help and performs no domain
operation. `route inspect` has no `--all`, content projection, link expansion,
search, mutation, write-policy flag, route-list mode, generic `inspect` alias,
or other operation-specific flag.

## Workspace And Subject

The command uses the exact current working directory or exact
`--workspace <path>` value. It never searches parent directories, substitutes a
Git root, or infers another workspace from the source reference.

The source reference may identify:

- A routed entrypoint.
- An ordinary routed Markdown source.
- A routed native source such as `SKILL.md` when its source contract establishes
  route metadata.
- A valid overwrite path, which resolves to its base logical source.
- An explicitly selected detached entrypoint whose local topology is
  unambiguous.
- An existing supported source that is not routed, so the command can report
  that exact state without inventing a route.

The Loader is the workspace routing root rather than one route subject. It is
invalid for this command. Use `status` for root-category orientation and `index`
for Loader-rooted generated navigation.

An unsupported source kind is invalid. An unresolved ambiguous source ID is
blocked. An exact path or interactive choice may resolve only the physical source
identity. If the selected source has a safe, complete route and its automatic ID
is still non-unique, the result is `completed-with-warnings`. Exact path input cannot turn an
ambiguous route relationship into a valid one. A structurally ambiguous route
remains blocked even when an exact path selects one recognized file.

Selecting a valid overwrite reference inspects the base and overwrite as one
logical source. The overwrite inherits the base route, reading behavior, and
scope. It is never reported as an independent route.

### Interactive collision selection

When a human request supplies a non-unique automatic ID and the host reports
that both standard input and the stderr prompt stream are terminal-capable,
Inspect lists every candidate in canonical-path order on stderr and asks one
question. The one answer must be either the one-based displayed candidate number
or one exact displayed path. There is no implicit default, fuzzy choice, retry,
or selection by kind, depth, enumeration order, or likely intent. A valid
response selects that exact physical source and records `interactive` as the
selection method.

JSON and redirected requests never prompt. They retain every candidate and the
existing blocked exact-path guidance. An invalid answer or end of input retains
that same blocked collision; caller cancellation forms `cancelled`. The prompt
never writes to stdout. Interactive selection does not repair an ambiguous route,
grant mutation authority, or change the accepted `completed-with-warnings` observation and
exact-path next action.

## Source-State Classification

Readable generated `Entries` in an ordinary entrypoint remain measurable when optional description or tags are absent. This includes intermediate entrypoints created by nested Route Create. Missing optional metadata alone does not make those Entries unavailable. Malformed metadata, unreadable bodies, unavailable Entries and required native metadata retain their existing strict boundaries. Independently unavailable selected-source reading facts remain unavailable.

The command uses this finite source-state classification after input resolution.
The table assumes that no higher incomplete, blocked, failed, or interrupted
condition applies. A state describes what the command can safely establish; it
does not diagnose the workspace or recommend a change.

| Established source state                                                                                     | Semantic result           | Route-profile treatment                                                                                                                                                     |
| ------------------------------------------------------------------------------------------------------------ | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Routed entrypoint, routed leaf, or routed native source                                                      | `completed`                | Applicable route, reading, topology, and measurements are reported.                                                                                                         |
| Accepted compatibility entrypoint filename                                                                   | `completed`                | The filename is reported as compatibility input; its route facts remain ordinary complete facts.                                                                            |
| Valid base and overwrite pair                                                                                | `completed`                | The pair is one logical source with base-first physical layers. Customization is neutral.                                                                                   |
| Detached entrypoint with safe local identity and topology                                                    | `completed`                | Local route facts are reported. Loader-root reading facts are not-applicable.                                                                                               |
| Known supported source with no established route                                                             | `completed`                | The source identity and applicable physical measurements are reported; route-dependent facts are not-applicable.                                                            |
| Safe source and route with a non-unique automatic ID, resolved by exact path or interactive source selection | `completed-with-warnings`               | The non-unique ID remains an observation. Interactive selection requires an exact path for later non-interactive use; exact-path selection has no invented required action. |
| Safe identity with an unreadable required source, incomplete route chain, or unmeasurable applicable fact    | `incomplete`              | Safe observations remain visible and the affected availability is `unavailable`.                                                                                            |
| Orphan or ambiguous overwrite, ambiguous route, unsafe identity, or containment failure                      | `blocked`                 | The command does not choose a pair, route, alias, or out-of-bound source.                                                                                                   |
| Zero or several operands, Loader, unknown, missing, or unsupported source                                    | `invalid-input`                 | Input is rejected before route inspection.                                                                                                                                  |
| Unexpected failure or caller interruption before completion                                                  | `failed` or `cancelled` | The event status retains its own meaning.                                                                                                                                   |

When several ordinary conditions occur, `blocked` takes precedence over
`incomplete`, `incomplete` over `completed-with-warnings`, and `completed-with-warnings` over `completed`.
Invalid input stops before operation resolution, while failure and interruption
retain their event meanings.

## Operands

| Operand              | State                         | Accepted value                                                                | Omission                                                                                                        | Repetition and order                                                                                     |
| -------------------- | ----------------------------- | ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `<source-reference>` | Required for domain execution | One shared automatic source ID or exact `.agents/...` or `./.agents/...` path | Invalid in domain mode; terminal `--help` and `--version` behavior follows the shared Global CLI Flags contract | Exactly one subject; missing or several source references are invalid. There is no source-list ordering. |

The source-reference contract remains the complete authority for exact matching,
quoting, path containment, collisions, interactive identity disambiguation,
and base/overwrite selection. This command does not add a qualifier syntax or
another source identity system.

## Flags

All six [Global CLI Flags](../../shared/global-flags/interface.md) apply to this command. Their
spelling, values, defaults, repetition, composition, terminal behavior, errors,
and presentation meaning remain defined only by that shared contract.

The command has no operation-specific flags, so it defines no additional
flag-repetition or flag-composition policy beyond the shared global contract.
The accepted source defines no operation-specific flag repetition case. No
repetition behavior is inferred for a flag that this command does not define.

## Identity

The result identifies:

- Requested source reference.
- Automatic source ID and canonical workspace-relative path.
- Routed source type.
- Source state and route state.
- Routed, detached, not-routed, ambiguous, or unresolved state.
- Canonical or accepted compatibility entrypoint filename when applicable.
- Base and overwrite physical paths when a valid pair exists.
- A non-unique automatic-ID observation when exact-path or interactive source
  selection resolved only the physical source identity.

An existing supported source with no mechanically established route returns a
complete `not routed` identity when that absence is known exactly. Reading,
route structure, and inherited `Axioms` fields are not applicable rather than
zero.

A detached entrypoint reports its local identity and route structure. It does
not claim Loader-rooted task-start, parent-triggered, or later-continuity
reading facts because no Loader-rooted chain establishes them. Local topology
and local `#LoadNow` descendants remain applicable when they can be measured.

A known supported source without a route reports route-dependent reading,
selection, topology, and inherited-`Axioms` facts as not-applicable. Its own
physical source measurement and other facts that do not require a route remain
applicable.

## Reading Behavior

Reading behavior is prospective Framework behavior derived from the current
workspace. It is not a claim that an agent runtime observed a read.

### Task Start Or Resume

The command resolves the same task-start context as `context` without operands.
It then reports whether the inspected logical source belongs to that set.

Human output uses:

```text
Read at task start or resume: yes
```

or:

```text
Read at task start or resume: no
```

Inspecting a source does not add that source to the task-start calculation.

For a detached entrypoint, Loader-rooted task-start membership is
`not-applicable`. A known supported unrouted source has no established task-start
membership because no route establishes one. These are not negative readings and
are not rendered as `no`.

### Automatic Reading Trigger

The command explains the concrete trigger in ordinary language:

- A source with `#LoadNow` is read when its exposing parent is read.
- A source without an automatic trigger is read when its route is explicitly
  selected.
- A `#KeepInMind` entrypoint or ordinary file is read when its exposing parent
  is read, then refreshed at applicable review points while its scope remains
  active. Neither loading tag activates an otherwise unselected ancestor or scope.
- An overwrite is read immediately after its base.

The human renderer names the actual exposing parent or event. It does not use
`policy`, `target-sensitive`, `continuity boundary`, or another internal label
instead of explaining what happens.

For example:

```text
Read automatically when: memory/working is read
```

An untagged on-demand source says:

```text
Read automatically after another route: no
Read when this route is selected: yes
```

Structured output keeps typed reason codes for automation, including on-demand,
parent-triggered `#LoadNow`, entrypoint `#KeepInMind`, routed-file
`#KeepInMind`, and overwrite inheritance. Human output always explains the
corresponding event in ordinary words. For both `KeepInMind` kinds,
`relatedSourceId` identifies the exposing parent and `events` contains
`exposing-parent-read` followed by `later-review`. Later review applies only
while that scope remains active.

Detached entrypoints have no Loader-rooted automatic trigger. Their local
`#LoadNow` descendants may still be measured as a local topology fact. Known
unrouted sources have no route-triggered automatic reading; the corresponding
route-dependent value is `not-applicable`.

### Later Reads

The command reports whether the source may be read again after task start and
states the actual occasions:

```text
May be read again: yes
When: after context restoration, before handoff or closeout, or after a change that may affect its follow-up work
```

or:

```text
May be read again: no
```

This means the current Framework says to revisit the source at those occasions.
It does not mean the source is read on every message, prompt, model request, or
tool call.

For a detached entrypoint or known supported unrouted source, Loader-rooted
later-read membership is `not-applicable`, not `no`.

## Context Cost

The command uses the accepted status measurements:

| Metric           | Meaning                                           |
| ---------------- | ------------------------------------------------- |
| Physical files   | Exact unique physical source-layer count          |
| Characters       | Exact Unicode scalar-value count                  |
| Size             | Exact UTF-8 byte length, displayed with IEC units |
| Estimated tokens | `ceiling(characters / 4)`, marked as an estimate  |

Human output omits character counts because bytes and estimated tokens provide
the useful route-inspection summary. Structured output retains exact characters
so all measurement-bearing commands use one typed measurement record.

The estimate is not a model tokenizer, context-window guarantee, billing value,
latency estimate, parsing cost, or runtime memory measurement.

### Own Source

`Own source` measures the inspected base and valid overwrite companion. It
includes complete physical file bytes, including frontmatter and generated
regions inside an entrypoint.

### Added By Selection

The command resolves the inspected source's selected route closure under the
same rules as `context`, without following ordinary links. It then subtracts
the current task-start context set.

`Selecting this route adds` measures that exact difference. It includes missing
ancestor entrypoints needed to establish the selected route, the target and
overwrite, visible `#LoadNow` descendants, and applicable scope-local loading
that is not already present at task start.

This is an incremental context measurement, not a claim that the route caused
existing task-start context. When every selected source is already present, the
human result says:

```text
Selecting this route adds: none; its required context is already read at task start
```

An empty selection difference is a measured zero and is complete. Human output
may say `none`; structured output retains numeric zero. It is not an unavailable
measurement.

### Automatically Read Below Through `#LoadNow`

For an entrypoint, `Automatically read below it` measures the unique descendant
sources reached through visible `#LoadNow` traversal after that entrypoint is
read. It excludes the inspected source and ancestor chain. It includes valid
overwrite layers immediately after their bases.

This measure is not applicable to an ordinary routed leaf. It remains distinct
from all structural descendants because on-demand descendants are not read only
because an ancestor entrypoint was read.

An entrypoint with no automatically read descendants has a measured zero
(`none` in human output), not an unavailable value. For a detached entrypoint,
local `#LoadNow` descendants remain applicable when its local topology is safe;
Loader-rooted reading comparisons remain `not-applicable`. For a known supported
unrouted source, route-based selection and descendant measures are
`not-applicable`.

Every applicable measurement distinguishes a numeric value, including zero,
from `unavailable` and `not-applicable`. An applicable value that cannot be
measured remains visibly `unavailable` and selects `incomplete`. Structured output
keeps all three states; human output never renders either unavailable or
not-applicable as zero.

### No Heaviness Score

The command reports exact measurements. It never assigns a `light`, `heavy`,
`too large`, grade, risk score, budget, or recommendation. Changes in size do
not produce `completed-with-warnings` by themselves. Users and automation may compare the
neutral facts with their own needs.

## Route Structure

For a routed source or a detached entrypoint with safe local topology, the
result reports:

- Root route.
- Ordered route chain.
- Direct exposing parent.
- Route depth.
- Direct routed file and child entrypoint counts when the subject is an
  entrypoint.
- Descendant routed file and entrypoint counts when the subject is an
  entrypoint.

Route depth is the number of source-ID route segments from the root route
through the inspected subject. A root entrypoint has depth `1`. A detached tree
uses the same local route-segment measure without claiming a Loader-rooted root
route.

Direct children and descendant counts come from current filesystem topology and
recognized source contracts, not from trusting current generated lines as an
independent inventory. `route inspect` does not compare that topology with the
current generated `Entries`; `index` regenerates `Entries`, and `doctor` owns
structural diagnosis.

For an ordinary routed leaf, child and descendant counts are not applicable. A
detached entrypoint reports applicable local topology, while a known supported
unrouted source reports route-dependent topology as not-applicable. An entrypoint
with no children reports numeric zero rather than omitting a measured empty
structure. minimal-detail output keeps these zero and not-applicable distinctions
visible.

### Why There Is No Scope Count

A scope is a narrowing role performed by a routed slug and entrypoint. It is not
a separately declared file type. Some slugs establish subject scopes while
others carry a defined route, component, or Memory-state role.

The command therefore does not count folders, route segments, entrypoints, or
tags and call them scopes. It shows the exact route chain, depth, parent,
children, and descendants. A mechanically established named scope role may be
retained in structured provenance, but unknown roles remain unknown.

## Rules And Customization

The result reports:

- Ordered Loader and ancestor entrypoint sources that contribute inherited
  `Axioms`.
- Whether the inspected entrypoint contributes local substantive `Axioms`.
- The base and overwrite relationship.

Human output lists source IDs, not rule bodies. Use `context` with exact section
projection to read inherited rules.

An inherited sentinel, empty section, or missing optional local `Axioms`
section contributes no new local rule. The command does not interpret an
ordinary `Axioms` heading in a routed leaf as active Framework rules because
only the Loader and recognized entrypoints can define them.

The command does not report Extension ownership or managed-file state. Those
are lifecycle facts rather than the reading and route behavior inspected here.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command-specific context, full
adds all bounded facts, and debug adds bounded diagnostics on stderr. Detail
does not change semantics, counts, or status. Filters select finding severities;
all is the default filter.



The catalogue text by detail level is:

`minimal`:

```text
memory  .agents/memory/_memory.md

Where this source belongs
  Route chain: memory
  Parent: none (Loader root)
  Direct children: 4 entrypoints
  Descendants: 11 entrypoints

When it is read
  At task start or resume: yes
  Read automatically when the Loader is read
  May be read again later: no

Context size
  This file: 3.36 KiB, about 861 tokens
  Selecting this route adds: nothing (already in startup context)
  Read automatically below it through #LoadNow: 6 files, 7.03 KiB, about 1801 tokens
```

Lines that do not apply are omitted: `Direct children` when a file has none,
`Read automatically below it` when nothing is tagged. Unusual facts appear as
extra lines in the first block: `Entrypoint name: index.md (compatibility
name; the canonical name is _memory.md)`, `Overwrite file: .agents/memory/_memory.overwrite.md`.

`standard` adds `Workspace:`, the Axioms block (`Inherited rules from:
loader`, `Local rules: yes`), and the tags line.

`full` adds the reason for the status, the selected-closure measurements
(`Selected context: 11 files, 14.41 KiB, about 3689 tokens`, `Already in
startup context: ...`), how the source was selected, and the physical layer
paths.

Results with completed, completed-with-warnings, or incomplete status use
stdout. Invalid-input, blocked, failed, and cancelled results use stderr.
A parser failure is text on stderr without a result envelope.

## Structured Output

--format json emits one schema-3 envelope on stdout for each semantic result.
The envelope has exactly these fields:

~~~text
{
  schemaVersion: 3,
  command,
  status,
  detail,
  filter,
  workspace,
  summary,
  findings,
  effects,
  counts,
  limitations,
  data,
  recovery,
  next
}
~~~

The command is exactly route inspect; data follows the catalogue:

| Level    | `data`                                                                                                                                                                                                                                         |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, path, kind, entrypointForm, overwritePath, belongs { routeChain, parent, directChildren { files, entrypoints }, descendants { files, entrypoints } }, read { atStart, automaticallyWhen, mayReadAgain }, size { own, adds, loadNow } }` |
| standard | + `axioms { inheritedFrom: [...], local }`, `tags`                                                                                                                                                                                             |
| full     | + `selected { closure, startupOverlap }`, `selection { kind, method, requested }`, `layers: [ { path, kind } ]`, `statusReason`                                                                                                                |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                    | Headline                                                                                  | Exit | Stream |
| ----------------------- | ------------------------------------------------------- | ----------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | resolved                                                | `<id>  <path>`                                                                            |    0 | stdout |
| completed-with-warnings | resolved by exact path or choice while the ID is shared | headline + warning row `The ID <id> also matches <other>. Use the exact path to be sure.` |    2 | stdout |
| incomplete              | a fact could not be measured or a layer read            | headline, blocks with the missing fact stated, warning rows                               |    3 | stdout |
| invalid-input           | unknown source, the Loader, several operands            | `Cannot inspect <ref>: <problem>.`                                                        |    4 | stderr |
| blocked                 | ambiguous route or overwrite, unsafe path               | `Cannot inspect <ref>: <reason>.`                                                         |    5 | stderr |
| failed                  | unexpected error                                        | `Route inspect stopped because of an unexpected error: <reason>.`                         |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                | `Route inspect was cancelled.`                                                            |  130 | stderr |

### Current merged behavior and open questions

The catalogue's next-action policy conflicts with the actions emitted by the
merged command: some rows emit --help where the catalogue asks for route list,
open-forge doctor where it says fix by hand, or a next action where it says none.
The frozen strings were preserved. Maintainer decision remains open.

The catalogue and current native report also differ in the detail of some
finding and next-action wording. This contract records the current report and
does not choose which wording to retain.

## Errors And Boundaries

The finding catalogue is:

| Code                                   | Severity | Family                | Message                                                                          | Next                                         |
| -------------------------------------- | -------- | --------------------- | -------------------------------------------------------------------------------- | -------------------------------------------- |
| route-inspect.invalid-source-reference | error    | invalid-input         | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.invalid-source-reference`).                          | `open-forge route list --depth=all`          |
| route-inspect.unknown-source           | error    | unknown-source        |                                                                                  |                                              |
| route-inspect.missing-source           | error    | unknown-source        |                                                                                  |                                              |
| route-inspect.missing-source-file      | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.missing-source-file`).                                                         | `open-forge route list --depth=all`          |
| route-inspect.multiple-sources         | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.multiple-sources`).                                                | none                                         |
| route-inspect.loader-subject           | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.loader-subject`).                  | `open-forge route list`                      |
| route-inspect.invalid-workspace        | error    | workspace-unavailable |                                                                                  |                                              |
| route-inspect.workspace-unavailable    | error    | workspace-unavailable |                                                                                  |                                              |
| route-inspect.unsafe-workspace         | error    | workspace-unsafe      |                                                                                  |                                              |
| route-inspect.ambiguous-source         | error    | source-ambiguous      | (the prompt resolves it in a terminal)                                           |                                              |
| route-inspect.ambiguous-route          | error    | route-ambiguous       |                                                                                  |                                              |
| route-inspect.ambiguous-overwrite      | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.ambiguous-overwrite`).                   | fix by hand                                  |
| route-inspect.unsafe-source            | error    | source-unsafe         |                                                                                  |                                              |
| route-inspect.unsupported-source       | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.unsupported-source`).                             | none                                         |
| route-inspect.orphan-overwrite         | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.orphan-overwrite`).                                | fix by hand                                  |
| route-inspect.unreadable-source        | warning  | inspection-incomplete |                                                                                  |                                              |
| route-inspect.incomplete-route         | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.incomplete-route`).    | `open-forge doctor`                          |
| route-inspect.unavailable-fact         | warning  | local                 | `<fact> could not be measured: <reason>.` (rendered in place of the block line)  | `open-forge doctor`                          |
| route-inspect.automatic-id-not-unique  | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.automatic-id-not-unique`).          | none                                         |
| route-inspect.not-routed               | info     | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.not-routed`). | `open-forge index` when its parent is routed |
| route-inspect.detached-source          | info     | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.detached-source`).                        | none                                         |
| route-inspect.compatibility-entrypoint | info     | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.compatibility-entrypoint`). | none                                         |
| route-inspect.valid-overwrite          | info     | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Inspect/Shared/Wording/RouteInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-inspect.valid-overwrite`).                                      | none                                         |
| route-inspect.operation-failed         | error    | operation-failed      |                                                                                  |                                              |
| route-inspect.interrupted              | error    | interrupted           |                                                                                  |                                              |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`ownBytes`, `ownTokens`, `addedFiles`, `addedBytes`, `addedTokens`,
`loadNowFiles`, `loadNowBytes`, `loadNowTokens`, `directChildren`, `descendants`.

## Scenarios

`entrypoint`, `routed-file`, `load-now-child`, `keep-in-mind`, `overwrite-pair`,
`compatibility-entrypoint`, `not-routed-file`, `id-not-unique-exact-path`
(warnings), `ambiguous-id-prompt`, `unknown-source` (invalid), `loader-subject`
(invalid), `unreadable-source` (incomplete), `orphan-overwrite` (blocked).

Prompt rules from the catalogue:

When an ID matches several files in a terminal: Select among the paths (see
[04 interaction system](../../../../../../archived/cli-development/tasks/task30-g4/04-interaction-system.md)). The result then carries the warning row above.

## Representative Transcripts

### completed

~~~text
docs  .agents/docs/_docs.md

Where this source belongs
  Route chain: docs
  Parent: none (Loader root)
  Direct children: 2 files
  Descendants: 2 files

When it is read
  At task start or resume: yes
  Read automatically when the Loader is read
  May be read again: no

Context size
  This file: 156 B, about 39 tokens
  Selecting this route adds: nothing (already in startup context)
~~~

### completed-with-warnings

~~~text
docs/guide  .agents/docs/guide.md
  Warning  docs/guide  ID is not unique
         The ID docs/guide also matches .agents/docs/guide/_guide.md. Use the exact path to be sure.
~~~

### incomplete

~~~text
docs/guide  .agents/docs/guide.md
  Warning  .agents/docs/guide.md  Source could not be read
         .agents/docs/guide.md could not be read completely.

Where this source belongs
  Route chain: docs -> guide
  Parent: docs

When it is read
  At task start or resume could not be measured: The loading facts needed for reading classification are unavailable.
  Read automatically could not be measured: The loading facts needed for reading classification are unavailable.
  May be read again could not be measured: The loading facts needed for reading classification are unavailable.

Context size
  This file could not be measured: The selected source body is unavailable.
  Selecting this route adds could not be measured: The selection addition cannot be established completely.
Next: open-forge doctor
~~~

### invalid-input

~~~text
Cannot inspect missing/source: No source has the ID missing/source.
Next: open-forge route inspect --help
~~~

### blocked

~~~text
Cannot inspect .agents/orphan.overwrite.md: orphan.overwrite.md has no orphan.md beside it.
Workspace: <workspace>
Next: open-forge doctor
~~~

### failed

~~~text
Route inspect stopped because of an unexpected error: <reason>.
~~~

### cancelled

~~~text
Route inspect was cancelled.
~~~

## Related Current Sources

- [route inspect Command Contract Set](_inspect.md)
- [Route Inspect Behavior Contract](behavior.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [Context Interface Contract](../../context/interface.md)
- [Status Interface Contract](../../status/interface.md)
- [Doctor Interface Contract](../../doctor/interface.md)
- [Route Init Interface Contract](../init/interface.md)
- [Route Create Interface Contract](../create/interface.md)
- [Route Update Interface Contract](../update/interface.md)
- [Find Interface Contract](../../find/interface.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Loading And Refreshing Context](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Core Primitive Model](../../../../framework/primitives/model.md)
- [Open Forge Framework Architecture](../../../../framework/architecture.md)
- [Accepted State And Synchronization](../../../../framework/truth.md)
- [Open Forge Principles](../../../../principles.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.inspect.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Inspect/RouteInspectText.cs).

<!-- @OpenForgeTextRef route.inspect.help.syntax -->
