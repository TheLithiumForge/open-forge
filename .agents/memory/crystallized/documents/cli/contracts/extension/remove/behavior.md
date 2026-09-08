---
open-forge:
  description: Accepted technology-neutral Extension removal behavior for ownership release, shared owners, same-request prune, and recovery
  responsibility: Define remove's deterministic trusted-fact flow, complete deletion and release plan, verification, recovery, and result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Remove, Behavior, Ownership, Prune, Safety, Recovery, CurrentTruth]
---

# extension remove Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping
`open-forge extension remove`. It defines exact ID and workspace resolution,
trusted ownership and route facts, dependency and shared-owner checks,
semantic removal classification, Keep-as-unmanaged/Delete intent, generated
projection, complete planning, dry-run/application, verification, lifecycle
publication, recovery, result formation, and conformance. It does not choose
package source, schema, parser, storage, or implementation technology. The
[Shared Result Coordinates](../../shared/result-coordinates/interface.md) define
the exact shared JSON result schema and exit mapping, while the [CLI
Architecture](../../../architecture.md) defines the cross-cutting implementation
boundary; this behavior does not duplicate those
mechanics or claim their Gate 5 proof.

## Complete Typed Flow

```text
validated managed IDs and same-request prune intent
  -> exact workspace and lifecycle trust
  -> fresh ownership, dependency, route, generated, and current-byte facts
  -> Keep-as-unmanaged/Delete intended state
  -> one complete ownership-release/removal plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> Extension-section publication and recovery-bundle disposition
  -> one typed result
```

Remove never applies a safe subset around a retained dependent, unsafe route,
untrusted section, ambiguous owner, or unavailable recovery coverage. Unavailable
storage is `incomplete`; malformed, mismatched, or colliding bundle facts are
`blocked`.

## Request And Workspace Resolution

1. Resolve terminal help/version before lifecycle reads.
2. Parse one or more exact stable-ID operands, reject duplicates and unknown
   selectors, and reject source, `--all`, force, package path, and other flags.
3. Resolve `--prune`, `--automatic`, and `--dry-run` as independent idempotent
   Booleans. Any unknown option is invalid.
4. Select exactly CWD or exact `--workspace` with no discovery.
5. In prompt-capable human mode, allow the finite managed-ID and
   Keep-as-unmanaged/Delete wizard. In JSON/noninteractive mode, missing IDs are
   invalid and no prompt occurs.

Without `--prune`, noninteractive and automatic requests choose
Keep-as-unmanaged for changed final-owner files. With `--prune` in the same
request, they choose Delete for eligible changed final-owner files. Automatic
mode never adds that flag or selects Delete.

## Trust And Source-Independent Facts

Read the `extensions` section of `.agents/open-forge.lifecycle.json`, schema v1,
and preserve the `framework` section and common-envelope meaning semantically.
A selected semantic change emits one deterministic canonical UTF-8 whole-document
representation, so lifecycle property order, whitespace, and line endings may be
normalized. A semantic no-op writes nothing. Exact prior bytes for every
existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) are
captured in the verified operation recovery bundle. Require trusted exact
workspace binding, selected ID records, dependency
reciprocity, path/owner sets, semantic baseline fingerprints, route and generated
coverage, and safe cross-section preservation. The document stores no plan,
runtime history, journal, recovery evidence, or session. Files outside this exact
path are ordinary workspace content, not lifecycle input.

An absent document or section cannot prove a repeated remove no-op or grant
removal/prune authority. Missing, malformed, unsupported, unverifiable, or
inconsistent facts retain safe read-only facts where possible; return `incomplete`
for safe unavailable coverage and `blocked` for unsafe ambiguity.

No package source bytes are required. Current ownership and exact bytes come from
the selected workspace. A missing source therefore does not erase readable
lifecycle or baseline facts, but it also does not create trust where trust is
missing.

## Current Identity And Removal Classification

