---
open-forge:
  description: Accepted current public interface and observable result for recursively initializing missing route entrypoints
  responsibility: Define what a caller may enter and observe from `route init`
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Init, Entrypoint, Interface, CurrentTruth]
---

# route init Interface Contract

## Ownership Receipt Boundary

The generated `.agents/open-forge.lock.json` distinguishes whole-file paths
from regions. Root `AGENTS.md` and `CLAUDE.md` hosts carry `open-forge` region
receipts for their existing managed blocks; the host files are not whole-file
ownership. Generated Entries use `entries` region receipts. Publication follows
verified operation effects, retains unaffected verified ownership, and does not
convert a region-only edit into ownership of its authored host. A missing or
unwritable lock does not authorize wider ownership or block the operation.

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route init`. The command is implemented in the merged CLI. Its local
implementation and complete executable proof are squash-integrated at
`cc5085ce`; the merged native CLI is the current delivery.

The current [Routed Markdown Representation](../../../../framework/markdown/routes.md)
defines canonical entrypoint syntax. The [Routing Model](../../../../framework/routing/model.md)
defines why every visible folder in a route chain needs one entrypoint. The
[Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
defines which existing compatibility filenames this command recognizes and
preserves.

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines the
complete spelling, values, defaults, repetition, terminal behavior, and errors
for `--workspace <path>`, `--format <text|json>`, `--detail <minimal|standard|full|debug>`, repeatable `--detail-filter <error|warning|info|all>`, `--help`, and `--version`.
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
and recovery identity relationships. This Interface Contract owns the Route Init
data inside that envelope, without changing those shared details. The
command-specific repetition, seven-status, stream, placeholder-metadata, and
native-report rules below are accepted current behavior.

## Purpose

`route init` makes one target folder routable by creating every missing
entrypoint in its route chain. Generic mode uses one fixed draft entrypoint
scaffold. Framework mode reuses embedded canonical Framework entrypoints while
creating user-owned draft entrypoints for inserted scopes. Neither mode
instantiates a Template, creates the Loader, or infers semantic meaning from a
folder name.

Given the same workspace bytes and explicit input, the command selects the same
missing entrypoints, produces the same intended files and generated navigation,
and returns the same semantic result. After successful application, invoking the
same target without creation metadata returns a verified no-op. Metadata flags
apply only while the final target entrypoint is missing; they are invalid once
it exists.

The technology-neutral mechanics behind this public promise are defined in the
[Behavior Contract](behavior.md). This file owns the complete caller-visible
meaning.

## Syntax

Tag values accept `--tag Memory`, `--tag=Memory`, and `--tag:Memory` with
identical meaning. Repeated values retain their existing order and validation.

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
of the base Framework files and route structure; there is no `install --route` spelling.

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
recreation, or a post-slug identity collision blocks before any write. The
actual installed root route and selected source/generated-region boundaries
must be safe and readable. Stored release metadata and baseline hashes do not
gate the operation.

The sole state file is `.agents/open-forge.lock.json`. Read it forgivingly as
whole-file and region ownership, preserving unselected Framework receipts and
other sections. Missing, malformed, null, unknown, or unreadable lock data does
not block safe creation or authorize adoption of existing files. Earlier records
remain untouched and are never read or migrated. Publish the lock after target
effects verify, or skip an unavailable write without claiming publication.
An identical receipt is preserved and a no-op invents no new ownership.

The plan creates only the requested sparse chain. It copies exact embedded
canonical entrypoint bytes for aligned missing Framework segments, then projects
their destination-local generated `Entries`. It creates the existing generic
draft scaffold for missing inserted scope segments. Scope entrypoints remain
user-owned; copied Framework entrypoints and derived generated regions are the
only new Framework ownership receipts. Current output retains canonical embedded
`sourceAssetPath` for copied entrypoints; the lock stores only paths and named
regions. Existing user-authored hosts acquire no whole-file claim from a region
rewrite.

A verified repeat is a no-op. Creating any draft scope retains the informational
`NeedsAuthoring` advisory while remaining `completed` at exit 0. Framework mode adds no blueprint,
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

- none - No entries - #Empty
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
6. One final `Entries` section containing its generated body without guards.

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
  frontmatter and the minimal-detail body definition.
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

### Placeholder metadata condition

After a safe complete plan and preflight have been established, a newly created
entrypoint whose intended tags contain the exact `NeedsAuthoring` tag remains
`completed` with exit 0. The `route-init.needs-authoring` finding is `info`,
contributes nothing to status or exit code, and renders as the advisory sentence
`Its description and tags are placeholders. Edit them before relying on this route.`
rather than an Info row. JSON sets `needsAuthoring: true` for the affected
entrypoint.

An unchanged existing entrypoint that already contains `NeedsAuthoring` does not
alter a verified no-op. Planned changes and placeholder metadata do not produce
`completed-with-warnings`. This condition does not score route health or semantic
quality.

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
nothing. Framework mode requires an actual safe installed root and therefore cannot start
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
`completed-with-warnings`; planned changes alone do not produce `completed-with-warnings`.

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
versioned `manifest.json` and streamed ordinal payload entries record
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
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery warning
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

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command-specific context, full
adds all bounded facts, and debug adds bounded diagnostics on stderr. Detail
does not change semantics, counts, or status. Filters select finding severities;
all is the default filter.

Route Init is non-interactive; it has no final confirmation and no `--automatic`
flag.

The catalogue text by detail level is:

`minimal`, one new entrypoint:

```text
Created .agents/memory/emerging/ideas/pricing/_pricing.md
  Listed in .agents/memory/emerging/ideas/_ideas.md
  Its description and tags are placeholders. Edit them before relying on this route.
