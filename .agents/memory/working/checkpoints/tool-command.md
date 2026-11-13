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

Concrete CodeGraph and Codebase Memory definitions and adapters belong to the next stacked branch. Production admission stays empty. Tests use minimal synthetic definitions and local hashed artifacts. Settings v6 shows installed pins and active roles. Lock v2 holds detailed receipts. Tool prior bytes use sibling backups. Host members, ignore editing, root-removal exclusions and recovery remain required core work.

## Current direction

The latest 2026-11-13 maintainer decision supersedes all earlier host-writing and backup choices below. Tool-owned pinned install, uninstall and index-cleanup commands handle agent files and native configuration. Open Forge edits only its settings, lock and managed ignore section. Remove Tomlyn, its generated context and host parser seam. Git workspaces create no `.bak`, including dirty or untracked targets. Non-Git workspaces use sibling backups. Status, Doctor and Repair still handle leftovers. Concrete profiles stay on the next branch.

The worktree starts this continuation clean at `9adb49e08`, after the overseer's rebase onto `9e6b9203b` and formatting correction. Restoring `60d034f` had one conflict in ToolHostStatusTests. The resolution keeps upstream formatting and the intended path constant migration. That resolved reporting and restoration increment is saved separately with the stash message `tool-command: reporting and restoration restored after whitespace rebase`. The original stash stays intact. Restore only compatible portions after the current command-model changes, and gate them before commit.

This increment revises contracts and public guidance before command implementation. The supported consequence is local managed-file changes and declared child commands. Git or sibling backups cover managed files according to the maintainer policy. Child-owned changes aren't covered by an invented rollback. Use BCL processes with separate arguments, source-generated records, exact admission, existing leases, expected-byte checks, grants and exclusions. No shell, TOML parser, new project or tool-specific runtime code is needed. Unit checks cover typed declarations. Integration checks cover real files, processes and composed results. CI owns full and native suites. Run both .NET whitespace and style checks before each commit.

The replacement-contract increment has a warning-free Integration build. It accepts 50 reviewed Status captures containing only the revised CLI Skill payload measurements. Initial focused results are 61/66, with five expected capture failures. After acceptance the rerun is 65/66, with only the thirteen stale anchor links identified by writing review. Those dependencies are corrected. Writing findings R9-R11 pass focused recheck. The required whitespace and style commands both pass. The final focused gate passes 66/66 cases with zero skips. Its selection is RepositoryMarkdownTests, StatusBeforeOutputSnapshotTests, StatusOutsideIntegrationTests and IncludedFileReportingIntegrationTests. The site build passes with the existing Node localStorage warning. Index preview has no changes. Reports use the `nov13-tool-owned-contracts-*` prefix under `artifacts/tool-command`. No local full or native suite runs.

Next increment removes Tomlyn and the host parsing seam, adds typed lifecycle declarations and command evidence, then implements the generic writer capabilities. The separate `toolRuns` maps record failed first attempts and completed uninstalls without representing an installed tool. Original and rebased reporting stashes remain saved and ungated. Don't restore either wholesale over the new command model.

The lifecycle-model increment removes Tomlyn from the central package versions and Framework project, deletes its generated context and the host parsing seam, and removes generated host-member receipts. Passive configuration now compares recorded hosts and root without parsing agent files. Lifecycle declarations specify separate arguments, workspace or host scope, declared workspace outputs and bounded timeout. A leading code-root variable is contained before application. Settings and lock codecs round trip separate `toolRuns` maps. Detailed attempts keep the exact definition snapshot and command results without an installed registration.

Evidence: restore passes with the authorized audit override. Fresh Unit and Integration builds pass with zero warnings and errors. The focused Unit gate passes 139/139 with zero skips. The first Integration gate passes 51/52, with only expected help capture differences. Review accepts the three read-leaf help captures, adding only the installer-ownership sentence. The final Integration gate passes 52/52 with zero skips. Both required .NET whitespace and style checks pass. Four schemas compile through Ajv 2020, examples and the existing synthetic manifest validate. Index preview has no changes. The focused writing check passes. Evidence uses `nov13-lifecycle-*` under `artifacts/tool-command`.

