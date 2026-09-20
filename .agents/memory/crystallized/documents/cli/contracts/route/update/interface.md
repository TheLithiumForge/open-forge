---
open-forge:
  description: Accepted current public interface for patching one existing routed Markdown source and completing a protected Template body when eligible
  responsibility: Define what a caller may enter and observe from `route update` without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, Interface, Metadata, Template, Mutation, CurrentTruth]
---

# route update Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route update`. The command is implemented in the merged CLI;
implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared result schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines System.CommandLine binding, fixed
Markdig and source-generated serialization relationships, the BCL-first
filesystem, workspace-lock and recovery boundaries, evidence, runtime, Native
AOT, and source layout. The observable byte-preservation and compatibility
requirements below remain the command's contract; their realization must satisfy
those authorities and Gate 5 proof.

The [canonical Markdown syntax](../../../../framework/markdown/syntax.md)
defines destination metadata. The [Template contract](../../../../framework/primitives/templates.md)
defines why copied starting content creates no continuing Template ownership.
The [Markdown compatibility boundary](../../../../framework/markdown/compatibility.md)
defines existing entrypoint filenames the command recognizes and preserves.

The [Behavior Contract](behavior.md) defines the deterministic operation behind
this public surface. Shared flag and source-reference meaning remains in the
[Global CLI Flags](../../shared/global-flags/interface.md) and [CLI Source References](../../shared/source-references/interface.md)
contracts. Generated navigation uses the complete [Index Interface Contract](../../index-candidate/interface.md)
projection.

## Purpose

`route update` changes explicitly selected Open Forge metadata on one existing
routed Markdown source. It may also copy one Template body when the destination
contains valid frontmatter and no authored body.

The operation is a field patch, not whole-file replacement. Omitted metadata
fields remain unchanged. An existing authored body remains byte-for-byte
unchanged even when `--template` is supplied.

Given the same workspace bytes and explicit input, the command produces the same
intended source and generated navigation. Repeating a successful update against
that state returns verified byte-level no-op facts. A protected Template request
retains `completed-with-warnings` when its explicit body intent remains unapplied; all other
successful repeated updates are `completed` verified no-ops.

## Syntax

Tag values accept `--tag Memory`, `--tag=Memory`, and `--tag:Memory` with
identical meaning. Repeated values retain their existing order and validation.
The separately defined empty-responsibility grammar remains unchanged.

```text
open-forge route update <source-reference>
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

The shared [Global CLI Flags](../../shared/global-flags/interface.md) contract defines
`--workspace <path>`, `--format <text|json>`, `--detail <minimal|standard|full|debug>`, repeatable `--detail-filter <error|warning|info|all>`, `--help`, and `--version`. All
six apply to `route update` under that contract.

The shared [CLI Source References](../../shared/source-references/interface.md) contract defines
the existing source and Template reference grammar, exact paths, quoting,
collisions, and overwrite identity.

`--description`, `--responsibility`, and `--tag` patch destination metadata.
`--template` selects optional starting body content. `--dry-run` is the
write-policy preview.

At least one metadata flag or `--template` is required. The command has no
whole-body value, implicit Template, Template machine-name registry, wizard,
automatic mode, `--no-responsibility`, `--yes`, `--force`, replacement mode,
alias, or other command-specific flag.

## Operands

`<source-reference>` is one required source operand. It uses one of the shared
reference forms:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

The prefix determines whether the value is an automatic source ID or an exact
workspace-relative `.agents` path. The complete grammar, quoting, exact-match,
collision, containment, and overwrite rules remain in [CLI Source References](../../shared/source-references/interface.md).

## Flags

| Flag                              | Role                               | Value                                                                                          | Omission                                           | Repetition and composition                                                                                                                   |
| --------------------------------- | ---------------------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `--description <text>`            | Selection of destination metadata  | One description value; empty or whitespace-only is invalid                                     | The destination `description` remains unchanged    | Singleton. Any repetition is invalid, even when the repeated value is equal. No last-wins behavior.                                          |
| `--responsibility <text>`         | Selection of destination metadata  | One responsibility value; whitespace-only is invalid; exact `""` removes the key               | The destination `responsibility` remains unchanged | Singleton. Any repetition is invalid, even when the repeated value is equal. No last-wins behavior.                                          |
| `--tag=<tag>`                     | Selection of destination metadata  | One canonical tag without the `#` prefix                                                       | The destination tag list remains unchanged         | Repeatable. Supplied values replace the complete tag list in command-line order; duplicate exact tags and an empty supplied set are invalid. |
| `--template <template-reference>` | Selection of starting body content | One automatic Template ID or exact `.agents/...` path for an existing routed Markdown Template | No Template body is selected                       | Singleton. Any repetition is invalid, even when the repeated reference is equal. No last-wins behavior.                                      |
| `--dry-run`                       | Write policy                       | Boolean flag with no value                                                                     | Application is selected                            | Repetition is accepted and idempotent; it does not add authority or precedence.                                                              |

