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

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact structured JSON result schema and numeric exit mapping. This Interface uses those shared definitions without
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

Update consumes the same neutral BCL embedded-resource inventory as Install and
Framework-aware Route Init. Runtime never reads the development checkout.

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

Every Framework target record has one required nullable `sourceAssetPath`.
Payload files and managed root/provider blocks record the normalized canonical
embedded asset-relative path that produced them; derived generated-region
targets record `null`. A trusted historical non-null path remains structurally
valid when the current inventory no longer contains it. That absence is the
retirement comparison fact. Every newly published non-null value must exist in
the exact current inventory. User-owned inserted scope entrypoints are not
Framework targets. Schema v1 adds no lifecycle-instance collection or migration
engine.

Workspace mutation uses the persistent external zero-byte path under the
application-owned `LocalApplicationData/OpenForge/locks/v1` catalogue defined by
the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md). File existence is not lock ownership: the
operation must hold the actual OS file lock. A crash releases that OS lock. An unlocked
file is reusable and may be manually removed only when no process is active.
This lock is concurrency safety, not lifecycle authority or history. An active
lock held by another process blocks mutation.

## Baseline, Current, And Intended Comparison

For every recognized Framework file and supported root/provider region, update
compares:

- the trusted baseline semantic fingerprint;
- the current semantic fingerprint and freshly captured exact bytes; and
- the recorded source provenance and intended current embedded source or derived
  generated relationship, plus its semantic fingerprint when source content is
  present.

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

| Flag                | Role                                             | Value                          | Omission                                             | Repetition and composition                                                                     |
| ------------------- | ------------------------------------------------ | ------------------------------ | ---------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| `--force`           | Current expected-footprint replacement authority | Boolean                        | Preserve changed or missing current expected content | Repeats idempotently. It never grants prune or another bypass.                                 |
| `--prune`           | Retired-content deletion authority               | Boolean                        | Preserve retired content                             | Repeats idempotently. It never replaces or restores current expected content.                  |
| `--automatic`       | Guided-input policy                              | Boolean                        | Human mode may review finite divergence choices      | Repeats idempotently. It selects only deterministic safe effects already authorized by update. |
| `--dry-run`         | Preview write policy                             | Boolean                        | Apply after the same preflight                       | Repeats idempotently. It writes nothing and cannot prove application or recovery.              |
| Shared global flags | Workspace and presentation                       | Defined by the shared contract | Shared defaults                                      | Shared rules apply.                                                                            |

### `--force`

Force may overwrite only a changed current expected file or valid managed region
whose trusted baseline identifies that exact current Framework footprint, or
restore only a missing current expected footprint. It never deletes retired
content, repairs markers, adopts an unowned path, overrides a competing owner,
or bypasses route, source, physical identity, containment, expected-state,
bundle, verification, or recovery checks.

### `--prune`

Prune may delete only an eligible retired managed path when the trusted baseline
identifies the exact previously managed path and recorded provenance. For a
payload file or managed block, its non-null `sourceAssetPath` and the current
inventory prove that exact asset is retired. For a derived generated-region
target, `sourceAssetPath` is `null` and the current intended topology proves that
the recorded relationship no longer produces the region. In both cases,
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

### Human Confirmation

After the complete plan and preflight succeed, a prompt-capable human apply
that would write asks exactly once for confirmation before acquiring the
workspace lease or beginning any recovery or workspace effect. Trimmed `y` and
`yes` answers are accepted case-insensitively. Refusal, end of input, or caller
cancellation returns `interrupted` and writes nothing.

A verified no-op, an effect-free `attention` result, dry-run, `--automatic`,
JSON, and redirected or otherwise non-prompt-capable execution never prompt or
consume input. A non-prompt-capable human apply that would write is `invalid`
unless `--automatic` is explicit. Its one next action reruns the same local
Update authority with `--automatic`. Automatic never supplies force or prune.
Decorative prompt wording is not contract meaning.

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
recognized bundle it created. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one.
When `Failed`/positively observed `Retained` recovery attention coexists with
finite unresolved divergence, cleanup guidance owns the single next action;
divergence facts remain visible evidence. Before post-verification deletion begins, handled application,
verification, or cancellation outcomes stop new effects and report the actual
residual draft or final path; a valid final remains when preparation completed.
A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. The CLI never restores a
target automatically, derives current target state from recovery provenance, or
saves a journal, progress receipt, history, or replayable plan. Cleanup owns
exact named final and draft deletion under its separate lease-bound contract.
A workspace move is outside
the automatic guarantee; Doctor/Cleanup may report orphaned original-root
bundles but never auto-binds or restores them.

