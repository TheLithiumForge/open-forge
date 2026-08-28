---
open-forge:
  description: Accepted non-shipping Interface for trusted managed Framework reconciliation with force and prune boundaries
  responsibility: Define update's exact syntax, baseline comparison, bounded authority, results, errors, and read-only boundaries
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Update, Framework, Interface, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Update Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for the non-shipping
root `update` command. It owns the public purpose, exact syntax, input and flag
meaning, trust and baseline facts, normal/force/prune/automatic behavior,
observable effects, output, statuses, errors, examples, non-goals, and public
conformance.

The sibling [Behavior Contract](behavior.md) defines the technology-neutral
operation behind this surface. The [Install Interface](../install/interface.md)
owns management establishment and exact install no-op behavior. The shared
[Global CLI Flags](../shared/global-flags/interface.md), current [Index
Interface](../index-candidate/interface.md), and Framework sources own their
shared meanings. The new CLI remains non-shipping.

The only new-CLI lifecycle document is `.agents/open-forge.lifecycle.json`, schema
v1. It has a common envelope and isolated `framework` and `extensions` sections.
An update operation changes only `framework`; a selected semantic change
preserves unrelated `extensions` and common-envelope meaning semantically while
serializing one deterministic canonical whole document, and a semantic no-op
writes nothing. The document stores no plan, runtime history, journal, recovery
evidence, or session. Files outside this exact path are ordinary workspace
content, not lifecycle input.

The shared CLI Architecture defines the exact structured JSON result schema and
numeric exit mapping. This Interface uses those shared definitions without
duplicating implementation mechanics. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. The new CLI remains
non-shipping and this contract does not claim that implementation or proof.

## Purpose And Boundary

`update` reconciles a trusted existing managed Framework state in one exact
workspace with the current Framework payload embedded in the running CLI. It
uses one transparent baseline/current/intended comparison and one complete plan.

Normal update applies safe baseline-unchanged managed content and genuinely new
safe content. It preserves changed current expected content, missing current
expected content, and retired managed content unless an explicit flag supplies
the authority for that exact class.

`--force` widens only replacement or restoration of changed or missing current
expected Framework content. `--prune` widens only deletion of eligible retired
managed content. `--force --prune` composes those two independent boundaries.
Neither flag adopts content, changes ownership, repairs markers, or weakens
verification and recovery.

`update` requires trusted existing Framework lifecycle state. It is not a fresh
install, a package manager, a version-range solver, a restore alias, or a
generic filesystem update.

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
--json
--view=compact|expanded
--verbose
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

## Required Managed State

Update starts only when a trusted Framework lifecycle section exists for the
exact selected workspace and recognized Framework target or managed-region
identities. The section must have supported envelope and section versions and
fingerprint policy, intact internal consistency, and complete verifiable
coverage. A path, matching bytes, matching fingerprint, or `--force` cannot
promote missing, malformed, unsupported, unverifiable, or ambiguous evidence to
trusted management.

Safe unavailable lifecycle coverage is `incomplete`; unsafe or ambiguous
identity, ownership, containment, marker, or cross-section facts are `blocked`.
No update flag changes that trust boundary. A safely absent state belongs to
`install`, not `update`.

