---
open-forge:
  description: Propose the shared channel, config and native planning contracts for Task 90
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Shared Tool contracts

Shared Tool contracts are a proposed Framework and Root input change that lets the implementation lanes plan the same effects they execute.
For example, D receives A's future executable and destinations before the tool exists.

Status: PROPOSAL_READY. The patch is uncommitted and unapplied.

## Proposal

CONTRACT_REQUEST F1: `contract(tool): complete shared setup planning`.

Apply the [proposed patch](shared-contracts.patch) only after Root accepts the contract.
It targets frozen base `2e517348ef19e34afa6297c7efa7d0db5451b45c`.
It changes 22 proposed files, with 419 additions and 29 deletions.
Full patch SHA-256: `04154710409995db3754eb9380b39f5187d1b7631e49f04c6fac3a674072068f`.
It adds data declarations, service signatures and defining text.
It implements no command, host, parser or filesystem behavior.

The applicability check selects review, patch checks and Markdown checks.
The baseline managed build checks only the unchanged execution base.
No test class changes, full suites, native build or real tool execution are part of this proposal.
The BCL and existing packages cover the proposed data shapes.
Exceptional machinery: none.

## Contract changes

Framework model paths below start at `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/`.
The patch gives the full declarations.
These are all changed or new callable and model declarations:

