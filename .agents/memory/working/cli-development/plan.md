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
  Mastermind owns architecture, sequencing, Task decomposition, integration, and
  Plan maintenance within that direction.
- Last updated: 2026-08-25.
- Current task: [Implement References](tasks/read-only/references-command.md) is
  Complete and squash-integrated into local `develop` at `09f00ef`, with exact
  tree equality to accepted feature tip `1312368`. Public Red `43b75f3`, Green
  `23d5e2e`, managed `1270/1270`, published process, no-write, architecture,
  protected-surface, and supported local `linux-x64` Native AOT gates pass.
  [Improve The
  Repository-Root CLI Developer Workflow](tasks/repository-root-developer-workflow.md)
  is Complete and its accepted changes are included in that baseline.
- Current step: Discuss the remaining task order and parallelization boundaries
  with the maintainer before selecting another Active Task. Context is now
  dependency-eligible; Extension discovery remains an independent candidate, but
  neither starts implicitly. Standard SDK publication restored the exact portable
  `Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` pack without a
  project workaround. The explicit `linux-x64` build passed with zero warnings
  and errors; build-selected References EndToEnd passed `12/12`; Native AOT
  Integration passed `308/308`; and Native AOT EndToEnd passed `82/82`, all with
  zero skips. Root tooling, automatic managed `open-forge-dev` publication,
  build-selected native EndToEnd discovery, temporary compatibility-name routing,
  WSL portability, and obsolete preserved-test removal remain implemented.
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
  mutation safety boundary, six native RIDs, package journey, support floor,
  supply-chain artifact, and release gate has reproducible evidence and maintainer
  acceptance.
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
5. Implement read-only commands in dependency order. Establish neutral mechanical
   foundations at the nearest shared scope when several accepted program outcomes
   require them, even when implementation order exposes one consumer first;
   promote semantic facts only after consumers prove identical meaning. Find's
   source catalogue is sequential; no shared source mutation runs in parallel.
   References may begin only after the source and document facts it needs are
   accepted.
6. Establish lock, lifecycle, mutation, recovery, and Git foundations before the
   first mutating command.
7. Implement mutations from narrow route operations to extension and root
   lifecycle operations.
8. Implement aggregate status, diagnosis, repair, and cleanup only after all state
   producers exist.
9. Complete package, six-RID, supply-chain, support-floor, documentation, and
   release evidence without partial publication.

Each step ends in one inspectable commit. Architecture and cross-cutting callable
contracts stay with the Mastermind. A smaller implementer receives one closed
child Task and exact predecessor outputs. A reviewer receives the exact commit or
diff, parent requirements, and claimed evidence.

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
| Six native runners                 | Final RID and support-floor evidence                   | CI                     | D1                     | Release Task     |
| Real OS temporary filesystems      | Identity, containment, mutation, and no-write evidence | TestSupport            | F4 and commands        | Owning Task      |
| Git repositories                   | Mutation and recovery evidence                         | Isolated fixtures      | M1 onward              | Owning Task      |
| Preserved test inventory           | Candidate expectations and fixtures                    | `src/cli/tests/`       | Relevant command Tasks | Task creator     |
| Independent advisors               | Named architecture or safety uncertainty only          | Optional               | Decision points        | Mastermind       |
| Bounded implementers and reviewers | Closed implementation and fresh diff review            | After Task readiness   | Commands               | Mastermind       |

## Work Graph

