---
open-forge:
  description: Accepted scope, ownership and evidence boundaries for the comprehensive Task 27 continuation
  tags: [Memory, Working, Contextual, CLI, CSharp, Refactoring, Preflight]
---

# Task 27 Comprehensive Preflight

## Authority And Coverage

The user requested the remaining C# duplication, design conformance and
streamlining work, including a top-down architecture assessment. The existing
[Task 27](csharp-structural-streamlining.md) owns execution state and budgets.
This packet defines accepted implementation boundaries, not passing evidence.
The local source baseline is `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`.

Ten disjoint reports account for all 1,732 production C# files: Shell/root 46,
Install/Update 82, Extensions 188, Library/Repair/Cleanup 264, Route mutations
269, Route discovery 208, discovery commands 324, Framework sources 137,
Framework mutation/lifecycle/recovery 116, and Framework packages 98.
`artifacts/task27-complete-preflight/coverage-summary.json` verifies no missing,
extra, overlapping or changed source paths. Exact bodies and findings are bound
by each partition's `coverage.json` and `findings.md` in that artifact directory.
This is full production-body assessment, not proof of every runtime branch.
The 669 test/support files received focused consumer and assertion inspection;
shared/public fixture support receives an additional bounded assessment.

Every C# assessor personally read the complete current directive trio. Root
also read it completely. Fingerprints:

- `_csharp.md`: `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`
- `design.md`: `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`
- `style.md`: `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`

## Architectural Decisions

Keep Framework responsible for neutral source interpretation, immutable facts,
filesystem primitives, lifecycle persistence and recovery identity. Commands
own selection, force/prune, permission decisions and application, effect order,
failure translation and completion certainty. Keep explicit Shell composition
and independently validated pipeline stages. There is no selected generic
command engine, registry, dependency container or new parser/runtime.

Share a mechanism only when actual consumers establish identical meaning.
Keep filesystem observations fresh at their existing temporal boundaries;
reuse immutable derivations within one stage. Model moves preserve validated
construction, serializer graphs, visibility and ownership. Do not widen a
private implementation fact solely to move its file. Group coherent model
topics without one-file microfolders, aliases or forwarding compatibility types.

The partition findings are accepted for implementation subject to their stated
equivalence and evidence conditions, with these explicit dispositions:

| Partition | Accepted work | Special boundary |
| --- | --- | --- |
| Install/Update U01–U11 | Bounded host correctness, direct plan predicates, wrapper removal, parse/projection reuse, exact validation, neutral snapshot read, models and readable formation/output | U01/U02 require separate Red and fixes; preserve public lifecycle coordinates |
| Framework F01–F14 | Parse/materialization ownership, retained derivations, syntax/decoder ownership, focused resolver decomposition, operational projection, dead processing, models/style and truthful nullability | F12 unknown-value changes require a separate callable behavior boundary; F13 uses native YAML facts only after frozen equivalence vectors |
| Framework mutation M01–M15 | Exact link identity, lifecycle formation/validation, owned byte copies, UTF-8 admission, receipt facts, exception classification, lease identity, wire mapping and locality | M10 requires Red; M13 strict comparisons are Blue, weaker command comparisons require independently justified Red |
| Framework packages P01–P10 | Permission proposal mechanics, direct link classification, model placement, cohesive bridge facts, retained Library mappings, generated-body reuse, lifecycle presence and portable-path reuse | P04 does not cache filesystem reads; P09 mapping stays producer-owned; permission writes remain in command owners |
| Extensions E01–E24 | Binding/model ownership, immutable topology, local parse reuse, Inspect reachability, dead code, cohesive result formation, generated-effect reuse, exact vocabulary and output | E07 joins M13; E20 needs input-mutation Red; E23 unknown-value changes are separate; E18 may use named construction if a new formation adds no value |
| Library/Repair/Cleanup L01–L16 | Identity/indexed validation, discarded outcome removal, retained effects, ancestor ownership, direct preflight, byte splicing, exact comparison/result reuse, models/output | Preparation accepted; L14/L16 require Red; L10 retained to preserve independent observations; L15 coordinates Doctor/Repair presentation ownership |
| Route mutations R01–R10 | Neutral category/exposure observations, retained derivations, direct Move equality, wrapper removal, models, dead code, composition, output and byte edits | Freeze every compared field before R04; R07 test migrations precede deletions; R10 freezes insertion/overlap behavior before allocation changes |
| Route discovery D01–D20 | D01–D11, D14–D17 and D19–D20: truthful facts, immutable formation, exact classifiers/readers/indexes, typed reasons, models/output and profile mechanics | Retain D12, D13 and D18 as documented; projection indexes include failed projections and preserve reference identity |
| Discovery commands C01–C20 | Local model/accumulator/projection/query ownership, exact vocabularies, dead inputs, direct lookup/output, typed Doctor subjects and serializer naming | C20 needs Red; C18 removes only proven unused outer projections and resolves Repair's real dependency; unknown enum behavior is not silently changed |

