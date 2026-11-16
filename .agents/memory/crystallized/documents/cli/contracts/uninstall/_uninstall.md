---
open-forge:
  description: Route the uninstall contracts that take Open Forge back out of a workspace while keeping the files the user wrote
  responsibility: Route uninstall's public Interface and Behavior without redefining the strip parts
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Lifecycle, Mutation, CurrentTruth]
---

# Uninstall Command Contract Set

`uninstall` removes the Framework and Extension files Open Forge installed and still recognizes, its managed sections and its state. Files the user wrote stay. The [strip contracts](../strip/_strip.md) define the optional `--strip` parts and `--flatten`.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Plan uninstall from the current ownership and payload comparison, then apply it as one verified strip plan](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Uninstall #Behavior #Lifecycle #CurrentTruth
- [Take Open Forge back out of a workspace, keep the files the user wrote and optionally strip what remains](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Uninstall #Interface #Lifecycle #CurrentTruth
