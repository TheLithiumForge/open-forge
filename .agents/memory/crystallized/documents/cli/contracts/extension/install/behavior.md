---
open-forge:
  description: Accepted technology-neutral Extension install behavior for source-universe closure, ownership establishment, and initial force
  responsibility: Define install's deterministic package resolution, complete plan, lifecycle publication, verification, recovery, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Install, Behavior, Dependency, Ownership, Safety, Recovery, CurrentTruth]
---

# extension install Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping
`open-forge extension install`. It defines request resolution, one exact source
universe, dependency closure, Framework-anchor and route-host facts, lifecycle
trust, semantic identity, intended topology, ownership planning, initial force,
dry-run/application, verification, recovery, result formation, and conformance.
The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the exact shared JSON result schema and exit mapping. The accepted [CLI
Architecture](../../../architecture.md) defines package serialization
relationships, parser and filesystem boundaries, and cross-cutting implementation
structure. This behavior does not duplicate those mechanics or
claim their Gate 5 proof.

## Complete Typed Flow

```text
validated command and package selection
  -> exact workspace and one source universe
  -> Framework anchor and route-host facts
  -> manifests, stable IDs, dependency closure, and package paths
  -> current lifecycle, ownership, and semantic facts
  -> intended authored topology and generated projection
  -> one dependency-first complete plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> dependency-first target and generated effects with per-effect verification
  -> intended target-topology verification
  -> Extension lifecycle publication and verification as the last workspace file effect
  -> final target, Extension lifecycle, and unchanged Framework reread
  -> recovery-disposition reporting
  -> one typed result
```

No effect begins until all selected roots, dependencies, ownership sets, route
hosts, generated boundaries, lifecycle sections, recovery-bundle, verification,
and preservation facts pass. The operation does not apply a safe subset around a blocked
dependency or path.

## Typed Callable Boundary

The command uses the existing closed shell operation delegate exactly as
`CliOperation<ExtensionInstallRequest, ExtensionInstallResult>`. Its directly
callable operation entry is equivalent to:

```text
ValueTask<ExtensionInstallResult> ExecuteAsync(
  ExtensionInstallRequest request,
  CancellationToken cancellationToken)
```

`ExtensionInstallRequest` is one immutable command-local value containing, in
order, these exact typed members:

```text
CliWorkspace Workspace
ExtensionInstallMode Mode
IReadOnlyList<string> RequestedIds
bool All
string? SourcePath
bool Force
bool Automatic
bool AllowInteraction
```

`ExtensionInstallMode` has only `Apply` and `DryRun`. `RequestedIds` is a
non-null immutable ordered snapshot. Empty requested IDs and `All = false`
preserve an unresolved selection for the operation to resolve through
single-package inference, the bounded prompt, or an `invalid` result. The
request carries no parser object, writer, stream, service collection, registry,
or context bag.

`ExtensionInstallResult` implements the shared non-wire `ICliCommandResult`
boundary and contains the exact command identity `extension install`, one shared
semantic status, genuine workspace presence, one shared next action or `null`,
and the fourteen ordered command-local facts defined by the Interface Contract.
Its exact typed members are:

```text
string Command
CliSemanticStatus Status
CliWorkspace? Workspace
CliNextAction? Next
ExtensionInstallMode Mode
bool Force
bool Automatic
ExtensionInstallSelection? Selection
ExtensionInstallSource? Source
IReadOnlyList<ExtensionInstallPackage> Packages
ExtensionInstallFramework? Framework
ExtensionInstallFootprint? Footprint
IReadOnlyList<ExtensionInstallEffect> Effects
ExtensionInstallGeneratedNavigation? GeneratedNavigation
ExtensionInstallLifecycle Lifecycle
ExtensionInstallRecovery Recovery
ExtensionInstallVerification Verification
IReadOnlyList<ExtensionInstallFinding> Findings
```

The command-local fact types are `ExtensionInstallSelection`,
`ExtensionInstallSource`, `ExtensionInstallPackage`,
`ExtensionInstallFramework`, `ExtensionInstallFootprint`,
`ExtensionInstallEffect`, `ExtensionInstallGeneratedNavigation`,
`ExtensionInstallLifecycle`, `ExtensionInstallRecovery`,
`ExtensionInstallVerification`, and `ExtensionInstallFinding`. Collections are
immutable and non-null. The nullable atomic facts remain nullable rather than
using plausible placeholder data.

