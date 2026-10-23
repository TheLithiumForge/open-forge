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
