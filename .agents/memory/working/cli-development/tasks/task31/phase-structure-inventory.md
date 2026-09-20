---
open-forge:
  description: Task 31 M5 inventory and disposition map for Core one-file folders and oversized file seams before any move
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Structural, Refactoring, Evidence]
---

# Task 31 M5 — Structure cleanup inventory

## Goal

Record a complete, branch-point inventory of the one-file leaf folders in
`src/cli/core/OpenForge.Cli.Core`, and a disposition for every one, before any
file is moved. Record the genuine semantic seams in the largest Core files so
that a later implementation batch has a bounded, independently verifiable
order of application.

## Depends on / Blocks

Task 30 G4; Task 31 M1; Task 31 M3; Task 31 M4

## References

- `.agents/memory/working/cli-development/tasks/task31/phase-structure.md:22` — M5 action boundary: inventory one-file folders and oversized files before moving anything.
- `.agents/memory/working/cli-development/tasks/task31/phase-structure.md:37` — acceptance requires a disposition for every one-file folder and every proposed oversized split.
- `.agents/memory/working/cli-development/tasks/task31-implementation-duplication.md:40` — preserve local contracts and prefer deletion or an existing owner; do not infer that a duplicate shape should become a shared API.
- `.agents/memory/working/cli-development/tasks/task31-implementation-duplication.md:49` — Route Move/Remove finding-code formation is a stop boundary; lifecycle and permission internals remain with Task 30.
- `.agents/memory/working/cli-development/tasks/task31/08-m1-enumerated-extractions.md:31` — M1 record style and evidence standard.
- `.agents/memory/emerging/analysis/cli-experience-audit/csharp-directives-and-structure.md:1` — sealed provenance read as evidence, not as a current disposition.
- `.agents/memory/emerging/analysis/cli-experience-audit/layer-adherence.md:1` — sealed provenance read as evidence, not as a current disposition.
- `.agents/directives/csharp/design.md:1` — ownership, cohesion, and contract-preserving design rules.
- `.agents/directives/csharp/style.md:1` — C# locality and naming rules.
- `.agents/directives/program-architecture.md:1` — dependency direction and project-boundary rules.
- `.agents/directives/source-locality.md:9` — keep a capability in the narrowest owner that can own it.
- `.agents/memory/crystallized/documents/cli/architecture.md:245` — the four Core layers and their dependency direction.
- `.agents/memory/crystallized/documents/cli/architecture.md:298` — command-local locality and presentation ownership.
- `.agents/memory/crystallized/documents/cli/architecture.md:346` — generated-code and serialization boundaries.
- `.agents/memory/crystallized/documents/cli/layers/_layers.md:39` — layer ownership contract.
- `.agents/memory/crystallized/documents/cli/layers/_layers.md:104` — presentation may consume command results only through its owning command boundary.
- `src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj:1` — Core is one SDK-style project with default compile globs.
- `src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj:27` — Core test visibility boundary.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj:16` — unit tests reference Core directly.
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj:18` — integration tests reference Core directly.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Architecture/LayerBoundaryTests.cs:16` — layer-boundary test fixture.
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Presentation/Invariants/CliReportInvariantsTests.cs:65` — presentation invariant characterization.

## Preconditions

- [x] Task 30 G4 is at the branch point: all 28 commands use the native
  `Presentation/<Command>/` report shape, and `Presentation/Legacy/` contains
  only the three composer-bound help-section files.
- [x] No source files were moved, renamed, deleted, or edited for this
  inventory.
- [x] The two behavior-fix lanes named by the task are not included in this
  branch-point checkout. Rows that mention `Commands/Library/**` or
  `Commands/Route/Remove/**` are explicitly marked for re-check after those
  lanes merge.

## Scan and counting rule

I count a folder when it is a directory below the Core project root, contains
exactly one `.cs` file directly in that folder, and has no child directories;
non-C# files are ignored, and `bin`/`obj` are excluded. This is a leaf-folder
count, not a count of every directory that happens to contain one direct C#
file.

The branch-point scan produced **294 one-file leaf folders** across **1,873 C#
files**. The result is **118 higher than the 176 recorded in
`phase-structure.md`**. The 176 figure is from the earlier pre-G4 inventory and
does not state a reproducible folder definition; the current tree also has the
settled per-command Presentation structure, which alone contributes 123 leaf
folders. The current tree confirms that the definition matters: counting one
direct C# file even when a folder also has child directories gives 336, while
counting folders recursively by total C# files gives 304. I use 294 below and
do not silently substitute either 176 or one of the alternative counts.

The scan can be reproduced from the repository root with this PowerShell
predicate (the final count is 294):

```powershell
$core = Get-Item 'src/cli/core/OpenForge.Cli.Core'
$oneFileLeafFolders = Get-ChildItem $core -Directory -Recurse |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj)(\\|$)' -and
        (Get-ChildItem $_.FullName -File -Filter '*.cs').Count -eq 1 -and
        (Get-ChildItem $_.FullName -Directory).Count -eq 0
    }
$oneFileLeafFolders.Count
```

## Disposition summary

Every one of the 294 folders is listed below, either as an individual decision
or as an explicit member of a group with one shared reason.

| Disposition | Folders |
| --- | ---: |
| Keep (retained) | 289 |
| Collapse into `Commands/Extension/List/Models` (applied) | 3 |
| Decision gates (resolved and applied) | 2 |
| Total | 294 |

The map names the branch-point file in every folder and records the resulting
disposition. `Keep` means that the current owner, layer, namespace, project
boundary, generated-code boundary, and test impact remain clearer than a move.
`Collapse` is reserved for the one case where flattening removes microfolders
without changing ownership or a public contract. The two former decision gates
were resolved by the maintainer ruling and are recorded below as applied.

## Disposition map

### Collapse into `Commands/Extension/List/Models` — 3 folders (applied)

The three direct model-topic children of the otherwise empty, command-local
`Commands/Extension/List/Models` folder were moved into their parent:

```text
Commands/Extension/List/Models/Binding -> ExtensionListBindingModels.cs
Commands/Extension/List/Models/Request -> ExtensionListRequest.cs
Commands/Extension/List/Models/Result -> ExtensionListResult.cs
```

The target namespace is
`OpenForge.Cli.Core.Commands.Extension.List.Models`. The three declarations
were changed to that namespace; the redundant internal usings in the binding
and result model files were removed; and the following production consumers
were updated from the three topic namespaces to the target namespace:

