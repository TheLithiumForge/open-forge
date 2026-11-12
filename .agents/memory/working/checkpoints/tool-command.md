---
open-forge:
  description: Resume issue 23 tool core contracts, staged implementation boundaries and required decisions
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool]
---

# Tool command core
The Tool command core is an implementation task that adds the eight contracted external-program lifecycle commands without concrete tools.

The user selected issue #23 and its two comments, the complete external specification, and the lane rules. Work stays in this worktree on `feature/tool-command`. The starting commit is `1ee8ab02987658fdd07fbc87269aa13bb9990ac3`. No push, merge or PR is authorized.

## Scope

The requested review boundaries are:

1. Contracts and docs.
2. Models, schemas and exact catalogue selection.
3. Read-only List, Inspect and Status.
4. Install, Configure, Choose, Update and Remove.

Concrete CodeGraph and Codebase Memory definitions and adapters belong to the next stacked branch. Production admission stays empty. Tests use minimal synthetic definitions and local hashed artifacts. Settings v6 shows installed pins and active roles. Lock v2 holds detailed receipts and recovery records. Host members, ignore editing, root-removal exclusions and recovery remain required core work.

## Current state

Evening continuation, 2026-11-12. The overseer rebased this branch onto PR #25, with contracts commit `96db76ca1`, and pushed draft PR #26. Keep source-reference and link code outside this work. CI now runs full suites. Locally run only builds and focused Unit and Integration tests. Don't run full suites or Native AOT. The overseer authorizes `NuGetAudit=false` if NU1903 prevents restore. Make one local commit per stage and don't push.

The saved increment is restored except for paths changed upstream. Those overlapping docs and snapshots keep the rebased versions until their Tool additions are merged or regenerated. The existing source-reference implementation is untouched. Stage 2 now has typed manifest and catalogue models, strict JSON and YAML decoding, release admission, exact source selection, settings and lock additions, an empty production catalogue and test-only local artifacts. The stage 2 focused checks are complete. Record its commit, then implement the three readers before the five writers.

The overseer directs implementation to use the specification's recommended recovery v2 and TOML choices behind separate seams while maintainer confirmation is pending. These choices no longer block models or readers. Record the exact recovery digest publication rule before writer work. The current recommendation is Tomlyn 2.10.2. No concrete tool or production adapter is admitted.

The earlier contracts-only increment contained the [Tool contracts](../../crystallized/documents/cli/contracts/tool/_tool.md), shared manifest contracts, cross-command contract changes, technical designs and the workspace-state decision. It changed no shipped payload, production code, test source, snapshot or site file. Recovery v2 is a separate proposal pending confirmation, as required by the source specification.

Read-only prose and contract review is complete. R1 clarified Inspect's blocked result for unsupported tuples. R2 added the required V1 graph Skill constraints. R3 centralized coexistence policy and replaced copied leaf rules with summaries and links. All three passed the focused correction recheck. That earlier increment added no command implementation, production tool definition, schema or parser package.

## Saved increment

Stash `de681f9d56f95484d789c3782fe2a774ba6fa584` is named `tool-command: preserve guidance payload and fixture changes before deadline`. It keeps the remaining correct work without including it in this commit. Restore only its tracked difference from the stash index and its untracked tree, so the committed contracts and this checkpoint stay intact. Review the resulting tree and keep the stash until its changes are verified and committed.

After loading context for the restored files, use these commands:

```powershell
$saved = 'de681f9d56f95484d789c3782fe2a774ba6fa584'
$pendingTracked = @(git diff --name-only "$saved^2" $saved)
$pendingNew = @(git ls-tree -r --name-only "$saved^3")
git restore --source=$saved --worktree -- @pendingTracked
git restore --source="$saved^3" --worktree -- @pendingNew
```

The saved increment contains public references, site changes, both CLI Skill copies, aligned loader guidance and their maintenance docs. Its Integration and EndToEnd fixtures read one source-derived test-support inventory instead of copied file lists. This covers the added Skill reference during assertions and cleanup. No new test case was needed for that fixture correction. The default-layout fixture keeps its byte-level file oracle. Its two rendered output expectations use Imprint instead of embedded text and JSON fields. The composed confirmation captures use the new interaction classification. Snapshot review and earlier execution evidence below describe this saved increment, not the contracts-only commit.

