---
open-forge:
  description: Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route update`
  responsibility: Define how a conforming implementation resolves one routed-source update and forms, applies, verifies, or recovers its complete result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Update, Behavior, Metadata, Template, Mutation, CurrentTruth]
---

# route update Behavior Contract

## Status And Authority

This is the accepted current Crystallized authority for the technology-neutral
Behavior Contract behind `route update`. The command is implemented in the
merged native CLI; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The [Interface Contract](interface.md) defines the complete public syntax,
inputs, observable output, semantic result names, errors, non-goals, and public
examples that this Behavior satisfies. This file defines deterministic
resolution, current facts, result formation, effects, safety, recovery, and
conformance. It does not add operands, flags, aliases, output fields, semantic
results, or other public meaning. Request normalization, status formation, and
presentation follow the accepted repetition, status, dry-run, stream, and
native-report rules in the Interface Contract.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared result schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines System.CommandLine binding, fixed
Markdig and source-generated serialization relationships, the BCL-first
filesystem, workspace-lock and recovery boundaries, evidence, runtime, Native
AOT, and source layout. This Behavior Contract remains technology-neutral within
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
retains `completed-with-warnings` while its explicit body intent remains unapplied; other
successful repeated updates are `completed` no-ops.

The operation is a field patch rather than whole-file replacement. It changes
the base file of the resolved logical source, never its overwrite companion,
and protects an authored target body byte-for-byte. Generated navigation is a
separate bounded dependency effect governed by the complete [Index Behavior Contract](../../index-candidate/behavior.md)
contract.

No persistent effect begins until the complete target, Template decision,
current facts, intended destination, generated projection, ordered plan, and
preflight are established. One blocker prevents every effect. There is no
partial-application or best-effort path.

The operation consumes the neutral no-follow logical-leaf guard for the base
target and every generated-region target. The guard inspects each logical final
leaf before ordinary physical resolution, at initial preflight, during
under-lease revalidation, and immediately before its effect. A present link,
reparse point, or special final leaf is `blocked`; Route Update never follows,
writes, or deletes a Library projection, and this decision does not depend on a
Library record.

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

Before resolving ordinary physical identity, the resolver obtains the no-follow
observation of the selected final leaf. A link, reparse point, or special final
leaf cannot become an ordinary Markdown or entrypoint target through its
resolved destination. Stable contained directory-link ancestry remains governed
by the ordinary filesystem path contract.

### Frontmatter and field patch

The resolver establishes exactly one safely parseable scoped `open-forge`
frontmatter block for the base target. If its `open-forge` value is an existing
safely parsed canonical `open-forge: {}` mapping, the narrow enrichment path admits only a request
with a complete non-empty description and at least one valid tag, with
responsibility following the existing set and exact-empty removal rules. A description-only or tags-only request
against that empty mapping is invalid. The resolver does not guess a missing
ownership boundary, choose among duplicate blocks, or continue through a
malformed boundary. The complete intended source must remain valid for its
ordinary-file or entrypoint representation.

The field patch is resolved as a set of explicitly supplied operations. Omitted
fields retain their current values. Supplied description, responsibility, and
tag values follow the replacement, addition, exact-empty removal, validation,
and required-metadata rules in [Metadata Patch](interface.md#metadata-patch).
For the empty-map enrichment path, the complete supplied metadata is formed by
an exact span patch that preserves unrelated YAML, body bytes, line endings,
and encoding. It rejects arbitrary non-empty flow mappings, aliases, duplicate
ownership maps, whole-file rewrites, and any partial enrichment request.
The resolver preserves unrelated top-level and unsupported scoped metadata when
safe preservation can be established. It does not derive, summarize, correct,
or judge field meaning from filenames, bodies, Templates, generated entries, or
overwrite companions.

An invocation with no metadata operation and no Template operation forms the
public `invalid-input` result. Invalid field syntax, invalid tags, required-metadata
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
  entrypoint structure when applicable. The facts include whether the scoped
  `open-forge` value is the safely parsed canonical `open-forge: {}` mapping eligible for narrow
  enrichment, its exact metadata span, and the surrounding bytes, line-ending,
  and encoding facts needed for preservation.
- The current destination metadata and the exact field operations requested by
  the invocation.
- The selected Template's identity, route, source classification, overwrite
  relationship, frontmatter boundary, body bytes, and valid intended use when a
  Template is supplied.
- The exposing parent and any target entrypoint generated region whose direct
  projection can change, including the target's own generated region when
  eligible Template completion establishes one.
- No-follow final-leaf observations for the base target and every planned
  generated-region destination.
- The current authoritative routed topology, authored descriptions and tags,
  generated boundaries, sibling projections, route identities, and actual
  compatibility destinations needed by the [Index Behavior Contract](../../index-candidate/behavior.md)
  projection.
- The expected current bytes, recovery-bundle readiness, and volatile target
  facts needed for preflight and later revalidation.

For an eligible empty mapping, current facts also prove that the requested
description and tag list are complete and valid before any exact-span patch is
formed. A non-empty flow mapping, alias, duplicate ownership map, malformed
boundary, or ambiguous layout is not treated as an enrichment candidate.

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
technology choices. They must satisfy their linked current authorities and the
observable requirements in this contract:

- Exact YAML and Markdown parser behavior, compatibility parsing, frontmatter
  preservation mechanics, line endings, encoding, heading and link handling,
  and canonical serialization.
- Exact filesystem APIs, physical identity, symlink and junction behavior, case
  and Unicode rules, containment implementation, and test seams follow the [CLI
  Architecture](../../../architecture.md). Exact atomic-file mechanics follow the
  [Mutation And Recovery Technical
  Design](../../../technical-designs/mutation-and-recovery.md).
- The neutral no-follow final-leaf observation is a Framework filesystem fact.
  Route Update consumes it at every stated lifecycle boundary and does not add
  a Library-specific ownership lookup or a link-following fallback.
- Exact recovery-bundle filenames, collision-handling mechanics, and related
  recovery implementation follow the Mutation And Recovery Technical Design.
- Lock scope, stale-lock handling, and broader cross-platform concurrency
  mechanics. Expected-state revalidation and preservation of unexpected
  concurrent edits remain current safety requirements. Exact lock mechanics
  follow the Mutation And Recovery Technical Design within the Architecture's
  accepted workspace-lock boundary.
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
required title, Axioms meaning, `Entries` section, and generated region of
an entrypoint. Its bytes are not normalized beyond the bounded representation
needed by the requested field patch or eligible body completion.

When the empty-map enrichment path is selected, the intended bytes are formed
by replacing only the exact scoped metadata span with the complete supplied
fields. Unrelated YAML and body bytes, line endings, and encoding remain
unchanged. The operation does not serialize the whole document, accept a
non-empty flow mapping or alias as an empty map, merge duplicate ownership
maps, or treat partial enrichment as valid.

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
status is `completed-with-warnings` because the explicit Template intent was not applied.
The public semantic conditions for `completed`, `completed-with-warnings`, `incomplete`,
`invalid-input`, `blocked`, `failed`, and `cancelled` remain exactly those in
[Semantic Results](interface.md#semantic-results); Behavior does not define
another status or an exit mapping.

The status formation rules are deterministic:

- `completed` is formed for a complete safe dry-run plan or a completed and
  verified application, including ordinary changes and all other verified
  no-ops, when the protected-Template `completed-with-warnings` condition does not apply.
  Planned changes alone do not create `completed-with-warnings`.
- `completed-with-warnings` is formed for the complete protected-Template condition above or
  post-verification recovery deletion `Failed`/positively observed `Retained`
  after verified target effects. In apply mode, requested metadata and generated effects complete and verify when
  present. In dry-run, only the protected-Template condition can form
  `completed-with-warnings`, because recovery deletion does not run. A Template-only request
  may have no replacement effect and still forms `completed-with-warnings`.
- `incomplete` is formed when safe facts are available but required inspection or
  planning coverage cannot finish. No mutation begins.
- `invalid-input` is formed for input, field, Template-reference, repetition, or
  target-kind violations. `blocked` is formed for a valid request whose safe
  complete plan cannot be established because an unsafe or ambiguous boundary
  remains. Neither is converted to `completed-with-warnings`.
- `failed` is formed when an application or verification failure after effects
  begin, post-verification recovery deletion `Failed`/`Unknown`, or another
  unexpected failure after a write prevents completion. `cancelled` retains its cancellation meaning when
  no unexpected application or verification failure changes the result.

For ordinary operation conditions, status precedence is `blocked` > `incomplete`

> `completed-with-warnings` > `completed`. Invalid input stops before operation resolution and
> forms `invalid-input`. Failed and cancelled results retain their event meaning.

## Effects

### Complete planning and preflight

The operation resolves every target and dependency, establishes every expected
fact, forms every intended byte sequence, and completes the ordered plan before
the first persistent effect. A blocker in the destination, Template, route,
generated projection, recovery-bundle readiness, containment, or expected
state blocks the complete plan. Safe but unfinished inspection or planning
coverage forms `incomplete` before any effect. There is no partial-application or
best-effort mode.

Initial preflight inspects the final logical leaf of every destination effect
without following it. A present link, reparse point, or special final leaf
blocks the complete plan. This applies even when the leaf resolves to a
contained Markdown source or when a Library record is absent; Route Update does
not follow or write through that projection.

The plan preserves siblings, overwrite companions, compatibility filenames,
authored target body content, and all bytes outside planned generated interiors.
It coalesces compatible changes to one physical file rather than scheduling
competing writes.

### Dry-run parity

Dry-run and application use the same normalized request, target resolution,
current facts, field patch, Template decision, intended bytes, generated
projection, planner, expected-state facts, preflight, and semantic status.
Dry-run stops before recovery-bundle creation, temporary file creation, replacement,
formatting, or another persistent effect. It still performs the same no-follow
leaf observations as application.

Dry-run rendering exposes every exact intended destination and bounded generated
diff required by the complete plan, plus the body-protection observation when an
authored body prevents Template copying. It does not expose unrelated authored
bytes or private recovery material. It reports no persistent file changes and
does not claim verification of bytes that were not written. A protected Template
request has the same `completed-with-warnings` status and observation as application; planned
changes alone do not create `completed-with-warnings`.

### Application

When `--dry-run` is absent, the explicit target and patch request confirms only
the described metadata changes, eligible Template body completion, and planned
generated-region changes. The operation does not prompt and does not accept
`--yes`.

Generated effects are applied after the authored facts they depend on, then the
complete parent operation is verified. Before each authored or generated effect,
the operation repeats the no-follow final-leaf and expected-state checks.
A generated planning failure blocks before the first destination write. A
generated application or verification failure retains the verified recovery
bundle and reports the complete update plan's residual state; it does not
restore earlier effects.

Each replacement uses the complete planned file bytes through a safe
same-directory replacement property. The operation does not edit the target in
place or fall back to a weaker write after an atomicity or identity check fails.
It never replaces bytes outside the intended destination or generated interior.
For empty-map enrichment, those planned file bytes differ only at the exact
scoped metadata span; the operation never obtains them by whole-document
reserialization.

After each replacement, the effect target bytes are verified. After all effects,
the operation verifies the requested field states, the authored-body protection
or Template-body-copy decision, source-type validity, compatibility filename
preservation, and the complete generated projection.

When the protected-Template condition is complete, the operation preserves the
authored body byte-for-byte and forms `completed-with-warnings` after the selected metadata
and generated effects have completed and verified. A Template-only request may
have no replacement effect, but retains verified byte-level no-op facts and the
same `completed-with-warnings` status because the explicit Template body was not applied.

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
versioned `manifest.json` and streamed ordinal payload entries record
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

Immediately before application under the held lease, the operation rechecks
every target, source, Template, route, generated fact, and other expected-state
fact in the complete plan. It also repeats the no-follow observation of every
logical final leaf. Volatile target facts and that leaf observation are rechecked
immediately before each replacement. The operation does not allow a stale plan
or a link, reparse point, or special leaf to replace a changed target.

The operation verifies each applied effect and then verifies the complete
semantic postcondition. Generated navigation is rebuilt from the authoritative
post-update topology and metadata projection, not from an assumed previous
generated body.

### Failure, interruption, and rerun

Before post-verification deletion begins, an application, verification, or
cancellation outcome stops new effects and reports the actual residual draft or
final path; a valid final remains when preparation completed. It never restores,
reverses, or compensates for an earlier effect. An unexpected concurrent edit is
preserved and reported as residual state rather than overwritten.

A failed operation remains `failed` because the requested update did not
complete. Any unexpected failure after a write, including application or
verification failure, is `failed`, not `completed-with-warnings`. An interruption remains
`cancelled` when no unexpected application or verification failure changes the
result. For these pre-deletion outcomes, the result reports ordinary effect
facts and the actual residual draft or final path without a recovery-derived
current-target classification.

The operation does not replay a saved plan. It computes a fresh plan from current
facts when rerun and converges when the remaining state is safe. A closed final
ZIP may remain after abrupt process termination, without an executable crash or
power-loss guarantee. Recovery provenance does not classify current target
state. After final verification, `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery warning
coexists with the protected-Template condition, cleanup guidance owns the single
next action; the Template-protection facts remain visible evidence. Cleanup owns exact named
final and draft deletion under its separate lease-bound contract.

