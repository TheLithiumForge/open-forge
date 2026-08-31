---
open-forge:
  description: Accepted current technology-neutral resolution, planning, effects, safety, recovery, and conformance for `route init`
  responsibility: Define how a conforming implementation resolves and safely applies route initialization without selecting technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Init, Entrypoint, Behavior, CurrentTruth]
---

# route init Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract for `route init`. The command does not ship yet. Its local
implementation and complete executable proof are squash-integrated at
`cc5085ce`; replacement-CLI delivery remains pending.

This contract defines only the deterministic, technology-neutral operation behind
the [Interface Contract](interface.md). The Interface Contract owns the complete
public syntax, metadata grammar, target forms, examples, human and structured
output, semantic results, errors, and non-goals. This file defines how a
conforming implementation resolves, plans, applies, verifies, recovers, and
forms that result. It adds no operand, flag, alias, output stream, schema,
semantic result, or implementation technology.

The [Behavior Contract in the CLI Command Contract Set](../../../command-contract-set.md#behavior-contract)
and [Shared CLI Operation Contract](../../../shared-operation-contract.md)
guide this boundary. The shared [Global CLI Flags](../../shared/global-flags/behavior.md),
[CLI Source References](../../shared/source-references/behavior.md), and [Index Behavior Contract](../../index-candidate/behavior.md)
contracts remain authoritative at their own scopes. The Framework routing and
Markdown sources remain authoritative for the meaning this operation consumes.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
structured schema, process-status mapping, parser and serialization, source
structure, package and runtime boundaries, BCL-first filesystem boundary,
workspace lock, and recovery identity model. This contract does not choose those
details and remains technology-neutral.

## Operation Invariants

- For unchanged workspace bytes and the same explicit input, resolution selects
  the same missing entrypoints, intended scaffold bytes, generated navigation,
  ordered plan, and semantic result described by the [Interface Contract](interface.md).
- Repeating a successful application against the resulting state forms a
  verified no-op. The no-op is an observation and does not become a synthetic
  effect.
- The operation follows one complete mutation flow. Persistent effects begin only
  after the complete route chain, intended generated projection, ordered plan,
  and preflight have succeeded.
- One blocked or incomplete target prevents the complete operation from
  applying. There is no best-effort or partial-application behavior.
- Existing entrypoints are read-only authored inputs. Only bounded generated
  `Entries` effects required by the intended topology may affect an existing
  entrypoint.
- The generic draft scaffold is command behavior, not a Template instantiation.
  Framework mode copies canonical embedded entrypoint assets through the neutral
  distribution capability. Neither mode creates the Loader or infers route
  meaning from folder names.
- Request resolution remains explicit and non-wizard. The operation has no
  `--automatic` mode, alias, or inferred current-scope mode.

## Request Resolution

### Workspace and command request

Resolve the workspace using the exact current directory or exact
`--workspace <path>` value under the shared [Global CLI Flags](../../shared/global-flags/behavior.md)
contract. Do not add parent, Git-root, marker, nearby-`.agents`, or route-based
workspace discovery. Terminal `--help` and `--version` handling remains in that
shared contract and does not run this operation.

Validate the command path, one required route-target operand, Framework mode,
command-specific metadata flags, write-policy flags, and global flags against the public [Syntax](interface.md#syntax)
and [Flags](interface.md#flags). Normalize command-specific repetition before
route resolution:

- A second `--description` or `--responsibility` is invalid, including when its
  value equals the first occurrence.
- Metadata flags combined with `--framework` are invalid.
- `--tag` occurrences remain one ordered multi-value list. Empty values, exact
  duplicates, and invalid tag syntax remain invalid.
- Repeated `--framework` occurrences collapse to one idempotent Boolean choice.
- Repeated `--dry-run` occurrences collapse to one idempotent Boolean choice.

Shared global flags retain the repetition, ordering, composition, and terminal
rules of their shared contract. No command-specific value uses precedence or
last-wins behavior. Invalid input stops before route resolution.

### Target normalization

The resolver uses the shared source-reference interpretation for an automatic ID
or an exact `.agents/...` path, then applies the selected `route init` target mapping in
[Route Target](interface.md#route-target):

- An ID resolves to the intended folder ID below `.agents` and its final
  canonical `_{folder-name}.md` entrypoint path.
- An exact canonical path identifies a missing target only when its filename is
  the final folder's canonical entrypoint filename.
- An exact existing compatibility path is accepted only to preserve that file
  while initializing missing ancestors.
- A compatibility path is never converted into a new canonical sibling.

Resolve the target lexically and physically inside the selected workspace. Reject
empty input, the `loader` ID, `.` or `..` ID segments, unsafe containment, and an
exact missing path with the wrong final filename. A directory, ordinary Markdown
file, Loader, overwrite, `SKILL.md`, or compatibility filename is not an exact
missing-target form. The shared [CLI Source References](../../shared/source-references/behavior.md)
contract supplies exact path detection, quoting, identity, and collision
boundaries; this command does not guess another target shape from filesystem
coincidence.

### Framework alignment and scope labels

When `--framework` is selected, resolve the complete embedded Framework payload
and canonical topology through the neutral distribution capability. Require a
trusted current root Install lifecycle whose source inventory matches the
running CLI. A missing or untrusted installation blocks; a trusted installation
from another inventory directs the caller to root Update before scoped
initialization.

Trust is physical currentness, not merely a structurally valid persisted
lifecycle record. One neutral Framework currentness reader consumes a validated
Framework lifecycle state, the running embedded payload, and current contained
target reads. It returns typed `current`, `source-mismatch`, `changed`,
`missing`, `unavailable`, `blocked`, or `cancelled` facts. It applies the accepted
fingerprint policy to source-backed semantic or exact targets, bounded generated
`entries` interiors, and root `AGENTS.md` and `CLAUDE.md` managed blocks, while
validating each current `sourceAssetPath` against the running inventory. It has
no mutation, Update, Install, diagnosis, or command-result policy. Route Init
maps those neutral facts to its own findings and next actions.

One separate neutral embedded-source adapter projects only recognized
`.agents/...` payload assets into canonical base sources for the supplied
workspace. Shared source and topology builders consume those facts. Framework
alignment, scope insertion, scaffold selection, and sparse-chain policy remain
Route Init-local; the adapter is not a general topology, Template, slug, or
mutation engine.

For either operand form, derive the ordered concrete folder chain and require its
first segment to be an exact installed root route. Align exact case-sensitive
non-root canonical Framework segments in their canonical order and treat
inserted segments as scope positions. Require exactly one alignment and a
canonical non-root Framework final segment. A trailing user-only scope belongs
to generic mode. Reject nested root recreation and reordered managed segments.

For ID-form input, convert each inserted scope label by iterating Unicode runes, lowercasing letters
invariantly, preserving digits, collapsing whitespace plus ASCII `_` and `-` to
one `-`, and trimming that separator. Reject other punctuation, control
characters, path separators, empty output, `.` and `..`. Then apply ordinary
portable collision, physical-identity, and containment checks. Exact
`.agents/...` input is already concrete and is never slugged, but its concrete
segments must pass the same unique topology alignment.

The intended Framework chain contains only the requested sparse path. Missing
aligned segments use exact embedded canonical entrypoint assets; missing
inserted scope segments use the generic draft scaffold. Inserted scope
entrypoints remain user-owned. Copied Framework entrypoints and derived generated
regions are the only new Framework lifecycle targets. Each copied target records
its normalized canonical embedded `sourceAssetPath`; a derived generated-region
target records `null`. The operation adds no lifecycle instance grouping.

### Chain selection

Inspect every folder from the first target segment through the final target
folder, in chain order. For each folder, establish the current recognized
entrypoint set:

1. Exactly one recognized entrypoint is an existing authored input to preserve
   and use.
2. No recognized entrypoint produces one planned canonical entrypoint.
3. More than one recognized entrypoint blocks the complete request.

The recognized set includes the canonical filename and the existing compatibility
filenames defined by the [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md).
Do not normalize, rename, rewrite, or create a canonical sibling beside one
existing compatibility entrypoint.

Inspect ordinary routed files and physical path identities for route collisions.
An automatic-ID collision with an intended entrypoint, or an identity that would
make two route subjects indistinguishable, blocks. Exact path input can resolve
which source reference was selected, but it cannot make an ambiguous route
structurally valid.

The chain may be detached below `.agents`. Never create `.agents/loader.md`. If a
valid Loader exposes or can expose the first route folder, include the Loader's
generated region in the same parent mutation. If no Loader exists, keep the first
entrypoint detached. A present Loader relationship that is ambiguous or unsafe
blocks instead of being ignored.

If the final target entrypoint already exists, metadata flags for that final
target are invalid. Existing authored entrypoints remain read-only except for
the generated effects required by the complete intended topology; changing an
existing source belongs to `route update`.

### Draft metadata resolution

For every planned missing ancestor, derive the draft description from its path
and assign `NeedsAuthoring`. For a missing final target, resolve explicit
metadata independently from the ancestor defaults:

- A non-empty description replaces the final draft description in frontmatter and
  the compact body definition.
- A non-empty responsibility adds the optional frontmatter field. The exact empty
  value omits it.
- Repeated tags are retained in argument order as the final target's explicit tag
  list.
- If either the explicit description or any explicit tag is absent, append
  `NeedsAuthoring` after supplied tags unless it is already present.
- If both an explicit description and one or more explicit tags are present, keep
  exactly the supplied tag list and do not add `NeedsAuthoring`.

Reject an empty or whitespace-only description, whitespace-only responsibility,
empty tag values or tag sets, duplicate exact tags, and tags that do not follow
canonical tag syntax without the `#` prefix. Validate syntax and presence only.
Do not judge or rewrite semantic accuracy. A later `doctor` operation may report
meaning-quality diagnostics separately.

The resolver records draft versus explicit metadata provenance for each created
entrypoint. It does not obtain metadata from a Template, filename meaning, or
folder semantics, and it does not invent a fallback.

Canonical emission and reading recognize exactly one Open Forge metadata root,
`open-forge`, with supported children `description`, `tags`, and optional
`responsibility`. A `rune` root and every other unrelated YAML root are opaque:
they supply no Open Forge metadata and bounded generated-region editing preserves
their bytes. When `open-forge` and `rune` coexist, the reader uses exactly
`open-forge` and ignores the meaning of `rune` without deleting or rewriting it.

The resolver records the intended tag list for every new entrypoint. After a safe
complete plan and preflight, any new entrypoint whose intended tags contain the
exact `NeedsAuthoring` tag forms `attention`, whether the marker came from the
draft rule or from explicit input. Existing unchanged entrypoints are not new;
an unchanged existing `NeedsAuthoring` tag therefore does not change a verified
no-op from `complete`. Planned changes alone do not form `attention`, and the
resolver does not score health or semantic quality.

## Current Facts And Coverage

The complete current-fact set for one plan includes:

- The selected workspace and the normalized target identity and canonical path.
- The selected generic or Framework mode and, for Framework mode, exact embedded
  inventory, canonical topology alignment, trusted root Install lifecycle, and
  source-asset provenance.
- Every folder in the target chain and its canonical or recognized
  compatibility entrypoint state.
- Ordinary routed files and physical identities that could collide with intended
  entrypoints.
- The exact Loader state and whether its relationship to the first target folder
  is valid, absent, ambiguous, or unsafe.
- Existing authored entrypoint bytes, metadata, generated ownership boundaries,
  and direct routed children that the intended topology may expose.
- The metadata required to represent every existing direct child in each planned
  generated region.
- The expected current source and destination facts needed for collision,
  revalidation, application, and recovery.

Framework-mode current facts include one canonical complete schema-v1 lifecycle
envelope at `.agents/open-forge.lifecycle.json`. Its standard root keys are
`schemaVersion`, `fingerprintPolicy`, `workspacePath`, `framework`, and
`extensions`, in canonical order. An installation with no Extensions still has
the complete empty section
`{ coverage: "complete", packages: [], paths: [] }`. A missing, `null`,
malformed, unsupported, or incomplete standard section is not assigned implicit
meaning and produces no Route Init write; Update or Doctor owns repair. Route
Init never creates the lifecycle document and publishes scoped Framework facts
only after the common envelope and both standard sections are complete and
trusted.

Selection and projection do not treat current generated lines as an independent
route inventory or metadata fallback. The complete [Index Behavior Contract](../../index-candidate/behavior.md)
projection derives generated navigation from current topology and authored
metadata, and the parent operation applies it to the complete intended
post-initialization topology.

Missing, empty, malformed, ambiguous, or mechanically inconsistent metadata
needed for an intended generated region prevents a complete plan before any
write. The operation does not omit an existing child or invent fallback metadata.
When safe authority and containment are established but required inspection or
planning coverage cannot finish, form `incomplete` and begin no write. Unsafe or
ambiguous authority, identity, containment, route, collision, or generated
ownership boundaries form `blocked`; a present malformed or ambiguous generated
boundary remains blocked and `route init` does not repair it.

The operation preserves authored bytes and compatibility filenames outside the
bounded generated regions it is explicitly allowed to update. The CLI
Architecture's accepted fixed Markdown and source-generated YAML paths realize
the parser and serialization boundary; any implementation must still produce the
public canonical scaffold and generated-region behavior.

## Selection And Result Formation

Resolve the chain in first-to-final order and form one intended post-write route
topology before planning effects. In generic mode, each missing folder contributes
one canonical entrypoint with its fixed scaffold and a valid empty generated
region. In Framework mode, each missing scope contributes that same draft shape,
while each missing aligned Framework segment contributes its canonical embedded
asset with a destination-local generated region. Each existing folder contributes
its preserved entrypoint and current authored facts.

The generated projection then includes, when applicable:

- Every new direct child entrypoint under its intended parent.
- The first new entrypoint under a valid exposing Loader.
- Existing direct routed children in a newly routable folder.

Use the complete [Index Behavior Contract](../../index-candidate/behavior.md) projection for generated
line shape, destination containment, ordering, marker ownership, and the
verification and recovery-disposition relationship. The route-init operation owns the
complete intended topology and its combined result; it does not start a hidden
public `index` command or perform a second independent projection.

Classify the complete intended state as changed or unchanged from current bytes.
An unchanged complete chain is a verified no-op with no mutation path. A changed
chain produces one typed result after dry-run preflight, application and final
verification, or retained partial-state reporting. The semantic conditions for `complete`, `attention`,
`incomplete`, `invalid`, `blocked`, `failed`, and `interrupted` are exactly those
in the [Interface Contract](interface.md#semantic-results); this Behavior
Contract does not add another result or choose numeric exits. A complete plan
with any new exact `NeedsAuthoring` tag forms `attention` for a safe preview or a
completed and verified application. Post-verification recovery deletion
`Failed`/positively observed `Retained` also forms `attention`;
`Failed`/`Unknown` forms `failed`. A complete plan without that marker is
`complete` when no other status
condition applies. Existing unchanged marker content and planned changes alone
do not change the no-op or successful result to `attention`; an unexpected
application or verification failure remains `failed`.

Aggregate status precedence is `failed`, `interrupted`, `invalid`, `blocked`,
`incomplete`, `attention`, then `complete`. Invalid input still stops before
operation resolution; the complete precedence governs one formed result when
several retained conditions coexist.

## Effects

The operation follows this complete typed flow:

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

### Complete plan

Build the full ordered plan before the first persistent effect. It contains the
new directories, new entrypoint files, and bounded replacements of existing
machine-owned generated interiors required by the intended topology. A blocked
target or projection fact prevents all effects. Safe facts that do not provide
the required inspection or planning coverage form `incomplete` and also prevent
all effects. There is no partial or best-effort application.

Keep directory creation separate from file Create/Replace/Delete and limit it
to directories in the intended route chain. A generic plan that starts without
`.agents` exposes that exact path as its first ordinary directory-create effect.
After acquiring the external workspace lease, the shared applier confirms the
path is missing, creates and verifies it, and reports its actual residual state.
Cancellation or lock contention before acquisition creates nothing. Framework
mode requires an existing trusted Install state and therefore never creates an
absent `.agents` root.

While holding the workspace lease, apply every explicitly planned missing
directory parent-first through the shared capability. Each is a descendant below
`.agents`: immediately revalidate the missing target and its exact contained
physical parent, call ordinary `Directory.CreateDirectory`, then verify the
resulting contained ordinary directory. Do not remove, rename, claim, or format
existing user content. A directory has no recovery entry; after contention,
later failure, or interruption, it remains and is reported as residual state.
The operation never rolls it back, compensates for it, or removes it.

Generated-navigation effects are part of this same parent plan. They use the
complete [Index Behavior Contract](../../index-candidate/behavior.md) projection, ordering, generated
boundary, verification, and recovery behavior. Compatible authored and
generated effects for the same physical file remain one bounded planned effect
under that contract. The operation never invokes a hidden `index` subprocess.

### Dry-run parity

Dry-run and application use the same normalized request, current route and
compatibility facts, intended entrypoint state and scaffold bytes, intended
topology, generated projection, ordered plan, expected-state facts, preflight,
and semantic status conditions. Dry-run stops before recovery-bundle creation, directory
or file creation, replacement, formatting, or any other persistent effect. Its
human and structured result exposes every new directory, new entrypoint,
generated-navigation effect, and exact bounded existing-file change required for
effect review without exposing unrelated authored bytes or private recovery
material. It reports complete effects and diffs even when the semantic status is
`attention`; planned changes alone do not form `attention`.

### Application

Omitting `--dry-run` selects application. The explicit command and target are
confirmation for the missing route chain and the planned generated interiors
only. Application does not prompt and does not accept `--yes`.

Before the first effect, preflight checks every planned new-path collision,
planned existing path, route relationship, metadata fact, generated boundary,
containment fact, expected-state condition, and recovery-bundle condition
required by the Interface Contract. A verified no-op has no affected mutation
path and therefore does not need a bundle.

Application first acquires the persistent external workspace lease, then
immediately revalidates the complete plan and prepares and verifies the complete
external recovery bundle when an existing-target effect requires one. It applies
and verifies the separate parent-first directory effects, beginning with missing
`.agents` in generic mode, and only then begins the planned file and
bounded-region effects with their own immediate target revalidation. Bundle
preparation completes before every workspace effect.

After preflight, the same finite attention rule applies to both modes: a dry-run
with any new entrypoint with exact `NeedsAuthoring` in its intended tags forms
`attention`, and an application that creates and verifies such an entrypoint
forms `attention`. Every new entrypoint with complete intended metadata and no
exact marker remains `complete` when no other status condition applies.

## Safety And Recovery

### Recovery-bundle boundary

The command does not inspect or report repository state. When the plan contains
an existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
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
Directory-create effects, file Create effects, and no-ops have no recovery
entry. A CreateNew draft is closed and reopened
for semantic manifest, exact ordered entry, length, hash, and payload-byte
validation, moved within the same directory to its deterministic final name,
and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. Every planned
existing-target effect must match the preparation; `FileChangeApplier` performs
one final effect per target. All preparation completes before the first target
effect; unknown, malformed, mismatched, or colliding bundles block.

### Revalidation and bounded writes

Immediately before application, recheck the complete source, destination,
collision, route, metadata, generated-boundary, and expected-state facts. Recheck
volatile target facts immediately before each replacement. Apply complete planned
bytes, not an in-place authored edit, and preserve all bytes outside the planned
generated interiors. The generated-region replacement must retain the safe
replacement and identity properties required by the [Index Behavior Contract](../../index-candidate/behavior.md)
contract; no weaker fallback is introduced when those properties cannot be
established.

Verify each applied effect. After all effects complete, rebuild the route
projection from the final chain and verify the final entrypoint chain, intended
scaffold representation, and generated navigation. A final mismatch is not a
successful initialization.

### Recovery and concurrency

When application or verification fails, stop new effects; do not restore, reverse,
or compensate for an earlier effect. A concurrent edit remains preserved and is
reported as residual state.

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
without an executable crash or power-loss guarantee. Recovery provenance does not classify current target
state. Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. A rerun computes a fresh plan from
current facts and never replays a saved plan, receipt, journal, history, or
progress record. When the remaining state is safe, it converges on the intended
route state and reports a verified no-op.

An unexpected application or post-write verification failure remains `failed`.
Caller cancellation or interruption is `interrupted` only when no stronger
failure remains. An `incomplete` or `blocked` result never begins a write.

Expected-state revalidation and preservation of unexpected concurrent edits are
current safety meaning. The persistent reusable zero-byte external workspace
lock below `LocalApplicationData/OpenForge/locks/v1` is held with one read/write
`FileShare.None` handle; it never receives metadata writes, deletion, or
truncation. The BCL-first filesystem boundary and recovery-bundle identity model
are defined by the CLI Architecture; they are not public command flags.

## Presentation Relationship

Form one typed result after request validation, incomplete or blocked planning,
dry-run preflight, application verification, or retained partial-state reporting.
Human and structured renderers
consume that result and do not rerun planning, application, verification, or
semantic interpretation.

The result builder preserves the Interface-owned property order, chain order,
effect execution order, tag argument order, unique ordinal unchanged paths, and
finding code/target/cause order. It derives the envelope status and at most one
next action once from that ordered finding set. It never broadens nullable
coordinates, exposes unrelated lifecycle bytes, or reconstructs target
operand-form provenance.

The [Interface Contract](interface.md) owns the exact human examples, compact and
expanded presentation requirements, structured facts, semantic statuses, error
meaning, and stream allocation. Compact rendering retains workspace, selection
method, and target identity when available, application or preview, status,
completeness, safety, created and unchanged paths, generated-navigation effects,
draft paths, and at most one required `Next:` line from this result. Structured
rendering retains the same identity and at most one required `Next:` action.
Verbose or structured presentation may expose bounded planning and preflight
evidence, but default human output does not expose successful internal stage
names. Primary human complete, attention, and incomplete results use stdout;
primary human invalid, blocked, failed, and interrupted results use stderr.
`--json` emits one complete result to stdout for every status, bounded
diagnostics use stderr, and human text is not mixed into JSON stdout.

## Conformance Evidence

Implementation evidence must cover the following behavior and safety obligations
in addition to the public checks in [Interface Verification](interface.md#verification):

- ID and exact canonical-path target resolution, including spaces, Unicode, and
  unsafe segments.
- Exact required-target parsing without a wizard, `--automatic`, alias, or an
  inferred current-scope mode.
- Generic and Framework mode selection, invalid metadata combinations, and
  idempotent repeated `--framework`.
- Unique embedded canonical-topology alignment for both ID-form and exact-path
  input, with zero, one, multiple, and consecutive scopes; scopes before and
  between managed segments; reordered segment, nested-root, ambiguous-alignment,
  and trailing-user-scope handling.
- Scope-label casing, digits, Unicode letters, separator collapse, invalid
  punctuation and path separators, empty output, exact-path non-slugging, and
  post-conversion collision checks.
- Trusted current root Install and embedded-inventory matching, sparse-chain-only
  creation, exact managed asset bytes, draft user scope bytes,
  destination-local generated navigation, per-target `sourceAssetPath`, and no
  Framework lifecycle claim for a scope entrypoint.
- Canonical complete lifecycle root keys, complete empty Extensions, physical
  target currentness, and rejection without writes for missing, `null`,
  malformed, unsupported, or incomplete standard sections.
- Singleton rejection for repeated `--description` and `--responsibility`,
  including equal values; ordered repeated `--tag` values with exact duplicate,
  empty, and syntax validation; idempotent repeated Boolean write-policy flags;
  and the shared global-flag repetition rules.
- One missing target, several missing ancestors, and a complete verified no-op
  chain.
- Detached chains, valid Loader exposure, absent Loader handling, and proof that
  the operation never creates `.agents/loader.md`.
- Existing canonical and each recognized compatibility entrypoint filename,
  including preservation without canonical sibling creation.
- Multiple-entrypoint, ordinary-file automatic-ID, portable-path, and physical
  identity collisions.
- Exact fixed scaffold bytes, literal visible slug titles, draft descriptions,
  `NeedsAuthoring`, inherited `Axioms`, and valid generated regions.
- Final description, responsibility addition and omission, tag replacement and
  ordering, partial metadata, duplicate and invalid field values, and metadata
  provenance for every created entrypoint.
- Canonical `open-forge` authoring and reading only, opaque `rune` preservation,
  and `open-forge` selection when unrelated sibling YAML is present.
- Existing direct children in a newly routable folder and blocking when child
  metadata needed by the intended generated region is missing.
- Automatic generated effects formed against the complete intended topology and
  the complete [Index Behavior Contract](../../index-candidate/behavior.md) projection.
- Dry-run and application parity for request, current facts, intended state,
  generated projection, ordered plan, preflight, and semantic status conditions;
  complete effects and exact bounded diffs in dry-run with no persistent effects.
- Finite `attention` formation for draft ancestors, a draft final target,
  automatically supplied or explicitly retained `NeedsAuthoring`, or
  `Failed`/positively observed `Retained` recovery, and no attention from planned
  changes alone or unchanged existing marker content.
- Complete status when every new entrypoint has complete intended metadata and
  no exact `NeedsAuthoring` marker.
- Verified no-op formation before recovery-bundle preparation.
- Separate parent-first directory effects under the held workspace lease, with
  immediate missing-target and physical-parent revalidation, ordinary BCL
  creation, post-verification, retained residuals, and no rollback,
  compensation, removal, recovery protocol, P/Invoke, or hostile same-user
  creator-identity guarantee.
- Generic-mode missing-`.agents` planning/reporting as the first ordinary
  lease-bound directory effect, cancellation/contention before workspace
  effects, later residual behavior, and Framework-mode refusal when trusted
  Install state is absent.
- External bundle storage, semantic final-ZIP verification, collision handling,
  typed post-verification deletion state/disposition facts, and exact named
  lease-bound Cleanup.
- Expected-state changes before and during application, safe creation and
  replacement, final route-projection verification, retained partial state
  without restoration, residual preservation, unexpected concurrent edits, and
  rerun convergence.
- Seven semantic results, including safe `incomplete` with no write, blocked
  unsafe or ambiguous authority or safety, `Failed`/positively observed
  `Retained` recovery `attention`, and failed application,
  post-write-verification, or `Failed`/`Unknown` recovery outcomes.
- Compact retention of workspace and target identity, application or preview,
  status, completeness, safety, created and unchanged paths, generated effects,
  draft paths, and at most one required `Next:` line.
- Human stdout/stderr assignment, one complete JSON result on stdout for every
  status, bounded diagnostics on stderr, and no human text in JSON stdout, all
  from one typed result.

The Interface defines the exact Route Init result graph. The CLI Architecture
defines the shared envelope and JSON compatibility, process-status mapping,
parser and serialization, filesystem and identity implementation,
recovery-bundle identity, concurrency mechanics, and source boundaries. Gate 5
executable proof must cover those decisions without weakening the accepted
repetition, status, stream, attention, dry-run, compact, safety, or recovery
requirements.

## Related Current Sources

- [Route Init Interface Contract](interface.md)
- [route init Command Contract Set](_init.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [CLI Architecture](../../../architecture.md)
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
