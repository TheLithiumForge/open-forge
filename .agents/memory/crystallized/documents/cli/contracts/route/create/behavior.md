---
open-forge:
  description: Accepted current technology-neutral contract for deterministic route create resolution, effects, safety, recovery, and conformance
  responsibility: Define how route create resolves current facts, forms one complete result, applies bounded effects, and recovers safely
  tags: [Memory, Crystallized, CLI, Release, Command, Behavior, Route, Create, Template, Mutation, CurrentTruth]
---

# route create Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route create`. The command does not ship yet. Its
local implementation and complete executable proof are squash-integrated at
`19412d2`; replacement-CLI delivery remains pending.

The [Interface Contract](interface.md) defines the complete public
surface. This file defines only the deterministic, technology-neutral semantics,
effects, safety, recovery, and conformance behind that surface. It does not
choose implementation technology or another Technical Design. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the accepted
shared structured schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines source and runtime boundaries,
BCL-first filesystem structure, the workspace-lock boundary, and recovery
identity relationships.

The Interface Contract defines command-specific repetition, seven semantic
statuses, stream allocation, and compact-result retention; this Behavior
implements those accepted meanings without changing shared global-flag rules.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the exact shared result. The [CLI Architecture](../../../architecture.md)
defines filesystem, concurrency, and source boundaries; exact recovery-bundle
mechanics live in the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md).

## Operation Invariants

