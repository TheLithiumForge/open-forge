---
open-forge:
  description: Accepted technology-neutral install behavior for management establishment, exact no-op, initial force, and recovery
  responsibility: Define how install resolves Framework facts, rejects managed divergence, plans one safe establishment, and forms its result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Behavior, Determinism, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Install Behavior Contract

## Ownership Receipt Formation

Form whole-file and region receipts according to what the operation manages.
The `open-forge` blocks in root `AGENTS.md` and `CLAUDE.md` produce region
receipts even when creating a previously missing host requires a physical file
creation. Generated Entries produce `entries` region receipts. Preserve existing
verified ownership when a planned no-op leaves its bytes unchanged. Publish the
ownership lock after the operation's target effects verify; a region-only edit
never establishes whole-file ownership of its authored host. Skip an unavailable
lock write without blocking the operation.

The named installed content-file population is separate from the generated
`.agents/open-forge.lock.json` ownership control file and from the managed
host regions. `filesCreated` counts only the named installed content files.
`directoriesCreated` counts only directories strictly below `.agents`;
creating the `.agents` container is still the first ordinary directory effect
when needed, but it is excluded from that count.

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
meanings. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the shared JSON
result schema and exit mapping. The [Ownership And Source Alignment Technical
Design](../../technical-designs/lifecycle-provenance.md) defines ownership serialization and current source alignment, while the [CLI Architecture](../../architecture.md)
defines cross-cutting implementation structure. This file does not duplicate
those mechanics or claim their Gate 5 proof.

## Operation Flow And Invariants

Install follows one complete typed flow:

```text
validated command input
  -> exact workspace and embedded Framework source
  -> recognized footprint and ownership receipts
  -> current/intended comparison
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
returns `cancelled` and writes nothing.

Dry-run, verified no-op, `--automatic`, JSON, and non-prompt-capable requests
never prompt. A non-prompt-capable human application that would write is
`invalid-input` unless `--automatic` is explicit and directs the caller to rerun that
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

- named installed content destinations below `.agents`;
- the generated `.agents/open-forge.lock.json` ownership control file as a
  separate state-file population;
- the canonical `AGENTS.md` managed region;
- the supported Claude `CLAUDE.md` bridge region;
- current authored topology and metadata needed for affected generated regions;
  and
- recognized recovery-bundle provenance, kept outside the ownership lock.

It excludes arbitrary providers, package sources, Extension payloads, overwrite
companions as Framework targets, retired-only paths, files outside the closed
Framework footprint, and the repository `.temp/` directory.

The selected fact universe is the closed base Install subset. Ownership may
also contain scoped paths and regions from Route Init. Preserve those receipts
and current content without selecting them as root effects or checking them
against stored integrity facts.

## Ownership Observation And Publication

The generated `.agents/open-forge.lock.json` is the only state input and output.
Read its ownership receipts with the forgiving workspace reader. Missing,
malformed, unreadable or unsupported ownership never becomes an integrity gate.
A readable lock supplies ownership; unavailable ownership supplies no claims.
Do not read, migrate, delete, or honour leftover records from earlier formats.
A matching file does not establish an ownership receipt.

Root Install selects only its current embedded destinations and supported managed
blocks. Existing Framework receipts outside that subset are preserved without
reading their content or making their absence a root Install failure. Source
release metadata in a receipt does not participate in currentness.

Plan one best-effort lock publication after target verification. Preserve the
other ownership sections and unaffected Framework claims. An identical intended
receipt plans no write. If the ownership writer cannot form a safe change, skip
that change and continue; the existing public publication outcome is
`not-requested`. When publication is planned, its exact prior state participates
in preflight, revalidation, recovery preparation and verified application.
The public effect list contains that one state-file effect. It replaces the
former state-file subject rather than adding another outcome.

A known Extension claim at a selected Framework path remains an ownership
conflict, including portable case aliases and claims on a region's host.
Force cannot overwrite that destination.

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
  retaining the Entries heading and outside bytes; and
- fails closed when syntax or equivalence is unsupported or ambiguous.

Unsupported, binary, and unparseable kinds use exact-byte comparison. No
fingerprint, comparison policy, or workspace binding is persisted in ownership.

Every invocation captures current exact bytes freshly for the plan, bounded diff,
expected-state revalidation, bundle payload, write verification, and recovery.
If
current and intended semantic fingerprints are equal while exact
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

1. No Framework receipt selects a target in the closed base Install subset.
2. No exact current payload destination is occupied.
3. No canonical `AGENTS.md` or supported `CLAUDE.md` managed block exists.
4. No recognized Framework recovery or residual evidence exists.

Existing hosts without matching markers and unrelated user-owned `.agents`
content do not alone defeat this state. An unavailable absence fact is
`incomplete`; partial, malformed, ambiguous, colliding, or occupied evidence is
`blocked` when unsafe. Neither writes.

### Trusted exact managed state

A Framework receipt selects the base footprint, and its current source content
matches the running payload under the operation-time comparison policy. The
current generated navigation also matches the intended projection. Install forms
a verified no-op and preserves unrelated scoped ownership and content. It does not invent a write to normalize timestamps, formatting,
provenance, or unrelated bytes. `--force` and `--automatic` do not change the
no-op.

### Managed divergence

A selected owned target or region is divergent when current content differs
from the running payload or intended generated projection, or a selected expected
path is missing. A stale source version or an unselected scoped claim does not
establish divergence.
Install does not reconcile any of these states. It forms no mutation plan,
returns `blocked`, preserves current bytes, and directs the caller to
`open-forge update`. The same result applies when `--force` is present. Initial
force is not managed-update authority.

### Eligible initial occupant

An exact current payload destination or supported managed block may be an
eligible initial occupant only when no known ownership receipt or competing
manager claims it and all route, source, physical-identity, containment, marker,
bundle identity and recovery-bundle facts are safe. A known user-owned or Extension-owned
path, route collision, ambiguous managed block, unknown path, or unsafe boundary
is not eligible.

Normal install blocks an eligible occupant without writing. Explicit force may
replace only the exact recognized occupant, never its surrounding host bytes,
and establishes management from the new current source after complete
verification. Previous occupant bytes are retained through recovery when a replacement is
planned. Matching pre-existing files do not become whole-file ownership merely
because a no-op verifies their bytes.

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

Only the body of one unique top-level `## Entries` section may change. A missing
or duplicate heading boundary blocks before any write. The heading and all
bytes outside the body remain unchanged; retired guard comments inside it are
removed by the shared generated-navigation projection.

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
for the persistent reusable zero-byte external path defined by the [Mutation And
Recovery Technical Design](../../technical-designs/mutation-and-recovery.md). The operation holds one read/write `FileShare.None` handle and
never writes metadata, truncates, or deletes the lock file. An active handle
blocks the plan; lock state is not lifecycle authority, history, or recovery
evidence.