| File                                              | Exact declaration change                                                                                                                                                                                                                                                                                                                                                                                                              | Affected lanes   |
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- |
| `Services/ToolChannelModels.cs`                   | Add `ToolChannelRequest.HomePath` and `ToolChannelResolution.Execution`. Add `ToolChannelExecutionPlan` with `HomePath`, `WorkspacePath`, `BinPath`, `NativeInstallDirectory`, `CommandPath`, `Invocation`, nullable `ToolPath` and `RuntimePath`, `Downloads`, `Extractions`, `Commands` and `Launchers`                                                                                                                             | A, C, D, F       |
| Same file                                         | Replace install request with `ToolChannelInstallRequest(ToolChannelResolution Plan)`. Add `ToolChannelInstallResult.PendingBackups`                                                                                                                                                                                                                                                                                                   | A, D, F          |
| `Recipes/ToolRecipeContext.cs`                    | Add `ToolRecipeContext` with `WorkspacePath`, `HomePath`, `BinPath`, `NativeHomePath`, `NativeInstallDirectory`, `MainRoot` and `ClientRoots`. Add `ToolRecipeDefinitions` for template tokens and `DollarEscapeToken="$$"`                                                                                                                                                                                                           | A, B, C, D, E, F |
| `Services/ToolConfigModels.cs`                    | Replace request `WorkspacePath` and `MainRoot` with `Context`. Add nullable `ToolCommandTarget? InstalledInvocation` beside the planned write `Invocation`. Replace remove request `WorkspacePath` with `Context`                                                                                                                                                                                                                     | B, D, F          |
| Same file                                         | Add `ToolConfigRemovalObservation` with `Addition`, `Current`, `Desired`, `Disposition` and nullable `Conflict`. Add `ToolConfigPlan.Removals`. Add `ToolConfigRemovePlan(Request, Changes)`                                                                                                                                                                                                                                          | B, C, D          |
| Same file                                         | Replace apply `Additions` and remove `KeptAdditions` with `RemainingAdditions`. Add apply `CompletedChanges` and `RemovedChanges`. Add `PendingBackups` to both results and `Unfinished` to remove results. Keep existing native-setting result members                                                                                                                                                                               | B, C, D          |
| `Services/ToolServiceContracts.cs`                | Add `Task<ToolConfigRemovePlan> PlanRemoveAsync(ToolConfigRemoveRequest request, CancellationToken cancellationToken)`. Change config removal to `Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigRemovePlan plan, CancellationToken cancellationToken)`                                                                                                                                                                           | B, D             |
| Same file                                         | Add `IToolSetupService.PlanAsync(ToolSetupRequest request, CancellationToken cancellationToken)`, returning `Task<ToolSetupPlan>`                                                                                                                                                                                                                                                                                                     | B, C, D, F       |
| Same file                                         | Add `IToolNativePlanReader.Read(ToolNativePlanParser parser, ToolProcessResult result)`, returning `ToolNativePlanObservation`                                                                                                                                                                                                                                                                                                        | B, C, F          |
| `Services/ToolSetupModels.cs`                     | Replace setup request `WorkspacePath` and `DescribeOnly` with `Context` and nullable `InstalledInvocation`. Keep planned `Invocation`. Add `ToolNativePreviewKind` and `ToolResolvedNativeOutput(PathPattern, Scope, Match, Purpose, ClientId)`                                                                                                                                                                                       | B, C, D, F       |
| Same file                                         | Add `ToolSetupPlan` with `Request`, `Command`, `PreviewKind`, nullable `DetectedClients`, `PossibleOutputs`, `ExpectedFileSha256` and `Problems`. Keep `ToolSetupResult` unchanged                                                                                                                                                                                                                                                    | B, C, D          |
| Same file                                         | Add `ToolNativePlanObservation` with `DetectedClients`, `Paths` and `Problems`                                                                                                                                                                                                                                                                                                                                                        | B, C             |
| `Services/ToolRecordModels.cs`                    | Add `ToolRecordWriteResult.PendingBackups`                                                                                                                                                                                                                                                                                                                                                                                            | B, C, D          |
| `Configuration/ToolNativeConfigChange.cs`         | Keep optional `IReadOnlyList<string>? ReadArguments`, `IReadOnlyDictionary<string, string?>? ReadEnvironment` and `string? ValueParserId`. Add optional `TimeSpan? ReadTimeout`, `int? ReadMaximumOutputBytes` and `ToolNativeStoredValueSpec? StoredValue`. Add `ToolNativeStoredValueSpec(PathTemplate, Parser, MissingValue, MaximumBytes)`, `ToolNativeStoredValueParser.CodeGraphTelemetryV1` and shared status parser constants | B, C, E          |
| Same file                                         | `CodeGraphTelemetryStatusParserId="codegraph-telemetry-status-v1"`, `TelemetryStatusPrefix`, `EnabledStatus`, `DisabledStatus`, `SavedChoiceReason`, `DefaultReason`, `DoNotTrackVariable`, `CodeGraphTelemetryVariable` and `CodeGraphNoDownloadVariable` name E’s status grammar and overlay keys                                                                                                                                   | B, C, E          |
| `Configuration/ToolMarkedBlockChange.cs`          | Add nullable `WhenNotIgnored` and `ToolEffectiveIgnoreCondition(string WorkspaceRelativePath)`                                                                                                                                                                                                                                                                                                                                        | B, C, E          |
| `Catalogue/ToolSetupSpec.cs`                      | Add `MaximumOutputBytes`, nullable `NativePlan` and `PossibleOutputs`. Add `ToolNativePlanSpec(Arguments, Parser, Timeout, MaximumOutputBytes)` and `ToolNativePlanParser.CbmInstallPlanV1`                                                                                                                                                                                                                                           | B, C, D, E       |
| Same file                                         | Add `ToolNativeOutputSpec(PathTemplate, Scope, Match, Purpose, OperatingSystem, ClientId)`, with nullable OS and client ID. Add `ToolNativeOutputMatch` and `ToolNativeOutputPurpose`                                                                                                                                                                                                                                                 | B, C, D, E       |
| `Channels/ToolChannelRequirements.cs`             | Add nullable `ToolTarget.MinimumLinuxKernelVersion` and `MinimumLibStdCppVersion`                                                                                                                                                                                                                                                                                                                                                     | A, C, D, E       |
| `Channels/ToolReleaseAsset.cs`                    | Add nullable `BundledRuntime`. Add `ToolBundledRuntimeSpec(Runtime, Version, RuntimeRelativePath, EntryRelativePath, Arguments)`                                                                                                                                                                                                                                                                                                      | A, C, E          |
| `Catalogue/ToolVersionProfile.cs`                 | Add required `ToolNativeHomePolicy NativeHomePolicy`. Add `ToolNativeHomePolicy { Node, HomeThenUserProfile }`                                                                                                                                                                                                                                                                                                                        | B, C, D, E, F    |
| `Records/ToolNativeSettingRecord.cs`              | Add required `string NativeHomePath` to bind previous and applied values                                                                                                                                                                                                                                                                                                                                                              | B, C, D, F       |
| Root `Composition/Models/CliCompositionInputs.cs` | Add nullable `Tools`                                                                                                                                                                                                                                                                                                                                                                                                                  | F                |
| Root `Composition/Models/CliToolHostInputs.cs`    | Add `CliToolHostInputs` with `HomePath`, optional `string? NativeHomePath`, `ClientRoots`, `ProcessEnvironment`, `Processes`, `Downloads` and `Path`                                                                                                                                                                                                                                                                                  | A, E, F          |

Names in parentheses for data records list their members.
The patch uses property initializers for those shapes except the small positional records shown there.
`IToolBackupService`, portable receipt shapes, schema versions and workspace controls stay unchanged.

## Defining text

All defining changes are inside the unapplied patch:

