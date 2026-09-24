---
open-forge:
  description: Accepted non-shipping Interface for trusted managed Framework reconciliation with force and prune boundaries
  responsibility: Define update's exact syntax, current/intended comparison, bounded authority, results, errors, and read-only boundaries
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Update, Framework, Interface, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Update Interface Contract

## Ownership Receipt Boundary

The generated `.agents/open-forge.lock.json` distinguishes whole-file paths
from regions. Root `AGENTS.md` and `CLAUDE.md` hosts carry `open-forge` region
receipts for their existing managed blocks; the host files are not whole-file
ownership. Generated Entries use `entries` region receipts. Publication follows
verified operation effects, retains unaffected verified ownership, and does not
convert a region-only edit into ownership of its authored host. A missing or
unwritable lock does not authorize wider ownership or block the operation.

## Status And Authority

This is the accepted current Crystallized Interface Contract for the non-shipping
root `update` command. It owns the public purpose, exact syntax, input and flag
meaning, ownership and current content facts, normal/force/prune/automatic behavior,
observable effects, output, statuses, errors, examples, non-goals, and public
conformance.

The sibling [Behavior Contract](behavior.md) defines the technology-neutral
operation behind this surface. The [Install Interface](../install/interface.md)
owns management establishment and exact install no-op behavior. The shared
[Global CLI Flags](../shared/global-flags/interface.md), current [Index
Interface](../index-candidate/interface.md), and Framework sources own their
shared meanings. The new CLI remains non-shipping.

The generated ownership lock is `.agents/open-forge.lock.json`, schema v1.
It has a common envelope and separate `framework`, `extensions`, and `libraries`
sections.
An update operation changes only `framework`; a selected semantic change
preserves unrelated Extension and Library sections semantically while
serializing one deterministic canonical whole document, and a semantic no-op
writes nothing. The document stores no plan, runtime history, journal, recovery
evidence, or session. Files outside this exact path are ordinary workspace
content, not ownership-lock input.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact structured JSON result schema and numeric exit mapping. This Interface uses those shared definitions without
duplicating implementation mechanics. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. The new CLI remains
non-shipping and this contract does not claim that implementation or proof.

## Purpose And Boundary

`update` reconciles a trusted existing managed Framework state in one exact
workspace with the current Framework payload embedded in the running CLI. It
uses one transparent current/intended comparison and one complete plan.

Ordinary Update replaces/restores owned current targets, creates safe absent
current payload targets, and preserves present retired content unless `--prune`
was selected. `--force` grants no additional authority. Shared destination,
ownership, physical and managed-region checks apply in every mode. Removed
categories stay excluded, and unrelated user content remains unchanged.

## Syntax

The complete public command form is:

```text
open-forge update [--force] [--prune] [--automatic] [--dry-run] [global flags]
```

`update` is a direct root command with no operands and no source argument. The
embedded current Framework is the only source. There is no `framework` group,
root `init`, generic `apply`, saved plan, `--yes`, or Framework remove/uninstall
leaf.

The shared global flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

Their complete grammar, defaults, repetition, terminal behavior, and errors are
owned by the [Global CLI Flags Interface](../shared/global-flags/interface.md).
`--help` and `--version` stop before workspace selection and update work.

## Exact Workspace And Embedded Source

Without `--workspace`, update uses the process current working directory. With
`--workspace <path>`, it uses that exact path, resolving relative values from
the process current working directory. It normalizes the selected path only for
reporting. It never searches upward, chooses a Git root, follows a nested
`.agents`, or discovers a nearby Framework.

The source is the current Framework payload embedded in the running CLI. There
is no source operand, network lookup, catalogue, semver solver, compatibility
negotiation, or fallback source. A required embedded source fact that is safely
unavailable is `incomplete`; malformed or unsafe source identity is `blocked`.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

Update consumes the same neutral BCL embedded-resource inventory as Install and
Framework-aware Route Init. Runtime never reads the development checkout.

## Ownership And Current State

Update reads `.agents/open-forge.lock.json` as forgiving ownership receipts.
Missing, stale, unreadable or unsupported metadata never gates on integrity.
Unknown ownership contributes no claims; matching current bytes do not establish
ownership. Uninterpretable destinations report `update.ownership-observation`
(complete, no inferred effects). Physical safety, reserved paths, portable
aliases, other owners and actual managed boundaries still control effects.
No legacy state file is read or migrated.

