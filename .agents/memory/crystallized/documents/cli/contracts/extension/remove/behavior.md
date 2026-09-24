---
open-forge:
  description: Accepted technology-neutral Extension removal behavior for ownership release, shared owners, final-owner deletion, and recovery
  responsibility: Define remove's deterministic trusted-fact flow, complete deletion and release plan, verification, recovery, and result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Remove, Behavior, Ownership, Safety, Recovery, CurrentTruth]
---

# extension remove Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping
`open-forge extension remove`. It defines exact ID and workspace resolution,
trusted ownership and route facts, dependency and shared-owner checks,
final-owner removal classification and intent, generated
projection, complete planning, dry-run/application, verification, lifecycle
publication, recovery, result formation, and conformance. It does not choose
package source, schema, parser, storage, or implementation technology. The
[Shared Result Coordinates](../../shared/result-coordinates/interface.md) define
the exact shared JSON result schema and exit mapping, while the [CLI
Architecture](../../../architecture.md) defines the cross-cutting implementation
boundary; this behavior does not duplicate those
mechanics or claim their Gate 5 proof.

## Consumer Destination Permissions

Explicit non-dry-run `--allow-path` edits the shared authored settings after safe
planning and before admission; failure is reported and stops content application.
It is a separate authored edit and remains if later content fails. Interactive
always approval declares a settings effect covered by the operation's recovery
bundle. Once approves only this operation without saving a permission grant;
the removal exclusion is still saved. Cancel applies
nothing. The shared contract owns the exact reader, authoring and receipt rules.

Consume the [Workspace Permissions Interface](../../shared/workspace-permissions/interface.md) and
[Behavior](../../shared/workspace-permissions/behavior.md). Require shared allow-list admission for every selected owned external path, including
shared-owner retention. Derive these requirements
from trusted ownership without reading package source.
Existing `.agents/` targets need no grant; their prior safety and ownership
checks remain. Revocation blocks the complete selected lifecycle operation,
including ownership release, until exact explicit reapproval. Unrelated
installed packages do not enter this request's required set.

An eligible human apply request asks once for the complete missing set after
safe preflight. JSON, automatic, redirected and dry-run execution never ask the
permission question. An explicit non-dry-run `--allow-path` still authors a grant. Selection questions keep their separate rules. Selection never supplies permission.
Malformed or unsafe settings are never overwritten by approval.

Interactive always approval creates a declared settings-file create/replace effect. Revalidate the
observed settings and approved plan under the existing workspace lease. Cover
prior settings bytes or proven absence in the one verified operation bundle,
then persist and verify approval before content and lifecycle effects. Later
failure retains the grant and its actual outcome. Restoration is manual; no
new automatic Repair behavior follows.

## Complete Typed Flow

```text
validated managed IDs
  -> exact workspace and ownership-record state
  -> fresh ownership, dependency, route, generated, and current-byte facts
  -> shared retention, final-owner deletion, and missing-path release
  -> one complete ownership-release/removal plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> Extension-section publication and recovery-bundle disposition
  -> one typed result
```

Remove never applies a safe subset around a retained dependent, unsafe route,
ambiguous owner, unknown ownership, or unavailable recovery coverage.
Unavailable storage is `incomplete`; malformed, mismatched, or colliding bundle
facts are `blocked`.

## Request And Workspace Resolution

Resolve terminal help/version before workspace inspection. Parse exact stable
IDs; reject duplicate IDs, missing noninteractive selection, and unknown options.
`--prune` is an unknown option. `--automatic` disables prompts; `--dry-run`
previews without writes. Select the exact workspace under the shared workspace
contract. Prompt-capable human mode may ask for package IDs. There is no
changed-content policy question.

## Trust And Source-Independent Facts

Read Extension identities, dependencies, paths, and regions from
`.agents/open-forge.lock.json`. A proven-absent ownership source, or a valid
readable lock with no selected record, is known-empty. Record each valid selected
ID in `removedExtensions` even when it is not installed. Already absent and
excluded IDs can form a no-op. A malformed or unavailable ownership source/record is unknown:
return `incomplete` before dependency or path planning, with the explicit
ownership-record subject and raw cause, and perform no effects, writes, recovery,
or publication. Never fall back to legacy claims or treat unknown as empty.
Current payload and matching bytes never establish ownership. Package source may
be gone.

Derive shared owners from the per-extension path lists. If differently spelled
receipts alias the same portable path, report the uninterpretable ownership and
perform no effects; exact string lookup must not turn another owner's file into
final-owner content. Framework and Library
path receipts remain separate. A Library path is source-relative: map it under
its `destinationRoot` using the existing Library mapping rules before comparing
workspace destinations. An uninterpretable mapping yields an ownership
observation and no effects. Preserve unrelated lock sections when publishing
Extension release after verified target effects. Persist and verify the selected
IDs in `.agents/open-forge.json` before content changes. When a permission grant
is also saved, combine both settings changes against the original observation.
Settings and lock prior states enter the same verified recovery bundle
before deletion. Earlier state files remain untouched.

Install and Update honor `removedExtensions`, including dependency selection.
They also preserve excluded paths and directories without reclaiming or rewriting
them. Restore a package by explicitly clearing its ID and any covering path
exclusions, then installing it again. Removal never clears an exclusion.
When removal updates navigation, existing excluded hosts keep their exact bytes.

## Current Identity And Removal Classification

Capture fresh exact bytes and physical identity for eligible existing paths.
No persisted fingerprint or changed-content distinction participates.

- `shared`: release selected owners; retain the file for remaining owners.
- `final-owner`: delete the existing ordinary file after all checks and verified
  recovery preparation, including when its bytes were edited.
