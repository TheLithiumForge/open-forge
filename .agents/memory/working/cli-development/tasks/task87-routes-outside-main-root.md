---
open-forge:
  description: Open Task 87 to let a folder anywhere in the repository, outside the main root, be included as a full route that every command maintains, following Task 85 Delivery 1
  tags: [Memory, Working, Task, CLI, Routing, Include, Settings, Contextual]
---

# Task 87: Routes outside the main root

Status: Active, selected by the maintainer on 2026-10-24. Implementation starts from Task 85 Delivery 1 at `a7e25a992`. Its final hosted qualification proceeds separately.

## Outcome

A folder in the repository, outside the main root, can be included as a route. Every command maintains it and works inside it: Index keeps its `Entries`, and route create, init, update, move and remove work within its boundary. Including a single file stays the read-only include it is today.

## Accepted design

The [multiple roots council](../../../emerging/analysis/multiple-roots-council/record.md) and the maintainer decided it. In short:

- `route include <folder>` registers the folder in settings v5 `outsideRoutes` and lists its canonical catalogue, created only when missing. `route exclude <folder>` removes both and never deletes content.
- Content keeps exact `./path` identities, so outside routes never collide with the main root.
- An outside route cannot overlap the main root, `.agents/`, another outside route, Git metadata or `.open-forge-state/`.
- Registration grants bounded maintenance, not package ownership.

## Plan

The [Task 87 plan](../../../emerging/analysis/multiple-roots-council/plan.md) has the frozen API, the inventory merge that keeps included rows, the command table, seven waves of packets and the evidence rules.

## Reconciliation with Task 85

Checked against `a7e25a992`. Task 85 delivered primary layout mapping, fixed Skills and controls, operation-wide observations and lease revalidation, literal included identities, ordinal implicit admission and the tracked public-token projection. Packet B's additive catalogue constructor, ordered `IncludedRows`, independent row retention, physical alias merge, traversal tracking and first-reaching metadata projection already exist. Generated navigation reconstruction retains layout, aliases and rows.

Remaining work is outside-route descriptors and maintenance boundaries, inventory roots, settings v5, directory observations and registration-aware classification. Packets C through I still need bounded maintenance, folder Include and Exclude, command integration, reporting, published journeys and documentation. Registration supplies neither loading membership nor implicit package permission.

The seven waves remain, with smaller sequential packets in chair amendment 1. Foundation and inventory precede maintenance, then command packets, reporting and lifecycle integration, published qualification and documentation. No main-root migration or Task 88 work is included.

## Execution capsule

This local Markdown tool changes user files, settings and navigation. Existing expected-state checks, workspace leases and recovery evidence cover ordinary errors, stale plans and interruption. Git does not recover uncommitted content by itself. The accepted same-user threat boundary stays unchanged.

Reuse Task 85 layout transport, the BCL filesystem, typed document parsers and existing mutation primitives. Exceptional machinery: none. Pure layout and codecs use Unit evidence, physical observation and effects use Integration, and three published journeys prove composition. Run affected classes locally. Complete managed and Native AOT qualification runs in hosted CI under the coordinator brief.

## Progress

A1 was committed and pushed as `ab410f996`. [Draft PR #10](https://github.com/TheLithiumForge/open-forge/pull/10) is stacked on Task 85, whose head remains `a7e25a992`. A1 adds the pure layout API, shared portable validation, settings v5 and the token projection owner check. Primary mapping and implicit permissions remain unchanged. Finding A1-R1 is fixed by changing only the default fixture's schema URL and version bytes from 4 to 5. Its other 21 file values, text, JSON and line endings are unchanged.

Managed Windows x64 evidence used SDK 10.0.101 and the plan's `BuildInSandbox` and `tu`/`ti`/`te` helpers. Unit selection: `WorkspaceLayoutTests`, `WorkspaceOutsideRouteTests`, `WorkspaceSettingsOutsideRoutesTests`, `WorkspaceSettingsCodecTests`, `WorkspaceSettingsRootsTests`, `WorkspaceSettingsMainRootTests`, `WorkspaceSettingsSchemaTests`, `WorkspaceSettingsWriteTests`, `PublishedTokensTests`, `SourceFormClassifierTests`. Integration selection: `WorkspaceSettingsSchemaIntegrationTests`, `WorkspaceSettingsReaderIntegrationTests`, `LayerBoundaryTests`, `CliReportInvariantsTests`, `Schema3CommandIntegrationTests`. Published selection: `PublishedMainRootDefaultCompatibilityProcessTests` and `PublishedMainRootInstallProcessTests`. Final results were 266 Unit, 62 Integration and 4 EndToEnd passes, with one existing declared Unix-permission skip. Release build and both format gates passed with no warnings or formatting changes. The coordinator independently built and passed 151 selected checks with that same skip. Schemas v1 through v4 are unchanged. Complete managed and Native AOT qualification remains in hosted CI.

A2 is integrated on `ab410f996`. It observes outside directories, ancestry and canonical catalogues from the retained settings snapshot, keeps outside diagnostics separate from primary readiness, and revalidates safe expectations and directory accessibility under the lease. Physical overlaps are checked before catalogue reads. Existing primary mappings, permissions, source inventory, command code, wire shapes and tokens are unchanged.

The coordinator rebuilt the integrated candidate in Release on managed Windows x64 with SDK 10.0.101. Using the plan's sandbox build block and class-filter helpers, Unit selection was `WorkspaceLayoutReadTests`, `WorkspaceLayoutReaderTests`, `WorkspaceOutsideRouteTests` and `WorkspaceSettingsOutsideRoutesTests`. Integration selection was `WorkspaceLayoutReaderIntegrationTests`, `WorkspaceOutsideRouteReaderIntegrationTests`, `WorkspaceLayoutObservationIntegrationTests`, `WorkspaceLayoutTargetIntegrationTests`, `WorkspaceLayoutPermissionIntegrationTests`, `LayerBoundaryTests` and `CliReportInvariantsTests`. Published selection was `PublishedMainRootInstallProcessTests` and `PublishedMainRootDefaultCompatibilityProcessTests`. All 232 discovered cases were accounted for: 109 Unit, 116 Integration and 4 EndToEnd passes, with three declared Unix-directory-permission skips. Build warnings and failures were zero. Both final coordinator format commands passed without source changes.

Review finding A2-R1, P2 evidence design, excluded the new `WorkspaceOutsideRouteReadTests.RetainsFacts` file. Its four cases only repeated values assigned to automatic record properties. The real reader Integration cases retain the capture evidence. The corrected selection was built and executed independently, and all eleven integrated C# files match the formatted implementation. Next is the B1/B2 inventory wave. The worktree-built CLI loads context, the external Git index remains unwritable, and PR CI is unobserved after the available public-read transports failed. Full managed and Native AOT qualification remains in hosted CI. No outside command integration or outside published journey is claimed yet.
