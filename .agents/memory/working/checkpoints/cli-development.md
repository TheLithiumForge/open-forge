---
open-forge:
  description: Current state and next action for the greenfield replacement CLI development program
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Architecture, Plan, Task, Contextual]
---

# CLI Development Checkpoint

## Goal

Build the complete replacement CLI from an accepted top-down architecture and a
detailed hierarchical Task set, then deliver every retained command and release
boundary without restoring the removed local architecture.

## Current State

Find Child 1 is Complete and accepted at exact commit `96fe413` (`Accept Find
source catalogue`). The [Child 1 acceptance
record](../cli-development/tasks/read-only/find-source-catalogue.md)
contains the neutral authority, Route-local projections and policy, final review
corrections, complete verification, public no-write gate, and recorded
cancellation limitation.

Find Child 3 and the Find parent are Complete and accepted in the commit containing
this record update. The Read-Only Commands group and replacement CLI program remain
Active. References remains Planned and must not start in this session. Find Child 2
original Preflight through Blue history is accepted through
exact commit `685e2dd`, but Purple exposed a top-down architecture defect:
generic YAML parsing remained Find-local while Route duplicated Markdown
frontmatter extraction. The Mastermind-owned shared document-foundation correction
is accepted through corrected Purple at exact `2d10474`. Corrected authored-tag
Green is accepted at exact `2337d62`. Final Child 2 evidence and acceptance are
recorded at exact `ff7ce3f` (`Accept Find query operation`). Child 3 focused
Preflight is accepted at exact `28d316a` (`Freeze Find presentation preflight`).
Gray contracts and stubs are accepted at exact `a76a217` (`Establish Find
presentation contracts`). The original ten-file Red packet is accepted at exact
`22d3bff`; its historical Unit evidence was `202` total with `92` passing and
`110` intentional failures, Integration was `43` total with `23` passing and
`20` intentional failures, and published EndToEnd was `13` total with `13`
intentional failures, all with zero skips. The Integration metadata correction
is accepted at exact `6a9a0de`; the mirrored EndToEnd metadata correction is
accepted at exact `eea3d59` (`Complete Find presentation metadata evidence`). Its
post-commit Red reproduction succeeded as intentional Red: managed non-AOT
`win-x64` publish passed, and published Find EndToEnd was `13` total with `13`
intentional failures and zero skips, all terminating at absent Green root
registration.

Historical pre-correction review found two escaping defects: the independent
human test expected `\t` although the frozen contract requires every control as
lowercase `\uXXXX`, and bounded diagnostics escaped before slicing code units,
allowing partial `\\`, `\"`, or `\uXXXX` tokens. That review returned the work
narrowly to Red without changing contracts, Child 2, Integration, EndToEnd,
production, package/project, generated routing, or Route behavior. Supplemental
escaping Red correction is accepted at exact `a865fd1` (`Correct Find escaping
evidence`). It changes only `FindHumanRenderingRedTests.cs` and
`FindDiagnosticsAndHelpRedTests.cs`: the TAB expectation is `\u0009`, and four
Windows renderer-level cases place backslash, quote, TAB, and U+0001 at the
diagnostic truncation boundary and independently validate complete escape-token
grammar, bounds, one-line output, and no payload leak. Clean detached-worktree
evidence includes locked restore, a warning-free Release solution build, format
verification, full focused Unit `206` total with `92` pass and `114` intentional
Gray-boundary failures, and narrow selected evidence `6` total with `6`
intentional failures at named Gray expanded/diagnostic stubs, all with zero
skips. Fresh test-only correctness review at that Red boundary is `PASS`, with
no material optional improvement. The pre-correction narrow Green selection for
TAB plus four token-boundary cases was `6` total with `1` pass and `5` intended
defect-exposing failures, zero skips.

Child 3 Green is accepted at exact commit `cb7874c` (`Implement Find
presentation`) over corrected Red `a865fd1`. It is the production/root-
composition-only Green; no test, support, contract, project, package,
configuration, generated-routing, or Route behavior change enters Green. The
exact Green scope is 15 production paths: Find binding, request, result builder,
and validation; compact, expanded, JSON, diagnostic, help, and shared text
escaping; Shell direct-root command tree and root factory; and root composition.
It keeps one symbol graph and operation/result flow, binding-owned help,
source-generated concrete `FindJsonDocument`, explicit malformed-content request
presence, deterministic bounded human/JSON/diagnostic behavior, and exact one
root Find registration. It adds no reflection, second parser/operation/renderer
catalogue, workspace writes, or Native AOT work. The correction encodes TAB as
`\u0009` and truncates only at complete escaped-token boundaries.

Fresh Green evidence passes locked restore, a warning-free Release solution build,
format verification, and `git diff --check`; focused Find+direct-root Unit
`206/206`; focused Find+generated-serialization Integration `43/43`; affected
Shell+Route Unit `347/347`; affected Shell+Route Integration `160/160`; managed
non-AOT `win-x64` publish; and published Find EndToEnd `13/13`. Every run has
zero skips. Fresh final bounded Green correctness review is `PASS` with no
material findings. It verifies corrected escaping, Find binding, explicit
malformed-content state, root leaf registration, renderer dispatch, JSON
projection/source generation, diagnostics, help, direct Shell integration, one
operation/result flow, and no protected-surface drift. Native AOT is intentionally
not claimed.

