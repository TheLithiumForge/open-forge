---
open-forge:
  description: Record the Root Tool graph, deterministic Integration checks and final capture gaps
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Tool composition

Tool composition is the Root connection that runs the checked Tool services through the existing CLI report pipeline.
For example, a synthetic Install uses supplied process, download and PATH hosts, writes owned settings and records, then returns one schema-3 document.

Status: Accepted for local implementation commit. Task 90 "Tool setup" (phase 3/3): milestone 3/7.

## Candidate

The [Root packet](../packets/lane-c-root.md) assigns this graph and its Integration checks to C.
The worktree is on checked core `885b17db3e56ceb6839ba23609e85a4380c75018`.
The coordinator authorized the fast-forward and implementation.
Both preflight Markdown files survived the fast-forward byte for byte.
Their hashes and copies are in [fast-forward evidence](../../../../../../artifacts/tool-root/fast-forward.json).
The original report is kept as `artifacts/tool-root/preflight-command-composition.md`.
The coordinator accepted the complete 36-file v3 candidate and authorized its local implementation commit.

The implementation connects these parts:

- One `tool` branch with List, Inspect, Status, Install, Update and Remove in that order.
- Deferred execution factories with typed requests and cancellation.
- One service graph and one shared backup service per selected execution.
- The actual Root plan, optional-choice, manager and PATH prompt adapters.
- Deferred Tool findings in workspace Status and Doctor exactly once.
- Normal typed host inputs for owned Integration captures.

