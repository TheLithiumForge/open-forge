---
open-forge:
  description: Current accepted interface for automatic, guided, explicit-relink, and dry-run Repair
  responsibility: Define the Repair public grammar, selection authority, finite catalogue, results, errors, and examples
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Repair, Interface, Mutation, Relink, Safety, CurrentTruth]
---

# Repair Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the public `repair`
Interface Contract. The command does not ship yet; implementation and executable
proof remain pending Gate 5. This contract owns the exact
syntax, flags, value grammar, repetition and composition, interactive and
non-interactive selection paths, admitted repair catalogue, output projections,
semantic results, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral mutation lifecycle behind this surface. The [Doctor
Interface Contract](../doctor/interface.md) defines the six-domain diagnosis,
finding kinds, coverage, candidate evidence, and local-reference boundary that
Repair consumes. The [Global CLI Flags](../shared/global-flags/interface.md)
contract defines the six global flags once. The [Source References](../shared/source-references/interface.md)
contract remains authoritative for any source identity that is reported as
diagnostic evidence; `--relink` uses its own exact occurrence grammar below.

The [CLI Architecture](../../architecture.md) defines the accepted shared
structured schema, process-status mapping, source structure, package and runtime
boundaries, BCL-first filesystem boundary, workspace lock, and recovery identity
model. No command-local Technical Design file exists for Repair. This Interface
Contract remains technology-neutral.

## Purpose And Boundary

`repair` applies only current diagnosis-backed, conflict-free, exact
meaning-preserving local corrections and explicitly selected contained relinks.
It does not author content, labels, metadata, generated navigation, route
topology, recovery cleanup, Framework lifecycle, Extension lifecycle, or
ownership decisions. It does not promise to eliminate every Doctor finding.

Repair is one mutation operation with a single plan. It is stateless and
deterministic for unchanged workspace bytes and explicit input. It reruns current
diagnosis for every invocation and never treats a rendered finding, previous
report, saved plan, message, or display code as authority.

## Exact Syntax

The complete public command form is:

```text
open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global flags]
```

The six and only six global flags are:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

`--automatic`, `--relink`, and `--dry-run` are
Repair-specific flags. `--automatic` is not global. All six global flags apply
to Repair under the shared contract, including its exact workspace selection,
human view, JSON, diagnostics, and terminal help and version behavior.

Repair has no positional operands. It accepts no generic proposal reference,
choice token, finding text or code, domain or kind mode, generic batch input,
plugin fixer registry, `guided` child, `--yes`, `--preview`, `--suggestions`,
`--all`, `--force`, or `--apply` flag. It has no saved plan, report, session,
receipt, or second `fix` spelling.

## Flag Grammar And Composition

| Input                                                             | Role                                                          | Value and omission                                                               | Repetition and composition                                                                                    |
| ----------------------------------------------------------------- | ------------------------------------------------------------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `--automatic`                                                     | Automatic selection and non-wizard mode                       | Boolean; omission leaves selection to explicit relinks or the interactive wizard | Repeatable and idempotent. It selects every current safe-exact proposal and never selects a guided candidate. |
| `--relink <source-location> <expected-destination> <target-path>` | Explicit local-reference selection and user intent            | Exactly three shell values per occurrence                                        | Repeatable. Identical triples deduplicate. Contradictory tuples for the same occurrence are invalid.          |
| `--dry-run`                                                       | Sole preview policy                                           | Boolean; omission permits application when selection and authority are complete  | Repeatable and idempotent. It never creates persistent effects.                                               |
| global flags                                                      | Workspace, presentation, diagnostics, or terminal information | The six shared values                                                            | Shared defaults, repetition, terminal, and composition rules apply.                                           |

Explicit inputs compose incrementally. `--automatic` adds all current safe-exact
proposals to any explicit relinks. An explicit relink supplies the selection for
its addressed occurrence and suppresses candidate selection for that occurrence.
There is no hidden precedence, disjunctive mode, order winner, or fallback
choice. An input that conflicts with another explicit input is invalid.

Repeated Boolean presence does not multiply selection authority, preview, or
recovery. A duplicate explicit tuple does not multiply an effect or change its
provenance.

`--help` and `--version` remain terminal informational modes under the shared
global contract. They are mutually exclusive and do not run diagnosis or
planning. Domain input, Repair-specific flags, and incomplete relink tuples
remain invalid with either terminal mode. Other well-formed global flags may be
no-ops in terminal mode as defined by the shared contract.

