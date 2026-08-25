---
open-forge:
  description: Accepted current public grammar and observable one-source route profile for `route inspect`
  responsibility: Define what a caller may enter and observe for `route inspect`
  tags: [Memory, Crystallized, CLI, Release, Command, Route, Inspect, Interface, CurrentTruth]
---

# route inspect Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route inspect`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral operation behind this public surface. The shared
[Global CLI Flags](../../shared/global-flags/interface.md) and [CLI Source References](../../shared/source-references/interface.md)
contracts retain their shared meanings rather than being redefined here.

The current [Routing Model](../../../../framework/routing/model.md),
[Routing Loading And Continuity](../../../../framework/routing/loading.md),
[Route Scope And Inheritance](../../../../framework/routing/scope.md),
and [Overwrite Customization](../../../../framework/routing/overwrites.md)
documents define the Framework meaning inspected by this command. The
accepted [Context Interface Contract](../../context/interface.md) defines selected closures,
and the [Status Interface Contract](../../status/interface.md) defines the shared physical
measurements and token estimate.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
JSON envelope, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, and diagnostic realization. Primary
human `complete`, `attention`, and `incomplete` results use
stdout. Primary human `invalid`, `blocked`, `failed`, and `interrupted` results
use stderr. `--json` writes one complete structured result to stdout for every
status; bounded diagnostics use stderr, and human text is never mixed into JSON
stdout. Command-specific repetition beyond the shared global flags is not
invented here.

The CLI Architecture defines the shared envelope, schema compatibility,
process-status mapping, serialization, parser and filesystem realization,
physical identity, containment, diagnostics, source structure, package and
runtime boundaries, and test boundaries. This Interface defines the exact
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
`--workspace`, `--json`, `--view`, `--verbose`, `--help`, and `--version`. All
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
is still non-unique, the result is `attention`. Exact path input cannot turn an
ambiguous route relationship into a valid one. A structurally ambiguous route
remains blocked even when an exact path selects one recognized file.

Selecting a valid overwrite reference inspects the base and overwrite as one
logical source. The overwrite inherits the base route, reading behavior, and
scope. It is never reported as an independent route.

## Source-State Classification

The command uses this finite source-state classification after input resolution.
The table assumes that no higher incomplete, blocked, failed, or interrupted
condition applies. A state describes what the command can safely establish; it
does not diagnose the workspace or recommend a change.

| Established source state                                                                                     | Semantic result           | Route-profile treatment                                                                                                                                                     |
| ------------------------------------------------------------------------------------------------------------ | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Routed entrypoint, routed leaf, or routed native source                                                      | `complete`                | Applicable route, reading, topology, and measurements are reported.                                                                                                         |
| Accepted compatibility entrypoint filename                                                                   | `complete`                | The filename is reported as compatibility input; its route facts remain ordinary complete facts.                                                                            |
| Valid base and overwrite pair                                                                                | `complete`                | The pair is one logical source with base-first physical layers. Customization is neutral.                                                                                   |
| Detached entrypoint with safe local identity and topology                                                    | `complete`                | Local route facts are reported. Loader-root reading facts are not-applicable.                                                                                               |
| Known supported source with no established route                                                             | `complete`                | The source identity and applicable physical measurements are reported; route-dependent facts are not-applicable.                                                            |
| Safe source and route with a non-unique automatic ID, resolved by exact path or interactive source selection | `attention`               | The non-unique ID remains an observation. Interactive selection requires an exact path for later non-interactive use; exact-path selection has no invented required action. |
| Safe identity with an unreadable required source, incomplete route chain, or unmeasurable applicable fact    | `incomplete`              | Safe observations remain visible and the affected availability is `unavailable`.                                                                                            |
| Orphan or ambiguous overwrite, ambiguous route, unsafe identity, or containment failure                      | `blocked`                 | The command does not choose a pair, route, alias, or out-of-bound source.                                                                                                   |
| Zero or several operands, Loader, unknown, missing, or unsupported source                                    | `invalid`                 | Input is rejected before route inspection.                                                                                                                                  |
| Unexpected failure or caller interruption before completion                                                  | `failed` or `interrupted` | The event status retains its own meaning.                                                                                                                                   |

When several ordinary conditions occur, `blocked` takes precedence over
`incomplete`, `incomplete` over `attention`, and `attention` over `complete`.
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
- A routed `#KeepInMind` file that is not an entrypoint is read at the defined
  task and later review points even when another route is active.