## Output, Streams, And Status

Human output starts with the outcome, semantic status, exact workspace and
selection method, then source identity and normal/force/prune/automatic and
apply/dry-run mode. Both views show every finding, effect and comparison path,
current/intended/retirement state, generated navigation, lifecycle trust and
publication, verification, recovery data and every protected path. A comparison
appears beside its effect path once; paths without effects remain visible.
Findings lead with uppercase status and cause, with the stable code afterward.
Expanded adds embedded-source version and inventory fingerprint, source-asset
facts and baseline/current/intended comparison fingerprints. Compact is a density
projection and retains partial effects and uncertainty. The actual `Next:` command
appears once, with its reason in expanded output. JSON remains complete and
unchanged. A failed or interrupted heading does not claim no writes occurred.
The three preservation finding kinds explain local changes kept, missing paths
left absent, and retired paths not removed. Their original producer causes stay
unchanged in JSON; human wording does not change force/prune requirements.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Bounded diagnostics use stderr. Human output may say `requires
attention`; structured output retains `attention`.

### Exact Schema-v1 Command-Local Result

The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
schema-v1 envelope wraps one non-null Update result. The envelope remains
exactly `{ schemaVersion, command, status, workspace, result, next }`, with
`schemaVersion` equal to integer `1` and `command` equal to `update`. The
command-local `result` uses camel-case properties in exactly this order, and
every property is present for every semantic status:

1. `mode`: `apply` or `dry-run`;
2. `force`: Boolean;
3. `prune`: Boolean;
4. `automatic`: Boolean;
5. `source`;
6. `comparisons`;
7. `generatedNavigation`;
8. `effects`;
9. `lifecycle`;
10. `recovery`;
11. `verification`;
12. `findings`.

`source` is `null` until a valid embedded Framework payload is available. A
non-null source is one atomic object whose members are `id`, `version`,
`inventoryFingerprint`, and `assetCount`, in that order. `id` is `framework`;
`version` is a nullable descriptive string; `inventoryFingerprint` is a
lowercase 64-hex SHA-256 value; and `assetCount` is a nonnegative integer.

`comparisons` is a non-null ordered array. Every member has `path`, `kind`,
`region`, `sourceAssetPath`, `fingerprintKind`, `baselineFingerprint`,
`currentFingerprint`, `intendedFingerprint`, `currentState`, `intendedState`,
and `retirementEligibility`, in that order.

- `path` is a canonical workspace-relative `/`-separated path without a leading
  slash.
- `kind` is `file`, `managed-region`, or `generated-region`.
- `region` is `null` only for a file and otherwise carries the exact persisted
  region identity.
- `sourceAssetPath` is a canonical embedded-asset-relative `/`-separated path.
  It is `null` only for a derived generated region.
- `fingerprintKind` is `open-forge-markdown-v1` or `exact-bytes`. A generated
  region always uses `exact-bytes`.
- The three fingerprint members are nullable lowercase 64-hex SHA-256 values.
  `baselineFingerprint` is `null` only for a genuinely new current-source
  target, `currentFingerprint` is `null` for a missing target, and
  `intendedFingerprint` is `null` for a retired, unavailable, or blocked target.
- `currentState` is `missing`, `baseline-equivalent`, `format-only`, `changed`,
  `unavailable`, or `blocked`.
- `intendedState` is `same`, `changed`, `new`, `retired`, `unavailable`, or
  `blocked`.
- `retirementEligibility` is `not-applicable`, `eligible`, `ineligible`,
  `unavailable`, or `blocked`.