Human, JSON, diagnostic, status, next-action, and process-completion projection
all consume that one result. The JSON document is a concrete source-generated
projection of the same fields and is never the operation or shell interface.
No dependency injection, service locator, reflection, runtime registry, generic
operation engine, or untyped command-local state participates in this boundary.

## Request, Workspace, And Source Resolution

The resolver rejects incompatible IDs and `--all`, duplicate positional IDs,
repeated singleton source values, unknown flags, Framework-group forms, and
terminal-mode conflicts. It collapses applicable Boolean flags idempotently and
keeps automatic, force, and dry-run independent; unknown options are invalid.

It selects exactly CWD or exact `--workspace`; no parent, Git-root, nested-root,
marker, or nearby source discovery is available. An external source must be
lexically and physically disjoint from the target workspace and is never
mutated.

Classify `--source` structurally as one package or catalogue. If omitted, use the
embedded catalogue deterministically and never prompt for a source. Resolve IDs
only from explicit operands, explicit `--all`, the one-package manifest-ID
inference rule, or the bounded exact-selection prompt. When the selected source
contains exactly one completely validated package and no IDs or `--all` were
supplied, use its valid manifest ID in human, non-interactive, and automatic
requests.

A prompt-capable human multi-package request displays the finite inventory and
accepts only exact stable package IDs or exact `all`. Invalid answers retry
locally. Selected dependencies are displayed as the complete mandatory closure
and are not optional answers. End-of-input produces a no-write `invalid` result
and cancellation produces a no-write `interrupted` result. JSON, automatic, and
other non-interactive multi-package requests without explicit selection are
`invalid`. Automatic mode never chooses among packages or broadens selection to
all.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Framework Anchor And Route Hosts

Before forming a managed mutation plan, establish a trustworthy installed
Framework anchor for the exact workspace. Establish complete facts for every
affected authored host, generated `Entries` boundary, ownership relationship,
route relationship, and cross-section preservation boundary. List and create do
not need this anchor; install and update do. If a required safe fact is
unavailable, return `incomplete`; if it is unsafe or ambiguous, return
`blocked` and write nothing. The installed anchor includes the existing
`.agents` Framework container. A safely absent container is therefore an
unavailable anchor and produces `incomplete` before planning; an unsafe or
ambiguous container remains `blocked`. Extension Install never plans creation of
`.agents` itself. The shared directory-creation capability remains unchanged;
after the anchor is established, this command may consume it only for explicitly
planned missing descendant directories beneath `.agents`.

## Manifest And Dependency Closure

Parse every selected manifest and validate its stable ID, optional descriptive
version, dependencies, package path, and source identity under the future package
rules. Resolve exact stable-ID dependencies transitively within the selected
source universe, offline and without semver negotiation. Reject unknown IDs,
duplicate active IDs, duplicate dependency declarations, invalid manifests,
cycles, unsafe paths, conflicting source identities, and incomplete closure.

Validate every payload target in that complete closure as a canonical strict
descendant of `.agents/` before planning. Reject `.agents` itself and every
target outside it, including `.apm/`, before a plan, lock, recovery artifact, or
workspace effect exists. Only legitimately missing descendant directories
beneath the established `.agents` anchor may enter the command-local directory
effect set. This is Extension Install policy only. It consumes the shared
directory-creation capability and `ExtensionTargetPath` grammar without
changing, narrowing, or replacing either shared authority. The deferred `.apm/`
idea creates no behavior or implementation authority here.

Deduplicate the closure by stable package identity and order dependencies before
dependents. Keep selected roots, dependency edges, and order facts in the one
operation result. A descriptive version never selects a different source or
grants compatibility authority.

## Lifecycle Trust And Ownership

