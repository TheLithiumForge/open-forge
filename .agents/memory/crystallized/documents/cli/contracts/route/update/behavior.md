---
open-forge:
  description: Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route update`
  responsibility: Define how a conforming implementation resolves one routed-source update and forms, applies, verifies, or recovers its complete result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, Behavior, Metadata, Template, Mutation, CurrentTruth]
---

# route update Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route update`. The command does not ship yet;
implementation and executable proof remain pending Gate 5.

The [Interface Contract](interface.md) defines the complete public syntax,
inputs, observable output, semantic result names, errors, non-goals, and public
examples that this Behavior satisfies. This file defines deterministic
resolution, current facts, result formation, effects, safety, recovery, and
conformance. It does not add operands, flags, aliases, output fields, semantic
results, or other public meaning. Request normalization, status formation, and
presentation follow the accepted repetition, status, dry-run, stream, and
compact-result rules in the Interface Contract.

The [CLI Architecture](../../../architecture.md) defines the accepted shared
result schema, process-status mapping, System.CommandLine binding, fixed Markdig
pipeline, source-generated YamlDotNet and JSON paths, BCL-first filesystem
boundary, workspace lock, recovery boundary, test evidence, runtime, Native AOT,
and source layout. This Behavior Contract remains technology-neutral within
those accepted boundaries.

## Operation Invariants

`route update` performs one complete bounded update of one existing routed
Markdown base source. A conforming implementation follows this conceptual flow:

