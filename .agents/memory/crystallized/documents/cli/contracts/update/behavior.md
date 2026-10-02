---
open-forge:
  description: Accepted technology-neutral behavior for Framework ownership update planning, ordinary replacement, explicit prune deletion, and recovery
  responsibility: Define update's deterministic fact flow, semantic comparison, complete plan, guarded effects, and typed result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Update, Framework, Behavior, Determinism, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Update Behavior Contract

## Ownership Receipt Formation

Form whole-file and region receipts according to what the operation manages.
The `open-forge` blocks in root `AGENTS.md` and `CLAUDE.md` produce region
receipts even when creating a previously missing host requires a physical file
creation. Generated Entries produce `entries` region receipts. Preserve existing
verified ownership when a planned no-op leaves its bytes unchanged. Publish the
ownership lock after the operation's target effects verify; a region-only edit
never establishes whole-file ownership of its authored host. Skip an unavailable
lock write without blocking the operation.

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for the current
`open-forge update` command. It defines deterministic request resolution,
Framework ownership receipts, current/intended comparison, semantic
fingerprints, generated projection, normal/force/prune planning, preflight,
dry-run, application, verification, recovery, result formation, and
technology-neutral conformance.

The [Interface Contract](interface.md) defines public syntax, flags, statuses,
output, errors, examples, and non-goals. `install` owns establishment and
initial force. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the shared JSON
result schema and exit mapping. The [Ownership And Source Alignment Technical
Design](../../technical-designs/lifecycle-provenance.md) defines ownership serialization and current source alignment, while the [CLI Architecture](../../architecture.md)
defines cross-cutting implementation structure. This behavior does not duplicate
those mechanics or claim their Gate 5 proof.

## Complete Typed Flow

