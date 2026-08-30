---
open-forge:
  description: Accepted technology-neutral install behavior for management establishment, exact no-op, initial force, and recovery
  responsibility: Define how install resolves Framework facts, rejects managed divergence, plans one safe establishment, and forms its result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Behavior, Determinism, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Install Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for the non-shipping
root `open-forge install` operation. It defines technology-neutral request
resolution, exact workspace and payload facts, management classification,
semantic identity, intended state, generated projection, complete planning,
preflight, dry-run and application, verification, lifecycle publication,
recovery, result formation, and conformance.

The [Interface Contract](interface.md) defines public syntax, states, output,
status vocabulary, errors, examples, and non-goals. Shared Global Flags and the
Framework, routing, maintenance, and Index contracts define their respective
meanings. The accepted CLI Architecture defines the exact lifecycle serialization,
shared JSON result schema, exit mapping, and implementation mechanics. This file
does not duplicate those mechanics or claim their Gate 5 proof.

## Operation Flow And Invariants

Install follows one complete typed flow:

```text
validated command input
  -> exact workspace and embedded Framework source
  -> recognized footprint and lifecycle trust facts
  -> baseline/current/intended comparison
  -> authoritative generated-navigation projection
  -> complete ordered establishment plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> lifecycle publication and recovery-disposition reporting
  -> one typed result
  -> human or structured rendering
```

No effect begins until the complete footprint, current facts, management state,
intended state, generated projection, expected bytes, recovery-bundle identity
and preparation readiness, verification conditions, and preservation conditions
are known. One
blocked, incomplete, ambiguous, or unsafe selected effect blocks the whole
plan. Install never applies a safe subset around a blocked target.

For unchanged payload, workspace bytes, explicit input, and relevant external
facts, the operation resolves the same classification, plan, status, and result.
Filesystem enumeration order, matching bytes, route names, tags, provider
resemblance, and current generated lines are never hidden selection inputs.

## Request Resolution

The resolver:

1. Parses direct root `install` and rejects operands, a Framework group, root
   aliases, `--prune`, replacement/reinstall forms, and other unaccepted flags.
2. Resolves shared terminal `--help` and `--version` before workspace or domain
   work. Command-specific input combined with a terminal mode is invalid.
3. Collapses repeated `--force`, `--automatic`, and `--dry-run` presence to one
   Boolean each. No occurrence wins by order.
4. Resolves the exact current directory or exact `--workspace` value through the
   shared contract. It does not discover another root.
5. Preserves independent dimensions: automatic does not set force, and dry-run
   does not remove authority from the plan it previews.

A human application that would write may continue without `--automatic` only
when standard input and the prompt stream on standard error are both terminal-
capable. After the complete plan and preflight succeed, it asks exactly once
before acquiring the workspace lease or beginning an effect. Confirmation
continues the already formed plan. Refusal, end of input, or caller cancellation
returns `interrupted` and writes nothing.

Dry-run, verified no-op, `--automatic`, JSON, and non-prompt-capable requests
never prompt. A non-prompt-capable human application that would write is
`invalid` unless `--automatic` is explicit and directs the caller to rerun that
same command with `--automatic`. Automatic never supplies force or bypasses a
safety boundary. Decorative prompt wording is not contract meaning.

## Exact Workspace And Payload

Workspace resolution establishes the selected directory, lexical containment,
physical identity, and access required by this operation. Missing, unavailable,
non-directory, escaping, aliased, or otherwise unsafe boundaries return
`blocked` before lifecycle work. The resolver never substitutes a Git root,
package root, marker location, nested `.agents`, or nearby source tree.

The source resolver admits only the embedded current Framework payload. It
validates current destination identity, supported file kinds, source identity,
and containment. Safely unavailable payload coverage is `incomplete`; malformed,
ambiguous, or unsafe source identity is `blocked`. Force cannot make an
unavailable or unsafe source usable.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof establishes distributed source
identity only; it is not workspace or runtime implementation evidence.

Resolve that inventory through the CLI Architecture's neutral Framework
distribution capability over ordinary .NET embedded resources. Runtime never
reads the repository source tree.

The closed current-fact universe includes:

- exact current payload destinations below `.agents`;
- the canonical `AGENTS.md` managed region;
- the supported Claude `CLAUDE.md` bridge region;
- the exact `.agents/open-forge.lifecycle.json` document and its `framework`
  section;
- current authored topology and metadata needed for affected generated regions;
  and