```text
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/ExtensionListBinding.cs:2-4
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/ExtensionListDefinitions.cs:1
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/ExtensionListOperation.cs:2-3
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/ExtensionListRequestBinder.cs:2-4
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/ExtensionListWorkspaceResultFactory.cs:1-2
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/Shared/Result/ExtensionListResultBuilder.cs:1-2
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/Shared/Result/ExtensionListRowBuilder.cs:1-2
src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/Shared/Result/ExtensionListStatusPolicy.cs:1
src/cli/core/OpenForge.Cli.Core/Presentation/Extension/List/ExtensionListPresentation.cs:3
src/cli/core/OpenForge.Cli.Core/Presentation/Extension/List/Shared/Selection/ExtensionListReportSelector.cs:2
src/cli/core/OpenForge.Cli.Core/Presentation/Extension/List/Shared/Wording/ExtensionListWording.cs:2
src/cli/root/OpenForge.Cli/Composition/CliExtensionComposer.cs:24-25
```

The independent whole-`src/cli` search found 12 production consumers: the 11
Core files listed first and the root composition file listed last. No further
production or test consumer was found beyond the three characterization tests
below.

The directly affected characterization tests are
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Extension/List/ExtensionListBindingAndPresentationTests.cs:4-5`,
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Extension/List/Shared/Rendering/ExtensionListOutputSnapshotTests.cs:1-2`,
and
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/ExtensionListApplicationIntegrationTests.cs:4-5`.
There is no project-reference change: Core remains the same SDK project, and
the existing direct Core references in the unit and integration project files
remain unchanged. The move keeps the command and Presentation layer ownership
intact and makes the three already-prefixed model types discoverable under one
small command-local `Models` namespace. It does not alter JSON names, finding
codes, exit codes, streams, or generated-code registration. This is the only
one-file-folder flattening proposed by this inventory.

### Former decision gates — 2 folders (resolved and applied)

#### `Commands/Route/List/Models/Result -> RouteListPresentationFacts.cs`

This folder was not safe to collapse on the one-file count alone. The file's
namespace is
`OpenForge.Cli.Core.Commands.Route.List.Models.Result`, but
`Commands/Route/List/RouteListResult.cs:7` declares that same namespace while
remaining physically at the command root. The maintainer ruling was to
co-locate it with `RouteListPresentationFacts.cs` under
`Commands/Route/List/Models/Result`. The move was pure: the namespace, all
usings, and every other file remained unchanged. The moved file's SHA-256 is
`FAF24C3D6FF69E21A0C54D09A3DFF7FBB81B7D15DD53F8E18F38F7BD8A8FCE1F` both before
and after the move.

The existing references that were rechecked after the move are:

```text
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Shared/Topology/RouteListResultBuilder.cs
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Shared/Application/RouteListResultComposer.cs
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListOperationFactory.cs
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListOperation.cs
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListBindingInputPolicy.cs
src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListBinding.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/Models/RouteListData.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/RouteListPresentation.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/Shared/Rendering/RouteListDataJsonConverter.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/Shared/Rendering/RouteListDataTextRenderer.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/Shared/Selection/RouteListReportSelector.cs
src/cli/core/OpenForge.Cli.Core/Presentation/Route/List/Shared/Wording/RouteListWording.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/RouteListBindingTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/RouteListPresentationTests.cs
src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/Shared/Topology/RouteListResultBuilderTests.cs
src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Topology/RouteListTopologyIntegrationTests.cs
```

The Route List result contract remains unchanged. Its focused tests, the
layer-boundary tests, and the presentation-invariant tests are included in the
verification gate.

#### `Shell/Presentation/Shared/Rendering -> WorkspaceSelectionWireVocabulary.cs`

The whole-tree preflight search for the type name and its only member,
`WorkspaceSelectionWireVocabulary.Read`, found no production consumer. The only
current reference outside its definition was
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Shell/Presentation/Shared/Rendering/WorkspaceSelectionWireVocabularyTests.cs:15`.
The maintainer ruling was to delete the now-unconsumed helper and its test.
The serialization characterization gate remains part of the verification
receipt.

### Keep — 289 folders

The following members are the complete Keep set. The group reason applies to
every member in that group, and the file name after `->` is the one current C#
file in the folder.

#### Commands — 137 folders

These folders stay with their nearest command owner. `Models` topics separate
binding, request, selection, planning, effects, result, and other distinct
questions; `Shared/<capability>` folders contain command-local collaborators
with named ownership. Flattening them would either mix different questions or
promote a command detail across a layer. The `Commands/Library/**` and
`Commands/Route/Remove/**` rows are branch-point observations only and must be
rechecked after the other workers' behavior fixes merge.