## Presentation Relationship

The operation forms one typed result after invalid or blocked resolution, safe but
incomplete coverage, dry-run preflight, application verification, recovery,
interruption, or verified no-op.
Human and structured renderers consume that same result. They do not rerun
resolution, planning, application, or verification and do not reinterpret its
semantic status.

The [Interface Contract](interface.md) owns the exact human blocks, minimal-detail and
full-detail content, structured facts, accepted stream allocations, and semantic
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
  The sole empty-mapping enrichment path requires complete description and tags
  (with the existing responsibility set/remove rules), patches one exact metadata
  span, preserves unrelated YAML/body bytes, line endings, and encoding, and
  rejects partial enrichment, non-empty flow mappings, aliases, duplicate
  ownership maps, and whole-file rewrites.
- Singleton rejection for repeated `--description`, `--responsibility`, and
  `--template` values, accepted complete tag-list replacement, idempotent
  Boolean repetition, and unchanged shared-global repetition and composition
  without last-wins or precedence behavior.
- Template source classification, ID and exact-path selection, collision and
  overwrite blocking, frontmatter stripping, no-substitution behavior, no
  provenance or continuing ownership, frontmatter-only whitespace insertion,
  ordinary-file validity, entrypoint validity, and authored-body protection.
- Byte-for-byte preservation of authored titles, prose, links, whitespace, line
  endings, generated section headings, and generated interiors whenever the body is
  protected, including prose, headings, comments, markers, and whitespace
  variants.