Only verified file/region effects establish new receipts. The existing single
`lifecycle` output describes lock publication: publish when planned, preserve
when unchanged, none/not-requested when skipped. Unaffected receipts and other
sections remain. No stored baseline, policy or workspace binding survives.

## Current And Intended Comparison

| Current fact                   | Ordinary Update                              | With `--prune`                                |
| ------------------------------ | -------------------------------------------- | --------------------------------------------- |
| Semantic equality              | No-op; preserve formatting                   | Same                                          |
| Edited owned current content   | Replace after recovery preparation           | Same                                          |
| Missing owned current target   | Restore                                      | Same                                          |
| Safe absent new payload target | Create and record actual effect              | Same                                          |
| Missing ordinary payload parent in an installed Framework | Plan and create the directory before its selected content | Same |
| Present retired whole file     | Preserve with a warning; result is `completed-with-warnings` | Delete only inside the shared safety boundary |
| Absent retired path            | No deletion or recreation                    | May release obsolete receipt; no deletion     |
| Region-only receipt            | Rewrite named region; preserve outside bytes | Never delete the host                         |

Current states use `same`, `changed`, `missing` and `format-only` from fresh
current/intended comparison. Exact bytes support recovery and expected-state
checks. Source alignment comes from current payloads and may be null for retired
targets. `removedCategories` prevents reinstating named `.agents/<category>/`
payload roots. `removedFiles` excludes exact canonical workspace-relative file
destinations from whole-file and generated-region effects, including root managed
hosts. It has no glob or recursive-directory semantics. `removedDirectories`
covers each listed directory and all descendants, including future files. Excluded ownership facts
are not rewritten, and removing an entry explicitly permits a later restoration.

When a readable Framework registration establishes an installed workspace,
restoration includes the ordinary missing directories needed by selected current
payloads. Directory creation appears in the same preview and result as file
effects. It does not bypass exclusions, no-follow checks or expected-state
revalidation. An empty `.agents` directory without Framework registration does
not authorize this behavior.

Mutation still holds the actual external OS lock described in the
[mutation design](../../technical-designs/mutation-and-recovery.md).

## Flags

| Flag                | Role                               | Value                          | Omission                                 | Repetition and composition                                                                     |
| ------------------- | ---------------------------------- | ------------------------------ | ---------------------------------------- | ---------------------------------------------------------------------------------------------- |
| `--force`           | Accepted flag; no extra authority  | Boolean                        | Ordinary replacement/restoration applies | Repeats idempotently. It never grants prune or another bypass.                                 |
| `--prune`           | Retired-content deletion authority | Boolean                        | Preserve retired content                 | Repeats idempotently. Ordinary update effects still apply.                                     |
| `--automatic`       | Guided-input policy                | Boolean                        | Human mode may confirm the complete plan | Repeats idempotently. It selects only deterministic safe effects already authorized by update. |
| `--dry-run`         | Preview write policy               | Boolean                        | Apply after the same preflight           | Repeats idempotently. It writes nothing and cannot prove application or recovery.              |
| Shared global flags | Workspace and presentation         | Defined by the shared contract | Shared defaults                          | Shared rules apply.                                                                            |

### `--force`

Accepted without additional authority. Ordinary Update already replaces or
restores owned current content; force bypasses no safety check.

### `--prune`

Additionally deletes eligible present retired whole files after verified
recovery preparation. Shared `allowInstallPaths` applies to every owner with
`.agents/` implicit. Reserved and physical checks remain. Absent paths and
region-only hosts are never deleted. Edited retired content remains eligible.

## Generated Navigation

Update forms the hypothetical post-update authored workspace, including only
permitted payload, bounded-region, force, and prune effects. It preserves
user-added routes and intentionally absent defaults. It projects affected
generated `Entries` bodies from that intended authored topology and metadata
using the current Index contracts.

Generated interiors are derived navigation, not Framework authored payload
identity and not a package-owned file. Update changes only valid bounded
generated heading bodies and preserves headings and outside bytes. Missing or
duplicate Entries boundaries block the whole
plan. `--force` and `--prune` do not repair them, and update does not invoke a
hidden `index` command.

