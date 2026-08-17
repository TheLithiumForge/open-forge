---
open-forge:
  description: Accepted non-shipping public interface for operand-free cleanup of recognized Open Forge transient and recovery artifacts
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

The [CLI Architecture](../../architecture.md) defines the accepted shared
structured schema, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, workspace lock, and recovery identity
model. This Interface Contract remains the authority for cleanup's public
meaning. Implementation and executable proof remain pending Gate 5; those
pending proofs do not weaken the observable boundaries below.

## Purpose And Operation Boundary

`cleanup` removes every currently eligible Open Forge-created transient or
recovery artifact in one selected workspace. Bare `cleanup` discovers the
current recognized catalogue and applies one complete deletion plan. It does
not accept an artifact operand or selector, and it does not interpret user
content, migration intent, or recovery strategy.

The command is a direct root operation. Human, JSON, TTY, and other
non-interactive invocations use the same catalogue and deletion domain. A TTY
does not add a wizard or confirmation step. `--dry-run` is the only preview or
review mechanism.

## Syntax

The complete public command form is:

```text
open-forge cleanup [--dry-run] [--skip-git-check] [global flags]
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

The bare command selects all artifacts that the current invocation can positively
recognize as eligible under the catalogue below. It does not select all files,
all backups, all temporary-looking paths, or a user-selected subset. There is
no wizard, prompt, confirmation flow, explicit-selection mode, or
`--automatic` mode. JSON and every non-interactive invocation use this same
recognized-only default-all behavior.

## Flags

| Flag                | Role                       | Value                          | Omission                                                              | Repetition and composition                                                                                      |
| ------------------- | -------------------------- | ------------------------------ | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `--dry-run`         | Write policy               | Boolean                        | Application is selected                                               | Repeats idempotently. It changes only whether effects are applied and never changes the catalogue or authority. |
| `--skip-git-check`  | Safety-policy exception    | Boolean                        | Keep the affected-path cleanliness check when Git can classify a path | Repeats idempotently. It bypasses only that Git check and never proves provenance or grants deletion authority. |
| Shared global flags | Workspace and presentation | Defined by the shared contract | Shared defaults apply                                                 | Shared repetition, ordering, composition, and terminal rules apply.                                             |

Cleanup has no `--force`, `--yes`, `--apply`, `--automatic`, age filter, glob,
recursive selector, artifact selector, saved plan, cleanup profile, or generic
delete flag. `--dry-run` is not an implicit plan command, and
`--skip-git-check` is not a recovery or provenance bypass.

## Recognized Catalogue

The catalogue is closed by positive artifact identity. It includes every
currently eligible transient or recovery artifact emitted by Open Forge under
the accepted artifact identity contract:

| Artifact kind                                    | Eligible current facts                                                                                                                                                                                                        |
| ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Target-associated adjacent backup                | Positive Open Forge provenance identifies the backup, its associated target, its bounded workspace relationship, and its physical identity. Known `.bak` compatibility forms are included only when those facts are provable. |
| Operation temporary or staging file or directory | Positive provenance identifies an Open Forge operation and its bounded workspace association; the artifact is physically contained and inactive.                                                                              |
| Residual recovery artifact                       | Positive provenance identifies an Open Forge artifact left by an incomplete or completed operation, its bounded workspace association, physical identity, and inactive state.                                                 |

Exact names, storage locations, and schemas are not part of this Interface
Contract. A suffix, age, extension, location, proximity, or temporary-looking
name alone never proves that an item belongs to Open Forge.

An artifact is outside the eligible catalogue when it is unknown, user-created,
ambiguous, aliased, externally resolving, active, in use, or concurrently
changing. Such an item remains untouched. A selected item's identity,
containment, inactive state, or expected bytes that become unsafe or ambiguous
block the operation rather than widening the catalogue.

The catalogue excludes repository `.temp/`, raw evidence and snapshots, source
files, managed Framework or Extension content, lifecycle documents and
receipts, generated navigation, build outputs, package caches, logs that are
not positively identified as one of the listed cleanup artifact kinds,
arbitrary backups, and arbitrary filesystem content. Cleanup does not
recursively delete a backup tree or infer provenance from a parent directory.

## Deletion Consent And Preservation

Omitting `--dry-run` is explicit consent to discard every artifact that is
currently eligible in the selected workspace. That consent includes a backup
the user no longer wants after manually preserving or migrating any desired
content. The command does not inspect user content to decide whether that
content should be preserved, and it does not infer migration or recovery
intent.

Positive Open Forge provenance, bounded workspace association, physical
containment, inactive state, expected identity, and the explicit command supply
deletion authority. Cleanup does not need to prove that a backup remains
unnecessary for recovery. Active-operation artifacts and any item whose current
facts are unknown or unsafe remain visible and untouched.

## Planning, Dry Run, And Application

Cleanup forms one complete current catalogue and one deterministic deletion plan
before effects. The plan includes every exact eligible artifact and its expected
identity, bytes or physical state, containment, inactive-state fact, Git fact,
verification condition, and result effect.

`--dry-run` uses the same catalogue, plan, deterministic ordering, expected-state
facts, and preflight as application. It lists every exact eligible artifact and
effect, writes nothing, creates no backup or other cleanup artifact, and does
not create `attention` merely because deletions are planned.

Before each deletion, cleanup revalidates the selected artifact's provenance,
bounded containment, inactive state, expected bytes or physical identity, and
other expected-state facts. If a selected item becomes unsafe or ambiguous
before effects begin, the complete operation is `blocked` and writes nothing.
If a selected item changes after deletion has begun, cleanup stops before that
item and reports the already verified deletions and the remaining item.

Once deletion begins, cleanup has a narrow monotonic recovery exception. It does
not create an adjacent backup, staging copy, receipt, journal, or tombstone just
to delete eligible cleanup artifacts, and it does not reverse a deletion that it
has already verified. This exception applies only to cleanup; it does not weaken
recovery for any other CLI write.

On failure or interruption, already verified deletions remain valid desired
effects. Unprocessed, remaining, and currently unsafe artifacts stay visible,
and residual facts are reported. A later invocation forms a fresh catalogue and
does not replay a saved plan.

## Git Policy And Gitless Workspaces

When Git can classify an eligible existing artifact path, affected-path
cleanliness is checked by default. A tracked artifact path that Git reports as
dirty, or another unclean affected path, blocks unless `--skip-git-check` is
explicit. That flag bypasses only the affected-path Git check.

A Gitless workspace is valid. Missing Git evidence does not block cleanup and
does not require a replacement backup because cleanup's eligible-artifact
exception applies. Git never establishes Open Forge provenance, inactive state,
or deletion authority.

## Human Output

Human output comes from one typed cleanup result. The default expanded view
includes the selected workspace and selection method, application or dry-run
mode, complete catalogue coverage, every exact eligible artifact, its artifact
kind and bounded identity evidence, every planned or verified deletion, Git
facts, preserved or remaining items, residual facts, and semantic status.

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
Would remove:
  <recognized-artifact-path>
  <recognized-artifact-path>
No files changed (--dry-run).
Result: complete
```

