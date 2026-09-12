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
recovery bundle defined by the [Mutation And Recovery Technical
Design](../../../technical-designs/mutation-and-recovery.md); the CLI does not
inspect or report repository state or claim history evidence.
The document stores no plan, runtime history, journal, recovery evidence, or session.
Files outside this exact path are ordinary workspace content, not lifecycle input.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the structured JSON result schema and numeric exit mapping. The [CLI
Architecture](../../../architecture.md) defines concrete source-generated package
serialization relationships. This Interface uses those shared definitions
without duplicating implementation mechanics. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.

## Consumer Destination Permissions

Consume the [Workspace Permissions Interface](../../shared/workspace-permissions/interface.md) and
[Behavior](../../shared/workspace-permissions/behavior.md). Require an exact grant per package for every external target in the complete
selected dependency closure.
Existing `.agents/` targets need no grant; their prior safety and ownership
checks remain. Revocation blocks the complete selected lifecycle operation,
including ownership release, until exact explicit reapproval. Unrelated
installed packages do not enter this request's required set.

An eligible human apply request asks once for the complete missing set after
safe preflight. JSON, automatic, redirected and dry-run execution never ask the
permission question or create grants. Existing selection and force/prune
questions keep their separate rules. Force and prune never supply permission.
Malformed or unsafe permission storage is diagnosed without overwriting it.

Permission create/replace is a declared control-file effect. Revalidate the
observed document and approved plan under the existing workspace lease. Cover
prior permission bytes or proven absence in the one verified operation bundle,
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
caller cancellation before an effect keeps the existing interrupted outcome.
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
occurs. The installed anchor includes the existing `.agents` Framework
container, so Extension Install cannot form a plan or create `.agents` when that
container is absent. This does not change the shared directory-creation
capability: after the anchor is established, this command may use it only for
explicitly planned missing descendant directories beneath the workspace for
admitted targets.

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

End-of-input during either prompt is a no-write `invalid` result. Caller
cancellation during either prompt is a no-write `interrupted` result. A caller
may decline eligible initial force; that leaves the occupants unchanged and
returns `blocked`. There is no generic confirmation before applying an already
authorized plan.

JSON, `--automatic`, and other non-interactive requests never prompt. Missing
package selection in a multi-package source is `invalid`; an eligible occupant
without explicit `--force` is `blocked`. `--automatic` uses only explicit IDs,
explicit `--all`, or the permitted single-package manifest-ID inference plus
deterministic safe effects. It grants neither selection nor force and never
broadens selection, adds replacement, adoption, ownership, or a safety bypass.

## Lifecycle Identity, Ownership, And Generated Navigation

The physical lifecycle document is `.agents/open-forge.lifecycle.json`, schema v1,
with separate `framework` and `extensions` sections. After dependency-first
target and generated effects and intended-topology verification, Install
publishes only the `extensions` change as the last workspace file effect. It
verifies that publication, then rereads every target, the Extension lifecycle
section, and unchanged Framework meaning before success or recovery cleanup.
Install preserves the common envelope and unrelated `framework` section meaning
semantically. A selected semantic change emits one deterministic canonical UTF-8
whole-document representation, so lifecycle property order, whitespace, and
line endings are not preserved. A semantic no-op writes nothing. Prior bytes for
existing replaced or deleted targets are retained only in the verified external
recovery bundle described below; the CLI does not inspect or report repository
state or claim history evidence. An absent document or section is not, by itself,
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
nested, or ambiguous boundary blocks; force does not repair it.

Every package payload target must be a canonical portable workspace-relative
file. Validate complete closure, exact consumer grants for external files, and
all protected destination rules before permission or content effects. The
established Framework anchor keeps `.agents` itself outside both payload and
directory-effect sets. Only declared missing ordinary parents beneath the
workspace can be directory-create effects; an external grant covers the exact
file and does not grant ownership of its parents or siblings.

Extension payloads also cannot target the lifecycle document, repository
metadata, recovery bundles or drafts, workspace overwrite companions, Framework
blocks, or another manager's paths.