- A `#KeepInMind` entrypoint is read when it is visible from task-start routing,
  when work selects that route or scope, or when it is needed as an ancestor of
  active content.
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
corresponding event in ordinary words.

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
not produce `attention` by themselves. Users and automation may compare the
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
structure. Compact output keeps these zero and not-applicable distinctions
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

The default expanded human output contains route facts, measurements,
observations, availability conditions, explanations, and provenance useful for
understanding one source. It uses plain explanations instead of internal stage
terms. It does not diagnose the workspace or recommend a route or content
change.

Both human views begin with the selected workspace, selection method, source ID,
and canonical path. Workspace identity remains separate from route identity.

Compact view is one stable summary. It retains:

- Workspace, selection method, ID, path, source state, and route state.
- Route chain, applicable parent, depth, and child structure.
- Task-start, automatic-reading, and later-reading behavior.
- Own-source, selection-addition, and `#LoadNow` descendant measurements.
- Numeric zero, `unavailable`, and `not-applicable` distinctions.
- Overwrite state, semantic status, completeness, and safety.
- At most one required operation-level `Next:` line.

Compact output omits inherited-`Axioms` detail and optional explanation. It keeps
an observation or availability condition when that condition is needed to
understand `attention`, `incomplete`, or `blocked`; it does not turn that
condition into a diagnosis.

Illustrative output:

```text
Open Forge route inspect
Workspace: D:/Repositories/open-forge
Selected by: current directory
ID: memory/working/checkpoints
Path: .agents/memory/working/checkpoints/_checkpoints.md
Entrypoint: canonical

Reading behavior
  Read at task start or resume: yes
  Why: memory/working exposes it as context that should be revisited during the task
  May be read again: yes
  When: after context restoration, before handoff or closeout, or after a change that may affect its follow-up work

Context cost
  This source: 3.7 KiB · ~955 tokens
  Selecting this route adds: none; its required context is already read at task start
  Automatically read below it through #LoadNow: 1 file · 2.1 KiB · ~525 tokens

Route structure
  Parent: memory/working
  Route chain: memory → working → checkpoints
  Depth: 3
  Direct children: 7 files, 1 child entrypoint
  All descendants: 12 files, 2 descendant entrypoints

Rules and customization
  Axioms inherited from: loader → memory → working
  Local Axioms: none
  Overwrite: none
```

The values are illustrative. They do not claim to measure this repository.

An illustrative compact result is:

```text
Open Forge route inspect
Workspace: D:/Repositories/open-forge
Selected by: current directory
ID: memory/working/checkpoints
Path: .agents/memory/working/checkpoints/_checkpoints.md
Source state: routed entrypoint
Route state: routed
Route chain: memory → working → checkpoints
Parent: memory/working
Depth: 3
Direct children: 7 files, 1 child entrypoint
Descendants: 12 files, 2 descendant entrypoints
Read at task start or resume: yes
Read automatically when: memory/working is read
May be read again: yes
Later: after context restoration, before handoff or closeout, or after a change that may affect its follow-up work
Own source: 1 file · 3.7 KiB · ~955 tokens
Selecting this route adds: none (0 files · 0 B · 0 tokens)
Automatically read below it through #LoadNow: 1 file · 2.1 KiB · ~525 tokens
Overwrite: none
Status: complete
Completeness: complete
Safety: safe
```

The values are illustrative. A complete entrypoint with no automatic
descendants reports a measured zero, while an ordinary routed leaf reports its
descendant measure as `not-applicable`. Detached and known unrouted subjects
show their Loader-rooted or route-dependent facts as `not-applicable`; an
applicable unmeasurable fact remains `unavailable`.

Human output may say `none` for a measured zero. It never uses `none` or zero to
stand for an unavailable or not-applicable fact.

The human result has no workspace startup table, total workspace inventory,
character count, Extension summary, recovery summary, scope count, health
grade, or recommendation. Those facts either belong to another operation or do
not have safe route-level meaning.

`--verbose` adds bounded diagnostics under the shared global contract. It does
not add source bodies, change measurement sets, or turn inspection into
diagnosis.

Primary human `complete`, `attention`, and `incomplete` results use stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. `--json` writes one complete structured result to stdout for every
semantic status. Separate bounded diagnostics use stderr, and no human text is
mixed into JSON stdout.

