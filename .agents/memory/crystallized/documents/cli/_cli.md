---
open-forge:
  description: Current replacement CLI architecture, contracts, technical designs, and distribution authority, kept separate from the frozen MVP and its historical evidence
  responsibility: Route the accepted non-shipping replacement CLI sources and the frozen open-forge-old reference without merging their authority
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Contract, TechnicalDesign, Distribution, MVP, Legacy]
---

# CLI

The replacement command contracts and top-down C# Architecture are current.
All 28 retained commands are implemented and locally accepted under `src/cli/`.
The CLI remains unreleased while complete six-target delivery evidence and final
acceptance are pending. The active [CLI
Development](../../../working/cli-development/_cli-development.md) route records
exact implementation and evidence state.

The frozen TypeScript MVP remains historical reference under `src/cli-mvp/`.
Replacement work uses the executable built in the same worktree and does not
change, build, or test the frozen MVP.

## Authority Map

| Question                                                                | Source                                                                                  |
| ----------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| What is the accepted greenfield structure and implementation model?     | [Replacement CLI Architecture](architecture.md)                                         |
| Where are replacement command-contract roles, topology, and boundaries? | [Command Contract Set](command-contract-set.md)                                         |
| What conventions cross replacement commands?                            | [Shared CLI Operation Contract](shared-operation-contract.md)                           |
| What are the exact shared result, status, exit, and stream coordinates? | [Shared Result Coordinates](contracts/shared/result-coordinates/_result-coordinates.md) |
| Where are the detailed replacement command contracts?                   | [Detailed Command Contracts](contracts/_contracts.md)                                   |
| Where are exact shared-capability realization designs?                  | [CLI Technical Designs](technical-designs/_technical-designs.md)                        |
| What is the accepted package graph and platform horizon?                | [CLI Distribution](distribution.md)                                                     |
| What does the frozen legacy implementation do?                          | [MVP Architecture](mvp-architecture.md)                                                 |
| Where are active implementation sequence and state?                     | [CLI Development](../../../working/cli-development/_cli-development.md)                 |
| What did deleted CLI v2 preserve as history?                            | [CLI v2 Archive](../../../archived/cli-v2/_cli-v2.md)                                   |

The removed CLI release program is historical evidence in Git and the archived
reset record. The CLI-v2 archive is raw historical input only. Validate and
discuss an old idea before carrying it into the replacement.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->

- [Current cross-cutting structure and invariants for the greenfield C# replacement CLI](architecture.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Architecture #Greenfield #DotNet #NativeAOT #Testing #Release
- [Accepted current replacement CLI command-contract roles, topology, and authority boundaries](command-contract-set.md) - #Memory #Crystallized #CLI #Release #Document #Evergreen #CurrentTruth #Contract #Set #Interface #Behavior #TechnicalDesign #Routing #Locality
- [Accepted detailed command contracts for the non-shipping replacement Open Forge CLI after Gate 2 and Gate 3 closeout](contracts/_contracts.md) - #Memory #Crystallized #CLI #Release #Command #Interface #Contract #CurrentTruth #Evergreen #Architecture
- [Accepted public package graph, x64 and ARM64 platform horizon, staging, packing, checksum, proof, and publication boundary for the replacement CLI](distribution.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #Distribution #Npm #NativeAOT #Release
- [Historical frozen Open Forge CLI MVP role, command surface, deterministic state, safety model, verification boundary, proven properties, and liabilities](mvp-architecture.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #Architecture #CLI #MVP #Tooling #Legacy
- [Accepted current shared operation contract for the non-shipping Open Forge CLI](shared-operation-contract.md) - #Memory #Crystallized #CLI #Release #Document #Evergreen #CurrentTruth #Contract #Operation #Shared #Interface #Behavior #Determinism #Output #Safety #Locality
- [Concrete designs for shared replacement CLI capabilities whose exact realization does not belong in system Architecture](technical-designs/_technical-designs.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #CLI #TechnicalDesign #Implementation

<!-- open-forge:generated-index:end -->