## `--relink` Value Contract

Each `--relink` occurrence has exactly three shell values. Quote the complete
value when it contains spaces, `@`, `:`, `#`, shell punctuation, or a destination
that would otherwise be split:

```text
--relink <source-location> <expected-destination> <target-path>
```

### Source location

The first value is a canonical workspace-relative Markdown source location with
a one-based line and one-based column:

```text
<path>@<line>:<column>
```

For example:

```text
.agents/docs/guide.md@12:8
```

The path uses canonical `/` separators and identifies the authored Markdown file
that contains one local-reference occurrence. The line and column identify the
occurrence in the current source. This value is not a source ID, opaque token, or
finding code. A source outside `.agents` is admitted only when Doctor's accepted
contained local-reference boundary includes it.

The location is parsed from the terminal `@<line>:<column>` suffix. An `@`
inside the path remains part of the path unless the remaining terminal text is
one valid positive line-and-column suffix. The line and column identify the
start of the authored destination token for the supported reference occurrence.

### Expected destination

The second value is the exact expected current authored destination string. It is
one shell value and preserves the complete old literal, including relative path,
encoding, fragment, spaces, and other supported authored punctuation. It is not
normalized before comparison and is not a candidate selector.

### Selected target path

The third value is the exact selected contained workspace-relative target path,
with an optional Markdown fragment:

```text
<path>[#<markdown-fragment>]
```

