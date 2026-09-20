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
meaning. The development implementation provides this operation;
[Task 20: Cleanup](../../../../../archived/cli-development/tasks/operations/cleanup.md)
records its implementation and execution evidence. The CLI remains non-shipping.

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
`--workspace <path>`, `--format json`, `--detail <minimal|standard|full|debug>`, `--detail debug`,
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

| Artifact kind    | Candidate and eligibility facts                                                                                                                                                                                                                                                                                                                                |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Exact-name final | Semantic source-generated current schema-v1 validation, including the required immutable typed attribution, determines `Verified`, `Malformed`, `Unsupported`, or `Unavailable`. Only a `Verified` ordinary file is deletion-eligible. A readable ordinary `Malformed` final is reported as a warning, preserved, and may coexist with independently eligible removals; it is never deleted. Every other integrity or file-kind condition, including unsupported, unavailable, non-ordinary, unsafe, or unreadable state, is reported, preserved, and blocks deletion. |
| Exact-name draft | An ordinary direct-child file is exact-name, path-only `Incomplete` support data, never a recovery preparation, and is deletion-eligible; observers do not inspect or use its bytes for attribution. A non-ordinary or otherwise unsafe exact-name draft is reported, preserved, and blocks deletion.                                                          |

A final is `Verified` only when the schema discriminator is exactly `1`, the
manifest has valid immutable typed attribution, its typed entries use the
admissible state pairs, and its ordered entries and payload bytes pass semantic
validation. This includes a typed `ordinary-create` entry whose prior state is
`Missing` and has no prior payload, and typed `relative-file-link-create` and
`relative-file-link-delete` entries whose identity is the exact
`relative-file-symbolic-link` kind and raw `/`-separated relative target;
absolute targets are invalid.
For a Library final, the attribution tuple is exactly `producer: library` with
one of `operation: attach`, `operation: sync`, or `operation: detach`, and
`subject.kind: workspace` with
`subject.identity` equal to the selected workspace key. Cleanup validates these
facts as recovery evidence only. It never resolves or follows a relative link,
reads source bytes as payload, applies or restores an entry, invokes Repair, or
deletes a Library projection, Library record, or source path. A schema-1 final
without valid attribution is malformed/unattributed and remains preserved; it is
not migrated, rewritten, repaired, adopted, or inferred. An unknown schema
version is `Unsupported`. Cleanup recognizes only current schema-v1 and has no
v2, dual reader, compatibility, or migration path. Attribution is an integrity
fact, not deletion authority: Cleanup still requires the exact selected bucket
and name, explicit cleanup intent, the same-workspace lease, under-lease
re-enumeration, and final semantic revalidation.

