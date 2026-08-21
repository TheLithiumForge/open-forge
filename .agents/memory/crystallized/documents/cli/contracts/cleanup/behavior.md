---
open-forge:
  description: Accepted technology-neutral catalogue, deletion, safety, monotonic recovery, and conformance for cleanup
  responsibility: Define how cleanup forms one recognized default-all plan, deletes only verified eligible cleanup artifacts, and reports partial results
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Cleanup, Behavior, Mutation, Recovery, Safety, Determinism, CurrentTruth]
---

# Cleanup Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for the non-shipping
root `open-forge cleanup` operation. It defines deterministic request and
workspace resolution, recognized-artifact catalogue formation, complete planning,
preflight, dry-run, deletion, verification, the cleanup-specific monotonic
recovery exception, result formation, and technology-neutral conformance.

The [Interface Contract](interface.md) owns public syntax, default-all
selection, catalogue scope, consent, observable effects, statuses, output,
errors, examples, non-goals, and public verification. This file does not add
operands, selectors, flags, artifact names, storage schema, JSON fields, numeric
exits, or implementation technology.

The [CLI Architecture](../../architecture.md) defines the accepted shared
structured schema, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, workspace lock, and recovery identity
model. Implementation and executable proof remain pending Gate 5. The shared
[Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md) owns
shared request, workspace, presentation, terminal, and repetition meaning.
Doctor, Repair, Index, lifecycle, package, and Gate 6 sources retain their own
boundaries; cleanup does not invoke them as public or hidden operations.

## Operation Flow And Invariants

Cleanup follows one complete typed flow:

```text
validated command input
  -> exact selected workspace
  -> complete current recognized-artifact catalogue
  -> deterministic ordered deletion plan
  -> preflight
  -> dry-run or application
  -> per-artifact revalidation and deletion
  -> per-effect verification and monotonic result formation
  -> one typed result
```

The operation satisfies these invariants:

- Bare `cleanup` resolves all currently eligible artifacts in the selected
  workspace. It never converts the request into artifact selection, a wizard,
  prompting, or a broad filesystem delete.
- An eligible artifact has positive Open Forge provenance, a bounded workspace
  association, physical containment, stable physical identity, inactive state,
  and the expected current bytes or physical state needed for its deletion.
- The catalogue includes recognized target-associated adjacent backups,
  recognized operation temporary or staging files and directories, and
  recognized residual recovery artifacts from incomplete or completed
  operations. Known `.bak` compatibility forms require the same positive proof.
- A suffix, age, extension, location, proximity, temporary-looking name, Git
  state, path, or matching bytes alone never establishes provenance or authority.
- Unknown, user-created, ambiguous, aliased, externally resolving, active,
  in-use, and concurrently changing items are not eligible. Active-operation
  artifacts remain preserved.
- Repository `.temp/`, raw evidence and snapshots, source and managed content,
  lifecycle documents and receipts, generated navigation, build outputs,
  package caches, logs that are not positively identified as one of the listed
  cleanup artifact kinds, and arbitrary backups remain outside the catalogue.
- The complete catalogue and deterministic ordered plan exist before the first
  effect. Dry-run and application use the same request, facts, plan, ordering,
  expected-state facts, and preflight.
- No deletion begins until the plan, Git policy, and all preflight facts are
  complete. A pre-effect unsafe or ambiguous selected item forms `blocked` and
  produces no write.
- Once deletion begins, verified deletion is monotonic. Cleanup does not create
  a backup, staging copy, receipt, journal, or tombstone merely to delete an
  eligible cleanup artifact and does not reverse a deletion it has verified.

The last invariant is a cleanup-only exception. Other CLI writes retain their
accepted backup, reverse-recovery, and residual-preservation rules.

## Request And Workspace Resolution

Request resolution:

1. Accepts only direct root `cleanup` with `--dry-run`, `--skip-git-check`, and
   shared global flags.
2. Rejects operands, artifact IDs and paths, selectors, profiles, age or glob
   expressions, aliases, unknown flags, and generic apply or delete forms.
3. Resolves shared `--help` and `--version` before workspace selection or domain
   work. Command-specific input with either terminal mode is invalid.
4. Collapses repeated `--dry-run` and `--skip-git-check` presence to one
   idempotent Boolean choice. Shared flag repetition retains its own contract.
5. Selects exactly the process current working directory or the exact
   `--workspace <path>` value. It does not search parent directories, choose a
   Git or package root, follow a nested `.agents` directory, or infer a
   workspace from an artifact path.

Human, TTY, JSON, and other non-interactive requests reach the same normalized
domain request. No request mode prompts or supplies a missing selector because
cleanup has no selector.

The selected workspace must be a safely accessible directory when cleanup needs
to inspect it. The operation reports the workspace and selection method in its
typed result. It does not treat workspace selection as proof of installation,
health, ownership, or artifact provenance.

