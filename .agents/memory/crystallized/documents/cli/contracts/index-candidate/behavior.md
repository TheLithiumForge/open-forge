---
open-forge:
  description: Current Crystallized deterministic behavior, mutation lifecycle, safety, recovery, and conformance for `index`
  responsibility: Define how the current non-shipping `index` operation resolves, projects, applies, verifies, and recovers without choosing technology
  tags: [Memory, Crystallized, CLI, Release, Command, Index, Behavior, CurrentTruth]
---

# Index Behavior Contract

## Status And Boundary

This is the current Crystallized technology-neutral Behavior Contract for the
non-shipping `index` operation. The [Interface Contract](interface.md) defines
the complete public syntax, observable output, semantic result names, and error
surface that this behavior satisfies. The [Technical Design](technical-design.md)
records the accepted realization without changing this contract. The command
does not ship, and this file does not claim an implementation.

The accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
defines the shared exact structured-result schema and numeric process-exit
mapping. This Behavior Contract forms the typed result that conforms to that
shared boundary and does not choose libraries, modules, or other technology.

## Operation Boundary And Invariants

The operation follows one complete typed flow:

```text
validated input
  -> resolved sources and target closure
  -> authoritative topology and metadata facts
  -> complete expected generated bodies
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained-recovery reporting
  -> one typed result
```

The operation is deterministic for unchanged workspace bytes, current facts,
and explicit input. It selects the same regions, produces the same generated
lines and ordering, and forms the same semantic result.

A successful application repeated against unchanged input converges to the same
verified no-op. The no-op is an observation, not a synthetic write effect.

Read and selection facts do not acquire mutation authority. Persistent effects
begin only after the complete selected target set, expected projection, ordered
plan, and preflight have succeeded.

## Input Normalization And Target Closure

