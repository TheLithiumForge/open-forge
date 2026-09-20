---
open-forge:
  description: Accepted technology-neutral catalogue, deletion, safety, monotonic recovery, and conformance for cleanup
  responsibility: Define how cleanup forms one recognized default-all plan, deletes only verified eligible recovery bundles and drafts, and reports partial results
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Cleanup, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# Cleanup Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for the non-shipping
root `open-forge cleanup` operation. It defines deterministic request and
workspace resolution, recognized bundle/draft catalogue formation, complete planning,
preflight, dry-run, deletion, verification, the cleanup-specific monotonic
recovery exception, result formation, and technology-neutral conformance.

The [Interface Contract](interface.md) owns public syntax, default-all
selection, catalogue scope, consent, observable effects, statuses, output,
errors, examples, non-goals, and public verification. This file does not add
operands, selectors, flags, artifact names, storage schema, JSON fields, numeric
exits, or implementation technology.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the accepted shared structured schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines source and runtime boundaries,
BCL-first filesystem structure, the workspace-lock boundary, and recovery
identity relationships.
[Task 20: Cleanup](../../../../../archived/cli-development/tasks/operations/cleanup.md)
records the development implementation and execution evidence. The shared
[Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md) owns
shared request, workspace, presentation, terminal, and repetition meaning.
Doctor, Repair, Index, lifecycle, package, and Gate 6 sources retain their own
boundaries; cleanup does not invoke them as public or hidden operations.

## Operation Flow And Invariants

Cleanup follows one complete typed flow:

```text
validated command input
  -> exact selected workspace and external recovery root
  -> complete current filtered exact-name candidate catalogue
  -> deterministic ordered candidate plan
  -> preflight
  -> dry-run, empty no-op, or same-workspace lease acquisition
  -> final under-lease catalogue and expected-state revalidation
  -> per-bundle-or-draft revalidation and deletion
  -> per-effect verification and monotonic result formation
  -> one typed result
```

The operation satisfies these invariants:

- Bare `cleanup` catalogues all exact-name recovery final and draft candidates
  associated with the selected workspace. It never converts the
  request into artifact selection, a wizard, prompting, or a broad filesystem
  delete.
- An eligible item is either a verified final ordinary file or an ordinary
  exact-name draft in the selected normalized workspace bucket and passes final
  validation while Cleanup holds the same-workspace `WorkspaceLockLease`.
- A final ZIP passes semantic source-generated current schema-v1 manifest
  validation, including the required immutable typed attribution, admissible
  typed entry state pairs, exact ordered entry names and counts, declared
  lengths and hashes, and exact payload-byte checks. This includes an ordinary
  typed `ordinary-create` entry whose prior state is `Missing` and has no prior
  payload, and typed `relative-file-link-create` and
  `relative-file-link-delete` entries whose identity is the exact
  `relative-file-symbolic-link` kind and raw `/`-separated relative target;
  absolute targets are invalid. A Library final's valid attribution tuple is
  exactly `producer: library` with one of `operation: attach`,
  `operation: sync`, or `operation: detach`, and
  `subject.kind: workspace` with
  `subject.identity` equal to the selected workspace key. A draft is exact-name,
  path-only `Incomplete` support data and never forms preparation; observers do
  not inspect or use its bytes for attribution.
- The schema discriminator is exactly `1`. A schema-1 final
  missing or carrying invalid attribution is malformed/unattributed and remains
  preserved; when it is a readable ordinary final, it is a warning preservation
  entry that may coexist with independently eligible removals, but it is never
  deleted. An unreadable or non-ordinary malformed final, and every unsupported
  or unavailable final, remains a blocking preserved candidate. No final is
  migrated, rewritten, repaired, adopted, or inferred. An unknown schema version
  is unsupported. Cleanup recognizes only current schema-v1 and has no v2, dual
  reader, compatibility, or migration path. Attribution is an integrity fact,
  not deletion authority.
