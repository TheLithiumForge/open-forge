---
open-forge:
  description: Accepted Interface for establishing managed Extension packages, resolving dependencies, and applying only eligible initial force
  responsibility: Define Extension install syntax, source universe, selection, management establishment, trust, ownership, effects, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Install, Interface, Lifecycle, Ownership, Dependency, Safety, CurrentTruth]
---

# extension install Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension install`. It owns the public syntax, exact source and
selection rules, Framework-anchor prerequisite, dependency closure, initial
force boundary, automatic and wizard behavior, ownership and generated effects,
output, statuses, errors, examples, non-goals, and public conformance. The new
CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral resolution and mutation behavior. The [Extension group
entrypoint](../_extension.md) defines routing only. Shared Global Flags define
workspace and presentation. The current Framework and routing sources define
the runtime meaning of installed files.

The generated lock is the only state-file input and output. Its ownership and
publication semantics are defined below. Exact prior bytes for planned
replacements remain in the verified external recovery bundle defined by the
[Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md).

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the structured JSON result schema and numeric exit mapping. The [CLI
Architecture](../../../architecture.md) defines concrete source-generated package
serialization relationships. This Interface uses those shared definitions
without duplicating implementation mechanics. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.

## Ownership Source

The generated `.agents/open-forge.lock.json` is the only ownership input and
state-file output. It records package IDs, descriptive versions and sources,
dependencies, whole-file paths, and regions. Read it forgivingly: missing or
unreadable ownership supplies no claims, without falling back to earlier files.
Duplicate recorded IDs or uninterpretable Library mappings produce an informational
observation and no inferred file effects. The Library boundary cannot turn a
missing or unreadable lock into an installation gate. Leftover records are not read, migrated, rewritten or deleted.

Compare current selected target content with intended source content in this
invocation. Recorded release metadata supplies no integrity baseline. A change
to selected dependency or path membership still requires Extension Update.
Preserve unselected receipts, existing regions and other ownership sections.
Revalidate the observed lock even when publication is unchanged.

Publication follows verified target and topology effects. The existing public
state-file outcome refers to the lock; no second outcome is added. An identical
receipt is preserved. A skipped write uses action `none`, outcome
`not-requested`, and does not block target effects. Its unavailable record
verification is `not-requested`, without claiming a verified publication.
A planned or preserved readable receipt is checked again after target effects.
Computed hashes remain operation-time facts; no baseline or policy is stored.

## Consumer Destination Permissions

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once changes no settings. Unknown or
malformed settings withhold external grants while implicit `.agents/` admission
remains independent; an always choice cannot overwrite malformed settings.

Consume the [Workspace Permissions Interface](../../shared/workspace-permissions/interface.md) and
[Behavior](../../shared/workspace-permissions/behavior.md). Require shared destination admission for every external target in the complete
selected dependency closure.
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
existing general target-safety findings: `install` uses the prefix
`extension-install.`, followed by `permission-required`,
`permission-declined`, `permissions-invalid`, `permissions-unavailable`,
`permissions-changed`, and `permission-write-failed`, in that order.
Their statuses are respectively `blocked`, `blocked`, `blocked`, `incomplete`,
`blocked`, and `failed`. A failed or unknown permission effect remains failed;
caller cancellation before an effect keeps the existing cancelled outcome.
Missing grants direct to rerun interactively or edit the displayed exact
consumer entries. Invalid storage directs to inspect and correct that file.

## Purpose And Boundary

`extension install` establishes managed ownership for explicitly selected absent
Extension package IDs and their exact dependency closure. It may verify an exact
trusted managed installation as a no-op. It is not ordinary managed update.

An existing managed ID with changed, missing, retired, or source-divergent state
is not reconciled by install. It returns the facts and directs the caller to
`open-forge extension update`. `--force` does not change that rule. An installed
ID without current source bytes cannot claim an exact no-op.

Install uses one selected source universe, one dependency closure, one route
projection, one payload plan, and one Extension lifecycle-section publication.
It never removes or rewrites the package source.

## Syntax

```text
open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global flags]
```

IDs are positional primary subjects. The selected source and complete dependency
closure are one source universe. `--all` is an explicit alternative to IDs, not
a default and not a last-wins selector. Combining explicit IDs with `--all` is
invalid.

The shared global flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

