---
open-forge:
  description: Implement Task 90 lane D install update and remove
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Install update and remove

Lane D is the implementation that plans tool writers, obtains consent and sequences native setup and additive project changes.
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

You alone edit Operations Commands/Tool/{Install,Update,Remove}/**, Commands/Tool/Shared/Writing/** and Commands/Tool/Models/Writing/** below src/cli/operations/OpenForge.Cli.Operations/. You own the matching leaf Rendering and OutputText files, tests and this lane-d.md packet.

A owns channels and privileged hosts. B owns config/backups/profile digest. C owns records and reads. You consume only frozen service interfaces and injected catalogue/home facts. No private copies of their mechanisms. Root Tool group composition, group registration, shared Models and schemas are protected. Each leaf binding accepts its parent Command and keeps dependencies typed. Report exact constructors and presentation APIs for integration.

## Steps

Follow this ordered implementation plan:

1. Check exact base, all service/model contracts and Install/Update/Remove leaf contracts. Inspect a current writer such as Commands/Library/Attach/LibraryAttachOperation.cs and its binder/planner/presentation, plus Shell's typed binding pipeline. Follow existing input, invalid-result, status, cancellation, detail and source-generated result conventions.
2. Implement Install named/bare selection and id@version. Named bare ID gets newest support. Bare Install restores the exact locked version and channel, otherwise latest within the declaration. Update uses the same named grammar and moves declared tools to newest support when bare. A supplies spec/range interpretation. --channel is valid only with a named spec. No --automatic, --allow-download or value-taking --version.
3. Preflight the entire selected batch in ID order before effects. Resolve versions, compatibility, channel/manager, native declared effects and additive drift, then form one concrete plan. Any blocked tool blocks all effects. Ask about manager choices unless --yes fallback applies. A single usable project-lockfile preference wins first. Never create a second install and disclose account-wide replacement.
4. Render a plan with commands, exact versions where known, paths, scopes, installer writes, additions, telemetry, verification, downloads, backups and PATH choices. --yes accepts that plan and downloads. Without terminal and without --yes return missing consent without effects. JSON never prompts. Dry-run uses native preview when available and otherwise a description. Network only resolves latest, with disclosure.
5. Apply install/update through A, verify detection, back up declared native outputs through B and invoke upstream setup unchanged via the process host. Then plan/apply B's additions one at a time, publishing verified lock and machine receipts through C before continuing. Stop on first effect failure and preserve completed/unstarted work. Don't roll back package-manager effects or invent interrupted receipts. Update rewrites launchers and retains older profiles.
6. Implement Remove with the latest C2/C7 decisions. CodeGraph's local native uninstall can precede project cleanup as declared. Codebase Memory uninstall is account-wide, including its binary and all agent entries. Only a live interactive confirmation that names those effects permits it, and never with --yes. With --yes, no terminal or a declined confirmation, skip that uninstall, print the native command and remove only Open Forge project additions. Intentional retention is not itself incomplete removal.
7. Never restore account-wide native settings on a project Remove. Report kept settings and B's manual native change commands. No cross-project registry. Never run the package-manager uninstall; print it. Ask separately before deleting Open Forge-placed shared tool bytes or unused runtimes, and offer PATH removal after the last tool. --yes/unattended paths never delete shared files or edit PATH. Repeated project cleanup is a no-op and conflicts retain their receipts.
8. Build and test the real operations with injected service fakes only at network/process/interactive boundaries and real owned temporary record/config files where crossing OS behavior. Cover named/bare and pinned restore, preflight-none, --yes, no TTY, dry run, exact channel/version plans, reapply, self-update, first failure, native-first confirmed ordering, CBM skipped uninstall and kept account-wide settings.
9. Run both .NET checks and return READY_FOR_REVIEW with all entry/binding/presentation signatures, exact focused evidence, payload snapshot impact and any contract needs. The coordinator handles the commit continuation.

## Acceptance

Every advertised path executes the accepted sequence and has focused evidence. No placeholder success or NotImplemented path. No Tool run/choose/configure/roles, no own MCP/Skill/guidance edits, no workspace Tool state or lease. Keep upstream behavior and its argument vectors. Own command results rather than parsing output text. Use Imprint snapshots for text and direct semantic assertions for effects and consent boundaries. Never exercise actual account installs or real PATH/profile changes in this pass.

## Progress

Done: Implemented E-J4 deferred account preparation on the verified core branch. The managed build and all 71 focused cases pass. E-J3 was cancelled before edits and stays with E.
Now: READY_FOR_REVIEW UNCOMMITTED on `feature/tool-setup-d-remove-fix`. Build, 71 focused cases and both mandatory formatter commands return exit 0.
Next: The coordinator reviews and applies the E-J4 patch for E's actual project-only Remove rerun. Keep the correction uncommitted until further direction.
Blocker: None identified in this continuation.

## Evidence

### Account removal

The coordinator assigned E-J4 after E's Windows journey showed an account uninstall dry run before its separate account choice. The preview can open HKCU, and its unchosen optional output bindings can also block project cleanup. Only live account confirmation permits concrete account preparation. CodeGraph project uninstall with incidental account telemetry keeps its native-first path. Binding, help, wording, Root, Framework models and services stay protected.

The six earlier async-entry files matched core byte-for-byte through `git show` byte streams. Exact local and core copies, hashes and the duplicate patch are saved under `artifacts/lane-d-remove-baseline/adf2df87aa8a48169f2b776b60dfab5b`. Its `receipt.json` lists every path, length and SHA-256. The normal index and other tracked and untracked work were clean before the scoped restore and after branch creation. No reset, stash or broad cleanup ran. HEAD is core `885b17db3e56ceb6839ba23609e85a4380c75018` on `feature/tool-setup-d-remove-fix`.

The correction handles local developer files and declared native account effects. Git, expected hashes, physical containment and pending sibling backups give the accepted recovery tools. The design keeps the complete uninstall request and supplied independent bindings. It formats only a description before account consent. After live confirmation it passes the original request to the existing setup service for full output, inventory and backup checks, then runs native uninstall before project cleanup. No new Framework contract or exceptional mechanism is needed. Unit evidence covers consent and sequencing. Integration evidence uses real owned temporary files with injected process hosts and never launches a native uninstaller or its dry run. Full Root and native journeys stay with E and integration.

The local plan adds `DeferredNativeRemoval` for the original request. Its description request clears preview arguments, the installed preview invocation and output/effect preparation only for that description. The original declaration and every independent client binding stay in the deferred request. After the live account question, `RefreshAsync` uses that original request. Preparation failures stop project cleanup. Prepared native plans stay in execution data for reporting. Expected hashes, physical containment, backup preparation, native-first order, incremental publication and final backup completion still use their existing services.

The question receives the original binary, all-agent-entry and persistent user PATH effects. `--yes`, JSON, unavailable interaction and a declined account choice never prepare native effects. Project cleanup can complete while keeping those account effects and settings. General plan consent still applies. Native settings, package-manager uninstall instructions and separately chosen shared-file/PATH actions keep their existing policy. No inventory-returned path becomes a client binding.

The six-file baseline receipt is preserved as described above. The correction base tree is `fed4c4f890787e831288334dd4ccb0563839c20d`. All 12 changed C# files are pinned in `artifacts/lane-d-remove-source-manifest.sha256`, SHA-256 `015BAB6F4103EAD84DC07242AE3AAFB5120904DC8A0C8C277CD3C46BF62914C0`. Their hashes stayed unchanged after final compilation.

The final managed build returned exit 0 with zero warnings and errors. Its log is `artifacts/lane-d-remove-build-accepted.log`. The exact command is:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Only the four touched or created classes ran. They selected, discovered and executed 71 cases, with 60 Unit and 11 Integration cases. Every command returned exit 0, with zero failures, skips and warnings. The runtime target is managed .NET 10.0.1, x64, using SDK 10.0.101. Each process set `OPENFORGE_SNAPSHOT_UPDATE=0`. The exact commands are:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteOperationTests --minimum-expected-tests 36 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-remove-final-tests/operations
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteSafetyTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-remove-final-tests/safety
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning.ToolWriteRemovePlanningTests --minimum-expected-tests 9 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-remove-final-tests/removal
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application.ToolWriteDeferredRemovalTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-remove-final-tests/deferred
```

Each report is `artifacts/lane-d-remove-final-tests/<name>/results.json`. Each log is `artifacts/lane-d-remove-final-<name>.log`. The final report hashes are:

| Name       | Passed | Exit | SHA-256                                                            |
| ---------- | -----: | ---: | ------------------------------------------------------------------ |
| operations |     36 |    0 | `F49461CE76232A151E0413864BE7C8C452AE7A56635D311FB144668CBBE96974` |
| safety     |     15 |    0 | `CC6F3DE1108C608722634FB1F16548F19F33F9F4B33B51F2D8326F03BA4851AF` |
| removal    |      9 |    0 | `80879E6A7BA53DD78A875CFB871E9E334BF380CA918EBE23F66361B365ED022B` |
| deferred   |     11 |    0 | `11D38F5F3A7D6244131CC0703F86C388CFAD5D6BBC90EF40C93A2A6266F1B4C4` |

The built Unit artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `A18484EE6265D3D0D08E8C551B011960907A528C5B03FBF13A5A4B090FB7770C`. The Integration artifact is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `324DABADF9BD4F619CDADAC36CF4244D6E1DE3E5B67074A280C514ADD1390C42`.

The initial build needed one test namespace import. The first focused run had four new assertions omit the synthetic invocation prefix. The corrected assertions compare prefix plus the original uninstall arguments. Production argument vectors stayed unchanged. The final focused runs all pass. Those initial artifacts stay under `artifacts/lane-d-remove-build.log` and `artifacts/lane-d-remove-tests/`.

No constructor or leaf binding signature changes. No Framework ABI request, protected model, parent rendering whitespace fix, Inspector, E wording/help or F host change. No snapshot or payload regeneration. Existing synthetic presentation snapshots don't consume this changed planning path. The new routed report is `evidence/remove-account-planning-fix.md` for the coordinator's later Index pass.

The final formatter commands returned exit 0 on the same C# source:

- `npm run check:dotnet:whitespace`, log `artifacts/lane-d-remove-whitespace.log`.
- `npm run check:dotnet:style`, log `artifacts/lane-d-remove-style.log`.

Prettier checking the two changed Markdown files, `git diff --check`, strict UTF-8, LF, BOM and source identity checks also return exit 0. The LF binary patch is `artifacts/lane-d-remove-fix.patch` against core `885b17db3e56ceb6839ba23609e85a4380c75018`. Alternate-index application reproduces all 14 candidate files. The normal index stays empty and no commit or merge runs. E's real project-only Remove rerun is still pending.

### Async entry

C-ROOT-ENTRY starts from clean commit `9044c04c2b758e9414a1992b743923d4180331f3`. The coordinator accepted a request-aware asynchronous typed factory for each writer leaf. C's `command-composition.md` Deferred entry section requires async settings, PATH and target facts after pure binding, without blocking or substitute facts.

The correction covers only the three leaf bindings, their local common binding support, the affected binding class and this packet. It reuses the existing typed request, catalogue, stable version service, result and failure codes. The BCL `ValueTask` and cancellation APIs cover the connection. No new dependency, Framework model or exceptional mechanism is needed. Source changes are reversible through Git. Focused managed Unit evidence is sufficient for this entry connection. Combined Root and native gates stay with integration under the coordinator's limits.

The shared entry policy awaits only factory acquisition under its failure guard. The writer's existing execution and progress handling stay outside that guard. Factory cancellation returns Interrupted. Expected host acquisition errors return Failed with `UnexpectedFailure`. Both keep the request's workspace and preview state and omit exception messages and raw native values. Expected errors use the writer's existing families `IOException`, `UnauthorizedAccessException`, `InvalidOperationException` and `ArgumentException`. Cancellation is checked before the factory and again after acquisition. No writer effect starts when either check fails.

The exact five-parameter signatures for C are:

```text
ToolInstallBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolWriteRequest, CancellationToken, ValueTask<ToolInstallOperation>>, ToolCatalogue, IToolVersionService, CliHelpContent)
ToolUpdateBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolWriteRequest, CancellationToken, ValueTask<ToolUpdateOperation>>, ToolCatalogue, IToolVersionService, CliHelpContent)
ToolRemoveBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolWriteRequest, CancellationToken, ValueTask<ToolRemoveOperation>>, ToolCatalogue, IToolVersionService, CliHelpContent)
```

Each returns `CliRequestBinding<ToolWriteRequest, ToolWriteResult>`. The factory runs only from the `Operation` delegate after the unchanged pure catalogue and channel checks. It receives the same bound request and cancellation token. The entry awaits the typed leaf, then executes it once through a static typed delegate. `CreateSymbols(Command)`, operation constructors, the binder and contextual invalid-result factory stay unchanged. No overload, synchronous wait, generic registry or substitute fact was added.

The 45 accepted R5 and R6 cases stay in place. The 25 new cases cover three leaves receiving the exact request and token, nine cancellation cases before, during and after acquisition, twelve async failures across the four expected error families and one synchronously throwing factory. They use the existing provider fixture and controlled task completion. No real host or per-user store is used. The cancellation cases invoke the entry delegate directly because Shell rejects an already-cancelled token before calling that delegate. The other cases use the typed Shell operation stage.

The correction changes six existing files. These are the three leaf bindings, `Shared/Writing/Binding/ToolWriteBinding.cs`, the owned `ToolWriteBindingTests.cs` and this packet. No other helper, Root code, Framework model, public flag, payload or snapshot changed. This correction must stay uncommitted while the main integration merge is pending.

The first two focused runs passed 69 and 68 of 70 cases. One new execution-count assertion assumed a native command for synthetic Remove with no detected installation, then assumed one configuration planning call for Install and Update. The implementation skips that native removal and replans configuration after installation. The test now checks the single initial channel or removal preflight call. Production sequencing stayed unchanged. Those failure logs are `artifacts/lane-d-entry-binding.log` and `artifacts/lane-d-entry-binding-final.log`. The corrected source was rebuilt before the final focused run.

The final managed solution build returned exit zero with zero warnings and errors. Its log is `artifacts/lane-d-entry-build-accepted.log`. The exact command is:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Only `ToolWriteBindingTests` ran. It selected, discovered and executed 70 managed Unit cases. All 70 passed, with zero failures, skips and warnings and exit zero. The target is .NET 10.0.1, x64, using SDK 10.0.101. The process set `OPENFORGE_SNAPSHOT_UPDATE=0`. The exact command is:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding.ToolWriteBindingTests --minimum-expected-tests 70 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-entry-checked-tests/binding
```

The execution log is `artifacts/lane-d-entry-binding-checked.log`. The report is `artifacts/lane-d-entry-checked-tests/binding/results.json`, SHA-256 `D7E9372D47033FCD1AE339B68F364DA56602FE594D200989B37E5B8CFA444C30`. The artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `8F829915552860A8AFF243782B6F180C43CE82CF448F2AE9D898965DD8D76828`.

The base commit is `9044c04c2b758e9414a1992b743923d4180331f3`, tree `45a1afda04f045c2a2a3c979739cda3f6ebb2dea`. All five changed C# files are pinned in `artifacts/lane-d-entry-source-manifest.sha256`, SHA-256 `D8BC6462FA2CCAB20CAE75E9C51B06DA0444FF3F5A295AAE67DBDF4A7A621A9B`. Their hashes stayed unchanged after final compilation. The six changed files use strict UTF-8, LF and no BOM.

The required checks returned exit zero:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-entry-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-entry-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check`, the six-file scope check, strict UTF-8, LF and BOM checks and the five-source hash check.

The LF patch is `artifacts/lane-d-entry.patch`. It is produced with `git diff --binary --full-index` against the exact base. Patch checking and application use an owned alternate index loaded from that base. The resulting six blobs match the candidate files. The normal index stays empty and HEAD stays at the base. No commit, integration merge, native action or payload regeneration runs in this correction.

### Catalogue binding

The coordinator accepted D-R5 and requested D-R6 on the same uncommitted correction. Named unknown IDs and unadvertised channels need to fail typed binding before Root resolves home or native hosts. Bare Install and Update still need record-based selection during execution.

This correction is local input validation for a developer CLI. It reuses the pure catalogue, stable version service, existing findings and typed Shell binding. Source changes are reversible through Git. The BCL dictionary lookup and ordinal channel comparison meet the requirement without a new dependency or exceptional mechanism. The cheapest decisive evidence is the affected Unit binding class. Combined Root and native evidence stays with integration under the coordinator's focused-check limits.

Each leaf has five behavioral parameters. `ToolCatalogue` passes through the common binding into `ToolWriteRequestBinder.Bind`. Syntax checks run first. A named spec must match a catalogue entry's ID, and its optional channel must be advertised by that entry. Failures return the existing `UnknownTool` or `ChannelInvalid` finding with Invalid status and the requested tool ID. The operation delegate and contextual invalid-result factory keep the R5 behavior. Requests without a spec don't inspect declared records during binding.

The five-parameter signatures at the accepted R6 review were:

```text
ToolInstallBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolInstallOperation>, ToolCatalogue, IToolVersionService, CliHelpContent)
ToolUpdateBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolUpdateOperation>, ToolCatalogue, IToolVersionService, CliHelpContent)
ToolRemoveBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolRemoveOperation>, ToolCatalogue, IToolVersionService, CliHelpContent)
```

Each returns `CliRequestBinding<ToolWriteRequest, ToolWriteResult>`. `CreateSymbols(Command)` and all operation constructors stay unchanged. There is no old overload, Root parser change or substitute host fact.

The combined correction changes seven existing files. These are the five files listed in the R5 evidence and the two production files `Shared/Writing/Binding/ToolWriteBinding.cs` and `ToolWriteRequestBinder.cs`. The seven new Unit cases cover unknown named IDs on all three leaves, unadvertised channels on Install and Update and bare Install and Update executing both declared records. The 38 accepted R5 cases stay in place. No consent, planning, effect, Framework, shared-model or snapshot file changed. No payload regeneration ran. There are no new routed files or contract requests.

The managed solution build returned exit zero with zero warnings and errors in `artifacts/lane-d-r6-build.log`. Its exact command is:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Only the affected binding class ran. It selected, discovered and executed 45 managed Unit cases. All 45 passed, with zero failures, skips and warnings and exit zero. The target is .NET 10.0.1, x64, using SDK 10.0.101. The process set `OPENFORGE_SNAPSHOT_UPDATE=0`. The exact command is:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding.ToolWriteBindingTests --minimum-expected-tests 45 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r6-tests/binding
```

The execution log is `artifacts/lane-d-r6-binding.log`. The report is `artifacts/lane-d-r6-tests/binding/results.json`, SHA-256 `4CA86DFD81AA2C1132E8B97850EE52A886431D75D320457F89EE9D47E2DC0EB7`. The artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `9C9CB874F8A370BDCE436333618C366F3DD4A910C5EC6E0B1125EF00D35EAE98`.

The base commit is `41ef63a7fc8987836e74315ea3450fe8bdd7273c` and its tree is `46378694b23dcddd3814b1288af44f6f9d175bd7`. The six changed C# files are pinned in `artifacts/lane-d-r6-source-manifest.sha256`, SHA-256 `DE02D0B71F445F615864F84EA01A954D150B25B0CE25D0E04FCC26DF9D37FF17`. Those source hashes stayed unchanged after compilation. All seven changed files use LF without a UTF-8 BOM. The coordinator authorized one combined local R5 and R6 correction commit after these gates, without another review return.

The final required checks returned exit zero:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-r6-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-r6-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check`, the seven-file scope check, LF and UTF-8 BOM checks and the six-source hash check.

### Deferred factories

D-R5 starts from implementation commit `41ef63a7fc8987836e74315ea3450fe8bdd7273c`. The starting worktree was clean. Root integration found that passing operation instances into leaf binding creation resolved native-home facts during composition.

Each leaf now takes its typed `Func<ToolLeafOperation>`. The leaf wraps that factory in the existing `Operation` delegate and calls it only when execution reaches that delegate. The stable version service stays in the pure binder. The contextual invalid-result factory stays unchanged. The instance-taking signatures were replaced without an old overload or a Root workaround.

The four-parameter signatures at the accepted R5 review were:

```text
ToolInstallBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolInstallOperation>, IToolVersionService, CliHelpContent)
ToolUpdateBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolUpdateOperation>, IToolVersionService, CliHelpContent)
ToolRemoveBinding.CreateRequestBinding(ToolWriteSymbols, Func<ToolRemoveOperation>, IToolVersionService, CliHelpContent)
```

Each returns `CliRequestBinding<ToolWriteRequest, ToolWriteResult>`. `CreateSymbols(Command)` and all operation constructors stay unchanged.

The correction edits only these five assigned files:

- `Commands/Tool/Install/ToolInstallBinding.cs`
- `Commands/Tool/Update/ToolUpdateBinding.cs`
- `Commands/Tool/Remove/ToolRemoveBinding.cs`
- `Commands/Tool/Shared/Writing/Binding/ToolWriteBindingTests.cs` in the Unit project
- This packet

The source change adds no dependency, service, model, state or effect. Unit evidence uses the existing provider fakes and typed Shell stages. The 24 new cases cover all three leaves during construction, help and unrelated selection, parser and semantic binding errors, missing workspace, contextual invalid results and valid execution. The original 14 input cases stay in the same class. Valid execution checks one factory call, and every non-execution case checks zero calls and no provider calls.

The final managed solution build passed with zero warnings and zero errors. The command returned exit zero and its log is `artifacts/lane-d-r5-build-final.log`:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Only the affected binding class ran. It selected, discovered and executed 38 Unit cases. All 38 passed, with zero failures, skips and warnings and exit zero. The target is managed .NET 10.0.1, x64, using SDK 10.0.101. The exact command is:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding.ToolWriteBindingTests --minimum-expected-tests 38 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r5-tests/binding
```

The test process set `OPENFORGE_SNAPSHOT_UPDATE=0`. The execution log is `artifacts/lane-d-r5-final-binding.log`. The report is `artifacts/lane-d-r5-tests/binding/results.json`, SHA-256 `A003220D0C2DE5FBA4276C272855341BA66F43DB03481636FCB3799E1A8DCDD8`. The built artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `2E90B8D701769674DC701469FF6FBBC92F4A398A55D2E017025567938A2FC9F0`.

The base tree is `46378694b23dcddd3814b1288af44f6f9d175bd7`. The four changed C# files and their hashes are in `artifacts/lane-d-r5-source-manifest.sha256`, SHA-256 `A750A43C77DADF0FAA40F01E51EDBBBB74C6D01B18AA28BB963D37569B47E7A2`. No C# source changed after final compilation. The five changed files use LF without a UTF-8 BOM.

The first focused run passed 37 cases and failed one extra assertion about the shared Remove help validator returning no finding when its required operand is absent. That assertion was outside D-R5's factory contract and was removed. The case still runs help selection, terminal validation and help-content lookup and checks zero factory and provider calls. Shared Shell behavior wasn't changed. The final run passed all 38 cases without flakes.

The required final checks returned exit zero:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-r5-final-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-r5-final-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check`, LF and UTF-8 BOM checks on all five changed files and source-manifest hash verification.

No consent, planning, effect, Framework or shared-model code changed. No snapshots or payload fixtures were changed or regenerated. There are no new files or routed Markdown sources. R5 was left uncommitted for the coordinator's continuation. The implementation commit is `41ef63a7fc8987836e74315ea3450fe8bdd7273c`.

### Prior commit

The coordinator accepted the exact 153-test candidate and authorized its local implementation commit. D-R2 and D-R4 are closed. D-R3 stays closed with four application stages. No extra review or payload update was requested.

Before staging, all 66 source and synthetic snapshot hashes matched the accepted manifest. The two built test artifacts and all nine report hashes matched the accepted receipt. The reports still contain 153 passing cases, with zero failures and skips. The accepted build and both mandatory .NET checks apply to this unchanged implementation. Only this packet's acceptance record changed in the commit continuation.

The local commit includes these 66 files and this packet. Root must give the actual running `OpenForgeVersion` and only the declared raw `NativeEnvironment` variables. Constructor, presentation and binding signatures below stay unchanged. No protected model, payload fixture or other lane file changed.

The review continuation base is `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`. The worktree kept the previous 105-test candidate. The updated design, record contracts, protected models and supplied `lane-d-review.md` were read before dependent edits.

The continuation changes these behaviors:

- D-R2 excludes incidental account telemetry from the account-uninstall choice. Native project uninstall still runs with `--yes` and no terminal. Declared account configuration, shared-file and PATH removal effects still need the live choice. There is no tool-ID branch.
- D-R4 compares the running `ToolWriteHostFacts.OpenForgeVersion` with each chosen profile's minimum before effects. It handles prerelease precedence and ignores build metadata. Invalid version facts block that profile.
- `ToolWriteHostFacts.NativeEnvironment` is required and nullable. Root gives only declared native variables. D passes null, empty and populated maps unchanged into recipe contexts. Reports carry stored native on/off and B's effective on/off observations separately. They never carry the raw host map.
- `ToolRecordGrantRequest` and `IToolRecordService.AddAllowInstallPathsAsync` replace the old grant proposal. D calls the settings-only method before planning, reads fresh expectations and reports the saved edit separately from tool consent. Preview describes the edit without writing. Saved grants and pending backups survive blocked planning, missing consent, cancellation and failures.
- PATH results contribute pending backups before result handling or publication. Verified partial PATH state is published before reporting its failure. Completion still waits for all final effects and record writes.
- D's record fake implements the new method. Synthetic npm entries give `EntryRelativePath`. D has no `IToolDownloadHost` fake or implementation to update for the metadata method. It keeps using A's channel service.

The running-version guard is the bounded implementation needed by the assigned prerelease requirement. The base contains no SemVer comparison capability, and `IToolVersionService` accepts only the stable Tool selector grammar. `System.Version` doesn't represent prereleases. D therefore uses one local comparison guard with the BCL regex source generator and ordinal and numeric identifier comparison. It adds no dependency or shared API. Its Unit cases cover precedence, metadata, large numeric identifiers and rejected malformed facts. Re-evaluate this local guard when A's shared version implementation becomes available. Full managed and Native AOT gates stay with integration under the assigned verification limits.

F keeps every constructor and leaf binding signature listed in the prior candidate below. `ToolWriteApplication` still takes exactly four stage parameters. F also sets these required host facts:

```text
string OpenForgeVersion
IReadOnlyDictionary<string, string>? NativeEnvironment
```

Root supplies the running SemVer string, including prerelease and optional build metadata. It filters raw native variables to the authored declarations before constructing the facts. These are input facts and aren't serialized.

### Review checks

The final managed solution build passed with zero warnings and zero errors. Its log is `artifacts/lane-d-r2-build-final.log`. The command returned exit zero:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

All nine focused class runs passed, with 153 selected, discovered and executed cases. The counts are 150 Unit and three Integration cases. All runs had zero failures, skips and warnings and returned exit zero. The target was managed .NET 10.0.1, x64, using SDK 10.0.101. Each PowerShell test invocation set the process-local `OPENFORGE_SNAPSHOT_UPDATE=0` before its command.

The exact test commands are:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Selection.ToolWriteSelectorTests --minimum-expected-tests 17 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/selection

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Consent.ToolWriteConsentOperationTests --minimum-expected-tests 30 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/consent

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteOperationTests --minimum-expected-tests 36 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/operations

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding.ToolWriteBindingTests --minimum-expected-tests 14 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/binding

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Presentation.Tool.Shared.Writing.ToolWritePresentationTests --minimum-expected-tests 6 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/presentation

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application.ToolWriteNativeApplicationTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/native

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning.ToolWriteMinimumVersionTests --minimum-expected-tests 22 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/minimum

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteGrantTests --minimum-expected-tests 10 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/grants

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteSafetyTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-r2-tests/safety
```

The reports below describe the final class runs on Windows. Each report is `artifacts/lane-d-r2-tests/<name>/results.json`. Each execution log is `artifacts/lane-d-r2-<name>.log`:

| Name         | Passed | Report SHA-256                                                     |
| ------------ | -----: | ------------------------------------------------------------------ |
| selection    |     17 | `96796289A04B2285FC95C80B986BCC04A046FECE6640FB1BE193E67763AA907A` |
| consent      |     30 | `3B81012117063D45D0DB5BAD19D8803D512EBF1E6B93E5F910BCE23531645283` |
| operations   |     36 | `E47C8FC90BFC395F21DEA8BF14DC060E5259D76C84DC347D2FCDCCF268F9AAC1` |
| binding      |     14 | `91C40165357BDE6F3DAA2A37C4B459992BC639E7618F597AB3FC540ACA34A490` |
| presentation |      6 | `A66BA43DB32ECAF79E86105732445CF578819ADE29985D2B3DDFB4589143199A` |
| native       |      3 | `BED9CC4F2250091F1F1EE87B9A4977253B0B5750BC5BE5A9ACA94DADF3744723` |
| minimum      |     22 | `72E635A1727EABA3D8E4A5583F36E5A44168D9360DACCFD9C00E49E58CADE79D` |
| grants       |     10 | `2711C21DF4A76D97AD0B52E92AA8C483363F6AF342B8D5AD4A9C5F4FEC2A2E71` |
| safety       |     15 | `CD57A9951574F458460FC1BBD486A5346D2DA1CA7E632DB8792276FD01822B32` |

The built artifacts are:

- `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `DAC018330CE3A9E578AC6647E19CCE76F980B0839B58A41E444BC1EA5CA36D3D`.
- `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `754E2CB540EB67C119F912A6763818FEC8C90B284CC801F8AD8107399CE61489`.

The base tree is `20c25b59229c7907411ba616e1d696e88e2638e5`. The 66 new source and synthetic snapshot files are listed in `artifacts/lane-d-r2-source-manifest.sha256`. Its SHA-256 is `8049C12C96433A5E78DE9E6F5FFEE52C47EE703DC210A0EBAB5A3D852DE6CC2A`. This packet is the only modified tracked file.

There are nine synthetic Imprint baselines. Three existing JSON captures gained grant and native observation fields. Two new captures show a saved grant and its backup during blocked tool planning, with separate stored and effective native settings. The received text and JSON were read before targeted serial acceptance. All nine baselines match those reviewed captures after LF conversion. Ordinary verification then passed with updates disabled. No payload snapshot or fixture was regenerated.

The 22 minimum-version cases cover prerelease ordering, numeric identifiers, stable precedence, build metadata, large numeric identifiers and malformed facts. The 15 safety cases cover incidental account telemetry, minimum-version blocking for all three leaves, private native input, separate effective native facts and PATH add and remove backup completion and failures. The ten grant cases cover planning before tool effects, blocked and cancelled work, preview, failed publication and reread, missing consent, invalid selection and grant-only completion and backup failure. The existing operation class now checks successful grants instead of the removed ABI guard.

Service fakes use the integrated interfaces because A, B and C's service implementations aren't in this base. These checks establish D's sequence and result handling. C's grant authoring conformance and full provider integration stay with their lanes and grouped integration. The three Integration cases still use real owned temporary native files. No native installer, real account store, persistent PATH, registry or real shell profile was changed. No full suite, Native AOT build or Doctor was run.

The implementation added `ToolWriteMinimumVersion.cs`, `ToolWriteMinimumVersionTests.cs`, `ToolWriteGrantTests.cs` and `ToolWriteSafetyTests.cs`. There are no new routed Markdown files. Shared Models, schemas, composition, other packets and other worktrees weren't edited. No commit, push, merge or GitHub mutation was made. No contract request is pending from D. The candidate awaits coordinator review and commit continuation.

The required final checks returned exit zero:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-r2-final-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-r2-final-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check`, a byte check confirming LF without a UTF-8 BOM in all 67 changed files and a check that every source-manifest hash still matches.

No C# source changed after final compilation. All final class runs passed without flakes. The first presentation run reported only expected synthetic baseline additions and changes. Those captures were reviewed before acceptance. Final ordinary verification passed with updates disabled.

### Prior candidate

The continuation base is `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`. The coordinator fast-forwarded this worktree and kept D's earlier changes. The updated design, tool-records contracts and protected models were read before dependent implementation.
The earlier channel, native setup, config removal, stored-value and backup requests are resolved by that base.

The implementation now includes these paths:

- Named and bare Install and Update select through A's version and channel interfaces. Bare Install pins only the lock version. Only a named user `--channel` becomes an override.
- The whole batch preflights before effects. Manager choices precede plan consent. JSON never prompts. Dry-run uses B's read-only setup and config plans.
- Application passes the same concrete channel resolution to A. Detection verifies its exact version before native setup. Native writes use expected hashes, physical containment and B's backup service.
- Config migration and removal run one observation at a time. Each request carries the full current receipt set. Each verified result replaces that tool's complete set through C before the next effect. Other tools stay unchanged.
- A, B and C backup handles stay pending through final verification and record publication. Completion groups by target, keeps the first original backup and uses the latest expected hash. Failures keep pending backups and completed and unstarted work.
- Account uninstall uses a separate live choice naming the binary and all agent entries. `--yes`, an unavailable terminal or a decline skips that native effect after the project plan is accepted. Project additions are still removed. Kept account settings and their manual commands don't make successful cleanup incomplete.
- Package-manager uninstall is printed. Shared bytes, runtimes and PATH changes need separate live choices. The code never restores account-wide native settings on project removal.

The common writer presentation lives in `Presentation/Tool/Shared/Writing/` and `OutputText/Tool/Shared/Writing/`. These directly required support paths avoid copying one writer report into three leaves. Central registration and shared composition wiring stay with F. E receives final wording after implementation.

The prior candidate proposed this ABI request. The separate grant method above supersedes it:

Proposed addition to protected `Services/ToolRecordModels.cs`, on `ToolRecordWriteRequest`:

```csharp
public IReadOnlyList<string> AllowInstallPaths { get; init; } = [];
```

The proposal asked C to merge these requested grants through its checked settings write. Consumers were C, D and F. No protected type or schema was changed. At that checkpoint, a guard reported `PermissionRequired` before effects for explicit grants. The new settings-only ABI replaces that guard.

F uses these exact leaf APIs, with `Install`, `Update` or `Remove` substituted for `Leaf`:

- `ToolLeafOperation(ToolWriteOperation writer)`
- `ToolLeafBinding.CreateSymbols(Command parent)` returns `ToolWriteSymbols`.
- `ToolLeafBinding.CreateRequestBinding(ToolWriteSymbols symbols, ToolLeafOperation operation, IToolVersionService versions, CliHelpContent help)` returns `CliRequestBinding<ToolWriteRequest, ToolWriteResult>`.
- `ToolLeafPresentation.Rendering` returns `CliReportRendering<ToolWriteResult, ToolWriteData>`.
- `ToolLeafPresentation.CreateHelp()` returns `CliHelpContent`.

The shared writer construction uses these existing constructors:

```text
ToolWriteRecordPublisher(IToolRecordService)
ToolWriteToolPlanner(IToolVersionService, IToolChannelService, IToolConfigService, IToolSetupService)
ToolWritePlanner(ToolCatalogue, ToolWriteToolPlanner)
ToolWriteNativeApplication(IToolProcessHost, IToolBackupService, IToolSetupService)
ToolWriteChannelApplication(IToolChannelService, IToolVersionService, ToolWriteRecordPublisher)
ToolWriteConfigApplication(IToolConfigService, ToolWriteRecordPublisher)
ToolWriteOptionalApplication(IToolChannelService, IToolPathHost, ToolWriteRecordPublisher)
ToolWriteApplication(ToolWriteChannelApplication, ToolWriteNativeApplication, ToolWriteConfigApplication, ToolWriteOptionalApplication)
ToolWriteOperation(ToolWritePlanner, ToolWriteApplication, ToolWriteRecordPublisher, ToolWriteHostFacts, ToolWriteInteraction)
```

F shares B's one backup-service instance with the service providers and D. `ToolWriteHostFacts` gives the Open Forge home, main root, target, resolved native homes by policy, client roots, project manager preferences and initial PATH facts. Root resolves native homes from its injected environment and keeps normal child environment unchanged.

`ToolWriteInteraction` has `CanPrompt`, typed `ConfirmPlan`, `ConfirmOptional`, `ChooseManager` and `ChoosePath` delegates. F renders the supplied preview through the existing minimal review before confirmation. Use `SelectAsync` with the Boolean optional question, so a No answer is an answered decline and project cleanup continues.

`ToolWriteInteractionPresentation` returns these types:

```text
CliConfirmQuestion PlanQuestion()
CliSelectQuestion<bool> OptionalQuestion(ToolWriteConsentQuestion)
CliSelectQuestion<int> ManagerQuestion(ToolWriteManagerQuestion)
ValueTask<CliPromptReply<ToolWritePathAnswer>> ChoosePathAsync(CliPrompts, ToolWritePathQuestion, CliPromptPolicy, CancellationToken)
```

Verification uses prescribed service returns for D's Unit orchestration while A, B and C implement their services in separate lanes. The Integration class crosses real owned temporary files for native expected-state and backup ordering. It executes no native installer, real user store, persistent PATH or real shell profile. Real provider integration and native journeys follow their merge through F and E.

### Prior checks

The solution build passed with zero warnings and zero errors. The command returned exit zero. The log is `artifacts/lane-d-build-final.log`:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The final focused run passed all 105 cases, with 102 Unit and three Integration cases. Each class selected, discovered and executed its expected count. All six commands returned exit zero, with zero failures, skips or warnings. The SDK is `10.0.101` and the runtime is `.NET 10.0.1`, x64.

The exact class commands are:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Selection.ToolWriteSelectorTests --minimum-expected-tests 17 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/selection

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Consent.ToolWriteConsentOperationTests --minimum-expected-tests 30 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/consent

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.ToolWriteOperationTests --minimum-expected-tests 36 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/operations

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding.ToolWriteBindingTests --minimum-expected-tests 14 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/binding

dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Presentation.Tool.Shared.Writing.ToolWritePresentationTests --minimum-expected-tests 5 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/presentation

dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application.ToolWriteNativeApplicationTests --minimum-expected-tests 3 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-final-tests/native
```

The reports below measure these six final class runs on Windows. Each report is `artifacts/lane-d-final-tests/<name>/results.json`. Each execution log is `artifacts/lane-d-final-<name>.log`:

| Name         | Passed | Report SHA-256                                                     |
| ------------ | -----: | ------------------------------------------------------------------ |
| selection    |     17 | `67E64C2F56C56749BC748FE6BFAC5D643D8475A21B0715C70AC2FD85908AD4C1` |
| consent      |     30 | `DA79277D232433AC94C5F9AC3B624BC6BD65FD2A1523FA2ECF58F4A1EC263DD6` |
| operations   |     36 | `897A4B2D1A7A9D3EDE5B05FFA154920A0D4041D2183C39157FE8153D8CCA7900` |
| binding      |     14 | `C5BE0C35E6160CE983ACE59EFAB5B11A13F73F9D508B328B8A1E289E6EC5B48E` |
| presentation |      5 | `21BBF5CFC526C1B1ACF0C71B93B5EE2800B6D0BEE942AECC95281A137E1D7E43` |
| native       |      3 | `C038529CDB826400A2F601B1661FF613C66A096A2A8C384731E4A945DE4A6B10` |

The final built test artifacts are:

- `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`, SHA-256 `9F443B34C5A411FCB2B031E9C9A48B89F78F6AB05F60DD8141225BFD0F0F29B9`.
- `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`, SHA-256 `4912ABD25B5B3EF20DA9C117695C428BCB0E02696F2DCB5D44E84FE936D54C27`.

The base tree is `8e34fb2ac5ddebd4711dc0173806a5cd31639343`. The 60 new source and synthetic snapshot files are listed with individual hashes in `artifacts/lane-d-source-manifest.sha256`. Its SHA-256 is `076AA00115EE394FD43DBC28EC8062F5AFB26FDC8CD6056F0E5D5D7327FCA09C`. The existing packet is the only modified tracked file.

Seven new synthetic Imprint baselines cover Install, Update and Remove text and JSON plus the account uninstall question. The captures were reviewed before acceptance. A shared test identity initially mixed the three report cases. The cases now have separate identities and pass ordinary verification with snapshot updates disabled. The snapshot files match the reviewed captures after LF conversion. No payload snapshot or fixture was regenerated. Final help and payload-driven captures stay with E and the integrator after wording transfer.

The 36 operation cases cover exact lock restore without a channel override, named latest support, stable minor and exact specs, concrete channel plans, manager choices, whole-batch blocking, unattended consent, preview, reapply, self-update, staged complete receipt sets, unrelated records, backup completion, first failure, record write failure, native-first removal, account uninstall skipped and confirmed, kept account settings, cleanup conflicts, repeated cleanup and separately chosen PATH effects. The explicit grant case checks the current blocked path. It doesn't claim grants are implemented.

The three Integration cases use owned temporary files to check expected hashes before the process, native verification and deferred backup completion, plus stale-file and failed-process handling. Broader provider conformance, all-platform and native journeys aren't covered by these class runs. No full suite, Native AOT build, Doctor, real installation or real user store was used.

The required final checks returned exit zero:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-final-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-final-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check` and a byte check of all 61 changed files. They use LF without a UTF-8 BOM.

No flakes were seen in the final class runs. During implementation, the binding tests caught a repeated-option read before invalid parse handling. The binder now rejects that invalid parse before reading values, and all 14 binding cases pass.

No new routed Markdown files were created. The integrator can Index once after grouped integration. No commit, push or GitHub change was made.

### First run

The receipt below describes the initial run before the coordinated contract base.

The execution base is `2e517348ef19e34afa6297c7efa7d0db5451b45c`. The initial worktree was clean.
`npm ci` passed. It reported 10 dependency vulnerabilities, with nine moderate and one high.

Applicability check:

- Tool writers affect developer project files and account setup. Git, sibling backups, expected-state checks and manual repair cover normal failures. Tests won't run native installation or change real account settings.
- The BCL, existing Shell prompt contracts and frozen Tool models cover independent selection and consent. No new dependency or exceptional mechanism is needed.
- Unit evidence exercises typed selection and interactive consent. Shared services provide planning and effect mechanics. Integration evidence waits for those implementations and the shared planning contract.
- Full suites and Native AOT stay with CI under the assigned verification limits.

Independent implementation:

- `ToolWriteSelector(ToolCatalogue).Select(ToolWriteSelectionRequest)` selects declarations in ordinal ID order. Bare Install keeps the exact lock version and channel. A declaration without a lock keeps its minor range. Named specs keep A's parsed selection. Bare Update requests latest support. Unknown IDs, unknown overrides and unrecorded named Updates return typed issues with no selected batch.
- `ToolWriteConsentOperation(CliPrompt<ToolWriteConsentQuestion, bool>).AskAsync(ToolWriteConsentRequest, CancellationToken)` handles plan consent and separate account uninstall, shared file deletion and PATH consent. JSON and previews never prompt. `--yes` accepts only the plan. Optional effects require a live text prompt. A declined optional effect doesn't decline project cleanup.
- The independent code uses frozen Framework values and Shell prompt contracts. Shared service mechanisms stay with A, B and C. Command registration and effect execution wait for the planning contracts below.

Proposed contract increments:

- `contract(tool): plan config removal before effects` - Add `ToolConfigRemovePlan` and `ToolConfigRemoveObservation` in `Services/ToolConfigModels.cs`. The plan carries the remove request and ordered observations. Each observation carries the original addition, current value, disposition and conflict. Add read-only `IToolConfigService.PlanRemoveAsync(ToolConfigRemoveRequest, CancellationToken)`. Change `RemoveAsync` to take that plan and revalidate its expected observations before effects. Keep native settings as kept facts with manual commands. Consumers are B and D. Alternatively, the integrator can define and test that `PlanAsync` with an empty desired recipe is the complete removal plan and `ApplyAsync` applies it. The current frozen contract doesn't state that behavior.
- `contract(tool): expose concrete channel execution plans` - Add `HomePath` to `ToolChannelRequest`. Extend `ToolChannelResolution` with the planned `ToolCommandTarget`, ordered process argument vectors and resolved destination paths. `InstallAsync` must execute that same checked plan. This lets D show exact commands, native setup invocation, paths and backups without reproducing A's package-manager, runtime and launcher rules. Consumers are A, D and root composition.

One dependency needs a shared decision before implementation. `ToolNativeConfigChange` reads previous account settings through the tool invocation. On a first install, that invocation doesn't exist before channel effects. The design requires the previous value and whole-batch preflight before effects. B needs a supported observation path that works before installation, or the maintainer needs to decide the first-install sequence. D hasn't added a private native-setting reader or moved that check after effects.

These requests are unimplemented. Install, Update and Remove operations, bindings, presentation, effect sequencing and Integration tests are blocked on the shared planning resolution. The `id@version` parser belongs to A and hasn't been duplicated. There are no leaf entry, binding or presentation APIs to integrate yet.

Managed verification uses SDK `10.0.101` and runtime `.NET 10.0.1`, x64. The base tree is `6cad9a2b981a409165f58e1987cda34b2e1fffe5`. The uncommitted source changes are the six new Operations files, two new Unit files and this packet.

The solution build passed with zero warnings and zero errors. Its log is `artifacts/lane-d-build.log`:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The built test artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`. Its SHA-256 is `D7ACEC8D91DA6B6895FD14138E730F477208213F9BA60192F94BB13421EBC408`.

Selection evidence selected, discovered and executed 17 cases. All 17 passed, with zero failures, skips or warnings. The report is `artifacts/lane-d-tests/selection/results.json`, SHA-256 `0754DCFC741E7AF38C67FF82186AE447E3A461833A9C0F6AA77559FE4D3FA912`. The execution log is `artifacts/lane-d-selection-tests.log`:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Selection.ToolWriteSelectorTests --minimum-expected-tests 17 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-tests/selection
```

Consent evidence selected, discovered and executed 30 cases. All 30 passed, with zero failures, skips or warnings. The report is `artifacts/lane-d-tests/consent/results.json`, SHA-256 `CE9BE2A4342EE47C13FE5E1E7EC04E8F8744FA88B2A6997A6C9578388E0F61AF`. The execution log is `artifacts/lane-d-consent-tests.log`:

```text
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Consent.ToolWriteConsentOperationTests --minimum-expected-tests 30 --fail-warns on --fail-skips on --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory artifacts/lane-d-tests/consent
```

The following checks also passed:

- `npm run check:dotnet:whitespace`, with log `artifacts/lane-d-whitespace.log`.
- `npm run check:dotnet:style`, with log `artifacts/lane-d-style.log`.
- `npx --no-install prettier --check .agents/memory/working/cli-development/tool-setup/packets/lane-d.md`.
- `git diff --check` and a byte check confirming LF for all nine changed or new files.

All verification commands above returned exit zero. No flakes were seen.
No snapshots were added or regenerated. The independent code changes no shipped payload or rendered output. Payload snapshot work stays with the integrator.
No new routed Markdown files were created. Only this existing packet changed. No commit was made.

New Operations files under `src/cli/operations/OpenForge.Cli.Operations/Commands/Tool/`:

- `Models/Writing/Selection/ToolWriteSelectionRequest.cs`
- `Models/Writing/Selection/ToolWriteSelection.cs`
- `Models/Writing/Selection/ToolWriteSelectionResult.cs`
- `Models/Writing/Consent/ToolWriteConsentRequest.cs`
- `Shared/Writing/Selection/ToolWriteSelector.cs`
- `Shared/Writing/Consent/ToolWriteConsentOperation.cs`

New Unit files under `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Tool/`:

- `Shared/Writing/Selection/ToolWriteSelectorTests.cs`
- `Shared/Writing/Consent/ToolWriteConsentOperationTests.cs`

## Divergences

None recorded before implementation.
