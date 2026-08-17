---
open-forge:
  description: Accepted technology-neutral Extension update behavior for trusted source reconciliation, force, prune, shared owners, and recovery
  responsibility: Define update's deterministic source and lifecycle resolution, one complete plan, bounded effects, verification, and result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Update, Behavior, Dependency, Ownership, Safety, Recovery, CurrentTruth]
---

# extension update Behavior Contract

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for non-shipping
`open-forge extension update`. It defines exact request and source resolution,
Framework-anchor and route-host gating, trusted lifecycle facts, dependency
closure, baseline/current/intended comparison, semantic fingerprints, ownership,
normal/force/prune planning, generated navigation, dry-run/application,
verification, recovery, result formation, and conformance. It does not choose
package schema, parser, storage, or implementation technology. The accepted CLI
Architecture defines the exact shared JSON result schema and exit mapping and
the implementation boundary; this behavior does not duplicate those mechanics
or claim their Gate 5 proof.

## Complete Typed Flow

```text
validated IDs or --all and exact source
  -> exact workspace and source disjointness
  -> Framework anchor and route-host coverage
  -> trusted Extension section and dependency closure
  -> baseline/current/intended semantic comparison
  -> intended authored topology and generated projection
  -> one complete dependency-first plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> lifecycle publication or reverse guarded recovery
  -> one typed result
```

No safe subset applies when a selected root, dependency, path, owner, route,
section, source, or recovery boundary is incomplete, blocked, ambiguous, or
unsafe.

## Request And Source Resolution

Normalize direct IDs, `--all`, one exact source, and independent Boolean flags.
Reject explicit IDs combined with `--all`, duplicate IDs, repeated singleton
source, unknown IDs, and terminal conflicts. `--all` selects all currently
managed IDs represented by the selected source universe and closure; it does not
select every package in the world and does not silently skip missing coverage.

Select exactly CWD or exact `--workspace`. Classify `--source` as one exact
package or catalogue and prove lexical and physical disjointness from the
workspace. When omitted, read the embedded catalogue. No network, registry,
cache, ambient search, glob, fuzzy matching, resemblance, or fallback source is
available.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

Every selected ID must be represented by readable source facts in the selected
universe. When the selected source contains exactly one completely validated
package and no IDs or `--all` were supplied, use its valid manifest ID as the
only deterministic inference in human, non-interactive, and automatic requests.
A multi-package source requires explicit IDs or `--all`. Automatic mode never
chooses among packages or broadens selection to all. A missing, duplicate,
malformed, or conflicting ID is invalid or blocked; it is never inferred from a
folder.

## Framework Anchor And Route Hosts

Before any managed mutation, establish a trustworthy installed Framework anchor
and complete facts for every affected authored host, generated boundary, route,
ownership relation, and cross-section preservation boundary. A missing safe fact
is `incomplete`; an unsafe or ambiguous fact is `blocked`. List and inspect may
read without this anchor, but update may not mutate around it.

## Lifecycle Trust And Dependency Closure

Read the `extensions` section of `.agents/open-forge.lifecycle.json`, schema v1.
Keep the `framework` section and common envelope isolated and preserve them
exactly on publication. Require supported versions and `open-forge-markdown-v1`,
exact workspace binding, stable ID and dependency reciprocity, target-relative
paths, shared owners, semantic baselines, duplicate-free identities, and complete
verifiable coverage. The document stores no plan, runtime history, journal,
recovery evidence, or session. Files outside this exact path are ordinary
workspace content, not lifecycle input.

An absent document or section is not, by itself, proof of unmanaged state.
Missing expected, malformed, unsupported, unverifiable, or inconsistent facts are
never treated as empty. Force and prune cannot promote them; safe unavailable
coverage is `incomplete` and unsafe ambiguity is `blocked`.

Resolve source manifests and dependency closure exactly, offline, transitively,
and within one source universe. Order dependencies before dependents. Reject
unknown IDs, duplicate active IDs or dependency declarations, invalid manifests,
cycles, unsafe package paths, incomplete closure, incompatible intended content,
and source identity conflicts before planning effects.

## Semantic Comparison

For each selected package path and owner set, collect current exact bytes and
semantic facts and compare persisted baseline, current state, and intended
source. Distinguish unchanged, new, changed, missing, retired, shared, unknown,
and source-unavailable paths.

Supported parseable kinds use the `open-forge-markdown-v1` conservative
parser/AST-derived syntax-aware fingerprints that
preserve Unicode, headings, tags, links and destinations, marker meaning, inline
text, code blocks, and semantically significant whitespace. Normalize only line
endings and parser-proven formatting trivia. Generated `Entries` interiors are
derived navigation and excluded from authored package identity. Unsupported,
binary, and unparseable kinds use exact-byte identity and fail closed.

Persist semantic baseline fingerprints, not exact-byte baseline digests. Capture
exact current bytes freshly for diff, expected-state revalidation, replacement,
deletion, verification, and recovery. Equal semantic identity with formatting-
only byte differences is an observation and does not create divergence or
shared-owner conflict. No formatter executes or produces persisted formatter
state.

## Ownership And Intended Topology