For every recorded package path, capture current exact bytes and semantic facts
freshly. Compare the current `open-forge-markdown-v1` conservative semantic
fingerprint with the persisted baseline
semantic fingerprint. Unsupported, binary, or unparseable content uses exact-byte
identity and fails closed. Formatting-only byte differences with equal semantic
identity are not divergence. Exact bytes remain necessary for plan, diff,
expected-state, deletion/write verification, and recovery.

Classify each path as:

- **shared:** another trusted owner remains. Release only the selected owner and
  retain the physical file.
- **unchanged final owner:** selected ownership is the final trusted owner and
  current semantic identity matches baseline. It may be deleted under ordinary
  remove authority after all checks.
- **changed final owner:** selected ownership is final but current semantic
  identity differs from baseline. Keep it as unmanaged by default; delete only
  when same-request `--prune` supplies Delete authority.
- **missing or already absent:** release recorded ownership only when lifecycle
  facts prove the selected ownership. Do not infer prior success from absence.
- **unknown, unowned, other-manager, Framework-owned, route-unsafe, or
  ambiguous:** do not claim or delete; block the complete plan.

An exact current path or matching fingerprint without a trusted recorded owner
never becomes managed during removal.

An exact destination path claim in the consumer Library record
`.agents/open-forge.libraries.json`, or a real relative projection link at that
destination, is separately owned by Library management. Remove never adopts,
overwrites, updates, or removes that destination in any removal mode, including
ordinary removal, same-request `--prune`, and Keep-as-unmanaged. The neutral
no-follow final-leaf guard blocks ordinary Extension `Create`, `Replace`,
`Delete`, or `ReplaceGeneratedRegion` when the leaf is a link or reparse point,
independently of whether the Library record is present, readable, valid, or
claims the path. Remove does not reinterpret the Library record or invoke a
Library operation.

## Dependencies, Routes, And Intended Topology

Reject removal when a retained dependent would be stranded. A dependency that
becomes orphaned remains recorded and installed; no automatic orphan prune is
formed. Reject removal when a route host cannot be safely removed while retained
routed descendants depend on it.

Form one hypothetical post-remove workspace with selected ownership releases,
permitted unchanged final-owner deletions, selected changed final-owner Delete
effects, and preserved shared/unmanaged/user/Framework content. Project affected
generated `Entries` from the intended authored topology and metadata through the
Index contract. Generated interiors are derived navigation, not package-owned
authored bytes. Preserve valid markers and outside bytes; malformed boundaries
block and are never repaired.

The package source is never in the plan. Neither the Framework section nor
Framework-owned paths are Extension removal targets.

## Complete Plan And Same-Request Prune

The plan contains selected IDs, retained dependents, dependency edges, exact
owner sets, path classifications, semantic/current bytes, Keep/Delete intent,
ownership-release effects, file deletion effects, generated projection,
lifecycle publication, expected-state guards, recovery-bundle readiness,
verification, and final cleanup handling.

`--prune` is resolved before planning and applies only to changed final-owner
content that independently passes every identity, ownership, route,
containment, verification, and recovery-bundle gate. It cannot be added
by a later invocation after ownership is released. Once a path is released as
unmanaged, later prune has no authority to act on it.

## Preflight, Dry-Run, Application, And Recovery

Preflight validates lifecycle trust, IDs, dependencies, owner sets, current exact
and semantic facts, route and generated boundaries, cross-section preservation,
expected state, deletion safety, verification, and recovery-bundle readiness.
One failed condition blocks all effects. Before the first workspace effect,
obtain the actual OS lock for the persistent external zero-byte path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no metadata,
timestamp, or ownership record.
Hold a `FileShare.None` handle; existence is not lock ownership, and another
process holding the handle blocks mutation. A crash releases the OS lock. The
lock is not lifecycle authority, history, or recovery evidence.

Dry-run uses the same request, Keep/Delete intent, facts, plan, and preflight as
application. It shows selected ownership release, shared retention, unchanged
deletion, changed preservation or prune deletion, generated effects, lifecycle
publication, and recovery-bundle requirements. It writes no file, lifecycle
section, recovery bundle, temporary artifact, or package source and cannot claim application
verification. It forms the same pre-effect planning status as application but
never produces an apply-time `failed` or `interrupted` result because it performs
no effects. A planning or read failure and caller cancellation before effects
retain their own event meaning.