`--automatic` admits only safe absent creation or exact no-op effects already
selected by the explicit operation. It cannot admit an eligible initial occupant
without explicit `--force`. It never admits divergence, deletion, adoption,
ownership, or marker repair.

## Dry-Run Parity

Dry-run uses the same normalized request, fresh current facts, ownership receipts,
classification, intended state, generated projection, complete plan, and
preflight as application. It reports all safe creations, eligible force effects,
bounded generated and managed-region changes, lifecycle publication that would
occur after verification, preserved content, and recovery readiness.

At minimal detail, the preview identifies the planned
`.agents/open-forge.lock.json` state-file effect when ownership publication
would occur, separately from named installed content files, child directories,
and managed host regions. It does not fold the lock path or host regions into
the installed content-file population.

It stops before directory, file, lifecycle, recovery-bundle, temporary, formatter,
or other persistent effects. It does not claim application, verification,
lifecycle publication, or bundle-handling success. It forms the same pre-effect status
as the corresponding application request. Because dry-run performs no effects,
it never produces an apply-time `failed` or `cancelled` result. A planning or
read failure and caller cancellation before effects retain their own event
meaning.

## Application, Verification, And Recovery

When application is selected:

1. If this is a prompt-capable human application that would write, ask the one
   confirmation after complete preflight. A refusal, end of input, or caller
   cancellation stops with no effects.
2. Acquire the persistent external workspace lease. The zero-byte ordinary lock
   lives under `LocalApplicationData/OpenForge/locks/v1`, with a display-only
   friendly workspace prefix and the authoritative full SHA-256 key of the
   normalized physical workspace path. Cancellation before acquisition creates
   no workspace effect.
3. Revalidate the complete plan and all volatile source, target, ownership,
   containment, marker, section, and expected-state facts.
4. Prepare and verify the one complete external recovery bundle when the plan
   contains an existing-target effect. Complete preparation before any workspace
   effect.
5. Apply every explicitly planned missing directory parent-first through the
   shared directory capability. Missing `.agents` is the first ordinary
   directory-create effect; its later planned directories are descendants.
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
10. Publish the planned Framework ownership update after target verification,
    preserving the other sections and unaffected existing ownership. An
    unchanged receipt writes nothing. Exact prior lock bytes participate in the
    same recovery bundle when replacement is planned. Verify the state-file
    effect and report its actual outcome.