A managed package owns exact target-relative paths recorded in the trusted
section. Shared owners are explicit and retain compatible files when one owner
is removed later. Equal semantic content does not adopt an unowned existing file.
Retained dependents, route-host descendants, Framework ownership, user ownership,
unknown content, and other manager claims remain safety boundaries.

Form one hypothetical post-update workspace from current authored content plus
only effects admitted by the selected normal/force/prune authority. Preserve
user routes, overwrite companions, Framework files, unknown paths, and
intentionally absent defaults. Project affected generated regions from intended
authored topology and metadata through current Index behavior. Generated
interiors are not package-owned authored bytes and never come from stale package
lines. A missing, duplicate, reversed, nested, misplaced, or ambiguous generated
boundary blocks; no hidden index subprocess runs.

Reject payload targets under the lifecycle document,
`.git`, recovery/temporary artifacts, workspace overwrite companions, Framework
blocks, or another manager's paths. Source remains read-only.

## Normal, Force, Prune, And Automatic Plans

Normal mode may update baseline-unchanged current expected paths and add safe new
paths. It preserves changed current expected paths, missing current expected
paths, retired paths, and ownership or route divergence. Safe effects can apply
while complete coverage returns `attention` for finite preserved divergence.

Force admits only changed current expected path replacement and missing current
expected path restoration. It does not delete retired content or bypass any
ownership, route, marker, containment, Git, expected-state, verification, or
recovery boundary.

Prune admits only retired managed deletion when trusted baseline identity,
current source retirement, current semantic and physical identity, owner/route
safety, Git, backup, and verification facts all pass. It never deletes unknown,
unowned, shared, current expected, or unsafe content. Force and prune compose
only their two named effect classes.

Automatic mode suppresses interaction but selects no package beyond explicit
IDs, `--all`, or the permitted single-package manifest-ID inference. It adds no
force, prune, adoption, ownership change, or safety bypass. It may apply ordinary
safe effects already authorized by the request and preserves/reports divergence.

## Plan, Preflight, And Dry-Run

The complete plan records source and target identity, dependency order, manifest
facts, baseline/current/intended fingerprints, current exact bytes, owner sets,
route and generated effects, lifecycle publication, expected-state guards,
affected paths, Git policy, backup readiness, per-effect verification, and reverse
recovery. One failed selected condition blocks all effects. Before the first
workspace effect, obtain the actual OS lock for the visible
`.agents/open-forge.lock` path defined by the accepted CLI Architecture. File
existence is not lock ownership; another process holding the lock blocks
mutation. A crash releases the OS lock, and an unlocked file is reusable and may
be manually removed only when no process is active. The lock is not lifecycle
authority, history, or recovery evidence.

Dry-run and apply share request, facts, source closure, intended state, plan,
preflight, and pre-effect status. Dry-run includes every selected safe,
force-authorized, and prune-authorized effect and bounded diff, then writes no
payload, generated region, lifecycle section, backup, or temporary artifact. It
cannot prove application-time verification, publication, or
recovery.

Because dry-run performs no effects, it never produces an apply-time `failed` or
`interrupted` result. A planning or read failure and caller cancellation before
effects retain their own event meaning.

## Application, Verification, And Recovery

Preflight checks the exact source, workspace, Framework anchor, lifecycle
section, package IDs, dependency closure, paths, owners, routes, generated
markers, containment, expected bytes, Git cleanliness, backup readiness, and
recovery. `--skip-git-check` bypasses only affected-path cleanliness and requires
accepted adjacent backup recovery when existing bytes or eligible deletion need
it.

Immediately before effects, revalidate all volatile facts. Apply dependency-first
payload and generated effects, then publish the complete Extension-section
transition only after every effect and the whole operation verify. Preserve
unrelated lifecycle-section and envelope bytes. Remove backups only after
verification.

If an effect or verification fails, stop new effects and reverse applied effects
in reverse order only when identity guards match. Preserve concurrent edits and
residual recovery evidence. A recovery failure is `failed`; cancellation without
stronger residual failure is `interrupted`. Rerun from fresh facts; never replay
a saved plan or journal.

## Result Formation And Conformance

Form one typed result with exact source/workspace, selected roots and closure,
trust/coverage, baseline/current/intended facts, safe/preserved/overwritten/
restored/deleted/shared effects, generated projection, lifecycle publication,
Git/backup/verification/recovery, status, and one next action. Human and JSON
renderers consume it once. Use the Interface seven statuses, ordinary precedence,
streams, and one-result JSON rule.

Conformance must cover source and selection, dependency-first closure, Framework
anchor and route-host gates, trusted/untrusted/absent/source-unavailable facts,
semantic identity, shared owners and retained dependents, normal/force/prune/
automatic effects, generated navigation, reserved paths, complete plan, Git and
backup recovery, expected-state revalidation, verification, interruption,
dry-run parity, no-op repetition, human/JSON parity, and no formatter or source
mutation. The shared CLI Architecture defines the exact JSON result schema and
exit mapping. Gate 5 must prove source-generated serialization, fixed Markdig
where used, real `System.IO`, Native AOT, OS locking, isolated tests, and package
journeys.