| Document under `.agents/memory/crystallized/documents/` | Change                                                                                                                                                    |
| ------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `cli/technical-designs/tool-setup.md`                   | Define channel parity, exact dollar escaping, profile home resolution, native output planning, bundled targets and injected Root hosts                    |
| `cli/technical-designs/tool-setup-lanes.md`             | Assign implementations and pending-backup completion to the existing lanes                                                                                |
| `cli/contracts/tool/behavior.md`                        | Record RD1 version pinning with normal channel choice and use native output planning before backups                                                       |
| `cli/contracts/tool/interface.md`                       | Define `data.tools` as an array of tool facts ordered by ID                                                                                               |
| `cli/contracts/shared/tool-records/interface.md`        | Define complete result sets, home-bound machine facts and the fixed digest projection, including authored command reads and home policy                   |
| `cli/contracts/shared/tool-records/behavior.md`         | Define stored-first native read selection, bounded command reads and status parsing, dropped-recipe planning, one-stage publication and backup completion |
| `maintenance/tools.md`                                  | Replace the native-status read instruction with the stored-setting contract and extend output review                                                      |

## Ordering

A plans the channel from the injected Open Forge home.
B then plans native outputs and additive drift using the resolved recipe context.
An installed invocation permits native inventory or dry run.
A missing binary permits only a description and declarations.
Preflight checks all available requirements before any effects.
A executes its checked channel plan, then D verifies the tool, revalidates native targets, backs them up and runs the unchanged upstream setup.
If installed native inventory reveals an unreviewed destination, stop before native setup and report it.

After native setup, B rechecks conditions and files affected by native output without widening the approved recipes.
D retires dropped recipes in prior receipt order, then applies current recipes in catalogue order.
Each call receives the full current receipt set even when it applies one observation.
Its `RemainingAdditions` is a complete replacement set, including untouched and unstarted entries.
`CompletedChanges` and `RemovedChanges` are per-call deltas.
C publishes each verified result before D starts another effect.
C keeps other tools and unrelated data unchanged and D reads fresh write expectations.
A failure stops work and keeps unresolved receipts.
No receipt is invented for an effect that couldn't be verified or published.
Replanning file changes keeps the first native previous-value observation.

A, B and C share one operation-scoped backup service.
They return pending completion handles.
D keeps one original backup per target across stages and supplies the latest target hash at completion.
Only final effect and record verification permits discard.
Failure keeps backups for manual recovery.
There is no rollback, journal, lease or new workspace state.

## Recipe checks

The grammar is a single nonrecursive expansion of the declared tokens.
It covers bin, workspace, Open Forge home, native home, native install directory, named client roots and slash-form mainRoot.
Unknown or malformed tokens and unavailable roots block planning.
`DollarEscapeToken="$$"` yields one literal dollar.
A single dollar outside a token stays literal and inserted values are never expanded again.
Null overlay values remove environment keys.
Managed-file resources use exact existing manifest IDs.
Template expansion stays inside the existing edit kinds.

Native config keeps optional command-read declarations and an optional stored reader.
Prefer the authored stored reader for both preflight and verification.
Otherwise use the bounded declared command read only through a verified installed invocation.
Neither source available blocks preflight, and a failing stored reader doesn't trigger command fallback.
The fixed digest includes all authored read metadata, bounds and the home policy.