An implementing owner must report a failed equivalence condition instead of
quietly broadening the change. Such a finding stays accounted for until root
records a concrete retained reason or a separate accepted behavioral correction.
Constructor size, a lexical duplicate count or a style suggestion alone does
not establish a useful abstraction.

## Deferred YAML Simplification

On 2026-09-12 the user rejected spending beta effort on these unusual spellings.
F13 is deferred, with no pending beta decision and no authorization to add
special syntax handling or tests for the examples below. A later simplification
should use ordinary parsed library facts for the fields Open Forge needs and
justify its scope through actual product use. The retained diagnostic explains
the earlier investigation; it does not make those inputs a support requirement.

Root's earlier seven-case isolated diagnostic proved that
equivalent native YAML tag/anchor facts can receive different current Skill
classifications: a verbatim string tag before its anchor becomes malformed,
while anchor-first and short-tag controls are complete. Escaped `!!n%75ll`
retains a text value, while ordinary and verbatim null tags with the same native
tag identity produce missing metadata. All seven frontmatter/native parses
completed. Exact source, input identities and output are in
`artifacts/task27-complete-preflight/framework/implementation-preflight/f13-probe/`.

The accepted routed-authored-metadata foundation freezes grammar or requires
a project decision before stricter/looser interpretation. The earlier question
about changing interpretation is closed by the beta deferral. Native-library
equivalence alone does not justify expanding the current work.
The diagnostic changed no source, test or shared project build artifact; its
isolated copied binaries were removed after retaining source and receipts.

For F12, the accepted physical mapping packet separates 26 new deterministic
cases at existing injection/model boundaries from literal mapping equivalence
and actual branch admission for the two sealed filesystem readers. It does not
claim every OS failure was executed or authorize test-only production hooks.
`artifacts/task27-complete-preflight/framework/implementation-preflight/f12-physical/`
owns the exact named mappings, source identities and evidence limits.

## Cross-Partition Ownership

- Recovery: one Framework-owned strict verified identity comparison. Preserve
  the deletion session's separate checks. Do not add ignore-field flags to
  disguise weaker command comparisons. Characterize each selected correction
  against the accepted exact prepared-final identity before strengthening it.
- Permissions: share pure observation admission and Create/no-op/Replace
  proposal formation in the existing Framework planner. Keep Extension and
  Library approval policy, prompts, leases, writes and result certainty local.
- Escaping: Install, Update and Index have the same bounded UTF-16 token
  policy; one Commands/Shared/Rendering owner is justified. References,
  Extension, Route Inspect and Route-family escaping retain their different
  control spellings and truncation policies. E22 is local allocation cleanup.
- Help and workspace naming: common status/stream help and workspace selection
  representation belong to Shell presentation when exact consumers agree.
  Keep command-specific prose and distinct human names local. Preserve LF
  versus platform-newline behavior explicitly during template rewrites.
- Status precedence: the Extension mutation family shares one identical order;
  Status keeps its different order. Do not promote either as universal policy.
- Byte edits: first establish the identical Route Move/Remove mechanism and
  Repair's validated projection. A broader owner requires equivalent bounds,
  insertion and overflow behavior; do not hide differences behind flags.
