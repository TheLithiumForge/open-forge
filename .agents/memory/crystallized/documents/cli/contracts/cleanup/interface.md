---
open-forge:
  description: Accepted non-shipping public interface for operand-free, lease-validated cleanup of recognized recovery bundles and drafts
  responsibility: Define cleanup's exact syntax, default-all catalogue, deletion authority, observable effects, statuses, and boundaries
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Cleanup, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# Cleanup Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for the
non-shipping root `open-forge cleanup` operation. It owns the public purpose, exact syntax,
operand and flag boundary, recognized catalogue, consent, observable effects,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
catalogue formation, planning, preflight, deletion, verification, and the
narrow cleanup-specific recovery exception. The shared [Global CLI Flags
Interface](../shared/global-flags/interface.md) defines the global flags once.
The [Doctor Interface](../doctor/interface.md) and [Repair Interface](../repair/interface.md)
remain authoritative for their read-only diagnosis and local-repair boundaries;
cleanup does not invoke either operation.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the accepted shared structured schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines source and runtime boundaries,
BCL-first filesystem structure, the workspace-lock boundary, and recovery
identity relationships. This Interface Contract remains the authority for cleanup's public
meaning. Implementation and executable proof remain pending Gate 5; those
pending proofs do not weaken the observable boundaries below.

## Purpose And Operation Boundary

`cleanup` removes every verified recovery final or ordinary exact-name draft
associated with one exact selected workspace when the complete candidate set
remains eligible after final validation under the same-workspace lease. Bare
`cleanup` discovers the current filtered exact-name candidate catalogue and
applies one complete deletion plan. It does
not accept an artifact operand or selector, and it does not interpret user
content, migration intent, or recovery strategy.

The command is a direct root operation. Human, JSON, TTY, and other
non-interactive invocations use the same catalogue and deletion domain. A TTY
does not add a wizard or confirmation step. `--dry-run` is the only preview or
review mechanism.

## Syntax

The complete public command form is:

```text
open-forge cleanup [--dry-run] [global flags]
```

The command has no child operations, aliases, or positional operands. The
shared [Global CLI Flags Interface](../shared/global-flags/interface.md) defines
`--workspace <path>`, `--json`, `--view=compact|expanded`, `--verbose`,
`--help`, and `--version`. Their grammar, defaults, repetition, composition,
and terminal behavior apply without local redefinition.

`--help` and `--version` stop before workspace selection and cleanup work.
Command-specific input is invalid with either terminal mode.

## Operands And Selection

Cleanup accepts no operands. Artifact IDs, paths, filenames, directory values,
selectors, globs, age expressions, profiles, and saved-plan references are not
alternate selection forms; they are invalid input.

The bare command catalogues all external exact-name recovery candidates under the
boundary below. Only verified finals and ordinary exact-name drafts are selected
for deletion. Application deletes a selected candidate only after acquiring the
same-workspace lease and repeating final catalogue and expected-state validation.
It does not select all files,
support-artifact lookalikes, or a user-selected subset. There is no wizard,
prompt, confirmation flow, explicit-selection mode, or `--automatic` mode. JSON
and every non-interactive invocation use this same exact-name default-all
behavior.

## Flags

| Flag                | Role                       | Value                          | Omission                | Repetition and composition                                                                                      |
| ------------------- | -------------------------- | ------------------------------ | ----------------------- | --------------------------------------------------------------------------------------------------------------- |
| `--dry-run`         | Write policy               | Boolean                        | Application is selected | Repeats idempotently. It changes only whether effects are applied and never changes the catalogue or authority. |
| Shared global flags | Workspace and presentation | Defined by the shared contract | Shared defaults apply   | Shared repetition, ordering, composition, and terminal rules apply.                                             |

Cleanup has no `--force`, `--yes`, `--apply`, `--automatic`, age filter, glob,
recursive selector, artifact selector, saved plan, cleanup profile, or generic
delete flag. `--dry-run` is not an implicit plan command.

