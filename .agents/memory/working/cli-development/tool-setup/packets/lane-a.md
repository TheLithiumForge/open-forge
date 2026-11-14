---
open-forge:
  description: Implement Task 90 lane A channels and hosts
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Channels and hosts

Lane A is the implementation that detects published channels and installs one active tool with verified bytes and direct launchers.
The coordinator supplies the exact execution base in the dispatch prompt.

Status: Prepared for Phase 2. Work starts only through the assigned Worker Watch run.

## Read first

Read AGENTS.md and .agents/loader.md in your worktree and load applicable Open Forge scopes before changing files. Independently read the complete current .agents/directives/csharp/_csharp.md, design.md and style.md, plus CLI, software, Open Forge CLI, testing and work-planning scopes. Read the current CLI architecture, selected Task 90 and these exact spec files:

- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md
- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup-lanes.md
- .agents/memory/crystallized/documents/cli/contracts/tool/_tool.md, interface.md and behavior.md, then your leaf contracts
- .agents/memory/crystallized/documents/cli/contracts/shared/tool-records/interface.md and behavior.md
- .agents/memory/crystallized/documents/maintenance/tools.md
- D:/Repositories/open-forge-worktrees/of-lane-rules.md
- D:/Repositories/open-forge-worktrees/of-tool-setup-decisions-2.md

The maintainer's C1 through C7 decisions in the design override the older round-2 file. CLI selectors are bare ID, ID@major.minor or ID@exact stable SemVer. No CLI minor.x, dist-tags or full npm range expressions. Bare Install restores exact lock versions. A named bare ID selects latest support. Use --yes, no --automatic or --allow-download. No Open Forge per-tool Skill or managed-block guidance.

Codebase Memory uninstall is account-wide, including the binary and all agent entries. It needs live interactive confirmation naming that effect. With --yes, no terminal or a declined confirmation, skip it, remove only Open Forge project additions and print its native command. Project Remove never restores account-wide native settings. Keep them, say so and print native manual change commands. No cross-project setting registry. These intentional kept effects do not alone make project removal incomplete.

## Shared limits

You are not alone in the codebase. Keep other lanes' work and never revert their edits. Work only in your assigned worktree and writable paths below. The master worktree and maintainer checkout are read-only to you. No push, PR, GitHub mutation or local integration merge. Use local Git objects for old code. Public upstream research and dependency restore are authorized for the assigned task. Don't inspect credentials or unrelated private stores.

The user chooses gpt-6.1-sol with xhigh effort, overriding the older Worker Watch roster. Don't spawn more agents. Start your response with your agent name, model and effort from the invocation.

Never change real persistent user PATH, registries or real shell profiles. Root PATH capabilities must be testable through owned test paths or a fake host. No real tool installation in this first implementation pass. E handles real journeys later in owned temporary homes and Git repos under D:.

Models under src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/, schemas, actual .agents/open-forge.json and .agents/open-forge.lock.json, parent Task 90, shared contracts, all other packets and central composition are protected. A required shared change is a separate proposed contract(tool): ... increment. Report CONTRACT_REQUEST with exact changes and affected consumers before implementing against it. The mastermind integrates the contract first. Do not create private substitutes or copy shared contracts into your lane.

Tool operations add no state, lease, journal or recovery bundle under .open-forge-state/. Repository facts and additive receipts go in the committed lock. Native previous settings and machine facts stay in per-user state. Tool commands do not run concurrently by design. Keep expected-state checks, physical containment, unchanged unrelated data, atomic replacement and Git-aware sibling backups. No automatic rollback.

Use the existing layer graph: Framework mechanisms and data, Operations command behavior, Rendering presentation, Root privileged process/network/platform hosts and wiring. Match physical paths and namespaces. Use the BCL and existing packages, source-generated concrete JSON and existing command result conventions. A new dependency, project or exceptional platform mechanism needs an upward request.

## Verification

Run npm ci in your worktree. For the later site pass also run npm ci in src/docusaurus. Keep LF. Do not run Doctor in this repository workspace.

Use a managed solution build, then only tests you touch or create. Never run complete Unit, Integration, EndToEnd or native suites, unrelated test classes or build:native. CI is the full gate. Before a commit run npm run check:dotnet:whitespace and npm run check:dotnet:style.

A local build can use:
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
Never commit that local NU1903 override. Never change PATH to make builds pass.

Use independently selectable tests with the right tier. For each touched class run the corresponding managed test project with --filter-class <qualified-class>, --minimum-expected-tests <actual-count>, --fail-warns on, --fail-skips on, --no-ansi and --progress off. Name the exact command, counts, artifact and result in your receipt. New tests use synthetic entries and owned temporary files, homes and processes. Never use a real per-user store. Avoid production test switches that alter semantics.

No literal assertions on user-facing text, help or rendered output. Use reviewed Imprint snapshots. Semantic assertions use the production owner for finding codes, environment names, paths and schema fields. Do not copy shipped payload strings into fixtures. Regeneration of payload-driven snapshots and fixtures is reserved for the integrator once, after all implementation and E's docs. Report any such pending snapshot impact.

## Return and commit

Update only your own packet's Progress and Evidence sections with the observed steps and exact checks. Do not edit parent state or common catalogues. Report new routed files so the integrator can Index once.