Next: implement bounded host process and transfer capabilities, artifact stores and references, Git-versus-sibling backup selection, the shared mechanical ignore editor and five concrete writer flows. An unexecuted edit script at `artifacts/tool-command/apply-tool-execution-host.py` prepares the first three capability files. It is disposable preparation, not completed or gated source. Review it before applying. The original and resolved reporting stashes are still retained. Status/Doctor reporting and Repair restoration still need integration.

The branch is rebased onto `feature/comment-links` at `6304c4046`. The three original increments are now `969eb1a42`, `be5688502` and `26fc75e57`. The checkpoint index keeps Comment Links and Tool Command. Code Link Fragments stays archived. Generated Status conflicts are refreshed from the final rebased source, not merged by hand.

Rebase evidence uses a warning-free Integration build and focused Status snapshot tests. The first run has 53 cases, with 48 passes and five expected snapshot failures. Review accepts 63 unique captures. Changes contain the Tool guidance row, combined payload measurements and the inherited Comment Links missing-target wording. The wording producers match the new base and are unchanged by this lane. Repeated captures agree. The verify rerun passes all 53 cases with zero skips. Evidence is recorded under `artifacts/tool-command/nov13-rebase-status-gate`.

The rebase refresh is committed as `5f2198e31`. The next increment replaces `technical-designs/recovery-v2.md` with the decided Tool backups design, aligns dependent contracts and public references, defines Repair backup-group selection and removes the unused v2 recovery records and placeholder coverage from the lock model, schema and readers. Codex writes stay on hold. No Tomlyn implementation or dependency removal is included.

The decision increment has warning-free Integration and Unit builds. Focused Unit receipt tests pass 7/7. Focused Integration catalogue, Tool readers, host status, Status and repository Markdown tests pass 106/106 with zero skips. The first run has four expected snapshot failures. Review accepts 49 unique regenerated Status captures containing only Skill payload measurements. Final evidence is under `artifacts/tool-command/nov13-backup-contracts-gate` and `nov13-backup-contracts-unit`. Lock schema compilation and example validation pass. Index preview has no changes. Writing review R6 and R7 are corrected and rechecked. The documentation site build passes with its existing Node localStorage warning.

The decision and model update is committed as `00a90a3b2`. The next increment adds workspace file backup sessions, bounded passive backup discovery and shared Tool file observations. Sessions require a live workspace lease, reject stale, duplicate, overlapping and existing-backup targets before effects, keep exact prior bytes, validate replacements and restore failed groups. Unsafe later edits keep their backups. New files have no prior backup. The existing Tool passive reader uses the promoted path checks, including ordinary directory ancestors.

Backup mechanics has a warning-free Integration build and passes all 48 focused cases in `artifacts/tool-command/nov13-backup-mechanics-gate`, with zero skips. This includes 19 new filesystem cases and 29 existing Tool reader and host-status cases. No snapshot changes. An initial 17-case run had four deterministic fixture failures from create-versus-replace helper misuse and unregistered retained-file cleanup. Those fixtures are corrected. This is not a flake. No new Unit case is needed for these real-filesystem behaviors. Full managed and native gates belong to CI under the current lane direction.

The backup mechanics increment is the current gated source baseline. Clean-stop verification rebuilds that source with zero warnings and errors. Its focused Integration gate passes 114/114 cases with zero skips. Review accepts 48 regenerated Status captures containing only measurements from the pending-feature Skill wording. The site build passes with the existing Node localStorage warning. Index preview has no changes. Evidence is under `artifacts/tool-command/nov13-clean-stop-gate`, `nov13-clean-stop-build.log` and `nov13-clean-stop-docs-build.log`. The unfinished next increment is saved in stash `60d034fef70b076bcb05a488d668f09fd538ddce`, named `tool-command: preserve backup reporting and restoration integration`. Its base is `c3e8ba334` before the checkpoint and public-status amendment. The source baseline is unchanged by that amendment. Restore this new stash with `git stash apply 60d034fef70b076bcb05a488d668f09fd538ddce`, then review its tracked and untracked files. Keep the stash until the restored increment is gated and committed. Don't restore the older `de681f9` stash again.

