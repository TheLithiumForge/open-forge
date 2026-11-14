---
open-forge:
  description: Plan the coordinator's final payload fixtures and reviewed help captures after E's wording
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Final payload pass

The final payload pass is the coordinator's single update window that brings shipped-file fixtures and reviewed CLI snapshots into step with E's final wording.
For example, the new CLI Skill reference changes the installed file count from 18 to 19 and needs both cleanup lists and captured output to agree.

Status: Preparation only. No generation, capture, baseline update, build or test ran in this preflight.

## Inputs

The [Task 90 record](../../tasks/task90-tool-setup.md) assigns final payload regeneration and Index to the coordinator.
E handles wording and the `CommandHelpIntegrationTests` literal-to-Imprint migration.
This plan changes no assignment and adds no review stage.

The read-only inputs are:

- Main - `D:/Repositories/open-forge-worktrees/tool-setup`, HEAD `885b17db3e56ceb6839ba23609e85a4380c75018`, plus its pending contract, Task and rendering changes.
- C - `D:/Repositories/open-forge-worktrees/tool-setup-c`, the same HEAD plus the 34-file Root candidate and `evidence/command-composition.md`. Its receipt reports 113 passing cases and two failing cleanup cases.
- E - `D:/Repositories/open-forge-worktrees/tool-setup-e`, current pending inputs read as background. The final wording and help migration aren't present in the inspected help source yet.
- F - this worktree at `ffef620bca8ae506c611fc3f244e63a72ceae1ec`. Only this report is writable in this preflight.

All source locations below are repository-relative in Main unless marked C.
Run the future commands from Main after the coordinator combines the checked branches, F's reader and E's final changes.
Bind that final input to its full HEAD, pending-file manifest and SHA-256 hashes before captures.
The pending design approves I1 and minimum Open Forge `0.9.0-beta.19`. It doesn't authorize a release-version bump in this pass.

## File inventory

The authored payload is `src/open-forge/**`, including hidden `.agents` files.
It currently contains 21 files: 19 under `.agents` and the two root hosts.
The production project embeds it directly at `src/cli/framework/OpenForge.Cli.Framework/OpenForge.Cli.Framework.csproj:19`.
There is no generated production payload list to edit.
The Integration project copies the same source to `artifacts/bin/OpenForge.Cli.IntegrationTests/release/FrameworkPayloadSource/` at `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj:38`.

Before the build, the coordinator adds `.agents/skills/open-forge-cli/references/tools.md` after `references/routes.md` in these four test lists:

| File below `src/cli/tests/`                                                                                 | Location                             | Result                                                               |
| ----------------------------------------------------------------------------------------------------------- | ------------------------------------ | -------------------------------------------------------------------- |
| `integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallOperationWorkspace.cs`                  | `EmbeddedPayloadPaths`, lines 24-44  | 19 paths. Install and Status cleanup use the same list               |
| `end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallWorkspace.cs`                                       | `EmbeddedPayloadPaths`, lines 19-39  | 19 paths. Published install, relocation and journey helpers reuse it |
| `integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallConfigurationHealthIntegrationTests.cs` | `SkillResources`, lines 14-20        | Five reference files checked when omitting the Skill                 |
| `end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitRestorationProcessTests.cs`                       | Expected `payloadPaths`, lines 25-34 | Six restored Skill files, including `SKILL.md`                       |

C's `artifacts/tool-root/WorkspaceShapeJourneyIntegrationTests-first.txt` reports both failures in `InstallOperationWorkspace.Dispose()`.
Its cleanup uses the list at lines 188-230.
`Commands/Status/StatusInstalledWorkspaceArtifacts.cs:10` and `:21` reuse that list, so they need no second edit.
Keep cleanup's ordinary-file and empty-directory checks.
Don't weaken the temporary-workspace ownership check or use recursive cleanup to hide an omitted path.

The separate checked-in fixture is `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/MainRoot/Fixtures/default-layout.json`.
It has `source`, `files` (base64 bytes), `text` (first Install) and `json` (second, no-op Install).
It currently has 22 file entries and needs 23 after the new reference.
`Shared/MainRoot/DefaultLayoutBaseline.cs:9-25` reads it as resource `MainRoot.DefaultLayout`.
The EndToEnd project embeds it at `OpenForge.Cli.EndToEndTests.csproj:27`.
Its two callers are `PublishedMainRootDefaultCompatibilityProcessTests.cs:16` and `PublishedMainRootOfflineStartupProcessTests.cs:23`.

