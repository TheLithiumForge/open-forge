---
open-forge:
  description: Route the export contracts that write the project as uninstall would leave it to a new folder
  responsibility: Route export's public Interface and Behavior without redefining the shared plain-docs plan
  tags: [Memory, Crystallized, CLI, Command, Contract, Export, CurrentTruth]
---

# Export Command Contract Set

`export` writes a copy of the project as it would look after `uninstall` to a new or empty folder. The [plain-docs contracts](../shared/plain-docs/_plain-docs.md) define the plan and the selection flags it shares with [`uninstall`](../uninstall/_uninstall.md).

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Plan export with the shared plain-docs plan, check the destination, then write every copied file without touching the project](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Export #Behavior #CurrentTruth
- [Write a copy of the project as it would look after uninstall to a new folder, optionally stripped and flattened, and leave the project unchanged](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Export #Interface #CurrentTruth
