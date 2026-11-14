---
open-forge:
  description: Implement Task 90 lane C records and read commands
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Records and read commands

Lane C is the implementation that persists portable and machine tool facts and reports catalogue, status and drift through the CLI.
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

You alone edit Framework/Tools/Shared/{Records,Observation,Serialization}/**, Framework/Settings/** and Framework/Ownership/** below src/cli/framework/OpenForge.Cli.Framework/. This includes their codecs and schema constants. Do not edit schema JSON or actual workspace settings/lock.

You own Operations Commands/Tool/{List,Inspect,Status}/**, Commands/Tool/Shared/Observation/** and Commands/Tool/Models/Read/**, plus corresponding leaf Rendering, OutputText and tests. You also own tool findings and their required call sites/models/presentation within existing workspace Commands/Status/** and Commands/Doctor/**. Keep these modifications confined to tool observations. All Root command composition is protected.

You own this lane-c.md packet. The integrator owns group registration, common Tool group types and central rendering wiring. Each leaf binding accepts its parent Command. Do not duplicate a group binding or replace existing shared result policy.

## Steps

Follow this ordered implementation plan:

1. Check exact base, all Models/Records and Services contracts and existing production Settings/Ownership readers, codecs and writers. Read Tool list/inspect/status and workspace Status/Doctor contracts. Reuse only the missing-tool appender sources in the copy table, adapting their facts and tests.
2. Implement IToolRecordService. Settings v6 and lock v2 derive from current released v5/v1. Preserve unrelated and unknown fields. Settings declare ID/range. Lock requires exact version, channel and additive changes, with optional digest. No local paths or config preimages in the lock. Keep the existing ownership/Library facts.
3. Persist per-user tools, runtimes/node, path and workspace-hash files using the accepted filenames. Validate physical workspace-path binding. Native previous settings stay in machine state. No .open-forge-state/ writes or workspace lease/recovery dependency. Use expected file SHA-256/absence facts and atomic replacement. A write failure reports actual completed paths and unfinished work.
4. Implement List, Inspect and Tool Status operations against injected catalogue, IToolChannelService, IToolConfigService and IToolRecordService where needed. Root supplies home paths and real service instances. Each leaf provides its typed binding, concrete result/payload, source-generated JSON, presentation and output text. Use schema 3 with data.tools in ID order and existing shared statuses/exits. Do not depend directly on E's still-unimplemented concrete catalogue.
5. List can show the catalogue without an installed workspace. Inspect reports identity, every channel requirement, profiles, native setup/uninstall writes, Open Forge additions, telemetry, bin, tool/runtime paths and PATH readiness. Tool Status checks exact lock versus detected self-updates, supported/unsupported state, config drift and missing machine facts. Both show bin and manager-global-bin readiness. Reads never install, resolve latest or write state.
6. Add workspace Status/Doctor missing-tool and drift findings with actionable bare Install or named reapply advice. Use bounded detection, including declared CodeGraph no-download environment. Keep findings and observed facts separate from Repair. No new Tool Repair action. Tests exercise Doctor only on owned fixtures, never the repository workspace.
7. Build and run only touched or created classes. Add codec tests for unknown keys/version shapes, all five receipt kinds, exact lock restore facts, absent machine state, expectation mismatch and failed writes. Use Imprint for rendered output. Do not regenerate existing payload-dependent fixtures/snapshots now. Report that impact for the final integrator pass.
8. Run both .NET checks, then return READY_FOR_REVIEW with service constructors, leaf binding/presentation signatures, workspace contributor wiring, exact tests and contract requests. The coordinator handles the commit continuation.

## Acceptance

No machine facts or prior repository values leak into settings/lock. Unknown keys survive writes. No old tool manifest/registry/roles/guidance is copied. Missing versus unavailable and current versus unsupported facts stay distinct. Reports share text/JSON facts, stdout stays one JSON document and existing commands keep their semantics. Account-wide native settings are kept on Remove; readers describe their state without restoring them. The global version flag still refers to Open Forge.

## Progress

Done: Implemented settings-only explicit grants and adapted the current ABI. The combined-base build, 69 focused cases and both .NET checks pass. The six synthetic baselines stay byte-identical.
Now: READY_FOR_REVIEW. Task 90 "Tool setup" (phase 3/3): milestone 3/7. The candidate is uncommitted.
Next: Receive the coordinator's review and commit continuation.
Blocker: None. No additional shared contract change is needed.

## Evidence

The current explicit-grant base is `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
The accepted four-fix candidate used `b22e1ec393280935dea93a312f2a1aa337834ad5`.
The coordinator advanced the idle worktree while keeping the lane candidate.
The prior frozen continuation used `6d007254d5ca2937f4874ba5a93161f585814d8c`.
The coordinator kept the first pass's uncommitted files while updating the base.
The first pass ran 42 focused cases, a managed build and both .NET checks successfully.
Its logs and source inventory are under `artifacts/tool-lane-c/first-run/`.

The applicability check uses the existing developer-file safety rules, BCL JSON and source generation, physical path checks, SHA-256 expectations, Git-aware sibling backups and atomic replacement.
Pure parsing, projection and binding evidence uses Unit tests.
Owned-file publication and workspace command evidence uses Integration tests.
The supported failures are malformed inputs, interruption, changed files and normal file errors.
No exceptional platform machinery is chosen.
The coordinator and CI handle the complete managed and Native AOT gates.

### Implementation

The candidate includes these parts:

- Settings v6 and lock v2 derived from released v5 and v1. Unknown fields and existing ownership and Library facts survive writes.
- Five additive receipt kinds, exact lock versions, channels and optional profile digests. Opaque JSON payloads replace exactly.
- Machine files at the decided tool, Node, PATH and workspace filenames. `HomePath` names the Open Forge home. State is directly below it.
- Missing-home reads that create no directories. Writes check expected state and physical containment before atomic replacement.
- Pending backup completions with actual written paths, verification progress and failure handles. C never completes backups.
- Native settings bound to tool ID, change ID and resolved physical native home. Unknown nested fields stay with that complete identity. Missing or relative home bindings report invalid state.
- A source-generated CBM inventory reader for every client, all five file arrays and hook paths. Failed, incomplete, wrong-shaped or unsafe inventory returns no usable paths. Command advice is never executed.
- Complete injected recipe context, recorded-install detection and installed invocation in read planning. Other-home setting facts stay in the complete input set. Retired additions contribute drift.
- Inspect text and JSON for Linux floors, bundled runtimes, native home policy, inventory, potential outputs, stored-setting policy and process bounds. Effective native environment values stay separate from stored values.
- Incomplete results for unavailable or invalid runtime and PATH records. Missing state stays distinct.
- Typed read bindings with deferred operation factories and pure invalid List catalogue data. Workspace Status and Doctor defer their Tool reader too.
- Workspace missing, drift, unsupported and unavailable findings with targeted commands and no Tool Repair action.

Native inventory follows the checked CBM source in `src/cli/cli.c:10980-11098,11298-11347` at tag `v0.12.1` and the design's Native outputs rules.
The process host enforces the authored output bound and reports failure or interruption before parsing.
The parser keeps stderr separate and doesn't select clients or infer cleanup destinations.
B combines the returned inventory with the declared supplements and checks containment before native writes.

Read commands never install, resolve latest, apply configuration or publish records.
They use injected services and contexts without reading ambient environment.
Stored native values and previous settings stay in machine state.
Raw inherited environment values don't enter reports or records.

### Connections

The service constructors are:

```csharp
ToolRecordService(ToolCatalogue catalogue, IToolVersionService versions, IToolBackupService backups)
ToolNativePlanReader()
ToolObservationReader(IToolChannelService channels, IToolConfigService config,
    IToolVersionService versions, IToolPathHost paths,
    Func<ToolObservationRequest, ToolVersionProfile, ToolDetectionResult, ToolRecipeContext> recipeContext)
ToolReadObserver(ToolCatalogue catalogue, IToolRecordService records,
    ToolObservationReader observations, string homePath)
ToolWorkspaceObservationReader(ToolCatalogue catalogue, IToolRecordService records,
    ToolObservationReader observations, string homePath)
```

Root provides the recipe-context delegate, resolving the profile's native home policy from its injected base environment.
The delegate receives the entry, record locations, recorded facts, main root, chosen profile and installed detection.
It gives resolved absolute paths, client roots and only the declared native environment variables.
Root appends `ToolRecordDefinitions.HomeDirectoryName` to its user home once.
Each record publication needs fresh `ReadAsync` expectations.
D completes the returned pending backups after final verification and record publication.

Each read operation takes `ToolReadObserver`.
The leaf connections are:

```csharp
ToolListBinding.CreateSymbols(Command parent) // returns Command
ToolInspectBinding.CreateSymbols(Command parent) // returns ToolInspectSymbols
ToolStatusBinding.CreateSymbols(Command parent) // returns ToolStatusSymbols
ToolListBinding.CreateRequestBinding(Command symbols, Func<ToolListOperation> createOperation,
    CliHelpContent help, ToolCatalogue catalogue)
ToolInspectBinding.CreateRequestBinding(ToolInspectSymbols symbols, Func<ToolInspectOperation> createOperation,
    CliHelpContent help, ToolCatalogue catalogue)
ToolStatusBinding.CreateRequestBinding(ToolStatusSymbols symbols, Func<ToolStatusOperation> createOperation,
    CliHelpContent help, ToolCatalogue catalogue)
```

Factories run only from the typed operation delegate.
Binding, help and invalid-result construction need no Tool home or service construction.
`ToolListOperation.Catalogue(ToolCatalogue, ToolRecordsResult? = null)` is a pure catalogue projection.
Each leaf has `CreateHelp()` and `Rendering` on its `ToolListPresentation`, `ToolInspectPresentation` or `ToolStatusPresentation` class.
The three presentation instances use the source-generated `ToolReadPayload` graph.
The integrator handles the common Tool group and central rendering registration.

Workspace contributors use these connections:

```csharp
StatusOperation(OperationalContributorCatalogue contributors, PhysicalPathResolver physicalPathResolver,
    Func<ToolWorkspaceObservationReader>? tools = null)
DoctorOperation(OperationalContributorCatalogue contributors, RenameEvidenceReader? renames = null,
    Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>>? readLayout = null,
    Func<ToolWorkspaceObservationReader>? tools = null)
ToolWorkspaceObservationReader.ReadAsync(string workspacePath, string mainRoot, CancellationToken token)
```

The workspace factories run during valid Status or Doctor execution after the existing observations.
Tool findings keep the existing aggregation and result policy.

### Frozen checks

The continuation uses SDK `10.0.101` on managed Windows x64 in this worktree.
`npm ci` exited zero. Its log is `artifacts/tool-lane-c/continuation/npm-ci.txt`.
The final solution build exited zero with zero warnings and errors:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Its log is `artifacts/tool-lane-c/continuation/build-accepted.txt`.
The initial continuation build used the same options with restore enabled and also passed.
The NuGet audit override stays on the local invocation.

Each Unit selection used this exact command shape, with the class and minimum from the table:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off
```

The Unit artifact is `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`.
These 50 Unit cases passed. Every minimum equals the executed count:

| Class                                                                                            | Count | Log under `artifacts/tool-lane-c/continuation/` |
| ------------------------------------------------------------------------------------------------ | ----- | ----------------------------------------------- |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolPortableCodecTests`       | 10    | `ToolPortableCodecTests.txt`                    |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolRecordJsonTests`          | 4     | `ToolRecordJsonTests.txt`                       |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation.ToolNativePlanReaderTests`      | 15    | `ToolNativePlanReaderTests.txt`                 |
| `OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Aggregation.StatusToolFindingAppenderTests` | 5     | `StatusToolFindingAppenderTests.txt`            |
| `OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Domains.DoctorToolFindingAppenderTests`     | 4     | `DoctorToolFindingAppenderTests.txt`            |
| `OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation.ToolReadBindingTests`             | 4     | `ToolReadBindingTests.txt`                      |
| `OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation.ToolReadObserverTests`            | 6     | `ToolReadObserverTests.txt`                     |
| `OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests`               | 2     | `ToolReadPresentationTests-final.txt`           |

Each Integration selection used this exact command shape, with the class and minimum from the table:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off
```

The Integration artifact is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`.
These 22 Integration cases passed. Every minimum equals the executed count:

| Class                                                                                  | Count | Log under `artifacts/tool-lane-c/continuation/` |
| -------------------------------------------------------------------------------------- | ----- | ----------------------------------------------- |
| `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records.ToolRecordServiceTests` | 13    | `ToolRecordServiceTests.txt`                    |
| `OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Observation.ToolReadCommandTests` | 9     | `ToolReadCommandTests-final.txt`                |

All 72 final selected cases passed with zero failures, warnings and skips.
The final two affected classes were rerun after snapshot wording review and the physical native-home assertion correction.
The earlier presentation selection failed on the expected reviewed baseline differences. It wasn't a flaky failure.

The final format checks both exited zero:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Their logs are `whitespace-final.txt` and `style-final.txt` in the same artifact folder.
Git whitespace, LF and UTF-8 BOM checks pass for all 77 changed paths.
`changed-files.txt` lists them and `source-sha256.txt` records their final SHA-256 values.
`test-artifact-sha256.txt` identifies the final managed test assemblies.
Protected paths are unchanged. The packet's content outside Progress and Evidence is unchanged.

### Frozen snapshots

The six first-pass Imprint baselines are synthetic read reports under `ToolReadPresentationTests/ReadReports`.
Only those lane-owned baselines change for the reviewed continuation output.
The reviewed receive artifact is `artifacts/command-output-snapshots/54e12d2a0e46483fb6734d94031d4715/ToolReadPresentationTests/ReadReports/c7ad498cec1f4fcdac7947177d59eb33`.
Review checked all three text reports and JSON reports.
Inspect and Status share the same JSON data. List differs only by its catalogue state in this synthetic case.
Review added the separate effective-on native override and retired additions to Status text and removed a generated channel-space gap.
The six final baselines pass Imprint verification in `ToolReadPresentationTests-final.txt`.
Existing payload-driven fixtures and snapshots aren't regenerated.
The integrator's one final pass covers settings v6, lock v2, new output definitions and the final catalogue and help composition.

### Grouped corrections

F's read-only `records-review.md` identified four changes at the preservation, presentation and machine-ingress stages.
All four are fixed:

| Finding | Change                                                                                                                                                                                                         | Focused evidence                                                                                                                                                                                                                                                        |
| ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-R1    | Ownership preservation reads known members from the existing source-generated schema. An omitted known member is removed before unknown siblings are merged. This replaces the one-off Framework removal rule. | Five round trips clear Library `GitIgnore`, Framework `GitIgnoredRoutes`, Extension version and source, and Framework source version. Every case keeps an unknown sibling.                                                                                              |
| C-R2    | PATH rows match typed kind, target and both markers. Region rows match path and region. Git-ignored route rows match directory and entrypoint. New identities receive no old unknown fields.                   | Four PATH cases cover reordered rows, a numeric enum representation, unknown row and prior/applied fields, every changed identity part and an unmatched row. Two region cases cover Framework and Extension rows. One route case covers reorder and changed entrypoint. |
| C-R3    | Authored Inspect labels, command headings, environment phrases and fallback words move into its leaf OutputText factories. Status's two override-state phrases move there too.                                 | Both synthetic presentation cases pass in verification mode. All six baseline SHA-256 values still match the frozen 72-case candidate.                                                                                                                                  |
| C-R4    | Machine validation rejects null launcher, native-setting and PATH-change elements before reading their members. The argument cause names the JSON collection.                                                  | Three read cases return Invalid facts and Incomplete Tool status. Three write cases return failure results with no writes, pending backups or backup calls.                                                                                                             |

The standard .NET 10 `JsonTypeInfo.Properties` and `ElementType` metadata provide the ownership schema.
The existing source-generated context resolves child types, including route entries stored in the envelope's JSON field.
No reflection resolver, dependency, model-contract change or exceptional platform mechanism is added.
Opaque receipt JSON and added-item replacement are unchanged and pass their existing focused codec cases.

The current SDK is `10.0.101` on managed Windows x64.
`npm ci` exited zero and installed 95 packages. Its log is `artifacts/tool-lane-c/review-fixes/npm-ci.txt`.
The combined-base solution build exited zero with zero warnings and errors:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Its log is `artifacts/tool-lane-c/review-fixes/build.txt`.
The audit override stays on the invocation.
The current Unit and Integration artifacts are the same managed assembly paths listed under Frozen checks.
Their current hashes are in `artifacts/tool-lane-c/review-fixes/test-artifact-sha256.txt`.

Each current Unit class used this exact command shape:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off
```

These current Unit selections all passed:

| Qualified class                                                                                             | Minimum and executed | Log under `artifacts/tool-lane-c/review-fixes/` |
| ----------------------------------------------------------------------------------------------------------- | -------------------- | ----------------------------------------------- |
| `OpenForge.Cli.Core.UnitTests.Framework.Ownership.Shared.Serialization.WorkspaceOwnershipPreservationTests` | 8                    | `WorkspaceOwnershipPreservationTests.txt`       |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolRecordJsonTests`                     | 8                    | `ToolRecordJsonTests.txt`                       |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolPortableCodecTests`                  | 10                   | `ToolPortableCodecTests.txt`                    |
| `OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests`                          | 2                    | `ToolReadPresentationTests.txt`                 |

The current Integration class used this exact command:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records.ToolRecordServiceTests --minimum-expected-tests 19 --fail-warns on --fail-skips on --no-ansi --progress off
```

All nineteen cases passed. Its log is `artifacts/tool-lane-c/review-fixes/ToolRecordServiceTests.txt`.
The current focused result is 47 passed, zero failed, zero skipped and zero warnings.
No full suite or native suite ran.

The required .NET checks both exited zero:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Their logs are `whitespace.txt` and `style.txt` under `artifacts/tool-lane-c/review-fixes/`.
Markdown formatting and final LF verification are recorded there too.
The current full inventory and source hashes are `changed-files.txt` and `source-sha256.txt` in that folder.
The six baseline comparisons are in `snapshot-sha256.txt`.
The source changes since the frozen review are listed in `review-delta.txt`.

The only new source file in this correction is `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Ownership/Shared/Serialization/WorkspaceOwnershipPreservationTests.cs`.
No new routed Markdown file is added.
All public construction and binding signatures in Connections stay unchanged on the current combined base.
The peer report, protected contracts, Root composition, schema JSON and actual workspace settings/lock are unchanged.
Only this packet's Progress and Evidence sections are updated.
The six baseline files and final payload fixtures aren't regenerated.

### Explicit grants

`IToolRecordService.AddAllowInstallPathsAsync(ToolRecordGrantRequest, CancellationToken)` now publishes the accepted explicit settings edit.
It calls a settings-only writer and never calls ordinary `WriteAsync`.
All requested values pass the existing `PortableWorkspacePath.TryNormalize` grammar before any mutation.
The existing `WorkspaceSettingsCodec.AddAllowInstallPath` builds the complete edit in memory.
It keeps unknown keys, key order and the existing settings schema version.
An absent settings file gets only the codec's authored settings shape.

The writer checks the settings SHA-256 or absence from the supplied observation.
It uses `WorkspaceSettingsReader` for readable, ordinary settings and checks physical paths before replacement.
The extracted `ToolRecordFileWriter` provides the same expected-state and atomic-replacement primitives to both record writers.
No normal Tool lock, machine record, runtime record or Open Forge home is created for a grant.
An invalid existing lock doesn't block an otherwise valid explicit settings edit.
Tool-home resolution isn't used by this path.

Backups use the injected shared Tool service.
Every returned handle stays pending, including failed or cancelled publication after preparation.
Verified settings replacements have verified pending handles and their actual applied SHA-256.
Already admitted values return a no-op without changing bytes or preparing a backup.
This code adds no lease, journal, recovery bundle, rollback or backup completion.
D keeps explicit grants separate from consent, calls this method only during apply and rereads current expectations before later Tool publication.
Dry-run doesn't call it. A later blocked or cancelled Tool operation keeps the saved grant.

The current callable connections are:

```csharp
ToolRecordService(ToolCatalogue catalogue, IToolVersionService versions, IToolBackupService backups)
Task<ToolRecordWriteResult> AddAllowInstallPathsAsync(ToolRecordGrantRequest request,
    CancellationToken cancellationToken)
ToolRecordGrantWriter(IToolBackupService backups)
```

Other constructors, typed read bindings and presentation connections in Connections stay unchanged.
The npm presentation seed now provides the required package entry.
C has no download-host test double. Its read tests don't read npm metadata.
The PATH double's effect methods stay throw-only and construct no setup/removal result requiring backup fields.
The current combined ABI builds without protected edits.

### Grant checks

The current checks ran on SDK `10.0.101`, managed Windows x64, at the explicit-grant base above.
`npm ci` exited zero and installed 95 packages. Its log is `artifacts/tool-lane-c/grants/npm-ci.txt`.
The combined solution build exited zero with zero warnings and errors:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

Its log is `artifacts/tool-lane-c/grants/build.txt`.
The audit override stays on the local invocation.
The current managed test assemblies use the paths listed in Frozen checks, with hashes in `artifacts/tool-lane-c/grants/test-artifact-sha256.txt`.

Each current Unit selection used this exact command shape:

```powershell
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off
```

All 28 selected Unit cases passed:

| Qualified class                                                                                             | Minimum and executed | Log under `artifacts/tool-lane-c/grants/` |
| ----------------------------------------------------------------------------------------------------------- | -------------------- | ----------------------------------------- |
| `OpenForge.Cli.Core.UnitTests.Framework.Ownership.Shared.Serialization.WorkspaceOwnershipPreservationTests` | 8                    | `WorkspaceOwnershipPreservationTests.txt` |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolRecordJsonTests`                     | 8                    | `ToolRecordJsonTests.txt`                 |
| `OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization.ToolPortableCodecTests`                  | 10                   | `ToolPortableCodecTests.txt`              |
| `OpenForge.Cli.Core.UnitTests.Commands.Tool.Models.Read.ToolReadPresentationTests`                          | 2                    | `ToolReadPresentationTests.txt`           |

Each current Integration selection used this exact command shape:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class <class> --minimum-expected-tests <count> --fail-warns on --fail-skips on --no-ansi --progress off
```

All 41 selected Integration cases passed:

| Qualified class                                                                        | Minimum and executed | Log under `artifacts/tool-lane-c/grants/` |
| -------------------------------------------------------------------------------------- | -------------------- | ----------------------------------------- |
| `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records.ToolRecordServiceTests` | 19                   | `ToolRecordServiceTests.txt`              |
| `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records.ToolRecordGrantTests`   | 22                   | `ToolRecordGrantTests.txt`                |

The new cases cover settings absence, existing unknown keys and order, codec parity, unchanged schema and lock bytes, an unusable Tool home, portable admission no-ops, invalid batches, stale bytes and absence, missing expectations, malformed and nonordinary settings, backup failure handles and cancellation.
The four prior review findings are rechecked by the existing focused classes above.
All 69 selected cases passed with zero failures, warnings and skips.
No full suite or native suite ran.
The six presentation baselines are verified without updates.

Both required .NET checks exited zero:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Their logs are `whitespace.txt` and `style.txt` under `artifacts/tool-lane-c/grants/`.
Markdown formatting, Git whitespace, LF, UTF-8 and BOM results are recorded there too.
`changed-files.txt` and `source-sha256.txt` give the current whole candidate.
`grant-delta.txt` lists the changes since the accepted four-fix return.
`snapshot-sha256.txt` binds the six unchanged baseline files.

Three source files are new in this continuation:

- `Framework/Tools/Shared/Records/ToolRecordFileWriter.cs`
- `Framework/Tools/Shared/Records/ToolRecordGrantWriter.cs`
- `Framework/Tools/Shared/Records/ToolRecordGrantTests.cs` in Integration tests

No new routed Markdown file is added.
The protected models, schema JSON, Root composition, parent Task, actual workspace settings/lock and final payload fixtures are unchanged.
Only this packet's Progress and Evidence sections change.

### Review return

The prior `contract(tool): expose native environment override facts` request is resolved.
No new shared contract request is open.
Root wiring, actual service integration, real tool journeys and the complete CI gate belong to the coordinator's later integration.
No new routed Markdown file is added.
The six new source files in this continuation are:

- `Framework/Tools/Shared/Observation/ToolNativePlanReader.cs`
- `Framework/Tools/Shared/Serialization/ToolNativePlanJsonContext.cs`
- `Framework/Tools/Shared/Serialization/Models/ToolCbmNativePlan.cs`
- `Framework/Tools/Shared/Observation/ToolNativePlanReaderTests.cs` in Unit tests
- `Framework/Tools/Shared/Serialization/ToolRecordJsonTests.cs` in Unit tests
- `Commands/Tool/Shared/Observation/ToolReadObserverTests.cs` in Unit tests

The full 77-path inventory includes the kept first-pass files and is recorded with the final checks.
Changes stay uncommitted for grouped review. No push, PR, merge, real tool installation or persistent PATH, registry or profile change runs here.
Doctor runs only on an owned temporary fixture, never this repository workspace.

## Divergences

None recorded before implementation.