```text
Commands/Cleanup/Models/Binding -> CleanupBindingModels.cs
Commands/Cleanup/Models/Request -> CleanupRequest.cs
Commands/Cleanup/Shared/Planning -> CleanupPlanner.cs
Commands/Context/Models/Request -> ContextRequest.cs
Commands/Context/Models/Selection -> ContextSelectionModels.cs
Commands/Context/Shared/Graph -> ContextGraphBuilder.cs
Commands/Context/Shared/Projection -> ContextProjectionBuilder.cs
Commands/Context/Shared/Links/Models -> ContextLinkEvidence.cs
Commands/Doctor/Models/Binding -> DoctorBindingModels.cs
Commands/Doctor/Models/Request -> DoctorRequest.cs
Commands/Extension/Models -> ExtensionPermissionStage.cs
Commands/Extension/Create/Models/Binding -> ExtensionCreateBindingModels.cs
Commands/Extension/Create/Models/Interaction -> ExtensionCreateInteraction.cs
Commands/Extension/Create/Models/Manifest -> ExtensionCreateManifest.cs
Commands/Extension/Create/Models/Operation -> ExtensionCreateOperationModels.cs
Commands/Extension/Create/Models/Request -> ExtensionCreateRequest.cs
Commands/Extension/Create/Models/Resolution -> ExtensionCreateResolutionModels.cs
Commands/Extension/Create/Models/Result -> ExtensionCreateResult.cs
Commands/Extension/Create/Shared/Application -> ExtensionCreateDestinationWriter.cs
Commands/Extension/Create/Shared/Manifest -> ExtensionCreateManifestBuilder.cs
Commands/Extension/Create/Shared/Resolution -> ExtensionCreateRequestResolver.cs
Commands/Extension/Create/Shared/Result -> ExtensionCreateResultFactory.cs
Commands/Extension/Inspect/Models/Binding -> ExtensionInspectBindingModels.cs
Commands/Extension/Inspect/Models/Request -> ExtensionInspectRequest.cs
Commands/Extension/Inspect/Shared/Reading -> ExtensionInspectCurrentPathReader.cs
Commands/Extension/Install/Models/Binding -> ExtensionInstallBindingModels.cs
Commands/Extension/Install/Models/Interaction -> ExtensionInstallInteraction.cs
Commands/Extension/Install/Models/Request -> ExtensionInstallRequest.cs
Commands/Extension/Install/Models/Result -> ExtensionInstallResult.cs
Commands/Extension/Install/Shared/Result -> ExtensionInstallResultFactory.cs
Commands/Extension/Remove/Models/Binding -> ExtensionRemoveBindingModels.cs
Commands/Extension/Remove/Models/Interaction -> ExtensionRemoveInteraction.cs
Commands/Extension/Remove/Models/Request -> ExtensionRemoveRequest.cs
Commands/Extension/Remove/Models/Selection -> ExtensionRemoveSelection.cs
Commands/Extension/Update/Models/Binding -> ExtensionUpdateBindingModels.cs
Commands/Extension/Update/Models/Effects -> ExtensionUpdateEffectModels.cs
Commands/Extension/Update/Models/Interaction -> ExtensionUpdateInteraction.cs
Commands/Extension/Update/Models/Request -> ExtensionUpdateRequest.cs
Commands/Extension/Update/Models/Selection -> ExtensionUpdateSelection.cs
Commands/Extension/Update/Models/Planning/Reconciliation -> ExtensionUpdateReconciliationModels.cs
Commands/Find/Models/Documents -> FindDocumentInspectionModels.cs
Commands/Find/Models/Presentation -> FindPresentationModels.cs
Commands/Find/Models/Query -> FindQueryModels.cs
Commands/Find/Models/Request -> FindRequest.cs
Commands/Find/Models/Selection -> FindSelectionModels.cs
Commands/Find/Shared/Application -> FindSourceResolver.cs
Commands/Find/Shared/Query -> FindQueryParser.cs
Commands/Find/Shared/Tags -> FindTagGrammar.cs
Commands/Index/Models/Binding -> IndexBindingModels.cs
Commands/Index/Models/Projection -> IndexProjectionFormation.cs
Commands/Index/Models/Request -> IndexRequest.cs
Commands/Index/Shared/Binding -> IndexBindingInputReader.cs
Commands/Index/Shared/Planning -> IndexPlanBuilder.cs
Commands/Index/Shared/Result -> IndexResultBuilder.cs
Commands/Install/Models/Binding -> InstallBindingModels.cs
Commands/Install/Models/Presentation -> InstallJsonDocument.cs
Commands/Install/Models/Request -> InstallRequest.cs
Commands/Install/Shared/Binding -> InstallBindingInputReader.cs
Commands/Install/Shared/Result -> InstallResultFactsFactory.cs
Commands/Library/Attach/Models/Application -> LibraryAttachApplicationInput.cs
Commands/Library/Attach/Models/Presentation -> LibraryAttachJsonDocument.cs
Commands/Library/Attach/Models/Request -> LibraryAttachRequest.cs
Commands/Library/Attach/Shared/Application -> LibraryAttachApplication.cs
Commands/Library/Attach/Shared/Binding -> LibraryAttachRequestBinder.cs
Commands/Library/Attach/Shared/Completion -> LibraryAttachCompletion.cs
Commands/Library/Attach/Shared/Planning -> LibraryAttachPlanner.cs
Commands/Library/Attach/Shared/Serialization -> LibraryAttachFindingCodeConverter.cs
Commands/Library/Detach/Models/Application -> LibraryDetachApplicationInput.cs
Commands/Library/Detach/Models/Presentation -> LibraryDetachJsonDocument.cs
Commands/Library/Detach/Models/Request -> LibraryDetachRequest.cs
Commands/Library/Detach/Shared/Application -> LibraryDetachApplication.cs
Commands/Library/Detach/Shared/Binding -> LibraryDetachRequestBinder.cs
Commands/Library/Detach/Shared/Completion -> LibraryDetachCompletion.cs
Commands/Library/Detach/Shared/Planning -> LibraryDetachPlanner.cs
Commands/Library/Detach/Shared/Serialization -> LibraryDetachFindingCodeConverter.cs
Commands/Library/Inspect/Models/Presentation -> LibraryInspectJsonDocument.cs
Commands/Library/Inspect/Models/Request -> LibraryInspectRequest.cs
Commands/Library/Inspect/Shared/Binding -> LibraryInspectRequestBinder.cs
Commands/Library/Inspect/Shared/Comparison/Models -> LibraryInspectComparisonRead.cs
Commands/Library/List/Models/Presentation -> LibraryListJsonDocument.cs
Commands/Library/List/Models/Request -> LibraryListRequest.cs
Commands/Library/List/Shared/Binding -> LibraryListRequestBinder.cs
Commands/Library/Models/Request -> LibraryMode.cs
Commands/Library/Models/Result/Coordinates/Effects -> LibraryEffectsValues.cs
Commands/Library/Models/Result/Coordinates/Observation -> LibraryObservationValues.cs
Commands/Library/Sync/Models/Application -> LibrarySyncApplicationInput.cs
Commands/Library/Sync/Models/Presentation -> LibrarySyncJsonDocument.cs
Commands/Library/Sync/Models/Request -> LibrarySyncRequest.cs
Commands/Library/Sync/Shared/Application -> LibrarySyncApplication.cs
Commands/Library/Sync/Shared/Binding -> LibrarySyncRequestBinder.cs
Commands/Library/Sync/Shared/Completion -> LibrarySyncCompletion.cs
Commands/Library/Sync/Shared/Planning -> LibrarySyncPlanner.cs
Commands/Library/Sync/Shared/Serialization -> LibrarySyncFindingCodeConverter.cs
Commands/References/Models/Occurrence -> ReferencesOccurrenceModels.cs
Commands/References/Models/Request -> ReferencesRequest.cs
Commands/References/Models/Resolution -> ReferencesResolutionModels.cs
Commands/References/Models/Selection -> ReferencesSelectionModels.cs
Commands/References/Models/Source -> ReferencesSourceModels.cs
Commands/References/Shared/Documents -> ReferencesMarkdownParser.cs
Commands/References/Shared/Extraction -> ReferencesLinkExtractor.cs
Commands/References/Shared/Inspection -> ReferencesLayerInspector.cs
Commands/Repair/Models/Binding -> RepairSymbols.cs
Commands/Repair/Models/Diagnosis -> RepairCatalogueRead.cs
Commands/Repair/Shared/Request -> RepairRelinkNormalizer.cs
Commands/Route/Create/Models/Request -> RouteCreateRequest.cs
Commands/Route/Create/Shared/Result -> RouteCreateResultBuilder.cs
Commands/Route/Init/Models/Operation -> RouteInitApplicationOutcome.cs
Commands/Route/Init/Models/Planning -> RouteInitPlan.cs
Commands/Route/Init/Models/Request -> RouteInitRequest.cs
Commands/Route/Init/Shared/Result -> RouteInitResultBuilder.cs
Commands/Route/Inspect/Models/Interaction -> RouteInspectSourceSelectionQuestion.cs
Commands/Route/Inspect/Shared/Interaction -> RouteInspectSourceSelector.cs
Commands/Route/Move/Models/Interaction -> RouteMoveSourceSelectionQuestion.cs
Commands/Route/Move/Models/Request -> RouteMoveRequest.cs
Commands/Route/Move/Shared/Result -> RouteMoveResultBuilder.cs
Commands/Route/Remove/Models/Request -> RouteRemoveRequest.cs
Commands/Route/Remove/Shared/Result -> RouteRemoveResultBuilder.cs
Commands/Route/Shared/Binding -> RouteOptionValidation.cs
Commands/Route/Shared/Filesystem -> RouteCategoryFilesystemReader.cs
Commands/Route/Shared/Navigation -> RouteNavigationExposureReader.cs
Commands/Route/Shared/Ownership -> RouteOwnershipEvidence.cs
Commands/Route/Shared/Models/Navigation -> RouteNavigationExposure.cs
Commands/Route/Shared/Models/References -> RouteMarkdownCatalogueModels.cs
Commands/Route/Update/Models/Interaction -> RouteUpdateSourceSelectionQuestion.cs
Commands/Route/Update/Models/Request -> RouteUpdateRequest.cs
Commands/Route/Update/Shared/Result -> RouteUpdateResultBuilder.cs
Commands/Shared/Permissions/Models -> WorkspacePermissionJson.cs
Commands/Status/Models/Binding -> StatusBindingModels.cs
Commands/Status/Models/Operation -> StatusObservationSet.cs
Commands/Status/Models/Request -> StatusRequest.cs
Commands/Update/Models/Binding -> UpdateBindingModels.cs
Commands/Update/Models/Comparison -> UpdateComparisonModels.cs
Commands/Update/Models/Effects -> UpdatePhysicalEffectModels.cs
Commands/Update/Models/Request -> UpdateRequest.cs
Commands/Update/Models/Result -> UpdateResultModels.cs
Commands/Update/Shared/Result -> UpdateResultBuilder.cs
Commands/Update/Shared/Validation -> UpdateValueSyntax.cs
```

