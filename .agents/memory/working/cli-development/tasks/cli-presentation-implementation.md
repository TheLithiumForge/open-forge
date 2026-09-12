---
open-forge:
  description: Implement approved compact and expanded command views, compact JSON, automatic colour and reusable CLI guidance in verified sequential sets
  tags: [Memory, Working, Contextual, CLI, Task, Implementation, Presentation]
---

# CLI Presentation Implementation

## Accepted Outcome

The user approved the command proposals, example style and views/JSON analysis
on 2026-09-12, with these corrections: implement compact and expanded for every
command in text and JSON; retain compact-to-expanded fallback when a renderer is
unavailable; no separate AI view; automatic fixed terminal colours with no colour
flag or user-defined palette. Written labels remain. Existing operation meaning,
diagnostic kinds, statuses, actions, effects and recovery remain intact.

The [command proposals](cli-command-output-proposals.md),
[gallery](cli-command-output-examples.md) and
[view analysis](cli-view-format-color-analysis.md) supply accepted direction.
Their placeholders are not frozen executable evidence. This capsule records
implementation decisions and supersedes their approval-pending language.
The colour-option proposal is rejected; do not add `--color` or configuration.
Automatic host capability selects colour on supported terminals; redirected
output, JSON and selected authored content stay plain. Colour is not a new
operation or a source of status information.

## Sequential Sets

1. View resolution: explicit per-format compact/expanded renderer selection and
   compact-to-expanded fallback, preserving existing output; meaningful tests.
2. Doctor/Status human output: approved wording/grouping, no repeated evidence,
   preserved distinct findings/counts, clear compact and expanded views.
3. Library human output: all five commands; restore mutation mappings/effects.
4. Extension human output: all six commands; distinct compact/expanded views.
5. Remaining read and mutation commands, grouped into bounded sets; exact content,
   Find TSV and preview diffs remain protected.
6. Compact JSON: freeze shared representation identity and per-command compact
   field membership, implement all 28 commands with full JSON default, then
   qualify the complete public boundary before exposing the new global behavior.
7. Automatic colour: fixed semantic accents, host/stream capability transport,
   no option/configuration, no user-content recolouring; qualify supported hosts.
8. Complete help/docs and reusable CLI UX/development Guidance and CLI/C# Directive
   consolidation. Refresh installed CLI and dogfood the final result.

Root works directly and sequentially. No agents or remote operations. Squash each
qualified set into local develop. Keep required source/tests/snapshots and these
records tracked; artifact outputs are disposable. Existing unrelated delivery
work at 6cd93fcf remains intact.

## Deferred Display Filtering

