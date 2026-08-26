---
open-forge:
  description: Parent outcome, scope, authority, and acceptance for the complete replacement CLI program
  tags: [Memory, Working, CLI, Task, Program, Architecture, Development, Contextual, Active]
---

# Complete The Replacement CLI

## Task State

- State: Active. Find, References, Context, Extension List, Modern C#
  Improvements, and the repository-root developer workflow are accepted.
  Context is squash-integrated into local `develop` at `ca097a2`; Extension List
  is integrated at current clean baseline `db0d39a`, whose tree exactly matches
  rebased integration tip `5e6babf`. The combined managed Unit `978/978`,
  Integration `354/354`, EndToEnd `111/111`, Native AOT Integration `354/354`,
  and Native AOT EndToEnd `111/111` gates pass with zero skips. Generated
  Navigation is paused after its first review correctly proved that native Skill
  metadata must move from Route-local parsing into a neutral Framework fact. The
  bounded
  [Routed Authored Metadata](read-only/routed-authored-metadata-foundation.md)
  foundation is the current sequential prerequisite; Generated Navigation then
  rebases and resumes. Public Index mutation remains blocked on the accepted
  lock/recovery dependency-order decision. Extension Inspect's exact contract is
  integrated at `92313a0`, its implementation-ready state is `db037ee`, and its
  production lane is independent of the routed-metadata prerequisite. The
  broader replacement CLI program remains Active.
- Responsible role: Overseer.
- Task source: This file.
- Last updated: 2026-08-26.

## Problem Statement

Open Forge has accepted command contracts, a complete top-down C# Architecture,
and active replacement source for the foundation, Route List, Route Inspect, and
Find. Remaining commands and delivery surfaces still require implementation and
acceptance. The removed first implementation remains evidence that command-first
delegation can create incompatible local architecture and miss system safety
boundaries.

## Expected Outcome

One complete, deterministic, Native-AOT executable implements every retained
command and ships through thin wrappers with six-RID, support-floor,
supply-chain, documentation, and release evidence. Architecture and command
behavior remain explicit, locally navigable, and safe to extend.

## Relationships And Backlinks

| Relationship        | Link                                                                                     | Relevance                                             |
| ------------------- | ---------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| Plan                | [Development Plan](../plan.md)                                                           | Defines sequence, dependencies, gates, and resumption |
| Checkpoint          | [CLI Development Checkpoint](../../checkpoints/cli-development.md)                       | Defines current state and next action                 |
| Child Tasks         | [Task index](_tasks.md)                                                                  | Routes every implementation group                     |
| Historical evidence | [Implementation Reset](../../../archived/cli-release/implementation-reset-2026-08-21.md) | Preserves lessons without constraining design         |

## References And Authority

| Source                                                                                        | Question it answers                             | Status                             | Change boundary                                      |
| --------------------------------------------------------------------------------------------- | ----------------------------------------------- | ---------------------------------- | ---------------------------------------------------- |
| [CLI Architecture](../../../crystallized/documents/cli/architecture.md)                       | How is the system structured?                   | Accepted current architecture      | Mastermind only through explicit architecture change |
| [Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md)           | Where is command meaning defined?               | Accepted current contract map      | Not by implementation Tasks                          |
| [Detailed Contracts](../../../crystallized/documents/cli/contracts/_contracts.md)             | What does each command do?                      | Accepted current product contracts | Only through maintainer decision                     |
| [Shared Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md) | What behavior crosses commands?                 | Accepted current contract          | Only through maintainer decision                     |
| [Program Architecture Directive](../../../../directives/program-architecture.md)              | Who owns architecture and delegation readiness? | Binding Directive                  | Not by child Tasks                                   |
| [CLI Implementation Directive](../../../../directives/open-forge/cli/implementation.md)       | Which implementation rules apply?               | Binding Directive                  | Not by child Tasks                                   |

## Scope

### Included

- The complete retained command tree and cross-command foundations.
- Repository-root .NET controls and artifacts, plus `src/cli/` root, Core, active
  tests, and package source.
- Native AOT, CI, packages, supply-chain evidence, documentation, and release.
- Necessary current Architecture, Plan, Task, Checkpoint, Directive, Pattern, and
  dogfooding updates.

### Excluded

- Frozen `src/cli-mvp/` source, tests, build, and behavior.
- Legacy compatibility, migration, aliases, or fallback dispatch.
- Partial publication or a command subset release.
- Framework product behavior not accepted by current contracts.
- Remote publication before the final delivery Task explicitly authorizes it.

### Constraints