The new stash adds passive backup findings to Tool readers, root Status and Doctor, fixed Tool workspace path constants, a shared quoted workspace continuation formatter, reporting tests and initial neutral backup-restoration types. It has no Repair command integration and no Tool writer. The extracted replacement helper and restoration types were added after the last reporting build and have no gate yet. Public references now state that backup reporting and Repair restoration are pending.

Reporting evidence is incomplete. Its initial run passes 29 existing cases and fails three new cases. Two failures are fixture misuse when creating a file inside the installed `.agents` directory. The third is an expected missing snapshot baseline. Fixture corrections use `WritePostInstallAgentText`, collect all command snapshots before matching and add malformed-lock coverage. The corrected reporting build passes with zero warnings. Those corrected tests have not run. The only received output set has eight Tool Status captures under `artifacts/command-output-snapshots/30930dd91b584cf58aabb6ebe46ef552`. None are accepted.

Writing review R8 found missing selected-workspace continuations. Tool instructions, Doctor actions and root Status actions now use the shared quote helper in the stash. R8 passes its focused recheck. Before committing the reporting increment, connect Repair restoration so its preview continuation performs the contracted work. Also replace the cause's unverified producer attribution. A sibling `.bak` name alone doesn't establish which command created it.

Next: restore and finish backup reporting and bounded Repair restoration. Validate saved control bytes, destination permission, exclusions, expected target and backup bytes, grouped settings and lock restoration, overlap with existing Repair work, success cleanup and failure retention. Existing recovery v1 stays unchanged. The neutral restoration primitive needs focused real-filesystem tests and another build. Then implement Install, Configure, Choose, Update and Remove, artifact and process consent, preview parity, Claude members, managed ignore entries, verified account stores and root-removal receipt release. These behaviors are contracted but still unbuilt. Codex writes stay on hold. No push or PR mutation is authorized.

## Earlier state

Evening continuation, 2026-11-12. The overseer rebased this branch onto PR #25, with contracts commit `96db76ca1`, and pushed draft PR #26. Keep source-reference and link code outside this work. CI now runs full suites. Locally run only builds and focused Unit and Integration tests. Don't run full suites or Native AOT. The overseer authorizes `NuGetAudit=false` if NU1903 prevents restore. Make one local commit per stage and don't push.

The saved increment is restored and its upstream overlaps are resolved. The existing source-reference implementation is untouched. Stage 2 has typed manifest and catalogue models, strict JSON and YAML decoding, release admission, exact source selection, settings and lock additions, an empty production catalogue and test-only local artifacts. Stage 2 is committed as `c710c38d74b710d72545b5fb1c95eb2e095ab842`. Stage 3 adds List, Inspect and Status. Its focused managed gates pass. The five writers are unstarted.

The overseer directs implementation to use the specification's recommended recovery v2 and TOML choices behind separate seams while maintainer confirmation is pending. These choices no longer block models or readers. Record the exact recovery digest publication rule before writer work. The current recommendation is Tomlyn 2.10.2. No concrete tool or production adapter is admitted.

The earlier contracts-only increment contained the [Tool contracts](../../crystallized/documents/cli/contracts/tool/_tool.md), shared manifest contracts, cross-command contract changes, technical designs and the workspace-state decision. It changed no shipped payload, production code, test source, snapshot or site file. Recovery v2 is a separate proposal pending confirmation, as required by the source specification.

Read-only prose and contract review is complete. R1 clarified Inspect's blocked result for unsupported tuples. R2 added the required V1 graph Skill constraints. R3 centralized coexistence policy and replaced copied leaf rules with summaries and links. All three passed the focused correction recheck. That earlier increment added no command implementation, production tool definition, schema or parser package.

## Saved increment