## Ownership And Deletion Boundary

Framework, Extension, user, and external-manager ownership remain distinct. A
matching semantic fingerprint never proves ownership. Prune supplies deletion
authority only for the exact trusted retired Framework path and does not infer
ownership from its presence. A shared or competing owner, route dependency,
unknown path, or unsafe host blocks deletion.

The root Remove command can remove selected Framework content; it does not
uninstall the entire Framework. The `.agents` container remains protected.
Manual guidance must never recommend deleting `.agents` wholesale. A future
Framework uninstall requires a separate product and safety review.

## Recovery Boundary

When the operation has one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`),
update uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is
pre-effect `incomplete`. The complete command operation gets exactly one
immutable ZIP bundle outside the workspace under a deterministic
normalized physical-workspace-path key and operation ID. A source-generated
schema-v1 `manifest.json` records command/operation/workspace identity,
ordered relative targets, change kinds, prior lengths/hashes/payload names, and
intended final absence or length/hash. Streamed ordinal payload entries contain
the exact prior bytes for every existing-target effect; fingerprints are
provenance, not an evolving journal. `Create` and semantic/byte no-op effects
create no bundle.

The draft is created with `CreateNew` under its exact name in the same external
directory, closed and reopened for semantic manifest, exact ordered entry,
length, hash, and payload-byte validation, moved within that directory to its
deterministic final name, and reopened and verified again. Only the valid final
ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. Every planned existing-target effect must match the preparation;
`FileChangeApplier` performs one final effect per target. All bundle
preparation completes before the first target effect.

After final verification, reopen and verify the created bundle identity and
retain it. Successful retention is complete with the exact bundle path. The
next action advises `git diff` when an ordinary `.git` directory exists, or
points at the recovery bundle otherwise. No Git executable runs. Partial
failure and interruption retain observed recovery and do not restore targets
automatically. Explicit Cleanup owns deletion under its separate contract.
A workspace move is outside
the automatic guarantee; Doctor/Cleanup may report orphaned original-root
bundles but never auto-binds or restores them.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                      | Headline                                                                                                                                                       | Exit | Stream |
| ----------------------- | --------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change                                         | `The Framework is up to date. Nothing to do.`                                                                                                                  |    0 | stdout |
| completed               | files replaced, restored or deleted                       | `Updated <N> Framework files.` (`Updated 1 Framework file.`)                                                                                                   |    0 | stdout |
| completed (dry run)     | changes planned                                           | `Would update <N> Framework files.`                                                                                                                            |    0 | stdout |
| completed               | no ownership record (Info observation)                    | `No ownership record exists, so update cannot tell which files it manages. Nothing was changed.`                                                               |    0 | stdout |
| completed-with-warnings | retired files kept without `--prune`                      | `Updated <N> Framework files. <K> files from an earlier version were kept.` or `The Framework is up to date, but <K> files from an earlier version were kept.` |    2 | stdout |
| completed-with-warnings | recovery bundle retained                                  | + family row                                                                                                                                                   |    2 | stdout |
| incomplete              | bundled Framework, record, target or recovery unreadable  | `Update could not start: <limitation>. Nothing was changed.`                                                                                                   |    3 | stdout |
| invalid-input           | bad input; confirmation unavailable                       | families                                                                                                                                                       |    4 | stderr |
| blocked                 | invalid record, conflict, unsafe target, prune ineligible | `Cannot update: <reason>.`                                                                                                                                     |    5 | stderr |
| failed                  | after effects                                             | `Update stopped after <n> of <m> changes.`                                                                                                                     |    1 | stderr |
| cancelled               | prompt refused, Ctrl+C                                    | `Update was cancelled. Nothing was changed.`                                                                                                                   |  130 | stderr |

### Text by level

`minimal`, changes:

```text
Updated 3 Framework files.
  .agents/guidance/_guidance.md     replaced (you had changed it)
  .agents/patterns/_patterns.md     restored (it was missing)
  .agents/workflows/_workflows.md   replaced (new content in this release)
  Previous content: git diff