## Applicability

This is a local development CLI. It can change authored workspace files and bounded account tool stores. Ordinary defects, malformed input, interruption, dependency failure and cooperating concurrent writers are in scope. A malicious same-account process that bypasses locks is outside the accepted model. Git, immutable prior-byte recovery bundles, atomic file replacement, regeneration and explicit reruns provide the practical recovery limits. Recovery never silently rolls back.

Reuse the accepted shell pipeline, source-generated serializers, standard YAML parser, physical path checks, settings grants, verified lifecycle receipts, workspace lease and recovery v1 reader. Keep tool policy in Operations and neutral store, record and effect facts in Framework. Root composition provides bounded network and finite process capabilities. New records stay in local Models folders. Exceptional machinery: none accepted. A standard TOML parser dependency needs a decision before Codex editing.

Unit covers pure validation, selection, plans and result graphs. Integration covers real owned filesystems, host preservation, stores, leases, recovery and paired writes. EndToEnd covers small published-command journeys. The evening lane policy makes CI the full-suite gate. Local changes need a build and focused Unit and Integration checks only. Do not run full suites or Native AOT locally. The daytime full-suite receipts below describe earlier work, not the current gate.

## Earlier evidence

The contracts-only gate uses the Integration and Unit build commands and D: temp settings recorded below. Its exact unfiltered suite command is:

```powershell
node --input-type=module -e 'import {runSuites} from "./scripts/delivery/test-suites.ts"; import {TestAssemblies} from "./scripts/delivery/layout.ts"; await runSuites(process.cwd(), Object.entries(TestAssemblies).filter(([name]) => name !== "public").map(([name, assembly]) => ({name, executable: `artifacts/bin/${assembly}/release/${assembly}.dll`})), "artifacts/tool-command/contracts-only-gate");'
```

Root and Docusaurus `npm ci` completed. The ordinary `npm run build -- --log artifacts/tool-command/build-contracts.log` failed during restore with NU1903 for the SDK's Microsoft.DotNet.ILCompiler 10.0.1 dependency. Auditing and warnings-as-errors stay enabled. The installed stable SDK is 10.0.101. No toolchain or package version was changed.

A managed-only standard restore and build passed with zero warnings and errors using these exact commands:

```powershell
$env:APPDATA = Join-Path (Get-Location).Path 'artifacts/tool-command/appdata'
dotnet restore OpenForge.Cli.slnx -p:PublishAot=false
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:PublishAot=false
```

Managed-only restore passed with auditing enabled. It excludes the unused native compiler and doesn't test Native AOT. Record final suite counts and results before committing. If default restore or Native AOT is needed later, resolve the audited toolchain failure through an accepted toolchain decision.


The prose review and grouped repairs are complete. Index preview and apply updated 14 affected navigation files. The site typecheck and production build passed. The final corrected managed build passed with 0 warnings and 0 errors.

The first full managed run passed 5,627 Unit tests. Integration ran 4,960 tests with 4,601 passes, 299 failures and 60 platform exclusions. Most failures came from copied fixture inventories, expected guidance snapshots and the managed-only profile's reflection default. The fixture inventories are corrected. The initial payload parity comparison used a binary built before the shipped loader edit, so that run doesn't test final source parity.

One Doctor Git-successor case failed in the full run. Both parameter cases passed the single isolated method rerun, with 2 passed and 0 failed. This is recorded as a busy-machine Git flake, not a passing first run.

The first published-command run selected 481 tests with 474 passes, 6 failures and 1 platform exclusion. Two failures were the expected default-layout byte-fixture change, one was the expected loader paragraph snapshot change and three were 30-second child-process timeouts. Each timeout case passed its single isolated rerun, with 1 pass and 0 failures each. Keep the original failures in the report.

The corrected default-layout and loader-context cases passed their focused run, with 3 passes and 0 failures. The CLI, Integration and EndToEnd managed profiles set `JsonSerializerIsReflectionEnabledByDefault=false`, as the accepted serialization contract requires. Unit keeps its normal runner setting because existing malformed-input fixtures use reflection to construct their input. An attempted full gate applied the reflection override to Unit too, with 5,618 passes and 9 fixture-construction failures. It stopped before Integration and created no commit. Rebuilding only Unit without that override passed with zero warnings and errors. No product or fixture behavior changed for this correction.