The Route Move and Route Remove members in that list are deliberately local.
Their bodies may coincide, but their published finding-code sets differ by
destination behavior; this inventory does not lift either formation. The
Library members are likewise a branch-point map only; the overseer must recheck
their rows after the parallel `Commands/Library/**` behavior lane merges.

#### Framework — 28 folders

These are Framework-owned capabilities, value objects, source-generated JSON
contexts, or safety/state boundaries. A move into a broader shared bucket would
weaken the `Framework` dependency direction or obscure an accepted operational
boundary. In particular, lifecycle, permission, recovery, settings, and
ownership internals stay available for Task 30's state work and are not M5
cleanup candidates.

```text
Framework/Serialization -> JsonDuplicatePropertyValidator.cs
Framework/Distribution/Models/Content -> FrameworkManagedBlockRecognition.cs
Framework/Distribution/Shared/Content -> FrameworkContentIdentity.cs
Framework/Extensions/Identity -> ExtensionIdentity.cs
Framework/Extensions/Serialization -> ExtensionPackageJsonContext.cs
Framework/Filesystem/LogicalPaths/Models -> CanonicalRelativePath.cs
Framework/Filesystem/Models/Reading -> ManagedTargetReadResult.cs
Framework/Filesystem/Shared/Paths -> PortableWorkspacePath.cs
Framework/Filesystem/Shared/Reading -> ManagedTargetReader.cs
Framework/Libraries/Shared/Permissions -> LibraryRecoveryPermissionReader.cs
Framework/Libraries/Shared/Source -> LibrarySourceRootReader.cs
Framework/Ownership/Models/Observation -> WorkspaceOwnershipRead.cs
Framework/Ownership/Shared/Observation -> WorkspaceOwnershipReader.cs
Framework/Recovery/Comparison -> RecoveryEntryComparer.cs
Framework/Recovery/Operational/Models -> RecoveryResidualOperationalModels.cs
Framework/Recovery/Shared/Deletion/Models -> RecoveryBundleDeletionSessionOpenResult.cs
Framework/Settings/Models/Document -> WorkspaceSettingsDocument.cs
Framework/Settings/Models/Mutation -> WorkspaceSettingsWrite.cs
Framework/Settings/Models/Observation -> WorkspaceSettingsRead.cs
Framework/Settings/Shared/Completion -> WorkspacePermissionReceiptProjection.cs
Framework/Settings/Shared/Observation -> WorkspaceSettingsReader.cs
Framework/Settings/Shared/Planning -> WorkspaceAllowList.cs
Framework/Settings/Shared/Serialization -> WorkspaceSettingsCodec.cs
Framework/Sources/Inventory -> SourceCatalogueReader.cs
Framework/Sources/Locations -> Utf8SourceMap.cs
Framework/Sources/Selection -> SourceUniverseFilterResolver.cs
Framework/Sources/Models/Locations -> SourceLocation.cs
Framework/Sources/Shared/Destinations -> SourceDestinationDecoder.cs
```

#### Presentation — 123 folders

All of these are intentional G4 command-local presentation seams. The native
shape is `Presentation/<Command>/Models`, with `Shared/Help`,
`Shared/Selection`, and `Shared/Wording` where that command needs them, plus
the explicitly named `Shared/Interaction` or `Shared/Prompts` topics. Each
folder answers a presentation question for its owning command and is guarded
by the Presentation layer invariants; flattening it would make ownership less
clear. The three `Presentation/Legacy` folders are the remaining
composer-bound `*HelpSections.cs` files and are deliberately left alone because
G4 just settled that seam.