## Recognized Catalogue

The catalogue is the filtered set of exact deterministic final and draft names
directly under the selected workspace bucket. It is read from
the current user's external root
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1`; this
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. It is not a recursive workspace scan. An absent application-data root
or recovery store produces an empty catalogue and a verified complete no-op. An
existing selected workspace bucket that cannot be read produces the locally
contracted `incomplete` result rather than an empty catalogue. Each exact-name
candidate retains its path, current file kind, and integrity condition:

| Artifact kind    | Candidate and eligibility facts                                                                                                                                                                                                                        |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Exact-name final | Semantic source-generated schema-v1 validation determines `Verified`, `Malformed`, `Unsupported`, or `Unavailable`. Only a `Verified` ordinary file is deletion-eligible; every other integrity condition is reported, preserved, and blocks deletion. |
| Exact-name draft | An ordinary direct-child file is `Incomplete` support data, never a recovery preparation, and is deletion-eligible. A non-ordinary or otherwise unsafe exact-name draft is reported, preserved, and blocks deletion.                                   |

Semantic final validation may stream each ZIP payload entry through fixed bounded
buffers solely to validate the exact declared length and lowercase SHA-256.
Cleanup never extracts, discloses, renders, logs, returns, retains, or
materializes payload bytes, and validation memory remains independent of payload
size.

The selected workspace bucket and its exact final and draft names are
deterministic from the normalized physical workspace path key and operation ID.
Unknown or differently named direct children remain outside the filtered
candidate catalogue and its equality checks; they and all content outside that
bucket remain untouched.

Cooperating-process exclusion uses the existing same-workspace
`WorkspaceLockLease` through the persistent reusable lock file and
`FileShare.None`. If a cooperating mutator owns the lease, Cleanup performs no
deletion. Cleanup writes no marker, PID, journal, or lock metadata.

The catalogue excludes workspace files, raw evidence and snapshots, source
files, managed Framework or Extension content, lifecycle documents and
receipts, generated navigation, build outputs, package caches, logs, arbitrary
support files, lookalike items, and arbitrary filesystem content. Cleanup does
not recursively delete a support-artifact tree or infer provenance from a
parent directory.

## Deletion Consent And Preservation

Omitting `--dry-run` is explicit consent to discard every verified final and
ordinary exact-name draft for the selected workspace that remains eligible after
final under-lease validation. The command does not inspect user content to decide
whether it should be preserved, and it does not infer migration or recovery
intent.

The exact selected-workspace bucket, deterministic direct-child name, semantic
final validation when applicable, explicit command, and held same-workspace
lease supply deletion authority. Lease contention prevents acquisition and
therefore all deletion. Any item whose current
facts are unknown or unsafe remains visible and untouched. A bundle associated
with an original workspace path after a workspace move is reportable but is not
auto-bound to the selected path and is not eligible through that selected path.

## Planning, Dry Run, And Application

Cleanup forms one complete filtered exact-name candidate catalogue and one
deterministic deletion plan before effects. The catalogue includes every exact
named final or draft path and its observed kind and integrity condition. The plan
includes only verified final ordinary files and ordinary exact-name drafts, with
the required same-workspace lease boundary, deletion verification condition, and
result effect. A malformed, unsupported, unavailable, non-ordinary, or unsafe
exact-name candidate remains reported and preserved and blocks all deletion.

`--dry-run` uses the same candidate catalogue, plan, deterministic ordering,
expected-state facts, and preflight as application. It lists every exact-name
candidate and identifies each eligible planned effect or blocking condition as
contingent on application acquiring the same-workspace lease and passing final
under-lease validation. It writes nothing,
acquires no lease, creates no recovery bundle or other cleanup artifact, and does
not create `attention` merely because deletions are planned.

An empty candidate catalogue is a verified complete no-op and acquires no lease.
Before any deletion, Cleanup acquires one live same-workspace
`WorkspaceLockLease` and holds it through deletion and verification. It then
re-enumerates the selected bucket once under that lease, filters the same exact
final and draft names, and compares that candidate set and its relevant current
path, kind, and integrity facts with the planned catalogue. A new, removed, or
changed exact-name candidate blocks deletion; an unknown or differently named
item remains outside the comparison. Each final ZIP repeats the semantic
validation above immediately before effect; each draft repeats its exact
path/name/kind check. Cleanup calls ordinary `File.Delete` for each validated
path and verifies absence. If lease acquisition fails, a cooperating mutator owns
the lease, or the filtered under-lease candidate set differs from the planned
catalogue, Cleanup performs no deletion. A later ordinary deletion or
verification failure stops the operation and reports already verified deletions
and remaining paths.

Once deletion begins, cleanup has a narrow monotonic support-artifact
exception. It does not create a recovery bundle, staging copy, receipt, journal,
or tombstone just to delete eligible bundles or drafts, and it does not reverse
a deletion that it has already verified. This exception applies only to
cleanup; it does not weaken recovery for any other CLI write.

On failure or interruption, already verified deletions remain valid desired
effects. Unprocessed, remaining, and currently unsafe artifacts stay visible,
and residual facts are reported. A later invocation forms a fresh catalogue and
does not replay a saved plan.

## External Recovery Boundary

Cleanup reads only the current user's
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1` root and the
deterministic selected-workspace bucket and exact names derived from the
normalized physical workspace path. Manifest workspace association is required
only as part of semantic verification for an exact-name final; it is not required
to catalogue a draft or a malformed, unsupported, or unavailable final. This
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. It does not discover a repository root, inspect version-control state,
or use any workspace-local fallback. If the external root or store is absent,
the catalogue is empty and the operation is a verified complete no-op. If an
existing root or store is unavailable or unsafe to inspect, the operation is
`incomplete` and makes no deletion; it does not treat access failure as absence.

