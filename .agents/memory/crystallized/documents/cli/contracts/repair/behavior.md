---
open-forge:
  description: Current technology-neutral exact and guided Repair lifecycle, safety, recovery, and conformance
  responsibility: Define how Repair resolves fresh diagnosis, plans one atomic mutation, verifies it, and preserves conflicts without selecting technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Repair, Behavior, Mutation, Safety, Recovery, CurrentTruth]
---

# Repair Behavior Contract

## Status And Boundary

This file is the accepted current Crystallized Behavior Contract for
`open-forge repair`. It defines deterministic request resolution, fresh
diagnosis, selection authority, exact and guided proposal handling, complete
planning, conflict checks, preflight, dry-run, application, verification,
typed Workspace Library residual recovery, recovery-bundle retention,
post-diagnosis, result formation, and conformance without
choosing implementation technology. The command does not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md).

The [Interface Contract](interface.md) owns the exact public grammar, selection
modes, relink value grammar, admitted catalogue, observable output, semantic
result names, errors, examples, non-goals, and public verification. This
Behavior Contract does not add flags, operands, aliases, or lifecycle command
syntax. The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
define the accepted shared JSON schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines source and runtime boundaries,
BCL-first filesystem structure, the workspace-lock boundary, and recovery
identity relationships. This file retains the technology-neutral concurrency, revalidation, and
recovery requirements behind that realization.

## Operation Flow And Invariants

Every general Repair invocation follows one typed mutation flow:

```text
validated request and selection authority
  -> fresh diagnosis and selected-edit relevant-domain gate
  -> selected safe-exact proposals and confirmed guided intent
  -> selected typed Library residual recovery effects when already authorized
  -> complete effect plan
  -> conflict check
  -> preflight, including recovery-bundle readiness
  -> exact dry-run or final interactive confirmation
  -> fresh revalidation
  -> apply
  -> per-effect and semantic verification
  -> recovery-disposition state when required
  -> fresh relevant-domain diagnosis
  -> one typed result
```

Dry-run uses the same request, facts, proposal resolution, plan, conflict check,
and preflight as application and stops before every persistent effect. A changed
selection or explicit intent discards any earlier in-memory plan and starts
planning again. There is no fixpoint loop and no saved plan replay.

The operation is stateless. It creates no session, receipt, saved report,
persistent proposal registry, transaction journal, or hidden mutation state.
It does not invoke a public command. Shared fact readers and planners may be
used directly, but `doctor`, `index`, route operations, cleanup, and lifecycle
operations are not called as subprocesses or hidden child operations.

## Request Normalization

Request normalization applies the exact Interface grammar and the shared Global
CLI Flags contract:

1. Resolve terminal `--help` or `--version` before workspace or domain work.
2. Reject positional operands, generic proposal references, choice tokens,
   finding text or codes, domain or kind modes, generic batch input, plugin
   fixer selectors, and every rejected alias or flag.
3. Parse `--automatic` and `--dry-run` as Boolean presence and collapse repeated
   presence to one idempotent choice. Unknown options are invalid.
4. Parse each `--relink` occurrence as exactly three shell values in order.
5. Deduplicate identical relink tuples and reject contradictory tuples for one
   source occurrence.
6. Resolve the exact current directory or exact `--workspace` value through the
   shared contract. Do not discover another workspace.
7. Classify the interaction as wizard, automatic, explicit, or automatic plus
   explicit according to the selection rules below.

Argument order does not select an effect. Compatible flags form one request;
conflicting explicit inputs form `invalid`. Repeated Boolean flags do not
multiply authority, preview, or recovery.

`--help` and `--version` terminate before workspace selection and mutation
resolution. They remain mutually exclusive. Other well-formed global flags may
be no-ops in those terminal modes, but Repair-specific input remains invalid.

## Selection Authority And Interaction

### Interactive wizard

When a human invokes bare `repair`, request resolution selects the wizard. The
wizard reruns the six-domain diagnosis, presents all current safe-exact
proposals, presents every applicable bounded guided candidate with evidence and
any recommendation-for-review, and leaves uncertain guided candidates
unselected.