- Template-only protected-body byte-level no-op formation with `completed-with-warnings` status,
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
  unsafe or ambiguous blocked boundaries, post-write or `Failed`/`Unknown`
  recovery failed behavior, ordinary complete changes and no-ops, and the
  protected-Template and `Failed`/positively observed `Retained` recovery
  completed-with-warnings conditions. Planned changes alone must not form
  `completed-with-warnings`.
- Verified no-op behavior before recovery-bundle preparation; external bundle
  storage, semantic final-ZIP verification, unknown/mismatched artifact
  protection, collision handling, typed post-verification deletion
  state/disposition facts, and exact named lease-bound Cleanup.
- Expected-state revalidation before application and before each replacement,
  safe same-directory replacement, no weaker fallback, per-effect verification,
  complete semantic verification, retained partial state without restoration,
  concurrent-edit preservation, residual evidence, interruption, and fresh-plan
  rerun convergence.
- Formation of one typed result and rendering of human and structured output
  from that result without a second operation run. Human completed,
  completed-with-warnings, and incomplete results use stdout; invalid-input,
  blocked, failed, and cancelled
  results use stderr. JSON uses stdout for every status and bounded diagnostics
  on stderr, with minimal-detail identity, mode, status, completeness, safety, field,
  Template, protection, path, effect, exact-preview, and at-most-one-Next
  evidence. No result proposes overwriting authored body content.

Direct tests should prove request resolution, field-patch semantics, Template
classification and body decisions, byte preservation, no-op formation,
authoritative generated projection, plan ordering, dry-run parity, effect
boundaries, statuses, stream allocation, minimal-detail next-action limits, and
recovery conditions. Focused integration tests should
use real temporary routed workspaces, ordinary and compatibility entrypoints,
overwrite pairs and orphans, external recovery bundles,
filesystem failures, expected-state changes, concurrent edits, and generated
parent effects. Gate 5 executable proof should prove command parsing, exact
dry-run output, human and structured rendering from one result, the [Shared
Result Coordinates](../../shared/result-coordinates/interface.md)
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
- [Loading And Refreshing Context](../../../../framework/routing/loading.md)
- [Route Scope And Inheritance](../../../../framework/routing/scope.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Templates](../../../../framework/primitives/templates.md)
