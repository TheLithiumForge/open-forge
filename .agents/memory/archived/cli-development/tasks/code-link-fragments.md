---
open-forge:
  description: Completed issue 22 fragment work and the issue 21 historical code-link follow-up
  tags: [Memory, Archived, Checkpoint, Contextual, Historical, CLI]
---
# Code-link fragments
Code-link fragments is a record that keeps the issue #22 fix and its completed code-link audit.

It began as a handoff from `fix/code-link-fragments` to `feature/comment-links`. The verification section keeps the results recorded at that handoff.

`File.cs#L111-L125` checks file lines, and `File.cs#Type.Member` checks each dotted part as an identifier token. Markdown targets keep heading checks. The [source-reference contract](../../../crystallized/documents/cli/contracts/shared/source-references/interface.md#local-fragments) defines the accepted behavior.

Original status: implementation complete, verification incomplete.

## Link follow-up

Status: finished.

The issue #21 follow-up fixes all 66 audited code-link occurrences across 15 documents. They used 54 old target spellings. The three `RouteSourceStructureReader.cs` citations are part of those 66 occurrences.

Each replacement selects code checked in retained Git history. Historical citations keep the code their prose describes, including retired TypeScript modules and behavior that later changed. The two colon-style line suffixes now use GitHub line fragments. Stale line ranges now select the relevant methods or constants. The `ReadAxioms` citations select its checked implementation at lines 97-129 of commit `62b0e23e8dd6f88c6ef44c70285ae9a58b12ad6f`.

Eight retired TypeScript files are kept as `.ts.txt` files under `../../cli-v2/source-snapshots/`. They match commit `c246601c64acf3916adf3da71126a17d424dfbc9` byte for byte. These copies keep the historical citations usable in a checkout because a public permalink for that revision could not be verified. The other citations use GitHub blobs from revisions contained in retained remote refs.

Focused References reads check all 15 affected documents. The 11 local replacement links resolve. The 55 web replacement links report `external-unchecked`, with their selected code checked through Git. Those reads also report 56 other archival findings outside the supplied code-link list. The local audit keeps them at `artifacts/comment-links-historical-reference-coverage.json` for a separate follow-up.

The link-only follow-up keeps the original verification results below. The maintainer's lane direction makes CI the full-suite gate for the stacked comment-link branch.

## Verification

The branch is `fix/code-link-fragments`, based on `1ee8ab02987658fdd07fbc87269aa13bb9990ac3`. Issue #21 will be stacked on it. The fix changes the shared resolver, command projections, accepted contracts, user docs and both CLI Skill copies.

The 29 focused Unit cases pass. The 47 Integration snapshot, help and report checks pass. We accepted 20 new code-link snapshots, two help snapshots, one custom-root template snapshot and 56 Status metric snapshots after checking valid targets, separate line and symbol findings, authored source coordinates and manual-edit guidance. The payload snapshots change only the added Skill paragraph and its byte, token and startup-share metrics.

NuGet restore initially failed with NU1903 for pinned `Microsoft.DotNet.ILCompiler` 10.0.1. Verification restores with `dotnet restore OpenForge.Cli.slnx -p:NuGetAudit=false`, then builds with `--no-restore`. Dependency versions are unchanged. This run does not pass a dependency audit.

The first complete Unit run executed 5,656 tests and failed two expectations that still listed the old finding count and status cases. We added the two new cases to those expectations. This was a test correction, not a flake.

After the daemon recovery on 12 November, the complete managed Unit suite passed 5,656 tests. The complete managed Integration suite passed 4,902 tests with 60 declared platform exclusions. The documentation site build also passed. The fresh delivery log is `artifacts/issue22/resume-full-gate.log`.

The managed EndToEnd attempt recorded nine process timeout failures, two default-layout expected-byte failures and one outside-lifecycle workspace-preservation failure. We stopped that incomplete attempt at 18:19 to leave time for isolated reruns before the 18:30 deadline. It has no completed suite result and does not pass. The original failure log is `artifacts/delivery/win-x64/reports/public/execution.log`.

The default-layout fixture now includes only the added fragment-guidance paragraph. It keeps the root frontmatter, every other file byte and both expected command outputs. Both default-layout cases pass after a managed test rebuild with zero warnings and errors. This is one reviewed expected-byte fixture update in addition to the 79 Imprint snapshots. Isolated EndToEnd receipts are in `artifacts/issue22/e2e-isolated-receipts.json`, with per-method logs and CTRF results under `artifacts/issue22/e2e-isolated/`.

An earlier parallel Integration attempt failed Extension list workspace preservation and one Extension recovery preparation case, then crashed during Git cancellation cleanup. The isolated reruns passed one Extension list case, three Extension preparation cases and two Git cancellation cases. The earlier attempt stays failed. The fresh complete Integration result above passes.

The Windows x64 Native AOT delivery build passed after recovery. Its binaries predate the default-layout fixture correction. The three native test modes have no fresh completed result. No local commit was created because the required complete EndToEnd gate did not pass. All task changes are kept in the worktree.

To finish this branch:

1. Read the isolated receipts and resolve any failures that still reproduce.
2. Rebuild delivery with `npm run build:native -- --no-restore`, using the lane's Visual Studio process PATH prefix.
3. Run `npm run test:built` without filters to complete all six managed and native modes.
4. Gate the local conventional commit on passing checks in the same command. Don't push, merge or open a PR.

The restore command above still needs `-p:NuGetAudit=false`. Both Index runs over the edited contracts completed with zero effects. Index also listed this checkpoint under Checkpoints. The outgoing reference check of this checkpoint completed with zero findings before the final verification notes were added.

## Isolated reruns

Each method ran separately with collection parallelism disabled. A theory includes all its cases. These results do not replace the incomplete full gate:

| Method | Passed | Skipped | Failed |
| --- | --- | --- | --- |
| `PublishedMainRootDefaultCompatibilityProcessTests.ExactDefaultCompatibility` | 2 | 0 | 0 |
| `PublishedDoctorGeneratedNavigationProcessTests.DetachedPlanningRecipeUsesOwningCatalogueIndexAction` | 1 | 0 | 0 |
| `Journeys.F30DeepPlainNotesJourneyTests.DeepChain` | 6 | 0 | 0 |
| `PublishedUpdateAdoptionProcessTests.AutomaticUpdateAdoptsNativeSkillAndReferenceWithoutReplacingUserBytes` | 2 | 0 | 0 |
| `PublishedUpdateProcessTests.CustomRootUpdateRestoreAndRepeatJourney` | 2 | 0 | 0 |
| `Journeys.F08RepairIsolationJourneyTests.UnsafeRequiredNavigationBlocksExactRelinkWithoutWrites` | 1 | 0 | 0 |
| `PublishedDoctorGeneratedNavigationProcessTests.InstalledPlanningCatalogueUsesDefaultIndexAndDoctorReturnsClean` | 1 | 0 | 0 |
| `PublishedMainRootLifecycleProcessTests.FullLifecycle` | 2 | 0 | 0 |

These methods have no completed isolated rerun before the deadline:

- `PublishedLibraryGitIgnoreProcessTests.BoundTerminalChoiceUsesDefaultNoAndDistinguishesEscape`
- `PublishedMainRootOfflineStartupProcessTests.OfflinePointersRemainSelfContained`
- `PublishedInstallProcessTests.OutsideLifecycleComposition`

The outside-lifecycle workspace-preservation failure needs an isolated rerun. No passing outcome is recorded for it. Stop work by 18:30 local time.

## Link audit

Before this checkpoint was added, a temporary Integration fixture used the production Markdown parser and `SourceLinkDestinationResolver` to read all 1,133 Markdown files under `.agents`, including archived notes. It checked 12,699 links and images. Doctor ran only on temporary fixtures. The scan fixture passed one test and was removed from the suite.

The scan found these failures:

| Finding | Occurrences |
| --- | --- |
| Missing target | 783 |
| Missing Markdown heading | 25 |
| Malformed destination | 4 |
| Total | 812 |

All 66 broken code-link occurrences below report a missing target. They include 54 target spellings. One link names an `.mjs` file and two append a line number with `:` instead of a fragment. The scan found no failed line or symbol check against an existing code file. The complete occurrence data and scan source are kept locally at `artifacts/issue22/agent-link-findings.tsv` and `artifacts/issue22/read-only-audit-source.cs`.

Three links name the old `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs` path. They are at lines 995 and 1008 of `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md`, and line 27 of `.agents/memory/archived/cli-development/analysis/cli-experience-audit/structural-requirements-and-markers.md`. The current file is `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Operational/RouteSourceStructureReader.cs`. Their ranges fit that file, but the next branch needs to check that they still select the intended code.

`FindUniverseResolver.cs` currently lives at `src/cli/operations/OpenForge.Cli.Operations/Commands/Find/Shared/Selection/FindUniverseResolver.cs`. No Markdown destination names it. Its code-formatted mentions are at `.agents/memory/emerging/analysis/multiple-roots-council/plan.md:592`, `.agents/memory/emerging/analysis/main-root-council/plan.md:359` and `.agents/memory/archived/cli-development/tasks/task62/command-plan.md:305`.

## Broken code links

Every row gives the authored source, its line and the missing target. Fragments stay as written:

| Source | Line | Missing target | Fragment |
| --- | --- | --- | --- |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 61 | `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliReportBinding.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 61 | `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliReportCommandBinding.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 62 | `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliReportPipeline.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 63 | `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliPipelineStages.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 64 | `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCoreApplication.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 65 | `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/Models/Presentation/CliRenderedOutput.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 73 | `src/cli/core/OpenForge.Cli.Core/Presentation/Library/Inspect/Shared/Selection/LibraryInspectReportSelector.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 73 | `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/Models/Operation/ICliCommandResult.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 75 | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Architecture/LayerBoundaryTests.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 77 | `src/cli/core/OpenForge.Cli.Core/Commands/Status/Models/Result/StatusResult.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 79 | `src/cli/core/OpenForge.Cli.Core/Presentation/Update/Shared/Selection/UpdateReportSelector.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 88 | `src/cli/core/OpenForge.Cli.Core/Framework/Distribution/EmbeddedFrameworkPayloadReader.cs` | `` |
| `.agents/memory/archived/beta-preparation/cli-modularization-review.md` | 88 | `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueAssets.cs` | `` |
| `.agents/memory/archived/cli-development/analysis/cli-experience-audit/structural-requirements-and-markers.md` | 27 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs` | `#L97-L128` |
| `.agents/memory/archived/cli-development/analysis/cli-experience-audit/structural-requirements-and-markers.md` | 72 | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs` | `#L27-L29` |
| `.agents/memory/archived/cli-development/analysis/cli-experience-audit/structural-requirements-and-markers.md` | 104 | `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownGeneratedRegionSyntax.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 995 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs` | `#L111-L125` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 999 | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs` | `#L27-L29` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1006 | `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownDocumentParser.cs` | `#L1-L3` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1008 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/RouteSourceStructureReader.cs` | `#L111` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1010 | `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownGeneratedRegionSyntax.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1016 | `src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/FrameworkContentIdentity.cs` | `#L12` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1024 | `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Metadata/Models/FrameworkMetadataYamlModels.cs` | `#L5-L18` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1030 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs` | `#L120` |
| `.agents/memory/archived/cli-development/tasks/cli-experience-remediation.md` | 1040 | `src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Rendering/IndexHelpSections.cs` | `#L41` |
| `.agents/memory/archived/cli-development/tasks/extension-catalogue-synchronization.md` | 118 | `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueAssets.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/scoped-continuity-loading.md` | 41 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Loading/SourceLoadingClosureResolver.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/scoped-continuity-loading.md` | 42 | `src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/scoped-continuity-loading.md` | 43 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteContextClosureResolver.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/scoped-continuity-loading.md` | 44 | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Profile/RouteInspectReadingProfileBuilder.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1826 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorHumanRenderer.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1826 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Rendering/DoctorFindingHumanRenderer.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1827 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/LocalReferenceDoctorCandidateInspector.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1828 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Aggregation/DoctorResultBuilder.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1829 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Shell/Presentation/Shared/Rendering/CliHumanText.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1830 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindCompactRenderer.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1831 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Planning/UpdatePlanningPolicy.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1832 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Interaction/RepairWizard.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1832 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallSelectionResolver.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1832 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Extension/Create/Shared/Resolution/ExtensionCreateRequestResolver.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1833 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Update/UpdateOperation.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1834 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Library/Attach/Shared/Planning/LibraryAttachPlanner.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1835 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Library/Sync/Shared/Planning/LibrarySyncPlanner.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1835 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Commands/Library/Detach/Shared/Planning/LibraryDetachPlanner.cs` | `` |
| `.agents/memory/archived/cli-development/tasks/task30/phase-4b-g4-output-proposal-astra.md` | 1836 | `.agents/memory/working/cli-development/tasks/task30/src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCoreApplication.cs` | `` |
| `.agents/memory/archived/cli-release/review/framework-lifecycle.md` | 120 | `src/cli-mvp/cli.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/contracts/managed-lifecycle.md` | 62 | `src/cli/commands/status/managed-record.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/contracts/status-results.md` | 48 | `src/cli/commands/status/framework-anchors.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/contracts/status-results.md` | 50 | `src/cli/commands/status/managed-record.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/contracts/status-results.md` | 51 | `src/cli/commands/status/recovery-artifacts.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/contracts/status-results.md` | 52 | `src/cli/commands/status/status-git-paths.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/development-toolchain.md` | 18 | `eslint.config.mjs` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 375 | `src/cli/commands/status/status-values.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 1134 | `src/cli/result/result.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 1164 | `src/cli/result/result.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 1202 | `src/cli/result/result.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 1224 | `src/cli/result/workspace-reference.ts` | `` |
| `.agents/memory/archived/cli-v2/documents/interface.md` | 1287 | `src/cli/result/process-completion.ts` | `` |
| `.agents/memory/archived/framework-review/proposals/p03-scoped-loading-proposal.md` | 27 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Loading/SourceLoadingClosureResolver.cs` | `` |
| `.agents/memory/archived/framework-review/proposals/p03-scoped-loading-proposal.md` | 28 | `src/cli/core/OpenForge.Cli.Core/Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs` | `` |
| `.agents/memory/archived/framework-review/proposals/p03-scoped-loading-proposal.md` | 28 | `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/Shared/Routes/RouteContextClosureResolver.cs` | `` |
| `.agents/memory/archived/framework-review/proposals/p03-scoped-loading-proposal.md` | 29 | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Profile/RouteInspectReadingProfileBuilder.cs` | `` |
| `.agents/memory/emerging/analysis/lossless-council/proposals.md` | 6093 | `.agents/memory/emerging/analysis/lossless-council/src/cli/framework/OpenForge.Cli.Framework/Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:147` | `` |
| `.agents/memory/emerging/analysis/lossless-council/reviews.md` | 164 | `.agents/memory/emerging/analysis/lossless-council/src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Loading/SourceGeneratedDestinationResolver.cs` | `` |
| `.agents/memory/emerging/analysis/lossless-council/reviews.md` | 1586 | `.agents/memory/emerging/analysis/lossless-council/src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Loading/SourceGeneratedDestinationResolver.cs:68` | `` |
| `.agents/memory/emerging/analysis/route-include-council/proposals.md` | 2326 | `src/cli/operations/OpenForge.Cli.Operations/Commands/Context/Shared/Graph/ContextLinkedEntryReader.cs` | `` |

## Original next step

The handoff ended with this instruction:

> Review these links in the next branch. This branch fixes none of them. Historical targets can be retired on purpose, so a matching filename alone does not establish a replacement.
