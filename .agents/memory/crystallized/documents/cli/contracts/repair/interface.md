---
open-forge:
  description: Current accepted interface for automatic, guided, explicit-relink, and dry-run Repair
  responsibility: Define the Repair public grammar, selection authority, finite catalogue, results, errors, and examples
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Repair, Interface, Mutation, Relink, Safety, CurrentTruth]
---

# Repair Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the public `repair`
Interface Contract. The command does not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md). This contract owns the exact
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

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the accepted shared structured schema and process-status mapping. The [CLI
Architecture](../../architecture.md) defines source and runtime boundaries,
BCL-first filesystem structure, the workspace-lock boundary, and recovery
identity relationships. No command-local Technical Design file exists for Repair. This Interface
Contract remains technology-neutral.

## Purpose And Boundary

`repair` applies only current diagnosis-backed, conflict-free, exact
meaning-preserving local corrections and explicitly selected contained relinks.
It may also consume one semantically verified current-v1 Workspace Library
residual when an already selected safe-exact effect authorizes the bounded
no-follow recovery described below. It does not author content, labels,
metadata, generated navigation, route topology, recovery cleanup, Framework
lifecycle, Extension lifecycle, or ownership decisions. It does not promise to
eliminate every Doctor finding.

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
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
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

Only authored local-reference occurrences enter Repair classification. A link in
a recognized generated `Entries` region is navigation output, not an authored
occurrence, and is excluded from Repair occurrence classification, catalogue
admission, and explicit relink resolution. An authored, unambiguous occurrence
elsewhere in the same source remains independently eligible when its own facts
are safe.

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
authored occurrence as either a same-target safe-exact catalogue member or a
missing-target guided choice whose current bounded candidate set contains the
selected target. The tuple is not an arbitrary link editor. A generated `Entries`
occurrence, a valid unrelated occurrence, a semantic target change without
current candidate evidence, or any other out-of-catalogue request is blocked.

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
`--detail`, `--detail debug`, or `--dry-run`. Those flags remain in the one request and
keep their normal meanings. `--format json`, `--help`, and
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
request. It remains visible and can produce `completed-with-warnings` after the selected scope
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
non-interactive rules even without `--format json`:

| Request                                                             | Result and required next action                                                                                                  |
| ------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| No `--automatic`, no `--relink`, no `--dry-run` selection authority | `blocked`; use the interactive wizard, `--automatic`, or one or more explicit relinks.                                           |
| No `--automatic`, no `--relink`, with `--dry-run`                   | `blocked`; dry-run is preview policy, not selection authority. Use `--automatic --dry-run` or explicit relinks with `--dry-run`. |
| `--automatic` without `--dry-run`                                   | Select and apply all current safe-exact proposals without prompting.                                                             |
| `--automatic --dry-run`                                             | Select and preview all current safe-exact proposals without prompting.                                                           |
| One or more `--relink` values without `--dry-run`                   | Resolve and apply only the explicit relinks without prompting.                                                                   |
| One or more `--relink` values with `--dry-run`                      | Resolve and preview only the explicit relinks without prompting.                                                                 |
| `--automatic` plus one or more `--relink` values                    | Apply or preview their union without prompting, subject to all conflicts and gates.                                              |

In particular, `--format json --dry-run` without a selection is blocked. It does not
invent a guided choice or silently change to an automatic default. Prefer the
explicit boring structured preview:

```text
open-forge repair --automatic --dry-run --format json
```

## Admitted First-Release Catalogue

General Repair admits only these effects:

