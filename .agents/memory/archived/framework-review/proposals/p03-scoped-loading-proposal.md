---
open-forge:
  description: Original scoped-loading wording proposal and the separate CLI implementation boundary
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# P03 — Scoped loading and the CLI boundary

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Status: wording patch approved and applied on 2026-09-11. The user explicitly chose to apply the Framework wording now and prepare a separate task for their CLI implementer in another chat. The [application receipt](../evidence/p03-application-receipt.json) verifies the six-file patch. The [Scoped Continuity Loading task](../../cli-development/tasks/scoped-continuity-loading.md) stays in this worktree until user-authorized integration; its implementation has not started.

## Applied wording

The [six-file patch](../evidence/p03-scoped-loading-proposal.diff) replaces global continuity loading with the accepted shared parent and scope boundary:

> #LoadNow and #KeepInMind operate through loaded parent routes. Neither tag activates an otherwise unselected ancestor or scope.

It treats tagged entrypoints and ordinary records alike. Read exposed continuity content when its parent loads, then refresh it at the established boundaries while its scope remains active. Selecting an on-demand route exposes its child loading rules. Recover active route chains after context restoration; do not reactivate unrelated scopes through continuity refresh.

The patch reconciles the shipped and dogfood loaders, current loading explanation, Framework architecture, loader maintenance contract, and the README's existing continuity bullet. It removes the inactive-ancestor exception and updates loading order rather than appending a competing rule. It adds no diagram. The shipped loader becomes 60 whitespace-separated words shorter relative to applied P02.

## Existing CLI behavior that needs to change with it

Static inspection found an explicit implementation of the old contract:

- [SourceLoadingClosureResolver.cs](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Loading/SourceLoadingClosureResolver.cs) (recorded line 51) invokes `AddGlobalContinuity` after initial traversal. That method scans ordinary tagged files across the source graph and loads their ancestor chains. Its visible-entry traversal separately limits KeepInMind handling to entrypoints at line 169. Removing the global pass alone would therefore lose exposed ordinary continuity records.
- [ContextLoadingClosureResolver.cs](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs) (recorded line 47) consumes the shared resolver. [RouteContextClosureResolver.cs](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteContextClosureResolver.cs) (recorded line 14) also consumes it, so impact assessment must include shared operational context measurements.
- [RouteInspectReadingProfileBuilder.cs](../../../../../src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Profile/RouteInspectReadingProfileBuilder.cs) (recorded line 105) gives routed ordinary KeepInMind files a global reading reason. Inspect's loading calculations and public explanations need alignment as well.
- [RouteInspectOperationLoadingIntegrationTests.cs](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Profile/RouteInspectOperationLoadingIntegrationTests.cs) (recorded line 99) explicitly tests preservation of global file continuity. This is evidence that current expectations encode the prior behavior, not a fresh test failure or a completed runtime reproduction.

The main checkout still contained the same global resolver path and named integration test at inspection time. The user explicitly accepted applying the wording first and assigning the matching code change to a separate implementer task. The known Framework/CLI mismatch remains open and is recorded in that task.

## Coherent next step

The matching implementation needs to include exposed tagged ordinary files in parent-driven traversal, remove activation of inactive ancestors, preserve inherited rules and adjacent overwrites, and reconcile selected-context traversal and inspection results. Verify inactive branch exclusion, inclusion after explicit scope selection, ordinary record and entrypoint parity, exposed child loading, and active continuity measurements. Inspect relevant command contracts and tests before choosing exact implementation changes.

The user selected separate CLI implementation. The new task records the accepted outcome, affected mechanisms and consumers, decisive positive and negative evidence, current baseline caveats, and completion criteria. It uses durable repository-relative references and is discoverable through the task catalog. No chat was launched, message sent, or CLI code changed. The task awaits the user's handoff to their implementer.

## Verification

The prose diff was inspected and checked before application. All six applied files match the proposed candidate hashes in the [source identities](../evidence/p03-scoped-loading-identities.json), and `git diff --check` passes. Main-checkout targets were unchanged by the application. The task and its catalog entry were added only in the review worktree. No CLI tests were run because no CLI code changed. Both diagram ideas remain deferred; no commits or merges occurred.