```text
validated target, field patch, and optional Template
  -> current target, route, and Template facts
  -> complete intended destination bytes
  -> generated-navigation projection
  -> complete ordered mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

For unchanged workspace bytes and explicit input, resolution, intended bytes,
generated navigation, plan, effects, evidence, and semantic result are
deterministic. A successful application repeated against unchanged state
converges to the verified byte-level no-op defined by the [Interface
Contract](interface.md#existing-state-and-no-ops). A protected Template request
retains `attention` while its explicit body intent remains unapplied; other
successful repeated updates are `complete` no-ops.

The operation is a field patch rather than whole-file replacement. It changes
the base file of the resolved logical source, never its overwrite companion,
and protects an authored target body byte-for-byte. Generated navigation is a
separate bounded dependency effect governed by the complete [Index Behavior Contract](../../index-candidate/behavior.md)
contract.

No persistent effect begins until the complete target, Template decision,
current facts, intended destination, generated projection, ordered plan, and
preflight are established. One blocker prevents every effect. There is no
partial-application or best-effort path.

## Request Resolution

### Command and workspace

Request resolution validates the command path, one required source operand,
operation-specific flags, shared global flags, the at-least-one-operation rule,
and the complete command-specific repetition rules according to the [Interface
Contract](interface.md#syntax). `--description`, `--responsibility`, and
`--template` each accept one occurrence only. Any repetition is invalid, even
when the value is equal. Repeated `--tag` values remain accepted and form one
complete replacement list in argument order, subject to the existing empty,
duplicate, and syntax validation. Repeated `--dry-run` collapses to an idempotent
Boolean choice. No command-specific
flag uses last-wins or precedence behavior. Shared global flags retain their
shared rules and are not reinterpreted here.

The selected workspace is the exact current working directory or exact
`--workspace <path>` value under the [Global CLI Flags](../../shared/global-flags/behavior.md)
contract. Resolution does not search parent directories, substitute a Git root,
or infer another workspace from a source operand. Workspace and global terminal
flag behavior remains shared meaning rather than a route-update-specific
implementation choice.

### Source identity and target kind

The resolver passes the source operand through the [CLI Source References](../../shared/source-references/behavior.md)
grammar without guessing whether it is an ID or path. It accepts the automatic
ID and exact `.agents/...` forms described by the Interface, resolves exact
matches and collisions under the shared contract, and records the requested and
resolved identities.

When the reference names a valid base, base path, or adjacent overwrite path,
resolution forms one logical source with its base and overwrite layers. The
mutation target is the base file. The overwrite remains a separate user-owned
layer and is never an effect target. An orphan or ambiguous overwrite cannot
establish the logical source.

The resolver admits only one existing ordinary routed Markdown file or one
recognized entrypoint. It rejects the Loader, `SKILL.md`, non-Markdown
resources, detached unsupported files, and other target kinds excluded by the
[Interface Contract](interface.md#target-source). An existing canonical or
recognized compatibility entrypoint retains its actual filename and route
identity. The resolver does not create a canonical sibling or turn compatibility
input into a rename request.

### Frontmatter and field patch

The resolver establishes exactly one safely parseable scoped `open-forge`
frontmatter block for the base target. It may identify one missing supported
field that the request explicitly supplies. It does not guess a missing
ownership boundary, choose among duplicate blocks, or continue through a
malformed boundary. The complete intended source must remain valid for its
ordinary-file or entrypoint representation.

The field patch is resolved as a set of explicitly supplied operations. Omitted
fields retain their current values. Supplied description, responsibility, and
tag values follow the replacement, addition, exact-empty removal, validation,
and required-metadata rules in [Metadata Patch](interface.md#metadata-patch).
The resolver preserves unrelated top-level and unsupported scoped metadata when
safe preservation can be established. It does not derive, summarize, correct,
or judge field meaning from filenames, bodies, Templates, generated entries, or
overwrite companions.

An invocation with no metadata operation and no Template operation forms the
public `invalid` result. Invalid field syntax, invalid tags, required-metadata
removal, unsafe preservation, or a malformed target boundary forms the public
invalid or blocked result required by the Interface, before any mutation plan
can be applied.

### Template resolution

When `--template` is supplied, the resolver uses the shared existing-source
grammar and the `route create` selection boundary named by the [Interface Contract](interface.md#template-body-completion).
It establishes one existing ordinary routed Markdown source whose base
frontmatter contains the exact canonical `Template` tag. The tag is a source
classification fact for this operation. It does not create a Templates root,
activate another primitive contract, grant authority over the destination, or
create a continuing ownership relationship.

The resolver rejects a Template with an overwrite companion because one-file
instantiation cannot safely collapse two authored layers into one body. It
reads only the selected Template body after its own frontmatter and performs no
placeholder substitution. Template frontmatter is not a destination metadata
source, and no provenance or continuing Template lifecycle state is retained.

The target body is resolved as the bytes after the target frontmatter closing
delimiter. A body containing only whitespace is eligible for canonical
frontmatter-to-body insertion. Any authored non-whitespace byte protects the
complete existing body and suppresses Template copying. The resolver does not
compare headings or decide whether the existing prose is finished. A protected
body does not prevent explicitly requested metadata patches.

For a frontmatter-only entrypoint, the inserted Template body must establish a
complete valid entrypoint representation. For an ordinary routed file, it must
establish valid Markdown under the selected route's applicable contracts. An
invalid intended representation blocks the complete plan.

## Current Facts And Coverage

Before planning, the operation establishes the complete facts needed for one
safe update and its dependency-minimal generated navigation:

- The exact selected workspace, source reference form, automatic ID, canonical
  base path, compatibility form, and logical overwrite layers.
- The base target's readable bytes, source kind, route relationship, scoped
  frontmatter boundary, supported and unrelated metadata, body bytes, and
  entrypoint structure when applicable.
- The current destination metadata and the exact field operations requested by
  the invocation.
- The selected Template's identity, route, source classification, overwrite
  relationship, frontmatter boundary, body bytes, and valid intended use when a
  Template is supplied.
- The exposing parent and any target entrypoint generated region whose direct
  projection can change, including the target's own generated region when
  eligible Template completion establishes one.
- The current authoritative routed topology, authored descriptions and tags,
  generated boundaries, sibling projections, route identities, and actual
  compatibility destinations needed by the [Index Behavior Contract](../../index-candidate/behavior.md)
  projection.
- The expected current bytes, recovery-bundle readiness, and volatile target
  facts needed for preflight and later revalidation.

Current generated entries are comparison input. Filesystem topology and authored
source metadata remain authoritative for the generated projection. The operation
does not let stale generated lines add, hide, or order a routed source.

The operation distinguishes incomplete coverage from an unsafe boundary. When
safe facts are available but required inspection or planning coverage cannot
finish, it forms `incomplete` and no write begins. When source identity, route
meaning, entrypoint structure, frontmatter boundaries, Template classification,
generated ownership, sibling projection, containment, expected state, or
recovery readiness is ambiguous or unsafe, it forms `blocked` under the existing
boundary rules and no write begins. The operation never guesses, applies
metadata first, or leaves generated navigation stale. A changed source or
destination after planning invalidates the plan before stale intent can replace
it; a failure after a write is `failed` under the existing recovery rules.

### Architecture-constrained realization

The following realization details are intentionally not repeated as command-local
technology choices. They must satisfy the CLI Architecture and the observable
requirements in this contract:

- Exact YAML and Markdown parser behavior, compatibility parsing, frontmatter
  preservation mechanics, line endings, encoding, heading and link handling,
  and canonical serialization.
- Exact filesystem APIs, physical identity, symlink and junction behavior, case
  and Unicode rules, atomic replacement mechanics, containment implementation,
  and test seams.
- Exact recovery-bundle filenames, collision-handling mechanics, and related recovery
  implementation boundaries.
- Lock scope, stale-lock handling, and broader cross-platform concurrency
  mechanics. Expected-state revalidation and preservation of unexpected
  concurrent edits remain current safety requirements, and mutation locking
  follows the Architecture's accepted workspace-lock boundary.
- Exact .NET modules, parser and serializer ownership, shared graph or mutation
  boundaries, and other implementation source boundaries.

These realization details do not weaken the current observable requirements for
safe containment, byte preservation, expected-state checks, same-directory
replacement, verification, or recovery.

## Selection And Result Formation

### Intended destination

The operation forms one complete intended destination from the current target,
the explicit field patch, and the Template body decision. It changes only
explicitly supplied metadata fields, removes `responsibility` only for the
exact-empty request, retains unrelated metadata when safe, and preserves the
target body whenever authored content protects it. A Template body is copied
only into an eligible whitespace-only body and becomes independent destination
content.

The intended destination must satisfy the source-type contract, including the
required title, Axioms meaning, final `Entries` section, and generated region of
an entrypoint. Its bytes are not normalized beyond the bounded representation
needed by the requested field patch or eligible body completion.

### Generated projection

After the intended destination is established, the operation projects generated
navigation against the hypothetical post-update workspace. It uses the [Index Behavior Contract](../../index-candidate/behavior.md)
rules for authoritative topology, authored metadata, direct-child projection,
ordering, generated boundaries, bounded interiors, verification, and recovery.

The dependency-minimal generated effects are:

- A description or tag change may change the generated entry in the exposing
  parent.
- Completing a frontmatter-only entrypoint from a valid Template may establish
  that entrypoint's own generated region and change its exposing parent's entry.
- A responsibility-only change and an ordinary-body-only change do not change
  generated entry text.

The operation blocks when it cannot establish a required generated boundary,
sibling projection, or route relationship. It never starts a hidden `index`
subprocess or schedules a second indexing pass.

### Comparison and plan formation

The planner compares the complete intended destination and every automatic
generated effect with current bytes. It classifies exact equality as an
unchanged observation and a byte difference as a replacement effect.

- An explicitly supplied field already at its intended value creates no field
  effect.
- Removing an already absent responsibility creates no effect.
- A protected target body is not compared with or recopied from the Template.
- An unchanged generated projection creates no effect.

The complete plan contains at most one destination replacement and the
dependency-minimal generated-region replacements required by the intended
metadata or route representation. Changes to the same physical file coalesce
into one exact replacement. Unchanged bytes receive no rewrite, formatting pass,
or timestamp-only effect.

A complete intended state with no replacement effects forms verified byte-level
no-op facts. When the caller supplied `--template` and authored non-whitespace
body content protects the target, those facts remain available but the semantic
status is `attention` because the explicit Template intent was not applied.
The public semantic conditions for `complete`, `attention`, `incomplete`,
`invalid`, `blocked`, `failed`, and `interrupted` remain exactly those in
[Semantic Results](interface.md#semantic-results); Behavior does not define
another status or an exit mapping.

The status formation rules are deterministic:

- `complete` is formed for a complete safe dry-run plan or a completed and
  verified application, including ordinary changes and all other verified
  no-ops, when the protected-Template `attention` condition does not apply.
  Planned changes alone do not create `attention`.
- `attention` is formed only for the complete protected-Template condition above.
  In apply mode, requested metadata and generated effects complete and verify
  when present. In dry-run, the same condition and effect evidence are reported
  without writes. A Template-only request may have no replacement effect and
  still forms `attention`.
- `incomplete` is formed when safe facts are available but required inspection or
  planning coverage cannot finish. No mutation begins.
- `invalid` is formed for input, field, Template-reference, repetition, or
  target-kind violations. `blocked` is formed for a valid request whose safe
  complete plan cannot be established because an unsafe or ambiguous boundary
  remains. Neither is converted to `attention`.
- `failed` is formed when an application, verification, bundle-handling failure
  after effects begin, or another unexpected failure after a write prevents
  completion. `interrupted` retains its cancellation meaning when no unexpected
  application or verification failure changes the result.

For ordinary operation conditions, status precedence is `blocked` > `incomplete`

> `attention` > `complete`. Invalid input stops before operation resolution and
> forms `invalid`. Failed and interrupted results retain their event meaning.

## Effects

### Complete planning and preflight

The operation resolves every target and dependency, establishes every expected
fact, forms every intended byte sequence, and completes the ordered plan before
the first persistent effect. A blocker in the destination, Template, route,
generated projection, recovery-bundle readiness, containment, or expected
state blocks the complete plan. Safe but unfinished inspection or planning
coverage forms `incomplete` before any effect. There is no partial-application or
best-effort mode.

The plan preserves siblings, overwrite companions, compatibility filenames,
authored target body content, and all bytes outside planned generated interiors.
It coalesces compatible changes to one physical file rather than scheduling
competing writes.

### Dry-run parity

Dry-run and application use the same normalized request, target resolution,
current facts, field patch, Template decision, intended bytes, generated
projection, planner, expected-state facts, preflight, and semantic status.
Dry-run stops before recovery-bundle creation, temporary file creation, replacement,
formatting, or another persistent effect.

Dry-run rendering exposes every exact intended destination and bounded generated
diff required by the complete plan, plus the body-protection observation when an
authored body prevents Template copying. It does not expose unrelated authored
bytes or private recovery material. It reports no persistent file changes and
does not claim verification of bytes that were not written. A protected Template
request has the same `attention` status and observation as application; planned
changes alone do not create `attention`.

### Application

When `--dry-run` is absent, the explicit target and patch request confirms only
the described metadata changes, eligible Template body completion, and planned
generated-region changes. The operation does not prompt and does not accept
`--yes`.

Generated effects are applied after the authored facts they depend on, then the
complete parent operation is verified. A generated planning failure blocks
before the first destination write. A generated application or verification
failure retains the verified recovery bundle and reports the complete update
plan's residual state; it does not restore earlier effects.

Each replacement uses the complete planned file bytes through a safe
same-directory replacement property. The operation does not edit the target in
place or fall back to a weaker write after an atomicity or identity check fails.
It never replaces bytes outside the intended destination or generated interior.

After each replacement, the effect target bytes are verified. After all effects,
the operation verifies the requested field states, the authored-body protection
or Template-body-copy decision, source-type validity, compatibility filename
preservation, and the complete generated projection.

When the protected-Template condition is complete, the operation preserves the
authored body byte-for-byte and forms `attention` after the selected metadata
and generated effects have completed and verified. A Template-only request may
have no replacement effect, but retains verified byte-level no-op facts and the
same `attention` status because the explicit Template body was not applied.

## Safety And Recovery

### Recovery-bundle boundary and no-ops

A verified no-op has no affected mutation path and therefore does not require a
bundle. The command does not inspect or report repository state.
When the plan contains an existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. It prepares exactly one immutable ZIP bundle
outside the workspace. An operation containing only Create effects or
no-ops creates no bundle. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to the deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires that preparation for every existing-target effect and
performs one final effect per target. All preparation completes before the first
target effect; unknown, malformed, mismatched, or colliding bundles block.

### Revalidation and verification

Immediately before application, the operation rechecks every target, source,
Template, route, generated fact, and other expected-state fact in the complete
plan. Volatile target facts are rechecked immediately before each replacement.
The operation does not allow a stale plan to replace a changed target.

The operation verifies each applied effect and then verifies the complete
semantic postcondition. Generated navigation is rebuilt from the authoritative
post-update topology and metadata projection, not from an assumed previous
generated body.

### Failure, interruption, and rerun

When application or verification fails, the operation stops new effects and
reports the actual residual draft or final path; a valid final remains when
failure occurs after preparation. It never restores, reverses, or compensates
for an earlier effect. An unexpected concurrent edit is preserved and reported
as residual state rather than overwritten.

A failed operation remains `failed` because the requested update did not
complete. Any unexpected failure after a write, including application or
verification failure, is `failed`, not `attention`. An interruption remains
`interrupted` when no unexpected application or verification failure changes the
result. The result reports ordinary effect facts and the actual residual draft
or final path without a recovery-derived current-target classification.

The operation does not replay a saved plan. It computes a fresh plan from current
facts when rerun and converges when the remaining state is safe. A closed final
ZIP may remain after abrupt process termination, without an executable crash or
power-loss guarantee. Recovery provenance does not classify current target
state. After final verification, effects remain successful if bundle deletion
fails; the result is `attention` with the exact residual path and cleanup
guidance. Cleanup owns exact named final and draft deletion under its separate
lease-bound contract.

## Presentation Relationship

The operation forms one typed result after invalid or blocked resolution, safe but
incomplete coverage, dry-run preflight, application verification, recovery,
interruption, or verified no-op.
Human and structured renderers consume that same result. They do not rerun
resolution, planning, application, or verification and do not reinterpret its
semantic status.

The [Interface Contract](interface.md) owns the exact human blocks, compact and
expanded content, structured facts, accepted stream allocations, and semantic
result meanings. Behavior supplies the result evidence for workspace, selection
method, and target identity, field state, Template decision, preservation,
completeness and safety, intended effects, exact preview effects, preflight,
recovery-bundle, application, verification, recovery, residual targets, observations,
availability conditions, and at most one required `Next:` action without defining
another output shape. JSON uses the one complete stdout result and bounded stderr
diagnostics required by the Interface.

## Conformance Evidence

The public obligations in [Interface Verification](interface.md#verification)
remain in force. Behavioral evidence must prove the following without selecting
one parser, filesystem API, recovery-bundle filename, lock strategy, or source-module
boundary:

- Exact source-ID, base-path, and overwrite-path resolution, logical base-only
  mutation, canonical and compatibility entrypoint preservation, unsupported
  target rejection, detached-file rejection, orphan handling, collisions, and
  ambiguous route blocking.
- Exact scoped-frontmatter boundary detection, supported-field addition,
  unrelated-metadata preservation, malformed and duplicate boundary blocking,
  field omission, replacement, exact-empty responsibility removal, required
  metadata protection, tag ordering, duplicate rejection, and invalid values.
- Singleton rejection for repeated `--description`, `--responsibility`, and
  `--template` values, accepted complete tag-list replacement, idempotent
  Boolean repetition, and unchanged shared-global repetition and composition
  without last-wins or precedence behavior.
- Template source classification, ID and exact-path selection, collision and
  overwrite blocking, frontmatter stripping, no-substitution behavior, no
  provenance or continuing ownership, frontmatter-only whitespace insertion,
  ordinary-file validity, entrypoint validity, and authored-body protection.
- Byte-for-byte preservation of authored titles, prose, links, whitespace, line
  endings, generated markers, and generated interiors whenever the body is
  protected, including prose, headings, comments, markers, and whitespace
  variants.
- Template-only protected-body byte-level no-op formation with `attention` status,
  metadata application alongside protection, exact intended-state comparison,
  unchanged-field observations, absent-responsibility removal observations, and
  unchanged generated projections without synthetic effects.
- Authoritative post-update generated projection, direct parent effects,
  entrypoint self-region completion, dependency-minimal target selection,
  ordering, generated-boundary validation, compatibility destinations, and
  preservation of bytes outside generated interiors.
- Complete plan formation before effects, one-blocker all-effects blocking,
  same-physical-file coalescing, exact dry-run parity and bounded diffs, and no
  persistent dry-run artifacts. Dry-run and apply must share request, facts,
  intended bytes, generated projection, plan, preflight, status, and full effect
  evidence, while dry-run writes nothing.
- All seven statuses, including safe-but-incomplete coverage with no writes,
  unsafe or ambiguous blocked boundaries, post-write failed behavior, ordinary
  complete changes and no-ops, and the sole protected-Template attention
  condition. Planned changes alone must not form `attention`.
- Verified no-op behavior before recovery-bundle preparation; external bundle
  storage, semantic final-ZIP verification, unknown/mismatched artifact
  protection, collision handling, cleanup attention, and exact named lease-bound
  Cleanup.
- Expected-state revalidation before application and before each replacement,
  safe same-directory replacement, no weaker fallback, per-effect verification,
  complete semantic verification, retained partial state without restoration,
  concurrent-edit preservation, residual evidence, interruption, and fresh-plan
  rerun convergence.
- Formation of one typed result and rendering of human and structured output
  from that result without a second operation run. Human complete, attention,
  and incomplete results use stdout; invalid, blocked, failed, and interrupted
  results use stderr. JSON uses stdout for every status and bounded diagnostics
  on stderr, with compact identity, mode, status, completeness, safety, field,
  Template, protection, path, effect, exact-preview, and at-most-one-Next
  evidence. No result proposes overwriting authored body content.

Direct tests should prove request resolution, field-patch semantics, Template
classification and body decisions, byte preservation, no-op formation,
authoritative generated projection, plan ordering, dry-run parity, effect
boundaries, statuses, stream allocation, compact next-action limits, and
recovery conditions. Focused integration tests should
use real temporary routed workspaces, ordinary and compatibility entrypoints,
overwrite pairs and orphans, external recovery bundles,
filesystem failures, expected-state changes, concurrent edits, and generated
parent effects. Gate 5 executable proof should prove command parsing, exact
dry-run output, human and structured rendering from one result, the Architecture's
status-preserving process completion, Native AOT execution, and packaged
execution without adding another command-local implementation choice.

## Related Current Sources

- [route update Interface Contract](interface.md)
- [route update Command Contract Set](_update.md)
- [CLI Architecture](../../../architecture.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [Context Behavior Contract](../../context/behavior.md)
- [Status Behavior Contract](../../status/behavior.md)
- [Route Init Behavior Contract](../init/behavior.md)
- [Route Create Behavior Contract](../create/behavior.md)
- [CLI Command Contract Set — Behavior Contract](../../../command-contract-set.md#behavior-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Contract Document Templates](../../../../../../../templates/cli/documents/_documents.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
- [Canonical Markdown Syntax](../../../../framework/markdown/syntax.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Loading And Continuity](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Templates](../../../../framework/primitives/templates.md)