- `missing`: release the claim without a deletion effect.

The shared allow list applies to all owners, with implicit `.agents/` admission.
Reserved paths and ordinary-ancestor/physical-containment checks remain. Never
delete `.agents/open-forge.json`, `.agents/open-forge.lock.json`, or descendants
of either, even when a stale receipt claims them. Never delete a Framework or
Library path, a region-only host, or an unowned target.

A no-follow observation rejects links and reparse points independently of lock
readability. An unsafe or unauthorized target blocks the complete plan.

## Dependencies, Routes, And Intended Topology

Reject removal when a retained dependent would be stranded. A dependency that
becomes orphaned remains recorded and installed; no automatic orphan prune is
formed. Reject removal when a route host cannot be safely removed while retained
routed descendants depend on it.

Form one hypothetical post-remove workspace with selected ownership releases,
eligible final-owner deletions and preserved shared/user/Framework content. Project affected
generated `Entries` from the intended authored topology and metadata through the
Index contract. Generated interiors are derived navigation, not package-owned
authored bytes. Preserve valid markers and outside bytes; malformed boundaries
block and are never repaired.

The package source is never in the plan. Neither the Framework section nor
Framework-owned paths are Extension removal targets.

## Complete Plan

Form one complete immutable plan containing selected ownership release,
dependencies, path classifications and actions, generated topology, target
changes, state publication, expected-state guards, recovery targets, and final
verification. A final-owner path always maps to Delete; no prune authority or
Keep-as-unmanaged action remains.

## Preflight, Dry-Run, Application, And Recovery

Preflight validates IDs, dependencies, owner sets, current exact
facts, route and generated boundaries, cross-section preservation,
expected state, deletion safety, verification, and recovery-bundle readiness.
One failed condition blocks all effects. Before the first workspace effect,
obtain the actual OS lock for the persistent external zero-byte path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no metadata,
timestamp, or ownership record.
Hold a `FileShare.None` handle; existence is not lock ownership, and another
process holding the handle blocks mutation. A crash releases the OS lock. The
lock is not lifecycle authority, history, or recovery evidence.

Dry-run uses the same request, deletion intent, facts, plan, and preflight as
application. It shows selected ownership release, shared retention, final-owner deletion, generated effects, lifecycle
publication, and recovery-bundle requirements. It writes no file, lifecycle
section, recovery bundle, temporary artifact, or package source and cannot claim application
verification. It forms the same pre-effect planning status as application but
never produces an apply-time `failed` or `cancelled` result because it performs
no effects. A planning or read failure and caller cancellation before effects
retain their own event meaning.

Application first prepares exactly one immutable ZIP recovery bundle outside the
workspace whenever the plan has an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) or a permission-file Create. The storage root is
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

Every planned existing-target effect has exactly one matching verified entry. Ordinary content Create and no-op effects have none; permission-file
Create has a reversible prior-absence entry, and all preparation completes before the first effect.
`FileChangeApplier` requires matching preparation for each existing-target effect and
performs one final effect per target. Application revalidates all volatile facts,
applies safe dependency/ownership transitions and file/generated effects under
identity guards, verifies each and the complete postcondition, then publishes
the Extension section atomically while preserving unrelated sections
semantically. A selected lifecycle semantic change is source-generated as one
deterministic canonical UTF-8 whole-document representation; formatting,
ordering, and line-ending trivia may be normalized. A semantic no-op publishes
no lifecycle write.

A handled application, verification,
publication, or cancellation outcome stops new effects and reports the actual
residual draft or final path; a valid final remains when preparation completed.
A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. Never restore, roll back, compensate
for an effect, derive current target state from recovery provenance, or create a
journal, progress receipt, or persisted plan. After whole-operation success, re-read and verify the prepared bundle and retain
it. Return `retained` with the exact bundle path and no failure finding. Its
payload is the protection for edited bytes. A missing or invalid prepared bundle
fails final recovery verification. Existing recovery-catalogue conflict checks
remain for later mutations; users review the bundle before explicit Cleanup.
An absent-ID Remove no-op may complete without mutation despite a retained bundle.

Explicit Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. Unknown or differently named artifacts remain untouched.
Same-path normalized
physical rediscovery is deterministic; a workspace move is outside the
automatic guarantee, and Doctor/Cleanup may report orphan bundles for the
original root without auto-binding or restoring them. Recovery reads use strict
schema and exact-entry validation and do not extract bundles or add a custom
archive parser.

## Repeated Remove And Result Formation

No selected claims in a proven-absent source or valid readable lock means a
complete no-op with no deletion. Malformed or unavailable ownership is instead
`incomplete` with zero effects and writes; preserve the ownership-record subject
and raw cause at minimal. Missing is never an alias for unavailable. Do not
consult legacy claims to fill the gap.

Form one typed result with workspace, selection, dependencies, classifications,
shared retention, final-owner deletion, missing-path release, generated effects,
state publication, recovery path, verification, and findings. The Interface
owns presentation, statuses, streams, and JSON ordering. The `prune` field is
absent from both JSON views and human output.

## Behavioral Conformance

Prove source-independent lock selection, proven-absent no-op, malformed/
unavailable ownership incomplete zero-effect refusal with exact subject/cause,
retained-dependent blocking, shared ownership, missing-path release without a deletion effect, and complete refusal
of stale claims outside the shared destination boundary. Prove deletion rejects
missing verified preparation, all prior bytes exist in the finalized recovery
bundle before any deletion, and edited bytes remain recoverable after successful
application. Cover unsafe final leaves, reserved state files, no legacy fallback,
dry-run parity, absent-ID no-ops, and unknown-option rejection for `--prune`.
Run the supported managed and Native AOT verification gates.