- recognized recovery-bundle provenance, kept outside the lifecycle document.

It excludes arbitrary providers, package sources, Extension payloads, overwrite
companions as Framework targets, retired-only paths, files outside the exact
lifecycle document, and the repository `.temp/` directory.

The selected fact universe is the closed base Install subset. The trusted
Framework lifecycle section may additionally contain scoped managed targets and
generated regions from Framework-aware Route Init. Install validates and
preserves those records and their current identities but does not select them as
root effects or classify their mere presence as root divergence.

## Lifecycle Document And Trust

The only new-CLI lifecycle document is `.agents/open-forge.lifecycle.json`, schema
v1. It contains a common envelope and isolated `framework` and `extensions`
sections. The envelope may identify document version, fingerprint policy, exact
workspace binding, and section presence. The document does not store a plan,
runtime history, journal, recovery evidence, or session.

Install reads and writes only the `framework` section for Framework management.
It preserves the unrelated `extensions` section and common-envelope meaning
semantically. When the selected lifecycle meaning changes, it source-generates
one deterministic canonical UTF-8 whole-document representation, so lifecycle
property order, whitespace, and line endings may be normalized. A semantic
no-op writes nothing. When an existing target is replaced, prior bytes remain
recoverable through the verified external recovery bundle described under
Application, Verification, and Recovery; the CLI does not inspect or report
repository state or claim history evidence. If
unrelated state cannot be parsed, preserved semantically, round-tripped, or
verified, a mutation is `incomplete` or `blocked` and writes nothing. It never
drops, repairs, or rewrites opaque malformed state as a side effect. Cross-section
path or owner collisions block preflight.

The Framework section is trusted only when schema v1 and its fingerprint policy
are supported, the workspace and target identities are exact, internal
consistency is intact, and coverage is complete and verifiable. A safely absent
section is established only after complete inspection proves that no expected
managed state, managed boundary, or recovery residual exists. An absent document
or section is not, by itself, proof of unmanaged state. A missing expected
section is not treated as an empty section. A malformed, unsupported,
unverifiable, or inconsistent section cannot be promoted by a matching path,
fingerprint, source, or `--force`.

Safe unavailable lifecycle coverage is `incomplete`; unsafe or ambiguous
lifecycle identity is `blocked`. Neither state grants management or replacement
authority.

Every target record contains required nullable `sourceAssetPath`. Validate a
non-null value as a normalized canonical embedded asset-relative path even when
the current inventory no longer contains that historical asset. Require
non-null provenance for payload files and managed root/provider blocks and
`null` only for derived generated-region targets. When publishing a new or
refreshed target, verify every non-null value against the exact inventory being
recorded. User-owned scope entrypoints never enter the Framework target set.

## Semantic Fingerprints And Current Bytes

For supported parseable Markdown and frontmatter kinds, the operation uses the
`open-forge-markdown-v1` conservative parser/AST-derived, syntax-aware semantic
fingerprint. It:

- preserves Unicode and semantic text without blanket ASCII conversion,
  Unicode loss, case folding, or unsupported normalization;
- preserves headings, tags, links and destinations, marker meaning, inline text,
  code-block content, and semantically significant whitespace;
- normalizes line endings and only parser-proven formatting trivia;
- excludes derived generated `Entries` interiors from authored identity while
  retaining marker and projection boundaries; and
- fails closed when syntax or equivalence is unsupported or ambiguous.

Unsupported, binary, and unparseable kinds use exact-byte managed identity. For
supported parseable kinds, only semantic baseline fingerprints are persisted.
There is no persistent exact-byte baseline digest in the lifecycle document.

Every invocation captures current exact bytes freshly for the plan, bounded diff,
expected-state revalidation, bundle payload, write verification, and recovery.
If
current, baseline, and intended semantic fingerprints are equal while exact
bytes differ only in parser-proven formatting trivia, the operation reports a
formatting-only observation and does not treat it as divergence or rewrite it
under install.

The accepted conservative formatter direction permits advice only. Install does
not execute a formatter or persist formatter state. Advice never grants authority
or changes the plan.

## Management Classification

The classifier evaluates every recognized target and managed region together:

### Safely absent

Normal planning may establish management only when complete inspection proves
all four facts:

1. No Framework lifecycle claim exists at the recognized lifecycle boundary.
2. No exact current payload destination is occupied.
3. No canonical `AGENTS.md` or supported `CLAUDE.md` managed block exists.
4. No recognized Framework recovery or residual evidence exists.

