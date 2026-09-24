---
open-forge:
  description: Accepted technology-neutral Extension update behavior for trusted source reconciliation, force, prune, shared owners, and recovery
  responsibility: Define update's deterministic source and lifecycle resolution, one complete plan, bounded effects, verification, and result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Update, Behavior, Dependency, Ownership, Safety, Recovery, CurrentTruth]
---

# extension update Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping
`open-forge extension update`. It defines exact request and source resolution,
Framework-anchor and route-host gating, trusted lifecycle facts, dependency
closure, current/intended comparison, semantic fingerprints, ownership,
normal/force/prune planning, generated navigation, dry-run/application,
verification, recovery, result formation, and conformance. It does not choose
package schema, parser, storage, or implementation technology. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact
shared JSON result schema and exit mapping, while the [CLI
Architecture](../../../architecture.md) defines the cross-cutting implementation
boundary; this behavior does not duplicate those mechanics
or claim their Gate 5 proof.

## Persistent Exclusions

Apply the [removal settings](../../remove/interface.md#keep-removed-and-restore)
to package selection and concrete destinations. Explicitly selected excluded
package IDs are blocked; bulk updates skip them. An excluded required dependency
blocks its parent. Preserve existing excluded files and claims, and omit excluded
destinations from replacement, prune, generated-region and new-ownership effects.
Do not recreate a missing excluded ancestor needed by selected content. Force
and automatic mode do not clear exclusions. Revalidate the exact settings
observation under the workspace lease.

## Ownership Source

`.agents/open-forge.lock.json` is the sole ownership input and publication target.
Selection, dependencies, whole-file paths and shared owners come from receipts.
Preserve unselected Extensions, selected region receipts, Framework and Libraries.
Missing or unreadable ownership supplies no claims; an unrecorded selected ID
reports `extension-update.lifecycle-observation` without inferred file effects.
Duplicated identities likewise produce information without partial inference.
No retired state file is read, migrated, changed or deleted.

The existing lifecycle outcome reports the lock publication. An unchanged lock
is preserved, a planned write reports its actual verified receipt, and a skipped
write reports `none`/`not-requested`. Never synthesize integrity from membership.

## Consumer Destination Permissions

Explicit non-dry-run `--allow-path` edits the shared authored settings after safe
planning and before admission; failure is reported and stops content application.
It is a separate authored edit and remains if later content fails. Interactive
always approval declares a settings effect covered by the operation's recovery
bundle. Once approves only this operation and writes no settings; cancel applies
nothing. The shared contract owns the exact reader, authoring and receipt rules.

Consume the [Workspace Permissions Interface](../../shared/workspace-permissions/interface.md) and
[Behavior](../../shared/workspace-permissions/behavior.md). Require shared allow-list admission for intended external targets and previously owned
external targets in the selected update plan, including retirement and
preserved paths.
Existing `.agents/` targets need no grant; their prior safety and ownership
checks remain. Revocation blocks the complete selected lifecycle operation,
including ownership release, until exact explicit reapproval. Unrelated
installed packages do not enter this request's required set.

An eligible human apply request asks once for the complete missing set after
safe preflight. JSON, automatic, redirected and dry-run execution never ask the
permission question. An explicit non-dry-run `--allow-path` still authors a grant. Existing selection and force/prune
questions keep their separate rules. Force and prune never supply permission.
Malformed or unsafe settings are never overwritten by approval.

Interactive always approval creates a declared settings-file create/replace effect. Revalidate the
observed settings and approved plan under the existing workspace lease. Cover
prior settings bytes or proven absence in the one verified operation bundle,
then persist and verify approval before content and lifecycle effects. Later
failure retains the grant and its actual outcome. Restoration is manual; no
new automatic Repair behavior follows.

## Complete Typed Flow

```text
validated IDs or --all and exact source
  -> exact workspace and source disjointness
  -> Framework anchor and route-host coverage
  -> trusted Extension section and dependency closure
  -> current/intended semantic comparison
  -> intended authored topology and generated projection
  -> one complete dependency-first plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> lifecycle publication and recovery-bundle disposition
  -> one typed result
```

No safe subset applies when a selected root, dependency, path, owner, route,
section, source, or recovery boundary is incomplete, blocked, ambiguous, or
unsafe.

## Request And Source Resolution

Normalize direct IDs, `--all`, one exact source, and independent Boolean flags.
Reject explicit IDs combined with `--all`, duplicate IDs, repeated singleton
source, unknown IDs, and terminal conflicts. `--all` selects all currently
managed IDs represented by the selected source universe and closure; it does not
select every package in the world and does not silently skip missing coverage.

Select exactly CWD or exact `--workspace`. Classify `--source` as one exact
package or catalogue and prove lexical and physical disjointness from the
workspace. When omitted, read the embedded catalogue. No network, registry,
cache, ambient search, glob, fuzzy matching, resemblance, or fallback source is
available.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

Every selected ID must be represented by readable source facts in the selected
universe. When the selected source contains exactly one completely validated
package and no IDs or `--all` were supplied, use its valid manifest ID as the
only deterministic inference in human, non-interactive, and automatic requests.
A multi-package source requires explicit IDs or `--all`. Automatic mode never
chooses among packages or broadens selection to all. A missing, duplicate,
malformed, or conflicting ID is invalid or blocked; it is never inferred from a
folder.

## Framework Anchor And Route Hosts

Before mutation, require an ordinary contained `.agents` container and complete
facts for affected authored hosts, generated boundaries, routes and physical
paths. Stored Framework version, inventory and content hashes are not gates. The
only exception is the exact proof for an unrelated readable ordinary
metadata-invalid generated region defined below; it does not relax any selected
or dependency boundary.

## Proven Unrelated Readable Metadata

An unavailable `MetadataInvalid` generated region may be treated as unrelated to
one selected Update only after exact typed evidence proves all of these facts:

1. The affected closure is derived from the original selected package paths,
   including exclusions, retired paths, admission-affected paths, ancestors, and
   direct projected dependencies through the existing formation.
2. The unavailable region itself is outside that closure. Every malformed direct
   child is also outside that closure, is ordinary Markdown or
   a recognized entrypoint, and has successfully read current bytes. Every other
   direct child observation is usable. Native Skill metadata never qualifies.
3. A pure counterfactual re-projection that changes only those malformed ordinary
   metadata facts to `Missing`, with the eligible automatic-ID fallback, has no
   other projection blocker.

When the proof succeeds, skip only that unavailable region from generated effects
and generated `Entries`. Preserve its exact on-disk bytes and malformed child,
retain every other projection, and continue all global catalogue safety,
alias/collision, selected-scope, dependency, read, encoding, ownership, and
admission checks. Do not add a global warning to selected Update. A selected
no-op/preview and valid selected changes remain `completed` / Complete0. This is
not an arbitrary scope filter or early success: normal selected-plan
verification, ownership, no-op, recovery, invalid-input, and JSON-schema behavior
still run.

If any proof fact is missing or false, the existing strict result remains in
force. Selected or dependency metadata, native Skill metadata, unreadable or
encoding failures, incomplete or unrepresentable coverage, unsafe or ambiguous
boundaries, and any other projection blocker are not skipped or demoted.

## Ownership And Dependency Closure

Read the Extension receipts from the shared lock. Normalize supported ownership
fields with the forgiving ownership codec; schema metadata is non-binding.
Use canonical stable IDs, dependencies and paths to resolve selected ownership.
Publish one normalized UTF-8 lock transition after target effects verify,
preserving other sections and unselected receipts. Unchanged state writes nothing.
Missing, malformed or unreadable state grants no ownership; report the observation
without using a legacy fallback. Old files remain unrelated user content.

Resolve source manifests and dependency closure exactly, offline, transitively,
and within one source universe. Order dependencies before dependents. Reject
unknown IDs, duplicate active IDs or dependency declarations, invalid manifests,
cycles, unsafe package paths, incomplete closure, incompatible intended content,
and source identity conflicts before planning effects.

## Semantic Comparison

For each selected package path and owner set, collect current exact bytes and
semantic facts and compare current state with intended source. Distinguish unchanged, new, changed, missing, retired, shared, unknown,
and source-unavailable paths.

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware fingerprints that
preserve Unicode, headings, tags, links and destinations, marker meaning, inline
text, code blocks, and semantically significant whitespace. Normalize only line
endings and parser-proven formatting trivia. Generated `Entries` interiors are
derived navigation and excluded from authored package identity. Unsupported,
binary, and unparseable kinds use exact-byte identity and fail closed.

Compute current and intended fingerprints within the operation; persist neither. Capture
exact current bytes freshly for diff, expected-state revalidation, replacement,
deletion, verification, and recovery. Equal semantic identity with formatting-
only byte differences is an observation and does not create divergence or
shared-owner conflict. No formatter executes or produces persisted formatter
state.

## Ownership And Intended Topology

A managed package owns exact target-relative paths recorded in the trusted
section. Shared owners are explicit and retain compatible files when one owner
is removed later. Equal semantic content does not adopt an unowned existing file.
Retained dependents, route-host descendants, Framework ownership, user ownership,
unknown content, and other manager claims remain safety boundaries.

Form one hypothetical post-update workspace from current authored content plus
only effects admitted by the selected normal/force/prune authority. Preserve
user routes, overwrite companions, Framework files, unknown paths, and
intentionally absent defaults. Project affected generated regions from intended
authored topology and metadata through current Index behavior. Generated
interiors are not package-owned authored bytes and never come from stale package
lines. A missing or duplicate top-level `## Entries` section blocks; no hidden
index subprocess runs.

Reject payload targets at or beneath `.agents/open-forge.json`,
`.agents/open-forge.lock.json`, or `.agents/open-forge.lock`. Also reject
repository metadata, recovery/temporary artifacts, workspace overwrite companions, Framework blocks,
or another manager's paths. Source remains read-only.

An exact destination path claim in the lock Libraries section, or a real relative projection link at that
destination, is separately owned by Library management. Update never adopts,
overwrites, updates, or removes that destination in any mode, including normal
operation, `--force`, `--prune`, and combined `--force --prune`. The neutral
no-follow final-leaf guard blocks ordinary Extension `Create`, `Replace`,
`Delete`, or `ReplaceGeneratedRegion` when the leaf is a link or reparse point,
independently of whether the Library record is present, readable, valid, or
claims the path. Update does not reinterpret the Library record or invoke a
Library operation.

## Normal, Force, Prune, And Automatic Plans

Normal update compares current content with the selected source, replaces owned
current paths that differ, restores missing owned current paths, and creates safe
new paths. Formatting-only equivalence does not cause a replacement. An existing
unowned target is never adopted. Without `--prune`, retired paths and receipts
remain and produce completed-with-warnings. Retained shared owners block incompatible rewrites.

When the proven unrelated readable-metadata condition holds, a selected no-op or
preview and valid selected changes remain `completed` / Complete0. The skipped
region does not become an effect, warning, adoption, or health claim, and all
ordinary selected effects and verification remain unchanged.

`--force` remains accepted but adds no authority to overwrite owned current paths;
ordinary update already does that. It cannot bypass ownership, physical, route,
permission, expected-state, verification or recovery checks and never implies prune.

`--prune` releases selected retired ownership. It deletes a retired whole-file
path only when it exists as an ordinary contained file, the shared allow list
(with implicit `.agents` admission) admits it, reserved-path checks pass, and no
remaining owner or route dependency requires it. Changed content is eligible.
Absent paths are never deleted. Out-of-boundary claims cannot authorize deletion.
The verified recovery bundle precedes every deletion.

Automatic mode suppresses interaction but selects no package beyond explicit
IDs, `--all`, or the permitted single-package manifest-ID inference. It adds no
force, prune, adoption, ownership change, or safety bypass. It may apply ordinary
safe effects already authorized by the request and preserves/reports divergence.

## Plan, Preflight, And Dry-Run

The complete plan records source and target identity, dependency order, manifest
facts, current/intended fingerprints, current exact bytes, owner sets,
route and generated effects, lifecycle publication, expected-state guards,
affected paths, recovery-bundle readiness, per-effect verification, and final
cleanup handling. One failed selected condition blocks all effects. Before the
first workspace effect, obtain the actual OS lock for the persistent external
zero-byte path under `LocalApplicationData/OpenForge/locks/v1` defined by the
[Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no
metadata, timestamp, or ownership record. Hold one read/write `FileShare.None` handle;
existence is not lock ownership, and another process holding the handle blocks
mutation. A crash releases the OS lock. The lock is not lifecycle authority,
history, or recovery evidence.

Dry-run and apply share request, facts, source closure, intended state, plan,
preflight, and pre-effect status. Dry-run includes every selected safe,
force-authorized, and prune-authorized effect and bounded diff, then writes no
payload, generated region, lifecycle section, recovery bundle, or temporary
artifact. It cannot prove application-time verification, publication, or
recovery.

Because dry-run performs no effects, it never produces an apply-time `failed` or
`cancelled` result. A planning or read failure and caller cancellation before
effects retain their own event meaning.

## Application, Verification, And Recovery

Preflight checks the exact source, workspace, Framework anchor, lifecycle
section, package IDs, dependency closure, paths, owners, routes, generated
markers, containment, and expected bytes. For an applying plan, it also
prepares exactly one immutable ZIP recovery bundle outside the workspace whenever
the plan contains an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) or a permission-file Create.
The storage root is
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1`; there is no
temporary, repository, `HOME`, or custom platform fallback. Unavailable storage
is `incomplete` before any target effect.

The bundle is keyed by the normalized physical workspace path and operation ID.
Write a `CreateNew` draft under its exact name in the same external directory,
stream one source-generated schema-v1 `manifest.json` and ordered ordinal payload
entries, close and reopen it, validate the semantic schema, exact ordered entry
names and counts, lengths, hashes, and exact prior bytes, then move it within the
same directory to its deterministic final name and reopen/verify it again. The
manifest records command/operation/workspace
identity, ordered relative targets and change kinds, prior length/hash/payload,
and intended final absence or length/hash. Only the valid final ZIP forms the
opaque `RecoveryBundlePreparation`; the draft remains `Incomplete`. The bundle
is immutable thereafter.

Every planned existing-target effect has exactly one matching verified entry. Ordinary content Create and no-op effects have none. Permission-file
Create has a reversible prior-absence entry. All preparation completes before the first effect.
`FileChangeApplier` refuses an existing-target effect without its matching preparation
and performs one final effect per target. Immediately before effects, revalidate
all volatile facts. Any under-lease topology recomputation repeats the same
unrelated-readable-metadata proof from fresh typed facts and cannot broaden the
excluded region. Apply dependency-first payload and generated effects, then
publish the selected Extension ownership transition after target effects verify,
then verify the whole operation. Preserve unrelated sections and receipts.
A skipped lock publication reports that fact without claiming a record write.

A handled application, verification, publication or cancellation outcome stops
new effects and reports the actual residual draft or final path. The valid final
bundle remains after successful target and lock verification; verify its identity
and report its exact path. Retention is successful protection, not cleanup failure.
When `.git` is an ordinary directory, point at `git diff`; otherwise point at the
retained bundle. No Git executable or cleanliness check is required.
Never restore, roll back, compensate, infer current target state from recovery,
or create a journal or persisted plan. A final ZIP may remain after abrupt
termination without a crash or power-loss guarantee.

Verified final bundles from earlier operations may coexist with the current
operation's bundle. Preserve that history unchanged. Once current preparation
exists, exactly one catalogue candidate must match its identity; other verified
final bundles do not invalidate it. Incomplete drafts and malformed, unsupported
or unavailable candidates still block the fresh plan. Recovery history never
supplies current target state or ownership.

Explicit Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. Unknown or differently named artifacts remain untouched.
Same-path normalized
physical workspace rediscovery is deterministic; a workspace move is outside
the automatic guarantee, and Doctor/Cleanup may report orphan bundles for the
original root without auto-binding or restoring them. Recovery reads use strict
schema and exact-entry validation; no extraction or custom archive parser is
introduced.

## Result Formation And Conformance

Form one typed result with exact source/workspace, selected roots and closure,
trust/coverage, current/intended facts, safe/preserved/overwritten/
restored/deleted/shared effects, generated projection, lifecycle publication,
recovery-bundle/verification, status, and one next action. Human and JSON
renderers consume it once. Use the Interface seven statuses, ordinary precedence,
streams, and one-result JSON rule.

Conformance must cover source and selection, dependency-first closure, Framework
anchor and route-host gates, trusted/untrusted/absent/source-unavailable facts,
semantic identity, shared owners and retained dependents, Library-record and
projection collisions, independent no-follow final-leaf guards,
normal/force/prune/automatic effects, generated navigation, reserved paths, complete plan,
recovery-bundle behavior, expected-state revalidation, verification, interruption,
dry-run parity, no-op repetition, human/JSON parity, and no formatter or source
mutation. It must also cover the exact unrelated readable ordinary metadata proof,
preserved skipped bytes and malformed children, no selected-update global warning,
under-lease proof reuse, and strict refusal for selected/dependency, native Skill,
unreadable, encoding, incomplete, unsafe, ambiguous, unrepresentable, and other
projection failures. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated serialization, fixed Markdig
where used, real `System.IO`, Native AOT, OS locking, isolated tests, and package
journeys.

A recognized recovery residual does not block an effect-free repeat. Recovery
inspection must remain available and in-flight preparation must still match.
A plan containing new content or ownership effects retains the existing recovery
conflict check. A write-free repeat leaves the retained review bundle untouched.