Request normalization accepts repeated `--dry-run` occurrences and collapses
the Boolean presence to one idempotent write-policy choice. A second or later
occurrence has no additional effect, does not multiply application authority,
and does not alter value-bearing repetition rules. The selected workspace is the exact
current working directory or exact `--workspace` value resolved by the shared
[Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md).
Normalization follows the no-discovery rules in the [Workspace
contract](interface.md#workspace) and does not introduce another workspace
resolution path.

Workspace validation applies the blocked condition in the [Workspace
contract](interface.md#workspace) before route resolution. It does not infer a
Loader or routed topology from a selected directory.

After request normalization, the resolver derives current routed topology from
current filesystem and source facts rather than generated lines. This satisfies
the rooted-selection and authoritative-projection boundaries in the [Rooted
selection](interface.md#rooted-selection) and [Authoritative
Projection](interface.md#authoritative-projection) sections.

Operand-free resolution applies the [Rooted selection](interface.md#rooted-selection)
rules. The resolver establishes the selected Loader and reachable entrypoint
regions from current topology before projection.

If the rooted-selection preconditions cannot be established, resolution blocks
without an alternate root or detached-tree fallback.

Explicit entrypoint resolution applies the [Entrypoint selection](interface.md#entrypoint-selection)
closure and parent rules. It computes that closure from direct routed
relationships, not current generated lines.

Parent inclusion and descendant completeness are treated as dependencies of the
resolved request. No higher ancestor is added solely by ancestry.

Leaf resolution applies the [Routed-leaf selection](interface.md#routed-leaf-selection)
rules: it resolves the exposing relationship from current topology and blocks
when the public target precondition cannot be established.

Detached resolution accepts only the conditions in [Detached entrypoint
selection](interface.md#detached-entrypoint-selection); it never creates absent
routing context.

An overwrite reference is normalized to the base logical source under the
shared [CLI Source References Behavior Contract](../shared/source-references/behavior.md).
Target resolution then applies the [Overwrite selection](interface.md#overwrite-selection)
rules.

Projection and effect planning exclude overwrite companions once base logical
identity is established. Overwrite content never becomes a generated entry and
the operation never modifies an overwrite companion.

The resolver unions operand closures and deduplicates target identities before
planning. Recognized `_index.md` entrypoints are canonicalized by physical
identity during this deduplication. If traversal reaches the same recognized
physical entrypoint through more than one path, it produces one target region,
one plan item, and at most one effect. Processing each recognized physical
entrypoint once is conformance behavior, not staging or migration behavior.

Canonical target ordering is computed after deduplication using canonical
workspace-relative containing-file paths and ordinal comparison. Discovery
order is not an input.

## Authoritative Projection

Projection treats current filesystem topology and authored source metadata as
authoritative and current generated entries as comparison data. Current
generated entries never become a second route inventory or a metadata fallback.

For each resolved target, projection derives current direct routed children and
authored source facts, validates required facts, then forms expected generated
bytes conforming to the [Generated lines](interface.md#generated-lines) section
before comparing current bounded content.

Projection rejects identity or route facts that fail the public precondition in
the [Authoritative Projection](interface.md#authoritative-projection) section
before plan completion.

Metadata acquisition follows source contracts and contributes only admitted
facts. Behavior does not define another metadata source.

Projection carries authored metadata through without semantic rewriting and
rejects invented fallback meaning. Required-metadata validation applies before
a complete projection can enter a mutation plan. Semantic usefulness remains
outside the operation.

### Generated body

The projector serializes each expected generated body to the exact public shapes
owned by the [Generated lines](interface.md#generated-lines) section. Behavior
requires conformance but does not define another line grammar.

Destination resolution and containment are validated before expected bytes are
accepted. The empty-child case is emitted exactly as required by the Interface
Contract; no alternate empty representation enters the plan.

Parsing, compatibility, encoding, line endings, and serialization follow the
accepted [Technical Design](technical-design.md#markdown-yaml-and-byte-boundaries)
and must produce the exact public generated shapes.

## Generated Boundary

Mutation planning admits a target only when the valid marker ownership
precondition in the [Generated Boundary And Bounded Effects](interface.md#generated-boundary-and-bounded-effects)
section is established. The implementation does not define another boundary.

Boundary validation checks the single-final-section condition, the complete
ordered marker pair, the non-nested and non-overlapping condition, and the
final-inline-region condition before an effect is planned.

Once the valid boundary and replacement preconditions are established, the
planner may treat malformed interior content as replaceable expected-content
input. Otherwise no replacement is planned.

When a boundary precondition fails, the planner refuses effects rather than
creating or repairing markers.

Replacement writes only the generated interior and preserves outside bytes and
marker tokens. No whole-file formatting or normalization is introduced.

## Complete Planning

The operation resolves the complete selected target set and preflights the
complete plan before the first write. One blocked target blocks every planned
update. There is no best-effort or partial-application mode.

Planning classifies each selected region as `update` when its current bounded
body differs from the expected body, or `unchanged` when the bytes already
match.

Unchanged regions are verified observations. They are never rewritten and do
not receive a new timestamp merely to represent a successful invocation.

The plan contains complete expected generated bodies and a canonical ordered
mutation sequence before application or dry-run rendering.

## Dry-Run Parity

Dry-run and application use the same request, authoritative topology
projection, current expected-state facts, planner, and preflight.

Dry-run stops before recovery-bundle or staging creation, temporary file creation,
replacement, formatting, or any other persistent effect.

Human dry-run output includes the exact bounded generated-region diff for every
planned update. Structured output includes the exact expected generated change
through typed before-and-after entries or equivalent bounded diff evidence. When
a non-blocking finding is present, the same complete plan and exact diffs remain
in the result, which states that no files changed. Neither presentation exposes
unrelated authored bytes or private recovery material.

A dry run with changes and no non-blocking finding is `complete` when the
complete plan and application preconditions were established safely. It reports
that regions would be updated and that no files changed. A dry run with safely
established planned changes and a non-blocking finding forms `attention`. It
exposes the complete plan and exact bounded diffs, states that no files changed,
and does not claim on-disk verification of bytes that were not written.

## Application Authority And Recovery

Omitting `--dry-run` selects application. The explicit command invocation is
confirmation to replace every changed body inside the selected machine-owned
generated boundary. Application does not prompt again and does not accept
`--yes`.

Application authority remains bounded to changed generated interiors. It does
not permit marker repair, authored-content replacement, whole-file formatting,
overwrite mutation, path escape, metadata invention, or another repair
operation.

A verified no-op has no affected mutation path and therefore does not create a
bundle. For an actual update with one or more existing targets to replace,
preparation uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a pre-effect
`incomplete` result. It prepares exactly one immutable ZIP bundle outside the
workspace. An operation containing only
Create effects or no-ops does not resolve recovery storage and creates no bundle.
Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries identify
the command/operation, normalized physical workspace, ordered relative
targets, change kinds, exact prior bytes/lengths/hashes, and intended final
absence or length/hash. `Create` and byte/semantic no-op effects add no bundle
entry.

The draft uses `CreateNew` under its exact name in the same external directory,
is closed and reopened for semantic manifest, exact ordered entry, length,
hash, and payload-byte validation, moved within that directory to the
deterministic final name, and reopened and verified. Only the valid final ZIP
forms the opaque `RecoveryBundlePreparation`; the draft remains `Incomplete`.
Every planned existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or
`Delete`) must match the preparation;
`FileChangeApplier` performs one final
effect per target. All bundle preparation is complete before the first target
effect. A collision or verification failure blocks before effects.

After every target effect and the complete projection verify, delete only the
positively recognized bundle created by this command. If deletion fails, target
effects remain successful and the result is `attention` with the exact residual
bundle path and cleanup guidance. Handled application, verification, or
cancellation failure stops new effects and reports the actual residual draft or
final path; a valid final remains after preparation. A closed final ZIP may
remain after abrupt process termination, without an executable crash or
power-loss guarantee. The command never restores a target automatically or
derives current target state from recovery provenance. Cleanup owns exact named
final and draft deletion under its separate lease-bound contract. A fresh
invocation plans from current facts and never replays
a saved plan or receipt.

## Application, Verification, And Recovery

Immediately before application, the operation rechecks the complete plan's
source and destination facts.

Volatile target facts are rechecked immediately before each replacement.

Each update applies complete planned file bytes through a safe same-directory
replacement property. The operation never edits the target in place and never
falls back to a weaker write after an atomicity or identity check fails.

The target bytes are verified after each replacement.

After all effects complete, the operation rebuilds the selected authoritative
projection and verifies that every selected generated body matches it.

An application or verification failure stops new effects. The prepared recovery
bundle remains available and the result identifies every residual target and bundle.
A failed operation remains `failed` because the requested index operation did
not complete. An interruption remains `interrupted` when no stronger failure
remains. A hard process stop after closed/readback verification may leave
complete old or new versions of individual target files and recognized bundle
evidence; this does not promise power-loss, directory-entry, or storage
durability. The operation creates no persistent transaction journal, plan,
receipt, or undo instruction.

Rerunning `index` computes a fresh plan from current facts and converges when the
remaining state is safe. It never replays a saved plan.

## Automatic Maintenance

Every CLI write that can change routing or indexed metadata uses the same
authoritative projection, planner, effect, verification, and recovery behavior
as `index`. Automatic maintenance does not start a hidden subprocess or invoke
the public parser recursively.

### Intended post-write state

A parent mutation first establishes its complete intended authored result. It
then builds generated-navigation effects from that hypothetical post-write
workspace before any persistent effect begins.

Automatic target selection includes exactly the generated regions whose
direct-child projection can change.

The accepted [`route move`](../route/move/behavior.md) and `route remove`
operations consider both current and intended topology so each affected
exposing parent is included: move includes its old and new parents, while remove
includes its old parent. A Loader projection is included when the selected
subject is a Loader-exposed category. These route operations own their subject,
ownership, reference, and complete mutation plans; `index` supplies only the
generated-navigation projection and does not perform their mutation.

When either route operation selects a category, its category plan may contain
many physically contained entrypoints and resources, but the generated
projection remains one dependency of that operation's one plan and one
recovery/verification boundary. It is not a series of independently committed
leaf index operations, and `index` does not expose a category batch mutation
surface.

Creating an entrypoint includes its own canonical generated region and every
parent region needed to expose it.

Automatic maintenance is dependency-minimal. It does not select the explicit
command's descendant recovery closure or rebuild the complete workspace unless
the parent operation's intended effects require that complete scope.

### One parent mutation

Authored and generated effects form one complete preflighted parent plan.

Compatible changes to the same physical file coalesce into one exact file
replacement rather than competing writes.

A parent dry run includes every generated-region diff.

Application orders generated effects after the authored facts they depend on,
then verifies the complete parent operation.

Generated planning failure blocks before the first parent write. Generated
application or verification failure stops new effects, retains the verified
recovery bundle, and reports the complete parent mutation's residual target
states; it does not restore an earlier effect.

The parent result contains generated-navigation results as typed postcondition
evidence. Human output summarizes that evidence in ordinary language. JSON
keeps the complete structured evidence in the same result document.

Writing a generated body never schedules another indexing pass. An unchanged
automatic projection remains a verified no-op without creating an effect.

## Typed Result And Ordering

The operation forms one typed result after dry-run preflight, application
verification, recovery, or an earlier invalid or blocked boundary. Human and
structured renderers consume that result; they do not rerun the operation or
reinterpret its semantic status. The human renderer sends primary `complete`,
`attention`, and `incomplete` results to stdout and primary `invalid`, `blocked`,
`failed`, and `interrupted` errors to stderr. It keeps each typed result together
on its assigned stream, including incomplete facts and findings. The structured
renderer sends one complete result document to stdout for every semantic status.
Separate bounded diagnostics use stderr, and ordinary human text is not mixed
into structured stdout.

The result records enough ordered evidence to distinguish requested and resolved
sources, target closure, authoritative topology and metadata coverage,
per-region action and change evidence, preflight and effect states, verification
and recovery, changed and unchanged targets, residual state, findings, semantic
status, and next actions. The shared exact field names and schema version come
from the accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).

The semantic conditions for `complete`, `attention`, `incomplete`, `invalid`,
`blocked`, `failed`, and `interrupted` are exactly the conditions in the
[Interface Contract](interface.md#semantic-results). Changes and verified
no-ops are ordinary `complete` results.

Target regions and generated entries use canonical ordinal ordering. Duplicate
and overlapping selections do not create duplicate result entries or effects.
Residual draft or final paths are reported without recovery-derived current
target classification; no restoration or rollback is selected. Numeric process
exits use the shared mapping defined by the accepted
[Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).

## Behavioral Conformance

The behavioral concerns allocated to this contract are mandatory evidence
concerns. Eventual implementation evidence must cover:

- Rooted, entrypoint, leaf, detached, overwrite, repeated Boolean write-policy,
  duplicate, overlapping, and recognized `_index.md` physical-identity
  selection behavior. A recognized `_index.md` entrypoint is processed exactly
  once by physical identity.
- Filesystem-derived direct children independent of current generated lines.
- Required metadata, native source metadata, missing values, malformed values,
  and the absence of invented fallback meaning.
- Canonical linked and empty entry lines, destination containment, ordinal
  ordering, and stable output bytes.
- Valid stale bodies, malformed bodies inside valid markers, and every invalid
  heading or marker boundary.
- Byte preservation outside generated interiors.
- Complete planning, exact dry-run diffs, no persistent dry-run effects, dry-run
  preflight blockers, and `attention` formation for safely established planned
  changes with non-blocking findings.
- Verified no-op behavior before bundle preparation.
- Existing-target bundle preparation and readback verification, unknown and
  colliding bundle protection, all-before-first-effect readiness, success-only
  cleanup, failure retention/reporting, and support-artifact nonrecursive scope.
- Expected-state changes before and during application.
- Safe replacement, matching preparation enforcement, per-effect verification,
  complete semantic verification, residual preservation, interruption, and
  rerun convergence without automatic restoration.
- Automatic creation, metadata change, move, remove, Framework, and Extension
  plans against intended post-write state.
- Parent dry-runs and results that include generated effects without recursive
  command invocation, while preserving one typed result and the accepted human
  and structured stream behavior.

Direct tests should prove the relevant selection, projection, ordering, marker,
effect-planning, status, and no-op concerns. Focused integration tests should
use real temporary rooted and detached source trees, external recovery bundles
and drafts, filesystem failures, concurrency changes, and parent mutations. A small built
Native AOT process suite should prove parsing,
exact dry-run output, human and structured results, exit behavior, and packaged
execution. Gate 5 AOT evidence remains pending.
