---
open-forge:
  description: Accepted technology-neutral behavior for trusted Framework update planning, force replacement, prune deletion, and recovery
  responsibility: Define update's deterministic fact flow, semantic comparison, complete plan, guarded effects, and typed result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Update, Framework, Behavior, Determinism, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Update Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping root
`open-forge update`. It defines deterministic request resolution, trusted
Framework lifecycle facts, baseline/current/intended comparison, semantic
fingerprints, generated projection, normal/force/prune planning, preflight,
dry-run, application, verification, recovery, result formation, and
technology-neutral conformance.

The [Interface Contract](interface.md) defines public syntax, flags, statuses,
output, errors, examples, and non-goals. `install` owns establishment and
initial force. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the shared JSON
result schema and exit mapping. The [Lifecycle Provenance Technical
Design](../../technical-designs/lifecycle-provenance.md) defines exact lifecycle
serialization realization, while the [CLI Architecture](../../architecture.md)
defines cross-cutting implementation structure. This behavior does not duplicate
those mechanics or claim their Gate 5 proof.

## Complete Typed Flow

```text
validated command input
  -> exact workspace and embedded current Framework
  -> trusted Framework lifecycle section
  -> fresh baseline/current/intended facts
  -> authoritative generated projection
  -> one complete ordered plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> lifecycle publication and recovery-disposition reporting
  -> one typed result
```

Update never chooses a safe subset when another selected effect is blocked,
incomplete, ambiguous, or unsafe. A verified no-op has no synthetic write.
Dry-run and application share request resolution, current facts, comparison,
intended state, plan, preflight, and pre-effect status.

## Request And Workspace Resolution

The resolver:

1. Accepts direct root `update` with no operand and rejects source, package,
   Framework-group, root-init, remove, reinstall, and generic apply forms.
2. Resolves shared help/version before workspace or lifecycle work.
3. Collapses repeated Boolean `--force`, `--prune`, `--automatic`, and
   `--dry-run` flags idempotently.
4. Selects exactly CWD or exactly `--workspace`, with no upward, Git-root,
   nested-root, marker, or nearby-source discovery.
5. Keeps authority dimensions independent. Force never enables prune; prune never
   enables force; automatic never enables either.

Workspace validation establishes lexical and physical containment and exact
identity. Missing, unavailable, non-directory, escaping, or aliased identity is
`blocked`. The source resolver admits only the embedded current Framework and
returns `incomplete` for safe unavailable payload facts or `blocked` for unsafe
source identity. The CLI distribution embeds Framework and first-party Extension
assets with deterministic inventory and hash proof; this is distributed source
identity, not workspace or runtime implementation evidence.

Resolve that inventory through the neutral Framework distribution capability
over ordinary .NET embedded resources. Runtime never reads the repository source
tree.

## Trusted Lifecycle Resolution

The operation reads `.agents/open-forge.lifecycle.json`, schema v1, as a common
envelope with isolated `framework` and `extensions` sections. It validates and
preserves the unaffected section and common-envelope meaning semantically. A
selected semantic change emits one deterministic canonical UTF-8 whole-document
representation; lifecycle formatting, property order, and line endings are not
preserved, while a semantic no-op writes nothing. An invalid unrelated section
blocks the write rather than being dropped, normalized, or merged. The document
stores no plan, runtime history, journal, recovery evidence, or session. Files
outside this exact path are ordinary workspace content, not lifecycle input.

The Framework section is trusted only when schema v1 and
`open-forge-markdown-v1`, exact workspace and target identities, reciprocal
internal facts, duplicate-free identities, and complete verifiable coverage all
hold. A safely absent section does not satisfy update's required state. An absent
document or section is not, by itself, proof of unmanaged state. Missing expected
sections are not empty. Unsupported or ambiguous schema facts form `incomplete`
when safely unavailable and `blocked` when unsafe or ambiguous.

Safe unavailable coverage forms `incomplete`; unsafe or ambiguous identity,
cross-section collision, malformed marker, or ownership fact forms `blocked`.
Force and prune never promote an untrusted section.

Every Framework target record has required nullable `sourceAssetPath`. Validate
non-null paths as normalized canonical embedded asset-relative identities even
when a historical asset is absent from the current inventory. Require non-null
provenance for payload files and managed root/provider blocks and `null` only for
derived generated-region targets. Verify every newly published non-null path
against the current inventory. User-owned inserted scope entrypoints never enter
the Framework target set.

## Semantic Comparison

For every recorded current target and bounded root/provider region, collect
fresh current exact bytes and semantic facts and compare them with the trusted
baseline, recorded source provenance, and embedded or derived source. The comparison records baseline, current, and
intended semantic fingerprints, exact bytes, target identity, ownership, route
and marker facts, and source availability.

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware fingerprints. The
canonical representation preserves Unicode, semantic text, headings, tags,
links and destinations, marker meaning, inline content, code blocks, and
significant whitespace. It normalizes only line endings and parser-proven
formatting trivia. Generated `Entries` interiors are excluded from authored
identity. Unsupported, binary, or unparseable kinds use exact-byte identity and
fail closed on unknown equivalence.