An exact destination path claim in the consumer Library record
`.agents/open-forge.libraries.json`, or a real relative projection link at that
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

Human and JSON presentation consume one immutable `ExtensionInstallResult`.
Both human views lead with outcome or preview, status, exact workspace/selection,
source and selected packages/dependency closure. Findings and permission/safety
conditions precede the affected paths. Every effect retains its actual action,
outcome and residual; a planned path is not presented as an applied change.

Human output groups effects by exact package/path identity and preserves any
payload or directory footprint path not represented by an effect. Expanded adds supporting Framework,
footprint, generated-navigation, permission, installation-record and verification
facts. Compact uses shorter rows while retaining package dependencies, effective
force/automatic/preview choices, blockers, affected/preserved paths and recovery
state/residual paths. Both show the actual Next command once when supplied;
expanded may add its reason. Paths are not truncated.

Compact may summarize navigation-only footprint paths by count when their
observed navigation is unchanged and they have no payload, directory or effect
that must remain visible. Expanded retains
those observations. Every changed or uncertain path and every actual effect
remains visible in both views.

Human grouping does not change dependency/effect order in the operation or JSON.
JSON retains the complete payload and declared order below. Help retains the
same selection, interaction, force and no-write rules. Diagnostics remain bounded
and project only known result facts without changing the result.

The command-local JSON `result` uses camel-case properties in exactly this
order. Every property is present for every semantic status:

1. `mode`: `apply` or `dry-run`;
2. `force`: Boolean;
3. `automatic`: Boolean;
4. `selection`: `null` or one atomic object whose members are `selectedBy` and
   `rootIds`, in that order;
5. `source`: `null` or one atomic object whose members are `kind`, `path`,
   `identity`, and `packageCount`, in that order;
6. `packages`: a non-null dependency-first array whose members are `id`,
   `selectedRoot`, and `dependencies`, in that order;
7. `framework`: `null` or one atomic object whose members are
   `inventoryFingerprint`, `targetCount`, and `generatedRegionCount`, in that
   order;
8. `footprint`: `null` or one atomic object whose members are `packageCount`,
   `payloadTargets`, `generatedRegions`, and `directories`, in that order;
9. `effects`: a non-null ordered array whose members are `path`, `packageId`,
   `kind`, `action`, `outcome`, and `residual`, in that order;
10. `generatedNavigation`: `null` or one atomic object whose sole member is the
    non-null `regions` array; each region contains `path` and `state`, in that
    order;
11. `permissions`: the complete object defined by the Workspace Permissions
    Interface, in its declared member order;
12. `lifecycle`: one object whose members are `action` and `outcome`, in that
    order;
13. `recovery`: one object whose members are `state`, `protectedPaths`, and
    `residualPath`, in that order;
14. `verification`: one object whose members are `targets`, `topology`,
    `extensionsLifecycle`, and `frameworkLifecycle`, in that order;
15. `findings`: a non-null ordered array whose members are `code`, `status`,
    `target`, and `cause`, in that order.

Every array is present and non-null, including nested `rootIds`, `dependencies`,
`regions`, and `protectedPaths` arrays. `selection`, `source`, `framework`,
`footprint`, and `generatedNavigation` are atomic nullable facts when their
complete safe value is unavailable before an early terminal result. Their
members are not independently nullable except for `source.path`, which is
`null` exactly for the embedded catalogue. Nonnegative counts are required.
`framework.inventoryFingerprint` is a lowercase SHA-256 value when the
Framework fact is present.

`selection.selectedBy` is `explicit-ids`, `explicit-all`,
`single-package-inference`, `interactive-ids`, or `interactive-all`.
`selection.rootIds` contains the exact selected root IDs in ordinal order.
`source.kind` is `embedded`, `package`, or `catalogue`. A non-null `source.path`
is the exact normalized source path. `source.identity` is the exact resolved
embedded or external source identity.

`packages` is the complete resolved closure in dependency-first order.
`selectedRoot` distinguishes explicit or inferred roots from dependencies.
Every package `dependencies` array contains its direct stable-ID dependencies in
ordinal order.

