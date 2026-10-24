---
open-forge:
  description: The frozen Delivery 1 plan for Task 85, with the layout API, settings v4, seven waves of packets, qualification, documentation and the Delivery 2 outline
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Layout, Install, Settings, Plan]
---

# Configurable Main Root Plan

The test and qualification engineer of the [main root council](record.md) wrote this plan from the chair decisions. It is frozen for Delivery 1 of [Task 85](../../../working/cli-development/tasks/task85-configurable-main-root.md) and starts after [Task 86](../../../working/cli-development/tasks/task86-workspace-runtime-lock.md) merges. It replaces the [2026-10-21 plan](../route-include-council/main-root-plan.md).

## Chair amendments

These override the plan below where they differ.

1. **No magic strings and no literal-string assertions.** The maintainer set this on 2026-10-24. A folder or file name, finding code, environment variable or layout segment is referenced through its production owner, in tests as in production code. No test asserts user-facing text with a literal, substring or pattern. Exact text is checked only through reviewed snapshots. Semantic facts stay direct assertions. See `.agents/directives/open-forge/testing/evidence-integrity.md`.
2. **Clean test tiers.** Unit tests stay in memory. Integration tests cross one real boundary in process. EndToEnd tests drive the published executable and do not repeat lower-tier branches. Traits match what each test really does.
3. **Routes outside the main root are Task 87.** The maintainer decided on 2026-10-24 that including a folder makes it a maintained route outside the main root, a separate task after this delivery. See the [multiple roots council](../multiple-roots-council/record.md). This delivery needs no API change for it, only these clarifications in Wave 0 and Wave 1 acceptance: `SourceRoots` lists the primary content locations and never implies loading membership or unrestricted write authority. `SourcePath`, `CategoryDirectory` and `MapPayloadPath` stay primary-layout mappings. `IsImplicitContentDestination` stays separate from any later outside-route permission. Catalogue reconstruction keeps included `./path` identities, aliases and conditions.
4. **Test corrections.** Packet Q selects the real Route Include and Exclude classes and the shared inclusion scope, and records discovered and executed counts. Prose assertions in existing Include tests and the published no-reactivation journey become reviewed snapshots or parsed structure, and moving prose into constants does not count. Pure classification stays in Unit and filesystem observation in Integration, and an internal composition checkpoint is not published EndToEnd evidence. The published journeys prove journeys while command branches stay in Integration. EndToEnd and test support get a small tracked build-time projection of the owned public tokens they need instead of copied constants. File preservation keeps byte comparisons because snapshots ignore line endings.
5. **A layout read keeps its layout whenever settings supply one.** Decided during Wave 4. `WorkspaceLayoutReader` returns the layout with valid settings even when ownership is unreadable, an anchor is unsafe, the loader is missing or conflicting, or the layout changed during observation. The state and issues still stop every writer. Only invalid or unavailable settings leave the layout empty, and then read-only observers keep every domain, report layout-dependent ones unavailable with the settings issue as the cause, and never assume `.agents`.
6. **A loader the settings record as removed is not missing.** Decided during Wave 5. The reader raises `MissingLoader` only when the selected loader is missing despite installation evidence and the settings do not record it as removed, so an intentionally removed loader never blocks writers.

Delivery 1 adds a context folder selected at first installation and supports its complete lifecycle. It does not move an established installation. An omitted or unchanged `--main-root` preserves ordinary Install behavior. A different root refuses before effects, including with `--configure`.

Implement after Task 86 merges. The baseline citations below refer to the supplied snapshot. Reconcile moved lines against Task 86 before dispatch, preserving the APIs and decisions in this plan. Task 85 remains open after Delivery 1.

No implementation, build or test execution was performed while preparing this plan.

## Ownership, paths and gates

| Prefix | Repository directory |
|---|---|
| F | `src/cli/framework/OpenForge.Cli.Framework/Framework` |
| O | `src/cli/operations/OpenForge.Cli.Operations/Commands` |
| R | `src/cli/rendering/OpenForge.Cli.Rendering/Presentation` |
| T | `src/cli/output-text/OpenForge.Cli.OutputText` |
| H | `src/cli/root/OpenForge.Cli/Composition` |
| SH | `src/cli/shell/OpenForge.Cli.Shell/Shell` |
| U | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests` |
| I | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests` |
| E | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests` |
| C | `.agents/memory/crystallized/documents/cli` |

Braces below enumerate exact alternatives. They are not open-ended directory ownership.

One foundation reasoning owner owns the frozen API and its integration. Implementers report API problems to that owner rather than adding alternate resolvers or changing signatures. Every packet owns its corresponding tests and snapshots. A production change and its proving tests integrate together.

Each wave starts from the preceding accepted integration. Each packet must compile independently. Overlapping packets run sequentially. A failed integration is corrected or withdrawn before another wave starts.

Temporary existing-signature adapters may delegate to the new implementation using an explicit default layout. They contain no duplicate policy. Record every adapter when introduced. Remove them and convert their test callers before public activation.

## Build in the sandbox

Paste this section into every packet.

The worker is Codex in a Windows `workspace-write` sandbox. It cannot write outside its worktree, cannot commit, and must leave its diff uncommitted. Read `.agents/loader.md` and applicable instructions. Other workers are active in separate worktrees. Do not revert their changes during integration.

```powershell
$env:APPDATA = Join-Path (Get-Location) 'artifacts\nuget-appdata'
New-Item -ItemType Directory -Force (Join-Path $env:APPDATA 'NuGet')
$env:OPENFORGE_DATA_HOME = Join-Path (Get-Location) 'artifacts\data-home'
New-Item -ItemType Directory -Force $env:OPENFORGE_DATA_HOME
$env:TEMP = Join-Path (Get-Location) 'artifacts\tmp'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Force $env:TEMP

dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config
$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
dotnet build OpenForge.Cli.slnx -c Release --disable-build-servers -m:1 -p:OpenForgeSkipDevelopmentPublish=true
```

Use these focused-test commands, abbreviated below as `tu`, `ti` and `te`.

```powershell
function tu($class) {
    dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build -- --filter-class $class
}
function ti($class) {
    dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build -- --filter-class $class
}
function te($class) {
    dotnet run --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build -- --filter-class $class
}
```

`OPENFORGE_DATA_HOME` alone does not relocate test initialization. `TestDataHome.Create` uses the runtime temporary directory at `src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs:94`. The TEMP/TMP override follows Task 86’s amendment.

Tests that still require writes outside the worktree, unavailable filesystem capabilities or native tooling go to the overseer. Report the exact command and missing capability. Do not weaken or skip their assertions.

The build above does not refresh the development CLI publish. The overseer must produce a fresh executable before end-to-end execution. Receipts record source identity, command, runtime, executed counts, failures, skips and artifact identity.

## Frozen foundation API

All additions are internal. Existing namespaces retain their current `OpenForge.Cli.Core` prefix.

### Pure layout

Add `F/Workspace/Models/Layout/WorkspaceLayout.cs` and `WorkspaceMainRootPath.cs`.

```text
WorkspaceMainRootPath.IsCanonical(string? value) → bool

WorkspaceLayout.Create(CliWorkspace workspace, string mainRoot) → WorkspaceLayout

WorkspaceLayout.Workspace → CliWorkspace
WorkspaceLayout.MainRoot → string
WorkspaceLayout.LoaderPath → string
WorkspaceLayout.HumanGuidePath → string
WorkspaceLayout.SourceRoots → ImmutableArray<string>
WorkspaceLayout.IsDefault → bool

WorkspaceLayout.CategoryDirectory(string category) → string
WorkspaceLayout.SourcePath(string sourceRelativePath) → string
WorkspaceLayout.MapPayloadPath(string packagePath) → string
WorkspaceLayout.TryGetSourceRelativePath(string path, out string relativePath) → bool
WorkspaceLayout.IsContentPath(string path) → bool
WorkspaceLayout.IsImplicitContentDestination(string path) → bool
```

These methods perform no filesystem or settings reads.

`SourceRoots` is `[.agents]` for the default layout. Otherwise it contains the main root and `.agents/skills`. An absent native Skills directory is optional. An inaccessible or unsafe existing Skills directory remains an observation failure.

`SourcePath("skills/...")` stays under `.agents/skills`. Other source-relative paths map under `MainRoot`. A competing custom-root `skills` category is invalid.

`MapPayloadPath` preserves host filenames, fixed controls and native Skills. It maps other `.agents/` package destinations once. It never redirects literal command operands.

Reuse `PortableWorkspacePath.TryNormalize` and `CreatePortableKey` from `F/Filesystem/Shared/Paths/PortableWorkspacePath.cs:13`. Reject noncanonical settings values, traversal, absolute paths, Git metadata, the workspace root, reserved runtime paths and `.agents` descendants. Exactly `.agents` remains valid.

Membership does not grant mutation permission. Existing overwrite, ownership, containment and control-file restrictions still apply.

### Settings and observation

Add `string? DeclaredMainRoot { get, init }` and computed `string MainRoot` to `WorkspaceSettingsDocument`. Absence resolves to `.agents`.

Add `WorkspaceSettingsCodec.MainRoot.cs`:

```text
WorkspaceSettingsCodec.SetMainRoot(
    ReadOnlyMemory<byte> existing,
    string mainRoot) → byte[]?
```

Return null when the effective root is unchanged. Never insert an absent default value. Creating settings emits v4. Explicitly selecting a custom root may update declarations to v4. Unrelated rewrites preserve existing declarations, unknown values and property order.

Change `IsValidRootPath` to accept the effective main root. `Read`, `AddRoot` and `RemoveRoot` validate registrations against both content locations. Keep their existing public-internal call shapes where the existing bytes already supply the root.

Add:

- `F/Settings/Models/Observation/WorkspaceLayoutRead.cs`
- `F/Settings/Models/Observation/WorkspaceLayoutIssue.cs`
- `F/Settings/Shared/Observation/WorkspaceLayoutReader.cs`
- `F/Settings/Shared/Mutation/WorkspaceLayoutRevalidator.cs`

```text
WorkspaceLayoutReader(PhysicalPathResolver paths)

ReadAsync(
    CliWorkspace workspace,
    CancellationToken cancellationToken) → ValueTask<WorkspaceLayoutRead>

WorkspaceLayoutRead.State → WorkspaceLayoutReadState
WorkspaceLayoutRead.Settings → WorkspaceSettingsRead
WorkspaceLayoutRead.Ownership → WorkspaceOwnershipRead
WorkspaceLayoutRead.Layout → WorkspaceLayout?
WorkspaceLayoutRead.Issues → ImmutableArray<WorkspaceLayoutIssue>
WorkspaceLayoutRead.Expectations → ImmutableArray<FileExpectation>

WorkspaceLayoutReadState:
    Ready, Uninstalled, Mismatch, Invalid, Unavailable

WorkspaceLayoutIssue:
    Code, Path, Cause, RelatedPaths

WorkspaceLayoutRevalidator.ValidateAsync(
    WorkspaceLockLease lease,
    WorkspaceLayoutRead observation,
    CancellationToken cancellationToken) → ValueTask<MutationValidationResult>
```

Issue codes are `InvalidSetting`, `UnsafeRoot`, `MissingLoader`, `ConflictingLoader`, `OwnershipMismatch`, `PointerMismatch` and `ObservationUnavailable`.

The reader observes fixed settings, ownership, selected loader, known default loader and recognized managed host pointers. It does not search arbitrary directories for another installation. Missing optional hosts remain lifecycle observations rather than invented conflicting-root evidence.

`Uninstalled` requires no positive installation evidence. A well-formed selected layout may accompany `Mismatch`, allowing exact diagnostics. Invalid settings never produce a usable fallback layout.

Capture file and ancestry expectations. Revalidation uses the held physical-workspace lease and existing `FileExpectationValidator`. It never silently selects a replacement layout.

### Source and plan transport

Freeze these changed signatures:

```text
SourceCatalogueRequest(
    WorkspaceLayout layout,
    IEnumerable<string> logicalRoots)

