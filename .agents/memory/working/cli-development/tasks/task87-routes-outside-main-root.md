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

A1 is integrated and ready for its first commit. It adds the pure layout API, shared portable validation, settings v5 and the token projection owner check. Primary mapping and implicit permissions remain unchanged. Finding A1-R1 is fixed by changing only the default fixture's schema URL and version bytes from 4 to 5. Its other 21 file values, text, JSON and line endings are unchanged.

Managed Windows x64 evidence used SDK 10.0.101 and the plan's `BuildInSandbox` and `tu`/`ti`/`te` helpers. Unit selection: `WorkspaceLayoutTests`, `WorkspaceOutsideRouteTests`, `WorkspaceSettingsOutsideRoutesTests`, `WorkspaceSettingsCodecTests`, `WorkspaceSettingsRootsTests`, `WorkspaceSettingsMainRootTests`, `WorkspaceSettingsSchemaTests`, `WorkspaceSettingsWriteTests`, `PublishedTokensTests`, `SourceFormClassifierTests`. Integration selection: `WorkspaceSettingsSchemaIntegrationTests`, `WorkspaceSettingsReaderIntegrationTests`, `LayerBoundaryTests`, `CliReportInvariantsTests`, `Schema3CommandIntegrationTests`. Published selection: `PublishedMainRootDefaultCompatibilityProcessTests` and `PublishedMainRootInstallProcessTests`. Final results were 266 Unit, 62 Integration and 4 EndToEnd passes, with one existing declared Unix-permission skip. Release build and both format gates passed with no warnings or formatting changes. The coordinator independently built and passed 151 selected checks with that same skip. Schemas v1 through v4 are unchanged. Complete managed and Native AOT qualification remains in hosted CI.

Next is A2 physical observation on the committed API. Context used the normal file fallback because the installed launcher was unavailable. External Git index writes were denied, so integration used checked complete patches without staging. The guardian must prepare a worker's new base when a reset needs that index. No command integration or outside-route published journey is claimed yet.
