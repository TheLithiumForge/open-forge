---
open-forge:
  description: Regenerate and verify the native CLI's embedded catalogue from the reviewed first-party Extension sources
  tags: [Memory, Working, Contextual, CLI, Task, Extension, Distribution, Testing]
---

# Extension Catalogue Synchronization

## Task State

Task 30 is COMPLETE on 2026-09-12, phase 3/3, milestone 3/3, as the first frozen
stage of the user's [sequential follow-up](cli-dogfood-follow-up-plan.md).
Feature `aef337fd` was squash-integrated into local develop at `b4a06740`, with
identical tree `64e88bdee3337295ae5712dbff78422c3f707d0b`. The integration
preserves develop's unrelated ignore/tool/artifact changes, and every qualified
production, test, package and build input equals candidate `ee5a5599`.
Root worked directly in `/tmp/open-forge-cli-refactor-sequential` on
`codex/cli-refactor-sequential`. Base `399b60b8` contains the fixture repairs.
Catalogue production is `1b86b066`; the complete build candidate is `ee5a5599`.
No parallel work is selected.

The user asked why embedding is not automatic, identified package manifests as
the source of IDs and dependencies, and instructed continued work. The accepted
implementation direction is the existing Framework resource pattern: embed
the current package files through Core `EmbeddedResource` items, derive package
facts and hashes from those bytes, and preserve manifest/dependency validation.
Remove the separate hand-maintained compressed C# archive and digest inventory.
No runtime source-checkout fallback, extra dependency, schema change, implicit
ownership migration, or package meaning change is selected.

Freeze: canonical package sources, all existing command behaviors and mutation
safety assertions. The intentional distribution delta is the six accepted source
packages replacing the stale Toolkit snapshot. Replace archive-format evidence
with independent resource/set/byte/hash parity evidence, and adapt only test
setup or inventory assertions that depend on the retired bundled content.
Record such adaptations explicitly; keep source parity strict.

Milestones: resource design and frozen evidence; implementation and complete
managed verification; published Native AOT isolation, package journeys, review
and authorized local squash integration. Verify prior Toolkit ownership effects
without inventing an automatic migration if the existing contracts reject it.

Frozen evidence is committed at `5b24b216`: the two source/resource parity tests
fail against the old implementation, with five focused discovery controls
passing. The first build caught a nullable-memory test compilation error; that
was corrected before the recorded Red run. Existing behavior assertions remain
frozen except explicitly recorded source-inventory fixture adaptations.

Implementation `1b86b066` passes both frozen parity tests and all 3,231 Unit
tests. The first full Integration run exposed exactly three old catalogue
expectations; a focused published run exposed one more. The two List Integration
cases and one published List case assumed a single available package. They now
compare ordered IDs with a source-manifest fixture embedded independently in the
test-support assembly. The Install case now asserts the accepted Toolkit bundle's
four dependencies precede the bundle. Status, read-only, prompt and effect
assertions are unchanged. Complete qualification is in progress.

Managed qualification at `29e19729` is green: 3,231 Unit, 1,724 Integration and
111 public CLI tests, zero failures/skips, Release build zero warnings/errors.
An additional relocated Extension catalogue test passes alongside the existing
relocated Framework test (2/2). The complete candidate passes the canonical
`npm run build:native -- --sha` and `npm run test:built` gate on Linux x64.
Unit is 3,231/3,231, managed and native Integration are each 1,724/1,724,
and managed-public, native-public and managed-on-native-public are each 112/112.
All suites have zero failures/skips. The manifest is marked `tested: true`;
reports are `artifacts/delivery/linux-x64/reports-ecxh8L`, summarized in
`artifacts/task30-automatic-catalogue/native-qualification.json`.
The isolated journey executable and gate executable have identical SHA-256
`33cda49d8cd1f8541724b5b4fddd1a819cea69021f13411abf1fdf52bf0dd546`.
Later edits only update documentation and work records. No foreign-host or
remote-release proof is claimed. Local integration is complete as recorded above.

### Native Package Journeys And Transition Findings

The locally linked Native AOT executable reports
`0.0.0-dev.sha-ee5a55999d830c337c6510fb94953a2bac3bcd53`. A copy of the executable
alone passed six isolated package install/inspect/repeat journeys and an all-six
installation. Manifest-derived dependency closures matched exactly. Installed
authored content matched source bytes outside generated Entries interiors;
the independent Integration parity tests verify exact embedded bytes including
those interiors. Repeated installations left workspace file hashes unchanged.