The wizard initially offers the safe-exact set as the proposed exact selection.
Select, skip, and back change the in-memory typed request. A guided candidate
enters the request only after the user selects it. Cancel stops the operation
without a mutation effect and without creating a saved session.

The wizard then forms one plan, shows exact effects and safety facts, and reaches
the final confirmation boundary. The default answer is `No`. A `Yes` supplies
the final interactive application confirmation only after the complete plan and
preflight have succeeded. A `No` does not apply; it returns the accepted
non-applied interruption result without an unexpected application or
verification failure.

Interactive `repair --dry-run` uses the same diagnosis, selections, plan, and
exact effect display, then stops without an application confirmation. Dry-run
remains authoritative: no answer can cause a persistent effect and no plan is
saved.

The same wizard is selected for any prompt-capable request with neither
`--automatic` nor an explicit relink, including one that carries only
`--workspace`, `--view`, `--verbose`, or `--dry-run`. Those inputs remain part of
the same normalized request. JSON never enters this path;
help and version remain terminal before it.

### Automatic selection

When `--automatic` is present, normalization suppresses the wizard. The proposal
resolver selects every current safe-exact proposal in the admitted catalogue and
selects no guided candidate, recommendation, divergent replacement, deletion,
adoption, ownership change, or fuzzy choice. Application proceeds without a
prompt when `--dry-run` is absent; dry-run stops before effects.

The normalized Boolean is one selection authority regardless of repetition. If
no safe-exact proposal exists, the selected automatic scope is a verified empty
selection or no-op, while guided and manual findings remain unselected.
Those unselected guided findings are reported as remaining evidence and may form
`attention`; they are not silently resolved. A guided choice becomes a blocking
unresolved choice only when the current request explicitly requires that effect
and supplies neither a wizard selection nor an exact relink.

### Explicit relinks

Each explicit relink supplies user selection authority for one source occurrence.
It suppresses candidate selection for that occurrence. The resolver never
replaces the supplied target with a recommendation, path-order winner, or
fallback.

An explicit-relink request is direct in every environment. It does not open the
wizard or request another confirmation because the exact tuples supply selection
and application intent. It applies unless dry-run is selected; JSON and other
non-interactive requests follow the same path.

With both automatic and explicit authority, the resolver forms the union of all
current safe-exact proposals and all explicit relink tuples. The explicit tuple
remains authoritative for its occurrence. Automatic selection never supplies a
guided candidate for that occurrence.

### Non-interactive requests

JSON and other non-interactive requests never prompt. A non-interactive request
without `--automatic` and without an explicit relink has no selection authority.
It is `blocked`, whether or not `--dry-run` is present. Dry-run is a write
policy, not an implicit automatic-selection command. The useful next action
identifies `--automatic`, explicit relinks, or an interactive wizard.

Thus `--json --dry-run` without a selection is blocked, while
`--automatic --dry-run --json` is an explicit automatic preview. The resolver
never invents a guided choice to make a structured request complete.

## Fresh Relevant-Domain Diagnosis Gate

Before proposal selection, Repair may run all Doctor domains for current display,
but it separately forms the exact diagnosis dependency closure for each
selectable local-reference edit. It does not use a prior Doctor result, rendered
command output, saved report, or previous dry-run as current authority.

A selected edit requires complete current coverage only for the workspace/path
containment, route/heading, and local-reference facts it actually consumes.
Incomplete or blocked unrelated lifecycle, Extension, Framework, or recovery-
observer coverage stays visible but does not block the edit. Recovery writer
readiness for a real Replace is checked separately in mutation preflight. The
planner never uses a partial required fact or invokes another command's authority.

After application, the operation freshly rechecks the same relevant-domain
dependency closure. It reports repaired, remaining, new, manual, guided, and
blocked findings in that closure without claiming workspace health.

## Proposal Formation

Proposal formation consumes current typed Doctor findings and forms only the
first-release catalogue admitted by the Interface.

### Safe-exact proposals

The proposal resolver may form a safe-exact effect for:

- A local reference whose authored path spelling differs from the canonical path
  but resolves to the same target.