The exact schema-v1 attribution vocabulary, valid producer/operation/subject
combinations, and required non-null workspace identity are defined by the
[Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
Cleanup accepts no unknown value or fallback attribution.

Semantic final validation may stream each ZIP payload entry through fixed bounded
buffers solely to validate the exact declared length and lowercase SHA-256.
Prior-missing ordinary `Create` entries and relative-file-link `Create` and
`Delete` entries have no source payload. Link validation uses only the recorded
kind and raw `/`-separated relative target.
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
files, managed Framework or Extension content, workspace settings, ownership
locks and receipts, generated navigation, build outputs, package caches, logs, arbitrary
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
lease supply deletion authority. Valid attribution only contributes to final
integrity and never supplies deletion authority. Lease contention prevents
acquisition and therefore all deletion. Any item whose current
facts are unknown or unsafe remains visible and untouched. A bundle associated
with an original workspace path after a workspace move is reportable but is not
auto-bound to the selected path and is not eligible through that selected path.

## Planning, Dry Run, And Application

Cleanup forms one complete filtered exact-name candidate catalogue and one
deterministic deletion plan before effects. The catalogue includes every exact
named final or draft path and its observed kind and integrity condition. The plan
includes only verified final ordinary files and ordinary exact-name drafts, with
the required same-workspace lease boundary, deletion verification condition, and
result effect. A readable ordinary `Malformed` final is retained as a warning
preservation entry and does not block independently eligible deletions; it is
never a deletion entry. Unsupported, unavailable, unreadable, non-ordinary,
unsafe, or changed exact-name candidates remain reported and preserved and
block all deletion.

`--dry-run` uses the same candidate catalogue, plan, deterministic ordering,
expected-state facts, and preflight as application. It lists every exact-name
candidate and identifies each eligible planned effect or blocking condition as
contingent on application acquiring the same-workspace lease and passing final
under-lease validation. It writes nothing,
acquires no lease, creates no recovery bundle or other cleanup artifact. A
readable ordinary `Malformed` final remains a warning preservation condition in
the dry-run result and does not prevent independently eligible effects from
being listed; planned deletions alone do not create `completed-with-warnings`.

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
normalized physical workspace path. Manifest workspace association and valid
current-v1 typed attribution are required as part of semantic verification for
an exact-name final; they are not required
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
but it never binds that bundle to the newly selected path. A readable ordinary
`Malformed` final remains reported and preserved as a warning and does not
block independently eligible removals. Unsupported, unavailable, unreadable,
non-ordinary, unsafe, or changed exact-name candidates remain reported and
preserved and block deletion. Unknown, mismatched, differently keyed, or
differently named items remain outside the candidate set and untouched.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status              | When                                            | Headline                                                                                                                        | Exit | Stream |
| ------------------- | ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed           | empty catalogue                                 | `No recovery data to remove.`                                                                                                   |    0 | stdout |
| completed           | removed                                         | `Removed <N> recovery bundles and <K> unfinished drafts.` (omit a zero part; singular forms)                                    |    0 | stdout |
| completed (dry run) | planned                                         | `Would remove <N> recovery bundles and <K> unfinished drafts.`                                                                  |    0 | stdout |
| completed-with-warnings | readable ordinary malformed final retained; any independently eligible items are handled separately | normal cleanup headline plus the retained-candidate warning | 2 | stdout |
| incomplete          | store could not be read completely              | `The recovery store could not be read completely. Nothing was removed.`                                                         |    3 | stdout |
| invalid-input       | operand or bad flag                             | family                                                                                                                          |    4 | stderr |
| blocked             | lock held                                       | `Cannot clean up: another Open Forge command holds the workspace lock. Nothing was removed.`                                    |    5 | stderr |
| blocked             | unsupported, unavailable, unreadable, non-ordinary, unsafe, or changed candidate | Identifies the preserved candidate and its actual blocking cause; states that nothing was removed. | 5 | stderr |
| blocked             | catalogue changed under the lock                | `Cannot clean up: the recovery store changed while cleanup was running. Nothing was removed.`                                   |    5 | stderr |
| failed              | a deletion or verification failed after effects | `Cleanup stopped after removing <n> of <m> items.`                                                                              |    1 | stderr |
| cancelled           | Ctrl+C                                          | `Cleanup was cancelled after removing <n> of <m> items.` / `... Nothing was removed.`                                           |  130 | stderr |

### Text by level

`minimal`, removed:

```text
Removed 2 recovery bundles and 1 unfinished draft.
  <store>/myrepo-2026-09-13T21-04-11.zip
  <store>/myrepo-2026-09-13T21-09-52.zip
  <store>/myrepo-2026-09-13T21-11-30.draft
```

`minimal`, dry run: the same rows under `Would remove ...` and `No files were
changed.`

`minimal`, partial (stderr):

```text
Cleanup stopped after removing 1 of 3 items.
  <path-1>   removed
  <path-2>   could not be removed: <reason>
  <path-3>   not started
Next: open-forge cleanup
```

`standard` adds `Workspace:` and per row the kind and origin: `(bundle from
update, verified)`, `(unfinished draft from extension install)`, and rows for
items seen but not eligible (`<path>  left in place: not recognized`).

`full` adds the lock and final-check facts in words and each item's
integrity check.

A readable ordinary malformed final is shown as a warning and remains in
place; independently verified eligible items may still be removed. Unsupported,
unavailable, unreadable, non-ordinary, unsafe, and changed candidates retain
the blocking result and no-delete rule.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-completed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/__snapshots__/CleanupBeforeOutputSnapshotTests/RecoveryCatalogue/nothing-to-remove.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/__snapshots__/CleanupBeforeOutputSnapshotTests/StoreUnreadable/store-unreadable.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/__snapshots__/CleanupBeforeOutputSnapshotTests/InvalidInput/invalid-input.standard.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/__snapshots__/CleanupBeforeOutputSnapshotTests/RecoveryCatalogue/lock-held.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#cleanup-cancelled). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/__snapshots__/CleanupBeforeOutputSnapshotTests/CancelledBetweenRealDeletionStages/cancelled-partial.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                               |
| -------- | ------------------------------------------------------------------------------------ |
| minimal  | `{ mode, items: [ { path, kind: "bundle" \| "draft", outcome } ] }`                  |
| standard | + per item `origin` (command), `integrity`, plus `notEligible: [ { path, reason } ]` |
| full     | + `lock`, `finalCheck` in words                                                      |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

`<path>  removed` / `would be removed` / `could not be removed: <reason>` /
`not started` / `left in place: <reason>`.

### Counts and limitations

`bundlesRemoved`, `draftsRemoved`, `itemsLeftInPlace`.

### Next rules

Partial, lock or changed -> `open-forge cleanup`; damaged -> a sentence;
completed -> none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                   | Severity | Family                     | Message                                                                      | Next                           |
| -------------------------------------- | -------- | -------------------------- | ---------------------------------------------------------------------------- | ------------------------------ |
| cleanup.invalid-input                  | error    | invalid-input              |                                                                              |                                |
| cleanup.workspace-unavailable          | error    | workspace-unavailable      |                                                                              |                                |
| cleanup.workspace-not-directory        | error    | workspace-not-directory    |                                                                              |                                |
| cleanup.workspace-unsafe               | error    | workspace-unsafe           |                                                                              |                                |
| cleanup.workspace-lock-unavailable     | error    | workspace-lock-unavailable | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.workspace-lock-unavailable`).                                                   | `open-forge cleanup`           |
| cleanup.catalogue-incomplete           | warning  | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.catalogue-incomplete`).                 | `open-forge doctor`            |
| cleanup.recovery-final-malformed       | warning  | local                      | [`cleanup.phrase.is-damaged-and-was-left-in-place`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.recovery-final-malformed`).                                   | remove it by hand after review |
| cleanup.recovery-final-unsupported     | error    | local                      | [`cleanup.phrase.was-written-by-an-unsupported-version-and-was-left-in-place`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.recovery-final-unsupported`).        | remove it by hand after review |
| cleanup.recovery-final-unavailable     | error    | local                      | [`cleanup.phrase.could-not-be-read-and-was-left-in-place`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.recovery-final-unavailable`).                            | none                           |
| cleanup.recovery-draft-unsafe          | error    | local                      | [`cleanup.phrase.is-not-an-ordinary-file-and-was-left-in-place`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.recovery-draft-unsafe`).                      | none                           |
| cleanup.catalogue-changed-during-apply | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.catalogue-changed-during-apply`). | `open-forge cleanup`           |
| cleanup.candidate-changed-during-apply | error    | local                      | [`cleanup.phrase.changed-while-cleanup-was-running-and-was-left-in-place`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.candidate-changed-during-apply`).            | `open-forge cleanup`           |
| cleanup.deletion-failed                | error    | local                      | [`cleanup.removal.failed-path`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.deletion-failed`).                                     | `open-forge cleanup`           |
| cleanup.verification-failed            | error    | local                      | [`cleanup.label.could-not-be-removed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupText.cs), [`cleanup.label.still-exists-after-removal`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupText.cs), [`cleanup.phrase.still-exists-after-removal`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs), [`cleanup.removal.failure-reason`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Cleanup/Shared/Wording/CleanupWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`cleanup.verification-failed`).                                         | `open-forge cleanup`           |
| cleanup.operation-failed               | error    | operation-failed           |                                                                              |                                |
| cleanup.interrupted                    | error    | cancelled |                                                                              |                                |

