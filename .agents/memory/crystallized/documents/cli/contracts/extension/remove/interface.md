---
open-forge:
  description: Accepted Interface for releasing selected Extension ownership with safe final-owner deletion and retained recovery
  responsibility: Define remove's exact syntax, managed-ID selection, final-owner deletion, effects, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Remove, Interface, Ownership, Safety, CurrentTruth]
---

# extension remove Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension remove`. It owns exact syntax, managed-ID selection,
source-independent behavior, dependency and ownership boundaries,
final-owner deletion, retained recovery, automatic and wizard
behavior, effects, output, statuses, errors, examples, non-goals, and public
conformance. The new CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and mutation. The [Extension group entrypoint](../_extension.md),
[Global CLI Flags](../../shared/global-flags/interface.md), and current Index
contracts own their shared boundaries. No Technical Design exists.

## Consumer Destination Permissions

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once saves no permission grant; removal
still records its persistent exclusion. Invalid or unavailable settings block planning before permission approval,
because removal exclusions cannot be determined safely. A missing settings file
means no saved exclusions or external grants. Implicit `.agents/` destinations
still require no permission grant.

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

The result adds `permissions` immediately before `lifecycle`, using the exact
shared member order and meanings. Human output presents those same facts before
lifecycle publication. Add these ordered findings immediately before the
existing general target-safety findings: `remove` uses the prefix
`extension-remove.`, followed by `permission-required`,
`permission-declined`, `permissions-invalid`, `permissions-unavailable`,
`permissions-changed`, and `permission-write-failed`, in that order.
Their statuses are respectively `blocked`, `blocked`, `blocked`, `incomplete`,
`blocked`, and `failed`. A failed or unknown permission effect remains failed;
caller cancellation before an effect keeps the existing cancelled outcome.
Missing grants direct to rerun interactively or edit the displayed exact
consumer entries. Invalid storage directs to inspect and correct that file.

## Purpose And Boundary

`remove` releases selected Extension ownership recorded in
`.agents/open-forge.lock.json`. Exact path receipts authorize eligible final-owner
deletion. Region receipts authorize only changes to the generated region, never
deletion of its host file. The shared allow list applies to all owners; `.agents/`
is implicit, and existing reserved-path and physical-containment checks remain.

Package source bytes are not required and are never removed. Framework-owned,
Library-owned, unowned, reserved, and route-unsafe content is protected.

Record every valid selected package ID in `removedExtensions` in
`.agents/open-forge.json`, including an ID that is not currently installed.
Verify that settings effect before content changes and publish ownership release
last. Subsequent install/update operations honor the ID, including when another
package requests it as a dependency. To restore, explicitly clear the ID and any
covering path exclusions, then install the package. An absent, already excluded
package needs no new effect.

## Syntax

```text
open-forge extension remove [<stable-id>...] [--automatic] [--dry-run] [global flags]
```

Explicit managed stable-ID operands are required for direct, JSON, and other
non-interactive application. Argumentless human `remove` may open a finite
wizard to select managed IDs. There is no `--source`, `--all`, `--force`, package
path, semver selector, or `--yes` option on this command. Use root `remove` to
select an ordinary path or managed route directly.

Shared flags are `--workspace <path>`, `--format json`, `--detail <minimal|standard|full|debug>`,
`--detail debug`, `--help`, and `--version`; their full grammar and terminal behavior
remain in [Global CLI Flags](../../shared/global-flags/interface.md).

## Required Workspace Facts And Source Independence

The target is the exact current workspace or exact `--workspace` value. Remove
does not discover another root or package source. It reads Extension ownership and dependencies from the generated lock, and
route, generated-navigation, physical identity, and exact bytes from current files.

Remove may proceed without a healthy current Framework only when those complete
trusted Extension facts can still be established, including route-host and
cross-section preservation facts. A truly absent ownership claim (or a valid
readable lock with no record for the selected ID) permits a settings-only
exclusion. It is a no-op when the ID is already excluded. A malformed or
unavailable ownership record is not absence: it is
`incomplete`, performs no managed mutation, and preserves the exact
ownership-record subject and raw cause. Other missing required facts remain
`incomplete` or `blocked` and perform no managed mutation.