```text
validated command input
  -> exact workspace and embedded current Framework
  -> forgiving Framework ownership observation
  -> fresh current/intended facts
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

After a complete plan and preflight, a prompt-capable human apply with at least
one effect and without `--automatic` asks exactly once before lease acquisition.
Trimmed `y` and `yes` answers are accepted case-insensitively. Refusal, end of
 input, or cancellation returns `cancelled` without a lease, recovery bundle,
 or workspace write. Verified no-op, effect-free `completed-with-warnings`, dry-run, automatic,
JSON, and redirected or otherwise non-prompt-capable execution never prompt or
 consume input. A non-prompt-capable human apply that would write is `invalid-input`
without explicit automatic mode. Automatic never supplies force or prune.

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

Read the forgiving `.agents/open-forge.lock.json` ownership record. Missing,
stale, unreadable or unknown schema metadata never establishes an integrity
gate. Unknown ownership contributes no claims, and matching existing bytes do
not establish new receipts. Uninterpretable recorded destinations produce
`update.ownership-observation` (info, no inferred effects).

Only actual Framework file or region effects establish new receipts. The single
lock publication preserves unaffected Framework receipts and Extension/Library
sections. A skipped publication reports none/not-requested; verification never
claims skipped ownership was published. No legacy file is read, converted,
rewritten or deleted.

The `.agents` container must be an ordinary contained directory. Reserved paths,
portable aliases, destination admission, other owners, target identity and
managed/generated boundaries remain safety checks. Current payload alignment
identifies intended source at operation time; no source path, baseline hash,
workspace binding or fingerprint policy is stored in ownership.

### Local Route Adoption During Update

Update may form local-route adoption candidates only when the ownership
observation is complete and trustworthy and the Framework registration is
present. It considers compatible documents under selected standard local
routes. Candidate paths are filtered through
`FrameworkPayloadSelection.IncludesPath`, including all exact-file,
directory-descendant, and category exclusions. The complete source catalogue
and strict projection inputs remain intact so excluded, malformed, foreign,
ambiguous, or unsafe facts continue to produce the existing block diagnostics;
they never become guessed effects. An absent, unknown, incomplete, or
unregistered Framework state cannot be established through adoption.

## Semantic Comparison

Compare fresh current content with intended embedded or generated content.
No stored baseline participates. `same` means semantic agreement, `changed`
means disagreement, `missing` means actual absence, and `format-only` means
semantic agreement with different exact bytes. Source alignment is nullable
for retired whole-file receipts and derived regions.

Supported Markdown uses conservative parser-derived semantic identity, excluding
generated Entries interiors from authored identity. Unsupported content uses
exact bytes. Fresh exact bytes support expected-state checks, recovery and
verification; no comparison hash is persisted in ownership.

## Intended State And Projection

The planner forms one hypothetical post-update workspace from current authored
content plus only the effects admitted by normal, force, prune, and bounded
local-route adoption authority. It preserves user-added routes, overwrite
companions, intentionally absent defaults, Extension paths, unknown files, and
all outside bytes.

For an eligible managed workspace, shared topology and document planners form
adoption route facts and intended document bytes before generated projection.
They complete only missing required native Skill fields and required resource
catalogue documents, retaining valid or unknown frontmatter, optional fields,
body bytes, encoding, newlines, and companion files. Metadata and generated
Entries changes for one path become one exact physical effect. Adoption targets
remain separate from owned Framework comparisons; local authored files have
no Framework source-asset path, are not counted as bundled payload, and receive
no whole-file Framework receipt. A verified generated Entries region may
receive its own region receipt. This bounded stage does not change Index, the
global parser, or the global projector.

The current Index projection derives every affected generated `Entries` body from
that intended authored topology and metadata. Generated lines never supply
topology or metadata, and package or embedded generated interiors are not copied.
Only valid bounded generated interiors may change. A missing, duplicate,
reversed, nested, misplaced, or ambiguous boundary blocks before the first
effect. Update never starts a hidden index subprocess.

Root/provider planning admits only absent-host creation when supported, bounded
append to a valid host with no boundary candidate, or replacement inside
exactly one valid canonical heading and footer pair or exact legacy
comment pair. Markdig parsed root-level blocks and typed source spans provide
the boundary facts. Fenced code blocks, quoted blocks, nested lists, and inline
examples are content, not delimiters. A complete legacy pair is input-only.
Normal and `--automatic` Update convert an owned legacy host to the canonical
heading and footer form under the existing ownership, recovery, preview,
confirmation, and expected-state rules. Dry-run reports that conversion without
writing. No migration flag or additional force requirement is needed. Missing,
reversed, duplicate, mixed, or otherwise ambiguous boundaries
block the complete plan. A heading without its named footer never captures the
user suffix or the end of the file. Outside bytes remain unchanged. Index is
unchanged and remains navigation-only. Force is not managed-host boundary-repair
authority.

## Normal Plan

Ordinary Update replaces edited owned Framework content, restores missing owned
current targets, creates safe absent current targets, adopts compatible local
route documents under the admission above, and projects affected navigation.
Semantic equality preserves current formatting. For an owned root host in the
exact legacy form, ordinary Update treats conversion to the
canonical heading and footer form as the normal replacement. `--automatic`
selects that replacement under the same safety rules, and preview reports it
without applying it. `removedCategories`
excludes payloads beneath each named root category from reinstatement.
`removedFiles` excludes exact canonical workspace-relative file destinations
from whole-file and generated-region planning, including root managed hosts. The
list is not glob or recursive-directory syntax. `removedDirectories` excludes
each canonical directory and all its descendants, including future files.
Force, prune, and automatic mode
do not bypass it. Existing content and receipts outside selected effects remain.
Removing an entry explicitly allows a later Update to restore that destination.

For an already installed Framework with a readable registration, that restoration
includes ordinary missing parent directories required by the selected current
payload. Plan each directory creation, show it in preview and results, and
revalidate its absence and contained ancestors under the same lease before
creating it. A directory is not created when an exclusion covers it. Links,
aliases, conflicting files and other unsafe ancestors block the plan. An empty
`.agents` directory without Framework registration does not authorize this
restoration behavior. Schema or release metadata alone does not invalidate an
otherwise readable registration.

Whole-file receipts authorize whole-file effects. Region receipts authorize
only the named block, preserving outside host bytes. Existing unowned content
is never adopted merely because it matches; a conflicting unowned destination
blocks the complete plan.

### Force

`--force` remains accepted but grants no additional authority or safety bypass.

### Prune

`--prune` additionally removes retired whole-file receipts whose paths still
exist, are admitted by shared `allowInstallPaths` (`.agents/` is implicit), and
pass reserved-path, physical identity, other-owner and recovery checks. Edited
retired content remains eligible. An absent retired path has no deletion effect;
its obsolete receipt may be released. A region receipt never authorizes deleting
its host. Without prune, present retired content is preserved with
`completed-with-warnings` while independent safe updates may proceed.

Changed owned content and missing owned content are represented by the normal
replacement and restoration effects and their counts. The root Update findings
catalogue has no separate finding for either condition; neither one blocks the
ordinary reconciliation.

## Automatic And Dry-Run

Automatic mode selects only safe normal effects for the fixed Framework subject.
It updates owned current content and preserves retired content unless prune
was explicitly selected. It never selects extra flags or bypasses safety.

Dry-run consumes the same complete plan and preflight as application. It reports
safe replacements/restorations, explicit prune deletions, preserved facts,
semantic comparisons, generated projections, lifecycle changes that would be
published after verification, recovery-bundle requirements, and diagnostics.
It writes no payload, generated region, lifecycle section, recovery bundle, temporary
artifact, or formatter output. It cannot claim application-time revalidation,
verification, publication, or bundle-handling success. It forms the same pre-effect
planning status as application but never produces an apply-time `failed` or
`cancelled` result because it performs no effects. A planning or read failure
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

For a prompt-capable human apply that would write, confirmation occurs after
this complete preflight and before the workspace lease or recovery preparation.
Confirmation continues the already formed plan; it never replans or widens
authority.

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
Framework projection, then publishes effect-backed Framework ownership atomically, preserving
unrelated sections in one canonical UTF-8 lock. Unchanged canonical receipts
need no publication.

Adoption uses this same complete preflight, lease, exact-byte revalidation,
recovery, application, and verification pipeline. A user-authored adoption
effect remains separate from Framework-owned payload comparisons and receipts.

After target and ownership verification, reopen and verify the recovery bundle
identity and retain it. Successful retention is complete with the exact bundle
path. When an ordinary `.git` directory exists, advise `git diff`; otherwise
point at the recovery bundle. No Git executable runs. Interruption or partial
failure retains recovery and reports observed residual state. The foundation
never restores automatically; explicit Cleanup owns deletion separately.
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
current/intended comparisons, safe and authority-selected effects, path-sorted
adoption migrations, preserved retired content, generated projection,
lifecycle publication,
recovery-bundle facts, verification, residuals, status, and at most one next
action.
Human and JSON renderers consume it once.

Each migration retains its path, actions, metadata fields, derivation sources,
and `planned` or `applied` outcome. Only the matching verified non-directory
physical effect permits `applied`; preview, cancellation, unstarted work, and
unverified effects remain `planned`. Migration facts do not add findings,
warnings, ownership, or history and do not change status.

The typed result forms exactly the ordered command-local JSON graph frozen by
the Interface: mode, force, prune, automatic, atomic nullable source,
comparisons, atomic nullable generated navigation, effects, lifecycle, recovery,
verification, and findings. All top-level properties are present for every
status, and arrays are empty rather than null. Each physical effect contains its
ordered logical authored and generated changes, so one physical path has one
application outcome and one recovery entry without hiding the logical
comparison meaning. Ownership receipts remain limited to verified Framework
payloads and authorized generated regions. Findings expose exactly `code`,
nullable `target`, and non-empty `cause`; their status comes from the finite
Interface code map rather than a duplicated wire member. Finding order follows
that declaration sequence, then null target before ordinal non-null target,
then ordinal cause.

Result formation uses `invalid-input` before operation work; then ordinary
precedence `blocked` > `incomplete` > `completed-with-warnings` > `completed`.
`failed` and `cancelled` retain event meaning. Completed includes a verified
no-op and a complete dry-run; completed-with-warnings reflects preserved retired content, not merely planned effects,
force/prune flags, or format-only observations.

Primary human completed/completed-with-warnings/incomplete results use stdout.
Primary human invalid-input/blocked/failed/cancelled results use stderr. JSON
emits one schema-3 result on stdout for every status, and bounded diagnostics use
stderr.

## Behavioral Conformance

Evidence covers syntax and confirmation, current/intended comparison, ordinary
replacement/restoration, explicit prune, removed categories, unowned content,
shared destinations, region-only ownership, portable and reserved paths, stale
and absent receipts, and unchanged bytes outside selected effects. Recovery
checks prove preparation before deletion, exact prior bytes, retention, final
ownership verification and fresh reruns. Snapshots cover the existing single
publication outcome and both views; native checks verify the same behavior.
Dry-run writes nothing. No formatter or Git executable runs.

Final verification may retain an informational ownership observation when lock
publication was skipped. It still verifies an effect-free intended projection,
the same raw ownership read state and expectation, and every applied content
effect. When a lock write was planned, its exact intended bytes must verify.

Managed-host evidence covers canonical heading and footer output for both hosts,
exact legacy comment input, normal and automatic Update conversion, dry-run
preview, source/dogfood parity, unchanged prefix and suffix bytes, and
root-level parsed spans. Incomplete, reversed, duplicate, mixed, and otherwise
ambiguous boundaries block without writes, and a heading without its footer
never captures the user suffix or end-of-file.

Local-route evidence proves complete-ownership and Framework-presence admission,
full exclusion filtering with strict source and projection inputs, preservation
of optional and unknown document content, one exact physical effect for
combined metadata and Entries changes, and separation from Framework payload
provenance/counts and whole-file ownership. Dry-run, cancellation, apply,
partial failure, repeat Update, malformed or foreign facts, and excluded files
retain the same lease, recovery, application, verification, and zero-effect
block guarantees.

Directory creation follows the installed-Framework and exclusion boundaries in
[Normal Plan](#normal-plan). A missing retired path requires neither parent
creation nor deletion.