```

`minimal`, retired kept:

```text
Updated 1 Framework file. 1 file from an earlier version was kept.
  .agents/guidance/_guidance.md     replaced (new content in this release)
  .agents/guidance/old-advice.md    kept; no longer part of this release
Next: open-forge update --prune --dry-run  (preview deleting it)
```

`minimal`, dry run:

```text
Would update 3 Framework files.
  .agents/guidance/_guidance.md     replace (you have changed it; a recovery bundle is written first)
  .agents/patterns/_patterns.md     restore
  .agents/workflows/_workflows.md   replace
No files were changed.
```

`standard` adds `Workspace:`, every other Framework file as `  <path>  unchanged`
grouped after the changed rows as one count line (`18 files unchanged`), the
Entries sections rewritten as rows, and the lock row.

`full` adds the current and shipped SHA-256 per changed path, the source
asset path, the bundled Framework identity and fingerprint, and the recovery
and verification facts in words.

### Prompts

Plan review at `minimal` then `Apply these changes? [y/N]`. When retired
files would be deleted under `--prune`, the question reads `Delete the <K>
files listed above? [y/N]`.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-completed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/__snapshots__/UpdateBeforeOutputSnapshotTests/UpToDate/up-to-date.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-invalid-input).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-blocked).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#update-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                                    |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, prune, automatic, previousContent: "git-diff" \| null, lockPath }`                                                        |
| standard | + `unchanged: [ { path } ]`, `entriesSections: [ { path, state } ]`                                                                       |
| full     | + per-effect `before`, `after`, `sourceAssetPath`, `relation { current, shipped }`, `source { id, version, fingerprint }`, `verification` |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

| Relation (current, shipped)   | Action    | Row                                                                 |
| ----------------------------- | --------- | ------------------------------------------------------------------- |
| changed, same shipped content | replaced  | `<path>  replaced (you had changed it)`                             |
| same current, changed shipped | replaced  | `<path>  replaced (new content in this release)`                    |
| changed both                  | replaced  | `<path>  replaced (you had changed it and this release changes it)` |
| missing                       | restored  | `<path>  restored (it was missing)`                                 |
| new in this release           | created   | `<path>  created (new in this release)`                             |
| retired, `--prune`            | deleted   | `<path>  deleted (no longer part of this release)`                  |
| retired, no `--prune`         | kept      | `<path>  kept; no longer part of this release`                      |
| format-only difference        | unchanged | counted; at `full`: `<path>  unchanged (line endings differ)`       |
| Entries section rewritten     | section   | `<path>  Entries section updated`                                   |
| lock                          | record    | `.agents/open-forge.lock.json  updated`                             |

Dry-run rows use the bare verb (`replace`, `restore`, `delete`, `keep`).
The previous-content line is `Previous content: git diff` when `.git` exists
and is omitted otherwise.

### Counts and limitations

`filesReplaced`, `filesRestored`, `filesCreated`, `filesDeleted`,
`filesKept`, `filesUnchanged`, `sectionsUpdated`.

### Next rules