Human results expose at most one required `Next:` line:

- `complete`: no `Next:` line.
- `attention`: only an interactive source disambiguation adds
  `Next: rerun with the exact path for non-interactive use.` An exact-path
  selection preserves the non-unique-ID observation without inventing an action.
- An unresolved source-ID collision in a non-interactive or JSON request is
  `blocked`, retains every candidate path, and uses `Next: rerun with one of the
listed exact paths.`
- `incomplete` or another structural `blocked`: name a direct safe correction
  when it is known; otherwise use `Next: open-forge doctor`.
- `invalid`: `Next: correct the named source or input.`
- `failed`: `Next: report the failure and retry with bounded diagnostics.`
- `interrupted`: `Next: rerun the same request.`

A direct safe correction names only the observed input or safety boundary when
that correction is already known. It is not a route mutation proposal.

The structured `next` member uses `{ command, reason }` with these exact values:

- `complete`, and `attention` after exact-path selection: null;
- interactive `attention`: the command is `open-forge route inspect
  "<canonical-path>"`, and the reason is `Rerun with the exact path for
  non-interactive use.`;
- `invalid`: `{ command: "open-forge route inspect --help", reason: "Correct
  the named source or input, then rerun route inspect." }`;
- `incomplete`: `{ command: "open-forge doctor", reason: "Review the unavailable
  route fact, then rerun route inspect." }`;
- a blocked source-ID collision: the command reruns `open-forge route inspect`
  with the first listed exact path, and the reason is `Rerun with one listed
  exact path to resolve the source collision.`;
- an ambiguous-route block: the command reruns `open-forge route inspect` with
  the selected exact path, and the reason is `Rerun with the exact source path
  after resolving the ambiguous route.`;
- another `blocked` result: `{ command: "open-forge doctor", reason: "Review the
  blocked source boundary, then rerun route inspect." }`;
- `failed`: `{ command: "open-forge route inspect", reason: "Address the reported
  failure, then retry route inspect." }`; and
- `interrupted`: `{ command: "open-forge route inspect", reason: "Rerun the same
  route-inspect request." }`.

`route inspect` never emits route mutation proposals, health recommendations,
content-placement advice, or diagnostic recommendations. `doctor` owns complete
diagnosis and recommendations; this command reports only observations,
availability conditions, and required next operations.

## Structured Output

`--json` returns the complete typed result used by human rendering. It never
prompts and never reruns resolution or measurement.

The structured result exposes:

- Workspace and selection method.
- Requested reference and resolved identity.
- Every candidate path for an unresolved source-ID collision.
- Source kind, route state, canonical or compatibility form, and physical
  layers.
- Task-start membership, later-read membership, automatic-reading event, and
  typed reasons.
- Exact own-source, selected-closure, task-start-overlap, selection-addition,
  and `#LoadNow` descendant measurements.
- Root route, route chain, depth, parent, direct counts, and descendant counts.
- Inherited and local Axioms source provenance.
- Overwrite relationship.
- Directly observed source and route facts.
- Availability conditions for every reported fact, including numeric zero,
  `unavailable`, and `not-applicable` states.
- Semantic status and next operations only when a required next operation exists.

The accepted camel-case command-local `result` object uses this exact member
shape and order. Every listed member is present. Generic `Fact<T>` values use
`{ state, value, reason }`; Boolean facts use the same members with a Boolean or
null `value`.