- Source projections: neutral payload-to-source formation belongs to Framework;
  Route projection indexes belong to the Route family. Local parsed-document
  reuse never crosses a revalidation/read boundary.
- Doctor/Repair: remove the sibling rendering dependency through a narrow
  shared presentation projection or direct neutral-fact projection. Preserve
  Doctor's serialized outer shape and Repair's actual recovery payload.

## Root Shell And Test Dispositions

S01 moves property-only Shell messages, parsing, invocation and presentation
facts into their nearest Models topics, and `CliCompositionInputs` into the
existing root composition Models folder. Keep pipeline stage validation and
direct typed command registration. S02 removes the `CliParser` forwarding
wrapper only after its argument-validation and direct-test receiver mapping are
frozen. S03 owns the exact escaping/help/workspace presentation work above.

S04 accounts for informational formatting suggestions across production and
tests. Apply idiomatic syntax only when concrete collection type, defensive
ownership, eager evaluation, overload selection and exception behavior remain
clear and equivalent. Do not run a blanket collection-expression rewrite or
suppress diagnostics. Preserve independently authored test oracles and report
retained suggestions with a concrete semantic reason. Fixture deduplication is
limited to the additional support assessment; general test cleanup hooks remain
the user's deferred idea.

## Behavior And Evidence Boundaries

U02 is governed by Update behavior's preservation of all bytes outside bounded
root/provider regions. Keep Install's existing lifecycle schema and null region
coordinates; Update must interpret root-managed ownership correctly. U01 uses
one coordinate system for strict UTF-8 host splicing. The tests must establish
real admitted input before either defect is claimed reproduced.

L14's original parser-invalid public example was rejected during root review:
Shell returns parser errors before invoking the command factory. The reachable
public issue is valid-syntax workspace failure in Attach/Detach/Sync, where the
factory still scans full argv. Inspect already uses parsed values for workspace
failure; its other scanner is a direct-call-only boundary under current routing.
Use composed evidence and the real parser's values; never another token scanner.

L16 must map atomic effect identity to plan steps, including several reference
proposals in one file effect. Adding one to an ordinal is not an adequate fix.
C20 must preserve supplied selector order and interrupted/incomplete facts when
References acquisition ends before resolution.

M13's refined admission trace is accepted in
`artifacts/task27-complete-preflight/framework-mutation/m13-preflight/packet.md`.
The two strict Framework comparisons and three equivalent Extension matcher
bodies share one identity meaning. Nine cleanup sites omit original attribution;
Update omits original command. Those fields are not bound by payload hashes,
and a persistent closed, otherwise valid archive can change them before the
command's fresh catalogue selection. Correct these ten sites only after
independent Red proves real Valid admission and the existing incorrect cleanup
outcome. Preserve each command's existing mismatch representation and the
deletion session's later temporal checks. Retaining verified facts inside a
preparation additionally requires proof of immutable collection ownership.
The existing deletion-attribution fixture needs a separate Purple correction:
preserve manifest-first ZIP order and assert Valid admission, so malformed
archive refusal cannot masquerade as identity-change evidence.

The remaining mutation preparation in
`artifacts/task27-complete-preflight/framework-mutation/implementation-preflight/`
is accepted subject to its exact equivalence stops and a current refreeze after
F10. `packet.md` owns the small production/evidence batches; `model-placement.md`
owns cohesive M14 destinations. Preserve planned-byte ownership, full exposed
UTF-8 failure causes, Prior-first equal-state comparison and null wire-kind
rejection through their specified Purple boundaries. M04's private validation
packet must not become stored result state or change record equality. M08 keeps
the three existing catch boundaries and unsupported-null outcome; M11 caches
only the immutable request key. Candidate identity derivations remain local.
M10 remains separate Red/fix work. M13's accepted identity work is the next
consequential boundary after F10; these later batches need no repeated discovery.

