---
open-forge:
  description: Accepted non-shipping Interface for establishing and verifying the managed root Framework lifecycle
  responsibility: Define install's exact syntax, management-establishment boundary, initial force rule, results, and read/write surface
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Interface, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Install Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for the non-shipping
root `install` command. It owns the public purpose, syntax, flags, exact
Framework footprint, management-establishment states, observable effects,
results, errors, examples, non-goals, and caller-visible conformance boundary.
The new CLI does not ship yet.

The sibling [Behavior Contract](behavior.md) defines technology-neutral request
resolution, lifecycle classification, planning, verification, recovery, and
conformance. The shared [Global CLI Flags Interface](../shared/global-flags/interface.md)
defines the six global flags once. Framework routing and maintenance sources
remain authoritative for the meaning of the files that this operation consumes.

The only new-CLI lifecycle document is `.agents/open-forge.lifecycle.json`, schema
v1. It has a common envelope and isolated `framework` and `extensions` sections.
An install operation changes only `framework` and preserves the unrelated
`extensions` section and common-envelope meaning. When selected lifecycle
meaning changes, the writer emits one deterministic canonical UTF-8
whole-document representation; lifecycle property order, whitespace, and line
endings are not preserved. A semantic no-op writes nothing. When an existing
target is replaced, its prior bytes are recoverable only through the verified
external recovery bundle described below; the CLI does not inspect or report
repository state or claim history evidence. The document stores no plan, runtime
history, journal, recovery evidence, or session. Files outside this exact path
are ordinary workspace content, not lifecycle input.

The shared CLI Architecture defines the exact structured JSON result schema and
numeric exit mapping. This Interface uses those shared definitions without
duplicating implementation mechanics. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. The accepted
lifecycle direction does not claim that implementation or proof.

## Purpose And Boundary

`install` establishes management of the embedded Framework in one exact
workspace. It creates a safely absent recognized Framework state or verifies an
exact trusted managed state as a no-op. It does not reconcile managed
divergence. Existing managed divergence directs the caller to the root
`update` operation.

The command has one stable root operation. Its request, current facts, intended
state, generated-navigation projection, complete plan, preflight, status model,
verification, and recovery remain the same for normal mode, initial force, and
dry-run. `--force` widens only the eligible initial-occupant boundary; it never
turns `install` into managed update, adoption, or generic replacement.

`install` may establish lifecycle facts only after the complete selected plan
has applied and verified. A dry run, incomplete result, blocked result, failed
result, or interrupted result does not publish lifecycle state.

## Syntax

The complete public command form is:

```text
open-forge install [--force] [--automatic] [--dry-run] [global flags]
```

`install` is a direct root command. It has no operands, child operations,
`framework` group, root `init`, replacement or reinstall alias, `--prune`,
`--yes`, or generic plan or apply mode.

The shared [Global CLI Flags Interface](../shared/global-flags/interface.md)
defines:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

Those flags retain their shared grammar, defaults, repetition, composition,
terminal behavior, and errors. `--help` and `--version` stop before workspace
selection and install work. Command-specific input remains invalid with a
terminal mode.

## Exact Workspace And Source

The operation selects one exact workspace:

- Without `--workspace`, it uses the process current working directory.
- With `--workspace <path>`, it uses exactly that path, resolving a relative
  value from the process current working directory.
- It normalizes the selected path for reporting but never substitutes another
  root discovered from Git, markers, a nested `.agents`, or nearby files.
- A missing, unavailable, non-directory, physically aliased, or unsafe selected
  workspace is blocked rather than discovered around.

The source is only the current Framework payload embedded in the running CLI.
Install does not download, fetch, search for, or restore a payload from a
network, package source, or another workspace.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Recognized Framework Footprint

The recognized footprint is closed. It contains only:

1. The embedded current Framework payload's exact recognized destinations below
   `.agents`, including authored files and affected generated `Entries` regions.
