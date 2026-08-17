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
accepted CLI Architecture defines the exact shared JSON result schema and exit
mapping and the implementation boundary; this behavior does not duplicate those
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
  -> Extension-section publication or reverse guarded recovery
  -> one typed result
```

Remove never applies a safe subset around a retained dependent, unsafe route,
untrusted section, ambiguous owner, or unavailable recovery fact.

## Request And Workspace Resolution

1. Resolve terminal help/version before lifecycle reads.
2. Parse one or more exact stable-ID operands, reject duplicates and unknown
   selectors, and reject source, `--all`, force, package path, and other flags.
3. Resolve `--prune`, `--automatic`, `--dry-run`, and `--skip-git-check` as
   independent idempotent Booleans.
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
and preserve the `framework` section and common envelope bytes and meaning.
Require trusted exact workspace binding, selected ID records, dependency
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
lifecycle publication, expected-state guards, Git, backup, verification, and
reverse recovery.

`--prune` is resolved before planning and applies only to changed final-owner
content that independently passes every identity, ownership, route,
containment, Git, backup, verification, and recovery gate. It cannot be added
by a later invocation after ownership is released. Once a path is released as
unmanaged, later prune has no authority to act on it.

## Preflight, Dry-Run, Application, And Recovery

Preflight validates lifecycle trust, IDs, dependencies, owner sets, current exact
and semantic facts, route and generated boundaries, cross-section preservation,
expected state, Git policy, backup readiness, deletion safety, verification, and
recovery. One failed condition blocks all effects. Before the first workspace
effect, obtain the actual OS lock for the visible `.agents/open-forge.lock` path
defined by the accepted CLI Architecture. File existence is not lock ownership;
another process holding the lock blocks mutation. A crash releases the OS lock,
and an unlocked file is reusable and may be manually removed only when no
process is active. The lock is not lifecycle authority, history, or recovery
evidence.

Dry-run uses the same request, Keep/Delete intent, facts, plan, and preflight as
application. It shows selected ownership release, shared retention, unchanged
deletion, changed preservation or prune deletion, generated effects, lifecycle
publication, and recovery requirements. It writes no file, lifecycle section,
backup, temporary artifact, or package source and cannot claim application
verification. It forms the same pre-effect planning status as application but
never produces an apply-time `failed` or `interrupted` result because it performs
no effects. A planning or read failure and caller cancellation before effects
retain their own event meaning.

Application revalidates all facts immediately before effects. Apply safe
dependency/ownership transitions and file/generated effects under identity
guards, verify each and the complete postcondition, then publish the Extension
section atomically while preserving unrelated sections. Remove backups only after
complete verification.

On failure, stop new effects and reverse applied effects in reverse order only
while identity guards match. Preserve unexpected concurrent edits and residual
backups. Recovery failure is `failed`; caller cancellation without stronger
failure is `interrupted`. A later remove forms a fresh plan and never replays a
saved plan or assumes prior consumption.

## Repeated Remove And Result Formation

A repeated remove is `complete` as a verified no-op only when a valid trusted
current lifecycle section proves the selected ID and all selected ownership are
absent. A missing document or section, malformed, unsupported, or untrusted
evidence is `incomplete` or `blocked`, never presumed success.

Form one typed result with exact workspace, IDs, trust/coverage, retained
dependents, owner/path classifications, Keep/Delete choice, releases, deletions,
shared retention, preserved unmanaged paths, generated effects, lifecycle
publication, verification, recovery, status, and one next action. Human and JSON
renderers consume it once. Use the Interface status and stream rules.

## Behavioral Conformance

Conformance must cover exact ID/workspace resolution, wizard/direct/automatic
choice, source independence, trusted/untrusted/absent lifecycle, no-op proof,
dependency and route-host blocking, shared-owner release, unchanged final-owner
deletion, changed Keep-as-unmanaged, same-request prune Delete, later-prune
refusal, unknown/unowned/Framework preservation, semantic fingerprints,
generated projection, complete plan, Git/backup/recovery, revalidation,
verification, reverse recovery, dry-run no-effects, statuses/streams/JSON, and
package-source preservation. The shared CLI Architecture defines the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.