F05 characterization exposed a separate F15 follow-up: a Loader destination
that decodes to whitespace alone can throw from Valid-result construction.
Its neutral callable propagation is recorded in
`artifacts/task27-complete-preflight/framework/implementation/decoder-purple/preserved-edge-disposition.md`.
The decoder extraction preserves that boundary. Root's downstream inspection
selected a nonempty decoded-value invariant, leaving lexical safety and source
existence in their current owners. Freeze independent Red for the admitted
declaration and real composed incomplete result retaining a known root; the
existing missing-source mapping owns its diagnostic. Run and commit Red before
the isolated correction. Exact rationale and admission requirements are in
`artifacts/task27-complete-preflight/framework/f15-preflight.md`; no executed
regression or fix is claimed by that preparation packet. The Task record owns
the subsequently committed Red and qualified fix. Characterization does not endorse the
exception as desirable behavior.

Before each slice freeze exact current source/test hashes, affected callables,
assertion mapping and selected evidence. Behavioral Red is committed separately
from its fix. Blue preserves test assertions. A physical move or callable
simplification may include only its pre-frozen mechanical test imports,
namespaces and receivers in the same coherent commit; prove the assertion
mapping and identify those paths separately in its receipt. This avoids broken
intermediate commits or compatibility scaffolding solely for test compilation.
Substantive Purple changes and behavior corrections remain separate commits.
Keep all 84 command journeys and the existing 27 process/payload cases.

Use the unchanged predecessor's qualified managed/native baseline initially.
Run focused evidence after each coherent slice; run the required complete
managed/Linux Native AOT and native-package qualification at the final coherent
boundary. No remote operation, publication, global install or foreign-host
execution is implied. Preserve the other chat's source Framework/Extensions and
all its Markdown notes throughout.

## Observation

Astra/high full-body assessors produced concrete caller and ownership evidence,
including defects missed by the earlier finite review. Root call-chain review
also corrected a high-confidence public reachability claim in L14. Full-body
coverage improves discovery but does not replace composed regression evidence.
No controlled comparison with the earlier models was performed, so this is an
observed workflow result, not a claim of measured model superiority.

The additional 27-file fixture assessment supplies T01–T08 in
`artifacts/task27-complete-preflight/test-support/findings.md`. Accept its exact
rich-snapshot, external-store facts, common Route seed, owned Context replace,
model placement, existing valid frontmatter seed, unused facade and relocated
process invocation improvements under separate Purple slices. Preserve all
journey assertions and distinct snapshots. T05 also moves `ProcessRunRequest`
with `ProcessRunResult`: intrinsic input validation and defensive copying do not
turn this state-shaped invocation request into a process-running capability.

## Framework Packages Execution Preparation — 2026-09-11

Execution refinement: P06's original packet overstates the existing record-reader
Integration coverage. Root and the Astra/high owner confirmed that its eight
cases do not exercise ambiguity versus malformed decoding. A separate Purple
may add two real-file cases to that existing class, freezing Blocked for duplicate
IDs and Malformed for unordered IDs, null Record, exact retained bytes and literal
Cause. The Task record owns the approved envelope addendum; preserve the original
packet and its evidence identity.

Root accepts the sequential P01–P10 packet in
`artifacts/task27-complete-preflight/framework-packages/implementation-preflight/packet.md`.
Its exact source/type/consumer inventory is preparation, not fresh execution.
Refreeze after the completed M04/M10 changes; preserve their new guards and
recovery classification test when adapting FilesystemFailure imports.
The ten proposed production boundaries and named Purple prerequisites preserve
approval/write policy, filesystem observation cadence and exact owned facts.
Start with filesystem P03 placement; source and shared builds have one writer.

Root read the complete packet and both affected bridge/catalogue contract test
classes. Accept a separate catalogue Purple retaining the actual seventh Library
contributor and asserting its identity while preserving the first six. For P04,
repair the existing ownership Fact's disconnected setup: construct its current
observation directly with the actual mutable owners array, preserve its original
assertions, and add the focused identity/guard oracles before Blue. This replaces
the proposed duplicate ownership Fact while retaining the same protection.
The private helper remains for the other states. Freeze this meaningful Purple
construction in the subsequent mechanical two-fact constructor mapping.