Read `.agents/open-forge.lifecycle.json`, schema v1, as isolated `framework` and
`extensions` sections. Validate and preserve the unrelated section and common
envelope meaning semantically. Publish a selected Extension semantic change only
after dependency-first target and generated effects and intended-topology
verification. Verify it as the last workspace file effect, then reread targets,
Extension lifecycle, and unchanged Framework meaning before success or recovery
cleanup. Publication emits one deterministic canonical UTF-8 whole-document
representation, so lifecycle property order, whitespace, and line endings may
be normalized. A semantic no-op writes nothing. Prior bytes for every
existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) remain
only in the verified external recovery bundle described below; the CLI does not
inspect or report repository state or claim history evidence. The document
stores no plan, runtime history, journal, recovery evidence, or session. Files
outside this exact path are ordinary workspace content, not lifecycle input.

The `extensions` section is trusted only with exact workspace binding, stable IDs,
dependency reciprocity, target-relative paths, shared owner sets, semantic
baseline fingerprints, supported versions and policy, no duplicate/conflicting
identities, and complete verifiable coverage. A safely absent section is valid
only after complete absence and recovery inspection. An absent document or
section is not, by itself, proof of unmanaged state. Missing expected, malformed,
unsupported, unverifiable, or inconsistent evidence is never treated as empty
or trusted; safe unavailable coverage is `incomplete` and unsafe ambiguity is
`blocked`.

Manual copying, idless packages, direct overlays, and external native Skills
remain unmanaged. Path, route, byte, or semantic coincidence never adopts them.

## Semantic Fingerprints And Shared Owners

For supported parseable kinds, compute the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware semantic
fingerprints. Preserve Unicode, semantic text, headings, tags, links,
destinations, marker meaning, inline and code-block content, and significant
whitespace. Normalize only line endings and parser-proven formatting trivia.
Exclude derived generated `Entries` interiors from authored package identity.
Unsupported, binary, and unparseable kinds use exact-byte identity and fail
closed.

Persist semantic baseline fingerprints, not exact-byte baseline digests. Capture
current exact bytes freshly for plan, diff, expected-state, write, verification,
recovery-bundle payload, and recovery. Formatting-only equal semantic identity is not divergence. No
formatter executes or produces persisted formatter state.

When two explicit package owners target one physical path, share it only when
canonical semantic fingerprints and path, route, and metadata facts are
compatible. Formatting-only source-byte differences are compatible. Different
intended content is a conflict. Semantic equality never adopts an unowned
existing path.

## Intended State And Generated Projection

Form one hypothetical post-install workspace from current authored content plus
selected package/dependency payload effects, preserving user content, overwrite
companions, Framework regions, and unrelated lifecycle facts. Project every
affected generated region from that intended authored topology and metadata using
the Index contract. Generated interiors are derived and not package-owned.

Reject package paths targeting the lifecycle document, repository metadata,
recovery or temporary artifacts, workspace-owned overwrite companions, Framework
root/provider blocks, another manager's path, or any location that fails the
command-local `.agents/`-descendant restriction. Missing, duplicate, reversed,
nested, misplaced, or ambiguous generated boundaries block; force never repairs
them. Include generated effects in the same parent plan and never invoke a hidden
index subprocess.

## Normal And Initial Force Planning

For each selected root and dependency:

- An absent safe footprint may be created dependency-first and becomes managed
  only after complete verification.
- An exact trusted managed installation is a verified no-op.
- A trusted managed changed, missing, retired, or source-divergent state is not
  updated by install. Return `blocked` with one update next action and no write.
- A recognized installed-only ID without embedded or selected source bytes is
  `incomplete`, not an exact no-op.
- A current exact destination occupied before management is eligible for force
  only with no trusted/competing owner, collision, marker ambiguity,
  containment, or recovery issue. Force replaces current source bytes and records
  the new verified state; it does not adopt old bytes.

Force never replaces managed divergence, deletes, adopts, changes ownership,
overrides a shared owner, repairs markers, or bypasses containment,
verification, or recovery. Automatic mode admits only safe absent/no-op effects
already selected by explicit IDs, `--all`, or the permitted single-package
manifest-ID inference; it never adds force or chooses among packages.

After exact inspection proves an eligible initial occupant, a prompt-capable
human request without `--force` may ask only whether to grant force for the
exact eligible initial occupants displayed for that request. Invalid answers
retry locally. Declining leaves the occupants unchanged and returns `blocked`.
End-of-input is no-write `invalid`, and caller cancellation is no-write
`interrupted`. Automatic and non-interactive requests never ask and remain
`blocked` without explicit `--force`. No later generic apply confirmation
exists.

