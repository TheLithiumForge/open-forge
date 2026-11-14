---
open-forge:
  description: Implement Task 90 lane B additive config and backups
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Additive config and backups

Lane B is the implementation that adds profile configuration and removes only recorded project additions while keeping account-wide settings.
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

You alone edit Framework/Tools/Shared/{ConfigChanges,Backups,Filesystem,NativeSettings}/** below src/cli/framework/OpenForge.Cli.Framework/. You own matching Unit and Integration tests and this lane-b.md packet. You implement IToolConfigService and IToolBackupService. C alone writes lock and machine JSON files. D sequences operations and calls C after each verified addition. A owns process hosts. Shared Models are protected.

## Steps

Follow this ordered implementation plan:

1. Check the exact base and read all Configuration, Records and Services models, Tool record contracts and the full lane-B copy table. Copy the Git-aware backup helpers, JSON Pointer editor and marked-block renderer through git show. Reuse the original-backup continuation fix from c2e7d119d. Strip leases, recovery bundles, roles, guidance, MCP key policies, TOML key rewrites and old registries.
2. Implement safe JSON Pointer edits using the existing byte/span editor. Array union appends missing values and records only added items. A set requires member absence, including when a pre-existing value already equals desired. Null is present. A non-array union conflicts. Keep unrelated bytes and values. Write pure parser/planner tests and real-file integration tests for effects.
3. Implement unique marked blocks and absent-only managed files. Duplicate or malformed markers conflict. Remove only the recorded managed span. Guard file removal by its hash. Reapply is a no-op when matching. Migrate a recorded set, file or block only when current equals recorded applied content or digest. Other drift conflicts. Removed profile recipes use guarded removal.
4. Implement allow-listed native config calls through the request's resolved invocation and IToolProcessHost. Probe stored native values independently of inherited telemetry override environment. Record previous/applied account settings only in returned machine facts. CodeGraph telemetry off is disclosed, including identity/queue deletion. Block before effects if previous state cannot be read.
5. On every project Remove, keep account-wide native settings, return KeptNativeSettings with ManualRestoreArguments and never execute a restore. Drop the project native receipt only as planned cleanup, while keeping machine facts needed for the report. No cross-project registry. Verify with a fake process host that removal invokes no native restore even when current matches applied.
6. Implement IToolBackupService and use the same backup mechanics for native setup targets and config edits. Git covers only tracked bytes matching HEAD. Dirty tracked and untracked files get sibling .bak. Keep one original across sequential edits. Occupied unrelated backup destinations block. Verified completion discards only an unchanged operation backup. Failure keeps it and exposes guarded manual instructions. No automatic rollback or Tool Repair action.
7. Implement ComputeProfileDigest using the fixed projection in the record Interface. It includes profile identity, ranges, minimums, native vectors/effects and recipes/template hashes. Exclude observed values, paths and timestamps. Keep authored array order and ordinal object keys. New templates or token semantics needed across B/D/E are contract requests, not local divergent formats.
8. Build, run touched/created tests and both .NET checks. Return READY_FOR_REVIEW with the service construction entry, all effect and receipt cases, exact tests and any blocked contract needs. The coordinator handles the commit continuation.

## Acceptance

Test fresh apply, no-op, migration, drift and removal for all five change kinds, union accounting, absence/null, unrelated byte preservation, duplicate markers, changed managed files, dropped recipes, unreadable native prior state, kept account-wide settings, failed native calls, backup continuation and partial completion. D can call ApplyAsync with one planned observation at a time and persist receipts before the next. Return completed receipts on later failure. No lock serialization, workspace state, leases, journals or fake filesystem. Synthetic entries and owned temporary homes only.

## Progress

Done: Completed the B implementation, all accepted grouped fixes and the named IgnoreFileName cleanup on `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
Now: Complete. This local B increment contains all 39 assigned files. The final build, 14 affected ignore cases and both mandatory .NET checks passed.
Next: The coordinator integrates the local B commit. F uses the unchanged constructors and shared operation-scoped backup instance described below.
Blocker: None. There is no open CONTRACT_REQUEST.

## Evidence

### Commit continuation

The coordinator accepted E-B1 and E-B2, then authorized this local commit after one named constant cleanup. `ToolEffectiveIgnore.IgnoreFileName` is internal and its test paths and native all-data pattern use that same definition. Synthetic generated and probe names stay unchanged. Reversing only the named constant substitutions reproduced the accepted raw source hashes for both files. There is no behavior, payload, test identity or count change. No sibling Library or CodeGraph path definition was imported.

The final managed Release build on `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` passed with zero warnings and errors. SDK `10.0.101` targeted managed `net10.0` on Windows x64. The local audit override stays on the invocation. Its artifact is `artifacts/lane-b/commit/build.log`:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Only `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolEffectiveIgnoreTests` was rerun. All 14 selected cases executed and passed, with zero failures, skips and runner warnings. The command exited zero. Its artifact is `artifacts/lane-b/commit/ignore.log`:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolEffectiveIgnoreTests --minimum-expected-tests 14 --fail-warns on --fail-skips on --no-ansi --progress off
```

Both required .NET checks exited zero. Their artifacts are `artifacts/lane-b/commit/whitespace.log` and `artifacts/lane-b/commit/style.log`:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The packet passed Markdown formatting. Its artifact is `artifacts/lane-b/commit/markdown.log`:

```powershell
npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-b.md
```

LF, Git whitespace, ownership and packet-section checks passed. The accepted 138, 67 and 69 check receipts below are kept unchanged. All 36 other candidate files match the accepted hashes. The complete B increment contains the same 39 assigned files and uses the local title `feat(tool): implement additive config and backups`. Service constructors and the supplied resource-reader overload are unchanged. There is no open contract request or new routed file. No protected source, snapshot, real account, PATH, registry, parent worktree or remote state was changed.

### Catalogue correction

The coordinator accepted the 67 affected B-R1 through B-R3 checks, then fast-forwarded the idle worktree to `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` while keeping all 39 candidate files. E's complete `config-catalogue-review.md` report was read from its assigned worktree. Its E-B1 and E-B2 findings are handled together in the existing B config and test paths.

E-B1 removes the synthetic child from the coverage decision. Git checks the directory itself and returns its winning source and rule through the documented verbose output. Root directory coverage and the native internal all-data forms still count. Name-specific rules and uncertain partial internal rules don't count. This uses Git's matching and precedence, without a general ignore parser.

E-B2 appends conditional blocks when the existing bytes end at a newline. It prepends when the file has an unterminated final line, so guarded removal can keep those bytes exactly. Git evaluates the proposed bytes in a new owned temporary work tree before project effects. The preview copies only the relevant ignore-file chain and reads the existing repository metadata with `--no-index`. It creates no index, journal or workspace state. An edit that cannot provide coverage returns a preflight conflict. An ineffective recorded block can migrate to effective placement only while its recorded hash matches. Apply verifies actual coverage again before success and keeps an already verified file receipt if that later check fails.

The [Git ignore rules](https://git-scm.com/docs/gitignore#_description) define rule order and directory exclusion. The [Git check-ignore output](https://git-scm.com/docs/git-check-ignore#_output) defines the machine fields used here. Existing managed BCL file APIs provide the owned temporary preview. It is deleted after the read check. There is no exceptional platform mechanism, dependency, constructor or shared-model change. There is no CONTRACT_REQUEST. Tests use real Git and owned files with every Git home redirected. Only directly affected Unit and Integration classes run. Full suites and native journeys stay with integration and CI.

The grouped review uses `b22e1ec393280935dea93a312f2a1aa337834ad5`. The coordinator's `artifacts/tool-setup/dispatch/lane-b-review.md`, updated design, native observation contracts, protected Models and E's committed inherited-override evidence were read before the fixes. The coordinator fast-forwarded this worktree while keeping the lane changes. The accepted 138-test candidate used shared contract `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`. The first run used `2e517348ef19e34afa6297c7efa7d0db5451b45c`.

### Grouped fixes

B-R1 evaluates only the declared native environment keys. Null stays unchecked. A checked map with no effective override returns an empty map. CodeGraph reports only the deciding variable mapped to on/off. It uses tagged precedence, case-insensitive false and exact zero exceptions without trimming. Stored values stay separate. Normal writes keep an empty process overlay and raw environment values never enter native facts.

B-R2 keeps absent source state in the first-install observation. A known documented default becomes a readable native previous value when forming the machine fact. Valid reapply keeps the original previous value. The focused case checks source absence, previous/applied presence and kinds, native home, manual arguments and the unchanged normal write environment.

B-R3 captures a retained failed backup handle as unverified before returning failure. Continuation also keeps its existing sibling path when its verification fails. The synthetic regression changes an owned saved sibling after planning and checks that Apply returns its failed handle, changes neither target nor backup and completes no backup.

The fixes use the existing managed BCL, typed contracts and operation-scoped backup service. There is no exceptional platform mechanism or new dependency. Integration evidence combines the native observations with owned stored files and exercises real sibling backup failures. Only affected or new classes run. Full managed and native gates stay with integration and CI.

### Implementation

The config service handles these effects and receipts:

- JSON union - append missing values and record only added items. Removal removes the recorded occurrence counts and keeps unrelated values. Reapply keeps receipts and migration updates only recorded items.
- JSON set - require member absence, including equal unrecorded values and JSON null. Matching recorded content permits reapply and migration. Drift blocks migration. Removal keeps changed values.
- Marked block - require unique complete markers. Reapply and migration check the recorded span hash. Removal deletes only the recorded unique span, including a changed interior. Duplicate or malformed markers conflict.
- Managed file - require absence without a receipt. Reapply and migration check its recorded hash. Removal keeps changed files.
- Native setting - read the stored prior value before effects, run the declared allow-listed invocation, verify the resulting value and return previous/applied facts for per-user storage. Matching facts and receipts permit a no-op.
- Retired recipe - plan guarded removal before additions. A target or kind migration checks the recorded set value, file hash or block hash before retargeting. A stale removal observation with a cleared or changed receipt fails before effects.
- Partial completion - return complete replacement receipt sets, including untouched and unstarted receipts. Return verified additions and removals before a later failure, unfinished IDs and pending backup handles. Read-only removal plans leave files and native settings unchanged.
- Project Remove - remove project native receipts, keep account-setting facts and manual command arguments, and run no native read or restore. Kept account settings alone don't return a failure.

Byte edits keep unrelated bytes, UTF-8 BOMs and surrounding line endings. File effects use physical containment, expected-state checks and atomic replacement. Existing targets use `File.Replace`. B adds no Tool state folder, lease, journal, automatic rollback or Repair action. B writes no lock, settings or machine JSON file.

Templates use the shared recipe token definitions, `$$` escaping and nonrecursive inserted values. Unknown or malformed tokens fail preflight. JSON keys, pointers, markers and embedded resource names stay literal. Managed resource content uses strict UTF-8. Root provides the per-profile `ToolRecipeContext.NativeHomePath` from `NativeHomePolicy`, its base environment and overrides. B doesn't guess a native home.

CodeGraph uses the bounded declared `~/.codegraph/telemetry.json` source before installation. The declared file reader wins over a command reader. The full tagged schema accepts the declared `machine_id` token kinds and never reads, returns or logs the identity content. Malformed, unreadable or oversized prior state blocks before writes. Generic command reads require an installed invocation and declared bounds, remove inherited telemetry overrides and suppress downloads. Native effects include the declared telemetry, identity and queue deletion disclosure. Prior values and manual arguments appear only in returned machine facts.

Setup plans use declared output inventories and C's `IToolNativePlanReader`. They don't imitate client discovery. Without an installed invocation, planning returns the setup description without executing a missing future binary. Output matching uses declared exact or single-directory wildcard patterns, records expected file hashes or absence and checks physical containment. Report-only outputs aren't read as files or trees. Backup feasibility is checked without creating backups. Effective Git ignore coverage uses Git's matching rules and keeps an existing matching Open Forge block receipt when that block provides coverage.

Backups use raw Git blob equality with HEAD for tracked bytes. Dirty tracked and untracked bytes receive a sibling `.bak`. Repeated preparation through one operation-scoped service keeps the first original, including the continuation behavior from `c2e7d119d`. Occupied unrelated destinations conflict. Verified completion checks the final target and original backup before discarding only an unchanged operation backup. Failure keeps saved bytes and returns guarded manual instructions. Git tests use synthetic HEAD trees and create no commits.

Profile digests use the fixed record-contract field set, including profile identity, ranges, minimums, native vectors and effects, read metadata and bounds, native home policy, output inventories, ignore conditions and recipes with raw template hashes. Object keys use ordinal order. Arrays keep authored order. Tokens are hashed before expansion. Tested versions, machine paths, observed values and timestamps are excluded.

### Composition

F constructs these services from A's process host and C's native plan reader:

```csharp
var backups = new ToolBackupService(processHost);
var config = new ToolConfigService(processHost, backups, nativePlanReader);
```

The signatures are `ToolBackupService(IToolProcessHost processes)` and `ToolConfigService(IToolProcessHost processes, ToolBackupService backups, IToolNativePlanReader nativePlans)`. A fourth argument, `Func<string, byte[]> readTemplate`, is available for a supplied resource reader and synthetic tests. The default reads literal embedded resource names.

`ToolConfigService` implements both `IToolConfigService` and `IToolSetupService`. Its concrete backup argument allows B's read-only feasibility check without changing the protected interface. Share this same operation-scoped backup instance with D through `IToolBackupService`.

D can apply one planned removal or addition at a time, publish each returned complete receipt set through C, then replan before the next effect. D collects pending backup handles, keeps the latest expected target hash for each original backup and calls `CompleteAsync` only after all effects and final records verify. Failure completion uses `Verified = false`. B never completes backups during Apply or Remove.

### Contract status

The first-run B1, B2 and B3 requests are closed by the integrated shared contract. No protected shared type was changed in this lane. The later suggestion for a backup observation method on `IToolBackupService` was withdrawn. Feasibility stays inside B's concrete service. There is no open CONTRACT_REQUEST.

### Changed files

Production files are under `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/`:

- `Backups/Models/ToolPreparedBackup.cs`
- `Backups/ToolBackupPaths.cs`
- `Backups/ToolBackupService.cs`
- `Backups/ToolGitRecoveryReader.cs`
- `ConfigChanges/Models/ToolConfigEdit.cs`
- `ConfigChanges/Models/ToolEntrySpan.cs`
- `ConfigChanges/ToolConfigBytes.cs`
- `ConfigChanges/ToolConfigEditor.cs`
- `ConfigChanges/ToolConfigService.Apply.cs`
- `ConfigChanges/ToolConfigService.Removal.cs`
- `ConfigChanges/ToolConfigService.cs`
- `ConfigChanges/ToolEffectiveIgnore.cs`
- `ConfigChanges/ToolJsonEntries.cs`
- `ConfigChanges/ToolMarkedBlocks.cs`
- `ConfigChanges/ToolNativeOutputs.cs`
- `ConfigChanges/ToolProfileDigest.cs`
- `ConfigChanges/ToolRecipeExpansion.cs`
- `ConfigChanges/ToolSetupPlanner.cs`
- `Filesystem/ToolFileReader.cs`
- `Filesystem/ToolFileReplacement.cs`
- `Filesystem/ToolGitEnvironment.cs`
- `Filesystem/ToolNativePaths.cs`
- `NativeSettings/ToolNativeSettings.cs`
- `NativeSettings/ToolNativeValueParser.cs`

Unit files are under `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Tools/Shared/`:

- `ConfigChanges/ToolConfigEditorTests.cs`
- `ConfigChanges/ToolProfileDigestTests.cs`
- `ConfigChanges/ToolRecipeExpansionTests.cs`
- `NativeSettings/ToolNativeValueParserTests.cs`

Integration files are under `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/`:

- `Backups/ToolBackupServiceTests.cs`
- `ConfigChanges/ToolConfigFixture.cs`
- `ConfigChanges/ToolConfigPublicationTests.cs`
- `ConfigChanges/ToolConfigServiceTests.cs`
- `ConfigChanges/ToolEffectiveIgnoreTests.cs`
- `ConfigChanges/ToolNativeEnvironmentTests.cs`
- `ConfigChanges/ToolNativeSettingsTests.cs`
- `ConfigChanges/ToolServiceScenario.cs`
- `ConfigChanges/ToolSetupPlannerTests.cs`
- `Filesystem/ToolGitTestHost.cs`

This packet is the only changed Markdown file. The candidate has 38 C# files and this packet. There are no new routed documents to Index. Protected Models, schemas, actual workspace records, central composition, parent state and other packets are unchanged. No dependency or project was added. No rendered output changed and no Imprint snapshot was generated or accepted. Payload-driven snapshot regeneration stays with the integrator after E's final pass.

### Catalogue checks

The E-B1 and E-B2 correction uses base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` in this worktree. The target is managed `net10.0` on Windows x64 with SDK `10.0.101`. The NU1903 override stays on the invocation. The earlier successful `npm ci` still applies because dependency manifests are unchanged.

The final Release solution build passed with zero warnings and errors. Its artifact is `artifacts/lane-b/catalogue-fit/build.log`:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Four directly affected classes selected and executed 69 tests. Each command exited zero with zero failures, skips and runner warnings:

| Class                        | Selected and executed | Artifact                                         |
| ---------------------------- | --------------------- | ------------------------------------------------ |
| `ToolEffectiveIgnoreTests`   | 14                    | `artifacts/lane-b/catalogue-fit/ignore.log`      |
| `ToolConfigPublicationTests` | 10                    | `artifacts/lane-b/catalogue-fit/publication.log` |
| `ToolConfigServiceTests`     | 11                    | `artifacts/lane-b/catalogue-fit/config.log`      |
| `ToolConfigEditorTests`      | 34                    | `artifacts/lane-b/catalogue-fit/editor.log`      |

These are the exact commands:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolEffectiveIgnoreTests --minimum-expected-tests 14 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigPublicationTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigServiceTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges.ToolConfigEditorTests --minimum-expected-tests 34 --fail-warns on --fail-skips on --no-ansi --progress off
```

Both mandatory .NET checks passed. Their artifacts are `artifacts/lane-b/catalogue-fit/whitespace.log` and `artifacts/lane-b/catalogue-fit/style.log`:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The packet passed Markdown formatting. Its artifact is `artifacts/lane-b/catalogue-fit/markdown.log`:

```powershell
npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-b.md
```

`git diff --check`, LF, ownership and packet-section checks passed across the same 39 candidate files. E-B1 and E-B2 changed five existing config implementation files, two existing Integration files and this packet. No new routed file needs Index. Constructors and protected shared Models are unchanged. All previous accepted receipts stay below. No unrelated class, full suite, native journey or final payload snapshot ran.

The first build found a missing test namespace import. The next build found use of `.Span` instead of the existing immutable byte array's `.AsSpan()` method. Both were corrected before the final successful build. Their artifacts are `artifacts/lane-b/catalogue-fit/failed-build.log` and `artifacts/lane-b/catalogue-fit/failed-build-span.log`. The first focused Git run found a missing owned fixture directory and the local Git requirement that NUL output needs stdin. The fixture now creates its directory and the implementation reads Git's documented verbose records through the existing host. Those failed artifacts are `artifacts/lane-b/catalogue-fit/failed-ignore.log` and `artifacts/lane-b/catalogue-fit/failed-publication.log`. No flakes were seen.

Both findings are fixed with no remaining contract request. A later negation in an unterminated file conflicts when the byte-preserving supported placement cannot cover the data. That case keeps every original byte and receipt. Other supported conditional edits and recorded migrations verify with real Git, then removal keeps unrelated bytes. The candidate is READY_FOR_REVIEW and stays uncommitted for the coordinator's commit continuation.

### Grouped checks

The grouped fixes were checked in this worktree on `b22e1ec393280935dea93a312f2a1aa337834ad5`. The runtime target is managed `net10.0` on Windows x64 with SDK `10.0.101`. The local NU1903 override stays on the invocation. No dependency manifest changed since the successful `npm ci`.

The managed Release solution build passed with zero warnings and errors. Its artifact is `artifacts/lane-b/review/build.log`:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Five affected or new Integration classes selected and executed 67 tests. Each command exited zero with zero failures, skips and runner warnings:

| Class                        | Selected and executed | Artifact                                         |
| ---------------------------- | --------------------- | ------------------------------------------------ |
| `ToolNativeEnvironmentTests` | 25                    | `artifacts/lane-b/review/native-environment.log` |
| `ToolNativeSettingsTests`    | 11                    | `artifacts/lane-b/review/native-settings.log`    |
| `ToolConfigPublicationTests` | 10                    | `artifacts/lane-b/review/publication.log`        |
| `ToolBackupServiceTests`     | 10                    | `artifacts/lane-b/review/backups.log`            |
| `ToolConfigServiceTests`     | 11                    | `artifacts/lane-b/review/config.log`             |

These are the exact focused commands:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolNativeEnvironmentTests --minimum-expected-tests 25 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolNativeSettingsTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigPublicationTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups.ToolBackupServiceTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigServiceTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
```

Both required .NET checks passed. Their artifacts are `artifacts/lane-b/review/whitespace.log` and `artifacts/lane-b/review/style.log`:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The packet passed Markdown formatting. Its artifact is `artifacts/lane-b/review/markdown.log`:

```powershell
npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-b.md
```

`git diff --check`, LF and lane ownership checks passed across all 39 candidate files. Only this packet's Progress and Evidence sections changed. The grouped fixes changed four existing production files and three existing test files, then added `ConfigChanges/ToolNativeEnvironmentTests.cs`. Constructors and protected shared types stayed unchanged. The accepted 138-test receipt below stays as the earlier candidate evidence. Unaffected classes weren't rerun. No rendered output or payload snapshots changed.

B-R1, B-R2 and B-R3 have no remaining gap. F uses the unchanged constructors in Composition and supplies the declared raw native environment map through `ToolRecipeContext.NativeEnvironment`. Reports use only `ToolConfigObservation.NativeEnvironmentOverrides`. The candidate stays uncommitted for the coordinator's grouped review and commit continuation.

### Accepted checks

The following receipt describes the accepted 138-test candidate on `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`. The grouped check receipt above covers the current fixes.

`npm ci` passed in this worktree during the first run. The contract continuation changed no dependency manifest. The toolchain is SDK `10.0.101`, managed `net10.0` on Windows x64. The NU1903 override is invocation-only.

The final managed Release build passed with zero warnings and errors. Its log is `artifacts/lane-b/continuation/build.log`:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The final focused commands selected and executed 138 tests in ten touched or created classes. Each command exited zero with zero failures, skips and runner warnings:

| Class                        | Selected and executed | Artifact                                            |
| ---------------------------- | --------------------- | --------------------------------------------------- |
| `ToolConfigEditorTests`      | 34                    | `artifacts/lane-b/continuation/editor.log`          |
| `ToolProfileDigestTests`     | 8                     | `artifacts/lane-b/continuation/digest.log`          |
| `ToolRecipeExpansionTests`   | 19                    | `artifacts/lane-b/continuation/recipes.log`         |
| `ToolNativeValueParserTests` | 22                    | `artifacts/lane-b/continuation/native-parser.log`   |
| `ToolConfigServiceTests`     | 11                    | `artifacts/lane-b/continuation/config.log`          |
| `ToolBackupServiceTests`     | 10                    | `artifacts/lane-b/continuation/backups.log`         |
| `ToolConfigPublicationTests` | 9                     | `artifacts/lane-b/continuation/publication.log`     |
| `ToolNativeSettingsTests`    | 11                    | `artifacts/lane-b/continuation/native-settings.log` |
| `ToolSetupPlannerTests`      | 11                    | `artifacts/lane-b/continuation/setups.log`          |
| `ToolEffectiveIgnoreTests`   | 3                     | `artifacts/lane-b/continuation/ignore.log`          |

These are the exact test commands:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges.ToolConfigEditorTests --minimum-expected-tests 34 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges.ToolProfileDigestTests --minimum-expected-tests 8 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.ConfigChanges.ToolRecipeExpansionTests --minimum-expected-tests 19 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.NativeSettings.ToolNativeValueParserTests --minimum-expected-tests 22 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigServiceTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups.ToolBackupServiceTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolConfigPublicationTests --minimum-expected-tests 9 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolNativeSettingsTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolSetupPlannerTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges.ToolEffectiveIgnoreTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off
```

Both required .NET checks exited zero. Their logs are `artifacts/lane-b/continuation/whitespace.log` and `artifacts/lane-b/continuation/style.log`:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

`git diff --check` passed. LF was verified in all 38 changed files. HEAD stays at the supplied shared-contract commit. No full suite, native build, real tool installation, real per-user store access or Doctor command ran. No persistent PATH, registry or real shell profile changed.

### Earlier results

The first-run 61 focused tests and managed build passed on the original execution base. Their logs stay under `artifacts/lane-b/`. The first Integration run found a missing `InvalidDataException` classification and fixture cleanup failures. Both were fixed before that run's final checks. Its failed logs are `failed-integration-config.log` and `failed-integration-backups.log`. The continuation's first build found a test accessibility error, which was corrected before the final build. Its log is `artifacts/lane-b/continuation/failed-build.log`. No flakes were seen.

Automatic approval review rejected cleanup of the older failed-test temporary directories with the reason "blocked by policy". Those directories are kept and their paths are in the failed-run logs. Cleanup wasn't retried or routed through another mechanism. The final tests clean up their newly created fixtures.

### Review status

The B implementation has no open blocked requirement. Central composition, D's complete command sequencing, E's real native journeys and the full CI gate are outside this lane. This checked candidate is READY_FOR_REVIEW and stays uncommitted for the coordinator's grouped review and commit continuation.

## Divergences

None recorded before implementation.
