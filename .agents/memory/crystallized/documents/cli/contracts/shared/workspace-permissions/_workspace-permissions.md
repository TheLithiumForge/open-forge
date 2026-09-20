---
open-forge:
  description: Route the accepted shared authored destination permission contracts
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, CurrentTruth]
---

# Workspace Permission Contracts

These contracts define shared authored destination grants and the common approval
and persistence boundary. A command explicitly opts into this capability and
owns its required grants, findings, results and effects. Permission is separate
from content ownership. The replacement CLI does not ship yet.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Define shared path admission, explicit approval and recovery-preserving settings publication](behavior.md) - #Memory #Crystallized #CLI #Contract #Shared #Permission #Behavior #CurrentTruth
- [Define shared authored destination admission and truthful permission result coordinates](interface.md) - #Memory #Crystallized #CLI #Contract #Shared #Permission #Interface #CurrentTruth