2. The exact canonical `AGENTS.md` managed block.
3. The exact supported Claude `CLAUDE.md` managed bridge block.
4. The transparent Framework lifecycle facts needed to establish or compare
   management for those targets and regions.

Generated `Entries` are derived navigation. Their expected bodies come from the
intended authored topology and metadata in the selected workspace, not from
generated interiors embedded in the payload. The current [Index Interface](../index-candidate/interface.md)
and [Index Behavior](../index-candidate/behavior.md) own the generated-region
projection and bounded-marker rules that install consumes in its one plan.

Install never expands this footprint from filename resemblance, tags, route
names, byte equality, globs, arbitrary provider files, the lifecycle document,
an Extension-owned path, an overwrite companion, a retired-only target, or an
operand. Bytes outside valid root/provider blocks remain workspace content.

## Operands

No operands are accepted. A directory, source reference, provider name, glob,
route, or path intended to narrow the Framework is invalid. There is no partial
footprint mode.

## Flags

| Flag                | Role                          | Value                          | Omission                                                         | Repetition and composition                                                                                              |
| ------------------- | ----------------------------- | ------------------------------ | ---------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `--force`           | Initial replacement authority | Boolean                        | Selects ordinary management establishment or exact managed no-op | Repeats idempotently. It does not imply update, prune, adoption, or ownership.                                           |
| `--automatic`       | Guided-input policy           | Boolean                        | Human input may use the compact inspection and confirmation flow | Repeats idempotently. It suppresses interaction and selects only deterministic safe defaults.                           |
| `--dry-run`         | Preview write policy          | Boolean                        | Permits application after the same preflight                     | Repeats idempotently. It writes nothing and uses the same request, facts, plan, and status as apply.                    |
| Shared global flags | Workspace and presentation    | Defined by the shared contract | Shared defaults                                                  | Shared repetition and terminal rules apply.                                                                             |

### `--force`

Normal `install` may create only safely absent current targets or verify an
exact managed state. An exact current destination occupied before management is
established is an eligible initial occupant only when complete facts establish
that it has no trusted lifecycle owner or competing manager, no route or source
collision, no ambiguous marker or containment boundary, and no unsafe recovery
condition. A manually authored or untracked occupant with a competing ownership
claim is not eligible.

`install --force` may replace only that exact recognized current occupant and
then establish management from the newly written current source after complete
verification. It records the verified result; it does not adopt the occupant's
old bytes as lifecycle history.

Force does not:

- reconcile an already managed changed, missing, retired, or source-divergent
  state;
- adopt an unowned or another-manager-owned path;
- bypass route, source, physical-identity, containment, ownership, marker,
  expected-state, bundle, verification, or recovery checks;
- repair malformed generated or managed markers;
- delete retired content; or
- replace bytes outside the exact current Framework footprint.

When an existing managed state diverges, both `install` and `install --force`
return `blocked`, make no write, and provide one useful `Next:` action for
`open-forge update`. Force is not an update shortcut.

### `--automatic`

`--automatic` suppresses the human inspection and confirmation flow. It selects
only the documented deterministic safe effects for the explicit `install`
operation. It never supplies initial force authority, replaces divergence,
restores missing managed content, deletes retired content, adopts content,
takes ownership, or bypasses a safety boundary.

For a safely absent workspace, automatic mode may establish the ordinary
installation. For an exact managed state, it may verify the no-op. For an
eligible initial occupant, it does not select `--force`; explicit force remains
required. Repetition is idempotent.

### `--dry-run`

Dry-run resolves the same exact workspace, source, lifecycle facts, intended
state, generated projection, complete plan, and preflight as application. It
shows every selected effect and bounded diff, but writes no payload file,
managed block, generated region, lifecycle fact, recovery bundle, temporary
artifact, or other persistent state. It cannot claim application, verification,
lifecycle publication, or bundle-handling success.