Before a workspace effect, the implementation must hold the actual OS lock for
the persistent external zero-byte path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no metadata,
timestamp, or ownership record. Hold a
`FileShare.None` handle for the operation; file existence is not lock
ownership. A crash releases the OS lock, and another process holding it blocks
mutation. The lock is concurrency safety, not lifecycle authority, history, or
recovery evidence.

A proven-absent ownership source, or a valid readable ownership lock with no
record for the selected ID, supplies no claims and authorizes no deletion. Its
exclusion does not prove an earlier successful removal. A malformed or
unavailable ownership source/record is `incomplete` instead: do not treat it as
empty, infer claims from payload bytes or legacy state, or perform dependency
inference, deletion, release, recovery, or publication. The minimal finding
retains the explicit ownership-record subject and raw cause. Earlier state files
are not read, rewritten, migrated, or deleted.

### Recovery boundary

Before the first target effect, application prepares and verifies exactly one
immutable ZIP recovery bundle for the complete operation when the plan contains
an existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) or a permission-file Create. The bundle
is outside the workspace under
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1`; no temporary,
repository, `HOME`, or custom platform fallback is permitted. Unavailable
storage makes the operation `incomplete` before any target effect.

The final bundle name is deterministic from the normalized physical workspace
path key and operation ID. A `CreateNew` draft in the same external directory
is closed and reopened for semantic manifest, exact ordered entry, length,
hash, and payload-byte validation, moved within that directory to the final
name, and reopened and verified again. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. The
source-generated `manifest.json` records schema-v1, command and operation
identity, workspace identity, ordered relative targets, change kinds, exact
prior byte lengths, hashes and payload names, and intended final absence or
length/hash. Ordered ordinal payload entries contain the exact prior bytes for
every existing-target effect. The bundle is immutable after preparation.

Every planned existing-target effect must match one verified bundle entry. Ordinary content Create and no-op effects create no entry. Permission-file
Create has a reversible prior-absence entry. All preparation completes before the first
mutation. `FileChangeApplier` requires matching preparation for each
existing-target effect and performs one final effect per target. A handled application, verification,
publication, or cancellation outcome reports the actual residual draft or final
path; a valid final remains when preparation completed. A closed final ZIP may remain after abrupt process
termination, without an executable crash or power-loss guarantee. The CLI never
restores, rolls back, compensates for an effect, derives current target state
from recovery provenance, or stores a journal, progress receipt, or history.

After final verification, re-read and verify the prepared bundle, retain it,
and report recovery state `retained` with its exact `residualPath`. Successful
retention does not by itself change the result to `completed-with-warnings` or imply a failure. The bundle contains edited
bytes that Git may never have recorded. Existing recovery-catalogue rules block
later mutations until the user reviews the bundle and runs Cleanup. An absent-ID
Remove no-op requires no mutation and may still complete. Missing or invalid
prepared recovery at final verification is a failure.

Explicit Cleanup
may delete only the exact selected-workspace final or draft candidate while
holding the same-workspace lease and after immediate ordinary path, kind, and
final semantic revalidation. Unknown names and unavailable, malformed, or
mismatched candidates remain untouched. A workspace move is outside the
automatic guarantee: deterministic rediscovery uses the same normalized
physical path, while Doctor/Cleanup may report orphan bundles for the original
root and never auto-bind or restore them.

Recovery storage is ordinary current-user `LocalApplicationData` under the
stable workspace and cooperating-client threat model. No special platform-
permission or encryption behavior is promised. Recovery reads use semantic
schema and exact ordered-entry validation; the
implementation does not extract bundles or add a custom archive parser,
reflection, native dependency, or package for this boundary.

## Selection And Flags

| Input            | Role                      | Rule                                              |
| ---------------- | ------------------------- | ------------------------------------------------- |
| `<stable-id>...` | Select managed packages   | Repeatable IDs; duplicates are invalid.           |
| `--automatic`    | Disable prompts           | Does not choose packages or bypass safety checks. |
| `--dry-run`      | Preview the complete plan | Performs no writes or recovery preparation.       |

Ambiguous portable path aliases in the lock produce an ownership observation
and no effects. Shared-owner lookup must never make another owner's aliased
file eligible for final-owner deletion.

`--prune` is removed. The parser reports it as an unknown option, including in
JSON and automatic requests. No public `prune` result field remains.

## Wizard, Direct, And Automatic Behavior

Prompt-capable human requests may select managed package IDs. Direct and
automatic requests require explicit IDs. Every mode plans the same final-owner
deletion; there is no changed-content Keep/Delete question.

## Ownership, Dependencies, And Removal Effects

Derive owners from the lock's per-extension path lists. Validate the selected
IDs, dependencies, complete owner sets, shared allow list, reserved paths,
physical containment, no-follow final leaf, route-host safety, expected bytes,
and recovery readiness before any effects.

A retained dependent blocks removal of its dependency. An orphan remains
installed. A route host with retained routed descendants cannot be deleted.

- `shared` maps to `retain-shared`: release selected owners and retain the file.
- `final-owner` maps to `delete`: delete an eligible existing ordinary file,
  including edited content, after recovery preparation.
- `missing` maps to `release-ownership`: release the receipt without a deletion
  effect. Absence does not prove a prior successful operation.

Unknown, unowned, other-manager-owned, reserved, unsafe, or unauthorized targets
are never deleted. `.agents/open-forge.json` and `.agents/open-forge.lock.json`,
including their descendants, are reserved regardless of Extension claims.
Framework and Library path ownership comes from their lock sections. Resolve
Library source-relative `paths` beneath the recorded `destinationRoot` before
checking portable destination identity. If a Library mapping is uninterpretable,
report an ownership observation and perform no effects. A real
projection link or reparse point blocks deletion independently of recorded
ownership. No Library operation is invoked.

Generated Entries are projected from intended topology using current Index
rules. Preserve valid boundaries and outside bytes; malformed boundaries block
the complete operation. The package source remains unchanged.

## Lifecycle Trust And Semantic Identity

The ownership lock records receipts, not content baselines. Removal performs no
changed-versus-unchanged classification and publishes no such distinction.
Exact current bytes are still required for expected-state revalidation,
verification, and recovery.

Publish Extension ownership only after target effects verify, preserving the
Framework and Library sections. An identical receipt writes nothing. The
existing state-file outcome reports the lock publication and its actual
verification; no second outcome is added. Recovery lists the lock when its
replacement is protected, and never includes an untouched earlier state file.
A skipped ownership write never authorizes additional deletion.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                           | Headline                                                                                    | Exit | Stream |
| ----------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | removed                                                        | `Removed the <id> Extension.` / `Removed <N> Extensions: <ids>.`                            |    0 | stdout |
| completed               | proven-absent ownership or valid readable lock has no record for the ID | `No files are recorded for <id>, so there is nothing to remove.`                            |    0 | stdout |
| completed (dry run)     | planned                                                        | `Would remove the <id> Extension.`                                                          |    0 | stdout |
| completed-with-warnings | a dependency remains installed and is no longer needed         | `Removed the <id> Extension. <dependency> remains installed and is no longer needed by it.` |    2 | stdout |
| incomplete              | malformed or unavailable ownership record, or route, permission or recovery unreadable | `The <id> Extension could not be removed: <limitation>. Nothing was changed.`; minimal retains the ownership-record subject and raw cause |    3 | stdout |
| invalid-input           | bad ID, duplicate IDs, no selection possible                   | `Cannot remove: <problem>.`                                                                 |    4 | stderr |
| blocked                 | another installed package needs it, permission, conflict, lock | `Cannot remove <id>: <reason>.`                                                             |    5 | stderr |
| failed                  | after effects                                                  | `Extension remove stopped after <n> of <m> changes.`                                        |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                       | `Extension remove was cancelled. Nothing was changed.`                                      |  130 | stderr |

### Text by level

`minimal`:

```text
Removed the toolkit Extension.
  .agents/changed.md     deleted
  .agents/unchanged.md   deleted
  .agents/shared.md      kept; still owned by survivor
  .agents/missing.md     was already gone; its ownership was released
  Updated the Entries section of .agents/loader.md
  The deleted files are kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