Build-time embedding, source-generated JSON serializers, `CliBuildVersion.g.cs` and the EndToEnd build-target source are normal generated artifacts under `artifacts/`.
They don't update the checked-in JSON fixture or `__snapshots__`.
The root build targets at `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:32` and `:63` generate version source and the managed development publication.

## Ordered commands

1. Finish E's wording, help migration and the four inventory corrections. Keep semantic effect, exit, stream and no-write assertions. The current help `Pages` source has 36 cases at `Shell/Presentation/Shared/Help/CommandHelpIntegrationTests.cs:12-20`. C lists seven additions: `tool`, `tool list`, `tool inspect`, `tool status`, `tool install`, `tool update` and `tool remove`. E's final source should therefore declare 43 cases. Recount if E changes that source again. The current literal checks at lines 42-73 must be removed by E before capturing.
2. Build the combined source, including the managed development executable needed by the fixture and focused EndToEnd checks. Reuse prepared dependencies. If needed, dependency setup is `npm ci` at Main, separately from capture. No native build is involved.

```powershell
Set-Location D:/Repositories/open-forge-worktrees/tool-setup
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false --disable-build-servers -m:1
if ($LASTEXITCODE -ne 0) { throw 'Managed build failed.' }
$taskRoot = (Get-Location).Path
$taskCli = Join-Path $taskRoot 'artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll'
$integration = 'src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj'
$unit = 'src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj'
$endToEnd = 'src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj'
```

Don't pass `OpenForgeSkipDevelopmentPublish=true` on this first build.
The publication is `artifacts/publish/open-forge-dev/Release/open-forge-dev.exe` on Windows.
`Shared/PublishedProcess/PublishedExecutableTarget.cs:51-78` selects it and checks its version marker.

3. Preview Index for the changed Working subtree, including F's new report and C/E reports. Read its planned files, then apply the same selection. The currently added files all sit in this subtree. Existing Tool contract and Skill files were already routed. Any new E file outside it needs its exact containing entrypoint added to both calls before this step. Index accepts repeated positional sources and only `--dry-run` as its command flag. There is no `--automatic` or `--yes` for Index. See `.agents/skills/open-forge-cli/references/routes.md:153-172`.

```powershell
$route = './.agents/memory/working/cli-development/tool-setup/_tool-setup.md'
dotnet $taskCli index $route --workspace $taskRoot --dry-run --format json --detail full
# Read the preview before the following apply.
dotnet $taskCli index $route --workspace $taskRoot --format json --detail full
dotnet $taskCli index $route --workspace $taskRoot --dry-run --format json --detail full
```

The last preview must show no further navigation changes.
Keep any required recovery evidence from Index.
Don't run repository Doctor, broad Repair or Index against the shipped payload directory.

4. Run only the capture classes listed below in verification mode first. Review the received changes against E's wording, the 19-file payload and semantic effects. New Tool help pages need new baselines. Existing unrelated output must stay byte-equivalent under the existing comparison policy. Then update only the classes whose received changes were reviewed, serially, with the same command and counts.

```powershell
function Invoke-Focused([string] $project, [string] $class, [int] $count) {
    dotnet run --project $project --configuration Release --no-build --no-restore -- `
        --filter-class $class --minimum-expected-tests $count `
        --fail-warns on --fail-skips on --no-ansi --progress off
    if ($LASTEXITCODE -ne 0) { throw "Focused class failed: $class" }
}
$env:OPENFORGE_SNAPSHOT_UPDATE = '0'
$env:IMPRINT_UPDATE = 'verify'
```

Expected baseline mismatches in this first verification are received evidence, not a passing gate.
Run each class separately so a mismatch doesn't prevent collecting another named class's evidence.
For each reviewed update, use this process-local flag scope:

```powershell
$oldSnapshotUpdate = $env:OPENFORGE_SNAPSHOT_UPDATE
$oldImprintUpdate = $env:IMPRINT_UPDATE
try {
    Remove-Item Env:IMPRINT_UPDATE -ErrorAction SilentlyContinue
    $env:OPENFORGE_SNAPSHOT_UPDATE = '1'
    # Invoke only the reviewed rows from the capture table, one at a time.
} finally {
    $env:OPENFORGE_SNAPSHOT_UPDATE = $oldSnapshotUpdate
    $env:IMPRINT_UPDATE = $oldImprintUpdate
}
$env:OPENFORGE_SNAPSHOT_UPDATE = '0'
$env:IMPRINT_UPDATE = 'verify'
```