For application with one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`),
orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree, with no temporary-directory, repository, `HOME`,
or custom-platform fallback. An operation containing only `Create` effects or
semantic or byte no-ops does not resolve recovery storage and creates no bundle.
Otherwise it prepares exactly one immutable ZIP recovery bundle
outside the workspace for the complete operation. Its deterministic external
directory key and final name use the normalized physical workspace path and
operation ID. The
source-generated schema-v1 `manifest.json` and streamed ordinal payload
entries identify the operation and normalized physical workspace, and record exact prior bytes,
lengths, hashes, ordered relative targets, change kinds, and intended final
absence or length/hash. A draft is CreateNew-written under its exact name,
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte verification, moved within the same directory to its
deterministic final name, and verified again. Only the valid final ZIP forms the
opaque `RecoveryBundlePreparation`; the draft remains `Incomplete`. Every
planned existing-target effect must match the preparation; Create and no-op effects have
none. All preparation is complete before the first effect. Unavailable storage
is `incomplete` before effects; a collision or failed final verification is
`blocked` before effects.

Workspace mutation uses the persistent, reusable `.agents/open-forge.lock` path
under the accepted CLI Architecture. Existing bytes are preserved; the
operation holds only a `FileShare.None` handle and never writes metadata,
deletes, or truncates the lock file. File existence is not lock ownership. An
active handle blocks mutation; lock behavior is concurrency safety, not
lifecycle authority or recovery history.

After final verification, whole-command success deletes only the positively
recognized bundle it created. Deletion failure leaves effects successful and
returns `attention` with the exact residual path and cleanup guidance. Handled
failure or cancellation stops new effects and reports the actual residual draft
or final path; a valid final remains when failure occurs after preparation. A
closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. No target is automatically restored,
no current target state is derived from recovery provenance, and no journal,
progress receipt, history, or replayable plan is saved. Cleanup owns exact named
final and draft deletion under its separate lease-bound contract.

## Management States

Install distinguishes these finite states without inferring ownership from a
path, tag, route, matching bytes, or matching fingerprint:

| Current facts                                                                                                                 | Normal `install`                                                  | `install --force`                                                      | Result                                                         |
| ----------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- | ---------------------------------------------------------------------- | -------------------------------------------------------------- |
| Safe absence: no lifecycle claim, no occupied exact current targets, no managed root/provider block, and no recovery residual | Establish the current footprint and management after verification | Same plan; force adds no authority                                     | `complete` after verified apply or complete pre-effect dry-run |
| Trusted managed state is semantically exact                                                                                   | Verified no-op; do not rewrite format-only bytes                  | Same no-op                                                             | `complete`                                                     |
| Trusted managed state is changed, missing, retired, or source-divergent                                                       | Do not reconcile; direct the caller to `update`                   | Same; force does not change the operation                              | `blocked`, no writes                                           |
| Exact current destination is an eligible initial occupant                                                                     | Preserve it                                                       | Replace the exact occupant and establish management after verification | Normal `blocked`; eligible force `complete`                    |
| User-owned, Extension-owned, unknown, colliding, or unsafe content intersects the footprint                                   | Preserve and stop                                                 | Preserve and stop                                                      | `blocked`, no writes                                           |
| Required lifecycle or absence coverage is safely unavailable                                                                  | Do not guess                                                      | Do not broaden the footprint                                           | `incomplete`, no writes                                        |
| Required identity, markers, containment, or lifecycle facts are malformed or ambiguous                                        | Do not write                                                      | Do not repair or bypass                                                | `blocked`, no writes                                           |

Safe non-Framework content, user routes, Memory, overwrite companions, and
content outside the recognized footprint are preserved and do not create a
status condition by themselves.

## Lifecycle Identity And Trust

The `framework` section records source identity, exact target and managed-region
identity, generated relationships, semantic baseline fingerprints, and
coverage/trust. Its common envelope and `extensions` section do not grant
Framework authority merely because they share a physical document.

For supported Markdown and frontmatter kinds, `open-forge-markdown-v1` is the
conservative semantic fingerprint policy. It preserves Unicode, semantic text,
headings, tags, links and destinations, marker meaning, inline and code-block
content, and significant whitespace. It normalizes only line endings and
parser-proven formatting trivia. Unsupported, binary, and unparseable kinds use
exact bytes and fail closed when equivalence cannot be proven.

A trusted section requires exact workspace and target identity, supported
versions and fingerprint policy, intact consistency, and complete verifiable
coverage. A safely absent section may be established only after complete
inspection proves that no expected managed state, boundary, or recovery
residual exists. An absent document or section is not, by itself, proof of
unmanaged state. Missing, malformed, unsupported, unverifiable, or internally
inconsistent lifecycle facts are not silently treated as empty or trusted. Safe
unavailable coverage is `incomplete`; unsafe ambiguity is `blocked`.

For supported parseable files, managed identity uses the `open-forge-markdown-v1`
parser/AST-derived, syntax-aware semantic fingerprint. Generated `Entries`
interiors are derived and are not authored identity. Unsupported, binary, and
unparseable kinds use exact bytes and fail closed when equivalence cannot be
proven.

The lifecycle document persists semantic baseline fingerprints for supported
parseable kinds, not a persistent exact-byte baseline digest. Install captures
exact current bytes only for operation-time planning, expected-state checks,
verification, and recovery. A format-only difference with equal semantic
identity is informational and is not managed divergence.

## Generated Navigation And Ownership

Install forms one hypothetical post-install workspace from current authored
content plus permitted payload and bounded-block effects. It then projects every
affected generated region from that topology and metadata, preserving user-added
routes and intentionally absent defaults. It changes only the valid generated
interior and preserves markers and outside bytes. A missing, duplicate, reversed,
nested, misplaced, or ambiguous generated boundary blocks the plan; force does
not repair it.

Extension ownership, Framework ownership, user ownership, and external-manager
claims remain distinct. Matching semantic fingerprints do not adopt an unowned
file. Install never writes the lifecycle document over a conflicting
`extensions` section or path and never changes Extension lifecycle facts or
package source.

## Output And Streams

The default human result leads with the operation and exact workspace. It reports
normal or force mode, automatic and dry-run state, recognized footprint counts,
created or replaced effects, preserved divergence, generated projections,
  lifecycle publication or preservation, recovery facts, status, and at
most one required `Next:` action. Compact view retains identity, mode, key
effects, safety facts, status, and the bounded next action. JSON carries one
complete structured result from the same typed result for every status.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Bounded diagnostics go to stderr. Human output uses `requires attention`
for the typed `attention` status; structured output retains `attention`.

## Semantic Results

| Result        | Meaning for `install`                                                                                                                                                                                                                                                                                                       |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Safe installation, eligible initial force, exact managed no-op, or complete pre-effect dry-run has complete coverage. The current contract has no finite condition that produces `attention`.                                                                                                                               |
| `attention`   | No accepted finite install condition reaches this status. `attention` remains in the shared status vocabulary but is currently unreachable. Planned effects, `--force`, format-only observations, automatic mode, and managed divergence do not make it reachable; managed divergence is `blocked` and directs to `update`. |
| `incomplete`  | Safe required source, lifecycle, absence, parser, or recovery coverage is unavailable. No write occurs.                                                                                                                                                                                                                     |
| `invalid`     | Syntax, operand, flag, repetition, value, or terminal-mode input prevents request resolution.                                                                                                                                                                                                                               |
| `blocked`     | An unsafe, ambiguous, colliding, untrusted, unauthorized, or managed-divergence boundary prevents one safe install plan.                                                                                                                                                                                                     |
| `failed`      | Application, verification, lifecycle publication, or bundle handling fails unexpectedly or leaves an unsafe residual after effects begin.                                                                                                                                                                       |
| `interrupted` | The caller interrupts before completion and no unexpected application or verification failure changes the result.                                                                                                                                                                                             |

Ordinary planning precedence remains `blocked` > `incomplete` > `attention` >
`complete` for the shared status vocabulary. `attention` is currently
unreachable: planned effects, force presence, format-only observations,
automatic mode, and managed divergence do not produce it. JSON uses one result
on stdout for every status; process exits use the exact shared CLI Architecture
mapping.

## Errors And Next Actions

Every ordinary error names `install`, the exact workspace and affected target or
lifecycle fact when known, the cause, and at most one useful next action.

- Any operand, `--prune`, alias, unknown flag, malformed value, invalid
  repetition, or command-specific input combined with terminal help/version is
  `invalid`.
- A missing or unavailable exact workspace is `blocked`.
- An unavailable required embedded payload or safe absence/trust fact is
  `incomplete`.
- An occupied exact target, managed divergence, ownership or route collision,
  ambiguous marker, unsafe containment, recovery-bundle collision, or other
  unsafe preservation condition is `blocked`.
- Unavailable or unsafe recovery-bundle storage is `incomplete` before effects;
  malformed or unverified bundle content is `blocked`.
- Managed divergence uses one compact `Next: open-forge update` action. It does
  not suggest force as an install shortcut.

## Examples

Establish the Framework in the exact current workspace:

```text
open-forge install
```

Preview a safely absent installation without interaction:

```text
open-forge install --automatic --dry-run --json
```

Replace one eligible exact initial occupant. This does not adopt its old bytes:

```text
open-forge install --force
```

Preview the same bounded initial authority:

```text
open-forge install --force --automatic --dry-run
```

An exact managed installation is a verified no-op. A managed changed, missing,
retired, or source-divergent installation returns `blocked` with `Next: open-forge
update`; it never becomes an update because `--force` or `--automatic` was
present.

## Non-Goals And Architecture Boundary

Install does not:

- perform managed update, reinstallation, replacement of a trusted divergent
  state, restoration of a missing managed target, or retired-content deletion;
- create a Framework group, root `init`, update/reinstall/replace/restore/recover
  alias, uninstall/remove leaf, generic apply, saved plan, session, or journal;
- discover providers, routes, package sources, Extension paths, or arbitrary
  workspace files;
- adopt matching bytes, repair markers, replace overwrite companions, or change
  user content outside valid managed regions;
- execute a formatter or persist formatter state;
- mutate the `extensions` section, the package source, or the repository
  `.temp/` directory.

If a supported formatter configuration is detected, the accepted conservative
direction allows informational advice only. Detection does not select a
formatter, execute it, change files, grant authority, make a formatting guess,
or persist formatter state.

The accepted CLI Architecture defines lifecycle serialization, filesystem
identity, concurrency, recovery-bundle names, temporary artifacts, diagnostics,
packaging, and implementation boundaries. Gate 5 must prove those boundaries and
the embedded deterministic inventory/hash evidence. This Interface remains
technology-neutral and does not claim that proof.

## Public Conformance

Future evidence must cover:

- exact root syntax, no operands, shared flags, terminal modes, and idempotent
  Boolean repetition;
- exact CWD and `--workspace` selection without discovery;
- safe absence's four facts, exact managed no-op, eligible initial occupant,
  managed divergence directing to update, and `--automatic` not supplying force;
- trusted, absent, untrusted, missing, unavailable, malformed, unsupported, and
  ambiguous lifecycle facts without inferred ownership;
- semantic equality for format-only differences, exact-byte operation facts,
  parser-proven fingerprint boundaries, and fail-closed equivalence;
- intended-topology generated projection, bounded markers, outside-byte
  preservation, and one complete lifecycle plan;
- one verified immutable external schema-v1 ZIP bundle for the complete
  operation, exact prior-byte and provenance facts, expected-state
  revalidation, per-effect and whole-operation verification, success-only
  removal, cleanup attention, failure retention/reporting, and fresh rerun
  behavior;
- dry-run parity with no payload, lifecycle, recovery bundle, or temporary
  effects;
- seven statuses, with `attention` currently unreachable, ordinary precedence,
  human streams, one-result JSON, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state, and no runtime
  implementation or shipping claim;
- Gate 5 evidence for source-generated serialization, fixed Markdig where used,
  real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.