Root additionally resolved the existing top-down TD-01 follow-ups after reading
both complete permission-family operations and their current Framework read and
planner owners. Accept P01's separate pure observation-comparison and planned
action/recovery projection extraction as specified in
`artifacts/task27-complete-preflight/framework-packages/implementation-preflight/permission-facts-followup.md`.
This follows private planner Blue, preserves each caller's I/O and failure order,
and adds no Framework writer. Retain the two application methods: Library's
prior-failure/receipt contract and the families' outer exception translations
are distinct, and existing mutation primitives already own their physical effects.
No generic permission application engine is selected.

## Install And Update Execution Preparation — 2026-09-11

Root read and accepts the complete U03–U11 packet at
`artifacts/task27-complete-preflight/install-update/implementation-preflight/packet.md`.
Its inventory is preparation; refreeze current source before each boundary.
Preserve command-specific admission, causes, observation timing and the completed
U01/U02 managed-host byte corrections. The ordered Blue/Purple boundaries and
root-owned rendering/help overlap remain as specified there.

For U10, use a genuine completed-planning model `UpdatePlanCompletion` under
Update's existing `Models/Planning/`. Construct it only after lifecycle planning
succeeds, with named Request, Payload, Intended, Plan, Effects, LifecycleChange
and Findings facts. Preview consumes it directly, retaining projection and
validation order. This complete stage resolves the existing Preview/Build/
Execution dependency cycle without partial initialization, a fabricated result
or a broad dependency bag. Do not retain it on the public result or change
existing result/execution equality. Keep next-action formation and its separate
Purple prerequisite as specified; never invent interactive-capability facts.

U10 Purple is committed at `deb9dc8a`, with 38 Unit passes. Blue at `77d2fa2d`
passes 38 Unit and thirty Integration cases and uses the exact fifteen receiver
adaptations in
`artifacts/task27-complete-preflight/install-update/implementation/next-action-purple/future-receiver-adaptations.json`.
No assertion, fixture, test title, import or other test bytes changed. Root's
bounded design correction centralizes eleven identical unstarted lifecycle
constructions in UpdatePlanResultFactory.UnstartedLifecycle, preserving their
argument positions and the distinct planned/preserved Preview lifecycle.

U07-C01 is a separate candidate, not an accepted behavior correction. The shared
operation contract requires initial no-follow preflight and Update's behavior
requires the same preflight for dry-run. The proposed contained managed-host
link scenario must first demonstrate trusted admission, a selected replacement,
the actual dry-run/application classification difference, and preserved bytes,
raw link target, lifecycle and recovery absence through real owned fixtures.
The reproduction assignment was automatically rejected for possible
cybersecurity risk before any source/test changes. It remains paused and must
not be retried or routed around. No production changes precede root
acceptance of a reproduced violation. Under-lease protection already exists;
no write-through defect is established. If reproduced, correct initial operation
preflight separately, using current mutation primitives rather than changing
the neutral reader's semantics. Independent U04 is complete; U07 remains paused.

## Extension Preparation Dispositions — 2026-09-11

Root read and accepts the corrected Extension preparation packet at
`artifacts/task27-complete-preflight/extensions/implementation-preflight/packet.md`.
It refines accepted E findings without source/build changes. Root verified 117
bounded source/test inputs, eleven authority hashes and six preparation artifacts,
including the preserved predecessor receipt. The original proposal widened
twelve private single-owner facts solely for placement; those facts now remain
nested, with explicit reasons in `retained-private-facts.json`. Only the two
identical Install observation shapes gain an internal shared model, justified
by their actual EffectPlanner and TargetInspector consumers. Twenty-five move
rows remain executable. This preparation is not runtime qualification.

E18 retains the validated twelve-field data constructor and uses exact named
arguments at both construction sites. Root inspected the complete model and
sole production call: no existing narrow input supplies all derived facts, and
another formation would duplicate the schema. Preserve argument evaluation
order, guards, equality and fingerprint provenance. Root additionally resolved
the following exact boundaries after inspecting their current call surfaces:

- E19 uses six explicit concrete dependencies at the one application composition
  constructor, with named factory arguments. This bounded exception keeps the
  six distinct application capabilities visible while removing a pure forwarder.
  A two-applier carrier adds no behavior; splitting unrelated validation solely
  to lower the count adds scope. Preserve instances, construction/effect order
  and exceptions. No service bag, container or general six-argument allowance.
