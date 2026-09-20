---
open-forge:
  description: Align CLI context loading, route inspection, and context measurements with scoped KeepInMind behavior
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Loading, Continuity, Testing]
---

# Scoped Continuity Loading

## Task State

- State: Complete. The user authorized implementation and local squash integration on 2026-09-12.
- Authority: The user's 2026-09-11 instruction to apply Task 28's scoped-loading wording and create a separate CLI implementation task.
- Responsible role: Root, direct sequential implementation in the Task 29 worktree.
- Task source: Task 29 “Scoped Continuity Loading”, registered in the [project control ledger](../project-control.md).
- Phase, milestone, and execution horizon: phase 2/2, milestone 3/3. M1: correction and regression evidence; M2: aligned explanations and focused verification; M3: final managed/native gates and review.
- Placement: Prepared with Task 28; activated here by the user on 2026-09-12.
- Last updated: 2026-09-12.

## Problem And Expected Outcome

At preparation, the Framework wording gave `LoadNow` and `KeepInMind` the same parent and scope boundaries. The CLI retained a global continuity pass that discovers ordinary tagged records under inactive ancestors, loads those ancestors, and can include unrelated context. Route inspection and existing tests also describe the earlier global behavior.

Make CLI context selection, reading explanations, and affected context measurements follow the accepted scoped-loading contract. The Framework wording has been changed in the review worktree; the CLI mismatch remains open until this task is completed. Do not restore the former wording to make existing implementation or tests appear consistent.

## Accepted Behavior

- `LoadNow` and `KeepInMind` operate through loaded parent routes. Neither activates an otherwise unselected ancestor or scope.
- Tagged entrypoints and ordinary records share that boundary. Exposed KeepInMind content is read when its parent loads; explicitly selecting an on-demand scope exposes its applicable child loading rules.
- Continuity refresh applies while the scope remains active. A refresh does not reactivate unrelated scopes. The CLI handles startup and explicitly requested selection from its available inputs; do not invent persistent session tracking or infer task relevance.
- Preserve generated entry order, applicable ancestor rules, adjacent overwrites, source identity, and read-only behavior. Scope selection does not merge authority between separate route chains.
- Loading changes visibility and timing, not acceptance or behavioral authority. A tagged record does not become accepted because it was read.

