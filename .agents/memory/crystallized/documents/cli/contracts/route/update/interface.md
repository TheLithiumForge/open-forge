---
open-forge:
  description: Accepted current public interface for patching one existing routed Markdown source and completing a protected Template body when eligible
  responsibility: Define what a caller may enter and observe from `route update` without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, Interface, Metadata, Template, Mutation, CurrentTruth]
---

# route update Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route update`. The command does not ship yet;
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
retains `attention` when its explicit body intent remains unapplied; all other
successful repeated updates are `complete` verified no-ops.

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
`--workspace`, `--json`, `--view`, `--verbose`, `--help`, and `--version`. All
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
The command may add one missing supported field, but it does not invent a
missing frontmatter ownership boundary or guess through malformed or duplicate
metadata. A malformed boundary blocks before writes.

The complete intended source must remain valid under the contract for its source
type. Updating an entrypoint therefore preserves or establishes the required
title, Axioms meaning, final `Entries` section, and generated region under the
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

This boundary does not consult `.agents/open-forge.libraries.json`. A missing,
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

The command preserves unrelated top-level frontmatter and unsupported scoped
metadata when it can do so safely. It never deletes or reinterprets an unknown
field merely because the current canonical writer would not create it. If safe
preservation cannot be established, the update blocks. YAML parsing and
canonical serialization use the Architecture's accepted source-generated path;
their command-local mechanics must be proven at Gate 5.

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
finite `attention` condition. Metadata and generated-navigation effects still
apply when requested, and the command safely previews or applies and verifies
those effects. The authored body remains byte-for-byte unchanged.