- E20 keeps mutable planning inputs and captures immutable leaf-owned topology
  snapshots once at Plan.Create. Capture Remove decisions and Update lifecycle
  graphs; preserve immutable nested identities and explicit collection semantics.
  Independent Red uses the existing plan inputs/callables before implementation.
  Retained DTO copies protect against input aliases; their schema remains mutable.
- E21 promotes the exact existing Install lifecycle copy helper to
  `Commands/Extension/Shared/Planning/ExtensionLifecycleSnapshots.cs`, relative
  to Core. Install and the accepted Update ownership correction are real
  consumers. Keep it within Extension. After E20 Red, isolate this pure move
  before E20's ownership fix; retain every necessary lifecycle graph copy.
- E23 has no admitted composed undefined-value regression through current
  providers. Preserve private/inline placement, all defined-state mappings and
  enclosing guards. Use a separate explicit-switch change, exact source mapping
  review and existing ordinary composed controls. Do not widen visibility,
  forge provider states or claim runtime coverage of private discard arms.
  The original finding made direct undefined-value tests conditional on an
  existing accessible seam; producer admission evidence remains distinct.

Cross-command help, workspace names and the selected shared token escaper remain
root-owned S03. Bounded additional observations are preserved in
`artifacts/task27-complete-preflight/root/presentation-preparation.md`; they are
not a complete consumer freeze or execution evidence.

## Library, Repair And Cleanup Preparation — 2026-09-11

Root accepts the complete packet, finding details, model placement and missing
oracles at
`artifacts/task27-complete-preflight/library-recovery/implementation-preflight/packet.md`.
All 400 source/test dependencies and 32 preparation artifacts are verified;
authority advancement is separately reconciled in
`artifacts/task27-complete-preflight/root/library-recovery-preparation-acceptance.json`.
Refreeze each current boundary before authoring. No build/test execution or
implementation authority follows from preparation alone.

- L10 is retained. Stable directory mappings and one current fact set do not
  promise an atomic source-catalogue snapshot across target resolutions. Batch
  caching would remove observations of ordinary intervening source changes.
  The accepted equivalence gate permits retention; no stronger contract,
  implicit cache, new test or pending user decision is introduced.
- L06 preserves each site's actual equality. Repair's List.IndexOf uses default
  record equality and the first matching index. A first-index dictionary with
  the default comparer and TryAdd preserves that behavior. Other membership and
  receipt sites retain their reference comparers. Preserve queried ambiguity,
  zero-use laziness and unrelated duplicate handling; no eager refusal.
- L12 retains the private AtomicPreparationResult in its single existing owner.
  Placement alone does not justify wider visibility.
- L15's real twelve-field Doctor/Repair intersection belongs in Core-relative
  `Commands/Shared/LibraryRecovery/LibraryRecoveryPresentation.cs` and
  `Commands/Shared/LibraryRecovery/Models/LibraryRecoveryProposal.cs`, without a
  single-file Presentation topic. Preserve all twelve ordered required members,
  nullability, JSON graphs, StateIdentity and existing projection guard messages
  and parameter names. Doctor's human formatting remains local. Repair's receipt
  projection has different guards and remains local.
- Before that literal L15 promotion, replace Doctor's correlated nullable
  suppressions with compiler-proven property patterns in a separate bounded
  local change. Root inspected RecoveryEntryState's private constructor,
  immutable discriminant/payloads and coherent non-null factories. Preserve all
  admitted states and the undefined fallback; do not forge states or add test
  seams or guard policy. Freeze existing projection evidence first.

The packet's ordered Red/Purple/Blue boundaries, exact L02 assertion disposition,
real-planner L16 ordinal vectors and independent byte-splice oracles are accepted.
Root retains cross-command help, workspace-label and escaping ownership in S03.

## Remaining Root Preparation — 2026-09-11

Root's S01 physical design and S02 parser-receiver preparation are at
`artifacts/task27-complete-preflight/root/shell-preparation.md`, with 35 proposed
file rows over nineteen current owners. This is a placement decision, not a
complete consumer/assertion freeze or runtime qualification. Its genuine model
clusters preserve visibility, intrinsic fact validation/query methods and
behavior-owning delegates/interfaces. Freeze all consumers before authoring.

