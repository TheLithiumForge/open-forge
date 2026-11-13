---
open-forge:
  description: Route the pinned tool execution contract
  tags: [Memory, Document, CLI, Contract, Tool, Exec, CurrentTruth]
---

# tool exec

`tool exec` is the command that runs the current workspace's pinned tool through its store launcher.

Agent entries and shell queries use the same version without PATH changes. The interface defines workspace selection, argument forwarding and silent success.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Define fast transparent execution of the current workspace tool pin](interface.md) - #Memory #Document #CLI #Contract #Tool #Exec #Interface #CurrentTruth