SourceCatalogue(
    WorkspaceLayout layout,
    IEnumerable<SourceCandidate> candidates,
    IEnumerable<SourceLogicalSource> sources,
    IEnumerable<SourceCatalogueIssue> issues,
    bool isCancelled,
    IReadOnlyDictionary<string, string>? aliases = null)

SourceReadSessionRead(
    WorkspaceLayout layout,
    CancellationToken cancellationToken) → ValueTask<SourceReadSession>

SourceReadSessionReader.ReadAsync(
    WorkspaceLayout layout,
    CancellationToken cancellationToken) → ValueTask<SourceReadSession>

SourceReadSessionReader.ReadIncludedAsync(
    WorkspaceLayout layout,
    CancellationToken cancellationToken) → ValueTask<SourceReadSession>

SourceReadSession(
    SourceCatalogue catalogue,
    SourceDocumentReader documentReader,
    IReadOnlyList<SourceCatalogueSelectionScope> defaultSelectionScopes)

SourceUniverseFilterRequest(
    SourceCatalogue catalogue,
    IReadOnlyList<SourceCatalogueSelectionScope> defaultSelectionScopes,
    IEnumerable<SourceUniverseSelectorOccurrence> occurrences)
```

Catalogue/request expose `Layout`. Existing `Workspace` properties delegate to it. Sessions expose `Layout` through their catalogue and replace `DefaultSelectionScope` with `DefaultSelectionScopes`.

Add the leading `WorkspaceLayout layout` argument to root-sensitive `SourceIdentity`, `SourceFormClassifier`, loader-parser and generated-destination resolution methods. Keep filename-only classifiers unchanged. `SourceLoadingClosureRequest` gains required `Layout`.

Make logical-path syntax root-neutral. Enforce content membership at catalogue and command boundaries. Included files retain their present `./path` identities. Catalogue reconstruction preserves both Layout and aliases.

Each mutation plan gains `WorkspaceLayoutRead LayoutRead`. Install additionally carries `WorkspaceLayout IntendedLayout`. Existing plan owners are:

- `InstallPlan.cs:28`
- `UpdatePlanningModels.cs:55`
- `IndexPlan.cs:30`
- `RepairPlanModels.cs:360`
- `RemovePathPlan.cs:14`
- Route `{Create,Init,Update,Move,Remove}` plan files
- `Route/Shared/Inclusion/Models/RouteInclusionPlan.cs:7`
- Extension `{Install,Update,Remove}` plan files
- Library `{Attach,Sync,Detach}` plan files
- `Cleanup/Models/Planning/CleanupPlan.cs`

Read once at operation entry, retain the observation through planning and use its settings rather than reading them again downstream. Under-lease checks validate that observation before grants or target effects. Post-effect verification checks intended state, not obsolete pre-effect settings.

Sources receive only the pure layout. Status, Doctor and Repair distribute one observation, passing pure layout to source contributors and existing settings/ownership observations to lifecycle contributors.

## Waves, packets and conversion inventory

The following lists semantic conversions. Carrier changes are separately identified afterward. Each packet’s tests belong beside the corresponding existing production scope.

### Wave 0: foundation owner, sequential packets

**A1 — Layout value.** Own the two new Workspace files. Test with new `U/Framework/Workspace/Models/Layout/WorkspaceLayoutTests.cs`.

Command: `tu '*WorkspaceLayoutTests'`.

Accept default identity mapping, nested roots, fixed Skills, category mapping, reserved paths, portable case collisions and literal-path behavior.

**A2 — Settings v4.** Own:

- `F/Settings/WorkspaceSettingsDefinitions.cs:24`
- `Models/Document/WorkspaceSettingsDocument.cs:14`
- `Shared/Serialization/WorkspaceSettingsCodec.cs:48`
- `WorkspaceSettingsCodec.Roots.cs:16`
- `WorkspaceSettingsCodec.LinkScope.cs:11`
- new `WorkspaceSettingsCodec.MainRoot.cs`
- `Shared/Planning/WorkspaceRemovals.cs:12`
- new `schemas/v4/open-forge.schema.json`

Remove fixed-root registration rejection, category-exclusion expansion and archive seeding. `WorkspaceRemovals.IsPathRemoved` receives Layout. Archive seeding receives the selected archive path. Leave published schemas v1–v3 unchanged.

Commands: `tu '*WorkspaceSettings*'`, `tu '*WorkspaceRemovalsTests'`.

Accept null/type rejection, canonical syntax, unknown-key retention, old declarations, new v4 files, no inserted default and registration validation against the selected layout.

**A3 — Observation and revalidation.** Own the four new Settings observation/revalidation files. Add matching unit and integration tests.

Commands: `tu '*WorkspaceLayoutReadTests'`, `ti '*WorkspaceLayoutReaderIntegrationTests'`.

Accept missing/empty custom destinations as observations, unsafe ancestry, contradictory pointers, missing selected loaders, settings edits after planning and zero effects during reading. Test Windows aliases and Unix links on overseer hosts.

### Wave 1: source foundation, sequential B1 then B2

**B1 — Identity and inventory.** Own these F/Sources files:

- `Identity/SourceLogicalPath.cs:22`, `SourceIdentity.cs:14`, `SourceFormClassifier.cs:16`, `SourceWorkspaceRelativePath.cs:11`
- `Models/Inventory/SourceCatalogueRequest.cs:9`, `SourceCatalogue.cs:18`, `SourceCandidate.cs:17`, `SourceLogicalIdentity.cs:12`, `SourceLayer.cs:20`, `SourceCatalogueSelectionScope.cs:11`
- `Inventory/SourceCatalogueReader.cs:136`

Separate canonical syntax from layout membership. Derive stable IDs relative to the appropriate content location. Preserve literal external identities and overwrite semantics. Inventory custom content and native Skills without treating absent optional Skills as a missing main root.

Commands: `tu '*SourceIdentity*'`, `tu '*SourceFormClassifierTests'`, `ti '*SourceCatalogueReaderIntegrationTests'`.

**B2 — Reading, loading and includes.** Own:

- `Reading/SourceReadSessionReader.cs:12`
- `Models/Reading/SourceReadSession.cs:8`
- `Models/Selection/SourceUniverseFilterRequest.cs:8`
- `Selection/SourceUniverseFilterResolver.cs:128`
- `Shared/Includes/SourceIncludedEntryReader.cs:62`
- `Routing/SourceLoaderDestinationParser.cs:45`, `SourceLoaderEntriesParser.cs:55`, `SourceRouteFactsResolver.cs:104`
- `Loading/SourceGeneratedDestinationResolver.cs:23`, `SourceLoadingClosureGraph.cs:43`, `SourceLoadingClosureResolver.cs:54`
- `Models/Loading/SourceLoadingClosureModels.cs:150`

Resolve loader destinations from the actual loader directory. Classify links to native Skills as ordinary native sources even when their relative spelling contains `..`. Preserve foreign includes, aliases, conditions and ordering.

Commands: `tu '*SourceLoader*'`, `tu '*SourceUniverseFilterResolverTests'`, `ti '*SourceReadSession*'`, `ti '*IncludedSourcesIntegrationTests'`.

Accept no-selector parity and exclusion-only preservation of unresolved candidates and unavailable-directory issues across both roots.

### Wave 2: C1 and C2 may run in parallel

**C1 — Navigation and shared route maintenance.** Own:

- `F/GeneratedNavigation/GeneratedNavigationFormationBuilder.cs:163`, `.Derivation.cs:52`, `GeneratedNavigationRegionPlanner.cs:166`
- `F/Sources/Sharing/SourceSharing.cs:23`
- `O/Shared/RouteScaffolding/{RouteScaffoldSourceBuilder.cs:18,RouteScaffoldChainDiscovery.cs:24,RouteScaffoldRenamePlanner.cs:34}`
- `O/Shared/RoutedMaintenance/RoutedMaintenancePlanner.cs:106`
- `Metadata/{RoutedMetadataCompletionPolicy.cs:35,RoutedMetadataDefaults.cs:61}`
- `IRoutedMaintenanceRenameEvidence.cs:9`
- `O/Shared/WorkspaceAdoption/Shared/{WorkspaceAdoptionTopologyPlanner.cs:20,WorkspaceAdoptionEntrypointComposer.cs:26}`
- `H/Shared/Git/RoutedMaintenanceRenameEvidenceAdapter.cs:38`

Remove root-depth arithmetic and fixed loader lookups. Make persisted sharing records structurally root-neutral, validating membership when consumed with a catalogue. Preserve Layout and aliases when filtering or replacing catalogues.

Commands: `tu '*GeneratedNavigation*'`, `ti '*IndexSkillCatalogueIntegrationTests'`, `ti '*RenameSourceMoveIntegrationTests'`.

**C2 — Package mapping and rendering.** Own:

- `F/Distribution/Models/FrameworkPayloadAsset.cs:48`
- `EmbeddedFrameworkSourceProjector.cs:26`
- `Shared/Sources/{FrameworkPayloadSourceProjection.cs:15,FrameworkSourceAlignment.cs:10,FrameworkPayloadSelection.cs:15}`
- `Shared/Content/WorkspacePayloadRenderer.cs:15`
- new `Shared/Content/WorkspaceLayoutPayloadRenderer.cs`

Keep package validation in package coordinates. Map before destination exclusions, collisions and ownership. Reverse alignment searches mapped payload destinations rather than assuming installed paths equal package paths.

The new renderer rebases parsed links and recognized host-template references. Preserve Claude’s `@AGENTS.md`. Custom rendered README and CLI Skill guidance use selected paths and generic `./...` operands. Default rendering returns original bytes, including line endings. Do not broadly replace text in authored files.

Commands: `tu '*FrameworkPayload*'`, `tu '*EmbeddedFrameworkSourceProjectorTests'`, `tu '*WorkspacePayloadRendererTests'`, `te '*PublishedEmbeddedPayloadProcessTests'`.

### Wave 3: D1, D2 and D3 may run in parallel

**D1 — Context, Find and References.** Own:

- `O/Context/ContextOperation.cs:17`
- `Shared/Graph/ContextGraphBuilder.cs:34`
- `Shared/Links/{ContextLinkExpander.cs:57,ContextLinkedSourceReader.cs:39}`
- `Shared/Selection/ContextLoadingClosureResolver.cs:316`
- `Shared/Projection/ContextProjectionBuilder.cs:196`
- `Models/Operation/ContextGraphModels.cs:99`
- `O/Find/{FindOperation.cs:36,FindOperationFactory.cs:25}`
- `Shared/Application/FindSourceResolver.cs:25`
- `Shared/Selection/FindUniverseResolver.cs:39`
- `Models/Selection/FindSelectionModels.cs:50`
- `Models/Result/{FindProjectionModels.cs:73,FindMatch.cs:34}`
- `O/References/{ReferencesOperation.cs:46,ReferencesOperationFactory.cs:19}`
- `Shared/Resolution/ReferencesSourceResolver.cs:31`

Replace independent settings reads and default session creation. Result constructors use established catalogue identities rather than re-deriving IDs without Layout.

Commands: `ti '*Commands.Context.*'`, `ti '*Commands.Find.*'`, `ti '*Commands.References.*'`.

Extend their existing `Published*ProcessTests` with independently authored custom-root fixtures.

**D2 — Index.** Own:

- `O/Index/Shared/Projection/IndexProjectionReader.cs:68`
- `Shared/Selection/IndexSelectionResolver.cs:451`
- `Models/Selection/IndexSelectionModels.cs:35`
- `Models/Planning/IndexPlan.cs:30`
- `Shared/Planning/IndexPlanBuilder.cs:11`
- Index application entry and revalidation consumers of that plan

Use the observed layout, preserve native Skill traversal and reject changed layout evidence before application. Keep `IndexSkillCatalogueTraversal.cs:11` fixed to native Skills.

Commands: `ti '*Commands.Index.*'`, `te '*PublishedIndexProcessTests'`.

Accept nested reference catalogues, private routes and deliberately unlisted Skills without reactivation.

**D3 — Internal Install kernel.** Own:

- `O/Install/Shared/Planning/{InstallPlanBuilder.cs:8,InstallPlanningInspector.cs:59,InstallPlanningBasisBuilder.cs:36,InstallIntendedStateBuilder.cs:37,InstallEstablishmentPlanner.cs:445,InstallWorkspaceAdoptionBuilder.cs:333,InstallPayloadRendering.cs:15}`
- `Shared/Configuration/{InstallConfigurationChoices.cs:49,InstallConfigurationSettings.cs:24,InstallConfigurationIntendedStateBuilder.cs:77,InstallConfigurationFrontmatterPlanner.cs:106,InstallConfigurationSharing.cs:23,InstallIgnoreSection.cs:34}`
- `Shared/Operation/InstallApplicationPreconditionValidator.cs:25`
- `Models/Planning/{InstallPlan.cs:28,InstallPlanningStages.cs:33,InstallConfigurationPlan.cs:10}`
- `Models/Request/InstallRequest.cs:14`

Add internal requested-root input and intended-layout planning, without binding an option or asking a question. Refuse occupied custom destinations and established-root changes before lease acquisition. Keep default adoption intact.

Commands: `ti '*Commands.Install.*'`, including new `InstallMainRootIntegrationTests`.

Before proceeding, prove internal custom Install → Context → Index, including host pointers, fixed Skills, ownership and both frontmatter forms.

### Wave 4: shared observations and permissions

**E1 — Observations and rename evidence.** Own:

- `F/Workspace/Operational/WorkspaceEntryOperationalContributor.cs:39`
- `F/Sources/Operational/{RouteOperationalContributor.cs:11,LocalReferenceOperationalContributor.cs:28,RouteDoctorFactReader.cs:35,RouteGeneratedEntryComparisonReader.cs:31}`
- `Shared/Routes/{RouteSourceInspector.cs:42,RouteObservationReader.cs:27,RouteIncludedFileReader.cs:23,RouteGeneratedNavigationReader.cs:35,RouteCurrentContextReader.cs:82,RouteContextClosureResolver.cs:15,RouteInitialContextReader.cs:27,RouteContextReader.cs:14}`
- `Shared/RenameEvidence/{RenameMetadataIdentity.cs:43,RenameRecordedEntryReader.cs:22,RenameCandidateAdmission.cs:15,RenameCanonicalEntrypointEvidence.cs:13}`
- `Shared/RenameEvidence/Models/Input/RenameEvidenceRequest.cs:12`
- `F/Distribution/Operational/{FrameworkLifecycleOperationalContributor.cs:18,FrameworkLifecycleTargetReader.cs:89}`
- `F/Extensions/Operational/{ExtensionLifecycleOperationalContributor.cs:12,ExtensionLifecycleDoctorReader.cs:40,ExtensionLifecycleTargetReader.cs:141,ExtensionBridgeRegistrationObservationReader.cs:73}`
- `O/Status/StatusOperation.cs:19`, `Shared/Aggregation/StatusLibraryAggregator.cs:79`
- `O/Doctor/DoctorDiagnosisReader.cs:18`
- `Shared/Maintenance/{DoctorMaintenanceReader.cs:162,DoctorMaintenanceGuidance.cs:119}`
- `Shared/Domains/{WorkspacePathDoctorInspector.cs:47,WorkspaceIncludeDoctorInspector.cs:31,RouteDoctorInspector.cs:25}`

Share observations rather than rereading settings. Compare ownership destinations with mapped payloads. Keep recovery contributors workspace-fixed.

Commands: `ti '*Commands.Status.*'`, `ti '*Commands.Doctor.*'`, `ti '*Framework.Sources.Operational.*'`.

**E2 — Permission boundaries.** Own:

- `O/Extension/Shared/Permissions/{ExtensionDestinationPolicy.cs:17,ExtensionPermissionOperation.cs:65}`
- `O/Extension/Models/ExtensionPermissionStage.cs:32`
- `O/Library/Shared/Permissions/{LibraryPermissionScopePlanner.cs:41,LibraryPermissionOperation.cs:125}`
- `O/Library/Models/Permissions/LibraryPermissionPlanning.cs:30`
- `O/Library/Shared/Planning/{LibraryDestinationPolicy.cs:75,LibraryConsumerBoundaryReader.cs:26,LibraryMutationPlanningPolicy.cs:32}`
- `F/Libraries/Shared/Permissions/LibraryRecoveryPermissionReader.cs:34`
- `SH/Interaction/Models/CliPromptModels.cs:54`
- `R/Shared/Prompts/CliPrompts.Permission.cs:34`
- `R/Shared/Wording/CliPromptWording.cs:46`
- `T/Shared/{CliPromptWording.cs:32,CliFindingWording.cs:95}`

Implicit admission follows content locations. Explicit grants stay literal. Permission questions receive projected context-folder facts, retaining default wording exactly. Revalidate before writing grants.

Commands: `tu '*WorkspacePermission*'`, `ti '*LibrarySharedGrant*'`, plus new `WorkspaceLayoutPermissionIntegrationTests`.

E1 and E2 are disjoint and may run in parallel.

### Wave 5: command-family packets may run in parallel

Each packet below owns its named files, local plan carriers, local revalidators and matching command tests. Shared APIs are already frozen.

| Packet | Semantic conversion files and removed assumption | Focused commands |
|---|---|---|
| L — Route List | `O/Route/List/Shared/Filesystem/{RouteListInventoryRequest.cs:12,RouteListInventoryReader.cs:24}`, `Shared/Selection/{RouteListSelectionResolver.cs:22,RouteListSelectionResolver.Loader.cs:96,RouteListSelectionResolver.LoaderIssues.cs:102}`, `Shared/Topology/{RouteListTopologySelectionFindings.cs:212,RouteListTopologyFindingDepthPolicy.cs:15}`, `Shared/Application/{RouteListSourceResolver.cs:61,RouteListIncludedResultBuilder.cs:62}`. Replace fixed inventory roots, loader comparisons and prefix stripping. | `ti '*Commands.Route.List.*'`, `te '*PublishedRouteListProcessTests'` |
| V — Route Inspect | `O/Route/Inspect/Shared/Resolution/{RouteInspectResolver.cs:84,RouteInspectResolutionSupport.cs:73,RouteInspectSourceFactsResolver.Issues.cs:82,RouteInspectSourceProjectionBuilder.cs:55}`, `Shared/Profile/{RouteInspectLoadingFactsBuilder.cs:18,RouteInspectLoadingFactsBuilder.Entries.cs:37,RouteInspectAxiomsProfileBuilder.cs:64}`. Use actual loader and retained source identities. | `ti '*Commands.Route.Inspect.*'`, `te '*PublishedRouteInspectProcessTests'` |
| N — Route Init | `O/Route/Init/RouteInitBinding.cs:193`, `Shared/Planning/{RouteInitTargetPlanner.cs:149,RouteInitCurrentStateReader.cs:83,RouteInitRestorationInspector.cs:61,RouteInitProspectivePlanBuilder.cs:338,RouteInitFrameworkOwnershipBuilder.cs:32,RouteInitFrameworkAlignmentBuilder.cs:153,RouteInitPlanningInspector.cs:278,RouteInitIntendedChainBuilder.cs:106,RouteInitRestorationEffects.cs:48}`. Support generic literal paths, mapped restoration and content-root boundaries. | `ti '*Commands.Route.Init.*'`, `te '*PublishedRouteInit*'` |
| C — Route Create | `O/Route/Create/Shared/Planning/{RouteCreateTargetPlanner.cs:23,RouteCreateTargetInspector.cs:48,RouteCreateGeneratedNavigationPlanner.cs:473}`. Construct destinations from stable IDs and stop ancestor traversal at the appropriate root. | `ti '*Commands.Route.Create.*'`, `te '*PublishedRouteCreateProcessTests'` |
| U — Route Update | `O/Route/Update/Shared/Planning/{RouteUpdateSourceSelector.cs:45,RouteUpdatePlanProjector.Effects.cs:128}`. Select both content locations and preserve root boundaries. | `ti '*Commands.Route.Update.*'`, `te '*PublishedRouteUpdateProcessTests'` |
| M — Route Move | `O/Route/Move/Shared/Planning/{RouteMoveSubjectSelector.cs:79,RouteMoveDestinationProjector.cs:72,RouteMoveNavigationSourceProjector.cs:64}`, `Shared/References/{RouteMoveReferenceScanner.cs:262,RouteMoveReferenceScanner.Projection.cs:171}`, `Shared/Application/RouteMovePostMoveObserver.Destination.cs:101`. Move sources within the selected layout without redirecting literal paths. | `ti '*Commands.Route.Move.*'`, `te '*PublishedRouteMoveProcessTests'` |
| X — Route Remove | `O/Route/Remove/Shared/Planning/{RouteRemoveSubjectSelector.cs:79,RouteRemoveCategoryAbsencePlanner.cs:69,RouteRemovePersistencePlanner.cs:181}`, `Models/Planning/RouteRemoveAbsenceScope.cs:114`, `Shared/Application/{RouteRemoveEffectApplication.Settings.cs:119,RouteRemoveAppliedVerifier.Persistence.cs:122}`. Expand category exclusions through Layout, including absent targets. | `ti '*Commands.Route.Remove.*'`, `te '*PublishedRouteRemoveProcessTests'` |
| Q — Include/Exclude | `O/Route/Shared/Inclusion/Planning/{RouteInclusionTargetResolver.cs:26,RouteInclusionPlanBuilder.cs:49}`, `Application/RouteInclusionApplication.cs:58`, `Models/{RouteInclusionPlan.cs:7,RouteInclusionOutcome.cs:9}`. Replace second-segment assumptions and distinguish native Skills from foreign includes. Effects carry their kind rather than renderers guessing from a fixed loader path. | `ti '*RouteInclusion*'`, `te '*PublishedRouteIncludeProcessTests'`, `te '*PublishedRouteExcludeProcessTests'` |
| EU — Top-level Update | `O/Update/Shared/Planning/{UpdatePlanBuilder.cs:104,UpdateIntendedStateBuilder.cs:117,UpdateGeneratedNavigationPlanner.cs:106,UpdateWorkspaceAdoptionBuilder.cs:45}`, `Shared/Application/UpdatePlanRevalidator.cs:16`. Compare mapped installed paths, preserve package identities and avoid recreating the default loader. | `ti '*Commands.Update.*'`, `te '*PublishedUpdate*'` |
| ER — Top-level Remove | `O/Remove/Shared/Selection/RemoveTargetResolver.cs:26`, `Shared/Planning/{RemovePathSafety.cs:47,RemovePathPlanner.cs:308,RemoveNavigationPlanner.cs:43}`, `Shared/Application/RemovePathApplication.cs:187`. Separate fixed settings storage from content selection and navigation removal. | `ti '*Commands.Remove.*'`, plus existing published removal journeys |
| RP — Repair/Cleanup | `O/Repair/Shared/Planning/{RepairTargetReader.cs:38,RepairMaintenancePlanner.cs:36,RepairPlanRevalidator.cs:12}`, `Shared/Diagnosis/{RepairCatalogueReader.cs:58,RepairOperationalContributorFactory.cs:20,RepairRenameProposalReader.cs:34,RepairContinuationReader.cs:48}`, `RepairOperation.cs:19`, `O/Cleanup/CleanupOperation.cs:26`. Carry layout observations while retaining Task 86’s recorded recovery locations. | `ti '*Commands.Repair.*'`, `ti '*Commands.Cleanup.*'`, `te '*PublishedRepair*'`, `te '*PublishedCleanupProcessTests'` |

The route packets also own these shared transport conversions, integrated sequentially before parallel route work: `O/Route/Shared/Models/Source/{RouteSource.cs:39,RouteSourceDocument.cs:34}` and `Shared/References/{RouteMarkdownCatalogueReader.Traversal.cs:77,RouteMarkdownCatalogueReader.State.cs:49}`. Their owner is C1. Do not duplicate these edits in route leaves.

**EX — Extensions**, sequential subpackets Install, Update, Remove, then inspection:

- Install planning: `ExtensionInstallFoundationReader.cs:43`, `ExtensionInstallPayloadNormalizer.cs:17`, `ExtensionInstallPlanner.cs:285`, `ExtensionInstallTargetInspector.cs:66`, `ExtensionInstallEffectPlanner.cs:41`, `ExtensionInstallTopologyBuilder.cs:67`
- Update planning: `ExtensionUpdatePlanner.cs:482`, `ExtensionUpdateReconciler.cs:384`, `ExtensionUpdateTopologyBuilder.cs:66`, `ExtensionUpdateIndependentMetadataProof.cs:78`
- Remove planning: `ExtensionRemovePlanner.cs:179`, `ExtensionRemoveTopologyBuilder.cs:43`, `ExtensionRemovePathInspector.cs:24`
- `ExtensionInstallOperationFactory.cs:169`, `ExtensionUpdateOperation.cs:127`, `ExtensionRemoveOperation.cs:118`
- Inspect: `ExtensionInspectOperation.cs:41`, `Shared/Result/{ExtensionInspectInstalledClosureReader.cs:15,ExtensionInspectFingerprintPolicy.cs:11,ExtensionInspectFingerprintBuilder.cs:75,ExtensionInspectPathFactsBuilder.cs:66,ExtensionInspectPathComparisonBuilder.cs:38,ExtensionInspectComparisonProjector.cs:140,ExtensionInspectPackageLifecycleBuilder.cs:187}`
- `Extension/List/ExtensionListOperation.cs:12`

Paths above are under their respective `O/Extension/<command>/` directories and `Shared/Planning` where stated.

Map portable package destinations before every exclusion, permission, collision and ownership calculation. Preserve package `Path`, record actual installed `TargetPath`, and use the same mapping for inspection and lifecycle fingerprints.

Commands per subpacket: `ti '*Commands.Extension.<command>.*'` and its existing `te '*PublishedExtension<Command>ProcessTests'`.

**LB — Libraries**, after E2:

- `O/Library/Shared/Planning/LibraryGeneratedNavigationReader.cs:51`
- `Shared/Completion/LibraryMutationCompletionProjection.cs:115`
- `{Attach,Sync,Detach}/Shared/Planning/Library{Attach,Sync,Detach}Planner.cs:69,96,82`
- `{Attach,Sync,Detach}/Library{Attach,Sync,Detach}Operation.cs:46,44,42`
- `Inspect/Shared/Comparison/LibraryInspectComparisonReader.cs:64`
- `Inspect/LibraryInspectOperation.cs:20`
- `List/LibraryListOperation.cs:220`

Keep registrations, sources and suffixes literal. Convert only consumer-layout boundaries, source IDs, navigation and implicit admission.

Commands: `ti '*Commands.Library.*'`, `te '*PublishedLibrary*'`. Real relative-link journeys run on Windows, Linux and macOS.

### Carrier and presentation inventory

The semantic files above require transport changes in their existing operation entry, plan construction and revalidation callers. Ownership follows the command packet:

- Route plan files and `Shared/Planning/*PlanBuilder.cs`, `*PlanProjector.cs`, `Shared/Application/*PlanRevalidator.cs`
- `O/Update/Models/Planning/{UpdatePlanningModels.cs:55,UpdatePlanArtifacts.cs:37}`
- `O/Extension/{Install,Update,Remove}/Models/Planning/Extension{Install,Update,Remove}Plan.cs:49,125,96`
- Library planning-input and plan files under `{Attach,Sync,Detach}/Models/Planning`
- `O/Repair/Models/Planning/RepairPlanModels.cs:360`
- `O/Remove/Models/Planning/RemovePathPlan.cs:14`
- `O/Cleanup/Models/Planning/CleanupPlan.cs:11`

These transport LayoutRead and compare it during revalidation. They do not implement another root policy.

Each command packet also converts its existing root-sensitive presentation sites:

- `R/Route/{Create,Move}/Shared/Selection/Route{Create,Move}ReportSelector.cs:636,764`
- `R/Route/{Include,Exclude}/Shared/Selection/Route{Include,Exclude}ReportSelector.cs:60`
- `R/Route/Remove/Shared/Wording/RouteRemoveWording.cs:17`
- `R/Route/Inspect/Shared/Wording/RouteInspectWording.cs:116`
- `R/Repair/Shared/Selection/RepairReportSelector.cs:963`
- `R/Doctor/Shared/Wording/DoctorMaintenanceWording.cs:99`
- `R/Library/Attach/Shared/Selection/LibraryAttachReportSelector.cs:827`
- `R/Extension/Update/Shared/Wording/ExtensionUpdateWording.cs:207`

Supply projected paths or IDs. Rendering must not infer roots from path segment positions.

Update corresponding T files containing fixed-root guidance: ContextText, FindText, FindPhrases, ReferencesText, ReferencesWording, StatusText, StatusWording, DoctorText, DoctorPhrases, DoctorMaintenanceText, route command Text/Phrases files, ExtensionInstallWording, ExtensionRemoveText and ExtensionUpdateText. Preserve default runtime wording, introducing custom-root variants. Help changes are reviewed exceptions.

Audit-only hits that do not require conversion include fixed settings/ownership readers, `LibraryMapping.cs:37`, Library source-coordinate classification, package manifests and scaffold paths, generic physical-path readers, filename-only classifiers and already projected result fields. `Extension/Create/ExtensionCreateBinding.cs:71` requires no workspace. Its package scaffold remains `content/.agents`. `Help/HelpOperation.cs:65` continues reading the embedded README.

### Wave 6: final code wave

**Z1 — Remove adapters and close observation output.** Foundation owner verifies every runtime caller supplies the operation’s layout. Add new Operations-owned `WorkspaceLayoutInfo(string MainRoot, string LoaderPath)` and projection files under `O/Shared/Models/Layout` and `O/Shared/Layout`.

Status and Install results may expose optional Layout facts. Update their result models, R data models, report selectors, text renderers and source-generated JSON contexts. Omit Layout entirely for default workspaces. Keep JSON envelope schema 3.

Commands: `te '*PublishedSchema3ProcessTests'`, `te '*PublishedStatusProcessTests'`, full managed acceptance.

**Z2 — Activate Install**, only after Z1 and all command packets pass:

- `O/Install/{InstallDefinitions.cs,InstallBinding.cs,InstallRequestBinder.cs,InstallOperation.cs,InstallOperationFactory.cs}`
- `Shared/Binding/InstallBindingInputReader.cs:14`
- `Models/Binding/InstallBindingModels.cs:9`
- `Models/Configuration/{InstallConfiguration.cs:10,InstallSetupRecap.cs:5}`
- `Shared/Configuration/InstallSetupResolver.cs:26`
- `H/CliStandaloneComposer.cs`
- `R/Install/Shared/{Prompts/InstallSetupQuestions.cs,Help/InstallHelpSections.cs,Selection/InstallSetupRecapSelector.cs,Selection/InstallReportSelector.cs:25}`
- `T/Install/{InstallSetupText.cs,InstallText.cs,InstallPhrases.cs:37}`

Add `--main-root <folder>` and the exact frozen question before route choices. Explicit input answers it. Preview, JSON, automatic and redirected execution never ask. Preserve interactive Minimal and unattended preset behavior.

Commands: `ti '*Commands.Install.*'`, `te '*PublishedInstall*'`, new `te '*PublishedMainRoot*'`.

## Qualification and snapshot acceptance

Add end-to-end journeys named:

- `PublishedMainRootInstallProcessTests`
- `PublishedMainRootLifecycleProcessTests`
- `PublishedMainRootOfflineStartupProcessTests`
- `PublishedMainRootDefaultCompatibilityProcessTests`

Run all four on Windows, Linux and macOS. Execute supported x64 and arm64 targets before shipping.

Cover nested roots, both frontmatter forms, fixed Skills and controls, external includes, literal operands, all command families, Update without default-root recreation, Remove preservation, Repair, Cleanup, mismatch refusal and settings changes after preview.

Offline tests independently follow both host files to the loader, native Skills and included files. Repeat without the installed README. No-reactivation tests remove a row, then run Index, Update, Configure and Extension Install.

Default compatibility compares exact paths, bytes, ownership and managed regions against the post-Task-86 baseline. Independently compare text and JSON. Existing snapshots ignore line endings, so snapshots alone are insufficient. Do not compare independently created timestamps as byte-preservation evidence.

Expected snapshot churn is limited to new custom-root cases, new v4 declarations, intentional help changes and the install question. Existing default command snapshots otherwise remain unchanged. Update targeted captures serially with `OPENFORGE_SNAPSHOT_UPDATE=1`, then verify without it.

After every integrated wave, run required managed and host-native gates. Final qualification uses fresh artifacts with `npm run build:native` and `npm run test:built` on each supported host. Six execution modes on one host are not six-platform evidence.

## Final documentation packet

One documentation owner, after Z2, updates:

- C `architecture.md`, `shared-operation-contract.md`, `command-contract-set.md`
- C `technical-designs/{_technical-designs.md,embedded-payload.md,generated-navigation.md,lifecycle-provenance.md,workspace-permissions.md,workspace-libraries.md,mutation-and-recovery.md,forgiving-maintenance.md}`
- New C `technical-designs/workspace-layout.md`
- C shared source-reference, source-universe-filter, workspace-permission and result-coordinate contracts
- Interface and behavior contracts for Install, Update, Remove, Repair, Cleanup, Status, Doctor, Context, Find, References, Index, all route leaves, Extension lifecycle/inspection and Library leaves
- Context, Find, Index and Route Update technical designs
- `docs/{cli.md,extensions.md}`, root `README.md`
- `src/docusaurus/docs/getting-started/{installation.md,ten-minute-guide.md,first-task.md,grow-your-framework.md,greenfield-and-brownfield.md}`
- `src/docusaurus/docs/cli/{index.md,flows.md}`
- `src/docusaurus/docs/concepts/{index.md,core-categories.md,customizing.md,routing.md,loading-and-tags.md,scopes.md,memory.md}`
- `src/docusaurus/docs/{intro.md,glossary.md,highlights.md}`
- `src/docusaurus/docs/extensions/{index.md,reading-installed-files.md,document-flow.md}`

Review rendered custom variants of `src/open-forge/{AGENTS.md,CLAUDE.md}`, `.agents/README.md`, CLI Skill `SKILL.md` and references `{common,discovery,routes,packages}.md`. Preserve default payload bytes. Keep default examples clearly identified and document generic exact paths, fixed Skills, unsupported older CLIs and unavailable relocation.

Commands: `npm run check:docs`, `npm run docs:build`, `te '*PublishedHelpProcessTests'`.

## Integration risks

| Risk | Early detection |
|---|---|
| Package paths mistaken for installed paths | C2 and Extension inspection tests compare both coordinate systems |
| Layout lost during catalogue reconstruction | Reference-identity and alias tests through includes, sharing and scaffolding |
| Skills treated as foreign includes | Custom loader → native Skill → reference catalogue journey |
| Errors disappear outside one selection scope | Exclusion-only unreadable-directory tests |
| Default output drifts through “helpful” wording changes | Existing snapshots plus direct byte oracles |
| Settings change between preview and effects | Under-lease stale-observation tests, including permission grants |
| Post-Task-86 history becomes a blocker again | Retain historical-warning tests and fixed recovery-store assertions |
| Stale development publish gives false confidence | Overseer rebuilds before published-process tests and records artifact identity |

Estimate: **14–18 engineering days after Task 86**, including integration and qualification. Parallelism reduces elapsed time, not the acceptance requirements.

## Delivery 2 outline

Start only after Delivery 1 ships. Budget **7–10 additional engineering days**.

1. **M7 inventory packet:** real filesystem inventory, ignored/untracked/binary/overwrite files, empty directories, collisions and unsupported links.
2. **M7 privacy and metadata packet:** protect both locations throughout application, preserve authored ignore text, define preserved metadata and refuse unsupported cases.
3. **M7 Library packet:** refuse intersecting source trees and prove every affected registration remains representable with one destination root and unchanged suffixes.
4. **M7 application packet:** verify destination bytes, revalidate sources before deletion, publish reviewed ownership/settings/pointers and delete the old loader last.
5. **M7 restoration packet:** close the Install third-state recovery-filter gap and test restoration without overwriting subsequent edits.
6. **M7 activation and qualification packet:** enable established-workspace relocation only after interruption and round-trip evidence passes on Windows, Linux and macOS.
