---
open-forge:
  description: Accepted current public contract for creating one ordinary routed Markdown file with optional metadata and optional Template body content
  responsibility: Define what route create accepts, creates, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Create, Template, Mutation, CurrentTruth]
---

# route create Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route create`. The command is implemented in the merged CLI. Its local
implementation and complete executable proof are squash-integrated at
`19412d2`; the merged native CLI is the current delivery.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared result schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines System.CommandLine binding, fixed
Markdig and source-generated serialization relationships, the BCL-first
filesystem boundary, workspace-lock and recovery boundaries, evidence, runtime,
Native AOT, and source layout. This Interface Contract adds no competing implementation
choice.

The current [Template contract](../../../../framework/primitives/templates.md)
defines one-time instantiation and ownership transfer. The [canonical Markdown
syntax](../../../../framework/markdown/syntax.md)
defines destination metadata. The [routing model](../../../../framework/routing/model.md)
defines how the parent entrypoint exposes the created file.

### Architecture-constrained realization

The following realization details are constrained by the CLI Architecture and
Gate 5 proof. This Interface Contract does not add command-local technology
choices:

- Human result streams and command-specific repetition are assigned in this
  contract. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
  contract remains authoritative for shared flags, including their repetition
  and composition; this command does not add precedence or last-wins behavior.
- The shared structured field names, schema versioning, compatibility rules, and
  numeric process-status mapping are defined by the [Shared Result
  Coordinates](../../shared/result-coordinates/interface.md). The
  semantic result names and meanings below remain part of this contract.
- YAML and Markdown realization uses the Architecture's accepted source-
  generated YAML path and fixed Markdown pipeline while preserving the byte,
  compatibility, and canonical-syntax requirements below.
- Filesystem APIs, physical identity, symlink and junction behavior, case and
  Unicode rules, containment implementation, and test seams must satisfy the
  Architecture's BCL-first and real-filesystem boundary. Exact atomic-file
  mechanics follow the [Mutation And Recovery Technical
  Design](../../../technical-designs/mutation-and-recovery.md).
