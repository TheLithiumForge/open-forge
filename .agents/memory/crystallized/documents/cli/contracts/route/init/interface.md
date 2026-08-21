---
open-forge:
  description: Accepted current public interface and observable result for recursively initializing missing route entrypoints
  responsibility: Define what a caller may enter and observe from `route init`
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Init, Entrypoint, Interface, CurrentTruth]
---

# route init Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route init`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The current [Routed Markdown Representation](../../../../framework/markdown/routes.md)
defines canonical entrypoint syntax. The [Routing Model](../../../../framework/routing/model.md)
defines why every visible folder in a route chain needs one entrypoint. The
[Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
defines which existing compatibility filenames this command recognizes and
preserves.

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines the
complete spelling, values, defaults, repetition, terminal behavior, and errors
for `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and `--version`.
All six apply to `route init` under that contract. The shared [CLI Source
References](../../shared/source-references/interface.md) contract defines source-ID segments,
exact `.agents/...` path detection, quoting, containment, and result identity.
The [Index Interface Contract](../../index/interface.md) defines the generated
navigation projection, ordering, generated boundary, verification, and recovery
behavior consumed by this command.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
structured schema, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, workspace lock, and recovery identity
model. This Interface Contract does not choose those details. The
command-specific repetition, seven-status, stream, finite-attention, and
compact-result rules below are accepted current behavior.

## Purpose

`route init` makes one target folder routable by creating every missing
entrypoint in its route chain. It uses one fixed entrypoint scaffold. It does
not instantiate a Template, create the Loader, or infer semantic meaning from a
folder name.

Given the same workspace bytes and explicit input, the command selects the same
missing entrypoints, produces the same intended files and generated navigation,
and returns the same semantic result. Repeating a successful invocation against
that state returns a verified no-op.

The technology-neutral mechanics behind this public promise are defined in the
[Behavior Contract](behavior.md). This file owns the complete caller-visible
meaning.

## Syntax

```text
open-forge route init <route-target>
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--dry-run]
  [--skip-git-check]
  [global flags]