```text
Presentation/Cleanup/Models -> CleanupData.cs
Presentation/Cleanup/Shared/Help -> CleanupHelpSections.cs
Presentation/Cleanup/Shared/Selection -> CleanupReportSelector.cs
Presentation/Cleanup/Shared/Wording -> CleanupWording.cs
Presentation/Context/Models -> ContextData.cs
Presentation/Context/Shared/Help -> ContextHelpSections.cs
Presentation/Context/Shared/Selection -> ContextReportSelector.cs
Presentation/Context/Shared/Wording -> ContextWording.cs
Presentation/Doctor/Models -> DoctorData.cs
Presentation/Doctor/Shared/Help -> DoctorHelpSections.cs
Presentation/Doctor/Shared/Selection -> DoctorReportSelector.cs
Presentation/Doctor/Shared/Wording -> DoctorWording.cs
Presentation/Extension/Create/Models -> ExtensionCreateData.cs
Presentation/Extension/Create/Shared/Help -> ExtensionCreateHelpSections.cs
Presentation/Extension/Create/Shared/Wording -> ExtensionCreateWording.cs
Presentation/Extension/Inspect/Models -> ExtensionInspectData.cs
Presentation/Extension/Inspect/Shared/Help -> ExtensionInspectHelpSections.cs
Presentation/Extension/Inspect/Shared/Selection -> ExtensionInspectReportSelector.cs
Presentation/Extension/Inspect/Shared/Wording -> ExtensionInspectWording.cs
Presentation/Extension/Install/Models -> ExtensionInstallData.cs
Presentation/Extension/Install/Shared/Help -> ExtensionInstallHelpSections.cs
Presentation/Extension/Install/Shared/Selection -> ExtensionInstallReportSelector.cs
Presentation/Extension/Install/Shared/Wording -> ExtensionInstallWording.cs
Presentation/Extension/List/Models -> ExtensionListData.cs
Presentation/Extension/List/Shared/Help -> ExtensionListHelpSections.cs
Presentation/Extension/List/Shared/Selection -> ExtensionListReportSelector.cs
Presentation/Extension/List/Shared/Wording -> ExtensionListWording.cs
Presentation/Extension/Remove/Models -> ExtensionRemoveData.cs
Presentation/Extension/Remove/Shared/Help -> ExtensionRemoveHelpSections.cs
Presentation/Extension/Remove/Shared/Selection -> ExtensionRemoveReportSelector.cs
Presentation/Extension/Remove/Shared/Wording -> ExtensionRemoveWording.cs
Presentation/Extension/Update/Models -> ExtensionUpdateData.cs
Presentation/Extension/Update/Shared/Help -> ExtensionUpdateHelpSections.cs
Presentation/Extension/Update/Shared/Selection -> ExtensionUpdateReportSelector.cs
Presentation/Extension/Update/Shared/Wording -> ExtensionUpdateWording.cs
Presentation/Find/Models -> FindData.cs
Presentation/Find/Shared/Help -> FindHelpSections.cs
Presentation/Find/Shared/Selection -> FindReportSelector.cs
Presentation/Find/Shared/Wording -> FindWording.cs
Presentation/Index/Models -> IndexData.cs
Presentation/Index/Shared/Help -> IndexHelpSections.cs
Presentation/Index/Shared/Selection -> IndexReportSelector.cs
Presentation/Index/Shared/Wording -> IndexWording.cs
Presentation/Install/Models -> InstallData.cs
Presentation/Install/Shared/Help -> InstallHelpSections.cs
Presentation/Install/Shared/Prompts -> InstallPlanConfirmationQuestion.cs
Presentation/Install/Shared/Wording -> InstallWording.cs
Presentation/Legacy/Extension/Shared/Rendering -> ExtensionHelpSections.cs
Presentation/Legacy/Library/Shared/Rendering -> LibraryHelpSections.cs
Presentation/Legacy/Route/Shared/Rendering -> RouteHelpSections.cs
Presentation/Library/Attach/Models -> LibraryAttachData.cs
Presentation/Library/Attach/Shared/Help -> LibraryAttachHelpSections.cs
Presentation/Library/Attach/Shared/Interaction -> LibraryAttachInteractionPresentation.cs
Presentation/Library/Attach/Shared/Selection -> LibraryAttachReportSelector.cs
Presentation/Library/Attach/Shared/Wording -> LibraryAttachWording.cs
Presentation/Library/Detach/Models -> LibraryDetachData.cs
Presentation/Library/Detach/Shared/Help -> LibraryDetachHelpSections.cs
Presentation/Library/Detach/Shared/Interaction -> LibraryDetachInteractionPresentation.cs
Presentation/Library/Detach/Shared/Selection -> LibraryDetachReportSelector.cs
Presentation/Library/Detach/Shared/Wording -> LibraryDetachWording.cs
Presentation/Library/Inspect/Models -> LibraryInspectData.cs
Presentation/Library/Inspect/Shared/Help -> LibraryInspectHelpSections.cs
Presentation/Library/Inspect/Shared/Selection -> LibraryInspectReportSelector.cs
Presentation/Library/Inspect/Shared/Wording -> LibraryInspectWording.cs
Presentation/Library/List/Models -> LibraryListData.cs
Presentation/Library/List/Shared/Help -> LibraryListHelpSections.cs
Presentation/Library/List/Shared/Selection -> LibraryListReportSelector.cs
Presentation/Library/List/Shared/Wording -> LibraryListWording.cs
Presentation/Library/Sync/Models -> LibrarySyncData.cs
Presentation/Library/Sync/Shared/Help -> LibrarySyncHelpSections.cs
Presentation/Library/Sync/Shared/Interaction -> LibrarySyncInteractionPresentation.cs
Presentation/Library/Sync/Shared/Selection -> LibrarySyncReportSelector.cs
Presentation/Library/Sync/Shared/Wording -> LibrarySyncWording.cs
Presentation/References/Models -> ReferencesData.cs
Presentation/References/Shared/Help -> ReferencesHelpSections.cs
Presentation/References/Shared/Wording -> ReferencesWording.cs
Presentation/Repair/Models -> RepairData.cs
Presentation/Repair/Shared/Help -> RepairHelpSections.cs
Presentation/Repair/Shared/Selection -> RepairReportSelector.cs
Presentation/Repair/Shared/Wording -> RepairWording.cs
Presentation/Route/Create/Models -> RouteCreateData.cs
Presentation/Route/Create/Shared/Help -> RouteCreateHelpSections.cs
Presentation/Route/Create/Shared/Wording -> RouteCreateWording.cs
Presentation/Route/Init/Models -> RouteInitData.cs
Presentation/Route/Init/Shared/Help -> RouteInitHelpSections.cs
Presentation/Route/Init/Shared/Wording -> RouteInitWording.cs
Presentation/Route/Inspect/Models -> RouteInspectData.cs
Presentation/Route/Inspect/Shared/Help -> RouteInspectHelpSections.cs
Presentation/Route/Inspect/Shared/Interaction -> RouteInspectSourceSelectionPrompt.cs
Presentation/Route/Inspect/Shared/Selection -> RouteInspectReportSelector.cs
Presentation/Route/Inspect/Shared/Wording -> RouteInspectWording.cs
Presentation/Route/List/Models -> RouteListData.cs
Presentation/Route/List/Shared/Help -> RouteListHelpSections.cs
Presentation/Route/List/Shared/Selection -> RouteListReportSelector.cs
Presentation/Route/List/Shared/Wording -> RouteListWording.cs
Presentation/Route/Move/Models -> RouteMoveData.cs
Presentation/Route/Move/Shared/Help -> RouteMoveHelpSections.cs
Presentation/Route/Move/Shared/Interaction -> RouteMoveSourceSelectionPrompt.cs
Presentation/Route/Move/Shared/Wording -> RouteMoveWording.cs
Presentation/Route/Remove/Models -> RouteRemoveData.cs
Presentation/Route/Remove/Shared/Help -> RouteRemoveHelpSections.cs
Presentation/Route/Remove/Shared/Interaction -> RouteRemovePrompts.cs
Presentation/Route/Remove/Shared/Selection -> RouteRemoveReportSelector.cs
Presentation/Route/Remove/Shared/Wording -> RouteRemoveWording.cs
Presentation/Route/Shared/Wording -> RouteSourceSelectionWording.cs
Presentation/Route/Update/Models -> RouteUpdateData.cs
Presentation/Route/Update/Shared/Help -> RouteUpdateHelpSections.cs
Presentation/Route/Update/Shared/Interaction -> RouteUpdateSourceSelectionPrompt.cs
Presentation/Route/Update/Shared/Wording -> RouteUpdateWording.cs
Presentation/Shared/Help -> CliHelpRenderer.cs
Presentation/Shared/Content/Models -> CliContentBlock.cs
Presentation/Shared/Prompts/Models -> PlanConfirmationRequest.cs
Presentation/Shared/Selection/Models -> CliSelection.cs
Presentation/Shared/Text/Models -> CliAuthoredSpan.cs
Presentation/Status/Models -> StatusData.cs
Presentation/Status/Shared/Help -> StatusHelpSections.cs
Presentation/Status/Shared/Selection -> StatusReportSelector.cs
Presentation/Status/Shared/Wording -> StatusWording.cs
Presentation/Update/Models -> UpdateData.cs
Presentation/Update/Shared/Help -> UpdateHelpSections.cs
Presentation/Update/Shared/Prompts -> UpdatePlanConfirmationQuestion.cs
Presentation/Update/Shared/Selection -> UpdateReportSelector.cs
Presentation/Update/Shared/Wording -> UpdateWording.cs
```

