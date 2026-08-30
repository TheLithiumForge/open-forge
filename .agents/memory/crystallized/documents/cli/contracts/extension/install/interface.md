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

The only new-CLI lifecycle document is `.agents/open-forge.lifecycle.json`, schema
v1. It has a common envelope and isolated `framework` and `extensions` sections.
Install changes only `extensions` and preserves the unrelated `framework` section
and common-envelope meaning. When selected lifecycle meaning changes, the writer
emits one deterministic canonical UTF-8 whole-document representation; lifecycle
property order, whitespace, and line endings are not preserved. A semantic
no-op writes nothing. Prior bytes for every existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) are retained only in the verified external
recovery bundle described by the shared Architecture; the CLI does not inspect
or report repository state or claim history evidence.
The document stores no plan, runtime history, journal, recovery evidence, or session.
Files outside this exact path are ordinary workspace content, not lifecycle input.

The shared CLI Architecture defines the exact package serialization, structured
JSON result schema, and numeric exit mapping. This Interface uses those shared
definitions without duplicating implementation mechanics. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.

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
--json
--view=compact|expanded
--verbose
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
complete route-host facts before it can form a mutation plan. Route-host facts
include affected authored hosts, generated-navigation boundaries, ownership
relationships, and cross-section preservation facts. A missing or unsafe anchor
or route-host boundary is `incomplete` or `blocked`, and no managed mutation
occurs.

Before a workspace effect, the implementation must hold the persistent reusable
zero-byte external lock under `LocalApplicationData/OpenForge/locks/v1` defined
by the accepted CLI Architecture. It holds one read/write `FileShare.None`
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

Managed divergence remains blocked and directs to `extension update`, even with
`--force`.

### Automatic and wizard behavior

Argumentless human `extension install` may open a finite wizard for package IDs
or `--all`, source when needed, dependency closure, and any eligible initial
force choice. Wizard answers, operands, and flags populate one typed request.
Recommendations do not supply force or selection authority.

JSON and other non-interactive modes never prompt. Missing package selection in a
multi-package source is `invalid`; unresolved safe authority or a required
initial force choice is `blocked`. `--automatic` suppresses the wizard and
uses only explicit IDs, explicit `--all`, or the permitted single-package
manifest-ID inference plus deterministic safe effects. That inference is a
documented exact-source default, not an automatic choice among packages.
Automatic mode never broadens selection, adds initial force, replacement,
adoption, ownership, or safety bypass.

## Lifecycle Identity, Ownership, And Generated Navigation

The physical lifecycle document is `.agents/open-forge.lifecycle.json`, schema v1,
with separate `framework` and `extensions` sections. Install writes only the
`extensions` section after complete verification and preserves the common
envelope and unrelated `framework` section meaning semantically. A selected
semantic change emits one deterministic canonical UTF-8 whole-document
representation, so lifecycle property order, whitespace, and line endings are
not preserved. A semantic no-op writes nothing. Prior bytes for existing replaced
or deleted targets are retained only in the verified external recovery bundle
described below; the CLI does not inspect or report repository state or claim
history evidence. An absent document or section is not, by itself,
proof of unmanaged state. Unsupported or ambiguous schema facts are `incomplete`
or `blocked` under the existing safety rules.

Managed package identity is stable ID plus exact source, dependency, target-
relative path, owner, and semantic baseline facts. Manual copying, idless
packages, direct overlays, and externally managed native Skills remain unmanaged;
matching path, bytes, or fingerprint never transfers ownership.

Dependencies are ordered before dependents. Shared-owner sets are explicit. Two
owners may share a supported path only with equal syntax-aware semantic identity
and compatible path, route, and metadata facts. Formatting-only source-byte
differences do not conflict. Different intended content is a conflict, and
semantic equality never adopts an unowned existing file.

The plan projects affected generated `Entries` from intended authored topology
and metadata using current Index rules. Generated interiors are derived
navigation, not package-owned authored bytes. A missing, duplicate, reversed,
nested, or ambiguous boundary blocks; force does not repair it. Extension payloads
cannot target the lifecycle document, repository metadata, recovery bundles or drafts,
workspace overwrite companions, Framework blocks, or another manager's paths.