The optional display filter remains backlog, reconfirmed by the user during
implementation. No spelling or global status/severity vocabulary is selected.
After the new views are dogfooded, evaluate Doctor category filtering first and
per-finding status/severity filtering where the command actually has that fact.
Overall command status is distinct from individual finding severity. Any future
display filter must disclose omitted rows/categories and preserve overall
coverage, status, exit, operation behavior and recovery information. The
[filtering recommendation](cli-command-output-proposals.md#filtering-recommendation)
owns the analysis. This does not block the approved presentation sets.

## Set 1 Freeze

Baseline: `8770d24a`. Branch: `codex/cli-presentation-implementation`, existing
worktree `/tmp/open-forge-cli-refactor-sequential`.

Scope: neutral Shell per-view rendering and its pipeline integration. A required
expanded renderer plus optional compact renderer defines one output format.
Selecting unavailable compact calls expanded with effective view expanded.
Selecting expanded never chooses compact. Format, result identity, operation,
status, exit and stream stay unchanged. Undefined views are rejected; failures
inside a renderer propagate and never trigger retry/fallback or another operation.

Existing combined renderers that already accept a view keep both views registered
through the existing two-delegate RendererSet construction. Explicit per-view
construction supports absence/fallback. Every command's final delivery must
provide both meaningful views; fallback is not evidence that compact is complete.

Placement: neutral behavior under Shell/Presentation/Shared/Rendering, used by
Shell/Pipeline/CliRendererSet. No command imports or universal view-model tree.
No new dependency; ordinary delegates and immutable fields are sufficient.

Evidence: focused Unit cases prove selected delegate/result identity/effective
view, unselected renderer non-execution, JSON format preservation, undefined input
and exception propagation. Existing Shell pipeline cases freeze operation and
stream behavior. This shared composition change triggers full managed/native
qualification before squash. Exact commands and receipts will be recorded here.

Applicability: local unreleased CLI; source changes are reversible in Git. The
set changes no filesystem behavior, input parser, serialized schema or content.
The supported cooperating-process and stable-workspace safety boundary remains.
Tests are under `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests`; no new test
project. Native and Integration gates cover the real composition boundary.

## Set 2 Freeze

Scope: Doctor and Status human rendering and affected Interface/Behavior text.
The rendering graph consumes the existing typed results. No inspection, mutation,
count, status, action formation, JSON projection, help grammar or default changes.
No new dependency or exceptional machinery. The local, read-only presentation
risk is hiding distinct facts or overstating candidate certainty; source changes
are reversible in Git. Preserve the cooperating-process/workspace boundary.

Doctor groups by the full typed subject within its existing six categories.
Distinct findings remain separate rows with severity, plain title, stable code
and resolution. Equal candidate lists use full typed equality of cardinality,
ordered candidates, subjects, evidence and provenance. Equal evidence/actions
use their record equality; no text-based identity or message parsing. Supporting
facts render once per group. Category/overall actions already shown below are
omitted only from those duplicated summaries; additional actions remain visible.
Line and column are inline; byte ranges remain in unchanged JSON. Remove the
512-character human truncation. Candidate inclusion is explained without ranking
or automatic selection. All finding kinds get deliberate human labels.

Status leads with the observed installation and status, then non-complete
findings, context, routes, lifecycle, Libraries, recovery and the existing Next.
Compact summarizes current navigation by count; all other paths remain visible.
Expanded shows every navigation path and existing bounded largest-source detail.
Every recovery candidate and honest unavailable/count state remains visible.

Placement remains each command's Shared/Rendering. Doctor local presentation
helpers own titles, grouping/equality, and evidence/proposal rendering. Do not
promote command semantics into Shell. Snapshot expectations use small authored
results, independent of shipped Markdown wording. Focused Unit assertions prove
no lost kinds/actions, distinct occurrences, candidate equality distinctions,
unchanged typed results/JSON, full paths, and meaningful view differences.
Affected Integration/public output assertions change only to the approved text;
existing status/stream/effect assertions remain. Qualify managed and native at
this first human-rendering archetype before the second squash.

## Set 1 Qualification

Frozen source: `f3e1729630c7f8a6aab711280c04ebd1af116c70`, clean executable
inputs. Linux x64, .NET SDK 10.0.111, Node 24.19.0, npm 11.17.0.
`npm run build:native -- --sha --offline` passed without build warnings/errors.
`npm run test:built` passed all six suites: Unit 3238; managed and native
Integration 1745 each; managed, native and managed-to-native Public 123 each.
Every selected suite discovered/executed its reported count, with zero failures
or skips. The delivery manifest reports `tested: true` and binds the source and
artifact hashes. Native CLI SHA-256:
`c8e548d57791739ddb0b465dabf055c6f114ec76e758d0feb56fb75c5f250735`.
Direct native `--version` and `--help` passed. Targeted `dotnet format
OpenForge.Cli.slnx --no-restore --verify-no-changes --severity warn --include`
on the three changed C# files passed. `git diff --check` passed.
The initial sandbox denied compiler IPC/process spawning; authorized offline
execution with local process access passed. No product failure was found.

Root reviewed the actual three-file source/test boundary: neutral dependency
direction, selected delegate only, unchanged operation/status/streams, and
exception propagation. No material finding remained. The next set's contract
edits and this plan do not affect the qualified executable inputs. Raw reports
and native binaries are disposable; this tracked receipt preserves acceptance.

## Set 2 Review Checkpoint

Doctor/Status human renderers and their contracts are implemented. The first
full managed pass completed with Unit 3249, Integration 1745 and Public 123,
zero failures/skips; build and targeted formatting were warning/error free.
The representative worktree comparison used the qualified Set 1 native CLI and
this set's managed development CLI on the same workspace: Doctor expanded fell
from 46,286,800 to 8,794,916 UTF-8 bytes; compact from 12,936,317 to 5,974,387.
Both builds/views returned exit 3 and empty stderr. This is an output-size
observation, not a managed/native execution-speed comparison. Counts reflected
1 error, 3073 warnings and 8238 information findings in the observed result.

The root review found one final readability correction pack: put compact
resolution on the finding row, omit the redundant static valid-link sentence,
show known-empty counts once, preserve unknown counts explicitly, and use
correct singular/plural count labels. The valid-link title retains validity,
not merely existence. Candidate match coordinates now attach to the match label.
A new direct check prevents unknown counts from collapsing into no findings.
These final source changes invalidate the earlier executable receipt; rebuild
and rerun focused evidence, then the complete frozen managed/native gate before
squash. No operation or JSON implementation changed. The earlier byte-size
measurements remain historical observations until remeasured on the final build.

## Set 2 Qualified Predecessor And Status Grouping Correction

Candidate `065e2427a3d4c0104a848f24903c48fc79cdf818` passed
`npm run build:native -- --sha --offline` and all six `npm run test:built`
suites: Unit 3250, managed/native Integration 1745 each, all three Public modes
123 each; zero failures/skips, no build warnings/errors. Manifest `tested: true`.
Final managed dogfood output at this candidate: Doctor compact 5,590,885 bytes /
78,679 lines, expanded 8,581,094 bytes / 164,936 lines, both exit 3 and empty
stderr. Compact Status was 7,458 bytes, exit 3, empty stderr.

R2, presentation/readability: the real Status report repeated the same generated
navigation cause once per path, then listed those paths again under Routes.
This was revealed by the Status dogfood excerpt after the earlier Doctor-focused
correction. Group findings by exact code, status and cause; keep every distinct
subject. For the four typed generated-navigation finding codes, a subject may
be referred to Routes only when its exact path is present in the non-current
navigation rows. Missing or unmatched subjects stay in the finding group.
Keep all original findings, statuses, causes, ordering and JSON untouched.
Add focused evidence for both views, matching/unmatched paths and same-JSON
preservation. This is a second bounded correction justified by new real-output
evidence, not a restarted broad review. Requalify the revised candidate before
squash; the predecessor receipt does not claim the later source was tested.

## Set 3 Freeze: Library Human Views

This set starts after Set 2's qualified squash. It implements all five Library
human views from their existing typed payloads and aligns their Interfaces.
No operation, parser, filesystem, permission decision, serializer or JSON schema
change is needed. List remains a bounded registered-link read; Inspect consumes
its existing inventory. No renderer performs a scan or reconstructs missing facts.

Promote the identical human header/status/selection/text mechanics now used by
Doctor and Status into Shell/Presentation/Shared/Rendering/CliHumanText, with
those existing consumers and the five Library commands. Header and Next helpers
consume the existing CliPresentationRequest and ICliCommandResult boundary.
Keep title wording and all command meanings local. This is neutral repeated
formatting, not a universal result model or rendering tree. Existing Doctor and
Status output must remain byte-equivalent under this promotion; their snapshots
and focused tests remain the regression boundary. It also provides one place
for the later accepted terminal accents without adding colour in this set.

Library List identifies missing/empty/unavailable records honestly, retains
workspace selection and the explicit statement that source inventory was not
scanned, then shows registered paths/IDs and link states. Expanded adds exact
expected/observed link targets. A missing count is not inferred from an empty
array. Inspect groups comparison facts beside each source/destination identity,
retains actual registration and eligibility membership, and separately retains
registered or eligible rows absent from the comparison. Identity uses the full
typed path/ID values; no comparison relation invents missing membership facts.
Expanded adds raw link targets and supporting observations. Both retain findings,
coverage, exact paths and source IDs when available.

Attach/Sync/Detach consume their shared Identity, Record, Projection, Plan,
Permissions and Application records through Library-local rendering helpers.
Attach/Sync additionally render their Source observation; Detach stays source
independent and does not gain a source scan. Show every mapping and blocker,
planned directory/link/generated-region/record effect, relevant permissions,
application/verification/recovery state and residual path. Partial or interrupted
application does not make planned paths verified effects. Library generated-region
payloads contain hashes rather than diff content: show those actual facts in
expanded output and never read files or invent a diff to fill that absence.
Next comes only from the result, once; expanded may add its actual reason.

Supporting helpers stay below Library/Shared/Rendering, grouped by observation,
plan, permissions and application only when that improves navigation. Existing
closed result types and source-generated JSON remain intact. Human labels may
explain existing coordinates; stable finding codes and direct causes remain.
Avoid a generic mutation result, string dispatch, parser or new dependency.

Evidence: use small authored presentation fixtures and reviewed snapshots;
assert required mappings/effect paths, preserved/unknown state, interrupted
application and source independence directly. Compare JSON before/after human
rendering. Retain existing status/stream/exit and mutation safety evidence.
The shared-header promotion triggers the full managed/native gate. Qualify all
five command views and the unchanged Doctor/Status neighborhood before squash.
The set is reversible source/prose work for the local unreleased CLI, with no
new trust boundary or exceptional machinery. Raw artifacts remain disposable.

## Current Checkpoint

Set 1 squash: `5bb4aefa`. Set 2 squash: `54c28134`. Set 3 Library human
rendering is implemented; focused preservation tests and prose review are active.
Sets 4–8 remain required. User approval already covers these changes.

## Set 2 Final Qualification

Frozen source: `39f0ec3950aaa0caab76436c8e4727bc4f9dd9cd`. The R2 correction
passes 67 focused Status tests and formatting verification. Final reproduction:
`npm run build:native -- --sha --offline`, then `npm run test:built`, using
SDK 10.0.111, Node 24.19.0, npm 11.17.0 and Linux x64. Build completed with zero
warnings/errors. All six suites passed with zero failures, skips or pending
cases: Unit 3,252; Integration 1,745 in managed and native modes; public 123 in
managed, native and managed-against-native modes. The delivery manifest verifies
source and executable closures before/after execution and records tested=true.

Native CLI SHA-256:
`fade6dbb1cf48e2017411f66cc5bf5ae8c30eb5512a09d169aba7983fe06ced2`.
Native Integration SHA-256:
`d77ea5e15a002eccc06eb99c53089e311f058c7458151c8c3b18562287f5ba87`.
Native public runner SHA-256:
`d6d19be6f5b72ab7932547f5a9233d79281578196e6bb96ad42d42d978b71bb8`.

Direct native `status --view compact` in this worktree returns exit 3, empty
stderr and 4,303 stdout bytes. Its observed 13 changed and 16 unavailable
navigation paths appear once under Routes; grouped findings retain their distinct
codes/status/cause and reference that section. Incomplete workspace observations
remain visible. Earlier Doctor output-size evidence remains above; no operation,
kind, count, exit, default visibility or JSON changes occurred in this set.
Required evidence and snapshots are tracked; raw artifacts are disposable.
This receipt accompanies the authorized local squash; no remote effect occurred.

## Set 2 Integration And Set 3 Progress

Local squash `54c28134` preserves the concurrent delivery integration `e3f803bc`.
CLI source/tests are identical to qualified `39f0ec39`; the repository tree also
contains that separately integrated delivery tooling and its work records. After
merging develop into the presentation branch, both committed trees equal
`3857d425cf78499d6c6e100ffae63532673884aa`. The Set 2 six-suite receipt belongs
to its exact source/tooling predecessor, not a rerun using the newer delivery
scripts. The next full gate uses the integrated delivery entry point.

Set 3 implements neutral human headers/Next/text and all five Library views.
Existing 472 Library/Doctor/Status Unit cases pass after the initial source
change, including Doctor/Status snapshots. New fixtures cover incomplete and
interrupted output, exact paths, membership, unknown counts, planned effects,
permissions and unchanged JSON. An initial new-test build found a nullable test
access and three missing test imports; corrected before qualification. No product
or operation issue was inferred from those compile errors. Shared headers retain
Doctor/Status output. Required fixtures remain tracked source, not artifacts.

Set 3 focused verification: 574 Library, Doctor, Status and Shell Unit cases
passed, zero failures/skips, after the corrected test build (zero warnings/errors).
Review corrected the human `retired` label to `no longer eligible`: retirement
must not claim that source bytes were physically removed. `added` is `new eligible
file`. The pending final gate includes this wording correction. The reviewed
List snapshot is authored from a small fixture; semantic assertions independently
cover counts, IDs, paths, unknown state and unchanged JSON. Existing public
journey counts and full JSON graphs remain unchanged.

Set 3 final review retained blocked/unavailable ownership observations in compact
output even when the result has no duplicate collision finding. The corresponding
Unit regression is part of the final candidate. Finding Library IDs differing
from the selected identity are also retained. No operation or JSON projection
changed. The final native build and six-suite gate now own qualification of all
source, tests and these bounded corrections.

## Set 3 Dogfood Review — Observation Labels

The managed 4bd30bd7 CLI passed 12 direct Library calls in an owned temporary
workspace: both views of Attach preview, List, Inspect, Sync preview and Detach
preview; then Attach and source-independent Detach application. All exits were
zero with empty stderr. The ten read-only calls preserved a complete snapshot
of files, directories, raw links and isolated external state. Source bytes were
preserved by Attach; Detach succeeded after the source file was deliberately
removed and deleted only the registered link. All temporary fixtures were removed.

This exposed a presentation correction before acceptance. Mutation relation
Retired includes source-independent Detach, so `no longer eligible` overstates a
source observation. LibraryMutationCompletionProjection.Relation confirms that
meaning. Use `not in the intended Library` and a neutral `Comparison` label.
Label the mutation record and link observations as before-change facts, so a
completed detach does not make the observed prior record/link appear still
installed. The current frozen build may finish, but it is not the final accepted
candidate and will not be squash-merged. Apply this bounded wording correction,
add a direct regression, and qualify the revised source before integration.

## Set 4 Freeze — Extension Human Views

Starts after the qualified Library squash. Cover List, Inspect, Create, Install,
Update and Remove using their existing typed results. Update each affected
Interface and Inspect Behavior's prohibition on human regrouping while keeping
operation and JSON ordering unchanged. No command selection, dependency closure,
force/prune/permission policy, ownership, filesystem effect, recovery, finding
kind, status, exit or JSON change is included. The user-approved compact JSON and
colour remain separate later sets. No dependency or exceptional machinery.

Use the neutral human header/status/Next/text behavior already accepted in Set 3.
Create operates on a catalogue and always has Workspace=null by contract; retain
its catalogue/destination framing and unchanged workspace-lifecycle fact rather
than implying a failed workspace lookup. If Library and Extension need identical
outcome wording, promote that small complete formatting behavior to CliHumanText
and preserve Library output; do not introduce a common command result or engine.
Keep command-specific facts and rendering below each leaf's Shared/Rendering.
Retain Extension diagnostic escaping and its limits; human metadata alone uses
neutral control-character sanitization with exact untruncated paths.

List preserves separate selected Installed/Available sections, exact source,
coverage/trust, IDs and known versions. Expanded adds authored name/description,
counts and managed-path details. An empty returned array with incomplete coverage
is not an unqualified empty installation. Inspect keeps the exact ID/source,
installed/available and lifecycle states, nullable counts, dependency coverage,
comparison mode and all significant path differences. Expanded explains installed
baseline/current workspace/selected package sides, ownership and fingerprint
facts, full known dependency/path inventories and derived generated navigation.
Unknown intended counts stay unknown; no renderer converts a missing package to
zero. Preserve all finding identities, locations and candidates; never infer a
new repair from a relation or a missing source.

Create shows manifest metadata/dependencies and all intended/applied scaffold
paths in both views; group exact effect identities without calling an intended
path applied. Expanded adds concrete verification and generated-file detail.
Keep failed/interrupted or no-op outcome distinctions from the typed result.

Install/Update/Remove place outcome, workspace/source/selected packages, findings
and safety conditions before changes. Preserve dependency ordering in operation
and JSON; human grouping uses exact path/package identities and preserves every
effect action, outcome and residual. Update groups comparisons beside actual
effects; Remove groups owner/keep/delete facts beside effects while distinguishing
kept ownership from files kept unmanaged. Install retains footprint paths absent
from its effect rows. Keep additional unmatched facts, not just joined rows.
Expanded adds full supporting Framework/lifecycle/verification/permission details;
both retain effective force/prune/automatic/preview choices, retained dependents,
protected local content, residual recovery path/state and the actual required Next.

Evidence: small authored snapshots plus independent identity/count/unknown-state,
partial-effect/recovery, grouping and unchanged-JSON assertions. Keep all existing
public journey counts. Readability does not justify new operation behavior or
claims about deleted source bytes. Direct managed/native command dogfood and the
full six-suite gate qualify this coherent family and any neutral promotion before
its authorized local squash. Source is reversible in Git; temporary fixtures and
raw artifacts are disposable, with tracked reproduction/evidence records here.

The before-change/retirement wording correction is now implemented with a direct
Unit regression. The 4bd30bd7 native build finished with zero warnings/errors,
but its six-suite gate was intentionally not run because dogfood had already
identified the wording issue. The revised candidate replaces that build as the
acceptance target. No issue was found with the actual source-independent Detach
behavior, which remains unchanged.

## Set 3 Public Evidence Alignment

Candidate cccfa3b9 builds with zero warnings/errors; its 3,270 Unit and 1,745
managed Integration cases pass. The managed public suite passes 122/123 and
exposes one outdated heading assertion in RequiredIdOmissionIsInvalidWithoutObservation.
It expected lower-case `library inspect` inside the old heading. The new heading
is `Library inspection: invalid.`. Direct native omission returns the expected
exit 4, empty stdout and exact stable `library-inspect.invalid-id` finding.
Replace the wording assertion with that diagnostic-code assertion; retain exit,
stream, cause/status and unchanged-workspace/infrastructure checks. No production
fix is needed. The gate stopped before its final three modes, so no full-suite
pass is claimed for cccfa3b9. Qualify the fixture-aligned candidate next.

Set 4 evidence order: after focused in-memory tests and a managed publish, run
all changed Extension public command classes before the native freeze. Their
human assertions include Create intended/applied counts and Remove selected IDs,
prune/source-preservation/no-op facts. Align wording assertions deliberately while
retaining independent effect, stream, status and unchanged-byte evidence. This
catches presentation-fixture mismatches before the expensive native qualification;
it does not replace the final six-suite gate. No additional public journey is added.

## Set 3 Qualified Closeout

Qualified source: `4d61ecb63244b3d3468d90ea5e94a6511c908d84`.
`npm run build:native -- --sha --no-restore` passes with zero warnings/errors;
`npm run test:built` passes all six Linux suites: 3,270 Unit, 1,745 Integration
in managed and native modes, and 123 public cases in each of managed, native
runner/native CLI, and managed runner/native CLI modes. All report zero failures,
skips, pending or other outcomes. The manifest confirms clean executable source
and `tested: true`; source and built closures were checked before and after tests.
Native CLI SHA-256: `d02f337ffbc370fcfc5f4608a67574a2566ea067ea120891f3aaebcb40bb6cb4`.
Reports under `artifacts/delivery/linux-x64/reports` are disposable; the commands,
source identity and receipt here are the durable evidence.

This closes Library List/Inspect/Attach/Sync/Detach human views and the neutral
header/Next promotion. Existing JSON graphs and mutation behavior are preserved.
The one public assertion correction checks stable diagnostic identity instead
of old heading wording. The direct managed/native dogfood and focused semantic
regressions are recorded above. Extension contract drafts are excluded from this
set's commit/squash; their frozen implementation is next. Main develop's separate
`b88f5e7a` delivery/help documentation change has no CLI source changes and is
preserved during local integration. No remote action or global CLI refresh occurs
in this set; the final installation refresh remains Set 8.

Set 3 squash is `56538d635ce0722fdb9bc8fd53a0a810970b84a0` on local develop.
The main checkout had concurrently switched to feature/test at b88f5e7a; the
squash initially landed there. After checking its exact HEAD and clean state,
the commit was assigned to develop with a compare-and-swap ref update, then
feature/test was restored with `reset --keep` to its unchanged b88f5e7a parent.
Both refs and the clean main checkout were verified. The presentation branch
merged develop and has an identical committed tree. Future integrations use an
owned integration checkout or explicit guarded ref operation, not an assumed
main-checkout branch. Set 4 implementation now starts from that qualified tree.

## Set 4 Implementation Checkpoint

All six Extension human renderers now consume their existing typed results.
List preserves selected sections, coverage, source availability and versions;
expanded adds names/descriptions and inventory counts. Create keeps every exact
scaffold path and separates intended from applied effects, including verified
no-ops. Inspect groups full path identities, retains findings/candidates and
unknown counts, and expands dependency/fingerprint/owner observations. Unmatched
source files with no target are not joined to workspace paths.

Install/Update/Remove now honor both views, place findings and permissions before
paths, retain every effect action/outcome and recovery fact, and group repeated
path observations without changing result arrays or operation policy. The shared
Library outcome wording is promoted unchanged to neutral CliHumanText for actual
Extension consumers. No JSON source graph, diagnostics, parser, operation or
filesystem source changes are part of this set.

The initial 656 existing Extension/Library/Shell Unit cases pass. Added evidence
now covers unknown counts, long finding paths and source line/column, verified
Create no-op paths, preserved planned effects in Update/Remove, and interrupted
Install effects plus unmatched footprint/recovery paths. Focused and public
verification of the revised source is pending; no Set 4 qualification is claimed.
The published Create/Remove wording assertions are aligned while all exit,
stream, real filesystem, unchanged-source and no-op evidence stays intact.

The user's later instruction is to leave feature/test alone. The branch had
already been restored before that instruction arrived; no further operation
will target it. Develop already contains the qualified Set 3 squash, so no
duplicate Library squash is needed. Continue only in owned task/integration
worktrees and verify the branch identity at every integration boundary.

## Set 4 Direct Dogfood Corrections

The initial revised candidate passes 665 focused Unit cases and all 18 existing
Extension public journeys (managed runner/managed CLI). The first public filter
command used an unsupported middle wildcard; the runner rejected it before
execution. The corrected supported prefix is `--filter-class
'OpenForge.Cli.EndToEndTests.PublishedExtension*' --minimum-expected-tests 18`.
No test platform workaround was added.

Direct managed List/Inspect against the worktree confirms incomplete installation
coverage and six available embedded packages. Inspect exposed a human-only
misstatement: Subject.Candidates also retains a resolved package, so a blanket
“no choice made” label was false. The label is now neutral “Package matches”;
a regression preserves resolved identity. Counts.Dependencies is the resolved
package count, including the selected package, so the output now says “resolved
packages” instead of implying that every resolved package is a dependency.

Compact Inspect now counts healthy unchanged/source-only observations and files
without generated regions. Exact unavailable paths remain visible. Expanded keeps
all observations and shared source paths are shown once beside their declarations.
The count and omission hint are explicit. Added regressions cover these distinctions
and known-empty versus unavailable List inventory. These are presentation-only
corrections; the final candidate is being rebuilt and tested before native freeze.

The next managed candidate passes 669 focused Unit cases and all 18 Extension
public journeys, with zero failures/skips and warning-clean builds. A direct
17-invocation managed journey uses an owned temporary workspace/catalogue and
isolated XDG state: Framework install; both Create previews and scaffold apply;
both List views; both Install previews and apply; both Inspect views; a source
edit, both Update previews and apply; both Remove previews and apply. All return
exit 0 and empty stderr. All 12 read-only/preview calls preserve complete
file/directory/link/state snapshots. Install/Update destination bytes match source;
Remove deletes the managed target while preserving source bytes. All fixtures
are deleted by the owning TemporaryDirectory; no experiment program is retained.

That journey exposed 20 unchanged navigation paths in compact mutation output.
The final correction counts navigation-only unchanged observations when no
footprint path, comparison, plan or effect needs that path. Changed, uncertain
and actual-effect rows remain visible; expanded retains the full observations.
Existing typed fixtures now assert this omission rule and one path heading per
effect/comparison group. List snapshot comparison normalizes line endings through
the BCL to avoid platform-dependent wording evidence. Qualify the corrected
source before native freeze; prior receipts do not claim this correction passed.

The new grouping assertions caught that a finding target and an effect path used
identical unlabelled rows. Finding targets now have an explicit Target label.
They remain separate from path/effect groups: a finding's general Target string
is not always a typed workspace path, so equal text alone cannot justify merging
those identities. The assertions still prove one actual comparison/effect heading.

Source inspection of ExtensionInstallResultFactory clarifies that footprint
generatedRegions includes every topology observation, including unchanged hosts;
those are not all planned writes. The human label now describes an observation.
Compact may count these navigation-only unchanged observations when no payload,
directory or effect needs the path; unknown/unmatched footprint rows stay visible.
This refines the initial all-footprint-path retention statement using the actual
typed meaning. Payload/directory paths and every effect remain in both views.
The install fixture explicitly covers an unchanged navigation footprint row.

## Set 4 Native Freeze

Final focused source builds without warnings/errors and passes 669 Unit cases
(Extension, Library and Shell) plus all 18 Extension managed public journeys.
The final explicit-target-label and navigation-only-footprint rules are included.
The prior two failed grouping assertions are resolved; they were not removed or
weakened. Source metadata/paths, finding status/cause, planned/unknown effects,
recovery, unchanged JSON and command status/stream boundaries remain covered.

Direct corrected Update/Remove previews retain the affected package path and
summarize only unchanged navigation. Both views return exit 0/empty stderr and
preserve all owned fixture/state bytes. Compact Update is 1,123 bytes versus
3,043 expanded; Compact Remove is 1,087 versus 2,634 expanded for those temporary
paths. These are output-size observations, not token or runtime benchmarks.
Formatting verification passed before the last bounded text/navigation changes;
their builds and focused/public evidence are current. The complete native build
and six-suite acceptance run are next. No Set 4 squash is authorized by evidence
until that final gate passes.

## Set 4 Full-Gate Fixture Correction

Frozen 00ebcf68 builds managed and all Linux native targets without warnings or
errors. Final changed-source formatting verification passes. Full Unit passes
3,283 cases; Integration passes 1,738 of 1,745 and stops the six-suite gate on
seven stale human-label expectations across four Extension test files. Permission
reapproval still saves the reviewed grant and verifies the affected bytes; Create,
Install and List retain their stream/selection/no-write assertions. Align the
labels with the approved output while preserving all those behavioral assertions.
No product correction or weakened assertion is required by this failure.

The direct final native journey passes 14 calls, including ten read-only calls
that preserve complete file/directory/link/XDG snapshots. Both views cover List,
Inspect and Install/Update/Remove previews. Applied Install/Update match package
bytes and Remove deletes its managed target while preserving the source. Native
SHA-256 is `4c555f3fd04eeffa057ecaf44ad00dbdc3a113cff74c83e2f0b96e813d0bd728`.
Compact Install is 1,055 bytes versus 3,811 expanded in the owned fixture; these
are size observations, not token or runtime benchmarks. An initial temporary
journey check expected a different hint phrase and stopped after the first
preview; correcting that wording expectation allowed the full journey to pass.
No experiment program is retained.

An owned detached integration checkout is prepared from develop 56538d63. A
fresh read shows the main checkout now has develop checked out. Before final
integration, recheck branch identity and clean state; update only develop through
the authorized integration boundary. Never alter feature/test.

All 35 Integration cases in the four corrected classes now pass, with zero
failures/skips and a warning-clean Release build. Install's assertions retain
selected-versus-dependency identity and order in the new labels. The permission
assertion now names the complete approved/replace/verified record outcome. No
production source changed after 00ebcf68. Freeze this corrected evidence and
repeat the full managed/native gate before qualification.

## Set 5A Read-Command Freeze

After Set 4 qualifies and integrates, implement Context, Find, References, Route
List and Route Inspect human views sequentially. This is a rendering-only set:
requests, operations, typed facts, ordering, source selection, scan boundaries,
statuses, exits, diagnostic kinds and JSON remain unchanged. It uses existing
BCL formatting and leaf-local helpers. No dependency or exceptional machinery.
The risk is hiding a partial result, confusing a candidate with a resolved target,
or altering selected authored content; edits are reversible in Git.

Context keeps exact projection text and overwrite boundaries. Put workspace,
status, selected content/link depth, startup inclusion and incomplete findings
before content. Compact prints each ordered path at its source boundary, or as
the selected paths-only projection, instead of duplicating the full path list
before source blocks. Expanded adds existing loading reasons and layer metadata.
Keep all unresolved links and full finding coordinates in both views; raw byte
ranges remain JSON. Do not infer new loading or safety facts.

Find compact retains its exact TSV summary and ID/path rows. Findings retain
full coordinates and candidates without the old 240-character subject clamp.
Expanded leads with match count, status and coverage, then the actual query,
selection and match evidence. Requested projections remain exact. Keep authored
content helpers separate from framing so future colour never touches them.
Use the existing typed Next command rather than substituting generic prose for
it; retain the reason in expanded output. Update the corresponding Interface
examples and Next presentation requirements in this same set.

References keeps requested directions, per-section status/coverage, occurrence
count and every physical occurrence in the existing order. Name the operation
“Direct links” and replace “Level 1” with that clear boundary. Findings precede
occurrences and retain code/status, cause, exact subject/path/line/column and
candidates. Compact uses source-to-target rows with resolution and layer facts;
expanded adds authored destination, fragment, target identity/layer, destination
line/column and actual scan origin in plain language. External URLs explicitly
remain unchecked over the network. Use exhaustive typed resolution vocabulary,
not transformations of enum spellings. Do not collapse distinct occurrences.
The authoritative contract remains under references-candidate; this work does
not perform its separate route migration.

Route List keeps every ordered hierarchical ID/path/description/tag row. Compact
omits repeated structural explanations while retaining root/depth/coverage and
all unresolved boundaries/findings. Expanded adds each row's actual parent,
depths, child count and source/selection facts once. Share only the identical
leaf-local vocabulary and framing between the two views. Do not invent tree
ancestors or use generated navigation as authored topology.

Route Inspect puts status and meaningful observations/conditions before the
profile and keeps exact candidates and operation Next. Group the existing
profile into route structure, when it is read, context size and customization.
Expanded includes inherited/local Axioms, selected closure and task-start overlap
measurements once; compact retains its required own/addition/descendant measures
and unavailable/not-applicable distinctions. Keep physical layer identities and
full observation paths. Do not diagnose content or invent advice.

Evidence: adapt small authored presentation fixtures and direct field/identity,
order, uncertainty and exact-content assertions. Run the affected Unit and
Integration/public classes during implementation, retaining their read-only
snapshots, stream and exit checks. Include the Integration boundary from the
start so human wording assertions are not discovered only in the final gate.
The five-command public presentation wave triggers complete managed/native
qualification before its squash. Sets 5B/5C remain the ten mutation commands;
compact JSON, automatic colour and reusable guidance follow as Sets 6–8.

## Follow-Up After This Presentation Task: Configurable Thresholds

The user requests analysis after the current presentation task is complete.
Investigate a thresholds object in open-forge.json for relevant diagnostics or
commands, with typed shared defaults and explicit default/override combination.
Their starting example is a warning threshold of ten sibling files for files
without useful scope. This is an analysis request, not accepted configuration
syntax, diagnostic behavior or a literal universal limit.

After Sets 5–8, inspect the actual configuration schema, loading/validation,
existing thresholds, affected typed diagnostics, and ownership of defaults.
Clarify which counted files and route/scope relationships the sibling example
means; compare configuration-level and command-specific ownership. Recommend
units, boundary comparison, missing/null/invalid handling, partial override
semantics, default placement, output visibility and evidence. Avoid duplicate
constants or a generic untyped settings engine. Return the concrete proposal
before implementing new configuration or changing diagnostic behavior.

Progress reported at this request: approximately 40% of the full approved
presentation task, an estimate rather than a measured work ratio. Human views
for 13 of 28 commands are implemented; Sets 1–3 are integrated and Set 4 is in
final qualification. Remaining human views, compact JSON, automatic colours,
guidance and final install/dogfood remain required work before this analysis.

## Set 4 Qualification

The final frozen candidate is `3deb2ebcea8193ce3c922ac6f63f89451c1ca69b`.
`npm run build:native -- --sha --no-restore` builds all managed and Linux native
targets without warnings/errors. `npm run test:built` passes all six suites:
3,283 Unit; 1,745 Integration in managed and native modes; 123 public cases in
managed/managed, native/native and managed/native modes. Failures, skips, pending
and other results are zero. The manifest confirms tested=true, dirty=false, the
exact source SHA and unchanged artifact closures. Native CLI SHA-256 is
`c2ad4ae425ea73a2e2902a970a40f3e18d467bef291e31ff38376d68789bfad4`.

Reproduction starts from that committed candidate using the two commands above.
All required inputs are tracked; build outputs and reports are disposable. The
seven stale Integration wording cases are corrected and covered by this final
run. Product source is unchanged from 00ebcf68; the final native journey on that
product is recorded above. Final source formatting and diff checks pass.

This qualifies the six Extension human views and the neutral outcome-helper
promotion. Operation behavior, typed result graphs, statuses/exits, JSON,
permissions, recovery and filesystem semantics are unchanged. Update local
develop through the owned integration checkout and verify the qualified tree.
Feature/test remains outside every mutation target.

## Set 4 Integration And Set 5A Activation

Set 4 is squash-integrated into local develop as
`a8a2e48f73e0d4b960f639d0f8c31a5c99ab8501`. The owned integration checkout's
squash tree equals qualified closeout 49ad3397 exactly. The main checkout was
verified clean and on develop immediately before its fast-forward; it remains
clean and the feature/test ref is unchanged. The presentation branch merges this
develop result and has the same tree. No remote action occurred.

Set 5A is active, beginning with References, followed by Context, Find and Route
List/Inspect. Its frozen meaning and evidence boundary above remain in force.
References Interface and Behavior now describe the approved human presentation;
JSON and operation contracts remain unchanged.

## User Pause: Reconsider Default Detail And Result Limits

The user explicitly pauses implementation to review the presentation direction.
Do not continue Set 5A or later implementation until the discussion resumes it.
Sets 1–4 remain qualified and integrated. Set 5A has only uncommitted References
Interface/Behavior wording edits and this activation record; no Set 5A renderer,
operation, test or JSON source has changed. No build/test process remains active.

Save these new proposals without treating them as approved behavior:

- A global --limit or equivalent for large result sets. Analyze whether it limits
  displayed rows or executed work, what counts as a result per command, ordering,
  truncation disclosure, total counts, JSON behavior and required effect/recovery
  details. No spelling, default or limit policy is selected.
- Make compact the default. The currently implemented/default contract remains
  expanded; the user is reconsidering it because ordinary output exposes too
  much explanatory detail. Separate useful expanded output from verbose/debug
  diagnostics rather than assuming those roles are interchangeable.
- Make common queries much smaller, especially Find by tag: the useful answer
  may be only the matching files. Reassess whether the existing compact TSV
  summary plus ID/path rows is still too much and whether compact should become
  simpler or another explicitly justified view is needed. Do not add a third
  view, remove identity/coverage, or change defaults without this discussion.

At this pause, revised human renderers are implemented for Doctor, Status; all
five Library commands; and all six Extension commands: thirteen of twenty-eight.
Context, Find, References, Route List/Inspect and the ten remaining mutation
commands still use their earlier renderers. Compact JSON, automatic colour and
final reusable guidance/install refresh remain unimplemented. View fallback is
already integrated. Optional filtering and configurable-threshold analysis remain
saved follow-ups. Overall task progress was last estimated at forty percent;
this direction review can change the remaining scope and estimate.

## Resumed Priority Slice: References, Context And Find

The user resumes implementation for exactly References, then Context, then Find.
This narrows Set 5A to those three high-value read commands. Route List/Inspect,
remaining mutations, compact JSON, colours and final guidance are deferred.
Complete and qualify this three-command slice, squash it into develop, then
report completion and stop for the user's minimal-output discussion. Keep current
expanded defaults, Find TSV and both existing views; no limit/filter/third view or
threshold configuration is authorized by this resumption. Existing frozen safety,
exact authored content, result/JSON and read-only boundaries remain unchanged.
Use focused evidence for each leaf and one complete managed/native gate for this
three-command presentation wave. No agents, remotes or feature/test changes.

## Three-Command Implementation Checkpoint

References human implementation is complete at the focused boundary: 64 Unit,
25 Integration and three public journeys pass. It retains every typed occurrence,
section coverage, location, target uncertainty and finding; JSON remains unchanged.
Two stale Integration labels were aligned without weakening behavior evidence.
A local name collision between a rendering helper and the Resolution namespace
was corrected through an explicit typed helper call, without a parser workaround.

Context initially passes 25 Unit, 51 Integration and three public journeys.
The final inspection retains the explicitly required compact layer sequence as
an order value. Findings now precede authored content; source boundaries avoid a
second path list when the content already includes source blocks. Exact selected
text and overwrite framing remain unchanged. Presentation fixtures normalize
platform framing newlines while preserving authored-text assertions, and a
control-character case uses an explicit newline rather than host line endings.

Find places the useful match list ahead of query explanation. Compact TSV is
unchanged; both human views retain complete finding subjects/coordinates and
actual typed Next commands. The immutable FindResult already validates Next, so
rendering no longer duplicates its command-selection policy or substitutes prose
for the command. Selected-content rendering is a separate leaf-local helper;
its body/section bytes and framing remain unchanged. Byte offsets remain JSON.
Projection-only vocabulary stays with that content helper, while the two views
share their actual common vocabulary. The first Find Unit run passes 207/208;
one stale zero-match boundary label is corrected. Full focused rechecks of all
three commands are running before direct dogfood and the immutable full gate.
No operation, parser, diagnostic kind, serializer or configuration source changes
are part of this slice. The user-deferred remainder stays deferred.

## Three-Command Qualification Freeze

Final focused evidence passes 297 Unit, 117 Integration and 11 public journeys,
with no failures or skips. Release build has zero warnings/errors; changed C#
files pass dotnet format verification at warning severity. Direct managed CLI
trials pass compact, expanded and JSON for References, Context and Find in an
isolated installed workspace. Each read leaves the complete workspace and XDG
state snapshot unchanged. Context preserves selected body bytes including a tab,
trailing spaces and mixed newlines. Find preserves its compact TSV ID/path rows.

The initial cross-view JSON comparison incorrectly expected identical documents.
Context and Find already record supplied/effective view in result.presentation;
the unchanged serializers and existing contracts establish that metadata. Trials
validate those exact view values separately and compare all remaining JSON data.
References JSON is identical across views. These checks pass; no serializer or
operation change was needed. The disposable trial workspace was removed and no
experiment script was retained.

Freeze the current three-command source for the complete Linux managed/native
build and all six suites. Expected counts are 3306 Unit, 1745 Integration per
managed/native lane and 123 public journeys per each of three runtime lanes.
Do not change executable inputs or HEAD during this gate. The narrowed user
scope remains References, Context and Find only; after qualification and local
squash, report completion and stop for the minimal-output discussion.

Direct native trials on the frozen 39eed987 CLI also pass References, Context
and Find in compact, expanded and JSON modes, including exact Context body bytes,
Find TSV rows, documented view metadata and unchanged workspace/XDG state. The
managed build reports zero warnings/errors. Full native test executable build is
still running; do not treat these direct trials as the complete suite receipt.
Find Interface table formatting is normalized by Prettier; all four changed
contract documents pass the formatting check after this prose-only correction.

## Three-Command Final Qualification And Closeout

Qualified source: `39eed987361464e427aeb55724ce7dee78bd005a`. The complete
`npm run build:native -- --sha --no-restore` and `npm run test:built` gate passes
on Linux x64. Both managed build steps report zero warnings/errors. The delivery
manifest records this clean source and `tested: true` after validating artifact
closures. Final counts, each with zero failures, pending, skips or other results:

| Suite                                      | Passed |
| ------------------------------------------ | -----: |
| Unit                                       |   3306 |
| Managed Integration                        |   1745 |
| Managed public journeys                    |    123 |
| Native Integration                         |   1745 |
| Native public journeys                     |    123 |
| Managed public journeys against native CLI |    123 |

Native CLI SHA-256:
`55e905f0f1fadb5e55269c8784a315e78a1bf702d4e2d8ddfd30beb767583432`.
Reports and native binaries are disposable build outputs. This tracked receipt,
normal build/test commands and committed source/tests retain the qualification
meaning without depending on artifacts. The focused and direct managed/native
receipts above supplement the complete suites. The final formatting correction
and continuity edits change only Markdown outside executable build inputs.

References now presents direct links, readable resolution states and actionable
source coordinates. Context puts findings before exact authored content and
avoids duplicate path lists when content already has source boundaries. Find
places matches before search details and retains its existing compact TSV. Both
human views keep typed status, coverage, uncertainty and actual Next commands;
JSON data, parsing, operation behavior, diagnostics and authored content remain
unchanged. Affected command Interfaces and References presentation wording in
Behavior are aligned. No maintenance policy or operation contract change is
needed for this renderer-only slice.

This closeout accompanies the authorized squash onto develop from baseline
`a8a2e48f73e0d4b960f639d0f8c31a5c99ab8501`. Integration must retain exactly
this closeout tree, verify the clean develop checkout before fast-forward, and
leave feature/test at its observed `b88f5e7af99c02fd85626ba1fc6182c32ed17798`.
Merge the resulting develop squash back into the presentation branch with the
same tree. No remote action or installed-CLI refresh belongs to this closeout.

The requested three-command slice is complete. Report completion after local
integration and stop. All other presentation work remains deferred, including
Route List/Inspect, remaining mutations, compact JSON, colour, final reusable
guidance and installed-CLI refresh. Keep the expanded default and current compact
views until the user discusses the proposed even more minimal output. Limits,
filters, a compact default and configurable thresholds remain saved proposals.

## Remaining Commands Resumed; Stop Before Colour

The user resumes the remaining command presentation work and explicitly requires
a stop before colour. Complete remaining human command views and the accepted
command JSON-view work sequentially, then report and stop. Do not begin automatic
colour or final guidance in this continuation. Limits, display filters, compact
defaults, a third/minimal view and threshold configuration remain proposals only.
At resumption, human views are complete for 16 of 28 commands; the overall
presentation estimate is about 55%, not a measured effort ratio. The completed
References/Context/Find slice is 100% and squash-integrated as 5fa590a4.

Set 5B starts with Route List and Route Inspect. Preserve operation results,
row membership/order, typed reading facts, exact paths, measurements, uncertainty,
status/exits, stream policy and read-only effects. Align human output with the
approved gallery: useful route identity/tree first, understandable reading
behavior, concise compact rows, expanded facts without repeated empty sections.
Both views retain required coverage, findings and actual typed Next commands.
Existing defaults and JSON schemas remain unchanged during the human slice.

Applicability: these reversible renderer edits affect displayed CLI facts, not
workspace mutation. Reuse typed command results, existing text escaping, neutral
CliHumanText presentation and ordinary BCL formatting. No new library, custom
parser, compatibility shim, reflection or exceptional machinery. Keep helpers
in the owning command's Shared/Rendering scope. Freeze each coherent set before
its full managed/native integration gate; use focused in-memory rendering and
existing application/public journeys to establish the local boundary first.
Direct review checks the applicable Interface/Behavior/maintenance meaning.
Update affected contracts and checkpoints with each qualified local squash.
Keep feature/test untouched; use no agents, experiments retained in source, or
remote operations. Current baseline is clean develop 5fa590a4 and feature a91a3c8b.

## Set 5B Route Human Views: Focused Qualification

Route List/Inspect implementation and affected Interface/Behavior presentation
wording are complete. Production changes are confined to leaf Shared/Rendering.
List shares identical local vocabulary/framing and preserves every ordered
ID/path/description/tag row; expanded adds actual structure and source facts.
Inspect keeps physical layers, reading and measurement facts, moves meaningful
conditions before the profile, and removes repeated measurements/explanations.
Both display the actual operation Next command. Ordinary quotes in a Next command
remain usable; source fields retain existing escaping without the old human
subject clamp. No operation, parser, JSON serializer or configuration changed.

Focused final evidence: 226 Unit, 178 Integration and six public journeys pass,
with zero failures/skips. Release build reports zero warnings/errors. Direct
managed CLI trials in an isolated installed workspace pass compact, expanded and
JSON for both commands; JSON is identical across human views and full workspace/
XDG snapshots remain unchanged. The disposable fixture is removed; no experiment
program is retained.

The initial Unit/application/public failures were old labels and are corrected
without dropping stream, selection, effect or status assertions. Strengthening
an existing ordering check exposed a false-positive pattern: IndexOf returned -1
for an absent Next line and the old less-than comparison passed. The corrected
check verifies presence and order only when the typed action exists, and absence
when it does not. Four added Inspect cases prove conditions-before-profile,
unchanged JSON, nonduplicated own-size output and complete long subjects. Local
helper/test variable name collisions were fixed through explicit naming.

Freeze after final changed-file formatting verification. This two-command
integration set requires the full Linux managed/native build and six-suite gate.
Expected full counts: 3310 Unit, 1745 Integration per managed/native lane, and
123 public journeys per each of three runtime configurations. Keep HEAD and
executable inputs unchanged during that gate. After its qualified local squash,
continue the ten remaining mutation human views, then accepted command JSON
views. The user requires a stop before colour; guidance remains pending.

Final changed-file `dotnet format --verify-no-changes --severity warn` passes.
The Set 5B source is ready to freeze for the complete managed/native gate.

## Remaining Human Mutation Work: Investigation Checkpoint

While the immutable Route gate runs, read-only inspection covers the approved
gallery, output contracts and current renderers for Index, Install, Update,
Repair, Cleanup and Route Create/Init/Update/Move/Remove. No mutation-renderer
source has changed yet. Use two bounded sets after Route integration: core
maintenance (Index, Install, Update, Repair, Cleanup), then the five Route
mutation commands. Each gets focused checks from the start and one complete
managed/native integration gate before its squash.

Common retained facts: every actual affected/preserved/protected path, mode and
authorization flags, typed outcome and uncertainty, coverage/safety, verification,
residual/recovery identity, exact preview/diff and actual Next command. Compact
may omit repeated explanation, absent optional fields and expanded fingerprint
detail; it cannot hide failures, partial effects or required review content.
No schema, effect, consent, lock, recovery, parser or status policy changes belong
to these human sets. Keep shared neutral framing with CliHumanText and semantic
wording at its real command owner; no universal mutation payload model.

Concrete presentation issues for the next sets:

- Index no-op early returns omit common identity/status/framing and can bypass
  findings. Retain verified no-op meaning while rendering required facts once.
  Keep every exact bounded dry-run diff and every partial effect identity.
- Install exposes inventory fingerprints in compact output before useful effects.
  Put embedded-source identity and footprint together, with expanded provenance.
- Update should put useful effect/preservation rows first and comparisons beside
  their paths; retain force/prune, lifecycle trust, protected paths and uncertainty.
- Repair starts Library details before the command header, hides most findings
  and selected effect details in compact, and repeats application information.
  Both views need actual selection/effect/remaining facts and preview content;
  extended candidate evidence and preflight explanation can remain expanded.
- Cleanup compact omits candidate-only preview rows; preserve every candidate and
  effect, distinguish intended removal from verified removal, and explain final
  workspace validation without claiming it ran during preview.
- Route Create/Init/Update hide some findings or recovery detail in compact.
  Init also substitutes its own prose for the typed Next command. Preserve all
  required cases and display the operation action. Avoid describing planned or
  unknown effects as completed merely from apply mode.
- Route Update should show each supplied field's actual changed/unchanged values,
  retain Template protection and show its cause once.
- Move/Remove must retain every rewrite/detachment, source location, category
  member, effect and protected path. Fingerprints and detailed source evidence
  may be expanded, but exact preview evidence and uncertainty stay visible.

These are renderer alignment choices under the accepted examples. Confirm each
against its concrete result and contract before implementation; do not infer new
operation behavior from the examples. The user-defined stop remains before colour.

## Set 5B Complete Gate And Integration Closeout

Frozen source `939ab802c18fa672ee38b269121a87419a2e08e2` passes the complete Linux
managed/native build and all six suites: Unit 3,310; managed and native Integration
1,745 each; managed public, native public and managed-on-native public 123 each.
All report zero failed, pending, skipped and other results. Build reports zero
warnings/errors. Reproduce with `npm run build:native -- --sha --no-restore`, then
`npm run test:built` on that source. The delivery manifest reports `dirty: false`
and `tested: true`. Native CLI SHA-256:
`637d1e8a0b16ce8448c03bd6d732b52a1c71d91fbea5a8c8b04113abf3a50e78`.

Only tracked checkpoint prose changed during this gate. Integrate this closeout
as one squash onto clean local develop `5fa590a4`, checking candidate/integration
tree equality and preserving feature/test `b88f5e7af99c02fd85626ba1fc6182c32ed17798`.
Human presentation coverage becomes 18/28 commands. No colour or final guidance
work has started. The next bounded set is the five core maintenance human views.