Retired kept -> `open-forge update --prune --dry-run`; confirmation ->
`--automatic`; partial or retained recovery -> `open-forge doctor` /
`open-forge cleanup`; no record -> `open-forge doctor`; otherwise none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                | Severity | Family                       | Message                                                                                                               | Next                                  |
| ----------------------------------- | -------- | ---------------------------- | --------------------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| update.invalid-input                | error    | invalid-input                |                                                                                                                       |                                       |
| update.confirmation-required        | error    | confirmation-required        |                                                                                                                       | `open-forge update --automatic`       |
| update.workspace-unavailable        | error    | workspace-unavailable        |                                                                                                                       |                                       |
| update.workspace-unsafe             | error    | workspace-unsafe             | also lock unavailable                                                                                                 |                                       |
| update.payload-unavailable          | warning  | payload-unavailable          |                                                                                                                       |                                       |
| update.payload-invalid              | error    | payload-invalid              |                                                                                                                       |                                       |
| update.lifecycle-missing            | warning  | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.lifecycle-missing`). | `open-forge doctor`                   |
| update.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                                       |                                       |
| update.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                                       |                                       |
| update.ownership-observation        | info     | ownership-observation        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.ownership-observation`).                                | `open-forge doctor`                   |
| update.ownership-conflict           | error    | ownership-conflict           |                                                                                                                       |                                       |
| update.target-unavailable           | warning  | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.target-unavailable`).                                                                    | `open-forge doctor`                   |
| update.target-unsafe                | error    | target-unsafe                |                                                                                                                       |                                       |
| update.source-provenance-invalid    | error    | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.source-provenance-invalid`).                                                            | `open-forge doctor`                   |
| update.fingerprint-unsupported      | error    | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.fingerprint-unsupported`).                                                       | `open-forge doctor`                   |
| update.retired-content-preserved    | warning  | local                        | row `<path>  kept; no longer part of this release`                                                                    | `open-forge update --prune --dry-run` |
| update.retirement-ineligible        | error    | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.retirement-ineligible`).                                    | fix by hand                           |
| update.projection-unavailable       | warning  | projection-unavailable       |                                                                                                                       |                                       |
| update.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                                       |                                       |
| update.plan-blocked                 | error    | local                        | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Update/Shared/Wording/UpdateWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`update.plan-blocked`).                                                                               | `open-forge doctor`                   |
| update.recovery-conflict            | error    | recovery-conflict            |                                                                                                                       |                                       |
| update.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                                       |                                       |
| update.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                                       |                                       |
| update.write-failed                 | error    | write-failed                 |                                                                                                                       |                                       |
| update.verification-failed          | error    | verification-failed          |                                                                                                                       |                                       |
| update.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                                       |                                       |
| update.recovery-failed              | error    | recovery-failed              |                                                                                                                       |                                       |
| update.operation-failed             | error    | operation-failed             |                                                                                                                       |                                       |
| update.interrupted                  | error    | cancelled                    |                                                                                                                       |                                       |

## Scenarios

### Catalogue situations

`up-to-date`, `changed-file-replaced`, `missing-file-restored`,
`retired-kept`, `retired-pruned`, `dry-run-changes`, `no-ownership-record`,
`confirmation-unavailable`, `write-failed-partial`, `cancelled`, `invalid-input`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native up-to-date report includes a `Workspace: <workspace>` echo when `--workspace` is explicit, although the catalogue's minimal one-line example omits it. This contract records the native echo and leaves the precedence question unresolved. **Maintainer decision remains open.**
## Non-Goals And Architecture Boundary

Update does not:

- establish an absent Framework state, adopt an untracked installation, or turn
  `install --force` into a managed update;
- accept a source operand, package version, range, network source, registry,
  cache, generic apply, saved plan, `--yes`, or Framework remove/uninstall;
- replace arbitrary user, Extension, shared, unknown, retired-only, or
  route-unsafe content;
- repair lifecycle or Entries headings, rewrite overwrite companions, or run a
  formatter;
- mutate the `extensions` section, an external source, or `.temp/`; or
- create runtime meaning, a session, operation history, journal, or formatter
  state.

If a supported formatter configuration is detected, update may give conservative
advice only. It does not execute a formatter or persist formatter state.

The [Ownership And Source Alignment Technical
Design](../../technical-designs/lifecycle-provenance.md) owns ownership serialization and current source alignment, and the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) owns exact lock,
recovery-bundle, and temporary-artifact mechanics. The [CLI
Architecture](../../architecture.md) owns parser roles, filesystem identity and
containment invariants, concurrency boundaries, diagnostics, and Native AOT
structure. Gate 5 must prove those boundaries, the shared result schema and exit
mapping from the [Shared Result
Coordinates](../shared/result-coordinates/interface.md), and the embedded
inventory/hash realization from the [Embedded Payload Technical
Design](../../technical-designs/embedded-payload.md).

## Public Conformance

Evidence covers syntax, the confirmation matrix, current/intended comparison,
ordinary owned replacement/restoration, explicit prune, removed categories,
unknown ownership, shared allow-list and other-owner boundaries, reserved and
physical paths, region-only hosts, and stale or absent receipts. Recovery tests
assert preparation before deletion, prior bytes, successful retention and review
advice. Both output views retain one truthful publication result, exact effects,
findings and verification. Repetition and dry-run remain effect-free when
appropriate. Managed and Native AOT checks exercise real filesystem behavior.






## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`update.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Update/UpdateText.cs).

<!-- @OpenForgeTextRef update.help.syntax -->