```text
result: {
  selection: {
    referenceKind: ReferenceKind,
    selectionMethod: SelectionMethod,
    requestedReference: string | null,
    candidatePaths: string[]
  },
  identity: {
    id: string,
    path: string,
    sourceKind: SourceKind,
    sourceForm: SourceForm,
    routeState: RouteState,
    physicalLayers: [{
      workspaceRelativePath: string,
      physicalPath: string,
      role: LayerRole
    }]
  } | null,
  profile: {
    reading: {
      taskStart: BooleanFact,
      automatic: Fact<{
        reasons: [{
          kind: ReadingKind,
          relatedSourceId: string | null,
          events: ReadingEvent[]
        }]
      }>,
      later: Fact<{
        mayBeReadAgain: boolean,
        occasions: LaterOccasion[]
      }>
    },
    measurements: {
      ownSource: Fact<Measurement>,
      selectedClosure: Fact<Measurement>,
      taskStartOverlap: Fact<Measurement>,
      selectionAddition: Fact<Measurement>,
      loadNowDescendants: Fact<Measurement>
    },
    topology: Fact<{
      rootRoute: string,
      routeChain: string[],
      parentId: string | null,
      depth: nonnegative-integer,
      counts: Fact<{
        directRoutedFileCount: nonnegative-integer,
        directEntrypointCount: nonnegative-integer,
        descendantRoutedFileCount: nonnegative-integer,
        descendantEntrypointCount: nonnegative-integer
      }>
    }>,
    axioms: Fact<{
      inherited: Fact<{ sourceIds: string[] }>,
      local: Fact<LocalAxiomsState>
    }>,
    completeness: Completeness,
    safety: Safety
  } | null,
  observations: [{
    code: ObservationCode,
    subject: string,
    message: string,
    paths: string[]
  }],
  conditions: [{
    code: ConditionCode,
    status: SharedStatus,
    subject: string,
    message: string,
    paths: string[]
  }]
}

Measurement: {
  physicalFileCount: nonnegative-integer,
  unicodeScalarCount: nonnegative-integer,
  utf8ByteCount: nonnegative-integer,
  estimatedTokens: nonnegative-integer
}

Fact<T>: {
  state: FactState,
  value: T | null,
  reason: string | null
}

BooleanFact: {
  state: FactState,
  value: boolean | null,
  reason: string | null
}
```

String, Boolean, integer, array, and null members use their JSON types.
Measurement and topology count members are nonnegative integers. `depth` is a
nonnegative integer. The finite strings are:

- `referenceKind`: `missing`, `source-id`, `source-path`, or `invalid`;
- `selectionMethod`: `unresolved`, `automatic-id`, `exact-path`, or
  `interactive`;
- `sourceKind`: `entrypoint`, `markdown`, or `native`;
- `sourceForm`: `canonical`, `compatibility`, `markdown`, or `native`;
- `routeState`: `routed`, `detached`, `not-routed`, `ambiguous`, or
  `unresolved`;
- physical-layer `role`: `base` or `overwrite`;
- fact `state`: `value`, `unavailable`, or `not-applicable`;
- automatic-reading `kind`: `on-demand`, `parent-load-now`,
  `entrypoint-keep-in-mind`, `routed-file-keep-in-mind`, or
  `overwrite-after-base`;
- automatic-reading `events`: `route-selected`, `exposing-parent-read`,
  `task-start-visible`, `scope-selected`, `ancestor-required`, `task-review`,
  `later-review`, or `base-read`;
- later-reading `occasions`: `context-restoration`, `handoff`, `closeout`, or
  `followup-transition`;
- profile `completeness`: `complete`, `incomplete`, or `not-started`;
- profile `safety`: `safe`, `blocked`, or `unknown`; and
- local Axioms fact value: `substantive`, `inherited-sentinel`, `empty`,
  `missing`, or `not-applicable`.

A fact in `value` state has a non-null value and null reason. An `unavailable` or
`not-applicable` fact has null value and one nonempty reason. Arrays are always
present. The exact observation codes are
`route-inspect.automatic-id-not-unique`,
`route-inspect.compatibility-entrypoint`, `route-inspect.detached-source`,
`route-inspect.not-routed`, and `route-inspect.valid-overwrite`.

The exact condition codes and their condition status are:

- `invalid`: `route-inspect.invalid-workspace`, `route-inspect.missing-source`,
  `route-inspect.multiple-sources`, `route-inspect.invalid-source-reference`,
  `route-inspect.loader-subject`, `route-inspect.unknown-source`,
  `route-inspect.missing-source-file`, and
  `route-inspect.unsupported-source`;
- `blocked`: `route-inspect.workspace-unavailable`,
  `route-inspect.unsafe-workspace`, `route-inspect.ambiguous-source`,
  `route-inspect.unsafe-source`, `route-inspect.ambiguous-route`,
  `route-inspect.orphan-overwrite`, and `route-inspect.ambiguous-overwrite`;
- `incomplete`: `route-inspect.unreadable-source`,
  `route-inspect.incomplete-route`, and `route-inspect.unavailable-fact`;
- `failed`: `route-inspect.operation-failed`; and
- `interrupted`: `route-inspect.interrupted`.