Existing hosts without matching markers and unrelated user-owned `.agents`
content do not alone defeat this state. An unavailable absence fact is
`incomplete`; partial, malformed, ambiguous, colliding, or occupied evidence is
`blocked` when unsafe. Neither writes.

### Trusted exact managed state

The section's trusted Framework identity, current semantic fingerprints, and
current source are exact for the closed base Install subset. Install forms a
verified no-op and preserves every other trusted scoped target and generated
region. It does not invent a write to normalize timestamps, formatting,
provenance, or unrelated bytes. `--force` and `--automatic` do not change the
no-op.

### Managed divergence

A trusted managed target or region is divergent when its semantic fingerprint is
changed from baseline, an expected path is missing, a trusted retired fact is
present, or the managed source identity is not the embedded current Framework.
Install does not reconcile any of these states. It forms no mutation plan,
returns `blocked`, preserves current bytes, and directs the caller to
`open-forge update`. The same result applies when `--force` is present. Initial
force is not managed-update authority.

### Eligible initial occupant

An exact current payload destination or supported managed block may be an
eligible initial occupant only when no trusted lifecycle owner or competing
manager claims it and all route, source, physical-identity, containment, marker,
bundle identity and recovery-bundle facts are safe. A known user-owned or Extension-owned
path, route collision, ambiguous managed block, unknown path, or unsafe boundary
is not eligible.

Normal install blocks an eligible occupant without writing. Explicit force may
replace only the exact recognized occupant, never its surrounding host bytes,
and establishes management from the new current source after complete
verification. Previous occupant bytes are not adopted as baseline history.

## Intended State And Generated Projection

For a safe establishment or eligible force request, the planner first forms one
hypothetical post-install workspace from current authored content and the effects
permitted by the request. It preserves user routes, Memory, overwrite
companions, intentionally absent defaults, Extension content, and all bytes
outside exact managed regions.

The generated-navigation projector then uses the current Index rules to derive
every affected `Entries` body from that hypothetical authored topology and
metadata. It does not use current generated lines as topology or metadata and
does not copy generated interiors from the embedded payload. A lifecycle plan
cannot invoke a hidden `index` operation.

Only one valid final generated region with one complete ordered marker pair may
change. Missing, duplicate, reversed, nested, misplaced, or otherwise ambiguous
markers block before any write. Markers and all bytes outside the generated
interior remain unchanged.

Root and provider resolution admits only an absent host where creation is
supported, an existing host with no markers for bounded append, or one complete
ordered marker pair for bounded replacement. It rejects malformed marker
topology and never replaces host bytes outside the managed block.

## Complete Plan And Preflight

The one ordered plan records, for every effect:

- exact logical and physical target identity and containment;
- creation or eligible initial replacement kind;
- complete expected current and intended bytes or bounded interiors;
- current expected-state and revalidation conditions;
- generated projection and lifecycle-section effects;
- recovery-bundle readiness, identity, and collision facts;
- per-effect and whole-operation verification;
- exact bundle identity, provenance, success-removal, and residual-reporting
  facts.

Preflight validates all source, target, route, ownership, containment, marker,
cross-section, expected-state, recovery-bundle, verification, and preservation
facts. Every planned existing-target effect (`Replace` or
`ReplaceGeneratedRegion`) must be covered by one
verified bundle preparation; a `Create` or semantic/byte no-op has none. A
verified no-op has no mutation path and needs no bundle.

Before the first workspace effect, the implementation obtains the actual OS lock
for the persistent, reusable `.agents/open-forge.lock` path defined by the
accepted CLI Architecture. Existing bytes are preserved. The operation holds a
`FileShare.None` handle only and never writes metadata, deletes, or truncates the
lock file. An active handle blocks the plan; lock state is not lifecycle
authority, history, or recovery evidence.

`--automatic` admits only safe absent creation or exact no-op effects already
selected by the explicit operation. It cannot admit an eligible initial occupant
without explicit `--force`. It never admits divergence, deletion, adoption,
ownership, or marker repair.

## Dry-Run Parity

Dry-run uses the same normalized request, fresh current facts, lifecycle trust,
classification, intended state, generated projection, complete plan, and
preflight as application. It reports all safe creations, eligible force effects,
bounded generated and managed-region changes, lifecycle publication that would
occur after verification, preserved content, and recovery readiness.

