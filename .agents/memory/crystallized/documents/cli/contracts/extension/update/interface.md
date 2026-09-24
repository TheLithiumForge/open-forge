---
open-forge:
  description: Accepted Interface for reconciling trusted Extension IDs from one exact source with force and prune boundaries
  responsibility: Define Extension update syntax, source universe, managed selection, normal/force/prune/automatic lifecycle, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Update, Interface, Lifecycle, Ownership, Dependency, Safety, CurrentTruth]
---

# extension update Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension update`. It owns exact syntax, source and selection,
trusted-state and Framework-anchor requirements, dependency closure, normal,
force, prune, force-plus-prune, and automatic meaning, generated and ownership
boundaries, output, statuses, errors, examples, non-goals, and public
conformance. The new CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral resolution and mutation. The [Extension group
entrypoint](../_extension.md), [Global CLI Flags](../../shared/global-flags/interface.md),
and current Index contracts define routing, shared presentation, and generated
navigation boundaries. No Technical Design exists.

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

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once changes no settings. Invalid or unavailable settings block planning before permission approval,
because removal exclusions cannot be determined safely. A missing settings file
means no saved exclusions or external grants. Implicit `.agents/` destinations
still require no permission grant.

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

The result adds `permissions` immediately before `lifecycle`, using the exact
shared member order and meanings. Human output presents those same facts before
lifecycle publication. Add these ordered findings immediately before the
existing general target-safety findings: `update` uses the prefix
`extension-update.`, followed by `permission-required`,
`permission-declined`, `permissions-invalid`, `permissions-unavailable`,
`permissions-changed`, and `permission-write-failed`, in that order.
Their statuses are respectively `blocked`, `blocked`, `blocked`, `incomplete`,
`blocked`, and `failed`. A failed or unknown permission effect remains failed;
caller cancellation before an effect keeps the existing cancelled outcome.
Missing grants direct to rerun interactively or edit the displayed exact
consumer entries. Invalid storage directs to inspect and correct that file.

## Purpose And Boundary

`extension update` reconciles trusted existing managed Extension IDs with the
current bytes from one explicitly selected source universe. It makes source and
intent explicit. A semantic version is descriptive package metadata; it does not
select a source, negotiate compatibility, or invoke a generic package-manager
operation.

Normal update replaces changed owned current paths, restores missing owned
current paths, and creates safe new files. `--prune` additionally deletes eligible
retired whole files after recovery preparation. `--force` adds no safety bypass.

## Syntax

```text
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]
```

IDs are repeatable positional managed package subjects. `--all` explicitly
selects all currently managed IDs represented by the selected source and
dependency closure. Explicit IDs and `--all` conflict and are invalid. `--all`
is not a package or file glob and missing source coverage is not silently
skipped.

The shared flags are `--workspace <path>`, `--format json`,
`--detail <minimal|standard|full|debug>`, `--detail debug`, `--help`, and `--version`. Their shared
grammar, defaults, repetition, terminal behavior, and output rules remain in
[Global CLI Flags](../../shared/global-flags/interface.md).

There is no semver-only selector, network update, registry, cache, source
fallback, replacement leaf, reinstall alias, generic apply, saved plan, or
`--yes` flag.

## Workspace, Source, And Required Trust

The target is the exact current workspace or exact `--workspace` value. No root
discovery occurs. `--source` is one exact package or catalogue read location.
The source and target must be lexically and physically disjoint, and the source
is never mutated.

Update requires:

- recorded Extension ownership for selecting installed IDs and dependencies;
- readable current source bytes for every selected identity and dependency;
- an ordinary contained `.agents` Framework container; and
- complete affected route-host, generated-navigation, ownership, and
  cross-section preservation facts, subject only to the proven unrelated
  readable ordinary metadata exception defined below.