- A local reference whose case differs from the exact same target spelling and
  whose physical identity is unambiguous.
- A local reference whose encoding differs from the canonical same-target form
  and whose expected and intended authored bytes are exact.
- A local reference whose target is the same and whose fragment has one proven
  canonical correction.

Each safe-exact proposal contains the current expected authored literal, intended
after-state, affected path, exact verification condition, and recovery
requirement. It has no candidate choice and is eligible for `--automatic` only
when the current facts still prove all of those values.

### Typed Workspace Library residual proposals

The proposal resolver may also admit a `library.recovery-safe-exact` finding
only when a neutral producer has semantically verified current-v1 attribution to
the selected workspace's consumer Library record and one exact typed residual
entry. The existing automatic/guided selection shape supplies authority for the
safe-exact effect; no new public syntax or generic rollback selection is formed.
The accepted entries are:

- prior-missing ordinary Library record `Create`, with the exact intended
  created record/object as the only removable effect;
- ordinary-file `Replace` or `Delete` entry with exact prior bytes, restorable
  only at the exact logical path after the current state is proven intended; and
- relative-file-link `Create` or `Delete` entry. `Create` restores prior
  missing state by deleting only the exact intended created link/object.
  `Delete` restores the exact link only when the destination is exactly missing,
  using the stored raw relative target even when that target is dangling.

For every such proposal, revalidate no-follow logical parent, leaf, and
raw-target identity. Never follow, read, write, or delete the source target;
link raw target text is identity, not payload. A third, changed, unavailable,
aliased, ambiguous, or unsafe state blocks the proposal, and no filename or path
alone can form attribution, a record, or a recovery effect.

### Guided proposals

For a missing local target, the resolver may form a guided proposal from bounded
filename, title, literal-content, and structural route-neighborhood evidence.
It records zero, one, or several candidates and preserves each candidate's
evidence and provenance. It may identify one candidate as a recommendation for
review, but it never selects that candidate automatically. A user selection in
the wizard or an explicit relink supplies intent; fresh resolution still proves
the selected source, target, containment, expected bytes, intended bytes,
verification, and recovery.

No other finding creates a general Repair proposal. A valid reference, an
external URL, an image, a repeated or cyclic fact, generated drift, route intent,
recovery bundle or draft, Framework finding, Extension finding, authored decision, or
ownership conflict remains information, a targeted action, a manual decision, or
a blocked boundary, except for the explicitly typed
`library.recovery-safe-exact` residual lane above.

## Exact Relink Resolution

For each explicit relink, the resolver parses:

```text
<canonical-workspace-relative-markdown-path>@<one-based-line>:<one-based-column>
<exact-current-authored-destination>
<exact-contained-target-path>[#<optional-fragment>]
```

Shell quoting has already been removed before this stage. The resolver preserves
the resulting bytes and does not implement another quote or escape language.

The source-location path must identify a supported Markdown source inside the
selected workspace. If it is outside `.agents`, Doctor's accepted contained
local-reference boundary must include it. The one-based line and column must
identify the start of the authored destination token for exactly one current
local-reference occurrence. Parsing uses the terminal `@<line>:<column>` suffix;
an earlier `@` remains path text unless the terminal remainder is one valid
positive location suffix.

The expected destination must equal the complete current authored destination
literal at that occurrence. The target path must identify one current contained
target, with an optional Markdown fragment, inside the same accepted boundary.
It cannot be an ID, URL, glob, query, or semantic expression.

The first unencoded `#` separates the target path from a fragment. A literal
path-component `#` must use the supported Markdown destination encoding `%23`.
An empty fragment, second unencoded fragment delimiter, or query component is
invalid. A supplied fragment must be one non-empty supported Markdown fragment
spelling and resolve through the current target-heading facts.

The resolver computes the intended authored relative destination from the source
file's directory to the selected target and fragment. It plans only a literal
replacement. The link label and every unrelated byte remain outside the effect.

The resolver does not use a target recommendation as a substitute for the
explicit target. It still checks that the selected target is the current target
or current user-selected candidate and that identity, containment, and fragment
facts are safe.