An empty catalogue is a verified complete no-op and does not prompt. A partial
result names every artifact already deleted and verified and every artifact
remaining or preserved.

## Structured Output

`--json` emits one complete structured result to stdout from the same typed
result used by human output. It never prompts and never reruns catalogue
formation, planning, preflight, deletion, or verification. Human text is not
mixed into JSON stdout; bounded diagnostics use stderr under the shared output
contract.

The result retains, using the shared structured schema defined by the [CLI
Architecture](../../architecture.md):

- workspace and selection method;
- dry-run or application mode and normalized flags;
- catalogue coverage and every exact recognized artifact with kind, provenance,
  bounded association, containment, inactive state, and expected identity facts;
- every planned, deleted-and-verified, remaining, preserved, and residual item;
- Git classification and the narrow skip decision;
- verification, cancellation, or failure facts; and
- semantic status and at most one required `Next:` action.

`--view` changes only human presentation. JSON always retains every exact
artifact and effect.

## Semantic Results

Cleanup uses the shared seven-status vocabulary:

| Result        | Meaning                                                                                                                                                                                                                                                |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `complete`    | The complete safe catalogue and deletion plan were established in dry-run, application deleted and verified every planned artifact, or a complete current catalogue verified that no eligible artifact exists.                                         |
| `attention`   | Reserved by the shared status contract and reachable only if a finite condition already required by that shared current authority applies. Cleanup has no such accepted condition, so planned deletions and preserved unknown items do not produce it. |
| `incomplete`  | Safe catalogue enumeration or another required coverage fact could not be completed. No deletion begins while the operation has only this pre-effect condition.                                                                                        |
| `invalid`     | Operands, an unknown or malformed flag, an invalid value, an invalid repetition, or command-specific input in a terminal help/version mode prevents request resolution.                                                                                |
| `blocked`     | An unsafe or ambiguous selected identity, physical containment, active or in-use state, changed expected item, or default Git cleanliness check prevents a safe deletion. No unstarted effect is applied.                                              |
| `failed`      | An unexpected deletion, verification, or other partial application failure occurs after effects begin. Already verified deletions and remaining or residual items are reported.                                                                        |
| `interrupted` | The caller cancels and no stronger unsafe residual condition applies. Any already verified monotonic deletions and all remaining items are reported.                                                                                                   |