`minimal`, unavailable ownership record (stdout):

```text
The toolkit Extension could not be removed: The ownership record is unavailable: <raw cause>. Nothing was changed.
```

The finding subject remains the exact ownership-record path or record identity;
`<raw cause>` is not replaced with “no claims” or another missing-state alias.

`minimal`, dependent blocks (stderr):

```text
Cannot remove planning: the orchestration Extension still needs it.
Next: open-forge extension remove orchestration planning
```

`standard` adds `Workspace:`, per row the owners before and after, the
removal order, and the lock row.

`full` adds the unchanged Entries sections and the recovery and
verification facts in words.

### Prompts

Multi-select of installed packages with `needed by` marks; plan review
listing every deletion; `Delete the <N> files listed above? [y/N]`.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-completed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/__snapshots__/ExtensionRemoveBeforeOutputSnapshotTests/PackageRemoval_single-package/single-package.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-completed-with-warnings). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/__snapshots__/ExtensionRemoveBeforeOutputSnapshotTests/PackageRemoval_orphaned-dependency/orphaned-dependency.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/__snapshots__/ExtensionRemoveBeforeOutputSnapshotTests/PackageRemoval_not-installed/not-installed.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-blocked).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-failed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/__snapshots__/ExtensionRemoveBeforeOutputSnapshotTests/PackageRemoval_write-failed-partial/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-remove-cancelled). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/__snapshots__/ExtensionRemoveBeforeOutputSnapshotTests/PackageRemoval_cancelled/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, automatic, packages: [ { id } ], orphaned: [ id ], permissions { ... } }` plus every effect with `owner` and `keptFor` |
| standard | + `removalOrder`, per effect `ownersBefore`, `ownersAfter`                                                                      |
| full     | + `entriesUnchanged`, `verification`, `recovery` details                                                                        |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

`<path>  deleted`, `<path>  kept; still owned by <owners>`, `<path>  was
already gone; its ownership was released`, `Updated the Entries section of
<path>`, lock `updated`. Dry run: `would delete`, `would keep`, `would
release`. Partial: `deleted`, `not started`, `final state unknown`.

### Counts and limitations

`packagesRemoved`, `filesDeleted`, `filesKept`, `filesReleased`, `sectionsUpdated`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                          | Severity | Family                       | Message                                                                                        | Next                                           |
| --------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| extension-remove.settings-invalid | error | local | Removal intent cannot be recorded in invalid settings. | Correct the named settings before retrying. |
| extension-remove.settings-unavailable | warning | local | Required workspace settings cannot be observed. | Restore access before retrying. |
| extension-remove.path-excluded | info | local | An excluded generated navigation host was left unchanged. | Clear all covering exclusions before explicitly rebuilding its navigation. |
| extension-remove.invalid-input                | error    | invalid-input                | includes `<id> is listed twice.`                                                               |                                                |
| extension-remove.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list --installed`        |
| extension-remove.interaction-ended            | error    | interaction-ended            |                                                                                                |                                                |
| extension-remove.ownership-observation        | info     | ownership-observation        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Remove/Shared/Wording/ExtensionRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-remove.ownership-observation`).            | none                                           |
| extension-remove.lifecycle-observation        | warning  | local                        | [`extension.remove.phrase.remains-installed-and-is-no-longer-needed-by`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Remove/ExtensionRemovePhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Remove/Shared/Wording/ExtensionRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-remove.lifecycle-observation`).                              | `open-forge extension remove <dependency>`     |
| extension-remove.dependency-blocked           | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Remove/Shared/Wording/ExtensionRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-remove.dependency-blocked`).                                                  | `open-forge extension remove <dependent> <id>` |
| extension-remove.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                                |
| extension-remove.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                                |
| extension-remove.lifecycle-unavailable        | warning  | lifecycle-unavailable        | malformed or unavailable ownership record; retain the explicit record subject and raw cause at minimal |                                                |
| extension-remove.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                                |
| extension-remove.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                                |
| extension-remove.permission-required          | error    | permission-required          |                                                                                                |                                                |
| extension-remove.permission-declined          | error    | permission-declined          |                                                                                                |                                                |
| extension-remove.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                                |
| extension-remove.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                                |
| extension-remove.permissions-changed          | error    | permissions-changed          |                                                                                                |                                                |
| extension-remove.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                                |
| extension-remove.target-unsafe                | error    | target-unsafe                |                                                                                                |                                                |
| extension-remove.projection-unavailable       | warning  | projection-unavailable       |                                                                                                |                                                |
| extension-remove.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                                |
| extension-remove.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                                |
| extension-remove.target-changed               | error    | target-changed               |                                                                                                |                                                |
| extension-remove.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                                |
| extension-remove.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                                |
| extension-remove.recovery-artifact-retained   | warning  | recovery-artifact-retained   | (only when cleanup after success failed; an intentionally kept bundle is not this)             |                                                |
| extension-remove.write-failed                 | error    | write-failed                 |                                                                                                |                                                |
| extension-remove.topology-verification-failed | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Remove/Shared/Wording/ExtensionRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-remove.topology-verification-failed`). | `open-forge doctor`                            |
| extension-remove.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                                |
| extension-remove.verification-failed          | error    | verification-failed          |                                                                                                |                                                |
| extension-remove.recovery-failed              | error    | recovery-failed              |                                                                                                |                                                |
| extension-remove.operation-failed             | error    | operation-failed             |                                                                                                |                                                |
| extension-remove.interrupted                  | error    | cancelled                    |                                                                                                |                                                |

