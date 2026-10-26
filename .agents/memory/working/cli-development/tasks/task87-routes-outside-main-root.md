---
open-forge:
  description: Open Task 87 to let a folder anywhere in the repository, outside the main root, be included as a full route that every command maintains, following Task 85 Delivery 1
  tags: [Memory, Working, Task, CLI, Routing, Include, Settings, Contextual]
---

# Task 87: Routes outside the main root

Status: Open, selected by the maintainer on 2026-10-24. It starts after [Task 85](task85-configurable-main-root.md) Delivery 1 and builds on its layout carriers.

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
