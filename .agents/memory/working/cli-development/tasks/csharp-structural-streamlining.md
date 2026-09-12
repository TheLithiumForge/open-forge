---
open-forge:
  description: Assess remaining C# internals and implement justified simplifications beyond the completed strategic command audit
  tags: [Memory, Working, Contextual, CLI, Task, CSharp, Architecture, Refactoring]
---

# Task 27: C# Structural Streamlining

## Current Dogfood Follow-Up

The user reprioritized work after installed-command smoke checks. Follow the
[sequential plan](cli-dogfood-follow-up-plan.md): outdated fixtures, parsing audit,
then Doctor. The later `continue` selects automatic catalogue embedding and a
full presentation pass with Doctor deduplication. Task 30 is qualifying the
restored baseline, including the earlier helper set at `50a36f35`, before its
authorized squash merge. The new Task 27 horizon remains phase 2/3, milestone
1/3 until this prerequisite completes. Fixture corrections and the
[audit report](cli-parsing-doctor-audit.md) are complete. Native delimiter
correction follows, then Doctor correctness and bounded presentation sets.

## Sequential Refactoring Set 1 — 2026-09-12

The user resumed this task with direct sequential execution, one improvement
set at a time, and authorized local squash integration into `develop` after
each verified set. This new horizon supersedes the beta deferral only for the
selected work. Earlier completed horizons and their evidence remain history.

- State: Preserved, integration held by the later user priority. Its previous
  position was phase 3/3, milestone 2/3. Milestones are baseline and freeze,
  implementation with focused verification, and review with local integration.