The orphaned-dependency warning is the headline clause plus a warning
finding with the dependency as subject: `<dependency> remains installed and
is no longer needed by <id>.` with `Next: open-forge extension remove
<dependency>`.

## Scenarios

### Catalogue situations

`single-package`, `shared-file-kept`, `missing-file-released`,
`orphaned-dependency` (warnings), `dependent-blocks` (blocked), `select-prompt`,
`no-selection-non-interactive`, `not-installed` (proven-absent/no claims),
`unknown-ownership` (incomplete, no effects), `dry-run`,
`permission-required`, `lock-held`, `write-failed-partial`, `cancelled`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

## Non-Goals And Public Conformance

Remove does not fetch or modify package source, install/update packages, delete
unowned or other-manager files, infer receipts from current payload, remove
orphans automatically, repair malformed boundaries, run a formatter, or create
a transaction journal.

Conformance covers selection modes, source independence, proven-absent and
stale lock state, malformed/unavailable ownership as incomplete zero-effect
results with exact record subject/cause, dependencies, shared owners, final-owner deletion of edited bytes,
reserved paths, allow-list rejection, no-follow guards, missing-path release,
recovery ordering and retained exact bytes, dry-run parity, no-op behavior,
unknown `--prune`, statuses/streams, and both JSON and human views. The [Shared
Result Coordinates](../../shared/result-coordinates/interface.md) define the
result schema and exit mapping. Verify source-generated serialization, real
filesystem behavior, Native AOT, OS locking, isolated tests, and package journeys.





## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.remove.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Remove/ExtensionRemoveText.cs).

<!-- @OpenForgeTextRef extension.remove.help.syntax -->
<!-- @OpenForgeTextRef extension.remove.phrase.remains-installed-and-is-no-longer-needed-by -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [ExtensionRemovePhrases.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Remove/ExtensionRemovePhrases.cs)
  <!-- @OpenForgeTextRef extension.remove.phrase.ownership-record-unavailable -->