Only verified final ordinary files and ordinary exact-name drafts that pass final
under-lease validation are eligible. A bundle is not a restore source: Cleanup
never extracts, binds, restores, rolls back, or compensates for target effects.
It may report a bundle for an original workspace path after a workspace move,
but it never binds that bundle to the newly selected path. Malformed,
unsupported, unavailable, non-ordinary, or changed exact-name candidates remain
reported and preserved and block deletion. Unknown, mismatched, differently
keyed, or differently named items remain outside the candidate set and untouched.

## Human Output

Human output comes from one typed cleanup result. The default expanded view
includes the selected workspace and selection method, application or dry-run
mode, complete filtered candidate-catalogue coverage, lease and final-validation
facts when application reaches deletion, every exact-name candidate, its kind
and integrity condition, every planned or verified deletion, preserved or
remaining items, residual facts, and semantic status.

Compact output retains the workspace identity, mode, deterministic artifact
order, every exact planned or verified effect, completeness and safety facts,
semantic status, and at most one required `Next:` action. It does not replace
the artifact list with a count.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Each primary result remains together on its assigned stream. Human
output may say `requires attention` for the `attention` status.

Representative complete application:

```text
Open Forge cleanup
Workspace: <workspace-path>
Selected by: current directory
Removed and verified:
  <recognized-artifact-path>
  <recognized-artifact-path>
All planned artifacts were removed and verified.
Result: complete
```

Representative dry run:

```text
Open Forge cleanup
Workspace: <workspace-path>
Mode: preview
Would remove after same-workspace lease acquisition and final validation:
  <recognized-artifact-path>
  <recognized-artifact-path>
No files changed (--dry-run).
Result: complete
```

An empty catalogue is a verified complete no-op, acquires no lease, and does not
prompt. A partial result names every artifact already deleted and verified and
every artifact remaining or preserved.

## Structured Output