The current finding must resolve to one admitted Interface catalogue member: a
same-target safe-exact correction, or a missing-target guided finding whose
current bounded candidate set includes the selected target. A valid unrelated
reference, an unproven semantic target change, a target outside the candidate
set, or another out-of-catalogue edit is blocked rather than converted into a
general-purpose relink.

If the source occurrence shifts, the old literal differs, the target or candidate
changes or disappears, identity or containment is ambiguous, or current bytes
match neither expected nor intended state, the resolver forms a blocked result.
It never searches for another occurrence, ranks another candidate, or chooses a
fallback.

## Plan Formation And Conflict Rules

After diagnosis and selection authority are complete, the planner resolves every
selected proposal and explicit relink against one fresh current fact set. It
forms one complete ordered plan before any effect. A selected safe-exact proposal
whose intended after-state already holds becomes a verified no-op and receives no
write effect.

The planner coalesces two effects only when all of these are identical:

- Addressed source byte range.
- Expected current state.
- Intended result.
- Verification condition.
- Recovery requirement.

Coalescing retains every originating Doctor finding, automatic origin, and
explicit tuple. Different automatic and explicit origins do not prevent
coalescing when the effect facts above are identical.

Several distinct relinks in one source resolve against one common expected
complete-file identity. Their destination ranges must not overlap. The planner
combines the literal replacements into one deterministic intended complete-file
state, one file effect, one verification boundary, and one recovery boundary,
while retaining every occurrence. Overlapping ranges, inconsistent expected
file identities, or an inability to prove one combined resulting file block the
complete plan. Revalidation checks the common expected file and every selected
occurrence; verification checks every intended destination and the complete
resulting file.

Contradictory tuples for one occurrence are invalid during request resolution.
Stale state, missing authority, missing recovery-bundle preparation, ambiguous
identity or containment, and overlapping non-equivalent effects block the
complete plan. No effect-order winner is chosen and no unrelated safe effect is
applied as a partial batch.

The plan records selected and unselected findings, effects and no-ops, affected
paths, expected and intended bytes, exact bounded diffs or equivalent evidence,
verification, recovery, and next actions. A previous plan is never reused.

For a selected Library residual, the plan records the trusted record identity,
typed entry class, exact prior/intended state, logical parent and leaf identity,
and raw relative-link target when applicable. It contains only the exact delete
of an intended created record/link/object, or the exact ordinary-file/link
recreation admitted by the Interface. It never contains a source-target write,
link follow, source-byte payload, inferred path, or third-state effect.

## Preflight And Recovery-Bundle Readiness

