---
open-forge:
  description: Accepted current public interface and observable result for recursively initializing missing route entrypoints
  responsibility: Define what a caller may enter and observe from `route init`
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Init, Entrypoint, Interface, CurrentTruth]
---

# route init Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route init`. The command does not ship yet. Its local
implementation and complete executable proof are squash-integrated at
`cc5085ce`; replacement-CLI delivery remains pending.

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
The [Index Interface Contract](../../index-candidate/interface.md) defines the generated
navigation projection, ordering, generated boundary, verification, and recovery
behavior consumed by this command.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the shared structured envelope, compatibility policy, and process-status
mapping. The [CLI Architecture](../../../architecture.md) defines source and
runtime boundaries, BCL-first filesystem structure, the workspace-lock boundary,
and recovery identity relationships. This Interface Contract owns the Route Init result
graph inside that envelope, without changing those shared details. The
command-specific repetition, seven-status, stream, finite-attention, and
compact-result rules below are accepted current behavior.

## Purpose

`route init` makes one target folder routable by creating every missing
entrypoint in its route chain. Generic mode uses one fixed draft entrypoint
scaffold. Framework mode reuses embedded canonical Framework entrypoints while
creating user-owned draft entrypoints for inserted scopes. Neither mode
instantiates a Template, creates the Loader, or infers semantic meaning from a
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
  [--framework]
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--dry-run]
  [global flags]
```

In generic mode, `--description`, `--responsibility`, and `--tag` provide
authored metadata for the final target only. `--framework` selects the embedded
Framework topology and asset mode. `--dry-run` is the write-policy preview.
The command has no `--template`, `--scope`, `--scaffold-from`, `--yes`, `--force`,
`--no-responsibility`, Loader-creation mode, or alias.

The `route` group performs no domain operation by itself. It shows help for its
accepted child operations. The [route group entrypoint](../_route.md)
routes that group relationship, while this leaf's help comes from the complete
public Interface surface under the shared global help rules. The group
description is not a second command contract. Group help does not resolve a
workspace or run a domain operation.

`route init` is an explicit, non-wizard leaf. It has no wizard mode,
`--automatic` mode, alias, or inferred current-scope mode.

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

In generic mode, an ID is the intended folder ID under `.agents`. In Framework
mode, the ID-form operand is the desired concrete route chain: exact
case-sensitive canonical Framework segments identify managed topology, while
inserted segments are scope labels converted to deterministic concrete slugs.
The representative invocations and canonical path mappings appear under
[Scenarios](#scenarios).

An exact path for a missing target must name its canonical entrypoint file. A
directory path, ordinary Markdown filename, Loader path, overwrite path,
`SKILL.md`, or compatibility filename is not a missing-target form.

The compatibility path forms are valid only when that exact file already exists
and the operation needs to initialize missing ancestors. The command preserves
that filename. It never creates a new compatibility filename.

The shared [CLI Source References](../../shared/source-references/interface.md) contract defines
ID segments, exact `.agents/...` path detection, quoting, containment, and result
identity. `route init` adds the deterministic generic missing-target mapping and
the Framework alignment and scope-label policy below. It does not guess another
target shape from filesystem coincidence.

The target is invalid when it is empty, identifies `loader`, contains `.` or `..`
as an ID segment, cannot be represented as a safe contained entrypoint path, or
uses an exact missing path that does not match the final folder's canonical
filename.

## Flags

The six global flags apply under the shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract. This command does not copy their complete definitions.

| Flag                      | Role              | Value                                                          | Omission                                                                                         | Repetition, ordering, and composition                                                                                           |
| ------------------------- | ----------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `--framework`             | Scaffold mode     | No value                                                       | Generic exact-chain initialization is selected                                                   | Repetition is accepted and idempotent. It selects embedded canonical Framework topology and assets.                             |
| `--description <text>`    | Authored metadata | One description value                                          | The final target uses its draft description unless another rule supplies an explicit description | Singleton. Repetition is invalid, including repetition with an equal value.                                                     |
| `--responsibility <text>` | Authored metadata | One responsibility value, including the exact empty value `""` | No responsibility field is added to a missing target                                             | A non-empty value adds the field and `""` omits it. The flag is singleton; any repetition is invalid, including an equal value. |
| `--tag=<tag>`             | Authored metadata | One tag without a `#` prefix                                   | The final target uses draft metadata and the `NeedsAuthoring` rule                               | Repeatable. Values retain argument order. Empty tags and duplicate exact tags are invalid.                                      |
| `--dry-run`               | Write policy      | No value                                                       | Application is selected                                                                          | Repetition is accepted and idempotent. It previews the same complete plan and preflight.                                        |