All six [Global CLI Flags](../../shared/global-flags/interface.md) apply. Their complete spelling,
values, defaults, repetition, composition, terminal behavior, errors, and
presentation meaning remain defined only by that shared contract.

The command-specific repetition rules above are complete. `--description`,
`--responsibility`, and `--template` are singleton inputs, and any second
occurrence is invalid even when it repeats the same value. Repeated `--tag`
values form one complete replacement list in argument order. Repeated
Repeated `--dry-run` occurrences collapse to their one idempotent Boolean choice.
No command-specific flag uses last-wins or precedence behavior.
The shared global flags keep their shared spelling, values, defaults, repetition,
composition, terminal behavior, and errors; this command does not change those
rules or add another global-flag precedence rule.

### Metadata flag effects

Each supplied metadata flag changes only its named destination field. The exact
patch rules are defined in [Metadata Patch](#metadata-patch).

### Template and write-policy effects

`--template` makes the body-completion decision in [Template Body Completion](#template-body-completion).
`--dry-run` uses the application boundaries in [Dry Run And Apply](#dry-run-and-apply).

## Target Source

The source reference must resolve to one existing routed Markdown base source.
It may identify an ordinary routed Markdown file or a recognized entrypoint.
The Loader, `SKILL.md`, a non-Markdown resource, a detached unsupported file,
and an orphan overwrite are invalid targets.

Selecting a valid base ID, base path, or overwrite path resolves the complete
logical source under the shared source-reference contract. `route update`
changes the base file only. It never changes the overwrite companion.

An existing canonical or compatibility entrypoint is updated in place. The
command does not rename it, create a canonical sibling, or treat compatibility
spelling as permission to migrate the route.

Canonical entrypoint authoring uses `_{folder-name}.md`. The new CLI recognizes
these existing compatibility entrypoint filenames in their containing-folder
context: `index.md`, `_index.md`, `references.md`, and `_references.md`. When
exactly one recognized entrypoint exists, its automatic ID is the containing
folder ID, its actual filename is preserved, and generated navigation uses its
actual relative path. More than one recognized entrypoint makes the route
structurally ambiguous for this mutation.

The target must have one safely parseable scoped Open Forge frontmatter block.
One narrow enrichment exception applies when that block contains an existing,
safely parsed canonical `open-forge: {}` mapping: a request that supplies a complete
non-empty description and at least one valid tag may add those fields, with a
responsibility following the existing set and exact-empty removal rules. A description-only
or tags-only request against the empty mapping is not accepted. Outside that
exception, the existing full intended-metadata validation remains in force.
The command does not invent a missing frontmatter ownership boundary or guess
through malformed or duplicate metadata. A malformed boundary blocks before
writes.

The complete intended source must remain valid under the contract for its source
type. Updating an entrypoint therefore preserves or establishes the required
title, Axioms meaning, `Entries` section, and generated region under the
current [Routed Markdown Representation](../../../../framework/markdown/routes.md)
contract.

## Final-Leaf Safety Boundary

Every file leaf that `route update` would replace, including the selected source
and any generated-navigation target, has a caller-visible no-follow observation
of its immediate final filesystem component. A filesystem link or reparse point
at that final leaf, including a relative file link that is an exact Workspace
Library projection, is separately owned and unsafe for ordinary Route Update
mutation. A special final leaf has the same boundary.
The command keeps its existing `blocked` target-safety result, identifies the
affected path, and performs no effect.

This physical-leaf boundary does not depend on Library registration state. A missing,
malformed, stale, or otherwise unreadable Library record neither makes the
final leaf ordinary nor grants Route Update mutation authority. Route Update
never follows a final filesystem leaf to write the source or generated bytes.
The guard concerns the final component addressed by each file effect; ordinary
directory-ancestry rules remain defined by the filesystem contract.

## Metadata Patch

Each supplied field replaces only that field in the destination's `open-forge`
metadata:

- `--description <text>` replaces `description`.
- Repeated `--tag=<tag>` values replace the complete tag list in argument order.
- `--responsibility <text>` adds or replaces `responsibility`.
- `--responsibility ""` removes the `responsibility` key.

Omitted supported fields remain unchanged. There is no default description,
responsibility, or tag list.

An empty or whitespace-only description is invalid. A whitespace-only
responsibility is invalid. Each tag must follow canonical tag syntax and omit
the `#` prefix. Empty tags, duplicate exact tags, and a supplied empty tag set
are invalid. `description` and tags cannot be removed because the source must
retain the metadata required for indexing.

For the empty-mapping enrichment exception only, the accepted patch is a
bounded exact-span insertion of the complete supplied description and tag list,
plus responsibility when supplied. It preserves unrelated YAML, scoped fields,
the Markdown body, line endings, and encoding. It does not reserialize the
whole document or permit arbitrary non-empty flow mappings, aliases, duplicate
ownership maps, or other ambiguous layouts. Partial enrichment is not a new
general update mode: a request that does not supply the complete description and
tag set is rejected, and all existing required-metadata validation remains.

The command preserves unrelated top-level frontmatter and unsupported scoped
metadata when it can do so safely. It never deletes or reinterprets an unknown
field merely because the current canonical writer would not create it. If safe
preservation cannot be established, the update blocks. Both block-mapping
updates and canonical empty-map enrichment use the accepted parser and canonical
field emitter with bounded exact-span patches. Neither rewrites the whole
document. All command-local mechanics
must be proven at Gate 5.

The command validates field syntax and presence. It does not derive, summarize,
correct, or judge semantic values from filenames, bodies, Templates, generated
entries, or overwrite companions. A later `doctor` operation may report
meaning-quality diagnostics separately.

## Template Body Completion

`--template` accepts the automatic ID or exact `.agents/...` path of one existing
routed Markdown Template. Selection, collision, and overwrite behavior match
the [route create Interface Contract](../create/interface.md).

The selected source must be ordinary routed Markdown whose base frontmatter
contains the exact canonical `Template` tag. The tag provides a deterministic
source classification for this operation. It does not create a Templates root
route, activate another primitive contract, or grant the source authority over
the destination. The selected source's loaded route and content still define
how the Template should be used.

A Template with an overwrite companion blocks because one-file instantiation has
no accepted rule for collapsing two authored layers into one body. Create or
select a standalone Template with the intended body instead. The command strips
the selected Template's own frontmatter and considers only its body as
destination starting content. It does not substitute placeholders or store
Template provenance.

Template frontmatter never changes destination metadata. Only explicit metadata
flags patch the destination.

The target body is the bytes after its frontmatter closing delimiter:

- When it contains only whitespace, the command replaces that whitespace with
  the Template body using canonical frontmatter-to-body separation.
- When it contains any authored non-whitespace byte, the command preserves the
  complete body byte-for-byte and does not apply the Template body.

This rule does not compare headings or attempt to decide whether existing prose
is finished. Any authored body is enough to protect it.

Metadata patches still apply when an authored body prevents Template copying. If
`--template` is supplied, the target has authored non-whitespace body content, and
all required facts and safety conditions are complete, the result is the one
finite `completed-with-warnings` condition. Metadata and generated-navigation effects still
apply when requested, and the command safely previews or applies and verifies
those effects. The authored body remains byte-for-byte unchanged.

When `--template` is the only requested input and the target body already has
authored content, the result retains verified byte-level no-op and effect facts
and explains why the Template body was not applied, but its semantic status is
`completed-with-warnings` because the explicit Template intent remains unapplied. Dry-run has
the same status and observation. It is not a failure and does not claim that the
existing body matches the Template. Invalid Template input, a Template overwrite
companion, a malformed or unsafe target, an unavailable or mismatched recovery
bundle, or an invalid
generated boundary keeps its existing `invalid-input`, `blocked`, or `failed` result;
none becomes `completed-with-warnings`. Other successful changes and no-ops are `completed`.

For an entrypoint target with a frontmatter-only body, the selected Template
body must produce a complete valid entrypoint representation after insertion.
For an ordinary routed file, it must produce valid Markdown under the selected
route's applicable contracts. A Template that cannot produce a valid intended
target blocks before writes.

The copied body becomes independently maintained destination content. Later
Template changes never update it. The destination stores no continuing Template
receipt, origin field, update relationship, or hidden ownership marker.

## Body And Generated Preservation

When the target already has an authored body, every body byte remains unchanged.
This includes titles, prose, links, whitespace, line endings, generated section headings,
and generated interiors in the target itself.

Generated navigation may still change in a different bounded region:

- Changing a source description or tags updates its exposing parent's generated
  entry.
- Completing a frontmatter-only entrypoint from a valid Template may establish
  its own generated region and update its exposing parent.
- Responsibility-only and ordinary-body-only changes do not affect generated
  entry text.

Automatic generated effects use the complete [Index Interface Contract](../../index-candidate/interface.md)
projection, ordering, generated-boundary, verification, and recovery behavior.
They are planned against the hypothetical post-update workspace and belong to
the same parent plan, dry run, application, and result. The command never starts
a hidden `index` subprocess.

If a required generated ownership boundary, sibling projection, or route
relationship is ambiguous, the complete update blocks before writes. The command
does not apply metadata first and leave navigation stale.

## Existing State And No-Ops

The planner compares the complete intended target and automatic generated effects
with current bytes.

- A supplied field already holding the exact intended value is unchanged.
- Removing an already absent responsibility is unchanged.
- A supplied Template body is protected rather than compared or recopied when
  authored target content is present.
- An unchanged generated projection creates no effect.

A request whose complete intended state already exists returns a verified no-op,
and the command never rewrites unchanged bytes merely to normalize formatting or
timestamps. The one exception to the ordinary `completed` semantic result is a
supplied Template intentionally protected by authored body content: its
byte-level no-op or effect facts remain verified, but its semantic status is
`completed-with-warnings` as defined under [Semantic Results](#semantic-results).

## Planning And Effects

The operation follows the accepted typed mutation flow:

```text
validated target, field patch, and optional Template
  -> current target, route, and Template facts
  -> complete intended destination bytes
  -> generated-navigation projection
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan contains at most one destination replacement plus the
dependency-minimal generated-navigation replacements required by the metadata or
route representation change. Compatible changes to the same physical file
coalesce into one exact replacement.

One blocker prevents every effect. The command has no partial-application or
best-effort mode. It preserves siblings, overwrite companions, authored body
content, compatibility filenames, and bytes outside planned generated interiors.

## Dry Run And Apply

`--dry-run` and application use the same normalized request, target and Template
facts, intended bytes, generated projection, planner, expected-state facts,
preflight, and semantic status. Dry-run shows every exact intended destination
and generated effect, including the full effect evidence for metadata changes
and the body-protection observation, then writes nothing.

When the final-leaf safety boundary fails, dry-run and application retain the
same existing `blocked` target-safety result and produce no effect.

Omitting `--dry-run` selects application. The explicit target and patch flags
confirm only the described field changes, eligible Template body changes, and
generated region changes. The command does not prompt and does not accept
`--yes`.

A verified no-op has no affected mutation path and needs no recovery bundle. An
actual update checks only paths the complete plan would replace. The command does
not inspect or report repository state.

When the plan contains an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), orchestration selects only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. It prepares exactly one immutable ZIP bundle
outside the workspace. An operation containing only Create effects or
no-ops creates no bundle. Its source-generated
versioned `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to the deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires that preparation for every existing-target effect and
performs one final effect per target. All preparation completes before the first
target effect; unknown, malformed, mismatched, or colliding bundles block.

Immediately before application, the command rechecks every target, source,
Template, route, and generated fact. It applies complete planned bytes through
safe same-directory replacement, verifies each effect, then verifies the
requested fields, body-preservation or body-copy decision, routed validity, and
generated navigation.

When the one protected-Template completed-with-warnings condition applies, application still
completes and verifies every requested metadata and generated-navigation effect.
When no replacement effect is needed, it retains the verified byte-level no-op
facts without preparing a bundle. An unexpected failure after a write is
`failed`, not `completed-with-warnings`.

A handled failure stops new effects and never restores, rolls back, or
compensates for an earlier effect. An unexpected concurrent edit is preserved
and reported rather than overwritten. After final verification, delete only the
positively recognized bundle created by this operation. `Deleted`/`Removed`
permits normal completion. `Failed`/positively observed `Retained` keeps target
effects successful and produces `completed-with-warnings`, the
exact residual path, and cleanup guidance. `Failed`/`Unknown` produces `failed`
and reports an exact expected path only when the
deletion result provides one. When `Failed`/positively observed `Retained`
recovery warning coexists with the protected-Template condition, cleanup
guidance owns the single next action; the
Template-protection facts remain visible evidence. Before post-verification
deletion begins, a handled application, verification, or cancellation outcome
reports the actual residual draft or final path; a valid final remains when
preparation completed. A closed final ZIP may remain after abrupt process
termination, without an executable crash or power-loss guarantee. Recovery provenance does not classify current
target state. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Route update is non-interactive; it does not ask for confirmation and has no --automatic flag.

The catalogue text by detail level is:

`minimal`:

```text
Updated memory/emerging/ideas/pricing/tiers
  description: "Pricing tier options" -> "Pricing tiers and their tradeoffs"
  tags: #Idea -> #Idea #Pricing
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
```

`minimal`, responsibility removed: `  responsibility: "Define the tiers" -> (removed)`.

`minimal`, Template protected:

```text
Updated memory/emerging/ideas/pricing/tiers, but the Template body was not copied.
  description: "Pricing tier options" -> "Pricing tiers and their tradeoffs"
  The file already has content, which was kept. The Template templates/memory/idea was not copied.
```

`standard` adds `Workspace:`, the path, the Template path when used, and
`<path>  frontmatter rewritten` as the effect row.

`full` adds the before and after hashes and the full frontmatter before and
after.

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

The command is exactly route update; data follows the catalogue:

| Level    | `data`                                                                                                                   |
| -------- | ------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, target { id, path }, changes: [ { field, before, after } ], template { id, path, applied } \| null, listedIn }` |
| standard | same                                                                                                                     |
| full     | + `frontmatterBefore`, `frontmatterAfter`, per effect `before`, `after`                                                  |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                          | Headline                                                        | Exit | Stream |
| ----------------------- | --------------------------------------------- | --------------------------------------------------------------- | ---: | ------ |
| completed               | fields changed                                | `Updated <id>`                                                  |    0 | stdout |
| completed               | already at the requested values               | `<id> already has these values. Nothing to do.`                 |    0 | stdout |
| completed (dry run)     | planned                                       | `Would update <id>`                                             |    0 | stdout |
| completed-with-warnings | Template body not copied (file has content)   | `Updated <id>, but the Template body was not copied.`           |    2 | stdout |
| completed-with-warnings | recovery bundle retained                      | + family row                                                    |    2 | stdout |
| incomplete              | source or Template unreadable                 | `<id> could not be updated: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | no patch, bad value, bad Template, bad target | `Cannot update <id>: <problem>.`                                |    4 | stderr |
| blocked                 | unsafe frontmatter, ambiguity, lock           | `Cannot update <id>: <reason>.`                                 |    5 | stderr |
| failed                  | after effects                                 | `Route update stopped after <n> of <m> changes.`                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                      | `Route update was cancelled. Nothing was changed.`              |  130 | stderr |



## Errors And Boundaries

The finding catalogue is:

| Code                                      | Severity | Family                      | Message                                                                                                                                             | Next                                |
| ----------------------------------------- | -------- | --------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| route-update.invalid-input                | error    | invalid-input               |                                                                                                                                                     |                                     |
| route-update.invalid-target               | error    | unknown-source              | `<ref> is not a routed source that can be updated.` when known but ineligible                                                                       | `open-forge route list --depth=all` |
| route-update.invalid-patch                | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.invalid-patch`). | corrected command                   |
| route-update.invalid-template             | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.invalid-template`).                                                                                                        | `open-forge find --tag Template`    |
| route-update.workspace-unavailable        | error    | workspace-unavailable       |                                                                                                                                                     |                                     |
| route-update.workspace-unsafe             | error    | workspace-unsafe            |                                                                                                                                                     |                                     |
| route-update.target-unsafe                | error    | target-unsafe               |                                                                                                                                                     |                                     |
| route-update.route-ambiguous              | error    | route-ambiguous             |                                                                                                                                                     |                                     |
| route-update.identity-collision           | error    | identity-collision          | (the prompt resolves it in a terminal)                                                                                                              |                                     |
| route-update.frontmatter-unsafe           | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.frontmatter-unsafe`).                                                                                   | fix by hand                         |
| route-update.metadata-preservation-unsafe | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.metadata-preservation-unsafe`).                                                                  | fix by hand                         |
| route-update.template-unsafe              | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.template-unsafe`).                                                                                                  | none                                |
| route-update.generated-region-unsafe      | error    | generated-region-unsafe     |                                                                                                                                                     |                                     |
| route-update.workspace-lock-unavailable   | error    | workspace-lock-unavailable  |                                                                                                                                                     |                                     |
| route-update.target-changed               | error    | target-changed              |                                                                                                                                                     |                                     |
| route-update.recovery-conflict            | error    | recovery-conflict           |                                                                                                                                                     |                                     |
| route-update.template-body-protected      | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.template-body-protected`).                                                                  | none                                |
| route-update.inspection-incomplete        | warning  | inspection-incomplete       |                                                                                                                                                     |                                     |
| route-update.projection-incomplete        | warning  | projection-unavailable      |                                                                                                                                                     |                                     |
| route-update.template-unavailable         | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Update/Shared/Wording/RouteUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-update.template-unavailable`).                                                                                                             | none                                |
| route-update.recovery-unavailable         | warning  | recovery-unavailable        |                                                                                                                                                     |                                     |
| route-update.recovery-artifact-retained   | warning  | recovery-artifact-retained  |                                                                                                                                                     |                                     |
| route-update.target-changed-during-apply  | error    | target-changed-during-apply |                                                                                                                                                     |                                     |
| route-update.write-failed                 | error    | write-failed                |                                                                                                                                                     |                                     |
| route-update.verification-failed          | error    | verification-failed         |                                                                                                                                                     |                                     |
| route-update.recovery-failed              | error    | recovery-failed             |                                                                                                                                                     |                                     |
| route-update.operation-failed             | error    | operation-failed            |                                                                                                                                                     |                                     |
| route-update.interrupted                  | error    | interrupted                 |                                                                                                                                                     |                                     |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`fieldsChanged`, `sectionsUpdated`.

## Scenarios

`description-changed`, `tags-replaced`, `responsibility-removed`,
`template-applied`, `template-body-protected` (warnings), `no-change`,
`dry-run`, `no-patch` (invalid), `unknown-source`, `ambiguous-id-prompt`,
`lock-held`, `write-failed-partial`, `cancelled`.

Prompt rules from the catalogue:

Select when the ID matches several files (see 04).

## Representative Transcripts

### completed

~~~text
Updated memory/project-alpha/overview
Workspace: <workspace>
  description: "Before overview" -> "After overview"
  Entry updated in .agents/memory/project-alpha/_project-alpha.md
~~~

### completed-with-warnings

~~~text
Updated memory/project-alpha/overview, but the Template body was not copied.
Workspace: <workspace>
  description: "Before overview" -> "After overview"
  The file already has content, which was kept. The Template templates/route was not copied.
  Entry updated in .agents/memory/project-alpha/_project-alpha.md
~~~

### incomplete

~~~text
memory/project-alpha/overview could not be updated: <limitation>. Nothing was changed.
~~~

### invalid-input

~~~text
Cannot update memory/project-alpha/overview: Nothing to update: pass --description, --responsibility, --tag or --template.
Next: open-forge route update memory/project-alpha/overview --description "<one sentence>"
~~~

### blocked

~~~text
Cannot update docs/guide: docs/guide could match more than one route.
Workspace: <workspace>
Next: open-forge route list --depth=all
~~~

### failed

~~~text
Route update stopped after 1 of 2 changes.
Workspace: <workspace>
  description: "Before overview" -> "After overview"
Next: open-forge route update --detail debug
~~~

### cancelled

~~~text
Route update was cancelled. Nothing was changed.
Workspace: <workspace>
Next: open-forge route update
~~~

## Related Current Sources

- [route update Behavior Contract](behavior.md)
- [route update Command Contract Set](_update.md)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [Context Interface Contract](../../context/interface.md)
- [Status Interface Contract](../../status/interface.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [Route Init Interface Contract](../init/interface.md)
- [Route Create Interface Contract](../create/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Contract Document Templates](../../../../../../../templates/cli/documents/_documents.md)
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

CLI help syntax: [`route.update.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Update/RouteUpdateText.cs).

<!-- @OpenForgeTextRef route.update.help.syntax -->