- All replacement projects, C# source, and tests stay below `src/cli/`; shared
  .NET workspace controls and artifacts remain at the repository root.
- The root host remains thin and Core remains independent of it.
- Real `System.IO`, managed BCL-first safety, source generation, trimming, and
  Native AOT are hard boundaries.
- Each delegated Task is closed before implementation begins.

## Requirements

- CLI-PROG-001: Implement the Architecture without local substitute foundations.
- CLI-PROG-002: Satisfy every detailed command and shared operation contract.
- CLI-PROG-003: Keep each command complete before a later command consumes its
  facts or promotes its support.
- CLI-PROG-004: Keep read-only, mutation, lifecycle, recovery, packaging, and
  release effects within their accepted boundaries.
- CLI-PROG-005: Preserve reproducible managed, real-OS, process, Native AOT,
  package, and release evidence.
- CLI-PROG-006: Keep current sources, Plan, Tasks, and Checkpoint aligned at every
  integration gate.

## Acceptance Evidence

| Acceptance condition            | Evidence                                                                     | Verifier      |
| ------------------------------- | ---------------------------------------------------------------------------- | ------------- |
| Every child outcome is accepted | Task index with Complete or deliberate Cancelled state                       | Mastermind    |
| Complete contract coverage      | Command-to-evidence matrices and public scenarios                            | Owning Tasks  |
| Architecture remains coherent   | Dependency, source-locality, project, and whole-system review                | Mastermind    |
| Native delivery is complete     | Six native RIDs, support floors, packed wrappers, and supply-chain artifacts | Delivery Task |
| Release is explicit             | Maintainer acceptance and main-only release record                           | Maintainer    |

## Prerequisites And Dependencies

The Architecture, Plan, Directives, and greenfield reset are complete. Child
groups add their own predecessor dependencies. No command implementation may
bypass the Foundation group.

## Risks And Safeguards

| Risk                                    | Signal                                     | Safeguard                        | Stop condition                |
| --------------------------------------- | ------------------------------------------ | -------------------------------- | ----------------------------- |
| Architecture fragments across commands  | Shell or shared policy appears in a leaf   | Parent integration review        | Return to Architecture        |
| Task volume becomes stale               | State or paths diverge from actual work    | Update at integration gates only | Consolidate before continuing |
| Passing local tests hide system defects | Missing process, safety, or AOT proof      | Parent acceptance matrix         | Child remains incomplete      |
| Historical code anchors design          | A Task copies old source or project layout | Contract-first test disposition  | Reject and replan leaf        |

## Progress And Evidence

- Current result: Architecture, Plan, reset, Task governance, Foundation, and
  Route Discovery are accepted. Route List is integrated at `edca509`; Route
  Inspect is squash-integrated at `bd5d280`. Generic improvements are Complete and
  squash-integrated at `063c59d`; their complete beginning suite passed. Parser
  remediation is Complete through Purple `d445e20` and
  focused public acceptance. [Improve Active-Test Architecture](generic-improvements/active-test-architecture.md)
  is Complete at `e277227`, with its accepted Working boundary at clean `eb2e336`,
  focused Integration `129/129`, freshly published managed EndToEnd `57/57`, zero
  skips, and accepted locality review. [Refine Callable And Project
  Architecture](generic-improvements/callable-project-architecture.md) is Complete
  at `cc2387d` with focused Unit `53/53`,
  Integration `49/49`, freshly published managed EndToEnd `57/57`, zero skips, and
  accepted architecture review. All generic child Tasks are Complete.
- Generic integration: `063c59d` exactly matches accepted feature tip `a107afe`.
- Find planning result: The reviewed Find authority and three-child decomposition
  are recorded at accepted planning-boundary commit `b2e3106` on clean branch
  `feature/cli-find`; `develop` remains `e77902a`. The exact production/source
  baseline remains `063c59d`; no Find command production or test code existed at
  that baseline.
  Three read-only explorers and one grounded advisor inspected the current Route
  List, Route Inspect, and Framework code from that exact boundary without edits.
- Child 1 is Complete and accepted at exact commit `96fe413` (`Accept Find source
catalogue`). Its
  [acceptance record](read-only/find-source-catalogue.md)
  contains the neutral authority, Route-local projections and policy, final
  corrections, full gate, no-write gate, and recorded cancellation limitation.