`--description`, `--responsibility`, and `--tag` are valid only in generic mode
as metadata for a missing final target. They are invalid with `--framework`
because caller metadata cannot rewrite embedded managed content. Repeating
`--description` or `--responsibility` is invalid,
even when the repeated values are equal. Repeated `--tag` values form one
ordered list; there is no last-wins or other precedence rule. Repeated
`--framework` and `--dry-run` occurrences collapse to their idempotent Boolean
choices and do not grant another operation or authority. Global flags
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

## Framework Mode

`--framework` initializes one sparse scoped chain from the same canonical
Framework payload embedded for root Install. Root Install remains the producer
of the trusted base Framework state; there is no `install --route` spelling.

For either operand form, the first concrete route segment must be an exact
installed root route. Exact case-sensitive non-root Framework segments align
against the embedded canonical topology, in canonical order. Inserted segments
occupy scope positions. Exactly one alignment is required, and the final segment
must be a canonical non-root Framework route. A trailing user-only scope belongs
to generic Route Init.

For an ID-form target, each inserted segment is a scope label and becomes one
concrete slug by this local rule:

1. Iterate Unicode runes and lowercase letters invariantly.
2. Preserve digits.
3. Collapse a run of whitespace, ASCII `_`, or ASCII `-` to one `-`.
4. Trim the resulting separator.
5. Reject every other punctuation, control character, and path separator, plus
   empty output, `.` and `..`.
6. Apply the ordinary portable collision, physical-identity, and containment
   checks to the resulting concrete path.

An exact `.agents/...` target path is already concrete and is never slugged, but
its concrete folder segments must satisfy the same unique canonical-topology
alignment. Ambiguous alignment, reordered managed segments, nested root
recreation, source-inventory mismatch, or a post-slug identity collision blocks
before any write. A trusted current root Install lifecycle matching the running
CLI's embedded inventory is required; otherwise the result directs the caller
to `open-forge install` or `open-forge update`.

The trusted lifecycle path is exactly `.agents/open-forge.lifecycle.json`. Its
schema-v1 root contains every standard key in canonical order:
`schemaVersion`, `fingerprintPolicy`, `workspacePath`, `framework`, and
`extensions`. The empty Extension state is the complete value
`{ coverage: "complete", packages: [], paths: [] }`; it is never `null` or
omitted. Route Init does not create the lifecycle document. A missing, `null`,
malformed, unsupported, or incomplete standard section blocks or leaves the
required fact unavailable without a write; explicit Update or Doctor work owns
repair.

The plan creates only the requested sparse chain. It copies exact embedded
canonical entrypoint bytes for aligned missing Framework segments, then projects
their destination-local generated `Entries`. It creates the existing generic
draft scaffold for missing inserted scope segments. Scope entrypoints remain
user-owned; copied Framework entrypoints and derived generated regions are the
only new Framework lifecycle targets. Each copied target records its canonical
embedded `sourceAssetPath`; a derived generated-region target records `null`.

A verified repeat is a no-op. Creating any draft scope retains the existing
finite `NeedsAuthoring` attention condition. Framework mode adds no blueprint,
Template selection, general scaffold engine, `--scope` placeholder language, or
caller metadata override.

## Generic Fixed Entrypoint Scaffold

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

1. Canonical scoped frontmatter with exactly one Open Forge metadata root key,
   `open-forge`, containing `description`, `tags`, and optional
   `responsibility` only.
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

Open Forge metadata recognizes only the `open-forge` root. A `rune` root or any
other YAML is unrelated opaque content: it supplies no Open Forge description,
tags, or responsibility and is preserved by bounded source edits. When both
`open-forge` and `rune` occur, Route Init reads exactly `open-forge`, ignores the
meaning of `rune`, and preserves the unrelated YAML bytes. Route Init never
authors `rune` or another metadata root.

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
before any persistent effect begins. Each new generic or scope entrypoint
initially has a valid empty generated region. Each copied Framework entrypoint
uses its embedded authored bytes while its generated interior is derived for the
concrete destination. Automatic generated-navigation effects then add:

- Every new direct child entrypoint to its intended parent entrypoint.
- The first new entrypoint in the chain to a valid exposing Loader when
  applicable.
- Any existing direct routed children already present in a newly routable folder.