## Current Facts And Catalogue Coverage

The resolver establishes a complete current catalogue through the accepted
artifact identity contract. The CLI Architecture defines the realization of
artifact names, storage, schema, and physical identity; the observable facts
below are fixed by this Behavior Contract.

For every candidate admitted to the catalogue, current facts include:

- artifact kind and positive Open Forge provenance;
- the associated operation or target when that kind requires one;
- bounded association with the selected workspace;
- lexical and physical containment without an external resolution or alias;
- stable physical identity and exact current bytes or physical state;
- inactive and not-in-use state, including whether an operation is still active;
- expected-state facts and the deletion verification condition; and
- affected-path Git classification when Git can classify the existing artifact.

The resolver considers:

1. target-associated adjacent backups, including a known `.bak` compatibility
   form only when positive identity is provable;
2. operation temporary and staging files or directories with positive operation
   provenance; and
3. residual recovery artifacts emitted by an incomplete or completed operation
   with positive provenance and bounded association.

The resolver does not use age, extension, suffix, location, proximity,
temporary-looking names, directory membership, or Git state as a substitute for
positive provenance. It does not recursively classify arbitrary backup trees.

Unknown or ambiguous material is preserved and excluded. If the required
catalogue cannot be completely enumerated or a required identity fact is safely
unavailable, result formation uses `incomplete`. If a candidate's identity,
containment, active state, or expected state is unsafe or ambiguous, the
candidate cannot enter a plan. A selected candidate that becomes unsafe or
ambiguous is `blocked`.

Cleanup does not require a proof that a recognized backup is unnecessary for
recovery, a proof that its bytes have been migrated, or an interpretation of
user content. Positive provenance, bounded association and containment,
inactive state, expected identity, and explicit command intent provide deletion
authority. This does not make an active or unsafe artifact eligible.

## Selection And Plan Formation

The planner selects every candidate that passes the complete eligibility facts.
It does not select a subset to avoid a difficult item, rank artifacts by age or
name, choose the newest or oldest backup, or treat a recommendation as input.

It forms one deterministic ordered plan containing, for each selected artifact:

- the exact logical and physical identity and artifact kind;
- provenance, operation or target association, and bounded containment;
- expected bytes or physical state and inactive-state evidence;
- the affected path and Git classification, when applicable;
- the deletion effect and exact ordering position; and
- the immediate revalidation and post-deletion verification conditions.

The plan contains no backup-creation effect, staging effect, lifecycle effect,
receipt publication, journal, tombstone, or hidden command invocation. A
recognized artifact directory is treated as one eligible deletion boundary only
when its own contained physical boundary is safe and no unknown or user-created
content would be removed by that boundary.

No selected item is silently omitted. An item that cannot satisfy its required
identity or safety facts prevents its selection or blocks the selected plan as
defined by the Interface status boundary. A catalogue coverage failure before
effects is `incomplete` and has no write.

## Preflight, Git, And Dry Run

Preflight validates the complete plan before the first deletion. It checks every
selected identity, containment, inactive-state fact, expected byte or physical
state, verification condition, and applicable Git condition.

When Git can classify an existing affected artifact path, a tracked path that
Git reports as dirty, or another unclean condition, blocks by default.
`--skip-git-check` changes only that cleanliness decision. It does not establish
provenance, inactive state, containment, expected identity, or deletion
authority. Gitless operation is valid: unavailable Git classification
does not block cleanup and does not require creation of a replacement backup.

Dry-run consumes the exact application request, current catalogue, plan,
ordering, expected-state facts, and preflight. It lists every exact selected
artifact and deletion effect, then stops before deletion or any persistent
effect. It does not create a backup, temporary file, staging copy, receipt,
journal, tombstone, or residual marker. Its planned deletions do not form
`attention`.

## Application, Revalidation, And Verification

When application is selected:

1. Revalidate the complete plan and all volatile workspace, identity,
   containment, inactive-state, expected-state, and Git facts.
2. Immediately before each deletion, revalidate that artifact's positive
   provenance, bounded association, physical containment, inactive state,
   expected bytes or physical identity, and deletion condition.
3. If any selected item is unsafe, ambiguous, active, in use, or changed before
   the first deletion, stop with `blocked` and no write. If the current item
   reaches that state after an earlier deletion was verified, stop before that
   item, preserve it, report the already verified effects, and return the
   applicable blocked result.
4. Delete only the current verified artifact boundary. Do not follow an alias
   or external resolution and do not delete a replacement that is not the
   current selected identity.
5. Verify the intended absence and the deletion identity immediately after each
   effect, then retain the exact deleted, remaining, preserved, and residual
   facts in the typed result.

