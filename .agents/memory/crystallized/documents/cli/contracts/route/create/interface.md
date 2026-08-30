---
open-forge:
  description: Accepted current public contract for creating one ordinary routed Markdown file with explicit metadata and optional Template body content
  responsibility: Define what route create accepts, creates, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Route, Create, Template, Mutation, CurrentTruth]
---

# route create Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route create`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
result schema, process-status mapping, System.CommandLine binding, fixed Markdig
pipeline, source-generated YamlDotNet and JSON paths, BCL-first filesystem
boundary, workspace lock, recovery boundary, test evidence, runtime, Native AOT,
and source layout. This Interface Contract adds no competing implementation
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
  numeric process-status mapping are defined by the CLI Architecture. The
  semantic result names and meanings below remain part of this contract.
- YAML and Markdown realization uses the Architecture's accepted source-
  generated YAML path and fixed Markdown pipeline while preserving the byte,
  compatibility, and canonical-syntax requirements below.
- Filesystem APIs, physical identity, symlink and junction behavior, case and
  Unicode rules, atomic replacement, containment implementation, and test seams
  must satisfy the Architecture's BCL-first and real-filesystem boundary.
- The recovery policy and preservation goals are current; recovery-bundle names,
  collision-handling mechanics, and related realization details remain
  implementation details constrained by the Architecture's recovery boundary.
- Expected-state revalidation and preservation of unexpected concurrent edits
  are current safety meaning. Mutation locking follows the Architecture's
  accepted workspace-lock and cross-platform concurrency boundary.
- The Architecture defines .NET modules, parser and serializer ownership,
  shared graph or mutation boundaries, and source layout. No Route Technical
  Design exists for this command.

## Purpose

`route create` creates one ordinary routed Markdown file below one existing
routable folder. It can create a metadata-only source or copy the body of one
explicit Template into the new destination.

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
  --description <text>
  --tag=<tag>...
  [--responsibility <text>]
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines
`--workspace`, `--json`, `--view`, `--verbose`, `--help`, and `--version`. All
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

- `--description <text>` is required and defines destination `description`.
- Repeated `--tag=<tag>` values define destination `tags` in argument order.
- `--responsibility <text>` is optional and defines destination
  `responsibility` when its value is non-empty.
- `--template <template-reference>` is optional and selects starting body
  content from one existing routed Markdown Template.