Comparisons order by path, kind, and then the empty string for a null region or
the exact region, all using ordinal comparison.

`generatedNavigation` is `null` until projection is attempted. Otherwise it is
one object whose members are `coverage` and `regions`, in that order. Coverage
is `complete`, `incomplete`, or `blocked`. `regions` is a non-null path-ordinal
array whose members are `path` and `state`, in that order. The path uses the
workspace-relative rule above, and state is `unchanged`, `changed`, `new`,
`retired`, `unavailable`, or `blocked`.

`effects` is a non-null array ordered by canonical physical path. Every member
has `path`, `action`, `changes`, `outcome`, and `residual`, in that order. Path
uses the workspace-relative rule above. Physical action is `create`, `replace`,
or `delete` and describes the one final `FileChangeApplier` effect for that
path. Outcome is `planned`, `not-started`, `verified`, `verification-failed`, or
`completion-unknown`. Residual is `none`, `retained`, or `unknown`; it is never
a Boolean.

`changes` is a non-empty array of logical selected changes coalesced into that
physical effect. Every change has `kind`, `action`, `region`, and
`sourceAssetPath`, in that order. Kind uses the comparison values. Logical
action is `create`, `replace`, `restore`, or `delete`. Region and source
provenance use the comparison nullability and canonical-path rules. File and
managed-region changes order before generated-region changes; within those
classes they order by kind, the empty string for a null region or the exact
region, and the empty string for a null source path or the exact source path,
all ordinal. A physical effect rejects duplicate logical identities. Every
logical change maps to exactly one comparison. A comparison may remain a no-op
or preserved fact and therefore have no selected change.

Each physical path has at most one effect, one application receipt, and one
recovery entry even when authored and generated changes share that path.
Lifecycle publication is separate and is not duplicated in this array.

`lifecycle` is one object whose members are `trust`, `coverage`, `action`, and
`outcome`, in that order. Trust is `not-requested`, `trusted`, `unavailable`, or
`blocked`. Coverage is `not-requested`, `complete`, `incomplete`, or `blocked`.
Action is `none`, `preserve`, or `publish`. Outcome is `not-requested`,
`planned`, `already-current`, `not-started`, `verified`,
`verification-failed`, or `completion-unknown`.

`recovery` is one object whose members are `state`, `protectedPaths`, and
`residualPath`, in that order. State is `not-required`, `not-created`,
`removed`, `retained`, or `unknown`. `protectedPaths` is a non-null array of
canonical workspace-relative paths, ordered like existing-target effects with
the lifecycle document last when it is replaced. `residualPath` is the exact
absolute external recovery path only when that residual identity is known and
is otherwise `null`.

`verification` is `not-requested`, `verified`, `failed`, or `unknown`.
`findings` is a non-null ordered array whose members are `code`, `target`, and
`cause`, in that order. Target is `null` for a command-wide fact and otherwise
is a canonical path or exact named identity. Cause is exact and non-empty.
Findings order first by the declaration sequence below, then with a null target
before every non-null target, then by non-null target and cause using ordinal
comparison. Arrays are empty rather than null.

Finding `code` uses exactly this finite vocabulary and status mapping, in its
primary ordering sequence:

