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

## Purpose And Boundary

`extension update` reconciles trusted existing managed Extension IDs with the
current bytes from one explicitly selected source universe. It makes source and
intent explicit. A semantic version is descriptive package metadata; it does not
select a source, negotiate compatibility, or invoke a generic package-manager
operation.

Normal update applies baseline-unchanged managed content and genuinely new safe
files. It preserves changed current expected paths, missing current expected
paths, and retired managed paths. `--force` replaces changed or restores missing
current expected paths. `--prune` deletes eligible retired managed content.
`--force --prune` composes those exact boundaries and no others.

## Syntax

```text
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]
```

IDs are repeatable positional managed package subjects. `--all` explicitly
selects all currently managed IDs represented by the selected source and
dependency closure. Explicit IDs and `--all` conflict and are invalid. `--all`
is not a package or file glob and missing source coverage is not silently
skipped.

The shared flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`. Their shared
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

- trusted existing Extension lifecycle state for every selected managed ID and
  dependency fact;
- readable current source bytes for every selected identity and dependency;
- a trustworthy installed Framework anchor; and
- complete affected route-host, generated-navigation, ownership, and
  cross-section preservation facts.

Before a workspace effect, the implementation must hold the actual OS lock for
the persistent external zero-byte path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../../technical-designs/mutation-and-recovery.md). The lock file is persistent and reusable: write no metadata,
timestamp, or ownership record. Hold a
`FileShare.None` handle for the operation; file existence is not lock
ownership. A crash releases the OS lock, and another process holding it blocks
mutation. The lock is concurrency safety, not lifecycle authority, history, or
recovery evidence.

Missing safe source or lifecycle coverage is `incomplete`. Malformed, ambiguous,
colliding, unsafe, or untrusted mutation facts are `blocked`. Force and prune do
not promote untrusted state.

The only new-CLI lifecycle document is `.agents/open-forge.lifecycle.json`, schema
v1. It has a common envelope and isolated `framework` and `extensions` sections.
Update changes only `extensions` and preserves the unrelated `framework` section
and common-envelope meaning. When selected lifecycle meaning changes, the writer
emits one deterministic canonical UTF-8 whole-document representation; lifecycle
property order, whitespace, and line endings are not preserved. A semantic
no-op writes nothing. The document stores no plan, runtime history, journal,
recovery evidence, or session. An absent document or section is not, by itself,
proof of unmanaged state. Unsupported or ambiguous schema facts are
`incomplete` or `blocked` under the existing safety rules.

### Recovery boundary

Before the first target effect, application prepares and verifies exactly one
immutable ZIP recovery bundle for the complete operation when the plan contains
an existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`). The bundle
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

Every planned existing-target effect must match one verified bundle entry; Create and
no-op effects create no entry. All bundle preparation completes before the
first mutation. `FileChangeApplier` requires that matching preparation for each
existing-target effect and performs one final effect per target. Before
post-verification deletion begins, a handled application, verification,
publication, or cancellation outcome reports the actual residual draft or final
path; a valid final remains when preparation completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
The CLI never restores, rolls back, compensates for an effect, derives current
target state from recovery provenance, or stores a journal, progress receipt, or
history.

After final verification of whole-operation success, delete the bundle. If
the deletion result is `Deleted`/`Removed`, normal completion continues.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery attention
coexists with finite divergence, cleanup guidance owns the single next action;
divergence facts remain visible evidence. Explicit Cleanup
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

| Flag          | Role                                             | Effect                                                                                                                              |
| ------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------- |
| `--force`     | Current expected-footprint replacement authority | Overwrite changed current expected paths and restore missing current expected paths only.                                           |
| `--prune`     | Retired-content deletion authority               | Delete eligible retired managed paths only.                                                                                         |
| `--automatic` | Guided-input policy                              | Suppress wizard and apply only safe effects authorized by explicit IDs, `--all`, or permitted single-package manifest-ID inference. |
| `--dry-run`   | Preview policy                                   | Use the same plan and preflight, then write nothing.                                                                                |

All Boolean flags repeat idempotently. Force never implies prune. Prune never
restores or overwrites.

### Normal update

Normal mode applies baseline-unchanged current expected paths and genuinely new
safe paths. It preserves changed current expected paths, missing current expected
paths, retired content, shared or competing ownership, route-unsafe content,
and unknown content. Preserved finite divergence produces `attention` after
complete safe coverage; planned effects alone do not.

### `--force`

Force widens only current expected-footprint replacement: changed managed paths
may be overwritten and missing current expected paths may be restored. It never
deletes retired content, adopts an unowned path, overrides shared or competing
owners, repairs markers, bypasses containment, or weakens verification or
recovery.

### `--prune`

Prune widens only retired managed-content deletion. A path is eligible only when
trusted baseline identity names it, current source proves retirement, current
semantic and physical identity are safe, no other owner/manager/route dependency
blocks, and verification and recovery checks pass. Prune never deletes unknown,
unowned, shared, current expected, or unsafe content.

### `--automatic` and wizard

Argumentless human install/update leaves may open finite wizards. For update the
questions are managed IDs or `--all`, source when needed, and whether to supply
force or prune authority for visible divergence. Recommendations are facts, not
authority.

`--automatic` suppresses the wizard and uses explicit IDs or `--all` plus safe
deterministic defaults. It never selects force, prune, replacement, restoration,
deletion, adoption, ownership, or a fuzzy choice. JSON and other non-interactive
requests never prompt; missing semantic selection is `invalid`, and missing
authority for a requested effect is `blocked`.

