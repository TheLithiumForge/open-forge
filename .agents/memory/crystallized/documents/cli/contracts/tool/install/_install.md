---
open-forge:
  description: Route the tool install interface and behavior contracts
  tags: [Memory, Document, CLI, Contract, Tool, Install, CurrentTruth]
---

# tool install
`tool install` is a write command that verifies and registers one supported external program pin.

Installation leaves workspace roles unchanged. The interface defines consent and output. The behavior defines verification and publication.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define staged tool verification, registration and offline reuse without role selection](behavior.md) - #Memory #Document #CLI #Contract #Tool #Install #Behavior #CurrentTruth
- [Define tool install inputs, artifact consent and inactive registration](interface.md) - #Memory #Document #CLI #Contract #Tool #Install #Interface #CurrentTruth