Their grammar, defaults, repetition, terminal behavior, and no-op applicability
are owned by [Global CLI Flags](../../shared/global-flags/interface.md). Install
adds no aliases, `--yes`, version selector, package manager mode, or replacement
leaf.

## Workspace And Framework Anchor

The target is the exact current workspace or exact `--workspace` value. Install
does not search for a parent, Git root, nested `.agents`, or nearby Framework.
An external source and target workspace must be lexically and physically
disjoint; the external source is read-only.

Managed Extension install requires a trustworthy installed Framework anchor and
route-host facts before it can form a mutation plan. Route-host facts include
affected authored hosts, generated-navigation boundaries, ownership
relationships, and cross-section preservation facts. A selected or affected
unsafe, unreadable, ambiguous, or otherwise required boundary is `incomplete`
or `blocked`, and no managed mutation occurs. An unrelated readable malformed
ordinary-metadata host outside the selected package's affected ancestor closure
may instead be reported as an unavailable projection host and omitted from
generated effects; all otherwise available topology projections remain in the
plan. The selected package is still applied with a visible
`extension-install.metadata-projection-skipped` Attention2 finding. This narrow
skip never covers unreadable or unsafe metadata, native Skill malformed
requirements, selected/affected hosts, or a missing anchor. The installed anchor
includes the existing `.agents` Framework container, so Extension Install
cannot form a plan or create `.agents` when that container is absent. This does
not change the shared directory-creation capability: after the anchor is
established, this command may use it only for explicitly planned missing
descendant directories beneath the workspace for admitted targets.

Before a workspace effect, the implementation must hold the persistent reusable
zero-byte external lock under `LocalApplicationData/OpenForge/locks/v1` defined
by the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md). It holds one read/write `FileShare.None`
handle and never writes metadata, deletes, or truncates the lock file. Another process holding
the handle blocks mutation; the lock is concurrency safety, not lifecycle
authority, recovery evidence, or history.

## Source Universe

`--source` is one exact package or catalogue path. Structural manifest and
package facts distinguish one package directory from one catalogue directory.
The CLI never uses ambient search, network, registry, cache, glob, fuzzy
matching, path resemblance, or an embedded fallback when explicit source input
was supplied.

When omitted, available packages come from the embedded catalogue. Dependencies
resolve only within that one selected universe, transitively, offline, and in
dependency-first order. An exact package source can provide its containing
directory as a dependency universe only when that directory is structurally a
valid catalogue; otherwise the request is invalid or blocked.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

A selected source may provide one ID by deterministic manifest-ID inference only
when it contains exactly one completely validated package with one valid stable
ID. The manifest ID is the only inferred identity. A multi-package source
requires explicit IDs or `--all`; `--automatic` never means `--all`.

Unknown IDs, duplicate active IDs, duplicate dependency declarations, malformed
manifests, missing dependencies, cycles, unsafe package paths, incomplete
closure, conflicting package identities, and source overlap reject the complete
request before writes.

## Selection And Flags

| Input             | Role                                                                          | Repetition and composition                                                                                                                          |
| ----------------- | ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `<stable-id>...`  | Explicit root package IDs                                                     | Repeatable positional subjects. Duplicate IDs are invalid rather than last-wins.                                                                    |
| `--source <path>` | One exact package or catalogue source                                         | Singleton; repetition is invalid.                                                                                                                   |
| `--all`           | Select all applicable available package roots in the selected source universe | Boolean and idempotent. It conflicts with explicit IDs. It never means all files or all sources.                                                    |
| `--force`         | Eligible initial-occupant replacement authority                               | Boolean and idempotent. It never updates managed divergence or adopts old bytes.                                                                    |
| `--automatic`     | Guided-input policy                                                           | Boolean and idempotent. It suppresses interaction but never chooses among packages, broadens to `--all`, or adds force, prune, adoption, or bypass. |
| `--dry-run`       | Preview policy                                                                | Boolean and idempotent. It shares the application plan and writes nothing.                                                                          |

### Initial force

Normal install may write an absent package footprint only when every target is
safely absent and ownership, route, containment, marker, source, and recovery
facts are complete. An exact current source destination already
occupied before management is an eligible initial occupant only when no trusted
owner or competing manager, route collision, marker ambiguity, containment risk,
or recovery collision exists.

