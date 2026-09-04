---
open-forge:
  description: Executable top-down work graph for completing the greenfield replacement CLI
  tags: [Memory, Working, CLI, Plan, Architecture, Development, Contextual, Active]
---

# Replacement CLI Development Plan

## Task And Planning Boundary

- Task: [Complete The Replacement CLI](tasks/00-cli-development.md).
- Plan state: Active.
- Planning authority: The maintainer accepts consequential decisions. The
  Overseer owns architecture, sequencing, Task decomposition, integration, and
  Plan maintenance within that direction.
- Last updated: 2026-09-03.
- I1 proportional prerequisite: committed at exact `b25d76e`. I1 is Complete in
  exact feature candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`.
  Operation orchestration is committed at `125b6a2a`; public composition and
  presentation are committed at `4e89d945`. Final closeout tip
  `2b353c48978ee88e53345be8037776181612222c` is locally squash-integrated at
  `09aa03eddb97831ff544afe1eac54ad9af501f5c`; both commits have exact tree
  `2dcfca18020980a9cafbc429a72930af3368df5f`.
- Task 4 “Route Move” is Complete at phase 6/6, milestone 12/12. Accepted
  closeout `631983ea1ec7d7ad5f5fc3999f938ba5f445ed81`, tree
  `d6f6fdf7d1caf62a9ac68582609c7282921f557c`, is squash-integrated at
  `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
  `6959b51e148af44d59512d8bdd801350d88fc651`, from local `develop` parent
  `83902c88`, tree `4b8324b2`.
- Task 12 “CLI Architecture Authority Remediation” is Complete at phase 5/5,
  milestone 6/6. Corrected source
  `a0e6bc8dc25ae9395beae98289f594e1f9f56af2`, exact tree
  `9c4a33b1c16617cf79beefd9f16a6e1d2d551382`, is squash-integrated at
  `495a7ed6b55bca2a879ece83818f89e530c33af2`, the same exact tree, from
  `develop` parent `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
  `6959b51e148af44d59512d8bdd801350d88fc651`. Task 14 is active at phase 4/5,
  milestone 4/8. Immutable Green
  `9a6ae2fa509d7bf1268f6012650e41655d3c27fe`, tree
  `c39cfcbec1ea532ee27060020c681caa9b306245`, follows the clean continuity
  base and accepted Gray/Red boundaries. Four fresh topic reviews and wider
  managed, Native AOT, and package gates are active under its Task Mastermind.
  Task 15 and Task 7 have separate read-only prerequisite audits; neither is
  authorized to implement before Task 14 acceptance.
- Accepted Route Update integration input baseline: local `develop` commit
  `996c2e17d1142ffb30dc7a2d17df657419566f97`, exact tree
  `877314d48db49013edc1c4dad1abb545b37caefc`. It integrates CLI
  Quality Remediation and closeout at `862cbf2a` and `5053bf0c`, bounded review
  orchestration at `5aad04ac`, permanent task identity and dynamic progress at
  `3356eba1`, repository-local npm linking for the managed development CLI at
  `128b70b3`, and the streamlined Task 4–6 trial. The Route Create command
  integration is `19412d2`. Route Init
  is squash-integrated at `cc5085ce` from
  reviewed closeout `c5801494ac6426add2c64e32cafbba6f0162561a` with exact tree
  equality. Its executable evidence candidate remains
  `cb62b19f73afcace163371af9093d877821fa800`, exact tree
  `be93900d0dc102fcf2d5a351651c0b0134de39a0`.
  The integration baseline includes the accepted
  intended-source formation integration after public Index, integrated D0/SF1-SF4
  foundations, M2 preparation, Route Inspect interaction correction, and the
  protected Extension Create and root Install public integrations. Task 3
  “Route Update” is Complete at phase 7/7, milestone 12/12, from accepted task
  base `5aad04a`, tree `0f56b2c`, through closeout `27df8325`, tree `b68d4349`.
  The commit containing this record squash-integrates that accepted delta after
  reconciling its five continuity overlaps. The baseline also
  includes canonical lifecycle creation at `1d404c5` and the neutral Markdown
  link-label projection at `89a35a7`. The preceding C2 baseline was
  `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. The preceding C1 baseline was
  `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, exact tree
  `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. The preceding C3 baseline was
  `fa3db1ee1dbfb687715b5b90b35f6104cbc45c6c`, exact tree
  `605620d990622e093b12e85e50c6e40083896a18`. The shared-foundation baseline was
  `33913dfe7f8f80598ca4765c516d308ed179c3ab`,
  exact tree `a56f3c201013b5999841414e1df469713f08cdfe`. The pre-foundation baseline was
  `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
  `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. Its Release
  build passes with 0 warnings and 0 errors. Managed Unit `1227/1227`,
  Integration `481/481`, and EndToEnd `125/125` pass. Portable `linux-x64`
  Native AOT Integration `481/481` and EndToEnd `125/125` pass. Every accepted
  final test run has zero failures and zero skips. Real-workspace projection
  proves zero writes, and independent Sol/xhigh review is `ROBUST` with 99%
  confidence. Context and
  Extension List are Complete and squash-integrated at `ca097a2` and `db0d39a`.
  Extension Inspect's exact public contract is
  squash-integrated at `92313a0`; accepted rebased feature `2b1e63d` is
  squash-integrated at `73b01be`. Routed Authored Metadata is Complete and squash-integrated at
  `5924698`. Generated Navigation is Complete and squash-integrated at `21e5200`;
  its full managed Unit `1030/1030`, Integration `357/357`, and local `linux-x64`
  Native AOT Integration `357/357` gates pass. The last complete public baseline
  passes managed Unit `978/978`, Integration `354/354`, EndToEnd `111/111`,
  Native AOT Integration `354/354`, and Native AOT EndToEnd `111/111`, all with
  zero skips. The earlier combined read-only baseline passed managed Unit `1054/1054`,
  Integration `378/378`, EndToEnd `116/116`, and `linux-x64` Native AOT
  Integration `378/378` and EndToEnd `116/116`.
  The routed-metadata acceptance gate separately passes full managed Unit
  `1012/1012`, Integration `354/354`, and Native AOT Integration `354/354`.
  [Improve The
  Repository-Root CLI Developer Workflow](tasks/repository-root-developer-workflow.md)
  is Complete and its accepted changes are included in that baseline. Read-Only
  CLI Dogfooding Corrections are squash-integrated at `bba84b6`, with managed
  Unit `1031/1031`, Integration `411/411`, and EndToEnd `120/120`, plus supported
  `linux-x64` Native AOT Integration `411/411` and EndToEnd `120/120`.
- [Correct Proportional CLI Findings](tasks/proportional-cli-corrections.md) is
  Complete. Proportionate guidance is integrated at `5f9f59e`, Route List
  corrections at `2cd525d`, and authoritative Markdown generated-region
  corrections at `0d88606`. The integrated Release build, managed
  `1053/411/120`, and portable `linux-x64` Native AOT `411/120` gates pass with
  zero skips; final Sol/xhigh review is `ROBUST`.
- Current result: the accepted Generated Navigation formation expansion adds
  `Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
  while preserving exact current-state `Build(SourceCatalogue)` behavior. It
  separates observed catalogue evidence from intended membership and derived
  topology facts without creating a prospective catalogue/source framework,
  virtual filesystem, temporary checkout, or hidden Index.
  [Mutation Foundation](tasks/mutation-foundation/_mutation-foundation.md)
  is Complete at exact production candidate `e7d937f` under authority
  `01dd552`. Its final managed, portable `linux-x64` Native AOT, static-absence,
  format, diff, and independent-review gates pass. Public
  [Index](tasks/read-only/index-command.md) is Complete in exact feature
  candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`. Its warning-free Release
  build, managed Unit `1206/1206`, Integration `480/480`, EndToEnd `125/125`,
  portable `linux-x64` Native AOT Integration `480/480`, EndToEnd `125/125`,
  and root publish/version smoke pass with zero failures or skips. Final closeout
  tip `2b353c48978ee88e53345be8037776181612222c` is locally integrated at
  `09aa03eddb97831ff544afe1eac54ad9af501f5c` with exact tree equality.
  [Test Architecture And
  Constants](tasks/test-architecture-and-constants.md) is Complete and its exact
  accepted tree is squash-integrated at `b6ce31f`.
  [Extension Inspect](tasks/read-only/extension-inspect.md) is
  Complete and integrated at `73b01be`. The [pure Generated Navigation
  foundation](tasks/read-only/index-generated-navigation-foundation.md) is
  Complete and integrated at `21e5200`.
  [Routed Authored Metadata
  foundation](tasks/read-only/routed-authored-metadata-foundation.md) is Complete
  and integrated at `5924698`.