| Catalogue member                | Proposal and selection rule                                                                                                                                                                                                                                                                                             |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Same-target canonical path      | The authored local destination resolves to the same target, but its path spelling is not canonical. It is `safe-exact` and may be selected by `--automatic`.                                                                                                                                                            |
| Same-target canonical case      | The authored local destination differs only in case from the exact target spelling, and physical identity is unambiguous. It is `safe-exact` only when that identity is proven.                                                                                                                                         |
| Same-target canonical encoding  | The authored destination uses a non-canonical encoding for the same target. It is `safe-exact` only when the expected and intended bytes are exact.                                                                                                                                                                     |
| Unique canonical fragment       | The target is the same and one canonical fragment correction is proven. It is `safe-exact` and may be selected by `--automatic`.                                                                                                                                                                                        |
| Missing-target relink           | The authored target is missing and Doctor supplies bounded filename, title, literal-content, or structural route-neighborhood candidates. It is `guided-choice`; the user must choose in the wizard or provide an explicit `--relink` target.                                                                           |
| Typed Library residual recovery | A semantically verified current-v1 residual is attributed to the selected workspace's Library record and the existing automatic/guided selection shape authorizes one exact safe-exact ordinary-record, ordinary-file, or relative-file-link recovery effect. It uses no new syntax and never becomes generic rollback. |

The filename, title, literal-content, and route-neighborhood evidence is finite
and visible. Zero, one, and several candidates remain distinct. A recommendation
is never selected automatically, including when there is only one candidate.
Explicit `--relink` supplies the user's selected target and does not rely on a
recommendation ranking. Only authored occurrences participate in this catalogue;
generated `Entries` links remain outside it. A concrete Library observation that
is not a typed residual recovery proposal remains a diagnostic observation with
its actual path, identifier, and cause. It is not converted into a Markdown
broken-link occurrence, a guided candidate, or a Repair proposal.

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

Library residual recovery is admitted only from typed current-v1 attribution and
entries, never from a filename, path, matching bytes, or an inferred record. It
uses no-follow logical parent/leaf/raw-target checks. It may delete only an exact
intended created link or object to restore a prior-missing state, or recreate an
exact deleted or dangling relative link from its stored raw target. It never
follows, writes, or deletes a source target and blocks a third or unsafe state.

## Diagnosis And Completeness Gate

Every Repair invocation may retain the complete Doctor diagnosis for display,
but a selected edit is gated only by the diagnosis dependency closure it
actually uses: exact workspace and path containment, the route/heading facts
needed by that target, and the local-reference occurrence and candidate facts.
Every required domain must have complete coverage for the selected edit.
Unsafe or unavailable facts required by a selected write dependency remain
strict and block that selected effect before application. Read or coverage
uncertainty outside the selected write dependency remains visible; it is not
silently treated as absence, safety, or complete coverage. In particular, a
concrete non-information Library observation retains its warning severity and `completed-with-warnings` status (exit 2) and actual known path, identifier, and cause while an independent
ordinary authored safe link may proceed. That Library observation does not become
a Markdown broken-link occurrence or grant a proposal. Incomplete or blocked
unrelated lifecycle, Extension, Framework, Library read/coverage, or recovery-
observer facts therefore remain visible without blocking an independent safe
local-reference repair. A selected typed Library residual instead requires
complete current Library record attribution, exact entry, workspace, and
no-follow identity facts for that residual; unrelated domains remain separate
and do not grant Library authority. Recovery writer readiness for a real Replace
is checked separately in mutation preflight. Repair never borrows mutation
authority from another domain.

## Mutation Authority And Safety

`--automatic` is an operation-specific selection authority, not a general force
or ownership grant. It never:

- Accepts a recommendation or guided candidate.
- Replaces divergent authored content.
- Deletes, adopts, or takes ownership of a target.
- Bypasses containment, identity, conflict, preflight, verification, or
  recovery requirements.
- Makes a fuzzy, semantic, or display-order choice.

### Typed Workspace Library residual recovery

Repair may consume a semantically verified current-v1 residual only when its
trusted immutable attribution names the selected workspace's Workspace Library
record and the existing automatic/guided selection shape already authorizes
that exact safe-exact effect. This is a bounded recovery effect in the existing
selection shape; it adds no flag, operand, child command, rollback mode, or
fixpoint loop. The accepted typed entry classes are:

- a prior-missing ordinary Library record `Create`, where the current record or
  object is the exact intended created state and Repair may delete only that
  exact object to restore the prior missing state;
- an ordinary-file entry carrying exact prior bytes, where Repair may restore
  those bytes only at the exact no-follow logical path after the current state
  is the verified intended state; and
- a relative-file-link `Create` or `Delete` entry. A `Create` entry may delete
  only its exact intended created link/object to restore prior absence. A
  `Delete` entry may recreate the exact relative link only when the destination
  is exactly missing, using the stored raw target text even when that target is
  dangling.

Every recovery check uses no-follow logical parent, leaf, and raw-target
identity. It never follows, writes, or deletes the source target or treats
source bytes as link payload. A destination that is third, changed, unavailable,
unsafe, aliased, ambiguous, or otherwise outside the verified prior/intended
states blocks the effect. Repair never infers a record or effect from a filename
or path alone.

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
`manifest.json` uses the one public schema-v1 discriminator value `1` and records
command/operation/workspace identity, one required immutable `attribution`
object, ordered relative targets and change kinds, exact prior byte
lengths/hashes/payload names, and intended final absence or length/hash. The
attribution contains one finite producer, one finite operation, and a typed
subject `{kind, identity}`. Ordered ordinal payload entries hold the exact prior
bytes for each existing-target effect. The bundle is immutable after
preparation.