5. Refresh `default-layout.json` once from the same built input. There is no fixture-update switch or script in the current tree. `OPENFORGE_SNAPSHOT_UPDATE` doesn't reach this resource. The following one-off coordinator command follows `PublishedMainRootDefaultCompatibilityProcessTests.cs:17-30`, `PublishedMainRootWorkspace.cs:55-56` and `:86-89`. It captures fresh Install, then no-op JSON, with owned child homes. It writes a received candidate under `artifacts/`, leaving the checked-in fixture for review.

Proposed:

```powershell
$captureRoot = Join-Path $taskRoot ('artifacts/payload-final-pass/' + [guid]::NewGuid().ToString('N'))
$captureWorkspace = Join-Path $captureRoot 'workspace'
$captureHome = Join-Path $captureRoot 'home'
New-Item -ItemType Directory -Path $captureWorkspace, $captureHome | Out-Null
$captureExe = Join-Path $taskRoot 'artifacts/publish/open-forge-dev/Release/open-forge-dev.exe'
function Invoke-FixtureInstall([string[]] $arguments) {
    $start = [Diagnostics.ProcessStartInfo]::new($captureExe)
    $start.WorkingDirectory = $captureWorkspace
    $start.UseShellExecute = $false
    $start.RedirectStandardInput = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.StandardOutputEncoding = [Text.UTF8Encoding]::new($false, $true)
    $start.StandardErrorEncoding = [Text.UTF8Encoding]::new($false, $true)
    foreach ($argument in $arguments) { $start.ArgumentList.Add($argument) }
    foreach ($name in @('HOME', 'USERPROFILE', 'APPDATA', 'LOCALAPPDATA', 'XDG_DATA_HOME', 'OPENFORGE_DATA_HOME')) {
        $start.Environment[$name] = $captureHome
    }
    $process = [Diagnostics.Process]::Start($start)
    try {
        $process.StandardInput.Close()
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $output = $stdout.GetAwaiter().GetResult()
        $errorOutput = $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0 -or $errorOutput.Length -ne 0) { throw 'Fixture Install failed.' }
        return $output.Replace($captureWorkspace.Replace('\', '\\'), '<workspace>').Replace($captureWorkspace, '<workspace>')
    } finally { $process.Dispose() }
}
$firstText = Invoke-FixtureInstall @('install', '--automatic')
$noOpJson = Invoke-FixtureInstall @('install', '--automatic', '--format', 'json')
$files = [ordered]@{}
foreach ($file in Get-ChildItem -LiteralPath $captureWorkspace -Recurse -Force -File) {
    $relative = [IO.Path]::GetRelativePath($captureWorkspace, $file.FullName).Replace('\', '/')
    if ($relative.StartsWith('.open-forge-state/', [StringComparison]::Ordinal)) { continue }
    $files[$relative] = [Convert]::ToBase64String([IO.File]::ReadAllBytes($file.FullName))
}
if ($files.Count -ne 23) { throw 'Unexpected fixture file inventory.' }
$fixtureCandidate = Join-Path $captureRoot 'default-layout.received.json'
$candidate = [ordered]@{
    source = "$(git rev-parse HEAD) plus final pending inputs recorded in the coordinator manifest"
    files = $files
    text = $firstText
    json = ($noOpJson | ConvertFrom-Json -AsHashtable)
}
$candidateText = ($candidate | ConvertTo-Json -Depth 100).Replace("`r`n", "`n") + "`n"
[IO.File]::WriteAllText($fixtureCandidate, $candidateText, [Text.UTF8Encoding]::new($false))
```

`OPENFORGE_DATA_HOME` is the existing test-store override at `src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs:23`.
The other environment values affect only these child processes.
The capture invokes Framework Install, never `tool install`, a native tool or a package manager.
The omitted `--workspace` matches the fixture's `current-directory` selection.
The runtime directory exclusion matches `TemporaryWorkspace.cs:329` and `:460-464`.
Keep the owned capture directory as evidence during review.

Review the 23 paths, decoded changed payload bytes, settings and lock hashes, first-install count and unchanged no-op meaning before replacing the fixture.
Record the received and accepted file hashes in the coordinator receipt.
After that review, the exact replacement and resource rebuild are:

```powershell
$fixture = Join-Path $taskRoot 'src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/MainRoot/Fixtures/default-layout.json'
Copy-Item -LiteralPath $fixtureCandidate -Destination $fixture
dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
if ($LASTEXITCODE -ne 0) { throw 'Fixture resource rebuild failed.' }
```

The second build refreshes the embedded test fixture. It reuses the already current development publication.
It doesn't capture or accept another baseline.

6. With updates off, rerun each changed capture class and the focused semantic checks below. Run the Markdown, site and mandatory .NET checks after the final source corrections. Keep selected, discovered and executed counts, zero warnings/skips, exit codes, input hashes and artifact paths in the coordinator receipt. A later source correction invalidates only its affected checks and both mandatory .NET checks.

## Focused selections

These are source-declared case counts, not new execution results.
`I` below is `OpenForge.Cli.IntegrationTests.` and `U` is `OpenForge.Cli.Core.UnitTests.`.
Each row uses the exact `Invoke-Focused` command defined above with the indicated project, qualified class and minimum count.
Baseline paths sit beside the indicated class under `__snapshots__`.

The payload and help capture commands are:

```powershell
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Shell.Presentation.Shared.Help.CommandHelpIntegrationTests' 43
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Framework.Distribution.WorkspaceLayoutPayloadRenderingIntegrationTests' 6
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallBeforeOutputSnapshotTests' 10
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallChangeOutputSnapshotTests' 3
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallOperationIntegrationTests' 8
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallSetupRecapIntegrationTests' 5
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallOutsideRouteSetupIntegrationTests' 10
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests' 5
```

The help count is conditional on E's seven stated additions landing, as step 1 explains.
The other seven classes currently declare 47 cases.
The Windows-specific failure case in `InstallBeforeOutputSnapshotTests.cs:98-101` requires this Windows pass. Don't accept a skipped run on another host as its check.
The custom-layout content capture is `Framework/Distribution/__snapshots__/WorkspaceLayoutPayloadRenderingIntegrationTests/CustomGuidanceMatchesReviewedTemplates/custom-guidance.content.txt`.
Its authored `GuidancePaths` at lines 34-44 select eight host/guide/reference inputs. It doesn't enumerate all payload references.
Help baselines use `Shell/Presentation/Shared/Help/__snapshots__/CommandHelpIntegrationTests/<page>/<page>.txt`, including `root/root.txt` and the seven new Tool page names with spaces replaced by hyphens (`CommandHelpIntegrationTests.cs:76-77`).

For final wording that touches the corresponding Tool reports or account question, include only the affected rows here in the same update window:

```powershell
Invoke-Focused $unit 'OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests' 2
Invoke-Focused $unit 'OpenForge.Cli.Core.UnitTests.Presentation.Tool.Shared.Writing.ToolWritePresentationTests' 6
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Tool.ToolCompositionInteractionIntegrationTests' 7
```

The first class records six read projections at `Commands/Tool/Models/Read/ToolReadPresentationTests.cs:23-59`.
The second records writer reports, saved native settings and account consent at `Presentation/Tool/Shared/Writing/ToolWritePresentationTests.cs:23-100`.
C's last class has two reviewed account-choice JSON transcripts under `Commands/Tool/__snapshots__/AccountChoice/`.
Leave those baselines alone if their wording inputs don't change.
Main's currently pending Inspect/Status edits only add braces. They alone require no new baseline.
Likewise, synthetic Extension reports and `help concepts` aren't part of this payload delta unless E changes their own inputs.

The semantic and fixture checks, with updates off, are:

```powershell
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Framework.Distribution.EmbeddedFrameworkPayloadReaderIntegrationTests' 2
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Framework.Distribution.WorkspacePayloadRenderingParityIntegrationTests' 1
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallFrontmatterIntegrationTests' 9
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Install.InstallConfigurationHealthIntegrationTests' 7
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Serialization.InstallGeneratedSerializationTests' 2
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Commands.Shared.Scenarios.WorkspaceShapeJourneyIntegrationTests' 2
Invoke-Focused $endToEnd 'OpenForge.Cli.EndToEndTests.PublishedInstallProcessTests' 6
Invoke-Focused $endToEnd 'OpenForge.Cli.EndToEndTests.PublishedEmbeddedPayloadProcessTests' 2
Invoke-Focused $endToEnd 'OpenForge.Cli.EndToEndTests.PublishedRouteInitRestorationProcessTests' 2
Invoke-Focused $endToEnd 'OpenForge.Cli.EndToEndTests.PublishedMainRootDefaultCompatibilityProcessTests' 2
Invoke-Focused $endToEnd 'OpenForge.Cli.EndToEndTests.PublishedMainRootOfflineStartupProcessTests' 1
```