Preflight checks the selected-edit relevant-domain gate, selected authority, effect
boundaries, expected state, target and source identity, containment, overlap,
and recovery-bundle readiness. It completes before application authority is
crossed. For a plan with existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), readiness means exactly one immutable
ZIP bundle has been finalized and independently
verified outside the workspace under
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1`. No temporary,
repository, `HOME`, or custom platform fallback is allowed; unavailable storage
is `incomplete` before any write.

The bundle uses the normalized physical workspace path key and operation ID. Its
source-generated `manifest.json` uses the one public schema-v1 discriminator
value `1` and records command/operation/workspace identity, one required
immutable `attribution` object, every planned relative target and change kind,
exact prior lengths/hashes/payloads, and intended final absence/length/hash. The
attribution contains one finite producer, one finite operation, and a typed
subject `{kind, identity}`. The exact schema-v1 attribution vocabulary, valid
producer/operation/subject combinations, and required non-null workspace identity
are defined by the [Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
Repair's future writer supplies its own exact
producer, operation, and subject from trusted Repair-owned facts; it never
infers them from free command, GUID, path, filename, or ordered-entry values and
never uses a generic field bag. A schema-1 final missing or carrying invalid
attribution is malformed/unattributed and remains preserved, while an unknown
schema version is unsupported. The draft is created with `CreateNew` under its
exact name in the same external directory, closed and reopened for semantic
manifest, exact ordered entry names and counts, lengths, hashes, and bytes,
moved within the same directory to its deterministic final name, and reopened
and verified again. Only the valid current-v1 final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains exact-name, path-only
`Incomplete` support data, and observers do not inspect or use its bytes for
attribution. Create and no-op
effects have no entries, and every planned existing-target effect has exactly
one matching entry. All preparation completes before the first effect;
`FileChangeApplier` requires the matching preparation and performs one final
effect per target. Before workspace mutation, hold the persistent external
zero-byte workspace lock with one read/write `FileShare.None` handle; write no
metadata, timestamp, or ownership record and never truncate or delete it. The
lock is concurrency safety, not lifecycle, history, or recovery evidence.

A selected typed Library residual consumes the already semantically verified
current-v1 bundle entry as bounded recovery evidence. It is not reinterpreted as
a generic forward plan and does not manufacture an automatic rollback or
fixpoint; ordinary forward Repair effects, if present, retain their normal
recovery preparation rules.

For a Library residual, preflight additionally revalidates the current-v1 typed
attribution, exact record and entry, selected workspace key, no-follow logical
parent/leaf identity, and raw relative target. It admits only the recorded
prior-missing or intended states named by the entry. A third, changed,
unavailable, aliased, ambiguous, or unsafe state blocks before any effect; no
source target is opened,
written, or deleted, and no link capability is probed.

## Dry-Run Parity

Dry-run and application use the same normalized request, fresh relevant-domain facts,
proposal set, selected intent, plan, conflict checks, and preflight. Dry-run
forms the complete result and includes every selected automatic and explicit
effect, exact affected paths, expected and intended evidence, bounded diffs or
fingerprints, findings, conflicts, and required next actions.

Dry-run stops before recovery-bundle creation, temporary-file creation,
replacement, formatting, receipt update, or any other persistent effect. It
does not claim on-disk verification of bytes that were not written. Human output
may say Preview, but `--dry-run` is the only preview spelling.

## Application, Verification, And Recovery

When application authority is complete, the operation revalidates the complete
plan against current source occurrences, old literals, targets, expected bytes,
identity, containment, and recovery-bundle facts immediately before effects. A
changed fact blocks before that effect can write.

Each selected ordinary local-reference effect writes only the computed
destination literal in its addressed Markdown source. It preserves labels,
authored metadata, route topology, generated navigation, overwrite layers, and
unrelated bytes. It never replaces a divergent file or authors missing content.

For a selected Library residual, application performs only the typed no-follow
effect admitted by preflight: delete the exact intended created record/link/
object, restore exact ordinary prior bytes at its logical path, or recreate the
exact relative link from its stored raw target. It never opens, follows, writes,
or deletes the source target and never treats link target bytes as payload.
For this recovery lane, the plan's intended post-effect state is the recorded
prior state, and per-effect and whole-operation verification use that exact
state.

After each effect, the operation verifies the intended bytes and semantic local
reference relationship. After all effects, it verifies the complete selected
operation and its semantic postconditions. Before post-verification deletion
begins, a handled application, verification, or cancellation outcome stops new
effects and reports the actual residual draft or final path; a valid final
remains when preparation completed. Outside the selected typed Library residual
exception, Repair never restores, rolls back, or compensates for an effect, and
it never derives current target state from recovery provenance.

A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. A failed operation remains `failed`;
cancellation with incomplete residual facts is also `failed`, while cancellation
before effects without a stronger failure is `interrupted`. After final
verification of whole-operation success, delete the bundle.
The bundle deleted on success is Repair's newly prepared forward bundle. A
selected original Library residual ZIP remains byte-identical, including its
unselected entries; only explicit Cleanup may delete that original bundle.
`Deleted`/`Removed`
permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. When `Failed`/positively observed `Retained` recovery attention
coexists with remaining non-information findings, cleanup guidance owns the
single next action; those findings remain visible evidence. No journal, progress receipt, or persisted
plan is created. Explicit Cleanup owns
exact named final and draft deletion under its separate lease-bound contract;
unknown or differently named artifacts remain untouched.

Recovery storage is ordinary current-user `LocalApplicationData` under the
stable workspace and cooperating-client threat model. No special platform-
permission or encryption behavior is promised. Recovery reads use semantic
schema and exact ordered-entry validation and do not
extract bundles or add a custom archive parser, reflection, native dependency,
or package for this boundary.

## Fresh Post-Diagnosis

After a successful application, verified no-op, handled failure, or interrupted
boundary, Repair forms fresh relevant-domain diagnosis from current workspace facts.
It does not run until a fixpoint or treat the post-diagnosis as another
application request. The post-diagnosis reports:

- Effects selected, coalesced, applied, verified, retained, or unchanged.
- Findings selected and unselected.
- Findings repaired, remaining, new, manual, guided, and blocked.
- Affected paths, typed recovery state and disposition, and any residual path.
- Relevant-domain coverage and selected-scope status.

The result says `complete` only for the selected Repair scope when its required
coverage and effect path completed. Remaining non-information findings produce
`attention`; an incomplete or blocked post-diagnosis preserves that status and
does not claim workspace health.

## Result Formation And Presentation

Repair forms one typed result after invalid or blocked request resolution,
preflight, dry-run, application verification, recovery, or post-diagnosis. The
result contains the conceptual facts owned by the Interface: mode, workspace,
diagnosis coverage, findings and effect counts, selection origins, affected
paths, exact bounded change evidence, preflight, application, verification,
recovery, residuals, semantic status, and post-diagnosis coverage.

Human and JSON renderers consume that result without rerunning diagnosis,
planning, application, verification, or retained-state reporting. Compact and expanded views
change only framing and density. `--verbose` adds bounded diagnostics without
changing behavior or status. JSON is complete and non-interactive.

Primary human `complete`, `attention`, and `incomplete` results remain together
on stdout. Primary human `invalid`, `blocked`, `failed`, and `interrupted`
results remain together on stderr. JSON emits one complete structured result to
stdout for every semantic status; separate bounded diagnostics use stderr.

## Semantic Result Formation

Result formation preserves the Interface meanings:

- Invalid grammar, malformed values, or contradictory explicit tuples form
  `invalid`.
- Missing non-interactive selection authority, incomplete or blocked required
  relevant-domain diagnosis, stale or conflicting facts, malformed/colliding/mismatched
  recovery-bundle facts, ambiguous identity, or another unsafe complete-plan
  boundary forms `blocked`. Unavailable recovery storage or preparation coverage
  is `incomplete` before effects.
- Safe facts without complete required relevant-domain diagnosis or post-diagnosis coverage form
  `incomplete` when no stronger blocked boundary applies.
- Post-verification deletion `Failed` with positively observed disposition
  `Retained` forms `attention` while preserving the exact residual path and
  cleanup guidance.
- Application, verification, or post-condition failure, or post-verification
  deletion `Failed` with disposition `Unknown`, forms `failed`. That result
  preserves typed observed or unknown recovery facts and includes an exact
  expected path only when the recovery result provides one.
- Caller cancellation before completion forms `interrupted` unless bundle
  handling is incomplete, which forms `failed`.
- A selected scope that completes with remaining non-information findings forms
  `attention`.
- A selected scope that completes, including a verified no-op or exact dry-run,
  with only informational remaining findings forms `complete`.

The selector does not treat a byte change as attention by itself, does not treat
an unchanged target as an effect, and does not treat a recommendation as a
selected repair. A typed Library residual is selected only through its existing
safe-exact authority and reaches `complete` only after its exact no-follow
effect verifies. `complete` does not mean every Doctor finding is gone.

## Read-Only And Mutation Boundaries

The diagnosis, candidate, and planning stages have no persistent effects. The
application stage has authority only for selected admitted local-reference
literal effects or the typed Workspace Library residual effects above, after the
complete gate, preflight, confirmation where required, verified recovery-bundle
preparation, and revalidation.

Repair never mutates generated navigation, route topology, route metadata,
overwrites, recovery bundles or drafts, Framework files, Extension files, receipts,
manifests, ownership, or authored labels and prose. It never accepts a force,
apply, yes, preview, suggestions, or all flag as an authority shortcut.
The Library exception does not authorize source-target mutation, link following,
filename/path inference, a third-state effect, automatic rollback, or a fixpoint
loop.

## Behavioral Conformance

The mandatory public evidence boundary is the [Repair Public Verification](interface.md#public-verification)
section. A conforming implementation must additionally prove:

- One request normalizer for exact grammar, all six global flags, operation-
  specific automatic selection, relink triples, idempotent Boolean repetition,
  duplicate tuple deduplication, contradictory tuple invalidity, and no hidden
  precedence.
- Interactive wizard selection, safe-exact initial proposal set, guided
  candidate no-default behavior, select/skip/back/cancel, one plan, exact effect
  review, final confirmation default No, and dry-run no-application parity.
- Automatic selection of every current safe-exact proposal and rejection of all
  guided, recommendation, divergent, destructive, ownership, and fuzzy choices.
- Typed Library residual safe-exact selection and recovery for prior-missing
  ordinary-record Create, ordinary-file prior bytes, and relative-file-link
  Create/Delete entries, with no-follow parent/leaf/raw-target checks and
  rejection of third, unavailable, unsafe, or source-target effects.
- Non-interactive and JSON selection authority, including blocked bare and
  blocked `--dry-run` requests without automatic or explicit relink authority.
- Exact relink source occurrence, expected literal, contained target, relative
  destination computation, label and unrelated-byte preservation, stale state,
  shifted occurrence, target change, identity ambiguity, containment, and
  candidate disappearance.
- Fresh relevant-domain diagnosis as the selected-edit write gate and fresh
  post-diagnosis without a fixpoint loop; unrelated lifecycle and recovery-
  observer unavailability remains non-blocking.
- Safe-exact path, case, encoding, and unique-fragment effects; guided missing-
  target candidates from each admitted evidence basis; zero/one/several
  cardinality; and absence of external or semantic repair.
- One atomic plan, equivalent-effect coalescing with origin retention,
  contradictory and overlapping conflict blocking, stale state, missing
  authority, verified recovery-bundle preparation, and no partial application.
- Exact dry-run evidence and absence of recovery bundles, temporary files, writes,
  lifecycle, receipt, and public-command effects.
- Revalidation, per-effect verification, semantic verification, residual draft
  or final paths, interruption, and rerun convergence.
- Human compact and expanded, JSON, and verbose projections from one typed
  result, including effect and finding distinctions and the accepted stream
  policy without choosing exact schema or numeric exits.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  meanings scoped to selected Repair work.

Gate 5 executable proof should prove request, selection, candidate, plan, conflict,
preflight, no-op, dry-run, recovery-bundle preparation, verification, status, and
post-diagnosis behavior. Focused integration tests should use real isolated
Markdown workspaces, contained targets, changed bytes, recovery-bundle residuals,
interruption, and concurrent edits. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the structured
schema and process status. The [CLI Architecture](../../architecture.md) defines
the parser, filesystem identity, hashing, concurrency, and Native AOT boundaries;
exact atomic-file mechanics live in the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md). Gate 5 provides the
executable proof.

## Related Current Sources

- [Repair Interface Contract](interface.md)
- [Repair Command Contract Set](_repair.md)
- [Doctor Behavior Contract](../doctor/behavior.md)
- [Doctor Interface Contract](../doctor/interface.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [CLI Architecture](../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)

## Library Destination Permissions

Library recovery derives every destination from the recorded `sourceRoot`,
`destinationRoot` and source-relative suffix. It preserves source independence:
recorded source identity is enough and no source bytes are required. The
permission control-file entry is excluded from automatic Library residual
attribution. A permission receipt may remain in the existing recovery bundle as
evidence, but Repair does not restore, revoke or widen grants.

Preflight and application under the held workspace lease require current
consumer permission for every selected external Library link effect. Revocation,
an invalid or unavailable permission observation, or source-binding mismatch
blocks the selected recovery. Existing `.agents` implicit permission remains;
protected controls, all selected/registered source trees, exact no-follow link
identity and collision checks still apply to each actual effect. Recovery adds
no permission prompt or broad grant. Original residual bytes remain unchanged.
