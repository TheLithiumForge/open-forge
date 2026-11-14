---
open-forge:
  description: Map checked Tool read connections and pending lane inputs to later Root composition
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Root integration

Root integration is the pending wiring that connects the Tool lanes to the composed CLI and its existing report pipeline.
For example, Tool Inspect uses C's typed binding and rendering with deferred service construction, so help doesn't resolve homes or create privileged hosts.

Status: Intermediate reviewable home-provider base. F-H1 and F-H2 are unresolved before final acceptance. Full Root graph and registration stay deferred.

## Source checkpoint

The intermediate home-provider commit uses base `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`. The original 49-case home-provider check used `b22e1ec393280935dea93a312f2a1aa337834ad5`. The C review below describes its earlier frozen candidate on `6d007254d5ca2937f4874ba5a93161f585814d8c`.
C returned a frozen, uncommitted 77-file candidate.
All file hashes and both test assembly hashes match C's continuation manifests.
The focused [records review](records-review.md) checks the nine grouped fixes and records four actionable findings.
This pass isn't the fresh final adherence review.

E's catalogue commit is `1ec6ad921943b881f819f3a8eb66b4796eeabacb`, whose parent is the earlier C review base.
The catalogue is integrated at this slice's base. C-R1 through C-R4 were accepted and routed to C. Their correction run is separate from this slice.
A, B and D signatures below are the earlier inspected partial-source connections at `09bcb9ebdb89abed343d4c228283a8ee5aeba7c9`.
They aren't a finished review of those mutable lanes.
Refresh their construction entries from checked integrated source before writing Root code.

Paths use these prefixes:

- Root - `src/cli/root/OpenForge.Cli/`.
- Framework - `src/cli/framework/OpenForge.Cli.Framework/Framework/`.
- Operations - `src/cli/operations/OpenForge.Cli.Operations/Commands/`.
- Rendering - `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/`.
- Integration tests - `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/`.

The accepted [Tool design](../../../../crystallized/documents/cli/technical-designs/tool-setup.md) defines behavior and shared inputs.
This map adds no contract or product decision.

## C connections

These are the exact constructors in C's returned candidate.
They construct services without reading homes or probing tools:

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
ToolListOperation(ToolReadObserver observer)
ToolInspectOperation(ToolReadObserver observer)
ToolStatusOperation(ToolReadObserver observer)
```

The source locations are Framework `Tools/Shared/Records/ToolRecordService.cs:8`, `Tools/Shared/Observation/ToolNativePlanReader.cs:10`, `ToolObservationReader.cs:14` and `ToolWorkspaceObservationReader.cs:7`.
The last two observation paths share that Observation folder.
Operations `Tool/Shared/Observation/ToolReadObserver.cs:13` and the three leaf `Tool*Operation.cs` files provide the command constructors.

Root gives the recipe-context delegate resolved paths, native home, client roots and the declared native environment map.
Its typed input is `ToolObservationRequest(ToolCatalogueEntry Entry, ToolRecordsRequest Locations, ToolRecordsResult Records, string MainRoot)`, together with the chosen profile and installed detection.
Use `request.MainRoot` from the current workspace settings.
C passes recorded machine state into detection and the verified invocation into B's config planning.
It keeps other-home native facts in the complete tool-specific list.

C's three symbol factories add each leaf to the supplied parent:

| Leaf    | Symbol return and factory                                             | Exact request binding                                                                                                                                                                                 |
| ------- | --------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| List    | `Command ToolListBinding.CreateSymbols(Command parent)`               | `CliRequestBinding<ToolListRequest, ToolListResult> CreateRequestBinding(Command symbols, Func<ToolListOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)`                     |
| Inspect | `ToolInspectSymbols ToolInspectBinding.CreateSymbols(Command parent)` | `CliRequestBinding<ToolInspectRequest, ToolInspectResult> CreateRequestBinding(ToolInspectSymbols symbols, Func<ToolInspectOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)` |
| Status  | `ToolStatusSymbols ToolStatusBinding.CreateSymbols(Command parent)`   | `CliRequestBinding<ToolStatusRequest, ToolStatusResult> CreateRequestBinding(ToolStatusSymbols symbols, Func<ToolStatusOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)`     |

Their factories run only from the typed `Operation` delegate, at binding lines 39, 41 and 41 respectively.
Invalid List uses `ToolListOperation.Catalogue(ToolCatalogue catalogue, ToolRecordsResult? records = null)` without constructing services.
Inspect and Status reject unknown IDs in their typed binders before their operation factory runs.
Root keeps these binders and invalid-result policies.

Workspace constructors are now deferred too:

```csharp
StatusOperation(OperationalContributorCatalogue contributors, PhysicalPathResolver physicalPathResolver,
    Func<ToolWorkspaceObservationReader>? tools = null)