## Dependencies, Ownership, And Generated Navigation

Resolve exact stable-ID dependencies offline, transitively, and dependency-first.
Reject unknown IDs, duplicates, duplicate declarations, invalid manifests,
cycles, unsafe package paths, incompatible intended content, and incomplete
closure before writes.

The `extensions` section records stable IDs, dependency facts,
target-relative paths, shared-owner sets, and semantic baseline fingerprints.
The schema-v1 document keeps `framework` and `extensions` logically isolated.
Update preserves unrelated section meaning semantically. A selected semantic
change emits one deterministic canonical UTF-8 whole-document representation;
lifecycle property order, whitespace, and line endings are not preserved. A
semantic no-op writes nothing. For an existing-target effect, exact prior bytes
are captured in the verified operation recovery bundle;
the lifecycle document itself stores no recovery evidence. Files outside the
exact lifecycle document are not lifecycle inputs.

Two explicit owners may share a physical path only with equal supported
canonical semantic fingerprints and compatible path, route, and metadata facts.
Formatting-only source-byte differences are compatible. Semantic equality never
adopts an unowned path. Retained dependents block changes that would strand
them; orphaned dependencies remain recorded and installed.

The operation projects affected generated `Entries` from intended authored
topology and metadata using current Index behavior. Generated interiors are
derived navigation, not package-owned authored bytes. A malformed boundary
blocks and is never repaired by force or prune. The lifecycle document, Framework,
overwrite, recovery, and other-manager paths are not package targets.

## Semantic Identity And Formatter Boundary

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware fingerprints that
preserve Unicode, semantic text, headings, tags, links, destinations, marker
meaning, inline and code-block content, and significant whitespace. Normalize
only line endings and parser-proven formatting trivia. Unsupported, binary, and
unparseable kinds use exact-byte identity and fail closed.

Persist semantic baselines, not exact-byte baseline digests. Fresh exact bytes
remain necessary for diff, revalidation, write/deletion, verification, and
recovery. Equal semantic identity with formatting-only byte differences is not
divergence and does not require persisted formatter state.

The accepted conservative formatter direction allows detection and advice only.
The CLI does not execute a formatter, select one, change files for formatting, or
persist formatter state.

## Output And Semantic Results

Human output leads with workspace/source, selected IDs and closure, trust,
normal/force/prune/automatic and apply/dry-run mode, baseline/current/intended
counts, safe/planned/applied/preserved/restored/overwritten/deleted/shared
effects, generated projection, lifecycle publication, recovery-bundle facts,
status, and at most one next action. JSON emits one complete typed result from
the same result for every status.

| Result        | Meaning for `extension update`                                                                                                                                                                                                                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The selected update, force, prune, composition, or dry-run has complete coverage and no unresolved finite divergence; a verified no-op is complete.                                                                                                                                                                        |
| `attention`   | Complete safe coverage preserves finite divergence not covered by the selected authority, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`. `Failed`/`Retained` recovery keeps target effects successful and reports the exact residual path with cleanup guidance. |
| `incomplete`  | Safe source, lifecycle, Framework-anchor, dependency, parser, route, or recovery-bundle coverage is unavailable. No write occurs.                                                                                                                                                                                          |
| `invalid`     | Selection, source, flags, operands, repetition, or terminal-mode input is invalid.                                                                                                                                                                                                                                         |
| `blocked`     | Unsafe, ambiguous, untrusted, colliding, retained-dependent, route-unsafe, ownership, containment, or recovery-bundle facts prevent one complete plan.                                                                                                                                                                     |
| `failed`      | Application, lifecycle publication, or verification fails unexpectedly after effects begin, or post-verification recovery deletion returns `Failed`/`Unknown`.                                                                                                                                                             |
| `interrupted` | The caller interrupts before completion and no unexpected application or verification failure remains.                                                                                                                                                                                                                     |

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr. Human attention may say `requires attention`; JSON retains `attention`.

## Errors And Examples

Every error names `extension update`, the workspace/source/ID/path when known,
the cause, and at most one useful next action. Missing source bytes for a
selected managed ID are `incomplete`, not a no-op and not a source fallback.

Update one managed ID from the embedded catalogue:

```text
open-forge extension update development-toolkit
```

Preview all represented managed IDs from an exact catalogue:

```text
open-forge extension update --all --source D:/packages/open-forge --dry-run --json
```

Replace changed/current missing content but do not prune retired paths:

```text
open-forge extension update development-toolkit --force --automatic
```

Prune retired content only:

```text
open-forge extension update development-toolkit --prune --dry-run
```

Compose both exact authority boundaries:

```text
open-forge extension update development-toolkit --force --prune
```

## Non-Goals And Public Conformance

Update does not install an absent ID, select a source by semver, use a network,
registry, cache, glob, or fallback, adopt unmanaged paths, delete current
expected or unknown content, remove a package source, repair markers, mutate
Framework-owned files, run a formatter, create a saved plan/journal, or create a
Framework uninstall operation.

Conformance must cover exact source universe, IDs/`--all`, dependency closure,
trusted lifecycle and Framework-anchor gates, source-unavailable behavior,
normal/force/prune/automatic semantics, shared ownership, retired/final path
boundaries, semantic fingerprints, generated navigation, lifecycle-section
preservation, complete planning, recovery-bundle behavior, dry-run parity, statuses,
streams, JSON, deterministic no-op repetition, and no package-source mutation.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the exact JSON result schema and exit mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, OS locking, isolated tests, and package
journeys.