## Scenarios

### Catalogue situations

`nothing-to-remove`, `two-bundles-one-draft`, `dry-run`, `damaged-bundle`,
`lock-held`, `store-unreadable`, `deletion-failed-partial`, `cancelled-partial`,
`invalid-input`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

## Non-Goals And Technical Boundary

Cleanup does not:

- accept artifact operands, IDs, paths, selectors, a wizard, prompts,
  confirmation flags, age or glob filters, recursive arbitrary cleanup, saved
  plans, profiles, or generic filesystem deletion;
- interpret user content, migration intent, or whether a recovery bundle is still useful
  to a person;
- delete unknown, differently keyed, differently named, or non-ordinary items;
- remove raw evidence or snapshots, source or managed
  content, workspace settings, ownership locks or receipts, generated navigation,
  build output,
  package caches, logs that are not positively identified as one of the listed
  cleanup artifact kinds, or arbitrary support files;
- run Doctor, Repair, Index, Framework or Extension lifecycle, package cleanup,
  or Gate 6 documentation, history, or release cleanup as a hidden operation;
- apply or restore a recovery entry, or delete a Library projection, Library
  record, or source path;
- create a replacement recovery bundle, staging copy, receipt, journal, or tombstone for
  cleanup, reverse a verified deletion, or replay a saved plan; or