`--force` may replace only that exact eligible current source-footprint occupant.
It writes current package bytes and records only the verified new state. It does
not adopt the occupant's old bytes, override another owner, repair a marker,
replace a route collision, or bypass recovery. A known user-owned, Framework-
owned, Extension-owned, unknown, or ambiguous path is not force-eligible.
For an ownership conflict or protected-path refusal, minimal blocked output
retains the exact occupied `finding.Target` path and unchanged cause; it never
collapses the refusal to a count-only headline.

Managed divergence remains blocked and directs to `extension update`, even with
`--force`.

### Automatic and wizard behavior

Omitted `--source` always selects the embedded catalogue. The command never asks
the caller to choose that deterministic default.

A prompt-capable human request may ask only for one unresolved multi-package
selection or one eligible initial-force choice. The selection prompt displays
the complete finite package inventory and accepts only exact stable package IDs
or the exact answer `all`. The exact IDs become the selected roots. The prompt
repeats locally after an invalid answer and displays the complete dependency
closure after selection. Dependencies are required by the selected roots and
cannot be deselected. The initial-force prompt grants authority only for the
exact eligible initial occupants displayed for that request.

End-of-input during either prompt is a no-write `invalid-input` result. Caller
cancellation during either prompt is a no-write `cancelled` result. A caller
may decline eligible initial force; that leaves the occupants unchanged and
returns `blocked`. There is no generic confirmation before applying an already
authorized plan.

JSON, `--automatic`, and other non-interactive requests never prompt. Missing
package selection in a multi-package source is `invalid-input`; an eligible occupant
without explicit `--force` is `blocked`. `--automatic` uses only explicit IDs,
explicit `--all`, or the permitted single-package manifest-ID inference plus
deterministic safe effects. It grants neither selection nor force and never
broadens selection, adds replacement, adoption, ownership, or a safety bypass.

## Lifecycle Identity, Ownership, And Generated Navigation

The generated lock contains Framework, Extension, and Library ownership.
After dependency-first target and generated effects and intended-topology
verification, Install publishes the selected Extension receipt as the last
workspace file effect. It preserves unrelated receipts, verifies publication,
and rereads targets and topology before success. If publication is unchanged,
verify that the observed receipt remains unchanged. If publication is skipped,
report that record verification was not requested and continue safely.

Managed ownership is stable package ID plus its declared dependency and path
membership. Source and version are descriptive receipt metadata. No stored hash,
workspace binding or policy establishes currentness. Matching paths or bytes do
not transfer ownership of manually copied or externally managed files.

Dependencies are ordered before dependents. Shared-owner sets are explicit. Two
owners may share a supported path only with equal syntax-aware semantic identity
and compatible path, route, and metadata facts. Formatting-only source-byte
differences do not conflict. Different intended content is a conflict, and
semantic equality never adopts an unowned existing file.

The plan projects affected generated `Entries` from intended authored topology
and metadata using current Index rules. Generated interiors are derived
navigation, not package-owned authored bytes. A missing, duplicate, reversed,
nested, or ambiguous required boundary blocks; force does not repair it. The
narrow unrelated-readable-malformed ordinary-metadata case may omit only its
unavailable projection host, preserving every other available topology
projection and effect rather than rewriting topology to the selected package.

Every package payload target must be a canonical portable workspace-relative
file. Validate complete closure, exact consumer grants for external files, and
all protected destination rules before permission or content effects. The
established Framework anchor keeps `.agents` itself outside both payload and
directory-effect sets. Only declared missing ordinary parents beneath the
workspace can be directory-create effects; an external grant covers the exact
file and does not grant ownership of its parents or siblings.

Extension payloads also cannot target `.agents/open-forge.json`,
`.agents/open-forge.lock.json`, `.agents/open-forge.lock`, any descendant of
those reserved paths, repository metadata, recovery bundles or drafts, workspace
overwrite companions, Framework blocks, or another manager's paths.

A destination claim mapped from Library ownership in
`.agents/open-forge.lock.json`, or a real relative projection link at that
destination, is separately owned by Library management. Install never adopts,
overwrites, updates, or removes that destination, including when `--force` is
supplied. The no-follow final-leaf guard blocks ordinary Extension `Create`,
`Replace`, `Delete`, or `ReplaceGeneratedRegion` when the leaf is a link or
reparse point, independently of whether the Library record is present,
readable, valid, or claims the path. Install does not reinterpret the Library
record or invoke a Library operation.

