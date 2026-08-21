---
open-forge:
  description: Hierarchical implementation Tasks for the complete greenfield replacement CLI
  tags: [Memory, Working, Contextual, Active, CLI, Task, Architecture, Development]
---

# CLI Development Tasks

Read the [program Task](00-cli-development.md), [Architecture](../../../crystallized/documents/cli/architecture.md), and [Plan](../plan.md) before selecting a Task group.

## Task Groups

- [x] [CLI Foundation](foundation/_foundation.md) — Complete — Implementer: Mastermind
- [ ] [Route Discovery](route-discovery/_route-discovery.md) — Active — Implementer: Mastermind
- [ ] [Read-Only Commands](read-only/_read-only.md) — Planned — Implementer: Not assigned
- [ ] [Mutation Foundation](mutation-foundation/_mutation-foundation.md) — Planned — Implementer: Not assigned
- [ ] [Route Mutation Commands](route-mutation/_route-mutation.md) — Planned — Implementer: Not assigned
- [ ] [Lifecycle Commands](lifecycle/_lifecycle.md) — Planned — Implementer: Not assigned
- [ ] [Operational Commands](operations/_operations.md) — Planned — Implementer: Not assigned
- [ ] [CLI Delivery](delivery/_delivery.md) — Planned — Implementer: Not assigned

## Axioms

- Read every parent Task before a child Task. Child scope inherits all parent
  constraints and may only narrow them.
- The Mastermind owns architecture, cross-cutting contracts, Task state,
  integration, and acceptance.
- A leaf Task is delegation-ready only when its decisions, predecessor outputs,
  class or algorithm model, allowed paths, tests, verification, and stop
  conditions are closed.
- An implementer changes only the leaf Task's allowed paths and returns evidence.
  It does not update Tasks, commit, promote shared code, or resolve architecture.
- A reviewer receives the accepted parent chain, exact baseline and changed paths,
  claimed evidence, and one named review horizon.
- Stop and return to the parent when implementation exposes a public-contract,
  architecture, dependency, platform, lifecycle, safety, or release choice.

## Task State Vocabulary

- `Planned`: Meaning exists, but a predecessor or design closure still blocks it.
- `Ready`: Every prerequisite and decision required to start is closed.
- `Active`: This is the selected implementation or integration Task.
- `Blocked`: A named unmet condition prevents progress.
- `Complete`: Acceptance evidence and integration are committed.
- `Cancelled`: The outcome is no longer required.

## Entries

<!-- open-forge:generated-index:start -->

- [Parent outcome, scope, authority, and acceptance for the complete replacement CLI program](00-cli-development.md) - #Memory #Working #CLI #Task #Program #Architecture #Development #Contextual #Active
- [Build and accept the actual command-free C# workspace, Core, host, safety, tests, and Native AOT foundation](foundation/_foundation.md) - #Memory #Working #CLI #Task #Foundation #Architecture #DotNet #NativeAOT #Contextual #Complete
- [Implement read-only route discovery, beginning with route list and then route inspect](route-discovery/_route-discovery.md) - #Memory #Working #CLI #Task #Route #Discovery #ReadOnly #Contextual #Active
- [Implement retained read-only source, context, extension, and generated-navigation commands](read-only/_read-only.md) - #Memory #Working #CLI #Task #ReadOnly #Source #Extension #Index #Contextual
- [Build and accept locking, lifecycle, planning, application, recovery, and Git foundations before mutations](mutation-foundation/_mutation-foundation.md) - #Memory #Working #CLI #Task #Mutation #Lifecycle #Recovery #Git #Contextual
- [Implement retained route mutation commands on the accepted mutation foundation](route-mutation/_route-mutation.md) - #Memory #Working #CLI #Task #Route #Mutation #Contextual
- [Implement root and Extension creation, installation, update, and removal lifecycle commands](lifecycle/_lifecycle.md) - #Memory #Working #CLI #Task #Lifecycle #Extension #Install #Update #Contextual
- [Implement aggregate status, diagnosis, repair, and cleanup after every state producer exists](operations/_operations.md) - #Memory #Working #CLI #Task #Status #Doctor #Repair #Cleanup #Contextual
- [Package, prove, document, and release the complete native CLI without partial publication](delivery/_delivery.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #SupplyChain #Release #Contextual

<!-- open-forge:generated-index:end -->