It stops before directory, file, lifecycle, recovery-bundle, temporary, formatter,
or other persistent effects. It does not claim application, verification,
lifecycle publication, or bundle-handling success. It forms the same pre-effect status
as the corresponding application request. Because dry-run performs no effects,
it never produces an apply-time `failed` or `interrupted` result. A planning or
read failure and caller cancellation before effects retain their own event
meaning.

## Application, Verification, And Recovery

When application is selected:

1. If this is a prompt-capable human application that would write, ask the one
   confirmation after complete preflight. A refusal, end of input, or caller
   cancellation stops with no effects.
2. Acquire the persistent workspace lease. If the accepted plan starts without
   `.agents`, expose that exact directory as the one planned lock-bootstrap
   effect; immediately confirm it is missing, create and verify it through
   `WorkspaceLockManager`, then open `.agents/open-forge.lock`. Cancellation
   before bootstrap creates nothing.
3. Revalidate the complete plan and all volatile source, target, ownership,
   containment, marker, section, and expected-state facts.
4. Prepare and verify the one complete external recovery bundle when the plan
   contains an existing-target effect. Complete preparation before any workspace
   effect.
5. Apply every other explicitly planned missing directory parent-first through
   the shared directory capability. Each is a descendant below `.agents`.
   Immediately revalidate each missing target and its exact contained physical
   parent, call ordinary `Directory.CreateDirectory`, then verify the exact
   resulting contained ordinary directory.
6. Revalidate each file or bounded-region target immediately before its effect.
7. Apply complete planned file or bounded-region bytes through the accepted safe
   replacement property. Do not edit in place or weaken the property after a
   check fails.
8. Verify each payload, root/provider, generated-region, and lifecycle effect.
9. Rebuild and verify the complete recognized Framework result and preservation
   boundaries as one operation.
10. Publish or refresh only the Framework lifecycle facts established by the
   complete verified result, preserving the unrelated lifecycle section
   semantically. A selected lifecycle semantic change is source-generated as one
   deterministic canonical UTF-8 whole-document representation; formatting,
   ordering, and line-ending trivia may be normalized. A semantic no-op publishes
   no lifecycle write. Prior bytes remain retained in the verified operation
   bundle.
11. After final verification, delete only the positively recognized bundle
   created for this operation. `Deleted`/`Removed` permits normal completion.
   `Failed`/positively observed `Retained` keeps target effects successful and
   produces `attention`, the exact residual path,
   and cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the
   deletion result provides one.

Every `WorkspaceLockResult` preserves the nullable bootstrap outcome already
reached. `Existing` records a validated pre-existing `.agents`, `Materialized`
records observed absence followed by attempted BCL creation and validation, and
`null` means neither outcome was successfully observed. Acquisition requires a
non-null outcome; later failure or cancellation does not erase one.

Install has no target deletion effect. Before any existing byte or bounded region
is replaced, orchestration selects only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is
pre-effect `incomplete`. For an operation with one or more existing replacement
targets, it creates one immutable ZIP bundle outside the
workspace for the complete operation. Its deterministic external directory key
and final name use the normalized physical workspace path and operation ID. An
operation containing only creates or no-ops creates no bundle. A source-generated schema-v1
`manifest.json` and streamed ordinal payload entries record operation and
normalized physical-workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
The draft uses `CreateNew` under its exact name, is closed and reopened for
semantic manifest, exact ordered entry, length, hash, and payload-byte
validation, moved within the same directory to the deterministic final name,
and reopened and verified again. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. Every planned
existing-target effect must match the preparation; Create and no-op effects create
no bundle. All preparation is complete before the first target effect.

Directory creation remains a distinct effect from file Create/Replace
and has no recovery entry. The missing `.agents` bootstrap is the sole pre-lease
directory effect and never enters the descendant applier. A directory created or
bootstrapped by this operation is retained and reported as residual state after
lock contention, later failure, or interruption. Install never rolls it back,
compensates for it, or removes it. The shared capability uses ordinary BCL
filesystem behavior; it adds no P/Invoke, recovery protocol, or hostile same-user
creator-identity guarantee. `LocalApplicationData` remains recovery-bundle
storage and is never used for the workspace lock.