Before a workspace effect, the implementation must hold the actual OS lock for
the persistent external zero-byte path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no metadata,
timestamp, or ownership record. Hold a
`FileShare.None` handle for the operation; file existence is not lock
ownership. A crash releases the OS lock, and another process holding it blocks
mutation. The lock is concurrency safety, not lifecycle authority, history, or
recovery evidence.

Missing selected source coverage is `incomplete`. Unsafe physical, route or
competing ownership facts are `blocked`. Unknown lock ownership is informational
and supplies no installed claims.

Read the Extension receipts from the shared lock. Normalize supported ownership
fields with the forgiving ownership codec; schema metadata is non-binding.
Use canonical stable IDs, dependencies and paths to resolve selected ownership.
Publish one normalized UTF-8 lock transition after target effects verify,
preserving other sections and unselected receipts. Unchanged state writes nothing.
Missing, malformed or unreadable state grants no ownership; report the observation
without using a legacy fallback. Old files remain unrelated user content.

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
path key and operation ID. A `CreateNew` draft in the same directory is closed
and reopened for semantic manifest, exact ordered entry, length, hash, and
payload-byte validation, moved within that directory to the final name, and
reopened and verified again. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. The source-generated
`manifest.json` records schema-v1, command and operation identity, workspace
identity, ordered relative targets, change kinds, exact prior byte lengths,
hashes and payload names, and each intended final absence or length and hash.
Ordered ordinal payload entries contain the exact prior bytes for every
existing-target effect. The bundle is immutable after preparation.

Every planned existing-target effect must match one verified bundle entry. Ordinary content Create and no-op effects create no entry. Permission-file
Create has a reversible prior-absence entry. All bundle preparation completes before the
first mutation. `FileChangeApplier` requires that matching preparation for each
existing-target effect and performs one final effect per target. A handled application, verification,
publication, or cancellation outcome reports the actual residual draft or final
path; a valid final remains when preparation completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
The CLI never restores, rolls back, compensates for an effect, derives current
target state from recovery provenance, or stores a journal, progress receipt, or
history.

After final verification, retain and verify the prepared bundle and report its
exact path. Retention is normal completion. When `.git` is an ordinary directory,
point at `git diff`; otherwise point at that bundle. No Git executable or clean-tree
gate is introduced. Explicit Cleanup
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

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the structured JSON result schema and numeric exit mapping. The [CLI
Architecture](../../../architecture.md) defines concrete source-generated package
serialization relationships. This Interface uses those shared definitions
without duplicating implementation mechanics. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.

When `--source` is omitted, the embedded catalogue is the available source. An
explicit source is the only source for the request. Dependencies resolve only
within that one source universe, offline and transitively.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Selection Rules

| Input             | Meaning                                                                         | Rule                                                                                                         |
| ----------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| `<stable-id>...`  | Select these currently managed package IDs                                      | Repeatable subjects. Unknown, duplicate, or untrusted IDs are invalid, incomplete, or blocked as applicable. |
| `--all`           | Select all currently managed IDs represented by the selected source and closure | Explicit; conflicts with IDs. Missing source coverage is not skipped.                                        |
| `--source <path>` | Select one exact package or catalogue source                                    | Singleton; repetition is invalid.                                                                            |

A selected exact package may supply its containing package directory as the
source universe for dependencies only when that directory is structurally a
valid catalogue. When the selected source contains exactly one completely
validated package and no IDs or `--all` were supplied, its valid manifest ID is
the one permitted deterministic inference in human, non-interactive, and
automatic use. A multi-package source requires explicit IDs or `--all`.
`--automatic` never chooses among packages or broadens an omitted selection to
`--all`.

## Lifecycle Flags

| Flag          | Role                               | Effect                                                                                                                              |
| ------------- | ---------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| `--force`     | Accepted redundant flag            | Ordinary update already replaces/restores owned current paths; adds no safety bypass.                                               |
| `--prune`     | Retired-content deletion authority | Delete eligible retired managed paths only.                                                                                         |
| `--automatic` | Guided-input policy                | Suppress wizard and apply only safe effects authorized by explicit IDs, `--all`, or permitted single-package manifest-ID inference. |
| `--dry-run`   | Preview policy                     | Use the same plan and preflight, then write nothing.                                                                                |