Application first prepares exactly one immutable ZIP recovery bundle outside the
workspace whenever the plan has an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`). The storage root is
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1`; temporary,
repository, `HOME`, and custom platform fallbacks are forbidden, and unavailable
storage is `incomplete` before any target effect.

Use the normalized physical workspace path key and operation ID for the final
bundle name. Stream a source-generated schema-v1 `manifest.json` and ordered
ordinal payload entries into a `CreateNew` draft under its exact name in the same
external directory. Close, reopen, and validate the semantic schema, exact
ordered entry names and counts, lengths, hashes, and exact prior bytes; move the
draft within the same directory to the deterministic final name and reopen/
verify it again. The manifest records command/operation/workspace
identity, ordered relative targets and change kinds, prior length/hash/payload,
and intended final absence or length/hash. Only the valid final ZIP forms the
opaque `RecoveryBundlePreparation`; the draft remains `Incomplete`. The bundle
is immutable thereafter.

Every planned existing-target effect has exactly one matching verified entry. Create and
no-op effects have none, and all preparation completes before the first effect.
`FileChangeApplier` requires matching preparation for each existing-target effect and
performs one final effect per target. Application revalidates all volatile facts,
applies safe dependency/ownership transitions and file/generated effects under
identity guards, verifies each and the complete postcondition, then publishes
the Extension section atomically while preserving unrelated sections
semantically. A selected lifecycle semantic change is source-generated as one
deterministic canonical UTF-8 whole-document representation; formatting,
ordering, and line-ending trivia may be normalized. A semantic no-op publishes
no lifecycle write.

Before post-verification deletion begins, a handled application, verification,
publication, or cancellation outcome stops new effects and reports the actual
residual draft or final path; a valid final remains when preparation completed.
A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. Never restore, roll back, compensate
for an effect, derive current target state from recovery provenance, or create a
journal, progress receipt, or persisted plan. After final verification of
whole-operation success, delete the bundle. `Deleted`/`Removed` permits normal
completion. `Failed`/positively observed `Retained` keeps target effects
successful and produces `attention`, the exact residual path, and cleanup
guidance.
`Failed`/`Unknown` produces `failed` and reports
an exact expected path only when the deletion result provides one.
When `Failed`/positively observed `Retained` recovery attention coexists with a
finite non-blocking fact, cleanup guidance owns the single next action; the
other fact remains visible evidence.

Explicit Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. Unknown or differently named artifacts remain untouched.
Same-path normalized
physical rediscovery is deterministic; a workspace move is outside the
automatic guarantee, and Doctor/Cleanup may report orphan bundles for the
original root without auto-binding or restoring them. Recovery reads use strict
schema and exact-entry validation and do not extract bundles or add a custom
archive parser.

## Repeated Remove And Result Formation

A repeated remove is `complete` as a verified no-op only when a valid trusted
current lifecycle section proves the selected ID and all selected ownership are
absent. A missing document or section, malformed, unsupported, or untrusted
evidence is `incomplete` or `blocked`, never presumed success.

Form one typed result with exact workspace, IDs, trust/coverage, retained
dependents, owner/path classifications, Keep/Delete choice, releases, deletions,
shared retention, preserved unmanaged paths, generated effects, lifecycle
publication, verification, recovery-bundle status, and one next action. Human and
JSON renderers consume it once. Use the Interface status and stream rules.

## Behavioral Conformance

Conformance must cover exact ID/workspace resolution, wizard/direct/automatic
choice, source independence, trusted/untrusted/absent lifecycle, no-op proof,
dependency and route-host blocking, shared-owner release, unchanged final-owner
deletion, changed Keep-as-unmanaged, same-request prune Delete, Library-record
and projection collisions, independent no-follow final-leaf guards, later-prune
refusal, unknown/unowned/Framework preservation, semantic fingerprints,
generated projection, complete plan, recovery-bundle behavior, revalidation,
verification, dry-run no-effects, statuses/streams/JSON, and
package-source preservation. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.
