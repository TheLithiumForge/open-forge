---
open-forge:
  description: Align CLI context loading, route inspection, and context measurements with scoped KeepInMind behavior
  tags: [Memory, Working, Contextual, CLI, Task, Loading, Continuity, Testing]
---

# Scoped Continuity Loading

## Task State

- State: Prepared for the user's implementer in a separate chat. Implementation has not started.
- Authority: The user's 2026-09-11 instruction to apply Task 28's scoped-loading wording and create a separate CLI implementation task.
- Responsible role: The user's designated CLI implementer, upon activation.
- Task source: This file. Register a permanent numeric ID in the current [project control ledger](../project-control.md) when scheduling the task; none is reserved by this isolated draft.
- Phase, milestone, and execution horizon: Define at activation against the actual implementation baseline.
- Placement: Integrated with Task 28 after explicit user authorization. Implementation remains with the designated separate chat; it is not dispatched by this merge.
- Last updated: 2026-09-11.

## Problem And Expected Outcome

The Framework wording now gives `LoadNow` and `KeepInMind` the same parent and scope boundaries. The CLI still has a global continuity pass that discovers ordinary tagged records under inactive ancestors, loads those ancestors, and can include unrelated context. Route inspection and existing tests also describe the earlier global behavior.

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

| Surface | Starting point and responsibility |
| --- | --- |
| Shared loading | [SourceLoadingClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Loading/SourceLoadingClosureResolver.cs): replace global continuity activation with traversal through exposed parent entries. |
| Requested context | [ContextLoadingClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs): preserve startup plus explicit-selection behavior and applicable child loading. |
| Operational measurements | [RouteContextClosureResolver](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteContextClosureResolver.cs): trace its callers, including affected Status measurements, and align their context sets. |
| Route inspection | [RouteInspectReadingProfileBuilder](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Profile/RouteInspectReadingProfileBuilder.cs) and adjacent loading-facts traversal: reconcile reading events, selected/startup sets, and measurements. |
| Public explanations | Affected human and JSON projections, typed reading models, [CLI documentation](../../../../../docs/cli.md), and current command contracts: explain actual scoped behavior without retaining global promises. |

At preparation, the shared resolver calls `AddGlobalContinuity` after its initial traversal. Its visible-entry filter handles KeepInMind only when the target is an entrypoint. Removing the global pass alone would therefore omit ordinary tagged files even when their parents are loaded. Correct both sides of that relationship.

Route inspection has its own loading calculations. Update all affected consumers of the accepted behavior rather than assuming one resolver edit covers them. Reconcile shared responsibilities using the current architecture; these starting paths are an investigation map, not a frozen callable design.

Preserve unrelated command behavior, source and user-file safety, route validation, and deterministic output. Inactive sources must not enter startup or continuity measurements solely through their tags. Full-workspace structural diagnostics may still inspect them under their own command contracts. Treat unavoidable changes to public model shape or unrelated behavior as a decision to surface before relying on them.

## Acceptance Evidence

| Case | Required observation |
| --- | --- |
| Tagged ordinary record below an inactive on-demand ancestor | Startup context excludes the record, its previously inactive ancestors, and unrelated siblings that global recovery formerly exposed. |
| Explicitly selected scope | Its ancestor chain and applicable exposed LoadNow and KeepInMind entries are included, while unselected sibling scopes stay excluded. |
| Tagged ordinary record below an already-loaded parent | It is included through the parent entry without a global scan. |
| Tagged entrypoint | Its applicable children follow the same parent-driven rules in declared order. |
| Both loading tags, shared ancestors, and overwrites | Membership, ordering, reasons, and measurements remain coherent without duplicate emitted content; adjacent overwrites retain their existing role. |
| Unrelated inactive continuity source with broken loading metadata | It does not create a failure solely through global continuity discovery. Preserve independently required structural diagnostics. |
| Context, Route Inspect, and affected Status results | Startup membership, explicit-selection additions, continuity measurements, and human/JSON explanations agree with the revised contract. |
| Read-only execution | Existing no-write evidence continues to pass for the affected public commands. |

The existing [Route Inspect loading integration tests](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Profile/RouteInspectOperationLoadingIntegrationTests.cs) include `ContinuityAndOnDemandReasonsRemainIndependent`, which explicitly expects global file continuity. Replace obsolete expectations with evidence for the accepted behavior. Include decisive negative controls for inactive branches and positive controls for exposed ordinary records. The preparation inspection is static evidence, not a fresh runtime reproduction.

Run focused tests for the affected shared mechanism and command integration, then the current repository-required build, formatting, managed/native, and public-command gates appropriate to the change. Record exact commands, results, failures or skips, and the tested source/runtime identities in this task. Do not prescribe stale commands or accept old receipts as verification of the new candidate.

## Activation And Handoff

1. Follow normal repository bootstrap and the current relevant CLI, C#, and testing rules. Task 28's initial source-review context exception does not apply to this implementation task.
2. Confirm the assigned worktree, accepted integration baseline, and the presence of the scoped-loading wording. The review branch originally forked from `8a52ede13ab6d622578a6cfae04031a9a5b2eb2d`; this is provenance, not a required implementation base. Other CLI work has advanced separately.
3. Reconcile the current [program task](00-cli-development.md), [Plan](../plan.md), and command contracts before editing. Preserve other agents' work. Do not edit a concurrently owned worktree without an explicit handoff.
4. Establish the smallest coherent implementation and evidence plan, then complete the bounded correction in the separately assigned chat.
5. Return the reviewable diff, validation receipts, and remaining limitations. Update this task's state only with actual progress.

Completion requires aligned implementation and current explanations, passing applicable evidence, and a reviewed candidate. A wording-only change does not complete this task. No publication, global installation refresh, or merge to `develop` is authorized by this record. The user controls integration.

## Current Evidence And Next Action

Framework wording was applied in the review worktree on 2026-09-11. CLI implementation and tests remain unchanged. Next action: the user hands this task to their implementer in a separate chat using an agreed worktree and baseline.
