---
open-forge:
  description: Route the uninstall contracts that take Open Forge back out of a workspace while keeping the files the user wrote
  responsibility: Route uninstall's public Interface and Behavior without redefining the shared plain-docs plan
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Lifecycle, Mutation, CurrentTruth]
---

# Uninstall Command Contract Set

`uninstall` removes the Framework and Extension files Open Forge installed and still recognizes, its managed sections and its state. Files the user wrote stay. The [plain-docs contracts](../shared/plain-docs/_plain-docs.md) define the selection flags and the `--strip` parts it shares with [`export`](../export/_export.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Plan uninstall from the current ownership and payload comparison, then apply it as one verified strip plan](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Uninstall #Behavior #Lifecycle #CurrentTruth
- [Take Open Forge back out of a workspace, keep the files the user wrote and optionally strip what remains](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Uninstall #Interface #Lifecycle #CurrentTruth