| ID  | State    | Action and observable result                                                                                                                                                                                                                                                                                                                                                                                                      | Depends on                              | Lane        | Task group           | Verification                                                |
| --- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------- | ----------- | -------------------- | ----------------------------------------------------------- |
| S0  | Complete | Preserve WIP, define architecture delegation rules, and reset old production                                                                                                                                                                                                                                                                                                                                                      | None                                    | Sequential  | Historical           | Commits `4b873de`, `aa7d178`, `40ba03e`                     |
| S1  | Complete | Define the complete top-down Architecture and executable Plan                                                                                                                                                                                                                                                                                                                                                                     | S0, P1-P4                               | Sequential  | Program              | Current sources and link checks                             |
| S2  | Complete | Author the complete hierarchical Task set with closed foundation and command boundaries                                                                                                                                                                                                                                                                                                                                           | S1                                      | Sequential  | `tasks/`             | Task graph audit and backlinks                              |
| F1  | Complete | Create the scoped C# workspace, project graph, dependencies, artifacts, and preserved-test quarantine                                                                                                                                                                                                                                                                                                                             | S2, P5                                  | Sequential  | Foundation           | Restore/build topology and no root C# files                 |
| F2  | Complete | Implement Shell definitions, invocation, composition contracts, parser, pipeline, output, and serialization with no command                                                                                                                                                                                                                                                                                                       | F1                                      | Foundation  | Foundation           | Unit and integration evidence                               |
| F3  | Complete | Implement the thin root host and explicit route-free composition                                                                                                                                                                                                                                                                                                                                                                  | F2                                      | Foundation  | Foundation           | Managed process and terminal evidence                       |
| F4  | Complete | Implement workspace, filesystem identity, typed reads, and physical-containment foundations                                                                                                                                                                                                                                                                                                                                       | F2                                      | Foundation  | Safety foundation    | Real-OS matrix including leave-and-reenter                  |
| F5  | Complete | Establish active Unit, Integration, EndToEnd, TestSupport, Native AOT, and CI foundations                                                                                                                                                                                                                                                                                                                                         | F1-F4                                   | Sequential  | Foundation           | Managed and published process evidence                      |
| G1  | Complete | Accept the actual command-free architectural foundation                                                                                                                                                                                                                                                                                                                                                                           | F1-F5                                   | Sequential  | Foundation gate      | Full diff, dependency audit, AOT execution                  |
| R1  | Complete | Accept the complete `route list` slice and keep shared route facts local until route inspect proves identical consumers                                                                                                                                                                                                                                                                                                           | G1                                      | Sequential  | Route discovery      | Complete contract and public scenario                       |
| R2  | Complete | Split and close route-inspect child Tasks before implementation; then implement and accept `route inspect` and promote proved shared route facts                                                                                                                                                                                                                                                                                  | R1                                      | Sequential  | Route discovery      | List and inspect regressions                                |
| GI1 | Complete | Remediate parser behavior, improve active-test architecture, and accept the closed callable/root-host structure before the next product Task                                                                                                                                                                                                                                                                                      | R2                                      | Sequential  | Generic improvement  | Beginning/final complete suites and focused phase evidence  |
| Q1  | Complete | Complete Find's source catalogue, corrected query operation, and presentation children in order. Children 1 and 2 are accepted, and Child 3 plus the Find parent are Complete and accepted in the commit containing this record update. The no-op Purple acceptance is recorded at exact `426d4f5`; the final managed/native gate and package, artifact, static, no-write, Route/Shell/root, and generated-routing audits passed. | GI1, P6                                 | Sequential  | Source queries       | Contract, CommonMark, process, AOT                          |
| GI2 | Complete | Complete the accepted behavior-neutral Modern C# Improvements batches and final managed, Native AOT, package, audit, and public no-write gates.                                                                                                                                                                                                                                                                                   | Q1 Child 1 acceptance                   | Sequential  | Generic improvement  | Final gate and integrated reviews pass                      |
| DX1 | Complete | Move .NET workspace controls and artifacts to the repository root, make ordinary EndToEnd builds publish and discover `open-forge-dev`, remove obsolete preserved tests, and repair temporary compatibility-name routing.                                                                                                                                                                                                         | Q1, GI2                                 | Sequential  | Developer workflow   | Root build/test, environment-free EndToEnd, routing, review |
| Q2  | Complete | Public Red `43b75f3`, Green `23d5e2e`, managed `1270/1270`, public no-write, and supported local `linux-x64` root, Integration, and EndToEnd Native AOT execution pass.                                                                                                                                                                                                                                                                | GI1, Q1 source/document acceptance, DX1 | Sequential  | Source queries       | Contract, process, no-write, and Native AOT execution pass  |
| Q3  | Pending  | Implement and accept `context` after Q1 and Q2 facts stabilize                                                                                                                                                                                                                                                                                                                                                                    | Q1, Q2                                  | Sequential  | Context              | Ordered context and token evidence                          |
| E1  | Pending  | Implement and accept `extension list` and `extension inspect`                                                                                                                                                                                                                                                                                                                                                                     | G1, GI1                                 | Read-only C | Extension discovery  | Catalogue and package-source evidence                       |
| I1  | Pending  | Implement and accept `index`                                                                                                                                                                                                                                                                                                                                                                                                      | Q1-Q3, R2                               | Sequential  | Generated navigation | Idempotence and unchanged-authority evidence                |
| M1  | Pending  | Implement shared lock, lifecycle, mutation, recovery, and Git foundations                                                                                                                                                                                                                                                                                                                                                         | I1, E1                                  | Sequential  | Mutation foundation  | Direct failure and crash-boundary evidence                  |
| M2  | Pending  | Implement route init/create/update/move/remove in dependency order                                                                                                                                                                                                                                                                                                                                                                | M1, R2, I1                              | Sequential  | Route mutation       | Per-command public and recovery evidence                    |
| M3  | Pending  | Implement extension create and root install/update                                                                                                                                                                                                                                                                                                                                                                                | M1, E1, I1                              | Sequential  | Lifecycle mutation   | Package, lifecycle, and workspace evidence                  |
| M4  | Pending  | Implement extension install/update/remove                                                                                                                                                                                                                                                                                                                                                                                         | M3                                      | Sequential  | Extension mutation   | Collision, recovery, and catalogue evidence                 |
| O1  | Pending  | Implement status and doctor from all produced facts                                                                                                                                                                                                                                                                                                                                                                               | M2-M4                                   | Sequential  | Operations           | Complete aggregate and diagnostic evidence                  |
| O2  | Pending  | Implement repair and cleanup                                                                                                                                                                                                                                                                                                                                                                                                      | O1                                      | Sequential  | Operations           | Plan/apply/recovery and idempotence evidence                |
| D1  | Pending  | Implement wrappers, package graph, six-RID CI, supply chain, support floors, and docs                                                                                                                                                                                                                                                                                                                                             | O2                                      | Delivery    | Distribution         | Packed journeys and native matrix                           |
| A1  | Pending  | Run final whole-program acceptance and local integration                                                                                                                                                                                                                                                                                                                                                                          | D1                                      | Sequential  | Acceptance           | Maintainer acceptance; no partial release                   |