These 36 cases check real payload parity, the corrected counts, omission/restoration, cleanup and the checked-in fixture.
`PublishedInstallProcessTests` has four facts in its primary file and two cases in `.Outside.cs`.
The EndToEnd project selects its published executable through build-generated facts, not a test environment switch.
Any pre-existing literal help assertion reached by a touched test stays for E's assigned migration or is reported. Don't turn an output mismatch into a new literal expectation.

## Baseline mechanism

`src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:38-61` and `:76-116` define the update behavior.
Only `OPENFORGE_SNAPSHOT_UPDATE=1` opts into `SnapshotUpdate.All`.
Without it, `IMPRINT_UPDATE` can be absent or `verify`. Other values are rejected.
No Imprint auto-accept command is needed.
Received evidence is under `artifacts/command-output-snapshots/<run>/<suite>/<test>/`.
Read the received content and run manifest before updating the named class.
Don't enable updates for a whole project or copy every received file into source.

The wrapper keeps content, case and trailing whitespace exact while ignoring host line endings.
Diagnostic templates with their own comparer stay in Verify even during an update (`CommandOutputSnapshot.cs:102-110`).
A template mismatch needs its existing template rules checked. Persisting a real temporary path would lose that contract.
Snapshot files can intentionally lack a terminal newline. Check them against reviewed Imprint bytes instead of adding one globally.

## Final checks

Run Prettier only on the final changed Markdown/JSON pack.
Keep plain-text snapshot bytes as Imprint wrote them.
Use the coordinator's exact manifest, including untracked files, rather than `prettier --write .`.
For example, with those repository-relative paths in `$changedMarkdownAndJson`:

```powershell
node node_modules/prettier/bin/prettier.cjs --check @changedMarkdownAndJson
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.ToolSetupDocumentationTests' 12
Invoke-Focused $integration 'OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.RepositoryMarkdownTests' 13
npm run check:dotnet:whitespace
npm run check:dotnet:style
git diff --check
```

The 12-case Tool documentation class has its exact source list at `Framework/Documents/RepositoryDocumentation/ToolSetupDocumentationTests.cs:17-76`.
If E removes or renames `Help/tool-setup-planned.md`, correct that inventory at line 74 before freezing it.
The 13-case repository class checks its declared Markdown inventory, including site pages (`Shared/RepositoryMarkdownScope.cs:9-29`).
Neither covers every new Working report or the entire shipped `.agents` tree.
For each changed Markdown file outside those inventories, check its direct links using the built CLI:

```powershell
foreach ($source in $changedMarkdownOutsideInventories) {
    dotnet $taskCli references (('./' + $source.Replace('\', '/'))) --direction out --workspace $taskRoot --format json --detail full
    if ($LASTEXITCODE -ne 0) { throw "Markdown references failed: $source" }
}
```

Read the returned links and coverage. External URLs reported unchecked aren't tested by this command.
Check strict UTF-8, LF, no BOM and trailing whitespace for every changed source/doc file, including untracked files.
Keep reviewed snapshot end-of-file bytes as described above.

When E changes the site, run its existing three-step gate once from Main:

```powershell
npm run docs:build
```

`scripts/delivery/ci/step-plan.ts:63-68` runs `npm ci`, `npm run typecheck` and `npm run build` in `src/docusaurus`.
No second site restore or build is needed for a later Working receipt-only edit.
Full suites, Native AOT, native Tool execution and the fresh whole-feature adherence review stay outside this pass.

## Prerequisites

No new product contract is required for these fixture and snapshot changes.
The current missing inputs are E's final prose and migrated 43-page help source, the combined checked Root graph and the four inventory corrections.
The stored default-layout fixture has no updater API. Step 5 gives a bounded one-off command using its existing shape instead of inventing a test flag.
All commands in this report are planned commands. Only source reads and report formatting/byte/link checks belong to this preparation.