`command` in the shared envelope is exactly `route inspect`. The envelope's
`workspace` uses the shared Architecture member, and `next` uses that shared
shape with the exact Route Inspect values above; neither is repeated inside
`result`. Arrays are present when empty. `requestedReference`, `identity`,
`profile`, nullable fact values, fact reasons, `relatedSourceId`, and `parentId`
retain null when their typed fact is unavailable or inapplicable under this
Interface.

The result does not include authored source bodies or sections, ordinary links,
all route paths in the workspace, a generated-index comparison, diagnosis,
recommendations, or route mutation proposals. Exact field names, schema
versioning, and command-local field meaning are defined by this Interface. The
shared envelope, compatibility coordinates, and serialization realization are
defined by the CLI Architecture.

`--view` is accepted with `--json` but has no effect because JSON always emits
the complete structured result under the shared global contract.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                             |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Every applicable inspection fact and measurement was established for a safely resolved source. This includes routed sources, accepted compatibility entrypoints, valid overwrite pairs, detached entrypoints, and known supported unrouted sources. |
| `attention`   | The physical source and route are safe and complete, but the automatic source ID is non-unique. The source identity was resolved by exact path or interactive selection.                                                                            |
| `incomplete`  | Safe identity is established, but an unreadable required source, incomplete route chain, or unmeasurable applicable fact leaves the profile incomplete.                                                                                             |
| `invalid`     | The request has zero or several subjects, names the Loader, or uses an unknown, missing, unsupported, or otherwise invalid source or input.                                                                                                         |
| `blocked`     | The command cannot establish a safe source or route boundary because identity, containment, route, or overwrite meaning is ambiguous or unsafe.                                                                                                     |
| `failed`      | An unexpected failure prevents normal completion.                                                                                                                                                                                                   |
| `interrupted` | The caller cancels or interrupts the operation before completion.                                                                                                                                                                                   |

Compatibility filenames, detached routes, known unrouted sources, valid overwrite
customization, route depth, added context, and context size are neutral facts.
None produces `attention` by itself. The only ordinary `attention` condition is
the safely resolved non-unique automatic ID described above.

For ordinary conditions, the precedence is `blocked`, `incomplete`, `attention`,
then `complete`. Invalid input stops before operation work; failed and
interrupted retain their event meanings.

The shared process-status mapping is defined by the CLI Architecture.

## Errors

Every error names the inspection operation, requested source, direct cause, and
useful next operation when one exists.

- A missing source reference or several source references are invalid.
- An empty, unknown, missing, or unsupported source reference is invalid.
- A Loader subject is invalid for this operation.
- An unresolved ambiguous source ID is blocked, retains every candidate path,
  and tells the caller to rerun with one listed exact path. Exact-path or
  interactive source selection may resolve only source identity; a safe,
  complete route with a still non-unique automatic ID is `attention`.
- An ambiguous route remains blocked even when an exact path selects one
  recognized file.
- An unsafe path, physical identity, or containment boundary is blocked.
- An orphan or ambiguous overwrite pair is blocked. A valid base and overwrite
  pair is one logical source and does not create attention.
- An incomplete route chain, unreadable required source, or unmeasurable source
  makes affected facts incomplete rather than guessed.

`doctor` provides complete diagnosis and recommendations. `route inspect`
reports only observations and availability conditions needed to answer its
route-profile questions.

## Scenarios

The smallest valid domain invocation has one source reference:

```text
open-forge route inspect memory/working/checkpoints
```

It resolves one known source and returns its route profile. The same logical
source may be identified by an exact path:

```text
open-forge route inspect ".agents/memory/working/checkpoints/_checkpoints.md"
open-forge route inspect "./.agents/memory/working/checkpoints/_checkpoints.md"
```

An overwrite path is also one subject when it names a valid pair:

```text
open-forge route inspect ".agents/<route>/<name>.overwrite.md"
```

The result reports the base and overwrite as one logical source rather than as
an independent route.

Compact human presentation and complete structured presentation are selected
without changing inspection:

```text
open-forge route inspect memory/working/checkpoints --view=compact
open-forge route inspect memory/working/checkpoints --view=expanded
open-forge route inspect memory/working/checkpoints --json
open-forge route inspect memory/working/checkpoints --json --view=compact
```

The first two select human density, the third returns the complete typed result,
and the fourth accepts `--view` as a JSON no-op. A verbose request adds bounded
diagnostics without changing the result:

```text
open-forge route inspect memory/working/checkpoints --verbose
```

Terminal informational modes follow the shared global contract and do not
inspect a subject:

```text
open-forge route inspect --help
open-forge route inspect --version
```

An explicitly selected detached entrypoint reports local identity and topology
without claiming Loader-rooted reading facts. An existing supported exact path
with no established route reports a complete `not routed` identity, with
route-dependent fields not-applicable. An unresolved ambiguous identity or route
relationship returns `blocked`.

When an automatic-ID collision is resolved interactively, the selected source
and route can be complete while the result is `attention`:

```text
Observation: automatic ID is not unique
Next: rerun with the exact path for non-interactive use.
```

When the same source is selected by its exact path, the result preserves the
non-unique-ID observation but emits no required next action. An entrypoint with
no `#LoadNow` descendants reports measured zero, an ordinary routed leaf reports
that descendant measure as not-applicable, and an applicable unreadable measure
is visibly unavailable and incomplete.

In a non-interactive or JSON request, an unresolved automatic-ID collision is
`blocked`, returns every candidate path, and says to rerun with one listed exact
path. It never chooses a candidate.

## Non-Goals

`route inspect` does not:

- Replace or rename `status`.
- Provide a generic `inspect` operation for unrelated subjects.
- Discover a source or list workspace routes.
- Return frontmatter, bodies, sections, or ordinary linked content.
- Follow links.
- Report actual model, runtime, cache, billing, latency, or memory behavior.
- Count scopes by inferring semantic roles from paths or tags.
- Assign a heaviness score, budget, grade, or recommendation.
- Diagnose workspace health, emit diagnostic recommendations, or advise content
  placement.
- Validate every route, generated region, link, overwrite, or Framework rule.
- Compare or repair generated `Entries`.
- Propose route mutations or other lifecycle changes.
- Mutate, format, index, repair, install, update, move, or remove anything.
- Create a Git commit.

Use `find` to discover sources, `context` to return selected content, `status` to
summarize the workspace, `doctor` to diagnose problems, and the other `route`
operations to change routed files or entrypoints.

## Verification

Gate 5 executable proof must cover:

- Exact CWD and `--workspace` selection without discovery.
- IDs, exact paths, quoting, collisions, and disambiguation from the shared
  source-reference contract.
- Routed entrypoints, routed leaves, native routed sources, canonical and each
  compatibility entrypoint filename, detached entrypoints, known unrouted
  sources, and unsupported source kinds.
- Base, overwrite-path, orphan, ambiguous, and inherited overwrite selection.
- The finite source-state classification, including accepted compatibility,
  detached, known unrouted, safely resolved non-unique-ID, incomplete,
  blocked, invalid, failed, and interrupted states.
- Task-start membership that is independent of the inspection operand.
- Plain human explanations for task-start, parent-triggered, selected, and later
  reads, including another transition that may affect standing follow-up work,
  without `target-sensitive` or `continuity boundary` labels.
- Human and structured distinction among measured zero, unavailable, and
  not-applicable facts, including empty selection, empty `#LoadNow` descendants,
  ordinary leaves, detached entrypoints, and known unrouted sources.
- Workspace, selection method, source ID, and canonical path framing in compact,
  expanded, and structured results.
- Compact retention of ID/path, source and route state, route chain, reading
  behavior, three route measurements, applicable topology, overwrite state,
  status, completeness, safety, and at most one required `Next:` line.
- Interactive collision recovery, exact-path non-unique-ID observation, and the
  absence of an invented action for an already exact-path selection, plus
  blocked non-interactive collisions with every candidate path and exact-path
  recovery.
- Human and structured rendering from one typed result.
- Primary human stdout/stderr assignment, one complete JSON result on stdout for
  every status, bounded diagnostics on stderr, and no human text in JSON stdout.
- Observations and availability conditions instead of diagnosis-like findings,
  and no route, health, placement, or diagnostic recommendations.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  outcomes.
- Evidence that size and customization do not create attention or
  recommendations.

The [Behavior Contract](behavior.md#conformance-evidence) owns the semantic
evidence for graph construction, reading classification, set calculation,
measurement, topology, inheritance provenance, availability, read-only safety,
and process-level conformance. The source and test modality remain visible
there, including the required direct, integration, and built-process evidence.

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
- [Routing Loading And Continuity](../../../../framework/routing/loading.md)
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
