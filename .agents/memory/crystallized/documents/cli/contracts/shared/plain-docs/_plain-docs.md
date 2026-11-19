---
open-forge:
  description: Route the plain-docs plan that uninstall and export share, with its selection flags, strip parts and flatten
  responsibility: Route the shared plain-docs Interface and Behavior without adding either command's output policy
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Uninstall, Export, Shared, CurrentTruth]
---

# Plain-Docs Plan Contract Set

[`uninstall`](../../uninstall/_uninstall.md) and [`export`](../../export/_export.md) share one plan: uninstall's selection, then the strip parts, then flatten. These contracts define the selection flags, the parts and flatten once for both commands.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Form one deterministic plain-docs plan from uninstall's selection, the strip parts and flatten before either command writes](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Strip #Uninstall #Export #Behavior #CurrentTruth
- [Define the selection flags, strip parts, flatten rules and plan results that uninstall and export share](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Strip #Uninstall #Export #Interface #CurrentTruth
