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
  -> lifecycle publication and retained-recovery reporting
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

The request may use a compact human inspection and confirmation flow when it is
prompt-capable and no automatic mode is selected. `--automatic`, JSON, and other
non-interactive modes never prompt. A recommendation or displayed choice never
supplies force authority.

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
current source are exact. Install forms a verified no-op. It does not invent a
write to normalize timestamps, formatting, or unrelated bytes. `--force` and
`--automatic` do not change the no-op.

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
facts. Every planned existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) must be covered by one
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

1. Revalidate the complete plan and all volatile source, target, ownership,
   containment, marker, section, and expected-state facts.
2. Revalidate each target immediately before its effect.
3. Apply complete planned file or bounded-region bytes through the accepted safe
   replacement property. Do not edit in place or weaken the property after a
   check fails.
4. Verify each payload, root/provider, generated-region, and lifecycle effect.
5. Rebuild and verify the complete recognized Framework result and preservation
   boundaries as one operation.
6. Publish or refresh only the Framework lifecycle facts established by the
   complete verified result, preserving the unrelated lifecycle section
   semantically. A selected lifecycle semantic change is source-generated as one
   deterministic canonical UTF-8 whole-document representation; formatting,
   ordering, and line-ending trivia may be normalized. A semantic no-op publishes
   no lifecycle write. Prior bytes remain retained in the verified operation
   bundle.
7. After final verification, delete only the positively recognized bundle
   created for this operation. If deletion fails, retain successful effects and
   return `attention` with the exact residual path and cleanup guidance.

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

If application, verification, or lifecycle publication fails, new effects stop
and the actual residual draft or final path is reported. A valid final remains
when failure occurs after preparation. The foundation never restores a target
automatically or derives current target state from recovery provenance. A closed
final ZIP may remain after abrupt process termination, without an executable
crash or power-loss guarantee. An unexpected concurrent edit is preserved and
reported as residual state. An unsafe residual is `failed`; caller cancellation
is `interrupted` only when no stronger failure remains. Cleanup owns exact named
final and draft deletion under its separate lease-bound contract. A later
invocation forms a fresh plan and never replays a saved plan, receipt, journal,
history, or progress record.

## Result Formation And Streams

The operation forms one typed result after invalid input, classification,
preflight, dry-run, verified application, interruption, or recovery. Human and
JSON renderers consume that result and do not rerun lifecycle work.

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
`attention` remains in the shared status vocabulary but has no accepted finite
install condition and is currently unreachable. Planned effects, format-only
facts, force presence, automatic mode, and managed divergence do not make it
reachable; managed divergence is `blocked` and directs the caller to `update`.

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
- all four safe-absence facts, trusted exact no-op, managed-divergence block with
  `update` next action, eligible initial occupant, and force-only initial
  replacement;
- exact schema-v1 lifecycle-document isolation, absent, untrusted, and missing
  states, source-unavailable facts, and unsupported or ambiguous schema handling;
- syntax-aware semantic fingerprints, exact operation-time bytes, format-only
  observations, generated-interior exclusion, and fail-closed equivalence;
- one intended topology and current Index projection;
- bounded generated and root/provider markers; no hidden subprocess;
- complete preflight, external schema-v1 recovery-bundle preparation and verification,
  exact prior-byte preservation, expected-state revalidation, per-effect and
  whole-operation verification, success-only bundle removal, failure
  retention/reporting, and fresh rerun;
- dry-run/application parity with no persistent dry-run effects;
- seven statuses, with `attention` currently unreachable, streams, one typed
  result, JSON stdout, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state;
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys;
- no runtime implementation or shipping claim.
