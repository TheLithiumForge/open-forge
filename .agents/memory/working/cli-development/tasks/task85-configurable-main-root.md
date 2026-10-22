---
open-forge:
  description: Open Task 85 to let a workspace keep its Open Forge knowledge in a folder other than .agents, chosen at install or moved later, deferred while route include covers reusing project documents
  tags: [Memory, Working, Task, CLI, Install, Settings, Layout, Contextual, Deferred]
---

# Task 85: Configurable main root

Status: Deferred by the maintainer on 2026-10-22. `route include` already lets a project use its documents where they live, so moving the main root waits.

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

The [implementation plan](../../../emerging/analysis/route-include-council/main-root-plan.md) has the full inventory, the layout type, the migration and the work packets.

- About 320 CLI source files assume `.agents`. Each assumption separates into the knowledge root, the control directory or the native Skills location.
- Wave 0 is one owner's foundation: the layout type, settings v4, ownership shapes and the contracts. Later waves convert source reading, navigation, route operations, Extensions, Libraries, the lifecycle commands and output, then add migration and the published-process journeys.
- The estimate is 22 to 25 engineering days, or 9 to 12 elapsed days with three implementation lanes.

## Before starting

- Re-check the plan against the code. It was written on 2026-10-21 before the merged review, so names and line numbers may have moved.
- Decide the plan's open questions with the maintainer, starting with whether a Library registration that straddles the moved folder blocks the move or gets a new projection mapping.