| Code                                  | Status        | Meaning                                                                                |
| ------------------------------------- | ------------- | -------------------------------------------------------------------------------------- |
| `update.invalid-input`                | `invalid`     | Command syntax or normalized input is invalid.                                         |
| `update.confirmation-required`        | `invalid`     | A non-prompt-capable human write requires explicit automatic mode.                     |
| `update.workspace-unavailable`        | `blocked`     | The exact workspace cannot be selected as a safe Update subject.                       |
| `update.workspace-unsafe`             | `blocked`     | Workspace identity, containment, or lock acquisition is unsafe.                        |
| `update.payload-unavailable`          | `incomplete`  | The embedded Framework payload cannot be read completely.                              |
| `update.payload-invalid`              | `blocked`     | Embedded payload identity or content is structurally invalid.                          |
| `update.lifecycle-missing`            | `incomplete`  | Required trusted Framework lifecycle state is safely absent.                           |
| `update.lifecycle-unavailable`        | `incomplete`  | Required lifecycle facts cannot be read completely.                                    |
| `update.lifecycle-blocked`            | `blocked`     | Lifecycle facts are invalid, untrusted, ambiguous, or conflicting.                     |
| `update.ownership-conflict`           | `blocked`     | Another owner or lifecycle section conflicts with the selected plan.                   |
| `update.target-unavailable`           | `incomplete`  | A required target fact cannot be read completely.                                      |
| `update.target-unsafe`                | `blocked`     | A target cannot be resolved, revalidated, or mutated safely.                           |
| `update.source-provenance-invalid`    | `blocked`     | Recorded source provenance is structurally invalid or inconsistent.                    |
| `update.fingerprint-unsupported`      | `blocked`     | Safe equivalence cannot be established under a supported fingerprint policy.           |
| `update.managed-divergence`           | `attention`   | Current expected managed content differs from its trusted baseline.                    |
| `update.managed-target-missing`       | `attention`   | Normal Update preserves a trusted missing current expected target.                     |
| `update.retired-content-preserved`    | `attention`   | Trusted retired content remains because prune authority was not selected.              |
| `update.retirement-ineligible`        | `blocked`     | A prune request selected retired content that cannot be deleted safely.                |
| `update.projection-unavailable`       | `incomplete`  | Intended topology or generated projection cannot be formed completely.                 |
| `update.generated-region-unsafe`      | `blocked`     | A required generated-region boundary is missing, malformed, or ambiguous.              |
| `update.plan-blocked`                 | `blocked`     | One selected fact prevents the complete Update plan.                                   |
| `update.recovery-conflict`            | `blocked`     | A recognized recovery candidate or destination conflicts with this operation.          |
| `update.recovery-unavailable`         | `incomplete`  | Required external recovery storage or evidence is unavailable before effects.          |
| `update.recovery-artifact-retained`   | `attention`   | Verified effects succeeded but a positively retained recovery artifact remains.        |
| `update.write-failed`                 | `failed`      | A planned target effect failed or could not be verified.                               |
| `update.verification-failed`          | `failed`      | Whole-target or whole-operation verification failed.                                   |
| `update.lifecycle-publication-failed` | `failed`      | Framework lifecycle publication failed or could not be verified.                       |
| `update.recovery-failed`              | `failed`      | Recovery preparation or cleanup has unsafe or unknown completion.                      |
| `update.operation-failed`             | `failed`      | Another unexpected Update operation failure occurred.                                  |
| `update.interrupted`                  | `interrupted` | Caller cancellation, refusal, or end of input stopped the operation without a failure. |

The shared envelope owns command, aggregate status, workspace, and next action;
none is duplicated inside this result. A breaking change to these required
members, their order, JSON types, nullability, or finite values is a
command-local schema-v1 compatibility change.

| Result        | Meaning for `update`                                                                                                                                                                                                                                                                                                                                                                     |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A safe full apply or dry-run, eligible force/prune application, or verified no-op has complete coverage and no preserved finite divergence.                                                                                                                                                                                                                                              |
| `attention`   | Complete safe coverage leaves finite unresolved divergence, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`. Planned changes, force/prune presence, and format-only observations do not create it alone. `Failed`/`Retained` recovery keeps target effects successful and reports the exact residual path with cleanup guidance. |
| `incomplete`  | Safe required lifecycle, source, parser, or recovery coverage is unavailable. No write occurs.                                                                                                                                                                                                                                                                                           |
| `invalid`     | Syntax, operand, flag value, repetition, or terminal-mode input is invalid.                                                                                                                                                                                                                                                                                                              |
| `blocked`     | Unsafe, ambiguous, untrusted, colliding, unauthorized, retained-dependent, or route-unsafe facts prevent one safe plan.                                                                                                                                                                                                                                                                  |
| `failed`      | Application, verification, or lifecycle publication fails unexpectedly, post-verification recovery deletion returns `Failed`/`Unknown`, or another unsafe residual remains after effects begin.                                                                                                                                                                                          |
| `interrupted` | The caller interrupts before completion and no unexpected application or verification failure changes the result.                                                                                                                                                                                                                                                                        |

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

The single structured and human next action uses these exact coordinates:

- `complete` has no next action.
- Invalid input uses command `open-forge update --help` and reason `Correct the
named Update input, then rerun the request.`
- `update.confirmation-required` uses the same normalized local force and prune
  flags followed by `--automatic`; its reason is `Rerun the same Update request