`CliBuildVersion.InformationalVersion` gives the production and synthetic-test version fact, including its prerelease.
The global version option still reports Open Forge.
The [Tool design](../../../../crystallized/documents/cli/technical-designs/tool-setup.md#root-hosts) defines the host inputs and failure limits.
Existing typed composition, BCL operations and source-generated JSON cover this graph.
The implementation adds no dependency or exceptional platform mechanism.

## Checked inputs

The preflight read these immutable checkpoints:

| Input             | Checkpoint                                                                                  | Use                                                        |
| ----------------- | ------------------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| Initial Root base | `f2200252bfff42c2b260df7375b92a468cf63263`, tree `4ac0207e0f6deb51620f3f8cc118aa770ee3048d` | Preflight and typed inputs                                 |
| F                 | `3afe9b3ed29cba34aca4fbe1452ae36e073d358e`                                                  | Committed [Root map](root-integration.md) and provider API |
| B                 | `46834001af0470d5cff565e772339856e335fa52`, tree `7bf8d806f0a0ea910094f503dd5a118723d2011c` | Config, setup and concrete backup dependency               |
| C                 | `9f4b0a4b192bc3875b38dcc2d0dfd82be99f6158`                                                  | Records, grants, observations and read bindings            |
| D                 | `41ef63a7fc8987836e74315ea3450fe8bdd7273c`, tree `46378694b23dcddd3814b1288af44f6f9d175bd7` | Writer graph and typed interactions                        |
| A                 | `38cc6a791a0bd5b8d31565e5e78232ef4d97ddfc`, tree `91d2da956e67bb076fc213e29e557634ebd24e16` | Process, download, PATH and runtime factories              |

The B, D and A sources were read through local Git objects during preflight.
The implementation rechecked actual constructors on the checked combined core.
That core includes D's asynchronous entry correction and A's process cleanup correction.
The coordinator reports 70 D binding tests, eight A cleanup tests and a clean combined 590-case gate across 41 classes.
Those are coordinator results, separate from the local checks below.
The merged Framework path namespace is `Shared.Paths`.
F's separate reader correction `ffef620bca8ae506c611fc3f244e63a72ceae1ec` keeps its provider API unchanged.
The coordinator handles that separate merge.

The coordinator closed these choices:

- C-ROOT-ENTRY - use D's asynchronous request/token factory after pure catalogue validation.
- C-ROOT-ISOLATION - C can add the normal typed `CliHost` entry and forward owned inputs through affected shared captures.
- Catalogue injection - the typed `ToolCatalogue` argument on `CliToolComposer` covers synthetic composition. An additional input field isn't needed.
- Final captures - E/coordinator handles shipped help assertion migration and payload capture once after final wording.

No protected contract change is requested by this implementation.

## Construction graph

[CliToolComposer](../../../../../../src/cli/root/OpenForge.Cli/Composition/CliToolComposer.cs) constructs pure symbols and closes the six bindings with their existing cached presentations.
It stores execution factories without invoking them.
`ToolCatalogueFactory.Create(ToolHostFactory.CreateNodeRuntimes())` provides the production catalogue.
No home resolution, native config read, Tool probe or HTTP client runs during symbol construction.

After binding succeeds, Tool leaves create this graph.
Workspace Status and Doctor first check project declarations as F-P1 describes below:

| Part             | Actual construction                                                                                 | Connection                                             |
| ---------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------ |
| Environment      | Supplied `CliToolHostInputs.ProcessEnvironment`, otherwise one execution-time environment snapshot  | Same values reach process, download and home services  |
| Homes            | `ToolHomeProvider(environment, knownFolder, operatingSystem, inputs)`                               | One provider, with execution-local known-folder values |
| Processes        | Supplied `Processes`, otherwise `ToolHostFactory.CreateProcesses(environment, catalogue)`           | Tool resolution, Git checks and native effects         |
| Backups          | `ToolBackupService(processes)`                                                                      | One concrete instance shared throughout                |
| Records          | `ToolRecordService(catalogue, versions, backups)`                                                   | Constructed before channels                            |
| Downloads        | Supplied `Downloads`, otherwise `ToolHostFactory.CreateDownloads(resolvedUserHome, environment)`    | `resolvedUserHome` is the Node/npm native home         |
| PATH             | Supplied `Path`, otherwise `ToolHostFactory.CreatePath(resolvedShellHome, backups, hasLiveConsent)` | Observe, manual instructions, add and removal          |
| Channels         | `ToolHostFactory.CreateChannels(processes, downloads, backups, records, environment)`               | The same hosts, backups and records                    |
| Native inventory | `ToolNativePlanReader()`                                                                            | C's checked parser passed to B                         |
| Config/setup     | `ToolConfigService(processes, backups, nativePlans)`                                                | One instance implements both services                  |
| Reads            | `ToolObservationReader(channels, config, versions, path, recipeContext)`                            | Current detection, config and machine facts            |

`CreateDownloads` receives the resolved Node/npm user home so npm registry, trust and proxy reads use that home.
It doesn't receive the Open Forge home.
Supplied hosts stay in use and aren't disposed as Root-created resources.
There is no application-wide mutable service cache.

The exact new Root constructors and entries are:

```csharp
CliToolComposer.Compose(CliInteractionComposition interaction,
    ToolCatalogue catalogue, CliToolHostInputs? inputs = null)
ToolCompositionFactory(ToolCatalogue catalogue,
    CliInteractionComposition interaction, CliToolHostInputs? inputs)
ToolCompositionGraph(ToolCatalogue catalogue,
    CliToolHostInputs? inputs, Func<bool> hasLiveConsent)
ToolCompositionInteraction(CliPrompts prompts)
CliStandaloneComposer.Compose(CliInteractionComposition interaction,
    WorkspaceLockStoreRoot? lockStoreRoot,
    OperationalContributorCatalogue operationalContributors,
    PhysicalPathResolver physicalPathResolver,
    Func<ToolWorkspaceObservationReader> tools)
CliHost.RunAsync(string[] arguments, string currentDirectory,
    CliOutputWriters writers, CliCompositionInputs inputs,
    CancellationToken cancellationToken = default)
```

`Compose` returns `CliToolComposition`, containing its `CliRootBranch`, six `ICliCommandBinding` values and `Func<ToolWorkspaceObservationReader> CreateWorkspaceReader`.
`CliHost.RunAsync` returns `ValueTask<int>` through the existing application and report pipeline.
The factory entries are `CreateReadObserver()`, `CreateWorkspaceReader()`, `CreateInstallAsync(request, token)`, `CreateUpdateAsync(request, token)` and `CreateRemoveAsync(request, token)`.
The three writer entries return `ValueTask<ToolInstallOperation>`, `ValueTask<ToolUpdateOperation>` and `ValueTask<ToolRemoveOperation>` respectively.

The writer graph keeps D's existing stages:

```text
ToolWriteRecordPublisher(IToolRecordService)
ToolWriteToolPlanner(IToolVersionService, IToolChannelService, IToolConfigService, IToolSetupService)
ToolWritePlanner(ToolCatalogue, ToolWriteToolPlanner)
ToolWriteChannelApplication(IToolChannelService, IToolVersionService, ToolWriteRecordPublisher)
ToolWriteNativeApplication(IToolProcessHost, IToolBackupService, IToolSetupService)
ToolWriteConfigApplication(IToolConfigService, ToolWriteRecordPublisher)
ToolWriteOptionalApplication(IToolChannelService, IToolPathHost, ToolWriteRecordPublisher)
ToolWriteApplication(ToolWriteChannelApplication, ToolWriteNativeApplication,
    ToolWriteConfigApplication, ToolWriteOptionalApplication)
ToolWriteOperation(ToolWritePlanner, ToolWriteApplication, ToolWriteRecordPublisher,
    ToolWriteHostFacts, ToolWriteInteraction)
ToolInstallOperation(ToolWriteOperation)
ToolUpdateOperation(ToolWriteOperation)
ToolRemoveOperation(ToolWriteOperation)
```

B's config constructor takes concrete `ToolBackupService`.
Its existing fourth `Func<string, byte[]>` template-reader parameter isn't needed here.
Root passes C's existing `ToolNativePlanReader` and keeps native inventory paths separate from F's file and directory bindings.

## Deferred execution

The existing application handles help, version, global errors and leaf binding before invoking any operation factory.
The new Tool group has no domain operation.
Its six leaves remain outside `rootLeaves`.
The production Root adds the branch and setup help entry, then registers the leaves in their declared order.

The checked D signature is:

```csharp
ToolInstallBinding.CreateRequestBinding(ToolWriteSymbols symbols,
    Func<ToolWriteRequest, CancellationToken, ValueTask<ToolInstallOperation>> operationFactory,
    ToolCatalogue catalogue, IToolVersionService versions, CliHelpContent help)
```

Update and Remove have the same five parameters with their own typed operation.
Each binding returns `CliRequestBinding<ToolWriteRequest, ToolWriteResult>`.
D validates named IDs and channels from pure catalogue data before invoking the factory.
It awaits the request/token entry and returns typed expected failure and cancellation results.
The local Integration cases check those paths with zero native or state effects.

The factory reads settings for the bound workspace and passes `MainRoot` to `ToolWriteHostFacts`.
It obtains target facts through `ToolHostFactory.ObserveTargetAsync` and PATH facts through the supplied PATH host.
`ToolProjectManagerReader` checks ordinary owned lockfile presence without parsing package files.
It ignores a directory occupying a lockfile path.

Recipe context uses request workspace and main root, F's Open Forge and native homes, independent client bindings and declared native environment values.
It derives the existing install directory from the verified detected command path, matching A's reuse plan.
Unavailable home policies stay absent so D can block a selected profile that needs one.
No fallback turns inventory output into a client binding.
Raw environment values stay out of read reports.
The CodeGraph Integration case checks declared native environment normalization and privacy.

## Prompts and findings

`ToolCompositionInteraction` connects these actual prompt adapters:

| Field             | Adapter                                                                                                                  |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `CanPrompt`       | `prompts.CanPrompt(new CliPromptPolicy(true))`                                                                           |
| `ConfirmPlan`     | `PlanConfirmation<ToolWriteResult, ToolWriteData, ToolWriteConsentQuestion>` with the writer rendering and plan question |
| `ConfirmOptional` | `SelectAsync` with `ToolWriteInteractionPresentation.OptionalQuestion(question)`                                         |
| `ChooseManager`   | `SelectAsync` with `ToolWriteInteractionPresentation.ManagerQuestion(question)`                                          |
| `ChoosePath`      | `ToolWriteInteractionPresentation.ChoosePathAsync`                                                                       |

Optional decline is `Answered(false)`, so D skips account uninstall and continues project cleanup.
The execution-local PATH consent flag is set only by an answered live PATH choice.
General terminal availability and plan consent don't set it.
JSON, `--yes` and missing terminals keep the existing D prompt policy.

`CliStandaloneComposer` passes one deferred `Func<ToolWorkspaceObservationReader>` to each existing Status and Doctor operation.
The existing operational contributors, physical resolver and rename reader stay in place.
Root adds no second appender or Tool Repair action.
Full-Root tests exposed the missing Tool category in `DoctorWording.Category`.
That Tool-only presentation branch now uses existing `DoctorToolText.Title()`.

## Test isolation

The synthetic catalogue enters through the typed `CliToolComposer` argument.
Tests use the real command tree, application, prompt adapters, services and generated renderers.
Separate full-Root cases use the production catalogue for registration, inert invocation and workspace findings.

`CliHost` now accepts ordinary `CliCompositionInputs` through its typed overload.
`CliHostCapture`, `ReadCommandOutputCapture` and the workspace-journey capture forward owned `CliToolHostInputs`.
`CliToolIsolation` provides an absent owned Open Forge home, owned native home, empty environment and supplied process, download and PATH hosts.
It resolves no Tool command and throws on effects or network requests.
Existing Status and Doctor adapters receive these inputs through their capture.
There is no ambient override, test mode or fake version fact.

The owned Git case runs Git initialization and coverage probes only in its temporary repository through A's real process host.
Its HOME, USERPROFILE and global Git config point to owned paths.
Tool probes, native effects, downloads and PATH operations use supplied doubles.
Synthetic download bytes are inert and never execute.
All file effects stay in owned temporary paths.

## Focused evidence

This 113-case evidence records the frozen candidate accepted before the version-minimum correction below.
The bounded correction reruns only its affected seven-case interaction class.

The dependency restore is `npm ci`, exit zero, recorded in `artifacts/tool-root/npm-ci.txt`.
The managed Release build uses this exact invocation:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The final build exits zero with zero warnings and zero errors in 1 minute 58.96 seconds.
Its log is `artifacts/tool-root/build-checked.txt`.
The audit override is invocation-only.
Tests run only the touched or created classes with the command list in `artifacts/tool-root/focused-commands.txt`.
Every command uses `--configuration Release --no-build --no-restore`, `--fail-warns on --fail-skips on --no-ansi --progress off` and its actual minimum count.
The final candidate passes 113 cases across six classes, with zero warnings and zero skips:

| Qualified class after `OpenForge.Cli.IntegrationTests.`           | Minimum and actual count | Result                         | Log in `artifacts/tool-root/`                            |
| ----------------------------------------------------------------- | ------------------------ | ------------------------------ | -------------------------------------------------------- |
| `Commands.Tool.ToolCompositionIntegrationTests`                   | 18                       | Pass                           | `ToolCompositionIntegrationTests-checked.txt`            |
| `Commands.Tool.ToolCompositionIsolationIntegrationTests`          | 37                       | Pass                           | `ToolCompositionIsolationIntegrationTests-checked.txt`   |
| `Commands.Tool.ToolCompositionInteractionIntegrationTests`        | 7                        | Pass                           | `ToolCompositionInteractionIntegrationTests-checked.txt` |
| `Commands.Tool.ToolGeneratedSerializationIntegrationTests`        | 7                        | Pass                           | `ToolGeneratedSerializationIntegrationTests-checked.txt` |
| `Commands.Tool.ToolWorkspaceCompositionIntegrationTests`          | 8                        | Pass                           | `ToolWorkspaceCompositionIntegrationTests-checked.txt`   |
| `Serialization.Schema3CommandIntegrationTests`                    | 36                       | Pass                           | `Schema3CommandIntegrationTests-checked.txt`             |
| `Commands.Shared.Scenarios.WorkspaceShapeJourneyIntegrationTests` | 2                        | Fail, deferred payload cleanup | `WorkspaceShapeJourneyIntegrationTests-first.txt`        |

The 18 behavior cases cover registration, three reads, three previews, full writer publication, supplied synthetic download, exact restore, grants kept after blocking, pending native backups, owned Git coverage, exact client-file binding, invalid native inventory, typed factory failure/cancellation and ordinary lockfile hints.
The 37 isolation cases cover both full Root and typed Tool composition with invalid home and throwing-host sentinels, plus three unrelated read commands.
The seven interaction cases check account approval and decline, unattended policies, cancellation, no terminal and actual typed manager/PATH answers.
The seven serialization cases check all six leaves at all four details and production CodeGraph environment privacy.
The eight workspace cases check Status and Doctor once each for missing, drifted, invalid and unsupported facts.
The schema class adds six Tool commands to its existing 30 cases.
Only the two deferred payload-fixture cases fail. Their failure is described under Remaining work.

The exact command for each selected class, including minimum counts and flags, is kept in [focused commands](../../../../../../artifacts/tool-root/focused-commands.txt).
The shared generated test assembly is `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`.
Its hash and those of each supporting product assembly are in `artifacts/tool-root/assembly-sha256.txt`.
Final candidate paths and hashes are in `artifacts/tool-root/changed-files.txt` and `artifacts/tool-root/source-sha256.txt`.
Check and log hashes are in `artifacts/tool-root/check-sha256.txt`.

The mandatory format commands are:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Both mandatory .NET commands exit zero on the final source candidate.
Their logs are `artifacts/tool-root/whitespace-checked.txt` and `artifacts/tool-root/style-checked.txt`.
Targeted `dotnet format whitespace --include` also exits zero before the final build.
The 34-file candidate contains 30 C# files, two Markdown files and two new snapshot files.
The complete [file manifest](../../../../../../artifacts/tool-root/changed-files.txt) includes nine modified files and 25 new files.
The assigned-path check confirms that Framework models, contracts, schemas, actual workspace settings/lock, A/F private hosts and other packets are unchanged.
The packet before Progress matches HEAD byte for byte.
The index is empty and HEAD is still the exact checked core SHA.

Markdown formatting, local-link checks, strict UTF-8/LF, no BOM, source/Markdown final newline, all-file trailing-whitespace and Git whitespace checks are recorded in `artifacts/tool-root/candidate-validation.txt`.
Snapshot end-of-file bytes match the reviewed Imprint received files, including their lack of a terminal newline.
No production source changes during the version-minimum correction below.

## Snapshot review

Only two new synthetic account-choice transcripts are accepted here.
They cover approval and decline through actual Root prompts.
Semantic assertions separately check native uninstall count, native-first order and project cleanup.
The entire transcript is normalized for owned paths, workspace hash, line endings and platform separators.
A concrete source-generated JSON string array stores every line, including trailing spaces and the final empty line.
This keeps the full transcript while avoiding trailing whitespace in the snapshot file.

The new baselines are below `Commands/Tool/__snapshots__/AccountChoice/`:

- `approved.json/approved.json.txt`.
- `declined.json/declined.json.txt`.

The reviewed received files and their `run.json` manifests are under `artifacts/command-output-snapshots/dbd80a80307048af87f82b9fb74ee30c/AccountChoice/`.
The review checked the plan, account-wide question, native PATH effect wording, both choices and the final selection prompt.
Both new baselines pass verification in the seven-case interaction class.
Their hashes are in `artifacts/tool-root/snapshot-sha256.txt`.
Existing six C read baselines and D synthetic writer baselines are unchanged.

## Version minimum

The synthetic catalogue uses one fixture-owned `SupportedOpenForgeMinimumVersion = "0.1.0"` constant.
Its profile minimum no longer follows the current release build.
Production `ToolWriteHostFacts.OpenForgeVersion` and process identity still use the actual `CliBuildVersion.InformationalVersion`.
The correction changes one fixture source, the two synthetic account-choice baselines and this receipt.
All 34 candidate files stay present.
The other 30 files, including every product file and the complete packet, match the frozen reviewed bytes.

The snapshot review checks every transcript line.
Only the displayed minimum changes from `0.9.0-beta.18` to `0.1.0`.
The received captures are under `artifacts/command-output-snapshots/eee47b82aa4b48e3bece7706f3746a33/AccountChoice/`.
Their review diff is `artifacts/tool-root/version-floor-correction/snapshot-review.diff`.
No other output baseline or payload fixture changes.

The exact build and seven-case test commands are in `artifacts/tool-root/version-floor-correction/commands.txt`.
The prospective build adds only the normal MSBuild property `-p:OpenForgeCliVersion=0.9.0-beta.19` to the managed Release build invocation above.
It doesn't change a release file or inject a fake host version.
Build-version generated sources and matching Root/Integration assembly hashes are retained for each stage.

The same seven-case interaction class passes with both actual build versions:

| Stage           | Actual generated build version | Build result         | Interaction cases | Logs under `artifacts/tool-root/version-floor-correction/` |
| --------------- | ------------------------------ | -------------------- | ----------------- | ---------------------------------------------------------- |
| Normal          | `0.9.0-beta.18`                | Zero warnings/errors | 7/7 pass          | `build-normal.txt`, `interaction-normal.txt`               |
| Prospective     | `0.9.0-beta.19`                | Zero warnings/errors | 7/7 pass          | `build-beta19.txt`, `interaction-beta19.txt`               |
| Restored normal | `0.9.0-beta.18`                | Zero warnings/errors | 7/7 pass          | `build-restored.txt`, `interaction-restored.txt`           |

The initial normal verification produced exactly two expected snapshot mismatches before those reviewed baselines were updated.
Its other five cases passed.
That review run is `interaction-normal-received.txt` in the same artifact directory.
Later runs use snapshot verification mode.

The corrected source hashes are:

| File                                  | Previous SHA-256                                                   | Corrected SHA-256                                                  |
| ------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------ |
| `ToolCompositionFixture.Catalogue.cs` | `3c494cae468aeab473cc00aab56b23e4bb810cf0a7cdedead2f14a4e7659fac1` | `bede74d5cbba416ee82f27f5c76e92f0803316e1b0d358ea58fbdd2c4601e6d7` |
| Each account-choice baseline          | `55dad19ad43bb6d1b03e94118bd31760914887e65a5875aeaa6e22b0096dfd84` | `b3b78fb7e587c276275c03b96476e586881c1d85bf2977a30d516aeafcfaf9d0` |

The original 34-file source manifest and exact bytes are kept in `artifacts/tool-root/version-floor-correction/before-source-sha256.txt` and its `before/` tree.
The updated complete manifests stay in `artifacts/tool-root/changed-files.txt` and `source-sha256.txt`.
All three builds exit zero with zero warnings and zero errors.
The normal, prospective and restored builds take 2 minutes 11.31 seconds, 2 minutes 43.01 seconds and 1 minute 54.29 seconds respectively.
Each seven-case run has zero failures, warnings and skips.
Normal artifacts are restored to the actual `0.9.0-beta.18` build.
The same two snapshot hashes hold through all three verification runs.
Both mandatory final format checks exit zero on the corrected source.
Their logs are `artifacts/tool-root/version-floor-correction/whitespace.txt` and `style.txt`.
The final comparison and receipt hashes are recorded under `artifacts/tool-root/version-floor-correction/`.

## Undeclared workspaces

F-P1 closes the no-tools path diagnosed in F's read-only `published-tool-read-isolation.md` report in the integrator worktree.
Workspace Status and Doctor now check project eligibility before Tool graph construction.
`ToolWorkspaceObservationReader.ReadAsync` resolves only the project path and reads its settings through the existing no-follow `WorkspaceSettingsReader`.
It passes that snapshot through existing `ToolRecordJson.Read` and `ToolPortableCodec.ReadSettings`.
Absent settings or a valid empty declaration return no Tool observations.
The eligibility check doesn't read the lock, a home, machine files or native settings.
Invalid or unavailable project settings fail before graph construction.
The workspace commands retain their existing unavailable-finding policy for those project failures.

The new Framework entry is:

```text
ToolWorkspaceObservationReader(IToolVersionService versions,
    Func<ToolWorkspaceObservationReader> createReader)
```

The existing four-parameter concrete constructor stays available.
Both entries check project eligibility before their record/observation delegate.
The deferred entry invokes the concrete reader's declared-observation delegate directly after eligibility, so it doesn't read eligibility twice.
`ToolCompositionFactory.CreateWorkspaceReader()` returns this deferred entry with the pure version service.
Its graph, home provider and `HomePath` access are inside the deferred factory.
A nonempty declaration invokes that factory once, then uses the unchanged combined record reader and per-tool observation loop.
No frozen Model or Service contract changes are needed.

The correction changes five files relative to accepted v2:

- Framework `Shared/Observation/ToolWorkspaceObservationReader.cs`.
- Root `Shared/ToolComposition/ToolCompositionFactory.cs`.
- Existing `ToolWorkspaceCompositionIntegrationTests.cs`.
- New `ToolWorkspaceObservationReaderTests.cs`.
- This receipt.

All 34 accepted v2 files stay present.
Thirty-one of them, including the synthetic version-minimum fixture, both account-choice baselines and the complete packet, match their accepted hashes.
The two additional files make the complete candidate 36 files, with 10 modified files and 26 new files.
The correction diff and five-file before/after hashes are `artifacts/tool-root/no-tools-isolation/correction.diff` and `delta-sha256.txt`.
Ordinary Tool List, Inspect and Status observers are unchanged.
Production running-version facts stay on `CliBuildVersion.InformationalVersion`.
The cancelled E-J3 Inspect binding and its tests are untouched.
E's help, B's config, F's home policies, Settings/Ownership readers and payload inventories are unchanged.

The original F-P1 commands remain in `artifacts/tool-root/no-tools-isolation/commands.txt` as prior evidence.
The filesystem eligibility class now belongs to Integration.
Its accepted-tier command is in `artifacts/tool-root/eligibility-tier-correction/commands.txt`.
The 18 Root cases keep their accepted source receipt and aren't rerun for this tier correction:

| Class                                                                                                   | Actual and minimum count | Result              | Artifact                                                          |
| ------------------------------------------------------------------------------------------------------- | ------------------------ | ------------------- | ----------------------------------------------------------------- |
| `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Observation.ToolWorkspaceObservationReaderTests` | 11                       | Exit 0, Integration | `artifacts/tool-root/eligibility-tier-correction/integration.txt` |
| `OpenForge.Cli.IntegrationTests.Commands.Tool.ToolWorkspaceCompositionIntegrationTests`                 | 18                       | Exit 0              | `artifacts/tool-root/no-tools-isolation/integration.txt`          |

The 11 Integration eligibility cases check six undeclared project shapes, one declared factory entry, invalid project JSON, two malformed declarations and cancellation.
They assert factory call counts with owned temporary files and a throwing construction delegate.
The 18 actual-Root cases check eight undeclared Status/Doctor combinations with invalid home and throwing process, download and PATH sentinels.
They require no Tool findings, no host calls, no created home and unchanged owned files.
Two declared invalid-home cases check that eligibility still reaches home observation once.
The existing eight cases still check missing, drifted, invalid and unsupported declared-tool findings exactly once.
The 18 Root cases passed with zero skips and the required fail-warns/fail-skips flags.
Their unchanged source SHA-256 is `74638119ec65a07ab70e7f4fe12bd91a900b26c279acdd2b22222f72e39dad17`.
The earlier 11-case Unit run stays in `artifacts/tool-root/no-tools-isolation/unit.txt` with its exit receipt.
That run is prior evidence, not the accepted tier, because `TemporaryWorkspace`, `CreateFile` and the real settings reader cross the filesystem limit.
No synthetic snapshot is affected or regenerated.

The original F-P1 managed Release build exits 0 with zero warnings and zero errors in 1 minute 41.67 seconds.
Its log is `artifacts/tool-root/no-tools-isolation/build.txt`.
The first compile required converting immutable snapshot bytes to the codec's existing memory input.
The initial Integration run passed 17 cases and exposed one new Doctor assertion at default detail.
That assertion now requests full detail, matching the existing declared-tool regression cases.
The earlier logs remain in the same artifact directory.
No production behavior changed for that test adjustment.

Both original F-P1 formatter commands exit 0.
Their exact exit codes are captured in `artifacts/tool-root/no-tools-isolation/whitespace-exit.json` and `style-exit.json`, with matching text logs.
Current complete source hashes, changed-file paths and assembly hashes stay in `artifacts/tool-root/`.
The accepted v2 source manifest and exact before bytes are retained under `artifacts/tool-root/no-tools-isolation/` for the bounded comparison.

### Evidence tier

`ToolWorkspaceObservationReaderTests.cs` moves from Unit to `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Tools/Shared/Observation/`.
Its namespace is `OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Observation` and all five Evidence traits are `Integration`.
Reversing only those namespace and trait changes reproduces the exact prior class bytes.
All eleven cases and production bytes are unchanged.
The old uncommitted Unit class is absent.
The candidate still contains 36 files, with 10 modified files and 26 new files.
The complete source manifest changes only that test path and this receipt.
The other 34 files match their accepted hashes.

The moved eleven-case Integration class is the only test class selected in this correction.
The managed Release build exits 0 with zero warnings and zero errors in 2 minutes 14.40 seconds.
The moved Integration class passes all 11 cases with zero failures and skips and the required strict warning/skip flags.
Its command returns exit 0.
Both mandatory final-source commands, `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`, exit 0.
Build, test and formatter exit receipts and matching logs are in `artifacts/tool-root/eligibility-tier-correction/`.
The old Unit run and the accepted 18-case Root receipt remain untouched.
The bounded move diff, before/after hashes and complete current manifest are recorded in the same artifact directory and `artifacts/tool-root/source-sha256.txt`.

Required after the final payload inventory correction: `OpenForge.Cli.EndToEndTests.PublishedInstallProcessTests.LegacyFilesAreNeitherReadNorDeleted`.
It exercises Status and Doctor through the published executable and hasn't run in this correction.
The owned Integration checks establish no-store reachability at the actual Root boundary.
The published process check remains a required final gate.
Declared-tool published tests still need owned process/home isolation in their assigned journeys.
This correction adds no environment flag, test mode, public option, published launcher protocol, global mutable cache or storage-home setting.

## Remaining work

The checked core supplies all production contracts needed here.
F's separate native JSON-like reader correction is coordinated separately and keeps the same provider API.
Its merged completion is needed before final native path parity acceptance and E's affected journeys.

The existing `Commands.Shared.Scenarios.WorkspaceShapeJourneyIntegrationTests` has two failing cleanup cases.
Its `InstallOperationWorkspace.EmbeddedPayloadPaths` inventory omits shipped `.agents/skills/open-forge-cli/references/tools.md`.
The two owned fixture directories left that file after cleanup, which raised "The temporary workspace contains an unowned entry."
The exact failing run is `artifacts/tool-root/WorkspaceShapeJourneyIntegrationTests-first.txt`.
This cleanup exception can hide earlier case assertions, so these cases aren't reported as passing.
The protected payload inventory stays with the coordinator's final payload pass.
The unchanged failure wasn't rerun without that correction.

E/coordinator also handles root-help capture, Tool group help and all six leaf help pages after final wording.
The seven added help cases are `tool`, `tool list`, `tool inspect`, `tool status`, `tool install`, `tool update` and `tool remove`.
The existing help-class literal assertion migration goes with that final pass, as approved.
No shipped payload fixture or existing output baseline is regenerated here.

The report is a new routed file for the coordinator's one Index pass.
Only this packet's Progress and this report change in working Memory.
Full suites, native builds, real Tool or network journeys, real stores and persistent platform changes aren't run in this lane.
The maintainer authorized the local implementation commit. CI runs every test.

## Commit checks

The maintainer accepted the complete v3 candidate and authorized its local implementation commit using real timestamps.
This continuation runs no local tests and regenerates no snapshots.
Earlier test results stay as historical evidence.
CI handles all tests, including the outstanding published-process and payload-fixture checks.

The precommit commands are:

```text
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The managed Release build exits 0 with zero warnings and zero errors in 2 minutes 18.35 seconds.
The whitespace command exits 0.
The style command exits 0.
The invocation-only NuGet audit override changes no repository file.
Exact commands, logs and exit receipts are in `artifacts/tool-root/commit-continuation/`.

The commit contains the complete 36-file candidate, including this receipt and the packet.
All production, test and snapshot bytes match accepted v3.
Only this receipt and the packet's Progress change for closeout.
The full paths and SHA-256 values are `artifacts/tool-root/changed-files.txt` and `artifacts/tool-root/source-sha256.txt`.
Git whitespace and UTF-8/LF checks pass on those files.
The local commit adds no attribution trailer.
No push, merge, GitHub mutation, actual Tool journey or persistent PATH, registry or profile change runs in this continuation.