S03's bounded workspace comparison identifies seventeen identical wire mappings,
including the existing narrow Extension Inspect enum method. Promote only that
exact meaning after its independent old-callable Purple and consumer freeze;
retain other guard contracts and outer null/human-label policies. The complete
subset and remaining help/escaping conditions are in the previously linked
root presentation preparation. No generic policy bag is selected.

R04 preparation is refined by root's direct source inspection: its existing
lifecycle mirror excludes Ownership.Findings, and its preview mirror excludes
Preview.Ownership, Preview.References, Preview.GeneratedNavigation and Preview.Effects.
The actual included/excluded field matrix governs Blue. Preserve the compared
Preview.Findings and Catalogue.Findings; the original report's general warning
about ownership findings does not authorize a stronger comparison. The remaining
Route mutation preparation is accepted after the complete packet and bounded
callable/model/equivalence/evidence inventories were read. Root verified its
135 initial dependencies, 278 current inputs, seven supplemental sources,
55 authority identities and thirteen artifacts, with no drift. Exact packet:
`artifacts/task27-complete-preflight/route-mutations/implementation-preflight/packet.md`.
This is preparation acceptance, not execution or the final test review.

D14 retains JSON mappings with their existing containing-object error parameters
and expanded human provenance. Share only identical text coverage, row-kind,
quoted-list and depth-explanation helpers. Do not add exception-name parameters
or callbacks to merge the remaining small private branches. Root's bounded S02
receiver freeze is `artifacts/task27-complete-preflight/root/parser-forwarding-preparation.json`;
its shared test-only accessor belongs at Commands/Shared/Composition, where all
five consuming composition tests meet. No production observation seam is added.

## Route Discovery Preparation And Cancellation Disposition

Root accepts `artifacts/task27-complete-preflight/route-discovery/implementation-preflight/packet.md`
after complete packet/oracle inspection, bounded model/consumer summaries and
direct formation/projection-set inspection. All 117 source, 95 test and seventeen
artifact hashes match; four advanced authority files reconcile at their prepared
commit. Ninety-five moves preserve twelve behavior/private owners. D02 retains
the existing thirteen-field formation and validated immutable copies. D12/D13/D18
remain retained; D14 follows the narrowed subset above. No runtime qualification
is claimed by this preparation.

C20 uses existing Unknown for selector rows whose identity resolution was not
established before interruption/failure. Preserve global supplied order,
role-local occurrence and shared form classification, with null Source/Expansion
and empty Candidates. Retain the existing event and Incomplete coverage; never
infer invalid-filter or completed absent-identity findings from this fallback.
The contract requires retained rows and does not reserve Unknown exclusively for
proved absence; completed zero-cardinality resolution keeps its existing invalid
policy. Fill only the absent IncomingSelection fallback and preserve established
rows and the existing operation mode policy. This root interpretation needs an
isolated real Red and fix; preparation does not execute or alter the behavior.

Root accepts `artifacts/task27-complete-preflight/discovery-commands/implementation-preflight/packet.md`
after complete packet, twenty finding plans and thirteen evidence steps were
read. All 308 original dependencies and 412 current inputs match, along with
fourteen preparation artifacts and unchanged canonical C rows. C07 retains the
reachable Context unknown-enum human fallback; C18 retains actual producer guards
and Repair's rich recovery dependency; C20 follows the separate Red/fix decision
above. Receipt:
`artifacts/task27-complete-preflight/root/discovery-commands-preparation-acceptance.json`.

Root accepts `artifacts/task27-complete-preflight/test-support/implementation-preflight/packet.md`
and its complete invariants/oracles after verifying 26 source, 35 test, 24
authority, seven configuration and 21 artifact identities. T03 keeps existing
cleanup ownership, observations, order and timing. T05 preserves request argument
copying, environment reference semantics and class equality. T08 requires the
environment on the string-path overload and forwards it unchanged; the target
overload retains its optional-environment policy. T04/T06 first pin the old public
support behavior in separate Purple commits. The remaining structural slices use
the existing journeys and exact source comparisons. No new E2E journey, general
cleanup redesign or final-test-review claim is authorized by this preparation.
Receipt: `artifacts/task27-complete-preflight/root/test-support-preparation-acceptance.json`.

