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
reverse recovery, post-diagnosis, result formation, and conformance without
choosing implementation technology. The command does not ship yet; implementation
and executable proof remain pending Gate 5.

The [Interface Contract](interface.md) owns the exact public grammar, selection
modes, relink value grammar, admitted catalogue, observable output, semantic
result names, errors, examples, non-goals, and public verification. This
Behavior Contract does not add flags, operands, aliases, or lifecycle command
syntax. The [CLI Architecture](../../architecture.md) defines the accepted
shared JSON schema, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, workspace lock, and recovery identity
model. This file retains the technology-neutral concurrency, revalidation, and
recovery requirements behind that realization.

## Operation Flow And Invariants

Every general Repair invocation follows one typed mutation flow:

```text
validated request and selection authority
  -> fresh six-domain diagnosis
  -> selected safe-exact proposals and confirmed guided intent
  -> complete effect plan
  -> conflict check
  -> preflight, including affected-path Git and recovery policy
  -> exact dry-run or final interactive confirmation
  -> fresh revalidation
  -> apply
  -> per-effect and semantic verification
  -> reverse recovery when required
  -> fresh six-domain diagnosis
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
3. Parse `--automatic`, `--dry-run`, and `--skip-git-check` as Boolean presence
   and collapse repeated presence to one idempotent choice.
4. Parse each `--relink` occurrence as exactly three shell values in order.
5. Deduplicate identical relink tuples and reject contradictory tuples for one
   source occurrence.
6. Resolve the exact current directory or exact `--workspace` value through the
   shared contract. Do not discover another workspace.
7. Classify the interaction as wizard, automatic, explicit, or automatic plus
   explicit according to the selection rules below.

Argument order does not select an effect. Compatible flags form one request;
conflicting explicit inputs form `invalid`. Repeated Boolean flags do not
multiply authority, preview, Git bypass, or recovery.

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
non-applied interruption result without residual recovery failure.

Interactive `repair --dry-run` uses the same diagnosis, selections, plan, and
exact effect display, then stops without an application confirmation. Dry-run
remains authoritative: no answer can cause a persistent effect and no plan is
saved.

The same wizard is selected for any prompt-capable request with neither
`--automatic` nor an explicit relink, including one that carries only
`--workspace`, `--view`, `--verbose`, `--skip-git-check`, or `--dry-run`. Those
inputs remain part of the same normalized request. JSON never enters this path;
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

## Fresh Six-Domain Diagnosis Gate

Before proposal selection, Repair runs all six Doctor domains in their fixed
order and retains their complete reports, findings, limitations, and coverage.
It does not use a prior Doctor result, rendered command output, saved report,
or previous dry-run as current authority.

General Repair writes require all six Doctor domains to have complete coverage.
If any one of the six domains has incomplete or blocked coverage, all general
Repair writes are blocked, including explicit relinks. The planner may retain
partial facts and display safe findings, but it cannot apply an effect under the
incomplete diagnosis gate. A targeted command can have its own contract and
boundary; this operation does not invoke it.

The diagnosis gate is checked again after application through fresh six-domain
diagnosis. The second diagnosis reports repaired, remaining, new, manual,
guided, and blocked findings without claiming that selected-scope completion is
workspace health.

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
recovery artifact, Framework finding, Extension finding, authored decision, or
ownership conflict remains information, a targeted action, a manual decision, or
a blocked boundary.

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
Stale state, dirty affected paths, missing authority, missing recovery,
ambiguous identity or containment, and overlapping non-equivalent effects block
the complete plan. No effect-order winner is chosen and no unrelated safe effect
is applied as a partial batch.

The plan records selected and unselected findings, effects and no-ops, affected
paths, expected and intended bytes, exact bounded diffs or equivalent evidence,
verification, recovery, and next actions. A previous plan is never reused.

## Preflight And Git Policy

Preflight checks the complete diagnosis gate, selected authority, effect
boundaries, expected state, target and source identity, containment, overlap,
affected paths, recovery readiness, and the applicable Git cleanliness policy.
It completes before application authority is crossed.

For an actual update, Git cleanliness is checked only for existing affected paths.
Dirty affected paths block by default. Read-only inputs that are not changed do
not become dirty affected paths merely because they contributed to a proposal.

The normalized presence of `--skip-git-check` bypasses only this affected-path
cleanliness check. It activates the required adjacent backup recovery boundary.
It does not bypass diagnosis completeness, expected-state checks, containment,
identity, overlap, verification, recovery readiness, or ownership.

A Gitless execution still requires safe adjacent recovery for each replacement.
An unavailable, unknown, or colliding required backup boundary blocks the entire
plan before the first write. A verified no-op has no affected mutation path and
does not require a Git cleanliness check.

## Dry-Run Parity

Dry-run and application use the same normalized request, fresh six-domain facts,
proposal set, selected intent, plan, conflict checks, and preflight. Dry-run
forms the complete result and includes every selected automatic and explicit
effect, exact affected paths, expected and intended evidence, bounded diffs or
fingerprints, findings, conflicts, and required next actions.

Dry-run stops before backup creation, temporary-file creation, replacement,
formatting, receipt update, Git change, or any other persistent effect. It does
not claim on-disk verification of bytes that were not written. Human output may
say Preview, but `--dry-run` is the only preview spelling.

## Application, Verification, And Recovery

When application authority is complete, the operation revalidates the complete
plan against current source occurrences, old literals, targets, expected bytes,
identity, containment, and recovery immediately before effects. A changed fact
blocks before that effect can write.

Each selected non-no-op effect writes only the computed destination literal in
its addressed Markdown source. It preserves labels, authored metadata, route
topology, generated navigation, overwrite layers, and unrelated bytes. It never
replaces a divergent file or authors missing content.

After each effect, the operation verifies the intended bytes and semantic local
reference relationship. After all effects, it verifies the complete selected
operation and its semantic postconditions. If application or verification fails,
new effects stop and already applied effects are recovered in reverse effect
order when their current identity still matches the applied state.

Recovery never overwrites a concurrent or otherwise changed target. Such a
target is preserved and reported as residual state. A failed operation remains
`failed` even when handled recovery succeeds because the requested Repair did
not complete. An interruption with incomplete recovery is `failed` and reports
retained backups and residuals. An interruption before effects or after complete
recovery is `interrupted`.

Backups are removed only after complete application and semantic verification.
Repair does not delete pre-existing or unknown recovery artifacts.

## Fresh Post-Diagnosis

After a successful application, verified no-op, handled failure, or interrupted
boundary, Repair forms fresh six-domain diagnosis from current workspace facts.
It does not run until a fixpoint or treat the post-diagnosis as another
application request. The post-diagnosis reports:

- Effects selected, coalesced, applied, verified, reverted, or unchanged.
- Findings selected and unselected.
- Findings repaired, remaining, new, manual, guided, and blocked.
- Affected paths and residual or retained recovery state.
- Per-domain coverage and aggregate status.

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
planning, application, verification, or recovery. Compact and expanded views
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
  diagnosis, stale or conflicting facts, dirty affected paths, missing recovery,
  ambiguous identity, or another unsafe complete-plan boundary forms `blocked`.
- Safe facts without complete required diagnosis or post-diagnosis coverage form
  `incomplete` when no stronger blocked boundary applies.
- Application, verification, recovery, or post-condition failure forms `failed`.
- Caller cancellation before completion forms `interrupted` unless recovery is
  incomplete, which forms `failed`.
- A selected scope that completes with remaining non-information findings forms
  `attention`.
- A selected scope that completes, including a verified no-op or exact dry-run,
  with only informational remaining findings forms `complete`.

The selector does not treat a byte change as attention by itself, does not treat
an unchanged target as an effect, and does not treat a recommendation as a
selected repair. `complete` does not mean every Doctor finding is gone.

## Read-Only And Mutation Boundaries

The diagnosis, candidate, and planning stages have no persistent effects. The
application stage has authority only for selected admitted local-reference
literal effects after the complete gate, preflight, confirmation where required,
Git policy, and revalidation.

Repair never mutates generated navigation, route topology, route metadata,
overwrites, recovery artifacts, Framework files, Extension files, receipts,
manifests, ownership, or authored labels and prose. It never accepts a force,
apply, yes, preview, suggestions, or all flag as an authority shortcut.

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
- Non-interactive and JSON selection authority, including blocked bare and
  blocked `--dry-run` requests without automatic or explicit relink authority.
- Exact relink source occurrence, expected literal, contained target, relative
  destination computation, label and unrelated-byte preservation, stale state,
  shifted occurrence, target change, identity ambiguity, containment, and
  candidate disappearance.
- Fresh six-domain diagnosis as a strict complete-coverage write gate and fresh
  post-diagnosis without a fixpoint loop.
- Safe-exact path, case, encoding, and unique-fragment effects; guided missing-
  target candidates from each admitted evidence basis; zero/one/several
  cardinality; and absence of external or semantic repair.
- One atomic plan, equivalent-effect coalescing with origin retention,
  contradictory and overlapping conflict blocking, stale and dirty state,
  missing authority, missing recovery, Git and Gitless policy, and no partial
  application.
- Exact dry-run evidence and absence of backups, temporary files, writes, Git,
  lifecycle, receipt, and public-command effects.
- Revalidation, per-effect verification, semantic verification, reverse
  recovery, residual preservation, interruption, and rerun convergence.
- Human compact and expanded, JSON, and verbose projections from one typed
  result, including effect and finding distinctions and the accepted stream
  policy without choosing exact schema or numeric exits.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  meanings scoped to selected Repair work.

Gate 5 executable proof should prove request, selection, candidate, plan, conflict,
preflight, no-op, dry-run, verification, recovery, status, and post-diagnosis
behavior. Focused integration tests should use real temporary Markdown sources,
contained targets, changed bytes, Git and Gitless states, recovery artifacts,
interruption, and concurrent edits. The CLI Architecture defines the accepted
parser, atomic replacement, filesystem identity, hashing, concurrency, structured
schema, process-status, package, and Native AOT boundaries. Gate 5 provides the
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
- [CLI Decision Agenda](../../../../../working/cli-release/decision-agenda.md)
- [CLI Release Plan](../../../../../working/cli-release/release-plan.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