Earlier local-improvement review found one material bounded Blue candidate only:
in `FindJsonProjection`, replace duplicate finite `Status` and `FindingCode`
switches with canonical `CliStatusDefinitions.Read(...).MachineName` and
`FindDefinitions.ReadFindingCode(...)`. Its separate escaping correctness finding
was resolved in Green; it is not a Green defect. Blue applied that candidate and
is accepted at exact `3f81e76` (`Simplify Find JSON projection`). It changed production
structure only in
`src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
it replaced the duplicate local switches with the canonical readers, then removed
the two duplicate private mapping methods. No behavior, public
output/order/schema, test/support, contract, package/project/configuration,
generated routing, Shell/root, Route, workspace-write, or Native AOT change
occurred. Blue evidence is a warning-free Release solution build, format
verification, `git diff --check`, focused Find+direct-root Unit `206/206`, and
focused Find+generated-serialization Integration `43/43`, all with zero skips.
No managed republish or Native AOT claim is needed for this one-file
behavior-preserving Blue. Fresh bounded Blue correctness review is `PASS`: all 7
status and 17 finding-code mappings and undefined-value exception behavior are
exact; JSON model/property order/context is unchanged; static canonical readers
remain source-generation/AOT-safe. Fresh local improvement review is
`APPROVED — NO_MATERIAL_IMPROVEMENTS`; the one-file simplification is complete,
removing duplicate mapping ownership without adding indirection.

Fresh Purple assessment ran read-only from exact clean Blue `3f81e76` against the
exact ten Find Child 3 test/support surfaces. Verdict:
`NO_MATERIAL_IMPROVEMENTS`; the no-op Purple acceptance is recorded at exact
`426d4f5`. It changed no test/support, production, contract, project, package,
configuration, generated, Route, Shell, or root file, and no Purple code/test
commit was manufactured. Unit tests and typed `FindPresentationTestData` are
coherent at nearest scope; splitting the fixture fragments authority. Integration
and EndToEnd workspace/assertion helpers are intentionally project-local. The
supplemental diagnostic token-boundary fixture/parser is local, independent,
structurally sound, and proves complete escapes, bounds, one-line output, and
payload exclusion. Minor wrappers and repeated status expectations are
preference-only independent contract oracles. Purple evidence from clean Blue is
focused Unit `206/206`, focused Integration/serialization `43/43`, and published
Find EndToEnd `13/13`, all with zero skips; source diff/check against `3f81e76` is
clean/empty. No Native AOT claim is made, and the record-only commit is not a test
change.

Find Child 3 and the Find parent are now Complete and accepted in the commit
containing this record update. The final managed/native gate and package, artifact,
static, public no-write, and protected-surface audits passed. The sole immediate
continuation is to freshly verify `develop`, squash-integrate the accepted
`feature/cli-find` tip into local `develop`, commit that one squash, prove exact tree
equality, do not push, and halt. `CLI-EDGE-001` remains non-product only.

The solution-wide
[Modern C# Improvements](../cli-development/tasks/modern-csharp-improvements.md)
Task is Complete and accepted at exact `a1cbf09`. Its Preflight
was accepted at exact commit `55eb82e`; the Framework modernization batch at
`a90af59`; the Shell/root batch at `fe10525` (`Modernize Shell nullable flow`);
the Route Inspect/family batch at `62a1dd9` (`Modernize Route Inspect nullable
flow`); the Route List batch at `273eb45` (`Modernize Route List nullable flow`);
and the Tests/support batch at exact clean source commit `6af5fb1`
(`Modernize test support nullable flow`). All five ordered mutation batches are
accepted, and no task-local correction pass was consumed. The final gate passed
from clean source commit `6af5fb1` and is recorded at `a1cbf09`. Find Child 2's
focused Preflight is accepted at exact `e24b9fe`. It freezes the detached grammar,
parser-owned occurrence adapter, neutral Markdown facts, typed operation/result
graph, complete Unit/Integration Red matrix, commands, and strict phase
boundaries. The Gray
callable contract is accepted at exact `8cef8f7` after a
warning-free build, exact package resolution, protected-path audit, and final
correctness and improvement passes. The complete Red packet is accepted at exact
`571ee87` with 50 declarations and 110 executable cases. Its exceptional Gray/Red
boundary correction is accepted at `c0af2d1`, and the maintainer-authorized final
match-count assertion correction at `ebbd9d0`. Green is accepted at exact
`02b5f7a` after a warning-free Release build, format and diff checks, focused Unit
`90/90`, focused Integration `20/20`, affected Source/Route Unit `562/562`, and
affected Source/Route Integration `183/183`, all with zero skips. Blue is accepted
at exact `685e2dd` after preserving focused `90/90` and `20/20`.
Two final bounded correctness reviews pass after correcting unavailable Markdown
boundary coverage, projection-only ambiguity coverage, repeated inspection
findings, and explicit-null `open-forge` shape handling. Child 2 is Complete with
shared `Framework/Documents/{Markdown,Yaml}` Gray accepted at exact `2cae7a4`, Red
accepted at exact `a673ebe`, and Green accepted at exact `d4701ad`. Correction Blue
is accepted at exact `0006915`, and corrected Purple at exact `2d10474`. Final
review supplemental Red is accepted at exact `c72dd5e`, and its evidence
correction at exact `23fe39a` after its Unit portion at exact `d494adb`. Corrected
Green is accepted at exact `2337d62`; final Child 2 evidence and acceptance are
  recorded at exact `ff7ce3f` (`Accept Find query operation`). Child 3 focused
  Preflight is accepted at exact `28d316a`, Gray at exact `a76a217`, original Red
  at exact `22d3bff`, the Integration metadata correction at exact `6a9a0de`,
  the mirrored EndToEnd metadata correction at exact `eea3d59`, and the
  supplemental escaping Red correction at exact `a865fd1`. The corrected Red
  boundary has Unit `206` total with `92` pass and `114` intentional Gray-boundary
  failures, zero skips. Child 3 Green is accepted at exact commit `cb7874c`
  (`Implement Find presentation`) over corrected Red `a865fd1`; fresh focused and
  affected evidence passes, and the final bounded correctness review is `PASS`
  with no material findings. Blue is accepted at exact `3f81e76`
  (`Simplify Find JSON projection`) after the one-file `FindJsonProjection.cs`
  canonical mapping simplification. Its focused Unit `206/206` and
  generated-serialization Integration `43/43` evidence, warning-free build,
  format verification, and diff check pass with zero skips; its bounded
  correctness review is `PASS`, and its local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`. The no-op Purple acceptance is recorded
  at exact `426d4f5` with verdict `NO_MATERIAL_IMPROVEMENTS`;
  focused Unit `206/206`, focused Integration/serialization `43/43`, and
  published Find EndToEnd `13/13` pass with zero skips, and source diff/check is
  clean/empty. No test/support, production, contract, project, package,
  configuration, generated, Route, Shell, or root file changed, and no Purple
  code/test commit was manufactured. Find Child 3 and the Find parent are now
  Complete and accepted in the commit containing this record update.

The accepted Route Inspect/family batch changed exactly 32 production C# files in
`Commands/Route/Shared/**` and `Commands/Route/Inspect/**`; the exact
`RouteBinding.cs` and `RouteDefinitions.cs` files were in the owned boundary and
were unchanged. No tests, projects, packages, dependencies, generated files, or
configuration changed. Directory-pathspec audits reduced ordinary postfix null
suppressions from `50` to `0` and `ArgumentNullException.ThrowIfNull` from `113`
to `83` (30 removed). After Framework, Shell/root, and Route Inspect/family,
global authored
production counts are `26` suppressions and `327` `ThrowIfNull` calls. Retained
guards and the truthful state, metadata, graph, JSON, and escaping changes
preserve parser/filesystem/cancellation/status/output behavior and the accepted
collection, ordering, allocation, comparer, serializer, source-generation,
AOT-shape, and public-exception boundaries.

Verification after the final local improvements passed `git diff --check`, format,
the Release solution build with zero warnings/errors, zero informational
CA1062/CA1510/CA2264 diagnostics, Unit Shared/Inspect `171/171`, Integration
List/Inspect `159/159`, and a fresh managed publish with PublishedRouteInspect
EndToEnd `31/31`, with zero skips. This batch makes no Native AOT claim. Fresh
correctness review passed, improvement review closed
`NO_MATERIAL_IMPROVEMENTS` after two local findings were applied, and the missing
internal parsed-model `RouteInspectGeneratedEntries.ReadReason` accessor found
missing by the initial build was added before acceptance and evidence rerun. No
task-local correction pass was consumed.

The accepted Route List modernization batch changed exactly 18 production C# files
under the exact `Commands/Route/List/**` scope from Route Inspect acceptance
commit `62a1dd9` (`Modernize Route Inspect nullable flow`) and was accepted at
exact commit `273eb45` (`Modernize Route List nullable flow`). No tests, projects,
packages, dependencies, generated files, configuration, or product contracts
changed. No output or test expectation changed. Scoped ordinary postfix null
suppressions fell from `26` to `0`; `ArgumentNullException.ThrowIfNull` fell from
`136` to `106` after removing 30 trusted internal duplicates. After all four
production batches, global authored production counts are `0` ordinary postfix
null suppressions and `297` `ArgumentNullException.ThrowIfNull` calls.

