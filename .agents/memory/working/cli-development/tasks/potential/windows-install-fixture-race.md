---
open-forge:
  description: Candidate follow-up for intermittent Windows integration failures where parallel Install fixtures fail together with DirectoryNotFoundException
  tags: [Memory, Working, CLI, Task, Potential, Contextual, Testing, Windows, Pipeline]
---

# Windows Install Fixture Race

## Question

Do parallel integration tests share machine state that one test removes while another Install writes into it? If so, is the race in the tests only, or can two real Open Forge commands on one machine meet it too?

## Evidence

- A local `test:built` on 2026-10-12 (win-x64, commit `8498d1af5`) failed six native-integration tests within about two seconds of each other. Five ended within 100 ms. Two re-runs of the same executable passed. Managed integration passed in the same run.
- Each failure came from an Install that did not complete. One reported `WriteFailed: DirectoryNotFoundException (0x80070003)` while a Framework fixture established a trusted Install. The failed tests: `ExtensionUpdatePlanningIntegrationTests.InstalledGeneratedCatalogueDoesNotClaimAnUpdate`, `StatusBeforeOutputSnapshotTests.HealthyWithExtension`, `RouteInitBeforeOutputSnapshotTests.FrameworkScaffold`, `InstallRouteSharingIntegrationTests.PrivateMetadataDiagnosisRemainsLocal`, `ExtensionListBeforeOutputSnapshotTests.OwnershipSelection` and `InstallConfigurationFrontmatterIntegrationTests.UnavailableExtensionSourceIsKept`.
- Hosted Builds failed only in Windows test steps four times in ten runs (win-arm64 three times, win-x64 once), with no public detail. [Task 71](../../../../archived/cli-development/tasks/task71-streamline-build-release-pipeline.md#timing-analysis) lists the runs. They may be the same race.
- The initial hypothesis assumed integration tests did not set `OPENFORGE_DATA_HOME`. Code inspection corrected that assumption: the Integration module initializer already redirects the process to an owned temporary data home. Parallel collections share its lock and recovery parent directories, with separate physical-workspace keys ([CurrentUserDataHome.cs](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Filesystem/PhysicalPaths/CurrentUserDataHome.cs)).

## Next steps

1. Qualify the uncommitted fix against a freshly built native Integration executable on the overseer's host, repeatedly with `--parallel collections`.
2. For another Install failure, retain its finding code, subject, cause and last effect, plus the complete CTRF report. Correlate `start` and `stop` with other tests before attributing it to this fixture race.
3. Keep the original native `WriteFailed` question open until that evidence identifies its workspace-effect failure. Hosted failure names and local overlap evidence have not established the same cause.

## Investigation, 2026-10-12

The maintainer selected investigation and repair of these failures. Work stays local and uncommitted.

### Execution applicability

- This is a local development CLI that changes user-owned workspace files. The supported boundary includes ordinary filesystem failures and cooperating commands. The investigation covers accidental test concurrency, not a malicious same-user actor.
- Use existing managed BCL file operations, workspace identity, lock and recovery capabilities. No new dependency or exceptional machinery is selected.
- Start with Integration evidence over real owned stores and a deterministic cleanup interleaving. Run the six affected classes and the complete managed Integration population repeatedly with `--parallel collections`. Native AOT qualification requires the unsandboxed host under the current Architecture.
- The initial store-isolation claim above is inaccurate: `TestProcessIsolation` already calls `TestDataHome.RedirectCurrentProcess` before tests, including at `8498d1af5`. Collections share a process-owned temporary data home rather than the signed-in user's profile.
- A baseline managed run selected 2,997 tests: 2,980 passed, 17 existing Windows platform exclusions, zero failures. Five focused baseline runs each passed all 43 tests from the six affected classes. The reported native `WriteFailed` has not yet been reproduced.
- Before the fix, `RepairIntegrationWorkspace.DeleteExactIfPresent` deleted the fixture's lock file, then walked upward deleting empty directories without a fixture boundary. Product lock disposal only releases a handle, and product recovery deletion only removes exact candidate files.

### Confirmed fixture race

[RepairFixtureIsolationIntegrationTests](../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/RepairFixtureIsolationIntegrationTests.cs) reproduces this interleaving with a real owned lock store, without sleeps or retries:

1. Acquire and release the Repair workspace's real lock handle.
2. Prepare another workspace's shared lock directory, pausing before opening its lock file.
3. Dispose the Repair fixture. Its old cleanup deletes its lock file and the now-empty shared `OpenForge/locks/v1` directory and ancestors.
4. Resume the second lock-file open. It throws `DirectoryNotFoundException` deterministically.

The red CTRF receipt selected one test and reported one failure (`start` 1791794924254, `stop` 1791794924377). The corrected cleanup removes only the fixture's exact lock file. Shared parents remain until process-owned data-home disposal. A fixture-only lock-root parameter keeps the regression's store independent of other collections and uses the existing product path identity.

This is a test isolation defect. Two cooperating product commands do not perform the fixture's upward pruning, so the product stores need no retry or behavior change. No assertion, skip, or snapshot was weakened. The current Crystallized locking and test-isolation contracts already require this behavior and need no change.

### Limit of the finding

The confirmed lock-parent race is not proof of the original native Install cluster. `InstallApplicationOperation` maps lock-acquisition failures to `WorkspaceUnsafe`, and recovery-preparation failures to `RecoveryUnavailable`. Its reported `WriteFailed` comes from a workspace effect. No inspected production store path deletes workspace-effect parents. Preserve that distinction until a failing native run supplies its finding subject and last effect.

### Verification

- Release managed build with .NET SDK 10.0.101 and runtime 10.0.1 passed with zero warnings or errors. The managed Integration DLL SHA-256 is `0b05a04ab78c24d6189ccb2cac1b84b9b75c24a00a3c7144c3c061e7b66d5df8`.
- All 83 Repair tests passed, including the deterministic regression. Five post-fix runs of the six affected classes each passed 43 tests, with zero skips or warnings.
- Three complete post-fix managed Integration runs each selected 2,998 tests: 2,981 passed, 17 existing Windows platform exclusions, zero failures and warnings, exit zero. Their durations were 5m 14.270s, 2m 24.523s and 2m 26.465s. The sole new test accounts for the increase from the 2,997-test baseline. This is the explicitly requested complete Integration gate, not a claim of native qualification or of a complete Unit/EndToEnd gate.
- Focused `dotnet format whitespace` and `dotnet format style` verification passed.
- All 13 repository Markdown checks passed after the record update. `git diff --check` passed. The three changed files remain uncommitted, with HEAD still at `dcb6bb5e0`.
- A full-suite trial with `TEMP` and `TMP` relocated into `artifacts/temp` selected 2,998 tests and reported 2,980 passes, 17 existing Windows exclusions and one Cleanup snapshot failure. Its longer recovery path is truncated by the existing bounded presentation text before exact-path normalization. The unchanged received artifact proves this separate environment-dependent limitation. Snapshots remain unchanged. Requalification uses the same build and the original temporary-directory setting.
- `npm run setup -- --offline` stopped in npm's inaccessible cache. Recreated the authorized dependency junction and used `npm run restore -- --offline`: managed restore succeeded, but the native restore lacked cached `Microsoft.NETCore.App.Runtime.NativeAOT.win-x64` 10.0.1. A direct offline solution restore then succeeded. Native qualification remains with the unsandboxed overseer under the Architecture.

### Exact commands

Run from the task worktree at base `dcb6bb5e0`, with the uncommitted fixture fix and regression. The executable below is the Release managed apphost loading its adjacent DLL, despite the runner's compiled `Native AOT` framework label. CTRF files preserve per-test timing and complete populations. No new skips or warnings were introduced.

```powershell
$env:APPDATA = Join-Path $PWD 'artifacts/appdata'
$env:OPENFORGE_DATA_HOME = Join-Path $PWD 'artifacts/data-home'
dotnet restore OpenForge.Cli.slnx --source artifacts/delivery/offline-feed -p:NuGetAudit=false
dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true

$exe = 'artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe'
$common = @('--parallel', 'collections', '--no-ansi', '--progress', 'off',
    '--report-xunit-ctrf', '--report-xunit-ctrf-filename', 'results.json')

# Before removing upward pruning: one test selected, one deterministic failure.
& $exe @common --filter-class '*RepairFixtureIsolationIntegrationTests' --results-directory artifacts/test-results/cleanup-red

# After the fix: 83 selected and passed, including the regression.
& $exe @common --filter-class 'OpenForge.Cli.IntegrationTests.Commands.Repair.*' --results-directory artifacts/test-results/repair-green

# Five runs, 43 selected and passed each time.
$classes = @('*ExtensionUpdatePlanningIntegrationTests', '*StatusBeforeOutputSnapshotTests',
    '*RouteInitBeforeOutputSnapshotTests', '*InstallRouteSharingIntegrationTests',
    '*ExtensionListBeforeOutputSnapshotTests', '*InstallConfigurationFrontmatterIntegrationTests')
$filters = @()
foreach ($class in $classes) { $filters += @('--filter-class', $class) }
for ($run = 1; $run -le 5; $run++) {
    & $exe @common @filters --results-directory "artifacts/test-results/affected-green-$run"
    if ($LASTEXITCODE -ne 0) { throw "Affected run $run failed" }
}

# Original TEMP/TMP setting. Three complete managed Integration runs.
for ($run = 1; $run -le 3; $run++) {
    & $exe @common --results-directory "artifacts/test-results/full-qualified-$run"
    if ($LASTEXITCODE -ne 0) { throw "Full run $run failed" }
}

dotnet format whitespace src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --no-restore --verify-no-changes --include src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/RepairIntegrationWorkspace.cs src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/RepairFixtureIsolationIntegrationTests.cs
dotnet format style src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --no-restore --verify-no-changes --severity warn --include src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/RepairIntegrationWorkspace.cs src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/RepairFixtureIsolationIntegrationTests.cs
& $exe @common --filter-class 'OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.RepositoryMarkdownTests' --results-directory artifacts/test-results/record-docs
```

`TestProcessIsolation` replaces the shell's `OPENFORGE_DATA_HOME` with a unique process-owned temporary data home before tests start. The focused post-fix runs used `TEMP`/`TMP` under `artifacts/temp`; complete qualification retains the original temporary-directory setting because of the separate long-path snapshot limitation above. All test stores remain owned temporary resources rather than the real per-user Open Forge store.

## Follow-up investigation, 2026-10-12

The maintainer retained the Repair fix and supplied timing from the original native report. This follow-up still owns the unresolved `WriteFailed` question, not another locking fix.

### Supplied overlap

Times are milliseconds relative to the first failed test's start. Five failures ended at 9573, 9577, 9598, 9621 and 9670. Extension List spanned 0..11212. `UpdateOwnershipSafetyIntegrationTests.InvalidStaleClaimDoesNotAuthorizeAnyDeletion` with target `.agents/open-forge.lock.json` ran 8766..9535. Its `.git/config` case started at 9535. Nearby cases were Route Init snapshots (8741..9710), Library Detach snapshots (8601..9416, including `write-failed-partial`), Route Move reference affectedness (9091..9856), the denied Route Remove source deletion (9421..9895), and Update recovery ordering (9710..10556). Recovery deletion storage-boundary cases ran earlier, at about -1700. This is correlation, not proof of an actor deleting another fixture's paths.

### Audit and diagnostic reproduction

- The CLI source and test trees have no difference between the failing `8498d1af5` and this worktree's `dcb6bb5e0` base. The failure mapping is therefore the same, not a later change that concealed a recovery failure.
- Audited `Directory.Delete`, `DirectoryInfo.Delete`, `Directory.Move`, parent traversal, temporary-root usage and environment changes in Integration and shared TestSupport. The retained Repair fix is the only observed unbounded shared-parent pruning. Other external recovery cleanup targets use the full physical-workspace SHA-256 bucket identity. `TestDataHome` deletes its process-owned root only at process exit.
- The Update ownership-safety fixture's registered cleanup paths are constructed through its own `InstallOperationWorkspace.Combine`. Its disposal deletes its own installed workspace, own lock store and own keyed recovery bucket. The invalid-claim case asserts zero effects and unchanged workspace bytes. It neither replaces nor prunes shared recovery parents.
- Recovery storage-boundary tests replace their own bucket with a link and remove that link during disposal. The ancestor-link cases create separate owned `store/recovery` trees, not the suite's actual shared ancestors. The nearby Library Detach and Route Remove partial-failure cases deny sharing on their own workspace files. Route Move cleanup and Update recovery ordering remain within their own workspace or keyed bucket.
- Only one production directory-deletion call exists: the contained, expected-state and emptiness-validated `DirectoryDeletionApplier`, using nonrecursive deletion. Product recovery deletion removes files. No inspected actor explains disappearance of an unrelated Install workspace parent.
- After recovery preparation, `FileChangeApplier` checks preparation identity in memory. Write application accesses only its workspace-adjacent stage and target. Recovery-directory disappearance would not itself throw a workspace-effect `WriteFailed` here. Lock acquisition and preparation still map to `WorkspaceUnsafe` and `RecoveryUnavailable` respectively.
- A temporary first-chance probe captured missing-directory exceptions only from the file/directory effect appliers into ignored logs. Ten repetitions of the twelve relevant classes passed 106/106 each. Full managed runs at 24 threads and unlimited collection concurrency each passed 2,981 with the same 17 exclusions. Neither run produced a matching missing-directory exception. The probe was removed after the experiment.

### Evidence improvement

Trusted Install fixtures now preserve findings with target paths, effect order and outcomes, workspace-root existence, affected-parent existence, and recovery residual details on failure. The Extension Install fixture captures full JSON when its seed fails. Existing success, exit, finding and filesystem assertions remain in force. This supports the next native occurrence without adding a product retry, skip or speculative behavior change. It does not establish or fix the reported root cause. Crystallized product behavior is unchanged.

The native failure still needs its target/effect details. Native publication remains unavailable in this sandbox and its cached native runtime pack is missing. The supplied timings alone do not justify naming a remover or changing product semantics.

### Follow-up commands

The twelve-class diagnostic selection extends the six-class `$classes` above with:

```powershell
$nearby = @('*UpdateOwnershipSafetyIntegrationTests', '*LibraryDetachBeforeOutputSnapshotTests',
    '*RouteMoveReferenceAffectednessIntegrationTests', '*RouteRemoveBeforeOutputSnapshotTests',
    '*UpdateRecoveryOrderingIntegrationTests', '*RecoveryDeletionStorageBoundaryIntegrationTests')
$diagnosticFilters = @()
foreach ($class in ($classes + $nearby)) { $diagnosticFilters += @('--filter-class', $class) }
for ($run = 1; $run -le 10; $run++) {
    & $exe @common @diagnosticFilters --results-directory "artifacts/test-results/writefailed-trace-focused-$run"
}
& $exe @common --max-threads 24 --results-directory artifacts/test-results/writefailed-trace-full
& $exe @common --max-threads unlimited --results-directory artifacts/test-results/writefailed-trace-unlimited
```

Those experiments used the temporary probe build and set `OPENFORGE_RACE_TRACE` to an ignored log under `artifacts`. They are managed investigation evidence, not native qualification.

### Final managed qualification of the failure evidence

The probe-free Release build passed with zero warnings and errors. Its Integration DLL SHA-256 is `50c6849a1096f9e2f64ac6b36edba6b0ee610a4613c38aa7fb18b6770cfa8806`. The six affected classes passed 43/43 in each of five runs. All 30 Install output tests passed, including the real Windows partial-write failure that verifies the diagnostic includes its affected target and retained bundle. Three complete managed runs each passed 2,981 tests, with 17 unchanged platform exclusions and zero failures; durations were 2m 50.548s, 2m 04.904s and 2m 45.763s. Whitespace and style verification passed for all eight diagnostic changes. These passes establish no reproduction of the original failure.

Exact qualification commands reuse `$exe`, `$common` and `$filters` from the first investigation above, with the original `TEMP` and `TMP` settings:

```powershell
$env:APPDATA = Join-Path $PWD 'artifacts/appdata'
$env:OPENFORGE_DATA_HOME = Join-Path $PWD 'artifacts/data-home'
dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
for ($run = 1; $run -le 5; $run++) {
    & $exe @common @filters --results-directory "artifacts/test-results/writefailed-final-focused-$run"
    if ($LASTEXITCODE -ne 0) { throw "Focused run $run failed" }
}
& $exe @common --filter-class '*InstallBeforeOutputSnapshotTests' --results-directory artifacts/test-results/writefailed-final-install
for ($run = 1; $run -le 3; $run++) {
    & $exe @common --results-directory "artifacts/test-results/writefailed-final-full-$run"
    if ($LASTEXITCODE -ne 0) { throw "Full run $run failed" }
}
& $exe @common --filter-class 'OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.RepositoryMarkdownTests' --results-directory artifacts/test-results/writefailed-final-docs
```

All changes remain uncommitted. No product contract changed. The original native `WriteFailed: DirectoryNotFoundException` remains unresolved; the retained Repair cleanup fixes a separately proven test-only race.