It is a canonical workspace-relative path, not a URL, source ID, glob, query,
fuzzy expression, or opaque choice token. The path and optional fragment must be
inside the [Doctor accepted contained local-reference boundary](../doctor/interface.md#local-reference-boundary).
A source or target outside `.agents` is allowed only within that boundary. A target that is missing,
ambiguous, outside the workspace, physically aliased, or unsupported blocks the
request.

The first literal `#` separates the target path from its optional fragment. A
literal `#` in a path component must use the supported Markdown destination
encoding `%23`; an unencoded `#` is always the fragment delimiter. A query
component, a second unencoded fragment delimiter, or an empty fragment after `#`
is invalid. A supplied fragment is one non-empty supported Markdown fragment
spelling and must resolve under the current target-heading facts.

Repair computes the correct authored relative destination from the source file
to the selected target. It changes only the addressed destination literal. The
link label, surrounding Markdown, source metadata, and all unrelated bytes stay
unchanged.

The three values provide user intent, not mechanical proof. Repair still freshly
resolves the source occurrence, old literal, target, and expected and intended
bytes before planning and again before application.

An explicit tuple is admitted only when current Doctor facts classify the
occurrence as either a same-target safe-exact catalogue member or a missing-target
guided choice whose current bounded candidate set contains the selected target.
The tuple is not an arbitrary link editor. A valid unrelated occurrence, a
semantic target change without current candidate evidence, or any other
out-of-catalogue request is blocked.

## Selection And Interaction

Repair has finite selection paths. Interaction and explicit authority complete
one request; they do not select different operations or disjunctive modes.
Finding text and display order never select a path.

### Human interactive `repair`

The simplest useful interactive invocation is:

```text
open-forge repair
```

It opens the Repair wizard. The wizard:

1. Reruns Doctor for display, then identifies the exact diagnosis domains and
   facts required by each selectable local-reference edit.
2. Shows every current safe-exact proposal.
3. Presents finite guided candidates with their evidence and a
   recommendation-for-review when one exists. No uncertain guided candidate is
   selected by default.
4. Initially makes the safe-exact set available as the proposed exact selection.
   The user may select or skip individual proposals.
5. Permits select, skip, back, and cancel without creating a saved session.
6. Builds one complete plan from the selected exact effects and explicit guided
   choices.
7. Shows the exact affected paths, bounded diffs, expected and intended state,
   verification, and recovery requirements before application.
8. Requests final confirmation with `No` as the default.

The wizard does not create another operation or implementation. Its answers
populate the same typed request used by direct flags. A guided candidate remains
unselected until the user chooses it. Choosing a candidate is user intent, not
proof that replaces fresh validation.

`--dry-run` in interactive use opens the same wizard and uses the same
selections, plan, and exact-effect display, but stops without an application
confirmation and writes nothing. Human text may call this result Preview. It
does not save the plan for later application.

Any prompt-capable request with neither `--automatic` nor an explicit `--relink`
enters this same wizard, including requests that supply only `--workspace`,
`--view`, `--verbose`, or `--dry-run`. Those flags remain in the one request and
keep their normal meanings. `--json`, `--help`, and
`--version` retain their non-prompting or terminal boundaries.

### Automatic selection

`--automatic` suppresses the wizard. It selects every current safe-exact proposal
admitted by the first-release catalogue and never selects a guided candidate,
recommendation, divergent replacement, deletion, adoption, ownership change, or
fuzzy choice. It applies the selected plan unless `--dry-run` is also present.

The direct preview form is:

```text
open-forge repair --automatic --dry-run
```

It previews every selected automatic effect through the same facts, planner, and
preflight used by application. If no safe-exact proposal exists, the selected
scope can be a verified no-op; guided and manual findings remain unselected and
visible.

An unselected guided finding is not an unresolved choice in the automatic
request. It remains visible and can produce `attention` after the selected scope
completes. If an explicit requested effect requires a guided choice and no user
choice or exact relink supplies it, that requested effect is blocked.

### Explicit relink mode

Each explicit `--relink` tuple supplies exact selection authority for one current
local-reference occurrence. It suppresses candidate selection for that
occurrence. The user-selected target is not silently replaced by a recommended
candidate or another path.

An explicit-relink request is direct in every environment and does not open the
wizard or request another confirmation. Its exact tuples supply selection and
application intent. It applies unless `--dry-run` is present; JSON and other
non-interactive forms use the same direct request.

When `--automatic` and one or more explicit relinks are combined, the selected
scope is the union of all current safe-exact proposals and the explicit relink
tuples. The explicit tuple remains authoritative for its occurrence; automatic
selection never supplies a guided choice for it. Identical effects may coalesce
only under the Behavior rules and retain both origins.

### Non-interactive and JSON omission states

JSON is always non-interactive. An environment that cannot prompt uses the same
non-interactive rules even without `--json`:

| Request                                                             | Result and required next action                                                                                                  |
| ------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| No `--automatic`, no `--relink`, no `--dry-run` selection authority | `blocked`; use the interactive wizard, `--automatic`, or one or more explicit relinks.                                           |
| No `--automatic`, no `--relink`, with `--dry-run`                   | `blocked`; dry-run is preview policy, not selection authority. Use `--automatic --dry-run` or explicit relinks with `--dry-run`. |
| `--automatic` without `--dry-run`                                   | Select and apply all current safe-exact proposals without prompting.                                                             |
| `--automatic --dry-run`                                             | Select and preview all current safe-exact proposals without prompting.                                                           |
| One or more `--relink` values without `--dry-run`                   | Resolve and apply only the explicit relinks without prompting.                                                                   |
| One or more `--relink` values with `--dry-run`                      | Resolve and preview only the explicit relinks without prompting.                                                                 |
| `--automatic` plus one or more `--relink` values                    | Apply or preview their union without prompting, subject to all conflicts and gates.                                              |

In particular, `--json --dry-run` without a selection is blocked. It does not
invent a guided choice or silently change to an automatic default. Prefer the
explicit boring structured preview:

```text
open-forge repair --automatic --dry-run --json
```

## Admitted First-Release Catalogue

General Repair admits only these effects:

| Catalogue member               | Proposal and selection rule                                                                                                                                                                                                                   |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Same-target canonical path     | The authored local destination resolves to the same target, but its path spelling is not canonical. It is `safe-exact` and may be selected by `--automatic`.                                                                                  |
| Same-target canonical case     | The authored local destination differs only in case from the exact target spelling, and physical identity is unambiguous. It is `safe-exact` only when that identity is proven.                                                               |
| Same-target canonical encoding | The authored destination uses a non-canonical encoding for the same target. It is `safe-exact` only when the expected and intended bytes are exact.                                                                                           |
| Unique canonical fragment      | The target is the same and one canonical fragment correction is proven. It is `safe-exact` and may be selected by `--automatic`.                                                                                                              |
| Missing-target relink          | The authored target is missing and Doctor supplies bounded filename, title, literal-content, or structural route-neighborhood candidates. It is `guided-choice`; the user must choose in the wizard or provide an explicit `--relink` target. |

The filename, title, literal-content, and route-neighborhood evidence is finite
and visible. Zero, one, and several candidates remain distinct. A recommendation
is never selected automatically, including when there is only one candidate.
Explicit `--relink` supplies the user's selected target and does not rely on a
recommendation ranking.

The catalogue does not include external repairs, semantic or fuzzy target
choices, authored prose or labels, generated navigation, route authoring or
topology, route metadata, overwrite content, recovery-bundle or draft cleanup or restoration,
Framework mutation, Extension mutation, ownership adoption, or any effect that
lacks complete verification and recovery.

Generated navigation points to `index`. Known route intent points to an accepted
route operation. Recovery-bundle or draft deletion belongs to the separate accepted
[`cleanup` operation](../cleanup/interface.md). Framework and Extension lifecycle
findings point to their accepted lifecycle operation or manual instructions.
Repair does not invoke any public command.

## Diagnosis And Completeness Gate

Every Repair invocation may retain the complete Doctor diagnosis for display,
but a selected edit is gated only by the diagnosis dependency closure it
actually uses: exact workspace and path containment, the route/heading facts
needed by that target, and the local-reference occurrence and candidate facts.
Every required domain must have complete coverage for the selected edit.
Incomplete or blocked unrelated lifecycle, Extension, Framework, or recovery-
observer coverage remains visible but does not block a safe local-reference
repair. Recovery writer readiness for a real Replace is checked separately in
mutation preflight. Repair never borrows mutation authority from another domain.

## Mutation Authority And Safety

`--automatic` is an operation-specific selection authority, not a general force
or ownership grant. It never:

- Accepts a recommendation or guided candidate.
- Replaces divergent authored content.
- Deletes, adopts, or takes ownership of a target.
- Bypasses containment, identity, conflict, preflight, verification, or
  recovery requirements.
- Makes a fuzzy, semantic, or display-order choice.

For an applying plan with one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`), the
orchestrator prepares and verifies exactly one immutable ZIP
recovery bundle before the first target effect. The root is
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)/OpenForge/recovery/v1`, outside the
workspace, with no temporary, repository, `HOME`, or custom platform fallback.
Unavailable storage is `incomplete` before any write. A verified
no-op creates no bundle.

The deterministic final name uses the normalized physical workspace path key and
operation ID. A `CreateNew` draft in that external directory is closed and
reopened for semantic manifest, exact ordered entry, length, hash, and payload-
byte validation, moved within the same directory to the final name, and reopened
and verified again. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. The source-generated
`manifest.json` records schema-v1, command/operation/workspace identity, ordered
relative targets and change kinds, exact prior byte lengths/hashes/payload names,
and intended final absence or length/hash. Ordered ordinal payload entries hold
the exact prior bytes for each existing-target effect. The bundle is
immutable after preparation.

Every planned existing-target effect must match exactly one verified entry; Create and
no-op effects have none. All preparation completes before the first effect.
`FileChangeApplier` requires matching preparation for each existing-target effect and
performs one final effect per target. Immediately before application, revalidate
the source occurrence, old literal, target, expected bytes, intended bytes,
identity, containment, and bundle facts.

Before post-verification deletion begins, a handled application, verification,
or cancellation outcome reports the actual residual draft or final path; a valid
final remains when preparation completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
Repair never restores, rolls back, compensates for a target effect,
derives current target state from recovery provenance, or stores a journal,
progress receipt, or persisted plan. After final verification of whole-operation
success, delete the bundle. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `attention`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one.
When `Failed`/positively observed `Retained` recovery attention coexists with
remaining non-information findings, cleanup guidance owns the single next
action; those findings remain visible evidence.

Explicit Cleanup owns exact named final and draft deletion under its separate
lease-bound contract. Unknown or differently named artifacts remain untouched.
Recovery storage is ordinary current-user `LocalApplicationData` under the
stable workspace and cooperating-client threat model. No special platform-
permission or encryption behavior is promised. Recovery reads use semantic
schema and exact ordered-entry validation and do not extract bundles or add a
custom archive parser, reflection, native dependency, or package for this
boundary.

## Fresh State And Conflict Rules

Before admitting an effect, Repair freshly resolves:

- The source occurrence and its exact current location.
- The exact old authored destination literal.
- The local target and any candidate or selected target.
- The expected current bytes and intended after-state bytes.
- Identity, containment, verification, and recovery facts.

If the intended after-state already holds, Repair reports a verified no-op. It
does not invent a write. If the source occurrence shifts, the old literal
differs, the target or candidate changes or disappears, identity or containment
is ambiguous, or current state matches neither the expected nor intended state,
the effect is blocked. Repair never falls back, ranks a replacement, or chooses a
different occurrence.

The complete selection is one atomic plan. Equivalent effects coalesce when
their addressed byte range, expected state, intended result, verification
condition, and recovery requirement are identical. Automatic and explicit
selection may therefore coalesce one effect while every selection origin,
finding, and tuple remains visible as provenance. Contradictory tuples are
invalid. Stale state, missing authority, missing recovery preparation, overlapping
non-equivalent effects, and other plan conflicts block the entire plan. There is
no order winner and no partial application.

Distinct non-overlapping relinks in one source file compose against one common
expected complete-file state. They produce one intended complete-file state and
one replacement effect for that file, covered by the operation's one verified
recovery-bundle preparation, while retaining each occurrence and selection
origin. Overlapping byte ranges, different expected
complete-file states, or any combination that cannot prove one deterministic
resulting file block the complete plan.

## Output

Repair output has one hierarchy:

1. Semantic status, selection source, application policy, and whether the
   request was interactive, automatic, explicit, or a combination.
2. Exact workspace identity and selection method.
3. Diagnosis and selection coverage, including the relevant-domain gate for the
   selected edits.
4. An explicit application or Preview statement and the no-files-changed fact
   when dry-run is selected.
5. Finding counts and effect counts, separated into selected, unselected,
   repaired, remaining, new, manual, guided, and blocked outcomes.
6. Affected paths and exact bounded diffs or equivalent byte and fingerprint
   evidence where available.
7. Preflight, application, verification, recovery, and residual state.
8. Fresh post-repair relevant-domain diagnosis and its coverage.

The default human view is `expanded`. Compact output retains identity, status,
mode, coverage, effect and finding counts, selected and remaining resolution
lanes, affected paths, and required next actions. Expanded output adds evidence,
provenance, locations, candidate basis, exact effects, bounded diffs or
fingerprints, and lifecycle facts. No view emits a health score or percentage.

Human output leads with the result. It does not require the reader to understand
planning stages to understand a normal success or failure. `--verbose` is a
separate diagnostic dimension and does not change selection, planning, effects,
verification, recovery, status, or exit behavior.

An illustrative automatic preview is:

```text
Open Forge repair
Workspace: D:/work/example
Selection: automatic
Application: preview
Diagnosis coverage: complete; 6 domains complete
Selected: 2 safe-exact effects
Guided: 1 candidate set unselected
Plan: 2 local-reference destination literals

<exact bounded diffs>

No files changed (--dry-run).
Status: requires attention
Next: review the guided candidate before selecting it.
```

An illustrative guided result is:

```text
Open Forge repair
Mode: interactive wizard
Selected: 1 guided relink
Unselected: 1 guided candidate set
Affected path: .agents/docs/guide.md
Effect: replace the destination at line 12, column 8
Verification: current target is contained and the authored label is unchanged
Confirmation: No [default]
```

The values are illustrative. JSON emits one complete structured result derived
from the same typed result as human output. JSON never prompts and retains the
complete selected, unselected, finding, effect, exact-diff, fingerprint,
preflight, application, verification, recovery, residual, and post-diagnosis
meaning that the contract exposes. Exact field names and schema compatibility are
defined by the CLI Architecture.

Primary human `complete`, `attention`, and `incomplete` results are kept
together on stdout. Primary human `invalid`, `blocked`, `failed`, and
`interrupted` results are kept together on stderr, following the accepted Index
policy. JSON always emits one complete structured result to stdout for every
semantic status; separate bounded diagnostics use stderr.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                                                                         |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The selected Repair scope completed its dry-run or application path, including a verified no-op, with complete required diagnosis and no remaining non-information finding that requires attention in that scope. It does not claim that all Doctor findings are gone.                                                                                          |
| `attention`   | The selected scope was safely processed but finite non-information findings remain, or post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`. `Failed`/`Retained` recovery keeps target effects successful and reports the exact residual path with cleanup guidance. Human output may say `requires attention`. |
| `incomplete`  | Safe facts or partial results are available, but required diagnosis, proposal resolution, post-diagnosis, or another declared coverage boundary could not finish. No general Repair write begins.                                                                                                                                                               |
| `invalid`     | Command grammar, flag values, relink tuples, repetition, or explicit inputs are invalid.                                                                                                                                                                                                                                                                        |
| `blocked`     | A valid request lacks required selection or authority, has incomplete or blocked coverage in a domain required by the selected edit, is stale or conflicting, has malformed, colliding, or mismatched recovery facts, or cannot establish a safe exact effect. No write begins.                                                                                 |
| `failed`      | Application, verification, or post-condition processing fails, or post-verification deletion returns `Failed` with disposition `Unknown`. Typed observed or unknown recovery facts remain visible; an exact expected path appears only when the recovery result provides one.                                                                                   |
| `interrupted` | The caller cancels or interrupts before the selected operation completes, unless an unexpected application or verification failure is classified as `failed`.                                                                                                                                                                                                   |

`--dry-run` uses the same status conditions while stopping before persistent
effects. It does not claim that intended bytes were written or verified on disk.
A successful automatic or explicit no-op is `complete`, not an artificial
update. A final interactive `No`, back out of the final application boundary,
or cancellation is a non-applied interruption with no residual failure.

`complete` is deliberately scoped to the selected Repair work. Remaining
informational findings do not change it. Remaining guided, manual, targeted,
blocked, or newly actionable findings produce `attention` when the selected
scope itself completed safely.

## Errors And Omission States

Repair has no positional-operand omission state. The following input states are
finite:

- A positional operand, generic proposal reference, choice token, finding text
  or code, domain or kind selector, generic batch input, plugin fixer selector,
  or unsupported alias is `invalid`.
- A missing value for a flag, a malformed source location, a non-positive or
  absent line or column, an incomplete `--relink` triple, or an invalid target
  path or fragment is `invalid`.
- Repeated identical relink tuples deduplicate. Contradictory tuples for one
  occurrence are `invalid`; argument order does not choose a winner.
- Repeated `--automatic` and `--dry-run` are accepted and idempotent. Shared
  global repetition follows the shared contract. Unknown options are invalid.
- An interactive bare `repair` opens the wizard. In a non-interactive or JSON
  request, no selection authority is `blocked`; the next action names
  `--automatic` or explicit `--relink`.
- Non-interactive or JSON `--dry-run` without `--automatic` or explicit relinks
  is `blocked`; dry-run alone never supplies selection authority.
- A source occurrence, expected literal, selected target, candidate, identity,
  containment boundary, expected bytes, or intended bytes that differs during
  fresh resolution is `blocked`.
- An incomplete or blocked required Doctor domain is `incomplete` or `blocked`
  and prevents all general Repair writes, including explicit relinks.
- Unavailable recovery storage or preparation coverage is `incomplete`; malformed,
  colliding, or mismatched recovery-bundle facts, overlap conflict, missing
  authority, or any other unsafe plan condition is `blocked`.
- An unexpected application, verification, or post-diagnosis failure, or
  post-verification recovery deletion `Failed`/`Unknown`, is `failed`. A caller
  interruption without an unexpected application or verification failure is
  `interrupted`.

Every ordinary error names the `repair` operation, affected selection, occurrence,
target, plan, or path when known, direct cause, and useful next action. It never
uses severity, message order, or a finding code to choose an effect.

## Examples

Open the interactive wizard. Safe-exact proposals are shown first, guided
candidates require selection, and final application confirmation defaults to No:

```text
open-forge repair
```

Preview every current safe-exact effect without prompting or applying:

```text
open-forge repair --automatic --dry-run
```

Request the same preview as one non-interactive structured result:

```text
open-forge repair --automatic --dry-run --json
```

Select one exact contained target for a local-reference occurrence. The complete
values are quoted so the shell passes each tuple member as one value:

```sh
open-forge repair \
  --relink ".agents/docs/guide.md@12:8" \
  "../old.md#Old" \
  ".agents/docs/new.md#New" \
  --dry-run
```

The CLI resolves the source occurrence and expected old literal, computes the
correct authored relative destination to `.agents/docs/new.md#New`, preserves
the label and every unrelated byte, and shows the exact bounded change without
writing. A shifted occurrence, changed old literal, missing target, changed
candidate, ambiguous identity, or unsafe containment blocks instead of using a
fallback.

Apply only explicit relinks in a non-interactive request:

```text
open-forge repair --json \
  --relink ".agents/docs/guide.md@12:8" \
  "../old.md#Old" \
  ".agents/docs/new.md#New"
```

This request does not prompt. It still requires complete relevant-domain diagnosis,
fresh expected-state validation, one conflict-free plan, verified recovery-bundle
preparation, and verification.

## Non-Goals And Targeted Boundary

Repair does not:

- Add a second command name, child operation, generic apply, saved plan, saved
  report, session, receipt, generic batch input, proposal registry, or plugin
  fixer registry.
- Accept `--yes`, `--preview`, `--suggestions`, `--all`, `--force`, `--apply`,
  domain or kind modes, finding text or code dispatch, or opaque choice tokens.
- Choose a guided target through severity, recommendation, path order,
  modification time, generated order, fuzzy matching, semantic matching, or
  another fallback.
- Repair external references, absolute or query destinations, authored prose or
  labels, generated navigation, route authoring or topology, metadata,
  overwrites, recovery-bundle cleanup or restoration, Framework lifecycle, Extension
  lifecycle, or ownership.
- Partially apply a plan, resolve a conflict by order, overwrite divergent
  content, adopt a target, or bypass safety, conflict, or recovery checks.
- Invoke `doctor`, `index`, a route operation, or a future lifecycle command as a
  public subprocess. Shared facts and planners may be reused without invoking a
  public command.

`index` owns standalone generated-navigation drift. Accepted route operations own
known route intent. The accepted [`cleanup` contract`](../cleanup/interface.md)
owns recognized transient and recovery-bundle/draft deletion. Accepted Framework and
Extension lifecycle contracts or manual instructions own those mutations. These
boundaries do not merge their operations or syntax.

## Public Verification

Conformance evidence must cover:

- Exact syntax, rejection of positional operands and every rejected alias or
  generic selection form, all six global flags, and the fact that `--automatic`
  is operation-specific rather than global.
- Repetition and composition of `--automatic`, `--relink`, `--dry-run`, shared
  flags, duplicate tuples, and contradictory tuples, with unknown options
  rejected.
- Human wizard entry, six-domain rerun, safe-exact presentation, guided
  candidate evidence, no uncertain default, select/skip/back/cancel, one plan,
  exact effects, and final confirmation default No.
- Interactive dry-run parity and the absence of every persistent effect.
- Automatic selection of every current safe-exact proposal, rejection of guided
  automatic choices, explicit relink union, and idempotent repetition.
- Non-interactive and JSON omission states, including blocked `--json --dry-run`
  without selection and the explicit `--automatic --dry-run --json` preview.
- Exact source-location, expected-destination, and selected-target grammar,
  shell quoting, one-based locations, contained local boundary, relative
  destination computation, label and unrelated-byte preservation, and all stale
  or ambiguous-state blocks.
- Every admitted safe-exact and guided catalogue member, candidate evidence
  basis, zero/one/several candidate cardinality, and no external or semantic
  repair.
- Complete coverage for the exact diagnosis domains required by selected edits,
  with unrelated incomplete lifecycle or recovery-observer facts visible but
  non-blocking.
- One atomic selection union, equivalent-effect coalescing, contradictory and
  overlapping conflict blocking, missing authority, verified recovery-bundle
  preparation, and no partial application.
- The complete mutation lifecycle: fresh diagnosis, selected intent, plan,
  conflict check, preflight, exact dry-run, confirmation, all-before-first-effect
  recovery-bundle preparation, revalidation, apply, per-effect and semantic
  verification, residual-path reporting, and fresh relevant-domain diagnosis,
  with no fixpoint loop.
- Human compact and expanded, JSON, and verbose projections from one typed
  result, including exact bounded effects and the accepted stdout and stderr
  policy under the shared schema and process-status mapping defined by the CLI
  Architecture.
- Selected, unselected, repaired, remaining, new, manual, guided, and blocked
  findings and effects, mode, affected paths, preflight, application,
  verification, recovery, residual, and post-diagnosis coverage.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  meanings, including selected-scope completion and remaining non-information
  attention.
- Repeatability, verified no-op behavior, preservation of user content, and
  rejection of all unsupported mutation domains.

## Related Current Sources

- [Repair Command Contract Set](_repair.md)
- [Repair Behavior Contract](behavior.md)
- [Doctor Interface Contract](../doctor/interface.md)
- [Doctor Behavior Contract](../doctor/behavior.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [CLI Architecture](../../architecture.md)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