### Parallel Lanes

Parallelism begins only after the shared predecessor is committed and each lane
has non-overlapping production and test ownership.

| Lane        | Steps                          | May start when                                                                                                 | Owned surfaces                                       | Shared dependency                                 | Integration point |
| ----------- | ------------------------------ | -------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------- | ----------------- |
| Foundation  | F2, F4 preparation             | F1 complete; callable contracts frozen by Mastermind                                                           | Separate Shell and Framework capability paths        | Core models and project graph                     | F5                |
| Find        | Q1                             | GI1 and P6 complete; Child 1 and Modern C# are accepted; Child 2 records its focused Preflight before mutation | Find source catalogue, query, and presentation paths | Neutral source/document facts owned by Mastermind | Q1 acceptance     |
| References  | Q2                             | GI1 complete and the Q1 source/document facts it needs are accepted                                            | References command paths; no shared-source mutation  | Accepted neutral source/document facts            | Q2 acceptance     |
| Read-only C | E1                             | G1 and GI1 complete; extension source contract frozen                                                          | Extension List/Inspect roots                         | Shell and filesystem foundation                   | M1                |
| Delivery    | CI, packages, docs preparation | O2 behavior complete; release contracts frozen                                                                 | Separate workflow, wrapper, and docs paths           | Published native artifacts                        | D1 acceptance     |

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
- End with managed, process, unchanged-state, AOT, diff, and architecture evidence.

### Mutation And Delivery Steps

- Mutation Tasks separate planning from application and prove revalidation after
  lock acquisition.
- Recovery and lifecycle schemas are frozen by the Mastermind before command
  implementation.
- Delivery Tasks consume accepted binaries. Wrappers never reproduce behavior.

## Decision Points

| ID  | Decision                                                                             | Current direction                                           | Decision-maker                       | Needed before                | Result if reopened                                 |
| --- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------- | ------------------------------------ | ---------------------------- | -------------------------------------------------- |
| D1  | Can the managed BCL prove required component-wise physical identity on every target? | Prove with real OS and AOT evidence; no interop             | Maintainer after Mastermind evidence | F4 acceptance                | Narrow Architecture return                         |
| D2  | Does `route init` gain a Framework-shape mode?                                       | Deferred; current command contract remains authoritative    | Maintainer                           | M2 route-init Task           | Update contract, architecture, Tasks, and evidence |
| D3  | Does a dependency version need replacement?                                          | Keep accepted exact versions until evidence requires change | Maintainer                           | Owning foundation/command    | Focused dependency decision and full AOT proof     |
| D4  | Do perspectives, Tasks, or Plans become Framework primitives?                        | No; continue local trial                                    | Maintainer                           | After several complete Tasks | Separate Framework proposal, not CLI scope         |

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

| Gate           | Inputs                                 | Verification                                                                                                  | Pass condition                                                        | Resulting update         |
| -------------- | -------------------------------------- | ------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------ |
| VG0 Planning   | Architecture, Plan, Task hierarchy     | Link, hierarchy, dependency, scope, and stop-condition audit                                                  | Every implementation Task is closed or explicitly blocked             | Foundation authorized    |
| VG1 Foundation | F1-F5                                  | Restore, format, build, unit, integration, end-to-end host, dependency audit, local AOT publish and execution | Clean route-free architecture with no probe or root C# clutter        | G1 accepted              |
| VG2 Command    | One command and affected shared facts  | Focused managed tests, full regressions, public process scenario, unchanged-state check, local AOT            | Contract complete with no architectural debt deferred to next command | Next command authorized  |
| VG3 Mutation   | M1 and one mutation command            | Failure matrix, lock/revalidation, planned effects, Git/recovery, idempotence, process and AOT                | No unverified partial state or hidden lifecycle behavior              | Next mutation authorized |
| VG4 Delivery   | Complete commands and release surfaces | Six native RIDs, support floors, packed wrappers, checksums, signatures, SBOM, provenance, attestation, docs  | Complete non-shipping candidate                                       | Final acceptance         |
| VG5 Release    | VG4 and maintainer review              | Main-only release procedure and public smoke tests                                                            | Maintainer explicitly accepts shipping release                        | Release and closeout     |

## Coordination And Continuity

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