```

`minimal`, a chain of two with explicit metadata:

```text
Created 2 entrypoints for memory/projects/alpha.
  .agents/memory/projects/_projects.md
  .agents/memory/projects/alpha/_alpha.md
  Listed in .agents/memory/_memory.md
```

`minimal`, dry run:

```text
Would create .agents/memory/emerging/ideas/pricing/_pricing.md
  Would list it in .agents/memory/emerging/ideas/_ideas.md
No files were changed.
```

`standard` adds `Workspace:`, every entrypoint in the chain with `created` or
`already present`, the scaffold kind (`generic` or `Framework`), the metadata
written, and the lock row when Framework ownership was recorded.

`full` shows the content of each created file verbatim with real line breaks
under a `--- <path> (new file) ---` header, the before and after hashes of
rewritten Entries sections, and recovery facts in words.

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

The command is exactly route init; data follows the catalogue:

| Level    | `data`                                                                                                            |
| -------- | ----------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, target { id, path }, scaffold, entrypoints: [ { path, outcome, needsAuthoring } ], listedIn: [ path ] }` |
| standard | + `metadata { description, responsibility, tags, sources }`, `lockPath`                                           |
| full     | + per entrypoint `content` (string), per section `before`, `after`, `frameworkFingerprint`, `verification`        |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                                   | Headline                                                                 | Exit | Stream |
| ----------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------------ | ---: | ------ |
| completed               | entrypoints created                                                    | `Created <path>` (one entrypoint) / `Created <N> entrypoints for <id>.`  |    0 | stdout |
| completed               | already initialized                                                    | `<id> is already initialized. Nothing to do.`                            |    0 | stdout |
| completed (dry run)     | planned                                                                | `Would create <path>` / `Would create <N> entrypoints for <id>.`         |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                               | + family row                                                             |    2 | stdout |
| incomplete              | a fact could not be read                                               | `The route could not be initialized: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | bad target or metadata                                                 | `Cannot initialize <target>: <problem>.`                                 |    4 | stderr |
| blocked                 | unsafe target, ambiguity, Framework mode needs install or update, lock | `Cannot initialize <target>: <reason>.`                                  |    5 | stderr |
| failed                  | after effects                                                          | `Route init stopped after <n> of <m> changes.`                           |    1 | stderr |
| cancelled               | Ctrl+C                                                                 | `Route init was cancelled. Nothing was changed.`                         |  130 | stderr |

### Current merged behavior and open questions

The finding route-init.needs-authoring is informational, contributes nothing to
status or exit code, and is rendered as the advisory sentence rather than an
Info row. The catalogue change is accepted: a placeholder scaffold is
completed at exit 0 with needsAuthoring: true in JSON.

Direct operation-level invalid-metadata keeps a generic cause while the command
binder emits the catalogue's tag-specific causes. Maintainer decision remains
open; this contract records both current forms.

## Errors And Boundaries

The finding catalogue is:

| Code                                     | Severity | Family                       | Message                                                                                                                                  | Next                           |
| ---------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ |
| route-init.invalid-input                 | error    | invalid-input                |                                                                                                                                          |                                |
| route-init.invalid-target                | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.invalid-target`).                                  | `open-forge route init --help` |
| route-init.invalid-metadata              | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.invalid-metadata`).                                                                   | corrected command              |
| route-init.workspace-unavailable         | error    | workspace-unavailable        |                                                                                                                                          |                                |
| route-init.workspace-unsafe              | error    | workspace-unsafe             |                                                                                                                                          |                                |
| route-init.target-unsafe                 | error    | target-unsafe                |                                                                                                                                          |                                |
| route-init.route-ambiguous               | error    | route-ambiguous              |                                                                                                                                          |                                |
| route-init.identity-collision            | error    | identity-collision           | (blocking here: the new ID would collide)                                                                                                | choose another name            |
| route-init.loader-unsafe                 | error    | local                        | [`route.init.message.agents-loader-md-could-not-be-verified-safely`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Init/RouteInitText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.loader-unsafe`).                                                                                        | `open-forge doctor`            |
| route-init.framework-payload-invalid     | error    | payload-invalid              |                                                                                                                                          |                                |
| route-init.framework-payload-unavailable | warning  | payload-unavailable          |                                                                                                                                          |                                |
| route-init.framework-install-required    | error    | local                        | [`route.init.message.the-framework-scaffold-needs-an-installed-framework`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Init/RouteInitText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.framework-install-required`).                                                                                   | `open-forge install --dry-run` |
| route-init.framework-update-required     | error    | local                        | [`route.init.message.the-installed-framework-is-older-than-the-one-this-cli-ships`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Init/RouteInitText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.framework-update-required`).                                                                          | `open-forge update`            |
| route-init.framework-alignment-blocked   | error    | local                        | [`route.init.message.the-installed-framework-does-not-match-the-version-this-cli-ships-so-the-framework-scaffold-cannot-be-used`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Init/RouteInitText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Init/Shared/Wording/RouteInitWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-init.framework-alignment-blocked`).                           | `open-forge doctor`            |
| route-init.metadata-unsafe               | error    | metadata-unsafe              |                                                                                                                                          |                                |
| route-init.generated-region-unsafe       | error    | generated-region-unsafe      |                                                                                                                                          |                                |
| route-init.lifecycle-blocked             | error    | lifecycle-blocked            |                                                                                                                                          |                                |
| route-init.workspace-lock-unavailable    | error    | workspace-lock-unavailable   |                                                                                                                                          |                                |
| route-init.target-changed                | error    | target-changed               |                                                                                                                                          |                                |
| route-init.recovery-conflict             | error    | recovery-conflict            |                                                                                                                                          |                                |
| route-init.inspection-incomplete         | warning  | inspection-incomplete        |                                                                                                                                          |                                |
| route-init.metadata-incomplete           | warning  | metadata-incomplete          |                                                                                                                                          |                                |
| route-init.projection-incomplete         | warning  | projection-unavailable       |                                                                                                                                          |                                |
| route-init.lifecycle-unavailable         | warning  | lifecycle-unavailable        |                                                                                                                                          |                                |
| route-init.recovery-unavailable          | warning  | recovery-unavailable         |                                                                                                                                          |                                |
| route-init.needs-authoring               | info     | local                        | `Its description and tags are placeholders. Edit them before relying on this route.` (rendered as the advisory line, not as an Info row) | none                           |
| route-init.recovery-artifact-retained    | warning  | recovery-artifact-retained   |                                                                                                                                          |                                |
| route-init.target-changed-during-apply   | error    | target-changed-during-apply  |                                                                                                                                          |                                |
| route-init.write-failed                  | error    | write-failed                 |                                                                                                                                          |                                |
| route-init.verification-failed           | error    | verification-failed          |                                                                                                                                          |                                |
| route-init.lifecycle-publication-failed  | error    | lifecycle-publication-failed |                                                                                                                                          |                                |
| route-init.recovery-failed               | error    | recovery-failed              |                                                                                                                                          |                                |
| route-init.operation-failed              | error    | operation-failed             |                                                                                                                                          |                                |
| route-init.interrupted                   | error    | interrupted                  |                                                                                                                                          |                                |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`entrypointsCreated`, `entrypointsPresent`, `sectionsUpdated`.

## Scenarios

`new-chain`, `already-initialized`, `dry-run`, `framework-scaffold`,
`explicit-metadata`, `invalid-target`, `invalid-metadata`,
`framework-not-installed` (blocked), `lock-held`, `write-failed-partial`,
`cancelled`.


Framework mode blocked -> `open-forge install --dry-run` or `open-forge
update`; invalid -> the corrected command; otherwise none. The old `Next:
open-forge route update` without an operand is never printed.

## Representative Transcripts

### completed

~~~text
Created 2 entrypoints for documents/design.
Workspace: <workspace>
  .agents/documents/_documents.md
  .agents/documents/design/_design.md
  Its description and tags are placeholders. Edit them before relying on this route.
~~~

### completed-with-warnings

~~~text
Created .agents/documents/_documents.md
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
The route could not be initialized: <limitation>. Nothing was changed.
~~~

### invalid-input

~~~text
Cannot initialize documents: Route Init metadata is invalid.
Workspace: <workspace>
Next: open-forge route init documents --description "<one sentence>" --tag <Tag>
~~~

### blocked

~~~text
Cannot initialize .agents/memory/crystallized/documents/_documents.md: The Framework scaffold needs an installed Framework.
Workspace: <workspace>
Next: open-forge install --dry-run
~~~

### failed

~~~text
Route init stopped after 1 of 3 changes.
Workspace: <workspace>
  Error  documents/design  Write failed
         Writing documents/design failed. Stopped after 1 of 3 changes. Recovery data: <recovery-bundle>.
  Created .agents/documents/design
  .agents/documents/_documents.md  not started
  .agents/documents/design/_design.md  not started
  Its description and tags are placeholders. Edit them before relying on this route.
Next: open-forge route init --detail debug
~~~

### cancelled

~~~text
Route init was cancelled. Nothing was changed.
Workspace: <workspace>
  Error  documents  Route Init was cancelled
         Route Init was cancelled. Nothing was changed.
         open-forge route init
Next: open-forge route init
~~~

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

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.init.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Init/RouteInitText.cs).

<!-- @OpenForgeTextRef route.init.help.syntax -->
<!-- @OpenForgeTextRef route.init.message.agents-loader-md-could-not-be-verified-safely -->
<!-- @OpenForgeTextRef route.init.message.the-framework-scaffold-needs-an-installed-framework -->
<!-- @OpenForgeTextRef route.init.message.the-installed-framework-does-not-match-the-version-this-cli-ships-so-the-framework-scaffold-cannot-be-used -->
<!-- @OpenForgeTextRef route.init.message.the-installed-framework-is-older-than-the-one-this-cli-ships -->