The exact schema-v1 attribution vocabulary, valid producer/operation/subject
combinations, and required non-null workspace identity are defined by the
[Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
Repair accepts no unknown value or fallback attribution.

Repair's future recovery writer uses this same current-v1 shape and supplies its
own exact producer, operation, and subject from trusted Repair-owned facts. It
does not forward free command, GUID, path, filename, or ordered-entry values as
attribution or place them in a generic field bag. A schema-1 final missing or
carrying invalid attribution is malformed/unattributed and remains preserved;
it is not migrated, rewritten, repaired, deleted, or adopted. Unknown schema
versions are unsupported. Drafts remain exact-name, path-only `Incomplete` facts;
observers do not inspect or use their bytes for attribution.

For a typed Library residual, the already verified current-v1 bundle entry is
the bounded recovery evidence consumed by the selected effect. Repair does not
reinterpret it as a generic forward plan, infer a new record, or manufacture an
automatic rollback or fixpoint; any ordinary forward Repair effects retain their
normal recovery preparation rules.

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
Outside the typed Library residual exception above, Repair never restores,
rolls back, or compensates for a target effect, and it never derives current
target state from recovery provenance or stores a journal, progress receipt, or
persisted plan. The exception is a selected, bounded recovery effect rather
than automatic rollback. After final verification of whole-operation success,
delete the bundle.
The bundle deleted on success is Repair's newly prepared forward bundle. A
selected original Library residual ZIP remains byte-identical, including its
unselected entries; only explicit Cleanup may delete that original bundle.
`Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one.
When `Failed`/positively observed `Retained` recovery warning coexists with
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

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                                      | Headline                                                                                  | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing in scope needs repair                                                             | `Nothing to repair.`                                                                      |    0 | stdout |
| completed               | repairs applied, nothing left in scope                                                    | `Repaired <N> links.` (`Repaired 1 link.`)                                                |    0 | stdout |
| completed (dry run)     | repairs planned                                                                           | `Would repair <N> links.`                                                                 |    0 | stdout |
| completed-with-warnings | repairs applied, problems remain that need a choice or hand                               | `Repaired <N> links. <K> problems still need a choice.` / `... need a choice or a hand.`  |    2 | stdout |
| completed-with-warnings | nothing safe, problems remain                                                             | `Nothing could be repaired automatically. <K> problems need a choice.`                    |    2 | stdout |
| completed-with-warnings | recovery bundle retained                                                                  | + family row                                                                              |    2 | stdout |
| incomplete              | diagnosis or a required fact could not finish                                             | `Repair could not check the workspace completely. Nothing was changed.` + limitation rows |    3 | stdout |
| invalid-input           | bad flags, invalid or contradictory `--relink`                                            | `Cannot repair: <problem>.`                                                               |    4 | stderr |
| blocked                 | no selection outside a terminal, diagnosis blocked, stale target, missing authority, lock | `Cannot repair: <reason>. Nothing was changed.`                                           |    5 | stderr |
| failed                  | after effects                                                                             | `Repair stopped after <n> of <m> links were rewritten.`                                   |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                                  | `Repair was cancelled. Nothing was changed.`                                              |  130 | stderr |

### Text by level

Guided broken-link findings for authored occurrences include the concrete
expected destination in the finding message so the unresolved target is visible
before candidate details. Concrete Library observations retain their actual
path, identifier, and cause and are not rendered as Markdown broken-link
findings. Cancelled and interrupted results retain their existing status,
headline, stream, and output boundary; a fresh Library observation does not add
new cancellation output.

`minimal`, applied:

```text
Repaired 2 links.
  .agents/loader.md:105:3    ../patterns/_patterns.md -> patterns/_patterns.md
  .agents/maps/_maps.md:32:3   nowhere/_Nope.md -> nowhere/_nope.md
```

`minimal`, applied with remaining problems:

Report completed link effects and each remaining problem. Guided authored-link findings name the unresolved destination before the available choices; Library findings show their actual observed subject and cause. The next action follows the rules below.

`minimal`, nothing safe outside a terminal:

Report that no automatic repair was available, retain each unresolved authored destination and its bounded choices, and direct the user to interactive Repair or an explicit relink. Concrete examples are retained in the reviewed Repair output snapshots.

`minimal`, no selection possible (stderr):

```text
Cannot repair: repair needs to know which repairs to apply, and this session cannot ask.
Next: open-forge repair --automatic  (apply the 6 repairs that are safe)
```

`minimal`, partial (stderr):

```text
Repair stopped after 1 of 2 links were rewritten.
  .agents/loader.md:105:3    rewritten
  .agents/maps/_maps.md:32:3   not started
  Recovery data: <path>
Next: open-forge doctor
```

`standard` adds `Workspace:`, each remaining authored guided-link problem with
its possible targets as rows, the Library recovery steps selected or skipped,
concrete Library observations with their actual path, identifier, and cause,
and the reason for `Next`. Library observations do not gain candidate rows.

`full` adds why each possible target was suggested, the expected and new
SHA-256 of each rewritten file, the checks that ran (diagnosis coverage as a
sentence), and recovery facts in words.

### Prompts

Per [04](../../../../../working/cli-development/tasks/task30-g4/04-interaction-system.md): summary line, `Apply the <N> repairs that
are safe? [y/N]`, then for each authored guided link a Select among possible targets
with `skip` as the last row, then plan review, then `Apply these changes?
[y/N]`. The old typed words (`select`, `skip`, `back`, `cancel`) and the
SHA-256 confirmation text are removed.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-completed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/AutomaticSelection/automatic-two-links.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-completed-with-warnings). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/AutomaticSelection/automatic-nothing-safe-two-guided.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/LibraryRecoveryStep/library-recovery-step.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/InvalidInput/invalid-input.standard.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/LockHeld/lock-held.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-failed). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#repair-cancelled). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/__snapshots__/RepairBeforeOutputSnapshotTests/SelectionBoundary/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                                                        |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, selection: "automatic" \| "relink" \| "prompt", repairs: [ { path, location, from, to } ], remaining: [ { path, location, kind, candidates: n } ] }` |
| standard | + `remaining[].candidates: [ { path } ]`, `libraryRecovery: [ { id, path, selected } ]`                                                                       |
| full     | + `candidates[].reasons`, per repair `before`, `after`, `diagnosis { coverage per category }`, `verification`                                                 |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

Post-diagnosis output uses current residual Library observations in place of
pre-diagnosis Library observations that were resolved, and deduplicates only
identical actual code, path, identifier, and cause facts. Concrete Library
observations retain their warning severity and `completed-with-warnings` status (exit 2) and actual path, identifier, and
cause, do not become Markdown broken-link rows or candidates, and do not add
members to the public schema.

### Effects wording

`<path>:l:c  <old destination> -> <new destination>` for a rewritten link;
`<path>  restored from recovery` for a Library recovery step. Dry run: same
rows under `Would repair`. Partial: `rewritten`, `not started`, `final state
unknown`.

### Counts and limitations

`linksRepaired`, `problemsRemaining`, `problemsNeedingChoice`,
`problemsNeedingHand`, `librarySteps`.

### Next rules

Remaining guided authored-link findings -> `open-forge repair`; no selection ->
`open-forge repair --automatic`; Library-only remaining problems ->
`open-forge doctor`; mixed authored-link and Library problems retain the Repair
action for authored links and explain Doctor for the Library diagnosis; partial
or retained recovery keeps its existing `open-forge doctor` /
`open-forge cleanup` boundary; completed -> none.
Never `verified` wording anywhere.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                              | Severity | Family                     | Message                                                                                        | Next                                                             |
| --------------------------------- | -------- | -------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| repair.invalid-input              | error    | invalid-input              |                                                                                                |                                                                  |
| repair.relink-invalid             | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.relink-invalid`). | `open-forge repair --help`                                       |
| repair.contradictory-relink       | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.contradictory-relink`).                      | none                                                             |
| repair.selection-required         | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.selection-required`).                    | `open-forge repair --automatic`                                  |
| repair.missing-authority          | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.missing-authority`).         | add the path to `allowInstallPaths` in `.agents/open-forge.json` |
| repair.diagnosis-blocked          | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.diagnosis-blocked`).                                                | `open-forge doctor`                                              |
| repair.diagnosis-incomplete       | warning  | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.diagnosis-incomplete`).                                 | `open-forge doctor`                                              |
| repair.proposal-unavailable       | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.proposal-unavailable`).                              | `open-forge doctor --detail standard`                            |
| repair.proposal-unsupported       | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.proposal-unsupported`).                                       | fix by hand                                                      |
| repair.plan-conflict              | error    | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.plan-conflict`).                                              | none                                                             |
| repair.guided-finding-remaining   | warning  | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.guided-finding-remaining`).                                                | `open-forge repair`                                              |
| repair.manual-finding-remaining   | warning  | local                      | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Repair/Shared/Wording/RepairWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`repair.manual-finding-remaining`).                                                             | none                                                             |
| repair.target-changed             | error    | target-changed             |                                                                                                |                                                                  |
| repair.target-unsafe              | error    | target-unsafe              |                                                                                                |                                                                  |
| repair.workspace-lock-unavailable | error    | workspace-lock-unavailable |                                                                                                |                                                                  |
| repair.recovery-conflict          | error    | recovery-conflict          |                                                                                                |                                                                  |
| repair.recovery-unavailable       | warning  | recovery-unavailable       |                                                                                                |                                                                  |
| repair.recovery-artifact-retained | warning  | recovery-artifact-retained |                                                                                                |                                                                  |
| repair.write-failed               | error    | write-failed               |                                                                                                |                                                                  |
| repair.verification-failed        | error    | verification-failed        |                                                                                                |                                                                  |
| repair.recovery-failed            | error    | recovery-failed            |                                                                                                |                                                                  |
| repair.operation-failed           | error    | operation-failed           |                                                                                                |                                                                  |
| repair.interrupted                | error    | cancelled |                                                                                                |                                                                  |