The automatic effects use the complete [Index Interface Contract](../../index-candidate/interface.md)
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
validated route target, mode, and applicable metadata
  -> current chain and compatibility facts
  -> complete intended entrypoint chain
  -> generated-navigation projection
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The complete plan includes new directories, new entrypoint files, and bounded
updates to existing generated regions. One blocked or incomplete target prevents
every effect. The command has no best-effort or partial-application mode.

Directory creation is a separate effect from file Create/Replace/Delete and is
limited to the intended route chain. In generic mode, when a fully preflighted
plan starts without `.agents`, that exact container is the one visible planned
and reported ordinary directory-create effect after the external workspace
lease is acquired. The shared applier confirms it is missing, creates and
verifies it, and leaves it as reported residual state on later failure or
interruption. Cancellation or lock contention before acquisition creates
nothing. Framework mode requires a trusted Install and therefore cannot start
from an absent `.agents` directory.

While holding the workspace lease, the command applies every explicitly
planned missing directory parent-first through the shared capability. Each is a
descendant below `.agents`: immediately revalidate the missing target and its
exact contained physical parent, call ordinary `Directory.CreateDirectory`, then
verify the resulting contained ordinary directory. It does not remove, rename,
claim, or format existing user content. A directory has no recovery entry and is
never rolled back, compensated for, or removed.

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

A verified no-op has no affected mutation path and needs no recovery bundle. An
actual mutation checks planned existing paths and collisions at planned new
paths. The command does not inspect or report repository state.

When the plan contains an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), orchestration selects only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a pre-effect
`incomplete` result. It prepares exactly one immutable ZIP bundle outside
the workspace. An operation containing only
Create effects or no-ops creates no bundle. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
Directory-create effects, file Create effects for new entrypoints, and no-ops
have no recovery entry. A
CreateNew draft is closed and reopened for semantic manifest, exact ordered
entry, length, hash, and payload-byte validation, moved within the same
directory to its deterministic final name, and reopened and verified. Only the
valid final ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. `FileChangeApplier` requires that preparation for every
existing-target effect and performs one final effect per target.
All preparation completes before the first target effect; unknown, malformed,
mismatched, or colliding bundles block.

Immediately before application, the command rechecks the complete source,
destination, and collision facts. It applies complete planned bytes, verifies
each effect, rebuilds the route projection, and verifies the final chain and
generated navigation. A handled failure stops new effects and never restores,
rolls back, or compensates for an earlier effect. An unexpected concurrent
change is preserved and reported as residual state.

After final verification, delete only the positively recognized bundle created
by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery attention
coexists with a new-entrypoint `NeedsAuthoring` condition, cleanup guidance owns
the single next action; the
`NeedsAuthoring` facts remain visible evidence. Before post-verification deletion
begins, a handled application, verification, or cancellation outcome reports the
actual residual draft or final path; a valid final remains when preparation
completed. A closed final ZIP may remain after abrupt process termination,
without an executable crash or power-loss guarantee.
Recovery provenance does not classify current target state. Cleanup owns exact
named final and draft deletion under its separate lease-bound contract. The
persistent reusable zero-byte external workspace lock below
`LocalApplicationData/OpenForge/locks/v1` is held with one read/write
`FileShare.None` handle; it never receives metadata writes, deletion, or
truncation.

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

If `Failed`/positively observed `Retained` recovery also applies, its exact
cleanup guidance owns the single `Next:` line and the `NeedsAuthoring` condition
remains visible as evidence.

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

The shared schema-v1 envelope remains exactly
`{ schemaVersion, command, status, workspace, result, next }`. The Route Init
`result` object is fully present in this property order:

```text
result {
  mode
  scaffold
  target { requested id path }
  plan { completeness safety }
  framework {
    inventoryFingerprint
    segments[] { path role sourceAssetPath }
  } | null
  entrypoints[] {
    id
    path
    form
    current
    ownership
    metadata {
      description
      descriptionSource
      responsibility
      responsibilitySource
      tags[]
      tagsSource
    } | null
    sourceAssetPath
    outcome
  }
  effects[] {
    path
    kind
    action
    sourceAssetPath
    change { before expected } | null
    outcome
    residual
  }
  unchangedPaths[]
  lifecycle { action outcome }
  recovery { state residualPath }
  verification
  findings[] { code status target cause }
}
```

`status` and `next` are derived once from the ordered findings and are not
duplicated inside `result`. Collections are immutable and never `null`.
Entrypoints and Framework segments retain first-to-final chain order; effects
retain execution order; tags retain argument order; unchanged paths are unique
and ordinally ordered. Findings use the fixed code order below, then nullable
target and cause in ordinal order.