with explicit automatic mode.`
- `blocked` uses command `open-forge doctor` and reason `Inspect the blocked
workspace, lifecycle, ownership, projection, or safety boundary before
rerunning Update.`
- `incomplete` uses command `open-forge doctor` and reason `Inspect the
unavailable workspace, source, lifecycle, projection, or recovery facts
before relying on Update.`
- Attention with retained recovery uses command `open-forge cleanup` and reason
  `Review and remove the reported recovery artifact after confirming the
verified Update result.`
- Other attention reconstructs `open-forge update` with local Boolean flags in
  fixed `--force --prune --automatic --dry-run` order. It preserves already
  selected authority, automatic, and dry-run, and adds only the exact force or
  prune authority named by the preserved divergence. Its reason is `Review the
preserved Update divergence, then rerun with the named explicit authority.`
- `failed` uses the same canonical local-flag reconstruction followed by
  `--verbose`; its reason is `Report the failure and retry the same Update
request with bounded diagnostics.`
- `interrupted` uses the same canonical local-flag reconstruction without
  `--verbose`; its reason is `Rerun the same Update request.`

Repeated local Boolean flags collapse to one. Next commands never copy
`--workspace`, `--json`, or `--view`; the result envelope retains selected
workspace identity without embedding a host path or shell-quoting policy in a
command string. `--verbose` appears only in failed guidance.

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

The [Lifecycle Provenance Technical
Design](../../technical-designs/lifecycle-provenance.md) owns exact document
serialization, and the [Mutation And Recovery Technical
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

Future evidence must cover:

- exact root syntax, fixed embedded source, no operands, terminal modes, and
  shared flags;
- trusted, absent, unavailable, malformed, unsupported, and ambiguous lifecycle
  facts;
- required nullable per-target `sourceAssetPath`, historical retired-asset
  recognition, current-inventory publication validation, derived-region `null`,
  and no lifecycle instance collection;
- baseline/current/intended semantic comparison for unchanged, new, changed,
  missing, retired, and format-only states;
- normal preservation and `attention`, force replacement/restoration, prune
  deletion, and force/prune composition without cross-boundary authority;
- automatic mode preserving divergence and never selecting force or prune;
- the exact one-confirmation matrix, no-write refusal, end-of-input, and
  cancellation, direct automatic rerun guidance for redirected human writes,
  and no prompt for no-op, effect-free attention, dry-run, automatic, JSON, or
  redirected modes;
- exact authored-topology generated projection, bounded markers, ownership,
  route, containment, bundle, expected-state, verification, and recovery
  behavior;
- one physical effect, application receipt, recovery entry, and protected path
  for multiple ordered logical authored and generated changes that share a
  physical file;
- exact schema-v1 `framework`-section semantics, unrelated-section and
  common-envelope semantic preservation, deterministic canonical whole-document
  serialization on selected change, no write on semantic no-op, and
  absence-not-unmanaged handling;
- dry-run/application parity and no dry-run effects;
- the exact ordered command-local schema-v1 result, 30 finding codes and their
  ordering, seven statuses, streams, one-result JSON, bounded diagnostics,
  next-action limits, and verified no-op repetition;
- no formatter execution or persisted formatter state; and
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys,
  without claiming that proof here.

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The compact result retains the complete command-owned result graph defined by
its structured schema, including every nullable value and ordered collection.
Its core already carries the facts needed to use the result. For mutation
commands this includes plans, exact previews, effects, permissions when
applicable, verification, findings and recovery. Rendering never asks a caller
to rerun a mutation to recover an omitted receipt.

No collection is truncated and no finding is filtered. Counts describe the
original operation. Both JSON views retain the same result facts.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