When `--template` is the only requested input and the target body already has
authored content, the result retains verified byte-level no-op and effect facts
and explains why the Template body was not applied, but its semantic status is
`attention` because the explicit Template intent remains unapplied. Dry-run has
the same status and observation. It is not a failure and does not claim that the
existing body matches the Template. Invalid Template input, a Template overwrite
companion, a malformed or unsafe target, an unavailable or mismatched recovery
bundle, or an invalid
generated boundary keeps its existing `invalid`, `blocked`, or `failed` result;
none becomes `attention`. Other successful changes and no-ops are `complete`.

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
This includes titles, prose, links, whitespace, line endings, generated markers,
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
timestamps. The one exception to the ordinary `complete` semantic result is a
supplied Template intentionally protected by authored body content: its
byte-level no-op or effect facts remain verified, but its semantic status is
`attention` as defined under [Semantic Results](#semantic-results).

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
schema-v1 `manifest.json` and streamed ordinal payload entries record
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

When the one protected-Template attention condition applies, application still
completes and verifies every requested metadata and generated-navigation effect.
When no replacement effect is needed, it retains the verified byte-level no-op
facts without preparing a bundle. An unexpected failure after a write is
`failed`, not `attention`.

A handled failure stops new effects and never restores, rolls back, or
compensates for an earlier effect. An unexpected concurrent edit is preserved
and reported rather than overwritten. After final verification, delete only the
positively recognized bundle created by this operation. `Deleted`/`Removed`
permits normal completion. `Failed`/positively observed `Retained` keeps target
effects successful and produces `attention`, the
exact residual path, and cleanup guidance. `Failed`/`Unknown` produces `failed`
and reports an exact expected path only when the
deletion result provides one. When `Failed`/positively observed `Retained`
recovery attention coexists with the protected-Template condition, cleanup
guidance owns the single next action; the
Template-protection facts remain visible evidence. Before post-verification
deletion begins, a handled application, verification, or cancellation outcome
reports the actual residual draft or final path; a valid final remains when
preparation completed. A closed final ZIP may remain after abrupt process
termination, without an executable crash or power-loss guarantee. Recovery provenance does not classify current
target state. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

Both views start with the outcome, `Status`, `Workspace`, and `Selected by`,
followed by command identity and mode. Expanded remains the default. Compact
uses the same typed result and retains completeness, safety, every affected and
unchanged path, every finding with its status, cause, stable code and available
target, and verification and recovery facts. A failure heading reports the
semantic outcome; it does not claim that no mutation occurred. Effect outcomes
and residual state describe any partial work.

Each required `Next:` line contains the actual command from the result, once.
Expanded adds its reason on the following line; compact omits that explanation.
Complete results have no Next action. Other statuses retain at most one direct
correction or recovery action supplied by the operation. Rendering does not
invent advice, change status, or select another action.

Primary human `complete`, `attention`, and `incomplete` results use stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. Each result stays together on its assigned stream. Separate bounded
diagnostics use stderr. JSON remains one complete structured result on stdout.

Both views retain target ID/path, selected field state, Template identity and
body decision, every changed and unchanged path and generated effect. Each
requested description, responsibility or tags field shows its state and
before/expected values, including unchanged and unresolved patches. `absent`
means an established missing value or requested removal; `unavailable` means a
value could not be established. An empty tags array is `[]`, not unavailable.
Requested or expected values do not imply a completed write.

Every effect retains exact before/expected values and preview rows in both
views, including compact apply. Dry-run output retains every exact planned
effect and affected path and prints `No files changed (--dry-run).`

Illustrative field rows for a selected patch:

```text
Description (changed): "Old description" -> "New description"
Responsibility (unchanged): absent -> absent
Tags (changed): ["Before"] -> ["After"]
```

Success headings distinguish updated content, an up-to-date no-op and a preview.
Protected-Template attention says `The routed source requires attention.` and
shows `Template body not applied: the target already has authored body content.`
once, with its finding status, code and available target. It never proposes
overwriting authored body content. When that condition owns the Next action,
`Next: open-forge route update` is followed in expanded by the operation's reason
to review the authored body. Retained recovery can instead own the single Next
command, while Template protection remains visible as a finding.

## Structured Output

`--json` returns the complete typed result used by human rendering. It never
prompts and never reruns planning, application, or verification.

The structured result exposes:

- Workspace and selection method.
- Requested and resolved target ID, base path, compatibility form, and overwrite
  layers.
- Requested field patches and prior, intended, changed, or unchanged state.
- Responsibility addition, replacement, removal, or unchanged state.
- Optional Template ID, path, classification, and body decision.
- Authored-body preservation or Template-body copy evidence.
- Intended destination and generated-region effects.
- Completeness and safety state, including bounded observations and availability
  conditions.
- Dry-run, application, verification, and recovery-bundle facts.
- Changed, unchanged, retained, and residual targets.
- Exact preview effects when dry-run is selected, semantic status, and at most
  one required `Next:` action when applicable.

Exact field names, schema versioning, and compatibility rules are defined by the
[Shared Result Coordinates](../../shared/result-coordinates/interface.md).

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                                                                                                                    |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A dry-run established the complete safe plan, or application and final verification completed, including ordinary changes and verified no-ops, with no protected-Template `attention` condition. Planned changes alone do not create `attention`.                                                                                                                                                          |
| `attention`   | The complete protected-Template condition applies, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`. The Template condition preserves its existing dry-run and application meaning. `Failed`/`Retained` recovery keeps target effects successful and reports the exact residual path with cleanup guidance. Human output says `requires attention`. |
| `incomplete`  | Safe facts are available, but required inspection or planning coverage cannot complete. No write begins.                                                                                                                                                                                                                                                                                                   |
| `invalid`     | Command input, field value, Template reference, flag repetition or use, or target kind does not follow this interface.                                                                                                                                                                                                                                                                                     |
| `blocked`     | A valid request cannot establish or apply one safe complete update plan because an unsafe or ambiguous boundary remains. No mutation begins.                                                                                                                                                                                                                                                               |
| `failed`      | A post-write unexpected failure, application or verification failure after effects begin, or post-verification recovery deletion `Failed`/`Unknown` prevents the update from completing.                                                                                                                                                                                                                   |
| `interrupted` | The caller cancelled before completion and no unexpected application or verification failure changes the result.                                                                                                                                                                                                                                                                                           |

The shared numeric process-status mapping is defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md).

For ordinary operation conditions, status precedence is `blocked` > `incomplete`

> `attention` > `complete`. Invalid input stops before operation resolution and
> forms `invalid`. Failed and interrupted results retain their event meaning.

## Errors

The command blocks or rejects:

- No requested metadata or Template operation.
- A repeated singleton `--description`, `--responsibility`, or `--template`,
  including repetition with an equal value.
- A target that is not one existing routed Markdown base source.
- Ambiguous source identity, route meaning, or entrypoint structure.
- Missing, malformed, duplicate, or unsafe scoped frontmatter.
- A field patch that would remove required metadata or create invalid syntax.
- A Template reference that does not resolve to one valid routed Template.
- A Template with an overwrite companion.
- A Template body that cannot produce a valid frontmatter-only target.
- A filesystem link or reparse point at any planned final file leaf, including
  an exact Workspace Library projection with or without a valid Library record;
  this is the existing `blocked` target-safety result and prevents every effect.
- An invalid generated ownership boundary or sibling projection.
- Unavailable or unsafe recovery-bundle storage is `incomplete`; a malformed,
  colliding, or mismatched recovery bundle is `blocked`.
- A changed source or destination that invalidates the plan.

Safe facts with unfinished required inspection or planning coverage form
`incomplete` and begin no write. Unsafe or ambiguous boundaries form `blocked`
unless an existing input rule makes them `invalid`; an unexpected failure after
a write forms `failed`. None of these conditions is the protected-Template
`attention` condition.

## Non-Goals

`route update` does not:

- Create a missing target or route chain.
- Replace, merge, normalize, or semantically edit an authored body.
- Apply a Template body over authored content.
- Copy Template frontmatter or retain Template ownership.
- Rename canonical or compatibility entrypoint files.
- Change an overwrite companion.
- Remove required description or tags.
- Infer metadata from a filename, body, Template, or generated entry.
- Repair arbitrary malformed Markdown, frontmatter, routes, links, or markers.
- Follow a final filesystem link or reparse point, write through a Workspace
  Library projection, or adopt or manage a Library record.
- Create a Git commit.

Use `route init` for missing route chains and `route create` for one missing
ordinary routed Markdown file.

## Scenarios

### Metadata update

```text
open-forge route update memory/crystallized/decisions/cache-policy \
  --description "Why the revised cache policy was chosen"
```

This patches only `description`, preserves omitted metadata and the complete
authored body, and updates generated navigation only when the intended parent
projection changes.

### Dry-run tag replacement

```text
open-forge route update memory/crystallized/decisions/cache-policy \
  --tag=Memory \
  --tag=Decision \
  --dry-run
```

This forms the same intended tag-list replacement and generated effects as
application, shows every exact planned diff, and writes nothing.

### Protected Template body

```text
open-forge route update memory/crystallized/decisions/cache-policy \
  --template templates/memory/decision
```

When the target already has any authored non-whitespace body byte, the Template
body is not applied and the result is `attention` once all required facts and
safety conditions are complete. With no metadata patch, byte-level no-op facts
remain verified and the body-protection observation is shown, but the semantic
status is still `attention`; it does not claim that the authored body matches the
Template. The same observation and status apply to dry-run.

The representative protected-Template, successful-application, and
successful-dry-run result blocks remain under [Human Output](#human-output).

## Verification

Gate 5 executable proof must cover:

- ID, base path, and overwrite-path target selection.
- Ordinary routed files, canonical entrypoints, each compatibility entrypoint
  name, unsupported source kinds, detached files, and ambiguous routes.
- Description replacement, tag-list replacement and ordering, responsibility
  addition, replacement, exact-empty removal, omitted fields, duplicates, and
  invalid values.
- Singleton rejection for repeated `--description`, `--responsibility`, and
  `--template` values, idempotent repetition of `--dry-run`, complete tag-list
  replacement, and unchanged shared-global
  repetition rules without last-wins or precedence behavior.
- Preservation of unrelated frontmatter and blocking when safe preservation is
  impossible.
- Template ID and exact-path selection, exact `Template` classification,
  collisions, blocked overwrite companions, invalid non-Templates, and orphan
  overwrites.
- Frontmatter-only targets with whitespace variants and exact Template body
  insertion.
- Authored bodies containing prose, headings, comments, markers, or only
  non-space whitespace, with byte-for-byte preservation.
- Template-only byte-level no-op facts with `attention` status when authored body
  exists, including the same observation in dry-run.
- Proof that Template frontmatter never changes destination metadata and no
  Template lifecycle state remains.
- Entrypoint Template completion that is valid and invalid for the intended route
  representation.
- Automatic generated effects for description and tag changes, no generated
  effect for responsibility-only changes, and intended-state planning.
- Final filesystem link and reparse-point leaves, including exact Workspace
  Library projections with and without a valid Library record, are reported as
  unsafe and block before effects; Route Update never writes through them.
- Complete dry-run output and no persistent dry-run effects.
- Dry-run and application parity for request, facts, intended bytes, generated
  projection, plan, preflight, status, full effects, and no-write behavior.
- Verified no-op behavior before recovery-bundle preparation.
- All seven statuses, including safe-but-incomplete coverage with no writes,
  unsafe or ambiguous blocked boundaries, post-write or `Failed`/`Unknown`
  recovery failed behavior, and the protected-Template and
  `Failed`/positively observed `Retained` recovery attention conditions. Planned
  changes alone must remain `complete`.
- Recovery-bundle readiness, exact-entry validation, all-before-first-effect
  preparation, typed post-verification deletion state/disposition facts,
  expected-state changes, safe replacement, final semantic verification,
  residual preservation, and rerun convergence without restoration or rollback.
- Human and structured results from one typed result, with complete/attention/
  incomplete human output on stdout, invalid/blocked/failed/interrupted human
  output on stderr, one JSON result for every status on stdout, bounded
  diagnostics on stderr, compact retention and next-action limits, and no
  suggestion to overwrite authored body content.

The proof must exercise the accepted CLI Architecture boundaries rather than
relying on source-level or managed-build claims. It must include the real parser,
filesystem, workspace lock, recovery, Native AOT, and package/process evidence
required by that Architecture.

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

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The compact result retains the complete command-owned result graph defined by
its structured schema, including every nullable value and ordered collection.
Its core already carries the facts needed to use the result. For mutation
commands this includes plans, exact previews, effects, permissions when
applicable, verification, findings and recovery. Rendering never asks a caller
to rerun a mutation to recover an omitted receipt.

No collection is truncated and no finding is filtered. Counts describe the
original operation. Both JSON views retain the same result facts.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