## Semantic Fingerprints

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware semantic
fingerprints. Preserve Unicode, semantic text, headings, tags, links and
destinations, marker meaning, inline content, code blocks, and significant
whitespace. Normalize only line endings and parser-proven formatting trivia.
Unsupported, binary, or unparseable kinds use exact-byte identity and fail
closed.

Persist only semantic baseline fingerprints for supported kinds. Capture exact
current bytes freshly for plan, diff, expected-state revalidation, verification,
deletion or write, and recovery. Do not persist a new exact-byte baseline digest.
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
existing-target effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`). An
operation containing only Create effects or
no-ops creates no bundle. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
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
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery attention
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

## Output And Results

Human output reports exact workspace and source, selected roots and dependency
closure, package and target-relative footprint, normal/force/automatic and
apply/dry-run mode, ownership conflicts, generated projections, lifecycle
publication, recovery, preserved facts, status, and at most one next action.
JSON emits one complete structured result from the same typed result for every
status.

| Result        | Meaning for `extension install`                                                                                                                                                                                                                                                                                                                                 |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Absent selected packages were fully installed and verified, an eligible initial force completed, an exact managed state was verified as a no-op, or a complete pre-effect dry-run finished without finite attention.                                                                                                                                            |
| `attention`   | Complete safe coverage preserves a finite lifecycle observation, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`. Managed divergence itself is `blocked` and directs to update. `Failed`/`Retained` recovery keeps target effects successful and reports the exact residual path with cleanup guidance. |
| `incomplete`  | Safe source, Framework-anchor, lifecycle, dependency, parser, route, or recovery coverage is unavailable. No write occurs.                                                                                                                                                                                                                                      |
| `invalid`     | IDs, source, `--all`, flags, operands, repetition, or terminal-mode input is invalid.                                                                                                                                                                                                                                                                           |
| `blocked`     | Unsafe, ambiguous, colliding, untrusted, unauthorized, retained, ownership, route, marker, or containment facts prevent one plan.                                                                                                                                                                                                                               |
| `failed`      | Application, lifecycle publication, or verification fails unexpectedly after effects begin, or post-verification recovery deletion returns `Failed`/`Unknown`.                                                                                                                                                                                                  |
| `interrupted` | The caller interrupts before completion and no unexpected application or verification failure remains.                                                                                                                                                                                                                                                          |

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr. JSON uses one result on stdout for every status.

## Errors And Examples

Every error names `extension install`, selected IDs/source/workspace, the cause,
and at most one useful next action. An installed-only ID without embedded or
selected package bytes is `incomplete`, writes nothing, and directs the caller
to `extension inspect` or to provide `--source`; it is not an exact no-op.

Install one embedded package:

```text
open-forge extension install development-toolkit
```

Install all available roots explicitly with a preview:

```text
open-forge extension install --all --automatic --dry-run --json
```

Install one exact local package with eligible initial force:

```text
open-forge extension install development-toolkit --source D:/packages/open-forge --force
```

Managed divergence is not reconciled by the last request. It returns `blocked`
with `Next: open-forge extension update development-toolkit`.

## Non-Goals And Public Conformance

Install does not update managed divergence, delete retired content, remove a
package source, select IDs by automatic inference from a multi-package source,
use semver to choose a source, access a network/registry/cache, adopt unmanaged
content, mutate Framework ownership, repair markers, run a formatter, use a
hidden `index` command, or create a saved plan/journal.

Conformance must cover exact source universe and ID rules, explicit `--all`,
single-package inference, dependency-first closure and failures, Framework-anchor
and route-host prerequisites, absent/no-op/divergent/initial-force states,
automatic and wizard/direct behavior, trusted/untrusted/absent handling, shared owners,
semantic fingerprints, generated navigation, reserved paths, complete planning,
external recovery-bundle storage and verification, typed post-verification
deletion state/disposition facts, dry-run parity,
seven statuses/streams, JSON, no mutation of
sources, and no runtime or shipping claim. The shared CLI Architecture defines
the exact JSON result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.