## Semantic Fingerprints

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware semantic
fingerprints. Preserve Unicode, semantic text, headings, tags, links and
destinations, marker meaning, inline content, code blocks, and significant
whitespace. Normalize only line endings and parser-proven formatting trivia.
Unsupported, binary, or unparseable kinds use exact-byte identity and fail
closed.

Persist no comparison fingerprints. Capture exact
current bytes freshly for plan, diff, expected-state revalidation, verification,
deletion or write, and recovery. Do not persist comparison policy or workspace binding.
Formatting-only equal semantic identity is not managed divergence. The CLI does
not execute a formatter or persist formatter state; it may give conservative
advice only.

## Recovery Boundary

Before any existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or
`Delete`), install uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a pre-effect
`incomplete` result. The operation prepares exactly one immutable ZIP bundle
outside the workspace under a deterministic normalized
physical workspace path key and operation ID when it contains one or more
existing-target effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`). An operation containing only ordinary content Create effects or no-ops
creates no bundle. Permission-file Create requires its prior-absence bundle
entry even when there is no existing-target effect. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
Ordinary content `Create` and semantic/byte no-op effects have no entry.
Permission-file `Create` has a reversible prior-absence entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires that preparation for every existing-target effect and
performs one final effect per target;
all preparation completes before the first target effect.

After final verification, delete only the positively recognized bundle created
by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery warning
coexists with a finite lifecycle observation, cleanup guidance owns the single
next action; the lifecycle facts remain visible evidence. Before
post-verification deletion begins, a handled
application, verification, publication, or cancellation outcome reports the
actual residual draft or final path; a valid final remains when preparation
completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
No target is restored automatically, no current target state is derived from
recovery provenance, and no journal, progress receipt, history, or replayable
plan is saved. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                                                  | Headline                                                                                       | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | one package                                                                                           | `Installed the <id> Extension.`                                                                |    0 | stdout |
| completed               | with dependencies                                                                                     | `Installed the <id> Extension and <N> packages it requires: <ids>.`                            |    0 | stdout |
| completed               | several selected                                                                                      | `Installed <N> Extensions: <ids>.`                                                             |    0 | stdout |
| completed               | already installed and equal                                                                           | `The <id> Extension is already installed and matches the package. Nothing to do.`              |    0 | stdout |
| completed (dry run)     | planned                                                                                               | `Would install the <id> Extension.` (variants as above)                                        |    0 | stdout |
| completed-with-warnings | no content directory, isolated unrelated malformed projection host, lifecycle observation, retained recovery | `Nothing was installed from <source>: the package has no content directory.` / install headline + visible finding rows |    2 | stdout |
| incomplete              | selected/required source, Framework, route host, permissions or recovery unreadable                         | `The <id> Extension could not be installed: <limitation>. Nothing was changed.`                |    3 | stdout |
| invalid-input           | bad ID, invalid package, no selection possible, prompt ended                                          | `Cannot install: <problem>.`                                                                   |    4 | stderr |
| blocked                 | permission required or declined, existing file, changed since install, conflict, lock, target changed | `Cannot install <id>: <reason>.`                                                               |    5 | stderr |
| failed                  | after effects                                                                                         | `Extension install stopped after <n> of <m> changes.`                                          |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                                              | `Extension install was cancelled. Nothing was changed.`                                        |  130 | stderr |

### Text by level

`minimal`, with dependencies:

```text
Installed the orchestration Extension and 2 packages it requires: planning, project-documents.
  Created 9 files under .agents/workflows, .agents/patterns and .agents/templates
  Updated the Entries section of 3 files
  Saved a grant for tools/review to .agents/open-forge.json
```

`minimal`, permission required outside a terminal (stderr):

```text
Cannot install team-tools: it writes outside .agents and no grant allows that.
  tools/review   (directory: everything under it)
Next: open-forge extension install team-tools --allow-path tools/review
  Or add "tools/review" to allowInstallPaths in .agents/open-forge.json.
```

`minimal`, no content (exit 2):

```text
Nothing was installed from ./mypkg: the package has no content directory.
  Package files belong under ./mypkg/content/.agents/.