The following restore history is complete. Don't restore the stash again. Its correct payload work is committed in stage 2. Stash `de681f9d56f95484d789c3782fe2a774ba6fa584` is named `tool-command: preserve guidance payload and fixture changes before deadline`. It keeps the remaining correct work without including it in this commit. Restore only its tracked difference from the stash index and its untracked tree, so the committed contracts and this checkpoint stay intact. Review the resulting tree and keep the stash until its changes are verified and committed.

The earlier restore attempt used these commands. Windows argument limits required a NUL-separated pathspec file for the actual tracked restore:

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


## Stage 3 scope

The reader stage uses the accepted typed shell binding and report pipeline. Each leaf has its own request, result and source-generated JSON data model. The Tool group is registered directly in root composition. Its bare form stops at help. Local source paths resolve from the invocation's process directory. List keeps catalogue and installed observations independent. Inspect keeps readable blocked definitions. Status reads settings, receipts, workspace file hashes, generated host members and passive index presence. It creates no stores, leases, processes, database sessions or downloads.

The consequence and recovery profile stays local and read-only. Malformed source documents, static links, unreadable stores and cancellation are in scope. Existing BCL path checks, source-generated System.Text.Json, standard YAML, settings and lock readers provide the normal path. Exceptional machinery: none. Unit covers parsing and finite finding codes. Integration covers real temporary filesystems, composed input, help, streams and reviewed output. The explicit evening lane policy puts full suites and Native AOT in CI.