All Boolean flags repeat idempotently. Force never implies prune. Prune adds only retirement deletion to ordinary update.

### Normal update, `--force`, and `--prune`

Normal update compares current content with the selected source, replaces owned
current paths that differ, restores missing owned current paths, and creates safe
new paths. Formatting-only equivalence does not cause a replacement. An existing
unowned target is never adopted. Without `--prune`, retired paths and receipts
remain and produce `completed-with-warnings`. Retained shared owners block
incompatible rewrites.

`--force` remains accepted but adds no authority to overwrite owned current paths;
ordinary update already does that. It cannot bypass ownership, physical, route,
permission, expected-state, verification or recovery checks and never implies prune.

`--prune` releases selected retired ownership. It deletes a retired whole-file
path only when it exists as an ordinary contained file, the shared allow list
(with implicit `.agents` admission) admits it, reserved-path checks pass, and no
remaining owner or route dependency requires it. Changed content is eligible.
Absent paths are never deleted. Out-of-boundary claims cannot authorize deletion.
The verified recovery bundle precedes every deletion.

### `--automatic` and wizard

Argumentless human install/update leaves may open finite wizards. For update the
questions are managed IDs or `--all`, source when needed, and whether to supply
prune authority for retired content. Recommendations are facts, not
authority.

`--automatic` suppresses the wizard and uses explicit IDs or `--all` plus safe
deterministic defaults. It never broadens selection, enables prune, adopts unowned content, or makes a fuzzy choice.
Ordinary owned replacement and restoration follow the explicit update request. JSON and other non-interactive
requests never prompt; missing semantic selection is `invalid-input`, and missing
authority for a requested effect is `blocked`.

## Dependencies, Ownership, And Generated Navigation

Resolve exact stable-ID dependencies offline, transitively, and dependency-first.
Reject unknown IDs, duplicates, duplicate declarations, invalid manifests,
cycles, unsafe package paths, incompatible intended content, and incomplete
closure before writes.

Read the Extension receipts from the shared lock. Normalize supported ownership
fields with the forgiving ownership codec; schema metadata is non-binding.
Use canonical stable IDs, dependencies and paths to resolve selected ownership.
Publish one normalized UTF-8 lock transition after target effects verify,
preserving other sections and unselected receipts. Unchanged state writes nothing.
Missing, malformed or unreadable state grants no ownership; report the observation
without using a legacy fallback. Old files remain unrelated user content.

Two explicit owners may share a physical path only with equal supported
canonical semantic fingerprints and compatible path, route, and metadata facts.
Formatting-only source-byte differences are compatible. Semantic equality never
adopts an unowned path. Retained dependents block changes that would strand
them; orphaned dependencies remain recorded and installed.

The operation projects affected generated `Entries` from intended authored
topology and metadata using current Index behavior. Generated interiors are
derived navigation, not package-owned authored bytes. A malformed, unreadable,
unrepresentable, or otherwise unavailable affected boundary, a native Skill
metadata failure, or an unsafe or ambiguous boundary retains the existing strict
projection or target result and is never repaired by force or prune. The reserved
`.agents/open-forge.json`, `.agents/open-forge.lock.json`, and
`.agents/open-forge.lock` paths and their descendants are not package targets.
Framework, overwrite, recovery, and other-manager paths are also excluded.

For a selected update, an unavailable generated region may be skipped from
generated effects and generated `Entries` only when exact typed dependency proof
establishes all of the following: the affected closure is formed from the
original selected package paths, including exclusions, retired paths,
admission-affected paths, ancestors, and direct projected dependencies using the
existing formation; the unavailable region itself and every malformed
direct child are outside that closure, is ordinary Markdown or a recognized
entrypoint, and has successfully read current bytes; every other direct child
fact is usable; and a pure counterfactual projection treating only those
malformed ordinary facts as `Missing`, with the eligible automatic-ID fallback,
has no other projection blocker. Native Skill metadata never qualifies.