- The solution-wide [Modern C# Improvements](modern-csharp-improvements.md) is
  Complete and accepted at exact `a1cbf09`. Its Preflight was
  accepted at `55eb82e`; Framework modernization at `a90af59`; Shell/root
  modernization at `fe10525` (`Modernize Shell nullable flow`); Route Inspect/family
  modernization at `62a1dd9` (`Modernize Route Inspect nullable flow`); Route List
  modernization at `273eb45` (`Modernize Route List nullable flow`); and
  Tests/support modernization at exact clean source commit `6af5fb1`
  (`Modernize test support nullable flow`). All five ordered mutation batches are
  accepted, and no task-local correction pass was consumed.
  Its exact production
  ownership is `Commands/Route/Shared/**`, `Commands/Route/Inspect/**`, and the
  exact `RouteBinding.cs` and `RouteDefinitions.cs` files, with the latter two
  unchanged. Exactly 32 production C# files changed. No tests, projects, packages,
  dependencies, generated files, or configuration changed. The Route List
  modernization batch is Complete and accepted at exact commit `273eb45`
  (`Modernize Route List nullable flow`). Its exact scope is
  `Commands/Route/List/**`; exactly 18 production C# files changed. No tests,
  projects, packages, dependencies, generated files, configuration, or product
  contracts changed. No output or test expectation changed. Route List Unit is
  `117/117`, Integration is `89/89`, and fresh managed published `CliProcessTests`
  EndToEnd is `26/26`, with zero skips. Tests/support modernization is Complete
  and accepted at exact clean source commit `6af5fb1`. It changed exactly 40 active
  test/support C# files: Unit 24, Integration 14, EndToEnd 1, and TestSupport 1.
  Test identities, expectations, tiers, order, fixtures, and count are unchanged.
  Test postfix suppressions are `169 → 8`; test
  `ArgumentNullException.ThrowIfNull` calls fell from `21` to `8`; and production
  remains at `0` suppressions and `297` guards. Full
  managed Unit `617/617`, Integration `241/241`, and EndToEnd `57/57` pass with
  zero skips. The final managed, Native AOT, package, audit, and public no-write
  gate is Complete and accepted at the `a1cbf09` boundary. Find Child 2 is Complete
  and accepted at exact `ff7ce3f` (`Accept Find query operation`). Child 3 focused
  Preflight is accepted at exact `28d316a`, Gray at exact `a76a217`, and the
  original Red evidence packet at exact `22d3bff`. Its historical focused Unit was
  `202` total with `92` pass and `110` intentional failures; Integration was `43`
  total with `23` pass and `20` intentional failures; and published EndToEnd was
  `13` total with `13` intentional failures, all with zero skips. The Integration
  metadata correction is accepted at exact `6a9a0de`, and the mirrored EndToEnd
  metadata correction is accepted at exact `eea3d59` (`Complete Find presentation
metadata evidence`). Its post-commit Red reproduction succeeded as intentional
  Red: managed non-AOT `win-x64` publish passed, and published Find EndToEnd was
  `13` total with `13` intentional failures and zero skips, all terminating at
  absent Green root registration. Historical pre-correction review found the
  frozen-contract mismatch for human `\t` versus lowercase `\uXXXX` controls and
  diagnostic escaped-code-unit slicing that allowed partial `\\`, `\"`, or
  `\uXXXX` tokens.
  This returned narrowly to Red without changing contracts, Child 2,
  Integration, EndToEnd, production, package/project, generated routing, or Route
  behavior. Supplemental escaping Red correction is accepted at exact `a865fd1`
  (`Correct Find escaping evidence`). It changes only
  `FindHumanRenderingRedTests.cs` and `FindDiagnosticsAndHelpRedTests.cs`, sets
  TAB to `\u0009`, and adds four Windows renderer-level truncation-boundary cases
  that independently validate complete escape-token grammar, bounds, one-line
  output, and no payload leak. Its clean detached-worktree evidence includes locked
  restore, warning-free Release build, format verification, full focused Unit
  `206` total with `92` pass and `114` intentional Gray-boundary failures, and
  narrow selected evidence `6` total with `6` intentional failures, all with zero
  skips; fresh test-only correctness review is `PASS` with no material optional
  improvement.
- The pre-correction narrow Green selection for TAB plus four token-boundary cases
  was `6` total with `1` pass and `5` intended defect-exposing failures, zero
  skips.
- Green result: Child 3 Green is accepted at exact commit `cb7874c` (`Implement
Find presentation`) over corrected Red `a865fd1`. It is the production/root-
  composition-only Green; no test, support, contract, project, package,
  configuration, generated-routing, or Route behavior change enters Green. The
  exact scope is 15 production paths: Find binding, request, result builder, and
  validation; compact, expanded, JSON, diagnostic, help, and shared text
  escaping; Shell direct-root command tree and root factory; and root composition.
  It keeps one symbol graph and operation/result flow, binding-owned help,
  source-generated concrete `FindJsonDocument`, explicit malformed-content request
  presence, deterministic bounded human/JSON/diagnostic behavior, and exact one
  root Find registration. It adds no reflection, second parser/operation/renderer
  catalogue, workspace writes, or Native AOT work. The correction encodes TAB as
  `\u0009` and truncates only at complete escaped-token boundaries.
- Fresh Green evidence passes locked restore, warning-free Release solution build,
  format verification, and `git diff --check`; focused Find+direct-root Unit
  `206/206`; focused Find+generated-serialization Integration `43/43`; affected
  Shell+Route Unit `347/347`; affected Shell+Route Integration `160/160`; managed
  non-AOT `win-x64` publish; and published Find EndToEnd `13/13`, all with zero
  skips. Fresh final bounded Green correctness review is `PASS` with no material
  findings. It verifies corrected escaping, Find binding, explicit malformed-
  content state, root leaf registration, renderer dispatch, JSON projection/source
  generation, diagnostics, help, direct Shell integration, one operation/result
  flow, and no protected-surface drift. Native AOT is intentionally not claimed.
- Earlier local-improvement review found one material bounded Blue candidate only:
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
  removing duplicate mapping ownership without adding indirection. Fresh Purple
  assessment ran read-only from exact clean Blue `3f81e76` against the exact ten
  Find Child 3 test/support surfaces. Verdict: `NO_MATERIAL_IMPROVEMENTS`; the
  no-op Purple acceptance is recorded at exact `426d4f5`. No test/support,
  production, contract, project, package, configuration, generated, Route, Shell,
  or root file changed, and no Purple code/test commit was manufactured. Unit
  tests and typed `FindPresentationTestData` are coherent at nearest scope;
  splitting the fixture fragments authority. Integration and EndToEnd
  workspace/assertion helpers are intentionally project-local. The supplemental
  diagnostic token-boundary fixture/parser is local, independent, structurally
  sound, and proves complete escapes, bounds, one-line output, and payload
  exclusion. Minor wrappers and repeated status expectations are preference-only
  independent contract oracles. Purple evidence is focused Unit `206/206`, focused
  Integration/serialization `43/43`, and published Find EndToEnd `13/13`, all with
  zero skips; source diff/check against `3f81e76` was clean/empty. No Native AOT
  claim was made, and the record-only commit was not a test change.
- At the accepted Find boundary, the Read-Only group and replacement CLI program
  remained Active, and References had not started in that session.
- Final generic feature gate: Warning-free build, format/diff, managed Unit `580/580`,
  Integration `213/213`, EndToEnd `57/57`, local `win-x64` Native AOT root with
  managed EndToEnd `57/57`, Native AOT Integration `213/213`, Native AOT EndToEnd
  `57/57`, package/artifact/public audits, and integrated review pass with zero skips
  and no blocker.
- Modern C# final gate: From exact clean source commit `6af5fb1`, the warning-free
  Release build, format/diff, zero informational `CA1062`/`CA1510`/`CA2264`, full
  managed Unit `617/617`, Integration `241/241`, freshly managed-published
  EndToEnd `57/57`, focused Source/Route Unit `562/562`, focused Integration
  `183/183`, local `win-x64` Native AOT root, native-root managed EndToEnd `57/57`,
  Native AOT Integration `241/241`, and Native AOT EndToEnd `57/57` all pass with
  zero skips. Managed/native Route List and Route Inspect no-write cases pass with
  exits/statuses `5/blocked` and `3/incomplete`, empty stderr, typed JSON, and
  unchanged snapshots. Package, project/dependency/config/generated, static,
  changed-path, and integrated review audits pass; the detailed record preserves
  the exact graph, counts, frozen test injections, and `win-x64` limitation.
- Find Child 3 and the Find parent are Complete and accepted in the commit
  containing this record update. The final package, artifact, static, public
  no-write, and protected-surface audits passed. The sole immediate continuation
  is to freshly verify `develop`, squash-integrate the accepted `feature/cli-find`
  tip into local `develop`, commit that one squash, prove exact tree equality, do
  not push, and halt. `CLI-EDGE-001` remains non-product only.

## Completion And Closeout

Complete only after final delivery and maintainer release acceptance. Consolidate
durable outcomes into current Architecture, contracts, Directives, Patterns, and
public documentation. Archive or prune temporary Plan, Task, and Checkpoint detail
that no longer earns its maintenance cost.