Tomlyn 2.10.2 is provisional under the overseer's direction. Framework Tools contains the host-config parser seam and a generated Tomlyn context. Claude JSON rejects duplicate keys. Codex TOML uses the standard DOM and compares the exact member value. The dependency remains subject to maintainer confirmation. The source is [Tomlyn's DOM documentation](https://xoofx.github.io/Tomlyn/docs/dom/). Host editing is stage 4 work.

External file stores need account-local binding evidence. Until stage 4 provides those records, readers report unavailable verification and never follow a receipt's external physical root. Index presence is `present-unverified`. Native completion and source identity readers belong with compiled adapters on the next branch. External instructions stay unavailable until the required verified store facts exist.

Lock receipts now hold store references with IDs, kinds and binding hashes. Physical binding roots stay in the account-local type. Schema v2 and receipt validation reject a physical root in a workspace binding reference. Pending v2 recovery records add unavailable recovery coverage until the safe bundle reader exists. Metadata alone doesn't verify prior bytes.

Writing review R5 rejected catch-all finding codes. The correction adds finite situation codes with fixed statuses and scoped catalogue rows. The focused recheck passed. Other changed public prose passed review, paired Skill references match and checked links resolve.

## Stage 3 evidence

The final Unit build and Integration build pass with zero warnings and errors. The focused Unit gate passes 39 cases. The affected Integration gate passes 265 cases, including the complete report-corpus invariant class. After the recovery coverage guard, the final reader and catalogue rerun passes 41 cases. These populations overlap. All three runs have zero failures and zero skips. The four schemas compile with Ajv 2020 and URI format support. Index preview checks 199 files and plans no changes. The site production build passes with Node's existing localStorage experimental warning.

Accepted 94 reviewed capture files. Fifty refresh existing baselines for the root help row, the changed CLI Skill reference and the resulting byte, token and ratio measurements. Forty-four add the Tool group and leaf help, empty inventories, a local synthetic catalogue, blocked candidate inspection and an unknown registration across the four detail levels. The fixture declares identical candidate artifacts on all six targets. Only the reported host target is replaced with `<native-target>`, and a direct assertion checks the actual target separately. Received files were decoded and written as UTF-8 with LF. Repeated observations of the same baseline matched.

The first Unit runs caught duplicate TOML keys accepted through the generated DOM context. The reader now runs Tomlyn's standard semantic validation before forming the DOM. The strict regression passes. This finding concerns the pinned 2.10.2 integration, not every Tomlyn API. The first affected Integration run had only expected capture failures. A later corpus run rejected the terminal help filenames. The corpus now classifies `.help.txt` separately from operation reports, alongside its existing content and interaction kinds. All operation reports stay in the invariant checks. No current focused-run flake was observed.

Primary review corrections are:

- R6 - Move physical binding coordinates out of workspace receipts. The reference round trip and rejection case pass.
- R7 - Keep guidance hashes separate from installation hashes. Matching, missing, changed, excluded and inactive guidance cases pass. Inactive guidance absence produces no missing-file finding.
- R8 - Report pending v2 recovery-store verification as unavailable. The new case returns incomplete without opening bundle coordinates or creating state.

The exact final build commands are:

```powershell
dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -p:PublishAot=false -p:OpenForgeSkipDevelopmentPublish=true -p:NuGetAudit=false
dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore -p:PublishAot=false -p:OpenForgeSkipDevelopmentPublish=true -p:JsonSerializerIsReflectionEnabledByDefault=false -p:NuGetAudit=false
```

The Unit selection is `*Framework.Tools.*` and `*Commands.Tool.*`. The 265-case Integration selection is the 17 affected classes recorded by stage 2, plus ToolReadCommandsTests, ToolHostStatusTests, CommandHelpIntegrationTests and CliReportInvariantsTests. The 41-case rerun selects ToolReadCommandsTests, ToolHostStatusTests and ToolCatalogueIntegrationTests. Invoke the matching Release DLL with repeated `--filter-class`, then `--minimum-expected-tests 1 --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory <owned-artifact-directory>`. Machine reports are under `artifacts/tool-command/stage3-unit-gate`, `stage3-integration-final` and `stage3-final-readers`. Full suites, published execution and Native AOT are left to CI under the evening lane rule.

## Resume writers

The clean stopping point completes the reader increment, with unavailable observations where writer or native-adapter facts don't exist yet. Don't restore the saved stash. Continue on the same branch after checking Git status and log.

Stage 4 still needs these parts:

1. Implement Install, Configure, Choose, Update and Remove with their typed requests, results, help, consent flags, preview parity and exact exits.
2. Add account-local stores, verified bindings and shared references, bounded transfers and extraction, finite process capabilities and process consent. Tests use local hashed artifacts and a test-only profile.
3. Add paired settings and lock publication, member edits for Claude JSON and Codex TOML, tracked-host checks and managed ignore entries. Keep unrelated values and TOML comments.
4. Add role activation, required guidance, exclusions and root Remove coexistence, plus removal and shared-artifact release.
5. Implement recommended recovery v2 behind its separate seam and choose a non-circular lock and bundle digest publication rule. Add the required Status, Doctor and Cleanup readers before writer effects. Version 1 keeps its existing meaning.
6. Replace unavailable local-store and recovery observations with verified facts when those capabilities exist, and resolve external instructions. Native adapter completion readers and concrete CodeGraph and Codebase Memory definitions stay on the next stacked branch.

The provisional assumptions are Tomlyn 2.10.2 and the spec's recommended v2 ZIP and manifest format. The digest publication rule is still unimplemented. No writer behavior or new recovery effect has been committed. No push, merge or new PR is authorized.

## Linux source fix

The 2026-11-13 restart kept the generic execution, ignore and Git-backup increment in a separate stash named `restart checkpoint for generic execution ignore and Git backup mechanics`. The earlier stashes are untouched.

The Linux CS0246 failure came from three local model files under `Models/Artifacts` excluded by the repository's case-insensitive artifact ignore on Windows. They now live in tracked `Models/Supply`, with both consuming imports updated. No Tomlyn source or package reference survives.

The fresh managed Unit build passed with zero warnings and errors. The focused `ToolManifestTests` and `ToolLifecycleTests` selection passed 48 tests, with zero failures and skips. Exact commands and reports are under `artifacts/tool-command/restart-ci-fix-*`. Both required formatting checks passed. CI still supplies clean Linux and Native AOT evidence.

Continue by restoring only the new restart stash, gate its mechanics, then update the supply contracts for the maintainer's verified-download model. Repeated Install must repair and rerun the pinned installer. Node uses the official pinned private runtime and frozen npm lock. Telemetry defaults off with a visible opt-in. Tool Exec is proposed only. The five writers are still pending.