## Preflight, Dry-Run, Application, And Recovery

The complete plan contains package/dependency order, exact source and target
identity, current bytes and semantic facts, intended bytes, shared owners,
generated-region effects, Extension-section publication, expected-state guards,
recovery-bundle readiness, verification, and preservation. Before the first
existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`), use only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a pre-effect
`incomplete` result. It prepares exactly one immutable ZIP bundle outside the
workspace. An operation containing only
Create effects or no-ops does not resolve recovery storage and creates no bundle.
Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires the matching preparation for every existing-target effect
and performs one final effect per target. All preparation completes before the
first target effect; unknown, malformed, mismatched, or colliding bundles block.
The persistent reusable zero-byte external workspace lock is held with one
read/write `FileShare.None` handle; it never receives metadata writes, deletion,
or truncation.

Dry-run uses the same request, facts, intended state, plan, and preflight as
application. It shows the complete selected closure, effects, preserved and
conflicting facts, generated projections, and lifecycle publication that would
follow verified apply, then writes nothing, including no lifecycle section,
recovery bundle, or temporary artifact.

Dry-run forms the same pre-effect planning status as application. Because it
performs no effects, it never produces an apply-time `failed` or `interrupted`
result. A planning or read failure and caller cancellation before effects retain
their own event meaning.

Application revalidates every selected source, target, owner, route, marker,
containment, expected-state, and recovery fact under the lease before effects.
It then applies package target and generated-region effects in the exact
dependency-first plan order and verifies each effect. After those effects, it
verifies the complete intended target topology.

Only verified target topology permits Extension lifecycle publication. That
publication is the last workspace file effect and is verified before any final
success decision. The operation then freshly rereads every target, the complete
Extension lifecycle section, and the Framework lifecycle section. Success
requires exact target bytes and identity, exact Extension lifecycle meaning,
and unchanged Framework meaning. No recovery cleanup starts before all four
verification members are `verified`.

After that final reread, delete only the positively recognized bundle created by
this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and cleanup guidance.
`Failed`/`Unknown` produces `failed` and reports an exact expected path only when
the deletion result provides one.
When `Failed`/positively observed `Retained` recovery attention coexists with a
finite lifecycle observation, cleanup guidance owns the single next action; the
lifecycle facts remain visible evidence.

Before post-verification deletion begins, a handled application, verification,
publication, or cancellation outcome stops new effects. Never restore, reverse,
or compensate for an earlier effect. Preserve concurrent changes and report the
actual residual draft or final path; a valid final remains when preparation
completed. A closed final ZIP may remain after abrupt process termination,
without an executable crash or power-loss guarantee. Recovery provenance does
not classify current target state. Cleanup owns exact named final and draft
deletion under its separate lease-bound contract.
A later invocation makes a fresh plan and never replays a saved plan or journal.

## Result Formation And Conformance

Form one typed result containing selection origin, source universe, roots and
closure, Framework-anchor and route-host coverage, baseline/current/intended
facts, owners, effects, generated navigation, lifecycle publication,
preservation, verification, recovery, status, and one next action. Human and
JSON renderers consume it once. Use the seven statuses and streams from the
Interface.

Conformance must cover source and selection rules, dependency failures and
ordering, Framework-anchor gating, trusted/untrusted/absent/unavailable state,
initial force and managed-divergence block, exact bounded prompt outcomes,
shared owners, semantic fingerprints, generated navigation, strict
`.agents/`-descendant payload validation before planning, directory effects
limited to missing descendants beneath the established anchor, reserved paths,
complete planning, dependency-first target/generated application,
target-topology verification, last-effect Extension lifecycle publication,
final target, Extension lifecycle, and Framework lifecycle rereads, external
recovery-bundle storage and verification, typed post-verification deletion
state/disposition facts, dry-run no-effects, revalidation, retained partial
state without restoration, no-op repetition, exact result/finding formation,
JSON/human parity, and no package-source mutation. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must
prove source-generated serialization, fixed Markdig where used, real
`System.IO`, Native AOT, OS locking, isolated tests, and package journeys.