- `--dry-run` selects the write-policy preview described in [Dry Run And
  Apply](#dry-run-and-apply).
- The six global flags are accepted with the meanings in the shared [Global CLI
  Flags](../../shared/global-flags/interface.md) contract.

`--tag` is a required multi-value flag. Repetition retains argument order. The
exact empty, duplicate, and syntax rules are defined in [Destination Metadata](#destination-metadata).
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

The final parent folder must already contain exactly one recognized entrypoint.
The command does not initialize a missing route chain. Use `route init` first
when an ancestor or parent entrypoint is missing.

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

`--description` is required, must be non-empty, and must contain more than
whitespace. At least one `--tag` is required. Repeated tags retain argument
order. Each tag follows canonical tag syntax and omits the `#` prefix. Empty or
duplicate exact tags are invalid.

`--responsibility` is optional. A non-empty value adds the field. An exact empty
value, `--responsibility ""`, omits it. A whitespace-only value is invalid.
There is no separate removal flag because the destination does not exist yet.

The command validates syntax and presence. It does not derive a description,
responsibility, or tag from the filename, parent, Template metadata, Template
body, or another routed source. It does not inspect Template placeholders or
infer authoring quality. Semantic accuracy remains authored responsibility.

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
becomes destination metadata. Explicit destination metadata is always required
and has no precedence relationship with Template metadata because the two
sources answer different questions.

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

The parent entrypoint's generated `Entries` must expose the new routed file from
its destination description and tags. The command plans this generated effect
against the hypothetical post-create workspace before any persistent effect.

The automatic effect uses the complete [Index Interface Contract](../../index-candidate/interface.md)
projection, ordering, generated-boundary, verification, and recovery behavior.
It is part of the same parent plan, dry run, application, and result. The
command never starts a hidden `index` subprocess.

Missing or ambiguous parent markers, invalid sibling metadata, unsafe
destinations, or another projection blocker prevents creation before writes.
The command does not create a file that its parent cannot safely index.

## Planning And Effects

The operation follows the accepted typed mutation flow:

```text
validated target, metadata, and optional Template
  -> parent route and Template facts
  -> complete intended destination bytes
  -> generated-navigation projection
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan contains at most one new routed file plus the dependency-minimal
generated-navigation changes required to expose it. One blocker prevents every
effect. The command has no partial-application or best-effort mode.

The command preserves every existing user-owned source outside planned bounded
generated interiors. It does not format siblings, adapt the Template
semantically, create parent folders, or change overwrite companions.

## Dry Run And Apply

`--dry-run` uses the same request, current facts, intended bytes, generated
projection, planner, expected-state facts, preflight, and status formation as
application. It shows the complete new file, generated-navigation effects, and
every exact existing-file diff, then writes nothing. Safely established planned
changes are `complete`; planned changes alone do not create `attention`.

Omitting `--dry-run` selects application. The explicit command, target,
metadata, and optional Template confirm creation of the intended file and
replacement of only planned machine-owned generated interiors. The command does
not prompt and does not accept `--yes`.

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
schema-v1 `manifest.json` and streamed ordinal payload entries record
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
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

The default expanded view uses the complete blocks below. Every workspace-aware
human result retains `Workspace`, `Selected by`, and the target identity when it
is available. Compact view keeps the
semantic result, workspace, selection method, and target identity when
available, preview or application mode, completeness and safety, the Template
identity when supplied, affected paths, generated-navigation effects, and the
exact effects or diffs required for a preview while omitting optional
explanation and provenance. It retains at most one required `Next:` action.
Structured results retain at most one required `Next:` action as well. Complete
results have no `Next:` action. Incomplete results and direct errors name one
required correction when one exists. Failed and interrupted results name
ordinary recovery when needed. Results do not provide diagnosis or
recommendations. Dry-run compact output still shows every planned affected path
and every exact planned effect or diff.

Primary human rendering for `complete`, `attention`, and `incomplete` results
goes to stdout. Primary human rendering for `invalid`, `blocked`, `failed`, and
`interrupted` results goes to stderr. Each primary typed result stays together
on its assigned stream. Bounded diagnostics, including verbose diagnostics, use
stderr when emitted.

### Verified No-Op

```text
The routed file already matches the requested content.
Workspace: D:/work/example
Selected by: current directory
Target: memory/crystallized/decisions/cache-policy
Path: .agents/memory/crystallized/decisions/cache-policy.md
No files changed.
```

### Successful Application

```text
The routed file was created.
Workspace: D:/work/example
Selected by: current directory
ID: memory/crystallized/decisions/cache-policy
Path: .agents/memory/crystallized/decisions/cache-policy.md
Updated 1 generated region.
```

### Successful Dry Run

```text
The routed file would be created.
Workspace: D:/work/example
Selected by: --workspace
ID: memory/crystallized/decisions/cache-policy
Path: .agents/memory/crystallized/decisions/cache-policy.md

<new file and exact bounded diffs>

No files changed (--dry-run).
```

Default human output identifies the Template when a Template supplied the body
content and lists each changed existing path. It states what happened without
naming successful internal stages. Verbose and structured output may include
planning and preflight evidence.

Every error names the route creation, target, direct cause, and one required
correction when one exists. It does not diagnose authoring quality or Template
placeholder completion.

## Structured Output

`--json` returns the complete typed result used by human rendering. It never
prompts and never reruns planning, application, or verification.
It writes one complete structured result to stdout for every semantic status,
including `incomplete` and `attention`. Bounded
diagnostics use stderr, and ordinary human text is never mixed into structured
JSON stdout.

The structured result exposes:

- Workspace and selection method.
- Requested target and resolved target ID and canonical path.
- Resolved parent entrypoint ID, path, and canonical or compatibility form.
- Explicit destination metadata.
- Optional Template ID, path, classification, and body-copy evidence.
- Application or dry-run mode, completeness, and safety state.
- Intended destination and generated-region effects.
- Dry-run, recovery-bundle, application, verification, and recovery facts.
- Changed and unchanged effects, verification facts, and typed observed or
  unknown recovery facts. An exact residual or expected path appears only when
  the recovery result provides one, without classifying current target state.
- Coverage observations, availability conditions, completeness and safety state,
  semantic status, and at most one required `Next:` action when applicable.

Exact field names, schema versioning, and compatibility rules are defined by the
CLI Architecture.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                               |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Normal creation, valid Template instantiation, a safe dry-run with planned changes, generated-navigation effects, or application and final verification completed, including a verified identical-target no-op.                       |
| `attention`   | Post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`; target effects remain successful, and human output says `requires attention` with the exact residual path and cleanup guidance. |
| `incomplete`  | Safe facts are available, but required inspection or planning coverage cannot complete; no mutation begins.                                                                                                                           |
| `invalid`     | Command input, metadata, Template reference, flag use, or target shape does not follow this interface.                                                                                                                                |
| `blocked`     | A valid request cannot establish or apply one safe complete creation plan because safety or authority is unsafe or ambiguous; no mutation begins.                                                                                     |
| `failed`      | An unexpected application or verification failure occurs after a persistent effect begins, or recovery deletion returns `Failed`/`Unknown`; `Failed`/positively observed `Retained` recovery is the distinct `attention` case.        |
| `interrupted` | The caller cancelled before completion; an unexpected application or verification failure remains `failed`.                                                                                                                           |

Planned changes do not create `attention`. Only `Failed`/positively observed
`Retained` recovery after verified target effects creates it. The command does not inspect
Template placeholders or infer authoring quality to manufacture another
condition.

For ordinary operation conditions, status precedence is
`blocked` > `incomplete` > `attention` > `complete`. Invalid input stops before
operation resolution and forms `invalid`. Failed and interrupted results retain
their event meaning.

The shared process-status mapping is defined by the CLI Architecture.

## Scenarios

The source-defined representative scenarios remain in their public sections:

- The ID target and metadata-only creation example is in [File Target](#file-target).
- The explicit Template selection example is in [Template Selection](#template-selection).
- Verified no-op, successful application, and successful dry-run results are in
  [Human Output](#human-output).

These links organize the complete examples without adding another invocation
or result shape.

## Errors

The command blocks or rejects:

- A target that is not one ordinary Markdown file below `.agents`.
- A missing, ambiguous, or unsafe parent route.
- A source-ID, child entrypoint, portable path, or physical identity collision.
- An orphan overwrite at the target.
- Missing or invalid destination description or tags.
- A Template reference that does not resolve to one valid routed Template.
- An existing target whose bytes differ from the intended result.
- An invalid generated ownership boundary or sibling projection.
- Unavailable or unsafe recovery-bundle storage is `incomplete`; an unverified
  bundle is `blocked`.
- A changed source or destination that invalidates the plan.

If safe facts are available but required inspection or planning coverage cannot
complete, the result is `incomplete` and no write begins. An unsafe or ambiguous
safety or authority fact is `blocked` instead. Direct errors name the required
correction when one exists; they do not diagnose authoring quality or provide a
recommendation.

## Non-Goals

`route create` does not:

- Create an entrypoint or missing route chain.
- Create the Loader.
- Copy Template frontmatter or retain Template ownership.
- Infer, merge, or override destination metadata from Template content.
- Substitute placeholders or claim the result is finished.
- Inspect Template placeholders, infer authoring quality, diagnose content, or
  provide recommendations.
- Overwrite, adopt, move, or remove an existing different file.
- Modify an overwrite companion.
- Create a Git commit.

Use `route init` for route chains and `route update` for existing routed
Markdown.

## Verification

Gate 5 executable proof must cover:

- ID and exact ordinary-file targets, spaces, Unicode, unsafe segments, and
  every excluded file kind.
- Canonical and compatibility parent entrypoints, missing parents, and
  ambiguous route structures.
- Required description and tags, optional responsibility, exact empty
  responsibility, singleton repetition rejection
  for description, responsibility, and Template, repeated Boolean idempotence,
  tag ordering, duplicates, empty values, and invalid values.
- Metadata-only creation.
- Template ID and exact-path selection, exact `Template` classification,
  collisions, blocked overwrite companions, invalid non-Templates, and orphan
  overwrites.
- Proof that Template frontmatter never enters destination metadata.
- Exact Template body copying without substitution and without a retained
  lifecycle relationship.
- Missing targets, identical existing targets, different existing targets,
  unsupported targets, and orphan target overwrites.
- Complete dry-run output and no persistent dry-run effects.
- Dry-run and application parity for request, facts, intended bytes, generated
  projection, plan, preflight, and status, with planned changes remaining
  `complete`.
- Verified no-op behavior before mutation and recovery-bundle preparation.
- All seven semantic statuses, including safe-coverage `incomplete`, blocked
  unsafe or ambiguous safety and authority, post-write or `Failed`/`Unknown`
  recovery `failed`, and `Failed`/positively observed `Retained` recovery
  `attention`.
- Human stream allocation, one JSON result on stdout for every status, bounded
  diagnostics on stderr, compact retention and at-most-one `Next:` behavior,
  and no mixed human text in JSON output.
- Human and structured results from one typed result without Template
  placeholder inspection or authoring-quality inference.

The [Behavior Contract](behavior.md) records the semantic, projection, effect,
safety, recovery, and conformance evidence for the remaining verification
obligations, including automatic parent effects, recovery-bundle behavior,
expected-state changes, post-verification deletion state/disposition facts, and rerun
convergence.

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
