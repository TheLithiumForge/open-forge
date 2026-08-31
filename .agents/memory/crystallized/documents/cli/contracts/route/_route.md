---
open-forge:
  description: Routing-only group entrypoint for the `route` command family
  responsibility: Route group help and child command scopes without defining command contracts
  tags: [Memory, Crystallized, CLI, Release, Command, Route, Contract, CurrentTruth]
---

# route Command Group

## Status And Authority

This is the accepted current Crystallized routing-only entrypoint for the
`route` command family. The new CLI does not ship yet. Route Init has a local
implementation and complete executable proof in exact feature candidate
`cb62b19`; the remaining mutation leaves and replacement-CLI delivery remain
unfinished. This entrypoint defines group routing and help only. The child
Interface and Behavior Contracts define detailed command meaning. No Route
Technical Design is needed.

## Routing And Help

The `route` group performs no domain operation. Group help exposes the leaf
operations `inspect`, `list`, `init`, `create`, `update`, `move`, and `remove`.
Each child entrypoint routes to its Interface and Behavior Contracts. The
structural `move` and `remove` leaves accept one eligible ordinary unmanaged
logical leaf or one complete ordinary unmanaged category; their local contracts
define the exact boundaries. Generated `Entries` provide navigation only.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Routing-only entrypoint for the accepted `route create` command contracts](create/_create.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Create #Template #Mutation #CurrentTruth
- [Routing-only entrypoint for the accepted `route init` command contracts](init/_init.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Init #Entrypoint #CurrentTruth
- [Routing-only entrypoint for the accepted `route inspect` command contracts](inspect/_inspect.md) - #Memory #Crystallized #CLI #Release #Command #Route #Inspect #Contract #CurrentTruth
- [Routing-only entrypoint for the accepted current `route list` command contracts](list/_list.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #List #CurrentTruth
- [Routing-only entrypoint for the accepted current `route move` contract](move/_move.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Move #Mutation #Reference #CurrentTruth
- [Routing-only entrypoint for the accepted current `route remove` contract](remove/_remove.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Remove #Mutation #Reference #CurrentTruth
- [Routing-only entrypoint for the accepted `route update` command contracts](update/_update.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Update #Metadata #Template #Mutation #CurrentTruth
<!-- open-forge:generated-index:end -->
