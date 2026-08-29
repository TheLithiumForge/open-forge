---
open-forge:
  description: Implement retained route mutation commands on the accepted mutation foundation
  tags: [Memory, Working, CLI, Task, Route, Mutation, Contextual]
---

# Route Mutation Commands

## Task State

- State: Planned. No command behavior is Active or Ready.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Common prerequisites: Route discovery, `index`, and Mutation Foundation.
- Intended-membership formation prerequisite: accepted feature
  `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated at
  `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, exact tree
  `867be79ebee4ba60f2116a83857edadb8a5dbf0a`. It applies only to Route Init,
  Route Create, Route Move, and Route Remove. Route Update is independent of it.

## Shared Boundary

Each route command owns one concrete plan and result. Commands reuse accepted
route facts, validation, lock, lifecycle, atomic file, and external
recovery-bundle primitives. They do not share a universal route mutation request
or plan.

Init, Create, Move, and Remove consume the accepted
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
overload to project their complete post-operation Generated Navigation graph from
real observed evidence. The overload extends existing formation only. It does not
create a prospective catalogue/source framework, virtual filesystem, temporary
checkout, or hidden Index. Existing `Build(SourceCatalogue)` remains unchanged
current-state behavior and delegates to the overload.

Implement in order because later commands consume identity, collision, overwrite,
and lifecycle facts established by earlier operations. Every command proves dry
run where contracted, unchanged bytes on blocked paths, revalidation after lock,
idempotence where promised, one verified external recovery bundle covering every
existing Replace/Delete prepared before the first target effect, retained partial state
without restoration, process streams, and Native AOT.

Route Init remains Blocked until the maintainer accepts generic exact-chain
behavior or a Framework mode. Do not infer product acceptance from the completed
shared prerequisite. Route Move and Route Remove also retain separate future
reference and lifecycle proportionality gates. Resolve those command-local gates
before their behavior lanes begin; the shared formation does not make either
command Active or Ready.

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