Persist only semantic baseline fingerprints for supported parseable kinds. Do not
persist a new exact-byte baseline digest. Fresh exact bytes remain required for
diff, expected-state checks, recovery-bundle payload, existing-target effect
application, verification, and recovery.

If current and baseline semantic identities are equal but exact bytes differ only
by parser-proven formatting trivia, report a formatting-only observation and do
not classify the path as divergence. If current equals baseline semantically and
the intended source has a real semantic change, normal update may apply current
source bytes to that baseline-unchanged target.

## Intended State And Projection

The planner forms one hypothetical post-update workspace from current authored
content plus only the effects admitted by normal, force, or prune authority. It
preserves user-added routes, overwrite companions, intentionally absent
defaults, Extension paths, unknown files, and all outside bytes.

The current Index projection derives every affected generated `Entries` body from
that intended authored topology and metadata. Generated lines never supply
topology or metadata, and package or embedded generated interiors are not copied.
Only valid bounded generated interiors may change. A missing, duplicate,
reversed, nested, misplaced, or ambiguous boundary blocks before the first
effect. Update never starts a hidden index subprocess.

Root/provider planning admits only absent-host creation when supported, bounded
append to a valid host with no markers, or replacement inside exactly one valid
ordered marker pair. Outside bytes remain unchanged. Force is not marker-repair
authority.

## Normal Plan

Normal mode admits:

- baseline-unchanged current expected files and valid managed regions whose
  intended current source differs;
- genuinely new current source files or regions when absent, safe, and fully
  covered; and
- generated-region effects required by those authored changes.

Normal mode preserves:

- current expected files or regions changed from baseline;
- current expected files or regions missing from the workspace;
- retired managed content, whether changed or unchanged;
- user, Extension, shared-owner, unknown, and route-colliding content; and
- all bytes outside bounded managed and generated regions.

Preserved finite divergence yields `attention` after safe non-divergent effects
apply or preview. A repeat with unchanged facts produces no effect and remains
`attention`. Planned changes alone do not create attention.

## Force Plan

Force starts from the same facts and intended state. It admits only:

- overwrite of a changed current expected Framework file or valid managed
  region whose exact baseline identity is trusted; and
- restoration of a missing current expected Framework target at its exact
  current destination.

Force does not admit retired deletion, unknown or unowned content, ownership or
route collision, marker repair, containment escape, arbitrary path replacement,
weaker verification, or weaker recovery. It never turns an absent
or untrusted lifecycle section into trusted management.

Force has no attention merely because replacement was planned. It remains
`attention` only when trusted present retired content must be preserved because
prune was not selected. If all effects verify and no such finite condition
remains, force is `complete`.

## Prune Plan

Prune admits only a retired path when all of these facts are complete:

- the trusted baseline names that exact previously managed path;
- a payload file or managed block has a non-null `sourceAssetPath` naming the
  exact previously embedded asset and the current inventory proves that asset is
  retired, or a derived generated-region target has `null` and the current
  intended topology proves that its recorded relationship no longer produces the
  region;
- current physical identity and semantic state are known and safe;
- no other owner, manager, shared owner, route descendant, or host dependency
  blocks deletion; and
- recovery-bundle and preservation facts pass.

Prune deletes no current expected path, does not restore a missing path, does not
infer ownership, and does not clean arbitrary artifacts. Unknown, unowned,
shared, changed-owner, route-unsafe, or ambiguous content blocks rather than
being deleted. `--force --prune` admits both exact classes in one plan and
performs no other effect.

## Automatic And Dry-Run

Automatic mode selects only safe normal effects for the fixed Framework subject.
It preserves changed, missing, and retired content and reports the resulting
attention facts. It never selects force or prune based on a recommendation,
never deletes or replaces divergent content, and never bypasses safety.

Dry-run consumes the same complete plan and preflight as application. It reports
safe changes, force replacements/restorations, prune deletions, preserved facts,
semantic comparisons, generated projections, lifecycle changes that would be
published after verification, recovery-bundle requirements, and diagnostics.
It writes no payload, generated region, lifecycle section, recovery bundle, temporary
artifact, or formatter output. It cannot claim application-time revalidation,
verification, publication, or bundle-handling success. It forms the same pre-effect
planning status as application but never produces an apply-time `failed` or
`interrupted` result because it performs no effects. A planning or read failure
and caller cancellation before effects retain their own event meaning.

## Preflight, Application, Verification, And Recovery

Preflight validates all selected target identities, source bytes, semantic
comparisons, route and generated boundaries, ownership, containment, cross-
section preservation, expected state, recovery-bundle readiness, and
verification conditions. One failed condition blocks the complete plan. Before
the first target effect, one immutable external schema-v1 ZIP
bundle covers every existing-target effect (`Replace`, `ReplaceGeneratedRegion`,
or `Delete`) in the complete operation.
Its source-generated `manifest.json` and streamed ordinal payload
entries contain command/operation/workspace identity, ordered relative targets,
change kinds, exact prior bytes/lengths/hashes, payload names, and intended final
absence or length/hash. `Create` and semantic/byte no-op effects have no entry
and create no bundle. Preparation uses CreateNew under the exact draft name in
the LocalApplicationData `OpenForge/recovery/v1` subtree only, closes and reopens
for semantic manifest, exact ordered entry, length, hash, and payload-byte
validation, moves within the same directory to the deterministic final name, and
reopens and verifies it. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. Unavailable storage
is `incomplete` before effects; malformed, mismatched, or colliding final facts
are `blocked` before effects.