The three `footprint` arrays contain unique canonical workspace-relative paths
in ordinal order. `payloadTargets` contains only package payload paths,
`generatedRegions` contains only generated host paths, and `directories`
contains only legitimately missing planned descendant directory-create paths
beneath the workspace for admitted targets; it never contains `.agents` itself.

Effect order is the exact planned and application order for target and generated
effects. Dependencies precede dependents. Lifecycle publication follows those
effects separately and is the final workspace file effect when required. Effect
`kind` is `directory`, `package-file`, or `generated-region`. A directory action
is `create` only for one declared missing ordinary parent of an admitted target;
a package-file action is `create` or `replace`; and a generated-region action is
`replace`. `packageId` is the exact package that owns a package effect and is
otherwise `null`. Effect `outcome` is `planned`, `not-started`, `verified`,
`verification-failed`, or `completion-unknown`. Effect `residual` is `none`,
`retained`, or `unknown` and is never Boolean.

Generated region `state` is `changed` or `unchanged`. Lifecycle `action` is
`none`, `preserve`, or `publish`. Lifecycle `outcome` is `not-requested`,
`planned`, `already-current`, `not-started`, `verified`,
`verification-failed`, or `completion-unknown`. Recovery `state` is
`not-required`, `not-created`, `removed`, `retained`, or `unknown`.
`protectedPaths` contains the exact canonical workspace-relative paths covered
by the final recovery bundle. `residualPath` is the exact absolute external
recovery path when known and is otherwise `null`.

Each verification member is `not-requested`, `planned`, `verified`, `failed`,
or `unknown`. Successful application requires `verified` targets, topology,
Extension lifecycle, and unchanged Framework lifecycle. A dry run uses
`planned` for the checks that application would perform. A verified no-op may
use `verified` without an effect.

Finding `code` uses exactly the following vocabulary and status mapping in this
declaration and primary ordering sequence:

| Code                                             | Status        | Meaning                                                                                   |
| ------------------------------------------------ | ------------- | ----------------------------------------------------------------------------------------- |
| `extension-install.invalid-input`                | `invalid`     | Syntax, normalized input, repetition, or terminal-mode input is invalid.                  |
| `extension-install.selection-required`           | `invalid`     | A multi-package source has no explicit or interactive exact selection.                    |
| `extension-install.interaction-ended`            | `invalid`     | End-of-input ended a selection or eligible-force prompt.                                  |
| `extension-install.source-unavailable`           | `incomplete`  | The selected source cannot be read completely.                                            |
| `extension-install.source-invalid`               | `invalid`     | The selected package or catalogue has invalid identity, manifests, or dependency closure. |
| `extension-install.framework-unavailable`        | `incomplete`  | Complete trusted Framework-anchor facts are unavailable.                                  |
| `extension-install.framework-unsafe`             | `blocked`     | Framework identity, ownership, or route-host facts are unsafe or ambiguous.               |
| `extension-install.lifecycle-unavailable`        | `incomplete`  | Required lifecycle facts cannot be read completely.                                       |
| `extension-install.lifecycle-blocked`            | `blocked`     | Lifecycle facts are malformed, unsupported, untrusted, or conflicting.                    |
| `extension-install.managed-divergence`           | `blocked`     | A managed package differs from its baseline and requires Extension Update.                |
| `extension-install.initial-force-required`       | `blocked`     | An eligible initial occupant remains without exact force authority.                       |
| `extension-install.ownership-conflict`           | `blocked`     | Another owner or manager conflicts with a selected effect.                                |
| `extension-install.permission-required`          | `blocked`     | Exact external destination grants are missing.                                            |
| `extension-install.permission-declined`          | `blocked`     | The caller declined the complete missing grant set.                                       |
| `extension-install.permissions-invalid`          | `blocked`     | The consumer permission document is malformed or unsafe.                                  |
| `extension-install.permissions-unavailable`      | `incomplete`  | Required consumer permission facts cannot be read completely.                             |
| `extension-install.permissions-changed`          | `blocked`     | The observed permission document changed before application.                              |
| `extension-install.permission-write-failed`      | `failed`      | Permission publication or verification failed or completion is unknown.                   |
| `extension-install.target-unsafe`                | `blocked`     | A selected target is unsafe, reserved, colliding, or cannot be revalidated.               |
| `extension-install.projection-unavailable`       | `incomplete`  | Intended topology or Generated Navigation cannot be formed completely.                    |
| `extension-install.generated-region-unsafe`      | `blocked`     | A required generated-region boundary is missing, malformed, or ambiguous.                 |
| `extension-install.workspace-lock-unavailable`   | `blocked`     | The exact workspace lease cannot be acquired safely.                                      |
| `extension-install.target-changed`               | `blocked`     | Source, target, ownership, route, marker, or lifecycle state changed before an effect.    |
| `extension-install.recovery-conflict`            | `blocked`     | A recovery candidate or destination conflicts with the operation.                         |
| `extension-install.recovery-unavailable`         | `incomplete`  | Required external recovery storage or evidence is unavailable before effects.             |
| `extension-install.lifecycle-observation`        | `attention`   | Complete safe coverage retains a finite non-blocking lifecycle observation.               |
| `extension-install.recovery-artifact-retained`   | `attention`   | Verified target effects succeeded but a positively retained recovery artifact remains.    |
| `extension-install.write-failed`                 | `failed`      | A planned target or generated-region effect failed or could not be verified.              |
| `extension-install.topology-verification-failed` | `failed`      | Intended target topology did not verify after target and generated effects.               |
| `extension-install.lifecycle-publication-failed` | `failed`      | Extension lifecycle publication failed or could not be verified.                          |
| `extension-install.verification-failed`          | `failed`      | Final target, lifecycle, or Framework-preservation reread failed.                         |
| `extension-install.recovery-failed`              | `failed`      | Recovery preparation or cleanup ended with unsafe or unknown completion.                  |
| `extension-install.operation-failed`             | `failed`      | Another unexpected Extension Install failure occurred.                                    |
| `extension-install.interrupted`                  | `interrupted` | Caller cancellation stopped the operation without a stronger failure.                     |

Findings order first by that code order. For equal codes, a `null` target comes
before every non-null target. Non-null targets and then causes use ordinal
ordering. Status uses exact precedence `failed` > `interrupted` > `invalid` >
`blocked` > `incomplete` > `attention` > `complete`. Every finding status agrees
with its code's declared mapping. The result status is the highest-precedence
finding status; an empty findings array yields `complete`. The result's primary
human stream, shared process status, and numeric exit agree with that status.

The result carries at most one `next` action. Positively retained recovery owns
`open-forge cleanup`. Managed divergence owns `open-forge extension update`
for the exact affected package. Eligible initial force owns the same exact
request with `--force`. Invalid input or unresolved selection owns
`open-forge extension install --help`. An installed-only package without source
bytes directs the caller to `extension inspect` or an exact `--source` rerun.
Recovery cleanup guidance wins when it coexists with another attention fact.
Other results may leave `next` null when no safe deterministic command follows.

The shared schema-v1 envelope already owns `command`, `status`, `workspace`, and
`next`; none is duplicated inside this result. Changing these required result
fields, their order, JSON types, nullability, array-presence rules, or finite
values is a command-local schema-v1 compatibility change.

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
automatic and exact bounded prompt behavior, trusted/untrusted/absent handling,
shared owners, Library-record and projection collisions, independent no-follow
final-leaf guards, semantic fingerprints, generated navigation, eligible
workspace-relative payload targets, rejection before planning, directory effects
limited to missing descendants beneath the workspace for admitted targets,
reserved paths, complete planning, dependency-first target/generated effects, topology
verification, last-effect Extension lifecycle publication, final target and
lifecycle rereads, external recovery-bundle storage and verification, typed
post-verification deletion state/disposition facts, dry-run parity, the exact
ordered result and finite findings, seven statuses/streams, JSON, no mutation of
sources, and no runtime or shipping claim. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.