```

`--description`, `--responsibility`, and `--tag` provide authored metadata for
the final target only. `--dry-run` and `--skip-git-check` are write-policy flags.
The command has no `--template`, `--yes`, `--force`, `--no-responsibility`,
Loader-creation mode, or alias.

The `route` group performs no domain operation by itself. It shows help for its
accepted child operations. The [route group entrypoint](../_route.md)
routes that group relationship, while this leaf's help comes from the complete
public Interface surface under the shared global help rules. The group
description is not a second command contract. Group help does not resolve a
workspace or run a domain operation.

`route init` is an explicit, non-wizard leaf. It has no wizard mode,
`--automatic` mode, alias, or additional operation-specific flag.

## Operands

`<route-target>` is the one required positional operand. Its accepted forms and
resolution rules are defined below.

## Route Target

A route target identifies the entrypoint that should exist after the command.
It accepts an ID for the intended folder under `.agents`, an exact canonical
entrypoint path, or the exact path of an existing recognized compatibility
entrypoint when only missing ancestors need initialization:

```text
<folder-id>
.agents/<folders>/_{final-folder}.md
./.agents/<folders>/_{final-folder}.md
.agents/<folders>/<existing-compatibility-filename>
./.agents/<folders>/<existing-compatibility-filename>
```

An ID is the intended folder ID under `.agents`. The representative invocation
and its canonical path mapping appear under [Scenarios](#scenarios).

An exact path for a missing target must name its canonical entrypoint file. A
directory path, ordinary Markdown filename, Loader path, overwrite path,
`SKILL.md`, or compatibility filename is not a missing-target form.

The compatibility path forms are valid only when that exact file already exists
and the operation needs to initialize missing ancestors. The command preserves
that filename. It never creates a new compatibility filename.

The shared [CLI Source References](../../shared/source-references/interface.md) contract defines
ID segments, exact `.agents/...` path detection, quoting, containment, and result
identity. `route init` adds only the deterministic missing-target mapping above.
It does not guess another target shape from filesystem coincidence.

The target is invalid when it is empty, identifies `loader`, contains `.` or `..`
as an ID segment, cannot be represented as a safe contained entrypoint path, or
uses an exact missing path that does not match the final folder's canonical
filename.

## Flags

The six global flags apply under the shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract. This command does not copy their complete definitions.

| Flag                      | Role              | Value                                                          | Omission                                                                                         | Repetition, ordering, and composition                                                                                                  |
| ------------------------- | ----------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------- |
| `--description <text>`    | Authored metadata | One description value                                          | The final target uses its draft description unless another rule supplies an explicit description | Singleton. Repetition is invalid, including repetition with an equal value.                                                            |
| `--responsibility <text>` | Authored metadata | One responsibility value, including the exact empty value `""` | No responsibility field is added to a missing target                                             | A non-empty value adds the field and `""` omits it. The flag is singleton; any repetition is invalid, including an equal value.        |
| `--tag=<tag>`             | Authored metadata | One tag without a `#` prefix                                   | The final target uses draft metadata and the `NeedsAuthoring` rule                               | Repeatable. Values retain argument order. Empty tags and duplicate exact tags are invalid.                                             |
| `--dry-run`               | Write policy      | No value                                                       | Application is selected                                                                          | Repetition is accepted and idempotent. It composes with `--skip-git-check` as described under [Dry Run And Apply](#dry-run-and-apply). |
| `--skip-git-check`        | Write policy      | No value                                                       | Relevant-path Git cleanliness is checked for an actual mutation                                  | Repetition is accepted and idempotent. It bypasses only the check described under [Dry Run And Apply](#dry-run-and-apply).             |

`--description`, `--responsibility`, and `--tag` are valid only as metadata for a
missing final target. Repeating `--description` or `--responsibility` is invalid,
even when the repeated values are equal. Repeated `--tag` values form one
ordered list; there is no last-wins or other precedence rule. Repeated
`--dry-run` and `--skip-git-check` occurrences collapse to their one idempotent
Boolean choice and do not grant another operation or authority. Global flags
retain the shared contract's repetition, ordering, composition, and terminal
rules.

## Chain Selection

The command examines every folder from the first target segment through the
final target folder. For each folder:

1. One recognized entrypoint already exists: preserve and use it.
2. No recognized entrypoint exists: plan one canonical entrypoint.
3. Several recognized entrypoints exist: block the complete operation.

Existing compatibility names count as recognized entrypoints. The command does
not create a canonical sibling beside one, rename it, or rewrite it merely to
normalize its filename.

An ordinary routed file whose automatic ID collides with one intended entrypoint
blocks the operation. A file or physical path identity that would make two route
subjects indistinguishable also blocks. Exact path input may disambiguate source
selection, but it cannot make an ambiguous route valid.

The command may initialize a detached route chain below `.agents`. It never
creates `.agents/loader.md`. When a valid Loader exists and exposes or can expose
the first route folder, its generated region joins the parent mutation. When no
Loader exists, the first entrypoint remains detached. A present but ambiguous or
unsafe Loader relationship blocks rather than being ignored.

Existing entrypoints are read-only authored inputs except for bounded generated
`Entries` effects required by the intended topology. Supplying final metadata
flags when the final target entrypoint already exists is invalid. Use `route
update` to change an existing source.

## Fixed Entrypoint Scaffold

Every missing folder receives the same canonical structure. The following is the
exact scaffold example for the final target from the route-target mapping above:

```md
---
open-forge:
  description: Draft route for memory/project-alpha/documents; replace this description before relying on it for selection
  tags: [NeedsAuthoring]
---

# documents

Draft route for memory/project-alpha/documents; replace this description before relying on it for selection.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->

- none - No entries - #Empty

<!-- open-forge:generated-index:end -->
```

The example route ID and literal final slug vary by target. The scaffold always
contains:

1. Canonical scoped frontmatter.
2. A non-empty route description and at least one tag.
3. One level-1 title whose visible text is the literal folder slug.
4. The same honest route description appears in the body.
5. The canonical inherited `Axioms` sentinel.
6. One final `Entries` section with a valid generated marker pair.

Markdown escaping may preserve the literal slug as visible heading text. It must
not humanize, title-case, expand, summarize, or assign semantic meaning to the
slug.

The fixed scaffold is command behavior, not a Template instance. Later changes
to a Template never affect it, and `route init` does not search the Templates
route for a default.

## Draft Metadata

Every missing ancestor receives its path-derived draft description and the
`NeedsAuthoring` tag. These values state the source's current draft condition;
they do not claim what the route will eventually contain.

Metadata flags affect only a missing final target:

- A non-empty `--description` replaces the final draft description in both
  frontmatter and the compact body definition.
- A non-empty `--responsibility` adds that optional frontmatter field.
- An exact empty `--responsibility ""` omits the field.
- Repeated `--tag` values provide the final target's explicit tags in argument
  order.

When the final target lacks either an explicit description or any explicit tag,
the command ensures that its tag list contains `NeedsAuthoring`. It appends that
tag after any supplied tags unless the caller already included it. When both a
description and one or more tags are supplied, the command uses the exact
supplied tag list and does not add `NeedsAuthoring`.

An empty or whitespace-only description is invalid. A whitespace-only
responsibility is invalid. Each tag must follow canonical tag syntax and omit the
`#` prefix. Empty tags, duplicate exact tags, and a supplied empty tag set are
invalid.

The command validates syntax and presence. It does not judge or rewrite the
semantic accuracy of supplied prose and tags. A later `doctor` operation may
report meaning-quality diagnostics separately.

### Finite attention condition

After a safe complete plan and preflight have been established, the result is
`attention` when this invocation safely previews or creates any **new** entrypoint
whose intended tags contain the exact `NeedsAuthoring` tag. This includes every
new draft ancestor and a draft final target, whether the marker was supplied by
the command's draft rule or explicitly retained by the caller. Human output says
`requires attention`; structured output retains `attention`.

An unchanged existing entrypoint that already contains `NeedsAuthoring` does not
change an otherwise verified no-op from `complete`. Planned changes alone do not
produce `attention`. When every new entrypoint has complete intended metadata
without the exact marker and no other status condition applies, application and
dry-run results are `complete`. This condition does not score route health or
semantic quality.

## Intended Topology And Generated Entries

The command plans generated navigation against the complete intended route chain
before any persistent effect begins. Each new entrypoint initially has a valid
empty generated region. Automatic generated-navigation effects then add:

- Every new direct child entrypoint to its intended parent entrypoint.
- The first new entrypoint in the chain to a valid exposing Loader when
  applicable.
- Any existing direct routed children already present in a newly routable folder.

The automatic effects use the complete [Index Interface Contract](../../index/interface.md)
projection, ordering, generated-boundary, verification, and recovery behavior.
They are part of the same parent plan, dry run, application, and result. The
command never starts a hidden `index` subprocess.

If existing children lack metadata needed for the intended generated region, the
complete initialization cannot finish before writes. When the child identities
and authority are safe but the required metadata coverage is unavailable, the
result is `incomplete`. An unsafe or ambiguous child boundary remains `blocked`.
The command does not omit existing children or invent fallback metadata to make
creation succeed.

## Planning And Effects

The operation follows the accepted typed mutation flow:

```text
validated route target and metadata
  -> current chain and compatibility facts
  -> complete intended entrypoint chain
  -> generated-navigation projection
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification or recovery
  -> one typed result
```

The complete plan includes new directories, new entrypoint files, and bounded
updates to existing generated regions. One blocked or incomplete target prevents
every effect. The command has no best-effort or partial-application mode.

Directory creation is limited to the intended route chain. The command does not
remove, rename, claim, or format existing user content. A newly created
directory may be removed during handled recovery only when this operation created
it and it is still empty.

## Dry Run And Apply

`--dry-run` and application use the same normalized request, route facts,
intended entrypoint state and scaffold bytes, generated projection, ordered plan,
expected-state facts, preflight, and semantic status conditions. Dry-run shows
every new directory, new entrypoint, generated-navigation effect, and exact
bounded existing-file diff required by that complete plan, then writes nothing.
It still reports the complete effects and diffs even when the result is
`attention`; planned changes alone do not produce `attention`.

Omitting `--dry-run` selects application. The explicit command and target confirm
creation of the missing route chain and replacement of only planned machine-owned
generated interiors. The command does not prompt and does not accept `--yes`.

A verified no-op has no affected mutation path and needs no Git cleanliness check.
An actual mutation checks only planned existing paths and collisions at planned
new paths. Dirty planned existing paths block by default.

`--skip-git-check` bypasses only relevant-path Git cleanliness. It does not bypass
route ambiguity, existing-target, metadata, containment, generated boundary,
expected-state, verification, or recovery requirements.

Gitless application and `--skip-git-check` application use adjacent backups for
planned replacements under the accepted recovery policy. New files do not
overwrite existing paths and need no old-byte backup. Recovery removes an
applied new file only when it still matches the operation's applied identity.

Immediately before application, the command rechecks the complete source,
destination, and collision facts. It applies complete planned bytes, verifies
each effect, rebuilds the route projection, and verifies the final chain and
generated navigation. A handled failure stops new effects and reverses applied
effects in reverse order without overwriting an unexpected concurrent change.

## Human Output

The default expanded view uses the complete blocks below. Compact view is a
projection of the same typed result. Every workspace-aware human result retains
`Workspace`, `Selected by`, and the target identity when it is available.
Compact output also retains application or preview mode, semantic status,
completeness and safety, created and unchanged paths, generated-navigation
effects, draft paths, and at most one required `Next:` line. Dry-run compact
output still shows every planned path, generated effect, and exact bounded diff.
Structured results retain at most one required `Next:` action as well.

### Verified No-Op

```text
Workspace: D:/work/example
Selected by: current directory
Target: memory/project-alpha/documents
Path: .agents/memory/project-alpha/documents/_documents.md
The route is initialized.
Checked 3 entrypoints. No files changed.
```

### Successful Application

```text
The route was initialized.
Created 2 entrypoints and updated 2 generated regions.
Workspace: D:/work/example
Selected by: current directory
Target: memory/project-alpha/documents
Path: .agents/memory/project-alpha/documents/_documents.md
Status: requires attention
Next: author each NeedsAuthoring entrypoint through route update before relying on its description or tags.
```

### Successful Dry Run

```text
The route would be initialized.
Would create 2 entrypoints and update 2 generated regions.

Workspace: D:/work/example
Selected by: --workspace
Target: memory/project-alpha/documents
Path: .agents/memory/project-alpha/documents/_documents.md

<new files and exact bounded diffs>

No files changed (--dry-run).
Status: requires attention
Next: author each NeedsAuthoring entrypoint through route update before relying on its description or tags.
```

Default human output lists every created entrypoint and changed existing path.
It states what happened without naming successful internal stages. Verbose and
structured output may include planning and preflight evidence.

Every error names the route initialization, target, direct cause, and direct
correction when one exists. Compact output has no required `Next:` line for a
`complete` result. An `attention` result has at most one required line:

```text
Next: author each NeedsAuthoring entrypoint through route update before relying on its description or tags.
```

For `incomplete`, `invalid`, and `blocked`, a required line names the direct
correction when it is known. Failed and interrupted results use ordinary retry
and recovery guidance. No result invents a health or semantic-quality score.

Primary human `complete`, `attention`, and `incomplete` results use stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. Each primary human result stays together on its assigned stream.
Separate bounded diagnostics use stderr.

## Structured Output

`--json` returns one complete structured result for every semantic status from
the same typed result used by human rendering. It never prompts and never reruns
planning, application, or verification. Bounded diagnostics use stderr. Human
rendering text is never mixed into JSON stdout.

The structured result exposes:

- Workspace and selection method.
- Requested target and resolved target ID and canonical path.
- Application or dry-run mode, completeness, and safety.
- Existing, compatibility, missing, and created entrypoints in chain order.
- Draft and explicit metadata provenance for every created entrypoint.
- Draft entrypoint paths.
- Planned directories, files, and generated-region effects.
- Dry-run, Git, backup, application, verification, and recovery facts.
- Changed, unchanged, reverted, and residual targets.
- Bounded observations, availability conditions, attention conditions, semantic
  status, and at most one required `Next:` action.

Exact field names, schema versioning, and compatibility rules are defined by the
CLI Architecture.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                | Process completion status            |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------ |
| `complete`    | Dry-run established the complete safe plan without a finite attention condition, or application and final verification completed without one, including a verified no-op.                              | Architecture-defined process status. |
| `attention`   | A safe complete dry-run preview or completed and verified application includes at least one new entrypoint whose intended tags contain exact `NeedsAuthoring`; human output says `requires attention`. | Architecture-defined process status. |
| `incomplete`  | Safe current facts are available, but required inspection or planning coverage cannot complete; no write begins.                                                                                       | Architecture-defined process status. |
| `invalid`     | Command input, metadata, flag use, or target shape does not follow this interface; invalid input stops before operation resolution.                                                                    | Architecture-defined process status. |
| `blocked`     | Unsafe or ambiguous authority or safety prevents one safe complete route plan; no mutation begins.                                                                                                     | Architecture-defined process status. |
| `failed`      | An unexpected application, post-write, verification, or recovery failure prevents normal completion.                                                                                                   | Architecture-defined process status. |
| `interrupted` | The caller cancelled or interrupted before completion and no residual recovery failure remains.                                                                                                        | Architecture-defined process status. |

The shared process-status mapping is defined by the CLI Architecture.

For ordinary operation conditions, status precedence is `blocked` > `incomplete`

> `attention` > `complete`. Invalid input stops before operation resolution and
> forms `invalid`. Failed and interrupted results retain their event meaning.

## Scenarios

The representative folder-ID target and its canonical path are:

```text
open-forge route init memory/project-alpha/documents
```

```text
.agents/memory/project-alpha/documents/_documents.md
```

The exact human result examples for verified no-op, successful application, and
successful dry run are under [Human Output](#human-output). They are the public
result examples and are not replaced by internal stage names.

## Errors

The command blocks or rejects:

- A missing-target path that is not canonical.
- Repeated `--description` or `--responsibility`, including equal repeated
  values.
- A Loader target or request to create a Loader.
- Several recognized entrypoints in one folder.
- An ordinary-file or physical-identity route collision.
- Unsafe containment or an existing non-entrypoint at a planned target path.
- Metadata that cannot produce canonical required fields.
- An invalid generated ownership boundary in a planned existing parent.
- A dirty planned existing path without the accepted Git bypass.
- A changed source or destination that invalidates the plan.

When safe facts are available but required inspection or planning coverage cannot
complete, the command returns `incomplete` without beginning a write. Unsafe or
ambiguous authority, identity, containment, route, generated ownership, or other
safety facts remain `blocked` rather than `incomplete`.

## Non-Goals

`route init` does not:

- Create an ordinary routed Markdown file.
- Instantiate or update from a Template.
- Update authored content in an existing entrypoint.
- Rename compatibility filenames to canonical filenames.
- Invent a route's purpose, loading behavior, scope, authority, or useful tags.
- Repair malformed existing entrypoints or generated markers.
- Create, install, or repair the Framework Loader.
- Create a Git commit.

Use `route create` for one ordinary routed Markdown file and `route update` for
an existing routed source.

## Verification

Implementation evidence must cover the public boundaries and observable results
of this Interface Contract:

- ID and exact canonical-path targets, spaces, Unicode, and unsafe segments.
- The exact required-target command shape, with no wizard, `--automatic`, alias,
  or additional operation-specific flag.
- Singleton rejection for repeated `--description` and `--responsibility`,
  including equal values; ordered repeated `--tag` values with exact duplicate,
  empty, and syntax validation; idempotent repetition of both Boolean
  write-policy flags; and shared global-flag repetition rules.
- One missing target, several missing ancestors, and complete no-op chains.
- Detached chains, valid Loader exposure, and proof that the Loader is never
  created.
- Existing canonical and each compatibility entrypoint name.
- Multiple-entrypoint, ordinary-file ID, portable path, and physical identity
  collisions.
- Exact fixed scaffold bytes, literal visible slug titles, draft descriptions,
  `NeedsAuthoring`, inherited Axioms, and valid generated regions.
- Final description, responsibility addition and omission, tag replacement,
  partial metadata, and invalid field values.
- Existing children in a newly routable folder and missing child metadata.
- Automatic generated effects against intended topology.
- Dry-run and application parity for request, facts, intended state, generated
  projection, plan, preflight, and semantic status conditions; complete effects
  and exact diffs in dry-run with no persistent effects.
- Finite `attention` formation for new draft ancestors, a draft final target,
  automatically supplied or explicitly retained `NeedsAuthoring`, and no
  attention from planned changes alone or unchanged existing `NeedsAuthoring`.
- Complete results when every new entrypoint has complete intended metadata and
  no exact `NeedsAuthoring` marker.
- Verified no-op behavior before Git mutation checks.
- Seven semantic results, including safe `incomplete` with no write, blocked
  unsafe or ambiguous safety, and failed post-write/application/verification/
  recovery failures.
- Compact retention of workspace and target identity, application or preview,
  status, completeness, safety, created and unchanged paths, generated effects,
  draft paths, and at most one required `Next:` line.
- Human stream assignment, one JSON result on stdout for every status, bounded
  diagnostics on stderr, and no human text in JSON stdout, all from one typed
  result.

The [Behavior Contract](behavior.md) records the required technology-neutral
evidence for planning, projection, effects, Git and backup boundaries,
revalidation, verification, recovery, concurrency, and convergence. Exact
schemas and JSON compatibility, numeric exits, parser and serialization,
filesystem and identity implementation, backup names, concurrency mechanics,
and source boundaries are defined by the CLI Architecture. Those accepted
technical choices do not weaken the accepted repetition, status, stream,
attention, dry-run, compact, or verification rules above.

## Related Current Sources

- [route init Command Contract Set](_init.md)
- [Route Init Behavior Contract](behavior.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [Index Interface Contract](../../index/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Route Create Interface Contract](../create/interface.md)
- [Route Update Interface Contract](../update/interface.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Loading And Continuity](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Templates](../../../../framework/primitives/templates.md)