The retained 106 guards cover System.CommandLine/parser, physical filesystem,
read/writer/serializer ingress, request/operation/resolver/topology stage
boundaries, constructors/model/state factories and invariants, cancellation, and
frozen direct exception boundaries. An initial review found over-aggressive
boundary removals; the guards were restored before acceptance and all evidence was
rerun. No task-local correction pass was consumed. The truthful changes use a
private immutable `RouteListSelectionStage` only for ResolveId/ResolvePath repeated
context, derive the attempted explicit reference once, use
`RouteListDepth.FiniteValue` for finite-depth consumers without changing the
`<=`/`<` off-by-one behavior, add filesystem `Entries`/failure-state accessors and
patterns, complete metadata patterns, and make `RouteListTextEscaping` inputs
non-null.

Loader/topology/depth, physical containment/alias/cycle/symlink, UTF-8/read,
cancellation, status/finding/next-action/no-write, exact human/diagnostic/JSON
output and source-generation/AOT shape, collection/order/allocation/comparer
semantics, and accepted public/direct exception behavior remain unchanged. Final
verification passed git diff/path checks, format, the Release build with zero
warnings/errors, zero `CA1062`/`CA1510`/`CA2264` diagnostics, Route List Unit
`117/117`, Integration `89/89`, and a fresh managed publish with
`CliProcessTests` EndToEnd `26/26`, all with zero skips. This batch makes no Native
AOT claim. Fresh correctness review passed after guards were restored. Improvement
review closed `NO_MATERIAL_IMPROVEMENTS` after applying the single-reference-
derivation and centralized-`FiniteValue` findings.