## Scenarios

### Catalogue situations

`nothing-to-repair`, `automatic-two-links`, `automatic-nothing-safe-two-guided`,
`dry-run-automatic`, `relink-one`, `relink-invalid`, `contradictory-relinks`,
`non-interactive-no-selection`, `library-recovery-step`, `lock-held`,
`write-failed-partial`, `cancelled`, `invalid-input`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native evidence leaves four catalogue questions unresolved: whether the `relink-invalid` situation should be renamed to the current `plan-conflict` situation or accompanied by a real malformed relink case; which code and message should describe the current explicit-relink conflict; whether the dry-run warning headline should say `Would repair`; and whether the singular sentence should read `1 problem still needs a choice.` The current outputs are recorded without choosing among those alternatives. **Maintainer decision remains open.**
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
- Recover a Workspace Library residual unless a current-v1 typed attribution and
  existing safe-exact selection authorize the exact ordinary record/file/link
  effect; infer recovery from a filename or path; follow or mutate a source
  target; or apply a third or unsafe state.
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
- Non-interactive and JSON omission states, including blocked `--format json --dry-run`
  without selection and the explicit `--automatic --dry-run --format json` preview.
- Exact source-location, expected-destination, and selected-target grammar,
  shell quoting, one-based locations, contained local boundary, relative
  destination computation, label and unrelated-byte preservation, and all stale
  or ambiguous-state blocks.