- Current step: D0 and the four next-wave shared foundations are integrated and
  accepted. D0 is integrated at `38e1498`; SF1 native interaction at `e782090`,
  SF2 embedded Framework distribution at `680915a`, SF3 lifecycle source-asset
  provenance at `0989356`, and SF4 lease-bound directory creation at `33913df`.
  The combined reviewed baseline has Release `0` warnings and `0` errors,
  managed Unit `1284/1284`, Integration `500/500`, EndToEnd `125/125`, Native
  AOT Integration `500/500`, Native AOT EndToEnd `125/125`, and zero skips.
  C1 Extension Create is Complete: command-local squash
  `3ef81227ba50fba869f0129b958eabc6d0c29fbc` and protected public integration
  squash `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5` close exact final candidate
  `789cc917f2d0cb38c5229cc2dc7fee013218d341` at tree
  `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. Its final managed Unit
  `1356/1356`, Integration `575/575`, EndToEnd `132/132`, and `linux-x64`
  Native AOT root version/ELF, Integration `575/575`, and EndToEnd `132/132`
  evidence pass with zero failures and zero skips. Native dogfood makes no
  writes; final independent Sol/xhigh review is `PASS — ROBUST` at `0.98`
  confidence, with direct PTY process proof deferred. C2 root Install is Complete:
  final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56` is squash-integrated
  at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, with exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. Its final Release build has `0`
  warnings and `0` errors; managed Unit `1390/1390`, Integration `598/598`, and
  EndToEnd `136/136` pass; `linux-x64` Native AOT Integration `598/598` and
  EndToEnd `136/136` pass; and two independent Sol/xhigh rechecks pass. Native
  dry-run dogfood safely blocks on the repository's existing generated-region
  state without effects, workspace changes, lifecycle publication, or a new
  external lock. C3 Route Inspect interaction is
  squash-integrated at `fa3db1ee` with exact tree equality to final reviewed
  candidate `37c9360`. C3's Root-owned terminal composition and process seams
  are closed; C2's root composition, serialization, help, process, and Native AOT
  seams are also closed. Its exact fully present ordered public Install JSON
  result is accepted and frozen, including typed residual values `none`,
  `retained`, and `unknown`. Generic and Framework-aware Route Init is Complete
  and squash-integrated at `cc5085ce`. Route Create is Complete and
  squash-integrated at `19412d2`, exact tree `2bbba7e`; it supplies the
  `RouteCreateJsonContext` and Route-help predecessor slices. The separate
  [CLI Quality Remediation](tasks/cli-quality-remediation.md)
  Task is Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`,
  from accepted implementation candidate `a4ccf19a`, tree `97254e65`. Task 3
  “Route Update” is Complete at accepted closeout `27df8325`, tree `b68d4349`.
  Route Move is Complete at accepted closeout `631983ea`, tree `d6f6fdf7`.
  Task 12 is Complete and squash-integrated at `495a7ed6`, exact source tree
  `9c4a33b1`. Route Remove remains Planned after the current adoption-slice
  priority. The complete Route Mutation M2 lane still precedes root Update M3.
  New lifecycle documents now emit all five ordered root keys. Framework-created
  documents use complete empty Extensions, and existing incomplete files remain
  untrusted. The integrated Markdown parser now exposes AST-only `Supported` or
  `Unsupported` link labels;
  its Release `0/0`, parser `31/31`, Unit `1410/1410`, Integration `614/614`, and
  independent `ROBUST PASS` review gates pass. Route Remove itself remains
  Planned after the integrated Task 12 boundary and the accepted adoption
  slice.
  Independent scope/contract/ownership discovery, callable-surface analysis,
  Gray/Red readiness, and worktree preparation may overlap earlier work;
  dependent command behavior may not. The localized C1/C2/C3 interaction,
  defaults, catalogue, and directory-effect decisions are frozen below;
  Extension Create's and root Install's exact command-local JSON results are
  frozen. Implementation may not silently broaden either boundary.
  Continue operations, delivery, and final acceptance in the persisted order
  after their accepted predecessors.
  The snapshot slice remains a no-op, with possible LithSnap-backed
  presentation candidates recorded only as a deferred Idea.
  The maintainer accepted that dependency correction; public Index must consume
  the shared accepted mechanics and must not create a local substitute.
  Standard SDK publication restored the exact portable
  `Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` pack without a
  project workaround. The explicit `linux-x64` build passed with zero warnings
  and errors; build-selected References EndToEnd passed `12/12`; Native AOT
  Integration passed `308/308`; and Native AOT EndToEnd passed `82/82`, all with
  zero skips. Root tooling, automatic managed `open-forge-dev` publication,
  build-selected native EndToEnd discovery, temporary compatibility-name routing,
  WSL portability, and obsolete preserved-test removal remain implemented.

### Next-Wave Foundation Integration

D0's accepted contract freeze is integrated at `38e1498`. The reviewed shared
foundations are integrated at `e782090` (SF1 native interaction), `680915a`
(SF2 embedded Framework payload), `0989356` (SF3 lifecycle source-asset
provenance), and `33913df` (SF4 directory mutation). The combined reviewed
baseline has a Release build with `0` warnings and `0` errors; managed Unit
`1284/1284`, Integration `500/500`, and EndToEnd `125/125`; Native AOT
Integration `500/500` and EndToEnd `125/125`; and zero skips in every stated
run.

The accepted lock-location correction replaces the workspace-contained lock and
bootstrap result with one persistent external zero-byte ordinary file under
`LocalApplicationData/OpenForge/locks/v1`. Its filename combines a bounded
display-only workspace name with the authoritative full SHA-256 key of the
normalized physical workspace path. Missing `.agents` is an ordinary
lease-bound directory-create effect.

C1 Extension Create is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with exact final candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341` and tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. Its final managed Unit
`1356/1356`, Integration `575/575`, EndToEnd `132/132`, and `linux-x64`
Native AOT root version/ELF, Integration `575/575`, and EndToEnd `132/132`
evidence pass with zero failures and zero skips. Native dogfood makes no writes;
the final independent Sol/xhigh review is `PASS — ROBUST` at `0.98` confidence,
with direct PTY process proof deferred. C2 root Install is Complete at local
integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`, from final reviewed candidate
`11994e4d21ddc807b7480afc39ae3612e5a69a56`. Its final managed
`1390/598/136`, portable `linux-x64` Native AOT `598/136`, focused post-rebase,
review, and safe-blocking dry-run dogfood evidence pass within their stated
boundaries. C3 Route Inspect interaction is squash-integrated at `fa3db1ee` with
exact tree equality to final reviewed candidate `37c9360`. C2's exact public
Install JSON result schema is accepted and frozen, including typed residual
values `none`, `retained`, and `unknown`. Route Init is Complete at `cc5085ce`;
Route Create is Complete at `19412d2`, exact tree `2bbba7e`, and supplies the
`RouteCreateJsonContext` and Route-help predecessor slices. The separate CLI
Quality Remediation Task is Complete and squash-integrated at `862cbf2a`, exact
tree `571f104f`, from accepted implementation candidate `a4ccf19a`, tree
`97254e65`. Task 3 “Route Update” is Complete at accepted closeout `27df8325`,
tree `b68d4349`.
M3 root Update remains after the complete M2 Route Mutation lane.

- Previous accepted Find history: Find Child 2 original Preflight through Blue history is accepted
  through exact commit `685e2dd`. During original Purple, top-down review found
  generic YAML event parsing inside Find and duplicate Markdown-frontmatter
  extraction in Route. The Mastermind established the shared
  `Framework/Documents/{Markdown,Yaml}` foundation through a bounded
  Gray → Red → Green → Blue correction. Gray is accepted at exact `2cae7a4`, Red
  at exact `a673ebe`, and Green at exact `d4701ad`. Correction Blue is accepted at
  exact `0006915`, and corrected Purple at exact `2d10474`. Final review found one
  original Green authored-tag mismatch; supplemental Red is accepted at exact
  `c72dd5e`, its inherited Unit expectation correction at exact `d494adb`, and its
  Integration expectation correction at exact `23fe39a`. Corrected Green is
  accepted at exact `2337d62`; Child 2 final evidence and acceptance are recorded
  at exact `ff7ce3f` (`Accept Find query operation`). Child 3 focused Preflight is
  accepted at exact `28d316a` (`Freeze Find presentation preflight`), and Gray is
  accepted at exact `a76a217` (`Establish Find presentation contracts`). The
  complete original Red packet is accepted at exact `22d3bff`. Its historical
  Unit evidence was `202` total with `92` pass and `110` intentional failures;
  Integration was `43` total with `23` pass and `20` intentional failures; and
  published EndToEnd was `13` total with `13` intentional failures, all with
  zero skips. The Integration metadata correction is accepted at exact `6a9a0de`;
  the mirrored EndToEnd metadata correction is accepted at exact `eea3d59`
  (`Complete Find presentation metadata evidence`). Its post-commit Red
  reproduction succeeded as intentional Red: managed non-AOT `win-x64` publish
  passed, and published Find EndToEnd was `13` total with `13` intentional
  failures and zero skips, all terminating at absent Green root registration.
  Historical pre-correction review found the independent human `\t` expectation
  contrary to the frozen lowercase `\uXXXX` control contract and found bounded
  diagnostics slicing escaped code units, which allowed partial `\\`, `\"`, or
  `\uXXXX` tokens. That review returned the work narrowly to Red without changing
  contracts, Child 2, Integration, EndToEnd, production, package/project,
  generated routing, or Route behavior. Supplemental escaping Red correction is
  accepted at exact `a865fd1` (`Correct Find escaping evidence`); it changes only
  `FindHumanRenderingRedTests.cs` and `FindDiagnosticsAndHelpRedTests.cs`, sets
  TAB to `\u0009`, and adds four Windows renderer-level boundary cases for
  complete escape-token grammar, bounds, one-line output, and no payload leak.
  Its clean detached-worktree evidence includes locked restore, warning-free
  Release build, format verification, full focused Unit `206` total with `92`
  pass and `114` intentional Gray-boundary failures, and narrow selected evidence
  `6` total with `6` intentional failures, all with zero skips. Fresh test-only
  correctness review at that Red boundary is `PASS`; no material optional
  improvement remains. The pre-correction narrow Green selection for TAB plus
  four token-boundary cases was `6` total with `1` pass and `5` intended
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

  Fresh Green evidence passes locked restore, a warning-free Release solution
  build, format verification, and `git diff --check`; focused Find+direct-root
  Unit `206/206`; focused Find+generated-serialization Integration `43/43`;
  affected Shell+Route Unit `347/347`; affected Shell+Route Integration `160/160`;
  managed non-AOT `win-x64` publish; and published Find EndToEnd `13/13`, all with
  zero skips. Fresh final bounded Green correctness review is `PASS` with no
  material findings. It verifies corrected escaping, Find binding, explicit
  malformed-content state, root leaf registration, renderer dispatch, JSON
  projection/source generation, diagnostics, help, direct Shell integration, one
  operation/result flow, and no protected-surface drift. Native AOT is intentionally
  not claimed.

  Earlier local-improvement review found one material bounded Blue candidate only:
  in `FindJsonProjection`, replace duplicate finite `Status` and `FindingCode`
  switches with canonical `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`. Its separate escaping correctness
  finding was resolved in Green; it is not a Green defect. Blue applied that
  candidate and is accepted at exact `3f81e76` (`Simplify Find JSON projection`). It changed
  production structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it replaced the duplicate local switches with the canonical readers, then
  removed the two duplicate private mapping methods. No behavior, public
  output/order/schema, test/support, contract, package/project/configuration,
  generated routing, Shell/root, Route, workspace-write, or Native AOT change
  occurred. Blue evidence is a warning-free Release solution build, format
  verification, `git diff --check`, focused Find+direct-root Unit `206/206`, and
  focused Find+generated-serialization Integration `43/43`, all with zero skips.
  No managed republish or Native AOT claim is needed for this one-file
  behavior-preserving Blue. Fresh bounded Blue correctness review is `PASS`: all
  7 status and 17 finding-code mappings and undefined-value exception behavior are
  exact; JSON model/property order/context is unchanged; static canonical readers
  remain source-generation/AOT-safe. Fresh local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`; the one-file simplification is complete,
  removing duplicate mapping ownership without adding indirection.

  Fresh Purple assessment ran read-only from exact clean Blue `3f81e76` against the
  exact ten Find Child 3 test/support surfaces. The no-op Purple acceptance is
  recorded at exact `426d4f5` with verdict `NO_MATERIAL_IMPROVEMENTS`. No
  test/support, production, contract, project, package, configuration, generated,
  Route, Shell, or root file changed, and no Purple code/test commit was
  manufactured. Its focused Unit `206/206`, focused Integration/serialization
  `43/43`, and published Find EndToEnd `13/13` evidence passed with zero skips;
  source diff/check against `3f81e76` was clean/empty. No Native AOT claim was made,
  and the record-only commit was not a test change.

  Find Child 3 and the Find parent completed the recorded acceptance boundary.
  Their accepted feature tree was later squash-integrated into local `develop` at
  `1f03d16`; that completed continuation no longer controls current work.

- Generic predecessor: Generic CLI Improvements are Complete and squash-integrated into `develop` at `063c59d`, with exact tree equality to accepted feature tip `a107afe`. Their final gate passes managed Unit `580/580`, Integration `213/213`, and EndToEnd `57/57`; local `win-x64` Native AOT root with managed EndToEnd `57/57`; Native AOT Integration `213/213`; Native AOT EndToEnd `57/57`; package/artifact/public audits; and integrated review.
- Route Inspect/family modernization: Complete and accepted at exact commit
  `62a1dd9` (`Modernize Route Inspect nullable flow`). It changed exactly 32
  production C# files in
  `Commands/Route/Shared/**` and `Commands/Route/Inspect/**`; exact
  `RouteBinding.cs` and `RouteDefinitions.cs` were in scope and unchanged. No
  tests, projects, packages, dependencies, generated files, or configuration
  changed. Scoped directory-pathspec audits are ordinary postfix null suppressions
  `50 → 0` and `ArgumentNullException.ThrowIfNull` `113 → 83` (30 removed).
  Global authored production counts after Framework, Shell/root, and Route
  Inspect/family are `26` suppressions and `327` `ThrowIfNull` calls. Final managed
  verification and review are recorded in the Modern C# Improvements Task; this
  batch makes no Native AOT claim.
- Route List modernization: Complete and accepted at exact commit `273eb45`
  (`Modernize Route List nullable flow`) from exact predecessor `62a1dd9`
  (`Modernize Route Inspect nullable flow`).
  The exact scope is `Commands/Route/List/**`; exactly 18 production C# files
  changed. No tests, projects, packages, dependencies, generated files,
  configuration, or product contracts changed. No output or test expectation
  changed. Scoped ordinary postfix null suppressions are `26 → 0` and
  `ArgumentNullException.ThrowIfNull` is `136 → 106` (30 trusted internal
  duplicates removed). Global authored production is now `0` ordinary postfix null
  suppressions and `297` `ArgumentNullException.ThrowIfNull` calls after all four
  production batches.
  Route List Unit is `117/117`, Integration is `89/89`, and managed published
  `CliProcessTests` EndToEnd is `26/26`, with zero skips. This batch makes no
  Native AOT claim. The final managed, Native AOT, package, audit, and no-write
  gates follow the accepted tests/support mutation batch.
- Tests/support modernization: Complete and accepted at exact clean source commit
  `6af5fb1` (`Modernize test support nullable flow`) from exact predecessor `273eb45`
  (`Modernize Route List nullable flow`).
  Exactly 40 active test/support C# files changed: Unit 24, Integration 14,
  EndToEnd 1, and TestSupport 1. Production, project, package, dependency,
  generated, configuration, and product-contract surfaces, plus test identities,
  expectations, tiers, order, fixtures, and count, are unchanged. Test postfix
  suppressions are `169 → 8`, test `ArgumentNullException.ThrowIfNull` calls fell
  from `21` to `8`,
  and production remains at `0` suppressions and `297` guards. Full managed Unit
  `617/617`, Integration `241/241`, and EndToEnd `57/57` pass with zero skips.
  This batch makes no Native AOT claim; the final gate is recorded below.
- Route-inspect baseline and sequence: exact `edca509` (`Establish and accept route list`) on `feature/cli-route-inspect`; planning commit `37d2e70`; contracts/evidence (Complete) → resolution/promotion (Complete at `a54f4e0`) → profile (Complete at `c407e24`) → presentation (Complete from production commit `51c0960`) → behavior-neutral locality correction (Complete at `9c690b4`) → integrated acceptance and Route Discovery closeout (Complete).
- Maintainer-authorized continuation, completed through generic integration: Route Inspect and Generic CLI Improvements were squash-integrated into `develop`; parser remediation, reusable test fixtures, named component inputs, retained root/Core ownership, and the final full gate were accepted in order. The prerequisite for beginning the next product Task is met at `063c59d`.
- Completed generic-improvements Task: [Improve Generic CLI Structure](tasks/generic-improvements/_generic-improvements.md), integrated at `063c59d`. The read-only [Find](tasks/read-only/find.md) parent and Child 3 are now Complete and accepted in the commit containing this record update; Child 1 is Complete and accepted at exact commit `96fe413`, and Child 2 final acceptance is recorded at exact `ff7ce3f`. Child 3's phase history remains Preflight `28d316a`, Gray `a76a217`, original Red `22d3bff`, metadata corrections `6a9a0de` and `eea3d59`, supplemental escaping Red `a865fd1`, Green `cb7874c`, Blue `3f81e76`, and no-op Purple `426d4f5`. The final managed/native gate and package, artifact, static, no-write, Route/Shell/root, and generated-routing audits passed. Modern C# Improvements is Complete and accepted at `a1cbf09`.

### Modern C# Final Acceptance

The final gate passed from exact clean source commit `6af5fb1`. The Release build
was warning-free, format and diff checks passed, and informational `CA1062`,
`CA1510`, and `CA2264` diagnostics were zero. Full managed Unit, Integration, and
freshly managed-published EndToEnd passed `617/617`, `241/241`, and `57/57`;
focused Source/Route Unit and Integration passed `562/562` and `183/183`. The
local `win-x64` Native AOT root publication drove managed EndToEnd `57/57`
against the native root, the Native AOT Integration executable passed `241/241`,
and the Native AOT EndToEnd executable passed `57/57`; all runs had zero skips.

The managed and native public no-write fixture passed four invocations. Route List
returned exit `5` and `blocked`, and Route Inspect returned exit `3` and
`incomplete`, for both executables. Stderr was empty, JSON `command` and `status`
were typed, and byte/hash/entry snapshots were unchanged. The package audit listed
all six projects and found no vulnerable transitive package. The exact
root→Core, Unit→Core, Integration→Core+root+TestSupport, and EndToEnd→TestSupport
graph, one `.slnx`, six projects, 343 authored active C# files, reflection-disabled
JSON/source-generation/AOT settings, and artifact routing were unchanged. At that
historical acceptance boundary, no project-local `bin/obj` directories existed
and ignored output remained under `src/cli/artifacts/`. DX1 later moved the
solution controls and ignored output to the repository root.

Static audits report production suppressions `126 → 0`, production
`ThrowIfNull` `380 → 297`, active test suppressions `169 → 8` frozen intentional
injections, and test guards `21 → 8`. `required` is `97`, `init` is `122`,
nullable-analysis attributes are `14`, and active test identities are
`500/500` for `DisplayName`, `Feature`, and `Evidence`, with `103` argument
assertions. No forbidden nullable pragmas or `SuppressMessage` entries exist.
The changed-path audit from accepted Child 1 `96fe413` is exactly 121 authorized
paths: 112 C# plus these nine Working records. No project, configuration,
dependency, or generated path changed. Fresh integrated production correctness and
test/evidence reviews passed, and final improvement review is
`NO_MATERIAL_IMPROVEMENTS/PASS`. Optional shared projection/failure-reader
extraction is deferred because it is not a blocker and would reopen accepted
architecture or batches. The native claim is `win-x64` only; no six-RID parity or
`develop` integration is claimed, and no push occurred. Branch `feature/cli-find`
remains isolated while `develop` remains `e77902a`.

This Plan defines how the accepted replacement CLI reaches complete local and
release acceptance. The CLI Architecture and command contracts define what the
system means. Child Tasks define bounded outcomes and acceptance. This Plan
defines dependencies, order, integration gates, evidence, and resumption.

## Planning Basis

- Outcome: One complete, predictable, Native-AOT replacement executable and thin
  package wrappers implement every retained command without importing or falling
  back to the frozen MVP.
- Acceptance: Every command contract, cross-command invariant, filesystem and
  mutation safety boundary, current `linux-x64` build and smoke, packed install and
  invocation journey, checksum, and release gate has reproducible evidence and
  maintainer acceptance.
- Find starting point: The exact integrated production/source tree is `063c59d`;
  it contains the accepted workspace, Foundation, Route Discovery, and Generic
  CLI Improvements but no Find production or test code. The parent planning
  boundary is recorded at `b2e3106` on `feature/cli-find`; `develop` remains
  `e77902a`. Later Child 1 execution baselines must remain distinct.
- Integration risk: High. The work crosses a complete command tree, filesystem
  identity, persisted lifecycle state, mutation and recovery, Native AOT, package
  distribution, and public release.
- Non-goals: Legacy compatibility, partial publication, runtime plug-ins, a fake
  filesystem, native interop, a universal domain engine, and implementation before
  its parent architecture and Task are ready.

### References And Authority

| Source                                                                                                       | Question it answers                                                     | Status or authority                                                        | Use in this Plan                            |
| ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------- |
| [CLI Architecture](../../crystallized/documents/cli/architecture.md)                                         | How is the replacement structured and integrated?                       | Accepted current architecture                                              | Governs all steps                           |
| [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md)                             | Which command-local sources define behavior?                            | Accepted current contract map                                              | Selects command Tasks                       |
| [Detailed Contracts](../../crystallized/documents/cli/contracts/_contracts.md)                               | What must each command and shared operation do?                         | Accepted current product contracts                                         | Requirements and evidence                   |
| [Shared Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md)                   | Which conventions cross commands?                                       | Accepted current contract                                                  | Foundation and integration                  |
| [Program Architecture Directive](../../../directives/program-architecture.md)                                | Who owns architecture and when is delegation ready?                     | Binding workspace Directive                                                | Task readiness and integration              |
| [CLI Directive](../../../directives/open-forge/cli/_cli.md)                                                  | What rules apply to replacement work?                                   | Binding CLI Directive                                                      | Every CLI step                              |
| [Architectural Perspectives](../../../guidance/architectural-perspectives.md)                                | Which top-down and task-master questions apply?                         | Accepted Guidance for this program                                         | Planning and review horizon                 |
| [Task Template](../../../templates/memory/task.md)                                                           | Which fields make one Task durable?                                     | Experimental local Template                                                | Child Task shape                            |
| [Plan Template](../../../templates/memory/plan.md)                                                           | Which fields make coordinated execution resumable?                      | Experimental local Template                                                | This Plan's shape                           |
| [Implementation Reset](../../archived/cli-release/implementation-reset-2026-08-21.md)                        | What was useful or harmful in the removed implementation?               | Historical evidence                                                        | Avoid rediscovery and sunk-cost restoration |
| Maintainer-supplied review files under `.temp/review-20.08.2026/`                                            | What did the independent architecture review find?                      | Historical external evidence accepted where projected into current sources | Task detail and risk checks                 |
| [Replacement CLI edge-case ledger](edge-cases.md)                                                            | Which deferred edge cases need owner resolution or explicit acceptance? | Active working evidence                                                    | Route discovery and delivery gate tracking  |
| [CLI development flow evaluation](../../emerging/observations/2026-08-21_cli-development-flow-evaluation.md) | Which current-flow lessons should shape later slices?                   | Contextual Emerging observation                                            | Route-inspect planning and review           |

## Approach

Complete the program top down:

1. Freeze the physical workspace, project graph, dependency direction, and shell
   call surfaces.
2. Author the complete Task hierarchy before production source returns.
3. Implement the actual route-free foundation in one integrated architecture
   increment owned by the Mastermind.
4. Close shared safety and Framework fact foundations before commands consume
   them.
5. Implement read-only commands and the pure GN1 projection foundation in
   dependency order. Establish neutral mechanical
   foundations at the nearest shared scope when several accepted program outcomes
   require them, even when implementation order exposes one consumer first;
   promote semantic facts only after consumers prove identical meaning. Find's
   source catalogue is sequential; no shared source mutation runs in parallel.
   References may begin only after the source and document facts it needs are
   accepted.
6. Establish M1 lock, lifecycle, mutation, and external recovery-bundle
   foundations before the first mutating command.
7. Implement public Index, including the I1-owned body-free formation expansion,
   on accepted GN1 projection and M1 mechanics.
8. Implement mutations from narrow route operations to extension and root
   lifecycle operations.
9. Implement aggregate status, diagnosis, repair, and cleanup only after all state
   producers exist.
10. Complete the currently supported `linux-x64` build and smoke, packed install
    and invocation, and checksum evidence without partial publication.

Each step ends in one inspectable commit. Architecture and cross-cutting callable
contracts stay with the Mastermind. A smaller implementer receives one closed
child Task and exact predecessor outputs. A reviewer receives the exact commit or
diff, parent requirements, and claimed evidence.

Operational aggregate domains use one explicit immutable application-scoped
`OperationalContributorCatalogue` built by `CliCompositionRoot`. Producer-owned
typed contributors project narrow Status and Doctor views from fresh invocation
observations. They do not use dependency injection, a service locator,
reflection, a runtime registry, a generic operational engine, or ambient
registration. Task 12 owns the durable architecture rewrite; Task 15 Gray owns
the exact contributor and view signatures. Composition alone does not change
Status or Doctor public contracts or the persisted operational-command order.

## Prerequisites

| ID  | Prerequisite           | Required state and evidence                                                                           | Responsible source or role        | Blocks                        |
| --- | ---------------------- | ----------------------------------------------------------------------------------------------------- | --------------------------------- | ----------------------------- |
| P1  | Product contracts      | Current contract route is complete and conflicts are explicit                                         | Crystallized CLI contracts        | All command Tasks             |
| P2  | Greenfield boundary    | Old production removed and useful evidence preserved                                                  | Commit `40ba03e` and reset record | Foundation                    |
| P3  | Architecture           | Physical, project, dependency, call-surface, evidence, and sequence boundaries accepted               | CLI Architecture                  | Task authoring and foundation |
| P4  | Task governance        | Top-down and task-master perspectives are binding                                                     | Program Architecture Directive    | Delegation                    |
| P5  | Local toolchain        | Stable .NET 10 SDK and native prerequisites are available                                             | Foundation verification           | Foundation acceptance         |
| P6  | Find planning boundary | Current Find contract/Working packet passes targeted reviews and is recorded as accepted at `b2e3106` | Find Task and Mastermind          | Q1 readiness                  |

## Resources

| Resource                           | Purpose                                                | Availability or source | Needed by              | Responsible role |
| ---------------------------------- | ------------------------------------------------------ | ---------------------- | ---------------------- | ---------------- |
| Stable .NET 10 SDK                 | Build, test, format, publish, and AOT                  | Local and CI setup     | F1 onward              | Mastermind       |
| Current `linux-x64` native runner  | Accepted build and invocation smoke evidence           | Local or CI            | D1                     | Release Task     |
| Real OS temporary filesystems      | Identity, containment, mutation, and no-write evidence | TestSupport            | F4 and commands        | Owning Task      |
| Recovery-bundle fixtures           | Mutation and interruption evidence                     | Isolated filesystems   | M1 onward              | Owning Task      |
| Preserved test inventory           | Candidate expectations and fixtures                    | `src/cli/tests/`       | Relevant command Tasks | Task creator     |
| Independent advisors               | Named architecture or safety uncertainty only          | Optional               | Decision points        | Mastermind       |
| Bounded implementers and reviewers | Closed implementation and fresh diff review            | After Task readiness   | Commands               | Mastermind       |

## Work Graph

| ID   | State                                                                                                                                                                                                                                         | Action and observable result                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Depends on                                  | Lane                                                   | Task group                              | Verification                                                                                                                      |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ------------------------------------------------------ | --------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| S0   | Complete                                                                                                                                                                                                                                      | Preserve WIP, define architecture delegation rules, and reset old production                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | None                                        | Sequential                                             | Historical                              | Commits `4b873de`, `aa7d178`, `40ba03e`                                                                                           |
| S1   | Complete                                                                                                                                                                                                                                      | Define the complete top-down Architecture and executable Plan                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | S0, P1-P4                                   | Sequential                                             | Program                                 | Current sources and link checks                                                                                                   |
| S2   | Complete                                                                                                                                                                                                                                      | Author the complete hierarchical Task set with closed foundation and command boundaries                                                                                                                                                                                                                                                                                                                                                                                                                                                        | S1                                          | Sequential                                             | `tasks/`                                | Task graph audit and backlinks                                                                                                    |
| F1   | Complete                                                                                                                                                                                                                                      | Create the scoped C# workspace, project graph, dependencies, artifacts, and preserved-test quarantine                                                                                                                                                                                                                                                                                                                                                                                                                                          | S2, P5                                      | Sequential                                             | Foundation                              | Restore/build topology and no root C# files                                                                                       |
| F2   | Complete                                                                                                                                                                                                                                      | Implement Shell definitions, invocation, composition contracts, parser, pipeline, output, and serialization with no command                                                                                                                                                                                                                                                                                                                                                                                                                    | F1                                          | Foundation                                             | Foundation                              | Unit and integration evidence                                                                                                     |
| F3   | Complete                                                                                                                                                                                                                                      | Implement the thin root host and explicit route-free composition                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | F2                                          | Foundation                                             | Foundation                              | Managed process and terminal evidence                                                                                             |
| F4   | Complete                                                                                                                                                                                                                                      | Implement workspace, filesystem identity, typed reads, and physical-containment foundations                                                                                                                                                                                                                                                                                                                                                                                                                                                    | F2                                          | Foundation                                             | Safety foundation                       | Real-OS matrix including leave-and-reenter                                                                                        |
| F5   | Complete                                                                                                                                                                                                                                      | Establish active Unit, Integration, EndToEnd, TestSupport, Native AOT, and CI foundations                                                                                                                                                                                                                                                                                                                                                                                                                                                      | F1-F4                                       | Sequential                                             | Foundation                              | Managed and published process evidence                                                                                            |
| G1   | Complete                                                                                                                                                                                                                                      | Accept the actual command-free architectural foundation                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | F1-F5                                       | Sequential                                             | Foundation gate                         | Full diff, dependency audit, AOT execution                                                                                        |
| R1   | Complete                                                                                                                                                                                                                                      | Accept the complete `route list` slice and keep shared route facts local until route inspect proves identical consumers                                                                                                                                                                                                                                                                                                                                                                                                                        | G1                                          | Sequential                                             | Route discovery                         | Complete contract and public scenario                                                                                             |
| R2   | Complete                                                                                                                                                                                                                                      | Split and close route-inspect child Tasks before implementation; then implement and accept `route inspect` and promote proved shared route facts                                                                                                                                                                                                                                                                                                                                                                                               | R1                                          | Sequential                                             | Route discovery                         | List and inspect regressions                                                                                                      |
| GI1  | Complete                                                                                                                                                                                                                                      | Remediate parser behavior, improve active-test architecture, and accept the closed callable/root-host structure before the next product Task                                                                                                                                                                                                                                                                                                                                                                                                   | R2                                          | Sequential                                             | Generic improvement                     | Beginning/final complete suites and focused phase evidence                                                                        |
| Q1   | Complete                                                                                                                                                                                                                                      | Complete Find's source catalogue, corrected query operation, and presentation children in order. Children 1 and 2 are accepted, and Child 3 plus the Find parent are Complete and accepted in the commit containing this record update. The no-op Purple acceptance is recorded at exact `426d4f5`; the final managed/native gate and package, artifact, static, no-write, Route/Shell/root, and generated-routing audits passed.                                                                                                              | GI1, P6                                     | Sequential                                             | Source queries                          | Contract, CommonMark, process, AOT                                                                                                |
| GI2  | Complete                                                                                                                                                                                                                                      | Complete the accepted behavior-neutral Modern C# Improvements batches and final managed, Native AOT, package, audit, and public no-write gates.                                                                                                                                                                                                                                                                                                                                                                                                | Q1 Child 1 acceptance                       | Sequential                                             | Generic improvement                     | Final gate and integrated reviews pass                                                                                            |
| DX1  | Complete                                                                                                                                                                                                                                      | Move .NET workspace controls and artifacts to the repository root, make ordinary EndToEnd builds publish and discover `open-forge-dev`, remove obsolete preserved tests, and repair temporary compatibility-name routing.                                                                                                                                                                                                                                                                                                                      | Q1, GI2                                     | Sequential                                             | Developer workflow                      | Root build/test, environment-free EndToEnd, routing, review                                                                       |
| Q2   | Complete                                                                                                                                                                                                                                      | Public Red `43b75f3`, Green `23d5e2e`, managed `1270/1270`, public no-write, and supported local `linux-x64` root, Integration, and EndToEnd Native AOT execution pass.                                                                                                                                                                                                                                                                                                                                                                        | GI1, Q1 source/document acceptance, DX1     | Sequential                                             | Source queries                          | Contract, process, no-write, and Native AOT execution pass                                                                        |
| Q3   | Complete                                                                                                                                                                                                                                      | Implement and accept `context`; accepted tip `303ad7d` is squash-integrated at `ca097a2` and retained in the combined managed/native baseline.                                                                                                                                                                                                                                                                                                                                                                                                 | Q1, Q2                                      | Sequential                                             | Context                                 | Ordered context and exact content evidence passes                                                                                 |
| E1   | Complete                                                                                                                                                                                                                                      | Extension List is Complete at `db0d39a`; Extension Inspect accepted rebased feature `2b1e63d` is squash-integrated at `73b01be`, with combined managed `1054/378/116` and portable `linux-x64` Native AOT `378/116` acceptance.                                                                                                                                                                                                                                                                                                                | G1, GI1                                     | Read-only C                                            | Extension discovery                     | Catalogue, package-source, lifecycle, process, and AOT                                                                            |
| RM1  | Complete                                                                                                                                                                                                                                      | Promote the neutral routed authored-metadata fact while retaining `SourceDocumentForm` as the sole source classification authority; accepted feature `8a29321` is integrated at `5924698`.                                                                                                                                                                                                                                                                                                                                                     | Q1-Q3, R2                                   | Sequential                                             | Source metadata                         | Route preservation, metadata grammar, and Native AOT pass                                                                         |
| GN1  | Complete                                                                                                                                                                                                                                      | Pure Generated Navigation projection and bounded-region facts are accepted and squash-integrated at `21e5200`; full managed and local `linux-x64` Native AOT Integration evidence passes.                                                                                                                                                                                                                                                                                                                                                      | Q1-Q3, R2, RM1                              | Read-only D                                            | Generated navigation                    | Determinism, exact bytes, no-write, and affected regressions                                                                      |
| DGC1 | Complete                                                                                                                                                                                                                                      | Correct form-aware Context Skill metadata, source-specific compact Context/Find findings, and stale Route Inspect Context help without changing structured command meaning; accepted feature candidate `deb3f14` is squash-integrated at `bba84b6`.                                                                                                                                                                                                                                                                                            | Q1-Q3, R2, RM1, GN1                         | Sequential                                             | Dogfooding correction                   | Managed `1031/411/120` and Native AOT `411/120` pass                                                                              |
| PC1  | Complete                                                                                                                                                                                                                                      | Proportionate guidance `5f9f59e`, Route List correction `2cd525d`, and Markdown correction `0d88606` are squash-integrated; PCF-001/002/003/007 are closed and all remaining dispositions are preserved.                                                                                                                                                                                                                                                                                                                                       | DGC1                                        | Parallel correction                                    | Proportional correctness                | Release 0/0; managed `1053/411/120`; portable `linux-x64` Native AOT `411/120`; dogfood and Sol/xhigh review pass                 |
| M1   | Complete                                                                                                                                                                                                                                      | Shared lock, lifecycle, revalidation, atomic application, receipt, neutral recovery catalogue, lease-gated deletion guard, and external recovery-bundle foundations are accepted at exact production candidate `e7d937f` under authority `01dd552`. All-five-root-key creation, Framework-created complete empty Extensions, and fail-closed existing incomplete-state planning are integrated at `1d404c5`; focused/full managed, Native AOT dogfood, diff, and review gates pass.                                                            | GN1, E1, DGC1, PC1                          | Sequential                                             | Mutation foundation                     | Direct failure, interruption, residual, lifecycle-envelope, and AOT evidence pass                                                 |
| I1   | Complete                                                                                                                                                                                                                                      | Public `index`, including the I1-owned body-free formation expansion and orchestration over accepted GN1 projection and M1 mechanics, is accepted at exact feature candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`. The warning-free Release, complete managed, portable `linux-x64` Native AOT, and controlled public-process gates pass with zero failures or skips. Final closeout tip `2b353c48978ee88e53345be8037776181612222c` is locally squash-integrated at `09aa03eddb97831ff544afe1eac54ad9af501f5c` with exact tree equality. | GN1, M1                                     | Sequential                                             | Generated navigation                    | Exact contract, idempotence, unchanged-authority, recovery, managed `1206/480/125`, and portable `linux-x64` AOT `480/125` pass   |
| D0   | Complete at `38e1498`                                                                                                                                                                                                                         | Freeze the accepted next-wave Architecture, command contracts, shared-foundation Tasks, command sequencing, checkpoint, and public docs before production mutation.                                                                                                                                                                                                                                                                                                                                                                            | I1, M1, maintainer decisions                | Sequential                                             | Program contract                        | Link/frontmatter/stale-claim checks, dogfood load/doctor, independent Sol/xhigh review                                            |
| SF1  | Complete at `e782090`                                                                                                                                                                                                                         | Add one BCL-only Shell interaction transport. Protected command integration later injects it only into prompt-capable operation factories; requests retain only command-local interaction-policy Booleans.                                                                                                                                                                                                                                                                                                                                     | D0, F2-F3                                   | Parallel foundation                                    | Shell interaction                       | Focused transport Unit; injected host and redirected-process proof with prompt-capable command integration                        |
| SF2  | Complete at `680915a`                                                                                                                                                                                                                         | Embed the canonical Framework payload and expose exact ordered asset bytes, paths, hashes, and inventory identity through a neutral BCL reader.                                                                                                                                                                                                                                                                                                                                                                                                | D0, F1                                      | Parallel foundation                                    | Framework distribution                  | Source parity, isolated published binary, Native AOT resource proof                                                               |
| SF3  | Complete at `0989356`                                                                                                                                                                                                                         | Add required nullable per-target `sourceAssetPath` provenance to Framework lifecycle schema v1 without adding instance collections or migration machinery.                                                                                                                                                                                                                                                                                                                                                                                     | D0, M1                                      | Parallel foundation                                    | Framework lifecycle                     | Source-generated JSON, structural validation, lifecycle and Native AOT regressions                                                |
| SF4  | Complete at `33913df`; lock-location correction integrated with C2 at `c60fcb98`                                                                                                                                                              | Add one lease-bound ordinary-BCL directory effect for Install and Route Init; the later shared correction makes missing `.agents` an ordinary first effect after external lease acquisition.                                                                                                                                                                                                                                                                                                                                                   | D0, M1                                      | Parallel foundation                                    | Mutation directory effect               | Real filesystem races, residuals, file/recovery non-regression, Native AOT proof                                                  |
| C1   | Complete at protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`; exact candidate `789cc917f2d0cb38c5229cc2dc7fee013218d341`, tree `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`                                              | Implement Extension Create with accepted manifest defaults/options, catalogue boundary, ordered JSON result, and a command-local wizard for missing required facts.                                                                                                                                                                                                                                                                                                                                                                            | SF1, M1, E1                                 | Parallel command                                       | Lifecycle mutation                      | Managed `1356/575/132`; `linux-x64` Native AOT root/version/ELF and `575/132`; dogfood and Sol/xhigh review pass                  |
| C2   | Complete at local integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`                                                   | Implement root Install over the closed embedded base Framework subset while preserving trusted dynamically added scoped lifecycle targets and forming the accepted exact public JSON result, including typed residual values `none`, `retained`, and `unknown`.                                                                                                                                                                                                                                                                                | SF1-SF4, M1, I1                             | Parallel command                                       | Framework lifecycle                     | Release `0/0`; managed `1390/598/136`; `linux-x64` Native AOT `598/136`; focused post-rebase, dogfood, and two Sol/xhigh rechecks |
| C3   | Complete and squash-integrated at `fa3db1ee` with exact tree equality to final reviewed candidate `37c9360`                                                                                                                                   | Correct Route Inspect's accepted one-answer interactive collision selection without changing its non-interactive contract or result model.                                                                                                                                                                                                                                                                                                                                                                                                     | SF1, R2                                     | Parallel command plus sequential protected integration | Route discovery correction              | Focused `131/8/82/32`, full managed `1284/511/125`, Native AOT `511/125`, no-write dogfood, and review pass                       |
| M2   | Active: Route Init, Route Create, QR1, Task 3 “Route Update”, Task 4 “Route Move”, and Task 12 Complete; Route Remove remains                                                                                                                 | Complete the current positive-unmanaged-only Route Remove through its separate command-local gate after the project-control priority advances through the adoption slice. No Route Remove command or public wire exists, and Remove remains Planned after the accepted Move and integrated Task 12 results.                                                                                                                                                                                                                                    | C2, M1, R2, I1, QR1                         | Sequential route mutation                              | Route mutation                          | Route Move managed `1701/888/165`, Native AOT `888/165`, managed-on-native `165`, focused `94/79/13`, dogfood, and review pass    |
| QR1  | Complete and squash-integrated at `862cbf2a847b5adac77c6923e51f2c28c315415f`, exact tree `571f104f507c3f72b7404cc0dacd86c818ec0a2e`, from accepted implementation candidate `a4ccf19a`, exact tree `97254e655f51ab421dacc8eff7a8c93f726f2625` | Closed all thirteen accepted findings/candidates, including final `QR-R1-001` evidence-tier correction and same-reviewer narrow revalidation.                                                                                                                                                                                                                                                                                                                                                                                                  | M2 Route Create integration, C2, M1, R2, I1 | Sequential quality remediation                         | Architecture/design/evidence correction | Final managed `1522/736/146`; supported Native AOT Integration `736/736`; focused, format, diff, and Sol/xhigh acceptance pass    |
| M3   | Planned after M2 integration                                                                                                                                                                                                                  | Implement root Update only after the complete Route Mutation lane, preserving its independent lifecycle reconciliation boundary. Independent preparation may begin earlier without implementing dependent behavior.                                                                                                                                                                                                                                                                                                                            | M2, C2, M1, I1                              | Sequential lifecycle mutation                          | Lifecycle mutation                      | Payload, lifecycle, recovery, workspace, and AOT evidence                                                                         |
| M4A  | Active at phase 3/5, milestone 3/8; Gray `fb8b98c8`, receipt `2c3918ae`, and Red `81c8bc1e` are immutable                                                                                                                                     | Implement Task 14 Extension Install as the first adoption command. One Brilliant Implementer owns coherent production and focused verification under its Task Mastermind. Unfinished Route Remove and root Update are not prerequisites.                                                                                                                                                                                                                                                                                                       | Task 12, C2, M1, I1                         | Prioritized lifecycle mutation                         | Extension mutation                      | Collision, dependency, source-review, recovery, catalogue, process, and AOT evidence                                              |
| O1A  | Queued after M4A                                                                                                                                                                                                                              | Implement Task 15 Status for every producer in its frozen baseline, then Task 16 Doctor for that complete declared contributor inventory. Later producers extend explicit typed contributors and affected evidence before their own acceptance.                                                                                                                                                                                                                                                                                                | M4A                                         | Sequential adoption slice                              | Operations                              | Complete declared aggregate and diagnostic evidence; no omitted-domain healthy claim                                              |
| M4B  | Pending after M3 and the adoption slice                                                                                                                                                                                                       | Implement Task 17 Extension Update and Task 18 Extension Remove, extending the accepted Status/Doctor contributor inventory before each Task closes.                                                                                                                                                                                                                                                                                                                                                                                           | M3, M4A, O1A                                | Sequential lifecycle mutation                          | Extension mutation                      | Collision, recovery, catalogue, and affected Status/Doctor regression evidence                                                    |
| O2   | Pending                                                                                                                                                                                                                                       | Finalize the complete Status/Doctor contributor inventory, then implement relevant-domain Task 19 Repair and Task 20 Cleanup.                                                                                                                                                                                                                                                                                                                                                                                                                  | M2-M4B, O1A                                 | Sequential                                             | Operations                              | Complete inventory, plan/apply/recovery, cleanup, and idempotence evidence                                                        |
| D1   | Preparation active; implementation pending                                                                                                                                                                                                    | Keep CI to a build/test matrix plus a separate on-demand publish path. Deliver npm shims for accepted Linux, macOS, and Windows x64 packages; ARM remains an explicit feasibility decision until accepted.                                                                                                                                                                                                                                                                                                                                     | O2, Task 10 and accepted remediation        | Preparation parallel; implementation later             | Distribution                            | Accepted-host native artifacts, thin package journeys, reproducibility, and checksums                                             |
| A1   | Pending                                                                                                                                                                                                                                       | Run final whole-program acceptance and local integration                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | D1                                          | Sequential                                             | Acceptance                              | Maintainer acceptance; no partial release                                                                                         |

### Post-M1 Durable Queue

Public Index is Complete in exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534` after its authority boundary was
explicitly selected following proportional prerequisite `b25d76e`. Its final
closeout is locally integrated with exact tree equality. Preserve these accepted
program items through the later boundaries that own them:

- [x] Author and accept the binding CLI proportionality and evidence Directive,
      and add its per-Task applicability check below before I1 Phase 0 activation;
      satisfied at `b25d76e`.
- [x] Keep generic Route Init as exact-chain initialization and add the accepted
      Framework-aware concrete-route mode after root Install. The accepted mode
      uniquely aligns canonical Framework segments, deterministically slugs only
      ID-form inserted scopes, creates one sparse chain, and keeps scope entrypoints
      user-owned.
- [x] Accept and integrate the existing Generated Navigation formation expansion
      at feature `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash commit
      `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with closeout integrated at
      `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
      `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. Route
      Init/Create/Update/Move/Remove and root Install/Update consume its accepted
      generated-navigation formation or projection capabilities. Extension Create
      remains independent of it.
- [x] Route Move completed its separate command-local reference and lifecycle
      proportionality gate and is integrated.
- [ ] Keep Route Remove behind its separate command-local reference and lifecycle
      proportionality gate. Route Move completion, canonical lifecycle creation,
      and the neutral Markdown label projection do not settle Route Remove's
      remaining command-local decisions or make it Active or Ready.
- [x] Accept Extension Create's manifest options and command-local missing-fact
      wizard. Invalid input may be corrected locally without an attempt limit; EOF
      is invalid and cancellation interrupted, both without writes. C1 was sequenced
      after SF1 and does not gain workspace or dependency-resolution behavior.
- [x] Freeze Extension Create's exact defaults, catalogue-parent and sibling
      eligibility, and command-local JSON result shape/order.
- [x] Freeze C3's one-answer prompt grammar, Install's one-confirmation
      interaction, and the shared lease-bound ordinary-BCL directory-create effect.
- [x] Supersede the earlier bootstrap boundary: acquire the persistent external
      zero-byte lock under `LocalApplicationData/OpenForge/locks/v1`, then apply
      missing `.agents` as the first ordinary visible planned/reported lease-bound
      directory effect. Lock and recovery use separate versioned subtrees.
- [x] Preserve Architecture order: complete Route Mutation M2 before root Update
      M3. Extension Install is independently prioritized after the integrated
      Task 12 boundary, followed by incremental Status and Doctor over the exact
      frozen producer inventory. Parallelize only independent preparation until
      each mutation dependency is satisfied.
- [x] Keep D1 at the accepted thin tier: delivery surfaces consume accepted
      binaries and do not reproduce CLI behavior. The maintainer-expanded target
      is Linux, macOS, and Windows x64 through a build/test matrix and a separate
      on-demand publish path. ARM requires an explicit feasibility disposition
      before it becomes a package target. Signing, SBOM, OIDC, provenance,
      attestation, and support-floor matrices remain outside current evidence.
- [ ] Keep Repair behind fresh relevant-domain diagnosis and verification.
- [x] Keep the simplified O1 Status/Doctor and O2 Repair/Cleanup split reflected
      in the work graph. O1 may land incrementally after Extension Install when
      it completely names its frozen contributor inventory; every later producer
      extends that inventory and final acceptance revalidates it in full.

This queue does not reopen M1. D0 and SF1-SF4 are complete at the integrated
baseline recorded above. C1 is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with exact candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341` and tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. C2 is Complete at local
integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`. Its exact fully present ordered
public Install JSON result is accepted and frozen, including typed residual
values `none`, `retained`, and `unknown`. Route Init is Complete at `cc5085ce`,
and Route Create is Complete at `19412d2`, exact tree `2bbba7e`. Route Create
supplies the `RouteCreateJsonContext` and Route-help predecessor slices. The
separate CLI Quality Remediation Task is Complete and squash-integrated at
`862cbf2a`, exact tree `571f104f`, from accepted implementation candidate
`a4ccf19a`, tree `97254e65`. Task 3 “Route Update” is Complete at accepted
closeout `27df8325`, tree `b68d4349`. The
M1 lifecycle correction is integrated at `1d404c5`, and the
neutral Markdown link-label prerequisite is integrated at `89a35a7`. Later
commands still wait for their listed predecessors and any named decision
frontier.

### Per-Task Proportionality And Evidence Check

Every CLI implementation Task records this check in its Execution Capsule before
mutation. It selects evidence after the CLI scope is routed and does not create
a second Directive or route-loading applicability gate.

| Check                                           | Required record                                                                                                                                                                                                                                                |
| ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Consequence, reversibility, and threat boundary | Affected users, data, and systems; practical recovery; ordinary failure modes; and the accepted cooperating-process boundary, including what a malicious same-user actor can defeat.                                                                           |
| Standard capability and platform sufficiency    | The pinned runtime/BCL/platform path that proves the requirement, or the exact unmet accepted guarantee that must return to Architecture.                                                                                                                      |
| Shared-foundation reuse                         | Accepted neutral capabilities and facts reused, local semantic policy, and any additional accepted consumer that justifies promotion.                                                                                                                          |
| Exceptional machinery                           | `none` by default; otherwise the accepted requirement, bounded exception, maintainer decision, evidence, documentation impact, and removal or re-evaluation condition.                                                                                         |
| Cheapest decisive evidence                      | The lowest Unit, Integration, EndToEnd, or PackageEndToEnd tier that proves each behavior, plus directly affected regressions and explicit contract/Architecture evidence.                                                                                     |
| Complete managed/AOT gate trigger               | First golden slice for an archetype, integration wave/shared promotion, or material public, composition, shared-capability, safety, serializer, dependency/runtime/toolchain, project/build/package, or release change. Record `none` when no trigger applies. |

Focused leaf evidence is the default. Run one complete managed suite and
supported Native AOT gate at the recorded golden-slice, integration-wave, or
material trigger, not for every leaf by default. An explicit Architecture,
contract, or Task requirement remains binding. An unchanged exact predecessor may
supply the beginning baseline when its projects, executable, environment, counts,
and result are recorded. Reassess the check if evidence changes consequence,
reversibility, trust boundary, compatibility, or likely harm.

### Parallel Lanes

Parallelism begins only after the shared predecessor is committed and each lane
has non-overlapping production and test ownership.

| Lane            | Steps                                                | May start when                                                                                                                                                                                                                                                                                                                                                           | Owned surfaces                                                                                                                                                      | Shared dependency                                                                     | Integration point                                             |
| --------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| Foundation      | F2, F4 preparation                                   | F1 complete; callable contracts frozen by Mastermind                                                                                                                                                                                                                                                                                                                     | Separate Shell and Framework capability paths                                                                                                                       | Core models and project graph                                                         | F5                                                            |
| Find            | Q1                                                   | GI1 and P6 complete; Child 1 and Modern C# are accepted; Child 2 records its focused Preflight before mutation                                                                                                                                                                                                                                                           | Find source catalogue, query, and presentation paths                                                                                                                | Neutral source/document facts owned by Mastermind                                     | Q1 acceptance                                                 |
| References      | Q2                                                   | GI1 complete and the Q1 source/document facts it needs are accepted                                                                                                                                                                                                                                                                                                      | References command paths; no shared-source mutation                                                                                                                 | Accepted neutral source/document facts                                                | Q2 acceptance                                                 |
| Read-only C     | E1                                                   | G1 and GI1 complete; extension source contract frozen                                                                                                                                                                                                                                                                                                                    | Extension List/Inspect roots                                                                                                                                        | Shell and filesystem foundation                                                       | M1                                                            |
| Read-only D     | GN1                                                  | Q1-Q3 and R2 complete; pure-effect boundary frozen                                                                                                                                                                                                                                                                                                                       | New `Framework/GeneratedNavigation/**` and mirrors                                                                                                                  | Accepted source/route/document facts                                                  | GN1 acceptance                                                |
| Next foundation | SF1, SF2, SF3, SF4                                   | Complete at integrated D0/SF1-SF4 baseline; combined managed and Native AOT evidence recorded above                                                                                                                                                                                                                                                                      | Separate Shell interaction, Framework distribution, lifecycle-provenance, and directory-effect paths                                                                | M1 and accepted root project graph                                                    | Complete; protected surfaces remain integration-owned         |
| Next commands   | C1, C2, C3, C4                                       | C1 complete at `4c85d1d6`; C3 complete at `fa3db1ee`; C2 complete at `c60fcb98`; C4 Route Init complete at `cc5085ce`; Route Create complete at `19412d2`; QR1 squash-integrated at `862cbf2a`/tree `571f104f`; Route Update complete at `27df8325`/tree `b68d4349`; Route Move complete at `631983ea`/tree `d6f6fdf7`; Task 12 integrated at `495a7ed6`/tree `9c4a33b1` | Separate Extension Create, Install, Route Inspect, Route Init, Route Create, Route Update, and Route Move paths; QR1 consumed Route Create's two predecessor slices | Root composition, serialization, help, process, recovery, lifecycle, and AOT evidence | Begin the prepared Task 14 adoption slice before Route Remove |
| Delivery        | Current `linux-x64` smoke, packed journey, checksums | O2 behavior complete; release contracts frozen                                                                                                                                                                                                                                                                                                                           | Separate workflow and package paths                                                                                                                                 | Accepted command binaries                                                             | D1 acceptance                                                 |

No parallel implementation may change the same shared capability. Promotion or
cross-lane contract changes return to a sequential Mastermind integration step.
After Find Child 1 acceptance, the solution-wide [Modern C# Improvements](tasks/modern-csharp-improvements.md)
Preflight was accepted at exact `55eb82e`. All five ordered modernization batches
and the final gate are Complete and accepted at exact `a1cbf09`:
Framework `a90af59`, Shell/root `fe10525`, Route Inspect/family `62a1dd9`, Route
List `273eb45`, and Tests/support `6af5fb1`. Find Child 2's original Preflight
through Blue history is accepted through exact `685e2dd`. Its shared
Markdown/YAML correction Gray is accepted at exact `2cae7a4`, Red at exact
`a673ebe`, and Green at exact `d4701ad`. Correction Blue is accepted at
exact `0006915`, and corrected Purple at exact `2d10474`. Final-review supplemental
Red is accepted at exact `c72dd5e`, and its evidence corrections through exact
`23fe39a`, with the Unit portion at exact `d494adb`. Corrected Green is accepted at
exact `2337d62`; final evidence and Child 2 acceptance are recorded at exact
`ff7ce3f`; Child 3 focused Preflight is accepted at exact `28d316a`, Gray at exact
`a76a217`, original Red at exact `22d3bff`, metadata corrections at exact
`6a9a0de` and `eea3d59`, and supplemental escaping Red at exact `a865fd1`.
Child 3 Green is accepted at exact `cb7874c` (`Implement Find presentation`) over
corrected Red `a865fd1`. Fresh Green evidence passes the locked restore,
warning-free Release build, format and diff checks, focused Find/direct-root and
generated-serialization tests, affected Shell/Route tests, managed non-AOT
publish, and published Find EndToEnd `13/13`, all with zero skips. Fresh final
bounded correctness review is `PASS` with no material findings; Native AOT was
intentionally not claimed for Green. Blue is accepted at exact `3f81e76`
(`Simplify Find JSON projection`) after changing production structure only in
`FindJsonProjection.cs` to use the canonical status and finding-code readers and
remove the two duplicate private mapping methods. Blue's warning-free build,
format/diff checks, focused Unit `206/206`, and focused Integration `43/43` pass
with zero skips; the bounded correctness review is `PASS`, and the local
improvement review is `APPROVED — NO_MATERIAL_IMPROVEMENTS`. The no-op Purple
acceptance is recorded at exact `426d4f5` with verdict
`NO_MATERIAL_IMPROVEMENTS`; focused Unit `206/206`, focused
Integration/serialization `43/43`, and published Find EndToEnd `13/13` passed with
zero skips, and source diff/check against `3f81e76` was clean/empty. No
test/support, production, contract, project, package, configuration, generated,
Route, Shell, or root file changed, and no Purple code/test commit was
manufactured. Find Child 3 and its parent are now Complete and accepted in the
commit containing this record update. The final artifact and public no-write audits
passed. `CLI-EDGE-001` remains non-product only.

## Step Rules

### S2: Task Authoring

- Instantiate one parent program Task, one Task-group entrypoint per phase, and one
  Task per coherent independently accepted result.
- Give foundation Tasks accepted class maps, project paths, dependencies, and
  exact evidence.
- Give command Tasks contract matrices, predecessor facts, local models, promoted
  capability rules, test disposition, and public scenarios.
- Split a Task into child or subchild files when separate ownership, state,
  evidence, or integration justifies it. Keep checklists inside a Task when another
  file would add only ceremony.
- Do not add production source in S2.

### F1-F5: Actual Foundation

- The Mastermind authors the first foundation directly from the Architecture and
  foundation Tasks.
- The result is an actual retained host and Core, not a probe or spike.
- No command symbol, route behavior, fake operation, or Foundation-named domain
  model is introduced merely to prove plumbing.
- Tests prove shell stages, terminal modes, process boundaries, serialization,
  filesystem safety, and AOT without inventing a retained command.

### Command Steps

- Begin with a contract-to-evidence matrix and preserved-test disposition.
- Freeze command-local definitions, request, result, binding, and shared-fact
  dependencies before behavior delegation.
- Implement one complete command. Do not create placeholders for later commands
  beyond symbol/help entries explicitly required by current product help.
- Promote a semantic unit only at the integration point where a second real
  consumer proves identical meaning.
- End with focused managed, process, unchanged-state, diff, and architecture
  evidence, plus directly affected regressions. Run the complete managed/AOT gate
  at the golden slice, integration wave, or material trigger recorded by the
  per-Task check, and whenever an accepted Architecture, contract, or Task
  requirement calls for it.

### Mutation And Delivery Steps

- Mutation Tasks separate planning from application and prove revalidation after
  lock acquisition.
- Recovery and lifecycle schemas are frozen by the Mastermind before command
  implementation.
- Delivery Tasks consume accepted binaries and do not reproduce behavior. D1 is
  limited to current `linux-x64` build and smoke, packed install and invocation, and
  checksums unless a later explicit maintainer decision expands it.

## Decision Points

| ID  | Decision                                                                                    | Current direction                                                                                                                                                                                                                            | Decision-maker                       | Needed before                | Result if reopened                                                                 |
| --- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ | ---------------------------- | ---------------------------------------------------------------------------------- |
| D1  | Can the managed BCL prove required component-wise physical identity on every target?        | Prove with real OS and AOT evidence; no interop                                                                                                                                                                                              | Maintainer after Mastermind evidence | F4 acceptance                | Narrow Architecture return                                                         |
| D2  | Does `route init` gain a Framework-shape mode?                                              | Accepted: concrete uniquely aligned sparse scoped chain after root Install                                                                                                                                                                   | Maintainer                           | D0 contract freeze           | Reopen contracts, lifecycle ownership, sequencing, and evidence                    |
| D3  | Does a dependency version need replacement?                                                 | Keep accepted exact versions until evidence requires change                                                                                                                                                                                  | Maintainer                           | Owning foundation/command    | Focused dependency decision and full AOT proof                                     |
| D4  | Do perspectives, Tasks, or Plans become Framework primitives?                               | No; continue local trial                                                                                                                                                                                                                     | Maintainer                           | After several complete Tasks | Separate Framework proposal, not CLI scope                                         |
| D5  | Where does native interaction cross generic Shell boundaries?                               | Root composition injects only prompt-capable factories; requests carry only local policy Booleans                                                                                                                                            | Maintainer                           | SF1                          | Reopen Shell/root callable architecture and affected tests                         |
| D6  | What exact prompt grammar and capability rule closes Route Inspect's collision gap?         | Accepted: terminal-capable stdin/stderr, one answer by one-based number or exact path; invalid/EOF blocked, cancellation interrupted                                                                                                         | Maintainer                           | C3                           | Reopen command contract and focused evidence                                       |
| D7  | What minimal BCL directory-create effect is shared by Install and Route Init?               | Accepted: lease, immediate missing/physical-parent revalidation, BCL create, post-verification, retained residual                                                                                                                            | Maintainer                           | SF4                          | Reopen mutation contract, reuse, and failure evidence                              |
| D8  | What exact name and description defaults does Extension Create write?                       | Accepted: hyphen-split first-ASCII-uppercase name and `Open Forge Extension package <stable-id>.`                                                                                                                                            | Maintainer                           | C1                           | Reopen manifest contract and exact evidence                                        |
| D9  | Which catalogue-parent states are eligible and how do sibling packages affect Create?       | Accepted: any existing safely resolved directory; no marker; empty/siblings allowed; inspect only exact ID destination                                                                                                                       | Maintainer                           | C1                           | Reopen destination classification and collision evidence                           |
| D10 | What exact command-local JSON result shape and property order does Extension Create expose? | Accepted ordered catalogue, destination, ID, manifest, mode, effects, verification, and unchanged-workspace fact                                                                                                                             | Maintainer                           | C1                           | Reopen source-generated schema and renderer evidence                               |
| D11 | How does Install confirm, refuse, handle EOF/cancellation, and avoid prompting?             | Accepted: prompt once only for prompt-capable human writes after preflight; refusal/EOF/cancel interrupted; other modes never prompt                                                                                                         | Maintainer                           | C2                           | Reopen command-local interaction contract and evidence                             |
| D12 | How does Extension Create gather and correct missing required human input?                  | Accepted: ask only missing stable-ID/catalogue facts; local correction without attempt limit; EOF invalid, cancellation interrupted; no generic retry framework                                                                              | Maintainer                           | C1                           | Reopen wizard contract and focused evidence                                        |
| D13 | How does a missing `.agents` container compose with the workspace lock and SF4?             | Superseded by accepted external-lock correction: acquire the persistent external lock first, then apply `.agents` as the first ordinary planned/reported lease-bound directory effect; no bootstrap result remains                           | Maintainer                           | SF4, C2, M2                  | Reopen lock location, result contract, mutation sequencing, and residual evidence  |
| D14 | May root Update M3 behavior begin alongside Route Mutation M2 after Install?                | Accepted: no; full M2 behavior precedes M3, while independent preparation may run earlier                                                                                                                                                    | Maintainer                           | M2, M3                       | Reopen program sequencing and lane ownership                                       |
| D15 | What exact public Root Install JSON result shape and residual vocabulary closes C2?         | Accepted: one fully present ordered result with mode, force, automatic, atomic nullable source/classification/footprint, exact effects, lifecycle, recovery, verification, findings, and typed residual values `none`, `retained`, `unknown` | Maintainer                           | C2                           | Reopen result model, serialization, help, process evidence, and command acceptance |

## Risks, Recovery, And Stop Conditions

| Risk or trigger                                                                            | Affected steps | Safeguard                                                        | Recovery or stop response                                             |
| ------------------------------------------------------------------------------------------ | -------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| Local command design bypasses the system architecture                                      | All commands   | Parent links, frozen foundation, Mastermind integration          | Reject local implementation and return to parent Task                 |
| A Task still contains an architecture choice                                               | S2 onward      | Task readiness audit                                             | Keep Task blocked; resolve in Architecture first                      |
| Preserved tests anchor obsolete structure                                                  | Command Tasks  | Map behavior to current contracts before porting                 | Rewrite fixture or test; never restore structure for test convenience |
| Shared semantic capability is promoted without a second consumer proving identical meaning | R2 onward      | Promotion evidence in integrating Task                           | Move it back to narrow scope or split semantics                       |
| Filesystem safety cannot be proved portably                                                | F4, mutations  | BCL-first real-OS and AOT matrix                                 | Stop and return to D1; do not weaken or add native code               |
| Native AOT differs from managed behavior                                                   | F5 onward      | Publish and execute affected boundaries every increment          | Reject managed-only pass; correct or reopen dependency                |
| Mutation leaves unverified partial state                                                   | M1 onward      | Plan, lock, revalidate, apply, verify, recovery evidence         | Block command acceptance and preserve owned fixture evidence          |
| Parallel lanes modify shared contracts                                                     | Parallel work  | Non-overlapping paths and sequential integration                 | Stop lanes and integrate one accepted contract first                  |
| Task records become stale bureaucracy                                                      | S2 onward      | Update only at state/evidence boundaries; prune completed detail | Consolidate outcomes and archive or prune temporary records           |

## Verification And Integration

| Gate           | Inputs                                      | Verification                                                                                                                                                                                            | Pass condition                                                        | Resulting update         |
| -------------- | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------ |
| VG0 Planning   | Architecture, Plan, Task hierarchy          | Link, hierarchy, dependency, scope, and stop-condition audit                                                                                                                                            | Every implementation Task is closed or explicitly blocked             | Foundation authorized    |
| VG1 Foundation | F1-F5                                       | Restore, format, build, unit, integration, end-to-end host, dependency audit, local AOT publish and execution                                                                                           | Clean route-free architecture with no probe or root C# clutter        | G1 accepted              |
| VG2 Command    | One command and affected shared facts       | Focused managed tests, directly affected regressions, public process scenario, and unchanged-state check; complete managed/AOT gate at the recorded golden-slice, integration-wave, or material trigger | Contract complete with no architectural debt deferred to next command | Next command authorized  |
| VG3 Mutation   | M1 and one mutation command                 | Focused failure matrix, lock/revalidation, planned effects, bundle retention/cleanup, idempotence, and process evidence; complete managed/AOT gate at the recorded trigger                              | No unverified partial state or hidden lifecycle behavior              | Next mutation authorized |
| VG4 Delivery   | Complete commands and thin release surfaces | Current `linux-x64` build and smoke, packed install and invocation, and checksums                                                                                                                       | Complete non-shipping candidate                                       | Final acceptance         |
| VG5 Release    | VG4 and maintainer review                   | Main-only release procedure and public smoke tests                                                                                                                                                      | Maintainer explicitly accepts shipping release                        | Release and closeout     |

## Coordination And Continuity

- Agents may surface architectural alternatives and supporting evidence, but may
  not accept a product feature addition or removal. Only explicit maintainer
  direction changes accepted product scope.
- Child Tasks: The `tasks/` hierarchy created in S2.
- Checkpoint: [CLI Development Checkpoint](../checkpoints/cli-development.md).
- Handoffs: [CLI Find Accepted Handoff](../handoffs/2026-08-25_cli-find-accepted.md)
  is sealed for the post-acceptance resumption boundary. Do not edit it. The
  generated Handoff `Entries` remain unchanged under `CLI-EDGE-001`.
- Related plans: The removed release Plan is historical at
  `../../archived/cli-release/release-plan-2026-08-21.md`.
- Update points: After S2, every foundation gate, every accepted command, each
  shared promotion, mutation foundation acceptance, delivery acceptance, and any
  Architecture return.
- Resumption path: Read the CLI Architecture, this Plan, the Checkpoint, the
  selected parent Task, and the active leaf Task. Then execute the leaf Task's
  stated next action.

## Completion

This Plan completes only when every child Task is accepted or deliberately
cancelled, all retained commands and release surfaces are integrated, complete
managed and native evidence passes, current Architecture and contracts match the
implementation, temporary records are consolidated, and the maintainer accepts
the release. Until then the replacement remains non-shipping.