11. After final verification, delete only the positively recognized bundle
    created for this operation. `Deleted`/`Removed` permits normal completion.
    `Failed`/positively observed `Retained` keeps target effects successful and
    produces `completed-with-warnings`, the exact residual path,
    and cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the
    deletion result provides one.

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

Directory creation remains a distinct effect from file Create/Replace and has no
recovery entry. Missing `.agents` is the first ordinary lease-bound directory
effect. A directory created by this operation is retained and reported as
residual state after later failure or cancellation. Install never rolls it back,
compensates for it, or removes it. The shared capability uses ordinary BCL
filesystem behavior; it adds no P/Invoke, recovery protocol, or hostile same-user
creator-identity guarantee. Lock and recovery data use separate
application-owned versioned subtrees under `LocalApplicationData`.

Before post-verification deletion begins, an application, verification,
lifecycle-publication, or cancellation outcome stops new effects and reports
the actual residual draft or final path; a valid final remains when preparation
completed. The foundation never restores a target automatically or derives
current target state from recovery provenance. A closed final ZIP may remain
after abrupt process termination, without an executable crash or power-loss
guarantee. An unexpected concurrent edit is preserved and reported as residual
state. An unsafe residual is `failed`; caller cancellation is `cancelled` only
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
findings. Managed-footprint counts distinguish named installed content files
from child directories below `.agents`; the `.agents` container effect is
not included in the directory count, and the generated lock file and host
regions remain separate populations. All top-level properties are present for every status, arrays are
non-null, and the shared envelope's command, status, workspace, and next action
are not duplicated. Effect residual state remains the typed value `none`,
`retained`, or `unknown`, so dry-run, retained state, partial failure, and
uncertain completion cannot be collapsed into a Boolean.

The result retains exact workspace and selection method, normalized flags, source
identity, recognized footprint, ownership and management classification, semantic
current/intended facts, generated projection, effects, preserved
content, lifecycle publication, recovery-bundle facts, verification, residuals,
and at most
one next action.

Use the Interface status meanings and ordinary precedence `blocked` >
`incomplete` > `completed-with-warnings` > `completed`. `completed` includes safe application,
eligible force, dry-run, and exact no-op. Ordinary managed divergence is
`blocked`, not `completed-with-warnings`, because install does not own update authority.
Planned effects, format-only facts, force presence, automatic mode, and managed
divergence do not form `completed-with-warnings`; managed divergence directs the caller to
`update`. Post-verification recovery deletion `Failed` with positively observed
disposition `Retained` is the only current install `completed-with-warnings` condition.

Primary human `completed`, `completed-with-warnings`, and `incomplete` results go to stdout.
Primary human `invalid-input`, `blocked`, `failed`, and `cancelled` results go to
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
- forgiving ownership observation and isolation, including absent and unreadable
  states, source-unavailable facts, and unsupported or ambiguous schema handling;
- nullable operation-time `sourceAssetPath` on selected effects, separate
  whole-file and region receipts, and preservation of unselected scoped claims;
- syntax-aware semantic fingerprints, exact operation-time bytes, format-only
  observations, generated-interior exclusion, and fail-closed equivalence;
- one intended topology and current Index projection;
- bounded generated sections and root/provider markers; no hidden subprocess;
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
- the exact visible missing-`.agents` first ordinary directory effect after
  external lease acquisition, including planning/reporting, immediate
  revalidation and verification, pre-effect cancellation/contention, and
  retained residuals after later failure;
- dry-run/application parity with no persistent dry-run effects;
- seven statuses, including `Failed`/positively observed `Retained` recovery
  `completed-with-warnings` and `Failed`/`Unknown` recovery `failed`, streams, one typed result,
  JSON stdout, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state;
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys;
- no runtime implementation or shipping claim.

## Deliberately Removed Framework Destinations

Read `removedCategories`, `removedFiles` and `removedDirectories` from authored settings. A category
such as `skills` excludes embedded targets beneath `.agents/skills/`. Each
`removedFiles` entry is one exact canonical workspace-relative file destination;
it has no glob or recursive-directory meaning. A `removedDirectories` entry
excludes that canonical directory and all descendants, including future files.
The concrete destination is
excluded from whole-file and generated-region planning, including root managed
hosts. Do not recreate excluded files, change settings, or infer new ownership
for them. Existing user content and prior receipts outside actual selected effects
remain preserved. If an excluded missing entrypoint makes another selected route
unreachable, report a structural blocker instead of recreating it. Required loader
and root host anchors are otherwise not categories.