- create or write an activity marker, PID, journal, lock metadata, or another
  activity mechanism; or
- change the accepted Architecture.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the structured schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines cleanup's cross-cutting artifact
identity, filesystem, concurrency, and runtime structure. The development
implementation follows these decisions; Task 20 records its verification.

## Verification Requirements

Conformance evidence must cover:

- exact root syntax, no operands, no wizard or prompts, shared flags, terminal
  modes, and idempotent Boolean repetition;
- exact CWD and `--workspace` selection without parent or marker discovery;
- filtered exact-name candidate formation, semantic final-ZIP integrity,
  deletion eligibility only for verified finals and ordinary drafts, warning
  preservation of a readable ordinary malformed final with independent eligible
  removal, and blocking preservation of unsupported, unavailable, unreadable,
  non-ordinary, or unsafe exact-name candidates;
- representative payload validation with exact declared
  lengths and hashes, bounded buffers and memory independent of entry size, and
  no extraction, disclosure, retention, or materialization;
- current-v1 typed validation for an ordinary prior-missing `Create` with no
  prior payload and relative-file-link `Create` or `Delete` entries with exact
  link kind and raw `/`-separated relative target, including Library attribution
  `library`/`attach`, `library`/`sync`, and `library`/`detach` with a trusted
  `workspace` subject matching the selected workspace key; no link following,
  source-byte payload reads, entry application, restoration, Repair invocation,
  or Library projection, record, or source deletion;
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
- all seven statuses, the accepted reachable `completed-with-warnings` warning
  condition, human stream rules, complete JSON parity, exact effect visibility,
  and no mixed JSON stdout; and
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

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`cleanup.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Cleanup/CleanupText.cs).

<!-- @OpenForgeTextRef cleanup.help.syntax -->
<!-- @OpenForgeTextRef cleanup.label.could-not-be-removed -->
<!-- @OpenForgeTextRef cleanup.label.still-exists-after-removal -->
<!-- @OpenForgeTextRef cleanup.phrase.changed-while-cleanup-was-running-and-was-left-in-place -->
<!-- @OpenForgeTextRef cleanup.phrase.could-not-be-read-and-was-left-in-place -->
<!-- @OpenForgeTextRef cleanup.phrase.is-damaged-and-was-left-in-place -->
<!-- @OpenForgeTextRef cleanup.phrase.is-not-an-ordinary-file-and-was-left-in-place -->
<!-- @OpenForgeTextRef cleanup.phrase.still-exists-after-removal -->
<!-- @OpenForgeTextRef cleanup.phrase.was-written-by-an-unsupported-version-and-was-left-in-place -->
<!-- @OpenForgeTextRef cleanup.removal.failed-path -->
<!-- @OpenForgeTextRef cleanup.removal.failure-reason -->