The proof skips only the proven unrelated unavailable region. It preserves that
region's exact bytes and malformed child, keeps all other projections and global
catalogue safety, alias, collision, read, encoding, selected-scope, dependency,
incomplete, unsafe, ambiguous, and native checks strict, and does not add a
global warning to selected Update. Selected no-op/preview and valid selected
changes remain `completed` / Complete0 under this proof. Doctor and Status own
global diagnostics. The proof is not an arbitrary scope filter or early success;
normal selected-plan verification, ownership, no-op, recovery, invalid-input, and
JSON-schema behavior still run.

An exact destination path claim in the lock Libraries section, or a real relative projection link at that
destination, is separately owned by Library management. Update never adopts,
overwrites, updates, or removes that destination in any mode, including normal
operation, `--force`, `--prune`, and combined `--force --prune`. The no-follow
final-leaf guard blocks ordinary Extension `Create`, `Replace`, `Delete`, or
`ReplaceGeneratedRegion` when the leaf is a link or reparse point,
independently of whether the Library record is present, readable, valid, or
claims the path. Update does not reinterpret the Library record or invoke a
Library operation.

## Semantic Identity And Formatter Boundary

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware fingerprints that
preserve Unicode, semantic text, headings, tags, links, destinations, marker
meaning, inline and code-block content, and significant whitespace. Normalize
only line endings and parser-proven formatting trivia. Unsupported, binary, and
unparseable kinds use exact-byte identity and fail closed.

Compute both fingerprints within the operation; persist neither. Fresh exact bytes
remain necessary for diff, revalidation, write/deletion, verification, and
recovery. Equal semantic identity with formatting-only byte differences is not
divergence and does not require persisted formatter state.

The accepted conservative formatter direction allows detection and advice only.
The CLI does not execute a formatter, select one, change files for formatting, or
persist formatter state.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                  | Headline                                                                                                 | Exit | Stream |
| ----------------------- | --------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change, including a selected no-op beside a proven unrelated readable ordinary metadata-invalid region | `The <id> Extension is up to date. Nothing to do.` / `All <N> Extensions are up to date. Nothing to do.` |    0 | stdout |
| completed               | valid selected changes, including changes beside a proven unrelated readable ordinary metadata-invalid region | `Updated the <id> Extension to <version>.` / `Updated <N> Extensions.`                                   |    0 | stdout |
| completed (dry run)     | planned selected no-op or valid selected changes, including beside a proven unrelated readable ordinary metadata-invalid region | `Would update the <id> Extension to <version>.`                                                          |    0 | stdout |
| completed-with-warnings | retired files kept, ownership cannot be established for a selected ID | headline + rows                                                                                          |    2 | stdout |
| incomplete              | source, Framework, record, permission or recovery unreadable          | `The <id> Extension could not be updated: <limitation>. Nothing was changed.`                            |    3 | stdout |
| invalid-input           | bad ID, `--all` with IDs, no selection possible                       | `Cannot update: <problem>.`                                                                              |    4 | stderr |
| blocked                 | permission, conflict, overlap, lock, target changed                   | `Cannot update <id>: <reason>.`                                                                          |    5 | stderr |
| failed                  | after effects                                                         | `Extension update stopped after <n> of <m> changes.`                                                     |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                              | `Extension update was cancelled. Nothing was changed.`                                                   |  130 | stderr |

### Text by level

`minimal`:

```text
Updated the development Extension to 0.2.0.
  .agents/workflows/review.md         replaced (new content in this version)
  .agents/workflows/development.md    replaced (you had changed it)
  .agents/workflows/checklist.md      created (new in this version)
  .agents/workflows/old.md            kept; no longer part of the package
  Previous content: git diff
Next: open-forge extension update development --prune --dry-run  (preview deleting the kept file)
```