DoctorOperation(OperationalContributorCatalogue contributors, RenameEvidenceReader? renames = null,
    Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>>? readLayout = null,
    Func<ToolWorkspaceObservationReader>? tools = null)
Task<IReadOnlyList<ToolObservation>> ToolWorkspaceObservationReader.ReadAsync(
    string workspacePath, string mainRoot, CancellationToken token)
```

In `CliStandaloneComposer.BuildStatus` and `BuildDoctor`, pass an execution-time reader factory as `tools:`.
Keep the existing contributors, rename reader and layout reader.
The factories run at Operations `Status/StatusOperation.cs:66` and `Doctor/DoctorOperation.cs:32`, after the normal observations.
C already appends findings and updates aggregation.
Root adds neither another Tool contributor nor a second append.
The appenders take `StatusResult` or `DoctorResult`, `IReadOnlyList<ToolObservation>` and optional `string? failure = null`.

## Bindings and reports

F's later Tool composer creates one group with List, Inspect, Status, Install, Update and Remove in that order.
Follow the existing [Library composer](../../../../../../src/cli/root/OpenForge.Cli/Composition/CliLibraryComposer.cs).
Add its branch to root's setup-and-management help group.
Grouped leaves don't enter `rootLeaves`.
The bare group shows help.

Each C presentation has `CreateHelp()` and cached `Rendering`.
Help currently returns `CliHelpContent.Empty`.
Rendering has type `CliReportRendering<ToolListResult, ToolReadPayload>`, `CliReportRendering<ToolInspectResult, ToolReadPayload>` or `CliReportRendering<ToolStatusResult, ToolReadPayload>`.
All use `ToolReadJsonContext.Default.ToolReadPayload`, camel-case properties and null omission.
The payload is `ToolReadPayload(IReadOnlyList<ToolReadTool> Tools)`.
Tool rows contain rich facts in ID order.

Root's existing closure is `CliReportBinding.Close<TRequest, TResult, TData>(CliRequestBinding<TRequest, TResult> binding, CliReportRendering<TResult, TData> rendering)`.
It returns `CliReportCommandBinding<TRequest, TResult, TData>` with the existing `ICliCommandResult` and class constraints.
Proposed Root call:

```csharp
CliReportBinding.Close(
    ToolInspectBinding.CreateRequestBinding(symbols, CreateInspectOperation,
        ToolInspectPresentation.CreateHelp(), catalogue),
    ToolInspectPresentation.Rendering)