- Base: `4c115f8e7`, branch `codex/cli-refactor-sequential`.
- Scope: the two optional extension-method follow-ups recorded in the
  [beta closeout](csharp-beta-closeout.md#final-review-observations).
  Remove the unused References result-builder receiver and its constructor
  plumbing. Move Update's single-caller observation lookup into its recovery
  operation as a private static method.
- Profile: direct, behavior-preserving C# refactoring. The CLI handles local
  user files; this set changes only internal call structure. Existing typed
  results, recovery snapshots, ordering, exceptions for remaining inputs,
  containment, leases and effect timing stay fixed. Git makes the code changes
  reversible; it does not protect uncommitted user data. The existing stable
  workspace and cooperating-process boundary remains in force.
- Architecture: keep each helper under its existing command-local owner.
  Ordinary C# static methods are sufficient. No shared promotion, new library,
  parser, serializer, schema, package or exceptional machinery is needed.
- Expected production paths: ReferencesBinding, ReferencesRequestBinder,
  ReferencesWorkspaceResultFactory, Shared/Result/ReferencesBindingResultBuilder,
  and Update/Shared/Recovery/UpdateRecoveryOperation, under the existing Core
  command scopes. Direct test adaptation: remove the unused constructor
  argument at five sites in ReferencesBindingTests. All assertions, test
  identities, inputs and other test files are frozen.
- Evidence: same-worktree offline restore and Release build; unchanged affected
  References/Update Unit and Integration selections before and after; their six
  existing public journeys after. Compare References public failure output
  bytes before and after on the same fixture. Inspect Update's exact lookup
  predicate, first-match selection and recovery inputs for equivalence.
  These local callable changes do not trigger a complete managed/native gate:
  no material shared, public, mutation-safety or build boundary changes.
  Focused Linux Native AOT command evidence is the final compatibility check.
- Budgets: root implements and reviews directly; no delegated review or council.
  One grouped correction if evidence finds a material issue. Stop and rescope
  on changed public behavior, recovery meaning, dependencies or test oracles.
- Protected: Framework/Extension payloads, public contracts/prose, build and
  package configuration, unrelated worktrees and existing historical evidence.
  No remote operations, publication or global installation refresh.
- Next: finish native qualification and local squash integration.

### Managed Qualification And Review

Fixture correction `b07f239e` restores both intended edits and passes 87/87
affected Integration cases. Refactor `50a36f35` passes 146/146 Unit, 87/87
Integration and 6/6 public journeys, all with zero failures or skips. The
Release solution build reports zero warnings and errors. Changed-path whitespace
and warning-level style checks pass. All ten frozen References failure cases
retain identical stdout, stderr and exit status in human and JSON output.

Root inspected the complete seven-path source/test delta. References removes
only an unused receiver, its guards and constructor plumbing. Five Unit
constructor calls adapt mechanically; an exact transformed-source comparison
proves the rest of that test file is unchanged. Update retains the exact
`First` predicate, observation collection, snapshot reference, effect ordering
and call timing while making the lookup private to recovery preparation.
No material finding remains in this reviewed boundary. T27-S1-F1 is fixed.

The canonical managed receipt and source-hash manifest are under
`artifacts/task27-sequential-set1/`; they record exact commands, test assembly
hashes, source/configuration hashes and CTRF execution counts. The initial
baseline failure and pre-refactor native publication remain preserved.
This evidence qualifies the selected set, not every deferred CLI finding.

### Baseline Fixture Correction

Baseline Unit passes 146/146. Baseline Integration passes 86/87 with the same
coalesced Update fixture failure recorded by Task 29. Its obsolete authored-text
replacement makes no change after the Framework wording update, so only the
generated region changes. Before Blue, correct that seed in
`Commands/Update/UpdateIntegrationWorkspace.cs` in the Integration project:
change a stable authored heading and independently verify both authored and
generated edits occurred. Preserve every assertion, test identity and production
file. This separate test-support correction becomes the frozen predecessor
for the pure refactor. Record it as T27-S1-F1; the earlier test-input freeze is
narrowed only for this exact broken fixture.

### Installation And Direct Dogfood

The user required local installation before further work. Offline npm preparation
and offline solution restore succeeded. Sandboxed .NET restore failed with exit
1 and no ordinary diagnostics; the same offline restore succeeded outside that
sandbox. `npm run cli:link` built and staged the Linux x64 Native AOT CLI and
completed all three offline npm links. The native and staged executable share
SHA-256 `115b638722968e0b5f19638b3a3e28a85d7d1c6a2be69428e5750640986d10d0`.
Direct `open-forge --version` now returns
`0.0.0-dev.sha-4c115f8e72626d7419b8b41ea5e9f683ba311b9c`.

The existing user-local PATH symlink initially shadowed npm's new installation
with an old staged package. After verifying both targets, root redirected that
existing symlink to npm's global command. No repository source changed for it.

Smoke evidence is retained under `artifacts/task27-sequential-set1/dogfood/`.
Fresh install, repeat install, Update, Context, Route List and Status complete
without findings. Repository Route List, outgoing References, corrected Find
syntax and Index dry-run complete. These are real installed-command invocations,
not test-suite or cross-platform qualification.

| ID | Observed issue | Disposition |
| --- | --- | --- |
| T27-DOG-01 | Find help shows `--tag Refactoring`, but the command rejects it and requires `--tag=Refactoring`. | Reproduced help/runtime mismatch; correct help against accepted grammar in a separate set. |
| T27-DOG-02 | Repository Context and Route Inspect remain incomplete. Context identifies five August handoffs with unavailable loading metadata. | Existing repository-content issue; preserve sealed historical files. Current scoped loading reduces the older installed CLI's much larger closure-failure set to five. |
| T27-DOG-03 | Doctor reports incomplete route facts and `extension.lifecycle-untrusted` on a fresh successful installation, while Status is complete. | Reproduced diagnostic inconsistency; cause and correction still require focused investigation. |
| T27-DOG-04 | Repository Doctor emits 168,127,363 bytes of JSON with 10,986 local-reference findings. | Output usability concern. The first wrapper capture exceeded 16 MiB and was invalid; direct file capture completed with exit 3 and valid JSON. Do not report the wrapper termination as a CLI crash. |

The installation authorization extends this set's earlier global-installation
protection for the requested refresh only. Remote operations remain unauthorized.

## Current Beta Acceptance — 2026-09-12

Task 27 “C# Structural Streamlining” is complete at phase 4/4, milestone 8/8.
The bounded implementation, evidence/style accounting, extension-method pass,
contract/test review, holistic review and grouped correction recheck are accepted.
All 6,997 C# executions and the Linux installed-package journey pass; the dependent
CI checks also pass. Candidate `31f9340f` retains the C# runtime built at `7f0a9112`.
The [beta closeout](csharp-beta-closeout.md) records every completed, retained,
partial, deferred and paused finding, exact evidence and platform limits.
M8 is complete: feature `1b101bf2` is squash-integrated at `73ef066a`,
with exact tree `43407622824112eb305f0e9903bff0d87dabd7c3`. The closeout record
owns the full integration identities and canonical qualification hash.
All implementation/review agents have finished. Wider refactoring stays deferred;
Task 28 source and all other worktrees remain protected.

The sections below preserve earlier execution boundaries and evidence. Their
historical active/next wording does not reopen work or supersede this status.

## Beta Closeout Preparation — 2026-09-12

The user narrowed the active work to finishing nearly completed slices, proving
a working beta baseline and squashing the accepted result into local develop.
The wider Library/Repair/Cleanup, Route and discovery-command refactorings are
deferred until after the beta. Other unstarted improvements also stay queued;
the comprehensive inventory remains the resumption source, not a claim that
every finding has been implemented. Preserve Task 28's independent source work.

The bounded production batch is complete: phase 3/4, milestone 5/8. M5 closes this
beta batch; M6 accounts for evidence, style and all deferred findings. M7 still
requires the requested extension-method pass before the contract/test analysis,
fresh review and full qualification. M8 owns the local squash and the complete
change/defer record. Reviews distinguish beta correctness corrections from
further structural work, which remains deferred.

The four implementation owners received these stop boundaries, now satisfied:

- Extension: E02 and its separate fifteen-site style correction are accepted at
  `87281d91` and `2d9ca4be`, each with eleven Unit and sixteen Integration
  passes. Finish only the already-frozen E20 ownership Red evidence, then stop
  for root's defect triage. E21 and its production corrections have not begun.
- Library/Repair/Cleanup: L16 Red and correction are accepted at `39426ecd`
  and `ffd1995f`; the redundant internal guard correction is `d6d87b8c`.
  Fifty-two Unit and thirty-eight Integration cases pass. L02 Purple is
  `d20ddee1`; accept the already-qualified L02 Blue and halt. L03's twenty-eight
  path draft was never executed. All later L work is deferred; L10 is retained.
- Route mutations: accept the already-qualified B03a/B03b with the isolated
  four-parameter ComposeChange correction, then halt. P04's useful comparison
  matrix remains an unexecuted draft. All later Route work is deferred.
- Discovery commands: accept the already-qualified Context accumulator and
  projection slice, including its separate Purple. C20 is already complete.
  C02/C04 close in this batch; C01/C09 close only their Context portions. All
  later location, Find, References, Index, Status and Doctor changes are deferred.

Root accepted E20's immutable Red at `04a57c1e`: seven Unit and twenty Integration
cases reached their exact post-mutation failures; twenty-seven ordinary controls
passed. Root read all three test files and independently verified the complete
inputs, raw outcomes and runtime snapshots. This establishes typed caller-input
aliasing, not a public CLI failure or exploit. The sole remaining implementation
lane may finish the accepted eight-path correction in three commits: exact E21
helper promotion, Remove snapshots and Update snapshots. Frozen tests and
comparison order remain protected. All other Extension changes stay deferred.

Root accepted and integrated E20 Red at `19524fc3`, E21's exact helper promotion
at `3ea130bd`, Remove snapshots at `9a27596d` and Update snapshots at `88d4d771`.
All fifty-four frozen cases pass, including the twenty-seven former failures;
the eight-path production correction changes no tests. Root checked the complete
production diff, immutable inputs, raw outcomes and recursive runtime snapshots.
The single pre-existing Remove informational style diagnostic is explicitly
retained. Every implementation lane has stopped with a clean source tree.

The extension-method review `T27-EXT-R1` passed at `c552db4b`: three declarations,
no beta correction and two optional later improvements. Root verified that the
reviewed declarations and consumers are unchanged through `88d4d771`.
`T27-TEST-R1` is now consumed by the single Astra/xhigh contract/evidence review
of that immutable candidate. Optional test simplifications remain deferred;
only material beta gaps or incorrect owned behavior warrant correction.

Final qualification also requires a narrow root-owned CI inventory adjustment:
refresh only `src/cli/ci/test-inventory.json` counts and the corresponding
`.github/workflows/cli.yml` minima from final test discovery. Verify the existing
deferred-theory source identities and case mappings; do not derive expectations
from failures. Other configuration and toolchain inputs stay protected. This is
local validation maintenance, with no remote workflow execution.

Fresh discovery reports 3,206 Unit selections, 1,718 Integration cases and 111
public cases. The nine pre-existing deferred theories expand Unit execution to
3,228 cases. Two referenced source hashes changed only because of previously
accepted namespace imports; root proved all three deferred-source bodies equal
to develop and retained every expected case identity and multiplicity. CI counts
and those two source identities are refreshed in the isolated inventory commit.

Consume `T27-FULL-R1` for one Astra/xhigh fresh review of the final bounded
candidate and its directly affected neighborhood. This complements the separate
test-contract lens and targets material behavior, ownership, integration and
evidence risks. Optional further refactoring remains deferred. The single grouped
correction/recheck `T27-FULL-C1` is consumed by two bounded follow-ups. Root's
local CI preparation found the producer fixture still describing the old suite:
its unchanged seven-case run has five passes and two count-admission failures.
Update only the three count rows in
`src/cli/ci/__tests__/producer-boundaries.integration.test.ts`, preserving every
assertion and independently frozen expected count. Qualify the Node-based CI
boundary in its own commit. The test-contract review also requires three stale
passages in Extension Install's `interface.md` and `behavior.md` to match their
existing workspace-destination permission rules. Make that two-file prose-only
correction separately. These exact paths are the only additional write scope;
Task 28 source and all C# implementation/test inputs remain protected.

The combined C# qualification at `7f0a9112` is complete: managed
3,228/1,718/111, native 1,718/111 and managed-on-native public 111 all pass,
with exact case matching, no failures/skips and clean required formatting.
The Linux native installed-package journey passes with the same input binary.
These follow-ups change neither C# nor its build inputs, so they require only
their dependent CI/document checks and the one grouped recheck. Raw prior
failures and the original source/runtime receipts remain intact.

The user requested one quick read-only code/test exploration for disproportionate
workarounds. Allocate `T27-PROP-E1` to one Astra/high explorer over the current
accepted beta code. It identifies concrete candidates and their actual product
connection, with no implementation or exhaustive new audit. Root triages any
findings against the beta scope. This does not consume the final contract/test
analysis or holistic review.

`T27-PROP-E1` completed at `b29c7621`. Three candidates are deferred: unreachable
Library raw-operand parsing, upstream-parser test matrices and arbitrary
multi-invalid internal guard-order tests. No beta-blocking failure was found.
Keep useful input, containment, recovery and ownership invariants. The final
contract/evidence review receives these findings without a second exploration.

The user deferred F13 after reviewing the unusual YAML examples. No special
syntax handling, additional tests for those spellings or YAML behavior change
belongs to this beta. Any later simplification must consume ordinary library
results and be justified by real product use. The root proportional-development
directive now makes that requirement explicit for implementation and evidence.
U07 remains paused under its existing preflight restriction and is excluded
from this beta batch.
Preserve every immutable commit, failed attempt, draft and untracked file.
No publication, global installation update or remote operation is authorized.

The beta target is the supported x64 platforms. Run local Linux managed and
Native AOT evidence and inspect Windows/macOS x64 configuration and commands;
do not claim foreign-host execution. Preserve the accepted broader platform
inventory. Root will write one tracked closeout inventory containing all
completed, retained, partial, deferred and unresolved findings, exact validation
limits and the final local integration identity.

## Comprehensive Continuation — 2026-09-10

- State: Active; phase 3/4, milestone 4/8. The previous finite horizon below
  remains complete. The user explicitly requested the broader C# duplication,
  streamlining and design-conformance work after reviewing its prior limits.
- Outcome: account for the complete current production C# inventory, inspect
  behavior and actual consumers, resolve demonstrated harmful duplication and
  design/style violations, and give every retained structure an honest reason.
  Include the previously deferred model-placement inventory and informational
  style diagnostics. Inspect tests and support for conformance and duplication;
  preserve their independent assertions and three public journeys per command.
  No promise of mathematical defect freedom or subjective perfection is made.
- Base: local develop `2b54fdc5`, branch
  `codex/csharp-complete-streamlining`. Exact source hashes and six exhaustive,
  disjoint production partitions are frozen in
  `artifacts/task27-complete-preflight/baseline.json`.
- Profile/applicability: a local developer CLI with real filesystem effects,
  leases and recovery. Preserve the accepted stable-workspace/cooperating-process
  boundary, public behavior and ordinary BCL/typed foundations. Exceptional
  machinery: none. No dependency, runtime, platform or threat-model expansion.
- Ownership: root owns cross-partition synthesis, Task/control records, freezes,
  semantic acceptance and Git integration. The user's latest direction enables
  isolated Astra/high implementation lanes; shared/public ownership and
  integration remain serialized. Every C# author/reviewer personally reads the complete current
  C# directive trio and records fingerprints. Luna/max is for exact routine
  execution only. No Python files or helpers are authored.
- Expected paths: C# under `src/cli/core`, `src/cli/root` and mirrored tests.
  Namespace/import adaptations accompany accepted moves without aliases or
  forwarding types. Freeze exact paths, contracts, assertions and the directly
  affected evidence before each coherent slice.
- Protected: `src/open-forge`, `src/extensions`, public prose and contracts,
  root toolchain/package/CI configuration, installed Framework/agent directives,
  global installation, all unrelated dirty/untracked files and Task 28's entire
  source review, reports and user notes. No remote Git/GitHub operation,
  publication, fetch, push, pull, upload or workflow execution is authorized.
- Method: systematic file inventory and read-only semantic assessment, then
  accepted per-finding dispositions and caller boundaries. Deduplication needs
  identical meaning and a nearest shared owner. Similar policy stays local.
  Gray checks freeze callable shapes and behavior; Blue changes production
  against frozen assertions. Pre-frozen mechanical namespace/receiver test
  adaptations may accompany their production move, with a separate path and
  assertion mapping in the receipt; no compatibility scaffolding or broken
  intermediate commit is introduced for that separation. Necessary substantive
  Purple changes are separate commits. A discovered behavior defect requires independent Red and
  a separate fix; never disguise changed behavior as refactoring.
- Evidence: unchanged exact predecessor receipts supply the beginning baseline.
  Run affected Unit/Integration checks per slice, with formatting, source and
  protected-path checks. Run the complete managed/Linux Native AOT suites and
  native package journey once at the final coherent integration boundary.
  Other supported platforms receive static review under the accepted local-only
  delivery boundary. Do not rerun broad gates for state-only changes.
- Budgets: six production partitions, with the 479-file foundations partition
  split into Framework and root-owned Shell/lifecycle assessment for tractable
  full-body coverage. One additional top-down architecture packet is selected
  under the user's explicit follow-up to assess reusable system structure.
  No repeated whole-source discovery;
  one fresh final review `T27-FULL-R1` and one grouped correction/recheck
  `T27-FULL-C1`, both consumed and accepted; council zero. Root may refine review partitions
  if the eventual delta demonstrates a distinct risk, recording the reason.
  The user requested a quick branch sanity check on 2026-09-11: one additional
  bounded review `T27-RESUME-R1` is allocated for representative completed
  refactors at `7013cb29`. It checks design fitness, behavior preservation and
  evidence quality without repeating whole-source discovery or consuming the
  final review. It is consumed with no material findings in the sampled scope;
  the resume receipt below records its limits.
  The user additionally requested an extension-method ownership pass
  `T27-EXT-R1`, consumed by the beta ownership review at `c552db4b`, with one
  Astra/xhigh owner. Remaining E20 edits do not define extension methods; root
  must verify the reviewed declarations and consumers against the final delta.
  Run it after remaining
  structural/style work and before `T27-TEST-R1`. Inventory extension methods
  and modern extension blocks; assess actual receivers/consumers, capability
  ownership, nearest scope, utility leakage, duplication and readable call
  sites. Retain appropriate receiver capabilities; give ordinary utilities
  honest static owners where the evidence supports it. Freeze behavior,
  null/guard/evaluation order, overload and method-group binding, and exact
  receiver/test adaptations before isolated structural corrections. Substantive
  Purple remains separate. This is an explicit additional user-requested lens;
  it does not repeat the whole-source audit or consume the final test review.
  The user additionally requested one final contract/evidence analysis
  `T27-TEST-R1`, consumed and accepted with T27-TEST-D01 fixed and
  T27-TEST-T01 deferred, with one Astra/xhigh owner. It reads all command
  interface/behavior contracts and Integration/public E2E test titles top down,
  then inspects bodies in coherent command groups. Assess missing coverage,
  unnecessary or duplicate evidence, independent assertions, test-tier fit and
  ownership of tested behavior. Inspect implementation where needed to establish
  documentation accuracy; do not mistake tests of third-party or OS internals
  for Open Forge evidence. Preserve exactly three simple public journeys per
  command; put detailed owned scenarios at the appropriate lower tier. This
  analysis follows remaining implementation and precedes final acceptance.
  Group accepted test corrections separately from production fixes, using
  `T27-FULL-C1` for the grouped correction/recheck where applicable.
- Horizon: phase 1 owns M1 complete inventory/coverage and M2 accepted findings;
  phase 2 owns M3 baseline evidence and M4 the initial callable/assertion freeze
  with mandatory per-slice refreezing; phase 3
  owns M5 production slices and M6 evidence/style consolidation; phase 4 owns
  M7 fresh review/full qualification and M8 exact local integration/closeout.
- Accepted scope: the [comprehensive preflight](csharp-comprehensive-preflight.md)
  owns M2 finding dispositions, cross-partition ownership and behavior gates.
  M3 reused the qualified local predecessor before production authoring: all
  1,732 production hashes matched, with unchanged source Framework, package and
  toolchain inputs.
- Now: M1–M4 complete. Ten disjoint coverage reports account for all 1,732
  production files with no gaps, overlaps or changed source hashes, verified in
  `artifacts/task27-complete-preflight/coverage-summary.json`. Framework was
  refined into three capability partitions; root covered 46 Shell/composition
  files and the warm architecture assessor covered 82 Install/Update files.
  The initial behavior/callable/assertion boundary is frozen; each subsequent
  slice must refreeze its exact inputs and mechanical test adaptations.
  U01/U02 received independent Update regression evidence first because the
  accepted contract preserves bytes outside managed root blocks. No production
  changes preceded Red. U01/U02 are corrected in `4eb8b65c` against immutable
  test commit `b42f6f08`, with 65 focused passes. The Framework source lane
  qualified F01 in `8f2a860c` with 67 focused passes and unchanged assertions.
  F08/F14 are committed in `657d8580`, with 54 focused passes. F04 is committed
  in `44ff07b6`, with 191 focused passes. F05's test-only characterization is
  committed in `2a337a0b`, with 151 passes; its shared decoder extraction is
  committed in `ed14a045`, with 166 focused passes and unchanged assertions.
  F02 characterization is committed in `9be2d2c3`, with 80 passes and one
  intentionally weakened implementation rejected. Its Blue ownership change
  is committed in `91069756`, with 108 focused passes. F03 is committed in
  `2793e70c`, with 147 focused passes. F07 is committed in `da09ccd2`, with 38
  focused passes. Its separate 16-case enum-mapping Purple is committed in
  `53562e03`, with 41 focused passes. F15's independent Red is committed in
  `b17ad765`: both intended regressions failed and both construction controls
  passed. Its isolated correction is committed in `fbc2b0d7`, with 192 focused
  passes against frozen tests. F06 characterization is committed in `24147ddd`,
  with 92 focused passes, including 27 new cases. Its production decomposition
  is committed in `423424bf`, with 127 focused passes against immutable tests.
  Its separate four-outcome lexical-stage Purple is committed at `0bb82faf`,
  with 39 Unit passes. F09/F11 mechanical conformance and the narrow S04
  redundant decoder guard correction are committed at `18728aea`, with 317
  focused passes. F12's separate internal fragment-mapping Red is committed
  at `836c4058`, with one intended failure and 26 passing controls. Its isolated
  correction at `37d4227d` passes 50 focused cases. Physical-state mapping
  Purple is committed at `dd2544e3`, with 40 focused passes, including 26 new
  cases. Its four-file explicit-group Blue is committed at `19854437`, with
  91 focused passes against frozen assertions. F10 model placement is committed
  at `db2a5d0e`: 65 preserved types, 503 preserved consumer/implementation bodies
  and 170 focused passes. Remaining mutation preparation is accepted. M13's
  guard fixture Purple is committed at `7d88c773`, with 16 focused passes.
  Recovery model placement is committed at `0ac2b77e`, with 142 focused passes;
  a separate nine-header Markdown cleanup is committed at `88c89efd`.
  M13's strict identity helper and equivalent receivers are committed at
  `76e76196`, with 212 focused passes. Separate Red at `799bb942` demonstrates
  deletion of valid changed finals in all ten omission-bearing adapters, with
  ten unchanged controls passing. The isolated correction at `dc6154b5` passes
  all twenty frozen cases and 124 existing controls, completing M13 and its E07
  overlap. M02 envelope formation and M04's independent nullable basis
  correction are committed at `377e61d2`, with 38 focused passes. M03's
  independent intended-byte ownership/empty-snapshot Purple is committed at
  `01a5ec45`, with 14 focused passes. Its capture/placement Blue at `21a316df`
  passes 26 focused cases and completes M03. M01 link identity and its cohesive
  model placement are committed at `c2c6d3ed`, with 39 focused passes. M07 at
  `0eb0bc08` shares exact receipt values and completes Mutation model placement,
  with 40 focused passes. M14 Lifecycle placement at `9528081f` passes 56
  focused cases and closes the full M14 placement chain. M05 shared snapshot
  path validation at `1755de51` passes 16 focused cases with all tests unchanged.
  M06 UTF-8 failure-output Purple is committed at `4b4cee42`, with 23 focused
  passes and every production file unchanged. Its Blue at `876e7b83` passes
  the same 23 cases with exactly three substitutions and all tests unchanged,
  completing M06. M08 classifier Blue at `2df446b7` passes all 24 focused
  Integration cases with every test unchanged. Its separate direct Purple at
  `9cbcd79b` passes all twelve focused Unit cases, completing M08. M09
  comparison-precedence Purple at `91abaf3d` passes all 27 focused Unit
  cases. Its Blue at `ba3a5d69` passes the same 27 cases with all tests
  unchanged, completing M09. M12 manifest mapping Purple at `d7faff64` passes
  all 52 selected Unit cases. Its Blue at `4eaf2893` passes the same 52 Unit
  and three archive Integration cases, completing M12. M11 lease identity
  reuse at `0e838c83` passes eight Unit and five Integration cases with all
  tests unchanged, completing M11. M15 local availability reuse at `4eccf835`
  passes twelve focused Integration cases with all tests unchanged, closing
  M15. M04 characterization at `a0f0d17b` and validation restructuring at
  `7e4d0a43` pass 29 Unit and 31 Integration cases with all tests frozen,
  completing M04. M10 Red at `2cc70ab4` demonstrated seven intended failures
  with fifteen passing controls; correction `92753288` passes 43 Unit and
  seven Integration cases, completing M10 and the full mutation partition.
  P03 filesystem model placement is committed at `5c5ac7a4`, with 34 Unit
  and twelve Integration passes. Extension and operational P03 placement remains
  queued. P02 local link-target precedence is committed at `4accc8eb`, with
  fourteen Integration passes and all tests unchanged. Initial Update preflight
  candidate U07-C01 is paused after automated rejection of its reproduction
  assignment; no source or test changes occurred. U04 composition is committed
  at `9e365492`, with 33 Integration passes and frozen tests. U03 direct Install
  plan predicates/count at `7e40388d` passes fifteen Integration cases with
  frozen tests. U08 model placement at `672f8120` passes nineteen Unit and
  43 Integration cases with all type bodies and tests preserved. U09 at
  `01189c63` passes thirty Unit and thirty Integration cases after one root
  design correction. U10 next-action Purple at `deb9dc8a` passes 38 Unit cases;
  its Blue at `77d2fa2d` passes 38 Unit and thirty Integration cases with exact
  receiver adaptations and frozen assertions. U05 Purple at `20b58087` passes
  51 Integration cases; Blue at `39fa77dc` passes the same frozen cases. U06
  Purple at `2a8703b2` and Blue at `3748663d` pass the same 66 Unit cases.
  U11's local renderer Purple at `cd53d35c` and Blue at `adb991fb` pass the
  same 36 Unit cases. S03's shared presentation Purple at `602c0071` passes
  54 Unit cases with all original inputs unchanged. Its separate Blue slices
  pass the same 54 cases at escaping Blue `d9553e52` and help Blue `ebadd8ef`,
  completing U11's local/shared scope. Workspace labels at `c8f1e31d` pass the
  same 54 cases. Separate assertion consolidation at `6b6a555b` passes the exact
  52-case remainder, completing S03. E11's three Extension help consumers are
  included in `ebadd8ef` and its literal oracles; its inventory state is also
  reconciled as complete, without another source change or test run.
  Library/Repair/Cleanup preparation is
  accepted, with L10 retained to preserve independent catalogue observations.
  Implementation and build artifacts have one writer per isolated worktree.
  F13's decoded YAML meaning decision is pending:
  see the preflight's concrete diagnostic and grammar boundary. Independent
  accepted work continues while awaiting the user's answer.
  M13's read-only preparation is
  complete and accepted in the preflight; the parallel packet below owns execution.
  Other implementation stays queued.
  Test-source inspection remains focused, not exhaustive.
  Task 28 proceeds independently in the user's other chat.

## Parallel Implementation Continuation — 2026-09-11

The user explicitly requested parallel implementation using Astra/high. Start
two isolated lanes from the same accepted S03 baseline: Shell owns S01/S02 and
their exact consumers; packages owns P10, Extension-only P03/P05, P06/P07/P08.
The package slice's six DTO-move files have no overlap with the frozen Shell
consumer set. Its other changes stay in named Framework owners and focused
tests, with existing callable shapes retained. Freeze actual full path sets
before either writer applies edits; escalate any overlap or shared-contract gap.
P01/P04 and operational P03/P09 stay outside this first parallel wave.

Each Task Mastermind acts directly as its lane's Astra/high implementer, keeps
one source/build writer per worktree and makes coherent local feature commits
after the required freezes and gates. Separate substantive Purple remains
separate from Blue. No additional lane review budget is introduced; root reviews
exact candidate boundaries and retains the final named reviews. Agents may use
Luna/max for exact routine execution if useful. No Sol allocation is used.

Each worktree owns its restore/build/test outputs and executable. Prepare assets
from the existing local package cache with an isolated offline NuGet config;
do not query remote sources. Limit each build to four workers, disable compiler
and MSBuild node reuse, and do not run global build-server shutdown while a peer
is active. Root owns worktree lifecycle, integration and Task/control records.
Integrate accepted lane results serially into the current Task 27 branch;
the final shared baseline receives the required combined evidence. No remote,
publication or global installation authorization changed.

Both lanes are active from `3b8f7f81`. `/root/shell_parallel` owns
`codex/task27-shell-parallel` in `../open-forge-worktree/task27-shell-parallel`;
`/root/packages_parallel` owns `codex/task27-packages-parallel` in
`../open-forge-worktree/task27-packages-parallel`. Both runtime agents are
`default`, assume `task-mastermind.agent.md`, and use `gpt-6-astra` / `high`.
The maximal Shell envelope has 434 paths; packages has fourteen, with zero
intersection. Each owner freezes its actual smaller slice before mutation.
The exact envelope is
`artifacts/task27-complete-preflight/root/parallel-write-envelopes.json`, SHA-256
`7901984a6728593401bcfd277ce1ef5af96efdd7e957139d355087d9c455ce7b`.
Both isolated offline restores passed from the existing local package cache.
Root keeps candidate acceptance, subsequent dependency preparation and tracked
coordination state; owners return immutable qualified feature commits.

Packages P10 is accepted and integrated without conflicts: lane Purple
`57e16e45` and Blue `d707407b` are retained on the Task branch as `83fd252a`
and `bbcdc53f`. Both pass 166 focused Unit cases with exact raw case identities;
the two new literal length-result cases precede the single join-to-original-text
replacement. Root verified all 3,261 candidate source/config inputs, separately
checked the local Git pointer, and checked each slice's 98 artifacts, 26 final
runtime snapshots and 54 predecessor runtime snapshots. The candidate's source
matches the integrated branch exactly; only root-owned Task state differs.
The acceptance receipt is
`artifacts/task27-complete-preflight/root/p10-parallel-acceptance.json`.
Combined qualification and the final named reviews remain queued.

The first envelope addendum is accepted and copied to both owners:
`artifacts/task27-complete-preflight/root/parallel-write-envelope-addendum-01.json`,
SHA-256 `1c2001f51a515c26c053a720dc477c40350fb840809eb1d11efb5813ec4151e5`.
Shell may adapt imports in the two newer U11 renderer test classes omitted by
the earlier lexical consumer inventory. Their full assertion bodies stay fixed.
Packages may add two separate P06 Purple cases to the existing real Library
record-reader Integration class: ambiguous ownership becomes Blocked, while
ordinally malformed input remains Malformed, with exact retained bytes and cause.
Root verified the reader and the prior eight cases; the earlier preparation
overstated that adapter coverage. The new combined envelopes still have no
intersection. Original preparation and failed/corrected compile evidence stay
intact; these are bounded refinements, not changes to product behavior.

The next accepted package slices are integrated: P05 Purple `d61569d0` and
Extension DTO/P05 Blue `5c3caa9b` preserve the observed complete invalid-encoding
Cause and pass eleven Integration, then 54 Unit and thirteen Integration cases.
The four DTO bodies and source-generation attributes/options stay exact.
P03's operational portion remains pending. P06 Purple `983120ab` adds eleven
Unit and two real-reader Integration cases; Blue `ea0ba699` retains the same
95 Unit and ten Integration case identities with all assertions unchanged.
It reuses already admitted typed candidates after the original complete
materialization, eligibility and ordering checks. No early record construction
changes ambiguous-ownership classification.

Shell S01 lane `08c91891` is accepted and integrated at `ab062c77`. Root verified
all 37 declarations in 35 model destinations, twelve retained source-owner
bodies and seven empty removed owners. The two retained-owner filename moves
preserve their bodies. All 364 other unchanged consumer bodies and three exact
qualified-type replacements in two consumers are accounted. The lane's 105
adapted test bodies and exact 74 Unit/94 Integration case multisets are preserved.
Root checked 3,288 committed inputs plus one Git pointer, 526 sealed artifacts
and 234 final/predecessor runtime snapshots per side. The first two builds
exposed one relative qualified name and two newer U11 test consumers; their
bounded corrections and original failure evidence remain intact. Integration
was conflict-free. S02 remains with the same owner.

Detailed root receipts:
`artifacts/task27-complete-preflight/root/extension-decoding-parallel-acceptance.json`
and `artifacts/task27-complete-preflight/root/s01-independent-declaration-proof.json`.
These are focused accepted slices; combined and final qualification remain due.

The first parallel wave is now complete. P07 is integrated at `90827feb` with
fourteen unchanged residual Integration cases; P08 at `f5110f51` retains seventeen
Unit and three Integration cases. Both recovery reads, ZIP/hash/cancellation
boundaries, generated-region bytes and line-ending precedence remain unchanged.
Shell S02 is integrated at `39c0791d`: 68 Unit and 76 Integration cases pass;
the direct tree keeps the original first arguments guard and all ten test
receiver adaptations. Root verified its 513 artifacts, 234 final/predecessor
runtime files per side and all 529 preceding S01 artifacts.

Combined worktree qualification at `39c0791d` passes 320 Unit and 134 Integration
cases across fifteen and fourteen classes respectively, with zero failures,
skips, warnings or formatting changes. All ten integrated patches exactly match
their immutable lane patches, and all 3,298 tracked inputs stayed unchanged
during execution. The root receipt is
`artifacts/task27-complete-preflight/root/first-parallel-wave/qualification.json`;
228 runtime/fixture files are sealed beside it. The initial root selection
script collapsed distinct theory rows sharing a truncated display label; its
preserved failure precedes the corrected multiset comparison. No test or source
correction occurred. Both lane owners returned clean worktrees and ownership.
The canonical inventory now has fifty completed findings and four retained.
Final full managed/Linux AOT/package and named review gates remain due.

## Second Parallel Wave — 2026-09-11

The first wave integrated cleanly and passed its combined focused gate. Continue
with two disjoint Astra/high owners from `c1692e37`: Route R01–R10 and the remaining
package foundations P01, operational P03/P09 and P04. No review budget changes.
The same package owner retains its worktree on the new clean branch
`codex/task27-package-foundations-parallel`; its original branch and all sealed
evidence remain intact. Route uses `codex/task27-route-mutations-parallel` in
`../open-forge-worktree/task27-route-mutations-parallel`. The Shell lane is done.

The frozen maximal envelope is
`artifacts/task27-complete-preflight/root/second-wave-write-envelopes.json`, SHA-256
`129c6e3c0ffe0eeb22890193f6266b072f308f4e0af11cecc12d48f053967c1e`:
37 package paths and 343 existing Route paths have zero overlap. New Route
helpers/models and mirrored evidence stay within the accepted R01–R10 map;
Route List/Inspect/Init, Framework dependencies and Extension examples are
read-only. Package model imports own their Status/Doctor/Repair and standalone
composition consumers. Route owns only its Route composer at that boundary.
Every actual slice is refrozen; additional paths return to root before mutation.

P01 retains different family approvals and all actual reads/writes; its follow-up
shares only the accepted observation comparison and action/recovery projection.
Operational P03/P09 preserve model bodies and producer-owned presence mapping;
the direct mapping and seventh catalogue-reference Purple changes stay separate.
For P04, follow this preflight's corrected ownership oracle: repair the existing
Fact to pass its actual mutable owners array, rather than add a duplicate Fact.
Keep the exact two-stage constructor guards and snapshot semantics.
Route follows its accepted Purple/Blue sequence, equivalence matrix and live
absence evidence; R10 shares the existing buffer algorithm without adding the
deferred one-buffer optimization. Root retains integration and Task/control state.
One writer and four-worker no-reuse build per worktree remain the operating bound.

Both owners started on 2026-09-12: `/root/route_mutations_parallel` and the
continued `/root/packages_parallel`, each runtime `default` assuming
`task-mastermind.agent.md` with `gpt-6-astra` / `high`. Their worktrees were clean
at `c1692e37`; Route's isolated offline restore completed successfully.
Root retains the serial main-worktree writer and takes the independent T04
test-support slice while these lanes run. Its only source paths are
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/TestSupport/TemporaryWorkspaceSafetyTests.cs`
and `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextWorkspace.cs`;
neither intersects the two envelopes. Freeze the existing support receiver's
successful exact bytes and replaced-link refusal in a prior Purple commit,
then change the Context fixture's receiver in a separate Purple commit. The
shared workspace implementation stays fixed. Focused Integration evidence and
the existing Context public journeys qualify the two boundaries; final full
gates remain unchanged. Root serializes its builds and integration, using the
same four-worker limit. No additional review owner or budget is introduced.

T04 is complete: prior receiver oracles at `37191c4e` pass four Integration
cases. Fixture delegation at `39a6939f` passes those four cases and all three
unchanged Context public journeys, also freshly passed before the change.
The only fixture delta is its replacement receiver; all shared support and CLI
implementation remain fixed. Both complete source inventories, literal changes,
recursive runtime snapshots and predecessor artifact hashes are verified in
`artifacts/task27-complete-preflight/test-support/implementation/` under
`t04-replacement-oracles-purple`, `t04-context-prior` and
`t04-context-replacement-purple`.

P01 is complete and integrated through five separate commits: planner evidence
`e5dfa1e2`, private helpers `2f2c6f09`, observation comparison `3248b3f1`, direct
observation evidence `ba666840`, and action/recovery projection `0495a611`.
The first two retain sixty Unit cases; the private Blue additionally passes
twenty-two Integration cases. Observation Blue passes sixty Unit and forty-two
Integration cases; its separate direct Purple adds seven named cases. Final
action/recovery Blue passes all 67 selected Unit and forty-two Integration cases.
Root verified every immutable source, exact transform, raw case multiset and
recursive runtime snapshot. Each integrated patch equals its lane patch.
Approval policy, actual reads, lease checks, application and failure reporting
stay with the command owners. The action switch still precedes recovery
formation. Package scope now continues with operational P03/P09 and P04.

Route's first reader Purple is integrated at `1fb94b99`: four new command-local
cases plus retained controls pass eight Unit and 72 Integration cases. Root
read all four oracles and verified the immutable receipt. One draft fixture
path correction is preserved separately. Its new import/attribute formatting
will receive a separate test-only correction after the already frozen B01;
the accepted extraction keeps both command oracles and all assertions intact.
Combined qualification follows the coherent wave. Main patch identities are
tracked in `artifacts/task27-complete-preflight/root/second-wave-integrated-patches.json`.

Root next takes T07's independently unused Remove fixture facade. The one
two-line `WriteText` member has no caller in the complete current E2E source;
its removal does not depend on T03's later seed relocation. This safely advances
T07 before T03 without changing that seed or the retained cleanup boundaries.
Only `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveWorkspace.cs`
is writable for this slice. Freeze its complete consumers, preserve all other
source and assertions, then build and run the existing three Remove journeys.
No new test, ownership adoption or runtime behavior is introduced.

T07 is complete at `d09eca90`, with its unchanged three Remove public journeys
passed against the freshly built same-worktree development executable. The
exact three-line deletion, all other source bytes and 85 runtime files are
sealed in `test-support/implementation/t07-remove-unused-fixture-purple` below
the comprehensive artifact root. Operational P03/P09 is integrated at
`5b519daf`, preceded by the seven-reference catalogue Purple `f8f66444` and
followed by direct mapping Purple `cc3af2fc`. Root independently compared the
complete catalogue, absence proof and extracted evidence model bodies. Six
Unit and seven Integration cases pass for the move; all eight mapping cases
and three existing Doctor absence controls pass afterward. Package P04 remains
with the same owner.

## Third Independent Owner — 2026-09-12

The completed first wave and the current exact integrations have no conflicts.
The available host has twelve logical processors and sufficient free memory
and disk for another isolated four-worker build. Under the user's explicit
parallel direction, expand the active lane maximum from two to three for the
closed T01/T02/T03/T08 test-support slice. No review budget changes. Root stops
authoring test-support source while this owner is active and retains serial
integration and acceptance.

`/root/test_support_parallel` assumes `task-mastermind.agent.md`, using
`gpt-6-astra` / `high` as its direct author. Its branch is
`codex/task27-test-support-parallel` in
`../open-forge-worktree/task27-test-support-parallel`, based on `cc3af2fc`.
The envelope `artifacts/task27-complete-preflight/root/test-support-parallel-write-envelope.json`
has thirteen existing paths, two accepted helper destinations and no overlap
with either current lane. SHA-256:
`c0eb3ace09f98d1b91a75f08d3628932c3ed8854e9d442e7c8d81ce0f7f405dd`.
Only accepted exact fixture/receiver changes are writable. All public assertions
and journey counts remain fixed. Required build publication selects that same
worktree's development executable; final managed/native/package gates stay
with root. Preparation and restore use the local package cache only.

Run T01/T02/T03/T08 as separate frozen Purple slices. T04/T07 are already
complete. T05/T06 remain outside this lane because their model/fixture consumers
overlap other command work. T08 may precede T05: its direct overload and existing
request semantics are independent of the later namespace move. Preserve the
required environment on the string-path overload and the optional fallback only
on the target overload. T03 retains Move-owned cleanup, paths, wording and
timing exactly. Additional paths or changed shared meaning return to root
before mutation. The owner may not create new E2E journeys, adopt process-created
files, redesign cleanup, alter production, or update Task/public/toolchain files.

## Accepted Second-Wave Progress And Extension Owner — 2026-09-12

The Packages owner completed its ten immutable second-wave commits. P04's
ownership/guard Purple is integrated at `1bd29c01`; cohesive construction Blue
at `993b2b1e` retains the complete validation order, lower-case parameter names,
single owner-list snapshot and existing observation equality. Three Unit and
two Integration cases pass. Root verified the full source trees, exact changes,
recursive runtime snapshots and evidence manifests. P03, P04 and P09 are now
complete; no package-foundation work remains in this lane.

Route R01 is integrated at `e315d3fd`, its separate formatting Purple at
`a5162bf9`, and R02 at `c8aae6f9`. Both extractions pass eight Unit and 72
Integration cases against the prior reader oracles. Root inspected filesystem
read/validation order, complete-only inventory publication, cancellation cause
translation, lexical exposure and the exact leaf/test adaptations. The shared
readers preserve observations at their existing stages. R01's neutral local
interruption has no cause; each leaf supplies its original command text.
Validator causes remain unchanged. The formatting-only successor passes all
55 affected Integration cases. B06 may include only the already accepted R07
unused `BuildDestinationLayout` effects parameter and its exact callers, with
the live effects dictionary retained. All other R07 work keeps its prior order.

T01 is integrated at `75313baf`, sharing the existing rich tree snapshot while
retaining Find's lock/release cadence. T02 at `9aa93352` uses the existing store
owner's admitted path facts; Status/Cleanup snapshots, manifests and disposal
retain their original stages. Each slice passes its six unchanged command
journeys. Root verified both immutable source/evidence trees and same-worktree
published runtime snapshots. No Windows execution or folder-resolution call
count equality is claimed. T03/T08 remain with the continuous test-support owner.

Reassign the warm `/root/packages_parallel` owner to the accepted Extension
command packet, still `task-mastermind.agent.md`, `gpt-6-astra` / `high`, direct
author. The active lane maximum remains three. Root inspected the clean old
worktree and preserved its completed branches and every sealed artifact, then
created `codex/task27-extension-commands-parallel` there from `9aa93352`.
The worktree remains `../open-forge-worktree/task27-packages-parallel`.
The maximum envelope has 248 existing Extension production/focused-test paths
plus accepted new destinations, with no overlap with Route or E2E support:
`artifacts/task27-complete-preflight/root/extension-command-parallel-write-envelope.json`,
SHA-256 `342068bf68d2e6be1f7db5636c226787be6b4c8972ce8a049bff9de3e7d562b4`.
The only root-composition path is `Composition/CliExtensionComposer.cs`.

Implement the corrected Extension preparation in its accepted small sequence,
refreezing current source and exact consumers before every slice. Retain all
twelve private facts; only the two real Install observation consumers justify
their named promotion. E07/E11 are complete and protected. E20 needs admitted
input-mutation Red, then E21's exact existing lifecycle-copy promotion, then
separate ownership fixes. E19's six concrete dependencies remain the bounded
composition exception. E23 preserves its source-bound undefined-state limits;
do not forge provider states or expose private test hooks. E2E, Framework,
public contracts, source payload, toolchain and Task 28 remain protected.
No new review budget or final-gate execution is assigned. Root owns semantic
acceptance, serial integration and final combined qualification.

## Combined Foundations Qualification And Library Owner — 2026-09-12

Root accepted the combined candidate `d75b5a50`: 123 Unit, 215 Integration
and 48 selected public/process cases passed with exact discovered/executed
multisets and zero failures, skips or warnings. The fresh Release solution and
same-worktree development publication passed, as did affected whitespace.
All 3,316 tracked inputs stayed unchanged; 338 recursive runtime snapshots and
362 evidence artifacts are sealed under
`artifacts/task27-complete-preflight/root/second-parallel-foundations/`.
Qualification SHA-256:
`2d6d0eeb440a192588c3cf94703edbae09d98867914af5e66ec90c976a25e356`;
manifest SHA-256:
`a016ac792665947af481f08cf9ce3ff19f09ec619801185610f5a2fbe949cef4`.
This is a focused integration boundary, not final full managed/native acceptance.

This candidate includes T03 at `6c976ceb` and T08 at `d75b5a50`. The complete
four-slice test-support lane passed 45 selected executions and four builds with
no implementation correction. Each immutable patch and qualified source tree
was independently checked before integration. Route B06 at `c6acef20` moves
eight existing internal facts with their exact bodies and no test edits;
39 Unit and 88 Integration cases passed. Its accepted R07 fragment removes only
the unused side-effect-free local argument, retaining the effect dictionary.
R06's later shared edit facts and the remaining R07 deletions remain pending.

Reassign the clean, completed `/root/test_support_parallel` owner to the accepted
Library/Repair/Cleanup packet as `task-mastermind.agent.md`, `gpt-6-astra` /
`high`, direct author. The active maximum stays three. Root verified the clean
worktree at `89051df2`, retained its branch and sealed artifacts, and created
`codex/task27-library-recovery-parallel` from `d75b5a50` in the same
`../open-forge-worktree/task27-test-support-parallel` worktree. Its maximum
385-path envelope and accepted new destinations are recorded in
`artifacts/task27-complete-preflight/root/library-recovery-parallel-write-envelope.json`,
SHA-256 `f60a47f42bd30c95335fd35bbfa7c103cca3428267f9ed4fb2633e19a097e27f`.
No path overlaps the active Extension or Route lanes.

Follow L14 Red/fix, L16 Red/fix and the prepared isolated Purple/Blue sequence.
Retain L10's independent catalogue reads. L06 preserves the exact equality at
each owner, including the one default-record-equality first-match index.
L15 alone owns the named Doctor/Repair projection consumers and the two neutral
`Commands/Shared/LibraryRecovery/` destinations. First replace correlated-payload
null suppression through compiler-proven branches; then move the exact model,
guard and projection. C18's outer JSON removal remains separate and unassigned.
Shell/Framework behavior, all E2E source, public contracts, source payload,
toolchain and Task 28 stay protected. Additional consumers or failed equivalence
return to root. T05/T06 remain queued outside this lane. No new review budget;
root keeps semantic acceptance, serial integration and final combined gates.

## Accepted Command Simplifications — 2026-09-12

R05 is complete at `b03b2c2d`. Nine transparent planning carriers and the
duplicate recovery/preparation carriers are replaced by the existing request,
observation, held-application and pipeline facts. Root inspected the complete
production delta, original catch placement, OperationId provenance and exact
test adaptations. Forty-five paths preserve every existing assertion; 39 Unit
and 88 Integration cases pass. The minor tuple deconstruction uses the existing
ValueTuple fields, with no custom deconstructor or changed traversal order.

R07's prior Purple is integrated at `2e56c71a`: the two dead reference DTO
claims move to actual absence scanning/planning evidence, while existing
navigation assertions remain. A separate live same-line Unicode/CRLF oracle
pins exact full document bytes. Nine Unit and 22 Integration cases pass; no
production change is included. Root read all new cases and their admission,
independent literal outcomes and retained assertions. Dead-code deletion follows.

E01 is complete at `6c00e153`. Create/Inspect/List/Install bindings own their
existing symbol-construction bodies directly. Data remains in Models, including
the newly placed Install binding facts. No test changes were needed. Root
inspected the exact bodies, symbol/option order and parent insertion; 42 Unit
and fifty Integration cases pass. One missing namespace import was caught by
the first build and corrected in a fresh frozen attempt; its failed evidence
is preserved. These newer command slices have individual qualification;
the earlier combined receipt remains bound to `d75b5a50`.

## Fourth Independent Command Owner — 2026-09-12

R07 is complete at `01636875`, following the independently committed live
absence and exact-byte Purple at `2e56c71a`. The obsolete observation path,
unused constructors and dead edit calculation were removed; all 9 Unit and
22 Integration cases passed with frozen assertions. E08's Install application
receipt and shared observation facts are placed at `01a4cd6e`, with 10 Unit
and 21 Integration passes. Its Remove and Update placements remain pending.

The user-authorized parallel wave now adds a fourth independent Astra/high
Task Mastermind, acting as its continuous author for Context, Find, References,
Index and Status. The new clean branch
`codex/task27-discovery-commands-parallel` starts from `01636875`.
The 339-file maximum envelope and copied accepted preparation are recorded in
`artifacts/task27-complete-preflight/root/discovery-command-parallel-write-envelope.json`.
There is no overlap with the three active owners. Every slice still requires
its exact callable, source, assertion and artifact freeze. C20's independently
proved interruption/failure regression and isolated correction come first;
the accepted command-local structural sequence follows. Doctor, C18, shared
recovery, root composition and all Framework/Shell support stay outside this
owner's write boundary. Doctor work waits for the Library owner's L15 boundary.
No additional review unit is allocated. Root continues serialized acceptance
and integration; the final extension-method, test-analysis and holistic passes
remain unused. Existing worktrees and evidence are preserved.

E02's preparation is clarified after inspecting Plan, Foundation, TargetState
and their three actual production consumers: retain the immutable topology and
intended-path objects for admitted typed internal inputs. Preserve existing
explicit guards and order, lifecycle copies, dictionary order/comparer and
member isolation. Do not introduce synthetic null-dereference compatibility
guards for incidental dereferences removed with redundant copies. These
construction sites are trusted typed flow, with no external deserialization
or nullable-oblivious ingress. The E02 receipt will bind that source proof;
no forged-null tests or public admission change is selected.

## Accepted Library Input Correction — 2026-09-12

L14's independent Red is integrated at `2d7d5a9d`: all twelve intended
Attach/Detach/Sync cases failed on the parsed Library subject, while fifty
controls passed, including four Inspect controls. The isolated three-binder
fix at `94c6fb25` passed 74 Integration and 20 Unit cases with the Red tests
unchanged. Workspace-selection failure now uses the existing parsed argument
values; the separate invalid-syntax fallback remains unchanged. Root inspected
all four test bodies and three production diffs and independently verified the
sealed inputs, recursive runtime snapshots and exact failure/pass identities.
The original unqualified exit-code draft and corrected verifier-report display
remain preserved in lane evidence. No production correction was required after
the frozen Green change. L16's separate actual-planner Red is next.

E08's Remove model placement is integrated at `2aa39481`, with 26 Unit and
twelve Integration passes. Type bodies remain exact; one test import and one
equivalent eager byte-array expression are explicitly mapped. The Update
placement is still active. Canonical findings now record 66 completed and four
retained out of 153; remaining findings and final passes are not complete.

## Accepted Model, Construction And Selector Boundaries — 2026-09-12

E05/E08 are complete after Update placement at `05c3c2a7`: seven Unit and
eleven Integration cases passed. The complete sequence retains the twelve
private single-owner facts and deletes only proven unused declarations.
E02's independent input-isolation Purple at `6f641b9b` passed eleven Unit
and sixteen Integration cases against unchanged production. Its exact three
redundant-copy removals qualify separately from fifteen existing informational
style suggestions in the same aggregate file. Root accepted a source-bound
retained-info receipt for that boundary, followed by a separate S04 style-only
Blue against frozen assertions. Preserve array/wrapper semantics, field and
guard order; E21 subsequently moves the then-qualified lifecycle helper
verbatim with original/style/promotion provenance. No style-clean claim applies
to the retained-info boundary.

R08 is complete at `61ca0a03`: eleven Unit and 25 Integration cases passed.
Both existing parser-sharing construction graphs remain unchanged; the 403
lines of absence-scope/planner type bodies moved byte-identically. Create
binding keeps its original validation precedence and exact case values.
R03's prior Purple at `56cc0048` adds independent complete projection bytes
and Unicode/CRLF reference coordinates; 37 Unit and 111 Integration cases
passed against unchanged production. R03 implementation remains active.

C20 is complete at `1a52d95a`, after independent Red `f9b9b088` proved four
intended failures with 23 passing controls. The correction passed 26 Unit and
fourteen Integration cases with all tests frozen. Existing established
selection is retained; only absent selection receives ordered unresolved rows
with truthful Unknown identity, role-local occurrence and unchanged events and
coverage. Root independently verified both immutable candidates and their
source-bound style sidecars. Five Red informational suggestions are retained
for frozen controls/visible typed injection; five old projection suggestions
remain for C08. S04 must resolve or justify them on final source. These are
explicit informational dispositions, with no warning suppression or clean-style
claim. The fourth owner continues the Context accumulator Purple.

L16's Red and correction passed their focused evidence and root's immutable
checks. Root requested one separate correction before acceptance: remove the
new shared effect-order helper's redundant null guard on trusted internal
plans. The Library owner finishes its already-frozen L02 Purple before that
one-line correction; neither commit is amended. L03's eleven preliminary
extra matches were rejected after typed inspection: they are public result-plan
views, whose shape is unchanged. They receive no edits or expanded selection.

## Resume — 2026-09-11

U10 Blue at `77d2fa2d` carries a genuine completed planning stage into Preview
and reuses the existing result formation for next-action policy. Result/execution
representation and equality remain unchanged. Root's one design correction
replaces eleven repeated stopped lifecycle initializers with a narrow two-fact
factory and names adjacent finding strings. Both versions pass 38 Unit and
thirty Integration cases. Exactly fifteen test receiver adaptations preserve
every assertion, fixture and title from Purple. Root verified both transformation
chains, 3,251 final inputs, 3,244 protected inputs, thirteen current runtime files,
51 final artifacts and six clean gates before staging. All 39 original artifacts,
thirteen original runtime snapshots and the Purple evidence remain intact.
One preparation occurrence-count assertion was corrected before source edits;
no build/test failure occurred. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/formations-corrected-blue/`.
U05 selected-input Purple at `20b58087` passes all 51 Integration cases. Exactly
two new tests freeze unselected overwrite observations with independent literal
paths/hash/bytes; all 3,251 original inputs remain unchanged. The first attempt
had two fixture-disposal failures; the corrected tests retain every behavior
assertion and use existing ownership cleanup. Root verified 3,253 final inputs,
seven runtime files, 41 final artifacts and exact raw discovery/execution cases.
Evidence: `artifacts/task27-complete-preflight/install-update/implementation/selected-input-corrected-purple/`.
U05 Blue at `39fa77dc` shares the pure payload projection and reuses parsed
Markdown facts within each planning stage. Metadata remains eager; region and
Loader parsing retain their deferred enumeration boundary. Root reviewed the
complete diff and existing uniqueness/materialization guards, then verified
3,254 final inputs, 3,251 protected inputs, seven runtime files, 44 artifacts
and the same 51 passing raw case identities. All tests remain byte-identical.
Only pre-build spacing changed; no failed build/test or design correction.
Evidence: `artifacts/task27-complete-preflight/install-update/implementation/payload-projection-blue/`.
U06 validation Purple at `2a8703b2` retains 37 cases and adds 29 cases across
two existing Unit files. All 66 pass; production and 3,252 protected inputs are
unchanged. Root verified the exact source transformations, raw discovery/execution
multiset, six runtime files, 48 artifacts and preserved predecessor snapshots.
Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/value-syntax-purple/`.
U06 Blue at `3748663d` removes eleven duplicated predicates and shares only
non-null relative-path and SHA syntax. Caller whitespace admission, guard order
and exact errors remain local. All tests are byte-identical and the same 66 cases
pass. Root read the full diff/helper and verified 3,255 final inputs, 3,251
protected inputs, six current and six copied runtime files, 41 artifacts and
the source transformation/creation. No correction or failed gate occurred.
Evidence: `artifacts/task27-complete-preflight/install-update/implementation/value-syntax-blue/`.
U11 local renderer Purple at `cd53d35c` adds four complete literal-output facts
in two mirrored Unit classes. Both original and corrected qualification pass
the same 36-case multiset. The sole post-build correction uses two collection
expressions and removes their unused import; all values, assertions and titles
remain fixed. Root verified 3,257 final inputs, all 3,255 original inputs, six
current runtime files, both preceding six-file runtime snapshots, 37 corrected
artifacts and 82 preserved artifacts. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/local-rendering-purple/corrected/`.
U11 local Blue at `adb991fb` consolidates five fixed renderer blocks with exact
LF-only replacement and retains command-private immutable status orders. All
36 Purple cases pass with every test byte unchanged. Root reviewed the complete
five-file diff and verified 3,257 final inputs, 3,252 protected inputs, six current
runtime files and six predecessor copies, 41 artifacts, 120 preserved artifacts
and exact transformations. No failed gate or correction occurred. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/local-rendering-blue/`.
S03 Purple at `602c0071` adds three shared-presentation Unit classes and passes
54 cases, including 28 new cases. Root read all three complete files and verified
3,260 final inputs, all 3,257 originals, 43 artifacts, six current runtime files,
six copied predecessors and 162 preserved artifacts. The raw discovery/execution
multiset matches exactly. A preparation assertion exposed three Route helpers'
actual Notes placement before source edits; complete literal Notes bodies now
preserve that existing layout. No build/test gate failed. Evidence:
`artifacts/task27-complete-preflight/shell/implementation/shared-presentation-purple/`.
Exactly 25 future receiver substitutions are frozen, with assertions unchanged.
Escaping Blue at `d9553e52` promotes the byte-identical helper body, updates six
production consumers and preserves the exact 23 test receiver substitutions.
The same 54 Purple cases pass. Root verified 3,258 final inputs, 3,248 protected
originals, 45 artifacts, 206 preserved artifacts and six current/copied runtimes.
Nine exact transformations, three removed helpers and one new path are accounted;
no correction or failed gate occurred. Evidence:
`artifacts/task27-complete-preflight/shell/implementation/escaping-blue/`.
Help Blue at `ebadd8ef` removes twelve private helper pairs and introduces one
Shell presentation owner. Every test byte and all 54 Purple case identities
remain fixed. Root verified 3,259 final inputs, 3,246 protected originals,
65 artifacts, 252 preserved artifacts and six current/copied runtimes. One
pre-edit inventory filename correction was retained; no build/test gate failed.
Evidence: `artifacts/task27-complete-preflight/shell/implementation/help-blue/`.
Workspace label Blue at `c8f1e31d` promotes the existing narrow enum mapping and
replaces seventeen matching callers, preserving outer null/DTO/error behavior.
All 54 Purple identities pass; one test file has only its two approved receiver
changes and import. Root verified nineteen transformations, one new path,
3,260 final inputs, 3,240 protected originals, 55 artifacts, 318 preserved
artifacts and six current/copied runtimes. No correction or failed gate occurred.
Evidence: `artifacts/task27-complete-preflight/shell/implementation/workspace-wire-blue/`.
Escaping assertion consolidation at `6b6a555b` preserves all production bytes
and all 21 shared cases. Its 52 passing identities equal the exact predecessor
minus two obsolete direct-helper cases; eight removed assertions map to four
retained literal vectors. Root verified all three exact prepared transformations,
3,260 final inputs, 3,257 protected originals, 41 artifacts, 374 preserved
artifacts and six current/copied runtimes. One pre-edit JSON-log inspection
correction was retained; no build/test gate failed. Evidence:
`artifacts/task27-complete-preflight/shell/implementation/escaping-consolidation-purple/`.
U11 and S03 are completed with focused qualification. U07 is paused.
Route mutation, Route discovery, discovery-command and test-support preparation
are accepted. Their exact dispositions and verified dependency inventories are
linked by the comprehensive preflight. Root's bounded Shell/model/presentation
preparation is also linked there. Preparation does not consume the final test
review or qualify execution.

U09 at `01189c63` replaces chained decisions and two nullable suppressions in
three Update files. Root's one design correction kept target/intended-state
classification local instead of forwarding whole targets to single-property
helpers. Both the initial and corrected versions pass thirty Unit and thirty
Integration cases. All tests remain frozen. Root independently reconstructed
both the final source transforms and initial-to-corrected chain, then verified
3,250 current inputs, 3,247 protected inputs, thirteen runtime files, 48 corrected
artifacts and six clean gates before staging. The original 32 artifacts and
thirteen copied runtime files remain intact. Root's initial verification applied
the correction twice; the corrected verifier passed before any staging, with
no source changes. Evidence and exact ownership accounting:
`artifacts/task27-complete-preflight/install-update/implementation/ordered-decisions-corrected-blue/`.
U10 Purple at `deb9dc8a` pins six exact default reasons and eight option/precedence
cases against the old callable. All 38 independently discovered Unit cases pass.
Root verified all 3,250 inputs, 3,249 protected inputs, six runtime files, 29
evidence artifacts and four clean gates. Fifteen later test receiver adaptations
are frozen independently; all assertion and fixture bytes remain fixed for Blue.
Evidence: `artifacts/task27-complete-preflight/install-update/implementation/next-action-purple/`.

Library/Repair/Cleanup preparation is accepted after root verified all 400
source/test dependencies and 32 artifacts and read the complete decision packet.
L10 retains per-resolution catalogue observations; L06 preserves default record
equality at the existing IndexOf site; L12 retains the private preparation fact;
L15 uses the nearest real Doctor/Repair shared scope with compiler-proven nullable
flow. The comprehensive preflight owns the exact dispositions. No Library
production change or runtime qualification is claimed by preparation.

U08 at `672f8120` relocates 25 Install/Update declarations through one complete
file move and seven trailing extractions. Seven retained implementation bodies
are unchanged. Root read every destination and independently reconstructed all
fifteen destination/owner bodies from Git before staging. All 3,250 current
inputs, 3,235 protected inputs, thirteen runtime files, 39 evidence artifacts,
six clean gates and the exact nineteen Unit/43 Integration cases were verified.
Eight new destinations and one removed source are accounted; no test or
qualified-reference changes were needed. Release build passed on its first run
with zero warnings/errors. Pre-edit inventory/receipt-path corrections remain
in the packet; production and test gates needed no correction. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/models-blue/`.

Root accepted the corrected Extension preparation after one grouped visibility
correction. Twelve single-owner private facts stay nested; the two identical
Install observations have actual shared consumers. The comprehensive preflight
owns the dispositions and links the preserved predecessor and exact inventory.

U03 at `7e40388d` derives Install's predicates and count from the existing
target/lifecycle facts, leaving both materialized-list bodies and their actual
consumers untouched. Three exact source transformations preserve target-first
recovery, absent lifecycle, directory-only no-op and uncached list semantics.
Root verified 3,243 current inputs, all 3,240 protected inputs, seven runtime
and 27 artifact hashes, four successful gates and the exact fifteen-case
Integration multiset before staging. Release build and scoped whitespace passed
without warnings, skips or corrections. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/plan-predicates-blue/`.

U04 at `9e365492` removes two transparent composition wrappers while retaining
the original dependency instances and revalidation call position. Four exact
body transformations and one file removal are accounted. Root verified 3,243
current inputs, all 3,239 protected inputs, seven runtime and 27 artifact hashes,
four successful gates and the exact 33-case Integration multiset before staging.
Solution Release build and scoped whitespace passed with no warnings or skips.
The initial ignored preparation-script syntax correction is retained; production
and test gates needed no correction. Evidence:
`artifacts/task27-complete-preflight/install-update/implementation/wrappers-blue/`.

P02 at `4accc8eb` preserves each observer's distinct ingress and the original
absolute, unsupported, relative precedence. Root inspected both local changes
and independently verified 3,244 current inputs, all 3,242 protected inputs,
the exact transformations, seven runtime hashes, 24 artifact hashes, four clean
gates and the complete fourteen-case discovery/execution multiset before staging.
Solution Release build and scoped whitespace passed without warnings or skips.
Evidence: `artifacts/task27-complete-preflight/framework-packages/implementation/link-target-precedence-blue/`.

The Install/Update preparation packet is accepted with the preflight's explicit
U10 completion-stage disposition. U07-C01 remains source-only until a real
trusted-installation regression proves the reported dry-run/application mismatch.
The reproduction assignment was rejected automatically for possible
cybersecurity risk before any source/test changes. Do not retry or route around
that rejection. U07-C01 remains unverified and paused; no production fix or
reader extraction proceeds on its basis. Unrelated accepted Install/Update
refactors continue with one source/build writer.

The user reaffirmed modern, idiomatic C#/.NET, cohesive design and easy review.
Use current syntax where it preserves ownership, lifetime, validation, overload
resolution, collection semantics and Native AOT. Continue isolated Blue/Purple
commits, with independent Red for behavior fixes. Root reread the complete
current C# directive trio; fingerprints are unchanged. Live `7013cb29` is clean
and no build/test process remains. The prior M04 owner is unavailable; its
ignored `lifecycle-facts-purple` draft and preparation are preserved for the
replacement writer. No M04 production or test edit was applied before interruption.

## Resumed Review And Model Direction — 2026-09-11

The user now permits Astra/xhigh where warranted and explicitly selects it for
final test analysis. Routine implementation may remain Astra/high. Root's
runtime identity is not independently reported by the session.

`T27-RESUME-R1` completed against immutable `7013cb29`, tree
`49534c91857590c513d573162418a024151e8e18`. The Astra/high reviewer personally
read the current complete C# directive trio and found no material actionable
finding in representative M13 identity/cleanup, M03 byte ownership, M06 UTF-8,
M08 classification, M09 precedence, M12 wire mappings, M11 leases, M15 local
assessment, earlier Update preservation and Markdown ownership changes.
This was a bounded semantic/source-and-test review with no execution or edits;
it does not substitute for final complete qualification or exhaustive review.
The final review and test-analysis budgets remain unused.

## Comprehensive Execution Receipts

- `5c5ac7a4` placed ten filesystem model types in their nearest Models
  scopes, retaining reader implementations separately and mirroring the direct
  classifier tests. The complete type bodies, 218 final source/consumer bodies
  and 71 test/support bodies remain exact after the declared namespace/import
  and seven qualified-reference substitutions. Root independently reconstructed
  every final body from frozen Git objects, verified 3,244 inputs, thirteen
  runtime hashes, 32 receipt hashes, six gate logs and exact case multisets.
  All 3,026 protected inputs are unchanged; six new destinations and four old
  paths are accounted for. Whole-solution Release compilation and whitespace
  passed without warnings, followed by 34 Unit and twelve Integration passes.
  P03 filesystem placement is complete; its Extension and operational groups
  remain pending. Evidence:
  `artifacts/task27-complete-preflight/framework-packages/implementation/filesystem-models-blue/`.

- `2cc70ab4` independently froze recovery classification admission and
  guard precedence: seven intended failures, fifteen passing controls and no
  skips/unexpected failures. Its isolated correction at `92753288` restricts
  the two Classified factories to their accepted finite state sets and validates
  candidate kind after integrity, before cause/path evaluation. All 43 Unit and
  seven Integration cases pass, including the unchanged 22-case Red selection.
  Root inspected the full new test class and two-file correction, reconstructed
  three exact source edits, and verified 3,242 input hashes, ten runtime hashes,
  seven correction gate logs and exact case reconciliation. All tests remained
  frozen through the fix. M10 completes the mutation partition. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/recovery-classification-red/`
  and `recovery-classification-green/`.

- `7e4d0a43` carries nine lifecycle read facts through private construction
  and state validation once. Seven factory surfaces, thirteen helper bodies,
  validation precedence, exception parameters/messages, references and the nine
  final result properties remain intact; the result retains no packet field.
  All 29 Unit and 31 Integration cases passed, with clean builds/whitespace and
  frozen assertions. Root inspected both full deltas and reconstructed nineteen
  exact transforms. Final verification covers 3,241 input hashes, ten runtime
  hashes, seven gate logs and exact case multisets. A supplementary verifier
  initially assumed a flat runtime list; misplaced shell fail-fast allowed the
  source commit before that verification completed. The corrected full check
  subsequently passed over the identical committed inputs; no source or test
  correction or history rewrite followed. The exact order is retained in
  `artifacts/task27-complete-preflight/framework-mutation/implementation/lifecycle-facts-blue/root-acceptance.json`.
  M04 is complete; final broad acceptance remains pending.

- `a0f0d17b` froze lifecycle read validation order and retained facts. Five
  existing factory-call lambdas remain identical and now assert exact owned
  exception messages and parameter names; thirteen additional cases cover
  ingress precedence, selected facts, cancellation and admitted cause bounds.
  All 29 selected Unit cases passed with clean build and scoped whitespace.
  Root reconstructed both source transformations and verified 3,241 input
  hashes, five runtime hashes, four gate logs and exact case multisets. All
  3,240 protected inputs remained unchanged. The existing 205-column attribute
  header remains unchanged; a supplementary receipt check was corrected to
  distinguish that retained guideline exception from newly overlong lines.
  Original draft and failed receipt diagnostic are preserved. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/lifecycle-facts-purple-resumed/`.

- `4eccf835` reused each operational reader's own source-availability result.
  Doctor retains Empty-first managed-set logic and target-state ordering; Status
  retains its distinct cancellation, blocking and lifecycle trust rules. No
  observation or policy was promoted between them. All twelve selected
  Integration cases passed with clean build and scoped whitespace. Root read
  both complete deltas, reconstructed eleven exact operations, and verified
  3,241 hashes, four logs and case reconciliation. All tests stayed unchanged.
  M15 is complete. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/lifecycle-availability-blue/`.

- `0e838c83` cached the lock request workspace identity after all existing
  constructor validation. Live handle state still leads each query; candidate
  identity/path comparisons and disposal remain unchanged. All eight Unit and
  five Integration cases passed, including synchronous disposal and asynchronous
  disposal/reacquisition controls. Root inspected all three edits, independently
  reconstructed the source, and verified 3,241 hashes, seven logs and exact case
  reconciliation. Every test stayed unchanged. M11 is complete.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/lease-key-blue/`.

- `4eaf2893` gave recovery manifest kind tokens one immutable typed map and
  an ordinal reverse map. The original unsupported-kind exception and null
  rejection/default behavior remain unchanged, as do serialization options,
  context, state decoding and call positions. All 52 frozen Unit cases and
  three archive Integration cases passed. Root inspected the complete diff,
  reconstructed four exact source transformations and verified all 3,241 input
  hashes, seven logs and exact case reconciliation. Every test stayed unchanged.
  M12 is complete. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/manifest-kinds-blue/`.

- `d7faff64` froze M12 manifest mappings with six real-target roundtrips and
  four altered kind values: null, missing, unknown and wrong case. Literal wire
  names accompany exact typed identity and payload checks. Malformed cases first
  prove valid original input and one changed JSON field. All 52 focused Unit
  cases passed with clean build and scoped whitespace. Root inspected the full
  addition and target/provenance formation, reconstructed four additive
  operations, and verified 3,241 hashes, four logs and exact case reconciliation.
  Every original test body/helper and all production stayed unchanged.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/manifest-kinds-purple/`.

- `ba3a5d69` replaced the nested recovery comparison conditional with ordered
  Prior, Intended and Third returns at a local helper. The receiving comparison
  entry is passed directly; all earlier branches and result members remain
  unchanged. All 27 focused Unit cases passed against frozen Purple. Root read
  the full delta, reconstructed its three exact operations, and verified all
  3,241 current hashes, four logs and case reconciliation. M09 is complete.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/prior-precedence-blue/`.

- `91abaf3d` added one M09 characterization fact using a valid ordinary-replace
  entry with separately created equal prior/intended identities. It freezes
  Prior precedence, retained input, exact observed identity and null cause.
  All 27 focused Unit cases passed with clean build and scoped whitespace.
  Root inspected the full addition, reconstructed every original test byte by
  removing only that block, and verified 3,241 hashes, four logs and exact case
  reconciliation. All production stayed unchanged. Separate Blue is next.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/prior-precedence-purple/`.

- `9cbcd79b` added M08 direct classifier contracts in one Unit class. Eight
  supported exception instances and two unsupported cases preserve classifications,
  parameter names, actual types and the original literal failure message. All
  twelve selected Unit cases passed. Root inspected the whole new class and
  verified 3,241 final hashes, four final/separate three preparation logs and
  exact case reconciliation. All 3,240 prior inputs stayed identical. The
  packet assumption that InvalidDataException maps to InputOutput was corrected:
  the existing classifier rejects it. Temporary inheritance assertions remain
  only in ignored preparation evidence; permanent tests cover owned behavior.
  M08 is complete. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/filesystem-classification-purple/`.

- `2df446b7` shared the exact filesystem exception classifier across file change,
  directory creation and directory deletion. Nine source files remove three
  duplicate mappings and two newly unused imports while preserving ten helper
  call positions, the inline receiver, all catch filters and failure formatting.
  All 24 selected Integration cases passed; build and scoped whitespace passed.
  Root inspected the complete diff, independently reconstructed all nine source
  transformations and verified 3,240 current hashes, four logs and exact case
  reconciliation. Every test stayed unchanged. M08 direct Purple remains next.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/filesystem-classification-blue/`.

- `876e7b83` replaced three discarded UTF-8 decoding calls with character-count
  validation at the same spans and positions. All 8 Unit and 15 Integration
  cases passed against the immutable Purple, including every full-message,
  state and byte oracle. Both builds, scoped whitespace and exact selection
  gates passed. Root read the entire three-line delta and verified 3,240
  current hashes, seven logs and case reconciliation. All 693 existing test
  files, headers, catches, encoding fields and JSON processing remain unchanged.
  M06 is complete; no measured speed claim is made. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/utf8-admission-blue/`.

- `4b4cee42` froze M06 UTF-8 admission behavior in two new test classes. Five
  malformed byte vectors, a valid non-ASCII document and a long duplicate-JSON
  property control exercise the actual store, document and ownership readers.
  Independent full literals retain decoder byte/index reporting and Store's
  256-character cap versus the 370-character full JSON cause. Culture is restored
  and owned file bytes/disposal are checked. All 8 Unit and 15 Integration cases
  passed, including fourteen new cases. Root inspected both complete classes
  and corrected attribute grouping/direct string escaping before final evidence.
  All 3,238 original inputs stayed identical; root verified 3,240 final hashes,
  eight final and eleven separate setup logs, and exact case reconciliation.
  Probe captures are characterization setup, not regression failures; their
  fixture/diagnostic corrections remain documented. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/utf8-admission-purple/`.

- `1755de51` shared lifecycle snapshot path validation at its existing validator
  owner. Both path bodies and all three prior comparison methods were identical;
  the owner comparison stays unchanged. Three snapshot call sites and the result
  caller's nullable-file early return remain in place. All sixteen selected
  Unit cases passed with clean build and scoped whitespace. Root inspected the
  full three-file diff and verified all 3,238 hashes, four logs and exact case
  reconciliation. All 3,235 other inputs, including every test, stayed identical.
  M05 is complete. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/snapshot-path-blue/`.

- `9528081f` completed M14 Lifecycle placement: 37 moved production types,
  four complete source splits, and one snapshot-test namespace move. Root
  independently reconstructed the original split sources from exact attributed
  type blocks, verified every one of 129 output bodies and 17 new headers, and
  confirmed all eight schema attributes and the source-generation context body
  unchanged. All 18 Unit and 38 Integration cases passed; both builds and three
  scoped whitespace gates passed without corrections. Root verified 3,238
  current hashes, nine logs and exact case reconciliation; 3,109 other inputs
  stayed identical. This closes M14 with its earlier Recovery/Mutation commits.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/lifecycle-model-blue/`.

- `0eb0bc08` shared the exact three receipt-state facts used by ordinary file
  and directory-creation receipts, retaining their original fields, equality
  and validation order. Five sources moved into Directories/Receipts and one
  shared value was added. All 19 Unit and 21 Integration cases passed with
  frozen assertions; builds and scoped whitespace passed on the first attempt.
  Root verified 3,233 current hashes, nine logs, exact case reconciliation,
  all 143 existing body transformations and the complete new value. The other
  3,089 inputs stayed identical. M07 is complete; M14 Mutation placement is
  complete, with Lifecycle still pending. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/receipt-state-blue/`.

- `c2c6d3ed` placed exact link-observation matching on the existing typed state,
  replacing six calls and three duplicate private helpers. The three link model
  files moved together; no state fields or local validation/effect policies
  changed. All 16 Unit and 23 Integration cases passed with frozen assertions,
  clean builds and scoped whitespace. Root verified 3,232 current hashes, all
  nine gate logs, exact case reconciliation and all 46 body transformations.
  The other 3,186 inputs stayed identical. M01 is complete; M14 link placement
  is complete, with the remaining groups pending. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/relative-link-blue/`.

- `21a316df` removed two transient byte-array copies and grouped the four file
  model sources under Files. All 26 selected Unit cases passed against frozen
  assertions. Unit/Integration builds and three scoped whitespace gates passed;
  the first failed build and three corrected shortened namespace references
  remain in the evidence. Root verified 3,232 current hashes, eight retained
  logs, exact case reconciliation and all 299 body transformations. The other
  2,933 inputs stayed byte-identical. M03 is complete; M14 Files placement is
  complete, with its other domains still pending. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/owned-byte-blue/`.

- `01a5ec45` added four independent intended-byte ownership and empty-snapshot
  facts before M03 production work. All fourteen discovered Unit cases passed,
  with clean build and scoped whitespace. Root verified all 3,232 input hashes,
  four gate logs and exact case reconciliation. Removing only the additive block
  recovers the entire prior test file; all 3,231 other inputs stayed identical.
  Evidence: `artifacts/task27-complete-preflight/framework-mutation/implementation/owned-byte-purple/`.

- `377e61d2` consolidated the two identical lifecycle envelope projections and
  annotated the existing non-null successful basis result. The same five
  assignments and original argument evaluation order remain; only two redundant
  null checks were removed. All 15 Unit and 23 Integration cases passed, with
  exact discovery/execution reconciliation, clean builds and scoped whitespace.
  Root verified all 3,232 current hashes and seven gate logs. Exactly two source
  files changed; all tests and 3,230 other inputs stayed byte-identical. M02 is
  complete; M04's broader validation packet remains separate and pending.
  `artifacts/task27-complete-preflight/framework-mutation/implementation/lifecycle-envelope-blue/`
  owns the literal proof and final evidence.
- `dc6154b5` corrected M13's ten admission-qualified identity omissions by
  delegating to the existing strict comparison. All 144 discovered Integration
  cases passed, including every full assertion in the twenty frozen Red/control
  rows and 124 existing controls. Final build and scoped whitespace passed
  without warnings, failures or skips. Root verified all 3,232 current hashes,
  four gate logs, 26 exact class selections and the ten bounded source deltas.
  The other 3,222 files, including every test and the strict helper, stayed
  byte-identical. Only matcher receivers, removed duplicate bodies and necessary
  imports changed; command-local guards, catches, ordering and result policies
  remain intact. This closes M13 and E07 using the earlier strict Blue and this
  separately frozen Red/fix chain. Evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/m13-omission-green/`.
- `799bb942` froze M13's ten command-local regression/control pairs and one
  shared closed-archive fixture. All twenty cases were discovered and executed:
  ten unchanged controls passed; all ten changed cases first proved valid
  readback and then directly observed their archive incorrectly deleted.
  Subsequent exact retention/outcome assertions remain frozen for Green.
  Final compilation and whitespace passed without warnings or skips. Root
  verified 3,232 current hashes, seven retained gate logs and all ten selected
  adapter pairs. All 3,221 original inputs stayed byte-identical. The initial
  run's two Extension snapshot mismatches were corrected by excluding only the
  exact tested archive from collateral comparisons; its independent byte and
  existence oracles remain. The full failed attempt is preserved. Exact evidence:
  `artifacts/task27-complete-preflight/framework-mutation/implementation/m13-omission-red/`.
- `76e76196` centralized M13's exact seven-fact recovery identity comparison
  and migrated only equivalent receivers. Preparation retains its immutable
  authenticated facts; the opaque token authority and existing matching bodies
  remain unchanged. FormatV1 moved unchanged beside the helper, including removal
  of seven imports of the now-empty former namespace. All 104 Unit and 108
  Integration cases passed; final builds and scoped whitespace were clean.
  Root verified 3,221 current inputs, 47 delta paths, all thirteen retained gate
  logs and 45 body proofs. Twenty-two test bodies have only mechanical import
  adaptations; 663 other tests, 787 protected inputs and all ten weaker cleanup
  adapters remain unchanged. The packet in
  `artifacts/task27-complete-preflight/framework-mutation/implementation/m13-identity-blue/`
  preserves exact ownership, selection, header and representation limits.
  M13's changed-artifact regressions and isolated correction are next; this
  receipt does not claim the pending omissions are fixed or final full gates ran.
- `88c89efd` removed nine redundant enclosing-namespace imports introduced by
  F10. Every namespace/declaration body and test remains byte-identical; Core
  compilation and scoped whitespace passed with zero warnings/errors. Exactly
  nine headers changed among 3,220 refrozen inputs. Earlier empty IDE0005
  receipts prove tool execution, not semantic import cleanliness. This concrete
  follow-up leaves their body/assertion/runtime evidence valid. The receipt is in
  `artifacts/task27-complete-preflight/framework/implementation/enclosing-import-blue/`.
- `0ac2b77e` completed M14's Recovery model/storage portion: 42 types, 19 new
  paths, 13 removals and 277 consumer bodies preserved. All 104 Unit and 38
  Integration cases passed, with final builds and whitespace gates clean.
  Known-unused-import probes exposed ineffective IDE0005 checks; complete
  manual review of all 19 new headers removed seven move-created imports.
  Root verified 3,220 current files, ten final gate logs and exact body proofs.
  Theory display-name collisions are reconciled as multisets with their actual
  multiplicity, preserving both runner/discovery IDs without invented pairing.
  The packet in
  `artifacts/task27-complete-preflight/framework-mutation/implementation/recovery-model-blue/`
  owns exact scope and limitations. Lifecycle/Mutation placement and the
  later FormatV1 move remain pending parts of M14.
- `7d88c773` corrected the existing after-selection recovery attribution
  fixture without production changes. It retains manifest order, independently
  verifies payload bytes and valid changed-identity admission, then requires
  deletion refusal to preserve exact archive and target bytes. All sixteen
  discovered Integration facts passed, with build, whitespace and IDE0005 gates
  clean. Root verified all 3,214 frozen files and five sequential gate receipts.
  Discovery and runner IDs differ; the final reconciliation uses exact
  class/method and equal display names, preserving both IDs without rerunning.
  The original fixture weakness remains a source diagnosis, not a reproduced
  baseline claim. Full evidence is in
  `artifacts/task27-complete-preflight/framework-mutation/implementation/m13-guard-purple/`.
- `db2a5d0e` completed F10's 26 model-placement entries. The change preserved
  all 65 moved type bodies, 503 consumer/implementation bodies and every test
  assertion while updating all 20 qualified references. All 138 Unit and
  32 Integration cases passed; three builds, three scoped whitespace gates and
  three scoped IDE0005 information-level gates passed without warnings/errors
  or import corrections. Root verified 3,158 current paths, all 19 planned
  removals and thirteen sequential receipts. Of 685 test files, 178 have only
  frozen reference adaptations and 507 are byte-identical; all 53 protections
  stayed unchanged. Exact mappings, hashes and body proofs are retained in
  `artifacts/task27-complete-preflight/framework/implementation/model-placement-blue/`.
- `19854437` made six physical-state switches explicit in their four existing
  owners, retaining every named/dormant outcome and the original callable,
  diagnostic, guard, catch and failure-fallback behavior. All 40 Unit and
  51 Integration cases passed with no warnings, failures or skips. Both builds
  and scoped formatting passed. Root verified all 3,151 current hashes and
  seven sequential gate receipts; all 685 test files and 53 protected files
  remained frozen. The exact diff and producer-admission/evidence limits are in
  `artifacts/task27-complete-preflight/framework/implementation/physical-mapping-blue/`.
  This closes F12's physical portion separately from its fragment correction.
- `dd2544e3` froze physical source mappings in two new Unit classes and one
  additive inventory-construction fact. All 40 discovered cases passed with
  zero warnings, failures or skips; build and scoped formatting passed.
  Root verified 3,151 file hashes, four gate-log hashes and exact preservation
  of the prior inventory assertions. The packet in
  `artifacts/task27-complete-preflight/framework/implementation/physical-mapping-purple/`
  retains factory admission and sealed-reader OS evidence limits. Production
  remained unchanged; the physical mapping Blue is a separate boundary.
- `37d4227d` corrected only F12's fragment-mapping switch: every remaining
  named fallback is explicit and undefined values throw at that boundary.
  All 27 Unit and 23 Integration cases passed, including exact exception
  attribution and both early-return paths for undefined resolutions. Both
  builds and scoped formatting passed without warnings/errors, failures or
  skips. All 683 test files and 53 protected files stayed frozen. Exact source
  SHA-256: `9bbe5527e0f25b89d315feb32dd22afdd8dcc10982160d1f32ae268c5cb5446f`.
  `artifacts/task27-complete-preflight/framework/implementation/fragment-mapping-green/`
  owns the one-switch diff and final evidence; physical mappings remain separate.
- `836c4058` froze F12's internal fragment mapping with 20 new cases: fifteen
  named/path controls, four early-return controls and one admitted undefined
  resolution. The undefined case failed because no exception was thrown after
  every setup assertion passed; nineteen new and seven existing controls passed.
  Exception-attribution assertions await Green. Build and scoped formatting
  passed without warnings/errors or skips. All existing sources, assertions and
  protected files stayed identical. Exact input and failure evidence:
  `artifacts/task27-complete-preflight/framework/implementation/fragment-mapping-red/`.
- `18728aea` completed F09/F11's flat precedence, exact coherent string
  construction and named ambiguous coordinates in ten existing source files.
  The narrow S04 correction removed only the shared decoder's redundant
  internal null guard; both caller ingress guards remain. All 246 Unit and
  71 Integration cases passed with zero failures/skips or build warnings/errors;
  scoped formatting passed and all gates ran sequentially. All 682 tests and
  53 protected files stayed identical. The optional route-facts accumulator
  remains local because no accepted split or demonstrated benefit warrants
  changing that ownership. Exact evidence and literal mechanical comparisons:
  `artifacts/task27-complete-preflight/framework/implementation/mechanical-conformance-blue/`.
- `0bb82faf` added four direct lexical-stage Unit cases for all named outcomes,
  with independent success coordinates and null failure coordinates. All four
  new and 35 retained Unit cases passed; build and scoped formatting passed
  without warnings/errors, failures or skips. All 3,147 existing inputs stayed
  identical, and the 92-case Integration receipt remains applicable. The
  synthetic volume-root input proves the helper's retained missing-directory
  classification, not catalogue admission. Exact evidence is under
  `artifacts/task27-complete-preflight/framework/implementation/lexical-resolution-purple/`.
- `423424bf` extracted F06's lexical stage, pure fragment projection and ordered
  target-identity queries while preserving the existing resolver callables,
  admission, exception/cancellation boundaries and result precedence. All
  35 Unit and 92 Integration cases passed with zero failures/skips or build
  warnings/errors. Scoped formatting passed. The five-file approved draft
  remained identical; all 681 tests and 53 protected files stayed frozen.
  `artifacts/task27-complete-preflight/framework/implementation/reference-resolution-blue/`
  binds the exact source identities and evidence. Two named lexical failures
  are assigned separate direct Purple; undefined result states have no admitted
  producer through the private constructors and fixed factory surface.
- `24147ddd` added F06's separate characterization: paired lexical admission,
  injected exception/cancellation boundaries and fragment-versus-ID-collision
  precedence. All 27 new and 65 selected existing Integration cases passed,
  with zero failures/skips or build warnings/errors. Scoped formatting passed;
  all 3,142 pre-existing frozen files stayed identical. One initial fixture
  setup failure was corrected without changing assertions; its receipts remain.
  The test SHA-256 is
  `3374effe0fa99990f2a6870c874b133118bed7641a59ae8dcbdac7b5ec4f0de3`.
  `artifacts/task27-complete-preflight/framework/implementation/reference-resolution-purple/`
  owns exact discovery, execution, preservation and refined Blue shape.
- `fbc2b0d7` changed only the decoded destination's nonempty invariant. All
  155 Unit and 37 Integration cases passed, including both construction controls
  and every composed incomplete/retained-root/finding/coverage assertion.
  Both builds and scoped whitespace passed with zero warnings/errors, failures
  or skips. All 680 tests and 53 protected inputs retained their exact bytes.
  Source SHA-256:
  `587142124dfad7f4da8f590d22ec1c3db883ec4c53d93f664454a02b4e7c6610`.
  `artifacts/task27-complete-preflight/framework/implementation/encoded-whitespace-green/`
  owns the exact one-line diff, unchanged callable proof and final receipts.
- `b17ad765` captured F15's two intended failures on unchanged production:
  decoded whitespace threw at construction, and the real command returned
  `failed` instead of `incomplete` after all inventory/declaration admission
  checks passed. Of 155 selected Unit cases, 154 passed, including the two new
  null/empty controls; the one selected Integration case failed at its intended
  status assertion. Later retained-row/finding assertions await Green. Both
  builds and scoped whitespace passed with zero warnings/errors or skips.
  Two Unit files changed and one Integration file was added; all production
  and protected inputs stayed identical. Exact receipts are in
  `artifacts/task27-complete-preflight/framework/implementation/encoded-whitespace-red/`.
- `53562e03` added the separate F07 mapping Purple: 14 literal named mappings
  and two undefined-value diagnostic-attribution cases. All 16 new and 25
  existing Unit cases passed, with zero failures/skips or build warnings/errors.
  Only one test class was added; all 3,140 frozen inputs stayed identical.
  Scoped whitespace passed. The test asserts owned exception attribution and
  message prefixes, not BCL-localized suffixes. Exact receipts are in
  `artifacts/task27-complete-preflight/framework/implementation/navigation-purple/`.
- `da09ccd2` shared generated-navigation preparation and finite state mapping
  between Status and Doctor, preserving their separate source selection,
  physical observations, comparisons and diagnostic attribution. Two readers
  changed and one helper was added. The final narrow-input version passed 25
  Unit and 13 Integration cases with zero failures/skips or build warnings/errors.
  Whitespace and source preservation passed; all 678 existing test files and 53
  protected inputs stayed identical. The 14 named mappings were source-compared;
  direct named/undefined enum evidence is assigned a separate 16-case Purple.
  `artifacts/task27-complete-preflight/framework/implementation/navigation-blue/`
  owns the final receipts and separately retained superseded execution.
- `2793e70c` reused retained documents, metadata projections, location mappings
  and strict UTF-8 counts within their original observation stages. All 128
  Unit and 19 Integration cases passed, with zero failures/skips or build
  warnings/errors. Five production files changed; 678 test files and 53
  protected inputs stayed identical. Scoped whitespace and final source hashes
  passed. Root review removed redundant internal guards and retained explicit
  Open Forge form handling with the original rejection owner for other forms.
  `artifacts/task27-complete-preflight/framework/implementation/retained-derivations-blue/`
  owns the exact freeze, diff and execution receipts.
- `ae507a3a` recorded the complete production assessment and accepted boundaries.
- `b42f6f08` captured six independent Update managed-host regressions. The same-
  worktree Release build passed with zero warnings/errors; all six selected and
  discovered cases failed at their intended assertions, with no setup failures
  or skips. Ordinary outside text produced false divergence, forced replacement
  discarded outside bytes, and explicit-region Unicode hosts exposed mixed
  coordinate slicing. The two root filenames are exercised in each theory.
- Red source, runtime identity, exact selection, raw receipts and final
  token-preserving attribute-format successor are frozen in
  `artifacts/task27-complete-preflight/install-update/red/`. Later verified and
  repeat-no-op assertions await Green; their presence is not a passing receipt.
- U01/U02 Green may change bounded content interpretation, retaining existing
  lifecycle schema and public null region coordinates. Tests are immutable;
  pure model/style work remains separate. Root owns acceptance and commits.
- `4eb8b65c` corrected U01/U02 in one production file, with 14 additions and
  seven removals. Six regressions, 35 existing Update Unit cases and 24 existing
  Update Integration cases passed: 65 total, zero failures/skips or build
  warnings. Exact discovery/execution identities match. All other 2,409 frozen
  source/test/resource inputs and configuration remained unchanged. Scoped
  whitespace passed; three pre-existing informational collection suggestions
  remain for the separate style slice. The exact source SHA-256 is
  `4c4300177395c43cec52758347757ab87f3ab381a210f1743c4195d5db2a4923`.
  `artifacts/task27-complete-preflight/install-update/green/` owns the receipts.
- `8f2a860c` reused the first successful Markdown parse when line-ending
  normalization returns the identical string. The CR/CRLF parse and both
  exception boundaries remain intact. All 29 fingerprint and 38 adjacent
  Integration cases passed, with zero skips, failures or build warnings.
  Scoped whitespace passed; all test assertions and protected inputs were
  unchanged. The source SHA-256 is
  `3345d86179bab29f5c9702ee752c6f8acde024ecce528c36f79b8004463d4467`.
  `artifacts/task27-complete-preflight/framework/implementation/first-blue/`
  owns the freeze, exact diff and execution receipts.
- `657d8580` removed unreachable source-inventory processing and made the
  established source-candidate parent non-nullable. Five files changed, with
  three additions and 41 removals. All 27 Unit and 27 Integration cases passed;
  both builds had zero warnings/errors, scoped whitespace passed, and all test
  assertions and protected inputs remained unchanged. The exact five-file
  freeze and receipts are in
  `artifacts/task27-complete-preflight/framework/implementation/inventory-blue/`.
- `44ff07b6` gave overwrite-path transforms and generated empty-entry syntax
  one owner each. Thirteen existing production files and one new helper changed;
  all tests stayed immutable. All 180 Unit and 11 Integration cases passed,
  with zero failures/skips or final build warnings/errors. Scoped whitespace
  and protected-path checks passed. Independent constant evaluation retained
  the Extension projection host's exact 137 UTF-8 bytes. Receipts are in
  `artifacts/task27-complete-preflight/framework/implementation/syntax-blue/`.
- `2a337a0b` froze F05 with 66 new independently expected decoding/policy cases
  plus 85 existing focused passes against unchanged production. Both added test
  files and all existing assertions are immutable for Blue. The final build
  had zero warnings/errors, scoped whitespace passed, and protected inputs
  were unchanged. The existing encoded-whitespace constructor exception is
  documented as preserved behavior requiring a separate composed disposition,
  not endorsed as desirable behavior. Exact receipts and the accepted four-file
  extraction are in
  `artifacts/task27-complete-preflight/framework/implementation/decoder-purple/`.
- `ed14a045` extracted one strict decoder with a typed failure result while
  preserving the two callers' separate policies. Four production files changed;
  all 151 frozen Unit and 15 Integration cases passed, with zero failures/skips
  or build warnings/errors. Scoped whitespace and exact preservation passed.
  `artifacts/task27-complete-preflight/framework/implementation/decoder-blue/`
  owns the receipts and accepted F02 ownership preparation.
- `9be2d2c3` added 19 Markdown ownership/construction cases; those and 61 existing
  parser/fingerprint cases passed against unchanged production. A bounded probe
  bypassed the six second-copy operations: exactly the intended ownership test
  failed, while the other 18 passed. Original production bytes were restored,
  hash-verified and requalified. Final build warnings/errors, failures/skips and
  whitespace diagnostics were zero. The test freezes owned diagnostic prefixes
  and parameter names without asserting BCL-localized suffixes. Receipts and the
  exact next production map are in
  `artifacts/task27-complete-preflight/framework/implementation/markdown-ownership-purple/`.
- `91069756` established read-only ownership at the six original snapshot
  points, removed materialization, and moved validation to construction support.
  All 80 frozen Unit and 28 Integration cases passed, with zero failures/skips
  or build warnings/errors. Tests, protected inputs, whitespace and normalized
  validator bodies passed preservation checks. Reports explicitly account for
  unconsumed synthesized-record equality effects of changed wrapper identity;
  no production consumer relying on those identities was found. No equality
  shim or concrete backing-type promise was introduced. Exact receipts are in
  `artifacts/task27-complete-preflight/framework/implementation/markdown-ownership-blue/`.

## Previous Completed Horizon — Task State

- State: Complete and locally integrated; Task 7 follows.
- Permanent identity: Task 27 in the [project control ledger](../project-control.md).
- Phase and milestone horizon: phase 4/4, milestone 6/6.
- Owner: root Overseer, sequential; substantive assistance uses Astra/high.
- Authority: the user's standing instruction to finish refactoring and improve
  simplicity, clarified on 2026-09-09 by asking whether C# opportunities were
  actually being assessed, and explicitly approved later that day. No new
  product behavior or publication is implied.

## Execution Capsule

- Accepted base: `6173fb5b`, containing reviewed Task 21 source `391ad956`
  and qualified 6,418 managed/native passes. Task 21 is integrated at
  `75f6ff49`; ancestry reconciliation `12d15251` retained the exact prior tree. Branch:
  `codex/csharp-structural-streamlining`, same clean primary worktree.
- Outcome/profile: bounded structural assessment and sequential isolated Blue
  and Purple slices. Root owns scope, contracts, freezes, commits and integration;
  one continuous Astra/high author prepares and implements accepted slices.
- Consequence: a nonpublic local developer CLI with real filesystem, permission,
  lease and recovery responsibilities. Preserve those guarantees and recovery
  through Git/owned artifacts. No stronger hostile-process or atomicity promise.
- Assessment: reconcile Task 10 prose and table coverage with current production
  inventory and caller relationships across command-private/family helpers,
  Framework capabilities and Shell/root. Evaluate the prepared dead-code,
  conditional-flow, test-oracle and model-placement candidates. Inspect directly
  related call chains for ownership/forwarding duplication. A lexical count is
  navigation, never a finding or an exhaustive semantic coverage claim.
- Preparation inputs: `artifacts/task27-preparation/`, including current
  candidate dispositions, live measurement evidence and model placement inventory.
  Recheck all source identities. Return one finite selected/retained/deferred
  candidate packet with exact callers, protected behavior, minimal paths, and
  evidence before production changes. Do not expand into an open-ended cleanup.
- Expected paths: selected `src/cli/core/` and mirrored Unit/Integration sources.
  Public journey bodies, source Framework/Extensions, parser/wire behavior,
  package/build/configuration, dependencies, and all unrelated work are protected.
  Root must accept any exact callable receiver adaptation before implementation.
- Standard capabilities: ordinary C# and existing typed foundations suffice;
  exceptional machinery is none. No DI, registry, reflective/string dispatch,
  compatibility wrapper, new parser/runtime or warning suppression.
- Evidence: preserve the exact accepted managed/native baseline. Freeze behavior,
  signatures, assertions and affected cases before each slice. Independent Red
  precedes a proved behavioral correction; pure Blue retains frozen tests.
  Purple separately preserves actual conditions and assertions, removing obsolete
  tests only with explicit disposition. For test-only callers, Purple may precede
  Blue deletion so every commit remains compilable. Focused build/tests and
  informational formatting qualify each slice; full managed/native and static,
  callable, prohibited-pattern, protected-path and source-preservation gates
  qualify the final candidate. Exactly three public journeys per command remain.
- Budgets: one fresh review `T27-R1`, one grouped correction `T27-C1` with bounded
  recheck, council zero. `T27-R1` and `T27-C1` are consumed. The latter was
  one exact token-preserving attribute-line wrap with the same reviewer's
  bounded recheck; no behavior or assertion correction was required. Root may record a justified adjustment
  within accepted scope; no new product or external authority is implied.
- Current boundary: M6 complete. Accepted feature `a5fcea25` is integrated at
  `81f22c43`, with exact tree `e7526e7f`. All twenty delta paths match by bytes,
  modes and Git blobs. No source or test work remains in this finite horizon.
  Task 7 follows; publication remains outside local authority.

## M1 Accepted Finite Scope

Root accepted `artifacts/task27-preflight/author/packet.md` and `scope.json`
on the source-identical `12d15251` predecessor. The inventory covers 1,732
production and 669 test/support C# files; caller evidence and semantic traces
support a finite assessment, not exhaustive branch review. The selected scope
contains seven production and four Unit files, no new paths or model moves.

Five isolated commits are selected: Purple moves physical-state and token
rounding evidence to live owners; Blue removes the eight unused alternate
callables/types and obsolete overload; Blue expresses five existing local
decisions directly; Blue reuses Route Move's eagerly retained parsed region
inputs; Purple removes only Install's self-assignment DTO readback test.
The packet owns exact member names, conditions, protected facts and assertion
dispositions. No behavior defect was established; new defects require an
independent Red boundary. All unaffected test bodies and public journeys freeze.

Retain the YAML accessor, isolated lock-store constructor, finite recovery
factory and finite option accessor for the packet's specific caller/contract
reasons. Defer the 82-path model inventory and wider allocation/size cleanup;
untouched model placement is not a reason to create a wholesale migration.
Root checked Route Move's one-to-one document/input order and eager retention.
No runtime speedup or complete codebase conformance claim is made.

M2 must bind exact callable removals, full assertion/body dispositions, literal
0/1/4/5 token oracles, actual focused cases and fresh source/runtime evidence
before implementation. All substantive work uses Astra/high; this concrete
preflight found one additional redundant parse, but supplies no controlled
model-to-model quality or cost comparison.

## M2 Callable And Evidence Freeze

Root accepts the exact callable removals and surviving source-body freeze in
`artifacts/task27-freeze/callable-freeze.json`, the four prospective Purple
bodies and `assertion-dispositions.json`, and each isolated slice's commands
in `slice-commands.json`. All 2,476 frozen source/config/resource hashes match.
Fresh no-restore baseline builds have zero warnings/errors; the deduplicated
focused union passes 111 Unit and 170 Integration cases with zero failures,
skips, pending or other results. Exact discovered/executed type, method and
display-name multisets agree; source/runtime identities remain unchanged.

S1 replaces the eight physical helper cases with real catalogue mapping and
adds four literal live token boundaries. Its candidate, issue and expected
canonical path all become `.agents/subject.md`; that intentional fixture
adaptation preserves projection meaning, not literal source-byte identity.
Unavailable token state is checked through actual AccessDenied observation.
The unused estimator's NotApplicable assertion and contained FromPhysical
assertion are obsolete under the explicit dispositions. Their retained live
projection and inventory evidence remain frozen. S5 separately removes the
Install DTO readbacks without claiming equivalent serialized retained-recovery
coverage. Every unlisted assertion and every public journey remains frozen.

S1 expects 29 focused Unit and nine Integration cases. Baseline Integration
may be reused only while its exact source/config/resources and runtime remain
unchanged. Every production slice rebuilds its affected projects. Full final
managed/native qualification remains required after fresh semantic review;
no new Red is needed for these accepted behavior-preserving changes.

## Isolated Slice Progress And S4 Amendment

- S1 `2ec27bc3` moved evidence to live owners: 29 Unit and nine Integration
  passes. Root matched the three exact accepted prospective bodies and all
  other frozen inputs. Whitespace and informational style checks passed.
- S2 `ff0adf8e` deleted nine declarations across six production files, with
  all surviving source and tests frozen: 62 Unit and 146 Integration passes.
  Informational style reported four pre-existing Route Move IDE0305 sites;
  this deletion-only commit retained them explicitly, without suppression.
- S3 `6d906027` clarified five local decisions: 33 Unit and 52 Integration
  passes, clean builds and informational style. Root inspected the original
  and resulting branch precedence. The author's 47-class truth-table comparison
  is supplementary reasoning, not an additional production-test count.

Root accepts one bounded S4 amendment in the already selected Route Move file:
replace its four eager `ToArray`/`ToImmutableArray` materialization sites with
collection expressions targeting their existing concrete array/ImmutableArray
members. Preserve the indexed Select, ordinal ordering, OfType filtering,
element references and single eager enumeration; remove the now-unused
immutable extension import. No model, signature or test changes are permitted.
`artifacts/task27-freeze/s4-collection-amendment.json` freezes the exact source
and four sites. This resolves observed style diagnostics in the planned source
neighborhood without widening into a general collection migration. Existing
73 focused Route Move Integration cases and informational style qualify S4.

## M3 Completed Candidate And Review

All five selected slices are accepted as separate commits. S4 `143e6f67`
reused parsed Route Move inputs and qualified the four exact eager collections:
73 Integration passes, clean build and informational style. S5 `634d80ef`
removed only the frozen Install DTO readback method: 19 Unit and twelve
Integration passes, clean builds and whitespace/informational style. The exact
prospective S5 body and every remaining source/test input match their freezes.

The final implementation changes eleven files: seven production and four Unit
sources, with no whole-file additions/deletions, model moves, namespace or
surviving callable changes. Each production slice retained all test bytes.
Public evidence remains exactly 28 commands with three journeys each, plus
26 Shell and one artifact case. Final expected managed counts are 2,882 Unit,
1,603 Integration and 111 public, reflecting only four new live token cases
and one removed readback. Complete managed/Linux Native AOT gates remain pending.

`T27-R1` is the single fresh review over accepted base `75f6ff49` and candidate
`634d80ef`, plus this source-identical authority successor. Review all eleven
changed source files and their necessary caller/evidence neighborhood, exact
A1–A5 dispositions and the accepted S4 amendment. Do not turn deferred model
inventory into new scope or count earlier qualified passes as final execution.
Use stable material finding IDs; any accepted correction returns once to the
continuous author under `T27-C1`. C# personal reads/fingerprints remain required.

## M4 Review Acceptance And Final Gate Boundary

Fresh Astra/high reviewer `csharp_streamlining_review` accepted immutable
`c4161e90`, tree `d2806074`, with no material findings. All eleven changed
files, actual callers, full affected bodies and exact per-slice source/runtime
and discovered/executed identities were inspected. The reviewer personally
read and fingerprinted the current complete C# directive trio. The recorded
behavior, ownership, assertion-disposition and isolated-commit claims hold.

Root's supplementary full formatter reported one pre-existing CA1859 hint on
an unchanged private return signature and six xUnit1045 hints on existing
object-backed internal-enum theory rows. The reviewer accepted retaining them:
no material benefit or discovery failure was established, and actual case
identities pass. No warning suppression was added. Fresh final whitespace and
informational style gates passed on all eleven files.

The line-length check found one retained 206-column attribute list in Install.
Root froze it and committed only a whitespace split at `7698f764`; C# tokens,
attribute values/order and all assertions/bodies remain unchanged. The same
reviewer accepted this exact T27-C1 recheck. Fresh affected whitespace/style
and final static checks pass: eleven source files, zero line-length/new
prohibited-pattern/protected-path violations, and all 24 unrelated dirty files
preserved. `artifacts/task27-c1/` and `artifacts/task27-final/` retain the failed
supplementary checks, exact correction and final accepted receipts.

Full managed and supported Linux x64 Native AOT evidence remains pending M5.
The canonical harness freezes source/runtime identity, checks exact case
multisets and requires 2,882 Unit, 1,603 Integration and 111 public cases.
Native Integration/public and managed public selecting the same native CLI
remain separate required executions. No six-platform execution is claimed.

## M5 Full Qualification

Root accepted `artifacts/task27-final/canonical-summary.json` on exact candidate
`d5ccca6d`, tree `34889805`. All 3,167 source/configuration inputs remained
unchanged. Managed Unit/Integration/public passed 2,882/1,603/111; native
Integration/public passed 1,603/111; managed public selecting the same native
CLI passed 111. All 6,421 executions passed with zero failures, skips, pending
or other results. Exact discovered/executed case multisets agree. Each of the
28 commands retains exactly three public journeys.

All five build/publish operations completed without warnings or errors. Native
CLI and test runners are verified Linux x64 ELF artifacts. Version/help, raw
log hashes and runtime manifests agree. Required whitespace and informational
style pass; the supplementary analyzer hints retain their explicit M4 disposition.
Final static/protected checks pass and all 24 unrelated dirty files retain
exact bytes and modes. The canonical summary binds review and execution receipts.
This proves the accepted structural scope on Linux x64, with no six-host,
exhaustive-internals, publication or measured-performance claim.

## M6 Exact Integration

`artifacts/task27-final/integration-freeze.json` freezes accepted feature
`a5fcea2534945914085407a9c3e3672a2694afec` over current develop
`75f6ff491091a619824328ddc5707b33a15ff7c6`. All twenty paths are modifications;
there are no formerly untracked additions or whole-file deletions. Integration
`81f22c43c61afffecbec796fbdd49d0bd8a295d7` has the exact candidate tree
`e7526e7f980c6990026b296227e4955a31b6b8c5`. The integration receipt confirms
all path bytes, modes and blobs and clean Git state. Task 21's already-integrated
source was not duplicated. Final executable evidence remains bound to unchanged
source `d5ccca6d`; subsequent changes only close Working Memory state.

## Milestones

| Phase | Milestone boundary |
| --- | --- |
| 1/4 | M1 finite candidate/placement scope; M2 callable and behavior/evidence freezes |
| 2/4 | M3 isolated structural/test slices and focused qualification |
| 3/4 | M4 fresh review and any bounded correction accepted |
| 4/4 | M5 full combined gates; M6 exact freeze and integration |

## Outcome And Boundary

Assess the remaining production C# internals for concrete simplification
opportunities, then implement justified bounded changes. Task 10 already
reviewed all 28 commands at strategic primary-path level, including design,
shared scope and tests. Its [report](cli-command-surface-audit-report.md) records
selected helpers and explicit limits. Task 21 closes its seven findings.
Neither completion establishes that every internal capability has been assessed.
Do not repeat accepted reviews merely to produce another report.

Start from the Task 21 accepted candidate. Inventory command-private helpers,
family-shared capabilities, Framework mechanisms and Shell/root composition;
reconcile prior semantic coverage with this inventory. Inspect remaining areas
by responsibility and caller relationships. File length and count are discovery
signals, not findings or targets to reduce mechanically. Test helpers are in
scope only where they materially affect owned evidence or production design.

For each candidate, show exact source/callers, the maintenance or reasoning
problem, the smaller design, protected behavior and cheapest decisive evidence.
Separate violated current directives from optional improvements and intentional
boundaries. Prefer direct data flow, cohesive existing facts, nearest shared
ownership and explicit finite policy. Remove duplication of meaning where real
consumers agree; preserve distinct command policy even when syntax looks alike.
Do not add speculative frameworks, DI, registries, reflective/string dispatch,
compatibility layers, dependencies or warning-silencing null suppression.

Every C# author/reviewer personally reads the complete current C# directive,
design and style and reports fingerprints. Match design force to this local,
nonpublic developer tool's actual filesystem and recovery responsibilities.
Retain source preservation, permissions, no-follow boundaries, leases, truthful
partial outcomes and record-last publication. Do not remove safeguards merely
because they add branches, or imply stronger transactional guarantees.

Freeze candidate scope before changing production. Use existing behavior
characterization for pure refactoring and independent failing evidence for
newly proved defects. Preserve three simple public journeys per command and
separate Shell/artifact subjects. Run appropriate focused evidence per slice,
then the required combined supported Linux managed/native boundary, with exact
source/runtime identity and all new/moved/deleted files accounted. Establish
review and correction budgets at activation; do not start an open-ended review
loop or invent an exhaustive-correctness claim.

## Accepted Commit Boundaries

On 2026-09-09 the maintainer explicitly approved this task and required isolated
refactoring and test changes. Freeze the current callable contracts, observable
behavior and test assertions at Gray before each bounded slice. For a proved
behavior defect, freeze independent Red evidence first and commit the correction
separately from pure refactoring.

Blue changes production structure against frozen behavior and tests. Commit
that production-only change after its focused evidence passes. Purple changes
or removes tests in a separate commit, mapping each unique assertion to retained
or replacement evidence at the appropriate tier. Record why an assertion is
obsolete when removal is justified. Test removal does not authorize behavior
change, and moving assertions must not weaken the accepted contract.

Keep the same continuous author where useful; separate commits and protected
boundaries do not require separate agents. Refreeze all changed, moved, deleted
and formerly untracked files before staging. Final combined evidence follows the
accepted Task scope after the isolated increments are qualified.

## Preparation Observation

Read-only inventory at Task 21 M1 found 1,729 tracked production C# files under
`src/cli/`, excluding tests. This is an inventory count, not a semantic coverage
percentage. Extension Update planning is one concrete follow-up input: its
`ExtensionUpdatePlanner.BuildAsync` contains nested conditional selections in
recovery classification, selected IDs and selection kind, while current C#
Design forbids nested or chained conditionals. Verify these exact branches and
their independent evidence at activation. Its overall size alone does not
justify a new abstraction or a command-wide rewrite. Uninspected regions remain
unassessed, not implicitly sound or defective.

## Additional Preparation Candidates

Ignored evidence under `artifacts/task27-preparation/` supplies candidates,
not accepted implementation scope. Reconcile Task 10's narrative as well as its
explicit table locators: absence from the table does not mean unreviewed source.
The coverage note records the broader mechanisms already assessed.

`repair-branch-candidate.json` identifies two nested conditional selections in
RepairPostVerifier. Assess ordered local branches preserving evaluation and
fallback semantics; do not widen undefined-value policy during Blue work.
`install-dto-evidence-candidate.json` identifies the pre-existing Install
`JsonDtoGraphPreservesExactPacketValues` self-assignment test. Task 21 leaves it
byte-frozen and replaces its claimed wire ownership with actual composed
serialization evidence. Consider removing that tautology in isolated Purple
work after freezing real evidence. Assess neighboring CLR shape/nullability
checks separately against their accepted callable contracts.

The caller inventory and `commit-order-candidate.json` also distinguish dead
production helpers from helpers whose only callers are tests. Where needed,
freeze evidence through the live path and remove obsolete helper tests in a
Purple commit before deleting the helper in a separate Blue commit. Do not
claim an equivalent live oracle until the actual condition and assertion prove
it. Refreeze current source and select only justified candidates at activation.

`live-measurement-evidence.json` supersedes the earlier unresolved search for
live token-measurement evidence: StatusRouteTotalAvailableTests invokes the real
RouteContextReader and proves literal zero, two-rune/three-byte and nine-rune
measurements, plus unavailable states. Exact 1/4/5-character rounding boundaries
still need disposition before deleting the dead Status estimator's assertions.
Do not mistake its unused NotApplicable branch for a live provider outcome.

## Activation Requirements

Root first reconciles prior coverage and freezes a finite assessment packet,
including allowed areas, candidate standard, source identity and stopping
boundary. Implement only accepted in-scope findings in coherent sequential
slices. Carry unresolved consequential behavior or support choices to the user;
routine refactoring choices stay with the authorized owner. Record concrete
before/after design and tests rather than claiming model superiority or savings
without comparable evidence. Task 7 ARM64 expansion and Task 13 remain downstream of this work. The
local Linux managed/native gate proves this structural candidate only; Task 13
and Task 22 own the subsequent complete six-target delivery acceptance.
