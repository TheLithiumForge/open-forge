---
open-forge:
  description: Active top-down architecture, Plan, Tasks, and evidence for the greenfield replacement CLI
  tags: [Memory, Working, Contextual, Active, KeepInMind, CLI, Architecture, Plan, Task, Development]
---

# CLI Development

This route contains the active execution state for the greenfield replacement
CLI. The current [CLI Architecture](../../crystallized/documents/cli/architecture.md)
and command contracts define accepted meaning. The records here define temporary
planning, Task state, sequencing, evidence, and resumption.

The Mastermind keeps the top-down architecture and integration model. Child Tasks
may be delegated only after their architecture, dependencies, callable contracts,
boundaries, evidence, and stop conditions are closed.

The preserved first implementation remains historical evidence in the
[reset record](../../archived/cli-release/implementation-reset-2026-08-21.md).
Do not restore its structure by default.

## Axioms

- Read the active Plan before selecting or changing a CLI Task.
- Read a parent Task before its child Tasks and preserve every inherited boundary.
- A child Task may narrow allowed work but may not broaden architecture, scope,
  dependencies, public behavior, or external effects.
- Only the Mastermind updates Plan and Task state, architecture, cross-cutting
  contracts, integration, and acceptance unless the maintainer explicitly assigns
  another responsible role.
- Stop implementation when a Task requires an unaccepted architecture or product
  choice. Record the decision frontier in the parent context before continuing.

## Entries

<!-- open-forge:generated-index:start -->
- [Active ledger of deferred replacement-CLI edge cases, owners, risks, and closure conditions](edge-cases.md) - #Memory #Working #CLI #EdgeCase #Evidence #Contextual #Active
- [Executable top-down work graph for completing the greenfield replacement CLI](plan.md) - #Memory #Working #CLI #Plan #Architecture #Development #Contextual #Active
- [Hierarchical implementation Tasks for the complete greenfield replacement CLI](tasks/_tasks.md) - #Memory #Working #Contextual #Active #CLI #Task #Architecture #Development
<!-- open-forge:generated-index:end -->