S01's lexical consumer index is
`artifacts/task27-complete-preflight/root/shell-model-consumers-preparation.json`:
37 symbols across 396 consumer files. This supplies physical receiver selection;
it is not a semantic review, execution receipt or final transformation freeze.

U11's raw-block plan is narrowed by
`artifacts/task27-complete-preflight/root/u11-rendering-refinement.json`.
Root read all three renderer bodies and the escaper. Preserve only the fixed
structural LF-to-platform-newline substitution; do not use broad
`ReplaceLineEndings` over the interpolated block. Retained Unicode separators
belong in the bounded old-renderer literal Purple vectors. Keep each existing
numeric provider and final trimming policy. S03's independently prepared literal
escaping vectors are at
`artifacts/task27-complete-preflight/root/escaping-oracle-preparation.json`;
they are preparation, not executed evidence or a new upstream Unicode matrix.

S03's helper placement and separate assertion-consolidation boundary are in
`artifacts/task27-complete-preflight/root/s03-execution-shape.md`. Keep the
standard help body in Shell presentation; command escaping and exact workspace
wire labels belong to Commands/Shared/Rendering. The manually prepared complete
help-body oracles are in
`artifacts/task27-complete-preflight/root/help-oracle-preparation.json`.
After extraction passes with frozen receivers, remove newly redundant escaping
assertions only in a separate Purple with an exact retained-coverage mapping.

The preceding S03 Purple is committed at `602c0071`: 54 focused Unit cases pass,
with every original input unchanged. Its qualified packet and exact future
receiver map are in
`artifacts/task27-complete-preflight/shell/implementation/shared-presentation-purple/`.
The author corrected a preparation assumption about Route Move/Update/Remove:
their status bodies occur inside Notes. Tests freeze their complete Notes text,
including the literal LF prefix, without changing production placement.

Escaping Blue `d9553e52` and help Blue `ebadd8ef` retain all 54 Purple cases;
their qualification packets are under the same Shell implementation directory.
Root's exact three-file consolidation draft is in
`artifacts/task27-complete-preflight/root/escaping-consolidation-preparation.json`.
It retains all 21 shared escaping cases, inlines their now-single-use exception
assertions, and maps two removed older cases to their exact retained vectors.
Only the two redundant test cases disappear; command-output evidence remains.

Root's bounded later S01 class selection is in
`artifacts/task27-complete-preflight/root/s01-evidence-selection-preparation.json`:
seven Shell Unit and nine parser/composition Integration classes. These are
selections, not discovered or executed counts. The writer still freezes actual
model/consumer transformations. P10's accepted two-case length-result oracle has
an exact draft in
`artifacts/task27-complete-preflight/root/p10-length-oracle-preparation.json`.
Neither preparation changes source or supplies execution evidence.

S01's literal body preparation is at
`artifacts/task27-complete-preflight/root/s01-declaration-body-preparation.json`:
all 37 declarations and 35 destinations match nineteen unchanged source owners
from the accepted full reads. This supplies body identity, not final imports,
consumer transformations or runtime qualification. A pre-write selector guard
caught a delegate return-type reference; the corrected selection requires the
explicit type declaration kind. No source or test was changed.

The accepted S01 naming refinement in
`artifacts/task27-complete-preflight/root/s01-retained-owner-naming-preparation.json`
renames the two nonempty source files left after model extraction: the three
Pipeline delegates use `CliPipelineDelegates.cs`, and the remaining completion
policy uses `CliProcessCompletionPolicy.cs`. Their namespaces and declaration
bodies remain fixed; account for both old/new paths within the same S01 move.

The user-requested extension-method pass runs after these structural/style
slices and before final test analysis. Its accepted scope and review unit are
owned by the Task capsule; it must evaluate actual receiver/capability ownership
instead of applying a blanket rule to extension syntax. Any resulting changes
require the same behavior freeze and isolated structural/Purple boundaries.