```

`minimal`, existing file without `--force` (stderr):

```text
Cannot install my-tools: 1 file already exists where the package would write.
  .agents/workflows/review.md
Next: open-forge extension install my-tools --force --dry-run  (preview replacing it)
```

`standard` adds `Workspace:`, the source, every created file as a row with
its package, every Entries section as a row, the grant scope, and the
dependency order.

`full` adds the unchanged Entries sections, the permission evaluation, the
Framework fingerprint, recovery and verification facts in words.

An isolated unrelated malformed ordinary-metadata finding remains visible in
minimal apply, dry-run, and verified no-op output. It names the exact source or
projection host and cause, while preserving the selected package effects and
all other available topology projections. It never presents an unreadable or
unsafe boundary as skippable.

### Prompts

Per [04](../../../../../../working/cli-development/tasks/task30-g4/04-interaction-system.md): multi-select when the source has several
packages and no ID was given; Permission for paths outside `.agents`; Confirm
for existing files (`Replace the 1 existing file listed above? [y/N]`); plan
review; `Apply these changes? [y/N]`.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-completed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_single-package/single-package.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-completed-with-warnings). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_no-content-directory/no-content-directory.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-incomplete). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_source-unreadable/source-unreadable.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_no-selection-non-interactive/no-selection-non-interactive.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-blocked). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_lock-held/lock-held.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-failed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-install-cancelled). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/__snapshots__/ExtensionInstallBeforeOutputSnapshotTests/PackageInstallation_cancelled/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                                                                                        |
| -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, automatic, source { kind, path }, packages: [ { id, version, selected: bool, requiredBy: [...] } ], permissions { decision, required: [...], missing: [...], saved: bool } }` |
| standard | + per effect `owner`, `sections: [ path ]`, `selection { method } `                                                                                                                           |
| full     | + `entriesUnchanged: [ path ]`, `frameworkFingerprint`, `verification`, `recovery` details                                                                                                    |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

Created files: at `minimal` counted by directory (`Created 3 files under
.agents/workflows`), at `standard` rows `<path>  created (<package>)`;
replaced existing files always rows `<path>  replaced (your previous file is
in the recovery bundle)`; `Updated the Entries section of <path>` (`minimal`
counts them when more than three); `Saved a grant for <path> to
.agents/open-forge.json`; lock `updated` at `standard`. Dry run: `Would
create`, `Would update`, `Would save`. Partial: `created`, `not started`,
`final state unknown`.

### Counts and limitations