First finish the implementation and focused checks, then return READY_FOR_REVIEW with the changed files, test evidence, contract requests and remaining gaps. Leave the changes uncommitted for the coordinator's grouped review and commit continuation. Do not wait inside a running shell.

On the coordinator's continuation, apply the accepted grouped fixes if any, repeat only affected checks, make a local conventional commit with no attribution and report its full SHA. This packet includes that final commit-and-report assignment. Do not push or merge. No fabricated timestamps.

If a blocker affects only part of the assignment, finish independent work and report the exact blocked requirement. Never silently omit a requirement or call an unimplemented path complete.

## File assignment

You alone edit Framework/Tools/Shared/{Supply,PackageManagers,Runtimes,Launchers,Path,Versions}/** below src/cli/framework/OpenForge.Cli.Framework/, and src/cli/root/OpenForge.Cli/Composition/Shared/Tools/**. You own matching paths in Unit and Integration test projects and this lane-a.md packet. C owns all record codecs. B owns config edits and backups. The integrator owns root command composition, central registration, shared models and schemas.

Implement IToolVersionService, IToolChannelService, IToolProcessHost, IToolDownloadHost and IToolPathHost from Framework/Tools/Models/Services/ToolServiceContracts.cs. Keep host implementations in Root and channel policy in Framework. Expose a focused construction entry that Root composition can use. Root supplies resolved home paths. No new public environment switch unless already accepted or returned as a contract request.

## Steps

Follow this ordered implementation plan:

1. Check Git base and applicable context. Read all Models/Catalogue, Channels, Launchers, Records and Services files. Read existing root process/network capabilities before making another owner. Reuse only the exact lane-A copy sources in tool-setup-lanes.md, through git show, with their tests. Never cherry-pick.
2. Implement minor/exact spec parsing and profile matching at Framework/Tools/Shared/Versions/. CLI minor.x is invalid. Catalogue/settings minor.x is valid. Reject prereleases and versions below the profile floor. Verify all cases through focused pure tests.
3. Implement bounded command detection and npm-family capability facts under PackageManagers/. Probe npm, pnpm, yarn and bun separately, excluding Yarn Modern globals. Honor per-client minimums, the entry's RequiredClient, runtime requirements, project-lockfile preference, multiple-client choices, --yes fallback and global-bin readiness. Prove choices through injected process results, not a real account install.
4. Implement published npm global installation, existing-install reuse and GitHub resolution/download/extraction under Supply/. Do not silently make a second install or switch channels after a failure. Honor registry/proxy trust and avoid leaking credentials. Exact versions are shown and recorded. Direct assets need catalogue-pinned SHA-256 or verified attestation. Use manager verification honestly, without claiming dependency closure.
5. Implement private Node under Runtimes/. Use official portable Node archives with SHA-256 pinned from signed SHASUMS256. Skip archive links in preparation before the unchanged link-rejecting extractor. Run the real node executable with npm-cli.js at its platform path. Install packages with that Node into tools/<id>/<version> so native addons match it. Share versioned runtimes. Several versions can coexist. Never proxy Node or put it in bin.
6. Implement absolute-path .cmd and executable sh launchers plus direct native binaries under Launchers/. Update rewrites them. Open Forge is absent at run time. Verify Windows argument quoting and ordinary Unix permissions in owned files. Record the cmd /c MCP constraint as native journey evidence, without rewriting upstream integration.
7. Implement PATH observation and the Root platform host. Interactive Windows writes use Environment.SetEnvironmentVariable(..., User), never setx or system PATH. Unix offers detected startup files and a custom file, writing self-identifying marked blocks only to chosen files. Show exact manual lines and restart instructions. Removal needs live confirmation and no launcher can still use a deleted runtime. Tests never execute real persistent user PATH or profile edits. Inject a platform host or exercise only redirected files.
8. Build and run only the tests you created or touched. Check no real user paths changed. Run both .NET checks. Return the construction entry, files, exact test evidence and READY_FOR_REVIEW. The coordinator handles the commit continuation.

## Acceptance

Cover stable spec selection, supported existing reuse, no duplicate install, npm-family requirements and preference, Node absence, pinned private runtime installation, archive escape/link rejection, SHA mismatch, bounded timeout/cancellation/output, dry-run latest-only lookup disclosure, launchers and chosen PATH files. Verify using local fixtures and owned temporary homes. GitHub production access is not required by tests. CodeGraph's version probe must carry CODEGRAPH_NO_DOWNLOAD=1 exactly as declared by E's later entry. Never alter normal tool fallback behavior.

Dependency direction and no real PATH writes matter independently of test counts. Do not add package-manager implementation policy to command rendering or record codecs.

## Progress

Done: Implemented the three integrated ABI additions, A-R1 and the responsible-manager update checks on `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`. The managed solution build, 191 focused tests and both .NET checks pass. The 12 reviewed portable Node pins are kept.
Now: Task 90 "Tool setup" (phase 3/3): milestone 3/7. The coordinator accepted the complete 191-test A candidate for a local implementation commit.
Next: The coordinator integrates the local commit. C handles central Root composition. F keeps private ToolHomes and the native reader. Both use the committed host API.
Blocker: None after contract commit `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.

## Evidence

### Local commit

The coordinator accepted the complete candidate and authorized its local conventional commit. All 57 C# source hashes and all four final assembly hashes match the accepted receipts. The 20 independent class receipts still total 191 passing tests with zero failures and skips. The accepted managed build and class runs are reused. Only this packet's progress and receipt formatting change during the commit continuation. LF and whitespace pass again. Both mandatory .NET checks pass again. Their logs are `artifacts/tool-setup-lane-a/abi/commit-check-whitespace.log` and `commit-check-style.log`. Final file hashes and validated class receipts are in `commit-source-hashes.json` and `commit-receipts.json` in the same directory. No additional tests ran for these prose changes.

### ABI continuation

The coordinator fast-forwarded this worktree to `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`, keeping the 162-test candidate. All three requested ABI additions and the unused settings-grant interface are integrated. This continuation supersedes the earlier blocker reports below.

This change affects developer files and native account operations through the existing service contracts. The supported failure cases are malformed metadata, cancellation, unavailable configuration and accidental file drift. Git, sibling backups and atomic replacement provide the existing recovery choices. No same-user adversarial guarantee is added. Root uses the BCL HTTP, proxy, TLS and source-generated JSON APIs. No new dependency, project, platform workaround or public environment switch is required. Focused Integration tests cover Root metadata and owned files. Unit tests keep the existing pure policies. Full suites, native journeys and payload snapshots stay with the integrator and CI.

A-R1 is accepted and fixed at `ToolPackageManagerPolicy.ReadEnvironment`. Manager version, bin, root and metadata reads receive the immutable child overlay. Normal install vectors keep their normal environment. An uncached Corepack client stays unavailable. Metadata queries through an available native client can still contact that client's registry. Tests use an owned fake Corepack-like shim and a package manifest sentinel.

Current construction for C and F uses `ToolHostFactory`:

- `CreateVersions()` returns `IToolVersionService`.
- `CreateProcesses(IReadOnlyDictionary<string, string?>? environment = null, ToolCatalogue? catalogue = null)` returns `IToolProcessHost`.
- `CreateDownloads(string resolvedUserHome, IReadOnlyDictionary<string, string?> environment)` returns `IToolDownloadHost`. Pass the resolved Node/npm user home, not the `.open-forge` directory. This factory creates the working metadata host and uses the BCL platform proxy.
- `CreateChannels(IToolProcessHost processes, IToolDownloadHost downloads, IToolBackupService backups, IToolRecordService records, IReadOnlyDictionary<string, string?> environment)` returns `IToolChannelService`.
- `CreateNodeRuntimes()` returns `IReadOnlyList<ToolPrivateNodeSpec>` with the 12 reviewed archive pins.
- `ObserveTargetAsync(IToolProcessHost processes, string workspace, CancellationToken cancellationToken)` returns `Task<ToolTarget>`.
- `CreatePath(string resolvedShellHome, IToolBackupService backups, Func<bool> hasLiveConsent)` returns `IToolPathHost`.

`ToolChannelService` keeps the same five-parameter constructor as `CreateChannels`. `ToolHttpTransfer(ToolNpmMetadataHost metadata, HttpClient? client = null, IProgress<ToolTransferProgress>? progress = null)` requires its metadata capability. Tests can construct `ToolNpmMetadataHost(ToolNpmMetadataEnvironment environment, Func<ToolNpmMetadataConnection, HttpClient>? connect = null)` with owned homes, additional config paths, a supplied platform proxy and an owned HTTP-client factory. The returned client belongs to that lookup and is disposed. The Worker Watch availability check found F unavailable for a follow-up. The coordinator can forward these signatures.

Root metadata uses a 30-second cancellation bound, an 8 MiB response bound, at most 4096 version entries and at most three same-registry redirects. The concrete JSON graph uses source generation. Registry and explicit proxy environment settings are honored. Standard proxy bypass stays with the supplied platform proxy. TLS verification stays enabled. Unsupported authentication, custom Node trust settings, TLS bypass settings and nonempty npm config files block fallback before HTTP. Credential environment values and config-file contents aren't read or returned. Empty redirected config files are permitted. A verified native client remains the preferred metadata reader and uses its own native configuration.

The exact npm path always comes from the channel declaration and stays offline. Without a client, latest reads Root metadata and selects the actual supported stable patch. Published Node bounds further restrict the approved runtime selection. Existing private npm updates keep a private prefix and Node even when global npm is available. External replacements require the responsible client and its same global-bin prefix. Unknown sources and manager prefixes block before a second installation.

PATH results now carry every pending backup handle, including an unverified handle when a replacement stops after backup preparation. D completes them after publication and final verification. No service completes them early. All A-owned fake download and record services implement the integrated interfaces, including unused settings grants.

### Completed ABI checks

READY_FOR_REVIEW is the complete checked A candidate on base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`, tree `20c25b59229c7907411ba616e1d696e88e2638e5`. All three earlier contract requests are resolved. A-R1 is fixed. No new contract request or implementation blocker remains. Changes are uncommitted for grouped review and the coordinator's commit continuation.

The final managed solution build passed with zero warnings and zero errors using SDK `10.0.101`. The exact command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The invocation-only audit override isn't committed. `npm ci`, `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` pass. LF and `git diff --check` pass. Build and check logs are under `artifacts/tool-setup-lane-a/abi/`. The restore log is `artifacts/tool-setup-lane-a/npm-ci-abi.log`.

All 20 created or touched classes were selected independently. The final combined receipt has 191 distinct passing tests, zero failures and zero skips. Every command rejects warnings and skips. The measured counts are:

| Tier        | Class                             | Minimum | Passed | Failed | Skipped |
| ----------- | --------------------------------- | ------: | -----: | -----: | ------: |
| unit        | `ToolVersionServiceTests`         |      34 |     34 |      0 |       0 |
| unit        | `ToolRuntimeRangeTests`           |      16 |     16 |      0 |       0 |
| unit        | `ToolPackageManagerPolicyTests`   |      16 |     16 |      0 |       0 |
| unit        | `ToolArtifactIntegrityTests`      |       7 |      7 |      0 |       0 |
| unit        | `ToolTargetPolicyTests`           |       8 |      8 |      0 |       0 |
| unit        | `ToolLauncherRenderingTests`      |       1 |      1 |      0 |       0 |
| unit        | `ToolNodeRuntimeCatalogueTests`   |       2 |      2 |      0 |       0 |
| integration | `ToolLauncherWriterTests`         |       2 |      2 |      0 |       0 |
| integration | `ToolArchiveExtractorTests`       |      15 |     15 |      0 |       0 |
| integration | `ToolPrivateNodeInstallerTests`   |       3 |      3 |      0 |       0 |
| integration | `ToolProcessHostTests`            |       5 |      5 |      0 |       0 |
| integration | `ToolHttpTransferTests`           |       5 |      5 |      0 |       0 |
| integration | `ToolPathHostTests`               |       7 |      7 |      0 |       0 |
| integration | `ToolCommandResolverTests`        |       4 |      4 |      0 |       0 |
| integration | `ToolTargetObserverTests`         |       8 |      8 |      0 |       0 |
| integration | `ToolChannelServiceTests`         |      16 |     16 |      0 |       0 |
| integration | `ToolNpmChannelTests`             |      16 |     16 |      0 |       0 |
| integration | `ToolManagedFilesRemovalTests`    |       4 |      4 |      0 |       0 |
| integration | `ToolPackageManagerDetectorTests` |       5 |      5 |      0 |       0 |
| integration | `ToolNpmMetadataHostTests`        |      17 |     17 |      0 |       0 |

The initial new metadata run exposed two uncaught typed metadata errors and three assertions that forgot the test workspace's existing marker. The grouped correction catches `InvalidDataException`, compares the existing file inventory and uses valid oversized JSON for both size-bound cases. The final solution build passed again and only the affected 17-test metadata class was repeated. All 17 passed. Its original failed receipt is kept as `first-run.log` and `first-results.json`. The earlier compile-only accessibility finding was fixed before the successful test run.

Each class has its exact `command.txt`, `run.log` and `results.json` under `artifacts/tool-setup-lane-a/abi/<Class>/`. `test-summary.json` joins the final counts. `first-assembly-hashes.json` identifies the first 20-class run. `assembly-hashes.json` identifies the final binaries. `source-hashes.json` identifies the final owned files. The exact class commands are:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions.ToolVersionServiceTests --minimum-expected-tests 34 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolVersionServiceTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions.ToolRuntimeRangeTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolRuntimeRangeTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.PackageManagers.ToolPackageManagerPolicyTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolPackageManagerPolicyTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply.ToolArtifactIntegrityTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolArtifactIntegrityTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply.ToolTargetPolicyTests --minimum-expected-tests 8 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolTargetPolicyTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Launchers.ToolLauncherRenderingTests --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolLauncherRenderingTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Runtimes.ToolNodeRuntimeCatalogueTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolNodeRuntimeCatalogueTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Launchers.ToolLauncherWriterTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolLauncherWriterTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolArchiveExtractorTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolArchiveExtractorTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Runtimes.ToolPrivateNodeInstallerTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolPrivateNodeInstallerTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolProcessHostTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolProcessHostTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolHttpTransferTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolHttpTransferTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Path.ToolPathHostTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolPathHostTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolCommandResolverTests --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolCommandResolverTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolTargetObserverTests --minimum-expected-tests 8 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolTargetObserverTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolChannelServiceTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolChannelServiceTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolNpmChannelTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolNpmChannelTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolManagedFilesRemovalTests --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolManagedFilesRemovalTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.PackageManagers.ToolPackageManagerDetectorTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolPackageManagerDetectorTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Npm.ToolNpmMetadataHostTests --minimum-expected-tests 17 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/abi/ToolNpmMetadataHostTests
```

The current candidate has 36 production files, 20 test classes, one shared test fixture and this packet. The complete path list is `artifacts/tool-setup-lane-a/abi/changed-files.txt`. New continuation files are the six Root files under `Composition/Shared/Tools/Npm/` and `ToolNpmMetadataHostTests.cs` at the matching Integration path. No shared Model, schema, common catalogue, record codec, central composition, other packet or other worktree was edited. No new routed Markdown file needs Index.

Tests used owned temporary homes and files, injected process results, an owned Corepack-like shim and injected HTTP responses. No real tool installation, persistent PATH or registry write, real shell-profile change, credential-store read, GitHub mutation or payload-snapshot regeneration ran. No dependency, project, public environment switch, Tool workspace state, lease, journal or automatic rollback was added. E's real native and MCP journeys and CI's full managed and Native AOT gates stay with their assigned stages.

### Earlier continuation

The coordinator fast-forwarded this worktree to `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9` while keeping the candidate. The earlier checked-candidate sections below describe the first run and aren't the current acceptance receipt.

Proposed CONTRACT_REQUEST `contract(tool): declare npm entries and metadata reads`:

- Add `public required string EntryRelativePath { get; init; }` to `ToolNpmChannel`. E declares CodeGraph's `npm-shim.js`. A can then plan the exact future Node invocation without network access for an exact selector. Consumers are E's entries, A's planner and D's exact execution reports.
- Add `Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)` to `IToolDownloadHost`. Proposed models are `ToolNpmMetadataRequest(ToolNpmChannel Channel, string WorkspacePath)`, `ToolNpmVersionMetadata(string Version, string EntryRelativePath, string? NodeRange)` and `ToolNpmMetadataResult(IReadOnlyList<ToolNpmVersionMetadata> Versions, Uri? Registry, string? Failure)`. Root supplies this read for latest resolution when no client can run. It must honor registry and proxy trust, and block unsupported authenticated configuration without reading or disclosing credentials. Consumers are A's Root transfer host and Framework planner, plus injected hosts in C/D/F tests. A failed configuration read can leave `Registry` null.
- Add required `IReadOnlyList<ToolBackupCompletionRequest> PendingBackups` to `ToolPathSetupResult` and `ToolPathRemoveResult`. PATH writes currently lack a result channel for retaining backups through D's record publication. Consumers are A's PATH host and D/F's publication and journey code.

No proposed shared type was edited. The implementation proceeds on the committed ABI and reports blocked paths honestly until the required declarations are coordinated.

Construction signatures for F are `CreateProcesses(IReadOnlyDictionary<string, string?>? environment = null, ToolCatalogue? catalogue = null)`, `CreateDownloads()`, `CreateVersions()`, `CreateChannels(IToolProcessHost processes, IToolDownloadHost downloads, IToolBackupService backups, IToolRecordService records, IReadOnlyDictionary<string, string?> environment)`, `CreateNodeRuntimes()`, `ObserveTargetAsync(IToolProcessHost processes, string workspace, CancellationToken cancellationToken)` and `CreatePath(string resolvedShellHome, IToolBackupService backups, Func<bool> hasLiveConsent)`. These are methods on `ToolHostFactory`. The return types are the corresponding service interfaces, `IReadOnlyList<ToolPrivateNodeSpec>` for the catalogue and `Task<ToolTarget>` for target checks. Root passes its full base environment to both the process host and channel service. Pass the catalogue to the process host so an unrecorded bundled command can resolve through its declared runtime and entry. `CreateNodeRuntimes()` returns the pinned portable catalogue. The attempt to send these details to F through Worker Watch was rejected because F is busy. A later availability check also showed F and the coordinator busy. No new run was started.

`npm ci` passed again on this continuation. The first compile exposed missing interface implementations and fixture updates from the revised ABI. These checks are development failures, not an acceptance gate.

### Channel candidate

`ToolChannelService` implements detection, separate manager probes, version selection, exact execution plans, installation and shared-file removal. Its constructor has the same five parameters as `CreateChannels`. The planner reads local package entries for an exact selector and uses bounded native registry queries for latest selectors. Existing supported installations keep their responsible channel and manager. A failed chosen operation stops without another install or channel switch.

Private packages use the chosen portable Node's real npm CLI with a versioned global prefix under `tools/<id>/<version>`. The install process prepends that Node's directory to its child PATH so native addons use that runtime. No Node proxy is created. Runtimes share only matching signed receipts and get a fresh receipt check before apply. Several runtime versions can coexist. Installation returns runtime records and launcher backup completion handles for D to publish and complete. Shared-file removal reads C again, checks its expected hashes and keeps directories or runtimes referenced by other records or a launcher that still exists.

The PATH host requires a chosen edit and live consent. Tests inject Windows PATH and redirect Unix startup files into temporary workspaces. Shell-file replacement keeps its sibling backup. The current PATH result ABI cannot return its completion handle. F must keep shell-file PATH publication out of completed composition until the proposed result fields are integrated. No backup is completed before record publication.

Exact npm plans without a local package entry and latest npm resolution without any runnable metadata client are blocked with an explanation on the committed ABI. They aren't complete implementations of those paths. Attestation-only assets are blocked honestly. The initial entries use SHA-256 pins.

### Runtime review

The private catalogue provides Node `24.22.3` and `22.23.5`, each for Windows, macOS and glibc Linux on x64 and ARM64. It has 12 archive pins in `ToolNodeRuntimeCatalogue.cs`. Runtime minimum and ceiling grammar is separate from CLI selector grammar. Selection checks stable versions, OS and CPU plus the declared OS, kernel, glibc and C++ library floors.

The checksum sources are [Node 24.22.3 signed SHASUMS](https://nodejs.org/dist/v24.22.3/SHASUMS256.txt.asc) and [Node 22.23.5 signed SHASUMS](https://nodejs.org/dist/v22.23.5/SHASUMS256.txt.asc). Both signatures name release-key fingerprint `5BE8A3F6C8A5C01D106C0AD820B1A390B168D356`. GPG returned `GOODSIG` and `VALIDSIG` for both alias downloads, signed on 11 November 2026. The immutable version downloads have the same SHA-256 hashes as those verified files. The public keyring came from the [Node release keys](https://github.com/nodejs/release-keys). The signer fingerprint also matches that repository's release-key list. GPG's undefined personal trust warning isn't a personal trust certification.

Review artifacts are under `artifacts/tool-setup-lane-a/node-review/`:

| File | SHA-256 |
| --- | --- |
| `24-latest.asc` and `24.22.3.asc` | `aaaff9ec5c8c5f0997786c06088134ddb4b21a41f74e872b30ed1acca309e2b5` |
| `22-latest.asc` and `22.23.5.asc` | `fa48c3ebf4d3246b12090c0a5e0de342b65c7a035737d5fc7d24d6af8c60b884` |
| `pubring.kbx` | `610b8d249da3d5733f5a128def2dd0294dbbf5b5713e6ca2529db8db419dee00` |

The successful status logs are `24-latest.verify.txt` and `22-latest.verify.txt`. The command uses Git's `gpg.exe`, `--batch --no-auto-key-retrieve`, an absolute owned `--homedir`, an absolute `--keyring`, `--no-default-keyring`, `--status-fd 1` and `--verify <signed-file>`. An initial relative-keyring attempt created an empty 32-byte keyring beside the worktree. Its exact path and creation details were checked and that new empty file was removed. A later redundant verification with Windows backslash paths looped on a GPG lock error. Only that owned verifier was stopped. The version files were instead matched by SHA-256 to the successful signed review. No real per-user GPG store was used.

E's read-only `upstream-recipes.md` supplied the bundled Node `24.16.0` facts. CodeGraph's bundle uses the declared runtime, flags and JavaScript entry. Root keeps a nonempty inherited `CODEGRAPH_HOST_PPID` and sets its own process ID only when the declared bundle needs one and none was provided. Detection uses E's catalogue environment, including `CODEGRAPH_NO_DOWNLOAD=1`. The Windows MCP `cmd /c` case stays a native journey requirement. No upstream integration was rewritten.

### Earlier checks

READY_FOR_REVIEW applies to this checked implementation candidate. Full lane acceptance still needs the three coordinated ABI changes above. All changes are uncommitted. The coordinator handles the grouped review and commit continuation.

The managed Release solution build passed with zero warnings and zero errors. The exact invocation is `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1`. Its receipt is `artifacts/tool-setup-lane-a/current/build.log`. The NuGet audit override is invocation-only. Both `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` passed. Their logs are `check-whitespace.log` and `check-style.log` beside the build log. Development failures included a missing test namespace import and two xUnit analyzer findings. They were fixed before the successful gate.

The 19 independently selected classes passed 162 distinct tests. The runs have zero failures or skips. Each command uses `--fail-warns on` and `--fail-skips on` and exits zero. The measured counts are:

| Tier | Class | Minimum | Passed | Failed | Skipped |
| --- | --- | ---: | ---: | ---: | ---: |
| unit | `ToolVersionServiceTests` | 34 | 34 | 0 | 0 |
| unit | `ToolRuntimeRangeTests` | 16 | 16 | 0 | 0 |
| unit | `ToolPackageManagerPolicyTests` | 16 | 16 | 0 | 0 |
| unit | `ToolArtifactIntegrityTests` | 7 | 7 | 0 | 0 |
| unit | `ToolTargetPolicyTests` | 8 | 8 | 0 | 0 |
| unit | `ToolLauncherRenderingTests` | 1 | 1 | 0 | 0 |
| unit | `ToolNodeRuntimeCatalogueTests` | 2 | 2 | 0 | 0 |
| integration | `ToolLauncherWriterTests` | 2 | 2 | 0 | 0 |
| integration | `ToolArchiveExtractorTests` | 15 | 15 | 0 | 0 |
| integration | `ToolPrivateNodeInstallerTests` | 3 | 3 | 0 | 0 |
| integration | `ToolProcessHostTests` | 5 | 5 | 0 | 0 |
| integration | `ToolHttpTransferTests` | 5 | 5 | 0 | 0 |
| integration | `ToolPathHostTests` | 7 | 7 | 0 | 0 |
| integration | `ToolCommandResolverTests` | 4 | 4 | 0 | 0 |
| integration | `ToolTargetObserverTests` | 8 | 8 | 0 | 0 |
| integration | `ToolChannelServiceTests` | 12 | 12 | 0 | 0 |
| integration | `ToolNpmChannelTests` | 10 | 10 | 0 | 0 |
| integration | `ToolManagedFilesRemovalTests` | 4 | 4 | 0 | 0 |
| integration | `ToolPackageManagerDetectorTests` | 3 | 3 | 0 | 0 |

Each class has `command.txt`, `run.log` and `results.json` in `artifacts/tool-setup-lane-a/current/<Class>/`. The combined machine-readable receipt is `test-summary.json`. The first 19-class run's assembly hashes are in `first-assembly-hashes.json`. After replacing two test literals with shared constants, the solution build passed again and only the affected `ToolNpmChannelTests` class was repeated. Its 10 tests passed. The final Linux library correction reads the default `libstdc++.so.6` alias and keeps an unknown floor when it can't identify that library. It ignores unused newer files. An owned-file regression was added, the managed solution build passed again and only `ToolTargetObserverTests` was repeated. All eight tests passed. An overlapping edit and build had produced a stale Root reference before that successful build. Final assembly hashes are in `assembly-hashes.json`. The exact commands are:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions.ToolVersionServiceTests --minimum-expected-tests 34 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolVersionServiceTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions.ToolRuntimeRangeTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolRuntimeRangeTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.PackageManagers.ToolPackageManagerPolicyTests --minimum-expected-tests 16 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolPackageManagerPolicyTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply.ToolArtifactIntegrityTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolArtifactIntegrityTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply.ToolTargetPolicyTests --minimum-expected-tests 8 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolTargetPolicyTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Launchers.ToolLauncherRenderingTests --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolLauncherRenderingTests

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Runtimes.ToolNodeRuntimeCatalogueTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolNodeRuntimeCatalogueTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Launchers.ToolLauncherWriterTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolLauncherWriterTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolArchiveExtractorTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolArchiveExtractorTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Runtimes.ToolPrivateNodeInstallerTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolPrivateNodeInstallerTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolProcessHostTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolProcessHostTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolHttpTransferTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolHttpTransferTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Path.ToolPathHostTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolPathHostTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolCommandResolverTests --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolCommandResolverTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolTargetObserverTests --minimum-expected-tests 8 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolTargetObserverTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolChannelServiceTests --minimum-expected-tests 12 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolChannelServiceTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolNpmChannelTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolNpmChannelTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolManagedFilesRemovalTests --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolManagedFilesRemovalTests

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.PackageManagers.ToolPackageManagerDetectorTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/current/ToolPackageManagerDetectorTests
```

The current diff has 30 production files, 19 test classes, one shared test fixture and this packet. The complete path list is `artifacts/tool-setup-lane-a/current/changed-files.txt`. Framework changes cover the assigned Versions, PackageManagers, Supply, Runtimes, Launchers and Path directories. Root changes cover `Composition/Shared/Tools/`. Tests use matching paths in the Unit and Integration projects. No shared Models, schema, record codec, central registration, common catalogue or other packet was edited. The new runtime catalogue stays within A's assigned Runtimes directory.

No real tool installation, persistent user PATH change, registry edit or real shell profile edit ran. All test files, homes, downloads and processes are synthetic or under owned temporary paths. No dependency, project or public environment switch was added. No Tool workspace state, lease, journal, automatic rollback or recovery bundle was added. No snapshot was regenerated or accepted. No new routed Markdown file needs Index. E's real native and MCP journeys and CI's Unix and Native AOT checks are pending.

### First-run history

The following sections describe the earlier first run. Their 93-test receipt and earlier missing channel service are historical. The current continuation above supersedes those implementation gaps where stated.

The execution base is `2e517348ef19e34afa6297c7efa7d0db5451b45c`. The initial worktree was clean.
`npm ci` passed. It reported 10 dependency audit findings (9 moderate and 1 high). No dependency versions changed.

Applicability: These components handle local developer tool files and account PATH effects. Ordinary failures, malformed archives, cancellation and changed files are in scope. A malicious same-user process is outside the architecture's limit. BCL process, HTTP, cryptography, archive and filesystem APIs cover the independent work. No new dependency or exceptional mechanism is chosen. Unit evidence covers values and policy. Integration evidence uses owned temporary files, injected processes and HTTP responses, and a fake PATH environment. Real tool installation, real persistent PATH and real shell profiles are excluded from this first pass. Full managed and Native AOT gates stay with CI.

### Contract requests

CONTRACT_REQUEST `contract(tool): carry resolved commands and verified channel receipts`.
These are proposed shared changes. This lane hasn't edited the protected models or implemented against these additions:

- Add `ToolResolvedCommand` to Services with an absolute `CommandPath` and a `ToolCommandTarget Invocation`. Add `IToolProcessHost.ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)`. Root resolves real executable and script prefixes, including Windows manager shims. Framework detection consumes the result without starting a shell. Consumers are A's process host and channel detection, and C's read observations.
- Add optional `ToolMachineState RecordedState` to `ToolDetectionRequest` and `ToolDetectionResult`, plus optional observed `ToolPackageManagerFacts PackageManager` to the result. Add optional `ToolDetectionResult Existing` to `ToolChannelResolution`. This carries the existing command, responsible manager and prior placement receipts through planning to apply. Consumers are A's channel service and C/D's detection and writer calls. Without these facts, reuse loses launchers and integrity evidence, and replacement can't check its originally planned installation.
- Add required `IReadOnlyList<ToolNodeRuntimeRecord> Runtimes` to `ToolChannelInstallResult`. A returns verified runtime receipts so D can pass them to C's record writer. The current result returns only tool state and effect descriptions. It can't publish the runtime record returned by private Node preparation.
- Add required `ToolRecordsRequest Locations` and `ToolRecordsResult Records` to `ToolManagedFilesRemoveRequest`. These give A the approved physical home, expected state hashes and complete machine launcher inventory. A can then re-read through C's service and block runtime deletion while another launcher uses it. Consumers are A/C/D and root construction.

Runtime range syntax also needs a shared definition. The independent manager policy currently supports the V1 Node floor (`>=20` or an exact stable lower bound prefixed by `>=`) and rejects other expressions. It doesn't implement npm range syntax for runtime requirements.

### Remaining work

The channel service isn't implemented in this candidate. Command resolution and detection, npm-family probes, published global installation, existing-install reuse, latest-only lookup and its disclosure, private npm package installation, shared runtime reuse and shared-file removal need the contract continuation. The download host implements pinned SHA-256 assets. Attestation-only assets are blocked until an accepted verification capability is supplied. No fallback channel, manager or second installation runs after a failure.

`ToolHostFactory` is the construction entry for version, process, download and PATH hosts. Private Node preparation and launcher publication are directly constructible Framework components. PATH publication and launchers call the frozen backup service. Integration uses a test double for that service because B's implementation belongs to another worktree. Full A/B composition still needs the integrator's check.

No real tool installation, real persistent user PATH write, registry edit or real shell profile edit has run. No new environment switch, dependency, project, Tool workspace state, lease or recovery bundle was added. No new routed Markdown file or payload snapshot was added. E's real native and MCP journeys and CI's Unix and Native AOT evidence are pending.

### Checked candidate

READY_FOR_REVIEW applies to the independent components. Full lane acceptance is blocked by the contract requests above. All changes are uncommitted. No push, merge or parent-worktree change ran.

The final Release solution build passed with zero warnings and zero errors. SDK `10.0.101` built the managed Windows x64 candidate. The test host uses .NET `10.0.1`. The exact build command was:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Both `npm run check:dotnet:whitespace` and `npm run check:dotnet:style` passed after the final source edits. The NU1903 override is invocation-only. LF and `git diff --check` pass.

The final focused managed runs passed 93 tests, with zero failures, warnings or skips. Every class ran independently with its actual minimum count:

| Tier        | Class                           | Minimum | Passed | Failed | Skipped |
| ----------- | ------------------------------- | ------: | -----: | -----: | ------: |
| unit        | `ToolVersionServiceTests`       |      34 |     34 |      0 |       0 |
| unit        | `ToolPackageManagerPolicyTests` |      14 |     14 |      0 |       0 |
| unit        | `ToolArtifactIntegrityTests`    |       7 |      7 |      0 |       0 |
| unit        | `ToolLauncherRenderingTests`    |       1 |      1 |      0 |       0 |
| integration | `ToolLauncherWriterTests`       |       2 |      2 |      0 |       0 |
| integration | `ToolArchiveExtractorTests`     |      15 |     15 |      0 |       0 |
| integration | `ToolPrivateNodeInstallerTests` |       3 |      3 |      0 |       0 |
| integration | `ToolProcessHostTests`          |       5 |      5 |      0 |       0 |
| integration | `ToolHttpTransferTests`         |       5 |      5 |      0 |       0 |
| integration | `ToolPathHostTests`             |       7 |      7 |      0 |       0 |

Each class has `command.txt`, `run.log` and `results.json` under `artifacts/tool-setup-lane-a/verified/<Class>/`. The test assembly hashes are in `artifacts/tool-setup-lane-a/verified/assembly-hashes.json`. These are the exact class commands:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions.ToolVersionServiceTests --minimum-expected-tests 34 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolVersionServiceTests
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.PackageManagers.ToolPackageManagerPolicyTests --minimum-expected-tests 14 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolPackageManagerPolicyTests
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply.ToolArtifactIntegrityTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolArtifactIntegrityTests
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Launchers.ToolLauncherRenderingTests --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolLauncherRenderingTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Launchers.ToolLauncherWriterTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolLauncherWriterTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply.ToolArchiveExtractorTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolArchiveExtractorTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Runtimes.ToolPrivateNodeInstallerTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolPrivateNodeInstallerTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolProcessHostTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolProcessHostTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.ToolHttpTransferTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolHttpTransferTests
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Path.ToolPathHostTests --minimum-expected-tests 7 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/tool-setup-lane-a/verified/ToolPathHostTests
```

Checks and source review found fixture cleanup gaps, a missing typed checksum-failure catch, synthetic PowerShell encoding, cmd process quoting, tar stream seekability and PATH receipt accounting. Those issues are fixed. The final runs show no remaining failures or flakes. No snapshot was regenerated or accepted.

### Changed files

The candidate has 15 production files, 10 test files and this packet. The Framework prefix is `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/`. Its changed files are:

- `Versions/ToolVersionService.cs`
- `PackageManagers/ToolPackageManagerPolicy.cs`
- `Supply/ToolArtifactIntegrity.cs`
- `Supply/ToolArtifactSource.cs`
- `Supply/ToolArchiveExtractor.cs`
- `Supply/Models/ToolArchivePlan.cs`
- `Runtimes/ToolNodeArchivePreparation.cs`
- `Runtimes/ToolPrivateNodeInstaller.cs`
- `Launchers/ToolLauncherWriter.cs`
- `Path/ToolPathPolicy.cs`

The Root prefix is `src/cli/root/OpenForge.Cli/Composition/Shared/Tools/`. Its changed files are:

- `ToolHostFactory.cs`
- `ToolProcessHost.cs`
- `ToolHttpTransfer.cs`
- `Path/ToolPathEnvironment.cs`
- `Path/ToolPathHost.cs`

The ten test classes above have matching source paths in their Unit or Integration projects. The test project paths and qualified class names are recorded in the commands. There are no shared-model, schema, codec, central-composition, catalogue or other-packet edits. No new routed file needs Index.

## Divergences

None recorded before implementation.