`standard` adds `Workspace:`, the source, unchanged files as a count, the
Entries sections updated, the dependency closure, and the grant.

`full` adds both SHA-256 values per changed path and the verification and
recovery facts in words.

### Prompts

Multi-select of installed packages when no ID and no `--all`; Permission;
plan review; Confirm; `Delete the <K> files listed above? [y/N]` under
`--prune`.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-completed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_up-to-date/up-to-date.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-incomplete). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_source-unreadable/source-unreadable.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_no-selection-non-interactive/no-selection-non-interactive.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-blocked). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_lock-held/lock-held.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-failed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_write-failed-partial/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-update-cancelled). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/__snapshots__/ExtensionUpdateBeforeOutputSnapshotTests/PackageUpdate_cancelled/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                           |
| -------- | -------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, prune, automatic, source { kind, path }, packages: [ { id, from, to } ], previousContent, permissions { ... } }` |
| standard | + `unchanged: [ path ]`, `sections: [ path ]`, `dependencies`                                                                    |
| full     | + per effect `before`, `after`, `relation`, `verification`, `recovery` details                                                   |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

As [13](../../../../../../working/cli-development/tasks/task30-g4/13-update.md), with `(new content in this version)` and `no longer
part of the package`.

### Counts and limitations

`packagesUpdated`, `filesReplaced`, `filesRestored`, `filesCreated`,
`filesDeleted`, `filesKept`, `filesUnchanged`, `sectionsUpdated`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                          | Severity | Family                       | Message                                                                                        | Next                                                 |
| --------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| extension-update.settings-invalid | error | local | Required workspace settings are invalid. | Correct the named settings before retrying. |
| extension-update.settings-unavailable | warning | local | Required workspace settings cannot be observed. | Restore access before retrying. |
| extension-update.removed-extension | error | local | A selected package or dependency is excluded. | Clear its removal entry only when restoration is intended. |
| extension-update.bulk-excluded | warning | local | Bulk selection skips an excluded package. | none |
| extension-update.path-excluded | info | local | An excluded destination remains untouched. | none |
| extension-update.excluded-ancestor | error | local | Selected content requires a missing excluded ancestor. | Clear all covering exclusions only when restoration is intended. |
| extension-update.invalid-input                | error    | invalid-input                | includes `Explicit Extension IDs and --all cannot be combined.`                                |                                                      |
| extension-update.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list --installed`              |
| extension-update.interaction-ended            | error    | interaction-ended            |                                                                                                |                                                      |
| extension-update.source-unavailable           | warning  | local                        | [`extension.shared.label.the-selected-source`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Shared/ExtensionSharedText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.source-unavailable`).                                                         | none                                                 |
| extension-update.source-invalid               | error    | local                        | [`extension.shared.label.the-selected-source`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Shared/ExtensionSharedText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.source-invalid`).                                   | fix by hand                                          |
| extension-update.source-overlap               | error    | local                        | [`extension.shared.label.the-selected-source`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Shared/ExtensionSharedText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.source-overlap`).                               | none                                                 |
| extension-update.source-identity-conflict     | error    | local                        | [`extension.shared.label.the-selected-source`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Shared/ExtensionSharedText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.source-identity-conflict`).                      | `open-forge extension inspect <id>`                  |
| extension-update.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                                      |
| extension-update.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                                      |
| extension-update.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                |                                                      |
| extension-update.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                                      |
| extension-update.lifecycle-observation        | warning  | local                        | [`extension.update.phrase.is-not-recorded-as-installed-so-it-was-not-updated`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Update/ExtensionUpdatePhrases.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.lifecycle-observation`).                                    | `open-forge extension list --installed`              |
| extension-update.managed-divergence           | warning  | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.managed-divergence`).                            | `open-forge extension update <id> --prune --dry-run` |
| extension-update.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                                      |
| extension-update.permission-required          | error    | permission-required          |                                                                                                |                                                      |
| extension-update.permission-declined          | error    | permission-declined          |                                                                                                |                                                      |
| extension-update.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                                      |
| extension-update.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                                      |
| extension-update.permissions-changed          | error    | permissions-changed          |                                                                                                |                                                      |
| extension-update.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                                      |
| extension-update.target-unsafe                | error    | target-unsafe                |                                                                                                |                                                      |
| extension-update.projection-unavailable       | warning  | projection-unavailable       |                                                                                                |                                                      |
| extension-update.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                                      |
| extension-update.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                                      |
| extension-update.target-changed               | error    | target-changed               |                                                                                                |                                                      |
| extension-update.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                                      |
| extension-update.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                                      |
| extension-update.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                |                                                      |
| extension-update.write-failed                 | error    | write-failed                 |                                                                                                |                                                      |
| extension-update.topology-verification-failed | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-update.topology-verification-failed`). | `open-forge doctor`                                  |
| extension-update.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                                      |
| extension-update.verification-failed          | error    | verification-failed          |                                                                                                |                                                      |
| extension-update.recovery-failed              | error    | recovery-failed              |                                                                                                |                                                      |
| extension-update.operation-failed             | error    | operation-failed             |                                                                                                |                                                      |
| extension-update.interrupted                  | error    | cancelled                    |                                                                                                |                                                      |

The retired-kept warning has no own code today; it is the `kept` effect row
plus the headline clause. If a code exists in the result for it, map it to
`warning` and the row.

## Scenarios

### Catalogue situations

`up-to-date`, `files-replaced`, `new-version-with-new-files`, `retired-kept`,
`retired-pruned`, `all-packages`, `select-prompt`, `no-selection-non-interactive`,
`permission-required`, `ownership-unknown` (warnings), `dry-run`,
`source-unreadable`, `lock-held`, `write-failed-partial`, `cancelled`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

## Non-Goals And Public Conformance

Update does not install an absent ID, select a source by semver, use a network,
registry, cache, glob, or fallback, adopt unmanaged paths, delete current
expected or unknown content, remove a package source, repair markers, mutate
Framework-owned files, run a formatter, create a saved plan/journal, or create a
Framework uninstall operation.

Conformance must cover exact source universe, IDs/`--all`, dependency closure,
trusted lifecycle and Framework-anchor gates, source-unavailable behavior,
normal/force/prune/automatic semantics, shared ownership, Library-record and
projection collisions, independent no-follow final-leaf guards, retired/final
path boundaries, semantic fingerprints, generated navigation, lifecycle-section
preservation, complete planning, recovery-bundle behavior, dry-run parity, statuses,
streams, JSON, deterministic no-op repetition, and no package-source mutation.
It must also cover the exact unrelated readable ordinary metadata proof for
selected no-op/preview and valid selected changes, preservation of skipped bytes
and malformed children, the absence of a selected-update global warning, reuse of
the proof under lease, and strict refusal for selected or dependency metadata,
native Skill, unreadable, encoding, incomplete, unsafe, ambiguous, and other
projection failures.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the exact JSON result schema and exit mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, OS locking, isolated tests, and package
journeys.




## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.update.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Update/ExtensionUpdateText.cs).

<!-- @OpenForgeTextRef extension.shared.label.the-selected-source -->
<!-- @OpenForgeTextRef extension.update.help.syntax -->
<!-- @OpenForgeTextRef extension.update.phrase.is-not-recorded-as-installed-so-it-was-not-updated -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [ExtensionUpdatePhrases.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Update/ExtensionUpdatePhrases.cs)
  <!-- @OpenForgeTextRef extension.update.phrase.would-update-with-kept -->
  <!-- @OpenForgeTextRef extension.update.phrase.would-save-a-grant-for-to-agents-open-forge-json -->
  <!-- @OpenForgeTextRef extension.update.phrase.saved-a-grant-for-to-agents-open-forge-json -->
