---
open-forge:
  description: Plan export with the shared plain-docs plan, check the destination, then write every copied file without touching the project
  responsibility: Define how export forms its copy, checks its destination and writes files, and what a failed write leaves
  tags: [Memory, Crystallized, CLI, Command, Contract, Export, Behavior, CurrentTruth]
---

# export Behavior Contract

The [Interface Contract](interface.md) defines the destination and what the copy holds. The [plain-docs Behavior](../shared/plain-docs/behavior.md) defines the plan.

## Plan

Check the destination before anything else, against the project's full path and the destination's resolved full path, comparing without regard to case on Windows. Then form the shared plan exactly as `uninstall` does. Read the project's file list once, leave out `.open-forge-state/`, the destination when it is inside the project, links and anything inside them, then match each listed file against the plan by path. Plan entries for files the list does not hold are not part of the export.

A preview stops here and writes nothing.

## Write

Create the destination when it is new. Write each copied file under its path in the copy, creating folders as needed, in ordinal path order: the plan's bytes for a rewrite or move, and the project file's current bytes for every other file. Export never takes the workspace lease, writes recovery data, or changes a byte in the project.

When a write fails or the caller interrupts, stop at once. The result is `export.write-failed` or `export.interrupted`, names the destination to delete and points at the same command as the next action. Export does not remove what it already wrote.

## Required Evidence

Cover a preview that writes nothing, a complete copy, uninstall's selection in the copy, each part through the copy, a destination inside the project, every refused destination, the file list without Git, and the eval harness's expected bytes through `export`. Verify the project's bytes are unchanged after every run. Git's file list is one shared helper with route inspect, whose Git evidence covers tracked, untracked and ignored files.