- Every admitted safe-exact and guided catalogue member, candidate evidence
  basis, zero/one/several candidate cardinality, generated `Entries` exclusion,
  and no external or semantic repair. Concrete Library observations remain
  observations with their actual path, identifier, and cause rather than
  Markdown broken-link occurrences or invented proposals.
- Complete coverage for the exact diagnosis domains required by selected edits,
  with unsafe or unavailable selected-write facts blocking strictly and
  unrelated read/coverage uncertainty visible without silent masking.
- One atomic selection union, equivalent-effect coalescing, contradictory and
  overlapping conflict blocking, missing authority, verified recovery-bundle
  preparation, and no partial application.
- Typed Workspace Library residual recovery for prior-missing ordinary-record
  Create, ordinary-file prior bytes, and relative-file-link Create/Delete
  entries, including exact no-follow parent/leaf/raw-target checks, safe delete
  or recreate boundaries, third/unavailable-state blocking, source-target
  non-mutation, no path/filename inference, and no automatic rollback or
  fixpoint.
- The complete mutation lifecycle: fresh diagnosis, selected intent, plan,
  conflict check, preflight, exact dry-run, confirmation, all-before-first-effect
  recovery-bundle preparation, revalidation, apply, per-effect and semantic
  verification, residual-path reporting, and fresh relevant-domain diagnosis,
  with no fixpoint loop.
- Human minimal, standard, full, debug, and JSON projections from one typed
  result, including exact bounded effects and the accepted stdout and stderr
  policy under the shared schema and process-status mapping defined by the
  [Shared Result Coordinates](../shared/result-coordinates/interface.md).
- Selected, unselected, repaired, remaining, new, manual, guided, and blocked
  findings and effects, mode, affected paths, preflight, application,
  verification, recovery, residual, and post-diagnosis coverage.
- Completed, completed-with-warnings, incomplete, invalid-input, blocked,
  failed, and cancelled meanings, including selected-scope completion and
  remaining non-information warnings, Library-only Doctor next action, mixed
  authored-link/Library next-action explanation, and the unchanged
  cancelled/interrupted output boundary.
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


## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`repair.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Repair/RepairText.cs).

<!-- @OpenForgeTextRef repair.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [RepairPhrases.cs](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Repair/RepairPhrases.cs)
  <!-- @OpenForgeTextRef repair.phrase.remaining-link -->
- [RepairWording.cs](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Repair/RepairWording.cs)
  <!-- @OpenForgeTextRef repair.wording.remaining-library-diagnosis -->
  <!-- @OpenForgeTextRef repair.wording.inspect-remaining-library-problems -->
  <!-- @OpenForgeTextRef repair.wording.review-remaining-links-and-inspect-library-problems -->
  <!-- @OpenForgeTextRef repair.wording.broken-destination-detail -->