```

Select and render once through that pipeline.
Root needs no reflection serializer, result switch or second renderer.
D's earlier partial result was `ToolWriteResult : ICliCommandResult` with `ToolWriteData.Tools`, `Preview`, `Plan` and `Outcomes`.
Its final leaf factories, operations and generated JSON connection await D's checked return.
Keep writer sequencing, typed consent, complete receipt publication and backup completion in D.

## Home provider

The coordinator authorized one independent implementation slice before core integration.
The new `Composition/Shared/ToolHomes/ToolHomeProvider.cs` accepts the explicit base environment, known-folder capability, platform fact and optional existing `CliToolHostInputs`.
Construction freezes environment and injected bindings without resolving paths or calling a host.
`HomePath` resolves the Open Forge home once on demand.
`Resolve(ToolVersionProfile)` gives immutable home, bin, native-home, client file and directory bindings, unavailable bindings and narrow raw native environment facts.
The [native-home receipt](native-home-roots.md) gives exact signatures, source choices, declared-key coverage and focused evidence.
The current ABI adds npm metadata reads to the injected download host and pending backup handles to PATH results.
Only the owned unused-host fake needs the new throwing metadata method.
That fake constructs no PATH result. The provider doesn't call either host or consume those results.
All ten provider files stay byte-identical to the original checked slice.
E confirmed two unresolved native-parity corrections. F-H1 covers supported JSON5 syntax and F-H2 covers zero-length config fallback. Include precedence is correct.
F keeps provider and parser implementation. Parser work waits for the coordinator's dated contract increment and E's exact functions, licence and fixture plan. This intermediate commit changes no parser or binding.

Later, construct one provider per selected invocation.
For C's context delegate, use its profile-specific `NativeHomePath`, `Clients.Bindings` and `NativeEnvironment`, together with C's workspace and main root and A's verified install directory.
For D's host facts, use `HomePath`, resolve each selected profile's policy and use CBM's `Clients.Bindings`.
Check unavailable required bindings after native discovery and stop before effects when B needs a path that has no safe resolved choice.
Never replace an unavailable binding with an inventory-returned path.
Full graph and registration aren't implemented in this slice.

## Homes and laziness

The accepted `CliCompositionInputs.Tools` seam is optional `CliToolHostInputs`.
It carries required `HomePath`, `ClientRoots`, `ProcessEnvironment`, `Processes`, `Downloads` and `Path`, plus optional `NativeHomePath`.
Resolve the production OS-user home and append `ToolRecordDefinitions.HomeDirectoryName` once at execution time.
C now places state directly under that Open Forge home.
Its missing-home reads create no directories.
The earlier home mismatch is closed.

For CodeGraph's Node policy, resolve Windows `USERPROFILE` or POSIX `HOME`, then the matching OS-user fallback when absent.
CBM's HomeThenUserProfile policy takes nonempty `HOME`, then `USERPROFILE`.
Resolve physical paths without changing the normal child environment.
An injected native home overrides that resolution for owned journeys.
The journey must provide child environment that makes the native tool use the same home.

For CodeGraph's native setting, pass only the declared `DO_NOT_TRACK` and `CODEGRAPH_TELEMETRY` values through `ToolRecipeContext.NativeEnvironment`, using their production definitions.
Null means unavailable and an empty map means checked with no declared variables present.
B evaluates precedence and returns `ToolConfigObservation.NativeEnvironmentOverrides` with deciding variable and effective on/off value.
C reports that normalized map separately from stored values.
Raw environment values stay out of reports and records.
The shared environment shape is accepted at this base.

Construct the home-dependent graph only after a valid relevant operation is selected.
Root, group and leaf help, parser failures, rejected IDs and unrelated commands construct only symbols and pure catalogue data.
Their callbacks must not resolve homes, read stores, probe tools, create HTTP clients or prepare backups.
Use one operation-local graph and one backup service for writer stages.
Avoid an application-scoped mutable service cache across invocations.
C's factories now provide the required execution entry without another binder.

Use supplied process, download and PATH hosts throughout.
Never replace an injected PATH host with the production user-PATH environment.
Owned tests inject an absent Open Forge home, native home, environment and hosts that implement both process resolution and execution.
Use an injected host for confirmed CBM account uninstall and native-first ordering.
E's real Windows cleanup uses `--yes` project-only removal.
`TestDataHome` redirects legacy stores and doesn't resolve Tool homes.

## Client bindings

E's committed Framework `Tools/Catalogue/Shared/CodebaseMemory/CodebaseMemoryNativeOutputs.cs` declares the bindings.
`Root`, `ConfigurationRootKey` and `Configuration` at lines 14-16 distinguish directory roots from exact config files.
Its built-in list at lines 41-49 and `CodebaseMemoryRegistryOutputs.cs:10-18` define 42 `<id>-config` keys.
The built-in IDs are:

`claude-code`, `codex`, `gemini`, `zed`, `opencode`, `antigravity`, `kilocode`, `vscode`, `cursor`, `windsurf`, `augment-auggie`, `openclaw`, `kiro`, `junie`, `hermes`, `openhands`, `cline`, `qwen`, `copilot-cli`, `factory-droid`, `crush`, `goose`, `mistral-vibe` and `grok`.

The registry IDs are:

`qoder`, `kimi`, `gitlab-duo`, `rovo-dev`, `amp`, `devin`, `tabnine`, `continue`, `visual-studio`, `trae`, `roo-code`, `amazon-q`, `codebuddy`, `ibm-bob-ide`, `ibm-bob-shell`, `pochi`, `sourcegraph-cody` and `omp`.

Each `-config` key binds an absolute file path, including a documented arbitrary native override.
It isn't necessarily a directory or its default filename.
Additional directory keys are `claude-code`, `claude-user`, `codex`, `kimi`, `kiro`, `hermes`, `qwen`, `copilot-cli`, `mistral-vibe`, `grok`, `devin`, `omp`, `vscode`, `shared-skills`, `cline-data` and `openclaw-workspace`.
`gitlab-duo-hooks` binds an exact hook config file.
`codebase-memory-cache` and `codebase-memory-runtime` bind report-only directories.
VS Code also needs `copilot-cli` for native shared context outputs.

Resolve these from the reviewed native environment overrides and existing-path choices before checking inventory.
E's `evidence/upstream-recipes.md`, Client outputs and Client bindings, gives those path rules.
C parses every native detected-client ID without limiting discovery to this map or to two clients.
B checks inventory against independently resolved reviewed outputs.
Root mustn't accept an arbitrary returned inventory path as its own binding.
These keys use the existing `ClientRoots` map and need no new model.

## Other lane inputs

These are earlier inspected signatures, pending checked integrated source:

| Lane | Earlier source connection                                                                                                                                                                                       | Remaining integration input                                                                                                                                                  |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A    | `ToolHostFactory.CreateVersions()`, `CreateProcesses()`, `CreateDownloads()`, `CreatePath(string resolvedShellHome, IToolBackupService backups, Func<bool> hasLiveConsent)`                                     | Final environment-taking construction entry, channel service, target and project-manager facts and reviewed private Node input                                               |
| A    | `ToolProcessHost(IReadOnlyDictionary<string, string?>? environment = null)`, `ToolHttpTransfer(HttpClient? client = null, IProgress<ToolTransferProgress>? progress = null)`                                    | Use captured injected environment. The earlier parameterless process factory used ambient fallback                                                                           |
| A    | `ToolPathHost(ToolPathEnvironment environment, string shellHome, IToolBackupService backups, Func<bool> hasLiveConsent)`                                                                                        | Keep supplied `IToolPathHost` in journeys. Production `ToolUserPathEnvironment` can write persistent user PATH                                                               |
| B    | `ToolBackupService(IToolProcessHost processes)`                                                                                                                                                                 | One shared operation-local backup instance                                                                                                                                   |
| B    | `ToolConfigService(IToolProcessHost processes, IToolBackupService backups, IToolNativePlanReader nativePlans)` and a fourth `Func<string, byte[]> readTemplate` overload                                        | One instance implements config and setup planning. Connect C's `ToolNativePlanReader`. Verify B's final normalized environment and template behavior from its checked return |
| D    | `ToolWritePlanner(ToolCatalogue catalogue, ToolWriteToolPlanner tools)`, `ToolWriteToolPlanner(IToolVersionService versions, IToolChannelService channels, IToolConfigService config, IToolSetupService setup)` | Final writer operations, leaf factories, result serialization and prompt adapters                                                                                            |
| D    | `ToolWriteRecordPublisher(IToolRecordService records)`, `ToolWriteNativeApplication(IToolProcessHost process, IToolBackupService backups)`                                                                      | Fresh record expectations and pending backup completion stay in D. Its earlier runtime input was empty and needs the reviewed A runtime collection                           |
| E    | `ToolCatalogueFactory.Create(IReadOnlyList<ToolPrivateNodeSpec> nodeRuntimes)`                                                                                                                                  | Factory is now committed and pure. Supply A's reviewed runtime collection. The catalogue's provisional shipping minimum isn't a version decision by F                        |
| C    | Final connections above                                                                                                                                                                                         | Correct C-R1 through C-R4 before integration. Home alignment, inventory parsing, native environment fields and deferred factories are present                                |

A handles channels and hosts.
B handles setup planning, templates, config and backups.
C handles codecs, native inventory and observations.
D sequences effects and completes backups.
F later wires Root and checks integrated behavior.

## Root files

These are the later integration files:

| File                                                                         | Required connection                                                                                                            |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Root `Composition/CliCompositionRoot.cs`                                     | Compose Tool, add help group, branch and six closed bindings                                                                   |
| Root `Composition/CliToolComposer.cs`, new                                   | Pure symbol construction and typed closure with deferred operation factories. Accept typed catalogue input for synthetic tests |
| Root `Composition/Models/CliToolComposition.cs`, new                         | One branch and six `ICliCommandBinding` values                                                                                 |
| Root `Composition/CliStandaloneComposer.cs`                                  | Pass deferred C workspace reader factories to Status and Doctor                                                                |
| Root `Composition/Models/CliCompositionInputs.cs` and `CliToolHostInputs.cs` | Consume existing injected Tool inputs. Further changes need an actual merged-signature requirement                             |
| Root `Hosting/CliHost.cs`                                                    | Keep process and terminal facts here and defer Tool-specific home and host resolution                                          |
| Operations `Tool/ToolBinding.cs` and `ToolDefinitions.cs`, new               | Common group following Library. Keep leaf binding behavior with C and D                                                        |
| Rendering `Tool/Shared/Help/ToolHelpSections.cs`, new                        | Group help, with final wording coordinated with E                                                                              |
| Root `Composition/CliReportBinding.cs`                                       | Reuse existing generic closure unchanged                                                                                       |

## Focused checks

Later tests belong to the existing Integration project.
These are planned targets, not tests changed or run in this pass:

| Test path                                                          | Focus                                                                                                                                                               |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Commands/Tool/ToolCompositionIntegrationTests.cs`, new            | Six leaves, injected services, bare group and no effects from reads or preview                                                                                      |
| `Commands/Tool/ToolCompositionIsolationIntegrationTests.cs`, new   | Throwing host sentinels on help, invalid inputs, rejected IDs and unrelated commands, with absent owned homes                                                       |
| `Commands/Tool/ToolGeneratedSerializationIntegrationTests.cs`, new | Concrete read and writer schema-3 graphs, ID ordering, preview, plan and outcomes with reflection disabled                                                          |
| `Commands/Tool/ToolWorkspaceCompositionIntegrationTests.cs`, new   | Owned Status and Doctor fixtures, once-only findings, missing or invalid records, no Tool state creation                                                            |
| `Shell/Presentation/Shared/Help/CommandHelpIntegrationTests.cs`    | Tool group and six pages. Replace the touched class's literal output checks with reviewed Imprint snapshots and keep semantic exit, filesystem and host-call checks |
| `Serialization/Schema3CommandIntegrationTests.cs`                  | Extend the explicit leaf matrix using production tokens                                                                                                             |
| `Architecture/LayerBoundaryTests.cs`                               | Recheck only if assertions change or integration exposes a new edge                                                                                                 |

Use an Integration-local Tool fixture and the existing typed `CliCoreApplicationAccess` helper.
Build the managed solution, then run only touched or new classes with actual counts and the required warning and skip gates.
Regenerate payload-driven fixtures and snapshots once after E's final wording.
C's existing synthetic baselines don't replace the later composed CLI checks.

## Preparation evidence

The earlier C review changed F's packet, this map and records-review.md. The current authorized slice adds only ToolHomes source, matching focused Integration tests and native-home-roots.md, plus updates to this map and packet.
The C review consumed existing logs and snapshots. The home-provider slice runs a managed build, its three new focused classes and required checks. Its own receipt records those results.
Tracked source outside the assigned new ToolHomes paths stays unchanged at the accepted base.
The historical shared-contract report and patch stay byte-identical.
No peer file, protected model, central Root file or parent Task changed.
No tool install, persistent PATH, registry or shell profile write ran.
No commit or integration ran.
The new review report and native-home receipt need the coordinator's later Index pass.