- For the same workspace bytes and explicit input, resolution selects the same
  target, forms the same intended destination and generated navigation, and
  returns the same semantic result. See [Purpose](interface.md#purpose).
- The operation creates one ordinary routed Markdown file below one existing
  routable folder. It does not initialize a missing route chain or create a
  parent folder. See [File Target](interface.md#file-target) and [Non-Goals](interface.md#non-goals).
- The destination has its own explicit metadata and independently maintained
  content. Template frontmatter, route identity, ownership, future changes,
  receipts, origin fields, update relationships, and hidden ownership markers
  do not transfer. See [Destination Metadata](interface.md#destination-metadata)
  and [Template Selection](interface.md#template-selection).
- The operation never overwrites an existing target. An identical complete
  result is a verified no-op; a different existing target blocks. See [Existing
  Target](interface.md#existing-target).
- The parent generated navigation is planned from the hypothetical post-create
  workspace and belongs to the same complete operation as destination creation.
  See [Generated Navigation](interface.md#generated-navigation).
- Planning establishes one complete safe plan before the first persistent
  effect. One blocker prevents every effect; there is no partial-application or
  best-effort mode. See [Planning And Effects](interface.md#planning-and-effects).
- A normal creation, valid Template instantiation, safe dry-run with planned
  changes, generated-navigation effect, or verified identical-target no-op is
  `complete`. `attention` is formed only after successful final verification
  when recovery deletion returns `Failed` and the recovery artifact is
  positively observed as `Retained`.

## Request Resolution

Request resolution establishes one explicit workspace, one ordinary-file target,
explicit destination metadata, and at most one valid Template before planning
can continue.

The resolver accepts one occurrence each of `--description`,
`--responsibility`, and `--template`. Any repeated occurrence is invalid, even
when its value is identical, and no last occurrence wins. `--tag` is a required
multi-value flag; it retains repeated tag order and rejects empty or duplicate
exact tags. It collapses repeated `--dry-run` occurrences to one idempotent
Boolean choice. Shared global flags retain their shared
repetition, composition, and terminal rules; this operation adds no precedence
or last-wins behavior and no wizard or automatic mode.

### Workspace and shared references

The operation uses the exact current working directory or the exact
`--workspace <path>` selected under the shared [Global CLI Flags](../../shared/global-flags/behavior.md)
contract. It does not search parent directories, substitute a Git root, infer a
workspace from the target, or use another `.agents` directory.

The [CLI Source References Behavior Contract](../../shared/source-references/behavior.md) determines whether an existing-source
reference is an automatic ID or an exact `.agents/...` path. It defines ID
segments, exact-path detection, quoting, containment, source identity, spaces,
Unicode, collisions, overwrite identity, and canonical reporting. `route create`
adds only the mapping from a target ID to one ordinary Markdown path described
in [File Target](interface.md#file-target). It does not guess another target
shape from filesystem coincidence.

The exact target path is normalized to the selected workspace's canonical
`.agents/...` reporting form and must remain lexically and physically inside
that workspace. A directory, Loader, recognized entrypoint, overwrite
companion, `SKILL.md`, non-Markdown resource, unsafe identity, path escape, or
other unsupported kind cannot become the ordinary-file target. Backslash input,
case compatibility, filesystem aliases, and exact physical identity follow the
accepted CLI Architecture boundary recorded in [Interface Status And
Authority](interface.md#status-and-authority).

### Parent route

The final parent folder is resolved only when it already contains exactly one
recognized entrypoint. Canonical and accepted compatibility entrypoint names
are recognized input; the existing filename remains the parent representation.
The command does not create, rename, normalize, or choose among multiple
recognized entrypoints.

If an ancestor or the final parent entrypoint is missing, the request is blocked
and the caller is directed to `route init`. An existing child entrypoint or
another source with the same route identity blocks creation. Exact path input
may disambiguate source selection, but it cannot make an ambiguous route valid
or permit two direct children whose identities cannot remain distinct.

An orphan overwrite companion at the intended base path is a blocking fact. The
operation does not adopt, delete, or reinterpret it. The [Overwrite
Customization](../../../../framework/routing/overwrites.md)
contract remains authoritative for the pair's identity and ownership boundary.

### Destination metadata

The resolver requires explicit `--description` and at least one `--tag`. It
rejects an empty or whitespace-only description, an empty tag, a duplicate exact
tag, a tag that violates canonical tag syntax, or a tag with a `#` prefix. It
retains repeated tag argument order.

It adds `responsibility` only for a non-empty value. It omits the field for the
exact empty value `--responsibility ""` and rejects a whitespace-only value.
There is no destination-removal flag because the destination does not yet
exist. The resolver validates syntax and presence only; it does not derive,
correct, summarize, or judge values from the target, parent, Template, body,
or another routed source. See [Destination Metadata](interface.md#destination-metadata)
and [Errors](interface.md#errors).

### Template resolution

When `--template` is omitted, no Template source is selected and the intended
destination is metadata-only unless its selected route or component contract
requires more content.

When `--template` is supplied, the resolver uses the shared existing-source
grammar and requires one existing ordinary routed Markdown source whose base
frontmatter contains the exact canonical `Template` tag. The tag is a
deterministic classification for this operation. It does not create a
Templates root route, activate another primitive contract, or grant authority
over the destination. The selected Template's loaded route and content still
define how the starting content is used.

An ID collision requires exact-path disambiguation. JSON and non-interactive use
never prompt. An entrypoint, Loader, overwrite companion, ordinary routed file
without the exact classification, orphan overwrite, ambiguous overwrite, or
Template with an overwrite companion cannot be selected. A Template with an
overwrite companion blocks because one-file instantiation has no accepted rule
for collapsing two authored layers into one body.

The resolver removes the selected Template source's own frontmatter and copies
only its body. It performs no semantic placeholder substitution, so visible
Template prompts remain in the copied body. The resolver does not inspect
placeholder text or infer authoring quality. It does not copy Template
frontmatter into destination metadata and retains no receipt, origin, update
relationship, or hidden ownership marker. This is the command-local use of the
[Framework Templates](../../../../framework/primitives/templates.md)
one-time instantiation contract, not a route-family shared Template contract.

## Current Facts And Coverage

The source universe is the selected workspace and the bounded route and source
facts needed to prove one complete creation plan:

- The requested target reference, its resolved target ID and canonical ordinary
  Markdown path, its parent folders, containment, and physical identity.
- Every folder relationship needed to establish the final parent entrypoint,
  including canonical or compatibility representation, missing or duplicate
  entrypoints, child-entrypoint collisions, route-identity collisions, and
  orphan overwrite evidence.
- The current target path, any existing bytes or unsupported path kind, and the
  current explicit metadata and Template input used to form intended bytes.
- The selected Template's base source, exact classification, frontmatter/body
  boundary, route, and overwrite relationship when a Template is supplied.
- The parent entrypoint's valid generated boundary and the authored metadata of
  every direct routed sibling needed by the complete parent projection.
- The current generated body as comparison input, without treating it as the
  source of route identity or metadata.
- Recovery-bundle storage, preparation, provenance, and collision facts, and
  expected-state facts for every planned existing replacement.

Filesystem topology and authored metadata, rather than current generated lines,
define the expected parent navigation. The complete [Index Behavior Contract](../../index-candidate/behavior.md)
projection derives the parent's direct routed children, validates the metadata
needed to represent them, produces canonical generated lines, orders them by
canonical containing-file-relative destination using ordinal comparison, and
compares that expected body with the current marker-bounded body. The parent
projection uses the new destination's authored description and tags.

Fact coverage is complete only when the parent marker boundary, sibling
metadata, target identity, Template identity, containment, and expected state
are safe to use. If safe facts are available but required inspection or
planning coverage cannot complete, result formation is `incomplete` and no
write begins. The operation does not use incomplete coverage to create a
partial plan. Missing or ambiguous parent markers, invalid sibling metadata,
unsafe destinations, unsupported identities, orphan or ambiguous overwrites,
or another unsafe or ambiguous safety or authority fact make the complete
request `blocked` before writes. The operation does not invent fallback
metadata, omit a direct child, repair a marker, or continue with unsafe facts.

The operation resolves these facts from the current workspace before planning
and rechecks every target, source, Template, route, and collision fact
immediately before application. Parser, serialization, filesystem, and
physical-identity mechanics follow the accepted CLI Architecture.

## Selection And Result Formation

Selection forms one complete intended destination before it forms generated
effects:

- The destination frontmatter contains only the explicit destination metadata
  required by [Destination Metadata](interface.md#destination-metadata).
- Without a Template, the destination contains only that canonical frontmatter
  and the canonical trailing line ending, unless the selected route or
  component contract requires more content.
- With a Template, the destination contains the explicit destination
  frontmatter followed by the copied Template body after the Template
  frontmatter has been removed. Placeholder substitution, Template metadata,
  and continuing Template lifecycle state are absent.
- The expected parent generated region is then formed from the hypothetical
  post-create workspace through the complete Index projection. It exposes the
  new direct routed file from its description and tags and changes no authored
  bytes outside the bounded generated interior.

The complete intended destination and required generated navigation are
compared with current bytes. If both already match, result formation produces a
verified no-op. That no-op has no affected mutation path and needs no bundle. An
existing target whose intended bytes differ is
blocked and directs the caller to `route update` or an explicit future
replacement operation. An unsupported kind, unsafe identity, or ambiguous
route relationship is blocked rather than adopted or overwritten. See [Existing
Target](interface.md#existing-target), [Dry Run And Apply](interface.md#dry-run-and-apply),
and [Errors](interface.md#errors).

The operation returns one typed result. It forms the public semantic result
according to [Semantic Results](interface.md#semantic-results):

- `complete` is formed when dry-run establishes the complete safe plan, or when
  application and final verification complete, including normal creation, valid
  Template instantiation, generated-navigation effects, and an identical-target
  no-op. A safe dry-run with planned changes is also `complete`.
- `attention` is formed only when post-verification recovery deletion returns
  `Failed` with positively observed disposition `Retained`. Planned changes,
  valid Template prompts, generated-navigation effects, and authoring-quality
  questions do not form it.
- `incomplete` is formed when safe facts are available but required inspection
  or planning coverage cannot complete. No mutation begins.
- `invalid` is formed for command input, metadata, Template reference, flag use,
  or target shape that does not follow the Interface.
- `blocked` is formed when a valid request cannot establish or apply one safe
  complete creation plan because safety or authority is unsafe or ambiguous.
  No mutation begins.
- `failed` is formed for an unexpected application or verification failure after
  a persistent effect begins, or post-verification recovery deletion
  `Failed`/`Unknown`. `Failed`/positively observed `Retained` is the distinct
  recovery `attention` case.
- `interrupted` is formed when the caller cancels before completion and no
  unexpected application or verification failure changes the result.

The result retains workspace and selection facts, target and parent identity,
metadata, Template evidence, intended and generated effects, dry-run,
recovery-bundle and application facts, verification, bundle provenance, and
retained partial-state facts, changed and residual targets, coverage observations
and availability conditions, semantic status,
completeness and safety state, and at most one required `Next:` action when
applicable. It does not contain a diagnosis or recommendation. Human and
structured renderers consume this one result and do not rerun resolution,
planning, application, or verification.

For ordinary operation conditions, status precedence is
`blocked` > `incomplete` > `attention` > `complete`. Invalid input stops before
operation resolution and forms `invalid`. Failed and interrupted results retain
their event meaning.

## Effects

The operation follows one visible typed mutation flow:

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

The complete plan contains at most one new routed file plus the
dependency-minimal generated-navigation changes required to expose it. It
contains no parent-directory creation, Template semantic adaptation, sibling
formatting, overwrite-companion change, or authored change outside planned
generated interiors. A blocker prevents every effect.

### Generated projection

The parent mutation first establishes the complete intended destination. It then
projects the parent generated `Entries` body against that hypothetical
post-create workspace before any persistent effect. The projection uses the
complete [Index Behavior Contract](../../index-candidate/behavior.md) ordering, generated-boundary,
verification, and recovery behavior. Generated effects are dependency-minimal,
part of the same parent plan, and never invoke a hidden `index` subprocess.

The complete plan blocks before writes when the parent marker boundary is
missing, ambiguous, duplicate, nested, reversed, malformed, or otherwise
unsafe; when a direct sibling lacks valid metadata; when a destination is
unsafe; or when another Index projection blocker exists. It never creates a
file that its parent cannot safely index.

Every planned generated replacement preserves its marker tokens and every byte
outside the bounded generated interior. Authored sources and overwrite
companions remain unchanged unless a planned generated interior is the only
machine-owned region affected.

### Dry-run and application effects

Dry-run and application use the same resolved request, current facts, intended
destination bytes, generated projection, ordered planner, expected-state facts,
preflight, and status formation. Dry-run produces the complete new file content,
generated-navigation effects, and every exact existing-file bounded diff as
result evidence, then stops before persistent effects. It creates no
destination, replacement, recovery-bundle, or formatting effect. A safely established
plan with changes is `complete`; planned changes do not create `attention`.
Application uses the same complete plan and applies only the new destination and
planned generated-region replacements.

Omitting `--dry-run` is explicit application authority for the intended target,
metadata, optional Template body, and planned generated interiors. The command
does not prompt and does not accept `--yes`. The new destination is created only
at an unoccupied safe path; it never overwrites old bytes. An existing generated
region is replaced only inside its established machine-owned boundary.

Compatible changes to one physical generated target are one planned exact
replacement, not competing writes. Generated effects depend on the authored
destination facts and are applied as part of the same parent mutation. A
generated planning failure blocks before the first parent write. Before
post-verification deletion begins, a generated application or verification
failure stops new effects, reports ordinary effect facts and the actual residual
draft or final path, and does not restore an earlier effect.

## Safety And Recovery

### Authority and recovery bundle

The explicit command, target, required metadata, and optional Template select
the intended creation. They do not grant overwrite, force, adoption, deletion,
ownership, marker-repair, or unrelated formatting authority. The command does
not inspect or report repository state. See [Dry Run And Apply](interface.md#dry-run-and-apply)
and [Non-Goals](interface.md#non-goals).

A verified no-op has no affected mutation path and needs no bundle. An actual
creation checks the new path for collision. If the plan contains an
existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
orchestration uses only
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
Create effects, including the new destination, have no payload entry. A
CreateNew draft is closed/reopened for semantic manifest, exact ordered entry,
length, hash, and payload-byte validation, moved within the same directory to
its deterministic final name, and reopened and verified. Only the valid final
ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. Every planned existing-target effect must match the preparation; all
preparation completes before the first target effect. Unknown, malformed,
mismatched, or colliding bundles block.

### Revalidation and verification

Immediately before application, the operation rechecks every target, source,
Template, route, collision, generated-boundary, and expected-state fact. It
applies complete planned bytes through the accepted safe replacement boundary,
verifies each effect, and then verifies destination identity, explicit metadata,
copied body or metadata-only state, parent route exposure, and complete
generated navigation. It never falls back to a weaker replacement after a
safety or identity check fails.

### Failure, interruption, and concurrency

Before post-verification deletion begins, a handled application, verification,
or cancellation outcome stops new effects and reports the actual residual draft
or final path; it does not restore, reverse, or compensate for an earlier effect.
A valid final remains when preparation completed. An unexpected concurrent edit
is preserved and reported as residual state rather than overwritten.

The requested creation remains `failed` when an unexpected application or
verification failure occurs after a persistent effect begins, even when handled
residual reporting succeeds. Cancellation before completion is `interrupted`
when no stronger failure remains. A closed final ZIP may remain after abrupt process
termination, without an executable crash or power-loss guarantee. Recovery
provenance does not classify current target state. After final verification,
`Deleted`/`Removed` permits normal completion. `Failed`/positively observed
`Retained` keeps target effects successful and produces `attention`, the exact residual path, and cleanup guidance.
`Failed`/`Unknown` produces `failed` and reports
an exact expected path only when the deletion result provides one. Cleanup owns
exact named final and draft deletion under its separate lease-bound contract. Rerunning
`route create` computes a fresh plan and never
replays a saved plan, receipt, journal, history, or progress record.

Expected-state revalidation and preservation of unexpected concurrent edits are
required safety properties. The persistent reusable zero-byte external workspace
lock below `LocalApplicationData/OpenForge/locks/v1` is held with one read/write
`FileShare.None` handle; it never receives metadata writes, deletion, or
truncation. The BCL-first filesystem boundary is defined by the [CLI
Architecture](../../../architecture.md), and exact recovery-bundle identity
mechanics are defined by the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md); they are not public
command flags.

## Presentation Relationship

One typed operation result feeds both the default expanded and compact human
renderers and the `--json` renderer. The exact public blocks, compact retention
rules, Template identification, changed-path reporting, and structured fields
remain in [Human Output](interface.md#human-output) and [Structured
Output](interface.md#structured-output). Presentation selection does not change
target resolution, planning, effects, verification, observations, availability
conditions, semantic status, or process result.

`--json` disables prompts and renders the same complete result without rerunning
any operation stage. It renders one complete structured result to stdout for
every semantic status. Bounded diagnostics use stderr, and ordinary human text
is never mixed into structured JSON stdout. Compact and structured results
retain at most one required `Next:` action. Exact structured schema fields and
compatibility rules are defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md).

The human renderer does not name successful internal stages by default.
`--verbose` and structured output may expose planning and preflight evidence
under the shared output contract. Human rendering uses `requires attention` for
the `attention` semantic status while structured output retains the
status value `attention`. Primary human `complete`, `attention`, and
`incomplete` results go to stdout; primary human `invalid`, `blocked`, `failed`,
and `interrupted` results go to stderr. Each typed result stays together on its
assigned stream. Compact rendering retains the workspace, selection method, and
target identity when available, preview or application mode, status,
completeness and safety, supplied Template identity, affected paths,
generated-navigation effects, and exact preview effects or diffs, with at most
one required `Next:` action. Structured rendering retains the same identity and
next-action limit. Complete results have no `Next:` action. Incomplete and
direct errors name corrections; failed and interrupted results name ordinary
recovery when needed. Presentation does not add diagnosis or recommendations.

## Conformance Evidence

Gate 5 executable proof must cover the complete Interface and Behavior without
introducing a permanent requirement-ID system or changing accepted Architecture.
Public input and output evidence is listed in
[Interface Verification](interface.md#verification). The following evidence
covers the semantic, projection, effect, safety, recovery, and process
obligations:

- Automatic parent generated effects are planned against intended post-create
  bytes and use the complete Index projection.
- Singleton repetition of `--description`, `--responsibility`, and
  `--template` is rejected even for equal values; required repeated tags retain
  order and reject empty or duplicate values; repeated Boolean write-policy
  flags are idempotent; shared global-flag behavior is unchanged.
- Complete dry-run output uses the same request, facts, intended bytes,
  generated projection, plan, preflight, and status formation as application,
  has no persistent dry-run effects, and keeps planned changes `complete`.
- Safe incomplete coverage produces no write, while unsafe or ambiguous safety
  or authority produces `blocked` rather than `incomplete`.
- Normal creation, valid Template instantiation, generated-navigation effects,
  and verified identical-target no-ops are `complete`;
  `Failed`/positively observed `Retained` recovery is `attention`; Template
  placeholders are not inspected and authoring quality is not inferred.
- Verified no-op behavior occurs before recovery-bundle preparation.
- External bundle storage, semantic final-ZIP verification, collision handling,
  typed post-verification deletion state/disposition facts, and exact named
  lease-bound Cleanup are covered.
- Expected-state changes, safe creation and replacement, final route
  verification, retained partial state without restoration, residual
  preservation, and rerun convergence are covered.
- Parent route exposure and generated navigation are verified after the
  destination effect, including direct sibling metadata and bounded generated
  ownership.
- Template body copying is verified after frontmatter removal, without
  substitution or placeholder inspection, destination metadata transfer, or a
  retained lifecycle relationship.
- An existing identical target, an existing different target, an unsupported
  target, and an orphan target overwrite produce the Interface-defined result
  without unintended mutation.
- Human and structured results are rendered from one typed result, with no
  rerun of planning, application, or verification. Human complete, attention,
  and incomplete results use stdout; invalid, blocked, failed, and interrupted
  results use stderr. JSON uses stdout for every status, bounded diagnostics use
  stderr, and no human text is mixed into JSON stdout.
- Compact output retains the accepted workspace and target identity,
  preview/application, status, completeness and safety, supplied Template
  identity, affected paths, generated-navigation effects, exact preview
  effects or diffs, and at most one required `Next:`. Complete results have no
  `Next:`; incomplete and direct errors name corrections, and failed or
  interrupted results name ordinary recovery without diagnosis or
  recommendations.

Gate 5 evidence should include direct semantic checks, real filesystem boundary
checks, and built-process checks for the public result. The [CLI
Architecture](../../../architecture.md) defines parser, concrete serialization,
filesystem, lock, concurrency, and source boundaries; exact recovery-bundle
identity mechanics live in the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md). This contract does
not change them.

## Related Current Sources

- [route create Command Contract Set](_create.md)
- [Route Init Behavior Contract](../init/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
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
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [CLI Contract Document Templates](../../../../../../../templates/cli/documents/_documents.md)
- [Interface Contract](interface.md)