The CodeGraph V1 stored reader returns only the on/off setting and the documented default for an absent file or missing setting.
It checks the native format using JSON token kinds without extracting identity text.
It needs no installed CLI.
Writes remain native `telemetry off`.
The tagged [telemetry source](https://github.com/colbymchenry/codegraph/blob/v1.6.2/src/telemetry/index.ts) defines the stored path, boolean and default.
The [telemetry documentation](https://github.com/colbymchenry/codegraph/blob/v1.6.2/TELEMETRY.md) describes the off command and its identity and queue effects.
The proposal doesn't probe, save, back up or report identity or queues.
Unreadable or malformed stored settings block.
Project Remove keeps the account setting and prints the native manual command.

CBM inventory uses its native file arrays and hook paths, plus declared legacy and profile supplements.
First-install declarations are a conservative possible set.
They never select clients or narrow default discovery.
Existing-pattern expansion covers profile subfolders and legacy files.
A literal final filename is kept even when absent inside an existing matched directory.
Client supplements follow native detected IDs when available and stay conditional descriptions before installation.
Install and uninstall have separate possible-output declarations.
Report-only outputs also cover native empty-directory cleanup.
Report-only effects exclude native telemetry and cache data from backup reads.
The effective-ignore condition checks coverage after native output and keeps an existing matching receipt.

The two added target fields describe only the evidenced Linux kernel and libstdc++ floors.
Existing libc and OS fields cover the other requirements.
Wrapper Node stays separate.
The tagged [bundle script](https://github.com/colbymchenry/codegraph/blob/v1.6.2/scripts/build-bundle.sh) declares the runtime entry paths and flags.
The [Node platform table](https://github.com/nodejs/node/blob/v24.16.0/BUILDING.md) defines its platform requirements.
E compiles these facts into entries after acceptance.

## Implementation split

The next assignments are:

- A - channel plans and execution, absolute invocation and runtime paths, environment-aware process hosts and PATH hosts.
- B - recipe expansion, config and removal plans, stored-value reads, native output planning, effective-ignore checks and backups.
- C - concrete JSON codecs, native inventory parsing through `IToolNativePlanReader`, observations and expected-state record publication.
- D - whole-batch preflight, separate consent, native-first effects, one-stage complete receipt publication and final backup completion.
- E - reviewed entry declarations and owned Windows journeys. Use injected process execution for confirmed account uninstall and real `--yes` project-only cleanup.
- F - later Root host creation and injection, command composition and focused integration checks.

The current A process host already removes null overlay keys.
Its factory still needs the base-environment input when this contract is implemented.
B currently blocks unresolved templates, native parser IDs and dropped recipes.
Its apply and removal implementations currently finish backups before C publishes.
C's writer also completes its own backups.
Those consumers need the grouped contract transfer before dependent changes.
D's returned selection and consent code doesn't implement command leaves.

## Grouped corrections

The grouped review changes these shared declarations:

- Native reads - preserve optional command metadata and add optional stored reads. CodeGraph V1 uses stored reads throughout and keeps its native off write. The declared status reader uses shared `codegraph-telemetry-status-v1` constants and E's setting-line grammar.
- Native homes - each profile declares `Node` or `HomeThenUserProfile`. Root resolves from the unchanged base environment. An injected home can override this for owned journeys. Machine facts match tool, change and resolved home before reusing a previous value.
- Dollars - two dollars produce one literal dollar, a single dollar outside a token stays literal and inserted values aren't expanded again.
- RD1 - bare Install pins only the lock version. Channel choice stays existing-supported-install first. Only named `--channel` overrides it. Keep one installation. D corrects its selector after this contract lands.
- Results - `data.tools` is an array of the leaf's tool facts ordered by ID, including Inspect facts.

C keeps native inventory parsing. B keeps setup planning, templates and config. D sequences effects and completes backups. F later wires Root.
No adapter or shipping version is added.

## Verification

The observed checks are:

- Initial `git rev-parse HEAD` matched the supplied base. `git status --short` showed only `packets/lane-f.md`.
- `npm ci` passed. It reported nine moderate and one high advisory. Tracked dependencies didn't change.
- Baseline command `dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1` passed with zero warnings and errors.
- `git apply --check --whitespace=error-all .agents/memory/working/cli-development/tool-setup/evidence/shared-contracts.patch` passed against the frozen base. It didn't apply the patch.
- Final formatting, LF, link and unchanged-source checks are recorded in the lane packet.
- The grouped revision passes patch applicability, report formatting and Prettier checks for all seven proposed Markdown files through binary LF stdin.
- Nine local links and their heading fragments resolve in the existing or proposed files. All three allowed files use LF.
- The packet's original instructions and Divergences are unchanged. Its only Prettier difference is the supplied build-command indentation outside the writable sections.

No test cases ran.
The proposed patch hasn't been built or executed.
No payload snapshots or fixtures were regenerated.
Later integrated changes need focused consumer tests and one final payload regeneration after E's docs.
No model, schema, central composition or parent Task changed in this worktree.
Grouped review revisions change only the proposed patch, this report and the packet.
The revision checks don't rebuild the unchanged baseline or claim execution of the patch.
Only this report, the patch and the packet's Progress and Evidence sections are writable.
The integrator needs to Index this new report once.

## Not decided yet

- Maintainer question submitted: Which Open Forge version first ships Tool setup and becomes both entries' `MinimumOpenForgeVersion`? No version is invented or bumped.
- Root acceptance of CONTRACT_REQUEST F1 before implementation.
- E's exact possible-output declarations and execution evidence after the shared contract transfers.

The shipping version stays a maintainer decision.
RD1 is the coordinator's grouped-review direction recorded in defining text.
DTO placement and service responsibilities implement the assigned planning needs.
The Windows account-uninstall test split follows the assignment and changes no native product behavior.