The next unfiltered gate passed all 5,627 Unit tests. Integration finished with 4,895 passes, 5 failures and 60 platform exclusions. It found additional startup-count snapshots and two copied effect-count expectations. Those expectations now derive payload count from the source inventory while keeping the infrastructure count fixed. The affected composed confirmation case now uses Imprint for its three output streams instead of literal text checks. Input consumption, no-op bytes, semantic status and lock assertions stay direct. The corrected Integration rebuild passed with zero warnings and errors. Its focused capture reached all semantic assertions and reported only the three missing baselines. Review confirmed the complete wizard, one confirmation, 19 files, 13 directories and the following no-op. Those three baselines are accepted.

The corrected build and unfiltered managed suite commands are:

```powershell
dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore -p:PublishAot=false -p:JsonSerializerIsReflectionEnabledByDefault=false
dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -p:PublishAot=false
$env:OPENFORGE_SNAPSHOT_UPDATE = '0'
$env:IMPRINT_UPDATE = 'verify'
$env:TEMP = 'D:/Temp/open-forge-tool-command'
$env:TMP = $env:TEMP
node --input-type=module -e 'import {runSuites} from "./scripts/delivery/test-suites.ts"; import {TestAssemblies} from "./scripts/delivery/layout.ts"; await runSuites(process.cwd(), Object.entries(TestAssemblies).map(([name, assembly]) => ({name, executable: `artifacts/bin/${assembly}/release/${assembly}.dll`})), "artifacts/tool-command/final-gate-6");'
```

The managed EndToEnd binary uses the earlier corrected solution build with the reflection override. The later Integration and Unit builds don't change its test source. The Integration build refreshes the managed CLI publish used by EndToEnd. Gate reports bind every suite to its assembly hash.

The outside-repository temp restart exposed a capture-classification error. The five corpus checks require native report filenames and all four detail levels. The three new confirmation captures record a terminal conversation. They now use `.interaction.txt`, and the corpus reader excludes that category alongside installed-file content and help. The confirmation test still compares every transcript byte through Imprint. Every native report still runs through its existing vocabulary, envelope and detail-ladder assertions. That gate was stopped before completion. Rebuild Integration and run a fresh complete gate after this correction.

## Next steps

The 17:25 daemon interruption stopped `final-gate-3` during Integration. It created no commit. The maintainer reported a full C: drive and restored space. All 469 changed files stayed staged and the branch stayed at its starting commit. No test process survived. That incomplete gate gives no acceptance evidence. The first restart put temp inside the worktree. A non-Git fixture then inherited the parent repository, so that run was stopped and gives no acceptance evidence either. The corrected restart uses an owned D: temp directory checked to be outside every Git repository. It reruns all three managed suites and both site checks before committing. The deadline for this session is 2026-11-12 at 18:30 Europe/Zurich. Boundary 1 is the feasible horizon before that deadline.

Restore the saved increment and complete its gates before models or command implementation. After the decisions below, implement models and published schemas, then the readers, then the writers in the selected order. Keep released schema documents unchanged. Add recovery v2 readers to Status, Doctor and Cleanup before any tool writer effect. Record every commit, suite result, snapshot acceptance, flake and deviation here for the final report.

## Not decided yet

- Recovery v2 coordinates, directory and native-settings snapshots, attribution and simultaneous v1/v2 reader rollout. The source specification requires confirmation separately from receipt acceptance. A user question is pending.
- Recovery-record digest and publication timing. A full bundle digest inside the next lock bytes would refer back to a bundle that includes the intended lock hash. Resolve that self-reference before writer implementation while keeping independent store discovery. No publication workaround was implemented.
- A centrally pinned standard TOML parser for complete Codex configuration parsing and byte-preserving edits. Tomlyn 2.10.2 is proposed based on its documented syntax tree and spans. A user question is pending. No package was added.
- The default build and future Native AOT gate need an audited stable toolchain that doesn't resolve the rejected 10.0.1 native compiler.

## Snapshot acceptance