The physical lifecycle document is `.agents/open-forge.lifecycle.json`, schema v1,
with isolated `framework` and `extensions` sections in a common envelope. Update
may publish Framework facts only after complete verification and must preserve
the unrelated `extensions` section and common-envelope meaning semantically.
When selected lifecycle meaning changes, it emits one deterministic canonical
UTF-8 whole-document representation; lifecycle property order, whitespace, and
line endings are not preserved. A semantic no-op writes nothing. Prior bytes for
every existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`)
are retained only in the verified external recovery bundle described in
[Recovery Boundary](#recovery-boundary); the CLI does not inspect or report
repository state or claim history evidence. The document is not a plan, history,
journal, recovery record, or session. An absent document or section is not, by
itself, proof of unmanaged state. Unsupported or
ambiguous schema facts remain
`incomplete` or `blocked` under the command's existing safety rules.

Workspace mutation uses the visible `.agents/open-forge.lock` path under the
accepted CLI Architecture. File existence is not lock ownership: the operation
must hold the actual OS file lock. A crash releases that OS lock. An unlocked
file is reusable and may be manually removed only when no process is active.
This lock is concurrency safety, not lifecycle authority or history. An active
lock held by another process blocks mutation.

## Baseline, Current, And Intended Comparison

For every recognized Framework file and supported root/provider region, update
compares:

- the trusted baseline semantic fingerprint;
- the current semantic fingerprint and freshly captured exact bytes; and
- the intended current embedded source and its semantic fingerprint.

The comparison distinguishes:

| State                                                                  | Normal update                                             | `--force`                                                           | `--prune`                                                           |
| ---------------------------------------------------------------------- | --------------------------------------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Current equals baseline and source has no semantic change              | Verified no-op                                            | Same                                                                | Same                                                                |
| Baseline-unchanged current target and genuinely new safe source target | Add or update safe expected content                       | Same                                                                | Same                                                                |
| Current expected file or managed region changed from baseline          | Preserve and report                                       | Replace only that exact current expected footprint after all checks | No effect                                                           |
| Current expected file or managed region is missing                     | Preserve and report; do not restore implicit user removal | Restore only that exact current expected footprint after all checks | No effect                                                           |
| Baseline-managed path is retired from current source and is present    | Preserve and report                                       | Preserve; force is not deletion authority                           | Delete only when eligible retired managed content passes every gate |
| Retired path is already absent                                         | Retain the lifecycle observation; do not recreate it      | Same                                                                | No deletion effect                                                  |

When current and baseline semantic fingerprints are equal but current exact bytes
differ only by parser-proven formatting trivia, the operation treats the state
as equal and preserves the current formatting. A source semantic change may
still be applied to baseline-unchanged current content under normal update.
Update never runs a formatter to create lifecycle identity.

For supported Markdown and frontmatter kinds, `open-forge-markdown-v1` is the
conservative semantic fingerprint policy. Unsupported, binary, and unparseable
kinds use exact bytes and fail closed when equivalence cannot be proven.

## Flags

| Flag                | Role                                             | Value                          | Omission                                             | Repetition and composition                                                                            |
| ------------------- | ------------------------------------------------ | ------------------------------ | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `--force`           | Current expected-footprint replacement authority | Boolean                        | Preserve changed or missing current expected content | Repeats idempotently. It never grants prune or another bypass.                                        |
| `--prune`           | Retired-content deletion authority               | Boolean                        | Preserve retired content                             | Repeats idempotently. It never replaces or restores current expected content.                         |
| `--automatic`       | Guided-input policy                              | Boolean                        | Human mode may review finite divergence choices      | Repeats idempotently. It selects only deterministic safe effects already authorized by update.        |
| `--dry-run`         | Preview write policy                             | Boolean                        | Apply after the same preflight                       | Repeats idempotently. It writes nothing and cannot prove application or recovery.                     |
| Shared global flags | Workspace and presentation                       | Defined by the shared contract | Shared defaults                                      | Shared rules apply.                                                                                   |

### `--force`

Force may overwrite only a changed current expected file or valid managed region
whose trusted baseline identifies that exact current Framework footprint, or
restore only a missing current expected footprint. It never deletes retired
content, repairs markers, adopts an unowned path, overrides a competing owner,
or bypasses route, source, physical identity, containment, expected-state,
bundle, verification, or recovery checks.

### `--prune`

Prune may delete only an eligible retired managed path when the trusted baseline
identifies the exact previously managed path, current source proves retirement,
current semantic state and physical identity are safe, no owner, manager, route,
or dependency blocks, and bundle and recovery checks pass. It is not arbitrary
cleanup. It never deletes unknown, unowned, shared, unsafe, or current expected
content and never restores or overwrites a path.

`--force --prune` composes the two exact boundaries in one complete plan. Neither
flag changes the other flag's class of effect.

### `--automatic`

Automatic mode suppresses interaction and selects only explicit update subjects
(the fixed Framework subject) plus deterministic safe defaults. It may apply
baseline-unchanged and genuinely new safe effects. It preserves changed,
missing, and retired divergence and reports the same status as normal mode. It
never silently activates force or prune. It does not select a recommendation,
adopt, delete, restore, or bypass safety.

## Generated Navigation

Update forms the hypothetical post-update authored workspace, including only
permitted payload, bounded-region, force, and prune effects. It preserves
user-added routes and intentionally absent defaults. It projects affected
generated `Entries` bodies from that intended authored topology and metadata
using the current Index contracts.

Generated interiors are derived navigation, not Framework authored payload
identity and not a package-owned file. Update changes only valid bounded
generated interiors and preserves markers and outside bytes. Missing, duplicate,
reversed, nested, misplaced, or ambiguous generated boundaries block the whole
plan. `--force` and `--prune` do not repair them, and update does not invoke a
hidden `index` command.

## Ownership And Deletion Boundary

Framework, Extension, user, and external-manager ownership remain distinct. A
matching semantic fingerprint never proves ownership. Prune supplies deletion
authority only for the exact trusted retired Framework path and does not infer
ownership from its presence. A shared or competing owner, route dependency,
unknown path, or unsafe host blocks deletion.

There is no root Framework remove or uninstall operation in this accepted
surface. Manual guidance must never recommend deleting `.agents` wholesale. A
future Framework uninstall requires a separate product and safety review.

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

After final verification, whole-command success deletes only the positively
recognized bundle it created. If deletion fails, effects remain successful and
the result is `attention` with the exact residual path and cleanup guidance.
Handled failure or cancellation stops new effects and reports the actual
residual draft or final path; a valid final remains when failure occurs after
preparation. A closed final ZIP may remain after abrupt process termination,
without an executable crash or power-loss guarantee. The CLI never restores a
target automatically, derives current target state from recovery provenance, or
saves a journal, progress receipt, history, or replayable plan. Cleanup owns
exact named final and draft deletion under its separate lease-bound contract.
A workspace move is outside
the automatic guarantee; Doctor/Cleanup may report orphaned original-root
bundles but never auto-binds or restores them.

## Output, Streams, And Status

Human output leads with exact workspace, source, normal/force/prune/automatic and
apply/dry-run mode, baseline/current/intended facts, safe and preserved effects,
generated projection, lifecycle publication or preservation, bundle and
recovery facts, status, and at most one required `Next:` action. Compact output is a
density projection, not a weaker plan. JSON emits one complete typed result from
the same operation result for every status.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Bounded diagnostics use stderr. Human output may say `requires
attention`; structured output retains `attention`.

| Result        | Meaning for `update`                                                                                                                                                                                                                                 |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A safe full apply or dry-run, eligible force/prune application, or verified no-op has complete coverage and no preserved finite divergence.                                                                                                          |
| `attention`   | Complete safe coverage exists, but changed, missing, retired, or equivalent finite divergence remains because the selected authority did not resolve it. Planned changes, force/prune presence, and format-only observations do not create it alone. |
| `incomplete`  | Safe required lifecycle, source, parser, or recovery coverage is unavailable. No write occurs.                                                                                                                                                       |
| `invalid`     | Syntax, operand, flag value, repetition, or terminal-mode input is invalid.                                                                                                                                                                          |
| `blocked`     | Unsafe, ambiguous, untrusted, colliding, unauthorized, retained-dependent, or route-unsafe facts prevent one safe plan.                                                                                                                          |
| `failed`      | Application, verification, lifecycle publication, or bundle handling fails unexpectedly or leaves an unsafe residual after effects begin.                                                                                                           |
| `interrupted` | The caller interrupts before completion and no unexpected application or verification failure changes the result.                                                                                                                                 |

Ordinary precedence is `blocked` > `incomplete` > `attention` > `complete`.
Invalid input stops before operation resolution. Failed and interrupted retain
their event meaning. JSON preserves one result on stdout for every status.

## Errors And Next Actions

Every error names `update`, the exact workspace and affected target or lifecycle
fact, the cause, and at most one useful next action.

- Operands, a source argument, `--all`, an unknown flag, malformed value,
  invalid repetition, or terminal-mode conflict is `invalid`.
- Missing or safely unavailable trusted lifecycle state is `incomplete`.
- Malformed or ambiguous lifecycle, ownership, route, marker, containment,
  expected-state, bundle, or recovery facts are `blocked`.
- A missing embedded source fact is `incomplete`; force and prune cannot guess.

## Examples

Reconcile safe current Framework changes:

```text
open-forge update
```

Preview replacement of changed or missing current expected content:

```text
open-forge update --force --dry-run --json
```

Preview retired-content deletion without granting replacement authority:

```text
open-forge update --prune --automatic --dry-run
```

Apply both exact boundaries in one request:

```text
open-forge update --force --prune
```

Normal update preserves unresolved divergence and reports `attention`. Repeating
it produces verified no-effect facts while that divergence remains. `--automatic`
does not turn that report into force or prune.

## Non-Goals And Architecture Boundary

Update does not:

- establish an absent Framework state, adopt an untracked installation, or turn
  `install --force` into a managed update;
- accept a source operand, package version, range, network source, registry,
  cache, generic apply, saved plan, `--yes`, or Framework remove/uninstall;
- replace arbitrary user, Extension, shared, unknown, retired-only, or
  route-unsafe content;
- repair lifecycle or generated markers, rewrite overwrite companions, or run a
  formatter;
- mutate the `extensions` section, an external source, or `.temp/`; or
- create runtime meaning, a session, operation history, journal, or formatter
  state.

If a supported formatter configuration is detected, update may give conservative
advice only. It does not execute a formatter or persist formatter state.

The accepted CLI Architecture owns exact document serialization, semantic
canonicalization mechanics, parser libraries, filesystem identity, containment,
locks, concurrency, recovery-bundle and temporary artifact details, diagnostics, Native
AOT architecture, package implementation, and tests. Gate 5 must prove those
boundaries, the shared result schema and exit mapping, and the embedded
deterministic inventory/hash evidence.

## Public Conformance

Future evidence must cover:

- exact root syntax, fixed embedded source, no operands, terminal modes, and
  shared flags;
- trusted, absent, unavailable, malformed, unsupported, and ambiguous lifecycle
  facts;
- baseline/current/intended semantic comparison for unchanged, new, changed,
  missing, retired, and format-only states;
- normal preservation and `attention`, force replacement/restoration, prune
  deletion, and force/prune composition without cross-boundary authority;
- automatic mode preserving divergence and never selecting force or prune;
- exact authored-topology generated projection, bounded markers, ownership,
  route, containment, bundle, expected-state, verification, and recovery
  behavior;
- exact schema-v1 `framework`-section semantics, unrelated-section and
  common-envelope semantic preservation, deterministic canonical whole-document
  serialization on selected change, no write on semantic no-op, and
  absence-not-unmanaged handling;
- dry-run/application parity and no dry-run effects;
- seven statuses, streams, one-result JSON, bounded diagnostics, next-action
  limits, and verified no-op repetition; and
- no formatter execution or persisted formatter state; and
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys,
  without claiming that proof here.