- The exact schema-v1 attribution vocabulary, valid producer/operation/subject
  combinations, and required non-null workspace identity are defined by the
  [Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
  Cleanup accepts no unknown value or fallback attribution.
- Cleanup validates Library recovery entries as evidence only. It never extracts
  or follows a relative link, reads source bytes as payload, applies or restores
  an entry, invokes Repair, or deletes a Library projection, Library record, or
  source path.
- A suffix, age, extension, location, proximity, temporary-looking name, path,
  or matching bytes alone never establishes provenance or authority.
- A readable ordinary malformed exact-name final is reported and preserved as a
  warning and does not block independently eligible deletion. Unsupported,
  unavailable, unreadable, non-ordinary, or unsafe exact-name candidates are
  reported and preserved and block deletion. Unknown, user-created, or
  differently named items remain outside the filtered catalogue and its equality
  checks. If Cleanup cannot acquire the workspace lease because of contention,
  it performs no deletion.
- Workspace files, raw evidence and snapshots, source and managed content,
  workspace settings, ownership locks and receipts, generated navigation,
  build outputs,
  package caches, logs, unknown support items, and arbitrary filesystem content
  remain outside the catalogue. Cleanup never recursively removes a support
  artifact tree.
- The complete catalogue and deterministic ordered plan exist before the first
  effect. Dry-run and application use the same request, facts, plan, ordering,
  expected-state facts, and preflight.
- An empty catalogue is a verified complete no-op without a lease. No deletion
  begins until Cleanup acquires the same-workspace lease, re-enumerates the
  selected bucket once, filters exact final and draft names, and compares that
  candidate set and its relevant path, kind, and integrity facts with the planned
  catalogue. A new, removed, or changed exact-name candidate or failure to acquire
  the lease produces no deletion; unknown names remain outside the comparison.
- Once deletion begins, verified deletion is monotonic. Cleanup does not create
  a recovery bundle, staging copy, receipt, journal, or tombstone merely to delete
  an eligible bundle or draft and does not reverse a deletion it has verified.

The last invariant is a cleanup-only support-artifact exception. Other CLI
writes retain their accepted immutable recovery-bundle and residual-preservation
rules.

## Request And Workspace Resolution

Request resolution:

1. Accepts only direct root `cleanup` with `--dry-run` and shared global flags.
2. Rejects operands, artifact IDs and paths, selectors, profiles, age or glob
   expressions, aliases, unknown flags, and generic apply or delete forms.
3. Resolves shared `--help` and `--version` before workspace selection or domain
   work. Command-specific input with either terminal mode is invalid.
4. Collapses repeated `--dry-run` presence to one idempotent Boolean choice.
   Shared flag repetition retains its own contract.
5. Selects exactly the process current working directory or the exact
   `--workspace <path>` value. It does not search parent directories, choose a
   Git or package root, follow a nested `.agents` directory, or infer a
   workspace from an artifact path.

Human, TTY, JSON, and other non-interactive requests reach the same normalized
domain request. No request mode prompts or supplies a missing selector because
cleanup has no selector.

The selected workspace must be a safely accessible directory when cleanup needs
to inspect its association facts. Recovery storage is exactly
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1`. This
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. No temporary, workspace, `HOME`, or custom platform fallback is
permitted. An absent application-data root or recovery store produces an empty
catalogue and a verified complete no-op. An existing root or store that is
unavailable or unsafe to inspect is `incomplete` and prevents deletion; access
failure is not absence. The operation reports the workspace and selection method
in its typed result. It does not
treat workspace selection as proof of installation, health, ownership, or
artifact provenance.

Cleanup uses the existing `WorkspaceLockLease` for cooperating-process
exclusion. It writes no marker, PID, journal, or lock metadata, never deletes
the lock file, and makes no activity inference. Exact lock identity, storage,
handle, and lifetime mechanics follow the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md).

## Current Facts And Catalogue Coverage

The resolver establishes a complete current catalogue by filtering exact
selected-workspace final and draft names, then determining each candidate's kind
and integrity. The [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines the realization
of names, storage, and schema; the observable facts below are fixed by this
Behavior Contract.

For every exact-name candidate admitted to the catalogue, current facts include:

- final or draft kind and exact-name Open Forge provenance;
- the associated operation, valid immutable typed attribution for a verified
  current-v1 final, and ordered target entries when the bundle requires them;
- exact association with the selected normalized physical workspace path;
- exact direct-child path, deterministic name, and current file kind;
- the deletion verification condition; and
- `Verified`, `Malformed` (including unattributed), `Unsupported`, or `Unavailable`
  semantic integrity for a final, or the exact `Incomplete` draft name and
  ordinary-kind fact for a draft. Draft facts remain path-only.

The verified current-v1 facts retain each typed entry's admissible state pair.
That includes a prior-missing ordinary `Create` without a prior payload and a
relative-file-link `Create` or `Delete` identity consisting of its exact link
kind and raw `/`-separated relative target. A verified Library final retains its
`library`/`attach`, `library`/`sync`, or `library`/`detach` attribution and the
trusted selected-workspace subject used for validation.

Strict final-bundle recognition may stream each ZIP payload entry through fixed
bounded buffers solely to validate the exact declared length and lowercase
SHA-256. Prior-missing ordinary `Create` entries and relative-file-link
`Create` and `Delete` entries have no source payload; link validation uses only
the recorded kind and raw `/`-separated relative target. Cleanup never extracts,
discloses, renders, logs, returns, retains, or materializes payload bytes, and
validation memory remains independent of payload size.

The filtered candidate catalogue includes:

1. every exact deterministic final name, with semantic validation producing
   `Verified`, `Malformed`, `Unsupported`, or `Unavailable`; and
2. every exact deterministic draft name in the selected workspace bucket, with
   ordinary direct-child files treated as `Incomplete` support data.

The resolver does not use age, extension, suffix, location, proximity,
temporary-looking names, directory membership, or matching bytes as a substitute
for positive provenance. It does not recursively classify arbitrary support
trees.

Unknown or differently named material is preserved and excluded from the
catalogue and its equality checks. If the selected workspace bucket cannot be
enumerated, result formation uses `incomplete`. A readable ordinary malformed
final remains in the catalogue, is excluded from the deletion plan, and is a
warning preservation entry; independently eligible deletion may continue. An
unsupported, unavailable, unreadable, non-ordinary, or unsafe exact-name
candidate remains in the catalogue, is excluded from the deletion plan, and
blocks every deletion.

Cleanup does not require interpretation of user content. The exact selected
workspace bucket, deterministic direct-child name, semantic final validation
when applicable, explicit command intent, and held same-workspace lease provide
deletion authority. Attribution contributes only to final integrity and does not
provide deletion authority. Only verified current-v1 final ordinary files and
ordinary exact-name drafts are eligible.

## Selection And Plan Formation

The planner selects every verified final ordinary file and ordinary exact-name
draft. It retains a readable ordinary malformed final as a non-deletion warning
preservation entry so independently eligible items can proceed. It does not
select a subset to avoid a blocking exact-name candidate, rank items by age or
name, or treat a recommendation as input.

It forms one deterministic ordered plan containing, for each selected bundle or
draft:

- the exact direct-child path, deterministic name, and bundle or draft kind;
- the required same-workspace lease boundary and final integrity condition;
- the deletion effect and exact ordering position; and
- the immediate revalidation and post-deletion verification conditions.

The plan contains no recovery-bundle-creation effect, staging effect, lifecycle
effect, receipt publication, journal, tombstone, or hidden command invocation.
Each planned verified final or ordinary draft is one eligible deletion boundary
only at its exact deterministic path; Cleanup never broadens that boundary to a
containing directory, follows an entry or link, or removes unknown or
user-created content.

No exact-name candidate is silently omitted. A readable ordinary malformed final
remains reported and preserved, is excluded from the deletion plan, and does not
block independently eligible deletion. An unsupported, unavailable, unreadable,
non-ordinary, or unsafe candidate remains reported and preserved, is excluded
from the deletion plan, and blocks all deletion. A catalogue coverage failure
before effects is `incomplete` and has no write.

## Preflight And Dry Run

Preflight validates the complete plan before the first deletion. It checks every
exact direct-child path and ordinary file kind, semantic final-ZIP validation
where applicable, and deletion verification condition. Before lease acquisition,
it does not claim final deletion eligibility.

Dry-run consumes the exact application request, current candidate catalogue,
plan, ordering, and preflight. It lists every exact-name candidate, its integrity
condition, and either its eligible deletion effect or its blocking preservation
condition. Every planned effect remains contingent on application acquiring the
same-workspace lease and passing final under-lease validation. Dry-run then stops
before lease acquisition, deletion, or any persistent effect. It does not create
a recovery bundle or draft, temporary file, staging copy, receipt, journal,
   tombstone, or residual marker. A readable ordinary malformed final remains a
   warning preservation condition in the result; planned deletions alone do not
   form `completed-with-warnings`.

## Application, Revalidation, And Verification

When application is selected:

1. If the current candidate catalogue is empty, return the verified complete
   no-op without acquiring a lease.
2. Acquire one live `WorkspaceLockLease` for the exact selected workspace and
   hold it through all deletion and verification work. If acquisition fails or a
   cooperating mutator owns the lease, perform no deletion.
3. Under that lease, re-enumerate the selected workspace bucket once, filter the
   same exact final and draft names, and compare the candidate set and its
   relevant path, kind, and integrity facts with the planned catalogue. A new,
   removed, or changed exact-name candidate blocks; unknown or differently named
   items remain outside the comparison.
4. Immediately before each deletion, repeat semantic validation for a final ZIP
   or the exact path/name/kind check for a draft.
5. If the filtered under-lease candidate set differs from the planned catalogue
   or validation fails before the first deletion, stop with `blocked` and no
   deletion. A later ordinary deletion or verification failure stops further
   effects and reports prior verified deletions and remaining paths.
6. Delete only the exact current path with ordinary `File.Delete`.
7. Verify absence immediately after each effect, then retain the exact deleted,
   remaining, preserved, and residual paths in the typed result.

Cleanup does not reverse a verified deletion. An unexpected deletion or
verification failure after effects begin stops further effects and forms
`failed`; already verified deletions remain desired effects. A caller
cancellation forms `cancelled` when no stronger unsafe residual condition
applies, with every verified deletion and every remaining or preserved artifact
reported. A later invocation never reuses this plan.

The operation does not create a recovery bundle or draft to justify or reverse
its own eligible bundle or draft deletions. This is the only accepted monotonic
recovery exception and does not alter the recovery behavior of other operations.

## Idempotence And Fresh Repetition

After successful application, a repeat resolves a fresh current filtered
catalogue. If no exact-name candidate exists, it forms a complete verified no-op
without prompting and without a mutation path. A blocking exact-name candidate
is not an empty no-op merely because it is ineligible. Cleanup does not use a
receipt, tombstone, journal, saved plan, absence alone, or prior result to
manufacture provenance.

Unknown, differently named, and otherwise ineligible content remains excluded by
the fresh catalogue and untouched.

## Result Formation And Status

One typed result records the selected workspace, normalized request, complete
filtered exact-name candidate-catalogue facts, deterministic eligible plan,
preflight, lease acquisition and final under-lease candidate/fact comparison when
application reaches deletion, every planned effect, every verified deletion,
remaining and preserved items, residual facts, verification or cancellation
events, and semantic status. The result does not claim that cleanup interpreted
content or migration intent.

Result formation follows the Interface meanings:

- A complete dry-run plan, a complete verified application, and a complete
  verified empty catalogue are `completed` unless a retained readable ordinary
  malformed final supplies the accepted warning condition.
- A readable ordinary malformed final is preserved and reported as a warning;
  independently verified eligible removals may complete, producing
  `completed-with-warnings` (Attention2/exit 2). Planned deletions or other
  preserved exclusions alone do not create that status.
- Safe but incomplete required catalogue enumeration is `incomplete` before
  effects.
- Invalid request input is `invalid-input` before catalogue work.
- Failure to acquire the required same-workspace lease; an unsupported,
  unavailable, unreadable, non-ordinary, or unsafe exact-name candidate; or a
  new, removed, or changed exact-name candidate or relevant fact under lease is
  `blocked` when safe classification is available. A readable ordinary
  malformed final is the narrow warning exception above and is not `blocked` by
  itself.
- An unexpected partial deletion or verification event is `failed`.
- Cancellation without a stronger unsafe residual condition is `cancelled`.

For ordinary conditions, precedence from strongest to weakest is `blocked`,
`incomplete`, `completed-with-warnings`, then `completed`. Invalid input stops
first. `failed` and `cancelled` retain their event meaning. The shared
process-status mapping is
defined by the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

## Presentation Relationship

Minimal, standard, full, debug, and JSON renderers consume the same typed result.
They do not rerun catalogue enumeration, planning, preflight, deletion, or
verification. Presentation may change framing or density only; it cannot change
the selected default-all catalogue, hide an exact effect, change status, or
erase a remaining or residual item.

Primary human `completed`, `completed-with-warnings`, and `incomplete` results use stdout.
Primary human `invalid-input`, `blocked`, `failed`, and `cancelled` results use
stderr. JSON emits one complete result on stdout for every semantic status, and
bounded diagnostics use stderr. The exact structured schema is defined by the
[Shared Result Coordinates](../shared/result-coordinates/interface.md).

## Behavioral Conformance

A conforming implementation must demonstrate:

- current-v1 typed recovery-entry validation, including a prior-missing ordinary
  `Create` without a prior payload and relative-file-link `Create` or `Delete`
  identity, plus Library `library`/`attach`, `library`/`sync`, and
  `library`/`detach` attribution with a trusted `workspace` subject matching the
  selected workspace key; no link following, source-byte payload reads, entry
  application or restoration, Repair invocation, or Library projection, record,
  or source deletion;
- exact request normalization, no operands, no wizard or prompts, shared flag
  behavior, terminal modes, and exact workspace selection without discovery;
- exact selected-workspace final and draft names, every exact candidate's current
  kind and integrity, and incomplete or completed residual state;
- exact direct-child current file kind, one under-lease re-enumeration, and
  filtered equality of exact-name candidates and relevant path, kind, and
  integrity facts, plus preservation and comparison exclusion of all unknown,
  user-created, differently named, differently keyed, and excluded content;
- representative payload validation with exact declared
  lengths and hashes, bounded buffers and memory independent of entry size, and
  no extraction, disclosure, retention, or materialization;
- default-all filtered exact-name catalogue formation, deterministic ordering,
  current-v1 attribution validation, plan eligibility only for verified finals
  and ordinary drafts, warning preservation of a readable ordinary malformed
  final alongside independent eligible removal, blocking preservation of
  unsupported, unavailable, unreadable, non-ordinary, and unsafe exact-name
  candidates, no arbitrary recursive support-artifact cleanup, and no hidden
  command;
- exact dry-run/application candidate-plan parity and no persistent dry-run
  effects;
- unavailable external recovery storage, deterministic bundle identity, and no
  workspace-local support-artifact scan;
- no-lease empty no-op, one live same-workspace `WorkspaceLockLease` before every
  deletion, one under-lease filtered-candidate and relevant-fact comparison, no
  effect from unknown names, no deletion on lease contention, and no marker,
  PID, journal, or lock-metadata write;
- immediate final semantic validation, ordinary `File.Delete`, absence
  verification, monotonic deletion, no reverse of
  verified effects, failure and interruption reporting, residual visibility, and
  fresh-catalogue rerun convergence;
- complete no-op repetition without deleting a later replacement;
- all seven statuses, the accepted reachable `completed-with-warnings` warning
  condition, ordinary precedence, stream assignment, one-result JSON, exact
  effect visibility, and bounded diagnostics; and
- no Doctor, Repair, Index, Framework or Extension lifecycle, package, build,
  arbitrary filesystem, or Gate 6 documentation/history/release cleanup.

Executable proof uses real isolated workspaces and real filesystem
identity, workspace association, and external-root boundaries. The CLI
Architecture defines the accepted parser, filesystem, concurrency, package, and
source boundaries. The [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines artifact naming
and schema; this Behavior Contract does not select another implementation.

## Related Sources

- [Cleanup Interface Contract](interface.md)
- [Cleanup Command Contract Set](_cleanup.md)
- [Global CLI Flags Behavior](../shared/global-flags/behavior.md)
- [Doctor Behavior Contract](../doctor/behavior.md)
- [Repair Behavior Contract](../repair/behavior.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