Before the first workspace effect, the implementation obtains the actual OS lock
for the persistent reusable zero-byte external path under
`LocalApplicationData/OpenForge/locks/v1` defined by the [Mutation And Recovery
Technical Design](../../technical-designs/mutation-and-recovery.md). It holds one read/write `FileShare.None` handle and never writes
metadata, deletes, or truncates the
lock file. An active handle blocks the plan; the lock is not lifecycle authority,
history, or recovery evidence.

Application revalidates the complete plan immediately before effects and each
volatile target immediately before its effect. The `FileChangeApplier` requires
the matching verified bundle preparation for every existing-target effect and
performs one final effect for each target. It applies complete planned bytes or
bounded deletion effects, verifies each effect and the complete resulting
Framework projection, then publishes refreshed Framework facts atomically while
preserving the unrelated lifecycle section semantically. A selected lifecycle
semantic change is source-generated as one deterministic canonical UTF-8
whole-document representation; formatting, ordering, and line-ending trivia may
be normalized. A semantic no-op publishes no lifecycle write.

After all target effects and final lifecycle verification succeed, the command
deletes only the positively recognized bundle it created. `Deleted`/`Removed`
permits normal completion. `Failed`/positively observed `Retained` keeps target
effects successful and produces `attention`, the
exact residual path, and cleanup guidance. `Failed`/`Unknown` produces `failed`
and reports an exact expected path only when the
deletion result provides one. When `Failed`/positively observed `Retained`
recovery attention coexists with finite unresolved divergence, cleanup guidance
owns the single next action; divergence facts remain visible evidence. Before post-verification deletion
begins, a handled application, verification, or cancellation outcome stops new
effects and reports the actual residual draft or final path; a valid final
remains when preparation completed. A closed final ZIP may remain after abrupt
process termination, without an executable crash or power-loss guarantee.
The foundation never restores a target automatically or derives current target
state from recovery provenance. Cleanup owns exact named final and draft deletion
under its separate lease-bound contract.
A later request forms fresh facts and never replays a saved plan, receipt,
journal, history, or progress record.

## Formatter Boundary

The accepted conservative formatter direction allows Update to detect supported
formatter configuration and advise the user to exclude `.agents` or intentionally
format and review before commit. It does not execute a formatter, choose one,
change files for formatting, or persist formatter state. Format-only semantic
equality is an observation, not a lifecycle mutation.

## Result Formation

One typed result records exact workspace and source, mode, trust and coverage,
baseline/current/intended comparisons, safe and authority-selected effects,
preserved divergence, generated projection, lifecycle publication,
recovery-bundle facts, verification, residuals, status, and at most one next
action.
Human and JSON renderers consume it once.

Result formation uses `invalid` before operation work; then ordinary precedence
`blocked` > `incomplete` > `attention` > `complete`. `failed` and `interrupted`
retain event meaning. Complete includes a verified no-op and a complete dry-run;
attention reflects preserved finite divergence, not merely planned effects,
force/prune flags, or format-only observations.

Primary human complete/attention/incomplete results use stdout. Primary human
invalid/blocked/failed/interrupted results use stderr. JSON emits one complete
result on stdout for every status, and bounded diagnostics use stderr.

## Behavioral Conformance

Conformance must show:

- exact parser and workspace resolution, shared global behavior, and independent
  force, prune, automatic, and dry-run dimensions;
- trusted-state requirement, schema-v1 section semantic preservation,
  deterministic canonical whole-document serialization on selected change, no
  write on semantic no-op,
  absent/untrusted/unavailable handling, and unsupported or ambiguous schema
  behavior;
- required nullable per-target `sourceAssetPath`, current and historical asset
  validation, generated-region `null`, publication against the current embedded
  inventory, and exclusion of user-owned scope entrypoints;
- baseline/current/intended classifications for unchanged, new, changed,
  missing, retired, and format-only content;
- semantic fingerprint preservation and fail-closed unsupported equivalence;
- normal preservation, force-only current replacement/restoration, prune-only
  retired deletion, and safe force/prune composition;
- authored-topology generated projection and bounded marker behavior;
- complete-plan blocking, one bundle covering each existing replacement or
  deletion, exact-byte preservation, expected-state verification, all three
  post-verification deletion state/disposition facts, residual reporting, interruption,
  fresh rerun, and visible OS-lock ownership;
- dry-run parity without persistent effects;
- one typed result, seven status meanings, stream assignment, JSON stdout,
  bounded diagnostics, and no more than one required next action; and
- no formatter execution or persisted formatter state;
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys; and
- no runtime, package, implementation, or shipping claim in this contract.