`--json` emits one complete structured result to stdout from the same typed
result used by human output. It never prompts and never reruns catalogue
formation, planning, preflight, deletion, or verification. Human text is not
mixed into JSON stdout; bounded diagnostics use stderr under the shared output
contract.

The result retains, using the shared structured schema defined by the [Shared
Result Coordinates](../shared/result-coordinates/interface.md):

- workspace and selection method;
- dry-run or application mode and normalized flags;
- filtered-catalogue coverage and every exact named final or draft with path,
  kind, integrity condition, eligibility, and required lease boundary;
- lease acquisition and one final under-lease filtered-catalogue comparison,
  including relevant path, kind, and integrity facts, when application reaches
  deletion;
- every planned, deleted-and-verified, remaining, and preserved bundle or draft,
  with current-workspace association and recovery-bundle provenance;
- verification, cancellation, or failure facts; and
- semantic status and at most one required `Next:` action.

`--view` changes only human presentation. JSON always retains every exact
artifact and effect.

## Semantic Results

Cleanup uses the shared seven-status vocabulary:

| Result        | Meaning                                                                                                                                                                                                                                                                                                   |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The complete safe candidate catalogue and contingent deletion plan were established in dry-run, application acquired the lease and deleted and verified every final planned artifact, or a complete current catalogue verified that no candidate exists without acquiring a lease.                        |
| `attention`   | Reserved by the shared status contract and reachable only if a finite condition already required by that shared current authority applies. Cleanup has no such accepted condition, so planned deletions and preserved unknown items do not produce it.                                                    |
| `incomplete`  | Safe catalogue enumeration or another required coverage fact could not be completed. No deletion begins while the operation has only this pre-effect condition.                                                                                                                                           |
| `invalid`     | Operands, an unknown or malformed flag, an invalid value, an invalid repetition, or command-specific input in a terminal help/version mode prevents request resolution.                                                                                                                                   |
| `blocked`     | The required same-workspace lease cannot be acquired, an exact-name candidate is malformed, unsupported, unavailable, non-ordinary, or unsafe, the filtered under-lease candidate set differs from the planned catalogue, or final revalidation prevents safe deletion. No unstarted deletion is applied. |
| `failed`      | An unexpected deletion, verification, or other partial application failure occurs after effects begin. Already verified deletions and remaining or residual items are reported.                                                                                                                           |
| `interrupted` | The caller cancels and no stronger unsafe residual condition applies. Any already verified monotonic deletions and all remaining items are reported.                                                                                                                                                      |

For ordinary pre-effect conditions, status precedence is `blocked` >
`incomplete` > `attention` > `complete`. Invalid input stops before catalogue
formation. `failed` and `interrupted` preserve their event meaning. Exact
process-status mapping is defined by the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

## Errors And Boundaries

Cleanup rejects or blocks:

- any positional operand, artifact ID, path, selector, profile, or generic
  deletion input;
- an unknown, malformed, repeated, or terminal-conflicting flag;
- a selected workspace that is unavailable or not a directory;
- incomplete required catalogue enumeration;
- an exact-name final that is malformed, unsupported, unavailable, non-ordinary,
  or unsafe, or a planned draft path that is not the exact named ordinary file
  under the selected workspace bucket;
- failure to acquire the same-workspace lease, including when a cooperating
  mutator owns it;
- an under-lease filtered exact-name candidate set or relevant current fact that
  differs from the planned catalogue; or
- an unexpected partial deletion or verification failure.

Unknown, user-created, arbitrary, and otherwise differently named items remain
outside candidate comparison and are preserved rather than reclassified or
deleted. Every ordinary error names `cleanup`, the affected workspace or bundle
or draft when known, the direct cause, and at most one useful next action.

## Scenarios

Discover and remove all currently eligible artifacts in the exact current
workspace:

```text
open-forge cleanup
```

Preview the same filtered exact-name default-all candidate catalogue as structured
output. Each proposed deletion remains contingent on application acquiring the
same-workspace lease and completing final validation:

```text
open-forge cleanup --dry-run --json
```

Repeat after successful cleanup:

```text
open-forge cleanup
```

The repeated invocation forms a fresh catalogue. With no exact named final or
draft candidate, it returns a verified complete no-op.

## Non-Goals And Technical Boundary

Cleanup does not:

- accept artifact operands, IDs, paths, selectors, a wizard, prompts,
  confirmation flags, age or glob filters, recursive arbitrary cleanup, saved
  plans, profiles, or generic filesystem deletion;
- interpret user content, migration intent, or whether a recovery bundle is still useful
  to a person;
- delete unknown, differently keyed, differently named, or non-ordinary items;
- remove raw evidence or snapshots, source or managed
  content, lifecycle documents or receipts, generated navigation, build output,
  package caches, logs that are not positively identified as one of the listed
  cleanup artifact kinds, or arbitrary support files;
- run Doctor, Repair, Index, Framework or Extension lifecycle, package cleanup,
  or Gate 6 documentation, history, or release cleanup as a hidden operation;
- create a replacement recovery bundle, staging copy, receipt, journal, or tombstone for
  cleanup, reverse a verified deletion, or replay a saved plan; or
- create or write an activity marker, PID, journal, lock metadata, or another
  activity mechanism; or
- change the accepted Architecture. Implementation and executable proof for
  the accepted boundaries remain pending Gate 5.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the structured schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines cleanup's cross-cutting artifact
identity, filesystem, concurrency, and runtime structure. Gate 5 provides
implementation and executable proof for those decisions.

## Verification Requirements

Future conformance evidence must cover:

- exact root syntax, no operands, no wizard or prompts, shared flags, terminal
  modes, and idempotent Boolean repetition;
- exact CWD and `--workspace` selection without parent or marker discovery;
- filtered exact-name candidate formation, semantic final-ZIP integrity,
  deletion eligibility only for verified finals and ordinary drafts, and
  blocking preservation of malformed, unsupported, unavailable, non-ordinary,
  or unsafe exact-name candidates;
- representative payload validation with exact declared
  lengths and hashes, bounded buffers and memory independent of entry size, and
  no extraction, disclosure, retention, or materialization;
- selected-workspace bucket association, exact direct-child file kind, and one
  under-lease re-enumeration comparing the filtered candidate set and relevant
  current facts with the planned catalogue;
- preservation and comparison exclusion of unknown, user-created, differently
  named, differently keyed, mismatched, and arbitrary support content, plus no
  deletion under cooperating-mutation lease contention;
- default-all filtered exact-name candidate-catalogue formation, deterministic
  ordering, complete contingent plan formation, preflight, exact dry-run
  candidate-plan parity, and no dry-run writes or lease acquisition;
- external-root unavailability, deterministic bundle identity, and the absence
  of workspace-local recovery or version-control inspection;
- empty no-lease no-op, one held same-workspace `WorkspaceLockLease` before every
  deletion, one filtered-candidate catalogue and relevant-fact comparison under
  that lease, unknown-name exclusion from equality, no deletion when a
  cooperating mutator owns the lease, and no marker, PID, journal, or
  lock-metadata write;
- immediate final semantic validation, ordinary `File.Delete`, absence
  verification, monotonic partial application, residual reporting,
  interruption, and fresh-catalogue rerun;
- all seven statuses, reserved unreachable `attention`, human stream rules,
  complete JSON parity, exact effect visibility, and no mixed JSON stdout; and
- no Doctor, Repair, Index, lifecycle, package, build, arbitrary filesystem, or
  Gate 6 cleanup side effects.

## Related Sources

- [Cleanup Behavior Contract](behavior.md)
- [Global CLI Flags Interface](../shared/global-flags/interface.md)
- [Global CLI Flags Behavior](../shared/global-flags/behavior.md)
- [Doctor Interface Contract](../doctor/interface.md)
- [Repair Interface Contract](../repair/interface.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
