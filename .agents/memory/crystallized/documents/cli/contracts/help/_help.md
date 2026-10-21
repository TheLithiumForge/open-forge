---
open-forge:
  description: Read built-in Open Forge topics without a workspace
  responsibility: Route the Help Interface and Behavior contracts
  tags: [Memory, Crystallized, CLI, Command, Contract, Help, CurrentTruth]
---

# Help Command Contract Set

## Status And Authority

This set defines `open-forge help [<topic>]`. Its only topic is `concepts`.
The [Interface](interface.md) defines the public command and output.
The [Behavior](behavior.md) defines topic selection, content assembly and read-only effects.
The [CLI Architecture](../../architecture.md) defines implementation boundaries.
The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
define the result envelope, status, exit and stream rules.

Help has no separate Technical Design.

## Axioms

- inherited - No local axioms, loaded ancestor axioms remain active.

## Entries

- [Resolve built-in topics and append the shipped guide's file formats without workspace reads](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Help #Behavior #CurrentTruth #Evergreen
- [List help topics and print concepts in text or JSON](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Help #Interface #CurrentTruth #Evergreen