Accepted 373 reviewed baseline files across these 42 groups from the corrected managed build and published CLI capture. There were 407 initial capture observations because the two InstalledWorkspace parameter cases produced 55 identical captures. Repeated observations matched byte-for-byte and are counted once below. Six additional published-output baselines cover the two default-layout outputs and four loader-context steps. Their focused cases passed after review. Twelve additional Status baselines change only startup bytes, characters and tokens by the loader paragraph's exact 240 bytes and 60 tokens. Three composed Install baselines replace literal output assertions with the full wizard, applied output and following no-op. The byte fixture adds the same one reference file, loader paragraph, Skill link and lock path, while keeping unrelated files byte-identical. The added on-demand Tool reference changes file, effect and source-asset counts. The aligned loader paragraph changes measured startup bytes and tokens. Install, Update and Status captures show those facts. Custom-layout guidance adds the new reference. Existing statuses, finding codes, effect outcomes and recovery rules stay unchanged. Changed diagnostic captures contain numeric effect or unchanged-file counts only. Prior-byte hashes and temporary paths weren't copied into new expectations.

| Group | Baseline files |
| --- | ---: |
| `InstallBeforeOutputSnapshotTests.AlreadyInstalled` | 6 |
| `InstallBeforeOutputSnapshotTests.Cancelled` | 8 |
| `InstallBeforeOutputSnapshotTests.ChangedFrameworkFile` | 6 |
| `InstallBeforeOutputSnapshotTests.ConfirmationUnavailable` | 8 |
| `InstallBeforeOutputSnapshotTests.ExistingAgentsDocument` | 10 |
| `InstallBeforeOutputSnapshotTests.FreshDirectory` | 20 |
| `InstallBeforeOutputSnapshotTests.OccupiedGeneratedRegion` | 20 |
| `InstallBeforeOutputSnapshotTests.PartialWriteFailure` | 10 |
| `InstallBeforeOutputSnapshotTests.RecoveryStoreUnavailable` | 6 |
| `InstallCompositionIntegrationTests.RootCompositionOwnsOneEligibleConfirmation` | 3 |
| `InstallChangeOutputSnapshotTests.ConfigureGuidanceAndArchivedMemory` | 4 |
| `InstallChangeOutputSnapshotTests.ConfigureMetadataPreview` | 10 |
| `InstallChangeOutputSnapshotTests.ExistingContentPreview` | 20 |
| `InstallOutsideRouteSetupIntegrationTests.FailedRegistration` | 4 |
| `InstallOutsideRouteSetupIntegrationTests.RegistersAfterInstall` | 4 |
| `InstallSetupRecapIntegrationTests.ConfigureRecapsOnlyChangedRoutes` | 4 |
| `InstallSetupRecapIntegrationTests.FirstInstallationRecapsResolvedSelection` | 20 |
| `InstallSetupRecapIntegrationTests.ProseFitsEightyColumns.preview` | 10 |
| `InstallSetupRecapIntegrationTests.ProseFitsEightyColumns.receipt` | 10 |
| `StatusBeforeOutputSnapshotTests.HealthyWithExtension` | 8 |
| `StatusBeforeOutputSnapshotTests.InstalledWorkspace` | 55 |
| `StatusBeforeOutputSnapshotTests.RecoveryBundlePresent` | 8 |
| `StatusBeforeOutputSnapshotTests.WorkspaceBoundary` | 4 |
| `IncludedFileReportingIntegrationTests.ReportsIncludedFiles-status` | 4 |
| `StatusOutsideIntegrationTests.OutsideHumanAndJsonViewsAreReviewed` | 8 |
| `UpdateBeforeOutputSnapshotTests.Cancelled` | 9 |
| `UpdateBeforeOutputSnapshotTests.ConfirmationUnavailable` | 6 |
| `UpdateBeforeOutputSnapshotTests.DryRunChanges` | 9 |
| `UpdateBeforeOutputSnapshotTests.PartialWriteFailure` | 9 |
| `UpdateBeforeOutputSnapshotTests.UpToDate` | 9 |
| `UpdateBeforeOutputSnapshotTests.changed-file-replaced` | 9 |
| `UpdateBeforeOutputSnapshotTests.missing-file-restored` | 9 |
| `UpdateBeforeOutputSnapshotTests.retired-kept` | 9 |
| `UpdateBeforeOutputSnapshotTests.retired-pruned` | 9 |
| `UpdateDirectoryRestorationIntegrationTests.PreviewsMissingDirectoryWithoutWrites` | 9 |
| `UpdateDirectoryRestorationIntegrationTests.RestoresRemovedGuidanceDirectory` | 9 |
| `WorkspaceLayoutPayloadRenderingIntegrationTests.CustomGuidanceMatchesReviewedTemplates` | 1 |
| `PublishedMainRootDefaultCompatibilityProcessTests.ExactDefaultCompatibility` | 2 |
| `PublishedRouteIncludeProcessTests.NoSilentReactivation.after-0` | 1 |
| `PublishedRouteIncludeProcessTests.NoSilentReactivation.after-1` | 1 |
| `PublishedRouteIncludeProcessTests.NoSilentReactivation.after-2` | 1 |
| `PublishedRouteIncludeProcessTests.NoSilentReactivation.after-3` | 1 |