Cleanup does not reverse a verified deletion. An unexpected deletion or
verification failure after effects begin stops further effects and forms
`failed`; already verified deletions remain desired effects. A caller
cancellation forms `interrupted` when no stronger unsafe residual condition
applies, with every verified deletion and every remaining or preserved artifact
reported. A later invocation never reuses this plan.

The operation does not create a recovery artifact to justify or reverse its own
eligible cleanup-artifact deletions. This is the only accepted monotonic recovery
exception and does not alter the recovery behavior of other operations.

## Idempotence And Fresh Repetition

After successful application, a repeat resolves a fresh current catalogue. If
no eligible artifact exists, it forms a complete verified no-op without
prompting and without a mutation path. It does not use a receipt, tombstone,
journal, saved plan, absence alone, or prior result to manufacture provenance.

A later user-created, unknown, ambiguous, aliased, externally resolving, or
otherwise ineligible item at a former artifact path is excluded by the fresh
catalogue and remains untouched. Only current positive artifact identity can
admit a later item.

## Result Formation And Status

One typed result records the selected workspace, normalized request, complete
catalogue facts, deterministic plan, preflight, Git decision, every planned
effect, every verified deletion, remaining and preserved items, residual facts,
verification or cancellation events, and semantic status. The result does not
claim that cleanup interpreted content or migration intent.

Result formation follows the Interface meanings:

- A complete dry-run plan, a complete verified application, and a complete
  verified empty catalogue are `complete`.
- `attention` is reserved by the shared vocabulary and is reachable only if a
  finite condition already required by shared current authority applies. Cleanup
  has no such accepted finite condition; planned deletions and preserved
  exclusions do not create it.
- Safe but incomplete required catalogue enumeration is `incomplete` before
  effects.
- Invalid request input is `invalid` before catalogue work.
- Unsafe or ambiguous selected identity, containment, active/in-use state,
  changed expected item, or applicable dirty Git state is `blocked`.
- An unexpected partial deletion or verification event is `failed`.
- Cancellation without a stronger unsafe residual condition is `interrupted`.

For ordinary conditions, precedence from strongest to weakest is `blocked`,
`incomplete`, `attention`, then `complete`. Invalid input stops first. `failed`
and `interrupted` retain their event meaning. The shared process-status mapping is
defined by the CLI Architecture.

## Presentation Relationship

Human expanded, human compact, and JSON renderers consume the same typed result.
They do not rerun catalogue enumeration, planning, preflight, deletion, or
verification. Presentation may change framing or density only; it cannot change
the selected default-all catalogue, hide an exact effect, change status, or
erase a remaining or residual item.

Primary human `complete`, `attention`, and `incomplete` results use stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. JSON emits one complete result on stdout for every semantic status, and
bounded diagnostics use stderr. The exact structured schema is defined by the CLI
Architecture.

## Behavioral Conformance

A conforming implementation must demonstrate:

- exact request normalization, no operands, no wizard or prompts, shared flag
  behavior, terminal modes, and exact workspace selection without discovery;
- positive provenance and bounded association for every accepted artifact kind,
  including known `.bak` compatibility forms and incomplete or completed
  residuals;
- physical containment and identity, expected bytes or state, inactive and
  in-use classification, concurrent-change handling, and preservation of all
  unknown, user-created, ambiguous, aliased, external, active, and excluded
  content;
- default-all catalogue formation, deterministic ordering, complete plan
  formation, no arbitrary recursive backup cleanup, and no hidden command;
- exact dry-run/application parity with no persistent dry-run effects;
- affected-path Git behavior, the narrow skip check, Gitless operation without
  replacement-backup creation, and no Git-based provenance inference;
- per-deletion revalidation, verification, monotonic deletion, no reverse of
  verified effects, failure and interruption reporting, residual visibility, and
  fresh-catalogue rerun convergence;
- complete no-op repetition without deleting a later replacement;
- all seven statuses, reserved unreachable `attention`, ordinary precedence,
  stream assignment, one-result JSON, exact effect visibility, and bounded
  diagnostics; and
- no Doctor, Repair, Index, Framework or Extension lifecycle, package, build,
  arbitrary filesystem, or Gate 6 documentation/history/release cleanup.

Gate 5 executable proof should use real isolated workspaces and real filesystem
identity and containment boundaries. The CLI Architecture defines the accepted
parser, filesystem, concurrency, artifact naming and schema, package, and source
realization; this Behavior Contract does not select another implementation.

## Related Sources

- [Cleanup Interface Contract](interface.md)
- [Cleanup Command Contract Set](_cleanup.md)
- [Global CLI Flags Behavior](../shared/global-flags/behavior.md)
- [Doctor Behavior Contract](../doctor/behavior.md)
- [Repair Behavior Contract](../repair/behavior.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