The 54-command receipt at
`artifacts/task30-automatic-catalogue/journeys/run3/summary.json` includes dependency
removal refusal and bundle removal with retained dependencies. The exploratory
wrapper initially assumed complete for retained-dependency attention, then used
the wrong blocked exit code. Both harness expectations were corrected against
the existing contract; neither required a product or committed-test change.

The user confirmed that unreleased package revisions have no legacy migration
requirement. Exploratory comparisons with the superseded Toolkit do not define
supported behavior or a future compatibility task. Current package ownership and
local-change protection remain covered by the tracked command tests.

Direct review of the final production, independent parity evidence,
and coherent changed documentation found no material remaining defect in the
automatic embedding stage. All 16 changed Markdown files' relative link targets
exist. Required changed-C# whitespace and style checks pass.

## Preparation Context

Task 28 authorized framework and extension wording changes while keeping CLI
changes separate. The original preparation record did not activate implementation;
the later user direction above now authorizes this bounded stage and verified
local squash integration. Remote publication remains unauthorized.

## Outcome

Make the native CLI's embedded Extension catalogue match the accepted files under `src/extensions/`. Task 28 defines five focused packages and a dependency-only Development Toolkit bundle. Orchestration depends on Planning and Development. Experience Design is removed. Planning-only development and C# extension notes now live under `docs/extension-candidates/` so the source catalogue contains only actual packages.

At the original preparation baseline, the CLI stored a compressed asset snapshot and a digest inventory separately from the source packages. Rebuilding alone did not refresh that snapshot. Without `--source`, it selected older Toolkit content and could not select Orchestration. Revalidate these observations on activation; source changes do not establish embedded parity.

## Starting Sources

Revalidate these paths against the implementation baseline selected after user review:

- [First-party catalogue](../../../../../src/extensions/README.md)
- [Embedded assets](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueAssets.cs)
- [Core resource inclusion](../../../../../src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj)
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

### Current Catalogue Verification

Verify the current package manifests, resource bytes, dependency closure and
ordinary installation/update/removal behavior against current contracts. Use
current fixtures. Do not add migration requirements for unreleased package
revisions. Preserve current local-change, ownership and recovery protections.

## Existing Diagnostic Gaps Observed During Preparation

The same-worktree native CLI installed the revised source catalogue, repeated installation without changes, removed Orchestration while reporting the retained Toolkit dependency, and then removed Toolkit explicitly. Doctor reported incomplete coverage after installation and removal. A control installation using the original Toolkit from the review base also produced incomplete Doctor coverage.

Observed findings include native Skill files classified as missing routed metadata or unreachable sources, and Framework-managed category indexes classified as changed after Extension-generated navigation updates. Preserve the native Skill contract and category indexing behavior. Triage these CLI findings against a fresh baseline and record any required separate fix; do not alter package meaning or suppress diagnostics merely to claim a passing check.

## Durable Qualification Summary

Generated logs and machine reports mentioned above are disposable. This tracked
summary, committed regression sources and ordinary build scripts retain the
required result and reproduction path. Exploratory task scripts are not build
inputs or a substitute for committed regressions.

- Qualified source: `ee5a55999d830c337c6510fb94953a2bac3bcd53`; local squash: `b4a06740`.
- Native target: `linux-x64`; version: `0.0.0-dev.sha-ee5a55999d830c337c6510fb94953a2bac3bcd53`.
- Native CLI SHA-256: `33cda49d8cd1f8541724b5b4fddd1a819cea69021f13411abf1fdf52bf0dd546`.
- Gate result: 3,231 Unit; 1,724 Integration in both modes; 112 public in all three modes; zero failures/skips.
- Toolchain: .NET SDK 10.0.111, Node 24.19.0, npm 11.17.0, Linux x64.
- Reproduce from that source commit with repository dependencies restored:
  `npm run build:native -- --sha`, then `npm run test:built`.
  These tracked scripts generate fresh outputs and validate all six suites.
- Required regression sources: `src/cli/tests/`; build/test orchestration:
  `scripts/delivery/`. No required helper exists only in `artifacts/`.

These are recorded past results. Deleting outputs discards raw receipts and
binaries; rerun the commands before claiming fresh execution evidence.