## Contract gate

The complete unfiltered managed Unit and Integration suites passed on the contracts-only tree. Both fresh builds passed. Snapshot updates were disabled. No snapshots changed in this commit. The shipped payload, public CLI output, site and test sources match the starting commit, so EndToEnd and site gates apply to the saved follow-up increment. Native AOT was not run.

| Suite | Passed | Failed | Platform exclusions |
| --- | ---: | ---: | ---: |
| unit | 5627 | 0 | 0 |
| integration | 4900 | 0 | 60 |

This commit completes the contracts portion of boundary 1. Restore the named stash and gate its guidance and fixture changes before models or command implementation. Reports are under `artifacts/tool-command/contracts-only-gate`.


## Stage 2 evidence

The stage 2 increment adds typed manifest and catalogue records, strict standard-parser JSON and YAML decoding, exact source hashes and selection, release admission, an empty embedded production catalogue, settings v6 and lock v2 with tool receipts. Synthetic definitions and hashed artifacts are test-only. Production admits no tool. It also restores the saved guidance, site and fixture work while keeping the rebased source-reference code unchanged.

Local results are:

- Final Unit build passed with 0 warnings and 0 errors. The 300 focused Tool, Settings serialization and Ownership serialization tests passed with no skips.
- Integration build passed with 0 warnings and 0 errors. The 191 focused catalogue, payload, install, update, status and Markdown checks passed with no skips. A final catalogue-only rerun refreshes evidence after the last two pure validation guards.
- The four new schemas compile under the existing documentation workspace's Ajv 2020 validator. The synthetic manifest validates, and optional graph guidance fails validation.
- Site production build passed. Node emitted its existing localStorage experimental warning.
- Index preview checked 199 files and planned no changes.
- Writing review R4 replaced copied implemented type declarations with schema links. The focused recheck passed.

The first affected-capture pass had 175 passes and five expected snapshot failures. Review found only the new Tool guidance row and resulting file, byte, token and ratio measurements. Accepted 57 unique rebased captures. An acceptance script briefly read one Unicode guidance capture through the Windows default encoding. The rerun caught that mistake. All 57 files were reapplied from received bytes with explicit UTF-8, and the focused rerun passed. Earlier reviewed captures and the 57-file rebase refresh overlap and aren't additive counts.

The default-layout byte fixture keeps upstream changes and adds the exact current loader, Skill and Tool reference bytes, plus settings v6 and lock v2 controls. Published execution of that fixture is left to CI under the evening policy. No local EndToEnd or native suite ran in this continuation. No real tool downloads occurred.

Current gate reports are in `artifacts/tool-command/stage2-final-unit`, `stage2-accept-integration` and `stage2-commit-catalogue`. Their commands use the built Release assemblies with explicit class filters. Builds use `PublishAot=false` and the authorized local `NuGetAudit=false` override. Integration keeps JSON reflection disabled. Unit keeps its normal fixture-construction setting.

Stage 3 next: add Tool group composition and the List, Inspect and Status leaves through the existing typed binding/report pipeline. Root examples are `Composition/CliExtensionComposer.cs`, `CliCompositionRoot.cs`, `Commands/Extension/List/ExtensionListBinding.cs` and the corresponding Presentation selector. Keep concrete results per leaf and OutputText wording. Source operands resolve from the invocation's process directory. Readers must preserve independently readable settings rows and create no state, leases, transfers or child processes.