The target intentionally exposes only `requested`, resolved `id`, and resolved
canonical `path`. It has no redundant operand-form or `selectedBy` provenance.
Nullable members are limited to unresolved target coordinates, `framework`,
existing-entrypoint `metadata`, metadata `responsibility`, `sourceAssetPath`,
directory `change`, create-change `before`, recovery `residualPath`, finding
`target`, envelope `workspace`, and envelope `next`.

Lifecycle evidence is bounded to Route Init-owned facts. `effects` contains only
`directory`, `entrypoint`, and `generated-region` effects. `lifecycle` reports
only the command's action and outcome; neither the result nor a dry-run change
exposes the whole lifecycle document, preserved Extension state, root Install
targets, or other unrelated lifecycle bytes. An entrypoint create change
contains that new entrypoint's complete UTF-8 text. A generated-region change
contains only its bounded interior. Directory changes are `null`.

The finite machine values are:

| Coordinate               | Values                                                                                                                |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------- |
| `mode`                   | `apply`, `dry-run`                                                                                                    |
| `scaffold`               | `generic`, `framework`                                                                                                |
| `plan.completeness`      | `not-established`, `incomplete`, `complete`                                                                           |
| `plan.safety`            | `not-established`, `safe`, `blocked`                                                                                  |
| Framework segment `role` | `installed-root`, `managed`, `scope`                                                                                  |
| Entrypoint `form`        | `canonical`, `compatibility`                                                                                          |
| Entrypoint `current`     | `existing`, `missing`                                                                                                 |
| Entrypoint `ownership`   | `user`, `framework`                                                                                                   |
| `descriptionSource`      | `draft`, `explicit`, `embedded`                                                                                       |
| `responsibilitySource`   | `default-omitted`, `explicit-omitted`, `explicit`, `embedded`                                                         |
| `tagsSource`             | `draft`, `explicit`, `mixed`, `embedded`                                                                              |
| Entrypoint `outcome`     | `unchanged`, `planned`, `not-started`, `created`, `verification-failed`, `completion-unknown`                         |
| Effect `kind`            | `directory`, `entrypoint`, `generated-region`                                                                         |
| Effect `action`          | `create`, `replace`                                                                                                   |
| Effect `outcome`         | `planned`, `not-started`, `verified`, `verification-failed`, `completion-unknown`                                     |
| Effect `residual`        | `none`, `retained`, `unknown`                                                                                         |
| Lifecycle `action`       | `none`, `preserve`, `publish`                                                                                         |
| Lifecycle `outcome`      | `not-requested`, `planned`, `already-current`, `not-started`, `verified`, `verification-failed`, `completion-unknown` |
| Recovery `state`         | `not-required`, `not-created`, `removed`, `retained`, `unknown`                                                       |
| `verification`           | `not-requested`, `verified`, `failed`, `unknown`                                                                      |

Every finding is exactly `{ code, status, target, cause }`. `complete` has no
finding. Within each status, finding codes use this exact order:

| Status        | Finding codes in order                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `invalid`     | `route-init.invalid-input`, `route-init.invalid-target`, `route-init.invalid-metadata`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| `blocked`     | `route-init.workspace-unavailable`, `route-init.workspace-unsafe`, `route-init.target-unsafe`, `route-init.route-ambiguous`, `route-init.identity-collision`, `route-init.loader-unsafe`, `route-init.framework-payload-invalid`, `route-init.framework-install-required`, `route-init.framework-update-required`, `route-init.framework-alignment-blocked`, `route-init.metadata-unsafe`, `route-init.generated-region-unsafe`, `route-init.lifecycle-blocked`, `route-init.workspace-lock-unavailable`, `route-init.target-changed`, `route-init.recovery-conflict` |
| `incomplete`  | `route-init.framework-payload-unavailable`, `route-init.inspection-incomplete`, `route-init.metadata-incomplete`, `route-init.projection-incomplete`, `route-init.lifecycle-unavailable`, `route-init.recovery-unavailable`                                                                                                                                                                                                                                                                                                                                           |
| `attention`   | `route-init.needs-authoring`, `route-init.recovery-artifact-retained`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| `failed`      | `route-init.target-changed-during-apply`, `route-init.write-failed`, `route-init.verification-failed`, `route-init.lifecycle-publication-failed`, `route-init.recovery-failed`, `route-init.operation-failed`                                                                                                                                                                                                                                                                                                                                                         |
| `interrupted` | `route-init.interrupted`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |

Aggregate precedence is `failed`, `interrupted`, `invalid`, `blocked`,
`incomplete`, `attention`, then `complete`.

The one structured `next` action uses this exact first-applicable policy:

| Condition                                    | `next.command`                    | `next.reason`                                                                                                                    |
| -------------------------------------------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `complete`                                   | `null`                            | `null`                                                                                                                           |
| `invalid`                                    | `open-forge route init --help`    | `Correct the named Route Init input, then rerun the request.`                                                                    |
| `route-init.framework-install-required`      | `open-forge install`              | `Establish a trusted current Framework installation before rerunning Route Init in Framework mode.`                              |
| `route-init.framework-update-required`       | `open-forge update`               | `Update the installed Framework state to the running CLI's embedded inventory before rerunning Route Init.`                      |
| Workspace lock unavailable or target changed | `open-forge route init`           | `Wait for the blocking condition or inspect the changed target, then rerun Route Init from a fresh plan.`                        |
| Other `blocked`                              | `open-forge doctor`               | `Inspect the blocked workspace, route, identity, lifecycle, generated-region, or recovery boundary before rerunning Route Init.` |
| `incomplete`                                 | `open-forge doctor`               | `Inspect the unavailable route, metadata, projection, lifecycle, or recovery facts before relying on this Route Init result.`    |
| Retained recovery artifact                   | `open-forge cleanup`              | `Review and remove the reported recovery artifact after confirming the verified Route Init result.`                              |
| NeedsAuthoring attention                     | `open-forge route update`         | `Author each reported NeedsAuthoring entrypoint before relying on its description or tags.`                                      |
| `failed`                                     | `open-forge route init --verbose` | `Report the failure and retry the same Route Init request with bounded diagnostics.`                                             |
| `interrupted`                                | `open-forge route init`           | `Rerun the same Route Init request.`                                                                                             |