The [loader](../../../../loader.md#defined-tags) defines the operative tag wording. The [loading and continuity document](../../../crystallized/documents/framework/routing/loading.md) explains the full contract, and its [maintenance contract](../../../crystallized/documents/maintenance/payload/agents/loader.md) records source alignment. The [shipped loader](../../../../../src/open-forge/.agents/loader.md) is the installable source. These sources, together with the user direction recorded above, establish this task independently of ignored review artifacts or conversation history.

## Implementation Scope

Update production behavior, directly affected command contracts and explanations, and decisive regression evidence for:

| Surface                  | Starting point and responsibility                                                                                                                                                                                                                                       |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Shared loading           | [SourceLoadingClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Loading/SourceLoadingClosureResolver.cs): replace global continuity activation with traversal through exposed parent entries.                                           |
| Requested context        | [ContextLoadingClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs): preserve startup plus explicit-selection behavior and applicable child loading.                                     |
| Operational measurements | [RouteContextClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteContextClosureResolver.cs): trace its callers, including affected Status measurements, and align their context sets.                      |
| Route inspection         | [RouteInspectReadingProfileBuilder](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Profile/RouteInspectReadingProfileBuilder.cs) and adjacent loading-facts traversal: reconcile reading events, selected/startup sets, and measurements. |
| Public explanations      | Affected human and JSON projections, typed reading models, [CLI documentation](../../../../../docs/cli.md), and current command contracts: explain actual scoped behavior without retaining global promises.                                                            |

At preparation, the shared resolver calls `AddGlobalContinuity` after its initial traversal. Its visible-entry filter handles KeepInMind only when the target is an entrypoint. Removing the global pass alone would therefore omit ordinary tagged files even when their parents are loaded. Correct both sides of that relationship.

Route inspection has its own loading calculations. Update all affected consumers of the accepted behavior rather than assuming one resolver edit covers them. Reconcile shared responsibilities using the current architecture; these starting paths are an investigation map, not a frozen callable design.

Preserve unrelated command behavior, source and user-file safety, route validation, and deterministic output. Inactive sources must not enter startup or continuity measurements solely through their tags. Full-workspace structural diagnostics may still inspect them under their own command contracts. Treat unavoidable changes to public model shape or unrelated behavior as a decision to surface before relying on them.

## Acceptance Evidence

| Case                                                              | Required observation                                                                                                                               |
| ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tagged ordinary record below an inactive on-demand ancestor       | Startup context excludes the record, its previously inactive ancestors, and unrelated siblings that global recovery formerly exposed.              |
| Explicitly selected scope                                         | Its ancestor chain and applicable exposed LoadNow and KeepInMind entries are included, while unselected sibling scopes stay excluded.              |
| Tagged ordinary record below an already-loaded parent             | It is included through the parent entry without a global scan.                                                                                     |
| Tagged entrypoint                                                 | Its applicable children follow the same parent-driven rules in declared order.                                                                     |
| Both loading tags, shared ancestors, and overwrites               | Membership, ordering, reasons, and measurements remain coherent without duplicate emitted content; adjacent overwrites retain their existing role. |
| Unrelated inactive continuity source with broken loading metadata | It does not create a failure solely through global continuity discovery. Preserve independently required structural diagnostics.                   |
| Context, Route Inspect, and affected Status results               | Startup membership, explicit-selection additions, continuity measurements, and human/JSON explanations agree with the revised contract.            |
| Read-only execution                                               | Existing no-write evidence continues to pass for the affected public commands.                                                                     |

The existing [Route Inspect loading integration tests](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Profile/RouteInspectOperationLoadingIntegrationTests.cs) include `ContinuityAndOnDemandReasonsRemainIndependent`, which explicitly expects global file continuity. Replace obsolete expectations with evidence for the accepted behavior. Include decisive negative controls for inactive branches and positive controls for exposed ordinary records. The preparation inspection is static evidence, not a fresh runtime reproduction.

Run focused tests for the affected shared mechanism and command integration, then the current repository-required build, formatting, managed/native, and public-command gates appropriate to the change. Record exact commands, results, failures or skips, and the tested source/runtime identities in this task. Do not prescribe stale commands or accept old receipts as verification of the new candidate.

## Activation And Handoff

1. Follow normal repository bootstrap and the current relevant CLI, C#, and testing rules. Task 28's initial source-review context exception does not apply to this implementation task.
2. Confirm the assigned worktree, accepted integration baseline, and the presence of the scoped-loading wording. The review branch originally forked from `8a52ede13ab6d622578a6cfae04031a9a5b2eb2d`; this is provenance, not a required implementation base. Other CLI work has advanced separately.
3. Reconcile the current [program task](00-cli-development.md), [Plan](../plan.md), and command contracts before editing. Preserve other agents' work. Do not edit a concurrently owned worktree without an explicit handoff.
4. Establish the smallest coherent implementation and evidence plan, then complete the bounded correction in the separately assigned chat.
5. Return the reviewable diff, validation receipts, and remaining limitations. Update this task's state only with actual progress.

Completion requires aligned implementation and current explanations, passing applicable evidence, and a reviewed candidate. A wording-only change does not complete this task. Publication and global installation refresh remain unauthorized. The user authorized local squash integration into `develop` during activation.

## Current Evidence And Next Action

The scoped-loading correction is complete. Both tags follow exposed parents in
startup and explicitly selected context. Route Inspect reports the exposing
parent and scoped refresh events. Status uses the corrected shared startup and
continuity sets. Both tags retain their reasons without duplicate source output.
The narrow LoadNow descendant measurement remains LoadNow-only.

The local worktree and squash-integration requirement is recorded in the
[Orchestration Directive](../../../../directives/hierarchical-orchestration.md).
No publication or global installation refresh was performed.

## Execution Capsule

- User authorization: implement locally on 2026-09-12. Root owns sequential work.
- Baseline: `f84fa53fff31d0492cfcd83f1d89ccb86a783c31`. Preserve existing tooling/package and Task 13 edits.
- Profile: standard bounded correction to accepted behavior. This read-only local development tool changes context visibility and measurements; Git diff is the recovery boundary. No mutation capability, dependency, public schema, session tracking, or architecture change is needed.
- Scope: shared loading, Context explicit closure, Route Inspect loading/profile/rendering, affected Status measurements, contracts and tests. Preserve the narrow LoadNow-only descendant measurement.
- Evidence: Unit project `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests`, Integration project `src/cli/tests/integration/OpenForge.Cli.IntegrationTests`, and published EndToEnd project `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests`. Focus Context, Route Inspect and Status. Shared loading and public reading explanations trigger final full managed and Linux x64 Native AOT gates after focused passes.
- Review budget: one root review of behavior, consumers, evidence and changed prose. No delegated implementation or additional review waves.
- Protected boundaries: unrelated dirty files and external/global installation effects. No publication or merge permission.

- Worktree correction: the user explicitly required isolated worktree execution and squash integration into local develop. Work now uses `<temp>/open-forge-task29`, branch `codex/task29-scoped-continuity`, from `f84fa53f`. Only Task 29 changes were transferred; all unrelated main-checkout edits were preserved. Local squash integration is authorized.

- Focused receipts: `<temp>/task29-focused-final-{0,1,2}.log`; Release build `<temp>/task29-build.log` has zero warnings/errors. Full gate receipts will bind the committed candidate and native artifact identities. Workspace dogfood startup selected 34 sources, with five applicable findings and no global-continuity findings; the inactive Framework loading document was excluded.

## Final Verification And Integration

- Tested implementation commit: `d32a6a0c`, based on `f84fa53f`.
- Integration is prepared in `codex/task29-integration` from current `develop`
  `36ec9843` after the concurrent tooling commit. Its CLI, Framework payload, Extension sources, .NET build props,
  central packages and SDK configuration match the tested candidate exactly.
  The commit containing this closeout is the local squash-integration result.
- Toolchain: .NET SDK `10.0.111`, Linux x64, product version `0.0.0`.
- `npm run restore -- --offline` used cached dependencies. Vulnerability audit
  was unavailable in this offline check. Release builds had zero warnings/errors.
- `npm run check:dotnet` and `git diff --check` passed. Added local Markdown
  links resolve. Root reviewed the actual source, models, consumers, tests and
  changed prose; no loading finding remains.
- Focused execution used each tier's own built assembly with
  `--filter-trait Feature=context Feature=route-inspect Feature=status-command`,
  `--minimum-expected-tests 1 --parallel none --fail-warns on --fail-skips on
--no-ansi --progress off`.

| Gate                                     | Passed | Failed | Skipped |
| ---------------------------------------- | -----: | -----: | ------: |
| Focused Unit                             |    196 |      0 |       0 |
| Focused Integration                      |    155 |      0 |       0 |
| Focused published EndToEnd               |      9 |      0 |       0 |
| Full managed Unit                        |   3226 |      0 |       0 |
| Full managed Integration                 |   1721 |      3 |       0 |
| Full managed published EndToEnd          |    111 |      0 |       0 |
| Full Native AOT Integration              |   1721 |      3 |       0 |
| Full Native AOT EndToEnd                 |    111 |      0 |       0 |
| Full managed EndToEnd against native CLI |    111 |      0 |       0 |

The full gate is **not entirely green**. These same three failures reproduce on
unchanged baseline `f84fa53f` (24 selected tests, 21 passed, three failed, zero
skips), and the native failures match the managed failures exactly:

1. `UpdatePlanningIntegrationTests.ProjectsGeneratedNavigationIntoOneCoalescedPhysicalEffect`:
   the fixture replaces wording removed by Task 28, leaving only a generated-region change.
2. `EmbeddedExtensionCatalogueIntegrationTests.EmbeddedCatalogueMatchesAuthoredPackagesAssetsAndHashes`.
3. `EmbeddedExtensionCatalogueIntegrationTests.EmbeddedCatalogueArchiveDecodesToCanonicalAuthoredAssets`.

The two catalogue failures belong to the separate
[Extension Catalogue Synchronization](extension-catalogue-synchronization.md)
task. All three predate this change and were preserved. Acceptance here covers
scoped loading and unchanged baseline failures, not a clean repository-wide gate.

`npm run build:native -- --rid linux-x64` could not resolve the old TypeScript
compiler entrypoint against the concurrently upgraded shared Node dependencies.
No tooling was changed to hide that mismatch. The equivalent cached .NET
publication and test commands ran directly, with `--no-restore`, for the CLI,
Integration and EndToEnd projects. All three native publications succeeded
without warnings; package-wrapper qualification is not claimed.

Canonical local receipts are retained under
`<temp>/open-forge-task29/artifacts/task29/`: `acceptance.json`,
`native-receipt.json`, `native-sha256.json`, the exact `native-gates.py` command
sequence, build/format/focused/baseline logs, and native CTRF reports. Full
managed CTRF reports are under
`<temp>/open-forge-task29/artifacts/delivery/managed-reports-rMDUMa/`.
The native CLI SHA-256 is
`fa338cd79bd8d0a2c3c9c6b92acd510a57dc9b32bc4f3b96a8ced5af38e24fdf`.