For ordinary pre-effect conditions, status precedence is `blocked` >
`incomplete` > `attention` > `complete`. Invalid input stops before catalogue
formation. `failed` and `interrupted` preserve their event meaning. Exact
The shared process-status mapping is defined by the CLI Architecture.

## Errors And Boundaries

Cleanup rejects or blocks:

- any positional operand, artifact ID, path, selector, profile, or generic
  deletion input;
- an unknown, malformed, repeated, or terminal-conflicting flag;
- a selected workspace that is unavailable or not a directory;
- incomplete required catalogue enumeration;
- a recognized candidate whose provenance, bounded association, physical
  containment, inactive state, expected bytes, or physical identity is unsafe
  or ambiguous;
- an artifact that becomes active, in use, or concurrently changed before its
  deletion;
- a tracked artifact path that Git reports as dirty, or another dirty affected
  artifact path, without `--skip-git-check`; or
- an unexpected partial deletion or verification failure.

Unknown, user-created, arbitrary, aliased, externally resolving, and otherwise
ineligible items are preserved rather than reclassified or deleted. Every
ordinary error names `cleanup`, the affected workspace or artifact when known,
the direct cause, and at most one useful next action.

## Scenarios

Discover and remove all currently eligible artifacts in the exact current
workspace:

```text
open-forge cleanup
```

Preview the same recognized-only default-all catalogue as structured output:

```text
open-forge cleanup --dry-run --json
```

Run in a Gitless workspace or bypass only Git cleanliness for eligible paths:

```text
open-forge cleanup --skip-git-check
```

Repeat after successful cleanup:

```text
open-forge cleanup
```

The repeated invocation forms a fresh catalogue. With no newly eligible Open
Forge artifact, it returns a verified complete no-op and does not delete a later
user-created or unknown replacement at a former artifact path.

## Non-Goals And Technical Boundary

Cleanup does not:

- accept artifact operands, IDs, paths, selectors, a wizard, prompts,
  confirmation flags, age or glob filters, recursive arbitrary cleanup, saved
  plans, profiles, or generic filesystem deletion;
- interpret user content, migration intent, or whether a backup is still useful
  to a person;
- delete unknown, ambiguous, active, in-use, aliased, externally resolving,
  user-created, or concurrently changing items;
- remove repository `.temp/`, raw evidence or snapshots, source or managed
  content, lifecycle documents or receipts, generated navigation, build output,
  package caches, logs that are not positively identified as one of the listed
  cleanup artifact kinds, or arbitrary backups;
- run Doctor, Repair, Index, Framework or Extension lifecycle, package cleanup,
  or Gate 6 documentation, history, or release cleanup as a hidden operation;
- create a replacement backup, staging copy, receipt, journal, or tombstone for
  cleanup, reverse a verified deletion, or replay a saved plan; or
- create a Git commit or change the accepted Architecture. Implementation and
  executable proof for the accepted boundaries remain pending Gate 5.

The CLI Architecture defines cleanup's accepted artifact identity realization,
structured schema, process-status mapping, filesystem and concurrency boundary,
package behavior, and runtime structure. Gate 5 provides implementation and
executable proof for those decisions.

## Verification Requirements

Future conformance evidence must cover:

- exact root syntax, no operands, no wizard or prompts, shared flags, terminal
  modes, and idempotent Boolean repetition;
- exact CWD and `--workspace` selection without parent or marker discovery;
- positive provenance for adjacent backups, known `.bak` compatibility forms,
  operation temporary or staging artifacts, and incomplete or completed residual
  artifacts;
- bounded workspace association, physical containment, physical identity,
  inactive and in-use state, expected bytes, and concurrent changes;
- preservation of unknown, user-created, ambiguous, aliased, externally
  resolving, active, and arbitrary backup content and all named exclusions;
- default-all recognized catalogue formation, deterministic ordering, complete
  plan formation, preflight, exact dry-run parity, and no dry-run writes;
- affected-path Git checks, the narrow skip boundary, Gitless operation without
  replacement-backup creation, and Git not proving provenance;
- per-deletion revalidation, verification, monotonic partial application,
  residual reporting, interruption, fresh-catalogue rerun, and no later
  replacement deletion;
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
- [CLI Decision Agenda](../../../../../working/cli-release/decision-agenda.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
