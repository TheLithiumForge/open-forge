---
open-forge:
  description: Open Task 85 to let a workspace keep its Open Forge knowledge in a folder other than .agents, chosen at install or moved later, selected to follow Task 86
  tags: [Memory, Working, Task, CLI, Install, Settings, Layout, Contextual]
---

# Task 85: Configurable main root

Status: Selected by the maintainer on 2026-10-23, to start after [Task 86](task86-workspace-runtime-lock.md). It was deferred on 2026-10-22 while `route include` covered reusing project documents.

## Outcome

A workspace can keep its Open Forge knowledge in a folder other than `.agents/`, such as `docs/forge/`. The folder is chosen during install or moved later through a reviewed plan. Agents, the CLI and the `AGENTS.md` and `CLAUDE.md` pointers all follow it, and a default workspace behaves exactly as before.

## Accepted design

The [route include council](../../../emerging/analysis/route-include-council/record.md#the-configurable-main-root-7) settled the design:

- `mainRoot` in settings schema v4 names where the knowledge lives. It defaults to `.agents`.
- `.agents/open-forge.json` and `.agents/open-forge.lock.json` stay where they are. Recovery bundles and the mutation lock stay keyed by the physical workspace.
- Native Skills stay at `.agents/skills/`, where hosts discover them. The custom loader can include their catalogue. No Skills category is created under the custom root.
- `AGENTS.md` and `CLAUDE.md` stay at the workspace root and point to the selected loader.
- `install --main-root <directory>` and a wizard question choose the folder. `install --configure --main-root <directory>` moves an existing workspace. Editing `mainRoot` by hand moves nothing, and writing commands stop until a configure plan resolves the mismatch.
- Migration inventories the real files, including ignored and untracked ones, checks for collisions and unsafe links, verifies copied bytes before removing originals, translates Install-owned ignore rules, keeps historical recovery paths literal and rewrites the links of files recorded in `roots`.

## Plan

The [main root council](../../../emerging/analysis/main-root-council/record.md) refreshed the design on 2026-10-23 and split it into two deliveries. Its [Delivery 1 plan](../../../emerging/analysis/main-root-council/plan.md) replaces the [2026-10-21 plan](../../../emerging/analysis/route-include-council/main-root-plan.md).

- **Delivery 1** is a custom context folder chosen at first installation, with every command supporting it. An established installation refuses a different root. The plan has the frozen layout API, settings v4 and seven waves of packets, and activates `--main-root` and the wizard question last. The estimate is 14 to 18 engineering days after Task 86.
- **Delivery 2** moves an existing workspace with `install --configure --main-root`. It starts after Delivery 1 ships, once the council's safety conditions are planned and tested. The estimate is 7 to 10 more days.
- This Task stays open until Delivery 2 ships, because moving an existing workspace is part of the accepted outcome.

## Before starting

- Start Delivery 1 only after [Task 86](task86-workspace-runtime-lock.md) merges, and reconcile the plan's line numbers against that baseline.
- Keep `.open-forge-state/` from Task 86 at the workspace root. A configured main root never moves it, and a main root can never be placed inside it.

## Wave 1 handoff

B1 separates source syntax from membership and derives IDs from the selected content location.
B2 carries Layout through catalogue requests, catalogues, sessions, filtering and loading closure.
Inventory reads the main root and native Skills, treating absent optional Skills correctly.
IncludedRows retains aliases, conditions, order and unresolved targets even for pre-inventoried sources.
The frozen WorkspaceLayout signatures remain available and loader destinations use the actual loader directory.
SourceContentRoots carries only logical roots, without a workspace or physical paths.
Its Default value supports legacy path-only adapters and must be removed in Z1.

Remaining default overloads to convert before activation:

- SourceIdentity, SourceFormClassifier and SourceWorkspaceRelativePath path-only overloads
- SourceLoaderDestinationParser, SourceLoaderEntriesParser and SourceGeneratedDestinationResolver
- SourceCatalogueRequest and SourceCatalogue workspace overloads
- SourceReadSessionReader workspace overloads and Find/References entry adapters
- Legacy membership callers using SourceContentRoots.Default
- Wave 0 WorkspaceRemovals.IsPathRemoved, WorkspaceSettingsCodec.IsValidRootPath and SeedArchiveLinkScope

W1-01 remains with Wave 2: navigation formation assumes every observed source has a candidate, so candidate-free includes can trigger KeyNotFoundException.
No new Wave 1 command path activates that existing case.

Follow-up solution build passed in Release with 0 warnings and 0 errors.
Complete Unit suite: total 5,245, passed 5,245, failed 0, skipped 0.
Complete Integration suite: total 3,752, passed 3,694, failed 0, skipped 58, all allowlisted.
The diff stays uncommitted. No snapshot baseline or public command activation is changed.

## Wave 2 C1 handoff

C1 carries layout through navigation, scaffolding, metadata, adoption, rename evidence and shared route transport. Sharing records are structural and catalogue consumers validate membership.
W1-01 is closed by real included-session tests. Formation preserves candidate-free sources, aliases and every included row without inventing candidates.
Z1 adapters remain in GeneratedNavigationRegionPlanner.PlanProjectedEntries, RouteScaffoldSourceBuilder.CanonicalPath/Create, RouteScaffoldRenamePlanner.Decide and the adoption Plan/Create/CreateEntrypoint overloads without layout.
Z1 adapters also remain in RoutedMetadataPolicyContext and RoutedMetadataDefaults.Tags, RouteSource and RouteSourceDocument legacy constructors, and RouteMarkdownCatalogueRequest's workspace constructor.
RenameEvidencePolicy.Roots, RenameRecordedEntryReader.Read, RenameCanonicalEntrypointEvidence.Read and the composition adapter's workspace ReadEvidenceAsync overloads retain default adapters for later command conversion.
The overseer owns C2 integration, later command conversion and fresh published/native and non-Windows qualification. C2 files and snapshot baselines are untouched.
Release build passed with zero warnings and errors. Unit: 5,252 total, 5,252 passed, 0 failed, 0 skipped. Integration: 3,757 total, 3,699 passed, 0 failed, 58 allowlisted skips. The diff remains uncommitted.

## Wave 2 C2 handoff

- C2 delivers mapped projection, exclusions, collisions and reverse alignment while retaining package-coordinate validation. Parsed links and recognized host references follow the layout, Claude retains `@AGENTS.md`, and reviewed README/CLI Skill variants describe both content folders, native Skills catalogues and generic `./...` operands. Default bytes and existing snapshots remain unchanged.
- Six temporary default-layout overloads remain: `EmbeddedFrameworkSourceProjector.Project`, `FrameworkPayloadSourceProjection.Create`, `FrameworkSourceAlignment.ReadAsset`, both `FrameworkPayloadSelection` methods and `WorkspacePayloadRenderer.Render`. Convert their callers before Z1 activation. `FrameworkPayloadAsset` must retain `SourceContentRoots.Default` as a package namespace invariant. Render callers supply the original package path and use mapped destinations for permissions and ownership.
- C2 final counts (total/passed/failed/skipped): Unit 5264/5264/0/0, Integration 3758/3700/0/58, published payload 2/2/0/0. All skips are allowlisted.

## Wave 3 D1 handoff

D1 carries the observed layout and catalogue identities through Context, Find and References.
Its default-session and Find result and selection adapters are removed. None remain in these command paths.
Three published custom-root journeys prove nested roots, native Skills, overwrite projections and byte preservation.

## Wave 3 D2 handoff

D2 carries the observed layout through Index and rejects changed layout evidence before any effect.
Its default adapters are removed from discovery, removal filtering, source identity and projection readiness.
Custom-root tests prove nested native Skill catalogues, private routes and unlisted loader membership without reactivation.

## Wave 3 D3 handoff

D3 adds internal requested roots, mapped Install/configuration and lease-time layout checks while preserving default adoption.
Install no longer calls C2 default payload overloads. Setup choices and IgnoreSection default adapters remain for Z2.
The overseer must integrate D1/D2 and complete both Install → Context → Index cases, currently failing at Context. No public activation or snapshot changes.
Release build passed. Unit 5268/5268/0/0. Integration 3782/3722/2/58, with both failures above and all skips allowlisted. Counts are total/passed/failed/skipped.

## Wave 4 E1 handoff

E1 shares one layout observation across Status, Doctor, route context, lifecycle comparisons and rename evidence. Ownership comparisons map package paths, and recovery stays fixed to the workspace and Task 86's recorded stores.
Its default-layout adapters are removed. Standalone workspace overloads observe the layout instead.
Valid settings keep a diagnostic layout even when other evidence is unsafe, and only invalid or unavailable settings make layout-dependent domains unavailable (chair amendment 5).

## Wave 4 E2 handoff

E2 makes implicit admission follow the content locations, keeps explicit grants literal, gives permission prompts projected context-folder facts and revalidates the layout before writing grants.
Extension caller prefilters are removed. Default permission carriers, Extension path-only admission and Library destination and consumer-boundary adapters remain for Wave 5.

## Overseer notes

- Every new settings file now declares schema v4, as decided, including default installs. The shipped README still says new settings use schema version 3. The final documentation packet corrects it, and C2's schema-sentence anchor then goes away.
- Before Z1 removes the permission adapters, resolve how maintenance treats an intentionally excluded missing loader against the frozen revalidator (from E2). The plan's `*LibrarySharedGrant*` filter selects nothing, because the shared-grant class has another name.
