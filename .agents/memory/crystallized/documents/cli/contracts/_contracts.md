---
open-forge:
  description: Accepted detailed command contracts for the non-shipping replacement Open Forge CLI after Gate 2 and Gate 3 closeout
  responsibility: Route current command behavior and implementation-boundary contracts without presenting unfinished commands as implemented
  tags: [Memory, Crystallized, CLI, Release, Command, Interface, Contract, CurrentTruth, Evergreen, Architecture]
---

# CLI Command Contracts

This route contains the detailed command contracts for the accepted replacement
CLI. Gate 2 command definition, Gate 3 Architecture, and current-source
reconciliation are complete. These contracts define accepted behavior and do
not duplicate current executable state. The replacement remains non-shipping.
All 28 retained commands are implemented and locally accepted. Complete
six-target delivery evidence, final acceptance, and release remain pending.
The active [CLI
Development](../../../../working/cli-development/_cli-development.md) route
records exact execution state.

Shared interfaces such as global flags live beside command contracts so every
command can link to one accepted definition.

## Shared Native Report

Every retained command uses the shared native report. `--format <text|json>`
selects text or one schema-3 JSON envelope; `--detail
<minimal|standard|full|debug>` selects the detail level; and repeatable
`--detail-filter <error|warning|info|all>` filters only the displayed severity
rows. The current result names are `completed`, `completed-with-warnings`,
`incomplete`, `invalid-input`, `blocked`, `failed`, and `cancelled`. All command
contracts use this report and do not define alternate legacy presentations or
envelope versions.

The grouped [`library`](library/_library.md) family has exactly five leaves:
`list`, `inspect`, `attach`, `sync`, and `detach`. Their local Interface and
Behavior pairs define operation meaning. The one shared-capability [Workspace
Libraries Technical Design](../technical-designs/workspace-libraries.md) defines
the common record, inventory, projection, and recovery realization.

The grouped [`route`](route/_route.md) family includes the accepted structural
`route move` and `route remove` leaves. Each accepts one eligible ordinary
unmanaged logical leaf or one complete ordinary unmanaged category and keeps
its Interface and Behavior contracts in the operation-local scope. These
commands are implemented and locally accepted; they do not ship yet.

The direct root [`cleanup`](cleanup/_cleanup.md) command is retained as an
operand-free operation over the current recognized external recovery-bundle and
draft catalogue. An empty catalogue is a no-lease no-op. Before any deletion,
Cleanup must acquire the existing same-workspace `WorkspaceLockLease` and repeat
final catalogue and expected-state validation under that lease. Its local
Interface and Behavior Contracts define the complete default-all deletion
boundary; this entrypoint does not duplicate that meaning.

The [CLI Architecture](../architecture.md) defines the accepted implementation
structure and cross-command boundaries. The [MVP Architecture](../mvp-architecture.md)
records the frozen `open-forge-old` implementation as historical reference. The
active development records track execution without replacing these contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Route the accepted non-shipping root cleanup contracts for lease-validated recovery-bundle and draft deletion](cleanup/_cleanup.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Cleanup #Mutation #Recovery #Safety #CurrentTruth
- [Current Crystallized contract set for stateless ordered context resolution, projection, and explicit link expansion](context/_context.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Context #CurrentTruth
- [Route the accepted current read-only Doctor Interface and Behavior contracts](doctor/_doctor.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Doctor #CurrentTruth
- [Route the accepted non-shipping Extension list, inspect, create, install, update, and remove contracts](extension/_extension.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Lifecycle #CurrentTruth
- [Route the accepted current `find` contracts for deterministic source inventory and typed tag or heading discovery](find/_find.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Find #Discovery #Tag #Heading #CurrentTruth
- [Route the current Crystallized non-shipping `index` contracts for its public interface, deterministic behavior, and accepted technical design](index-candidate/_index-candidate.md) - #Memory #Crystallized #CLI #Release #Command #Index #Contract #CurrentTruth
- [Route the accepted non-shipping root Framework management-establishment and exact-no-op contracts for `install`](install/_install.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Install #Framework #Lifecycle #CurrentTruth
- [Route the accepted current Workspace Library list, inspect, attach, sync, and detach contracts](library/_library.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Workspace #CurrentTruth
- [Routing-only entrypoint for the accepted current `references` command contracts](references-candidate/_references-candidate.md) - #Memory #Crystallized #CLI #Release #Command #Contract #References #CurrentTruth
- [Route the accepted current Repair Interface and Behavior contracts for exact and guided local repair](repair/_repair.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Repair #CurrentTruth
- [Routing-only group entrypoint for the `route` command family](route/_route.md) - #Memory #Crystallized #CLI #Release #Command #Route #Contract #CurrentTruth
- [Permanent route for accepted shared CLI contract sets](shared/_shared.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #CurrentTruth
- [Current Crystallized contract set for workspace status, context-size comparison, root customization, managed Extensions, and recognized recovery bundles](status/_status.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Status #CurrentTruth
- [Route the accepted non-shipping root Framework update contracts for trusted managed reconciliation](update/_update.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Update #Framework #Lifecycle #CurrentTruth