`packagesInstalled`, `filesCreated`, `filesReplaced`, `sectionsUpdated`,
`grantsSaved`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                           | Severity | Family                       | Message                                                                                        | Next                                                  |
| ---------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| extension-install.invalid-input                | error    | invalid-input                |                                                                                                |                                                       |
| extension-install.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list`                           |
| extension-install.interaction-ended            | error    | interaction-ended            | (status `cancelled`; record the classification change)                                         |                                                       |
| extension-install.source-unavailable           | warning  | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Install/Shared/Wording/ExtensionInstallWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-install.source-unavailable`).                                                         | none                                                  |
| extension-install.source-invalid               | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Install/Shared/Wording/ExtensionInstallWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-install.source-invalid`).                                   | fix by hand                                           |
| extension-install.package-content-missing      | warning  | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Install/Shared/Wording/ExtensionInstallWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-install.package-content-missing`).    | none                                                  |
| extension-install.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                                       |
| extension-install.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                                       |
| extension-install.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                |                                                       |
| extension-install.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                                       |
| extension-install.lifecycle-observation        | info     | ownership-observation        |                                                                                                |                                                       |
| extension-install.managed-divergence           | error    | managed-divergence           |                                                                                                | `open-forge extension update <id>`                    |
| extension-install.package-contents-changed     | error    | managed-divergence           | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Install/Shared/Wording/ExtensionInstallWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-install.package-contents-changed`).                | `open-forge extension update <id>`                    |
| extension-install.initial-force-required       | error    | target-occupied              | row per existing file                                                                          | `open-forge extension install <id> --force --dry-run` |
| extension-install.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                                       |
| extension-install.permission-required          | error    | permission-required          |                                                                                                |                                                       |
| extension-install.permission-declined          | error    | permission-declined          |                                                                                                |                                                       |
| extension-install.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                                       |
| extension-install.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                                       |
| extension-install.permissions-changed          | error    | permissions-changed          |                                                                                                |                                                       |
| extension-install.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                                       |
| extension-install.target-unsafe                | error    | target-unsafe                | includes reserved paths: `<path> is reserved for Open Forge's own files.`                      |                                                       |
| extension-install.projection-unavailable | warning | projection-unavailable | Required projection could not be established; remains incomplete. | |
| extension-install.metadata-projection-skipped | warning | metadata-projection-skipped | Exact readable malformed source whose unrelated host projection was skipped; retain cause. | |
| extension-install.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                                       |
| extension-install.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                                       |
| extension-install.target-changed               | error    | target-changed               |                                                                                                |                                                       |
| extension-install.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                                       |
| extension-install.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                                       |
| extension-install.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                |                                                       |
| extension-install.write-failed                 | error    | write-failed                 |                                                                                                |                                                       |
| extension-install.topology-verification-failed | error    | local                        | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Install/Shared/Wording/ExtensionInstallWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-install.topology-verification-failed`). | `open-forge doctor`                                   |
| extension-install.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                                       |
| extension-install.verification-failed          | error    | verification-failed          |                                                                                                |                                                       |
| extension-install.recovery-failed              | error    | recovery-failed              |                                                                                                |                                                       |
| extension-install.operation-failed             | error    | operation-failed             |                                                                                                |                                                       |
| extension-install.interrupted                  | error    | cancelled                    |                                                                                                |                                                       |

## Scenarios

### Catalogue situations

`single-package`, `with-dependencies`, `select-from-source-prompt`,
`no-selection-non-interactive` (invalid-input), `permission-required-non-interactive`
(blocked), `permission-prompt-always`, `permission-prompt-once`,
`allow-path-flag`, `existing-file-without-force` (blocked), `with-force`,
`already-installed` (no-op), `changed-since-install` (blocked, points at
update), `no-content-directory` (warnings),
`unrelated-readable-malformed-metadata` (warning with selected effects),
`dry-run`, `source-unreadable`, `lock-held`, `write-failed-partial`, `cancelled`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The catalogue says `--all cannot be combined with package IDs.`; the native command says `Explicit Extension IDs and --all cannot be combined.` The same wording conflict is present in Extension Update and is kept unresolved. The no-content case is current native behavior: `Nothing was installed from <extension-source>: the package has no content directory.` followed by the required content-path row. **Maintainer decision remains open.**

The Extension Install catalogue requires the continuation `Or add
".apm/agents/team.md" to allowInstallPaths in .agents/open-forge.json.` after
`Next:`, while the shared report invariant requires `Next:` to be the final
line. The current output is recorded without deciding whether the continuation
should move before `Next:` or the invariant should change. **Maintainer
decision remains open.**

## Non-Goals And Public Conformance

Install does not update managed divergence, delete retired content, remove a
package source, select IDs by automatic inference from a multi-package source,
use semver to choose a source, access a network/registry/cache, adopt unmanaged
content, mutate Framework ownership, repair markers, run a formatter, use a
hidden `index` command, or create a saved plan/journal.

Conformance must cover exact source universe and ID rules, explicit `--all`,
single-package inference, dependency-first closure and failures, Framework-anchor
and route-host prerequisites, isolated unrelated readable malformed projection
hosts with preserved available topology, absent/no-op/divergent/initial-force states,
automatic and exact bounded prompt behavior, trusted/untrusted/absent handling,
shared owners, Library-record and projection collisions, independent no-follow
final-leaf guards, semantic fingerprints, generated navigation, eligible
workspace-relative payload targets, rejection before planning, directory effects
limited to missing descendants beneath the workspace for admitted targets,
reserved paths, exact occupied target paths in minimal refusal output, complete planning, dependency-first target/generated effects, topology
verification, last-effect Extension lifecycle publication, final target and
lifecycle rereads, external recovery-bundle storage and verification, typed
post-verification deletion state/disposition facts, dry-run parity, the exact
ordered result and finite findings, seven statuses/streams, JSON, no mutation of
sources, and no runtime or shipping claim. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.install.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Install/ExtensionInstallText.cs).

<!-- @OpenForgeTextRef extension.install.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [ExtensionInstallPhrases.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Install/ExtensionInstallPhrases.cs)
  <!-- @OpenForgeTextRef extension.install.phrase.ownership-conflict -->
