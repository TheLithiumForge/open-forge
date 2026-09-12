---
open-forge:
  description: Regenerate and verify the native CLI's embedded catalogue from the reviewed first-party Extension sources
  tags: [Memory, Working, Contextual, CLI, Task, Extension, Distribution, Testing]
---

# Extension Catalogue Synchronization

## Task State

Prepared for the user's CLI implementer in a separate task. Implementation has not started. This record is integrated with Task 28 after explicit user authorization. Register a task ID in the [project control ledger](../project-control.md) on activation.

Task 28 authorizes framework and extension wording changes while keeping CLI changes separate. This task records the resulting distribution work. Its creation does not dispatch an implementer or authorize merging, publishing, or a new runtime design.

## Outcome

Make the native CLI's embedded Extension catalogue match the accepted files under `src/extensions/`. Task 28 defines five focused packages and a dependency-only Development Toolkit bundle. Orchestration depends on Planning and Development. Experience Design is removed. Planning-only development and C# extension notes now live under `docs/extension-candidates/` so the source catalogue contains only actual packages.

At the original preparation baseline, the CLI stored a compressed asset snapshot and a digest inventory separately from the source packages. Rebuilding alone did not refresh that snapshot. Without `--source`, it selected older Toolkit content and could not select Orchestration. Revalidate these observations on activation; source changes do not establish embedded parity.

## Starting Sources

Revalidate these paths against the implementation baseline selected after user review:

- [First-party catalogue](../../../../../src/extensions/README.md)
- [Embedded assets](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueAssets.cs)
- [Embedded inventory](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueInventory.json)
- [Catalogue integration tests](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Extensions/EmbeddedExtensionCatalogueIntegrationTests.cs)
- [Extension package contract](../../../crystallized/documents/cli/contracts/extension/_extension.md)

The preparation baseline is Task 28's `codex/task28-framework-review` worktree. Choose a fresh accepted baseline on activation. Source files may change during user review, so the eventual accepted package bytes define the target.

## Work And Evidence

1. Inspect the existing asset generation and inventory procedure. Regenerate from accepted package sources with deterministic ordering and digests. Do not hand-author a competing copy of workflow meaning in C#.
2. Cover all six accepted package identities, exact content parity, dependency closure, and exclusion of planning notes. Establish a reproducible source-to-embedded check so stale snapshots cannot silently pass.
3. Build and exercise the CLI from the same worktree. With no `--source`, list and inspect the complete catalogue, install each focused package, install the dependency-only Toolkit bundle, and install Orchestration with its dependencies into isolated workspaces.
4. Verify generated routes and links, repeat no-op behavior, update from a prior accepted Toolkit installation, and removal with retained dependencies and user changes protected. Use the current lifecycle contracts for expected behavior.
5. Run the appropriate integration and published-executable checks. Record exact source and executable identities, commands, results, and limits. Native AOT proof follows the accepted distribution gate.
6. Remove the temporary explicit-source caveats from current setup documentation only after the embedded catalogue actually agrees. Keep explicit-source examples where they remain useful.

### Planning Starter Follow-Up

Task 28 adds four generic Templates under `planning/content/.agents/templates/planning/`, with one scoped entrypoint, and updates the Work Records Pattern and Planning Workflow to reference them. The focused package split preserves installed destination paths. Revalidate the final accepted source on activation.

Include these files in catalogue parity and dependency-closure verification. Check generated navigation to the planning scope, Template classification and on-demand loading, and the Checkpoint Template's link to the shipped destination rules. Keep repository-local specialized Task/Plan Templates independent of the generic package copies. Use the complete accepted catalogue on activation rather than freezing the current package count.

Task 28's source-package checks are preparation evidence only. They do not establish embedded catalogue parity or the implementation task's acceptance.

### Package Ownership Transition

Verify the transition from the previous Toolkit-owned payload to focused package ownership at unchanged installed paths. Do not assume byte equality authorizes adoption, or that ordinary update already supports this transition. Establish the supported sequence against current ownership and lifecycle contracts before documenting it.

Cover prior receipts, unchanged files, local edits, overwrite companions, deliberately removed defaults, obsolete Experience Design files, retained dependencies, and removal of either the bundle or a focused package. A dependency-only bundle has no payload to adopt. Protect workspace changes and retain recovery evidence where required. If the implementation lacks a supported transition, report that gap and prepare the necessary behavior work in the CLI task rather than silently rewriting receipts or widening force semantics.

Update source-to-dogfood parity checks for the new package paths. The review worktree removes its local Experience Design copies and route entry; it does not regenerate lifecycle receipts or qualify managed retirement. Preserve general native Skill support and use an appropriate fixture for its verification.

## Existing Diagnostic Gaps Observed During Preparation

The same-worktree native CLI installed the revised source catalogue, repeated installation without changes, removed Orchestration while reporting the retained Toolkit dependency, and then removed Toolkit explicitly. Doctor reported incomplete coverage after installation and removal. A control installation using the original Toolkit from the review base also produced incomplete Doctor coverage.

Observed findings include native Skill files classified as missing routed metadata or unreachable sources, and Framework-managed category indexes classified as changed after Extension-generated navigation updates. Preserve the native Skill contract and category indexing behavior. Triage these CLI findings against a fresh baseline and record any required separate fix; do not alter package meaning or suppress diagnostics merely to claim a passing check.