The accepted Tests/support modernization batch changed exactly 40 active test and
support C# files: Unit 24, Integration 14, EndToEnd 1, and TestSupport 1. The
production, project, package, dependency, generated, configuration, and product-
contract surfaces are unchanged, as are test identities, expectations, tiers,
order, fixtures, and count. Active test postfix suppressions fell from `169` to
`8`, and test `ArgumentNullException.ThrowIfNull` calls fell from `21` to `8`;
production remains at `0` suppressions and `297` guards. Full Unit `617/617`,
Integration `241/241`, fresh managed published EndToEnd `57/57`, and the final
EndToEnd rerun `57/57` pass with zero skips. Static audits and reviews pass. This
batch makes no Native AOT claim. The detailed truthful-change and frozen-retention ledger is in
the [Modern C# Improvements](../cli-development/tasks/modern-csharp-improvements.md)
Task.

The final Modern C# acceptance gate passed from exact clean source commit
`6af5fb1`. The Release build was warning-free, format and diff checks passed, and
informational `CA1062`, `CA1510`, and `CA2264` diagnostics were zero. Full managed
Unit, Integration, and freshly managed-published EndToEnd passed `617/617`,
`241/241`, and `57/57`; focused Source/Route Unit and Integration passed
`562/562` and `183/183`. The local `win-x64` Native AOT root publication drove
managed EndToEnd `57/57` against the native root, the Native AOT Integration
executable passed `241/241`, and the Native AOT EndToEnd executable passed
`57/57`; every run had zero skips.

The managed and native public no-write fixture passed all four invocations. Route
List returned exit `5` and `blocked`, and Route Inspect returned exit `3` and
`incomplete`, for both executables. Each invocation had empty stderr, typed JSON
`command` and `status`, and unchanged byte/hash/entry snapshots. The package audit
listed all six projects and found no vulnerable transitive package. The exact
project graph, one `.slnx`, six projects, 343 authored active C# files,
reflection-disabled JSON/source-generation/AOT settings, and artifact routing
remained unchanged. No project-local `bin/obj` directories were present; ignored
outputs remained under `src/cli/artifacts/`.

The final static audits report production suppressions `126 → 0` and production
`ArgumentNullException.ThrowIfNull` `380 → 297`. Active test suppressions are
`169 → 8` frozen intentional injections, and test guards are `21 → 8`.
`required` remains `97`, `init` remains `122`, nullable-analysis attributes are
`14`, and all active tests retain `500/500` `DisplayName`, `Feature`, and
`Evidence` identities with `103` argument assertions. No forbidden nullable
pragmas or `SuppressMessage` entries exist.

The changed-path audit from accepted Child 1 commit `96fe413` is exactly 121
authorized paths: 112 C# paths and these nine Working records. No project,
configuration, dependency, or generated path changed. Fresh final integrated
production correctness review: `PASS`. Final test/evidence review: `PASS`. Final
improvement review: `NO_MATERIAL_IMPROVEMENTS/PASS`. Optional pre-existing shared
projection and failure-reader extraction remains deferred because it is not a
blocker and would reopen accepted architecture or mutation batches. The native
claim is local `win-x64` only. No six-RID parity, `develop` integration, or push is
claimed; `feature/cli-find` remains isolated and `develop` remains `e77902a`.

## Historical Progress

- The final removed implementation is preserved at Git commit `4b873de`.
- Architecture and delegation governance is committed at `aa7d178`.
- The old production tree and root C# workspace were removed at `40ba03e`.
- Candidate tests are quarantined under
  `src/cli/tests/preserved/route-list-v1/` and are not active projects.
- The complete greenfield CLI Architecture, active Plan, and initial 57-file
  hierarchical Task set are authored; the current route-inspect decomposition adds
  five ordered child Task records.
- The scoped .NET 10 workspace, six-project graph, exact package graph, and
  artifacts routing exist entirely below `src/cli/`; restore and topology checks
  pass.
- The command-free Core and thin root host are committed at `2662f50`; active
  Foundation evidence and six-RID CI scaffolding are committed at `7a601cc`.
- Foundation acceptance passed with 33 Unit, 20 Integration, and 4 EndToEnd cases,
  current `win-x64` Native AOT root and test executable runs, a clean package
  audit, on-disk boundary audits, and a final independent review with no findings.
- Route-list command-local definitions, binding surface, request, selection, row,
  provenance, finding, coverage, result, operation call surface, preserved-test
  disposition, and complete evidence matrix are frozen. The earlier route-list
  contract increment passed a warning-free build with 46 Unit cases; independent
  code and writing reviews passed.
- The completed route-list selection/Loader increment accepts source-reference,
  exact ID/path, catalogue identity, adjacent overwrite, strict Loader
  region/destination, Loader-root selection, and selection-level physical-alias
  behavior. The complete warning-free Release build, 152 Unit cases, and 59
  Integration cases pass; final correctness and local improvement reviews pass.
- The completed route-list filesystem increment inventories only proven-contained
  sources through deterministic real-OS traversal, strict UTF-8 and generated-YAML
  metadata reads, typed physical/read findings, finite aliases, active cycles, and
  cancellation retention. The complete warning-free Release build, 214 Unit cases,
  and 69 Integration cases pass; final correctness and local improvement reviews
  pass.
- The completed route-list topology increment forms immutable authored route
  relationships, selected finite or complete depth, canonical rows, coverage,
  status, findings, and next actions without filesystem access. The warning-free
  Release build, 243 Unit cases, and 80 Integration cases pass.
- The completed route-list presentation increment adds route-list request binding,
  typed invalid depth/workspace results, one inventory/selection/topology/result
  operation coordinator, compact and expanded human output, a dedicated
  source-generated JSON DTO projection, bounded verbose diagnostics, route/group/list
  help, root composition, and managed plus published-process evidence. JSON follows
  the contract with a top-level envelope and result fields `selection`,
  `requestedDepth`, `effectiveDepth`, `coverage`, `findings`, and `rows`; finite
  depths are numbers and `all` is a string.
- The route-list slice is accepted for local development and integrated by squash
  commit `edca509` (`Establish and accept route list`). Before this planning
  update, exact merged-tree equality was verified: branch
  `feature/cli-route-inspect` and `develop` both resolve to the exact accepted
  route-list tree at `edca509`; no route-inspect production or test tree is
  preserved at that baseline. The exact-commit verification passed format, the
  warning-free Release build, 259 Unit cases, 84 managed Integration cases, 7
  managed EndToEnd cases, the published `win-x64` Native AOT root, 84 native
  `win-x64` Integration cases, and 7 native `win-x64` EndToEnd cases, all without
  a skip. The package/vulnerability audit, scoped restore, `git diff --check`,
  and no-write workspace-hash evidence remain clean. All executable and code
  route-list acceptance gates are green. No shared route facts were promoted
  before route inspect.
- Route-inspect planning is committed at `37d2e70`. Its contracts/evidence child
  has an accepted production-only Gray surface under
  `Commands/Route/Inspect/`: command definitions plus topical Operation,
  Resolution, Profile, and Result models. Focused formatting and the full Release
  solution build pass with zero warnings/errors; targeted correctness and local
  improvement reviews pass. Gray contains no tests, inspection behavior,
  promotion, binding, rendering, serialization, or composition.
- Resolution/promotion Phase 0 and Gray are accepted at exact commit `ea4c1dd`
  (`Freeze shared route fact contracts`). The reviewed Task-local Gray extension
  defines candidate route-family source, Loader-parser, overwrite, catalogue, and
  topology facts plus a fact-only Inspect resolution handoff and throwing callable
  skeletons. It contains no behavior, List migration, or completed promotion.
- The initial reviewed Red packet is committed at `fcf78d0` with 187 Unit cases and
  24 real-workspace Inspect Integration cases. Green review exposed omitted Loader
  safety, required-ancestor availability, and List unsupported-form regression
  requirements. The first supplemental Red packet is committed at `ea95b27`. A
  second production-free supplement adds unreadable/malformed Loader route-state
  evidence and three root/suffix source-form parity regressions. Focused whitespace
  formatting and the warning-free Release solution build pass. The final
  production-free supplement adds two-case real-workspace Red evidence for the
  expectation that selecting either exact base candidate leaves the overwrite pair
  ambiguous. The unchanged full Unit run is 446 total: 276 pass and 170 intentional
  failures rooted in shared throwing skeletons. The full Integration run is 118
  total: 88 pass, including all 84 accepted baseline cases and four supplemental
  List regressions, while 30 Inspect cases fail only because
  `RouteInspectResolver.ResolveAsync` throws `NotImplementedException`.
- The accepted resolution/promotion Green is committed at `334f2ba` (`Implement
  shared route resolution`) from final Red commit `fcf6124`. Shared route grammar,
  source facts, metadata, catalogue, overwrite,
  Loader parsing, and authored topology now have List and Inspect consumers;
  obsolete List-private implementations and superseded evidence are removed or
  moved without promoting command policy. Inspect-local safe inventory and source
  resolution distinguish routed, detached, unrouted, unresolved, ambiguous,
  overwrite, cancellation, and containment outcomes. Format verification and
  `git diff --check` are clean; the Release build has zero warnings/errors. The
  reviewed Purple route-list delimiter regression raises Unit evidence to 443/443
  and proves lexical scanning stops at `--`; the accepted Green boundary records
  118/118 Integration cases and 7/7 process EndToEnd cases against the built
  Release executable. Correctness and local-improvement reviews pass, and all
  changed/new production files are under 200 lines. Resolution/promotion and
  profile formation are complete; presentation is active.
- Resolution/promotion Purple is committed at `a54f4e0` (`Complete shared route
  resolution`). Read-only profile Preflight adopts that exact baseline and freezes
  one resolution invocation, one accepted graph/fact input, inspect-local profile
  and result builders, ordered generated visibility intersected with authored
  topology, all typed reading reasons, independent loading/Axioms availability,
  strict UTF-8 round-trip measurement, protected presentation boundaries, and the
  complete Unit/real-OS Integration Red matrix. Its no-change evidence carries
  into Gray; no profile production behavior has changed.
- Profile Gray is committed at `d53e5f7` (`Freeze route inspect profile
  contracts`). Reviewed Red adds 47 Unit and 12 real-OS Integration cases while
  leaving Gray production unchanged. The warning-free Release build passes. Full
  Unit is 490 total with 449 pass and 41 intentional failures caused by the Gray
  skeleton; full Integration is 130 total with 118 pass and 12 intentional
  failures caused by the Gray skeleton;
  accepted Inspect resolution remains 30/30. Correctness reviews pass after
  generated-marker/tag, possible-resolution, measurement, cancellation, snapshot,
  diagnostic, and low-arity fixture corrections.
- The maintainer authorized the post-route-inspect sequence: squash the accepted
  route-inspect branch into `develop`; create one generic-improvements branch;
  first remediate parser/raw-argument/special-edge deviations using standard
  library behavior and general typed validation; then improve reusable test
  fixtures, long signatures, and the root-host/Core architecture; accept and
  squash that branch into `develop`; only then start the next product Task.
- Green implementation exposed one impossible Red assertion: generic
  `RouteInspectFact<bool>` represents not-applicable through its state and reason,
  while unused value storage remains `false`, not runtime `null`. Green production
  WIP was isolated and the first correction committed at `dec7b3f`.
- Resumed Green exposed two further impossible Red assertions: unavailable generic
  value-type facts have the same default-value representation, and the
  determinism case dereferenced topology counts that the Behavior Contract makes
  not-applicable for an ordinary routed leaf. Those corrections committed at
  `bd8a5b7`.
- The next real-OS Green run exposed a missing line break between valid generated
  frontmatter and body in the profile fixture, plus one Unit expectation that
  collapsed the frozen independent inherited/local Axioms model despite matching
  Integration evidence. Those corrections committed at `2016100`.
- Resumed cross-layer Green then exposed two remaining conflicting expectations:
  one Unit count omitted a visible ancestor `#LoadNow` sibling required by the
  selected-closure contract, and one Integration assertion contradicted the
  frozen routed-leaf local-Axioms `NotApplicable` value. Those corrections
  committed at `e1884ff`.
- Fresh Green review then exposed one inherited-Axioms expectation that listed a
  Loader with no `Axioms` section as a contributor, contrary to the accepted
  contributor-only provenance contract. That correction committed at `868a2c1`.
- Final Green improvement review then exposed measurement/reading fixtures whose
  assertions require root startup membership but whose Loader declared no
  `#LoadNow` edge. Loader root topology establishes route exposure, not task-start
  loading. That correction committed at `e8dba19`.
- Profile Green is accepted at `c407e24` (`Implement route inspect profile`). One
  resolver result drives one typed profile/result with Loader-owned startup
  loading, selected and narrow closures, reading reasons, five exact physical
  measurements, topology, effective Axioms provenance, availability, cancellation,
  conditions, observations, status, and next action. Targeted formatting and
  `git diff --check` pass; Release build has zero warnings/errors; Unit is 490/490,
  Integration is 130/130, and accepted Inspect resolution is 30/30. All changed
  production files remain below 200 lines and new callables remain at four or
  fewer parameters. Final correctness review passes and final improvement review
  finds no material improvement.
- The completed profile record remains in its current route because the authorized
  `open-forge-old index` routing action reproduced the existing `CLI-EDGE-001`
  duplicate-entrypoint report outside this Task. Integrated acceptance owns the
  later link-safe move and generated-index refresh; presentation is not blocked.
- Presentation Phase 0 inspected the actual binding, composition, rendering,
  serialization, parser, and evidence surfaces. It proved that root-owned
  workspace/view delimiter guards and raw global-input scans conflict with the
  pinned-parser rules in the CLI implementation Directive, while the delimiter
  guard crosses `--` and can misclassify an option-like Inspect source operand. A
  narrow correction is accepted before presentation Gray: typed parse results
  own all six global values, parser-owned data supplies occurrences, root
  workspace/view delimiter policies are removed, and the explicitly contracted
  Route List depth exception stops at `--`. The planned focused Red freezes
  nonempty values across valid spaced/equals/colon forms and invalid
  attached-empty forms for `--workspace` and `--view`. The correction starts with
  production-free Red from exact activation commit `6a3c860`; every broader
  parser audit remains in the maintainer-authorized generic-improvements branch.
- Initial Shell Red is accepted at `41e46d5`. Green investigation exposed a
  pinned-parser interaction hidden by the removed raw global scan:
  `--depth= --json` assigns `--json` to an exactly-one depth symbol and therefore
  removes the typed JSON occurrence. The accepted published Route List result
  must remain JSON. Green WIP is isolated; a production-free supplement now owns
  the narrow zero-or-one depth token-capture evidence before the one-line
  RouteListBinding correction. Public depth validation and every broader
  raw-depth remediation remain unchanged and deferred, respectively.
- The depth-arity Red supplement is accepted at `36b1cfe`. The resulting
  five-file Shell Green passes a warning-free Release build, `501/501` Unit,
  `143/143` Integration, and `9/9` managed published-process EndToEnd cases.
  Formatting, `git diff --check`, raw-helper audit, file-size limits, and final
  correctness review pass. Broader Route List signature and parser remediation
  remain in the authorized later audit.
- Shell Green is accepted at `d53a44b`. The reviewed Blue cleanup moves typed
  extraction into `CliGlobalInputReader` and documents the exceptional depth
  token-capture setting. The warning-free Release build, `501/501` Unit,
  `143/143` Integration, freshly published `9/9` EndToEnd, format, diff, and final
  reviews pass again. No broader parser or Route List signature work was admitted.
- Shell correction Blue is accepted at `222ada9` (`Clarify typed shell input
  ownership`). Its `501/501` Unit, `143/143` Integration, and `9/9` freshly
  published EndToEnd result is the presentation Task's complete beginning
  baseline. The maintainer's updated execution rule now limits the presentation
  inner loop to new and directly affected evidence and requires the complete suite
  once more only after the final presentation change. The generic-improvements
  sequence remains parser remediation first, then test architecture, then
  signature and root-host/Core work. The active presentation Task's
  [Beginning Full-Suite Baseline](../cli-development/tasks/route-discovery/route-inspect-presentation.md#beginning-full-suite-baseline)
  records the projects, managed environment, published `win-x64` executable,
  variables, commands, counts, and results.
- Compile-only Presentation Gray is complete and reviewed from exact planning
  baseline `3b5078e`. It freezes route-family ownership, typed invalid-binding
  context, Inspect binding/rendering/help, required JSON projection models, and
  source-generation registration with all behavior entrypoints throwing. Release
  build, format, diff, focused Route List syntax `5/5`, focused serialization
  `2/2`, file-size, arity, correctness, and local-improvement checks pass. No full
  suite was repeated under the focused inner-loop rule.
- Presentation Gray is committed at `bc300ea`. The production-free Presentation
  Red matrix is complete and accepted after corrections for exact public
  next-action wording, typed JSON next semantics, zero-operation invalid paths,
  complete overwrite-layer and required DTO coverage, semantically valid fixed
  fixtures, contract-bounded escaping and diagnostics, and mirrored helper
  locality. The warning-free Release build, format, and diff checks pass.
  RouteInspect Unit is `237` pass plus `38` intentional Gray failures;
  Integration is `43` pass plus `22` intentional absent-registration failures;
  generated serialization is `1/1`. Directly affected List and Hosting failures
  are limited to new Inspect help truth. The existing managed Gray executable
  produces `28/28` intended RouteInspect process failures at absent registration.
  Final correctness review passes and local-improvement review finds no material
  improvement. Green had not started at that Red evidence boundary.
- Presentation Red is committed at `767c146` (`Freeze route inspect presentation
  evidence`). Presentation Green is active from that exact predecessor with all
  Red expectations and fixtures frozen.
- Presentation Green implementation and focused review are complete. One
  family-owned route group now composes List and Inspect; Inspect binding,
  contextual invalid input, human/JSON/diagnostic/help projection, and root
  registration are implemented; temporary List/Shell compatibility paths are
  deleted. Release build, format, and diff pass. Focused Unit passes with
  RouteInspect `284/284`, Route List `219/219`, Shell binding `9/9`, and parser
  `20/20`; focused Integration passes with RouteInspect `65/65`, Route List
  application `7/7`, Hosting `5/5`, and pinned parser `12/12`; freshly published
  managed RouteInspect EndToEnd is `28/28`. Final correctness review passes and
  local-improvement review finds no material improvement. The Green packet is
  ready to commit before the one final complete acceptance suite.
- Presentation Green is committed at `51c0960` (`Implement route inspect
  presentation`). Final presentation acceptance verification is active from that
  exact production tree.
- Presentation final acceptance passes from the unchanged `51c0960` production
  tree: warning-free Release build, format, diff, Unit `549/549`, Integration
  `166/166`, fresh managed `win-x64` publish, and EndToEnd `36/36`, with no skip.
  Presentation is Complete. Integrated Route Inspect acceptance is Active for
  local Native AOT, promotion/architecture audits, edge dispositions, and Route
  Discovery closeout.
- Integrated acceptance native execution passes: the `win-x64` Native AOT root
  drives `36/36` managed public EndToEnd cases, the published native Integration
  runner passes `166/166`, and the published native EndToEnd runner passes
  `36/36`. Package/vulnerability and initial dependency/promotion audits are
  clean. The locality audit found three newly or materially changed state carriers
  outside their nearest `Models/` scopes, so acceptance has returned to one
  behavior-neutral locality correction with contracts and expectations frozen.
- The bounded locality correction is complete: the three state carriers now
  occupy their nearest topical `Models/` scopes with matching namespaces and no
  forwarding or duplicate type. Release build, format, focused Unit `71/71`, and
  focused Integration `52/52` pass; fresh correctness review passes; the local
  improvement review's stale imports are removed. The corrected final tree also
  republishes and executes the Native AOT root with public EndToEnd `36/36`, the
  native Integration runner `166/166`, and the native EndToEnd runner `36/36`.
  Integrated acceptance then resumed final matrix, promotion, edge, and closeout
  recording.
- The final contract matrix, C1-C6 source dispositions, eight-candidate promotion
  audit, architecture checks, exact verification commands, and CLI-EDGE-002
  through CLI-EDGE-007 dispositions are now recorded. Every executable gate passes
  or has its explicit bounded residual. Fresh final acceptance and writing review
  were active before lifecycle closeout.
- Final independent correctness and writing reviews pass after the candidate,
  edge-range, native-surface, command, and lifecycle-provenance corrections.
  Route Inspect and Route Discovery are Complete from production correction
  `9c690b4`. The `CLI-EDGE-001` routing/index waiver leaves completed records and
  generated regions in place without hand editing.
- Route Inspect is squash-integrated into `develop` at `bd5d280`; its tree exactly
  matches the accepted feature tip. Branch `feature/cli-generic-improvements`
  starts from that exact commit. The generic beginning baseline passes warning-free
  build, format, Unit `549/549`, Integration `166/166`, and freshly published
  managed EndToEnd `36/36`. Parser-remediation Preflight completed with Gray next.
- Parser-remediation planning is accepted at `e118daa`. Compile-only Gray adds one
  immutable option-result fact model and two exact throwing callable skeletons,
  with no existing production/test mutation or runtime behavior. Release build,
  format, diff, correctness review, and local-improvement review pass; Red is next.
- Parser-remediation Gray is accepted at `ddd2683`. Production-free Red adds eight
  affected test files and leaves Gray production unchanged. Release build, format,
  and diff pass. Focused Unit is `67` total with `52` pass and `15` intentional
  failures; focused Integration is `93` total with `52` pass and `41` intentional
  failures; a fresh managed publish drives `57` selected EndToEnd cases with `51`
  pass and `6` intentional failures. Failures are limited to Gray skeletons, current
  raw or custom-parser deviations, and terminal bypass. Final correctness review
  passes and local-improvement review finds no material improvement. Green is next.
- Initial Green exposed one contradictory Route List Unit expectation that required
  terminal-domain validation both in the integrated terminal resolver and through a
  second direct call. Green production work was isolated. A production-free Red
  correction now asserts the accepted single integrated validation path, preserves
  the remaining matrix, and consumes the one exceptional correction cycle from exact
  Red commit `a47f145`. Green resumes only after this correction is committed.
- Corrected Green review then found that removing the workspace parser callback also
  removed non-empty validation for a spaced explicit empty workspace token. Green
  production remains isolated while one focused Unit expectation supplements the
  same correction cycle. Corrected Red Unit is `68` total with `52` pass and `16`
  intentional failures; Green resumes only from the latest correction commit.
- Corrected Red is accepted at `68455f6`. Parser-remediation Green is complete from
  that exact predecessor in seven production files: public typed option facts,
  ZeroOrOne scalar options, semantic missing/empty validation, one integrated typed
  terminal-input validator, and typed Route List depth replace the removed callbacks
  and raw depth helper while retaining the one delimiter guard. Release build,
  format, and diff pass; focused Unit is `68/68`, Integration is `93/93`, and a fresh
  managed publish drives selected EndToEnd `57/57`, all without skips. Both exact
  public terminal/domain scenarios pass. Final correctness review passes and final
  local-improvement review finds no material improvement. The post-Green callable
  migration is next after the Green phase commit.
- Parser-remediation Green is accepted at `661b89b`. Its post-Green callable
  migration is complete from that exact predecessor: `CliBindingParse` now contains
  only `ParseResult` at its matching Models path, the old declaration/property and
  Route List raw parameter are absent, and all production/Unit call sites use the
  narrow model without forwarding compatibility. Release build, format, and diff
  pass; focused Unit is `68/68`, Integration is `93/93`, and fresh managed selected
  EndToEnd is `57/57`, all without skips. Correctness review passes; the only local-
  improvement finding was one stale test method name, now corrected, and final review
  finds no material improvement. Blue is next after the migration commit.
- The parser-remediation callable migration is accepted at `d7056e8`. Blue is complete
  from that exact predecessor: one named explicit-without-value fact replaces repeated
  predicates, and one immutable omitted-facts instance avoids redundant construction.
  Release build, format, and diff pass; focused Unit is `68/68`, Integration is
  `93/93`, and a fresh managed publish drives selected EndToEnd `57/57`, all without
  skips. Correctness review passes; final local-improvement review recommends keeping
  the bounded change and finds no further material improvement. Purple is next after
  the Blue commit.
- Parser-remediation Blue is accepted at `f9f4dbe`. Purple is complete from that exact
  predecessor: the published version journey now snapshots both the missing-workspace
  parent and the separate process current directory and proves both remain byte-
  unchanged. Release build, format, and diff pass; focused Unit is `68/68`, Integration
  is `93/93`, and a fresh managed publish drives selected EndToEnd `57/57`, all without
  skips. Correctness review passes with the existing snapshot helper's metadata and
  transient-write limitations explicitly bounded. Purple is accepted at `d445e20`.
- Parser remediation is Complete through Purple `d445e20`. Exact published terminal,
  ordinary post-terminator domain, and attached-empty depth scenarios pass. Final
  whole-Task review finds no production or evidence gap after correcting stale next-
  step text. CLI-EDGE-005 and CLI-EDGE-007 are closed for this Task. The one correction
  cycle is consumed, and the parent generic final complete managed, local `win-x64`
  Native AOT, package, and vulnerability gates remain deferred. Active-test architecture read-only
  Preflight then completed from exact accepted parser baseline `a14f66d`, leading to
  accepted Purple commit `e277227`.
- Active-test architecture is Complete at `e277227`. One exact shared Loader-
  document builder replaces six duplicate bodies; process support is EndToEnd-local;
  Integration and EndToEnd each own one nearest-scope capture fixture; and Unit's
  unused TestSupport reference is removed. Warning-free build, format/diff checks,
  focused Integration `129/129`, freshly published managed EndToEnd `57/57`, zero
  skips, source/dependency audits, and fresh correctness/local-improvement reviews
  pass. Shared workspace/hash snapshot support and distinct measurement/rich
  snapshots remain unchanged. The accepted active-test predecessor is the clean
  `eb2e336` tree. At the active-test completion boundary, the parent final full
  managed, local `win-x64` Native AOT, package, vulnerability, artifact, and
  integrated-review gate remained deferred until all generic children were
  complete. Callable/project architecture read-only Preflight then completed from
  that exact clean predecessor, followed by planning commit `e54f2b1` and accepted
  refinement `cc2387d`.
- Callable/project architecture is Complete at `cc2387d`. Four required named-input
  component models replace the accepted long composition surfaces; focused Unit
  `53/53`, Integration `49/49`, freshly published managed EndToEnd `57/57`, zero
  skips, warning-free build, format/diff checks, source/project audits, and fresh
  correctness/local-improvement review pass. Root/Core remains retained without
  project, IVT, solution, CI, publish-path, package, generated, or Native AOT policy
  mutation. All generic child Tasks are Complete; the parent Final Full Gate and
  integrated correctness review pass from clean `7871764` after exact `cc2387d`.
- Generic-improvements feature acceptance passes from clean `7871764`: warning-free
  build, format/diff checks, managed Unit `580/580`, Integration `213/213`, fresh
  EndToEnd `57/57`, native-root managed EndToEnd `57/57`, Native AOT Integration
  `213/213`, Native AOT EndToEnd `57/57`, zero skips, no vulnerable packages, clean
  project/artifact audits, exact native public scenarios, and fresh integrated
  correctness review. Local native evidence is explicitly `win-x64`, not six-RID
  parity. No material finding remains; the authorized squash-merge later completed
  at `063c59d`.
- Generic CLI Improvements are squash-integrated into `develop` at `063c59d`; the
  integrated tree exactly equals accepted feature tip `a107afe`. The parent Task is
  Complete. The reviewed Find planning packet is accepted at `b2e3106`; Child 1
  execution now follows its recorded Preflight.

## Historical Find Preflight (2026-08-23)

- Planning boundary: the reviewed 14-path Find planning packet is accepted at
  `b2e3106` (`Plan and freeze CLI find`) on clean branch `feature/cli-find`.
  `develop` remains `e77902a` (`Record generic CLI integration`). The initial
  production/source baseline is the exact `063c59d` (`Improve and accept generic
  CLI structure`); later Child 1 execution baselines remain distinct.
- The accepted parent boundary freezes one fresh neutral Framework source
  catalogue, one selected-layer strict-read boundary, neutral on-demand route
  facts without scope inference, one fixed Markdig `1.3.2` CommonMark pipeline,
  one location-preserving semantic frontmatter reader, and one deterministic
  read-only Find operation with the exact Interface schema, findings, statuses,
  and `next` values. Filters form the effective universe before selected-layer
  reads; no index, cache, network, or workspace mutation is allowed.
- Read-only explorers, correctness reviewers, and grounded adversarial advisors
  inspected current Route List, Route Inspect, Framework code, accepted filters,
  and affected evidence from exact `b2e3106`. They made no source or test edits.
  Their evidence closed the Child 1 gaps for roots, parse results, lookups,
  scope-derived filtered selection, physical revalidation and memoized logical-
  path rebinding, Loader stages, topology, missing Loader, aliases, overwrite
  divergence, profile reads, projection invariants, and graph mapping.
- The detailed Child 1 Gray/migration/evidence Preflight passed targeted review
  and is recorded at `7e081ef`. It freezes exact Framework paths and neutral names,
  issue and ordering invariants, selected-layer reads, route allowlists and
  missing-Loader facts, Route projections, migration boundaries, and the exact
  Unit/Integration evidence paths. The Preflight itself claimed no implementation
  or tests.
- Accepted Child 1 decisions include: contained aliases remain distinct logical
  candidates with ordered `PhysicalAlias` facts; identity collisions retain all
  sources; only exact adjacent overwrites pair; neutral pairing does not reproduce
  Inspect's legacy ambiguity; Inspect recreates multiple-candidate ambiguity
  locally with a base-only projection while non-adjacent single candidates remain
  orphans; selected route sources are a complete allowlist; and cancellation
  retains safe facts.
- Gray is compile-only and throws `NotSupportedException` at new behavior
  entrypoints. Temporary old-authority coexistence is allowed through Gray and
  production-free Red, with no dual runtime wiring or forwarding wrappers. Green
  migrates Route consumers and removes duplicate authorities. The full Child 1
  gate runs once after the final Child 1 change, including local `win-x64` Native
  AOT Route regressions. No Find command, six-RID parity, or Find-specific AOT
  claim belongs to Child 1.
- Ordered children remain: source catalogue and Route migration; query operation
  and document facts; presentation, registration, and final acceptance. No
  shared source mutation runs in parallel. References may begin only after the
  source/document facts it needs are accepted; Context waits for Find and
  References.
- Beginning evidence is the accepted generic gate: managed Unit `580/580`,
  Integration `213/213`, EndToEnd `57/57`, local `win-x64` Native AOT root with
  native-root managed EndToEnd `57/57`, Native AOT Integration `213/213`, and
  Native AOT EndToEnd `57/57`, zero skips,
  package/vulnerability/artifact/public audits, and exact accepted-tree equality.
  Find's inner loop is focused; its final gate repeats complete managed and local
  `win-x64` managed/native/public/no-write/package audits once after the final
  change.
- Routing condition: `CLI-EDGE-001` prevents an authoritative legacy
  `open-forge-old index` refresh. The generated block remains unchanged; explicit
  parent-before-children navigation links sit outside it. No manual generated
  edit or routing-tool run is claimed.
- Review result at the accepted planning boundary: targeted correctness and
  writing reviews pass with no findings. Child 1's initial correctness and
  adversarial lenses required corrections for root semantics, filtered candidate
  evidence, physical revalidation, overwrite/profile adapters, Loader/topology
  support, phase coexistence, and exact test commands. Fresh correctness review
  passes after correcting scope-derived issue membership, Native AOT EndToEnd,
  route-issue ordering, Inspect's early terminal path, typed projection
  association, test disposition, and overwrite metadata/adjacency invariants. At
  the recorded Preflight boundary, no production, test, project, package, or
  generated source had changed for Child 1.
- Child 1 Preflight is recorded at `7e081ef`. Compile-only Gray is complete in 39
  new production files. It freezes neutral source Identity, Inventory, Reading,
  and Routing contracts plus Route projection/List/Inspect adapter contracts;
  every behavior entrypoint throws `NotSupportedException`. Existing runtime
  wiring and old authorities are unchanged, and no tests or project/package files
  changed. Release build passes with zero warnings/errors; format and diff checks
  pass. Correctness review passes after projection association corrections. The
  only improvement recommendation, centralizing repeated path-shape checks, is
  deferred until Green because Gray's identity callables intentionally throw.
- The maintainer accepted two C# design directions after analysis. They apply now
  to new or materially changed C#: use truthful nullable contracts and compiler
  flow for trusted internal values while retaining runtime guards at uncertain
  ingress; and choose constructors, factories, object initializers, primary
  constructors, target-typed creation, and collection expressions by invariant
  and semantic fitness rather than novelty.
  The binding [C# callable design Directive](../../../directives/csharp/design.md)
  records the rules and evidence. The planned [Modern C# Improvements Task](../cli-development/tasks/modern-csharp-improvements.md)
  migrates existing source solution-wide after Child 1 acceptance without
  widening current Red/Green.
- Child 1 production-free Red is complete and reviewed in the commit containing
  this checkpoint update. Gray production remains unchanged from `7b34cc7`.
  Release build and format pass with zero warnings/errors; Unit is `562` total,
  `403` passing, and `159` intentional Gray failures; Integration is `183` total,
  `155` passing, and `28` intentional Gray failures; both projects have zero
  skips. Existing Route List and Inspect regressions remain green. Correctness
  review passes after fixture, selected-candidate, alias-identity, unsafe-path,
  compatibility-predicate, and Inspect BaseOnly corrections. Local improvement
  review has only optional Purple fixture-consolidation ideas.
- Red records two real-filesystem evidence limits. Physical-resolution
  `Unavailable` has exact model evidence and a separate verified-then-read locked
  file case, but no safe portable physical-resolution fixture. Catalogue
  pre-cancellation is direct, but deterministic accumulated mid-traversal
  cancellation would require a race or an artificial seam. Green and final review
  must preserve and report these limits.
- Initial Green made the neutral focused packet pass but exposed a Gray contract
  gap: active Route policy evidence still requires obsolete catalogue, identity,
  Loader, and topology authorities, and the initial Gray surface cannot accept
  final neutral command inputs. The one correction cycle returned to Gray rather
  than changing frozen tests in Green, retaining wrappers, or deferring obsolete
  production to Purple.
- Corrected Gray is complete in the commit containing this checkpoint update. It
  adds five compile-only neutral migration seams across Route List inventory,
  selection, topology, and the Route Inspect graph. Every new entrypoint throws a
  named `NotSupportedException`; old runtime wiring remains unchanged. Release
  build, format, diff, correctness review, and local improvement review pass after
  removing one overload ambiguity. Corrected Red must now migrate every active
  test that references an authority corrected Green will delete.
- Corrected Gray is committed at `6a9b143`. Corrected Red is complete in the
  commit containing this checkpoint update. It migrates the exercised neutral
  identity, reference, Loader, catalogue, and topology call paths while retaining
  command policy through final neutral seams. Its review recorded the obsolete-
  reference audit as clear, but the corrected Green deletion audit later
  disproved that broad conclusion for the Route projection form, Route List path
  wrapper, and one-argument inventory entrypoint. Release build and format pass
  with zero warnings/errors. Unit is `562` total with `251` passing and `311`
  intentional Gray failures. Integration is `184` total with `79` passing and
  `105` intentional Gray failures. Both report zero skips. Production remains
  unchanged from corrected Gray.
- Corrected Red correctness review passes after empty-Loader completeness,
  duplicate-entrypoint policy, projection ownership, and neutral trait
  corrections. Local improvement review defers only Purple consolidation of
  large boundary-specific fixture builders. Initial Green remains isolated and
  may be restored only after corrected Red commits.
- Pre-refinement Green result: Corrected Red is committed at `70cde33`. Restored
  corrected Green substantially implements neutral Framework behavior and
  migrates active Route List, Route Inspect resolution, profile, and result
  composition to one invocation-scoped reader, neutral catalogue/projections, and
  neutral route facts. That unaccepted
  production worktree passed a warning-free Release build. Its last
  complete focused Unit run is `562/562`; the last complete focused Integration
  run was `181/184`. Two neutral Loader external-boundary mappings were corrected
  afterward and their exact targeted rerun passes `2/2`; no complete post-fix
  Integration result is claimed.
- Final legacy-authority deletion exposed a corrected-Red contradiction. Active
  frozen tests still compile against `RouteSourceForm`, `RouteListLogicalPath`,
  and the one-argument Route List inventory entrypoint, while the accepted Child
  1 boundary requires neutral `SourceDocumentForm`, deletion of Route-prefixed
  duplicate identity/form authorities and every legacy overload, no forwarding
  wrappers, and no Green test edits. Removing the production surfaces breaks the
  frozen test build; retaining them violates the accepted Green boundary. The
  attempted production-only form migration was reverted immediately, and the
  worktree builds again without changing tests.
- The one exceptional Child 1 correction cycle is already consumed. Child 1
  returned to the Find parent, and the maintainer's continuation direction is now
  adopted as a bounded parent refinement. The unaccepted Green worktree is
  isolated while one combined behavior-neutral production/evidence correction
  replaces `RouteSourceForm` with `SourceDocumentForm`, replaces active-test
  `RouteListLogicalPath` use with `SourceLogicalPath`, and moves the three Route
  List inventory boundary calls to the final reader seam. Compatibility APIs are
  rejected. Modern C# improvements and Find Children 2 and 3 remain blocked until
  Child 1 acceptance.
- The bounded parent refinement is implemented and reviewed in the current
  worktree and ready to commit. It changes exactly 27 production and active-test
  paths, removes `RouteSourceForm` and its classifier, moves consumers to neutral
  `SourceDocumentForm`, implements neutral `SourceLogicalPath` and
  `SourceFormClassifier` to preserve existing Route construction, moves the two
  retained inventory boundary cases to the final reader seam, and removes the
  predecessor-only timing cancellation test. Build is warning-free; format and
  diff checks pass. Focused Unit is `562` total with `295` passing and `267`
  intentional Gray failures. Focused Integration is `183` total with `76` passing
  and `107` intentional Gray failures. Both have zero skips, and every failed block
  reaches a named Gray `NotSupportedException`. Symbol audits and fresh correctness
  review pass; local-improvement review finds no material change.

## Current Step

The Modern C# Improvements Task is Complete and accepted at exact `a1cbf09`.
Find Child 2 is Complete and accepted at exact `ff7ce3f`. Find Child 3 and the
Find parent are Complete and accepted in the commit containing this record update.
The accepted Child 3 history is Preflight `28d316a`, Gray `a76a217`, original Red
`22d3bff`, metadata corrections `6a9a0de` and `eea3d59`, supplemental escaping Red
`a865fd1`, Green `cb7874c`, Blue `3f81e76`, and no-op Purple `426d4f5`. The final
managed/native gate and package, artifact, static, public no-write, and
protected-surface audits passed. The sole immediate continuation is to freshly
verify `develop`, squash-integrate the accepted `feature/cli-find` tip into local
`develop`, commit that one squash, prove exact tree equality, do not push, and halt.
References remains Planned and must not start in this session. `CLI-EDGE-001`
remains non-product only.

## Route-List Closeout Decision

The maintainer's closeout/waiver decision authorized commit/squash integration
after the local route-list evidence. The accepted state is integrated by squash
commit `edca509` (`Establish and accept route list`). The two legacy-router errors are routed to [CLI-EDGE-001 — Legacy
routing-tool duplicate-entrypoint reports](../cli-development/edge-cases.md#cli-edge-001--legacy-routing-tool-duplicate-entrypoint-reports)
and are not route-list blockers. Exact merged-tree equality and the route-list
evidence above are recorded for the current route-inspect baseline.

## Accepted Decisions

- All replacement-specific C# material is scoped below `src/cli/`.
- Every new route-inspect record, interface, and property-only class, and every
  existing model materially changed or promoted by this slice, belongs under its
  nearest topical `Models/` path. Untouched accepted CLI types are outside this
  migration.
- The initial physical boundaries are `root/`, `core/`, and `tests/`.
- The project graph contains one executable, one Core library, three runnable test
  projects, and one test-support library.
- The Mastermind authors architecture, cross-cutting callable contracts, and the
  actual route-free foundation.
- Smaller implementers receive only closed Tasks after the relevant foundation is
  accepted.
- Tests remain under `src/cli/tests/`; preserved tests are candidate evidence,
  not architecture authority.

## Blockers

- No decision blocker remains for Find. Child 3 and the Find parent are Complete
  and accepted in the commit containing this record update. The final
  managed/native gate and package, artifact, static, public no-write, and
  protected-surface audits passed.
- The Read-Only group and broader CLI program remain Active. References remains
  Planned and must not start in this session. The only immediate continuation is
  the local `develop` verification, one squash integration of the accepted
  `feature/cli-find` tip, exact tree-equality proof, no push, and halt.
- `open-forge-old index` remains unable to provide an authoritative routing refresh
  because of the duplicate `_index.md` `CLI-EDGE-001` report. `CLI-EDGE-001`
  remains non-product only; it is not a product blocker or a Child 3 decision.

## Resume

Read:

1. [CLI Find Accepted Handoff](../handoffs/2026-08-25_cli-find-accepted.md)
2. [CLI Architecture](../../crystallized/documents/cli/architecture.md)
3. [Find Contract Set](../../crystallized/documents/cli/contracts/find/_find.md)
4. [Find Task](../cli-development/tasks/read-only/find.md)
5. [Find Source Catalogue](../cli-development/tasks/read-only/find-source-catalogue.md)
6. [Find Query Operation](../cli-development/tasks/read-only/find-query-operation.md)
7. [Find Presentation And Acceptance](../cli-development/tasks/read-only/find-presentation-acceptance.md)
8. [CLI Development Plan](../cli-development/plan.md)
9. [Program Architecture Directive](../../../directives/program-architecture.md)
10. [CLI Implementation Directive](../../../directives/open-forge/cli/implementation.md)
11. [Test Evidence Integrity](../../../directives/open-forge/testing/evidence-integrity.md)
12. [Evidence Tiers](../../../patterns/testing/evidence-tiers.md)

After the containing acceptance commit, freshly verify `develop`, squash-integrate
the accepted `feature/cli-find` tip into local `develop`, commit that one squash,
prove exact tree equality, do not push, and halt. Do not start References in this
session. The Find parent, Read-Only Commands group, and program remain Active, and
`CLI-EDGE-001` remains non-product only.