- The recovery policy and preservation goals are current; exact recovery-bundle
  names, collision handling, and related realization follow the [Mutation And
  Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
  within the Architecture's recovery boundary.
- Expected-state revalidation and preservation of unexpected concurrent edits
  are current safety meaning. Mutation locking follows the Architecture's
  accepted workspace-lock and cross-platform concurrency boundary.
- The Architecture defines .NET modules, parser and serializer ownership,
  shared graph or mutation boundaries, and source layout. No Route Technical
  Design exists for this command.

## Purpose

`route create` creates one ordinary routed Markdown file inside an existing
Loader-recognized route root. Its final parent may already exist or may be a
missing intermediate folder within that root. It can create a metadata-only
source or copy the body of one explicit Template into the new destination.

The destination receives its own authored metadata and becomes independent. The
Template's frontmatter, route identity, ownership, and future changes never
transfer to it.

Given the same workspace bytes and explicit input, the command selects the same
target, produces the same intended file and generated navigation, and returns
the same semantic result. Repeating it against an identical existing result
returns a verified no-op.

## Syntax

```text
open-forge route create <file-target>
  [--description <text>]
  [--tag=<tag>]...
  [--responsibility <text>]
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines
`--workspace <path>`, `--format <text|json>`, `--detail <minimal|standard|full|debug>`, repeatable `--detail-filter <error|warning|info|all>`, `--help`, and `--version`. All
six apply to `route create` under that contract.

`--description`, `--responsibility`, and `--tag` define destination metadata.
`--template` selects optional starting body content. `--dry-run` is the write-
policy preview.

The command has no implicit Template, Template machine-name registry, stdin
mode, content-value flag, `--yes`, `--force`, overwrite mode,
`--no-responsibility`, wizard, automatic mode, or alias.

## Operands

The file target is the required positional operand. Its complete accepted forms
are defined in [File Target](#file-target). It is not a directory operand or a
general external filesystem path.

## Flags

The command-specific flags have these public states and meanings:

- `--description <text>` is optional and defines destination `description` when
  supplied. Omission is valid and is reported as optional metadata missing.
- Repeated `--tag=<tag>` values optionally define destination `tags` in argument
  order. Omission is valid and is reported as optional metadata missing.
- `--responsibility <text>` is optional and defines destination
  `responsibility` when its value is non-empty.
- `--template <template-reference>` is optional and selects starting body
  content from one existing routed Markdown Template.
- `--dry-run` selects the write-policy preview described in [Dry Run And
  Apply](#dry-run-and-apply).
- The six global flags are accepted with the meanings in the shared [Global CLI
  Flags](../../shared/global-flags/interface.md) contract.

`--tag` accepts `--tag Memory`, `--tag=Memory`, and `--tag:Memory` with identical
meaning. It is an optional multi-value flag. When supplied, repetition retains
argument order. The exact empty, duplicate, and syntax rules are defined in
[Destination Metadata](#destination-metadata).
`--description`, `--responsibility`, and `--template` are singleton flags. Any
repeated occurrence of one of them is invalid, even when the repeated value is
identical; no last occurrence wins. Repeated `--dry-run` occurrences are
accepted and idempotent. The shared global
flags keep their own repetition and composition rules, with no command-specific
precedence or last-wins behavior.

## File Target

A file target identifies one intended ordinary Markdown file through one of
these forms:

```text
<source-id>
.agents/<folders>/<filename>.md
./.agents/<folders>/<filename>.md
```

For this operation, an ID maps to an ordinary Markdown path:

```text
open-forge route create memory/crystallized/decisions/cache-policy \
  --description "Why the cache policy was chosen" \
  --tag=Memory \
  --tag=Decision
```

The target path is:

```text
.agents/memory/crystallized/decisions/cache-policy.md
```

An exact path must name the same ordinary Markdown target shape. A directory,
Loader, canonical or compatibility entrypoint filename, overwrite companion,
`SKILL.md`, or non-Markdown resource is invalid.

The shared [CLI Source References](../../shared/source-references/interface.md) contract defines
ID segments, exact path detection, quoting, containment, and result identity.
`route create` adds only the deterministic ordinary-file mapping above.

The final parent folder may already contain exactly one recognized entrypoint,
or it may need a canonical entrypoint within a chain below an existing
Loader-recognized root. Safely existing ordinary intermediate directories may
be reused. For missing route structure, the command creates only absent directories
and canonical entrypoints from the root toward the final parent. Each new
entrypoint uses its automatic ID heading, inherited Axioms, and generated
direct-child Entries. Existing recognized entrypoints retain their actual
filenames.

An unknown or unrecognized root is invalid input with process status 4 and never
advises `route init`. A missing Framework or other required existing boundary
retains the `route-create.parent-missing` boundary and its existing `route init`
next action.

An existing child entrypoint or another source with the same route identity
blocks creation. Exact path input can disambiguate source selection, but it
cannot authorize a structurally ambiguous route or create two direct children
that cannot retain distinct identities.

An orphan overwrite companion at the intended base path blocks. The command
does not adopt, delete, or reinterpret it.

## Destination Metadata

The destination starts with canonical scoped frontmatter:

```yaml
---
open-forge:
  description: Why the cache policy was chosen
  responsibility: Record the accepted choice and its durable rationale
  tags: [Memory, Decision]
---
```

Only explicitly supplied destination fields are written. An omitted description
or tag list is absent; the command never fabricates description, tags, or
responsibility from a filename, parent, Template, Template body, or another
routed source. An empty mapping is valid when no destination field is supplied.

When supplied, `--description` must be non-empty and contain more than
whitespace. Supplied tags retain argument order. Each tag follows canonical tag
syntax and omits the `#` prefix. Empty or duplicate exact tags are invalid.

`--responsibility` is optional. A non-empty value adds the field. An exact empty
value, `--responsibility ""`, omits it. A whitespace-only value is invalid.
There is no separate removal flag because the destination does not exist yet.

The command validates syntax and supplied values. It does not inspect Template
placeholders or infer authoring quality. Semantic accuracy remains authored
responsibility. Omitting description or tags produces the
`route-create.optional-metadata` Attention2 warning after a successful create or
dry run; the warning alone is not a file or directory effect.

## Template Selection

`--template` accepts the automatic ID or exact `.agents/...` path of one existing
routed Markdown Template:

```text
open-forge route create memory/crystallized/decisions/cache-policy \
  --template templates/memory/decision \
  --description "Why the cache policy was chosen" \
  --tag=Memory \
  --tag=Decision
```

The Template reference follows the shared existing-source grammar. A collision
requires exact-path disambiguation. JSON and non-interactive use never prompt.

The selected source must be ordinary routed Markdown whose base frontmatter
contains the exact canonical `Template` tag. The tag provides a deterministic
source classification for this operation. It does not create a Templates root
route, activate another primitive contract, or grant the source authority over
the destination. The selected source's loaded route and content still define
how the Template should be used.

An entrypoint, Loader, overwrite companion, or ordinary routed file without
that classification is invalid. A Template with an overwrite companion also
blocks because one-file instantiation has no accepted rule for collapsing two
authored layers into one body. Create or select a standalone Template with the
intended body instead. An orphan or ambiguous overwrite blocks for the same
operation.

The command copies Template body content after removing the Template source's
own frontmatter. It does not perform semantic placeholder substitution. Visible
Template prompts remain in the copied body; the command does not inspect them or
infer authoring quality from them.

Template frontmatter describes and classifies the Template source. None of it
becomes destination metadata. Supplied destination metadata has no precedence
relationship with Template metadata because the two sources answer different
questions; omitted destination metadata remains absent.

The copied body is starting content only. The result stores no Template
receipt, origin field, update relationship, or hidden ownership marker. Later
Template changes do not update the destination.

When `--template` is omitted, the destination contains only canonical
frontmatter and the canonical trailing line ending. A metadata-only routed file
is valid unless its selected route or component contract requires more content.
`route update --template` may later add a Template body while the file remains
frontmatter-only.

The [Framework Templates](../../../../framework/primitives/templates.md)
contract defines one-time instantiation, independent destination content, and
relinquished Template authority. This command keeps its Template selection and
body rules local; it does not create a route-family shared Template contract.

## Existing Target

The command never overwrites an existing target.

When the target exists, the command resolves the complete intended bytes from
the current explicit input:

- If the existing bytes and required generated navigation already match, return
  a verified no-op.
- If the existing target differs, block and direct the caller to `route update`
  or an explicit future replacement operation.
- If the existing path has an unsupported kind, unsafe identity, or ambiguous
  route relationship, block.

An existing identical file is not adopted as CLI-managed content. The no-op
states only that this creation request is already satisfied.

## Generated Navigation

The command plans generated `Entries` for the hypothetical post-create workspace
before any persistent effect. It updates the existing exposing parent when one
is present and creates canonical entrypoints for missing intermediate folders.
Every new entrypoint has its automatic-ID heading, inherited Axioms, and
generated direct-child Entries. A generated child entry reflects only the
child's actual identity and explicitly supplied metadata; it never fabricates
description, tags, or responsibility and never copies leaf metadata to an
ancestor.

The automatic effect uses the complete [Index Interface Contract](../../index-candidate/interface.md)
projection, ordering, generated-boundary, verification, and recovery behavior.
It is part of the same parent plan, dry run, application, and result. The
command never starts a hidden `index` subprocess.

Missing or ambiguous Entries headings in an existing region, invalid sibling
metadata, unsafe destinations, an unknown root, or another projection blocker
prevents creation before writes. The command does not create a file or route
chain that cannot be safely represented.

## Planning And Effects

The operation follows the accepted typed mutation flow:

```text
validated target, optional metadata, and optional Template
  -> parent route and Template facts
  -> missing-directory and entrypoint facts
  -> complete intended destination and generated-entry bytes
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan is ordered as `Directory*` outer-to-inner, `Entrypoint*`
outer-to-inner, the new `RoutedFile`, and then existing `GeneratedRegion*`
replacements in deterministic path order. One blocker prevents every effect.
The command has no partial-application or best-effort mode.

The command preserves every existing user-owned source outside planned bounded
generated interiors. It does not create a missing Loader root, format siblings,
adapt the Template semantically, change overwrite companions, or copy leaf
metadata to ancestors.

## Dry Run And Apply

`--dry-run` uses the same request, current facts, intended bytes, generated
projection, planner, expected-state facts, preflight, and status formation as
application. It shows every planned new directory and canonical entrypoint, the
complete new file, generated-navigation effects, and every exact existing-file
diff, then writes nothing. A warning for omitted optional metadata creates no
effect; a no-op is reported only when no directory or file change is planned.

Omitting `--dry-run` selects application. The explicit command, target, supplied
metadata, and optional Template confirm creation of the planned directories,
entrypoints, destination, and replacement of only planned machine-owned
generated interiors. The command does not prompt and does not accept `--yes`.

A verified no-op has no affected mutation path and creates no bundle. An actual
creation checks the planned new path for collision and, when the plan contains
an existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a pre-effect
`incomplete` result. It prepares exactly one immutable ZIP bundle outside
the workspace. An operation containing only
Create effects or no-ops creates no bundle. Its source-generated
versioned `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change
kinds, exact prior bytes/lengths/hashes, and intended final absence or
length/hash. `Create` effects (including the new destination) and no-ops have
no entry. A CreateNew draft is closed/reopened for semantic manifest, exact
ordered entry, length, hash, and payload-byte validation, moved within the same
directory to its deterministic final name, and reopened and verified. Only the
valid final ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. `FileChangeApplier` requires the matching preparation for every
existing-target effect and performs one final effect per
target. All preparation completes before the first target effect.

Immediately before application, the command rechecks every target, source,
Template, route, and collision fact. It applies complete planned bytes, verifies
each effect, then verifies destination identity, metadata, copied body, parent
route exposure, and generated navigation. Before post-verification deletion
begins, a handled application, verification, or cancellation outcome stops new
effects and reports the actual residual draft or final path; a valid final
remains when preparation completed. A closed final ZIP
may remain after abrupt process termination, without an executable crash or
power-loss guarantee. Recovery provenance does not classify current target
state, and no target is restored automatically. After whole-command
verification, delete only the positively recognized bundle created by this
operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command-specific context, full
adds all bounded facts, and debug adds bounded diagnostics on stderr. Detail
does not change semantics, counts, or status. Filters select finding severities;
all is the default filter.

Route create is one non-interactive mutation: it does not ask for confirmation and has no --automatic flag.

The catalogue text by detail level is:

`minimal`, created:

```text
Created .agents/memory/emerging/ideas/pricing/tiers.md  (memory/emerging/ideas/pricing/tiers)
  Listed in .agents/memory/emerging/ideas/pricing/_pricing.md
```

`minimal`, from a Template:

```text
Created .agents/memory/emerging/ideas/pricing/tiers.md  (memory/emerging/ideas/pricing/tiers)
  Body copied from the Template templates/memory/idea
  Listed in .agents/memory/emerging/ideas/pricing/_pricing.md
```

There is no pinned exact transcript for omitted optional metadata until runtime
evidence is accepted. The renderer reports the
`route-create.optional-metadata` warning finding and any supplied metadata; it
does not invent metadata values.

`standard` adds `Workspace:`, the supplied metadata fields, warning findings,
and the Template path.

`full` adds the new file's content verbatim under a `--- <path> (new file)
---` header and the before and after hashes of the parent's Entries section.

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

The command is exactly route create; data follows the catalogue:

| Level    | `data`                                                                   |
| -------- | ------------------------------------------------------------------------ |
| minimal  | `{ mode, target { id, path }, listedIn, template { id, path } \| null }` |
| standard | + `metadata { description, responsibility, tags }`; omitted description/responsibility are null, tags remain an array |
| full     | + `content`, per section `before`, `after`, `verification`               |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                   | Headline                                                              | Exit | Stream |
| ----------------------- | ------------------------------------------------------ | --------------------------------------------------------------------- | ---: | ------ |
| completed               | created with no optional-metadata finding              | `Created <path>  (<id>)`                                              |    0 | stdout |
| completed               | identical file already present with no finding         | `<path> already has the requested content. Nothing to do.`            |    0 | stdout |
| completed (dry run)     | planned with no optional-metadata finding             | `Would create <path>  (<id>)`                                         |    0 | stdout |
| completed-with-warnings | optional metadata omitted or recovery bundle retained  | + family row                                                          |    2 | stdout |
| incomplete              | template, parent or record unreadable                  | `The file could not be created: <limitation>. Nothing was changed.`   |    3 | stdout |
| invalid-input           | explicitly invalid supplied metadata, unknown/bad target | `Cannot create the routed file: <problem>.` (all problems in one run) |    4 | stderr |
| blocked                 | exists with different content, unsafe, ambiguous, lock | `Cannot create <path>: <reason>.`                                     |    5 | stderr |
| failed                  | after effects                                          | `Route create stopped after <n> of <m> changes.`                      |    1 | stderr |
| cancelled               | Ctrl+C                                                 | `Route create was cancelled. Nothing was changed.`                    |  130 | stderr |



## Errors And Boundaries

The finding catalogue is:

| Code                                     | Severity | Family                      | Message                                                                                                                                                      | Next                                |
| ---------------------------------------- | -------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------- |
| route-create.invalid-input               | error    | invalid-input               |                                                                                                                                                              |                                     |
| route-create.invalid-target              | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.invalid-target`). | `open-forge route create --help`    |
| route-create.invalid-metadata            | error    | local                       | `--description is empty.` / `--tag <value> is empty.` / `--tag <value> is repeated.` / `--tag <value> is not a valid tag.` joined with `and`       | corrected command                   |
| route-create.optional-metadata | warning | local | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Selection/RouteCreateReportSelector.cs): distinguish preview, verified creation, and already-current optional metadata. | `open-forge route update <target id>`; add an optional description or tag when useful. |
| route-create.invalid-template            | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.invalid-template`).                                                                                                                 | `open-forge find --tag Template`    |
| route-create.parent-missing              | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.parent-missing`); retained only for a missing Framework or other required existing boundary. | `open-forge route init <parent id>` |
| route-create.workspace-unavailable       | error    | workspace-unavailable       |                                                                                                                                                              |                                     |
| route-create.workspace-unsafe            | error    | workspace-unsafe            |                                                                                                                                                              |                                     |
| route-create.target-unsafe               | error    | target-unsafe               |                                                                                                                                                              |                                     |
| route-create.target-content-differs      | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.target-content-differs`).                                                                                                              | `open-forge route update <id>`      |
| route-create.route-ambiguous             | error    | route-ambiguous             |                                                                                                                                                              |                                     |
| route-create.identity-collision          | error    | identity-collision          | (blocking: the new ID would collide)                                                                                                                         | choose another name                 |
| route-create.metadata-unsafe             | error    | metadata-unsafe             |                                                                                                                                                              |                                     |
| route-create.template-unsafe             | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.template-unsafe`).                                                                                                           | none                                |
| route-create.generated-region-unsafe     | error    | generated-region-unsafe     |                                                                                                                                                              |                                     |
| route-create.workspace-lock-unavailable  | error    | workspace-lock-unavailable  |                                                                                                                                                              |                                     |
| route-create.target-changed              | error    | target-changed              |                                                                                                                                                              |                                     |
| route-create.recovery-conflict           | error    | recovery-conflict           |                                                                                                                                                              |                                     |
| route-create.inspection-incomplete       | warning  | inspection-incomplete       |                                                                                                                                                              |                                     |
| route-create.metadata-incomplete         | warning  | metadata-incomplete         |                                                                                                                                                              |                                     |
| route-create.projection-incomplete       | warning  | projection-unavailable      |                                                                                                                                                              |                                     |
| route-create.template-unavailable        | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Create/Shared/Wording/RouteCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-create.template-unavailable`).                                                                                                                      | none                                |
| route-create.recovery-unavailable        | warning  | recovery-unavailable        |                                                                                                                                                              |                                     |
| route-create.recovery-artifact-retained  | warning  | recovery-artifact-retained  |                                                                                                                                                              |                                     |
| route-create.target-changed-during-apply | error    | target-changed-during-apply |                                                                                                                                                              |                                     |
| route-create.write-failed                | error    | write-failed                |                                                                                                                                                              |                                     |
| route-create.verification-failed         | error    | verification-failed         |                                                                                                                                                              |                                     |
| route-create.recovery-failed             | error    | recovery-failed             |                                                                                                                                                              |                                     |
| route-create.operation-failed            | error    | operation-failed            |                                                                                                                                                              |                                     |
| route-create.interrupted                 | error    | interrupted                 |                                                                                                                                                              |                                     |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`filesCreated`, `sectionsUpdated`.

## Scenarios

`created`, `created-from-template`, `dry-run`, `already-matching` (no-op),
`missing-description`, `missing-tag`, `missing-both` (optional-metadata warnings),
`invalid-target`, `parent-missing`,
`exists-with-different-content` (blocked),
`template-unknown`, `lock-held`, `write-failed-partial`, `cancelled`.


## Representative Transcripts

### completed

~~~text
Created .agents/memory/project-alpha/overview.md  (memory/project-alpha/overview)
Workspace: <workspace>
  Listed in .agents/memory/project-alpha/_project-alpha.md
~~~

### completed-with-warnings

~~~text
Created .agents/memory/project-alpha/overview.md  (memory/project-alpha/overview)
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
The file could not be created: <limitation>. Nothing was changed.
~~~

### blocked

~~~text
Cannot create .agents/memory/project-alpha/overview.md: .agents/memory/project-alpha/overview.md already exists with different content.
Workspace: <workspace>
Next: open-forge route update memory/project-alpha/overview
~~~

### failed

~~~text
Route create stopped after 1 of 2 changes.
Workspace: <workspace>
  Error  .agents/memory/project-alpha/_project-alpha.md  Write failed
         Writing .agents/memory/project-alpha/_project-alpha.md failed. Stopped after 1 of 2 changes. Recovery data: <recovery-bundle>.
  Created .agents/memory/project-alpha/overview.md
  .agents/memory/project-alpha/_project-alpha.md  not started
Next: open-forge route create --detail debug
~~~

### cancelled

~~~text
Route create was cancelled. Nothing was changed.
Workspace: <workspace>
  Error  memory/project-alpha/overview  Route create was cancelled
         Route create was cancelled. Nothing was changed.
         open-forge route create
~~~

## Related Current Sources

- [route create Command Contract Set](_create.md)
- [Route Init Interface Contract](../init/interface.md)
- [Route Update Interface Contract](../update/interface.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Templates](../../../../framework/primitives/templates.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [CLI Contract Document Templates](../../../../../../../templates/cli/documents/_documents.md)
- [Behavior Contract](behavior.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.create.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Create/RouteCreateText.cs).

<!-- @OpenForgeTextRef route.create.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [RouteCreateWording.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Create/RouteCreateWording.cs)
  <!-- @OpenForgeTextRef route.create.title.optional-metadata-is-missing -->
  <!-- @OpenForgeTextRef route.create.message.would-create-without-optional-metadata -->
  <!-- @OpenForgeTextRef route.create.message.created-without-optional-metadata -->
  <!-- @OpenForgeTextRef route.create.message.current-without-optional-metadata -->
  <!-- @OpenForgeTextRef route.create.message.optional-metadata-is-missing -->
  <!-- @OpenForgeTextRef route.create.next.add-optional-description-or-tag -->
  <!-- @OpenForgeTextRef route.create.next.optional-metadata-inline -->