The `Presentation/Library/**` and `Presentation/Route/Remove/**` entries are
also branch-point rows associated with the parallel behavior lanes. Recheck
them after those lanes merge; this inventory proposes no presentation move.

#### Shell — 1 folder

```text
Shell/Serialization -> CliJsonContext.cs
```

Keep this source-generated `JsonSerializerContext` registration at the Shell
serialization boundary. Its direct integration evidence is
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Serialization/GeneratedSerializationTests.cs:23`.
Moving it into a generic JSON folder would blur the AOT/generated-code
boundary; changing it would require serialization characterization and the
supported AOT gate.

## Oversized files: seam or no seam

For reproducibility, “oversized” here means **600 or more physical lines** in a
Core `.cs` file at this branch point. There are 22 such files. A genuine seam
means that a new owner would answer a distinct question; line count alone never
creates one. The verdict is explicit for every file at or above the cutoff.

### Genuine semantic seam — one file

#### `Commands/Cleanup/Models/Result/CleanupResultFacts.cs` — 1,019 lines — Yes, bounded seam

This file contains separately named top-level fact families. The type starts
that establish the boundaries are `CleanupWorkspaceAssociation:12`,
`CleanupLeaseBoundary:219`, `CleanupRecoveryProvenance:330`,
`CleanupVerificationCondition:343`, `CleanupEffectCondition:469`,
`CleanupEffectFactsValidation:541`, `CleanupPreflight:603`, `CleanupLease:610`,
`CleanupCatalogueComparison:617`, `CleanupEffect:628`, `CleanupResidual:680`,
`CleanupVerification:730`, `CleanupFinding:737`, and the aggregate
`CleanupResultFacts:789`. The first thirteen types describe reusable cleanup
facts and validation; the aggregate is the command result contract.

A later, separately reviewed split may use the same namespace
`OpenForge.Cli.Core.Commands.Cleanup.Models.Result` and three cohesive files:
`CleanupWorkspaceAndLeaseFacts.cs` for workspace association and lease
boundaries, `CleanupEffectFacts.cs` for recovery/verification/effect conditions
and their validator, and `CleanupResultFacts.cs` for the preflight, result
coordinates, finding, and aggregate. This is a proposed semantic split, not a
permission to create one file per record or a generic facts bucket. The split
is acceptable only if each file still answers a distinct question, all types
remain internal/local, and the dependency graph stays within the Cleanup
command.

The direct Core consumers to re-check are
`Commands/Cleanup/CleanupOperation.cs`,
`Commands/Cleanup/Shared/Application/CleanupApplicationOperation.cs`,
`Commands/Cleanup/Shared/Result/CleanupResultBuilder.cs`,
`Commands/Cleanup/Shared/Planning/CleanupPlanner.cs`, and
`Presentation/Cleanup/Shared/Selection/CleanupReportSelector.cs`. The
characterization tests are
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Cleanup/CleanupResultContractTests.cs`,
`CleanupPlanningContractTests.cs`, and `CleanupPresentationContractTests.cs`,
plus
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Cleanup/CleanupBeforeOutputSnapshotTests.cs`.
No namespace or project-reference change is necessary for this proposed
same-namespace split. Run those focused tests, the layer-boundary tests, and
the presentation invariants before considering the seam applied.

### No genuine semantic seam — keep the file whole

The following 21 files are large because the subject they own is large. Each
already has one coherent owner; splitting by method size, detail level, or
serialization format would either hide invariants or create a generic utility
bag. No source split is proposed for any of them.

| File | Lines | Seam verdict and reason |
| --- | ---: | --- |
| `Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs` | 849 | No. One command plan owner answers what constitutes a valid extension-update plan; it already composes topology and reconciliation collaborators. |
| `Presentation/Library/Attach/Shared/Selection/LibraryAttachReportSelector.cs` | 830 | No. One G4 selector answers which Attach facts are exposed at the selected detail/filter; helper groups are the same presentation question. Recheck after the Library behavior lane merges. |
| `Presentation/Route/Move/Shared/Selection/RouteMoveReportSelector.cs` | 825 | No. One G4 selector owns Move report selection; splitting it risks changing destination-sensitive output. Keep local and recheck after the Route lane merges. |
| `Presentation/Extension/Install/Shared/Selection/ExtensionInstallReportSelector.cs` | 810 | No. One command selector owns Install report selection and its accepted detail behavior. |
| `Commands/Repair/Shared/Application/RepairLibraryRecoveryApplication.cs` | 809 | No. One application boundary owns preflight, prepare/apply, verify, and completion; splitting would hide recovery and lease invariants. |
| `Presentation/Extension/Update/Shared/Selection/ExtensionUpdateReportSelector.cs` | 759 | No. One G4 selector answers the Extension Update presentation question; detail helpers are not new owners. |
| `Presentation/Repair/Shared/Selection/RepairReportSelector.cs` | 724 | No. One selector owns Repair's presentation projection, including its findings and effects. |
| `Presentation/Doctor/Shared/Wording/DoctorWording.cs` | 700 | No. One wording vocabulary owns the command's many status/finding phrases; it is large because Doctor's subject is large. |
| `Presentation/Library/Sync/Shared/Selection/LibrarySyncReportSelector.cs` | 684 | No. One G4 selector owns Sync report selection. Recheck after the Library behavior lane merges. |
| `Presentation/Status/Shared/Selection/StatusReportSelector.cs` | 671 | No. One selector answers which Status observations are rendered; splitting by severity would duplicate that answer. |
| `Presentation/Update/Shared/Selection/UpdateReportSelector.cs` | 666 | No. One selector owns Update's detail/filter projection and public report shape. |
| `Presentation/Library/Detach/Shared/Selection/LibraryDetachReportSelector.cs` | 662 | No. One G4 selector owns Detach report selection. Recheck after the Library behavior lane merges. |
| `Framework/Sources/References/SourceLinkDestinationResolver.cs` | 636 | No. One Framework capability resolves source-link destinations, including URI, fragment, percent, path, and safety rules; parser fragments would be internal helpers, not distinct owners. |
| `Presentation/Install/Shared/Selection/InstallReportSelector.cs` | 624 | No. One selector owns Install's report projection and accepted output choices. |
| `Commands/Update/Shared/Planning/UpdateIntendedStateBuilder.cs` | 620 | No. One planning owner answers intended-versus-current target state, fingerprints, generated boundaries, and retirement. |
| `Presentation/Route/Remove/Shared/Selection/RouteRemoveReportSelector.cs` | 617 | No. One G4 selector owns Remove report selection; do not lift its finding/result formation. Recheck after the Route lane merges. |
| `Commands/Library/Shared/Completion/LibraryMutationCompletionProjection.cs` | 615 | No. One shared Library completion projection owns the accepted mutation-to-report projection; lifecycle and permission internals remain Task 30 work. Recheck after the Library behavior lane merges. |
| `Commands/Extension/Update/Shared/Planning/ExtensionUpdateReconciler.cs` | 615 | No. One reconciliation algorithm owns package-path, retired-path, expected-state, and fingerprint invariants; its staging types are not new public seams. |
| `Commands/Extension/Update/Shared/Application/ExtensionUpdateApplicationOperation.cs` | 610 | No. One application stage owns lock, revalidation, prepare/apply, verify, and recovery. |
| `Presentation/Status/Shared/Wording/StatusWording.cs` | 605 | No. One Status wording vocabulary owns its command phrases and delegates typed value states to the accepted shared report vocabulary. |
| `Commands/Install/Shared/Operation/InstallApplicationOperation.cs` | 603 | No. One Install application protocol owns preconditions, lease, effects, verification, and outcome. |

The other large files below 600 lines were not proposed as seams: they remain
covered by their one-owner folders and would need the same distinct-question
test before any later proposal.

## Explicit exclusions

These are out of scope, and this map would be an error if it touched them:

- Route Move and Route Remove finding codes, request formations, and result
  formations stay local. Their bodies coincide, but their published
  finding-code sets differ by destination behavior; this record proposes no
  lift, shared formation, or cross-command target.
- Lifecycle and permission internals belong to Task 30's state work. The
  Framework and Library completion rows above are Keeps, not invitations to
  reorganize those internals.
- No public finding code, JSON, exit code, or stream behavior may change. M5 is
  behavior-preserving by definition. A namespace or file split is acceptable
  only when characterization output is unchanged.
- Source-generated contexts and other generated-code boundaries are not generic
  cleanup targets. `Shell/Serialization/CliJsonContext.cs` and
  `Framework/Extensions/Serialization/ExtensionPackageJsonContext.cs` remain
  at their owners.

## Steps — application and verification record

Each batch below is independently verifiable. The order starts with the
smallest blast radius and leaves unresolved questions out of source changes.

1. **Re-baseline at the inventory branch point.** The leaf-folder count,
   namespace/path audit, and behavior characterization were established before
   application. The `Commands/Library/**` and `Commands/Route/Remove/**` rows
   remain branch-point observations for re-check after those lanes merge.
2. **Flatten Extension List models (applied).** The three explicitly listed
   files were moved into `Commands/Extension/List/Models`, their three
   namespace declarations and named `using` references were updated, and the
   target namespace was kept as
   `OpenForge.Cli.Core.Commands.Extension.List.Models`. Stale namespace
   references were checked with `rg`; the focused tests, complete managed
   gates, and characterization checks passed. This batch changed no project
   reference and no public contract.
3. **Apply the Cleanup seam only if accepted.** Split
   `CleanupResultFacts.cs` into the three named cohesive same-namespace files
   only after the owner accepts the distinct-question boundary. Run all
   Cleanup contract tests, the Cleanup integration snapshot test,
   `LayerBoundaryTests`, and `CliReportInvariantsTests`; compare finding,
   JSON, exit, and stream characterization. Because the split is internal and
   same-namespace, no project-reference change is expected. Do not create a
   per-record folder set.
4. **Resolve the Route List question separately (applied).** The maintainer
   approved co-location of the root result with its declared
   `Models/Result` namespace. The file was moved without namespace, using, or
   consumer edits; Route List tests, boundary/invariant checks, and the
   serialization characterization passed.
5. **Close with the M5 gate (completed).** The complete managed test gate and
   whitespace verification were run after the accepted moves and deletions.
   The dependency graph still points inward through Shell → Framework →
   Commands → Presentation, no prohibited project or layer reference was
   introduced, and public finding codes, JSON, exit codes, streams, and output
   strings remained unchanged.

The 289 Keep rows are not an application batch. They are deliberate no-op
dispositions. In particular, do not tidy the newly settled G4 Presentation
shape, the three `Presentation/Legacy` files, Framework state/safety folders,
Shell generated serialization, Route Move/Remove formation, or the Library
behavior lanes as part of M5.

## Expected result

The approved M5 source changes are applied with no behavior changes. The three
Extension List model folders are flattened into their parent namespace, the
Route List result is co-located with its declared namespace, and the unconsumed
Shell helper and test are deleted. All other one-file folders retain their
documented owner, layer, namespace, project/generated boundary, or test-impact
reason to stay where they are.

## Acceptance

- [x] The counting rule is stated, the branch-point number is reported, and the
  difference from 176 is explained.
- [x] Every one-file leaf folder is listed exactly once in the disposition map:
  289 Keep, 3 Collapse (applied), and 2 former decision gates (resolved and
  applied), for 294 total.
- [x] Every proposed oversized split has an explicit seam verdict; only
  `CleanupResultFacts.cs` has a bounded semantic split proposal, and all other
  files examined are explicitly No.
- [x] Ownership, layer, namespace, project-reference, generated-code, and test
  impact are recorded for the proposed Extension List move and each decision
  gate.
- [x] Route Move/Remove formation, lifecycle/permission internals, and public
  finding/JSON/exit/stream behavior are explicitly excluded.
- [x] Independent application batches and the layer-boundary tests guarding
  them are specified.
- [x] The approved M5 moves, deletions, namespace updates, consumer updates,
  and snapshot-directory renames were applied under `src/cli/**` without
  changing public finding codes, JSON names, exit codes, streams, or output
  strings.

## Observations

- `Commands/Route/List/RouteListResult.cs:7` declared the
  `Commands.Route.List.Models.Result` namespace from the command root, while
  `Commands/Route/List/Models/Result/RouteListPresentationFacts.cs` was already
  physically in that namespace folder. The maintainer-approved pure move has
  corrected this locality defect without changing the file bytes or references.
- `Shell/Presentation/Shared/Rendering/WorkspaceSelectionWireVocabulary.cs`
  had no production consumer; its only external reference was its unit test.
  The maintainer-approved deletion removed both files, with serialization
  characterization retained as a gate.
- A static namespace-reference audit found no `Commands -> Presentation`,
  `Framework -> Commands/Shell`, or `Presentation -> Framework` violations at
  this branch point. The six Shell-to-Presentation references are the accepted
  report pipeline boundary. No source defect was fixed here.
- The sealed experience-audit records contain older structural proposals. G4's
  stabilized Presentation structure is the current decision boundary used by
  this map; those sealed proposals were not copied as current actions.

## Changes ledger

- **Applicability check:** this is local replacement-CLI maintenance. The
  affected data is source layout and test metadata; practical recovery is
  available through the overseer's Git staging plus reversible filesystem
  moves, while generated build output remains disposable. The supported
  failure boundary is ordinary editing, compilation, test, and filesystem
  failure; no external system, credential, or adversarial-process boundary is
  involved.
- **Capability and design:** ordinary filesystem moves/deletions, SDK default
  compile globs, and C# namespace imports provide the required capability.
  No new dependency, project, shared owner, serializer registration, or
  exceptional machinery was introduced. Extension List remains command-local;
  Route List remains in its declared result namespace; the dead Shell helper is
  deleted rather than reconnected.
- **Applied Change 1:** confirmed the helper type and its sole `Read` member
  had no production consumer, then deleted the helper and its unit test. The
  deleted test contributed two theory cases and one fact.
- **Applied Change 2:** moved `RouteListResult.cs` into
  `Commands/Route/List/Models/Result/` with filesystem `Move-Item`. Its
  namespace, usings, and bytes were unchanged; no other file was edited for
  this move.
- **Applied Change 3:** moved the three Extension List model files into
  `Commands/Extension/List/Models/`, changed their declarations to the target
  namespace, removed two redundant model usings, and updated 12 production
  consumer files plus 3 test files. The independent search found no consumer
  beyond those 12. The snapshot test now uses `MinimalText`, `StandardText`,
  `MinimalJson`, and `StandardJson`; the four matching directories were
  renamed without changing snapshot bytes.
- **Evidence selection:** the shared model/composition/serialization boundary
  required the complete managed Unit and Integration gates, plus focused
  snapshot, layer-boundary, report-invariant, and serialization checks. Results
  were Unit `3,176 / 0 / 0`, Integration `2,219 total / 0 failed / 17 skipped`,
  and whitespace `5` known pre-existing errors.

## Divergences observed

- **Planned figure:** 176 one-file folders in the phase packet. **Observed:**
  294 under the explicit leaf-folder rule above, with 123 in the settled G4
  Presentation tree. **Disposition:** retained the reproducible current scan
  and documented the alternative counts rather than changing the definition to
  match the old figure.
- **Planned work:** inventory only. **Observed:** the maintainer approved all
  three structural changes. **Disposition:** applied the two decision-gate
  outcomes, the Extension List collapse, its consumer/test namespace updates,
  and the snapshot vocabulary renames.
- **Inventory coverage:** the prior `src/cli/core/` search listed 11
  production consumers. **Observed:** the whole `src/cli/` search also found
  `src/cli/root/OpenForge.Cli/Composition/CliExtensionComposer.cs`, which
  imported the Binding and Result topic namespaces. **Disposition:** updated
  it; no consumer beyond the resulting 12 production files and 3 tests was
  found.
- **Build environment:** the prescribed build initially failed because NuGet
  could not read the redirected per-user config under `artifacts/`. An
  explicit `dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config
  --ignore-failed-sources -p:NuGetAudit=false`, followed by the same Release
  build with `--no-restore -m:1 -p:UseSharedCompilation=false`, passed with 0
  warnings and 0 errors. Scratch stayed under `artifacts/`.
- **Whitespace:** verification reports exactly the baseline five pre-existing
  findings: two in `ReferencesOperation.cs` and three existing long-line
  findings in `ExtensionListApplicationIntegrationTests.cs`. No formatting
  correction was made.

## Rollback

If rollback is required, restore the two deleted files from the overseer's
Git baseline, reverse the four filesystem source/snapshot moves, and restore
the prior Extension List topic namespace imports, declarations, test names,
display names, and snapshot directory names. Re-run the same focused and full
managed gates after rollback; no public output contract requires a migration.
