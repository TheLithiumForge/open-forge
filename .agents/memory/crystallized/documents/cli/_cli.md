---
open-forge:
  description: Current replacement CLI Architecture and contracts, kept separate from the frozen MVP and its historical evidence
  responsibility: Route the accepted non-shipping replacement CLI sources and the frozen open-forge-old reference without merging their authority
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Contract, MVP, Legacy]
---

# CLI

This route keeps two CLI implementations distinct. The accepted C# replacement
is current Architecture and contract meaning, but it remains non-shipping. Its
active foundation source and local Native AOT outputs are evidence only; it has
no retained command or accepted shipping executable. The frozen TypeScript MVP remains
the only established executable CLI reference and is exposed as
`open-forge-old` for its existing repository routing assistance. Replacement
work must not change, build, or test the frozen MVP as if it were the new CLI.

## Authority Map

| Question                                                                | Source                                                              |
| ----------------------------------------------------------------------- | ------------------------------------------------------------------- |
| What is the accepted replacement structure and Gate 3 boundary?         | [Replacement CLI Architecture](architecture.md)                     |
| Where are replacement command-contract roles, topology, and boundaries? | [Command Contract Set](command-contract-set.md)                     |
| What conventions cross replacement commands?                            | [Shared CLI Operation Contract](shared-operation-contract.md)       |
| Where are the detailed replacement command contracts?                   | [Detailed Command Contracts](contracts/_contracts.md)               |
| What does the frozen legacy implementation do?                          | [MVP Architecture](mvp-architecture.md)                             |
| What is the contextual program and gate record?                         | [CLI Release Program](../../../working/cli-release/_cli-release.md) |
| What did deleted CLI v2 preserve as history?                            | [CLI v2 Archive](../../../archived/cli-v2/_cli-v2.md)               |

The release program is contextual execution history, not a replacement for the
current Architecture or contracts. The CLI-v2 archive is raw historical input
only. Validate and discuss an old idea before carrying it into the replacement.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->

- [Accepted Gate 3 architecture for the non-shipping C# replacement CLI, its evidence, and its release boundary](architecture.md) - #Memory #Crystallized #CLI #Architecture #Gate #CurrentTruth #Evergreen #DotNet #NativeAOT #Testing #Release
- [Accepted current replacement CLI command-contract roles, topology, and authority boundaries](command-contract-set.md) - #Memory #Crystallized #CLI #Release #Document #Evergreen #CurrentTruth #Contract #Set #Interface #Behavior #TechnicalDesign #Routing #Locality
- [Accepted detailed command contracts for the non-shipping replacement Open Forge CLI after Gate 2 and Gate 3 closeout](contracts/_contracts.md) - #Memory #Crystallized #CLI #Release #Command #Interface #Contract #CurrentTruth #Evergreen #Architecture
- [Historical frozen Open Forge CLI MVP role, command surface, deterministic state, safety model, verification boundary, proven properties, and liabilities](mvp-architecture.md) - #Memory #Crystallized #Document #CurrentTruth #Evergreen #Architecture #CLI #MVP #Tooling #Legacy
- [Accepted current shared operation contract for the non-shipping Open Forge CLI](shared-operation-contract.md) - #Memory #Crystallized #CLI #Release #Document #Evergreen #CurrentTruth #Contract #Operation #Shared #Interface #Behavior #Determinism #Output #Safety #Locality

<!-- open-forge:generated-index:end -->
