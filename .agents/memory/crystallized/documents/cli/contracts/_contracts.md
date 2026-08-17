---
open-forge:
  description: Accepted detailed command contracts for the non-shipping replacement Open Forge CLI after Gate 2 and Gate 3 closeout
  responsibility: Route current command behavior and implementation-boundary contracts without presenting unfinished commands as implemented
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Contract, CurrentTruth, Evergreen, Architecture]
---

# CLI Command Contracts

This route contains the detailed command contracts for the accepted replacement
CLI. Gate 2 command definition, Gate 3 Architecture, and current-source
reconciliation are complete. The contracts define intended behavior, not
current executable behavior. The replacement remains non-shipping while Gate 5
implementation and complete evidence are pending.

Shared interfaces such as global flags live beside command contracts so every
command can link to one accepted definition.

The grouped [`route`](route/_route.md) family includes the accepted structural
`route move` and `route remove` leaves. Each accepts one eligible ordinary
unmanaged logical leaf or one complete ordinary unmanaged category and keeps
its Interface and Behavior contracts in the operation-local scope. These
commands define intended behavior only; they do not ship yet.

The direct root [`cleanup`](cleanup/_cleanup.md) command is retained as an
operand-free operation over the current recognized Open Forge transient and
recovery-artifact catalogue. Its local Interface and Behavior Contracts define
the complete default-all deletion boundary; this entrypoint does not duplicate
that meaning.

The [CLI Architecture](../architecture.md) defines the accepted implementation
structure and cross-command boundaries. The [frozen legacy CLI documentation](../_cli.md)
and [MVP Architecture](../mvp-architecture.md) remain the source for the
executable currently available as `open-forge-old`. The contextual release
program records execution history but does not replace these current contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Route the accepted non-shipping root cleanup contracts for recognized Open Forge transient and recovery artifacts](cleanup/_cleanup.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Cleanup #Mutation #Recovery #Safety #CurrentTruth
- [Current Crystallized contract set for stateless ordered context resolution, projection, and explicit link expansion](context/_context.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Context #CurrentTruth
- [Route the accepted current read-only Doctor Interface and Behavior contracts](doctor/_doctor.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Doctor #CurrentTruth
- [Route the accepted non-shipping Extension list, inspect, create, install, update, and remove contracts](extension/_extension.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Lifecycle #CurrentTruth
- [Route the accepted current `find` contracts for deterministic source inventory and typed tag or heading discovery](find/_find.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Find #Discovery #Tag #Heading #CurrentTruth
- [Route the current Crystallized non-shipping `index` contracts for its public interface, deterministic behavior, and accepted technical design](index/_index.md) - #Memory #Crystallized #CLI #Release #Command #Index #Contract #CurrentTruth
- [Route the accepted non-shipping root Framework management-establishment and exact-no-op contracts for `install`](install/_install.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Install #Framework #Lifecycle #CurrentTruth
- [Routing-only entrypoint for the accepted current `references` command contracts](references/_references.md) - #Memory #Crystallized #CLI #Release #Command #Contract #References #CurrentTruth
- [Route the accepted current Repair Interface and Behavior contracts for exact and guided local repair](repair/_repair.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Repair #CurrentTruth
- [Routing-only group entrypoint for the `route` command family](route/_route.md) - #Memory #Crystallized #CLI #Release #Command #Route #Contract #CurrentTruth
- [Permanent route for accepted shared CLI contract sets](shared/_shared.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #CurrentTruth
- [Current Crystallized contract set for workspace status, context-size comparison, root customization, managed Extensions, and recovery evidence](status/_status.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Status #CurrentTruth
- [Route the accepted non-shipping root Framework update contracts for trusted managed reconciliation](update/_update.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Update #Framework #Lifecycle #CurrentTruth
<!-- open-forge:generated-index:end -->