Before post-verification deletion begins, an application, verification,
lifecycle-publication, or cancellation outcome stops new effects and reports
the actual residual draft or final path; a valid final remains when preparation
completed. The foundation never restores a target automatically or derives
current target state from recovery provenance. A closed final ZIP may remain
after abrupt process termination, without an executable crash or power-loss
guarantee. An unexpected concurrent edit is preserved and reported as residual
state. An unsafe residual is `failed`; caller cancellation is `interrupted` only
when no stronger failure remains. Cleanup owns exact named final and draft
deletion under its separate lease-bound contract. A later
invocation forms a fresh plan and never replays a saved plan, receipt, journal,
history, or progress record.

## Result Formation And Streams

The operation forms one typed result after invalid input, classification,
preflight, dry-run, verified application, interruption, or recovery. Human and
JSON renderers consume that result and do not rerun lifecycle work.

The typed result forms exactly the ordered command-local JSON graph frozen by
the Interface: mode, force, automatic, atomic nullable embedded-source identity,
atomic nullable destination classification, atomic nullable managed-footprint
counts, exact ordered effects, lifecycle, recovery, verification, and ordered
findings. All top-level properties are present for every status, arrays are
non-null, and the shared envelope's command, status, workspace, and next action
are not duplicated. Effect residual state remains the typed value `none`,
`retained`, or `unknown`, so dry-run, retained state, partial failure, and
uncertain completion cannot be collapsed into a Boolean.

The result retains exact workspace and selection method, normalized flags, source
identity, recognized footprint, trust and management classification, semantic
baseline/current/intended facts, generated projection, effects, preserved
content, lifecycle publication, recovery-bundle facts, verification, residuals,
and at most
one next action.

Use the Interface status meanings and ordinary precedence `blocked` >
`incomplete` > `attention` > `complete`. `complete` includes safe application,
eligible force, dry-run, and exact no-op. Ordinary managed divergence is
`blocked`, not `attention`, because install does not own update authority.
Planned effects, format-only facts, force presence, automatic mode, and managed
divergence do not form `attention`; managed divergence directs the caller to
`update`. Post-verification recovery deletion `Failed` with positively observed
disposition `Retained` is the only current install `attention` condition.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. JSON emits one complete result to stdout for every semantic status, and
bounded diagnostics use stderr. No status claims a runtime implementation or
shipping evidence.

## Behavioral Conformance

A conforming implementation must demonstrate:

- exact request normalization, terminal handling, Boolean repetition, and workspace
  selection without discovery;
- closed embedded-payload footprint and rejection of arbitrary providers,
  operands, route coincidence, tags, and matching-byte ownership inference;
- source/payload set and byte parity plus published Native AOT resource access
  away from the checkout;
- all four safe-absence facts, trusted exact no-op, managed-divergence block with
  `update` next action, eligible initial occupant, and force-only initial
  replacement;
- exact schema-v1 lifecycle-document isolation, absent, untrusted, and missing
  states, source-unavailable facts, and unsupported or ambiguous schema handling;
- required nullable per-target `sourceAssetPath`, current-inventory publication
  validation, generated-region `null`, and preservation of trusted scoped targets
  outside the base Install subset;
- syntax-aware semantic fingerprints, exact operation-time bytes, format-only
  observations, generated-interior exclusion, and fail-closed equivalence;
- one intended topology and current Index projection;
- bounded generated and root/provider markers; no hidden subprocess;
- complete preflight, external schema-v1 recovery-bundle preparation and verification,
  exact prior-byte preservation, expected-state revalidation, per-effect and
  whole-operation verification, typed post-verification deletion
  state/disposition facts, residual reporting, and fresh rerun;
- the exact one-prompt matrix, no-write refusal/end-of-input/cancellation,
  direct automatic rerun guidance for non-prompt-capable human writes, and no
  prompt in dry-run, no-op, automatic, JSON, or redirected modes;
- separate parent-first directory effects under the held workspace lease, with
  immediate missing-target and physical-parent revalidation, ordinary BCL
  creation, post-verification, retained residuals, and no rollback,
  compensation, removal, or recovery entry;
- the exact visible missing-`.agents` bootstrap before lease acquisition,
  including planning/reporting, immediate verification, pre-bootstrap
  cancellation, post-bootstrap contention, retained residuals, and exclusion
  from the descendant applier; plus nullable `WorkspaceLockResult` outcome
  retention for acquired, failed, and cancelled results;
- dry-run/application parity with no persistent dry-run effects;
- seven statuses, including `Failed`/positively observed `Retained` recovery
  `attention` and `Failed`/`Unknown` recovery `failed`, streams, one typed result,
  JSON stdout, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state;
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys;
- no runtime implementation or shipping claim.