Recovery cleanup wins when both attention findings coexist. Exact schema
versioning and compatibility rules remain defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md).

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                         | Process completion status                |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| `complete`    | Dry-run established the complete safe plan without a finite attention condition, or application and final verification completed without one, including a verified no-op.                                                                                                                                       | Shared result-coordinate process status. |
| `attention`   | A safe complete dry-run preview or completed and verified application includes at least one new entrypoint whose intended tags contain exact `NeedsAuthoring`, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`; human output says `requires attention`. | Shared result-coordinate process status. |
| `incomplete`  | Safe current facts are available, but required inspection or planning coverage cannot complete; no write begins.                                                                                                                                                                                                | Shared result-coordinate process status. |
| `invalid`     | Command input, metadata, flag use, or target shape does not follow this interface; invalid input stops before operation resolution.                                                                                                                                                                             | Shared result-coordinate process status. |
| `blocked`     | Unsafe or ambiguous authority or safety prevents one safe complete route plan; no mutation begins.                                                                                                                                                                                                              | Shared result-coordinate process status. |
| `failed`      | An unexpected application, post-write, or verification failure, or post-verification recovery deletion `Failed`/`Unknown`, prevents normal completion.                                                                                                                                                          | Shared result-coordinate process status. |
| `interrupted` | The caller cancelled or interrupted before completion and no unexpected application or verification failure changes the result.                                                                                                                                                                                 | Shared result-coordinate process status. |

The shared process-status mapping is defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md).

Aggregate status precedence is `failed`, `interrupted`, `invalid`, `blocked`,
`incomplete`, `attention`, then `complete`. Invalid input still stops before
operation resolution; the complete precedence governs one formed result when
several retained conditions coexist.

## Scenarios

The representative folder-ID target and its canonical path are:

```text
open-forge route init memory/project-alpha/documents
```

```text
.agents/memory/project-alpha/documents/_documents.md
```

The representative Framework target:

```text
open-forge route init "memory/Mobile App/crystallized/documents" --framework
```

resolves to the concrete route `memory/mobile-app/crystallized/documents` and
creates only that missing sparse chain. `mobile-app` is a user-owned scope;
`crystallized` and `documents` retain their canonical managed segment meaning.

The exact human result examples for verified no-op, successful application, and
successful dry run are under [Human Output](#human-output). They are the public
result examples and are not replaced by internal stage names.

## Errors

The command blocks or rejects:

- A missing-target path that is not canonical.
- Repeated `--description` or `--responsibility`, including equal repeated
  values.
- Metadata flags combined with `--framework`.
- Missing, untrusted, or source-outdated root Framework lifecycle in Framework
  mode.
- Ambiguous Framework alignment, managed-segment reordering, root recreation,
  invalid scope labels, or post-slug identity collision.
- A Loader target or request to create a Loader.
- Several recognized entrypoints in one folder.
- An ordinary-file or physical-identity route collision.
- Unsafe containment or an existing non-entrypoint at a planned target path.
- Metadata that cannot produce canonical required fields.
- An invalid generated ownership boundary in a planned existing parent.
- Unavailable or unsafe recovery-bundle storage is `incomplete`; an unverified
  bundle is `blocked`.
- A changed source or destination that invalidates the plan.

When safe facts are available but required inspection or planning coverage cannot
complete, the command returns `incomplete` without beginning a write. Unsafe or
ambiguous authority, identity, containment, route, generated ownership, or other
safety facts remain `blocked` rather than `incomplete`.

## Non-Goals

`route init` does not:

- Create an ordinary routed Markdown file.
- Instantiate or update from a Template.
- Discover a blueprint, render a general scaffold, or accept a `--scope`
  placeholder grammar.
- Update authored content in an existing entrypoint.
- Rename compatibility filenames to canonical filenames.
- Invent a route's purpose, loading behavior, authority, or useful tags. In
  Framework mode, inserted path segments express caller-supplied scope placement;
  the command does not infer that placement from workspace content.
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
  or inferred current-scope mode.
- Generic mode and Framework mode selection, including invalid metadata
  combinations and idempotent repeated `--framework`.
- Framework routes with zero, one, multiple, and consecutive inserted scopes;
  scope positions before and between managed segments; and sparse-chain-only
  effects.
- Scope-label casing, digits, Unicode letters, separator collapse, rejected
  punctuation and separators, empty output, exact-path non-slugging, and
  post-conversion collisions.
- Unique and ambiguous canonical alignment, managed-segment reordering, nested
  root recreation, trailing user scope, and missing, untrusted, or outdated root
  Install lifecycle.
- Canonical complete lifecycle root keys, complete empty Extensions, physical
  currentness, and rejection without writes for missing, `null`, malformed,
  unsupported, or incomplete standard sections.
- Exact embedded bytes for managed entrypoints, draft bytes for user-owned scope
  entrypoints, destination-local generated navigation, `sourceAssetPath`
  publication, and the exclusion of scope files from Framework ownership.
- Singleton rejection for repeated `--description` and `--responsibility`,
  including equal values; ordered repeated `--tag` values with exact duplicate,
  empty, and syntax validation; idempotent repetition of the Boolean
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
- Canonical `open-forge` authoring and reading only, opaque `rune` preservation,
  and `open-forge` selection when unrelated sibling YAML is present.
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
- Verified no-op behavior before recovery-bundle preparation.
- Separate parent-first directory effects under the held workspace lease, with
  immediate missing-target and physical-parent revalidation, ordinary BCL
  creation, post-verification, retained residuals, and no rollback,
  compensation, removal, or recovery entry.
- Generic-mode missing-`.agents` planning/reporting as the first ordinary
  lease-bound directory effect, cancellation/contention before workspace
  effects, later residual behavior, and proof that Framework mode requires
  existing trusted Install state instead.
- Seven semantic results, including safe `incomplete` with no write, blocked
  unsafe or ambiguous safety, `Failed`/positively observed `Retained` recovery
  `attention`, and failed post-write, application, verification, or
  `Failed`/`Unknown` recovery outcomes.
- Compact retention of workspace and target identity, application or preview,
  status, completeness, safety, created and unchanged paths, generated effects,
  draft paths, and at most one required `Next:` line.
- Human stream assignment, one JSON result on stdout for every status, bounded
  diagnostics on stderr, and no human text in JSON stdout, all from one typed
  result.

The [Behavior Contract](behavior.md) records the required technology-neutral
evidence for planning, projection, effects, recovery-bundle boundaries,
revalidation, verification, recovery, concurrency, and convergence. This
Interface owns the exact command-local schema while shared JSON compatibility and
numeric exits remain defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md). Parser and concrete
serialization, filesystem identity, concurrency, and source boundaries remain
defined by the [CLI Architecture](../../../architecture.md); exact recovery-bundle
names follow the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md). Those accepted technical choices do not weaken
the repetition, status, stream, attention, dry-run, compact, or verification
rules above.

## Related Current Sources

- [route init Command Contract Set](_init.md)
- [Route Init Behavior Contract](behavior.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [Index Interface Contract](../../index-candidate/interface.md)
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
