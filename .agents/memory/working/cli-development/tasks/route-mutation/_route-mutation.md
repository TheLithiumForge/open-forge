---
open-forge:
  description: Implement retained route mutation commands on the accepted mutation foundation
  tags: [Memory, Working, CLI, Task, Route, Mutation, Contextual]
---

# Route Mutation Commands

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Route discovery, `index`, and Mutation Foundation.

## Shared Boundary

Each route command owns one concrete plan and result. Commands reuse accepted
route facts, validation, lock, lifecycle, atomic file, and external
recovery-bundle primitives. They do not share a universal route mutation request
or plan.

Implement in order because later commands consume identity, collision, overwrite,
and lifecycle facts established by earlier operations. Every command proves dry
run where contracted, unchanged bytes on blocked paths, revalidation after lock,
idempotence where promised, one verified external recovery bundle covering every
existing Replace/Delete prepared before the first target effect, retained partial state
without restoration, process streams, and Native AOT.

## Child Tasks

- [ ] [Implement exact-chain route initialization and resolve the deferred Framework-shape decision before behavior starts](route-init.md) — Blocked — Implementer: Not assigned
- [ ] [Implement one-file route creation below an existing routable parent](route-create.md) — Planned — Implementer: Not assigned
- [ ] [Implement bounded route content and metadata update without identity drift](route-update.md) — Planned — Implementer: Not assigned
- [ ] [Implement route move with reference, overwrite, generated-navigation, and recovery integrity](route-move.md) — Planned — Implementer: Not assigned
- [ ] [Implement route removal with dependency, reference, generated-navigation, and recovery integrity](route-remove.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->
- [Implement one-file route creation below an existing routable parent](route-create.md) - #Memory #Working #CLI #Task #Route #Create #Mutation #Contextual
- [Implement exact-chain route initialization and resolve the deferred Framework-shape decision before behavior starts](route-init.md) - #Memory #Working #CLI #Task #Route #Init #Mutation #Contextual
- [Implement route move with reference, overwrite, generated-navigation, and recovery integrity](route-move.md) - #Memory #Working #CLI #Task #Route #Move #Mutation #Contextual
- [Implement route removal with dependency, reference, generated-navigation, and recovery integrity](route-remove.md) - #Memory #Working #CLI #Task #Route #Remove #Mutation #Contextual
- [Implement bounded route content and metadata update without identity drift](route-update.md) - #Memory #Working #CLI #Task #Route #Update #Mutation #Contextual
<!-- open-forge:generated-index:end -->
